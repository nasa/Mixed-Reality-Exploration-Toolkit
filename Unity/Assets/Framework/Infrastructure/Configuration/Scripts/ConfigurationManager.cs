using System;
using System.IO;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using GSFC.ARVR.MRET.Common.Schemas;

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

public class AnimationInfo
{
    public string name = "UNSET";
    public string animationFile = "UNSET";
    public DateTime timeStamp = new DateTime();

    public AnimationInfo(string _name, string _animationFile, DateTime _timeStamp)
    {
        name = _name;
        animationFile = _animationFile;
        timeStamp = _timeStamp;
    }
}

public class AnnotationInfo
{
    public string name = "UNSET";
    public string annotationFile = "UNSET";
    public DateTime timeStamp = new DateTime();
    public enum AnnotationType { Text, Audio }
    public AnnotationType type;

    public AnnotationInfo(string _name, string _annotationFile, AnnotationType _type, DateTime _timeStamp)
    {
        name = _name;
        annotationFile = _annotationFile;
        type = _type;
        timeStamp = _timeStamp;
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

public class AssetInfo
{
    public string name = "UNSET";
    public string assetFile = "UNSET";
    public string description = "";
    public DateTime timeStamp = new DateTime();
    public Texture2D thumbnail = null;
    public List<string[]> filters = new List<string[]>();

    public AssetInfo(string _name, string _assetFile, DateTime _timeStamp, Texture2D _thumbnail, string _description)
    {
        name = _name;
        assetFile = _assetFile;
        timeStamp = _timeStamp;
        thumbnail = _thumbnail;
        description = _description;
    }

    public void AddFilter(string key, string value)
    {
        bool filterExists = false;
        foreach (string[] filter in filters)
        {   // If filter key exists, update value.
            if (filter[0] == key)
            {
                filterExists = true;
                filter[1] = value;
                break;
            }
        }

        // If filter key does not exist, create new one.
        if (!filterExists)
        {
            filters.Add(new string[] { key, value });
        }
    }

    public bool CheckFilter(string key, string value)
    {
        // No filter.
        if (key == "" || value == "")
        {
            return true;
        }

        // Check to see if there is a match.
        foreach (string[] filter in filters)
        {
            if (filter[0] == key)
            {
                if (filter[1] == value)
                {
                    return true;
                }
            }
        }
        return false;
    }
}

public class TAssetInfo
{
    public string name = "UNSET";
    public string assetFile = "UNSET";
    public string description = "";
    public DateTime timeStamp = new DateTime();
    public Texture2D thumbnail = null;
    //public List<string[]> filters = new List<string[]>();

    public TAssetInfo(string _name, string _assetFile, DateTime _timeStamp, Texture2D _thumbnail, string _description)
    {
        name = _name;
        assetFile = _assetFile;
        timeStamp = _timeStamp;
        thumbnail = _thumbnail;
        description = _description;
    }
}

public class AssetFilterList
{
    private List<string> key = new List<string>();
    private List<string> value = new List<string>();

    public void Add(string filterKey, string filterValue)
    {
        // Iterate through filters to see if value exists.
        bool filterAlreadyExists = false;
        for (int i = 0; i < key.Count; i++)
        {
            if (key[i] == filterKey)
            {
                if (value[i] == filterValue)
                {
                    filterAlreadyExists = true;
                    break;
                }
            }
        }

        if (!filterAlreadyExists)
        {
            // Value does not exist yet, add to list.
            key.Add(filterKey);
            value.Add(filterValue);
        }
    }

    public void Add(AssetInfo assetToAddFilters)
    {
        XmlSerializer ser = new XmlSerializer(typeof(PartType));
        XmlReader reader = XmlReader.Create(assetToAddFilters.assetFile);
        PartType prt = (PartType)ser.Deserialize(reader);

        // Check if Vendor field exists for part.
        if (prt.Vendor != null)
        {
            if (prt.Vendor[0] != null && prt.Vendor[0] != "")
            {
                Add("Vendor", prt.Vendor[0]);
                assetToAddFilters.AddFilter("Vendor", prt.Vendor[0]);
            }
        }

        // Check if Subsystem field exists for part.
        if (prt.Subsystem != null)
        {
            if (prt.Subsystem != "")
            {
                Add("Subsystem", prt.Subsystem);
                assetToAddFilters.AddFilter("Subsystem", prt.Subsystem);
            }
        }
    }

    public List<string[]> Get()
    {
        List<string[]> returnVal = new List<string[]>();

        for (int i = 0; i < key.Count; i++)
        {
            returnVal.Add(new string[] { key[i], value[i] });
        }

        return returnVal;
    }
}

public class PointCloudInfo
{
    public string name = "UNSET";
    public string pcFile = "UNSET";
    public DateTime timeStamp = new DateTime();

    public PointCloudInfo(string _name, string _pcFile, DateTime _timeStamp)
    {
        name = _name;
        pcFile = _pcFile;
        timeStamp = _timeStamp;
    }
}

public class ConfigurationManager : MonoBehaviour
{
    public static ConfigurationManager instance;

    public enum RecentType { Projects, Templates, Collaborative };
    public enum ColliderMode { None, Box, NonConvex };

    public ConfigurationType config;
    public string configurationFileName = "Configuration.xml";
    public string recentProjectsFileName = ".recents";
    public string recentTemplatesFileName = ".rtemp";
    public int maxRecentProjects = 5;
    public string defaultProjectDirectory;
    public string defaultTemplateDirectory;
    public string defaultPartDirectory;
    public string defaultTerrainDirectory;
    public string defaultAnimationDirectory;
    public string defaultAnnotationDirectory;
    public string defaultHUDDirectory;
    public string defaultPointCloudDirectory;
    public string defaultTimeSimulationDirectory;

    public List<ProjectInfo> projects = new List<ProjectInfo>();
    public List<ProjectInfo> templates = new List<ProjectInfo>();
    public List<AssetInfo> assets = new List<AssetInfo>();
    public List<TAssetInfo> Tassets = new List<TAssetInfo>();
    public List<AnimationInfo> animations = new List<AnimationInfo>();
    public List<AnnotationInfo> annotations = new List<AnnotationInfo>();
    public List<HudInfo> huds = new List<HudInfo>();
    public List<PointCloudInfo> pcs = new List<PointCloudInfo>();
    public ColliderMode colliderMode = ColliderMode.None;

    public AssetFilterList assetFilters = new AssetFilterList();
    public bool initialized = false;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        try
        {
            ReadConfig();
            initialized = true;
        }
        catch (Exception e)
        {
            Debug.Log("[ConfigurationManager->Start] " + e.ToString());
        }
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

            if (!File.Exists(Application.dataPath + "/" + recentFileNameToUse))
            {
                FileStream fs = File.Create(Application.dataPath + "/" + recentFileNameToUse);
                fs.Close();
                fs.Dispose();
            }

            int numProjectsRecorded = 1;
            string recentProjectsToWrite = projName;
            foreach (string line in File.ReadAllLines(Application.dataPath + "/" + recentFileNameToUse))
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
            File.WriteAllText(Application.dataPath + "/" + recentFileNameToUse, recentProjectsToWrite);
        }
        catch (Exception e)
        {
            Debug.Log("[ConfigurationManager->AddRecentProject] " + e.ToString());
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

        if (File.Exists(Application.dataPath + "/" + recentFileNameToUse))
        {
            foreach (string line in File.ReadAllLines(Application.dataPath + "/" + recentFileNameToUse))
            {
                if (line.Substring(Math.Max(0, line.Length - 7)).Equals(".mtproj"))
                {
                    int startIndex = line.LastIndexOf(Path.DirectorySeparatorChar) + 1;
                    string name = line.Substring(startIndex, Math.Max(0, line.Length - 7 - startIndex));
                    DateTime timeStamp = File.GetLastWriteTime(line);
                    Texture2D thumbnail = LoadThumbnail(line.Replace(".mtproj", ".png"));

                    projectsToReturn.Add(new ProjectInfo(name, line, timeStamp, thumbnail));
                }
            }
        }

        return projectsToReturn.ToArray();
    }

    /*public void UpdateConfig()
    {
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(ConfigurationType));
        StreamReader sr = new StreamReader(Application.dataPath + Path.DirectorySeparatorChar + configurationFileName);
        config = (ConfigurationType)xmlSerializer.Deserialize(sr);

        huds.Clear();

        String dirPath = Application.dataPath + Path.DirectorySeparatorChar + config.HudPath;
        foreach (string hudFile in Directory.GetFiles(dirPath))
        {
            if (hudFile.Substring(Math.Max(0, hudFile.Length - 4)).Equals(".xml"))
            {
                int startIndex = hudFile.LastIndexOf(Path.DirectorySeparatorChar) + 1;
                string name = hudFile.Substring(startIndex, Math.Max(0, hudFile.Length - 4 - startIndex));
                DateTime timeStamp = File.GetLastWriteTime(hudFile);

                huds.Add(new HudInfo(name, hudFile, timeStamp));
            }
        }
    }*/


    private void ReadConfig()
    {
        try
        {
            // Get the top-level config object.
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ConfigurationType));
            StreamReader sr = new StreamReader(Application.dataPath + Path.DirectorySeparatorChar + configurationFileName);
            config = (ConfigurationType)xmlSerializer.Deserialize(sr);

            if (!string.IsNullOrEmpty(config.ProjectsPath))
            {
                defaultProjectDirectory
                    = Path.Combine(Application.dataPath, config.ProjectsPath);
            }
            else
            {
                defaultProjectDirectory = Application.dataPath;
            }

            if (!string.IsNullOrEmpty(config.TemplatesPath))
            {
                defaultTemplateDirectory
                    = Path.Combine(Application.dataPath, config.TemplatesPath);
            }
            else
            {
                defaultTemplateDirectory = Application.dataPath;
            }

            if (!string.IsNullOrEmpty(config.AssetsPath))
            {
                defaultPartDirectory
                    = Path.Combine(Application.dataPath, config.AssetsPath);
            }
            else
            {
                defaultPartDirectory = Application.dataPath;
            }

            //if (!string.IsNullOrEmpty(config.TerrainsPath))
            {
                defaultTerrainDirectory
                    = Path.Combine(Application.dataPath, "Terrains"); // config.TerrainsPath
            }
            //else
            //{
            //    defaultTerrainDirectory = Application.dataPath;
            //}

            if (!string.IsNullOrEmpty(config.AnimationsPath))
            {
                defaultAnimationDirectory
                    = Path.Combine(Application.dataPath, config.AnimationsPath);
            }
            else
            {
                defaultAnimationDirectory = Application.dataPath;
            }

            if (!string.IsNullOrEmpty(config.TextAnnotationPath))
            {
                defaultAnnotationDirectory
                    = Path.Combine(Application.dataPath, config.TextAnnotationPath);
            }
            else
            {
                defaultAnnotationDirectory = Application.dataPath;
            }

            if (!string.IsNullOrEmpty(config.HudPath))
            {
                defaultHUDDirectory
                    = Path.Combine(Application.dataPath, config.HudPath);
            }
            else
            {
                defaultHUDDirectory = Application.dataPath;
            }

            if (!string.IsNullOrEmpty(config.PointCloudsPath))
            {
                defaultPointCloudDirectory
                    = Path.Combine(Application.dataPath, config.PointCloudsPath);
            }
            else
            {
                defaultPointCloudDirectory = Application.dataPath;
            }

            if (!string.IsNullOrEmpty(config.TimeSimulationPath))
            {
                defaultTimeSimulationDirectory
                    = Path.Combine(Application.dataPath, config.TimeSimulationPath);
            }
            else
            {
                defaultTimeSimulationDirectory = Application.dataPath;
            }

            /*// Read all of the projects into a list.
            string dirPath = Application.dataPath + Path.DirectorySeparatorChar + config.ProjectsPath;
            if (Directory.Exists(dirPath))
            {
                foreach (string projFile in Directory.GetFiles(dirPath))
                {
                    if (projFile.Substring(Math.Max(0, projFile.Length - 7)).Equals(".mtproj"))
                    {
                        int startIndex = projFile.LastIndexOf(Path.DirectorySeparatorChar) + 1;
                        string name = projFile.Substring(startIndex, Math.Max(0, projFile.Length - 7 - startIndex));
                        DateTime timeStamp = File.GetLastWriteTime(projFile);
                        Texture2D thumbnail = LoadThumbnail(projFile.Replace(".mtproj", ".png"));

                        projects.Add(new ProjectInfo(name, projFile, timeStamp, thumbnail));
                    }
                }
            }

            // Read all of the templates into a list.
            dirPath = Application.dataPath + Path.DirectorySeparatorChar + config.TemplatesPath;
            if (Directory.Exists(dirPath))
            {
                foreach (string projFile in Directory.GetFiles(dirPath))
                {
                    if (projFile.Substring(Math.Max(0, projFile.Length - 7)).Equals(".mtproj"))
                    {
                        int startIndex = projFile.LastIndexOf(Path.DirectorySeparatorChar) + 1;
                        string name = projFile.Substring(startIndex, Math.Max(0, projFile.Length - 7 - startIndex));
                        DateTime timeStamp = File.GetLastWriteTime(projFile);

                        templates.Add(new ProjectInfo(name, projFile, timeStamp, null));
                    }
                }
            }

            // Read all of the HUDs into a list
            dirPath = Application.dataPath + Path.DirectorySeparatorChar + config.HudPath;
            if (Directory.Exists(dirPath))
            {
                foreach (string hudFile in Directory.GetFiles(dirPath))
                {
                    if (hudFile.Substring(Math.Max(0, hudFile.Length - 4)).Equals(".xml"))
                    {
                        int startIndex = hudFile.LastIndexOf(Path.DirectorySeparatorChar) + 1;
                        string name = hudFile.Substring(startIndex, Math.Max(0, hudFile.Length - 4 - startIndex));
                        DateTime timeStamp = File.GetLastWriteTime(hudFile);

                        huds.Add(new HudInfo(name, hudFile, timeStamp));
                    }
                }
            }

            // Read all of the animations into a list
            dirPath = Application.dataPath + Path.DirectorySeparatorChar + config.AnimationsPath;
            if (Directory.Exists(dirPath))
            {
                foreach (string animationFile in Directory.GetFiles(dirPath))
                {
                    if (animationFile.Substring(Math.Max(0, animationFile.Length - 7)).Equals(".mtanim"))
                    {
                        int startIndex = animationFile.LastIndexOf(Path.DirectorySeparatorChar) + 1;
                        string name = animationFile.Substring(startIndex, Math.Max(0, animationFile.Length - 7 - startIndex));
                        DateTime timeStamp = File.GetLastWriteTime(animationFile);

                        animations.Add(new AnimationInfo(name, animationFile, timeStamp));
                    }
                }
            }

            // Read all of the text annotations into a list
            dirPath = Application.dataPath + Path.DirectorySeparatorChar + config.TextAnnotationPath;
            if (Directory.Exists(dirPath))
            {
                foreach (string annotationFile in Directory.GetFiles(dirPath))
                {
                    if (annotationFile.Substring(Math.Max(0, annotationFile.Length - 8)).Equals(".mtannot"))
                    {
                        int startIndex = annotationFile.LastIndexOf(Path.DirectorySeparatorChar) + 1;
                        string name = annotationFile.Substring(startIndex, Math.Max(0, annotationFile.Length - 4 - startIndex));
                        DateTime timeStamp = File.GetLastWriteTime(annotationFile);

                        annotations.Add(new AnnotationInfo(name, annotationFile, AnnotationInfo.AnnotationType.Text, timeStamp));
                    }
                }
            }

            // Read all of the audio annotations into a list
            dirPath = Application.dataPath + Path.DirectorySeparatorChar + config.AudioRecordingPath;
            if (Directory.Exists(dirPath))
            {
                foreach (string annotationFile in Directory.GetFiles(dirPath))
                {
                    if (annotationFile.Substring(Math.Max(0, annotationFile.Length - 8)).Equals(".mtannot"))
                    {
                        int startIndex = annotationFile.LastIndexOf(Path.DirectorySeparatorChar) + 1;
                        string name = annotationFile.Substring(startIndex, Math.Max(0, annotationFile.Length - 4 - startIndex));
                        DateTime timeStamp = File.GetLastWriteTime(annotationFile);

                        annotations.Add(new AnnotationInfo(name, annotationFile, AnnotationInfo.AnnotationType.Audio, timeStamp));
                    }
                }
            }

            // Read all of the parts into a list.
            dirPath = Application.dataPath + Path.DirectorySeparatorChar + config.AssetsPath;
            if (Directory.Exists(dirPath))
            {
                foreach (string assetFile in Directory.GetFiles(dirPath))
                {
                    if (assetFile.Substring(Math.Max(0, assetFile.Length - 4)).Equals(".xml"))
                    {
                        int startIndex = assetFile.LastIndexOf(Path.DirectorySeparatorChar) + 1;
                        string name = assetFile.Substring(startIndex, Math.Max(0, assetFile.Length - 4 - startIndex));
                        DateTime timeStamp = File.GetLastWriteTime(assetFile);
                        Texture2D thumbnail = LoadThumbnail(assetFile.Replace(".xml", ".png"));

                        // Get any information from the part file.
                        XmlSerializer pSer = new XmlSerializer(typeof(PartType));
                        XmlReader pReader = XmlReader.Create(assetFile);
                        PartType prt = (PartType)pSer.Deserialize(pReader);

                        string description = "";
                        if (prt != null)
                        {
                            if (prt.Description != null)
                            {
                                description = prt.Description[0];
                            }
                        }

                        assets.Add(new AssetInfo(name, assetFile, timeStamp, thumbnail, description));
                        assetFilters.Add(assets[assets.Count - 1]);
                    }
                }
            }

            // Read all of the Terrains into a list.
            dirPath = Application.dataPath + Path.DirectorySeparatorChar + "Terrains"; // config.TAssetsPath;
            if (Directory.Exists(dirPath))
            {
                foreach (string assetFile in Directory.GetFiles(dirPath))
                {
                    if (assetFile.Substring(Math.Max(0, assetFile.Length - 4)).Equals(".xml"))
                    {
                        int startIndex = assetFile.LastIndexOf(Path.DirectorySeparatorChar) + 1;
                        string name = assetFile.Substring(startIndex, Math.Max(0, assetFile.Length - 4 - startIndex));
                        DateTime timeStamp = File.GetLastWriteTime(assetFile);
                        Texture2D thumbnail = LoadThumbnail(assetFile.Replace(".xml", ".png"));

                        Tassets.Add(new TAssetInfo(name, assetFile, timeStamp, thumbnail, ""));
                    }
                }
            }

            // Read all of the point clouds into a list
            dirPath = Application.dataPath + Path.DirectorySeparatorChar + config.PointCloudsPath;
            if (Directory.Exists(dirPath))
            {
                foreach (string pcFile in Directory.GetFiles(dirPath))
                {
                    if (pcFile.Substring(Math.Max(0, pcFile.Length - 8)).Equals(".mpoints"))
                    {
                        int startIndex = pcFile.LastIndexOf(Path.DirectorySeparatorChar) + 1;
                        string name = pcFile.Substring(startIndex, Math.Max(0, pcFile.Length - 4 - startIndex));
                        DateTime timeStamp = File.GetLastWriteTime(pcFile);

                        pcs.Add(new PointCloudInfo(name, pcFile, timeStamp));
                    }
                }
            }*/

            switch (config.ColliderMode)
            {
                case ConfigurationTypeColliderMode.None:
                    colliderMode = ColliderMode.None;
                    break;

                case ConfigurationTypeColliderMode.Box:
                    colliderMode = ColliderMode.Box;
                    break;

                case ConfigurationTypeColliderMode.NonConvex:
                    colliderMode = ColliderMode.NonConvex;
                    break;

                default:
                    // Default to box.
                    colliderMode = ColliderMode.Box;
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.Log("[ConfigurationManager->ReadConfig] " + e.ToString());
        }
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
}