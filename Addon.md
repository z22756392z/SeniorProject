## Addon

### Package Manager

* input system
* cinemachine
* universal render pipeline
* localization
* addressable

### import

* mediapipe package



package裡面有script需要修改: `HolisticLandmarkListAnnotationController.cs`、`HandLandmarkListAnnotation.cs`



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

    private const int _LandmarkCount = 21;
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
      _landmarkListAnnotation.Fill(_LandmarkCount);
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
                if(_drawConnection)
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



