using System.Collections.Generic;
using UnityEngine;

public static class KnnEdges
{
    public static List<LevelGenerator.Edge> Build(
        List<Vector2> pins, int k, float maxEdgeLength)
    {
        var edges = new List<LevelGenerator.Edge>();
        var seen = new HashSet<long>();
        int n = pins.Count;
        float maxSq = maxEdgeLength * maxEdgeLength;

        for (int i = 0; i < n; i++)
        {
            var nearest = new List<(int j, float d2)>(n - 1);
            for (int j = 0; j < n; j++)
            {
                if (j == i) continue;
                float d2 = (pins[i] - pins[j]).sqrMagnitude;
                nearest.Add((j, d2));
            }

            nearest.Sort((a, b) => a.d2.CompareTo(b.d2));

            int take = Mathf.Min(k, nearest.Count);
            for (int m = 0; m < take; m++)
            {
                int j = nearest[m].j;


                if (nearest[m].d2 > maxSq) continue;

                long key = Key(i, j);
                if (seen.Add(key))
                    edges.Add(new LevelGenerator.Edge(i, j));
            }
        }
        return edges;
    }

    static long Key(int a, int b)
    {
        if (a > b) (a, b) = (b, a);
        return ((long)a << 32) | (uint)b;
    }
}