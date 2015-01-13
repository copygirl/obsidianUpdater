using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using obsidianUpdater.Data;
using obsidianUpdater.Utility;

namespace obsidianUpdater.Monitor
{
	public class MonitorAction : ProgramAction
	{
		private StreamWriter _serverInput;
		private MonitorServer _monitorServer;

		private bool _restartOnce = true;
		private int _startupProgress = 0;

		public ServerStatus Status { get; private set; }
		public string LastStatusString { get; private set; }


		public MonitorAction(ProgramAction parent) : base("__internal__monitor", parent)
		{
			IsHidden = true;
		}

		public override void Handle(Stack<string> args)
		{
			if (args.Count > 0)
				new InvalidUsageException(this, "Too many arguments.");

			_monitorServer = new MonitorServer(this);
			_monitorServer.Start();
			_startupProgress = 0;

			while (true) {
				var data = ProgramData.Load();
				ChangeStatus(ServerStatus.Starting, "");

				using (var process = StartServerProcess(data)) {
					_serverInput = process.StandardInput;
					process.OutputDataReceived += (sender, e) => OnOutput(e.Data);
					process.BeginOutputReadLine();
					process.WaitForExit();
				}

				if (Status < ServerStatus.Stopped)
					ChangeStatus(ServerStatus.Stopped, "Server stopped!");

				if (!data.Server.AutoRestart || _restartOnce)
					break;
				_restartOnce = false;
			}

			_monitorServer.Stop();
		}

		public void Command(string command)
		{
			_serverInput.WriteLine(command);
		}
		public void Stop()
		{
			Command("stop");
		}
		public void Restart()
		{
			_restartOnce = true;
			Stop();
		}


		public void ChangeStatus(ServerStatus status, string statusString)
		{
			if ((Status == status) && (LastStatusString == statusString))
				return;
			Status = status;
			LastStatusString = statusString;
			_monitorServer.OnStatusChange(Status, statusString);
		}

		public void OnOutput(string line)
		{
			if (line == null)
				return;

			_monitorServer.OnOutput(line);

			string modName, eventName;
			LookForEvent(line, out modName, out eventName);

			// TODO: Detect crashing / shutting down.

			switch (Status) {
				case ServerStatus.Starting:
					var statusString = _startup[_startupProgress](line, eventName, modName);
					if (statusString != null) {
						var status = ((++_startupProgress < _startup.Length) ? ServerStatus.Starting : ServerStatus.Running);
						ChangeStatus(status, statusString);
					} else if (_startupProgress > 0) {
						statusString = _startup[_startupProgress - 1](line, eventName, modName);
						if (statusString != null)
							ChangeStatus(Status, statusString);
					}
					break;
			}
		}


		private static Process StartServerProcess(ProgramData data)
		{
			var arguments = Constants.SERVER_STARTUP
				.Replace("{JAR_FILE}", data.Server.JarFile)
			    .Replace("{JAVA_ARGS}", data.Server.JavaArguments)
			    .Replace("{MC_ARGS}", data.Server.MinecraftArguments)
				.Replace("{LOG_CONFIG}", Constants.LOG_CONFIG_FILE);
			var process = new Process {
				StartInfo = new ProcessStartInfo {
					WorkingDirectory = data.Server.Directory,
					FileName = Constants.JAVA_BIN, Arguments = arguments,
					UseShellExecute = false, CreateNoWindow = true,
					RedirectStandardOutput = true, RedirectStandardInput = true
				}
			};
			process.Start();
			return process;
		}


		private delegate string MatchingDelegate(string line, string eventName, string modName);

		private static readonly MatchingDelegate[] _startup = new MatchingDelegate[]{
			LookForText("[00:00:00] [main/INFO] [FML]: ", "Forge Mod Loader version", "FML initializing"),
			LookForText("[00:00:00] [Server thread/INFO] [MinecraftForge]: ", "Attempting early MinecraftForge initialization", "Forge initializing"),
			LookForEvent("FMLPreInitializationEvent", "Pre-initialization"),
			LookForText("[00:00:00] [Server thread/INFO] [FML]: ", "Applying holder lookups", "Pre-initialization done"),
			LookForEvent("FMLInitializationEvent", "Initialization"),
			LookForText("[00:00:00] [Server thread/TRACE] [FML]: ", "Attempting to deliver", "Initialization done"),
			LookForEvent("FMLPostInitializationEvent", "Post-initialization"),
			LookForText("[00:00:00] [Server thread/TRACE] [mcp]: ", "Sending event FMLLoadCompleteEvent", "Post-initialization done"),
			LookForText("[00:00:00] [Server thread/INFO] [FML]: ", "Injecting existing block and item data", "Server starting"),
			LookForText("[00:00:00] [Server thread/INFO]: ", "Done (", "Server started!")
		};


		private static bool LookForText(string line, int start, string message)
		{
			return ((line.Length >= start + message.Length) && (line[0] == '[') &&
			        (line.IndexOf(message, start, message.Length) >= 0));
		}
		private static MatchingDelegate LookForText(string start, string message, string status)
		{
			return (line, eventName, modName) => LookForText(line, start.Length, message) ? status : null;
		}
		// Example: [21:42:34] [Server thread/TRACE] [Forge]: Sending event FMLServerStartingEvent to mod Forge
		private static readonly Regex _eventRegex = new Regex(
			@"^\[..:..:..\] \[Server thread/TRACE\] \[(?<mod>\w+)\]: Sending event (?<event>\w+) to mod \k<mod>$");
		public static bool LookForEvent(string line, out string modName, out string eventName)
		{
			var match = _eventRegex.Match(line);
			modName = (match.Success ? match.Groups["mod"].Value : null);
			eventName = (match.Success ? match.Groups["event"].Value : null);
			return match.Success;
		}
		private static MatchingDelegate LookForEvent(string match, string status)
		{
			return (line, eventName, modName) => (eventName == match) ? (status + ": " + modName) : null;
		}
	}
}

