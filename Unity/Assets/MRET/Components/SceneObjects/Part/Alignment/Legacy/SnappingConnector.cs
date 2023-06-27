// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using UnityEngine;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Part.Alignment
{
    public class SnappingConnector : MonoBehaviour
    {
        public enum ConnectorType { Male, Female };

        public ConnectorType connectorType = ConnectorType.Female;
        public SnappingConnector connectorToSnapTo
        {
            get
            {
                return touchingConnector;
            }
        }

        private SnappingConnector touchingConnector;
        private InteractablePart partToSnap;
        private Collider snapConnectorCollider;

        public void Snap()
        {
            partToSnap.transform.rotation = touchingConnector.transform.rotation * Quaternion.Inverse(transform.localRotation);
            partToSnap.transform.position = DetermineSnapPosition();
        }

        void Start()
        {
            partToSnap = GetComponentInParent<InteractablePart>();
        }

        void OnTriggerEnter(Collider other)
        {
            SnappingConnector touchingConn = other.GetComponent<SnappingConnector>();
            if (touchingConn && touchingConn.connectorType != connectorType)
            {
                touchingConnector = touchingConn;
                partToSnap.SetCurrentSnappingConnector(this);
            }
        }

        void OnTriggerExit(Collider other)
        {
            SnappingConnector touchingConn = other.GetComponent<SnappingConnector>();
            if (touchingConn != null && touchingConn == touchingConnector && touchingConn.connectorType != connectorType)
            {
                touchingConnector = null;
                partToSnap.UnsetSnappingConnector(this);
            }
        }

        private Vector3 DetermineSnapPosition()
        {
            float xOffset = transform.position.x - partToSnap.transform.position.x;
            float yOffset = transform.position.y - partToSnap.transform.position.y;
            float zOffset = transform.position.z - partToSnap.transform.position.z;

            //if (partToSnap.transform.position.x > transform.position.x)
            {
                xOffset -= 2 * xOffset;
            }

            //if (partToSnap.transform.position.y > transform.position.y)
            {
                yOffset -= 2 * yOffset;
            }

            //if (partToSnap.transform.position.z > transform.position.z)
            {
                zOffset -= 2 * zOffset;
            }

            return new Vector3(touchingConnector.transform.position.x + xOffset,
                               touchingConnector.transform.position.y + yOffset,
                               touchingConnector.transform.position.z + zOffset);
        }
    }
}