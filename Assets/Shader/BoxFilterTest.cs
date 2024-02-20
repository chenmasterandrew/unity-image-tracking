using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

//[RequireComponent(typeof(ARCameraBackground))]
public class BoxFilterTest : MonoBehaviour
{
    [SerializeField] private int boxFilterKernel = 5;
    [SerializeField] private bool useSecondaryCamera = false;
    [SerializeField] private ComputeShader rgba2Gray;
    [SerializeField] private ComputeShader boxFilter;
    [SerializeField] private ComputeShader gray2Rgba;
    [SerializeField] private Camera secondaryCamera;
    [SerializeField] private Texture2D markerImg;

    private Camera primaryCamera;
    private ARCameraBackground arCameraBackground;
    private RenderTexture rgbaMarkerRT;
    private RenderTexture grayMarkerRT;
    private RenderTexture rgbaRT;
    private RenderTexture grayRT;
    private RenderTexture grayBoxRT;
    private RenderTexture rgbaBoxRT;
    private int width;
    private int height;
    private int depth;
    private int scale = 2;
    // Start is called before the first frame update
    void Start()
    {
        primaryCamera = GetComponent<Camera>();
        arCameraBackground = GetComponent<ARCameraBackground>();
        width = primaryCamera.pixelWidth/ scale;
        height = primaryCamera.pixelHeight/ scale;
        depth = 1;


        if (useSecondaryCamera)
        {
            width = secondaryCamera.pixelWidth/ scale;
            height = secondaryCamera.pixelHeight/ scale;
        }

        rgbaRT = new RenderTexture(width, height, depth);
        rgbaRT.enableRandomWrite = true;
        rgbaRT.Create();

        grayRT = new RenderTexture(width, height, depth);
        grayRT.enableRandomWrite = true;
        grayRT.Create();

        grayBoxRT = new RenderTexture(width, height, depth);
        grayBoxRT.enableRandomWrite = true;
        grayBoxRT.Create();

        rgbaBoxRT = new RenderTexture(width, height, depth);
        rgbaBoxRT.enableRandomWrite = true;
        rgbaBoxRT.Create();


        if (useSecondaryCamera)
        {
            secondaryCamera.targetTexture = rgbaRT;
        }

        int markerWidth = markerImg.width;
        int markerHeight = markerImg.height;

        rgbaMarkerRT = new RenderTexture(markerWidth, markerHeight, depth);
        rgbaMarkerRT.enableRandomWrite = true;
        rgbaMarkerRT.Create();

        grayMarkerRT = new RenderTexture(markerWidth, markerHeight, depth);
        grayMarkerRT.enableRandomWrite = true;
        grayMarkerRT.Create();

        RenderTexture.active = rgbaMarkerRT;
        Graphics.Blit(markerImg, rgbaMarkerRT);

        rgba2Gray.SetTexture(0, "Input", rgbaMarkerRT);
        rgba2Gray.SetTexture(0, "Result", grayMarkerRT);
        rgba2Gray.Dispatch(0, markerWidth, markerHeight, 1);


    }

    // Update is called once per frame
    void Update()
    {
        if (arCameraBackground.material != null)
        {
            Graphics.Blit(null, rgbaRT, arCameraBackground.material);
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        rgba2Gray.SetTexture(0, "Input", rgbaRT);
        rgba2Gray.SetTexture(0, "Result", grayRT);
        rgba2Gray.Dispatch(0, width, height, 1);
        
        boxFilter.SetTexture(0, "Input", grayRT);
        boxFilter.SetTexture(0, "Result", grayBoxRT);
        boxFilter.SetInt("Width", width);
        boxFilter.SetInt("Height", height);
        boxFilter.SetInt("K", boxFilterKernel);
        boxFilter.Dispatch(0, width, height, 1);
        
        gray2Rgba.SetTexture(0, "Input", grayBoxRT);
        gray2Rgba.SetTexture(0, "Result", rgbaBoxRT);
        gray2Rgba.Dispatch(0, width, height, 1);

        Graphics.Blit(rgbaBoxRT, destination);
        // Graphics.Blit(grayMarkerRT, destination);
    }

    public void IncrementBoxFilterKernel(int n)
    {
        boxFilterKernel += n;
    }
    public void DecrementBoxFilterKernel(int n)
    {
        boxFilterKernel = Mathf.Max(boxFilterKernel - n, 0);
    }
}
