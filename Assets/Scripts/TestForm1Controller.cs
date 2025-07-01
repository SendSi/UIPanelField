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
    [SerializeField] private Toggle toggle = null;
    [SerializeField] private Button button = null;
    [SerializeField] private RawImage rawImage = null;
    [SerializeField] private Slider slider = null;
    [SerializeField] private ScrollRect scrollView = null;
    [SerializeField] private Image panel = null;
    [SerializeField] private TextMeshProUGUI title = null;
    [SerializeField] private Image background = null;
    [SerializeField] private GameObject nameTxtGo = null;
    [SerializeField] private TextMeshProUGUI nameTxt = null;
    [SerializeField] private Image image_new = null;
    [SerializeField] private GameObject image_new2 = null;
//结束
    protected override void OnInit()
    {
        base.OnInit();
    }
}