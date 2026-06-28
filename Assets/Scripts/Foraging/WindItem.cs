using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Lean.Pool;

public class WindItem : MonoBehaviour
{
    private readonly int[] animStates = { Animator.StringToHash("Path1"), Animator.StringToHash("Path2"), Animator.StringToHash("Path3") };
    [SerializeField] private Animator animator;
    [SerializeField] private Transform lootParent;
    [SerializeField] private Image tempImage;

    public Loot Loot { get; private set; }

    private Tweener tweener;

    public void Configure(LootData inLootData)
    {
        this.tempImage.enabled = false;

        if(this.Loot != null)
            LeanPool.Despawn(this.Loot.gameObject);

        this.Loot = Pool.Spawn<Loot>("WindLoot", this.lootParent);
        this.Loot.Configure(inLootData, () =>
        {
            LeanPool.Despawn(this.gameObject);
            this.tweener?.Kill();
        }, maxLootQuantity: 1);
        this.Loot.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        //flip depending on wind direction
        this.transform.localScale = new Vector3(-WeatherManager.WindDirection, 1, 1);
    }

    public void PlayRandomAnim(float inSpeed)
    {
        this.animator.speed = inSpeed;

        var rnd = UnityEngine.Random.Range(0, this.animStates.Length);
        var state = this.animStates[rnd];
        this.animator.Play(state);

        var destPosX = WeatherManager.WindDirection == -1 ? 0 : Screen.width;

        this.tweener = this.transform.DOMoveX(destPosX, 15f).SetEase(Ease.Linear).OnComplete(() =>
        {
            if (this.Loot != null)
            {
                LeanPool.Despawn(this.Loot.gameObject);
                this.Loot = null;
            }

            LeanPool.Despawn(this.gameObject);
        });
    }
}