using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Node : IComparable<Node>
{
    public Vector3 position; //grid 위치
    public int gridX;
    public int gridY;

    public int hCost; //휴리스틱 비용
    public int gCost; //출발 지점으로부터 현재 거리까지의 비용
    public Node parent; //부모의 레퍼런스
    public int CompareTo(Node other)
    {
        if(this.fCost>other.fCost)
        {
            return -1;
        }
        else if(this.fCost<other.fCost)
        {
            return 1;
        }    
        else
        {
            return 0;
        }
        
    }

    public int fCost //전체 비용
    {
        get
        {
            return gCost + hCost;
        }
    }
}
