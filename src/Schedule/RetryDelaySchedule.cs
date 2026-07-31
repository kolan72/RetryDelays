using System;
using System.Collections;
using System.Collections.Generic;

namespace RetryDelays
{
	/// <summary>
	/// An immutable, reusable sequence of <see cref="TimeSpan"/> delay values produced by composing
	/// one or more retry delay strategies into ordered phases.
	/// Each enumeration starts fresh from the first phase.
	/// </summary>
	/// <remarks>
	/// Use <see cref="Create"/> to obtain a <see cref="RetryDelayScheduleBuilder"/> and compose phases
	/// with the fluent API, then call <see cref="RetryDelayScheduleBuilder.Build"/> to get the schedule.
	/// </remarks>
	/// <example>
	/// <code>
	/// var schedule = RetryDelaySchedule.Create()
	///     .WithConstant(TimeSpan.FromMilliseconds(100), attempts: 3)
	///     .WithExponential(TimeSpan.FromSeconds(1), attempts: 5, maxDelay: TimeSpan.FromSeconds(30))
	///     .WithConstant(TimeSpan.FromMinutes(1))            // infinite final phase
	///     .Build();
	///
	/// foreach (var delay in schedule)
	/// {
	///     await Task.Delay(delay);
	///     // … retry logic …
	/// }
	/// </code>
	/// </example>
	public sealed class RetryDelaySchedule : IEnumerable<TimeSpan>
	{
		private readonly RetryDelayPhase[] _phases;
		private readonly int? _maxTotalAttempts;
		private readonly TimeSpan? _globalMaxDelay;

		internal RetryDelaySchedule(RetryDelayPhase[] phases, int? maxTotalAttempts, TimeSpan? globalMaxDelay)
		{
			_phases = phases ?? throw new ArgumentNullException(nameof(phases));
			_maxTotalAttempts = maxTotalAttempts;
			_globalMaxDelay = globalMaxDelay;
		}

		/// <summary>
		/// Creates a new <see cref="RetryDelayScheduleBuilder"/> to compose the schedule.
		/// </summary>
		public static RetryDelayScheduleBuilder Create() => new RetryDelayScheduleBuilder();

		/// <inheritdoc/>
		public IEnumerator<TimeSpan> GetEnumerator() => new ScheduleEnumerator(_phases, _maxTotalAttempts, _globalMaxDelay);

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		private sealed class ScheduleEnumerator : IEnumerator<TimeSpan>
		{
			private readonly RetryDelayPhase[] _phases;
			private readonly int? _maxTotalAttempts;
			private readonly TimeSpan? _globalMaxDelay;

			private int _phaseIndex;
			private int _phaseAttempt;    // per-phase attempt counter (resets each phase)
			private int _totalAttempts;   // global attempt counter across all phases

			private TimeSpan _current;

			public ScheduleEnumerator(RetryDelayPhase[] phases, int? maxTotalAttempts, TimeSpan? globalMaxDelay)
			{
				_phases = phases;
				_maxTotalAttempts = maxTotalAttempts;
				_globalMaxDelay = globalMaxDelay;
				Reset();
			}

			public TimeSpan Current => _current;

			object IEnumerator.Current => _current;

			public bool MoveNext()
			{
				// Global attempt cap
				if (_maxTotalAttempts.HasValue && _totalAttempts >= _maxTotalAttempts.Value)
					return false;

				// Find the active phase, skipping exhausted finite ones
				while (_phaseIndex < _phases.Length)
				{
					var phase = _phases[_phaseIndex];

					if (!phase.IsInfinite && _phaseAttempt >= phase.Attempts.Value)
					{
						// This phase is exhausted — move to the next one
						_phaseIndex++;
						_phaseAttempt = 0;
						continue;
					}

					// Compute the delay from the current phase using the per-phase attempt index
					var delay = phase.Delay.GetDelay(_phaseAttempt);

					// Apply global max delay cap if set
					if (_globalMaxDelay.HasValue && delay > _globalMaxDelay.Value)
						delay = _globalMaxDelay.Value;

					_current = delay;
					_phaseAttempt++;
					_totalAttempts++;
					return true;
				}

				// All phases exhausted
				return false;
			}

			public void Reset()
			{
				_phaseIndex = 0;
				_phaseAttempt = 0;
				_totalAttempts = 0;
				_current = default;
			}

			public void Dispose() { }
		}
	}
}
