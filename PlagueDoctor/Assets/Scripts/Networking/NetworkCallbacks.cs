using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

[BoltGlobalBehaviour]
public class NetworkCallbacks : GlobalEventListener
{
    // The player's spawn point
    public Transform playerSpawnPoint;

    List<string> logMessages = new List<string>();

    /// <summary>
    /// Build the GUI
    /// </summary>
    void OnGUI()
    {
        // only display max the 5 latest log messages
        int maxMessages = Mathf.Min(5, logMessages.Count);

        GUILayout.BeginArea(new Rect(Screen.width / 2 - 200, Screen.height - 100, 400, 100), GUI.skin.box);

        for (int i = 0; i < maxMessages; ++i)
        {
            GUILayout.Label(logMessages[i]);
        }

        GUILayout.EndArea();
    }

    /// <summary>
    /// Instantiate a player when the scene completes loading
    /// </summary>
    /// <param name="scene">The Scene in question</param>
    public override void SceneLoadLocalDone(string scene)
    {
        GameObject spawner = GameObject.Find("PlayerSpawnPoint");
        // Find the spawn point
        playerSpawnPoint = spawner.transform;

        // randomise a spawn position
        Vector2 spawnPosition = playerSpawnPoint.position + new Vector3(Random.Range(-.5f, .5f), Random.Range(-.5f, .5f));

        // instantiate player
        BoltNetwork.Instantiate(BoltPrefabs.Player, spawnPosition, Quaternion.identity);
    }

    /// <summary>
    /// Respond to an event
    /// </summary>
    /// <param name="evnt">The received event</param>
    public override void OnEvent(LogEvent evnt)
    {
        logMessages.Insert(0, evnt.Message);
    }
}
