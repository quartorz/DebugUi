using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PrimitiveRenderer : IDisposable
{
	struct SubMesh : IDisposable
	{
		public Material Material;
		public Texture Texture;
		public int StartIndex;
		public Matrix4x4 Matrix;

		public void Dispose()
		{
			Material = null;
			Texture = null;
		}
	}

	struct SubMeshList : IDisposable
	{
		public int Count { get; private set; }

		SubMesh[] subMeshes;
		int capacity;

		public SubMeshList(int capacity)
		{
			Count = 0;
			subMeshes = new SubMesh[capacity];
			this.capacity = capacity;
		}

		public void Dispose()
		{
			for (int i = 0; i < subMeshes.Length; ++i)
			{
				subMeshes[i].Dispose();
			}
			subMeshes = null;
		}

		public void Add(int startIndex)
		{
			if (++Count > capacity)
			{
				capacity *= 2;
				Array.Resize(ref subMeshes, capacity);
			}

			subMeshes[Count - 1].StartIndex = startIndex;
		}
		public void Clear()
		{
			Count = 0;
		}

		public void SetMaterial(int index, Material material)
		{
			subMeshes[index].Material = material;
		}
		public void SetTexture(int index, Texture texture)
		{
			subMeshes[index].Texture = texture;
		}
		public void SetMatrix(int index, ref Matrix4x4 matrix)
		{
			subMeshes[index].Matrix = matrix;
		}

		public Material GetMaterial(int index)
		{
			return subMeshes[index].Material;
		}
		public Texture GetTexture(int index)
		{
			return subMeshes[index].Texture;
		}
		public int GetStartIndex(int index)
		{
			return subMeshes[index].StartIndex;
		}
		public void GetMatrix(int index, out Matrix4x4 matrix)
		{
			matrix = subMeshes[index].Matrix;
		}
	}

	/// <summary>
	/// Sutherland-Hodgmanアルゴリズムをallocせずにするために作ったけど、stackallocとunsafeを使えばいらない気がする。
	/// </summary>
	protected struct SmallList<T> where T : struct
	{
		public int Count { get; private set; }

		T v0, v1, v2, v3, v4, v5, v6, v7;

		public void Clear()
		{
			Count = 0;
		}

		public void Add(ref T value)
		{
			Set(Count++, ref value);
		}

		public void Set(int i, ref T value)
		{
			switch (i & 7)
			{
				case 0:
					v0 = value;
					return;
				case 1:
					v1 = value;
					return;
				case 2:
					v2 = value;
					return;
				case 3:
					v3 = value;
					return;
				case 4:
					v4 = value;
					return;
				case 5:
					v5 = value;
					return;
				case 6:
					v6 = value;
					return;
				case 7:
					v7 = value;
					return;
				default:
					System.Diagnostics.Debug.Assert(false);
					return;
			}
		}

		public void Get(int i, out T value)
		{
			switch (i & 7)
			{
				case 0:
					value = v0;
					return;
				case 1:
					value = v1;
					return;
				case 2:
					value = v2;
					return;
				case 3:
					value = v3;
					return;
				case 4:
					value = v4;
					return;
				case 5:
					value = v5;
					return;
				case 6:
					value = v6;
					return;
				case 7:
					value = v7;
					return;
				default:
					System.Diagnostics.Debug.Assert(false);
					value = default(T);
					return;
			}
		}
	}

	public struct Vertex
	{
		public Vector3 Position;
		public Vector2 Uv;
		public Color32 Color;

		public static void Lerp(ref Vertex v1, ref Vertex v2, float t, out Vertex vertex)
		{
			vertex.Position = Vector3.Lerp(v1.Position, v2.Position, t);
			vertex.Uv = Vector2.Lerp(v1.Uv, v2.Uv, t);
			vertex.Color = Color32.Lerp(v1.Color, v2.Color, t);
		}
	}

	protected const int DefaultTriangleCapacity = 1024;
	private const int InitialSubMeshCapacity = 16;
	protected const int FontSize = 20;

	Material textMaterial;
	Material texturedMaterial;
	Mesh mesh;

	SubMeshList subMeshes;
	Texture currentTexture;

	protected Font font;
	Texture fontTexture
	{
		get
		{
			return font.material.mainTexture;
		}
	}

	public Vector2 WhiteUv
	{
		get
		{
			return whiteUv;
		}
		private set
		{
			whiteUv = value;
		}
	}
	protected Vector2 whiteUv;
	List<Vector2> uvs;
	List<Vector3> vertices;
	List<Color32> colors;
	List<int> indices;
	List<int> temporaryIndices;
	CommandBuffer commandBuffer = new CommandBuffer();
	int propertyId;
	MaterialPropertyBlock materialPropertyBlock;

	Matrix4x4 matrix;
	protected Matrix4x4 Matrix
	{
		get { return matrix; }
		set
		{
			matrix = value;
		}
	}

	protected int VertexCount { get { return vertices.Count; } }

	protected Camera camera;

	public PrimitiveRenderer(
		Camera camera,
		Shader textShader,
		Shader texturedShader,
		Font font,
		int capacity = DefaultTriangleCapacity)
	{
		mesh = new Mesh();
		mesh.MarkDynamic();

		subMeshes = new SubMeshList(InitialSubMeshCapacity + 1);

		this.camera = camera;
		this.font = font;

		Font.textureRebuilt += OnFontTextureRebuilt;
		font.RequestCharactersInTexture("■");
		OnFontTextureRebuilt(font);

		camera.AddCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);

		textMaterial = new Material(textShader);
		texturedMaterial = new Material(texturedShader);

		uvs = new List<Vector2>(capacity);
		vertices = new List<Vector3>(capacity);
		colors = new List<Color32>(capacity);
		indices = new List<int>(capacity);
		temporaryIndices = new List<int>(capacity);

		propertyId = Shader.PropertyToID("_MainTex");
		materialPropertyBlock = new MaterialPropertyBlock();

		Matrix = Matrix4x4.identity;
	}

	public void Dispose()
	{
		Font.textureRebuilt -= OnFontTextureRebuilt;
		subMeshes.Dispose();
		if (camera != null)
		{
			camera.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);
			camera = null;
		}
		currentTexture = null;
		mesh = null;
		materialPropertyBlock = null;
		uvs = null;
		vertices = null;
		colors = null;
		indices = null;
		temporaryIndices = null;
		font = null;
		textMaterial = null;
		texturedMaterial = null;
	}

	public void Draw()
	{
		commandBuffer.Clear();
		mesh.Clear();

		if (subMeshes.Count > 0)
		{
			subMeshes.Add(indices.Count);

			mesh.subMeshCount = subMeshes.Count;

			mesh.SetVertices(vertices);
			mesh.SetUVs(0, uvs);
			mesh.SetColors(colors);

			int startIndex = subMeshes.GetStartIndex(0);

			for (int i = 0; i < subMeshes.Count - 1; ++i)
			{
				var material = subMeshes.GetMaterial(i);
				var texture = subMeshes.GetTexture(i);
				var end = subMeshes.GetStartIndex(i + 1);
				Matrix4x4 matrix;
				subMeshes.GetMatrix(i, out matrix);

				var a = new SubMesh();
				a.Matrix = new Matrix4x4();

				for (int j = startIndex; j < end; ++j)
				{
					//indices.CopyTo()
					temporaryIndices.Add(indices[j]);
				}

				mesh.SetTriangles(temporaryIndices, i, false);
				temporaryIndices.Clear();

				materialPropertyBlock.SetTexture(propertyId, texture);
				commandBuffer.DrawMesh(mesh, matrix, material, i, -1, materialPropertyBlock);

				startIndex = end;
			}
		}

		vertices.Clear();
		indices.Clear();
		uvs.Clear();
		colors.Clear();
		subMeshes.Clear();
		currentTexture = null;
	}

	/// <summary>
	/// ■の中心のUVを取り直す
	/// </summary>
	void OnFontTextureRebuilt(Font font)
	{
		if (font == this.font)
		{
			CharacterInfo ch;
			this.font.GetCharacterInfo('■', out ch);
			WhiteUv = ch.uvTopLeft;
			WhiteUv += ch.uvBottomRight;
			WhiteUv *= 0.5f;
		}
	}

	public void SetTexture(Texture texture)
	{
		currentTexture = texture;
		AddSubMesh();
	}

	public void ResetTexture()
	{
		SetTexture(fontTexture);
	}

	void AddSubMesh()
	{
		if (subMeshes.Count == 0 || subMeshes.GetStartIndex(subMeshes.Count - 1) != indices.Count)
		{
			subMeshes.Add(indices.Count);
		}

		subMeshes.SetTexture(subMeshes.Count - 1, currentTexture);
		subMeshes.SetMatrix(subMeshes.Count - 1, ref matrix);

		if (currentTexture == fontTexture)
		{
			subMeshes.SetMaterial(subMeshes.Count - 1, textMaterial);
		}
		else
		{
			subMeshes.SetMaterial(subMeshes.Count - 1, texturedMaterial);
		}
	}

	public void AddVertex(ref Vertex vertex)
	{
		vertices.Add(vertex.Position);
		uvs.Add(vertex.Uv);
		colors.Add(vertex.Color);
	}

	public void AddVertex(Vertex vertex)
	{
		AddVertex(ref vertex);
	}

	public void AddIndices(int i0, int i1, int i2)
	{
		indices.Add(i0);
		indices.Add(i1);
		indices.Add(i2);
	}

	public void AddIndices(int i0, int i1, int i2, int i3)
	{
		indices.Add(i0);
		indices.Add(i1);
		indices.Add(i2);
		indices.Add(i0);
		indices.Add(i2);
		indices.Add(i3);
	}

	protected void AddPolygon(ref SmallList<Vertex> vertices)
	{
		if (vertices.Count <= 2)
		{
			return;
		}

		int vertexCount = VertexCount;
		Vertex vertex;

		vertices.Get(0, out vertex);
		AddVertex(ref vertex);
		vertices.Get(1, out vertex);
		AddVertex(ref vertex);

		for (int i = 3; i < vertices.Count; i += 2)
		{
			vertices.Get(i - 1, out vertex);
			AddVertex(ref vertex);
			vertices.Get(i, out vertex);
			AddVertex(ref vertex);
			AddIndices(vertexCount, vertexCount + i - 2, vertexCount + i - 1, vertexCount + i);
		}

		if (vertices.Count % 2 == 1)
		{
			vertices.Get(vertices.Count - 1, out vertex);
			AddVertex(ref vertex);
			AddIndices(vertexCount, vertexCount + vertices.Count - 2, vertexCount + vertices.Count - 1);
		}
	}
}