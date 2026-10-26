# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 프로젝트 개요

Unity 6 (`6000.3.22f1`, URP) 3D FPS 학습용 프로젝트. 게임 코드는 전부 `Assets/02.Scripts/` 아래에 있고 씬은 `Assets/01.Scenes/GameScene.unity` 하나다. 코드 주석과 커밋 메시지는 한국어로 작성한다.

## 명령어

빌드/실행/테스트는 Unity 에디터에서 한다 (자동화된 테스트 없음). CLI에서 쓰는 것은 포맷 검사뿐이다.

```bash
dotnet tool restore            # Husky.Net 설치 (dotnet-tools.json)
dotnet husky install           # git hook 연결

# pre-commit 훅과 동일한 포맷/네이밍 검사
dotnet format 3DFPSGAME.sln --verify-no-changes --severity warn --include Assets/02.Scripts/
# 자동 수정은 --verify-no-changes 를 빼고 실행
```

`.csproj`/`.sln`은 Unity가 생성하므로 직접 수정하지 않는다 (Unity → Preferences → External Tools → Regenerate project files).

## 커밋 규칙 (`.husky/commit-msg`에서 강제)

- 제목: `type: 설명` — type은 `asset|design|build|chore|ci|docs|feat|fix|perf|refactor|revert|style|test`
- **본문 최소 1줄 필수** (빈 줄·주석 제외). 제목만 있는 커밋은 거부된다.
- `main`에 `Assets/02.Scripts/**/*.cs` 변경을 push하면 GitHub Actions(`gemini-commit-review.yml`)가 `Docs/CodeReviewGuide.md` 기준으로 커밋 리뷰 코멘트를 단다.

## 코드 스타일 (`.editorconfig`, `Assets/02.Scripts/`에만 적용)

- private 필드 `_camelCase`, private `const` / `static readonly`는 `PascalCase`
- `UI_`로 시작하는 파일의 클래스는 `UI_` + PascalCase (예: `UI_Minimap`)
- 중괄호는 새 줄(Allman). 한 줄짜리 조기 리턴은 중괄호 생략 허용
- 코루틴 메서드는 `Xxx_Coroutine` 접미사
- 조정 가능한 수치는 `[SerializeField] private` + `[Header("스탯")]`로 노출

## 아키텍처

- **게임 상태 게이트**: `GameManager`(싱글톤, `Game/GameManger.cs` — 파일명 오타 주의)가 `Ready → Playing → GameOver` 상태를 가진다. 입력/AI를 처리하는 모든 `Update()`는 맨 앞에서 `GameManager.Instance.State != GameState.Playing`이면 리턴한다. 새 게임플레이 컴포넌트도 이 패턴을 따른다.
- **대미지 시스템**: 대미지를 받는 모든 대상은 `IDamageable.TakeDamage(DamageInfo)`를 구현한다 (`Player`, `Zombie`, `Drum`, `Box`). 공격 측(`PlayerGunFire` 레이캐스트, `Bomb`/`Drum` 범위 폭발, `Zombie` 근접공격)은 구체 타입이 아니라 `GetComponent<IDamageable>()?.TakeDamage(...)`로 호출한다. 체력 판정·사망 처리는 대상 객체 내부에서 한다.
- **범위 폭발**은 `Physics.OverlapSphere` + `LayerMask.GetMask("Drum", "Zombie")`를 쓴다. 새로운 피격 대상을 폭발에 포함하려면 레이어 지정이 필요하다.
- **좀비 AI**: `Zombie`는 `ZombieState` enum + `switch` 기반 FSM. Hit/Death는 코루틴으로 처리하고 `TakeDamage`에서 `StopAllCoroutines()`로 이전 상태 코루틴을 끊는다.
- **플레이어**: 기능별 컴포넌트로 분리 (`PlayerMove` — CharacterController 이동/중력/2단 점프/스태미나, `PlayerRotate`, `PlayerGunFire`, `PlayerBombFire`, `Player` — 체력). UI(`UI_PlayerStats` 등)는 이 컴포넌트들을 Inspector 참조로 받아 값을 읽어 표시한다.
- **입력**은 레거시 `Input` API(`Input.GetButton("Fire1")`, `"Sprint"`, `"Jump"` 등)를 사용한다. Input System 패키지는 설치되어 있지만 게임 코드에서 쓰지 않는다.
- **로깅**: `GameLogger.Log`는 `[Conditional("ENABLE_LOG")]`라 해당 심볼이 없으면 호출 자체가 제거된다 (Warning/Error는 항상 출력).
- **오브젝트 풀**: `Assets/Modules/Submodules/Pools`에 LeanPool(서드파티, `Pools.asmdef`)이 들어 있으나 아직 게임 코드에서 사용하지 않는다 — 현재 이펙트/폭탄은 `Instantiate`/`Destroy` (`Bomb.cs`에 풀링 Todo 있음). 서드파티 폴더(`Modules`, `Workshop`, `BloodDecalsAndEffects`, `Minimap_Resource`, `TextMesh Pro`)는 수정하지 않는다.
