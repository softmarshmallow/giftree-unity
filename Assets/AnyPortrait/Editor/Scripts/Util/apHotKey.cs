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

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
//using System.Diagnostics;


using AnyPortrait;

namespace AnyPortrait
{

	/// <summary>
	/// 에디터의 단축키를 처리하는 객체.
	/// 단축키 처리는 OnGUI이 후반부에 해야하는데,
	/// UI별로 단축키에 대한 처리 요구가 임의의 위치에서 이루어지므로, 이를 대신 받아서 지연시키는 객체.
	/// 모든 함수 요청은 OnGUI마다 리셋되고 다시 받는다.
	/// 이벤트에 따라 묵살될 수 있다.
	/// </summary>
	public class apHotKey
	{
		public delegate void FUNC_HOTKEY_EVENT(object paramObject);

		// Unit Class
		public class HotKeyEvent
		{
			public KeyCode _keyCode;
			public string _label;
			public bool _isShift;
			public bool _isAlt;
			public bool _isCtrl;
			public object _paramObject;
			public FUNC_HOTKEY_EVENT _funcEvent;
			public bool _isCombination;

			public HotKeyEvent()
			{
				_keyCode = KeyCode.Space;
				//_label;
				_isShift = false;
				_isAlt = false;
				_isCtrl = false;
				_paramObject = null;
				_funcEvent = null;
				_isCombination = false;
			}

			public void SetEvent(FUNC_HOTKEY_EVENT funcEvent, string label, KeyCode keyCode, bool isShift, bool isAlt, bool isCtrl, object paramObject)
			{
				_funcEvent = funcEvent;
				_label = label;
				_keyCode = keyCode;
				_isShift = isShift;
				_isAlt = isAlt;
				_isCtrl = isCtrl;

				_isCombination = _isShift || _isAlt || _isCtrl;

				_paramObject = paramObject;
			}
		}

		// Members
		//---------------------------------------------
		private int _iEvent = 0;
		private const int NUM_INIT_EVENT_POOL = 20;
		private List<HotKeyEvent> _hotKeyEvents_Pool = new List<HotKeyEvent>();
		private List<HotKeyEvent> _hotKeyEvents_Live = new List<HotKeyEvent>();


		private bool _isAnyEvent = false;


		private KeyCode _prevKey = KeyCode.None;

		//특수키의 딜레이 처리
		//특수키는 Up 이벤트 발생 후에도 아주 짧은 시간(0.3초)동안 Down 이벤트로 기록해야한다.
		public enum SPECIAL_KEY
		{
			Ctrl, Shift, Alt
		}
		private class SpecialKeyProcess
		{
			public SPECIAL_KEY _key;
			public bool _isPressed_Input;
			public bool _isPressed_Delayed;
			public System.Diagnostics.Stopwatch _timer = new System.Diagnostics.Stopwatch();

			private const long DELAY_TIME_MSEC = 300;//0.3초

			public SpecialKeyProcess(SPECIAL_KEY specialKey)
			{
				_key = specialKey;
				_isPressed_Input = false;
				_isPressed_Delayed = false;
			}

			public void OnKeyDown()
			{
				_isPressed_Input = true;
				_isPressed_Delayed = true;
			}

			public void OnKeyUp()
			{
				if (_isPressed_Input)
				{
					_isPressed_Input = false;
					_isPressed_Delayed = true;//일단은 True

					_timer.Reset();
					_timer.Start();
				}
			}

			public bool IsPressed()
			{
				if(_isPressed_Input)
				{
					//실제로 눌린 상태
					return true;
				}
				else
				{
					if(_isPressed_Delayed)
					{
						//일단은 딜레이 중이다.
						if(_timer.ElapsedMilliseconds > DELAY_TIME_MSEC)
						{
							_isPressed_Delayed = false;
							_timer.Stop();
						}
						//else
						//{
						//	Debug.Log("딜레이된 특수 키 : " + _key + " (" + (float)(_timer.ElapsedMilliseconds / 1000.0) + "s)");
						//}
					}

					return _isPressed_Delayed;

				}
			}

			public void ResetTimer()
			{
				if(!_isPressed_Input)
				{
					_isPressed_Delayed = false;
					_timer.Reset();
					_timer.Stop();
				}
			}
		}

		private Dictionary<SPECIAL_KEY, SpecialKeyProcess> _specialKeys = new Dictionary<SPECIAL_KEY, SpecialKeyProcess>();


		// Init
		//---------------------------------------------
		public apHotKey()
		{
			_isAnyEvent = false;
			_hotKeyEvents_Pool.Clear();
			_hotKeyEvents_Live.Clear();

			for (int i = 0; i < NUM_INIT_EVENT_POOL; i++)
			{
				_hotKeyEvents_Pool.Add(new HotKeyEvent());
			}
			_iEvent = 0;


			//_isLock = false;
			//_isSpecialKey_Ctrl = false;
			//_isSpecialKey_Shift = false;
			//_isSpecialKey_Alt = false;
			_prevKey = KeyCode.None;

			if(_specialKeys == null)
			{
				_specialKeys = new Dictionary<SPECIAL_KEY, SpecialKeyProcess>();
			}
			_specialKeys.Clear();

			_specialKeys.Add(SPECIAL_KEY.Ctrl, new SpecialKeyProcess(SPECIAL_KEY.Ctrl));
			_specialKeys.Add(SPECIAL_KEY.Alt, new SpecialKeyProcess(SPECIAL_KEY.Alt));
			_specialKeys.Add(SPECIAL_KEY.Shift, new SpecialKeyProcess(SPECIAL_KEY.Shift));
		}


		/// <summary>
		/// OnGUI 초기에 호출해주자
		/// </summary>
		public void Clear()
		{
			if (_isAnyEvent)
			{
				_isAnyEvent = false;
				_hotKeyEvents_Live.Clear();
				_iEvent = 0;
			}
		}


		// Input Event
		//------------------------------------------------------------------------------
		public apHotKey.HotKeyEvent OnKeyEvent(KeyCode keyCode, bool isCtrl, bool isShift, bool isAlt, bool isPressed)
		{
			if(keyCode == KeyCode.None)
			{
				return null;
			}
			
			if (isPressed)
			{
				//Pressed 이벤트의 경우, 너무 잦은 이벤트 호출이 문제다.
				//if (_isLock)
				//{
				//	Debug.LogWarning("키 입력 되었으나 Lock [" + keyCode + "]");
				//	return null;
				//}

				if (_prevKey == keyCode)
				{
					//Debug.Log(">> 이전 키와 같음 : " + keyCode);
					return null;
				}

				_prevKey = keyCode;
			}
			else
			{
				_prevKey = KeyCode.None;
			}

			//추가적으로, 유니티에서 제공하는 값에 따라서도 변동
			if (isPressed)
			{
				//Pressed인 경우 False > True로만 보정
				if (isCtrl)
				{
					_specialKeys[SPECIAL_KEY.Ctrl].OnKeyDown();
				}
				if (isShift)
				{
					_specialKeys[SPECIAL_KEY.Shift].OnKeyDown();
				}
				if (isAlt)
				{
					_specialKeys[SPECIAL_KEY.Alt].OnKeyDown();
				}
			}
			else
			{
				//Released인 경우 False > True로만 보정
				if (!isCtrl)
				{
					_specialKeys[SPECIAL_KEY.Ctrl].OnKeyUp();
				}
				if (!isShift)
				{
					_specialKeys[SPECIAL_KEY.Shift].OnKeyUp();
				}
				if (!isAlt)
				{
					_specialKeys[SPECIAL_KEY.Alt].OnKeyUp();
				}
			}

			switch (keyCode)
			{		
				// Special Key
#if UNITY_EDITOR_OSX
				case KeyCode.LeftCommand:
				case KeyCode.RightCommand:
#else
				case KeyCode.LeftControl:
				case KeyCode.RightControl:
#endif
					if(isPressed)
					{
						_specialKeys[SPECIAL_KEY.Ctrl].OnKeyDown();
					}
					else
					{
						_specialKeys[SPECIAL_KEY.Ctrl].OnKeyUp();
					}
					break;

				case KeyCode.LeftShift:
				case KeyCode.RightShift:
					if(isPressed)
					{
						_specialKeys[SPECIAL_KEY.Shift].OnKeyDown();
					}
					else
					{
						_specialKeys[SPECIAL_KEY.Shift].OnKeyUp();
					}
					break;

				case KeyCode.LeftAlt:
				case KeyCode.RightAlt:
					if(isPressed)
					{
						_specialKeys[SPECIAL_KEY.Alt].OnKeyDown();
					}
					else
					{
						_specialKeys[SPECIAL_KEY.Alt].OnKeyUp();
					}
					break;

				default:
					//그 외의 키값이라면..
					//Up 이벤트에만 반응하자 > 변경
					//특수키가 있는 단축키 => Up 이벤트에서만 적용
					//특수키가 없는 단축키 => Down 이벤트에서만 적용
					//if (!isPressed)
					{
						//해당하는 이벤트가 있는가?
						apHotKey.HotKeyEvent hotkeyEvent = CheckHotKeyEvent(keyCode, 
																			_specialKeys[SPECIAL_KEY.Shift].IsPressed(), 
																			_specialKeys[SPECIAL_KEY.Alt].IsPressed(), 
																			_specialKeys[SPECIAL_KEY.Ctrl].IsPressed(),
																			isPressed);

						//딜레이 처리가 끝났다면 특수키 타이머를 리셋한다.
						_specialKeys[SPECIAL_KEY.Shift].ResetTimer();
						_specialKeys[SPECIAL_KEY.Alt].ResetTimer();
						_specialKeys[SPECIAL_KEY.Ctrl].ResetTimer();

						if (hotkeyEvent != null)
						{
							////일단 이 메인 키를 누른 상태에서 Lock을 건다.
							//_isLock = true;

							

							return hotkeyEvent;
						}
					}
					
					break;
			}

			return null;
			
		}




		#region [미사용 코드]
		//		public apHotKey.HotKeyEvent OnKeyDown(KeyCode keyCode, bool isCtrl, bool isShift, bool isAlt)
		//		{
		//			Debug.Log("OnKeyDown : " + keyCode);
		//			if(keyCode == KeyCode.None)
		//			{
		//				return null;
		//			}

		//			if(_isLock)
		//			{
		//				Debug.LogWarning("키 입력 되었으나 Lock [" + keyCode + "]");
		//				return null;
		//			}

		//			if(_prevKey == keyCode)
		//			{
		//				Debug.Log(">> 이전 키와 같음 : " + keyCode);
		//				return null;
		//			}

		//			_prevKey = keyCode;

		//			Debug.LogWarning("Key Down : " + keyCode);


		//			//추가적으로, 유니티에서 제공하는 값에 따라서도 변동
		//			if(isCtrl)
		//			{
		//				_isSpecialKey_Ctrl = true;
		//			}
		//			if(isShift)
		//			{
		//				_isSpecialKey_Shift = true;
		//			}
		//			if (isAlt)
		//			{
		//				_isSpecialKey_Alt = true;
		//			}

		//			switch (keyCode)
		//			{		
		//				// Special Key
		//#if UNITY_EDITOR_OSX
		//				case KeyCode.LeftCommand:
		//				case KeyCode.RightCommand:
		//#else
		//				case KeyCode.LeftControl:
		//				case KeyCode.RightControl:
		//#endif
		//					_isSpecialKey_Ctrl = true;
		//					break;

		//				case KeyCode.LeftShift:
		//				case KeyCode.RightShift:
		//					_isSpecialKey_Shift = true;
		//					break;

		//				case KeyCode.LeftAlt:
		//				case KeyCode.RightAlt:
		//					_isSpecialKey_Alt = true;
		//					break;

		//				default:
		//					//그 외의 키값이라면..
		//					_mainKey = keyCode;

		//					//해당하는 이벤트가 있는가?
		//					apHotKey.HotKeyEvent hotkeyEvent = CheckHotKeyEvent(_mainKey, _isSpecialKey_Shift, _isSpecialKey_Alt, _isSpecialKey_Ctrl);
		//					if(hotkeyEvent != null)
		//					{
		//						//일단 이 메인 키를 누른 상태에서 Lock을 건다.
		//						_isLock = true;

		//						return hotkeyEvent;
		//					}
		//					break;
		//			}

		//			return null;

		//		}

		//		public void OnKeyUp(KeyCode keyCode, bool isCtrl, bool isShift, bool isAlt)
		//		{
		//			if(keyCode == KeyCode.None)
		//			{
		//				return;
		//			}

		//			//추가적으로, 유니티에서 제공하는 값에 따라서도 변동 (False로만)
		//			if(!isCtrl)
		//			{
		//				_isSpecialKey_Ctrl = false;
		//			}
		//			if(!isShift)
		//			{
		//				_isSpecialKey_Shift = false;
		//			}
		//			if (!isAlt)
		//			{
		//				_isSpecialKey_Alt = false;
		//			}

		//			Debug.LogError("Key Up : " + keyCode);
		//			_prevKey = KeyCode.None;

		//			//Lock을 풀 수 있을까
		//			switch (keyCode)
		//			{		
		//				// Special Key
		//#if UNITY_EDITOR_OSX
		//				case KeyCode.LeftCommand:
		//				case KeyCode.RightCommand:
		//#else
		//				case KeyCode.LeftControl:
		//				case KeyCode.RightControl:
		//#endif
		//					_isSpecialKey_Ctrl = false;
		//					break;

		//				case KeyCode.LeftShift:
		//				case KeyCode.RightShift:
		//					_isSpecialKey_Shift = false;
		//					break;

		//				case KeyCode.LeftAlt:
		//				case KeyCode.RightAlt:
		//					_isSpecialKey_Alt = false;
		//					break;

		//				default:
		//					if(keyCode == _mainKey)
		//					{
		//						//Lock을 풀자
		//						_isLock = false;

		//						Debug.Log("[" + _mainKey + "] 단축키 Lock 해제됨");
		//						_mainKey = KeyCode.ScrollLock;
		//					}
		//					break;
		//			}

		//		} 
		#endregion

		// Functions
		//------------------------------------------------------------------------------
		public void AddHotKeyEvent(FUNC_HOTKEY_EVENT funcEvent, string label, KeyCode keyCode, bool isShift, bool isAlt, bool isCtrl, object paramObject)
		{
			_hotKeyEvents_Live.Add(PopEvent(funcEvent, label, keyCode, isShift, isAlt, isCtrl, paramObject));
			_isAnyEvent = true;
		}

		// 변경 3.25 : 매번 생성하는 방식에서 Pop 방식으로 변경
		private HotKeyEvent PopEvent(FUNC_HOTKEY_EVENT funcEvent, string label, KeyCode keyCode, bool isShift, bool isAlt, bool isCtrl, object paramObject)
		{
			if(_iEvent >= _hotKeyEvents_Pool.Count)
			{
				//두개씩 늘리자
				for (int i = 0; i < 2; i++)
				{
					_hotKeyEvents_Pool.Add(new HotKeyEvent());
				}

				//Debug.Log("입력 풀 부족 : " + _hotKeyEvents_Pool.Count + " [" + label + "]");
			}

			HotKeyEvent result = _hotKeyEvents_Pool[_iEvent];
			_iEvent++;
			result.SetEvent(funcEvent, label, keyCode, isShift, isAlt, isCtrl, paramObject);
			return result;
		}

		/// <summary>
		/// OnGUI 후반부에 체크해준다.
		/// Event가 used가 아니라면 호출 가능
		/// </summary>
		/// <param name=""></param>
		public HotKeyEvent CheckHotKeyEvent(KeyCode keyCode, bool isShift, bool isAlt, bool isCtrl, bool isPressed)
		{
			if (!_isAnyEvent)
			{
				return null;
			}

			
			HotKeyEvent hkEvent = null;
			for (int i = 0; i < _hotKeyEvents_Live.Count; i++)
			{
				hkEvent = _hotKeyEvents_Live[i];

				//Pressed 이벤트 = 단일 키
				//Released 이벤트 = 조합 키
				// 위조건이 안맞으면 continue
				if((isPressed && hkEvent._isCombination)
					|| (!isPressed && !hkEvent._isCombination))
				{
					//조합키인데 Pressed 이벤트이거나
					//단일키인데 Released 이벤트라면
					//패스
					continue;
				}

				if (hkEvent._keyCode == keyCode &&
					hkEvent._isShift == isShift &&
					hkEvent._isAlt == isAlt &&
					hkEvent._isCtrl == isCtrl)
				{
					try
					{
						//저장된 이벤트를 실행하자
						hkEvent._funcEvent(hkEvent._paramObject);

						return hkEvent;
					}
					catch (Exception ex)
					{
						Debug.LogError("HotKey Event Exception : " + ex);
						return null;
					}
				}
			}
			return null;
		}





		// Get / Set
		//---------------------------------------------
	}

}