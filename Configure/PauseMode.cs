namespace Configure
{
	/// <summary>
	/// Describes when the application pauses execution.
	/// </summary>
    enum PauseMode
    {
		/// <summary>
		/// The application does not pause.
		/// </summary>
		False = 0,
		No = 0,

		/// <summary>
		/// The application pauses at end.
		/// </summary>
		True = 1,
		Yes = 1,

		/// <summary>
		/// The application pauses on error.
		/// </summary>
		Error = 2
    }
}
