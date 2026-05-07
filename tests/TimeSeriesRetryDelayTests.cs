namespace RetryDelays.Tests
{
	[TestFixture]
	public class TimeSeriesRetryDelayTests
	{
		[Test]
		public void Should_InitializeWithOptions_WhenValidOptionsProvided()
		{
			// Arrange
			var options = new TimeSeriesRetryDelayOptions
			{
				BaseDelay = TimeSpan.FromSeconds(1),
				MaxDelay = TimeSpan.FromSeconds(10),
				UseJitter = false,
				Times = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3) }
			};

			// Act
			var delay = new TimeSeriesRetryDelay(options);

			// Assert
			Assert.That(delay, Is.Not.Null);
		}

		[Test]
		public void Should_UseBaseDelayAsDefaultTime_WhenTimesArrayIsEmpty()
		{
			// Arrange
			var options = new TimeSeriesRetryDelayOptions
			{
				BaseDelay = TimeSpan.FromSeconds(2),
				MaxDelay = TimeSpan.FromSeconds(10),
				UseJitter = false,
				Times = Array.Empty<TimeSpan>()
			};

			// Act
			var delay = new TimeSeriesRetryDelay(options);
			var result = delay.GetDelay(0);

			// Assert
			Assert.That(result, Is.EqualTo(TimeSpan.FromSeconds(2)));
		}

		[Test]
		public void Should_UseMaxDelayAsDefaultTime_WhenBaseDelayExceedsMaxDelay()
		{
			// Arrange
			var options = new TimeSeriesRetryDelayOptions
			{
				BaseDelay = TimeSpan.FromSeconds(15),
				MaxDelay = TimeSpan.FromSeconds(10),
				UseJitter = false,
				Times = new TimeSpan[0]
			};

			// Act
			var delay = new TimeSeriesRetryDelay(options);
			var result = delay.GetDelay(0);

			// Assert
			Assert.That(result, Is.EqualTo(TimeSpan.FromSeconds(10)));
		}

		[Test]
		public void Should_ReturnCorrectDelayForEachAttempt_WhenWithinTimesArrayBounds()
		{
			// Arrange
			var times = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3) };
			var options = new TimeSeriesRetryDelayOptions
			{
				BaseDelay = TimeSpan.FromSeconds(1),
				MaxDelay = TimeSpan.FromSeconds(10),
				UseJitter = false,
				Times = times
			};
			var delay = new TimeSeriesRetryDelay(options);

			// Act & Assert
			Assert.That(delay.GetDelay(0), Is.EqualTo(TimeSpan.FromSeconds(1)));
			Assert.That(delay.GetDelay(1), Is.EqualTo(TimeSpan.FromSeconds(2)));
			Assert.That(delay.GetDelay(2), Is.EqualTo(TimeSpan.FromSeconds(3)));
		}

		[Test]
		public void Should_ReturnLastElement_WhenAttemptExceedsTimesArrayLength()
		{
			// Arrange
			var times = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3) };
			var options = new TimeSeriesRetryDelayOptions
			{
				BaseDelay = TimeSpan.FromSeconds(1),
				MaxDelay = TimeSpan.FromSeconds(10),
				UseJitter = false,
				Times = times
			};
			var delay = new TimeSeriesRetryDelay(options);

			// Act & Assert
			Assert.That(delay.GetDelay(3), Is.EqualTo(TimeSpan.FromSeconds(3)));
			Assert.That(delay.GetDelay(4), Is.EqualTo(TimeSpan.FromSeconds(3)));
			Assert.That(delay.GetDelay(10), Is.EqualTo(TimeSpan.FromSeconds(3)));
		}

		[Test]
		public void Should_ApplyMaxDelayLimit_WhenDelayExceedsMaxDelay()
		{
			// Arrange
			var times = new[] { TimeSpan.FromSeconds(15) };
			var options = new TimeSeriesRetryDelayOptions
			{
				BaseDelay = TimeSpan.FromSeconds(1),
				MaxDelay = TimeSpan.FromSeconds(10),
				UseJitter = false,
				Times = times
			};
			var delay = new TimeSeriesRetryDelay(options);

			// Act
			var result = delay.GetDelay(0);

			// Assert
			Assert.That(result, Is.EqualTo(TimeSpan.FromSeconds(10)));
		}

		[Test]
		public void Should_CreateWithSingleTime_WhenUsingCreateWithOneParameter()
		{
			// Arrange
			var firstTime = TimeSpan.FromSeconds(2);

			// Act
			var delay = TimeSeriesRetryDelay.Create(firstTime);

			// Assert
			Assert.That(delay.GetDelay(0), Is.EqualTo(firstTime));
			Assert.That(delay.GetDelay(1), Is.EqualTo(firstTime));
		}

		[Test]
		public void Should_CreateWithTwoTimes_WhenUsingCreateWithTwoParameters()
		{
			// Arrange
			var firstTime = TimeSpan.FromSeconds(1);
			var secondTime = TimeSpan.FromSeconds(2);

			// Act
			var delay = TimeSeriesRetryDelay.Create(firstTime, secondTime);

			// Assert
			Assert.That(delay.GetDelay(0), Is.EqualTo(firstTime));
			Assert.That(delay.GetDelay(1), Is.EqualTo(secondTime));
			Assert.That(delay.GetDelay(2), Is.EqualTo(secondTime));
		}

		[Test]
		public void Should_CreateWithThreeTimes_WhenUsingCreateWithThreeParameters()
		{
			// Arrange
			var firstTime = TimeSpan.FromSeconds(1);
			var secondTime = TimeSpan.FromSeconds(2);
			var thirdTime = TimeSpan.FromSeconds(3);

			// Act
			var delay = TimeSeriesRetryDelay.Create(firstTime, secondTime, thirdTime);

			// Assert
			Assert.That(delay.GetDelay(0), Is.EqualTo(firstTime));
			Assert.That(delay.GetDelay(1), Is.EqualTo(secondTime));
			Assert.That(delay.GetDelay(2), Is.EqualTo(thirdTime));
			Assert.That(delay.GetDelay(3), Is.EqualTo(thirdTime));
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
			var delay = TimeSeriesRetryDelay.Create(firstTime, secondTime, thirdTime, fourthTime);

			// Assert
			Assert.That(delay.GetDelay(0), Is.EqualTo(firstTime));
			Assert.That(delay.GetDelay(1), Is.EqualTo(secondTime));
			Assert.That(delay.GetDelay(2), Is.EqualTo(thirdTime));
			Assert.That(delay.GetDelay(3), Is.EqualTo(fourthTime));
			Assert.That(delay.GetDelay(4), Is.EqualTo(fourthTime));
		}

		[Test]
		public void Should_CreateWithEnumerableTimes_WhenUsingCreateWithEnumerable()
		{
			// Arrange
			var times = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3) };

			// Act
			var delay = TimeSeriesRetryDelay.Create(times);

			// Assert
			Assert.That(delay.GetDelay(0), Is.EqualTo(TimeSpan.FromSeconds(1)));
			Assert.That(delay.GetDelay(1), Is.EqualTo(TimeSpan.FromSeconds(2)));
			Assert.That(delay.GetDelay(2), Is.EqualTo(TimeSpan.FromSeconds(3)));
			Assert.That(delay.GetDelay(3), Is.EqualTo(TimeSpan.FromSeconds(3)));
		}

		[Test]
		public void Should_ThrowArgumentNullException_WhenCreateCalledWithNullTimes()
		{
			// Act & Assert
			Assert.That(() => TimeSeriesRetryDelay.Create(null),
				Throws.ArgumentNullException.With.Property("ParamName").EqualTo("times"));
		}

		[Test]
		public void Should_ApplyMaxDelayInCreateMethods_WhenMaxDelaySpecified()
		{
			// Arrange
			TimeSpan? maxDelay = TimeSpan.FromSeconds(2);

			// Act
			var delay = TimeSeriesRetryDelay.Create(TimeSpan.FromSeconds(5), maxDelay);
			var result = delay.GetDelay(0);

			// Assert
			Assert.That(result, Is.EqualTo(maxDelay));
		}

		[Test]
		public void Should_UseJitteredValues_WhenJitterEnabled()
		{
			// Arrange
			var times = new[] { TimeSpan.FromSeconds(1) };
			var options = new TimeSeriesRetryDelayOptions
			{
				BaseDelay = TimeSpan.FromSeconds(1),
				MaxDelay = TimeSpan.FromSeconds(10),
				UseJitter = true,
				Times = times
			};
			var delay = new TimeSeriesRetryDelay(options);

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
			var options = new TimeSeriesRetryDelayOptions
			{
				BaseDelay = TimeSpan.FromSeconds(1),
				MaxDelay = TimeSpan.FromSeconds(10),
				UseJitter = false,
				Times = times
			};
			var delay = new TimeSeriesRetryDelay(options);

			// Act
			var results = Enumerable.Range(0, 10).Select(_ => delay.GetDelay(0)).ToArray();

			// Assert - Without jitter, all results should be identical
			Assert.That(results.Distinct().Count(), Is.EqualTo(1));
			Assert.That(results[0], Is.EqualTo(TimeSpan.FromSeconds(1)));
		}

		[Test]
		public void Should_HandleInternalConstructor_WhenCalledWithValidParameters()
		{
			// This test uses reflection to test the internal constructor
			// Arrange
			var baseDelay = TimeSpan.FromSeconds(1);
			var maxDelay = TimeSpan.FromSeconds(5);

			// Act
			var delay = new TimeSeriesRetryDelay(baseDelay, maxDelay, false);

			// Assert
			Assert.That(delay.GetDelay(0), Is.EqualTo(baseDelay));
			Assert.That(delay.GetDelay(1), Is.EqualTo(baseDelay));
		}

		[Test]
		public void Should_UseTimeSpanMaxValue_WhenMaxDelayIsNull()
		{
			// Arrange
			var baseDelay = TimeSpan.FromSeconds(1);

			// Act
			var delay = new TimeSeriesRetryDelay(baseDelay, null, false);

			// Assert
			Assert.That(delay.GetDelay(0), Is.EqualTo(baseDelay));
		}
	}

	[TestFixture]
	public class TimeSeriesRetryDelayOptionsTests
	{
		[Test]
		public void Should_HaveCorrectDelayType_WhenAccessed()
		{
			// Arrange
			var options = new TimeSeriesRetryDelayOptions();

			// Act
			var delayType = options.DelayType;

			// Assert
			Assert.That(delayType, Is.EqualTo(RetryDelayType.TimeSeries));
		}

		[Test]
		public void Should_InitializeWithEmptyTimesArray_WhenCreated()
		{
			// Arrange & Act
			var options = new TimeSeriesRetryDelayOptions();

			// Assert
			Assert.That(options.Times, Is.Not.Null);
			Assert.That(options.Times.Length, Is.EqualTo(0));
		}

		[Test]
		public void Should_AllowSettingTimes_WhenTimesPropertySet()
		{
			// Arrange
			var options = new TimeSeriesRetryDelayOptions();
			var times = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2) };

			// Act
			options.Times = times;

			// Assert
			Assert.That(options.Times, Is.EqualTo(times));
		}
	}
}
