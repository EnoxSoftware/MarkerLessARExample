using UnityEngine;
using UnityEngine.SceneManagement;

namespace MarkerLessARSample
{
    /// <summary>
    /// Show License
    /// </summary>
    public class ShowLicense : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnBackButtonButtonClick()
        {
            SceneManager.LoadScene("MarkerLessARExample");
        }
    }
}
