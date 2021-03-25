// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using GSFC.ARVR.Utilities.Collider;
using GSFC.ARVR.Utilities.Renderer;
using GSFC.ARVR.Utilities.Transforms;
using GSFC.ARVR.MRET.Randomize;
using GSFC.ARVR.MRET.Common.Schemas;

namespace GSFC.ARVR.MRET.ProceduralObjectGeneration
{
    /**
     * Performs procedural object generation from a set of prefab information. This class will
     * automatically, using the <code>Start</code> method, randomly select prefabs based upon
     * customizable property settings, instantiate new instances from the prefab information,
     * and place the new instances at random locations constrained by property settings.<br/>
     * 
     * If this generator instance is disabled, the randomization will not occur until either the
     * instance is enabled, or a direct call to <code>PerformRandomization</code>.<br/>
     * 
     * The random seed for this instance may be specified to ensure deterministic results. If
     * the random seed is not specified, the current random seed is used.<br/>
     * 
     * @author Weston Bell-Geddes (Concept and initial implementation)
     * @author Jeffrey Hosler (Refactored to support improved customization and operation isolation
     */
    public class ProceduralObjectGenerator : MonoBehaviour
    {
        public static readonly string NAME = nameof(ProceduralObjectGenerator);

        public const int DEFAULT_NUM_OBJECTS = 15;

        public const float DEFAULT_PERCENT_OF_HIGHPOLYS = 20.0f;

        public const int DEFAULT_RANDOM_SEED = default;

        public const DistributionFunctionType DEFAULT_SPREAD_TYPE = DistributionFunctionType.Linear;
        public const float DEFAULT_SPREAD_MEAN = 0f;
        public const float DEFAULT_SPREAD_STANDARD_DEVIATION = 1f;

        public const float DEFAULT_COLLISION_DISTANCE = 100f;
        public const float DEFAULT_COLLISION_DEPTH = 0f;
        public const int DEFAULT_COLLISION_LAYER = ~0; // Everything

        // Placeholder for the generated instances
        private class GeneratedObject
        {
            public GameObject instance = null;
            public Bounds bounds;
            public RaycastHit[] collisions;
            public int calculatedCollisionIndex = -1;
            public Vector3 adjustedPosition;
            public bool valid = true;
        }
        private List<GeneratedObject> generatedObjects = new List<GeneratedObject>();

        [Tooltip("Array of low poly prefabs")]
        public ProceduralPrefab[] prefabsLowPoly; // array of procedurally generated objects (low poly)
        [Tooltip("Array of high poly prefabs")]
        public ProceduralPrefab[] prefabsHighPoly; // array of procedurally generated objects (high poly)

        [Tooltip("Number of objects to generate within the constrained area")]
        public int numObjects = DEFAULT_NUM_OBJECTS;
        [Tooltip("Random number seed. If not explicitly set, will default to the current random seed.")]
        public int randomSeed = DEFAULT_RANDOM_SEED;

        [Tooltip("Percentage of high poly objects to generate")]
        public float percentOfHighPoly = DEFAULT_PERCENT_OF_HIGHPOLYS;

        [Tooltip("Distance of the downward raycast for collision detection")]
        public float collisionDistance = DEFAULT_COLLISION_DISTANCE;
        [Tooltip("Depth the object will sink into the collision object")]
        public float collisionDepth = DEFAULT_COLLISION_DEPTH;
        [Tooltip("Layer for collision detection")]
        public LayerMask collisionLayerMask = DEFAULT_COLLISION_LAYER;

        [Tooltip("Dsplays the adjusted collision box position instead of actually moving the object. Used for debugging.")]
        public bool useGizmos = false;

        void Start()
        {
            // Perform the object generation
            PerformGeneration();
        }

        /**
         * Performs the object generation, factoring in the random seed settings
         * if specified
         * 
         * @see #randomSeed
         */
        public void PerformGeneration()
        {
            // Position the objects
            StartCoroutine(GenerateObjects());
        }

        /**
         * Generates the object instances
         */
        private IEnumerator GenerateObjects()
        {
            // Retain the existing random state in case we have to restore it later
            Random.State oldState = Random.state;

            // If the random seed was specifically set, use it
            if (randomSeed != DEFAULT_RANDOM_SEED)
            {
                // Make sure our seed is accurate
                Random.InitState(randomSeed);
            }

            // Start with a clean array of generatetd objects
            generatedObjects.Clear();

            // Generate all the objects
            for (int i = 0; i < numObjects; i++)
            {
                // Position the objects
                yield return GenerateAndPlaceObject(i);
            }

            // Show all the objects now that they are created
            foreach (GeneratedObject generatedObj in generatedObjects)
            {
                // Make sure it wasn;t destroyed by another part of the system
                if ((generatedObj != null) && generatedObj.valid)
                {
                    RendererUtil.Show(generatedObj.instance);
                }
            }

            // Restore the old state so we don't mess up logic elsewhere in the application
            if (randomSeed != DEFAULT_RANDOM_SEED)
            {
                Random.state = oldState;
            }

            yield return null;
        }

        /**
         * Draws the the calcaulted BoxCast collidere as a gizmo to show where the generated object
         * would have been placed. Useful for testing and can be disabled with the <code>useGizmos</code>
         * property.
         * 
         * @see useGizmos
         */
        void OnDrawGizmos()
        {
            // Quick abort if not using
            if (!useGizmos) return;

            // Display gizmos for collisions
            Gizmos.color = Color.red;

            foreach (GeneratedObject generatedObject in generatedObjects)
            {
                //Check if there has been a hit yet
                if (generatedObject.calculatedCollisionIndex != -1)
                {
                    //Draw a Ray forward from GameObject toward the hit
                    //                    Gizmos.DrawRay(transform.position, transform.forward * m_Hit.distance);
                    //Draw a cube that extends to where the hit exists
                    //                    Gizmos.DrawWireCube(transform.position + transform.forward * m_Hit.distance, transform.localScale);
                    Gizmos.DrawWireCube(
                        generatedObject.instance.transform.position + Vector3.down * generatedObject.collisions[generatedObject.calculatedCollisionIndex].distance,
                        generatedObject.instance.transform.localScale);
                }
                //If there hasn't been a hit yet, draw the ray at the maximum distance
                else
                {
                    //Draw a Ray forward from GameObject toward the maximum distance
                    //                    Gizmos.DrawRay(transform.position, Vector3.down * m_MaxDistance);
                    //Draw a cube at the maximum distance
                    //                    Gizmos.DrawWireCube(transform.position + transform.forward * m_MaxDistance, transform.localScale);
                    Gizmos.DrawWireCube(
                        generatedObject.instance.transform.position + Vector3.down * collisionDistance,
                        generatedObject.instance.transform.transform.localScale);
                }
            }
        }

        /**
         * Generates and places the object instance
         * 
         * @see index The index of the object
         * 
         * @see #randomSeed
         */
        private IEnumerator GenerateAndPlaceObject(int index)
        {
            // Check that we have at least one prefab defined
            if (((prefabsLowPoly == null) || (prefabsLowPoly.Length < 1)) &&
                ((prefabsHighPoly == null) || (prefabsHighPoly.Length < 1)))
            {
                Debug.LogWarning("[" + NAME + "]: No prefabs defined");
                yield break;
            }

            // Create the structure that will hold our generated object info
            GeneratedObject generatedObject = new GeneratedObject();

            // Build a new object from the prefab
            generatedObject.instance = GenerateObject(index);

            // Get the collider and renderer for the object
            Collider collider = generatedObject.instance.GetComponent<Collider>();

            // Get the bounds of the object
            generatedObject.bounds = TransformUtil.GetBounds(generatedObject.instance);

            // Check if the object Y position should be moved down toward the first collision
            generatedObject.adjustedPosition = generatedObject.instance.transform.position;
            Quaternion worldDown = Quaternion.LookRotation(Vector3.up, Vector3.down);

            // Find the collisions using a box cast and figure out where we need to relocate the object
            generatedObject.collisions = Physics.BoxCastAll(generatedObject.bounds.center, generatedObject.bounds.extents,
                Vector3.down, worldDown, collisionDistance, collisionLayerMask);
            if (generatedObject.collisions.Length > 0)
            {
                // The colliders are supposed to be in order of furthest to closest, so let's process in reverse order
                // because we want to find the closest valid collision. Invalid collisions (started inside) will be
                // marked with a distance of 0
                // NOTE: Not all collision arrays appear to be sorted properly. Just process all finding the closest non-zero
                generatedObject.calculatedCollisionIndex = -1;
                for (int i = (generatedObject.collisions.Length -1); i >= 0; i--)
                {
/** TODO: Is there something I can do to look for valid positions?
                    // Determine if there are any colliders that overlap in the proposed new position
                    Quaternion up = Quaternion.FromToRotation(generatedObject.transform.up, collisions[collisions.Length - 1].normal);
                    Collider[] collidersInsideOverlapBox = new Collider[10];
                    int numOverlaps = Physics.OverlapBoxNonAlloc(adjustedPosition, objectBounds.extents, collidersInsideOverlapBox, generatedObject.transform.rotation, collisionLayerMask);

                    // See if the object is colliding with anything other than the terrain
                    if (numOverlaps > 1)
                    {
                        // TODO: We have a problem with the proposed position, so determine how to proceed
                    }
*/
                    // See if this collision is valid
                    if ((generatedObject.collisions[i].distance != 0) &&
                        ((generatedObject.calculatedCollisionIndex == -1) ||
                         (generatedObject.collisions[i].distance < generatedObject.collisions[generatedObject.calculatedCollisionIndex].distance)))
                    {
                        generatedObject.calculatedCollisionIndex = i;
                    }
                }

                // See if we found a valid collision point and reposition
                if (generatedObject.calculatedCollisionIndex != -1)
                {
                    // Calculate the depth we need to sink into the collision object
                    float depth = (generatedObject.bounds.extents.y * 2f) * (collisionDepth / 100f);

                    // We want the object repositioned so that the hit point will be the bottom of the collider.
                    // Move it up half of the object bounds and then sink in
                    generatedObject.adjustedPosition.y = generatedObject.collisions[generatedObject.calculatedCollisionIndex].point.y + generatedObject.bounds.extents.y - depth;

                    // Place the object if not using gizmos
                    if (!useGizmos)
                    {
                        generatedObject.instance.transform.position = generatedObject.adjustedPosition;
                    }
                }
                else
                {
                    Debug.LogWarning("[" + NAME + "]: No valid collision found for " + gameObject.name);
                }

            }

            // Hide the object. We will display later
            RendererUtil.Show(generatedObject.instance, false);

            // Store the object
            generatedObjects.Add(generatedObject);

            yield return null;
        }

        /**
         * Selects a random prefab and uses that to generates and place a new
         * cloned object instance
         * 
         * @see index The index of the object
         */
        private GameObject GenerateObject(int index)
        {
            // Determine if we are placing a low or high poly object
            float randomValue = Random.Range(0.0f, 1.0f);

            // Figure out which prefab list we are using
            ProceduralPrefab[] prefabList = null;
            if ((prefabsHighPoly != null) && (prefabsHighPoly.Length > 0) &&
                (prefabsLowPoly != null) && (prefabsLowPoly.Length > 0))
            {
                // Use the high poly percentage to determine which prefab list to use
                prefabList = (randomValue <= (percentOfHighPoly / 100.0f)) ? prefabsHighPoly : prefabsLowPoly;
            }
            else if ((prefabsHighPoly != null) && (prefabsHighPoly.Length > 0))
            {
                // Only high poly prefabs
                prefabList = prefabsHighPoly;
            }
            else
            {
                // Only low poly prefabs
                prefabList = prefabsLowPoly;
            }

            // Select a prefab from a randomized list index
            int randomIndex = Random.Range(0, prefabList.Length); // NOTE: For Random.Range(int,int), Max is exclusive! Random.Range(float,float) is inclusive
            ProceduralPrefab prefab = prefabList[randomIndex];

            // Generate a name for this instance
            int numDigits = numObjects.ToString(NumberFormatInfo.InvariantInfo).Length;
            string name = this.name + "_" + index.ToString("D" + numDigits);

            // Create the new object
            return prefab.Instantiate(name);
        }

    }

    /**
     * Container class for all of the information necessary to generate an procedural
     * object instance from prefab assets and randomization information. This class is
     * specific to the <code>ProceduralObjectGenerator</code>, so is placed here for
     * maintainability.<br/>
     * 
     * @see ProceduralObjectGenerator
     * 
     * @author Jeffrey Hosler
     */
    [System.Serializable]
    public class ProceduralPrefab
    {
        public static string NAME;

        private ConfigurationManager configurationManager;

        [Tooltip("The game object that will contain the instatiated objects")]
        public GameObject instanceContainer = null;

        [Tooltip("Prefab assets used on instantiation")]
        public PrefabAssets assets = null;

        [Tooltip("Randomize position configuration for instantiated objects")]
        public RandomizePositionConfig positionRandomizationConfig = null;
        [Tooltip("Randomize rotation configuration for instantiated objects")]
        public RandomizeRotationConfig rotationRandomizationConfig = null;
        [Tooltip("Randomize scale configuration for instantiated objects")]
        public RandomizeScaleConfig scaleRandomizationConfig = null;

        /**
         * Constructor
         */
        public ProceduralPrefab()
        {
            NAME = nameof(ProceduralPrefab);

            // Assign the configuration manager
            configurationManager = ConfigurationManager.instance;

            // Contruct the assets
            assets = new PrefabAssets();
        }

        /**
         * Instantiates a new object instance from the prefab assets and randomizationinformation Applies a randomization transform to the supplied object.
         * 
         * @param gameObject The <code>GameObject</code> to perform a randomized transform
         * @param config The <code>RandomizeTransformConfig</code> containing the settings for the randomization
         * 
         * @see RandomizeTransform
         * @see RandomizeTransformConfig
         */
        public GameObject Instantiate(string name)
        {
            // Generate the clone and parent it to the container
            GameObject instance = Object.Instantiate(assets.mesh, instanceContainer.transform);

            // Assign the basic properties from the prefab information
            instance.name = name;
            instance.transform.parent = instanceContainer.transform;

            // Assign the material
            if (assets.material != null)
            {
                // Assign the material
                RendererUtil.ApplyMaterial(instance, assets.material);
            }

            // Check for the existence of a collider
            if ((instance.GetComponent<Collider>() == null) &&
                (instance.GetComponent<NonConvexMeshCollider>() == null)) // Doesn't extend Collider
            {
                // Create the collider. *Must* be done before we start transformations
                CreateCollider(instance);
            }

            // Let's disable and apply the randomization now so that we control the order of execution.
            // This is key for random number seeding since unity cannot guarantee order of execution of a
            // Mono script
            Randomize.Randomize.ApplyRandomization<RandomizePosition>(instance, positionRandomizationConfig, false);
            Randomize.Randomize.ApplyRandomization<RandomizeRotation>(instance, rotationRandomizationConfig, false);
            Randomize.Randomize.ApplyRandomization<RandomizeScale>(instance, scaleRandomizationConfig, false);

            return instance;
        }

        /**
         * Creates a collider for the supplied object. The ConfigurationManager will be used
         * to determine the type of collider to create.
         * 
         * @param generatedObject The <code>GameObject</code> to assign the new collider
         * 
         * @see Collider
         * @See COnfigurationManager
         */
        private Collider CreateCollider(GameObject generatedObject)
        {
            Collider collider = null;
            if (configurationManager != null)
            {
                ConfigurationManager.ColliderMode mode = configurationManager.colliderMode;

                // Log a message
                Debug.Log("[" + NAME + "]: No collider detected. Generating collider type: " + mode);

                // Create the collider based upon the configuration manager settings
                switch (mode)
                {
                    case ConfigurationManager.ColliderMode.Box:

                        // Create the box collider
                        collider = ColliderUtil.CreateBoxCollider(generatedObject);
                        break;

                    case ConfigurationManager.ColliderMode.NonConvex:

                        // Create the NonConvexMeshColliders for each mesh making up the object
                        ColliderUtil.CreateNonConvexMeshColliders(generatedObject);
                        break;

                    case ConfigurationManager.ColliderMode.None:
                    default:
                        break;
                }
            }

            return collider;
        }

    }

}
