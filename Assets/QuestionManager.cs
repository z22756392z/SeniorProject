using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestionManager : MonoBehaviour
{
    [SerializeField] private DialogueDataSO _defaultDialogue;
	[SerializeField] private DialogueDataSO _winDialogue;
	[SerializeField] private DialogueDataSO _loseDialogue;

	[SerializeField] private DialogueDataChannelSO _startDialogueEvent = default;

    [Header("Listening to channels")]
    [SerializeField] private VoidEventChannelSO _winDialogueEvent = default;
    [SerializeField] private VoidEventChannelSO _loseDialogueEvent = default;
    [SerializeField] private IntEventChannelSO _endDialogueEvent = default;

    private DialogueDataSO _currentDialogue;
    public void PlayDefaultQuest()
    {
        if (_defaultDialogue != null)
        {
            _currentDialogue = _defaultDialogue;
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        _startDialogueEvent.RaiseEvent(_currentDialogue);
        _endDialogueEvent.OnEventRaised += EndDialogue;
        _winDialogueEvent.OnEventRaised += PlayWinDialogue;
        _loseDialogueEvent.OnEventRaised += PlayLoseDialogue;
    }

	void EndDialogue(int dialogueType)
	{
		_endDialogueEvent.OnEventRaised -= EndDialogue;
		_winDialogueEvent.OnEventRaised -= PlayWinDialogue;
		_loseDialogueEvent.OnEventRaised -= PlayLoseDialogue;
	}

	void PlayLoseDialogue()
	{
		if (_winDialogue != null)
		{
			_currentDialogue = _winDialogue;
			StartDialogue();

		}
	}

	void PlayWinDialogue()
	{
		if (_winDialogue != null)
		{
			_currentDialogue = _winDialogue;
			StartDialogue();
		}
	}
}
