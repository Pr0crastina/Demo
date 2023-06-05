using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using System;

public class Triangle
{
    public readonly Vertex_hex a;
    public readonly Vertex_hex b;
    public readonly Vertex_hex c;
    public readonly Vertex_triangleCenter center;
    public readonly Vertex_hex[] vertices;
    public readonly Edge ab;
    public readonly Edge bc;
    public readonly Edge ac;
    public readonly Edge[] edges;
    public Triangle(Vertex_hex a, Vertex_hex b, Vertex_hex c, List<Vertex_mid> mids, List<Vertex_center> centers, List<Edge> edges, List<Triangle> triangles)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        vertices = new Vertex_hex[] { a, b, c };

        //创建边线
        ab = Edge.FindEdge(a, b, mids, edges);
        bc = Edge.FindEdge(b, c, mids, edges);
        ac = Edge.FindEdge(a, c, mids, edges);

        if (ab == null) { ab = new Edge(a, b, mids, edges); }
        if (ac == null) { ac = new Edge(a, c, mids, edges); }
        if (bc == null) { bc = new Edge(b, c, mids, edges); }

        this.edges = new Edge[] { ab, bc, ac };

        center = new Vertex_triangleCenter(this);

        triangles.Add(this);

        centers.Add(center);
    }
    public static void Triangles_Ring(int radius, List<Vertex_hex> hexes, List<Vertex_mid> mids, List<Vertex_center> centers, List<Edge> edges, List<Triangle> triangles)
    {
        List<Vertex_hex> inner = Vertex_hex.GrabRing(radius - 1, hexes);
        List<Vertex_hex> outer = Vertex_hex.GrabRing(radius, hexes);
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < radius; j++)
            {
                //创建两个顶点在外圈，一个顶点在内圈的三角形
                Vertex_hex a = outer[i * radius + j];
                Vertex_hex b = outer[(i * radius + j + 1) % outer.Count];
                Vertex_hex c = inner[(i * (radius - 1) + j) % inner.Count];
                new Triangle(a, b, c, mids, centers, edges, triangles);
                //创建一个顶点在外圈，两个顶点在内圈的三角形
                if (j > 0)
                {
                    Vertex_hex d = inner[i * (radius - 1) + j - 1];
                    new Triangle(a, c, d, mids, centers, edges, triangles);
                }
            }
        }
    }
    public static void Triangles_Hex(List<Vertex_hex> hexes, List<Vertex_mid> mids, List<Vertex_center> centers, List<Edge> edges, List<Triangle> triangles)
    {
        for (int i = 1; i <= Grid.radius; i++)
        {
            Triangles_Ring(i, hexes, mids, centers, edges, triangles);
        }
    }
    //判断相邻三角形
    public bool isNeighbour(Triangle target)
    {
        HashSet<Edge> intersection = new HashSet<Edge>(edges);
        intersection.IntersectWith(target.edges);
        return intersection.Count == 1;
    }
    public List<Triangle> FindAllNeighborTriangles(List<Triangle> triangles)
    {
        List<Triangle> result = new List<Triangle>();
        foreach (Triangle triangle in triangles)
        {
            if (this.isNeighbour(triangle))
            {
                result.Add(triangle);
            }
        }
        return result;
    }
    public Edge NeighborEdge(Triangle neighbor)
    {
        HashSet<Edge> intersection = new HashSet<Edge>(edges);
        intersection.IntersectWith(neighbor.edges);
        return intersection.Single();
    }
    public Vertex_hex IsolatedVertex_Self(Triangle neighbor)
    {
        HashSet<Vertex_hex> exception = new HashSet<Vertex_hex>(vertices);
        exception.ExceptWith(NeighborEdge(neighbor).hexes);
        return exception.Single();
    }
    public Vertex_hex IsolatedVertex_Neighbor(Triangle neighbor)
    {
        HashSet<Vertex_hex> exception = new HashSet<Vertex_hex>(neighbor.vertices);
        exception.ExceptWith(NeighborEdge(neighbor).hexes);
        return exception.Single();
    }
    public void MergeNeighborTriangles(Triangle neighbor, List<Vertex_mid> mids, List<Vertex_center> centers, List<Edge> edges, List<Triangle> triangles, List<Quad> quads)
    {
        Vertex_hex a = IsolatedVertex_Self(neighbor);
        Vertex_hex b = vertices[(Array.IndexOf(vertices, a) + 1) % 3];
        //debug:Vertex_hex b =neighbor.vertices[(Array.IndexOf(vertices, a) + 1) % 3];
        Vertex_hex c = IsolatedVertex_Neighbor(neighbor);
        Vertex_hex d = neighbor.vertices[(Array.IndexOf(neighbor.vertices, c) + 1) % 3];
        Quad quad = new Quad(a, b, c, d, mids, centers, edges, quads);
        edges.Remove(NeighborEdge(neighbor));
        mids.Remove(NeighborEdge(neighbor).mid);
        triangles.Remove(this);
        triangles.Remove(neighbor);
        centers.Remove(this.center);
        centers.Remove(neighbor.center);
    }
    public static bool HasNeighborTriangles(List<Triangle> triangles)
    {
        foreach (Triangle triangle_a in triangles)
        {
            foreach (Triangle triangle_b in triangles)
            {
                if (triangle_a.isNeighbour(triangle_b))
                    return true;
            }
        }
        return false;
    }
    public static void RandomlyMergeTriangles(List<Vertex_mid> mids, List<Vertex_center> centers, List<Edge> edges, List<Triangle> triangles, List<Quad> quads)
    {
        int randomIndex = UnityEngine.Random.Range(0, triangles.Count);
        List<Triangle> neighbors = triangles[randomIndex].FindAllNeighborTriangles(triangles);
        if (neighbors.Count != 0)
        {
            int randomNeighborIndex = UnityEngine.Random.Range(0, neighbors.Count);
            triangles[randomIndex].MergeNeighborTriangles(neighbors[randomNeighborIndex], mids, centers, edges, triangles, quads);
        }
    }
    public void subdivide(List<SubQuad> subQuads)
    {
        SubQuad quad_a = new SubQuad(a, ab.mid, center, ac.mid, subQuads);
        SubQuad quad_b = new SubQuad(b, bc.mid, center, ab.mid, subQuads);
        SubQuad quad_c = new SubQuad(c, ac.mid, center, bc.mid, subQuads);

        a.subQuads.Add(quad_a);
        b.subQuads.Add(quad_b);
        c.subQuads.Add(quad_c);

        center.subQuads.Add(quad_a);
        center.subQuads.Add(quad_b);
        center.subQuads.Add(quad_c);

        ab.mid.subQuads.Add(quad_a);
        ab.mid.subQuads.Add(quad_b);
        bc.mid.subQuads.Add(quad_b);
        bc.mid.subQuads.Add(quad_c);
        ac.mid.subQuads.Add(quad_a);
        ac.mid.subQuads.Add(quad_c);

        quad_a.neighbors[1] = quad_b;
        quad_a.neighborVertices.Add(quad_b, new Vertex[] { ab.mid, center });
        quad_a.neighbors[2] = quad_c;
        quad_a.neighborVertices.Add(quad_c, new Vertex[] { ac.mid, center });
        quad_b.neighbors[1] = quad_c;
        quad_b.neighborVertices.Add(quad_c, new Vertex[] { bc.mid, center });
        quad_b.neighbors[2] = quad_a;
        quad_b.neighborVertices.Add(quad_a, new Vertex[] { ab.mid, center });
        quad_c.neighbors[1] = quad_a;
        quad_c.neighborVertices.Add(quad_a, new Vertex[] { ac.mid, center });
        quad_c.neighbors[2] = quad_b;
        quad_c.neighborVertices.Add(quad_b, new Vertex[] { bc.mid, center });
    }
}
