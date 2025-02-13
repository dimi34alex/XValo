using UnityEngine;
using Steamworks;
using Mirror;
using TMPro;
public class SteanLobby : MonoBehaviour
{
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> joinRequest;
    protected Callback<LobbyEnter_t> lobbyEnter;

    public ulong CurrentLobbyID;
    private const string HostAdressKey = "HostAdress";
    private NetworkManagerSteam manager;

    public GameObject HostButton;
    public TextMeshProUGUI LobbyNameText;
    public TextMeshProUGUI Notification;
    private string HostSteamName;

    void Start()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam is not initialized!");
            LobbyNameText.gameObject.SetActive(true);
            LobbyNameText.text = "Steam is not initialized!";
            return;
        }

        manager = GetComponent<NetworkManagerSteam>();

        lobbyCreated = new Callback<LobbyCreated_t>(OnLobbyCreated);
        joinRequest = new Callback<GameLobbyJoinRequested_t>(OnJoinRequest);
        lobbyEnter = new Callback<LobbyEnter_t>(OnEnteredLobby);
    }

    public void HostLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, manager.maxConnections);
    }
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK) { return; }
        manager.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAdressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName().ToString() + "'s lobby");
        HostSteamName = SteamFriends.GetPersonaName().ToString();
    }
    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Request to join lobby");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
    private void OnEnteredLobby(LobbyEnter_t callback)
    {
        //everyone
        HostButton.SetActive(false);
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        LobbyNameText.gameObject.SetActive(true);
        LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name");

        //client
        if (NetworkServer.active) { return; }
        manager.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAdressKey);
        Notification.text = "Вы успешно подключились к " + HostSteamName + "!";
        manager.StartClient();
    }

}
