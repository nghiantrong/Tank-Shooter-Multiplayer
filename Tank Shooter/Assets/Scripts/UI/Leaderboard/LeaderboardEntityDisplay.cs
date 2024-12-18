using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LeaderboardEntityDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private Color myColor;

    private FixedString32Bytes _playerName;
    public ulong ClientId { get; private set; }
    public int Coins { get; private set; }

    public void Initialize(ulong clienId, FixedString32Bytes playerName, int coins)
    {
        ClientId = clienId;
        _playerName = playerName;

        if (clienId == NetworkManager.Singleton.LocalClientId)
        {
            displayText.color = myColor;
        }

        UpdateCoins(coins);
    }

    public void UpdateCoins(int coins)
    {
        Coins = coins;

        UpdateText();
    }

    public void UpdateText()
    {
        displayText.text = $"{transform.GetSiblingIndex() + 1}. {_playerName} - {Coins}";

    }
}
