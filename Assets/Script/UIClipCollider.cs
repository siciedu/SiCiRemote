//--------------------------------------------------------------------------------
// Author	   : ������
// Date		   : 2012-09-06
// Copyright   : 2011-2012 Hansung Univ. Robots in Education & Entertainment Lab.
//
// Description : Ŭ���� �ɰ�� �ڵ����� Collider�� ���� ���ִ� ������Ʈ
//				 ���� �Ϻ� ���� ����
//--------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;

public class UIClipCollider : MonoBehaviour 
{
	UIPanel clipPanel = null;
	UIWidget myWidget = null;

	bool ClipEnable = true;

	void EnableClip(bool flg)
	{
		ClipEnable = flg;
	}

	
	void Start () 
	{
		Init();
	}
	
	
	void Update () 
	{
		if(clipPanel == null || myWidget == null)
			return;

		if(ClipEnable == false)
			return;

		bool bVis = clipPanel.IsVisible(myWidget);

		if(collider.enabled != bVis)
		{
			collider.enabled = bVis;
		}
	}


	public void Init()
	{
		clipPanel = transform.parent.gameObject.GetComponent<UIPanel>() as UIPanel;

		if(clipPanel != null)
		{
			if(clipPanel.clipping == UIDrawCall.Clipping.None)
			{
				clipPanel = null;
			}
		}

		myWidget = GetComponentInChildren<UIWidget>() as UIWidget;

		// �г� �� ã�� ��� ������ �г� ���� - ������
		if(clipPanel == null)
		{
			clipPanel = myWidget.panel;
		}
	}
}
