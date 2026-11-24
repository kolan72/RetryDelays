using System;

namespace RetryDelays
{
	internal static class StandardJitter
	{
		internal static double AddJitter(double delayInMs, double jitterPercent)
		{
			return AddJitter(delayInMs, jitterPercent, StaticRandom.RandDouble());
		}

		internal static double AddJitter(double delayInMs, double jitterPercent, double randomValue)
		{
			if (jitterPercent < 0 || jitterPercent > 100)
			{
				throw new ArgumentOutOfRangeException(nameof(jitterPercent), "Jitter percent must be between 0 and 100.");
			}

			if (randomValue < 0 || randomValue > 1)
			{
				throw new ArgumentOutOfRangeException(nameof(randomValue), "Random value must be between 0 and 1.");
			}

			var fraction = jitterPercent / 100.0;
			return delayInMs * (1.0 + (fraction * (2.0 * randomValue - 1.0)));
		}
	}
}
