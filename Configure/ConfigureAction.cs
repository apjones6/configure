namespace Configure
{
	public class ConfigureAction
	{
		public ConfigureActionType Action { get; set; }
		public string AppSetting { get; set; }
		public string Path { get; set; }
		public string Value { get; set; }
	}

	public enum ConfigureActionType
	{
		Update,
		Create,
		Remove
	}
}
