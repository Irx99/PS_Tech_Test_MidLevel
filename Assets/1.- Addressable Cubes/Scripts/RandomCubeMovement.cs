using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomCubeMovement : MonoBehaviour
{
    private void Update()
    {
        this.transform.position = new Vector3(this.transform.position.x, Mathf.Sin(Time.timeSinceLevelLoad * 5f), this.transform.position.z);
        this.transform.Rotate(Time.deltaTime * 90f * Vector3.up + Time.deltaTime * 90f * Vector3.right * 0.8f);
    }
}
