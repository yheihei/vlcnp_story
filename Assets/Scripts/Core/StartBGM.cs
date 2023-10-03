using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLCNP.Core;

public class StartBGM : MonoBehaviour
{
    AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        // audioSourceにclipが設定されていなければ、AreaBGMを探して再生
        if (audioSource.clip == null)
        {
            // AreaBGMタグを使って取得
            AreaBGM areaBGM = GameObject.FindWithTag("AreaBGM").GetComponent<AreaBGM>();
            if (areaBGM != null)
            {
                audioSource.clip = areaBGM.GetAudioClip();
                audioSource.volume = areaBGM.GetVolume();
                audioSource.pitch = areaBGM.GetPitch();
                audioSource.Play();
            }
        }
    }
}
