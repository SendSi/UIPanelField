using TMPro;
using UnityEngine;
using UnityEngine.UI;

#region << 脚 本 注 释 >>

//作  用:    TestForm1Controller
//作  者:    曾思信
//创建时间:  #CREATETIME#

#endregion

public class TestForm1Controller : BaseController
{
//开始
    [SerializeField] [HideInInspector] private Toggle toggle = null;
    [SerializeField] [HideInInspector] private Button button = null;
    [SerializeField] [HideInInspector] private RawImage rawImage = null;
    [SerializeField] [HideInInspector] private Slider slider = null;
    [SerializeField] [HideInInspector] private ScrollRect scrollView = null;
    [SerializeField] [HideInInspector] private Image panel = null;
    [SerializeField] [HideInInspector] private TextMeshProUGUI title = null;
    [SerializeField] [HideInInspector] private Image background = null;
//结束    

    protected override void OnInit()
    {
        base.OnInit();
    }
}