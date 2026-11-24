using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerScript : MonoBehaviour
{
    private int moveInput;
    [SerializeField] float moveSpeed;
    [SerializeField] float accellerationSpeed;
    private Vector2 currentMovement;
    private Rigidbody2D rb;
    private CircleCollider2D col;
    private bool touchingGround;
    [SerializeField] float floorRaycastDist;
    [SerializeField] float wallRaycastDist;
    [SerializeField] float wallCheckRadius;
    [SerializeField] float tileCollisionCheckRadius;
    [SerializeField] float jumpPower;
    [SerializeField] LayerMask playerLayer;
    private bool choosingBounce;
    [SerializeField] float bounceMulitplier;

    [SerializeField] Tilemap map;
    [SerializeField] List<TileData> TileDatas;
    private Dictionary<TileBase, TileData> dataFromTiles;

    [SerializeField] GameObject TestingDot;
    private Vector2 lastCheckedSpot;
    [SerializeField] float distChangeThresholdForCheckingTiles;
    [SerializeField] float bouncingCooldown;
    private float currentBounceCooldown;


    [SerializeField] Animator animator;
    private bool ignoreFlip;    
    
    // Start is called before the first frame update
    void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        col = gameObject.GetComponent<CircleCollider2D>();
        dataFromTiles = new Dictionary<TileBase, TileData>();

        foreach(var TileData in TileDatas)
        {
            foreach(var tile in TileData.tiles)
            {
                dataFromTiles.Add(tile, TileData);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        moveInput = (int)Input.GetAxisRaw("Horizontal");
        currentMovement.y = rb.velocity.y;
        currentMovement = Vector2.MoveTowards(currentMovement, new Vector2(moveInput * moveSpeed, currentMovement.y), accellerationSpeed * Time.deltaTime);

        rb.velocity = currentMovement;

        RaycastHit2D groundHit = Physics2D.Raycast(transform.position, -transform.up, floorRaycastDist, ~playerLayer);
            Debug.DrawRay(transform.position, -transform.up * floorRaycastDist, Color.red);
        // Debug.Log(hit == true);
        touchingGround = groundHit;
        
        RaycastHit2D wallHit = Physics2D.CircleCast(transform.position, wallCheckRadius, new Vector2(currentMovement.x, 0).normalized, wallRaycastDist, ~playerLayer);
            Debug.DrawCircleCast(transform.position, wallCheckRadius, new Vector2(currentMovement.x, 0).normalized, 10, Color.red);
            Debug.DrawRay(transform.position, new Vector2(currentMovement.x, 0).normalized, Color.red);
            // Debug.DrawLine(transform.position, new Vector2(currentMovement.x, 0).normalized, Color.red);

        if(wallHit)
            currentMovement.x = 0;

        RaycastHit2D[] tileHit = Physics2D.CircleCastAll(transform.position, tileCollisionCheckRadius, new Vector2(), 0, ~playerLayer);
        // Debug.Log(tileHit.Length);

        foreach(RaycastHit2D hit in tileHit)
        {
            if(hit.collider.gameObject.layer == 3)
                RunTileEffects(hit.point - hit.normal * .01f);  
        }

        if(Input.GetKeyDown(KeyCode.F) && currentBounceCooldown >= bouncingCooldown)
        {
            
            Time.timeScale = 0;
            choosingBounce = true;
        }
        else if(Input.GetKeyUp(KeyCode.F))
            BounceBack();

        if(!choosingBounce)
        {
            if(Input.GetKeyDown(KeyCode.Space) && touchingGround)
            {
                rb.AddForce(transform.up * jumpPower, ForceMode2D.Impulse);
                animator.SetTrigger("jump");    
            }

            // Debug.Log(moveInput);
        }
        else
        {
            if(Input.GetKeyDown(KeyCode.A))
                BounceBack(new Vector2(-1, 0));
            else if(Input.GetKeyDown(KeyCode.D))
                BounceBack(new Vector2(1, 0));
            else if(Input.GetKeyDown(KeyCode.S))
                BounceBack(new Vector2(0, -1));
            else if(Input.GetKeyDown(KeyCode.W))
                BounceBack(new Vector2(0, 1));
        }

        Animate();
    }

    void BounceBack(Vector2 bounceDirection = new Vector2())
    {
        AnimateBounce(bounceDirection);

        choosingBounce = false;
        Time.timeScale = 1;

        if(bounceDirection.x * currentMovement.x > 0)
            currentMovement.x *= -bounceMulitplier;
        else if(bounceDirection.y * currentMovement.y > 0)
            currentMovement.y *= -bounceMulitplier;

        rb.velocity = currentMovement;
    }

    TileData GetTileDataFromFloor(RaycastHit2D hitPoint)
    {
        var tilePos = hitPoint.point - new Vector2(0, .01f);
        Vector3Int cellPosition = map.WorldToCell(tilePos);
        TileBase collidedTile = map.GetTile(cellPosition);
        Debug.Log(dataFromTiles[collidedTile]?.SpeedDirection);

        return null;
    }

    // void OnCollisionEnter2D(Collision2D collision)
    // {
    //     // Debug.Log(collision.GetContact(0).normal);
    //     // if(collision.gameObject.layer == 3)
    //     //     Vector2 hitPoint = collision.GetContact(0).point - collision.GetContact(0).normal * .01f;
        
    // }

    void RunTileEffects(Vector2 checkingPoint)
    {
        Vector3Int cellPosition = map.WorldToCell(checkingPoint);
        TileBase collidedTile = map.GetTile(cellPosition);

        if(collidedTile != null && (checkingPoint - lastCheckedSpot).magnitude >= distChangeThresholdForCheckingTiles)
        {
            // Instantiate(TestingDot, checkingPoint, transform.rotation);
            lastCheckedSpot = checkingPoint;
            currentMovement += dataFromTiles[collidedTile].SpeedDirection;
        }

    }
    
#region animationing

    void Animate()
    {
        animator.SetFloat("runningSpeed", Mathf.Abs(currentMovement.x));
        animator.SetFloat("yVelocity", currentMovement.y);
        animator.SetBool("touchingGround", touchingGround);
        animator.SetBool("choosingBounce", choosingBounce);

        if(!(animator.GetCurrentAnimatorStateInfo(0).IsName("bounceSideAnimation") || ignoreFlip))
        {
            if(currentMovement.x > 0)
                animator.gameObject.GetComponent<SpriteRenderer>().flipX = false;
            else if(currentMovement.x < 0)
                animator.gameObject.GetComponent<SpriteRenderer>().flipX = true;
        }
        else if(animator.GetCurrentAnimatorStateInfo(0).IsName("bounceSideAnimation"))
            ignoreFlip = false;
    }

    void AnimateBounce(Vector2 bounceDirection)
    {
        if(bounceDirection.y < 0)
            animator.SetTrigger("bounceUp");
        else if(bounceDirection.y > 0)
            animator.SetTrigger("bounceDown");
        else if(bounceDirection.x != 0)
        {
            animator.SetTrigger("bounceSide");
            animator.gameObject.GetComponent<SpriteRenderer>().flipX = bounceDirection.x < 0;
            ignoreFlip = true;
        }
    }

#endregion animationing
}
