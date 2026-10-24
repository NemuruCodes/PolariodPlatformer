using UnityEngine;

[CreateAssetMenu(menuName = "Level Gen/Config", fileName = "LevelGenConfig")]
public class LevelGenConfig : ScriptableObject
{
    [Header("Player (meters)")]
    public float playerWidth = 0.4f;
    public float playerHeight = 1.0f;
    public float jumpHeight = 2.0f;   
    public float jumpForwardReach = 1.5f; 

    [Header("Section (in player-heights)")]
    public float sectionWidthInPlayerHeights = 9f;
    public float sectionHeightInPlayerHeights = 5f;

    [Header("Pins")]
    public float minPinDistance = 0.8f;   
    public float edgeMargin = 0.5f;  
    public float startDensity = 0.6f;  
    public float endDensity = 1.2f;
    public float densityRampSections = 12f; 

    [Header("Yarn edges")]
    [Range(0f, 1f)] public float edgeKeepProb = 0.75f;
    public float nearHorizontalThreshold = 20f;
    public float yarnThickness = 0.06f;  
    public float yarnSag = 0.08f;
    public int kNeighbors = 3;
    public float maxEdgeLength = 5f;

    [Header("Solvability")]
    public float minGapHeight = 2.4f;    // > jumpHeight * 1.2
    public int maxPhotosStart = 3;
    public int maxPhotosEnd = 6;
    public float photoMinLength = 0.8f;  // 2 * playerWidth
    public float photoMaxLength = 1.6f;  // 4 * playerWidth
    public float photoThickness = 0.05f;

    [Header("Generation retries")]
    public int maxEdgeRerolls = 30;

    [Header("Seam")]
    public float inheritedStripWidth = 1.5f;

    public float SectionWidth => sectionWidthInPlayerHeights * playerHeight;
    public float SectionHeight => sectionHeightInPlayerHeights * playerHeight;

    public float DensityAt(int sectionIndex)
    {
        float t = Mathf.Clamp01(sectionIndex / Mathf.Max(1f, densityRampSections));
        return Mathf.Lerp(startDensity, endDensity, t);
    }

    public int MaxPhotosAt(int sectionIndex)
    {
        float t = Mathf.Clamp01(sectionIndex / Mathf.Max(1f, densityRampSections));
        return Mathf.RoundToInt(Mathf.Lerp(maxPhotosStart, maxPhotosEnd, t));
    }
}