using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class PrimitiveRenderer2D : PrimitiveRenderer
{
	public enum Alignment
	{
		Left,
		Center,
		//Right,
	}

	public enum VerticalAlignment
	{
		Top,
		Middle,
		//Bottom,
	}

	public enum Overflow
	{
		Nothing,
		//Wrap,
		Shrink
	}

	float toCoordScale;
	float toCoordOffsetX;
	float toCoordOffsetY;

	public float ReferenceScreenWidth { get; private set; }
	public float ReferenceScreenHeight { get; private set; }
	public float ToVirtualScreenScale { get; private set; }

	public Color32 Color;

	public PrimitiveRenderer2D(
		Camera camera,
		Shader textShader,
		Shader texturedShader,
		Font font,
		int capacity = DefaultTriangleCapacity)
		: base(camera, textShader, texturedShader, font, capacity)
	{
		if (camera.aspect >= 16f / 9f)
		{
			SetReferenceScreenHeight(640); // 仮
		}
		else
		{
			SetReferenceScreenWidth(1136);
		}
	}

	//void ToCoord(ref float x, ref float y)
	//{
	//	x *= toCoordScale;
	//	x += toCoordOffsetX;
	//	y *= -toCoordScale;
	//	y += toCoordOffsetY;
	//}

	//void ToCoord(ref float x, ref float y)
	//{
	//	x -= toCoordOffsetX;
	//	x *= rcpToCoordScale;
	//	y -= toCoordOffsetY;
	//	y *= -rcpToCoordScale;
	//}

	public void SetReferenceScreenHeight(int rHeight)
	{
		ReferenceScreenHeight = (float)rHeight;
		int screenWidth = Screen.width;
		int screenHeight = Screen.height;
		ReferenceScreenWidth = ReferenceScreenHeight * (float)screenWidth / (float)screenHeight;

		ToVirtualScreenScale = ReferenceScreenHeight / screenHeight;

		InitializeTransform();
	}

	public void SetReferenceScreenWidth(int rWidth)
	{
		ReferenceScreenWidth = (float)rWidth;
		int screenWidth = Screen.width;
		int screenHeight = Screen.height;
		ReferenceScreenHeight = ReferenceScreenWidth * (float)screenHeight / (float)screenWidth;

		ToVirtualScreenScale = ReferenceScreenWidth / screenWidth;

		InitializeTransform();
	}

	void InitializeTransform()
	{
		if (camera == null)
		{
			return;
		}

		float cameraScreenHalfHeight = camera.orthographicSize;
		float cameraScreenHalfWidth = cameraScreenHalfHeight * ReferenceScreenWidth / ReferenceScreenHeight;

		toCoordScale = 2f * cameraScreenHalfHeight / ReferenceScreenHeight;
		toCoordOffsetX = -cameraScreenHalfWidth;
		toCoordOffsetY = cameraScreenHalfHeight;

		Matrix = Matrix4x4.Translate(new Vector3(toCoordOffsetX, toCoordOffsetY)) * Matrix4x4.Scale(new Vector3(toCoordScale, -toCoordScale));
	}
}
