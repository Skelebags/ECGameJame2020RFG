using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
using Bolt.Matchmaking;
using UdpKit;
using System;

public class Menu : GlobalEventListener
{
    /// <summary>
    /// Create the menu GUI
    /// </summary>
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height - 20));

        // START SERVER BUTTON
        if (GUILayout.Button("Start Server", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
        {
            // START SERVER
            BoltLauncher.StartServer();
        }

        // START CLIENT BUTTON
        if(GUILayout.Button("Start Client", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
        {
            // START CLIENT
            BoltLauncher.StartClient();
        }

        GUILayout.EndArea();
    }

    /// <summary>
    /// Runs on startup completion
    /// </summary>
    public override void BoltStartDone()
    {
        // If this instance is the server
        if (BoltNetwork.IsServer)
        {
            // Generate match ID
            string matchname = System.Guid.NewGuid().ToString();

            // Create matchmaking session
            BoltMatchmaking.CreateSession(
                sessionID: matchname,
                sceneToLoad: "LevelDevSandbox"
            );
        }
    }

    /// <summary>
    /// Updates the list of sessions
    /// </summary>
    /// <param name="sessionList">The map of match IDs to sessions</param>
    public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
    {
        // Debug output number of sessions
        Debug.LogFormat("Session list update: {0} total sessions", sessionList.Count);

        // Find and join an available session
        foreach (var session in sessionList)
        {
            UdpSession photonSession = session.Value as UdpSession;

            if (photonSession.Source == UdpSessionSource.Photon)
            {
                BoltMatchmaking.JoinSession(photonSession);
            }
        }
    }
}
