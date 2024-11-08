using OpenCVForUnity.CoreModule;
using OpenCVForUnity.Features2dModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MarkerLessARExample
{
    /// <summary>
    /// Pattern capture.
    /// </summary>
    [RequireComponent(typeof(MultiSource2MatHelper))]
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
        /// The multi source to mat helper.
        /// </summary>
        MultiSource2MatHelper multiSource2MatHelper;

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

        /// <summary>
        /// The FPS monitor.
        /// </summary>
        FpsMonitor fpsMonitor;

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

            multiSource2MatHelper = gameObject.GetComponent<MultiSource2MatHelper>();
            multiSource2MatHelper.outputColorFormat = Source2MatHelperColorFormat.RGBA;
            multiSource2MatHelper.Initialize();

            detector = ORB.create();
            detector.setMaxFeatures(1000);
            keypoints = new MatOfKeyPoint();
        }

        /// <summary>
        /// Raises the source to mat helper initialized event.
        /// </summary>
        public void OnSourceToMatHelperInitialized()
        {
            Debug.Log("OnSourceToMatHelperInitialized");


            Mat rgbaMat = multiSource2MatHelper.GetMat();

            texture = new Texture2D(rgbaMat.width(), rgbaMat.height(), TextureFormat.RGB24, false);
            rgbMat = new Mat(rgbaMat.rows(), rgbaMat.cols(), CvType.CV_8UC3);
            outputMat = new Mat(rgbaMat.rows(), rgbaMat.cols(), CvType.CV_8UC3);

            // Set the Texture2D as the main texture of the Renderer component attached to the game object
            gameObject.GetComponent<Renderer>().material.mainTexture = texture;

            // Adjust the scale of the game object to match the dimensions of the texture
            gameObject.transform.localScale = new Vector3(rgbaMat.width(), rgbaMat.height(), 1);
            Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            // Adjust the orthographic size of the main Camera to fit the aspect ratio of the image
            float width = rgbaMat.width();
            float height = rgbaMat.height();
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


            // If the WebCam is front facing, flip the Mat horizontally. Required for successful detection.
            if (multiSource2MatHelper.source2MatHelper is WebCamTexture2MatHelper webCamHelper)
                webCamHelper.flipHorizontal = webCamHelper.IsFrontFacing();


            int patternWidth = (int)(Mathf.Min(rgbaMat.width(), rgbaMat.height()) * 0.8f);

            patternRect = new OpenCVForUnity.CoreModule.Rect(rgbaMat.width() / 2 - patternWidth / 2, rgbaMat.height() / 2 - patternWidth / 2, patternWidth, patternWidth);
        }

        /// <summary>
        /// Raises the source to mat helper disposed event.
        /// </summary>
        public void OnSourceToMatHelperDisposed()
        {
            Debug.Log("OnSourceToMatHelperDisposed");

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
        /// Raises the source to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        /// <param name="message">Message.</param>
        public void OnSourceToMatHelperErrorOccurred(Source2MatHelperErrorCode errorCode, string message)
        {
            Debug.Log("OnSourceToMatHelperErrorOccurred " + errorCode + ":" + message);

            if (fpsMonitor != null)
            {
                fpsMonitor.consoleText = "ErrorCode: " + errorCode + ":" + message;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (multiSource2MatHelper.IsPlaying() && multiSource2MatHelper.DidUpdateThisFrame())
            {
                Mat rgbaMat = multiSource2MatHelper.GetMat();

                Imgproc.cvtColor(rgbaMat, rgbMat, Imgproc.COLOR_RGBA2RGB);
                Imgproc.cvtColor(rgbaMat, outputMat, Imgproc.COLOR_RGBA2RGB);

                detector.detect(rgbMat, keypoints);
                //Debug.Log ("keypoints.ToString() " + keypoints.ToString());
                Features2d.drawKeypoints(rgbMat, keypoints, rgbMat, Scalar.all(-1));


                Imgproc.rectangle(rgbMat, patternRect.tl(), patternRect.br(), new Scalar(255, 0, 0, 255), 5);

                Utils.matToTexture2D(rgbMat, texture);
            }
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy()
        {
            multiSource2MatHelper.Dispose();

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
            multiSource2MatHelper.Play();
        }

        /// <summary>
        /// Raises the pause button click event.
        /// </summary>
        public void OnPauseButtonClick()
        {
            multiSource2MatHelper.Pause();
        }

        /// <summary>
        /// Raises the stop button click event.
        /// </summary>
        public void OnStopButtonClick()
        {
            multiSource2MatHelper.Stop();
        }

        /// <summary>
        /// Raises the change camera button click event.
        /// </summary>
        public void OnChangeCameraButtonClick()
        {
            multiSource2MatHelper.requestedIsFrontFacing = !multiSource2MatHelper.requestedIsFrontFacing;
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

                if (GraphicsSettings.defaultRenderPipeline == null)
                {
                    SceneManager.LoadScene("MultiSourceMarkerLessARExample_Built-in");
                }
                else
                {
                    SceneManager.LoadScene("MultiSourceMarkerLessARExample_SRP");
                }
            }
        }
    }
}
