namespace RetryDelays
{
	/// <summary>
	/// Represents options for the <see cref="LinearRetryDelay"/>.
	/// </summary>
	public class LinearRetryDelayOptions : JitteredRetryDelayOptions
	{
		public override RetryDelayType DelayType => RetryDelayType.Linear;

		/// <summary>
		/// Slope factor to use.
		/// </summary>
		public double SlopeFactor { get; set; } = RetryDelayConstants.SlopeFactor;

		public static implicit operator LinearRetryDelay(LinearRetryDelayOptions options) => new LinearRetryDelay(options);
	}
}
