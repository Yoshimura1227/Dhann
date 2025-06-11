using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("移動設定")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float jumpForce = 8f;
    public float rotationSpeed = 10f;
    
    [Header("アニメーション設定")]
    public float animationSmoothTime = 0.1f;
    
    [Header("グラウンドチェック")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer = 1;
    
    [Header("カメラ")]
    public Transform cameraTransform;
    
    private Rigidbody rb;
    private Animator animator;
    private bool isGrounded;
    private float currentSpeed;
    private Vector3 moveDirection;
    private bool isRunning;
    
    // アニメーターパラメーター名
    private readonly string SPEED_PARAM = "Speed";
    private readonly string JUMP_PARAM = "Jump";
    private readonly string GROUNDED_PARAM = "Grounded";
    private readonly string RUN_PARAM = "Run";
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        
        // カメラが設定されていない場合は自動で取得
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        // グラウンドチェック用のオブジェクトがない場合は自動作成
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.parent = transform;
            groundCheckObj.transform.localPosition = new Vector3(0, 0f, 0);
            groundCheck = groundCheckObj.transform;
        }
    }
    
    void Update()
    {
        HandleInput();
        CheckGrounded();
        UpdateAnimations();
    }
    
    void FixedUpdate()
    {
        MoveCharacter();
    }
    
    void HandleInput()
    {
        // 移動入力
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // 走り判定（左シフトキー）
        isRunning = Input.GetKey(KeyCode.LeftShift);
        
        // カメラの向きを基準とした移動方向を計算
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        
        // Y成分を除去して水平面に投影
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        // 移動方向を計算
        moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;
        
        // 現在の速度を設定
        if (moveDirection != Vector3.zero)
        {
            currentSpeed = isRunning ? runSpeed : walkSpeed;
        }
        else
        {
            currentSpeed = 0f;
        }
        
        // ジャンプ処理
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }
    }
    
    void MoveCharacter()
    {
        if (moveDirection != Vector3.zero)
        {
            // 移動
            Vector3 targetVelocity = moveDirection * currentSpeed;
            rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
            
            // 回転
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // 停止時は水平移動を止める
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }
    
    void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        
        // ジャンプアニメーション開始
        if (animator != null)
        {
            animator.SetTrigger(JUMP_PARAM);
        }
    }
    
    void CheckGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }
    
    void UpdateAnimations()
    {
        if (animator == null) return;
        
        // 速度パラメーター（0-1の範囲で正規化）
        float speedValue = currentSpeed / runSpeed;
        animator.SetFloat(SPEED_PARAM, speedValue, animationSmoothTime, Time.deltaTime);
        
        // 走り状態
        animator.SetBool(RUN_PARAM, isRunning && currentSpeed > 0);
        
        // 地面判定
        animator.SetBool(GROUNDED_PARAM, isGrounded);
    }
    
    void OnDrawGizmosSelected()
    {
        // グラウンドチェックの可視化
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        
        // 移動方向の可視化
        if (moveDirection != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, moveDirection * 2f);
        }
    }
}