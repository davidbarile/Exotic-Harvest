using System;
using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class UiTickTextToValue : MonoBehaviour
{
    public Action<int> OnTickComplete;

    [SerializeField] private float maxCounterTickDuration = 1;

    private TMP_Text textComponent;

    private void Awake()
    {
        this.textComponent = GetComponent<TMP_Text>();
    }

    public void AddValue(int inAmountToAdd)
    {
        int previousValue = this.textComponent.text == "" ? 0 : int.Parse(this.textComponent.text, System.Globalization.NumberStyles.AllowThousands);

        if (inAmountToAdd > 0)
        {
            if (this.gameObject.activeInHierarchy)
            {
                if (inAmountToAdd > 1)
                {
                    StartCoroutine(TickToValue(previousValue, previousValue + inAmountToAdd, this.textComponent));
                }
                else
                {
                    if (AudioManager.IN.IncrementCounterClip != null)
                        AudioManager.IN.PlayClip(AudioManager.IN.IncrementCounterClip, .3f);

                    this.textComponent.text = (previousValue + inAmountToAdd).ToString("N0");
                }
            }
            else
            {
                this.textComponent.text = (previousValue + inAmountToAdd).ToString("N0");
            }
        }
    }
    
    public void SetValue(int inNewValue)
    {
        int previousValue = this.textComponent.text == "" ? 0 : int.Parse(this.textComponent.text, System.Globalization.NumberStyles.AllowThousands);

        if (inNewValue > previousValue)
        {
            AddValue(inNewValue - previousValue);
        }
        else
        {
            this.textComponent.text = inNewValue.ToString("N0");
        }
    }

    private IEnumerator TickToValue(int inInitValue, int inDestinationValue, TMP_Text inTextLabel)
    {
        float deltaValue = Mathf.Abs(inDestinationValue - inInitValue);

        float valueTickDuration = Mathf.Min(this.maxCounterTickDuration, deltaValue / 500);
        valueTickDuration = Mathf.Max(valueTickDuration, .25f);//min time 1/4 second

        float endTime = Time.time + valueTickDuration;

        float tickSpeed = .03f;//lower is faster

        float changingValue = inInitValue;

        int counter = 0;

        float subDivision = valueTickDuration / tickSpeed;

        float incrementAmount = deltaValue / subDivision / .66f;//don't know why the magic number here

        while (Time.time < endTime)
        {
            if (counter % 2 == 0)
            {
                float pitch = 1f + (counter * .1f);
                //Debug.Log(counter + "   pitch = " + pitch);

                if (AudioManager.IN.IncrementCounterClip != null)
                    AudioManager.IN.PlayClip(AudioManager.IN.IncrementCounterClip, .3f, pitch);
            }

            ++counter;

            changingValue += incrementAmount;

            changingValue = Mathf.Min(changingValue, inDestinationValue);

            inTextLabel.text = changingValue.ToString("N0");

            yield return new WaitForSeconds(tickSpeed);
        }

        inTextLabel.text = inDestinationValue.ToString("N0");

        OnTickComplete?.Invoke(inDestinationValue);

        yield break;
    }
}