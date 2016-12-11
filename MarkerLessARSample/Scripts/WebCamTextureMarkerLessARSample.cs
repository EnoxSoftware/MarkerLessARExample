using UnityEngine;
using System.Collections;
using UnityEngine.UI;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using OpenCVForUnity;

namespace MarkerLessARSample
{
    /// <summary>
    /// Marker Less AR sample from WebCamTexture.
    /// https://github.com/MasteringOpenCV/code/tree/master/Chapter3_MarkerlessAR by using "OpenCV for Unity"
    /// </summary>
    [RequireComponent(typeof(WebCamTextureToMatHelper))]
    public class WebCamTextureMarkerLessARSample : MonoBehaviour
    {
        /// <summary>
        /// The pattern mat.
        /// </summary>
        Mat patternMat;

        /// <summary>
        /// The pattern raw image.
        /// </summary>
        public RawImage patternRawImage;

        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

        /// <summary>
        /// The web cam texture to mat helper.
        /// </summary>
        WebCamTextureToMatHelper webCamTextureToMatHelper;

        /// <summary>
        /// The gray mat.
        /// </summary>
        Mat grayMat;

        /// <summary>
        /// The AR game object.
        /// </summary>
        public GameObject ARGameObject;
        
        /// <summary>
        /// The AR camera.
        /// </summary>
        public Camera ARCamera;
        
        /// <summary>
        /// The cam matrix.
        /// </summary>
        Mat camMatrix;
        
        /// <summary>
        /// The dist coeffs.
        /// </summary>
        MatOfDouble distCoeffs;
        
        /// <summary>
        /// The invert Y.
        /// </summary>
        Matrix4x4 invertYM;
        
        /// <summary>
        /// The transformation m.
        /// </summary>
        Matrix4x4 transformationM;
        
        /// <summary>
        /// The invert Z.
        /// </summary>
        Matrix4x4 invertZM;
        
        /// <summary>
        /// The ar m.
        /// </summary>
        Matrix4x4 ARM;
        
        /// <summary>
        /// The should move AR camera.
        /// </summary>
        public bool shouldMoveARCamera;

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
        /// The is showing axes.
        /// </summary>
        public bool isShowingAxes = false;

        /// <summary>
        /// The is showing axes toggle.
        /// </summary>
        public Toggle isShowingAxesToggle;

        /// <summary>
        /// The axes.
        /// </summary>
        public GameObject axes;

        /// <summary>
        /// The is showing cube.
        /// </summary>
        public bool isShowingCube = false;

        /// <summary>
        /// The is showing cube toggle.
        /// </summary>
        public Toggle isShowingCubeToggle;

        /// <summary>
        /// The cube.
        /// </summary>
        public GameObject cube;

        /// <summary>
        /// The is showing video.
        /// </summary>
        public bool isShowingVideo = false;

        /// <summary>
        /// The is showing video toggle.
        /// </summary>
        public Toggle isShowingVideoToggle;

        /// <summary>
        /// The video.
        /// </summary>
        public GameObject video;

        
        // Use this for initialization
        void Start ()
        {

            isShowingAxesToggle.isOn = isShowingAxes;
            axes.SetActive (isShowingAxes);
            isShowingCubeToggle.isOn = isShowingCube;
            cube.SetActive (isShowingCube);
            isShowingVideoToggle.isOn = isShowingVideo;
            video.SetActive (isShowingVideo);

            ARGameObject.gameObject.SetActive (false);


            webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper> ();

            patternMat = Imgcodecs.imread (Application.persistentDataPath + "/patternImg.jpg");
            if (patternMat.empty ()) {

                OnPatternCaptureButton ();
            } else {
            
                Imgproc.cvtColor (patternMat, patternMat, Imgproc.COLOR_BGR2RGB);

                webCamTextureToMatHelper.Init ();
            }

        }

        /// <summary>
        /// Raises the web cam texture to mat helper inited event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInited ()
        {
            Debug.Log ("OnWebCamTextureToMatHelperInited");


            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat ();
                    
            texture = new Texture2D (webCamTextureMat.cols (), webCamTextureMat.rows (), TextureFormat.RGBA32, false);
            gameObject.GetComponent<Renderer> ().material.mainTexture = texture;


            grayMat = new Mat (webCamTextureMat.cols (), webCamTextureMat.rows (), CvType.CV_8UC1);
                    

            gameObject.transform.localScale = new Vector3 (webCamTextureMat.cols (), webCamTextureMat.rows (), 1);
            
            Debug.Log ("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);



            float width = webCamTextureMat.width ();
            float height = webCamTextureMat.height ();
            
            float imageSizeScale = 1.0f;
            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale) {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
                imageSizeScale = (float)Screen.height / (float)Screen.width;
            } else {
                Camera.main.orthographicSize = height / 2;
            }
            
            
            //set cameraparam
            int max_d = (int)Mathf.Max (width, height);
            double fx = max_d;
            double fy = max_d;
            double cx = width / 2.0f;
            double cy = height / 2.0f;
            camMatrix = new Mat (3, 3, CvType.CV_64FC1);
            camMatrix.put (0, 0, fx);
            camMatrix.put (0, 1, 0);
            camMatrix.put (0, 2, cx);
            camMatrix.put (1, 0, 0);
            camMatrix.put (1, 1, fy);
            camMatrix.put (1, 2, cy);
            camMatrix.put (2, 0, 0);
            camMatrix.put (2, 1, 0);
            camMatrix.put (2, 2, 1.0f);
            Debug.Log ("camMatrix " + camMatrix.dump ());
            
            
            distCoeffs = new MatOfDouble (0, 0, 0, 0);
            Debug.Log ("distCoeffs " + distCoeffs.dump ());
            
            
            //calibration camera
            Size imageSize = new Size (width * imageSizeScale, height * imageSizeScale);
            double apertureWidth = 0;
            double apertureHeight = 0;
            double[] fovx = new double[1];
            double[] fovy = new double[1];
            double[] focalLength = new double[1];
            Point principalPoint = new Point (0, 0);
            double[] aspectratio = new double[1];
            
            Calib3d.calibrationMatrixValues (camMatrix, imageSize, apertureWidth, apertureHeight, fovx, fovy, focalLength, principalPoint, aspectratio);
            
            Debug.Log ("imageSize " + imageSize.ToString ());
            Debug.Log ("apertureWidth " + apertureWidth);
            Debug.Log ("apertureHeight " + apertureHeight);
            Debug.Log ("fovx " + fovx [0]);
            Debug.Log ("fovy " + fovy [0]);
            Debug.Log ("focalLength " + focalLength [0]);
            Debug.Log ("principalPoint " + principalPoint.ToString ());
            Debug.Log ("aspectratio " + aspectratio [0]);
            
            
            //To convert the difference of the FOV value of the OpenCV and Unity. 
            double fovXScale = (2.0 * Mathf.Atan ((float)(imageSize.width / (2.0 * fx)))) / (Mathf.Atan2 ((float)cx, (float)fx) + Mathf.Atan2 ((float)(imageSize.width - cx), (float)fx));
            double fovYScale = (2.0 * Mathf.Atan ((float)(imageSize.height / (2.0 * fy)))) / (Mathf.Atan2 ((float)cy, (float)fy) + Mathf.Atan2 ((float)(imageSize.height - cy), (float)fy));
            
            Debug.Log ("fovXScale " + fovXScale);
            Debug.Log ("fovYScale " + fovYScale);
            
            
            //Adjust Unity Camera FOV https://github.com/opencv/opencv/commit/8ed1945ccd52501f5ab22bdec6aa1f91f1e2cfd4
            if (widthScale < heightScale) {
                ARCamera.fieldOfView = (float)(fovx [0] * fovXScale);
            } else {
                ARCamera.fieldOfView = (float)(fovy [0] * fovYScale);
            }
                        
                        
                        
                        
            transformationM = new Matrix4x4 ();
                        
            invertYM = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, -1, 1));
            Debug.Log ("invertYM " + invertYM.ToString ());
                        
            invertZM = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, 1, -1));
            Debug.Log ("invertZM " + invertZM.ToString ());
                        


            Texture2D patternTexture = new Texture2D (patternMat.width (), patternMat.height (), TextureFormat.RGBA32, false);
            Utils.matToTexture2D (patternMat, patternTexture);
            Debug.Log ("patternMat dst ToString " + patternMat.ToString ());

            patternRawImage.texture = patternTexture;
            patternRawImage.rectTransform.localScale = new Vector3 (1.0f, (float)patternMat.height () / (float)patternMat.width (), 1.0f);

            pattern = new Pattern ();
            patternTrackingInfo = new PatternTrackingInfo ();
                    
            patternDetector = new PatternDetector (null, null, null, true);
                    
            patternDetector.buildPatternFromImage (patternMat, pattern);
            patternDetector.train (pattern);


            //if WebCamera is frontFaceing,flip Mat.
            if (webCamTextureToMatHelper.GetWebCamDevice ().isFrontFacing) {
                webCamTextureToMatHelper.flipHorizontal = true;
            }               
                           
        }

        /// <summary>
        /// Raises the web cam texture to mat helper disposed event.
        /// </summary>
        public void OnWebCamTextureToMatHelperDisposed ()
        {
            Debug.Log ("OnWebCamTextureToMatHelperDisposed");
            
            if (grayMat != null)
                grayMat.Dispose ();

            if (patternMat != null)
                patternMat.Dispose ();
        }

        /// <summary>
        /// Raises the web cam texture to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        public void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode){
            Debug.Log ("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
        }
        
        // Update is called once per frame
        void Update ()
        {
            if (webCamTextureToMatHelper.isPlaying () && webCamTextureToMatHelper.didUpdateThisFrame ()) {
                
                Mat rgbaMat = webCamTextureToMatHelper.GetMat ();

                Imgproc.cvtColor (rgbaMat, grayMat, Imgproc.COLOR_RGBA2GRAY);


                bool patternFound = patternDetector.findPattern (grayMat, patternTrackingInfo);
                
//              Debug.Log ("patternFound " + patternFound);
                if (patternFound) {
                    patternTrackingInfo.computePose (pattern, camMatrix, distCoeffs);

                    //Marker to Camera Coordinate System Convert Matrix
                    transformationM = patternTrackingInfo.pose3d;
                    //Debug.Log ("transformationM " + transformationM.ToString ());

                    if (shouldMoveARCamera) {
                        ARM = ARGameObject.transform.localToWorldMatrix * invertZM * transformationM.inverse * invertYM;
                        //Debug.Log ("ARM " + ARM.ToString ());
                                
                        ARUtils.SetTransformFromMatrix (ARCamera.transform, ref ARM);
                    } else {
                                
                        ARM = ARCamera.transform.localToWorldMatrix * invertYM * transformationM * invertZM;
                        //Debug.Log ("ARM " + ARM.ToString ());
                                
                        ARUtils.SetTransformFromMatrix (ARGameObject.transform, ref ARM);
                    }

                    ARGameObject.GetComponent<DelayableSetActive> ().SetActive (true);
                } else {

                    ARGameObject.GetComponent<DelayableSetActive> ().SetActive (false, 0.5f);
                }
                            
                
                Utils.matToTexture2D (rgbaMat, texture, webCamTextureToMatHelper.GetBufferColors ());

            }
            
        }

        /// <summary>
        /// Raises the disable event.
        /// </summary>
        void OnDisable ()
        {
            webCamTextureToMatHelper.Dispose ();
        }


        /// <summary>
        /// Raises the back button event.
        /// </summary>
        public void OnBackButton ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("MarkerLessARSample");
            #else
            Application.LoadLevel ("MarkerLessARSample");
            #endif
        }
        
        /// <summary>
        /// Raises the play button event.
        /// </summary>
        public void OnPlayButton ()
        {
            webCamTextureToMatHelper.Play ();
        }
        
        /// <summary>
        /// Raises the pause button event.
        /// </summary>
        public void OnPauseButton ()
        {
            webCamTextureToMatHelper.Pause ();
        }
        
        /// <summary>
        /// Raises the stop button event.
        /// </summary>
        public void OnStopButton ()
        {
            webCamTextureToMatHelper.Stop ();
        }
        
        /// <summary>
        /// Raises the change camera button event.
        /// </summary>
        public void OnChangeCameraButton ()
        {
            webCamTextureToMatHelper.Init (null, webCamTextureToMatHelper.requestWidth, webCamTextureToMatHelper.requestHeight, !webCamTextureToMatHelper.requestIsFrontFacing);
        }

        /// <summary>
        /// Raises the is showing axes toggle event.
        /// </summary>
        public void OnIsShowingAxesToggle ()
        {
            if (isShowingAxesToggle.isOn) {
                isShowingAxes = true;
            } else {
                isShowingAxes = false;
            }
            axes.SetActive (isShowingAxes);
        }

        /// <summary>
        /// Raises the is showing cube toggle event.
        /// </summary>
        public void OnIsShowingCubeToggle ()
        {
            if (isShowingCubeToggle.isOn) {
                isShowingCube = true;
            } else {
                isShowingCube = false;
            }
            cube.SetActive (isShowingCube);
        }

        /// <summary>
        /// Raises the is showing video toggle event.
        /// </summary>
        public void OnIsShowingVideoToggle ()
        {
            if (isShowingVideoToggle.isOn) {
                isShowingVideo = true;
            } else {
                isShowingVideo = false;
            }
            video.SetActive (isShowingVideo);
        }

        /// <summary>
        /// Raises the pattern capture button event.
        /// </summary>
        public void OnPatternCaptureButton ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("CapturePattern");
            #else
            Application.LoadLevel ("CapturePattern");
            #endif
        }
    }
    
}
