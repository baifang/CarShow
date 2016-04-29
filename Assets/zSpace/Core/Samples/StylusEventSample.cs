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
    public class StylusEventSample : MonoBehaviour
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

            // Register event handlers.
            _core.TargetMove          += HandleMove;
            _core.TargetButtonPress   += HandleButtonPress;
            _core.TargetButtonRelease += HandleButtonRelease;
            _core.TargetTapPress      += HandleTapPress;
            _core.TargetTapRelease    += HandleTapRelease;
        }

        void OnGUI()
        {
            _logMoveEvents   = GUILayout.Toggle(_logMoveEvents, "Log Move Events");
            _logButtonEvents = GUILayout.Toggle(_logButtonEvents, "Log Button Events");
            _logTapEvents    = GUILayout.Toggle(_logTapEvents, "Log Tap Events");
        }

        void OnDestroy()
        {
            // Unregister event handlers.
            _core.TargetMove          -= HandleMove;
            _core.TargetButtonPress   -= HandleButtonPress;
            _core.TargetButtonRelease -= HandleButtonRelease;
            _core.TargetTapPress      -= HandleTapPress;
            _core.TargetTapRelease    -= HandleTapRelease;
        }


        //////////////////////////////////////////////////////////////////
        // Event Handlers
        //////////////////////////////////////////////////////////////////

        private void HandleMove(ZCore sender, ZCore.TrackerEventInfo info)
        {
            if (!_logMoveEvents)
            {
                return;
            }

            if (info.TargetType == ZCore.TargetType.Primary)
            {
                Debug.Log(
                    string.Format(
                        "<color=blue>Stylus Moved:</color> Position {0}, Rotation {1}", 
                        info.WorldPose.Position, 
                        info.WorldPose.Rotation.eulerAngles));
            }
        }

        private void HandleButtonPress(ZCore sender, ZCore.TrackerButtonEventInfo info)
        {
            if (!_logButtonEvents)
            {
                return;
            }

            if (info.TargetType == ZCore.TargetType.Primary)
            {
                Debug.Log(
                    string.Format(
                        "<color=green>Stylus Button Pressed:</color> {0}", 
                        info.ButtonId));
            }
        }

        private void HandleButtonRelease(ZCore sender, ZCore.TrackerButtonEventInfo info)
        {
            if (!_logButtonEvents)
            {
                return;
            }

            if (info.TargetType == ZCore.TargetType.Primary)
            {
                Debug.Log(
                    string.Format(
                        "<color=green>Stylus Button Released:</color> {0}", 
                        info.ButtonId));
            }
        }

        private void HandleTapPress(ZCore sender, ZCore.TrackerEventInfo info)
        {
            if (!_logTapEvents)
            {
                return;
            }

            if (info.TargetType == ZCore.TargetType.Primary)
            {
                Debug.Log(string.Format("<color=purple>Stylus Tap Pressed</color>"));
            }
        }

        private void HandleTapRelease(ZCore sender, ZCore.TrackerEventInfo info)
        {
            if (!_logTapEvents)
            {
                return;
            }

            if (info.TargetType == ZCore.TargetType.Primary)
            {
                Debug.Log(string.Format("<color=purple>Stylus Tap Released</color>"));
            }
        }


        //////////////////////////////////////////////////////////////////
        // Private Members
        //////////////////////////////////////////////////////////////////

        private ZCore _core = null;

        private bool _logMoveEvents   = false;
        private bool _logButtonEvents = true;
        private bool _logTapEvents    = true;
    }
}