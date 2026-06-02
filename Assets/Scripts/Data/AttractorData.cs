using System;
using UnityEngine;

[Serializable]
public class AttractorData
{
    public bool OnlyAttractWhileDragging;
    public float Strength;
    public float MaxDistance;
    public float CollisionRadius;
    public LayerMask AttractableLayer;
    public string TagToAttract = "Attractable";

    public static AttractorData Copy(AttractorData inAttractorData)
    {
        return new AttractorData
        {
            OnlyAttractWhileDragging = inAttractorData.OnlyAttractWhileDragging,
            Strength = inAttractorData.Strength,
            MaxDistance = inAttractorData.MaxDistance,
            CollisionRadius = inAttractorData.CollisionRadius,
            AttractableLayer = inAttractorData.AttractableLayer,
            TagToAttract = inAttractorData.TagToAttract
        };
    }
}