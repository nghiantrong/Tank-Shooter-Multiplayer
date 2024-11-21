using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DealDamageOnContact : MonoBehaviour
{
    [SerializeField] private int damage = 5;

    //Ulong is big positive integer, can be used to store IDs in networking
    private ulong ownerClientId;

    public void SetOwner(ulong ownerClientId)
    {
        this.ownerClientId = ownerClientId;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.attachedRigidbody == null) return;

        //so that the owner cannot be hit by his projectile
        if (collision.attachedRigidbody.TryGetComponent<NetworkObject>(out NetworkObject networkObj))
        {
            if (ownerClientId == networkObj.OwnerClientId)
            {
                return;
            }
        }

        if (collision.attachedRigidbody.TryGetComponent<Health>(out Health health))
        {
            health.TakeDamage(damage);
        }
    }
}
