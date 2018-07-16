using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateSampleMap : MonoBehaviour {

    public GameObject tilePrefab;
    public GameObject obstaclePrefab;

    List<Node> AllNodeList;

    [Range(0, 1)]
    public float outlinePercent;

    private Vector2 mapSize = new Vector2(8, 6);

    private void Start()
    {
        GenerateMap();

        Node StartNode = AllNodeList[((int)mapSize.y * 2) + 3];
        StartNode.TileType = NodeType.Start;

        Node EndNode = AllNodeList[((int)mapSize.y * 6) + 3];
        EndNode.TileType = NodeType.End;

        AllNodeList[((int)mapSize.y * 4) + 2].TileType = NodeType.Obstacle;
        AllNodeList[((int)mapSize.y * 4) + 3].TileType = NodeType.Obstacle;
        AllNodeList[((int)mapSize.y * 4) + 4].TileType = NodeType.Obstacle;

        SetAgent(StartNode, EndNode);
    }

    private void SetAgent(Node s, Node e)
    {
        GameObject obj = (GameObject)Resources.Load("space_man_player");
        if (obj != null)
        {
            GameObject agent = GameObject.Instantiate(obj);
            if (agent != null)
            {
                agent.transform.localPosition = CoordToPosition(s.X, s.Y);

                A_Star a_Star = agent.GetComponent<A_Star>();
                if (a_Star != null)
                {
                    a_Star.StartNode = s;
                    a_Star.EndNode = e;
                    a_Star.MapInfoList = AllNodeList;
                    a_Star.MapW = (int)mapSize.x;
                    a_Star.MapH = (int)mapSize.y;
                }
            }

        }
    }

    private void GenerateMap()
    {
        // 자동생성된 맵이 가지게될 부모 Object 체크
        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;
        mapHolder.transform.localPosition = Vector3.zero;

        if (AllNodeList == null)
            AllNodeList = new List<Node>();
        else
            AllNodeList.Clear();

        // 맵 사이즈 만큼 반복하면서 타일을 만든다.
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y);
                GameObject newTile = Instantiate(tilePrefab, mapHolder);
                newTile.name = string.Format("Tile_{0}_{1}", x, y);
                newTile.transform.localPosition = tilePosition;
                newTile.transform.localScale = Vector3.one * (1 - outlinePercent);
                newTile.transform.parent = mapHolder;
                Node newNode = newTile.GetComponent<Node>();
                newNode.X = x;
                newNode.Y = y;
                newNode.TileType = NodeType.None;

                AllNodeList.Add(newNode);
            }
        }
    }

    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-mapSize.x / 2 + 0.5f + x, -mapSize.y / 2 + 0.5f + y, 0);
    }
}
