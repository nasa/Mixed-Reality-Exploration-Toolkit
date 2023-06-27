﻿// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
#if MRET_EXTENSION_EASYBUILDSYSTEM
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Builder;
using EasyBuildSystem.Features.Scripts.Core.Base.Builder.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
#endif

namespace GOV.NASA.GSFC.XR.MRET.Extensions.EasyBuildSystem.Alignment
{
    public class SnappingManager : MRETManager<SnappingManager>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SnappingManager);

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            base.MRETStart();

#if MRET_EXTENSION_EASYBUILDSYSTEM

            // Initialize EBS parts collection.
            InitializeEBSPartCollection();
#else
            LogWarning("Easy Build System is unavailable", nameof(MRETStart));
#endif
        }

#if MRET_EXTENSION_EASYBUILDSYSTEM
        public void InitiatePreview(PieceBehaviour pb)
        {
            // If not already previewing, enter preview state.
            if (pb.CurrentState != StateType.Preview)
            {
                pb.ChangeState(StateType.Preview);

                BuilderBehaviour.Instance.CurrentMode = BuildMode.Placement;
                BuilderBehaviour.Instance.SelectPrefab(pb);
                BuilderBehaviour.Instance.SelectPrefab(pb);
                // Update Offsets.
            }
        }

        public void StopPreview(PieceBehaviour pb)
        {
            // If not already placed, place object.
            if (pb.CurrentState != StateType.Placed)
            {
                if (BuilderBehaviour.Instance.AllowPlacement)
                {
                    PieceBehaviour preview = BuilderBehaviour.Instance.CurrentPreview;
                    if (preview)
                    {
                        pb.transform.position = preview.transform.position;
                        pb.transform.rotation = preview.transform.rotation;
                        Destroy(preview.gameObject);
                    }

                    pb.ChangeState(StateType.Placed);
                    BuilderBehaviour.Instance.CurrentMode = BuildMode.None;

                    if (BuilderBehaviour.Instance.CurrentEditionPreview)
                    {
                        Destroy(BuilderBehaviour.Instance.CurrentEditionPreview.gameObject);
                    }

                    if (BuilderBehaviour.Instance.CurrentPreview)
                    {
                        Destroy(BuilderBehaviour.Instance.CurrentPreview.gameObject);
                    }

                    // Remove Offsets.
                }
            }
        }

        private void InitializeEBSPartCollection()
        {
            BuildManager.Instance.Pieces = new List<PieceBehaviour>();
        }
#endif
    }
}