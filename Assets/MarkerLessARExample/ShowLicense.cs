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
        private void Start()
        {

        }

        // Update is called once per frame
        private void Update()
        {

        }

        public void OnBackButtonButtonClick()
        {
            SceneManager.LoadScene("MarkerLessARExample");
        }
    }
}
