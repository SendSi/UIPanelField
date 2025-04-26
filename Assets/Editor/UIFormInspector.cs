using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UIForm), true)]
public class UIFormInspector : Editor
{
    private UIForm uiFormScript;
    private UnityEngine.Object dragInEle;
    string genScriptPath = $"{Application.dataPath}/scripts/";

    private void OnEnable()
    {
        uiFormScript = (UIForm)target;
    }

    private void DelNullReference()
    {
        var keyOneEnter = new Dictionary<string, UICompData>();
        foreach (var item in uiFormScript.Data)
        {
            if (item.Obj != null)
            {
                keyOneEnter[item.Key] = item;
            }
        }

        uiFormScript.Data.Clear();
        uiFormScript.Data = keyOneEnter.Values.ToList();
    }

    public override void OnInspectorGUI()
    {
        var dataProperty = serializedObject.FindProperty("Data");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.ObjectField(dragInEle, typeof(UnityEngine.Object), false);
        GUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("删除[空引用] [同字段名]"))
        {
            DelNullReference();
        }

        if (GUILayout.Button("生成代码"))
        {
            var allComps = uiFormScript.GetAllComps();
            var scriptTxt = GetStrCopyBuffer(allComps);

            BaseController[] baseCtrls = uiFormScript.GetComponents<BaseController>();
            if (baseCtrls.Length > 0)
            {
                string scriptName = baseCtrls[0].GetType().Name;

                var files = Directory.GetFiles(genScriptPath, "*.cs", SearchOption.AllDirectories);
                var filePath = string.Empty;
                foreach (var pathT in files)
                {
                    if (pathT.Contains(scriptName))
                    {
                        filePath = pathT;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(filePath) == false)
                {
                    string fileContent = File.ReadAllText(filePath);
                    string pattern = @"\/\/开始[\s\S]*?\/\/结束";
                    string newContent = Regex.Replace(fileContent, pattern, "", RegexOptions.Singleline);

                    var newT = tmpTxtCtrl + scriptTxt; //  BaseController { 后面的内容
                    var newContent2 = newContent.Replace(tmpTxtCtrl, newT);
                    File.WriteAllText(filePath, newContent2);
                    AssetDatabase.Refresh();

                    Debug.LogError($"修改脚本={scriptName}");
                }
            }
        }

        if (GUILayout.Button("填充序列值"))
        {
            BaseController[] baseCtrls = uiFormScript.GetComponents<BaseController>();
            if (baseCtrls.Length > 0)
            {
                SerializedObject serializedObject = new SerializedObject(baseCtrls[0]);
                serializedObject.Update();
                var allComps = uiFormScript.GetData();

                foreach (var item in allComps)
                {
                    string varName = item.Key;
                    var property = serializedObject.FindProperty(varName);
                    var itemCom = item.Comp;
                    if (itemCom == null)
                    {
                        Debug.LogError($"######检测到变量:{varName}, GameObject引用丢失!########");
                        continue;
                    }

                    property.objectReferenceValue = item.Comp != null ? item.Comp : item.Obj;
                    serializedObject.ApplyModifiedProperties();
                }

                AssetDatabase.Refresh();
            }
        }

        EditorGUILayout.EndHorizontal();

        var delList = new List<int>();
        for (int i = uiFormScript.Data.Count - 1; i >= 0; i--)
        {
            GUILayout.BeginHorizontal();
            UICompData data = uiFormScript.Data[i];

            string keyT;
            Object gameObjectT;
            keyT = EditorGUILayout.TextField(data.Key, GUILayout.Width(150));
            keyT = keyT.Substring(0, 1).ToLower() + keyT.Substring(1); // 首字母小写
            gameObjectT = EditorGUILayout.ObjectField(data.Obj, typeof(Object), true);

            data.Key = keyT;
            data.Obj = gameObjectT;

            if (data.Obj && (GameObject)data.Obj)
            {
                GameObject mObject = (GameObject)data.Obj;
                Component[] components = mObject.GetComponents<Component>();
                if (components.Length > 0)
                {
                    data.Components = new string[components.Length + 1];
                    int[] values = new int[components.Length + 1];
                    if (components.Length > 0)
                    {
                        for (int n = 0; n < components.Length; n++)
                        {
                            values[n] = n;
                            data.Components[n] = components[n].GetType().Name;
                        }
                    }

                    data.Components[components.Length] = "GameObject";
                    values[components.Length] = components.Length;
                    data.Index =
                        EditorGUILayout.IntPopup("", data.Index, data.Components, values, GUILayout.Width(150));
                    if (data.Index == components.Length)
                    {
                        data.Comp = null;
                    }
                    else
                    {
                        data.Index = data.Index > components.Length ? 0 : data.Index;
                        data.Comp = components[data.Index];
                    }
                }
            }

            if (GUILayout.Button("X"))
            {
                delList.Add(i); //将元素添加进删除list
            }

            GUILayout.EndHorizontal();
        }


        var eventType = Event.current.type;
        if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (eventType == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                foreach (var o in DragAndDrop.objectReferences)
                {
                    AddReference(dataProperty, o.name, o);
                }
            }

            Event.current.Use();
        }

        //遍历删除list，将其删除掉
        foreach (var i in delList)
        {
            dataProperty.DeleteArrayElementAtIndex(i);
        }

        serializedObject.ApplyModifiedProperties();
        serializedObject.UpdateIfRequiredOrScript();
    }

    private void AddReference(SerializedProperty dataProperty, string key, Object obj)
    {
        int index = dataProperty.arraySize;
        dataProperty.InsertArrayElementAtIndex(index);
        var element = dataProperty.GetArrayElementAtIndex(index);

        var nameStr = key.Substring(0, 1).ToLower() + key.Substring(1); // 首字母小写

        element.FindPropertyRelative("Key").stringValue = nameStr;
        element.FindPropertyRelative("Obj").objectReferenceValue = obj;
    }

    private string GetStrCopyBuffer(Dictionary<string, UICompData> allComps)
    {
        string scriptTxt = "//开始\n";
        foreach (var item in allComps)
        {
            if (item.Value.Comp != null)
            {
                var nameStr = item.Key.Substring(0, 1).ToLower() + item.Key.Substring(1); // 首字母小写
                // scriptTxt += $"    [SerializeField] [HideInInspector] private {item.Value.Comp.GetType().Name} {nameStr} =null;\n";
                scriptTxt += $"    [SerializeField] [HideInInspector] private {item.Value.Comp.GetType().Name} {nameStr} = null;\n";
            }
            else
            {
                var nameStr = item.Key.Substring(0, 1).ToLower() + item.Key.Substring(1); // 首字母小写
                scriptTxt += $"    [SerializeField] [HideInInspector] private GameObject {nameStr} = null;\n";
            }
        }

        scriptTxt += "//结束";
        EditorGUIUtility.systemCopyBuffer = scriptTxt;
        return scriptTxt;
    }


    private string tmpTxtCtrl = @": BaseController
{
";
}