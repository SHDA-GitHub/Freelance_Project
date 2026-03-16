using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class BattleTransition
{
    public BattleTransitionType type;

    [Header("Objects")]
    public GameObject rootObject;
    public List<Image> fillImages;

    [Header("Audio")]
    public AudioClip introSound;
}