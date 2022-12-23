using OpenCVForUnity.CoreModule;
using OpenCVForUnity.Features2dModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MarkerLessARExample
{
    /// <summary>
    /// Pattern capture.
    /// </summary>
    [RequireComponent(typeof(WebCamTextureToMatHelper))]
    public class CapturePattern : MonoBehaviour
    {
        /// <summary>
        /// The pattern raw image.
        /// </summary>
        public RawImage patternRawImage;

        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

        /// <summary>
        /// The webcam texture to mat helper.
        /// </summary>
        WebCamTextureToMatHelper webCamTextureToMatHelper;

        /// <summary>
        /// The pattern rect.
        /// </summary>
        OpenCVForUnity.CoreModule.Rect patternRect;

        /// <summary>
        /// The rgb mat.
        /// </summary>
        Mat rgbMat;

        /// <summary>
        /// The output mat.
        /// </summary>
        Mat outputMat;

        /// <summary>
        /// The detector.
        /// </summary>
        ORB detector;

        /// <summary>
        /// The keypoints.
        /// </summary>
        MatOfKeyPoint keypoints;

        // Use this for initialization
        void Start()
        {
            //Utils.setDebugMode(true);

            using (Mat patternMat = Imgcodecs.imread(Application.persistentDataPath + "/patternImg.jpg"))
            {
                if (patternMat.total() == 0)
                {
                    patternRawImage.gameObject.SetActive(false);
                }
                else
                {
                    Imgproc.cvtColor(patternMat, patternMat, Imgproc.COLOR_BGR2RGB);

                    Texture2D patternTexture = new Texture2D(patternMat.width(), patternMat.height(), TextureFormat.RGBA32, false);

                    Utils.matToTexture2D(patternMat, patternTexture);

                    patternRawImage.texture = patternTexture;
                    patternRawImage.rectTransform.localScale = new Vector3(1.0f, (float)patternMat.height() / (float)patternMat.width(), 1.0f);

                    patternRawImage.gameObject.SetActive(true);
                }
            }

            webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper>();

            webCamTextureToMatHelper.Initialize();

            detector = ORB.create();
            detector.setMaxFeatures(1000);
            keypoints = new MatOfKeyPoint();
        }

        /// <summary>
        /// Raises the web cam texture to mat helper initialized event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInitialized()
        {
            Debug.Log("OnWebCamTextureToMatHelperInitialized");


            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat();

            texture = new Texture2D(webCamTextureMat.width(), webCamTextureMat.height(), TextureFormat.RGB24, false);
            rgbMat = new Mat(webCamTextureMat.rows(), webCamTextureMat.cols(), CvType.CV_8UC3);
            outputMat = new Mat(webCamTextureMat.rows(), webCamTextureMat.cols(), CvType.CV_8UC3);

            gameObject.transform.localScale = new Vector3(webCamTextureMat.width(), webCamTextureMat.height(), 1);

            Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);


            float width = webCamTextureMat.width();
            float height = webCamTextureMat.height();

            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale)
            {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            }
            else
            {
                Camera.main.orthographicSize = height / 2;
            }

            gameObject.GetComponent<Renderer>().material.mainTexture = texture;


            //if WebCamera is frontFaceing,flip Mat.
            if (webCamTextureToMatHelper.GetWebCamDevice().isFrontFacing)
            {
                webCamTextureToMatHelper.flipHorizontal = true;
            }


            int patternWidth = (int)(Mathf.Min(webCamTextureMat.width(), webCamTextureMat.height()) * 0.8f);

            patternRect = new OpenCVForUnity.CoreModule.Rect(webCamTextureMat.width() / 2 - patternWidth / 2, webCamTextureMat.height() / 2 - patternWidth / 2, patternWidth, patternWidth);
        }

        /// <summary>
        /// Raises the web cam texture to mat helper disposed event.
        /// </summary>
        public void OnWebCamTextureToMatHelperDisposed()
        {
            Debug.Log("OnWebCamTextureToMatHelperDisposed");

            if (rgbMat != null)
            {
                rgbMat.Dispose();
            }
            if (outputMat != null)
            {
                outputMat.Dispose();
            }
        }

        /// <summary>
        /// Raises the web cam texture to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        public void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode)
        {
            Debug.Log("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
        }

        // Update is called once per frame
        void Update()
        {
            if (webCamTextureToMatHelper.IsPlaying() && webCamTextureToMatHelper.DidUpdateThisFrame())
            {
                Mat rgbaMat = webCamTextureToMatHelper.GetMat();

                Imgproc.cvtColor(rgbaMat, rgbMat, Imgproc.COLOR_RGBA2RGB);
                Imgproc.cvtColor(rgbaMat, outputMat, Imgproc.COLOR_RGBA2RGB);

                detector.detect(rgbMat, keypoints);
                //Debug.Log ("keypoints.ToString() " + keypoints.ToString());
                Features2d.drawKeypoints(rgbMat, keypoints, rgbMat, Scalar.all(-1));


                Imgproc.rectangle(rgbMat, patternRect.tl(), patternRect.br(), new Scalar(255, 0, 0, 255), 5);

                Utils.fastMatToTexture2D(rgbMat, texture);
            }
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy()
        {
            webCamTextureToMatHelper.Dispose();

            detector.Dispose();
            if (keypoints != null)
                keypoints.Dispose();

            //Utils.setDebugMode(false);
        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick()
        {
            SceneManager.LoadScene("MarkerLessARExample");
        }

        /// <summary>
        /// Raises the play button click event.
        /// </summary>
        public void OnPlayButtonClick()
        {
            webCamTextureToMatHelper.Play();
        }

        /// <summary>
        /// Raises the pause button click event.
        /// </summary>
        public void OnPauseButtonClick()
        {
            webCamTextureToMatHelper.Pause();
        }

        /// <summary>
        /// Raises the stop button click event.
        /// </summary>
        public void OnStopButtonClick()
        {
            webCamTextureToMatHelper.Stop();
        }

        /// <summary>
        /// Raises the change camera button click event.
        /// </summary>
        public void OnChangeCameraButtonClick()
        {
            webCamTextureToMatHelper.requestedIsFrontFacing = !webCamTextureToMatHelper.IsFrontFacing();
        }

        /// <summary>
        /// Raises the capture button click event.
        /// </summary>
        public void OnCaptureButtonClick()
        {
            Mat patternMat = new Mat(outputMat, patternRect);

            detector.detect(patternMat, keypoints);
            if (keypoints.total() == 0)
            {
                Debug.LogWarning("Input image could not be used as pattern image due to missing keypoints.");
                return;
            }

            Texture2D patternTexture = new Texture2D(patternMat.width(), patternMat.height(), TextureFormat.RGBA32, false);

            Utils.matToTexture2D(patternMat, patternTexture);

            patternRawImage.texture = patternTexture;

            patternRawImage.gameObject.SetActive(true);
        }

        /// <summary>
        /// Raises the save button click event.
        /// </summary>
        public void OnSaveButtonClick()
        {
            if (patternRawImage.texture != null)
            {
                Texture2D patternTexture = (Texture2D)patternRawImage.texture;
                Mat patternMat = new Mat(patternRect.size(), CvType.CV_8UC3);
                Utils.texture2DToMat(patternTexture, patternMat);
                Imgproc.cvtColor(patternMat, patternMat, Imgproc.COLOR_RGB2BGR);

                string savePath = Application.persistentDataPath;
                Debug.Log("savePath " + savePath);

                Imgcodecs.imwrite(savePath + "/patternImg.jpg", patternMat);

                SceneManager.LoadScene("WebCamTextureMarkerLessARExample");
            }
        }
    }
}
