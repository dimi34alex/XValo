using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] [SyncVar] private int playerId;

    public void SetPlayerId(int id)
    {
        playerId = id;
    }

    public int GetPlayerId()
    {
        return playerId;
    }
}
