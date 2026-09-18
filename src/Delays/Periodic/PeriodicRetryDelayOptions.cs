using System;

namespace RetryDelays
{
	/// <summary>
	/// Represents options for the periodic retry delay type.
	/// </summary>
	public class PeriodicRetryDelayOptions : RetryDelayOptions
	{
		/// <summary>
		/// Gets or sets the sequence of time intervals to use between attempts.
		/// The sequence will cycle back to the first value after reaching the last.
		/// </summary>
		public TimeSpan[] Times { get; set; } = Array.Empty<TimeSpan>();

		/// <inheritdoc/>
		public override RetryDelayType DelayType => RetryDelayType.Periodic;

		public static implicit operator PeriodicRetryDelay(PeriodicRetryDelayOptions options) => new PeriodicRetryDelay(options);

	}
}
