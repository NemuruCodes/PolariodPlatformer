using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] LevelGenConfig cfg;
    [SerializeField] Transform sectionRoot;
    [SerializeField] Material yarnMaterial;
    [SerializeField] GameObject pinPrefab;

    //McGuffin spawn
    public GameObject macGuffinPrefab;
    public Vector3 MacGuffinSpawnPoint { get; private set; }
    int macGuffinGridX;
    int macGuffinGridY;

    int currentX = 0;
    int currentY = 0;
    public bool isGenerating = false;

    public Vector3 CurrentCheckpoint { get; private set; }

    int sectionIndex = 0;

    private void Start()
    {
        macGuffinGridX = Random.value > 0.5f ? -1 : 1;
        macGuffinGridY = 2;
        GenerateGrid();
        Instantiate(macGuffinPrefab, MacGuffinSpawnPoint, Quaternion.identity);
    }

    void GenerateGrid()
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 2; y++)
            {
                GenerateSectionAt(x, y);
            }
        }
    }

    public Transform playerTransform; 
    public void GenerateSectionAt(int gridX, int gridY)
    {
        var w = cfg.SectionWidth;
        var h = cfg.SectionHeight;

        float originX = gridX * w;
        float originY = gridY * h;

        List<Vector2> pins = SamplePins(w, h, cfg.DensityAt(Mathf.Abs(gridX) + Mathf.Abs(gridY)));
        List<Edge> edges = EnsureNoFullWalls(BuildEdges(pins, w, h), pins, w, h);

        var root = new GameObject($"Section_{gridX}_{gridY}").transform;
        root.SetParent(sectionRoot, false);

        root.position = new Vector3(originX, originY, 0);

        HashSet<int> usedPins = new HashSet<int>();
        foreach (var e in edges) { usedPins.Add(e.a); usedPins.Add(e.b); }

        List<Vector2> prunedPins = new List<Vector2>();
        foreach (int oldIndex in usedPins) prunedPins.Add(pins[oldIndex]);

        foreach (var p in prunedPins) SpawnPin(root, p);

        foreach (var e in edges)
        {
            SpawnYarn(root, pins[e.a], pins[e.b]);
        }

        //player spawn bridge
        if (gridX == 0 && gridY == -1)
        {
            Vector2 spawnPinA = new Vector2(w * 0.5f - 0.5f, h * 0.5f);
            Vector2 spawnPinB = new Vector2(w * 0.5f + 0.5f, h * 0.5f);

            SpawnPin(root, spawnPinA);
            SpawnPin(root, spawnPinB);
            SpawnYarn(root, spawnPinA, spawnPinB);
        }

        //McGuffin spawn bridge
        if (gridX == macGuffinGridX && gridY == macGuffinGridY)
        {
            Vector2 magPinA = new Vector2(w * 0.5f - 0.5f, h * 0.5f);
            Vector2 magPinB = new Vector2(w * 0.5f + 0.5f, h * 0.5f);

            SpawnPin(root, magPinA);
            SpawnPin(root, magPinB);
            SpawnYarn(root, magPinA, magPinB);

            MacGuffinSpawnPoint = root.position + new Vector3(w * 0.5f, h * 0.5f, 0f);
        }

        if (gridX == 0 && gridY == -1)
        {
            CurrentCheckpoint = new Vector3(0f, -2.25f, 0f);
        }
    }
    
    public void GenerateNextSection()
    {
        if (sectionIndex > 0)
        {
            float prevX = -cfg.SectionWidth * 0.5f + ((sectionIndex - 1) * cfg.SectionWidth);
            CurrentCheckpoint = new Vector3(prevX, 2.25f, 0f);
        }
        else
        {
            CurrentCheckpoint = GetSpawnPoint();
        }

        var w = cfg.SectionWidth;
        var h = cfg.SectionHeight;

        var originX = (sectionIndex * w) - (w * 0.5f);

        List<Vector2> pins = SamplePins(w, h, cfg.DensityAt(sectionIndex));
        List<Edge> edges = EnsureNoFullWalls(BuildEdges(pins, w, h), pins, w, h);

        var root = new GameObject($"Section_{sectionIndex}").transform;
        root.SetParent(sectionRoot, false);
        root.position = new Vector3(originX, 0, 0);

        // Prune dead pins
        HashSet<int> usedPins = new HashSet<int>();
        foreach (var e in edges) { usedPins.Add(e.a); usedPins.Add(e.b); }

        List<Vector2> prunedPins = new List<Vector2>();
        foreach (int oldIndex in usedPins) prunedPins.Add(pins[oldIndex]);

        foreach (var p in prunedPins) SpawnPin(root, p);

        foreach (var e in edges)
        {
            SpawnYarn(root, pins[e.a], pins[e.b]);
        }

        //spawn bridge pins
        Vector2 spawnPinA = new Vector2(0.5f, 2f);
        Vector2 spawnPinB = new Vector2(1.5f, 2f);

        SpawnPin(root, spawnPinA);
        SpawnPin(root, spawnPinB);
        SpawnYarn(root, spawnPinA, spawnPinB);

        Debug.Log($"[LevelGen] Section {sectionIndex}: {pins.Count} pins, {edges.Count} edges. Spawn is at: {GetSpawnPoint()}");
        sectionIndex++;
    }

    List<Vector2> SamplePins(float w, float h, float density)
    {
        var pins = new List<Vector2>();
        float cell = 1f / Mathf.Sqrt(Mathf.Max(0.01f, density));
        float jitter = cell * 0.35f;
        float margin = cfg.edgeMargin;
        float top = h - margin;
        float bottom = margin;

        for (float x = margin; x <= w - margin; x += cell)
            for (float y = bottom; y <= top; y += cell)
            {
                if (x > w * 0.5f - 1.0f && x < w * 0.5f + 1.0f && y > h * 0.5f - 1.0f && y < h * 0.5f + 1.0f) continue;
                var p = new Vector2(
                    x + Random.Range(-jitter, jitter),
                    y + Random.Range(-jitter, jitter));
                p.x = Mathf.Clamp(p.x, margin, w - margin);
                p.y = Mathf.Clamp(p.y, bottom, top);
                pins.Add(p);
            }
        return pins;
    }

    List<Edge> EnsureNoFullWalls(List<Edge> edges, List<Vector2> pins, float w, float h)
    {
        var safeEdges = new List<Edge>();
        foreach (var e in edges)
        {
            Vector2 a = pins[e.a];
            Vector2 b = pins[e.b];
            if ((a.y < 1.0f && b.y > h - 1.0f) || (a.y > h - 1.0f && b.y < 1.0f)) continue;
            safeEdges.Add(e);
        }
        return safeEdges;
    }

    List<Edge> BuildEdges(List<Vector2> pins, float w, float h)
    {
        var candidate = KnnEdges.Build(pins, k: cfg.kNeighbors, maxEdgeLength: 4.0f);
        var edges = new List<Edge>();

        foreach (var e in candidate)
        {
            Vector2 a = pins[e.a];
            Vector2 b = pins[e.b];

            if (Mathf.Abs(a.x - b.x) < 0.5f && Mathf.Abs(a.y - b.y) > 4.0f) continue;

            edges.Add(e);
        }
        return edges;
    }

    void SpawnPin(Transform parent, Vector2 local)
    {
        var go = Instantiate(pinPrefab, parent);
        go.transform.localPosition = local;
    }

    void SpawnYarn(Transform parent, Vector2 a, Vector2 b)
    {
        var go = new GameObject("Yarn");
        go.transform.SetParent(parent, false);
        go.layer = LayerMask.NameToLayer("Ground");

        Vector2 mid = (a + b) * 0.5f;
        go.transform.localPosition = mid;

        float angle = Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
        go.transform.localRotation = Quaternion.Euler(0, 0, angle);

        var lr = go.AddComponent<LineRenderer>();
        lr.material = yarnMaterial;
        lr.startWidth = cfg.yarnThickness;
        lr.endWidth = cfg.yarnThickness;
        lr.useWorldSpace = false;
        lr.positionCount = 10;

        float length = Vector2.Distance(a, b);
        for (int i = 0; i < 10; i++)
        {
            float t = i / 9f;
            float x = Mathf.Lerp(-length * 0.5f, length * 0.5f, t);
            float y = -cfg.yarnSag * Mathf.Sin(t * Mathf.PI);
            lr.SetPosition(i, new Vector3(x, y, 0));
        }

        var cap = go.AddComponent<CapsuleCollider2D>();
        cap.direction = CapsuleDirection2D.Horizontal;
        cap.size = new Vector2(length, cfg.yarnThickness);
    }

    public struct Edge { public int a, b; public Edge(int a, int b) { this.a = a; this.b = b; } }

    public Vector3 GetSpawnPoint()
    {
        return new Vector3(0f, -2.25f, 0f);
    }
}