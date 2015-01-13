using System;

namespace obsidianUpdater.Actions
{
	public class Players : ProgramAction
	{
		public Players(ProgramAction parent) : base("players", parent)
		{
			AddSubAction(new Add(this));
			AddSubAction(new Remove(this));
			AddSubAction(new Invite(this));
			AddSubAction(new Modify(this));
		}

		public override string GetShortHelp()
		{
			return base.GetShortHelp() + " <player> [...]";
		}

		public class Add : ProgramAction
		{
			public Add(ProgramAction parent) : base("add", parent)
			{
			}
		}

		public class Remove : ProgramAction
		{
			public Remove(ProgramAction parent) : base("remove", parent)
			{
			}
		}

		public class Invite : ProgramAction
		{
			public Invite(ProgramAction parent) : base("invite", parent)
			{
			}
		}

		public class Modify : ProgramAction
		{
			public Modify(ProgramAction parent) : base("modify", parent)
			{
			}
		}
	}
}

