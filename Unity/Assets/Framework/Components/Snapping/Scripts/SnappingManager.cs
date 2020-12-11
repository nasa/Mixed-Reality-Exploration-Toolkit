using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyBuildSystem.Runtimes.Internal.Managers;
using EasyBuildSystem.Runtimes.Internal.Managers.Data;
using EasyBuildSystem.Runtimes.Internal.Part;
using EasyBuildSystem.Runtimes.Internal.Socket;
using EasyBuildSystem.Runtimes.Extensions;
using EasyBuildSystem.Runtimes.Internal.Socket.Data;
using EasyBuildSystem.Runtimes.Internal.Builder;

namespace GSFC.ARVR.MRET.Infrastructure.Integrations.Snapping
{
    public class SnappingManager : MonoBehaviour
    {
        public static SnappingManager instance;

        void Start()
        {
            // Set static instance.
            instance = this;

            // Initialize EBS parts collection.
            InitializeEBSPartCollection();
        }
        
        public void InitiatePreview(PartBehaviour pb)
        {
            // If not already previewing, enter preview state.
            if (pb.CurrentState != StateType.Preview)
            {
                pb.ChangeState(StateType.Preview);

                BuilderBehaviour.Instance.CurrentMode = BuildMode.Placement;
                BuilderBehaviour.Instance.SelectPrefab(pb);
                BuilderBehaviour.Instance.SelectedPrefab = pb;
                // Update Offsets.
            }
        }

        public void StopPreview(PartBehaviour pb)
        {
            // If not already placed, place object.
            if (pb.CurrentState != StateType.Placed)
            {
                if (BuilderBehaviour.Instance.AllowPlacement)
                {
                    PartBehaviour preview = BuilderBehaviour.Instance.CurrentPreview;
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
            BuildManager.Instance.PartsCollection = ScriptableObject.CreateInstance<PartsCollection>();
            BuildManager.Instance.PartsCollection.Parts = new List<PartBehaviour>();
        }
    }
}