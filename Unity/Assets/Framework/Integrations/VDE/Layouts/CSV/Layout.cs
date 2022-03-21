/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using Assets.VDE.UI;
using System.Linq;
using UnityEngine;

namespace Assets.VDE.Layouts.CSV
{
    public class Layout : Assets.VDE.Layouts.Layout
    {
        Log log;
        public Layout(string name) : base(name)
        {
            log = new Log("Layout " + name);
        }

        override internal Vector3 PositionShallowGroup(UI.Group.Container container)
        {
            Vector3 toReturn = Vector3.zero;
            container.SetPositionCorrection(Container.PositionCorrectionDirection.keepTheCentre);

            // this is weird.. but may happen in some conditions.
            if (container.entity.siblings is null || container.entity.siblings.Count() == 0)
            {
                return Vector3.zero;
            }

            if (container.immediateSiblings.Count() > 0)
            {
                toReturn = GetTier1ContainerPosition(container, GetRigidJointValueFromConf(UI.Joint.Type.MemberGroot, "JointMaxDistance") * 2);

                // this is set only for the tier1 groups so, that their positions wouldnt be reset to zero, when the group is being repositioned because their shapes change.
                container.resetToPosition = toReturn;

                container.AdoptParent(container.entity.parent, container.entity.parent.containers.GetGroup(this), UI.Joint.Type.MemberGroot);
                foreach (Entity sibling in container.entity.siblings.Where(ent => ent != container.entity))
                {
                    container.AdoptSibling(sibling, sibling.containers.GetGroup(this), UI.Joint.Type.MemberMemberLong);
                }
                container.doNotFreeze = true;
            }

            return toReturn;
        }
    }
}
