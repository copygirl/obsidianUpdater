using System;
using System.Collections.Generic;
using System.Linq;

namespace obsidianUpdater
{
	public class Program : ProgramAction
	{
		public static void Main(string[] arguments)
		{
			var data = Data.Load("obsidian.json");
			data.Save();

			//Stack<string> args = new Stack<string>(arguments.Reverse());
			//new Program().Handle(args);
		}

		private Program() : base("obsidian", null)
		{
			AddSubAction(new Setup(this));
			AddSubAction(new Server(this));
			AddSubAction(new Players(this));
			AddSubAction(new Mods(this));
			AddSubAction(new Help(this));

			Help = new string[] {
				"Handles various actions for the obsidian Minecraft server,",
				"like restarting the server, updating mods and the website."
			};
		}

		public override void Handle(Stack<string> args)
		{
			if (args.Count > 0) {
				try {
					base.Handle(args);
				} catch (InvalidUsageException e) {
					Console.WriteLine("Error: {0}", e.Message);
					Console.WriteLine("Usage: {0}", e.Action.GetUsingHelp());
					Console.WriteLine(e.Action.IsRoot
						? "Try '{0} help' for more information."
						: "Try '{0} help {1}' for more information.", Name, e.Action.FullNameWithoutRoot);
				} catch (Exception e) {
					Console.WriteLine("Error: {0}", e.Message);
					Console.WriteLine("Try '{0} help' for more information.", Name);
				}
			} else {
				Console.WriteLine("Usage: " + GetUsingHelp());
				foreach (var s in Help)
					Console.WriteLine("  " + s);
				Console.WriteLine("Try '{0} help' for more information.", Name);
			}
		}

		public override string GetUsingHelp()
		{
			return (Name + " <action> [...]");
		}
	}
}

