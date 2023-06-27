// Copyright © 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using System.IO;
using GOV.NASA.GSFC.XR.MRET.Schema;

namespace GOV.NASA.GSFC.XR.MRET.UI.Tools.FileBrowser
{
    public class FileInfo
    {
        public string name, description, timestamp, path;
        public Texture2D thumbnail;
        public UnityEngine.Events.UnityEvent eventToCall;
        public List<SchemaFilter> filters;
        public bool isDirectory;
        public bool findMetadata = true;

        private static readonly string[] thumbnailExtensions = new string[] { ".png" };

        public FileInfo(string n, string d, string ts, bool findMetadata, Texture2D th,
            UnityEngine.Events.UnityEvent ev, SchemaFilter[] filt = null, string pth = null)
        {
            name = n;
            description = d;
            timestamp = ts;
            this.findMetadata = findMetadata;
            thumbnail = th;
            eventToCall = ev;
            path = pth;

            filters = new List<SchemaFilter>();
            if (filt != null)
            {
                foreach (SchemaFilter f in filt)
                {
                    filters.Add(f);
                }
            }
        }

        public FileInfo(string n, string ts, bool findMetadata, Texture2D th,
            UnityEngine.Events.UnityEvent ev, string pth = null)
        {
            name = n;
            timestamp = ts;
            this.findMetadata = findMetadata;
            thumbnail = th;
            eventToCall = ev;
            path = pth;

            if (!th && !string.IsNullOrEmpty(path))
            {
                FindThumbnail(path);
                FindMetaData(path);
            }
        }

        public static FileInfo CreateDirectoryInfo(string n, string ts,
            bool findMetadata, Texture2D th,
            UnityEngine.Events.UnityEvent ev, string pth)
        {
            return new FileInfo(n, ts, findMetadata, th, ev, pth)
            {
                isDirectory = true
            };
        }

        public bool CheckFilter(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                return true;
            }

            return filters.Contains(new SchemaFilter(key, value));
        }

        public void FindMetaData(string filePath)
        {
            // Set the defaults
            description = "";
            filters = new List<SchemaFilter>();

            // Read the metadata
            if (findMetadata)
            {
                description = SchemaHandler.GetDescriptionField(filePath);
                SchemaFilter[] foundFilters = SchemaHandler.GetFilters(filePath);
                if (foundFilters != null)
                {
                    foreach (SchemaFilter filter in foundFilters)
                    {
                        filters.Add(filter);
                    }
                }
            }
        }

        private void FindThumbnail(string filePath)
        {
            foreach (string ext in thumbnailExtensions)
            {
                string attemptedPath = Path.ChangeExtension(filePath, ext);
                if (File.Exists(attemptedPath))
                {
                    Texture2D tex = LoadThumbnail(attemptedPath);
                    if (tex)
                    {
                        thumbnail = tex;
                        return;
                    }
                }
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
}