using UnityEngine;
using System.Collections;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace MarkerLessARSample
{
    public class ShowLicense : MonoBehaviour
    {
        // Use this for initialization
        void Start ()
        {
            
        }
        
        // Update is called once per frame
        void Update ()
        {
            
        }
        
        public void OnBackButtonButtonClick ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("MarkerLessARExample");
            #else
            Application.LoadLevel ("MarkerLessARExample");
            #endif
        }
    }
}
