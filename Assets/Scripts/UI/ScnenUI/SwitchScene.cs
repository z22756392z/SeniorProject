using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    public Animator MainCanvas;

    public void TriggerAnimation()
    {
        MainCanvas.SetTrigger("GoNextStage");
        Debug.Log("Onclick");
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
    public void GoToExercise()
    {
        SceneManager.LoadScene("000");
    }
    public void GoToTeachMode()
    {
        SceneManager.LoadScene("001");
    }
    public void GoToAR()
    {
        SceneManager.LoadScene("002");
    }
    public void GoToAI()
    {
        SceneManager.LoadScene("003");
    }
    public void GoToStory()
    {
        SceneManager.LoadScene("004");
    }
    public void GoToMain()
    {
        SceneManager.LoadScene("Main");
    }

}
