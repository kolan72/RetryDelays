using System;

namespace RetryDelays
{
	/// <summary>
	/// The base class to get the delay value before the next attempt.
	/// </summary>
	public class RetryDelay
	{
		private Func<int, TimeSpan> DelayValueProvider { get; }

		internal RetryDelay(DelayCoreBase delayCore) : this(delayCore.DelayProvider) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="RetryDelay"/> class with a custom delay provider function.
		/// </summary>
		/// <param name="delayValueProvider">A function that calculates the delay duration for a given retry number.</param>
		public RetryDelay(Func<int, TimeSpan> delayValueProvider)
		{
			DelayValueProvider = delayValueProvider ?? throw new ArgumentNullException(nameof(delayValueProvider));
		}

		public static implicit operator RetryDelay(Func<int, TimeSpan> delayValueProvider) => new RetryDelay(delayValueProvider);

		/// <summary>
		/// Gets the delay for the specified retry attempt.
		/// </summary>
		/// <param name="attempt">
		/// The zero-based retry attempt number. The first retry attempt is 0, the second is 1, and so on.
		/// </param>
		/// <returns>The delay associated with the specified retry attempt.</returns>
		public TimeSpan GetDelay(int attempt)
		{
			return DelayValueProvider(attempt);
		}
	}
}
