using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRotationTrigger : MonoBehaviour
{
    private PlayerControl playerControl;

    [SerializeField] private float targetRotationY = 90f;

    private void OnTriggerEnter(Collider other)
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
                playerControl.rotated = true;
                playerControl.SetRotated(true);
            }
        }
    }

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        if (playerControl == null)
    //        {
    //            playerControl = other.GetComponent<PlayerControl>();
    //        }

    //        if (playerControl != null)
    //        {
    //            playerControl.ResetCameraPivotRotation();
    //            playerControl.rotated = false;
    //            playerControl.SetRotated(false);
    //        }
    //    }
    //}
}