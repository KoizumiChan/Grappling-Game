using System;
using System.Collections;
using System.Collections.Generic;
using Nomnom.RaycastVisualization;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;


public class movement2 : MonoBehaviour
{
    //references
    Rigidbody2D rb;
    public Collider2D playerCollider;
    LayerMask groundedLayer;
    public PhysicsMaterial2D playerMaterial;
    public Transform resetPoint;
    public DistanceJoint2D grapple;
    public LineRenderer grappleLine;
    private Vector2 grapplePoint;

    //states
    public enum movementState{
        Grappling,
        Hookshot,
        Grounded,
        Airborne
    }
    public movementState currentState;

    //configurations
    private float grabDistance = 10f;
    private float groundDistance = 0.8f;
    private float grappleLength = 8f;
    private float grappleJump = 1f;
    public float speed = 10f;
    private float airSpeed = 20f;
    private float jumpForce = 25f;
    private float dashForce = 25f;
    private float hShotBreakForce = 15f;


    //ability unlocks

    public bool dashUnlocked = false;
    public bool hShotUnlocked = false;
    public bool grappleUnlocked = true;

    //bool flags
    private bool dashUsed = false;
    private bool grappleUsed = false;
    private bool hshotUsed = false; //wallgrab replacement
    private bool dashResetUsed = false;
    private bool _jump = false;
    private bool _grapple = false;
    private bool _hShot = false;
    private bool _airDash = false;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        groundedLayer= LayerMask.GetMask("ground");
    }
    void FixedUpdate()
    {
        Grounded();
        Grapple();
        Movement();
        AirDash();
        Hookshot();
    }
    void Update()
    {
        HandleInput();
        CheckLine();
    }
    void HandleInput()
    {
        if(Input.GetKeyDown(KeyCode.Space) && (currentState == movementState.Grounded || currentState == movementState.Grappling || currentState == movementState.Hookshot)){
            _jump = true;
            print("Jump trigger");
        }
        if(Input.GetMouseButton(0) && currentState != movementState.Grounded && !grappleUsed){
            _grapple = true;
        }
        if(Input.GetKeyDown(KeyCode.Space) && currentState == movementState.Airborne && !dashUsed){
            _airDash = true;
        }
        if(Input.GetMouseButton(1) && currentState == movementState.Airborne && !hshotUsed){
            _hShot = true;
        }
        if(Input.GetKeyDown(KeyCode.R)){
            transform.position = resetPoint.position;
        }
        if(Input.GetKeyDown(KeyCode.Escape)){
            Application.Quit();
        }
    }
    void Grounded(){
        if (VisualPhysics2D.CircleCast(transform.position, 0.2f,-Vector2.up,groundDistance, groundedLayer) && currentState != movementState.Hookshot){
            currentState =  movementState.Grounded;
            dashUsed = false;
            grappleUsed = false;
            hshotUsed = false;
            dashResetUsed = false;
            print("Grounded");
            BreakGrapple();
        }
        else if(currentState != movementState.Grappling && currentState != movementState.Hookshot){
            currentState = movementState.Airborne;
        }
        else{
        }
    }
    void Movement(){
        if(currentState == movementState.Grounded && !_jump){//platform moving
            if (Input.GetAxisRaw("Horizontal") != 0){
                rb.velocityX = Input.GetAxisRaw("Horizontal") * speed;
            }
        }
        else{
        }
        if(currentState == movementState.Grounded && _jump){//jump
            _jump = false;
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            currentState = movementState.Airborne;
            print("test2");
        }
        if(currentState == movementState.Airborne){
            rb.AddForce(new Vector2(Input.GetAxisRaw("Horizontal"), 0) * (airSpeed/2));
        }
        else if(currentState == movementState.Grappling && _jump){
            _jump = false;
            BreakGrapple();
        }
        else if(currentState == movementState.Hookshot && _jump){
            _jump = false;
            print("Break HookShot");
            BreakHookShot();
        }
        else if(currentState == movementState.Grappling){
            rb.AddForce(new Vector2(Input.GetAxisRaw("Horizontal"), 0) * airSpeed);//swing on grapple
        }


    }
    void Grapple(){
        if(_grapple){
            _grapple = false;
            RaycastHit2D grappleHit = VisualPhysics2D.Raycast(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position, grappleLength, groundedLayer);
            if(grappleHit){
                currentState = movementState.Grappling;
                grapplePoint = grappleHit.point;
                grappleUsed = true;
                grapple.connectedAnchor = grappleHit.point;
                grapple.enabled = true;
                grapple.autoConfigureDistance = true;
                grapple.maxDistanceOnly = false;
            }          
        }
    }
    void BreakGrapple(){
        grapple.enabled = false;
        if(currentState != movementState.Grounded){
            currentState = movementState.Airborne;
            rb.AddForce(rb.velocity * grappleJump, ForceMode2D.Impulse);
        }
    }
    void AirDash(){
        if(_airDash){
            rb.AddForce(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) * dashForce, ForceMode2D.Impulse);
            dashUsed = true;
        }
        _airDash = false;
    }
    void Hookshot(){
        if(_hShot){
            RaycastHit2D hShotHit = VisualPhysics2D.Raycast(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position, grabDistance, groundedLayer);
            if(hShotHit){
                hshotUsed = true;
                grapplePoint = hShotHit.point;
                currentState = movementState.Hookshot;
                grapple.connectedAnchor = hShotHit.point;
                grapple.enabled = true;
                grapple.autoConfigureDistance = false;
                grapple.distance = 0.5f;
                grapple.maxDistanceOnly = true;
            }
        }
       _hShot = false;
    }
    void BreakHookShot(){
        grapple.enabled = false;
        currentState = movementState.Airborne;
        rb.AddForce(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) * hShotBreakForce, ForceMode2D.Impulse);
    }
    void CheckLine(){
        if(currentState == movementState.Grappling || currentState == movementState.Hookshot){
            grappleLine.positionCount = 2;
            var grapplePoints = new Vector3[2];
            grapplePoints[0] = rb.position;
            grapplePoints[1] = grapplePoint;
            grappleLine.enabled = true;
            grappleLine.SetPositions(grapplePoints);
        }
        else{ grappleLine.enabled = false;}
    }
}

