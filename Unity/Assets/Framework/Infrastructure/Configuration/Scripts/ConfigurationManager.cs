// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;
using System.IO;
using UnityEngine;
using System.Xml.Serialization;
using System.Collections.Generic;

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

    public ColliderMode colliderMode = ColliderMode.None;

    public bool initialized = false;

    public void Initialize()
    {
        instance = this;

        try
        {
            ReadConfig();
            initialized = true;
        }
        catch (Exception e)
        {
            Debug.Log("[ConfigurationManager->Initialize] " + e.ToString());
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