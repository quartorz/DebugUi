using System;
using UnityEngine;

public partial class PrimitiveRenderer2D
{
	/// <summary>
	/// 任意の凸な四角形を描画する。
	/// マスクが設定されていればクリップする。
	/// </summary>
	public void FillRectangle(ref Vertex v0, ref Vertex v1, ref Vertex v2, ref Vertex v3)
	{
		if (!hasMask)
		{
			var vertexCount = VertexCount;

			AddVertex(ref v0);
			AddVertex(ref v1);
			AddVertex(ref v2);
			AddVertex(ref v3);
			AddIndices(vertexCount, vertexCount + 1, vertexCount + 2, vertexCount + 3);

			return;
		}

		var list = new SmallList<Vertex>();
		list.Add(ref v0);
		list.Add(ref v1);
		list.Add(ref v2);
		list.Add(ref v3);

		SutherlandHodgman(ref list);
		AddPolygon(ref list);
	}

	/// <summary>
	/// 辺が軸に平行な長方形を描画する。
	/// マスクが設定されていればクリップする。
	/// 色はPrimitiveRenderer2D.Colorで指定する。
	/// </summary>
	public void FillRectangle(Vector4 rectangle, Vector2 uv)
	{
		Vector4 drawRect;

		if (hasMask)
		{
			if (mask.x > rectangle.z || rectangle.x > mask.z
				|| mask.y > rectangle.w || rectangle.y > mask.w)
			{
				return;
			}

			drawRect.x = Math.Max(mask.x, rectangle.x);
			drawRect.y = Math.Max(mask.y, rectangle.y);
			drawRect.z = Math.Min(mask.z, rectangle.z);
			drawRect.w = Math.Min(mask.w, rectangle.w);
		}
		else
		{
			drawRect = rectangle;
		}

		var vertexCount = VertexCount;
		var v = new Vertex
		{
			Position = new Vector3(rectangle.x, rectangle.y),
			Uv = uv,
			Color = Color,
		};
		AddVertex(ref v);
		v.Position = new Vector3(rectangle.z, rectangle.y);
		AddVertex(ref v);
		v.Position = new Vector3(rectangle.z, rectangle.w);
		AddVertex(ref v);
		v.Position = new Vector3(rectangle.x, rectangle.w);
		AddVertex(ref v);
		AddIndices(vertexCount, vertexCount + 1, vertexCount + 2, vertexCount + 3);
	}

	/// <summary>
	/// 辺が軸に平行な長方形を描画する。
	/// マスクが設定されていればクリップする。
	/// </summary>
	public void FillRectangle(
		ref Vector4 rectangle,
		ref Vector2 uv0, ref Vector2 uv1, ref Vector2 uv2, ref Vector2 uv3)
	{
		Vector4 drawRect;

		bool clipped = false;

		if (hasMask)
		{
			if (mask.x > rectangle.z || rectangle.x > mask.z
				|| mask.y > rectangle.w || rectangle.y > mask.w)
			{
				return;
			}

			clipped = true;

			drawRect.x = Math.Max(mask.x, rectangle.x);
			drawRect.y = Math.Max(mask.y, rectangle.y);
			drawRect.z = Math.Min(mask.z, rectangle.z);
			drawRect.w = Math.Min(mask.w, rectangle.w);
		}
		else
		{
			drawRect = rectangle;
		}

		var vertexCount = VertexCount;

		var v0 = new Vertex
		{
			Position = new Vector3(drawRect.x, drawRect.y),
			Uv = uv0,
			Color = Color,
		};
		var v1 = new Vertex
		{
			Position = new Vector3(drawRect.z, drawRect.y),
			Uv = uv1,
			Color = Color,
		};
		var v2 = new Vertex
		{
			Position = new Vector3(drawRect.z, drawRect.w),
			Uv = uv2,
			Color = Color,
		};
		var v3 = new Vertex
		{
			Position = new Vector3(drawRect.x, drawRect.w),
			Uv = uv3,
			Color = Color,
		};

		if (clipped)
		{
			var t0 = (drawRect.x - rectangle.x) / (rectangle.z - rectangle.x);
			if (t0 > 0f && t0 <= 1f)
			{
				v0.Uv = Vector2.Lerp(v0.Uv, v1.Uv, t0);
				v3.Uv = Vector2.Lerp(v3.Uv, v2.Uv, t0);
			}

			var t1 = (drawRect.y - rectangle.y) / (rectangle.w - rectangle.y);
			if (t1 > 0f && t1 <= 1f)
			{
				v0.Uv = Vector2.Lerp(v0.Uv, v3.Uv, t1);
				v1.Uv = Vector2.Lerp(v1.Uv, v2.Uv, t1);
			}

			var t2 = -(drawRect.z - rectangle.z) / (rectangle.z - rectangle.x);
			if (t2 > 0f && t2 <= 1f)
			{
				v1.Uv = Vector2.Lerp(v1.Uv, v0.Uv, t2);
				v2.Uv = Vector2.Lerp(v2.Uv, v3.Uv, t2);
			}

			var t3 = -(drawRect.w - rectangle.w) / (rectangle.w - rectangle.y);
			if (t3 > 0f && t3 <= 1f)
			{
				v2.Uv = Vector2.Lerp(v2.Uv, v1.Uv, t3);
				v3.Uv = Vector2.Lerp(v3.Uv, v0.Uv, t3);
			}
		}

		AddVertex(ref v0);
		AddVertex(ref v1);
		AddVertex(ref v2);
		AddVertex(ref v3);
		AddIndices(vertexCount, vertexCount + 1, vertexCount + 2, vertexCount + 3);
	}

	/// <summary>
	/// 辺が軸に平行な長方形を描画する。
	/// マスクが設定されていればクリップする。
	/// </summary>
	public void FillRectangle(
		ref Vector4 rectangle,
		ref Vector2 uv0, ref Vector2 uv1, ref Vector2 uv2, ref Vector2 uv3,
		ref Color32 c0, ref Color32 c1, ref Color32 c2, ref Color32 c3)
	{
		Vector4 drawRect;

		bool clipped = false;

		if (hasMask)
		{
			if (mask.x > rectangle.z || rectangle.x > mask.z
				|| mask.y > rectangle.w || rectangle.y > mask.w)
			{
				return;
			}

			clipped = true;

			drawRect.x = Math.Max(mask.x, rectangle.x);
			drawRect.y = Math.Max(mask.y, rectangle.y);
			drawRect.z = Math.Min(mask.z, rectangle.z);
			drawRect.w = Math.Min(mask.w, rectangle.w);
		}
		else
		{
			drawRect = rectangle;
		}

		var vertexCount = VertexCount;

		var v0 = new Vertex
		{
			Position = new Vector3(drawRect.x, drawRect.y),
			Uv = uv0,
			Color = c0,
		};
		var v1 = new Vertex
		{
			Position = new Vector3(drawRect.z, drawRect.y),
			Uv = uv1,
			Color = c1,
		};
		var v2 = new Vertex
		{
			Position = new Vector3(drawRect.z, drawRect.w),
			Uv = uv2,
			Color = c2,
		};
		var v3 = new Vertex
		{
			Position = new Vector3(drawRect.x, drawRect.w),
			Uv = uv3,
			Color = c3,
		};

		if (clipped)
		{
			var t0 = (drawRect.x - rectangle.x) / (rectangle.z - rectangle.x);
			if (t0 > 0f && t0 <= 1f)
			{
				v0.Uv = Vector2.Lerp(v0.Uv, v1.Uv, t0);
				v3.Uv = Vector2.Lerp(v3.Uv, v2.Uv, t0);
				v0.Color = Color32.Lerp(v0.Color, v1.Color, t0);
				v3.Color = Color32.Lerp(v3.Color, v2.Color, t0);
			}

			var t1 = (drawRect.y - rectangle.y) / (rectangle.w - rectangle.y);
			if (t1 > 0f && t1 <= 1f)
			{
				v0.Uv = Vector2.Lerp(v0.Uv, v3.Uv, t1);
				v1.Uv = Vector2.Lerp(v1.Uv, v2.Uv, t1);
				v0.Color = Color32.Lerp(v0.Color, v3.Color, t1);
				v1.Color = Color32.Lerp(v1.Color, v2.Color, t1);
			}

			var t2 = -(drawRect.z - rectangle.z) / (rectangle.z - rectangle.x);
			if (t2 > 0f && t2 <= 1f)
			{
				v1.Uv = Vector2.Lerp(v1.Uv, v0.Uv, t2);
				v2.Uv = Vector2.Lerp(v2.Uv, v3.Uv, t2);
				v1.Color = Color32.Lerp(v1.Color, v0.Color, t2);
				v2.Color = Color32.Lerp(v2.Color, v3.Color, t2);
			}

			var t3 = -(drawRect.w - rectangle.w) / (rectangle.w - rectangle.y);
			if (t3 > 0f && t3 <= 1f)
			{
				v2.Uv = Vector2.Lerp(v2.Uv, v1.Uv, t3);
				v3.Uv = Vector2.Lerp(v3.Uv, v0.Uv, t3);
				v2.Color = Color32.Lerp(v2.Color, v1.Color, t3);
				v3.Color = Color32.Lerp(v3.Color, v0.Color, t3);
			}
		}

		AddVertex(ref v0);
		AddVertex(ref v1);
		AddVertex(ref v2);
		AddVertex(ref v3);
		AddIndices(vertexCount, vertexCount + 1, vertexCount + 2, vertexCount + 3);
	}

	public void FillTriangle(Vector2 v1, Vector2 v2, Vector2 v3)
	{
		var vert1 = new Vertex()
		{
			Position = v1,
			Uv = whiteUv,
			Color = Color,
		};
		var vert2 = new Vertex()
		{
			Position = v2,
			Uv = whiteUv,
			Color = Color,
		};
		var vert3 = new Vertex()
		{
			Position = v3,
			Uv = whiteUv,
			Color = Color,
		};

		ResetTexture();
		FillTriangle(ref vert1, ref vert2, ref vert3);
	}

	public void FillRectangle(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4)
	{
		var vert1 = new Vertex()
		{
			Position = v1,
			Uv = whiteUv,
			Color = Color,
		};
		var vert2 = new Vertex()
		{
			Position = v2,
			Uv = whiteUv,
			Color = Color,
		};
		var vert3 = new Vertex()
		{
			Position = v3,
			Uv = whiteUv,
			Color = Color,
		};
		var vert4 = new Vertex()
		{
			Position = v4,
			Uv = whiteUv,
			Color = Color,
		};

		ResetTexture();
		FillRectangle(ref vert1, ref vert2, ref vert3, ref vert4);
	}

	public void FillRectangle(Vector4 rectangle)
	{
		ResetTexture();
		FillRectangle(rectangle, whiteUv);
	}

	/// <summary>
	/// 長方形の枠を描く。
	/// </summary>
	public void DrawRectangle(Vector4 rectangle, float lineWidth = 1f)
	{
		var halfWidth = lineWidth * 0.5f;
		var rectWidth = rectangle.z - rectangle.x;
		var rectHeight = rectangle.w - rectangle.y;

		DrawHorizontalLine(rectangle.x - halfWidth, rectangle.y, rectWidth + lineWidth, lineWidth);
		DrawHorizontalLine(rectangle.x - halfWidth, rectangle.w, rectWidth + lineWidth, lineWidth);
		DrawVerticalLine(rectangle.x, rectangle.y + halfWidth, rectHeight - lineWidth, lineWidth);
		DrawVerticalLine(rectangle.z, rectangle.y + halfWidth, rectHeight - lineWidth, lineWidth);
	}

	/// <summary>
	/// 長方形の枠を内側に描く
	/// </summary>
	public void DrawRectangleInside(Vector4 rectangle, float lineWidth = 1f)
	{
		var halfWidth = lineWidth * 0.5f;
		var rectWidth = rectangle.z - rectangle.x;
		var rectHeight = rectangle.w - rectangle.y;

		DrawHorizontalLine(rectangle.x, rectangle.y, rectWidth + halfWidth, lineWidth);
		DrawHorizontalLine(rectangle.x, rectangle.w, rectWidth + halfWidth, lineWidth);
		DrawVerticalLine(rectangle.x, rectangle.y + lineWidth, rectHeight - lineWidth, lineWidth);
		DrawVerticalLine(rectangle.z, rectangle.y + lineWidth, rectHeight - lineWidth, lineWidth);
	}
}
