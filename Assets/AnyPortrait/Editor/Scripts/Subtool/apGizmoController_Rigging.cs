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
using UnityEditor;
using System.Collections;
using System;
using System.Collections.Generic;


using AnyPortrait;

namespace AnyPortrait
{

	public partial class apGizmoController
	{

		// 작성해야하는 함수
		// Select : int - (Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
		// Move : void - (Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex)
		// Rotate : void - (float deltaAngleW)
		// Scale : void - (Vector2 deltaScaleW)

		//	TODO : 현재 Transform이 가능한지도 알아야 할 것 같다.
		// Transform Position : void - (Vector2 pos, int depth)
		// Transform Rotation : void - (float angle)
		// Transform Scale : void - (Vector2 scale)
		// Transform Color : void - (Color color)

		// Pivot Return : apGizmos.TransformParam - ()

		// Multiple Select : int - (Vector2 mousePosGL_Min, Vector2 mousePosGL_Max, Vector2 mousePosW_Min, Vector2 mousePosW_Max, SELECT_TYPE areaSelectType)
		// FFD Style Transform : void - (List<object> srcObjects, List<Vector2> posWorlds)
		// FFD Style Transform Start : bool - ()

		// Vertex 전용 툴
		// SoftSelection() : bool
		// PressBlur(Vector2 pos, float tDelta) : bool

		//----------------------------------------------------------------
		// Gizmo - Rigging Modifier에서 Vertex 또는 Bone을 선택하고 제어
		// Test Posing On/Off에 따라 처리가 다르다.
		// Test Posing 상태에 따라서도 Vertex는 선택 가능하다.
		// Area 선택이 가능하다 (Vertex의 경우에 따라서)
		// TRS는 Bone에 대해서만 가능하다.
		//----------------------------------------------------------------
		public apGizmos.GizmoEventSet GetEventSet_Modifier_Rigging()
		{
			return new apGizmos.GizmoEventSet(Select__Modifier_Rigging,
												Unselect__Modifier_Rigging,
												Move__Modifier_Rigging,
												Rotate__Modifier_Rigging,
												Scale__Modifier_Rigging,
												TransformChanged_Position__Modifier_Rigging,
												TransformChanged_Rotate__Modifier_Rigging,
												TransformChanged_Scale__Modifier_Rigging,
												null, null, null,
												PivotReturn__Modifier_Rigging,
												MultipleSelect__Modifier_Rigging,
												null, null, null, 
												SyncBrushStatus__Modifier_Rigging, 
												PressBrush__Modifier_Rigging,
												apGizmos.TRANSFORM_UI.TRS_NoDepth,
												FirstLink__Modifier_Rigging,
												AddHotKeys__Modifier_Rigging);
		}

		public apGizmos.SelectResult FirstLink__Modifier_Rigging()
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return null;
			}

			if (Editor.Select.ModRenderVertListOfMod != null)
			{
				//return Editor.Select.ModRenderVertListOfMod.Count;
				return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVertListOfMod);
			}

			if (Editor.Select.Bone != null)
			{
				return apGizmos.SelectResult.Main.SetSingle(Editor.Select.Bone);
			}


			return null;
		}

		/// <summary>
		/// Rigging Modifier 내에서의 Gizmo 이벤트 : Vertex 선택 또는 Bone 선택 [단일 선택]
		/// </summary>
		/// <param name="mousePosGL"></param>
		/// <param name="mousePosW"></param>
		/// <param name="btnIndex"></param>
		/// <param name="selectType"></param>
		/// <returns></returns>
		public apGizmos.SelectResult Select__Modifier_Rigging(Vector2 mousePosGL, Vector2 mousePosW, int btnIndex, apGizmos.SELECT_TYPE selectType)
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return null;
			}

			// 선택은 Test Posing 상관없이 수행한다.
			// - Vertex > (선택 못했어도 일단 이전에 선택한 Vertex를 해제하지 않는다.)
			// - Bone > (선택 못했으면 이전에 선택한 Bone 해제)
			// Vertex나 Bone 선택이 되지 않았다면, 이전에 선택했던 Vertex, Bone을 모두 해제한다.

			// - Mesh Transform 선택 (Lock이 안걸린 경우. 이건 리턴하지 않는다)
			// (아무것도 선택하지 않은 경우) -> Vertex, Bone, Mesh Transform 해제

			bool isAnySelected = false;


			if (Editor.Select.ModRenderVertListOfMod == null)
			{
				return null;
			}

			if(Editor.Gizmos.IsBrushMode || Editor.Select.RiggingBrush_Mode != apSelection.RIGGING_BRUSH_TOOL_MODE.None)
			{
				//브러시 작동중이면 선택 안됨
				return null;
			}


			if (!Editor.Controller.IsMouseInGUI(mousePosGL))
			{
				if (Editor.Select.ModRenderVertListOfMod.Count > 0)
				{
					return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVertListOfMod);
				}
				if (Editor.Select.Bone != null)
				{
					return apGizmos.SelectResult.Main.SetSingle(Editor.Select.Bone);
				}
				return null;
			}
			int prevSelectedVertex = Editor.Select.ModRenderVertListOfMod.Count;
			//bool isAnyVertexSelected = false;
			//bool isAnyBoneSelected = false;

			List<apSelection.ModRenderVert> prevSelectedVertices = new List<apSelection.ModRenderVert>();
			for (int i = 0; i < Editor.Select.ModRenderVertListOfMod.Count; i++)
			{
				prevSelectedVertices.Add(Editor.Select.ModRenderVertListOfMod[i]);
			}

			//1. 버텍스 선택을 먼저
			bool isChildMeshTransformSelectable = Editor.Select.Modifier.IsTarget_ChildMeshTransform;


			if (Editor.Select.ExKey_ModMesh != null && Editor.Select.MeshGroup != null)
			{
				//bool selectVertex = false;
				//일단 선택한 Vertex가 클릭 가능한지 체크
				if (Editor.Select.ModRenderVertOfMod != null)
				{
					if (Editor.Select.ModRenderVertListOfMod.Count == 1)
					{
						if (Editor.Controller.IsVertexClickable(apGL.World2GL(Editor.Select.ModRenderVertOfMod._renderVert._pos_World), mousePosGL))
						{
							if (selectType == apGizmos.SELECT_TYPE.Subtract)
							{
								//삭제인 경우
								//ModVert가 아니라 ModVertRig인 점 주의
								Editor.Select.RemoveModVertexOfModifier(null, Editor.Select.ModRenderVertOfMod._modVertRig, null, Editor.Select.ModRenderVertOfMod._renderVert);
								//return apGizmos.SELECT_RESULT.None;
							}
							else
							{
								//그 외에는 => 그대로 갑시다.
								isAnySelected = true;
								//return apGizmos.SELECT_RESULT.SameSelected;
							}
							//return Editor.Select.ModRenderVertListOfMod.Count;
							return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVertListOfMod);
						}
					}
					else
					{
						//여러개라고 하네요.
						List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;
						for (int iModRenderVert = 0; iModRenderVert < modRenderVerts.Count; iModRenderVert++)
						{
							apSelection.ModRenderVert modRenderVert = modRenderVerts[iModRenderVert];

							if (Editor.Controller.IsVertexClickable(apGL.World2GL(modRenderVert._renderVert._pos_World), mousePosGL))
							{
								if (selectType == apGizmos.SELECT_TYPE.Subtract)
								{
									//삭제인 경우
									//하나 지우고 끝
									//결과는 List의 개수
									Editor.Select.RemoveModVertexOfModifier(null, modRenderVert._modVertRig, null, modRenderVert._renderVert);

								}
								else if (selectType == apGizmos.SELECT_TYPE.Add)
								{
									//Add 상태에서 원래 선택된걸 누른다면
									//추가인 경우 => 그대로
									isAnySelected = true;
								}
								else
								{
									//만약... new 라면?
									//다른건 초기화하고
									//얘만 선택해야함
									apRenderVertex selectedRenderVert = modRenderVert._renderVert;
									apModifiedVertexRig selectedModVertRig = modRenderVert._modVertRig;
									Editor.Select.SetModVertexOfModifier(null, null, null, null);
									Editor.Select.SetModVertexOfModifier(null, selectedModVertRig, null, selectedRenderVert);
									//return apGizmos.SELECT_RESULT.NewSelected;
									//return Editor.Select.ModRenderVertOfModList.Count;
								}

								//return Editor.Select.ModRenderVertListOfMod.Count;
								return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVertListOfMod);
							}
						}
					}

				}

				//이부분 주의
				//일단 날리되, "Bone을 선택했다면 이전에 선택한 vertex를 유지한다"를 지켜야한다.
				if (selectType == apGizmos.SELECT_TYPE.New)
				{
					//Add나 Subtract가 아닐땐, 잘못 클릭하면 선택을 해제하자 (전부)
					Editor.Select.SetModVertexOfModifier(null, null, null, null);
				}

				if (selectType != apGizmos.SELECT_TYPE.Subtract)
				{
					if (Editor.Select.ExKey_ModMesh._transform_Mesh != null &&
							Editor.Select.ExKey_ModMesh._vertRigs != null)
					{
						//선택된 RenderUnit을 고르자
						apRenderUnit targetRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.ExKey_ModMesh._transform_Mesh);

						if (targetRenderUnit != null)
						{
							for (int iVert = 0; iVert < targetRenderUnit._renderVerts.Count; iVert++)
							{
								apRenderVertex renderVert = targetRenderUnit._renderVerts[iVert];
								bool isClick = Editor.Controller.IsVertexClickable(apGL.World2GL(renderVert._pos_World), mousePosGL);
								if (isClick)
								{
									apModifiedVertexRig selectedModVertRig = Editor.Select.ExKey_ModMesh._vertRigs.Find(delegate (apModifiedVertexRig a)
									{
										return renderVert._vertex._uniqueID == a._vertexUniqueID;
									});

									if (selectedModVertRig != null)
									{
										if (selectType == apGizmos.SELECT_TYPE.New)
										{
											Editor.Select.SetModVertexOfModifier(null, selectedModVertRig, null, renderVert);
										}
										else if (selectType == apGizmos.SELECT_TYPE.Add)
										{
											Editor.Select.AddModVertexOfModifier(null, selectedModVertRig, null, renderVert);
										}

										isAnySelected = true;
										//isAnyVertexSelected = true;

										//result = apGizmos.SELECT_RESULT.NewSelected;
										break;
									}

								}
							}

							Editor.RefreshControllerAndHierarchy(false);
							//Editor.Repaint();
							Editor.SetRepaint();
						}
					}
				}

				if (isAnySelected)
				{
					Editor.Select.AutoSelectModMeshOrModBone();
				}
			}

			//if (!Editor.Select.IsLockExEditKey)
			//{
				//기존 << Selection Lock이 안걸려 있을때로 한정
			//}

			//변경 : Selection Lock에 상관없이 Bone을 선택할 수 있다.
			if (!isAnySelected)
			{
				//2. Bone을 선택하자
				//Bone은 Select Mode가 Subtract가 아닌 이상 무조건 작동을 한다.
				//만약, 잠금 버튼이 눌렸다면 -> Bone 선택하지 않았어도 해제를 하지 않는다.
				apMeshGroup meshGroup = Editor.Select.MeshGroup;
				apBone prevBone = Editor.Select.Bone;

				apBone bone = null;
				apBone resultBone = null;

				//<BONE_EDIT>
				//List<apBone> boneList = meshGroup._boneList_All;
				
				//for (int i = 0; i < boneList.Count; i++)
				//{
				//	bone = boneList[i];
				//	if (IsBoneClick(bone, mousePosW, mousePosGL, Editor._boneGUIRenderMode, Editor.Select.IsBoneIKRenderable))
				//	{
				//		if (resultBone == null || resultBone._depth < bone._depth)
				//		{
				//			resultBone = bone;
				//		}
				//	}
				//}

				//>>Bone Set으로 변경
				apMeshGroup.BoneListSet boneSet = null;
				for (int iSet = 0; iSet < meshGroup._boneListSets.Count; iSet++)
				{
					boneSet = meshGroup._boneListSets[iSet];
					for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)
					{
						bone = CheckBoneClick(boneSet._bones_Root[iRoot], mousePosW, mousePosGL, Editor._boneGUIRenderMode, -1, Editor.Select.IsBoneIKRenderable);
						if (bone != null)
						{
							resultBone = bone;
						}
					}
					if(resultBone != null)
					{
						//이 Set에서 선택이 완료되었다.
						break;
					}
				}

				if (resultBone != null)
				{
					bone = resultBone;
					if (selectType != apGizmos.SELECT_TYPE.Subtract)
					{
						Editor.Select.SetBone(bone);
						isAnySelected = true;
					}
					else
					{
						Editor.Select.SetBone(null);
					}

					//isAnyBoneSelected = true;
				}
				else
				{
					bone = null;
					//isAnyBoneSelected = false;
				}

				if (!isAnySelected)
				{
					//기존 : 선택한게 없으면 Bone을 Null로 만든다. 단, SelectionLock이 켜져 있으면 유지하도록 한다.
					//Editor.Select.SetBone(null);
					//if (Editor.Select.IsLockExEditKey)
					//{
					//	Editor.Select.SetBone(prevBone);//복구
					//}

					//변경 : Bone은 해제되지 않는다.
					if (Editor.Select.IsSelectionLock)
					{
						Editor.Select.SetBone(prevBone);//복구
					}

				}
				else
				{
					//Bone을 선택했다면
					//Vertex를 복구해주자
					for (int i = 0; i < prevSelectedVertices.Count; i++)
					{
						apSelection.ModRenderVert modRenderVert = prevSelectedVertices[i];
						Editor.Select.AddModVertexOfModifier(modRenderVert._modVert, modRenderVert._modVertRig, null, modRenderVert._renderVert);
					}
				}

				if (prevBone != Editor.Select.Bone)
				{
					_isBoneSelect_MovePosReset = true;
					Editor.RefreshControllerAndHierarchy(false);
				}
			}


			if (!Editor.Select.IsSelectionLock)
			{
				if (!isAnySelected && selectType == apGizmos.SELECT_TYPE.New)
				{
					//3. Mesh Transform을 선택하자
					//이건 선택 영역에 포함되지 않는다.
					apTransform_Mesh selectedMeshTransform = null;

					//정렬된 Render Unit
					//List<apRenderUnit> renderUnits = Editor.Select.MeshGroup._renderUnits_All;//<<이전 : RenderUnits All 이용
					List<apRenderUnit> renderUnits = Editor.Select.MeshGroup.SortedRenderUnits;//<<변경 : Sorted 리스트 사용

					for (int iUnit = 0; iUnit < renderUnits.Count; iUnit++)
					{
						apRenderUnit renderUnit = renderUnits[iUnit];
						if (renderUnit._meshTransform != null && renderUnit._meshTransform._mesh != null)
						{
							if (renderUnit._meshTransform._isVisible_Default && renderUnit._meshColor2X.a > 0.1f)//Alpha 옵션 추가
							{
								//Debug.LogError("TODO : Mouse Picking 바꿀것");
								bool isPick = apEditorUtil.IsMouseInRenderUnitMesh(
									mousePosGL, renderUnit);

								if (isPick)
								{
									selectedMeshTransform = renderUnit._meshTransform;
									//찾았어도 계속 찾는다.
									//뒤의 아이템이 "앞쪽"에 있는 것이기 때문
								}
							}
						}
					}

					if (selectedMeshTransform != null)
					{
						//만약 ChildMeshGroup에 속한 거라면,
						//Mesh Group 자체를 선택해야 한다. <- 추가 : Child Mesh Transform이 허용되는 경우 그럴 필요가 없다.
						apMeshGroup parentMeshGroup = Editor.Select.MeshGroup.FindParentMeshGroupOfMeshTransform(selectedMeshTransform);
						if (parentMeshGroup == null || parentMeshGroup == Editor.Select.MeshGroup || isChildMeshTransformSelectable)
						{
							Editor.Select.SetSubMeshInGroup(selectedMeshTransform);
						}
						else
						{
							apTransform_MeshGroup childMeshGroupTransform = Editor.Select.MeshGroup.FindChildMeshGroupTransform(parentMeshGroup);
							if (childMeshGroupTransform != null)
							{
								Editor.Select.SetSubMeshGroupInGroup(childMeshGroupTransform);
							}
							else
							{
								Editor.Select.SetSubMeshInGroup(selectedMeshTransform);
							}
						}
					}
					else
					{
						Editor.Select.SetSubMeshInGroup(null);
					}

					Editor.RefreshControllerAndHierarchy(false);
					//Editor.Repaint();
					Editor.SetRepaint();
				}
			}

			if (isAnySelected)
			{
				if (Editor.Select.ModRenderVertListOfMod != null && Editor.Select.ModRenderVertListOfMod.Count > 0)
				{
					//return Editor.Select.ModRenderVertListOfMod.Count;
					return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVertListOfMod);
				}

				if (Editor.Select.Bone != null)
				{
					return apGizmos.SelectResult.Main.SetSingle(Editor.Select.Bone);
				}
			}

			return null;

		}


		public void Unselect__Modifier_Rigging()
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return;
			}

			if(Editor.Gizmos.IsBrushMode || Editor.Select.RiggingBrush_Mode != apSelection.RIGGING_BRUSH_TOOL_MODE.None)
			{
				//브러시 모드에서 우클릭했으면 보류
				return;
			}

			//1. Vertex 먼저 해제
			//2. Bone 해제
			if (Editor.Select.ModRenderVertListOfMod.Count > 0)
			{
				Editor.Select.SetModVertexOfModifier(null, null, null, null);
			}
			else
			{
				Editor.Select.SetBone(null);
			}

			if (!Editor.Select.IsSelectionLock)
			{
				Editor.Select.SetSubMeshInGroup(null);
			}

			Editor.RefreshControllerAndHierarchy(false);
			Editor.SetRepaint();
		}



		//-----------------------------------------------------------------------------------------
		// 단축키
		//-----------------------------------------------------------------------------------------
		public void AddHotKeys__Modifier_Rigging()
		{
			Editor.AddHotKeyEvent(OnHotKeyEvent__Modifier_Rigging__Ctrl_A, "Select All Vertices", KeyCode.A, false, false, true, null);
		}

		// 단축키 : 버텍스 전체 선택
		private void OnHotKeyEvent__Modifier_Rigging__Ctrl_A(object paramObject)
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return;
			}

			if (Editor.Select.ModRenderVertListOfMod == null)
			{
				return;
			}
			
			if(Editor.Gizmos.IsBrushMode || Editor.Select.RiggingBrush_Mode != apSelection.RIGGING_BRUSH_TOOL_MODE.None)
			{
				//브러시 모드에서 우클릭했으면 무시
				return;
			}

			bool isAnyChanged = false;
			if (Editor.Select.ExKey_ModMesh != null && Editor.Select.MeshGroup != null)
			{
				//선택된 RenderUnit을 고르자
				apRenderUnit targetRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.ExKey_ModMesh._transform_Mesh);

				if (targetRenderUnit != null)
				{
					for (int iVert = 0; iVert < targetRenderUnit._renderVerts.Count; iVert++)
					{
						apRenderVertex renderVert = targetRenderUnit._renderVerts[iVert];

						apModifiedVertexRig selectedModVertRig = Editor.Select.ExKey_ModMesh._vertRigs.Find(delegate (apModifiedVertexRig a)
						{
							return renderVert._vertex._uniqueID == a._vertexUniqueID;
						});

						if (selectedModVertRig != null)
						{
							Editor.Select.AddModVertexOfModifier(null, selectedModVertRig, null, renderVert);

							isAnyChanged = true;
						}

					}

					Editor.RefreshControllerAndHierarchy(false);
					//Editor.Repaint();
					Editor.SetRepaint();
				}
			}

			if (isAnyChanged)
			{
				Editor.Gizmos.SetSelectResultForce_Multiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVertListOfMod);

				Editor.Select.AutoSelectModMeshOrModBone();
			}
		}

		//-----------------------------------------------------------------------------------------
		/// <summary>
		/// Rigging Modifier내에서의 Gizmo 이벤트 : Vertex 다중 선택. Bone은 선택하지 않는다.
		/// </summary>
		/// <param name="mousePosGL_Min"></param>
		/// <param name="mousePosGL_Max"></param>
		/// <param name="mousePosW_Min"></param>
		/// <param name="mousePosW_Max"></param>
		/// <param name="areaSelectType"></param>
		/// <returns></returns>
		public apGizmos.SelectResult MultipleSelect__Modifier_Rigging(Vector2 mousePosGL_Min, Vector2 mousePosGL_Max, Vector2 mousePosW_Min, Vector2 mousePosW_Max, apGizmos.SELECT_TYPE areaSelectType)
		{
			if (Editor.Select.MeshGroup == null || Editor.Select.Modifier == null)
			{
				return null;
			}


			if (Editor.Select.ModRenderVertListOfMod == null)
			{
				return null;
			}
			// 이건 다중 버텍스 선택밖에 없다.
			//Transform 선택은 없음

			//if (!Editor.Controller.IsMouseInGUI(mousePosGL))
			//{
			//	return apGizmos.SELECT_RESULT.None;
			//}

			//apGizmos.SELECT_RESULT result = apGizmos.SELECT_RESULT.None;

			if(Editor.Gizmos.IsBrushMode || Editor.Select.RiggingBrush_Mode != apSelection.RIGGING_BRUSH_TOOL_MODE.None)
			{
				//브러시 모드에서 우클릭했으면 무시
				return null;
			}

			bool isAnyChanged = false;
			if (Editor.Select.ExKey_ModMesh != null && Editor.Select.MeshGroup != null)
			{
				//선택된 RenderUnit을 고르자
				apRenderUnit targetRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.ExKey_ModMesh._transform_Mesh);

				if (targetRenderUnit != null)
				{
					for (int iVert = 0; iVert < targetRenderUnit._renderVerts.Count; iVert++)
					{
						apRenderVertex renderVert = targetRenderUnit._renderVerts[iVert];
						bool isSelectable = (mousePosW_Min.x < renderVert._pos_World.x && renderVert._pos_World.x < mousePosW_Max.x)
									&& (mousePosW_Min.y < renderVert._pos_World.y && renderVert._pos_World.y < mousePosW_Max.y);
						if (isSelectable)
						{
							apModifiedVertexRig selectedModVertRig = Editor.Select.ExKey_ModMesh._vertRigs.Find(delegate (apModifiedVertexRig a)
							{
								return renderVert._vertex._uniqueID == a._vertexUniqueID;
							});

							if (selectedModVertRig != null)
							{
								if (areaSelectType == apGizmos.SELECT_TYPE.Add ||
									areaSelectType == apGizmos.SELECT_TYPE.New)
								{
									Editor.Select.AddModVertexOfModifier(null, selectedModVertRig, null, renderVert);
								}
								else
								{
									Editor.Select.RemoveModVertexOfModifier(null, selectedModVertRig, null, renderVert);
								}

								isAnyChanged = true;
								//result = apGizmos.SELECT_RESULT.NewSelected;
								//break;
							}
						}
					}

					Editor.RefreshControllerAndHierarchy(false);
					//Editor.Repaint();
					Editor.SetRepaint();
				}


			}


			if (isAnyChanged)
			{
				Editor.Select.AutoSelectModMeshOrModBone();
			}

			//return Editor.Select.ModRenderVertListOfMod.Count;
			return apGizmos.SelectResult.Main.SetMultiple<apSelection.ModRenderVert>(Editor.Select.ModRenderVertListOfMod);
		}




		/// <summary>
		/// MeshGroup 메뉴 + Modifier 중 Rigging Modifier에서만 제어할 수 있다.
		/// Rigging 테스트를 위해 임시 WorldMatrix를 만들어서 움직인다.
		/// Rigging Modifier 활성할때마다 변수가 초기화됨.
		/// 자식 MeshGroup의 Bone도 제어 가능하다 (!)
		/// Default와 달리 IK Lock이 걸려있으므로 IK 계산을 해야한다.
		/// </summary>
		/// <param name="curMouseGL"></param>
		/// <param name="curMousePosW"></param>
		/// <param name="deltaMoveW"></param>
		/// <param name="btnIndex"></param>
		/// <param name="isFirstMove"></param>
		public void Move__Modifier_Rigging(Vector2 curMouseGL, Vector2 curMousePosW, Vector2 deltaMoveW, int btnIndex, bool isFirstMove)
		{
			if (Editor.Select.MeshGroup == null
				|| Editor.Select.Bone == null
				|| Editor._boneGUIRenderMode != apEditor.BONE_RENDER_MODE.Render
				|| Editor.Select.Modifier == null //<<Modifier가 null이면 안된다.
				|| !Editor.Controller.IsMouseInGUI(curMouseGL)
				|| deltaMoveW.sqrMagnitude == 0.0f
				)
			{
				return;
			}

			apBone bone = Editor.Select.Bone;
			apMeshGroup meshGroup = Editor.Select.MeshGroup;
			//apMeshGroup boneParentMeshGroup = bone._meshGroup;//Bone이 속한 MeshGroup. 다를 수 있다.
			apModifierBase modifier = Editor.Select.Modifier;//선택한 Modifier

			if (modifier.ModifierType != apModifierBase.MODIFIER_TYPE.Rigging)
			{
				//리깅 Modifier가 아니라면 패스
				return;
			}
			if (!Editor.Select.IsRigEditTestPosing)
			{
				//TestPosing이 허용되지 않았다면 패스
				return;
			}

			if(Editor.Gizmos.IsBrushMode || Editor.Select.RiggingBrush_Mode != apSelection.RIGGING_BRUSH_TOOL_MODE.None)
			{
				//브러시 모드에서는 무시
				return;
			}

			_boneSelectPosW = curMousePosW;

			//Move로 제어 가능한 경우는
			//1. IK Tail일 때
			//2. Root Bone일때 (절대값)
			if (bone._isIKTail)
			{
				//Debug.Log("Request IK : " + _boneSelectPosW);
				float weight = 1.0f;
				//if (deltaMoveW.sqrMagnitude < 5.0f)
				//{
				//	//weight = 0.2f;
				//}

				if (bone != _prevSelected_TransformBone || isFirstMove)
				{
					_prevSelected_TransformBone = bone;
					_prevSelected_MousePosW = bone._worldMatrix._pos;
				}

				_prevSelected_MousePosW += deltaMoveW;
				Vector2 bonePosW = _prevSelected_MousePosW;//DeltaPos + 절대 위치 절충

				//bool successIK = bone.RequestIK(_boneSelectPosW, weight, !isFirstSelectBone);
				//bool successIK = bone.RequestIK(_boneSelectPosW, weight, true);

				bool successIK = bone.RequestIK(bonePosW, weight, true);

				if (!successIK)
				{
					return;
				}




				apBone headBone = bone._IKHeaderBone;
				if (headBone != null)
				{
					apBone curBone = bone;
					//위로 올라가면서 IK 결과값을 Default에 적용하자
					while (true)
					{
						float deltaAngle = curBone._IKRequestAngleResult;
						//float nextAngle = curBone._defaultMatrix._angleDeg + deltaAngle;
						float nextAngle = curBone._rigTestMatrix._angleDeg + deltaAngle;//Rig Test로 할 것

						if (nextAngle < -180.0f)
						{ nextAngle += 360.0f; }
						if (nextAngle > 180.0f)
						{ nextAngle -= 360.0f; }

						//curBone._defaultMatrix.SetRotate(nextAngle);
						curBone._rigTestMatrix.SetRotate(nextAngle);

						curBone._isIKCalculated = false;
						curBone._IKRequestAngleResult = 0.0f;

						if (curBone == headBone)
						{
							break;
						}
						if (curBone._parentBone == null)
						{
							break;
						}
						curBone = curBone._parentBone;
					}

					//마지막으론 World Matrix 갱신
					//headBone.MakeWorldMatrix(true);//<<이전 : 단일 본만 업데이트
					//<BONE_EDIT>
					//if(headBone._meshGroup != null)
					//{
					//	headBone._meshGroup.UpdateBonesWorldMatrix();//<<변경 : 전체 본 업데이트 (IK 때문)
					//}

					
					//>Root MeshGroup에서 변경
					if(meshGroup != null)
					{
						meshGroup.UpdateBonesWorldMatrix();
					}

			
					headBone.GUIUpdate(true);
				}
			}
			else if (bone._parentBone == null
				|| (bone._parentBone._IKNextChainedBone != bone))
			{
				//수정 : Parent가 있지만 IK로 연결 안된 경우 / Parent가 없는 경우 2가지 모두 처리한다.

				apMatrix parentMatrix = null;
				if (bone._parentBone == null)
				{
					if (bone._renderUnit != null)
					{
						//Render Unit의 World Matrix를 참조하여
						//로컬 값을 Default로 적용하자
						parentMatrix = bone._renderUnit.WorldMatrixWrap;
					}
				}
				else
				{
					parentMatrix = bone._parentBone._worldMatrix;
				}

				apMatrix localMatrix = bone._localMatrix;
				apMatrix newWorldMatrix = new apMatrix(bone._worldMatrix);
				newWorldMatrix.SetPos(newWorldMatrix._pos + deltaMoveW);

				if (parentMatrix != null)
				{
					newWorldMatrix.RInverse(parentMatrix);
				}
				newWorldMatrix.Subtract(localMatrix);//이건 Add로 연산된거라 Subtract해야한다.



				//bone._defaultMatrix.SetPos(newWorldMatrix._pos);
				bone._rigTestMatrix.SetPos(newWorldMatrix._pos - bone._defaultMatrix._pos);


				//bone.MakeWorldMatrix(true);//이전 : 단일 본만 업데이트
				//<BONE_EDIT>
				//if(bone._meshGroup != null)
				//{
				//	bone._meshGroup.UpdateBonesWorldMatrix();//변경 : 전체 본 업데이트 (IK 때문)
				//}

				//>Root MeshGroup에서 변경
				if(meshGroup != null)
				{
					meshGroup.UpdateBonesWorldMatrix();
				}
				bone.GUIUpdate(true);
			}
		}



		/// <summary>
		/// MeshGroup 메뉴 + Modifier 중 Rigging Modifier에서만 제어할 수 있다.
		/// Rigging 테스트를 위해 임시 WorldMatrix를 만들어서 움직인다.
		/// Rigging Modifier 활성할때마다 변수가 초기화됨.
		/// 자식 MeshGroup의 Bone도 제어 가능하다 (!)
		/// IK의 영향을 받지 않는다.
		/// </summary>
		/// <param name="deltaAngleW"></param>
		public void Rotate__Modifier_Rigging(float deltaAngleW, bool isFirstRotate)
		{
			if (Editor.Select.MeshGroup == null
				|| Editor.Select.Bone == null
				|| Editor._boneGUIRenderMode != apEditor.BONE_RENDER_MODE.Render
				|| Editor.Select.Modifier == null //<<Modifier가 null이면 안된다.
				|| deltaAngleW == 0.0f
				)
			{
				return;
			}

			if (!Editor.Select.IsRigEditTestPosing)
			{
				//TestPosing이 허용되지 않았다면 패스
				return;
			}

			if(Editor.Gizmos.IsBrushMode || Editor.Select.RiggingBrush_Mode != apSelection.RIGGING_BRUSH_TOOL_MODE.None)
			{
				//브러시 모드에서는 무시
				return;
			}

			apBone bone = Editor.Select.Bone;
			apMeshGroup meshGroup = Editor.Select.MeshGroup;
			//apMeshGroup boneParentMeshGroup = bone._meshGroup;//Bone이 속한 MeshGroup. 다를 수 있다.
			apModifierBase modifier = Editor.Select.Modifier;//선택한 Modifier

			if (modifier.ModifierType != apModifierBase.MODIFIER_TYPE.Rigging)
			{
				//리깅 Modifier가 아니라면 패스
				return;
			}

			//Default Angle은 -180 ~ 180 범위 안에 들어간다.
			float nextAngle = bone._rigTestMatrix._angleDeg + deltaAngleW;
			if (nextAngle < -180.0f)
			{ nextAngle += 360.0f; }
			if (nextAngle > 180.0f)
			{ nextAngle -= 360.0f; }

			bone._rigTestMatrix.SetRotate(nextAngle);
			//bone.MakeWorldMatrix(true);//<<이전 : 단일 본 수정

			//<BONE_EDIT>
			//if(bone._meshGroup != null)
			//{
			//	bone._meshGroup.UpdateBonesWorldMatrix();//변경 : 전체 본 수정
			//}

			//>Root MeshGroup에서 변경
			if(meshGroup != null)
			{
				meshGroup.UpdateBonesWorldMatrix();
			}
			bone.GUIUpdate(true);
		}



		/// <summary>
		/// MeshGroup 메뉴 + Modifier 중 Rigging Modifier에서만 제어할 수 있다.
		/// Rigging 테스트를 위해 임시 WorldMatrix를 만들어서 움직인다.
		/// Rigging Modifier 활성할때마다 변수가 초기화됨.
		/// 자식 MeshGroup의 Bone도 제어 가능하다 (!)
		/// IK의 영향을 받지 않는다.
		/// </summary>
		/// <param name="deltaScaleW"></param>
		public void Scale__Modifier_Rigging(Vector2 deltaScaleW, bool isFirstScale)
		{
			if (Editor.Select.MeshGroup == null
				|| Editor.Select.Bone == null
				|| Editor._boneGUIRenderMode != apEditor.BONE_RENDER_MODE.Render
				|| Editor.Select.Modifier == null //<<Modifier가 null이면 안된다.
				|| deltaScaleW.sqrMagnitude == 0.0f
				)
			{
				return;
			}

			if (!Editor.Select.IsRigEditTestPosing)
			{
				//TestPosing이 허용되지 않았다면 패스
				return;
			}

			if(Editor.Gizmos.IsBrushMode || Editor.Select.RiggingBrush_Mode != apSelection.RIGGING_BRUSH_TOOL_MODE.None)
			{
				//브러시 모드에서는 무시
				return;
			}

			apBone bone = Editor.Select.Bone;
			apMeshGroup meshGroup = Editor.Select.MeshGroup;
			//apMeshGroup boneParentMeshGroup = bone._meshGroup;//Bone이 속한 MeshGroup. 다를 수 있다.
			apModifierBase modifier = Editor.Select.Modifier;//선택한 Modifier

			if (modifier.ModifierType != apModifierBase.MODIFIER_TYPE.Rigging)
			{
				//리깅 Modifier가 아니라면 패스
				return;
			}

			Vector3 prevScale = bone._rigTestMatrix._scale;
			Vector2 nextScale = new Vector2(prevScale.x + deltaScaleW.x, prevScale.y + deltaScaleW.y);

			bone._rigTestMatrix.SetScale(nextScale);
			//bone.MakeWorldMatrix(true);//<<이전 : 단일 본 갱신
			//<BONE_EDIT>
			//if(bone._meshGroup != null)
			//{
			//	bone._meshGroup.UpdateBonesWorldMatrix();//<<변경 : 전체 본 갱신
			//}

			//>Root MeshGroup에서 변경
			if(meshGroup != null)
			{
				meshGroup.UpdateBonesWorldMatrix();
			}
			bone.GUIUpdate(true);
		}


		//------------------------------------------------------------------------------------------
		// Bone - TransformChanged (Default / Rigging Test / Modifier / AnimClip Modifier)
		// Move 값 직접 수정. IK, Local Move 옵션에 따라 무시될 수 있다.
		// World 값이 아니라 Local 값을 수정한다. Local Move가 Lock이 걸린 경우엔 값이 적용되지 않는다.
		//------------------------------------------------------------------------------------------
		public void TransformChanged_Position__Modifier_Rigging(Vector2 pos)
		{
			if (Editor.Select.MeshGroup == null
				|| Editor.Select.Bone == null
				|| Editor._boneGUIRenderMode != apEditor.BONE_RENDER_MODE.Render
				|| Editor.Select.Modifier == null //<<Modifier가 null이면 안된다.
				)
			{
				return;
			}

			apBone bone = Editor.Select.Bone;
			apMeshGroup meshGroup = Editor.Select.MeshGroup;
			//apMeshGroup boneParentMeshGroup = bone._meshGroup;//Bone이 속한 MeshGroup. 다를 수 있다.
			apModifierBase modifier = Editor.Select.Modifier;//선택한 Modifier

			if (modifier.ModifierType != apModifierBase.MODIFIER_TYPE.Rigging)
			{
				//리깅 Modifier가 아니라면 패스
				return;
			}
			if (!Editor.Select.IsRigEditTestPosing)
			{
				//TestPosing이 허용되지 않았다면 패스
				return;
			}

			if(Editor.Gizmos.IsBrushMode || Editor.Select.RiggingBrush_Mode != apSelection.RIGGING_BRUSH_TOOL_MODE.None)
			{
				//브러시 모드에서는 무시
				return;
			}


			//직접 대입하고 끝
			bone._rigTestMatrix.SetPos(pos);
			//bone.MakeWorldMatrix(true);//<<이전 : 단일 본 변경
			//<BONE_EDIT>
			//if(bone._meshGroup != null)
			//{
			//	bone._meshGroup.UpdateBonesWorldMatrix();//변경 : 전체 본 갱신
			//}

			//>Root MeshGroup에서 변경
			if(meshGroup != null)
			{
				meshGroup.UpdateBonesWorldMatrix();
			}
			bone.GUIUpdate(true);
			
		}


		//------------------------------------------------------------------------------------------
		// Bone - TransformChanged (Default / Rigging Test / Modifier / AnimClip Modifier)
		// Rotate 값 직접 수정 (IK Range 확인)
		//------------------------------------------------------------------------------------------
		public void TransformChanged_Rotate__Modifier_Rigging(float angle)
		{
			if (Editor.Select.MeshGroup == null
				|| Editor.Select.Bone == null
				|| Editor._boneGUIRenderMode != apEditor.BONE_RENDER_MODE.Render
				|| Editor.Select.Modifier == null //<<Modifier가 null이면 안된다.
				)
			{
				return;
			}

			if (!Editor.Select.IsRigEditTestPosing)
			{
				//TestPosing이 허용되지 않았다면 패스
				return;
			}

			if(Editor.Gizmos.IsBrushMode || Editor.Select.RiggingBrush_Mode != apSelection.RIGGING_BRUSH_TOOL_MODE.None)
			{
				//브러시 모드에서는 무시
				return;
			}

			apBone bone = Editor.Select.Bone;
			apMeshGroup meshGroup = Editor.Select.MeshGroup;
			//apMeshGroup boneParentMeshGroup = bone._meshGroup;//Bone이 속한 MeshGroup. 다를 수 있다.
			apModifierBase modifier = Editor.Select.Modifier;//선택한 Modifier

			if (modifier.ModifierType != apModifierBase.MODIFIER_TYPE.Rigging)
			{
				//리깅 Modifier가 아니라면 패스
				return;
			}

			if (bone._isIKAngleRange)
			{
				if (angle < bone._defaultMatrix._angleDeg + bone._IKAngleRange_Lower)
				{
					angle = bone._defaultMatrix._angleDeg + bone._IKAngleRange_Lower;
				}
				else if (angle > bone._defaultMatrix._angleDeg + bone._IKAngleRange_Upper)
				{
					angle = bone._defaultMatrix._angleDeg + bone._IKAngleRange_Upper;
				}
			}

			//직접 대입한다.
			bone._rigTestMatrix.SetRotate(angle);
			//bone.MakeWorldMatrix(true);//<<이전
			
			//<BONE_EDIT>
			//if(bone._meshGroup != null)
			//{
			//	bone._meshGroup.UpdateBonesWorldMatrix();//<<변경 : 모든 본 동시에 갱신
			//}

			//>Root MeshGroup에서 변경
			if(meshGroup != null)
			{
				meshGroup.UpdateBonesWorldMatrix();
			}

			bone.GUIUpdate(true);

			
		}


		//------------------------------------------------------------------------------------------
		// Bone - TransformChanged (Default / Rigging Test / Modifier / AnimClip Modifier)
		// Scale 값 직접 수정 (Scale Lock 체크)
		//------------------------------------------------------------------------------------------
		public void TransformChanged_Scale__Modifier_Rigging(Vector2 scale)
		{
			if (Editor.Select.MeshGroup == null
				|| Editor.Select.Bone == null
				|| Editor._boneGUIRenderMode != apEditor.BONE_RENDER_MODE.Render
				|| Editor.Select.Modifier == null //<<Modifier가 null이면 안된다.
				)
			{
				return;
			}

			if (!Editor.Select.IsRigEditTestPosing)
			{
				//TestPosing이 허용되지 않았다면 패스
				return;
			}

			if(Editor.Gizmos.IsBrushMode || Editor.Select.RiggingBrush_Mode != apSelection.RIGGING_BRUSH_TOOL_MODE.None)
			{
				//브러시 모드에서는 무시
				return;
			}

			apBone bone = Editor.Select.Bone;
			apMeshGroup meshGroup = Editor.Select.MeshGroup;
			//apMeshGroup boneParentMeshGroup = bone._meshGroup;//Bone이 속한 MeshGroup. 다를 수 있다.
			apModifierBase modifier = Editor.Select.Modifier;//선택한 Modifier

			if (modifier.ModifierType != apModifierBase.MODIFIER_TYPE.Rigging)
			{
				//리깅 Modifier가 아니라면 패스
				return;
			}

			//직접 대입한다.
			bone._rigTestMatrix.SetScale(scale);
			//bone.MakeWorldMatrix(true);//<<이전
			//<BONE_EDIT>
			//if(bone._meshGroup != null)
			//{
			//	bone._meshGroup.UpdateBonesWorldMatrix();//<<변경 : 모든 본 동시에 갱신
			//}

			//>Root MeshGroup에서 변경
			if(meshGroup != null)
			{
				meshGroup.UpdateBonesWorldMatrix();
			}
			bone.GUIUpdate(true);
			
		}


		//------------------------------------------------------------------------------------------
		// Bone - Pivot Return (Default / Rigging Test / Modifier / AnimClip Modifier)
		//------------------------------------------------------------------------------------------
		public apGizmos.TransformParam PivotReturn__Modifier_Rigging()
		{
			if (Editor.Select.MeshGroup == null
				|| Editor.Select.Bone == null
				|| Editor._boneGUIRenderMode != apEditor.BONE_RENDER_MODE.Render
				|| Editor.Select.Modifier == null
				)
			{
				return null;
			}

			apBone bone = Editor.Select.Bone;
			apMeshGroup meshGroup = Editor.Select.MeshGroup;
			//apMeshGroup boneParentMeshGroup = bone._meshGroup;//Bone이 속한 MeshGroup. 다를 수 있다.
			apModifierBase modifier = Editor.Select.Modifier;//선택한 Modifier

			if (modifier.ModifierType != apModifierBase.MODIFIER_TYPE.Rigging)
			{
				//리깅 Modifier가 아니라면 패스
				return null;
			}
			if (!Editor.Select.IsRigEditTestPosing)
			{
				//TestPosing이 허용되지 않았다면 패스
				return null;
			}

			return apGizmos.TransformParam.Make(
					bone._worldMatrix._pos,
					bone._worldMatrix._angleDeg,
					bone._worldMatrix._scale,
					0, bone._color,
					true,
					bone._worldMatrix.MtrxToSpace,
					false, apGizmos.TRANSFORM_UI.TRS_NoDepth,
					bone._rigTestMatrix._pos,
					bone._rigTestMatrix._angleDeg,
					bone._rigTestMatrix._scale);

		}

		//------------------------------------------------------------------------------------------
		// Bone - Brush Mode
		//------------------------------------------------------------------------------------------
		public apGizmos.BrushInfo SyncBrushStatus__Modifier_Rigging(bool isEnded)
		{
			if (isEnded)
			{
				Editor.Select.ResetRiggingBrushMode();
			}
			else
			{
				apGizmos.BRUSH_COLOR_MODE colorMode = apGizmos.BRUSH_COLOR_MODE.Default;
				switch (Editor.Select.RiggingBrush_Mode)
				{
					case apSelection.RIGGING_BRUSH_TOOL_MODE.None:
						return null;
					case apSelection.RIGGING_BRUSH_TOOL_MODE.Add:
						{
							float intensityAdd = Editor.Select.RiggingBrush_Intensity_Add;
							if(Mathf.Approximately(intensityAdd, 0.0f))	{ colorMode = apGizmos.BRUSH_COLOR_MODE.Default; }
							else if(intensityAdd < -0.6f)				{ colorMode = apGizmos.BRUSH_COLOR_MODE.Decrease_Lv3; }
							else if(intensityAdd < -0.3f)				{ colorMode = apGizmos.BRUSH_COLOR_MODE.Decrease_Lv2; }
							else if(intensityAdd < 0.0f)				{ colorMode = apGizmos.BRUSH_COLOR_MODE.Decrease_Lv1; }
							else if(intensityAdd < 0.3f)				{ colorMode = apGizmos.BRUSH_COLOR_MODE.Increase_Lv1; }
							else if(intensityAdd < 0.6f)				{ colorMode = apGizmos.BRUSH_COLOR_MODE.Increase_Lv2; }
							else										{ colorMode = apGizmos.BRUSH_COLOR_MODE.Increase_Lv3; }

							return apGizmos.BrushInfo.MakeInfo(Editor.Select.RiggingBrush_Radius, intensityAdd, colorMode, Editor.ImageSet.Get(apImageSet.PRESET.Rig_BrushAdd));
						}
					case apSelection.RIGGING_BRUSH_TOOL_MODE.Multiply:
						{
							float intensityMul = Editor.Select.RiggingBrush_Intensity_Multiply - 1.0f;
							if(Mathf.Approximately(intensityMul, 0.0f))	{ colorMode = apGizmos.BRUSH_COLOR_MODE.Default; }
							else if(intensityMul < -0.4f)				{ colorMode = apGizmos.BRUSH_COLOR_MODE.Decrease_Lv3; }
							else if(intensityMul < -0.2f)				{ colorMode = apGizmos.BRUSH_COLOR_MODE.Decrease_Lv2; }
							else if(intensityMul < 0.0f)				{ colorMode = apGizmos.BRUSH_COLOR_MODE.Decrease_Lv1; }
							else if(intensityMul < 0.2f)				{ colorMode = apGizmos.BRUSH_COLOR_MODE.Increase_Lv1; }
							else if(intensityMul < 0.4f)				{ colorMode = apGizmos.BRUSH_COLOR_MODE.Increase_Lv2; }
							else										{ colorMode = apGizmos.BRUSH_COLOR_MODE.Increase_Lv3; }

							return apGizmos.BrushInfo.MakeInfo(Editor.Select.RiggingBrush_Radius, intensityMul, colorMode, Editor.ImageSet.Get(apImageSet.PRESET.Rig_BrushMultiply));
						}
					case apSelection.RIGGING_BRUSH_TOOL_MODE.Blur:
						{
							int intensityBlur100 = (int)(Editor.Select.RiggingBrush_Intensity_Blur);
							if(intensityBlur100 == 0)		{ colorMode = apGizmos.BRUSH_COLOR_MODE.Default; }
							else if(intensityBlur100 < 33)	{ colorMode = apGizmos.BRUSH_COLOR_MODE.Increase_Lv1; }
							else if(intensityBlur100 < 66)	{ colorMode = apGizmos.BRUSH_COLOR_MODE.Increase_Lv2; }
							else							{ colorMode = apGizmos.BRUSH_COLOR_MODE.Increase_Lv3; }

							return apGizmos.BrushInfo.MakeInfo(Editor.Select.RiggingBrush_Radius, Mathf.Clamp01((float)Editor.Select.RiggingBrush_Intensity_Blur / 100.0f), colorMode, Editor.ImageSet.Get(apImageSet.PRESET.Rig_BrushBlur));
						}
				}

			}
			return null;
		}

		private List<apModifiedVertexRig> _tmpRigBrushModVerts = new List<apModifiedVertexRig>();
		private List<apModifiedVertexRig.WeightPair> _tmpRigBrushTargetBoneWeightPair = new List<apModifiedVertexRig.WeightPair>();
		private List<float> _tmpRigBrushVertWeights = new List<float>();

		public bool PressBrush__Modifier_Rigging(Vector2 pos, float tDelta, bool isFirstBrush)
		{
			if (Editor.Select.MeshGroup == null 
				|| Editor.Select.Modifier == null
				|| Editor.Select.Bone == null
				|| Editor.Select.ExKey_ModMesh == null)
			{
				return false;
			}

			if (Editor.Select.ModRenderVertListOfMod == null
				|| Editor.Select.ModRenderVertListOfMod.Count == 0)
			{
				//선택된 버텍스가 없어도 실패
				return false;
			}
			if(Editor.Select.RiggingBrush_Mode == apSelection.RIGGING_BRUSH_TOOL_MODE.None)
			{
				return false;
			}
			apRenderUnit targetRenderUnit = Editor.Select.MeshGroup.GetRenderUnit(Editor.Select.ExKey_ModMesh._transform_Mesh);
			if(targetRenderUnit == null)
			{
				return false;
			}
			
			float radius = Editor.Gizmos.BrushRadiusGL;
			float intensity = Editor.Gizmos.BrushIntensity * tDelta;
			
			apSelection.RIGGING_BRUSH_TOOL_MODE brushMode = Editor.Select.RiggingBrush_Mode;

			if (radius <= 0.0f)
			{
				return false;
			}

			apBone targetBone = Editor.Select.Bone;
			
			_tmpRigBrushModVerts.Clear();
			_tmpRigBrushTargetBoneWeightPair.Clear();
			_tmpRigBrushVertWeights.Clear();

			float dist = 0.0f;
			float weight = 0.0f;
			float totalWeight = 0.0f;
			float totalRigWeight = 0.0f;//<<Blur인 경우, ModRigVert와 Bone의 연결 정보를 받아서 모두 저장하자.

			apModifiedVertexRig.WeightPair curWeightPair = null;
				
			int nBrushVert = 0;
			for (int i = 0; i < Editor.Select.ModRenderVertListOfMod.Count; i++)
			{
				apSelection.ModRenderVert modRenderVert = Editor.Select.ModRenderVertListOfMod[i];
				dist = Vector2.Distance(modRenderVert._renderVert._pos_GL, pos);
				if (dist > radius)
				{
					continue;
				}

				

				apModifiedVertexRig selectedModVertRig = Editor.Select.ExKey_ModMesh._vertRigs.Find(delegate (apModifiedVertexRig a)
				{
					return modRenderVert._renderVert._vertex._uniqueID == a._vertexUniqueID;
				});

				if(selectedModVertRig == null)
				{
					continue;
				}

				weight = (radius - dist) / radius;
				totalWeight += weight;
				nBrushVert++;

				//Debug.Log("Rig VertPos : " + modRenderVert._renderVert._pos_GL + " / MousePos : " + pos + " / Dist : " + dist + " / Radius : " + radius + " >> Weight : " + weight);

				_tmpRigBrushModVerts.Add(selectedModVertRig);
				_tmpRigBrushVertWeights.Add(weight);

				//본 리깅 정보를 찾거나 만들어서 리스트로 저장하자. 직접 Weight 값을 대입하기 위함
				curWeightPair = selectedModVertRig._weightPairs.Find(delegate(apModifiedVertexRig.WeightPair a)
				{
					return a._bone == targetBone;
				});

				if(curWeightPair == null)
				{
					//없으면 만들자
					curWeightPair = new apModifiedVertexRig.WeightPair(targetBone);
					curWeightPair._weight = 0.0f;
					selectedModVertRig._weightPairs.Add(curWeightPair);
				}

				_tmpRigBrushTargetBoneWeightPair.Add(curWeightPair);

				//Blur인 경우, 선택된 Bone에 대한 RigWeight의 총합(가중치 포함)을 구한다.
				totalRigWeight += curWeightPair._weight * weight;
			}

			
			if(_tmpRigBrushModVerts.Count == 0 || totalWeight <= 0.0f)
			{
				return false;
			}

			//일단 SetRecord
			if(isFirstBrush)
			{
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_RiggingWeightChanged, Editor, Editor.Select.Modifier, targetRenderUnit, false);
			}



			//버텍스를 선택했으니, 이제 연산을 하자

			apModifiedVertexRig curModVertRig = null;
			float curIntensity = 0.0f;
			float weightLerp = 0.0f;
			bool isAutoNormalize = Editor.Select._rigEdit_isAutoNormalize;

			switch (brushMode)
			{
				case apSelection.RIGGING_BRUSH_TOOL_MODE.Add:
					{
						for (int i = 0; i < _tmpRigBrushModVerts.Count; i++)
						{
							curModVertRig = _tmpRigBrushModVerts[i];
							curIntensity = intensity * _tmpRigBrushVertWeights[i];
							curWeightPair = _tmpRigBrushTargetBoneWeightPair[i];

							//더하기
							curWeightPair._weight += curIntensity;

							//Weight 다시 계산
							curModVertRig.CalculateTotalWeight();

							//Normalize
							if(isAutoNormalize)
							{
								curModVertRig.NormalizeExceptPair(curWeightPair, true);
							}
						}
					}
					break;

				case apSelection.RIGGING_BRUSH_TOOL_MODE.Multiply:
					{
						intensity += 1.0f;//<<인자로 넣을때 1일 뺐기 때문에 복원

						
						for (int i = 0; i < _tmpRigBrushModVerts.Count; i++)
						{
							curModVertRig = _tmpRigBrushModVerts[i];
							weightLerp = Mathf.Clamp01(_tmpRigBrushVertWeights[i]);
							curIntensity = (intensity * weightLerp) + (1.0f - weightLerp);
							curWeightPair = _tmpRigBrushTargetBoneWeightPair[i];

							//곱하기
							curWeightPair._weight *= curIntensity;

							//Weight 다시 계산
							curModVertRig.CalculateTotalWeight();

							//Normalize
							if(isAutoNormalize)
							{
								curModVertRig.NormalizeExceptPair(curWeightPair, true);
							}
						}
					}
					break;

				case apSelection.RIGGING_BRUSH_TOOL_MODE.Blur:
					{
						//Blur인 경우에는 두차례씩 계산한다.
						float avgRigWeight = totalRigWeight / totalWeight;

						for (int i = 0; i < _tmpRigBrushModVerts.Count; i++)
						{
							curModVertRig = _tmpRigBrushModVerts[i];
							curIntensity = intensity * _tmpRigBrushVertWeights[i];
							curWeightPair = _tmpRigBrushTargetBoneWeightPair[i];

							//보간값 적용
							float itp = Mathf.Clamp01(curIntensity * 5.0f);
							curWeightPair._weight = curWeightPair._weight * (1.0f - itp) + avgRigWeight * itp;

							//Weight 다시 계산
							curModVertRig.CalculateTotalWeight();

							//Normalize
							if(isAutoNormalize)
							{
								curModVertRig.NormalizeExceptPair(curWeightPair, true);
							}
						}
					}
					break;
			}

			Editor.Select.MeshGroup.RefreshForce();
			Editor.SetRepaint();
			return true;
		}
	}
}