using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace obsidianUpdater
{
	[DataContract]
	public class Mod
	{
		[DataMember(Order = 0, IsRequired = true)]
		public string Name { get; set; }
		[DataMember(Order = 1, EmitDefaultValue = false)]
		public string ShortName { get; set; }
		[DataMember(Order = 2, IsRequired = true)]
		public string Version { get; set; }
		[DataMember(Order = 3, EmitDefaultValue = false)]
		public ModType Type { get; set; }
		[DataMember(Order = 4, EmitDefaultValue = false)]
		public bool Disabled { get; set; }

		[DataMember(Order = 10, EmitDefaultValue = false)]
		public string Website { get; set; }
		[DataMember(Order = 11, EmitDefaultValue = false)]
		public string GitHub { get; set; }
		[DataMember(Order = 12, EmitDefaultValue = false)]
		public string Downloads { get; set; }
		[DataMember(Order = 13, EmitDefaultValue = false)]
		public string Builds { get; set; }
		[DataMember(Order = 14, EmitDefaultValue = false)]
		public string Forum { get; set; }
		[DataMember(Order = 15, EmitDefaultValue = false)]
		public string Curse { get; set; }

		[DataMember(Order = 20, EmitDefaultValue = false)]
		public NullableCollection<string> Notes { get; set; }
		[DataMember(Order = 21, EmitDefaultValue = false)]
		public NullableCollection<string> Authors { get; set; }
		[DataMember(Order = 22, EmitDefaultValue = false)]
		public NullableCollection<string> Requires { get; set; }
		[DataMember(Order = 23, EmitDefaultValue = false)]
		public NullableCollection<string> RequiredFor { get; set; }
		[DataMember(Order = 24, EmitDefaultValue = false)]
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

