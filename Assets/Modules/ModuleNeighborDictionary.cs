using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleNeighborDictionary
{
    //���������©�����ŵ��³�ʼ�����ɹ�����debug�������
    public static Dictionary<string, HashSet<string>> neighborDictionary = new Dictionary<string, HashSet<string>>()
    {
        {"a",new HashSet<string>{"a"} },
        {"b",new HashSet<string>{"b"} },
        {"c",new HashSet<string>{"c"} },
        {"d",new HashSet<string>{"d"} },
    };
}
