using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRotationTrigger : MonoBehaviour
{
    private PlayerControl playerControl;

    [SerializeField] private float targetRotationY = 90f;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerControl == null)
            {
                playerControl = other.GetComponent<PlayerControl>();
            }

            if (playerControl != null)
            {
                playerControl.SetCameraPivotRotation(targetRotationY);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerControl == null)
            {
                playerControl = other.GetComponent<PlayerControl>();
            }

            if (playerControl != null)
            {
                playerControl.ResetCameraPivotRotation();
            }
        }
    }
}