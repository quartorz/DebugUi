using System;
using UnityEngine;

public partial class PrimitiveRenderer2D
{
	public struct TextFormat
	{
		public Alignment Alignment;
		public VerticalAlignment VerticalAlignment;
		public Overflow Overflow;
		public UnityEngine.FontStyle FontStyle;
		public float FontSize;
		public float LineHeight;
	}

	static readonly TextFormat DefaultTextFormat = new TextFormat()
	{
		Alignment = Alignment.Left,
		VerticalAlignment = VerticalAlignment.Top,
		Overflow = Overflow.Nothing,
		FontStyle = FontStyle.Normal,
		FontSize = 20f,
		LineHeight = 30f,
	};

	public void DrawText(string text, TextFormat? format, Vector4 area)
	{
		TextFormat textFormat;
		if (format.HasValue)
		{
			textFormat = format.Value;
		}
		else
		{
			textFormat = DefaultTextFormat;
		}

		font.RequestCharactersInTexture(text, FontSize, textFormat.FontStyle);

		var size = new Vector2();
		if (textFormat.Overflow == Overflow.Shrink || textFormat.VerticalAlignment == VerticalAlignment.Middle)
		{
			MeasureText(text, textFormat, out size);
		}

		if (textFormat.Overflow == Overflow.Shrink)
		{
			var areaWidth = area.z - area.x;
			var areaHeight = area.w - area.y;

			var scaleX = 1f;
			var scaleY = 1f;

			if (size.x > areaWidth)
			{
				scaleX = areaWidth / size.x;
			}

			if (size.y > areaHeight)
			{
				scaleY = areaHeight / size.y;
			}

			var scale = Math.Max(scaleX, scaleY);
			if (scale != 1f)
			{
				textFormat.FontSize *= scale;
				textFormat.LineHeight *= scale;
			}
		}

		if (textFormat.VerticalAlignment == VerticalAlignment.Middle)
		{
			area.y += ((area.w - area.y) - size.y) / 2f;
		}

		var fontScale = textFormat.FontSize / font.lineHeight;

		ResetTexture();

		switch (textFormat.Alignment)
		{
			case Alignment.Left:
				area.y += (textFormat.LineHeight - textFormat.FontSize) / 2f;
				DrawTextInternal(text, 0, text.Length, textFormat, area, fontScale);
				break;
			case Alignment.Center:
				var start = 0;
				var length = text.Length;
				Vector2 lineSize;
				var topLeft = new Vector2(0, area.y + (textFormat.LineHeight - textFormat.FontSize) / 2f);
				var areaWidth = area.z - area.x;
				while (true)
				{
					var end = text.IndexOf('\n', start);
					var endOfString = false;
					if (end == -1)
					{
						endOfString = true;
						end = length;
					}

					MeasureText(text, start, end, textFormat, out lineSize, true);
					topLeft.x = area.x + (areaWidth - lineSize.x) / 2f;
					DrawTextInternal(text, start, end, textFormat, topLeft, fontScale);

					if (endOfString)
					{
						break;
					}

					topLeft.y += textFormat.LineHeight;
					start = end + 1;
				}
				break;
		}
	}

	void DrawTextInternal(string text, int start, int end, TextFormat format, Vector2 topLeft, float scale)
	{
		Vector4 rect;
		Vector2 uv0, uv1, uv2, uv3;

		var left = topLeft.x;
		topLeft.y += format.FontSize;

		for (var i = start; i < end; ++i)
		{
			var ch = text[i];
			if (char.IsControl(ch))
			{
				if (ch == '\n')
				{
					topLeft.y += format.LineHeight;
					topLeft.x = left;
				}

				continue;
			}

			CharacterInfo info;

			if (font.GetCharacterInfo(ch, out info, FontSize, format.FontStyle))
			{
				rect = new Vector4(topLeft.x + info.minX * scale, topLeft.y - info.maxY * scale, topLeft.x + info.maxX * scale, topLeft.y - info.minY * scale);
				uv0 = info.uvTopLeft;
				uv1 = info.uvTopRight;
				uv2 = info.uvBottomRight;
				uv3 = info.uvBottomLeft;
				FillRectangle(ref rect, ref uv0, ref uv1, ref uv2, ref uv3);
				topLeft.x += info.advance * scale;
			}
		}
	}

	public void MeasureText(string text, TextFormat? format, out Vector2 result)
	{
		if (string.IsNullOrEmpty(text))
		{
			result = new Vector2();
			return;
		}

		MeasureText(text, 0, text.Length, format, out result);
	}

	/// <summary>
	/// テキストの大きさを測る。
	/// [start, end)。
	/// </summary>
	public void MeasureText(string text, int start, int end, TextFormat? format, out Vector2 size, bool dontRequest = false)
	{
		size = new Vector2();

		if (string.IsNullOrEmpty(text))
		{
			return;
		}

		TextFormat textFormat;
		if (format.HasValue)
		{
			textFormat = format.Value;
		}
		else
		{
			textFormat = DefaultTextFormat;
		}

		if (!dontRequest)
		{
			font.RequestCharactersInTexture(text, FontSize, textFormat.FontStyle);
		}

		size.y = textFormat.LineHeight;

		var maxSizeX = 0;
		var sizeX = 0;
		for (var i = start; i < end; ++i)
		{
			var ch = text[i];

			if (char.IsControl(ch))
			{
				if (ch == '\n')
				{
					if (sizeX > maxSizeX)
					{
						maxSizeX = sizeX;
					}

					size.y += textFormat.LineHeight;
					sizeX = 0;
				}

				continue;
			}

			CharacterInfo info;
			if (font.GetCharacterInfo(ch, out info, FontSize, textFormat.FontStyle))
			{
				sizeX += info.advance;
			}
		}

		if (sizeX > maxSizeX)
		{
			maxSizeX = sizeX;
		}

		size.x = maxSizeX * (textFormat.FontSize / (float)font.lineHeight);
	}
}
