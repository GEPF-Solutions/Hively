using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Hively.Server.DbModel;

public partial class HivelyContext : DbContext
{
    public HivelyContext(DbContextOptions<HivelyContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Consumer> Consumers { get; set; }

    public virtual DbSet<Producer> Producers { get; set; }

    public virtual DbSet<Rule> Rules { get; set; }

    public virtual DbSet<Schema> Schemas { get; set; }

    public virtual DbSet<SchemaVersion> SchemaVersions { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<Topic> Topics { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserIdentity> UserIdentities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Consumer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("consumers_pkey");

            entity.ToTable("consumers");

            entity.HasIndex(e => e.Name, "consumers_name_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<Producer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("producers_pkey");

            entity.ToTable("producers");

            entity.HasIndex(e => e.Name, "producers_name_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<Rule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("rules_pkey");

            entity.ToTable("rules");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AutoApply).HasColumnName("auto_apply");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Pattern).HasColumnName("pattern");
            entity.Property(e => e.ProducerId).HasColumnName("producer_id");
            entity.Property(e => e.SchemaId).HasColumnName("schema_id");

            entity.HasOne(d => d.Producer).WithMany(p => p.Rules)
                .HasForeignKey(d => d.ProducerId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_rules_producer");

            entity.HasOne(d => d.Schema).WithMany(p => p.Rules)
                .HasForeignKey(d => d.SchemaId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_rules_schema");

            entity.HasMany(d => d.Tags).WithMany(p => p.Rules)
                .UsingEntity<Dictionary<string, object>>(
                    "RuleTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .HasConstraintName("fk_rule_tags_tag"),
                    l => l.HasOne<Rule>().WithMany()
                        .HasForeignKey("RuleId")
                        .HasConstraintName("fk_rule_tags_rule"),
                    j =>
                    {
                        j.HasKey("RuleId", "TagId").HasName("rule_tags_pkey");
                        j.ToTable("rule_tags");
                        j.IndexerProperty<Guid>("RuleId").HasColumnName("rule_id");
                        j.IndexerProperty<string>("TagId").HasColumnName("tag_id");
                    });
        });

        modelBuilder.Entity<Schema>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("schemas_pkey");

            entity.ToTable("schemas");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Definition)
                .HasColumnType("jsonb")
                .HasColumnName("definition");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Version).HasColumnName("version");
        });

        modelBuilder.Entity<SchemaVersion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("schema_versions_pkey");

            entity.ToTable("schema_versions");

            entity.HasIndex(e => new { e.SchemaId, e.Version }, "schema_versions_schema_id_version_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Definition)
                .HasColumnType("jsonb")
                .HasColumnName("definition");
            entity.Property(e => e.SchemaId).HasColumnName("schema_id");
            entity.Property(e => e.Version).HasColumnName("version");

            entity.HasOne(d => d.Schema).WithMany(p => p.SchemaVersions)
                .HasForeignKey(d => d.SchemaId)
                .HasConstraintName("fk_schema_versions_schema");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tags_pkey");

            entity.ToTable("tags");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Hue).HasColumnName("hue");
            entity.Property(e => e.Label).HasColumnName("label");
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("topics_pkey");

            entity.ToTable("topics");

            entity.HasIndex(e => e.Tracked, "idx_topics_tracked");

            entity.HasIndex(e => e.Path, "topics_path_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ActivityHistogram)
                .HasDefaultValueSql("'[0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("activity_histogram");
            entity.Property(e => e.LastClearedAt).HasColumnName("last_cleared_at");
            entity.Property(e => e.LastPayload)
                .HasColumnType("jsonb")
                .HasColumnName("last_payload");
            entity.Property(e => e.LastSeenAt).HasColumnName("last_seen_at");
            entity.Property(e => e.MergedIntoTopicId).HasColumnName("merged_into_topic_id");
            entity.Property(e => e.Path).HasColumnName("path");
            entity.Property(e => e.ProducerId).HasColumnName("producer_id");
            entity.Property(e => e.Retained).HasColumnName("retained");
            entity.Property(e => e.RetiredAt).HasColumnName("retired_at");
            entity.Property(e => e.SchemaId).HasColumnName("schema_id");
            entity.Property(e => e.Tracked).HasColumnName("tracked");
            entity.Property(e => e.ViolationCount).HasColumnName("violation_count");

            entity.HasOne(d => d.MergedIntoTopic).WithMany(p => p.InverseMergedIntoTopic)
                .HasForeignKey(d => d.MergedIntoTopicId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_topics_merged_into");

            entity.HasOne(d => d.Producer).WithMany(p => p.Topics)
                .HasForeignKey(d => d.ProducerId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_topics_producer");

            entity.HasOne(d => d.Schema).WithMany(p => p.Topics)
                .HasForeignKey(d => d.SchemaId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_topics_schema");

            entity.HasMany(d => d.Consumers).WithMany(p => p.Topics)
                .UsingEntity<Dictionary<string, object>>(
                    "TopicConsumer",
                    r => r.HasOne<Consumer>().WithMany()
                        .HasForeignKey("ConsumerId")
                        .HasConstraintName("fk_topic_consumers_consumer"),
                    l => l.HasOne<Topic>().WithMany()
                        .HasForeignKey("TopicId")
                        .HasConstraintName("fk_topic_consumers_topic"),
                    j =>
                    {
                        j.HasKey("TopicId", "ConsumerId").HasName("topic_consumers_pkey");
                        j.ToTable("topic_consumers");
                        j.IndexerProperty<Guid>("TopicId").HasColumnName("topic_id");
                        j.IndexerProperty<Guid>("ConsumerId").HasColumnName("consumer_id");
                    });

            entity.HasMany(d => d.Tags).WithMany(p => p.Topics)
                .UsingEntity<Dictionary<string, object>>(
                    "TopicTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .HasConstraintName("fk_topic_tags_tag"),
                    l => l.HasOne<Topic>().WithMany()
                        .HasForeignKey("TopicId")
                        .HasConstraintName("fk_topic_tags_topic"),
                    j =>
                    {
                        j.HasKey("TopicId", "TagId").HasName("topic_tags_pkey");
                        j.ToTable("topic_tags");
                        j.IndexerProperty<Guid>("TopicId").HasColumnName("topic_id");
                        j.IndexerProperty<string>("TagId").HasColumnName("tag_id");
                    });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Role)
                .HasDefaultValueSql("'Viewer'::text")
                .HasColumnName("role");
        });

        modelBuilder.Entity<UserIdentity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_identities_pkey");

            entity.ToTable("user_identities");

            entity.HasIndex(e => new { e.AuthProvider, e.ExternalSubject }, "user_identities_auth_provider_external_subject_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AuthProvider).HasColumnName("auth_provider");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.ExternalSubject).HasColumnName("external_subject");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserIdentities)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_user_identities_user");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
