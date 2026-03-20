using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;      // 걷기 속도
    [SerializeField] private float runSpeed = 9f;       // 달리기 속도
    [SerializeField] private float mouseSensitivity = 2f;   // 마우스 감도

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpDelay = 0.15f;
    [SerializeField] private int maxJumpCount = 2;
    [SerializeField] private LayerMask groundLayer;

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaConsumeRun = 20f;  // 초당 달리기 소모량
    [SerializeField] private float staminaConsumeJump = 15f; // 점프 1회 소모량
    [SerializeField] private float staminaRegen = 10f;       // 초당 회복량
    [SerializeField] private float staminaRegenDelay = 1.5f; // 소모 후 회복 시작 대기시간

    [Header("Camera Settings")]
    [SerializeField] private Camera fpsCamera; 
    [SerializeField] private Camera tpsCamera;
    [SerializeField] private float tpsDistance = 4f;       
    [SerializeField] private float tpsHeightOffset = 1.5f; 
    [SerializeField] private float cameraSmoothing = 15f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private string playerBodyLayerName = "PlayerBody";

    [Header("Animation Settings")]
    [SerializeField] private Animator anim;
    [SerializeField] private float animSmoothTime = 0.1f;
    [Header("Ground Check Settings")]
    [SerializeField] private float rayDistance = 0.2f; // 발밑으로 쏠 레이의 길이
    [SerializeField] private Color rayColor = Color.red;
    [Header("Wepon Object")]
    [SerializeField] private GameObject weaponObject;
    [SerializeField] private Transform backPos;
    [SerializeField] private Transform handPos;
    [SerializeField] private Transform firePoint; 
    [SerializeField] private float fireRate = 0.2f; 

    
    private Player player;
    private Rigidbody rb;
    private int currentJumpCount = 0;
    private bool isGrounded;
    private float verticalRotation = 0f; 
    private bool isFirstPerson = true; 
    private bool isJumping = false;

    private Vector2 currentInputVector;
    private Vector2 smoothInputVelocity;
    private float currentMoveSpeed;
    private float currentStamina;
    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    private float lastStaminaUseTime;
    public bool isArmed = false;
    private float nextFireTime = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (anim == null) anim = GetComponentInChildren<Animator>();

        rb.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
        currentMoveSpeed = walkSpeed;
        currentStamina = maxStamina;
        InitializeView();
        AttachWeaponTo(backPos);
    }

    void Update()
    {
        HandleRotation();
        HandleJump();
        HandleViewToggle();
        HandleStaminaRegen();
        ToggleArmed();
        UpdateAnimation(); 
        CheckFire();
    }

    void FixedUpdate()
    {
        HandleMovement();
        CheckGround();
    }

    void LateUpdate()
    {
        if (!isFirstPerson) UpdateThirdPersonCameraOrbit();
    }

    private void UpdateAnimation()
    {
        if (anim == null) return;

        float targetX = Input.GetAxisRaw("Horizontal");
        float targetZ = Input.GetAxisRaw("Vertical");

        bool isRunningForward = Input.GetKey(KeyCode.LeftShift) && targetZ > 0 && currentStamina > 0;
        float multiplier = isRunningForward ? 2.0f : 1.0f;

        currentInputVector = Vector2.SmoothDamp(currentInputVector, new Vector2(targetX, targetZ) * multiplier, ref smoothInputVelocity, animSmoothTime);

        anim.SetFloat("InputX", currentInputVector.x);
        anim.SetFloat("InputZ", currentInputVector.y);
        anim.SetBool("IsGrounded", isGrounded);
        anim.SetBool("IsArmed", isArmed);
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(KeyCode.LeftShift) && v > 0 && currentStamina > 0)
        {
            currentMoveSpeed = runSpeed;
            ConsumeStamina(staminaConsumeRun * Time.deltaTime);
        }
        else
        {
            currentMoveSpeed = walkSpeed;
        }

        // 이동방향
        Vector3 moveDir = (transform.forward * v + transform.right * h).normalized;
        
        // 이동 속도 적용 (Y축은 중력 유지를 위해 rb 속도 그대로 사용)
        rb.linearVelocity = new Vector3(moveDir.x * currentMoveSpeed, rb.linearVelocity.y, moveDir.z * currentMoveSpeed);
    }

    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -40f, 60f);

        if (isFirstPerson)
        {
            fpsCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        }
    }

    private void HandleJump()
    {
        // 스페이스바 입력 시 + 점프 가능 횟수가 남았을 때 + 이미 점프 중이 아닐 때 + 스테미나가 충분할때
        if (Input.GetKeyDown(KeyCode.Space) && currentJumpCount < maxJumpCount && !isJumping && currentStamina >= staminaConsumeJump)
        {
            StartCoroutine(JumpRoutine());
        }
    }

    private System.Collections.IEnumerator JumpRoutine()
    {
        isJumping = true; // 점프 시작 상태 (중복 방지)
        ConsumeStamina(staminaConsumeJump);

        // 애니메이션 트리거 실행
        if (anim != null) anim.SetTrigger("DoJump");

        // 설정한 시간만큼 대기 (선딜레이)
        yield return new WaitForSeconds(jumpDelay);

        // 점프 실행
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        
        currentJumpCount++;
        isJumping = false; 
    }
    private void ConsumeStamina(float amount)
    {
        currentStamina -= amount;
        if (currentStamina < 0) currentStamina = 0;
        lastStaminaUseTime = Time.time; // 마지막 사용 시간 기록
    }
    private void HandleStaminaRegen()
    {
        // 마지막 사용 후 일정 시간이 지났고, 최대치가 아닐 때 회복
        if (Time.time - lastStaminaUseTime > staminaRegenDelay && currentStamina < maxStamina)
        {
            currentStamina += staminaRegen * Time.deltaTime;
            if (currentStamina > maxStamina) currentStamina = maxStamina;
        }
    }

    private void UpdateThirdPersonCameraOrbit()
    {
        Vector3 pivotPos = transform.position + Vector3.up * tpsHeightOffset;
        Quaternion orbitRotation = Quaternion.Euler(verticalRotation, transform.eulerAngles.y, 0);
        Vector3 targetPos = pivotPos + (orbitRotation * Vector3.back * tpsDistance);

        Vector3 dir = (targetPos - pivotPos).normalized;
        if (Physics.Raycast(pivotPos, dir, out RaycastHit hit, tpsDistance, obstacleLayer))
        {
            tpsCamera.transform.position = Vector3.Lerp(tpsCamera.transform.position, hit.point - dir * 0.2f, Time.deltaTime * cameraSmoothing);
        }
        else
        {
            tpsCamera.transform.position = Vector3.Lerp(tpsCamera.transform.position, targetPos, Time.deltaTime * cameraSmoothing);
        }
        tpsCamera.transform.LookAt(pivotPos);
    }

    private void InitializeView()
    {
        fpsCamera.gameObject.SetActive(isFirstPerson);
        tpsCamera.gameObject.SetActive(!isFirstPerson);
        
        int playerBodyLayer = LayerMask.NameToLayer(playerBodyLayerName);
        if (isFirstPerson) fpsCamera.cullingMask &= ~(1 << playerBodyLayer);
        else fpsCamera.cullingMask |= (1 << playerBodyLayer);
    }

    private void HandleViewToggle()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            isFirstPerson = !isFirstPerson;
            InitializeView();
        }
    }

    private void CheckGround()
    {
        Vector3 rayStartPos = transform.position + Vector3.up * 0.1f; 

        bool hit = Physics.Raycast(rayStartPos, Vector3.down, rayDistance, groundLayer);

        Debug.DrawRay(rayStartPos, Vector3.down * rayDistance, hit ? Color.green : rayColor);
        isGrounded = hit;
        if (isGrounded && rb.linearVelocity.y <= 0) currentJumpCount = 0;
    }

    private void ToggleArmed()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1)) {
            
            if(!isArmed)
            {
                if(weaponObject != null)
                {
                    AttachWeaponTo(handPos);
                    Debug.Log("무장 활성화");
                }
            }
            else
            {
                if(weaponObject != null)
                {
                    AttachWeaponTo(backPos);
                    Debug.Log("무장해제");
                }
            }
            isArmed = !isArmed;
        }
    }
    private void AttachWeaponTo(Transform target)
    {
        weaponObject.transform.SetParent(target);
        weaponObject.transform.localPosition = Vector3.zero;
        weaponObject.transform.localRotation = Quaternion.identity;
    }
    void CheckFire()
    {
        if (!isArmed) return;

        // 좌클릭 + 연사 딜레이 체크
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            if (BulletManager.Instance == null)
            {
                Debug.LogError("BulletPool이 씬에 없습니다!");
                return;
            }

            // 풀에서 총알 가져오기
            GameObject bullet = BulletManager.Instance.GetBullet();

            // 발사 위치와 방향 설정
            bullet.transform.position = firePoint.position;
            bullet.transform.rotation = firePoint.rotation;

            Debug.Log("총알 발사!");
            nextFireTime = Time.time + fireRate;
        }
    }
}