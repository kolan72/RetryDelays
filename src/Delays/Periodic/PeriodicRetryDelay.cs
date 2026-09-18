using System;
using System.Collections.Generic;
using System.Linq;

namespace RetryDelays
{
	/// <summary>
	/// Represents a sealed class for handling periodic retry delay values.
	/// The delay sequence cycles back to the first value after reaching the last.
	/// </summary>
	public sealed class PeriodicRetryDelay : RetryDelay
	{
		/// <summary>
		/// Initializes a new instance of <see cref="PeriodicRetryDelay"/>.
		/// </summary>
		/// <param name="periodicOptions"><see cref="PeriodicRetryDelayOptions"/></param>
		public PeriodicRetryDelay(PeriodicRetryDelayOptions periodicOptions) : base(new PeriodicDelayCore(periodicOptions)) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="PeriodicRetryDelay"/> class with the specified base delay, optional maximum delay, and jitter setting.
		/// </summary>
		/// <param name="baseDelay">The base delay time span.</param>
		/// <param name="maxDelay">The optional maximum delay time span. If null, <see cref="TimeSpan.MaxValue"/> will be used.</param>
		/// <param name="useJitter">Whether to apply jitter to the delay values.</param>
		internal PeriodicRetryDelay(TimeSpan baseDelay, TimeSpan? maxDelay = null, bool useJitter = false)
		: this(new PeriodicRetryDelayOptions() { BaseDelay = baseDelay, MaxDelay = maxDelay ?? TimeSpan.MaxValue, UseJitter = useJitter, Times = new[] { baseDelay } })
		{
		}

		/// <summary>
		/// Creates a new <see cref="PeriodicRetryDelay"/> instance with four specified delay times.
		/// </summary>
		/// <param name="firstTime">The first delay time.</param>
		/// <param name="secondTime">The second delay time.</param>
		/// <param name="thirdTime">The third delay time.</param>
		/// <param name="fourthTime">The fourth delay time.</param>
		/// <param name="maxDelay">The optional maximum delay time span.</param>
		/// <param name="useJitter">Whether to apply jitter to the delay values.</param>
		/// <returns>A new <see cref="PeriodicRetryDelay"/> instance.</returns>
		public static PeriodicRetryDelay Create(TimeSpan firstTime, TimeSpan secondTime, TimeSpan thirdTime, TimeSpan fourthTime, TimeSpan? maxDelay = null, bool useJitter = false) =>
				Create(new TimeSpan[] { firstTime, secondTime, thirdTime, fourthTime }, maxDelay, useJitter);

		/// <summary>
		/// Creates a new <see cref="PeriodicRetryDelay"/> instance with three specified delay times.
		/// </summary>
		/// <param name="firstTime">The first delay time.</param>
		/// <param name="secondTime">The second delay time.</param>
		/// <param name="thirdTime">The third delay time.</param>
		/// <param name="maxDelay">The optional maximum delay time span.</param>
		/// <param name="useJitter">Whether to apply jitter to the delay values.</param>
		/// <returns>A new <see cref="PeriodicRetryDelay"/> instance.</returns>
		public static PeriodicRetryDelay Create(TimeSpan firstTime, TimeSpan secondTime, TimeSpan thirdTime, TimeSpan? maxDelay = null, bool useJitter = false) =>
				Create(new TimeSpan[] { firstTime, secondTime, thirdTime }, maxDelay, useJitter);

		/// <summary>
		/// Creates a new <see cref="PeriodicRetryDelay"/> instance with two specified delay times.
		/// </summary>
		/// <param name="firstTime">The first delay time.</param>
		/// <param name="secondTime">The second delay time.</param>
		/// <param name="maxDelay">The optional maximum delay time span.</param>
		/// <param name="useJitter">Whether to apply jitter to the delay values.</param>
		/// <returns>A new <see cref="PeriodicRetryDelay"/> instance.</returns>
		public static PeriodicRetryDelay Create(TimeSpan firstTime, TimeSpan secondTime, TimeSpan? maxDelay = null, bool useJitter = false) =>
				Create(new TimeSpan[] { firstTime, secondTime }, maxDelay, useJitter);

		/// <summary>
		/// Creates a new <see cref="PeriodicRetryDelay"/> instance with a single specified delay time.
		/// </summary>
		/// <param name="firstTime">The delay time.</param>
		/// <param name="maxDelay">The optional maximum delay time span.</param>
		/// <param name="useJitter">Whether to apply jitter to the delay values.</param>
		/// <returns>A new <see cref="PeriodicRetryDelay"/> instance.</returns>
		public static PeriodicRetryDelay Create(TimeSpan firstTime, TimeSpan? maxDelay = null, bool useJitter = false) =>
				Create(new TimeSpan[] { firstTime }, maxDelay, useJitter);

		/// <summary>
		/// Creates a new <see cref="PeriodicRetryDelay"/> instance with a collection of specified delay times.
		/// </summary>
		/// <param name="times">The collection of delay times.</param>
		/// <param name="maxDelay">The optional maximum delay time span.</param>
		/// <param name="useJitter">Whether to apply jitter to the delay values.</param>
		/// <returns>A new <see cref="PeriodicRetryDelay"/> instance.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="times"/> is null.</exception>
		public static PeriodicRetryDelay Create(IEnumerable<TimeSpan> times, TimeSpan? maxDelay = null, bool useJitter = false)
		{
			if (times is null)
			{
				throw new ArgumentNullException(nameof(times));
			}
			return new PeriodicRetryDelay(new PeriodicRetryDelayOptions()
			{
				BaseDelay = TimeSpan.Zero,
				MaxDelay = maxDelay ?? TimeSpan.MaxValue,
				UseJitter = useJitter,
				Times = times.ToArray()
			});
		}
	}
}
