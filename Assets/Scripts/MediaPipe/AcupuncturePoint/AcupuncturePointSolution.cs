// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using pbc = global::Google.Protobuf.Collections;

namespace Mediapipe.Unity.Holistic
{
    public class AcupuncturePointSolution : ImageSourceSolution<HolisticTrackingGraph>
    {
        [SerializeField] private InventorySO AcupuncturePointInventory;
        [SerializeField] private RectTransform _worldAnnotationArea;
        [SerializeField] private DetectionAnnotationController _poseDetectionAnnotationController;
        [SerializeField] private HolisticLandmarkListAnnotationController _holisticAnnotationController;
        [SerializeField] private PoseWorldLandmarkListAnnotationController _poseWorldLandmarksAnnotationController;
        [SerializeField] private MaskAnnotationController _segmentationMaskAnnotationController;
        [SerializeField] private NormalizedRectAnnotationController _poseRoiAnnotationController;

        public HolisticTrackingGraph.ModelComplexity modelComplexity
        {
            get => graphRunner.modelComplexity;
            set => graphRunner.modelComplexity = value;
        }

        public bool smoothLandmarks
        {
            get => graphRunner.smoothLandmarks;
            set => graphRunner.smoothLandmarks = value;
        }

        public bool refineFaceLandmarks
        {
            get => graphRunner.refineFaceLandmarks;
            set => graphRunner.refineFaceLandmarks = value;
        }

        public bool enableSegmentation
        {
            get => graphRunner.enableSegmentation;
            set => graphRunner.enableSegmentation = value;
        }

        public bool smoothSegmentation
        {
            get => graphRunner.smoothSegmentation;
            set => graphRunner.smoothSegmentation = value;
        }

        public float minDetectionConfidence
        {
            get => graphRunner.minDetectionConfidence;
            set => graphRunner.minDetectionConfidence = value;
        }

        public float minTrackingConfidence
        {
            get => graphRunner.minTrackingConfidence;
            set => graphRunner.minTrackingConfidence = value;
        }

        protected override void SetupScreen(ImageSource imageSource)
        {
            base.SetupScreen(imageSource);
            _worldAnnotationArea.localEulerAngles = imageSource.rotation.Reverse().GetEulerAngles();
        }

        protected override void OnStartRun()
        {
            if (!runningMode.IsSynchronous())
            {
                //graphRunner.OnPoseDetectionOutput += OnPoseDetectionOutput;
                graphRunner.OnFaceLandmarksOutput += OnFaceLandmarksOutput;
                //graphRunner.OnPoseLandmarksOutput += OnPoseLandmarksOutput;
                graphRunner.OnLeftHandLandmarksOutput += OnLeftHandLandmarksOutput;
                graphRunner.OnRightHandLandmarksOutput += OnRightHandLandmarksOutput;
                //graphRunner.OnPoseWorldLandmarksOutput += OnPoseWorldLandmarksOutput;
               // graphRunner.OnSegmentationMaskOutput += OnSegmentationMaskOutput;
                //graphRunner.OnPoseRoiOutput += OnPoseRoiOutput;
            }

            var imageSource = ImageSourceProvider.ImageSource;
            //SetupAnnotationController(_poseDetectionAnnotationController, imageSource);
            SetupAnnotationController(_holisticAnnotationController, imageSource);
            //SetupAnnotationController(_poseWorldLandmarksAnnotationController, imageSource);
            //SetupAnnotationController(_segmentationMaskAnnotationController, imageSource);
            //_segmentationMaskAnnotationController.InitScreen(imageSource.textureWidth, imageSource.textureHeight);
            //SetupAnnotationController(_poseRoiAnnotationController, imageSource);
        }

        protected override void AddTextureFrameToInputStream(TextureFrame textureFrame)
        {
            graphRunner.AddTextureFrameToInputStream(textureFrame);
        }

        protected override IEnumerator WaitForNextValue()
        {
            NormalizedLandmarkList faceLandmarks = null;
            NormalizedLandmarkList leftHandLandmarks = null;
            NormalizedLandmarkList rightHandLandmarks = null;

            if (runningMode == RunningMode.Sync)
            {
                var _ = graphRunner.TryGetNext(out faceLandmarks, out leftHandLandmarks, out rightHandLandmarks, true);
            }
            else if (runningMode == RunningMode.NonBlockingSync)
            {
                yield return new WaitUntil(() =>
                  graphRunner.TryGetNext(out faceLandmarks, out leftHandLandmarks, out rightHandLandmarks, false));
            }
            IList<NormalizedLandmark> _currentLeftHandLandmarkList = leftHandLandmarks?.Landmark;
            pbc::RepeatedField<NormalizedLandmark> _newLeftHandLandarkList = new pbc::RepeatedField<NormalizedLandmark>();
            if(_currentLeftHandLandmarkList != null)
            {
                foreach (var itemStack in AcupuncturePointInventory.Items)
                {
                    NormalizedLandmark landmark = new NormalizedLandmark( _currentLeftHandLandmarkList[itemStack.Item.LandMark]);
                    landmark.X += itemStack.Item.Offest.x / 100;
                    landmark.Y += itemStack.Item.Offest.y / 100;

                    _newLeftHandLandarkList.Add(landmark);
                }
            }
            
            _holisticAnnotationController.DrawNow(faceLandmarks, null, _newLeftHandLandarkList, rightHandLandmarks);
            
        }

        private void OnPoseDetectionOutput(object stream, OutputEventArgs<Detection> eventArgs)
        {
            _poseDetectionAnnotationController.DrawLater(eventArgs.value);
        }

        private void OnFaceLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs)
        {
            _holisticAnnotationController.DrawFaceLandmarkListLater(eventArgs.value);
        }

        private void OnPoseLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs)
        {
            _holisticAnnotationController.DrawPoseLandmarkListLater(eventArgs.value);
        }

        private void OnLeftHandLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs)
        {
            _holisticAnnotationController.DrawLeftHandLandmarkListLater(eventArgs.value);
        }

        private void OnRightHandLandmarksOutput(object stream, OutputEventArgs<NormalizedLandmarkList> eventArgs)
        {
            _holisticAnnotationController.DrawRightHandLandmarkListLater(eventArgs.value);
        }

        private void OnPoseWorldLandmarksOutput(object stream, OutputEventArgs<LandmarkList> eventArgs)
        {
            _poseWorldLandmarksAnnotationController.DrawLater(eventArgs.value);
        }

        private void OnSegmentationMaskOutput(object stream, OutputEventArgs<ImageFrame> eventArgs)
        {
            _segmentationMaskAnnotationController.DrawLater(eventArgs.value);
        }

        private void OnPoseRoiOutput(object stream, OutputEventArgs<NormalizedRect> eventArgs)
        {
            _poseRoiAnnotationController.DrawLater(eventArgs.value);
        }
    }
}
