using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

public class LightningBolt : MonoBehaviour
{
    private static WaitForSecondsRealtime _waitForSecondsRealtime_1 = new(.01f);
    [SerializeField] private List<LightningNode> lightningNodes = new();

    [SerializeField] private WeightedRandom boltXPosMinMax;

    [SerializeField] private WeightedRandom nodeLengthMinMax;
    [SerializeField] private WeightedRandom nodeAngleMinMax;
    [SerializeField] private WeightedRandom nodeBranchesMinMax;
    [SerializeField] private WeightedRandom showDurationMinMax;
    [SerializeField] private int cyclesBetweenBranches = 3;
    [SerializeField] private int maxBranches = 3;
    [SerializeField] private float cutoffYPos = 1080f;
    [SerializeField] private Vector3 worldCameraOffset;

    private List<LightningNode> activeNodes = new();

    [Button(ButtonSizes.Large)]
    public void Strike()
    {
        Generate(false);
        Play();
    }

    [Button(ButtonSizes.Large)]
    private void Generate()
    {
        Generate(true);
    }

    private void Generate(bool inShouldShow)
    {
        Reset();

        var xPos = this.boltXPosMinMax.GetWeightedRandomQuantity() * 10;
        xPos -= this.boltXPosMinMax.MaxQuantity * 5;

        if (Application.isPlaying)
        {
            this.transform.position = new Vector3(UiManager.IN.WorldCamera.transform.position.x, 0, 0);
            this.transform.localPosition += this.worldCameraOffset;
            this.transform.localPosition += new Vector3(xPos, 540, 0);
        }
        else
            this.transform.localPosition = new Vector3(xPos, 540, 0);

        LightningNode node = null;
        var counter = 0;
        int numBranches = 1;
        int totalBranchCounter = 0;
        int cyclesUntilNextBranch = 0;
        int length = 0;
        float angle = 0;
        Transform parent = null;
        bool isNodeOnscreen = true;

        for(var i = 0; i < this.lightningNodes.Count; ++i)
        {
            node = this.lightningNodes[i];

            length = this.nodeLengthMinMax.GetWeightedRandomQuantity();

            angle = this.nodeAngleMinMax.GetWeightedRandomQuantity();
            angle -= this.nodeAngleMinMax.MaxQuantity * .5f;
            angle *= 20f;
            if ((angle < 180f && angle > 50f) || (angle > 180f && angle > 310f))
                angle = 0f;

            parent = this.transform;

            if (this.activeNodes.Count > 0)
            {
                parent = this.activeNodes[counter].ChildAttachPoint;

                if (cyclesUntilNextBranch == 0 && totalBranchCounter <= this.maxBranches)
                    numBranches = this.nodeBranchesMinMax.GetWeightedRandomQuantity();

                if (numBranches == 1)
                {
                    if (isNodeOnscreen)
                        ++counter;
                        
                    --cyclesUntilNextBranch;
                }
                else
                {
                    cyclesUntilNextBranch = numBranches == 3 ? this.cyclesBetweenBranches + 2 : this.cyclesBetweenBranches;
                    numBranches = 1;
                    ++totalBranchCounter;
                }
            }

            if (isNodeOnscreen)
            {
                node.Configure(length, angle, parent, numBranches, inShouldShow);
                this.activeNodes.Add(node);
            }

            isNodeOnscreen = node.transform.position.y > this.cutoffYPos;

            if (!isNodeOnscreen)
                break;
        }
    }

    private void Play()
    {
        StartCoroutine(PlayStrikeCo());
    }
    
    private IEnumerator PlayStrikeCo()
    {
        for (int i = 0; i < this.activeNodes.Count; ++i)
        {
            var node = this.activeNodes[i];

            if (i % 3 == 0)
                yield return _waitForSecondsRealtime_1;

            node.gameObject.SetActive(true);
        }

        var showDuration = this.showDurationMinMax.GetWeightedRandomQuantity();
        yield return new WaitForSecondsRealtime(showDuration * .01f);

        Reset();
    }
    
    [Button(ButtonSizes.Large)]
    public void Reset()
    {
        StopAllCoroutines();

        this.activeNodes.Clear();

        foreach(var node in this.lightningNodes)
        {
            node.transform.parent = this.transform;
            node.Reset();
            node.transform.SetAsLastSibling();
        }
    }
}