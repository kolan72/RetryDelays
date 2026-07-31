using System;
using System.Runtime.CompilerServices;

namespace RetryDelays
{
	internal class PeriodicDelayCore : DelayCoreBase
	{
		private readonly double _adaptedMaxDelayMs;
		private readonly TimeSpan _maxDelay;
		private readonly TimeSpan[] _times;
		private readonly int _timesLength;

		public PeriodicDelayCore(PeriodicRetryDelayOptions delayOptions) : base(delayOptions)
		{
			_adaptedMaxDelayMs = MaxDelayHelper.GetAdaptedMaxDelayMs(delayOptions.MaxDelay);
			_maxDelay = delayOptions.MaxDelay;
			if (delayOptions.Times?.Length == 0)
			{
				_times = new[] { delayOptions.BaseDelay > delayOptions.MaxDelay ? delayOptions.MaxDelay : delayOptions.BaseDelay };
			}
			else
			{
				_times = delayOptions.Times;
			}
			_timesLength = _times.Length;
		}

		protected override TimeSpan GetBaseDelay(int attempt)
		{
			return MaxDelayHelper.LimitToMaxDelay(GetDelayInner(attempt).TotalMilliseconds, _adaptedMaxDelayMs, _maxDelay);
		}

		protected override TimeSpan GetJitteredDelay(int attempt)
		{
			return MaxDelayHelper.LimitToMaxDelay(StandardJitter.AddJitter(GetDelayInner(attempt).TotalMilliseconds), _adaptedMaxDelayMs, _maxDelay);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private TimeSpan GetDelayInner(int attempt)
		{
			// Use modulo to cycle through the times array
			return _times[attempt % _timesLength];
		}
	}
}
