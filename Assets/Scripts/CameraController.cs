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

    public GameObject player;
    public float horizontalOffset = 0f;
    public float verticalOffset = 0f;

    void Awake() {
        _instance = this;
    }

    // Update is called once per frame
    void LateUpdate() {
        if (player != null)
            transform.position = new Vector3(player.transform.position.x + horizontalOffset, verticalOffset, -10.0f);
    }
}
