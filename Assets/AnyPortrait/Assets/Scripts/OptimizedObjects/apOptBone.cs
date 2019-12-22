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
	/// The bound of the region where the position of the bone is restricted
	/// </summary>
	public enum ConstraintBound
	{
		/// <summary>Recommended X-value of position limited</summary>
		Xprefer,
		/// <summary>Minimum X-value of position limited</summary>
		Xmin,
		/// <summary>Maxmum X-value of position limited</summary>
		Xmax,
		/// <summary>Recommended Y-value of position limited</summary>
		Yprefer,
		/// <summary>Minimum Y-value of position limited</summary>
		Ymin,
		/// <summary>Maxmum Y-value of position limited</summary>
		Ymax
	}
	public enum ConstraintSurface
	{
		/// <summary>The surface of the X axis. It is like a wall</summary>
		Xsurface,
		/// <summary>The surface of the Y axis. It is like a ground</summary>
		Ysurface
	}

	// MeshGroup에 포함되는 apBone의 Opt 버전
	// MeshGroup에 해당되는 Root OptTransform의 "Bones" GameObject에 포함된다.
	// Matrix 계산은 Bone과 동일하며, Transform에 반영되지 않는다. (Transform은 Local Pos와 Rotation만 계산된다)
	// Transform은 Rigging에 반영되지는 않지만, 만약 어떤 오브젝트를 Attachment 한다면 사용되어야 한다.
	// Opt Bone의 Transform은 외부 입력은 무시하며, Attachment를 하는 용도로만 사용된다.
	// Attachment를 하는 경우 하위에 Socket Transform을 생성한뒤, 거기서 WorldMatrix에 해당하는 TRS를 넣는다. (값 자체는 Local Matrix)
	
		/// <summary>
	/// A class in which "apBone" is baked
	/// It belongs to "apOptTransform".
	/// (It is recommended to use the functions of "apPortrait" to control the position of the bone, and it is recommended to use "Socket" when referring to the position.)
	/// </summary>
	public class apOptBone : MonoBehaviour
	{
		// Members
		//---------------------------------------------------------------
		// apBone 정보를 옮기자
		/// <summary>Bone Name</summary>
		public string _name = "";

		/// <summary>[Please do not use it] Bone's Unique ID</summary>
		public int _uniqueID = -1;

		/// <summary>[Please do not use it] Parent MeshGroup ID</summary>
		public int _meshGroupID = -1;

		//이건 Serialize가 된다.
		/// <summary>Parent Opt-Transform</summary>
		public apOptTransform _parentOptTransform = null;

		/// <summary>
		/// Parent Opt-Bone
		/// </summary>
		public apOptBone _parentBone = null;

		/// <summary>
		/// Children of a Bone
		/// </summary>
		public apOptBone[] _childBones = null;//<ChildBones의 배열버전

		/// <summary>
		/// [Please do not use it] Default Matrix
		/// </summary>
		[SerializeField]
		public apMatrix _defaultMatrix = new apMatrix();

		[NonSerialized]
		private Vector2 _deltaPos = Vector2.zero;

		[NonSerialized]
		private float _deltaAngle = 0.0f;

		[NonSerialized]
		private Vector2 _deltaScale = Vector2.one;

		/// <summary>[Please do not use it] Local Matrix</summary>
		[NonSerialized]
		public apMatrix _localMatrix = new apMatrix();
		
		/// <summary>[Please do not use it] World Matrix</summary>
		[NonSerialized]
		public apMatrix _worldMatrix = new apMatrix();

		/// <summary>
		/// [Please do not use it] World Matrix (Default)
		/// </summary>
		[NonSerialized]
		public apMatrix _worldMatrix_NonModified = new apMatrix();

		//추가 : 계산된 Bone IK Controller Weight와 IK용 worldMatrix > 이건 편집과 구분하기 위해 별도로 계산.
		[NonSerialized]
		public float _calculatedBoneIKWeight = 0.0f;

		/// <summary>
		/// [Please do not use it] Rigging Matrix
		/// </summary>
		//리깅을 위한 통합 Matrix
		[NonSerialized]
		public apMatrix3x3 _vertWorld2BoneModWorldMatrix = new apMatrix3x3();//<<이게 문제

		


		//Shape 계열
		/// <summary>
		/// [Please do not use it] Bone Color in Editor / Gizmo
		/// </summary>
		[SerializeField]
		public Color _color = Color.white;

		public int _shapeWidth = 30;
		public int _shapeLength = 50;//<<이 값은 생성할 때 Child와의 거리로 판단한다.
		public int _shapeTaper = 100;//기본값은 뾰족

#if UNITY_EDITOR
		private Vector2 _shapePoint_End = Vector3.zero;

		private Vector2 _shapePoint_Mid1 = Vector3.zero;
		private Vector2 _shapePoint_Mid2 = Vector3.zero;
		private Vector2 _shapePoint_End1 = Vector3.zero;
		private Vector2 _shapePoint_End2 = Vector3.zero;
#endif

		//IK 정보
		/// <summary>[Please do not use it]</summary>
		public apBone.OPTION_LOCAL_MOVE _optionLocalMove = apBone.OPTION_LOCAL_MOVE.Disabled;

		/// <summary>[Please do not use it] Bone's IK Type</summary>
		public apBone.OPTION_IK _optionIK = apBone.OPTION_IK.IKSingle;

		// Parent로부터 IK의 대상이 되는가? IK Single일 때에도 Tail이 된다.
		// (자신이 IK를 설정하는 것과는 무관함)
		/// <summary> [Please do not use it] </summary>
		public bool _isIKTail = false;

		//IK의 타겟과 Parent
		/// <summary>[Please do not use it]</summary>
		public int _IKTargetBoneID = -1;

		/// <summary>[Please do not use it]</summary>
		public apOptBone _IKTargetBone = null;

		/// <summary>[Please do not use it]</summary>
		public int _IKNextChainedBoneID = -1;

		/// <summary>[Please do not use it]</summary>
		public apOptBone _IKNextChainedBone = null;


		// IK Tail이거나 IK Chained 상태라면 Header를 저장하고, Chaining 처리를 해야한다.
		/// <summary>[Please do not use it]</summary>
		public int _IKHeaderBoneID = -1;

		/// <summary>[Please do not use it]</summary>
		public apOptBone _IKHeaderBone = null;



		//IK시 추가 옵션

		// IK 적용시, 각도를 제한을 줄 것인가 (기본값 False)
		/// <summary>[Please do not use it] IK Angle Contraint Option</summary>
		public bool _isIKAngleRange = false;

		/// <summary>[Please do not use it]</summary>
		public float _IKAngleRange_Lower = -90.0f;//음수여야 한다.

		/// <summary>[Please do not use it]</summary>
		public float _IKAngleRange_Upper = 90.0f;//양수여야 한다.

		/// <summary>[Please do not use it]</summary>
		public float _IKAnglePreferred = 0.0f;//선호하는 각도 Offset



		// IK 연산이 되었는가
		/// <summary>
		/// Is IK Calculated
		/// </summary>
		[NonSerialized]
		public bool _isIKCalculated = false;

		// IK 연산이 발생했을 경우, World 좌표계에서 Angle을 어떻게 만들어야 하는지 계산 결과값
		/// <summary>[Please do not use it]</summary>
		[NonSerialized]
		public float _IKRequestAngleResult = 0.0f;

		/// <summary>[Please do not use it]</summary>
		[NonSerialized]
		public float _IKRequestWeight = 0.0f;
		

		/// <summary>
		/// IK 계산을 해주는 Chain Set.
		/// </summary>
		[SerializeField]
		private apOptBoneIKChainSet _IKChainSet = null;//<<이거 Opt 버전으로 만들자

		[SerializeField]
		private bool _isIKChainSetAvailable = false;

		private bool _isIKChainInit = false;



		//추가 : 이건 나중에 세팅하자
		//Transform에 적용되는 Local Matrix 값 (Scale이 없다)
		/// <summary>[Please do not use it]</summary>
		[NonSerialized]
		public apMatrix _transformLocalMatrix = new apMatrix();

		//Attach시 만들어지는 Socket
		//Socket 옵션은 Bone에서 미리 세팅해야한다.
		/// <summary>
		/// Socket Transform.
		/// In Unity World, this is a Socket that actually has the position, rotation, and size of the bone. 
		/// If you want to refer to the position or rotation of the bone from the outside, it is recommended to use Socket.
		/// </summary>
		public Transform _socketTransform = null;

		//추가 5.8
		//Position Controller와 LookAt Controller를 추가했다.
		[SerializeField]
		public apOptBoneIKController _IKController = null;

		[NonSerialized]
		public bool _isIKCalculated_Controlled = false;

		[NonSerialized]
		public float _IKRequestAngleResult_Controlled = 0.0f;

		[NonSerialized]
		public float _IKRequestWeight_Controlled = 0.0f;

		//[NonSerialized]
		//private bool _isIKRendered_Controller = false;




		//스크립트로 TRS를 직접 제어할 수 있다.
		//단 Update마다 매번 설정해야한다.
		//좌표계는 WorldMatrix를 기준으로 한다.
		//값 자체는 절대값을 기준으로 한다.
		private bool _isExternalUpdate_Position = false;
		private bool _isExternalUpdate_Rotation = false;
		private bool _isExternalUpdate_Scaling = false;
		private float _externalUpdateWeight = 0.0f;
		private Vector2 _exUpdate_Pos = Vector2.zero;
		private float _exUpdate_Angle = 0.0f;
		private Vector2 _exUpdate_Scale = Vector2.zero;


		//추가 6.7 : 영역을 제한하자
		private bool _isExternalConstraint = false;
		private bool _isExternalConstraint_Xmin = false;
		private bool _isExternalConstraint_Xmax = false;
		private bool _isExternalConstraint_Ymin = false;
		private bool _isExternalConstraint_Ymax = false;
		private bool _isExternalConstraint_Xpref = false;
		private bool _isExternalConstraint_Ypref = false;
		private bool _isExternalConstraint_Xsurface = false;
		private bool _isExternalConstraint_Ysurface = false;
		private Vector3 _externalConstraint_PosX = Vector3.zero;//x:min, y:pref, z:max 순서
		private Vector3 _externalConstraint_PosY = Vector3.zero;
		private Vector4 _externalConstraint_PosSurfaceX = Vector4.zero;//x:기준 좌표, y:surface의 현재 좌표, z:min, w:max
		private Vector4 _externalConstraint_PosSurfaceY = Vector4.zero;

		//처리된 TRS
		
		private Vector3 _updatedWorldPos = Vector3.zero;
		private float _updatedWorldAngle = 0.0f;
		private Vector3 _updatedWorldScale = Vector3.one;

		private Vector3 _updatedWorldPos_NoRequest = Vector3.zero;
		private float _updatedWorldAngle_NoRequest = 0.0f;
		private Vector3 _updatedWorldScale_NoRequest = Vector3.one;

		// Init
		//---------------------------------------------------------------
		void Start()
		{
			//업데이트 안합니더
			this.enabled = false;

			_isExternalUpdate_Position = false;
			_isExternalUpdate_Rotation = false;
			_isExternalUpdate_Scaling = false;

			_isExternalConstraint = false;
			_isExternalConstraint_Xmin = false;
			_isExternalConstraint_Xmax = false;
			_isExternalConstraint_Ymin = false;
			_isExternalConstraint_Ymax = false;
			_isExternalConstraint_Xpref = false;
			_isExternalConstraint_Ypref = false;
			_isExternalConstraint_Xsurface = false;
			_isExternalConstraint_Ysurface = false;
		
		}


		//Link 함수의 내용은 Bake 시에 진행해야한다.
		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="bone"></param>
		public void Bake(apBone bone)
		{
			_name = bone._name;
			_uniqueID = bone._uniqueID;
			_meshGroupID = bone._meshGroupID;
			_defaultMatrix.SetMatrix(bone._defaultMatrix);


			_deltaPos = Vector2.zero;
			_deltaAngle = 0.0f;
			_deltaScale = Vector2.one;

			_localMatrix.SetIdentity();

			_worldMatrix.SetIdentity();

			_worldMatrix_NonModified.SetIdentity();
			_vertWorld2BoneModWorldMatrix.SetIdentity();

			_color = bone._color;
			_shapeWidth = bone._shapeWidth;
			_shapeLength = bone._shapeLength;
			_shapeTaper = bone._shapeTaper;

			_optionLocalMove = bone._optionLocalMove;
			_optionIK = bone._optionIK;

			_isIKTail = bone._isIKTail;

			_IKTargetBoneID = bone._IKTargetBoneID;
			_IKTargetBone = null;//<<나중에 링크

			_IKNextChainedBoneID = bone._IKNextChainedBoneID;
			_IKNextChainedBone = null;//<<나중에 링크


			_IKHeaderBoneID = bone._IKHeaderBoneID;
			_IKHeaderBone = null;//<<나중에 링크


			_isIKAngleRange = bone._isIKAngleRange;
			//이게 기존 코드
			_IKAngleRange_Lower = bone._IKAngleRange_Lower;
			_IKAngleRange_Upper = bone._IKAngleRange_Upper;
			_IKAnglePreferred = bone._IKAnglePreferred;

			//이게 변경된 IK 코드
			//_IKAngleRange_Lower = bone._defaultMatrix._angleDeg + bone._IKAngleRange_Lower;
			//_IKAngleRange_Upper = bone._defaultMatrix._angleDeg + bone._IKAngleRange_Upper;
			//_IKAnglePreferred = bone._defaultMatrix._angleDeg + bone._IKAnglePreferred;


			_isIKCalculated = false;
			_IKRequestAngleResult = 0.0f;
			_IKRequestWeight = 0.0f;

			_socketTransform = null;

			_transformLocalMatrix.SetIdentity();

			_childBones = null;

			_isIKChainInit = false;

			//추가 : IKController를 추가한다.
			if(_IKController == null)
			{
				_IKController = new apOptBoneIKController();
			}
			_IKController.Bake(this, 
				bone._IKController._effectorBoneID, 
				bone._IKController._controllerType, 
				bone._IKController._defaultMixWeight,
				bone._IKController._isWeightByControlParam,
				bone._IKController._weightControlParamID);

		}

		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="targetOptTransform"></param>
		public void Link(apOptTransform targetOptTransform)
		{
			_parentOptTransform = targetOptTransform;
			if (_parentOptTransform == null)
			{
				//??
				Debug.LogError("[" + transform.name + "] ParentOptTransform of OptBone is Null [" + _meshGroupID + "]");
				_IKTargetBone = null;
				_IKNextChainedBone = null;
				_IKHeaderBone = null;

				//LinkBoneChaining();


				return;
			}


			_IKTargetBone = _parentOptTransform.GetBone(_IKTargetBoneID);
			_IKNextChainedBone = _parentOptTransform.GetBone(_IKNextChainedBoneID);
			_IKHeaderBone = _parentOptTransform.GetBone(_IKHeaderBoneID);

			//LinkBoneChaining();

			//추가 : EffectorBone을 연결한다.
			if (_IKController._controllerType != apOptBoneIKController.CONTROLLER_TYPE.None
				&& _IKController._effectorBoneID >= 0)
			{
				_IKController.LinkEffector(targetOptTransform.GetBone(_IKController._effectorBoneID));
			}
			
			
		}



		//여기서는 LinkBoneChaining만 진행
		// Bone Chaining 직후에 재귀적으로 호출한다.
		// Tail이 가지는 -> Head로의 IK 리스트를 만든다.
		/// <summary>
		/// [Please do not use it] IK Link
		/// </summary>
		public void LinkBoneChaining()
		{
			if (_localMatrix == null)
			{
				_localMatrix = new apMatrix();
			}
			if (_worldMatrix == null)
			{
				_worldMatrix = new apMatrix();
			}
			if (_worldMatrix_NonModified == null)
			{
				_worldMatrix_NonModified = new apMatrix();
			}


			if (_isIKTail)
			{
				apOptBone curParentBone = _parentBone;
				apOptBone headBone = _IKHeaderBone;

				bool isParentExist = (curParentBone != null);
				bool isHeaderExist = (headBone != null);
				bool isHeaderIsInParents = false;
				if (isParentExist && isHeaderExist)
				{
					isHeaderIsInParents = (GetParentRecursive(headBone._uniqueID) != null);
				}


				if (isParentExist && isHeaderExist && isHeaderIsInParents)
				{
					if (_IKChainSet == null)
					{
						_IKChainSet = new apOptBoneIKChainSet(this);
					}
					_isIKChainSetAvailable = true;
					//Chain을 Refresh한다.
					//_IKChainSet.RefreshChain();//<<수정. 이건 Runtime에 해야한다.
				}
				else
				{
					_IKChainSet = null;

					Debug.LogError("[" + transform.name + "] IK Chaining Error : Parent -> Chain List Connection Error "
						+ "[ Parent : " + isParentExist
						+ " / Header : " + isHeaderExist
						+ " / IsHeader Is In Parent : " + isHeaderIsInParents + " ]");
					_isIKChainSetAvailable = false;
				}
			}
			else
			{
				_IKChainSet = null;
				_isIKChainSetAvailable = false;
			}

			if (_childBones != null)
			{
				for (int i = 0; i < _childBones.Length; i++)
				{
					_childBones[i].LinkBoneChaining();
				}
			}

		}


		// Update
		//---------------------------------------------------------------
		// Update Transform Matrix를 초기화한다.
		public void ReadyToUpdate(bool isRecursive)
		{
			//_localModifiedTransformMatrix.SetIdentity();

			_deltaPos = Vector2.zero;
			_deltaAngle = 0.0f;
			_deltaScale = Vector2.one;

			//_isIKCalculated = false;
			//_IKRequestAngleResult = 0.0f;

			//추가 : Bone IK
			_isIKCalculated_Controlled = false;
			_IKRequestAngleResult_Controlled = 0.0f;
			_IKRequestWeight_Controlled = 0.0f;
			//_isIKRendered_Controller = false;

			_calculatedBoneIKWeight = 0.0f;//<<추가

			if (_IKController._controllerType != apOptBoneIKController.CONTROLLER_TYPE.None)
			{
				_calculatedBoneIKWeight = _IKController._defaultMixWeight;

				if (!_isIKChainInit)
				{
					InitIKChain();
					_isIKChainInit = true;
				}
			}

			//_worldMatrix.SetIdentity();
			if (isRecursive && _childBones != null)
			{
				for (int i = 0; i < _childBones.Length; i++)
				{
					_childBones[i].ReadyToUpdate(true);
				}
			}
		}

		/// <summary>
		/// Bake를 위해서 BoneMatrix를 초기화한다.
		/// </summary>
		/// <param name="isRecursive"></param>
		public void ResetBoneMatrixForBake(bool isRecursive)
		{
			
			_deltaPos = Vector2.zero;
			_deltaAngle = 0.0f;
			_deltaScale = Vector2.one;

			_localMatrix.SetIdentity();
			_worldMatrix.SetIdentity();

			_worldMatrix_NonModified.SetIdentity();
			_vertWorld2BoneModWorldMatrix.SetIdentity();

			
			if (_parentBone == null)
			{
				_worldMatrix.SetMatrix(_defaultMatrix);
				_worldMatrix.Add(_localMatrix);

				_worldMatrix_NonModified.SetMatrix(_defaultMatrix);//Local Matrix 없이 Default만 지정


				if (_parentOptTransform != null)
				{
					//Debug.Log("SetParentOptTransform Matrix : [" + _parentOptTransform.transform.name + "] : " + _parentOptTransform._matrix_TFResult_World.Scale2);
					//Non Modified도 동일하게 적용
					//렌더유닛의 WorldMatrix를 넣어주자
					_worldMatrix.RMultiply(_parentOptTransform._matrix_TFResult_WorldWithoutMod);//RenderUnit의 WorldMatrixWrap의 Opt 버전
					

					_worldMatrix_NonModified.RMultiply(_parentOptTransform._matrix_TFResult_WorldWithoutMod);

				}
			}
			else
			{
				_worldMatrix.SetMatrix(_defaultMatrix);
				_worldMatrix.Add(_localMatrix);
				_worldMatrix.RMultiply(_parentBone._worldMatrix_NonModified);

				_worldMatrix_NonModified.SetMatrix(_defaultMatrix);//Local Matrix 없이 Default만 지정
				_worldMatrix_NonModified.RMultiply(_parentBone._worldMatrix_NonModified);
			}

			_worldMatrix.SetMatrix(_worldMatrix_NonModified);

			_vertWorld2BoneModWorldMatrix = _worldMatrix_NonModified.MtrxToSpace;
			_vertWorld2BoneModWorldMatrix *= _worldMatrix_NonModified.MtrxToLowerSpace;


			
			//Debug.Log("Reset Bone Matrix [" + this.name + "]");
			//Debug.Log("World Matrix [ " + _worldMatrix.ToString() + "]");

			if (isRecursive)
			{
				if (_childBones != null && _childBones.Length > 0)
				{
					for (int i = 0; i < _childBones.Length; i++)
					{
						_childBones[i].ResetBoneMatrixForBake(true);
					}
				}
			}
			
		}



		//public void DebugBoneMatrix()
		//{
		//	//if (string.Equals(this.name, "Bone 1"))
		//	//{
		//	//	Debug.LogError("Debug Bone Matrix (After Update)");
		//	//	Debug.Log(this.name + " / Local Modified [ " + _localMatrix.ToString() + " ]");
		//	//	Debug.Log(this.name + " / World [ " + _worldMatrix.ToString() + " ]");

		//	//	Debug.Log("Delta : Pos : " + _deltaPos + " / Angle : " + _deltaAngle + " / Scale : " + _deltaScale);
		//	//}

		//	if (_childBones != null && _childBones.Length > 0)
		//	{
		//		for (int i = 0; i < _childBones.Length; i++)
		//		{
		//			_childBones[i].DebugBoneMatrix();
		//		}
		//	}
		//}






		// 2) Update된 TRS 값을 넣는다.
		public void UpdateModifiedValue(Vector2 deltaPos, float deltaAngle, Vector2 deltaScale)
		{
			_deltaPos = deltaPos;
			_deltaAngle = deltaAngle;
			_deltaScale = deltaScale;

			//if (isBoneIKCalculated)
			//{
			//	_calculatedBoneIKWeight = Mathf.Clamp01(boneIKWeight);//<<1이상 입력될 수 있다.
			//	Debug.Log("Opt IK : " + name + " : " + _calculatedBoneIKWeight);
			//}
		}

		/// <summary>
		/// [Please do not use it]
		/// </summary>
		/// <param name="IKAngle"></param>
		/// <param name="weight"></param>
		public void AddIKAngle(float IKAngle, float weight)
		{
			//Debug.Log("IK [" + _name + "] : " + IKAngle);
			_isIKCalculated = true;
			//_IKRequestAngleResult += IKAngle;
			_IKRequestWeight = weight;
			_IKRequestAngleResult += IKAngle;
		}


		public void AddIKAngle_Controlled(float IKAngle, float weight)
		{
			_isIKCalculated_Controlled = true;
			//_isIKRendered_Controller = true;
			_IKRequestAngleResult_Controlled += (IKAngle) * weight;
			_IKRequestWeight_Controlled += weight;
		}


		// 4) World Matrix를 만든다.
		// 이 함수는 Parent의 MeshGroupTransform이 연산된 후 -> Vertex가 연산되기 전에 호출되어야 한다.
		public void MakeWorldMatrix(bool isRecursive)
		{
			_localMatrix.SetIdentity();
			_localMatrix._pos = _deltaPos;
			_localMatrix._angleDeg = _deltaAngle;
			_localMatrix._scale.x = _deltaScale.x;
			_localMatrix._scale.y = _deltaScale.y;

			_localMatrix.MakeMatrix();

			//World Matrix = ParentMatrix x LocalMatrix
			//Root인 경우에는 MeshGroup의 Matrix를 이용하자

			//_invWorldMatrix_NonModified.SetIdentity();

			if (_parentBone == null)
			{
				_worldMatrix.SetMatrix(_defaultMatrix);
				_worldMatrix.Add(_localMatrix);

				_worldMatrix_NonModified.SetMatrix(_defaultMatrix);//Local Matrix 없이 Default만 지정


				if (_parentOptTransform != null)
				{
					//Debug.Log("SetParentOptTransform Matrix : [" + _parentOptTransform.transform.name + "] : " + _parentOptTransform._matrix_TFResult_World.Scale2);
					//Non Modified도 동일하게 적용
					//렌더유닛의 WorldMatrix를 넣어주자
					_worldMatrix.RMultiply(_parentOptTransform._matrix_TFResult_World);//RenderUnit의 WorldMatrixWrap의 Opt 버전

					_worldMatrix_NonModified.RMultiply(_parentOptTransform._matrix_TFResult_WorldWithoutMod);

				}
			}
			else
			{
				_worldMatrix.SetMatrix(_defaultMatrix);
				_worldMatrix.Add(_localMatrix);
				_worldMatrix.RMultiply(_parentBone._worldMatrix);

				_worldMatrix_NonModified.SetMatrix(_defaultMatrix);//Local Matrix 없이 Default만 지정
				_worldMatrix_NonModified.RMultiply(_parentBone._worldMatrix_NonModified);
			}

			//추가 : 외부 변수들은 이 함수에서 처리한다.
			UpdateExternalRequest();
			

			//World Matrix는 MeshGroup과 동일한 Space의 값을 가진다.
			//그러나 실제로 Bone World Matrix는
			//Root - MeshGroup...(Rec) - Bone Group - Bone.. (Rec <- 여기)
			//의 레벨을 가진다.
			//Root 밑으로는 모두 World에 대해서 동일한 Space를 가지므로
			//Root를 찾아서 Scale을 제어하자...?
			//일단 Parent에서 빼두자
			//_transformLocalMatrix.SetMatrix(_worldMatrix);

			//>>UpdatePostRecursive() 함수에서 나중에 일괄적으로 갱신한다.
			
			//Child도 호출해준다.
			if (isRecursive && _childBones != null)
			{
				for (int i = 0; i < _childBones.Length; i++)
				{
					_childBones[i].MakeWorldMatrix(true);
				}
			}
		}



		/// <summary>
		/// IK Controlled가 있는 경우 IK를 계산한다.
		/// Child 중 하나라도 계산이 되었다면 True를 리턴한다.
		/// 계산 자체는 IK Controller가 활성화된 경우에 한한다. (Chain되어서 처리가 된다.)
		/// </summary>
		/// <param name="isRecursive"></param>
		public bool CalculateIK(bool isRecursive)
		{
			bool IKCalculated = false;

			if (_IKChainSet != null && _isIKChainSetAvailable)
			{

				//Control Param의 영향을 받는다.
				if (_IKController._controllerType != apOptBoneIKController.CONTROLLER_TYPE.None
					&& _IKController._isWeightByControlParam
					&& _IKController._weightControlParam != null)
				{
					_calculatedBoneIKWeight = Mathf.Clamp01(_IKController._weightControlParam._float_Cur);
				}


				if (_calculatedBoneIKWeight > 0.001f
					&& _IKController._controllerType != apOptBoneIKController.CONTROLLER_TYPE.None
					)
				{

					if (_IKController._controllerType == apOptBoneIKController.CONTROLLER_TYPE.Position)
					{
						//1. Position 타입일 때
						if (_IKController._effectorBone != null)
						{
							//bool result = _IKChainSet.SimulateIK(_IKController._effectorBone._worldMatrix._pos, true, true);//<<논리상 EffectorBone은 IK의 영향을 받으면 안된다.
							bool result = _IKChainSet.SimulateIK(_IKController._effectorBone._worldMatrix._pos, true);//<<논리상 EffectorBone은 IK의 영향을 받으면 안된다.
							if (result)
							{
								IKCalculated = true;
								_IKChainSet.AdaptIKResultToBones_ByController(_calculatedBoneIKWeight);
							}
						}
					}
					else
					{
						//2. LookAt 타입일 때
						if (_IKController._effectorBone != null
							)
						{
							//bool result = _IKChainSet.SimulateLookAtIK(_IKController._effectorBone._worldMatrix_NonModified._pos, _IKController._effectorBone._worldMatrix._pos, true, true);
							bool result = _IKChainSet.SimulateLookAtIK(_IKController._effectorBone._worldMatrix_NonModified._pos, _IKController._effectorBone._worldMatrix._pos, true);

							if (result)
							{
								IKCalculated = true;
								_IKChainSet.AdaptIKResultToBones_ByController(_calculatedBoneIKWeight);

								//이 Bone은 그냥 바라보도록 한다.

								AddIKAngle_Controlled(
									(apBoneIKChainUnit.Vector2Angle(_IKController._effectorBone._worldMatrix._pos - _IKChainSet._tailBoneNextPosW) - 90)
									- _worldMatrix._angleDeg
									//- apBoneIKChainUnit.Vector2Angle(_IKController._effectorBone._worldMatrix_NonModified._pos - _worldMatrix._pos)
									,
									_calculatedBoneIKWeight
									);
							}
							//else
							//{
							//	Debug.LogError("LookAt IK Failed");
							//}
						}
					}
				}
			}
			else
			{
				//추가 19.8.16 : 만약 IK ChainSet이 없거나 비활성화 될 때, IK Controller가 LookAt이라면
				//단일 본에 대해서도 LookAt을 처리할 수 있어야 한다.
				if(_IKController != null && _IKController._controllerType != apOptBoneIKController.CONTROLLER_TYPE.None)
				{
					//Debug.LogError("IK Controller Not Work : [" + _IKController._controllerType + "]");
					if(_IKController._controllerType == apOptBoneIKController.CONTROLLER_TYPE.LookAt)
					{
						//LookAt IK에 한해서 단일 본에서도 IK가 처리될 수 있다.
						if(CalculateSingleLookAtIK())
						{
							IKCalculated = true;
						}
					}
				}
			}

			//자식 본도 업데이트
			if(isRecursive)
			{
				if (_childBones != null)
				{
					for (int i = 0; i < _childBones.Length; i++)
					{
						if (_childBones[i].CalculateIK(true))
						{
							//자식 본 중에 처리 결과가 True라면
							//나중에 전체 True 처리
							IKCalculated = true;
						}
					}
				}
			}

			return IKCalculated;
		}

		/// <summary>
		/// IK가 포함된 WorldMatrix를 계산하는 함수
		/// 렌더링 직전에만 따로 수행한다.
		/// IK Controller에 의한 IK 연산이 있다면 이 함수에서 계산 및 WorldMatrix
		/// IK용 GUI 업데이트도 동시에 실행된다.
		/// </summary>
		public void MakeWorldMatrixForIK(bool isRecursive, bool isCalculateMatrixForce)
		{
			if(_isIKCalculated_Controlled)
			{
				//IK가 계산된 결과를 넣자
				//World Matrix 재계산
				float prevWorldMatrixAngle = _worldMatrix._angleDeg;

				_worldMatrix.SetMatrix(_defaultMatrix);
				_worldMatrix.Add(_localMatrix);


				if (_parentBone == null)
				{
					if (_parentOptTransform != null)
					{
						_worldMatrix.RMultiply(_parentOptTransform._matrix_TFResult_World);
					}
				}
				else
				{
					_worldMatrix.RMultiply(_parentBone._worldMatrix);
				}

				//여기서도 External Request를 적용해준다.
				UpdateExternalRequest();

				
				//IK 적용
				if (_IKRequestWeight_Controlled > 1.0f)
				{
					_worldMatrix.SetRotate(prevWorldMatrixAngle + (_IKRequestAngleResult_Controlled / _IKRequestWeight_Controlled));
				}
				else if (_IKRequestWeight_Controlled > 0.0f)
				{
					//기존 코드
					//_worldMatrix.SetRotate(
					//		prevWorldMatrixAngle * (1 - _IKRequestWeight_Controlled)
					//		+ (prevWorldMatrixAngle + _IKRequestAngleResult_Controlled / _IKRequestWeight_Controlled) * _IKRequestWeight_Controlled);

					//Slerp가 적용된 코드
					_worldMatrix.SetRotate(
						apUtil.AngleSlerp(	prevWorldMatrixAngle,
						prevWorldMatrixAngle + (_IKRequestAngleResult_Controlled / _IKRequestWeight_Controlled),
						_IKRequestWeight_Controlled)
						);
				}

				isCalculateMatrixForce = true;//<<다음 Child 부터는 무조건 갱신을 해야한다.
			}
			else if(isCalculateMatrixForce)
			{
				//Debug.Log("IK Force [" + _name + "] : " + _IKRequestAngleResult_Controlled);

				//IK 자체는 적용되지 않았으나, Parent에서 적용된게 있어서 WorldMatrix를 그대로 쓸 순 없다.
				_worldMatrix.SetMatrix(_defaultMatrix);
				_worldMatrix.Add(_localMatrix);

				if (_parentBone == null)
				{
					if (_parentOptTransform != null)
					{
						_worldMatrix.RMultiply(_parentOptTransform._matrix_TFResult_World);
					}
				}
				else
				{
					_worldMatrix.RMultiply(_parentBone._worldMatrix);
				}
				//_isIKRendered_Controller = true;
			}
			else
			{
				//World Matrix와 동일하다.
				//생략
				//_worldMatrix_IK.SetMatrix(_worldMatrix);//<<동일하다.
			}


			
			//자식 본도 업데이트
			if(isRecursive)
			{
				if (_childBones != null)
				{
					for (int i = 0; i < _childBones.Length; i++)
					{
						_childBones[i].MakeWorldMatrixForIK(true, isCalculateMatrixForce);
					}
				}
			}
		}

		//추가 19.8.16 : 단일 본에 대한 LookAt 처리
		private bool CalculateSingleLookAtIK()
		{
			if(_IKController == null)
			{
				return false;
			}

			if(_IKController._controllerType != apOptBoneIKController.CONTROLLER_TYPE.LookAt)
			{
				return false;
			}

			if(_IKController._effectorBone == null)
			{
				return false;
			}

			//Control Param의 영향을 받는다.
			if (_IKController._controllerType != apOptBoneIKController.CONTROLLER_TYPE.None
				&& _IKController._isWeightByControlParam
				&& _IKController._weightControlParam != null)
			{
				_calculatedBoneIKWeight = Mathf.Clamp01(_IKController._weightControlParam._float_Cur);
			}

			//if(_IKController._isWeightByControlParam && _IKController._weightControlParam == null)
			//{
			//	Debug.LogError("Control Param을 찾을 수 없다.");
			//}

			if(_calculatedBoneIKWeight <= 0.001f)
			{
				return false;
			}

			Vector2 srcPos = _worldMatrix._pos;
			Vector2 dstPos = _IKController._effectorBone._worldMatrix._pos;

			float dist = Vector2.Distance(srcPos, dstPos);
			if(dist < 0.00001f)
			{
				return false;
			}
			float angleToDst = Mathf.Atan2(dstPos.y - srcPos.y, dstPos.x - srcPos.x);
			angleToDst *= Mathf.Rad2Deg;
			angleToDst -= 90.0f;

			angleToDst -= _worldMatrix._angleDeg;

			angleToDst = apUtil.AngleTo180(angleToDst);

			AddIKAngle_Controlled(angleToDst, _calculatedBoneIKWeight);

			//Debug.LogWarning("[" + _name + "] Single LookAt : " + angleToDst);

			return true;
		}

		/// <summary>
		/// 외부에서 요청한 TRS / IK를 적용한다.
		/// 2번 호출될 수 있다. (MakeWorldMatrix / MakeWorldMatrix_IK)
		/// </summary>
		private void UpdateExternalRequest()
		{
			//처리된 TRS
			_updatedWorldPos_NoRequest.x = _worldMatrix._pos.x;
			_updatedWorldPos_NoRequest.y = _worldMatrix._pos.y;

			_updatedWorldAngle_NoRequest = _worldMatrix._angleDeg;

			_updatedWorldScale_NoRequest.x = _worldMatrix._scale.x;
			_updatedWorldScale_NoRequest.y = _worldMatrix._scale.y;

			_updatedWorldPos = _updatedWorldPos_NoRequest;
			_updatedWorldAngle = _updatedWorldAngle_NoRequest;
			_updatedWorldScale = _updatedWorldScale_NoRequest;


			if(_isIKCalculated)
			{
				_IKRequestAngleResult -= 90.0f;
				while(_IKRequestAngleResult > 180.0f)	{ _IKRequestAngleResult -= 360.0f; }
				while(_IKRequestAngleResult < -180.0f)	{ _IKRequestAngleResult += 360.0f; }

				//_updatedWorldAngle += _IKRequestAngleResult * _IKRequestWeight;
				_updatedWorldAngle = _updatedWorldAngle * (1.0f - _IKRequestWeight) + (_IKRequestAngleResult * _IKRequestWeight);
				//Debug.Log("Add IK [" + _name + "] : " + _IKRequestAngleResult);

				_IKRequestAngleResult = 0.0f;
				_IKRequestWeight = 0.0f;
			}


			

			//스크립트로 외부에서 제어한 경우
			if (_isExternalUpdate_Position)
			{
				_updatedWorldPos.x = (_exUpdate_Pos.x * _externalUpdateWeight) + (_updatedWorldPos.x * (1.0f - _externalUpdateWeight));
				_updatedWorldPos.y = (_exUpdate_Pos.y * _externalUpdateWeight) + (_updatedWorldPos.y * (1.0f - _externalUpdateWeight));
			}

			if (_isExternalUpdate_Rotation)
			{
				_updatedWorldAngle = (_exUpdate_Angle * _externalUpdateWeight) + (_updatedWorldAngle * (1.0f - _externalUpdateWeight));
			}

			if(_isExternalUpdate_Scaling)
			{ 
				_updatedWorldScale.x = (_exUpdate_Scale.x * _externalUpdateWeight) + (_updatedWorldScale.x * (1.0f - _externalUpdateWeight));
				_updatedWorldScale.y = (_exUpdate_Scale.y * _externalUpdateWeight) + (_updatedWorldScale.y * (1.0f - _externalUpdateWeight));
			}

			if(_isExternalConstraint)
			{
				if(_isExternalConstraint_Xsurface)
				{

				}
				if(_isExternalConstraint_Xpref)
				{
					_updatedWorldPos.x = _externalConstraint_PosX.y;
				}
				if(_isExternalConstraint_Ypref)
				{
					_updatedWorldPos.y = _externalConstraint_PosY.y;
				}
				if (_isExternalConstraint_Xmin)
				{
					_updatedWorldPos.x = Mathf.Max(_updatedWorldPos.x, _externalConstraint_PosX.x);
				}
				if (_isExternalConstraint_Xmax)
				{
					_updatedWorldPos.x = Mathf.Min(_updatedWorldPos.x, _externalConstraint_PosX.z);
				}
				if (_isExternalConstraint_Ymin)
				{
					_updatedWorldPos.y = Mathf.Max(_updatedWorldPos.y, _externalConstraint_PosY.x);
				}
				if (_isExternalConstraint_Ymax)
				{
					_updatedWorldPos.y = Mathf.Min(_updatedWorldPos.y, _externalConstraint_PosY.z);
				}

				if(_isExternalConstraint_Xsurface)
				{
					_updatedWorldPos.x = Mathf.Clamp((_updatedWorldPos.x - _externalConstraint_PosSurfaceX.x) + _externalConstraint_PosSurfaceX.y, _externalConstraint_PosSurfaceX.z, _externalConstraint_PosSurfaceX.w);
				}
				if(_isExternalConstraint_Ysurface)
				{
					_updatedWorldPos.y = Mathf.Clamp((_updatedWorldPos.y - _externalConstraint_PosSurfaceY.x) + _externalConstraint_PosSurfaceY.y, _externalConstraint_PosSurfaceY.z, _externalConstraint_PosSurfaceY.w);
				}
			}
			

			if (_isIKCalculated || _isExternalUpdate_Position || _isExternalUpdate_Rotation || _isExternalUpdate_Scaling || _isExternalConstraint)
			{
				//WorldMatrix를 갱신해주자
				_worldMatrix.SetTRS(_updatedWorldPos.x, _updatedWorldPos.y,
										_updatedWorldAngle,
										_updatedWorldScale.x, _updatedWorldScale.y);

				//>> 이건 Post에서 처리
				//_isIKCalculated = false;
				//_isExternalUpdate_Position = false;
				//_isExternalUpdate_Rotation = false;
				//_isExternalUpdate_Scaling = false;
				//
			}
		}

		/// <summary>
		/// 모든 WorldMatrix가 끝나고, WorldMatrix에 영향을 받는 변수들을 갱신한다.
		/// </summary>
		public void UpdatePostRecursive()
		{
			//TODO : 이건 나중에 일괄적으로 업데이트 해야한다.
#if UNITY_EDITOR
			_shapePoint_End = new Vector2(0.0f, _shapeLength);


			_shapePoint_Mid1 = new Vector2(-_shapeWidth * 0.5f, _shapeLength * 0.2f);
			_shapePoint_Mid2 = new Vector2(_shapeWidth * 0.5f, _shapeLength * 0.2f);

			float taperRatio = Mathf.Clamp01((float)(100 - _shapeTaper) / 100.0f);

			_shapePoint_End1 = new Vector2(-_shapeWidth * 0.5f * taperRatio, _shapeLength);
			_shapePoint_End2 = new Vector2(_shapeWidth * 0.5f * taperRatio, _shapeLength);

			_shapePoint_End = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_End);
			_shapePoint_Mid1 = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_Mid1);
			_shapePoint_Mid2 = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_Mid2);
			_shapePoint_End1 = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_End1);
			_shapePoint_End2 = _worldMatrix.MtrxToSpace.MultiplyPoint(_shapePoint_End2);
#endif

			//TODO : 이것도 나중에 일괄적으로 업데이트 해야한다.

			//Rigging을 위해서 Matrix 통합 식을 만들자
			//실제 식
			// world * default_inv * VertPos W
			_vertWorld2BoneModWorldMatrix = _worldMatrix.MtrxToSpace;
			_vertWorld2BoneModWorldMatrix *= _worldMatrix_NonModified.MtrxToLowerSpace;

			
			


			if (_socketTransform != null)
			{
				//소켓을 업데이트 하자
				_socketTransform.localPosition = new Vector3(_worldMatrix._pos.x, _worldMatrix._pos.y, 0);
				_socketTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, _worldMatrix._angleDeg);
				_socketTransform.localScale = new Vector3(_worldMatrix._scale.x, _worldMatrix._scale.y, 1.0f);
			}

			_isIKCalculated = false;
			_isExternalUpdate_Position = false;
			_isExternalUpdate_Rotation = false;
			_isExternalUpdate_Scaling = false;

			_isExternalConstraint = false;
			_isExternalConstraint_Xmin = false;
			_isExternalConstraint_Xmax = false;
			_isExternalConstraint_Ymin = false;
			_isExternalConstraint_Ymax = false;
			_isExternalConstraint_Xpref = false;
			_isExternalConstraint_Ypref = false;
			_isExternalConstraint_Xsurface = false;
			_isExternalConstraint_Ysurface = false;

			if (_childBones != null)
			{
				for (int i = 0; i < _childBones.Length; i++)
				{
					_childBones[i].UpdatePostRecursive();

				}
			}
		}



		// Functions
		//---------------------------------------------------------------
		// 외부 제어 코드를 넣자
		// <Portrait 기준으로 Local Space = Bone 기준으로 World Space 로 설정한다 >
		/// <summary>
		/// Set Position
		/// </summary>
		/// <param name="worldPosition"></param>
		/// <param name="weight"></param>
		public void SetPosition(Vector2 worldPosition, float weight = 1.0f)
		{
			_isExternalUpdate_Position = true;
			_externalUpdateWeight = Mathf.Clamp01(weight);
			_exUpdate_Pos = worldPosition;
		}

		/// <summary>
		/// Set Rotation
		/// </summary>
		/// <param name="worldAngle"></param>
		/// <param name="weight"></param>
		public void SetRotation(float worldAngle, float weight = 1.0f)
		{
			_isExternalUpdate_Rotation = true;
			_externalUpdateWeight = Mathf.Clamp01(weight);
			_exUpdate_Angle = worldAngle;
		}


		/// <summary>
		/// Set Scale
		/// </summary>
		/// <param name="worldScale"></param>
		/// <param name="weight"></param>
		public void SetScale(Vector2 worldScale, float weight = 1.0f)
		{
			_isExternalUpdate_Scaling = true;
			_externalUpdateWeight = Mathf.Clamp01(weight);
			_exUpdate_Scale = worldScale;
		}
		

		/// <summary>
		/// Set TRS (Position, Rotation, Scale)
		/// </summary>
		/// <param name="worldPosition"></param>
		/// <param name="worldAngle"></param>
		/// <param name="worldScale"></param>
		/// <param name="weight"></param>
		public void SetTRS(Vector2 worldPosition, float worldAngle, Vector2 worldScale, float weight = 1.0f)
		{
			_isExternalUpdate_Position = true;
			_isExternalUpdate_Rotation = true;
			_isExternalUpdate_Scaling = true;

			_externalUpdateWeight = Mathf.Clamp01(weight);
			_exUpdate_Pos = worldPosition;
			_exUpdate_Angle = worldAngle;
			_exUpdate_Scale = worldScale;
		}

		public void SetPositionConstraint(float worldPositionValue, ConstraintBound constraintBound)
		{
			//Debug.Log("PosConst [" + _name + "] : " + constraintBound + " >>  " + worldPositionValue);
			_isExternalConstraint = true;
			switch (constraintBound)
			{
				case ConstraintBound.Xprefer:
					_isExternalConstraint_Xpref = true;
					_externalConstraint_PosX.y = worldPositionValue;
					break;

				case ConstraintBound.Xmin:
					_isExternalConstraint_Xmin = true;
					_externalConstraint_PosX.x = worldPositionValue;
					break;

				case ConstraintBound.Xmax:
					_isExternalConstraint_Xmax = true;
					_externalConstraint_PosX.z = worldPositionValue;
					break;

				case ConstraintBound.Yprefer:
					_isExternalConstraint_Ypref = true;
					_externalConstraint_PosY.y = worldPositionValue;
					break;
				case ConstraintBound.Ymin:
					_isExternalConstraint_Ymin = true;
					_externalConstraint_PosY.x = worldPositionValue;
					break;

				case ConstraintBound.Ymax:
					_isExternalConstraint_Ymax = true;
					_externalConstraint_PosY.z = worldPositionValue;
					break;
			}
		}

		public void SetPositionConstraintSurface(float defaultSurfacePos, float curSufracePos, float minSurfacePos, float maxSurfacePos, ConstraintSurface constraintSurface)
		{
			_isExternalConstraint = true;
			switch (constraintSurface)
			{
				case ConstraintSurface.Xsurface:
					_isExternalConstraint_Xsurface = true;
					_externalConstraint_PosSurfaceX.x = defaultSurfacePos;
					_externalConstraint_PosSurfaceX.y = curSufracePos;
					_externalConstraint_PosSurfaceX.z = minSurfacePos;
					_externalConstraint_PosSurfaceX.w = maxSurfacePos;
					break;

				case ConstraintSurface.Ysurface:
					_isExternalConstraint_Ysurface = true;
					_externalConstraint_PosSurfaceY.x = defaultSurfacePos;
					_externalConstraint_PosSurfaceY.y = curSufracePos;
					_externalConstraint_PosSurfaceY.z = minSurfacePos;
					_externalConstraint_PosSurfaceY.w = maxSurfacePos;
					break;
			}
		}



		// IK 요청을 하자
		//-------------------------------------------------------------------------------
		/// <summary>
		/// IK is calculated. Depending on the location requested, all Bones connected by IK move automatically.
		/// </summary>
		/// <param name="targetPosW"></param>
		/// <param name="weight"></param>
		/// <param name="isContinuous"></param>
		/// <returns></returns>
		public bool RequestIK(Vector2 targetPosW, float weight, bool isContinuous)
		{
			if (!_isIKTail || _IKChainSet == null || !_isIKChainSetAvailable)
			{
				//Debug.LogError("End -> _isIKTail : " + _isIKTail + " / _IKChainSet : " + _IKChainSet);
				return false;
			}

			if(!_isIKChainInit)
			{
				InitIKChain();
				_isIKChainInit = true;
			}


			bool isSuccess = _IKChainSet.SimulateIK(targetPosW, isContinuous);

			//IK가 실패하면 패스
			if (!isSuccess)
			{
				//Debug.LogError("Failed");
				return false;
			}

			//IK 결과값을 Bone에 넣어주자
			_IKChainSet.AdaptIKResultToBones(weight);

			//Debug.Log("Success");

			return true;
		}

		/// <summary>
		/// [Please do not use it] Initialize IK Chain
		/// </summary>
		public void InitIKChain()
		{
			if(_IKChainSet != null && _isIKChainSetAvailable)
			{
				if (_IKChainSet._bone == null)
				{
					_IKChainSet._bone = this;
					Debug.LogError("AnyPortrait : BoneIK Settings are wrong : "+ _name);
				}
				_IKChainSet.RefreshChain();
			}

			//if(_IKController != null && _isIKChainSetAvailable && _IKController._controllerType != apOptBoneIKController.CONTROLLER_TYPE.None)
			if(_IKController != null && _IKController._controllerType != apOptBoneIKController.CONTROLLER_TYPE.None)
			{
				//변경 : _isIKChainSetAvailable가 false여도 일단 Link를 하자.
				_IKController.Link(_parentOptTransform._portrait);
			}

			if (_childBones != null)
			{
				for (int i = 0; i < _childBones.Length; i++)
				{
					_childBones[i].InitIKChain();
				}
			}
		}

		// Get / Set
		//---------------------------------------------------------------
		// boneID를 가지는 Bone을 자식 노드로 두고 있는가.
		// 재귀적으로 찾는다.
		public apOptBone GetChildBoneRecursive(int boneID)
		{
			if (_childBones == null)
			{
				return null;
			}
			//바로 아래의 자식 노드를 검색
			for (int i = 0; i < _childBones.Length; i++)
			{
				if (_childBones[i]._uniqueID == boneID)
				{
					return _childBones[i];
				}
			}

			//못찾았다면..
			//재귀적으로 검색해보자

			for (int i = 0; i < _childBones.Length; i++)
			{
				apOptBone result = _childBones[i].GetChildBoneRecursive(boneID);
				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		// 바로 아래의 자식 Bone을 검색한다.
		public apOptBone GetChildBone(int boneID)
		{
			//바로 아래의 자식 노드를 검색
			for (int i = 0; i < _childBones.Length; i++)
			{
				if (_childBones[i]._uniqueID == boneID)
				{
					return _childBones[i];
				}
			}

			return null;
		}

		// 자식 Bone 중에서 특정 Target Bone을 재귀적인 자식으로 가지는 시작 Bone을 찾는다.
		public apOptBone FindNextChainedBone(int targetBoneID)
		{
			//바로 아래의 자식 노드를 검색
			if (_childBones == null)
			{
				return null;
			}
			for (int i = 0; i < _childBones.Length; i++)
			{
				if (_childBones[i]._uniqueID == targetBoneID)
				{
					return _childBones[i];
				}
			}

			//못찾았다면..
			//재귀적으로 검색해서, 그 중에 실제로 Target Bone을 포함하는 Child Bone을 리턴하자

			for (int i = 0; i < _childBones.Length; i++)
			{
				apOptBone result = _childBones[i].GetChildBoneRecursive(targetBoneID);
				if (result != null)
				{
					//return result;
					return _childBones[i];//<<Result가 아니라, ChildBone을 리턴
				}
			}
			return null;
		}

		// 요청한 boneID를 가지는 Bone을 부모 노드로 두고 있는가.
		// 재귀적으로 찾는다.
		public apOptBone GetParentRecursive(int boneID)
		{
			if (_parentBone == null)
			{
				return null;
			}

			if (_parentBone._uniqueID == boneID)
			{
				return _parentBone;
			}

			//재귀적으로 검색해보자
			return _parentBone.GetParentRecursive(boneID);

		}


		//-----------------------------------------------------------------------------------------------
		/// <summary>Bone's Position</summary>
		public Vector3 Position { get { return _updatedWorldPos; } }

		/// <summary>Bone's Angle (Degree)</summary>
		public float Angle {  get { return _updatedWorldAngle; } }

		/// <summary>Bone's Scale</summary>
		public Vector3 Scale { get { return _updatedWorldScale; } }

		
		/// <summary>Bone's Position without User's external request</summary>
		public Vector3 PositionWithouEditing {  get { return _updatedWorldPos_NoRequest; } }
		
		/// <summary>Bone's Angle without User's external request</summary>
		public float AngleWithouEditing {  get { return _updatedWorldAngle_NoRequest; } }
		
		/// <summary>Bone's Scale without User's external request</summary>
		public Vector3 ScaleWithouEditing {  get { return _updatedWorldScale_NoRequest; } }


		//-----------------------------------------------------------------------------------------------


//		// Gizmo Event
//#if UNITY_EDITOR
//		void OnDrawGizmosSelected()
//		{
//			Gizmos.color = _color;

//			Matrix4x4 tfMatrix = transform.localToWorldMatrix;
//			Gizmos.DrawLine(tfMatrix.MultiplyPoint3x4(_worldMatrix._pos), tfMatrix.MultiplyPoint3x4(_shapePoint_End));

//			Gizmos.DrawLine(tfMatrix.MultiplyPoint3x4(_worldMatrix._pos), tfMatrix.MultiplyPoint3x4(_shapePoint_Mid1));
//			Gizmos.DrawLine(tfMatrix.MultiplyPoint3x4(_worldMatrix._pos), tfMatrix.MultiplyPoint3x4(_shapePoint_Mid2));
//			Gizmos.DrawLine(tfMatrix.MultiplyPoint3x4(_shapePoint_Mid1), tfMatrix.MultiplyPoint3x4(_shapePoint_End1));
//			Gizmos.DrawLine(tfMatrix.MultiplyPoint3x4(_shapePoint_Mid2), tfMatrix.MultiplyPoint3x4(_shapePoint_End2));
//			Gizmos.DrawLine(tfMatrix.MultiplyPoint3x4(_shapePoint_End1), tfMatrix.MultiplyPoint3x4(_shapePoint_End2));
//		}
//#endif
	}

}