using UnityEngine;
using TMPro;

public class UiClock : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private Transform hourHand;
    [SerializeField] private bool doValidate;

    private void OnValidate() 
    {
        var hour = this.name.Split("_")[1];
        if (int.TryParse(hour, out var hourInt))
        {
            var rotation = Quaternion.Euler(0f, 0f, -hourInt * 30f);
            this.hourHand.localRotation = rotation;

            var amPm = hourInt >= 12 ? "PM" : "AM";

            hourInt = hourInt > 12 ? (hourInt - 12) : hourInt;

            if(hourInt == 0)
                hourInt = 12;

            this.timeText.text = $"{hourInt}:00 <size=80%>{amPm}</size>";
        }
    }
}
