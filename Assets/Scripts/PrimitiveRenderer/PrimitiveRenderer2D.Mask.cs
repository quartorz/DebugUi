using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PrimitiveRenderer2D : PrimitiveRenderer
{
	public Vector4? Mask
	{
		get
		{
			return (hasMask ? new Vector4?(mask) : null);
		}
		set
		{
			if (value == null)
			{
				hasMask = false;
			}
			else
			{
				hasMask = true;
				mask = value.Value;
			}
		}
	}

	bool hasMask;
	Vector4 mask;

	/// <summary>
	/// Sutherland-Hodgman algorithmでクリップする。
	/// </summary>
	/// <param name="list">三角形か凸な四角形</param>
	void SutherlandHodgman(ref SmallList<Vertex> list)
	{
		var list2 = new SmallList<Vertex>();
		Vertex vertex, prev, temporary;

		list.Get(list.Count - 1, out prev);
		for (int i = 0; i < list.Count; ++i)
		{
			list.Get(i, out vertex);

			if (vertex.Position.x >= mask.x)
			{
				if (prev.Position.x < mask.x)
				{
					var t = (mask.x - vertex.Position.x) / (prev.Position.x - vertex.Position.x);
					Vertex.Lerp(ref vertex, ref prev, t, out temporary);
					list2.Add(ref temporary);
				}

				list2.Add(ref vertex);
			}
			else if (prev.Position.x >= mask.x)
			{
				var t = (mask.x - vertex.Position.x) / (prev.Position.x - vertex.Position.x);
				Vertex.Lerp(ref vertex, ref prev, t, out temporary);
				list2.Add(ref temporary);
			}

			prev = vertex;
		}

		list.Clear();

		if (list2.Count <= 2)
		{
			return;
		}

		list2.Get(list2.Count - 1, out prev);
		for (int i = 0; i < list2.Count; ++i)
		{
			list2.Get(i, out vertex);

			if (vertex.Position.y >= mask.y)
			{
				if (prev.Position.y < mask.y)
				{
					var t = (mask.y - vertex.Position.y) / (prev.Position.y - vertex.Position.y);
					Vertex.Lerp(ref vertex, ref prev, t, out temporary);
					list.Add(ref temporary);
				}

				list.Add(ref vertex);
			}
			else if (prev.Position.y >= mask.y)
			{
				var t = (mask.y - vertex.Position.y) / (prev.Position.y - vertex.Position.y);
				Vertex.Lerp(ref vertex, ref prev, t, out temporary);
				list.Add(ref temporary);
			}

			prev = vertex;
		}

		if (list.Count <= 2)
		{
			return;
		}

		list2.Clear();
		list.Get(list.Count - 1, out prev);
		for (int i = 0; i < list.Count; ++i)
		{
			list.Get(i, out vertex);

			if (vertex.Position.x <= mask.z)
			{
				if (prev.Position.x > mask.z)
				{
					var t = (mask.z - vertex.Position.x) / (prev.Position.x - vertex.Position.x);
					Vertex.Lerp(ref vertex, ref prev, t, out temporary);
					list2.Add(ref temporary);
				}

				list2.Add(ref vertex);
			}
			else if (prev.Position.x <= mask.z)
			{
				var t = (mask.z - vertex.Position.x) / (prev.Position.x - vertex.Position.x);
				Vertex.Lerp(ref vertex, ref prev, t, out temporary);
				list2.Add(ref temporary);
			}

			prev = vertex;
		}

		list.Clear();

		if (list2.Count <= 2)
		{
			return;
		}

		list2.Get(list2.Count - 1, out prev);
		for (int i = 0; i < list2.Count; ++i)
		{
			list2.Get(i, out vertex);

			if (vertex.Position.y <= mask.w)
			{
				if (prev.Position.y > mask.w)
				{
					var t = (mask.w - vertex.Position.y) / (prev.Position.y - vertex.Position.y);
					Vertex.Lerp(ref vertex, ref prev, t, out temporary);
					list.Add(ref temporary);
				}

				list.Add(ref vertex);
			}
			else if (prev.Position.y <= mask.w)
			{
				var t = (mask.w - vertex.Position.y) / (prev.Position.y - vertex.Position.y);
				Vertex.Lerp(ref vertex, ref prev, t, out temporary);
				list.Add(ref temporary);
			}

			prev = vertex;
		}
	}
}
