using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 로그인 화면. 메인/로그인/회원가입 패널을 바꿔 보여 주고, 입력을 확인해 PlayerPrefsAccountRepository에 부탁한다.
public class UI_Login : MonoBehaviour
{
    // 아이디: 이메일 형식, 또는 영문/숫자/-/@/_ 로 된 3~20자
    private static readonly Regex IdRegex = new Regex(
        @"^(?:[A-Za-z0-9._%+\-]+@[A-Za-z0-9\-]+(?:\.[A-Za-z0-9\-]+)+|[A-Za-z0-9\-@_]{3,20})$");

    // 비밀번호: 대문자/소문자/숫자/기호를 모두 포함한 8~30자 (공백과 한글 등은 불가, ASCII 문자만)
    private static readonly Regex PasswordRegex = new Regex(
        @"^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*[^A-Za-z0-9])[!-~]{8,30}$");

    private enum Panel
    {
        Main,      // 로그인 / 회원가입 / 종료 선택
        Login,     // 로그인 입력
        SignUp,    // 회원가입 입력
    }

    [Header("패널")]
    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _loginPanel;
    [SerializeField] private GameObject _signUpPanel;

    [Header("메인 패널 버튼")]
    [SerializeField] private Button _openLoginButton;
    [SerializeField] private Button _openSignUpButton;
    [SerializeField] private Button _quitButton;

    [Header("로그인 패널")]
    [SerializeField] private TMP_InputField _loginIdInput;
    [SerializeField] private TMP_InputField _loginPasswordInput;
    [SerializeField] private Button _loginSubmitButton;
    [SerializeField] private Button _loginBackButton;

    [Header("회원가입 패널")]
    [SerializeField] private TMP_InputField _signUpIdInput;
    [SerializeField] private TMP_InputField _signUpPasswordInput;
    [SerializeField] private TMP_InputField _signUpPasswordConfirmInput;
    [SerializeField] private Button _signUpSubmitButton;
    [SerializeField] private Button _signUpBackButton;

    [Header("기타")]
    [SerializeField] private TextMeshProUGUI _messageText;         // 안내 글자
    [SerializeField] private string _gameSceneName = "GameScene";  // 로그인하면 불러올 씬

    private void Start()
    {
        _openLoginButton.onClick.AddListener(() => Show(Panel.Login));
        _openSignUpButton.onClick.AddListener(() => Show(Panel.SignUp));
        _quitButton.onClick.AddListener(Quit);

        _loginSubmitButton.onClick.AddListener(Login);
        _loginBackButton.onClick.AddListener(() => Show(Panel.Main));

        _signUpSubmitButton.onClick.AddListener(SignUp);
        _signUpBackButton.onClick.AddListener(() => Show(Panel.Main));

        Show(Panel.Main);
    }

    // 해당 패널만 보여 주고, 입력 칸과 안내 글자를 비운다.
    private void Show(Panel panel)
    {
        _mainPanel.SetActive(panel == Panel.Main);
        _loginPanel.SetActive(panel == Panel.Login);
        _signUpPanel.SetActive(panel == Panel.SignUp);

        _loginIdInput.text = "";
        _loginPasswordInput.text = "";
        _signUpIdInput.text = "";
        _signUpPasswordInput.text = "";
        _signUpPasswordConfirmInput.text = "";
        _messageText.text = "";
    }

    // 로그인: 입력을 확인하고, 맞으면 게임 씬을 불러온다.
    private void Login()
    {
        string id = _loginIdInput.text;
        string password = _loginPasswordInput.text;

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password))
        {
            _messageText.text = "아이디와 비밀번호를 입력해 주세요.";
            return;
        }

        if (!PlayerPrefsAccountRepository.CheckPassword(id, password))
        {
            // 아이디가 없는지 비밀번호가 틀렸는지는 알려 주지 않는다.
            _messageText.text = "아이디 또는 비밀번호가 맞지 않습니다.";
            return;
        }

        SceneManager.LoadScene(_gameSceneName);
    }

    // 회원가입: 입력을 확인하고, 문제가 없으면 계정을 저장한 뒤 로그인 패널로 간다.
    private void SignUp()
    {
        string id = _signUpIdInput.text;
        string password = _signUpPasswordInput.text;
        string passwordConfirm = _signUpPasswordConfirmInput.text;

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password))
        {
            _messageText.text = "아이디와 비밀번호를 입력해 주세요.";
            return;
        }

        if (!IsValidId(id))
        {
            _messageText.text = "아이디는 이메일 또는 영문, 숫자, -, @, _ 를 쓴 3~20자여야 합니다.";
            return;
        }

        if (!IsValidPassword(password))
        {
            _messageText.text = "비밀번호는 대문자, 소문자, 숫자, 기호를 모두 포함한 8~30자여야 합니다.";
            return;
        }

        if (password != passwordConfirm)
        {
            _messageText.text = "비밀번호 확인이 맞지 않습니다.";
            return;
        }

        if (PlayerPrefsAccountRepository.Exists(id))
        {
            _messageText.text = "이미 있는 아이디입니다.";
            return;
        }

        PlayerPrefsAccountRepository.Register(id, password);
        Show(Panel.Login);
        _messageText.text = "가입했습니다. 로그인해 주세요.";
    }

    private static bool IsValidId(string id)
    {
        return IdRegex.IsMatch(id);
    }

    private static bool IsValidPassword(string password)
    {
        return PasswordRegex.IsMatch(password);
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
