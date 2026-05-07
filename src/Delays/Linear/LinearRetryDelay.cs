using System;

namespace RetryDelays
{
	/// <summary>
	/// Class to get the delay value calculated linearly.
	/// </summary>
	public sealed class LinearRetryDelay : RetryDelay
	{
		/// <summary>
		/// Initializes a new instance of <see cref="LinearRetryDelay"/>.
		/// </summary>
		/// <param name="retryDelayOptions"><see cref="LinearRetryDelayOptions"/></param>
		public LinearRetryDelay(LinearRetryDelayOptions retryDelayOptions) : base(new LinearDelayCore(retryDelayOptions))
		{
		}

		/// <summary>
		///  Creates <see cref="LinearRetryDelay"/>.
		/// </summary>
		/// <param name="baseDelay">Base delay value between retries.</param>
		/// <param name="maxDelay">Maximum delay value. If null, it will be set to <see cref="TimeSpan.MaxValue"/>.</param>
		/// <param name="useJitter">Whether jitter is used.</param>
		/// <returns></returns>
		public static LinearRetryDelay Create(TimeSpan baseDelay, TimeSpan? maxDelay = null, bool useJitter = false) => new LinearRetryDelay(baseDelay, maxDelay: maxDelay, useJitter: useJitter);

		/// <summary>
		///  Creates <see cref="LinearRetryDelay"/>.
		/// </summary>
		/// <param name="baseDelay">Base delay value between retries.</param>
		/// <param name="slopeFactor">Slope factor to use.</param>
		/// <param name="maxDelay">Maximum delay value. If null, it will be set to <see cref="TimeSpan.MaxValue"/>.</param>
		/// <param name="useJitter">Whether jitter is used.</param>
		/// <returns></returns>
		public static LinearRetryDelay Create(TimeSpan baseDelay, double slopeFactor, TimeSpan? maxDelay = null, bool useJitter = false) => new LinearRetryDelay(baseDelay, slopeFactor, maxDelay, useJitter);

		internal LinearRetryDelay(TimeSpan baseDelay, double slopeFactor = RetryDelayConstants.SlopeFactor, TimeSpan? maxDelay = null, bool useJitter = false) :
			 this(new LinearRetryDelayOptions() { BaseDelay = baseDelay, SlopeFactor = slopeFactor, UseJitter = useJitter, MaxDelay = maxDelay ?? TimeSpan.MaxValue })
		{ }
	}
}
