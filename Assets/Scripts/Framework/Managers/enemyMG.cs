using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyMG : MonoBehaviour
{
    [System.Serializable]
    public class Enemys
    {
       public GameObject enemy;
       public int Speed;
      

    }
    [Header("位置点")] 
    public GameObject Up_pos;
    public GameObject Min_pos;
    public GameObject Down_pos;
    private List<Vector3> pos_list = new List<Vector3>();
    private int pos_index=1;
    public int Dir=1;
    [Header("生成属性")] 
    public List<Enemys> Enemy_List = new List<Enemys>();
    public List<int> SpawnWeights = new List<int>(); // 添加权重列表
    public float MakeCD=1f;
    private float MakeTime=0f;
    public float DuringCD=60f;
    private float DuringTime=-1f;
    private int roundCounter = 0; // 添加计数器
    private int Post_Index;
    private int Type_Index;
   
    void Start()
    {
        pos_list.Add(Up_pos.transform.position);
        pos_list.Add(Min_pos.transform.position);
        pos_list.Add(Down_pos.transform.position);
        transform.position = pos_list[pos_index];
        foreach (Enemys enemy in Enemy_List)
        {
            Pool.Instance.PreloadPool(enemy.enemy, 10);
        }
        // 如果没有设置权重，初始化默认权重都为1
        if (SpawnWeights.Count == 0)
        {
            for (int i = 0; i < Enemy_List.Count; i++)
            {
                SpawnWeights.Add(1);
            }
        }
       
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameMG.Instance.IsPaused)
        {
            if (DuringTime == 0)
            {
                GameMG.Instance.Round++;
            }
            TimeCount();
            Actor();
        }
    }
    private void TimeCount()
    { 
        MakeTime-=Time.deltaTime;
        DuringTime-=Time.deltaTime;
        
    }
    private void Actor()
    { 
        if (MakeTime<=0&&DuringTime>0)
        {
            MakeEnemy();
            MakeTime=MakeCD;
        }
        
        // 每过60秒，DuringTime会重置
        if (DuringTime <= -5f)
        {
            DuringTime = DuringCD;
            roundCounter++;
            GameMG.Instance.Round = roundCounter; // 直接设置回合数
        }
    }

    private void MakeEnemy()
    {
        Post_Index = Random.Range(0, pos_list.Count);
        // 使用权重随机选择敌人类型
        Type_Index = GetRandomEnemyIndexWithWeight();
        GameObject enemy = null;
        float enemySpeed = 0f;
        
        enemy = Pool.Instance.GetObject(Enemy_List[Type_Index].enemy, pos_list[Post_Index], Quaternion.identity);
        enemySpeed = Enemy_List[Type_Index].Speed;
      
        
        if (enemy != null)
        {
            Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // 通过localScale.x判断朝向（通常2D游戏中这样处理）
                float direction = Mathf.Sign(transform.localScale.x); // 1表示向右，-1表示向左
                rb.velocity = new Vector2(Dir * direction * enemySpeed, 0);
                // **然后设置旋转**
                if (Dir == 1)
                {   
                  
                    if(Type_Index!=2)
                    enemy.transform.eulerAngles = new Vector3(0, 180, 0);
                    else
                    {
                        enemy.transform.eulerAngles = new Vector3(0, 0, 0);
                    }
                }
                else
                {
                    
                    if(Type_Index!=2)
                    enemy.transform.eulerAngles = new Vector3(0, 0, 0);
                    else
                    {
                        enemy.transform.eulerAngles = new Vector3(0, 180, 0);
                    }
                   
                }
            }
        }

        
    }
    // 根据权重随机选择敌人
    private int GetRandomEnemyIndexWithWeight()
    {
        // 计算总权重
        int totalWeight = 0;
        foreach (int weight in SpawnWeights)
        {
            totalWeight += weight;
        }
    
        // 随机一个数值
        int randomValue = Random.Range(0, totalWeight);
    
        // 根据权重选择
        int cumulativeWeight = 0;
        for (int i = 0; i < SpawnWeights.Count; i++)
        {
            cumulativeWeight += SpawnWeights[i];
            if (randomValue < cumulativeWeight)
            {
                return i;
            }
        }
    
        return 0; // 默认返回第一个
    }
}

