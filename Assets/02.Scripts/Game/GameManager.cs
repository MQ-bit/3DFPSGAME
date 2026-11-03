using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// 게임 전체의 상태
public enum GameState
{
    Ready,     // 준비: 시작 연출 중. 아무도 움직이지 않는다.
    Playing,   // 진행: 플레이 중
    Pause,     // 일시정지: 게임 시간과 입력이 멈춘다.
    GameOver   // 게임 오버: 모두 멈춘다.
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _stateText;
    [SerializeField] private CursorManager _cursorManager;  // 멈추면 커서를 풀고, 이어서 하면 다시 잠근다.
    [SerializeField] private GameObject _optionPopup;       // 일시정지할 때 보여 줄 옵션 팝업
    [SerializeField] private GameObject _gameOverPopup;     // 쓰러졌을 때 보여 줄 게임 오버 창

    private GameState _state = GameState.Ready;

    public static GameManager Instance { get; private set; }
    public GameState State => _state;

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

    private void Update()
    {
        // ESC를 누르면 게임 중에는 멈추고, 멈춰 있으면 이어서 한다.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_state == GameState.Playing)
            {
                Pause();
            }
            else if (_state == GameState.Pause)
            {
                Resume();
            }
        }
    }

    private IEnumerator StartSequence_Coroutine()
    {
        _stateText.gameObject.SetActive(true);
        _stateText.text = "준비";
        yield return new WaitForSeconds(1.5f);
        _stateText.text = "시작!";
        yield return new WaitForSeconds(0.5f);
        _stateText.gameObject.SetActive(false);

        _state = GameState.Playing;
    }

    // 일시정지: 게임 시간을 멈추고 일시정지 상태로 바꾼다. 게임 중일 때만 멈춘다.
    public void Pause()
    {
        if (_state != GameState.Playing)
        {
            return;
        }

        _state = GameState.Pause;
        Time.timeScale = 0f;  // 게임 시간이 흐르지 않는다.
        _optionPopup.SetActive(true);
        _cursorManager.UnlockCursor();
    }

    // 이어서 하기: 게임 시간을 다시 흐르게 하고 게임 중으로 돌아간다. 일시정지 중일 때만 한다.
    public void Resume()
    {
        if (_state != GameState.Pause)
        {
            return;
        }

        _state = GameState.Playing;
        Time.timeScale = 1f;  // 게임 시간이 다시 실제 시간과 같은 빠르기로 흐른다.
        _optionPopup.SetActive(false);
        _cursorManager.LockCursor();
    }

    // 다시 시작: 지금 씬을 처음부터 다시 불러온다.
    public void Restart()
    {
        Time.timeScale = 1f;  // timeScale은 씬을 다시 불러와도 그대로 남으므로 먼저 되돌린다.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 게임 종료: 빌드한 게임은 끝내고, 에디터에서는 플레이를 멈춘다.
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // 게임 오버: 모두 멈추고 게임 오버 창을 띄운다. 플레이어가 쓰러지면 Player가 부른다.
    public void GameOver()
    {
        _state = GameState.GameOver;
        Time.timeScale = 0f;  // 좀비의 걸음과 애니메이션까지 그 자리에서 멈춘다.
        _gameOverPopup.SetActive(true);
        _cursorManager.UnlockCursor();
    }
}
