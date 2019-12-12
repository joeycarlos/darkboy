using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shaker : MonoBehaviour
{
    public float intensity;

    Transform _target;
    Vector3 _initialPos;

    private void Start() {
        _target = GetComponent<Transform>();
        _initialPos = _target.transform.position;
    }

    float _pendingShakeDuration = 0f;

    public void Shake (float duration, Vector3 startingPosition) {
        if (duration > 0) {
            _pendingShakeDuration += duration;
            _initialPos = startingPosition;
        }
    }

    [HideInInspector] public bool _isShaking = false;

    private void Update() {
        if (_pendingShakeDuration > 0 && !_isShaking) {
            StartCoroutine(DoShake());
        }
    }

    IEnumerator DoShake() {
        _isShaking = true;

        var startTime = Time.realtimeSinceStartup;
        
        while (Time.realtimeSinceStartup < startTime + _pendingShakeDuration) {
            var randomPoint = new Vector3(Random.Range(-1f, 1f)*intensity, Random.Range(-1f, 1f)*intensity, _initialPos.z) + _initialPos;
            _target.transform.position = randomPoint;
            yield return null;
        }

        _pendingShakeDuration = 0f;
        _target.transform.position = _initialPos;
        _isShaking = false;
    }
}
