namespace RetryDelays.Tests
{
	[TestFixture]
	public class LinearRetryDelayTests
	{
		[Test]
		public void Should_CalculateLinearDelayWithoutJitter_WhenCreatedWithBaseDelay()
		{
			// Arrange
			var baseDelay = TimeSpan.FromMilliseconds(100);
			var linearDelay = LinearRetryDelay.Create(baseDelay);

			// Act & Assert
			Assert.That(linearDelay.GetDelay(0), Is.EqualTo(TimeSpan.FromMilliseconds(100))); // (0+1) * 1.0 * 100 = 100ms
			Assert.That(linearDelay.GetDelay(1), Is.EqualTo(TimeSpan.FromMilliseconds(200))); // (1+1) * 1.0 * 100 = 200ms
			Assert.That(linearDelay.GetDelay(2), Is.EqualTo(TimeSpan.FromMilliseconds(300))); // (2+1) * 1.0 * 100 = 300ms
			Assert.That(linearDelay.GetDelay(3), Is.EqualTo(TimeSpan.FromMilliseconds(400))); // (3+1) * 1.0 * 100 = 400ms
		}

		[Test]
		public void Should_CalculateLinearDelayWithCustomSlopeFactor_WhenCreatedWithSlopeFactor()
		{
			// Arrange
			var baseDelay = TimeSpan.FromMilliseconds(100);
			var slopeFactor = 2.0;
			var linearDelay = LinearRetryDelay.Create(baseDelay, slopeFactor);

			// Act & Assert
			Assert.That(linearDelay.GetDelay(0), Is.EqualTo(TimeSpan.FromMilliseconds(200))); // (0+1) * 2.0 * 100 = 200ms
			Assert.That(linearDelay.GetDelay(1), Is.EqualTo(TimeSpan.FromMilliseconds(400))); // (1+1) * 2.0 * 100 = 400ms
			Assert.That(linearDelay.GetDelay(2), Is.EqualTo(TimeSpan.FromMilliseconds(600))); // (2+1) * 2.0 * 100 = 600ms
		}

		[Test]
		public void Should_RespectMaxDelay_WhenMaxDelayIsSpecified()
		{
			// Arrange
			var baseDelay = TimeSpan.FromMilliseconds(100);
			var maxDelay = TimeSpan.FromMilliseconds(250);
			var linearDelay = LinearRetryDelay.Create(baseDelay, maxDelay);

			// Act & Assert
			Assert.That(linearDelay.GetDelay(0), Is.EqualTo(TimeSpan.FromMilliseconds(100))); // 100ms < 250ms
			Assert.That(linearDelay.GetDelay(1), Is.EqualTo(TimeSpan.FromMilliseconds(200))); // 200ms < 250ms
			Assert.That(linearDelay.GetDelay(2), Is.EqualTo(TimeSpan.FromMilliseconds(250))); // 300ms capped to 250ms
			Assert.That(linearDelay.GetDelay(3), Is.EqualTo(TimeSpan.FromMilliseconds(250))); // 400ms capped to 250ms
		}

		[Test]
		public void Should_UseTimeSpanMaxValue_WhenMaxDelayIsNotSpecified()
		{
			// Arrange
			var baseDelay = TimeSpan.FromMilliseconds(100);
			var linearDelay = LinearRetryDelay.Create(baseDelay);

			// Act & Assert - Test a large attempt that would exceed reasonable delays
			var largeAttempt = 1000000;
			var result = linearDelay.GetDelay(largeAttempt);

			// The delay should be capped at some reasonable maximum (implementation detail of MaxDelayDelimiter)
			Assert.That(result, Is.LessThan(TimeSpan.MaxValue));
		}

		[Test]
		public void Should_ApplyJitterWhenEnabled_WithoutExceedingMaxDelay()
		{
			// Arrange
			var baseDelay = TimeSpan.FromMilliseconds(100);
			var maxDelay = TimeSpan.FromMilliseconds(1000);
			var linearDelay = LinearRetryDelay.Create(baseDelay, maxDelay, useJitter: true);

			// Act - Get delay multiple times for the same attempt
			var delay1 = linearDelay.GetDelay(1);
			var delay2 = linearDelay.GetDelay(1);
			var delay3 = linearDelay.GetDelay(1);

			// Assert - Due to jitter, delays should vary but stay within reasonable bounds
			Assert.That(delay1, Is.LessThanOrEqualTo(maxDelay));
			Assert.That(delay2, Is.LessThanOrEqualTo(maxDelay));
			Assert.That(delay3, Is.LessThanOrEqualTo(maxDelay));

			// At least one should be different due to jitter (very high probability)
			Assert.That(delay1 == delay2 && delay2 == delay3, Is.False);
		}

		[Test]
		public void Should_CreateUsingOptions_WhenConstructorIsUsedWithOptions()
		{
			// Arrange
			var options = new LinearRetryDelayOptions
			{
				BaseDelay = TimeSpan.FromMilliseconds(150),
				SlopeFactor = 1.5,
				MaxDelay = TimeSpan.FromMilliseconds(500),
				UseJitter = false
			};

			// Act
			var linearDelay = new LinearRetryDelay(options);

			// Assert
			Assert.That(linearDelay.GetDelay(0), Is.EqualTo(TimeSpan.FromMilliseconds(225))); // (0+1) * 1.5 * 150 = 225ms
			Assert.That(linearDelay.GetDelay(1), Is.EqualTo(TimeSpan.FromMilliseconds(450))); // (1+1) * 1.5 * 150 = 450ms
			Assert.That(linearDelay.GetDelay(2), Is.EqualTo(TimeSpan.FromMilliseconds(500))); // (2+1) * 1.5 * 150 = 675ms capped to 500ms
		}

		[Test]
		public void Should_ReturnCorrectDelayType_WhenCheckingOptionsDelayType()
		{
			// Arrange
			var options = new LinearRetryDelayOptions();

			// Act & Assert
			Assert.That(options.DelayType, Is.EqualTo(RetryDelayType.Linear));
		}

		[Test]
		public void Should_HandleZeroAttempt_WhenCalculatingDelay()
		{
			// Arrange
			var baseDelay = TimeSpan.FromMilliseconds(50);
			var linearDelay = LinearRetryDelay.Create(baseDelay);

			// Act
			var result = linearDelay.GetDelay(0);

			// Assert
			Assert.That(result, Is.EqualTo(TimeSpan.FromMilliseconds(50))); // (0+1) * 1.0 * 50 = 50ms
		}

		[Test]
		public void Should_Apply_Jitter_Within_Expected_Range()
		{
			var baseDelay = TimeSpan.FromMilliseconds(100);
			var options = new LinearRetryDelayOptions
			{
				BaseDelay = baseDelay,
				UseJitter = true,
				SlopeFactor = 1.0
			};
			var delay = new LinearRetryDelay(options);

			var result = delay.GetDelay(1);
			var baseValue = 200.0; // 2 * 100 * 1.0
			var jitterRange = baseValue * RetryDelayConstants.JitterFactor / 2;
			var minExpected = TimeSpan.FromMilliseconds(baseValue - jitterRange);
			var maxExpected = TimeSpan.FromMilliseconds(baseValue + jitterRange);

			Assert.That(result, Is.GreaterThanOrEqualTo(minExpected));
			Assert.That(result, Is.LessThanOrEqualTo(maxExpected));
		}

		[Test]
		public void Should_Cap_Delay_At_MaxDelay_When_Exceeded()
		{
			var baseDelay = TimeSpan.FromMilliseconds(100);
			var maxDelay = TimeSpan.FromMilliseconds(250);
			var options = new LinearRetryDelayOptions
			{
				BaseDelay = baseDelay,
				MaxDelay = maxDelay,
				UseJitter = false,
				SlopeFactor = 1.0
			};
			var delay = new LinearRetryDelay(options);

			var result = delay.GetDelay(2); // Should be 300ms without cap

			Assert.That(result, Is.EqualTo(maxDelay));
		}

		[Test]
		public void Should_Create_Instance_With_Create_Method_Default_Slope()
		{
			var baseDelay = TimeSpan.FromMilliseconds(100);
			var instance = LinearRetryDelay.Create(baseDelay);

			var result = instance.GetDelay(1);

			Assert.That(result, Is.EqualTo(TimeSpan.FromMilliseconds(200)));
		}

		[Test]
		public void Should_Create_Instance_With_Create_Method_Custom_Slope()
		{
			var baseDelay = TimeSpan.FromMilliseconds(100);
			var instance = LinearRetryDelay.Create(baseDelay, 2.0);

			var result = instance.GetDelay(1);

			Assert.That(result, Is.EqualTo(TimeSpan.FromMilliseconds(400)));
		}

		[Test]
		public void Should_Respect_MaxDelay_With_Jitter_Applied()
		{
			var baseDelay = TimeSpan.FromMilliseconds(100);
			var maxDelay = TimeSpan.FromMilliseconds(250);
			var options = new LinearRetryDelayOptions
			{
				BaseDelay = baseDelay,
				MaxDelay = maxDelay,
				UseJitter = true,
				SlopeFactor = 1.0
			};
			var delay = new LinearRetryDelay(options);

			var result = delay.GetDelay(2); // Base value would be 300ms

			Assert.That(result, Is.LessThanOrEqualTo(maxDelay));
		}

		[Test]
		public void Should_Handle_Zero_BaseDelay()
		{
			var baseDelay = TimeSpan.Zero;
			var options = new LinearRetryDelayOptions
			{
				BaseDelay = baseDelay,
				UseJitter = false,
				SlopeFactor = 1.0
			};
			var delay = new LinearRetryDelay(options);

			var result = delay.GetDelay(5);

			Assert.That(result, Is.EqualTo(TimeSpan.Zero));
		}
	}
}
