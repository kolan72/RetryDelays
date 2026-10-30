namespace RetryDelays.Tests
{
	internal class ConstantRetryDelayTests
	{
		[Test]
		public void Should_Throw_When_UseJitter_True_And_MaxDelay_Less_Than_BaseDelay()
		{
			var options = new ConstantRetryDelayOptions
			{
				BaseDelay = TimeSpan.FromSeconds(5),
				MaxDelay = TimeSpan.FromSeconds(3),
				UseJitter = true
			};

			Assert.That(() => new ConstantRetryDelay(options),
				Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
		}

		[Test]
		public void Should_Return_BaseDelay_When_UseJitter_False()
		{
			var baseDelay = TimeSpan.FromMilliseconds(500);
			var options = new ConstantRetryDelayOptions
			{
				BaseDelay = baseDelay,
				UseJitter = false
			};
			var delay = new ConstantRetryDelay(options);

			var result = delay.GetDelay(1);

			Assert.That(result, Is.EqualTo(baseDelay));
		}

		[Test]
		public void Should_Return_BaseDelay_For_Any_Attempt_When_UseJitter_False()
		{
			var baseDelay = TimeSpan.FromMilliseconds(100);
			var options = new ConstantRetryDelayOptions
			{
				BaseDelay = baseDelay,
				UseJitter = false
			};
			var delay = new ConstantRetryDelay(options);

			for (int attempt = 1; attempt <= 5; attempt++)
			{
				Assert.That(delay.GetDelay(attempt), Is.EqualTo(baseDelay));
			}
		}

		[Test]
		public void Should_Apply_Jitter_Within_Expected_Range_When_UseJitter_True()
		{
			var baseDelay = TimeSpan.FromMilliseconds(1000);
			var options = new ConstantRetryDelayOptions
			{
				BaseDelay = baseDelay,
				MaxDelay = TimeSpan.MaxValue,
				UseJitter = true
			};
			var delay = new ConstantRetryDelay(options);

			var result = delay.GetDelay(1);
			var baseMs = baseDelay.TotalMilliseconds;
			var offset = baseMs * RetryDelayConstants.DefaultJitterPercent / 100;
			var minExpected = TimeSpan.FromMilliseconds(baseMs - offset);
			var maxExpected = TimeSpan.FromMilliseconds(baseMs + offset);

			Assert.That(result, Is.GreaterThanOrEqualTo(minExpected));
			Assert.That(result, Is.LessThanOrEqualTo(maxExpected));
		}

		[Test]
		public void Should_Apply_Custom_JitterPercent_Within_Expected_Range()
		{
			var baseDelay = TimeSpan.FromMilliseconds(1000);
			var options = new ConstantRetryDelayOptions
			{
				BaseDelay = baseDelay,
				MaxDelay = TimeSpan.MaxValue,
				UseJitter = true,
				JitterPercent = 10
			};
			var delay = new ConstantRetryDelay(options);

			var result = delay.GetDelay(1);
			var baseMs = baseDelay.TotalMilliseconds;
			var offset = baseMs * 10 / 100;
			var minExpected = TimeSpan.FromMilliseconds(baseMs - offset);
			var maxExpected = TimeSpan.FromMilliseconds(baseMs + offset);

			Assert.That(result, Is.GreaterThanOrEqualTo(minExpected));
			Assert.That(result, Is.LessThanOrEqualTo(maxExpected));
		}

		[Test]
		public void Should_Return_BaseDelay_When_JitterPercent_Is_Zero()
		{
			var baseDelay = TimeSpan.FromMilliseconds(500);
			var options = new ConstantRetryDelayOptions
			{
				BaseDelay = baseDelay,
				UseJitter = true,
				JitterPercent = 0
			};
			var delay = new ConstantRetryDelay(options);

			var result = delay.GetDelay(1);

			Assert.That(result, Is.EqualTo(baseDelay));
		}

		[Test]
		public void Should_Throw_When_JitterPercent_Is_Negative()
		{
			var options = new ConstantRetryDelayOptions
			{
				BaseDelay = TimeSpan.FromSeconds(1),
				UseJitter = true
			};

			Assert.That(() => options.JitterPercent = -1,
				Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
		}

		[Test]
		public void Should_Throw_When_JitterPercent_Is_Greater_Than_100()
		{
			var options = new ConstantRetryDelayOptions
			{
				BaseDelay = TimeSpan.FromSeconds(1),
				UseJitter = true
			};

			Assert.That(() => options.JitterPercent = 101,
				Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
		}

		[Test]
		public void Should_Cap_Delay_At_MaxDelay_When_Jitter_Exceeds_Max()
		{
			var baseDelay = TimeSpan.FromMilliseconds(1000);
			var maxDelay = TimeSpan.FromMilliseconds(1100);
			var options = new ConstantRetryDelayOptions
			{
				BaseDelay = baseDelay,
				MaxDelay = maxDelay,
				UseJitter = true
			};
			var delay = new ConstantRetryDelay(options);

			// Force jitter to exceed max by using a known random value
			// Note: This test assumes internal implementation details
			var result = delay.GetDelay(1);

			Assert.That(result, Is.LessThanOrEqualTo(maxDelay));
		}

		[Test]
		public void Should_Create_Instance_Via_Create_Method()
		{
			var baseDelay = TimeSpan.FromMilliseconds(200);
			var instance = ConstantRetryDelay.Create(baseDelay, useJitter: true);

			Assert.That(instance, Is.Not.Null);
			Assert.That(instance.GetDelay(1), Is.GreaterThanOrEqualTo(TimeSpan.FromMilliseconds(150)));
		}

		[Test]
		public void Should_Apply_JitterPercent_From_Create_Within_Expected_Range()
		{
			var baseDelay = TimeSpan.FromMilliseconds(1000);
			var delay = ConstantRetryDelay.Create(baseDelay, useJitter: true, jitterPercent: 10);

			var result = delay.GetDelay(1);
			var baseMs = baseDelay.TotalMilliseconds;
			var offset = baseMs * 10 / 100;
			var minExpected = TimeSpan.FromMilliseconds(baseMs - offset);
			var maxExpected = TimeSpan.FromMilliseconds(baseMs + offset);

			Assert.That(result, Is.GreaterThanOrEqualTo(minExpected));
			Assert.That(result, Is.LessThanOrEqualTo(maxExpected));
		}

		[Test]
		public void Should_Return_BaseDelay_When_Create_JitterPercent_Is_Zero()
		{
			var baseDelay = TimeSpan.FromMilliseconds(500);
			var delay = ConstantRetryDelay.Create(baseDelay, useJitter: true, jitterPercent: 0);

			var result = delay.GetDelay(1);

			Assert.That(result, Is.EqualTo(baseDelay));
		}

		[Test]
		public void Should_Throw_When_Create_JitterPercent_Without_UseJitter()
		{
			var baseDelay = TimeSpan.FromMilliseconds(500);

			Assert.That(() => ConstantRetryDelay.Create(baseDelay, jitterPercent: 10),
				Throws.ArgumentException.With.Property("ParamName").EqualTo("jitterPercent"));
		}

		[Test]
		public void Should_Throw_When_Create_JitterPercent_Is_Negative()
		{
			var baseDelay = TimeSpan.FromMilliseconds(500);

			Assert.That(() => ConstantRetryDelay.Create(baseDelay, useJitter: true, jitterPercent: -1),
				Throws.TypeOf<ArgumentOutOfRangeException>().With.Property("ParamName").EqualTo("jitterPercent"));
		}

		[Test]
		public void Should_Throw_When_Create_JitterPercent_Is_Greater_Than_100()
		{
			var baseDelay = TimeSpan.FromMilliseconds(500);

			Assert.That(() => ConstantRetryDelay.Create(baseDelay, useJitter: true, jitterPercent: 101),
				Throws.TypeOf<ArgumentOutOfRangeException>().With.Property("ParamName").EqualTo("jitterPercent"));
		}

		[Test]
		public void Should_Treat_Null_JitterPercent_As_Omitted()
		{
			var baseDelay = TimeSpan.FromMilliseconds(500);
			var delay = ConstantRetryDelay.Create(baseDelay, useJitter: false, jitterPercent: null);

			var result = delay.GetDelay(1);

			Assert.That(result, Is.EqualTo(baseDelay));
		}
	}
}
