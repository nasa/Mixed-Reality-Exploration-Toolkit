/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using Assets.VDE.Communication;
using System;
using UnityEngine;

namespace Assets.VDE
{

    internal class Log
    {
        string entryPrefix = "Log";
        internal Messenger messenger;

        internal bool enabled = true;
        internal enum Event
        {
            NotSet,

            FromServer,
            ToServer,
            Lethal,
            HUD,
        }
        /// <summary>
        /// designating wether to log to Unity or send the entries to VDE server is done by implementing the Log using either with or without Messenger.
        /// </summary>
        /// <param name="prefix"></param>
        internal Log(string prefix)
        {
            entryPrefix = prefix;
        }
        /// <summary>
        /// designating wether to log to Unity or send the entries to VDE server is done by implementing the Log using either with or without Messenger.
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="messenger"></param>
        internal Log(string prefix, Messenger messenger)
        {
            entryPrefix = prefix;
            this.messenger = messenger;
        }
        internal void SetPrefix(string prefix)
        {
            entryPrefix = prefix;
        }
        internal void Entry(string entry)
        {
            if (enabled)// || (!(messenger is null) && !(messenger.data is null) && !(messenger.data.VDE is null) && messenger.data.VDE.debug))
            {
                Debug.Log(Format(entry)); 
            }
        }

        internal void SetMessenger(Messenger messenger)
        {
            this.messenger = messenger;
        }

        internal void Entry(string entry, Event logingEvent)
        {
            if (!(messenger is null))
            {
                messenger.Post(new Message()
                {
                    LogingEvent = logingEvent,
                    message = Format(entry)
                });
            }
            else
            {
                Entry("Messenger is undefined! Anyway: " + entry);
            }
        }

        string Format(string entry)
        {
            return $"{DateTime.Now:yyyyMMddHHmmss} | {entryPrefix}: {entry}";
        }
    }
}
