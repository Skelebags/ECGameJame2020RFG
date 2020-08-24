using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatientController : MonoBehaviour
{
    // How often to cough
    public float coughTime = 1.0f;
    private float coughTimer = 0;


    // The game's disease controller
    private DiseaseController diseaseController;

    // This patient's symptoms
    private int[] symptoms;

    [SerializeField]
    // The counter to cycle symptom icons
    private int iconCounter = 0;

    [SerializeField]
    // The timer to cycle symptoms
    private float iconTimer;

    // Does the patient have the plague?
    public bool isInfected = false;

    // Has the patient been treated?
    public bool treated = false;

    /// <summary>
    /// Run code on attach to entity
    /// </summary>
    void Start()
    {
        diseaseController = GetComponentInParent<DiseaseController>();

        if(Random.Range(1, 10) < diseaseController.plagueChance)
        {
            isInfected = true;
            symptoms = diseaseController.PlagueSymptoms;
        }
        else
        {
            isInfected = false;
            symptoms = new int[Random.Range(1, diseaseController.maxSymptomCount + 1)];

            int uniqueInts = 0;
            do
            {
                int randInt = Random.Range(0, diseaseController.symptoms.Count);
                bool unique = true;
                for (int i = 0; i < symptoms.Length; i++)
                {
                    if (randInt == symptoms[i])
                    {
                        unique = false;
                    }
                }
                if (unique)
                {
                    symptoms[uniqueInts] = randInt;
                    uniqueInts++;
                }
            } while (uniqueInts < symptoms.Length);
        }
    }

    public void Update()
    {
        if (!treated)
        {
            if (iconCounter >= symptoms.Length)
            {
                iconCounter = 0;
            }

            transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = diseaseController.symptoms[iconCounter];

            if (iconTimer < diseaseController.iconTime)
            {
                iconTimer += Time.deltaTime;
            }
            else
            {
                iconTimer = 0;
                iconCounter++;
            }

            if (coughTimer < coughTime)
            {
                coughTimer += Time.deltaTime;
            }
            else
            {
                coughTimer = 0;
                transform.GetChild(1).GetComponent<AudioSource>().Play();
            }
        }
    }

    public void ClearSymptoms()
    {
        treated = true;
        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = null;
    }
}
