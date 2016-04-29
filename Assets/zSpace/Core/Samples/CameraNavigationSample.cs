//////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2016 zSpace, Inc.  All Rights Reserved.
//
//////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace zSpace.Core.Samples
{
    public class CameraNavigationSample : MonoBehaviour
    {
        //////////////////////////////////////////////////////////////////
        // Unity Monobehaviour Callbacks
        //////////////////////////////////////////////////////////////////

        void Start()
        {
            _core = GameObject.FindObjectOfType<ZCore>();
            if (_core == null)
            {
                Debug.LogError("Unable to find reference to zSpace.Core.Core Monobehaviour.");
                this.enabled = false;
                return;
            }

            _isMinimizeLatencyEnabled = _core.MinimizeLatency;
        }

        void Update()
        {
            this.UpdateTweens();
        }

        void OnGUI()
        {
            // Navigate to object:
            if (GUILayout.Button("Navigate to Cube"))
            {
                this.NavigateTo(GameObject.Find("Cube").transform.position, Quaternion.identity, 1.0f);
            }
            if (GUILayout.Button("Navigate to Sphere"))
            {
                this.NavigateTo(GameObject.Find("Sphere").transform.position, Quaternion.Euler(45.0f, 0.0f, 0.0f), 0.5f);
            }
            if (GUILayout.Button("Navigate to Capsule"))
            {
                this.NavigateTo(GameObject.Find("Capsule").transform.position, Quaternion.Euler(0.0f, -45.0f, 0.0f), 2.0f);
            }

            // Perspective:
            if (GUILayout.Button("Left Perspective"))
            {
                this.NavigateTo(Quaternion.Euler(0.0f, 90.0f, 0.0f));
            }
            if (GUILayout.Button("Right Perspective"))
            {
                this.NavigateTo(Quaternion.Euler(0.0f, -90.0f, 0.0f));
            }
            if (GUILayout.Button("Top Perspective"))
            {
                this.NavigateTo(Quaternion.Euler(90.0f, 0.0f, 0.0f));
            }
            if (GUILayout.Button("Bottom Perspective"))
            {
                this.NavigateTo(Quaternion.Euler(-90.0f, 0.0f, 0.0f));
            }
            if (GUILayout.Button("Front Perspective"))
            {
                this.NavigateTo(Quaternion.Euler(0.0f, 0.0f, 0.0f));
            }
            if (GUILayout.Button("Back Perspective"))
            {
                this.NavigateTo(Quaternion.Euler(180.0f, 0.0f, -180.0f));
            }

            // Zoom in based on relative scale factor:
            GUILayout.BeginHorizontal();
            _zoomInTextField = GUILayout.TextField(_zoomInTextField, GUILayout.Width(50.0f));
            if (GUILayout.Button("Zoom In"))
            {
                try
                {
                    float scaleFactor = float.Parse(_zoomInTextField);
                    this.Zoom(scaleFactor);
                }
                catch
                {
                    Debug.LogError("Invalid scale factor.");
                }
            }
            GUILayout.EndHorizontal();

            // Zoom out based on scale factor:
            GUILayout.BeginHorizontal();
            _zoomOutTextField = GUILayout.TextField(_zoomOutTextField, GUILayout.Width(50.0f));
            if (GUILayout.Button("Zoom Out"))
            {
                try
                {
                    float scaleFactor = float.Parse(_zoomOutTextField);
                    this.Zoom(1.0f / scaleFactor);
                }
                catch
                {
                    Debug.LogError("Invalid scale factor.");
                }
            }
            GUILayout.EndHorizontal();

            // Zoom in based on absolute scale:
            GUILayout.BeginHorizontal();
            _zoomInAbsoluteTextField = GUILayout.TextField(_zoomInAbsoluteTextField, GUILayout.Width(50.0f));
            if (GUILayout.Button("Zoom In Absolute"))
            {
                try
                {
                    float scale = float.Parse(_zoomInAbsoluteTextField);
                    float scaleFactor = _core.ViewerScale * scale;
                    this.Zoom(scaleFactor);
                }
                catch
                {
                    Debug.LogError("Invalid absolute scale.");
                }
            }
            GUILayout.EndHorizontal();

            // Zoom out based on absolute scale:
            GUILayout.BeginHorizontal();
            _zoomOutAbsoluteTextField = GUILayout.TextField(_zoomOutAbsoluteTextField, GUILayout.Width(50.0f));
            if (GUILayout.Button("Zoom Out Absolute"))
            {
                try
                {
                    float scale = float.Parse(_zoomOutAbsoluteTextField);
                    float scaleFactor = _core.ViewerScale / scale;
                    this.Zoom(scaleFactor);
                }
                catch
                {
                    Debug.LogError("Invalid absolute scale.");
                }
            }
            GUILayout.EndHorizontal();
        }


        //////////////////////////////////////////////////////////////////
        // Private Methods
        //////////////////////////////////////////////////////////////////

        /// <summary>
        /// Navigate the stereo rig such that the center of the viewport will be positioned
        /// at the specified viewport center.
        /// </summary>
        /// <param name="viewportCenter">The viewport center position in world space.</param>
        /// <returns>Tween info corresponding to the current navigation.</returns>
        private TweenInfo NavigateTo(Vector3 viewportCenter)
        {
            Quaternion viewportRotation = this.ComputeViewportRotation(_core.CurrentCameraObject);

            return this.NavigateTo(viewportCenter, viewportRotation, _core.ViewerScale);
        }

        /// <summary>
        /// Navigate the stereo rig such that the viewport will be oriented based on the specified
        /// viewport rotation.
        /// </summary>
        /// <param name="viewportRotation">The viewport rotation in world space.</param>
        /// <returns>Tween info corresponding to the current navigation.</returns>
        private TweenInfo NavigateTo(Quaternion viewportRotation)
        {
            Vector3 viewportCenter = this.ComputeViewportCenter(_core.CurrentCameraObject);

            return this.NavigateTo(viewportCenter, viewportRotation, _core.ViewerScale);
        }

        /// <summary>
        /// Navigate the stereo rig such that the viewport will be scaled based on the
        /// the specifed viewer scale.
        /// </summary>
        /// <param name="viewerScale">The viewer scale.</param>
        /// <returns>Tween info corresponding to the current navigation.</returns>
        private TweenInfo NavigateTo(float viewerScale)
        {
            Vector3 viewportCenter = this.ComputeViewportCenter(_core.CurrentCameraObject);
            Quaternion viewportRotation = this.ComputeViewportRotation(_core.CurrentCameraObject);

            return this.NavigateTo(viewportCenter, viewportRotation, viewerScale);
        }

        /// <summary>
        /// Navigate the stereo rig such that the viewport is positioned, oriented, and scaled
        /// based on the specified viewport center, viewport rotation, and viewer scale.
        /// </summary>
        /// <param name="viewportCenter">The viewport center position in world space.</param>
        /// <param name="viewportRotation">The viewport rotation in world space.</param>
        /// <param name="viewerScale">The viewer scale.</param>
        /// <returns>Tween info corresponding to the current navigation.</returns>
        private TweenInfo NavigateTo(Vector3 viewportCenter, Quaternion viewportRotation, float viewerScale)
        {
            if (_core.CurrentCameraObject == null)
            {
                return null;
            }

            if (_cameraTweenInfo != null)
            {
                this.CancelTween(_cameraTweenInfo);
                _cameraTweenInfo = null;
            }

            // Disable MinimizeLatency while animating the current camera's transform.
            _core.MinimizeLatency = false;

            // Grab the current viewport center, viewport rotation, and viewer scale.
            Vector3 currentViewportCenter = this.ComputeViewportCenter(_core.CurrentCameraObject);
            Quaternion currentViewportRotation = this.ComputeViewportRotation(_core.CurrentCameraObject);
            float currentViewerScale = _core.ViewerScale;

            Action<float> onUpdate = (t) =>
            {
                // Interpolate between the current and final values.
                Vector3 p = Vector3.Lerp(currentViewportCenter, viewportCenter, t);
                Quaternion r = Quaternion.Lerp(currentViewportRotation, viewportRotation, t);
                float v = Mathf.Lerp(currentViewerScale, viewerScale, t);

                // Compute the new camera matrix.
                Matrix4x4 cameraMatrix = this.ComputeCameraMatrix(p, r, v);

                // Update Core's current camera transform and viewer scale.
                _core.CurrentCameraObject.transform.position = cameraMatrix.GetColumn(3);
                _core.CurrentCameraObject.transform.rotation = Quaternion.LookRotation(cameraMatrix.GetColumn(2), cameraMatrix.GetColumn(1));
                _core.SetFrustumAttribute(ZCore.FrustumAttribute.ViewerScale, v);
            };

            Action onComplete = () =>
            {
                // Restore the the MinimizeLatency field to its original state.
                _core.MinimizeLatency = _isMinimizeLatencyEnabled;
            };

            _cameraTweenInfo = this.StartTween(onUpdate, 1.5f).SetEase(EaseType.EaseOutExpo);
            _cameraTweenInfo.SetOnComplete(onComplete);
            return _cameraTweenInfo;
        }

        /// <summary>
        /// Perform a zoom based on the specified relative scale factor.
        /// </summary>
        /// <param name="scaleFactor">The relative scale factor.</param>
        /// <returns>Tween info corresponding to the current zoom.</returns>
        private TweenInfo Zoom(float scaleFactor)
        {
            if (_core.CurrentCameraObject == null)
            {
                return null;
            }

            if (_cameraTweenInfo != null)
            {
                this.CancelTween(_cameraTweenInfo);
                _cameraTweenInfo = null;
            }

            // Disable MinimizeLatency while animating the current camera's transform.
            _core.MinimizeLatency = false;

            // Grab the current viewport center, camera position, and viewer scale.
            Vector3 currentViewportCenter = this.ComputeViewportCenter(_core.CurrentCameraObject);
            Vector3 currentCameraPosition = _core.CurrentCameraObject.transform.position;
            float currentViewerScale = _core.ViewerScale;

            // Calculate the final camera position and viewer scale.
            Vector3 cameraPosition = ((currentCameraPosition - currentViewportCenter) / scaleFactor) + currentViewportCenter;
            float viewerScale = currentViewerScale / scaleFactor;

            Action<float> onUpdate = (t) =>
            {
                // Interpolate between the current and final values.
                Vector3 p = Vector3.Lerp(currentCameraPosition, cameraPosition, t);
                float v = Mathf.Lerp(currentViewerScale, viewerScale, t);

                // Update Core's current camera position and viewer scale.
                _core.CurrentCameraObject.transform.position = p;
                _core.SetFrustumAttribute(ZCore.FrustumAttribute.ViewerScale, v);
            };

            Action onComplete = () =>
            {
                // Restore the the MinimizeLatency field to its original state.
                _core.MinimizeLatency = _isMinimizeLatencyEnabled;
            };

            _cameraTweenInfo = this.StartTween(onUpdate, 1.5f).SetEase(EaseType.EaseOutExpo);
            _cameraTweenInfo.SetOnComplete(onComplete);
            return _cameraTweenInfo;
        }

        /// <summary>
        /// Computes the world transform to apply to a monoscopic camera in order position
        /// and orient the stereo rig's viewport at the specified viewport center and rotation
        /// in world space.
        /// </summary>
        /// <param name="viewportCenter">The viewport center in world space.</param>
        /// <param name="viewportRotation">The viewport rotation in world space.</param>
        /// <param name="viewerScale">The viewer scale.</param>
        /// <returns>New world transform to apply to a monoscopic camera.</returns>
        private Matrix4x4 ComputeCameraMatrix(Vector3 viewportCenter, Quaternion viewportRotation, float viewerScale)
        {
            Vector3 cameraOffset = _core.GetFrustumCameraOffset();
            float angle = this.ComputeAngleBetweenCameraAndDisplay();

            Quaternion cameraRotation = viewportRotation * Quaternion.Euler(90.0f - angle, 0.0f, 0.0f);
            Vector3 cameraPosition = viewportCenter + (cameraRotation * (Vector3.back * cameraOffset.magnitude * viewerScale));

            return Matrix4x4.TRS(cameraPosition, cameraRotation, Vector3.one);
        }

        /// <summary>
        /// Computes the stereo rig's viewport center in world space based on the transform
        /// of the specified monoscopic camera.
        /// </summary>
        /// <param name="cameraObject">The reference a monoscopic camera's GameObject.</param>
        /// <returns>The viewport center in world space.</returns>
        private Vector3 ComputeViewportCenter(GameObject cameraObject)
        {
            Vector3 viewportCenter = Vector3.zero;

            if (cameraObject != null)
            {
                Vector3 cameraOffset = _core.GetFrustumCameraOffset();
                Vector3 cameraPosition = cameraObject.transform.position;
                Quaternion cameraRotation = cameraObject.transform.rotation;

                viewportCenter = cameraRotation * (Vector3.forward * cameraOffset.magnitude * _core.ViewerScale) + cameraPosition;
            }

            return viewportCenter;
        }

        /// <summary>
        /// Computes the stereo rig's viewport rotation in world space based on the transform
        /// of the specified monoscopic camera.
        /// </summary>
        /// <param name="cameraObject">The reference a monoscopic camera's GameObject.</param>
        /// <returns>The viewport rotation in world space.</returns>
        private Quaternion ComputeViewportRotation(GameObject cameraObject)
        {
            Quaternion viewportRotation = Quaternion.identity;

            if (cameraObject != null)
            {
                float angle = this.ComputeAngleBetweenCameraAndDisplay();
                Quaternion cameraRotation = cameraObject.transform.rotation;

                viewportRotation = cameraRotation * Quaternion.Inverse(Quaternion.Euler(90.0f - angle, 0.0f, 0.0f));
            }

            return viewportRotation;
        }

        /// <summary>
        /// Computes the angle between the camera offset vector and the display plane.
        /// </summary>
        /// <returns>The angle in degrees.</returns>
        private float ComputeAngleBetweenCameraAndDisplay()
        {
            Vector3 cameraOffset = _core.GetFrustumCameraOffset();
            Vector3 displayAngle = _core.GetDisplayAngle();
            Vector3 displayDirection = Quaternion.Euler(-displayAngle.x, 0.0f, 0.0f) * Vector3.forward;

            return Vector3.Angle(cameraOffset.normalized, displayDirection.normalized);
        }

        /// <summary>
        /// Computes a normalized time (between 0.0 and 1.0) based on a current time,
        /// duration, and ease type.
        /// </summary>
        /// <param name="t">Current time in seconds.</param>
        /// <param name="d">Duration in seconds.</param>
        /// <param name="ease">Ease type.</param>
        /// <returns>Normalized time between 0.0 and 1.0 (inclusive).</returns>
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

        /// <summary>
        /// Starts a tween.
        /// </summary>
        /// <param name="onUpdate">Update callback invoked evert frame the tween is active.</param>
        /// <param name="duration">Duration of the tween in seconds.</param>
        /// <returns></returns>
        private TweenInfo StartTween(Action<float> onUpdate, float duration)
        {
            TweenInfo info = new TweenInfo(onUpdate);
            info.Duration = duration;

            _tweenInfos.Add(info);

            return info;
        }

        /// <summary>
        /// Cancels an existing tween that is being updated.
        /// </summary>
        /// <param name="info">Reference to the tween.</param>
        private void CancelTween(TweenInfo info)
        {
            if (_tweenInfos.Contains(info))
            {
                _tweenInfos.Remove(info);
            }
        }

        /// <summary>
        /// Updates all existing tweens.
        /// </summary>
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


        //////////////////////////////////////////////////////////////////
        // Private Enums
        //////////////////////////////////////////////////////////////////

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

        /// <summary>
        /// Class providing very basic tweening support.
        /// </summary>
        private class TweenInfo
        {
            public Action<float> OnUpdate { get; private set; }
            public Action OnComplete { get; private set; }
            public float Delay { get; set; }
            public float Duration { get; set; }
            public float Time { get; set; }
            public EaseType Ease { get; set; }

            public TweenInfo()
            {
                this.OnUpdate = null;
                this.OnComplete = null;
                this.Delay = 0.0f;
                this.Duration = 0.0f;
                this.Time = 0.0f;
                this.Ease = EaseType.Linear;
            }

            public TweenInfo(Action<float> onUpdate)
            {
                this.OnUpdate = onUpdate;
                this.OnComplete = null;
                this.Delay = 0.0f;
                this.Duration = 0.0f;
                this.Time = 0.0f;
                this.Ease = EaseType.Linear;
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

        private ZCore _core = null;
        private bool _isMinimizeLatencyEnabled = false;

        private List<TweenInfo> _tweenInfos = new List<TweenInfo>();
        private TweenInfo _cameraTweenInfo  = null;

        private string _zoomInTextField          = "2.0";
        private string _zoomOutTextField         = "2.0";
        private string _zoomInAbsoluteTextField  = "4.0";
        private string _zoomOutAbsoluteTextField = "4.0";
    }
}
