using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace obsidianUpdater
{
	[DataContract]
	public class Player
	{
		[DataMember(Order = 0, Name = "name", IsRequired = true)]
		public string Name { get; set; }
		[DataMember(Order = 1, Name = "ign", EmitDefaultValue = false)]
		public string InGameName { get; set; }
		[DataMember(Order = 2, Name = "invited", EmitDefaultValue = false)]
		public bool Invited { get; set; }

		[DataMember(Order = 10, Name = "website", EmitDefaultValue = false)]
		public string Website { get; set; }
		[DataMember(Order = 11, Name = "twitter", EmitDefaultValue = false)]
		public string Twitter { get; set; }
		[DataMember(Order = 12, Name = "github", EmitDefaultValue = false)]
		public string GitHub { get; set; }
		[DataMember(Order = 13, Name = "twitch", EmitDefaultValue = false)]
		public string Twitch { get; set; }
		[DataMember(Order = 14, Name = "youtube", EmitDefaultValue = false)]
		public string YouTube { get; set; }
		[DataMember(Order = 15, Name = "donate", EmitDefaultValue = false)]
		public string Donate { get; set; }
		[DataMember(Order = 16, Name = "patreon", EmitDefaultValue = false)]
		public string Patreon { get; set; }

		[DataMember(Order = 20, Name = "notes", EmitDefaultValue = false)]
		public NullableCollection<string> Notes { get; set; }
		[DataMember(Order = 21, Name = "mods", EmitDefaultValue = false)]
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

