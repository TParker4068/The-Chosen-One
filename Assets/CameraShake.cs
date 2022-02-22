using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private float shakeTimeRemaining, shakePower;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            StartShake(.5f, 1f);
        }
    }

    private void LateUpdate()
    {
        if (shakeTimeRemaining > 0)
        {
            shakeTimeRemaining -= Time.deltaTime;

            float xAmount = Random.Range(-1f, 1f) * shakePower;
            float yAmount = Random.Range(-1f, -1f) * shakePower;

            transform.position += new Vector3(xAmount, yAmount, 0f);
        }
    }

    public void StartShake(float length, float power)
    {
        shakeTimeRemaining = length;
        shakePower = power;
    }
}
