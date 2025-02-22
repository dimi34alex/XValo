﻿/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using V_AnimationSystem;

public class SwordAttack : MonoBehaviour, IAttack {

    private enum State {
        Normal,
        Attacking
    }

    private Character_Base characterBase;
    private State state;

    private void Awake() {
        characterBase = GetComponent<Character_Base>();
        SetStateNormal();
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Vector3 attackDir = (UtilsClass.GetMouseWorldPosition() - GetPosition()).normalized;
            Attack(attackDir, () => { });
        }

        switch (state) {
            default:
                break;
        }
    }

    private void SetStateAttacking() {
        state = State.Attacking;
        GetComponent<IMoveVelocity>().Disable();
    }

    private void SetStateNormal() {
        state = State.Normal;
        GetComponent<IMoveVelocity>().Enable();
    }

    public void Attack(Vector3 attackDir, Action onAttackComplete) {
        // Attack
        SetStateAttacking();
            
        //Vector3 attackDir = (UtilsClass.GetMouseWorldPosition() - GetPosition()).normalized;

        //transform.position = transform.position + attackDir * 4f;

        Transform swordSlashTransform = Instantiate(GameAssetsOld.i.pfSwordSlash, GetPosition() + attackDir * 13f, Quaternion.Euler(0, 0, UtilsClass.GetAngleFromVector(attackDir)));
        swordSlashTransform.GetComponent<SpriteAnimator>().onLoop = () => Destroy(swordSlashTransform.gameObject);

        UnitAnimType activeAnimType = characterBase.GetUnitAnimation().GetActiveAnimType();
        if (activeAnimType == GameAssetsOld.UnitAnimTypeEnum.dSwordTwoHandedBack_Sword) {
            swordSlashTransform.localScale = new Vector3(swordSlashTransform.localScale.x, swordSlashTransform.localScale.y * -1, swordSlashTransform.localScale.z);
            characterBase.GetUnitAnimation().PlayAnimForced(GameAssetsOld.UnitAnimTypeEnum.dSwordTwoHandedBack_Sword2, attackDir, 1f, (UnitAnim unitAnim) => { SetStateNormal(); onAttackComplete(); }, null, null);
        } else {
            characterBase.GetUnitAnimation().PlayAnimForced(GameAssetsOld.UnitAnimTypeEnum.dSwordTwoHandedBack_Sword, attackDir, 1f, (UnitAnim unitAnim) => { SetStateNormal(); onAttackComplete(); }, null, null);
        }
    }

    private Vector3 GetPosition() {
        return transform.position;
    }

}
