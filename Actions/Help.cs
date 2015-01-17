using System;
using System.Collections.Generic;

namespace obsidianUpdater.Actions
{
	public class Help : ProgramAction
	{
		public Help(ProgramAction parent) : base("help", parent)
		{
			Help = new string[]{ "Displays this message or information on an action." };
			Aliases = new string[]{ "-help", "--help", "-?" };
		}

		public override void Handle(Stack<string> args)
		{
			ProgramAction action = Parent;
			while ((args.Count > 0) && action.HasSubActions) {
				string name = args.Pop().ToLowerInvariant();
				string fullName = ((action != Parent) ? (action.FullNameWithoutRoot + " " + name) : name);
				action = action[name];
				if ((action == null) || action.IsHidden)
					throw new ArgumentException(String.Format("No help available for '{0}'.", fullName));
			}
			if (action == this)
				throw new StackOverflowException();

			Console.WriteLine("Usage: " + action.GetUsingHelp());

			if ((action.Help == null) && !action.HasSubActions)
				throw new ArgumentException(String.Format("No help available for '{0}'.", action.FullNameWithoutRoot));

			if (action.Help != null)
				foreach (var s in action.Help)
					Console.WriteLine("  " + s);

			if (action.HasSubActions)
				Console.WriteLine();
			foreach (var a in action.SubActions)
				if (!a.IsHidden)
					Console.WriteLine("> " + a.GetLongHelp());

			if (action.HasParameters)
				Console.WriteLine();
			foreach (var p in action.Parameters)
				if (!p.IsHidden) {
					string line = ActionParameter.PREFIX + p.Name;
					if (p.Alias != null)
						line += "," + ActionParameter.PREFIX + p.Alias;
					if (p.Help != null)
						line += ": " + p.Help;
					Console.WriteLine(line);
				}

			if (action.HasSubActions) {
				Console.WriteLine();
				Console.WriteLine("Try '{0} help{1} <action>' for more information.",
					Parent.FullName, ((action != Parent) ? (" " + action.FullNameWithoutRoot) : ""));
			}
		}

		public override string GetShortHelp()
		{
			return base.GetShortHelp() + " [action]";
		}
	}
}

