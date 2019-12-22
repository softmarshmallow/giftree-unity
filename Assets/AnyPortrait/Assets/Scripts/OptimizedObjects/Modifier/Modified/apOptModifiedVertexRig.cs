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
	/// apModifiedVertexRig의 Opt 버전
	/// </summary>
	[Serializable]
	public class apOptModifiedVertexRig
	{
		// Members
		//-----------------------------------------------
		public int _vertexUniqueID = -1;
		public int _vertIndex = -1;

		public apOptMesh _mesh = null;

		[Serializable]
		public class OptWeightPair
		{
			public int _boneID = -1;

			[SerializeField]
			public apOptBone _bone = null;

			public int _meshGroupID = -1;

			[SerializeField]
			public apOptTransform _meshGroup = null;

			public float _weight = 0.0f;

			public OptWeightPair(apModifiedVertexRig.WeightPair srcWeightPair)
			{
				_boneID = srcWeightPair._boneID;
				_meshGroupID = srcWeightPair._meshGroupID;
				_weight = srcWeightPair._weight;
			}

			public bool Link(apPortrait portrait)
			{
				_meshGroup = portrait.GetOptTransformAsMeshGroup(_meshGroupID);
				if (_meshGroup == null)
				{
					Debug.LogError("VertRig Bake 실패 : MeshGroup을 찾을 수 없다. [" + _meshGroupID + "]");
					return false;
				}

				_bone = _meshGroup.GetBone(_boneID);
				if (_bone == null)
				{
					Debug.LogError("VertRig Bake 실패 : Bone을 찾을 수 없다. [" + _boneID + "]");
					return false;
				}

				return true;
			}
		}

		[SerializeField]
		public OptWeightPair[] _weightPairs = null;



		// Init
		//-----------------------------------------------------------
		public apOptModifiedVertexRig()
		{

		}

		public bool Bake(apModifiedVertexRig srcModVert, apOptMesh mesh, apPortrait portrait)
		{
			_vertexUniqueID = srcModVert._vertexUniqueID;
			_vertIndex = srcModVert._vertIndex;
			_mesh = mesh;

			//변경 : 8.2 유효한 Weight Pair만 전달하자
			List<apModifiedVertexRig.WeightPair> validSrcWeightPairs = new List<apModifiedVertexRig.WeightPair>();
			for (int i = 0; i < srcModVert._weightPairs.Count; i++)
			{
				apModifiedVertexRig.WeightPair srcWeightPair = srcModVert._weightPairs[i];
				if(srcWeightPair == null)
				{
					continue;
				}
				if(srcWeightPair._weight <= 0.00001f)
				{
					continue;;
				}
				validSrcWeightPairs.Add(srcWeightPair);
			}

			_weightPairs = new OptWeightPair[validSrcWeightPairs.Count];

			//변경 : 8.2 유효한 Vertex가 없어도 일단 Bake는 되어야 한다.
			//if(srcModVert._weightPairs.Count == 0)
			//{
			//	//Rig가 안된 Vertex가 있다.
			//	Debug.LogError("AnyPortrait : There is a vertex with no rigging data. The Rigging Modifier is not applied to this mesh [" + mesh.name + "]. Check the Rigging value.");
			//	return false;
			//}

			//for (int i = 0; i < srcModVert._weightPairs.Count; i++)
			for (int i = 0; i < validSrcWeightPairs.Count; i++)
			{
				//apModifiedVertexRig.WeightPair srcWeightPair = srcModVert._weightPairs[i];
				apModifiedVertexRig.WeightPair srcWeightPair = validSrcWeightPairs[i];
				
				//추가 : 유효한 Weight만 추가 > 이 코드가 버그를 발생시킨다.
				//if(srcWeightPair._weight <= 0.00001f)
				//{
				//	continue;
				//}
				OptWeightPair optWeightPair = new OptWeightPair(srcWeightPair);
				optWeightPair.Link(portrait);

				_weightPairs[i] = optWeightPair;
			}

			//변경 : 8.2 0개라도 Bake를 하는 것으로 변경
			//if(_weightPairs.Length == 0)
			//{
			//	//처리된 Pair가 없으면 false
			//	Debug.LogError("AnyPortrait : There is a vertex with no valid rigging data. The Rigging Modifier is not applied to this mesh [" + mesh.name + "]. Check the Rigging value.");
			//	return false;
			//}

			//추가 : Normalize를 하자
			float totalWeight = 0.0f;


			for (int i = 0; i < _weightPairs.Length; i++)
			{
				totalWeight += _weightPairs[i]._weight;
			}
			//Noamlize는 1 이상일 때에만
			//if (totalWeight > 0.0f)
			if (totalWeight > 1.0f)
			{
				//if(totalWeight > 1.0f)
				//{
				//	Debug.LogError("OPT Rigging Weight Normalize [" + totalWeight + "]");
				//}

				for (int i = 0; i < _weightPairs.Length; i++)
				{
					//Debug.LogError("[" + i + "] : " + _weightPairs[i]._weight + " >> " + (_weightPairs[i]._weight / totalWeight));
					_weightPairs[i]._weight /= totalWeight;
				}
			}
			//변경 8.2 : Weight 에러는 없다.
			//else
			//{
			//	//전체 Weight가 0이다?
			//	Debug.LogError("AnyPortrait : There is a vertex with a sum of Rigging values of zero. The Rigging Modifier is not applied to this mesh [" + mesh.name + "]. Check the Rigging value.");
			//	return false;
			//}



			return true;
		}
	}
}