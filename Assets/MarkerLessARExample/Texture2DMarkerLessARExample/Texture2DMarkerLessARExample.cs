using OpenCVForUnity.Calib3dModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
using OpenCVMarkerLessAR;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MarkerLessARExample
{
    /// <summary>
    /// Texture2D Markerless AR Example
    /// This code is a rewrite of https://github.com/MasteringOpenCV/code/tree/master/Chapter3_MarkerlessAR using "OpenCV for Unity".
    /// </summary>
    public class Texture2DMarkerLessARExample : MonoBehaviour
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
        /// The image texture.
        /// </summary>
        public Texture2D imgTexture;

        /// <summary>
        /// The AR camera.
        /// </summary>
        public Camera ARCamera;

        /// <summary>
        /// Determines if should move AR camera.
        /// </summary>
        public bool shouldMoveARCamera;

        /// <summary>
        /// The AR game object.
        /// </summary>
        public GameObject ARGameObject;

        // Use this for initialization
        void Start()
        {
            Mat patternMat = new Mat(patternTexture.height, patternTexture.width, CvType.CV_8UC4);

            Utils.texture2DToMat(patternTexture, patternMat);
            Debug.Log("patternMat dst ToString " + patternMat.ToString());

            patternRawImage.texture = patternTexture;
            patternRawImage.rectTransform.localScale = new Vector3(1.0f, (float)patternMat.height() / (float)patternMat.width(), 1.0f);


            Mat imgMat = new Mat(imgTexture.height, imgTexture.width, CvType.CV_8UC4);

            Utils.texture2DToMat(imgTexture, imgMat);
            Debug.Log("imgMat dst ToString " + imgMat.ToString());

            gameObject.transform.localScale = new Vector3(imgTexture.width, imgTexture.height, 1);
            Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);


            float width = imgMat.width();
            float height = imgMat.height();

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
            Mat camMatrix = new Mat(3, 3, CvType.CV_64FC1);
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


            MatOfDouble distCoeffs = new MatOfDouble(0, 0, 0, 0);
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



            //Learning the feature points of the pattern image.
            Pattern pattern = new Pattern();
            PatternTrackingInfo patternTrackingInfo = new PatternTrackingInfo();

            PatternDetector patternDetector = new PatternDetector(null, null, null, true);

            bool patternFound = false;
            bool patternBuildSucceeded = patternDetector.buildPatternFromImage(patternMat, pattern);

            Debug.Log("patternBuildSucceeded " + patternBuildSucceeded);

            if (patternBuildSucceeded)
            {
                patternDetector.train(pattern);
                patternFound = patternDetector.findPattern(imgMat, patternTrackingInfo);
            }

            Debug.Log("patternFound " + patternFound);

            if (patternFound)
            {
                patternTrackingInfo.computePose(pattern, camMatrix, distCoeffs);

                Matrix4x4 transformationM = patternTrackingInfo.pose3d;
                Debug.Log("transformationM " + transformationM.ToString());

                Matrix4x4 invertZM = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
                Debug.Log("invertZM " + invertZM.ToString());

                Matrix4x4 invertYM = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, 1));
                Debug.Log("invertYM " + invertYM.ToString());


                // right-handed coordinates system (OpenCV) to left-handed one (Unity)
                // https://stackoverflow.com/questions/30234945/change-handedness-of-a-row-major-4x4-transformation-matrix
                Matrix4x4 ARM = invertYM * transformationM * invertYM;

                // Apply Y-axis and Z-axis refletion matrix. (Adjust the posture of the AR object)
                ARM = ARM * invertYM * invertZM;

                if (shouldMoveARCamera)
                {
                    ARM = ARGameObject.transform.localToWorldMatrix * ARM.inverse;

                    Debug.Log("ARM " + ARM.ToString());

                    ARUtils.SetTransformFromMatrix(ARCamera.transform, ref ARM);
                }
                else
                {
                    ARM = ARCamera.transform.localToWorldMatrix * ARM;

                    Debug.Log("ARM " + ARM.ToString());

                    ARUtils.SetTransformFromMatrix(ARGameObject.transform, ref ARM);
                }
            }

            Texture2D texture = new Texture2D(imgMat.cols(), imgMat.rows(), TextureFormat.RGBA32, false);

            Utils.matToTexture2D(imgMat, texture);

            gameObject.GetComponent<Renderer>().material.mainTexture = texture;
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick()
        {
            SceneManager.LoadScene("MarkerLessARExample");
        }
    }
}
