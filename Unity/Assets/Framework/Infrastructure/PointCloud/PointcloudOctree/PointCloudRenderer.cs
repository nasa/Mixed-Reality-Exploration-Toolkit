// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.IO.MemoryMappedFiles;
using UnityEngine;
using System.Diagnostics;
using Octree;
//**********************************************************
//
//**********************************************************

namespace PointCloud
{
    public class PointCloudRenderer : MonoBehaviour
    {
        //******************************************************
        //classes
        //private Renderer_PCE_Camera_Manager _GD;
        //******************************************************
        //declarations
        public string Glopal_ImportPath;
        public bool bUseLocalPath = true;
        private string _RootPath;//our constant path to where our pointclouds are stored
        private string _RootPath_Final;
        public string Pointcloud_Name;
        //private int ActiveCellCount = 0;
        private int LOD_Count = 0;
        public int LOD_Index = 5;
        public bool BackFaceCulling = false;
        public bool Lighting = false;

        public float Fresnel_Min = 0.25f;
        public float Fresnel_Max = 1.0f;
        public float Fresnel_Multiplier = 0.32f;
        public float Reflect = 0.75f;
        public float Refract = 0.75f;

        public GameObject scaleToMatch;

        //public bool NoStreaming = false;
        private bool NoVoxel = false;

        public Light DirectionalLight = null;
        public ReflectionProbe _ReflectionProbe = null;

        private Camera _Camera = null;

        private bool bHasNormals = false;

        public GameObject trackedController = null;
        public float[][] PointCloudDataOctree;

        //private int SectorSize = 16;
        //private bool bIsVisible_Main = false;
        //private Vector3 PreviousPos = new Vector3(0.0f, 0.0f, 0.0f);
        //private float MainSideLength = 1.0f;

        //private Quaternion Orientation_Current = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
        //private Vector3 Position_Current = new Vector3(0.0f, 0.0f, 0.0f);
        //private Matrix4x4 _Mat_World = Matrix4x4.identity;

        private int _Per_Point_Data_Count = 6;

        private int BufferCount;
        private int[] BufferSize;
        private int MaxBufferSize;

        public float[][] Pointcloud_Data_Final;
        public ComputeBuffer[] cb_Float_Vert_Dynamic;
        public ComputeBuffer[][] cb_Float_Vert_Dynamic_DP;
        private int Live_Buffer_Index = 0;

        //private Vector4[] pl_n;
        //private ComputeBuffer cb_VoxelVerts_CF;
        //private Vector4[] VoxelVerts_CF;

        //private int LOD_Rez;

        public ComputeShader cs_Fill_Buffer_Points = null;
        private int cs_Kernel_Fill_Buffer_Points = 0;
        public Material _Mat_Fill_Buffer;

        private Matrix4x4 _W_Matrix;
        private Matrix4x4 _W_Matrix_Inv;
        private bool bCanUpdatePosition = true;
        //******************************************************
        //bounds
        public struct AABB_MAIN
        {
            public Vector3 Min;
            public Vector3 Max;
            public Vector3 Center;
            //public Vector3 Color;
            public float Length;
            public float HalfLength;
            public bool bIsActive;
            public int ID;
            public int CellPointCount;

            public Bounds Bound;

            //public Renderer_PCE_Custom_Classes.AABB _AABB;

            public void Init()
            {
                Min = new Vector3(0.0f, 0.0f, 0.0f);
                Max = new Vector3(0.0f, 0.0f, 0.0f);
                Center = new Vector3(0.0f, 0.0f, 0.0f);
                //Color = new Vector3(1.0f, 1.0f, 1.0f);
                Length = 1.0f;
                HalfLength = 0.5f;
                bIsActive = false;
                ID = 0;
                CellPointCount = 0;

                Bound = new Bounds();
                Bound.center = Center;
                Bound.extents = new Vector3(0.5f, 0.5f, 0.5f);
                Bound.min = Center - new Vector3(0.5f, 0.5f, 0.5f);
                Bound.max = Center + new Vector3(0.5f, 0.5f, 0.5f);
                Bound.size = new Vector3(1.0f, 1.0f, 1.0f);
            }
        };

        private AABB_MAIN AABB_Main;
        //------------------------------------------------------
        private Stopwatch Timer;
        //------------------------------------------------------
#if UNITY_EDITOR
        //debug visual - editor visual
        private GameObject[] _GO;
        private Mesh[] _Mesh;
        private MeshFilter[] _MeshFilter;
        private Renderer[] _MeshRenderer;
        private int NumberOfMeshes = 1;

        private const string TAG_HANDLE = "debug_pc_VertHandle_soAwesome";

        public Material DebugVisualMat;
#endif
        private bool bCanDraw = false;

        private GameObject _GameObject;

        //******************************************************
        //START
        //******************************************************
        void Start()
        {
            UnityEngine.Debug.Log("PointCloudRenderer started");
        }

        public void Initialize()
        {
            //--------------------------------------------------
            _GameObject = GameObject.FindGameObjectWithTag("MainCamera");
            //_GD = _GameObject.GetComponent<Renderer_PCE_Camera_Manager>();
            //--------------------------------------------------
            //get main camera
            _GameObject = GameObject.FindGameObjectWithTag("MainCamera");
            _Camera = _GameObject.GetComponent<Camera>();

            if (_Camera == null) { print("Set Camera with Tag - MainCamera"); }

            Vector3 cameraPosition = new Vector3(_Camera.transform.position.x, _Camera.transform.position.y, _Camera.transform.position.z);
            UnityEngine.Debug.Log("Camera position: " + cameraPosition);

            //--------------------------------------------------
            //compute shaders
            if (cs_Fill_Buffer_Points != null)
            {
                cs_Kernel_Fill_Buffer_Points = cs_Fill_Buffer_Points.FindKernel("PointCloud");
            }
            //--------------------------------------------------
            //voxel loader file - bounds data, max voxel rez, LOD count and cell count (how many 1x1x1 cells make up cloud)
            string ReadPath_Data = "";
            //if (bUseLocalPath)
            {
                // = _RootPath + Pointcloud_Name + "/Voxel_Point_Data/";
                //ReadPath_Data = _RootPath_Final + "Voxel_Loader_File.binary";
            }
            //else
            {
                _RootPath_Final = Glopal_ImportPath + "/" + Pointcloud_Name + "/Voxel_Point_Data/";
                ReadPath_Data = _RootPath_Final + "Voxel_Loader_File.binary";
            }
            //--------------------------------------------------
            //read basic data
            BinaryReader Read_Data = new BinaryReader(File.Open(ReadPath_Data, FileMode.Open));
            //--------------------------------------------------
            //main bounds
            AABB_Main.Init();
            AABB_Main.bIsActive = true;

            AABB_Main.Min.x = Read_Data.ReadSingle();
            AABB_Main.Min.y = Read_Data.ReadSingle();
            AABB_Main.Min.z = Read_Data.ReadSingle();
            AABB_Main.Max.x = Read_Data.ReadSingle();
            AABB_Main.Max.y = Read_Data.ReadSingle();
            AABB_Main.Max.z = Read_Data.ReadSingle();
            AABB_Main.Center.x = Read_Data.ReadSingle();
            AABB_Main.Center.y = Read_Data.ReadSingle();
            AABB_Main.Center.z = Read_Data.ReadSingle();
            AABB_Main.Length = Read_Data.ReadSingle();
            AABB_Main.HalfLength = Read_Data.ReadSingle();
            AABB_Main.CellPointCount = Read_Data.ReadInt32();

            AABB_Main.Bound = new Bounds();
            AABB_Main.Bound.center = AABB_Main.Center;
            AABB_Main.Bound.extents = new Vector3(AABB_Main.HalfLength, AABB_Main.HalfLength, AABB_Main.HalfLength);
            AABB_Main.Bound.min = AABB_Main.Center - new Vector3(AABB_Main.HalfLength, AABB_Main.HalfLength, AABB_Main.HalfLength);
            AABB_Main.Bound.max = AABB_Main.Center + new Vector3(AABB_Main.HalfLength, AABB_Main.HalfLength, AABB_Main.HalfLength);
            AABB_Main.Bound.size = new Vector3(AABB_Main.Length, AABB_Main.Length, AABB_Main.Length);

            _Per_Point_Data_Count = Read_Data.ReadInt32();
            LOD_Count = Read_Data.ReadInt32();
            bHasNormals = Read_Data.ReadBoolean();

            Read_Data.Close();

            //--------------------------------------------------
            //set lod index - for now statically set
            int _t_LOD_Index = 4;

            if (LOD_Index < 0) { LOD_Index = 0; }
            if (LOD_Index >= LOD_Count) { LOD_Index = (LOD_Count - 1); }
            //if (NoStreaming)
            //{
            _t_LOD_Index = LOD_Index;
            //}
            //--------------------------------------------------
            //read in data
            string ReadPath_Sector_Point_Data_All = _RootPath_Final;
            ReadPath_Sector_Point_Data_All += AABB_Main.Min.x.ToString() + "_" +
                                              AABB_Main.Min.z.ToString() + "_" +
                                              AABB_Main.Min.y.ToString() + "/";

            ReadPath_Sector_Point_Data_All += "LOD_" + _t_LOD_Index.ToString() + "/";

            ReadPath_Sector_Point_Data_All += "Original_Cell_Data_All.binary";

            byte[] _b = File.ReadAllBytes(ReadPath_Sector_Point_Data_All);

            int Length_Actual = (_b.Length / 4) / _Per_Point_Data_Count;// file holds 5 value per sub cell - 5 x int (size of int = 4 bytes);
            int Length_Of_All = Length_Actual * _Per_Point_Data_Count;

            print(Length_Actual);//point count

            //float[] _f_data = new float[Length_Of_All];
            
            ////copy bytes to float array
            //System.Buffer.BlockCopy(_b, 0, _f_data, 0, _b.Length);


            //--------------------------------------------------
            //create and fill buffers
            MaxBufferSize = (1024 * 1024);
            int bs = Length_Actual;
            BufferCount = 1;

            if (bs > MaxBufferSize)
            {
                float AVO = bs;
                float DVC = MaxBufferSize;
                BufferCount = Mathf.CeilToInt((float)(AVO / DVC));

                int tt = (MaxBufferSize) * BufferCount;


                BufferSize = new int[BufferCount];

                if (bs < tt)
                {
                    for (int j = 0; j < BufferCount; j++)
                    {
                        if (j < (BufferCount - 1))
                        {
                            BufferSize[j] = MaxBufferSize;
                        }
                        else
                        {
                            BufferSize[j] = (MaxBufferSize) - (tt - bs);
                        }
                    }
                }
                else
                {
                    BufferSize[0] = bs;
                }
            }
            else
            {
                BufferSize = new int[1];
                BufferSize[0] = bs;
            }

            Pointcloud_Data_Final = new float[BufferCount][];

            for (int j = 0; j < BufferCount; j++)
            {
                Pointcloud_Data_Final[j] = new float[(BufferSize[j] * _Per_Point_Data_Count)];
            }

            cb_Float_Vert_Dynamic = new ComputeBuffer[BufferCount];
            cb_Float_Vert_Dynamic_DP = new ComputeBuffer[2][];
            cb_Float_Vert_Dynamic_DP[0] = new ComputeBuffer[BufferCount];
            //cb_Float_Vert_Dynamic_DP[1] = new ComputeBuffer[BufferCount];

            int _src_pos = 0;
            for (int j = 0; j < BufferCount; j++)
            {
                System.Buffer.BlockCopy(_b, _src_pos, Pointcloud_Data_Final[j], 0, ((BufferSize[j] * 4) * _Per_Point_Data_Count));//copy bytes to float array

                _src_pos = _src_pos + ((BufferSize[j] * 4) * _Per_Point_Data_Count);
                //compute buffer
                cb_Float_Vert_Dynamic[j] = new ComputeBuffer(Pointcloud_Data_Final[j].Length, 4);

                cb_Float_Vert_Dynamic[j].SetData(Pointcloud_Data_Final[j]);
                cb_Float_Vert_Dynamic_DP[0][j] = new ComputeBuffer(MaxBufferSize, 32);
                //cb_Float_Vert_Dynamic_DP[1][j] = new ComputeBuffer(MaxBufferSize, 32);
            }


            for (int i = 0; i < Pointcloud_Data_Final.Length; i++)
            {
                for(int j = 0; j < Pointcloud_Data_Final[i].Length; j++)
                {
                    try
                    {
                        Vector3 point = new Vector3(Pointcloud_Data_Final[i][j], Pointcloud_Data_Final[i][++j], Pointcloud_Data_Final[i][++j]);
                        AABB_Main.Bound.Encapsulate(point);

                    }
                    catch (IndexOutOfRangeException)
                    {
                        UnityEngine.Debug.Log("Leftover vector values from pointcloud; throwing away");
                    }
                }
            }

            PointCloudDataOctree = Pointcloud_Data_Final;



            //--------------------------------------------------
            //_f_data = null;
            _b = null;
            //--------------------------------------------------
            //pointcloud position and rotation and scale
            _W_Matrix = Matrix4x4.TRS(this.transform.position, this.transform.rotation, this.transform.localScale);
            _W_Matrix_Inv = Matrix4x4.Inverse(_W_Matrix);
            //--------------------------------------------------
            //light
            Vector3 LightRay = new Vector3(0.0f, 0.0f, 1.0f);
            float Ambient = 0.5f; //DirectionalLight.color.a;
            Vector3 LightAmbient = new Vector3(Ambient, Ambient, Ambient);
            Vector3 LightColor = new Vector3(1.0f, 1.0f, 1.0f);
            if (DirectionalLight != null)
            {
                LightRay = DirectionalLight.transform.rotation * LightRay;
                Ambient = DirectionalLight.color.a;
                LightAmbient = new Vector3(Ambient, Ambient, Ambient);
                LightColor = new Vector3(DirectionalLight.color.r, DirectionalLight.color.g, DirectionalLight.color.b);
            }
            //--------------------------------------------------
            //update compute shader
            for (int i = 0; i < BufferCount; i++)
            {
                int StateSize = 1024;
                //----------------------------------------------
                //reflection probe
                if (_ReflectionProbe != null)
                {
                    if (_ReflectionProbe.texture != null)
                    {
                        cs_Fill_Buffer_Points.SetTexture(cs_Kernel_Fill_Buffer_Points, "_Reflection_Probe", _ReflectionProbe.texture);
                    }
                }
                else
                {
                    //print("WTF!");
                }
                //----------------------------------------------
                cs_Fill_Buffer_Points.SetVector("_CameraPos", new Vector4(cameraPosition.x, cameraPosition.y, cameraPosition.z, 1.0f));
                cs_Fill_Buffer_Points.SetVector("_DirLight", new Vector4(LightRay.x, LightRay.y, LightRay.z, 1.0f));
                cs_Fill_Buffer_Points.SetVector("_DirLightColor", new Vector4(LightColor.x, LightColor.y, LightColor.z, 1.0f));
                cs_Fill_Buffer_Points.SetVector("_DirLightAmbient", new Vector4(LightAmbient.x, LightAmbient.y, LightAmbient.z, 1.0f));
                cs_Fill_Buffer_Points.SetFloat("_Fresnel_Min", Fresnel_Min);
                cs_Fill_Buffer_Points.SetFloat("_Fresnel_Max", Fresnel_Max);
                cs_Fill_Buffer_Points.SetFloat("_Fresnel_Multiplier", Fresnel_Multiplier);
                cs_Fill_Buffer_Points.SetFloat("_Reflect", Reflect);
                cs_Fill_Buffer_Points.SetFloat("_Refract", Refract);
                cs_Fill_Buffer_Points.SetBool("Lighting", Lighting);
                cs_Fill_Buffer_Points.SetBool("BackFaceCulling", BackFaceCulling);
                cs_Fill_Buffer_Points.SetBool("HasNormals", bHasNormals);
                cs_Fill_Buffer_Points.SetMatrix("_W_Matrix", _W_Matrix);
                cs_Fill_Buffer_Points.SetMatrix("_W_Matrix_Inv", _W_Matrix_Inv);
                cs_Fill_Buffer_Points.SetInt("StateSize", StateSize);
                cs_Fill_Buffer_Points.SetInt("Per_Point_Data_Count", _Per_Point_Data_Count);
                cs_Fill_Buffer_Points.SetInt("StartIndex", 0);// (j * (MaxBufferSize * _Per_Point_Data_Count)));
                cs_Fill_Buffer_Points.SetBuffer(cs_Kernel_Fill_Buffer_Points, "pc", cb_Float_Vert_Dynamic[i]);
                cs_Fill_Buffer_Points.SetInt("MaxBufferSize", BufferSize[i]);
                cs_Fill_Buffer_Points.SetBuffer(cs_Kernel_Fill_Buffer_Points, "vPoints", cb_Float_Vert_Dynamic_DP[0][i]);

                ModifyPointVolumeColor();
                ModifyPointVolumePosition();

                cs_Fill_Buffer_Points.Dispatch(cs_Kernel_Fill_Buffer_Points, StateSize / 32, StateSize / 32, 1);
                //----------------------------------------------
            }
            if (!bCanDraw)
            {
                bCanDraw = true;
            }
            //--------------------------------------------------
            //add to list of renderer
            //if (_GD != null)
            //{
            //    _GD._iPointCloudStaticListModify();
            //}

            //--------------------------------------------------
        }
        //******************************************************
        //UPDATE
        //******************************************************
        void Update()
        {
            MatchScale();

            //--------------------------------------------------
            //pointcloud position and rotation and scale
            _W_Matrix = Matrix4x4.TRS(this.transform.position, this.transform.rotation, this.transform.localScale);
            _W_Matrix_Inv = Matrix4x4.Inverse(_W_Matrix);
            //--------------------------------------------------
            //light
            Vector3 LightRay = new Vector3(0.0f, 0.0f, 1.0f);
            float Ambient = 0.5f; //DirectionalLight.color.a;
            Vector3 LightAmbient = new Vector3(Ambient, Ambient, Ambient);
            Vector3 LightColor = new Vector3(1.0f, 1.0f, 1.0f);
            if (DirectionalLight != null)
            {
                LightRay = DirectionalLight.transform.rotation * LightRay;
                Ambient = DirectionalLight.color.a;
                LightAmbient = new Vector3(Ambient, Ambient, Ambient);
                LightColor = new Vector3(DirectionalLight.color.r, DirectionalLight.color.g, DirectionalLight.color.b);
            }
            //--------------------------------------------------
            //update compute shader
            int t_Live_Buffer_Index = Live_Buffer_Index;
            //if (t_Live_Buffer_Index == 0) { t_Live_Buffer_Index = 1; }
            //else if (t_Live_Buffer_Index == 1) { t_Live_Buffer_Index = 0; }

            for (int i = 0; i < BufferCount; i++)
            {
                int StateSize = 1024;
                //----------------------------------------------
                //reflection probe
                if (_ReflectionProbe != null)
                {
                    if (_ReflectionProbe.texture != null)
                    {
                        cs_Fill_Buffer_Points.SetTexture(cs_Kernel_Fill_Buffer_Points, "_Reflection_Probe", _ReflectionProbe.texture);
                    }
                }
                else
                {
                    //print("WTF!");
                }
                //----------------------------------------------
                cs_Fill_Buffer_Points.SetVector("_CameraPos", new Vector4(_Camera.transform.position.x, _Camera.transform.position.y, _Camera.transform.position.z, 1.0f));
                cs_Fill_Buffer_Points.SetVector("_DirLight", new Vector4(LightRay.x, LightRay.y, LightRay.z, 1.0f));
                cs_Fill_Buffer_Points.SetVector("_DirLightColor", new Vector4(LightColor.x, LightColor.y, LightColor.z, 1.0f));
                cs_Fill_Buffer_Points.SetVector("_DirLightAmbient", new Vector4(LightAmbient.x, LightAmbient.y, LightAmbient.z, 1.0f));
                cs_Fill_Buffer_Points.SetFloat("_Fresnel_Min", Fresnel_Min);
                cs_Fill_Buffer_Points.SetFloat("_Fresnel_Max", Fresnel_Max);
                cs_Fill_Buffer_Points.SetFloat("_Fresnel_Multiplier", Fresnel_Multiplier);
                cs_Fill_Buffer_Points.SetFloat("_Reflect", Reflect);
                cs_Fill_Buffer_Points.SetFloat("_Refract", Refract);
                cs_Fill_Buffer_Points.SetBool("Lighting", Lighting);
                cs_Fill_Buffer_Points.SetBool("BackFaceCulling", BackFaceCulling);
                cs_Fill_Buffer_Points.SetBool("HasNormals", bHasNormals);
                cs_Fill_Buffer_Points.SetMatrix("_W_Matrix", _W_Matrix);
                cs_Fill_Buffer_Points.SetMatrix("_W_Matrix_Inv", _W_Matrix_Inv);
                cs_Fill_Buffer_Points.SetInt("StateSize", StateSize);
                cs_Fill_Buffer_Points.SetInt("Per_Point_Data_Count", _Per_Point_Data_Count);
                cs_Fill_Buffer_Points.SetInt("StartIndex", 0);// (j * (MaxBufferSize * _Per_Point_Data_Count)));
                cs_Fill_Buffer_Points.SetBuffer(cs_Kernel_Fill_Buffer_Points, "pc", cb_Float_Vert_Dynamic[i]);
                cs_Fill_Buffer_Points.SetInt("MaxBufferSize", BufferSize[i]);
                cs_Fill_Buffer_Points.SetBuffer(cs_Kernel_Fill_Buffer_Points, "vPoints", cb_Float_Vert_Dynamic_DP[t_Live_Buffer_Index][i]);
                cs_Fill_Buffer_Points.Dispatch(cs_Kernel_Fill_Buffer_Points, StateSize / 32, StateSize / 32, 1);
                //----------------------------------------------
            }
            if (!bCanDraw)
            {
                bCanDraw = true;
            }
        }

        /** ADDING MY OWN FUNCTIONS HERE - MARK **/
        // -----------------------------------------------------
        void ModifyPointVolumeColor()
        {
            Vector4 min = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            Vector4 max = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

            Vector4[] minMaxValues = new Vector4[2];
            minMaxValues[0] = min;
            minMaxValues[1] = max;
            // TODO: Fix size, just put it at max for now
            ComputeBuffer minMax = new ComputeBuffer(MaxBufferSize, 32);
            minMax.SetData(minMaxValues);
            Vector4 col = new Vector4(0f, 0f, 1f, 1f);
            cs_Fill_Buffer_Points.SetBuffer(cs_Kernel_Fill_Buffer_Points, "minMaxColor", minMax);
            cs_Fill_Buffer_Points.SetVector("modifiedColor", col);

        }

        void AddPoint()
        {

        }

        void DeletePoint()
        {

        }

        void ModifyPointVolumePosition()
        {
            Vector4 min = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            Vector4 max = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

            Vector4[] minMaxValues = new Vector4[2];
            minMaxValues[0] = min;
            minMaxValues[1] = max;

            // Note that in modified_pos_W_Matrix the rotation I have defaulted so that I can figure out why points are not showing up directly on the controller
            Vector3 scale = new Vector3(0.0f, 0.0f, 0.0f);
            Matrix4x4 modified_pos_W_Matrix = Matrix4x4.TRS(trackedController.transform.position, this.transform.rotation, trackedController.transform.localScale + scale);

            // TODO: Fix size, just put it at max for now
            ComputeBuffer minMax = new ComputeBuffer(MaxBufferSize, 32);
            minMax.SetData(minMaxValues);
            cs_Fill_Buffer_Points.SetMatrix("modified_pos_W_Matrix", modified_pos_W_Matrix);
            cs_Fill_Buffer_Points.SetBuffer(cs_Kernel_Fill_Buffer_Points, "minMaxPosition", minMax);

            // Debugging junk, leave for now, delete when done
            //Vector4 modifiedPos = new Vector4(257.6f, 258.8f, 0f, 1.0f);
            // Vector3 modifiedPos = new Vector3(trackedController.transform.position.x, trackedController.transform.position.y, trackedController.transform.position.z);
            //Vector3 modifiedPos = trackedController.transform.TransformPoint(trackedController.transform.position.x, trackedController.transform.position.y, trackedController.transform.position.z);
            //cs_Fill_Buffer_Points.SetVector("modifiedPosition", modifiedPos);
        }

        // TODO: Make these private and add accessors to them

        public float[][] GetPointCloudData()
        {
            UnityEngine.Debug.Log("pointcloudDataOctree at this time: " + PointCloudDataOctree[0][1]);
            return PointCloudDataOctree;
        }

        // using this instead of GetPointCloudSize to mess around with the bounds
        public Bounds GetPointCloudBounds()
        {
            return AABB_Main.Bound;
        }

        public Vector3 GetPointCloudSize()
        {
            return AABB_Main.Bound.size;
        }

        public Vector3 GetPointCloudPosition()
        {
            return this.gameObject.transform.position;
        }

        // -----------------------------------------------------
        //** END ADDING MY OWN FUNCTIONS HERE - MARK **/
        //******************************************************
        //ON RENDER OBJECT - Draw Procedural
        //******************************************************
        //public void _iOnRenderObject()
        void OnRenderObject()
        {
            //--------------------------------------------------
            if (bCanDraw)
            {
                for (int i = 0; i < BufferCount; i++)
                {
                    _Mat_Fill_Buffer.SetPass(0);

                    _Mat_Fill_Buffer.SetBuffer("points", cb_Float_Vert_Dynamic_DP[Live_Buffer_Index][i]);

                    Graphics.DrawProceduralNow(MeshTopology.Points, MaxBufferSize);// BufferSize[i]);
                }
            }
            //--------------------------------------------------
        }
        //******************************************************
        //ON APPLICATION QUIT
        //******************************************************
        void OnApplicationQuit()
        {
            //--------------------------------------------------
            if (cb_Float_Vert_Dynamic != null)
            {
                for (int j = 0; j < BufferCount; j++)
                {
                    if (cb_Float_Vert_Dynamic[j] != null)
                    {
                        cb_Float_Vert_Dynamic[j].Dispose();
                        cb_Float_Vert_Dynamic[j].Release();
                        cb_Float_Vert_Dynamic[j] = null;
                    }
                }
            }
            if (cb_Float_Vert_Dynamic_DP != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (cb_Float_Vert_Dynamic_DP[i] != null)
                    {
                        for (int j = 0; j < BufferCount; j++)
                        {
                            if (cb_Float_Vert_Dynamic_DP[i][j] != null)
                            {
                                cb_Float_Vert_Dynamic_DP[i][j].Dispose();
                                cb_Float_Vert_Dynamic_DP[i][j].Release();
                                cb_Float_Vert_Dynamic_DP[i][j] = null;
                            }
                        }
                    }
                }
            }
            Pointcloud_Data_Final = null;
            BufferSize = null;
            //--------------------------------------------------
        }
        //******************************************************
        //ON DESTROY
        //******************************************************
        void OnDestroy()
        {
            //--------------------------------------------------
            if (cb_Float_Vert_Dynamic != null)
            {
                for (int j = 0; j < BufferCount; j++)
                {
                    if (cb_Float_Vert_Dynamic[j] != null)
                    {
                        cb_Float_Vert_Dynamic[j].Dispose();
                        cb_Float_Vert_Dynamic[j].Release();
                        cb_Float_Vert_Dynamic[j] = null;
                    }
                }
            }
            if (cb_Float_Vert_Dynamic_DP != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (cb_Float_Vert_Dynamic_DP[i] != null)
                    {
                        for (int j = 0; j < BufferCount; j++)
                        {
                            if (cb_Float_Vert_Dynamic_DP[i][j] != null)
                            {
                                cb_Float_Vert_Dynamic_DP[i][j].Dispose();
                                cb_Float_Vert_Dynamic_DP[i][j].Release();
                                cb_Float_Vert_Dynamic_DP[i][j] = null;
                            }
                        }
                    }
                }
            }
            Pointcloud_Data_Final = null;
            BufferSize = null;
            //--------------------------------------------------
            //add to list of renderer
            //if (_GD != null)
            //{
            //    _GD._iPointCloudStaticListModify();
            //}
            //--------------------------------------------------
        }
        //******************************************************
        //DEBUG VISUAL - CREATE - used to create the visual point cloud for the dynamic type
        //******************************************************
#if UNITY_EDITOR
        [ContextMenu("Point Cloud Visual Create")]
        void VisualCreate()
        {
            //--------------------------------------------------
            _i_DebugVisualCreate(this.gameObject);//,  _GD.DebugVisualMat);
                                                  //_PCE_Draw_V1.iDebugVisualCreate(this.gameObject, _RootPath + _MMF_LoaderFile.name + "/" + _MMF_LoaderFile.name, DebugVisualMat);
                                                  //--------------------------------------------------
        }
        //******************************************************
        //DEBUG VISUAL - DESTROY - used to destroy the visual point cloud for the dynamic type
        //******************************************************
        [ContextMenu("Point Cloud Visual Destroy")]
        void VisualDestroy()
        {
            //--------------------------------------------------
            _i_DebugVisualDestroy(this.gameObject);
            //_PCE_Draw_V1.iDebugVisualDestroy(this.gameObject);
            //--------------------------------------------------
        }
        void Reset()
        {
            VisualDestroy();
        }

        //******************************************************
        //DEBUG VISUAL - DESTROY
        //******************************************************
        public void _i_DebugVisualDestroy(GameObject _PC_GO)
        {
            //--------------------------------------------------
            Transform[] children = _PC_GO.GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
            {
                if (child.name == TAG_HANDLE)
                {
                    GameObject.DestroyImmediate(child.gameObject);
                }
            }
            //--------------------------------------------------
        }
        //******************************************************
        // DEBUG VISUAL - CREATE
        //******************************************************
        public void _i_DebugVisualCreate(GameObject _PC_GO
                                       //string _RootPath,
                                       )//Material _DebugVisualMat)
        {
            //--------------------------------------------------
            if (!Application.isPlaying)
            {
                _i_DebugVisualDestroy(_PC_GO);

                //DebugVisualMat = _DebugVisualMat;

                //--------------------------------------------------
                //voxel loader file - bounds data, max voxel rez, LOD count and cell count (how many 1x1x1 cells make up cloud)
                string ReadPath_Data = "";
                if (bUseLocalPath)
                {
                    ReadPath_Data = _RootPath + Pointcloud_Name + "/DebugVisual.binary";
                }
                else
                {
                    ReadPath_Data = Glopal_ImportPath + Pointcloud_Name + "/DebugVisual.binary";
                }
                //--------------------------------------------------
                if (File.Exists(ReadPath_Data))
                {
                    _i_LoadLowDesityPointCloudVisual(_PC_GO, ReadPath_Data);

                    //_PC_GO.enabled = false;
                }
            }
            //--------------------------------------------------
        }
        //******************************************************
        //LOW DENSITY POINT CLOUD VISUAL
        //******************************************************
        private void _i_LoadLowDesityPointCloudVisual(GameObject _PC_GO, string _FP_Position)
        {
            //--------------------------------------------------
            byte[] byte_P = File.ReadAllBytes(_FP_Position);

            int ActualLength_Orig = (byte_P.Length / 4) / 6;

            int ActualLength = ActualLength_Orig * 3;
            float[] fPos = new float[ActualLength];
            float[] fCol = new float[ActualLength];

            int _tpos = 0;
            int _fpos = 0;
            for (int i = 0; i < ActualLength_Orig; i++)
            {
                System.Buffer.BlockCopy(byte_P, _tpos, fPos, _fpos, (3 * 4));

                _fpos = _fpos + (3 * 4);

                _tpos = _tpos + (6 * 4);
            }
            _tpos = (3 * 4);
            _fpos = 0;
            for (int i = 0; i < ActualLength_Orig; i++)
            {
                System.Buffer.BlockCopy(byte_P, _tpos, fCol, _fpos, (3 * 4));

                _fpos = _fpos + (3 * 4);

                _tpos = _tpos + (6 * 4);
            }
            //System.Buffer.BlockCopy(byte_P, 0, fPos, 0, byte_P.Length);
            //System.Buffer.BlockCopy(byte_C, 0, fCol, 0, byte_C.Length);

            int desiredCountPerMesh = 64000;
            if (desiredCountPerMesh > ActualLength_Orig) { desiredCountPerMesh = ActualLength_Orig; }
            NumberOfMeshes = (int)Mathf.FloorToInt(((float)ActualLength_Orig / (float)desiredCountPerMesh));

            _GO = new GameObject[NumberOfMeshes];
            _Mesh = new Mesh[NumberOfMeshes];
            _MeshFilter = new MeshFilter[NumberOfMeshes];
            _MeshRenderer = new MeshRenderer[NumberOfMeshes];

            List<Vector3>[] positions = new List<Vector3>[NumberOfMeshes];
            int[][] indices = new int[NumberOfMeshes][];
            List<Color>[] colors = new List<Color>[NumberOfMeshes];

            int c = 0;

            for (int i = 0; i < NumberOfMeshes; i++)
            {
                positions[i] = new List<Vector3>(desiredCountPerMesh);
                indices[i] = new int[desiredCountPerMesh];
                colors[i] = new List<Color>(desiredCountPerMesh);

                _GO[i] = new GameObject(TAG_HANDLE);
                _GO[i].gameObject.AddComponent<MeshFilter>();
                _GO[i].gameObject.AddComponent<MeshRenderer>();
                _MeshRenderer[i] = new MeshRenderer();
                _MeshRenderer[i] = _GO[i].GetComponent<MeshRenderer>();

                //Material Mat = myMat;// new Material(Shader.Find("DX11/EPCV"));
                _MeshRenderer[i].material = DebugVisualMat;// (Material)Resources.Load("#OI_PointCloudEngine_Data\\EPCV_Material", typeof(Material)); //AssetDatabase.GetBuiltinExtraResource<Material>("EPCV_Material");
                _MeshRenderer[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                _GO[i].layer = LayerMask.NameToLayer("EditorLayer");
                _Mesh[i] = new Mesh();
                _MeshFilter[i] = new MeshFilter();
                _MeshFilter[i] = _GO[i].GetComponent<MeshFilter>();

                _MeshFilter[i].mesh = new Mesh();

                for (int j = 0; j < desiredCountPerMesh; j++)
                {
                    if (c < ActualLength)
                    {
                        Vector3 s1 = _PC_GO.transform.localScale;
                        Vector3 v = _PC_GO.transform.position + new Vector3(fPos[c] * s1.x, fPos[c + 1] * s1.y, fPos[c + 2] * s1.z);
                        positions[i].Add(v);
                        indices[i][j] = j;
                        colors[i].Add(new Color(fCol[c], fCol[c + 1], fCol[c + 2], 1.0f));
                        c++;
                        c++;
                        c++;
                    }
                }

                _Mesh[i].Clear();
                _Mesh[i].SetVertices(positions[i]);
                _Mesh[i].SetIndices(indices[i], MeshTopology.Points, 0);
                _Mesh[i].SetColors(colors[i]);


                _GO[i].hideFlags = HideFlags.HideInHierarchy;
                _MeshFilter[i].sharedMesh = _Mesh[i];

                _GO[i].transform.SetParent(_PC_GO.transform);
                //--------------------------------------------------
            }
            byte_P = null;
            //byte_C = null;
            fPos = null;
            fCol = null;
        }
#endif

        private void MatchScale()
        {
            if (scaleToMatch)
            {
                transform.localScale = scaleToMatch.transform.localScale;
            }
        }
    }
}
