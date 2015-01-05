using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace obsidianUpdater
{
	[DataContract]
	public class Data
	{
		public static readonly string DATA_FILE = "obsidian.json";

		private static readonly DataContractJsonSerializer _serializer =
			new DataContractJsonSerializer(typeof(Data));

		[DataMember(Order = 0, Name = "screen", EmitDefaultValue = false)]
		public string ScreenName { get; set; }

		[DataMember(Order = 10, Name = "mods", EmitDefaultValue = false)]
		public NullableCollection<Mod> Mods { get; set; }
		[DataMember(Order = 11, Name = "players", EmitDefaultValue = false)]
		public NullableCollection<Player> Players { get; set; }

		public static Data Load()
		{
			Data data;
			if (File.Exists(DATA_FILE))
				while (true) {
					try {
						using (var stream = File.OpenRead(DATA_FILE))
							data = (Data)_serializer.ReadObject(stream);
						break;
					} catch (Exception e) {
						Console.WriteLine("There was a problem reading the data file '{0}'.", DATA_FILE);
						Console.WriteLine("Error: {0}", e.Message);
						if (Console.IsInputRedirected)
							Environment.Exit(1);
						Console.WriteLine("  Enter 'new' to create a new data file,");
						Console.Write("  anything else to retry or CTRL+C to abort: ");
						var action = Console.ReadLine();
						Console.WriteLine();
						if ("new".Equals(action, StringComparison.InvariantCultureIgnoreCase)) {
							data = new Data();
							data.Save();
							break;
						}
					}
				}
			else
				data = new Data();
			return data;
		}

		public void Save()
		{
			while (true) {
				try {
					using (var writer = JsonReaderWriterFactory.CreateJsonWriter(
						                   File.Open(DATA_FILE, FileMode.Create), Encoding.UTF8, true, true)) {
						_serializer.WriteStartObject(writer, this);
						_serializer.WriteObjectContent(writer, this);
						_serializer.WriteEndObject(writer);
					}
					break;
				} catch (Exception e) {
					Console.WriteLine("There was a problem writing the data file '{0}'.", DATA_FILE);
					Console.WriteLine("Error: {0}", e.Message);
					if (Console.IsInputRedirected)
						Environment.Exit(1);
					Console.WriteLine("  Press any key to retry or CTRL+C to abort.");
					var action = Console.ReadKey(false);
					Console.WriteLine();
				}
			}
		}
	}
}

