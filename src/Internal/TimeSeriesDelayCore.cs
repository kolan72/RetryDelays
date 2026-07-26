using System;
using System.Runtime.CompilerServices;

namespace RetryDelays
{
	internal class TimeSeriesDelayCore : DelayCoreBase
	{
		private readonly double _adaptedMaxDelayMs;
		private readonly TimeSpan _maxDelay;
		private readonly TimeSpan[] _times;
		private readonly int _maxIndex;

		public TimeSeriesDelayCore(TimeSeriesRetryDelayOptions delayOptions) : base(delayOptions)
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
			_maxIndex = _times.Length - 1;
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
			return _times[(uint)attempt <= (uint)_maxIndex ? attempt : _maxIndex];
		}
	}
}
