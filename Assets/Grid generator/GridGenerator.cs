using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField]
    private int radius;
    [SerializeField]
    private int height;
    [SerializeField]
    private int cellHeight;
    [SerializeField]
    private float scale;
    [SerializeField]
    private float cellSize;
    [SerializeField]
    private int relaxTimes;
    [SerializeField]
    public ModuleLibrary moduleLibrary;
    [SerializeField]
    private Material moduleMaterial;
    public Transform addSphere;
    public Transform deleteSphere;
    private Grid grid;
    public List<Slot> slots;
    private WorldMaster worldMaster;
    private WaveFunctionCollapse waveFunctionCollapse;
    private void Awake()
    {
        worldMaster = GetComponentInParent<WorldMaster>();
        waveFunctionCollapse = worldMaster.waveFunctionCollapse;
        grid = new Grid(radius, height, cellHeight, scale, cellSize, relaxTimes);
        moduleLibrary = Instantiate(moduleLibrary);
    }
    private void Update()
    {
        //if (relaxTimes > 0)
        //{
        //    foreach (SubQuad subquad in grid.subQuads)
        //    {
        //        subquad.CalculateRelaxOffset();
        //    }
        //    foreach (Vertex vertex in grid.vertices)
        //    {
        //        vertex.Relax();
        //    }
        //    relaxTimes -= 1;
        //}
        foreach (Vertex vertex in grid.vertices)
        {
            foreach (Vertex_Y vertex_Y in vertex.vertex_Ys)
            {
                if (!vertex_Y.isActive && Vector3.Distance(vertex_Y.worldPosition, addSphere.position) < 2f && !vertex_Y.isBoundary)
                { vertex_Y.isActive = true; }
                else if (vertex_Y.isActive && Vector3.Distance(vertex_Y.worldPosition, deleteSphere.position) < 2f)
                { vertex_Y.isActive = false; }
            }
        }
        foreach (SubQuad subQuad in grid.subQuads)
        {
            foreach (SubQuad_Cube subQuad_Cube in subQuad.subQuad_Cubes)
            {
                subQuad_Cube.UpdateBit();
                if (subQuad_Cube.pre_bit != subQuad_Cube.bit)
                { UpdateSlot(subQuad_Cube); }
            }
        }
    }
    private void UpdateSlot(SubQuad_Cube subQuad_Cube)
    {
        string name = "Slot_" + grid.subQuads.IndexOf(subQuad_Cube.subQuad) + "_" + subQuad_Cube.y;
        GameObject slot_GameObject;
        if (transform.Find(name))
        {
            slot_GameObject = transform.Find(name).gameObject;
        }
        else
        {
            slot_GameObject = null;
        }
        if (slot_GameObject == null)
        {
            if (subQuad_Cube.bit != "00000000" && subQuad_Cube.bit != "11111111")
            {
                slot_GameObject = new GameObject(name, typeof(Slot));
                slot_GameObject.transform.SetParent(transform);
                slot_GameObject.transform.localPosition = subQuad_Cube.centerPosition;
                Slot slot = slot_GameObject.GetComponent<Slot>();
                slot.Initialize(moduleLibrary, subQuad_Cube, moduleMaterial);
                slots.Add(slot);
                slot.UpdateModule(slot.possibleModules[0]);

                waveFunctionCollapse.resetSlots.Add(slot);
                waveFunctionCollapse.cur_collapseSlots.Add(slot);
            }
        }
        else
        {
            Slot slot = slot_GameObject.GetComponent<Slot>();
            if (subQuad_Cube.bit == "00000000" || subQuad_Cube.bit == "11111111")
            {
                slots.Remove(slot);
                if (waveFunctionCollapse.resetSlots.Contains(slot)) { waveFunctionCollapse.resetSlots.Remove(slot); }
                if (waveFunctionCollapse.cur_collapseSlots.Contains(slot)) { waveFunctionCollapse.cur_collapseSlots.Remove(slot); }
                Destroy(slot_GameObject);
                Resources.UnloadUnusedAssets();
            }
            else
            {
                slot.ResetSlot(moduleLibrary);
                slot.UpdateModule(slot.possibleModules[0]);
                if (!waveFunctionCollapse.resetSlots.Contains(slot)) { waveFunctionCollapse.resetSlots.Add(slot); }
                if (!waveFunctionCollapse.cur_collapseSlots.Contains(slot)) { waveFunctionCollapse.cur_collapseSlots.Add(slot); }
            }
        }
    }
    public void ToggleSlot(Vertex_Y vertex_Y)
    {
        vertex_Y.isActive = !vertex_Y.isActive;
        foreach (SubQuad_Cube subQuad_Cube in vertex_Y.subQuad_Cubes)
        {
            subQuad_Cube.UpdateBit();
            UpdateSlot(subQuad_Cube);
        }
    }
    public Grid GetGrid()
    {
        return grid;
    }
    //private void OnDrawGizmos()
    //{
    //    if (grid != null)
    //    {
    //        foreach (SubQuad subQuad in grid.subQuads)
    //        {
    //            foreach (SubQuad_Cube subQuad_Cube in subQuad.subQuad_Cubes)
    //            {
    //                Gizmos.color = Color.gray;
    //                Gizmos.DrawLine(subQuad_Cube.vertex_Ys[0].worldPosition, subQuad_Cube.vertex_Ys[1].worldPosition);
    //                Gizmos.DrawLine(subQuad_Cube.vertex_Ys[1].worldPosition, subQuad_Cube.vertex_Ys[2].worldPosition);
    //                Gizmos.DrawLine(subQuad_Cube.vertex_Ys[2].worldPosition, subQuad_Cube.vertex_Ys[3].worldPosition);
    //                Gizmos.DrawLine(subQuad_Cube.vertex_Ys[3].worldPosition, subQuad_Cube.vertex_Ys[0].worldPosition);
    //                Gizmos.DrawLine(subQuad_Cube.vertex_Ys[4].worldPosition, subQuad_Cube.vertex_Ys[5].worldPosition);
    //                Gizmos.DrawLine(subQuad_Cube.vertex_Ys[5].worldPosition, subQuad_Cube.vertex_Ys[6].worldPosition);
    //                Gizmos.DrawLine(subQuad_Cube.vertex_Ys[6].worldPosition, subQuad_Cube.vertex_Ys[7].worldPosition);
    //                Gizmos.DrawLine(subQuad_Cube.vertex_Ys[7].worldPosition, subQuad_Cube.vertex_Ys[4].worldPosition);
    //                Gizmos.DrawLine(subQuad_Cube.vertex_Ys[0].worldPosition, subQuad_Cube.vertex_Ys[4].worldPosition);
    //                Gizmos.DrawLine(subQuad_Cube.vertex_Ys[1].worldPosition, subQuad_Cube.vertex_Ys[5].worldPosition);
    //                Gizmos.DrawLine(subQuad_Cube.vertex_Ys[2].worldPosition, subQuad_Cube.vertex_Ys[6].worldPosition);
    //                Gizmos.DrawLine(subQuad_Cube.vertex_Ys[3].worldPosition, subQuad_Cube.vertex_Ys[7].worldPosition);

    //                Gizmos.color = Color.blue;
    //                Gizmos.DrawSphere(subQuad_Cube.centerPosition, 0.3f);

    //                GUI.color = Color.blue;
    //                Handles.Label(subQuad_Cube.centerPosition, subQuad_Cube.bit);
    //            }
    //        }
    //foreach (Vertex vertex in grid.vertices)
    //{
    //    foreach (Vertex_Y vertex_Y in vertex.vertex_Ys)
    //    {
    //        if (vertex_Y.isActive)
    //        {
    //            Gizmos.color = Color.red;
    //            Gizmos.DrawSphere(vertex_Y.worldPosition, 0.1f);
    //        }
    //        else
    //        {
    //            Gizmos.color = Color.gray;
    //            Gizmos.DrawSphere(vertex_Y.worldPosition, 0.1f);
    //        }
    //    }
    //}
    //foreach (Vertex_hex vertex in grid.hexes)
    //{
    //    Gizmos.DrawSphere(vertex.currentPosition, 0.3f);
    //    Debug.Log(vertex.coord.worldPosition);
    //    Debug.Log(vertex.coord.localPosition);
    //}
    //Gizmos.color = Color.yellow;
    //foreach (Triangle triangle in grid.triangles)
    //{
    //    Gizmos.DrawLine(triangle.a.currentPosition, triangle.b.currentPosition);
    //    Gizmos.DrawLine(triangle.a.currentPosition, triangle.c.currentPosition);
    //    Gizmos.DrawLine(triangle.b.currentPosition, triangle.c.currentPosition);
    //    Gizmos.DrawSphere((triangle.a.currentPosition + triangle.b.currentPosition + triangle.c.currentPosition) / 3, 0.05f);
    //}
    //Gizmos.color = Color.green;
    //foreach (Quad quad in grid.quads)
    //{
    //    Gizmos.DrawLine(quad.a.currentPosition, quad.b.currentPosition);
    //    Gizmos.DrawLine(quad.b.currentPosition, quad.c.currentPosition);
    //    Gizmos.DrawLine(quad.c.currentPosition, quad.d.currentPosition);
    //    Gizmos.DrawLine(quad.d.currentPosition, quad.a.currentPosition);
    //}
    //Gizmos.color = Color.red;
    //foreach (Vertex_mid mid in grid.mids)
    //{
    //    Gizmos.DrawSphere(mid.currentPosition, 0.1f);
    //}
    //Gizmos.color = Color.blue;
    //foreach (Vertex_center center in grid.centers)
    //{
    //    Gizmos.DrawSphere(center.currentPosition, 0.1f);
    //}
    //Gizmos.color = Color.blue;
    //foreach (SubQuad subQuad in grid.subQuads)
    //{
    //    Gizmos.DrawLine(subQuad.a.currentPosition, subQuad.b.currentPosition);
    //    Gizmos.DrawLine(subQuad.b.currentPosition, subQuad.c.currentPosition);
    //    Gizmos.DrawLine(subQuad.c.currentPosition, subQuad.d.currentPosition);
    //    Gizmos.DrawLine(subQuad.a.currentPosition, subQuad.d.currentPosition);
    //}
    //}
    //}
    //private void OnDrawGizmosSelected()
    //{
    //    if (grid != null)
    //    {
    //        foreach (Vertex_hex vertex in grid.hexes)
    //        {
    //            Gizmos.color = new Color(0.5f, 1, 1, 1);
    //            Gizmos.DrawSphere(vertex.coord.localPosition, 0.3f);
    //            Debug.Log(vertex.coord.worldPosition);
    //            Debug.Log(vertex.coord.localPosition);
    //        }
    //    }
    //}
}
