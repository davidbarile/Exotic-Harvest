using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager IN; 

    public static bool IsDragModeActivated = false;

    public static Action<bool> OnDragModeChanged;

    [SerializeField] private CanvasGroup bgCanvasGroup;

    private bool isBgShowing;

    private void Awake()
    {
        if (IN == null)
        {
            IN = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public TMP_Text DebugText;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleQuitButtonClick();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isBgShowing)
            {
                StartCoroutine(FadeOutBackground());
            }
            else
            {
                StartCoroutine(FadeInBackground());
            }
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            IsDragModeActivated = !IsDragModeActivated;
            OnDragModeChanged?.Invoke(IsDragModeActivated);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (DebugText != null)
            {
                DebugText.text = $"Frame Count: {Time.frameCount}";
            }
        }
    }

    private IEnumerator FadeInBackground()
    {
        isBgShowing = true;
        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bgCanvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        bgCanvasGroup.alpha = 1f;
        bgCanvasGroup.interactable = true;
        bgCanvasGroup.blocksRaycasts = true;
    }

    private IEnumerator FadeOutBackground()
    {
        isBgShowing = false;
        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bgCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        bgCanvasGroup.alpha = 0f;
         bgCanvasGroup.interactable = false;
        bgCanvasGroup.blocksRaycasts = false;
    }

    public void HandleQuitButtonClick()
    {
        Application.Quit();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if(DebugText != null)
            DebugText.text = "App Focus: " + hasFocus;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if(DebugText != null)
            DebugText.text = "App Paused: " + pauseStatus;
    }
}