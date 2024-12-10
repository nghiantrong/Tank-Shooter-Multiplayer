using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CoinCollector : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private BountyCoin coinPrefab;

    [Header("References")]
    [SerializeField] private float cointSpread = 3f;
    [SerializeField] private float bountyPercentage = 40f;
    [SerializeField] private int bountyCoinCount = 10;
    [SerializeField] private int minBountyCoinValue = 5;
    [SerializeField] private LayerMask layerMask;

    private Collider2D[] coinBuffer = new Collider2D[1];
    private float coinRadius;

    public NetworkVariable<int> TotalCoins = new NetworkVariable<int>();

    private int coinValue;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        coinRadius = coinPrefab.GetComponent<CircleCollider2D>().radius;

        health.OnDie += HandleDie;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        health.OnDie -= HandleDie;
    }

    private void HandleDie(Health health)
    {
        int bountyValue = (int)(TotalCoins.Value * (bountyPercentage / 100f));
        int bountyCoinValue = bountyValue / bountyCoinCount;

        if (bountyCoinValue < minBountyCoinValue) { return; }

        for (int i = 0; i < bountyCoinCount; i++)
        {
            BountyCoin coinInstance = Instantiate(coinPrefab, GetSpawnPoint(), Quaternion.identity);
            coinInstance.SetValue(bountyCoinValue);
            coinInstance.NetworkObject.Spawn();
        }
    }

    private Vector2 GetSpawnPoint()
    {
        while (true)
        {
            Vector2 spawnPoint = (Vector2)transform.position + 
                UnityEngine.Random.insideUnitCircle * cointSpread;
            //will not allocate memory at runtime
            int numColliders = Physics2D.OverlapCircleNonAlloc(
                spawnPoint, coinRadius, coinBuffer, layerMask);
            if (numColliders == 0)
            {
                return spawnPoint;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent<Coin>(out Coin coin))
        {
            return;
        }

        coinValue = coin.Collect();

        if (!IsServer) { return; }

        TotalCoins.Value += coinValue;
    }

    public void SpendCoins(int costToFire)
    {
        TotalCoins.Value -= costToFire;
    }
}
