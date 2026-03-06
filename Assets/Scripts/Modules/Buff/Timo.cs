using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timo : MonoBehaviour
{
    [Header("位置点")] 
    public GameObject Nom_pos;
    public GameObject Work_pos;
    private bool isworking;
    public bool blind;

    public GameObject Left_buttom;
    public GameObject Right_buttom;
    private Color Left_buttom_coloer;
    private Color Right_buttom_coloer;
    public static Timo Instance { get; private set; }
    private enum State
    {
        MovingToWork,
        Working,
        MovingToHome,
        Idle
    }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    private State currentState = State.Idle;
    private float workDuration = 2f; // 工作时间
    private float workTimer = 0f;

    void Start()
    {
        Left_buttom_coloer = Left_buttom.GetComponent<Image>().color; 
        Right_buttom_coloer = Right_buttom.GetComponent<Image>().color;

        blind=false;
    }

    // Update is called once per frame
    void Update()
    {
       
        switch (currentState)
        {
            case State.MovingToWork:
                // 向工作位置移动
                transform.position = Vector3.MoveTowards(
                    transform.position, 
                    Work_pos.transform.position, 
                    Time.deltaTime * 5f);
            
                // 检查是否到达
                if (transform.position == Work_pos.transform.position)
                {
                    currentState = State.Working;
                    workTimer = workDuration;
                }
                break;
            
            case State.Working:
                // 执行工作（致盲）
                blind=true;
                Left_buttom.GetComponent<Image>().color = new Color(1, 1, 1, 0f);
                Right_buttom.GetComponent<Image>().color = new Color(1, 1, 1, 0f);
                workTimer -= Time.deltaTime;
                if (workTimer <= 0)
                {
                    currentState = State.MovingToHome;
                }
                break;
            
            case State.MovingToHome:
                Left_buttom.GetComponent<Image>().color = Left_buttom_coloer;
                Right_buttom.GetComponent<Image>().color = Right_buttom_coloer;
                blind=false;
                // 向初始位置返回
                transform.position = Vector3.MoveTowards(
                    transform.position, 
                    Nom_pos.transform.position, 
                    Time.deltaTime * 5f);
            
                // 检查是否到达
                if (transform.position == Nom_pos.transform.position)
                {
                    currentState = State.Idle;
                }
                break;
            
            case State.Idle:
                // 空闲状态，等待指令
                break;
        }
    }

// 外部调用，开始工作
    public void StartWorking()
    {
        Debug.Log("开始工作");
        currentState = State.MovingToWork;
    }

   
}
