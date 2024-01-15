using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PriorityQueue : MonoBehaviour
{
    List<Node> nodes;

    PriorityQueue()
    {
        nodes = new List<Node>();
    }

    public void Add(Node item)
    {
        nodes.Add(item);

        int child = nodes.Count - 1;

        while(child > 0)
        {
            int parent = (child - 1) % 2; //부모 인덱스를 가져옴

            if (nodes[child].fCost >= nodes[parent].fCost)
            {
                break;
            }

            Node buf = nodes[child];
            nodes[child] = nodes[parent];
            nodes[parent] = buf;

            child = parent;
        }
    }

    public void Remove()
    {
        nodes[0] = nodes[nodes.Count - 1];
        nodes.RemoveAt(nodes.Count - 1); //마지막 노드를 삭제

        int parent = 0;

        while(true)
        {
            int left = (parent * 2) + 1;
            int right = (parent * 2) + 2;

            if (left >= nodes.Count) break;

            int child = parent;

            if (nodes[left].fCost < nodes[parent].fCost)
            {
                child = left;
            }

            if (right < nodes.Count && nodes[left].fCost > nodes[right].fCost)
            {
                child = right;
            }

            if (parent == child) break;

            Node tmp = nodes[parent];
            nodes[parent] = nodes[child];
            nodes[child] = tmp;

            parent = child;
        }
    }

    public bool isEmpty()
    {
        return nodes.Count == 0;    
    }

    public int Count
    {
        get
        {
            return nodes.Count;
        }
    }

    public Node Top()
    {
        return nodes[0];
    }

}
