using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class followCamera : MonoBehaviour
{
    public Transform plyr;
    public Rigidbody2D rb;
    public float damping;
    private Vector2 move;
    public Vector3 offset;
    private Vector3 velocity = Vector3.zero;


    void FixedUpdate()
    {
        Vector3 movePosition = plyr.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, movePosition, ref velocity, damping);
    }
}
