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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.Profiling;
#endif
using System;

using AnyPortrait;


namespace AnyPortrait
{
	//Important를 끈 객체가 간헐적인 업데이트를 할 때, 
	public class apOptUpdateChecker
	{
		// Const
		//---------------------------------------------------
		private const int MAX_FPS = 60;
		private const int MIN_FPS = 2;
		//private const int MAX_INV_LOW_FPS_RATIO = 4;

		private const int REMOVABLE_COUNT = 100;

		// SubClass
		//---------------------------------------------------
		public class UpdateToken
		{
			private int _FPS = -1;
			public int _delayedFrame = 0;
			

			private bool _result = false;
			private float _elapsedTime = 0.0f;
			private float _updatableTimeLength = 0.0f;

			private float _resultTime = 0.0f;
			private bool _isOverDelayed = false;
			private int _resultDelayedFrame = 0;



			public UpdateToken(int fps)
			{
				_FPS = Mathf.Clamp(fps, MIN_FPS, MAX_FPS);
				_delayedFrame = 0;
				_result = false;
				_elapsedTime = 0.0f;
				_resultTime = 0.0f;

				_updatableTimeLength = (1.0f / (float)_FPS) - 0.01f;//Bias만큼 약간 더 감소
			}

			public void SetFPS(int fps)
			{
				if(fps != _FPS)
				{
					_FPS = Mathf.Clamp(fps, MIN_FPS, MAX_FPS);
					_updatableTimeLength = (1.0f / (float)_FPS) - 0.01f;
				}
			}

			public void UpdateTime(float deltaTime)
			{
				_elapsedTime += deltaTime;
			}

			public bool IsUpdatable()
			{
				return _elapsedTime > _updatableTimeLength;
			}

			public bool IsUpdatableInLowSceneFPS(float lowFPSRatio)
			{
				return _elapsedTime > (_updatableTimeLength / lowFPSRatio) - 0.01f;//Bias만큼 약간 더 감소
			}

			public void ReadyToCalculate()
			{
				_result = false;
				_resultTime = 0.0f;
				_isOverDelayed = false;
				_resultDelayedFrame = 0;
			}

			public void SetSuccess(bool isOverDelayed)
			{
				_result = true;
				_resultTime = _elapsedTime;
				_elapsedTime = 0.0f;

				
				_resultDelayedFrame = _delayedFrame;
				_isOverDelayed = isOverDelayed;

				_delayedFrame = 0;
			}

			
			public bool SetFailIfNotCalculated()
			{
				if(_result)
				{
					return false;
				}

				_resultTime = 0.0f;
				_delayedFrame++;

				return true;
			}

			public bool IsSuccess { get { return _result; } }
			public float ResultElapsedTime { get { return _resultTime; } }
			public bool IsOverDelayed { get { return _isOverDelayed; } }
			public int DelayedFrame {  get {  return _resultDelayedFrame; } }
		}

		public class TokenList
		{
			public int _FPS = -1;
			
			private List<UpdateToken> _tokens = null;

			private int _nRequest = 0;//<<요청 개수
			private int _maxDelay = 0;//<<몇개의 그룹으로 분할해야 하는가
			//private int _maxDelayLowSceneFPS = 0;//<<몇개의 그룹으로 분할해야 하는가

			private int _maxCount = -1;
			private int _successCount = -1;

			private Dictionary<int, List<UpdateToken>> _delayedTokens = null;//바로 처리되지 못하고 잠시 딜레이된 토큰들. 계산용

			//private float _tSceneFrame = 0.0f;//유니티 씬에서의 Frame 시간
			private int _sceneFPS = 0;

			private float _countBias = 1.0f;

			private float _tCycle = 0.0f;
			private float _tCycleLength = 0.0f;

			private int _cycle_totalRequests = 0;
			private int _cycle_totalFailed = 0;
			private int _cycle_totalMargin = 0;
			
			//private int _prevBias = 0;

			//만약 SceneFPS가 목표된 FPS보다 낮은 경우 (이건 Cycle마다 체크한다)
			private bool _isLowSceneFPS = false;
			private int _lowSceneFPS = 0;

			//private int _lowFPS_Min = 0;

			private int _lowFPS = 0;
			private int _avgSceneFPS = 0;
			private int _nSceneFPSCount = 0;
			private float _lowFPSRatio = 0.0f;

			//삭제 가능성
			//긴 프레임동안 Request가 없었던 토큰 리스트는 삭제되어야 한다.
			private bool _isRemovable = false;
			private int _nNoRequestFrames = 0;



			public TokenList(int fps)
			{
				_FPS = Mathf.Clamp(fps, MIN_FPS, MAX_FPS);
				_maxDelay = Mathf.Max((MAX_FPS / _FPS) + 1, 2);
				
				_tokens = new List<UpdateToken>();
				_delayedTokens = new Dictionary<int, List<UpdateToken>>();
				for (int i = 0; i < _maxDelay; i++)
				{
					_delayedTokens.Add(i, new List<UpdateToken>());
				}

				_tCycle = 0.0f;
				_tCycleLength = 1.0f / (float)_FPS;

				_isLowSceneFPS = false;
				_lowSceneFPS = 0;
				_avgSceneFPS = 0;
				_nSceneFPSCount = 0;

				_isRemovable = false;
				_nNoRequestFrames = 0;
			}

			public void Reset(float deltaTime)
			{
				_nRequest = 0;
				_maxCount = 0;
				_successCount = 0;
				
				if(deltaTime > 0.0f)
				{
					_sceneFPS = (int)(1.0f / deltaTime);
				}
				else
				{
					_sceneFPS = MIN_FPS;
				}

				_avgSceneFPS += _sceneFPS;
				_nSceneFPSCount++;
				
				_tCycle += deltaTime;
				if(_tCycle > _tCycleLength)
				{
					// 이 토큰의 사이클이 한바퀴 돌았다
					//크기 보정 배수를 재계산하자
					if(_cycle_totalRequests == 0)
					{
						_countBias = 1.0f;
						
					}
					else
					{
						//보정값
						//처리 횟수
						//성공 + 실패 = 전체
						//maxSize대비 -> 
						//1 + ((실패 - 잉여) / 전체) + 0.5 (Bias)
						//단, 실패에 약간의 가중치가 더 붙는다.
						//float newCountBias = 1.5f + ((float)(_cycle_totalFailed * 1.5f - _cycle_totalMargin * 0.5f) / (float)_cycle_totalRequests );
						//float newCountBias = 1.0f;
						//float prevBias = _countBias;
						//float newCountBias = _countBias;
						if (_cycle_totalFailed > 0)
						{
							//newCountBias += ((float)(_cycle_totalFailed) / (float)_cycle_totalRequests);
							_countBias *= 1.1f;

							//Debug.LogError("[" + _FPS  + "] 배수 증가 : " + prevBias + " > " + _countBias);
							
						}
						else if (_cycle_totalMargin > 0)
						{
							//newCountBias -= ((float)(_cycle_totalMargin) / (float)_cycle_totalRequests);
							_countBias *= 0.95f;

							//Debug.LogWarning("[" + _FPS  + "] 배수 감소 : " + prevBias + " > " + _countBias);
						}
					}
					_cycle_totalRequests = 0;
					_cycle_totalFailed = 0;
					_cycle_totalMargin = 0;
					
					_tCycle = 0.0f;

					if(_nSceneFPSCount > 0)
					{
						_avgSceneFPS = _avgSceneFPS / _nSceneFPSCount;
						if(_avgSceneFPS / 2 < _FPS)
						{
							//실행중인 프레임이 매우 낮아서 실제 업데이트되는 FPS를 낮추어야 한다.
							//실제 FPS는 그 절반으로 낮추어야 한다.
							
							_lowSceneFPS = _avgSceneFPS;
							_lowFPS = _lowSceneFPS / 2;

							if(_lowFPS < MIN_FPS)
							{
								_lowFPS = MIN_FPS;
							}
							
							_lowFPSRatio = (float)_lowFPS / (float)_FPS;

							//if (!_isLowSceneFPS)
							//{
							//	Debug.LogWarning("[" + _FPS + "] Low Scene FPS : " + _FPS + " >> " + _lowFPS);
							//}

							_isLowSceneFPS = true;
						}
						else
						{	
							_lowSceneFPS = 0;
							_lowFPSRatio = 1.0f;
							_lowFPS = _FPS;

							//if (_isLowSceneFPS)
							//{
							//	Debug.Log("[" + _FPS + "] Recover Scene FPS");
							//}

							_isLowSceneFPS = false;
						}
					}
					else
					{
						_isLowSceneFPS = false;
						_lowSceneFPS = 0;
						_lowFPSRatio = 1.0f;
						_lowFPS = _FPS;
					}

					_avgSceneFPS = 0;
					_nSceneFPSCount = 0;
				}

				_tokens.Clear();

				for (int i = 0; i < _maxDelay; i++)
				{
					_delayedTokens[i].Clear();
				}
			}

			public void AddRequest(UpdateToken token)
			{
				//token._result = false;//<<일단 False로 설정
				//token.ReadyToCalculate();//계산 준비 //<< AddRequest가 호출되기 전에 이미 호출되었다.

				if(_isLowSceneFPS)
				{
					//만약, 현재 Scene의 FPS가 낮다면,
					//이 토큰의 업데이트 여부를 한번더 체크해야한다.
					if(token.IsUpdatableInLowSceneFPS(_lowFPSRatio))
					{
						_nRequest++;
						_tokens.Add(token);
					}
				}
				else
				{
					_nRequest++;
					_tokens.Add(token);
				}

				
			}

			public bool Calculate()
			{
				//이게 핵심. 
				//- maxCount 결정 + curCount = 0

				//- 요청된 토큰의 각각의 가중치를 보고 처리할 수 있는지 여부를 결정
				//1. DelayedFrame이 divide를 넘었으면 무조건 처리 => Sort 필요 없음
				//2. DelayedFrame이 큰것 부터 처리. curCount가 maxCount보다 크면 종료

				//- result가 true인 토큰은 delayedFrame을 0으로 초기화
				//- result가 false인 토큰은 delayedFrame을 1 증가
				
				if(_nRequest == 0)
				{
					if (!_isRemovable)
					{
						_nNoRequestFrames++;

						if (_nNoRequestFrames > REMOVABLE_COUNT)
						{
							_isRemovable = true;
						}
					}
					return _isRemovable;
				}

				_isRemovable = false;
				_nNoRequestFrames = 0;

				UpdateToken token = null;

				//프레임이 너무 낮은 경우 전부 OverDelayed를 하고 처리하자
				if(_sceneFPS < MIN_FPS)
				{
					for (int i = 0; i < _tokens.Count; i++)
					{
						token = _tokens[i];
						if (token == null)
						{
							return false;
						}

						token.SetSuccess(true);
						_successCount++;
					}

					_cycle_totalFailed += _nRequest;
					return false;
				}

				//Slot의 크기를 구하자
				//- 기본 : 전체 요청 / (현재 프레임 / FPS) => (전체 요청 * FPS) / 현재 프레임
				
				if(_isLowSceneFPS)
				{
					_maxCount = ((_nRequest * _lowFPS) / _sceneFPS) + 1;
				}
				else
				{
					_maxCount = ((_nRequest * _FPS) / _sceneFPS) + 1;
				}

				_maxCount = (int)(_maxCount * _countBias + 0.5f);//<<알아서 바뀌는 보정값으로 변경
				

				_successCount = 0;
				
				//1차로 : 무조건 처리해야하는거 찾기
				for (int i = 0; i < _tokens.Count; i++)
				{
					token = _tokens[i];
					if(token == null)
					{
						continue;
					}
					
					if (token._delayedFrame >= _maxDelay)
					{
						//한계치를 넘었다. > 무조건 성공
						//token._result = true;
						token.SetSuccess(true);
						_successCount++;
					}
					else
					{
						//일단 뒤로 미루자
						_delayedTokens[token._delayedFrame].Add(token);
					}
				}

				if(_successCount < _maxCount)
				{
					//아직 더 처리할 수 있다면
					//delayedFrame이 큰것부터 처리하자
					List<UpdateToken> delayedList = null;
					for (int iDelay = _maxDelay - 1; iDelay >= 0; iDelay--)
					{	
						delayedList = _delayedTokens[iDelay];
						for (int i = 0; i < delayedList.Count; i++)
						{
							token = delayedList[i];
							//token._result = true;
							token.SetSuccess(false);
							_successCount++;

							//처리가 모두 끝났으면 리턴
							if(_successCount >= _maxCount)
							{
								break;
							}
						}

						//처리가 모두 끝났으면 리턴
						if(_successCount >= _maxCount)
						{
							break;
						}
					}
				}

				int nFailed = 0;
				for (int i = 0; i < _tokens.Count; i++)
				{
					token = _tokens[i];
					//<<처리되지 않았다면 Fail 처리
					if(token.SetFailIfNotCalculated())
					{
						nFailed++;
					}
				}

				_cycle_totalRequests += _nRequest;
				
				//실패값
				if (_nRequest > _maxCount)
				{
					_cycle_totalFailed += _nRequest - _maxCount;
				}
				
				//잉여값
				if (_nRequest < _maxCount)
				{
					_cycle_totalMargin += _maxCount - _nRequest;
				}

				return false;
			}

			public bool IsRemovable
			{
				get { return _isRemovable; }
			}
		}


		// Members
		//---------------------------------------------------
		private static apOptUpdateChecker _instance = new apOptUpdateChecker();
		public static apOptUpdateChecker I { get { return _instance; } }

		private Dictionary<int, TokenList> _fps2Tokens = new Dictionary<int, TokenList>();

		private enum STATE
		{
			Ready, Update, LateUpdate
		}
		private STATE _state = STATE.Ready;

		// Init
		//---------------------------------------------------
		private apOptUpdateChecker()
		{
			//초기화
			_fps2Tokens.Clear();
			_state = STATE.Ready;
		}


		// Functions
		//---------------------------------------------------
		//Update에서 호출하는 함수와
		//LateUpdate에서 호출하는 함수 2개로 나뉜다.
		/// <summary>
		/// 이 함수를 Update에서 호출하자
		/// 토큰이 없다면 null로 하되, 리턴값을 멤버로 가지고 있자
		/// </summary>
		/// <param name="token"></param>
		/// <param name="fps"></param>
		/// <returns></returns>
		public UpdateToken AddRequest(UpdateToken token, int fps, float deltaTime)
		{
			//상태가 바뀌면 초기화를 해야한다.
			if(_state != STATE.Update)
			{
#if UNITY_EDITOR
				Profiler.BeginSample("AddRequest > Reset");
#endif
				//_fps2Tokens.Clear();
				foreach (KeyValuePair<int, TokenList> keyValuePair in _fps2Tokens)
				{
					keyValuePair.Value.Reset(deltaTime);
				}

				_state = STATE.Update;

#if UNITY_EDITOR
				Profiler.EndSample();
#endif
			}

			fps = Mathf.Clamp(fps, MIN_FPS, MAX_FPS);

			if(token == null)
			{
				token = new UpdateToken(fps);
			}
			else
			{
				token.SetFPS(fps);
			}
			token.UpdateTime(deltaTime);

			token.ReadyToCalculate();

			if (token.IsUpdatable())
			{
				//업데이트될 수 있다면 토큰을 리스트에 넣자
				if (_fps2Tokens.ContainsKey(fps))
				{
					_fps2Tokens[fps].AddRequest(token);
				}
				else
				{
					TokenList newTokenList = new TokenList(fps);

					newTokenList.Reset(deltaTime);
					_fps2Tokens.Add(fps, newTokenList);
					newTokenList.AddRequest(token);

					//Debug.Log("New Token List : " + fps);
				}
			}

			return token;
		}

		/// <summary>
		/// 이 함수를 LateUpdate에서 호출하자. True면 업데이트 할 수 있다.
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public bool GetUpdatable(UpdateToken token)
		{
			if(token == null)
			{
				return false;
			}
			if(_state != STATE.LateUpdate)
			{
#if UNITY_EDITOR
				Profiler.BeginSample("Calculate > Reset");
#endif
				//_fps2Tokens.Clear();
				bool isAnyRemovableList = false;
				List<int> removalbeFPS = null;
				foreach (KeyValuePair<int, TokenList> keyValuePair in _fps2Tokens)
				{
					if(keyValuePair.Value.Calculate())
					{
						//삭제할 게 있다.
						isAnyRemovableList = true;
						if(removalbeFPS == null)
						{
							removalbeFPS = new List<int>();
						}
						removalbeFPS.Add(keyValuePair.Key);
					}
				}

				//삭제해야할 때도 있다.
				if(isAnyRemovableList)
				{
					for (int i = 0; i < removalbeFPS.Count; i++)
					{
						Debug.Log("Token List 삭제 : " + removalbeFPS[i]);
						_fps2Tokens.Remove(removalbeFPS[i]);
					}
				}

				

				_state = STATE.LateUpdate;

#if UNITY_EDITOR
				Profiler.EndSample();
#endif
			}

			//return token._result;
			return token.IsSuccess;
		}


		// Get / Set
		//---------------------------------------------------


	}
}