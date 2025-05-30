//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.SceneManagement;
//using BEKStudio;

//public class SettingsManager : MonoBehaviour
//{
//    public static SettingsManager Instance;  // Singleton Instance

//    [Header("UI Elements")]
//    public Toggle musicToggle;
//    public Toggle sfxToggle;
//    public Button saveButton;

//    private void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//        }
//    }

//    private void Start()
//    {
//        LoadSettings(); // Load saved settings at startup

//        if (musicToggle != null) musicToggle.onValueChanged.AddListener(delegate { OnSettingsChanged(); });
//        if (sfxToggle != null) sfxToggle.onValueChanged.AddListener(delegate { OnSettingsChanged(); });
//        if (saveButton != null) saveButton.onClick.AddListener(SaveSettings);
//    }

//    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//    {
//        ApplySettings(); // Apply settings in every new scene
//    }

//    void OnSettingsChanged()
//    {
//        if (saveButton != null) saveButton.gameObject.SetActive(true);
//    }

//    public void SaveSettings()
//    {
//        PlayerPrefs.SetInt("MusicEnabled", musicToggle != null && musicToggle.isOn ? 1 : 0);
//        PlayerPrefs.SetInt("SFXEnabled", sfxToggle != null && sfxToggle.isOn ? 1 : 0);
//        PlayerPrefs.Save();
//        ApplySettings();
//        if (saveButton != null) saveButton.gameObject.SetActive(false);
//    }

//    void LoadSettings()
//    {
//        bool isMusicOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
//        bool isSfxOn = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;

//        if (musicToggle != null) musicToggle.isOn = isMusicOn;
//        if (sfxToggle != null) sfxToggle.isOn = isSfxOn;

//        ApplySettings();
//    }

//    void ApplySettings()
//    {
//        if (AudioController.Instance != null)
//        {
//            AudioController.Instance.SetMusicVolume(PlayerPrefs.GetInt("MusicEnabled", 1) == 1 ? 1f : 0f);
//            AudioController.Instance.SetSFXVolume(PlayerPrefs.GetInt("SFXEnabled", 1) == 1 ? 1f : 0f);
//        }
//        else {
//            Debug.Log("Not working");
//        }
//    }

//    private void OnDestroy()
//    {
//        SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe event
//    }
//}
