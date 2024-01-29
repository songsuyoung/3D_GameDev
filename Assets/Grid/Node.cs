using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Node : IComparable<Node> //정렬 기준 : Node 와 Node 사이
{
    public Node parent;
    public bool isWalkable;
    public int gridX;
    public int gridY;

    public int gridR;
    public int gridC;
    //grid내에서 x,y
    public Vector3 position; //plane에서의 위치
    public int fCost
    {
        get
        {
            return gCost + hCost; //시작 정점으로 부터의 위치 + 휴리스틱 알고리즘에 의해 결정된 비용
        }
    }

    public int hCost;
    public int gCost;

    public int CompareTo(Node other)
    {
        if (this.fCost < other.fCost) return 1;
        else if (this.fCost > other.fCost) return -1;
        else return 0;
    }

    public Node(bool isWalkable,Vector3 position,int gridX, int gridY)
    {
        this.isWalkable = isWalkable;
        this.position = position;
        this.gridX = gridX;
        this.gridY = gridY;
    }
}
