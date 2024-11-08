using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVMarkerLessAR;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MarkerLessARExample
{
    /// <summary>
    /// MultiSource Markerless AR Example
    /// This code is a rewrite of https://github.com/MasteringOpenCV/code/tree/master/Chapter3_MarkerlessAR using "OpenCV for Unity".
    /// </summary>
    [RequireComponent(typeof(MultiSource2MatHelper))]
    public class MultiSourceMarkerLessARExample : MonoBehaviour
    {
        /// <summary>
        /// The pattern texture.
        /// </summary>
        public Texture2D patternTexture;

        /// <summary>
        /// The pattern raw image.
        /// </summary>
        public RawImage patternRawImage;

        /// <summary>
        /// The AR game object.
        /// </summary>
        public GameObject ARGameObject;

        /// <summary>
        /// The AR camera.
        /// </summary>
        public Camera ARCamera;

        /// <summary>
        /// Determines if should move AR camera.
        /// </summary>
        public bool shouldMoveARCamera;

        /// <summary>
        /// Determines if displays axes.
        /// </summary>
        public bool displayAxes = false;

        /// <summary>
        /// The display axes toggle.
        /// </summary>
        public Toggle displayAxesToggle;

        /// <summary>
        /// The axes.
        /// </summary>
        public GameObject axes;

        /// <summary>
        /// Determines if displays cube.
        /// </summary>
        public bool displayCube = false;

        /// <summary>
        /// The display cube toggle.
        /// </summary>
        public Toggle displayCubeToggle;

        /// <summary>
        /// The cube.
        /// </summary>
        public GameObject cube;

        /// <summary>
        /// Determines if displays video.
        /// </summary>
        public bool displayVideo = false;

        /// <summary>
        /// The display video toggle.
        /// </summary>
        public Toggle displayVideoToggle;

        /// <summary>
        /// The video.
        /// </summary>
        public GameObject video;

        /// <summary>
        /// The CapturePattern Button.
        /// </summary>
        public Button CapturePatternButton;

        /// <summary>
        /// The pattern mat.
        /// </summary>
        Mat patternMat;

        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

        /// <summary>
        /// The multi source to mat helper.
        /// </summary>
        MultiSource2MatHelper multiSource2MatHelper;

        /// <summary>
        /// The gray mat.
        /// </summary>
        Mat grayMat;

        /// <summary>
        /// The cameraparam matrix.
        /// </summary>
        Mat camMatrix;

        /// <summary>
        /// The dist coeffs.
        /// </summary>
        MatOfDouble distCoeffs;

        /// <summary>
        /// The matrix that inverts the Y axis.
        /// </summary>
        Matrix4x4 invertYM;

        /// <summary>
        /// The matrix that inverts the Z axis.
        /// </summary>
        Matrix4x4 invertZM;

        /// <summary>
        /// The pattern.
        /// </summary>
        Pattern pattern;

        /// <summary>
        /// The pattern tracking info.
        /// </summary>
        PatternTrackingInfo patternTrackingInfo;

        /// <summary>
        /// The pattern detector.
        /// </summary>
        PatternDetector patternDetector;

        /// <summary>
        /// The FPS monitor.
        /// </summary>
        FpsMonitor fpsMonitor;

        // Use this for initialization
        void Start()
        {
            displayAxesToggle.isOn = displayAxes;
            axes.SetActive(displayAxes);
            displayCubeToggle.isOn = displayCube;
            cube.SetActive(displayCube);
            displayVideoToggle.isOn = displayVideo;
            video.SetActive(displayVideo);

            ARGameObject.gameObject.SetActive(false);

            multiSource2MatHelper = gameObject.GetComponent<MultiSource2MatHelper>();
            multiSource2MatHelper.outputColorFormat = Source2MatHelperColorFormat.RGBA;

            if (patternTexture != null)
            {
                patternMat = new Mat(patternTexture.height, patternTexture.width, CvType.CV_8UC3);
                Utils.texture2DToMat(patternTexture, patternMat);
                Imgproc.cvtColor(patternMat, patternMat, Imgproc.COLOR_RGB2BGR);
                Debug.Log("patternMat dst ToString " + patternMat.ToString());

                CapturePatternButton.interactable = false;
            }
            else
            {
                patternMat = Imgcodecs.imread(Application.persistentDataPath + "/patternImg.jpg");
            }

            if (patternMat == null || patternMat.total() == 0)
            {
                OnCapturePatternButtonClick();
            }
            else
            {
                Imgproc.cvtColor(patternMat, patternMat, Imgproc.COLOR_BGR2RGB);

                Texture2D patternTexture = new Texture2D(patternMat.width(), patternMat.height(), TextureFormat.RGBA32, false);

                //To reuse mat, set the flipAfter flag to true.
                Utils.matToTexture2D(patternMat, patternTexture, true, 0, true);
                Debug.Log("patternMat dst ToString " + patternMat.ToString());

                patternRawImage.texture = patternTexture;
                patternRawImage.rectTransform.localScale = new Vector3(1.0f, (float)patternMat.height() / (float)patternMat.width(), 1.0f);

                pattern = new Pattern();
                patternTrackingInfo = new PatternTrackingInfo();

                patternDetector = new PatternDetector(null, null, null, true);

                bool patternBuildSucceeded = patternDetector.buildPatternFromImage(patternMat, pattern);

                if (patternBuildSucceeded)
                {
                    patternDetector.train(pattern);

                    multiSource2MatHelper.Initialize();
                }
                else
                {
                    Debug.LogError("Input image could not be used as pattern image due to missing keypoints.");
                }
            }
        }

        /// <summary>
        /// Raises the source to mat helper initialized event.
        /// </summary>
        public void OnSourceToMatHelperInitialized()
        {
            Debug.Log("OnSourceToMatHelperInitialized");

            Mat rgbaMat = multiSource2MatHelper.GetMat();

            texture = new Texture2D(rgbaMat.width(), rgbaMat.height(), TextureFormat.RGBA32, false);
            grayMat = new Mat(rgbaMat.rows(), rgbaMat.cols(), CvType.CV_8UC1);

            // Set the Texture2D as the main texture of the Renderer component attached to the game object
            gameObject.GetComponent<Renderer>().material.mainTexture = texture;

            // Adjust the scale of the game object to match the dimensions of the texture
            gameObject.transform.localScale = new Vector3(rgbaMat.width(), rgbaMat.height(), 1);
            Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            // Adjust the orthographic size of the main Camera to fit the aspect ratio of the image
            float width = rgbaMat.width();
            float height = rgbaMat.height();
            float imageSizeScale = 1.0f;
            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale)
            {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
                imageSizeScale = (float)Screen.height / (float)Screen.width;
            }
            else
            {
                Camera.main.orthographicSize = height / 2;
            }


            //set cameraparam
            int max_d = (int)Mathf.Max(width, height);
            double fx = max_d;
            double fy = max_d;
            double cx = width / 2.0f;
            double cy = height / 2.0f;
            camMatrix = new Mat(3, 3, CvType.CV_64FC1);
            camMatrix.put(0, 0, fx);
            camMatrix.put(0, 1, 0);
            camMatrix.put(0, 2, cx);
            camMatrix.put(1, 0, 0);
            camMatrix.put(1, 1, fy);
            camMatrix.put(1, 2, cy);
            camMatrix.put(2, 0, 0);
            camMatrix.put(2, 1, 0);
            camMatrix.put(2, 2, 1.0f);
            Debug.Log("camMatrix " + camMatrix.dump());


            distCoeffs = new MatOfDouble(0, 0, 0, 0);
            Debug.Log("distCoeffs " + distCoeffs.dump());


            //calibration camera
            Size imageSize = new Size(width * imageSizeScale, height * imageSizeScale);
            double apertureWidth = 0;
            double apertureHeight = 0;
            double[] fovx = new double[1];
            double[] fovy = new double[1];
            double[] focalLength = new double[1];
            Point principalPoint = new Point(0, 0);
            double[] aspectratio = new double[1];

            Calib3d.calibrationMatrixValues(camMatrix, imageSize, apertureWidth, apertureHeight, fovx, fovy, focalLength, principalPoint, aspectratio);

            Debug.Log("imageSize " + imageSize.ToString());
            Debug.Log("apertureWidth " + apertureWidth);
            Debug.Log("apertureHeight " + apertureHeight);
            Debug.Log("fovx " + fovx[0]);
            Debug.Log("fovy " + fovy[0]);
            Debug.Log("focalLength " + focalLength[0]);
            Debug.Log("principalPoint " + principalPoint.ToString());
            Debug.Log("aspectratio " + aspectratio[0]);


            //To convert the difference of the FOV value of the OpenCV and Unity. 
            double fovXScale = (2.0 * Mathf.Atan((float)(imageSize.width / (2.0 * fx)))) / (Mathf.Atan2((float)cx, (float)fx) + Mathf.Atan2((float)(imageSize.width - cx), (float)fx));
            double fovYScale = (2.0 * Mathf.Atan((float)(imageSize.height / (2.0 * fy)))) / (Mathf.Atan2((float)cy, (float)fy) + Mathf.Atan2((float)(imageSize.height - cy), (float)fy));

            Debug.Log("fovXScale " + fovXScale);
            Debug.Log("fovYScale " + fovYScale);


            //Adjust Unity Camera FOV https://github.com/opencv/opencv/commit/8ed1945ccd52501f5ab22bdec6aa1f91f1e2cfd4
            if (widthScale < heightScale)
            {
                ARCamera.fieldOfView = (float)(fovx[0] * fovXScale);
            }
            else
            {
                ARCamera.fieldOfView = (float)(fovy[0] * fovYScale);
            }


            invertYM = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, 1));
            Debug.Log("invertYM " + invertYM.ToString());

            invertZM = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
            Debug.Log("invertZM " + invertZM.ToString());


            // If the WebCam is front facing, flip the Mat horizontally. Required for successful detection.
            if (multiSource2MatHelper.source2MatHelper is WebCamTexture2MatHelper webCamHelper)
                webCamHelper.flipHorizontal = webCamHelper.IsFrontFacing();
        }

        /// <summary>
        /// Raises the source to mat helper disposed event.
        /// </summary>
        public void OnSourceToMatHelperDisposed()
        {
            Debug.Log("OnSourceToMatHelperDisposed");

            if (grayMat != null)
                grayMat.Dispose();
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

                Imgproc.cvtColor(rgbaMat, grayMat, Imgproc.COLOR_RGBA2GRAY);


                bool patternFound = patternDetector.findPattern(grayMat, patternTrackingInfo);

                //Debug.Log ("patternFound " + patternFound);
                if (patternFound)
                {
                    patternTrackingInfo.computePose(pattern, camMatrix, distCoeffs);

                    //Marker to Camera Coordinate System Convert Matrix
                    Matrix4x4 transformationM = patternTrackingInfo.pose3d;


                    // right-handed coordinates system (OpenCV) to left-handed one (Unity)
                    // https://stackoverflow.com/questions/30234945/change-handedness-of-a-row-major-4x4-transformation-matrix
                    Matrix4x4 ARM = invertYM * transformationM * invertYM;

                    // Apply Y-axis and Z-axis refletion matrix. (Adjust the posture of the AR object)
                    ARM = ARM * invertYM * invertZM;

                    if (shouldMoveARCamera)
                    {
                        ARM = ARGameObject.transform.localToWorldMatrix * ARM.inverse;

                        //Debug.Log("ARM " + ARM.ToString());

                        ARUtils.SetTransformFromMatrix(ARCamera.transform, ref ARM);
                    }
                    else
                    {
                        ARM = ARCamera.transform.localToWorldMatrix * ARM;

                        //Debug.Log("ARM " + ARM.ToString());

                        ARUtils.SetTransformFromMatrix(ARGameObject.transform, ref ARM);
                    }

                    ARGameObject.GetComponent<DelayableSetActive>().SetActive(true);
                }
                else
                {
                    ARGameObject.GetComponent<DelayableSetActive>().SetActive(false, 0.5f);
                }

                Utils.matToTexture2D(rgbaMat, texture);
            }
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy()
        {
            multiSource2MatHelper.Dispose();

            if (patternMat != null)
                patternMat.Dispose();
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
        /// Raises the display axes toggle value changed event.
        /// </summary>
        public void OnDisplayAxesToggleValueChanged()
        {
            if (displayAxesToggle.isOn)
            {
                displayAxes = true;
            }
            else
            {
                displayAxes = false;
            }
            axes.SetActive(displayAxes);
        }

        /// <summary>
        /// Raises the display cube toggle value changed event.
        /// </summary>
        public void OnDisplayCubeToggleValueChanged()
        {
            if (displayCubeToggle.isOn)
            {
                displayCube = true;
            }
            else
            {
                displayCube = false;
            }
            cube.SetActive(displayCube);
        }

        /// <summary>
        /// Raises the display video toggle value changed event.
        /// </summary>
        public void OnDisplayVideoToggleValueChanged()
        {
            if (displayVideoToggle.isOn)
            {
                displayVideo = true;
            }
            else
            {
                displayVideo = false;
            }
            video.SetActive(displayVideo);
        }

        /// <summary>
        /// Raises the capture pattern button click event.
        /// </summary>
        public void OnCapturePatternButtonClick()
        {
            SceneManager.LoadScene("CapturePattern");
        }
    }
}
