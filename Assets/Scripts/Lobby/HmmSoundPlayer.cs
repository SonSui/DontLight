using UnityEngine;
using System.Collections;

public class HmmSoundPlayer : MonoBehaviour
{
    public AudioClip[] soundClips; // ��Inspector�з��������Ч
    public float minDelay = 5f;    // ��С�ӳ�ʱ��(��)
    public float maxDelay = 10f;    // ����ӳ�ʱ��(��)

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
            // �ȴ����ʱ��
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            // ����Ƿ�����Ч�ɹ�����
            if (soundClips.Length == 0)
            {
                Debug.LogWarning("No sound clips assigned!");
                continue;
            }

            // ���ѡ��һ����Ч
            AudioClip clip = soundClips[Random.Range(0, soundClips.Length)];

            // ������Ч�ٶ�Ϊ0.7��
            audioSource.pitch = 0.7f;
            audioSource.PlayOneShot(clip);
        }
    }
}