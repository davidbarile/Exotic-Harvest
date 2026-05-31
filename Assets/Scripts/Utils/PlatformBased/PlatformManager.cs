using System;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif


[Flags]
public enum PlatformFlags
{
    None = 0,
    Desktop = 1 << 1,
    Android = 1 << 2,
    iOS = 1 << 3,
    Mobile = 1 << 4,
    IsTablet = 1 << 5,
    IsPhone = 1 << 6,
    IsUnityEditor = 1 << 7
}

public class PlatformManager : MonoBehaviour
{
    public static PlatformManager IN;

    public static bool IsDesktop => IN.PlatformFlags.HasFlag(PlatformFlags.Desktop);
    public static bool IsMobile => IN.PlatformFlags.HasFlag(PlatformFlags.Mobile);
#if UNITY_EDITOR
    public static bool IsPhone => IN.PlatformFlags.HasFlag(PlatformFlags.IsPhone);
#else
    public static bool IsPhone => IN.PlatformFlags.HasFlag(PlatformFlags.IsPhone) || (IN.PlatformFlags.HasFlag(PlatformFlags.Mobile) && !IsMostLikelyTablet());
#endif
    public static bool IsTablet => IN.PlatformFlags.HasFlag(PlatformFlags.IsTablet);
    public static bool IsTouchInput => IN?.PlatformFlags.HasFlag(PlatformFlags.Mobile) == true;

    [Tooltip("Note: If Selecting IsTablet, be sure to also select Mobile (or Desktop) as well")]
    [SerializeField] private PlatformFlags _platformOverride;

    private PlatformFlags _platformFlags;

    public PlatformFlags PlatformFlags => _platformFlags;

    protected void Awake()
    {
#if UNITY_ANDROID
        _platformFlags |= PlatformFlags.Android;
#elif UNITY_IOS
        _platformFlags |= PlatformFlags.iOS;
#else
        _platformFlags |= PlatformFlags.Desktop;
#endif

#if UNITY_EDITOR
        if (_platformOverride != PlatformFlags.None)
        {
            _platformFlags = _platformOverride;
            return;
        }
        else
        {
            _platformFlags |= PlatformFlags.IsUnityEditor;
        }
#endif // UNITY_EDITOR

        if ((_platformFlags & (PlatformFlags.Desktop)) != PlatformFlags.None)
        {
            _platformFlags |= PlatformFlags.Desktop;
        }

        if ((_platformFlags & (PlatformFlags.Android | PlatformFlags.iOS)) != PlatformFlags.None)
        {
            _platformFlags |= PlatformFlags.Mobile;

            if (IsMostLikelyTablet())
            {
                _platformFlags |= PlatformFlags.IsTablet;
            }
            else
            {
                _platformFlags |= PlatformFlags.IsPhone;
            }
        }
    }

    private static bool IsMostLikelyTablet()
    {
#if UNITY_IOS && !UNITY_EDITOR
// Strong signal: iPad is always a tablet form factor for our purposes.
if (Device.generation.ToString().Contains("iPad"))
    return true;
#endif

        var dpi = Screen.dpi;

        float widthPx  = Screen.width;
        float heightPx = Screen.height;

        var longSide  = Mathf.Max(widthPx, heightPx);
        var shortSide = Mathf.Min(widthPx, heightPx);

        if (shortSide <= 0f) //Guard against potential divide by zero or uninitialized resolution default to phone
            return false;

        var aspect = longSide / shortSide;

        if (!(dpi > 0f) || !(dpi < 1000f))
            return shortSide >= 900 && aspect < 2.0f; // Fallback heuristic when DPI is unknown/unreliable

        var wIn = widthPx / dpi;
        var hIn = heightPx / dpi;
        var diagIn = Mathf.Sqrt(wIn * wIn + hIn * hIn);

        return diagIn >= 6.8f; //or 7.0f (tune as needed)
    }

    public bool Matches(PlatformFlags platformFlags, PlatformFlags platformFlagsToExclude = PlatformFlags.None)
    {
        return (_platformFlags & platformFlags) != PlatformFlags.None && (_platformFlags & platformFlagsToExclude) == PlatformFlags.None;
    }
}