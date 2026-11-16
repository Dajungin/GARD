
using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    [Header("재생할 배경음")]
    [SerializeField]
    private AudioClip bgmClip;

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(bgmClip);
        }
    }
}
