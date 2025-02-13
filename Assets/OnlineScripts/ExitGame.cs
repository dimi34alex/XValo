using Steamworks;
using UnityEngine;

public class ExitGame : MonoBehaviour
{

    public void Exit()
    {
        SteamAPI.Shutdown();
        Application.Quit();
    }
}
