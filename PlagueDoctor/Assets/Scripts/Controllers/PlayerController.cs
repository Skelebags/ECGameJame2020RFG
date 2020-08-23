using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : Bolt.EntityEventListener<IPlayerState>
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

    [Range(0, 20)]
    [Tooltip("Player's 360 vision radius")]
    public float visionRadius = 1;

    [Range(10, 360)]
    [Tooltip("The number of vertices on the vision radius mesh")]
    public int radiusNumVerts = 10;

    [Tooltip("The Material used by the vision cone")]
    public Material visionMaterial;

    // The player's camera
    private GameObject mainCam;

    /// <summary>
    /// The player's rigidbody 2d component
    /// </summary>
    private Rigidbody2D rb {get; set; }

    /// <summary>
    /// The Player's movement vector
    /// </summary>
    private Vector2 movementVector;

    /// <summary>
    /// Arrays and objects for building vision cone
    /// </summary>
    private Vector2[] visionVerts2D;
    private Vector3[] visionVerts3D;
    private int[] visionTris;
    private GameObject visionCone;

    /// <summary>
    /// Arrays and objects for building vision cone
    /// </summary>
    private Vector2[] radiusVerts2D;
    private Vector3[] radiusVerts3D;
    private int[] radiusTris;
    private GameObject radiusObj;

    private float resetColourTime;
    private Renderer rend;

    // NETWORK CODE //
    /// <summary>
    /// Run code on attach to entity
    /// </summary>
    public override void Attached()
    {
        // Sync the transforms with the PlayerState transform
        state.SetTransforms(state.PlayerTransform, transform);

        rend = GetComponent<Renderer>();

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

            mainCam = GameObject.FindGameObjectWithTag("MainCamera");
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

        if (Input.GetKeyDown(KeyCode.F))
        {
            FlashColourEvent flash = FlashColourEvent.Create(entity);
            flash.FlashColour = Color.red;
            flash.Send();
        }
    }

    /// <summary>
    /// Change the players material colour
    /// </summary>
    void ColourChanged()
    {
        GetComponent<Renderer>().material.color = state.PlayerColour;
    }

    /// <summary>
    /// Run on receive FlashColourEvent
    /// </summary>
    public override void OnEvent(FlashColourEvent evnt)
    {
        resetColourTime = Time.time + 0.2f;
        rend.material.color = evnt.FlashColour;
    }


    // STANDALONE CODE //
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        movementVector = Vector2.zero;

        visionCone = new GameObject();
        visionCone.layer = 2;
        visionCone.AddComponent(typeof(MeshRenderer));
        visionCone.AddComponent(typeof(MeshFilter));
        visionCone.GetComponent<MeshRenderer>().sortingLayerName = "Vision";
        visionCone.GetComponent<MeshRenderer>().sortingOrder = 0;

        radiusObj = new GameObject();
        radiusObj.layer = 2;
        radiusObj.AddComponent(typeof(MeshRenderer));
        radiusObj.AddComponent(typeof(MeshFilter));
        radiusObj.GetComponent<MeshRenderer>().sortingLayerName = "Vision";
        radiusObj.GetComponent<MeshRenderer>().sortingOrder = 0;
    }


    /// <summary>
    /// Handle all physics updates
    /// </summary>
    private void FixedUpdate()
    {
        if (entity.IsOwner)
        {
            // Initialise the visionVerts array
            visionVerts2D = new Vector2[numRays + 1];
            // Cast the vision rays
            for (int i = 0; i < numRays; i++)
            {
                // Get the ray's direction
                Vector2 dir = Quaternion.AngleAxis(-fov/2 + (((float)i/(float)numRays) * fov), -transform.forward) * transform.up;

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


            // Initialise the radiusVerts array
            radiusVerts2D = new Vector2[radiusNumVerts];
            // Cast the vision rays
            for (int i = 0; i < radiusNumVerts; i++)
            {
                // Get the ray's direction
                Vector2 dir = Quaternion.AngleAxis(-360 / 2 + (((float)i / (float)radiusNumVerts) * 360), -transform.forward) * transform.up;

                // Cast the ray
                RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, visionRadius);

                // Check for a hit, put the hit point into the vert array OR put the ray's end point into the vert array
                if (hit.collider != null)
                {
                    Debug.DrawRay(transform.position, hit.point - (Vector2)transform.position, Color.red);
                    radiusVerts2D[i] = hit.point;
                }
                else
                {
                    Debug.DrawRay(transform.position, dir.normalized * visionRadius, Color.red);
                    radiusVerts2D[i] = (Vector2)transform.position + dir * visionRadius;
                }
            }
        }
    }

    private void Update()
    {
        // CREATE VISION CONE
        if (entity.IsOwner)
        {
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

            // CREATE VISIONRADIUS
            // Use the triangulator to get indices for creating triangles
            Triangulator radtr = new Triangulator(radiusVerts2D);
            radiusTris = radtr.Triangulate();

            // Create the Vector3 vertices and indices
            radiusVerts3D = new Vector3[radiusVerts2D.Length];
            for (int i = 0; i < radiusVerts3D.Length; i++)
            {
                radiusVerts3D[i] = new Vector3(radiusVerts2D[i].x, radiusVerts2D[i].y, -1);
            }


            // Create the mesh
            Mesh radiusMesh = radiusObj.GetComponent<MeshFilter>().mesh;

            radiusMesh.Clear();

            radiusMesh.vertices = radiusVerts3D;
            radiusMesh.triangles = radiusTris;
            radiusMesh.RecalculateNormals();
            radiusMesh.RecalculateBounds();

            // Apply the mesh
            MeshFilter radiusFilter = radiusObj.GetComponent<MeshFilter>();
            radiusFilter.mesh = radiusMesh;

            // Apply the visionCone Material
            radiusObj.GetComponent<MeshRenderer>().material = visionMaterial;
        }

        if (resetColourTime < Time.time)
        {
            rend.material.color = state.PlayerColour;
        }

        mainCam.transform.position = new Vector3(transform.position.x, transform.position.y, mainCam.transform.position.z);
    }
}
