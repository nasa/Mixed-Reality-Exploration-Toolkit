using UnityEngine;
using System.Collections.Generic;
using System.Threading;

public class MATLABCommandHandler : MonoBehaviour
{
    public enum MatlabCommandType { Legacy, JSON };

    public MatlabCommandType commandType = MatlabCommandType.JSON;
    public MATLABClient matlabClient;

    private bool isReceiving = false, receivingEnabled = false, sendingEnabled = false;
    private Queue<object> matlabCommands = new Queue<object>();

    public void Send(string lineToSend)
    {
        if (sendingEnabled)
        {
            string[] commandElements = lineToSend.Split(' ');
            if (commandElements.Length > 0)
            {
                LegacyMATLABCommand matlabCommand = new LegacyMATLABCommand();
                matlabCommand.command = commandElements[0];
                matlabCommand.arguments = new List<string>();
                for (int i = 1; i < commandElements.Length; i++)
                {
                    matlabCommand.arguments.Add(commandElements[i]);
                }
            }
        }
    }

    public void Send(JSONMATLABCommand commandToSend)
    {
        if (sendingEnabled)
        {
            if (commandToSend != null)
            {
                matlabClient.Send(commandToSend.ToJSON());
            }
        }
    }

    public void Send(LegacyMATLABCommand commandToSend)
    {
        if (sendingEnabled)
        {
            if (commandToSend.command != null && commandToSend.command != "")
            {
                string commandBundle = "CMD:;" + commandToSend.command;

                foreach (string argument in commandToSend.arguments)
                {
                    commandBundle = commandBundle + ":;" + argument;
                }

                matlabClient.Send(commandBundle);
            }
        }
    }

    public void EnableReceiver()
    {
        receivingEnabled = true;
    }

    public void DisableReceiver()
    {
        receivingEnabled = false;
    }

    public void EnableSender()
    {
        sendingEnabled = true;
    }

    public void DisableSender()
    {
        sendingEnabled = false;
    }

    private void Update()
    {
        // Update receiving status if necessary.
        if (receivingEnabled != isReceiving)
        {
            if (receivingEnabled)
            {
                stopReceivingNow = false;
                isReceiving = true;
                Receive();
            }
            else
            {
                stopReceivingNow = true;
            }
        }

        // Handle any outstanding commands.
        if (matlabCommands.Count > 0)
        {
            if (commandType == MatlabCommandType.JSON)
            {
                JSONMATLABResponse responseToHandle = (JSONMATLABResponse) matlabCommands.Dequeue();
                if (responseToHandle.cmd_id == 101)
                {

                }
            }
            else if (commandType == MatlabCommandType.Legacy)
            {
                LegacyMATLABCommand commandToHandle = (LegacyMATLABCommand) matlabCommands.Dequeue();
                switch (commandToHandle.command.ToUpper())
                {
                    case "LOCALPOSITION":
                        if (commandToHandle.arguments.Count == 4)
                        {
                            GameObject objectToControl = GameObject.Find(commandToHandle.arguments[0]);
                            if (objectToControl)
                            {
                                objectToControl.transform.localPosition = new Vector3(float.Parse(commandToHandle.arguments[1]),
                                    float.Parse(commandToHandle.arguments[2]), float.Parse(commandToHandle.arguments[3]));
                            }
                        }
                        break;

                    case "GLOBALPOSITION":
                        if (commandToHandle.arguments.Count == 4)
                        {
                            GameObject objectToControl = GameObject.Find(commandToHandle.arguments[0]);
                            if (objectToControl)
                            {
                                objectToControl.transform.position = new Vector3(float.Parse(commandToHandle.arguments[1]),
                                    float.Parse(commandToHandle.arguments[2]), float.Parse(commandToHandle.arguments[3]));
                            }
                        }
                        break;

                    case "LOCALROTATION":
                        if (commandToHandle.arguments.Count == 5)
                        {
                            GameObject objectToControl = GameObject.Find(commandToHandle.arguments[0]);
                            if (objectToControl)
                            {
                                objectToControl.transform.localRotation = new Quaternion(float.Parse(commandToHandle.arguments[1]),
                                    float.Parse(commandToHandle.arguments[2]), float.Parse(commandToHandle.arguments[3]), float.Parse(commandToHandle.arguments[4]));
                            }
                        }
                        break;

                    case "GLOBALROTATION":
                        if (commandToHandle.arguments.Count == 5)
                        {
                            GameObject objectToControl = GameObject.Find(commandToHandle.arguments[0]);
                            if (objectToControl)
                            {
                                objectToControl.transform.rotation = new Quaternion(float.Parse(commandToHandle.arguments[1]),
                                    float.Parse(commandToHandle.arguments[2]), float.Parse(commandToHandle.arguments[3]), float.Parse(commandToHandle.arguments[4]));
                            }
                        }
                        break;

                    case "SCALE":
                        if (commandToHandle.arguments.Count == 4)
                        {
                            GameObject objectToControl = GameObject.Find(commandToHandle.arguments[0]);
                            if (objectToControl)
                            {
                                objectToControl.transform.localScale = new Vector3(float.Parse(commandToHandle.arguments[1]),
                                    float.Parse(commandToHandle.arguments[2]), float.Parse(commandToHandle.arguments[3]));
                            }
                        }
                        break;

                    default:
                        Debug.LogWarning("[MATLABCommandHandler] Unknown MATLAB command.");
                        break;
                }
            }
        }
    }

    private bool stopReceivingNow = false;
    private void Receive()
    {
        Thread receiveThread = new Thread(()=>
        {
            if (commandType == MatlabCommandType.Legacy)
            {
                while (!stopReceivingNow)
                {
                    LegacyMATLABCommand commandToQueue = matlabClient.ReceiveLegacyCommand();
                    if (commandToQueue != null)
                    {
                        matlabCommands.Enqueue(commandToQueue);
                    }
                }
            }
            else if (commandType == MatlabCommandType.JSON)
            {
                while (!stopReceivingNow)
                {
                    JSONMATLABResponse responseToQueue = matlabClient.ReceiveJSONResponse();
                    if (responseToQueue != null)
                    {
                        matlabCommands.Enqueue(responseToQueue);
                    }
                }
            }
            
            isReceiving = false;
        });
        receiveThread.Start();
    }

    public void OnDestroy()
    {
        stopReceivingNow = true;
        DisableReceiver();
        receivingEnabled = false;
    }
}