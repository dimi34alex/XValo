using System;
using Mirror;
using UnityEngine;

public abstract class BaseActionNetwork : NetworkBehaviour, IUnitAction
{
    public abstract ActionType GetActionType();
    [SyncVar]
    public Action onActionStarted;
    public Action onActionFinished;

    public void Setup(Unit unit)
    {

    }
    public Unit GetUnit()
    {
        return null;
    }
    public bool IsActive()
    {
        return false;
    }
}
