using System;
using UnityEngine;

//[CreateAssetMenu(fileName = "Direction", menuName = "New Transform Direction")]
public class TransformDirection : MonoBehaviour
{
    public enum Vertical
    { None, Up, Down }

    public enum Horizontal
    { None, Right, Left }

    [Serializable]
    public struct MoveDirection
    {
        public Vertical vertical;
        public Horizontal horizontal;
    }

    public MoveDirection direction;
}
