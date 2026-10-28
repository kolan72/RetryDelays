using System;

namespace RetryDelays
{
	internal static class StandardJitter
	{
		internal static double AddJitter(double delayInMs, double jitterPercent)
		{
			if (jitterPercent < 0 || jitterPercent > 100)
			{
				throw new ArgumentOutOfRangeException(nameof(jitterPercent), "Jitter percent must be between 0 and 100.");
			}

			var fraction = jitterPercent / 100.0;
			return delayInMs * (1.0 + (fraction * (2.0 * StaticRandom.RandDouble() - 1.0)));
		}
	}
}
