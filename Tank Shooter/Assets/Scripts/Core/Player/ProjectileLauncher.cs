using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProjectileLauncher : NetworkBehaviour
{
    [Header("Reference")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private CoinCollector coinCollector;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject serverProjectilePrefab;
    [SerializeField] private GameObject clientProjectilePrefab;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private Collider2D playerCollider;

    [Header("Settings")]
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private float muzzleFlashDuration;
    [SerializeField] private int costToFire;

    private bool shouldFire;

    private float timer;

    private float muzzleFlashTimer;
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }

        inputReader.PrimaryFireEvent += HandlePrimaryFire;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) { return; }

        inputReader.PrimaryFireEvent -= HandlePrimaryFire;

    }

    private void Update()
    {
        if (muzzleFlashTimer > 0f)
        {
            muzzleFlashTimer -= Time.deltaTime;

            if (muzzleFlashTimer <= 0f)
            {
                muzzleFlash.SetActive(false);
            }
        }

        if (!IsOwner) return;

        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }

        if (!shouldFire) return;

        if (timer > 0)
        {
            return;
        }

        if (coinCollector.TotalCoins.Value < costToFire)
        {
            return;
        }

        //For other users to see the projectile
        PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);

        SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.up);

        //cooldown for the firing
        timer = 1 / fireRate;

    }

    private void HandlePrimaryFire(bool shouldFire)
    {
        this.shouldFire = shouldFire;
    }

    //Client calls serverRpc
    [ServerRpc]
    private void PrimaryFireServerRpc(Vector3 spawnPos, Vector3 direction)
    {
        if (coinCollector.TotalCoins.Value < costToFire)
        {
            return;
        }

        coinCollector.SpendCoins(costToFire);

        GameObject projectileInstance = Instantiate(
            serverProjectilePrefab,
            spawnPos,
            Quaternion.identity);

        projectileInstance.transform.up = direction;

        Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());

        //the one who lauches the projectile is the owner
        if (projectileInstance.TryGetComponent<DealDamageOnContact>(out DealDamageOnContact dealDamage))
        {
            dealDamage.SetOwner(OwnerClientId);
        }

        if (projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.velocity = rb.transform.up * projectileSpeed;
        }

        SpawnDummyProjectileClientRpc(spawnPos, direction); 
    }

    //Server calls clientRpc
    [ClientRpc]
    private void SpawnDummyProjectileClientRpc(Vector3 spawnPos, Vector3 direction)
    {
        if (IsOwner) return;

        SpawnDummyProjectile(spawnPos, direction);
    }

    private void SpawnDummyProjectile(Vector3 spawnPos, Vector3 direction)
    {
        muzzleFlash.SetActive(true);
        muzzleFlashTimer = muzzleFlashDuration;

        GameObject projectileInstance = Instantiate(
            clientProjectilePrefab,
            spawnPos,
            Quaternion.identity);

        projectileInstance.transform.up = direction;

        Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());

        if (projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.velocity = rb.transform.up * projectileSpeed;
        }
    }
}
