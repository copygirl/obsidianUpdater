using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace obsidianUpdater
{
	[DataContract]
	public class Mod
	{
		[DataMember(Order = 0, Name = "name", IsRequired = true)]
		public string Name { get; set; }
		[DataMember(Order = 1, Name = "short", EmitDefaultValue = false)]
		public string ShortName { get; set; }
		[DataMember(Order = 2, Name = "version", IsRequired = true)]
		public string Version { get; set; }
		[DataMember(Order = 3, Name = "type", EmitDefaultValue = false)]
		public ModType Type { get; set; }
		[DataMember(Order = 4, Name = "disabled", EmitDefaultValue = false)]
		public bool Disabled { get; set; }

		[DataMember(Order = 10, Name = "website", EmitDefaultValue = false)]
		public string Website { get; set; }
		[DataMember(Order = 11, Name = "github", EmitDefaultValue = false)]
		public string GitHub { get; set; }
		[DataMember(Order = 12, Name = "downloads", EmitDefaultValue = false)]
		public string Downloads { get; set; }
		[DataMember(Order = 13, Name = "builds", EmitDefaultValue = false)]
		public string Builds { get; set; }
		[DataMember(Order = 14, Name = "mcf", EmitDefaultValue = false)]
		public string Forum { get; set; }
		[DataMember(Order = 15, Name = "curse", EmitDefaultValue = false)]
		public string Curse { get; set; }

		[DataMember(Order = 20, Name = "notes", EmitDefaultValue = false)]
		public NullableCollection<string> Notes { get; set; }
		[DataMember(Order = 21, Name = "authors", EmitDefaultValue = false)]
		public NullableCollection<string> Authors { get; set; }
		[DataMember(Order = 22, Name = "requires", EmitDefaultValue = false)]
		public NullableCollection<string> Requires { get; set; }
		[DataMember(Order = 23, Name = "required", EmitDefaultValue = false)]
		public NullableCollection<string> RequiredFor { get; set; }
		[DataMember(Order = 24, Name = "supports", EmitDefaultValue = false)]
		public NullableCollection<string> Supports { get; set; }


		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			Mod mod = obj as Mod;
			return ((mod != null) && (Name.Equals(mod.Name, StringComparison.InvariantCultureIgnoreCase)));
		}
	}

	public enum ModType
	{
		Normal,
		Important,
		Server,
		Core,
		Api
	}
}

