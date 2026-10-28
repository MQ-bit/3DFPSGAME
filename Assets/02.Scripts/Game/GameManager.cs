using System.Collections;
using TMPro;
using UnityEngine;

public enum GameState
{
    Ready,
    Playing,
    GameOver
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _stateText;
    [SerializeField] private Animator _playerAnimator; // 게임 오버 때 군인을 서 있는 동작으로 돌린다.

    public static GameManager Instance { get; private set; }

    public GameState State { get; private set; } = GameState.Ready;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(StartSequence_Coroutine());
    }

    private IEnumerator StartSequence_Coroutine()
    {
        _stateText.gameObject.SetActive(true);
        _stateText.text = "준비";
        yield return new WaitForSeconds(1.5f);
        _stateText.text = "시작!";
        yield return new WaitForSeconds(0.5f);
        _stateText.gameObject.SetActive(false);

        State = GameState.Playing;
    }

    public void GameOver()
    {
        State = GameState.GameOver;

        _stateText.gameObject.SetActive(true);
        _stateText.text = "게임오버";
        _stateText.color = Color.red;

        _playerAnimator.SetFloat("MoveMotion", 0f); // 달리던 중이었어도 서 있는 동작으로 돌린다.
    }
}