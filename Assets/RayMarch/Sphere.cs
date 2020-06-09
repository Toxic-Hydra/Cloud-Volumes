using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sphere : MonoBehaviour
{
    
    
    [Range(0, 256)] public int noiseIterations;

    

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
    
    

    private void Start()
    {
        
    }
}
