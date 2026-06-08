using UnityEditor.EditorTools;
using UnityEngine;

public class PlatformBasedDisabler : MonoBehaviour
{
    private enum ECallType
    {
        Awake,
        Start,
        OnEnable,
        Update
    }

    private enum EHideType
    {
        Hide,
        Destroy,
        DestroyImmediate
    }

    [SerializeField] private PlatformFlags _enableOnPlatforms;

    [SerializeField] private EHideType _hideType = EHideType.Destroy;

    [SerializeField] private ECallType _callType = ECallType.Start;

    [Space, SerializeField] private bool _reverseLogic;

    [Tooltip("Check this to work with PlatformFlags.UnityEditor")]
    [Space, SerializeField] private bool _showInEditor;

    private bool _isComplete;

    private void Awake()
    {
        if (_callType == ECallType.Awake)
            Execute();
    }

    private void Start()
    {
        if (_callType == ECallType.Start)
            Execute();
    }

    private void OnEnable()
    {
        if (_callType == ECallType.OnEnable)
            Execute();
    }

    private void Update()
    {
        if(_isComplete) return;

        if (_callType == ECallType.Update)
            Execute();
    }

    private void Execute()
    {
        if(PlatformManager.IN == null)
        {
            Debug.LogError($"PlatformManager is not initialized!  FeaturePlatformFlagsGameObjectEnabler on {name}.  Frame = {Time.frameCount}", gameObject);
            return;
        }

        var shouldShow = PlatformManager.IN.Matches(_enableOnPlatforms);

        if (_showInEditor && PlatformManager.IN.PlatformFlags.HasFlag(PlatformFlags.IsUnityEditor))
            shouldShow = true;

        if (_reverseLogic)
            shouldShow = !shouldShow;
            
        if(!shouldShow)
        {
            switch (_hideType)
            {
                case EHideType.Hide:
                    gameObject.SetActive(false);
                    break;
                case EHideType.Destroy:
                    Destroy(gameObject);
                    break;
                case EHideType.DestroyImmediate:
                    DestroyImmediate(gameObject);
                    break;
            }
        }

        _isComplete = true;
    }
}