using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;

public class DebugUiManager :
	Graphic,
	IDisposable,
	IPointerClickHandler,
	IPointerDownHandler,
	IPointerUpHandler,
	IBeginDragHandler,
	IDragHandler,
	ICanvasRaycastFilter
{
	public class Input
	{
		public Vector2 PointerPosition;
		public DebugUiControl DraggedControl;
		public bool IsPointerDown;
		public bool DragStarted;

		public bool IsDragging
		{
			get
			{
				return DraggedControl != null;
			}
		}
	}

	[Serializable]
	public class Message
	{
		public int id;
		public string eventType;
		public string data;
	}

	PrimitiveRenderer2D primitiveRenderer;

	Input input;
	DebugUiControl.Root root;
	List<DebugUiControl> instances = new List<DebugUiControl>();

	public static DebugUiManager Create(GameObject gameObject, PrimitiveRenderer2D renderer)
	{
		var self = gameObject.GetComponent<DebugUiManager>();
		if (self != null)
		{
			return self;
		}

		self = gameObject.AddComponent<DebugUiManager>();
		self.Initialize(renderer);

		var rectTransform = self.rectTransform;
		Debug.Assert(rectTransform != null, "RectTransformがない!canvasの下にあるGameObjectを指定してください!");
		if (rectTransform)
		{
			rectTransform.anchorMin = new Vector2(0f, 0f);
			rectTransform.anchorMax = new Vector2(1f, 1f);
			rectTransform.offsetMin = new Vector2(0f, 0f);
			rectTransform.offsetMax = new Vector2(0f, 0f);
		}

		return self;
	}

	public void Initialize(PrimitiveRenderer2D renderer)
	{
		primitiveRenderer = renderer;
		input = new Input();
		root = new DebugUiControl.Root();
	}

	public void Dispose()
	{
		primitiveRenderer = null;
		input = null;

		root.Dispose();
		root = null;

		if (instances != null)
		{
			instances.Clear();
			instances = null;
		}
	}

	public void ManualUpdate()
	{
		root.Update(input);
		root.DrawRecursive(primitiveRenderer);
	}

	public void Add(DebugUiControl control)
	{
		root.AddChild(control);
	}

	public void ToJson(StreamWriter writer)
	{
		root.ToJson(writer);
	}

	public void OnCreate(DebugUiControl control)
	{
		// IDは連番なので、生成された順にaddしていくと自動的にIDでソートされる
		instances.Add(control);
	}

	public void OnDispose(DebugUiControl control)
	{
		instances.Remove(control);
	}

	// こんな実装じゃなくてもいい気がする
	public void SendMessage(Message message)
	{
		var control = GetControl(message.id);

		if (control != null)
		{
			control.ReceiveMessage(message.eventType, message.data);
		}
		else
		{
			Debug.LogWarningFormat("DebugUiManager.SendMessage control not found: id: {0}", message.id);
		}
	}

	public void OutputHierarchy()
	{
		using (var a = new System.IO.MemoryStream())
		{
			using (var w = new System.IO.StreamWriter(a))
			{
				root.ToJson(w);
			}
			Debug.Log(System.Text.Encoding.UTF8.GetString(a.ToArray()));
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		UpdatePoisition(eventData.position);
		Debug.LogFormat("OnPointerClick: {0}", input.PointerPosition);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		UpdatePoisition(eventData.position);
		Debug.LogFormat("OnPointerDown: {0}", input.PointerPosition);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		UpdatePoisition(eventData.position);
		Debug.LogFormat("OnPointerUp: {0}", input.PointerPosition);
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		UpdatePoisition(eventData.position);
		Debug.LogFormat("OnBeginDrag: {0}", input.PointerPosition);
	}

	public void OnDrag(PointerEventData eventData)
	{
		UpdatePoisition(eventData.position);
		Debug.LogFormat("OnDrag: {0}", input.PointerPosition);
	}

	public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
	{
		//throw new System.NotImplementedException();
		if (input.IsDragging)
		{

		}
		return true;
	}

	/// <summary>
	/// 座標系を仮想解像度向け(1136x640とかでY下向き)に変換
	/// </summary>
	public void ConvertCoordFromUnityScreen(ref Vector2 position)
	{
		// Xはスケールするだけ
		position.x = position.x * primitiveRenderer.ToVirtualScreenScale;
		// Yはスケールして反転
		position.y = primitiveRenderer.ReferenceScreenHeight - (position.y * primitiveRenderer.ToVirtualScreenScale);
	}

	void UpdatePoisition(Vector2 position)
	{
		input.PointerPosition = position;
		ConvertCoordFromUnityScreen(ref input.PointerPosition);
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
	}

	DebugUiControl GetControl(int id)
	{
		var first = 0;
		var last = instances.Count - 1;

		while (first <= last)
		{
			var i = (first + last) / 2;
			var controlId = instances[i].Id;
			if (controlId == id)
			{
				return instances[i];
			}
			else
			{
				if (controlId < id)
				{
					first = i + 1;
				}
				else
				{
					first = i - 1;
				}
			}
		}

		return null;
	}
}
