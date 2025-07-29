using UnityEngine;
using System.Collections;

public class HmmSoundPlayer : MonoBehaviour
{
    public AudioClip[] soundClips; // 在Inspector中分配你的音效
    public float minDelay = 5f;    // 最小延迟时间(秒)
    public float maxDelay = 10f;    // 最大延迟时间(秒)

    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        StartCoroutine(PlayRandomSound());
    }

    IEnumerator PlayRandomSound()
    {
        while (true)
        {
            // 等待随机时间
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            // 检查是否有音效可供播放
            if (soundClips.Length == 0)
            {
                Debug.LogWarning("No sound clips assigned!");
                continue;
            }

            // 随机选择一个音效
            AudioClip clip = soundClips[Random.Range(0, soundClips.Length)];

            // 设置音效速度为0.7倍
            audioSource.pitch = 0.7f;
            audioSource.PlayOneShot(clip);
        }
    }
}