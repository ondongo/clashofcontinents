namespace DevelopersHub.ClashOfWhatecer
{
    using UnityEngine;

    public class SoundManager : MonoBehaviour
    {
        [Header("Sources")]
        public AudioSource musicSource = null;
        public AudioSource soundSource = null;

        [Header("Clips")]
        public AudioClip mainMusic = null;
        public AudioClip battleMusic = null;
        public AudioClip victoryMusic = null;
        public AudioClip victorySound = null;
        public AudioClip buttonClickSound = null;
        public AudioClip goldCollect = null;
        public AudioClip elixirCollect = null;
        public AudioClip buildStart = null;
        public AudioClip placeUnitSound = null;

        private static SoundManager _instance = null;
        public static SoundManager instanse { get { return _instance; } }

        private bool _musicMute = false;
        public bool musicMute
        {
            get { return _musicMute; }
            set
            {
                _musicMute = value;
                if (musicSource != null)
                    musicSource.mute = value;
            }
        }

        private bool _soundMute = false;
        public bool soundMute
        {
            get { return _soundMute; }
            set
            {
                _soundMute = value;
                if (soundSource != null)
                    soundSource.mute = value;
            }
        }

        private void Awake()
        {
            // singleton propre
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (musicSource == null || soundSource == null)
            {
                Debug.LogError("SoundManager: musicSource ou soundSource non assigné.");
                return;
            }

            musicSource.playOnAwake = false;
            soundSource.playOnAwake = false;
            musicSource.loop = true;
            soundSource.loop = false;

            try
            {
                if (PlayerPrefs.HasKey("music_mute"))
                    _musicMute = (PlayerPrefs.GetInt("music_mute") == 1);

                if (PlayerPrefs.HasKey("sound_mute"))
                    _soundMute = (PlayerPrefs.GetInt("sound_mute") == 1);
            }
            catch (System.Exception)
            {
            }

            musicSource.mute = _musicMute;
            soundSource.mute = _soundMute;
        }

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogWarning("SoundManager: clip de musique null.");
                return;
            }

            if (musicSource == null)
            {
                Debug.LogError("SoundManager: musicSource null.");
                return;
            }

            musicSource.clip = clip;
            musicSource.time = 0f;
            musicSource.Play();

            Debug.Log("SoundManager: lecture musique " + clip.name);
        }

        public void PlaySound(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogWarning("SoundManager: clip de son null.");
                return;
            }

            if (soundSource == null)
            {
                Debug.LogError("SoundManager: soundSource null.");
                return;
            }

            soundSource.PlayOneShot(clip);

            Debug.Log("SoundManager: lecture son " + clip.name);
        }

        public void StopMusic()
        {
            if (musicSource == null)
                return;

            musicSource.Stop();
            musicSource.clip = null;
        }

        public void StopAllSounds()
        {
            if (musicSource != null)
                musicSource.Stop();

            if (soundSource != null)
                soundSource.Stop();
        }
    }
}