using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

[BoltGlobalBehaviour]
public class NetworkCallbacks : GlobalEventListener
{

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
        // randomise a spawn position
        Vector2 spawnPosition = new Vector2(Random.Range(11.5f, 12.5f), Random.Range(-11.5f, -12.5f));

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
