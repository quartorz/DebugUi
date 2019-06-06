using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class Test : MonoBehaviour
{
	[SerializeField] Shader textShader;
	[SerializeField] Shader texturedShader;
	[SerializeField] Font font;

	[SerializeField] new Camera camera;

	[SerializeField] bool useMask;
	[SerializeField] Vector4 mask;

	[SerializeField] Vector4 rectangle;

	[SerializeField] bool isTextCenter;
	[SerializeField] bool isTextMiddle;

	[SerializeField] ServerManager serverManager;

	PrimitiveRenderer2D primitiveRenderer;
	DebugUiManager debugUiManager;

	// Use this for initialization
	void Start()
	{
		if (primitiveRenderer == null)
		{
			primitiveRenderer = new PrimitiveRenderer2D(
				camera,
				textShader,
				texturedShader,
				font);
		}
		debugUiManager = DebugUiManager.Create(gameObject, primitiveRenderer);

		serverManager.debugUiManager = debugUiManager;

		var panel = new DebugUiPanel(manager: debugUiManager)
		{
			LocalX = 10f,
			LocalY = 10f,
			BackgroundColor = new Color32(0, 255, 0, 100),
			BorderColor = new Color32(0, 0, 0, 255)
		};

		var panel2 = new DebugUiPanel(manager: debugUiManager)
		{
			BackgroundColor = new Color32(255, 0, 255, 100),
			BorderColor = new Color32(0, 0, 0, 255)
		};

		var button = new DebugUiButton(debugUiManager)
		{
			LocalX = 100,
			LocalY = 200,
			Width = 100,
			Height = 50,
			Text = "ボタン1"
		};
		panel.AddChildAuto(button);

		button = new DebugUiButton(debugUiManager)
		{
			LocalX = 200,
			LocalY = 200,
			Width = 150,
			Height = 70,
			Text = "ボタン\n2"
		};
		panel.AddChildAuto(button);

		panel.AdjustSize();
		panel2.AddChild(panel);
		panel2.AdjustSize();
		debugUiManager.Add(panel2);

		debugUiManager.OutputHierarchy();
	}

	void OnDestroy()
	{
		if (primitiveRenderer != null)
		{
			primitiveRenderer.Dispose();
			primitiveRenderer = null;
		}

		debugUiManager.Dispose();
	}

	//void Start()
	//{
		//var start = Time.realtimeSinceStartup;

		//var a = new Vector3[100000];
		//for (int j = 0; j < 100; ++j)
		//{
		//	for (int i = 0; i < a.Length; ++i)
		//	{
		//		a[i] = Vector3.back;
		//	}
		//}

		//Debug.Log(Time.realtimeSinceStartup - start);

		//start = Time.realtimeSinceStartup;

		//var b = new List<Vector3>(100000);
		//for (int j = 0; j < 100; ++j)
		//{
		//	for (int i = 0; i < 100000; ++i)
		//	{
		//		b.Add(Vector3.back);
		//	}
		//	b.Clear();
		//}

		//Debug.Log(Time.realtimeSinceStartup - start);
		//Debug.Log(b.Capacity);
	//}

	void Update()
	{
		debugUiManager.ManualUpdate();
		return;
#if true
		var white = primitiveRenderer.WhiteUv;
		PrimitiveRenderer.Vertex v1, v2, v3;

		v1.Position = new Vector3(170, 30);
		v1.Uv = white;
		v1.Color = new Color32(0, 255, 0, 255);
		v2.Position = new Vector3(1100, 300);
		v2.Uv = white;
		v2.Color = new Color32(255, 0, 0, 0);
		v3.Position = new Vector3(300, 630);
		v3.Uv = white;
		v3.Color = new Color32(0, 0, 255, 255);

		if (useMask)
		{
			primitiveRenderer.Mask = mask;
		}

		primitiveRenderer.ResetTexture();
		primitiveRenderer.FillTriangle(ref v1, ref v2, ref v3);
		primitiveRenderer.Color = new Color32(0, 255, 255, 100);
		//primitiveRenderer.FillRectangle(ref rectangle, ref white, ref white, ref white, ref white, ref v1.Color, ref v3.Color, ref v2.Color, ref v2.Color);
		v1.Position = new Vector3(rectangle.x, rectangle.y);
		v2.Position = new Vector3(rectangle.z, rectangle.y);
		v2.Color = v3.Color;
		v3.Position = new Vector3(rectangle.z, rectangle.w);
		v3.Color = new Color32(255, 0, 0, 0);
		var v4 = new PrimitiveRenderer.Vertex
		{
			Position = new Vector3(rectangle.x, rectangle.w),
			Uv = white,
			Color = v3.Color,
		};
		primitiveRenderer.Color = new Color32(255, 255, 255, 255);
		primitiveRenderer.FillRectangle(ref v1, ref v2, ref v3, ref v4);
		primitiveRenderer.DrawText("Text 1\nText 2\nText Text Text", new PrimitiveRenderer2D.TextFormat
		{
			FontSize = 40,
			LineHeight = 60,
			Alignment = isTextCenter ? PrimitiveRenderer2D.Alignment.Center : PrimitiveRenderer2D.Alignment.Left,
			VerticalAlignment = isTextMiddle ? PrimitiveRenderer2D.VerticalAlignment.Middle : PrimitiveRenderer2D.VerticalAlignment.Top,
		}, mask);
		primitiveRenderer.Mask = null;

		primitiveRenderer.Color = new Color32(255, 255, 255, 100);
		primitiveRenderer.DrawRectangle(mask, 10);
#endif
	}

	void LateUpdate()
	{
		//PrimitiveRenderer2D.TextFormat.Default.Alignment = PrimitiveRenderer2D.Alignment.Center;
		primitiveRenderer.Draw();
	}
}
