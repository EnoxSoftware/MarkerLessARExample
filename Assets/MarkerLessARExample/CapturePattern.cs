using OpenCVForUnity.CoreModule;
using OpenCVForUnity.Extensions.SourceToMat;
using OpenCVForUnity.FeaturesModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityIntegration;
using OpenCVForUnity.UnityIntegration.Helper.SourceToMat;
using OpenCVForUnity.UnityIntegration.Helper.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MarkerLessARExample
{
    /// <summary>
    /// Pattern capture.
    /// </summary>
    [RequireComponent(typeof(MultiSourceToMatHelper))]
    public class CapturePattern : MonoBehaviour
    {
        /// <summary>
        /// The pattern raw image.
        /// </summary>
        public RawImage patternRawImage;

        /// <summary>
        /// The texture.
        /// </summary>
        private Texture2D texture;

        /// <summary>
        /// The pattern preview texture created at runtime.
        /// </summary>
        private Texture2D patternPreviewTexture;

        /// <summary>
        /// The multi source to mat helper.
        /// </summary>
        private MultiSourceToMatHelper multiSourceToMatHelper;

        /// <summary>
        /// The pattern rect.
        /// </summary>
        private OpenCVForUnity.CoreModule.Rect patternRect;

        /// <summary>
        /// The rgb mat.
        /// </summary>
        private Mat rgbMat;

        /// <summary>
        /// The output mat.
        /// </summary>
        private Mat outputMat;

        /// <summary>
        /// The detector.
        /// </summary>
        private ORB detector;

        /// <summary>
        /// The keypoints.
        /// </summary>
        private MatOfKeyPoint keypoints;

        /// <summary>
        /// The FPS monitor.
        /// </summary>
        private FpsMonitor fpsMonitor;

        // Use this for initialization
        private void Start()
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

                    patternPreviewTexture = new Texture2D(patternMat.width(), patternMat.height(), TextureFormat.RGBA32, false);

                    OpenCVMatUnityUtils.MatToTexture2D(patternMat, patternPreviewTexture);

                    patternRawImage.texture = patternPreviewTexture;
                    patternRawImage.rectTransform.localScale = new Vector3(1.0f, (float)patternMat.height() / (float)patternMat.width(), 1.0f);

                    patternRawImage.gameObject.SetActive(true);
                }
            }

            multiSourceToMatHelper = gameObject.GetComponent<MultiSourceToMatHelper>();
            multiSourceToMatHelper.OutputColorFormat = SourceToMatColorFormat.RGBA;
            multiSourceToMatHelper.Initialize();

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

            Mat rgbaMat = multiSourceToMatHelper.FrameMat;

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
            if (multiSourceToMatHelper.ActiveHelper is ICameraFacingToMatHelperControls cameraFacingControls)
            {
                multiSourceToMatHelper.ActiveHelper.FlipHorizontal = cameraFacingControls.IsFrontFacing;
            }

            int patternWidth = (int)(Mathf.Min(rgbaMat.width(), rgbaMat.height()) * 0.8f);

            patternRect = new OpenCVForUnity.CoreModule.Rect(rgbaMat.width() / 2 - patternWidth / 2, rgbaMat.height() / 2 - patternWidth / 2, patternWidth, patternWidth);

            if (!multiSourceToMatHelper.IsPlaying && !multiSourceToMatHelper.IsPaused)
            {
                multiSourceToMatHelper.Play();
            }
        }

        /// <summary>
        /// Raises the source to mat helper disposed event.
        /// </summary>
        public void OnSourceToMatHelperDisposed()
        {
            Debug.Log("OnSourceToMatHelperDisposed");
            DisposeFrameResources();
        }

        /// <summary>
        /// Raises the frame mat layout changed event.
        /// </summary>
        public void OnFrameMatLayoutChanged()
        {
            DisposeFrameResources();
            OnSourceToMatHelperInitialized();
        }

        private void DisposeFrameResources()
        {
            if (texture != null)
            {
                Destroy(texture);
                texture = null;
            }

            rgbMat?.Dispose();
            rgbMat = null;
            outputMat?.Dispose();
            outputMat = null;
        }

        /// <summary>
        /// Raises the source to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        /// <param name="message">Message.</param>
        public void OnSourceToMatHelperErrorOccurred(SourceToMatErrorCode errorCode, string message)
        {
            Debug.Log("OnSourceToMatHelperErrorOccurred " + errorCode + ":" + message);

            if (fpsMonitor != null)
            {
                fpsMonitor.ConsoleText = "ErrorCode: " + errorCode + ":" + message;
            }
        }

        // Update is called once per frame
        private void Update()
        {
            if (multiSourceToMatHelper.IsPlaying && multiSourceToMatHelper.DidUpdateThisFrame)
            {
                Mat rgbaMat = multiSourceToMatHelper.FrameMat;

                Imgproc.cvtColor(rgbaMat, rgbMat, Imgproc.COLOR_RGBA2RGB);
                Imgproc.cvtColor(rgbaMat, outputMat, Imgproc.COLOR_RGBA2RGB);

                detector.detect(rgbMat, keypoints);
                //Debug.Log ("keypoints.ToString() " + keypoints.ToString());
                Features.drawKeypoints(rgbMat, keypoints, rgbMat, Scalar.all(-1));

                Imgproc.rectangle(rgbMat, patternRect.tl(), patternRect.br(), new Scalar(255, 0, 0, 255), 5);

                OpenCVMatUnityUtils.MatToTexture2D(rgbMat, texture);
            }
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        private void OnDestroy()
        {
            if (patternPreviewTexture != null)
            {
                Destroy(patternPreviewTexture);
                patternPreviewTexture = null;
            }

            detector?.Dispose();
            detector = null;
            if (keypoints != null)
            {
                keypoints.Dispose();
                keypoints = null;
            }

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
            multiSourceToMatHelper.Play();
        }

        /// <summary>
        /// Raises the pause button click event.
        /// </summary>
        public void OnPauseButtonClick()
        {
            multiSourceToMatHelper.Pause();
        }

        /// <summary>
        /// Raises the stop button click event.
        /// </summary>
        public void OnStopButtonClick()
        {
            multiSourceToMatHelper.Stop();
        }

        /// <summary>
        /// Raises the change camera button click event.
        /// </summary>
        public void OnChangeCameraButtonClick()
        {
            if (multiSourceToMatHelper.ActiveHelper is ICameraFacingToMatHelperControls cameraFacingControls)
            {
                cameraFacingControls.RequestedIsFrontFacing = !cameraFacingControls.RequestedIsFrontFacing;
            }
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

            if (patternPreviewTexture != null)
            {
                Destroy(patternPreviewTexture);
                patternPreviewTexture = null;
            }

            patternPreviewTexture = new Texture2D(patternMat.width(), patternMat.height(), TextureFormat.RGBA32, false);

            OpenCVMatUnityUtils.MatToTexture2D(patternMat, patternPreviewTexture);

            patternRawImage.texture = patternPreviewTexture;

            patternRawImage.gameObject.SetActive(true);
        }

        /// <summary>
        /// Raises the save button click event.
        /// </summary>
        public void OnSaveButtonClick()
        {
            if (patternPreviewTexture != null)
            {
                Mat patternMat = new Mat(patternRect.size(), CvType.CV_8UC3);
                OpenCVMatUnityUtils.Texture2DToMat(patternPreviewTexture, patternMat);
                Imgproc.cvtColor(patternMat, patternMat, Imgproc.COLOR_RGB2BGR);

                string savePath = Application.persistentDataPath;
                Debug.Log("savePath " + savePath);

                Imgcodecs.imwrite(savePath + "/patternImg.jpg", patternMat);

                if (GraphicsSettings.currentRenderPipeline == null)
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
