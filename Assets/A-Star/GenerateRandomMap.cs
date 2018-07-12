using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateRandomMap : MonoBehaviour {

    public GameObject tilePrefab;
    public GameObject obstaclePrefab;

    [Range(0, 1)]
    public float outlinePercent;

    private void Start()
    {
        GenerateMap();
        Node startNode = SetStartPoint();
        SetEndPoint();
        if (startNode != null)
        {
            SetAgent(startNode);
        }
    }

    private Node SetStartPoint()
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Node node = AllNodeList[((int)mapSize.x * x) + y];
                if (node != null)
                {
                    if (node.TileType == NodeType.None)
                    {
                        node.TileType = NodeType.Start;
                        node.SetTileColor();
                        return node;
                    }
                }
            }
        }

        return null;
    }

    private void SetEndPoint()
    {
        for (int x = (int)mapSize.x - 1; x >= 0; x--)
        {
            for (int y = (int)mapSize.y - 1; y >= 0 ; y--)
            {
                Node node = AllNodeList[((int)mapSize.x * x) + y];
                if (node != null)
                {
                    if (node.TileType == NodeType.None)
                    {
                        node.TileType = NodeType.End;
                        node.SetTileColor();
                        return;
                    }
                }
            }
        }
    }

    private void SetAgent(Node s)
    {
        GameObject obj = (GameObject)Resources.Load("space_man_player");
        if (obj != null)
        {
            obj.transform.localPosition = CoordToPosition(s.X, s.Y);
        }
    }

    #region Generate Accessible Map

    [Range(0, 1)]
    public float obstaclePercent;

    List<Coord> allTileCoords;
    Queue<Coord> shuffledTileCoords;

    public int seed = 10;

    private Vector2 mapSize = new Vector2(10, 10);

    Coord mapCentre;
    public bool isRandomSeed = false;

    List<Node> AllNodeList;

    public void GenerateMap()
    {        
        if (isRandomSeed)
            seed = Random.Range(1, 100);

        // 맵 크기에 해당하는 모든 타일 좌표를 저장.
        allTileCoords = new List<Coord>();
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));
            }
        }

        // FisherYatesShuffle 알고리즘으로 모든 타일 좌표를 섞는다.
        shuffledTileCoords = new Queue<Coord>(FisherYatesShuffle.ShuffleArray(allTileCoords.ToArray(), seed));

        // 맵의 가운데를 찾는다.
        mapCentre = new Coord((int)mapSize.x / 2, (int)mapSize.y / 2);

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
                Node newNode = newTile.AddComponent<Node>();
                newNode.X = x;
                newNode.Y = y;
                newNode.TileType = NodeType.None;
                
                AllNodeList.Add(newNode);
            }
        }

        // 장애물 여부를 저장할 2차원 배열을 맵크기만 큼 생성
        bool[,] obstacleMap = new bool[(int)mapSize.x, (int)mapSize.y];

        // 장애물 개수를 계산
        int obstacleCount = (int)(mapSize.x * mapSize.y * obstaclePercent);
        // 현재까지 만들어진 장애물의 개수
        int currentObstacleCount = 0;

        for (int i = 0; i < obstacleCount; i++)
        {
            // 셔플된 좌표중에서 하나를 가져온다.
            Coord randomCoord = GetRandomCoord();

            // 장애물 여부를 true 처리함.
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            // 장애물의 좌표가 맵의 중앙이 아니고, 접근가능한 위치에 놓여져 있다면
            if (randomCoord != mapCentre && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
            {
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);
                // 장애물을 생성한다.
                GameObject newObstacle = Instantiate(obstaclePrefab, mapHolder);
                newObstacle.transform.localPosition = obstaclePosition;
                newObstacle.transform.parent = mapHolder;

                AllNodeList[((int)mapSize.x * randomCoord.x) + randomCoord.y].TileType = NodeType.Obstacle;
                AllNodeList[((int)mapSize.x * randomCoord.x) + randomCoord.y].SetTileColor();
            }
            else
            {
                // 장애물 여부를 false 처리함.
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }

    }

    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        /*
		 * [Flood fill 알고리즘을 이용해서 닿지 않는 타일이 존재하는지 체크한다.]
		 * 중앙에는 장애물이 없는걸아니까, 먼저 obstacleMap의 중앙에서 부터 시작해서 밖으로 퍼져나가면서 타일을 검색해 나간다.
		 * 그리고 얼마나 장애물이 아닌 타일들이 있는지 숫자를 센다.
		 * 전체 타일수가 얼마나 되는지 알고 있는 상태에서, currentObstableCount를 이용해 장애물이 아닌 타일이 얼만큼 반드시 존재해야 하는지 알수 있다.
		 * 그래서 만약 Flood fill 알고리즘으로 얻은 값이 반드시 존재해야 하는 비장애물 타일 갯수와 다르다면
		 * Flood fill 알고리즘이 맵에 있는 모든 타일에 닿지 못했다는 뜻이다.
		 * 장애물에 막혀있단 의미이므로 맵전체가 접근 가능하다는 것이 아니며 false를 리턴한다.
		 * Flood fill 알고리즘을 사용할 때 중요한 점은
		 * 이미 살펴보았던 타일들을 표시해서, 같은 타일을 계속 또 살펴보지 않도록 하는것이다.
		 */

        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();

        // 비어있는 좌표는 큐에 넣어준다.
        queue.Enqueue(mapCentre);

        // 맵 중앙은 비어있는것을 아니깐 이미 체크했다는 의미로 true값을 넣어준다.
        mapFlags[mapCentre.x, mapCentre.y] = true;

        // 접근 가능한 타일의 개수를 설정한다. 중앙은 비어있으니 기본적으로 1이다.
        int accessibleTileCount = 1;

        // Flood fill을 시작한다.
        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    // 인접한 8개 타일을 순환한다.
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;

                    // 수직, 수평만 살펴보고 싶으니, 대각선 방향은 체크하지 않는다.
                    if (x == 0 || y == 0)
                    {
                        // 반드시 인접한 타일이 맵에 존재하는지 체크해야한다.
                        if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
                        {
                            // 이미 체크했는지 확인하고, 장애물이 아닌지 체크한다.
                            if (!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY])
                            {
                                // 체크했으므로 true값을 넣어준다.
                                mapFlags[neighbourX, neighbourY] = true;

                                // 비어있는 좌표이므로 큐에 넣어준다.
                                queue.Enqueue(new Coord(neighbourX, neighbourY));

                                // 접근 가능한 타일의 수를 증가시킨다.
                                accessibleTileCount++;
                            }
                        }
                    }
                }
            }
        }

        int targetAccessibleTileCount = (int)(mapSize.x * mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }

    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-mapSize.x / 2 + 0.5f + x, -mapSize.y / 2 + 0.5f + y, 0);
    }

    public Coord GetRandomCoord()
    {
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }

    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public static bool operator ==(Coord c1, Coord c2)
        {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coord c1, Coord c2)
        {
            return !(c1 == c2);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    #endregion
}
