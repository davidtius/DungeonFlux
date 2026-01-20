using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject tutorialOverlay;
    public List<GameObject> pages;

    [Header("Buttons")]
    public Button btnNext;
    public Button btnPrev;
    public TextMeshProUGUI btnNextText;

    [Header("Settings")]
    public string gameplaySceneName = "GamePlay";

    [Header("Audio Settings")]
    public AudioSource sfxSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private int currentPageIndex = 0;
    private bool isNewGameRun = false;

    void Start()
    {
        tutorialOverlay.SetActive(false);
    }

    public void StartTutorialSequence()
    {
        isNewGameRun = true;
        OpenTutorial();
    }

    public void OpenHelpOnly()
    {
        isNewGameRun = false;
        OpenTutorial();
    }

    void OpenTutorial()
    {
        PlayClickSFX();
        tutorialOverlay.SetActive(true);
        currentPageIndex = 0;
        UpdatePage();
    }

    public void NextPage()
    {
        PlayClickSFX();
        if (currentPageIndex < pages.Count - 1)
        {
            currentPageIndex++;
            UpdatePage();
        }
        else
        {

            FinishTutorial();
        }
    }

    public void PrevPage()
    {
        PlayClickSFX();
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdatePage();
        }
    }

    public void FinishTutorial()
    {
        PlayClickSFX();

        PlayerPrefs.SetInt("HasSeenTutorial", 1);
        PlayerPrefs.Save();

        tutorialOverlay.SetActive(false);

        if (isNewGameRun)
        {
            LevelLoader.LoadLevel("Sanctuary");
        }
    }

    public void SkipTutorial()
    {
        PlayClickSFX();
        FinishTutorial();
    }

    void UpdatePage()
    {

        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].SetActive(i == currentPageIndex);
        }

        if (btnPrev) btnPrev.interactable = (currentPageIndex > 0);

        if (btnNextText)
        {
            if (currentPageIndex == pages.Count - 1)
                btnNextText.text = isNewGameRun ? "Start" : "Close";
            else
                btnNextText.text = "Next";
        }
    }

    public void PlayClickSFX()
    {
        if (sfxSource != null && clickSound != null) sfxSource.PlayOneShot(clickSound);
    }

    public void PlayHoverSFX()
    {
        if (sfxSource != null && hoverSound != null) sfxSource.PlayOneShot(hoverSound);
    }
}
