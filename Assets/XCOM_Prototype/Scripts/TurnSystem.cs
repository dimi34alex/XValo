using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class TurnSystem : NetworkBehaviour {

    public static TurnSystem Instance { get; private set; }


    public event EventHandler OnTurnChanged;


    private int turnNumber;
    private bool isPlayerTurn;


    private void Awake() {
        Instance = this;

        turnNumber = 1;
        isPlayerTurn = true;
    }

    public int GetTurnNumber() {
        return turnNumber;
    }

    public bool IsPlayerTurn() {
        return isPlayerTurn;
    }
    [Command]
    public void NextTurn() {
        turnNumber++;
        isPlayerTurn = !isPlayerTurn;
        OnTurnChanged?.Invoke(this, EventArgs.Empty);
    }


}