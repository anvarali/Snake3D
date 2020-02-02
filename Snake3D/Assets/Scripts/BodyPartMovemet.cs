using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum SnakeBodyType
{
    tail,
    head,
    body
}
public class BodyPartMovemet : MonoBehaviour
{
    internal BodyPartMovemet previousPartMovemt;
    [SerializeField] private SnakeBodyType snakeBodyType;

    public void Move(Vector3 newPos, float newZRot)
    {
        //If Snake part is there after this, invoke the movement for that part
        if (previousPartMovemt != null)
        {
            previousPartMovemt.Move(transform.localPosition, transform.localRotation.eulerAngles.y);
        }
        //Set new pos
        transform.localPosition = newPos;
        //Head rotation will be handled in Snake handler script itself
        if (snakeBodyType != SnakeBodyType.head)
            transform.localRotation = Quaternion.Euler(0, 0, newZRot);
    }
}
