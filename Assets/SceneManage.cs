using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using zSpace.Core;
using HighlightingSystem;
public class SceneManage : MonoBehaviour 
{
    public static CarManage carManage;
    public static  GameObject currentGrabPart = null;//当前抓取的物体部位。
    public static Color carBodyColor;
    private Shader transparentShader;
    private Shader defShader;
    private static StylusFunction _stylusFunction = StylusFunction.Move;
    private ZCore _core = null;

    public GameObject tt;
	// Use this for initialization
    public static StylusFunction StylusFuc
    {
        get
        {
            return _stylusFunction;
        }
        set
        {
            _stylusFunction = value;
        }
    }
    public GameObject CurrentGrabPart
    {
        get
        {
            return currentGrabPart;
        }
        set
        {
            currentGrabPart = value;
        }
    }
	void Start () 
    {
        
        _core = GameObject.FindObjectOfType<ZCore>();
        if (_core == null)
        {
            this.enabled = false;
            return;
        }
        //_core.TargetButtonRelease += HandleButtonRelease;
        carManage = GameObject.Find("Car").GetComponent<CarManage>();

        transparentShader = Shader.Find("Transparent/Diffuse");
        defShader = Shader.Find("Beffio/Car Paint Opaque");
       
	}

    //private void HandleButtonRelease(ZCore sender, ZCore.TrackerButtonEventInfo info)
    //{
    //    CheckStylusFuncState();
    //}
	// Update is called once per frame
	void Update () 
    {
        CheckStylusFuncState();
	}
    void CheckStylusFuncState()
    {
        switch (_stylusFunction)
        {
            case StylusFunction.Transparent:
                {
                    SetModelTransparent();
                }
                break;
            case StylusFunction.Opaque:
                {
                    SetOpaque();
                }
                break;
            case StylusFunction.Move:
                break;
            default:
                break;
        }
    }
    public static void ChangeColor()
    {
        carManage.ChangeColor(carBodyColor);
    }
    /// <summary>
    /// 将选中部分设置成透明状态
    /// </summary>
   public static void SetModelTransparent( )
    {
        Debug.LogFormat("8888888");
        if (currentGrabPart != null)
        {
            Debug.LogFormat("8888888"+currentGrabPart.name);
            carManage.SetModelTransparent(currentGrabPart);
        }
        else
        {
            Debug.LogFormat("********");
        }
    }
    /// <summary>
   /// 将选中部分恢复成正常非透明状态
    /// </summary>
    public static void SetOpaque( )
    {
        if (currentGrabPart != null)
        carManage.SetOpaque(currentGrabPart);
    }
    /// <summary>
    /// 获取目标物体及子物体上所有材质
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    List<Material> GetMats(GameObject target)
    {
        List<Material> changeMat = new List<Material>();
        if (target.GetComponent<Renderer>() != null)
        {
            foreach (Material item in target.GetComponent<Renderer>().materials)
            {
                changeMat.Add(item);
            }
        }
        foreach (Renderer item in target.GetComponentsInChildren<Renderer>())
        {
            foreach (Material mat in item.materials)
            {
                changeMat.Add(mat); 
            }
        }
        return changeMat;
    }
}
public enum StylusFunction
{
    Transparent,Opaque,Move
}
