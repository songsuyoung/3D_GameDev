using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class Grid : MonoBehaviour
{
    public Transform player;
    Node[,] grid; //grid가 있고, 한개의 패넬에 grid의 x,y좌표가 존재
    public float nodeSize;
    public Vector2 numberOfGrids; //그리드 갯수 
    public LayerMask layerMask;
    int gridSizeX,gridSizeY;
    float halfNodeSize;
    public GameObject ground;
    public List<Node> path;

    private void Awake()
    {

        //현재 그리드 수/노드 갯수
        gridSizeX = Mathf.RoundToInt(numberOfGrids.x / nodeSize);
        gridSizeY = Mathf.RoundToInt(numberOfGrids.y / nodeSize);
        halfNodeSize = nodeSize * 0.5f;

        grid = new Node [gridSizeX, gridSizeY];
        CreateGrid();
    }

    void CreateGrid()
    {
        //transform.position (0,0,0)을 의미, 1,0,0 * grid.x(width) /2 => 중앙에 위치해 있기 때문에 뺄 경우 가장 왼쪽으로 이동, y값을 빼면서 가장 아래로 내려간다. 
        //즉,
        //  . . .   . . .
        //  . o .   . . .
        //  . . .   o . .  o위치가 이동함
        //gridY height, gridX width

        Vector3 bottomLeft = ground.transform.position - Vector3.right * numberOfGrids.x / 2 - Vector3.forward * numberOfGrids.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 position = bottomLeft + Vector3.right * (x * nodeSize + halfNodeSize) + Vector3.forward * (y * nodeSize + halfNodeSize);
                //그리드의 중앙 위치를 의미, position 
                bool isWalkable = Physics.CheckSphere(position, halfNodeSize, layerMask); //중앙 위치에서 radius크기만큼 구를 이룸, 구와 layerMask가 부딪혔을 때 true가 반환되어짐.

                Debug.Log(isWalkable);
                grid[x, y] = new Node(isWalkable, position, x, y);
            }
        }

    }

    public List<Node> GetNeighborNode(Node node) 
    {
        List<Node> neighbours = new List<Node>();

        int[] rows = {-1,1,0,0 };
        int[] cols = { 0,0,-1,1 };

        for(int i=0;i<rows.Length; i++)
        {
            int nextR=node.gridX + rows[i];
            int nextC=node.gridY + cols[i];

            if (nextR >= 0 && nextR < gridSizeX && nextC >= 0 && nextC < gridSizeY)
            {
                neighbours.Add(grid[nextR, nextC]);
            }
        }
        return neighbours;
    }

    public Node GetNodeFromPosition(Vector3 position)
    {
        float percentX = (position.x + numberOfGrids.x / 2) / numberOfGrids.x;
        float percentY = (position.z + numberOfGrids.y / 2) / numberOfGrids.y;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        return grid[x,y];
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(numberOfGrids.x, 1, numberOfGrids.y));

        if (grid != null)
        {

            Node playernode = GetNodeFromPosition(player.position);

            foreach (Node n in grid)
            {
                Gizmos.color = (n.isWalkable) ? Color.white : Color.red;

                if (playernode == n)
                {
                    Gizmos.color = Color.black;
                }
                else
                {
                    if (path != null && path.Contains(n))
                        Gizmos.color = Color.black;
                }

                Gizmos.DrawCube(n.position, Vector3.one * (nodeSize - 0.1f));
            }
        }
    }

}
