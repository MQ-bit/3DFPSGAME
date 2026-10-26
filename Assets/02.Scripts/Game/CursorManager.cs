using UnityEngine;

// 시작할 때 커서를 잠그고, Esc 키로 잠금과 해제를 전환한다.
public class CursorManager : MonoBehaviour
{
    // 에디터가 바꾸는 실제 상태와 게임에서 원하는 잠금 상태를 구분한다.
    private bool _isLocked;

    private void Awake()
    {
        LockCursor();
    }

    private void Update()
    {
        // 게임 중이 아니면 커서를 건드리지 않는다.
        if (GameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        // 왼쪽 Alt를 누르고 있는 동안만 커서를 보여 준다.
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            UnlockCursor();
        }
        else if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            LockCursor();
        }
    }

    public void LockCursor()
    {
        _isLocked = true;
        Cursor.lockState = CursorLockMode.Locked; // 화면 가운데에 고정
        Cursor.visible = false;                   // 커서 숨기기
    }

    public void UnlockCursor()
    {
        _isLocked = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
