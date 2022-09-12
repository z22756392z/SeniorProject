using UnityEngine;

public class SwitchSceneAnimation : MonoBehaviour
{
    public Animator MainCanvas;
    [SerializeField] private VoidEventChannelSO _introEnd = default;
    [SerializeField] private VoidEventChannelSO _closeDialogue = default;
    [SerializeField] private VoidEventChannelSO _leaveScene = default;
    [SerializeField] private VoidEventChannelSO _hideInspector = default;
    [SerializeField] private VoidEventChannelSO _showSolution = default;

    [Header("Listening on")]
    [SerializeField] private VoidEventChannelSO _leaveFromUIPause = default;
    [SerializeField] private VoidEventChannelSO _leaveFromCurrentScene = default;

    private void OnEnable()
    {
        if (_leaveFromUIPause != null)
            _leaveFromUIPause.OnEventRaised += TriggerAnimation;
        if (_leaveFromCurrentScene != null)
            _leaveFromCurrentScene.OnEventRaised += TriggerAnimation;
    }

    private void OnDisable()
    {
        if (_leaveFromUIPause != null)
            _leaveFromUIPause.OnEventRaised -= TriggerAnimation;
        if (_leaveFromCurrentScene != null)
            _leaveFromCurrentScene.OnEventRaised -= TriggerAnimation;
    }

    public void IntroEnd()
    {
        _introEnd?.RaiseEvent();
    }

    public void TriggerAnimation()
    {
        MainCanvas.SetTrigger("GoNextStage");
        _leaveScene?.RaiseEvent();
        _closeDialogue?.RaiseEvent();
    }

    public void TriggerAnimationLeave()
    {
        MainCanvas.SetTrigger("Leave");
    }
    public void CloseMirror()
    {
        MainCanvas.SetTrigger("CloseMirror");
        _hideInspector?.RaiseEvent();
        
    }
    public void MirrorOnclick()
    {
        MainCanvas.SetTrigger("MirrorOnclick");
        
    }

    public void ShowSolution()
    {
        _showSolution?.RaiseEvent();
    }
}
