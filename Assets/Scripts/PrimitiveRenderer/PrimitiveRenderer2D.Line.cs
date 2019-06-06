using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PrimitiveRenderer2D : PrimitiveRenderer
{
	public void DrawHorizontalLine(float leftX, float y, float length, float width = 1)
	{
		width *= 0.5f;
		FillRectangle(new Vector4(leftX, y - width, leftX + length, y + width));
	}

	public void DrawVerticalLine(float x, float topY, float length, float width = 1)
	{
		width *= 0.5f;
		FillRectangle(new Vector4(x - width, topY, x + width, topY + length));
	}
}
