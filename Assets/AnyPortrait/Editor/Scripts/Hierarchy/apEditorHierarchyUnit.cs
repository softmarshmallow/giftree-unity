/*
*	Copyright (c) 2017-2019. RainyRizzle. All rights reserved
*	Contact to : https://www.rainyrizzle.com/ , contactrainyrizzle@gmail.com
*
*	This file is part of [AnyPortrait].
*
*	AnyPortrait can not be copied and/or distributed without
*	the express perission of [Seungjik Lee].
*
*	Unless this file is downloaded from the Unity Asset Store or RainyRizzle homepage, 
*	this file and its users are illegal.
*	In that case, the act may be subject to legal penalties.
*/

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using AnyPortrait;

namespace AnyPortrait
{

	public class apEditorHierarchyUnit
	{
		// Member
		//--------------------------------------------------------------------------
		public delegate void FUNC_UNIT_CLICK(apEditorHierarchyUnit eventUnit, int savedKey, object savedObj);
		public delegate void FUNC_UNIT_CLICK_VISIBLE(apEditorHierarchyUnit eventUnit, int savedKey, object savedObj, bool isVisible, bool isPostfixIcon);
		public delegate void FUNC_UNIT_CLICK_ORDER_CHANGED(apEditorHierarchyUnit eventUnit, int savedKey, object savedObj, bool isOrderUp);
		public delegate void FUNC_UNIT_CLICK_RESTORE_TMPWORK();


		public enum UNIT_TYPE
		{
			Label,
			ToggleButton,
			ToggleButton_Visible,
			OnlyButton,
		}
		public UNIT_TYPE _unitType = UNIT_TYPE.Label;
		public Texture2D _icon = null;
		public string _text = "";

		public int _level = 0;
		public int _savedKey = -1;
		public object _savedObj = null;

		public enum VISIBLE_TYPE
		{
			None,//<<안보입니더
			NoKey,//MoKey는 없지만 출력은 됩니다.
			Current_Visible,
			Current_NonVisible,
			TmpWork_Visible,
			TmpWork_NonVisible,
			ModKey_Visible,
			ModKey_NonVisible,
			Default_Visible,
			Default_NonVisible

		}
		public VISIBLE_TYPE _visibleType_Prefix = VISIBLE_TYPE.None;//Visible 속성이 붙은 경우는 이것도 세팅해야한다.
		public VISIBLE_TYPE _visibleType_Postfix = VISIBLE_TYPE.None;//Visible 속성이 붙은 경우는 이것도 세팅해야한다.

		//추가 19.11.24 : VisiblePrefix를 실시간으로 갱신해야할 필요가 있는데, 매번 RefreshUnit을 호출할 순 없다.
		//함수 대리자를 이용해서 실시간 갱신을 하도록 하자
		public delegate void FUNC_REFRESH_VISIBLE_PREFIX(apEditorHierarchyUnit unit);
		private FUNC_REFRESH_VISIBLE_PREFIX _funcRefreshVisiblePreFix = null;

		//추가 19.6.29 : VisiblePrefix를 리셋하는 버튼을 Label에 추가할 수 있다.
		public bool _isRestoreTmpWorkVisibleBtn = false;
		private bool _isTmpWorkAnyChanged = false;

		public apEditorHierarchyUnit _parentUnit = null;
		public List<apEditorHierarchyUnit> _childUnits = new List<apEditorHierarchyUnit>();

		private bool _isFoldOut = true;
		private bool _isSelected = false;
		private bool _isModRegistered = false;//추가) 현재 선택한 Mod 등에서 등록된
		private bool _isAvailable = true;//선택 가능한가. 기본적으로는 True. 예외적으로 False를 설정해야한다.

		public void SetFoldOut(bool isFoldOut) { _isFoldOut = isFoldOut; }
		public void SetSelected(bool isSelected) { _isSelected = isSelected; }
		public void SetModRegistered(bool isModRegistered)
		{
			_isModRegistered = isModRegistered;
			RefreshPrevRender();
		}
		public void SetAvailable(bool isAvailable) {  _isAvailable = isAvailable; }


		public bool IsFoldOut { get { return _isFoldOut; } }
		public bool IsSelected { get { return _isSelected; } }
		public bool IsAvailable {  get {  return _isAvailable; } }

		public FUNC_UNIT_CLICK _funcClick = null;
		public FUNC_UNIT_CLICK_VISIBLE _funcClickVisible = null;
		public FUNC_UNIT_CLICK_ORDER_CHANGED _funcClickOrderChanged = null;

		//이전
		//private GUIContent _guiContent_Text = new GUIContent();
		//private GUIContent _guiContent_Icon = new GUIContent();

		//변경 19.11.16
		private apGUIContentWrapper _guiContent_Text = new apGUIContentWrapper();
		private apGUIContentWrapper _guiContent_Icon = new apGUIContentWrapper();

		//private GUIContent _guiContent_Folded = new GUIContent();
		private GUIStyle _guiStyle_None;
		private GUIStyle _guiStyle_Selected;
		private GUIStyle _guiStyle_ModIcon;
		private Color _guiColor_TextColor_None;
		private Color _guiColor_TextColor_Selected;
		private bool _isGUIStyleCreated = false;

		//이전
		//private GUIContent _guiContent_FoldDown = new GUIContent();
		//private GUIContent _guiContent_FoldRight = new GUIContent();

		//변경 19.11.16
		private apGUIContentWrapper _guiContent_FoldDown = new apGUIContentWrapper();
		private apGUIContentWrapper _guiContent_FoldRight = new apGUIContentWrapper();

		private enum VISIBLE_ICON
		{
			Current,
			TmpWork,
			Default,
			ModKey
		}
		//이전
		//private GUIContent _guiContent_NoKey = null;
		//private GUIContent[] _guiContent_Visible = new GUIContent[4];
		//private GUIContent[] _guiContent_Nonvisible = new GUIContent[4];

		//private GUIContent _guiContent_ModRegisted = new GUIContent();

		//private GUIContent _guiContent_OrderUp = new GUIContent();
		//private GUIContent _guiContent_OrderDown = new GUIContent();

		//변경 19.11.16
		private apGUIContentWrapper _guiContent_NoKey = null;
		private apGUIContentWrapper[] _guiContent_Visible = new apGUIContentWrapper[4];
		private apGUIContentWrapper[] _guiContent_Nonvisible = new apGUIContentWrapper[4];

		private apGUIContentWrapper _guiContent_ModRegisted = new apGUIContentWrapper();

		private apGUIContentWrapper _guiContent_OrderUp = new apGUIContentWrapper();
		private apGUIContentWrapper _guiContent_OrderDown = new apGUIContentWrapper();


		private bool _isOrderChangable = false;

		public int _indexPerParent = -1;
		private int _indexCountForChild = 0;


		//추가 19.6.29 : RestoreTmpWorkVisible 버튼
		//private GUIContent _guiContent_RestoreTmpWorkVisible_ON = null;
		//private GUIContent _guiContent_RestoreTmpWorkVisible_OFF = null;

		//변경 19.11.16
		private apGUIContentWrapper _guiContent_RestoreTmpWorkVisible_ON = null;
		private apGUIContentWrapper _guiContent_RestoreTmpWorkVisible_OFF = null;


		//추가된 내용
		//일부 버튼들은 나오거나 안나올 수 있다.
		//GUIEvent (Layout)을 기준으로 모두 갱신되는데,
		//그 외의 이벤트에서는 "이전 프레임의 기록"을 따라야 한다.
		//이전 프레임에서 렌더링 되었을때 -> 현재 안될때
		// >> 더미 렌더링을 한다. 클릭 이벤트는 발생하지 않는다.
		//이전 프레임에서 렌더링 안되고 -> 현재 될때
		// >> 렌더링하지 않는다.

		//리셋할때 체크한다.
		//GUIEvent에서는 이 변수를 무시한다. (렌더 여부를 갱신한다.)
		private bool _isPrevRender_ModRegBox = false;
		private bool _isPrevRender_VisiblePrefix = false;
		private bool _isPrevRender_VisiblePostfix = false;
		private bool _isPrevRender_Fold = false;

		private bool _isCurRender_ModRegBox;
		private bool _isCurRender_VisiblePrefix;
		private bool _isCurRenderFoldBtn;
		private bool _isCurRender_VisiblePostFix;

		private bool _isNextRender_ModRegBox;
		private bool _isNextRender_VisiblePrefix;
		private bool _isNextRenderFoldBtn;
		private bool _isNextRender_VisiblePostFix;

		//추가 19.6.29 : RestoreTmpWorkVisible 버튼
		private bool _isPrevRender_RestoreTmpWorkVisible = false;
		private bool _isCurRender_RestoreTmpWorkVisible = false;
		private bool _isNextRender_RestoreTmpWorkVisible = false;

		private FUNC_UNIT_CLICK_RESTORE_TMPWORK _funcClickRestoreTmpWorkVisible = null;

		private const string NO_NAME = " <No Name>  ";

		// Init
		//--------------------------------------------------------------------------
		public apEditorHierarchyUnit()
		{
			_isSelected = false;
			_isGUIStyleCreated = false;
			
			_indexPerParent = -1;

			_isAvailable = true;

			InitPrevRender();
		}

		private void InitPrevRender()
		{
			_isPrevRender_ModRegBox = false;
			_isPrevRender_VisiblePrefix = false;
			_isPrevRender_VisiblePostfix = false;
			_isPrevRender_Fold = false;

			_isPrevRender_RestoreTmpWorkVisible = false;

			_funcRefreshVisiblePreFix = null;
		}

		//추가
		/// <summary>
		/// GUIStyle이 있는지 체크하고 없으면 생성합니다.
		/// </summary>
		private void CheckAndCreateGUIStyle()
		{
			if(_isGUIStyleCreated)
			{
				return;
			}
			_isGUIStyleCreated = true;

			

			_guiStyle_None = new GUIStyle(GUIStyle.none);
			
			if (EditorGUIUtility.isProSkin)
			{
				_guiStyle_None.normal.textColor = GUI.skin.label.normal.textColor;
			}
			else
			{
				_guiStyle_None.normal.textColor = Color.black;
			}
			_guiStyle_None.alignment = TextAnchor.MiddleLeft;
			_guiStyle_None.margin = new RectOffset(0, 0, 0, 0);//추가 19.11.22

			_guiStyle_Selected = new GUIStyle(GUIStyle.none);

			if (EditorGUIUtility.isProSkin)
			{
				_guiStyle_Selected.normal.textColor = Color.cyan;
			}
			else
			{
				_guiStyle_Selected.normal.textColor = Color.white;
			}
			
			_guiStyle_Selected.alignment = TextAnchor.MiddleLeft;
			_guiStyle_Selected.margin = new RectOffset(0, 0, 0, 0);//추가 19.11.22

			_guiStyle_ModIcon = new GUIStyle(GUIStyle.none);
			_guiStyle_ModIcon.alignment = TextAnchor.MiddleCenter;
			_guiStyle_ModIcon.margin = new RectOffset(0, 0, 0, 0);

			_guiColor_TextColor_None = _guiStyle_None.normal.textColor;
			_guiColor_TextColor_Selected = _guiStyle_Selected.normal.textColor;
		}

		// Common
		//--------------------------------------------------------------------------

		//public void SetBasicIconImg(Texture2D imgFoldDown, Texture2D imgFoldRight, Texture2D imgModRegisted)//이전
		public void SetBasicIconImg(	apGUIContentWrapper guiContent_imgFoldDown, 
										apGUIContentWrapper guiContent_imgFoldRight, 
										apGUIContentWrapper guiContent_imgModRegisted)//변경 19.11.16 : 공유할 수 있게
		{
			//이전
			//_guiContent_FoldDown = new GUIContent(imgFoldDown);
			//_guiContent_FoldRight = new GUIContent(imgFoldRight);
			//_guiContent_ModRegisted = new GUIContent(imgModRegisted);

			//변경
			_guiContent_FoldDown = guiContent_imgFoldDown;
			_guiContent_FoldRight = guiContent_imgFoldRight;
			_guiContent_ModRegisted = guiContent_imgModRegisted;
		}

		//public void SetBasicIconImg(Texture2D imgFoldDown, Texture2D imgFoldRight, Texture2D imgModRegisted, Texture2D imgOrderUp, Texture2D imgOrderDown)//이전
		public void SetBasicIconImg(	apGUIContentWrapper guiContent_imgFoldDown, 
										apGUIContentWrapper guiContent_imgFoldRight, 
										apGUIContentWrapper guiContent_imgModRegisted, 
										apGUIContentWrapper guiContent_imgOrderUp, 
										apGUIContentWrapper guiContent_imgOrderDown)//변경 19.11.16 : 공유할 수 있게
		{
			//이전
			//_guiContent_FoldDown = new GUIContent(imgFoldDown);
			//_guiContent_FoldRight = new GUIContent(imgFoldRight);
			//_guiContent_ModRegisted = new GUIContent(imgModRegisted);

			//_guiContent_OrderUp = new GUIContent(imgOrderUp);
			//_guiContent_OrderDown = new GUIContent(imgOrderDown);

			//변경
			_guiContent_FoldDown = guiContent_imgFoldDown;
			_guiContent_FoldRight = guiContent_imgFoldRight;
			_guiContent_ModRegisted = guiContent_imgModRegisted;

			_guiContent_OrderUp = guiContent_imgOrderUp;
			_guiContent_OrderDown = guiContent_imgOrderDown;
		}


		//Visible 속성이 붙은 경우는 이걸 호출해서 세팅해줘야 한다.
		//이전
		//public void SetVisibleIconImage(GUIContent guiVisible_Current, GUIContent guiNonVisible_Current,
		//									GUIContent guiVisible_TmpWork, GUIContent guiNonVisible_TmpWork,
		//									GUIContent guiVisible_Default, GUIContent guiNonVisible_Default,
		//									GUIContent guiVisible_ModKey, GUIContent guiNonVisible_ModKey,
		//									GUIContent gui_NoKey
		//									)
		//변경 19.11.16 : Wrapper 클래스 이용
		public void SetVisibleIconImage(	apGUIContentWrapper guiVisible_Current, apGUIContentWrapper guiNonVisible_Current,
											apGUIContentWrapper guiVisible_TmpWork, apGUIContentWrapper guiNonVisible_TmpWork,
											apGUIContentWrapper guiVisible_Default, apGUIContentWrapper guiNonVisible_Default,
											apGUIContentWrapper guiVisible_ModKey, apGUIContentWrapper guiNonVisible_ModKey,
											apGUIContentWrapper gui_NoKey,
											FUNC_REFRESH_VISIBLE_PREFIX funcVisiblePrePostFix
											)
		{
			if (_guiContent_Visible == null)
			{
				_guiContent_Visible = new apGUIContentWrapper[4];
			}
			if (_guiContent_Nonvisible == null)
			{
				_guiContent_Nonvisible = new apGUIContentWrapper[4];
			}

			_guiContent_Visible[(int)VISIBLE_ICON.Current] = guiVisible_Current;
			_guiContent_Visible[(int)VISIBLE_ICON.TmpWork] = guiVisible_TmpWork;
			_guiContent_Visible[(int)VISIBLE_ICON.Default] = guiVisible_Default;
			_guiContent_Visible[(int)VISIBLE_ICON.ModKey] = guiVisible_ModKey;

			_guiContent_Nonvisible[(int)VISIBLE_ICON.Current] = guiNonVisible_Current;
			_guiContent_Nonvisible[(int)VISIBLE_ICON.TmpWork] = guiNonVisible_TmpWork;
			_guiContent_Nonvisible[(int)VISIBLE_ICON.Default] = guiNonVisible_Default;
			_guiContent_Nonvisible[(int)VISIBLE_ICON.ModKey] = guiNonVisible_ModKey;

			_guiContent_NoKey = gui_NoKey;

			_funcRefreshVisiblePreFix = funcVisiblePrePostFix;//추가 19.11.24
		}

		public void SetEvent(FUNC_UNIT_CLICK funcUnitClick)
		{
			_funcClick = funcUnitClick;
			_funcClickVisible = null;
			_funcClickOrderChanged = null;
			_isOrderChangable = false;
		}

		//TODO : Visible 속성이 붙은 경우는 위 함수(SetEvent)대신 이걸 호출해야한다.
		public void SetEvent(FUNC_UNIT_CLICK funcUnitClick, FUNC_UNIT_CLICK_VISIBLE funcClickVisible, FUNC_UNIT_CLICK_ORDER_CHANGED funcClickOrderChanged = null)
		{
			_funcClick = funcUnitClick;
			_funcClickVisible = funcClickVisible;
			_funcClickOrderChanged = funcClickOrderChanged;

			_isOrderChangable = _funcClickOrderChanged != null;
		}

		public void SetParent(apEditorHierarchyUnit parentUnit)
		{
			_parentUnit = parentUnit;
		}

		public void AddChild(apEditorHierarchyUnit childUnit)
		{
			childUnit._indexPerParent = _indexCountForChild;
			_indexCountForChild++;

			_childUnits.Add(childUnit);

			RefreshPrevRender();
		}

		//추가 Label에서 TmpWorkVisible을 초기화하는 버튼을 추가할 수 있다.
		//public void SetRestoreTmpWorkVisible(Texture2D btnIcon_ON, Texture2D btnIcon_OFF, FUNC_UNIT_CLICK_RESTORE_TMPWORK funcRestoreClick)//이전
		public void SetRestoreTmpWorkVisible(	apGUIContentWrapper guiContent_btnIcon_ON, 
												apGUIContentWrapper guiContent_btnIcon_OFF, 
												FUNC_UNIT_CLICK_RESTORE_TMPWORK funcRestoreClick)//변경 19.11.16 : 공유할 수 있게
		{
			_isRestoreTmpWorkVisibleBtn = true;
			_isTmpWorkAnyChanged = false;
			//이전
			//_guiContent_RestoreTmpWorkVisible_ON = new GUIContent(btnIcon_ON);
			//_guiContent_RestoreTmpWorkVisible_OFF = new GUIContent(btnIcon_OFF);

			//변경
			_guiContent_RestoreTmpWorkVisible_ON = guiContent_btnIcon_ON;
			_guiContent_RestoreTmpWorkVisible_OFF = guiContent_btnIcon_OFF;

			_funcClickRestoreTmpWorkVisible = funcRestoreClick;
		}

		public void SetRestoreTmpWorkVisibleAnyChanged(bool isAnyChanged)
		{
			_isTmpWorkAnyChanged = isAnyChanged;
		}


		// Set
		//--------------------------------------------------------------------------
		public void ChangeText(string text)
		{
			//수정 1.1 : 버그
			if(text == null)
			{
				text = "";
			}
			_text = text;
			MakeGUIContent();
		}
		public void ChangeIcon(Texture2D icon)
		{
			_icon = icon;
			MakeGUIContent();
		}

		public void SetLabel(Texture2D icon, string text, int savedKey, object savedObj)
		{
			//수정 1.1 : 버그
			if(text == null)
			{
				text = "";
			}

			_unitType = UNIT_TYPE.Label;
			_icon = icon;
			_text = text;
			_savedKey = savedKey;
			_savedObj = savedObj;

			_isRestoreTmpWorkVisibleBtn = false;

			MakeGUIContent();
		}

		public void SetToggleButton(Texture2D icon, string text, int savedKey, object savedObj)
		{
			//수정 1.1 : 버그
			if(text == null)
			{
				text = "";
			}

			_unitType = UNIT_TYPE.ToggleButton;
			_icon = icon;
			_text = text;
			_savedKey = savedKey;
			_savedObj = savedObj;

			MakeGUIContent();
		}

		public void SetToggleButton_Visible(Texture2D icon, string text, int savedKey, object savedObj, VISIBLE_TYPE visibleType_Prefix, VISIBLE_TYPE visibleType_Postfix)
		{
			//수정 1.1 : 버그
			if(text == null)
			{
				text = "";
			}

			_unitType = UNIT_TYPE.ToggleButton_Visible;
			_icon = icon;
			_text = text;
			_savedKey = savedKey;
			_savedObj = savedObj;
			_visibleType_Prefix = visibleType_Prefix;
			_visibleType_Postfix = visibleType_Postfix;

			MakeGUIContent();
		}

		public void SetOnlyButton(Texture2D icon, string text, int savedKey, object savedObj)
		{
			//수정 1.1 : 버그
			if(text == null)
			{
				text = "";
			}

			_unitType = UNIT_TYPE.OnlyButton;
			_icon = icon;
			_text = text;
			_savedKey = savedKey;
			_savedObj = savedObj;

			MakeGUIContent();
		}

		private void MakeGUIContent()
		{
			if (_icon != null)
			{
				if(_guiContent_Icon == null)
				{
					_guiContent_Icon = apGUIContentWrapper.Make(_icon);
				}
				else
				{
					_guiContent_Icon.SetImage(_icon);
				}
				//_guiContent_Icon = new GUIContent(_icon);//이전
			}
			else
			{
				//_guiContent_Icon = null;//null로 만들진 않는다.
				_guiContent_Icon.SetVisible(false);
			}

			//이전
			//if (!string.IsNullOrEmpty(_text))
			//{
			//	_guiContent_Text = new GUIContent(" " + _text + "  ");
			//}
			//else
			//{
			//	_guiContent_Text = new GUIContent(" <No Name>  ");
			//}

			//변경
			if(_guiContent_Text == null)
			{
				_guiContent_Text = new apGUIContentWrapper();
			}

			if (!string.IsNullOrEmpty(_text))
			{
				//공백(1) + 텍스트 + 공백(2)
				_guiContent_Text.ClearText(false);
				_guiContent_Text.AppendSpaceText(1, false);
				_guiContent_Text.AppendText(_text, false);
				_guiContent_Text.AppendSpaceText(2, true);
			}
			else
			{
				_guiContent_Text.SetText(NO_NAME);
			}

			RefreshPrevRender();
		}



		public void RefreshPrevRender()
		{
			_isPrevRender_ModRegBox = _isModRegistered;
			_isPrevRender_VisiblePrefix = (_unitType == UNIT_TYPE.ToggleButton_Visible && _visibleType_Prefix != VISIBLE_TYPE.None);
			_isPrevRender_VisiblePostfix = (_unitType == UNIT_TYPE.ToggleButton_Visible && _visibleType_Postfix != VISIBLE_TYPE.None);
			_isPrevRender_Fold = (_childUnits.Count > 0 || (_parentUnit == null && _unitType == UNIT_TYPE.Label));

			//추가 19.6.29 : TmpWork
			_isPrevRender_RestoreTmpWorkVisible = _unitType == UNIT_TYPE.Label && _isRestoreTmpWorkVisibleBtn ;
		}

		// GUI
		//--------------------------------------------------------------------------
		public void GUI_Render(int posY, int leftWidth, int width, int height, Vector2 scroll, int scrollLayoutHeight, bool isGUIEvent, int level, bool isOrderButtonVisible = false)
		{
			CheckAndCreateGUIStyle();//<<추가 : GUI Style 생성

			//추가 19.11.22
			//만약 렌더링하지 않아도 된다면 렌더링하지 않고 여백만 주고 넘어가야한다.
			if(!apEditorUtil.IsItemInScroll(posY, height, scroll, scrollLayoutHeight))
			{
				GUILayout.Space(height);
				return;
			}

			Rect lastRect = GUILayoutUtility.GetLastRect();

			//배경 렌더링
			if (_isSelected)
			{
				Color prevColor = GUI.backgroundColor;

				if(EditorGUIUtility.isProSkin)
				{
					GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
				}
				else
				{
					GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f, 1.0f);
				}
				

				GUI.Box(new Rect(lastRect.x + scroll.x, lastRect.y + height, width + 10, height), "");
				GUI.backgroundColor = prevColor;
			}

			EditorGUILayout.BeginHorizontal(GUILayout.Height(height));


			GUILayout.Space(2);

			//추가 : 19.11.24 : Pre/Post Fix 아이콘을 여기서 실시간으로 갱신할 수 있다.
			if(isGUIEvent)
			{
				if(_funcRefreshVisiblePreFix != null)
				{
					//체크 : 만약 값이 바뀌었다면 DebugLog (에러를 잡았당!)
					//VISIBLE_TYPE debug_Prefix = _visibleType_Prefix;
					_funcRefreshVisiblePreFix(this);
					//if(debug_Prefix != _visibleType_Prefix)
					//{
					//	Debug.LogError("Prefix 미갱신 오류가 보정되었다! : " + _guiContent_Text.Content.text);
					//}
				}
			}

			_isCurRender_ModRegBox = _isModRegistered;
			_isCurRender_VisiblePrefix = (_unitType == UNIT_TYPE.ToggleButton_Visible && _visibleType_Prefix != VISIBLE_TYPE.None);
			_isCurRenderFoldBtn = (_childUnits.Count > 0 || (_parentUnit == null && _unitType == UNIT_TYPE.Label));
			_isCurRender_VisiblePostFix = (_unitType == UNIT_TYPE.ToggleButton_Visible && _visibleType_Postfix != VISIBLE_TYPE.None);

			_isCurRender_RestoreTmpWorkVisible = (_unitType == UNIT_TYPE.Label && _isRestoreTmpWorkVisibleBtn);

			if (isGUIEvent)
			{
				//GUIEvent에서는 Prev를 무시한다.
				_isNextRender_ModRegBox = _isCurRender_ModRegBox;
				_isNextRender_VisiblePrefix = _isCurRender_VisiblePrefix;
				_isNextRenderFoldBtn = _isCurRenderFoldBtn;
				_isNextRender_VisiblePostFix = _isCurRender_VisiblePostFix;
				_isNextRender_RestoreTmpWorkVisible = _isCurRender_RestoreTmpWorkVisible;
			}
			else
			{
				//GUIEvent가 아닐 때에는 Prev의 값을 따른다.
				_isNextRender_ModRegBox = _isPrevRender_ModRegBox;
				_isNextRender_VisiblePrefix = _isPrevRender_VisiblePrefix;
				_isNextRenderFoldBtn = _isPrevRender_Fold;
				_isNextRender_VisiblePostFix = _isPrevRender_VisiblePostfix;
				_isNextRender_RestoreTmpWorkVisible = _isPrevRender_RestoreTmpWorkVisible;
			}
			
			

			// Modifier 등록 박스 렌더링
			//if (_isCurRender_ModRegBox)
			if(_isNextRender_ModRegBox)
			{
				//이전
				//GUILayout.Box(_guiContent_ModRegisted, _guiStyle_ModIcon, GUILayout.Width(8), GUILayout.Height(height));

				//변경
				GUILayout.Box(_guiContent_ModRegisted.Content, _guiStyle_ModIcon, GUILayout.Width(8), GUILayout.Height(height));
			}
			else
			{
				GUILayout.Space(8);
			}



			// 앞쪽의 "보기" 버튼
			//if (_unitType == UNIT_TYPE.ToggleButton_Visible && _visibleType_Prefix != VISIBLE_TYPE.None)
			if(_isNextRender_VisiblePrefix)
			{
				//앞쪽에도 Visible Button을 띄워야겠다면
				apGUIContentWrapper visibleGUIContent = null;

				if (!_isCurRender_VisiblePrefix)
				{
					//만약 더미를 렌더링 하는 경우
					visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.Current];
				}
				else
				{
					//정식 렌더링인 경우
					switch (_visibleType_Prefix)
					{
						case VISIBLE_TYPE.Current_Visible:		visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.Current]; break;
						case VISIBLE_TYPE.Current_NonVisible:	visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.Current]; break;
						case VISIBLE_TYPE.TmpWork_Visible:		visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.TmpWork]; break;
						case VISIBLE_TYPE.TmpWork_NonVisible:	visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.TmpWork]; break;
						case VISIBLE_TYPE.Default_Visible:		visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.Default]; break;
						case VISIBLE_TYPE.Default_NonVisible:	visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.Default]; break;
						case VISIBLE_TYPE.ModKey_Visible:		visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.ModKey]; break;
						case VISIBLE_TYPE.ModKey_NonVisible:	visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.ModKey]; break;
						case VISIBLE_TYPE.NoKey:				visibleGUIContent = _guiContent_NoKey; break;

					}
				}

				if (GUILayout.Button(visibleGUIContent.Content, _guiStyle_None, GUILayout.Width(20), GUILayout.Height(height)))
				{
					if (_isCurRender_VisiblePrefix)
					{
						if (_funcClickVisible != null)
						{
							_funcClickVisible(this, _savedKey, _savedObj,
								_visibleType_Prefix == VISIBLE_TYPE.Current_Visible ||
								_visibleType_Prefix == VISIBLE_TYPE.Default_Visible ||
								_visibleType_Prefix == VISIBLE_TYPE.TmpWork_Visible ||
								_visibleType_Prefix == VISIBLE_TYPE.ModKey_Visible, true);
						}
					}
				}
				leftWidth -= 22;
				if (leftWidth < 0)
				{
					leftWidth = 0;
					//leftWidth = level * 5;
				}
			}

			//추가 : 레이어 순서 변경 버튼
			if(isOrderButtonVisible && _isOrderChangable)
			{
				if(GUILayout.Button(_guiContent_OrderUp.Content, _guiStyle_None, GUILayout.Width(12), GUILayout.Height(height)))
				{
					if(_funcClickOrderChanged != null)
					{
						_funcClickOrderChanged(this, _savedKey, _savedObj, true);
					}
				}
				if(GUILayout.Button(_guiContent_OrderDown.Content, _guiStyle_None, GUILayout.Width(12), GUILayout.Height(height)))
				{
					if(_funcClickOrderChanged != null)
					{
						_funcClickOrderChanged(this, _savedKey, _savedObj, false);
					}
				}

				leftWidth -= 30;
				if (leftWidth < 0)
				{
					leftWidth = 0;
					//leftWidth = level * 5;
				}
				GUILayout.Space(leftWidth);
			}
			else
			{
				//기본 여백
				GUILayout.Space(Mathf.Max(leftWidth, level * 10));
			}
			
			


			//맨 앞에 ▼/▶ 아이콘을 보이고, 작동시킬지를 결정
			//bool isFoldVisible = false;
			//if (_childUnits.Count > 0 || (_parentUnit == null && _unitType == UNIT_TYPE.Label))
			//{
			//	isFoldVisible = true;
			//}

			int width_FoldBtn = height - 4;
			//int width_Icon = height - 2;
			int width_Icon = height - 6;

			//추가 19.6.29 : RestoreTmpWorkVisible버튼
			if(_isNextRender_RestoreTmpWorkVisible)
			{
				if (GUILayout.Button((_isTmpWorkAnyChanged ? _guiContent_RestoreTmpWorkVisible_ON.Content : _guiContent_RestoreTmpWorkVisible_OFF.Content), _guiStyle_None, GUILayout.Width(width_FoldBtn), GUILayout.Height(height)))
				{
					if (_funcClickRestoreTmpWorkVisible != null)
					{
						_funcClickRestoreTmpWorkVisible();
					}
				}
				GUILayout.Space(2);
			}

			
			if(_isNextRenderFoldBtn)
			{
				//Fold 아이콘을 출력하고 Button 기능을 추가한다.
				GUIContent btnContent = _guiContent_FoldDown.Content;
				if (!_isFoldOut)
				{
					btnContent = _guiContent_FoldRight.Content;
				}
				if (GUILayout.Button(btnContent, _guiStyle_None, GUILayout.Width(width_FoldBtn), GUILayout.Height(height)))
				{
					if (_isCurRenderFoldBtn)
					{
						//정식 렌더링인 경우에 바꿔주자
						_isFoldOut = !_isFoldOut;
					}
				}
			}
			else if(isOrderButtonVisible && _isOrderChangable)
			{
				GUILayout.Space(2);
			}
			else
			{
				GUILayout.Space(width_FoldBtn);
			}


			// 기본 아이콘
			if (_guiContent_Icon != null && _guiContent_Icon.IsVisible)
			{
				if (GUILayout.Button(_guiContent_Icon.Content, _guiStyle_None, GUILayout.Width(width_Icon), GUILayout.Height(height)))
				{
					if (_unitType == UNIT_TYPE.Label)
					{
						//if (isFoldVisible)
						if(_isNextRenderFoldBtn && _isCurRenderFoldBtn)
						{
							_isFoldOut = !_isFoldOut;
						}
					}
					else
					{
						if (_funcClick != null)
						{
							apEditorUtil.ReleaseGUIFocus();//<<추가 : 메뉴 바뀌면 무조건 GUI Focus를 날린다.
							_funcClick(this, _savedKey, _savedObj);
						}
					}
				}
			}


			if(!_isAvailable)
			{
				//GUI.contentColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
				_guiStyle_None.normal.textColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				_guiStyle_Selected.normal.textColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			}

			//유닛의 타입에 따라 다르게 출력한다.
			switch (_unitType)
			{
				//Label : 별도의 버튼 기능 없이 아이콘+텍스트만 보인다.
				//만약, Fold가 가능한 경우 버튼으로 바뀌는데, Fold Toggle에 사용된다.
				case UNIT_TYPE.Label:
					//if (isFoldVisible)
					if(_isNextRenderFoldBtn)
					{
						if (GUILayout.Button(_guiContent_Text.Content, _guiStyle_None, GUILayout.Height(height)))
						{
							if (_isNextRenderFoldBtn && _isCurRenderFoldBtn)
							{
								_isFoldOut = !_isFoldOut;
							}
						}
					}
					else
					{
						EditorGUILayout.LabelField(_guiContent_Text.Content, GUILayout.Height(height));
					}
					break;

				//OnlyButton : Toggle 기능 없이 항상 버튼의 역할을 한다.
				case UNIT_TYPE.OnlyButton:
					if (GUILayout.Button(_guiContent_Text.Content, _guiStyle_None, GUILayout.Height(height)))
					{
						if (_funcClick != null)
						{
							apEditorUtil.ReleaseGUIFocus();//<<추가 : 메뉴 바뀌면 무조건 GUI Focus를 날린다.
							_funcClick(this, _savedKey, _savedObj);
						}
					}
					break;

				//ToggleButton : Off된 상태에서는 On하기 위한 버튼이며, On이 된 경우는 단순히 아이콘+텍스트만 출력한다.
				case UNIT_TYPE.ToggleButton:
					if (!_isSelected)
					{
						if (GUILayout.Button(_guiContent_Text.Content, _guiStyle_None, GUILayout.Height(height)))
						{
							if (_funcClick != null)
							{
								apEditorUtil.ReleaseGUIFocus();//<<추가 : 메뉴 바뀌면 무조건 GUI Focus를 날린다.
								_funcClick(this, _savedKey, _savedObj);
							}
						}
					}
					else
					{

						GUILayout.Label(_guiContent_Text.Content, _guiStyle_Selected, GUILayout.Height(height));
					}

					break;

				//ToggleButton
				case UNIT_TYPE.ToggleButton_Visible:
					if (!_isSelected)
					{
						if (GUILayout.Button(_guiContent_Text.Content, _guiStyle_None, GUILayout.Height(height)))
						{
							if (_funcClick != null)
							{
								apEditorUtil.ReleaseGUIFocus();//<<추가 : 메뉴 바뀌면 무조건 GUI Focus를 날린다.
								_funcClick(this, _savedKey, _savedObj);
							}
						}
					}
					else
					{
						GUILayout.Label(_guiContent_Text.Content, _guiStyle_Selected, GUILayout.Height(height));
					}


					//if (_visibleType_Postfix != VISIBLE_TYPE.None)
					if(_isNextRender_VisiblePostFix)
					{
						apGUIContentWrapper visibleGUIContent = null;

						if (!_isCurRender_VisiblePostFix)
						{
							//더미 렌더링이라면
							visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.Current];
						}
						else
						{
							switch (_visibleType_Postfix)
							{
								case VISIBLE_TYPE.Current_Visible:		visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.Current]; break;
								case VISIBLE_TYPE.Current_NonVisible:	visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.Current]; break;
								case VISIBLE_TYPE.TmpWork_Visible:		visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.TmpWork]; break;
								case VISIBLE_TYPE.TmpWork_NonVisible:	visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.TmpWork]; break;
								case VISIBLE_TYPE.Default_Visible:		visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.Default]; break;
								case VISIBLE_TYPE.Default_NonVisible:	visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.Default]; break;
								case VISIBLE_TYPE.ModKey_Visible:		visibleGUIContent = _guiContent_Visible[(int)VISIBLE_ICON.ModKey]; break;
								case VISIBLE_TYPE.ModKey_NonVisible:	visibleGUIContent = _guiContent_Nonvisible[(int)VISIBLE_ICON.ModKey]; break;
								case VISIBLE_TYPE.NoKey:				visibleGUIContent = _guiContent_NoKey; break;

							}
						}

						if (GUILayout.Button(visibleGUIContent.Content, _guiStyle_None, GUILayout.Width(20), GUILayout.Height(height)))
						{
							if (_isCurRender_VisiblePostFix)
							{
								if (_funcClickVisible != null)
								{

									_funcClickVisible(this, _savedKey, _savedObj,
										_visibleType_Postfix == VISIBLE_TYPE.Current_Visible ||
										_visibleType_Postfix == VISIBLE_TYPE.Default_Visible ||
										_visibleType_Postfix == VISIBLE_TYPE.TmpWork_Visible ||
										_visibleType_Postfix == VISIBLE_TYPE.ModKey_Visible, false);
								}
							}
						}
					}
					break;
			}

			EditorGUILayout.EndHorizontal();

			if(!_isAvailable)
			{
				//GUI.contentColor = _guiColor_ContentColor;
				_guiStyle_None.normal.textColor = _guiColor_TextColor_None;
				_guiStyle_Selected.normal.textColor = _guiColor_TextColor_Selected;
			}

			if (isGUIEvent)
			{
				//이전 프레임과 렌더링 동기화
				//설정에 의해 Prev를 갱신합니다.
				RefreshPrevRender();
			}
			
		}
	}

}