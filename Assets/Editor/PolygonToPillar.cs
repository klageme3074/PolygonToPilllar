using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using KBMEditor;

public class PolygonToPillar : EditorWindow
{
    GameObject parantsObj;
    GameObject DataInstance;
    public Object buildingcsvFile;
    public Object roadcsvFile;
    public Object buildingMat;
    public Object roadMat;
    public List<List<Vector2>> positions = new List<List<Vector2>>();

    // Start is called before the first frame update
    [MenuItem("Window/Polygon To Pillar")]
    static void Init()
    {
        PolygonToPillar window = (PolygonToPillar)GetWindow(typeof(PolygonToPillar));
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal("box");
        GUILayout.Label("BuildingcsvFile");
        buildingcsvFile = EditorGUILayout.ObjectField(buildingcsvFile, typeof(TextAsset),true);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("box");
        GUILayout.Label("BuildingMaterial");
        buildingMat = EditorGUILayout.ObjectField(buildingMat, typeof(Material), true);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("box");
        GUILayout.Label("RoadcsvFile");
        roadcsvFile = EditorGUILayout.ObjectField(roadcsvFile, typeof(TextAsset), true);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("box");
        GUILayout.Label("RoadMaterial");
        roadMat = EditorGUILayout.ObjectField(roadMat, typeof(Material), true);
        GUILayout.EndHorizontal();

        GUILayout.BeginVertical("box");
        if (GUILayout.Button("BuildingCreate"))
        {
            parantsObj = new GameObject("Buildings");
            Parser(buildingcsvFile);
            SetBuilding();
        }
        
        GUILayout.BeginVertical("box");
        if (GUILayout.Button("RoadCreate"))
        {
            parantsObj = new GameObject("Roads");
            Parser(roadcsvFile);
            SetRoad();
        }
        GUILayout.EndVertical();
        this.Repaint();
    } 

    //파싱
    private void Parser(Object csvfile)
    {
        float maxX = float.MinValue;
        float maxY = float.MinValue;
        float minX = float.MaxValue;
        float minY = float.MaxValue;

        if (csvfile != null)
        {
            StringReader sr = new StringReader((csvfile as TextAsset).text);

            string line;

            while ((line = sr.ReadLine()) != null)
            {
                string[] holeTest = line.Split(')');
                string[] lineSplit = holeTest[0].Split(',');
                lineSplit[0] = lineSplit[0].Replace("\"", "");
                lineSplit[lineSplit.Length - 1] = lineSplit[lineSplit.Length - 1].Replace("\"", "");

                List<Vector2> tmpVecs = new List<Vector2>();

                for (int i = 0; i < lineSplit.Length; ++i)
                {
                    string[] str = lineSplit[i].Split(' ');
                    if (splitCheck(ref str))
                    {
                        Vector2 tV = new Vector2(float.Parse(str[0]), float.Parse(str[1]));

                        tmpVecs.Add(tV);

                        if (tV.x > maxX) maxX = tV.x;
                        if (tV.y > maxY) maxY = tV.y;
                        if (tV.x < minX) minX = tV.x;
                        if (tV.y < minY) minY = tV.y;
                    }
                }
                positions.Add(tmpVecs);
            }

            Vector2 avgVec = new Vector2((maxX + minX) * 0.5f, (maxY + minY) * 0.5f); 
            
            for (int i = 0; i < positions.Count; ++i)
            {
                for (int j = 0; j < positions[i].Count; ++j)
                {
                    positions[i][j] *= 0.1f;
                }
            }
        }
        else
        {
            Debug.LogError("setting CSV file");
        }
    }
    
    bool splitCheck(ref string[] str)
    {
        float sss;
        if (str != null
            && str.Length > 1
            && !string.IsNullOrEmpty(str[0])
            && float.TryParse(str[0], out sss)
            && float.TryParse(str[1], out sss)
            )
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //건물 세우기
    private void SetBuilding()
    {
        List<int> CombineIndices = new List<int>();
        List<Vector3> CombineVertices = new List<Vector3>();
        List<Vector3> CombineNormals = new List<Vector3>();

        int count = 0;

        foreach (List<Vector2> vectors in positions)
        {
            List<int> indices = TriangulateEditor.EarClipping(vectors, CombineVertices.Count);

            List<Vector3> vertices = Vector2ToVector3(vectors.ToArray());
            int verticesCount = vertices.Count;
            Vector3[] tmpVec = vertices.ToArray();

            //노말값을 위해 버텍스 복제 추가
            vertices.AddRange(tmpVec);
            vertices.AddRange(tmpVec);
            vertices.AddRange(tmpVec);
            vertices.AddRange(tmpVec);

            Vector3[] normals = new Vector3[vertices.Count];

            //기둥으로 만들기 위해 버텍스 좌표계산
            float rand = Random.Range(1f, 3f);
            for (int i = 0; i < verticesCount * 3; ++i)
            {
                vertices[i] += Vector3.up * rand;
            }

            //윗 평면 노말
            for (int i = 0; i < verticesCount; ++i)
            {
                normals[i] = Vector3.up;
            }

            int LU;
            int RU;
            int LD;
            int RD;

            //옆 평면 노말 계산 및 인덱스 버퍼 계산
            for (int i = 0; i < verticesCount; ++i)
            {
                Vector3 n = Vector3.Cross(vertices[ClampIndex(i + 1, verticesCount)] - vertices[i], Vector3.up).normalized;

                if (i % 2 == 1)
                {
                    LU = ClampIndex(i, verticesCount, verticesCount * 2);
                    RU = ClampIndex(i + 1, verticesCount, verticesCount * 2);
                    LD = ClampIndex(i, verticesCount, verticesCount * 4);
                    RD = ClampIndex(i + 1, verticesCount, verticesCount * 4);
                }
                else
                {
                    LU = ClampIndex(i, verticesCount, verticesCount);
                    RU = ClampIndex(i + 1, verticesCount, verticesCount);
                    LD = ClampIndex(i, verticesCount, verticesCount * 3);
                    RD = ClampIndex(i + 1, verticesCount, verticesCount * 3);
                }

                normals[LU] = n;
                normals[RU] = n;
                normals[LD] = n;
                normals[RD] = n;

                indices.Add(LU + CombineVertices.Count);
                indices.Add(LD + CombineVertices.Count);
                indices.Add(RU + CombineVertices.Count);

                indices.Add(LD + CombineVertices.Count);
                indices.Add(RD + CombineVertices.Count);
                indices.Add(RU + CombineVertices.Count);
            }
            CombineVertices.AddRange(vertices);
            CombineIndices.AddRange(indices);
            CombineNormals.AddRange(normals);

            //500개 씩 mesh combine
            if (count % 500 == 0)
            {
                CreateMesh((Material)buildingMat,CombineVertices.ToArray(), CombineNormals.ToArray(), CombineIndices.ToArray());
                CombineVertices.Clear();
                CombineNormals.Clear();
                CombineIndices.Clear();
            }
            
            count++;
        }
        CreateMesh((Material)buildingMat,CombineVertices.ToArray(), CombineNormals.ToArray(), CombineIndices.ToArray());
        //삭제
        CombineIndices.Clear();
        CombineVertices.Clear();
        CombineNormals.Clear();
        positions.Clear();
    }

    //도로 깔기
    void SetRoad()
    {
        List<int> CombineIndices = new List<int>();
        List<Vector3> CombineVertices = new List<Vector3>();
        List<Vector3> CombineNormals = new List<Vector3>();
        List<Vector2> CombineUVs = new List<Vector2>();
        int count = 0;

        foreach (List<Vector2> vectors in positions)
        {
            List<int> indices = new List<int>();
            List<Vector3> tmpvertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector3> vertices = Vector2ToVector3(vectors.ToArray());


            int LU = 0;
            int RU = 0;
            int LD = 0;
            int RD = 0;

            for (int i = 0; i < vertices.Count - 1; ++i)
            {
                Vector3 cross = Vector3.Cross(vertices[i] - vertices[i + 1], Vector3.up).normalized * 0.5f;
                Vector3 precross;
                Vector3 nextcross;

                Vector3 LUV = vertices[i] + cross;
                Vector3 RUV = vertices[i + 1] + cross;
                Vector3 LDV = vertices[i] - cross;
                Vector3 RDV = vertices[i + 1] - cross;

                if (!(vertices.Count <= 2))
                {
                    if (i != 0)
                    {
                        precross = Vector3.Cross(vertices[i - 1] - vertices[i], Vector3.up).normalized * 0.5f;
                        nextcross = Vector3.Cross(vertices[i] - vertices[i + 1], Vector3.up).normalized * 0.5f;
                        if (!MathUtil.IntersectionPoint(
                            vertices[i - 1] + precross, vertices[i] + precross,
                               vertices[i] + nextcross, vertices[i + 1] + nextcross,
                               out LUV))
                        {
                            LUV = vertices[i] + cross;
                        }
                        if (!MathUtil.IntersectionPoint(
                            vertices[i - 1] - precross, vertices[i] - precross,
                            vertices[i] - nextcross, vertices[i + 1] - nextcross,
                            out LDV))
                        {
                            LDV = vertices[i] - cross;
                        }
                    }
                    if (i != vertices.Count - 2)
                    {
                        precross = Vector3.Cross(vertices[i] - vertices[i + 1], Vector3.up).normalized * 0.5f;
                        nextcross = Vector3.Cross(vertices[i + 1] - vertices[i + 2], Vector3.up).normalized * 0.5f;
                        if (!MathUtil.IntersectionPoint(
                             vertices[i] + precross, vertices[i + 1] + precross,
                             vertices[i + 1] + nextcross, vertices[i + 2] + nextcross,
                             out RUV))
                        {
                            RUV = vertices[i + 1] + cross;
                        }
                        if (!MathUtil.IntersectionPoint(
                             vertices[i] - precross, vertices[i + 1] - precross,
                             vertices[i + 1] - nextcross, vertices[i + 2] - nextcross,
                             out RDV))
                        {
                            RDV = vertices[i + 1] - cross;
                        }
                    }
                }

                tmpvertices.Add(LUV);//lu
                tmpvertices.Add(LDV);//ld
                tmpvertices.Add(RUV);//ru
                tmpvertices.Add(RDV);//rd

                float upUV = Vector3.Distance(LUV, RUV);
                float downUV = Vector3.Distance(LDV, RDV);

                uvs.Add(new Vector2(0, 1));//lu
                uvs.Add(new Vector2(0, 0));//ld
                uvs.Add(new Vector2(upUV, 1));//ru
                uvs.Add(new Vector2(downUV, 0));//rd


                for (int j = 0; j < 4; j++)
                {
                    CombineNormals.Add(Vector3.up);
                }

                LU = i * 4;
                LD = i * 4 + 1;
                RU = i * 4 + 2;
                RD = i * 4 + 3;

                indices.Add(LU + CombineVertices.Count);
                indices.Add(LD + CombineVertices.Count);
                indices.Add(RU + CombineVertices.Count);

                indices.Add(LD + CombineVertices.Count);
                indices.Add(RD + CombineVertices.Count);
                indices.Add(RU + CombineVertices.Count);
            }

            CombineVertices.AddRange(tmpvertices);
            CombineIndices.AddRange(indices);
            CombineUVs.AddRange(uvs);

            //500개 씩 mesh combine
            if (count % 500 == 0)
            {
                CreateMesh((Material)roadMat,CombineVertices.ToArray(), CombineNormals.ToArray(), CombineIndices.ToArray(), CombineUVs.ToArray());
                indices.Clear();
                tmpvertices.Clear();
                uvs.Clear();
                CombineVertices.Clear();
                CombineNormals.Clear();
                CombineIndices.Clear();
                CombineUVs.Clear();
            }
            count++;
        }
        CreateMesh((Material)roadMat,CombineVertices.ToArray(), CombineNormals.ToArray(), CombineIndices.ToArray(),CombineUVs.ToArray());
        //삭제
        CombineIndices.Clear();
        CombineVertices.Clear();
        CombineNormals.Clear();
        CombineUVs.Clear();
        positions.Clear();
    }

    private void CreateMesh(Material mat, Vector3[] inVertices, Vector3[] normals, int[] inIndices,Vector2[] uvs = null)
    {
        GameObject gameObject = new GameObject("building");
        gameObject.transform.SetParent(parantsObj.transform);

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();

        mesh.vertices = inVertices;
        mesh.triangles = inIndices;
        mesh.normals = normals;
        mesh.uv = uvs;
        meshFilter.mesh = mesh;

        meshRenderer.material = mat;
    }

    public List<Vector3> Vector2ToVector3(Vector2[] inVec)
    {
        List<Vector3> vector3s = new List<Vector3>();

        for (int i = 0; i < inVec.Length; ++i)
        {
            vector3s.Add(new Vector3(inVec[i].x, 0, inVec[i].y));
        }
        return vector3s;
    }

    int ClampIndex(int index, int listSize,int startindex = 0)
    {
        return startindex + (index % listSize);
    }
}
