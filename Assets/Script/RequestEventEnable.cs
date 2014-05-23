//--------------------------------------------------------------------------------
// Author	   : ������
// Date		   : 2012-10-16
// Copyright   : 2011-2012 Hansung Univ. Robots in Education & Entertainment Lab.
//
// Description : ��ư�� ���ԵǾ� �̺�Ʈ ���� ��û ������ ������Ʈ
//
//--------------------------------------------------------------------------------


using UnityEngine;
using System.Collections;
using System;
using System.Xml;


public class RequestEventEnable : MonoBehaviour
{
    // PlayRule ��ư�� �������� ���
    void OnClick()
    {
        // Xml ����
        XmlSave();

        // PlayRule ����
        SendPlayRule();
        
        // ��ư ȿ��
        StartCoroutine(ChangeColor());
    }

    // PlayRule �����Լ�
    private void SendPlayRule()
    {
        string tmp = this.transform.FindChild("Label").GetComponent<UILabel>().text.Trim().Split(':')[0] + "\n";
        RemoteManagerClient.Instance.Send(tmp);
        Debug.Log("Send Message : " + tmp);
    }

    private void XmlSave()
    {
        // xml ��� ��Ģ
        // 1. tag �̸��� !"#$%&'()*+,/;<=>?@[\]^`{|}~, ��� ���� �ȵǰ� ��ĭ, -, ., ���ڷ� ������ �� ���ٴ� ��Ģ
        // 2. "<", "&"�� ���� ���� �ȵȴ�. CDATA �� ��� ����.
        // 3. �̽������� ���� &lt; = "<", &gt; = ">", &amp; = "&", &apos; = ', &quot; = "

        // PlayRule �̸� ��� ��
        string str = this.transform.FindChild("Label").GetComponent<UILabel>().text.Trim().Split(':')[1];
        
        // string to char[], char[] to string
        char[] bbb = str.ToCharArray();
        
        // �Ǿ��� �����̸� ����� �ٲ��ش�.
        // ������ �ӽ� ����
        //if (char.IsNumber(bbb[0]))
        //    bbb[0] = 'e';
        
        str = new string(bbb);
        
        // xml ��Ģ�� ���� ����ó��
        str = System.Text.RegularExpressions.Regex.Replace(str, @"[!#$%&'()*+,/;<=>?@[\]^`{|}~,]", string.Empty);
        str = System.Text.RegularExpressions.Regex.Replace(str, " ", string.Empty);

        XmlElement xmlElement1_1 = RemoteManagerClient.Instance.xmldoc1.CreateElement("PlayRule");
        try
        {
            // ���� �ð��� �Բ� ����
            xmlElement1_1.SetAttribute(str, DateTime.Now.ToString());
        }
        catch (Exception)
        {
            // ���� ���Ŀ� ���� �ʴٸ� EPlayRuleNameError��� �̸����� ����
            xmlElement1_1.SetAttribute("PlayRuleNameError", DateTime.Now.ToString());
        }
        RemoteManagerClient.Instance.xmlElement1.AppendChild(xmlElement1_1);
        RemoteManagerClient.Instance.xmldoc1.AppendChild(RemoteManagerClient.Instance.xmlElement1);
        RemoteManagerClient.Instance.xmldoc1.Save(RemoteManagerClient.Instance._filePath1);

        // XML COUNT ���� �κ�
        bool _tempBool = false;
        if (RemoteManagerClient.Instance.xmlElement2.ChildNodes.Count != null)
        {
            for (int i = 0; i < RemoteManagerClient.Instance.xmlElement2.ChildNodes.Count; i++)
            {
                for (int j = 0; j < RemoteManagerClient.Instance.xmlElement2.ChildNodes[i].Attributes.Count; j++)
                {
                    if (str == RemoteManagerClient.Instance.xmlElement2.ChildNodes[i].Attributes[j].Name)
                    {
                        int _temp = int.Parse(RemoteManagerClient.Instance.xmlElement2.ChildNodes[i].Attributes[j].Value.Split(':')[1]);
                        _temp++;
                        
                        RemoteManagerClient.Instance.xmlElement2.ChildNodes[i].Attributes[j].Value = "count:" + _temp.ToString();
                        RemoteManagerClient.Instance.xmldoc2.Save(RemoteManagerClient.Instance._filePath2);
                        
                        _tempBool = true;
                        return;
                    }
                }
            }
        }
        // �̹� ����� PlayRule�� ���� ��쿡�� ���� ����� �ش�.
        if (_tempBool == false)
        {
            XmlElement xmlElement2_2 = RemoteManagerClient.Instance.xmldoc2.CreateElement("PlayRule");

            xmlElement2_2.SetAttribute(str, "count:1");
            RemoteManagerClient.Instance.xmlElement2.AppendChild(xmlElement2_2);
            RemoteManagerClient.Instance.xmldoc2.AppendChild(RemoteManagerClient.Instance.xmlElement2);
            RemoteManagerClient.Instance.xmldoc2.Save(RemoteManagerClient.Instance._filePath2);
        }
    }

    // ��ư ȿ�� (���� ����)
    IEnumerator ChangeColor()
    {
        this.transform.FindChild("Background").GetComponent<UISlicedSprite>().color = new Color(0.3f, 0.3f, 1f);
        yield return new WaitForSeconds(0.3f);
        this.transform.FindChild("Background").GetComponent<UISlicedSprite>().color = new Color(1, 1, 1);
    }
}
