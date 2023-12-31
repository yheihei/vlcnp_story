using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLCNP.SceneManagement
{
    public class TransitionSpawnPoint : MonoBehaviour
    {
        [SerializeField] public string spawnPointName = "A";
        [SerializeField] public bool isPlayerDirectionLeft;
    }
}
