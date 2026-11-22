using System;

namespace RetryDelays
{
	/// <summary>
	/// Base options for retry delays that support standard jitter — a random spread
	/// of ±<see cref="JitterPercent"/> around the calculated delay.
	/// </summary>
	/// <remarks>
	/// Delays based on this class apply standard jitter when
	/// <see cref="RetryDelayOptions.UseJitter"/> is <see langword="true"/>.
	/// Note that <see cref="ExponentialRetryDelay"/> uses decorrelated jitter instead
	/// and therefore its options do not derive from this class.
	/// </remarks>
	public abstract class JitteredRetryDelayOptions : RetryDelayOptions
	{
		private double _jitterPercent = RetryDelayConstants.DefaultJitterPercent;

		/// <summary>
		/// Jitter spread as a percentage of the calculated delay (±).
		/// The default value is <c>25</c>, which randomizes delays within ±25% of the calculated value.
		/// Only used when <see cref="RetryDelayOptions.UseJitter"/> is <see langword="true"/>.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when the assigned value is negative or greater than 100.
		/// </exception>
		public double JitterPercent
		{
			get => _jitterPercent;
			set
			{
				if (value < 0 || value > 100)
				{
					throw new ArgumentOutOfRangeException(nameof(value), "Jitter percent must be between 0 and 100.");
				}

				_jitterPercent = value;
			}
		}
	}
}
