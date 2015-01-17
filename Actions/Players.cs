using System;
using System.Collections.Generic;
using System.Linq;
using obsidianUpdater.Data;
using obsidianUpdater.Monitor;
using System.Text.RegularExpressions;

namespace obsidianUpdater.Actions
{
	public class Players : ProgramAction
	{
		private static readonly string regexBase = @"^\[..:..:..\] \[Server thread/INFO\]: {0} \w+ {1}$";

		public static readonly Regex WhitelistAddSuccess = new Regex(String.Format(regexBase, "Added", "to the whitelist"));
		public static readonly Regex WhitelistAddFailure = new Regex(String.Format(regexBase, "Could not add", "to the whitelist"));

		public static readonly Regex WhitelistRemoveSuccess = new Regex(String.Format(regexBase, "Removed", "from the whitelist"));
		public static readonly Regex WhitelistRemoveFailure = new Regex(String.Format(regexBase, "Could not remove", "from the whitelist"));

		public Players(ProgramAction parent) : base("players", parent)
		{
			AddSubAction(new Add(this));
			AddSubAction(new Remove(this));
			AddSubAction(new Modify(this));
			AddSubAction(new Whitelist(this));
		}

		public override string GetShortHelp()
		{
			return base.GetShortHelp() + " <player> [...]";
		}


		private static Player GetPlayer(ProgramData data, string name)
		{
			return data.Players.AsEnumerable().FirstOrDefault(
				p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
		}

		private static CommandResult TrySendCommand(string command, Regex success, Regex failure)
		{
			using (var monitor = new MonitorClient()) {
				if (monitor.ConnectAsync().Result) {
					monitor.WaitForStatus();
					if (monitor.Status < ServerStatus.Running) {
						Console.WriteLine("Server starting up, please wait ...");
						monitor.WaitForStatus(ServerStatus.Running);
					}
					if (monitor.Status == ServerStatus.Running) {
						monitor.ReceiveOutput();
						monitor.Command(command);

						string output;
						while ((output = monitor.ReadOutput()) != null) {
							if (success.IsMatch(output))
								return CommandResult.Success;
							else if (failure.IsMatch(output))
								return CommandResult.Failure;
						}
					}
					return CommandResult.UnexpectedError;
				} else
					return CommandResult.ServerNotRunning;
			}
		}
		private enum CommandResult
		{
			Success,
			Failure,
			UnexpectedError,
			ServerNotRunning
		}

		private static void TryWhitelist(string player)
		{
			switch (TrySendCommand("whitelist add " + player,
			                       WhitelistAddSuccess, WhitelistAddFailure)) {
				case CommandResult.Success:
					Console.WriteLine("Player '{0}' was added to the whitelist.", player);
					break;
				case CommandResult.Failure:
					Console.WriteLine("Warning: Couldn't whitelist player '{0}'. (Wrong name, already whitelisted?)", player);
					break;
				case CommandResult.UnexpectedError:
					Console.WriteLine("Warning: Unexpected server error, player '{0}' wasn't whitelisted.", player);
					break;
				case CommandResult.ServerNotRunning:
					Console.WriteLine("Warning: Server is not running, player '{0}' wasn't whitelisted.", player);
					break;
			}
		}
		private static void TryUnWhitelist(string player)
		{
			switch (TrySendCommand("whitelist remove " + player,
			                       WhitelistRemoveSuccess, WhitelistRemoveFailure)) {
				case CommandResult.Success:
					Console.WriteLine("Player '{0}' was removed from the whitelist.", player);
					break;
				case CommandResult.Failure:
					Console.WriteLine("Warning: Couldn't un-whitelist player '{0}'. (Not whitelisted?)", player);
					break;
				case CommandResult.UnexpectedError:
					Console.WriteLine("Warning: Unexpected server error, player '{0}' wasn't un-whitelisted.", player);
					break;
				case CommandResult.ServerNotRunning:
					Console.WriteLine("Warning: Server not running, player '{0}' wasn't un-whitelisted.", player);
					break;
			}
		}


		public class Add : ProgramAction {
			
			private ActionParameter _ign     = new ActionParameter("ingame-name", ParameterType.Value,   alias: "ign", help: "Sets the in-game name");
			private ActionParameter _invited = new ActionParameter("invited",     ParameterType.Enabled, alias: "i",   help: "Set if only invited, not whitelisted");

			private ActionParameter _website = new ActionParameter("website", ParameterType.Value, help: "Sets the website URL");
			private ActionParameter _twitter = new ActionParameter("twitter", ParameterType.Value, help: "Sets the Twitter name");
			private ActionParameter _github  = new ActionParameter("github",  ParameterType.Value, help: "Sets the GitHub profile");
			private ActionParameter _twitch  = new ActionParameter("twitch",  ParameterType.Value, help: "Sets the Twitch profile");
			private ActionParameter _youtube = new ActionParameter("youtube", ParameterType.Value, help: "Sets the YouTube channel");
			private ActionParameter _donate  = new ActionParameter("donate",  ParameterType.Value, help: "Sets the donation URL");
			private ActionParameter _patreon = new ActionParameter("patreon", ParameterType.Value, help: "Sets the Patreon page");

			private ActionParameter _notes   = new ActionParameter("notes", ParameterType.Value, help: "Sets the notes");
			private ActionParameter _mods    = new ActionParameter("mods",  ParameterType.Value, help: "Sets the authored mods");

			public Add(ProgramAction parent) : base("add", parent)
			{
				Help = new string[]{ "Adds a new player, possibly whitlisting them" };

				AddParameters(
					_ign, _invited, _website, _twitter, _github,
					_twitch, _youtube, _donate, _patreon, _notes, _mods);
			}

			public override string GetShortHelp()
			{
				return base.GetShortHelp() + " <player> [parameters]";
			}

			public override void Handle(Stack<string> args)
			{
				if (args.Count < 1)
					throw new InvalidUsageException(this, "Expected player name.");

				var name = args.Pop();
				HandleParameters(args);

				var data = ProgramData.Load();
				if (GetPlayer(data, name) != null)
					throw new InvalidOperationException(String.Format("Player '{0}' already exists.", name));

				data.Players += new Player {
					Name = name,
					InGameName = _ign.Value,
					Invited = _invited.IsSet,
					Website = _website.Value,
					Twitter = _twitter.Value,
					GitHub = _github.Value,
					Twitch = _twitch.Value,
					YouTube = _youtube.Value,
					Donate = _donate.Value,
					Notes = NullableCollection<string>.FromString(_notes.Value),
					Mods = NullableCollection<string>.FromString(_mods.Value),
				};
				data.Save();

				if (!_invited.IsSet)
					TryWhitelist(_ign.Value ?? name);
			}
		}

		public class Remove : ProgramAction
		{
			public Remove(ProgramAction parent) : base("remove", parent)
			{
				Help = new string[]{ "Removes a player, possibly un-whitlisting them" };
			}

			public override string GetShortHelp()
			{
				return base.GetShortHelp() + " <player>";
			}

			public override void Handle(Stack<string> args)
			{
				if (args.Count < 1)
					throw new InvalidUsageException(this, "Expected player name.");
				if (args.Count > 1)
					throw new InvalidUsageException(this, "Too many arguments.");

				var name = args.Pop();

				var data = ProgramData.Load();
				var player = GetPlayer(data, name);
				if (player == null)
					throw new InvalidOperationException(String.Format("Player '{0}' doesn't exists.", name));

				data.Players -= player;
				data.Save();

				if (!player.Invited)
					TryUnWhitelist(player.InGameName ?? player.Name);
			}
		}

		public class Modify : ProgramAction
		{
			private ActionParameter _website = new ActionParameter("website", ParameterType.Value, help: "Sets the website URL");
			private ActionParameter _twitter = new ActionParameter("twitter", ParameterType.Value, help: "Sets the Twitter name");
			private ActionParameter _github  = new ActionParameter("github",  ParameterType.Value, help: "Sets the GitHub profile");
			private ActionParameter _twitch  = new ActionParameter("twitch",  ParameterType.Value, help: "Sets the Twitch profile");
			private ActionParameter _youtube = new ActionParameter("youtube", ParameterType.Value, help: "Sets the YouTube channel");
			private ActionParameter _donate  = new ActionParameter("donate",  ParameterType.Value, help: "Sets the donation URL");
			private ActionParameter _patreon = new ActionParameter("patreon", ParameterType.Value, help: "Sets the Patreon page");

			private ActionParameter _notes  = new ActionParameter("notes",  ParameterType.Value, help: "Sets the notes");
			private ActionParameter _mods   = new ActionParameter("mods",   ParameterType.Value, help: "Sets the authored mods");
			private ActionParameter _remove = new ActionParameter("remove", ParameterType.Value, help: "Removes some fields");

			public Modify(ProgramAction parent) : base("modify", parent)
			{
				Help = new string[]{ "Modifies or removes a player's data" };

				AddParameters(
					_website, _twitter, _github, _twitch, _youtube,
					_donate, _patreon, _notes, _mods, _remove);
			}

			public override string GetShortHelp()
			{
				return base.GetShortHelp() + " <player> [parameters]";
			}

			public override void Handle(Stack<string> args)
			{
				if (args.Count < 1)
					throw new InvalidUsageException(this, "Expected player name.");
				if (args.Count < 2)
					throw new InvalidUsageException(this, "Expected parameter.");

				var name = args.Pop();
				HandleParameters(args);

				var data = ProgramData.Load();
				var player = GetPlayer(data, name);
				if (player == null)
					throw new InvalidOperationException(String.Format("Player '{0}' doesn't exists.", name));

				if (_remove.HasValue) {
					var parameters = Parameters.Where(param => (param != _remove));
					foreach (var rem in _remove.Value.Split(',')) {
						var parameter = parameters.FirstOrDefault(param => param.Name.Equals(rem, StringComparison.InvariantCultureIgnoreCase));
						if (parameter == null)
							throw new InvalidUsageException(this, String.Format("Invalid field '{0}'.", rem));
						if (parameter.IsSet)
							throw new InvalidUsageException(this, String.Format("Can't both set and remove field '{0}'.", parameter.Name));
						parameter.Handle("");
					}
				}

				player.Website = SetOrUnset(player.Website, _website);
				player.Twitter = SetOrUnset(player.Twitter, _twitter);
				player.GitHub  = SetOrUnset(player.GitHub,  _github);
				player.Twitch  = SetOrUnset(player.Twitch,  _twitch);
				player.YouTube = SetOrUnset(player.YouTube, _youtube);
				player.Donate  = SetOrUnset(player.Donate,  _donate);
				player.Patreon = SetOrUnset(player.Patreon, _patreon);
				player.Notes = SetOrUnset(player.Notes, _notes);
				player.Mods  = SetOrUnset(player.Mods,  _mods);

				data.Save();
			}

			private static string SetOrUnset(string current, ActionParameter parameter)
			{
				return (parameter.IsSet ? (parameter.HasValue ? parameter.Value : null) : current);
			}
			private static NullableCollection<string> SetOrUnset(NullableCollection<string> current, ActionParameter parameter)
			{
				return (parameter.IsSet ? (parameter.HasValue ? NullableCollection<string>.FromString(parameter.Value) : null) : current);
			}
		}

		public class Whitelist : ProgramAction
		{
			private ActionParameter _ign = new ActionParameter("ingame-name", ParameterType.Value, alias: "ign", help: "Sets the in-game name");
			private ActionParameter _remove = new ActionParameter("remove", ParameterType.Enabled, help: "Removes instead of adds");

			public Whitelist(ProgramAction parent) : base("whitelist", parent)
			{
				Help = new string[]{ "Whitelists or un-whitelists a player" };

				AddParameters(_ign, _remove);
			}

			public override string GetShortHelp()
			{
				return base.GetShortHelp() + " <player> [parameters]";
			}

			public override void Handle(Stack<string> args)
			{
				if (args.Count < 1)
					throw new InvalidUsageException(this, "Expected player name.");

				var name = args.Pop();
				HandleParameters(args);

				var data = ProgramData.Load();
				var player = GetPlayer(data, name);
				if (player == null)
					throw new InvalidOperationException(String.Format("Player '{0}' doesn't exists.", name));

				if (_remove.IsSet) {
					if (_ign.IsSet)
						throw new InvalidOperationException("Can't use -ingame-name parameter together with -remove.");
					if (player.Invited)
						throw new InvalidOperationException(String.Format("Player '{0}' isn't whitelisted.", player.Name));

					TryUnWhitelist(player.InGameName ?? player.Name);
				} else {
					if (!player.Invited)
						throw new InvalidOperationException(String.Format("Player '{0}' is already whitelisted.", player.Name));

					if (_ign.IsSet)
						player.InGameName = _ign.Value;
					TryWhitelist(player.InGameName ?? player.Name);
				}

				player.Invited = !player.Invited;
				data.Save();
			}
		}
	}
}

