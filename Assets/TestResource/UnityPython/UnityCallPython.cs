using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class UnityCallPython : MonoBehaviour
{

    public string[] argvs = new string[2] {"10", "20"};

    public float a = 0;
    public string b = null;
    public int c = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CallPythonCode(argvs);

            UnityEngine.Debug.Log(a*2);
        }
    }

    void CallPythonCode(string[] argvs)
    {
        Process p = new Process();

        string path = @"/Users/xuchengqi/UnityProject/UnityProj/CGAndCV/Assets/TestResource/UnityPython/python4Unity.py" + " "+argvs[0] + " " + argvs[1];

        

        p.StartInfo.FileName = @"/Users/xuchengqi/opt/anaconda3/bin/pythonw";

        

        p.StartInfo.UseShellExecute = false;
        p.StartInfo.Arguments = path;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.CreateNoWindow = true;


        p.Start();
        p.BeginOutputReadLine();
        p.OutputDataReceived += new DataReceivedEventHandler(GetData);
        p.WaitForExit();

       
       

    }

    private void  GetData(object sender, DataReceivedEventArgs eventArgs)
    {
       
        if (!string.IsNullOrEmpty(eventArgs.Data))
        {
            UnityEngine.Debug.Log(eventArgs.Data);
        }

        a = float.Parse(eventArgs.Data);

    }
}
