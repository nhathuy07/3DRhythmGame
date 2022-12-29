using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerLogic : MonoBehaviour
{

    //Enums
 
    enum Direction
    {
        Left,
        Right,
        None
    }

    public float speed;
    private bool canJump;
    private Tuple<float, float, float> lanes;
    [SerializeField] private float fallSpeedMultiplier;
    [SerializeField] private Collider jumpTrigger1;
    [SerializeField] private Collider jumpTrigger2;
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private Vector3 velocity = new Vector3(0, 0, 0);
    [SerializeField] private float jumpForce = 20;
    [SerializeField] private GameObject mapCoordinator;
    [SerializeField] private GameObject roadSegment;
    [SerializeField] private GameObject CoinObj;

    private Direction currentDirection = Direction.None;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject == roadSegment)
        {
            canJump = true;
            velocity.y = 0;
            Debug.Log("Landed");
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject == roadSegment)
        {
            canJump = false;
            velocity.y = 0;
            Debug.Log("Jumping");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        lanes = mapCoordinator.GetComponent<MapLoader>().Lanes;
        
    }

    // Update is called once per frame
    void Update()
    {
        // jumping
        if (Input.GetKeyDown(KeyCode.Space) & canJump)
        {
            velocity.y = jumpForce;
            canJump = false;
        }
        

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            currentDirection = Direction.Left;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            currentDirection = Direction.Right;
        }
        else
        {
            currentDirection = Direction.None;
        }
    }
    void FixedUpdate()
    {
        if (mapCoordinator.GetComponent<AudioSource>().isPlaying)
        {
            velocity.z = -mapCoordinator.GetComponent<MapLoader>().CalculatedSpeed;
            rigidBody.velocity = velocity;

            if (!canJump)
            {
                velocity += Vector3.up * Physics.gravity.y * fallSpeedMultiplier * Time.deltaTime;

            }
            

            // control player's velocity based on given direction
            if (currentDirection == Direction.None)
            {
                rigidBody.MovePosition(new Vector3(lanes.Item2, transform.position.y, transform.position.z));
                
            }
            else if (currentDirection == Direction.Left)
            {
                rigidBody.MovePosition(new Vector3(lanes.Item3, transform.position.y, transform.position.z));
            }
            else if (currentDirection == Direction.Right)
            {
                rigidBody.MovePosition(new Vector3(lanes.Item1, transform.position.y, transform.position.z));
            }
            Debug.Log(transform.position.x);
        }
        else
        {
            velocity.z = 0;
        }
        rigidBody.velocity = velocity;
        velocity = rigidBody.velocity;


    }
}
