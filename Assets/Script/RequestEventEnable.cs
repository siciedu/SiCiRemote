//--------------------------------------------------------------------------------
// Author	   : 정재훈
// Date		   : 2012-10-16
// Copyright   : 2011-2012 Hansung Univ. Robots in Education & Entertainment Lab.
//
// Description : 버튼에 포함되어 이벤트 실행 요청 보내는 콤포넌트
//
//--------------------------------------------------------------------------------


using UnityEngine;
using System.Collections;
using System;
using System.Xml;


public class RequestEventEnable : MonoBehaviour
{
    // PlayRule 버튼이 눌러졌을 경우
    void OnClick()
    {
        // Xml 저장
        XmlSave();

        // PlayRule 전송
        SendPlayRule();
        
        // 버튼 효과
        StartCoroutine(ChangeColor());
    }

    // PlayRule 전송함수
    private void SendPlayRule()
    {
        string tmp = this.transform.FindChild("Label").GetComponent<UILabel>().text.Trim().Split(':')[0] + "\n";
        RemoteManagerClient.Instance.Send(tmp);
        Debug.Log("Send Message : " + tmp);
    }

    private void XmlSave()
    {
        // xml 명명 규칙
        // 1. tag 이름에 !"#$%&'()*+,/;<=>?@[\]^`{|}~, 들어 가면 안되고 빈칸, -, ., 숫자로 시작할 수 없다는 규칙
        // 2. "<", "&"는 절대 들어가면 안된다. CDATA 로 사용 가능.
        // 3. 이스케이프 문자 &lt; = "<", &gt; = ">", &amp; = "&", &apos; = ', &quot; = "

        // PlayRule 이름 얻어 옴
        string str = this.transform.FindChild("Label").GetComponent<UILabel>().text.Trim().Split(':')[1];
        
        // string to char[], char[] to string
        char[] bbb = str.ToCharArray();
        
        // 맨앞이 숫자이면 영어로 바꿔준다.
        // 오류로 임시 막음
        //if (char.IsNumber(bbb[0]))
        //    bbb[0] = 'e';
        
        str = new string(bbb);
        
        // xml 규칙에 따라 예외처리
        str = System.Text.RegularExpressions.Regex.Replace(str, @"[!#$%&'()*+,/;<=>?@[\]^`{|}~,]", string.Empty);
        str = System.Text.RegularExpressions.Regex.Replace(str, " ", string.Empty);

        XmlElement xmlElement1_1 = RemoteManagerClient.Instance.xmldoc1.CreateElement("PlayRule");
        try
        {
            // 현재 시간과 함께 저장
            xmlElement1_1.SetAttribute(str, DateTime.Now.ToString());
        }
        catch (Exception)
        {
            // 만일 형식에 맞지 않다면 EPlayRuleNameError라는 이름으로 저장
            xmlElement1_1.SetAttribute("PlayRuleNameError", DateTime.Now.ToString());
        }
        RemoteManagerClient.Instance.xmlElement1.AppendChild(xmlElement1_1);
        RemoteManagerClient.Instance.xmldoc1.AppendChild(RemoteManagerClient.Instance.xmlElement1);
        RemoteManagerClient.Instance.xmldoc1.Save(RemoteManagerClient.Instance._filePath1);

        // XML COUNT 저장 부분
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
        // 이미 저장된 PlayRule이 없을 경우에는 새로 만들어 준다.
        if (_tempBool == false)
        {
            XmlElement xmlElement2_2 = RemoteManagerClient.Instance.xmldoc2.CreateElement("PlayRule");

            xmlElement2_2.SetAttribute(str, "count:1");
            RemoteManagerClient.Instance.xmlElement2.AppendChild(xmlElement2_2);
            RemoteManagerClient.Instance.xmldoc2.AppendChild(RemoteManagerClient.Instance.xmlElement2);
            RemoteManagerClient.Instance.xmldoc2.Save(RemoteManagerClient.Instance._filePath2);
        }
    }

    // 버튼 효과 (색상 변경)
    IEnumerator ChangeColor()
    {
        this.transform.FindChild("Background").GetComponent<UISlicedSprite>().color = new Color(0.3f, 0.3f, 1f);
        yield return new WaitForSeconds(0.3f);
        this.transform.FindChild("Background").GetComponent<UISlicedSprite>().color = new Color(1, 1, 1);
    }
}
