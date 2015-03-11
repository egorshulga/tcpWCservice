using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace tcpWCservice
{
	static class Program
	{
		static void Main(string[] args)
		{
			if (!args.Any())
			{
				Console.WriteLine("Choose either server or client to start. Terminating...");
				return;
			}
			switch (args[args.Count() - 1])
			{
				case "server":
					Server.StartListening();
					break;
				case "client":
					Client.Start();
					break;
				default:
					Console.WriteLine("Choose either server or client to start. Terminating...");
					break;
			}
		}

		static class Server
		{
			private static TcpListener server;
			private const int port = 1414;

			public static void StartListening()
			{
				server = new TcpListener(IPAddress.Any, port);
				try
				{
					server.Start();

					while (true)
					{
						Console.WriteLine("Server: waiting for connections...");
						TcpClient client = server.AcceptTcpClient();
						Console.WriteLine("Server: received connection from {0}.", (IPEndPoint)client.Client.RemoteEndPoint);

						Thread thread = new Thread(ProcessClient);
						thread.Start(client);

						Thread.Sleep(1000);
					}

				}
				catch (Exception e)
				{
					Console.WriteLine("Error {0}", e);
				}
				finally
				{
					server.Stop();
				}
			}

			private static void ProcessClient(object obj)
			{
				TcpClient client = (TcpClient)obj;
				EndPoint clientInfo = client.Client.RemoteEndPoint;

				while (true)
				{
					string currentTime = DateTime.Now.ToString(CultureInfo.CurrentCulture);
					byte[] bufferedCurrentTime = Encoding.ASCII.GetBytes(currentTime);
					try
					{
						client.GetStream().Write(bufferedCurrentTime, 0, bufferedCurrentTime.Length);
					}
					catch (IOException)
					{
						Console.WriteLine("Server: client {0} has just disconnected.", clientInfo);
						Console.WriteLine("Server: waiting for connections...");
						return;
					}
					Thread.Sleep(1000);
				}

			}

		}


		static class Client
		{
			private static readonly TcpClient client = new TcpClient();
			private static readonly IPEndPoint remoteServer = new IPEndPoint(new IPAddress(new byte[] {192,168,33,109}), 1414);

			public static void Start()
			{
				try
				{
					client.Connect(remoteServer);
					string currentTime = DateTime.Now.ToString(CultureInfo.CurrentCulture);
					byte[] bufferedCurrentTime = Encoding.ASCII.GetBytes(currentTime);
					while (true)
					{
						client.GetStream().Read(bufferedCurrentTime, 0, bufferedCurrentTime.Length);
						currentTime = Encoding.ASCII.GetString(bufferedCurrentTime);
						Console.WriteLine(currentTime);
					}
				}
				catch (IOException)
				{
					Console.WriteLine("Client: server unexpectedly disconnected. Terminating...");
				}
				catch (SocketException)
				{
					Console.WriteLine("Client: server not found. Terminating...");
				}
			}


		}

		

	}
}
