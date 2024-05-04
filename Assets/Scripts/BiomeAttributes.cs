using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Biome Attributes", menuName = "MinecraftTutorial/Biome Attribute")]
public class BiomeAttributes : ScriptableObject
{
    //its not the best way to do it/ just ilustrate the idea 
    [Header("Biome things")]
    public string biomeName;
    public int offset;
    public float scale;

    public int terrainH;
    public float terrainScale;

    public byte surfaceBlock;
    public byte subSurfaceBlock;

    [Header("Major Flora")]
    public int majorFloraIndex;
    public float majorFloraZoneScale = 1.3f;
    [Range(0.1f, 1f)]
    public float majorFloraZoneThreshold = 0.6f;
    public float majorFloraPlacementScale = 15f;
    [Range(0.1f, 1f)]
    public float majorFloraPlacementThreshold = 0.8f;
    public bool PlaceMajorFlora = true;

    public int maxHeight = 12;
    public int minHeight = 5;

    public Lode[] lodes;
}

[System.Serializable]
public class Lode //lode - ¿y³a
{
    public string nodeName;
    public byte blockID;
    public int minHeight;
    public int maxHeight;
    public float scale;
    public float threshold;
    public float noiseOffset;
}