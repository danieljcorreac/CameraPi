using System;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace CameraPi.Server.Services
{
	public class StreamSocketListenerServer
	{
		private readonly StreamSocketListener _listener;
		public byte[] ReadByte;
		public int ReceiveBufferFlag;
		public int ReceiveByteFlag;
		public int ReceiveClientIp;
		public IBuffer Receiverbuffer;
		public string StringTemp;

		public StreamSocketListenerServer()
		{
			_listener = new StreamSocketListener();
			_listener.ConnectionReceived += OnConnection;
		}

		public async Task Start(string servicename)
		{
			try
			{
				await _listener.BindServiceNameAsync(servicename);
			}
			catch (Exception exception)
			{
				// If this is an unknown status it means that the error is fatal and retry will likely fail.
				if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
					throw;
			}
		}

		public async Task Start(string hostname, string servicename)
		{
			try
			{
				var hostName = new HostName(hostname);
				await _listener.BindEndpointAsync(hostName, servicename);
			}
			catch (Exception exception)
			{
				// If this is an unknown status it means that the error is fatal and retry will likely fail.
				if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
					throw;
			}
		}

		private async void OnConnection(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
		{
			var reader = new DataReader(args.Socket.InputStream);
			try
			{
				while (true)
				{
					// Read first 4 bytes (length of the subsequent string).
					var sizeFieldCount = await reader.LoadAsync(sizeof(uint));
					if (sizeFieldCount != sizeof(uint))
						return;

					var sizeFieldCount1 = await reader.LoadAsync(sizeof(uint));
					if (sizeFieldCount1 != sizeof(uint))
						return;

					// Read the string.
					var stringLength = reader.ReadUInt32();
					var msgtype = reader.ReadUInt32();
					var actualStringLength = await reader.LoadAsync(stringLength);
					if (stringLength != actualStringLength)
						return;

					// Display the string on the screen. The event is invoked on a non-UI thread, so we need to marshal
					// the text back to the UI thread.

					if (msgtype == 1)
					{
						ReadByte = new byte[actualStringLength];
						reader.ReadBytes(ReadByte);
						ReceiveByteFlag = 1;
					}
					else if (msgtype == 2)
					{
						StringTemp = reader.ReadString(actualStringLength);
						ReceiveClientIp = 1;
					}
					else if (msgtype == 3)
					{
						Receiverbuffer = reader.ReadBuffer(actualStringLength);
						ReceiveBufferFlag = 1;
					}
				}
			}
			catch (Exception exception)
			{
				// If this is an unknown status it means that the error is fatal and retry will likely fail.
				if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
					throw;
			}
		}
	}
}