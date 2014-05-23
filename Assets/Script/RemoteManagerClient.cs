//--------------------------------------------------------------------------------
// Author	   : 진용규 & 오세민
// Date		   : Last Modify(2014-05-15)
// Copyright   : Hansung Univ. Robots in Education & Entertainment Lab.
//
// Description : TCP를 이용한 Remote Controller
//--------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Net.Sockets;
using System.Xml;
using System.Xml.Serialization;
using System.Text;

public class RemoteManagerClient : MonoBehaviour {

    // xml 변수
    public XmlDocument xmldoc1;
    public XmlDocument xmldoc2;
    public XmlElement xmlElement1;
    public XmlElement xmlElement2;

    // xml 경로 변수
    private string _folderPath;
    public string _filePath1;
    public string _filePath2;

    // 소켓 변수
    private Socket _sendSocket1;
    private Socket _sendSocket2;
    ArrayList connectionList1 = new ArrayList();
    ArrayList listenlist1;

    // 영상 변수
    Texture2D texture;
    byte[] cameraBytes = new byte[11520];
    byte[] colBytes = new byte[11520];
    Color32[] cols = new Color32[3840];
    int receiveLine = 0;
    int remainBytes = 0;

    // PlayRule 변수
    byte[] ruleBytes = new byte[512];
    ArrayList ruleBuffer = new ArrayList();

    bool _isOpened = false;
    bool _isCameraCheck = false;
    bool _isMessageOnly = false;

    private int _tempNum = 1;

    int mode;
    float cameraSpeed = 0.5f;

    // 종료 시 소켓 연결 해제
    void OnApplicationQuit()
    {
        ShutDown();
    }

    private void ShutDown()
    {
        Debug.Log("ShutDown");
        for (int i = 0; i < connectionList1.Count; i++)
        {
            Socket curSorket = connectionList1[i] as Socket;
            if (curSorket != null)
                curSorket.Close();
        }

        if (_sendSocket1 != null)
        {
            _sendSocket1.Close();
            _sendSocket1 = null;
        }

        if (_sendSocket2 != null)
        {
            _sendSocket2.Close();
            _sendSocket2 = null;
        }

        ruleBuffer.Clear();
        connectionList1.Clear();

        if (listenlist1 != null)
            listenlist1.Clear();

        _isOpened = false;
        receiveLine = 0;
    }

    // SiCi에 Message 전달 함수
	public void Send(string message)
	{
        byte[] checkData = EncodingString(message+"\n");
        _sendSocket2.Send(checkData);
	}
	
    void Awake()
    {
        // 백그라운드 동작 활성화
        Application.runInBackground = true;
        ShutDown();
        
        // Alpha value 고정
        for (int i = 0; i < 3840; i++)
            cols[i].a = 255;
        
        texture = new Texture2D(80, 48);
        transform.FindChild("Plane").renderer.material.mainTexture = texture;

        // Xml Data Load
        XmlLoad();
    }

    // Xml Load 함수
    private void XmlLoad()
    {
        // Folder, File Path 설정
        // Player Setting - Other Setting - Write Access = External 설정 필요
        // 안드로이드 기준으로 /root/Android/data/hansung.reel.remotecontroller/files
        _folderPath = Application.persistentDataPath;
        _filePath1 = Application.persistentDataPath + "/XmlLog.xml";
        _filePath2 = Application.persistentDataPath + "/XmlCount.xml";
        
        // 만일 폴더가 없을 경우에는 폴더 생성
        DirectoryInfo di = new DirectoryInfo(_folderPath);
        if (di.Exists == false)
            di.Create();

        // 파일이 없을 경우에는 파일 생성
        // 파일이 있을 경우에는 해당 xml 파일 로드
        FileInfo fi1 = new FileInfo(_filePath1);
        if (fi1.Exists == false)
        {
            xmldoc1 = new XmlDocument();
            xmlElement1 = xmldoc1.CreateElement("PlayRuleLog");
            xmldoc1.AppendChild(xmlElement1);
        }
        else
        {
            xmldoc1 = new XmlDocument();
            xmldoc1.Load(_filePath1);
            xmlElement1 = xmldoc1.DocumentElement;
        }

        FileInfo fi2 = new FileInfo(_filePath2);
        if (fi2.Exists == false)
        {
            xmldoc2 = new XmlDocument();
            xmlElement2 = xmldoc2.CreateElement("PlayRuleCount");
            xmldoc2.AppendChild(xmlElement2);
        }
        else
        {
            xmldoc2 = new XmlDocument();
            xmldoc2.Load(_filePath2);
            xmlElement2 = xmldoc2.DocumentElement;
        }
    }

    // Update is called once per frame
    void Update()
    {
        ReceiveMessage();
    }

    // 서버 열기 버튼 누르면 동작되는 함수
    private void OpenSocket()
    {
        mode = int.Parse(GameObject.Find("WebcamSelect").transform.FindChild("Label").GetComponent<UILabel>().text.Trim());
        
        // Message Only, Message + Webcam 모드 구분
        if (0 == mode)
            _isMessageOnly = true;
        else
            _isMessageOnly = false;
        
        // 소켓 오픈
        _sendSocket1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint iplocal1 = new IPEndPoint(IPAddress.Any, int.Parse(GameObject.Find("port").transform.FindChild("Label").GetComponent<UILabel>().text));
        _sendSocket1.Bind(iplocal1);
        _sendSocket1.Listen(100);
        _isOpened = true;
    }

    private void ReceiveMessage()
    {
        if (_isOpened)
            SocketWaiting();

        if (_isMessageOnly == true)
            MessageOnly();
        else
            MessageAndWebcam();
    }

    // 클라이언트 소켓의 접속을 확인하는 함수
    private void SocketWaiting()
    {
        listenlist1 = new ArrayList();
        listenlist1.Add(_sendSocket1);
        Socket.Select(listenlist1, null, null, 1000);

        for (int i = 0; i < listenlist1.Count; i++)
        {
            Debug.Log("cameraSocket Accept");
            _sendSocket2 = ((Socket)listenlist1[i]).Accept();
            connectionList1.Add(_sendSocket2);
            ruleBuffer.Add(new ArrayList());

            IPEndPoint clientIP = (IPEndPoint)_sendSocket2.RemoteEndPoint;
        }
    }

    // Message Only Mode 일 경우에는 Message만 주고 받는다.
    private void MessageOnly()
    {
        if (connectionList1.Count != 0)
        {
            ArrayList cons = new ArrayList(connectionList1);
            Socket.Select(cons, null, null, 1000);
            
            foreach (Socket soc in cons)
            {
                int read = soc.Receive(cameraBytes);
                Debug.Log("read : " + read);

                if (read == 0)
                    return;

                // Message 정보 받을때
                ArrayList buf = (ArrayList)ruleBuffer[connectionList1.IndexOf(soc)];
                Debug.Log("Receive Message : " + System.Text.Encoding.UTF7.GetString(cameraBytes));

                for (int i = 0; i < read; i++)
                {
                    if (System.Text.Encoding.UTF7.GetString(cameraBytes, i, 1) == "\n")
                    {
                        string str = EncodingBytes((byte[])buf.ToArray(typeof(byte)));
                        Debug.Log("Receive Message : " + str);

                        if (str == "reset")
                        {
                            DeleteButton();
                            _tempNum = 1;
                        }
                        else if (str == "end")
                        { }
                        else if (str == "#SelectMode#")
                        {
                            SendMessage("#Mode#");
                        }
                        else
                        {
                            CreateButton(_tempNum++ + " : " + str);
                        }
                        buf.Clear();
                    }
                    else
                    {
                        buf.Add(cameraBytes[i]);
                    }
                }
                buf.Clear();
            }

        }
    }

    // Message + Webcam 모드
    private void MessageAndWebcam()
    {
        // 소켓이 연결되어 있을 경우에는
        if (connectionList1.Count != 0)
        {
            ArrayList cons = new ArrayList(connectionList1);
            Socket.Select(cons, null, null, 1000);
            
            foreach (Socket soc in cons)
            {
                // 영상을 받아온다.
                int read = soc.Receive(cameraBytes);

                // 받은 데이터가 0일 경우에는 return
                if (read == 0)
                    return;

                if (read > 100)
                {
                    try
                    {
                        if (!_isCameraCheck)
                            return;

                        // 영상 byte 정보들을 한 사이클이 완료 될때까지 받는다.
                        int j = 0;
                        for (int i = receiveLine; i < receiveLine + read; i++)
                        {
                            colBytes[i] = cameraBytes[j];
                            j++;
                        }
                        receiveLine += read;

                        // 영상 정보가 다 받아졌으면
                        if (receiveLine == 11520)
                        {
                            // 영상을 뿌려준다.
                            int idx = 0;
                            for (int height = 0; height < 48; height++)
                            {
                                for (int width = 0; width < 80; width++)
                                {
                                    cols[height * 80 + width].r = colBytes[idx];
                                    cols[height * 80 + width].g = colBytes[idx + 1];
                                    cols[height * 80 + width].b = colBytes[idx + 2];

                                    idx += 3;
                                }
                            }
                            SetTexture();
                            return;
                        }
                        else
                            break;
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        // 만일 영상정보가 잘 못 되었을 경우에는 재 접속 한다.
                        Debug.Log(e.Message);
                        SendMessage("#ReConnect#");
                        _isCameraCheck = false;
                        receiveLine = 0;
                        read = 0;
                        return;
                    }
                }
                else
                {
                    // Message 정보 받을때
                    ArrayList buf = (ArrayList)ruleBuffer[connectionList1.IndexOf(soc)];
                    //Debug.Log("Receive Message : " + System.Text.Encoding.UTF7.GetString(cameraBytes));

                    for (int i = 0; i < read; i++)
                    {
                        if (System.Text.Encoding.UTF7.GetString(cameraBytes, i, 1) == "\n")
                        {
                            string str = EncodingBytes((byte[])buf.ToArray(typeof(byte)));
                            Debug.Log("Receive Message : " + str);

                            if (str == "reset")
                            {
                                DeleteButton();
                                _tempNum = 1;
                            }
                            else if (str == "end")
                            { }
                            else if (str == "#CameraStart#")
                            {
                                _isCameraCheck = true;
                            }
                            else if (str == "#SelectMode#")
                            {
                                SendMessage("#CameraSpeed#:" + cameraSpeed.ToString());
                            }
                            else
                            {
                                CreateButton(_tempNum++ + " : " + str);
                            }
                            buf.Clear();
                        }
                        else
                        {
                            buf.Add(cameraBytes[i]);
                        }
                    }
                    buf.Clear();
                }
            }
        }
    }

    // 영상을 화면에 재생
    private void SetTexture()
    {
        receiveLine = 0;
        remainBytes = 0;

        texture.SetPixels32(cols);
        texture.Apply();
    }

    // PlayRule 정보 다시 받기
    void RequestEventList()
    {
        SendMessage("#reset#");
    }

    // Message Send
    private void SendMessage(string message)
    {
        if (_sendSocket2.Connected)
        {
            byte[] checkData = EncodingString(message + "\n");
            _sendSocket2.Send(checkData);
            Debug.Log("Send Message : " + message);
        }
    }

    // 바이트를 문자열로 인코딩
    public string EncodingBytes(byte[] data)
    {
        return System.Text.Encoding.UTF7.GetString(data);
    }

    // 문자열을 바이트로 인코딩
    public byte[] EncodingString(string data)
    {
        return System.Text.Encoding.UTF7.GetBytes(data);
    }

    // 버튼 생성 함수
    public void CreateButton(string eventText)
    {
        Debug.Log(eventText);
        ButtonUIDrawer.Instance.AddButton(eventText);
    }

    public void DeleteButton()
    {
        ButtonUIDrawer.Instance.DestroyAllButton();
    }

    void OnAppQuit()
    {
        Application.Quit();
    }
	//-------------------------------------------------------------------------
	// 싱글톤 지정
	//-------------------------------------------------------------------------
	private static RemoteManagerClient _instance = null;
	public static RemoteManagerClient Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType(typeof(RemoteManagerClient)) as RemoteManagerClient;
				if (_instance == null)
					Debug.LogError("There needs to be one active RemoteManager script on a GameObject in your scene.");

			}
			return _instance;
		}
	}
}

