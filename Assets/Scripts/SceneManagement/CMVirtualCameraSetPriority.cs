using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace VLCNP.SceneManagement
{
    public class CMVirtualCameraSetPriority : MonoBehaviour
    {
        CinemachineVirtualCamera vcam;

        void Awake()
        {
            vcam = GetComponent<CinemachineVirtualCamera>();
        }

        public void SetPriority(int priority)
        {
            vcam.Priority = priority;
        }
    }    
}
