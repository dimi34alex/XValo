using Mirror;
using UnityEngine;
using System.Collections.Generic;
using Calroot.MirrorEvents;

public class CustomNetworkManager : NetworkManager
{
    public static CustomNetworkManager Instance { get; private set; }
    private int nextPlayerId = 1;
    private Dictionary<int, NetworkConnectionToClient> connectedPlayers = new Dictionary<int, NetworkConnectionToClient>();

    public override void OnStartServer()
    {
        base.OnStartServer();
        EventManager.Server_Register();
    }
    // Отмените регистрацию сервера для обработки сетевых событий в OnStopServer()
    public override void OnStopServer()
    {
        base.OnStopServer();
        EventManager.Server_Unregister();
    }

    // Зарегистрируйте клиента для обработки сетевых событий в OnStartClient()
    public override void OnStartClient()
    {
        base.OnStartClient();
        EventManager.Client_Register();
    }

    // Отмените регистрацию клиента для обработки сетевых событий в OnStopClient()
    public override void OnStopClient()
    {
        base.OnStopClient();
        EventManager.Client_Unregister();
    }


    private void Awake()
    {
        Instance = this;
    }
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        if (connectedPlayers.Count >= 2)
        {
            Debug.LogWarning("Два игрока уже подключены! Новые подключения запрещены.");
            conn.Disconnect();
            return;
        }

        int playerId = nextPlayerId++; // Первый игрок = 1, второй = 2
        connectedPlayers.Add(playerId, conn);

        Player player = conn.identity.GetComponent<Player>();
        player.SetPlayerId(playerId);

        Debug.Log($"Игрок {playerId} подключился (connectionId: {conn.connectionId})");

        SpawnUnitsForPlayer(playerId);
    }

    private void SpawnUnitsForPlayer(int playerId)
    {
        NetworkConnectionToClient conn = connectedPlayers[playerId];
        for (int i = 0; i < 3; i++)
        {
            Vector3 spawnPosition = (playerId == 1)
                ? new Vector3(0 + i * 2, 0, 2)
                : new Vector3(10 - i * 2, 0, 6); // Юниты спавнятся друг напротив друга

            GameObject unitObj = Instantiate(spawnPrefabs[0], spawnPosition, Quaternion.identity);
            Unit unit = unitObj.GetComponent<Unit>();
            unit.SetOwner((uint)playerId);
            unit.SetEnemy(playerId == 1 ? 2 : 1); // Враги автоматически назначаются

            NetworkServer.Spawn(unitObj);

            // Назначаем authority только клиенту-владельцу
            NetworkIdentity unitIdentity = unitObj.GetComponent<NetworkIdentity>();

            // Удаляем старый authority, если он есть
            if (unitIdentity.connectionToClient != null)
            {
                unitIdentity.RemoveClientAuthority();
            }

            // Назначаем authority текущему игроку
            unitIdentity.AssignClientAuthority(conn);

            Debug.Log($"[SERVER] Назначен authority для юнита {unitObj.name} игроку {playerId} (connId: {conn.connectionId})");
        }
    }
}
