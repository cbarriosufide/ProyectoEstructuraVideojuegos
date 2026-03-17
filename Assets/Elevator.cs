using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Elevator : MonoBehaviour
{
    public enum MovementType
    {
        Vertical,
        HorizontalX,
        HorizontalZ
    }

    [Header("Movimiento")]
    public MovementType movementType = MovementType.Vertical;
    public float distance = 59f;
    public float speed = 1f; 

    [Header("Easing")]
    [Range(0f, 2f)]
    public float easingStrength = 1f;
    private Vector3 startPos;
    private Vector3 direction;

    private Rigidbody rb;
    private float progress = 0f;
    private int directionSign = 1;

    [Header("Jugador")]
    public string playerTag = "Player";

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;

        startPos = transform.position;

        switch (movementType)
        {
            case MovementType.Vertical:
                direction = Vector3.up;
                break;
            case MovementType.HorizontalX:
                direction = Vector3.right;
                break;
            case MovementType.HorizontalZ:
                direction = Vector3.forward;
                break;
        }
    }

    void FixedUpdate()
    {
        progress += directionSign * speed * Time.fixedDeltaTime;

        if (progress >= distance)
        {
            progress = distance;
            directionSign = -1;
        }
        else if (progress <= 0f)
        {
            progress = 0f;
            directionSign = 1;
        }

        float t = progress / distance;

        t = ApplyEasing(t, easingStrength);
        Vector3 targetPosition = startPos + direction * (t * distance);

        rb.MovePosition(targetPosition);
    }

    float ApplyEasing(float t, float strength)
    {
        float smooth = t * t * (3f - 2f * t);

        return Mathf.Lerp(t, smooth, strength);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            collision.transform.SetParent(null);
        }
    }
}