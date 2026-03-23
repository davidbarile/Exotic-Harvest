using UnityEngine;

public class UiCompass : MonoBehaviour
{
    [SerializeField] private Transform needle;

    [SerializeField, Range(-180f, 180f)] private float offsetAngle = 0f;

    public void SetDirection(float angle)
    {
        this.needle.localRotation = Quaternion.Euler(0, 0, (angle * 360f) + this.offsetAngle);
    }
}