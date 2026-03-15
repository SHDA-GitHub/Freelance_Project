using UnityEngine;

public struct PlayerSnapshot
{
    public Vector3 position;
    public Quaternion rotation;
    public float blendX;
    public float blendY;
    public bool jump;
    public bool sprint;

    public PlayerSnapshot(Vector3 pos, Quaternion rot, float bx, float by, bool j, bool s)
    {
        position = pos;
        rotation = rot;
        blendX = bx;
        blendY = by;
        jump = j;
        sprint = s;
    }
}