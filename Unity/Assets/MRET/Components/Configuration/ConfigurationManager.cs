// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Xml.Serialization;
using System.Collections.Generic;
using GOV.NASA.GSFC.XR.MRET.Schema;
using GOV.NASA.GSFC.XR.MRET.Schema.v0_9;
using Schema_v0_1 = GOV.NASA.GSFC.XR.MRET.Schema.v0_1;

namespace GOV.NASA.GSFC.XR.MRET.Configuration
{
    // TODO: This all needs to be refactored. It's atrocious.

    public class ProjectInfo
    {
        public string name = "UNSET";
        public string projFile = "UNSET";
        public DateTime timeStamp = new DateTime();
        public Texture2D thumbnail = null;

        public ProjectInfo(string _name, string _projFile, DateTime _timeStamp, Texture2D _thumbnail)
        {
            name = _name;
            projFile = _projFile;
            timeStamp = _timeStamp;
            thumbnail = _thumbnail;
        }
    }

    public class HudInfo
    {
        public string name = "UNSET";
        public string hudFile = "UNSET";
        public DateTime timeStamp = new DateTime();
        public Texture2D thumbnail = null;

        public HudInfo(string _name, string _hudFile, DateTime _timeStamp)
        {
            name = _name;
            hudFile = _hudFile;
            timeStamp = _timeStamp;
        }
    }

    /// <summary>
    /// TODO: user preferences need to be revisited
    /// </summary>
    public class Preferences
    {
        public enum ExplodeMode { Relative, Random }

        public const float DEFAULT_GLOBAL_EXPLODE_MIN = 1f;
        public const float DEFAULT_GLOBAL_EXPLODE_MAX = 10f;
        public const float DEFAULT_GLOBAL_EXPLODE_SCALE_VALUE = 5f;

        public bool globalExplodeMenuOn = true;

        public ExplodeMode globalExplodeMode = ExplodeMode.Relative;

        /// <summary>
        /// The global explode scalevalue preference
        /// </summary>
        private float _globalExplodeScaleValue = DEFAULT_GLOBAL_EXPLODE_SCALE_VALUE;
        public float GlobalExplodeScaleValue
        {
            get => _globalExplodeScaleValue;
            set
            {
                SetGlobalExplodeScaleValue(value);
            }
        }

        /// <summary>
        /// The global explode minimum value preference
        /// </summary>
        private float _globalExplodeMin = DEFAULT_GLOBAL_EXPLODE_MIN;
        public float GlobalExplodeMin
        {
            get => _globalExplodeMin;
            set
            {
                SetGlobalExplodeMin(value);
            }
        }

        /// <summary>
        /// The global explode maximum value preference
        /// </summary>
        private float _globalExplodeMax = DEFAULT_GLOBAL_EXPLODE_MAX;
        public float GlobalExplodeMax
        {
            get => _globalExplodeMax;
            set
            {
                SetGlobalExplodeMax(value);
            }
        }

        /// <summary>
        /// Sets the new global explode scale value. Validates against the min/max.
        /// </summary>
        /// <param name="newValue">The new scale value. If the new value extends beyond the min/max
        /// range, the value will be capped.</param>
        protected virtual void SetGlobalExplodeScaleValue(float newValue)
        {
            if (newValue < GlobalExplodeMin)
            {
                newValue = GlobalExplodeMin;
            }
            if (newValue > GlobalExplodeMax)
            {
                newValue = GlobalExplodeMax;
            }
            _globalExplodeScaleValue = newValue;
        }

        /// <summary>
        /// Sets the new global explode minimum value. Cannot go above the max.
        /// </summary>
        /// <param name="newMinValue">The new value. If the new value extends beyond the max,
        /// the value will be set to the max.</param>
        protected virtual void SetGlobalExplodeMin(float newMinValue)
        {
            if (newMinValue > GlobalExplodeMax)
            {
                newMinValue = GlobalExplodeMax;
            }
            _globalExplodeMin = newMinValue;
            if (GlobalExplodeScaleValue < GlobalExplodeMin)
            {
                GlobalExplodeScaleValue = GlobalExplodeMin;
            }
        }

        /// <summary>
        /// Sets the new global explode maximum value. Cannot go below the min.
        /// </summary>
        /// <param name="newMaxValue">The new value. If the new value extends below the min,
        /// the value will be set to the min.</param>
        protected virtual void SetGlobalExplodeMax(float newMaxValue)
        {
            if (newMaxValue < GlobalExplodeMin)
            {
                newMaxValue = GlobalExplodeMin;
            }
            _globalExplodeMax = newMaxValue;
            if (GlobalExplodeScaleValue > GlobalExplodeMax)
            {
                GlobalExplodeScaleValue = GlobalExplodeMax;
            }
        }
    }

    /// <summary>
    /// TODO: user preferences need to be revisited
    /// </summary>
    public class UserPreferences
    {
        public enum HeightType
        {
            minimum,
            belowAverage,
            average,
            aboveAverage,
            maximum
        }

        public enum WeightType
        {
            minimum,
            belowAverage,
            average,
            aboveAverage,
            maximum
        }

        public string avatarId = "";
        public float avatarSize = 0.5f;
        public float avatarWeight = 0.5f;
        public float avatarHeight = 0.5f;
        public float avatarLegLength = 0.5f;
        public float avatarArmLength = 0.5f;

        public void SetAvatarWeight(WeightType weight)
        {
            // Assign a weight based upon the type
            switch (weight)
            {
                case WeightType.maximum:
                    avatarWeight = 1.0f;
                    break;
                case WeightType.aboveAverage:
                    avatarWeight = 0.75f;
                    break;
                case WeightType.belowAverage:
                    avatarWeight = 0.25f;
                    break;
                case WeightType.minimum:
                    avatarWeight = 0.0f;
                    break;
                case WeightType.average:
                default:
                    avatarWeight = 0.5f;
                    break;
            }
        }

        public void SetAvatarHeight(HeightType height)
        {
            // Assign a height based upon the type
            switch (height)
            {
                case HeightType.maximum:
                    avatarHeight = 1.0f;
                    break;
                case HeightType.aboveAverage:
                    avatarHeight = 0.75f;
                    break;
                case HeightType.belowAverage:
                    avatarHeight = 0.25f;
                    break;
                case HeightType.minimum:
                    avatarHeight = 0.0f;
                    break;
                case HeightType.average:
                default:
                    avatarHeight = 0.5f;
                    break;
            }
        }

        public void Serialize()
        {

        }

        public void Deserialize()
        {

        }

        public void WriteToXML()
        {

        }

        public void ReadFromXML()
        {

        }
    }

    public class ConfigurationManager : MRETManager<ConfigurationManager>
    {
        /// <seealso cref="MRETBehaviour.ClassName"/>
        public override string ClassName => nameof(ConfigurationManager);

        public const string TYPEMAP_SERIALIZABLE_TYPES = "SerializableTypes";
        public const string DEFAULT_PREFERENCES_DIRECTORY = ".mret";

        public const float DEFAULT_COROUTINE_WAIT = 0f;

        public enum RecentType { Projects, Templates, Collaborative };
        public enum ColliderMode { None, Box, NonConvex };

        public Schema_v0_1.ConfigurationType config;
        public string configurationFileName = "Configuration.xml";
        public string typeMapFileName = "TypeMap.xml";
        public string recentProjectsFileName = ".recents";
        public string recentTemplatesFileName = ".rtemp";
        public int maxRecentProjects = 5;
        public string defaultAssetBundlesDirectory;
        public string defaultProjectDirectory;
        public string defaultTemplateDirectory;
        public string defaultPartDirectory;
        public string defaultMarkerDirectory;
        public string defaultTerrainDirectory;
        public string defaultAnimationDirectory;
        public string defaultAnnotationDirectory;
        public string defaultHUDDirectory;
        public string defaultPointCloudDirectory;
        public string defaultTimeSimulationDirectory;
        public string defaultUserDirectory;
        public string defaultPreferencesDirectory;
        public string defaultTypeMapDirectory;
        public string defaultSpiceKernelDirectory;
        public string rmitDirectory;
        public bool RMITAvailable => !string.IsNullOrEmpty(rmitDirectory);
        public bool validateXml = new Schema_v0_1.ConfigurationTypeXmlValidation().Value;
        public Schema_v0_1.XmlValidationType validateXmlType = new Schema_v0_1.ConfigurationTypeXmlValidation().type;

        /** TODO: Temp **/
        public Preferences preferences = new Preferences();
        public UserPreferences userPreferences = new UserPreferences();
        /**            **/

        public ColliderMode colliderMode = ColliderMode.None;

        public bool initialized = false;

        // TypeMap
        private Dictionary<Type, Type> serializableTypeMap = new Dictionary<Type, Type>();
        public Type LookupSerializableType(Type serializedType) => serializableTypeMap[serializedType];

        /// <seealso cref="MRETBehaviour.IntegrityCheck"/>
        protected override IntegrityState IntegrityCheck()
        {
            // Take the inherited behavior
            IntegrityState state = base.IntegrityCheck();

            // Custom integrity checks here
            return (
                (state == IntegrityState.Failure) // TODO: || (MyRequiredRef == null)
                    ? IntegrityState.Failure      // Fail is base class fails or anything is null
                    : IntegrityState.Success);    // Otherwise, our integrity is valid
        }


        /// <seealso cref="MRETManager{M}.Initialize"/>
        public override void Initialize()
        {
            // Take the inherited behavior
            base.Initialize();

            try
            {
                ReadConfig();
                ReadTypeMap();
                initialized = true;
            }
            catch (Exception e)
            {
                LogError("Initialization failed: " + e.ToString(), nameof(Initialize));
            }

#if HOLOLENS_BUILD
        CreateHololensDirectories();
#endif
        }

        public void AddRecentProject(ProjectInfo projInfo)
        {
            AddRecent(projInfo.projFile, RecentType.Projects);
        }

        public void AddRecentTemplate(ProjectInfo projInfo)
        {
            AddRecent(projInfo.projFile, RecentType.Templates);
        }

        public void AddRecentCollaboration(ProjectInfo projInfo)
        {
            AddRecent(projInfo.projFile, RecentType.Collaborative);
        }

        public void AddRecentProject(string projName)
        {
            AddRecent(projName, RecentType.Projects);
        }

        public void AddRecentTemplate(string projName)
        {
            AddRecent(projName, RecentType.Templates);
        }

        public void AddRecentCollaboration(string projName)
        {
            AddRecent(projName, RecentType.Collaborative);
        }

        public ProjectInfo[] GetRecentProjects()
        {
            return GetRecent(RecentType.Projects);
        }

        public ProjectInfo[] GetRecentTemplates()
        {
            return GetRecent(RecentType.Templates);
        }

        public ProjectInfo[] GetRecentCollaborations()
        {
            return GetRecent(RecentType.Collaborative);
        }

        private void AddRecent(string projName, RecentType typeToAdd)
        {
            try
            {
                string recentFileNameToUse = ".file";
                switch (typeToAdd)
                {
                    case RecentType.Projects:
                        recentFileNameToUse = recentProjectsFileName;
                        break;

                    case RecentType.Templates:
                        recentFileNameToUse = recentTemplatesFileName;
                        break;

                    case RecentType.Collaborative:
                        break;

                    default:
                        return;
                }

                if (!File.Exists(GetDatapath() + Path.DirectorySeparatorChar + recentFileNameToUse))
                {
                    FileStream fs = File.Create(GetDatapath() + Path.DirectorySeparatorChar + recentFileNameToUse);
                    fs.Close();
                    fs.Dispose();
                }

                int numProjectsRecorded = 1;
                string recentProjectsToWrite = projName;
                foreach (string line in File.ReadAllLines(GetDatapath() + Path.DirectorySeparatorChar + recentFileNameToUse))
                {
                    if (line != "\n" && line != projName)
                    {
                        if (numProjectsRecorded < maxRecentProjects)
                        {
                            recentProjectsToWrite += "\n" + line;
                            numProjectsRecorded++;
                        }
                    }
                }
                File.WriteAllText(GetDatapath() + Path.DirectorySeparatorChar + recentFileNameToUse, recentProjectsToWrite);
            }
            catch (Exception e)
            {
                LogWarning("A problem was encountered storing a recent project: " + e.ToString(), nameof(AddRecent));
            }

            switch (typeToAdd)
            {
                case RecentType.Projects:
                    break;

                case RecentType.Templates:
                    break;

                case RecentType.Collaborative:
                    break;

                default:
                    break;
            }
        }

        private ProjectInfo[] GetRecent(RecentType typeToGet)
        {
            List<ProjectInfo> projectsToReturn = new List<ProjectInfo>();

            string recentFileNameToUse = ".file";
            switch (typeToGet)
            {
                case RecentType.Projects:
                    recentFileNameToUse = recentProjectsFileName;
                    break;

                case RecentType.Templates:
                    recentFileNameToUse = recentTemplatesFileName;
                    break;

                case RecentType.Collaborative:
                    break;

                default:
                    return null;
            }

            if (File.Exists(GetDatapath() + Path.DirectorySeparatorChar + recentFileNameToUse))
            {
                foreach (string line in File.ReadAllLines(GetDatapath() + Path.DirectorySeparatorChar + recentFileNameToUse))
                {
                    if (line.EndsWith(ProjectFileSchema.FILE_EXTENSION))
                    {
                        int startIndex = line.LastIndexOf(Path.DirectorySeparatorChar) + 1;
                        string name = line.Substring(startIndex, Math.Max(0, line.Length - ProjectFileSchema.FILE_EXTENSION.Length - startIndex));
                        DateTime timeStamp = File.GetLastWriteTime(line);
                        Texture2D thumbnail = LoadThumbnail(line.Replace(ProjectFileSchema.FILE_EXTENSION, ".png"));

                        projectsToReturn.Add(new ProjectInfo(name, line, timeStamp, thumbnail));
                    }
                }
            }

            return projectsToReturn.ToArray();
        }

        private void ReadConfig()
        {
            try
            {
                // Get the top-level config object.
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Schema_v0_1.ConfigurationType));
                StreamReader sr = new StreamReader(Path.Combine(GetDatapath(), configurationFileName));
                config = (Schema_v0_1.ConfigurationType)xmlSerializer.Deserialize(sr);

                if (!string.IsNullOrEmpty(config.TypeMapPath))
                {
                    if (Directory.Exists(config.TypeMapPath))
                    {
                        defaultTypeMapDirectory = config.TypeMapPath;
                    }
                    else
                    {
                        defaultTypeMapDirectory = Path.Combine(GetDatapath(), config.TypeMapPath);
                    }
                }
                else
                {
                    defaultTypeMapDirectory = GetDatapath();
                }

                if (!string.IsNullOrEmpty(config.AssetBundlesPath))
                {
                    if (Directory.Exists(config.AssetBundlesPath))
                    {
                        defaultAssetBundlesDirectory = config.AssetBundlesPath;
                    }
                    else
                    {
                        defaultAssetBundlesDirectory = Path.Combine(GetDatapath(), config.AssetBundlesPath);
                    }
                }
                else
                {
#if (!UNITY_EDITOR && HOLOLENS_BUILD)
                //defaultAssetBundlesDirectory = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "MRET/UWP/");
                defaultAssetBundlesDirectory = Path.Combine(Windows.Storage.KnownFolders.DocumentsLibrary.Path, "MRET/UWP/");
#else
                    defaultAssetBundlesDirectory = Path.Combine(Application.streamingAssetsPath, "Windows");
#endif
                }

                if (!string.IsNullOrEmpty(config.ProjectsPath))
                {
                    if (Directory.Exists(config.ProjectsPath))
                    {
                        defaultProjectDirectory = config.ProjectsPath;
                    }
                    else
                    {
                        defaultProjectDirectory = Path.Combine(GetDatapath(), config.ProjectsPath);
                    }
                }
                else
                {
                    defaultProjectDirectory = GetDatapath();
                }

                if (!string.IsNullOrEmpty(config.UserPath))
                {
                    if (Directory.Exists(config.UserPath))
                    {
                        defaultUserDirectory = config.UserPath;
                    }
                    else
                    {
                        defaultUserDirectory = Path.Combine(GetDatapath(), config.UserPath);
                    }
                }
                else
                {
                    defaultUserDirectory = GetLocalFolder(Environment.SpecialFolder.UserProfile);
                }

                if (!string.IsNullOrEmpty(config.PreferencesPath))
                {
                    if (Directory.Exists(config.PreferencesPath))
                    {
                        defaultPreferencesDirectory = config.PreferencesPath;
                    }
                    else
                    {
                        defaultPreferencesDirectory = Path.Combine(GetDatapath(), config.PreferencesPath);
                    }
                }
                else
                {
                    defaultPreferencesDirectory = Path.Combine(defaultUserDirectory, DEFAULT_PREFERENCES_DIRECTORY);
                    if (!Directory.Exists(defaultPreferencesDirectory))
                    {
                        DirectoryInfo info = Directory.CreateDirectory(defaultPreferencesDirectory);
                    }
                }

                if (!string.IsNullOrEmpty(config.TemplatesPath))
                {
                    if (Directory.Exists(config.TemplatesPath))
                    {
                        defaultTemplateDirectory = config.TemplatesPath;
                    }
                    else
                    {
                        defaultTemplateDirectory = Path.Combine(GetDatapath(), config.TemplatesPath);
                    }
                }
                else
                {
                    defaultTemplateDirectory = GetDatapath();
                }

                if (!string.IsNullOrEmpty(config.AssetsPath))
                {
                    if (Directory.Exists(config.AssetsPath))
                    {
                        defaultPartDirectory = config.AssetsPath;
                    }
                    else
                    {
                        defaultPartDirectory = Path.Combine(GetDatapath(), config.AssetsPath);
                    }
                }
                else
                {
                    defaultPartDirectory = GetDatapath();
                }

                if (!string.IsNullOrEmpty(config.MarkersPath))
                {
                    if (Directory.Exists(config.MarkersPath))
                    {
                        defaultMarkerDirectory = config.MarkersPath;
                    }
                    else
                    {
                        defaultMarkerDirectory = Path.Combine(GetDatapath(), config.MarkersPath);
                    }
                }
                else
                {
                    defaultMarkerDirectory = GetDatapath();
                }

                if (!string.IsNullOrEmpty(config.TerrainsPath))
                {
                    if (Directory.Exists(config.TerrainsPath))
                    {
                        defaultTerrainDirectory = config.TerrainsPath;
                    }
                    else
                    {
                        defaultTerrainDirectory = Path.Combine(GetDatapath(), config.TerrainsPath);
                    }
                }
                else
                {
                    defaultTerrainDirectory = GetDatapath();
                }

                if (!string.IsNullOrEmpty(config.AnimationsPath))
                {
                    if (Directory.Exists(config.AnimationsPath))
                    {
                        defaultAnimationDirectory = config.AnimationsPath;
                    }
                    else
                    {
                        defaultAnimationDirectory = Path.Combine(GetDatapath(), config.AnimationsPath);
                    }
                }
                else
                {
                    defaultAnimationDirectory = GetDatapath();
                }

                if (!string.IsNullOrEmpty(config.TextAnnotationPath))
                {
                    if (Directory.Exists(config.TextAnnotationPath))
                    {
                        defaultAnnotationDirectory = config.TextAnnotationPath;
                    }
                    else
                    {
                        defaultAnnotationDirectory = Path.Combine(GetDatapath(), config.TextAnnotationPath);
                    }
                }
                else
                {
                    defaultAnnotationDirectory = GetDatapath();
                }

                if (!string.IsNullOrEmpty(config.HudPath))
                {
                    if (Directory.Exists(config.HudPath))
                    {
                        defaultHUDDirectory = config.HudPath;
                    }
                    else
                    {
                        defaultHUDDirectory = Path.Combine(GetDatapath(), config.HudPath);
                    }
                }
                else
                {
                    defaultHUDDirectory = GetDatapath();
                }

                if (!string.IsNullOrEmpty(config.PointCloudsPath))
                {
                    if (Directory.Exists(config.PointCloudsPath))
                    {
                        defaultPointCloudDirectory = config.PointCloudsPath;
                    }
                    else
                    {
                        defaultPointCloudDirectory = Path.Combine(GetDatapath(), config.PointCloudsPath);
                    }
                }
                else
                {
                    defaultPointCloudDirectory = GetDatapath();
                }

                if (!string.IsNullOrEmpty(config.TimeSimulationPath))
                {
                    if (Directory.Exists(config.TimeSimulationPath))
                    {
                        defaultTimeSimulationDirectory = config.TimeSimulationPath;
                    }
                    else
                    {
                        defaultTimeSimulationDirectory = Path.Combine(GetDatapath(), config.TimeSimulationPath);
                    }
                }
                else
                {
                    defaultTimeSimulationDirectory = GetDatapath();
                }

                if (!string.IsNullOrEmpty(config.SpiceKernelPath))
                {
                    if (Directory.Exists(config.SpiceKernelPath))
                    {
                        defaultSpiceKernelDirectory = config.SpiceKernelPath;
                    }
                    else
                    {
                        defaultSpiceKernelDirectory = Path.Combine(GetDatapath(), config.SpiceKernelPath);
                    }
                }
                else
                {
                    defaultSpiceKernelDirectory = GetDatapath();
                }

                // RMIT installation directory
                rmitDirectory = null;
                if (!string.IsNullOrEmpty(config.RMITPath))
                {
                    if (Directory.Exists(config.RMITPath))
                    {
                        rmitDirectory = config.RMITPath;
                    }
                }

                switch (config.ColliderMode)
                {
                    case Schema_v0_1.ConfigurationTypeColliderMode.None:
                        colliderMode = ColliderMode.None;
                        break;

                    case Schema_v0_1.ConfigurationTypeColliderMode.Box:
                        colliderMode = ColliderMode.Box;
                        break;

                    case Schema_v0_1.ConfigurationTypeColliderMode.NonConvex:
                        colliderMode = ColliderMode.NonConvex;
                        break;

                    default:
                        // Default to box.
                        colliderMode = ColliderMode.Box;
                        break;
                }

                if (config.XmlValidation != null)
                {
                    validateXml = config.XmlValidation.Value;
                    validateXmlType = config.XmlValidation.type;
                }

            }
            catch (Exception e)
            {
                LogError("A problem was encountered reading the typemap: " + e.ToString(), nameof(ReadConfig));
                MRET.Quit();
            }
        }

        private void ReadTypeMap()
        {
            bool needAbort = false;

            try
            {
                // Get the typemap file
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(LookupTableType));
                StreamReader sr = new StreamReader(Path.Combine(defaultTypeMapDirectory, typeMapFileName));
                LookupTableType serializedTypeMap = (LookupTableType)xmlSerializer.Deserialize(sr);

                // Locate the correct typemap namespace
                List<NamespaceTableType> serializedNamespaceTables = new List<NamespaceTableType>(serializedTypeMap.NamespaceTable);
                NamespaceTableType serializableTypesNamespace = serializedNamespaceTables.Find(table => table.name == TYPEMAP_SERIALIZABLE_TYPES);
                if (serializableTypesNamespace != null)
                {
                    // We will use reflection to obtain the system types for the typemap
                    var assembly = Assembly.GetExecutingAssembly();
                    string schemaNamespace = typeof(VersionedType).Namespace;

                    foreach (MappingType mapping in serializableTypesNamespace.Mapping)
                    {
                        try
                        {
                            string serializedTypeName = schemaNamespace + Type.Delimiter + mapping.name;
                            Type serializedType = assembly.GetType(serializedTypeName, true);
                            Type serializableType = assembly.GetType(mapping.value, true);

                            // Add the type mapping
                            serializableTypeMap.Add(serializedType, serializableType);
                        }
                        catch (Exception e)
                        {
                            LogError("Type could not be resolved: " + e, nameof(ReadTypeMap));
                            needAbort = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogError("A problem was encountered reading the typemap: " + e.ToString(), nameof(ReadTypeMap));
                needAbort = true;
            }

            if (needAbort) MRET.Quit();
        }

        private Texture2D LoadThumbnail(string thumbnailPath)
        {
            Texture2D returnTexture = null;

            if (File.Exists(thumbnailPath))
            {
                returnTexture = new Texture2D(2, 2);
                returnTexture.LoadImage(File.ReadAllBytes(thumbnailPath));
            }

            return returnTexture;
        }

        public string GetLocalFolder(Environment.SpecialFolder folder)
        {
#if (!UNITY_EDITOR && HOLOLENS_BUILD)
            return Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "MRET");
#else
            return Environment.GetFolderPath(folder);
#endif
        }

        public string GetDatapath()
        {
#if (!UNITY_EDITOR && HOLOLENS_BUILD)
            return Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "MRET");
#else
            return Application.dataPath;
#endif
        }

        private void CreateHololensDirectories()
        {
#if (!UNITY_EDITOR && HOLOLENS_BUILD)
        List<string> dirsToAdd = new List<string>();
        string mretDir = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "MRET");
        dirsToAdd.Add(mretDir);
        dirsToAdd.Add(Path.Combine(mretDir, "Anims"));
        dirsToAdd.Add(Path.Combine(mretDir, "Huds"));
        dirsToAdd.Add(Path.Combine(mretDir, "Parts"));
        dirsToAdd.Add(Path.Combine(mretDir, "Projects"));
        dirsToAdd.Add(Path.Combine(mretDir, "TimeSimulations"));
        dirsToAdd.Add(Path.Combine(mretDir, "UWP"));

        foreach (string dirToAdd in dirsToAdd)
        {
            Directory.CreateDirectory(dirToAdd);
        }
#endif
        }
    }
}