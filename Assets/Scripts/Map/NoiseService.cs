using UnityEngine;
using Unity.Mathematics;

public static class NoiseService
{
    public static float GetNoiseValue(int x, int y, Vector2 offset, float noiseScale, int noiseOctaves)
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

    public static float GetFalloffValue(int x, int y, Vector2 mapSize, float falloffStrength, float islandSizeMultiplier)
    {
        float xv = x / (float)mapSize.x * 2 - 1;
        float yv = y / (float)mapSize.y * 2 - 1;
        float v = Mathf.Max(Mathf.Abs(xv), Mathf.Abs(yv));
        
        float a = falloffStrength;
        float b = islandSizeMultiplier; 
        return Mathf.Pow(v, a) / (Mathf.Pow(v, a) + Mathf.Pow(b - b * v, a));
    }
}