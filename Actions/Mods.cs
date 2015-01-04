using System;

namespace obsidianUpdater
{
	public class Mods : ProgramAction
	{
		public Mods(ProgramAction parent) : base("mods", parent)
		{
			AddSubAction(new Add(this));
			AddSubAction(new Remove(this));
			AddSubAction(new Update(this));
			AddSubAction(new Modify(this));
			AddSubAction(new Enable(this));
			AddSubAction(new Disable(this));
		}

		public override string GetShortHelp()
		{
			return base.GetShortHelp() + " <mod> [...]";
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

		public class Update : ProgramAction
		{
			public Update(ProgramAction parent) : base("update", parent)
			{
			}
		}

		public class Modify : ProgramAction
		{
			public Modify(ProgramAction parent) : base("modify", parent)
			{
			}
		}

		public class Enable : ProgramAction
		{
			public Enable(ProgramAction parent) : base("enable", parent)
			{
			}
		}

		public class Disable : ProgramAction
		{
			public Disable(ProgramAction parent) : base("disable", parent)
			{
			}
		}
	}
}

