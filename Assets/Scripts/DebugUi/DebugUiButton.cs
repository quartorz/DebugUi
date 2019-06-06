using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DebugUiButton : DebugUiColoredControl
{
	public string Text;
	public Color32 TextColor;
	public float FontSize;

	public DebugUiButton(DebugUiManager manager = null) : base(manager)
	{
		BorderWidth = 1f;
		BackgroundColor = new Color32(100, 100, 100, 100);
		BorderColor = new Color32(255, 255, 255, 255);
		TextColor = new Color32(255, 255, 255, 255);
		FontSize = 10f;

		OnClick = () =>
		{
			Debug.LogFormat("Button.OnClick: id: {0}", Id);
		};
	}

	protected override void Draw(PrimitiveRenderer2D renderer)
	{
		base.Draw(renderer);

		var area = new Vector4(GlobalX, GlobalY, GlobalX + Width, GlobalY + Height);

		renderer.DrawText(Text, new PrimitiveRenderer2D.TextFormat()
		{
			Alignment = PrimitiveRenderer2D.Alignment.Center,
			VerticalAlignment = PrimitiveRenderer2D.VerticalAlignment.Middle,
			Overflow = PrimitiveRenderer2D.Overflow.Shrink,
			FontSize = FontSize,
			LineHeight = FontSize * 1.5f,
		}, area);
	}

	public override void ToJson(StreamWriter stream)
	{
		stream.Write(
			string.Format("{{" +
				"\"type\":\"button\"," +
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
		stream.Write("}}");
	}
}
