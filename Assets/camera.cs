using UnityEngine;

public class camera : MonoBehaviour
{
    [Header("ターゲット設定")]
    public Transform target; // ユニティちゃんのTransform
    public Vector3 offset = new Vector3(0, 1.8f, -3f); // カメラオフセット
    public float height = 1.5f; // 注視点の高さ
    
    [Header("カメラ制御")]
    public float mouseSensitivity = 2f;
    public float smoothSpeed = 2f;
    public float minVerticalAngle = -20f;
    public float maxVerticalAngle = 45f;
    
    [Header("ズーム設定")]
    public float minDistance = 1.5f;
    public float maxDistance = 5f;
    public float zoomSpeed = 2f;
    
    [Header("衝突回避")]
    public LayerMask collisionLayers = 1;
    public float collisionBuffer = 0.3f;
    
    private float yaw;
    private float pitch;
    private float currentDistance;
    private Vector3 currentPosition;
    private Vector3 velocity;
    
    void Start()
    {
        // 初期設定
        currentDistance = offset.magnitude;
        
        if (target != null)
        {
            Vector3 angles = transform.eulerAngles;
            yaw = angles.y;
            pitch = angles.x;
        }
        
        // マウスカーソルをロック
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    void Update()
    {
        HandleInput();
        
        // ESCキーでカーソルのロック/解除を切り替え
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursorLock();
        }
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        UpdateCameraPosition();
    }
    
    void HandleInput()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;
        
        // マウス入力
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
        
        // マウスホイールでズーム
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentDistance = Mathf.Clamp(currentDistance - scroll * zoomSpeed, minDistance, maxDistance);
    }
    
    void UpdateCameraPosition()
    {
        // 注視点を計算（ユニティちゃんの少し上）
        Vector3 targetPosition = target.position + Vector3.up * height;
        
        // カメラの理想位置を計算
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desiredPosition = targetPosition + rotation * Vector3.back * currentDistance;
        
        // 壁との衝突をチェック
        Vector3 finalPosition = CheckForCollision(targetPosition, desiredPosition);
        
        // スムーズに移動
        currentPosition = Vector3.SmoothDamp(currentPosition, finalPosition, ref velocity, 1f / smoothSpeed);
        transform.position = currentPosition;
        
        // カメラを注視点に向ける
        transform.LookAt(targetPosition);
    }
    
    Vector3 CheckForCollision(Vector3 targetPos, Vector3 desiredPos)
    {
        Vector3 direction = desiredPos - targetPos;
        float distance = direction.magnitude;
        
        RaycastHit hit;
        if (Physics.Raycast(targetPos, direction.normalized, out hit, distance, collisionLayers))
        {
            // 壁がある場合、少し手前に配置
            return hit.point - direction.normalized * collisionBuffer;
        }
        
        return desiredPos;
    }
    
    void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (target == null) return;
        
        // 注視点を可視化
        Vector3 lookAtPoint = target.position + Vector3.up * height;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(lookAtPoint, 0.1f);
        
        // カメラとターゲットの線を可視化
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, lookAtPoint);
        
        // ズーム範囲を可視化
        Gizmos.color = Color.blue;
        Vector3 direction = (transform.position - lookAtPoint).normalized;
        Gizmos.DrawWireSphere(lookAtPoint + direction * minDistance, 0.1f);
        Gizmos.DrawWireSphere(lookAtPoint + direction * maxDistance, 0.1f);
    }
}