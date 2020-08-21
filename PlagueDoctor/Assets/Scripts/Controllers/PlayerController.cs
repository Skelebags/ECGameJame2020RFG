using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : Bolt.EntityBehaviour<IPlayerState>
{
    [Tooltip("The Player Speed")]
    public float speed;

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
    }

    void ColourChanged()
    {
        GetComponent<Renderer>().material.color = state.PlayerColour;
    }
}
