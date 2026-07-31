using System;
using System.Collections.Generic;
using System.Linq;

namespace RetryDelays.Tests
{
	internal class RetryDelayScheduleTests
	{
		// -------------------------------------------------------------------------
		// Build validation
		// -------------------------------------------------------------------------

		[Test]
		public void Build_Should_Throw_When_No_Phases_Added()
		{
			Assert.That(
				() => RetryDelaySchedule.Create().Build(),
				Throws.InvalidOperationException);
		}

		[Test]
		public void Build_Should_Throw_When_Non_Last_Phase_Is_Infinite()
		{
			Assert.That(
				() => RetryDelaySchedule.Create()
					.WithConstant(TimeSpan.FromSeconds(1), attempts: null) // infinite but not last
					.WithExponential(TimeSpan.FromSeconds(2), attempts: 3)
					.Build(),
				Throws.InvalidOperationException);
		}

		[Test]
		public void Build_Should_Succeed_When_Last_Phase_Is_Infinite()
		{
			Assert.That(
				() => RetryDelaySchedule.Create()
					.WithConstant(TimeSpan.FromMilliseconds(100), attempts: 3)
					.WithConstant(TimeSpan.FromSeconds(1), attempts: null)
					.Build(),
				Throws.Nothing);
		}

		[Test]
		public void WithMaxTotalAttempts_Should_Throw_When_Zero_Or_Negative()
		{
			Assert.That(
				() => RetryDelaySchedule.Create().WithMaxTotalAttempts(0),
				Throws.TypeOf<ArgumentOutOfRangeException>());

			Assert.That(
				() => RetryDelaySchedule.Create().WithMaxTotalAttempts(-1),
				Throws.TypeOf<ArgumentOutOfRangeException>());
		}

		[Test]
		public void WithGlobalMaxDelay_Should_Throw_When_Zero_Or_Negative()
		{
			Assert.That(
				() => RetryDelaySchedule.Create().WithGlobalMaxDelay(TimeSpan.Zero),
				Throws.TypeOf<ArgumentOutOfRangeException>());

			Assert.That(
				() => RetryDelaySchedule.Create().WithGlobalMaxDelay(TimeSpan.FromSeconds(-1)),
				Throws.TypeOf<ArgumentOutOfRangeException>());
		}

		[Test]
		public void WithCustom_Should_Throw_When_Provider_Is_Null()
		{
			Assert.That(
				() => RetryDelaySchedule.Create().WithCustom(null),
				Throws.TypeOf<ArgumentNullException>());
		}

		// -------------------------------------------------------------------------
		// Single-phase schedules
		// -------------------------------------------------------------------------

		[Test]
		public void SingleConstantPhase_Should_Emit_Correct_Number_Of_Delays()
		{
			var delay = TimeSpan.FromMilliseconds(200);
			var schedule = RetryDelaySchedule.Create()
				.WithConstant(delay, attempts: 4)
				.Build();

			var results = schedule.ToList();

			Assert.That(results.Count, Is.EqualTo(4));
		}

		[Test]
		public void SingleConstantPhase_Should_Emit_Constant_Values()
		{
			var delay = TimeSpan.FromMilliseconds(300);
			var schedule = RetryDelaySchedule.Create()
				.WithConstant(delay, attempts: 3)
				.Build();

			foreach (var d in schedule)
				Assert.That(d, Is.EqualTo(delay));
		}

		[Test]
		public void SingleInfinitePhase_With_MaxTotalAttempts_Should_Emit_Capped_Count()
		{
			var schedule = RetryDelaySchedule.Create()
				.WithConstant(TimeSpan.FromSeconds(1), attempts: null)
				.WithMaxTotalAttempts(5)
				.Build();

			Assert.That(schedule.Count(), Is.EqualTo(5));
		}

		// -------------------------------------------------------------------------
		// Multi-phase sequencing
		// -------------------------------------------------------------------------

		[Test]
		public void TwoPhases_Should_Emit_Combined_Count()
		{
			var schedule = RetryDelaySchedule.Create()
				.WithConstant(TimeSpan.FromMilliseconds(100), attempts: 3)
				.WithConstant(TimeSpan.FromSeconds(1), attempts: 2)
				.Build();

			Assert.That(schedule.Count(), Is.EqualTo(5));
		}

		[Test]
		public void TwoPhases_Should_Emit_Values_In_Phase_Order()
		{
			var fast = TimeSpan.FromMilliseconds(100);
			var slow = TimeSpan.FromSeconds(2);

			var schedule = RetryDelaySchedule.Create()
				.WithConstant(fast, attempts: 2)
				.WithConstant(slow, attempts: 2)
				.Build();

			var results = schedule.ToList();

			Assert.That(results[0], Is.EqualTo(fast));
			Assert.That(results[1], Is.EqualTo(fast));
			Assert.That(results[2], Is.EqualTo(slow));
			Assert.That(results[3], Is.EqualTo(slow));
		}

		[Test]
		public void ThreePhases_Should_Sequence_Correctly()
		{
			// constant(3) → linear(attempts 2) → constant-infinite capped at 2
			var schedule = RetryDelaySchedule.Create()
				.WithConstant(TimeSpan.FromMilliseconds(50), attempts: 2)
				.WithConstant(TimeSpan.FromMilliseconds(200), attempts: 3)
				.WithConstant(TimeSpan.FromSeconds(1), attempts: null)
				.WithMaxTotalAttempts(7)
				.Build();

			Assert.That(schedule.Count(), Is.EqualTo(7));

			var results = schedule.ToList();

			// First 2 from phase 1
			Assert.That(results.Take(2).All(d => d == TimeSpan.FromMilliseconds(50)), Is.True);
			// Next 3 from phase 2
			Assert.That(results.Skip(2).Take(3).All(d => d == TimeSpan.FromMilliseconds(200)), Is.True);
			// Last 2 from infinite phase 3
			Assert.That(results.Skip(5).All(d => d == TimeSpan.FromSeconds(1)), Is.True);
		}

		// -------------------------------------------------------------------------
		// Per-phase attempt indexing
		// -------------------------------------------------------------------------

		[Test]
		public void LinearPhase_Should_Reset_Attempt_Index_Per_Phase()
		{
			// Linear: (attempt+1) * slopeFactor * baseDelay
			// With baseDelay=1s, slopeFactor=1: attempt0→1s, attempt1→2s, attempt2→3s
			var baseDelay = TimeSpan.FromSeconds(1);

			var schedule = RetryDelaySchedule.Create()
				.WithConstant(TimeSpan.FromMilliseconds(100), attempts: 2)
				.WithLinear(baseDelay, attempts: 3, slopeFactor: 1.0)
				.Build();

			var results = schedule.ToList();

			// Linear phase starts at per-phase attempt 0
			Assert.That(results[2], Is.EqualTo(TimeSpan.FromSeconds(1)));  // attempt 0
			Assert.That(results[3], Is.EqualTo(TimeSpan.FromSeconds(2)));  // attempt 1
			Assert.That(results[4], Is.EqualTo(TimeSpan.FromSeconds(3)));  // attempt 2
		}

		// -------------------------------------------------------------------------
		// Global modifiers
		// -------------------------------------------------------------------------

		[Test]
		public void GlobalMaxDelay_Should_Cap_All_Values()
		{
			var cap = TimeSpan.FromSeconds(1);

			// Exponential will grow beyond 1s quickly
			var schedule = RetryDelaySchedule.Create()
				.WithExponential(TimeSpan.FromMilliseconds(500), attempts: 6)
				.WithGlobalMaxDelay(cap)
				.Build();

			foreach (var d in schedule)
				Assert.That(d, Is.LessThanOrEqualTo(cap));
		}

		[Test]
		public void MaxTotalAttempts_Should_Override_Phase_Attempts()
		{
			// Two finite phases totalling 10; capped at 4
			var schedule = RetryDelaySchedule.Create()
				.WithConstant(TimeSpan.FromMilliseconds(100), attempts: 6)
				.WithConstant(TimeSpan.FromSeconds(1), attempts: 4)
				.WithMaxTotalAttempts(4)
				.Build();

			Assert.That(schedule.Count(), Is.EqualTo(4));
		}

		// -------------------------------------------------------------------------
		// Custom phase
		// -------------------------------------------------------------------------

		[Test]
		public void WithCustom_Should_Emit_Provider_Values()
		{
			var schedule = RetryDelaySchedule.Create()
				.WithCustom(attempt => TimeSpan.FromSeconds(attempt + 1), attempts: 3)
				.Build();

			var results = schedule.ToList();

			Assert.That(results[0], Is.EqualTo(TimeSpan.FromSeconds(1)));
			Assert.That(results[1], Is.EqualTo(TimeSpan.FromSeconds(2)));
			Assert.That(results[2], Is.EqualTo(TimeSpan.FromSeconds(3)));
		}

		// -------------------------------------------------------------------------
		// WithDelay — pass existing RetryDelay instance
		// -------------------------------------------------------------------------

		[Test]
		public void WithDelay_Should_Use_Provided_RetryDelay_Instance()
		{
			var retryDelay = ExponentialRetryDelay.Create(TimeSpan.FromMilliseconds(100));

			var schedule = RetryDelaySchedule.Create()
				.WithDelay(retryDelay, attempts: 3)
				.Build();

			var results = schedule.ToList();

			Assert.That(results.Count, Is.EqualTo(3));
			// Exponential without jitter: 100ms * 2^0=100, 2^1=200, 2^2=400
			Assert.That(results[0], Is.EqualTo(TimeSpan.FromMilliseconds(100)));
			Assert.That(results[1], Is.EqualTo(TimeSpan.FromMilliseconds(200)));
			Assert.That(results[2], Is.EqualTo(TimeSpan.FromMilliseconds(400)));
		}

		// -------------------------------------------------------------------------
		// Periodic and TimeSeries phases
		// -------------------------------------------------------------------------

		[Test]
		public void PeriodicPhase_Should_Cycle_Values()
		{
			var times = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3) };

			var schedule = RetryDelaySchedule.Create()
				.WithPeriodic(times, attempts: 6)
				.Build();

			var results = schedule.ToList();

			// Should cycle: 1s,3s,1s,3s,1s,3s
			for (int i = 0; i < 6; i++)
				Assert.That(results[i], Is.EqualTo(times[i % 2]));
		}

		[Test]
		public void TimeSeriesPhase_Should_Repeat_Last_Value()
		{
			var times = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5) };

			var schedule = RetryDelaySchedule.Create()
				.WithTimeSeries(times, attempts: 5)
				.Build();

			var results = schedule.ToList();

			Assert.That(results[0], Is.EqualTo(TimeSpan.FromSeconds(1)));
			Assert.That(results[1], Is.EqualTo(TimeSpan.FromSeconds(5)));
			// Repeats last
			Assert.That(results[2], Is.EqualTo(TimeSpan.FromSeconds(5)));
			Assert.That(results[3], Is.EqualTo(TimeSpan.FromSeconds(5)));
			Assert.That(results[4], Is.EqualTo(TimeSpan.FromSeconds(5)));
		}

		// -------------------------------------------------------------------------
		// Reusability — each enumeration is independent
		// -------------------------------------------------------------------------

		[Test]
		public void Enumeration_Should_Be_Independent_And_Repeatable()
		{
			var schedule = RetryDelaySchedule.Create()
				.WithConstant(TimeSpan.FromMilliseconds(100), attempts: 3)
				.Build();

			var first = schedule.ToList();
			var second = schedule.ToList();

			Assert.That(first, Is.EqualTo(second));
		}

		[Test]
		public void Multiple_Enumerations_Should_Start_From_First_Phase()
		{
			var fast = TimeSpan.FromMilliseconds(50);
			var slow = TimeSpan.FromSeconds(1);

			var schedule = RetryDelaySchedule.Create()
				.WithConstant(fast, attempts: 2)
				.WithConstant(slow, attempts: 2)
				.Build();

			// Both enumerations must start with the fast delay
			var first = schedule.First();
			var second = schedule.First();

			Assert.That(first, Is.EqualTo(fast));
			Assert.That(second, Is.EqualTo(fast));
		}

		// -------------------------------------------------------------------------
		// Edge cases
		// -------------------------------------------------------------------------

		[Test]
		public void SinglePhase_With_OneAttempt_Should_Emit_One_Value()
		{
			var delay = TimeSpan.FromSeconds(5);

			var schedule = RetryDelaySchedule.Create()
				.WithConstant(delay, attempts: 1)
				.Build();

			var results = schedule.ToList();

			Assert.That(results.Count, Is.EqualTo(1));
			Assert.That(results[0], Is.EqualTo(delay));
		}

		[Test]
		public void MaxTotalAttempts_Smaller_Than_First_Phase_Should_Truncate_First_Phase()
		{
			var schedule = RetryDelaySchedule.Create()
				.WithConstant(TimeSpan.FromMilliseconds(100), attempts: 10)
				.WithConstant(TimeSpan.FromSeconds(1), attempts: 5)
				.WithMaxTotalAttempts(3)
				.Build();

			Assert.That(schedule.Count(), Is.EqualTo(3));
			Assert.That(schedule.All(d => d == TimeSpan.FromMilliseconds(100)), Is.True);
		}
	}
}
