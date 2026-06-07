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

    private List<LightningNode> nodes = new();

    [Button(ButtonSizes.Large)]
    public void Generate()
    {
        var xPos = this.boltXPosMinMax.GetWeightedRandomQuantity() * 10;
        xPos -= this.boltXPosMinMax.MaxQuantity * 5;

        this.transform.localPosition = new Vector3(xPos, 540, 0);

        this.nodes.Clear();

        var tierNodes = new List<LightningNode>();

        for(var i = 0; i < this.lightningNodes.Count; ++i)
        {
            var node = this.lightningNodes[i];

            var length = this.nodeLengthMinMax.GetWeightedRandomQuantity();

            float angle = this.nodeAngleMinMax.GetWeightedRandomQuantity();
            angle -= this.nodeAngleMinMax.MaxQuantity * .5f;
            angle *= 20f;

            var parent = this.transform;

            if (tierNodes.Count > 0)
                parent = tierNodes[tierNodes.Count - 1].ChildAttachPoint;

            node.Configure(length, angle, parent);
            tierNodes.Add(node);
        }
        
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
            node.transform.parent = this.transform;
            node.Reset();
        }
    }
}