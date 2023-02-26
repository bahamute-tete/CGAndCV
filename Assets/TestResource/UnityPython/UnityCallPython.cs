using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

using System.IO;
using MathNet.Numerics.LinearAlgebra;

public class UnityCallPython : MonoBehaviour
{

    private string[] argvs = new string[2] {"10", "20"};

    private float a = 0;
    string b = null;
    int c = 0;

    string path = @"D:\Python\Test.py";

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{

        //    //CallPythonCode(argvs);
        //    PythonRunner.RunFile(path);

        //}
    }

    void CallPythonCode(string[] argvs)
    {

        
        Process p = new Process();
        //MAC
        //string path = @"/Users/xuchengqi/UnityProject/UnityProj/CGAndCV/Assets/TestResource/UnityPython/python4Unity.py" + " "+argvs[0] + " " + argvs[1];

        //Windows
        //string path = @"D:\UnityProject\GitHub\CGAndCV\Assets\TestResource\UnityPython\PCA.py";
        string path = @"D:\Python\Test.py";

        //MAC
        // p.StartInfo.FileName = @"/Users/xuchengqi/opt/anaconda3/bin/pythonw";

        //windows 
        p.StartInfo.FileName = @"D:\Soft\anaconda3\python.exe";
   


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

        UnityEngine.Debug.Log("1111");


    }

    private void  GetData(object sender, DataReceivedEventArgs eventArgs)
    {
       
        if (!string.IsNullOrEmpty(eventArgs.Data))
        {
            UnityEngine.Debug.Log(eventArgs.Data);
        }

    }
}
