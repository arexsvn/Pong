using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class DefinesPanel : EditorWindow
{
    private List<DefinesScriptableObject> _definesScriptableObjects;
    private DefinesScriptableObject _rspOnlyDefines;
    private Dictionary<string, bool> _definesState;
    private List<string> _rspNonDefinesEntries;
    private List<RSPInfo> _rsps;
    private string _defaultRSPFilePath;
    private const string DEFAULT_RSP_FILENAME = "csc.rsp";
    private int _newDefinesCount = 0;
    [SerializeField] private int _selectedRSPIndex;

    [MenuItem("Window/Defines Panel")]
    public static void ShowWindow()
    {
        DefinesPanel window = GetWindow<DefinesPanel>("Defines Panel");
        window.minSize = new Vector2(600, 400);
        window.Refresh();
    }

    public void Refresh()
    {
        if (!IsInited)
        {
            Init();
        }

        LoadDefines();
        CheckForDefaultRSP();
        LoadSelectedRSP();
    }

    private void OnEnable()
    {
        if (!IsInited)
        {
            Refresh();
        }
    }

    private void Init()
    {
        _definesScriptableObjects = new List<DefinesScriptableObject>();
        _definesState = new Dictionary<string, bool>();
        _defaultRSPFilePath = Path.Combine(Application.dataPath, DEFAULT_RSP_FILENAME);
        _rsps = new List<RSPInfo>();
        _rspNonDefinesEntries = new List<string>();
        _rspOnlyDefines = ScriptableObject.CreateInstance<DefinesScriptableObject>();
        _rspOnlyDefines.Defines = new List<string>();
        _rspOnlyDefines.name = "RSP Only";

        ScanForRSPs();
    }

    private void LoadDefines()
    {
        _definesScriptableObjects.Clear();
        _definesState.Clear();

        string[] defineSoGuids = AssetDatabase.FindAssets($"t:{nameof(DefinesScriptableObject)}");
        foreach (string defineSoGuid in defineSoGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(defineSoGuid);
            DefinesScriptableObject dso = AssetDatabase.LoadAssetAtPath<DefinesScriptableObject>(path);
            if (dso != null)
            {
                _definesScriptableObjects.Add(dso);

                foreach (string defineName in dso.Defines)
                {
                    _definesState[defineName] = false;
                }
            }
        }
    }

    private void CheckForDefaultRSP()
    {
        if (!File.Exists(_defaultRSPFilePath))
        {
            FileStream fs = File.Create(_defaultRSPFilePath);
            fs.Close();
        }
    }

    private void LoadSelectedRSP()
    {
        ReadDefinesInRSP(GetSelectedRSP().path);
    }

    private void ReadDefinesInRSP(string rspPath)
    {
        _rspOnlyDefines.Defines.Clear();

        string rspFileText = File.ReadAllText(rspPath);
        List<string> storedDefines = GetDefines(rspFileText);

        foreach (string define in storedDefines)
        {
            if (!_definesState.ContainsKey(define))
            {
                _rspOnlyDefines.Defines.Add(define);
            }
            _definesState[define] = true;
        }
    }

    private void ScanForRSPs()
    {
        _rsps.Clear();
        _rsps.Add(new RSPInfo(DEFAULT_RSP_FILENAME, _defaultRSPFilePath));

        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        string[] rspPaths = DirectorySearch(projectRoot, "*.rsp");

        foreach (string rspPath in rspPaths)
        {
            if (rspPath == _defaultRSPFilePath)
            {
                continue;
            }
            _rsps.Add(new RSPInfo(Path.GetFileName(rspPath), rspPath));
        }

        if (_selectedRSPIndex >= _rsps.Count)
        {
            // reset to default .rsp if the selected index exceeds total .rsps.
            _selectedRSPIndex = 0;
        }
    }

    private string[] DirectorySearch(string path, string pattern)
    {
        string[] paths = Directory.GetFiles(path, pattern);

        foreach (string directory in Directory.GetDirectories(path))
        {
            if (directory.Contains("Library") || directory.Contains(".") || directory.Contains("Temp") || directory.Contains("obj") || directory.Contains("Plugins"))
            {
                continue;
            }

            paths = paths.Concat(DirectorySearch(directory, pattern)).ToArray();
        }

        return paths;
    }

    private void DisplayRSPSelection()
    {
        List<string> rspNames = _rsps.ConvertAll(x => x.path).ToList();
        string[] rspNamesDisplay = new string[rspNames.Count];

        for (int n = 0; n < rspNames.Count; n++)
        {
            string[] parts = rspNames[n].Split("/");
            rspNamesDisplay[n] = parts[parts.Length - 2] + "/" + parts[parts.Length - 1];
        }

        int newSelectedRSPIndex = EditorGUILayout.Popup("Select .rsp", _selectedRSPIndex, rspNamesDisplay);

        if (newSelectedRSPIndex != _selectedRSPIndex)
        {
            _selectedRSPIndex = newSelectedRSPIndex;
            Refresh();
        }
    }

    private void DisplayDefines()
    {
        foreach (DefinesScriptableObject dso in _definesScriptableObjects.ToList())
        {
            DisplayDefine(dso);
        }

        if (_rspOnlyDefines.Defines.Count > 0)
        {
            DisplayDefine(_rspOnlyDefines, true);
        }
    }

    private void DisplayDefine(DefinesScriptableObject dso, bool rspOnly = false)
    {
        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(dso.name);

        GUILayout.Space(5);

        if (!rspOnly)
        {
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_scenevis_visible_hover@2x"), GUILayout.Width(28)))
            {
                Selection.activeObject = dso;
            }

            GUILayout.Space(5);

            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                string newName = GetNewDefinesObjectName();
                dso.Defines.Add(newName);
                _definesState[newName] = false;

                EditorGUILayout.EndHorizontal();

                SaveChanges(dso);
                return;
            }
        }

        EditorGUILayout.EndHorizontal();

        foreach (string defineName in dso.Defines.ToList())
        {
            if (!_definesState.ContainsKey(defineName))
            {
                Refresh();
                return;
            }

            EditorGUILayout.BeginHorizontal();
            _definesState[defineName] = EditorGUILayout.Toggle(_definesState[defineName], GUILayout.Width(20));

            if (!rspOnly)
            {
                string newDefineName = EditorGUILayout.TextField(defineName);

                if (newDefineName != defineName)
                {
                    dso.Defines[dso.Defines.IndexOf(defineName)] = newDefineName;
                    _definesState[newDefineName] = _definesState[defineName];
                    _definesState.Remove(defineName);

                    EditorGUILayout.EndHorizontal();

                    SaveChanges(dso);
                    return;
                }

                GUILayout.Space(5);

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    dso.Defines.Remove(defineName);

                    EditorGUILayout.EndHorizontal();

                    SaveChanges(dso);
                    return;
                }
            }
            else
            {
                EditorGUILayout.LabelField(defineName);
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private void SaveChanges(DefinesScriptableObject dso)
    {
        EditorUtility.SetDirty(dso);
        AssetDatabase.SaveAssets();
    }

    private string GetNewDefinesObjectName()
    {
        string newName = $"NEW_DEFINE_{_newDefinesCount}";

        while (_definesState.ContainsKey(newName))
        {
            _newDefinesCount++;
            newName = $"NEW_DEFINE_{_newDefinesCount}";
        }

        return newName;
    }

    private void OnGUI()
    {
        if (!IsInited)
        {
            return;
        }

        if (_rsps.Count > 1)
        {
            DisplayRSPSelection();
        }

        DisplayDefines();

        GUILayout.Space(20);

        if (GUILayout.Button("Create New Defines Object"))
        {
            ShowCreateDefinesObjectDialog();
        }

        if (GUILayout.Button("Create New .rsp File"))
        {
            ShowCreateRSPFileDialog();
        }

        if (GUILayout.Button("Force Refresh"))
        {
            ScanForRSPs();
            Refresh();
        }

        GUILayout.Space(10);

        if (_definesScriptableObjects.Count > 0)
        {
            string label = "Save Enabled Defines";
            if (_rsps.Count > 1)
            {
                label = $"Save Enabled Defines to {GetSelectedRSP().name}";
            }

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            ColorUtility.TryParseHtmlString("#70b3fa", out Color color);
            buttonStyle.normal.textColor = color;

            if (GUILayout.Button(label, buttonStyle))
            {
                StoreDefines();
            }
        }
    }

    private List<string> GetDefines(string rspFileContents)
    {
        _rspNonDefinesEntries.Clear();

        if (string.IsNullOrEmpty(rspFileContents))
        {
            return new List<string>();
        }

        rspFileContents = rspFileContents.TrimEnd('\r', '\n');  // Remove newlines from end of string
        rspFileContents = rspFileContents.Replace("\n", ";");   // Replace newline after each define with a semicolon
        List<string> entries = rspFileContents.Split(';').ToList();
        foreach (string entry in entries.ToList())
        {
            if (!entry.Contains("-define:"))
            {
                // .rsp files can have other entries that are not defines. Store them so they can be written out to the .rsp when saving defines.
                _rspNonDefinesEntries.Add(entry);
                entries.Remove(entry);
            }
            else if (string.IsNullOrEmpty(entry))
            {
                entries.Remove(entry);
            }
        }

        rspFileContents = string.Join(';', entries);
        rspFileContents = rspFileContents.Replace("-define:", "");  // Remove "-define" string
        entries = rspFileContents.Split(';').ToList();
        return entries;
    }

    private void StoreDefines()
    {
        string defineString = "";

        foreach (string rspNonDefinesEntry in _rspNonDefinesEntries)
        {
            defineString += $"{rspNonDefinesEntry}\n";
        }

        foreach (KeyValuePair<string, bool> kvp in _definesState)
        {
            if (kvp.Value)
            {
                defineString += $"-define:{kvp.Key}\n";
            }
        }

        File.WriteAllText(GetSelectedRSP().path, defineString);

        AssetDatabase.Refresh();
    }

    private RSPInfo GetSelectedRSP()
    {
        return _rsps[_selectedRSPIndex];
    }

    private void ShowCreateDefinesObjectDialog()
    {
        string path = EditorUtility.SaveFilePanelInProject("Create New Defines Object", "NewDefines.asset", "asset", "Select location to create Scriptable Object");
        if (!string.IsNullOrEmpty(path))
        {
            CreateNewScriptableObject(path);
        }
    }

    private void ShowCreateRSPFileDialog()
    {
        string path = EditorUtility.SaveFilePanel("Create New .rsp File", "", "NewRSP.rsp", "rsp");
        if (!string.IsNullOrEmpty(path))
        {
            CreateNewRSPFile(path);
            ScanForRSPs();
            SelectRSP(Path.GetFullPath(path));
        }
    }

    private void SelectRSP(string path)
    {
        for (int n = 0; n < _rsps.Count; n++)
        {
            if (_rsps[n].path == path)
            {
                _selectedRSPIndex = n;
                Refresh();
                return;
            }
        }
    }

    private void CreateNewScriptableObject(string path)
    {
        string extension = ".asset";
        string fullFileName = Path.GetFileName(path);
        string fileName = fullFileName.Substring(0, fullFileName.Length - extension.Length);

        DefinesScriptableObject definesScriptableObject = ScriptableObject.CreateInstance<DefinesScriptableObject>();
        definesScriptableObject.Defines = new List<string>();

        AssetDatabase.CreateAsset(definesScriptableObject, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Refresh();
    }

    private void CreateNewRSPFile(string path)
    {
        FileStream fs = File.Create(path);
        fs.Close();
        AssetDatabase.Refresh();
    }

    private bool IsInited
    {
        get
        {
            return _definesScriptableObjects != null && _definesState != null;
        }
    }
}

public class RSPInfo
{
    public string name;
    public string path;

    public RSPInfo(string name, string path)
    {
        this.name = name;
        this.path = path;
    }
}