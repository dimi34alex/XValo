using System;
using UnityEngine;
using Mirror;

public abstract class BaseAction : NetworkBehaviour, IUnitAction
{
    public abstract ActionType GetActionType();

    public event EventHandler OnActionStarted;
    public event EventHandler OnActionComplete;

    protected Unit unit; // Убрали SyncVar
    protected bool isActive;
    protected Action onActionComplete;

    public void Setup(Unit assignedUnit)
    {
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

    protected void ActionStarted(Action onActionComplete)
    {
        Debug.Log($"[BaseAction] {GetType().Name} начат для юнита {unit.name}");
        if (!isServer)
        {
            CmdActionStarted();
            return;
        }

        this.onActionComplete = onActionComplete;
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
        // Проверка, если мы на сервере
        if (!isServer)
        {
            CmdActionComplete();
            return;
        }

        // Логика для завершения действия на сервере
        CompleteAction();
    }

    [Command]
    private void CmdActionComplete()
    {
        // Запрос на завершение действия на сервере
        CompleteAction();
        RpcActionComplete();  // Вызов клиента для завершения действия
    }

    [ClientRpc]
    private void RpcActionComplete()
    {
        // Завершение действия на клиентах
        CompleteAction();
    }

    private void CompleteAction()
    {
        isActive = false;
        unit.UnitActionComplete(this);

        // Вызов события завершения действия
        OnActionComplete?.Invoke(this, EventArgs.Empty);
    }

}
