using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MoveAction : BaseAction
{
    public override ActionType GetActionType() => ActionType.Move;

    private int maxMoveDistance = 4;
    private SyncList<Vector3> pathPositionList = new SyncList<Vector3>();
    [SyncVar] private int currentPositionIndex;

    Action actionCom;

    private void Awake()
    {
        currentPositionIndex = 0;
    }

    private void Update()
    {
        if (!isActive) return;
        if (pathPositionList == null || pathPositionList.Count == 0)
        {
            Debug.LogError("[MoveAction] Ошибка: Путь пуст!");
            ActionComplete();
            return;
        }

        if (currentPositionIndex >= pathPositionList.Count) // Проверяем перед использованием индекса
        {
            Debug.LogError("[MoveAction] Ошибка: Индекс вне диапазона!");
            ActionComplete();
            return;
        }

        Vector3 targetPosition = pathPositionList[currentPositionIndex];
        Vector3 moveDir = (targetPosition - transform.position).normalized;

        float rotationSpeed = 10f;
        transform.forward = Vector3.Lerp(transform.forward, moveDir, Time.deltaTime * rotationSpeed);

        float moveSpeed = 4f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        float reachedDistance = 0.1f;
        if (Vector3.Distance(transform.position, targetPosition) < reachedDistance)
        {
            if (currentPositionIndex + 1 >= pathPositionList.Count) // Если следующий индекс выйдет за пределы
            {
                ActionComplete();
                actionCom.Invoke();
                //isActive = false; // Остановка Update()
                return;
            }

            currentPositionIndex++;
        }

    }

    // Клиент отправляет команду на сервер для начала перемещения
    public void Move(Vector3 targetPosition, Action onActionComplete)
    {
        actionCom = onActionComplete;
        Debug.Log($"[MoveAction] Move() вызван: {targetPosition}");

        if (!isOwned)
        {
            Debug.LogError("[MoveAction] Этот клиент не владеет юнитом!");
            ActionComplete();
            actionCom.Invoke();
            return;
        }

        if (!isServer)
        {
            CmdMove(targetPosition);
            return;
        }

        ExecuteMove(targetPosition);
    }


    [Command]
    private void CmdMove(Vector3 targetPosition)
    {
        Debug.Log($"[SERVER] CmdMove() вызван: {targetPosition}");

        List<Vector3> foundPath = LevelPathfinding.Instance.FindPath(unit.GetPosition(), targetPosition, out int pathLength);
        if (foundPath == null || foundPath.Count == 0)
        {
            Debug.LogError("[SERVER] Путь не найден!");
            ActionComplete();
            actionCom.Invoke();
            return;
        }

        pathPositionList.Clear();
        foreach (var pos in foundPath)
        {
            pathPositionList.Add(pos);
        }

        currentPositionIndex = 0;
        RpcMove();
    }


    [ClientRpc]
    private void RpcMove()
    {
        if (isServer) return; // Сервер уже выполняет перемещение
        currentPositionIndex = 0;
        isActive = true;
    }


    private void ExecuteMove(Vector3 targetPosition)
    {
        Debug.Log($"[ExecuteMove] Начинаем поиск пути к {targetPosition}");

        // Найти путь
        List<Vector3> foundPath = LevelPathfinding.Instance.FindPath(unit.GetPosition(), targetPosition, out int pathLength);

        if (foundPath == null || foundPath.Count == 0)
        {
            Debug.LogError("[ExecuteMove] Ошибка! Путь не найден.");
            ActionComplete();
            actionCom.Invoke();
            return;
        }

        // Очистить старый путь и обновить SyncList<Vector3>
        pathPositionList.Clear();
        foreach (var pos in foundPath)
        {
            pathPositionList.Add(pos);
        }

        // Сброс индекса и активация движения
        currentPositionIndex = 0;
        isActive = true;
    }


    public bool IsMoving() => isActive;

    public bool IsValidMovePosition(Vector3 moveWorldPosition)
    {
        return IsValidMovePosition(LevelGrid.Instance.GetGridPosition(moveWorldPosition));
    }

    public bool IsValidMovePosition(Vector2Int moveGridPosition)
    {
        List<Vector2Int> validMoveGridPositionList = GetValidMoveGridPositionList();
        return validMoveGridPositionList.Contains(moveGridPosition);
    }

    public List<Vector2Int> GetValidMoveGridPositionList()
    {
        Vector2Int currentGridPosition = unit.GetGridPosition();
        List<Vector2Int> validMoveGridPositionList = new List<Vector2Int>();

        for (int x = -maxMoveDistance; x <= maxMoveDistance; x++)
        {
            for (int y = -maxMoveDistance; y <= maxMoveDistance; y++)
            {
                Vector2Int moveGridPosition = currentGridPosition + new Vector2Int(x, y);
                if (moveGridPosition == currentGridPosition) continue;
                if (!LevelGrid.Instance.IsValidGridPosition(moveGridPosition)) continue;
                if (LevelGrid.Instance.HasUnitAtPosition(moveGridPosition)) continue;

                List<Vector3> path = LevelPathfinding.Instance.FindPath(
                    unit.GetPosition(),
                    LevelGrid.Instance.GetWorldPosition(moveGridPosition.x, moveGridPosition.y),
                    out int pathLength
                );

                if (path != null)
                {
                    float moveCost = (float)PathfindingXCOM.Pathfinding.MOVE_STRAIGHT_COST;
                    if ((pathLength / moveCost) <= maxMoveDistance)
                    {
                        validMoveGridPositionList.Add(moveGridPosition);
                    }
                }
            }
        }
        return validMoveGridPositionList;
    }

    public EnemyAIAction GetEnemyAIAction()
    {
        List<Vector2Int> validMoveGridPositionList = GetValidMoveGridPositionList();
        List<EnemyAIAction> enemyAIActionList = new List<EnemyAIAction>();

        ShootAction shootAction = unit.GetAction<ShootAction>();

        foreach (Vector2Int gridPosition in validMoveGridPositionList)
        {
            int targetCount = shootAction.GetTargetCountAtPosition(gridPosition);
            enemyAIActionList.Add(new EnemyAIAction
            {
                actionGridPosition = gridPosition,
                actionValue = 10 * targetCount,
            });
        }

        if (enemyAIActionList.Count > 0)
        {
            enemyAIActionList.Sort((EnemyAIAction a, EnemyAIAction b) => b.actionValue - a.actionValue);
            return enemyAIActionList[0];
        }
        else
        {
            return null;
        }
    }

    public class EnemyAIAction
    {
        public Vector2Int actionGridPosition;
        public int actionValue;
    }
}
