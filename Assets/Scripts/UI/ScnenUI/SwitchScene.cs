using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    public Animator MainCanvas;
    [SerializeField] private VoidEventChannelSO _introEnd = default;
    [SerializeField] private VoidEventChannelSO _closeDialogue = default;
    [SerializeField] private VoidEventChannelSO _leaveScene = default;
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

    public void TriggerAnimationExercise()
    {
        MainCanvas.SetTrigger("GoToExercise");
        Debug.Log("Onclick");
    }
    public void TriggerAnimationTeachMode()
    {
        MainCanvas.SetTrigger("GoToTeachMode");
        Debug.Log("Onclick");
    }
    public void TriggerAnimationAR()
    {
        MainCanvas.SetTrigger("GoToAR");
        Debug.Log("Onclick");
    }
    public void TriggerAnimationAI()
    {
        MainCanvas.SetTrigger("GoToAI");
        Debug.Log("Onclick");
    }
    public void TriggerAnimationStory()
    {
        MainCanvas.SetTrigger("GoToStory");
        Debug.Log("Onclick");
    }
    public void CloseMirror()
    {
        MainCanvas.SetTrigger("CloseMirror");
        Debug.Log("close");
    }
    public void MirrorOnclick()
    {
        MainCanvas.SetTrigger("MirrorOnclick");
        Debug.Log("Open");
    }
}
