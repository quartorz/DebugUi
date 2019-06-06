using System;
using System.Collections.Generic;
using UnityEngine;

public class DebugUiColoredControl : DebugUiControl
{
	public Color32 BorderColor;
	public Color32 BackgroundColor;

	public DebugUiColoredControl(DebugUiManager manager = null) : base(manager)
	{
	}

	protected override void Draw(PrimitiveRenderer2D renderer)
	{
		var area = new Vector4(GlobalX, GlobalY, GlobalX + Width, GlobalY + Height);
		renderer.Color = BackgroundColor;
		renderer.FillRectangle(area);
		renderer.Color = BorderColor;
		renderer.DrawRectangleInside(area, BorderWidth);
	}
}
