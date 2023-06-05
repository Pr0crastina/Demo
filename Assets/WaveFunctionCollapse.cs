using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour
{
    private WorldMaster worldMaster;
    private GridGenerator gridGenerator;
    private ModuleLibrary moduleLibrary;

    public List<Slot> resetSlots = new List<Slot>();
    public Slot cur_collapseSlot;
    public List<Slot> cur_collapseSlots = new List<Slot>();
    public Stack<Slot> propagateSlotStack = new Stack<Slot>();
    public Slot cur_propagateSlot;
    public Stack<Slot> collapseSlotStack = new Stack<Slot>();
    public Stack<List<Slot>> collapseSlotsStack = new Stack<List<Slot>>();

    private void Awake()
    {
        worldMaster = GetComponentInParent<WorldMaster>();
        gridGenerator = worldMaster.gridGenerator;
        moduleLibrary = Instantiate(gridGenerator.moduleLibrary);
    }

    public void WFC()
    {
        Reset();
        CollapseAndPropagate();
        UpdateModule();
    }

    private void Reset()
    {
        while (resetSlots.Count > 0)
        {
            //去除第一个待传递重置的Slot
            Slot cur_resetSlot = resetSlots[0];
            resetSlots.RemoveAt(0);
            //尝试向该Slot相邻的Slot传递重置
            SubQuad_Cube[] neighbors = cur_resetSlot.subQuad_Cube.neighbors;
            foreach (SubQuad_Cube subQuad_Cube in neighbors)
            {
                if (subQuad_Cube != null && subQuad_Cube.isActive && !subQuad_Cube.slot.reset)
                {
                    //判断相邻slot是否为独立模块
                    bool independentNeighbor = true;
                    foreach (Vertex_Y vertex_Y in cur_resetSlot.subQuad_Cube.neighborVertices[subQuad_Cube])
                    { if (vertex_Y.isActive) { independentNeighbor = false; break; } }

                    if (!independentNeighbor)
                    {
                        Debug.Log("重置");
                        subQuad_Cube.slot.ResetSlot(moduleLibrary);
                        resetSlots.Add(subQuad_Cube.slot);
                        if (!cur_collapseSlots.Contains(subQuad_Cube.slot)) { cur_collapseSlots.Add(subQuad_Cube.slot); }
                    }
                }
            }

        }
    }
    private void CollapseAndPropagate()
    {
        while (cur_collapseSlots.Count > 0)
        {
            while (propagateSlotStack.Count == 0)
            { GetCollapseSlot(); Collapse(); }
            Propagate();
        }
    }
    public void GetCollapseSlot()
    {
        //计算最少可能性
        int minPossibility = cur_collapseSlots[0].possibleModules.Count;
        foreach (Slot slot in cur_collapseSlots) { if (slot.possibleModules.Count < minPossibility) minPossibility = slot.possibleModules.Count; }

        //找到Index最小的可能性并设置为cur_collapseSlot
        bool findFirst = false;
        foreach (Slot slot in cur_collapseSlots)
        {
            if (slot.possibleModules.Count == minPossibility)
            {
                if (!findFirst) { cur_collapseSlot = slot; findFirst = true; }
                else if (cur_collapseSlot.subQuad_Cube.index > slot.subQuad_Cube.index) { cur_collapseSlot = slot; }
            }
        }
    }
    public void Propagate()
    {
        cur_propagateSlot = propagateSlotStack.Pop();

        Dictionary<int, HashSet<string>> possibleSockets = new Dictionary<int, HashSet<string>>();
        for (int i = 0; i < 6; i++)
        {
            possibleSockets[i] = new HashSet<string>();
            foreach (Module module in cur_propagateSlot.possibleModules)
            {
                foreach (string socket in ModuleNeighborDictionary.neighborDictionary[module.sockets[i]]) { possibleSockets[i].Add(socket); }
            }
        }
        SubQuad_Cube[] neighbors = cur_propagateSlot.subQuad_Cube.neighbors;
        for (int i = 0; i < 6; i++)
        {
            if (neighbors[i] != null && neighbors[i].isActive)
            {
                ConstrainPossibility(neighbors, possibleSockets, i);

                if (neighbors[i].slot.possibleModules.Count == 0) { Backtrack(); break; }
            }
        }
    }
    public void Collapse()
    {
        //存储当前状况
        bool backtrackAvailable = (cur_collapseSlot.possibleModules.Count > 1);
        if (backtrackAvailable)
        {
            collapseSlotStack.Push(cur_collapseSlot);
            collapseSlotsStack.Push(cur_collapseSlots.ConvertAll(x => x));
            foreach (Slot slot in gridGenerator.slots) { slot.pre_possibleModules.Push(slot.possibleModules); }
        }

        System.Random random = new System.Random(cur_collapseSlot.subQuad_Cube.index);
        //Debug.Log($"PossibleModules:{cur_collapseSlot.possibleModules.Count}" + " " + $"Slot:{cur_collapseSlot.subQuad_Cube.slot.name}");
        int chosenModule = random.Next() % (cur_collapseSlot.possibleModules.Count);
        cur_collapseSlot.Collapse(chosenModule);
        cur_collapseSlots.Remove(cur_collapseSlot);
        propagateSlotStack.Push(cur_collapseSlot);

        //将当前的可能性去除
        if (backtrackAvailable)
        {
            List<Module> modules = cur_collapseSlot.pre_possibleModules.Pop();
            modules.Remove(cur_collapseSlot.possibleModules[0]);
            cur_collapseSlot.pre_possibleModules.Push(modules);
        }
    }
    public void ConstrainPossibility(SubQuad_Cube[] neighbors, Dictionary<int, HashSet<string>> possibleSockets, int i)
    {
        List<Module> possibleModules = neighbors[i].slot.possibleModules.ConvertAll(x => x);
        foreach (Module module in neighbors[i].slot.possibleModules)
        {
            if (!possibleSockets[i].Contains(module.sockets[Module.neighborSocket[i]]))
            {
                possibleModules.Remove(module);
                if (!propagateSlotStack.Contains(neighbors[i].slot)) { propagateSlotStack.Push(neighbors[i].slot); }
            }
        }
        neighbors[i].slot.possibleModules = possibleModules;
    }

    public void Backtrack()
    {
        Debug.Log("回溯!");
        cur_collapseSlot = collapseSlotStack.Pop();
        cur_collapseSlots = collapseSlotsStack.Pop();
        foreach (Slot slot in gridGenerator.slots) { slot.possibleModules = slot.pre_possibleModules.Pop(); }
        propagateSlotStack.Clear();
        Collapse();
    }

    public void ClearBacktrackStack()
    {
        collapseSlotStack.Clear();
        collapseSlotsStack.Clear();
        foreach (Slot slot in gridGenerator.slots) { slot.pre_possibleModules.Clear(); }
        cur_collapseSlot = null;
        cur_propagateSlot = null;
    }
    private void UpdateModule()
    {
        ClearBacktrackStack();
        foreach (Slot slot in gridGenerator.slots) { slot.UpdateModule(slot.possibleModules[0]); }
    }
}
