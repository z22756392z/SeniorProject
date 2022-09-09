using System.Collections;
using UnityEngine;

public class Teach : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO _onSceneReady = default;
    [SerializeField] private LoadEventChannelSO _loadEventChannelSO = default;
    [SerializeField] private MenuSO _menuMenu = default;

    [SerializeField] private GameObject _choices = default;
    [SerializeField] private VoidEventChannelSO _salute = default;
    [SerializeField] private VoidEventChannelSO _leave = default;

    [SerializeField] private DialogueDataChannelSO _startDialogueEvent = default;
    [SerializeField] private VoidEventChannelSO _closeUIDialogueEvent = default; // only used for teach scene

    [SerializeField] private IntEventChannelSO _endDialogueEvent = default;

    [SerializeField] private DialogueDataSO _introDialogue = default;
    [SerializeField] private DialogueDataSO _acupunturePointDialogue = default;
    [SerializeField] private DialogueDataSO _AIDialogue = default;
    [SerializeField] private DialogueDataSO _exceriseDialogue = default;
    [SerializeField] private DialogueDataSO _faceMeshDialogue = default;
    [SerializeField] private DialogueDataSO _holisticDialogue = default;
    [SerializeField] private DialogueDataSO _introEndDialogue = default;

    [SerializeField] private VoidEventChannelSO _winDialogueEvent = default;
    [SerializeField] private VoidEventChannelSO _loseDialogueEvent = default;

    private void OnEnable()
    {
        _onSceneReady.OnEventRaised += StartIntro;
    }

    private void OnDisable()
    {
        _onSceneReady.OnEventRaised -= StartIntro;
        if(_endDialogueEvent.OnEventRaised != null)
        {
            _endDialogueEvent.OnEventRaised -= EndDialogue;
        }
    }

    IEnumerator PlayIntroDialogue()
    {
        yield return new WaitForSeconds(1f); //waiting time for all scenes to be loaded 
        _startDialogueEvent.RaiseEvent(_introDialogue);
        _endDialogueEvent.OnEventRaised += EndDialogue;
    }

    void StartIntro()
    {
        StartCoroutine(PlayIntroDialogue());
    }

    public void ShowChoice()
    {
        _choices.SetActive(true);
    }

    void CloseChoice()
    {
        _choices.SetActive(false);
    }


    void EndDialogue(int dialogueType)
    {
        _endDialogueEvent.OnEventRaised -= EndDialogue;
        ShowChoice();
    }

    void RestartDialogue()
    {
        _winDialogueEvent.OnEventRaised -= RestartDialogue;
        _salute.RaiseEvent();
        _startDialogueEvent.RaiseEvent(_introDialogue);
        _endDialogueEvent.OnEventRaised += EndDialogue;
    }

    void LeaveTeachMode()
    {
        _loseDialogueEvent.OnEventRaised -= LeaveTeachMode;
        StartCoroutine( EndTeachMode());
    }

    IEnumerator EndTeachMode()
    {
        _leave.RaiseEvent();
        _closeUIDialogueEvent.RaiseEvent();
        yield return new WaitForSeconds(1f);
        _loadEventChannelSO.RaiseEvent(_menuMenu, false); //load main menu
    }

    public void OnAupuncturePointClicked()
    {
        CloseChoice();
        _startDialogueEvent.RaiseEvent(_acupunturePointDialogue);
        _endDialogueEvent.OnEventRaised += EndDialogue;
    }

    public void OnRasaClicked()
    {
        CloseChoice();
        _startDialogueEvent.RaiseEvent(_AIDialogue);
        _endDialogueEvent.OnEventRaised += EndDialogue;
    }

    public void OnExerciesClicked()
    {
        CloseChoice();
        _startDialogueEvent.RaiseEvent(_exceriseDialogue);
        _endDialogueEvent.OnEventRaised += EndDialogue;
    }

    public void OnFaceMeshClicked()
    {
        CloseChoice();
        _startDialogueEvent.RaiseEvent(_faceMeshDialogue);
        _endDialogueEvent.OnEventRaised += EndDialogue;
    }

    public void OnHolisticClicked()
    {
        CloseChoice();
        _startDialogueEvent.RaiseEvent(_holisticDialogue);
        _endDialogueEvent.OnEventRaised += EndDialogue;
    }

    public void OnIntroEndClicked()
    {
        CloseChoice();
        _startDialogueEvent.RaiseEvent(_introEndDialogue);
        _winDialogueEvent.OnEventRaised += RestartDialogue;
        _loseDialogueEvent.OnEventRaised += LeaveTeachMode;
    }
}
