using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameMG : MonoBehaviour
{
    public static GameMG Instance { get; private set; }
    
    [Header("角色生命")]
    public int HP;
    public int HP_max;
    [Header("角色攻击")]
    public int Attack;
    [Header("角色复活")]
    public int Revive;

    public bool Revive_net;
    [Header("金币数量")]
    public int Coin;
    [Header("补兵数")]
    public int Supply;
    [Header("大招数")]
    public int Skill;
    [Header("回合数")]
    public int Round=0;
    [Header("TMP文本组件")]
    public TextMeshProUGUI hpText;           // 生命值文本
    public TextMeshProUGUI coinText;          // 金币文本
    public TextMeshProUGUI supplyText;        // 补兵数文本
    public TextMeshProUGUI roundText;         // 回合数文本（可选）
    [Header("文本格式")]
    public string hpFormat = "HP: {0}/{1}";
    public string coinFormat = "Coin: {0}";
    public string supplyFormat = "Num: {0}";
    public string roundFormat = "Round: {0}";
    [Header("颜色设置")]
    public Color normalColor = Color.black;
    public Color lowHealthColor = Color.red;
    public int lowHealthThreshold = 30; // 生命值低于这个百分比时变红


    [Header("暂停设置")]
    [SerializeField] private bool pauseAudio = true; // 是否暂停音频
    [SerializeField] private bool pausePhysics = true; // 是否暂停物理
    private bool isStop= false;
    
    [Header("商店页面")]
    public GameObject ShopPage;
    public bool isShopOpen=false;
    
    [Header("特殊事件")]
    public float TimoEventCD=60f;
    public float TimoEventTime=5f;
    void Start()
    {
        Revive_net=true;
        // 初始更新一次
        UpdateAllText();
    }
    // 更新所有文本
    public void UpdateAllText()
    {
        UpdateHpText();
        UpdateCoinText();
        UpdateSupplyText();
        UpdateRoundText();
    }
    
    // 更新生命值文本
    public void UpdateHpText()
    {
        if (hpText != null && GameMG.Instance != null)
        {
            hpText.text = string.Format(hpFormat, GameMG.Instance.HP, GameMG.Instance.HP_max);
            
            // 根据生命值百分比改变颜色
            float healthPercent = (float)GameMG.Instance.HP / GameMG.Instance.HP_max * 100;
            if (healthPercent <= lowHealthThreshold)
            {
                hpText.color = lowHealthColor;
            }
            else
            {
                hpText.color = normalColor;
            }
        }
    }
    
    // 更新金币文本
    public void UpdateCoinText()
    {
        if (coinText != null && GameMG.Instance != null)
        {
            coinText.text = string.Format(coinFormat, GameMG.Instance.Coin);
        }
    }
    
    // 更新补兵数文本
    public void UpdateSupplyText()
    {
        if (supplyText != null && GameMG.Instance != null)
        {
            supplyText.text = string.Format(supplyFormat, GameMG.Instance.Supply);
        }
    }
    
    // 更新回合数文本
    public void UpdateRoundText()
    {
        if (roundText != null && GameMG.Instance != null)
        {
            roundText.text = string.Format(roundFormat, GameMG.Instance.Round);
        }
    }
    // 添加动画效果（可选）
    public void PlayCoinAnimation()
    {
        if (coinText != null)
        {
            StopCoroutine("CoinAnimation");
            StartCoroutine(CoinAnimation());
        }
    }
    
    private IEnumerator CoinAnimation()
    {
        // 简单的缩放动画
        Vector3 originalScale = coinText.transform.localScale;
        Vector3 targetScale = originalScale * 1.3f;
        
        float duration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            coinText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }
        
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            coinText.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }
        
        coinText.transform.localScale = originalScale;
    }

    void Awake()
    {
        // 设置单例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 场景切换时不销毁
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Update is called once per frame
    void Update()
    {
        UpdateAllText();
        TimeCount();
        Act();
    }
    #region 生命及受伤

    
    public void GetHurt(int  hurt)
    { 
        HP -= hurt;
        if (HP <= 0)
        {
            if (Revive > 0&&Revive_net)
            {
                Revive_net=false;
                ToRevive();
            }
            Die();
        }
    }
    public void ToRevive()
    {
        Revive--;
        HP = HP_max;
        Revive_net=true;
    }

    public void GetHP( int  hp)
    {
        HP = HP+hp>HP_max?HP_max:HP+hp;

    }
    public void Die()
    {
        Debug.Log("死亡");
    }

    #endregion
    #region 计时器
    public void TimeCount()
    { 
       
        TimoEventTime-=Time.deltaTime;
    }

    
    #endregion
    #region 技能

    public void Act()
    {
        if (TimoEventTime <= 0)
        {
            Debug.Log("技能");
            TimoEventTime = TimoEventCD;
            TiMuo();
            
        }
    }
    #endregion

    #region 暂停

     public void Stop()
    {
        if (!isStop)
        {
            isStop = true;
            Time.timeScale = 0;
            
            // 暂停音频
            if (pauseAudio)
            {
                AudioListener.pause = true;
            }
            
            // 可选：暂停物理系统（2D和3D）
            if (pausePhysics)
            {
                // 如果有 Rigidbody 需要单独处理冻结
                FreezeAllRigidbodies(true);
            }

           
            OnGamePaused();
            
            Debug.Log("游戏已暂停");
        }
    }
    
    public void Resume()
    {
        if (isStop)
        {
            isStop = false;
            Time.timeScale = 1;
            
            // 恢复音频
            if (pauseAudio)
            {
                AudioListener.pause = false;
            }
            
            // 恢复物理系统
            if (pausePhysics)
            {
                FreezeAllRigidbodies(false);
            }
            
            // 触发恢复事件
            OnGameResumed();
            
            Debug.Log("游戏已恢复");
        }
    }
    
    private Dictionary<Rigidbody2D, Tuple<Vector2, float, RigidbodyConstraints2D>> rigidbodyStates2D = 
        new Dictionary<Rigidbody2D, Tuple<Vector2, float, RigidbodyConstraints2D>>();

    private void FreezeAllRigidbodies(bool freeze)
    {
        Rigidbody2D[] rigidbodies2D = FindObjectsOfType<Rigidbody2D>();
    
        foreach (Rigidbody2D rb in rigidbodies2D)
        {
            if (freeze)
            {
                // 保存速度、角速度和约束
                if (!rigidbodyStates2D.ContainsKey(rb))
                {
                    rigidbodyStates2D[rb] = Tuple.Create(rb.velocity, rb.angularVelocity, rb.constraints);
                }
            
                // 清零速度并冻结
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            else
            {
                // 恢复
                if (rigidbodyStates2D.ContainsKey(rb) && rb != null)
                {
                    var state = rigidbodyStates2D[rb];
                    rb.velocity = state.Item1;
                    rb.angularVelocity = state.Item2;
                    rb.constraints = state.Item3;
                
                    rigidbodyStates2D.Remove(rb);
                }
            }
        }
    }
    
    // 暂停事件（供其他脚本监听）
    public System.Action OnPaused;
    public System.Action OnResumed;
    
    private void OnGamePaused()
    {
        OnPaused?.Invoke();
    }
    
    private void OnGameResumed()
    {
        OnResumed?.Invoke();
    }
    
    // 切换暂停状态
    public void TogglePause()
    {
        if (isStop)
            Resume();
        else
            Stop();
    }
    
    // 获取暂停状态
    public bool IsPaused
    {
        get { return isStop; }
    }

    #endregion

    public void transShop()
    {
        if (!isShopOpen)
        {
            isShopOpen = true;
            ShopPage.SetActive(true);
        }
        else
        {
            isShopOpen = false;
            ShopPage.SetActive(false);
        }
        
    }

    #region 特殊事件

    public void TiMuo()
    { 
        Timo.Instance.StartWorking() ;
    }

    #endregion
    
  
}
