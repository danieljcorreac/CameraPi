using System;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace CameraPi.Server.Services
{
	public class StreamSocketClient
	{
		private StreamSocket _client;
		private HostName _hostName;
		public int FlagClientStart;
		public DataWriter Writer;

		public async Task Start(string hostNameString, string servicename)
		{
			if (FlagClientStart == 1)
				return;

			FlagClientStart = 1;
			try
			{
				_client = new StreamSocket();
				_hostName = new HostName(hostNameString);
				await _client.ConnectAsync(_hostName, servicename);
				Writer = new DataWriter(_client.OutputStream);
				FlagClientStart = 2;
			}
			catch (Exception)
			{
				FlagClientStart = 0;
			}
		}

		public async Task SendMessageString(string sendmsg)
		{
			if (Writer == null)
				return;

			try
			{
				Writer.WriteUInt32(Writer.MeasureString(sendmsg));
				Writer.WriteUInt32(2);
				Writer.WriteString(sendmsg);
				await Writer.StoreAsync();
			}
			catch (Exception)
			{
				// If this is an unknown status it means that the error if fatal and retry will likely fail.
				SocketConnectFailed();
			}
		}

		public async Task SendMessageByte(byte[] sendmsgByte)
		{
			if (Writer == null)
				return;

			try
			{
				Writer.WriteUInt32((uint) sendmsgByte.Length);
				Writer.WriteUInt32(1);
				Writer.WriteBytes(sendmsgByte);
				await Writer.StoreAsync();
			}
			catch (Exception)
			{
				SocketConnectFailed();
			}
		}

		public async Task SendBuffer(IBuffer sendmsgbuffer)
		{
			if (Writer == null)
				return;
			try
			{
				Writer.WriteUInt32(sendmsgbuffer.Length);
				Writer.WriteUInt32(3);
				Writer.WriteBuffer(sendmsgbuffer);
				await Writer.StoreAsync();
			}
			catch (Exception)
			{
				SocketConnectFailed();
			}
		}

		public void SocketConnectFailed()
		{
			Writer.Dispose();
			Writer = null;
			_client.Dispose();
			_client = null;
			FlagClientStart = 0;
		}
	}
}