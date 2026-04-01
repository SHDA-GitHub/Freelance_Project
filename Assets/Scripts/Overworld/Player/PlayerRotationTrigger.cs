using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRotationTrigger : MonoBehaviour
{
    private PlayerControl playerControl;

    [SerializeField] private float targetRotationY = 90f;
    [SerializeField] private bool setRotateToX = true;

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
                if (setRotateToX == true)
                {
                    playerControl.rotated = true;
                    playerControl.SetRotated(true);
                }
                else
                {
                    playerControl.rotated = false;
                    playerControl.SetRotated(false);
                }
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