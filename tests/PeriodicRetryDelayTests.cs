using System;

namespace RetryDelays.Tests
{
	[TestFixture]
	public class PeriodicRetryDelayTests
	{
		[Test]
		public void Should_InitializeWithOptions_WhenValidOptionsProvided()
		{
			// Arrange
			var options = new PeriodicRetryDelayOptions
			{
				BaseDelay = TimeSpan.FromSeconds(1),
				MaxDelay = TimeSpan.FromSeconds(10),
				UseJitter = false,
				Times = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3) }
			};

			// Act
			var delay = new PeriodicRetryDelay(options);

			// Assert
			Assert.That(delay, Is.Not.Null);
		}

		[Test]
		public void Should_UseBaseDelayAsDefaultTime_WhenTimesArrayIsEmpty()
		{
			// Arrange
			var options = new PeriodicRetryDelayOptions
			{
				BaseDelay = TimeSpan.FromSeconds(2),
				MaxDelay = TimeSpan.FromSeconds(10),
				UseJitter = false,
				Times = Array.Empty<TimeSpan>()
			};

			// Act
			var delay = new PeriodicRetryDelay(options);
			var result = delay.GetDelay(0);

			// Assert
			Assert.That(result, Is.EqualTo(TimeSpan.FromSeconds(2)));
		}

		[Test]
		public void Should_UseMaxDelayAsDefaultTime_WhenBaseDelayExceedsMaxDelay()
		{
			// Arrange
			var options = new PeriodicRetryDelayOptions
			{
				BaseDelay = TimeSpan.FromSeconds(15),
				MaxDelay = TimeSpan.FromSeconds(10),
				UseJitter = false,
				Times = new TimeSpan[0]
			};

			// Act
			var delay = new PeriodicRetryDelay(options);
			var result = delay.GetDelay(0);

			// Assert
			Assert.That(result, Is.EqualTo(TimeSpan.FromSeconds(10)));
		}

		[Test]
		public void Should_CycleThroughTimesArray_WhenAttemptExceedsArrayLength()
		{
			// Arrange
			var times = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3) };
			var options = new PeriodicRetryDelayOptions
			{
				BaseDelay = TimeSpan.FromSeconds(1),
				MaxDelay = TimeSpan.FromSeconds(10),
				UseJitter = false,
				Times = times
			};
			var delay = new PeriodicRetryDelay(options);

			// Act & Assert - First cycle
			Assert.That(delay.GetDelay(0), Is.EqualTo(TimeSpan.FromSeconds(1)));
			Assert.That(delay.GetDelay(1), Is.EqualTo(TimeSpan.FromSeconds(2)));
			Assert.That(delay.GetDelay(2), Is.EqualTo(TimeSpan.FromSeconds(3)));
			
			// Second cycle - should wrap around
			Assert.That(delay.GetDelay(3), Is.EqualTo(TimeSpan.FromSeconds(1)));
			Assert.That(delay.GetDelay(4), Is.EqualTo(TimeSpan.FromSeconds(2)));
			Assert.That(delay.GetDelay(5), Is.EqualTo(TimeSpan.FromSeconds(3)));
			
			// Third cycle
			Assert.That(delay.GetDelay(6), Is.EqualTo(TimeSpan.FromSeconds(1)));
		}

		[Test]
		public void Should_ApplyMaxDelayLimit_WhenDelayExceedsMaxDelay()
		{
			// Arrange
			var times = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(3) };
			var options = new PeriodicRetryDelayOptions
			{
				BaseDelay = TimeSpan.FromSeconds(1),
				MaxDelay = TimeSpan.FromSeconds(10),
				UseJitter = false,
				Times = times
			};
			var delay = new PeriodicRetryDelay(options);

			// Act & Assert
			Assert.That(delay.GetDelay(0), Is.EqualTo(TimeSpan.FromSeconds(1)));
			Assert.That(delay.GetDelay(1), Is.EqualTo(TimeSpan.FromSeconds(10))); // 15s capped to 10s
			Assert.That(delay.GetDelay(2), Is.EqualTo(TimeSpan.FromSeconds(3)));
			Assert.That(delay.GetDelay(3), Is.EqualTo(TimeSpan.FromSeconds(1))); // Cycles back
		}

		[Test]
		public void Should_CreateWithSingleTime_WhenUsingCreateWithOneParameter()
		{
			// Arrange
			var firstTime = TimeSpan.FromSeconds(2);

			// Act
			var delay = PeriodicRetryDelay.Create(firstTime);

			// Assert - With single time, all attempts should return the same value
			Assert.That(delay.GetDelay(0), Is.EqualTo(firstTime));
			Assert.That(delay.GetDelay(1), Is.EqualTo(firstTime));
			Assert.That(delay.GetDelay(100), Is.EqualTo(firstTime));
		}

		[Test]
		public void Should_CreateWithTwoTimes_WhenUsingCreateWithTwoParameters()
		{
			// Arrange
			var firstTime = TimeSpan.FromSeconds(1);
			var secondTime = TimeSpan.FromSeconds(2);

			// Act
			var delay = PeriodicRetryDelay.Create(firstTime, secondTime);

			// Assert - Should cycle between the two values
			Assert.That(delay.GetDelay(0), Is.EqualTo(firstTime));
			Assert.That(delay.GetDelay(1), Is.EqualTo(secondTime));
			Assert.That(delay.GetDelay(2), Is.EqualTo(firstTime)); // Cycles back
			Assert.That(delay.GetDelay(3), Is.EqualTo(secondTime));
		}

		[Test]
		public void Should_CreateWithThreeTimes_WhenUsingCreateWithThreeParameters()
		{
			// Arrange
			var firstTime = TimeSpan.FromSeconds(1);
			var secondTime = TimeSpan.FromSeconds(2);
			var thirdTime = TimeSpan.FromSeconds(3);

			// Act
			var delay = PeriodicRetryDelay.Create(firstTime, secondTime, thirdTime);

			// Assert
			Assert.That(delay.GetDelay(0), Is.EqualTo(firstTime));
			Assert.That(delay.GetDelay(1), Is.EqualTo(secondTime));
			Assert.That(delay.GetDelay(2), Is.EqualTo(thirdTime));
			Assert.That(delay.GetDelay(3), Is.EqualTo(firstTime)); // Cycles back
		}

		[Test]
		public void Should_CreateWithFourTimes_WhenUsingCreateWithFourParameters()
		{
			// Arrange
			var firstTime = TimeSpan.FromSeconds(1);
			var secondTime = TimeSpan.FromSeconds(2);
			var thirdTime = TimeSpan.FromSeconds(3);
			var fourthTime = TimeSpan.FromSeconds(4);

			// Act
			var delay = PeriodicRetryDelay.Create(firstTime, secondTime, thirdTime, fourthTime);

			// Assert
			Assert.That(delay.GetDelay(0), Is.EqualTo(firstTime));
			Assert.That(delay.GetDelay(1), Is.EqualTo(secondTime));
			Assert.That(delay.GetDelay(2), Is.EqualTo(thirdTime));
			Assert.That(delay.GetDelay(3), Is.EqualTo(fourthTime));
			Assert.That(delay.GetDelay(4), Is.EqualTo(firstTime)); // Cycles back
		}

		[Test]
		public void Should_CreateWithEnumerableTimes_WhenUsingCreateWithEnumerable()
		{
			// Arrange
			var times = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3) };

			// Act
			var delay = PeriodicRetryDelay.Create(times);

			// Assert
			Assert.That(delay.GetDelay(0), Is.EqualTo(TimeSpan.FromSeconds(1)));
			Assert.That(delay.GetDelay(1), Is.EqualTo(TimeSpan.FromSeconds(2)));
			Assert.That(delay.GetDelay(2), Is.EqualTo(TimeSpan.FromSeconds(3)));
			Assert.That(delay.GetDelay(3), Is.EqualTo(TimeSpan.FromSeconds(1))); // Cycles back
			Assert.That(delay.GetDelay(6), Is.EqualTo(TimeSpan.FromSeconds(1))); // Another full cycle
		}

		[Test]
		public void Should_ThrowArgumentNullException_WhenCreateCalledWithNullTimes()
		{
			// Act & Assert
			Assert.That(() => PeriodicRetryDelay.Create(null),
				Throws.ArgumentNullException.With.Property("ParamName").EqualTo("times"));
		}

		[Test]
		public void Should_ApplyMaxDelayInCreateMethods_WhenMaxDelaySpecified()
		{
			// Arrange
			TimeSpan? maxDelay = TimeSpan.FromSeconds(2);

			// Act
			var delay = PeriodicRetryDelay.Create(TimeSpan.FromSeconds(5), maxDelay);
			var result = delay.GetDelay(0);

			// Assert
			Assert.That(result, Is.EqualTo(maxDelay));
		}

		[Test]
		public void Should_UseJitteredValues_WhenJitterEnabled()
		{
			// Arrange
			var times = new[] { TimeSpan.FromSeconds(1) };
			var options = new PeriodicRetryDelayOptions
			{
				BaseDelay = TimeSpan.FromSeconds(1),
				MaxDelay = TimeSpan.FromSeconds(10),
				UseJitter = true,
				Times = times
			};
			var delay = new PeriodicRetryDelay(options);

			// Act
			var results = Enumerable.Range(0, 10).Select(_ => delay.GetDelay(0)).ToArray();

			// Assert - With jitter, we should get some variation in results
			Assert.That(results.Distinct().Count(), Is.GreaterThan(1));
			Assert.That(results.ToList().TrueForAll(r => r.TotalMilliseconds >= 500 && r.TotalMilliseconds <= 1500), Is.True);
		}

		[Test]
		public void Should_UseNonJitteredValues_WhenJitterDisabled()
		{
			// Arrange
			var times = new[] { TimeSpan.FromSeconds(1) };
			var options = new PeriodicRetryDelayOptions
			{
				BaseDelay = TimeSpan.FromSeconds(1),
				MaxDelay = TimeSpan.FromSeconds(10),
				UseJitter = false,
				Times = times
			};
			var delay = new PeriodicRetryDelay(options);

			// Act
			var results = Enumerable.Range(0, 10).Select(_ => delay.GetDelay(0)).ToArray();

			// Assert - Without jitter, all results should be identical
			Assert.That(results.Distinct().Count(), Is.EqualTo(1));
			Assert.That(results[0], Is.EqualTo(TimeSpan.FromSeconds(1)));
		}

		[Test]
		public void Should_HandleLargeAttemptNumbers_WhenCycling()
		{
			// Arrange
			var delay = PeriodicRetryDelay.Create(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));

			// Act & Assert - Test with large attempt numbers
			Assert.That(delay.GetDelay(100), Is.EqualTo(TimeSpan.FromSeconds(1))); // 100 % 2 = 0
			Assert.That(delay.GetDelay(101), Is.EqualTo(TimeSpan.FromSeconds(2))); // 101 % 2 = 1
			Assert.That(delay.GetDelay(1000), Is.EqualTo(TimeSpan.FromSeconds(1))); // 1000 % 2 = 0
			Assert.That(delay.GetDelay(1001), Is.EqualTo(TimeSpan.FromSeconds(2))); // 1001 % 2 = 1
		}

		[Test]
		public void Should_HandleInternalConstructor_WhenCalledWithValidParameters()
		{
			// Arrange
			var baseDelay = TimeSpan.FromSeconds(1);
			var maxDelay = TimeSpan.FromSeconds(5);

			// Act
			var delay = new PeriodicRetryDelay(baseDelay, maxDelay, false);

			// Assert - With single value, it should always return the same value
			Assert.That(delay.GetDelay(0), Is.EqualTo(baseDelay));
			Assert.That(delay.GetDelay(1), Is.EqualTo(baseDelay));
			Assert.That(delay.GetDelay(10), Is.EqualTo(baseDelay));
		}

		[Test]
		public void Should_UseTimeSpanMaxValue_WhenMaxDelayIsNull()
		{
			// Arrange
			var baseDelay = TimeSpan.FromSeconds(1);

			// Act
			var delay = new PeriodicRetryDelay(baseDelay, null, false);

			// Assert
			Assert.That(delay.GetDelay(0), Is.EqualTo(baseDelay));
		}
	}

	[TestFixture]
	public class PeriodicRetryDelayOptionsTests
	{
		[Test]
		public void Should_HaveCorrectDelayType_WhenAccessed()
		{
			// Arrange
			var options = new PeriodicRetryDelayOptions();

			// Act
			var delayType = options.DelayType;

			// Assert
			Assert.That(delayType, Is.EqualTo(RetryDelayType.Periodic));
		}

		[Test]
		public void Should_InitializeWithEmptyTimesArray_WhenCreated()
		{
			// Arrange & Act
			var options = new PeriodicRetryDelayOptions();

			// Assert
			Assert.That(options.Times, Is.Not.Null);
			Assert.That(options.Times.Length, Is.EqualTo(0));
		}

		[Test]
		public void Should_AllowSettingTimes_WhenTimesPropertySet()
		{
			// Arrange
			var options = new PeriodicRetryDelayOptions();
			var times = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2) };

			// Act
			options.Times = times;

			// Assert
			Assert.That(options.Times, Is.EqualTo(times));
		}

		[Test]
		public void Should_ImplicitlyConvertToPeriodicRetryDelay()
		{
			// Arrange
			var options = new PeriodicRetryDelayOptions
			{
				Times = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2) }
			};

			// Act
			PeriodicRetryDelay delay = options;

			// Assert
			Assert.That(delay, Is.Not.Null);
			Assert.That(delay.GetDelay(0), Is.EqualTo(TimeSpan.FromSeconds(1)));
			Assert.That(delay.GetDelay(1), Is.EqualTo(TimeSpan.FromSeconds(2)));
			Assert.That(delay.GetDelay(2), Is.EqualTo(TimeSpan.FromSeconds(1))); // Cycles back
		}
	}
}


