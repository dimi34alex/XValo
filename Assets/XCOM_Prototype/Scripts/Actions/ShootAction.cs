using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

// Класс ShootAction наследует BaseAction и отвечает за механику стрельбы юнита
public class ShootAction : BaseAction
{

    // Переопределяем метод, который возвращает тип действия - в данном случае это "Shoot"
    public override ActionType GetActionType() => ActionType.Shoot;

    // Событие, которое вызывается при стрельбе (передает информацию о цели и попадании)
    public event EventHandler<OnShootEventArgs> OnShoot;

    EventHandler actionCom;

    // Класс аргументов события стрельбы
    public class OnShootEventArgs : EventArgs
    {
        public Unit shotUnit; // Юнит, в которого стреляют
        public bool hit; // Попадание (true - попал, false - промахнулся)
    }

    // Перечисление состояний стрельбы
    private enum State
    {
        Aiming,   // Прицеливание
        Shooting, // Стрельба
        Cooloff,  // Ожидание после выстрела (перезарядка)
    }

    private int maxShootDistance = 6; // Максимальная дальность стрельбы в клетках
    private Unit targetUnit; // Цель, в которую стреляет юнит
    private State state; // Текущее состояние стрельбы
    private float stateTimer; // Таймер состояния
    private float maxAccuracy; // Максимальная точность юнита

    // Метод Awake вызывается при инициализации объекта
    private void Awake()
    {
        maxAccuracy = UnityEngine.Random.Range(.8f, 1f); // Случайное значение точности в диапазоне 80-100%
    }

    // Метод начала стрельбы
    public void Shoot(Unit targetUnit, EventHandler onActionComplete)
    {
        actionCom = onActionComplete;
        if (!isServer)
        {
            CmdShoot(targetUnit);
            return;
        }

        this.targetUnit = targetUnit;
        ActionStarted(onActionComplete);
        StartCoroutine(ShootSequence());
    }


    // Команда для вызова на сервере
    [Command]
    private void CmdShoot(Unit targetUnit)
    {
        this.targetUnit = targetUnit;
        RpcStartAiming(targetUnit);
    }

    // Уведомляем клиентов о начале стрельбы
    [ClientRpc]
    private void RpcStartAiming(Unit targetUnit)
    {
        this.targetUnit = targetUnit;
        ActionStarted(actionCom);
    }
    [ClientRpc]
    private void RpcProcessShot(Unit targetUnit, bool hit, int damageAmount)
    {
        OnShoot?.Invoke(this, new OnShootEventArgs { shotUnit = targetUnit, hit = hit });

        if (hit)
        {
            targetUnit.GetHealthSystem().Damage(damageAmount);
        }
    }

    private IEnumerator ShootSequence()
    {
        state = State.Aiming;

        Vector3 aimDir = (targetUnit.GetPosition() - transform.position).normalized;
        float rotationSpeed = 10f;
        float timeElapsed = 0f;

        while (timeElapsed < 1f)
        {
            transform.forward = Vector3.Lerp(transform.forward, aimDir, Time.deltaTime * rotationSpeed);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        if (isServer)
        {
            bool hit = UnityEngine.Random.Range(0f, 1f) < GetHitPercent(targetUnit);
            int damageAmount = hit ? UnityEngine.Random.Range(30, 60) : 0;
            RpcProcessShot(targetUnit, hit, damageAmount);
        }

        state = State.Shooting;
        yield return new WaitForSeconds(0.5f);
        state = State.Cooloff;
        yield return new WaitForSeconds(0.5f);

        ActionComplete();
    }


    // Возвращает текущего противника, в которого стреляем
    public Unit GetTargetUnit()
    {
        return targetUnit;
    }

    // Проверяет, можно ли стрелять из данной позиции
    public bool IsValidShootPosition(Vector3 moveWorldPosition)
    {
        return IsValidShootPosition(LevelGrid.Instance.GetGridPosition(moveWorldPosition));
    }

    public bool IsValidShootPosition(Vector2Int moveGridPosition)
    {
        List<Vector2Int> validShootGridPositionList = GetValidShootGridPositionList();
        return validShootGridPositionList.Contains(moveGridPosition);
    }

    // Возвращает количество целей, доступных для атаки с данной позиции
    public int GetTargetCountAtPosition(Vector2Int currentGridPosition)
    {
        return GetValidShootGridPositionList(currentGridPosition).Count;
    }

    // Возвращает список всех возможных позиций для стрельбы
    public List<Vector2Int> GetValidShootGridPositionList()
    {
        Vector2Int currentGridPosition = unit.GetGridPosition();
        return GetValidShootGridPositionList(currentGridPosition);
    }

    // Рассчитывает возможные позиции для стрельбы вокруг юнита
    public List<Vector2Int> GetValidShootGridPositionList(Vector2Int currentGridPosition)
    {
        List<Vector2Int> validShootGridPositionList = new List<Vector2Int>();

        for (int x = -maxShootDistance; x <= maxShootDistance; x++)
        {
            for (int y = -maxShootDistance; y <= maxShootDistance; y++)
            {
                Vector2Int shootGridPosition = currentGridPosition + new Vector2Int(x, y);
                Vector2Int shootVector = shootGridPosition - currentGridPosition;
                int shootDistance = Mathf.Abs(shootVector.x) + Mathf.Abs(shootVector.y);
                if (shootDistance <= maxShootDistance)
                { // Проверяем, в пределах ли дистанции
                    if (LevelGrid.Instance.IsValidGridPosition(shootGridPosition))
                    { // Проверяем, что позиция не выходит за границы
                        Unit shootUnit = LevelGrid.Instance.GetUnit(shootGridPosition);
                        if (shootUnit != null && shootUnit != unit && shootUnit.IsEnemy() != unit.IsEnemy() && shootUnit.IsVisible())
                        {
                            // Если есть вражеский юнит на этой позиции, добавляем в список
                            validShootGridPositionList.Add(shootGridPosition);
                        }
                    }
                }
            }
        }
        return validShootGridPositionList;
    }

    public int GetMaxShootDistance()
    {
        return maxShootDistance;
    }

    private int GetShootDistance(Vector2Int shootGridPosition)
    {
        Vector2Int currentGridPosition = unit.GetGridPosition();
        Vector2Int shootVector = shootGridPosition - currentGridPosition;
        return Mathf.Abs(shootVector.x) + Mathf.Abs(shootVector.y);
    }

    public bool IsWithinShootingDistance(Vector2Int shootGridPosition)
    {
        return GetShootDistance(shootGridPosition) <= maxShootDistance;
    }

    // Определяет вероятность попадания по цели
    public float GetHitPercent(Unit shootUnit)
    {
        if (IsWithinShootingDistance(shootUnit.GetGridPosition()))
        {
            float hitPercent = maxAccuracy;

            int shootDistance = GetShootDistance(shootUnit.GetGridPosition());
            int fullAccuracyShootDistance = 3;
            int remainingShootDistance = Mathf.Max(0, shootDistance - fullAccuracyShootDistance);
            hitPercent -= .05f * remainingShootDistance; // Чем дальше, тем меньше шанс попасть

            switch (shootUnit.GetCoverType())
            {
                case CoverType.Full:
                    hitPercent -= .3f; // Полное укрытие снижает точность на 30%
                    break;
                case CoverType.Half:
                    hitPercent -= .1f; // Полу-укрытие снижает точность на 10%
                    break;
            }

            return hitPercent;
        }
        else
        {
            return 0f; // Если вне зоны атаки, шанс попасть 0%
        }
    }

}
