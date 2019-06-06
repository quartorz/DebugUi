using System;
using UnityEngine;

public partial class PrimitiveRenderer2D
{
	public void FillTriangle(ref Vertex v0, ref Vertex v1, ref Vertex v2)
	{
		if (!hasMask)
		{
			int vertexCount = VertexCount;

			AddVertex(ref v0);
			AddVertex(ref v1);
			AddVertex(ref v2);
			AddIndices(vertexCount, vertexCount + 1, vertexCount + 2);

			return;
		}

		var list = new SmallList<Vertex>();
		list.Add(ref v0);
		list.Add(ref v1);
		list.Add(ref v2);

		SutherlandHodgman(ref list);
		AddPolygon(ref list);
	}
}
