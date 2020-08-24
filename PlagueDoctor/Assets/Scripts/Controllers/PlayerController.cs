using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    [Tooltip("The camera's offset divider to the look direction")]
    public float cameraOffsetDivider = 10;
    private Vector3 cameraOffsetVect;

    [Tooltip("Is the player infected?")]
    public bool isInfected;


    [Tooltip("How long it takes for a player to become infected")]
    public float infectionTime = 2.0f;
    public float infectionTimer = 0f;

    [Tooltip("How long it takes for the player to treat a patient")]
    public float treatTime = 2.0f;
    private float treatTimer = 0f;

    [Tooltip("The treatment progress bar")]
    public Slider treatmentSlider;

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

    private Renderer rend;

    private bool treating = false;
    private GameObject closestPatient;

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
            mainCam = GameObject.FindGameObjectWithTag("MainCamera");
        }

        //state.AddCallback("PlayerColour", ColourChanged);
    }


    /// <summary>
    /// Handle input
    /// </summary>
    public override void SimulateOwner()
    {
        if (!treating)
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

            // Get the direction vector between the player and the mouse
            cameraOffsetVect = mouse - transform.position;

            if (Input.GetKeyDown(KeyCode.E))
            {
                closestPatient = FindClosestPatient();
                if ((closestPatient.transform.position - transform.position).sqrMagnitude < 0.5f && !treating && !closestPatient.GetComponent<PatientController>().treated)
                {
                    treating = true;
                }
            }

            if(Input.GetKeyDown(KeyCode.Escape))
            {
                BoltNetwork.Shutdown();
                SceneManager.LoadScene("SandBoxMenu", LoadSceneMode.Single);
            }
        }
    }

    /// <summary>
    /// Play footstep sounds
    /// </summary>
    /// <param name="evnt"></param>
    public override void OnEvent(Footstep evnt)
    {
        if(evnt.Play)
        {
            evnt.Entity.gameObject.GetComponentInChildren<AudioSource>().PlayDelayed(0.45f);
        }
        else
        {
            evnt.Entity.gameObject.GetComponentInChildren<AudioSource>().Stop();
        }
        
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

        isInfected = false;
        treatmentSlider.gameObject.SetActive(false);
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

            if(treating)
            {
                treatmentSlider.gameObject.SetActive(true);
                treatTimer += Time.deltaTime;
                treatmentSlider.value = treatTimer / treatTime;
                if (treatTimer > treatTime)
                {
                    PatientController patientController = closestPatient.GetComponent<PatientController>();
                    if (patientController.isInfected)
                    {
                        isInfected = true;
                    }
                    patientController.ClearSymptoms();
                    treatTimer = 0f;
                    treatTimer = 0;
                    treating = false;

                    treatmentSlider.gameObject.SetActive(false);
                }
            }
        }

        mainCam.transform.position = new Vector3(transform.position.x, transform.position.y, -10) + (cameraOffsetVect / cameraOffsetDivider);

        if (movementVector != new Vector2(0, 0) && !GetComponentInChildren<AudioSource>().isPlaying)
        {
            GetComponentInChildren<AudioSource>().PlayDelayed(0.45f);
            Footstep step = Footstep.Create(entity);
            step.Play = true;
            step.Entity = entity;
            step.Send();
        }
        else if (movementVector == new Vector2(0, 0) && GetComponentInChildren<AudioSource>().isPlaying)
        {
            GetComponentInChildren<AudioSource>().Stop();
            Footstep step = Footstep.Create(entity);
            step.Play = false;
            step.Entity = entity;
            step.Send();
        }

        if(infectionTimer > infectionTime)
        {
            isInfected = true;
            infectionTimer = 0f;
        }
    }

    public GameObject FindClosestPatient()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Patient");
        GameObject closest = null;

        float distance = Mathf.Infinity;
        Vector3 position = transform.position;

        foreach(GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject != gameObject)
        {
            if (collision.gameObject.GetComponent<PlayerController>().isInfected && !isInfected)
            {
                infectionTimer += Time.deltaTime;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        infectionTimer = 0;
    }
}
