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

    private float dist = 50f;

    // 0 = Poincaré
    // 1 = Klein-Beltrami
    private int projection = 0;

    private string sentence = "";

    List<Tile> visible2;

    System.DateTime lastTime;

    PlayerController pc;

    SpriteCreater sc;

    WebClient wc;

    // Start is called before the first frame update
    void Start()
    {
        // Get inputs [n, k, sentence], then deactivate menu
        GameObject startMenu = GameObject.Find("StartMenu");
        // Default to (4,5) if starting in game instead of at start menu (testing purposes only)
        if (startMenu == null) {
            n = 4;
            k = 5;
            
            // sentence = "A street scene with a double-decker bus on the side of the road.";
            sentence = "A crowd watching baseball players at a game.";
            // sentence = "A minimalist room features white appliances and beige walls.";
            // sentence = "A city street line with brick buildings and trees.";
        }
        else {
            List<int> nk = startMenu.GetComponent<StartMenu>().getNK();
            n = nk[0];
            k = nk[1];
            sentence = startMenu.GetComponent<StartMenu>().getSentence();
            startMenu.SetActive(false);
        }

        // Create origin tile
        curTile = new Tile(n, k);
        curTile.setStart(new Vector3d(0, 0, 0), dist);
        pc = GameObject.Find("Player").GetComponent<PlayerController>();
        lastTime = System.DateTime.Now;
        visible2 = new List<Tile>();
        tilePos = Vector3d.up;

        sc = GameObject.Find("SpriteCreater").GetComponent<SpriteCreater>();
        wc = GameObject.Find("WebClient").GetComponent<WebClient>();
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
        curTile.setStart(tilePos, dist);

        // Hide old tiles and images
        foreach (Tile t in visible2) {
            tile_dict[t].SetActive(false);
            if (t.generated)
                t.image.SetActive(false);
        }
        
        visible2 = new List<Tile>(Tile.visible);

        // Render all visible tiles and images
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

            // Render image if it exists
            if (t.image != null) {
                t.image.SetActive(true);

                Vector3 center = (Vector3) project(t.center);
                
                // Set image position and rotation
                t.image.transform.position = center + new Vector3(0, 0.08f, 0);
                Vector3 target = Vector3.zero - center;
                t.image.transform.eulerAngles = new Vector3(0, -90f + Mathf.Rad2Deg * Mathf.Atan2(-target.z, target.x), 0);

                // Scale image size
                Texture2D tex = t.image.GetComponent<SpriteRenderer>().sprite.texture;
                float scale = Vector3.Distance(center, (Vector3) project(t.vertices[0].getPos())) / tex.width;

                t.image.GetComponent<SpriteRenderer>().transform.localScale = Vector3.one * scale * 70f;
                // t.image.GetComponent<SpriteRenderer>().sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 2.0f / scale);
            }
        }

        // Generate images for megatiles
        if (Tile.megatiles.Count != 0) {
            List<Tile> megatile = Tile.megatiles.Dequeue();
            
            // "{'world': [], 'coords': [[0.0, 0.0], [0.899454, 0.899454], [-0.899454, 0.899454], [-0.899454, -0.899454], [0.899454, -0.899454]]}";

            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();
            List<List<float>> coords = new List<List<float>>();
            List<List<float>> world = new List<List<float>>();
            List<List<float>> latent_vectors = new List<List<float>>();

            foreach (Tile t in megatile) {
                t.image = sc.createSprite(Texture2D.whiteTexture);
                coords.Add(new List<float>() {(float) t.center.x, (float) t.center.z});
            }
            foreach (Tile t in Tile.visible) {
                if (t.generated) {
                    world.Add(new List<float>() {(float) t.center.x, (float) t.center.z});
                    latent_vectors.Add(t.latent_vector);
                }
            }
            data.Add("coords", coords);
            data.Add("world", world);
            data.Add("vectors", latent_vectors);
            data.Add("sentence", sentence);

            if (sentence != "")
                sentence = "";


            StartCoroutine(wc.SendRequest(data, megatile));
        }
    }

    // https://forum.unity.com/threads/how-to-instantiate-a-mesh-asset.1088176/
    GameObject createPolygon()
    {
        GameObject go = new GameObject();

        Mesh mesh = new Mesh();
        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("UI/Default"));
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
        Debug.Assert(projection == 0 || projection == 1, "setPolygonVerts: invalid projection");

        Vector3[] vertices = new Vector3[n];
        if (projection == 0) {
            for (int i = 0; i < n; i++)
                vertices[i] = (Vector3) Hyper.getPoincare(t.vertices[n - i - 1].getPos());
        }
        else if (projection == 1) {
            for (int i = 0; i < n; i++)
                vertices[i] = (Vector3) Hyper.getBeltrami(t.vertices[n - i - 1].getPos());
        }

        pg.GetComponent<MeshFilter>().mesh.vertices = vertices;
        
        // Without this line, the polygons will visibly disappear when looking at certain angles
        pg.GetComponent<MeshFilter>().mesh.RecalculateBounds();
    }

    // Set a polygon's color to a tile's color
    void setPolygonColor(Tile t, GameObject pg) {
        pg.GetComponent<MeshRenderer>().material.SetColor("_Color", t.color);
    }

    public void setDist(float input) {
        dist = input;
    }

    public void setProjection(int input) {
        projection = input;
    }

    public void setSentence(string input) {
        sentence = input;
    }

    Vector3d project(Vector3d v) {
        if (projection == 0) return Hyper.getPoincare(v);
        else if (projection == 1) return Hyper.getBeltrami(v);
        Debug.Assert(false, "project: no matching projection");
        return Vector3d.up;
    }
}
