using UnityEngine;
using System.Collections;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace MarkerLessARSample
{
    public class MarkerLessARSample : MonoBehaviour
    {
        
        // Use this for initialization
        void Start ()
        {
            
        }
        
        // Update is called once per frame
        void Update ()
        {
            
        }

        public void OnShowLicenseButton ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("ShowLicense");
            #else
            Application.LoadLevel ("ShowLicense");
            #endif
        }
        
        public void Texture2DMarkerLessARSample ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("Texture2DMarkerLessARSample");
            #else
            Application.LoadLevel ("Texture2DMarkerLessARSample");
            #endif
        }
        
        public void WebCamTextureMarkerLessARSample ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("WebCamTextureMarkerLessARSample");
            #else
            Application.LoadLevel ("WebCamTextureMarkerLessARSample");
            #endif
        }
    }
}