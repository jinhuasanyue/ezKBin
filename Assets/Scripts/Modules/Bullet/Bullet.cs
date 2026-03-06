using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 在子弹脚本中
public class Bullet : MonoBehaviour, IPoolable
{
    public float lifeTime = 3f;
    
    void OnEnable()
    {
        // 启动生命周期协程
        StartCoroutine(AutoReturn());
    }
    
    IEnumerator AutoReturn()
    {
        // 如果暂停了，等待直到恢复
        while (!GameMG.Instance.IsPaused)
        {
            yield return new WaitForSeconds(lifeTime);
        }
       
        // 回收回对象池
        Pool.Instance.ReturnObject(gameObject);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
           
            Pool.Instance.ReturnObject(gameObject);
        }
    }
    
    // IPoolable 接口实现
    public void OnObjectSpawn()
    {
        // 从池中取出时调用
        Debug.Log("子弹已生成");
    }
    
    public void OnObjectReturn()
    {
        // 回收时调用
        Debug.Log("子弹已回收");
        // 停止所有协程，避免重复运行
        StopAllCoroutines();
    }
}
