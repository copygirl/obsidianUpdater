using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using obsidianUpdater.Utility;

namespace obsidianUpdater.Monitor
{
	public class MonitorClient : IDisposable
	{
		private readonly TcpClient _client;

		private StreamReader _reader;
		private StreamWriter _writer;

		public ServerStatus Status { get; private set; }

		public bool Connected { get { return _client.Connected; } }

		public event Action<string> StatusChanged;
		public event Action<string> OutputReceived;


		public MonitorClient()
		{
			_client = new TcpClient();
		}


		public async Task<bool> Connect()
		{
			var timeout = Task.Delay(TimeSpan.FromSeconds(1));
			var connect = _client.ConnectAsync(IPAddress.Loopback, Constants.MONITOR_PORT);

			var completed = await Task.WhenAny(timeout, connect).ConfigureAwait(false);
			if ((completed != connect) || !_client.Connected) {
				_client.Close();
				return false;
			}

			var stream = _client.GetStream();
			_reader = new StreamReader(stream);
			_writer = new StreamWriter(stream){ AutoFlush = true };

			return true;
		}

		public void Disconnect()
		{
			_client.Close();
		}


		public void Stop()
		{
			_writer.WriteLine("stop");
		}
		public void Restart()
		{
			_writer.WriteLine("restart");
		}
		public void Command(string command)
		{
			_writer.WriteLine("command=" + command);
		}
		public void ReceiveOutput()
		{
			_writer.WriteLine("output");
		}


		public void WaitUntilStarted()
		{
			while (Connected && (Status < ServerStatus.Running)) {
				try { ParseLine(_reader.ReadLine()); }
				catch (IOException) { _client.Close(); }
			}
		}
		public void WaitUntilStopped()
		{
			while (Connected && (Status < ServerStatus.Stopped)) {
				try { ParseLine(_reader.ReadLine()); }
				catch (IOException) { _client.Close(); }
			}
		}

		private void ParseLine(string line)
		{
			if (line == null) {
				_client.Close();
				return;
			}
			if (line.StartsWith("status=")) {
				line = line.Substring("status=".Length);
				var args = line.Split(new char[]{ ':' }, 2);
				Status = (ServerStatus)Enum.Parse(typeof(ServerStatus), args[0]);
				string statusString = args[1];
				if (StatusChanged != null)
					StatusChanged(statusString);
			} else if (line.StartsWith("output=")) {
				line = line.Substring("output=".Length);
				if (OutputReceived != null)
					OutputReceived(line);
			}
		}

		#region IDisposable implementation

		public void Dispose()
		{
			Disconnect();
		}

		#endregion
	}
}

