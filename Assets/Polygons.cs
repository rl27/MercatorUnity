using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygons : MonoBehaviour
{
    float radius = 1;
    int numVertices = 5;

    // https://forum.unity.com/threads/how-to-instantiate-a-mesh-asset.1088176/
    GameObject createPolygon()
    {
        GameObject go = new GameObject();

        Mesh mesh = new Mesh();
        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Standard"));
        meshRenderer.material.SetColor("_Color", Random.ColorHSV());


        float angle = 2 * Mathf.PI / numVertices;

        // https://www.youtube.com/watch?v=dMBkigAN9B8
        // Polygon vertices
        Vector3[] vertices = new Vector3[numVertices];
        for (int i = 0; i < numVertices; i++)
        {
            vertices[i] = new Vector3(Mathf.Sin(i * angle), 0, Mathf.Cos(i * angle)) * radius;
        }
        mesh.vertices = vertices;

        // Triangle vertices (vertices in clockwise order)
        int[] triangles = new int[3 * (numVertices - 2)];
        for (int i = 0; i < numVertices - 2; i++)
        {
            triangles[3 * i] = 0;
            triangles[(3 * i) + 1] = i + 1;
            triangles[(3 * i) + 2] = i + 2;
        }
        mesh.triangles = triangles;

        return go;
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject go = createPolygon();
        // Debug.Log(go.GetComponent<MeshRenderer>().material.color);
        GameObject go2 = createPolygon();


        float angle = 2 * Mathf.PI / numVertices;
        Vector3[] vertices = new Vector3[numVertices];
        for (int i = 0; i < numVertices; i++)
        {
            vertices[i] = new Vector3(Mathf.Sin(i * angle), 0, Mathf.Cos(i * angle)) * radius / 2f;
            vertices[i][1] += 1;
        }
        go2.GetComponent<MeshFilter>().mesh.vertices = vertices;
    }

    // Update is called once per frame
    void Update()
    {
        //
    }
}
