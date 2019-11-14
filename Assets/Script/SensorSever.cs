using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using MQTTnet;
using MQTTnet.Client;
using UniRx;

public class SensorSever : MonoBehaviour {
    private float deltatime = 0;
    [SerializeField]
    GameObject target;
    [SerializeField]
    Text dataText;
    [SerializeField]
    InputField address; //url
    [SerializeField]
    Text consoleText;
    private string serverurl = "http://localhost:3000/";
    //IMqttClient mqttClient;
    [SerializeField]
    private MqttController mqttController;

    void Start () {
        address.text = serverurl;
        StartCoroutine (ServerTest ());
        StartCoroutine (GetSensorData ());

        //StartCoroutine (PostServerData ());
        //MqttTest();
        mqttController.OnMessageReceived.Subscribe (message =>
        {
            Debug.Log(message);
            //データ成型
        });
    }

    // Update is called once per frame
    void Update () {
        deltatime += Time.deltaTime;
        if (deltatime > 1.0f) {
            StartCoroutine (GetSensorData ());
            
            deltatime -= 1.0f;
        }
    }
    //SensorData取得通信
    IEnumerator GetSensorData () {
        UnityWebRequest req = UnityWebRequest.Get (serverurl + "sensorData");
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
        //gx -1-1を想定
        float GX = ((data.gx + 1.0f) * 180) - 90.0f;
        //gy -1-1を想定
        float GY = ((data.gy + 1.0f) * 180);
        //gz -1-1を想定
        float GZ = ((data.gz + 1.0f) * 180);

        //Debug.Log ($"{data.ax:F3}");
        target.transform.rotation = Quaternion.Euler (90, 180, 0) * (new Quaternion (-data.gx, -data.gy, data.gz, 0));;
        string datastr = "AX:" + $"{data.ax:F3}" + " AY:" + $"{data.ay:F3}" + " AZ:" + $"{data.az:F3}" + "\n";
        datastr += "LX:" + $"{data.lx:F3}" + " LY:" + $"{data.ly:F3}" + " LZ:" + $"{data.lz:F3}" + "\n";
        datastr += "GX:" + $"{data.gx:F3}" + " GY:" + $"{data.gy:F3}" + " GZ:" + $"{data.gz:F3}" + "\n";
        dataText.text = datastr;
    }
    //Post通信テスト
    IEnumerator PostServerData () {
        WWWForm form = new WWWForm ();
        //key:data
        form.AddField ("myField", "myData");
        form.AddField ("newxt", 20);

        using (UnityWebRequest req = UnityWebRequest.Post (serverurl + "Post", form)) {
            yield return req.SendWebRequest ();

            if (req.isNetworkError) {
                Debug.Log (req.error);
            } else {
                Debug.Log ("Form upload complete!");
            }
        }
    }
    IEnumerator ServerTest () {
        consoleText.text = "No Server";
        UnityWebRequest req = UnityWebRequest.Get (serverurl + "tests");
        req.SetRequestHeader ("key", "KEY");
        req.timeout = 10;
        yield return req.SendWebRequest ();
        if (req.isNetworkError) {
            Debug.Log (req.error);
            consoleText.text = req.error;
        } else {
            if (req.responseCode == 200) {
                //OK
            }
        }
        consoleText.text = "responceCode:" + req.responseCode.ToString ();
    }
    public void OnClickSetServer () {
        serverurl = address.text;
        StartCoroutine (ServerTest ());
    }
    /*
    async void MqttTest(){
        Debug.Log("start");
        
        MqttClient client = new MqttClient("192.168.1.4", 1883 , false , null);
            client.MqttMsgPublishReceived += (sender, eventArgs) =>
                {
                    var msg = Encoding.UTF8.GetString(eventArgs.Message);
                    var topic = eventArgs.Topic;
                    Debug.Log(topic + ", " + msg);
                };
            var ret = client.Connect(Guid.NewGuid().ToString());
            Console.WriteLine("Connected with result code {0}", ret);
            client.Subscribe(new[] { "itoyuNineAxis" }, new[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            while (client.IsConnected)
            {
            }
            
            var factory = new MqttFactory ();
        mqttClient = factory.CreateMqttClient ();

        var options = new MqttClientOptionsBuilder ()
            .WithTcpServer ("192.168.1.4",  1883)
            .WithClientId ("Unity.client")//Guid.NewGuid ().ToString ())
            .WithCredentials ("your_MQTT_username", "your_MQTT_password")
            .WithTls ()
            .Build ();

        mqttClient.Connected += async (s, e) =>
        {
            Debug.Log ("MQTTブローカに接続しました");
            await mqttClient.SubscribeAsync (
                new TopicFilterBuilder ()
                .WithTopic ("itoyuNineAxis")
                .Build ());
            Debug.Log ("指定したトピックをSubscribeしました");
        };
        mqttClient.Disconnected += async (s, e) =>
        {
            if (e.Exception == null)
            {
                Debug.Log ("サーバとの通信を切断しました");
                return;
            }

            Debug.Log ("サーバから切断されました。5秒後に再接続します");
        };
        mqttClient.ApplicationMessageReceived += (s, e) =>
        {
            var message = System.Text.Encoding.UTF8.GetString (e.ApplicationMessage.Payload);
            Debug.Log ($"メッセージ受信 : {message}");
            
        };

        await mqttClient.ConnectAsync (options);


    }
    */
}