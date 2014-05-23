//--------------------------------------------------------------------------------
// Author	   : 
// Date		   : 
// Copyright   : 2011-2012 Hansung Univ. Robots in Education & Entertainment Lab.
//
// Description : 
//
//--------------------------------------------------------------------------------


using UnityEngine;
using System.Collections;


public class ButtonUIDrawer : MonoBehaviour
{
	private const int _uiBaseDepth = 2;

	public Transform RootPanel;
	
	public UIAtlas UseAtlas;

	public UIFont UseFont;

	private BetterList<GameObject> _arrayInterplay = new BetterList<GameObject>();

    public AudioClip audio;

	public void DestroyAllButton()
	{
		foreach(GameObject curObject in _arrayInterplay)
		{
			GameObject.Destroy(curObject);
		}

		_arrayInterplay.Clear();
	}


	public void AddButton(string eventName)
	{
		GameObject curInterplayRoot = _AddInterplayRootObject();

		float startOffsetY = 15f;
		float buttonHeight = 70f;
		float marginHeight = 2f;
		float fontSize = 50f;

		int childCount = curInterplayRoot.transform.GetChildCount();

		// 백그라운드 사이즈 증가
		for(int i = 0; i < childCount; ++i)
		{
			Transform curChildTransform = curInterplayRoot.transform.GetChild(i);
			if(curChildTransform.gameObject.name == "SpriteBackGround")
			{
				Vector3 newScale = curChildTransform.localScale;
				newScale.y += buttonHeight + marginHeight;
				curChildTransform.localScale = newScale;
				break;
			}
		}

		float curOffsetY = startOffsetY + (childCount - 1) * (buttonHeight + marginHeight);
		Vector2 offsetPosition = new Vector2(10f, curOffsetY);

		_CreateEventButton(curInterplayRoot, offsetPosition, eventName, buttonHeight, fontSize);		

		_RepositionWidgets();
	}


	private GameObject _AddInterplayRootObject()
	{
		string _strBaseObjectName = "InterplayUI";
		foreach(GameObject curObject in _arrayInterplay)
		{
			if(curObject.name == _strBaseObjectName)
			{
				return curObject;
			}
		}
		
		// Root GameObject 설정
		GameObject baseObject = new GameObject(_strBaseObjectName);		

		// 패널을 부모로
		baseObject.transform.parent = RootPanel.transform;

		Vector3 createPosition = new Vector3(0f, 0f, 0f);
		int arraySize = _arrayInterplay.size;
		if(arraySize > 0)
		{
			createPosition.y = _arrayInterplay[_arrayInterplay.size - 1].transform.localPosition.y + 10f;
		}

		// 초기위치로 수정
		baseObject.transform.localPosition = createPosition;
		baseObject.transform.localScale = new Vector3(1, 1, 1);

		_arrayInterplay.Add(baseObject);

		_CreateInterplayBackground(baseObject);

		return baseObject;
	}


	private void _CreateInterplayBackground(GameObject parentObject)
	{
		const string _strSpriteBackGround = "SpriteBackGround";

		UISlicedSprite spriteBG = NGUITools.AddWidget<UISlicedSprite>(parentObject);
		spriteBG.name = _strSpriteBackGround;
		spriteBG.atlas = UseAtlas;
		spriteBG.pivot = UIWidget.Pivot.TopLeft;
		spriteBG.depth = _uiBaseDepth;
		spriteBG.spriteName = "BG_InterplayInfo";
		spriteBG.MakePixelPerfect();

		spriteBG.transform.localPosition = new Vector3(0f, 0f, 0f);
		spriteBG.transform.localScale = new Vector3(740f, 30f, 1f);

		NGUITools.AddWidgetCollider(spriteBG.gameObject);

		UIDragPanelContents dragContents = spriteBG.gameObject.AddComponent<UIDragPanelContents>();
		dragContents.draggablePanel = RootPanel.GetComponent<UIDraggablePanel>();		
	}


	//(부모게임오브젝트, ,버튼이름, 라벨내용, )
	private void _CreateEventButton(GameObject parentObject, Vector2 offsetPosition,string showMsg, float buttonHeight, float fontSize)
	{
		GameObject eventButtonObject = NGUITools.AddChild(parentObject);

		eventButtonObject.name = "EventButton";
        eventButtonObject.gameObject.AddComponent<RequestEventEnable>();

		Vector2 buttonSize = UseFont.CalculatePrintedSize(showMsg, false, UIFont.SymbolStyle.None);
		buttonSize *= fontSize;
		buttonSize.x += 70f;
		buttonSize.y = buttonHeight;
		
		eventButtonObject.transform.localPosition = new Vector3(offsetPosition.x + buttonSize.x * 0.5f, -offsetPosition.y - buttonSize.y * 0.5f, 0.0f);

		UISlicedSprite eventButtonBGSprite = NGUITools.AddWidget<UISlicedSprite>(eventButtonObject);
		eventButtonBGSprite.name = "Background";
		eventButtonBGSprite.atlas = UseAtlas;
		eventButtonBGSprite.spriteName = "Listbox_Out_Green";
		eventButtonBGSprite.depth = _uiBaseDepth + 1;		
		eventButtonBGSprite.transform.localScale = new Vector3(buttonSize.x, buttonSize.y, 1f);
		eventButtonBGSprite.MakePixelPerfect();

		UILabel lbl = NGUITools.AddWidget<UILabel>(eventButtonObject);
		lbl.font = UseFont;
		lbl.text = showMsg;
		lbl.depth = _uiBaseDepth + 2;
		lbl.MakePixelPerfect();

		// Add a collider
		NGUITools.AddWidgetCollider(eventButtonObject);

		// Add the scripts
		UIButton uibuttonComp = eventButtonObject.AddComponent<UIButton>();
		uibuttonComp.tweenTarget = eventButtonBGSprite.gameObject;
		uibuttonComp.hover = new Color(1f, 1f, 1f, 1f);

		eventButtonObject.AddComponent<UIButtonScale>();
		eventButtonObject.AddComponent<UIButtonOffset>();

		eventButtonObject.AddComponent<UIButtonSound>();
        eventButtonObject.GetComponent<UIButtonSound>().audioClip = audio;

        eventButtonObject.AddComponent<UIButtonScale>();

		UIClipCollider clipCollider = eventButtonObject.AddComponent<UIClipCollider>();
		clipCollider.Init();

		lbl.transform.localPosition = new Vector3(0.0f, 2.0f, 0f);			// 텍스트를 약간 앞으로 보내기 위해
		lbl.transform.localScale = new Vector3(fontSize, fontSize, 1f);
	}


	private void _RepositionWidgets()
	{
		UITable rootPanelTable = RootPanel.GetComponent<UITable>();
		if(null != rootPanelTable)
		{
			rootPanelTable.repositionNow = true;
		}
	}


	//-------------------------------------------------------------------------
	// 싱글톤 지정
	//-------------------------------------------------------------------------
	private static ButtonUIDrawer _instance = null;
	public static ButtonUIDrawer Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType(typeof(ButtonUIDrawer)) as ButtonUIDrawer;
				if (_instance == null)
					Debug.LogError("There needs to be one active ButtonUIDrawer script on a GameObject in your scene.");

			}
			return _instance;
		}
	}
}