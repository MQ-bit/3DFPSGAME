using System.Collections;
using UnityEngine;

// E 키를 누르면 소총을 집어넣고 검을 꺼내 휘두른다.
public class PlayerSwordSlash : MonoBehaviour
{
    [SerializeField] private GameObject _rifle;       // 평소에 들고 있는 소총
    [SerializeField] private GameObject _sword;       // 휘두르는 동안만 보이는 검
    [SerializeField] private float _slashTime = 0.6f; // 휘두르는 시간 (초)

    private Animator _animator;
    private bool _isSlashing;

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _sword.SetActive(false);
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;
        if (_isSlashing || Cursor.visible) return;

        if (Input.GetKeyDown(KeyCode.E))
            StartCoroutine(Slash_Coroutine());
    }

    private IEnumerator Slash_Coroutine()
    {
        _isSlashing = true;
        _rifle.SetActive(false);
        _sword.SetActive(true);
        _animator.SetTrigger("Slash");

        yield return new WaitForSeconds(_slashTime);

        _sword.SetActive(false);
        _rifle.SetActive(true);
        _isSlashing = false;
    }
}
