using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAnims : MonoBehaviour {

    public enum ColorName {
        Green,
        Red,
        Blue,
    }


    private Animator doorAnimator;


    private void Awake() {
        doorAnimator = GetComponent<Animator>();
    }

    public void OpenDoor() {
        doorAnimator.SetBool("Open", true);
    }

    public void CloseDoor() {
        doorAnimator.SetBool("Open", false);
    }

    public void PlayOpenFailAnim() {
        doorAnimator.SetTrigger("OpenFail");
    }

    public void SetColor(ColorName colorName) {
        Material doorMaterial = GameAssetsOld.i.m_DoorRed;
        Material doorKeyHoleMaterial = GameAssetsOld.i.m_DoorKeyHoleRed;

        switch (colorName) {
        default:
        case ColorName.Red:
            doorMaterial = GameAssetsOld.i.m_DoorRed;
            doorKeyHoleMaterial = GameAssetsOld.i.m_DoorKeyHoleRed;
            break;
        case ColorName.Green:
            doorMaterial = GameAssetsOld.i.m_DoorGreen;
            doorKeyHoleMaterial = GameAssetsOld.i.m_DoorKeyHoleGreen;
            break;
        case ColorName.Blue:
            doorMaterial = GameAssetsOld.i.m_DoorBlue;
            doorKeyHoleMaterial = GameAssetsOld.i.m_DoorKeyHoleBlue;
            break;
        }

        transform.Find("DoorLeft").GetComponent<SpriteRenderer>().material = doorMaterial;
        transform.Find("DoorRight").GetComponent<SpriteRenderer>().material = doorMaterial;
        if (transform.Find("DoorKeyHoleLeft") != null) transform.Find("DoorKeyHoleLeft").GetComponent<SpriteRenderer>().material = doorKeyHoleMaterial;
        if (transform.Find("DoorKeyHoleRight") != null) transform.Find("DoorKeyHoleRight").GetComponent<SpriteRenderer>().material = doorKeyHoleMaterial;
    }

}
