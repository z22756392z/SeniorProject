using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelTransformTranslate : MonoBehaviour
{
  [System.Serializable]
  public struct Model
  {
    public Body body;
    public List<GameObject> modelPart;
  }

  [SerializeField] private List<Model> _models = new List<Model>();
  private Dictionary<Body, List<GameObject>> _modelDictionary;

  [SerializeField] private LandMarkExtracter _landMarkExtracter = default;
  [SerializeField] [Range(0f, 2f)] private float _scaleMultipler = 0.01f;

  private Vector3 _relativePos;

  private void Awake()
  {
    _modelDictionary = new Dictionary<Body, List<GameObject>>();

    foreach (Model _model in _models)
    {
      _modelDictionary.Add(_model.body, _model.modelPart);
    }
  }

  private void Update()
  {
    TranslateModel(Body.LeftHand);
    TranslateModel(Body.RightHand);
  }

  private void TranslateModel(Body body)
  {
    if (_landMarkExtracter.IsLandmarkComplete(body))
    {
      List<GameObject> landmarks = _landMarkExtracter.GetLandmark(body);
      List<GameObject> modelparts = GetModel(body);
      if (modelparts == default) return;
      for (int i = 1; i < landmarks.Count; i++)
      {
        if (modelparts[i] == default) continue;
        _relativePos = (landmarks[i].transform.position - landmarks[0].transform.position) * _scaleMultipler;
        modelparts[i].transform.position = modelparts[0].transform.position + _relativePos;
      }
    }
  }

  public List<GameObject> GetModel(Body body)
  {
    if (!_modelDictionary.ContainsKey(body))
    {
      Debug.LogWarning("Model with body part:" + body.ToString() + "doesn't exist");
      return default;
    }
    return _modelDictionary[body];
  }
}
