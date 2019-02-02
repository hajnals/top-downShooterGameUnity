using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    float masterVolumentPercent = 1;
    float sfxVolumePercent = 1;
    float musicVolumePercent = 0.2f;

    AudioSource[] musicSources;
    int activeMusicSourceIndex;

    Transform audioListener;
    Transform playerT;

    /* Make it singleton */
    public static AudioManager instance;

    private void Awake() {

        instance = this;

        musicSources = new AudioSource[2];
        for (int i=0; i < 2; i++) {
            GameObject newMusicSource = new GameObject ("Music source " + (i + 1));
            musicSources[i] = newMusicSource.AddComponent<AudioSource> ();
            newMusicSource.transform.parent = transform;
        }

        audioListener = FindObjectOfType<AudioListener> ().transform;
        playerT = FindObjectOfType<Player> ().transform;
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 1) {
        activeMusicSourceIndex = 1 - activeMusicSourceIndex;
        musicSources[activeMusicSourceIndex].clip = clip;
        musicSources[activeMusicSourceIndex].Play ();

        StartCoroutine (AnimateMusicCrossfade (fadeDuration));
    }

    public void PlaySound(AudioClip clip, Vector3 pos) {
        if(clip != null) {
            /* For short sounds, cant change volume while playing */
            AudioSource.PlayClipAtPoint (clip, pos, sfxVolumePercent * masterVolumentPercent);
        }
    }

    IEnumerator AnimateMusicCrossfade(float duration) {
        float percent = 0;

        while(percent < 1) {
            percent += Time.deltaTime * 1 / duration;
            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp (0, musicVolumePercent * masterVolumentPercent, percent);
            musicSources[1-activeMusicSourceIndex].volume = Mathf.Lerp (musicVolumePercent * masterVolumentPercent, 0, percent);
            yield return null;
        }
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(playerT != null) {
            audioListener.position = playerT.position;
        }
	}
}
