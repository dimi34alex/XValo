using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance { get; private set; }
    private Player localPlayer;

    void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
        }
    }

    public Player FindLocalPlayer()
    {
        foreach (var player in FindObjectsOfType<Player>())
        {
            if (player.isLocalPlayer) 
            {
                localPlayer = player;
                Debug.Log($"[TurnSystemUI] Локальный игрок найден: {localPlayer.GetPlayerId()}");
                return localPlayer;
            }
        }

        return null;
        // Повторная проверка через секунду, если игрок еще не найден
        //Invoke(nameof(FindLocalPlayer), 1f);
    }

}
