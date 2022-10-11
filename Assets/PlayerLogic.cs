using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private Vector3 velocity = new Vector3(0, 0, 0);
    [SerializeField] private GameObject mapCoordinator;

    private Direction currentDirection = Direction.None;
    

    // Start is called before the first frame update

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (mapCoordinator.GetComponent<AudioSource>().isPlaying)
        {
            velocity.z = -mapCoordinator.GetComponent<MapLoader>().CalculatedSpeed;
            rigidBody.velocity = velocity;
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

            // control player's velocity based on given direction
            if (currentDirection == Direction.None)
            {
                velocity.x = 0;
            }
            else if (currentDirection == Direction.Left)
            {
                velocity.x = 30;
            }
            else if (currentDirection == Direction.Right)
            {
                velocity.x = -30;
            }

        }
        else
        {
            velocity.z = 0;
        }
        rigidBody.velocity = velocity;
    }
}
