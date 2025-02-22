using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public abstract class BaseAction : NetworkBehaviour, IUnitAction
{

    public abstract ActionType GetActionType();


    public event EventHandler OnActionStarted;
    public event EventHandler OnActionComplete;



    protected Unit unit;
    protected bool isActive;
    protected Action onActionComplete;


    public void Setup(Unit unit)
    {
        this.unit = unit;
        unit.GetHealthSystem().OnDead += Unit_OnDead;
    }

    public Unit GetUnit() => unit;

    public bool IsActive() => isActive;



    private void Unit_OnDead(object sender, EventArgs e)
    {
        unit.GetHealthSystem().OnDead -= Unit_OnDead;
        if (IsActive())
        {
            ActionComplete();
        }
    }


    protected void ActionStarted(Action onActionComplete)
    {
        this.onActionComplete = onActionComplete;
        unit.SetActiveUnitAction(this);
        isActive = true;

        OnActionStarted?.Invoke(this, EventArgs.Empty);
    }

    protected void ActionComplete()
    {
        if (isServer) {
        RpcActionComplete(); // Сервер говорит клиенту завершить действие
    }
    }
    [ClientRpc]
    private void RpcActionComplete()
    {
        isActive = false;
        unit.UnitActionComplete(this);
        onActionComplete?.Invoke();
        OnActionComplete?.Invoke(this, EventArgs.Empty);
    }


}
