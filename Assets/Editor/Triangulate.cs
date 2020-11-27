using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Vertex
{
    public Vector2 point;
    public int index;
}

public class TriangulateEditor
{
    //EAR CLIPPING 알고리즘 
    public static List<int> EarClipping(List<Vector2> points, int StartIndex = 0)
    {
        List<int> reVal = new List<int>();
        List<Vertex> vertex = new List<Vertex>();

        for (int i = 0; i < points.Count; ++i)
        {
            vertex.Add(new Vertex { point = points[i], index = i });
        }
        int index = 0;

        //EAR CLIPPING이 불가하다면 간단한 삼각화 알고리즘을 이용 
        int maxLoop = points.Count * points.Count;
        while (true)
        {
            if (maxLoop < index)
            {
                vertex.Clear();
                reVal.Clear();
                return SimpleTriangulate(points, StartIndex);
            }

            int index1 = KBMEditor.MathUtil.ListClamp(index - 1, vertex.Count);
            int index2 = KBMEditor.MathUtil.ListClamp(index, vertex.Count);
            int index3 = KBMEditor.MathUtil.ListClamp(index + 1, vertex.Count);

            Vector2 v1 = vertex[index1].point;
            Vector2 v2 = vertex[index2].point;
            Vector2 v3 = vertex[index3].point;

            if (vertex.Count <= 3)
            {
                if (KBMEditor.MathUtil.IsTriangleOrientedClockwise(v1, v2, v3))
                {
                    reVal.Add(vertex[index1].index + StartIndex);
                    reVal.Add(vertex[index2].index + StartIndex);
                    reVal.Add(vertex[index3].index + StartIndex);
                }
                else
                {
                    reVal.Add(vertex[index2].index + StartIndex);
                    reVal.Add(vertex[index1].index + StartIndex);
                    reVal.Add(vertex[index3].index + StartIndex);
                }
                break;
            }

            bool isInPoint = false;

            if (KBMEditor.MathUtil.IsConvex(v1, v2, v3))
            {
                for (int i = 0; i < vertex.Count; ++i)
                {
                    if (i == index1 || i == index2 || i == index3) continue;

                    if (KBMEditor.MathUtil.IsPointInTriangle(v1, v2, v3, vertex[i].point))
                    {
                        isInPoint = true;
                        break;
                    }
                }
                if (!isInPoint)
                {
                    if (KBMEditor.MathUtil.IsTriangleOrientedClockwise(v1, v2, v3))
                    {
                        reVal.Add(vertex[index1].index + StartIndex);
                        reVal.Add(vertex[index2].index + StartIndex);
                        reVal.Add(vertex[index3].index + StartIndex);
                    }
                    else
                    {
                        reVal.Add(vertex[index2].index + StartIndex);
                        reVal.Add(vertex[index1].index + StartIndex);
                        reVal.Add(vertex[index3].index + StartIndex);
                    }
                    vertex.RemoveAt(index2);
                }
            }
            index++;
        }
        return reVal;
    }

    //간단한 삼각화 알고리즘
    public static List<int> SimpleTriangulate(List<Vector2> convexHullpoints, int StartIndex = 0)
    {
        List<int> triangles = new List<int>();

        for (int i = 2; i < convexHullpoints.Count; i++)
        {
            triangles.Add(0 + StartIndex);
            triangles.Add(i - 1 + StartIndex);
            triangles.Add(i + StartIndex);
        }
        return triangles;
    }
}

