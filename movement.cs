using System;
using System.Collections;
using System.Collections.Generic;
using Nomnom.RaycastVisualization;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;


public class movement : MonoBehaviour
    {
    public float speed = 300f;
    public float airSpeed = 100f;
    public float horDrag = 5f;
    public float jumpForce = 1000f;
    public float dashForce = 10f;
    public bool dashUsed = false;
    public float groundedDistance = 5f;
    RaycastHit2D hit;
    RaycastHit2D grappleHit;
    public bool isGrounded;

    //grappling
    public bool isGrappling = false;
    public float grappleLength = 10f;
    public bool grappleUsed = false;
    public float grappleJump;
    Vector3 grapplePoint;


    //wallGrab
    public bool wallgrabUsed;
    public bool wallgrabRefreshUsed = false;
    public bool isWallGrabbing = false;
    RaycastHit2D grabPoint1;
    RaycastHit2D grabPoint2;
    public Transform wallGrabRay1;
    public Transform wallGrabRay2;
    public float grabLength = 1f;
    

    public Transform resetPoint;
    public bool isJumping = false;
    public bool _jump = false;
    private bool _grapple = false;
    Vector2 move;
    Rigidbody2D rb;
    LayerMask groundedLayer;
    public DistanceJoint2D grapple;
    public LineRenderer grappleLine;
    

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        groundedLayer= LayerMask.GetMask("ground");

    }

    void FixedUpdate()
    {
        GroundCheck();
        Movement();
        PerformGrapple();
        WallGrabCheck();
    }

    // Update is called once per frame
    void Update()
    {
        move = new Vector2(Input.GetAxisRaw("Horizontal"), rb.velocity.y);
        if(Input.GetKeyDown(KeyCode.Space) == true)
        {
            _jump = true;
        }
        if(Input.GetKeyDown(KeyCode.R) == true){
            Reset();
        }
        if(Input.GetKeyDown(KeyCode.Escape) == true){
            Application.Quit();
        }
        if(Input.GetMouseButton(0) ==  true){_grapple = true;}
    }
    void JumpReset(){
        isJumping = false;
    }
    void GroundCheck(){
        hit = VisualPhysics2D.CircleCast(transform.position, 0.25f,-Vector2.up,groundedDistance, groundedLayer);
        if(hit == true){
            isGrounded = true;
            isGrappling = false;
            grappleUsed =  false;
            wallgrabUsed = false;
            wallgrabRefreshUsed = false;
            dashUsed = false;
            grapple.enabled = false;
            grappleLine.enabled = false;
            }
        else{Invoke("NotGrounded",0.2f);}
    }
    void NotGrounded(){
        isGrounded = false;
    }
    void Movement(){
        if(_jump && isGrounded == true && isJumping == false && isGrappling == false){
            isJumping = true;
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            
            Invoke("JumpReset", 0.5f);
        }
        else if(_jump && isGrappling == true){
            BreakGrapple();
        }
        else if(_jump && isGrounded == false && isGrappling == false && dashUsed == false){
            move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            rb.AddForce(move * dashForce, ForceMode2D.Impulse);
            dashUsed = true;
        }
        else if(isGrounded == true && isJumping == false){
            rb.velocityX = Input.GetAxisRaw("Horizontal") * speed;
        }
        else if(isGrappling){
            move = new Vector2(Input.GetAxisRaw("Horizontal"), 0);
            rb.AddForce(move * airSpeed);
        }
        else if(wallgrabUsed == false && isGrounded == false && Input.GetAxisRaw("Horizontal") == 1){
            grabPoint1 = VisualPhysics2D.Raycast(wallGrabRay1.position, wallGrabRay1.right, grabLength, groundedLayer);
            grabPoint2 = VisualPhysics2D.Raycast(wallGrabRay2.position, wallGrabRay2.right, grabLength, groundedLayer);
            if(grabPoint1 == true && grabPoint2 == true){
                rb.velocityY = 0f;
                rb.velocityX = 0f;
                isWallGrabbing = true;
                wallgrabRefreshUsed = true;
                if(wallgrabRefreshUsed == false){
                    dashUsed = false;
                }
            }
        }
        else if(wallgrabUsed == false && isGrounded == false && Input.GetAxisRaw("Horizontal") == -1){
            grabPoint1 = VisualPhysics2D.Raycast(wallGrabRay1.position, -wallGrabRay1.right, grabLength, groundedLayer);
            grabPoint2 = VisualPhysics2D.Raycast(wallGrabRay2.position, -wallGrabRay2.right, grabLength, groundedLayer);
            if(grabPoint1 == true && grabPoint2 == true){
                rb.velocityY = 0f;
                rb.velocityX = 0f;
                isWallGrabbing = true;
                wallgrabRefreshUsed = true;
                if(wallgrabRefreshUsed == false){
                    dashUsed = false;
                }
            }
        }
        else{
            rb.velocityX = Mathf.Lerp(rb.velocity.x, 0, 1 - Mathf.Pow(horDrag, Time.deltaTime));
        }
        _jump = false;
    }
    void WallGrabCheck(){
        if(isWallGrabbing == true){
            rb.gravityScale = 0f;
            if(Input.GetAxisRaw("Horizontal") == -1){
                grabPoint1 = VisualPhysics2D.Raycast(wallGrabRay1.position, -wallGrabRay1.right, grabLength, groundedLayer);
                grabPoint2 = VisualPhysics2D.Raycast(wallGrabRay2.position, -wallGrabRay2.right, grabLength, groundedLayer);
                if(grabPoint1 == false | grabPoint2 == false){
                    isWallGrabbing = false;
                    rb.gravityScale = 0f;
                    wallgrabUsed = true;
                }
            }
            else if(Input.GetAxisRaw("Horizontal") == 1){
                grabPoint1 = VisualPhysics2D.Raycast(wallGrabRay1.position, wallGrabRay1.right, grabLength, groundedLayer);
                grabPoint2 = VisualPhysics2D.Raycast(wallGrabRay2.position, wallGrabRay2.right, grabLength, groundedLayer);
                if(grabPoint1 == false | grabPoint2 == false){
                    isWallGrabbing = false;
                    rb.gravityScale = 0f;
                    wallgrabUsed = true;
                }
            }
            else{
                isWallGrabbing = false;
                rb.gravityScale = 5f;
                wallgrabUsed = true;
            }
        }
        else{
            rb.gravityScale = 5f;
        }    
    }
    void PerformGrapple(){
        grappleHit = VisualPhysics2D.Raycast(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position, grappleLength, groundedLayer);
        if(_grapple == true && isGrappling == false && isGrounded == false && grappleUsed == false && grappleHit == true)
        {
            print("Grapple!");
            grapplePoint = grappleHit.point;
            grapplePoint.z = 0f;
            grapple.connectedAnchor = grapplePoint;
            grappleLine.positionCount = 2;
            var grapplePoints = new Vector3[2];
            grapplePoints[0] = rb.position;
            grapplePoints[1] = grapplePoint;
            grappleLine.SetPositions(grapplePoints);
            grapple.enabled = true;
            isGrappling = true;
            grappleUsed = true;
        }
        else if(isGrappling == true){
            var grapplePoints = new Vector3[2];
            grapplePoints[0] = rb.position;
            grapplePoints[1] = grapplePoint;
            grappleLine.SetPositions(grapplePoints);
            grappleLine.enabled = true;
        }
        else if(isGrappling == false){
            grappleLine.positionCount = 0;
        }
        _grapple = false;
    }
    void BreakGrapple(){
        isGrappling = false;
        grapple.enabled = false;
        grappleLine.positionCount = 0;
        if(isGrounded == false){
            rb.AddForce(rb.velocity * grappleJump, ForceMode2D.Impulse);
        }

        if(isGrounded){
            grappleUsed = false;
        }
    }
    void Reset(){
        BreakGrapple();
        rb.position = resetPoint.position;
    }
}
