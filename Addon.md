## Addon

### Package Manager

* input system
* cinemachine
* universal render pipeline
* localization
* addressable
* tilemap

### import

* mediapipe package



package裡面有script需要修改: `HolisticLandmarkListAnnotationController.cs`、`HandLandmarkListAnnotation.cs`

> HolisticTrackingGraph

```
// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

using Google.Protobuf;

namespace Mediapipe.Unity.Holistic
{
  public class HolisticTrackingGraph : GraphRunner
  {
    public enum ModelComplexity
    {
      Lite = 0,
      Full = 1,
      Heavy = 2,
    }

    public bool refineFaceLandmarks = false;
    public ModelComplexity modelComplexity = ModelComplexity.Lite;
    public bool smoothLandmarks = true;
    public bool enableSegmentation = true;
    public bool smoothSegmentation = true;

    private float _minDetectionConfidence = 0.5f;
    public float minDetectionConfidence
    {
      get => _minDetectionConfidence;
      set => _minDetectionConfidence = Mathf.Clamp01(value);
    }

    private float _minTrackingConfidence = 0.5f;
    public float minTrackingConfidence
    {
      get => _minTrackingConfidence;
      set => _minTrackingConfidence = Mathf.Clamp01(value);
    }

    public event EventHandler<OutputEventArgs<Detection>> OnPoseDetectionOutput
    {
      add => _poseDetectionStream.AddListener(value);
      remove => _poseDetectionStream.RemoveListener(value);
    }

    public event EventHandler<OutputEventArgs<NormalizedLandmarkList>> OnPoseLandmarksOutput
    {
      add => _poseLandmarksStream.AddListener(value);
      remove => _poseLandmarksStream.RemoveListener(value);
    }

    public event EventHandler<OutputEventArgs<NormalizedLandmarkList>> OnFaceLandmarksOutput
    {
      add => _faceLandmarksStream.AddListener(value);
      remove => _faceLandmarksStream.RemoveListener(value);
    }

    public event EventHandler<OutputEventArgs<NormalizedLandmarkList>> OnLeftHandLandmarksOutput
    {
      add => _leftHandLandmarksStream.AddListener(value);
      remove => _leftHandLandmarksStream.RemoveListener(value);
    }

    public event EventHandler<OutputEventArgs<NormalizedLandmarkList>> OnRightHandLandmarksOutput
    {
      add => _rightHandLandmarksStream.AddListener(value);
      remove => _rightHandLandmarksStream.RemoveListener(value);
    }

    public event EventHandler<OutputEventArgs<LandmarkList>> OnPoseWorldLandmarksOutput
    {
      add => _poseWorldLandmarksStream.AddListener(value);
      remove => _poseWorldLandmarksStream.RemoveListener(value);
    }

    public event EventHandler<OutputEventArgs<ImageFrame>> OnSegmentationMaskOutput
    {
      add => _segmentationMaskStream.AddListener(value);
      remove => _segmentationMaskStream.RemoveListener(value);
    }

    public event EventHandler<OutputEventArgs<NormalizedRect>> OnPoseRoiOutput
    {
      add => _poseRoiStream.AddListener(value);
      remove => _poseRoiStream.RemoveListener(value);
    }

    private const string _InputStreamName = "input_video";
    private const string _PoseDetectionStreamName = "pose_detection";
    private const string _PoseLandmarksStreamName = "pose_landmarks";
    private const string _FaceLandmarksStreamName = "face_landmarks";
    private const string _LeftHandLandmarksStreamName = "left_hand_landmarks";
    private const string _RightHandLandmarksStreamName = "right_hand_landmarks";
    private const string _PoseWorldLandmarksStreamName = "pose_world_landmarks";
    private const string _SegmentationMaskStreamName = "segmentation_mask";
    private const string _PoseRoiStreamName = "pose_roi";

    private OutputStream<DetectionPacket, Detection> _poseDetectionStream;
    private OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList> _poseLandmarksStream;
    private OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList> _faceLandmarksStream;
    private OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList> _leftHandLandmarksStream;
    private OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList> _rightHandLandmarksStream;
    private OutputStream<LandmarkListPacket, LandmarkList> _poseWorldLandmarksStream;
    private OutputStream<ImageFramePacket, ImageFrame> _segmentationMaskStream;
    private OutputStream<NormalizedRectPacket, NormalizedRect> _poseRoiStream;

    public override void StartRun(ImageSource imageSource)
    {
      if (runningMode.IsSynchronous())
      {
        _poseDetectionStream.StartPolling().AssertOk();
        _poseLandmarksStream.StartPolling().AssertOk();
        _faceLandmarksStream.StartPolling().AssertOk();
        _leftHandLandmarksStream.StartPolling().AssertOk();
        _rightHandLandmarksStream.StartPolling().AssertOk();
        _poseWorldLandmarksStream.StartPolling().AssertOk();
        _segmentationMaskStream.StartPolling().AssertOk();
        _poseRoiStream.StartPolling().AssertOk();
      }
      StartRun(BuildSidePacket(imageSource));
    }

    public override void Stop()
    {
      _poseDetectionStream.RemoveAllListeners();
      _poseDetectionStream = null;
      _poseLandmarksStream.RemoveAllListeners();
      _poseLandmarksStream = null;
      _faceLandmarksStream.RemoveAllListeners();
      _faceLandmarksStream = null;
      _leftHandLandmarksStream.RemoveAllListeners();
      _leftHandLandmarksStream = null;
      _rightHandLandmarksStream.RemoveAllListeners();
      _rightHandLandmarksStream = null;
      _poseWorldLandmarksStream.RemoveAllListeners();
      _poseWorldLandmarksStream = null;
      _segmentationMaskStream.RemoveAllListeners();
      _segmentationMaskStream = null;
      _poseRoiStream.RemoveAllListeners();
      _poseRoiStream = null;
      base.Stop();
    }

    public void AddTextureFrameToInputStream(TextureFrame textureFrame)
    {
      AddTextureFrameToInputStream(_InputStreamName, textureFrame);
    }

    public bool TryGetNext(out Detection poseDetection, out NormalizedLandmarkList poseLandmarks, out NormalizedLandmarkList faceLandmarks, out NormalizedLandmarkList leftHandLandmarks,
                           out NormalizedLandmarkList rightHandLandmarks, out LandmarkList poseWorldLandmarks, out ImageFrame segmentationMask, out NormalizedRect poseRoi, bool allowBlock = true)
    {
      var currentTimestampMicrosec = GetCurrentTimestampMicrosec();
      var r1 = TryGetNext(_poseDetectionStream, out poseDetection, allowBlock, currentTimestampMicrosec);
      var r2 = TryGetNext(_poseLandmarksStream, out poseLandmarks, allowBlock, currentTimestampMicrosec);
      var r3 = TryGetNext(_faceLandmarksStream, out faceLandmarks, allowBlock, currentTimestampMicrosec);
      var r4 = TryGetNext(_leftHandLandmarksStream, out leftHandLandmarks, allowBlock, currentTimestampMicrosec);
      var r5 = TryGetNext(_rightHandLandmarksStream, out rightHandLandmarks, allowBlock, currentTimestampMicrosec);
      var r6 = TryGetNext(_poseWorldLandmarksStream, out poseWorldLandmarks, allowBlock, currentTimestampMicrosec);
      var r7 = TryGetNext(_segmentationMaskStream, out segmentationMask, allowBlock, currentTimestampMicrosec);
      var r8 = TryGetNext(_poseRoiStream, out poseRoi, allowBlock, currentTimestampMicrosec);

      return r1 || r2 || r3 || r4 || r5 || r6 || r7 || r8;
    }

        public bool TryGetNext(out NormalizedLandmarkList faceLandmarks, out NormalizedLandmarkList leftHandLandmarks,
                                  out NormalizedLandmarkList rightHandLandmarks, bool allowBlock = true)
        {
            var currentTimestampMicrosec = GetCurrentTimestampMicrosec();
            var r2 = TryGetNext(_faceLandmarksStream, out faceLandmarks, allowBlock, currentTimestampMicrosec);
            var r3 = TryGetNext(_leftHandLandmarksStream, out leftHandLandmarks, allowBlock, currentTimestampMicrosec);
            var r4 = TryGetNext(_rightHandLandmarksStream, out rightHandLandmarks, allowBlock, currentTimestampMicrosec);


            return  r2 || r3 || r4;
        }

        protected override IList<WaitForResult> RequestDependentAssets()
    {
      return new List<WaitForResult> {
        WaitForAsset("face_detection_short_range.bytes"),
        WaitForAsset(refineFaceLandmarks ? "face_landmark_with_attention.bytes" : "face_landmark.bytes"),
        WaitForAsset("iris_landmark.bytes"),
        WaitForAsset("hand_landmark_full.bytes"),
        WaitForAsset("hand_recrop.bytes"),
        WaitForAsset("handedness.txt"),
        WaitForAsset("palm_detection_full.bytes"),
        WaitForAsset("pose_detection.bytes"),
        WaitForPoseLandmarkModel(),
      };
    }

    private WaitForResult WaitForPoseLandmarkModel()
    {
      switch (modelComplexity)
      {
        case ModelComplexity.Lite: return WaitForAsset("pose_landmark_lite.bytes");
        case ModelComplexity.Full: return WaitForAsset("pose_landmark_full.bytes");
        case ModelComplexity.Heavy: return WaitForAsset("pose_landmark_heavy.bytes");
        default: throw new InternalException($"Invalid model complexity: {modelComplexity}");
      }
    }

    protected override Status ConfigureCalculatorGraph(CalculatorGraphConfig config)
    {
      if (runningMode == RunningMode.NonBlockingSync)
      {
        _poseDetectionStream = new OutputStream<DetectionPacket, Detection>(
            calculatorGraph, _PoseDetectionStreamName, config.AddPacketPresenceCalculator(_PoseDetectionStreamName), timeoutMicrosec);
        _poseLandmarksStream = new OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList>(
            calculatorGraph, _PoseLandmarksStreamName, config.AddPacketPresenceCalculator(_PoseLandmarksStreamName), timeoutMicrosec);
        _faceLandmarksStream = new OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList>(
            calculatorGraph, _FaceLandmarksStreamName, config.AddPacketPresenceCalculator(_FaceLandmarksStreamName), timeoutMicrosec);
        _leftHandLandmarksStream = new OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList>(
            calculatorGraph, _LeftHandLandmarksStreamName, config.AddPacketPresenceCalculator(_LeftHandLandmarksStreamName), timeoutMicrosec);
        _rightHandLandmarksStream = new OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList>(
            calculatorGraph, _RightHandLandmarksStreamName, config.AddPacketPresenceCalculator(_RightHandLandmarksStreamName), timeoutMicrosec);
        _poseWorldLandmarksStream = new OutputStream<LandmarkListPacket, LandmarkList>(
            calculatorGraph, _PoseWorldLandmarksStreamName, config.AddPacketPresenceCalculator(_PoseWorldLandmarksStreamName), timeoutMicrosec);
        _segmentationMaskStream = new OutputStream<ImageFramePacket, ImageFrame>(
            calculatorGraph, _SegmentationMaskStreamName, config.AddPacketPresenceCalculator(_SegmentationMaskStreamName), timeoutMicrosec);
        _poseRoiStream = new OutputStream<NormalizedRectPacket, NormalizedRect>(
            calculatorGraph, _PoseRoiStreamName, config.AddPacketPresenceCalculator(_PoseRoiStreamName), timeoutMicrosec);
      }
      else
      {
        _poseDetectionStream = new OutputStream<DetectionPacket, Detection>(calculatorGraph, _PoseDetectionStreamName, true, timeoutMicrosec);
        _poseLandmarksStream = new OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList>(calculatorGraph, _PoseLandmarksStreamName, true, timeoutMicrosec);
        _faceLandmarksStream = new OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList>(calculatorGraph, _FaceLandmarksStreamName, true, timeoutMicrosec);
        _leftHandLandmarksStream = new OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList>(calculatorGraph, _LeftHandLandmarksStreamName, true, timeoutMicrosec);
        _rightHandLandmarksStream = new OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList>(calculatorGraph, _RightHandLandmarksStreamName, true, timeoutMicrosec);
        _poseWorldLandmarksStream = new OutputStream<LandmarkListPacket, LandmarkList>(calculatorGraph, _PoseWorldLandmarksStreamName, true, timeoutMicrosec);
        _segmentationMaskStream = new OutputStream<ImageFramePacket, ImageFrame>(calculatorGraph, _SegmentationMaskStreamName, true, timeoutMicrosec);
        _poseRoiStream = new OutputStream<NormalizedRectPacket, NormalizedRect>(calculatorGraph, _PoseRoiStreamName, true, timeoutMicrosec);
      }

      using (var validatedGraphConfig = new ValidatedGraphConfig())
      {
        var status = validatedGraphConfig.Initialize(config);

        if (!status.Ok()) { return status; }

        var extensionRegistry = new ExtensionRegistry() { TensorsToDetectionsCalculatorOptions.Extensions.Ext, ThresholdingCalculatorOptions.Extensions.Ext };
        var cannonicalizedConfig = validatedGraphConfig.Config(extensionRegistry);

        var poseDetectionCalculatorPattern = new Regex("__posedetection[a-z]+__TensorsToDetectionsCalculator$");
        var tensorsToDetectionsCalculators = cannonicalizedConfig.Node.Where((node) => poseDetectionCalculatorPattern.Match(node.Name).Success).ToList();

        var poseTrackingCalculatorPattern = new Regex("tensorstoposelandmarksandsegmentation__ThresholdingCalculator$");
        var thresholdingCalculators = cannonicalizedConfig.Node.Where((node) => poseTrackingCalculatorPattern.Match(node.Name).Success).ToList();

        foreach (var calculator in tensorsToDetectionsCalculators)
        {
          if (calculator.Options.HasExtension(TensorsToDetectionsCalculatorOptions.Extensions.Ext))
          {
            var options = calculator.Options.GetExtension(TensorsToDetectionsCalculatorOptions.Extensions.Ext);
            options.MinScoreThresh = minDetectionConfidence;
            Logger.LogInfo(TAG, $"Min Detection Confidence = {minDetectionConfidence}");
          }
        }

        foreach (var calculator in thresholdingCalculators)
        {
          if (calculator.Options.HasExtension(ThresholdingCalculatorOptions.Extensions.Ext))
          {
            var options = calculator.Options.GetExtension(ThresholdingCalculatorOptions.Extensions.Ext);
            options.Threshold = minTrackingConfidence;
            Logger.LogInfo(TAG, $"Min Tracking Confidence = {minTrackingConfidence}");
          }
        }
        return calculatorGraph.Initialize(cannonicalizedConfig);
      }
    }

    private SidePacket BuildSidePacket(ImageSource imageSource)
    {
      var sidePacket = new SidePacket();

      SetImageTransformationOptions(sidePacket, imageSource);

      // TODO: refactoring
      // The orientation of the output image must match that of the input image.
      var isInverted = CoordinateSystem.ImageCoordinate.IsInverted(imageSource.rotation);
      var outputRotation = imageSource.rotation;
      var outputHorizontallyFlipped = !isInverted && imageSource.isHorizontallyFlipped;
      var outputVerticallyFlipped = (!runningMode.IsSynchronous() && imageSource.isVerticallyFlipped) ^ (isInverted && imageSource.isHorizontallyFlipped);

      if ((outputHorizontallyFlipped && outputVerticallyFlipped) || outputRotation == RotationAngle.Rotation180)
      {
        outputRotation = outputRotation.Add(RotationAngle.Rotation180);
        outputHorizontallyFlipped = !outputHorizontallyFlipped;
        outputVerticallyFlipped = !outputVerticallyFlipped;
      }

      sidePacket.Emplace("output_rotation", new IntPacket((int)outputRotation));
      sidePacket.Emplace("output_horizontally_flipped", new BoolPacket(outputHorizontallyFlipped));
      sidePacket.Emplace("output_vertically_flipped", new BoolPacket(outputVerticallyFlipped));

      Logger.LogDebug($"output_rotation = {outputRotation}, output_horizontally_flipped = {outputHorizontallyFlipped}, output_vertically_flipped = {outputVerticallyFlipped}");

      sidePacket.Emplace("refine_face_landmarks", new BoolPacket(refineFaceLandmarks));
      sidePacket.Emplace("model_complexity", new IntPacket((int)modelComplexity));
      sidePacket.Emplace("smooth_landmarks", new BoolPacket(smoothLandmarks));
      sidePacket.Emplace("enable_segmentation", new BoolPacket(enableSegmentation));
      sidePacket.Emplace("smooth_segmentation", new BoolPacket(smoothSegmentation));

      Logger.LogInfo(TAG, $"Refine Face Landmarks = {refineFaceLandmarks}");
      Logger.LogInfo(TAG, $"Model Complexity = {modelComplexity}");
      Logger.LogInfo(TAG, $"Smooth Landmarks = {smoothLandmarks}");
      Logger.LogInfo(TAG, $"Enable Segmentation = {enableSegmentation}");
      Logger.LogInfo(TAG, $"Smooth Segmentation = {smoothSegmentation}");

      return sidePacket;
    }
  }
}
```



> HolisticLandmarkListAnnotationController

```c#
// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections.Generic;
using UnityEngine;

namespace Mediapipe.Unity
{
  public class HolisticLandmarkListAnnotationController : AnnotationController<HolisticLandmarkListAnnotation>
  {
    [SerializeField] private bool _visualizeZ = false;
    [SerializeField] private int _circleVertices = 128;

    private IList<NormalizedLandmark> _currentFaceLandmarkList;
    private IList<NormalizedLandmark> _currentPoseLandmarkList;
    private IList<NormalizedLandmark> _currentLeftHandLandmarkList;
    private IList<NormalizedLandmark> _currentRightHandLandmarkList;

    public void DrawNow(IList<NormalizedLandmark> faceLandmarkList, IList<NormalizedLandmark> poseLandmarkList,
                        IList<NormalizedLandmark> leftHandLandmarkList, IList<NormalizedLandmark> rightHandLandmarkList)
    {
      _currentFaceLandmarkList = faceLandmarkList;
      _currentPoseLandmarkList = poseLandmarkList;
      _currentLeftHandLandmarkList = leftHandLandmarkList;
      _currentRightHandLandmarkList = rightHandLandmarkList;
      SyncNow();
    }

    public void DrawNow(NormalizedLandmarkList faceLandmarkList, NormalizedLandmarkList poseLandmarkList,
                    NormalizedLandmarkList leftHandLandmarkList, NormalizedLandmarkList rightHandLandmarkList)
    {
        DrawNow(
            faceLandmarkList?.Landmark,
            poseLandmarkList?.Landmark,
            leftHandLandmarkList?.Landmark,
            rightHandLandmarkList?.Landmark
        );
        SyncNow();
    }

    public void DrawNow(NormalizedLandmarkList faceLandmarkList, NormalizedLandmarkList poseLandmarkList,
                    IList<NormalizedLandmark> leftHandLandmarkList, NormalizedLandmarkList rightHandLandmarkList)
    {
        DrawNow(
            faceLandmarkList?.Landmark,
            poseLandmarkList?.Landmark,
            leftHandLandmarkList,
            rightHandLandmarkList?.Landmark
        );
        SyncNow();
    }

        public void DrawFaceLandmarkListLater(IList<NormalizedLandmark> faceLandmarkList)
    {
      UpdateCurrentTarget(faceLandmarkList, ref _currentFaceLandmarkList);
    }

    public void DrawFaceLandmarkListLater(NormalizedLandmarkList faceLandmarkList)
    {
      DrawFaceLandmarkListLater(faceLandmarkList?.Landmark);
    }

    public void DrawPoseLandmarkListLater(IList<NormalizedLandmark> poseLandmarkList)
    {
      UpdateCurrentTarget(poseLandmarkList, ref _currentPoseLandmarkList);
    }

    public void DrawPoseLandmarkListLater(NormalizedLandmarkList poseLandmarkList)
    {
      DrawPoseLandmarkListLater(poseLandmarkList?.Landmark);
    }

    public void DrawLeftHandLandmarkListLater(IList<NormalizedLandmark> leftHandLandmarkList)
    {
      UpdateCurrentTarget(leftHandLandmarkList, ref _currentLeftHandLandmarkList);
    }

    public void DrawLeftHandLandmarkListLater(NormalizedLandmarkList leftHandLandmarkList)
    {
      DrawLeftHandLandmarkListLater(leftHandLandmarkList?.Landmark);
    }

    public void DrawRightHandLandmarkListLater(IList<NormalizedLandmark> rightHandLandmarkList)
    {
      UpdateCurrentTarget(rightHandLandmarkList, ref _currentRightHandLandmarkList);
    }

    public void DrawRightHandLandmarkListLater(NormalizedLandmarkList rightHandLandmarkList)
    {
      DrawRightHandLandmarkListLater(rightHandLandmarkList?.Landmark);
    }

    protected override void SyncNow()
    {
      isStale = false;
      annotation.Draw(
        _currentFaceLandmarkList,
        _currentPoseLandmarkList,
        _currentLeftHandLandmarkList,
        _currentRightHandLandmarkList,
        _visualizeZ,
        _circleVertices
      );
    }
  }
}
```



> HandLandmarkListAnnotation

```c#
// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections.Generic;
using UnityEngine;

namespace Mediapipe.Unity
{
#pragma warning disable IDE0065
    using Color = UnityEngine.Color;
#pragma warning restore IDE0065

    public sealed class HandLandmarkListAnnotation : HierarchicalAnnotation
    {
        [SerializeField] private PointListAnnotation _landmarkListAnnotation;
        [SerializeField] private ConnectionListAnnotation _connectionListAnnotation;
        [SerializeField] private Color _leftLandmarkColor = Color.green;
        [SerializeField] private Color _rightLandmarkColor = Color.green;
        [SerializeField] private bool _drawConnection = true;
        public GameObject LandMarkExtracter = default;
        public enum Hand
        {
            Left,
            Right,
        }

        public int LandmarkCount = 21;
        
        private readonly List<(int, int)> _connections = new List<(int, int)> {
      (0, 1),
      (1, 2),
      (2, 3),
      (3, 4),
      (0, 5),
      (5, 9),
      (9, 13),
      (13, 17),
      (0, 17),
      (5, 6),
      (6, 7),
      (7, 8),
      (9, 10),
      (10, 11),
      (11, 12),
      (13, 14),
      (14, 15),
      (15, 16),
      (17, 18),
      (18, 19),
      (19, 20),
    };

        public override bool isMirrored
        {
            set
            {
                _landmarkListAnnotation.isMirrored = value;
                _connectionListAnnotation.isMirrored = value;
                base.isMirrored = value;
            }
        }

        public override RotationAngle rotationAngle
        {
            set
            {
                _landmarkListAnnotation.rotationAngle = value;
                _connectionListAnnotation.rotationAngle = value;
                base.rotationAngle = value;
            }
        }
        private void OnDisable()
        {
            if(LandMarkExtracter != default)
            LandMarkExtracter.SendMessage(methodName: "ClearHands", value: transform.GetChild(0).GetChild(0).GetComponent<PointAnnotation>());
        }

        public PointAnnotation this[int index] => _landmarkListAnnotation[index];

        private void Start()
        {
            _landmarkListAnnotation.Fill(LandmarkCount < 21 ? 21 : LandmarkCount,true);
            if(_drawConnection)
                _connectionListAnnotation.Fill(_connections, _landmarkListAnnotation);
        }

        public void SetLeftLandmarkColor(Color leftLandmarkColor)
        {
            _leftLandmarkColor = leftLandmarkColor;
        }

        public void SetRightLandmarkColor(Color rightLandmarkColor)
        {
            _rightLandmarkColor = rightLandmarkColor;
        }

        public void SetLandmarkRadius(float landmarkRadius)
        {
            _landmarkListAnnotation.SetRadius(landmarkRadius);
        }

        public void SetConnectionColor(Color connectionColor)
        {
            _connectionListAnnotation.SetColor(connectionColor);
        }

        public void SetConnectionWidth(float connectionWidth)
        {
            _connectionListAnnotation.SetLineWidth(connectionWidth);
        }

        public void SetHandedness(Hand handedness)
        {
            if (handedness == Hand.Left)
            {
                _landmarkListAnnotation.SetColor(_leftLandmarkColor);
            }
            else if (handedness == Hand.Right)
            {
                _landmarkListAnnotation.SetColor(_rightLandmarkColor);
            }
        }

        public void SetHandedness(IList<Classification> handedness)
        {
            if (handedness == null || handedness.Count == 0 || handedness[0].Label == "Left")
            {
                SetHandedness(Hand.Left);
            }
            else if (handedness[0].Label == "Right")
            {
                SetHandedness(Hand.Right);
            }
            // ignore unknown label
        }

        public void SetHandedness(ClassificationList handedness)
        {
            SetHandedness(handedness.Classification);
        }

        public void Draw(IList<NormalizedLandmark> target, bool visualizeZ = false)
        {
            if (ActivateFor(target))
            {
                _landmarkListAnnotation.Draw(target, visualizeZ);
                // Draw explicitly because connection annotation's targets remain the same.
                if (_drawConnection)
                    _connectionListAnnotation.Redraw();
            }
        }


        public void Draw(NormalizedLandmarkList target, bool visualizeZ = false)
        {
            Draw(target?.Landmark, visualizeZ);
        }
    }
}

```



> HierarchicalAnnotation

```c#
// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;

namespace Mediapipe.Unity
{
  public interface IHierachicalAnnotation
  {
    IHierachicalAnnotation root { get; }
    Transform transform { get; }
    RectTransform GetAnnotationLayer();
  }

  public abstract class HierarchicalAnnotation : MonoBehaviour, IHierachicalAnnotation
  {
    private IHierachicalAnnotation _root;
    public IHierachicalAnnotation root
    {
      get
      {
        if (_root == null)
        {
          var parentObj = transform.parent == null ? null : transform.parent.gameObject;
          _root = (parentObj != null && parentObj.TryGetComponent<IHierachicalAnnotation>(out var parent)) ? parent.root : this;
        }
        return _root;
      }
      protected set => _root = value;
    }

    public RectTransform GetAnnotationLayer()
    {
      return root.transform.parent.gameObject.GetComponent<RectTransform>();
    }

    public bool isActive => gameObject.activeSelf;
    public bool isActiveInHierarchy => gameObject.activeInHierarchy;

    public void SetActive(bool isActive)
    {
      if (this.isActive != isActive)
      {
        gameObject.SetActive(isActive);
      }
    }

    /// <summary>
    ///   Prepare to annotate <paramref name="target" />.
    ///   If <paramref name="target" /> is not null, it activates itself.
    /// </summary>
    /// <return>
    ///   If it is activated and <paramref name="target" /> can be drawn.
    ///   In effect, it returns if <paramref name="target" /> is null or not.
    /// </return>
    /// <param name="target">Data to be annotated</param>
    protected bool ActivateFor<T>(T target)
    {
      if (target == null)
      {
        SetActive(false);
        return false;
      }
      SetActive(true);
      return true;
    }

        protected bool ActivateFor<T>(T target, bool SetTargetDisable)
        {
            if (target == null)
            {
                if(SetTargetDisable)
                    SetActive(false);
                return false;
            }
            SetActive(true);
            return true;
        }

        public virtual bool isMirrored { get; set; }
    public virtual RotationAngle rotationAngle { get; set; } = RotationAngle.Rotation0;

    protected TAnnotation InstantiateChild<TAnnotation>(GameObject prefab) where TAnnotation : HierarchicalAnnotation
    {
      var annotation = Instantiate(prefab, transform).GetComponent<TAnnotation>();
      annotation.isMirrored = isMirrored;
      annotation.rotationAngle = rotationAngle;
      return annotation;
    }

    protected TAnnotation InstantiateChild<TAnnotation>(string name = "Game Object") where TAnnotation : HierarchicalAnnotation
    {
      var gameObject = new GameObject(name);
      gameObject.transform.SetParent(transform);

      return gameObject.AddComponent<TAnnotation>();
    }
  }
}

```

> PointAnnotation

```c#
// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using Mediapipe.Unity.CoordinateSystem;
using UnityEngine;

using mplt = Mediapipe.LocationData.Types;

namespace Mediapipe.Unity
{
#pragma warning disable IDE0065
    using Color = UnityEngine.Color;
#pragma warning restore IDE0065

    public class PointAnnotation : HierarchicalAnnotation
    {
        [SerializeField] private Color _color = Color.green;
        [SerializeField] private float _radius = 15.0f;
        [SerializeField] private bool _setPointDisable = true;
        public Color Color => _color;
        private void OnEnable()
        {
            ApplyColor(_color);
            ApplyRadius(_radius);
        }

        private void OnDisable()
        {
            ApplyRadius(0.0f);
        }

        public void SetColor(Color color)
        {
            _color = color;
            ApplyColor(_color);
        }

        public void SetRadius(float radius)
        {
            _radius = radius;
            ApplyRadius(_radius);
        }
        public void SetPointDisable(bool value)
        {
            _setPointDisable = value;
        }


        public void Draw(Vector3 position)
        {
            SetActive(true); // Vector3 is not nullable
            transform.localPosition = position;
        }

        public void Draw(Landmark target, Vector3 scale, bool visualizeZ = true)
        {
            if (ActivateFor(target, _setPointDisable))
            {
                var position = GetAnnotationLayer().GetLocalPosition(target, scale, rotationAngle, isMirrored);
                if (!visualizeZ)
                {
                    position.z = 0.0f;
                }
                transform.localPosition = position;
            }
        }

        public void Draw(NormalizedLandmark target, bool visualizeZ = true)
        {
            if (ActivateFor(target, _setPointDisable))
            {
                var position = GetAnnotationLayer().GetLocalPosition(target, rotationAngle, isMirrored);
                if (!visualizeZ)
                {
                    position.z = 0.0f;
                }
                transform.localPosition = position;
                gameObject.SendMessage("SetupItemCore", target.index, SendMessageOptions.DontRequireReceiver);
            }
        }

        public void Draw(NormalizedPoint2D target)
        {
            if (ActivateFor(target, _setPointDisable))
            {
                var position = GetAnnotationLayer().GetLocalPosition(target, rotationAngle, isMirrored);
                transform.localPosition = position;
            }
        }

        public void Draw(Point3D target, Vector2 focalLength, Vector2 principalPoint, float zScale, bool visualizeZ = true)
        {
            if (ActivateFor(target, _setPointDisable))
            {
                var position = GetAnnotationLayer().GetLocalPosition(target, focalLength, principalPoint, zScale, rotationAngle, isMirrored);
                if (!visualizeZ)
                {
                    position.z = 0.0f;
                }
                transform.localPosition = position;
            }
        }

        public void Draw(AnnotatedKeyPoint target, Vector2 focalLength, Vector2 principalPoint, float zScale, bool visualizeZ = true)
        {
            if (visualizeZ)
            {
                Draw(target?.Point3D, focalLength, principalPoint, zScale, true);
            }
            else
            {
                Draw(target?.Point2D);
            }
        }

        public void Draw(mplt.RelativeKeypoint target, float threshold = 0.0f)
        {
            if (ActivateFor(target, _setPointDisable))
            {
                Draw(GetAnnotationLayer().GetLocalPosition(target, rotationAngle, isMirrored));
                SetColor(GetColor(target.Score, threshold));
            }
        }

        private void ApplyColor(Color color)
        {
            GetComponent<Renderer>().material.color = color;
            
            SendMessage("ApplyColorToUI", color, SendMessageOptions.DontRequireReceiver);
        }

        private void ApplyRadius(float radius)
        {
            transform.localScale = radius * Vector3.one;
        }

        private Color GetColor(float score, float threshold)
        {
            var t = (score - threshold) / (1 - threshold);
            var h = Mathf.Lerp(90, 0, t) / 360; // from yellow-green to red
            return Color.HSVToRGB(h, 1, 1);
        }
    }
}

```

> PointLIstAnnotation

```c#
// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections.Generic;
using UnityEngine;

using mplt = Mediapipe.LocationData.Types;

namespace Mediapipe.Unity
{
#pragma warning disable IDE0065
  using Color = UnityEngine.Color;
#pragma warning restore IDE0065

  public class PointListAnnotation : ListAnnotation<PointAnnotation>
  {
    [SerializeField] private Color _color = Color.green;
    [SerializeField] private float _radius = 15.0f;
        [SerializeField] private bool _setPointDisable = true;

    private void OnValidate()
    {
      ApplyColor(_color);
      ApplyRadius(_radius);
            ApplyPointDisable(_setPointDisable);
    }

    public void SetColor(Color color)
    {
      _color = color;
      ApplyColor(_color);
    }

    public void SetRadius(float radius)
    {
      _radius = radius;
      ApplyRadius(_radius);
    }

    public void Draw(IList<Vector3> targets)
    {
      if (ActivateFor(targets))
      {
        CallActionForAll(targets, (annotation, target) =>
        {
          if (annotation != null) { annotation.Draw(target); }
        });
      }
    }

    public void Draw(IList<Landmark> targets, Vector3 scale, bool visualizeZ = true)
    {
      if (ActivateFor(targets))
      {
        CallActionForAll(targets, (annotation, target) =>
        {
          if (annotation != null) { annotation.Draw(target, scale, visualizeZ); }
        });
      }
    }

    public void Draw(LandmarkList targets, Vector3 scale, bool visualizeZ = true)
    {
      Draw(targets.Landmark, scale, visualizeZ);
    }

    public void Draw(IList<NormalizedLandmark> targets, bool visualizeZ = true)
    {
      if (ActivateFor(targets))
      {
        CallActionForAll(targets, (annotation, target) =>
        {
          if (annotation != null) { annotation.Draw(target, visualizeZ); }
        });
      }
    }

    public void Draw(NormalizedLandmarkList targets, bool visualizeZ = true)
    {
      Draw(targets.Landmark, visualizeZ);
    }

    public void Draw(IList<AnnotatedKeyPoint> targets, Vector2 focalLength, Vector2 principalPoint, float zScale, bool visualizeZ = true)
    {
      if (ActivateFor(targets))
      {
        CallActionForAll(targets, (annotation, target) =>
        {
          if (annotation != null) { annotation.Draw(target, focalLength, principalPoint, zScale, visualizeZ); }
        });
      }
    }

    public void Draw(IList<mplt.RelativeKeypoint> targets, float threshold = 0.0f)
    {
      if (ActivateFor(targets))
      {
        CallActionForAll(targets, (annotation, target) =>
        {
          if (annotation != null) { annotation.Draw(target, threshold); }
        });
      }
    }

    protected override PointAnnotation InstantiateChild(bool isActive = true)
    {
      var annotation = base.InstantiateChild(isActive);
      annotation.SetColor(_color);
      annotation.SetRadius(_radius);
            annotation.SetPointDisable(_setPointDisable);
      return annotation;
    }

    private void ApplyColor(Color color)
    {
      foreach (var point in children)
      {
        if (point != null) { point.SetColor(color); }
      }
    }

    private void ApplyRadius(float radius)
    {
      foreach (var point in children)
      {
        if (point != null) { point.SetRadius(radius); }
      }
    }
        private void ApplyPointDisable(bool value)
        {
            foreach (var point in children)
            {
                if (point != null) { point.SetPointDisable(value); }
            }
        }
  }
}

```

> ListAnnotation

```c#
// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mediapipe.Unity
{
    public abstract class ListAnnotation<T> : HierarchicalAnnotation where T : HierarchicalAnnotation
    {
        [SerializeField] private GameObject _annotationPrefab;

        private List<T> _children;
        protected List<T> children
        {
            get
            {
                if (_children == null)
                {
                    _children = new List<T>();
                }
                return _children;
            }
        }

        public T this[int index] => children[index];

        public int count => children.Count;

        public void Fill(int count, bool isSetupItemCore = false)
        {
            int i = 0;
            while (children.Count < count)
            {
                if(isSetupItemCore)
                    children.Add(InstantiateChild(i, false));
                else
                    children.Add(InstantiateChild(false));
                i++;
            }
        }

        public void Add(T element)
        {
            children.Add(element);
        }

        public override bool isMirrored
        {
            set
            {
                foreach (var child in children)
                {
                    child.isMirrored = value;
                }
                base.isMirrored = value;
            }
        }

        public override RotationAngle rotationAngle
        {
            set
            {
                foreach (var child in children)
                {
                    child.rotationAngle = value;
                }
                base.rotationAngle = value;
            }
        }

        protected virtual void Destroy()
        {
            foreach (var child in children)
            {
                Destroy(child);
            }
            _children = null;
        }

        protected virtual T InstantiateChild(bool isActive = true)
        {
            var annotation = InstantiateChild<T>(_annotationPrefab);
            annotation.SetActive(isActive);
            return annotation;
        }

        protected virtual T InstantiateChild(int index, bool isActive = true)
        {
            var annotation = InstantiateChild<T>(_annotationPrefab);
            annotation.gameObject.SendMessage("SetupItemCore", index,SendMessageOptions.DontRequireReceiver);
            annotation.SetActive(isActive);
            return annotation;
        }

        /// <summary>
        ///   Zip <see cref="children" /> and <paramref name="argumentList" />, and call <paramref name="action" /> with each pair.
        ///   If <paramref name="argumentList" /> has more elements than <see cref="children" />, <see cref="children" /> elements will be initialized with <see cref="InstantiateChild" />.
        /// </summary>
        /// <param name="action">
        ///   This will receive 2 arguments and return void.
        ///   The 1st argument is <typeparamref name="T" />, that is an ith element in <see cref="children" />.
        ///   The 2nd argument is <typeparamref name="TArg" />, that is also an ith element in <paramref name="argumentList" />.
        /// </param>
        protected void CallActionForAll<TArg>(IList<TArg> argumentList, Action<T, TArg> action)
        {
            for (var i = 0; i < Mathf.Max(children.Count, argumentList.Count); i++)
            {
                if (i >= argumentList.Count)
                {
                    // children.Count > argumentList.Count
                    action(children[i], default);
                    continue;
                }

                // reset annotations
                if (i >= children.Count)
                {
                    // children.Count < argumentList.Count
                    children.Add(InstantiateChild(i, false));
                }
                else if (children[i] == null)
                {
                    // child is not initialized yet
                    children.Add(InstantiateChild(i, false));
                }
                action(children[i], argumentList[i]);
            }
        }
    }
}

```



> HolisticLandmarkListAnnotation

```c#
// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections.Generic;
using UnityEngine;

namespace Mediapipe.Unity
{
  public sealed class HolisticLandmarkListAnnotation : HierarchicalAnnotation
  {
    [SerializeField] private FaceLandmarkListWithIrisAnnotation _faceLandmarkListAnnotation;
    [SerializeField] private PoseLandmarkListAnnotation _poseLandmarkListAnnotation;
    [SerializeField] public HandLandmarkListAnnotation _leftHandLandmarkListAnnotation;
    [SerializeField] private HandLandmarkListAnnotation _rightHandLandmarkListAnnotation;
    [SerializeField] private ConnectionListAnnotation _connectionListAnnotation;

    public override bool isMirrored
    {
      set
      {
        _faceLandmarkListAnnotation.isMirrored = value;
        _poseLandmarkListAnnotation.isMirrored = value;
        _leftHandLandmarkListAnnotation.isMirrored = value;
        _rightHandLandmarkListAnnotation.isMirrored = value;
        _connectionListAnnotation.isMirrored = value;
        base.isMirrored = value;
      }
    }

    public override RotationAngle rotationAngle
    {
      set
      {
        _faceLandmarkListAnnotation.rotationAngle = value;
        _poseLandmarkListAnnotation.rotationAngle = value;
        _leftHandLandmarkListAnnotation.rotationAngle = value;
        _rightHandLandmarkListAnnotation.rotationAngle = value;
        _connectionListAnnotation.rotationAngle = value;
        base.rotationAngle = value;
      }
    }

    private void Start()
    {
      _leftHandLandmarkListAnnotation.SetHandedness(HandLandmarkListAnnotation.Hand.Left);
      _rightHandLandmarkListAnnotation.SetHandedness(HandLandmarkListAnnotation.Hand.Right);
      _connectionListAnnotation.Fill(2); // left/right wrist joint
    }

    public void Draw(IList<NormalizedLandmark> faceLandmarks, IList<NormalizedLandmark> poseLandmarks,
                     IList<NormalizedLandmark> leftHandLandmarks, IList<NormalizedLandmark> rightHandLandmarks, bool visualizeZ = false, int circleVertices = 128)
    {
      var mask = PoseLandmarkListAnnotation.BodyParts.All;
      if (faceLandmarks != null)
      {
        mask ^= PoseLandmarkListAnnotation.BodyParts.Face;
      }
      if (leftHandLandmarks != null)
      {
        mask ^= PoseLandmarkListAnnotation.BodyParts.LeftHand;
      }
      if (rightHandLandmarks != null)
      {
        mask ^= PoseLandmarkListAnnotation.BodyParts.RightHand;
      }
      _faceLandmarkListAnnotation.Draw(faceLandmarks, visualizeZ, circleVertices);
      _poseLandmarkListAnnotation.Draw(poseLandmarks, mask, visualizeZ);
      _leftHandLandmarkListAnnotation.Draw(leftHandLandmarks, visualizeZ);
      _rightHandLandmarkListAnnotation.Draw(rightHandLandmarks, visualizeZ);
      RedrawWristJoints();
    }

    public void Draw(NormalizedLandmarkList faceLandmarks, NormalizedLandmarkList poseLandmarks,
                     NormalizedLandmarkList leftHandLandmarks, NormalizedLandmarkList rightHandLandmarks, bool visualizeZ = false, int circleVertices = 128)
    {
      Draw(
        faceLandmarks?.Landmark,
        poseLandmarks?.Landmark,
        leftHandLandmarks?.Landmark,
        rightHandLandmarks?.Landmark,
        visualizeZ,
        circleVertices
      );
    }

    private void RedrawWristJoints()
    {
      if (_connectionListAnnotation[0].isEmpty)
      {
        // connect left elbow and wrist
        _connectionListAnnotation[0].Draw(new Connection(_poseLandmarkListAnnotation[13], _leftHandLandmarkListAnnotation[0]));
      }
      if (_connectionListAnnotation[1].isEmpty)
      {
        // connect right elbow and wrist
        _connectionListAnnotation[1].Draw(new Connection(_poseLandmarkListAnnotation[14], _rightHandLandmarkListAnnotation[0]));
      }
      _connectionListAnnotation.Redraw();
    }
  }
}

```



> HolisticTrackingGraph

```c#
// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

using Google.Protobuf;

namespace Mediapipe.Unity.Holistic
{
  public class HolisticTrackingGraph : GraphRunner
  {
    public enum ModelComplexity
    {
      Lite = 0,
      Full = 1,
      Heavy = 2,
    }

    public bool refineFaceLandmarks = false;
    public ModelComplexity modelComplexity = ModelComplexity.Lite;
    public bool smoothLandmarks = true;
    public bool enableSegmentation = true;
    public bool smoothSegmentation = true;

    private float _minDetectionConfidence = 0.5f;
    public float minDetectionConfidence
    {
      get => _minDetectionConfidence;
      set => _minDetectionConfidence = Mathf.Clamp01(value);
    }

    private float _minTrackingConfidence = 0.5f;
    public float minTrackingConfidence
    {
      get => _minTrackingConfidence;
      set => _minTrackingConfidence = Mathf.Clamp01(value);
    }

    public event EventHandler<OutputEventArgs<Detection>> OnPoseDetectionOutput
    {
      add => _poseDetectionStream.AddListener(value);
      remove => _poseDetectionStream.RemoveListener(value);
    }

    public event EventHandler<OutputEventArgs<NormalizedLandmarkList>> OnPoseLandmarksOutput
    {
      add => _poseLandmarksStream.AddListener(value);
      remove => _poseLandmarksStream.RemoveListener(value);
    }

    public event EventHandler<OutputEventArgs<NormalizedLandmarkList>> OnFaceLandmarksOutput
    {
      add => _faceLandmarksStream.AddListener(value);
      remove => _faceLandmarksStream.RemoveListener(value);
    }

    public event EventHandler<OutputEventArgs<NormalizedLandmarkList>> OnLeftHandLandmarksOutput
    {
      add => _leftHandLandmarksStream.AddListener(value);
      remove => _leftHandLandmarksStream.RemoveListener(value);
    }

    public event EventHandler<OutputEventArgs<NormalizedLandmarkList>> OnRightHandLandmarksOutput
    {
      add => _rightHandLandmarksStream.AddListener(value);
      remove => _rightHandLandmarksStream.RemoveListener(value);
    }

    public event EventHandler<OutputEventArgs<LandmarkList>> OnPoseWorldLandmarksOutput
    {
      add => _poseWorldLandmarksStream.AddListener(value);
      remove => _poseWorldLandmarksStream.RemoveListener(value);
    }

    public event EventHandler<OutputEventArgs<ImageFrame>> OnSegmentationMaskOutput
    {
      add => _segmentationMaskStream.AddListener(value);
      remove => _segmentationMaskStream.RemoveListener(value);
    }

    public event EventHandler<OutputEventArgs<NormalizedRect>> OnPoseRoiOutput
    {
      add => _poseRoiStream.AddListener(value);
      remove => _poseRoiStream.RemoveListener(value);
    }

    private const string _InputStreamName = "input_video";
    private const string _PoseDetectionStreamName = "pose_detection";
    private const string _PoseLandmarksStreamName = "pose_landmarks";
    private const string _FaceLandmarksStreamName = "face_landmarks";
    private const string _LeftHandLandmarksStreamName = "left_hand_landmarks";
    private const string _RightHandLandmarksStreamName = "right_hand_landmarks";
    private const string _PoseWorldLandmarksStreamName = "pose_world_landmarks";
    private const string _SegmentationMaskStreamName = "segmentation_mask";
    private const string _PoseRoiStreamName = "pose_roi";

    private OutputStream<DetectionPacket, Detection> _poseDetectionStream;
    private OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList> _poseLandmarksStream;
    private OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList> _faceLandmarksStream;
    private OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList> _leftHandLandmarksStream;
    private OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList> _rightHandLandmarksStream;
    private OutputStream<LandmarkListPacket, LandmarkList> _poseWorldLandmarksStream;
    private OutputStream<ImageFramePacket, ImageFrame> _segmentationMaskStream;
    private OutputStream<NormalizedRectPacket, NormalizedRect> _poseRoiStream;

    public override void StartRun(ImageSource imageSource)
    {
      if (runningMode.IsSynchronous())
      {
        _poseDetectionStream.StartPolling().AssertOk();
        _poseLandmarksStream.StartPolling().AssertOk();
        _faceLandmarksStream.StartPolling().AssertOk();
        _leftHandLandmarksStream.StartPolling().AssertOk();
        _rightHandLandmarksStream.StartPolling().AssertOk();
        _poseWorldLandmarksStream.StartPolling().AssertOk();
        _segmentationMaskStream.StartPolling().AssertOk();
        _poseRoiStream.StartPolling().AssertOk();
      }
      StartRun(BuildSidePacket(imageSource));
    }

    public override void Stop()
    {
      _poseDetectionStream.RemoveAllListeners();
      _poseDetectionStream = null;
      _poseLandmarksStream.RemoveAllListeners();
      _poseLandmarksStream = null;
      _faceLandmarksStream.RemoveAllListeners();
      _faceLandmarksStream = null;
      _leftHandLandmarksStream.RemoveAllListeners();
      _leftHandLandmarksStream = null;
      _rightHandLandmarksStream.RemoveAllListeners();
      _rightHandLandmarksStream = null;
      _poseWorldLandmarksStream.RemoveAllListeners();
      _poseWorldLandmarksStream = null;
      _segmentationMaskStream.RemoveAllListeners();
      _segmentationMaskStream = null;
      _poseRoiStream.RemoveAllListeners();
      _poseRoiStream = null;
      base.Stop();
    }

    public void AddTextureFrameToInputStream(TextureFrame textureFrame)
    {
      AddTextureFrameToInputStream(_InputStreamName, textureFrame);
    }

    public bool TryGetNext(out Detection poseDetection, out NormalizedLandmarkList poseLandmarks, out NormalizedLandmarkList faceLandmarks, out NormalizedLandmarkList leftHandLandmarks,
                           out NormalizedLandmarkList rightHandLandmarks, out LandmarkList poseWorldLandmarks, out ImageFrame segmentationMask, out NormalizedRect poseRoi, bool allowBlock = true)
    {
      var currentTimestampMicrosec = GetCurrentTimestampMicrosec();
      var r1 = TryGetNext(_poseDetectionStream, out poseDetection, allowBlock, currentTimestampMicrosec);
      var r2 = TryGetNext(_poseLandmarksStream, out poseLandmarks, allowBlock, currentTimestampMicrosec);
      var r3 = TryGetNext(_faceLandmarksStream, out faceLandmarks, allowBlock, currentTimestampMicrosec);
      var r4 = TryGetNext(_leftHandLandmarksStream, out leftHandLandmarks, allowBlock, currentTimestampMicrosec);
      var r5 = TryGetNext(_rightHandLandmarksStream, out rightHandLandmarks, allowBlock, currentTimestampMicrosec);
      var r6 = TryGetNext(_poseWorldLandmarksStream, out poseWorldLandmarks, allowBlock, currentTimestampMicrosec);
      var r7 = TryGetNext(_segmentationMaskStream, out segmentationMask, allowBlock, currentTimestampMicrosec);
      var r8 = TryGetNext(_poseRoiStream, out poseRoi, allowBlock, currentTimestampMicrosec);

      return r1 || r2 || r3 || r4 || r5 || r6 || r7 || r8;
    }

        public bool TryGetNext(out NormalizedLandmarkList faceLandmarks, out NormalizedLandmarkList leftHandLandmarks,
                                  out NormalizedLandmarkList rightHandLandmarks, bool allowBlock = true)
        {
            var currentTimestampMicrosec = GetCurrentTimestampMicrosec();
            var r2 = TryGetNext(_faceLandmarksStream, out faceLandmarks, allowBlock, currentTimestampMicrosec);
            var r3 = TryGetNext(_leftHandLandmarksStream, out leftHandLandmarks, allowBlock, currentTimestampMicrosec);
            var r4 = TryGetNext(_rightHandLandmarksStream, out rightHandLandmarks, allowBlock, currentTimestampMicrosec);


            return  r2 || r3 || r4;
        }

        protected override IList<WaitForResult> RequestDependentAssets()
    {
      return new List<WaitForResult> {
        WaitForAsset("face_detection_short_range.bytes"),
        WaitForAsset(refineFaceLandmarks ? "face_landmark_with_attention.bytes" : "face_landmark.bytes"),
        WaitForAsset("iris_landmark.bytes"),
        WaitForAsset("hand_landmark_full.bytes"),
        WaitForAsset("hand_recrop.bytes"),
        WaitForAsset("handedness.txt"),
        WaitForAsset("palm_detection_full.bytes"),
        WaitForAsset("pose_detection.bytes"),
        WaitForPoseLandmarkModel(),
      };
    }

    private WaitForResult WaitForPoseLandmarkModel()
    {
      switch (modelComplexity)
      {
        case ModelComplexity.Lite: return WaitForAsset("pose_landmark_lite.bytes");
        case ModelComplexity.Full: return WaitForAsset("pose_landmark_full.bytes");
        case ModelComplexity.Heavy: return WaitForAsset("pose_landmark_heavy.bytes");
        default: throw new InternalException($"Invalid model complexity: {modelComplexity}");
      }
    }

    protected override Status ConfigureCalculatorGraph(CalculatorGraphConfig config)
    {
      if (runningMode == RunningMode.NonBlockingSync)
      {
        _poseDetectionStream = new OutputStream<DetectionPacket, Detection>(
            calculatorGraph, _PoseDetectionStreamName, config.AddPacketPresenceCalculator(_PoseDetectionStreamName), timeoutMicrosec);
        _poseLandmarksStream = new OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList>(
            calculatorGraph, _PoseLandmarksStreamName, config.AddPacketPresenceCalculator(_PoseLandmarksStreamName), timeoutMicrosec);
        _faceLandmarksStream = new OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList>(
            calculatorGraph, _FaceLandmarksStreamName, config.AddPacketPresenceCalculator(_FaceLandmarksStreamName), timeoutMicrosec);
        _leftHandLandmarksStream = new OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList>(
            calculatorGraph, _LeftHandLandmarksStreamName, config.AddPacketPresenceCalculator(_LeftHandLandmarksStreamName), timeoutMicrosec);
        _rightHandLandmarksStream = new OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList>(
            calculatorGraph, _RightHandLandmarksStreamName, config.AddPacketPresenceCalculator(_RightHandLandmarksStreamName), timeoutMicrosec);
        _poseWorldLandmarksStream = new OutputStream<LandmarkListPacket, LandmarkList>(
            calculatorGraph, _PoseWorldLandmarksStreamName, config.AddPacketPresenceCalculator(_PoseWorldLandmarksStreamName), timeoutMicrosec);
        _segmentationMaskStream = new OutputStream<ImageFramePacket, ImageFrame>(
            calculatorGraph, _SegmentationMaskStreamName, config.AddPacketPresenceCalculator(_SegmentationMaskStreamName), timeoutMicrosec);
        _poseRoiStream = new OutputStream<NormalizedRectPacket, NormalizedRect>(
            calculatorGraph, _PoseRoiStreamName, config.AddPacketPresenceCalculator(_PoseRoiStreamName), timeoutMicrosec);
      }
      else
      {
        _poseDetectionStream = new OutputStream<DetectionPacket, Detection>(calculatorGraph, _PoseDetectionStreamName, true, timeoutMicrosec);
        _poseLandmarksStream = new OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList>(calculatorGraph, _PoseLandmarksStreamName, true, timeoutMicrosec);
        _faceLandmarksStream = new OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList>(calculatorGraph, _FaceLandmarksStreamName, true, timeoutMicrosec);
        _leftHandLandmarksStream = new OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList>(calculatorGraph, _LeftHandLandmarksStreamName, true, timeoutMicrosec);
        _rightHandLandmarksStream = new OutputStream<NormalizedLandmarkListPacket, NormalizedLandmarkList>(calculatorGraph, _RightHandLandmarksStreamName, true, timeoutMicrosec);
        _poseWorldLandmarksStream = new OutputStream<LandmarkListPacket, LandmarkList>(calculatorGraph, _PoseWorldLandmarksStreamName, true, timeoutMicrosec);
        _segmentationMaskStream = new OutputStream<ImageFramePacket, ImageFrame>(calculatorGraph, _SegmentationMaskStreamName, true, timeoutMicrosec);
        _poseRoiStream = new OutputStream<NormalizedRectPacket, NormalizedRect>(calculatorGraph, _PoseRoiStreamName, true, timeoutMicrosec);
      }

      using (var validatedGraphConfig = new ValidatedGraphConfig())
      {
        var status = validatedGraphConfig.Initialize(config);

        if (!status.Ok()) { return status; }

        var extensionRegistry = new ExtensionRegistry() { TensorsToDetectionsCalculatorOptions.Extensions.Ext, ThresholdingCalculatorOptions.Extensions.Ext };
        var cannonicalizedConfig = validatedGraphConfig.Config(extensionRegistry);

        var poseDetectionCalculatorPattern = new Regex("__posedetection[a-z]+__TensorsToDetectionsCalculator$");
        var tensorsToDetectionsCalculators = cannonicalizedConfig.Node.Where((node) => poseDetectionCalculatorPattern.Match(node.Name).Success).ToList();

        var poseTrackingCalculatorPattern = new Regex("tensorstoposelandmarksandsegmentation__ThresholdingCalculator$");
        var thresholdingCalculators = cannonicalizedConfig.Node.Where((node) => poseTrackingCalculatorPattern.Match(node.Name).Success).ToList();

        foreach (var calculator in tensorsToDetectionsCalculators)
        {
          if (calculator.Options.HasExtension(TensorsToDetectionsCalculatorOptions.Extensions.Ext))
          {
            var options = calculator.Options.GetExtension(TensorsToDetectionsCalculatorOptions.Extensions.Ext);
            options.MinScoreThresh = minDetectionConfidence;
            Logger.LogInfo(TAG, $"Min Detection Confidence = {minDetectionConfidence}");
          }
        }

        foreach (var calculator in thresholdingCalculators)
        {
          if (calculator.Options.HasExtension(ThresholdingCalculatorOptions.Extensions.Ext))
          {
            var options = calculator.Options.GetExtension(ThresholdingCalculatorOptions.Extensions.Ext);
            options.Threshold = minTrackingConfidence;
            Logger.LogInfo(TAG, $"Min Tracking Confidence = {minTrackingConfidence}");
          }
        }
        return calculatorGraph.Initialize(cannonicalizedConfig);
      }
    }

    private SidePacket BuildSidePacket(ImageSource imageSource)
    {
      var sidePacket = new SidePacket();

      SetImageTransformationOptions(sidePacket, imageSource);

      // TODO: refactoring
      // The orientation of the output image must match that of the input image.
      var isInverted = CoordinateSystem.ImageCoordinate.IsInverted(imageSource.rotation);
      var outputRotation = imageSource.rotation;
      var outputHorizontallyFlipped = !isInverted && imageSource.isHorizontallyFlipped;
      var outputVerticallyFlipped = (!runningMode.IsSynchronous() && imageSource.isVerticallyFlipped) ^ (isInverted && imageSource.isHorizontallyFlipped);

      if ((outputHorizontallyFlipped && outputVerticallyFlipped) || outputRotation == RotationAngle.Rotation180)
      {
        outputRotation = outputRotation.Add(RotationAngle.Rotation180);
        outputHorizontallyFlipped = !outputHorizontallyFlipped;
        outputVerticallyFlipped = !outputVerticallyFlipped;
      }

      sidePacket.Emplace("output_rotation", new IntPacket((int)outputRotation));
      sidePacket.Emplace("output_horizontally_flipped", new BoolPacket(outputHorizontallyFlipped));
      sidePacket.Emplace("output_vertically_flipped", new BoolPacket(outputVerticallyFlipped));

      Logger.LogDebug($"output_rotation = {outputRotation}, output_horizontally_flipped = {outputHorizontallyFlipped}, output_vertically_flipped = {outputVerticallyFlipped}");

      sidePacket.Emplace("refine_face_landmarks", new BoolPacket(refineFaceLandmarks));
      sidePacket.Emplace("model_complexity", new IntPacket((int)modelComplexity));
      sidePacket.Emplace("smooth_landmarks", new BoolPacket(smoothLandmarks));
      sidePacket.Emplace("enable_segmentation", new BoolPacket(enableSegmentation));
      sidePacket.Emplace("smooth_segmentation", new BoolPacket(smoothSegmentation));

      Logger.LogInfo(TAG, $"Refine Face Landmarks = {refineFaceLandmarks}");
      Logger.LogInfo(TAG, $"Model Complexity = {modelComplexity}");
      Logger.LogInfo(TAG, $"Smooth Landmarks = {smoothLandmarks}");
      Logger.LogInfo(TAG, $"Enable Segmentation = {enableSegmentation}");
      Logger.LogInfo(TAG, $"Smooth Segmentation = {smoothSegmentation}");

      return sidePacket;
    }
  }
}

```



> MultiHandLandmarkListAnnotation.cs

```c#
// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections.Generic;
using UnityEngine;

namespace Mediapipe.Unity
{
#pragma warning disable IDE0065
    using Color = UnityEngine.Color;
#pragma warning restore IDE0065

    public sealed class MultiHandLandmarkListAnnotation : ListAnnotation<HandLandmarkListAnnotation>
    {
        [SerializeField] private Color _leftLandmarkColor = Color.green;
        [SerializeField] private Color _rightLandmarkColor = Color.green;
        [SerializeField] private float _landmarkRadius = 15.0f;
        [SerializeField] private Color _connectionColor = Color.white;
        [SerializeField, Range(0, 1)] private float _connectionWidth = 1.0f;
        [SerializeField] private GameObject _landMarkExtracter = default;
        private void OnValidate()
        {
            ApplyLeftLandmarkColor(_leftLandmarkColor);
            ApplyRightLandmarkColor(_rightLandmarkColor);
            ApplyLandmarkRadius(_landmarkRadius);
            ApplyConnectionColor(_connectionColor);
            ApplyConnectionWidth(_connectionWidth);
        }

        public void SetLeftLandmarkColor(Color leftLandmarkColor)
        {
            _leftLandmarkColor = leftLandmarkColor;
            ApplyLeftLandmarkColor(_leftLandmarkColor);
        }

        public void SetRightLandmarkColor(Color rightLandmarkColor)
        {
            _rightLandmarkColor = rightLandmarkColor;
            ApplyRightLandmarkColor(_rightLandmarkColor);
        }

        public void SetLandmarkRadius(float landmarkRadius)
        {
            _landmarkRadius = landmarkRadius;
            ApplyLandmarkRadius(_landmarkRadius);
        }

        public void SetConnectionColor(Color connectionColor)
        {
            _connectionColor = connectionColor;
            ApplyConnectionColor(_connectionColor);
        }

        public void SetConnectionWidth(float connectionWidth)
        {
            _connectionWidth = connectionWidth;
            ApplyConnectionWidth(_connectionWidth);
        }

        public void SetHandedness(IList<ClassificationList> handedness)
        {
            var count = handedness == null ? 0 : handedness.Count;
            for (var i = 0; i < Mathf.Min(count, children.Count); i++)
            {
                children[i].SetHandedness(handedness[i]);
            }
            for (var i = count; i < children.Count; i++)
            {
                children[i].SetHandedness((IList<Classification>)null);
            }
        }

        public void Draw(IList<NormalizedLandmarkList> targets, bool visualizeZ = false)
        {
            if (ActivateFor(targets))
            {
                CallActionForAll(targets, (annotation, target) =>
                {
                    if (annotation != null) { annotation.Draw(target, visualizeZ); }
                });
            }
        }

        protected override HandLandmarkListAnnotation InstantiateChild(bool isActive = true)
        {
            var annotation = base.InstantiateChild(isActive);
            annotation.LandMarkExtracter = _landMarkExtracter;
            annotation.SetLeftLandmarkColor(_leftLandmarkColor);
            annotation.SetRightLandmarkColor(_rightLandmarkColor);
            annotation.SetLandmarkRadius(_landmarkRadius);
            annotation.SetConnectionColor(_connectionColor);
            annotation.SetConnectionWidth(_connectionWidth);
            return annotation;
        }

        private void ApplyLeftLandmarkColor(Color leftLandmarkColor)
        {
            foreach (var handLandmarkList in children)
            {
                if (handLandmarkList != null) { handLandmarkList.SetLeftLandmarkColor(leftLandmarkColor); }
            }
        }

        private void ApplyRightLandmarkColor(Color rightLandmarkColor)
        {
            foreach (var handLandmarkList in children)
            {
                if (handLandmarkList != null) { handLandmarkList.SetRightLandmarkColor(rightLandmarkColor); }
            }
        }

        private void ApplyLandmarkRadius(float landmarkRadius)
        {
            foreach (var handLandmarkList in children)
            {
                if (handLandmarkList != null) { handLandmarkList.SetLandmarkRadius(landmarkRadius); }
            }
        }

        private void ApplyConnectionColor(Color connectionColor)
        {
            foreach (var handLandmarkList in children)
            {
                if (handLandmarkList != null) { handLandmarkList.SetConnectionColor(connectionColor); }
            }
        }

        private void ApplyConnectionWidth(float connectionWidth)
        {
            foreach (var handLandmarkList in children)
            {
                if (handLandmarkList != null) { handLandmarkList.SetConnectionWidth(connectionWidth); }
            }
        }
    }
}

```





> Landmark

```c#
// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: mediapipe/framework/formats/landmark.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Mediapipe {

  /// <summary>Holder for reflection information generated from mediapipe/framework/formats/landmark.proto</summary>
  public static partial class LandmarkReflection {

    #region Descriptor
    /// <summary>File descriptor for mediapipe/framework/formats/landmark.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static LandmarkReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CiptZWRpYXBpcGUvZnJhbWV3b3JrL2Zvcm1hdHMvbGFuZG1hcmsucHJvdG8S",
            "CW1lZGlhcGlwZSJRCghMYW5kbWFyaxIJCgF4GAEgASgCEgkKAXkYAiABKAIS",
            "CQoBehgDIAEoAhISCgp2aXNpYmlsaXR5GAQgASgCEhAKCHByZXNlbmNlGAUg",
            "ASgCIjUKDExhbmRtYXJrTGlzdBIlCghsYW5kbWFyaxgBIAMoCzITLm1lZGlh",
            "cGlwZS5MYW5kbWFyayJIChZMYW5kbWFya0xpc3RDb2xsZWN0aW9uEi4KDWxh",
            "bmRtYXJrX2xpc3QYASADKAsyFy5tZWRpYXBpcGUuTGFuZG1hcmtMaXN0IlsK",
            "Ek5vcm1hbGl6ZWRMYW5kbWFyaxIJCgF4GAEgASgCEgkKAXkYAiABKAISCQoB",
            "ehgDIAEoAhISCgp2aXNpYmlsaXR5GAQgASgCEhAKCHByZXNlbmNlGAUgASgC",
            "IkkKFk5vcm1hbGl6ZWRMYW5kbWFya0xpc3QSLwoIbGFuZG1hcmsYASADKAsy",
            "HS5tZWRpYXBpcGUuTm9ybWFsaXplZExhbmRtYXJrIlwKIE5vcm1hbGl6ZWRM",
            "YW5kbWFya0xpc3RDb2xsZWN0aW9uEjgKDWxhbmRtYXJrX2xpc3QYASADKAsy",
            "IS5tZWRpYXBpcGUuTm9ybWFsaXplZExhbmRtYXJrTGlzdEIzCiJjb20uZ29v",
            "Z2xlLm1lZGlhcGlwZS5mb3JtYXRzLnByb3RvQg1MYW5kbWFya1Byb3Rv"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Mediapipe.Landmark), global::Mediapipe.Landmark.Parser, new[]{ "X", "Y", "Z", "Visibility", "Presence" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Mediapipe.LandmarkList), global::Mediapipe.LandmarkList.Parser, new[]{ "Landmark" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Mediapipe.LandmarkListCollection), global::Mediapipe.LandmarkListCollection.Parser, new[]{ "LandmarkList" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Mediapipe.NormalizedLandmark), global::Mediapipe.NormalizedLandmark.Parser, new[]{ "X", "Y", "Z", "Visibility", "Presence" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Mediapipe.NormalizedLandmarkList), global::Mediapipe.NormalizedLandmarkList.Parser, new[]{ "Landmark" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Mediapipe.NormalizedLandmarkListCollection), global::Mediapipe.NormalizedLandmarkListCollection.Parser, new[]{ "LandmarkList" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  /// <summary>
  /// A landmark that can have 1 to 3 dimensions. Use x for 1D points, (x, y) for
  /// 2D points and (x, y, z) for 3D points. For more dimensions, consider using
  /// matrix_data.proto.
  /// </summary>
  public sealed partial class Landmark : pb::IMessage<Landmark>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<Landmark> _parser = new pb::MessageParser<Landmark>(() => new Landmark());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<Landmark> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Mediapipe.LandmarkReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public Landmark() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public Landmark(Landmark other) : this() {
      _hasBits0 = other._hasBits0;
      x_ = other.x_;
      y_ = other.y_;
      z_ = other.z_;
      visibility_ = other.visibility_;
      presence_ = other.presence_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public Landmark Clone() {
      return new Landmark(this);
    }

    /// <summary>Field number for the "x" field.</summary>
    public const int XFieldNumber = 1;
    private readonly static float XDefaultValue = 0F;

    private float x_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float X {
      get { if ((_hasBits0 & 1) != 0) { return x_; } else { return XDefaultValue; } }
      set {
        _hasBits0 |= 1;
        x_ = value;
      }
    }
    /// <summary>Gets whether the "x" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasX {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "x" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearX() {
      _hasBits0 &= ~1;
    }

    /// <summary>Field number for the "y" field.</summary>
    public const int YFieldNumber = 2;
    private readonly static float YDefaultValue = 0F;

    private float y_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Y {
      get { if ((_hasBits0 & 2) != 0) { return y_; } else { return YDefaultValue; } }
      set {
        _hasBits0 |= 2;
        y_ = value;
      }
    }
    /// <summary>Gets whether the "y" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasY {
      get { return (_hasBits0 & 2) != 0; }
    }
    /// <summary>Clears the value of the "y" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearY() {
      _hasBits0 &= ~2;
    }

    /// <summary>Field number for the "z" field.</summary>
    public const int ZFieldNumber = 3;
    private readonly static float ZDefaultValue = 0F;

    private float z_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Z {
      get { if ((_hasBits0 & 4) != 0) { return z_; } else { return ZDefaultValue; } }
      set {
        _hasBits0 |= 4;
        z_ = value;
      }
    }
    /// <summary>Gets whether the "z" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasZ {
      get { return (_hasBits0 & 4) != 0; }
    }
    /// <summary>Clears the value of the "z" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearZ() {
      _hasBits0 &= ~4;
    }

    /// <summary>Field number for the "visibility" field.</summary>
    public const int VisibilityFieldNumber = 4;
    private readonly static float VisibilityDefaultValue = 0F;

    private float visibility_;
    /// <summary>
    /// Landmark visibility. Should stay unset if not supported.
    /// Float score of whether landmark is visible or occluded by other objects.
    /// Landmark considered as invisible also if it is not present on the screen
    /// (out of scene bounds). Depending on the model, visibility value is either a
    /// sigmoid or an argument of sigmoid.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Visibility {
      get { if ((_hasBits0 & 8) != 0) { return visibility_; } else { return VisibilityDefaultValue; } }
      set {
        _hasBits0 |= 8;
        visibility_ = value;
      }
    }
    /// <summary>Gets whether the "visibility" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasVisibility {
      get { return (_hasBits0 & 8) != 0; }
    }
    /// <summary>Clears the value of the "visibility" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearVisibility() {
      _hasBits0 &= ~8;
    }

    /// <summary>Field number for the "presence" field.</summary>
    public const int PresenceFieldNumber = 5;
    private readonly static float PresenceDefaultValue = 0F;

    private float presence_;
    /// <summary>
    /// Landmark presence. Should stay unset if not supported.
    /// Float score of whether landmark is present on the scene (located within
    /// scene bounds). Depending on the model, presence value is either a result of
    /// sigmoid or an argument of sigmoid function to get landmark presence
    /// probability.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Presence {
      get { if ((_hasBits0 & 16) != 0) { return presence_; } else { return PresenceDefaultValue; } }
      set {
        _hasBits0 |= 16;
        presence_ = value;
      }
    }
    /// <summary>Gets whether the "presence" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasPresence {
      get { return (_hasBits0 & 16) != 0; }
    }
    /// <summary>Clears the value of the "presence" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearPresence() {
      _hasBits0 &= ~16;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as Landmark);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(Landmark other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(X, other.X)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Y, other.Y)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Z, other.Z)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Visibility, other.Visibility)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Presence, other.Presence)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (HasX) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(X);
      if (HasY) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Y);
      if (HasZ) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Z);
      if (HasVisibility) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Visibility);
      if (HasPresence) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Presence);
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (HasX) {
        output.WriteRawTag(13);
        output.WriteFloat(X);
      }
      if (HasY) {
        output.WriteRawTag(21);
        output.WriteFloat(Y);
      }
      if (HasZ) {
        output.WriteRawTag(29);
        output.WriteFloat(Z);
      }
      if (HasVisibility) {
        output.WriteRawTag(37);
        output.WriteFloat(Visibility);
      }
      if (HasPresence) {
        output.WriteRawTag(45);
        output.WriteFloat(Presence);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (HasX) {
        output.WriteRawTag(13);
        output.WriteFloat(X);
      }
      if (HasY) {
        output.WriteRawTag(21);
        output.WriteFloat(Y);
      }
      if (HasZ) {
        output.WriteRawTag(29);
        output.WriteFloat(Z);
      }
      if (HasVisibility) {
        output.WriteRawTag(37);
        output.WriteFloat(Visibility);
      }
      if (HasPresence) {
        output.WriteRawTag(45);
        output.WriteFloat(Presence);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (HasX) {
        size += 1 + 4;
      }
      if (HasY) {
        size += 1 + 4;
      }
      if (HasZ) {
        size += 1 + 4;
      }
      if (HasVisibility) {
        size += 1 + 4;
      }
      if (HasPresence) {
        size += 1 + 4;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(Landmark other) {
      if (other == null) {
        return;
      }
      if (other.HasX) {
        X = other.X;
      }
      if (other.HasY) {
        Y = other.Y;
      }
      if (other.HasZ) {
        Z = other.Z;
      }
      if (other.HasVisibility) {
        Visibility = other.Visibility;
      }
      if (other.HasPresence) {
        Presence = other.Presence;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 13: {
            X = input.ReadFloat();
            break;
          }
          case 21: {
            Y = input.ReadFloat();
            break;
          }
          case 29: {
            Z = input.ReadFloat();
            break;
          }
          case 37: {
            Visibility = input.ReadFloat();
            break;
          }
          case 45: {
            Presence = input.ReadFloat();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 13: {
            X = input.ReadFloat();
            break;
          }
          case 21: {
            Y = input.ReadFloat();
            break;
          }
          case 29: {
            Z = input.ReadFloat();
            break;
          }
          case 37: {
            Visibility = input.ReadFloat();
            break;
          }
          case 45: {
            Presence = input.ReadFloat();
            break;
          }
        }
      }
    }
    #endif

  }

  /// <summary>
  /// Group of Landmark protos.
  /// </summary>
  public sealed partial class LandmarkList : pb::IMessage<LandmarkList>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<LandmarkList> _parser = new pb::MessageParser<LandmarkList>(() => new LandmarkList());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<LandmarkList> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Mediapipe.LandmarkReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public LandmarkList() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public LandmarkList(LandmarkList other) : this() {
      landmark_ = other.landmark_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public LandmarkList Clone() {
      return new LandmarkList(this);
    }

    /// <summary>Field number for the "landmark" field.</summary>
    public const int LandmarkFieldNumber = 1;
    private static readonly pb::FieldCodec<global::Mediapipe.Landmark> _repeated_landmark_codec
        = pb::FieldCodec.ForMessage(10, global::Mediapipe.Landmark.Parser);
    private readonly pbc::RepeatedField<global::Mediapipe.Landmark> landmark_ = new pbc::RepeatedField<global::Mediapipe.Landmark>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<global::Mediapipe.Landmark> Landmark {
      get { return landmark_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as LandmarkList);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(LandmarkList other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!landmark_.Equals(other.landmark_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= landmark_.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      landmark_.WriteTo(output, _repeated_landmark_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      landmark_.WriteTo(ref output, _repeated_landmark_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      size += landmark_.CalculateSize(_repeated_landmark_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(LandmarkList other) {
      if (other == null) {
        return;
      }
      landmark_.Add(other.landmark_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            landmark_.AddEntriesFrom(input, _repeated_landmark_codec);
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            landmark_.AddEntriesFrom(ref input, _repeated_landmark_codec);
            break;
          }
        }
      }
    }
    #endif

  }

  /// <summary>
  /// Group of LandmarkList protos.
  /// </summary>
  public sealed partial class LandmarkListCollection : pb::IMessage<LandmarkListCollection>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<LandmarkListCollection> _parser = new pb::MessageParser<LandmarkListCollection>(() => new LandmarkListCollection());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<LandmarkListCollection> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Mediapipe.LandmarkReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public LandmarkListCollection() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public LandmarkListCollection(LandmarkListCollection other) : this() {
      landmarkList_ = other.landmarkList_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public LandmarkListCollection Clone() {
      return new LandmarkListCollection(this);
    }

    /// <summary>Field number for the "landmark_list" field.</summary>
    public const int LandmarkListFieldNumber = 1;
    private static readonly pb::FieldCodec<global::Mediapipe.LandmarkList> _repeated_landmarkList_codec
        = pb::FieldCodec.ForMessage(10, global::Mediapipe.LandmarkList.Parser);
    private readonly pbc::RepeatedField<global::Mediapipe.LandmarkList> landmarkList_ = new pbc::RepeatedField<global::Mediapipe.LandmarkList>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<global::Mediapipe.LandmarkList> LandmarkList {
      get { return landmarkList_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as LandmarkListCollection);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(LandmarkListCollection other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!landmarkList_.Equals(other.landmarkList_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= landmarkList_.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      landmarkList_.WriteTo(output, _repeated_landmarkList_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      landmarkList_.WriteTo(ref output, _repeated_landmarkList_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      size += landmarkList_.CalculateSize(_repeated_landmarkList_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(LandmarkListCollection other) {
      if (other == null) {
        return;
      }
      landmarkList_.Add(other.landmarkList_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            landmarkList_.AddEntriesFrom(input, _repeated_landmarkList_codec);
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            landmarkList_.AddEntriesFrom(ref input, _repeated_landmarkList_codec);
            break;
          }
        }
      }
    }
    #endif

  }

  /// <summary>
  /// A normalized version of above Landmark proto. All coordinates should be
  /// within [0, 1].
  /// </summary>
  public sealed partial class NormalizedLandmark : pb::IMessage<NormalizedLandmark>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<NormalizedLandmark> _parser = new pb::MessageParser<NormalizedLandmark>(() => new NormalizedLandmark());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<NormalizedLandmark> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Mediapipe.LandmarkReflection.Descriptor.MessageTypes[3]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public NormalizedLandmark() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public NormalizedLandmark(NormalizedLandmark other) : this() {
      _hasBits0 = other._hasBits0;
      x_ = other.x_;
      y_ = other.y_;
      z_ = other.z_;
      visibility_ = other.visibility_;
      presence_ = other.presence_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public NormalizedLandmark Clone() {
      return new NormalizedLandmark(this);
    }

    /// <summary>Field number for the "x" field.</summary>
    public const int XFieldNumber = 1;
    private readonly static float XDefaultValue = 0F;

    private float x_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float X {
      get { if ((_hasBits0 & 1) != 0) { return x_; } else { return XDefaultValue; } }
      set {
        _hasBits0 |= 1;
        x_ = value;
      }
    }
        public int index;
    /// <summary>Gets whether the "x" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasX {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "x" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearX() {
      _hasBits0 &= ~1;
    }

    /// <summary>Field number for the "y" field.</summary>
    public const int YFieldNumber = 2;
    private readonly static float YDefaultValue = 0F;

    private float y_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Y {
      get { if ((_hasBits0 & 2) != 0) { return y_; } else { return YDefaultValue; } }
      set {
        _hasBits0 |= 2;
        y_ = value;
      }
    }
    /// <summary>Gets whether the "y" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasY {
      get { return (_hasBits0 & 2) != 0; }
    }
    /// <summary>Clears the value of the "y" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearY() {
      _hasBits0 &= ~2;
    }

    /// <summary>Field number for the "z" field.</summary>
    public const int ZFieldNumber = 3;
    private readonly static float ZDefaultValue = 0F;

    private float z_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Z {
      get { if ((_hasBits0 & 4) != 0) { return z_; } else { return ZDefaultValue; } }
      set {
        _hasBits0 |= 4;
        z_ = value;
      }
    }
    /// <summary>Gets whether the "z" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasZ {
      get { return (_hasBits0 & 4) != 0; }
    }
    /// <summary>Clears the value of the "z" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearZ() {
      _hasBits0 &= ~4;
    }

    /// <summary>Field number for the "visibility" field.</summary>
    public const int VisibilityFieldNumber = 4;
    private readonly static float VisibilityDefaultValue = 0F;

    private float visibility_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Visibility {
      get { if ((_hasBits0 & 8) != 0) { return visibility_; } else { return VisibilityDefaultValue; } }
      set {
        _hasBits0 |= 8;
        visibility_ = value;
      }
    }
    /// <summary>Gets whether the "visibility" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasVisibility {
      get { return (_hasBits0 & 8) != 0; }
    }
    /// <summary>Clears the value of the "visibility" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearVisibility() {
      _hasBits0 &= ~8;
    }

    /// <summary>Field number for the "presence" field.</summary>
    public const int PresenceFieldNumber = 5;
    private readonly static float PresenceDefaultValue = 0F;

    private float presence_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Presence {
      get { if ((_hasBits0 & 16) != 0) { return presence_; } else { return PresenceDefaultValue; } }
      set {
        _hasBits0 |= 16;
        presence_ = value;
      }
    }
    /// <summary>Gets whether the "presence" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasPresence {
      get { return (_hasBits0 & 16) != 0; }
    }
    /// <summary>Clears the value of the "presence" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearPresence() {
      _hasBits0 &= ~16;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as NormalizedLandmark);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(NormalizedLandmark other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(X, other.X)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Y, other.Y)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Z, other.Z)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Visibility, other.Visibility)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Presence, other.Presence)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (HasX) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(X);
      if (HasY) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Y);
      if (HasZ) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Z);
      if (HasVisibility) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Visibility);
      if (HasPresence) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Presence);
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (HasX) {
        output.WriteRawTag(13);
        output.WriteFloat(X);
      }
      if (HasY) {
        output.WriteRawTag(21);
        output.WriteFloat(Y);
      }
      if (HasZ) {
        output.WriteRawTag(29);
        output.WriteFloat(Z);
      }
      if (HasVisibility) {
        output.WriteRawTag(37);
        output.WriteFloat(Visibility);
      }
      if (HasPresence) {
        output.WriteRawTag(45);
        output.WriteFloat(Presence);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (HasX) {
        output.WriteRawTag(13);
        output.WriteFloat(X);
      }
      if (HasY) {
        output.WriteRawTag(21);
        output.WriteFloat(Y);
      }
      if (HasZ) {
        output.WriteRawTag(29);
        output.WriteFloat(Z);
      }
      if (HasVisibility) {
        output.WriteRawTag(37);
        output.WriteFloat(Visibility);
      }
      if (HasPresence) {
        output.WriteRawTag(45);
        output.WriteFloat(Presence);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (HasX) {
        size += 1 + 4;
      }
      if (HasY) {
        size += 1 + 4;
      }
      if (HasZ) {
        size += 1 + 4;
      }
      if (HasVisibility) {
        size += 1 + 4;
      }
      if (HasPresence) {
        size += 1 + 4;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(NormalizedLandmark other) {
      if (other == null) {
        return;
      }
      if (other.HasX) {
        X = other.X;
      }
      if (other.HasY) {
        Y = other.Y;
      }
      if (other.HasZ) {
        Z = other.Z;
      }
      if (other.HasVisibility) {
        Visibility = other.Visibility;
      }
      if (other.HasPresence) {
        Presence = other.Presence;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 13: {
            X = input.ReadFloat();
            break;
          }
          case 21: {
            Y = input.ReadFloat();
            break;
          }
          case 29: {
            Z = input.ReadFloat();
            break;
          }
          case 37: {
            Visibility = input.ReadFloat();
            break;
          }
          case 45: {
            Presence = input.ReadFloat();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 13: {
            X = input.ReadFloat();
            break;
          }
          case 21: {
            Y = input.ReadFloat();
            break;
          }
          case 29: {
            Z = input.ReadFloat();
            break;
          }
          case 37: {
            Visibility = input.ReadFloat();
            break;
          }
          case 45: {
            Presence = input.ReadFloat();
            break;
          }
        }
      }
    }
    #endif

  }

  /// <summary>
  /// Group of NormalizedLandmark protos.
  /// </summary>
  public sealed partial class NormalizedLandmarkList : pb::IMessage<NormalizedLandmarkList>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<NormalizedLandmarkList> _parser = new pb::MessageParser<NormalizedLandmarkList>(() => new NormalizedLandmarkList());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<NormalizedLandmarkList> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Mediapipe.LandmarkReflection.Descriptor.MessageTypes[4]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public NormalizedLandmarkList() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public NormalizedLandmarkList(NormalizedLandmarkList other) : this() {
      landmark_ = other.landmark_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public NormalizedLandmarkList Clone() {
      return new NormalizedLandmarkList(this);
    }

    /// <summary>Field number for the "landmark" field.</summary>
    public const int LandmarkFieldNumber = 1;
    private static readonly pb::FieldCodec<global::Mediapipe.NormalizedLandmark> _repeated_landmark_codec
        = pb::FieldCodec.ForMessage(10, global::Mediapipe.NormalizedLandmark.Parser);
    private readonly pbc::RepeatedField<global::Mediapipe.NormalizedLandmark> landmark_ = new pbc::RepeatedField<global::Mediapipe.NormalizedLandmark>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<global::Mediapipe.NormalizedLandmark> Landmark {
      get { return landmark_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as NormalizedLandmarkList);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(NormalizedLandmarkList other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!landmark_.Equals(other.landmark_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= landmark_.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      landmark_.WriteTo(output, _repeated_landmark_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      landmark_.WriteTo(ref output, _repeated_landmark_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      size += landmark_.CalculateSize(_repeated_landmark_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(NormalizedLandmarkList other) {
      if (other == null) {
        return;
      }
      landmark_.Add(other.landmark_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            landmark_.AddEntriesFrom(input, _repeated_landmark_codec);
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            landmark_.AddEntriesFrom(ref input, _repeated_landmark_codec);
            break;
          }
        }
      }
    }
    #endif

  }

  /// <summary>
  /// Group of NormalizedLandmarkList protos.
  /// </summary>
  public sealed partial class NormalizedLandmarkListCollection : pb::IMessage<NormalizedLandmarkListCollection>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<NormalizedLandmarkListCollection> _parser = new pb::MessageParser<NormalizedLandmarkListCollection>(() => new NormalizedLandmarkListCollection());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<NormalizedLandmarkListCollection> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Mediapipe.LandmarkReflection.Descriptor.MessageTypes[5]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public NormalizedLandmarkListCollection() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public NormalizedLandmarkListCollection(NormalizedLandmarkListCollection other) : this() {
      landmarkList_ = other.landmarkList_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public NormalizedLandmarkListCollection Clone() {
      return new NormalizedLandmarkListCollection(this);
    }

    /// <summary>Field number for the "landmark_list" field.</summary>
    public const int LandmarkListFieldNumber = 1;
    private static readonly pb::FieldCodec<global::Mediapipe.NormalizedLandmarkList> _repeated_landmarkList_codec
        = pb::FieldCodec.ForMessage(10, global::Mediapipe.NormalizedLandmarkList.Parser);
    private readonly pbc::RepeatedField<global::Mediapipe.NormalizedLandmarkList> landmarkList_ = new pbc::RepeatedField<global::Mediapipe.NormalizedLandmarkList>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<global::Mediapipe.NormalizedLandmarkList> LandmarkList {
      get { return landmarkList_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as NormalizedLandmarkListCollection);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(NormalizedLandmarkListCollection other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!landmarkList_.Equals(other.landmarkList_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= landmarkList_.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      landmarkList_.WriteTo(output, _repeated_landmarkList_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      landmarkList_.WriteTo(ref output, _repeated_landmarkList_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      size += landmarkList_.CalculateSize(_repeated_landmarkList_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(NormalizedLandmarkListCollection other) {
      if (other == null) {
        return;
      }
      landmarkList_.Add(other.landmarkList_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            landmarkList_.AddEntriesFrom(input, _repeated_landmarkList_codec);
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            landmarkList_.AddEntriesFrom(ref input, _repeated_landmarkList_codec);
            break;
          }
        }
      }
    }
    #endif

  }

  #endregion

}

#endregion Designer generated code

```





prefab也要做更動

MediaPipe Uinty Plugin/Objects/HandLandmarkList Annotation 裡面的 Point List Annotation 的 script Point List Annotation  Annotation Prefab 使用 Prefab/UI/Acupuncture Point Item Annotation
