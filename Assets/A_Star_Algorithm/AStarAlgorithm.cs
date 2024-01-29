using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarAlgorithm : MonoBehaviour
{
    public Transform start;
    public Transform end;

    Vector3 cachedStart, cachedEnd;
    Grid grid;

    private void Start()
    {
        grid = GetComponent<Grid>();

    }
    private void Update()
    {
        if (start.position != cachedStart || end.position != cachedEnd)
        {
            FindPath(start.position, end.position);
            cachedStart = start.position;
            cachedEnd = end.position;
        }
    }

    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = grid.GetNodeFromPosition(startPos);
        Node endNode = grid.GetNodeFromPosition(targetPos);

        PriorityQueue<Node> openSet = new PriorityQueue<Node>();
        HashSet<Node> closedSet = new HashSet<Node>(); //닫힌 노드

        openSet.Add(startNode);

        while(!openSet.isEmpty())
        {

            Node currentNode = openSet.Top();
            openSet.Remove();

            if (closedSet.Contains(currentNode)) continue; //가지고 있음 멈춘다.
            closedSet.Add(currentNode); //현재 노드 다음 탐색 안하도록 함.

            if (currentNode == endNode)
            {
                RetracePath(startNode, endNode);
                return; //현재노드가 마지막 노드에 도달하면 멈춘다.
            }

            foreach (Node neighbor in grid.GetNeighborNode(currentNode))
            {
                if (!neighbor.isWalkable || closedSet.Contains(neighbor))
                {
                    continue;
                }
                //현재 노드의 gcost / hcost 가져오기
                int g=  currentNode.gCost + GetDistance(currentNode, neighbor); //내 현재 위치에서 부터 이웃 위치의 떨어진 휴리스틱 거리
                int h = GetDistance(neighbor, endNode); //마지막 노드까지의 거리 계산
                int f = g + h;
                if(!openSet.Contains(neighbor))
                {
                    neighbor.gCost = g;
                    neighbor.hCost = h;
                    neighbor.parent = currentNode; //길을 저장하기 위해서 parent 를 알아야한다.
                    openSet.Add(neighbor);
                }
                else
                {
                    if(f < neighbor.fCost)
                    {
                        neighbor.gCost = g;
                        neighbor.hCost = h;
                    }
                }
            }
        }
    }

    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode=endNode;

        while(startNode != currentNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse(); //역으로 돌림
        grid.path = path;
    }
    //휴리스틱 계산, 가로는 10, 세로는 14 => 1:1:루트2 삼각형 원리
    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }

        return 14 * dstX + 10 * (dstY - dstX);
    }
}
