﻿using AutSoft.UnityResourceGenerator.Editor.Extensions;
using AutSoft.UnityResourceGenerator.Editor.Generation;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AutSoft.UnityResourceGenerator.Editor
{
    public sealed class ResourceGeneratorSettings : ScriptableObject
    {
        [Serializable]
        public sealed class ResourceData : IResourceData
        {
            [SerializeField] private string _className = default;
            [SerializeField] private string[] _fileExtensions = default;
            [SerializeField] private bool _isResource = default;
            [SerializeField] private string _dataType = default;

            public ResourceData()
            {
            }

            public ResourceData(string className, string[] fileExtensions, bool isResource, string dataType)
            {
                _className = className;
                _fileExtensions = fileExtensions;
                _isResource = isResource;
                _dataType = dataType;
            }

            public string ClassName => _className;
            public IReadOnlyList<string> FileExtensions => _fileExtensions;
            public bool IsResource => _isResource;
            public string DataType => _dataType;
        }

        private const string SettingsPath = "Assets/ResourceGenerator.asset";

        [SerializeField] private string _baseNamespace;
        [SerializeField] private string _className;

        [SerializeField]
        [Tooltip("Relative path from the Assets folder")]
        private string _folderPath;

        [SerializeField] private bool _logInfo;
        [SerializeField] private bool _logError;
        [SerializeField] private List<string> _usings;
        [SerializeField] private List<ResourceData> _data;

        public string FolderPath => _folderPath;
        public string BaseNamespace => _baseNamespace;
        public string ClassName => _className;
        public bool LogInfo => _logInfo;
        public bool LogError => _logError;
        public IReadOnlyList<string> Usings => _usings;
        public IReadOnlyList<ResourceData> Data => _data;

        public static ResourceGeneratorSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<ResourceGeneratorSettings>(SettingsPath);
            if (settings != null) return settings;

            settings = CreateInstance<ResourceGeneratorSettings>();

            settings._folderPath = string.Empty;
            settings._baseNamespace = string.Empty;
            settings._className = "ResourcePaths";
            settings._logInfo = false;
            settings._logError = true;

            var (data, usings) = CreateDefaultFileMappings();

            settings._data = data;
            settings._usings = usings;

            AssetDatabase.CreateAsset(settings, SettingsPath);
            AssetDatabase.SaveAssets();

            return settings;
        }

        private static (List<ResourceData> data, List<string> usings) CreateDefaultFileMappings() =>
            // https://docs.unity3d.com/Manual/BuiltInImporters.html
            (
                new List<ResourceData>
                {
                    new ResourceData("Scenes", new[] { "*.unity" }, false, "Scene"),
                    new ResourceData("Prefabs", new[] { "*.prefab" }, true, "GameObject"),
                    new ResourceData("Materials", new[] { "*.mat" }, true, "Material"),
                    new ResourceData("AudioClips", new[] { "*.ogg", "*.aif", "*.aiff", "*.flac", "*.mp3", "*.mod", "*.it", "*.s3m", "*.xm", "*.wav" }, true, "AudioClip"),
                    new ResourceData("Sprites", new[] { "*.jpg", "*.jpeg", "*.tif", "*.tiff", "*.tga", "*.gif", "*.png", "*.psd", "*.bmp", "*.iff", "*.pict", "*.pic", "*.pct", "*.exr", "*.hdr" }, true, "Sprite"),
                    new ResourceData("TextAssets", new[] { "*.txt", "*.html", "*.htm", "*.xml", "*.bytes", "*.json", "*.csv", "*.yaml", "*.fnt" }, true, "TextAsset"),
                    new ResourceData("Fonts", new[] { "*.ttf", "*.dfont", "*.otf", "*.ttc" }, true, "Font")
                },
                new List<string>
                {
                    "UnityEngine",
                    "UnityEngine.SceneManagement",
                }
            );

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider() =>
            new SettingsProvider("Project/ResourceGenerator", SettingsScope.Project)
            {
                label = "ResourceGenerator",
                guiHandler = searchContext =>
                {
                    var settings = new SerializedObject(GetOrCreateSettings());

                    EditorGUILayout.PropertyField(settings.FindProperty(nameof(_folderPath)), new GUIContent("Folder from Assets"));
                    EditorGUILayout.PropertyField(settings.FindProperty(nameof(_baseNamespace)), new GUIContent("Namespace"));
                    EditorGUILayout.PropertyField(settings.FindProperty(nameof(_className)), new GUIContent("Class name"));
                    EditorGUILayout.PropertyField(settings.FindProperty(nameof(_logInfo)), new GUIContent("Log Infos"));
                    EditorGUILayout.PropertyField(settings.FindProperty(nameof(_logError)), new GUIContent("Log Errors"));

                    if (GUILayout.Button("Reset file mappings"))
                    {
                        var (data, usings) = CreateDefaultFileMappings();
                        settings.FindProperty(nameof(_data)).SetValue(data);
                        settings.FindProperty(nameof(_usings)).SetValue(usings);
                    }

                    EditorGUILayout.PropertyField(settings.FindProperty(nameof(_usings)), new GUIContent("Using directives"));
                    EditorGUILayout.PropertyField(settings.FindProperty(nameof(_data)), new GUIContent("Data"));

                    settings.ApplyModifiedProperties();
                },
                keywords = new HashSet<string>(new[] { "Resource", "Path" }),
            };
    }
}