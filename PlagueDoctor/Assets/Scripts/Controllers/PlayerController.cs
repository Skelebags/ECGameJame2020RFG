using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : Bolt.EntityBehaviour<IPlayerState>
{
    [Range(0, 50)]
    [Tooltip("The Player Speed")]
    public float speed = 5;

    [Range(0, 100)]
    [Tooltip("The Player Sight Radius")]
    public float sightRange = 40;

    [Range(0, 50)]
    [Tooltip("How many rays to fire to the sides of the main")]
    public int numRays = 4;

    [Range(0, 120)]
    [Tooltip("Player Field of View")]
    public float fov = 45;

    /// <summary>
    /// The player's rigidbody 2d component
    /// </summary>
    private Rigidbody2D rb {get; set; }

    /// <summary>
    /// The Player's movement vector
    /// </summary>
    private Vector2 movementVector;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        movementVector = Vector2.zero;
    }


    public override void Attached()
    {
        // Sync the transforms with the PlayerState transform
        state.SetTransforms(state.PlayerTransform, transform);

        // Randomise the player's colour
        if(entity.IsOwner)
        {
            switch(Random.Range(1, 3))
            {
                case 1:
                    state.PlayerColour = new Color(1, .3f, 1);
                    break;
                case 2:
                    state.PlayerColour = new Color(.3f, 1, 1);
                    break;
                case 3:
                    state.PlayerColour = new Color(1, 1, 1);
                    break;
            }
            
        }

        state.AddCallback("PlayerColour", ColourChanged);
    }


    /// <summary>
    /// Handle input
    /// </summary>
    public override void SimulateOwner()
    {
        // Get WASD or Arrow Key input, multiply by speed
        float x = Input.GetAxis("Horizontal") * speed;
        float y = Input.GetAxis("Vertical") * speed;

        // Build movement vector
        movementVector = new Vector2(x, y);

        // Change to per second
        movementVector *= BoltNetwork.FrameDeltaTime;

        transform.Translate(movementVector, Space.World);

        // Face mouse cursor
        Vector3 mouseScreen = Input.mousePosition;
        Vector3 mouse = Camera.main.ScreenToWorldPoint(mouseScreen);
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(mouse.y - transform.position.y, mouse.x - transform.position.x) * Mathf.Rad2Deg - 90);

    }

    /// <summary>
    /// Change the players material colour
    /// </summary>
    void ColourChanged()
    {
        GetComponent<Renderer>().material.color = state.PlayerColour;
    }

    /// <summary>
    /// Handle all physics updates
    /// </summary>
    private void FixedUpdate()
    {
        //RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, sightRange);

        //if (hit.collider != null)
        //{
        //    Debug.DrawRay(transform.position, hit.point - (Vector2)transform.position, Color.red);
        //}
        //else
        //{
        //    Debug.DrawRay(transform.position, transform.up, Color.red);
        //}

        for (int i = 0; i < numRays; i++)
        {
            Vector2 dir = Quaternion.AngleAxis(fov / 2 - (((2 * (float)i) / numRays) * fov), -transform.forward) * transform.up;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, sightRange);

            if (hit.collider != null)
            {
                Debug.DrawRay(transform.position, hit.point - (Vector2)transform.position, Color.red);
            }
            else
            {
                Debug.DrawRay(transform.position, transform.up, Color.red);
            }
            //Debug.DrawRay(transform.position, dir, Color.red);
            Debug.Log(i);
        }
    }
}
