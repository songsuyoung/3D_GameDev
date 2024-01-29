using System;
using System.Collections;
using System.Collections.Generic;

public class PriorityQueue<T> where T : IComparable<T>
{
    List<T> nodes;

    public PriorityQueue()
    {
        nodes = new List<T>();
    }

    public void Add(T item)
    {
        nodes.Add(item);

        int child = nodes.Count - 1;

        while (child > 0)
        {
            int parent = (child - 1) % 2; //부모 인덱스를 가져옴

            if (nodes[child].CompareTo(nodes[parent]) < 1)
            {
                break;
            }

            T buf = nodes[child];
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

        while (true)
        {
            int left = (parent * 2) + 1;
            int right = (parent * 2) + 2;

            if (left >= nodes.Count) break;

            int child = parent;

            if (nodes[left].CompareTo(nodes[parent]) == 1)
            {
                child = left;
            }

            if (right < nodes.Count && nodes[left].CompareTo(nodes[right]) == -1)
            {
                child = right;
            }

            if (parent == child) break;

            T tmp = nodes[parent];
            nodes[parent] = nodes[child];
            nodes[child] = tmp;

            parent = child;
        }
    }
    public bool Contains(T node)
    {
        for (int i = 0; i < nodes.Count; i++)
            if (nodes[i].Equals(node))
                return true;

        return false;

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

    public T Top()
    {
        return nodes[0];
    }
}
