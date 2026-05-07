using System;

namespace RetryDelays
{
	/// <summary>
	/// Represents options for subclasses of <see cref="RetryDelay"/>.
	/// </summary>
	public abstract class RetryDelayOptions
	{
		/// <summary>
		/// The type of delay.
		/// </summary>
		public abstract RetryDelayType DelayType { get; }

		/// <summary>
		/// Base delay value between retries.
		/// </summary>
		public TimeSpan BaseDelay { get; set; }

		/// <summary>
		/// Indicates whether jitter is used. The default value is <see langword="false"/>.
		/// </summary>
		public bool UseJitter { get; set; }

		/// <summary>
		/// Maximum delay between retries.
		/// </summary>
		public TimeSpan MaxDelay { get; set; } = TimeSpan.MaxValue;
	}

}
