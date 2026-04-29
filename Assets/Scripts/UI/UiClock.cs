using UnityEngine;
using TMPro;

public class UiClock : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private Transform hourHand;
    [SerializeField] private bool doValidate;

    private void OnValidate()
    {
        var splitName = this.name.Split("_");

        if (splitName.Length < 2)
            return;
        
        var hour = splitName[1];
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
