using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeActionVisual : MonoBehaviour {

    [SerializeField] private Transform pfGrenadeRange;


    private Transform grenadeRangeTransform;


    private void Start() {
        UnitActionSystem.Instance.OnSelectedChanged += UnitActionSystem_OnSelectedChanged;
    }

    private void Update() {
        if (grenadeRangeTransform != null) {
            grenadeRangeTransform.position = LevelGrid.Instance.SnapWorldPosition(Mouse3D.GetMouseWorldPosition());
        }
    }

    private void UnitActionSystem_OnSelectedChanged(object sender, UnitActionSystem.OnSelectedChangedEventArgs eventArgs) {
        if (grenadeRangeTransform != null) {
            Destroy(grenadeRangeTransform.gameObject);
        }

        if (eventArgs.unitAction.GetActionType() == ActionType.Grenade) {
            grenadeRangeTransform = Instantiate(pfGrenadeRange, LevelGrid.Instance.SnapWorldPosition(Mouse3D.GetMouseWorldPosition()), Quaternion.identity);
            grenadeRangeTransform.localScale = Vector3.one * (GrenadeAction.GRENADE_DAMAGE_RANGE * LevelGrid.Instance.GetCellSize() + 1.3f);
        }
    }

}