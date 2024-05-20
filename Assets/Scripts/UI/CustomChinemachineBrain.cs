using Cinemachine;
using UnityEngine;

namespace VLCNP.UI
{
    public class CustomChinemachineBrain : MonoBehaviour
    {
        CinemachineBrain brain;
        void Awake()
        {
            brain = GetComponent<CinemachineBrain>();
        }

        public void SetBlendSpeed(float speed)
        {
            brain.m_DefaultBlend.m_Time = speed;
        }
    }
}
