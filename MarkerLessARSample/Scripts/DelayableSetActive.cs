using UnityEngine;
using System.Collections;

namespace MarkerLessARSample
{
    public class DelayableSetActive : MonoBehaviour
    {

//    private Coroutine activateCoroutine;
        private Coroutine deactivateCoroutine;

        /// <summary>
        /// Sets the active.
        /// </summary>
        /// <param name="value">If set to <c>true</c> value.</param>
        /// <param name="delayTime">Delay time.</param>
        public void SetActive (bool value, float delayTime = 0.0f)
        {

            if (value) {
                if (deactivateCoroutine != null) {
                    StopCoroutine (deactivateCoroutine);
                    deactivateCoroutine = null;
                }


                gameObject.SetActive (value);


//            if (!gameObject.activeSelf && activateCoroutine == null)
//                activateCoroutine = StartCoroutine (ActivateGameObject (delayTime));

            } else {
//            if (activateCoroutine != null) {
//                StopCoroutine (activateCoroutine);
//                activateCoroutine = null;
//            }

                if (delayTime == 0.0f) {
                    gameObject.SetActive (value);
                    return;
                }

                if (gameObject.activeSelf && deactivateCoroutine == null)
                    deactivateCoroutine = StartCoroutine (DeactivateGameObject (delayTime));
            }
        }

//    private IEnumerator ActivateGameObject (float delayTime)
//    {
//        Debug.Log ("ActivateGameObject start");
//
//        yield return new WaitForSeconds (delayTime);
//        
//        gameObject.SetActive (true);
//        activateCoroutine = null;
//
//        Debug.Log ("ActivateGameObject end");
//    }

        private IEnumerator DeactivateGameObject (float delayTime)
        {

            yield return new WaitForSeconds (delayTime);
        
            gameObject.SetActive (false);
            deactivateCoroutine = null;

        }
    }
}
