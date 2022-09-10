using System.Collections.Generic;
using UnityEngine;
namespace Mediapipe.Unity.UI
{
    public class SolutionController : MonoBehaviour
    {
        [Header("Listening to")]
        [SerializeField] private VoidEventChannelSO onSceneReady = default;
        [SerializeField] private InputReader _inputReader;
        public Solution solution = default;

        private void Awake()
        {
            solution.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            onSceneReady.OnEventRaised += _Start;
            _inputReader.MenuPauseEvent += PauseSolution;
            _inputReader.MenuCloseEvent += UnpauseSolution;
        }

        private void OnDisable()
        {
            onSceneReady.OnEventRaised -= _Start;
            _inputReader.MenuPauseEvent -= PauseSolution;
            _inputReader.MenuCloseEvent -= UnpauseSolution;
        }

        private void _Start()
        {
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