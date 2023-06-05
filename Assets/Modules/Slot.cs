using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Slot : MonoBehaviour
{
    public List<Module> possibleModules;
    public SubQuad_Cube subQuad_Cube;
    public GameObject module;
    public bool reset;
    public Material material;
    public Stack<List<Module>> pre_possibleModules = new Stack<List<Module>>();
    private void Awake()
    {
        module = new GameObject("Module", typeof(MeshFilter), typeof(MeshRenderer));
        module.transform.SetParent(transform);
        module.transform.localScale = Vector3.one;
        //debug: module.transform.localScale = Vector3.zero;
    }
    public void Initialize(ModuleLibrary moduleLibrary, SubQuad_Cube subQuad_Cube, Material material)
    {
        this.subQuad_Cube = subQuad_Cube;
        this.subQuad_Cube.slot = this;
        ResetSlot(moduleLibrary);
        this.material = material;
    }
    public void ResetSlot(ModuleLibrary moduleLibrary)
    {
        possibleModules = moduleLibrary.GetModules(subQuad_Cube.bit).ConvertAll(x => x);
        reset = true;
    }
    private void RotateModule(Mesh mesh, int rotation)
    {
        if (rotation != 0)
        {
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            { vertices[i] = Quaternion.AngleAxis(90 * rotation, Vector3.up) * vertices[i]; }
            mesh.vertices = vertices;
        }
    }
    private void DeformModule(Mesh mesh, SubQuad_Cube subQuad_Cube)
    {
        Vector3[] vertices = mesh.vertices;
        SubQuad subQuad = subQuad_Cube.subQuad;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 ad_x = Vector3.Lerp(subQuad.a.currentPosition, subQuad.d.currentPosition, (vertices[i].x + 0.5f));
            Vector3 bc_x = Vector3.Lerp(subQuad.b.currentPosition, subQuad.c.currentPosition, (vertices[i].x + 0.5f));
            vertices[i] = Vector3.Lerp(ad_x, bc_x, (vertices[i].z + 0.5f)) + Vector3.up * vertices[i].y * Grid.cellHeight - subQuad.GetCenterPosition();
        }
        mesh.vertices = vertices;
    }
    private void FlipModule(Mesh mesh, bool flip)
    {
        if (flip)
        {
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            { vertices[i] = new Vector3(-vertices[i].x, vertices[i].y, vertices[i].z); }
            mesh.vertices = vertices;
            mesh.triangles = mesh.triangles.Reverse().ToArray();
        }
    }
    public void UpdateModule(Module module)
    {
        this.module.GetComponent<MeshFilter>().mesh = module.mesh;
        FlipModule(this.module.GetComponent<MeshFilter>().mesh, module.flip);
        RotateModule(this.module.GetComponent<MeshFilter>().mesh, module.rotation);
        DeformModule(this.module.GetComponent<MeshFilter>().mesh, subQuad_Cube);
        this.module.GetComponent<MeshRenderer>().material = material;
        this.module.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        this.module.GetComponent<MeshFilter>().mesh.RecalculateBounds();
    }
    public void Collapse(int i)
    {
        possibleModules = new List<Module>() { possibleModules[i] };
        reset = false;
    }
}
