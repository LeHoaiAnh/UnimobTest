using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    public Vector2Int sizeMap;
    public List<BlockConfig> blockInfors;
    private BFS bfs;
    private List<Vector2Int> path1;
    private List<Vector2Int> path2;
    private List<Vector2Int> path3;

    public List<Vector2Int> Path1 { get => path1; set => path1 = value; }
    public List<Vector2Int> Path2 { get => path2; set => path2 = value; }
    public List<Vector2Int> Path3 { get => path3; set => path3 = value; }

    private void Awake()
    {
        ResetPath();
        GetGraph();
        CachePath();
    }

    private void ResetPath()
    {
        if (Path1 == null) Path1 = new List<Vector2Int>();
        else Path1.Clear();
        if (Path2 == null) Path2 = new List<Vector2Int>();
        else Path2.Clear();
        if (Path3 == null) Path3 = new List<Vector2Int>();
        else Path3.Clear();

        bfs = new BFS();
    }

    void GetGraph()
    {
        bfs.ResetGraph(blockInfors, sizeMap.x, sizeMap.y);
    }

    void CachePath()
    {
        bfs.FindPathFromGate(Path1, BLOCKTYPE.GATE1);
        bfs.FindPathFromGate(Path2, BLOCKTYPE.GATE2);
        bfs.FindPathFromGate(Path3, BLOCKTYPE.GATE3);
    }
}
