using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CoinCollector : NetworkBehaviour
{
    public NetworkVariable<int> TotalCoins = new NetworkVariable<int>();

    private int coinValue;

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
