using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shopping : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void BuyAttack()
    {
        if(GameMG.Instance.Coin<50)
        {
            return;
        }
        GameMG.Instance.Coin-=50;
        GameMG.Instance.Attack+=10;
    }
    public void BuyHP()
    {
        if(GameMG.Instance.Coin<50)
        {
            return;
        }
        GameMG.Instance.Coin-=50;
        GameMG.Instance.HP_max+=10;
        GameMG.Instance.HP=GameMG.Instance.HP_max;
    }
    public void BuyRevive()
    {
        if(GameMG.Instance.Coin<50)
        {
            return;
        }
        GameMG.Instance.Coin-=50;
        GameMG.Instance.Revive+=1;
    }
    public void BuySkill()
    {
        if(GameMG.Instance.Coin<50)
        {
            return;
        }
        GameMG.Instance.Coin-=50;
        GameMG.Instance.Skill+=1;
    }
}
