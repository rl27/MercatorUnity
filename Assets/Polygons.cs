using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygons : MonoBehaviour
{
    public Dictionary<Tile, GameObject> tile_dict = new Dictionary<Tile, GameObject>();
    int n = 4;
    int k = 5;
    Tile curTile;
    Vector3d tilePos;

    List<Tile> visible2;

    System.DateTime lastTime;

    PlayerController pc;

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
        for (int i = 0; i < n; i++)
            vertices[i] = (Vector3) Hyper.getPoincare(t.vertices[n - i - 1].getPos());
        pg.GetComponent<MeshFilter>().mesh.vertices = vertices;
        
        // Without this line, the polygons will visibly disappear when looking at certain angles
        pg.GetComponent<MeshFilter>().mesh.RecalculateBounds();
    }

    // Set a polygon's color to a tile's color
    void setPolygonColor(Tile t, GameObject pg) {
        pg.GetComponent<MeshRenderer>().material.SetColor("_Color", t.color);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Origin
        curTile = new Tile(n, k);
        curTile.setStart(new Vector3d(0, 0, 0));
        pc = GameObject.FindObjectOfType<PlayerController>();
        lastTime = System.DateTime.Now;
        visible2 = new List<Tile>();
        tilePos = Vector3d.up;
    }

    // Update is called once per frame
    void Update()
    {
        // Check for tile change
        System.DateTime curTime = System.DateTime.Now;
        if ((curTime - lastTime).TotalSeconds > 0.3) {
            double yCur = curTile.center.y;
            Tile newTile = curTile;
            //foreach (Tile neighbor in curTile.getNeighbors()) {
            foreach (Tile t in Tile.visible) {
                if (t.center.y < yCur) {
                    yCur = t.center.y;
                    newTile = t;
                }
            }
            
            if (newTile != curTile) {
                curTile = newTile;

                tilePos = curTile.center;

                // Compare current tile rotation to default rotation by moving tile to (0, 1, 0)
                // and inspecting vertices[0]
                Vector3d xz = Hyper.getXZ(tilePos);
                Vector3d reversed = Hyper.reverseXZ(curTile.vertices[0].getPos(), xz.x, xz.z);
                curTile.angle = Mathd.Atan2(reversed.z, reversed.x);

                lastTime = curTime;
            }
        }

        // If pc.pos changed, transform it to (0, 1, 0) and perform same transform on curTile
        if (Vector3d.Magnitude(pc.pos - Vector3d.up) != 0)
        {
            // Transform center
            Vector3d xz = Hyper.getXZ(pc.pos);
            tilePos = Hyper.hypNormalize(Hyper.reverseXZ(tilePos, xz.x, xz.z));
            pc.pos = Vector3d.up;

            // Get new vertex position and set angle
            Vector3d v0_newpos = Hyper.reverseXZ(curTile.vertices[0].getPos(), xz.x, xz.z);
            xz = Hyper.getXZ(tilePos);
            Vector3d reversed = Hyper.reverseXZ(v0_newpos, xz.x, xz.z);
            curTile.angle = Mathd.Atan2(reversed.z, reversed.x);

        }

        // Update tiles to be created/rendered based on current tile
        curTile.setStart(tilePos);

        // Hide old tiles
        foreach (Tile t in visible2)
            tile_dict[t].SetActive(false);
        
        visible2 = new List<Tile>(Tile.visible);

        // Render all visible tiles
        foreach (Tile t in Tile.visible) {
            // Create polygon and dict entry if not present
            if (!tile_dict.ContainsKey(t)) {
                GameObject pg = createPolygon();
                setPolygonColor(t, pg);
                tile_dict.Add(t, pg);
            }
            
            // Set polygon position in world
            setPolygonVerts(t, tile_dict[t]);
            tile_dict[t].SetActive(true);
        }
    }
}
