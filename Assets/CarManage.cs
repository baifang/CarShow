using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using zSpace.Core;
public class CarManage : MonoBehaviour 
{
    private List<Transform> allTrans;
    private List<TransInfos> listInfos;
    private TransInfos infos;
    private List<Vector3> explodeEndPos;
    private float radius=0.1f;
    private float changeRate = 0.1f;

    private Shader transparentShader;
    private Shader defShader;

    public Material carBodyMat;//车身材质
	// Use this for initialization
	void Start () 
    {
        allTrans = new List<Transform>();
        listInfos = new List<TransInfos>();
        explodeEndPos = new List<Vector3>();
        RecordDefaultInfos(this.gameObject);

        transparentShader = Shader.Find("Transparent/Diffuse");
        defShader = Shader.Find("Beffio/Car Paint Opaque");
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ExplodeSphere();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            ChangeColor(Random.ColorHSV());
        }
	}
    /// <summary>
    /// 记录模型初始信息
    /// </summary>
    /// <param name="target"></param>
    public void RecordDefaultInfos(GameObject target)
    {
        for (int i = 0; i < target.transform.childCount; i++)
		{
            infos.defPos = target.transform.GetChild(i).position;
            infos.defRot = target.transform.GetChild(i).rotation;
            infos.defScal = target.transform.GetChild(i).localScale;
            allTrans.Add(target.transform.GetChild(i));

            listInfos.Add(infos);
		}        
    }
    private float j = 0;
    /// <summary>
    /// 圆形爆炸
    /// </summary>
  public  void ExplodeSphere()
    {
        for (int i = 0; i < allTrans.Count; i++)
        {
            float x = radius * Mathf.Cos(j);
            float y = radius * Mathf.Sin(j);
            allTrans[i].DOMove(new Vector3(x + listInfos[i].defPos.x, y + listInfos[i].defPos.y, listInfos[i].defPos.z), 1);
            j += changeRate;
        }
    }
    /// <summary>
    /// 组合
    /// </summary>
    public void ReExplode()
    {
        for (int i = 0; i < allTrans.Count; i++)
        {
            allTrans[i].DOMove(listInfos[i].defPos, 1);
            allTrans[i].DORotate(listInfos[i].defRot.eulerAngles, 1);
            allTrans[i].DOScale(listInfos[i].defScal, 1);
        }
    }
    /// <summary>
    /// g
    /// </summary>
    /// <param name="targetColor"></param>
    public void ChangeColor(Color targetColor)
    {
        carBodyMat.DOColor(targetColor, "_DiffuseColor",0.5f);
    }

    /// <summary>
    /// 设置成透明状态
    /// </summary>
    public void SetModelTransparent(GameObject target)
    {
        foreach (Material item in GetMats(target))
        {
            item.shader = transparentShader;
            item.color = new Color(Color.white.r, Color.white.g, Color.white.b, 0.4f);
        }
    }
    /// <summary>
    /// 恢复成正常非透明状态
    /// </summary>
    public void SetOpaque(GameObject target)
    {
        foreach (Material item in GetMats(target))
        {
            item.shader = defShader;
            item.SetColor("_DiffuseColor",SceneManage.carBodyColor);
        }
    }
    /// <summary>
    /// 获取目标物体及子物体上所有材质
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    private  List<Material> GetMats(GameObject target)
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
    private struct TransInfos
    {
        public Vector3 defPos;
        public Quaternion defRot;
        public Vector3 defScal;
    }
}
