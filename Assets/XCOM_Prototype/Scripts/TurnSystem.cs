using Mirror;
using UnityEngine;
using System;

public class TurnSystem : NetworkBehaviour
{
    public static TurnSystem Instance { get; private set; }

    public event EventHandler OnTurnChanged;

    [SyncVar] private int turnNumber = 1;

    [SyncVar(hook = nameof(OnTurnPlayerChanged))]
    private int currentTurnPlayerId = 1;


    public Player localPlayer { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        FindLocalPlayer();
    }

    public int GetCurrentTurnPlayer()
    {
        return currentTurnPlayerId;
    }

    public int GetTurnNumber()
    {
        return turnNumber;
    }

    public bool IsPlayerTurn(int playerId)
    {
        return playerId == currentTurnPlayerId;
    }

    [Server]
    public void ServerNextTurn(int playerId)
    {
        if (!IsPlayerTurn(playerId))
        {
            Debug.LogWarning($"[TurnSystem] Игрок {playerId} не может завершить ход, сейчас ходит {currentTurnPlayerId}!");
            return;
        }

        turnNumber++;
        currentTurnPlayerId = (currentTurnPlayerId == 1) ? 2 : 1;

        Debug.Log($"[TurnSystem] Ход переключен на игрока {currentTurnPlayerId}");
        //RpcOnTurnChanged(currentTurnPlayerId, turnNumber);
    }

    private void OnTurnPlayerChanged(int oldPlayerId, int newPlayerId)
    {
        Debug.Log($"[TurnSystem] Новый ход у игрока {newPlayerId}");

        OnTurnChanged?.Invoke(this, EventArgs.Empty);
    }

    private void FindLocalPlayer()
    {
        foreach (var player in FindObjectsOfType<Player>())
        {
            if (player.isLocalPlayer)
            {
                localPlayer = player;
                Debug.Log($"[TurnSystem] Локальный игрок найден: {localPlayer.GetPlayerId()}");
                return;
            }
        }

        Invoke(nameof(FindLocalPlayer), 1f);
    }
}
