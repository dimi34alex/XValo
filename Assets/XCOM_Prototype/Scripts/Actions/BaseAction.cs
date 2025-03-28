using System;
using UnityEngine;
using Mirror;
using Calroot.MirrorEvents;

public abstract class BaseAction : NetworkBehaviour, IUnitAction
{
    public abstract ActionType GetActionType();

    public EventHandler OnActionStarted;
    public EventHandler OnActionComplete;


    protected Unit unit; // Убрали SyncVar
    [SyncVar] protected bool isActive;

    public void Setup(Unit assignedUnit)
    {
        EventManager.RegisterListeners(this);
        if (assignedUnit == null)
        {
            Debug.LogError($"[BaseAction] Unit is NULL during Setup! Object: {gameObject.name}");
            return;
        }

        unit = assignedUnit;
        unit.GetHealthSystem().OnDead += Unit_OnDead;
    }

    public Unit GetUnit() => unit;
    public bool IsActive() => isActive;

    private void Unit_OnDead(object sender, EventArgs e)
    {
        unit.GetHealthSystem().OnDead -= Unit_OnDead;
        if (IsActive()) ActionComplete();
    }

    protected void ActionStarted(EventHandler onActionComplete)
    {
        Debug.Log($"[BaseAction] {GetType().Name} начат для юнита {unit.name}");
        Debug.Log($"[BaseAction] {onActionComplete} событие");
        
        OnActionComplete += onActionComplete;
        if (!isServer)
        {
            Debug.Log($"[BaseAction] {onActionComplete} событие 2");
            CmdActionStarted();
            return;
        }
        //OnActionComplete = null;
        Debug.Log($"[BaseAction] {onActionComplete} событие 3");
        
        unit.SetActiveUnitAction(this);
        isActive = true;

        OnActionStarted?.Invoke(this, EventArgs.Empty);
    }

    [Command] private void CmdActionStarted() => RpcActionStarted();
    [ClientRpc]
    private void RpcActionStarted()
    {
        isActive = true;
        OnActionStarted?.Invoke(this, EventArgs.Empty);
    }

    protected void ActionComplete()
    {

        if (isServer)
        {
            CompleteAction();  // Завершаем действие на сервере
            RpcActionComplete();  // Рассылаем завершение клиентам
        }
        else
        {
            CmdActionComplete();  // Клиент отправляет команду серверу
        }
    }

    [Command]
    private void CmdActionComplete()
    {
        Debug.Log("CmdActionComplete вызван");
        UnitActionSystem.Instance.ClearBusy(this, EventArgs.Empty);
        // Завершаем действие только на сервере!
        CompleteAction();

        // Рассылаем завершение клиентам
        RpcActionComplete();
    }

    [ClientRpc]
    private void RpcActionComplete()
    {
        if (isServer) return;  // На сервере уже было выполнено!
        UnitActionSystem.Instance.ClearBusy(this, EventArgs.Empty);
        // Завершение действия на клиентах
        CompleteAction();
    }

    [Server]
    private void CompleteAction()
    {
        Debug.Log("OnActionComplete == null: " + (OnActionComplete == null));
        isActive = false;
        unit.UnitActionComplete(this);

        // Вызов события завершения действия
        
        OnActionComplete?.Invoke(this, EventArgs.Empty);

        
    }


}
