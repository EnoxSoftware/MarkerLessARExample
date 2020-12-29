using System.Collections;
using UnityEngine;

namespace MarkerLessARExample
{
    public class DelayableSetActive : MonoBehaviour
    {
        private Coroutine deactivateCoroutine;

        /// <summary>
        /// Activates/Deactivates the GameObject.
        /// </summary>
        /// <param name="value">If set to <c>true</c> value.</param>
        /// <param name="delayTime">Delay time.</param>
        public void SetActive(bool value, float delayTime = 0.0f)
        {
            if (value)
            {
                if (deactivateCoroutine != null)
                {
                    StopCoroutine(deactivateCoroutine);
                    deactivateCoroutine = null;
                }

                gameObject.SetActive(value);
            }
            else
            {
                if (delayTime == 0.0f)
                {
                    gameObject.SetActive(value);
                    return;
                }

                if (gameObject.activeSelf && deactivateCoroutine == null)
                    deactivateCoroutine = StartCoroutine(DeactivateGameObject(delayTime));
            }
        }

        private IEnumerator DeactivateGameObject(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            gameObject.SetActive(false);
            deactivateCoroutine = null;
        }
    }
}
