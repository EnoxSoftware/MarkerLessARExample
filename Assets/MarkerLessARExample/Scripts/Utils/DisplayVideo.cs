using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.VideoioModule;
using UnityEngine;
using VideoCapture = OpenCVForUnity.VideoioModule.VideoCapture;

namespace MarkerLessARExample
{
    /// <summary>
    /// Display video.
    /// </summary>
    public class DisplayVideo : MonoBehaviour
    {
        /// <summary>
        /// The name of the file.
        /// </summary>
        public string fileName;

        /// <summary>
        /// The video capture.
        /// </summary>
        VideoCapture capture;

        /// <summary>
        /// The rgb mat.
        /// </summary>
        Mat rgbMat;

        /// <summary>
        /// The colors.
        /// </summary>
        Color32[] colors;

        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

        // Use this for initialization
        void Start()
        {
            capture = new VideoCapture();

#if UNITY_WEBGL
            StartCoroutine(Utils.getFilePathAsync(fileName, (result) => {
                capture.open (result);
                Init();
            }));
#else
            capture.open(Utils.getFilePath(fileName));
            Init();
#endif
        }

        private void Init()
        {
            rgbMat = new Mat();

            if (capture.isOpened())
            {
                Debug.Log("capture.isOpened() true");
            }
            else
            {
                Debug.Log("capture.isOpened() false");
            }

            Debug.Log("CAP_PROP_FORMAT: " + capture.get(Videoio.CAP_PROP_FORMAT));
            Debug.Log("CAP_PROP_POS_MSEC: " + capture.get(Videoio.CAP_PROP_POS_MSEC));
            Debug.Log("CAP_PROP_POS_FRAMES: " + capture.get(Videoio.CAP_PROP_POS_FRAMES));
            Debug.Log("CAP_PROP_POS_AVI_RATIO: " + capture.get(Videoio.CAP_PROP_POS_AVI_RATIO));
            Debug.Log("CAP_PROP_FRAME_COUNT: " + capture.get(Videoio.CAP_PROP_FRAME_COUNT));
            Debug.Log("CAP_PROP_FPS: " + capture.get(Videoio.CAP_PROP_FPS));
            Debug.Log("CAP_PROP_FRAME_WIDTH: " + capture.get(Videoio.CAP_PROP_FRAME_WIDTH));
            Debug.Log("CAP_PROP_FRAME_HEIGHT: " + capture.get(Videoio.CAP_PROP_FRAME_HEIGHT));

            capture.grab();
            capture.retrieve(rgbMat, 0);
            colors = new Color32[rgbMat.cols() * rgbMat.rows()];
            texture = new Texture2D(rgbMat.cols(), rgbMat.rows(), TextureFormat.RGBA32, false);
            gameObject.transform.localScale = new Vector3(-((float)rgbMat.cols() / (float)rgbMat.rows()), -1, -1);
            capture.set(Videoio.CAP_PROP_POS_FRAMES, 0);


            gameObject.GetComponent<Renderer>().material.mainTexture = texture;
        }

        // Update is called once per frame
        void Update()
        {
            //Loop play
            if (capture.get(Videoio.CAP_PROP_POS_FRAMES) >= capture.get(Videoio.CAP_PROP_FRAME_COUNT))
                capture.set(Videoio.CAP_PROP_POS_FRAMES, 0);

            if (capture.grab())
            {
                capture.retrieve(rgbMat, 0);

                Imgproc.cvtColor(rgbMat, rgbMat, Imgproc.COLOR_BGR2RGB);

                Utils.matToTexture2D(rgbMat, texture, colors);
            }
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy()
        {
            capture.release();

            if (rgbMat != null)
                rgbMat.Dispose();
        }
    }
}