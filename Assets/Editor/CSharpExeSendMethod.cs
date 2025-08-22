using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;

public class CSharpExeSendMethod : EditorWindow
{
    private const string FILE_PATH_KEY = "CSharpExecute_FilePath";
    private string filePath = @"D:/git_Project/UIPanelField/Assets/Scripts/SLGKingCtrl.cs";
    private string selectedFileName = "";
    private string namespaceName = "";
    private string className = "";
    private List<MethodInfo> sendMethods = new List<MethodInfo>();
    private int selectedMethodIndex = 0;
    private List<object[]> methodParameters = new List<object[]>();
    private Vector2 scrollPosition;

    [MenuItem("Tools/执行C#文件_Send_**")]
    public static void ShowWindow()
    {
        GetWindow<CSharpExeSendMethod>("执行_Send方法");
    }

    void OnEnable()
    {
        // 从 EditorPrefs 读取保存的文件路径
        if (EditorPrefs.HasKey(FILE_PATH_KEY))
        {
            filePath = EditorPrefs.GetString(FILE_PATH_KEY);
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                selectedFileName = Path.GetFileName(filePath);
                ParseCSharpFile(filePath);
            }
        }
    }

    void OnDisable()
    {
        // 在窗口关闭时保存文件路径到 EditorPrefs
        if (!string.IsNullOrEmpty(filePath))
        {
            EditorPrefs.SetString(FILE_PATH_KEY, filePath);
        }
    }

    void OnGUI()
    {
        // 快速打开按钮
        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            EditorGUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("在编辑器中打开文件"))
            {
                OpenFileInEditor(filePath);
            }

            if (GUILayout.Button("在资源管理器中显示"))
            {
                EditorUtility.RevealInFinder(filePath);
            }

            if (GUILayout.Button("重新解析文件"))
            {
                ParseCSharpFile(filePath);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        GUILayout.Label("C# 执行Send_**的静态方法工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 路径输入区域
        EditorGUILayout.BeginHorizontal();
        filePath = EditorGUILayout.TextField("文件路径", filePath);

        if (GUILayout.Button("浏览", GUILayout.Width(60)))
        {
            string selectedPath = EditorUtility.OpenFilePanel("选择 C# 文件", Application.dataPath, "cs");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                filePath = selectedPath;
                selectedFileName = Path.GetFileName(selectedPath);
                ParseCSharpFile(filePath);

                // 立即保存路径到 EditorPrefs
                EditorPrefs.SetString(FILE_PATH_KEY, filePath);
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal("Box");
        // 显示选中的文件名、命名空间和类名
        if (!string.IsNullOrEmpty(selectedFileName))
        {
            EditorGUILayout.LabelField($"选中的文件:   {selectedFileName}");
        }

        if (!string.IsNullOrEmpty(namespaceName))
        {
            EditorGUILayout.LabelField($"命名空间:   {namespaceName}");
        }

        if (!string.IsNullOrEmpty(className))
        {
            EditorGUILayout.LabelField($"类名:   {className}");
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 显示Send_方法下拉框
        if (sendMethods.Count > 0)
        {
            EditorGUILayout.LabelField("Send_静态方法:", EditorStyles.boldLabel);

            selectedMethodIndex = EditorGUILayout.Popup("选择方法", selectedMethodIndex,
                sendMethods.Select(m => m.Name).ToArray());

            // 显示方法参数输入
            DisplayMethodParameters(selectedMethodIndex);

            // 执行按钮
            if (GUILayout.Button("执行方法", GUILayout.Height(30)))
            {
                ExecuteSelectedMethod(selectedMethodIndex);
            }
        }
        else if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            EditorGUILayout.HelpBox("未找到以'Send_'开头的静态方法", MessageType.Info);
        }

        // 验证路径有效性
        if (!string.IsNullOrEmpty(filePath) && !File.Exists(filePath))
        {
            EditorGUILayout.HelpBox("文件不存在，请检查路径是否正确。", MessageType.Warning);
        }
    }

    private void DisplayMethodParameters(int methodIndex)
    {
        if (methodIndex < 0 || methodIndex >= sendMethods.Count) return;

        var method = sendMethods[methodIndex];
        var parameters = method.GetParameters();

        // 确保参数列表足够大
        while (methodParameters.Count <= methodIndex)
        {
            methodParameters.Add(new object[parameters.Length]);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("方法参数:", EditorStyles.boldLabel);

        for (int i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"{parameter.Name} ({parameter.ParameterType.Name})",
                GUILayout.Width(150));

            // 根据参数类型显示不同的输入控件
            methodParameters[methodIndex][i] = DisplayParameterInput(
                parameter.ParameterType,
                methodParameters[methodIndex][i],
                parameter.Name);

            EditorGUILayout.EndHorizontal();
        }
    }

    private object DisplayParameterInput(Type parameterType, object currentValue, string parameterName)
    {
        if (parameterType == typeof(string))
        {
            return EditorGUILayout.TextField(currentValue as string ?? "");
        }
        else if (parameterType == typeof(int))
        {
            return EditorGUILayout.IntField((int)(currentValue ?? 0));
        }
        else if (parameterType == typeof(float))
        {
            return EditorGUILayout.FloatField((float)(currentValue ?? 0f));
        }
        else if (parameterType == typeof(double))
        {
            return EditorGUILayout.DoubleField((double)(currentValue ?? 0));
        }
        else if (parameterType == typeof(bool))
        {
            return EditorGUILayout.Toggle((bool)(currentValue ?? false));
        }
        else if (parameterType == typeof(Vector2))
        {
            return EditorGUILayout.Vector2Field("", (Vector2)(currentValue ?? Vector2.zero));
        }
        else if (parameterType == typeof(Vector3))
        {
            return EditorGUILayout.Vector3Field("", (Vector3)(currentValue ?? Vector3.zero));
        }
        else if (parameterType.IsEnum)
        {
            return EditorGUILayout.EnumPopup((Enum)(currentValue ?? Enum.GetValues(parameterType).GetValue(0)));
        }
        else
        {
            EditorGUILayout.LabelField($"不支持的类型: {parameterType.Name}");
            return currentValue;
        }
    }

    private void ExecuteSelectedMethod(int methodIndex)
    {
        if (methodIndex < 0 || methodIndex >= sendMethods.Count) return;

        try
        {
            var method = sendMethods[methodIndex];
            var parameters = methodParameters[methodIndex];

            // 调用静态方法
            object result = method.Invoke(null, parameters);

            if (method.ReturnType != typeof(void))
            {
                ShowMsg($"执行成功 方法 {method.Name} 执行成功！\n返回值: {result}");
            }
            else
            {
                ShowMsg($"执行成功 方法 {method.Name} 执行完成！");
            }
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("执行错误",
                $"执行方法时出错: {ex.InnerException?.Message ?? ex.Message}", "确定");
            Debug.LogError($"方法执行错误: {ex}");
        }
    }

    public void ShowMsg(string content)
    {
        ShowNotification(new GUIContent(content), 2f);
    }

    private void ParseCSharpFile(string path)
    {
        namespaceName = "";
        className = "";
        sendMethods.Clear();
        methodParameters.Clear();
        selectedMethodIndex = 0;

        try
        {
            // 读取文件内容
            string fileContent = File.ReadAllText(path);

            // 解析命名空间和类名
            ParseNamespaceAndClass(fileContent);

            // 使用反射查找指定文件中的方法
            FindSendMethodsInFile();
        }
        catch (Exception ex)
        {
            Debug.LogError($"解析文件失败: {ex.Message}");
            EditorUtility.DisplayDialog("错误", $"解析文件失败: {ex.Message}", "确定");
        }
    }

    private void ParseNamespaceAndClass(string fileContent)
    {
        // 解析命名空间
        int namespaceIndex = fileContent.IndexOf("namespace ");
        if (namespaceIndex != -1)
        {
            int start = namespaceIndex + "namespace ".Length;
            int end = fileContent.IndexOfAny(new char[] { '{', '\n', '\r' }, start);
            if (end != -1)
            {
                namespaceName = fileContent.Substring(start, end - start).Trim();
            }
        }

        // 解析类名 - 查找第一个公共类
        Match classMatch = Regex.Match(fileContent, @"class\s+(\w+)");
        if (classMatch.Success)
        {
            className = classMatch.Groups[1].Value;
        }
    }

    private void FindSendMethodsInFile()
    {
        try
        {
            // 获取当前加载的所有程序集
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // 查找包含指定命名空间和类名的类型
            Type targetType = null;

            foreach (var assembly in assemblies)
            {
                try
                {
                    // 查找匹配命名空间和类名的类型
                    var types = assembly.GetTypes()
                        .Where(t => t.Namespace == namespaceName && t.Name == className)
                        .ToList();

                    if (types.Count > 0)
                    {
                        targetType = types[0];
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"检查程序集 {assembly.FullName} 时出错: {ex.Message}");
                }
            }

            if (targetType != null)
            {
                // 查找该类型中以Send_开头的静态公共方法
                var methods = targetType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.Name.StartsWith("Send_"))
                    .ToList();

                sendMethods.AddRange(methods);

                // 初始化参数列表
                for (int i = 0; i < sendMethods.Count; i++)
                {
                    var parameters = sendMethods[i].GetParameters();
                    methodParameters.Add(new object[parameters.Length]);

                    // 设置默认值
                    for (int j = 0; j < parameters.Length; j++)
                    {
                        methodParameters[i][j] = GetDefaultValue(parameters[j].ParameterType);
                    }
                }
            }
            else
            {
                Debug.LogWarning($"未找到类型: {namespaceName}.{className}，请确保文件已编译");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"查找方法失败: {ex.Message}");
        }
    }

    private object GetDefaultValue(Type type)
    {
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }

        return null;
    }

    private void OpenFileInEditor(string path)
    {
        bool success = UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(path, 1);

        if (!success)
        {
            Debug.LogError($"无法打开文件: {path}");
            EditorUtility.DisplayDialog("错误", "无法打开文件，可能没有关联的编辑器。", "确定");
        }
    }

    // 拖拽功能
    private void OnDragUpdated()
    {
        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
    }

    private void OnDragPerform()
    {
        foreach (string draggedPath in DragAndDrop.paths)
        {
            if (draggedPath.EndsWith(".cs"))
            {
                filePath = draggedPath;
                selectedFileName = Path.GetFileName(draggedPath);
                ParseCSharpFile(filePath);

                // 保存路径到 EditorPrefs
                EditorPrefs.SetString(FILE_PATH_KEY, filePath);

                Repaint();
                break;
            }
        }

        DragAndDrop.AcceptDrag();
    }
}