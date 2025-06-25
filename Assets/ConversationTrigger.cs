using UnityEngine;
using Fungus;

public class ConversationTrigger : MonoBehaviour
{
    [Header("会話設定")]
    public Flowchart flowchart; // Fungus Flowchartへの参照
    public string blockName = "Start"; // 実行するブロック名
    public float interactionDistance = 3f; // 会話可能距離
    
    [Header("UI設定")]
    public GameObject interactionUI; // 「Eキーで話す」のようなUI表示
    
    private GameObject player;
    private bool playerInRange = false;
    private bool isConversationActive = false;

    void Start()
    {
        // Playerタグを持つオブジェクトを取得
        player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            Debug.LogError("Playerタグを持つオブジェクトが見つかりません！");
        }
        
        if (flowchart == null)
        {
            Debug.LogError("Flowchartが設定されていません！");
        }
        
        // UI非表示にしておく
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }

    void Update()
    {
        if (player == null) return;
        
        // 会話中でない場合の処理
        if (!isConversationActive)
        {
            // プレイヤーとの距離を計算
            float distance = Vector3.Distance(transform.position, player.transform.position);
            
            // 距離チェック
            if (distance <= interactionDistance)
            {
                if (!playerInRange)
                {
                    playerInRange = true;
                    ShowInteractionUI(true);
                }
                
                // Eキーが押された場合
                if (Input.GetKeyDown(KeyCode.E))
                {
                    StartConversation();
                }
            }
            else
            {
                if (playerInRange)
                {
                    playerInRange = false;
                    ShowInteractionUI(false);
                }
            }
        }
    }
    
    void StartConversation()
    {
        if (flowchart != null && !isConversationActive)
        {
            isConversationActive = true;
            ShowInteractionUI(false);
            
            // プレイヤーの移動を無効化（オプション）
            DisablePlayerMovement(true);
            
            // Fungusの会話開始
            flowchart.ExecuteBlock(blockName);
            
            // 会話終了を監視
            StartCoroutine(WaitForConversationEnd());
        }
    }
    
    System.Collections.IEnumerator WaitForConversationEnd()
    {
        // Fungusの会話が終了するまで待機
        while (flowchart.GetExecutingBlocks().Count > 0)
        {
            yield return null;
        }
        
        // 会話終了後の処理
        isConversationActive = false;
        DisablePlayerMovement(false);
    }
    
    void DisablePlayerMovement(bool disable)
    {
        // プレイヤーの移動スクリプトを無効/有効化
        if (player != null)
        {
            var playerScript = player.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.enabled = !disable;
            }
            
            // Rigidbodyがある場合は停止
            var rb = player.GetComponent<Rigidbody>();
            if (rb != null && disable)
            {
                rb.velocity = Vector3.zero;
            }
        }
    }
    
    void ShowInteractionUI(bool show)
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(show);
        }
    }
    
    // デバッグ用：距離の可視化
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}