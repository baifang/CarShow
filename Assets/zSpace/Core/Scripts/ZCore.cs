//////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2016 zSpace, Inc.  All Rights Reserved.
//
//////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif 


namespace zSpace.Core
{
    [ExecuteInEditMode]
    public partial class ZCore : MonoBehaviour
    {
        //////////////////////////////////////////////////////////////////
        // Events
        //////////////////////////////////////////////////////////////////

        public delegate void EventHandler(ZCore sender);
        public delegate void TrackerEventHandler(ZCore sender, TrackerEventInfo info);
        public delegate void TrackerButtonEventHandler(ZCore sender, TrackerButtonEventInfo info);

        /// <summary>
        /// Event dispatched at the beginning of the internal core update.
        /// </summary>
        public event EventHandler PreUpdate;

        /// <summary>
        /// Event dispatched at the end of the internal core update.
        /// </summary>
        public event EventHandler PostUpdate;
        
        /// <summary>
        /// Event dispatched when a target moves.
        /// </summary>
        public event TrackerEventHandler TargetMove;

        /// <summary>
        /// Event dispatched when a target initially taps/presses the zSpace display.
        /// </summary>
        public event TrackerEventHandler TargetTapPress;

        /// <summary>
        /// Event dispatched when a target transitions to no longer touching the surface 
        /// of the zSpace display after a tap press has been initiated.
        /// </summary>
        public event TrackerEventHandler TargetTapRelease;

        /// <summary>
        /// Event dispatched when a target's button is pressed.
        /// </summary>
        public event TrackerButtonEventHandler TargetButtonPress;

        /// <summary>
        /// Event dispatched when a target's button is released.
        /// </summary>
        public event TrackerButtonEventHandler TargetButtonRelease;


        //////////////////////////////////////////////////////////////////
        // Enumerations
        //////////////////////////////////////////////////////////////////

        /// <summary>
        /// Defines the types of displays available.
        /// </summary>
        public enum DisplayType
        {
            Unknown = -1,
            Generic =  0,
            zSpace  =  1,
        }

        /// <summary>
        /// Defines the attributes queryable for the display.
        /// </summary>
        public enum DisplayAttribute
        {
            AdapterName      = 0,
            AdapterString    = 1,
            AdapterId        = 2,
            AdapterVendorId  = 3,
            AdapterDeviceId  = 4,
            AdapterKey       = 5,
            MonitorName      = 6,
            MonitorString    = 7,
            MonitorId        = 8,
            MonitorVendorId  = 9,
            MonitorDeviceId  = 10,
            MonitorKey       = 11,
            ManufacturerName = 12,
            ProductCode      = 13,
            SerialNumber     = 14,
            VideoInterface   = 15,
            Model            = 16,
        }

        /// <summary>
        /// Defines the coordinate spaces used by the zSpace Core SDK
        /// and Unity.
        /// </summary>
        public enum CoordinateSpace
        {
            Tracker  = 0,
            Display  = 1,
            Viewport = 2,
            Camera   = 3,
            World    = 4,
            NumSpaces,
        }

        /// <summary>
        /// Defines the attributes queryable and settable for the stereo frustum.
        /// </summary>
        public enum FrustumAttribute
        {
            /// <summary>
            /// The physical separation, or inter-pupillary distance, between the eyes
            /// in meters. An IPD of 0 will effectively disable stereo since the eyes are 
            /// assumed to be at the same location. (Default: 0.060)
            /// </summary>
            Ipd = 0,

            /// <summary>
            /// Adjusts the scale of the frustum. Use larger values for scenes with large models 
            /// and smallers values for smaller models. The default value of 1.0 denotes that 
            /// all content will be displayed at real-world scale in meters. (Default: 1)
            /// </summary>
            ViewerScale = 1,

            /// <summary>
            /// Uniform scale factor applied to the frustum's incoming head pose. (Default: 1)
            /// </summary>
            HeadScale = 3,

            /// <summary>
            /// Near clipping plane for the frustum in meters. (Default: 0.1)
            /// </summary>
            NearClip = 4,

            /// <summary>
            /// Far clipping plane for the frustum in meters. (Default: 1000)
            /// </summary>
            FarClip = 5,

            /// <summary>
            /// Distance between the bridge of the glasses and the bridge of the nose in 
            /// meters. (Default: 0.01)
            /// </summary>
            GlassesOffset = 6,

            /// <summary>
            /// Maximum pixel disparity for crossed images (negative parallax) in the 
            /// coupled zone. The coupled zone refers to the area where our eyes can both 
            /// comfortably converge and focus on an object. (Default: -100) 
            /// </summary>
            CCLimit = 7,

            /// <summary>
            /// Maximum pixel disparity for uncrossed images (positive parallax) in the 
            /// coupled zone. (Default: 100)
            /// </summary>
            UCLimit = 8,

            /// <summary>
            /// Maximum pixel disparity for crossed images (negative parallax) in the 
            /// uncoupled zone. (Default: -200)
            /// </summary>
            CULimit = 9,

            /// <summary>
            /// Maximum pixel disparity for uncrossed images (positive parallax) in the 
            /// uncoupled zone. (Default: 250)
            /// </summary>
            UULimit = 10,

            /// <summary>
            /// Maximum depth in meters for negative parallax in the coupled zone. (Default: 0.13)
            /// </summary>
            CCDepth = 11,

            /// <summary>
            /// Maximum depth in meters for positive parallax in the coupled zone. (Default: -0.30)
            /// </summary>
            UCDepth = 12,

            /// <summary>
            /// Display angle in degrees about the X axis. Is only used when PortalMode.Angle is
            /// not enabled on the frustum. (Default: 30.0)
            /// </summary>
            DisplayAngleX = 13,

            /// <summary>
            /// Display angle in degrees about the Y axis. Is only used when PortalMode.Angle is
            /// not enabled on the frustum. (Default: 0.0)
            /// </summary>
            DisplayAngleY = 14,

            /// <summary>
            /// Display angle in degrees about the Z axis. Is only used when PortalMode.Angle is
            /// not enabled on the frustum. (Default: 0.0)
            /// </summary>
            DisplayAngleZ = 15,
        }

        /// <summary>
        /// Defines options for positioning the scene relative to the physical display
        /// or relative to the viewport.
        /// </summary>
        [Flags]
        public enum PortalMode
        {
            /// <summary>
            /// The scene is positioned relative to the viewport.
            /// </summary>
            None = 0,

            /// <summary>
            /// The scene's orientation is fixed relative to the physical desktop.
            /// </summary>
            Angle = 1,

            /// <summary>
            /// The scene's position is fixed relative to the center of the display.
            /// </summary>
            Position = 2,

            /// <summary>
            /// All portal modes except "none" are enabled.
            /// </summary>
            All = ~0, 
        }

        /// <summary>
        /// Defines the eyes for the stereo frustum.
        /// </summary>
        public enum Eye
        {
            Left   = 0,
            Right  = 1,
            Center = 2,
            NumEyes,
        }

        /// <summary>
        /// Defines the types of 6DOF trackable targets supported.
        /// </summary>
        public enum TargetType
        {
            /// <summary>
            /// The target corresponding to the user's head.
            /// </summary>
            Head = 0,

            /// <summary>
            /// The target corresponding to the user's primary hand.
            /// </summary>
            Primary = 1,

            NumTypes,
        }

        /// <summary>
        /// Defines the modes in which the mouse emulator can move the mouse.
        /// </summary>
        public enum MovementMode
        {
            /// <summary>
            /// The stylus uses absolute positions. In this mode, the mouse and target (currently 
            /// emulating the mouse) can fight for control of the cursor if both are in use. This is 
            /// the default mode. 
            /// </summary>
            Absolute = 0,

            /// <summary>
            /// The stylus applies delta positions to the mouse cursor's current position.
            /// Movements by the mouse and target (currently emulating the mouse) are compounded 
            /// without fighting. 
            /// </summary>
            Relative = 1,
        }

        /// <summary>
        /// Defines the mouse buttons available when mapping a target's buttons to
        /// corresponding mouse buttons.
        /// </summary>
        public enum MouseButton
        {
            Unknown = -1,
            Left    =  0,
            Right   =  1,
            Center  =  2,
        }


        //////////////////////////////////////////////////////////////////
        // Compound Types
        //////////////////////////////////////////////////////////////////

        /// <summary>
        /// Info class representing all information returned after performing a display 
        /// intersection collision query.
        /// </summary>
        public class DisplayIntersectionInfo
        {
            /// <summary>
            /// Whether the display was intersected.
            /// </summary>
            public bool Hit { get; private set; }

            /// <summary>
            /// The distance from the origin of the raycast to the point of intersection
            /// on the display in meters.
            /// </summary>
            public float Distance { get; private set; }

            /// <summary>
            /// The x pixel coordinate on the virtual desktop.
            /// </summary>
            public int X { get; private set; }

            /// <summary>
            /// The y pixel coordinate on the virtual desktop.
            /// </summary>
            public int Y { get; private set; }

            /// <summary>
            /// The normalized absolute x pixel coordinate on the virtual desktop.
            /// </summary>
            public int NX { get; private set; }

            /// <summary>
            /// The normalized absolute y pixel coordinate on the virtual desktop.
            /// </summary>
            public int NY { get; private set; }

            public DisplayIntersectionInfo(bool hit, float distance, int x, int y, int nx, int ny)
            {
                this.Hit      = hit;
                this.Distance = distance;
                this.X        = x;
                this.Y        = y;
                this.NX       = nx;
                this.NY       = ny;
            }
        }

        /// <summary>
        /// Class representing the bounds of a frustum. All property values are in meters.
        /// </summary>
        public class FrustumBounds
        {
            public float Left   { get; private set; }
            public float Right  { get; private set; }
            public float Bottom { get; private set; }
            public float Top    { get; private set; }
            public float Near   { get; private set; }
            public float Far    { get; private set; }

            public FrustumBounds(float left, float right, float bottom, float top, float near, float far)
            {
                this.Left   = left;
                this.Right  = right;
                this.Bottom = bottom;
                this.Top    = top;
                this.Near   = near;
                this.Far    = far;
            }
        }

        /// <summary>
        /// Class representing a target's 6DOF pose.
        /// </summary>
        public class Pose
        {
            /// <summary>
            /// The position and orientation in 4x4 matrix format.
            /// </summary>
            public Matrix4x4 Matrix { get; private set; }

            /// <summary>
            /// The position in meters.
            /// </summary>
            public Vector3 Position { get; private set; }

            /// <summary>
            /// The orientation.
            /// </summary>
            public Quaternion Rotation { get; private set; }

            /// <summary>
            /// The forward direction.
            /// </summary>
            public Vector3 Direction { get; private set; }

            /// <summary>
            /// The time that the pose was captured (represented in seconds since last system reboot).
            /// </summary>
            public double Timestamp { get; private set; }

            /// <summary>
            /// The coordinate space of the pose.
            /// </summary>
            public CoordinateSpace CoordinateSpace { get; private set; }
            
            public Pose(Matrix4x4 matrix, double timestamp, CoordinateSpace coordinateSpace)
            {
                this.Matrix          = matrix;
                this.Position        = matrix.GetColumn(3);
                this.Rotation        = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
                this.Direction       = (this.Rotation * Vector3.forward).normalized;
                this.Timestamp       = timestamp;
                this.CoordinateSpace = coordinateSpace;
            }

            public Pose(Vector3 position, Quaternion rotation, double timestamp, CoordinateSpace coordinateSpace)
            {
                this.Matrix          = Matrix4x4.TRS(position, rotation, Vector3.one);
                this.Position        = position;
                this.Rotation        = rotation;
                this.Direction       = (this.Rotation * Vector3.forward).normalized;
                this.Timestamp       = timestamp;
                this.CoordinateSpace = coordinateSpace;
            }
        }

        /// <summary>
        /// Base info class for tracker based events.
        /// </summary>
        public class TrackerEventInfo
        {
            /// <summary>
            /// A handle to the target responsible for generating the event.
            /// </summary>
            public IntPtr TargetHandle { get; private set; }

            /// <summary>
            /// The target's type.
            /// </summary>
            public TargetType TargetType { get; private set; }

            /// <summary>
            /// The target's current world-space pose.
            /// </summary>
            public Pose WorldPose { get; private set; }

            public TrackerEventInfo(IntPtr targetHandle, TargetType targetType, Pose worldPose)
            {
                this.TargetHandle = targetHandle;
                this.TargetType   = targetType;
                this.WorldPose    = worldPose;
            }
        }

        /// <summary>
        /// Info class for target button events.
        /// </summary>
        public class TrackerButtonEventInfo : TrackerEventInfo
        {
            /// <summary>
            /// The id of the button responsible for generating the event.
            /// </summary>
            public int ButtonId { get; private set; }

            public TrackerButtonEventInfo(IntPtr targetHandle, TargetType targetType, Pose worldPose, int buttonId)
                : base(targetHandle, targetType, worldPose)
            {
                this.ButtonId = buttonId;
            }
        }


        //////////////////////////////////////////////////////////////////
        // Unity Inspector Fields
        //////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show labels for all debug related visualizations that are
        /// rendered to the Scene View window.
        /// </summary>
        public bool ShowLabels = true;

        /// <summary>
        /// Shows a debug visualization of the application's viewport (zero parallax)
        /// in the Scene View window.
        /// </summary>
        public bool ShowViewport = true;

        /// <summary>
        /// Shows a debug visualization of the negative parallax comfort zone
        /// in the Scene View window.
        /// </summary>
        public bool ShowCCZone = false;

        /// <summary>
        /// Shows a debug visualization of the positive parallax comfort zone
        /// in the Scene View window.
        /// </summary>
        public bool ShowUCZone = false;

        /// <summary>
        /// Shows a debug visualization of the viewport's current display (zero parallax)
        /// in the Scene View window.
        /// </summary>
        public bool ShowDisplay = true;

        /// <summary>
        /// Shows a debug visualization of the real-world up vector in the
        /// Scene View window.
        /// </summary>
        public bool ShowRealWorldUp = false;

        /// <summary>
        /// Shows a debug visualization of the real-world ground plane in the
        /// Scene View window.
        /// </summary>
        public bool ShowGroundPlane = false;

        /// <summary>
        /// Shows the world-space position and orientation of the glasses in 
        /// the Scene View window.
        /// </summary>
        public bool ShowGlasses = true;

        /// <summary>
        /// Shows the world-space position and orientation of the stylus in
        /// the Scene View window.
        /// </summary>
        public bool ShowStylus = true;

        /// <summary>
        /// The current camera GameObject responsible for driving the stereo rig.
        /// </summary>
        public GameObject CurrentCameraObject = null;

        /// <summary>
        /// Enables stereoscopic 3D rendering.
        /// </summary>
        public bool EnableStereo = true; 

        /// <summary>
        /// Enables whether the stereo rig will automatically transition between
        /// stereoscopic 3D and monoscopic 3D based on the visibility of the
        /// default head target (glasses).
        /// </summary>
        public bool EnableAutoStereo = true;

        /// <summary>
        /// Controls whether a subset of the current camera's attributes are copied
        /// to the stereo rig's left and right cameras when the current camera has
        /// changed.
        /// </summary>
        public bool CopyCurrentCameraAttributes = true;

        /// <summary>
        /// Minimizes the latency between the time Core captures the latest head
        /// pose information and applies it to the stereo rig's left and right
        /// view/projection matrices.
        /// </summary>
        /// 
        /// <remarks>
        /// Minimizing latency may cause undesirable effects with respect to maintaining 
        /// synchronization between the head pose queryable through GetTargetPose() 
        /// and the head pose used to calculate the view/projection matrices for the
        /// stereo rig.
        /// </remarks>
        public bool MinimizeLatency = true;

        /// <summary>
        /// The physical separation, or inter-pupillary distance, between the 
        /// eyes in meters.
        /// </summary>
        public float Ipd = 0.06f;

        /// <summary>
        /// Adjusts the scale of the stereo rig's frustums. Use larger values for scenes 
        /// with large models and smallers values for smaller models. The default value of 
        /// 1.0 denotes that all content will be displayed at real-world scale in meters.
        /// </summary>
        public float ViewerScale = 1.0f;

        /// <summary>
        /// The delay in seconds before the automatic transition from stereo to
        /// mono begins.
        /// </summary>
        public float AutoStereoDelay = 5.0f;

        /// <summary>
        /// The duration in seconds of the automatic transition from stereo to mono.
        /// </summary>
        public float AutoStereoDuration = 1.0f;

        /// <summary>
        /// Enables mouse emulation.
        /// </summary>
        /// 
        /// <remarks>
        /// The default target responsible for emulating the mouse is the stylus.
        /// This can be changed via the SetMouseEmulationTarget() method.
        /// </remarks>
        public bool EnableMouseEmulation = false;

        /// <summary>
        /// Enables the mouse cursor to auto-hide after a specified time in seconds
        /// due to inactivity.
        /// </summary>
        public bool EnableMouseAutoHide = false;

        /// <summary>
        /// The time in seconds of mouse inactivity before its cursor will be hidden
        /// if EnableMouseAutoHide is true.
        /// </summary>
        public float MouseAutoHideDelay = 5.0f;


        //////////////////////////////////////////////////////////////////
        // Unity Monobehaviour Callbacks
        //////////////////////////////////////////////////////////////////

        void Awake()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                return;
            }

            // Force initialization of the global state.
            GlobalState globalState = GlobalState.Instance;
            if (globalState == null || !globalState.IsInitialized)
            {
                Debug.LogError("Failed to initialize global state. Disabling zCore GameObject");
                this.gameObject.SetActive(false);
                return;
            }

            // Guarantee that the current camera object has been 
            // properly initialized.
            if (this.CurrentCameraObject == null && Camera.main != null)
            {
                this.CurrentCameraObject = Camera.main.gameObject;
            }

            // Continue initialization.
            this.InitializeLRDetect();
            this.InitializeStereoInfo();
            this.InitializeStereoRig();
            this.InitializeTrackingInfo();

            // Kick off the end-of-frame update coroutine.
            this.StartCoroutine(EndOfFrameUpdate());
        }

        void OnEnable()
        {
        #if UNITY_EDITOR
            // Force initialization of the global state.
            GlobalState globalState = GlobalState.Instance;
            if (globalState == null || !globalState.IsInitialized)
            {
                Debug.LogError("Failed to initialize global state. Disabling zCore GameObject.");
                this.gameObject.SetActive(false);
                return;
            }

            if (Application.isEditor && !Application.isPlaying)
            {
                this.InitializeStereoInfo();
                this.InitializeStereoRig();
                this.InitializeTrackingInfo();

                _windowWidth = Screen.width;
                _windowHeight = Screen.height;

                EditorApplication.update -= this.EditModeUpdate;
                EditorApplication.update += this.EditModeUpdate;
            }
        #endif
        }

        void OnDisable()
        {
        #if UNITY_EDITOR
            if (Application.isEditor && !Application.isPlaying)
            {
                EditorApplication.update -= this.EditModeUpdate;
            }
        #endif
        }

        void Update()
        {
            this.PlayModeUpdate();
        }

        void LateUpdate()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                return;
            }

            // If stereo is enabled, disable the current camera to prevent 
            // it from rendering.
            if (_currentCamera != null && _isStereoEnabled)
            {
                _currentCamera.enabled = false;
            }
        }

        void OnGUI()
        {
            this.EditModeUpdateViewportDimensions();
        }

        void OnDrawGizmos()
        {
        #if UNITY_EDITOR
            // Cache original colors and matrices to be restored after
            // drawing is finished.
            Color     originalGizmosColor   = Gizmos.color;
            Matrix4x4 originalGizmosMatrix  = Gizmos.matrix;
            
            Color     originalHandlesColor  = Handles.color;
            Matrix4x4 originalHandlesMatrix = Handles.matrix;            

            // Grab coordinate space transformations.
            Matrix4x4 displayToWorld   = this.GetCoordinateSpaceTransform(CoordinateSpace.Display, CoordinateSpace.World);
            Matrix4x4 trackerToWorld   = this.GetCoordinateSpaceTransform(CoordinateSpace.Tracker, CoordinateSpace.World);
            Matrix4x4 displayToTracker = this.GetCoordinateSpaceTransform(CoordinateSpace.Display, CoordinateSpace.Tracker);

            // Grab the comfort zone depth.
            float ccDepth = -this.GetFrustumAttributeFloat(FrustumAttribute.CCDepth);
            float ucDepth = -this.GetFrustumAttributeFloat(FrustumAttribute.UCDepth);

            // Grab the viewport's current display number and type.
            int displayNumber = -1;
            DisplayType displayType = DisplayType.Unknown;
            if (_displayHandle != IntPtr.Zero)
            {
                displayNumber = this.GetDisplayNumber(_displayHandle);
                displayType = this.GetDisplayType(_displayHandle);
            }

            // Calculate the viewport half size in meters.
            Vector2 viewportHalfSizeInMeters = _viewportSizeInMeters * 0.5f;
            
            // Draw coupled zone (negative parallax).
            if (this.ShowCCZone)
            {
                Gizmos.color = Color.black;
                Gizmos.matrix = displayToWorld;

                Vector3 center = new Vector3(_viewportDisplayCenter.x, _viewportDisplayCenter.y, ccDepth * 0.5f);
                Vector3 size   = new Vector3(_viewportSizeInMeters.x, _viewportSizeInMeters.y, ccDepth);

                Gizmos.DrawWireCube(center, size);

                if (this.ShowLabels)
                {
                    Vector3 labelPosition = new Vector3(center.x, center.y + viewportHalfSizeInMeters.y, center.z + ccDepth * 0.5f);

                    Handles.matrix = displayToWorld;
                    Handles.Label(labelPosition, new GUIContent("Comfort Zone (Negative Parallax)"));
                }
            }
            
            // Draw uncoupled zone (positive parallax).
            if (this.ShowUCZone)
            {
                Gizmos.color = Color.red;
                Gizmos.matrix = displayToWorld;

                Vector3 center = new Vector3(_viewportDisplayCenter.x, _viewportDisplayCenter.y, ucDepth * 0.5f);
                Vector3 size   = new Vector3(_viewportSizeInMeters.x, _viewportSizeInMeters.y, ucDepth);

                Gizmos.DrawWireCube(center, size);

                if (this.ShowLabels)
                {
                    Vector3 labelPosition = new Vector3(center.x, center.y + viewportHalfSizeInMeters.y, center.z + ucDepth * 0.5f);

                    Handles.matrix = displayToWorld;
                    Handles.Label(labelPosition, new GUIContent("Comfort Zone (Positive Parallax)"));
                }
            }
            
            // Draw the viewport bounds at the zero parallax plane.
            if (this.ShowViewport)
            {
                Gizmos.color = Color.white;
                Gizmos.matrix = displayToWorld;
                Gizmos.DrawWireCube(_viewportDisplayCenter, _viewportSizeInMeters);

                Handles.matrix = displayToWorld;
                this.DrawTriAxes(_viewportDisplayCenter + viewportHalfSizeInMeters, Quaternion.identity, 0.01f, true);

                if (this.ShowLabels)
                {
                    Handles.matrix = displayToWorld;
                    Handles.Label(_viewportDisplayCenter + (Vector2.up * viewportHalfSizeInMeters.y), new GUIContent("Viewport (Zero Parallax)"));
                }
            }

            // Draw the display bounds at the zero parallax plane.
            if (this.ShowDisplay)
            {
                Gizmos.color = Color.yellow;
                Gizmos.matrix = displayToWorld;

                Gizmos.DrawWireCube(Vector3.zero, new Vector3(_displaySize.x, _displaySize.y));

                if (this.ShowLabels)
                {
                    Handles.matrix = displayToWorld;
                    Handles.Label(new Vector3(0.0f, _displaySize.y * 0.5f, 0.0f), new GUIContent(string.Format("Display {0} ({1})", displayNumber, displayType)));
                }
            }

            // Draw the real world ground plane.
            if (this.ShowGroundPlane)
            {
                Gizmos.color = Color.white;
                Gizmos.matrix = trackerToWorld;

                Vector3 center = displayToTracker * new Vector4(_viewportDisplayCenter.x, _viewportDisplayCenter.y - viewportHalfSizeInMeters.y);
                center.z      += viewportHalfSizeInMeters.y;
                Vector3 size   = new Vector3(_viewportSizeInMeters.x, 0.0f, _viewportSizeInMeters.y);

                Gizmos.DrawWireCube(center, size);

                if (this.ShowLabels)
                {
                    Handles.matrix = trackerToWorld;
                    Handles.Label(center + (Vector3.forward * viewportHalfSizeInMeters.y), new GUIContent("Ground Plane"));
                }
            }
            
            // Draw real-world up vector.
            if (this.ShowRealWorldUp)
            {
                Handles.color = Color.green;
                Handles.matrix = trackerToWorld;

                Vector3    position = displayToTracker * new Vector4(_viewportDisplayCenter.x, _viewportDisplayCenter.y);
                Quaternion rotation = Quaternion.Euler(-90.0f, 0.0f, 0.0f);

                Handles.ArrowCap(0, position, rotation, 0.05f);

                if (this.ShowLabels)
                {
                    Handles.Label(position + (Vector3.up * 0.065f), new GUIContent("Real-World Up"));
                }
            }

            // Draw the glasses transform.
            if (this.ShowGlasses)
            {
                bool isVisible = this.IsTargetVisible(TargetType.Head);
                Pose pose = this.GetTargetPose(TargetType.Head, CoordinateSpace.World);
                
                Handles.matrix = pose.Matrix;

                this.DrawTriAxes(Vector3.zero, Quaternion.identity, 0.03f, isVisible);

                if (this.ShowLabels)
                {
                    Handles.Label(Vector3.zero, new GUIContent("Glasses"));
                }
            }

            // Draw the stylus transform.
            if (this.ShowStylus)
            {
                bool isVisible = this.IsTargetVisible(TargetType.Primary);
                Pose pose = this.GetTargetPose(TargetType.Primary, CoordinateSpace.World);
                
                Handles.matrix = pose.Matrix;

                this.DrawTriAxes(Vector3.zero, Quaternion.identity, 0.03f, isVisible);

                if (this.ShowLabels)
                {
                    Handles.Label(Vector3.zero, new GUIContent("Stylus"));
                }
            }

            // Restore original colors and matrices.
            Gizmos.color   = originalGizmosColor;
            Gizmos.matrix  = originalGizmosMatrix;

            Handles.color  = originalHandlesColor;
            Handles.matrix = originalHandlesMatrix;
        #endif
        }

        void OnDestroy()
        {
            ZCore.IssuePluginEvent(ZCore.PluginEvent.DisableStereo);
            GL.InvalidateState();
        }

        void OnApplicationQuit()
        {
            this.ShutdownLRDetect();

            ZCore.IssuePluginEvent(ZCore.PluginEvent.DisableStereo);
            GL.InvalidateState();
        }


        //////////////////////////////////////////////////////////////////
        // Public API
        //////////////////////////////////////////////////////////////////

        /// <summary>
        /// Checks whether the zSpace Core SDK was properly initialized.
        /// </summary>
        /// 
        /// <returns>
        /// True if initialized. False otherwise.
        /// </returns>
        public bool IsInitialized()
        {
            return GlobalState.Instance.IsInitialized;
        }

        /// <summary>
        /// Gets the current version of the zSpace Core Unity plugin.
        /// </summary>
        /// 
        /// <returns>
        /// The plugin version in major.minor.patch string format.
        /// </returns>
        public string GetPluginVersion()
        {
            int major = 0;
            int minor = 0;
            int patch = 0;

            zcuGetPluginVersion(out major, out minor, out patch);

            return string.Format("{0}.{1}.{2}", major, minor, patch); 
        }

        /// <summary>
        /// Gets the current runtime version of the zSpace Core SDK.
        /// </summary>
        /// 
        /// <returns>
        /// The runtime version in major.minor.patch string format.
        /// </returns>
        public string GetRuntimeVersion()
        {
            int major = 0;
            int minor = 0;
            int patch = 0;

            PluginError error = zcuGetRuntimeVersion(GlobalState.Instance.Context, out major, out minor, out patch);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return string.Format("{0}.{1}.{2}", major, minor, patch);
        }

        /// <summary>
        /// Sets whether stereoscopic 3D is enabled.
        /// </summary>
        /// 
        /// <param name="isEnabled">
        /// True to enable stereoscopic 3D. False otherwise.
        /// </param>
        public void SetStereoEnabled(bool isEnabled)
        {
            if (Application.isPlaying)
            {
                _stereoCameras[(int)Eye.Left].SetCameraEnabled(isEnabled);
                _stereoCameras[(int)Eye.Right].SetCameraEnabled(isEnabled);

                if (this.CurrentCameraObject != null)
                {
                    this.CurrentCameraObject.GetComponent<Camera>().enabled = !isEnabled;
                }
            }
            
            this.EnableStereo = isEnabled;
            _isStereoEnabled  = isEnabled;
        }

        /// <summary>
        /// Checks whether stereoscopic 3D is enabled.
        /// </summary>
        /// 
        /// <returns>
        /// True if stereoscopic 3D is enabled. False otherwise.
        /// </returns>
        public bool IsStereoEnabled()
        {
            return _isStereoEnabled;
        }

        /// <summary>
        /// Sets whether tracking is enabled globally.
        /// </summary>
        /// 
        /// <remarks>
        /// This setting is applied to all tracking devices and targets.
        /// </remarks>
        /// 
        /// <param name="isEnabled">
        /// True to enable tracking. False otherwise.
        /// </param>
        public void SetTrackingEnabled(bool isEnabled)
        {
            PluginError error = zcuSetTrackingEnabled(GlobalState.Instance.Context, isEnabled);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
        }

        /// <summary>
        /// Checks whether tracking is enabled globally for all devices and targets.
        /// </summary>
        /// 
        /// <returns>
        /// True if tracking is enabled globally. False otherwise.
        /// </returns>
        public bool IsTrackingEnabled()
        {
            bool isEnabled = false;
            
            PluginError error = zcuIsTrackingEnabled(GlobalState.Instance.Context, out isEnabled);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }

            return isEnabled;
        }

        /// <summary>
        /// Gets the associated StereoCamera for the specified eye.
        /// </summary>
        /// 
        /// <param name="eye">
        /// The eye (left, right, or center).
        /// </param>
        /// 
        /// <returns>
        /// A reference to the eye’s StereoCamera.
        /// </returns>
        public StereoCamera GetStereoCamera(Eye eye)
        {
            return _stereoCameras[(int)eye];
        }

        /// <summary>
        /// Refreshes all internally cached display information.
        /// </summary>
        /// 
        /// <remarks>
        /// All display handles used prior to invoking RefreshDisplays()
        /// will be invalidated.
        /// </remarks>
        public void RefreshDisplays()
        {
            PluginError error = zcuRefreshDisplays(GlobalState.Instance.Context);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
        }

        /// <summary>
        /// Retrieves the total number of connected displays.
        /// </summary>
        /// 
        /// <returns>
        /// The total number of connected displays.
        /// </returns>
        public int GetNumDisplays()
        {
            int numDisplays = 0;

            PluginError error = zcuGetNumDisplays(GlobalState.Instance.Context, out numDisplays);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return numDisplays;
        }

        /// <summary>
        /// Retrieves the total number of connected displays of a specified type.
        /// </summary>
        /// 
        /// <param name="displayType">
        /// The type of display.
        /// </param>
        /// 
        /// <returns>
        /// The total number of connected displays of the specified type.
        /// </returns>
        public int GetNumDisplays(DisplayType displayType)
        {
            int numDisplays = 0;

            PluginError error = zcuGetNumDisplaysByType(GlobalState.Instance.Context, displayType, out numDisplays);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }

            return numDisplays;
        }

        /// <summary>
        /// Gets a handle to the display based on the specified (x, y) pixel 
        /// coordinates on the virtual desktop.
        /// </summary>
        /// 
        /// <param name="x">
        /// The x pixel coordinate on the virtual desktop.
        /// </param>
        /// <param name="y">
        /// The y pixel coordinate on the virtual desktop.
        /// </param>
        /// 
        /// <returns>
        /// The handle of the display at the specified (x, y) pixel location.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.DisplayNotFoundException">
        /// Thrown if a display is not found for the specified set of pixel coordinates.
        /// </exception>
        public IntPtr GetDisplay(int x, int y)
        {
            IntPtr displayHandle = IntPtr.Zero;

            PluginError error = zcuGetDisplay(GlobalState.Instance.Context, x, y, out displayHandle);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return displayHandle;
        }

        /// <summary>
        /// Gets a handle to the display at a specified index.
        /// </summary>
        /// 
        /// <param name="index">
        /// The index of the display to query.
        /// </param>
        /// 
        /// <returns>
        /// The handle of the display at the specified index.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.DisplayNotFoundException">
        /// Thrown if a display is not found for the specified index.
        /// </exception>
        public IntPtr GetDisplay(int index)
        {
            IntPtr displayHandle = IntPtr.Zero;

            PluginError error = zcuGetDisplayByIndex(GlobalState.Instance.Context, index, out displayHandle);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }

            return displayHandle;
        }

        /// <summary>
        /// Gets the display handle for a specified type and index.
        /// </summary>
        /// 
        /// <param name="displayType">
        /// The display type to query.
        /// </param>
        /// <param name="index">
        /// The index of the display to query.
        /// </param>
        /// 
        /// <returns>
        /// The handle of the display for the specified type and index.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.DisplayNotFoundException">
        /// Thrown if a display is not found for the specified type and index.
        /// </exception>
        public IntPtr GetDisplay(DisplayType displayType, int index)
        {
            IntPtr displayHandle = IntPtr.Zero;

            PluginError error = zcuGetDisplayByType(GlobalState.Instance.Context, displayType, index, out displayHandle);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }

            return displayHandle;
        }

        /// <summary>
        /// Gets the type of the viewport's current display.
        /// </summary>
        /// 
        /// <returns>
        /// The current display’s type.
        /// </returns>
        public DisplayType GetDisplayType()
        {
            return this.GetDisplayType(_displayHandle);
        }

        /// <summary>
        /// Gets the specified display’s type.
        /// </summary>
        /// 
        /// <param name="displayHandle">
        /// A handle to the display.
        /// </param>
        /// 
        /// <returns>
        /// The display’s type.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the display handle is invalid.
        /// </exception>
        public DisplayType GetDisplayType(IntPtr displayHandle)
        {
            DisplayType displayType = DisplayType.Unknown;

            PluginError error = zcuGetDisplayType(displayHandle, out displayType);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }

            return displayType;
        }

        /// <summary>
        /// Gets the display number of the viewport's current display.
        /// </summary>
        /// 
        /// <returns>
        /// The number of the current display.
        /// </returns>
        public int GetDisplayNumber()
        {
            return this.GetDisplayNumber(_displayHandle);
        }

        /// <summary>
        /// Gets the number of the specified display.
        /// </summary>
        /// 
        /// <param name="displayHandle">
        /// A handle to the display.
        /// </param>
        /// 
        /// <returns>
        /// The number of the specified display.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the display handle is invalid.
        /// </exception>
        public int GetDisplayNumber(IntPtr displayHandle)
        {
            int displayNumber = -1;

            PluginError error = zcuGetDisplayNumber(displayHandle, out displayNumber);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }

            return displayNumber;
        }

        /// <summary>
        /// Gets the adapter index of the viewport's current display.
        /// </summary>
        /// 
        /// <returns>
        /// The adapter index of the current display.
        /// </returns>
        public int GetDisplayAdapterIndex()
        {
            return this.GetDisplayAdapterIndex(_displayHandle);
        }

        /// <summary>
        /// Gets the adapter index of the specified display.
        /// </summary>
        /// 
        /// <param name="displayHandle">
        /// A handle to the display.
        /// </param>
        /// 
        /// <returns>
        /// The adapter index of the specified display.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the display handle is invalid.
        /// </exception>
        public int GetDisplayAdapterIndex(IntPtr displayHandle)
        {
            int adapterIndex = -1;

            PluginError error = zcuGetAdapterIndex(displayHandle, out adapterIndex);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return adapterIndex;
        }

        /// <summary>
        /// Gets the monitor index of the viewport's current display.
        /// </summary>
        /// 
        /// <returns>
        /// The monitor index of the current display.
        /// </returns>
        public int GetDisplayMonitorIndex()
        {
            return this.GetDisplayMonitorIndex(_displayHandle);
        }

        /// <summary>
        /// Gets the monitor index of the specified display.
        /// </summary>
        /// 
        /// <param name="displayHandle">
        /// A handle to the display.
        /// </param>
        /// 
        /// <returns>
        /// The monitor index of the specified display.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the display handle is invalid.
        /// </exception>
        public int GetDisplayMonitorIndex(IntPtr displayHandle)
        {
            int monitorIndex = -1;

            PluginError error = zcuGetMonitorIndex(displayHandle, out monitorIndex);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }

            return monitorIndex;
        }

        /// <summary>
        /// Gets the string value of the specified attribute for the 
        /// viewport's current display.
        /// </summary>
        /// 
        /// <param name="attribute">
        /// The attribute to query.
        /// </param>
        /// 
        /// <returns>
        /// The attribute's string value.
        /// </returns>
        public string GetDisplayAttributeString(DisplayAttribute attribute)
        {
            return this.GetDisplayAttributeString(_displayHandle, attribute);
        }

        /// <summary>
        /// Gets the string value of the specified attribute for the 
        /// specified display. 
        /// </summary>
        /// 
        /// <param name="displayHandle">
        /// A handle to the display.
        /// </param>
        /// <param name="attribute">
        /// The attribute to query.
        /// </param>
        /// 
        /// <returns>
        /// The attribute's string value.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the display handle is invalid.
        /// </exception>
        /// <exception cref="zSpace.Core.InvalidAttributeException">
        /// Thrown if the attribute is invalid.
        /// </exception>
        public string GetDisplayAttributeString(IntPtr displayHandle, DisplayAttribute attribute)
        {
            // Get the size of the display's attribute value.
            int attributeValueSize = 500;
            zcuGetDisplayAttributeStrSize(displayHandle, attribute, out attributeValueSize);

            // Get the attribute value.
            StringBuilder attributeValue = new StringBuilder(attributeValueSize);
            PluginError error = zcuGetDisplayAttributeStr(displayHandle, attribute, attributeValue, attributeValueSize);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return attributeValue.ToString();
        }

        /// <summary>
        /// Gets the size of the viewport's current display in meters.
        /// </summary>
        /// 
        /// <returns>
        /// The size (width, height) in meters of the current display.
        /// </returns>
        public Vector2 GetDisplaySize()
        {
            return this.GetDisplaySize(_displayHandle);
        }

        /// <summary>
        /// Gets the size of the specified display in meters.
        /// </summary>
        /// 
        /// <param name="displayHandle">
        /// A handle to the display.
        /// </param>
        /// 
        /// <returns>
        /// The size (width, height) in meters of the specified display.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the display handle is invalid.
        /// </exception>
        public Vector2 GetDisplaySize(IntPtr displayHandle)
        {
            float width = 0.0f;
            float height = 0.0f;

            PluginError error = zcuGetDisplaySize(displayHandle, out width, out height);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return new Vector2(width, height);
        }

        /// <summary>
        /// Gets the (x, y) pixel location of the viewport's current display on 
        /// the virtual desktop (top-left corner).
        /// </summary>
        /// 
        /// <returns>
        /// The (x, y) pixel location of the current display on the virtual 
        /// desktop (top-left corner).
        /// </returns>
        public Vector2 GetDisplayPosition()
        {
            return this.GetDisplayPosition(_displayHandle);
        }

        /// <summary>
        /// Gets the (x, y) pixel location of the specified display on the virtual 
        /// desktop (top-left corner).
        /// </summary>
        /// 
        /// <param name="displayHandle">
        /// A handle to the display.
        /// </param>
        /// 
        /// <returns>
        /// The (x, y) pixel location of the specified display on the virtual 
        /// desktop (top-left corner).
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the display handle is invalid.
        /// </exception>
        public Vector2 GetDisplayPosition(IntPtr displayHandle)
        {
            int x = 0;
            int y = 0;

            PluginError error = zcuGetDisplayPosition(displayHandle, out x, out y);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return new Vector2(x, y);
        }

        /// <summary>
        /// Gets the preferred native resolution in pixels of viewport's
        /// current display.
        /// </summary>
        /// 
        /// <returns>
        /// The native resolution in pixels of the current display.
        /// </returns>
        public Vector2 GetDisplayNativeResolution()
        {
            return this.GetDisplayNativeResolution(_displayHandle);
        }

        /// <summary>
        /// Gets the preferred native resolution in pixels of the specified display.
        /// </summary>
        /// 
        /// <param name="displayHandle">
        /// A handle to the display.
        /// </param>
        /// 
        /// <returns>
        /// The native resolution in pixels of the specified display.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the display handle is invalid.
        /// </exception>
        public Vector2 GetDisplayNativeResolution(IntPtr displayHandle)
        {
            int x = 0;
            int y = 0;
            
            PluginError error = zcuGetDisplayNativeResolution(displayHandle, out x, out y);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return new Vector2(x, y);
        }

        /// <summary>
        /// Gets the angles in degrees about each axis of the viewport's 
        /// current display.
        /// </summary>
        /// 
        /// <returns>
        /// The angles in degrees about each axis of the current display.
        /// </returns>
        public Vector3 GetDisplayAngle()
        {
            return this.GetDisplayAngle(_displayHandle);
        }

        /// <summary>
        /// Gets the angles in degrees about each axis of the specified
        /// display.
        /// </summary>
        /// 
        /// <param name="displayHandle">
        /// A handle to the display.
        /// </param>
        /// 
        /// <returns>
        /// The angles in degrees about each axis of the specified display.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the display handle is invalid.
        /// </exception>
        public Vector3 GetDisplayAngle(IntPtr displayHandle)
        {
            float x = 0.0f;
            float y = 0.0f;
            float z = 0.0f;

            PluginError error = zcuGetDisplayAngle(displayHandle, out x, out y, out z);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Gets the vertical refresh rate of the viewport's current display.
        /// </summary>
        /// 
        /// <returns>
        /// The vertical refresh rate of the current display.
        /// </returns>
        public float GetDisplayVerticalRefreshRate()
        {
            return this.GetDisplayVerticalRefreshRate(_displayHandle);
        }

        /// <summary>
        /// Gets the vertical refresh rate of the specified display.
        /// </summary>
        /// 
        /// <param name="displayHandle">
        /// A handle to the display.
        /// </param>
        /// 
        /// <returns>
        /// The vertical refresh rate of the specified display.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the display handle is invalid.
        /// </exception>
        public float GetDisplayVerticalRefreshRate(IntPtr displayHandle)
        {
            float refreshRate = 0.0f;

            PluginError error = zcuGetDisplayVerticalRefreshRate(displayHandle, out refreshRate);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return refreshRate;
        }

        /// <summary>
        /// Checks if the viewport's current display is connected via USB port. 
        /// </summary>
        /// 
        /// <remarks>
        /// Currently this only applies to zSpace displays. Additionally, 
        /// IsDisplayHardwarePresent() is expensive performance-wise and should not
        /// be invoked on a per-frame basis. 
        /// </remarks>
        /// 
        /// <returns>
        /// True if the current display is connected via USB port. False otherwise.
        /// </returns>
        public bool IsDisplayHardwarePresent()
        {
            return this.IsDisplayHardwarePresent(_displayHandle);
        }

        /// <summary>
        /// Checks if the specified display is connected via USB port. 
        /// </summary>
        /// 
        /// <remarks>
        /// Currently this only applies to zSpace displays. Additionally, 
        /// IsDisplayHardwarePresent() is expensive performance-wise and should not
        /// be invoked on a per-frame basis. 
        /// </remarks>
        /// 
        /// <param name="displayHandle">
        /// A handle to the display.
        /// </param>
        /// 
        /// <returns>
        /// True if the specified display is connected via USB port. False otherwise.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the display handle is invalid.
        /// </exception>
        public bool IsDisplayHardwarePresent(IntPtr displayHandle)
        {
            bool isHardwarePresent = false;

            PluginError error = zcuIsDisplayHardwarePresent(displayHandle, out isHardwarePresent);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return isHardwarePresent;
        }

        /// <summary>
        /// Performs a raycast against the viewport's current display.
        /// </summary>
        /// 
        /// <remarks>
        /// If the incoming pose is not in tracker-space, the pose will be
        /// converted to tracker-space before the collision query is performed.
        /// </remarks>
        /// 
        /// <param name="pose">
        /// A pose in any coordinate space.
        /// </param>
        /// 
        /// <returns>
        /// Data structure containing information about the intersection (i.e. hit, 
        /// screen position, etc.)
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidParameterException">
        /// Thrown if the incoming pose is null or invalid.
        /// </exception>
        public DisplayIntersectionInfo IntersectDisplay(Pose pose)
        {
            return this.IntersectDisplay(_displayHandle, pose);
        }

        /// <summary>
        /// Performs a raycast against the specified display.
        /// </summary>
        /// 
        /// <remarks>
        /// If the incoming pose is not in tracker-space, the pose will be
        /// converted to tracker-space before the collision query is performed.
        /// </remarks>
        /// 
        /// <param name="displayHandle">
        /// A handle to the display.
        /// </param>
        /// <param name="pose">
        /// A pose in any coordinate space.
        /// </param>
        /// 
        /// <returns>
        /// Data structure containing information about the intersection (i.e. hit, 
        /// screen position, etc.)
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the display handle is invalid.
        /// </exception>
        /// <exception cref="zSpace.Core.InvalidParameterException">
        /// Thrown if the incoming pose is invalid.
        /// </exception>
        public DisplayIntersectionInfo IntersectDisplay(IntPtr displayHandle, Pose pose)
        {
            if (pose == null)
            {
                throw new zSpace.Core.InvalidParameterException();
            }

            ZCDisplayIntersectionInfo intersectionInfo;

            // If the pose is not in tracker-space, convert it to tracker-space.
            if (pose.CoordinateSpace != CoordinateSpace.Tracker)
            {
                Matrix4x4 poseMatrix = this.TransformMatrix(pose.CoordinateSpace, CoordinateSpace.Tracker, pose.Matrix);
                pose = new Pose(poseMatrix, pose.Timestamp, CoordinateSpace.Tracker);
            }

            // Perform the collision query against the display.
            PluginError error = zcuIntersectDisplay(displayHandle, this.Convert(pose), out intersectionInfo);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }

            return this.Convert(intersectionInfo);
        }

        /// <summary>
        /// Creates a stereo viewport.
        /// </summary>
        /// 
        /// <remarks>
        /// The stereo viewport is abstract and not an actual window that is created and 
        /// registered through the OS. It manages a stereo frustum, which is responsible 
        /// for various stereoscopic 3D calculations such as calculating the view and 
        /// projection matrices for each eye.
        /// </remarks>
        /// 
        /// <returns>
        /// A handle to the newly created viewport.
        /// </returns>
        public IntPtr CreateViewport()
        {
            IntPtr viewportHandle = IntPtr.Zero;

            PluginError error = zcuCreateViewport(GlobalState.Instance.Context, out viewportHandle);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return viewportHandle;
        }

        /// <summary>
        /// Destroys the specified stereo viewport.
        /// </summary>
        /// 
        /// <remarks>
        /// Any viewports that have not been explicitly destroyed by calling 
        /// DestroyViewport() will get cleaned up on ApplicationQuit().
        /// </remarks>
        /// 
        /// <param name="viewportHandle">
        /// A handle to the viewport.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the viewport handle is invalid.
        /// </exception>
        public void DestroyViewport(IntPtr viewportHandle)
        {
            PluginError error = zcuDestroyViewport(viewportHandle);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
        }

        /// <summary>
        /// Sets the specified viewport's absolute virtual desktop coordinates 
        /// (top-left corner) in pixels.
        /// </summary>
        /// 
        /// <param name="viewportHandle">
        /// A handle to the viewport.
        /// </param>
        /// <param name="x">
        /// The x virtual desktop coordinate for the upper left corner of the viewport.
        /// </param>
        /// <param name="y">
        /// The y virtual desktop coordinate for the upper left corner of the viewport.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the viewport handle is invalid.
        /// </exception>
        public void SetViewportPosition(IntPtr viewportHandle, int x, int y)
        {
            PluginError error = zcuSetViewportPosition(viewportHandle, x, y);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
        }

        /// <summary>
        /// Gets the absolute virtual desktop coordinates (top-left corner) of the 
        /// primary viewport.
        /// </summary>
        /// 
        /// <returns>
        /// The absolute virtual desktop coordinates (top-left corner) of the primary 
        /// viewport.
        /// </returns>
        public Vector2 GetViewportPosition()
        {
            return this.GetViewportPosition(GlobalState.Instance.ViewportHandle);
        }

        /// <summary>
        /// Gets the absolute virtual desktop coordinates (top-left corner) of the 
        /// specified viewport.
        /// </summary>
        /// 
        /// <param name="viewportHandle">
        /// A handle to the viewport.
        /// </param>
        /// 
        /// <returns>
        /// The absolute virtual desktop coordinates (top-left corner) of the specified 
        /// viewport.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the viewport handle is invalid.
        /// </exception>
        public Vector2 GetViewportPosition(IntPtr viewportHandle)
        {
            int x = 0;
            int y = 0;

            PluginError error = zcuGetViewportPosition(viewportHandle, out x, out y);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return new Vector2(x, y);
        }

        /// <summary>
        /// Sets the specified viewport's size in pixels.
        /// </summary>
        /// 
        /// <param name="viewportHandle">
        /// A handle to the viewport.
        /// </param>
        /// <param name="width">
        /// The width of the viewport in pixels.
        /// </param>
        /// <param name="height">
        /// The height of the viewport in pixels.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the viewport handle is invalid.
        /// </exception>
        public void SetViewportSize(IntPtr viewportHandle, int width, int height)
        {
            PluginError error = zcuSetViewportSize(viewportHandle, width, height);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
        }

        /// <summary>
        /// Gets the size in pixels of the primary viewport.
        /// </summary>
        /// 
        /// <returns>
        /// The size in pixels of the primary viewport.
        /// </returns>
        public Vector2 GetViewportSize()
        {
            return this.GetViewportSize(GlobalState.Instance.ViewportHandle);
        }

        /// <summary>
        /// Gets the size in pixels of the specified viewport.
        /// </summary>
        /// 
        /// <param name="viewportHandle">
        /// A handle to the viewport.
        /// </param>
        /// 
        /// <returns>
        /// The size in pixels of the specified viewport.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the viewport handle is invalid.
        /// </exception>
        public Vector2 GetViewportSize(IntPtr viewportHandle)
        {
            int width = 0;
            int height = 0;

            PluginError error = zcuGetViewportSize(viewportHandle, out width, out height);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return new Vector2(width, height);
        }

        /// <summary>
        /// Gets the center position of the primary viewport in world space.
        /// </summary>
        /// 
        /// <returns>
        /// The center position of the primary viewport in world space.
        /// </returns>
        public Vector3 GetViewportWorldCenter()
        {
            this.PlayModeUpdate();

            return _viewportWorldCenter;
        }

        /// <summary>
        /// Gets the rotation of the primary viewport in world space.
        /// </summary>
        /// 
        /// <returns>
        /// The rotation of the primary viewport in world space.
        /// </returns>
        public Quaternion GetViewportWorldRotation()
        {
            this.PlayModeUpdate();

            return _viewportWorldRotation;
        }

        /// <summary>
        /// Gets the size in meters of the primary viewport in world space.
        /// </summary>
        /// 
        /// <remarks>
        /// The world space size in meters is equivalent to converting the viewport's
        /// size from pixels to meters and multiplying by the current viewer scale.
        /// </remarks>
        /// 
        /// <returns>
        /// The size in meters of the primary viewport in world space.
        /// </returns>
        public Vector2 GetViewportWorldSize()
        {
            this.PlayModeUpdate();

            return _viewportWorldSize;
        }

        /// <summary>
        /// Gets the coordinate space transformation from space a to b for the 
        /// primary viewport.
        /// </summary>
        /// 
        /// <param name="a">
        /// The source coordinate space.
        /// </param>
        /// <param name="b">
        /// The destination coordinate space.
        /// </param>
        /// 
        /// <returns>
        /// The coordinate space transformation from space a to b for the primary viewport.
        /// </returns>
        public Matrix4x4 GetCoordinateSpaceTransform(CoordinateSpace a, CoordinateSpace b)
        {
            this.PlayModeUpdate();

            return _coordinateSpaceMatrices[(int)a,(int)b];
        }

        /// <summary>
        /// Gets the coordinate space transformation from space a to b for the 
        /// specified viewport.
        /// </summary>
        /// 
        /// <param name="viewportHandle">
        /// A handle to the viewport.
        /// </param>
        /// <param name="a">
        /// The source coordinate space.
        /// </param>
        /// <param name="b">
        /// The destination coordinate space.
        /// </param>
        /// 
        /// <returns>
        /// The coordinate space transformation from space a to b for the specifed viewport.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the viewport handle is invalid.
        /// </exception>
        public Matrix4x4 GetCoordinateSpaceTransform(IntPtr viewportHandle, CoordinateSpace a, CoordinateSpace b)
        {
            if (a == b)
            {
                return Matrix4x4.identity;
            }

            ZSMatrix4 temp;

            PluginError error = zcuGetCoordinateSpaceTransform(viewportHandle, a, b, out temp);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }

            Matrix4x4 map = this.Convert(temp, true);
            if (this.CurrentCameraObject != null)
            {
                if (a == CoordinateSpace.World)
                {
                    map = map * this.CurrentCameraObject.transform.worldToLocalMatrix;
                }
                else if (b == CoordinateSpace.World)
                {
                    map = this.CurrentCameraObject.transform.localToWorldMatrix * map;
                }
            }
            
            return map;
        }

        /// <summary>
        /// Transforms a 4x4 transformation matrix from space a to b in the context 
        /// of the primary viewport.
        /// </summary>
        /// 
        /// <param name="a">
        /// The source coordinate space.
        /// </param>
        /// <param name="b">
        /// The destination coordinate space.
        /// </param>
        /// <param name="matrix">
        /// The input matrix to be transformed.
        /// </param>
        /// 
        /// <returns>
        /// The newly transformed matrix.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidParameterException">
        /// Thrown if the input matrix is invalid.
        /// </exception>
        public Matrix4x4 TransformMatrix(CoordinateSpace a, CoordinateSpace b, Matrix4x4 matrix)
        {
            if (a == b)
            {
                return matrix;
            }
            else
            {
                return this.GetCoordinateSpaceTransform(a, b) * matrix;
            }
        }

        /// <summary>
        /// Transforms a 4x4 transformation matrix from space a to b in the context 
        /// of the specified viewport.
        /// </summary>
        /// 
        /// <param name="viewportHandle">
        /// A handle to the viewport.
        /// </param>
        /// <param name="a">
        /// The source coordinate space.
        /// </param>
        /// <param name="b">
        /// The destination coordinate space.
        /// </param>
        /// <param name="matrix">
        /// The input matrix to be transformed.
        /// </param>
        /// 
        /// <returns>
        /// The newly transformed matrix.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the viewport handle is invalid.
        /// </exception>
        /// <exception cref="zSpace.Core.InvalidParameterException">
        /// Thrown if the input matrix is invalid.
        /// </exception>
        public Matrix4x4 TransformMatrix(IntPtr viewportHandle, CoordinateSpace a, CoordinateSpace b, Matrix4x4 matrix)
        {
            if (a == b)
            {
                return matrix;
            }
            else
            {
                return this.GetCoordinateSpaceTransform(viewportHandle, a, b) * matrix;
            }
        }

        /// <summary>
        /// Gets a handle to the frustum owned by the specified viewport.
        /// </summary>
        /// 
        /// <param name="viewportHandle">
        /// A handle to the viewport.
        /// </param>
        /// 
        /// <returns>
        /// The handle to the specified viewport’s frustum.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the viewport handle is invalid.
        /// </exception>
        public IntPtr GetFrustum(IntPtr viewportHandle)
        {
            IntPtr frustumHandle = IntPtr.Zero;

            PluginError error = zcuGetFrustum(viewportHandle, out frustumHandle);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }

            return frustumHandle;
        }

        /// <summary>
        /// Sets the attribute value for the primary frustum.
        /// </summary>
        /// 
        /// <param name="attribute">
        /// The attribute to be modified.
        /// </param>
        /// <param name="value">
        /// The desired floating point value to be applied to the attribute.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.InvalidAttributeException">
        /// Thrown if the attribute is invalid.
        /// </exception>
        public void SetFrustumAttribute(FrustumAttribute attribute, float value)
        {
            switch (attribute)
            {
                case FrustumAttribute.Ipd:
                    this.Ipd = value;
                    if (GlobalState.Instance.AutoStereoState != AutoStereoState.IdleStereo)
                    {
                        return;
                    }
                    break;

                case FrustumAttribute.ViewerScale:
                    this.ViewerScale = value;
                    if (Application.isPlaying)
                    {
                        this.SetFrustumAttribute(FrustumAttribute.NearClip, _nearClip);
                        this.SetFrustumAttribute(FrustumAttribute.FarClip, _farClip);
                    }
                    break;

                case FrustumAttribute.NearClip:
                    {
                        _nearClip = value;
                        value *= this.ViewerScale;

                        for (int i = 0; i < (int)Eye.NumEyes; ++i)
                        {
                            _stereoCameras[i].SetCameraNearClip(value);
                        }
                    }
                    break;

                case FrustumAttribute.FarClip:
                    {
                        _farClip = value;
                        value *= this.ViewerScale;
                        
                        for (int i = 0; i < (int)Eye.NumEyes; ++i)
                        {
                            _stereoCameras[i].SetCameraFarClip(value);
                        }
                    }
                    break;

                default:
                    break;
            }

            this.SetFrustumAttribute(GlobalState.Instance.FrustumHandle, attribute, value);
        }

        /// <summary>
        /// Sets the attribute value for the specified frustum.
        /// </summary>
        /// 
        /// <param name="frustumHandle">
        /// A handle to the frustum.
        /// </param>
        /// <param name="attribute">
        /// The attribute to be modified.
        /// </param>
        /// <param name="value">
        /// The desired floating point value to be applied to the attribute.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the frustum handle is invalid.
        /// </exception>
        /// <exception cref="zSpace.Core.InvalidAttributeException">
        /// Thrown if the attribute is invalid.
        /// </exception>
        public void SetFrustumAttribute(IntPtr frustumHandle, FrustumAttribute attribute, float value)
        {
            PluginError error = zcuSetFrustumAttributeF32(frustumHandle, attribute, value);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
        }

        /// <summary>
        /// Gets the attribute value for the primary frustum.
        /// </summary>
        /// 
        /// <param name="attribute">
        /// The attribute to be queried.
        /// </param>
        /// 
        /// <returns>
        /// The attribute’s current floating point value.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidAttributeException">
        /// Thrown if the attribute is invalid.
        /// </exception>
        public float GetFrustumAttributeFloat(FrustumAttribute attribute)
        {
            switch (attribute)
            {
                case FrustumAttribute.Ipd:
                    if (GlobalState.Instance.AutoStereoState == AutoStereoState.Animating)
                    {
                        return this.Ipd;
                    }
                    break;
                default:
                    break;
            }

            return this.GetFrustumAttributeFloat(GlobalState.Instance.FrustumHandle, attribute);
        }

        /// <summary>
        /// Gets the attribute value for the specified frustum.
        /// </summary>
        /// 
        /// <param name="frustumHandle">
        /// A handle to the frustum.
        /// </param>
        /// <param name="attribute">
        /// The attribute to be queried.
        /// </param>
        /// 
        /// <returns>
        /// The attribute’s current floating point value.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the frustum handle is invalid.
        /// </exception>
        /// <exception cref="zSpace.Core.InvalidAttributeException">
        /// Thrown if the attribute is invalid.
        /// </exception>
        public float GetFrustumAttributeFloat(IntPtr frustumHandle, FrustumAttribute attribute)
        {
            float value = 0.0f;

            PluginError error = zcuGetFrustumAttributeF32(frustumHandle, attribute, out value);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return value;
        }

        /// <summary>
        /// Sets the attribute value for the primary frustum.
        /// </summary>
        /// 
        /// <param name="attribute">
        /// The attribute to be modified.
        /// </param>
        /// <param name="value">
        /// The desired boolean value to be applied to the attribute.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.InvalidAttributeException">
        /// Thrown if the attribute is invalid.
        /// </exception>
        public void SetFrustumAttribute(FrustumAttribute attribute, bool value)
        {
            this.SetFrustumAttribute(GlobalState.Instance.FrustumHandle, attribute, value);
        }

        /// <summary>
        /// Sets the attribute value for the specified frustum.
        /// </summary>
        /// 
        /// <param name="frustumHandle">
        /// A handle to the frustum.
        /// </param>
        /// <param name="attribute">
        /// The attribute to be modified.
        /// </param>
        /// <param name="value">
        /// The desired boolean value to be applied to the attribute.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the frustum handle is invalid.
        /// </exception>
        /// <exception cref="zSpace.Core.InvalidAttributeException">
        /// Thrown if the attribute is invalid.
        /// </exception>
        public void SetFrustumAttribute(IntPtr frustumHandle, FrustumAttribute attribute, bool value)
        {
            PluginError error = zcuSetFrustumAttributeB(frustumHandle, attribute, value);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
        }

        /// <summary>
        /// Gets the attribute value for the primary frustum.
        /// </summary>
        /// 
        /// <param name="attribute">
        /// The attribute to be queried.
        /// </param>
        /// 
        /// <returns>
        /// The attribute’s current boolean value.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidAttributeException">
        /// Thrown if the attribute is invalid.
        /// </exception>
        public bool GetFrustumAttributeBool(FrustumAttribute attribute)
        {
            return this.GetFrustumAttributeBool(GlobalState.Instance.FrustumHandle, attribute);
        }

        /// <summary>
        /// Gets the attribute value for the specified frustum.
        /// </summary>
        /// 
        /// <param name="frustumHandle">
        /// A handle to the frustum.
        /// </param>
        /// <param name="attribute">
        /// The attribute to be queried.
        /// </param>
        /// 
        /// <returns>
        /// The attribute’s current boolean value.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the frustum handle is invalid.
        /// </exception>
        /// <exception cref="zSpace.Core.InvalidAttributeException">
        /// Thrown if the attribute is invalid.
        /// </exception>
        public bool GetFrustumAttributeBool(IntPtr frustumHandle, FrustumAttribute attribute)
        {
            bool value = false;

            PluginError error = zcuGetFrustumAttributeB(frustumHandle, attribute, out value);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return value;
        }

        /// <summary>
        /// Sets the portal mode for the primary frustum. 
        /// </summary>
        /// 
        /// <remarks>
        /// In portal mode, the scene is fixed relative to the physical world, not 
        /// the viewport. Refer to PortalMode for details on portal modes available.
        /// </remarks>
        /// 
        /// <param name="portalModeFlags">
        /// A bitmask for the portal mode flags.
        /// </param>
        public void SetFrustumPortalMode(int portalModeFlags)
        {
            this.SetFrustumPortalMode(GlobalState.Instance.FrustumHandle, portalModeFlags);
        }

        /// <summary>
        /// Sets the portal mode for the specified frustum. 
        /// </summary>
        /// 
        /// <remarks>
        /// In portal mode, the scene is fixed relative to the physical world, not 
        /// the viewport. Refer to PortalMode for details on portal modes available.
        /// </remarks>
        /// 
        /// <param name="frustumHandle">
        /// A handle to the frustum.
        /// </param>
        /// <param name="portalModeFlags">
        /// A bitmask for the portal mode flags.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the frustum handle is invalid.
        /// </exception>
        public void SetFrustumPortalMode(IntPtr frustumHandle, int portalModeFlags)
        {
            PluginError error = zcuSetFrustumPortalMode(frustumHandle, portalModeFlags);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
        }

        /// <summary>
        /// Gets the portal mode for the primary frustum.
        /// </summary>
        /// 
        /// <returns>
        /// A bitmask representing the primary frustum’s current portal mode settings.
        /// </returns>
        public int GetFrustumPortalMode()
        {
            return this.GetFrustumPortalMode(GlobalState.Instance.FrustumHandle);
        }

        /// <summary>
        /// Gets the portal mode for the specified frustum.
        /// </summary>
        /// 
        /// <param name="frustumHandle">
        /// A handle to the frustum.
        /// </param>
        /// 
        /// <returns>
        /// A bitmask representing the specified frustum’s current portal mode settings.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the frustum handle is invalid.
        /// </exception>
        public int GetFrustumPortalMode(IntPtr frustumHandle)
        {
            int portalModeFlags = 0;

            PluginError error = zcuGetFrustumPortalMode(frustumHandle, out portalModeFlags);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return portalModeFlags;
        }

        /// <summary>
        /// Sets the camera offset for the primary frustum.
        /// </summary>
        /// 
        /// <param name="cameraOffset">
        /// The desired camera offset in meters.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.InvalidParameterException">
        /// Thrown if the camera offset is invalid.
        /// </exception>
        public void SetFrustumCameraOffset(Vector3 cameraOffset)
        {
            this.SetFrustumCameraOffset(GlobalState.Instance.FrustumHandle, cameraOffset);
        }

        /// <summary>
        /// Sets the camera offset for the specified frustum.
        /// </summary>
        /// 
        /// <param name="frustumHandle">
        /// A handle to the frustum.
        /// </param>
        /// <param name="cameraOffset">
        /// The desired camera offset in meters.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the frustum handle is invalid.
        /// </exception>
        /// <exception cref="zSpace.Core.InvalidParameterException">
        /// Thrown if the camera offset is invalid.
        /// </exception>
        public void SetFrustumCameraOffset(IntPtr frustumHandle, Vector3 cameraOffset)
        {
            PluginError error = zcuSetFrustumCameraOffset(frustumHandle, this.Convert(cameraOffset, true));
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
        }

        /// <summary>
        /// Gets the camera offset of the primary frustum.
        /// </summary>
        /// 
        /// <returns>
        /// The camera offset in meters.
        /// </returns>
        public Vector3 GetFrustumCameraOffset()
        {
            return this.GetFrustumCameraOffset(GlobalState.Instance.FrustumHandle);
        }

        /// <summary>
        /// Gets the camera offset of the specified frustum.
        /// </summary>
        /// 
        /// <param name="frustumHandle">
        /// A handle to the frustum.
        /// </param>
        /// 
        /// <returns>
        /// The camera offset in meters.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the frustum handle is invalid.
        /// </exception>
        public Vector3 GetFrustumCameraOffset(IntPtr frustumHandle)
        {
            ZSVector3 cameraOffset;

            PluginError error = zcuGetFrustumCameraOffset(frustumHandle, out cameraOffset);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return this.Convert(cameraOffset, true);
        }

        /// <summary>
        /// Sets the head pose of the specified frustum.
        /// </summary>
        /// 
        /// <remarks>
        /// If the head pose is not in tracker-space, it will be transformed
        /// to tracker-space.
        /// </remarks>
        /// 
        /// <param name="frustumHandle">
        /// A handle to the frustum.
        /// </param>
        /// <param name="headPose">
        /// The desired head pose.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the frustum handle is invalid.
        /// </exception>
        /// <exception cref="zSpace.Core.InvalidParameterException">
        /// Thrown if the head pose is invalid.
        /// </exception>
        public void SetFrustumHeadPose(IntPtr frustumHandle, Pose headPose)
        {
            if (headPose == null)
            {
                throw new zSpace.Core.InvalidParameterException();
            }

            // If the head pose is not in tracker-space, convert it to tracker-space.
            if (headPose.CoordinateSpace != CoordinateSpace.Tracker)
            {
                Matrix4x4 poseMatrix = this.TransformMatrix(headPose.CoordinateSpace, CoordinateSpace.Tracker, headPose.Matrix);
                headPose = new Pose(poseMatrix, headPose.Timestamp, CoordinateSpace.Tracker);
            }

            PluginError error = zcuSetFrustumHeadPose(frustumHandle, this.Convert(headPose));
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
        }

        /// <summary>
        /// Gets the current head pose of the primary frustum.
        /// </summary>
        /// 
        /// <returns>
        /// The current head pose in tracker-space.
        /// </returns>
        public Pose GetFrustumHeadPose()
        {
            return this.GetFrustumHeadPose(GlobalState.Instance.FrustumHandle);
        }

        /// <summary>
        /// Gets the current head pose of the specified frustum.
        /// </summary>
        /// 
        /// <param name="frustumHandle">
        /// A handle to the frustum.
        /// </param>
        /// 
        /// <returns>
        /// The current head pose in tracker-space.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the frustum handle is invalid.
        /// </exception>
        public Pose GetFrustumHeadPose(IntPtr frustumHandle)
        {
            ZCTrackerPose headPose;

            PluginError error = zcuGetFrustumHeadPose(frustumHandle, out headPose);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return this.Convert(headPose);
        }

        /// <summary>
        /// Gets the view matrix for the primary frustum.
        /// </summary>
        /// 
        /// <param name="eye">
        /// The eye to query.
        /// </param>
        /// 
        /// <returns>
        /// The view matrix for the specified eye.
        /// </returns>
        public Matrix4x4 GetFrustumViewMatrix(Eye eye)
        {
            this.PlayModeUpdate();

            return _viewMatrices[(int)eye];
        }

        /// <summary>
        /// Gets the view matrix for the specified frustum.
        /// </summary>
        /// 
        /// <param name="frustumHandle">
        /// A handle to the frustum.
        /// </param>
        /// <param name="eye">
        /// The eye to query.
        /// </param>
        /// 
        /// <returns>
        /// The view matrix for the specified eye.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the frustum handle is invalid.
        /// </exception>
        public Matrix4x4 GetFrustumViewMatrix(IntPtr frustumHandle, Eye eye)
        {
            this.PlayModeUpdate();

            ZSMatrix4 viewMatrix;

            PluginError error = zcuGetFrustumViewMatrix(frustumHandle, eye, out viewMatrix);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }

            return this.Convert(viewMatrix, true);
        }

        /// <summary>
        /// Gets the projection matrix for the primary frustum.
        /// </summary>
        /// 
        /// <remarks>
        /// The projection matrix is right-handed because Unity cameras expect
        /// projection matrices to be right-handed.
        /// </remarks>
        /// 
        /// <param name="eye">
        /// The eye to query.
        /// </param>
        /// 
        /// <returns>
        /// The projection matrix for the specified eye.
        /// </returns>
        public Matrix4x4 GetFrustumProjectionMatrix(Eye eye)
        {
            this.PlayModeUpdate();

            return _projectionMatrices[(int)eye];
        }

        /// <summary>
        /// Gets projection matrix for the specified frustum.
        /// </summary>
        /// 
        /// <remarks>
        /// The projection matrix is right-handed because Unity cameras expect
        /// projection matrices to be right-handed.
        /// </remarks>
        /// 
        /// <param name="frustumHandle">
        /// A handle to the frustum.
        /// </param>
        /// <param name="eye">
        /// The eye to query.
        /// </param>
        /// 
        /// <returns>
        /// The projection matrix for the specified eye.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the frustum handle is invalid.
        /// </exception>
        public Matrix4x4 GetFrustumProjectionMatrix(IntPtr frustumHandle, Eye eye)
        {
            this.PlayModeUpdate();

            ZSMatrix4 projectionMatrix;

            PluginError error = zcuGetFrustumProjectionMatrix(frustumHandle, eye, out projectionMatrix);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }

            return this.Convert(projectionMatrix, false);
        }

        /// <summary>
        /// Gets the bounds for the primary frustum.
        /// </summary>
        /// 
        /// <param name="eye">
        /// The eye to query.
        /// </param>
        /// 
        /// <returns>
        /// The frustum bounds for the specified eye.
        /// </returns>
        public FrustumBounds GetFrustumBounds(Eye eye)
        {
            return this.GetFrustumBounds(GlobalState.Instance.FrustumHandle, eye);
        }

        /// <summary>
        /// Gets the bounds for the specified frustum.
        /// </summary>
        /// 
        /// <param name="frustumHandle">
        /// A handle to the frustum.
        /// </param>
        /// <param name="eye">
        /// The eye to query.
        /// </param>
        /// 
        /// <returns>
        /// The frustum bounds for the specified eye.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the frustum handle is invalid.
        /// </exception>
        public FrustumBounds GetFrustumBounds(IntPtr frustumHandle, Eye eye)
        {
            this.PlayModeUpdate();

            ZCFrustumBounds bounds;

            PluginError error = zcuGetFrustumBounds(frustumHandle, eye, out bounds);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }

            return this.Convert(bounds);
        }

        /// <summary>
        /// Gets the eye position for the primary frustum.
        /// </summary>
        /// 
        /// <param name="eye">
        /// The eye to query.
        /// </param>
        /// <param name="coordinateSpace">
        /// THe coordinate space in which to return the eye position.
        /// </param>
        /// 
        /// <returns>
        /// The eye's position in meters.
        /// </returns>
        public Vector3 GetFrustumEyePosition(Eye eye, CoordinateSpace coordinateSpace)
        {
            return this.GetFrustumEyePosition(GlobalState.Instance.FrustumHandle, eye, coordinateSpace);
        }

        /// <summary>
        /// Gets the eye position for the specified frustum.
        /// </summary>
        /// 
        /// <param name="frustumHandle">
        /// A handle t the frustum.
        /// </param>
        /// <param name="eye">
        /// The eye to query.
        /// </param>
        /// <param name="coordinateSpace">
        /// THe coordinate space in which to return the eye position.
        /// </param>
        /// 
        /// <returns>
        /// The eye's position in meters.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the frustum handle is invalid.
        /// </exception>
        public Vector3 GetFrustumEyePosition(IntPtr frustumHandle, Eye eye, CoordinateSpace coordinateSpace)
        {
            this.PlayModeUpdate();

            ZSVector3 temp;
            CoordinateSpace tempSpace = (coordinateSpace == CoordinateSpace.World) ? CoordinateSpace.Camera : coordinateSpace;

            PluginError error = zcuGetFrustumEyePosition(frustumHandle, eye, tempSpace, out temp);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }

            Vector3 eyePosition = this.Convert(temp, true);
            if (coordinateSpace == CoordinateSpace.World)
            {
                Matrix4x4 cameraToWorld = this.GetCoordinateSpaceTransform(CoordinateSpace.Camera, CoordinateSpace.World);
                eyePosition = cameraToWorld * eyePosition;
            }

            return eyePosition;
        }

        /// <summary>
        /// Gets the camera-space axis-aligned bounding box of the coupled comfort
        /// zone for the primary frustum.
        /// </summary>
        /// 
        /// <returns>
        /// The bounding box containing the coupled comfort zone's minimum and 
        /// maximum extents in meters.
        /// </returns>
        public Bounds GetFrustumCoupledBoundingBox()
        {
            return this.GetFrustumCoupledBoundingBox(GlobalState.Instance.FrustumHandle);
        }

        /// <summary>
        /// Gets the camera-space axis-aligned bounding box of the coupled comfort
        /// zone for the specified frustum.
        /// </summary>
        /// 
        /// <param name="frustumHandle">
        /// A handle to the frustum.
        /// </param>
        /// 
        /// <returns>
        /// The bounding box containing the coupled comfort zone's minimum and 
        /// maximum extents in meters.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the frustum handle is invalid.
        /// </exception>
        public Bounds GetFrustumCoupledBoundingBox(IntPtr frustumHandle)
        {
            ZCBoundingBox boundingBox;

            PluginError error = zcuGetFrustumCoupledBoundingBox(frustumHandle, out boundingBox);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }

            return this.Convert(boundingBox);
        }

        /// <summary>
        /// Gets the number of available tracker devices.
        /// </summary>
        /// 
        /// <returns>
        /// The number of available tracker devices.
        /// </returns>
        public int GetNumTrackerDevices()
        {
            int numDevices = 0;

            PluginError error = zcuGetNumTrackerDevices(GlobalState.Instance.Context, out numDevices);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }

            return numDevices;
        }

        /// <summary>
        /// Gets a handle to a tracker device for a specified index.
        /// </summary>
        /// 
        /// <param name="index">
        /// The index of the tracker device.
        /// </param>
        /// 
        /// <returns>
        /// A handle to the tracker device.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.DeviceNotFoundException">
        /// Thrown if the tracker device could not be found.
        /// </exception>
        public IntPtr GetTrackerDevice(int index)
        {
            IntPtr deviceHandle = IntPtr.Zero;

            PluginError error = zcuGetTrackerDevice(GlobalState.Instance.Context, index, out deviceHandle);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return deviceHandle;
        }

        /// <summary>
        /// Gets a handle to a tracker device for a specified device name.
        /// </summary>
        /// 
        /// <param name="deviceName">
        /// The name of the tracker device.
        /// </param>
        /// 
        /// <returns>
        /// A handle to the tracker device.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.DeviceNotFoundException">
        /// Thrown if the tracker device could not be found.
        /// </exception>
        public IntPtr GetTrackerDevice(string deviceName)
        {
            IntPtr deviceHandle;

            PluginError error = zcuGetTrackerDeviceByName(GlobalState.Instance.Context, deviceName, out deviceHandle);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return deviceHandle;
        }

        /// <summary>
        /// Sets whether the specified tracker device is enabled.
        /// </summary>
        /// 
        /// <param name="deviceHandle">
        /// A handle to the tracker device.
        /// </param>
        /// <param name="isEnabled">
        /// True to enable the tracker device. False otherwise.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the tracker device handle is invalid.
        /// </exception>
        public void SetTrackerDeviceEnabled(IntPtr deviceHandle, bool isEnabled)
        {
            PluginError error = zcuSetTrackerDeviceEnabled(deviceHandle, isEnabled);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
        }

        /// <summary>
        /// Checks whether the specfied tracker device is enabled.
        /// </summary>
        /// 
        /// <param name="deviceHandle">
        /// A handle to the tracker device.
        /// </param>
        /// 
        /// <returns>
        /// True if the tracker device is enabled. False otherwise.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the tracker device handle is invalid.
        /// </exception>
        public bool IsTrackerDeviceEnabled(IntPtr deviceHandle)
        {
            bool isEnabled = false;

            PluginError error = zcuIsTrackerDeviceEnabled(deviceHandle, out isEnabled);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return isEnabled;
        }

        /// <summary>
        /// Gets the name of the specified tracker device.
        /// </summary>
        /// 
        /// <param name="deviceHandle">
        /// A handle to the tracker device.
        /// </param>
        /// 
        /// <returns>
        /// The name of the tracker device.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the tracker device handle is invalid.
        /// </exception>
        public string GetTrackerDeviceName(IntPtr deviceHandle)
        {
            // Get the size of the device's name.
            int deviceNameSize = 500;
            zcuGetTrackerDeviceNameSize(deviceHandle, out deviceNameSize);

            // Get the name of the device.
            StringBuilder deviceName = new StringBuilder(deviceNameSize);
            PluginError error = zcuGetTrackerDeviceName(deviceHandle, deviceName, deviceNameSize);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return deviceName.ToString();
        }

        /// <summary>
        /// Gets the number of targets owned by the specified tracker device.
        /// </summary>
        /// 
        /// <param name="deviceHandle">
        /// A handle to the tracker device.
        /// </param>
        /// 
        /// <returns>
        /// The number of targets for the specified tracker device.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the tracker device handle is invalid.
        /// </exception>
        public int GetNumTargets(IntPtr deviceHandle)
        {
            int numTargets = 0;
            
            PluginError error = zcuGetNumTargets(deviceHandle, out numTargets);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return numTargets;
        }

        /// <summary>
        /// Gets the number of targets of a specified type.
        /// </summary>
        /// 
        /// <param name="targetType">
        /// The type of tracker target to query.
        /// </param>
        /// 
        /// <returns>
        /// The number of targets of the specified type.
        /// </returns>
        public int GetNumTargets(TargetType targetType)
        {
            int numTargets = 0;

            PluginError error = zcuGetNumTargetsByType(GlobalState.Instance.Context, targetType, out numTargets);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return numTargets;
        }

        /// <summary>
        /// Gets a handle to a target based on a specified tracker device and index.
        /// </summary>
        /// 
        /// <param name="deviceHandle">
        /// A handle to the tracker device.
        /// </param>
        /// <param name="index">
        /// Index into the underlying list of targets owned by the specified tracker device.
        /// </param>
        /// 
        /// <returns>
        /// A handle to the tracker target.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the tracker device handle is invalid.
        /// </exception>
        /// <exception cref="zSpace.Core.TargetNotFoundException">
        /// Thrown if the target was not found.
        /// </exception>
        public IntPtr GetTarget(IntPtr deviceHandle, int index)
        {
            IntPtr targetHandle = IntPtr.Zero;

            PluginError error = zcuGetTarget(deviceHandle, index, out targetHandle);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return targetHandle;
        }

        /// <summary>
        /// Gets the handle to a tracker target by name.
        /// </summary>
        /// 
        /// <param name="deviceHandle">
        /// A handle to the tracker device.
        /// </param>
        /// <param name="targetName">
        /// Name of the target to query.
        /// </param>
        /// 
        /// <returns>
        /// A handle to the target.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the tracker device handle is invalid.
        /// </exception>
        /// <exception cref="zSpace.Core.TargetNotFoundException">
        /// Thrown if the target was not found.
        /// </exception>
        public IntPtr GetTarget(IntPtr deviceHandle, string targetName)
        {
            IntPtr targetHandle = IntPtr.Zero;

            PluginError error = zcuGetTargetByName(deviceHandle, targetName, out targetHandle);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return targetHandle;
        }

        /// <summary>
        /// Gets the handle to a tracker target based on a specified type and index.
        /// </summary>
        /// 
        /// <param name="targetType">
        /// The type of target to query.
        /// </param>
        /// <param name="index">
        /// Index for the target into the underlying list of all targets of the specified type.
        /// </param>
        /// 
        /// <returns>
        /// A handle to the target.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.TargetNotFoundException">
        /// Thrown if the target was not found.
        /// </exception>
        public IntPtr GetTarget(TargetType targetType, int index)
        {
            IntPtr targetHandle = IntPtr.Zero;

            PluginError error = zcuGetTargetByType(GlobalState.Instance.Context, targetType, index, out targetHandle);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return targetHandle;
        }

        /// <summary>
        /// Gets the name of the default target.
        /// </summary>
        /// 
        /// <param name="targetType">
        /// The type of target.
        /// </param>
        /// 
        /// <returns>
        /// The name of the target.
        /// </returns>
        public string GetTargetName(TargetType targetType)
        {
            return this.GetTargetName(GlobalState.Instance.TargetHandles[(int)targetType]);
        }

        /// <summary>
        /// Gets the name of the specified target.
        /// </summary>
        /// 
        /// <param name="targetHandle">
        /// A handle to the target.
        /// </param>
        /// 
        /// <returns>
        /// The name of the target.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the target handle is invalid.
        /// </exception>
        public string GetTargetName(IntPtr targetHandle)
        {
            // Get the size of the target's name.
            int targetNameSize = 500;
            zcuGetTargetNameSize(targetHandle, out targetNameSize);

            // Get the name of the target.
            StringBuilder targetName = new StringBuilder(targetNameSize);
            PluginError error = zcuGetTargetName(targetHandle, targetName, targetNameSize);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return targetName.ToString();
        }
        
        /// <summary>
        /// Sets whether the default target is enabled.
        /// </summary>
        /// 
        /// <param name="targetType">
        /// The type of target.
        /// </param>
        /// <param name="isEnabled">
        /// True to enable. False otherwise.
        /// </param>
        public void SetTargetEnabled(TargetType targetType, bool isEnabled)
        {
            this.SetTargetEnabled(GlobalState.Instance.TargetHandles[(int)targetType], isEnabled);
        }

        /// <summary>
        /// Sets whether the specified target is enabled.
        /// </summary>
        /// 
        /// <param name="targetHandle">
        /// A handle to the target.
        /// </param>
        /// <param name="isEnabled">
        /// True to enable. False otherwise.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the target handle is invalid.
        /// </exception>
        public void SetTargetEnabled(IntPtr targetHandle, bool isEnabled)
        {
            PluginError error = zcuSetTargetEnabled(targetHandle, isEnabled);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
        }

        /// <summary>
        /// Checks whether the default target is enabled.
        /// </summary>
        /// 
        /// <param name="targetType">
        /// The type of target.
        /// </param>
        /// 
        /// <returns>
        /// True if the target is enabled. False otherwise.
        /// </returns>
        public bool IsTargetEnabled(TargetType targetType)
        {
            return this.IsTargetEnabled(GlobalState.Instance.TargetHandles[(int)targetType]);
        }

        /// <summary>
        /// Checks whether the specified target is enabled.
        /// </summary>
        /// 
        /// <param name="targetHandle">
        /// A handle to the target.
        /// </param>
        /// 
        /// <returns>
        /// True if the target is enabled. False otherwise.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the target handle is invalid.
        /// </exception>
        public bool IsTargetEnabled(IntPtr targetHandle)
        {
            bool isEnabled = false;

            PluginError error = zcuIsTargetEnabled(targetHandle, out isEnabled);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }

            return isEnabled;
        }

        /// <summary>
        /// Checks if the default target is visible.
        /// </summary>
        /// 
        /// <param name="targetType">
        /// The type of target.
        /// </param>
        /// 
        /// <returns>
        /// True if the target is visible. False otherwise.
        /// </returns>
        public bool IsTargetVisible(TargetType targetType)
        {
            return this.IsTargetVisible(GlobalState.Instance.TargetHandles[(int)targetType]);
        }

        /// <summary>
        /// Checks if the specified target is visible.
        /// </summary>
        /// 
        /// <param name="targetHandle">
        /// A handle to the target.
        /// </param>
        /// 
        /// <returns>
        /// True if the target is visible. False otherwise.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the target handle is invalid.
        /// </exception>
        public bool IsTargetVisible(IntPtr targetHandle)
        {
            bool isVisible = false;

            PluginError error = zcuIsTargetVisible(targetHandle, out isVisible);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return isVisible;
        }

        /// <summary>
        /// Gets the last valid cached pose of the default target.
        /// </summary>
        /// 
        /// <param name="targetType">
        /// The type of target.
        /// </param>
        /// <param name="coordinateSpace">
        /// The coordinate space to return the pose in.
        /// </param>
        /// 
        /// <returns>
        /// The last valid cached pose in the specified coordinate space.
        /// </returns>
        public Pose GetTargetPose(TargetType targetType, CoordinateSpace coordinateSpace)
        {
            this.PlayModeUpdate();

            return _targetPoses[(int)targetType,(int)coordinateSpace];
        }

        /// <summary>
        /// Get the last valid pose of the specified target.
        /// </summary>
        /// 
        /// <remarks>
        /// Only tracker-space poses are cached. Querying for poses in other
        /// coordinate spaces will require them to be recalculated for their
        /// respective coordinate space (additional matrix multiply).
        /// </remarks>
        /// 
        /// <param name="targetHandle">
        /// A handle to the target.
        /// </param>
        /// 
        /// <returns>
        /// The last valid pose in the specified coordinate space.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the target handle is invalid.
        /// </exception>
        public Pose GetTargetPose(IntPtr targetHandle, CoordinateSpace coordinateSpace)
        {
            this.PlayModeUpdate();

            ZCTrackerPose temp;
            
            // Retrieve the target pose.
            PluginError error = zcuGetTargetPose(targetHandle, out temp);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }

            // Convert the pose to the specified coordinate space.
            Pose pose = this.Convert(temp);
            if (coordinateSpace != CoordinateSpace.Tracker)
            {
                Matrix4x4 poseMatrix = this.GetCoordinateSpaceTransform(CoordinateSpace.Tracker, coordinateSpace) * pose.Matrix;
                pose = new Pose(poseMatrix, pose.Timestamp, coordinateSpace);
            }

            return pose;
        }

        /// <summary>
        /// Sets whether pose buffering is enabled for the default target. 
        /// </summary>
        /// 
        /// <remarks>
        /// Each buffered pose is timestamped so that the target's movement can be 
        /// queried over a specified time interval.
        /// </remarks>
        /// 
        /// <param name="targetType">
        /// The type of target.
        /// </param>
        /// <param name="isPoseBufferingEnabled">
        /// True to enable pose buffering. False otherwise.
        /// </param>
        public void SetTargetPoseBufferingEnabled(TargetType targetType, bool isPoseBufferingEnabled)
        {
            this.SetTargetPoseBufferingEnabled(GlobalState.Instance.TargetHandles[(int)targetType], isPoseBufferingEnabled);
        }

        /// <summary>
        /// Sets whether pose buffering is enabled for the specified target. 
        /// </summary>
        /// 
        /// <remarks>
        /// Each buffered pose is timestamped so that the target's movement can be 
        /// queried over a specified time interval.
        /// </remarks>
        /// 
        /// <param name="targetHandle">
        /// A handle to the target.
        /// </param>
        /// <param name="isPoseBufferingEnabled">
        /// True to enable pose buffering. False otherwise.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the target handle is invalid.
        /// </exception>
        public void SetTargetPoseBufferingEnabled(IntPtr targetHandle, bool isPoseBufferingEnabled)
        {
            PluginError error = zcuSetTargetPoseBufferingEnabled(targetHandle, isPoseBufferingEnabled);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
        }

        /// <summary>
        /// Checks whether pose buffering is enabled for the default target.
        /// </summary>
        /// 
        /// <param name="targetType">
        /// The type of target.
        /// </param>
        /// 
        /// <returns>
        /// True if pose buffering is enabled. False otherwise.
        /// </returns>
        public bool IsTargetPoseBufferingEnabled(TargetType targetType)
        {
            return this.IsTargetPoseBufferingEnabled(GlobalState.Instance.TargetHandles[(int)targetType]);
        }

        /// <summary>
        /// Checks whether pose buffering is enabled for the specified target.
        /// </summary>
        /// 
        /// <param name="targetHandle">
        /// A handle to the target.
        /// </param>
        /// 
        /// <returns>
        /// True if pose buffering is enabled. False otherwise.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the target handle is invalid.
        /// </exception>
        public bool IsTargetPoseBufferingEnabled(IntPtr targetHandle)
        {
            bool isPoseBufferingEnabled = false;

            PluginError error = zcuIsTargetPoseBufferingEnabled(targetHandle, out isPoseBufferingEnabled);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return isPoseBufferingEnabled;
        }

        /// <summary>
        /// Gets the pose buffer for the default target over a specified time range.
        /// </summary>
        /// 
        /// <remarks>
        /// All poses in the buffer are in tracker-space.
        /// </remarks>
        /// 
        /// <param name="targetType">
        /// The type of target.
        /// </param>
        /// <param name="minDelta">
        /// A time relative to the current time, in seconds, corresponding to 
        /// the minimum of the time period’s range.
        /// </param>
        /// <param name="maxDelta">
        /// A time relative to the current time, in seconds, corresponding to 
        /// the maximum of the time period’s range.
        /// </param>
        /// <param name="maxNumPoses">
        /// The maximum number of poses to retrieve.
        /// </param>
        /// 
        /// <returns>
        /// The pose buffer for the specified time period.
        /// </returns>
        public IList<Pose> GetTargetPoseBuffer(TargetType targetType, float minDelta, float maxDelta, int maxNumPoses)
        {
            return this.GetTargetPoseBuffer(GlobalState.Instance.TargetHandles[(int)targetType], minDelta, maxDelta, maxNumPoses);
        }

        /// <summary>
        /// Gets the pose buffer for the specified target over a specified time range.
        /// </summary>
        /// 
        /// <remarks>
        /// All poses in the buffer are in tracker-space.
        /// </remarks>
        /// 
        /// <param name="targetHandle">
        /// A handle to the target.
        /// </param>
        /// <param name="minDelta">
        /// A time relative to the current time, in seconds, corresponding to 
        /// the minimum of the time period’s range.
        /// </param>
        /// <param name="maxDelta">
        /// A time relative to the current time, in seconds, corresponding to 
        /// the maximum of the time period’s range.
        /// </param>
        /// <param name="maxNumPoses">
        /// The maximum number of poses to retrieve.
        /// </param>
        /// 
        /// <returns>
        /// The pose buffer for the specified time period.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the target handle is invalid.
        /// </exception>
        public IList<Pose> GetTargetPoseBuffer(IntPtr targetHandle, float minDelta, float maxDelta, int maxNumPoses)
        {
            this.PlayModeUpdate();

            ZCTrackerPose[] poseBuffer = new ZCTrackerPose[maxNumPoses];

            PluginError error = zcuGetTargetPoseBuffer(targetHandle, minDelta, maxDelta, poseBuffer, out maxNumPoses);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }

            List<Pose> poses = new List<Pose>(maxNumPoses);
            for (int i = 0; i < maxNumPoses; ++i)
            {
                poses.Add(this.Convert(poseBuffer[i]));
            }

            return poses;
        }

        /// <summary>
        /// Resizes the internal pose buffer of the default target.
        /// </summary>
        /// 
        /// <remarks>
        /// Resizing a targets pose buffer will clear all existing buffered poses.
        /// </remarks>
        /// 
        /// <param name="targetType">
        /// The type of target.
        /// </param>
        /// <param name="capacity">
        /// The new capacity of the pose buffer.
        /// </param>
        public void ResizeTargetPoseBuffer(TargetType targetType, int capacity)
        {
            this.ResizeTargetPoseBuffer(GlobalState.Instance.TargetHandles[(int)targetType], capacity);
        }

        /// <summary>
        /// Resizes the internal pose buffer of the specified target.
        /// </summary>
        /// 
        /// <remarks>
        /// Resizing a targets pose buffer will clear all existing buffered poses.
        /// </remarks>
        /// 
        /// <param name="targetHandle">
        /// A handle to the target.
        /// </param>
        /// <param name="capacity">
        /// The new capacity of the pose buffer.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the target handle is invalid.
        /// </exception>
        public void ResizeTargetPoseBuffer(IntPtr targetHandle, int capacity)
        {
            PluginError error = zcuResizeTargetPoseBuffer(targetHandle, capacity);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
        }

        /// <summary>
        /// Gets the capacity of the default target's pose buffer.
        /// </summary>
        /// 
        /// <param name="targetType">
        /// The type of target.
        /// </param>
        /// 
        /// <returns>
        /// The capacity of the pose buffer.
        /// </returns>
        public int GetTargetPoseBufferCapacity(TargetType targetType)
        {
            return this.GetTargetPoseBufferCapacity(GlobalState.Instance.TargetHandles[(int)targetType]);
        }

        /// <summary>
        /// Gets the capacity of the specified target's pose buffer.
        /// </summary>
        /// 
        /// <param name="targetHandle">
        /// A handle to the target.
        /// </param>
        /// 
        /// <returns>
        /// The capacity of the pose buffer.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the target handle is invalid.
        /// </exception>
        public int GetTargetPoseBufferCapacity(IntPtr targetHandle)
        {
            int capacity = 0;

            PluginError error = zcuGetTargetPoseBufferCapacity(targetHandle, out capacity);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }

            return capacity;
        }

        /// <summary>
        /// Gets the number of buttons on the default target.
        /// </summary>
        /// 
        /// <param name="targetType">
        /// The type of target.
        /// </param>
        /// 
        /// <returns>
        /// The number of buttons belonging to the target.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.CapabilityNotFoundException">
        /// Thrown if the target does not have button capabilities.
        /// </exception>
        public int GetNumTargetButtons(TargetType targetType)
        {
            return this.GetNumTargetButtons(GlobalState.Instance.TargetHandles[(int)targetType]);
        }

        /// <summary>
        /// Gets the number of buttons on the specified target.
        /// </summary>
        /// 
        /// <param name="targetHandle">
        /// A handle to the target.
        /// </param>
        /// 
        /// <returns>
        /// The number of buttons belonging to the target.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the target handle is invalid.
        /// </exception>
        /// <exception cref="zSpace.Core.CapabilityNotFoundException">
        /// Thrown if the target does not have button capabilities.
        /// </exception>
        public int GetNumTargetButtons(IntPtr targetHandle)
        {
            int numButtons = 0;

            PluginError error = zcuGetNumTargetButtons(targetHandle, out numButtons);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return numButtons;
        }

        /// <summary>
        /// Checks whether the button on the default target is pressed.
        /// </summary>
        /// 
        /// <param name="targetType">
        /// The type of target.
        /// </param>
        /// <param name="buttonId">
        /// The id of the button to check.
        /// </param>
        /// 
        /// <returns>
        /// True if the button is pressed. False otherwise.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.CapabilityNotFoundException">
        /// Thrown if the target does not have button capabilities.
        /// </exception>
        public bool IsTargetButtonPressed(TargetType targetType, int buttonId)
        {
            return this.IsTargetButtonPressed(GlobalState.Instance.TargetHandles[(int)targetType], buttonId);
        }

        /// <summary>
        /// Checks whether the button on the specified target is pressed.
        /// </summary>
        /// 
        /// <param name="targetHandle">
        /// A handle to the target.
        /// </param>
        /// <param name="buttonId">
        /// The id of the button to check.
        /// </param>
        /// 
        /// <returns>
        /// True if the button is pressed. False otherwise.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the target handle is invalid.
        /// </exception>
        /// <exception cref="zSpace.Core.CapabilityNotFoundException">
        /// Thrown if the target does not have button capabilities.
        /// </exception>
        public bool IsTargetButtonPressed(IntPtr targetHandle, int buttonId)
        {
            bool isButtonPressed = false;

            PluginError error = zcuIsTargetButtonPressed(targetHandle, buttonId, out isButtonPressed);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return isButtonPressed;
        }

        /// <summary>
        /// Sets whether the default target's LED light is enabled.
        /// </summary>
        /// 
        /// <param name="targetType">
        /// The type of target.
        /// </param>
        /// <param name="isLedEnabled">
        /// True to enable the LED. False otherwise.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.CapabilityNotFoundException">
        /// Thrown if the target does not have LED capabilities.
        /// </exception>
        public void SetTargetLedEnabled(TargetType targetType, bool isLedEnabled)
        {
            this.SetTargetLedEnabled(GlobalState.Instance.TargetHandles[(int)targetType], isLedEnabled);
        }

        /// <summary>
        /// Sets whether the specified target's LED light is enabled.
        /// </summary>
        /// 
        /// <param name="targetHandle">
        /// A handle to the target.
        /// </param>
        /// <param name="isLedEnabled">
        /// True to enable the LED. False otherwise.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the target handle is invalid.
        /// </exception>
        /// <exception cref="zSpace.Core.CapabilityNotFoundException">
        /// Thrown if the target does not have LED capabilities.
        /// </exception>
        public void SetTargetLedEnabled(IntPtr targetHandle, bool isLedEnabled)
        {
            PluginError error = zcuSetTargetLedEnabled(targetHandle, isLedEnabled);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
        }

        /// <summary>
        /// Checks whether the default target's LED light is enabled.
        /// </summary>
        /// 
        /// <param name="targetType">
        /// The type of target.
        /// </param>
        /// 
        /// <returns>
        /// True if the LED is enabled. False otherwise.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.CapabilityNotFoundException">
        /// Thrown if the target does not have LED capabilities.
        /// </exception>
        public bool IsTargetLedEnabled(TargetType targetType)
        {
            return this.IsTargetLedEnabled(GlobalState.Instance.TargetHandles[(int)targetType]);
        }

        /// <summary>
        /// Checks whether the specified target's LED light is enabled.
        /// </summary>
        /// 
        /// <param name="targetHandle">
        /// A handle to the target.
        /// </param>
        /// 
        /// <returns>
        /// True if the LED is enabled. False otherwise.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the target handle is invalid.
        /// </exception>
        /// <exception cref="zSpace.Core.CapabilityNotFoundException">
        /// Thrown if the target does not have LED capabilities.
        /// </exception>
        public bool IsTargetLedEnabled(IntPtr targetHandle)
        {
            bool isLedEnabled = false;

            PluginError error = zcuIsTargetLedEnabled(targetHandle, out isLedEnabled);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return isLedEnabled;
        }

        /// <summary>
        /// Sets the color of the default target's LED light.
        /// </summary>
        /// 
        /// <param name="targetType">
        /// The type of target.
        /// </param>
        /// <param name="ledColor">
        /// The color of the LED. Currently only (r, g, b) values of 0 or 1 are 
        /// supported. If values between 0 and 1 are specified, they will be rounded 
        /// internally to the nearest integer.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.CapabilityNotFoundException">
        /// Thrown if the target does not have LED capabilities.
        /// </exception>
        public void SetTargetLedColor(TargetType targetType, Color ledColor)
        {
            this.SetTargetLedColor(GlobalState.Instance.TargetHandles[(int)targetType], ledColor);
        }

        /// <summary>
        /// Sets the color of the specified target's LED light.
        /// </summary>
        /// 
        /// <param name="targetHandle">
        /// A handle to the target.
        /// </param>
        /// <param name="ledColor">
        /// The color of the LED. Currently only (r, g, b) values of 0 or 1 are 
        /// supported. If values between 0 and 1 are specified, they will be rounded 
        /// internally to the nearest integer.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the target handle is invalid.
        /// </exception>
        /// <exception cref="zSpace.Core.CapabilityNotFoundException">
        /// Thrown if the target does not have LED capabilities.
        /// </exception>
        public void SetTargetLedColor(IntPtr targetHandle, Color ledColor)
        {
            PluginError error = zcuSetTargetLedColor(targetHandle, ledColor.r, ledColor.g, ledColor.b);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
        }

        /// <summary>
        /// Gets the color of the default target's LED light.
        /// </summary>
        /// 
        /// <param name="targetType">
        /// The type of target.
        /// </param>
        /// 
        /// <returns>
        /// The color of the LED.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.CapabilityNotFoundException">
        /// Thrown if the target does not have LED capabilities.
        /// </exception>
        public Color GetTargetLedColor(TargetType targetType)
        {
            return this.GetTargetLedColor(GlobalState.Instance.TargetHandles[(int)targetType]);
        }

        /// <summary>
        /// Gets the color of the specified target's LED light.
        /// </summary>
        /// 
        /// <param name="targetHandle">
        /// A handle to the target.
        /// </param>
        /// 
        /// <returns>
        /// The color of the LED.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the target handle is invalid.
        /// </exception>
        /// <exception cref="zSpace.Core.CapabilityNotFoundException">
        /// Thrown if the target does not have LED capabilities.
        /// </exception>
        public Color GetTargetLedColor(IntPtr targetHandle)
        {
            float r = 0.0f;
            float g = 0.0f;
            float b = 0.0f;

            PluginError error = zcuGetTargetLedColor(targetHandle, out r, out g, out b);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }

            return this.Convert(r, g, b);
        }

        /// <summary>
        /// Sets whether vibration is enabled for the default target.
        /// </summary>
        /// 
        /// <param name="targetType">
        /// The type of target.
        /// </param>
        /// <param name="isVibrationEnabled">
        /// True to enable vibration. False otherwise.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.CapabilityNotFoundException">
        /// Thrown if the target does not have vibration capabilities.
        /// </exception>
        public void SetTargetVibrationEnabled(TargetType targetType, bool isVibrationEnabled)
        {
            this.SetTargetVibrationEnabled(GlobalState.Instance.TargetHandles[(int)targetType], isVibrationEnabled);
        }

        /// <summary>
        /// Sets whether vibration is enabled for the specified target.
        /// </summary>
        /// 
        /// <param name="targetHandle">
        /// A handle to the target.
        /// </param>
        /// <param name="isVibrationEnabled">
        /// True to enable vibration. False otherwise.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the target handle is invalid.
        /// </exception>
        /// <exception cref="zSpace.Core.CapabilityNotFoundException">
        /// Thrown if the target does not have vibration capabilities.
        /// </exception>
        public void SetTargetVibrationEnabled(IntPtr targetHandle, bool isVibrationEnabled)
        {
            PluginError error = zcuSetTargetVibrationEnabled(targetHandle, isVibrationEnabled);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
        }

        /// <summary>
        /// Checks whether vibration is enabled for the default target.
        /// </summary>
        /// 
        /// <param name="targetType">
        /// The type of target.
        /// </param>
        /// 
        /// <returns>
        /// True if vibration is enabled. False otherwise.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.CapabilityNotFoundException">
        /// Thrown if the target does not have vibration capabilities.
        /// </exception>
        public bool IsTargetVibrationEnabled(TargetType targetType)
        {
            return this.IsTargetVibrationEnabled(GlobalState.Instance.TargetHandles[(int)targetType]);
        }

        /// <summary>
        /// Checks whether vibration is enabled for the specified target.
        /// </summary>
        /// 
        /// <param name="targetHandle">
        /// A handle to the target.
        /// </param>
        /// 
        /// <returns>
        /// True if vibration is enabled. False otherwise.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the target handle is invalid.
        /// </exception>
        /// <exception cref="zSpace.Core.CapabilityNotFoundException">
        /// Thrown if the target does not have vibration capabilities.
        /// </exception>
        public bool IsTargetVibrationEnabled(IntPtr targetHandle)
        {
            bool isVibrationEnabled = false;

            PluginError error = zcuIsTargetVibrationEnabled(targetHandle, out isVibrationEnabled);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return isVibrationEnabled;
        }

        /// <summary>
        /// Checks whether the default target is currently vibrating.
        /// </summary>
        /// 
        /// <param name="targetType">
        /// The type of target.
        /// </param>
        /// 
        /// <returns>
        /// True if the target is vibrating. False otherwise.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.CapabilityNotFoundException">
        /// Thrown if the target does not have vibration capabilities.
        /// </exception>
        public bool IsTargetVibrating(TargetType targetType)
        {
            return this.IsTargetVibrating(GlobalState.Instance.TargetHandles[(int)targetType]);
        }

        /// <summary>
        /// Checks whether the specified target is currently vibrating.
        /// </summary>
        /// 
        /// <param name="targetHandle">
        /// A handle to the target.
        /// </param>
        /// 
        /// <returns>
        /// True if the target is vibrating. False otherwise.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the target handle is invalid.
        /// </exception>
        /// <exception cref="zSpace.Core.CapabilityNotFoundException">
        /// Thrown if the target does not have vibration capabilities.
        /// </exception>
        public bool IsTargetVibrating(IntPtr targetHandle)
        {
            bool isVibrating = false;

            PluginError error = zcuIsTargetVibrating(targetHandle, out isVibrating);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return isVibrating;
        }

        /// <summary>
        /// Initiates a vibration on the default target.
        /// </summary>
        /// 
        /// <remarks>
        /// If intensity is not supported in the target's hardware, it will be
        /// ignored and the target will vibrate at 100% intensity (equivalent to
        /// an intensity value of 1.0).
        /// </remarks>
        /// 
        /// <param name="targetType">
        /// The type of target.
        /// </param>
        /// <param name="onPeriod">
        /// The duration in seconds of the vibration.
        /// </param>
        /// <param name="offPeriod">
        /// The duration in seconds between vibrations.
        /// </param>
        /// <param name="numTimes">
        /// The number of times the vibration occurs.
        /// </param>
        /// <param name="intensity">
        /// The intensity of the vibration from 0.0 to 1.0. Currently only intensities 
        /// at 0.1 intervals are supported (i.e. 0.1, 0.2, …, 0.9, 1.0). Intensity values not 
        /// specified at a valid interval will be rounded down to the nearest valid interval.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.CapabilityNotFoundException">
        /// Thrown if the target does not have vibration capabilities.
        /// </exception>
        /// <exception cref="zSpace.Core.OperationFailedException">
        /// Thrown if vibration is supported, but the vibration failed to trigger or
        /// is currently disabled.
        /// </exception>
        public void StartTargetVibration(TargetType targetType, float onPeriod, float offPeriod, int numTimes, float intensity)
        {
            this.StartTargetVibration(GlobalState.Instance.TargetHandles[(int)targetType], onPeriod, offPeriod, numTimes, intensity);
        }

        /// <summary>
        /// Initiates a vibration on the specified target.
        /// </summary>
        /// 
        /// <remarks>
        /// If intensity is not supported in the target's hardware, it will be
        /// ignored and the target will vibrate at 100% intensity (equivalent to
        /// an intensity value of 1.0).
        /// </remarks>
        /// 
        /// <param name="targetHandle">
        /// A handle to the target.
        /// </param>
        /// <param name="onPeriod">
        /// The duration in seconds of the vibration.
        /// </param>
        /// <param name="offPeriod">
        /// The duration in seconds between vibrations.
        /// </param>
        /// <param name="numTimes">
        /// The number of times the vibration occurs.
        /// </param>
        /// <param name="intensity">
        /// The intensity of the vibration from 0.0 to 1.0. Currently only intensities 
        /// at 0.1 intervals are supported (i.e. 0.1, 0.2, …, 0.9, 1.0). Intensity values not 
        /// specified at a valid interval will be rounded down to the nearest valid interval.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the target handle is invalid.
        /// </exception>
        /// <exception cref="zSpace.Core.CapabilityNotFoundException">
        /// Thrown if the target does not have vibration capabilities.
        /// </exception>
        /// <exception cref="zSpace.Core.OperationFailedException">
        /// Thrown if vibration is supported, but the vibration failed to trigger or
        /// is currently disabled.
        /// </exception>
        public void StartTargetVibration(IntPtr targetHandle, float onPeriod, float offPeriod, int numTimes, float intensity)
        {
            PluginError error = zcuStartTargetVibration(targetHandle, onPeriod, offPeriod, numTimes, intensity);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
        }

        /// <summary>
        /// Interrupts the vibration of the default target.
        /// </summary>
        /// 
        /// <param name="targetType">
        /// The type of target.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.CapabilityNotFoundException">
        /// Thrown if the target does not have vibration capabilities.
        /// </exception>
        /// <exception cref="zSpace.Core.OperationFailedException">
        /// Thrown if vibration is supported, but the vibration failed to be interrupted
        /// or is currently disabled.
        /// </exception>
        public void StopTargetVibration(TargetType targetType)
        {
            this.StopTargetVibration(GlobalState.Instance.TargetHandles[(int)targetType]);
        }

        /// <summary>
        /// Interrupts the vibration of the specified target.
        /// </summary>
        /// 
        /// <param name="targetHandle">
        /// A handle to the target.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the target handle is invalid.
        /// </exception>
        /// <exception cref="zSpace.Core.CapabilityNotFoundException">
        /// Thrown if the target does not have vibration capabilities.
        /// </exception>
        /// <exception cref="zSpace.Core.OperationFailedException">
        /// Thrown if vibration is supported, but the vibration failed to be interrupted
        /// or is currently disabled.
        /// </exception>
        public void StopTargetVibration(IntPtr targetHandle)
        {
            PluginError error = zcuStopTargetVibration(targetHandle);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
        }

        /// <summary>
        /// Checks if the default target is tapping/pressing the zSpace 
        /// display's surface. 
        /// </summary>
        /// 
        /// <param name="targetType">
        /// The type of target.
        /// </param>
        /// 
        /// <returns>
        /// True if the target is tapping the zSpace display's surface. False otherwise.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.CapabilityNotFoundException">
        /// Thrown if the target does not have tap capabilities.
        /// </exception>
        public bool IsTargetTapPressed(TargetType targetType)
        {
            return this.IsTargetTapPressed(GlobalState.Instance.TargetHandles[(int)targetType]);
        }

        /// <summary>
        /// Checks if the specified target is tapping/pressing the zSpace 
        /// display's surface. 
        /// </summary>
        /// 
        /// <param name="targetHandle">
        /// A handle to the target.
        /// </param>
        /// 
        /// <returns>
        /// True if the target is tapping the zSpace display's surface. False otherwise.
        /// </returns>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the target handle is invalid.
        /// </exception>
        /// <exception cref="zSpace.Core.CapabilityNotFoundException">
        /// Thrown if the target does not have tap capabilities.
        /// </exception>
        public bool IsTargetTapPressed(IntPtr targetHandle)
        {
            bool isTapPressed = false;

            PluginError error = zcuIsTargetTapPressed(targetHandle, out isTapPressed);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return isTapPressed;
        }

        /// <summary>
        /// Sets whether mouse emulation is enabled.
        /// </summary>
        /// 
        /// <param name="isEnabled">
        /// True to enable mouse emulation. False otherwise.
        /// </param>
        public void SetMouseEmulationEnabled(bool isEnabled)
        {
            PluginError error = zcuSetMouseEmulationEnabled(GlobalState.Instance.Context, isEnabled);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
        }

        /// <summary>
        /// Checks whether mouse emulation is enabled.
        /// </summary>
        /// 
        /// <returns>
        /// True if mouse emulation is enabled. False otherwise.
        /// </returns>
        public bool IsMouseEmulationEnabled()
        {
            bool isEnabled = false;

            PluginError error = zcuIsMouseEmulationEnabled(GlobalState.Instance.Context, out isEnabled);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return isEnabled;
        }

        /// <summary>
        /// Assigns the default target to emulate the mouse.
        /// </summary>
        /// 
        /// <param name="targetType">
        /// The type of target.
        /// </param>
        public void SetMouseEmulationTarget(TargetType targetType)
        {
            this.SetMouseEmulationTarget(GlobalState.Instance.TargetHandles[(int)targetType]);
        }

        /// <summary>
        /// Assigns the specified target to emulate the mouse.
        /// </summary>
        /// 
        /// <param name="targetHandle">
        /// A handle to the target.
        /// </param>
        /// 
        /// <exception cref="zSpace.Core.InvalidHandleException">
        /// Thrown if the target handle is invalid.
        /// </exception>
        public void SetMouseEmulationTarget(IntPtr targetHandle)
        {
            PluginError error = zcuSetMouseEmulationTarget(GlobalState.Instance.Context, targetHandle);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
        }

        /// <summary>
        /// Gets a handle to the target currently responsible for emulating the mouse.
        /// </summary>
        /// 
        /// <returns>
        /// A handle to the target.
        /// </returns>
        public IntPtr GetMouseEmulationTarget()
        {
            IntPtr targetHandle = IntPtr.Zero;

            PluginError error = zcuGetMouseEmulationTarget(GlobalState.Instance.Context, out targetHandle);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return targetHandle;
        }

        /// <summary>
        /// Sets the movement mode of the mouse emulator.
        /// </summary>
        /// 
        /// <param name="movementMode">
        /// Whether movement is either absolute, or relative to the mouse's current position.
        /// </param>
        public void SetMouseEmulationMovementMode(MovementMode movementMode)
        {
            PluginError error = zcuSetMouseEmulationMovementMode(GlobalState.Instance.Context, movementMode);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
        }

        /// <summary>
        /// Gets the movement mode of the mouse emulator.
        /// </summary>
        /// 
        /// <returns>
        /// Whether movement is either absolute, or relative to the mouse’s current position.
        /// </returns>
        public MovementMode GetMouseEmulationMovementMode()
        {
            MovementMode movementMode;

            PluginError error = zcuGetMouseEmulationMovementMode(GlobalState.Instance.Context, out movementMode);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return movementMode;
        }

        /// <summary>
        /// Sets the maximum distance (in meters) that the target can be from the 
        /// display while emulating the mouse.
        /// </summary>
        /// 
        /// <param name="maxDistance">
        /// The maximum distance in meters.
        /// </param>
        public void SetMouseEmulationMaxDistance(float maxDistance)
        {
            PluginError error = zcuSetMouseEmulationMaxDistance(GlobalState.Instance.Context, maxDistance);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
        }

        /// <summary>
        /// Gets the maximum distance (in meters) that the target can be from the 
        /// display while emulating the mouse.
        /// </summary>
        /// 
        /// <returns>
        /// The maximum distance in meters.
        /// </returns>
        public float GetMouseEmulationMaxDistance()
        {
            float maxDistance = 0.0f;

            PluginError error = zcuGetMouseEmulationMaxDistance(GlobalState.Instance.Context, out maxDistance);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return maxDistance;
        }

        /// <summary>
        /// Maps a button the the target to a mouse button.
        /// </summary>
        /// 
        /// <param name="buttonId">
        /// The button on the target.
        /// </param>
        /// <param name="mouseButton">
        /// The corresponding button on the mouse.
        /// </param>
        public void SetMouseEmulationButtonMapping(int buttonId, MouseButton mouseButton)
        {
            PluginError error = zcuSetMouseEmulationButtonMapping(GlobalState.Instance.Context, buttonId, mouseButton);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
        }

        /// <summary>
        /// Gets the currently mapped mouse button for the specified target button.
        /// </summary>
        /// 
        /// <param name="buttonId">
        /// The button on the target.
        /// </param>
        /// 
        /// <returns>
        /// The corresponding button on the mouse.
        /// </returns>
        public MouseButton GetMouseEmulationButtonMapping(int buttonId)
        {
            MouseButton mouseButton = MouseButton.Unknown;
            
            PluginError error = zcuGetMouseEmulationButtonMapping(GlobalState.Instance.Context, buttonId, out mouseButton);
            if (error != PluginError.Ok)
            {
                throw this.NewPluginException(error);
            }
            
            return mouseButton;
        }

        /// <summary>
        /// Calculates a world-space monoscopic camera position based on a specified
        /// world-space focal point and camera rotation.
        /// </summary>
        /// 
        /// <remarks>
        /// Positioning a monoscopic camera at the position returned by ComputeCameraPosition()
        /// will place the focal point at the center of the viewport at zero parallax.
        /// </remarks>
        /// 
        /// <param name="focalPoint">
        /// Position in world-space to center the viewport on.
        /// </param>
        /// <param name="cameraRotation">
        /// The current world-space rotation of the camera.
        /// </param>
        /// 
        /// <returns>
        /// The new world-space camera position.
        /// </returns>
        public Vector3 ComputeCameraPosition(Vector3 focalPoint, Quaternion cameraRotation)
        {
            Vector3 cameraOffset = this.GetFrustumCameraOffset();
            Vector3 cameraPosition = focalPoint + (cameraRotation * (Vector3.back * cameraOffset.magnitude * this.ViewerScale));

            return cameraPosition;
        }


        //////////////////////////////////////////////////////////////////
        // Private Methods
        //////////////////////////////////////////////////////////////////

        private void InitializeLRDetect()
        {
            ZCore.IssuePluginEvent(ZCore.PluginEvent.InitializeLRDetect);
        }

        private void InitializeStereoInfo()
        {
            // Initialize current display.
            try
            {
                // Attempt to grab the first zSpace display and cache it
                // as the current display.
                _displayHandle = this.GetDisplay(DisplayType.zSpace, 0);
            }
            catch
            {
                // Otherwise, grab the first display and cache it.
                if (this.GetNumDisplays() > 0)
                {
                    _displayHandle = this.GetDisplay(0);
                }
                else
                {
                    Debug.LogError("Failed to initialize current display. No displays found.");
                }
            }

            // Initialize cached stereo information.
            for (int i = 0; i < (int)Eye.NumEyes; ++i)
            {
                _viewMatrices[i] = Matrix4x4.identity;
                _projectionMatrices[i] = Matrix4x4.identity;
            }

            for (int i = 0; i < (int)CoordinateSpace.NumSpaces; ++i)
            {
                for (int j = 0; j < (int)CoordinateSpace.NumSpaces; ++j)
                {
                    _coordinateSpaceMatrices[i,j] = Matrix4x4.identity;
                }
            }

            // Initialize the cached fullscreen flag.
            _wasFullScreen = Screen.fullScreen;
        }

        private void InitializeStereoRig()
        {
            // Grab a reference to the stereo rig.
            _stereoRigTransform = this.transform.Find("StereoRig");
            if (_stereoRigTransform == null)
            {
                Debug.LogError("Unable to find stereo rig transform.");
                return;
            }

            // Grab references to all stereo cameras.
            for (int i = 0; i < s_stereoCameraNames.Count; ++i)
            {
                if (_stereoCameras[i] == null)
                {
                    Transform cameraTransform = _stereoRigTransform.Find(s_stereoCameraNames[i]);
                    if (cameraTransform == null)
                    {
                        Debug.LogError(string.Format("Unable to find {0} transform.", s_stereoCameraNames[i]));
                        continue;
                    }

                    _stereoCameras[i] = cameraTransform.GetComponent<StereoCamera>();
                    _stereoCameras[i].SetCameraEnabled(false);
                }
            }

            // Register the PreCullUpdate with the left stereo camera's
            // precull event.
            if (Application.isPlaying)
            {
                if (_stereoCameras[(int)Eye.Left] != null)
                {
                    // Guarantee only one instance of the callback has been registered.
                    _stereoCameras[(int)Eye.Left].PreCull -= PreCullUpdate;
                    _stereoCameras[(int)Eye.Left].PreCull += PreCullUpdate;
                }
            }
        }

        private void InitializeTrackingInfo()
        {
            for (int i = 0; i < (int)TargetType.NumTypes; ++i)
            {
                // Initialize pose information for all coordinate spaces.
                for (int j = 0; j < (int)CoordinateSpace.NumSpaces; ++j)
                {
                    if (_targetPoses[i,j] == null)
                    {
                        _targetPoses[i,j] = new Pose(Matrix4x4.identity, 0.0, (CoordinateSpace)j);
                    }
                }

                // Initialize button state information.
                if (_targetButtonStates[i] == null)
                {
                    try
                    {
                        int numButtons = this.GetNumTargetButtons((TargetType)i);
                        
                        _targetButtonStates[i] = new List<bool>(numButtons);
                        _targetButtonStates[i].AddRange(Enumerable.Repeat(false, numButtons));
                    }
                    catch
                    {
                        _targetButtonStates[i] = new List<bool>();
                    }
                }

                // Initialize tap state information.
                _targetTapStates[i] = false;
            }
        }

        private void CheckCameraChanged()
        {
            // Update the camera information only if the current camera has changed.
            if (this.CurrentCameraObject != _previousCameraObject)
            {
                // Grab the current mono camera if it exists.
                _currentCamera = null;

                if (this.CurrentCameraObject != null)
                {
                    _currentCamera = this.CurrentCameraObject.GetComponent<Camera>();
                }
              
                if (Application.isPlaying)
                {
                    float monoCameraDepth = 0.0f;

                    if (_currentCamera != null)
                    {
                        // Grab the mono camera's depth.
                        monoCameraDepth = _currentCamera.depth;

                        if (this.CopyCurrentCameraAttributes)
                        {
                            // Update the near and far clip planes.
                            this.SetFrustumAttribute(FrustumAttribute.NearClip, _currentCamera.nearClipPlane);
                            this.SetFrustumAttribute(FrustumAttribute.FarClip, _currentCamera.farClipPlane);

                            // Copy camera attributes.
                            for (int i = 0; i < (int)Eye.Center; ++i)
                            {
                                Camera camera = _stereoCameras[i].GetComponent<Camera>();

                                camera.clearFlags = _currentCamera.clearFlags;
                                camera.backgroundColor = _currentCamera.backgroundColor;
                                camera.cullingMask = _currentCamera.cullingMask;
                            }
                        }
                    }

                    // Update the depth values for the stereo cameras.
                    for (int i = 0; i < (int)Eye.NumEyes; ++i)
                    {
                        _stereoCameras[i].SetCameraDepth(monoCameraDepth + i + 1.0f);
                    }
                }
            }

            _previousCameraObject = this.CurrentCameraObject;
        }

        private void CheckFieldsChanged()
        {
            if (this.EnableStereo != this.IsStereoEnabled())
            {
                this.SetStereoEnabled(this.EnableStereo);

                if (Application.isEditor && !Application.isPlaying)
                {
                    StereoCamera centerStereoCamera = this.GetStereoCamera(Eye.Center);
                    if (centerStereoCamera != null)
                    {
                        centerStereoCamera.SetCameraEnabled(this.EnableStereo);
                    }
                }
            }

            if (this.Ipd != this.GetFrustumAttributeFloat(FrustumAttribute.Ipd))
            {
                this.SetFrustumAttribute(FrustumAttribute.Ipd, this.Ipd);
            }

            if (this.ViewerScale != this.GetFrustumAttributeFloat(FrustumAttribute.ViewerScale))
            {
                this.SetFrustumAttribute(FrustumAttribute.ViewerScale, this.ViewerScale);
            }

            if (this.EnableMouseEmulation != this.IsMouseEmulationEnabled())
            {
                this.SetMouseEmulationEnabled(this.EnableMouseEmulation);
            }
        }

        private void UpdateTweens()
        {
            for (int i = _tweenInfos.Count - 1; i >= 0; i--)
            {
                TweenInfo info = _tweenInfos[i];

                if (info.Delay > 0.0f)
                {
                    info.Delay -= Time.unscaledDeltaTime;
                    continue;
                }

                // Advance the time.
                info.Time += Time.unscaledDeltaTime;

                // Compute the normalized time.
                float t = this.ComputeNormalizedTime(info.Time, info.Duration, info.Ease);
                
                // Invoke custom updaters.
                if (info.OnUpdate != null)
                {
                    info.OnUpdate(t);
                }

                // Check if the tween has finished.
                if (info.Time >= info.Duration)
                {
                    if (info.OnComplete != null)
                    {
                        info.OnComplete();
                    }

                    _tweenInfos.RemoveAt(i);
                }
            }
        }

        private void UpdateAutoStereo()
        {
            if (!this.EnableAutoStereo)
            {
                return;
            }

            switch (GlobalState.Instance.AutoStereoState)
            {
                case AutoStereoState.IdleMono:
                    {
                        Pose headPose = this.ComputeDefaultHeadPose();
                        zcuSetFrustumHeadPose(GlobalState.Instance.FrustumHandle, this.Convert(headPose));

                        if (this.IsTargetVisible(TargetType.Head))
                        {
                            GlobalState.Instance.AutoStereoState = AutoStereoState.Animating;

                            Action<float> onUpdate = (t) =>
                            {
                                float ipd = Mathf.Lerp(0.0f, this.Ipd, t);
                                zcuSetFrustumAttributeF32(GlobalState.Instance.FrustumHandle, FrustumAttribute.Ipd, ipd);

                                Pose pose = this.Lerp(headPose, this.GetTargetPose(TargetType.Head, CoordinateSpace.Tracker), t);
                                zcuSetFrustumHeadPose(GlobalState.Instance.FrustumHandle, this.Convert(pose));
                            };

                            Action onComplete = () =>
                            {
                                GlobalState.Instance.AutoStereoState = AutoStereoState.IdleStereo;
                                _autoStereoElapsedTime = 0.0f;
                            };

                            this.StartTween(onUpdate, this.AutoStereoDuration)
                                .SetOnComplete(onComplete)
                                .SetEase(EaseType.EaseOutQuad);
                        }
                    }
                    break;

                case AutoStereoState.IdleStereo:
                    {
                        if (this.IsTargetVisible(TargetType.Head))
                        {
                            _autoStereoElapsedTime = 0.0f;
                        }
                        else
                        {
                            _autoStereoElapsedTime += Time.unscaledDeltaTime;

                            if (_autoStereoElapsedTime >= this.AutoStereoDelay)
                            {
                                GlobalState.Instance.AutoStereoState = AutoStereoState.Animating;

                                Pose defaultPose = this.ComputeDefaultHeadPose();
                                Pose frustumPose = this.GetFrustumHeadPose();

                                Action<float> onUpdate = (t) =>
                                {
                                    float ipd = Mathf.Lerp(this.Ipd, 0.0f, t);
                                    zcuSetFrustumAttributeF32(GlobalState.Instance.FrustumHandle, FrustumAttribute.Ipd, ipd);

                                    Pose pose = this.Lerp(frustumPose, defaultPose, t);
                                    zcuSetFrustumHeadPose(GlobalState.Instance.FrustumHandle, this.Convert(pose));
                                };

                                Action onComplete = () =>
                                {
                                    GlobalState.Instance.AutoStereoState = AutoStereoState.IdleMono;
                                };

                                this.StartTween(onUpdate, this.AutoStereoDuration)
                                    .SetOnComplete(onComplete)
                                    .SetEase(EaseType.EaseOutQuad);
                            }
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        private void UpdateMouseAutoHide()
        {
            Vector3 mousePosition = Input.mousePosition;

            if (this.EnableMouseAutoHide)
            {
                if (mousePosition != _previousMousePosition ||
                    Input.GetMouseButton(0) ||
                    Input.GetMouseButton(1) ||
                    Input.GetMouseButton(2))
                {
                    // If the cursor was previously disabled and the mouse
                    // moved or button was pressed, show the cursor and reset 
                    // the elapsed time before the mouse auto-hides.
                    this.SetMouseCursorVisible(true);
                    _mouseAutoHideElapsedTime = 0.0f;
                }
                else if (this.IsMouseCursorVisible())
                {
                    // Update the ekapsed time before the mouse auto-hides.
                    // If the elapsed time falls exceeds the delay, we know 
                    // that the mouse has not been moved.
                    _mouseAutoHideElapsedTime += Time.unscaledDeltaTime;
                    if (_mouseAutoHideElapsedTime >= this.MouseAutoHideDelay)
                    {
                        this.SetMouseCursorVisible(false);
                    }
                }
            }
            else if (!this.EnableMouseAutoHide && _wasMouseAutoHideEnabled)
            {
                // Mouse auto-hide was toggled off. Restore the mouse cursor.
                this.SetMouseCursorVisible(true);
            }

            _previousMousePosition = mousePosition;
            _wasMouseAutoHideEnabled = this.EnableMouseAutoHide;
        }

        private void UpdateStereoInfo()
        {
            // Grab the viewport's virtual desktop position.
            if (Application.isEditor)
            {
            #if UNITY_EDITOR
                // Grab a reference to the editor window's type.
                if (_gameViewType == null)
                {
                    _gameViewType = Type.GetType("UnityEditor.GameView, UnityEditor");
                    if (_gameViewType == null)
                    {
                        Debug.LogError("Failed to get type of UnityEditor.GameView.");
                    }
                }

                // If the GameView EditorWindow's native window handle
                // has not been grabbed yet, put the GameView into focus.
                if (s_gameViewWindowHandle == IntPtr.Zero && _gameViewType != null)
                {
                    EditorWindow.FocusWindowIfItsOpen(_gameViewType);
                }

                // If the focused window is currently the GameView window,
                // grab its associed native Win32 window handle.
                if (EditorWindow.focusedWindow != null && 
                    EditorWindow.focusedWindow.GetType() == _gameViewType)
                {
                    IntPtr windowHandle = GetFocus();
                    if (windowHandle != IntPtr.Zero)
                    {
                        s_gameViewWindowHandle = windowHandle;
                    }
                }
            #endif

                // Get the window's top-left (x, y) virtual desktop position
                // and apply a vertical pixel offset to account for the GameView's
                // title bar.
                if (s_gameViewWindowHandle != IntPtr.Zero)
                {
                    RECT rect;
                    bool success = GetWindowRect(s_gameViewWindowHandle, out rect);
                    if (success)
                    {
                        _windowX = rect.Left;
                        _windowY = rect.Top + s_gameViewPixelOffsetY;
                    }
                    else
                    {
                        s_gameViewWindowHandle = IntPtr.Zero;
                    }
                }
            }
            else
            {
                zcuGetWindowPosition(out _windowX, out _windowY);
            }

            // Update the viewport size.
            this.SetViewportSize(GlobalState.Instance.ViewportHandle, _windowWidth, _windowHeight);

            // Update the viewport position.
            this.SetViewportPosition(GlobalState.Instance.ViewportHandle, _windowX, _windowY);

            // Update the cached view and projection matrices.
            for (int i = 0; i < (int)Eye.NumEyes; ++i)
            {
                _viewMatrices[i] = this.GetFrustumViewMatrix(GlobalState.Instance.FrustumHandle, (Eye)i);
                _projectionMatrices[i] = this.GetFrustumProjectionMatrix(GlobalState.Instance.FrustumHandle, (Eye)i);
            }
        }

        private void UpdateStereoRig()
        {
            if (this.CurrentCameraObject != null)
            {
                if (_stereoRigTransform != null)
                {
                    // Update the stereo rig's transform.
                    _stereoRigTransform.position   = this.CurrentCameraObject.transform.position;
                    _stereoRigTransform.rotation   = this.CurrentCameraObject.transform.rotation;
                    _stereoRigTransform.localScale = this.CurrentCameraObject.transform.localScale;
                }
            }
        }

        private void UpdateStereoCameras()
        {
            // Update the view and projection matrices for the stereo cameras.
            for (int i = 0; i < (int)Eye.NumEyes; ++i)
            {
                // Update the stereo camera.
                if (_stereoCameras[i] != null)
                {
                    _stereoCameras[i].SetViewMatrix(_viewMatrices[i]);
                    _stereoCameras[i].SetProjectionMatrix(_projectionMatrices[i]);
                }
            }
        }

        private void UpdateCoordinateSpaceInfo()
        {
            // Grab all coordinate space transformation matrices in tracker, display,
            // viewport, and camera space. Convert them to Unity's left-handed 4x4
            // matrices.
            ZSMatrix4 temp;
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    PluginError error = 
                        zcuGetCoordinateSpaceTransform(
                            GlobalState.Instance.ViewportHandle, 
                            (CoordinateSpace)i, 
                            (CoordinateSpace)j, 
                            out temp);
                    
                    if (error == PluginError.Ok)
                    {
                        _coordinateSpaceMatrices[i,j] = this.Convert(temp, true);
                    }
                }
            }

            // Calculate all coordinate space transformations that incorporate world
            // space based on the current camera.
            Matrix4x4 cameraLocalToWorld = Matrix4x4.identity;
            Matrix4x4 cameraWorldToLocal = Matrix4x4.identity;
            if (this.CurrentCameraObject != null)
            {
                cameraLocalToWorld = this.CurrentCameraObject.transform.localToWorldMatrix;
                cameraWorldToLocal = this.CurrentCameraObject.transform.worldToLocalMatrix;
            }

            _coordinateSpaceMatrices[4,4] = Matrix4x4.identity;

            // To world space:
            _coordinateSpaceMatrices[0,4] = cameraLocalToWorld * _coordinateSpaceMatrices[0,3];
            _coordinateSpaceMatrices[1,4] = cameraLocalToWorld * _coordinateSpaceMatrices[1,3];
            _coordinateSpaceMatrices[2,4] = cameraLocalToWorld * _coordinateSpaceMatrices[2,3];
            _coordinateSpaceMatrices[3,4] = cameraLocalToWorld;

            // From world space:
            _coordinateSpaceMatrices[4,0] = _coordinateSpaceMatrices[3,0] * cameraWorldToLocal;
            _coordinateSpaceMatrices[4,1] = _coordinateSpaceMatrices[3,1] * cameraWorldToLocal;
            _coordinateSpaceMatrices[4,2] = _coordinateSpaceMatrices[3,2] * cameraWorldToLocal;
            _coordinateSpaceMatrices[4,3] = cameraWorldToLocal;
        }

        private void UpdateViewportInfo()
        {
            // Calculate the viewport's center position on the virtual desktop (in pixels).
            _viewportVirtualDesktopCenter = new Vector2(_windowX + (_windowWidth * 0.5f), _windowY + (_windowHeight * 0.5f));

            try
            {
                // Get the viewport's current display.
                _displayHandle = this.GetDisplay((int)_viewportVirtualDesktopCenter.x, (int)_viewportVirtualDesktopCenter.y);

                // Get the relevant display information.
                _displayPosition       = this.GetDisplayPosition(_displayHandle);
                _displaySize           = this.GetDisplaySize(_displayHandle);
                _displayResolution     = this.GetDisplayNativeResolution(_displayHandle);
                _displayAngle          = this.GetDisplayAngle(_displayHandle);
                _displayCenter         = _displayPosition + (_displayResolution * 0.5f);
                _displayMetersPerPixel = new Vector2(_displaySize.x / _displayResolution.x, _displaySize.y / _displayResolution.y);
            }
            catch
            {
                //Debug.LogWarning("Could not find current display based on new viewport position. Failed to update display information.");
            }

            // Calculate the viewport's size and half-size (in meters). 
            _viewportSizeInMeters =
                new Vector2(
                    _windowWidth  * _displayMetersPerPixel.x,
                    _windowHeight * _displayMetersPerPixel.y);

            // Calculate the viewport's center position in display space. This is relative to 
            // the current display (in meters).
            _viewportDisplayCenter =
                new Vector2(
                    (_viewportVirtualDesktopCenter.x - _displayCenter.x) * _displayMetersPerPixel.x,
                    (_displayCenter.y - _viewportVirtualDesktopCenter.y) * _displayMetersPerPixel.y);

            // Calculate the viewport's transform in world space.
            Matrix4x4 displayToWorld = this.GetCoordinateSpaceTransform(CoordinateSpace.Display, CoordinateSpace.World);

            _viewportWorldCenter = displayToWorld * new Vector4(_viewportDisplayCenter.x, _viewportDisplayCenter.y, 0.0f, 1.0f);
            _viewportWorldRotation = Quaternion.LookRotation(displayToWorld.GetColumn(2), displayToWorld.GetColumn(1));
            _viewportWorldSize = _viewportSizeInMeters * this.ViewerScale;
        }

        private void UpdateTrackingInfo()
        {
            for (int i = 0; i < (int)TargetType.NumTypes; ++i)
            {
                IntPtr targetHandle = GlobalState.Instance.TargetHandles[i];
                TargetType targetType = (TargetType)i;

                // Update the pose information.
                if (this.IsTargetVisible(targetHandle))
                {
                    // Grab the target's tracker-space pose.
                    Pose currentPose = this.GetTargetPose(targetHandle, CoordinateSpace.Tracker);
                    Pose previousPose = _targetPoses[i,(int)CoordinateSpace.Tracker];

                    // Cache the target's tracker-space pose.
                    _targetPoses[i,(int)CoordinateSpace.Tracker] = currentPose;

                    // Calculate the pose for every other coordinate space.
                    for (int j = (int)CoordinateSpace.Display; j < (int)CoordinateSpace.NumSpaces; ++j)
                    {
                        Matrix4x4 poseMatrix = this.GetCoordinateSpaceTransform(CoordinateSpace.Tracker, (CoordinateSpace)j) * currentPose.Matrix;
                        _targetPoses[i,j] = new Pose(poseMatrix, currentPose.Timestamp, (CoordinateSpace)j);
                    }

                    // Generate move events.
                    if (currentPose.Position != previousPose.Position ||
                        currentPose.Rotation != previousPose.Rotation)
                    {
                        if (this.TargetMove != null)
                        {
                            this.TargetMove(
                                this, 
                                new TrackerEventInfo(
                                    targetHandle, 
                                    targetType, 
                                    _targetPoses[i, (int)CoordinateSpace.World]));
                        }
                    }
                }

                // Generate button events.
                for (int buttonId = 0; buttonId < _targetButtonStates[i].Count; ++buttonId)
                {
                    bool isButtonPressed = this.IsTargetButtonPressed(targetHandle, buttonId);
                    bool wasButtonPressed = _targetButtonStates[i][buttonId];

                    if (isButtonPressed && !wasButtonPressed)
                    {
                        if (this.TargetButtonPress != null)
                        {
                            this.TargetButtonPress(
                                this, 
                                new TrackerButtonEventInfo(
                                    targetHandle,
                                    targetType,
                                    _targetPoses[i, (int)CoordinateSpace.World],
                                    buttonId));
                        }
                    }
                    else if (!isButtonPressed && wasButtonPressed)
                    {
                        if (this.TargetButtonRelease != null)
                        {
                            this.TargetButtonRelease(this, 
                                new TrackerButtonEventInfo(
                                    targetHandle,
                                    targetType,
                                    _targetPoses[i, (int)CoordinateSpace.World],
                                    buttonId));
                        }
                    }

                    _targetButtonStates[i][buttonId] = isButtonPressed;
                }

                // Generate tap events.
                try
                {
                    bool isTapPressed = this.IsTargetTapPressed(targetHandle);
                    bool wasTapPressed = _targetTapStates[i];

                    if (isTapPressed && !wasTapPressed)
                    {
                        if (this.TargetTapPress != null)
                        {
                            this.TargetTapPress(
                                this, 
                                new TrackerEventInfo(
                                    targetHandle, 
                                    targetType, 
                                    _targetPoses[i, (int)CoordinateSpace.World]));
                        }
                    }
                    else if (!isTapPressed && wasTapPressed)
                    {
                        if (this.TargetTapRelease != null)
                        {
                            this.TargetTapRelease(
                                this, 
                                new TrackerEventInfo(
                                    targetHandle, 
                                    targetType, 
                                    _targetPoses[i, (int)CoordinateSpace.World]));
                        }
                    }

                    _targetTapStates[i] = isTapPressed;
                }
                catch
                {
                    // Target does not support tap capability. Do nothing.
                }
            }
        }

        private void UpdateLRDetect()
        {
            if (Screen.fullScreen)
            {
                ZCore.IssuePluginEvent(ZCore.PluginEvent.UpdateLRDetectFullscreen);
            }
            else
            {
                ZCore.IssuePluginEvent(ZCore.PluginEvent.UpdateLRDetectWindowed);
            }

            // For ATI cards, transitioning from windowed mode to fullscreen mode
            // can sometimes cause the left/right frames to become out of sync.
            // Force a sync for this case.
            if (SystemInfo.graphicsDeviceVendorID == 4098)
            {
                if (_wasFullScreen == false && Screen.fullScreen == true)
                {
                    ZCore.IssuePluginEvent(ZCore.PluginEvent.SyncLRDetectFullscreen);
                }
            }
        }

        private void UpdateWindowInfo()
        {
            // Get the updated viewport position in Windows virtual desktop coordinates.
            ZCore.IssuePluginEvent(ZCore.PluginEvent.UpdateWindowPosition);

            _windowWidth = Screen.width;
            _windowHeight = Screen.height;
            _wasFullScreen = Screen.fullScreen;
        }

        private void EditModeUpdateViewportDimensions()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                _windowWidth = Screen.width;
                _windowHeight = Screen.height;
            }
        }

        private void EditModeUpdate()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                // Perform an update on stereo and tracking.
                PluginError error = zcuUpdate(GlobalState.Instance.Context);
                if (error != PluginError.Ok)
                {
                    Debug.LogError(string.Format("Failed to update zSpace Core SDK: ({0})", error));
                }

                // Update the frustum's head pose to force a recalculation of the
                // projections in edit mode.
                this.SetFrustumHeadPose(GlobalState.Instance.FrustumHandle, this.ComputeDefaultHeadPose());

                // Check for updates.
                this.CheckFieldsChanged();

                // Perform updates.
                this.UpdateStereoInfo();
                this.UpdateCoordinateSpaceInfo();
                this.UpdateViewportInfo();
                this.UpdateTrackingInfo();
                this.UpdateStereoRig();
                this.UpdateStereoCameras();

                // Force a redraw of the scene view.
                this.ForceSceneViewRedraw();
            }
        }

        private void PlayModeUpdate()
        {
            if (Application.isPlaying)
            {
                if (!_forceUpdate)
                {
                    return;
                }

                _forceUpdate = false;

                // Dispatch pre-update event.
                if (this.PreUpdate != null)
                {
                    this.PreUpdate(this);
                }

                // Perform an update on stereo and tracking.
                PluginError error = zcuUpdate(GlobalState.Instance.Context);
                if (error != PluginError.Ok)
                {
                    Debug.LogError(string.Format("Failed to update zSpace Core SDK: ({0})", error));
                }

                // Check for updates.
                this.CheckCameraChanged();
                this.CheckFieldsChanged();

                // Perform updates.
                this.UpdateAutoStereo();
                this.UpdateMouseAutoHide();
                this.UpdateTweens();
                this.UpdateStereoInfo();
                this.UpdateCoordinateSpaceInfo();
                this.UpdateViewportInfo();
                this.UpdateTrackingInfo();
                this.UpdateStereoRig();

                // Dispatch post-update event.
                if (this.PostUpdate != null)
                {
                    this.PostUpdate(this);
                }
            }
        }

        private void PreCullUpdate(StereoCamera sender)
        {
            if (this.MinimizeLatency)
            {
                if (GlobalState.Instance.AutoStereoState == AutoStereoState.IdleStereo)
                {
                    // Perform an update on stereo and tracking.
                    PluginError error = zcuUpdate(GlobalState.Instance.Context);
                    if (error != PluginError.Ok)
                    {
                        Debug.LogError(string.Format("Failed to update zSpace Core SDK: ({0})", error));
                    }

                    // Update the cached view and projection matrices.
                    for (int i = 0; i < (int)Eye.NumEyes; ++i)
                    {
                        _viewMatrices[i] = this.GetFrustumViewMatrix(GlobalState.Instance.FrustumHandle, (Eye)i);
                        _projectionMatrices[i] = this.GetFrustumProjectionMatrix(GlobalState.Instance.FrustumHandle, (Eye)i);
                    }
                }
            }

            this.UpdateStereoCameras();
        }

        private IEnumerator EndOfFrameUpdate()
        {
            while (true)
            {                
                // Perform end of frame updates.
                this.UpdateLRDetect();
                this.UpdateWindowInfo();

                // Re-enable the center stereo camera and the current mono camera.
                if (this.IsStereoEnabled())
                {
                    _stereoCameras[(int)Eye.Center].SetCameraEnabled(true);

                    if (_currentCamera != null)
                    {
                        _currentCamera.enabled = true;
                    }   
                }
                
                yield return new WaitForEndOfFrame();

                _forceUpdate = true;
            }
        }

        private void ShutdownLRDetect()
        {
            ZCore.IssuePluginEvent(ZCore.PluginEvent.ShutdownLRDetect);
        }

        private Vector3 Convert(ZSVector3 v, bool flipHandedness)
        {
            return new Vector3(v.x, v.y, flipHandedness ? -v.z : v.z);
        }

        private ZSVector3 Convert(Vector3 v, bool flipHandedness)
        {
            ZSVector3 result;
            result.x = v.x;
            result.y = v.y;
            result.z = flipHandedness ? -v.z : v.z;

            return result;
        }

        private Matrix4x4 Convert(ZSMatrix4 m, bool flipHandedness)
        {
            Matrix4x4 result = Matrix4x4.identity;
            result[0,0] = m.m00;
            result[0,1] = m.m01;
            result[0,2] = m.m02;
            result[0,3] = m.m03;

            result[1,0] = m.m10;
            result[1,1] = m.m11;
            result[1,2] = m.m12;
            result[1,3] = m.m13;

            result[2,0] = m.m20;
            result[2,1] = m.m21;
            result[2,2] = m.m22;
            result[2,3] = m.m23;

            result[3,0] = m.m30;
            result[3,1] = m.m31;
            result[3,2] = m.m32;
            result[3,3] = m.m33;

            if (flipHandedness)
            {
                result = this.FlipHandedness(result);
            }

            return result;
        }

        private ZSMatrix4 Convert(Matrix4x4 m, bool flipHandedness)
        {
            if (flipHandedness)
            {
                m = this.FlipHandedness(m);
            }

            ZSMatrix4 result;
            result.m00 = m[0,0];
            result.m01 = m[0,1];
            result.m02 = m[0,2];
            result.m03 = m[0,3];

            result.m10 = m[1,0];
            result.m11 = m[1,1];
            result.m12 = m[1,2];
            result.m13 = m[1,3];

            result.m20 = m[2,0];
            result.m21 = m[2,1];
            result.m22 = m[2,2];
            result.m23 = m[2,3];

            result.m30 = m[3,0];
            result.m31 = m[3,1];
            result.m32 = m[3,2];
            result.m33 = m[3,3];

            return result;
        }

        private DisplayIntersectionInfo Convert(ZCDisplayIntersectionInfo i)
        {
            return new DisplayIntersectionInfo(i.hit, i.distance, i.x, i.y, i.nx, i.ny);
        }

        private FrustumBounds Convert(ZCFrustumBounds b)
        {
            return new FrustumBounds(b.left, b.right, b.bottom, b.top, b.nearClip, b.farClip);
        }

        private Bounds Convert(ZCBoundingBox b)
        {
            Vector3 min = this.Convert(b.lower, true);
            Vector3 max = this.Convert(b.upper, true);
            Vector3 size = max - min;
            Vector3 center = min + (size * 0.5f);

            return new Bounds(center, size);
        }

        private ZCBoundingBox Convert(Bounds b)
        {
            ZCBoundingBox result;
            result.lower = this.Convert(b.min, true);
            result.upper = this.Convert(b.max, true);

            return result;
        }

        private Pose Convert(ZCTrackerPose p)
        {
            return new Pose(this.Convert(p.matrix, true), p.timestamp, CoordinateSpace.Tracker);
        }

        private ZCTrackerPose Convert(Pose p)
        {
            ZCTrackerPose result;
            result.matrix    = this.Convert(p.Matrix, true);
            result.timestamp = p.Timestamp;

            return result;
        }

        private Color Convert(float r, float g, float b)
        {
            return new Color(r, g, b);
        }

        private Matrix4x4 FlipHandedness(Matrix4x4 matrix)
        {
            return s_flipHandednessMap * matrix * s_flipHandednessMap;
        }

        private Matrix4x4 Lerp(Matrix4x4 from, Matrix4x4 to, float t)
        {
            Vector3 position = Vector3.Lerp(from.GetColumn(3), to.GetColumn(3), t);

            Quaternion fromRotation = Quaternion.LookRotation(from.GetColumn(2), from.GetColumn(1));
            Quaternion toRotation   = Quaternion.LookRotation(to.GetColumn(2), to.GetColumn(1));
            Quaternion rotation     = Quaternion.Lerp(fromRotation, toRotation, t);

            return Matrix4x4.TRS(position, rotation, Vector3.one);
        }

        private Pose Lerp(Pose from, Pose to, float t)
        {
            Vector3    position = Vector3.Lerp(from.Position, to.Position, t);
            Quaternion rotation = Quaternion.Lerp(from.Rotation, to.Rotation, t);

            return new Pose(position, rotation, 0.0f, to.CoordinateSpace);
        }

        private Matrix4x4 ComputeCameraMatrix(Vector3 viewportCenter, Quaternion viewportRotation, float viewerScale)
        {
            Vector3 cameraOffset = this.GetFrustumCameraOffset();
            float angle = this.ComputeAngleBetweenCameraAndDisplay();

            Quaternion cameraRotation = viewportRotation * Quaternion.Euler(90.0f - angle, 0.0f, 0.0f);
            Vector3 cameraPosition = viewportCenter + (cameraRotation * (Vector3.back * cameraOffset.magnitude * viewerScale));

            return Matrix4x4.TRS(cameraPosition, cameraRotation, Vector3.one);
        }

        private float ComputeAngleBetweenCameraAndDisplay()
        {
            Vector3 cameraOffset = this.GetFrustumCameraOffset();
            Vector3 displayDirection = Quaternion.Euler(-_displayAngle.x, 0.0f, 0.0f) * Vector3.forward;

            return Vector3.Angle(cameraOffset.normalized, displayDirection.normalized);
        }

        private Pose ComputeDefaultHeadPose()
        {
            Matrix4x4 cameraMatrix = 
                this.ComputeCameraMatrix(Vector3.zero, Quaternion.Euler(90.0f - _displayAngle.x, 0.0f, 0.0f), 1.0f);

            return new Pose(cameraMatrix, 0.0f, CoordinateSpace.Tracker);
        }

        private float ComputeNormalizedTime(float t, float d, EaseType ease)
        {
            if (t <= 0)
            {
                return 0;
            }

            float a = Mathf.Clamp01(t / d);

            switch (ease)
            {
                case EaseType.Linear:
                    return a;

                case EaseType.EaseInQuad:
                    return Mathf.Pow(a, 2);

                case EaseType.EaseOutQuad:
                    return -a * (a - 2);
                        
                case EaseType.EaseInExpo:
                    return Mathf.Pow(2, 10 * (a - 1));

                case EaseType.EaseOutExpo:
                    return (-Mathf.Pow(2, -10 * a) + 1);

                default:
                    return 1.0f;
            }
        }

        private TweenInfo StartTween(Action<float> onUpdate, float duration)
        {
            TweenInfo info = new TweenInfo(onUpdate);
            info.Duration = duration;

            _tweenInfos.Add(info);

            return info;
        }

        private void CancelTween(TweenInfo info)
        {
            if (_tweenInfos.Contains(info))
            {
                _tweenInfos.Remove(info);
            }
        }

        private void DrawTriAxes(Vector3 position, Quaternion rotation, float size, bool isEnabled)
        {
        #if UNITY_EDITOR
            // Draw local right vector.
            Handles.color = isEnabled ? Color.red : Color.gray;
            Handles.ArrowCap(0, position, rotation * Quaternion.Euler(0.0f, 90.0f, 0.0f), size);

            // Draw local up vector.
            Handles.color = isEnabled ? Color.green : Color.gray;
            Handles.ArrowCap(0, position, rotation * Quaternion.Euler(-90.0f, 0.0f, 0.0f), size);

            // Draw local forward vector.
            Handles.color = isEnabled ? Color.blue : Color.gray;
            Handles.ArrowCap(0, position, rotation * Quaternion.identity, size);
        #endif
        }

        private void ForceSceneViewRedraw()
        {
            // HACK: Modifying an enabled camera's depth will force the scene
            //       view to be redrawn in edit mode.
            StereoCamera centerStereoCamera = this.GetStereoCamera(Eye.Center);
            if (centerStereoCamera != null)
            {
                float depth = (centerStereoCamera.GetCamera().depth == 100000.0f) ? 1000001.0f : 1000000.0f;
                centerStereoCamera.SetCameraDepth(depth);
            }
        }

        private void SetMouseCursorVisible(bool isVisible)
        {
        #if (UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7)
            Screen.showCursor = isVisible;
        #else
            Cursor.visible = isVisible;
        #endif
        }

        private bool IsMouseCursorVisible()
        {
        #if (UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7)
            return Screen.showCursor;
        #else
            return Cursor.visible;
        #endif
        }


        //////////////////////////////////////////////////////////////////
        // Private Enums
        //////////////////////////////////////////////////////////////////

        private enum AutoStereoState
        {
            IdleMono   = 0,
            IdleStereo = 1,
            Animating  = 2,
        }

        private enum EaseType
        {
            Linear      = 0,
            EaseInQuad  = 1,
            EaseOutQuad = 2,
            EaseInExpo  = 3,
            EaseOutExpo = 4,
        }


        //////////////////////////////////////////////////////////////////
        // Private Compound Types
        //////////////////////////////////////////////////////////////////

        private class TweenInfo
        {
            public Action<float> OnUpdate   { get; private set; }
            public Action        OnComplete { get; private set; }
            public float         Delay      { get; set; }
            public float         Duration   { get; set; }
            public float         Time       { get; set; }
            public EaseType      Ease       { get; set; }

            public TweenInfo()
            {
                this.OnUpdate   = null;
                this.OnComplete = null;
                this.Delay      = 0.0f;
                this.Duration   = 0.0f;
                this.Time       = 0.0f;
                this.Ease       = EaseType.Linear;
            }

            public TweenInfo(Action<float> onUpdate)
            {
                this.OnUpdate   = onUpdate;
                this.OnComplete = null;
                this.Delay      = 0.0f;
                this.Duration   = 0.0f;
                this.Time       = 0.0f;
                this.Ease       = EaseType.Linear;
            }

            public TweenInfo SetDelay(float delay)
            {
                this.Delay = delay;
                return this;
            }

            public TweenInfo SetTime(float time)
            {
                this.Time = time;
                return this;
            }

            public TweenInfo SetDuration(float duration)
            {
                this.Duration = duration;
                return this;
            }

            public TweenInfo SetOnComplete(Action onComplete)
            {
                this.OnComplete = onComplete;
                return this;
            }

            public TweenInfo SetEase(EaseType ease)
            {
                this.Ease = ease;
                return this;
            }
        }


        //////////////////////////////////////////////////////////////////
        // Private Members
        //////////////////////////////////////////////////////////////////
        
        private static readonly int           s_gameViewPixelOffsetY = 35;
        private static IntPtr                 s_gameViewWindowHandle = IntPtr.Zero;
        private static readonly Matrix4x4     s_flipHandednessMap = Matrix4x4.Scale(new Vector4(1.0f, 1.0f, -1.0f));
        private static readonly IList<string> s_stereoCameraNames = new ReadOnlyCollection<string>
            (
                new []
                {
                    "LeftCamera",
                    "RightCamera",
                    "CenterCamera",
                }
            );

        private bool            _isStereoEnabled      = false;
        private bool            _forceUpdate          = true;
        private bool            _wasFullScreen        = false;
        private Camera          _currentCamera        = null;
        private GameObject      _previousCameraObject = null;

        private List<TweenInfo> _tweenInfos = new List<TweenInfo>();

        private Matrix4x4[]     _viewMatrices       = new Matrix4x4[(int)Eye.NumEyes];
        private Matrix4x4[]     _projectionMatrices = new Matrix4x4[(int)Eye.NumEyes];
        
        private int             _windowX = 0;
        private int             _windowY = 0;
        private int             _windowWidth = 2;
        private int             _windowHeight = 2;
        private float           _nearClip = 0.1f;
        private float           _farClip = 1000.0f;

        private Type            _gameViewType;
        private Vector2         _viewportVirtualDesktopCenter = Vector2.zero;
        private Vector2         _viewportDisplayCenter = Vector2.zero;
        private Vector2         _viewportSizeInMeters = Vector2.zero;
        private Vector3         _viewportWorldCenter = Vector3.zero;
        private Quaternion      _viewportWorldRotation = Quaternion.identity;
        private Vector2         _viewportWorldSize = Vector2.zero;

        private IntPtr          _displayHandle = IntPtr.Zero;
        private Vector2         _displayPosition = Vector2.zero;
        private Vector2         _displaySize = Vector2.zero;
        private Vector2         _displayResolution = Vector2.zero;
        private Vector3         _displayAngle = Vector3.zero;
        private Vector2         _displayCenter = Vector2.zero;
        private Vector2         _displayMetersPerPixel = Vector2.zero;

        private Transform       _stereoRigTransform    = null;
        private StereoCamera[]  _stereoCameras         = new StereoCamera[(int)Eye.NumEyes];
        private float           _autoStereoElapsedTime = 0.0f;

        private Vector3         _previousMousePosition = Vector3.zero;
        private bool            _wasMouseAutoHideEnabled = false;
        private float           _mouseAutoHideElapsedTime = 0.0f;

        private Matrix4x4[,]    _coordinateSpaceMatrices = new Matrix4x4[(int)CoordinateSpace.NumSpaces,(int)CoordinateSpace.NumSpaces];
        
        private Pose[,]         _targetPoses = new Pose[(int)TargetType.NumTypes,(int)CoordinateSpace.NumSpaces];
        private List<bool>[]    _targetButtonStates = new List<bool>[(int)TargetType.NumTypes];
        private bool[]          _targetTapStates = new bool[(int)TargetType.NumTypes];
    }
}

