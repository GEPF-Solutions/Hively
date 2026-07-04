namespace Hively.Server.Services
{
    /// <summary>
    /// Rolls a topic's 24-hour activity histogram forward as a new message arrives.
    /// The histogram is 24 hourly message-count buckets, index 23 = the current hour
    /// (matches the prototype's hourLabel(23 - i) display mapping — oldest hour first).
    /// </summary>
    public static class ActivityHistogramRoller
    {
        public const int BucketCount = 24;

        /// <summary>
        /// Shifts <paramref name="current"/> left by however many whole hours have
        /// elapsed since <paramref name="lastSeenAtUtc"/> (dropping the oldest hours,
        /// padding zeros for the newest), then increments the current-hour bucket.
        /// A null <paramref name="lastSeenAtUtc"/> (brand-new topic) skips the shift
        /// entirely, since the histogram is already all zeros.
        /// </summary>
        public static int[] Roll(int[]? current, DateTime? lastSeenAtUtc, DateTime nowUtc)
        {
            var histogram = Normalize(current);

            if (lastSeenAtUtc.HasValue)
            {
                var hoursElapsed = HourBucket(nowUtc) - HourBucket(lastSeenAtUtc.Value);
                var shift = Math.Clamp(hoursElapsed, 0, BucketCount);

                if (shift >= BucketCount)
                {
                    histogram = new int[BucketCount];
                }
                else if (shift > 0)
                {
                    var shifted = new int[BucketCount];
                    Array.Copy(histogram, shift, shifted, 0, BucketCount - shift);
                    histogram = shifted;
                }
            }

            histogram[BucketCount - 1]++;
            return histogram;
        }

        private static int[] Normalize(int[]? current)
        {
            if (current != null && current.Length == BucketCount)
            {
                return (int[])current.Clone();
            }

            return new int[BucketCount];
        }

        private static long HourBucket(DateTime timestamp)
        {
            return timestamp.Ticks / TimeSpan.TicksPerHour;
        }
    }
}
