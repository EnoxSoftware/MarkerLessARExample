using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.VideoioModule;
using System;
using System.Collections;
using System.Collections.Generic;
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

        /// <summary>
        /// Indicates whether the video is playing.
        /// </summary>
        bool isPlaying = false;

#if UNITY_WEBGL
        IEnumerator getFilePath_Coroutine;
#endif

        // Use this for initialization
        void Start()
        {
            capture = new VideoCapture();

#if UNITY_WEBGL

            getFilePath_Coroutine = Utils.getFilePathAsync(fileName, (result) =>
            {
                getFilePath_Coroutine = null;

                Debug.Log("result "+ result);

                capture.open(result);
                Init();
            });
            StartCoroutine(getFilePath_Coroutine);
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

                isPlaying = true;
            }
            else
            {
                Debug.Log("capture.isOpened() false");
            }


        }

        // Update is called once per frame
        void Update()
        {
            if (isPlaying)
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
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy()
        {

            if (capture != null)
                capture.release();

            if (rgbMat != null)
                rgbMat.Dispose();

#if UNITY_WEBGL
            if (getFilePath_Coroutine != null)
            {
                StopCoroutine(getFilePath_Coroutine);
                ((IDisposable)getFilePath_Coroutine).Dispose();
            }
#endif
        }
    }
}