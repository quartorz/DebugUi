using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
	class SynchronousRequest : IDisposable
	{
		public DebugHttpServer.Context Context;
		public string Payload;

		public void Dispose()
		{
			if (Context != null)
			{
				Context.Dispose();
				Context = null;
			}
		}
	}

	DebugHttpServer server = new DebugHttpServer();

	public DebugUiManager debugUiManager;
	public TextAsset textAsset;

	string html;

	Queue<SynchronousRequest> requests = new Queue<SynchronousRequest>();

	void Awake()
	{
		server.Router = Router;
		server.Start(8000);
		Debug.Log(server.IPAddress);

		html = textAsset.text;
	}

	void OnDestroy()
	{
		server.Dispose();
		server = null;
	}

	void Update()
	{
		if (requests.Count != 0)
		{
			lock (requests)
			{
				ProcessDebugUiMessage(requests.Dequeue());
			}
		}
	}

	void ProcessDebugUiMessage(SynchronousRequest request)
	{
		using (request)
		{
			request.Context.WriteStatus(DebugHttpServer.StatusCode.OK);
			request.Context.WriteDate();
			request.Context.Response.Write("\r\n");
		}

		var message = JsonUtility.FromJson<DebugUiManager.Message>(request.Payload);
		debugUiManager.SendMessage(message);
	}

	bool Router(DebugHttpServer.Context context)
	{
		Debug.LogFormat("{0} {1}", context.Method, context.Uri);
		foreach (var v in context.Headers)
		{
			//Debug.LogFormat("{0}: {1}", v.Key, v.Value);
		}

		var response = context.Response;
		response.AutoFlush = true;

		if (context.Method == DebugHttpServer.Method.GET && context.Uri == "/api/debugui-event")
		{
			if (debugUiManager == null)
			{
				context.WriteStatus(DebugHttpServer.StatusCode.ServiceUnavailable);
				context.WriteDate();
				response.Write("\r\n");
				return true;
			}

			context.WriteStatus(DebugHttpServer.StatusCode.OK);
			context.WriteDate();
			response.Write("Content-Type: text/event-stream\r\n\r\n");
			response.Write("event: start\r\n");
			response.Write("data: ");
			debugUiManager.ToJson(response);
			response.Write("\r\n\r\n");

			return true;
		}
		else if (context.Method == DebugHttpServer.Method.POST && context.Uri == "/api/debugui-event")
		{
			try
			{
				lock (requests)
				{
					requests.Enqueue(new SynchronousRequest()
					{
						Context = context,
						Payload = context.Body.ReadLine()
					});
				}

				return false;
			}
			catch (Exception)
			{
				context.WriteStatus(DebugHttpServer.StatusCode.BadRequest);
				context.WriteDate();
				response.Write("\r\n");
				return true;
			}
		}
		else
		{
			if (context.Method != DebugHttpServer.Method.GET)
			{
				context.WriteStatus(DebugHttpServer.StatusCode.MethodNotAllowed);
				context.WriteDate();
				response.Write("\r\nContent-Length: 0\r\n\r\n");
				return true;
			}

			context.WriteStatus(DebugHttpServer.StatusCode.OK);
			context.WriteDate();
			response.Write("Content-Type: text/html; charset=UTF-8\r\n\r\n");

			response.Write(html);
			return true;
		}
	}
}
