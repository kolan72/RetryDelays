using System;
using System.Runtime.CompilerServices;

namespace RetryDelays
{
	internal class LinearDelayCore : DelayCoreBase
	{
		private readonly LinearRetryDelayOptions _delayOptions;
		private readonly double _adaptedMaxDelayMs;

		public LinearDelayCore(LinearRetryDelayOptions delayOptions) : base(delayOptions)
		{
			_delayOptions = delayOptions;
			_adaptedMaxDelayMs = MaxDelayHelper.GetAdaptedMaxDelayMs(delayOptions.MaxDelay);
		}

		protected override TimeSpan GetBaseDelay(int attempt)
		{
			return MaxDelayHelper.LimitToMaxDelay(GetDelayValueInMs(attempt, _delayOptions), _adaptedMaxDelayMs, _delayOptions.MaxDelay);
		}

		protected override TimeSpan GetJitteredDelay(int attempt)
		{
			return MaxDelayHelper.LimitToMaxDelay(StandardJitter.AddJitter(GetDelayValueInMs(attempt, _delayOptions)), _adaptedMaxDelayMs, _delayOptions.MaxDelay);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static double GetDelayValueInMs(int attempt, LinearRetryDelayOptions options)
		{
			return (attempt + 1) * options.SlopeFactor * options.BaseDelay.TotalMilliseconds;
		}
	}
}
