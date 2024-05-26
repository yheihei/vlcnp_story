using UnityEngine;

namespace VLCNP.Core
{
    public class PlaySpeed : MonoBehaviour
    {
        public void SetSpeed(float speed)
        {
            Time.timeScale = speed;
        }
    }
}
