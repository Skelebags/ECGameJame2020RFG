using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiseaseController : MonoBehaviour
{
    [Tooltip("The array of sprites to be displayed for each symptom")]
    public Sprite[] symptomSprites;

    [Tooltip("The number of syptoms that the plague will have")]
    public int plagueSymptomCount = 3;

    public int[] PlagueSymptoms { get; private set; }

    // Dictionary of symptom ids and their sprites
    public Dictionary<int, Sprite> symptoms;

    // Start is called before the first frame update
    void Start()
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
