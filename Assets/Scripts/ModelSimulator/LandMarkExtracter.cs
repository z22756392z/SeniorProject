using Mediapipe.Unity;
using System.Collections.Generic;
using UnityEngine;
public enum Body
{
  LeftHand,
  RightHand
}

public class LandMarkExtracter : MonoBehaviour
{
  [SerializeField] private GameObject _multiHandLandmarksAnnotation = default;
  [SerializeField] private Color _leftHandColor;
  [SerializeField] private Color _rightHandColor;
  private Transform _handOneLandmarksAnnotation;
  private Transform _handTwoLandmarkAnnoation;
  public Transform HandOneLandmarksAnnotation
  {
    get
    {
      if (_multiHandLandmarksAnnotation.transform.childCount > 0)
        return _handOneLandmarksAnnotation ??= _multiHandLandmarksAnnotation.transform.GetChild(0);
      return default;
    }
  }

  public Transform HandTwoLandmarksAnnotation
  {
    get
    {
      if (_multiHandLandmarksAnnotation.transform.childCount > 1)
        return _handTwoLandmarkAnnoation ??= _multiHandLandmarksAnnotation.transform.GetChild(1);
      return default;
    }
  }

  private Transform _pointListOne = default;
  private Transform _pointListTwo = default;
  public Transform HandIndexOnePointList
  {
    get
    {
      if (HandOneLandmarksAnnotation != default)
      {
        if (HandOneLandmarksAnnotation.transform.childCount > 0)
          return _pointListOne ??= HandOneLandmarksAnnotation.GetChild(0);
      }
      return default;
    }
  }

  public Transform HandIndexTwoPointList
  {
    get
    {
      if (HandTwoLandmarksAnnotation != default)
      {
        if (HandTwoLandmarksAnnotation.transform.childCount > 0)
          return _pointListTwo ??= HandTwoLandmarksAnnotation.GetChild(0);
      }
      return default;
    }
  }

  private PointAnnotation handIndexOnePoint;
  private PointAnnotation handIndexTwoPoint;

  public PointAnnotation HandIndexOnePoint
  {
    get
    {
      if(HandIndexOnePointList != default)
      {
        if (HandIndexOnePointList.transform.childCount > 0)
          return handIndexOnePoint ??= HandIndexOnePointList.GetChild(0).GetComponent<PointAnnotation>();
      }
      return default;
    }
  }

  public PointAnnotation HandIndexTwoPoint
  {
    get
    {
      if (HandIndexTwoPointList != default)
      {
        if (HandIndexTwoPointList.transform.childCount > 0)
          return handIndexTwoPoint ??= HandIndexTwoPointList.GetChild(0).GetComponent<PointAnnotation>();
      }
      return default;
    }
  }

  [System.Serializable]
  public struct BodyLandmark
  {
    public Body body; 
    public int size;
  }
  public List<BodyLandmark> Landmarks = new List<BodyLandmark>();
  public Dictionary<Body, List<GameObject>> landmarkDictionary;

  private void Awake()
  {
    landmarkDictionary = new Dictionary<Body, List<GameObject>>();

    foreach (BodyLandmark landmark in Landmarks)
    {
      List<GameObject> landmarkList = new List<GameObject>();
      landmarkDictionary.Add(landmark.body, landmarkList);
    }
  }

  private void Update()
  {
    // only with two hand can controll
    //if (!HandIndexTwoPointList) return;
    //if (!HandIndexTwoPointList.gameObject.activeInHierarchy) return;
    HandLandmarkSetup(HandIndexOnePoint);
    HandLandmarkSetup(HandIndexTwoPoint);

    // fix one hand controll both
    if(HandIndexTwoPoint == null || !HandIndexTwoPointList.gameObject.activeInHierarchy)
    {
      List<GameObject> leftHandLandmarks = GetLandmark(Body.LeftHand);
      List<GameObject> rightHandLandmarks = GetLandmark(Body.RightHand);
      if (leftHandLandmarks.Count > 0 && rightHandLandmarks.Count > 0)
      {
        if(handIndexOnePoint.Color == _leftHandColor)
        {
          rightHandLandmarks.Clear();
        }
        else
        {
          leftHandLandmarks.Clear();
        }
      }
    }
  }

  private void HandLandmarkSetup(PointAnnotation target)
  {
    if (target == null || !target.gameObject.activeInHierarchy)
    {
      return;
    }

    DetectHand(target, Body.LeftHand, _leftHandColor);
    DetectHand(target, Body.RightHand, _rightHandColor);
  }

  public void ClearHands(PointAnnotation target)
  {
    ClearHand(target, Body.LeftHand, _leftHandColor);
    ClearHand(target, Body.RightHand, _rightHandColor);
  }

  private void ClearHand(PointAnnotation target, Body hand, Color handColor)
  {
    List<GameObject> handLandmarks = GetLandmark(hand);
    if (handLandmarks.Count == 0) return;
    if (target != null)
    {
      if (target.Color != handColor) return;
      handLandmarks.Clear();
      Debug.Log("Clear " + hand.ToString() + "Landmark");
      return;
    }
  }

  private void DetectHand(PointAnnotation target, Body hand, Color handColor)
  {
    if (target.Color != handColor)
    {
      return;
    }

    List<GameObject> handLandmarks = GetLandmark(hand);
    if (handLandmarks != default && handLandmarks.Count != 0)
    {
      if (target.transform != handLandmarks[0].transform)
      {
        ClearHands(target);
        Debug.Log("Hand switch");
      }
    }
    else
    {
      AddLandmarks(handLandmarks, target.transform.parent, 21);
      Debug.Log(hand.ToString() + "Landmark Load Complete");
    }
  }

  private void AddLandmarks(List<GameObject> targetList,Transform target,int size)
  {
    for(int i = 0; i < size; i++)
    {
      Transform transform = target.GetChild(i);
      targetList.Add(transform.gameObject);
    }
  }

  public List<GameObject> GetLandmark(Body body)
  {
    if (!landmarkDictionary.ContainsKey(body))
    {
      Debug.LogWarning("Landmark with body part:" + body.ToString() + "doesn't exist");
      return default;
    }
    return landmarkDictionary[body];
  }

  public bool IsLandmarkComplete(Body body)
  {
    List<GameObject> landmarks = GetLandmark(body);
    return landmarks != default && landmarks.Count != 0;
  }

  private void SwapDictionary(Body bodyA, Body bodyB)
  {
    List<GameObject> LandmarksA = GetLandmark(bodyA);
    List<GameObject> LandmarksB = GetLandmark(bodyB);
    _ = landmarkDictionary.Remove(bodyA);
    _ = landmarkDictionary.Remove(bodyB);
    landmarkDictionary.Add(bodyA, LandmarksB);
    landmarkDictionary.Add(bodyB, LandmarksA);
    
  }
}
