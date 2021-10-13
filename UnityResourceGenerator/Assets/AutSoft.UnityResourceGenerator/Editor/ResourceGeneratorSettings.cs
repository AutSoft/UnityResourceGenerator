﻿using AutSoft.UnityResourceGenerator.Editor.Generation;
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

            public ResourceData()
            {
            }

            public ResourceData(string className, string[] fileExtensions, bool isResource)
            {
                _className = className;
                _fileExtensions = fileExtensions;
                _isResource = isResource;
            }

            public string ClassName => _className;
            public IReadOnlyList<string> FileExtensions => _fileExtensions;
            public bool IsResource => _isResource;
        }

        private const string SettingsPath = "Assets/ResourceGenerator.asset";

        [SerializeField] private string _baseNamespace;
        [SerializeField] private string _className;

        [SerializeField]
        [Tooltip("Relative path from the Assets folder")]
        private string _folderPath;

        [SerializeField] private bool _logInfo;
        [SerializeField] private bool _logError;
        [SerializeField] private List<ResourceData> _data;

        public string FolderPath => _folderPath;
        public string BaseNamespace => _baseNamespace;
        public string ClassName => _className;
        public bool LogInfo => _logInfo;
        public bool LogError => _logError;
        public IReadOnlyList<ResourceData> Data => _data;

        public static ResourceGeneratorSettings GetOrCreateSettings
        (
            string folderPath = null,
            string baseNamespace = null,
            string className = null,
            bool? logInfo = null,
            bool? logError = null
        )
        {
            var settings = AssetDatabase.LoadAssetAtPath<ResourceGeneratorSettings>(SettingsPath);
            if (settings != null) return settings;

            settings = CreateInstance<ResourceGeneratorSettings>();

            settings._folderPath = folderPath ?? string.Empty;
            settings._baseNamespace = baseNamespace ?? "Resources";
            settings._className = className ?? "ResourcePaths";
            settings._logInfo = logInfo ?? false;
            settings._logError = logError ?? true;

            // https://docs.unity3d.com/Manual/BuiltInImporters.html
            settings._data = new List<ResourceData>
            {
                new ResourceData("Scenes", new[]{"*.unity"}, false),
                new ResourceData("Prefabs", new[]{"*.prefab"}, true),
                new ResourceData("Materials", new[]{"*.mat"}, true),
                new ResourceData("AudioClips", new[]{"*.ogg", "*.aif", "*.aiff", "*.flac", "*.mp3", "*.mod", "*.it", "*.s3m", "*.xm"}, true),
                new ResourceData("Sprites", new[]{"*.jpg", "*.jpeg", "*.tif", "*.tiff", "*.tga", "*.gif", "*.png", "*.psd", "*.bmp", "*.iff", "*.pict", "*.pic", "*.pct", "*.exr", "*.hdr"}, true),
                new ResourceData("TextAssets", new[]{"*.txt", "*.html", "*.htm", "*.xml", "*.bytes", "*.json", "*.csv", "*.yaml", "*.fnt"}, true),
                new ResourceData("Fonts", new[]{"*.ttf", "*.dfont", "*.otf", "*.ttc"}, true)
            };

            AssetDatabase.CreateAsset(settings, SettingsPath);
            AssetDatabase.SaveAssets();

            return settings;
        }

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
                    EditorGUILayout.PropertyField(settings.FindProperty(nameof(_data)), new GUIContent("Data"));

                    settings.ApplyModifiedProperties();
                },
                keywords = new HashSet<string>(new[] { "Resource", "Path" }),
            };
    }
}