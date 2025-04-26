using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#region << 脚 本 注 释 >>

//作  用:    UIForm
//作  者:    曾思信
//创建时间:  #CREATETIME#

#endregion

public class UIForm : MonoBehaviour,ISerializationCallbackReceiver
{
    //用于序列化的List
    public List<UICompData> Data = new();
    
    //Object并非C#基础中的Object，而是 UnityEngine.Object
    private readonly Dictionary<string, UICompData> m_Dict = new Dictionary<string, UICompData>();

    public Dictionary<string, UICompData> GetAllComps()
    {
        return m_Dict;
    }
    public List<UICompData> GetData()
    {
        return Data;
    }

    public void OnBeforeSerialize()
    {

    }

    public void OnAfterDeserialize()
    {
        m_Dict.Clear();
        foreach (UICompData referenceCollectorData in Data)
        {
            if (!m_Dict.ContainsKey(referenceCollectorData.Key))
            {
                m_Dict.Add(referenceCollectorData.Key, referenceCollectorData);
            }
        }
    }
}

[Serializable]
public class UICompData
{
    public int Index = 0;
    public string Key;
    public string[] Components;
    public UnityEngine.Object Obj;    // UnityEngine.Object
    public UnityEngine.Component Comp;
}


// public void Add(string key, Object obj)
//  {
//      SerializedObject serializedObject = new(this);
//
//      SerializedProperty dataProperty = serializedObject.FindProperty("data");
//      int i;
//      for (i = 0; i < Data.Count; i++)
//      {
//          if (Data[i].Key == key)
//          {
//              break;
//          }
//      }
//      if (i != Data.Count)
//      {
//          SerializedProperty element = dataProperty.GetArrayElementAtIndex(i);
//          element.FindPropertyRelative("gameObject").objectReferenceValue = obj;
//      }
//      else
//      {
//          //等于则说明key在data中无对应元素，所以得向其插入新的元素
//          dataProperty.InsertArrayElementAtIndex(i);
//          SerializedProperty element = dataProperty.GetArrayElementAtIndex(i);
//          element.FindPropertyRelative("key").stringValue = key;
//          element.FindPropertyRelative("gameObject").objectReferenceValue = obj;
//      }
//      //应用与更新
//      EditorUtility.SetDirty(this);
//      serializedObject.ApplyModifiedProperties();
//      serializedObject.UpdateIfRequiredOrScript();
//  }
//  //删除元素，知识点与上面的添加相似
//  public void Remove(string key)
//  {
//      SerializedObject serializedObject = new SerializedObject(this);
//      SerializedProperty dataProperty = serializedObject.FindProperty("Data");
//      int i;
//      for (i = 0; i < Data.Count; i++)
//      {
//          if (Data[i].Key == key)
//          {
//              break;
//          }
//      }
//      if (i != Data.Count)
//      {
//          dataProperty.DeleteArrayElementAtIndex(i);
//      }
//      EditorUtility.SetDirty(this);
//      serializedObject.ApplyModifiedProperties();
//      serializedObject.UpdateIfRequiredOrScript();
//  }

