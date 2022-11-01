using UnityEngine;

class FPS : MonoBehaviour
{
    [SerializeField] private InputReader _input;

    public int FpsTarget;
    public float UpdateInterval = 0.5f;

    private float _lastInterval;
    private int _frames = 0;
    private float _fps;
    private bool _isFPSShow = false;

    private void OnEnable()
    {
        _input.FPSEvent += FPSEvent;
        GUI.color = Color.black;
    }

    private void OnDisable()
    {
        _input.FPSEvent -= FPSEvent;
    }

    void Start()
    {
        Application.targetFrameRate = FpsTarget;
        _lastInterval = Time.realtimeSinceStartup;
        _frames = 0;
    }
     
    void Update()
    {
        if (!_isFPSShow) return;

        ++_frames;
        float timeNow = Time.realtimeSinceStartup;
        if (timeNow >= _lastInterval + UpdateInterval)
        {
            _fps = _frames / (timeNow - _lastInterval);
            _frames = 0;
            _lastInterval = timeNow;
        }
    }

    private void FPSEvent()
    {
        _isFPSShow = !_isFPSShow;

    }

    void OnGUI()
    {
        if (!_isFPSShow) return;
        GUIStyle labelFont = new GUIStyle();
        labelFont.normal.textColor = new Color(0, 0, 0);
        labelFont.fontSize = 30;
        labelFont.fixedHeight = 100;

        GUI.Label(new Rect(Screen.width / 10, 0, Screen.width / 20, Screen.height / 20), "FPS: " + _fps.ToString(), labelFont);

    }
}