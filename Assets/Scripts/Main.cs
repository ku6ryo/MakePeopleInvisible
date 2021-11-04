using UnityEngine;
using UnityEngine.UI;
public class Main : MonoBehaviour
{
    [SerializeField]
    private RawImage finalImage;

    private RenderTexture resultTexture1;
    private RenderTexture resultTexture2;

    private RenderTexture finalResultTexture;

    private HumanSegmentationMaskGenerator segmentaionMaskGenerator;
    WebCamTexture cameraTexture;

    [SerializeField]
    ComputeShader backgroundShader;

    int index = 0;

    // Start is called before the first frame update
    void Start()
    {
        int resolutionX = 1920;
        int resolutionY = 1080;
        cameraTexture = new WebCamTexture("", resolutionX, resolutionY);
        cameraTexture.Play();
        segmentaionMaskGenerator = new HumanSegmentationMaskGenerator();

        resultTexture1 = new RenderTexture(resolutionX, resolutionY, 0, RenderTextureFormat.ARGBFloat);
        resultTexture1.enableRandomWrite = true;
        resultTexture1.Create();

        resultTexture2 = new RenderTexture(resolutionX, resolutionY, 0, RenderTextureFormat.ARGBFloat);
        resultTexture2.enableRandomWrite = true;
        resultTexture2.Create();

        finalResultTexture = new RenderTexture(resolutionX, resolutionY, 0, RenderTextureFormat.ARGBFloat);
        finalResultTexture.enableRandomWrite = true;
        finalResultTexture.Create();
    }

    // Update is called once per frame
    void Update()
    {
        if (!cameraTexture.didUpdateThisFrame) return;
        segmentaionMaskGenerator.ProcessImage(cameraTexture);

        var kernel = backgroundShader.FindKernel("CSMain");
        var prev = resultTexture1;
        var next = resultTexture2;
        if (index % 2 == 0) {
          prev = resultTexture2;
          next = resultTexture1;
        } else {
          prev = resultTexture1;
          next = resultTexture2;
        }
        backgroundShader.SetTexture(kernel, "Previous", prev);
        backgroundShader.SetTexture(kernel, "Current", cameraTexture);
        backgroundShader.SetTexture(kernel, "Segment", segmentaionMaskGenerator.texture);
        backgroundShader.SetTexture(kernel, "Result", next);
        backgroundShader.Dispatch(
            kernel,
            cameraTexture.width / 8,
            cameraTexture.height / 8,
            1
        );
        finalImage.texture = prev;
        index += 1;
    }
    void OnDestroy()
    {
        if (cameraTexture != null) Destroy(cameraTexture);
        if (finalResultTexture != null) Destroy(finalResultTexture);
        if (resultTexture1 != null) Destroy(resultTexture1);
        if (resultTexture2 != null) Destroy(resultTexture2);
    }
}