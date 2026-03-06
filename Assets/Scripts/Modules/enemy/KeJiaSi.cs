using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeJiaSi : MonoBehaviour, IPoolable
{
    // Start is called before the first frame update
    public float lifeTime = 3f;
    
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
}
