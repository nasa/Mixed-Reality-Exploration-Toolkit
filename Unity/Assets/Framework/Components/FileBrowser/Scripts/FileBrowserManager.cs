using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GSFC.ARVR.MRET.Common
{
    public class FileBrowserManager : MonoBehaviour
    {
        [Tooltip("Name to apply to the filter scroll menu.")]
        public string filterListTitle = "Filters";

        [Tooltip("Name to apply to the file scroll menu.")]
        public string fileListTitle = "Files";

        [Tooltip("Name to apply to the filter tile menu.")]
        public string fileTilesTitle = "Files";

        [Tooltip("File extensions to search for. Any if empty.")]
        public List<string> searchFileExtensions;

        [Tooltip("File extension to use for writing files.")]
        public string usedFileExtension = ".mret";

        [Tooltip("Whether or not to show files (in addition to directories).")]
        public bool showFiles = true;

        public ScrollListManager filterListDisplay, fileListDisplay;
        public TileListManager fileTileDisplay;
        public Texture2D defaultThumbnail;
        public Texture2D fileThumbnail;
        public GameObject listViewIcon, gridViewIcon;
        public InputField fileInputText;
        public InputField fileNameText;

        public UnityEvent selectFileEvent;

        private int currentSelection = -1, currentFilterSelection = -1;
        private List<string[]> availableFilters = new List<string[]>();
        private string[] fileFilter = new string[] { "", "" };
        private bool gridViewEnabled = false;
        private List<FileInfo> currentDirectoryFiles = new List<FileInfo>();
        private Queue<FileInfo> fileButtonsQueue = new Queue<FileInfo>();
        private string currentDirectory;

        public void ToggleListView()
        {
            if (gridViewEnabled)
            {
                listViewIcon.SetActive(false);
                gridViewIcon.SetActive(true);
                SwitchToListView();
            }
            else
            {
                listViewIcon.SetActive(true);
                gridViewIcon.SetActive(false);
                SwitchToGridView();
            }
        }

        public void AddToAvailableFilters(string newKey, string newValue)
        {
            if (string.IsNullOrEmpty(newKey) || string.IsNullOrEmpty(newValue))
            {
                Debug.LogWarning("[FileBrowserManager] Invalid filter.");
                return;
            }

            if (availableFilters.Exists(x => x[0] == newKey && x[1] == newValue))
            {
                Debug.LogWarning("[FileBrowserManager] Unable to add filter. Already exists.");
            }
            else
            {
                availableFilters.Add(new string[] { newKey, newValue });
            }
        }

        public void RemoveFromAvailableFilters(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                Debug.LogWarning("[FileBrowserManager] Invalid filter.");
                return;
            }

            if (!availableFilters.Exists(x => x[0] == key && x[1] == value))
            {
                Debug.LogWarning("[FileBrowserManager] Unable to remove filter. Not found.");
            }
            else
            {
                availableFilters.Remove(new string[] { key, value });
            }
        }

        public void ClearAvailableFilters()
        {
            availableFilters = new List<string[]>();
        }

        public void OpenDirectory(string directoryPath)
        {
            SelectDirectory(directoryPath);
            PopulateFilterScrollList();
            PopulateFileLists();
        }

        public void SelectDirectory(string directoryPath)
        {
            if (System.IO.Directory.Exists(directoryPath))
            {
                int buttonNum = 0;
                currentDirectoryFiles = new List<FileInfo>();
                string[] files = System.IO.Directory.GetFiles(directoryPath);

                int i = 0, skipped = 0;
                if (showFiles)
                {
                    for (i = 0; i < files.Length; i++)
                    {
                        if (searchFileExtensions.Count > 0)
                        {
                            if (!System.IO.Path.HasExtension(files[i]))
                            {
                                // No extension, skip file.
                                skipped++;
                                continue;
                            }

                            if (!searchFileExtensions.Contains(System.IO.Path.GetExtension(files[i])))
                            {
                                // Extension doesn't match, skip file.
                                skipped++;
                                continue;
                            }
                        }

                        int indexToSelect = i - skipped;
                        int btnToSelect = buttonNum++;
                        UnityEvent clickEvent = new UnityEvent();
                        clickEvent.AddListener(new UnityAction(() =>
                        {
                            SetActiveSelectionListID(indexToSelect);
                            SetActiveSelectionButtonID(btnToSelect);
                        }));

                        string fileName = System.IO.Path.GetFileNameWithoutExtension(files[i]);
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            FileInfo newInfo = new FileInfo(
                                fileName,
                                System.IO.File.GetLastWriteTime(files[i]).ToString(), null,
                                clickEvent,
                                files[i]);

                            if (newInfo.thumbnail == null)
                            {
                                newInfo.thumbnail = defaultThumbnail;
                            }

                            currentDirectoryFiles.Add(newInfo);
                        }
                        else
                        {
                            skipped++;
                        }
                    }
                }

                foreach (string dir in System.IO.Directory.GetDirectories(directoryPath))
                {
                    int indexToSelect = i++ - skipped;
                    int btnToSelect = buttonNum++;
                    UnityEvent clickEvent = new UnityEvent();
                    clickEvent.AddListener(new UnityAction(() => {
                        SetActiveSelectionListID(indexToSelect);
                        SetActiveSelectionButtonID(btnToSelect);
                    }));
                    currentDirectoryFiles.Add(FileInfo.CreateDirectoryInfo(System.IO.Path.GetFileName(dir),
                        System.IO.Directory.GetLastWriteTime(dir).ToString(), fileThumbnail,
                        clickEvent, dir));
                }

                currentDirectory = fileInputText.text = directoryPath;
                currentSelection = -1;
            }
            else
            {
                Debug.LogWarning("[FileBrowserManager] Directory not found.");
            }
        }

        public void OpenParentDirectory()
        {
            System.IO.DirectoryInfo parent = System.IO.Directory.GetParent(currentDirectory);

            if (parent != null)
            {
                OpenDirectory(parent.FullName);
            }
        }

        public void Open()
        {
            if (currentSelection > -1 && currentSelection < currentDirectoryFiles.Count)
            {
                if (currentDirectoryFiles[currentSelection] != null)
                {
                    if (currentDirectoryFiles[currentSelection].isDirectory)
                    {
                        SelectDirectory(currentDirectoryFiles[currentSelection].path);
                        PopulateFilterScrollList();
                        PopulateFileLists();
                    }
                    else
                    {
                        if (selectFileEvent != null)
                        {
                            selectFileEvent.Invoke();
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("[FileBrowserManager] Attempting to open invalid selection.");
                }
            }
        }

        public object GetSelectedFile()
        {
            if (currentSelection > -1)
            {
                return SchemaHandler.ReadXML(currentDirectoryFiles[currentSelection].path);
            }
            return null;
        }

        public string GetSelectedFilePath()
        {
            if (currentSelection > -1)
            {
                return currentDirectoryFiles[currentSelection].path;
            }
            return null;
        }

        public string GetSaveFilePath()
        {
            if (!string.IsNullOrEmpty(fileNameText.text) && System.IO.Directory.Exists(currentDirectory))
            {
                return System.IO.Path.Combine(currentDirectory, fileNameText.text);
            }

            return null;
        }

        public void Save()
        {
            if (currentSelection > -1)
            {
                // Something is selected. Attempt to open directory.
                if (currentDirectoryFiles[currentSelection] != null)
                {
                    // Selection not null.
                    if (currentDirectoryFiles[currentSelection].isDirectory)
                    {
                        // Selection is directory. Open directory.
                        OpenDirectory(currentDirectoryFiles[currentSelection].path);
                    }
                    else
                    {
                        Debug.LogError("[FileBrowserManager] Directory selection error. Not a directory.");
                        return;
                    }
                }
                else
                {
                    Debug.LogError("[FileBrowserManager] Directory selection error. Null.");
                    return;
                }
            }
            else if (currentSelection == -1)
            {
                // Nothing selected. Try to save file.
                if (string.IsNullOrEmpty(fileNameText.text))
                {
                    Debug.LogWarning("[FileBrowserManager] Invalid file name.");
                    return;
                }

                if (selectFileEvent != null)
                {
                    selectFileEvent.Invoke();
                }
            }
            
            return;
        }

        private void SwitchToListView()
        {
            gridViewEnabled = false;
            fileListDisplay.gameObject.SetActive(true);
            fileTileDisplay.gameObject.SetActive(false);
        }

        private void SwitchToGridView()
        {
            gridViewEnabled = true;
            fileListDisplay.gameObject.SetActive(false);
            fileTileDisplay.gameObject.SetActive(true);
        }

        private void SetActiveSelectionListID(int listID)
        {
            currentSelection = listID;
        }

        private void SetActiveSelectionButtonID(int buttonID)
        {
            fileListDisplay.HighlightItem(buttonID);
            fileTileDisplay.HighlightItem(buttonID);
        }

        public void SetFilter(int listID)
        {
            if (listID == 0)
            {
                fileFilter = new string[] { "", "" };
            }
            else
            {
                currentFilterSelection = listID - 1;
                if (availableFilters[currentFilterSelection] != null)
                {
                    fileFilter = availableFilters[currentFilterSelection];
                }
            }

            filterListDisplay.HighlightItem(listID);
            PopulateFileLists();
        }

        public void PopulateFilterScrollList()
        {
            filterListDisplay.ClearScrollList();

            UnityEvent firstClickEvent = new UnityEvent();
            firstClickEvent.AddListener(new UnityAction(() => { SetFilter(0); }));
            filterListDisplay.AddScrollListItem("None", firstClickEvent);

            int i = 1;
            foreach (string[] filter in availableFilters)
            {
                int indexToSelect = i;
                i++;
                UnityEvent clickEvent = new UnityEvent();
                clickEvent.AddListener(new UnityAction(() => { SetFilter(indexToSelect); }));
                filterListDisplay.AddScrollListItem(filter[0] + " | " + filter[1], clickEvent);
            }
        }

        public void PopulateFileLists()
        {
            fileButtonsQueue.Clear();
            fileListDisplay.ClearScrollList();
            fileTileDisplay.ClearTileList();

            int buttonNum = 0;
            for (int i = 0; i < currentDirectoryFiles.Count; i++)
            {
                if (currentDirectoryFiles[i].CheckFilter(fileFilter[0], fileFilter[1]))
                {
                    int indexToSelect = i;
                    int btnToSelect = buttonNum++;
                    fileButtonsQueue.Enqueue(currentDirectoryFiles[i]);
                }
            }
        }

        private void Start()
        {
            if (currentDirectory == null)
            {
                currentDirectory = fileInputText.text = Application.dataPath;
                OpenDirectory(currentDirectory);
            }
            filterListDisplay.SetTitle(filterListTitle);
            fileListDisplay.SetTitle(fileListTitle);
            fileTileDisplay.SetTitle(fileTilesTitle);
        }

        private void Update()
        {
            ServiceFileButtons();
        }

        private void OnEnable()
        {
            if (!string.IsNullOrEmpty(currentDirectory))
            {
                OpenDirectory(currentDirectory);
            }
        }

        private string HandleFileExtension(string inputPath)
        {
            return System.IO.Path.ChangeExtension(inputPath, usedFileExtension);
        }

        private int fileButtonCounter = 0;
        private void ServiceFileButtons()
        {
            if (fileButtonCounter++ > 5)
            {
                fileButtonCounter = 0;
            }
            else
            {
                return;
            }
            if (fileButtonsQueue.Count > 0)
            {
                FileInfo info = fileButtonsQueue.Dequeue();
                fileListDisplay.AddScrollListItem(info.name + "\n" + info.description + "\n"
                    + info.timestamp, info.thumbnail, info.eventToCall);
                fileTileDisplay.AddTileListItem(info.name, info.thumbnail, info.eventToCall);
            }
        }
    }
}