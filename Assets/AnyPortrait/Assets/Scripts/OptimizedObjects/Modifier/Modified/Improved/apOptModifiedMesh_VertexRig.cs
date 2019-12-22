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
	/// 기존의 optModifiedMesh에서 Rigging 데이터 부분만 가져온 클래스
	/// 최적화를 위해서 분리가 되었다.
	/// 데이터가 훨씬 더 최적화되었다.
	/// </summary>
	[Serializable]
	public class apOptModifiedMesh_VertexRig
	{
		// Members
		//--------------------------------------------
		//[NonSerialized]
		//private apOptModifiedMeshSet _parentModMeshSet = null;

		[SerializeField]
		public int _nVertRigs = 0;

		


		//데이터가 조금 더 최적화된 VertexRig 데이터
		//버텍스 순서대로 저장되어 있으므로 버텍스에 대한 별도의 데이터는 가지고 있지 않다.
		[Serializable]
		public class VertexRig
		{
			//Sub Class
			//----------------------------------------------------------------
			[Serializable]
			public class WeightPair
			{
				[SerializeField]
				public int _boneID = -1;

				[NonSerialized]
				public apOptBone _bone = null;

				[SerializeField]
				public int _meshGroupID = -1;

				[SerializeField]
				public float _weight = 0.0f;

				
				public WeightPair()
				{

				}

				public void Bake(apModifiedVertexRig.WeightPair srcWeightPair)
				{
					_boneID = srcWeightPair._boneID;
					_meshGroupID = srcWeightPair._meshGroupID;
					_weight = srcWeightPair._weight;
				}

				public bool Link(apPortrait portrait, apCache<apOptTransform> cache)
				{
					_bone = null;

					//Cache를 통해서 일단 MeshGroup에 해당하는 optTransform을 찾자
					apOptTransform meshGroup = null;
					if (cache.IsContain(_meshGroupID))
					{
						meshGroup = cache.Get(_meshGroupID);
					}
					else
					{
						meshGroup = portrait.GetOptTransformAsMeshGroup(_meshGroupID);
						if (meshGroup == null)
						{
							Debug.LogError("VertRig Bake 실패 : MeshGroup을 찾을 수 없다. [" + _meshGroupID + "]");
							return false;
						}

						//캐시에 담고
						cache.Add(_meshGroupID, meshGroup);
					}

					//본을 찾자
					_bone = meshGroup.GetBone(_boneID);
					if (_bone == null)
					{
						Debug.LogError("VertRig Bake 실패 : Bone을 찾을 수 없다. [" + _boneID + "]");
						return false;
					}

					return true;
				}
			}

			// Members
			//----------------------------------------------------------------
			[SerializeField]
			public WeightPair[] _weightPairs = null;


			// Init
			//----------------------------------------------------------------
			public VertexRig()
			{

			}

			// Bake
			//----------------------------------------------------------------
			public bool Bake(apModifiedVertexRig srcModVert)
			{
				//변경 : 8.2 유효한 Weight Pair만 전달하자
				List<apModifiedVertexRig.WeightPair> validSrcWeightPairs = new List<apModifiedVertexRig.WeightPair>();
				for (int i = 0; i < srcModVert._weightPairs.Count; i++)
				{
					apModifiedVertexRig.WeightPair srcWeightPair = srcModVert._weightPairs[i];
					if (srcWeightPair == null)
					{
						continue;
					}
					if (srcWeightPair._weight <= 0.00001f)
					{
						continue;
						;
					}
					validSrcWeightPairs.Add(srcWeightPair);
				}

				_weightPairs = new WeightPair[validSrcWeightPairs.Count];

				for (int i = 0; i < validSrcWeightPairs.Count; i++)
				{
					apModifiedVertexRig.WeightPair srcWeightPair = validSrcWeightPairs[i];

					WeightPair optWeightPair = new WeightPair();
					optWeightPair.Bake(srcWeightPair);

					_weightPairs[i] = optWeightPair;
				}

				//Normalize를 하자
				float totalWeight = 0.0f;
				
				for (int i = 0; i < _weightPairs.Length; i++)
				{
					totalWeight += _weightPairs[i]._weight;
				}
				
				//Noamlize는 1 이상일 때에만
				//if (totalWeight > 0.0f)
				if (totalWeight > 1.0f)
				{
					for (int i = 0; i < _weightPairs.Length; i++)
					{
						_weightPairs[i]._weight /= totalWeight;
					}
				}

				return true;
			}

			//Link를 해야한다. (기존에는 없음)
			public void Link(apPortrait portrait, apCache<apOptTransform> cache)
			{
				if(_weightPairs != null && _weightPairs.Length > 0)
				{
					for (int i = 0; i < _weightPairs.Length; i++)
					{
						_weightPairs[i].Link(portrait, cache);
					}
				}
			}

		}

		//서브 클래스로 만듬 (기존의 apOptModifiedVertexRig를 사용하지 않는다.)
		[SerializeField]
		public VertexRig[] _vertRigs = null;

		// Init
		//--------------------------------------------
		public apOptModifiedMesh_VertexRig()
		{

		}

		public void Link(apPortrait portrait, apOptModifiedMeshSet parentModMeshSet)
		{
			//_parentModMeshSet = parentModMeshSet;

			//캐시를 만들어서 빠른 처리가 되도록 한다.
			apCache<apOptTransform> cache = new apCache<apOptTransform>();
			for (int i = 0; i < _nVertRigs; i++)
			{
				_vertRigs[i].Link(portrait, cache);
			}
		}


		// Init - Bake
		//--------------------------------------------
		public void Bake(List<apModifiedVertexRig> modVertRigs)
		{
			_nVertRigs = modVertRigs.Count;
			_vertRigs = new VertexRig[_nVertRigs];

			for (int i = 0; i < _nVertRigs; i++)
			{
				apModifiedVertexRig srcModVertRig = modVertRigs[i];
				VertexRig vertRig = new VertexRig();
				vertRig.Bake(srcModVertRig);
				
				_vertRigs[i] = vertRig;
			}
		}
	}
}