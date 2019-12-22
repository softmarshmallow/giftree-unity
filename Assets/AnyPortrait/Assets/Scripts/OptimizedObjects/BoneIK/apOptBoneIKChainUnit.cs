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


using AnyPortrait;

namespace AnyPortrait
{

	/// <summary>
	/// apBoneIKChainUnit의 Opt 버전
	/// Serialize가 안되므로 초기에 Link를 해줘야 한다.
	/// 작업시 그냥 apBoneIKChainUnit을 복붙하고 Opt만 붙여주자
	/// </summary>
	[Serializable]
	public class apOptBoneIKChainUnit
	{
		//Members
		//-------------------------------------------------
		//변경 : 링크되는걸 Serialize로 할 수 없다.
		[NonSerialized]
		public apOptBoneIKChainUnit _parentChainUnit = null;

		[NonSerialized]
		public apOptBoneIKChainUnit _childChainUnit = null;

		[NonSerialized]
		public apOptBoneIKChainUnit _tailChainUnit = null;


		[SerializeField]
		public apOptBone _parentBone = null;//<Parent를 넣어야 IK

		[SerializeField]
		public apOptBone _baseBone = null;

		[SerializeField]
		public apOptBone _targetBone = null;



		//<현재 World Pos>
		//Bone들의 World 좌표
		//연산하면서 위치가 계속 바뀌지만, World Matrix로 반영하지 않고 별도로 계산하기 위해
		public Vector2 _bonePosW = Vector2.zero;

		//만약 Head라면 이 값을 사용해야한다.
		public Vector2 _parentPosW = Vector2.zero;
		public float _angleWorld_Parent = 0.0f;

		//만약 Tail이라면 완성된 World 값을 이용해서 Target Pos W를 구해야한다.
		public Vector2 _targetPosW = Vector2.zero;

		public float _lengthBoneToTarget = 0.0f;


		/// <summary>
		/// 계산 전의 벡터 각도(World). 
		/// 순수하게 Pos로만 계산된 각도이다.
		/// 계산 후에는 -90를 해서 Bone에 넣어줘야한다. (이 값은 -90 처리를 하지 않았다)
		/// </summary>
		public float _angleWorld_Prev = 0.0f;


		/// <summary>
		/// 계산 후의 벡터 각도(Parent 대비 상대값). 
		/// 순수하게 Pos로만 계산된 각도이다.
		/// 기본 연산에 의한 결과
		/// </summary>
		public float _angleLocal_Next = 0.0f;


		public float _angleWorld_Next = 0.0f;
		public float _angleLocal_Prev = 0.0f;//<<이건 계산용 값이다. 업데이트시에 매번 바뀜
		public float _angleLocal_Delta = 0.0f;//<<이건 계산용 값이다. 업데이트시에 매번 바뀜


		public bool _isAngleContraint = false;

		public float _angleDir_Preferred = 0.0f;
		public float _angleDir_Lower = 0.0f;
		public float _angleDir_Upper = 0.0f;
		//public bool _isAngleDir_Plus = true;

		public bool _isPreferredAngleAdapted = false;


		// Functions
		//-------------------------------------------------
		public apOptBoneIKChainUnit(apOptBone baseBone, apOptBone targetBone, apOptBone parentBone)
		{
			_baseBone = baseBone;
			_targetBone = targetBone;
			_parentBone = parentBone;

			_parentChainUnit = null;
			_childChainUnit = null;

			_isPreferredAngleAdapted = false;
		}


		public void SetParent(apOptBoneIKChainUnit parentUnit)
		{
			_parentChainUnit = parentUnit;
		}

		public void SetChild(apOptBoneIKChainUnit childUnit)
		{
			_childChainUnit = childUnit;
		}

		public void SetTail(apOptBoneIKChainUnit tailChainUnit)
		{
			_tailChainUnit = tailChainUnit;
		}


		private float Lerp(float A, float B, float itp, float length)
		{
			itp = Mathf.Clamp(itp, 0.0f, length);
			return (A * (length - itp) + B * itp) / length;
		}

		// Update
		//-----------------------------------------------------------
		/// <summary>
		/// IK 시뮬레이션하기전에 호출하는 함수
		/// 위치값을 모두 넣어주고, baseBone의 설정을 복사한다.
		/// 이 함수를 호출한 후, Head에서 CalculateWorldRecursive를 호출하자
		/// </summary>
		public void ReadyToSimulate()
		{
			//이전 코드
			//_isAngleContraint = _baseBone._isIKAngleRange;
			//_angleDir_Preferred = _baseBone._IKAnglePreferred;
			//_angleDir_Lower = _baseBone._IKAngleRange_Lower;
			//_angleDir_Upper = _baseBone._IKAngleRange_Upper;


			////현재의 Bone의 Pos World를 이용해서 Local 정보를 만들자

			////_bonePosW = _baseBone._worldMatrix._pos;
			////_targetPosW = _targetBone._worldMatrix._pos;
			//_bonePosW = _baseBone.PositionWithouEditing;
			//_targetPosW = _targetBone.PositionWithouEditing;
			

			//if (_parentBone != null)
			//{
			//	//_parentPosW = _parentBone._worldMatrix._pos;
			//	_parentPosW = _parentBone.PositionWithouEditing;

			//	//Angle : Parent -> Base
			//	_angleWorld_Parent = Vector2Angle(_bonePosW - _parentPosW);
			//}
			//else
			//{
			//	_parentPosW = Vector2.zero;
			//	//Parent가 없다면 AngleConstraint를 할 수 없다.
			//	_isAngleContraint = false;
			//	_angleWorld_Parent = 0.0f;
			//}
			
			//변경된 코드
			_isAngleContraint = _baseBone._isIKAngleRange;
			_bonePosW = _baseBone._worldMatrix._pos;
			_targetPosW = _targetBone._worldMatrix._pos;

			bool isXReverse = _baseBone._worldMatrix._scale.x < 0.0f;
			
			float _angleParentToBase_Offset = 0.0f;
			if (_parentBone != null)
			{
				_parentPosW = _parentBone._worldMatrix._pos;

				//Angle : Parent -> Base
				_angleWorld_Parent = Vector2Angle(_bonePosW - _parentPosW);

				//Parent에서 Base로의 상대 각도가 Default와 같은지 체크
				//Offset만큼 위치가 맞지 않는다.
				//_angleParentToBase_Offset = apUtil.AngleTo180((_angleWorld_Parent - _parentBone._worldMatrix._angleDeg) - 90);
				_angleParentToBase_Offset = apUtil.AngleTo180((_angleWorld_Parent - _parentBone._worldMatrix._angleDeg) - 90);
			}
			else
			{
				//>>>변경
				//RootBone에서도 AngleConstraint를 적용할 수 있다.
				_parentPosW = Vector2.zero;
				_angleWorld_Parent = 0.0f; 
				_angleParentToBase_Offset = 0.0f - 90.0f;//<<실제 각도와 IK각도가 90만큼 차이가 있기 때문
				//_angleParentToBase_Offset = 0.0f;
				//TODO
				if(_baseBone._parentOptTransform != null)
				{
					//TODO
					//RenderUnit과의 계산
					_angleWorld_Parent = _baseBone._parentOptTransform._matrix_TFResult_World._angleDeg;
				}
			}

			float defaultAngle180 = apUtil.AngleTo180((_baseBone._defaultMatrix._angleDeg - _angleParentToBase_Offset));
			if(isXReverse)
			{
				_angleDir_Preferred = apUtil.AngleTo180(defaultAngle180 - _baseBone._IKAnglePreferred);
				_angleDir_Lower = apUtil.AngleTo180(defaultAngle180 - _baseBone._IKAngleRange_Upper);
				_angleDir_Upper = apUtil.AngleTo180(defaultAngle180 - _baseBone._IKAngleRange_Lower);
			}
			else
			{
				_angleDir_Preferred = apUtil.AngleTo180(defaultAngle180 + _baseBone._IKAnglePreferred);
				_angleDir_Lower = apUtil.AngleTo180(defaultAngle180 + _baseBone._IKAngleRange_Lower);
				_angleDir_Upper = apUtil.AngleTo180(defaultAngle180 + _baseBone._IKAngleRange_Upper);
			}
			

			//----------------------------

			_lengthBoneToTarget = Vector2.Distance(_targetPosW, _bonePosW);

			//Debug.Log("ReadyToSimulate [" + _baseBone._name + " > " + _targetBone._name + "]");
			//Debug.Log("AnglePrev : " + _bonePosW + " >> " + _targetPosW + " = " + _angleWorld_Prev);
			_angleWorld_Prev = Vector2Angle(_targetPosW - _bonePosW);
			_angleWorld_Next = _angleWorld_Prev;

			_angleLocal_Next = _angleWorld_Next - _angleWorld_Parent;


		}


		/// <summary>
		/// 현재 호출한 Bone Unit을 시작으로 Tail 방향으로 World를 갱신한다.
		/// Parent의 PosW, AngleWorld가 갱신되었어야 한다.
		/// IK의 핵심이 되는 _angleLocal_Next가 계산된 상태여야 한다.
		/// </summary>
		public void CalculateWorldRecursive()
		{
			if (_parentChainUnit != null)
			{
				//Parent 기준으로 Pos를 갱신한다.
				_parentPosW = _parentChainUnit._bonePosW;
				_angleWorld_Parent = _parentChainUnit._angleWorld_Next;

				_bonePosW.x = _parentPosW.x + _parentChainUnit._lengthBoneToTarget * Mathf.Cos(_angleWorld_Parent * Mathf.Deg2Rad);
				_bonePosW.y = _parentPosW.y + _parentChainUnit._lengthBoneToTarget * Mathf.Sin(_angleWorld_Parent * Mathf.Deg2Rad);
			}

			//Local Angle에 따라 World Angle을 갱신한다.
			_angleWorld_Next = _angleLocal_Next + _angleWorld_Parent;

			//Child Unit도 같이 갱신해주자
			if (_childChainUnit != null)
			{
				_childChainUnit.CalculateWorldRecursive();

			}
			else
			{
				//엥 여기가 Tail인가염
				_targetPosW.x = _bonePosW.x + _lengthBoneToTarget * Mathf.Cos(_angleWorld_Next * Mathf.Deg2Rad);
				_targetPosW.y = _bonePosW.y + _lengthBoneToTarget * Mathf.Sin(_angleWorld_Next * Mathf.Deg2Rad);
			}
		}



		/// <summary>
		/// IK를 요청한다.
		/// </summary>
		/// <param name="requestIKPosW"></param>
		/// <param name="isContinuous"></param>
		/// <returns></returns>
		public void RequestIK(Vector2 requestIKPosW, bool isContinuous)
		{
			_angleLocal_Prev = _angleLocal_Next;

			//회전해야하는 World 각도
			float angleIK_Bone2IKtarget = Vector2Angle(requestIKPosW - _bonePosW);
			float angleIK_Bone2Tail = Vector2Angle(_tailChainUnit._targetPosW - _bonePosW);


			
			//현재 각도에서 빼자
			//float angleIK_Delta = angleIK_World - _angleWorld_Next;
			float angleIK_Delta = apUtil.AngleTo180(angleIK_Bone2IKtarget - angleIK_Bone2Tail);



			//Local에 더해주자
			//이전 코드
			//_angleLocal_Next += angleIK_Delta;
			//while (_angleLocal_Next > 180.0f)
			//{
			//	_angleLocal_Next -= 360.0f;
			//}
			//while (_angleLocal_Next < -180.0f)
			//{
			//	_angleLocal_Next += 360.0f;
			//}

			//변경된 코드
			_angleLocal_Next = apUtil.AngleTo360(_angleLocal_Next + angleIK_Delta);

			//Angle Constraint에 걸리나
			if (_isAngleContraint)
			{
				if (_angleLocal_Next < _angleDir_Lower)
				{
					_angleLocal_Next = _angleDir_Lower;
				}
				else if (_angleLocal_Next > _angleDir_Upper)
				{
					_angleLocal_Next = _angleDir_Upper;
				}
			}
			//이후에 Calculate를 외부에서 호출해주자
			_angleLocal_Delta = _angleLocal_Next - _angleLocal_Prev;
		}


		public void RequestIK_Weighted(Vector2 requestIKPosW, bool isContinuous, float weight)
		{
			
			_angleLocal_Prev = _angleLocal_Next;

			//회전해야하는 World 각도
			float angleIK_Bone2IKtarget = Vector2Angle(requestIKPosW - _bonePosW);
			float angleIK_Bone2Tail = Vector2Angle(_tailChainUnit._targetPosW - _bonePosW);

			//현재 각도에서 빼자
			float angleIK_Delta = apUtil.AngleTo180(angleIK_Bone2IKtarget - angleIK_Bone2Tail);
			angleIK_Delta *= weight;//<<추가 : 가중치를 두어서 제한을 한다.

			
			//Local에 더해주자
			_angleLocal_Next = apUtil.AngleTo360(_angleLocal_Next + angleIK_Delta);

			//Angle Constraint에 걸리나
			if (_isAngleContraint)
			{
				
				if (_angleLocal_Next < _angleDir_Lower)
				{
					_angleLocal_Next = _angleDir_Lower;
				}
				else if (_angleLocal_Next > _angleDir_Upper)
				{
					_angleLocal_Next = _angleDir_Upper;
				}
			}

			
			//이후에 Calculate를 외부에서 호출해주자
			_angleLocal_Delta = _angleLocal_Next - _angleLocal_Prev;
		}

		// 계산 함수들
		//----------------------------------------------------------------------------------
		/// <summary>
		/// Dir Vector의 Angle (Degree)를 리턴한다.
		/// </summary>
		/// <param name="dirVec"></param>
		/// <returns></returns>
		public static float Vector2Angle(Vector2 dirVec)
		{
			return Mathf.Atan2(dirVec.y, dirVec.x) * Mathf.Rad2Deg;
		}

		/// <summary>
		/// 두개의 좌표계에서, Origin Pos를 기준으로 Target Pos를 회전하고, 그 위치를 리턴한다.
		/// 각도는 변환 뒤의 절대값이다. (Degree)
		/// </summary>
		/// <param name="originPos"></param>
		/// <param name="targetPos"></param>
		/// <param name="nextAngle"></param>
		/// <returns></returns>
		public static Vector2 RotateAngle(Vector2 originPos, Vector2 targetPos, float nextAngle)
		{
			float dist = Vector2.Distance(targetPos, originPos);

			return new Vector2(originPos.x + dist * Mathf.Cos(nextAngle * Mathf.Deg2Rad),
								originPos.y + dist * Mathf.Sin(nextAngle * Mathf.Deg2Rad)
								);
		}
	}

}