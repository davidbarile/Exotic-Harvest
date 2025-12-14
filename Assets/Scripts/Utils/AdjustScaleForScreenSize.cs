using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustScaleForScreenSize : MonoBehaviour
{
    public enum EScreenDimension
    {
        Width,
        Height,
        AspectRatio
    }

    private Vector3 initScale = Vector3.one;

    [SerializeField] private float              scaleOffset = 1;//this is a hack... should be able to calculate this internally
    [SerializeField] private Vector2            defaultScreenDims;
    [SerializeField] private Vector3            maxScale;
    [SerializeField] private EScreenDimension    useDimension;

    [Header("Debug")]
    [SerializeField] private float              multiplier = 1;
    [SerializeField] private Vector3            adjustedScale;
    

    private void Start()
    {
        this.initScale = this.transform.localScale;

        AdjustSize();
    }

    private void AdjustSize()
    {
        if (this.useDimension == EScreenDimension.Width)
            this.multiplier = (float)Screen.width / (float)this.defaultScreenDims[0];
        else if( this.useDimension == EScreenDimension.Height)
            this.multiplier = Screen.height / (float) this.defaultScreenDims[1];
        else
            this.multiplier = this.scaleOffset * (   ((float)this.defaultScreenDims[0]/(float)this.defaultScreenDims[1]) /  ((float)Screen.height / (float)Screen.width)   );

        //((float)Screen.width / (float)defaultScreenDims[0])

        float newWidth = Mathf.Round(this.initScale[0] * this.multiplier * 10000f) / 10000f;
        float newHeight = Mathf.Round(this.initScale[1] * this.multiplier * 10000f) / 10000f;

        newWidth = Mathf.Min(this.maxScale.x, newWidth);
        newHeight = Mathf.Min(this.maxScale.y, newHeight);

        this.adjustedScale = new Vector3(newWidth, newHeight, 1);

        this.transform.localScale = this.adjustedScale;
    }

    #if UNITY_EDITOR
    private void Update()
    {
       AdjustSize();
    }
    #endif
}
