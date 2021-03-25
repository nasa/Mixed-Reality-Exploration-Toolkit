// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using UnityEngine;
using System.Net.Sockets;
using System.Collections.Generic;

public class MATLABClient : MonoBehaviour
{
    public string host = "127.0.0.1";
    [Range(1, 65535)]
    public int port = 25525;

    private TcpClient client;
    private NetworkStream stream;

    public void Send(string content)
    {
        SendString(content);
    }

    public string Receive()
    {
        return ReceiveString();
    }

    public LegacyMATLABCommand ReceiveLegacyCommand()
    {
        if (client.Connected)
        {
            string[] receivedElements = ReceiveString().Split(new string[] { ":;" }, StringSplitOptions.None);
            if (receivedElements.Length > 1)
            {
                LegacyMATLABCommand commandToReturn = new LegacyMATLABCommand();
                commandToReturn.command = receivedElements[0];
                commandToReturn.destination = receivedElements[1] == "." ? null : receivedElements[1];

                commandToReturn.arguments = new List<string>();
                for (int i = 2; i < receivedElements.Length; i++)
                {
                    commandToReturn.arguments.Add(receivedElements[i]);
                }
                return commandToReturn;
            }
        }
        return null;
    }

    public JSONMATLABResponse ReceiveJSONResponse()
    {
        if (client.Connected)
        {
            string receivedCommand = ReceiveString();
            if (receivedCommand != null)
            {
                return JSONMATLABResponse.FromJSON(receivedCommand);
            }
        }
        return null;
    }

    public void Initialize()
    {
        DestroyConnection();

        if (host == "")
        {
            host = "127.0.0.1";
        }

        try
        {
            // Set up TCPClient.
            client = new TcpClient(host, port);

            // Connect to server.
            byte[] data = System.Text.Encoding.UTF8.GetBytes("Connecting\n");
            stream = client.GetStream();
            stream.Write(data, 0, data.Length);
            Debug.Log("[MATLABClient->Initialize] Connected!");
        }
        catch (Exception e)
        {
            Debug.Log("[MATLABClient->Initialize] " + e.ToString());
        }
    }

    private void DestroyConnection()
    {
        try
        {
            if (stream != null)
            {
                stream.Close();
            }

            if (client != null)
            {
                client.Close();
            }
        }
        catch (Exception e)
        {
            Debug.Log("[MATLABClient->DestroyConnection] " + e.ToString());
        }
    }

    private void SendString(string stringToSend)
    {
        try
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(stringToSend + "\n");
            stream = client.GetStream();
            stream.Write(data, 0, data.Length);
            Debug.Log("[MATLABClient->SendString] Sent!");
        }
        catch (Exception e)
        {
            Debug.Log("[MATLABClient->SendString] " + e.ToString());
        }
    }

    private string ReceiveString()
    {
        string returnString = "*error*";
        try
        {
            byte[] data = new byte[1024];
            stream = client.GetStream();
            stream.ReadTimeout = 500;

            int bytes = stream.Read(data, 0, data.Length);
            returnString = System.Text.Encoding.UTF8.GetString(data, 0, bytes);
            Debug.Log("got " + returnString);
            Debug.Log("[MATLABClient->ReceiveString] Received!");
        }
        catch (Exception e)
        {
            Debug.Log("[MATLABClient->ReceiveString] " + e.ToString());
        }

        return returnString;
    }

    public void OnDestroy()
    {
        DestroyConnection();
    }
}