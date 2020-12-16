using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GSFC.ARVR.MRET.Infrastructure.Components.AssetBundles;
using GSFC.ARVR.MRET.Randomize;
using GSFC.ARVR.MRET.Common.Schemas;

namespace GSFC.ARVR.MRET.ProceduralObjectGeneration
{
    /**
     * The procedural object generator manager. This class is responsible for the
     * deserialization and generation of the procedural objects in MRET.</br>
     * 
     * @author Weston Bell-Geddes (Initial version)
     * @author Jeffrey Hosler (Refactored to support major improvements in deserialization
     *      to support detailed customization of the procedural object generators,
     *      isolation of operations into functions and better handling of Coroutine
     *      success and error conditions
     */
    public class ProceduralObjectGeneratorManager : MonoBehaviour
    {
        public static readonly string NAME = nameof(ProceduralObjectGeneratorManager);

        public static ProceduralObjectGeneratorManager instance;

        [Tooltip("Container for the procedural object generators")]
        public GameObject objectGeneratorsContainer;

        /**
         * Extends the ProceduralPrefab class description to add information related
         * to the deserialization process
         */
        private class DeserializedPrefab : ProceduralPrefab
        {
            public bool loaded = false;
        }

        void Awake()
        {
            instance = this;
        }

        /**
        * Adds a object generator to this manager, instantiated from the supplied serialized information.<br/>
        * 
        * @param serialized The <code>ObjectGeneratorType</code> The serialized object generator information
        *      
        * @returns IEnumerator The <code>IEnumerator</code> reentrant point for the asychronous handler
        */
        public IEnumerator AddGenerator(ObjectGeneratorType serialized)
        {
            if (objectGeneratorsContainer == null)
            {
                Debug.LogWarning("[" + NAME + "]: Invalid state. Container properties are not set.");
                yield break;
            }

            if (serialized == null)
            {
                Debug.LogWarning("[" + NAME + "]: Supplied serialized object generator is invalid");
                yield break;
            }

            // Create the generator object
            GameObject objectGenerationObject = new GameObject(serialized.Name);
            objectGenerationObject.transform.parent = objectGeneratorsContainer.transform;

            // Deserialize the properties from the serialized generator
            yield return DeserializeObjectGenerationType(serialized, objectGenerationObject);
        }

        /**
        * Performs a deserialization of the serialized object generator information and places the deserialized
        * values into the supplied argument.<br/>
        * 
        * @param serialized The <code>ObjectGeneratorType</code> The serialized object generator information
        * @param objectGenerationObject The <code>GameObject></code> that will contain the deserialized
        *      object generator object
        *      
        * @returns IEnumerator The <code>IEnumerator</code> reentrant point for the asychronous handler
        */
        private IEnumerator DeserializeObjectGenerationType(ObjectGeneratorType serialized, GameObject objectGenerationObject)
        {
            // Local transform
            Vector3 localPosition = new Vector3(
                serialized.Transform.Position.X,
                serialized.Transform.Position.Y,
                serialized.Transform.Position.Z);
            Vector3 localScale = new Vector3(
                serialized.Transform.Scale.X,
                serialized.Transform.Scale.Y,
                serialized.Transform.Scale.Z);
            Quaternion localRotation = new Quaternion(
                serialized.Transform.Rotation.X,
                serialized.Transform.Rotation.Y,
                serialized.Transform.Rotation.Z,
                serialized.Transform.Rotation.W);

            // Create the object container
            GameObject generatedObjectsContainer = new GameObject(serialized.Name + " Instances");
            generatedObjectsContainer.transform.parent = objectGenerationObject.transform;

            // Set the transform
            objectGenerationObject.transform.localPosition = localPosition;
            objectGenerationObject.transform.localScale = localScale;
            objectGenerationObject.transform.localRotation = localRotation;

            // Create the object generator
            ProceduralObjectGenerator objectGenerator = objectGenerationObject.AddComponent<ProceduralObjectGenerator>();
            objectGenerator.name = serialized.Name;
            objectGenerator.transform.parent = objectGenerationObject.transform;

            // Let's disable so that we control the order of execution. We want to make sure that
            // all prefabs are deserialized and loaded before the Mono script executes
            objectGenerator.enabled = false;

            // Deserialize the properties from the serialized generator
            objectGenerator.numObjects = serialized.NumObjects;
            objectGenerator.randomSeed = (serialized.RandomSeed != null)
                ? serialized.RandomSeed[0]
                : objectGenerator.randomSeed;
            objectGenerator.percentOfHighPoly = (serialized.PercentOfHighQuality != null)
                ? serialized.PercentOfHighQuality[0]
                : objectGenerator.percentOfHighPoly;

            // Collision parameters
            objectGenerator.collisionDistance = (serialized.CollisionDistance != null)
                ? serialized.CollisionDistance[0]
                : ProceduralObjectGenerator.DEFAULT_COLLISION_DISTANCE;
            objectGenerator.collisionDepth = (serialized.CollisionDepth != null)
                ? serialized.CollisionDepth[0]
                : ProceduralObjectGenerator.DEFAULT_COLLISION_DEPTH;

            // Determine what was specified, if anything, for the collision layer mask
            LayerMask collisionLayerMask = ProceduralObjectGenerator.DEFAULT_COLLISION_LAYER;
            if (serialized.CollisionLayerMask != null)
            {
                // Take the specified value as the mask
                collisionLayerMask = serialized.CollisionLayerMask[0];
            }
            else if (serialized.CollisionLayerMaskName != null)
            {
                // Convert the name to a mask
                collisionLayerMask = LayerMask.NameToLayer(serialized.CollisionLayerMaskName[0]);
            }
            objectGenerator.collisionLayerMask = collisionLayerMask;

            // Log the collision layer mask so that it's easier for people to debug their settings
            string layerMaskName = LayerMask.LayerToName(objectGenerator.collisionLayerMask);
            if (layerMaskName == "") layerMaskName = "Everything";
            Debug.Log("[" + NAME + "]: Collision layer for object placement of " + serialized.Name + " is: " + layerMaskName);

            // Deserialize the high quality prefabs
            List<ProceduralPrefab> highQualityPrefabs = new List<ProceduralPrefab>();
            if (serialized.HighQualityPrefabs != null)
            {
                // Deserialize the high quality prefabs
                yield return DeserializeObjectGenerationPrefabsType(serialized.HighQualityPrefabs, highQualityPrefabs, generatedObjectsContainer);

                // Copy over the array of prefabs to the generator
                objectGenerator.prefabsHighPoly = highQualityPrefabs.ToArray();
            }

            // Deserialize the low quality prefabs
            List<ProceduralPrefab> lowQualityPrefabs = new List<ProceduralPrefab>();
            if (serialized.LowQualityPrefabs != null)
            {
                // Deserialize the high quality prefabs
                yield return DeserializeObjectGenerationPrefabsType(serialized.LowQualityPrefabs, lowQualityPrefabs, generatedObjectsContainer);

                // Copy over the array of prefabs to the generator
                objectGenerator.prefabsLowPoly = lowQualityPrefabs.ToArray();
            }

            // Everything is loaded so enable the generator so that it can run
            objectGenerator.enabled = true;

            yield return null;
        }

        /**
        * Performs a deserialization of the serialized object generation prefabs information and places the deserialized
        * prefab values into the supplied argument list.<br/>
        * 
        * @param serialized The <code>ObjectGenerationPrefabsType</code> The serialized object generation prefabs
        *      information
        * @param deserializedConfig The <code>List<ProceduralPrefab></code> list that will contain the deserialized
        *      object generation prefab objects
        * @param instanceContainer The <code>GameObject</code> that will parent the instances that get generated
        *      
        * @returns IEnumerator The <code>IEnumerator</code> reentrant point for the asychronous handler
        */
        private IEnumerator DeserializeObjectGenerationPrefabsType(ObjectGenerationPrefabsType serialized,
            List<ProceduralPrefab> deserializedPrefabs, GameObject instanceContainer)
        {
            // Loop through each serialized prefab description deserializing each one.
            foreach (ObjectGenerationPrefabType hqp in serialized.Prefab)
            {
                // Build the prefab from the serialized info
                DeserializedPrefab deserializedPrefab = new DeserializedPrefab();
                deserializedPrefab.instanceContainer = instanceContainer;

                // Deserialize the prefab
                yield return DeserializeObjectGenerationPrefabType(hqp, deserializedPrefab);

                // Make sure we got a good prefab
                if (deserializedPrefab.assets.mesh != null)
                {
                    // Place the prefab into the array
                    deserializedPrefabs.Add(deserializedPrefab);
                }
                else
                {
                    Debug.LogWarning("[" + NAME + "]: Problem occurred trying to load prefab '" +
                        hqp.MeshAsset.Name + "'");
                }
            }

            yield return null;
        }

        /**
        * Performs a deserialization of the serialized object generation prefab information and places the deserialized
        * prefab values into the supplied argument.<br/>
        * 
        * @param serialized The <code>ObjectGenerationPrefabType</code> The serialized object generation prefab
        *      information
        * @param deserializedConfig The <code>DeserializedPrefab</code> object that will contain the deserialized
        *      object generation prefab values
        *      
        * @returns IEnumerator The <code>IEnumerator</code> reentrant point for the asychronous handler
        */
        private IEnumerator DeserializeObjectGenerationPrefabType(ObjectGenerationPrefabType serialized,
            DeserializedPrefab deserializedPrefab)
        {
            StartCoroutine(LoadMeshAndMaterial(serialized, deserializedPrefab));

            // Wait for the material and mesh to be loaded
            while (!deserializedPrefab.loaded)
            {
                yield return new WaitForSeconds(0.1f);
            }

            // Make sure there wasn't an issue loading the mesh
            if (deserializedPrefab.assets.mesh != null)
            {
                // Deserialize the randomize position element if it exists
                if (serialized.RandomizePosition != null)
                {
                    // Create the RandomizePosition instance
                    RandomizePositionConfig positionRandomizationConfig = new RandomizePositionConfig();

                    // Perform the deserialization
                    yield return DeserializeObjectGenerationRandomize(serialized.RandomizePosition, positionRandomizationConfig);

                    // Store the result in the deserialized prefab
                    deserializedPrefab.positionRandomizationConfig = positionRandomizationConfig;
                }

                // Deserialize the randomize rotation element if it exists
                if (serialized.RandomizeRotation != null)
                {
                    // Create the RandomizeRotation instance
                    RandomizeRotationConfig rotationRandomizationConfig = new RandomizeRotationConfig();

                    // Perform the deserialization
                    yield return DeserializeObjectGenerationRandomize(serialized.RandomizeRotation, rotationRandomizationConfig);

                    // Store the result in the deserialized prefab
                    deserializedPrefab.rotationRandomizationConfig = rotationRandomizationConfig;
                }

                // Deserialize the randomize scale element if it exists
                if (serialized.RandomizeScale != null)
                {
                    // Create the RandomizeScale instance
                    RandomizeScaleConfig scaleRandomizationConfig = new RandomizeScaleConfig();

                    // Perform the deserialization
                    yield return DeserializeObjectGenerationRandomize(serialized.RandomizeScale, scaleRandomizationConfig);

                    // Store the result in the deserialized prefab
                    deserializedPrefab.scaleRandomizationConfig = scaleRandomizationConfig;
                }
            }
            else
            {
                Debug.LogWarning("[" + NAME + "]: An issue was encountered trying to load mesh '" + serialized.MeshAsset.Name + "'");
                yield break;
            }

            yield return null;
        }

        /**
         * Performs a deserialization of the serialized randomize information and places the deserialized
         * values into the supplied argument.<br/>
         * 
         * @param serialized The <code>RandomizeType</code> The serialized randomize information
         * @param deserializedConfig The <code>RandomizeConfig</code> object that will contain the deserialized
         *      randomize values
         *      
         * @returns IEnumerator The <code>IEnumerator</code> reentrant point for the asychronous handler
         */
        private IEnumerator DeserializeObjectGenerationRandomize(RandomizeType serialized,
            RandomizeConfig deserializedConfig)
        {
            // Deserialize the RandomizeType
            deserializedConfig.randomSeed = (serialized.RandomSeed != null)
                ? serialized.RandomSeed[0]
                : RandomizePosition.DEFAULT_RANDOM_SEED;

            // Check if there is more deserialization to perform
            if ((serialized is RandomizeTransformType) &&
                (deserializedConfig is RandomizeTransformConfig))
            {
                // Perform the randomize transform deserialization
                yield return DeserializeObjectGenerationRandomizeTransform(
                    serialized as RandomizeTransformType,
                    deserializedConfig as RandomizeTransformConfig);
            }

            yield return null;
        }

        /**
         * Performs a deserialization of the serialized randomize transform information and places the deserialized
         * values into the supplied argument.<br/>
         * 
         * @param serialized The <code>RandomizeTransformType</code> The serialized randomize transform information
         * @param deserializedConfig The <code>RandomizeTransformConfig</code> object that will contain the deserialized
         *      randomize transform values
         *      
         * @returns IEnumerator The <code>IEnumerator</code> reentrant point for the asychronous handler
         */
        private IEnumerator DeserializeObjectGenerationRandomizeTransform(RandomizeTransformType serialized,
            RandomizeTransformConfig deserializedConfig)
        {
            // Deserialize the RandomizeTransformType
            deserializedConfig.randomizeX = (serialized.RandomizeX != null)
                ? serialized.RandomizeX[0]
                : RandomizePosition.DEFAULT_RANDOM_X;
            deserializedConfig.randomizeY = (serialized.RandomizeY != null)
                ? serialized.RandomizeY[0]
                : RandomizePosition.DEFAULT_RANDOM_Y;
            deserializedConfig.randomizeZ = (serialized.RandomizeZ != null)
                ? serialized.RandomizeZ[0]
                : RandomizePosition.DEFAULT_RANDOM_Z;

            // Check if there is more deserialization to perform
            if ((serialized is RandomizePositionType) &&
                (deserializedConfig is RandomizePositionConfig))
            {
                // Perform the randomize position deserialization
                yield return DeserializeObjectGenerationRandomizePosition(
                    serialized as RandomizePositionType,
                    deserializedConfig as RandomizePositionConfig);
            }
            else if ((serialized is RandomizeRotationType) &&
                     (deserializedConfig is RandomizeRotationConfig))
            {
                // Perform the randomize rotation deserialization
                yield return DeserializeObjectGenerationRandomizeRotation(
                    serialized as RandomizeRotationType,
                    deserializedConfig as RandomizeRotationConfig);
            }
            else if ((serialized is RandomizeScaleType) &&
                     (deserializedConfig is RandomizeScaleConfig))
            {
                // Perform the randomize scale deserialization
                yield return DeserializeObjectGenerationRandomizeScale(
                    serialized as RandomizeScaleType,
                    deserializedConfig as RandomizeScaleConfig);
            }

            yield return null;
        }

        /**
         * Performs a deserialization of the serialized randomize position information and places the deserialized
         * values into the supplied argument.<br/>
         * 
         * @param serialized The <code>RandomizePositionType</code> The serialized randomize position information
         * @param deserializedConfig The <code>RandomizePositionConfig</code> object that will contain the deserialized
         *      randomize position values
         *      
         * @returns IEnumerator The <code>IEnumerator</code> reentrant point for the asychronous handler
         */
        private IEnumerator DeserializeObjectGenerationRandomizePosition(RandomizePositionType serialized,
            RandomizePositionConfig deserializedConfig)
        {
            // Deserialize the RandomizePositionType
            deserializedConfig.distributionType = (serialized.DistributionType != null)
                ? serialized.DistributionType[0]
                : RandomizePosition.DEFAULT_DISTRIBUTION_TYPE;

            // Determine which set of parameters to deserialize
            string functionSettings = "";
            switch (deserializedConfig.distributionType)
            {
                case DistributionFunctionType.Linear:

                    // Linear distribution
                    if (serialized.PositionMin != null)
                    {
                        deserializedConfig.positionMin = new Vector3(
                            serialized.PositionMin.X,
                            serialized.PositionMin.Y,
                            serialized.PositionMin.Z);
                    }
                    if (serialized.PositionMax != null)
                    {
                        deserializedConfig.positionMax = new Vector3(
                            serialized.PositionMax.X,
                            serialized.PositionMax.Y,
                            serialized.PositionMax.Z);
                    }

                    // Generate the function settings for logging
                    functionSettings =
                        "X: [" + deserializedConfig.positionMin.x + ", " + deserializedConfig.positionMax.x + "], " +
                        "Y: [" + deserializedConfig.positionMin.y + ", " + deserializedConfig.positionMax.y + "], " +
                        "Z: [" + deserializedConfig.positionMin.z + ", " + deserializedConfig.positionMax.z + "]";
                    break;

                case DistributionFunctionType.Normal:

                    // Normalized distribution
                    if (serialized.PositionMean != null)
                    {
                        deserializedConfig.positionMean = new Vector3(
                            serialized.PositionMean.X,
                            serialized.PositionMean.Y,
                            serialized.PositionMean.Z);
                    }
                    if (serialized.PositionStandardDeviation != null)
                    {
                        deserializedConfig.positionStandardDeviation = new Vector3(
                            serialized.PositionStandardDeviation.X,
                            serialized.PositionStandardDeviation.Y,
                            serialized.PositionStandardDeviation.Z);
                    }

                    // Generate the function settings for logging
                    functionSettings =
                        "μ:  [" +
                            deserializedConfig.positionMean.x + ", " +
                            deserializedConfig.positionMean.y + ", " +
                            deserializedConfig.positionMean.z + "], " +
                        "σ²: [" +
                            deserializedConfig.positionStandardDeviation.x + ", " +
                            deserializedConfig.positionStandardDeviation.y + ", " +
                            deserializedConfig.positionStandardDeviation.z + "]";
                    break;

                default:
                    Debug.LogWarning("[" + NAME + "]: " + RandomizePosition.NAME + " has an invalid spread type specified.");
                    yield break;
            }
            Debug.Log("[" + NAME + "]: " + RandomizePosition.NAME + " using distribution function: " +
                deserializedConfig.distributionType + " with settings:\n" + functionSettings);

            yield return null;
        }

        /**
         * Performs a deserialization of the serialized randomize rotation information and places the deserialized
         * values into the supplied argument.<br/>
         * 
         * @param serialized The <code>RandomizeRotationType</code> The serialized randomize rotation information
         * @param deserializedConfig The <code>RandomizeRotationConfig</code> object that will contain the deserialized
         *      randomize rotation values
         *      
         * @returns IEnumerator The <code>IEnumerator</code> reentrant point for the asychronous handler
         */
        private IEnumerator DeserializeObjectGenerationRandomizeRotation(RandomizeRotationType serialized,
            RandomizeRotationConfig deserializedConfig)
        {
            // Deserialize the RandomizeRotationType
            if (serialized.RotationMin != null)
            {
                deserializedConfig.rotationMin = new Vector3(
                    serialized.RotationMin.X,
                    serialized.RotationMin.Y,
                    serialized.RotationMin.Z);
            }
            if (serialized.RotationMax != null)
            {
                deserializedConfig.rotationMax = new Vector3(
                    serialized.RotationMax.X,
                    serialized.RotationMax.Y,
                    serialized.RotationMax.Z);
            }

            yield return null;
        }

        /**
         * Performs a deserialization of the serialized randomize scale information and places the deserialized
         * values into the supplied argument.<br/>
         * 
         * @param serialized The <code>RandomizeScaleType</code> The serialized randomize scale information
         * @param deserializedConfig The <code>RandomizeScaleConfig</code> object that will contain the deserialized
         *      randomize scale values
         *      
         * @returns IEnumerator The <code>IEnumerator</code> reentrant point for the asychronous handler
         */
        private IEnumerator DeserializeObjectGenerationRandomizeScale(RandomizeScaleType serialized,
            RandomizeScaleConfig deserializedConfig)
        {
            // Deserialize the RandomizeScaleType
            deserializedConfig.maintainAxialRatio = (serialized.MaintainAxialRatio != null)
                ? serialized.MaintainAxialRatio[0]
                : RandomizeScale.DEFAULT_MAINTAIN_AXIAL_RATIO;

            // Determine which set of parameters to deserialize
            if (deserializedConfig.maintainAxialRatio)
            {
                // Maintain axial ratio
                deserializedConfig.maintainAxialRatioScaleMin = (serialized.MaintainAxialRatioScaleMin != null)
                    ? serialized.MaintainAxialRatioScaleMin[0]
                    : RandomizeScale.DEFAULT_SCALE_MIN;
                deserializedConfig.maintainAxialRatioScaleMax = (serialized.MaintainAxialRatioScaleMax != null)
                    ? serialized.MaintainAxialRatioScaleMax[0]
                    : RandomizeScale.DEFAULT_SCALE_MAX;
            }
            else
            {
                // Independent scaling of axes
                if (serialized.ScaleMin != null)
                {
                    deserializedConfig.scaleMin = new Vector3(
                        serialized.ScaleMin.X,
                        serialized.ScaleMin.Y,
                        serialized.ScaleMin.Z);
                }
                if (serialized.ScaleMax != null)
                {
                    deserializedConfig.scaleMax = new Vector3(
                        serialized.ScaleMax.X,
                        serialized.ScaleMax.Y,
                        serialized.ScaleMax.Z);
                }
            }

            yield return null;
        }

        /**
         * Performs a load of optional material assets.<br/>
         * 
         * @param loadedMesh The <code>GameObject</code> The gameobject that was loaded
         * 
         * @param serialized The <code>ObjectGenerationPrefabType</code> The serialized object generation prefab
         *      information
         * @param deserializedPrefab The <code>DeserializedPrefab</code> object that will contain the deserialized
         *      object generation prefab information
         */
        private void FinishLoadingMaterial(Material loadedMaterial, ObjectGenerationPrefabType serialized, DeserializedPrefab deserializedPrefab)
        {
            // Store the loaded asset
            deserializedPrefab.assets.material = loadedMaterial;

            // Mark as loaded
            deserializedPrefab.loaded = true;
        }

        /**
         * Performs a load of the mesh and initiates the loading optional material assets.<br/>
         * 
         * @param loadedMesh The <code>GameObject</code> The gameobject that was loaded
         * 
         * @param serialized The <code>ObjectGenerationPrefabType</code> The serialized object generation prefab
         *      information
         * @param deserializedPrefab The <code>DeserializedPrefab</code> object that will contain the deserialized
         *      object generation prefab information
         */
        private void FinishLoadingMesh(GameObject loadedMesh, ObjectGenerationPrefabType serialized, DeserializedPrefab deserializedPrefab)
        {
            // Store the loaded asset
            deserializedPrefab.assets.mesh = loadedMesh;

            // Check if the material was specified
            if (serialized.MaterialAsset != null)
            {
                // Perform the asychronous loading of the material from the asset bundle
                System.Action<object> action = (object loaded) =>
                {
                    FinishLoadingMaterial((Material) loaded, serialized, deserializedPrefab);
                };
                AssetBundleHelper.instance.LoadAssetAsync("file://" + Application.dataPath
                + "/StreamingAssets/Windows/" + serialized.MeshAsset.AssetBundle, serialized.MeshAsset.Name, typeof(Material), action);
            }
        }

        /**
         * Initiates an asynchronous load of the mesh and optional material assets.<br/>
         * 
         * @param serialized The <code>ObjectGenerationPrefabType</code> The serialized object generation prefab
         *      information
         * @param deserializedPrefab The <code>DeserializedPrefab</code> object that will contain the deserialized
         *      object generation prefab information
         *      
         * @returns IEnumerator The <code>IEnumerator</code> reentrant point for the asychronous handler
         */
        private IEnumerator LoadMeshAndMaterial(ObjectGenerationPrefabType serialized, DeserializedPrefab deserializedPrefab)
        {
            if (serialized.MeshAsset != null)
            {
                // Perform the asychronous loading of the mesh from the asset bundle
                System.Action<object> action = (object loadedMesh) =>
                {
                    FinishLoadingMesh((GameObject) loadedMesh, serialized, deserializedPrefab);
                };
                AssetBundleHelper.instance.LoadAssetAsync("file://" + Application.dataPath
                + "/StreamingAssets/Windows/" + serialized.MeshAsset.AssetBundle, serialized.MeshAsset.Name, typeof(GameObject), action);
            }

            yield return null;
        }
    }

}