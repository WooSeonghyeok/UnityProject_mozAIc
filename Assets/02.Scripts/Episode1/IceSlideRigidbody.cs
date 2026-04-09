using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class IceSlideRigidbody : MonoBehaviour
{
    [Header("이동 설정")]
    public float slideSpeed = 15f;              // 미끄러지는 속도
    public float stopDistance = 0.6f;           // 장애물 앞에서 멈출 검사 거리
    [Header("충돌 검사 설정")]
    public float castHeight = 0.5f;             // SphereCast를 쏘는 높이
    public float castRadius = 0.25f;            // SphereCast 반지름
    public LayerMask obstacleLayer;             // 기둥, 벽 등 장애물 레이어
    [Header("회전 설정")]
    public bool rotateToMoveDirection = true;   // 이동 시작할 때 바라보는 방향도 바꿀지
    private Rigidbody rb;
    private Animator animator;
    private readonly int hashIsSliding = Animator.StringToHash("IsSliding");
    private bool isSliding = false;                 // 현재 미끄러지는 중인지
    private Vector3 slideDirection = Vector3.zero;  // 현재 미끄러지는 방향
    private PlayerInput user;
    private readonly string playerTag = "Player";

    // 입력 단계 관리
    // 0 : 첫 입력 (W만 가능)
    // 1 : 두 번째 입력 (A,D만 가능)
    // 2 : 이후 입력 (모두 가능)
    private int inputPhase = 0;

    private void Awake()
    {
        user = GameObject.FindGameObjectWithTag(playerTag).GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        ResetInputPhase();
    }

    private void OnDisable()
    {
        // 스크립트가 꺼질 때 슬라이딩 상태를 확실히 해제
        isSliding = false;

        if (animator != null)
            animator.SetBool(hashIsSliding, false);

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        // 슬라이딩 중 꺼질 때 SoundManager 루프 SFX 정지
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopLoopSFX();
        }
    }

    private void Update()
    {
        // 이미 미끄러지는 중이면 입력을 받지 않음
        if (isSliding) return;
        if (inputPhase == 0)
        {
            HandleFirstInput();
        }
        else if (inputPhase == 1)
        {
            HandleSecondInput();
        }
        else
        {
            HandleNormalInput();
        }
    }
    private void FixedUpdate()
    {
        // 미끄러지는 중일 때만 이동 처리
        if (!isSliding) return;

        SlideMove();
    }
    void HandleFirstInput()  // 슬라이드존 진입시 바로 앞으로 슬라이딩
    {
        if (user.moveInput.x != 0 || user.moveInput.y < 0) return;
        StartSlideFromDirection(transform.forward);
        if (isSliding)
        {
            inputPhase = 1;
        }
    }
    void HandleSecondInput()  // 두 번째 입력 (A,D만 가능)
    {
        if (user.moveInput.y == 0 && user.moveInput.x != 0)
        {
            StartSlideFromDirection(transform.right * Mathf.Sign(user.moveInput.x));
        }
        if (isSliding)
        {
            inputPhase = 2;
        }
    }
    // 이후 입력 (모두 가능)
    void HandleNormalInput()
    {
        if (user.moveInput.y > 0 && user.moveInput.x == 0)
        {
            StartSlideFromDirection(transform.forward);
        }
        else if (user.moveInput.y < 0 && user.moveInput.x == 0)
        {
            StartSlideFromDirection(-transform.forward);
        }
        else if (user.moveInput.y == 0 && user.moveInput.x < 0)
        {
            StartSlideFromDirection(-transform.right);
        }
        else if (user.moveInput.y == 0 && user.moveInput.x > 0)
        {
            StartSlideFromDirection(transform.right);
        }
        else return;
    }
    /// 입력 또는 외부 지정 방향으로 슬라이딩 시작
    public void StartSlideFromDirection(Vector3 dir)
    {
        // 실제 이동은 퍼즐에 맞게 월드 4방향으로 보정
        slideDirection = SnapToCardinal(dir);

        // 방향 보정 후에도 방향이 이상하면 종료
        if (slideDirection == Vector3.zero)
            return;

        // 이동 시작할 때 캐릭터가 바라보는 방향도 맞추기
        if (rotateToMoveDirection)
        {
            transform.forward = slideDirection;
        }

        // 시작하자마자 바로 앞이 막혀 있으면 이동 시작 안 함
        if (IsBlockedAhead())
        {
            StopSliding();
            return;
        }

        // 미끄러지기 시작
        isSliding = true;

        // 미끄럼 애니메이션 시작
        if (animator != null)
            animator.SetBool(hashIsSliding, true);

        // SoundManager 루프 슬라이딩 사운드 재생
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayLoopSFX(SoundManager.SFXType.Ep1_1Slide);
        }
    }

    void SlideMove()
    {
        // 슬라이딩 중에는 항상 이동 방향을 바라보도록 유지
        if (rotateToMoveDirection && slideDirection != Vector3.zero)
        {
            transform.forward = slideDirection;
        }

        // 이동 전에 앞을 먼저 체크해서 장애물이 있으면 멈춤
        if (IsBlockedAhead())
        {
            StopSliding();
            return;
        }

        // Rigidbody를 사용해서 현재 방향으로 계속 이동
        Vector3 nextPos = rb.position + slideDirection * slideSpeed * Time.fixedDeltaTime;
        rb.MovePosition(nextPos);
    }

    bool IsBlockedAhead()
    {
        // 충돌 검사를 몸통 높이에서 하기 위해 약간 위에서 시작
        Vector3 origin = rb.position + Vector3.up * castHeight;

        bool hit = Physics.SphereCast(
            origin,
            castRadius,
            slideDirection,
            out RaycastHit hitInfo,
            stopDistance,
            obstacleLayer
        );

        // 디버그용 선 표시
        Debug.DrawRay(origin, slideDirection * stopDistance, hit ? Color.red : Color.green);

        return hit;
    }

    void StopSliding()
    {
        // 미끄럼 상태 해제
        isSliding = false;

        // 미끄럼 애니메이션 종료
        if (animator != null)
            animator.SetBool(hashIsSliding, false);

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 슬라이딩 루프 정지
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopLoopSFX();
        }

        // 정지 SFX 재생
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.SFXType.Ep1_1SlideHit);
        }
    }

    Vector3 SnapToCardinal(Vector3 dir)
    {
        // Y축 제거해서 평면 이동만 하도록 만듦
        dir.y = 0f;

        // 방향이 너무 작으면 0 반환
        if (dir.sqrMagnitude < 0.001f)
            return Vector3.zero;

        dir.Normalize();

        // X축 또는 Z축 중 하나로 강제 보정
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
        {
            return new Vector3(Mathf.Sign(dir.x), 0f, 0f);
        }
        else
        {
            return new Vector3(0f, 0f, Mathf.Sign(dir.z));
        }
    }

    // 슬라이드 퍼즐 시작 시 상태 초기화
    public void ResetInputPhase()
    {
        inputPhase = 0;
        isSliding = false;
        slideDirection = Vector3.zero;

        if (animator != null)
            animator.SetBool(hashIsSliding, false);

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        // 상태 초기화 시 루프 사운드도 정지
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopLoopSFX();
        }
    }

    /// SlideStartPoint에서 월드 기준 방향으로 첫 슬라이드 시작
    public void StartInitialSlideInWorldDirection(Vector3 worldDirection)
    {
        // SlideStartPoint가 지정한 월드 방향으로 시작
        StartSlideFromDirection(worldDirection);

        // 첫 슬라이드가 정상 시작되면 다음 입력 단계로 넘김
        if (isSliding)
        {
            inputPhase = 1;
        }
    }
    // 현재 미끄러지는 상태 확인
    public bool IsSliding()
    {
        return isSliding;
    }

    // 현재 미끄러지는 방향 확인
    public Vector3 GetSlideDirection()
    {
        return slideDirection;
    }
}