namespace RetryDelays.Tests
{
	internal class RetryDelayTests
	{
		[Test]
		public void Should_Implicitly_Convert_ConstantRetryDelayOptions_To_RetryDelay()
		{
			var crdo = new ConstantRetryDelayOptions() { BaseDelay = TimeSpan.FromMilliseconds(1) };
			var tester = new RetryDelayTester();
			Assert.That(tester.GetAttemptDelay(crdo), Is.EqualTo(TimeSpan.FromMilliseconds(1)));
		}

		[Test]
		public void Should_Implicitly_Convert_LinearRetryDelayOptions_To_RetryDelay()
		{
			var crdo = new LinearRetryDelayOptions() { BaseDelay = TimeSpan.FromMilliseconds(2), SlopeFactor = 2 };
			var tester = new RetryDelayTester();
			Assert.That(tester.GetAttemptDelay(crdo), Is.EqualTo(TimeSpan.FromMilliseconds(2 * crdo.SlopeFactor)));
		}

		[Test]
		public void Should_Implicitly_Convert_ExponentialRetryDelayOptions_To_RetryDelay()
		{
			var crdo = new ExponentialRetryDelayOptions() { BaseDelay = TimeSpan.FromMilliseconds(2), ExponentialFactor = 2 };
			var tester = new RetryDelayTester();
			Assert.That(tester.GetAttemptDelay(crdo), Is.EqualTo(TimeSpan.FromMilliseconds(2 * Math.Pow(crdo.ExponentialFactor, 0))));
		}

		[Test]
		public void Should_Implicitly_Convert_TimeSeriesRetryDelayOptions_To_RetryDelay()
		{
			var crdo = new TimeSeriesRetryDelayOptions() { BaseDelay = TimeSpan.FromMilliseconds(2), Times = [TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(2)] };
			var tester = new RetryDelayTester();
			Assert.That(tester.GetAttemptDelay(crdo), Is.EqualTo(TimeSpan.FromMilliseconds(1)));
		}

		[Test]
		public void Should_CustomRetryDelay_Be_Created_From_Func()
		{
			var customDelay = new RetryDelay(attempt =>
							TimeSpan.FromSeconds(Math.Min((attempt + 1) * 2, 60)));
			var tester = new RetryDelayTester();
			Assert.That(tester.GetAttemptDelay(customDelay), Is.EqualTo(TimeSpan.FromSeconds(2)));
		}

		private class RetryDelayTester
		{
			public TimeSpan GetAttemptDelay(RetryDelay retryDelay, int attemptNumber = 0)
			{
				return retryDelay.GetDelay(attemptNumber);
			}
		}
	}
}
