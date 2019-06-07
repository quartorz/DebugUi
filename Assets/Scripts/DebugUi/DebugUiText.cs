using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DebugUiText : DebugUiControl
{
	public string Text;
	public Color32 TextColor;
	public PrimitiveRenderer2D.TextFormat Format;
	public float FontSize
	{
		get { return Format.FontSize; }
		set { Format.FontSize = value; }
	}
	public float LineHeight
	{
		get { return Format.LineHeight; }
		set { Format.LineHeight = value; }
	}

	public DebugUiText(string text = "", DebugUiManager manager = null) : base(manager)
	{
		Text = text;
		TextColor = new Color32(255, 255, 255, 255);
		Format = new PrimitiveRenderer2D.TextFormat()
		{
			Alignment = PrimitiveRenderer2D.Alignment.Left,
			VerticalAlignment = PrimitiveRenderer2D.VerticalAlignment.Top,
			Overflow = PrimitiveRenderer2D.Overflow.Nothing,
			FontStyle = FontStyle.Normal,
			FontSize = 10f,
			LineHeight = 15f,
		};
	}

	protected override void Draw(PrimitiveRenderer2D renderer)
	{
		var area = new Vector4(GlobalX, GlobalY, GlobalX + Width, GlobalY + Height);
		renderer.DrawText(Text, Format, area);
	}

	public override void ToJson(StreamWriter stream)
	{
		stream.Write(
			string.Format("{{" +
				"\"type\":\"text\"," +
				"\"id\":{0}," +
				"\"x\":{1}," +
				"\"y\":{2}," +
				"\"width\":{3}," +
				"\"height\":{4}," +
				"\"events\":[\"click\"]," +
				"\"attributes\":{{\"text\":",
			Id,
			LocalX,
			LocalY,
			Width,
			Height));
		StreamUtil.WriteEscapedString(stream, Text);
		stream.Write(string.Format(",\"fontSize\":{0}", FontSize));
		stream.Write(string.Format(",\"align\":\"{0}\"", (Format.Alignment == PrimitiveRenderer2D.Alignment.Left ? "left" : "center")));
		stream.Write(string.Format(",\"verticalAlign\":\"{0}\"", (Format.VerticalAlignment == PrimitiveRenderer2D.VerticalAlignment.Top ? "top" : "middle")));
		stream.Write("}}");
	}
}
