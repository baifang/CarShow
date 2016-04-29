//////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2016 zSpace, Inc.  All Rights Reserved.
//
//////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;

using UnityEngine;


namespace zSpace.Core
{
    public class StereoCamera : MonoBehaviour 
    {
        //////////////////////////////////////////////////////////////////
        // Events
        //////////////////////////////////////////////////////////////////

        public delegate void EventHandler(StereoCamera sender);

        public event EventHandler PreCull;
        public event EventHandler PreRender;
        public event EventHandler PostRender;


        //////////////////////////////////////////////////////////////////
        // Unity Inspector Fields
        //////////////////////////////////////////////////////////////////

        public ZCore.Eye Eye;


        //////////////////////////////////////////////////////////////////
        // Unity Monobehaviour Callbacks
        //////////////////////////////////////////////////////////////////

        void OnPreCull()
        {
            if (this.PreCull != null)
            {
                this.PreCull(this);
            }

            switch (this.Eye)
            {
                case ZCore.Eye.Left:
                    ZCore.IssuePluginEvent(ZCore.PluginEvent.SetRenderTargetLeft);
                    break;
                case ZCore.Eye.Right:
                    ZCore.IssuePluginEvent(ZCore.PluginEvent.SetRenderTargetRight);
                    break;
                case ZCore.Eye.Center:
                    ZCore.IssuePluginEvent(ZCore.PluginEvent.FrameDone);
                    ZCore.IssuePluginEvent(ZCore.PluginEvent.DisableStereo);
                    GL.InvalidateState();

                    // Disable the Unity camera to prevent the center stereo camera
                    // from rendering.
                    this.SetCameraEnabled(false);
                    break;
                default:
                    break;
            }
        }

        void OnPreRender()
        {
            if (this.PreRender != null)
            {
                this.PreRender(this);
            }
        }

        void OnPostRender()
        {
            if (this.PostRender != null)
            {
                this.PostRender(this);
            }
        }


        //////////////////////////////////////////////////////////////////
        // Public API
        //////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns a reference to the associated Unity camera.
        /// </summary>
        public Camera GetCamera()
        {
            return this.GetComponent<Camera>();
        }

        /// <summary>
        /// Set whether the camera is enabled.
        /// </summary>
        public void SetCameraEnabled(bool isEnabled)
        {
            this.GetComponent<Camera>().enabled = isEnabled;
        }

        /// <summary>
        /// Set the camera's near clip distance (in meters).
        /// </summary>
        public void SetCameraNearClip(float nearClip)
        {
            this.GetComponent<Camera>().nearClipPlane = nearClip;
        }

        /// <summary>
        /// Set the camera's far clip distance (in meters).
        /// </summary>
        public void SetCameraFarClip(float farClip)
        {
            this.GetComponent<Camera>().farClipPlane = farClip;
        }

        /// <summary>
        /// Set the camera's depth (AKA rendering order).
        /// </summary>
        public void SetCameraDepth(float depth)
        {
            this.GetComponent<Camera>().depth = depth;
        }

        /// <summary>
        /// Set the stereo camera's view matrix.
        /// NOTE: This must be in Unity's left-hand coordinate system.
        /// </summary>
        public void SetViewMatrix(Matrix4x4 viewMatrix)
        {
            Matrix4x4 inverseViewMatrix = viewMatrix.inverse;

            this.transform.localPosition = inverseViewMatrix.GetColumn(3);
            this.transform.localRotation = Quaternion.LookRotation(inverseViewMatrix.GetColumn(2), inverseViewMatrix.GetColumn(1));
        }

        /// <summary>
        /// Set the stereo camera's projection matrix.
        /// NOTE: This must be right-handed due to coordinate system
        ///       inconsistencies in Unity.
        /// </summary>
        public void SetProjectionMatrix(Matrix4x4 projectionMatrix)
        {
            this.GetComponent<Camera>().projectionMatrix = projectionMatrix;
        }
    }
}

