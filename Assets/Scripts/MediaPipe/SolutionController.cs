using System.Collections.Generic;
using UnityEngine;
namespace Mediapipe.Unity.UI
{
    public class SolutionController : MonoBehaviour
    {
        [Header("Listening to")]
        [SerializeField] private VoidEventChannelSO _onEventRaisedStartSolution = default;
        [SerializeField] private VoidEventChannelSO _showSolution = default;
        [SerializeField] private VoidEventChannelSO _hideSolution = default;
        [SerializeField] private InputReader _inputReader;
        public Solution solution = default;
        public GameObject annotation = default;

        private void Awake()
        {
            if (annotation != null) annotation.SetActive(false);
            solution.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if(_onEventRaisedStartSolution != null)
                _onEventRaisedStartSolution.OnEventRaised += StartSolution;
            if (_hideSolution != null)
                _hideSolution.OnEventRaised += HideSolution;
            _inputReader.MenuPauseEvent += PauseSolution;
            _inputReader.MenuCloseEvent += UnpauseSolution;
            if (_showSolution != null)
                _showSolution.OnEventRaised += ShowSolution;
        }

        private void OnDisable()
        {
            if (_onEventRaisedStartSolution != null)
                _onEventRaisedStartSolution.OnEventRaised -= StartSolution;
            if (_hideSolution != null)
                _hideSolution.OnEventRaised -= HideSolution;
            _inputReader.MenuPauseEvent -= PauseSolution;
            _inputReader.MenuCloseEvent -= UnpauseSolution;
            if (_showSolution != null)
                _showSolution.OnEventRaised -= ShowSolution;
        }

        public void StartSolution()
        {
            if (annotation != null) annotation.SetActive(true);
            solution.gameObject.SetActive(true);
        }

        void PauseSolution()
        {
            solution.Pause();
        }

        void UnpauseSolution()
        {
            solution.Resume();
        }

        public void HideSolution()
        {
            annotation.SetActive(false);
            solution.Pause();
        }

        public void ShowSolution()
        {
            annotation.SetActive(true);
            solution.Resume();
        }
    }
}