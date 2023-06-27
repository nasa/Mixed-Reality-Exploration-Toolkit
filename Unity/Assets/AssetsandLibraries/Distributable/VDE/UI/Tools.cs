/* 
 * This file is part of the Virtual Data Explorer distribution (https://coda.ee/vde).
 * Copyright (c) 2020 Kaur Kullman.
 */
using Assets.VDE.Communication;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.VDE.UI
{
    public class Tools
    {
        public static string ByteToHexBitFiddle(byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];
            int b;
            for (int i = 0; i < bytes.Length; i++)
            {
                b = bytes[i] >> 4;
                c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }
            return new string(c);
        }
        public static Vector3 CalculateQuadriaticBezierPoint(float t, Vector3[] points)
        {
            return (((1 - t) * (1 - t)) * points[0]) + (2 * (1 - t) * t * points[1]) + ((t * t) * points[2]);
        }
        public static Vector3 CalculateCubicBezierPoint(int positionOfThisPoint, int pointsOnLine, Vector3[] anchors)
        {
            return CalculateCubicBezierPoint((float)positionOfThisPoint / ((float)pointsOnLine + 1), anchors);
        }
        public static Vector3 CalculateCubicBezierPoint(float t, Vector3[] points)
        {
            // kudos to http://www.theappguruz.com/blog/bezier-curve-in-games

            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * points[0];
            p += 3 * uu * t * points[1];
            p += 3 * u * tt * points[2];
            p += ttt * points[3];
            return p;
        }
        public static Bounds CalculateBounds(GameObject min, GameObject max)
        {
            Renderer[] minrends = min.GetComponentsInChildren<Renderer>();
            Renderer[] maxrends = max.GetComponentsInChildren<Renderer>();
            Bounds bounds = minrends[0].bounds;
            bounds = bounds.GrowBounds(maxrends[0].bounds);
            return bounds;
        }
        public static Bounds CalculateBounds(GameObject GO)
        {
            Bounds bounds = new Bounds();
            try
            {
                Renderer[] rends = GO.GetComponentsInChildren<Renderer>();
                bounds = rends[0].bounds;
                foreach (Renderer rend in rends)
                {
                    bounds = bounds.GrowBounds(rend.bounds);
                }
            }
            catch (System.IndexOutOfRangeException exe )
            {
                Debug.LogWarning("calculating bounds for " + GO.name + " resulted with IndexOutOfRangeException: " + exe.Message);
            }
            catch (Exception exe) {
                Debug.LogWarning("calculating bounds for " + GO.name + " resulted with Exception: " + exe.Message);
            }

            return bounds;
        }
        public static void ResetTransform(Transform trans, bool activate = true)
        {
            Quaternion tmpRot = new Quaternion();
            tmpRot[0] = tmpRot[1] = tmpRot[2] = tmpRot[3] = 0;
            trans.rotation = tmpRot;
            trans.position = Vector3.zero;
            trans.localRotation = tmpRot;
            trans.localPosition = Vector3.zero;
            trans.localScale = Vector3.one;
            trans.gameObject.SetActive(activate);
        }
        /// <summary>
        /// this here is a hack to be able to show different views to an HMD and to the external/internal display during the same session.
        /// oculus is..
        /// </summary>
        /// <param name="camera"></param>
        public static void ResetCamera(Camera camera, int targetDisplay)
        {
            camera.gameObject.SetActive(false);
            camera.gameObject.SetActive(true);
            camera.targetDisplay = targetDisplay + 1;
            camera.targetDisplay = targetDisplay;
        }
        public static GameObject Cubify(
            Vector3 position,
            Vector3 scale,
            GameObject parent,
            Material material,
            PhysicMaterial materielDePhysique,
            string title,
            string tag = "",
            bool local = false,
            bool visible = true,
            bool collider = false,
            bool isTrigger = false,
            PrimitiveType type = PrimitiveType.Cube,
            Color color = new Color()
        )
        {
            if (color.a == 0)
            {
                color = new Color(1F, 1F, 1F, 0.009F);
            }

            GameObject cubeShape = GameObject.CreatePrimitive(type);
            cubeShape.name = title;
            if (tag != null && tag.Length > 0)
            {
                cubeShape.tag = tag;
                if (tag == "blade" || tag == "gazable")
                {
                    cubeShape.layer = LayerMask.NameToLayer(tag);
                }
            }
            cubeShape.transform.SetParent(parent.transform);
            cubeShape.transform.localScale = scale;
            cubeShape.GetComponent<Collider>().enabled = collider;
            cubeShape.GetComponent<Collider>().isTrigger = isTrigger;
            cubeShape.GetComponent<Collider>().sharedMaterial = materielDePhysique;

            if (!local)
            {
                cubeShape.transform.position = position;
            }
            else
            {
                cubeShape.transform.localPosition = position;
            }

            if (visible)
            {
                cubeShape.GetComponent<MeshRenderer>().material = material;
                cubeShape.GetComponent<MeshRenderer>().material.SetColor("_TintColor", color);
            }
            return cubeShape;
        }
        public static Rigidbody CreateDefaultRigidbody(
            GameObject aGameObject,
            bool kinematic = false,
            bool gravity = false,
            bool collie = true,
            float drag = 30F)
        {
            Rigidbody rigidBody = aGameObject.AddComponent<Rigidbody>();
            rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rigidBody.detectCollisions = collie;
            rigidBody.isKinematic = kinematic;
            rigidBody.useGravity = gravity;
            rigidBody.drag = drag;

            return rigidBody;
        }
        public static void CreateTagsAndLayers(List<string> tags, Dictionary<string, int> layers)
        {
            try
            {
#if UNITY_EDITOR
                UnityEditor.SerializedObject tagManager = new UnityEditor.SerializedObject(UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                UnityEditor.SerializedProperty tagsProp = tagManager.FindProperty("tags");
                UnityEditor.SerializedProperty layaProp = tagManager.FindProperty("layers");

                // *** Check the Tags
                foreach (string s in tags)
                {
                    // check if the tag is defined
                    bool found = false;
                    for (int i = 0; i < tagsProp.arraySize; i++)
                    {
                        UnityEditor.SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
                        if (t.stringValue.Equals(s))
                        {
                            found = true;
                            break;
                        }
                    }

                    // if not found, add it
                    if (!found)
                    {
                        tagsProp.InsertArrayElementAtIndex(0);
                        UnityEditor.SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
                        n.stringValue = s;
                    }
                }

                // now add layers
                foreach (KeyValuePair<string, int> kv in layers)
                {
                    UnityEditor.SerializedProperty sp = layaProp.GetArrayElementAtIndex(kv.Value);
                    if (sp.stringValue.Length == 0)
                    {
                        sp.stringValue = kv.Key;
                    }
                }

                // *** save changes
                tagManager.ApplyModifiedProperties();
#endif
            }
            catch (System.Exception exe)
            {
                throw new Message()
                {
                    Fatal = Message.Fatals.ErrorAddingLayersAndTags,
                    message = exe.Message
                };
            }

            GameObject testGO = new GameObject();
            string var = "";

            try
            {
                foreach (var tag in tags)
                {
                    var = tag;
                    testGO.tag = tag;
                }
            }
            catch (System.Exception)
            {
                throw new Message() { Fatal = Message.Fatals.ErrorAddingLayersAndTags, message = "Tag with name " + var + " has not been defined in Unity's Tags&Slayers. Please do that before compiling." };
            }

            try
            {
                foreach (var layer in layers)
                {
                    var = layer.Key;
                    testGO.layer = LayerMask.NameToLayer(layer.Key);
                }
            }
            catch (System.Exception)
            {
                throw new Message() { Fatal = Message.Fatals.ErrorAddingLayersAndTags, message = "Layer with name " + var + " has not been defined in Unity's Tags&Slayers. Please do that before compiling." };
            }
            UnityEngine.Object.Destroy(testGO);
        }
    }
}
