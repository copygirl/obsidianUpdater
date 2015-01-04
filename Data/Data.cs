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
		private static readonly DataContractJsonSerializer _serializer =
			new DataContractJsonSerializer(typeof(Data));
		
		private string _file;

		[DataMember(Order = 0, Name = "mods", EmitDefaultValue = false)]
		public NullableCollection<Mod> Mods { get; set; }
		[DataMember(Order = 1, Name = "players", EmitDefaultValue = false)]
		public NullableCollection<Player> Players { get; set; }

		public static Data Load(string file)
		{
			Data data;
			if (File.Exists(file))
				using (var stream = File.OpenRead(file))
					data = (Data)_serializer.ReadObject(stream);
			else
				data = new Data();
			data._file = file;
			return data;
		}

		public void Save()
		{
			using (var writer = JsonReaderWriterFactory.CreateJsonWriter(
				                    File.Open(_file, FileMode.Create), Encoding.UTF8, true, true)) {
				_serializer.WriteStartObject(writer, this);
				_serializer.WriteObjectContent(writer, this);
				_serializer.WriteEndObject(writer);
			}
		}
	}
}

