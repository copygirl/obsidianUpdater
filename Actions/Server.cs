using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using obsidianUpdater.Data;
using obsidianUpdater.Monitor;
using obsidianUpdater.Utility;

namespace obsidianUpdater.Actions
{
	public class Server : ProgramAction
	{
		// TODO: Implement timed stopping / restarting.

		public Server(ProgramAction parent) : base("server", parent)
		{
			AddSubAction(new Start(this));
			AddSubAction(new Stop(this));
			AddSubAction(new Restart(this));
			AddSubAction(new Output(this));
		}

		public static void StartSelf(string arguments)
		{
			var file = Environment.GetCommandLineArgs()[0];

			if (Type.GetType("Mono.Runtime") != null) {
				arguments = "\"" + file + "\" " + arguments;
				file = Constants.MONO_BIN;
			}

			using (var process = new Process()) {
				process.StartInfo = new ProcessStartInfo(file, arguments){ CreateNoWindow = true };
				process.Start();
			}
		}


		public static void VerifyData()
		{
			var data = ProgramData.Load();
			if (data.Server == null)
				throw new InvalidOperationException("Server options not set, try running 'obsidian setup'");

			if (!Directory.Exists(data.Server.Directory))
				throw new InvalidOperationException("Server directory does not exist, try running 'obsidian setup'");
			if (!File.Exists(Path.Combine(data.Server.Directory, data.Server.JarFile)))
				throw new InvalidOperationException("Server .jar does not exist, try running 'obsidian setup'.");
		}

		public static MonitorClient ConnectMonitor()
		{
			var monitor = new MonitorClient();
			if (!monitor.Connect().Result)
				throw new InvalidOperationException("Couldn't connect to server, is it not running?");
			return monitor;
		}

		public static void OutputStatusChanges(MonitorClient monitor)
		{
			if (Console.IsOutputRedirected)
				return;
			Console.WriteLine();
			monitor.StatusChanged += (status) => {
				Console.SetCursorPosition(0, Console.CursorTop - 1);
				string line = String.Format("[{0}] > {1}", monitor.Status, status);
				Console.WriteLine("{0,-" + (Console.BufferWidth - 1) + "}", line);
			};
		}

		public static bool IsServerRunning()
		{
			return new MonitorClient().Connect().Result;
		}


		public static void StartServer()
		{
			StartSelf("__internal__monitor");
			Thread.Sleep(TimeSpan.FromSeconds(0.2));

			using (var monitor = ConnectMonitor()) {
				OutputStatusChanges(monitor);
				monitor.WaitUntilStarted();
			}
		}

		public static void StopServer()
		{
			using (var monitor = ConnectMonitor()) {
				OutputStatusChanges(monitor);
				monitor.Stop();
				monitor.WaitUntilStopped();
			}
		}

		public static void RestartServer()
		{
			using (var monitor = ConnectMonitor()) {
				OutputStatusChanges(monitor);
				monitor.Restart();
				monitor.WaitUntilStopped();
				monitor.WaitUntilStarted();
			}
		}

		public static void GetServerOutput()
		{
			using (var monitor = ConnectMonitor()) {
				monitor.OutputReceived += (line) => Console.WriteLine(line);
				monitor.ReceiveOutput();
				monitor.WaitUntilStopped();
			}
		}


		public class Start : ProgramAction
		{
			public Start(ProgramAction parent) : base("start", parent)
			{
				Help = new string[]{ "Starts the server, if it isn't running already." };
			}

			public override void Handle(Stack<string> args)
			{
				if (args.Count > 0)
					new InvalidUsageException(this, "Too many arguments.");

				VerifyData();
				if (IsServerRunning())
					throw new ArgumentException("Server is already running.");
				StartServer();
			}
		}

		public class Stop : ProgramAction
		{
			public Stop(ProgramAction parent) : base("stop", parent)
			{
				Help = new string[]{ "Stops the server, if it is currently running." };
			}

			public override void Handle(Stack<string> args)
			{
				if (args.Count > 0)
					new InvalidUsageException(this, "No arguments required.");

				StopServer();
			}
		}

		public class Restart : ProgramAction
		{
			public Restart(ProgramAction parent) : base("restart", parent)
			{
				Help = new string[]{ "Restarts the server, if it is currently running." };
			}

			public override void Handle(Stack<string> args)
			{
				if (args.Count > 0)
					new InvalidUsageException(this, "No arguments required.");

				VerifyData();
				RestartServer();
			}
		}

		public class Output : ProgramAction
		{
			public Output(ProgramAction parent) : base("output", parent)
			{
				Help = new string[]{ "Prints the output of the server" };
			}

			public override void Handle(Stack<string> args)
			{
				if (args.Count > 0)
					new InvalidUsageException(this, "No arguments required.");

				GetServerOutput();
			}
		}
	}
}

