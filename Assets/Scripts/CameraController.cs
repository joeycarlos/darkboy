using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private static CameraController _instance;

    public static CameraController Instance {
        get {
            if (_instance == null) {
                GameObject go = new GameObject("CameraController");
                go.AddComponent<CameraController>();
            }

            return _instance;
        }
    }

    [SerializeField] private GameObject player;
    [SerializeField] private float horizontalOffset = 0f;
    [SerializeField] private float verticalOffset = 0f;

    void Awake() {
        _instance = this;
    }

    void LateUpdate() {
        if (player != null)
            transform.position = new Vector3(player.transform.position.x + horizontalOffset, verticalOffset, -10.0f);
    }
}
