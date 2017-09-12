using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinDebug : MonoBehaviour {
    public PerlinMapParameters param;
    public MapDimensions dim;

    private void Start()
    {
        //ShowAmplitudes();
        ShowPerlinTextures();
    }

    public void ShowPerlinTextures()
    {
        if(gameObject.transform.childCount > 0)
        {
            foreach(Transform t in gameObject.transform)
            {
                Destroy(t.gameObject);
            }
        }

        for(int i = 0; i < param.octaves; i++)
        {
            if(i <= 0)
            {
                float[] values = PerlinMapGenerator.GeneratePerlinMap(dim.width, dim.height, param);
                ShowPerlinOnTexture(values);
            }
            else
            {
                PerlinMapParameters newParam = new PerlinMapParameters(param.baseFrequency * i+1, param.lacunarity, param.persistence);
                float[] values = PerlinMapGenerator.GeneratePerlinMap(dim.width, dim.height, newParam);
                ShowPerlinOnTexture(values,i+1);
            }
        }
    }

    public void ShowAmplitudes()
    {
        ShowPerlinOnTexture(PerlinMapGenerator.DebugAmplitudes(dim.width, dim.height));
    }

    private void ShowPerlinOnTexture(float[] array, int octaveNumber = 1) //Debug Method
    {
        int width = dim.width;
        int height = dim.height;

        Texture2D elevation = new Texture2D(dim.width, dim.height);
        Color[] colorMapElevation = new Color[dim.width * dim.height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int coordiante = y * width + x;
                colorMapElevation[coordiante] = Color.Lerp(Color.black, Color.white, array[coordiante]);
            }
        }
        elevation.SetPixels(colorMapElevation);
        elevation.Apply();

        GameObject debugPlane = Instantiate(Resources.Load("DebugPlane") as GameObject,gameObject.transform);
        debugPlane.name = "PerlinOctave" + octaveNumber;
        debugPlane.GetComponent<Renderer>().material.mainTexture = elevation;
    }
}
