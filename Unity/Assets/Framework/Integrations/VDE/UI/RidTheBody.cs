using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.VDE.UI
{
    class RidTheBody : MonoBehaviour
    {
        internal Vector3 position;
        internal float radius;

        private void Update()
        {
            if (TryGetComponent(out Rigidbody uselessRB))
            {
                if (transform.localPosition == position)
                {
                    //Destroy(uselessRB);
                    uselessRB.mass = 0;
                    uselessRB.isKinematic = true;
                    uselessRB.useGravity = false;
                    uselessRB.angularDrag = 0;
                    uselessRB.drag = 0;
                    Destroy(this);
                } 
                else
                {
                    transform.localPosition = position;
                    uselessRB.mass = 0;
                    uselessRB.isKinematic = true;
                    uselessRB.useGravity = false;
                    uselessRB.angularDrag = 0;
                    uselessRB.drag = 0;
                }
            }
        }
    }
}
