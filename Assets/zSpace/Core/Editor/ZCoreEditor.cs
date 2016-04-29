//////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2016 zSpace, Inc.  All Rights Reserved.
//
//////////////////////////////////////////////////////////////////////////

using System;

using UnityEditor;
using UnityEngine;


namespace zSpace.Core
{
    [CustomEditor(typeof(ZCore))]
    public class ZCoreEditor : Editor 
    {
        //////////////////////////////////////////////////////////////////
        // Serialized Properties
        //////////////////////////////////////////////////////////////////

        SerializedProperty ShowLabelsProperty;
        SerializedProperty ShowViewportProperty;
        SerializedProperty ShowCCZoneProperty;
        SerializedProperty ShowUCZoneProperty;
        SerializedProperty ShowDisplayProperty;
        SerializedProperty ShowRealWorldUpProperty;
        SerializedProperty ShowGroundPlaneProperty;
        SerializedProperty ShowGlassesProperty;
        SerializedProperty ShowStylusProperty;

        SerializedProperty CurrentCameraObjectProperty;
        SerializedProperty EnableStereoProperty;
        SerializedProperty EnableAutoStereoProperty;
        SerializedProperty CopyCurrentCameraAttributesProperty;
        SerializedProperty MinimizeLatencyProperty;
        SerializedProperty IpdProperty;
        SerializedProperty ViewerScaleProperty;
        SerializedProperty AutoStereoDelayProperty;
        SerializedProperty AutoStereoDurationProperty;
        
        SerializedProperty EnableMouseEmulationProperty;
        SerializedProperty EnableMouseAutoHideProperty;
        SerializedProperty MouseAutoHideDelayProperty;


        //////////////////////////////////////////////////////////////////
        // Unity Callbacks
        //////////////////////////////////////////////////////////////////

        void OnEnable()
        {
            this.LoadIconTextures();
            this.FindSerializedProperties();

            // Ensure only one callback has been registered.
            EditorApplication.update -= OnEditorApplicationUpdate;
            EditorApplication.update += OnEditorApplicationUpdate;
        }

        void OnDisable()
        {
            EditorApplication.update -= OnEditorApplicationUpdate;
        }

        public override void OnInspectorGUI()
        {
            this.InitializeGUIStyles();

            this.serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            this.CheckCoreInitialized();
            this.CheckOpenGLEnabledForStandaloneBuilds();
            this.DrawInfoSection();
            this.DrawDebugSection();
            this.DrawStereoRigSection();
            this.DrawGlassesSection();
            this.DrawStylusSection();
            this.DrawDisplaySection();

            this.serializedObject.ApplyModifiedProperties();
        }


        //////////////////////////////////////////////////////////////////
        // Section Draw Helpers
        //////////////////////////////////////////////////////////////////

        private void CheckCoreInitialized()
        {
            ZCore core = (ZCore)this.target;

            if (!core.IsInitialized())
            {
                EditorGUILayout.HelpBox(
                    "Failed to properly initialize the zSpace Core SDK. As a result, most zSpace " +
                        "Core functionality will be disabled. Please make sure that the zSpace System " +
                        "Software has been properly installed on your machine.",
                    MessageType.Error);
                EditorGUILayout.Space();
            }
        }

        private void CheckOpenGLEnabledForStandaloneBuilds()
        {
        #if (UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7)
            return;
        #else
            UnityEngine.Rendering.GraphicsDeviceType[] graphicsAPIs = null;
            bool isOpenGLEnabledWin32 = false;
            bool isOpenGLEnabledWin64 = false;

            // Check if OpenGL2 is enabled for Win32 standalone builds:
            graphicsAPIs = PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows);
            foreach (var g in graphicsAPIs)
            {
                if (g == UnityEngine.Rendering.GraphicsDeviceType.OpenGL2)
                {
                    isOpenGLEnabledWin32 = true;
                }
            }

            // Check if OpenGL2 is enabled for Win64 standalone builds:
            graphicsAPIs = PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows64);
            foreach (var g in graphicsAPIs)
            {
                if (g == UnityEngine.Rendering.GraphicsDeviceType.OpenGL2)
                {
                    isOpenGLEnabledWin64 = true;
                }
            }

            // If OpenGL2 is not enabled, display a warning for the user with instructions
            // on how to enable it.
            if (!isOpenGLEnabledWin32 || !isOpenGLEnabledWin64)
            {
                string unsupportedBuildTargets = string.Empty;
                if (!isOpenGLEnabledWin32)
                {
                    unsupportedBuildTargets += "\nStandaloneWindows (x86)";
                }
                if (!isOpenGLEnabledWin64)
                {
                    unsupportedBuildTargets += "\nStandaloneWindows64 (x86_64)";
                }

                EditorGUILayout.HelpBox(
                    "zSpace has detected that OpenGL2 rendering support is currently disabled for the following " +
                        "standalone player build targets:\n" + unsupportedBuildTargets + "\n\n" +
                        "As a result, standalone player builds will be unable to support OpenGL stereoscopic 3D rendering. " +
                        "To enable OpenGL stereoscopic 3D rendering support, go to:\n\n" +
                        "Edit > Project Settings > Player > PC, Mac & Linux Standalone > Other Settings\n\n" +
                        "Uncheck \"Auto Graphics API for Windows\" " +
                        "and manually add OpenGL2 to the list of graphics APIs for Windows.",
                    MessageType.Warning);
                EditorGUILayout.Space();
            }
        #endif
        }

        private void DrawInfoSection()
        {
            ZCore core = (ZCore)this.target;

            _isInfoSectionExpanded = this.DrawSectionHeader("General Info", _infoIconTexture, _isInfoSectionExpanded);
            if (_isInfoSectionExpanded)
            {
                string pluginVersion = core.GetPluginVersion();
                string runtimeVersion = core.IsInitialized() ? core.GetRuntimeVersion() : "Unknown";

                EditorGUILayout.LabelField("Plugin Version: " + pluginVersion);
                EditorGUILayout.LabelField("Runtime Version: " + runtimeVersion);
                EditorGUILayout.Space();
            }
        }

        private void DrawDebugSection()
        {
            _isDebugSectionExpanded = this.DrawSectionHeader("Debug", _debugIconTexture, _isDebugSectionExpanded);
            if (_isDebugSectionExpanded)
            {
                this.DrawToggleLeft("Show Labels", this.ShowLabelsProperty);
                this.DrawToggleLeft("Show Viewport (Zero Parallax)", this.ShowViewportProperty);
                this.DrawToggleLeft("Show Comfort Zone (Negative Parallax)", this.ShowCCZoneProperty);
                this.DrawToggleLeft("Show Comfort Zone (Positive Parallax)", this.ShowUCZoneProperty);
                this.DrawToggleLeft("Show Display", this.ShowDisplayProperty);
                this.DrawToggleLeft("Show Real-World Up", this.ShowRealWorldUpProperty);
                this.DrawToggleLeft("Show Ground Plane", this.ShowGroundPlaneProperty);
                this.DrawToggleLeft("Show Glasses", this.ShowGlassesProperty);
                this.DrawToggleLeft("Show Stylus", this.ShowStylusProperty);
                EditorGUILayout.Space();
            }
        }

        private void DrawStereoRigSection()
        {
            ZCore core = (ZCore)this.target;

            _isStereoRigSectionExpanded = this.DrawSectionHeader("Stereo Rig", _cameraIconTexture, _isStereoRigSectionExpanded);
            if (_isStereoRigSectionExpanded)
            {
                EditorGUILayout.PropertyField(this.CurrentCameraObjectProperty, new GUIContent("Current Camera"));
                EditorGUILayout.Space();

                _viewportCenter = EditorGUILayout.Vector3Field(new GUIContent("Viewport World Center"), _viewportCenter);
                _viewportRotation = EditorGUILayout.Vector3Field(new GUIContent("Viewport World Rotation"), _viewportRotation);

                GUI.enabled = core.IsInitialized();
                if (GUILayout.Button(new GUIContent("Update Current Camera Transform")))
                {
                    Vector3 displayAngle = core.GetDisplayAngle();
                    Vector3 cameraOffset = core.GetFrustumCameraOffset();
                    Vector3 displayDirection = Quaternion.Euler(-displayAngle.x, 0.0f, 0.0f) * Vector3.forward;
                    float angle = Vector3.Angle(cameraOffset.normalized, displayDirection.normalized);

                    Quaternion cameraRotation = Quaternion.Euler(_viewportRotation) * Quaternion.Euler(90.0f - angle, 0.0f, 0.0f);
                    Vector3    cameraPosition = core.ComputeCameraPosition(_viewportCenter, cameraRotation);

                    if (core.CurrentCameraObject != null)
                    {
                        core.CurrentCameraObject.transform.position = cameraPosition;
                        core.CurrentCameraObject.transform.rotation = cameraRotation;
                    }
                }
                GUI.enabled = true;
                EditorGUILayout.Space();

                this.DrawToggleLeft("Enable Stereo", this.EnableStereoProperty);
                this.DrawToggleLeft("Enable Auto-Transition to Mono", this.EnableAutoStereoProperty);
                this.DrawToggleLeft("Copy Current Camera Attributes", this.CopyCurrentCameraAttributesProperty);
                this.DrawToggleLeft("Minimize Latency", this.MinimizeLatencyProperty);

                if (this.MinimizeLatencyProperty.boolValue)
                {
                    EditorGUILayout.HelpBox(
                        "Minimizing latency may cause undesirable effects with respect to maintaining " +
                            "synchronization between the head pose queryable through ZCore.GetTargetPose() " +
                            "and the head pose used to calculate the view/projection matrices for the " +
                            "ZCore stereo rig.", 
                        MessageType.Warning);
                }

                EditorGUILayout.Space();

                EditorGUILayout.Slider(this.IpdProperty, 0.0f, 0.1f, new GUIContent("IPD"));
                EditorGUILayout.Slider(this.ViewerScaleProperty, 0.001f, 500.0f, new GUIContent("Viewer Scale"));
                EditorGUILayout.Slider(this.AutoStereoDelayProperty, 0.0f, 60.0f, new GUIContent("Auto Stereo Delay"));
                EditorGUILayout.Slider(this.AutoStereoDurationProperty, 0.0f, 60.0f, new GUIContent("Auto Stereo Duration"));
                EditorGUILayout.Space();
            }
        }

        private void DrawGlassesSection()
        {
            ZCore core = (ZCore)this.target;

            _isGlassesSectionExpanded = this.DrawSectionHeader("Glasses", _glassesIconTexture, _isGlassesSectionExpanded);
            if (_isGlassesSectionExpanded)
            {
                // Display pose information (readonly).
                if (core.IsInitialized())
                {
                    this.DrawPoseInfo("Tracker-Space Pose:", core.GetTargetPose(ZCore.TargetType.Head, ZCore.CoordinateSpace.Tracker));
                    this.DrawPoseInfo("World-Space Pose:", core.GetTargetPose(ZCore.TargetType.Head, ZCore.CoordinateSpace.World));
                }
                else
                {
                    EditorGUILayout.LabelField("Tracker-Space Pose: Unknown");
                    EditorGUILayout.LabelField("World-Space Pose: Unknown");
                }
            }
        }

        private void DrawStylusSection()
        {
            ZCore core = (ZCore)this.target;

            _isStylusSectionExpanded = this.DrawSectionHeader("Stylus", _stylusIconTexture, _isStylusSectionExpanded);
            if (_isStylusSectionExpanded)
            {
                this.DrawToggleLeft("Enable Mouse Emulation", this.EnableMouseEmulationProperty);
                this.DrawToggleLeft("Enable Mouse Auto-Hide", this.EnableMouseAutoHideProperty);
                EditorGUILayout.Space();

                EditorGUILayout.Slider(this.MouseAutoHideDelayProperty, 0.0f, 60.0f, new GUIContent("Mouse Auto-Hide Delay"));
                EditorGUILayout.Space();

                // Display pose information (readonly).
                if (core.IsInitialized())
                {
                    this.DrawPoseInfo("Tracker-Space Pose:", core.GetTargetPose(ZCore.TargetType.Primary, ZCore.CoordinateSpace.Tracker));
                    this.DrawPoseInfo("World-Space Pose:", core.GetTargetPose(ZCore.TargetType.Primary, ZCore.CoordinateSpace.World));
                }
                else
                {
                    EditorGUILayout.LabelField("Tracker-Space Pose: Unknown");
                    EditorGUILayout.LabelField("World-Space Pose: Unknown");
                }
            }
        }

        private void DrawDisplaySection()
        {
            ZCore core = (ZCore)this.target;

            int numDisplays = core.IsInitialized() ? core.GetNumDisplays() : 0;
            string sectionName = (numDisplays == 1) ? "Display" : "Displays";

            _isDisplaySectionExpanded = this.DrawSectionHeader(sectionName, _displayIconTexture, _isDisplaySectionExpanded);
            if (_isDisplaySectionExpanded)
            {
                for (int i = 0; i < numDisplays; ++i)
                {
                    IntPtr displayHandle = core.GetDisplay(i);
                    string displayName = string.Format("{0}. {1}{2} ({3})",
                                            core.GetDisplayNumber(displayHandle),
                                            core.GetDisplayAttributeString(displayHandle, ZCore.DisplayAttribute.ManufacturerName),
                                            core.GetDisplayAttributeString(displayHandle, ZCore.DisplayAttribute.ProductCode),
                                            core.GetDisplayType(displayHandle));
                    
                    EditorGUILayout.LabelField(displayName);
                    EditorGUI.indentLevel++;
                    {
                        GUI.enabled = false;
                        EditorGUILayout.Vector2Field(new GUIContent("Position"), core.GetDisplayPosition(displayHandle));
                        EditorGUILayout.Vector2Field(new GUIContent("Size"), core.GetDisplaySize(displayHandle));
                        EditorGUILayout.Vector2Field(new GUIContent("Resolution"), core.GetDisplayNativeResolution(displayHandle));
                        EditorGUILayout.Vector3Field(new GUIContent("Angle"), core.GetDisplayAngle(displayHandle));
                        GUI.enabled = true;
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }
                EditorGUILayout.Space();
            }
        }


        //////////////////////////////////////////////////////////////////
        // Private Helper Methods
        //////////////////////////////////////////////////////////////////

        private void OnEditorApplicationUpdate()
        {
            // Only force the inspector to update/repaint if a section
            // with dynamically changing data is expanded (i.e. glasses,
            // stylus, or display).
            if (_isGlassesSectionExpanded ||
                _isStylusSectionExpanded  ||
                _isDisplaySectionExpanded)
            {
                if (_updateFrameCount >= 60)
                {
                    EditorUtility.SetDirty(this.target);
                    _updateFrameCount = 0;
                }

                ++_updateFrameCount;
            }
        }

        private void LoadIconTextures()
        {
            if (_infoIconTexture == null)
            {
                _infoIconTexture = this.LoadIconTexture("InfoIcon.png");
            }

            if (_debugIconTexture == null)
            {
                _debugIconTexture = this.LoadIconTexture("DebugIcon.png");
            }
            
            if (_cameraIconTexture == null)
            {
                _cameraIconTexture = this.LoadIconTexture("CameraIcon.png");
            }
            
            if (_glassesIconTexture == null)
            {
                _glassesIconTexture = this.LoadIconTexture("GlassesIcon.png");
            }
            
            if (_stylusIconTexture == null)
            {
                _stylusIconTexture = this.LoadIconTexture("StylusIcon.png");
            }
            
            if (_displayIconTexture == null)
            {
                _displayIconTexture = this.LoadIconTexture("DisplayIcon.png");
            }
        }

        private Texture2D LoadIconTexture(string iconName)
        {
            return AssetDatabase.LoadAssetAtPath(INSPECTOR_ICON_PATH + iconName, typeof(Texture2D)) as Texture2D;
        }

        private void FindSerializedProperties()
        {
            // Visual Debugging Properties:
            this.ShowLabelsProperty = this.serializedObject.FindProperty("ShowLabels");
            this.ShowViewportProperty = this.serializedObject.FindProperty("ShowViewport");
            this.ShowCCZoneProperty = this.serializedObject.FindProperty("ShowCCZone");
            this.ShowUCZoneProperty = this.serializedObject.FindProperty("ShowUCZone");
            this.ShowDisplayProperty = this.serializedObject.FindProperty("ShowDisplay");
            this.ShowRealWorldUpProperty = this.serializedObject.FindProperty("ShowRealWorldUp");
            this.ShowGroundPlaneProperty = this.serializedObject.FindProperty("ShowGroundPlane");
            this.ShowGlassesProperty = this.serializedObject.FindProperty("ShowGlasses");
            this.ShowStylusProperty = this.serializedObject.FindProperty("ShowStylus");

            // Stereo Rig Properties:
            this.CurrentCameraObjectProperty = this.serializedObject.FindProperty("CurrentCameraObject");
            this.EnableStereoProperty = this.serializedObject.FindProperty("EnableStereo");
            this.EnableAutoStereoProperty = this.serializedObject.FindProperty("EnableAutoStereo");
            this.CopyCurrentCameraAttributesProperty = this.serializedObject.FindProperty("CopyCurrentCameraAttributes");
            this.MinimizeLatencyProperty = this.serializedObject.FindProperty("MinimizeLatency");
            this.IpdProperty = this.serializedObject.FindProperty("Ipd");
            this.ViewerScaleProperty = this.serializedObject.FindProperty("ViewerScale");
            this.AutoStereoDelayProperty = this.serializedObject.FindProperty("AutoStereoDelay");
            this.AutoStereoDurationProperty = this.serializedObject.FindProperty("AutoStereoDuration");

            // Stylus Properties:
            this.EnableMouseEmulationProperty = this.serializedObject.FindProperty("EnableMouseEmulation");
            this.EnableMouseAutoHideProperty = this.serializedObject.FindProperty("EnableMouseAutoHide");
            this.MouseAutoHideDelayProperty = this.serializedObject.FindProperty("MouseAutoHideDelay");
        }

        private void InitializeGUIStyles()
        {
            if (_foldoutStyle == null)
            {
                _foldoutStyle = new GUIStyle(EditorStyles.foldout);
                _foldoutStyle.fontStyle = FontStyle.Bold;
                _foldoutStyle.fixedWidth = 2000.0f;
            }

            if (_lineStyle == null)
            {
                _lineStyle = new GUIStyle(GUI.skin.box);
                _lineStyle.border.top     = 1;
                _lineStyle.border.bottom  = 1;
                _lineStyle.margin.top     = 1;
                _lineStyle.margin.bottom  = 1;
                _lineStyle.padding.top    = 1;
                _lineStyle.padding.bottom = 1;
            }
        }

        private bool DrawSectionHeader(string name, Texture2D icon, bool isExpanded)
        {
            // Create the divider line.
            GUILayout.Box(GUIContent.none, _lineStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1.0f));

            // Create the foldout (AKA expandable section).
            Rect position = GUILayoutUtility.GetRect(40.0f, 2000.0f, 16.0f, 16.0f, _foldoutStyle);
            isExpanded = EditorGUI.Foldout(position, isExpanded, new GUIContent(" " + name, icon), true, _foldoutStyle);

            return isExpanded;
        }

        private void DrawToggleLeft(string label, SerializedProperty property)
        {
            property.boolValue = EditorGUILayout.ToggleLeft(new GUIContent(" " + label), property.boolValue);
        }

        private void DrawPoseInfo(string label, ZCore.Pose pose)
        {
            EditorGUILayout.LabelField(new GUIContent(label));

            // Readonly.
            GUI.enabled = false;
            EditorGUILayout.Vector3Field(new GUIContent("Position"), pose.Position);
            EditorGUILayout.Vector3Field(new GUIContent("Rotation"), pose.Rotation.eulerAngles);
            EditorGUILayout.Space();
            GUI.enabled = true;
        }


        //////////////////////////////////////////////////////////////////
        // Private Members
        //////////////////////////////////////////////////////////////////

        private static readonly string INSPECTOR_ICON_PATH = "Assets/zSpace/Core/Editor/Icons/";

        private int _updateFrameCount = 0;

        private Texture2D _infoIconTexture      = null;
        private Texture2D _debugIconTexture     = null;
        private Texture2D _cameraIconTexture    = null;
        private Texture2D _glassesIconTexture   = null;
        private Texture2D _stylusIconTexture    = null;
        private Texture2D _displayIconTexture   = null;

        private GUIStyle _foldoutStyle = null;
        private GUIStyle _lineStyle = null;

        private bool _isInfoSectionExpanded       = true;
        private bool _isDebugSectionExpanded      = true;
        private bool _isStereoRigSectionExpanded  = true;
        private bool _isGlassesSectionExpanded    = false;
        private bool _isStylusSectionExpanded     = false;
        private bool _isDisplaySectionExpanded    = false;

        private Vector3 _viewportCenter = Vector3.zero;
        private Vector3 _viewportRotation = Vector3.zero;
    }
}

