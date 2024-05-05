using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private Button _playPortalGun;
    [SerializeField] private Button _playOptimized;
    [SerializeField] private List<GameObject> _gunObj;
    [SerializeField] private List<GameObject> _optimizeObj;

    private void Start()
    {
        _playOptimized.onClick.AddListener(PlayOptimized);
        _playPortalGun.onClick.AddListener(PlayPortal);
        Observable.EveryUpdate().Subscribe(InputPlayer).AddTo(this);
    }

    private void InputPlayer(long obj)
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene(0);
    }

    private void PlayOptimized()
    {
        _player.SetType(PortalType.Worlds);
        _gunObj.ForEach(obj => obj.SetActive(false));
        gameObject.SetActive(false);
    }

    private void PlayPortal()
    {
        _player.SetType(PortalType.Gun);
        _optimizeObj.ForEach(obj => obj.SetActive(false));
        gameObject.SetActive(false);
    }
}
