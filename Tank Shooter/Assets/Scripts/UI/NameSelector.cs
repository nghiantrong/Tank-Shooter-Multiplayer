using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NameSelector : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private Button connectButton;
    [SerializeField] private int minNameLength = 1;
    [SerializeField] private int maxNameLength = 12;

    private const string PLAYER_NAME_KEY = "PlayerName";

    private void Start()
    {
        // headless/dedicated server, skips this scene and move on
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }

        nameField.text = PlayerPrefs.GetString(PLAYER_NAME_KEY, string.Empty);
        HandleNameChange();
    }

    public void HandleNameChange()
    {
        connectButton.interactable =
            nameField.text.Length >= minNameLength &&
            nameField.text.Length <= maxNameLength;
    }

    public void Connect()
    {
        PlayerPrefs.SetString(PLAYER_NAME_KEY, nameField.text);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    } 
}
