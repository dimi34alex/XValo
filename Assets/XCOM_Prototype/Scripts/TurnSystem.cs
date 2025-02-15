using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class TurnSystem : NetworkBehaviour {

    public static TurnSystem Instance { get; private set; }


    public event EventHandler OnTurnChanged;


    private int turnNumber;
    private bool isPlayer1Turn;


    private void Awake() {
        Instance = this;

        turnNumber = 1;
        isPlayer1Turn = true;
    }

    public int GetTurnNumber() {
        return turnNumber;
    }

    public bool IsPlayerTurn() {
        return isPlayer1Turn;
    }

    [Command]
    public void NextTurn() {
        turnNumber++;
        isPlayer1Turn = !isPlayer1Turn;
        OnTurnChanged?.Invoke(this, EventArgs.Empty);
    }


}