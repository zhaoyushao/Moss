using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boot : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ShowStartPanel();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void ShowStartPanel()
    {
        var panel = UIMgr.Instance.ShowUIView("MossPanel");
        
    }
}
