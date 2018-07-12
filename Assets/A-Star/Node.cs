using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeType
{
    None,
    Obstacle,
    Start,
    End,
    Path,
}

public class Node : MonoBehaviour
{   
    public int X;
    public int Y;
    public NodeType TileType;

    public int F;
    public int G;
    public int H;

    private Renderer _renderer;

    private void Awake()
    {
        if (_renderer == null)
            _renderer = GetComponent<MeshRenderer>();
    }

    public void SetTileColor()
    {
        if (TileType == NodeType.Start)
            _renderer.material = Resources.Load<Material>("BlueMaterial");
        else if(TileType == NodeType.End)
            _renderer.material = Resources.Load<Material>("RedMaterial");
        else if(TileType == NodeType.Path)
            _renderer.material = Resources.Load<Material>("GreenMaterial");
        else if(TileType == NodeType.Obstacle)
            _renderer.material = Resources.Load<Material>("BlackMaterial");
    }
}
