using System.Collections.Generic;
using UnityEngine;
namespace Mediapipe.Unity.UI
{
    public class SolutionController : MonoBehaviour
    {
        [Header("Listening to")]
        [SerializeField] private VoidEventChannelSO _onEventRaisedStartSolution = default;
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
            _onEventRaisedStartSolution.OnEventRaised += _Start;
            _inputReader.MenuPauseEvent += PauseSolution;
            _inputReader.MenuCloseEvent += UnpauseSolution;
        }

        private void OnDisable()
        {
            _onEventRaisedStartSolution.OnEventRaised -= _Start;
            _inputReader.MenuPauseEvent -= PauseSolution;
            _inputReader.MenuCloseEvent -= UnpauseSolution;
        }

        private void _Start()
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
    }
}