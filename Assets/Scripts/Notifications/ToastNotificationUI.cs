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
        this.rectTransform = GetComponent<RectTransform>();
        this.canvasGroup = GetComponent<CanvasGroup>();
        
        if (this.canvasGroup == null)
        {
            this.canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        if (this.dismissButton != null)
        {
            this.dismissButton.onClick.AddListener(Dismiss);
        }
    }
    
    public void Initialize(ToastNotification notification, System.Action<ToastNotificationUI> onDismissCallback)
    {
        this.notificationData = notification;
        this.onDismissed = onDismissCallback;
        
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
        if (this.backgroundImage != null)
        {
            this.backgroundImage.color = this.notificationData.backgroundColor;
        }
        
        // Set title
        if (this.titleText != null)
        {
            this.titleText.text = this.notificationData.title;
            this.titleText.color = this.notificationData.textColor;
            this.titleText.gameObject.SetActive(!string.IsNullOrEmpty(this.notificationData.title));
        }
        
        // Set message
        if (this.messageText != null)
        {
            this.messageText.text = this.notificationData.message;
            this.messageText.color = this.notificationData.textColor;
        }
        
        // Set icon
        if (this.iconImage != null)
        {
            if (this.notificationData.icon != null)
            {
                this.iconImage.sprite = this.notificationData.icon;
                this.iconImage.gameObject.SetActive(true);
            }
            else
            {
                this.iconImage.gameObject.SetActive(false);
            }
        }
        
        // Set dismiss button visibility
        if (this.dismissButton != null)
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
        
        showSequence.Append(this.animRoot.DOAnchorPos(Vector2.zero, this.slideInDuration)
            .SetEase(this.slideInEase));
        
        this.animationTween = showSequence;
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

        if (this.animationTween != null)
        {
            this.animationTween.Kill();
        }
        
        if(isImmediate)
        {
            this.onDismissed?.Invoke(this);
            Destroy(gameObject);
            return;
        }
        
        // Start hide animation
        Sequence hideSequence = DOTween.Sequence();
        
        hideSequence.Append(this.animRoot.DOAnchorPos(this.animRoot.anchoredPosition + new Vector2(this.animRoot.rect.width * .5f, 0f), this.slideOutDuration)
            .SetEase(this.slideOutEase));

        hideSequence.OnComplete(() => {
            this.onDismissed?.Invoke(this);
            Destroy(gameObject);
        });
        
        this.animationTween = hideSequence;
    }
    
    private void OnDestroy()
    {
        if (this.animationTween != null)
        {
            this.animationTween.Kill();
        }
    }
    
    // Allow clicking anywhere on the notification to dismiss (optional)
    public void OnPointerClick()
    {
        if (!this.notificationData.autoDismiss)
        {
            Dismiss();
        }
    }
}