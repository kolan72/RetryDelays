using System;
using System.Runtime.CompilerServices;

namespace RetryDelays
{
	internal class ConstantDelayCore : DelayCoreBase
	{
		private readonly ConstantRetryDelayOptions _delayOptions;
		private readonly double _adaptedMaxDelayMs;

		public ConstantDelayCore(ConstantRetryDelayOptions delayOptions) : base(delayOptions)
		{
			_delayOptions = delayOptions;
			if (delayOptions.UseJitter)
			{
				_adaptedMaxDelayMs = MaxDelayHelper.GetAdaptedMaxDelayMs(delayOptions.MaxDelay);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override TimeSpan GetBaseDelay(int attempt)
		{
			return _delayOptions.BaseDelay;
		}

		protected override TimeSpan GetJitteredDelay(int attempt)
		{
			return MaxDelayHelper.LimitToMaxDelay(
				StandardJitter.AddJitter(_delayOptions.BaseDelay.TotalMilliseconds),
				_adaptedMaxDelayMs,
				_delayOptions.MaxDelay);
		}
	}
}
