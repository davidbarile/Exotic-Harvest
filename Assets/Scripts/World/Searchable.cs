using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Searchable : Loot
{
    [Header("Searchable Settings --------------")]
    public string SearchableName;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(this.SearchableName))
        {
            this.SearchableName = gameObject.name;
        }
    }

    public override void Spawn()
    {
        //this.resourceType = EResourceType.Dew;
        //this.collectionType = ECollectionMethod.Click;
        this.amount = 1;
        this.lifetime = 60f;

        //this.canvasGroup.alpha = 0f;

        this.initScale = Random.Range(0.5f, 1f);
        this.transform.localScale = 0.1f * this.initScale * Vector3.one;

        // KillTweens();

        // //start slow grow/shrink animation
        // var sequence = DOTween.Sequence()
        //     .Append(this.transform.DOScale(this.initScale, this.lifetime * 0.5f))
        //     .Join(this.canvasGroup.DOFade(1f, 5f))
        //     .Append(this.transform.DOScale(this.initScale * 0.1f, this.lifetime * 0.5f))
        //     .Append(this.canvasGroup.DOFade(0f, 5f))
        //     .OnComplete(() => base.OnCollected());

        base.Spawn();
    }
}