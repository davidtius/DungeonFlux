using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Audio;

public class MainMenuController : MonoBehaviour
{
    [Header("Tutorial")]
    public TutorialManager tutorialManager;
    public GameObject gameTitle;

    [Header("Scene Settings")]
    public string gameplaySceneName = "Gameplay";

    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject playPanel;
    public GameObject controlsPanel;
    public GameObject optionsPanel;

    [Header("UI Sliders")]
    public Slider sliderMaster;
    public Slider sliderMusic;
    public Slider sliderSFX;
    public Slider sliderBrightness;

    [Header("UI Buttons")]
    public Button continueButton;

    [Header("Brightness Settings")]
    public Image brightnessOverlay;

    [Header("Audio Settings")]
    public AudioMixer mainMixer;
    public AudioSource sfxSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private bool isSceneLoading = false;

    void Start()
    {

        float volMaster = PlayerPrefs.GetFloat("MasterVol", 1f);
        float volMusic = PlayerPrefs.GetFloat("MusicVol", 1f);
        float volSFX = PlayerPrefs.GetFloat("SFXVol", 1f);

        SetMasterVolume(volMaster);
        if (sliderMaster != null) sliderMaster.value = volMaster;

        SetMusicVolume(volMusic);
        SetSFXVolume(volSFX);

        float brightVal = PlayerPrefs.GetFloat("Brightness", 1f);
        SetBrightness(brightVal);
        if (sliderBrightness != null) sliderBrightness.value = brightVal;
    }

    public void PlayGame()
    {

        LevelLoader.LoadLevel(gameplaySceneName);

    }

    public void OpenPlayMenu()
    {
        PlayClickSFX();
        mainMenuPanel.SetActive(false);
        playPanel.SetActive(true);

        bool hasSave = SaveManager.HasSaveFile();

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(hasSave);
        }
    }

    public void ClosePlayMenu()
    {
        PlayClickSFX();
        playPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void ContinueGame()
    {
        if (SaveManager.HasSaveFile())
        {
            SaveData data = SaveManager.Load();
            string targetScene = data.lastSceneName;

            if (string.IsNullOrEmpty(targetScene)) targetScene = gameplaySceneName;

                LevelLoader.LoadLevel(targetScene);

        }
    }

    public void QuitGame()
    {
        PlayClickSFX();
        Invoke("ExitApplication", 0.5f);
    }

    private void ExitApplication()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void PlayHoverSFX()
    {
        if (sfxSource != null && hoverSound != null)
        {
            sfxSource.PlayOneShot(hoverSound);
        }
    }

    public void PlayClickSFX()
    {
        if (sfxSource != null && clickSound != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }

    IEnumerator LoadSceneWithDelay()
    {
        PlayClickSFX();

        yield return new WaitForSeconds(0.5f);

        Time.timeScale = 1f;
        LevelLoader.LoadLevel(gameplaySceneName);
    }

    IEnumerator LoadSceneWithDelay(string sceneName)
    {
        PlayClickSFX();
        yield return new WaitForSeconds(0.5f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void OpenOptions()
    {
        PlayClickSFX();
        optionsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    public void CloseOptions()
    {
        PlayClickSFX();
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OpenControls()
    {
        PlayClickSFX();
        controlsPanel.SetActive(true);

        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    public void CloseControls()
    {
        PlayClickSFX();
        controlsPanel.SetActive(false);

        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void SetMasterVolume(float volume)
    {

        if (mainMixer != null) mainMixer.SetFloat("MasterVol", Mathf.Log10(volume) * 20);

        PlayerPrefs.SetFloat("MasterVol", volume);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float volume)
    {
        if (mainMixer != null) mainMixer.SetFloat("MusicVol", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVol", volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        if (mainMixer != null) mainMixer.SetFloat("SFXVol", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVol", volume);
        PlayerPrefs.Save();
    }

    public void SetBrightness(float value)
    {
        if (brightnessOverlay != null)
        {
            float alpha = 1f - value;
            Color color = brightnessOverlay.color;
            color.a = alpha;
            brightnessOverlay.color = color;
        }

        PlayerPrefs.SetFloat("Brightness", value);
        PlayerPrefs.Save();
    }

    public void NewGame()
    {
        PlayClickSFX();

        SaveManager.DeleteSave();

        if (PlayerDataTracker.Instance != null)
        {
             Destroy(PlayerDataTracker.Instance.gameObject);
        }
        if (DDAManager.Instance != null)
        {
             Destroy(DDAManager.Instance.gameObject);
        }

        gameTitle.SetActive(false);

            if (tutorialManager != null)
            {
                tutorialManager.StartTutorialSequence();
            }
            else
            {

                LevelLoader.LoadLevel(gameplaySceneName);
            }

            playPanel.SetActive(false);

    }
}