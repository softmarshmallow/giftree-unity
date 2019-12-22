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
using System;

using AnyPortrait;

namespace AnyPortrait
{
	/// <summary>
	/// TimelineUI에서 여러개의 키프레임들을 선택했을 때, 한번에 "동기화된" 커브 편집을 위한 클래스
	/// 여러개의 키프레임들의 커브를 Prev/Next로 구분하여 동기화 여부를 테스트한 뒤,
	/// 편집을 위한 가상 커브를 제공한다.
	/// </summary>
	public class apTimelineCommonCurve
	{
		// Members
		//----------------------------------------------------
		public enum SYNC_STATUS
		{
			/// <summary>키프레임들이 없다.</summary>
			NoKeyframes,
			/// <summary>키프레임은 있지만 동기화가 안되었다.</summary>
			NotSync,
			/// <summary>동기화가 된 상태</summary>
			Sync,
		}
		private SYNC_STATUS _syncStatus = SYNC_STATUS.NoKeyframes;
		public List<apAnimKeyframe> _keyframes = new List<apAnimKeyframe>();

		//두개의 키프레임을 저장하고, 두개의 커브(A, B)를 확인한다.
		//동기와 체크 및 동기화에 사용된다.
		public class CurveSet
		{
			public apAnimKeyframe _prevKeyframe = null;
			public apAnimKeyframe _nextKeyframe = null;

			public CurveSet(apAnimKeyframe prevKeyframe, apAnimKeyframe nextKeyframe)
			{
				_prevKeyframe = prevKeyframe;
				_nextKeyframe = nextKeyframe;
			}

			public bool IsSame(CurveSet target)
			{
				//Prev, Next간의 CurveResult의 값을 비교한다.
				apAnimCurve src_A = _prevKeyframe._curveKey;
				apAnimCurve src_B = _nextKeyframe._curveKey;

				apAnimCurve dst_A = target._prevKeyframe._curveKey;
				apAnimCurve dst_B = target._nextKeyframe._curveKey;

				//A의 Next와 B의 Prev끼리 묶어서 같은지 확인한다.
				if(src_A._nextTangentType != dst_A._nextTangentType ||
					src_B._prevTangentType != dst_B._prevTangentType)
				{
					return false;
				}

				apAnimCurve.TANGENT_TYPE srcTangentType = apAnimCurve.TANGENT_TYPE.Constant;
				apAnimCurve.TANGENT_TYPE dstTangentType = apAnimCurve.TANGENT_TYPE.Constant;

				//실제 탄젠트 타입 비교
				if(src_A._nextTangentType == apAnimCurve.TANGENT_TYPE.Constant &&
					src_B._prevTangentType == apAnimCurve.TANGENT_TYPE.Constant)
				{
					srcTangentType = apAnimCurve.TANGENT_TYPE.Constant;
				}
				else if(src_A._nextTangentType == apAnimCurve.TANGENT_TYPE.Linear &&
					src_B._prevTangentType == apAnimCurve.TANGENT_TYPE.Linear)
				{
					srcTangentType = apAnimCurve.TANGENT_TYPE.Linear;
				}
				else
				{
					srcTangentType = apAnimCurve.TANGENT_TYPE.Smooth;
				}

				if(dst_A._nextTangentType == apAnimCurve.TANGENT_TYPE.Constant &&
					dst_B._prevTangentType == apAnimCurve.TANGENT_TYPE.Constant)
				{
					dstTangentType = apAnimCurve.TANGENT_TYPE.Constant;
				}
				else if(dst_A._nextTangentType == apAnimCurve.TANGENT_TYPE.Linear &&
					dst_B._prevTangentType == apAnimCurve.TANGENT_TYPE.Linear)
				{
					dstTangentType = apAnimCurve.TANGENT_TYPE.Linear;
				}
				else
				{
					dstTangentType = apAnimCurve.TANGENT_TYPE.Smooth;
				}

				if(srcTangentType != dstTangentType)
				{
					return false;
				}

				if(srcTangentType == apAnimCurve.TANGENT_TYPE.Smooth &&
					dstTangentType == apAnimCurve.TANGENT_TYPE.Smooth)
				{
					//Smooth의 경우, Smooth에 사용된 값도 같아야 한다.
					float bias = 0.001f;
					if(Mathf.Abs(src_A._nextSmoothX - dst_A._nextSmoothX) > bias ||
						Mathf.Abs(src_A._nextSmoothY - dst_A._nextSmoothY) > bias ||
						Mathf.Abs(src_B._prevSmoothX - dst_B._prevSmoothX) > bias ||
						Mathf.Abs(src_B._prevSmoothY - dst_B._prevSmoothY) > bias)
					{
						return false;
					}
				}

				return true;
			}
		}

		private List<CurveSet> _curveSets = new List<CurveSet>();
		private Dictionary<apAnimKeyframe, CurveSet> _prevKey2CurveSet = new Dictionary<apAnimKeyframe, CurveSet>();
		private Dictionary<apAnimKeyframe, CurveSet> _nextKey2CurveSet = new Dictionary<apAnimKeyframe, CurveSet>();

		//동기화된 커브 2개
		public apAnimCurve _syncCurve_Prev = new apAnimCurve();
		public apAnimCurve _syncCurve_Next = new apAnimCurve();
		public apAnimCurveResult SyncCurveResult
		{
			get { return _syncCurve_Prev._nextCurveResult; }
		}

		private bool _isAnyChangedRequest = false;


		// Init
		//----------------------------------------------------
		public apTimelineCommonCurve()
		{
			Clear();
		}


		public void Clear()
		{
			_syncStatus = SYNC_STATUS.NoKeyframes;

			if (_keyframes == null)
			{
				_keyframes = new List<apAnimKeyframe>();
			}
			_keyframes.Clear();

			if (_curveSets == null)
			{
				_curveSets = new List<CurveSet>();
			}
			_curveSets.Clear();

			if (_prevKey2CurveSet == null)
			{
				_prevKey2CurveSet = new Dictionary<apAnimKeyframe, CurveSet>();
			}
			if (_nextKey2CurveSet == null)
			{
				_nextKey2CurveSet = new Dictionary<apAnimKeyframe, CurveSet>();
			}
			_prevKey2CurveSet.Clear();
			_nextKey2CurveSet.Clear();

			_isAnyChangedRequest = false;

			_syncCurve_Prev.Init();
			_syncCurve_Next.Init();

			_syncCurve_Prev._keyIndex = 0;
			_syncCurve_Next._keyIndex = 1;

			_syncCurve_Prev._nextIndex = 1;
			_syncCurve_Next._prevIndex = 0;

			//이전
			//_syncCurve_Prev._nextCurveResult.Link(_syncCurve_Prev, _syncCurve_Next, true, true);
			//_syncCurve_Next._prevCurveResult.Link(_syncCurve_Prev, _syncCurve_Next, false, true);

			//변경 19.5.20 : MakeCurve를 항상 하는 걸로 변경
			_syncCurve_Prev._nextCurveResult.Link(_syncCurve_Prev, _syncCurve_Next, true);
			_syncCurve_Next._prevCurveResult.Link(_syncCurve_Prev, _syncCurve_Next, false);

			_syncCurve_Prev.Refresh();
			_syncCurve_Next.Refresh();
		}


		// Functions
		//----------------------------------------------------
		public void SetKeyframes(List<apAnimKeyframe> keyframes)
		{
			Clear();

			//키프레임들을 하나씩 돌면서 CurveSet에 넣자.
			//중복을 막기 위해서 Key2CurveSet 확인
			if(keyframes == null || keyframes.Count == 0)
			{
				return;
			}

			apAnimKeyframe srcKeyframe = null;
			apAnimKeyframe prevKey = null;
			apAnimKeyframe nextKey = null;
			for (int iKey = 0; iKey < keyframes.Count; iKey++)
			{
				srcKeyframe = keyframes[iKey];

				_keyframes.Add(srcKeyframe);

				prevKey = srcKeyframe._prevLinkedKeyframe;
				nextKey = srcKeyframe._nextLinkedKeyframe;

				srcKeyframe._curveKey.Refresh();

				if(prevKey != null && srcKeyframe != prevKey)
				{
					

					//Prev -> Src
					if(!_prevKey2CurveSet.ContainsKey(prevKey) && !_nextKey2CurveSet.ContainsKey(srcKeyframe))
					{
						//아직 등록되지 않은 키프레임들
						CurveSet newSet = new CurveSet(prevKey, srcKeyframe);
						_curveSets.Add(newSet);

						_prevKey2CurveSet.Add(prevKey, newSet);
						_nextKey2CurveSet.Add(srcKeyframe, newSet);
					}
				}

				if(nextKey != null && srcKeyframe != nextKey)
				{
					//Src -> Next
					if(!_prevKey2CurveSet.ContainsKey(srcKeyframe) && !_nextKey2CurveSet.ContainsKey(nextKey))
					{
						//아직 등록되지 않은 키프레임들
						CurveSet newSet = new CurveSet(srcKeyframe, nextKey);
						_curveSets.Add(newSet);

						_prevKey2CurveSet.Add(srcKeyframe, newSet);
						_nextKey2CurveSet.Add(nextKey, newSet);
					}
				}
			}

			if(_curveSets.Count <= 1)
			{
				Clear();
				return;
			}

			//일단 동기화가 안됨
			_syncStatus = SYNC_STATUS.NotSync;
			
			//커브값이 같은지 체크하자
			CurveSet firstSet = _curveSets[0];//<<첫번째것과 비교하자

			bool isAllSame = true;//<<모두 동일한 커브를 가졌는가

			CurveSet curSet = null;
			for (int i = 1; i < _curveSets.Count; i++)
			{
				curSet = _curveSets[i];
				if(!curSet.IsSame(firstSet))
				{
					//하나라도 다르다면
					isAllSame = false;
					break;
				}
			}

			if(isAllSame)
			{
				//오잉 모두 같았다.
				//공통 Curve를 만든다.
				_syncStatus = SYNC_STATUS.Sync;

				//공통 Curve를 만들자
				_syncCurve_Prev._nextLinkedCurveKey = _syncCurve_Next;
				_syncCurve_Next._prevLinkedCurveKey = _syncCurve_Prev;

				_syncCurve_Prev._keyIndex = 0;
				_syncCurve_Next._keyIndex = 1;

				_syncCurve_Prev._nextIndex = 1;
				_syncCurve_Next._prevIndex = 0;

				apAnimCurve prevCurve = firstSet._prevKeyframe._curveKey;
				apAnimCurve nextCurve = firstSet._nextKeyframe._curveKey;

				_syncCurve_Prev._nextSmoothX = prevCurve._nextSmoothX;
				_syncCurve_Prev._nextSmoothY = prevCurve._nextSmoothY;
				_syncCurve_Prev._nextTangentType = prevCurve._nextTangentType;

				_syncCurve_Next._prevSmoothX = nextCurve._prevSmoothX;
				_syncCurve_Next._prevSmoothY = nextCurve._prevSmoothY;
				_syncCurve_Next._prevTangentType = nextCurve._prevTangentType;

				//이전
				//_syncCurve_Prev._nextCurveResult.Link(_syncCurve_Prev, _syncCurve_Next, true, true);
				//_syncCurve_Next._prevCurveResult.Link(_syncCurve_Prev, _syncCurve_Next, false, true);

				//변경 19.5.20 : MakeCurve를 항상 수행
				_syncCurve_Prev._nextCurveResult.Link(_syncCurve_Prev, _syncCurve_Next, true);
				_syncCurve_Next._prevCurveResult.Link(_syncCurve_Prev, _syncCurve_Next, false);

				_syncCurve_Prev.Refresh();
				_syncCurve_Next.Refresh();

				_syncCurve_Prev._nextCurveResult.MakeCurve();
				_syncCurve_Next._prevCurveResult.MakeCurve();
			}
			else
			{
				//몇개가 다르다.
				_syncStatus = SYNC_STATUS.NotSync;

				_syncCurve_Prev.Refresh();
				_syncCurve_Next.Refresh();
			}
		}

		//--------------------------------------------------------

		public void SetTangentType(apAnimCurve.TANGENT_TYPE tangentType)
		{
			if(_syncStatus != SYNC_STATUS.Sync)
			{
				return;
			}


			SyncCurveResult.SetTangent(tangentType);

			SetChanged();
			ApplySync(true, false);
		}

		public void SetCurvePreset_Default()
		{
			if(_syncStatus != SYNC_STATUS.Sync)
			{
				return;
			}

			SyncCurveResult.SetCurvePreset_Default();

			SetChanged();
			ApplySync(true, false);
		}

		public void SetCurvePreset_Hard()
		{
			if(_syncStatus != SYNC_STATUS.Sync)
			{
				return;
			}

			SyncCurveResult.SetCurvePreset_Hard();

			SetChanged();
			ApplySync(true, false);
		}

		public void SetCurvePreset_Acc()
		{
			if(_syncStatus != SYNC_STATUS.Sync)
			{
				return;
			}

			SyncCurveResult.SetCurvePreset_Acc();

			SetChanged();
			ApplySync(true, false);
		}

		public void SetCurvePreset_Dec()
		{
			if(_syncStatus != SYNC_STATUS.Sync)
			{
				return;
			}

			SyncCurveResult.SetCurvePreset_Dec();

			SetChanged();
			ApplySync(true, false);
		}

		public void ResetSmoothSetting()
		{
			if(_syncStatus != SYNC_STATUS.Sync)
			{
				return;
			}

			SyncCurveResult.ResetSmoothSetting();

			SetChanged();
			ApplySync(true, false);
		}
		//--------------------------------------------------------

		//설정이 바뀌었음을 알려준다.
		//이 함수가 호출되어야 ApplySync가 동작한다.
		public void SetChanged()
		{	
			_isAnyChangedRequest = true;
		}


		

		public void ApplySync(bool isApplyForce, bool isMousePressed)
		{
			if(_syncStatus != SYNC_STATUS.Sync || !_isAnyChangedRequest)
			{
				return;
			}

			if(_curveSets == null || _curveSets.Count == 0)
			{
				return;
			}

			//isApplyForce = true이거나
			//isMousePressed = false일때 Apply를 한다.
			if(!isApplyForce && isMousePressed)
			{
				return;
			}

			apEditorUtil.SetEditorDirty();

			_syncCurve_Prev._nextCurveResult.MakeCurve();
			_syncCurve_Next._prevCurveResult.MakeCurve();

			//동기화를 적용하자
			CurveSet curSet = null;
			apAnimCurve prevCurve = null;
			apAnimCurve nextCurve = null;
			for (int i = 0; i < _curveSets.Count; i++)
			{
				curSet = _curveSets[i];

				if(curSet._prevKeyframe == null || curSet._nextKeyframe == null)
				{
					continue;
				}
				prevCurve = curSet._prevKeyframe._curveKey;
				nextCurve = curSet._nextKeyframe._curveKey;

				//공통 커브와 동일하게 만들자.
				//Prev의 Next와 Next의 Prev.. 아 헷갈려;;
				prevCurve._nextTangentType = _syncCurve_Prev._nextTangentType;
				prevCurve._nextSmoothX = _syncCurve_Prev._nextSmoothX;
				prevCurve._nextSmoothY = _syncCurve_Prev._nextSmoothY;

				nextCurve._prevTangentType = _syncCurve_Next._prevTangentType;
				nextCurve._prevSmoothX = _syncCurve_Next._prevSmoothX;
				nextCurve._prevSmoothY = _syncCurve_Next._prevSmoothY;

				prevCurve.Refresh();
				nextCurve.Refresh();
			}

			_isAnyChangedRequest = false;
			
		}



		public void NotSync2SyncStatus()
		{
			if(_syncStatus != SYNC_STATUS.NotSync)
			{
				return;
			}


			_syncStatus = SYNC_STATUS.Sync;

			//공통 Curve를 만들자
			_syncCurve_Prev._nextLinkedCurveKey = _syncCurve_Next;
			_syncCurve_Next._prevLinkedCurveKey = _syncCurve_Prev;

			_syncCurve_Prev._keyIndex = 0;
			_syncCurve_Next._keyIndex = 1;

			_syncCurve_Prev._nextIndex = 1;
			_syncCurve_Next._prevIndex = 0;

			//이전
			//_syncCurve_Prev._nextCurveResult.Link(_syncCurve_Prev, _syncCurve_Next, true, true);
			//_syncCurve_Next._prevCurveResult.Link(_syncCurve_Prev, _syncCurve_Next, false, true);

			//변경 19.5.20 : MakeCurve를 항상 수행
			_syncCurve_Prev._nextCurveResult.Link(_syncCurve_Prev, _syncCurve_Next, true);
			_syncCurve_Next._prevCurveResult.Link(_syncCurve_Prev, _syncCurve_Next, false);

			//Smooth 상태로 리셋한다.
			SyncCurveResult.ResetSmoothSetting();
			
			//동기화를 한다.
			SetChanged();
			ApplySync(true, false);
		}

		// Get / Set
		//----------------------------------------------------
		public SYNC_STATUS SyncStatus
		{
			get
			{
				return _syncStatus;
			}
		}

	}
}