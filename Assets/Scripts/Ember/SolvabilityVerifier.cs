using System.Collections.Generic;
using UnityEngine;

public static class SolvabilityVerifier
{
    public static int MinPhotosToCross(
        List<LevelGenerator.Edge> edges,
        List<Vector2> pins,
        float w, float h,
        LevelGenConfig cfg,
        float spawnX)
    {
        float cell = cfg.playerWidth * 0.25f;
        int nx = Mathf.CeilToInt(w / cell);
        int ny = Mathf.CeilToInt(h / cell);
        bool[,] solid = new bool[nx, ny];

        foreach (var e in edges)
            RasterizeEdge(pins[e.a], pins[e.b], cfg.yarnThickness, solid, cell, nx, ny);

        int[,] cost = new int[nx, ny];
        for (int i = 0; i < nx; i++)
            for (int j = 0; j < ny; j++)
                cost[i, j] = int.MaxValue;

        int sx = Mathf.Clamp(Mathf.RoundToInt(spawnX / cell), 0, nx - 1);
        int sy = FirstFreeAboveFloor(solid, sx, ny);
        if (sy < 0) return int.MaxValue;
        cost[sx, sy] = 0;

        var q = new MinHeap<(int x, int y)>();
        q.Enqueue((sx, sy), 0);

        int jumpY = Mathf.RoundToInt(cfg.jumpHeight / cell);
        int jumpX = Mathf.RoundToInt(cfg.jumpForwardReach / cell);

        while (q.Count > 0)
        {
            var node = q.Dequeue();
            int x = node.x, y = node.y;
            int c = cost[x, y];

            // ballistic reach
            for (int dx = -jumpX; dx <= jumpX; dx++)
                for (int dy = 0; dy <= jumpY; dy++)
                {
                    int tx = x + dx, ty = y + dy;
                    if (!InBounds(tx, ty, nx, ny)) continue;
                    if (ArcBlocked(solid, x, y, tx, ty)) continue;
                    if (!CanStand(solid, tx, ty, nx, ny)) continue;
                    if (cost[tx, ty] > c)
                    {
                        cost[tx, ty] = c;
                        q.Enqueue((tx, ty), c);
                    }
                }

            // photo bridge
            int photoLen = Mathf.RoundToInt(cfg.photoMaxLength / cell);
            int photoY = y - 1;
            for (int px = x - 2; px <= x + 2; px++)
            {
                if (!InBounds(px, photoY, nx, ny)) continue;
                if (PhotoBlocked(solid, px, photoY, photoLen, nx, ny)) continue;
                int top = photoY + 1;
                for (int ix = px; ix < px + photoLen; ix++)
                {
                    if (!InBounds(ix, top, nx, ny)) continue;
                    if (cost[ix, top] > c + 1)
                    {
                        cost[ix, top] = c + 1;
                        q.Enqueue((ix, top), c + 1);
                    }
                }
            }
        }

        int best = int.MaxValue;
        int edgeX = nx - 1;
        for (int y = 0; y < ny; y++)
            best = Mathf.Min(best, cost[edgeX, y]);
        return best;
    }

    static void RasterizeEdge(Vector2 a, Vector2 b, float thickness,
        bool[,] solid, float cell, int nx, int ny)
    {
        int steps = Mathf.CeilToInt(Vector2.Distance(a, b) / cell);
        int half = Mathf.Max(1, Mathf.CeilToInt(thickness * 0.5f / cell));
        for (int s = 0; s <= steps; s++)
        {
            float t = s / (float)steps;
            Vector2 p = Vector2.Lerp(a, b, t);
            int cx = Mathf.RoundToInt(p.x / cell);
            int cy = Mathf.RoundToInt(p.y / cell);
            for (int dx = -half; dx <= half; dx++)
                for (int dy = -half; dy <= half; dy++)
                {
                    int x = cx + dx, y = cy + dy;
                    if (InBounds(x, y, nx, ny)) solid[x, y] = true;
                }
        }
    }

    static int FirstFreeAboveFloor(bool[,] solid, int x, int ny)
    {
        for (int y = 0; y < ny; y++) if (!solid[x, y]) return y;
        return -1;
    }

    static bool CanStand(bool[,] solid, int x, int y, int nx, int ny)
        => InBounds(x, y, nx, ny) && !solid[x, y];

    static bool ArcBlocked(bool[,] solid, int x0, int y0, int x1, int y1)
    {
        int steps = Mathf.Max(4, Mathf.CeilToInt(Vector2.Distance(
            new Vector2(x0, y0), new Vector2(x1, y1))) * 2);
        for (int i = 1; i < steps; i++)
        {
            float t = i / (float)steps;
            int x = Mathf.RoundToInt(Mathf.Lerp(x0, x1, t));
            int y = Mathf.RoundToInt(Mathf.Lerp(y0, y1, t));
            if (x >= 0 && y >= 0 && x < solid.GetLength(0) && y < solid.GetLength(1)
                && solid[x, y]) return true;
        }
        return false;
    }

    static bool PhotoBlocked(bool[,] solid, int px, int py, int len, int nx, int ny)
    {
        for (int x = px; x < px + len; x++)
            if (x < 0 || x >= nx || py < 0 || py >= ny || solid[x, py]) return true;
        return false;
    }

    static bool InBounds(int x, int y, int nx, int ny) => x >= 0 && y >= 0 && x < nx && y < ny;
}