using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IPoolable
{
    private Animation anim;
    private float hp = 10f;
    private SpriteRenderer sr;
    public float lifeTime = 3f;
    
    private bool isDead = false; // 添加死亡标记
    private Coroutine autoReturnCoroutine; // 保存协程引用
    
    void Start() 
    {
        anim = GetComponentInChildren<Animation>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }
    
    void OnEnable()
    {
        // 重置状态
        isDead = false;
        
        // 根据回合数设置生命值
        if (GameMG.Instance != null)
        {
            hp = GameMG.Instance.Round * 10f ;
        }
        else
        {
            hp = 10f;
        }
        
        // 重置颜色
        if (sr != null)
        {
            sr.color = Color.white;
        }
        
        // 启动生命周期协程并保存引用
        autoReturnCoroutine = StartCoroutine(AutoReturn());
    }
    
    IEnumerator AutoReturn()
    {
       
            yield return new WaitForSeconds(lifeTime);
        
        // 只有在没有死亡的情况下才自动回收
        if (!isDead && gameObject.activeInHierarchy)
        {
            ReturnToPool();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return; // 如果已经死亡，不再处理
        
        if (other.CompareTag("KeJiaSi"))
        {
            ReturnToPool();
        }
        else if (other.CompareTag("Bullet"))
        {
            // 确保 GameMG 实例存在
            if (GameMG.Instance == null) return;
            
            // 扣血
            hp -= GameMG.Instance.Attack;
            
            // 受伤变色
            StartCoroutine(HurtColor());
            
            // 检查死亡
            if (hp <= 0)
            {
                Die();
            }
        }
    }
    
    private void Die()
    {
        if (isDead) return; // 防止重复死亡
        
        isDead = true;
        
        // 先停止自动回收协程
        if (autoReturnCoroutine != null)
        {
            StopCoroutine(autoReturnCoroutine);
            autoReturnCoroutine = null;
        }
        
        // 增加游戏数据
        if (GameMG.Instance != null)
        {
            // 注意：hp <= 0，不能直接加hp，应该加固定值或绝对值
            GameMG.Instance.Coin += 10; // 假设每个敌人固定给10金币
            // 或者 GameMG.Instance.Coin += Mathf.Abs((int)hp); // 取绝对值
            
            GameMG.Instance.Supply += 1;
        }
        
        // 回收对象
        ReturnToPool();
    }
    
    private void ReturnToPool()
    {
        if (!gameObject.activeInHierarchy) return; // 防止重复回收
        
        if (Pool.Instance != null)
        {
            Pool.Instance.ReturnObject(gameObject);
        }
        else
        {
            // 如果没有对象池，直接销毁（备用方案）
            Destroy(gameObject);
        }
    }
    
    IEnumerator HurtColor()
    {
        if (sr == null || isDead) yield break;
        
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        
        // 只有在没有死亡的情况下才恢复白色
        if (!isDead && sr != null)
        {
            sr.color = Color.white;
        }
    }
    
    // IPoolable 接口实现
    public void OnObjectSpawn()
    {
        // 从池中取出时调用
        Debug.Log("敌人已生成");
    }
    
    public void OnObjectReturn()
    {
        // 停止所有协程
        StopAllCoroutines();
        autoReturnCoroutine = null;
        
        // 重置状态
        isDead = false;
        
        Debug.Log("敌人已回收");
    }
    
    private void OnDisable()
    {
        // 当对象被禁用时（比如被回收），停止所有协程
        StopAllCoroutines();
        autoReturnCoroutine = null;
    }
}