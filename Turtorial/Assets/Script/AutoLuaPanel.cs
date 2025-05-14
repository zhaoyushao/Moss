using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class AutoLuaPanel : MonoBehaviour
{
    private static string COMPONENTS_WARNING = "//-------自动生成，请勿手动修改！！！--------";
    private static string COMPONENTS_START = "//----------COMPONENTS BEGIN----------";
    private static string COMPONENTS_END = "//-----------COMPONENTS END-----------";
    static string str = string.Empty;
    static StringBuilder code;
    static Regex ListNameRegex = new Regex(@"^(\w*[^\d])(\d+)$");
    static Regex SubPageRegex = new Regex(@"^(\w+SubPage)(\d*)$");

    [MenuItem("Assets/生成UI代码")]
    static void ExportPackage()
    {
        CreateUIPanelLua();
    }
    
    [MenuItem("Assets/生成UI代码_移动")]
    static void ExportPackage_Mobile()
    {
        CreateUIPanelLua(true);
    }
    
    [MenuItem("Assets/生成UI代码_PC")]
    static void ExportPackage_PC()
    {
        CreateUIPanelLua(false, true);
    }
    
    //[MenuItem("ClientTools/GenUICode")]
    static void CreateUIPanelLua(bool isMoble = false, bool isStandAlone = false)
    {
        if (Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.TopLevel)[0].GetType() != typeof(UnityEngine.GameObject))
        {
            Debug.LogError("选中的资源类型为 == " + Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.TopLevel)[0].GetType() + "   请检查");
            return;
        }
        UnityEngine.Transform[] arr = Selection.GetTransforms(SelectionMode.TopLevel);
        Transform form = arr[0] as Transform;
        
        
        bool isView = form.name.EndsWith("Panel+");
        bool isPanel = form.name.EndsWith("Panel");

        if (!isPanel && !isView)
        {
            //Debug.LogError("prefab不包含panel不能导出lua脚本");
            //return;
        }

        string panelname = form.name.Substring(0, form.name.Length - 5);
        str = form.name + "/";
        string strC = string.Format("\"{0}\"", form.name);
        code = new StringBuilder();

        List<string> codes = new List<string>();
        string luaPath = Application.dataPath + "/Script/Nurse/UI/AutoGenerateCode";
        
        var map = new Dictionary<string, List<int>>();
        var titles = new Dictionary<string, string>();
        var list = new List<string>();
        FindAllObj(false, form, form.name, "    this.", map, titles, list, code);


        string scriptName = luaPath + "/" + form.name + "+.cs";
        bool isInitAwake = false;
        bool isInitStart = false;
        if (File.Exists(scriptName))
        {
            File.Delete(scriptName);
        }

        // 写入Component注释段
        if (isMoble)
        {
            code.Append("#if UNITY_ANDROID || UNITY_IOS\n");
        }
        else if(isStandAlone)
        {
            code.Append("#if UNITY_STANDALONE\n");
        }
        
        code.Append(COMPONENTS_WARNING);
        code.Append("\n");
        code.Append(COMPONENTS_START);
        code.Append("\n");
        code.Append("using System.Collections.Generic;\n");
        code.Append("using LuaFramework;\n");
        code.Append("using UnityEngine;\n");
        code.Append("using UnityEngine.UI;\n");
        code.Append("using SuperScrollView;\n");
        /*if (isView)
            code.Append("local ");*/
        code.Append("\n");
        code.Append("public partial class " + form.name + " : PanelBehaviour");
        code.Append("\n");
        code.Append("{");
        code.Append("\n");
        foreach (var VARIABLE in titles)
        {
            code.Append("    public " + VARIABLE.Value + " " + VARIABLE.Key + ";\n");
        }
        
        foreach(var entry in map)
        {
            entry.Value.Sort();
            if (!IsSortList(entry.Value)) continue;

            var strs = entry.Key.Split(',');
            code.Append("    public List<" + strs[1] + "> " + strs[0] + "s;\n");
        }
        
        //Awake
        //code.Append("function " + form.name + ":Awake()\n");
        //code.Append("    self:InitAwake()\n");
        //code.Append("    self:InitPanel()\n");
        //code.Append("end\n");
        //code.Append("\n");

        //Start
        //code.Append("function " + form.name + ":Start()\n");
        //code.Append("end\n");
        //code.Append("\n");

        //InitPanel
        code.Append("\n");
        code.Append("    public override void InitUI()\n    {\n");
        
        
        foreach (var VARIABLE in list)
        {
            code.Append(VARIABLE);
        }
        ProgressListName(map, "    this.", code);
        code.Append("    }");
        code.Append("\n}");
        code.Append(COMPONENTS_END);

        if (isMoble || isStandAlone)
        {
            code.Append("\n");
            code.Append("#endif");
        }


        var filepath = luaPath + "/" + "/A_" + form.name + "+.cs";
        if (isMoble)
        {
            filepath = luaPath + "/" + "/M_" + form.name + "+.cs";
        }
        if (File.Exists(filepath))
        {
            File.Delete(filepath);
        }

        FileStream fs = null;
        if (isMoble)
        {
            fs = new FileStream(luaPath + "/" + "/M_" + form.name + "+.cs", FileMode.OpenOrCreate, FileAccess.Write);
        }
        else
        {
            fs = new FileStream(luaPath + "/" + "/A_" + form.name + "+.cs", FileMode.OpenOrCreate, FileAccess.Write);
        }
        UTF8Encoding utf8 = new UTF8Encoding(false);
        StreamWriter sw = new StreamWriter(fs, utf8);
        sw.Write(code.ToString());
        /*if (!isInitAwake)
        {
            sw.WriteLine("\nfunction " + form.name + ":Awake()");
            sw.WriteLine("    self:InitPanel()");
            sw.WriteLine("end");
        }
        if (!isInitStart)
        {
            sw.WriteLine("\nfunction " + form.name + ":Start()");
            sw.WriteLine("end");
        }
        
        foreach (string cd in codes)
        {
            sw.WriteLine(cd);
        }


        if (isView)
        {
            // View需要return
            bool hasReturn = false;
            for (var i = codes.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrWhiteSpace(codes[i])) continue;
                hasReturn = codes[i].StartsWith("return ");
                break;
            }
            if(!hasReturn)
            {
                sw.WriteLine($"return {form.name}");
            }
        }*/

        sw.Close();
        fs.Close();

        //TryCreateCtrlFile(form.name, $"{Application.dataPath}/Game/UI/Controller", isView);
        TryCreateCtrlFile(panelname, $"{Application.dataPath}/Script/Nurse/UI/", isView);
        
        //GeneratePanelDef(form.name);
        
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
    
    static void GeneratePanelDef(string Name)
        {
            Name = Name.Substring(0, Name.Length - "Panel".Length);
            string rootPath = Application.dataPath.Replace('\\', '/');
            rootPath = rootPath.Substring(0, rootPath.LastIndexOf('/'));
    
            string resPath = rootPath + "/Assets/LuaFramework/Lua/Logic/PanelDef.lua";
            string savePath = rootPath + "/Assets/LuaFramework/Lua/Logic/PanelDef.lua";
            
            bool bFind = false;
            int index = 0;
            string newData = "ControllerNames = {";
            foreach (string str in System.IO.File.ReadAllLines(resPath, Encoding.UTF8))
            {
                if (str.Contains("\"") && str.Contains(","))
                {
                    if (str.IndexOf(Name) != -1)
                    {
                        bFind = true;
                        break;
                    }
                    
                    newData += "\n" + str;
                }
            }
            
            if(bFind == true)
                return;
            
            newData += "\n \t\"" + Name + "\",";
            newData += "\n }";
            
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }
    
            //newData = "//此文件为工具生成,需要增加新事件添加到GameEvent.lua中\n" + "public class GameEvent {" + newData + "\n}";
            File.WriteAllText(savePath, newData);
            AssetDatabase.Refresh();
            //UnityEngine.Debug.Log("事件文件生成完毕");
        }

    private static void TryCreateCtrlFile(string name, string dirName, bool isView)
    {
        string path;
        string viewPathCode = "";
        string baseCls;
        string dir;
        /*if (isView)
        {
            path = $"{dirName}/ViewCtrl/{name}.lua";
            viewPathCode = $"{name}.__panelviewpath = \"View/ViewPanel/{name}\"";
            baseCls = "BaseView";
        }
        else*/
        {
            dir = dirName + name;
            path = $"{dir}/{name}Panel.cs";
        }
        
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        if (File.Exists(path)) return;
        
        var code = new StringBuilder();
        code.Append("using LuaFramework;\n");
        code.Append("using UnityEngine.UI;\n");

        code.Append("\n");
        code.Append($"//{name}\n");
        code.Append($"public partial class {name}Panel : PanelBehaviour");
        code.Append("\n");
        code.Append("{");
        code.Append("\n");
        code.Append("   //界面初始化\n");
        code.Append("   public override void Init()\n");
        code.Append("   {\n");
        code.Append("       //Util.AddClick(this.m_CloseBtn, this.CloseClick);");
        code.Append("\n");
        code.Append("   }");
        
        code.Append("\n\n");
        code.Append("   //处理参数\n");
        code.Append("   public override void HandleParams(UIParams param)\n");
        code.Append("   {");
        code.Append("\n");
        code.Append("   }");
        
        code.Append("\n\n");
        code.Append("   //界面关闭\n");
        code.Append("   public override void Close()\n");
        code.Append("   {");
        code.Append("\n");
        code.Append("   }");
        
        code.Append("\n\n");
        code.Append("   //注册事件\n");
        code.Append("   public override void RegistListener()\n");
        code.Append("   {");
        code.Append("\n");
        code.Append("       //e.p\n");
        code.Append("       //this.AddEvent(GameEvent.UpdateEnergy, this.UpdateEnergy);");
        code.Append("\n");
        code.Append("   }");
        
        code.Append("\n\n");
        code.Append("   //Click\n");
        code.Append("   public void CloseClick(Button btn)\n");
        code.Append("   {");
        code.Append("\n");
        code.Append("       //e.p\n");
        code.Append("       CloseUI();");
        code.Append("\n");
        code.Append("   }");
        
        /*code.Append("\n");
        code.Append("   public override void RemoveListener()\n");
        code.Append("   {");
        code.Append("\n");
        code.Append("   }");*/
        
        code.Append("\n");
        code.Append("}");
        
        FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
        UTF8Encoding utf8 = new UTF8Encoding(false);
        StreamWriter sw = new StreamWriter(fs, utf8);
        sw.Write(code.ToString());
        sw.Close();
    }
    
    static void FindAllObj(bool isRoot, Transform form, string path, string prefix, Dictionary<string, List<int>> map , Dictionary<string, string> titles, List<string> list, StringBuilder parent)
    {
        foreach (Transform trans in form)
        {
            string nowPrefix = prefix;
            var nowMap = map;
            if (trans.name.Contains("m_") || trans.name.Contains("S_"))
            {
                string objPath = "";
                if (isRoot)
                {
                    //objPath = (trans.name).Replace(str, "");
                    objPath = (trans.name).Replace(str, "");
                    //objPath = (path + "/" + trans.name).Replace("path", "");
                }
                else
                {
                    objPath = (path + "/" + trans.name).Replace(str, "");
                }
                objPath = "\"" + objPath + "\"";
                string strGetC = "";
                string strType = "GameObject";
                //Debug.LogError(objPath);
                if (trans.gameObject.GetComponent<Image>())
                {
                    strGetC = ".GetComponent<Image>()";
                    strType = "Image";
                }
                if (trans.gameObject.GetComponent<Text>())
                {
                    strGetC = ".GetComponent<Text>()";
                    strType = "Text";
                }
                if (trans.gameObject.GetComponent<Button>())
                {
                    strGetC = ".GetComponent<Button>()";
                    strType = "Button";
                }
                if (trans.gameObject.GetComponent<Toggle>())
                {
                    strGetC = ".GetComponent<Toggle>()";
                    strType = "Toggle";
                }
                if (trans.gameObject.GetComponent<ToggleGroup>())
                {
                    strGetC = ".GetComponent<ToggleGroup>()";
                    strType = "ToggleGroup";
                }
                if (trans.gameObject.GetComponent<Slider>())
                {
                    strGetC = ".GetComponent<Slider>()";
                    strType = "Slider";
                }

                if (trans.gameObject.GetComponent<Scrollbar>())
                {
                    strGetC = ".GetComponent<Scrollbar>()";
                    strType = "Scrollbar";
                }
                if (trans.gameObject.GetComponent<ScrollRect>())
                {
                    strGetC = ".GetComponent<ScrollRect>()";
                    strType = "ScrollRect";
                }
                if (trans.gameObject.GetComponent<GridLayoutGroup>())
                {
                    strGetC = ".GetComponent<GridLayoutGroup>()";
                    strType = "GridLayoutGroup";
                }
                if (trans.gameObject.GetComponent<InputField>())
                {
                    strGetC = ".GetComponent<InputField>()";
                    strType = "InputField";
                }
                if (trans.gameObject.GetComponent<TMPro.TextMeshProUGUI>())
                {
                    strGetC = ".GetComponent<TextMeshProUGUI>()";
                    strType = "TextMeshProUGUI";
                }


                if (trans.name.Contains("S_"))
                {
                    strGetC = $".GetComponent<{trans.name}>()";
                    strType = trans.name;
                }

                TryAddListName(nowMap, trans.name, strType);

                if (strGetC == "")
                {
                    list.Add("    " + prefix + trans.name + " = this.transform.Find(" + objPath + ").gameObject;\n");
                    titles.Add(trans.name, "GameObject");
                }
                    
                else
                {
                    list.Add("    " + prefix + trans.name + " = this.transform.Find(" + objPath + ")" + strGetC + ";\n");
                    titles.Add(trans.name, strType);
                }
                    
                if (SubPageRegex.IsMatch(trans.name))
                {
                    // 子界面
                    nowPrefix += trans.name.Substring("m_".Length);
                    list.Add(nowPrefix + "= {}\n");
                    nowPrefix += ".";
                    nowMap = new Dictionary<string, List<int>>();
                }
                
                if (trans.name.Contains("S_"))
                {
                    if (trans != form)
                    {
                        GeneraSubPanel(trans);
                        continue;
                    }
                }
            }
            /*else if (trans.name.Contains("S_"))
            {
                if (trans != form)
                {
                    GeneraSubPanel(trans);
                    continue;
                }
            }*/
            if(isRoot)
                FindAllObj(false, trans, trans.name, nowPrefix, nowMap, titles , list, parent);
            else
            {
                FindAllObj(false, trans, path + "/" + trans.name, nowPrefix, nowMap, titles , list, parent);
            }
            if (nowMap != map)
                ProgressListName(nowMap, nowPrefix, parent);
        }
    }

    static void GeneraSubPanel(Transform form)
    {
        string luaPath = Application.dataPath + "/Script/Nurse/UI/AutoGenerateCode";
        
        var map = new Dictionary<string, List<int>>();
        var titles = new Dictionary<string, string>();
        var list = new List<string>();
        
        var subcode = new StringBuilder();
        FindAllObj(true, form, form.name, "    this.", map, titles, list, subcode);
        subcode.Append(COMPONENTS_WARNING);
        subcode.Append("\n");
        subcode.Append(COMPONENTS_START);
        subcode.Append("\n");
        subcode.Append("using System.Collections.Generic;\n");
        subcode.Append("using LuaFramework;\n");
        subcode.Append("using UnityEngine.UI;\n");
        subcode.Append("using UnityEngine;\n");
        /*if (isView)
            code.Append("local ");*/
        subcode.Append("\n");
        subcode.Append("public partial class " + form.name + " : MonoBehaviour");
        subcode.Append("\n");
        subcode.Append("{");
        subcode.Append("\n");
        foreach (var VARIABLE in titles)
        {
            subcode.Append("    public " + VARIABLE.Value + " " + VARIABLE.Key + ";\n");
        }
        
        foreach(var entry in map)
        {
            entry.Value.Sort();
            if (!IsSortList(entry.Value)) continue;

            var strs = entry.Key.Split(',');
            subcode.Append("    public List<" + strs[1] + "> " + strs[0] + "s;\n");
        }
        
        subcode.Append("\n");
        subcode.Append("    public void Awake()\n    {\n");

        foreach (var VARIABLE in list)
        {
            subcode.Append(VARIABLE);
        }
        ProgressListName(map, "    this.", subcode);
        subcode.Append("    }");
        subcode.Append("\n}");
        subcode.Append(COMPONENTS_END);
       

        FileStream fs = new FileStream(luaPath + "/" + "/" + form.name + "+.cs", FileMode.OpenOrCreate, FileAccess.Write);
        UTF8Encoding utf8 = new UTF8Encoding(false);
        StreamWriter sw = new StreamWriter(fs, utf8);
        sw.Write(subcode.ToString());

        sw.Close();
        fs.Close();

    }

    private static void TryAddListName(Dictionary<string, List<int>> map,string name, string compType)
    {
        var match = ListNameRegex.Match(name);
        if (!match.Success) return;

        var key = match.Groups[1].Value + "," + compType;
        var index = int.Parse(match.Groups[2].Value);
        List<int> list;
        if (!map.TryGetValue(key, out list))
        {
            list = new List<int>();
            map[key] = list;
        }
        list.Add(index);
        
    }

    private static void ProgressListName(Dictionary<string, List<int>> map, string prefix, StringBuilder parent)
    {
        string trimPrefix = prefix.Trim();
        foreach(var entry in map)
        {
            entry.Value.Sort();
            if (!IsSortList(entry.Value)) continue;

            var strs = entry.Key.Split(',');
            string des = "s = new List<" + strs[1] + ">(){";
            parent.Append("    " + prefix + strs[0] + des);
            foreach (var v in entry.Value)
            {
                parent.AppendFormat("{0}{1}{2}, ", trimPrefix, strs[0], v);
            }
            parent.Append("};\n");
        }
    }
    private static void ProgressListName000(Dictionary<string, List<int>> map, string prefix)
    {
        string trimPrefix = prefix.Trim();
        foreach(var entry in map)
        {
            entry.Value.Sort();
            if (!IsSortList(entry.Value)) continue;

            code.Append(prefix + entry.Key+ "s = {");
            foreach (var v in entry.Value)
            {
                code.AppendFormat("{0}{1}{2},", trimPrefix, entry.Key, v);
            }
            code.Append("}\n");
        }
    }
    private static bool IsSortList(List<int> list)
    {
        for(var i = 0;i < list.Count;i++)
        {
            if (list[i] != i + 1) return false;
        }
        return true;
    }
    
    
    //生成CMD
    //[MenuItem("ClientTools/GenProto")]
    static void GenCmdFile()
    {
        string rootPath = Application.dataPath.Replace('\\', '/');
        rootPath = rootPath.Substring(0, rootPath.LastIndexOf('/'));
    
        string resPath = rootPath + "/../ProtoTool/Proto/Login.proto";
        string savePath = rootPath + "/Assets/Script/Game/Network/CmdDef.cs";
        
        int index = 0;
        string newData = "//Auto Gen..\n\n";
        newData += "public enum Cmd \n{";
        foreach (string str in System.IO.File.ReadAllLines(resPath, Encoding.UTF8))
        {
            if (str.Contains("message"))
            {
                string sub = str.Substring(8);
                //Debug.LogError(sub);
                    
                newData += "\n    " + sub + ",";
            }
        }
        
        newData += "\n }";
            
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
    
        //newData = "//此文件为工具生成,需要增加新事件添加到GameEvent.lua中\n" + "public class GameEvent {" + newData + "\n}";
        File.WriteAllText(savePath, newData);
        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("Cmd生成完毕");
        
        RunPython();
    }
    
    static string protocPath = Application.dataPath + "/../../ProtoTool/genproto"; 
    //static string configPath = Application.dataPath + "/Res/Config/";
    
    static void RunPython()
    {
        RunPy(protocPath);
    }
   
    static void RunPy(string pyPath)
    {
        Process pro = new Process();

        FileInfo file = new FileInfo(pyPath);
        pro.StartInfo.WorkingDirectory = file.Directory.FullName;
        pro.StartInfo.FileName = pyPath;
        pro.StartInfo.CreateNoWindow = false;
        pro.Start();
        pro.WaitForExit();
    }
    
}
