using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sphere : MonoBehaviour
{
    
    //Noise Values
    public int noiseW = 256;
    public int noiseH = 256;
    public float cycles = 1.0f;
    [Range(0, 256)] public int noiseIterations;

    public Texture2D Noise;

    public enum Operation
    {
        None,
        Blend,
        Cut,
        Mask
    }
    public Operation operation;

    [Range(0, 1)] public float blending;

    [HideInInspector] public int children;

    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
    }

    public Vector3 Scale
    {
        get
        {
            Vector3 pScale = Vector3.one;
            if (transform.parent != null && transform.parent.GetComponent<Sphere>() != null)
            {
                pScale = transform.parent.GetComponent<Sphere>().Scale;
            }

            return Vector3.Scale(transform.localScale, pScale);
        }
    }
    
    //https://docs.unity3d.com/ScriptReference/Mathf.PerlinNoise.html
    void generateNoiseTexture()
    {
        float y = 0.0f;
        Color[] values = new Color[noiseW * noiseH];

        while (y < noiseH)
        {
            float x = 0.0f;
            while (x < noiseW)
            {
                float u = 0 + x / noiseW * cycles;
                float v = 0 + y / noiseH * cycles;
                float sample = Mathf.PerlinNoise(u, v);
                values[(int)y * noiseW + (int)x] = new Color(sample, sample, sample);
                x++; //Like a total goblin I forgot the incrementers which resulted in me being very angry when my unity editor crashed
            }

            y++;
        }
        
        Noise = new Texture2D(noiseW, noiseH);
        Noise.SetPixels(values);
        Noise.Apply();
    }

    private void Start()
    {
        generateNoiseTexture();
    }
}
