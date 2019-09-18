using System;

namespace Services.Broadcast.Extensions
{
    /// <summary>
    /// Extensions for nullable data types.
    /// </summary>
    public static class NullableExtensions
    {
        /// <summary>
        /// Retrieves the value from the nullable.
        /// Throws if given null.
        /// </summary>
        /// <param name="candidate">The candidate.</param>
        /// <returns>The candidate value.</returns>
        /// <exception cref="ArgumentNullException">date</exception>
        public static DateTime GetValueOrThrow(this DateTime? candidate) 
        {
            if (candidate.HasValue == false)
            {
                throw new ArgumentNullException(nameof(candidate));
            }

            return candidate.Value;
        }

        /// <summary>
        /// Retrieves the value from the nullable.
        /// Throws if given null.
        /// </summary>
        /// <param name="candidate">The candidate.</param>
        /// <returns>The candidate value.</returns>
        /// <exception cref="ArgumentNullException">date</exception>
        public static int GetValueOrThrow(this int? candidate)
        {
            if (candidate.HasValue == false)
            {
                throw new ArgumentNullException(nameof(candidate));
            }

            return candidate.Value;
        }

        /// <summary>
        /// Retrieves the value from the nullable.
        /// Throws if given null.
        /// </summary>
        /// <param name="candidate">The candidate.</param>
        /// <returns>The candidate value.</returns>
        /// <exception cref="ArgumentNullException">date</exception>
        public static decimal GetValueOrThrow(this decimal? candidate)
        {
            if (candidate.HasValue == false)
            {
                throw new ArgumentNullException(nameof(candidate));
            }

            return candidate.Value;
        }

        /// <summary>
        /// Retrieves the value from the nullable.
        /// Throws if given null.
        /// </summary>
        /// <param name="candidate">The candidate.</param>
        /// <returns>The candidate value.</returns>
        /// <exception cref="ArgumentNullException">date</exception>
        public static double GetValueOrThrow(this double? candidate)
        {
            if (candidate.HasValue == false)
            {
                throw new ArgumentNullException(nameof(candidate));
            }

            return candidate.Value;
        }
    }
}