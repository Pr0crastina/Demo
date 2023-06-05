using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UI;
using UnityEngine;

public class SlotColliderSystem : MonoBehaviour
{
    private string GetSlotColliderName(Vertex_Y vertex_Y)
    {
        return "SlotCollider_" + vertex_Y.name;
    }
    public void CreatCollider(Vertex_Y vertex_Y)
    {
        //创建Slot模块父对象
        GameObject slotCollider = new GameObject(GetSlotColliderName(vertex_Y), typeof(SlotCollider));
        slotCollider.GetComponent<SlotCollider>().vertex_Y = vertex_Y;
        slotCollider.transform.SetParent(transform);
        slotCollider.transform.localPosition = vertex_Y.worldPosition;

        //创建顶部碰撞面 - 与创建CursorUI一致
        GameObject top = new GameObject("Top_to_" + (vertex_Y.y + 1), typeof(MeshCollider), typeof(SlotCollider_Top));
        top.GetComponent<MeshCollider>().sharedMesh = vertex_Y.vertex.CreatMesh();
        top.layer = LayerMask.NameToLayer("SlotCollider");
        top.transform.SetParent(slotCollider.transform);
        top.transform.localPosition = Vector3.up * Grid.cellHeight * (0.5f);
        //创建底部碰撞面
        GameObject bottom = new GameObject("Bottom_to_" + (vertex_Y.y + 1), typeof(MeshCollider), typeof(SlotCollider_Bottom));
        bottom.GetComponent<MeshCollider>().sharedMesh = vertex_Y.vertex.CreatMesh();
        bottom.GetComponent<MeshCollider>().sharedMesh.triangles = bottom.GetComponent<MeshCollider>().sharedMesh.triangles.Reverse().ToArray();
        bottom.layer = LayerMask.NameToLayer("SlotCollider");
        bottom.transform.SetParent(slotCollider.transform);
        bottom.transform.localPosition = Vector3.down * Grid.cellHeight * (0.5f);
        //创建侧面碰撞面，并计算相邻块
        if (vertex_Y.vertex is Vertex_center)
        {
            List<Mesh> meshes = ((Vertex_center)vertex_Y.vertex).CreateSideMesh();
            for (int i = 0; i < vertex_Y.vertex.subQuads.Count; i++)
            {
                Vertex_Y neighbor = vertex_Y.vertex.subQuads[i].d.vertex_Ys[vertex_Y.y];
                GameObject side = new GameObject("Center" + "Side_to" + neighbor.name, typeof(MeshCollider), typeof(SlotCollider_Side));
                side.GetComponent<SlotCollider_Side>().neighbor = neighbor;
                side.GetComponent<MeshCollider>().sharedMesh = meshes[i];
                side.layer = LayerMask.NameToLayer("SlotCollider");
                side.transform.SetParent(slotCollider.transform);
                side.transform.localPosition = Vector3.zero;
            }
        }
        else if (vertex_Y.vertex is Vertex_hex)
        {
            List<Mesh> meshes = ((Vertex_hex)vertex_Y.vertex).CreateSideMesh();
            for (int i = 0; i < vertex_Y.vertex.subQuads.Count; i++)
            {
                Vertex_Y neighbor = vertex_Y.vertex.subQuads[i].b.vertex_Ys[vertex_Y.y];
                GameObject side = new GameObject("Hex" + "Side_to" + neighbor.name, typeof(MeshCollider), typeof(SlotCollider_Side));
                side.GetComponent<SlotCollider_Side>().neighbor = neighbor;
                side.GetComponent<MeshCollider>().sharedMesh = meshes[i];
                side.layer = LayerMask.NameToLayer("SlotCollider");
                side.transform.SetParent(slotCollider.transform);
                side.transform.localPosition = Vector3.zero;
            }
        }
        else
        {
            List<Mesh> meshes = ((Vertex_mid)vertex_Y.vertex).CreateSideMesh();
            for (int i = 0; i < 4; i++)
            {
                Vertex_Y neighbor;
                if (vertex_Y.vertex == vertex_Y.vertex.subQuads[i].b)
                {
                    neighbor = vertex_Y.vertex.subQuads[i].c.vertex_Ys[vertex_Y.y];
                }
                else
                {
                    neighbor = vertex_Y.vertex.subQuads[i].a.vertex_Ys[vertex_Y.y];
                }
                GameObject side = new GameObject("Mid" + "Side_to" + neighbor.name, typeof(MeshCollider), typeof(SlotCollider_Side));
                side.GetComponent<SlotCollider_Side>().neighbor = neighbor;
                side.GetComponent<MeshCollider>().sharedMesh = meshes[i];
                side.layer = LayerMask.NameToLayer("SlotCollider");
                side.transform.SetParent(slotCollider.transform);
                side.transform.localPosition = Vector3.zero;
            }
        }
    }
    public void DestroyCollider(Vertex_Y vertex_Y)
    {
        Destroy(transform.Find(GetSlotColliderName(vertex_Y)).gameObject);
        Resources.UnloadUnusedAssets();
    }
}
public class SlotCollider : MonoBehaviour
{
    public Vertex_Y vertex_Y;
}
public class SlotCollider_Top : MonoBehaviour { }
public class SlotCollider_Bottom : MonoBehaviour { }
public class SlotCollider_Side : MonoBehaviour { public Vertex_Y neighbor; }
