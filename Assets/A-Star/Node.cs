#if UNITY_EDITOR
using UnityEditor;
#endif

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
    public int _X;
    public int X { get { return _X; } set { _X = value; posX_TextMesh.text = _X.ToString(); } }

    public int _Y;
    public int Y { get { return _Y; } set { _Y = value; posY_TextMesh.text = _Y.ToString(); } }

    private NodeType _tileType = NodeType.None;
    public NodeType TileType
    {
        get { return _tileType; }
        set { _tileType = value; SetTileColor(); }
    }

    // 현재까지 이동하는데 걸린 비용과 예상 비용을 합친 총 비용.
    public int _F;
    public int F { get { return _F; } set { _F = value; F_TextMesh.text = _F.ToString(); } }
    // 시작점 A로부터 현재 사각형까지 경로를 따라 이동하는데 소요되는 비용.
    public int _G;
    public int G { get { return _G; } set { _G = value; G_TextMesh.text = _G.ToString(); } }
    // 현재 사각형에서 목적지 B까지의 예상 이동비용.(사이의 장애물 등으로 인해 실제 거리는 알지 못하고, 무시한 예상거리를 산출. 가로/세로 이동비용만 계산함. - 맨하탄 방식)
    public int _H;
    public int H { get { return _H; } set { _H = value; H_TextMesh.text = _H.ToString(); } }

    public Node _ParentNode;
    public Node ParentNode { get { return _ParentNode; } set { _ParentNode = value; LookArrow(); } }

    public TextMesh posX_TextMesh;
    public TextMesh posY_TextMesh;
    public TextMesh G_TextMesh;
    public TextMesh H_TextMesh;
    public TextMesh F_TextMesh;
    public SpriteRenderer LookRenderer;
    
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

    public void LookArrow()
    {
        if (LookRenderer != null && _ParentNode != null)
        {
            Vector3 dir = _ParentNode.transform.localPosition - transform.localPosition;
            float cosSeta = Vector3.Dot(dir.normalized, Vector3.down);
            float rad = Mathf.Acos(cosSeta);
            float deg = rad * Mathf.Rad2Deg;

            Debug.LogFormat("({0}, {1}), Degree : {2}", X, Y, deg);

            float angle =  ContAngle(Vector3.down, dir.normalized);
            Debug.LogFormat("Angle : {0}", angle);

            LookRenderer.transform.localRotation = Quaternion.identity;
            LookRenderer.transform.Rotate(Vector3.forward, angle);
        }
    }

    public float ContAngle(Vector3 fwd, Vector3 targetDir)
    {
        float angle = Vector3.Angle(fwd, targetDir);

        if (AngleDir(fwd, targetDir, Vector3.up) == -1)
        {
            angle = 360.0f - angle;
            if (angle > 359.9999f)
                angle -= 360.0f;
            return angle;
        }
        else
            return angle;
    }

    public int AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        
        //float dir = Vector3.Dot(perp, up);
        float dir = Vector3.Dot(perp, Vector3.forward);

        if (dir > 0.0)
            return 1;
        else if (dir < 0.0)
            return -1;
        else
            return 0;
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(Node))]
public class NodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Look"))
        {
            Node n = target as Node;
            if (n != null)
            {
                n.LookArrow();
            }
        }
    }
}

#endif