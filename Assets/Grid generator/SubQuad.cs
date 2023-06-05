using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubQuad
{
    public readonly Vertex_hex a;
    public readonly Vertex_mid b;
    public readonly Vertex_center c;
    public readonly Vertex_mid d;
    public List<SubQuad_Cube> subQuad_Cubes = new List<SubQuad_Cube>();
    public Dictionary<SubQuad, Vertex[]> neighborVertices = new Dictionary<SubQuad, Vertex[]>();
    public SubQuad[] neighbors = new SubQuad[4];
    public SubQuad(Vertex_hex a, Vertex_mid b, Vertex_center c, Vertex_mid d, List<SubQuad> subQuads)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;

        subQuads.Add(this);
    }
    public void CalculateRelaxOffset()
    {
        Vector3 center = (a.currentPosition + b.currentPosition + c.currentPosition + d.currentPosition) / 4;

        Vector3 vector_a = (a.currentPosition
                              + Quaternion.AngleAxis(-90, Vector3.up) * (b.currentPosition - center) + center
                              + Quaternion.AngleAxis(-180, Vector3.up) * (c.currentPosition - center) + center
                              + Quaternion.AngleAxis(-270, Vector3.up) * (d.currentPosition - center) + center) / 4;
        Vector3 vector_b = Quaternion.AngleAxis(90, Vector3.up) * (vector_a - center) + center;
        Vector3 vector_c = Quaternion.AngleAxis(180, Vector3.up) * (vector_a - center) + center;
        Vector3 vector_d = Quaternion.AngleAxis(270, Vector3.up) * (vector_a - center) + center;

        a.offset += (vector_a - a.currentPosition) * Grid.scale;
        b.offset += (vector_b - b.currentPosition) * Grid.scale;
        c.offset += (vector_c - c.currentPosition) * Grid.scale;
        d.offset += (vector_d - d.currentPosition) * Grid.scale;
    }
    public Vector3 GetCenterPosition()
    {
        return (a.currentPosition + b.currentPosition + c.currentPosition + d.currentPosition) / 4;
    }
    public Vector3 GetMid_ab()
    {
        return (a.currentPosition + b.currentPosition) / 2;
    }
    public Vector3 GetMid_bc()
    {
        return (b.currentPosition + c.currentPosition) / 2;
    }
    public Vector3 GetMid_cd()
    {
        return (c.currentPosition + d.currentPosition) / 2;
    }
    public Vector3 GetMid_ad()
    {
        return (a.currentPosition + d.currentPosition) / 2;
    }
}
public class SubQuad_Cube
{
    public readonly SubQuad subQuad;
    public readonly int y;
    public readonly Vector3 centerPosition;
    public readonly Vertex_Y[] vertex_Ys = new Vertex_Y[8];
    public SubQuad_Cube[] neighbors = new SubQuad_Cube[6];
    public Dictionary<SubQuad_Cube, Vertex_Y[]> neighborVertices = new Dictionary<SubQuad_Cube, Vertex_Y[]>();
    public bool isActive;
    public Slot slot;
    public int index;
    public string bit = "00000000";
    public string pre_bit = "00000000";
    public SubQuad_Cube(SubQuad subQuad, int y, List<SubQuad_Cube> subQuad_Cubes)
    {
        this.subQuad = subQuad;
        this.y = y;
        subQuad_Cubes.Add(this);
        index = subQuad_Cubes.IndexOf(this);
        centerPosition = subQuad.GetCenterPosition() + Vector3.up * Grid.cellHeight * (y + 0.5f);

        vertex_Ys[0] = subQuad.a.vertex_Ys[y + 1];
        vertex_Ys[1] = subQuad.b.vertex_Ys[y + 1];
        vertex_Ys[2] = subQuad.c.vertex_Ys[y + 1];
        vertex_Ys[3] = subQuad.d.vertex_Ys[y + 1];
        vertex_Ys[4] = subQuad.a.vertex_Ys[y];
        vertex_Ys[5] = subQuad.b.vertex_Ys[y];
        vertex_Ys[6] = subQuad.c.vertex_Ys[y];
        vertex_Ys[7] = subQuad.d.vertex_Ys[y];

        foreach (Vertex_Y vertex_Y in vertex_Ys)
        {
            vertex_Y.subQuad_Cubes.Add(this);
        }
    }
    public void UpdateBit()
    {
        pre_bit = bit;
        string result = "";
        if (vertex_Ys[0].isActive) result += "1";
        else result += "0";
        if (vertex_Ys[1].isActive) result += "1";
        else result += "0";
        if (vertex_Ys[2].isActive) result += "1";
        else result += "0";
        if (vertex_Ys[3].isActive) result += "1";
        else result += "0";
        if (vertex_Ys[4].isActive) result += "1";
        else result += "0";
        if (vertex_Ys[5].isActive) result += "1";
        else result += "0";
        if (vertex_Ys[6].isActive) result += "1";
        else result += "0";
        if (vertex_Ys[7].isActive) result += "1";
        else result += "0";

        bit = result;

        if (bit == "00000000") { isActive = false; }
        else { isActive = true; }
    }
    public void NeighborsCheck()
    {
        if (subQuad.neighbors[0] != null)
        {
            neighbors[0] = subQuad.neighbors[0].subQuad_Cubes[y];
            neighborVertices.Add(neighbors[0], new Vertex_Y[] { subQuad.neighborVertices[subQuad.neighbors[0]][0].vertex_Ys[y], subQuad.neighborVertices[subQuad.neighbors[0]][1].vertex_Ys[y],
            subQuad.neighborVertices[subQuad.neighbors[0]][0].vertex_Ys[y + 1],subQuad.neighborVertices[subQuad.neighbors[0]][1].vertex_Ys[ y + 1 ]});
        }
        if (subQuad.neighbors[1] != null)
        {
            neighbors[1] = subQuad.neighbors[1].subQuad_Cubes[y];
            neighborVertices.Add(neighbors[1], new Vertex_Y[] { subQuad.neighborVertices[subQuad.neighbors[1]][0].vertex_Ys[y], subQuad.neighborVertices[subQuad.neighbors[1]][1].vertex_Ys[y],
            subQuad.neighborVertices[subQuad.neighbors[1]][0].vertex_Ys[y + 1],subQuad.neighborVertices[subQuad.neighbors[1]][1].vertex_Ys[ y + 1 ]});
        }
        if (subQuad.neighbors[2] != null)
        {
            neighbors[2] = subQuad.neighbors[2].subQuad_Cubes[y];
            neighborVertices.Add(neighbors[2], new Vertex_Y[] { subQuad.neighborVertices[subQuad.neighbors[2]][0].vertex_Ys[y], subQuad.neighborVertices[subQuad.neighbors[2]][1].vertex_Ys[y],
            subQuad.neighborVertices[subQuad.neighbors[2]][0].vertex_Ys[y + 1],subQuad.neighborVertices[subQuad.neighbors[2]][1].vertex_Ys[ y + 1 ]});
        }
        if (subQuad.neighbors[3] != null)
        {
            neighbors[3] = subQuad.neighbors[3].subQuad_Cubes[y];
            neighborVertices.Add(neighbors[3], new Vertex_Y[] { subQuad.neighborVertices[subQuad.neighbors[3]][0].vertex_Ys[y], subQuad.neighborVertices[subQuad.neighbors[3]][1].vertex_Ys[y],
            subQuad.neighborVertices[subQuad.neighbors[3]][0].vertex_Ys[y + 1],subQuad.neighborVertices[subQuad.neighbors[3]][1].vertex_Ys[ y + 1 ]});
        }
        if (y < Grid.height - 1)
        {
            neighbors[4] = subQuad.subQuad_Cubes[y + 1];
            neighborVertices.Add(neighbors[4], new Vertex_Y[] { vertex_Ys[4], vertex_Ys[5], vertex_Ys[6], vertex_Ys[7] });
        }
        if (y > 0)
        {
            neighbors[5] = subQuad.subQuad_Cubes[y - 1];
            neighborVertices.Add(neighbors[5], new Vertex_Y[] { vertex_Ys[0], vertex_Ys[1], vertex_Ys[2], vertex_Ys[3] });
        }
    }
}
