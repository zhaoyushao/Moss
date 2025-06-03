using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UI_ctrl : MonoBehaviour
{
    public Dictionary<string, GameObject> view = new Dictionary<string, GameObject>();

    private void load_all_object(GameObject root, string path)
    {
        foreach (Transform tf in root.transform)
        {
            if (this.view.ContainsKey(path + tf.gameObject.name))
            {
                // Debugger.LogWarning("Warning object is exist:" + path + tf.gameObject.name + "!");
                continue;
            }
            this.view.Add(path + tf.gameObject.name, tf.gameObject);
            load_all_object(tf.gameObject, path + tf.gameObject.name + "/");
        }

    }

    public virtual void Awake()
    {
        this.load_all_object(this.gameObject, "");
    }


    public void add_button_listener(string view_name, UnityAction onclick)
    {
        Button bt = this.view[view_name].GetComponent<Button>();
        if (bt == null)
        {
            Debug.LogWarning("UI_manager add_button_listener: not Button Component!");
            return;
        }

        bt.onClick.AddListener(onclick);
    }

}
public class UIMgr : UnitySingleton<UIMgr>
{
    private Transform canvas = null;
    public override void Awake() {
        base.Awake();

        //this.canvas = GameObject.Find("Canvas").transform;
    }

    
    public UI_ctrl ShowUIView(string name)
    {
        string path = "MyAsset/Prefabs/" + name + ".prefab";
        GameObject ui_prefab = (GameObject)ResMgr.Instance.GetAssetCache<GameObject>(path);
        GameObject ui_view = GameObject.Instantiate(ui_prefab);
        ui_view.name = ui_prefab.name;
        ui_view.transform.SetParent(this.canvas, false);

        int lastIndex = name.LastIndexOf("/");
        if (lastIndex > 0)
        {
            name = name.Substring(lastIndex + 1);
        }

        Type type = Type.GetType(name + "_UICtrl");
        UI_ctrl ctrl = (UI_ctrl)ui_view.AddComponent(type);

        return ctrl;
    }
}
