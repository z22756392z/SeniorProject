using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogController : MonoBehaviour
{
    [SerializeField] private GameSceneSO _mainMenu = default;
    [SerializeField] private GameSceneStorageSO _previousScene = default;
    [SerializeField] private VoidEventChannelSO _onSceneReady = default;
    [SerializeField] private Animator _animator;
    private void Awake()
    {
        _animator.enabled = true;
    }
    private void OnEnable()
    {
        SetFogAnim(true);

    }

    void SetFogAnim(bool value)
    {
        _animator.SetBool("Fog", value);
        StartCoroutine(AnimEnable());
    }

    IEnumerator AnimEnable()
    {
        yield return new WaitForSeconds(0f);
        _animator.enabled = true;
    }
}
