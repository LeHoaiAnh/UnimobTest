using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Hara.GUI
{
    public class GUIDelegateAttribute : System.Attribute { }

    public class GUIManager : MonoBehaviour
    {
        public static GUIManager Instance {
            get {
                if (instance == null)
                {
                    instance = FindObjectOfType<GUIManager>();
                }
                return instance;
            }
        }
        static GUIManager instance = null;

        [Header ("Set up for Screen")]
        public Transform screenContainer;
        public Canvas canvasScreen;
        public RectTransform canvasScreenRect;
        public GraphicRaycaster graphicRaycasterGroupCard;

        [Header("Set up for top group")]
        public TopGroup topGroup;
        
        [Header ("Set up for Popup")]
        public GraphicRaycaster graphicRaycasterPopupContainer;
        
        [Header( "Set up For General")]
        public EventSystem m_EventSystem;
        public string LastScreen { get; set; }
        public string CurrentScreen { get; set; }
        [System.NonSerialized]
        public Dictionary<string, ScreenBase> screens = new Dictionary<string, ScreenBase>();

        ScreenBase LoadScreen(string screen_name)
        {
            GameObject go = null;

            if (go == null)
            {
                go = Instantiate(Resources.Load<GameObject>("Screens/Screen" + screen_name), screenContainer);
            }

            if (go == null)
            {
                return null;
            }

            go.transform.localScale = Vector3.one;
            //go.SetActive(false);

            ScreenBase screen_base = go.GetComponent<ScreenBase>();

            if (screen_base == null)
            {
                return null;
            }

            if (screens.ContainsKey(screen_name))
            {
                screens[screen_name] = screen_base;
            }
            else
            {
                screens.Add(screen_name, screen_base);
            }

            return screen_base;
        }

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                GameObject.Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(this.gameObject);
            soundCollections = Sound.Instance;
            LoadSoundAndMusicSetting();
            ApplySoundAndMusicSetting();
        }
        
        void OnGoToBattle()
        {
            AudioListener camera_listener = Camera.main.GetComponent<AudioListener>();
            if (camera_listener != null)
                camera_listener.enabled = false;

            AudioListener.volume = SoundEnable ? 1f : 0f;
            topGroup.gameObject.SetActive(false);
        }

        void OnGoToMainScreen()
        {
            topGroup.gameObject.SetActive(true);
        }

        public void ClearScreen()
        {
            screenContainer.DestroyChildren();
            this.screens.Clear();
        }

        public ScreenBase SetScreen(string screen_name, System.Action onScreenActive = null)
        {
            
            if (screen_name.Equals("Main"))
            {
                StartFadeInMusic();
            }
            else
            {
                StartFadeOutMusic();
            }
#if BUILD_HORIZONTAL
            if (screen_name == "BattleHorizontal")
            {
                OnGoToBattle();
            }
#else
            if (screen_name == "Battle")
            {
                OnGoToBattle();
            }
#endif


            if (screen_name == "Main")
            {
                OnGoToMainScreen();
            }
            
            if (screens.ContainsKey(screen_name) == false)
            {
                ScreenBase screen = LoadScreen(screen_name);
            }

            if (CurrentScreen == screen_name)
            {
                var screen = this.screens[CurrentScreen];
                screen.OnActive();
                if (onScreenActive != null) onScreenActive();

                return screen;
            }

            Debug.Log("SetScreen :" + screen_name);
            LastScreen = CurrentScreen;

            ScreenBase curScreen = null;

            if (string.IsNullOrEmpty(CurrentScreen) == false &&
                screens.ContainsKey(CurrentScreen))
            {
                curScreen = screens[CurrentScreen];
                curScreen.OnDeactive();
                curScreen.gameObject.SetActive(false);
            }

            curScreen = screens[screen_name];
            CurrentScreen = screen_name;
            if (!curScreen.gameObject.activeSelf)
                curScreen.gameObject.SetActive(true);

            curScreen.OnActive();

            if (onScreenActive != null) onScreenActive();

            return curScreen;
        }

        public void RestartGame()
        {

        }

        private void Start()
        {
            NGUITools.audioSource = sfxSource;
        }

        public AudioSource musicSource;
        public AudioSource sfxSource;
        public bool MusicEnable = true;
        public bool SoundEnable = true;

        public void LoadSoundAndMusicSetting()
        {
            MusicEnable = (PlayerPrefs.GetInt("MusicEnable", 1) == 1);
            SoundEnable = (PlayerPrefs.GetInt("SoundEnable", 1) == 1);
        }

        public void SaveSoundAndMusicSetting()
        {
            if (MusicEnable)
                PlayerPrefs.SetInt("MusicEnable", 1);
            else
                PlayerPrefs.SetInt("MusicEnable", 0);

            if (SoundEnable)
                PlayerPrefs.SetInt("SoundEnable", 1);
            else
                PlayerPrefs.SetInt("SoundEnable", 0);

        }

        public void MuteAudio()
        {
            AudioListener.volume = 0;
        }

        public void UnmuteAudio()
        {
            AudioListener.volume = SoundEnable ? 1f : 0f;
        }

        public void ApplySoundAndMusicSetting()
        {
            if (MusicEnable == false)
            {
                musicSource.Stop();
                musicSource.volume = 0;
            }
            else if (GUIManager.instance.CurrentScreen == "Main")
            {
                StartFadeInMusic();
            }

            AudioListener.volume = SoundEnable ? 1f : 0f;
        }

        public void StartFadeInMusic()
        {
            if (MusicEnable)
            {
                musicSource.volume = 0f;
                this.StopCoroutine("FadeOut");
                this.StopCoroutine("FadeIn");
                this.StartCoroutine("FadeIn", 2f);
            }
        }
        public void StartFadeOutMusic()
        {
            if (MusicEnable)
            {
                this.StopCoroutine("FadeOut");
                this.StopCoroutine("FadeIn");
                this.StartCoroutine("FadeOut", 1f);
            }
        }

        public void UpdateTopGroup()
        {
            topGroup.UpdateTopGroup();
        }

        public IEnumerator FadeOut(float FadeTime)
        {
            AudioSource audioSource = musicSource;

            float startVolume = audioSource.volume;
            float downVolumnInFrame = startVolume * Time.deltaTime / FadeTime;
            while (audioSource.volume > 0)
            {
                audioSource.volume -= downVolumnInFrame;
                yield return null;
            }
            audioSource.Stop();
            audioSource.volume = 0;
            //Destroy(audioSource);
        }

        public IEnumerator FadeIn(float FadeTime)
        {
            float maxVolumn = 0.6f;
            AudioSource audioSource = musicSource;

            float upVolumnInFrame = maxVolumn * Time.deltaTime / FadeTime;

            if (audioSource.isPlaying == false)
                audioSource.Play();

            while (audioSource.volume < maxVolumn)
            {
                audioSource.volume += upVolumnInFrame;
                yield return null;
            }
            audioSource.volume = maxVolumn;
        }

        public void PlaySound(string path, Vector3 pos, float vol)
        {
            AudioClip clip = Resources.Load(path) as AudioClip;

            if (clip != null)
            {
                NGUITools.PlaySound(clip);
            }
        }

        private Sound soundCollections;

        public void PlaySound(string clipName)
        {
            AudioClip clip = soundCollections.GetClip(clipName);
            if (clip != null)
            {
                NGUITools.PlaySound(clip);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning(clipName + "  clip is null");
#endif
            }
        }
    }

}

