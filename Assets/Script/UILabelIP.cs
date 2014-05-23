using UnityEngine;
using System.Collections;
using System.Net;

public class UILabelIP : MonoBehaviour
{

	// Use this for initialization
	void Start()
	{
		string host = Dns.GetHostName();
		string ip = Dns.GetHostAddresses(host)[0].ToString();

		if(string.IsNullOrEmpty(ip) == false)
		{
			GetComponent<UILabel>().text = "IP:" + ip;
		}
	}
}
