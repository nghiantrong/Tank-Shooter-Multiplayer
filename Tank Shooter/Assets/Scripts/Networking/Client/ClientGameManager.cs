using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager
{
    private const string MENU_SCENE_NAME = "Menu";

    public async Task<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();

        //Authenticate player
        AuthState authState = await AuthenticationHandler.DoAuth();

        if (authState == AuthState.Authenticated)
        {
            return true;
        }
        return false;
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(MENU_SCENE_NAME);
    }
}
