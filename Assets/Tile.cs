using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    static List<Tile> visible;
    static List<Tile> next;
    static List<Tile> all;
    static List<Tile> parents;

    Vector3d center;
    Color color;
    // int texture;
    double angle;
    // int queueNum;
    Tile parent;

    int n; // Number of vertices per tile
    int k; // Number of tiles per vertex
    // https://math.stackexchange.com/questions/85793/symbolic-coordinates-for-a-hyperbolic-grid/192407#192407
    // Assert(2*n + 2*k < n*k);

    public Tile(int n, int k)
    {
    }

    public Tile(Tile t, Edge e, int n, int k)
    {

    }

}

public class Vertex
{
    public Vertex()
    {

    }
}

public class Edge
{
    public Edge()
    {

    }
}