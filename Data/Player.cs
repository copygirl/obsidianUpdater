using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace obsidianUpdater
{
	[DataContract]
	public class Player
	{
		[DataMember(Order = 0, IsRequired = true)]
		public string Name { get; set; }
		[DataMember(Order = 1, EmitDefaultValue = false)]
		public string InGameName { get; set; }
		[DataMember(Order = 2, EmitDefaultValue = false)]
		public bool Invited { get; set; }

		[DataMember(Order = 10, EmitDefaultValue = false)]
		public string Website { get; set; }
		[DataMember(Order = 11, EmitDefaultValue = false)]
		public string Twitter { get; set; }
		[DataMember(Order = 12, EmitDefaultValue = false)]
		public string GitHub { get; set; }
		[DataMember(Order = 13, EmitDefaultValue = false)]
		public string Twitch { get; set; }
		[DataMember(Order = 14, EmitDefaultValue = false)]
		public string YouTube { get; set; }
		[DataMember(Order = 15, EmitDefaultValue = false)]
		public string Donate { get; set; }
		[DataMember(Order = 16, EmitDefaultValue = false)]
		public string Patreon { get; set; }

		[DataMember(Order = 20, EmitDefaultValue = false)]
		public NullableCollection<string> Notes { get; set; }
		[DataMember(Order = 21, EmitDefaultValue = false)]
		public NullableCollection<string> Mods { get; set; }


		public override int GetHashCode()
		{
			return Name.ToLowerInvariant().GetHashCode();
		}

		public override bool Equals(object obj)
		{
			Player player = obj as Player;
			return ((player != null) && (Name.Equals(player.Name, StringComparison.InvariantCultureIgnoreCase)));
		}
	}
}

