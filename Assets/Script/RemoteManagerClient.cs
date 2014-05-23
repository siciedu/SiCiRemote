//--------------------------------------------------------------------------------
// Author	   : ����� & ������
// Date		   : Last Modify(2014-05-15)
// Copyright   : Hansung Univ. Robots in Education & Entertainment Lab.
//
// Description : TCP�� �̿��� Remote Controller
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

    // xml ����
    public XmlDocument xmldoc1;
    public XmlDocument xmldoc2;
    public XmlElement xmlElement1;
    public XmlElement xmlElement2;

    // xml ��� ����
    private string _folderPath;
    public string _filePath1;
    public string _filePath2;

    // ���� ����
    private Socket _sendSocket1;
    private Socket _sendSocket2;
    ArrayList connectionList1 = new ArrayList();
    ArrayList listenlist1;

    // ���� ����
    Texture2D texture;
    byte[] cameraBytes = new byte[11520];
    byte[] colBytes = new byte[11520];
    Color32[] cols = new Color32[3840];
    int receiveLine = 0;
    int remainBytes = 0;

    // PlayRule ����
    byte[] ruleBytes = new byte[512];
    ArrayList ruleBuffer = new ArrayList();

    bool _isOpened = false;
    bool _isCameraCheck = false;
    bool _isMessageOnly = false;

    private int _tempNum = 1;

    int mode;
    float cameraSpeed = 0.5f;

    // ���� �� ���� ���� ����
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

    // SiCi�� Message ���� �Լ�
	public void Send(string message)
	{
        byte[] checkData = EncodingString(message+"\n");
        _sendSocket2.Send(checkData);
	}
	
    void Awake()
    {
        // ��׶��� ���� Ȱ��ȭ
        Application.runInBackground = true;
        ShutDown();
        
        // Alpha value ����
        for (int i = 0; i < 3840; i++)
            cols[i].a = 255;
        
        texture = new Texture2D(80, 48);
        transform.FindChild("Plane").renderer.material.mainTexture = texture;

        // Xml Data Load
        XmlLoad();
    }

    // Xml Load �Լ�
    private void XmlLoad()
    {
        // Folder, File Path ����
        // Player Setting - Other Setting - Write Access = External ���� �ʿ�
        // �ȵ���̵� �������� /root/Android/data/hansung.reel.remotecontroller/files
        _folderPath = Application.persistentDataPath;
        _filePath1 = Application.persistentDataPath + "/XmlLog.xml";
        _filePath2 = Application.persistentDataPath + "/XmlCount.xml";
        
        // ���� ������ ���� ��쿡�� ���� ����
        DirectoryInfo di = new DirectoryInfo(_folderPath);
        if (di.Exists == false)
            di.Create();

        // ������ ���� ��쿡�� ���� ����
        // ������ ���� ��쿡�� �ش� xml ���� �ε�
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

    // ���� ���� ��ư ������ ���۵Ǵ� �Լ�
    private void OpenSocket()
    {
        mode = int.Parse(GameObject.Find("WebcamSelect").transform.FindChild("Label").GetComponent<UILabel>().text.Trim());
        
        // Message Only, Message + Webcam ��� ����
        if (0 == mode)
            _isMessageOnly = true;
        else
            _isMessageOnly = false;
        
        // ���� ����
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

    // Ŭ���̾�Ʈ ������ ������ Ȯ���ϴ� �Լ�
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

    // Message Only Mode �� ��쿡�� Message�� �ְ� �޴´�.
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

                // Message ���� ������
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

    // Message + Webcam ���
    private void MessageAndWebcam()
    {
        // ������ ����Ǿ� ���� ��쿡��
        if (connectionList1.Count != 0)
        {
            ArrayList cons = new ArrayList(connectionList1);
            Socket.Select(cons, null, null, 1000);
            
            foreach (Socket soc in cons)
            {
                // ������ �޾ƿ´�.
                int read = soc.Receive(cameraBytes);

                // ���� �����Ͱ� 0�� ��쿡�� return
                if (read == 0)
                    return;

                if (read > 100)
                {
                    try
                    {
                        if (!_isCameraCheck)
                            return;

                        // ���� byte �������� �� ����Ŭ�� �Ϸ� �ɶ����� �޴´�.
                        int j = 0;
                        for (int i = receiveLine; i < receiveLine + read; i++)
                        {
                            colBytes[i] = cameraBytes[j];
                            j++;
                        }
                        receiveLine += read;

                        // ���� ������ �� �޾�������
                        if (receiveLine == 11520)
                        {
                            // ������ �ѷ��ش�.
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
                        // ���� ���������� �� �� �Ǿ��� ��쿡�� �� ���� �Ѵ�.
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
                    // Message ���� ������
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

    // ������ ȭ�鿡 ���
    private void SetTexture()
    {
        receiveLine = 0;
        remainBytes = 0;

        texture.SetPixels32(cols);
        texture.Apply();
    }

    // PlayRule ���� �ٽ� �ޱ�
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

    // ����Ʈ�� ���ڿ��� ���ڵ�
    public string EncodingBytes(byte[] data)
    {
        return System.Text.Encoding.UTF7.GetString(data);
    }

    // ���ڿ��� ����Ʈ�� ���ڵ�
    public byte[] EncodingString(string data)
    {
        return System.Text.Encoding.UTF7.GetBytes(data);
    }

    // ��ư ���� �Լ�
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
	// �̱��� ����
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

