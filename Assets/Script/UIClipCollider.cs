//--------------------------------------------------------------------------------
// Author	   : 김재현
// Date		   : 2012-09-06
// Copyright   : 2011-2012 Hansung Univ. Robots in Education & Entertainment Lab.
//
// Description : 클리핑 될경우 자동으로 Collider를 설정 해주는 컴포넌트
//				 완전 완벽 하지 않음
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

		// 패널 못 찾을 경우 위젯의 패널 참조 - 김진성
		if(clipPanel == null)
		{
			clipPanel = myWidget.panel;
		}
	}
}
