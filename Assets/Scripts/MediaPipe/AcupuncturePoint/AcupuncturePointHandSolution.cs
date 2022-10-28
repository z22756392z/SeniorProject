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

        private static readonly Vector3 _initL1ToL9 = new Vector3(-0.0376609f,-0.3828629f,0f);
        public Vector3 LeftHandNormalVector = default;
        public Vector3 RightHandNormalVector = default;
        public  static float LeftBaseLength = 0.075f;
        public static float RightBaseLength = 0.075f;
        public Quaternion PalmDir = default;
        public Vector3 PalmEular = default;
        public Vector3 Offset = default;
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
                //_initL1ToL9 = new Vector3(_currentHandLandmark1[9].X, _currentHandLandmark1[9].Y, 0) -
                new Vector3(_currentHandLandmark1[0].X, _currentHandLandmark1[0].Y, 0);

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
            
            if (_currentHandLandmark1 != null && handedness != null)
            {
                
                _newHandLandarkList.Add(Detect(_currentHandLandmark1, handedness[0].Classification[0].Label));
                
            }
                
            if (_currentHandLandmark2 != null && handedness != null && handedness.Count > 1)
            {
               
                _newHandLandarkList.Add(Detect(_currentHandLandmark2, handedness[1].Classification[0].Label));
            }
               


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
                NormalVectorCal(curHandLankmark,'L');
                LeftBaseLength = Vector2.Distance(new Vector2(curHandLankmark[0].X, curHandLankmark[0].Y),
               new Vector2(curHandLankmark[1].X, curHandLankmark[1].Y));
                if (LeftHandNormalVector.z < 0)
                {
                    return HandFacingDetect(curHandLankmark, 3, LeftBaseLength);
                }
                else
                {
                    return HandFacingDetect(curHandLankmark,4,LeftBaseLength);
                }
            }
            else if(handedness[0] =='R')
            {
                NormalVectorCal(curHandLankmark, 'R');
                RightBaseLength = Vector2.Distance(new Vector2(curHandLankmark[0].X, curHandLankmark[0].Y),
             new Vector2(curHandLankmark[1].X, curHandLankmark[1].Y));
                if (RightHandNormalVector.z < 0)
                {
                    return HandFacingDetect(curHandLankmark, 6, RightBaseLength);
                }
                else
                {
                    return HandFacingDetect(curHandLankmark, 5,RightBaseLength);
                }
            }
            return default;
        }

        private void NormalVectorCal(IList<NormalizedLandmark> curHandLankmark, char handness)
        {
            
            Vector3 dir1 = new Vector3(curHandLankmark[5].X, curHandLankmark[5].Y, curHandLankmark[5].Z) - new Vector3(curHandLankmark[0].X, curHandLankmark[0].Y, curHandLankmark[0].Z);
            Vector3 dir2 = new Vector3(curHandLankmark[17].X, curHandLankmark[17].Y, curHandLankmark[17].Z) - new Vector3(curHandLankmark[0].X, curHandLankmark[0].Y, curHandLankmark[0].Z);
            if (handness == 'L') 
            {
                LeftHandNormalVector = Vector3.Cross(dir1, dir2).normalized;
            }
            else
            {
                RightHandNormalVector = Vector3.Cross(dir1, dir2).normalized;
            }
        }

        NormalizedLandmarkList HandFacingDetect(IList<NormalizedLandmark> curHandLankmark, int Custom, float BaseLength)
        {
            int i = 0;
            NormalizedLandmarkList normalizedLandmarkList = new NormalizedLandmarkList();
            
            PalmDir = Quaternion.FromToRotation(_initL1ToL9, new Vector3(curHandLankmark[9].X, curHandLankmark[9].Y, 0) -
                new Vector3(curHandLankmark[0].X, curHandLankmark[0].Y, 0)).normalized;
            PalmEular = PalmDir.eulerAngles;
            foreach (var itemStack in _handAcupunturePointsInventory.Items)
            {
                if (itemStack.Item.Customize == Custom)
                {
                    if (_isShowDesireAcpunturePoint)
                    {
                        if(_desireAcpunturePoints.Count == 0)
                        {
                            continue;
                        }
                        string key = itemStack.Item.Name.TableEntryReference.Key;
                        if (_desireAcpunturePoints.Find(o => o.TableEntryReference.Key == key) != null)
                        {
                            NormalizedLandmark landmark = new NormalizedLandmark(curHandLankmark[itemStack.Item.LandMark]);
                            Offset = PalmDir * new Vector3(itemStack.Item.Offest.x, itemStack.Item.Offest.y, 0);
                            if (landmark.Z >= 0)
                            {
                                landmark.X += Offset.x * BaseLength / 15;
                                landmark.Y += Offset.y * BaseLength / 15;
                            }
                            else
                            {
                                landmark.X += Offset.x * BaseLength / 15;
                                landmark.Y += Offset.y * BaseLength / 15;
                            }
                            landmark.index = i;
                            normalizedLandmarkList.Landmark.Add(landmark);
                        }
                    }
                    else if (_aiIsShowDesireAcpunturePoint)
                    {
                        if (_aiDesireAcpunturePoints.Count == 0)
                        {
                            continue;
                        }
                        string localizeString = itemStack.Item.Name.GetLocalizedString();
                        if (_aiDesireAcpunturePoints.Find(o => o == localizeString) != null)
                        {
                            NormalizedLandmark landmark = new NormalizedLandmark(curHandLankmark[itemStack.Item.LandMark]);
                            Offset = PalmDir * new Vector3(itemStack.Item.Offest.x, itemStack.Item.Offest.y, 0);
                            if (landmark.Z >= 0)
                            {
                                landmark.X += Offset.x * BaseLength / 15;
                                landmark.Y += Offset.y * BaseLength / 15;
                            }
                            else
                            {
                                landmark.X += Offset.x * BaseLength / 15;
                                landmark.Y += Offset.y * BaseLength / 15;
                            }
                            landmark.index = i;
                            normalizedLandmarkList.Landmark.Add(landmark);
                        }
                    }
                    else
                    {
                        
                        NormalizedLandmark landmark = new NormalizedLandmark(curHandLankmark[itemStack.Item.LandMark]);
                        Offset = PalmDir * new Vector3(itemStack.Item.Offest.x, itemStack.Item.Offest.y,0);
                        if (landmark.Z >= 0)
                        {
                            landmark.X += Offset.x * BaseLength / 15;
                            landmark.Y += Offset.y * BaseLength / 15;
                        }
                        else
                        {
                            landmark.X += Offset.x * BaseLength / 15;
                            landmark.Y += Offset.y * BaseLength / 15;
                        }
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



