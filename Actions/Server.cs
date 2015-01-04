using System;

namespace obsidianUpdater
{
	public class Server : ProgramAction
	{
		public Server(ProgramAction parent) : base("server", parent)
		{
			AddSubAction(new Start(this));
			AddSubAction(new Stop(this));
			AddSubAction(new Restart(this));
		}

		public class Start : ProgramAction
		{
			public Start(ProgramAction parent) : base("start", parent)
			{
				Help = new string[]{ "Starts the server, if it isn't running already." };
			}
		}

		public class Stop : ProgramAction
		{
			public Stop(ProgramAction parent) : base("stop", parent)
			{
				Help = new string[]{ "Stops the server, if it is currently running." };
			}
		}

		public class Restart : ProgramAction
		{
			public Restart(ProgramAction parent) : base("restart", parent)
			{
				Help = new string[]{ "Restarts the server, if it is currently running." };
			}
		}
	}
}

