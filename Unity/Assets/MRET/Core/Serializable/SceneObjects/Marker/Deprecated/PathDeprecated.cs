using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GOV.NASA.GSFC.XR.MRET.Project;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;
using GOV.NASA.GSFC.XR.MRET.UI.SceneObjects.Drawing;

namespace GOV.NASA.GSFC.XR.MRET.SceneObjects.Pin
{
    /// <remarks>
    /// History:
    /// 5 October 2022: Created
    /// </remarks>
    ///
    /// <summary>
    /// PathDeprecated
    ///
    /// Class that contains pin related functions when path mode is enabled
    /// <Works with cref="PinManagerDeprecated"/>
    ///
    /// Author: Jeffrey Hosler and Sean Letavish
    /// </summary>
    /// 
    public class PathDeprecated : MonoBehaviour
    {
        private List<InteractablePinDeprecated> pins = new List<InteractablePinDeprecated>();
        public List<MeasurementTextDeprecated> segmentMeasurements = new List<MeasurementTextDeprecated>();
        private LineDrawingDeprecated line = null;
        private MeasurementTextDeprecated _mtext = null;
        bool pinDeleted = false;
        public MeasurementTextDeprecated mtext
        {
            get => _mtext;

        }
        

        private Vector3 _PinA = new Vector3(0, 0, 0);
        private Vector3 _PinB = new Vector3(0, 0, 0);

        public Vector3 PinA
        {
            get => _PinA;
        }

        public Vector3 PinB
        {
            get => _PinB;
        }

        /// <summary>
        /// Sets the measurement text component for a specific path
        /// </summary>
        /// <param name="linedrawing"></param>
        public PathDeprecated(LineDrawingDeprecated linedrawing)
        {
            line = linedrawing;
            _mtext = line.GetComponentInChildren<MeasurementTextDeprecated>(true);
            
        }

        /// <summary>
        /// Conditional remove all statement. If a pin is set to be deleted, 
        /// it is first removed from the path
        /// "Coding with sugar" -Tom Grubb 
        /// </summary>
        /// <param name="pinToDelete"></param>
        public void DeletePin(InteractablePinDeprecated pinToDelete)
        {
            pins.RemoveAll(pin => pin == pinToDelete);
            pinDeleted = true;
            if (ProjectManager.PinManagerDeprecated.segmentedMeasurementEnabled)
            {
                SegmentPins();
            }
        }

        public void DeleteSegmentMeasurement(MeasurementTextDeprecated segmentMeasurementToDelete)
        {
            segmentMeasurements.RemoveAll(segmentmeasurement => segmentmeasurement == segmentMeasurementToDelete);
            
        }

        public void DisplayMeasurements()
        {
            if (line != null)
                line.EnableMeasurement();
        }


        public void DisableMeasurements()
        {
            if (line != null)
                line.DisableMeasurement();
        }

        public void AddPin(InteractablePinDeprecated pin)
        {
            if (pins != null)
            {
                pins.Add(pin);
                if(ProjectManager.PinManagerDeprecated.segmentedMeasurementEnabled)
                {
                    line.SetUpSegmentMeasurementText();
                }
            }
        }

        public void SegmentPins()
        {
            if(_PinA==new Vector3(0,0,0))
            {
                _PinA = pins[0].transform.position;
            }

            else if (_PinA !=new Vector3(0,0,0) && _PinB == new Vector3(0,0,0))
            {
                _PinB = pins[1].transform.position;
            }

            else if(pinDeleted)
            {
                _PinA = pins[pins.Count - 2].transform.position;
                _PinB = pins.Last().transform.position;
                pinDeleted = false;
            }

            else
            {
                _PinA = _PinB;
                _PinB = pins.Last().transform.position;
            }
            segmentMeasurements.Last().SetValue(line.segmentLength, line.remainingLength);
        }

        /// <summary>
        /// Adds a new point to the line drawaing 
        /// Could use a rework, as list of interactable pins is 
        /// constantly rewritten
        /// </summary>
        public void Update()
        {
            if (line != null)
            {
                line.points = new Vector3[0];

                foreach (InteractablePinDeprecated iPin in pins)
                {
                    line.AddPoint(iPin.transform.position);
                }
            }
        }
    }
}
