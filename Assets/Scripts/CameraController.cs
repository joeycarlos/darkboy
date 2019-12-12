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

    private Shaker s;

    void Awake() {
        _instance = this;
    }

    private void Start() {
        s = GetComponent<Shaker>();
    }

    void LateUpdate() {
        if (player != null && s._isShaking == false)
            transform.position = new Vector3(player.transform.position.x, 0, -10.0f);
    }
}
