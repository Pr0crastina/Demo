using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    public static int radius;
    public static int height;
    public static int cellHeight;
    public static float scale;
    public static float cellSize;
    public readonly List<Vertex_hex> hexes = new List<Vertex_hex>();
    public readonly List<Triangle> triangles = new List<Triangle>();
    public readonly List<Vertex> vertices = new List<Vertex>();
    public readonly List<Edge> edges = new List<Edge>();
    public readonly List<Quad> quads = new List<Quad>();
    public readonly List<Vertex_mid> mids = new List<Vertex_mid>();
    public readonly List<Vertex_center> centers = new List<Vertex_center>();
    public readonly List<SubQuad> subQuads = new List<SubQuad>();
    public readonly List<SubQuad_Cube> subQuad_Cubes = new List<SubQuad_Cube>();
    public Grid(int radius, int height, int cellHeight, float scale, float cellSize, int relaxTimes)
    {
        Grid.scale = scale;
        Grid.radius = radius;
        Grid.cellSize = cellSize;
        Grid.height = height;
        Grid.cellHeight = cellHeight;
        Vertex_hex.Hex(hexes);
        Triangle.Triangles_Hex(hexes, mids, centers, edges, triangles);
        while (Triangle.HasNeighborTriangles(triangles))
        { Triangle.RandomlyMergeTriangles(mids, centers, edges, triangles, quads); }

        vertices.AddRange(hexes);
        vertices.AddRange(mids);
        vertices.AddRange(centers);

        foreach (Triangle triangle in triangles)
        { triangle.subdivide(subQuads); }
        foreach (Quad quad in quads)
        { quad.subdivide(subQuads); }
        for (int i = 0; i < relaxTimes; i++)
        {
            foreach (SubQuad subQuad in subQuads)
            { subQuad.CalculateRelaxOffset(); }
            foreach (Vertex vertex in vertices)
            { vertex.Relax(); }
        }
        foreach (Vertex vertex in vertices)
        {
            vertex.index = vertices.IndexOf(vertex);
            vertex.BoundaryCheck();
            if (vertex is Vertex_hex)
            { ((Vertex_hex)vertex).NeighborSubQuadCheck(); }
            for (int i = 0; i < Grid.height + 1; i++)
            { vertex.vertex_Ys.Add(new Vertex_Y(vertex, i)); }
        }
        foreach (SubQuad subQuad in subQuads)
        {
            for (int i = 0; i < Grid.height; i++)
            { subQuad.subQuad_Cubes.Add(new SubQuad_Cube(subQuad, i, subQuad_Cubes)); }
        }
        foreach (SubQuad subQuad in subQuads)
        {
            foreach (SubQuad_Cube subQuad_Cube in subQuad.subQuad_Cubes)
            { subQuad_Cube.NeighborsCheck(); }
        }
    }
}
