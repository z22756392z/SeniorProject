// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using pbc = global::Google.Protobuf.Collections;

namespace Mediapipe.Unity.HandTracking
{
    public class AcupuncturePointHandSolution : ImageSourceSolution<HandTrackingGraph>
    {
        [SerializeField] private InventorySO _handAcupunturePointsInventory = default;
        [SerializeField] private DetectionListAnnotationController _palmDetectionsAnnotationController;
        [SerializeField] private NormalizedRectListAnnotationController _handRectsFromPalmDetectionsAnnotationController;
        [SerializeField] private MultiHandLandmarkListAnnotationController _handLandmarksAnnotationController;
        [SerializeField] private NormalizedRectListAnnotationController _handRectsFromLandmarksAnnotationController;
        [Header("Listening on channel")]
        [SerializeField] private ListStringEventChannelSO _aiShowAupunturePoints = default;
        [SerializeField] private ListLocalizedStringEventChannelSO _showAcpunturePoints = default;
        [HideInInspector][SerializeField] private List<LocalizedString> _desireAcpunturePoints;
        private List<string> _aiDesireAcpunturePoints;
        private bool _isFirstTime1 = true;
        private bool _isFirstTime2 = true;
        private bool _isShowDesireAcpunturePoint = false;
        private bool _aiIsShowDesireAcpunturePoint = false;

        private void OnEnable()
        {
            if(_showAcpunturePoints != null)
            _showAcpunturePoints.OnEventRaised += SetAcpunturePoints;
            if (_aiShowAupunturePoints != null)
                _aiShowAupunturePoints.OnEventRaised += AISetAcpunturePoints;
        }

        private void OnDisable()
        {
            if (_showAcpunturePoints != null)
                _showAcpunturePoints.OnEventRaised -= SetAcpunturePoints;
            if (_aiShowAupunturePoints != null)
                _aiShowAupunturePoints.OnEventRaised -= AISetAcpunturePoints;
        }

        void SetAcpunturePoints(List<LocalizedString> value)
        {
            _isShowDesireAcpunturePoint = true;
            _desireAcpunturePoints = value;
        }

        void AISetAcpunturePoints(List<string> value)
        {
            _aiDesireAcpunturePoints = new List<string>();
            _aiIsShowDesireAcpunturePoint = true;
            foreach (var item in value)
            {
                _aiDesireAcpunturePoints.Add(item);
            }
        }

        public HandTrackingGraph.ModelComplexity modelComplexity
        {
            get => graphRunner.modelComplexity;
            set => graphRunner.modelComplexity = value;
        }

        public int maxNumHands
        {
            get => graphRunner.maxNumHands;
            set => graphRunner.maxNumHands = value;
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

        protected override void OnStartRun()
        {
            if (!runningMode.IsSynchronous())
            {
                //graphRunner.OnPalmDetectectionsOutput += OnPalmDetectionsOutput;
                //graphRunner.OnHandRectsFromPalmDetectionsOutput += OnHandRectsFromPalmDetectionsOutput;
                graphRunner.OnHandLandmarksOutput += OnHandLandmarksOutput;
                // TODO: render HandWorldLandmarks annotations
                //graphRunner.OnHandRectsFromLandmarksOutput += OnHandRectsFromLandmarksOutput;
                //graphRunner.OnHandednessOutput += OnHandednessOutput;
            }

            var imageSource = ImageSourceProvider.ImageSource;
            //SetupAnnotationController(_palmDetectionsAnnotationController, imageSource, true);
            //SetupAnnotationController(_handRectsFromPalmDetectionsAnnotationController, imageSource, true);
            SetupAnnotationController(_handLandmarksAnnotationController, imageSource, true);
            //SetupAnnotationController(_handRectsFromLandmarksAnnotationController, imageSource, true);
        }

        protected override void AddTextureFrameToInputStream(TextureFrame textureFrame)
        {
            graphRunner.AddTextureFrameToInputStream(textureFrame);
        }

        protected override IEnumerator WaitForNextValue()
        {
            //List<Detection> palmDetections = null;
            //List<NormalizedRect> handRectsFromPalmDetections = null;
            List<NormalizedLandmarkList> handLandmarks = null;
            //List<LandmarkList> handWorldLandmarks = null;
            //List<NormalizedRect> handRectsFromLandmarks = null;
            List<ClassificationList> handedness = null;

            if (runningMode == RunningMode.Sync)
            {
                var _ = graphRunner.TryGetNext(out handLandmarks, out handedness, true);
                //var _ = graphRunner.TryGetNext(out palmDetections, out handRectsFromPalmDetections, out handLandmarks, out handWorldLandmarks, out handRectsFromLandmarks, out handedness, true);
            }
            else if (runningMode == RunningMode.NonBlockingSync)
            {
                yield return new WaitUntil(() => graphRunner.TryGetNext(out handLandmarks, out handedness, false));
                //yield return new WaitUntil(() => graphRunner.TryGetNext(out palmDetections, out handRectsFromPalmDetections, out handLandmarks, out handWorldLandmarks, out handRectsFromLandmarks, out handedness, false));
            }

            IList<NormalizedLandmark> _currentHandLandmark1 = handLandmarks?[0]?.Landmark;
            IList<NormalizedLandmark> _currentHandLandmark2 = null;
            if (handLandmarks != null && handLandmarks.Count > 1)
                _currentHandLandmark2 = handLandmarks?[1]?.Landmark;
            pbc::RepeatedField<NormalizedLandmarkList> _newHandLandarkList = new pbc::RepeatedField<NormalizedLandmarkList>();

            if (_isFirstTime1 && _currentHandLandmark1 != null)
            {
                NormalizedLandmarkList normalizedLandmarkList = new NormalizedLandmarkList();
                _isFirstTime1 = false;
                foreach (var itemStack in _handAcupunturePointsInventory.Items)
                {
                    NormalizedLandmark landmark = new NormalizedLandmark(_currentHandLandmark1[itemStack.Item.LandMark]);
                    landmark.X += itemStack.Item.Offest.x / 100;
                    landmark.Y += itemStack.Item.Offest.y / 100;
                    normalizedLandmarkList.Landmark.Add(landmark);
                }
                _newHandLandarkList.Add(normalizedLandmarkList);
                if (_isFirstTime2 && _currentHandLandmark2 != null)
                {
                    NormalizedLandmarkList normalizedLandmarkList2 = new NormalizedLandmarkList();
                    _isFirstTime2 = false;
                    foreach (var itemStack in _handAcupunturePointsInventory.Items)
                    {
                        NormalizedLandmark landmark = new NormalizedLandmark(_currentHandLandmark2[itemStack.Item.LandMark]);
                        landmark.X += itemStack.Item.Offest.x / 100;
                        landmark.Y += itemStack.Item.Offest.y / 100;
                        normalizedLandmarkList2.Landmark.Add(landmark);
                    }
                    _newHandLandarkList.Add(normalizedLandmarkList2);
                }
                _handLandmarksAnnotationController.DrawNow(_newHandLandarkList, handedness);
                yield break;
            }
            if (_isFirstTime2 && _currentHandLandmark2 != null)
            {
                NormalizedLandmarkList normalizedLandmarkList = new NormalizedLandmarkList();
                _isFirstTime2 = false;
                foreach (var itemStack in _handAcupunturePointsInventory.Items)
                {
                    NormalizedLandmark landmark = new NormalizedLandmark(_currentHandLandmark1[itemStack.Item.LandMark]);
                    landmark.X += itemStack.Item.Offest.x / 100;
                    landmark.Y += itemStack.Item.Offest.y / 100;
                    normalizedLandmarkList.Landmark.Add(landmark);
                }
                _handLandmarksAnnotationController.DrawNow(_newHandLandarkList, handedness);
                yield break;
            }
            if(_currentHandLandmark1 != null && handedness != null)
            {
                _newHandLandarkList.Add(Detect(_currentHandLandmark1, handedness[0].Classification[0].Label));
            }
                
            if (_currentHandLandmark2 != null && handedness != null && handedness.Count > 1)
                _newHandLandarkList.Add(Detect(_currentHandLandmark2, handedness[1].Classification[0].Label));


            //Debug uses
            /*
            if (_currentHandLandmark1 != null)
            {
                NormalizedLandmarkList normalizedLandmarkList = new NormalizedLandmarkList();
                foreach (var itemStack in _handAcupunturePointsInventory.Items)
                {
                    NormalizedLandmark landmark = new NormalizedLandmark(_currentHandLandmark1[itemStack.Item.LandMark]);
                    landmark.X += itemStack.Item.Offest.x / 100;
                    landmark.Y += itemStack.Item.Offest.y / 100;
                    normalizedLandmarkList.Landmark.Add(landmark);
                }
                _newHandLandarkList.Add(normalizedLandmarkList);
               
            }
            if (_currentHandLandmark2 != null)
            {
                NormalizedLandmarkList normalizedLandmarkList2 = new NormalizedLandmarkList();
                foreach (var itemStack in _handAcupunturePointsInventory.Items)
                {
                    NormalizedLandmark landmark = new NormalizedLandmark(_currentHandLandmark2[itemStack.Item.LandMark]);
                    landmark.X += itemStack.Item.Offest.x / 100;
                    landmark.Y += itemStack.Item.Offest.y / 100;
                    normalizedLandmarkList2.Landmark.Add(landmark);
                }
                _newHandLandarkList.Add(normalizedLandmarkList2);
            }*/

            _handLandmarksAnnotationController.DrawNow(_newHandLandarkList, handedness);
        }

        private NormalizedLandmarkList Detect(IList<NormalizedLandmark> curHandLankmark, string handedness)
        {
            if (curHandLankmark == null) return default;
            if (handedness[0] == 'L')
            {
                if (curHandLankmark[0].X > curHandLankmark[17].X)
                {
                    return HandFacingDetect(curHandLankmark, 3);
                }
                else
                {
                    return HandFacingDetect(curHandLankmark,4);
                }
            }
            else if(handedness[0] =='R')
            {
                if (curHandLankmark[0].X > curHandLankmark[17].X)
                {
                    return HandFacingDetect(curHandLankmark, 6);
                }
                else
                {
                    return HandFacingDetect(curHandLankmark, 5);
                }
            }
            return default;
        }
       
        NormalizedLandmarkList HandFacingDetect(IList<NormalizedLandmark> curHandLankmark, int Custom)
        {
            int i = 0;
            NormalizedLandmarkList normalizedLandmarkList = new NormalizedLandmarkList();
            foreach (var itemStack in _handAcupunturePointsInventory.Items)
            {
                if (itemStack.Item.Customize == Custom)
                {
                    if (_isShowDesireAcpunturePoint)
                    {
                        string key = itemStack.Item.Name.TableEntryReference.Key;
                        if (_desireAcpunturePoints.Find(o => o.TableEntryReference.Key == key) != null)
                        {
                            NormalizedLandmark landmark = new NormalizedLandmark(curHandLankmark[itemStack.Item.LandMark]);
                            landmark.X += itemStack.Item.Offest.x / 100;
                            landmark.Y += itemStack.Item.Offest.y / 100;
                            landmark.index = i;
                            normalizedLandmarkList.Landmark.Add(landmark);
                        }
                    }
                    else if (_aiIsShowDesireAcpunturePoint)
                    {
                        string localizeString = itemStack.Item.Name.GetLocalizedString();
                        Debug.Log(_aiDesireAcpunturePoints[0] + " " + localizeString);
                        if (_aiDesireAcpunturePoints.Find(o => o == localizeString) != null)
                        {
                            NormalizedLandmark landmark = new NormalizedLandmark(curHandLankmark[itemStack.Item.LandMark]);
                            landmark.X += itemStack.Item.Offest.x / 100;
                            landmark.Y += itemStack.Item.Offest.y / 100;
                            landmark.index = i;
                            normalizedLandmarkList.Landmark.Add(landmark);
                        }
                    }
                    else
                    {
                        
                        NormalizedLandmark landmark = new NormalizedLandmark(curHandLankmark[itemStack.Item.LandMark]);
                        landmark.X += itemStack.Item.Offest.x / 100;
                        landmark.Y += itemStack.Item.Offest.y / 100;
                        landmark.index = i;
                        normalizedLandmarkList.Landmark.Add(landmark);
                    }
                }
                i++;
            }
            
            return normalizedLandmarkList;
        }

        private void OnPalmDetectionsOutput(object stream, OutputEventArgs<List<Detection>> eventArgs)
        {
            _palmDetectionsAnnotationController.DrawLater(eventArgs.value);
        }

        private void OnHandRectsFromPalmDetectionsOutput(object stream, OutputEventArgs<List<NormalizedRect>> eventArgs)
        {
            _handRectsFromPalmDetectionsAnnotationController.DrawLater(eventArgs.value);
        }

        private void OnHandLandmarksOutput(object stream, OutputEventArgs<List<NormalizedLandmarkList>> eventArgs)
        {
            _handLandmarksAnnotationController.DrawLater(eventArgs.value);
        }

        private void OnHandRectsFromLandmarksOutput(object stream, OutputEventArgs<List<NormalizedRect>> eventArgs)
        {
            _handRectsFromLandmarksAnnotationController.DrawLater(eventArgs.value);
        }

        private void OnHandednessOutput(object stream, OutputEventArgs<List<ClassificationList>> eventArgs)
        {
            _handLandmarksAnnotationController.DrawLater(eventArgs.value);
        }
    }
}



