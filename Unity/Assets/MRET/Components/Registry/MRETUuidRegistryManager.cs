// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using GOV.NASA.GSFC.XR.MRET.SceneObjects;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Drawing;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Note;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.Part;
using GOV.NASA.GSFC.XR.MRET.SceneObjects.User;

namespace GOV.NASA.GSFC.XR.MRET.Registry
{
    /// <remarks>
    /// History:
    /// 4 April 2021: Created
    /// 1 Oct 2021: Generalized for all IIdentifiable objects across MRET (J. Hosler)
    /// </remarks>
    /// <summary>
    /// Manager for all <code>IIdentifiable</code> objects in MRET.
    /// Author: Dylan Z. Baker
    /// </summary>
    public class MRETUuidRegistryManager : MRETManager<MRETUuidRegistryManager>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(MRETUuidRegistryManager);

        public readonly string DEFAULT_ID = "id-";

        /// <summary>
        /// Dictionary of maintained <code>IIdentifiable</code> objects keyed on UUID.
        /// </summary>
        //private Dictionary<Guid, IIdentifiable> registeredObjects = new Dictionary<Guid, IIdentifiable>();
        private Dictionary<Guid, WeakReference> registeredObjects = new Dictionary<Guid, WeakReference>();

        /// <summary>
        /// Cleans the registry of dead references
        /// </summary>
        public void Clean()
        {
            // Locate all the dead references
            var deadReferences = registeredObjects.Where(entry =>
                (entry.Value != null) &&
                (entry.Value.Target == null)).ToList();

            // Remove the dead references
            foreach (var deadReference in deadReferences)
            {
                registeredObjects.Remove(deadReference.Key);
            }
        }

        /// <summary>
        /// Array of all registered identifiable objects of the specified type.
        /// </summary>
        public T[] RegisteredTypes<T>()
            where T : IIdentifiable
        {
            List<T> rtn = new List<T>();
            //foreach (KeyValuePair<Guid, IIdentifiable> kvp in registeredObjects)
            foreach (KeyValuePair<Guid, WeakReference> kvp in registeredObjects)
            {
                if (kvp.Value.Target is T)
                {
                    rtn.Add((T)kvp.Value.Target);
                }
            }
            return rtn.ToArray();
        }

        /// <summary>
        /// Array of all registered <code>IIdentifiable</code> objects.
        /// </summary>
        public IIdentifiable[] IdentifiableObjects => RegisteredTypes<IIdentifiable>();
        /*
        public IIdentifiable[] IdentifiableObjects
        {
            get
            {
                IIdentifiable[] rtn = new IIdentifiable[registeredObjects.Count];
                registeredObjects.Values.CopyTo(rtn, 0);
                return rtn;
            }
        }
        */

        /// <summary>
        /// Array of all registered <code>ISceneObject</code> objects.
        /// </summary>
        public ISceneObject[] SceneObjects => RegisteredTypes<ISceneObject>();

        /// <summary>
        /// Array of all registered <code>IInteractable</code> objects.
        /// </summary>
        public IInteractable[] Interactables => RegisteredTypes<IInteractable>();

        /// <summary>
        /// Array of all registered <code>IPhysicalSceneObject</code> objects.
        /// </summary>
        public IPhysicalSceneObject[] PhysicalSceneObjects => RegisteredTypes<IPhysicalSceneObject>();

        /// <summary>
        /// Array of all registered <code>InteractablePart</code> objects.
        /// </summary>
        public InteractablePart[] Parts => RegisteredTypes<InteractablePart>();

        /// <summary>
        /// Array of all registered <code>InteractableNote</code> objects.
        /// </summary>
        public InteractableNote[] Notes => RegisteredTypes<InteractableNote>();

        /// <summary>
        /// Array of all registered drawings.
        /// </summary>
        public IInteractableDrawing[] Drawings => RegisteredTypes<IInteractableDrawing>();

        /// <summary>
        /// Array of all registered users.
        /// </summary>
        public IUser[] Users => RegisteredTypes<IUser>();

        /// <summary>
        /// Convenience function that creates a unique ID in the recommended format
        /// from a supplied name.
        /// </summary>
        /// <param name="name">The name to use as a basis for the resultant id</param>
        /// <returns>A unique ID in the recommended format</returns>
        public string CreateUniqueIDFromName(string name)
        {
            name = name.Replace("(clone)", ""); // Invalid syntax in XML schemas for ID types
            string id = DEFAULT_ID + name;
            return CreateUniqueID(id.ToLower());
        }

        /// <summary>
        /// Helper function that generates a unique ID from the registered identifiable objects.
        /// Use of this function is highly encouraged to ensure uniqueness of registered
        /// objects, but not enforced.
        /// </summary>
        /// <param name="desiredId">The desired ID, or null if <code>DEFAULT_ID</code> is
        ///     acceptable. Whitespace is removed.</param>
        /// <returns>
        /// One of the following:
        ///     - The desiredID if specified and is unique, or
        ///     - If specified and not unique, a number will be appended that makes it unique, or
        ///     - If not specified, a unique numbered ID based upon <code>DEFAULT_ID</code>
        /// </returns>
        public string CreateUniqueID(string desiredId = null)
        {
            // Start with our recommended default if not supplied
            string uniqueId = (!string.IsNullOrEmpty(desiredId)) ? desiredId : DEFAULT_ID;

            // Remove all whitespace. Split/Join proves faster than Regex
            uniqueId = string.Join("", uniqueId.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));

            // Try IDs until we find one that doesn't exist
            bool exists;
            string tryId = uniqueId;
            int uniqueNum = 1;
            do
            {
                // Check to see if this this ID exists
                exists = IDExists(tryId);
                if (exists)
                {
                    // Already exists, so append a number and try again
                    tryId = uniqueId + uniqueNum++;
                }
            }
            while (exists);

            return tryId;
        }

        /// <summary>
        /// Get an IIdentifiable object by its UUID.
        /// </summary>
        /// <param name="uuid">UUID of the <code>IIdentifiable</code> object to locate.</param>
        /// <returns>The corresponding <code>IIdentifiable</code> or null</returns>
        public IIdentifiable GetByUUID(Guid uuid)
        {
            registeredObjects.TryGetValue(uuid, out WeakReference reference);
            return (reference != null) ? reference.Target as IIdentifiable : null;
        }

        /// <summary>
        /// Get an IIdentifiable object by its ID.
        /// </summary>
        /// <param name="id">
        ///     ID of the <code>IIdentifiable</code> object to locate.
        ///     NOTE: This is different from the UUID.
        /// </param>
        /// <returns>The first <code>IIdentifiable</code> with the ID matching the supplied ID, or null</returns>
        public IIdentifiable GetByID(string id)
        {
            // Linq doesn't seem to work well with WeakReferences. This did not produce the expected results:
            // 
            //return registeredObjects.FirstOrDefault(entry =>
            //    (entry.Value != null) &&
            //    (entry.Value.Target is IIdentifiable) &&
            //    ((entry.Value.Target as IIdentifiable).id == id)).Value.Target as IIdentifiable;
            IIdentifiable result = null;
            foreach(KeyValuePair<Guid, WeakReference> entry in registeredObjects)
            {
                IIdentifiable identifiable = entry.Value.Target as IIdentifiable;
                if ((identifiable != null) && (identifiable.id == id))
                {
                    result = identifiable;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Get an identifiable object by its name.
        /// </summary>
        /// <param name="name">Name of the <code>IIdentifiable</code> object to locate</param>
        /// <returns>The first <code>IIdentifiable</code> with the ID matching the supplied ID, or null</returns>
        public IIdentifiable GetByName(string name)
        {
            // Linq doesn't seem to work well with WeakReferences. This did not produce the expected results:
            // 
            //return registeredObjects.FirstOrDefault(entry =>
            //    (entry.Value != null) &&
            //    (entry.Value.Target is IIdentifiable) &&
            //    ((entry.Value.Target as IIdentifiable).name == name)).Value.Target as IIdentifiable;
            IIdentifiable result = null;
            foreach (KeyValuePair<Guid, WeakReference> entry in registeredObjects)
            {
                IIdentifiable identifiable = entry.Value.Target as IIdentifiable;
                if ((identifiable != null) && (identifiable.name == name))
                {
                    result = identifiable;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Checks the registry for a registered object with the supplied ID.
        /// </summary>
        /// <param name="id">The ID to query</param>
        /// <returns>An indicator that an entry exists with the supplied ID</returns>
        public bool IDExists(string id)
        {
            //return registeredObjects.Any(entry => (entry.Value != null) && (entry.Value.id == id));
            //return registeredObjects.Any(entry =>
            //    (entry.Value != null) &&
            //    (entry.Value.Target is IIdentifiable) &&
            //    ((entry.Value.Target as IIdentifiable).id == id));

            bool result = false;
            foreach (KeyValuePair<Guid, WeakReference> entry in registeredObjects)
            {
                IIdentifiable identifiable = entry.Value.Target as IIdentifiable;
                if ((identifiable != null) && (identifiable.id == id))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Looks up an identifiable from a supplied ID. This function is useful if unsure
        /// if the ID is a MRET ID or a UUID.
        /// </summary>
        /// <param name="id">The ID to lookup</param>
        /// <returns>The first <code>IIdentifiable</code> where the ID was matched, or null</returns>
        public IIdentifiable Lookup(string id)
        {
            IIdentifiable result = null;
            if (!string.IsNullOrEmpty(id))
            {
                // Lookup by ID
                result = GetByID(id);
                if (result == null)
                {
                    try
                    {
                        // It may not be a valid Guid
                        result = GetByUUID(new Guid(id));
                    }
                    catch (Exception)
                    {
                        // Ignore
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Regsiters the supplied identifiable object by its UUID
        /// </summary>
        /// <param name="identifiable">The <code>IIdentifiable</code> to register</param>
        public void Register(IIdentifiable identifiable)
        {
            try
            {
                //registeredObjects.Add(identifiable.uuid, identifiable);
                registeredObjects.Add(identifiable.uuid, new WeakReference(identifiable, false));
                Log("Registering " + identifiable.name + " (" + identifiable.id + ")");
            }
            catch (ArgumentException)
            {
                LogWarning("Identifiable object already exists by the supplied UUID: " + identifiable.uuid, nameof(Register));
            }
        }

        /// <summary>
        /// Unregsiters the supplied identifiable object by its UUID
        /// </summary>
        /// <param name="uuid">The <code>Guid</code> to unregister</param>
        /// <param name="suppressWarning">Flag to suppress warning message if supplied UUID isn't registered. Default false.</param>
        public void Unregister(Guid uuid, bool suppressWarning = false)
        {
            IIdentifiable identifiable = GetByUUID(uuid);
            if (identifiable != null)
            {
                Unregister(identifiable, suppressWarning);
            }
        }

        /// <summary>
        /// Unregsiters the supplied identifiable object by its UUID
        /// </summary>
        /// <param name="identifiable">The <code>IIdentifiable</code> to unregister</param>
        /// <param name="suppressWarning">Flag to suppress warning message if supplied UUID isn't registered. Default false.</param>
        public void Unregister(IIdentifiable identifiable, bool suppressWarning = false)
        {
            if (identifiable != null)
            {
                if (registeredObjects.ContainsKey(identifiable.uuid))
                {
                    Log("Unregistering " + identifiable.name + " (" + identifiable.id + ")");
                    registeredObjects.Remove(identifiable.uuid);
                }
                else if (!suppressWarning)
                {
                    LogWarning("Identifiable object not found: " + identifiable.uuid, nameof(Unregister));
                }
            }
            else
            {
                LogWarning("Identifiable object is null", nameof(Unregister));
            }
        }

    }
}