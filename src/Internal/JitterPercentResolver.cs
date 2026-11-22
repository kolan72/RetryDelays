using System;

namespace RetryDelays
{
	internal static class JitterPercentResolver
	{
		internal static double Resolve(double? jitterPercent, bool useJitter, string paramName)
		{
			if (!jitterPercent.HasValue)
			{
				return RetryDelayConstants.DefaultJitterPercent;
			}

			if (!useJitter)
			{
				throw new ArgumentException("jitterPercent can only be specified when useJitter is true.", paramName);
			}

			var value = jitterPercent.Value;
			if (value < 0 || value > 100)
			{
				throw new ArgumentOutOfRangeException(paramName, "Jitter percent must be between 0 and 100.");
			}

			return value;
		}
	}
}