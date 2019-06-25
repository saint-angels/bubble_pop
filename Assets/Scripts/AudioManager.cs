using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum SFXType
    {
        BubbleExplode,
        BubbleMerge,
    }

    [System.Serializable]
    struct SFXTypeClipSetting
    {
        public SFXType sfxType;
        public AudioClip clip;
    }


    [SerializeField] private AudioSource audioSource = null;
    [SerializeField] private SFXTypeClipSetting[] clipSettings = null;

    public void Play(SFXType sfxType)
    {
        AudioClip clipToPlay = null;

        //Get clip for sfxType
        foreach (var clipSetting in clipSettings)
        {
            if (clipSetting.sfxType == sfxType)
            {
                clipToPlay = clipSetting.clip;
                break;
            }
        }

        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay);
        }
        else
        {
            Debug.LogWarning($"Don't have a clip for sfxType {sfxType}");
        }
    }
}
