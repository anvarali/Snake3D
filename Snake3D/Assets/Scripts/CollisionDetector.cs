using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{

    public static Action OnCollide;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Body"))
        {
            OnCollide?.Invoke();
        }
    }
}
