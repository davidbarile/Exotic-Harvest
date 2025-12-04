using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// UI component for displaying individual toast notifications
/// </summary>
public class ToastNotificationUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button dismissButton;
    [SerializeField] private RectTransform animRoot;

    [Header("Animation Settings")]
    [SerializeField] private float slideInDuration = 0.5f;
    [SerializeField] private float slideOutDuration = 0.3f;
    [SerializeField] private Ease slideInEase = Ease.OutBack;
    [SerializeField] private Ease slideOutEase = Ease.InBack;
    
    private ToastNotification notificationData;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Tween animationTween;
    private System.Action<ToastNotificationUI> onDismissed;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        if (dismissButton != null)
        {
            dismissButton.onClick.AddListener(Dismiss);
        }
    }
    
    public void Initialize(ToastNotification notification, System.Action<ToastNotificationUI> onDismissCallback)
    {
        notificationData = notification;
        onDismissed = onDismissCallback;
        
        SetupVisuals();
        StartShowAnimation();
        
        if (notification.autoDismiss)
        {
            Invoke(nameof(AutoDismiss), notification.displayDuration);
        }
    }
    
    private void SetupVisuals()
    {
        // Set background color
        if (backgroundImage != null)
        {
            backgroundImage.color = notificationData.backgroundColor;
        }
        
        // Set title
        if (titleText != null)
        {
            titleText.text = notificationData.title;
            titleText.color = notificationData.textColor;
            titleText.gameObject.SetActive(!string.IsNullOrEmpty(notificationData.title));
        }
        
        // Set message
        if (messageText != null)
        {
            messageText.text = notificationData.message;
            messageText.color = notificationData.textColor;
        }
        
        // Set icon
        if (iconImage != null)
        {
            if (notificationData.icon != null)
            {
                iconImage.sprite = notificationData.icon;
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }
        
        // Set dismiss button visibility
        if (dismissButton != null)
        {
            dismissButton.gameObject.SetActive(!notificationData.autoDismiss);
        }
    }
    
    private void StartShowAnimation()
    {
        // Start off-screen
        Vector2 startPos = animRoot.anchoredPosition + new Vector2(animRoot.rect.width * .5f, 0f);
        animRoot.anchoredPosition = startPos;
        
        // Create animation sequence
        Sequence showSequence = DOTween.Sequence();
        
        showSequence.Append(animRoot.DOAnchorPos(Vector2.zero, slideInDuration)
            .SetEase(slideInEase));
        
        animationTween = showSequence;
    }

    private void AutoDismiss()
    {
        if (gameObject != null && gameObject.activeInHierarchy)
        {
            Dismiss();
        }
    }
    
    public void Dismiss()
    {
        Dismiss(false);
    }
    
    public void Dismiss(bool isImmediate)
    {
        CancelInvoke(nameof(AutoDismiss));

        if (animationTween != null)
        {
            animationTween.Kill();
        }
        
        if(isImmediate)
        {
            onDismissed?.Invoke(this);
            Destroy(gameObject);
            return;
        }
        
        // Start hide animation
        Sequence hideSequence = DOTween.Sequence();
        
        hideSequence.Append(animRoot.DOAnchorPos(animRoot.anchoredPosition + new Vector2(animRoot.rect.width * .5f, 0f), slideOutDuration)
            .SetEase(slideOutEase));

        hideSequence.OnComplete(() => {
            onDismissed?.Invoke(this);
            Destroy(gameObject);
        });
        
        animationTween = hideSequence;
    }
    
    private void OnDestroy()
    {
        if (animationTween != null)
        {
            animationTween.Kill();
        }
    }
    
    // Allow clicking anywhere on the notification to dismiss (optional)
    public void OnPointerClick()
    {
        if (!notificationData.autoDismiss)
        {
            Dismiss();
        }
    }
}