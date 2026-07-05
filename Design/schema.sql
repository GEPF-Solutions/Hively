-- Hively database schema (PostgreSQL 16).
-- Source of truth for the domain model described in Design/README.md.
-- This is the design artifact: import it into drawdb.app (Import -> SQL) to view/edit
-- the ER diagram, then re-export here when the shape changes. EF Core entities in
-- Hively.Server/DbModel are generated to match this file, not the other way around.
--
-- Deliberately omitted (computed at read time, not stored):
--   Topic.compliant, Topic.mismatches — derived by validating last_payload against
--   the assigned schema's definition.
--
-- All foreign keys are declared as named table-level CONSTRAINT ... FOREIGN KEY
-- clauses (not inline column REFERENCES) — drawdb.app's SQL importer detects
-- relationships far more reliably in this form.

CREATE TABLE producers (
    id          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    name        text NOT NULL UNIQUE,
    description text
);

CREATE TABLE consumers (
    id          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    name        text NOT NULL UNIQUE,
    description text
);

-- Current version of each schema. Full edit history lives in schema_versions.
CREATE TABLE schemas (
    id         uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    name       text NOT NULL,
    definition jsonb NOT NULL,
    version    text NOT NULL
);

CREATE TABLE schema_versions (
    id         uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    schema_id  uuid NOT NULL,
    version    text NOT NULL,
    definition jsonb NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    UNIQUE (schema_id, version),
    CONSTRAINT fk_schema_versions_schema FOREIGN KEY (schema_id) REFERENCES schemas (id) ON DELETE CASCADE
);

-- Freely admin-defined colored pill. id is a slug, not a surrogate key.
CREATE TABLE tags (
    id    text PRIMARY KEY,
    label text NOT NULL,
    hue   integer
);

-- segments (used by the namespace drill-down sidebar) are derived from `path`
-- by splitting on '/' in the service layer, not stored redundantly — avoids
-- Postgres array-typed columns, which drawdb.app's importer handles poorly.
CREATE TABLE topics (
    id                   uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    path                 text NOT NULL UNIQUE,
    tracked              boolean NOT NULL DEFAULT false,
    producer_id          uuid,
    schema_id            uuid,
    last_payload         jsonb,
    last_seen_at         timestamptz,
    retained             boolean NOT NULL DEFAULT false,
    violation_count      integer NOT NULL DEFAULT 0,
    last_cleared_at      timestamptz,
    -- 24 hourly message-count buckets, e.g. [0,0,3,12,...] — jsonb instead of
    -- integer[] for the same importer-compatibility reason as above.
    activity_histogram   jsonb NOT NULL DEFAULT '[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]',
    -- Set when a relink is accepted: this topic's producer/schema/tags/violation_count
    -- were inherited from a relocated topic, and merged_into_topic_id below points at
    -- the surviving record instead of silently deleting the old one.
    retired_at           timestamptz,
    merged_into_topic_id uuid,
    CONSTRAINT fk_topics_producer FOREIGN KEY (producer_id) REFERENCES producers (id) ON DELETE SET NULL,
    CONSTRAINT fk_topics_schema FOREIGN KEY (schema_id) REFERENCES schemas (id) ON DELETE SET NULL,
    CONSTRAINT fk_topics_merged_into FOREIGN KEY (merged_into_topic_id) REFERENCES topics (id) ON DELETE SET NULL
);

CREATE INDEX idx_topics_tracked ON topics (tracked);

CREATE TABLE topic_consumers (
    topic_id    uuid NOT NULL,
    consumer_id uuid NOT NULL,
    PRIMARY KEY (topic_id, consumer_id),
    CONSTRAINT fk_topic_consumers_topic FOREIGN KEY (topic_id) REFERENCES topics (id) ON DELETE CASCADE,
    CONSTRAINT fk_topic_consumers_consumer FOREIGN KEY (consumer_id) REFERENCES consumers (id) ON DELETE CASCADE
);

CREATE TABLE topic_tags (
    topic_id uuid NOT NULL,
    tag_id   text NOT NULL,
    PRIMARY KEY (topic_id, tag_id),
    CONSTRAINT fk_topic_tags_topic FOREIGN KEY (topic_id) REFERENCES topics (id) ON DELETE CASCADE,
    CONSTRAINT fk_topic_tags_tag FOREIGN KEY (tag_id) REFERENCES tags (id) ON DELETE CASCADE
);

-- An MQTT-pattern match that can SET or EXCLUDE each of
-- producer/schema/tags/consumers, resolved automatically (the most-specific
-- applicable match wins per field, and per tag/consumer id independently for
-- the multi-valued fields) instead of forcing an admin to pick one whole
-- bundle when several match — replaces the earlier flat `rules`/`rule_tags`
-- design, which could only ever set a value, never exclude one, and had no
-- way to combine two single-purpose matches on the same topic. A branch
-- config from the namespace sidebar and an advanced wildcard pattern are both
-- just a `matches` row — the only difference is whether the admin authored
-- the pattern by drilling into the sidebar (fully-literal segments ending in
-- '#') or by typing wildcards. topic_id is non-null only for the single
-- private per-topic override a manual TopicDetail edit creates — its pattern
-- is that topic's own literal path, so it can only ever match that one topic.
CREATE TABLE matches (
    id                uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    name              text,
    pattern           text NOT NULL,
    topic_id          uuid,
    producer_id       uuid,
    exclude_producer  boolean NOT NULL DEFAULT false,
    schema_id         uuid,
    exclude_schema    boolean NOT NULL DEFAULT false,
    auto_apply        boolean NOT NULL DEFAULT false,
    updated_at        timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT fk_matches_topic FOREIGN KEY (topic_id) REFERENCES topics (id) ON DELETE CASCADE,
    CONSTRAINT fk_matches_producer FOREIGN KEY (producer_id) REFERENCES producers (id) ON DELETE SET NULL,
    CONSTRAINT fk_matches_schema FOREIGN KEY (schema_id) REFERENCES schemas (id) ON DELETE SET NULL,
    CONSTRAINT chk_matches_producer_action CHECK (NOT (producer_id IS NOT NULL AND exclude_producer)),
    CONSTRAINT chk_matches_schema_action CHECK (NOT (schema_id IS NOT NULL AND exclude_schema))
);

CREATE UNIQUE INDEX uq_matches_topic_id ON matches (topic_id) WHERE topic_id IS NOT NULL;
-- Not unique: two admin-authored matches may legitimately share a pattern
-- (e.g. one only sets a producer, another only sets a tag on the same filter).
CREATE INDEX idx_matches_pattern ON matches (pattern);

CREATE TABLE match_tag_actions (
    match_id   uuid NOT NULL,
    tag_id     text NOT NULL,
    is_exclude boolean NOT NULL DEFAULT false,
    PRIMARY KEY (match_id, tag_id),
    CONSTRAINT fk_match_tag_actions_match FOREIGN KEY (match_id) REFERENCES matches (id) ON DELETE CASCADE,
    CONSTRAINT fk_match_tag_actions_tag FOREIGN KEY (tag_id) REFERENCES tags (id) ON DELETE CASCADE
);

CREATE TABLE match_consumer_actions (
    match_id    uuid NOT NULL,
    consumer_id uuid NOT NULL,
    is_exclude  boolean NOT NULL DEFAULT false,
    PRIMARY KEY (match_id, consumer_id),
    CONSTRAINT fk_match_consumer_actions_match FOREIGN KEY (match_id) REFERENCES matches (id) ON DELETE CASCADE,
    CONSTRAINT fk_match_consumer_actions_consumer FOREIGN KEY (consumer_id) REFERENCES consumers (id) ON DELETE CASCADE
);

-- App-level account. One row per human, independent of which OIDC provider they
-- used to sign in — email is the link between multiple identities and one account.
-- Bootstrap: if this table is empty, the very first successful login is auto-
-- promoted to Admin (app logic, not enforced here); every subsequent signup
-- defaults to Viewer and is promoted by an existing Admin.
CREATE TABLE users (
    id         uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    email      text NOT NULL UNIQUE,
    role       text NOT NULL DEFAULT 'Viewer' CHECK (role IN ('Admin', 'Viewer')),
    created_at timestamptz NOT NULL DEFAULT now()
);

-- One row per (user, external login method). Login resolves here first by
-- (auth_provider, external_subject); if not found, falls back to matching by
-- email on `users` to link a new provider to an existing account, and only
-- creates a brand-new user when neither matches.
CREATE TABLE user_identities (
    id               uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id          uuid NOT NULL,
    auth_provider    text NOT NULL CHECK (auth_provider IN ('entra', 'google', 'basic')),
    external_subject text NOT NULL,
    created_at       timestamptz NOT NULL DEFAULT now(),
    UNIQUE (auth_provider, external_subject),
    CONSTRAINT fk_user_identities_user FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);
