using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DirType
{
    Up,
    Down,
    Left,
    Right,
    LT,
    LB,
    RT,
    RB
}

public struct TileDirection
{
    public int X;
    public int Y;
    public int W;
    public DirType Type;

    public TileDirection(int x, int y, int w, DirType t)
    {
        this.X = x;
        this.Y = y;
        this.W = w;
        this.Type = t;
    }
}

public class A_Star : MonoBehaviour {

    public Node StartNode { get; set; }
    public Node EndNode { get; set; }

    public int MapW;
    public int MapH;
    public List<Node> MapInfoList { get; set; }

    private List<Node> _openList;
    private List<Node> _closeList;
    private List<Node> _path;
    
    // 맵 생성 방식에 기준하여 방향을 설정하였음.
    private TileDirection[] directions = 
    {
        // 현재 위치가 (6,2) 라면,
        new TileDirection(0, 1, 10, DirType.Up),    // 0 : 상         (6, 3)
        new TileDirection(1, 1, 14, DirType.RT),    // 4 : 우상       (7, 3)
        new TileDirection(1, 0, 10, DirType.Right),    // 1 : 우         (7, 2)         
        new TileDirection(1, -1, 14, DirType.RB),   // 5 : 우하       (7, 1)
        new TileDirection(0, -1, 10, DirType.Down),   // 2 : 하         (6, 1)
        new TileDirection(-1, -1, 14, DirType.LB),  // 6 : 좌하       (5, 1)
        new TileDirection(-1, 0, 10, DirType.Left),   // 3 : 좌         (5, 2)
        new TileDirection(-1, 1, 14, DirType.LT)    // 7 : 좌상       (5, 3)
    };
    
    private void Awake()
    {
        _openList = new List<Node>();
        _closeList = new List<Node>();
        _path = new List<Node>();        
    }

    private void Start()
    {
        Start_A_Star();
        FindPath();
        PathHighligh();
    }

    private void Start_A_Star()    
    {
        // 경로 채점
        PathScoring(StartNode, StartNode);
        
        // 열린목록에 추가
        _openList.Add(StartNode);

        while (_openList.Count > 0)
        {
            Node node = _openList[0];

            // 열린 목록에서 가장작은 F비용을 가지고 있는 노드를 선택한다.
            for (int i = 0; i < _openList.Count; ++i)
            {
                if (_openList[i].F < node.F)
                    node = _openList[i];
            }

            if (node.TileType == NodeType.End)
                break;

            // 선택한 노드를 열린목록에서 빼고 닫힘목록에 넣는다.
            _openList.Remove(node);
            _closeList.Add(node);

            // 인접노드를 체크한다.
            CheckNearNode(node);
        }
    }
        
    /// <summary>
    /// 선택한 타일에 인접한 8개의 타일에 대해 탐색한다.
    /// </summary>
    /// <param name="cur"> 현재 노드 </param>
    private void CheckNearNode(Node curNode)
    {        
        // 시계방향으로 돈다.
        for (int i = 0; i < directions.Length; i++)
        {
            // 방향에 따른 인접타일의 좌표
            TileDirection dir = directions[i];
            int x = curNode.X + dir.X;
            int y = curNode.Y + dir.Y;

            // 맵의 크기를 벗어나는지 확인
            if (x < 0 || y < 0 || x >= MapW || y >= MapH)
                continue;

            // 타일좌표에 해당하는 노드를 가져온다.
            Node node = MapInfoList[(MapH * x) + y];

            // 장애물이면 무시한다.            
            if (node.TileType == NodeType.Obstacle)
            {   
                continue;
            }
            
            // 닫힘목록에 있다면 무시한다.
            if (_closeList.Contains(node))
                continue;

            // 만약, 열림목록에 있지 않다면
            if (!_openList.Contains(node))
            {
                // F, G, H 값 계산
                PathScoring(curNode, node);

                // 부모를 현재 타일로 설정
                node.ParentNode = curNode;

                // 열림목록에 추가
                _openList.Add(node);
            }
            else // 열림목록에 있다면
            {
                // G비용을 이용하여 이 사각형이 더 나은가 알아보고
                //if (node.G < (curNode.G + dir.W))
                //{
                //    Debug.LogFormat("Node : ({0},{1}) === CurNode({2},{3}), G({4} / {5})", node.X, node.Y, curNode.X, curNode.Y, node.G, (curNode.G + dir.W));
                //}
                if (node.G > (curNode.G + dir.W))
                {
                    PathScoring(curNode, node);
                    node.ParentNode = curNode;
                }
            }

        }

        
    }

    private void FindPath()
    {
        Node tile = EndNode;
        while (tile != null)
        {
            _path.Add(tile);

            if (tile.ParentNode.Equals(StartNode))
                break;

            tile = tile.ParentNode;
        }
        _path.Reverse();
    }

    private void PathHighligh()
    {
        for (int i = 0; i < _path.Count; ++i)
            _path[i].TileType = NodeType.Path;
    }

    /// <summary>
    /// 경로 채점, F=G+H 공식을 구하기 위해, F, G, H의 값을 각각 계산한다.
    /// </summary>
    /// <param name="cur"> 현재노드 </param>
    /// <param name="target"> 목표노드 </param>
    private void PathScoring(Node cur, Node target)
    {
        int sum = Mathf.Abs(target.X - cur.X) + Mathf.Abs(target.Y - cur.Y);

        if (sum >= 2)
            target.G = cur.G + 14; // 우상, 우하, 좌상, 좌하
        else if (sum == 1)
            target.G = cur.G + 10; // 상, 하, 좌, 우
        else
            target.G = cur.G + 0; // 현위치

        target.H = (Mathf.Abs(EndNode.X - target.X) * 10) + (Mathf.Abs(EndNode.Y - target.Y) * 10);
        target.F = target.G + target.H;

        if (target.X == 2 && target.Y == 2)
        {
            Debug.Log("");
        }

    }
}
