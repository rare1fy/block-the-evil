using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectAutoDestory : MonoBehaviour
{
    public float Duration;
    private float _startPlayTime = 0;
    private void Update()
    {
        var deltaTime = Time.time - _startPlayTime;
        if (deltaTime >= Duration)
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        _startPlayTime = Time.time;
    }

}
