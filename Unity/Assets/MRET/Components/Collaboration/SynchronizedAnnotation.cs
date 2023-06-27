// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using GOV.NASA.GSFC.XR.MRET.Annotation;

namespace GOV.NASA.GSFC.XR.MRET.Collaboration
{
    /// <remarks>
    /// History:
    /// 6 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// SynchronizedAnnotation
	///
	/// Performs synchronization on an annotation object in MRET
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public abstract class SynchronizedAnnotation<T> : Synchronized<T>, ISynchronizedAnnotation<T>
        where T : IAnnotation
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SynchronizedAnnotation<T>);

        // Tracked properties
        private bool lastLoop;
        private float lastStartDelay;
        private string lastAttachTo;

        #region ISynchronizedAnnotation
        /// <seealso cref="ISynchronizedAnnotation.SynchronizedObject"/>
        IAnnotation ISynchronizedAnnotation.SynchronizedObject => SynchronizedObject;

        /// <seealso cref="ISynchronizedAnnotation.Initialize(IAnnotation)"/>
        void ISynchronizedAnnotation.Initialize(IAnnotation synchronizedObject)
        {
            Initialize((T)synchronizedObject);
        }
        #endregion ISynchronizedAnnotation

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)

                    ? IntegrityState.Failure   // Fail is base class fails or anything is null
                    : IntegrityState.Success); // Otherwise, our integrity is valid
        }

        /// <seealso cref="MRETBehaviour.MRETAwake"/>
        protected override void MRETAwake()
        {
            // Take the inherited behavior
            base.MRETAwake();

            // TODO: Custom initialization (before deserialization)
        }

        /// <seealso cref="MRETBehaviour.MRETStart"/>
        protected override void MRETStart()
        {
            // Take the inherited behavior
            base.MRETStart();
        }

        /// <seealso cref="Synchronized{T}.CheckForUpdate"/>
        protected override void CheckForUpdate()
        {
            // Take the inherited behavior
            base.CheckForUpdate();

            // TODO: Annotation Settings. Can do this with polling here or with property listeners
            if (SynchronizedObject != null)
            {
                bool needsUpdate = false;

                // Loop
                if (SynchronizedObject.Loop != lastLoop)
                {
                    lastLoop = SynchronizedObject.Loop;
                    needsUpdate = true;
                }

                // Opacity
                if (SynchronizedObject.AttachTo != lastAttachTo)
                {
                    lastAttachTo = SynchronizedObject.AttachTo;
                    needsUpdate = true;
                }

                // Mass
                if (SynchronizedObject.StartDelay != lastStartDelay)
                {
                    lastStartDelay = SynchronizedObject.StartDelay;
                    needsUpdate = true;
                }

                // Notify XRC if we have anything to update
                if (needsUpdate)
                {
                    ReportUpdate();
                }
            }
        }

        /// <seealso cref="SynchronizedInteractable{T}.RefreshState"/>
        protected override void RefreshState()
        {
            // Take the inherited behavior
            base.RefreshState();

            // Interactions
            lastLoop = (SynchronizedObject != null) ? SynchronizedObject.Loop : AnnotationDefaults.LOOP;
            lastAttachTo = (SynchronizedObject != null) ? SynchronizedObject.AttachTo : AnnotationDefaults.ATTACH_TO;
            lastStartDelay = (SynchronizedObject != null) ? SynchronizedObject.StartDelay : AnnotationDefaults.START_DELAY;
        }
    }

    /// <summary>
    /// Provides an implementation for the abstract synchronize annotation object component
    /// </summary>
    public class SynchronizedAnnotation : SynchronizedAnnotation<IAnnotation>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(SynchronizedAnnotation);
    }

}
