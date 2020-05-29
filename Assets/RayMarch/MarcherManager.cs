using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Since you can't just click and drag Compute shaders you gotta shove them into a script and start them through there.
//It's why we define which function is the "Main" function within the compute script.
[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class MarcherManager : MonoBehaviour
{
    public ComputeShader rayMarcher;
    //Compute Shader tells us to do this.
    RenderTexture target;

    private Camera view;

    private Light light;

    private List<ComputeBuffer> buffers;

    private Texture2D Noise;

    void Setup()
    {
        view = Camera.current;
        light = FindObjectOfType<Light>();
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Setup();
        buffers = new List<ComputeBuffer>();

        InitRenderTexture();
        CreateScene();
        ParameterSetup();
        
        //Setting up the Raymarcher shader
        rayMarcher.SetTexture(0, "Origin", src);
        rayMarcher.SetTexture(0, "Result", target);

        //Gotta be honest i'm not sure why the groups are calculated like this.
        int threadGroupsX = Mathf.CeilToInt(view.pixelWidth / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(view.pixelHeight / 8.0f);
        rayMarcher.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        Graphics.Blit(target, dest);

        foreach (var buffer in buffers)
        {
            buffer.Dispose();
        }
    }

    void InitRenderTexture()
    {
        if (target == null || target.width != view.pixelWidth || target.height != view.pixelHeight)
        {
            if(target != null)
                target.Release();
            
            target = new RenderTexture(view.pixelWidth, view.pixelHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true; //this is the important part
            target.Create();
        }
    }

    struct SphereData
    {
        public Vector3 position;
        public Vector3 scale;

        public int operation;
        public float blending;
        public int children;

        public static int GetSize()
        {
            return sizeof(float) * 7 + sizeof(int) * 2;
        }
    }
    void CreateScene()
    {
        List<Sphere> allSpheres = new List<Sphere>(FindObjectsOfType<Sphere>());
        allSpheres.Sort((a,b) => a.operation.CompareTo(b.operation));
        
        List<Sphere> orderedSpheres = new List<Sphere>();

        for (int i = 0; i < allSpheres.Count; i++)
        {
            //top level spheres
            if (allSpheres[i].transform.parent == null)
            {
                Transform parentSphere = allSpheres[i].transform;
                orderedSpheres.Add(allSpheres[i]);
                allSpheres[i].children = parentSphere.childCount;
                for (int j = 0; j < parentSphere.childCount; j++)
                {
                    if (parentSphere.GetChild(j).GetComponent<Sphere>())
                    {
                        orderedSpheres.Add(parentSphere.GetChild(j).GetComponent<Sphere>());
                        orderedSpheres[orderedSpheres.Count - 1].children = 0;
                    }
                }
            }
        }
        
        SphereData[] sphereData = new SphereData[orderedSpheres.Count];
        for (int i = 0; i < orderedSpheres.Count; i++)
        {
            var sphere = orderedSpheres[i];
            sphereData[i] = new SphereData()
            {
                position = sphere.Position,
                scale = sphere.Scale,
                operation = (int) sphere.operation,
                blending = sphere.blending * 3,
                children = sphere.children
            };
        }

        Noise = orderedSpheres[0].Noise; //TODO: the 2d noise isn't working out. I'll probably remove it.
        if (Noise)
        {
            rayMarcher.SetInt("noiseIterations", orderedSpheres[0].noiseIterations);
            rayMarcher.SetTexture(0,"Noise",Noise);
        }
        
        int dataSize = sizeof(float) * 7 + sizeof(int) * 2;
        ComputeBuffer sphereBuffer = new ComputeBuffer(sphereData.Length, dataSize);
        sphereBuffer.SetData(sphereData);
        rayMarcher.SetBuffer(0, "scene", sphereBuffer);
        rayMarcher.SetInt("totalShapes", sphereData.Length);
        
        buffers.Add(sphereBuffer);
    }

    void ParameterSetup()
    {
        //ommitting light stuff
        rayMarcher.SetMatrix("WorldCamera", view.cameraToWorldMatrix);
        rayMarcher.SetMatrix("InverseCameraProj", view.projectionMatrix.inverse);
        
        //Set light parameters.
    }

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
