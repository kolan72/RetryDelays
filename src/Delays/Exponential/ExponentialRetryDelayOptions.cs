namespace RetryDelays
{
	/// <summary>
	///  Represents options for the <see cref="ExponentialRetryDelay"/>.
	/// </summary>
	public class ExponentialRetryDelayOptions : RetryDelayOptions
	{
		public override RetryDelayType DelayType => RetryDelayType.Exponential;

		/// <summary>
		/// Exponential factor to use.
		/// </summary>
		public double ExponentialFactor { get; set; } = RetryDelayConstants.ExponentialFactor;

		public static implicit operator ExponentialRetryDelay(ExponentialRetryDelayOptions options) => new ExponentialRetryDelay(options);
	}
}
