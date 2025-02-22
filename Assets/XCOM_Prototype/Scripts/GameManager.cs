using Mirror;
using UnityEngine;

public class GameManager : NetworkBehaviour {

    public static GameManager Instance { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            // Можно сохранить объект между сценами, если требуется: DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    // Вызывается на сервере при запуске игры
    public override void OnStartServer() {
        base.OnStartServer();
        Debug.Log("GameManager: Server started.");
    }

    // Вызывается на клиенте после подключения к серверу
    public override void OnStartClient() {
        base.OnStartClient();
        Debug.Log("GameManager: Client started.");
    }
}
