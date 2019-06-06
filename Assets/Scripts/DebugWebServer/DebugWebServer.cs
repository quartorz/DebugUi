using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using UnityEngine;

public abstract class DebugWebServer : IDisposable
{
	class InternalTcpListener : TcpListener
	{
		public new bool Active { get { return base.Active; } }
		public InternalTcpListener(IPAddress address, int port) : base(address, port)
		{
		}
	}

	InternalTcpListener listener;

	public string IPAddress
	{
		get
		{
			if (listener == null)
			{
				return null;
			}

			var ip = "localhost";
			foreach (var ipAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
			{
				if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
				{
					ip = ipAddress.ToString();
					break;
				}
			}
			return string.Format("{0}:{1}", ip, ((IPEndPoint)listener.LocalEndpoint).Port);
		}
	}

	public virtual void Dispose()
	{
		if (listener != null)
		{
			listener.Stop();
			//listener = null;
		}
	}

	public void Start(int port = 0)
	{
		if (listener != null)
		{
			return;
		}

		listener = new InternalTcpListener(System.Net.IPAddress.Any, port);
		listener.Start();
		listener.BeginAcceptTcpClient(OnAccept, null);
	}

	public abstract bool Handler(TcpClient client, StreamReader reader, StreamWriter writer);

	void OnAccept(IAsyncResult ar)
	{
		TcpClient client = null;
		try
		{
			client = listener.EndAcceptTcpClient(ar);
		}
		catch (SocketException e)
		{
			Debug.LogException(e);
			return;
		}
		finally
		{
			if (listener.Active)
			{
				listener.BeginAcceptTcpClient(OnAccept, null);
			}
		}

		if (!listener.Active)
		{
			return;
		}

		var stream = client.GetStream();
		var reader = new StreamReader(stream);
		var writer = new StreamWriter(stream);

		try
		{
			if (Handler(client, reader, writer))
			{
				stream.Dispose();
				reader.Dispose();
				writer.Dispose();
				client.Close();
			}
		}
		catch (Exception e)
		{
			Debug.LogException(e);

			stream.Dispose();
			reader.Dispose();
			writer.Dispose();
			client.Close();
		}
	}
}
