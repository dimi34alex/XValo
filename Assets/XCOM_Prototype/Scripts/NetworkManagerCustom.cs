using System;
using Mirror;
using UnityEngine;

public class NetworkManagerCustom : NetworkManager {

    public GameObject unitPrefab; // Префаб юнита, который будет спавниться для игрока

    // Вызывается на сервере при запуске
    public override void OnStartServer() {
        base.OnStartServer();
        // Можно здесь зарегистрировать дополнительные сетевые сообщения, если понадобится
    }

    // Вызывается на сервере, когда новый клиент подключается
    public override void OnServerAddPlayer(NetworkConnectionToClient conn) {
        // Спавним игрока в случайной позиции
        Vector3 playerSpawnPosition = GetSpawnPositionForPlayer();
        GameObject player = Instantiate(playerPrefab, playerSpawnPosition, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, player);
        Debug.Log($"Player {conn.connectionId} connected at {playerSpawnPosition}");

        // Спавним юниты для этого игрока
        SpawnUnitsForPlayer(conn, playerSpawnPosition);
    }

    // Метод для спавна юнитов для подключившегося игрока
    private void SpawnUnitsForPlayer(NetworkConnectionToClient conn, Vector3 playerSpawnPosition) {
        for (int i = 0; i < 3; i++) {
            // Расставляем юниты рядом с позицией игрока (с небольшим смещением)
            Vector3 unitSpawnPosition = playerSpawnPosition + new Vector3((i + 1) * 2, 0, 0);
            GameObject unit = Instantiate(unitPrefab, unitSpawnPosition, Quaternion.identity);
            // Спавним юнит и сразу передаём владение (authority) клиенту conn
            NetworkServer.Spawn(unit, conn);
        }
    }

    // Пример получения случайной позиции для спавна игрока
    private Vector3 GetSpawnPositionForPlayer() {
        float range = 5f;
        return new Vector3(UnityEngine.Random.Range(-range, range), 0, UnityEngine.Random.Range(-range, range));
    }
}
