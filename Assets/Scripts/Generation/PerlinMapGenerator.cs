using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PerlinMapParameters
{
    public int octaves;
    [Range(0.1f, 10)]
    public float baseFrequency;
    [Range(1, 2)]
    public float lacunarity;
    [Range(0.1f, 1)]
    public float persistence;

    public PerlinMapParameters(float bF, float lac, float per)
    {
        octaves = 1;
        baseFrequency = bF * lac;
        lacunarity = lac;
        persistence = per;
    }
}

public class PerlinMapGenerator
{
    private static bool stretchValues;
    public const int MAX_OFFSET_VALUE = 100000;
    private static int mapWidth;
    private static int mapHeight;

    public static float[] GeneratePerlinMap(int width, int height, PerlinMapParameters parameters, bool stretchToZeroToOne = true)
    {
        mapWidth = width;
        mapHeight = height;
        stretchValues = stretchToZeroToOne;

        float[] perlinValues = new float[width * height];
        int offset = Random.Range(-MAX_OFFSET_VALUE, MAX_OFFSET_VALUE);
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;


        for (int i = 0; i < perlinValues.Length; i++)
        {
            int x = i % width;
            int y = i / width;

            float sampleValue = 0;
            float frequency = parameters.baseFrequency;
            float amplitude = 1;

            for (int octaveStep = 0; octaveStep < parameters.octaves; octaveStep++)
            {
                sampleValue += GetNoiseSample(x + offset, y + offset, frequency, amplitude);
                amplitude *= parameters.persistence;
                frequency *= parameters.lacunarity;
            }

            //Later used to shift Values back to 0-1
            if (stretchValues)
            {
                if (sampleValue < minValue) minValue = sampleValue;
                else if (sampleValue > maxValue) maxValue = sampleValue;
            }

            perlinValues[i] = sampleValue;
        }

        //Stretching Values back between 0 & 1 if needed
        if(stretchValues) perlinValues = Utility.StretchValuesToZeroAndOne(perlinValues, minValue, maxValue);

        return perlinValues;
    }

    private static float GetNoiseSample(int x, int y, float frequency, float amplitude)
    {
        float xSample = (float)x / mapWidth * frequency;
        float ySample = (float)y / mapHeight * frequency;
        float value = (Mathf.PerlinNoise(xSample, ySample) * 2) - 1 ;
        return amplitude * value;
    }
}
