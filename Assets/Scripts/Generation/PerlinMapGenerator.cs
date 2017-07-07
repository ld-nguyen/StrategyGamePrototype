using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinMapGenerator
{
    public const int MAX_OFFSET_VALUE = 100000;

    public static float[] GeneratePerlinMap(int width, int height,PerlinMapParameters parameters)
    {
        float[] perlinValues = new float[width * height];
        int offset = Random.Range(0, MAX_OFFSET_VALUE);
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;


        for (int i = 0; i < perlinValues.Length; i++)
        {
            int x = i % width;
            int y = Mathf.FloorToInt(i / width);

            float sampleValue = 0;
            float frequency = 1;
            float amplitude = 1;

            for (int octaveStep = 0; octaveStep < parameters.octaves; octaveStep++)
            {
                sampleValue += GetNoiseSample(x + offset, y + offset, frequency, amplitude, parameters.scale);
                amplitude *= parameters.persistence;
                frequency *= parameters.lacunarity;
            }

            //Later used to shift Values back to 0-1
            if (sampleValue < minValue) minValue = sampleValue;
            else if (sampleValue > maxValue) maxValue = sampleValue;

            perlinValues[i] = sampleValue;
        }

        //Stretching Values back between 0 & 1
        for (int i = 0; i < perlinValues.Length; i++)
        {
            perlinValues[i] = Mathf.InverseLerp(minValue, maxValue, perlinValues[i]);
        }

        return perlinValues;
    }


    private static float GetNoiseSample(int x, int y, float frequency, float amplitude, float scale)
    {
        float xSample = x / scale * frequency;
        float ySample = y / scale * frequency;
        float value = Mathf.PerlinNoise(xSample, ySample) * 2 - 1; //Shift possible values to be able to get negative values
        return amplitude * value;
    }
}
