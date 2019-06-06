using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Sockets;

public class DebugHttpServer : DebugWebServer
{
	public enum Method
	{
		GET,
		POST,
		Other,
	}

	public enum StatusCode
	{
		OK = 200,
		BadRequest = 400,
		NotFound = 404,
		MethodNotAllowed = 405,
		NotImplemented = 501,
		ServiceUnavailable = 503,
	}

	public class Context : IDisposable
	{
		public Method Method;
		public string Uri;
		public Dictionary<string, string> Headers;
		public StreamReader Body;
		public StreamWriter Response;

		TcpClient tcpClient;

		public bool Disconnected
		{
			get
			{
				return tcpClient.Client.Poll(1000, SelectMode.SelectRead) && tcpClient.Client.Available == 0;
			}
		}

		public Context(TcpClient client, StreamReader request, StreamWriter response)
		{
			tcpClient = client;
			this.Response = response;

			var requestLine = requestLineRegex.Match(request.ReadLine());

			if (!requestLine.Success)
			{
				Method = Method.Other;
				Uri = null;
				Headers = null;
				Body = null;
				return;
			}

			switch (requestLine.Groups["method"].Value)
			{
				case "GET":
					Method = Method.GET;
					break;
				case "POST":
					Method = Method.POST;
					break;
				default:
					Method = Method.Other;
					break;
			}

			Uri = requestLine.Groups["uri"].Value;
			Headers = new Dictionary<string, string>();

			while (true)
			{
				var line = request.ReadLine();

				if (string.IsNullOrEmpty(line))
				{
					break;
				}

				var index = line.IndexOf(':');

				if (index == -1)
				{
					continue;
				}

				var key = line.Substring(0, index).Trim();

				if (string.IsNullOrEmpty(key))
				{
					continue;
				}

				Headers.Add(key, line.Substring(index + 1).Trim());
			}

			Body = request;
		}

		public void Dispose()
		{
			Close();
			tcpClient = null;
			Body = null;
			Response = null;
		}

		public void WriteStatus(StatusCode statusCode)
		{
			Response.Write("HTTP/1.1 ");
			Response.Write(statusCode.ToReasonPhrase());
			Response.Write("\r\n");
		}

		public void WriteDate()
		{
			Response.Write("Date: ");
			Response.Write(DateTime.UtcNow.ToString("R"));
			Response.Write("\r\n");
		}

		void Close()
		{
			tcpClient.Close();
			Body.Dispose();
			Response.Dispose();
		}
	}

	static Regex requestLineRegex = new Regex(@"^(?<method>[A-Z]+) (?<uri>[^ ]+) HTTP/1\.1$");

	/// <summary>
	/// 接続を切断しても問題ない場合はtrueを返す。
	/// </summary>
	public Func<Context, bool> Router { get; set; }

	public override void Dispose()
	{
		base.Dispose();
		Router = null;
	}

	public override bool Handler(TcpClient client, StreamReader reader, StreamWriter writer)
	{
		if (Router != null)
		{
			var context = new Context(client, reader, writer);
			try
			{
				if (Router(context))
				{
					context.Dispose();
					return true;
				}
				return false;
			}
			catch (Exception e)
			{
				context.Dispose();
				throw e;
			}
		}

		return true;
	}
}

static class StatusCodeExt
{
	public static string ToReasonPhrase(this DebugHttpServer.StatusCode self)
	{
		switch (self)
		{
			case DebugHttpServer.StatusCode.OK:
				return "200 OK";
			case DebugHttpServer.StatusCode.BadRequest:
				return "400 Bad Request";
			case DebugHttpServer.StatusCode.NotFound:
				return "404 Not Found";
			case DebugHttpServer.StatusCode.MethodNotAllowed:
				return "405 Method Not Allowed";
			case DebugHttpServer.StatusCode.NotImplemented:
				return "501 Not Implemented";
			case DebugHttpServer.StatusCode.ServiceUnavailable:
				return "503 Service Unavailable";
			default:
				return "";
		}
	}
}
