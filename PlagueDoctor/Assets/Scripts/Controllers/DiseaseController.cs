using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiseaseController : MonoBehaviour
{
    [Tooltip("The array of sprites to be displayed for each symptom")]
    public Sprite[] symptomSprites;

    [Tooltip("The number of syptoms that the plague will have")]
    public int plagueSymptomCount = 3;

    [Tooltip("The maximum number of symptoms per patient. DO NOT MAKE THIS LARGER THAN THE NUMBER OF SYMPTOMS!!")]
    public int maxSymptomCount = 2;

    [Range(0, 10)]
    [Tooltip("The plague infection chance per patient")]
    public int plagueChance = 2;

    [Range(0, 5)]
    [Tooltip("How long symptom icons linger")]
    public float iconTime = 1f;

    public int[] PlagueSymptoms { get; private set; }

    [Tooltip("The patient prefab")]
    public GameObject patientPrefab;

    // Dictionary of symptom ids and their sprites
    public Dictionary<int, Sprite> symptoms;

    // Start is called before the first frame update
    void Awake()
    {
        symptoms = new Dictionary<int, Sprite>();

        for(int i = 0; i < symptomSprites.Length; i++)
        {
            symptoms.Add(i, symptomSprites[i]);
        }



        PlagueSymptoms = new int[plagueSymptomCount];


        int uniqueInts = 0;
        do
        {
            int randInt = Random.Range(0, symptoms.Count);
            bool unique = true;
            foreach (int number in PlagueSymptoms)
            {
                if (randInt == number)
                {
                    unique = false;
                }
            }
            if (unique)
            {
                PlagueSymptoms[uniqueInts] = randInt;
                uniqueInts++;
            }
        } while (uniqueInts < plagueSymptomCount);
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < plagueSymptomCount; i++)
        {
            Debug.Log(PlagueSymptoms[i]);
        }
    }
}
