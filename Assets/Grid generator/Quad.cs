using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quad
{
    public readonly Vertex_hex a;
    public readonly Vertex_hex b;
    public readonly Vertex_hex c;
    public readonly Vertex_hex d;
    public readonly Edge ab;
    public readonly Edge bc;
    public readonly Edge cd;
    public readonly Edge ad;
    public readonly Vertex_quadCenter center;
    public Quad(Vertex_hex a, Vertex_hex b, Vertex_hex c, Vertex_hex d, List<Vertex_mid> mids, List<Vertex_center> centers, List<Edge> edges, List<Quad> quads)
    {
        this.a = a;
        this.b = b;
        this.c = c;
        this.d = d;

        ab = Edge.FindEdge(a, b, mids, edges);
        bc = Edge.FindEdge(b, c, mids, edges);
        cd = Edge.FindEdge(c, d, mids, edges);
        ad = Edge.FindEdge(a, d, mids, edges);

        center = new Vertex_quadCenter(this);
        centers.Add(center);

        quads.Add(this);
    }
    public void subdivide(List<SubQuad> subQuads)
    {
        SubQuad quad_a = new SubQuad(a, ab.mid, center, ad.mid, subQuads);
        SubQuad quad_b = new SubQuad(b, bc.mid, center, ab.mid, subQuads);
        SubQuad quad_c = new SubQuad(c, cd.mid, center, bc.mid, subQuads);
        SubQuad quad_d = new SubQuad(d, ad.mid, center, cd.mid, subQuads);

        a.subQuads.Add(quad_a);
        b.subQuads.Add(quad_b);
        c.subQuads.Add(quad_c);
        d.subQuads.Add(quad_d);

        center.subQuads.Add(quad_a);
        center.subQuads.Add(quad_b);
        center.subQuads.Add(quad_c);
        center.subQuads.Add(quad_d);
        ab.mid.subQuads.Add(quad_a);
        ab.mid.subQuads.Add(quad_b);
        bc.mid.subQuads.Add(quad_b);
        bc.mid.subQuads.Add(quad_c);
        cd.mid.subQuads.Add(quad_c);
        cd.mid.subQuads.Add(quad_d);
        ad.mid.subQuads.Add(quad_d);
        ad.mid.subQuads.Add(quad_a);

        quad_a.neighbors[1] = quad_b;
        quad_a.neighborVertices.Add(quad_b, new Vertex[] { ab.mid, center });
        quad_a.neighbors[2] = quad_d;
        quad_a.neighborVertices.Add(quad_d, new Vertex[] { ad.mid, center });
        quad_b.neighbors[1] = quad_c;
        quad_b.neighborVertices.Add(quad_c, new Vertex[] { bc.mid, center });
        quad_b.neighbors[2] = quad_a;
        quad_b.neighborVertices.Add(quad_a, new Vertex[] { ab.mid, center });
        quad_c.neighbors[1] = quad_d;
        quad_c.neighborVertices.Add(quad_d, new Vertex[] { cd.mid, center });
        quad_c.neighbors[2] = quad_b;
        quad_c.neighborVertices.Add(quad_b, new Vertex[] { bc.mid, center });
        quad_d.neighbors[1] = quad_a;
        quad_d.neighborVertices.Add(quad_a, new Vertex[] { ad.mid, center });
        quad_d.neighbors[2] = quad_c;
        quad_d.neighborVertices.Add(quad_c, new Vertex[] { cd.mid, center });
    }
}
