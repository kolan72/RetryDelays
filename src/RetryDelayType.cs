namespace RetryDelays
{
	/// <summary>
	/// Represents the type of delay.
	/// </summary>
	public enum RetryDelayType
	{
		/// <summary>
		/// The constant delay type.
		/// </summary>
		/// <remarks>
		/// Constant delay for each attempt.
		/// </remarks>
		Constant,

		/// <summary>
		/// The linear delay type.
		/// </summary>
		/// <remarks>
		/// Generates delays in an linear manner.
		/// </remarks>
		Linear,

		/// <summary>
		/// The exponential backoff type.
		/// /// </summary>
		Exponential,

		/// <summary>
		/// The fixed time series delay type
		/// </summary>
		/// <remarks>
		/// Uses predefined delay sequence. When exhausted, repeats last value.
		/// Provides deterministic control over retry intervals.
		/// </remarks>
		/// <example>
		/// Series [0.5s, 2s, 5s] would repeat 5s after 3rd attempt
		/// </example>
		TimeSeries,

		/// <summary>
		/// The periodic delay type
		/// </summary>
		/// <remarks>
		/// Uses predefined delay sequence that cycles back to the first value after the last.
		/// Provides repeating pattern of retry intervals.
		/// </remarks>
		/// <example>
		/// Series [0.5s, 2s, 5s] would cycle: 0.5s, 2s, 5s, 0.5s, 2s, 5s, ...
		/// </example>
		Periodic
	}
}
