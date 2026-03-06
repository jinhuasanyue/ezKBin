using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sien : MonoBehaviour, IPoolable
{
   
    public float lifeTime = 3f;
    public int Hurt = 10;
    
    void OnEnable()
    {
        // 启动生命周期协程
        StartCoroutine(AutoReturn());
    }
    
    IEnumerator AutoReturn()
    {
       
            yield return new WaitForSeconds(lifeTime);
        
        // 回收回对象池
        Pool.Instance.ReturnObject(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnObjectSpawn()
    {
       
    }
    
    public void OnObjectReturn()
    {
        
        StopAllCoroutines();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag("Player"))
        {
            GameMG.Instance.GetHurt(Hurt);
        }
    }
}
