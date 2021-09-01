using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
//using Valve.VR;

namespace Assets.VDE.UI.Input
{
    internal class ViveObserver : Observer
    {
#if BUILD_FOR_VIVE
        //public SteamVR_ActionSet activateActionSetOnAttach = SteamVR_Input.GetActionSet("/actions/VDE",false);
        internal SteamVR_Action_Vector2 thumbPositionOnTouch    = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("default", "ThumbOnTouch");
        internal SteamVR_Action_Single  squeeze                 = SteamVR_Input.GetAction<SteamVR_Action_Single>("default", "Squeeze");
        internal SteamVR_Action_Boolean grabGrip                = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("default", "GrabGrip");

        private void Awake()
        {
            //activateActionSetOnAttach.Activate(SteamVR_Input_Sources.Any);
        }

        private void Update()
        {
            SteamVR_Input_Sources hand = SteamVR_Input_Sources.RightHand;// Valve.VR.InteractionSystem.Hand. interactable.attachedToHand.handType;
            
            Vector2 thumbPositionOnTouchV2 = thumbPositionOnTouch.GetAxis(SteamVR_Input_Sources.RightHand);
            float triggerSqueeze = squeeze.GetAxis(hand);
            bool gripping = grabGrip.GetState(hand);

            if (thumbPositionOnTouchV2.y != 0)
            {
                inputEvent.Invoke(new Input.Event
                {
                    function = Input.Event.Function.Move,
                    type = Input.Event.Type.Vector2,
                    Vector2 = Vector2.up * thumbPositionOnTouchV2
                });
            }
        }
#endif
    }
}
