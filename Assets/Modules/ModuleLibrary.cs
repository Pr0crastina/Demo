using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;

[CreateAssetMenu(menuName = "ScriptableObject/ModuleLibrary")]

public class ModuleLibrary : ScriptableObject
{
    [SerializeField]
    private GameObject importedModules;
    public Dictionary<string, List<Module>> moduleLibrary = new Dictionary<string, List<Module>>();
    private void Awake()
    {
        ImportedModule();
    }

    public void ImportedModule()
    {
        for (int i = 1; i < 256; i++)
        { moduleLibrary.Add(Convert.ToString(i, 2).PadLeft(8, '0'), new List<Module>()); }
        foreach (Transform child in importedModules.transform)
        {
            Mesh mesh = child.GetComponent<MeshFilter>().sharedMesh;
            string name = child.name;
            string bit = name.Substring(0, 8);
            string sockets;
            if (name.Length == 8) { sockets = "aaaaaa"; }
            else { sockets = name.Substring(9, 6); }

            moduleLibrary[bit].Add(new Module(name, mesh, 0, false));
            if (!RotationEqualCheck(bit))
            {
                moduleLibrary[RotateBit(bit, 1)].Add(new Module(RotateName(bit, sockets, 1), mesh, 1, false));
                if (!RotationTwiceEqualCheck(bit))
                {
                    moduleLibrary[RotateBit(bit, 2)].Add(new Module(RotateName(bit, sockets, 2), mesh, 2, false));
                    moduleLibrary[RotateBit(bit, 3)].Add(new Module(RotateName(bit, sockets, 3), mesh, 3, false));
                    if (!FlipRotationEqualCheck(bit))
                    {
                        moduleLibrary[FlipBit(bit)].Add(new Module(FlipName(bit, sockets), mesh, 0, true));
                        moduleLibrary[RotateBit(FlipBit(bit), 1)].Add(new Module(RotateName(FlipName(bit, sockets).Substring(0, 8), FlipName(bit, sockets).Substring(9, 6), 1), mesh, 1, true));
                        moduleLibrary[RotateBit(FlipBit(bit), 2)].Add(new Module(RotateName(FlipName(bit, sockets).Substring(0, 8), FlipName(bit, sockets).Substring(9, 6), 2), mesh, 2, true));
                        moduleLibrary[RotateBit(FlipBit(bit), 3)].Add(new Module(RotateName(FlipName(bit, sockets).Substring(0, 8), FlipName(bit, sockets).Substring(9, 6), 3), mesh, 3, true));
                    }
                }
            }
        }
    }
    private string RotateBit(string bit, int time)
    {
        string result = bit;
        for (int i = 0; i < time; i++)
        { result = result[3] + result.Substring(0, 3) + result[7] + result.Substring(4, 3); }
        return result;
    }
    private string RotateName(string bit, string sockets, int time)
    {
        string result = sockets;
        for (int i = 0; i < time; i++)
        { result = result.Substring(3, 1) + result.Substring(0, 3) + result.Substring(4); }
        return RotateBit(bit, time) + "_" + result;
    }
    private string FlipBit(string bit)
    { return bit[3].ToString() + bit[2] + bit[1] + bit[0] + bit[7] + bit[6] + bit[5] + bit[4]; }

    public string FlipName(string bit, string sockets)
    {
        string result = sockets;
        result = result.Substring(2, 1) + result.Substring(1, 1) + result.Substring(0, 1) + result.Substring(3, 1) + result.Substring(4);
        return FlipBit(bit) + "_" + result;
    }
    private bool RotationEqualCheck(string bit)
    {
        return bit[0] == bit[1] && bit[1] == bit[2] && bit[2] == bit[3] && bit[4] == bit[5] && bit[5] == bit[6] && bit[6] == bit[7];
    }
    private bool RotationTwiceEqualCheck(string bit)
    {
        return bit[0] == bit[2] && bit[1] == bit[3] && bit[4] == bit[6] && bit[5] == bit[7];
    }
    private bool FlipRotationEqualCheck(string bit)
    {
        string symmetry_vertical = bit[3].ToString() + bit[2] + bit[1] + bit[0] + bit[7] + bit[6] + bit[5] + bit[4];
        string symmetry_horizontal = bit[1].ToString() + bit[0] + bit[3] + bit[2] + bit[5] + bit[4] + bit[7] + bit[6];
        string symmetry_02 = bit[0].ToString() + bit[3] + bit[2] + bit[1] + bit[4] + bit[7] + bit[6] + bit[5];
        string symmetry_13 = bit[2].ToString() + bit[1] + bit[0] + bit[3] + bit[6] + bit[5] + bit[4] + bit[7];
        return bit == symmetry_horizontal || bit == symmetry_vertical || bit == symmetry_02 || bit == symmetry_13;
    }
    public List<Module> GetModules(string name)
    {
        List<Module> result;
        if (moduleLibrary.TryGetValue(name, out result))
            return result;
        return null;
    }
}