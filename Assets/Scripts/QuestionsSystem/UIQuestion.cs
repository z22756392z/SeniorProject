using TMPro;
using UnityEngine;

public class UIQuestion : MonoBehaviour
{
    [SerializeField] private GameObject _questionButtons;
    [SerializeField] private GameObject _questionCount;
    [SerializeField] private TextMeshProUGUI _totalQuestionCount = default;
    [SerializeField] private TextMeshProUGUI _curQuestionCount = default;

    [SerializeField] private IntEventChannelSO _onQestionGroupSelected = default;

    [Header("Listening to")]
    [SerializeField] private IntEventChannelSO _setTotalQestionCount = default;
    [SerializeField] private IntEventChannelSO _setCurrentQestionCount = default;
    [SerializeField] private VoidEventChannelSO _showQuestionButtons = default;
    [SerializeField] private VoidEventChannelSO _onQuestionFinish = default;
    [SerializeField] private VoidEventChannelSO _leaveScene = default;
   
    private void OnEnable()
    {
        _questionButtons.SetActive(false);
        _questionCount.SetActive(false);
        _showQuestionButtons.OnEventRaised += ShowQuestoinButtons;
        _setTotalQestionCount.OnEventRaised += SetTotalQuestionCount;
        _setCurrentQestionCount.OnEventRaised += SetCurrentQuestionCount;
        _onQuestionFinish.OnEventRaised += Close;
        _leaveScene.OnEventRaised += Leave;
    }

    private void OnDisable()
    {
        _showQuestionButtons.OnEventRaised -= ShowQuestoinButtons;
        _setTotalQestionCount.OnEventRaised -= SetTotalQuestionCount;
        _setCurrentQestionCount.OnEventRaised -= SetCurrentQuestionCount;
        _onQuestionFinish.OnEventRaised -= Close;
        _leaveScene.OnEventRaised -= Leave;
    }

    public void OnDefaultQuestionClicked()
    {
        _questionButtons.SetActive(false);
        _onQestionGroupSelected.RaiseEvent(0);
        _questionCount.SetActive(true);
    }

    public void OnDefaultQuestion2Clicked()
    {
        _questionButtons.SetActive(false);
        _onQestionGroupSelected.RaiseEvent(1);
        _questionCount.SetActive(true);
    }


    void SetTotalQuestionCount(int value)
    {
        _totalQuestionCount.text = value.ToString();
    }

    void SetCurrentQuestionCount(int value)
    {
        _curQuestionCount.text = value.ToString();
    }

    void ShowQuestoinButtons()
    {
        _questionButtons.SetActive(true);
        _questionCount.SetActive(false);
    }

    void Close()
    {
        _questionButtons.SetActive(false);
        _questionCount.SetActive(false);
        ShowQuestoinButtons();
    }

    void Leave()
    {
        _questionButtons.SetActive(false);
        _questionCount.SetActive(false);
    }
}
