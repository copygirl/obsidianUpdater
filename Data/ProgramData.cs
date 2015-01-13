using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using obsidianUpdater.Utility;

namespace obsidianUpdater.Data
{
	[DataContract]
	public class ProgramData
	{
		private static readonly DataContractJsonSerializer _serializer =
			new DataContractJsonSerializer(typeof(ProgramData));

		[DataMember(Order = 0, Name = "server", EmitDefaultValue = false)]
		public ServerData Server { get; set; }

		[DataMember(Order = 10, Name = "mods", EmitDefaultValue = false)]
		public NullableCollection<Mod> Mods { get; set; }
		[DataMember(Order = 11, Name = "players", EmitDefaultValue = false)]
		public NullableCollection<Player> Players { get; set; }

		public static ProgramData Load()
		{
			ProgramData data;
			if (File.Exists(Constants.DATA_FILE))
				while (true) {
					try {
						using (var stream = File.OpenRead(Constants.DATA_FILE))
							data = (ProgramData)_serializer.ReadObject(stream);
						break;
					} catch (Exception e) {
						Console.WriteLine("There was a problem reading the data file '{0}'.", Constants.DATA_FILE);
						Console.WriteLine("Error: {0}", e.Message);
						if (Console.IsInputRedirected)
							Environment.Exit(1);
						Console.WriteLine("  Enter 'new' to create a new data file,");
						Console.Write("  anything else to retry or CTRL+C to abort: ");
						var action = Console.ReadLine();
						Console.WriteLine();
						if ("new".Equals(action, StringComparison.InvariantCultureIgnoreCase)) {
							data = new ProgramData();
							data.Save();
							break;
						}
					}
				}
			else
				data = new ProgramData();
			return data;
		}

		public void Save()
		{
			while (true) {
				try {
					using (var stream = File.Open(Constants.DATA_FILE, FileMode.Create))
						_serializer.WriteObject(stream, this);
					break;
				} catch (Exception e) {
					Console.WriteLine("There was a problem writing the data file '{0}'.", Constants.DATA_FILE);
					Console.WriteLine("Error: {0}", e.Message);
					if (Console.IsInputRedirected)
						Environment.Exit(1);
					Console.WriteLine("  Press any key to retry or CTRL+C to abort.");
					var action = Console.ReadKey(false);
					Console.WriteLine();
				}
			}
		}

		[DataContract]
		public class ServerData
		{
			[DataMember(Order = 0, Name = "directory", IsRequired = true)]
			public string Directory { get; set; }
			[DataMember(Order = 1, Name = "jar-file", IsRequired = true)]
			public string JarFile { get; set; }
			[DataMember(Order = 2, Name = "java-arguments", IsRequired = true)]
			public string JavaArguments { get; set; }
			[DataMember(Order = 3, Name = "minecraft-arguments", IsRequired = true)]
			public string MinecraftArguments { get; set; }
			[DataMember(Order = 4, Name = "auto-restart", IsRequired = true)]
			public bool AutoRestart { get; set; }
		}
	}
}

