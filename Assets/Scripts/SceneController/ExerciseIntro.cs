using System.Collections;
using UnityEngine;

public class ExerciseIntro : MonoBehaviour
{
    [SerializeField] private DialogueDataSO _introDialogue = default;
    [SerializeField] private VoidEventChannelSO _onSceneReady = default;
    [SerializeField] private VoidEventChannelSO _showQuestionButtons = default;
    [SerializeField] private DialogueDataChannelSO _startDialogueEvent = default;
    [SerializeField] private IntEventChannelSO _endDialogueEvent = default;
    private void OnEnable()
    {
        _onSceneReady.OnEventRaised += StartIntro;
    }

    private void OnDisable()
    {
        _onSceneReady.OnEventRaised -= StartIntro;
    }

    void StartIntro()
    {
        StartCoroutine(PlayIntroDialogue());
    }

    IEnumerator PlayIntroDialogue()
    {
        yield return new WaitForSeconds(1f); //waiting time for all scenes to be loaded 
        _startDialogueEvent.RaiseEvent(_introDialogue);
        _endDialogueEvent.OnEventRaised += EndDialogue;
    }
    void EndDialogue(int dialogueType)
    {
        _endDialogueEvent.OnEventRaised -= EndDialogue;
        _showQuestionButtons.RaiseEvent();
    }
}
