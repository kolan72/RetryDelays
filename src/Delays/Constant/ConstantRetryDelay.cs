using System;

namespace RetryDelays
{
	/// <summary>
	/// Class to get the constant delay value.
	/// </summary>
	public sealed class ConstantRetryDelay : RetryDelay
	{
		/// <summary>
		/// Initializes a new instance of <see cref="ConstantRetryDelay"/>.
		/// </summary>
		/// <param name="retryDelayOptions"><see cref="ConstantRetryDelayOptions"/></param>
		public ConstantRetryDelay(ConstantRetryDelayOptions retryDelayOptions) : base(new ConstantDelayCore(retryDelayOptions))
		{
			if (retryDelayOptions.UseJitter && retryDelayOptions.MaxDelay < retryDelayOptions.BaseDelay)
			{
				throw new ArgumentOutOfRangeException(nameof(retryDelayOptions), "MaxDelay must be greater than or equal to BaseDelay.");
			}
		}

		/// <summary>
		/// Creates <see cref="ConstantRetryDelay"/>.
		/// </summary>
		/// <param name="baseDelay">Base delay value between retries.</param>
		/// <param name="maxDelay">Maximum delay value. Only used if <paramref name="useJitter"/> is true. If null, it will be set to <see cref="TimeSpan.MaxValue"/>.</param>
		/// <param name="useJitter">Whether jitter is used.</param>
		/// <returns></returns>
		public static ConstantRetryDelay Create(TimeSpan baseDelay, TimeSpan? maxDelay = null, bool useJitter = false) => new ConstantRetryDelay(baseDelay, maxDelay, useJitter);

		internal ConstantRetryDelay(TimeSpan baseDelay, TimeSpan? maxDelay = null, bool useJitter = false) : this(new ConstantRetryDelayOptions() { BaseDelay = baseDelay, UseJitter = useJitter, MaxDelay = maxDelay ?? TimeSpan.MaxValue }) { }
	}
}
