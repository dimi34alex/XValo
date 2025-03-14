using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SyncVar] private int playerId;
    [SyncVar] private bool isPlayerTurn;

    public void SetPlayerId(int id)
    {
        playerId = id;
    }

    public int GetPlayerId()
    {
        return playerId;
    }

    public void SetPlayerTurn(bool turn)
    {
        isPlayerTurn = turn;
    }

    public bool IsPlayerTurn()
    {
        return isPlayerTurn;
    }

    public void NextTurn()
    {
        if (!isLocalPlayer) return;

        Debug.Log($"[Player] Игрок {playerId} пытается завершить ход");

        CmdRequestNextTurn();
    }

    [Command]
    private void CmdRequestNextTurn()
    {
        if (TurnSystem.Instance != null)
        {
            TurnSystem.Instance.ServerNextTurn(playerId);
        }
    }
}
