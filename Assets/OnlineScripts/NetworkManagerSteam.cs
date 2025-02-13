using Mirror;

public class NetworkManagerSteam : NetworkManager {
    public void StartHostFromLobby() {
        StartHost();  // Начинаем хост-сессию
    }

    public void JoinGameFromLobby(string ipAddress) {
        networkAddress = ipAddress;
        StartClient();  // Подключение клиента
    }
}
