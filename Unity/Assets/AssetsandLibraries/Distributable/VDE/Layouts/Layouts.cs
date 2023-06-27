/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using Assets.VDE.Communication;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.VDE.Layouts
{
    public class Layouts
    {
        Log log;
        Data data;
        internal Layout current;
        internal enum LayoutEvent
        {
            NotSet,
            ActivateLayout,

            Ready,
            Reshaping,
            Repositioning,

            GotJoint,
            GotRigid,
            GotCollider,
            GotContainer,
            GotVisibleShape,

            Subscribe,
            Unsubscribe,
            Reshaped,

            CollidingWith,
            StoppedCollidingWith,
            HasSettled,
            LayoutReady,
            LayoutPopulated,
            AdjustForFPS,
            InitializeAll,
            EntitiesReady,
            LinksReady
        }
        internal enum EventOrigin
        {
            Joint,
            Shape,
            Group,
            Entity,
            Node
        }
        internal Dictionary<string, Layout> layouts = new Dictionary<string, Layout> { };
        internal Layouts(Data data)
        {
            log = new Log("VDE.Layouts", data.messenger);
            this.data = data;
        }
        internal void InitializeLayouts()
        {
            data.config.layouts.Keys.ToList().ForEach(layout => InitializeLayout(layout));
            SetActiveLayout(data.config.VDE["defaultLayout"]);
        }
        /// <summary>
        /// this will (try to) load the Layout requested in the configuration. if that is not found in the current build, will yell back at the VDE server.
        /// </summary>
        /// <param name="layoutName"></param>
        /// <returns></returns>
        Layout InitializeLayout(string layoutName)
        {
            if (
                data.config.layouts != null && 
                data.config.layouts.ContainsKey(layoutName) 
                )
            {
                try
                {
                    Type layoutToUse = Type.GetType("Assets.VDE.Layouts." + layoutName + ".Layout");
                    if (layoutToUse != null)
                    {
                        if (!layouts.ContainsKey(layoutName))
                        {
                            layouts.Add(layoutName, (Layout)Activator.CreateInstance(layoutToUse,new string[1] { layoutName } ));
                            layouts[layoutName].Init(data);
                            return layouts[layoutName];
                        } else
                        {
                            // this is quite common, if config is reloaded, so no fuss is to be had because of this.
                            return layouts[layoutName];
                        }
                    } else if(layoutName != "default")
                    {
                        log.Entry("No such layout included in the build nor found in modules: " + layoutName, Log.Event.ToServer);
                    }
                }
                catch (ArgumentNullException exe)
                {
                    log.Entry("Unable to find layout: " + layoutName + " (" + exe.Message + ")\n" + exe.StackTrace, Log.Event.ToServer);
                }
                catch (Exception exe)
                {
                    log.Entry("Unable to find layout: " + layoutName + " (" + exe.Message + ")\n" + exe.StackTrace, Log.Event.ToServer);
                }
            }
            else
            {
                log.Entry("no such layout defined: " + layoutName, Log.Event.ToServer);
            }
            return null;
        }

        internal void SetActiveLayout(string layoutName)
        {
            if (!layouts.ContainsKey(layoutName))
            {
                InitializeLayout(layoutName);
            }
            // not else, as InitializeLayout may be unsuccessful in initializing a layout, hence we check it here again,
            if (layouts.ContainsKey(layoutName))
            {
                current = layouts[layoutName];
                data.messenger.Post(new Message()
                {
                    LayoutEvent = LayoutEvent.ActivateLayout,
                    obj = new object[2] { current, true },
                    from = data.entities.Get(0),
                    to = data.entities.Get(0)
                });
            }
        }
    }
}
