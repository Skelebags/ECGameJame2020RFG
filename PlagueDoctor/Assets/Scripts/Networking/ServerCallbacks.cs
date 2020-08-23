using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

[BoltGlobalBehaviour(BoltNetworkModes.Server)]
public class ServerCallbacks : Bolt.GlobalEventListener
{
    /// <summary>
    /// Sends an event on connection
    /// </summary>
    /// <param name="connection">The connecting connection</param>
    public override void Connected(BoltConnection connection)
    {
        LogEvent log = LogEvent.Create();
        log.Message = string.Format("{0} connected", connection.RemoteEndPoint);
        log.Send();
    }

    /// <summary>
    /// Sends an event on disconnection
    /// </summary>
    /// <param name="connection">The disconnecting connection</param>
    public override void Disconnected(BoltConnection connection)
    {
        LogEvent log = LogEvent.Create();
        log.Message = string.Format("{0} disconnected", connection.RemoteEndPoint);
        log.Send();
    }
}
