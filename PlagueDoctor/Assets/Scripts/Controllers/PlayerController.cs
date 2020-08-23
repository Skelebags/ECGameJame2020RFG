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

    [Tooltip("The Material used by the vision cone")]
    public Material visionMaterial;

    /// <summary>
    /// The player's rigidbody 2d component
    /// </summary>
    private Rigidbody2D rb {get; set; }

    /// <summary>
    /// The Player's movement vector
    /// </summary>
    private Vector2 movementVector;

    private Vector2[] visionVerts2D;
    private Vector3[] visionVerts3D;
    private int[] visionTris;
    private GameObject visionCone;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        movementVector = Vector2.zero;
        visionCone = new GameObject();
        visionCone.AddComponent(typeof(MeshRenderer));
        visionCone.AddComponent(typeof(MeshFilter));
        visionCone.GetComponent<MeshRenderer>().sortingLayerName = "Vision";
        visionCone.GetComponent<MeshRenderer>().sortingOrder = 0;

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

        // Initialise the visionVerts array
        visionVerts2D = new Vector2[numRays + 1];
        // Cast the vision rays
        for (int i = 0; i < numRays; i++)
        {
            // Get the ray's direction
            Vector2 dir = Quaternion.AngleAxis(fov / 2 - (((2 * (float)i) / numRays) * fov), -transform.forward) * transform.up;
            
            // Cast the ray
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, sightRange);

            // Check for a hit, put the hit point into the vert array OR put the ray's end point into the vert array
            if (hit.collider != null)
            {
                Debug.DrawRay(transform.position, hit.point - (Vector2)transform.position, Color.red);
                visionVerts2D[i] = hit.point;
            }
            else
            {
                Debug.DrawRay(transform.position, dir.normalized * sightRange, Color.red);
                visionVerts2D[i] = (Vector2)transform.position + dir * sightRange;
            }
        }

        // The last vertice is the player's position
        visionVerts2D[numRays] = transform.position;

    }

    private void Update()
    {
        // CREATE VISION CONE

        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(visionVerts2D);
        visionTris = tr.Triangulate();

        // Create the Vector3 vertices and indices
        visionVerts3D = new Vector3[visionVerts2D.Length];
        for (int i = 0; i < visionVerts3D.Length; i++)
        {
            visionVerts3D[i] = new Vector3(visionVerts2D[i].x, visionVerts2D[i].y, -1);
        }


        // Create the mesh
        Mesh visionMesh = visionCone.GetComponent<MeshFilter>().mesh;

        visionMesh.Clear();

        visionMesh.vertices = visionVerts3D;
        visionMesh.triangles = visionTris;
        visionMesh.RecalculateNormals();
        visionMesh.RecalculateBounds();

        // Apply the mesh
        MeshFilter filter = visionCone.GetComponent<MeshFilter>();
        filter.mesh = visionMesh;

        // Apply the visionCone Material
        visionCone.GetComponent<MeshRenderer>().material = visionMaterial;
    }
}
