using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
//自定义类必须要序列化才可显示在Inspector中
public class Module
{
    public string name;
    public string bit;
    public string[] sockets = new string[6];
    public Mesh mesh;
    public int rotation;
    public bool flip;
    public static Dictionary<int, int> neighborSocket = new Dictionary<int, int>() { { 0, 3 }, { 1, 2 }, { 2, 1 }, { 3, 0 }, { 4, 5 }, { 5, 4 } };
    public Module(string name, Mesh mesh, int rotation, bool flip)
    {
        this.name = name;
        this.bit = name.Substring(0, 8);

        if (name.Length != 8)
        {
            sockets[0] = name.Substring(9, 1);
            sockets[1] = name.Substring(10, 1);
            sockets[2] = name.Substring(11, 1);
            sockets[3] = name.Substring(12, 1);
            sockets[4] = name.Substring(13, 1);
            sockets[5] = name.Substring(14, 1);
        }
        else
        {
            sockets[0] = "a";
            sockets[1] = "a";
            sockets[2] = "a";
            sockets[3] = "a";
            sockets[4] = "a";
            sockets[5] = "a";
        }

        this.mesh = mesh;
        this.rotation = rotation;
        this.flip = flip;
    }
}
