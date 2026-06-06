using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class LightningBolt : MonoBehaviour
{
    [SerializeField] private List<LightningNode> lightningNodes = new();

    [SerializeField] private WeightedRandom boltXPosMinMax;

    [SerializeField] private WeightedRandom nodeLengthMinMax;
    [SerializeField] private WeightedRandom nodeAngleMinMax;
    [SerializeField] private WeightedRandom nodeCountMinMax;

    [Button(ButtonSizes.Large)]
    public void Generate()
    {
        var xPos = this.boltXPosMinMax.GetWeightedRandomQuantity();
        this.transform.localPosition = new Vector3(xPos, 0, 0);
        
        //place nodes, decide if split, rotate, length
    }

    public void Play()
    {

    }
    
    [Button(ButtonSizes.Large)]
    public void Reset()
    {
        foreach(var node in this.lightningNodes)
        {
            node.Reset();
        }
    }
}