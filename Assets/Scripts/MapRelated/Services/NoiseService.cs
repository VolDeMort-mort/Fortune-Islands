using UnityEngine;
using Unity.Mathematics;
using System.CodeDom.Compiler;

public class NoiseService
{
    public void GenerateTerrain(WorldMap map, MapConfig mapConfig)
    {
        float seed = UnityEngine.Random.Range(0f, 10000f);
        Vector2 offset = new Vector2(seed, seed);

        for (int x = 0; x < map.mapSize.x; x++)
        {
            for (int y = 0; y < map.mapSize.y; y++)
            {
                // 1. Calculate Noise + Falloff
                float noiseVal = GetNoiseValue(x, y, offset, mapConfig.noiseScale, mapConfig.noiseOctaves);
                float falloff = GetFalloffValue(x, y, new Vector2(map.mapSize.x, map.mapSize.y), mapConfig.falloffStrength, mapConfig.islandSizeMultiplier);
                float finalValue = noiseVal - falloff;

                // 2. Determine Water/Ground
                if (finalValue > mapConfig.noiseThreshold)
                {
                    // Grid[x, y].Type = CellType.Ground;
                    map.GetCell(x, y).Type = CellType.Ground;
                }
                else
                {   
                    map.GetCell(x, y).Type = CellType.Water;
                    // Grid[x, y].Type = CellType.Water;
                }
            }
        }

    }

    public float GetNoiseValue(int x, int y, Vector2 offset, float noiseScale, int noiseOctaves)
    {
        float noiseVal = 0;
        float scale = noiseScale;
        float opacity = 1;
        float norm = 0;

        for (int i = 0; i < noiseOctaves; i++)
        {
            float xCoord = (x / scale) + offset.x;
            float yCoord = (y / scale) + offset.y;

            noiseVal += noise.snoise(new float2(xCoord, yCoord)) * opacity;
            norm += opacity;
            scale /= 2f;
            opacity *= 0.5f;
        }
        return Mathf.InverseLerp(-1, 1, noiseVal / norm);
    }

    public float GetFalloffValue(int x, int y, Vector2 mapSize, float falloffStrength, float islandSizeMultiplier)
    {
        float xv = x / (float)mapSize.x * 2 - 1;
        float yv = y / (float)mapSize.y * 2 - 1;
        float v = Mathf.Max(Mathf.Abs(xv), Mathf.Abs(yv));
        
        float a = falloffStrength;
        float b = islandSizeMultiplier; 
        return Mathf.Pow(v, a) / (Mathf.Pow(v, a) + Mathf.Pow(b - b * v, a));
    }
}