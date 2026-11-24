namespace RetryDelays.Tests
{
	internal class StandardJitterTests
	{
		[Test]
		public void Should_Return_Exact_Minimum_When_RandomValue_Is_Zero()
		{
			var result = StandardJitter.AddJitter(1000, 10, 0);

			var expected = 1000 * (1.0 - 0.10);
			Assert.That(result, Is.EqualTo(expected));
		}

		[Test]
		public void Should_Return_Exact_Maximum_When_RandomValue_Is_One()
		{
			var result = StandardJitter.AddJitter(1000, 10, 1);

			var expected = 1000 * (1.0 + 0.10);
			Assert.That(result, Is.EqualTo(expected));
		}

		[Test]
		public void Should_Return_BaseDelay_When_RandomValue_Is_Half()
		{
			var result = StandardJitter.AddJitter(1000, 10, 0.5);

			Assert.That(result, Is.EqualTo(1000));
		}

		[Test]
		public void Should_Return_BaseDelay_When_JitterPercent_Is_Zero()
		{
			Assert.That(StandardJitter.AddJitter(1000, 0, 0), Is.EqualTo(1000));
			Assert.That(StandardJitter.AddJitter(1000, 0, 1), Is.EqualTo(1000));
		}

		[Test]
		public void Should_Return_Double_Or_Zero_When_JitterPercent_Is_Hundred()
		{
			Assert.That(StandardJitter.AddJitter(1000, 100, 1), Is.EqualTo(2000));
			Assert.That(StandardJitter.AddJitter(1000, 100, 0), Is.EqualTo(0));
		}

		[Test]
		public void Should_Return_Exact_Minimum_With_DefaultJitterPercent_When_RandomValue_Is_Zero()
		{
			var fraction = RetryDelayConstants.DefaultJitterPercent / 100.0;

			var result = StandardJitter.AddJitter(1000, RetryDelayConstants.DefaultJitterPercent, 0);

			Assert.That(result, Is.EqualTo(1000 * (1.0 - fraction)));
		}

		[Test]
		public void Should_Return_Exact_Maximum_With_DefaultJitterPercent_When_RandomValue_Is_One()
		{
			var fraction = RetryDelayConstants.DefaultJitterPercent / 100.0;

			var result = StandardJitter.AddJitter(1000, RetryDelayConstants.DefaultJitterPercent, 1);

			Assert.That(result, Is.EqualTo(1000 * (1.0 + fraction)));
		}

		[Test]
		public void Should_Return_BaseDelay_With_DefaultJitterPercent_When_RandomValue_Is_Half()
		{
			var result = StandardJitter.AddJitter(1000, RetryDelayConstants.DefaultJitterPercent, 0.5);

			Assert.That(result, Is.EqualTo(1000));
		}

		[Test]
		public void Should_Throw_When_JitterPercent_Is_Negative()
		{
			Assert.That(() => StandardJitter.AddJitter(1000, -1, 0.5),
				Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
		}

		[Test]
		public void Should_Throw_When_JitterPercent_Is_Greater_Than_100()
		{
			Assert.That(() => StandardJitter.AddJitter(1000, 101, 0.5),
				Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
		}

		[Test]
		public void Should_Throw_When_RandomValue_Is_Negative()
		{
			Assert.That(() => StandardJitter.AddJitter(1000, 10, -0.001),
				Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
		}

		[Test]
		public void Should_Throw_When_RandomValue_Is_Greater_Than_One()
		{
			Assert.That(() => StandardJitter.AddJitter(1000, 10, 1.001),
				Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
		}
	}
}