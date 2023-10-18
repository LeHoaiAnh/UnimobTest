using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class Node2
{
    public Vector2 pos;
    public int area;
    public bool walkable;

    public Node2(int i, int j, int area, bool walkable)
    {
        pos = new Vector2(i, j);
        this.area = area;
        this.walkable = walkable;
    }
}

[System.Serializable]
public class Node
{
    public int x;
    public int y;
    public float fCost;
    public float gCost;
    public float hCost;
    public Node parent;

    public Node(int x, int y)
    {
        ResetNode(x, y);
    }

    public void ResetNode(int x, int y)
    {
        this.x = x;
        this.y = y;
        fCost = 0;
        gCost = 0;
        hCost = 0;
        parent = null;
    }
}

public enum BLOCKTYPE
{
    PATH,
    OBSTACLE,
    GATE1,
    GATE2,
    GATE3,
    END
}
public class BlockInfor
{
    public Vector2Int postion;
    public BLOCKTYPE type;
}

[System.Serializable]
public class BlockConfig
{
    public BLOCKTYPE type;
    public List<Vector2Int> pos;
}

[System.Serializable]
public class Graph
{
    public int[,] area;
    public bool[,] walkable;
    public bool[,] visted;
    public int sizeX;
    public int sizeY;
    public float minTurnCost = 0.001f;

    public Graph(int sizeX, int sizeY)
    {
        this.sizeX = sizeX;
        this.sizeY = sizeY;
        area = new int[sizeX + 1, sizeY + 1];
        walkable = new bool[sizeX + 1, sizeY + 1];
        visted = new bool[sizeX + 1, sizeY + 1];
    }

    public void Clear()
    {
        for (int i = 0; i <= sizeX; ++i)
        {
            for (int j = 0; j <= sizeY; ++j)
            {
                area[i, j] = 0;
                visted[i, j] = false;
                walkable[i, j] = true;
            }
        }
    }

    public void ResetVisted()
    {
        for (int i = 1; i <= sizeX; i++)
        {
            for (int j = 1; j <= sizeY; j++)
            {
                visted[i, j] = false;
                area[i, j] = i * sizeY + j;
            }
        }
    }

    public void UpdateGraph()
    {
        ResetVisted();
        for (int i = 1; i <= sizeX; i++)
        {
            for (int j = 1; j <= sizeY; j++)
            {
                if (!visted[i, j] && walkable[i, j])
                {
                    DFS(i, j);
                }
            }
        }
    }

    private void DFS(int x, int y)
    {
        visted[x, y] = true;
        SmallDFS(x - 1, y, area[x, y]);
        SmallDFS(x + 1, y, area[x, y]);
        SmallDFS(x, y - 1, area[x, y]);
        SmallDFS(x, y + 1, area[x, y]);
    }

    private void SmallDFS(int x, int y, int ara)
    {
        if (x >= 1 & y >= 1 && x <= sizeX && y <= sizeY)
        {
            if (!visted[x, y] && walkable[x, y])
            {
                area[x, y] = ara;
                DFS(x, y);
            }
        }
    }
    public bool CheckHasPath(int i, int j, Vector2Int startGate1, Vector2Int startGate2, Vector2Int startGate3, Vector2Int endGate)
    {
        walkable[i, j] = false;
        UpdateGraph();
        walkable[i, j] = true;

        bool condition = true;
        if (startGate1 != Vector2Int.zero)
        {
            condition = condition && area[(int)startGate1.x, (int)startGate1.y] == area[(int)endGate.x, (int)endGate.y];
        }
        if (startGate2 != Vector2Int.zero)
        {
            condition = condition && area[(int)startGate2.x, (int)startGate2.y] == area[(int)endGate.x, (int)endGate.y];

        }
        if (startGate3 != Vector2Int.zero)
        {
            condition = condition &&
                        area[(int)startGate3.x, (int)startGate3.y] == area[(int)endGate.x, (int)endGate.y];
        }
        return condition;
    }

    public bool SetTemporaryCondition(int i, int j, bool isPut)
    {
        bool tmp = walkable[i, j];
        walkable[i, j] = isPut;
        /*
        UpdateGraph();
        */
        return tmp;
    }

    public void ResetTemporaryCondition(int i, int j, bool tmp)
    {
        walkable[i, j] = tmp;
    }

    public void UpdateGraphAtBlock(int x, int y, bool isPut = true)
    {
        if (isPut)
        {
            walkable[x, y] = false;
        }
        else
        {
            walkable[x, y] = true;
        }
        UpdateGraph();
    }

    #region FindPath
    public float ComputeHCost(int startX, int startY, int targetX, int targetY)
    {
        return Math.Abs(targetX - startX) + Math.Abs(targetY - startY) + CheckNeedTurnAtLeastOnce(startX, startY, targetX, targetY);
    }

    static readonly List<Node> listCacheNodes = new List<Node>();
    static readonly List<Node> listActiveNodes = new List<Node>();

    static Node GetCacheNode(int x, int y)
    {
        var c = listCacheNodes.Count;
        if (c > 0)
        {
            var node = listCacheNodes[c - 1];
            listCacheNodes.RemoveAt(c - 1);

            node.ResetNode(x, y);

            listActiveNodes.Add(node);
            return node;
        }
        else
        {
            var node = new Node(x, y);
            listActiveNodes.Add(node);
            return node;
        }
    }
    static void ReleaseNode(Node n)
    {
        listActiveNodes.Remove(n);
        listCacheNodes.Add(n);
    }

    static void ReleaseAllCachedNode()
    {
        listCacheNodes.AddRange(listActiveNodes);
        listActiveNodes.Clear();
    }

    static Node GetLowestNodeFCost(List<Node> nodes)
    {
        Node result = null;
        for (int i = 0; i < nodes.Count; ++i)
        {
            var n = nodes[i];

            if (result == null || result.fCost > n.fCost)
            {
                result = n;
            }
        }
        return result;
    }

    static Node GetNodeByPos(List<Node> nodes, int x, int y)
    {
        for (int i = 0; i < nodes.Count; ++i)
        {
            var n = nodes[i];

            if (n.x == x && n.y == y)
            {
                return n;
            }
        }
        return null;
    }

    public bool FindPath(List<Vector2Int> path, int startPointX, int startPointY, int targetPointX, int targetPointY)
    {
        path.Clear();

        Node current = null;
        Node start = GetCacheNode(startPointX, startPointY);
        List<Node> openNodes = HoaiAnh.Util.ListPool<Node>.Claim();
        List<Node> closedNodes = HoaiAnh.Util.ListPool<Node>.Claim();

        openNodes.Add(start);

        bool result = false;

        while (openNodes.Count > 0)
        {
            current = GetLowestNodeFCost(openNodes);

            closedNodes.Add(current);
            openNodes.Remove(current);

            if (GetNodeByPos(closedNodes, targetPointX, targetPointY) != null)
            {
                break;
            }

            List<Node> adjNodes = HoaiAnh.Util.ListPool<Node>.Claim();
            GetWalkableAdjacentNodes(adjNodes, current.x, current.y);
            float g = current.gCost + 1;
            for (int i = 0; i < adjNodes.Count; ++i)
            {
                var VARIABLE = adjNodes[i];
                float turnCost = CheckTurnCorner(current, VARIABLE) ? minTurnCost : 0;

                if (GetNodeByPos(closedNodes, VARIABLE.x, VARIABLE.y) != null)
                {
                    continue;
                }

                var e2 = GetNodeByPos(openNodes, VARIABLE.x, VARIABLE.y);
                if (e2 == null)
                {
                    VARIABLE.gCost = g + turnCost;
                    VARIABLE.hCost = ComputeHCost(VARIABLE.x, VARIABLE.y, targetPointX, targetPointY);
                    VARIABLE.fCost = VARIABLE.gCost + VARIABLE.hCost;
                    VARIABLE.parent = current;

                    openNodes.Insert(0, VARIABLE);
                }
                else
                {
                    if (g + turnCost + e2.hCost < e2.fCost)
                    {
                        e2.gCost = g + turnCost;
                        e2.fCost = e2.gCost + e2.hCost;
                        e2.parent = current;
                    }
                }
            }
            HoaiAnh.Util.ListPool<Node>.Release(adjNodes);
        }

        while (current != null)
        {
            path.Insert(0, new Vector2Int(current.x, current.y));
            current = current.parent;
        }

        HoaiAnh.Util.ListPool<Node>.Release(openNodes);
        HoaiAnh.Util.ListPool<Node>.Release(closedNodes);

        ReleaseAllCachedNode();

        return result;
    }

    private bool CheckTurnCorner(Node current, Node check)
    {
        if (current.parent == null)
        {
            return false;
        }

        Vector2 a = new Vector2(current.parent.x - current.x, current.parent.y - current.y);
        Vector2 b = new Vector2(current.x - check.x, current.y - check.y);

        return !((int)a.x == (int)b.x && (int)a.y == (int)b.y);
    }

    private float CheckNeedTurnAtLeastOnce(int startX, int startY, int targetX, int targetY)
    {
        return (targetX - startX == 0 || targetY - startY == 0) ? 0 : minTurnCost;
    }
    public void GetWalkableAdjacentNodes(List<Node> nodes, int posX, int posY)
    {
        nodes.Clear();
        Node node1 = CheckNodeCanWalk(posX - 1, posY, posX, posY);
        Node node2 = CheckNodeCanWalk(posX + 1, posY, posX, posY);
        Node node3 = CheckNodeCanWalk(posX, posY - 1, posX, posY);
        Node node4 = CheckNodeCanWalk(posX, posY + 1, posX, posY);

        if (node3 != null)
        {
            nodes.Add(node3);
        }
        if (node4 != null)
        {
            nodes.Add(node4);
        }

        if (node1 != null)
        {
            nodes.Add(node1);
        }
        if (node2 != null)
        {
            nodes.Add(node2);
        }
    }

    public Node CheckNodeCanWalk(int newX, int newY, int oldX, int oldY)
    {
        if (newX >= 1 & newY >= 1 && newX <= sizeX && newY <= sizeY)
        {
            if (walkable[newX, newY] && area[newX, newY] == area[oldX, oldY])
            {
                return GetCacheNode(newX, newY);
            }
        }

        return null;
    }

    public void FindPath(List<Vector2Int> path, Vector3 startPos, Vector3 targetPos)
    {
        path.Clear();

        int startX = (int)Mathf.Round(startPos.x);
        int startY = (int)Mathf.Round(startPos.z);
        int endX = (int)Mathf.Round(targetPos.x);
        int endY = (int)Mathf.Round(targetPos.z);

        FindPath(path, startX, startY, endX, endY);
    }

    #endregion
}
public class BFS
{
    public Graph normalGraph;

    public int sizeX;
    public int sizeY;

    public Vector2Int startGate1;
    public Vector2Int startGate2;
    public Vector2Int startGate3;
    public Vector2Int endGate;


    public Graph GetGraph()
    {
        return normalGraph;
    }

    public void ResetGraph(List<BlockConfig> blockControllers, int sizeX, int sizeY)
    {
        this.sizeX = sizeX;
        this.sizeY = sizeY;

        startGate1 = Vector2Int.zero;
        startGate2 = Vector2Int.zero;
        startGate3 = Vector2Int.zero;

        if (normalGraph == null ||
            normalGraph.sizeX != sizeX || normalGraph.sizeY != sizeY)
        {
            //for normal graph
            normalGraph = new Graph(sizeX, sizeY);
        }
        else
        {
            normalGraph.Clear();
        }

        var graph = GetGraph();
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                if (i == 0 || j == 0 || i == sizeX - 1 || j == sizeY - 1)
                {
                    graph.walkable[i, j] = false;
                }
                else
                {
                    graph.walkable[i, j] = true;

                }
                graph.area[i, j] = (i - 1) * sizeX + j;
            }
        }
        for (int i = 0; i < blockControllers.Count; ++i)
        {
            var blockType = blockControllers[i];
            if (blockType == null || blockType.pos == null || blockType.pos.Count == 0)
            {
                continue;
            }

            for (int j = 0; j < blockType.pos.Count; ++j)
            {
                var VARIABLE = blockType.pos[j];
                int px = VARIABLE.x;
                int py = VARIABLE.y;

                graph.area[px, py] = (py - 1) * sizeX + px;

                if (blockType.type == BLOCKTYPE.OBSTACLE)
                {
                    graph.walkable[px, py] = false;
                }
                else
                {
                    graph.walkable[px, py] = true;
                }

                switch (blockType.type)
                {
                    case BLOCKTYPE.GATE1:
                        startGate1 = VARIABLE;
                        break;
                    case BLOCKTYPE.GATE2:
                        startGate2 = VARIABLE;
                        break;
                    case BLOCKTYPE.GATE3:
                        startGate3 = VARIABLE;
                        break;
                    case BLOCKTYPE.END:
                        endGate = VARIABLE;
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public void ResetVisted()
    {
        var graph = GetGraph();
        if (graph != null)
        {
            graph.ResetVisted();
        }
    }

    public void UpdateGraph()
    {
        var graph = GetGraph();
        if (graph != null) graph.UpdateGraph();
    }

    public void FindPath(List<Vector2Int> path, Vector3 startPos, Vector3 targetPos)
    {
        var graph = GetGraph();
        if (graph != null)
        {
            graph.FindPath(path, startPos, targetPos);
        }
    }

    public bool SetTemporaryCondition(int i, int j, bool isPut)
    {
        var graph = GetGraph();
        if (graph != null)
        {
            return graph.SetTemporaryCondition(i, j, isPut);
        }

        return false;
    }

    public void ResetTemporaryCondition(int i, int j, bool tmp)
    {
        var graph = GetGraph();
        if (graph != null)
        {
            graph.ResetTemporaryCondition(i, j, tmp);
        }
    }

    public bool CheckHasPath(int i, int j)
    {
        var graph = GetGraph();
        if (graph != null)
        {
            return graph.CheckHasPath(i, j, startGate1, startGate2, startGate3, endGate);
        }

        return false;
    }

    public bool FindPathFromGate(List<Vector2Int> path, BLOCKTYPE gate)
    {
        var re = false;
        var graph = GetGraph();
        if (graph != null)
        {
            graph.UpdateGraph();
            Vector2Int st = Vector2Int.zero;
            switch (gate)
            {
                case BLOCKTYPE.GATE1:
                    st = startGate1;
                    break;
                case BLOCKTYPE.GATE2:
                    st = startGate2;
                    break;
                case BLOCKTYPE.GATE3:
                    st = startGate3;
                    break;
                default:
#if UNITY_EDITOR
                    Debug.LogError("Check gate to find path");
#endif
                    return false;
            }

            re = graph.FindPath(path, (int)st.x, (int)st.y, (int)this.endGate.x, (int)this.endGate.y);
            return re;
        }

        return false;
    }

    public void UpdateGraphAtBlock(int x, int y, bool isPut = true)
    {
        var graph = GetGraph();
        if (graph != null) graph.UpdateGraphAtBlock(x, y, isPut);
    }

}
