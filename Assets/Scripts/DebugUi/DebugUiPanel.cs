using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DebugUiPanel : DebugUiColoredControl
{
	public float LineSpacing = 0f;

	float nextX;
	float nextY;
	float currentLineHeight;

	public DebugUiPanel(
		float width = float.MaxValue,
		float height = float.MaxValue,
		float borderWidth = 1f,
		DebugUiManager manager = null) : base(manager)
	{
		BorderWidth = borderWidth;
		BackgroundColor = new Color32(100, 100, 100, 100);
		BorderColor = new Color32(255, 255, 255, 255);
		Width = width;
		Height = height;
		nextX = borderWidth;
		nextY = nextX;
		currentLineHeight = 0f;
	}

	public void AddChildAuto(DebugUiControl control)
	{
		control.LocalX = nextX;
		control.LocalY = nextY;

		AddChild(control);

		nextX += control.Width;
		currentLineHeight = Math.Max(currentLineHeight, control.Height);
	}

	public void BreakLine()
	{
		nextX = BorderWidth;
		nextY += currentLineHeight + LineSpacing;
		currentLineHeight = 0f;
	}


	protected override void Draw(PrimitiveRenderer2D renderer)
	{
		renderer.Color = BackgroundColor;
		renderer.FillRectangle(new Vector4(GlobalX, GlobalY, GlobalX + Width, GlobalY + Height));
		renderer.Color = BorderColor;
		renderer.DrawRectangleInside(new Vector4(GlobalX, GlobalY, GlobalX + Width, GlobalY + Height), BorderWidth);
	}

	public override void ToJson(StreamWriter stream)
	{
		stream.Write(
			string.Format("{{" +
				"\"type\":\"panel\"," +
				"\"id\":{0}," +
				"\"x\":{1}," +
				"\"y\":{2}," +
				"\"width\":{3}," +
				"\"height\":{4}," +
				"\"events\":[],",
			Id,
			LocalX,
			LocalY,
			Width - BorderWidth * 2f,
			Height - BorderWidth * 2f));
		stream.Write("\"attributes\":{");
		stream.Write("\"backgroundColor\":");
		StreamUtil.WriteColorRGBA(stream, BackgroundColor);
		stream.Write(",\"borderColor\":");
		StreamUtil.WriteColorRGBA(stream, BorderColor);
		stream.Write(string.Format(",\"borderWidth\":{0}", BorderWidth));
		stream.Write("},");
		ChildrenToJson(stream);
		stream.Write('}');
	}
}
