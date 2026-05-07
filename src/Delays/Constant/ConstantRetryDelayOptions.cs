namespace RetryDelays
{
	/// <summary>
	/// Represents options for the <see cref="ConstantRetryDelay"/>.
	/// </summary>
	public class ConstantRetryDelayOptions : RetryDelayOptions
	{
		public override RetryDelayType DelayType => RetryDelayType.Constant;

		public static implicit operator ConstantRetryDelay(ConstantRetryDelayOptions options) => new ConstantRetryDelay(options);
	}
}
