using UnityEngine;

public class QuestionManager : MonoBehaviour
{
    [SerializeField] private IntEventChannelSO _setTotalQestionCount = default;
    [SerializeField] private IntEventChannelSO _setCurrentQestionCount = default;
    [SerializeField] private VoidEventChannelSO _onQuestionFinish = default;

    [Header("Listening to")]
    [SerializeField] private VoidEventChannelSO _onQuestionAnswered = default;
    [SerializeField] private IntEventChannelSO _onQestionGroupSelected = default;

    [SerializeField] private QuestionsSO _questionsSO = default;
    
    private int _currentQusetionCount;
    private int _totalQuestionCount;

    private void OnEnable()
    {
        _onQestionGroupSelected.OnEventRaised += SetQuestionGroup;
        _onQuestionAnswered.OnEventRaised += OnQuestionAnswered;
        
    }

    private void OnDisable()
    {
        _onQestionGroupSelected.OnEventRaised -= SetQuestionGroup;
        _onQuestionAnswered.OnEventRaised -= OnQuestionAnswered;
    }

    void SetQuestionGroup(int value)
    {
        _questionsSO.SetCurrentQuestoinGroup(value);
        _currentQusetionCount = 1;
        _totalQuestionCount = _questionsSO.CurrentQusetionGroupCount;
        _setTotalQestionCount.RaiseEvent(_totalQuestionCount);
        _setCurrentQestionCount.RaiseEvent(_currentQusetionCount);
        _questionsSO.PlayDefaultQuest();
    }

    void OnQuestionAnswered()
    {
        if (_currentQusetionCount + 1> _totalQuestionCount)
        {
            _onQuestionFinish.RaiseEvent();
            return;
        }
        _currentQusetionCount++;
        _setCurrentQestionCount.RaiseEvent(_currentQusetionCount);
        _questionsSO.NextQuestion();
    }
}
