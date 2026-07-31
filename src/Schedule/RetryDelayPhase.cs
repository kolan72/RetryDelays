using System;

namespace RetryDelays
{
	/// <summary>
	/// Represents a single phase in a <see cref="RetryDelaySchedule"/>: a delay strategy
	/// paired with the number of attempts it applies to.
	/// </summary>
	internal readonly struct RetryDelayPhase
	{
		/// <summary>
		/// The delay strategy for this phase.
		/// </summary>
		internal RetryDelay Delay { get; }

		/// <summary>
		/// The number of attempts this phase covers.
		/// <see langword="null"/> means the phase is infinite (no upper bound).
		/// </summary>
		internal int? Attempts { get; }

		internal RetryDelayPhase(RetryDelay delay, int? attempts)
		{
			if (delay is null)
				throw new ArgumentNullException(nameof(delay));

			if (attempts.HasValue && attempts.Value <= 0)
				throw new ArgumentOutOfRangeException(nameof(attempts), "Attempts must be a positive integer or null for an infinite phase.");

			Delay = delay;
			Attempts = attempts;
		}

		/// <summary>
		/// Returns <see langword="true"/> when this phase repeats indefinitely.
		/// </summary>
		internal bool IsInfinite => !Attempts.HasValue;
	}
}
