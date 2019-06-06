using System;
using System.IO;
using UnityEngine;

public class DebugUiControl : IDisposable
{
	static int instanceCount = 0;

	public class Root : DebugUiControl
	{
		public new void Update(DebugUiManager.Input input)
		{
			var control = lastChild;
			while (control != null)
			{
				control.Update(input);
				control = control.prevSibling;
			}
		}

		public override void ToJson(StreamWriter stream)
		{
			stream.Write('{');
			ChildrenToJson(stream);
			stream.Write('}');
		}
	}

	/// <summary>
	/// どこに置こうか迷ったのでとりあえず内部クラスにしておく
	/// </summary>
	static protected class StreamUtil
	{
		public static void WriteEscapedString(StreamWriter writer, string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				writer.Write("\"\"");
				return;
			}

			writer.Write('"');

			foreach (var ch in text)
			{
				// ASCIIコードの印字可能文字はそのまま出力して、それ以外は全部エスケープする
				if (ch >= 0x20 && ch <= 0x7e)
				{
					if (ch == '"')
					{
						writer.Write('\\');
					}
					writer.Write(ch);
				}
				else
				{
					writer.Write("\\u");
					writer.Write(((int)ch).ToString("x4"));
				}
			}

			writer.Write('"');
		}

		public static void WriteColorRGBA(StreamWriter writer, Color32 color)
		{
			writer.Write(string.Format("\"#{0:x02}{1:x02}{2:x02}{3:x02}\"", color.r, color.g, color.b, color.a));
		}
	}

	readonly int id;
	public int Id { get { return id; } }

	public Action OnClick { get; set; }

	public bool Enabled = true;
	public bool EventEnabled;
	public bool Draggable;

	public float GlobalX { get; private set; }
	public float GlobalY { get; private set; }
	public float LocalX { get; set; }
	public float LocalY { get; set; }
	public float Width { get; set; }
	public float Height { get; set; }
	public float BorderWidth { get; set; }

	DebugUiManager manager;

	DebugUiControl parent;
	DebugUiControl prevSibling;
	DebugUiControl nextSibling;
	DebugUiControl firstChild;
	DebugUiControl lastChild;

	public DebugUiControl(DebugUiManager manager = null)
	{
		// スレッドセーフじゃなくてよさそうなのでInterlocked.Incrementは使わない
		id = instanceCount++;

		if (manager != null)
		{
			this.manager = manager;
			manager.OnCreate(this);
		}

		Enabled = true;
	}

	DebugUiControl()
	{
	}

	public virtual void Dispose()
	{
		if (manager != null)
		{
			manager.OnDispose(this);
			manager = null;
		}

		RemoveAllChildren();

		parent = null;
		prevSibling = null;
		nextSibling = null;
		firstChild = null;
		lastChild = null;
	}

	public void AddChild(DebugUiControl control)
	{
		control.parent = this;
		LinkToTail(control);
	}

	public void RemoveAllChildren()
	{
		var node = firstChild;
		while (node != null)
		{
			var next = node.nextSibling;
			node.Dispose();
			node = next;
		}
		firstChild = null;
		lastChild = null;
	}

	/// <summary>
	/// 諸々のデータを更新する
	/// </summary>
	public void Update(DebugUiManager.Input input)
	{
		GlobalX = parent.GlobalX + LocalX;
		GlobalY = parent.GlobalY + LocalY;

		var control = lastChild;
		while (control != null)
		{
			control.Update(input);
			control = control.prevSibling;
		}

		// TODO: マウスの入力を処理する
	}

	protected virtual void Draw(PrimitiveRenderer2D renderer)
	{
	}

	public void DrawRecursive(PrimitiveRenderer2D renderer)
	{
		if (!Enabled)
		{
			return;
		}

		Draw(renderer);

		var child = firstChild;
		while (child != null)
		{
			child.DrawRecursive(renderer);
			child = child.nextSibling;
		}
	}

	/// <summary>
	/// 座標が自身の内側にあればtrue、それ以外はfalseを返す。
	/// </summary>
	/// <returns>座標が自身の内側にあればtrue、それ以外はfalseを返す。</returns>
	public bool HitTest(float globalPointerX, float globalPointerY)
	{
		if (!Enabled)
		{
			return false;
		}

		if (EventEnabled)
		{
			if (
				globalPointerX >= GlobalX
				&& globalPointerX <= GlobalX + Width
				&& globalPointerY >= GlobalY
				&& globalPointerY <= GlobalY + Height)
			{
				return true;
			}
		}

		var child = firstChild;
		while (child != null)
		{
			if (child.HitTest(globalPointerX, globalPointerY))
			{
				return true;
			}
			child = child.nextSibling;
		}

		return false;
	}

	public void AdjustSize()
	{
		var maxX = 0f;
		var maxY = 0f;

		var control = firstChild;
		while (control != null)
		{
			var x = control.LocalX + control.Width;
			var y = control.LocalY + control.Height;
			maxX = Math.Max(maxX, x);
			maxY = Math.Max(maxY, y);

			control = control.nextSibling;
		}

		Width = maxX + BorderWidth * 2f;
		Height = maxY + BorderWidth * 2f;
	}

	public virtual void ToJson(StreamWriter stream)
	{
		stream.Write("{}");
	}

	protected void ChildrenToJson(StreamWriter stream)
	{
		stream.Write("\"children\":[");

		var child = firstChild;
		while (true)
		{
			child.ToJson(stream);
			child = child.nextSibling;

			if (child  == null)
			{
				break;
			}

			stream.Write(",");
		}
		stream.Write("]");
	}

	public virtual void ReceiveMessage(string message, string data)
	{
		switch (message)
		{
			case "click":
				if (OnClick != null)
				{
					OnClick();
				}
				break;
		}
	}

	void LinkToTail(DebugUiControl control)
	{
		control.nextSibling = null;

		if (firstChild == null)
		{
			firstChild = control;
			lastChild = control;
			control.prevSibling = null;
		}
		else
		{
			lastChild.nextSibling = control;
			control.prevSibling = lastChild;
			lastChild = control;
		}
	}

	protected void UnlinkSelf()
	{
		prevSibling.nextSibling = nextSibling;
		nextSibling.prevSibling = prevSibling;
		prevSibling = null;
		nextSibling = null;
		parent = null;
	}
}
