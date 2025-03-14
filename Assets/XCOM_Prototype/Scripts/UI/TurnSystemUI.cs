using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class TurnSystemUI : MonoBehaviour 
{
    private TextMeshProUGUI turnText;
    private Button endTurnButton;
    private Player localPlayer;

    bool TurnPlayer1;
    bool TurnPlayer2;

    private void Awake() 
    {
        turnText = transform.Find("TurnText").GetComponent<TextMeshProUGUI>();
        endTurnButton = transform.Find("EndTurnBtn").GetComponent<Button>();
        endTurnButton.onClick.AddListener(OnEndTurnClicked);
    }

    private void Start() 
    {
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
        FindLocalPlayer(); // Ищем игрока сразу, если он уже есть
    }

    private void FindLocalPlayer()
    {
        foreach (var player in FindObjectsOfType<Player>())
        {
            if (player.isLocalPlayer) 
            {
                localPlayer = player;
                Debug.Log($"[TurnSystemUI] Локальный игрок найден: {localPlayer.GetPlayerId()}");
                UpdateTurnText();
                return;
            }
        }

        // Повторная проверка через секунду, если игрок еще не найден
        Invoke(nameof(FindLocalPlayer), 1f);
    }

    private void OnEndTurnClicked()
    {
        if (localPlayer == null)
        {
            Debug.LogWarning("[TurnSystemUI] Локальный игрок не найден!");
            return;
        }

        if (TurnSystem.Instance.IsPlayerTurn(localPlayer.GetPlayerId()))
        {
            Debug.Log($"[TurnSystemUI] Игрок {localPlayer.GetPlayerId()} пытается завершить ход");
            localPlayer.NextTurn();
            //localPlayer = ;
        }
        else
        {
            Debug.Log("Вы не можете завершить этот ход!");
        }
    }

    private void TurnSystem_OnTurnChanged(object sender, System.EventArgs e) 
    {
        UpdateTurnText();
    }

    private void UpdateTurnText() 
    {
        turnText.text = $"TURN {TurnSystem.Instance.GetTurnNumber()} - Player {TurnSystem.Instance.GetCurrentTurnPlayer()}";
        
        // Включаем/отключаем кнопку в зависимости от хода
        if (localPlayer != null)
        {
            bool canEndTurn = TurnSystem.Instance.IsPlayerTurn(localPlayer.GetPlayerId());
            endTurnButton.interactable = canEndTurn;
            Debug.Log($"[TurnSystemUI] Кнопка 'Закончить ход' активна: {canEndTurn}");
        }
    }
}
