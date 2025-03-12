using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouse3D : MonoBehaviour {

    public static Mouse3D Instance { get; private set; }

    [SerializeField] private LayerMask mouseColliderLayerMask = new LayerMask();

    private void Awake() {
        if (Instance != null && Instance != this) {
        Debug.LogError("Multiple Mouse3D instances found! Destroying extra instance.");
        Destroy(gameObject);
        return;
    }
    Instance = this;
    Debug.Log("[Mouse3D] Instance создан!");

    }

    private void Update() {
        if (Camera.main == null) {
            //Debug.LogError("No main camera found!");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, mouseColliderLayerMask)) {
            transform.position = raycastHit.point;
        }
    }

    public static Vector3 GetMouseWorldPosition() {
        if (Instance == null) {
            Debug.LogError("Mouse3D Instance is null! Make sure Mouse3D exists in the scene.");
            return Vector3.zero; // Возвращаем нулевую позицию, чтобы избежать ошибок
        }
        return Instance.GetMouseWorldPosition_Instance();
    }

    private Vector3 GetMouseWorldPosition_Instance() {
        if (Camera.main == null) {
            //Debug.LogError("No main camera found!");
            return Vector3.zero;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, mouseColliderLayerMask)) {
            return raycastHit.point;
        } else {
            return Vector3.zero;
        }
    }
}
