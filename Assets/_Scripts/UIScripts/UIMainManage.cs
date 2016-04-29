using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using DG.Tweening;
public class UIMainManage : MonoBehaviour
{
    Button button_Reset;
    Button button_Move;
    Button button_Exploder;
    Button button_Exit;


    Button button_Transparent;
    Button button_Opaque;
    Button button_ChangeColor;

    private GameObject lastClickButton;
    private Image lastClickButtonImage;
    public GameObject colorPanel;
    private Vector3 endPos = new Vector3(700, 30, 0);
    private Vector3 startPos = new Vector3(700, -80, 0);
    private bool isExploder = false;
    private bool isChooseColor = false;
    /// <summary>
    /// 检测是否悬浮的UI上
    /// </summary>
    void IfIsOnUI()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Cursor.visible = true;
        }
        else
        {
            Cursor.visible = false;
        }
    }
    void Update()
    {
       // IfIsOnUI();
    }
    void Start()
    {
        //button_Move = GameObject.Find("Button_Move").GetComponent<Button>();
         button_Reset = GameObject.Find("Button_Reset").GetComponent<Button>();
        button_Exploder = GameObject.Find("Button_Exploder").GetComponent<Button>();
        button_Transparent = GameObject.Find("Button_Transparent").GetComponent<Button>();
        button_Opaque = GameObject.Find("Button_Opaque").GetComponent<Button>();
        button_Exit = GameObject.Find("Button_Exit").GetComponent<Button>();
        button_ChangeColor = GameObject.Find("Button_ChangeColor").GetComponent<Button>();
        if (button_Move == null || button_Exploder == null || button_Transparent == null || button_Opaque == null || button_Exit == null)
        {
            this.enabled = true;
        }


      //  EventTriggerListener.Get(button_Move.gameObject).onClick = OnButtonClick;
        EventTriggerListener.Get(button_Transparent.gameObject).onClick = OnButtonClick;
        EventTriggerListener.Get(button_Opaque.gameObject).onClick = OnButtonClick;
        EventTriggerListener.Get(button_Exploder.gameObject).onClick = OnButtonClick;
        EventTriggerListener.Get(button_Exit.gameObject).onClick = OnButtonClick;
        EventTriggerListener.Get(button_ChangeColor.gameObject).onClick = OnButtonClick;
        EventTriggerListener.Get(button_Reset.gameObject).onClick = OnButtonClick;


        EventTriggerListener.Get(button_ChangeColor.gameObject).onEnter = OnEnter;
        EventTriggerListener.Get(button_ChangeColor.gameObject).onExit = OnExit;

        EventTriggerListener.Get(button_Transparent.gameObject).onEnter = OnEnter;
        EventTriggerListener.Get(button_Transparent.gameObject).onExit = OnExit;

        EventTriggerListener.Get(button_Opaque.gameObject).onEnter = OnEnter;
        EventTriggerListener.Get(button_Opaque.gameObject).onExit = OnExit;

        EventTriggerListener.Get(button_Reset.gameObject).onEnter = OnEnter;
        EventTriggerListener.Get(button_Reset.gameObject).onExit = OnExit;

        //EventTriggerListener.Get(button_Move.gameObject).onEnter = OnEnter;
        //EventTriggerListener.Get(button_Move.gameObject).onExit = OnExit;

        EventTriggerListener.Get(button_Exploder.gameObject).onEnter = OnEnter;
        EventTriggerListener.Get(button_Exploder.gameObject).onExit = OnExit;

        EventTriggerListener.Get(button_Exit.gameObject).onEnter = OnEnter;
        EventTriggerListener.Get(button_Exit.gameObject).onExit = OnExit;
    }

    private void OnEnter(GameObject go)
    {
        if (go.transform.GetChild(0) != null)
        go.transform.GetChild(0).gameObject.SetActive(true);
        
        //go.GetComponentInChildren<Transform>().gameObject.SetActive(true);

    }
    private void OnExit(GameObject go)
    {
        if (go.transform.GetChild(0)!=null)
        go.transform.GetChild(0).gameObject.SetActive(false);
    }
    /// <summary>
    /// 点击按钮响应函数
    /// </summary>
    /// <param name="go"></param>
    private void OnButtonClick(GameObject go)
    {
        if (lastClickButtonImage != null)
        {
            lastClickButtonImage.color = Color.white;
        }
            lastClickButton = go;
            lastClickButtonImage = go.GetComponent<Image>();
            lastClickButtonImage.color = Color.blue;
        
        switch (go.name)
        {

            case "Button_Move":
                {
                    SceneManage.StylusFuc = StylusFunction.Move;
                }
                break;
            case "Button_Reset":
                {
                    SceneManage.StylusFuc = StylusFunction.Move;
                    SceneManage.carBodyColor = Color.grey;
                    SceneManage.currentGrabPart = null;
                    Application.LoadLevel(Application.loadedLevel);
                }
                break;
            case "Button_Exploder":
                {
                    isExploder = !isExploder;
                    if (isExploder)
                    {
                        SceneManage.carManage.ExplodeSphere();
                        go.transform.GetChild(0).gameObject.GetComponent<Text>().text = "组合";
                    }
                    else
                    {
                        SceneManage.carManage.ReExplode();
                        go.transform.GetChild(0).gameObject.GetComponent<Text>().text = "拆分";
                    }
                }
                break;
            case "Button_Transparent":
                {
                    SceneManage.StylusFuc = StylusFunction.Transparent;
                }
                break;
            case "Button_Opaque":
                {
                    SceneManage.StylusFuc = StylusFunction.Opaque;
                }
                break;
            case "Button_ChangeColor":
                {
                    isChooseColor = !isChooseColor;
                    SceneManage.StylusFuc = StylusFunction.Move;
                    if (isChooseColor)
                    {
                        colorPanel.GetComponent<RectTransform>().DOMove(endPos, 0.5f);
                    }
                    else
                    {
                        colorPanel.GetComponent<RectTransform>().DOMove(startPos, 0.5f);
                    }
                }
                break;
            case "Button_Exit":
                {
                    Application.Quit();
                }
                break;
            default:
                break;
        }
    }
    public void ChangeColor()
    {
        Color targetColor;
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            targetColor = EventSystem.current.currentSelectedGameObject.GetComponent<Image>().color;
            SceneManage.carBodyColor = targetColor;
            SceneManage.ChangeColor();
        }
    
    }


}