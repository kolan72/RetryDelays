using System;

namespace RetryDelays
{
	internal class ExponentialDelayCore : DelayCoreBase
	{
		private readonly ExponentialRetryDelayOptions _delayOptions;

		private readonly DecorrelatedJitter _jitter;
		private readonly double _adaptedMaxDelayMs;

		public ExponentialDelayCore(ExponentialRetryDelayOptions delayOptions) : base(delayOptions)
		{
			_delayOptions = delayOptions;

			if (delayOptions.UseJitter)
			{
				_jitter = new DecorrelatedJitter(delayOptions.BaseDelay, delayOptions.ExponentialFactor, delayOptions.MaxDelay);
			}
			else
			{
				_adaptedMaxDelayMs = MaxDelayHelper.GetAdaptedMaxDelayMs(delayOptions.MaxDelay);
			}
		}

		protected override TimeSpan GetBaseDelay(int attempt)
			=> MaxDelayHelper.LimitToMaxDelay(Math.Pow(_delayOptions.ExponentialFactor, attempt) * _delayOptions.BaseDelay.TotalMilliseconds, _adaptedMaxDelayMs, _delayOptions.MaxDelay);

		protected override TimeSpan GetJitteredDelay(int attempt)
			=> _jitter.DecorrelatedJitterBackoffV2(attempt);
	}
}
