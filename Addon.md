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

        public PointAnnotation this[int index] => _landmarkListAnnotation[index];

        private void Start()
        {
            _landmarkListAnnotation.Fill(LandmarkCount < 21 ? 21 : LandmarkCount);
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
      if (ActivateFor(target))
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
      }
    }

    public void Draw(NormalizedPoint2D target)
    {
      if (ActivateFor(target))
      {
        var position = GetAnnotationLayer().GetLocalPosition(target, rotationAngle, isMirrored);
        transform.localPosition = position;
      }
    }

    public void Draw(Point3D target, Vector2 focalLength, Vector2 principalPoint, float zScale, bool visualizeZ = true)
    {
      if (ActivateFor(target))
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
      if (ActivateFor(target))
      {
        Draw(GetAnnotationLayer().GetLocalPosition(target, rotationAngle, isMirrored));
        SetColor(GetColor(target.Score, threshold));
      }
    }

    private void ApplyColor(Color color)
    {
      GetComponent<Renderer>().material.color = color;
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
            for (int i = 0; i < count; i++)
            {
                if(isSetupItemCore)
                    children.Add(InstantiateChild(i, false));
                else
                    children.Add(InstantiateChild(false));
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
                    children.Add(InstantiateChild());
                }
                else if (children[i] == null)
                {
                    // child is not initialized yet
                    children[i] = InstantiateChild();
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

