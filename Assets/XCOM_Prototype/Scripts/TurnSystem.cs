using Mirror;
using UnityEngine;
using System;

public class TurnSystem : NetworkBehaviour
{
    public static TurnSystem Instance { get; private set; }

    public event EventHandler OnTurnChanged;
    [SyncVar] private int turnNumber;

    [SerializeField] [SyncVar] private int currentTurnPlayerId = 1; // Начинает первый игрок

    private void Awake()
    {
/*         if (isServer)
        {
            currentTurnPlayerId = 1; // Первый ход за первым игроком
        } */
        Instance = this;

        turnNumber = 1;
    }

    private void Start()
    {

    }

    public int GetTurnNumber()
    {
        return turnNumber;
    }
    public bool IsPlayerTurn(int playerId)
    {
        return playerId == currentTurnPlayerId;
    }

    [Command(requiresAuthority = false)]
    public void CmdNextTurn()
    {
        if (!isServer) return;

        currentTurnPlayerId = (currentTurnPlayerId == 1) ? 2 : 1; // Переключаем ходы

        RpcOnTurnChanged(currentTurnPlayerId);
    }

    [ClientRpc]
    private void RpcOnTurnChanged(int newTurnPlayerId)
    {
        Debug.Log($"Теперь ходит игрок {newTurnPlayerId}");
        OnTurnChanged?.Invoke(this, EventArgs.Empty);
    }
}