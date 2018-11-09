using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UITest : MonoBehaviour
{
    static UITest m_cInstance;
    public static UITest instance { get { return m_cInstance; } }
    private void Awake()
    {
        m_cInstance = this;
    }

    public string txt = "测试";
    private Text m_cTxt;

    // Use this for initialization
    void Start()
    {
        m_cTxt = this.gameObject.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        m_cTxt.text = txt;
    }
}
