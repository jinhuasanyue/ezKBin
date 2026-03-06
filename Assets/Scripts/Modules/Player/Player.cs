using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("位置点")] 
    public GameObject Up_pos;
    public GameObject Min_pos;
    public GameObject Down_pos;
    private List<Vector3> pos_list = new List<Vector3>();
    private int pos_index=1;
    private bool jump_net;
    
    [Header("图片")]
    public Sprite player_Idel;
    public Sprite player_Attack;
    public Sprite player_Jump;
    private SpriteRenderer sp;
    [Header("粒子")]
    public ParticleSystem particle;

    private float originalRadius;
    [Header("攻击")]
    private bool attack_net;
    public GameObject Attack_pos;
    public float bulletSpeed = 10f;
    public GameObject Bullet;


    void Start()
    {
        pos_list.Add(Up_pos.transform.position);
        pos_list.Add(Min_pos.transform.position);
        pos_list.Add(Down_pos.transform.position);
        transform.position = pos_list[pos_index];
        particle.gameObject.SetActive(false);
        sp = GetComponentInChildren<SpriteRenderer>();
        jump_net=true;
        attack_net=true;
        sp.sprite = player_Idel;
        // 获取粒子的形状模块
        var shape = particle.shape;
        originalRadius = shape.radius; // 保存原始半径
        // 预加载子弹池，初始10个
        Pool.Instance.PreloadPool(Bullet, 10);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region 位移及攻击
    
    public  void TransPos_Up()
    {
        // 如果暂停了，等待直到恢复
        if(!GameMG.Instance.IsPaused)
            if (pos_index > 0&&jump_net)
            {
                jump_net = false;
                pos_index -=1;
            
                StartCoroutine(TransPos_Wait());
           
            }
        
       
       
    }
    public IEnumerator TransPos_Wait()
    {
        sp.sprite = player_Jump;
        particle.gameObject.SetActive(true);
    
       
        float elapsedTime = 0f;
        float shrinkDuration = 0.2f; // 收缩持续时间
        var shape = particle.shape;
        // 在0.2秒内逐渐将半径缩小到0
        while (elapsedTime < shrinkDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / shrinkDuration; // 计算进度（0到1）
            shape.radius = Mathf.Lerp(originalRadius, 0, t); // 从原始半径插值到0
            yield return null; // 等待一帧
        }
    
        // 确保半径为0
        shape.radius = 0;
        sp.sprite = null;
        particle.gameObject.SetActive(false);
    
        // 等待0.4秒
        yield return new WaitForSeconds(0.4f);
    
        // 移动位置
        transform.position = pos_list[pos_index];
        sp.sprite = player_Idel;
    
        // 恢复粒子半径
        shape.radius = originalRadius;
        jump_net=true;
    }
    public void TransPos_Down()
    {
        if(!GameMG.Instance.IsPaused)
          if (pos_index < pos_list.Count - 1&&jump_net)
        {
            jump_net=false;
            pos_index += 1;
          
            StartCoroutine(TransPos_Wait());
            
        }
    }
    

    public void TransFace_Left()
    {
        if(!GameMG.Instance.IsPaused&&!Timo.Instance.blind)
          if (attack_net)
          {
            attack_net = false;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            StartCoroutine(Attack_Wait(-1));
          }
        
    }

    public IEnumerator Attack_Wait(int dir)
    {
        sp.sprite = player_Attack;
    
        // 生成并发射子弹
        GameObject bullet = Pool.Instance.GetObject(Bullet, Attack_pos.transform.position,  Quaternion.Euler(0, 0, 90));
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
    
        if (rb != null)
        {
            // 通过localScale.x判断朝向（通常2D游戏中这样处理）
            float direction = Mathf.Sign(transform.localScale.x); // 1表示向右，-1表示向左
            rb.velocity = new Vector2(dir*direction * bulletSpeed, 0);
        }
    
        yield return new WaitForSeconds(0.2f);
        sp.sprite = player_Idel;
        attack_net = true;
    }
    public void TransFace_Right()
    {
        if(!GameMG.Instance.IsPaused&&!Timo.Instance.blind)
          if (attack_net)
          {
            attack_net = false;
            transform.rotation = Quaternion.Euler(0, 180, 0);
            StartCoroutine(Attack_Wait(1));
          }

    }
    #endregion

   
}
