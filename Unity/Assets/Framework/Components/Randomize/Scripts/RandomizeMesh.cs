using System.Collections.Generic;
using UnityEngine;

namespace GSFC.ARVR.MRET.Randomize
{
    /**
    * Randomizes a mesh to add variability in appearance. Take a 3D isosphere and randomizes
    * the verticies a bit and performs a smooth to create a new mesh. This class will
    * automatically randomize the scale on the current transform based upon the property
    * values at <code>Start</code>.
    * 
    * If this instance is disabled, the randomization will not occur until either the
    * instance is enabled, or a direct call to <code>PerformRandomization</code>.<br/>
    * 
    * The random seed for this instance may be specified to ensure deterministic results.
    * If the random seed is not specified, the current random seed is used.<br/>
    * 
    * @see Randomize
    * 
    * @author Weston Bell-Geddes
    * @author Jeffrey Hosler (Repackaged into Randomize framework and added comments)
    */
    public class RandomizeMesh : Randomize
    {
        public new static readonly string NAME = nameof(RandomizeMesh);

        public const float DEFAULT_OFFSET_MIN = 0.0f;
        public const float DEFAULT_OFFSET_MAX = 20.0f;

        public const int DEFAULT_SMOOTHING_MIN = 50000;
        public const int DEFAULT_SMOOTHING_MAX = 60000;

        [Tooltip("Minimum offset value (Inclusive).")]
        public float offsetMin = DEFAULT_OFFSET_MIN;
        [Tooltip("Maximum offset value (Inclusive)")]
        public float offsetMax = DEFAULT_OFFSET_MAX;

        [Tooltip("Minimum smoothing value (Inclusive)")]
        public int smoothingMin = DEFAULT_SMOOTHING_MIN;
        [Tooltip("Maximum smoothing value (Inclusive)")]
        public int smoothingMax = DEFAULT_SMOOTHING_MAX;

        /**
         * Overridden to perform the mesh randomization
         */
        protected override void PerformObjectRandomization()
        {
            float offset = Random.Range(offsetMin, offsetMax); // additional offset value

            Mesh mesh = GetComponent<MeshFilter>().mesh;
            if (mesh != null)
            {
                // lists to store 3D points of the vertices of the mesh
                List<Vector3> vertices = new List<Vector3>(); // vertices of current mesh
                List<Vector3> randomizedVertices = new List<Vector3>(); // final vertices of mesh

                // Adds verticies of icosphere to a list
                for (int i = 0; i < mesh.vertices.Length; i++)
                {
                    vertices.Add(mesh.vertices[i]);
                }

                // Center of the object
                Vector3 center = GetComponent<Renderer>().bounds.center;

                // Walk through each vertex in the mesh randomizing each one
                for (int j = 0; j < vertices.Count; j++)
                {
                    bool isUsed = false;
                    for (int k = 0; k < randomizedVertices.Count; k++)
                    {
                        if (randomizedVertices[k] == vertices[j])
                        {
                            isUsed = true;
                        }
                    }

                    if (!isUsed)
                    {
                        Vector3 curVector = vertices[j]; //
                        randomizedVertices.Add(curVector); //
                        int smoothing = Random.Range(smoothingMin, smoothingMax); // smoothes

                        // Modify the vertex
                        Vector3 changedVector = (curVector + ((curVector - center) * (Mathf.PerlinNoise((float)j / offset, (float)j / offset) / smoothing))); //

                        //
                        for (int l = 0; l < vertices.Count; l++)
                        {
                            if (vertices[l] == curVector)
                            {
                                vertices[l] = changedVector;
                            }
                        }
                    }

                    // Reassign the vertices
                    mesh.SetVertices(vertices);
                    mesh.RecalculateBounds();
                    mesh.RecalculateNormals();

                    // TODO: Recalculate collider?
                }
            }
            else
            {
                Debug.Log("[" + NAME + "]: Attached object does not contain a Mesh. Aborting randomization.");
            }

        }

        /**
         * Configures this RandomizeMesh instance from the supplied configuration settings.
         * 
         * @param config A <code>RandomizeConfig</code> containing the configuration settings
         * 
         * @see RandomizeConfig
         */
        public override void Configure(RandomizeConfig config)
        {
            // Take the inherited behavior
            base.Configure(config);

            // Process the mesh specific configuration
            if (config is RandomizeMeshConfig)
            {
                RandomizeMeshConfig meshConfig = config as RandomizeMeshConfig;

                offsetMin = meshConfig.offsetMin;
                offsetMax = meshConfig.offsetMax;

                smoothingMin = meshConfig.smoothingMin;
                smoothingMax = meshConfig.smoothingMax;
            }
            else
            {
                Debug.LogWarning("[" + NAME + "]: Unexpected configuration type: " + nameof(RandomizeTransformConfig));
            }
        }

    }

    /**
     * Helper class to contain configuration settings for a RandomizeMesh class. Useful
     * for maintaining a set of configuration values for future instantiation of a RandomizeMesh
     * class, i.e. deserialization with instantiation at a later time.
     * 
     * This class is maintained here to make maintenance on RandomizeMesh easier should changes to
     * the properties need to occur.
     */
    public class RandomizeMeshConfig : RandomizeConfig
    {
        public float offsetMin = RandomizeMesh.DEFAULT_OFFSET_MAX;
        public float offsetMax = RandomizeMesh.DEFAULT_OFFSET_MAX;

        public int smoothingMin = RandomizeMesh.DEFAULT_SMOOTHING_MIN;
        public int smoothingMax = RandomizeMesh.DEFAULT_SMOOTHING_MAX;
    }

}
