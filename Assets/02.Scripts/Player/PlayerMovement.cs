using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("플레이어 사운드")]
    [SerializeField] private PlayerSound playerSound;

    [Header("발걸음 설정")]
    [SerializeField] private float walkFootstepInterval = 0.5f;
    [SerializeField] private float sprintFootstepInterval = 0.35f;

    private float footstepTimer = 0f;
    private bool wasGrounded = false;

    [Header ("이동속도")]
    public float walkSpeed = 4f;  // 걷기 속도
    public float sprintSpeed = 6f;  // 달리기 속도
    public float rotSpeed = 10f;  // 회전 속도

    public Transform cameraTarget; // 플레이어 캐릭터 하위 임의의 카메라 타겟
    [Header ("카메라")]
    public float sensitivity = 1f;  // 회전 감도
    public float minY = -40f;  // 카메라 y 각도 최솟값 제한
    public float maxY = 70f;  //카메라 y 각도 최대값 제한

    [Header ("이동불가구역 검사")]
    public LayerMask obstacleLayer;   // OBSTACLE 레이어
    public float obstacleCheckDistance = 0.5f; // 앞 체크 거리
    public float obstacleCheckHeight = 0.4f;   // 체크 높이 (발보다 조금 위)
    public float obstacleCheckRadius = 0.3f;   // SphereCast 반지름
    public float blockSlopeAngle = 45f;        // 이 각도 이상이면 못 올라감

    [Header("점프")]
    public float jumpForce = 5f;             // 점프 힘
    public LayerMask groundLayer;            // 바닥 레이어
    public Transform groundCheck;            //발밑 체크 위치
    public float groundCheckRadius = 0.25f;  // 바닥 체크 반지름
    
    private bool isGrounded;                 // 바닥 위에 있는지 여부

    private bool isMoveLocked = false;       // 대화 중 이동 잠금 여부
    public bool IsMoveLocked => isMoveLocked;
    private bool isInputLocked = false;      // 입력 잠금 여부
    float yaw;  // 좌우 회전값
    float pitch;  // 상하 회전값
    private Rigidbody rb;
    private PlayerInput input;
    private Camera cam;
    private CapsuleCollider capsuleCollider;
    private Animator animator;
    private readonly int hashSpeed = Animator.StringToHash("Speed");
    private readonly int hashIsGrounded = Animator.StringToHash("IsGrounded");
    private readonly int hashJump = Animator.StringToHash("Jump");
    private readonly int hashCanMove = Animator.StringToHash("CanMove");
    private readonly int hashIsSliding = Animator.StringToHash("IsSliding");
    private IceSlideRigidbody iceSlide;  // 슬라이딩 참조
    const string mouseKey = "Sensitivity";  // 옵션 팝업에서 마우스 감도를 저장하는 키

    // 바닥 검사 결과를 담아둘 버퍼
    private Collider[] groundHits = new Collider[10];
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<PlayerInput>();
        cam = Camera.main;
        yaw = transform.eulerAngles.y;
        capsuleCollider = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
        iceSlide = GetComponent<IceSlideRigidbody>();

        if (playerSound == null)
            playerSound = GetComponent<PlayerSound>();
    }
    private void Start()
    {
        CheckGround();
        wasGrounded = isGrounded;
    }
    public void SetInputLock(bool value)
    {
        isInputLocked = value;

        // Rigidbody 멈추고 싶으면 같이 처리
        if (value)
        {
            rb.velocity = Vector3.zero;
        }
    }
    // 외부(ChatNPCManager)에서 호출해서 이동 잠금/해제를 제어
    public void SetMoveLock(bool isLocked)
    {
        isMoveLocked = isLocked;

        if (input != null)
        {
            input.lookInput = Vector2.zero;
        }

        if (isLocked)
        {
            // 수평 이동 즉시 정지
            Vector3 velocity = rb.velocity;
            velocity.x = 0f;
            velocity.z = 0f;
            rb.velocity = velocity;
            input.ResetInputState();  // 입력값도 즉시 비움
            // 애니메이션도 Idle로 고정
            if (animator != null)
            {
                animator.SetBool(hashCanMove, false);
                animator.SetFloat(hashSpeed, 0f);
            }
            footstepTimer = 0f;
        }
    }
    private void Update()
    {
              // 대화 중에는 카메라 회전/애니메이션 갱신을 막음
        if (isMoveLocked || isInputLocked)
        {
            return;
        }
        RotateCamera();  // 마우스 회전 처리
        UpdateAnimation();   // 애니메이션 파라미터 갱신
    }
    private void FixedUpdate()
    {
        if (isMoveLocked)
        {
            footstepTimer = 0f;
            return;
        }

        CheckGround();
        HandleLandingSound();
        Move();
        Jump();
        HandleFootstep();

        wasGrounded = isGrounded;
    }

    void Move()
    {
        Vector3 camForward = cam.transform.forward; // 카메라의 앞 방향
        Vector3 camRight = cam.transform.right;     // 카메라의 오른쪽 방향

        // 수평 이동만 할 거라 y값 제거
        camForward.y = 0f;
        camRight.y = 0f;

        // 방향 벡터 길이 1로 정규화
        camForward.Normalize();
        camRight.Normalize();

        // 입력값을 카메라 기준 월드 이동 방향으로 변환
        Vector3 moveDir = camForward * input.moveInput.y + camRight * input.moveInput.x;
        

        if (moveDir.sqrMagnitude > 0.01f) // 입력이 있을 때만 이동/회전
        {
            moveDir.Normalize(); // 대각선 이동이 더 빨라지지 않도록 보정

            // 플레이어 앞쪽 검사 시작 위치
            Vector3 origin = transform.position + Vector3.up * obstacleCheckHeight;

            bool hitObstacle = Physics.SphereCast(
            origin,                 // 시작 위치
            obstacleCheckRadius,    // 구 반지름
            moveDir,                // 진행 방향
            out RaycastHit hit,     // 맞은 정보 저장
            obstacleCheckDistance,  // 검사 거리
            obstacleLayer           // 검사할 레이어
            );

            // 장애물에 부딪혔을 때
            if (hitObstacle)
            {
                // 맞은 면의 법선과 위쪽 방향 사이 각도를 계산
                // 평지에 가까울수록 0도, 벽에 가까울수록 90도
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

                // 디버그용: 맞은 위치의 법선 표시
                Debug.DrawRay(hit.point, hit.normal, Color.yellow);

                // 경사각이 너무 크면 이동 금지
                if (slopeAngle >= blockSlopeAngle)
                {
                    // 빨간색: 이동 막힘
                    Debug.DrawRay(origin, moveDir * obstacleCheckDistance, Color.red);
                    return;
                }
            }
            Debug.DrawRay(origin, moveDir * obstacleCheckDistance, Color.green);

            float currentSpeed = input.isSprint ? sprintSpeed : walkSpeed;  // Shift를 누르면 sprintSpeed, 아니면 walkSpeed 사용

            Vector3 nextPos = rb.position + moveDir * currentSpeed * Time.fixedDeltaTime;
            rb.MovePosition(nextPos); // Rigidbody를 이용해 이동
        }
    }

    void HandleFootstep()
    {
        // 바닥 위가 아니면 발걸음 리셋
        if (!isGrounded)
        {
            footstepTimer = 0f;
            return;
        }

        // 슬라이딩 중이면 발걸음 안 냄
        if (iceSlide != null && iceSlide.enabled && iceSlide.IsSliding())
        {
            footstepTimer = 0f;
            return;
        }

        // 이동 입력이 없으면 발걸음 안 냄
        bool hasMoveInput = input.moveInput.sqrMagnitude > 0.01f;
        if (!hasMoveInput)
        {
            footstepTimer = 0f;
            return;
        }

        float currentInterval = input.isSprint ? sprintFootstepInterval : walkFootstepInterval;

        footstepTimer += Time.fixedDeltaTime;

        if (footstepTimer >= currentInterval)
        {
            footstepTimer = 0f;
            playerSound?.PlayFootstep();
        }
    }

    void RotateCamera()
    {
        if (GameManager.Instance != null && GameManager.Instance.lookLock) return;  //시선 고정 상태에서는 동작 없음

        Vector2 look = input.lookInput;  // 마우스 입력

        // 좌우 회전 (Player 기준)
        yaw += look.x * sensitivity * PlayerPrefs.GetFloat(mouseKey);

        // 상하 회전
        pitch -= look.y * sensitivity * PlayerPrefs.GetFloat(mouseKey);
        pitch = Mathf.Clamp(pitch, minY, maxY);

        // Player 회전
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // CameraTarget 회전 (상하만)
        cameraTarget.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void CheckGround()
    {
        // groundCheck가 비어 있으면 안전하게 종료
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        // groundCheck 위치 기준으로 주변 콜라이더를 모두 찾음
        int hitCount = Physics.OverlapSphereNonAlloc(
            groundCheck.position,
            groundCheckRadius,
            groundHits,
            ~0, // 모든 레이어 검사
            QueryTriggerInteraction.Ignore
        );

        isGrounded = false;

        for (int i = 0; i < hitCount; i++)
        {
            Collider col = groundHits[i];

            // 자기 자신 콜라이더는 무시
            if (col.transform == transform || col.transform.IsChildOf(transform))
                continue;

            // 맞은 오브젝트의 레이어가 groundLayer에 포함되는지 직접 검사
            int hitLayer = col.gameObject.layer;
            bool isGroundLayer = (groundLayer.value & (1 << hitLayer)) != 0;

            if (isGroundLayer)
            {
                isGrounded = true;
                break;
            }
        }

        // 디버그용 선 표시
        Debug.DrawRay(
            groundCheck.position,
            Vector3.up * 0.2f,
            isGrounded ? Color.green : Color.red
        );
    }

    void Jump()
    {
        if (!input.jumpTriggered)
            return;

        input.jumpTriggered = false;

        if (!isGrounded)
        {
            Debug.Log("바닥 아님");
            return;
        }

        Vector3 velocity = rb.velocity;
        velocity.y = 0f;
        rb.velocity = velocity;

        playerSound?.PlayJump();   // 점프 성공 사운드

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        animator.SetTrigger(hashJump);
    }

    void HandleLandingSound()
    {
        // 이전 프레임엔 공중, 현재는 바닥이면 착지
        if (!wasGrounded && isGrounded)
        {
            playerSound?.PlayLand();
        }
    }

    void UpdateAnimation()
    {
        if (animator == null)
            return;

        // 슬라이딩 스크립트가 활성화된 동안에는
        // PlayerMovement 쪽에서 Movement 상태를 절대 건드리지 않음
        if (iceSlide != null && iceSlide.enabled)
        {
            // 실제 슬라이딩 중이 아닐 때는 Idle 유지
            animator.SetBool(hashCanMove, false);
            animator.SetBool(hashIsGrounded, true);

            // 아직 미끄러지기 전이라면 Speed도 0으로 유지
            if (!iceSlide.IsSliding())
            {
                animator.SetFloat(hashSpeed, 0f);
            }

            return;
        }

        // 이동 입력이 있는지 확인
        bool hasMoveInput = input.moveInput.sqrMagnitude > 0.01f;

        // 이동 입력이 있으면 Movement, 없으면 Idle
        animator.SetBool(hashCanMove, hasMoveInput);

        // 바닥 여부 전달
        animator.SetBool(hashIsGrounded, isGrounded);

        // Movement 상태 안에서만 사용할 Speed 값 계산
        float targetSpeed = 0f;

        if (hasMoveInput)
        {
            // 뒤로 이동
            if (input.moveInput.y < -0.1f)
            {
                targetSpeed = -1f;
            }
            // 달리기
            else if (input.isSprint)
            {
                targetSpeed = 1f;
            }
            // 걷기
            else
            {
                targetSpeed = 0.5f;
            }
        }

        // 부드럽게 보간
        float currentSpeed = animator.GetFloat(hashSpeed);
        float smoothSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 12f);

        animator.SetFloat(hashSpeed, smoothSpeed);
    }

    public void EnterSlideZoneAnimationMode()
    {
        if (animator == null)
            return;

        // 슬라이드 맵 진입 순간에는 Movement로 넘어가지 않게 막음
        animator.SetBool(hashCanMove, false);

        // 아직 실제 미끄러지는 중은 아니므로 Sliding도 false
        animator.SetBool(hashIsSliding, false);

        // Blend Tree 값도 0으로 초기화해서 Idle 상태로 맞춤
        animator.SetFloat(hashSpeed, 0f);

        // 바닥 위 상태로 유지
        animator.SetBool(hashIsGrounded, true);

        // 혹시 남아 있던 점프 상태도 정리
        animator.ResetTrigger(hashJump);
    }

    public void ExitSlideZoneAnimationMode()
    {
        if (animator == null)
            return;

        // 슬라이딩 종료 순간에는 Sliding 애니메이션을 확실히 끔
        animator.SetBool(hashIsSliding, false);

        // 바로 Movement로 가지 않고 일단 Idle로 복귀
        animator.SetBool(hashCanMove, false);

        // Blend Tree 값 초기화
        animator.SetFloat(hashSpeed, 0f);

        // 바닥 위 상태로 유지
        animator.SetBool(hashIsGrounded, true);

        // 혹시 남아 있던 점프 트리거 제거
        animator.ResetTrigger(hashJump);
    }

    // 포탈에서 텔레포트 후 포탈 맞은편을 바라보게 하기 위해
    public void SetLookRotation(Quaternion worldRotation)
    {
        Vector3 euler = worldRotation.eulerAngles;

        // 플레이어의 좌우 회전값 갱신
        yaw = euler.y;

        // pitch는 유지해서 카메라 상하 각도가 갑자기 바뀌지 않게 함
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        cameraTarget.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
