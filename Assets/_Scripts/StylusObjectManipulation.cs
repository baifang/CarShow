//////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2016 zSpace, Inc.  All Rights Reserved.
//
//////////////////////////////////////////////////////////////////////////

using HighlightingSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace zSpace.Core.Samples
{
    public class StylusObjectManipulation : MonoBehaviour
    {
        //private static readonly float DEFAULT_STYLUS_BEAM_WIDTH = 0.0002f;
        //private static readonly float DEFAULT_STYLUS_BEAM_LENGTH = 0.3f;
        private static readonly float DEFAULT_STYLUS_BEAM_WIDTH = 0.0008f;
        private static readonly float DEFAULT_STYLUS_BEAM_LENGTH = 0.3f;
        private ZCore _core = null;
        private bool _wasButtonPressed = false;

        private GameObject _stylusBeamObject = null;
        private LineRenderer _stylusBeamRenderer = null;
        private float _stylusBeamLength = DEFAULT_STYLUS_BEAM_LENGTH;

        private StylusState _stylusState = StylusState.Idle;

        private SceneManage sceneManage;
        private GameObject _grabObject = null;
        private Vector3 _initialGrabOffset = Vector3.zero;
        private Quaternion _initialGrabRotation = Quaternion.identity;
        private float _initialGrabDistance = 0.0f;

        public bool isGrabAll = false;//是否抓取整个物体

        private GameObject currentHoverPart = null;//当前悬浮的物体部位。
        private GameObject lastHoverPart = null;

        private enum StylusState
        {
            Idle = 0,
            Grab = 1,
        }
        void Start()
        {
            sceneManage = GameObject.Find("PutScripts").GetComponent<SceneManage>();
            _core = GameObject.FindObjectOfType<ZCore>();
            if (_core == null)
            {
                this.enabled = false;
                return;
            }
            // 建立zSpace操作笔模型
            _stylusBeamObject = new GameObject("StylusBeam");
            _stylusBeamRenderer = _stylusBeamObject.AddComponent<LineRenderer>();
            _stylusBeamRenderer.material = new Material(Shader.Find("Transparent/Diffuse"));
            _stylusBeamRenderer.SetColors(Color.white, Color.white);
        }

        void Update()
        {
            ZCore.Pose pose = _core.GetTargetPose(ZCore.TargetType.Primary, ZCore.CoordinateSpace.World);
            bool isButtonPressed = _core.IsTargetButtonPressed(ZCore.TargetType.Primary, 0) || _core.IsTargetButtonPressed(ZCore.TargetType.Primary, 1);
            switch (_stylusState)
            {
                case StylusState.Idle:
                    {
                        _stylusBeamLength = DEFAULT_STYLUS_BEAM_LENGTH;
                        sceneManage.CurrentGrabPart = null;
                        //对整个场景进行碰撞检测确定与笔进行碰撞的物体
                        RaycastHit hit;
                        if (Physics.Raycast(pose.Position, pose.Direction, out hit))
                        {
                            // 更新zSpace操作笔的长度
                            _stylusBeamLength = hit.distance / _core.ViewerScale;

                            // 如果zSpace触控笔主按钮按下，开始抓取物体。
                            if (isButtonPressed && !_wasButtonPressed)
                            {
                                // 开始抓取
                                this.BeginGrab(hit.collider.gameObject, hit.distance, pose.Position, pose.Rotation);
                                _stylusState = StylusState.Grab;

                                if (lastHoverPart != null && lastHoverPart.GetComponent<FlashingController>() != null)
                                {
                                    Destroy(lastHoverPart.GetComponent<FlashingController>());
                                    Destroy(lastHoverPart.GetComponent<Highlighter>());
                                    lastHoverPart = null;
                                }
                            }
                            else
                            {
                                currentHoverPart = hit.collider.gameObject;
                                if (lastHoverPart != currentHoverPart)
                                {
                                    if (lastHoverPart != null && lastHoverPart.GetComponent<FlashingController>() != null)
                                    {
                                        Destroy(lastHoverPart.GetComponent<FlashingController>());
                                        Destroy(lastHoverPart.GetComponent<Highlighter>());
                                    }
                                    lastHoverPart = currentHoverPart;
                                    FlashingController fla= currentHoverPart.AddComponent<FlashingController>();
                                    fla.flashingDelay = 0;
                                    fla.flashingFrequency = 1;
                                }
                            }
                        }
                        else
                        {
                            if (lastHoverPart != null && lastHoverPart.GetComponent<FlashingController>() != null)
                            {
                                Destroy(lastHoverPart.GetComponent<FlashingController>());
                                Destroy(lastHoverPart.GetComponent<Highlighter>());
                                lastHoverPart = null;
                            } 
                        }
                    }
                    break;

                case StylusState.Grab:
                    {
                        // Update the grab.
                        this.UpdateGrab(pose.Position, pose.Rotation);

                        // End the grab if the front stylus button was released.
                        if (!isButtonPressed && _wasButtonPressed)
                        {
                            _stylusState = StylusState.Idle;
                        }
                    }
                    break;

                default:
                    break;
            }
            // 更新zSpace操作笔的位置以及旋转参数。
            this.UpdateStylusBeam(pose.Position, pose.Direction);

            // 为下一帧缓存点击状态.
            _wasButtonPressed = isButtonPressed;
        }

        private void BeginGrab(GameObject hitObject, float hitDistance, Vector3 inputPosition, Quaternion inputRotation)
        {
            Vector3 inputEndPosition = inputPosition + (inputRotation * (Vector3.forward * hitDistance));
            isGrabAll = _core.IsTargetButtonPressed(ZCore.TargetType.Primary, 1) == true ? true : false;
            // 保存初始抓取状态数据
            if (isGrabAll)
            {
                //("全部抓取模式");
                _grabObject = hitObject.transform.root.gameObject;
            }
            else
            {
                _grabObject = hitObject;
                sceneManage.CurrentGrabPart = _grabObject;
            }
            _initialGrabOffset = Quaternion.Inverse(_grabObject.transform.rotation) * (_grabObject.transform.position - inputEndPosition);
            _initialGrabRotation = Quaternion.Inverse(inputRotation) * _grabObject.transform.rotation;
            _initialGrabDistance = hitDistance;
        }

        private void UpdateGrab(Vector3 inputPosition, Quaternion inputRotation)
        {
            Vector3 inputEndPosition = inputPosition + (inputRotation * (Vector3.forward * _initialGrabDistance));

            // Update the grab object's rotation.
            Quaternion objectRotation = inputRotation * _initialGrabRotation;
            _grabObject.transform.rotation = objectRotation;
            // Update the grab object's position.
            Vector3 objectPosition = inputEndPosition + (objectRotation * _initialGrabOffset);
            _grabObject.transform.position = objectPosition;
        }
        /// <summary>
        /// 更新操作笔的参数
        /// </summary>
        /// <param name="stylusPosition"></param>
        /// <param name="stylusDirection"></param>
        private void UpdateStylusBeam(Vector3 stylusPosition, Vector3 stylusDirection)
        {
            if (_stylusBeamRenderer != null)
            {
                float stylusBeamWidth = DEFAULT_STYLUS_BEAM_WIDTH * _core.ViewerScale;
                float stylusBeamLength = _stylusBeamLength * _core.ViewerScale;

                _stylusBeamRenderer.SetWidth(stylusBeamWidth, stylusBeamWidth);
                _stylusBeamRenderer.SetPosition(0, stylusPosition);
                _stylusBeamRenderer.SetPosition(1, stylusPosition + (stylusDirection * stylusBeamLength));
            }
        }

        void AddHeghLightCompound()
        {
            if (currentHoverPart == null)
            {
                if (lastHoverPart != null && lastHoverPart.GetComponent<FlashingController>() != null)
                {
                    Destroy(lastHoverPart.GetComponent<FlashingController>());
                    Destroy(lastHoverPart.GetComponent<Highlighter>());
                }
            }
            else
            {
                currentHoverPart.AddComponent<FlashingController>();
                if (lastHoverPart != currentHoverPart)
                {
                    if (lastHoverPart != null)
                    {
                        Destroy(lastHoverPart.GetComponent<FlashingController>());
                        Destroy(lastHoverPart.GetComponent<Highlighter>());
                    }
                    else
                    {
                        lastHoverPart = currentHoverPart;
                    }
                }
            }
        }

    }
}