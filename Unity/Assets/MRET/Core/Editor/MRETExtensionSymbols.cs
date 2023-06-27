// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

#if UNITY_EDITOR
using GOV.NASA.GSFC.XR.MRET.Editor;

// Detect if specific packages are installed and, if so,
// define scripting symbols for MRET conditional compilation.

// RECOMMENDED
[assembly: MRETExtension("DigitalRuby.Earth.SphereScript", "MRET_EXTENSION_DIGITALRUBYEARTH")]
[assembly: MRETExtension("Siccity.GLTFUtility.Importer", "MRET_EXTENSION_SICCITYGLTF")]
[assembly: MRETExtension("Dummiesman.OBJLoader", "MRET_EXTENSION_OBJIMPORTER")]
[assembly: MRETExtension("Microsoft.MixedReality.OpenXR.ControllerModel", "MRET_EXTENSION_MROPENXR")]
[assembly: MRETExtension("Microsoft.MixedReality.Toolkit.MixedRealityToolkit", "MRET_EXTENSION_MRTK")]

// OPTIONAL
[assembly: MRETExtension("ChartAndGraph.GraphChart", "MRET_EXTENSION_CHARTANDGRAPH")]
[assembly: MRETExtension("EasyBuildSystem.Features.Scripts.Core.Base.Manager.BuildManager", "MRET_EXTENSION_EASYBUILDSYSTEM")]
[assembly: MRETExtension("RootMotion.FinalIK.CCDIK", "MRET_EXTENSION_FINALIK")]
[assembly: MRETExtension("OSGeo.GDAL.Gdal", "MRET_EXTENSION_GDAL")]
[assembly: MRETExtension("uPLibrary.Networking.M2Mqtt.MqttClient", "MRET_EXTENSION_M2MQTT")]
[assembly: MRETExtension("NonConvexMeshCollider", "MRET_EXTENSION_NONCONVEXMESHCOLLIDER")]
[assembly: MRETExtension("PointOctree`1", "MRET_EXTENSION_UNITYOCTREE")]
[assembly: MRETExtension("unitycodercom_PointCloudBinaryViewer.PointCloudViewerDX11", "MRET_EXTENSION_POINTCLOUDVIEWER")]
[assembly: MRETExtension("RockVR.Video.VideoCaptureCtrl", "MRET_EXTENSION_ROCKVR")]
[assembly: MRETExtension("RosSharp.RosBridgeClient.RosConnector", "MRET_EXTENSION_ROSSHARP")]
[assembly: MRETExtension("System.Text.Json.JsonSerializer", "MRET_EXTENSION_SYSTEMTEXTJSON")]
[assembly: MRETExtension("Newtonsoft.Json.JsonConvert", "MRET_EXTENSION_NEWTONSOFTJSON")]
[assembly: MRETExtension("UnityEngine.Experimental.TerrainAPI.Heightmap", "MRET_EXTENSION_TERRAINTOOLS")]
[assembly: MRETExtension("Assets.VDE.VDE", "MRET_EXTENSION_VDE")]
[assembly: MRETExtension("Vuplex.WebView.StandaloneWebPlugin", "MRET_EXTENSION_VUPLEX")]
[assembly: MRETExtension("Vuplex.WebView.UwpWebPlugin", "MRET_EXTENSION_VUPLEX")]

// REQUIRED FOR VDE
[assembly: MRETExtension("BestHTTP.SignalRCore.HubConnection", "MRET_EXTENSION_BESTHTTP")]

#endif
