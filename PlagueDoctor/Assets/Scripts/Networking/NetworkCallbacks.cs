using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

[BoltGlobalBehaviour]
public class NetworkCallbacks : GlobalEventListener
{
    private GameObject ground;

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
}
