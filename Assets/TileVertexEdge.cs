using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public static List<Tile> visible;
    public static List<Tile> next;
    public static List<Tile> all;
    public static Queue<Tile> parents;

    public Vector3d center;
    public Color color;
    // public int texture;
    public double angle;
    // public int queueNum;
    public Tile parent;

    public List<Vertex> vertices; // CCW order
    public List<Edge> edges; // CCW order

    public int n; // Number of vertices per tile
    public int k; // Number of tiles per vertex
    double fv; // Precompute firstVertex()
    double fvY;

    // Only for origin tile
    public Tile(int n, int k)
    {
        // https://math.stackexchange.com/questions/85793/symbolic-coordinates-for-a-hyperbolic-grid/192407#192407
        Debug.Assert(2*n + 2*k < n*k, "Tile(): invalid n and k");

        // Initialize static lists; Only do this once, i.e. only do for origin tile
        visible = new List<Tile>();
        next = new List<Tile>();
        all = new List<Tile>();
        parents = new Queue<Tile>();

        // Manually add origin tile; the rest are added by expand()
        all.Add(this);

        this.n = n;
        this.k = k;

        vertices = new List<Vertex>();
        edges = new List<Edge>();

        color = Random.ColorHSV();
        center = Vector3d.up;

        // Vertex first_vert = new Vertex(k, Hyper.reversePoincare(Hyper.circleRadius(n, k), 0));
        fv = Hyper.firstVertex(n, k);
        fvY = Mathd.Sqrt(Mathd.Pow(fv, 2.0d) + 1);
        Vertex first_vert = new Vertex(k, new Vector3d(fv, Mathd.Sqrt(Mathd.Pow(fv, 2.0d) + 1), 0));
        vertices.Add(first_vert);
        Edge first_edge = first_vert.edges[0];
        
        Edge edge = first_edge;
        edge.addTile(this);

        for (int i = 1; i < n; i++) {
            Vertex next_vert = edge.vertex2;
            Vector3d next_loc = Hyper.rotate(vertices[i - 1].getPos(), 2 * Mathd.PI_PRECISE / n);
            next_vert.clamp(next_loc);
            vertices.Add(next_vert);

            edge = next_vert.prev(edge);
            edge.addTile(this);
        }
        
        Edge merge_edge = first_vert.next(first_edge);
        edge.merge(merge_edge);

        populateEdges();

        // texture = -1;
        angle = 0;
        // queueNum = -1;
    }

    public Tile(Tile ref_t, Edge e, int n, int k)
    {
        Debug.Assert(2*n + 2*k < n*k, "Tile(): invalid n and k");
        
        this.n = n;
        this.k = k;
        fv = Hyper.firstVertex(n, k);
        fvY = Mathd.Sqrt(Mathd.Pow(fv, 2.0d) + 1);

        vertices = new List<Vertex>();
        edges = new List<Edge>();

        color = Random.ColorHSV();

        e.addTile(this);

        center = Hyper.extend(ref_t.center, Hyper.midpoint(e.vertex1.getPos(), e.vertex2.getPos()));

        // List<Vertex> verts = e.verts(center);
        // Swapped verts[0] and verts[1] when changing from verts to verts2
        List<Vertex> verts = e.verts2(ref_t);

        Vertex back_vert = verts[1];
        Vertex front_vert = verts[0];

        vertices.Add(back_vert);
        vertices.Add(front_vert);

        Edge back_edge = back_vert.next(e);
        while (back_vert != front_vert && !back_edge.hasDangling()) {
            back_edge.addTile(this);
            back_vert = (back_vert == back_edge.vertex1) ? back_edge.vertex2 : back_edge.vertex1; //back_vert = back_edge.verts(center).at(0);
            vertices.Insert(0, back_vert);
            back_edge = back_vert.next(back_edge);
        }

        // Made a loop; all vertices accounted for
        if (back_vert == front_vert)
            vertices.RemoveAt(0);
        // Otherwise, need to complete the vertices; start from front_vert
        else {
            // Use process similar to setVertexLocs3

            Vertex vertex = front_vert;
            Edge edge = vertex.prev(e);
            edge.addTile(this);

            Vector3d dir = Hyper.getDir(center, verts[1].getPos());
            Vector3d dir2 = Hyper.getDir(center, verts[0].getPos());
            Vector3d dir3 = Hyper.getDir(dir, dir2);
            double angle = 2 * Mathd.PI_PRECISE / n;
            double dist = System.Math.Acosh(fvY);

            Vector3d midpt = Hyper.midpoint(e.vertex1.getPos(), e.vertex2.getPos());

            int size = vertices.Count;
            for (int i = 2; i < n - size + 2; i++) {

                Vector3d newDir = dir * Mathd.Cos(i * angle) + dir3 * Mathd.Sin(i * angle);
                Vector3d next_loc = Hyper.line(center, newDir, dist);

                if (!edge.vertex2.initialized) {
                    vertex = edge.vertex2;
                    vertex.clamp(next_loc);
                } else {
                    vertex = (vertex == edge.vertex1) ? edge.vertex2 : edge.vertex1; // vertex = edge.verts(center).at(1);
                    vertex.setPos(next_loc);
                }

                vertices.Add(vertex);

                edge = vertex.prev(edge);
                edge.addTile(this);
            }

            edge.merge(back_edge);
        }

        populateEdges();

        // texture = -1;
        angle = 0;
        // queueNum = -1;
    }

    // Once all vertices are set, fill edges vector with edges
    public void populateEdges()
    {
        for (int i = 0; i < n; i++) {
            Vertex v1 = vertices[i];
            Vertex v2 = vertices[(i + 1) % n];
            edges.Add(v1.seekVertex(v2));
        }
    }

    // Find index of edge in edges vector
    public int findEdge(Edge e)
    {
        for (int i = 0; i < n; i++) {
            if (e == edges[i])
                return i;
        }
        Debug.Assert(false, "Tile.findEdge: edge not found");
        return -1;
    }

    // Set vertex locations for this tile, given a reference tile and edge
    // This version reflects vertex locations through the midpoint of the edge
    public void setVertexLocs1(Tile ref_t, Edge e)
    {
        Vector3d midpt = Hyper.midpoint(e.vertex1.getPos(), e.vertex2.getPos());
        center = Hyper.extend(ref_t.center, midpt);

        Vertex vertex = e.verts(center)[1];
        Edge edge = vertex.prev(e);

        Vertex reflecting_vertex = e.verts(ref_t.center)[1];
        Edge ref_edge = reflecting_vertex.prev(e);

        for (int i = 0; i < n - 2; i++) {
            // ccw vertex ordering breaks down when the vertex locations are inaccurate.
            // Need to rely on comparing vertex1 and vertex2 instead of using verts().
            vertex = (vertex == edge.vertex1) ? edge.vertex2 : edge.vertex1;
            reflecting_vertex = (reflecting_vertex == ref_edge.vertex1) ? ref_edge.vertex2 : ref_edge.vertex1; //reflecting_vertex = ref_edge.verts(ref.center).at(1);
            
            Vector3d next_loc = Hyper.extend(reflecting_vertex.getPos(), midpt);
            vertex.setPos(next_loc);

            edge = vertex.prev(edge);
            ref_edge = reflecting_vertex.prev(ref_edge);
        }
    }

    // This version uses Hyper.symmetry and is more accurate than version 1.
    public void setVertexLocs2(Tile ref_t, Edge e)
    {
        Vector3d midpt = Hyper.midpoint(e.vertex1.getPos(), e.vertex2.getPos());
        center = Hyper.extend(ref_t.center, midpt);

        Vertex vertex = e.verts(center)[1];
        Edge edge = vertex.prev(e);

        Vertex reflecting_vertex = vertex;
        Edge ref_edge = reflecting_vertex.next(e);

        for (int i = 0; i < n - 2; i++) {
            // ccw vertex ordering breaks down when the vertex locations are inaccurate.
            // Need to rely on comparing vertex1 and vertex2 instead of using verts().
            vertex = (vertex == edge.vertex1) ? edge.vertex2 : edge.vertex1;
            reflecting_vertex = (reflecting_vertex == ref_edge.vertex1) ? ref_edge.vertex2 : ref_edge.vertex1;

            Vector3d next_loc = Hyper.symmetry(reflecting_vertex.getPos(), ref_t.center, center);
            vertex.setPos(next_loc);

            edge = vertex.prev(edge);
            ref_edge = reflecting_vertex.next(ref_edge);
        }
    }

    // This version uses rotation around the tile center
    // Seems slightly more accurate than setVertexLocs2 when using (n,k) = (4,5) and expand radius = 0.89
    public void setVertexLocs3(Tile ref_t, Edge e)
    {
        Vector3d midpt = Hyper.midpoint(e.vertex1.getPos(), e.vertex2.getPos());
        center = Hyper.extend(ref_t.center, midpt);

        List<Vertex> verts = e.verts2(ref_t);
        Vertex vertex = verts[0];
        Edge edge = vertex.prev(e);

        Vector3d dir = Hyper.getDir(center, verts[1].getPos());
        Vector3d dir2 = Hyper.getDir(center, verts[0].getPos());
        Vector3d dir3 = Hyper.getDir(dir, dir2);
        double angle = 2 * Mathd.PI_PRECISE / n;

        // Distance from center to vertex
        double dist = System.Math.Acosh(fvY);

        for (int i = 2; i < n; i++) {
            vertex = (vertex == edge.vertex1) ? edge.vertex2 : edge.vertex1;

            Vector3d newDir = dir * Mathd.Cos(i * angle) + dir3 * Mathd.Sin(i * angle);
            Vector3d next_loc = Hyper.line(center, newDir, dist);
            vertex.setPos(next_loc);

            edge = vertex.prev(edge);
        }
    }

    // Get tile neighbors
    public List<Tile> getNeighbors()
    {
        List<Tile> neighbors = new List<Tile>();
        foreach (Edge e in edges) {
            if (e.tiles.Count == 2) {
                Tile neighbor = (this == e.tiles[0]) ? e.tiles[1] : e.tiles[0];
                neighbors.Add(neighbor);
            }
        }
        return neighbors;
    }

    // Expand in all n directions, creating new tiles if necessary
    public void expand()
    {
        foreach (Edge e in edges) {
            Tile other_tile = null;
            if (e.tiles.Count < 2) {
                other_tile = new Tile(this, e, n, k);
                all.Add(other_tile);
            }
            else {
                Debug.Assert(e.tiles.Count == 2, "Tile.expand: edge has more than 2 tiles");
                other_tile = (this == e.tiles[0]) ? e.tiles[1] : e.tiles[0];
                // other_tile->setVertexLocs(this, e);
            }
            if (other_tile is not null && !other_tile.isVisible()) {
                next.Add(other_tile);
                visible.Add(other_tile);
                other_tile.setVertexLocs3(this, e);
            }
        }
    }

    // Set starting tile position based on relative position to its center
    // Calls expand() repeatedly
    public void setStart(Vector3d pos)
    {
        vertices[0].setPos(Hyper.rotate(new Vector3d(fv, fvY, 0), angle));

        for (int i = 1; i < n; i++)
            vertices[i].setPos(Hyper.rotate(vertices[i-1].getPos(), 2 * Mathd.PI_PRECISE / n));

        Vector3d xz = Hyper.getXZ(pos);
        for (int i = 0; i < n; i++)
            vertices[i].setPos(Hyper.translateXZ(vertices[i].getPos(), xz.x, xz.z));

        center = Hyper.translateXZ(Vector3d.up, xz.x, xz.z);

        next.Clear();
        next.Add(this);

        visible.Clear();
        visible.Add(this);

        while (next.Count != 0) {
            Tile t = next[next.Count - 1];
            next.RemoveAt(next.Count - 1);
            if (t.withinRadius(0.9))
                t.expand();
        }

        /*
        // This is for marking tiles to receive generated outputs; comment out to disable
        if (!parent) {
            parents.push(this);
            parent = this;
            foreach (Tile t in getNeighbors()) {
                if (!t->parent)
                    t->parent = this;
            }
        }*/
    }

    // Check if tile is in vector of all currently updated/visible tiles
    public bool isVisible()
    {
        return visible.Contains(this);
    }

    // Check if any of the tile's Poincare-projected vertices are within the given radius
    bool withinRadius(double rad)
    {
        foreach (Vertex v in vertices) {
            if (Vector3d.Distance(Vector3d.zero, Hyper.getPoincare(v.getPos())) < rad)
                return true;
        }
        return false;
    }
}

/*********************************************************************/
/*********************************************************************/
/*********************************************************************/

public class Vertex
{
    int k; // Number of edges per vertex.
    public List<Edge> edges;
    Vector3d pos;
    public bool initialized;

    public Vertex(int k)
    {
        this.k = k;
        edges = new List<Edge>();
        initialized = false;
	    pos = Vector3d.zero;
    }

    public Vertex(int k, Vector3d loc)
    {
        this.k = k;
        edges = new List<Edge>();
        initialized = false;
	    clamp(loc);
    }

    public void setPos(Vector3d loc)
    {
        pos = loc;
    }

    public Vector3d getPos()
    {
        return pos;
    }

    public void addEdge(Edge e)
    {
        edges.Add(e);
    }

    // Get next edge in counter clockwise order.
    public Edge next(Edge e)
    {
        int idx = seekEdge(e);
	    return edges[(idx + 1) % k];
    }

    // Get prev edge in ccw order (i.e. next edge in clockwise order).
    public Edge prev(Edge e)
    {
        int idx = seekEdge(e);
	    return edges[(idx - 1 + k) % k];
    }

    public void clamp(Vector3d loc)
    {
        Debug.Assert(!initialized, "Vertex.clamp: already initialized");
        initialized = true;
        pos = loc;
        int start = edges.Count;
        for (int i = start; i < k; i++) {
            Vertex loose_vert = new Vertex(k);
            Edge loose_edge = new Edge(this, loose_vert);
        }
    }

    public void replaceEdge(Edge oldEdge, Edge newEdge)
    {
        int idx = seekEdge(oldEdge);
        edges[idx] = newEdge;
    }

    // Find index of given edge in the vector of edges.
    public int seekEdge(Edge e)
    {
        for (int i = 0; i < k; i++) {
            if (e == edges[i])
                return i;
        }
        Debug.Assert(false, "Vertex.findEdge: edge not found");
        return -1;
    }

    // Find edge connecting this vertex to the given one.
    public Edge seekVertex(Vertex v)
    {
        foreach (Edge e in edges) {
            if (e.vertex1 == v || e.vertex2 == v)
                return e;
        }
        Debug.Assert(false, "Vertex.seekVertex: vert not found");
        return null;
    }
}

/*********************************************************************/
/*********************************************************************/
/*********************************************************************/

public class Edge
{
    public List<Tile> tiles;
    public Vertex vertex1;
    public Vertex vertex2;

    public Edge(Vertex v1, Vertex v2)
    {
        tiles = new List<Tile>();

        vertex1 = v1;
        vertex2 = v2;
        
        v1.addEdge(this);
	    v2.addEdge(this);
    }

    public void addTile(Tile t)
    {
        if (tiles.Count == 2)
            Debug.Log("Edge.addTile: Already has 2 tiles");
        if (!tiles.Contains(t))
            tiles.Add(t);
        else
            Debug.Assert(false, "Edge.addTile: Duplicate tile");
    }

    public List<Vertex> verts()
    {
        return new List<Vertex> { vertex1, vertex2 };
    }

    // Get vertices in ccw order relative to given center
    public List<Vertex> verts(Vector3d center)
    {
        Vector3d v1 = Hyper.getPoincare(vertex1.getPos()) - Hyper.getPoincare(center);
        Vector3d v2 = Hyper.getPoincare(vertex2.getPos()) - Hyper.getPoincare(center);
        double rad1 = Mathd.Atan2(v1[2], v1[0]);
        double rad2 = Mathd.Atan2(v2[2], v2[0]);

        double angle = (rad2 - rad1 + 2 * Mathd.PI_PRECISE) % (2 * Mathd.PI_PRECISE);

        if (angle > Mathd.PI_PRECISE) {
            return new List<Vertex> { vertex2, vertex1 };
        } else {
            return new List<Vertex> { vertex1, vertex2 };
        }
    }

    public List<Vertex> verts2(Tile ref_t)
    {
        List<Vertex> vertices = ref_t.vertices;
        int n = ref_t.n;
        for (int i = 0; i < vertices.Count; i++) {
            if (vertices[i] == vertex1) {
                if (vertices[(i + 1) % n] == vertex2)
                    return new List<Vertex> { vertex1, vertex2 };
                else if (vertices[(i - 1 + n) % n] == vertex2) {
                    return new List<Vertex> { vertex2, vertex1 };
                }
                else Debug.Assert(false, "verts2: vertices not adjacent");
            }
        }
        Debug.Assert(false, "verts2: failed to find vertex1");
        return null;
    }

    public bool hasDangling()
    {
        return (!vertex1.initialized) || (!vertex2.initialized);
    }

    public void merge(Edge e)
    {
        Debug.Assert(this.hasDangling() && e.hasDangling(), "Edge.merge: no dangling");
        
        if (!vertex1.initialized) {
            if (!e.vertex1.initialized)
                vertex1 = e.vertex2;
            else
                vertex1 = e.vertex1;
            vertex1.replaceEdge(e, this);
        }
        else {
            if (!e.vertex1.initialized)
                vertex2 = e.vertex2;
            else
                vertex2 = e.vertex1;
            vertex2.replaceEdge(e, this);
        }
    }
}