// Copyright © 2018-2021 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace GSFC.ARVR.MRET.Common
{
    public class FileInfo
    {
        public class FileFilter
        {
            public string key;
            public string value;

            public FileFilter(string _key, string _value)
            {
                key = _key;
                value = _value;
            }
        }

        public string name, description, timestamp, path;
        public Texture2D thumbnail;
        public UnityEngine.Events.UnityEvent eventToCall;
        public List<FileFilter> filters;
        public bool isDirectory;

        private static readonly string[] thumbnailExtensions = new string[] { ".png" };

        public FileInfo(string n, string d, string ts, Texture2D th,
            UnityEngine.Events.UnityEvent ev, FileFilter[] filt = null, string pth = null)
        {
            name = n;
            description = d;
            timestamp = ts;
            thumbnail = th;
            eventToCall = ev;
            path = pth;

            filters = new List<FileFilter>();
            if (filt != null)
            {
                foreach (FileFilter f in filt)
                {
                    filters.Add(f);
                }
            }
        }

        public FileInfo(string n, string ts, Texture2D th,
            UnityEngine.Events.UnityEvent ev, string pth = null)
        {
            name = n;
            timestamp = ts;
            thumbnail = th;
            eventToCall = ev;
            path = pth;

            if (!th && !string.IsNullOrEmpty(path))
            {
                FindThumbnail(path);
                FindMetaData(path);
            }
        }

        public static FileInfo CreateDirectoryInfo(string n, string ts, Texture2D th,
            UnityEngine.Events.UnityEvent ev, string pth)
        {
            return new FileInfo(n, ts, th, ev, pth)
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

            return filters.Contains(new FileFilter(key, value));
        }

        public void FindMetaData(string filePath)
        {
            description = SchemaHandler.GetDescriptionField(filePath);

            filters = new List<FileFilter>();
            FileFilter[] foundFilters = SchemaHandler.GetFilters(filePath);
            if (foundFilters != null)
            {
                foreach (FileFilter filter in foundFilters)
                {
                    filters.Add(filter);
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