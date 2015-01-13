using System;
using System.Collections.Generic;
using System.IO;
using obsidianUpdater.Data;
using obsidianUpdater.Utility;

namespace obsidianUpdater.Actions
{
	public class Setup : ProgramAction
	{
		public Setup(ProgramAction parent) : base("setup", parent)
		{
		}

		public override void Handle(Stack<string> args)
		{
			if (args.Count > 0)
				new InvalidUsageException(this, "Too many arguments.");

			string dir;
			var data = ProgramData.Load();
			data.Server = new ProgramData.ServerData {

				Directory = dir = Input("Directory", 
					((data.Server != null) ? data.Server.Directory
					                       : Constants.DEFAULT_DIRECTORY),
						(value) => {
							if (!Directory.Exists(value))
								throw new Exception("Server directory doesn't exist.");
						}),

				JarFile = Input("Jar file", 
					((data.Server != null) ? data.Server.JarFile
					                       : Constants.DEFAULT_JAR_FILE),
						(value) => {
							if (!File.Exists(Path.Combine(dir, value)))
								throw new Exception("Server .jar doesn't exist.");
						}),

				JavaArguments = Input("Java args", 
					((data.Server != null) ? data.Server.JavaArguments
					                       : Constants.DEFAULT_JAVA_ARGUMENTS)),

				MinecraftArguments = Input("Minecraft args", 
					((data.Server != null) ? data.Server.MinecraftArguments
					                       : Constants.DEFAULT_MINECRAFT_ARGUMENTS)),

				AutoRestart = Input("Auto restart", 
					((data.Server != null) ? data.Server.AutoRestart
					                       : true))
			};
			data.Save();

			var logConfigFile = Path.Combine(data.Server.Directory, Constants.LOG_CONFIG_FILE);
			if (!File.Exists(logConfigFile))
				File.WriteAllText(logConfigFile, Constants.LOG_CONFIG_DATA);
		}

		public static T Input<T>(string name, T defaultValue, Action<T> verify = null)
		{
			while (true) {
				var defaultString = defaultValue.ToString();
				if (defaultString.Length > 20)
					defaultString = defaultString.Substring(0, 17) + "...";
				Console.Write("{0} (default='{1}'): ", name, defaultString);

				var input = Console.ReadLine();
				if (input.Length <= 0)
					return defaultValue;

				try {
					T value = (T)Convert.ChangeType(input, typeof(T));
					if (verify != null)
						verify(value);
					return value;
				} catch (Exception e) {
					Console.WriteLine("Error: " + e.Message);
				}
			}
		}
	}
}

