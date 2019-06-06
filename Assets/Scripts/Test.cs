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

	void Update()
	{
		debugUiManager.ManualUpdate();
	}

	void LateUpdate()
	{
		primitiveRenderer.Draw();
	}
}
