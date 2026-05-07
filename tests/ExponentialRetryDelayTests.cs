namespace RetryDelays.Tests
{
	public class ExponentialRetryDelayTests
	{
		[Test]
		public void Should_CreateInstanceWithOptions()
		{
			// Arrange
			var options = new ExponentialRetryDelayOptions
			{
				BaseDelay = TimeSpan.FromSeconds(1),
				ExponentialFactor = 2.0,
				MaxDelay = TimeSpan.FromMinutes(5),
				UseJitter = false
			};

			// Act
			var delay = new ExponentialRetryDelay(options);

			// Assert
			Assert.That(delay, Is.Not.Null);
		}

		[Test]
		public void Should_CreateWithStaticCreateMethod()
		{
			// Arrange & Act
			var delay = ExponentialRetryDelay.Create(TimeSpan.FromSeconds(1));

			// Assert
			Assert.That(delay, Is.Not.Null);
		}

		[Test]
		public void Should_CreateWithStaticCreateMethodWithAllParameters()
		{
			// Arrange & Act
			var delay = ExponentialRetryDelay.Create(
				TimeSpan.FromSeconds(1),
				2.5,
				TimeSpan.FromMinutes(10),
				true);

			// Assert
			Assert.That(delay, Is.Not.Null);
		}

		[Test]
		public void Should_CalculateExponentialDelayWithoutJitter()
		{
			// Arrange
			var baseDelay = TimeSpan.FromMilliseconds(100);
			var delay = ExponentialRetryDelay.Create(baseDelay, 2.0, TimeSpan.FromMinutes(5), false);

			// Act
			var firstDelay = delay.GetDelay(0);
			var secondDelay = delay.GetDelay(1);
			var thirdDelay = delay.GetDelay(2);

			// Assert
			Assert.That(firstDelay.TotalMilliseconds, Is.EqualTo(100)); // 2^0 * 100
			Assert.That(secondDelay.TotalMilliseconds, Is.EqualTo(200)); // 2^1 * 100
			Assert.That(thirdDelay.TotalMilliseconds, Is.EqualTo(400)); // 2^2 * 100
		}

		[Test]
		public void Should_RespectMaxDelayWithoutJitter()
		{
			// Arrange
			var baseDelay = TimeSpan.FromMilliseconds(100);
			var maxDelay = TimeSpan.FromMilliseconds(250);
			var delay = ExponentialRetryDelay.Create(baseDelay, 2.0, maxDelay, false);

			// Act
			var highAttemptDelay = delay.GetDelay(10);

			// Assert
			Assert.That(highAttemptDelay, Is.EqualTo(maxDelay));
		}

		[Test]
		public void Should_UseJitterWhenEnabled()
		{
			// Arrange
			var baseDelay = TimeSpan.FromMilliseconds(100);
			var delay = ExponentialRetryDelay.Create(baseDelay, 2.0, TimeSpan.FromMinutes(5), true);

			// Act
			var firstDelay = delay.GetDelay(0);
			var secondDelay = delay.GetDelay(1);

			// Assert - With jitter, delays should be different from exact exponential values
			Assert.That(firstDelay.TotalMilliseconds, Is.GreaterThan(0));
			Assert.That(secondDelay.TotalMilliseconds, Is.GreaterThan(0));
			// Note: Due to randomness in jitter, we can't assert exact values
		}

		[Test]
		public void Should_HandleMaxDelayTicksLimit()
		{
			// Arrange
			var options = new ExponentialRetryDelayOptions
			{
				BaseDelay = TimeSpan.FromSeconds(1),
				ExponentialFactor = 2.0,
				MaxDelay = TimeSpan.MaxValue,
				UseJitter = true
			};

			// Act & Assert - Should not throw
			Assert.DoesNotThrow(() => new ExponentialRetryDelay(options));
		}

		[Test]
		public void Should_UseDefaultExponentialFactorInInternalConstructor()
		{
			// Arrange & Act
			var delay = new ExponentialRetryDelay(TimeSpan.FromMilliseconds(100));

			// Assert
			var firstDelay = delay.GetDelay(0);
			var secondDelay = delay.GetDelay(1);

			Assert.That(firstDelay.TotalMilliseconds, Is.EqualTo(100));
			Assert.That(secondDelay.TotalMilliseconds, Is.EqualTo(200)); // Default factor is 2.0
		}

		[Test]
		public void Should_UseCustomExponentialFactorInInternalConstructor()
		{
			// Arrange & Act
			var delay = new ExponentialRetryDelay(TimeSpan.FromMilliseconds(100), 3.0);

			// Assert
			var firstDelay = delay.GetDelay(0);
			var secondDelay = delay.GetDelay(1);

			Assert.That(firstDelay.TotalMilliseconds, Is.EqualTo(100));
			Assert.That(secondDelay.TotalMilliseconds, Is.EqualTo(300)); // 3^1 * 100
		}
	}

	[TestFixture]
	public class DecorrelatedJitterTests
	{
		private DecorrelatedJitter _jitter;

		[SetUp]
		public void Setup()
		{
			_jitter = new DecorrelatedJitter(
				TimeSpan.FromMilliseconds(100),
				2.0,
				TimeSpan.FromMinutes(5));
		}

		[Test]
		public void Should_CreateDecorrelatedJitterInstance()
		{
			// Arrange & Act & Assert
			Assert.That(_jitter, Is.Not.Null);
		}

		[Test]
		public void Should_GeneratePositiveDelaysForAllAttempts()
		{
			// Act & Assert
			for (int i = 0; i < 10; i++)
			{
				var delay = _jitter.DecorrelatedJitterBackoffV2(i);
				Assert.That(delay.TotalMilliseconds, Is.GreaterThan(0));
			}
		}

		[Test]
		public void Should_RespectMaxDelayLimit()
		{
			// Arrange
			var maxDelay = TimeSpan.FromMilliseconds(500);
			var jitter = new DecorrelatedJitter(
				TimeSpan.FromMilliseconds(100),
				2.0,
				maxDelay);

			// Act
			var delay = jitter.DecorrelatedJitterBackoffV2(100); // Very high attempt

			// Assert
			Assert.That(delay, Is.LessThanOrEqualTo(maxDelay));
		}

		[Test]
		public void Should_HandleInfinityCase()
		{
			// Arrange
			var maxDelay = TimeSpan.FromSeconds(30);
			var jitter = new DecorrelatedJitter(
				TimeSpan.FromMilliseconds(100),
				2.0,
				maxDelay);

			// Act
			var delay = jitter.DecorrelatedJitterBackoffV2(1024); // This should trigger infinity case

			// Assert
			Assert.That(delay, Is.EqualTo(maxDelay));
		}

		[Test]
		public void Should_HandleMaxDelayTicksLimit()
		{
			// Arrange
			var jitter = new DecorrelatedJitter(
				TimeSpan.FromMilliseconds(100),
				2.0,
				TimeSpan.MaxValue);

			// Act & Assert - Should not throw
			Assert.DoesNotThrow(() => jitter.DecorrelatedJitterBackoffV2(0));
		}

		[Test]
		public void Should_ProduceVariedDelaysWithSameAttempt()
		{
			// Arrange
			var delays = new TimeSpan[10];

			// Act
			for (int i = 0; i < 10; i++)
			{
				delays[i] = _jitter.DecorrelatedJitterBackoffV2(1);
			}

			// Assert - Due to randomization, not all delays should be identical
			bool hasVariation = false;
			for (int i = 1; i < delays.Length; i++)
			{
				if (delays[i] != delays[0])
				{
					hasVariation = true;
					break;
				}
			}
			Assert.That(hasVariation, Is.True);
		}

		[Test]
		public void Should_GenerateReasonableDelayProgression()
		{
			// Arrange
			var baseDelay = TimeSpan.FromMilliseconds(100);
			var jitter = new DecorrelatedJitter(
				baseDelay,
				2.0,
				TimeSpan.FromMinutes(5));

			// Act
			var delay0 = jitter.DecorrelatedJitterBackoffV2(0);
			var delay1 = jitter.DecorrelatedJitterBackoffV2(1);
			var delay2 = jitter.DecorrelatedJitterBackoffV2(2);

			// Assert - Generally, delays should increase (though jitter may cause some variation)
			Assert.That(delay0.TotalMilliseconds, Is.GreaterThan(0));
			Assert.That(delay1.TotalMilliseconds, Is.GreaterThan(0));
			Assert.That(delay2.TotalMilliseconds, Is.GreaterThan(0));
		}

		[Test]
		public void Should_HandleZeroAttempt()
		{
			// Act
			var delay = _jitter.DecorrelatedJitterBackoffV2(0);

			// Assert
			Assert.That(delay.TotalMilliseconds, Is.GreaterThan(0));
		}
	}

	[TestFixture]
	public class ExponentialRetryDelayOptionsTests
	{
		[Test]
		public void Should_HaveCorrectDelayType()
		{
			// Arrange
			var options = new ExponentialRetryDelayOptions();

			// Act & Assert
			Assert.That(options.DelayType, Is.EqualTo(RetryDelayType.Exponential));
		}

		[Test]
		public void Should_HaveDefaultExponentialFactor()
		{
			// Arrange
			var options = new ExponentialRetryDelayOptions();

			// Act & Assert
			Assert.That(options.ExponentialFactor, Is.EqualTo(RetryDelayConstants.ExponentialFactor));
		}

		[Test]
		public void Should_AllowSettingExponentialFactor()
		{
			// Arrange
			var options = new ExponentialRetryDelayOptions();
			var customFactor = 3.5;

			// Act
			options.ExponentialFactor = customFactor;

			// Assert
			Assert.That(options.ExponentialFactor, Is.EqualTo(customFactor));
		}
	}
}
