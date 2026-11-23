using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerScript : MonoBehaviour
{
    private int moveInput;
    [SerializeField] float moveSpeed;
    [SerializeField] float accellerationSpeed;
    private Vector2 currentMovement;
    public Rigidbody2D rb;
    private bool touchingGround;
    [SerializeField] float floorRaycastDist;
    [SerializeField] float wallRaycastDist;
    [SerializeField] float jumpPower;
    [SerializeField] LayerMask playerLayer;
    private bool choosingBounce;
    [SerializeField] float bounceMulitplier;

    [SerializeField] Tilemap map;
    [SerializeField] List<TileData> TileDatas;
    private Dictionary<TileBase, TileData> dataFromTiles;

    [SerializeField] GameObject TestingDot;
    
    
    // Start is called before the first frame update
    void Awake()
    {
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

        RaycastHit2D hit = Physics2D.Raycast(transform.position, -transform.up, floorRaycastDist, ~playerLayer);
            Debug.DrawRay(transform.position, -transform.up * floorRaycastDist, Color.red);
        // Debug.Log(hit == true);
        touchingGround = hit;
        
        RaycastHit2D otherHit = Physics2D.Raycast(transform.position, new Vector2(currentMovement.x, 0).normalized, wallRaycastDist, ~playerLayer);
            Debug.DrawRay(transform.position, new Vector2(currentMovement.x, 0).normalized, Color.red);

        if(otherHit)
            currentMovement.x = 0;

        if(Input.GetKeyDown(KeyCode.F))
        {
            // Debug.Log("Running");
            Time.timeScale = 0;
            choosingBounce = true;
        }
        else if(Input.GetKeyUp(KeyCode.F))
            BounceBack();

        if(!choosingBounce)
        {
            if(Input.GetKeyDown(KeyCode.Space) && touchingGround)
                rb.AddForce(transform.up * jumpPower, ForceMode2D.Impulse);

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
    }

    void BounceBack(Vector2 bounceDirection = new Vector2())
    {
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(collision.GetContact(0).normal);
        if(collision.gameObject.layer == 3)
        {
            Vector2 hitPoint = collision.GetContact(0).point - collision.GetContact(0).normal * .01f;
            Vector3Int cellPosition = map.WorldToCell(hitPoint);
            TileBase collidedTile = map.GetTile(cellPosition);

            if (collidedTile != null)
            {
                // if(!collidedTiles.Contains(collidedTile))
                //     collidedTiles.Add(collidedTile);
                // Debug.Log(dataFromTiles[collidedTile]?.SpeedDirection);
                currentMovement += dataFromTiles[collidedTile].SpeedDirection;
                
                // Debug.Log(rb.velocity);
            }

        }
        
    }

    void RunTileEffects(Vector2 checkingPoint)
    {
        
    }

    // void OnCollisionExit2D(Collision2D collision)
    // {
    //     if(collision.gameObject.layer == 3)
    //     {
    //         Vector2 hitPoint = collision.GetContact(0).point - collision.GetContact(0).normal * .01f;
    //         Vector3Int cellPosition = map.WorldToCell(hitPoint);
    //         TileBase collidedTile = map.GetTile(cellPosition);

    //         if (collidedTile != null)
    //         {
    //             collidedTiles.Remove(collidedTile);
    //             // Debug.Log(dataFromTiles[collidedTile]?.SpeedDirection);
    //             // additionalImpulse = dataFromTiles[collidedTile].SpeedDirection;
                
    //             // Debug.Log(rb.velocity);
    //         }
    //     }
        
    // }
}
