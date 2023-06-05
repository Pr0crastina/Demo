using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleNeighborDictionary
{
    //他妈的这里漏了括号导致初始化不成功后面debug半天错误
    public static Dictionary<string, HashSet<string>> neighborDictionary = new Dictionary<string, HashSet<string>>()
    {
        {"a",new HashSet<string>{"a"} },
        {"b",new HashSet<string>{"b"} },
        {"c",new HashSet<string>{"c"} },
        {"d",new HashSet<string>{"d"} },
    };
}
