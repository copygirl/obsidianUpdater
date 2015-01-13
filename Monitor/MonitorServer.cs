using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using obsidianUpdater.Utility;

namespace obsidianUpdater.Monitor
{
	public class MonitorServer
	{
		private readonly int RECENT_OUTPUT_LINES = 100;

		private readonly object _outputLock = new object();

		private readonly MonitorAction _monitor;
		private readonly HashSet<Client> _clients = new HashSet<Client>();
		private readonly LinkedList<string> _recentOutput = new LinkedList<string>();

		private TcpListener _server;
		private CancellationTokenSource _cts;

		public MonitorServer(MonitorAction monitor)
		{
			_monitor = monitor;
		}

		public void Start()
		{
			_cts = new CancellationTokenSource();
			_server = new TcpListener(IPAddress.Loopback, Constants.MONITOR_PORT);
			_server.Start();
			var acceptClients = AcceptClientsAsync(_server, _cts.Token);
		}
		public void Stop()
		{
			_cts.Cancel();
			_server.Stop();
		}

		public void OnStatusChange(ServerStatus status, string statusString)
		{
			foreach (var client in _clients)
				client.Output.WriteLine("status=" + status + ":" + statusString);
		}
		public void OnOutput(string line)
		{
			lock (_outputLock) {
				_recentOutput.AddLast(line);
				if (_recentOutput.Count > RECENT_OUTPUT_LINES)
					_recentOutput.RemoveFirst();

				foreach (var client in _clients)
					if (client.ReceivesOutput)
						client.Output.WriteLine("output=" + line);
			}
		}

		private async Task AcceptClientsAsync(TcpListener listener, CancellationToken ct)
		{
			while (!ct.IsCancellationRequested) {
				var tcpClient = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
				var client = new Client(_monitor, tcpClient);
				_clients.Add(client);
				client.OnDisconnect += () => _clients.Remove(client);
				client.OnMessage += (line) => HandleMessage(client, line);
				client.ReceiveMessages(ct);
			}
		}

		private void HandleMessage(Client client, string line)
		{
			switch (line) {
				case "stop":
					_monitor.Stop();
					break;
				case "restart":
					_monitor.Restart();
					break;
				case "output":
					client.ReceivesOutput = true;
					lock (_outputLock)
						foreach (string output in _recentOutput)
							client.Output.WriteLine("output=" + output);
					break;
				default:
					if (line.StartsWith("command="))
						_monitor.Command(line.Substring("command=".Length));
					break;
			}
		}


		private class Client
		{
			private readonly TcpClient _client;
			private bool _disconnectFired = false;

			public StreamReader Input { get; private set; }
			public StreamWriter Output { get; private set; }

			public bool Connected { get { return _client.Connected; } }
			public bool ReceivesOutput { get; set; }

			public event Action OnDisconnect;
			public event Action<string> OnMessage;

			public Client(MonitorAction monitor, TcpClient client)
			{
				_client = client;

				var stream = client.GetStream();
				Input = new StreamReader(stream);
				Output = new StreamWriter(stream){ AutoFlush = true };

				Output.WriteLine("status=" + monitor.Status + ":" + monitor.LastStatusString);
			}

			public void Disconnect()
			{
				try { _client.Close(); }
				catch (SocketException) {  }
				if (!_disconnectFired) {
					_disconnectFired = true;
					OnDisconnect.Raise();
				}
			}

			public async void ReceiveMessages(CancellationToken ct)
			{
				while (Connected && !ct.IsCancellationRequested) {
					try {
						var timeout = Task.Delay(TimeSpan.FromSeconds(20));
						var readLine = Input.ReadLineAsync();
				
						var completed = await Task.WhenAny(timeout, readLine).ConfigureAwait(false);
						if ((completed != readLine) || (readLine.Result == null))
							break;

						OnMessage.Raise(readLine.Result);
					} catch (AggregateException e) {
						if (!(e.InnerException is IOException))
							throw;
					}
				}
				Disconnect();
			}
		}
	}
}

