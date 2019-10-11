﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SensorSever : MonoBehaviour {
    private float deltatime = 0;
    [SerializeField]
    GameObject target;
    [SerializeField]
    Text dataText;
    void Start () {
        StartCoroutine (GetSensorData ());

        //StartCoroutine (PostServerData ());
    }

    // Update is called once per frame
    void Update () {
        deltatime += Time.deltaTime;
        if (deltatime > 3.0f) {
            StartCoroutine (GetSensorData ());
            deltatime -= 3.0f;
        }
    }
    //SensorData取得通信
    IEnumerator GetSensorData () {
        UnityWebRequest req = UnityWebRequest.Get ("http://localhost:3000/sensorData");
        req.SetRequestHeader ("key", "KEY");
        yield return req.SendWebRequest ();
        if (req.isNetworkError) {
            Debug.Log (req.error);
        } else {
            if (req.responseCode == 200) {
                //OK
                string jsonText = req.downloadHandler.text;
                //Debug.Log (jsonText);
                List<NineAxis> list = new List<NineAxis> ();
                list = JsonUtility.FromJson<Serialize<NineAxis>> (jsonText).ToList ();
                //Debug.Log (list[0].datetime);
                setTarget (list[0]);
            }
        }
    }
    //9軸オブジェクトへ適用
    void setTarget (NineAxis data) {
        //ax 0-10を想定
        float AX = (data.ax * 18) - 90.0f;
        //ay 0-10を想定
        float AY = (data.ay * 18) + 90.0f;
        //az 0-10を想定
        float AZ = -((data.az * 18) - 90.0f);
        Debug.Log (data.ax + " : " + AX);
        Debug.Log (data.ay + " : " + AY);
        Debug.Log (data.az + " : " + AZ);
        Debug.Log ($"{data.ax:F3}");
        target.transform.rotation = Quaternion.Euler (AX, AY, AZ);
        string datastr = "AX:" + $"{data.ax:F3}" + " AY:" + $"{data.ay:F3}" + " AZ:" + $"{data.az:F3}" + "\n";
        datastr += "LX:" + $"{data.Lx:F3}" + " LY:" + $"{data.Ly:F3}" + " LZ:" + $"{data.Lz:F3}" + "\n";
        datastr += "GX:" + $"{data.gx:F3}" + " GY:" + $"{data.gy:F3}" + " GZ:" + $"{data.gz:F3}" + "\n";
        dataText.text = datastr;
    }
    //Post通信テスト
    IEnumerator PostServerData () {
        WWWForm form = new WWWForm ();
        //key:data
        form.AddField ("myField", "myData");
        form.AddField ("newxt", 20);

        using (UnityWebRequest req = UnityWebRequest.Post ("http://localhost:3000/Post", form)) {
            yield return req.SendWebRequest ();

            if (req.isNetworkError) {
                Debug.Log (req.error);
            } else {
                Debug.Log ("Form upload complete!");
            }
        }
    }
}