using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygons : MonoBehaviour
{
    public Dictionary<Tile, GameObject> tile_dict = new Dictionary<Tile, GameObject>();
    int n = 4;
    int k = 5;

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

        // Polygon vertices
        Vector3[] vertices = new Vector3[n];
        mesh.vertices = vertices;

        // Triangle vertices (vertices in clockwise order)
        int[] triangles = new int[3 * (n - 2)];
        for (int i = 0; i < n - 2; i++)
        {
            triangles[3 * i] = 0;
            triangles[(3 * i) + 1] = i + 1;
            triangles[(3 * i) + 2] = i + 2;
        }
        mesh.triangles = triangles;

        return go;
    }

    // Set a polygon's vertex positions to a tile's Poincare-projected Vertex positions
    void setPolygonVerts(Tile t, GameObject pg) {
        Vector3[] vertices = new Vector3[n];
        for (int i = 0; i < n; i++) {
            vertices[i] = (Vector3) Hyper.getPoincare(t.vertices[n - i - 1].getPos());
        }
        pg.GetComponent<MeshFilter>().mesh.vertices = vertices;
    }

    // Set a polygon's color to a tile's color
    void setPolygonColor(Tile t, GameObject pg) {
        pg.GetComponent<MeshRenderer>().material.SetColor("_Color", t.color);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Debug.Log(go.GetComponent<MeshRenderer>().material.color);

        // Origin
        Tile curTile = new Tile(n, k);
        curTile.setStart(new Vector3d(0, 0, 0));
        // curTile.expand();
    }

    // Update is called once per frame
    void Update()
    {
        // Render all tiles
        foreach (Tile t in Tile.visible) {
            if (!tile_dict.ContainsKey(t)) {
                GameObject pg = createPolygon();
                setPolygonColor(t, pg);
                tile_dict.Add(t, pg);
            }
            setPolygonVerts(t, tile_dict[t]);
        }
    }
}
