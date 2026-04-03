using System;
using UnityEngine;

public class Attractor : MonoBehaviour
{
    public bool IsActive = true;
    [SerializeField ] private float strength = 1f;
    [SerializeField] private float maxDistance = 5f;
    [SerializeField] private LayerMask attractableLayer;

    [SerializeField] private string tagToAttract = "Attractable";

    private void FixedUpdate()
    {
        if (!IsActive) return;
        
        Collider2D[] attractables = new Collider2D[10];
        int count = Physics2D.OverlapCircleNonAlloc(this.transform.position, this.maxDistance, attractables, this.attractableLayer);
        for (int i = 0; i < count; i++)
        {
            Collider2D attractable = attractables[i];
            if (!attractable.CompareTag(this.tagToAttract)) continue;

            Vector3 direction = this.transform.position - attractable.transform.position;
            float distance = direction.magnitude;
            if (distance > 0f)
            {
                var moveTowards = Vector3.MoveTowards(attractable.transform.position, this.transform.position, this.strength * 1000 * Time.deltaTime / distance);
                //Debug.Log($"RB = {attractable.name}.  distance = {distance}. this.strength = {this.strength} is being attracted to {name}.  pos = {attractable.transform.position}.  moveTowards = {moveTowards}");
                attractable.transform.position = moveTowards;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, this.maxDistance);
    }
}
