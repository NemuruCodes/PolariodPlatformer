using System.Collections.Generic;

public class MinHeap<T>
{
    readonly List<(T item, int priority)> _items = new();

    public int Count => _items.Count;

    public void Enqueue(T item, int priority)
    {
        _items.Add((item, priority));
        SiftUp(_items.Count - 1);
    }

    public T Dequeue()
    {
        var root = _items[0].item;
        int last = _items.Count - 1;
        _items[0] = _items[last];
        _items.RemoveAt(last);
        if (_items.Count > 0) SiftDown(0);
        return root;
    }

    void SiftUp(int i)
    {
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (_items[parent].priority <= _items[i].priority) break;
            (_items[parent], _items[i]) = (_items[i], _items[parent]);
            i = parent;
        }
    }

    void SiftDown(int i)
    {
        int n = _items.Count;
        while (true)
        {
            int l = i * 2 + 1, r = l + 1, smallest = i;
            if (l < n && _items[l].priority < _items[smallest].priority) smallest = l;
            if (r < n && _items[r].priority < _items[smallest].priority) smallest = r;
            if (smallest == i) break;
            (_items[smallest], _items[i]) = (_items[i], _items[smallest]);
            i = smallest;
        }
    }
}