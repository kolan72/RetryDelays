using System;
using System.Collections.Generic;

namespace RetryDelays
{
	/// <summary>
	/// A fluent builder for composing a <see cref="RetryDelaySchedule"/> from ordered phases.
	/// Each phase pairs a delay strategy with an optional attempt count.
	/// When <c>attempts</c> is <see langword="null"/> the phase is infinite; only the last phase
	/// should be infinite (a validation error is raised if an earlier phase is unbounded).
	/// </summary>
	public sealed class RetryDelayScheduleBuilder
	{
		private readonly List<RetryDelayPhase> _phases = new List<RetryDelayPhase>();
		private int? _maxTotalAttempts;
		private TimeSpan? _globalMaxDelay;

		internal RetryDelayScheduleBuilder() { }

		// -------------------------------------------------------------------------
		// Phase methods — existing RetryDelay strategies
		// -------------------------------------------------------------------------

		/// <summary>
		/// Adds a phase that uses an existing <see cref="RetryDelay"/> instance.
		/// </summary>
		/// <param name="retryDelay">The delay strategy.</param>
		/// <param name="attempts">
		/// Number of attempts for this phase. Pass <see langword="null"/> for an infinite phase (last phase only).
		/// </param>
		public RetryDelayScheduleBuilder WithDelay(RetryDelay retryDelay, int? attempts = null)
		{
			AddPhase(retryDelay, attempts);
			return this;
		}

		/// <summary>
		/// Adds a constant-delay phase.
		/// </summary>
		/// <param name="delay">The constant delay value.</param>
		/// <param name="attempts">
		/// Number of attempts for this phase. Pass <see langword="null"/> for an infinite phase (last phase only).
		/// </param>
		/// <param name="maxDelay">Optional maximum delay cap.</param>
		/// <param name="useJitter">Whether to apply standard jitter.</param>
		public RetryDelayScheduleBuilder WithConstant(
			TimeSpan delay,
			int? attempts = null,
			TimeSpan? maxDelay = null,
			bool useJitter = false)
		{
			AddPhase(ConstantRetryDelay.Create(delay, maxDelay, useJitter), attempts);
			return this;
		}

		/// <summary>
		/// Adds a linear-backoff phase.
		/// </summary>
		/// <param name="baseDelay">Base delay for the linear calculation.</param>
		/// <param name="attempts">
		/// Number of attempts for this phase. Pass <see langword="null"/> for an infinite phase (last phase only).
		/// </param>
		/// <param name="slopeFactor">Multiplier applied per attempt (default: 1.0).</param>
		/// <param name="maxDelay">Optional maximum delay cap.</param>
		/// <param name="useJitter">Whether to apply standard jitter.</param>
		public RetryDelayScheduleBuilder WithLinear(
			TimeSpan baseDelay,
			int? attempts = null,
			double slopeFactor = RetryDelayConstants.SlopeFactor,
			TimeSpan? maxDelay = null,
			bool useJitter = false)
		{
			AddPhase(LinearRetryDelay.Create(baseDelay, slopeFactor, maxDelay, useJitter), attempts);
			return this;
		}

		/// <summary>
		/// Adds an exponential-backoff phase.
		/// </summary>
		/// <param name="baseDelay">Base delay for the exponential calculation.</param>
		/// <param name="attempts">
		/// Number of attempts for this phase. Pass <see langword="null"/> for an infinite phase (last phase only).
		/// </param>
		/// <param name="exponentialFactor">Exponential growth factor (default: 2.0).</param>
		/// <param name="maxDelay">Optional maximum delay cap.</param>
		/// <param name="useJitter">Whether to apply jitter (decorrelated for exponential).</param>
		public RetryDelayScheduleBuilder WithExponential(
			TimeSpan baseDelay,
			int? attempts = null,
			double exponentialFactor = RetryDelayConstants.ExponentialFactor,
			TimeSpan? maxDelay = null,
			bool useJitter = false)
		{
			AddPhase(ExponentialRetryDelay.Create(baseDelay, exponentialFactor, maxDelay, useJitter), attempts);
			return this;
		}

		/// <summary>
		/// Adds a time-series phase. Once the series is exhausted the last value repeats.
		/// </summary>
		/// <param name="times">Ordered list of delay values.</param>
		/// <param name="attempts">
		/// Number of attempts for this phase. Pass <see langword="null"/> for an infinite phase (last phase only).
		/// </param>
		/// <param name="maxDelay">Optional maximum delay cap.</param>
		/// <param name="useJitter">Whether to apply standard jitter.</param>
		public RetryDelayScheduleBuilder WithTimeSeries(
			IEnumerable<TimeSpan> times,
			int? attempts = null,
			TimeSpan? maxDelay = null,
			bool useJitter = false)
		{
			AddPhase(TimeSeriesRetryDelay.Create(times, maxDelay, useJitter), attempts);
			return this;
		}

		/// <summary>
		/// Adds a periodic phase. The delay series cycles back to the first value after the last.
		/// </summary>
		/// <param name="times">Ordered list of delay values that cycle.</param>
		/// <param name="attempts">
		/// Number of attempts for this phase. Pass <see langword="null"/> for an infinite phase (last phase only).
		/// </param>
		/// <param name="maxDelay">Optional maximum delay cap.</param>
		/// <param name="useJitter">Whether to apply standard jitter.</param>
		public RetryDelayScheduleBuilder WithPeriodic(
			IEnumerable<TimeSpan> times,
			int? attempts = null,
			TimeSpan? maxDelay = null,
			bool useJitter = false)
		{
			AddPhase(PeriodicRetryDelay.Create(times, maxDelay, useJitter), attempts);
			return this;
		}

		/// <summary>
		/// Adds a phase backed by a custom delay provider function.
		/// The function receives the per-phase attempt index (zero-based) and returns the delay.
		/// </summary>
		/// <param name="delayProvider">Custom delay calculation function.</param>
		/// <param name="attempts">
		/// Number of attempts for this phase. Pass <see langword="null"/> for an infinite phase (last phase only).
		/// </param>
		public RetryDelayScheduleBuilder WithCustom(Func<int, TimeSpan> delayProvider, int? attempts = null)
		{
			if (delayProvider is null)
				throw new ArgumentNullException(nameof(delayProvider));

			AddPhase(new RetryDelay(delayProvider), attempts);
			return this;
		}

		// -------------------------------------------------------------------------
		// Global modifiers
		// -------------------------------------------------------------------------

		/// <summary>
		/// Caps the total number of delay values the schedule will emit across all phases.
		/// </summary>
		/// <param name="maxAttempts">Maximum total attempts (must be positive).</param>
		public RetryDelayScheduleBuilder WithMaxTotalAttempts(int maxAttempts)
		{
			if (maxAttempts <= 0)
				throw new ArgumentOutOfRangeException(nameof(maxAttempts), "MaxTotalAttempts must be a positive integer.");

			_maxTotalAttempts = maxAttempts;
			return this;
		}

		/// <summary>
		/// Applies an upper-bound cap to every delay value emitted by the schedule,
		/// regardless of what the individual phase strategy computes.
		/// </summary>
		/// <param name="maxDelay">The global maximum delay.</param>
		public RetryDelayScheduleBuilder WithGlobalMaxDelay(TimeSpan maxDelay)
		{
			if (maxDelay <= TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(maxDelay), "GlobalMaxDelay must be greater than zero.");

			_globalMaxDelay = maxDelay;
			return this;
		}

		// -------------------------------------------------------------------------
		// Build
		// -------------------------------------------------------------------------

		/// <summary>
		/// Validates the composed phases and returns an immutable <see cref="RetryDelaySchedule"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the schedule has no phases, or when a non-last phase is infinite.
		/// </exception>
		public RetryDelaySchedule Build()
		{
			if (_phases.Count == 0)
				throw new InvalidOperationException("A schedule must have at least one phase. Call one of the With* methods before Build().");

			// Only the last phase may be infinite
			for (int i = 0; i < _phases.Count - 1; i++)
			{
				if (_phases[i].IsInfinite)
					throw new InvalidOperationException(
						$"Phase {i} is infinite (attempts: null) but is not the last phase. " +
						"Only the last phase may be infinite.");
			}

			return new RetryDelaySchedule(_phases.ToArray(), _maxTotalAttempts, _globalMaxDelay);
		}

		// -------------------------------------------------------------------------
		// Private helpers
		// -------------------------------------------------------------------------

		private void AddPhase(RetryDelay delay, int? attempts)
		{
			_phases.Add(new RetryDelayPhase(delay, attempts));
		}
	}
}
