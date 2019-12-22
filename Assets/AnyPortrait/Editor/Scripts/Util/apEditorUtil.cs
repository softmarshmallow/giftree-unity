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
using UnityEditor.SceneManagement;
using System.Collections;
using System;
using System.Collections.Generic;


using AnyPortrait;

namespace AnyPortrait
{

	public static class apEditorUtil
	{


		//----------------------------------------------------------------------------------------------------------
		// GUI Delimeter
		//----------------------------------------------------------------------------------------------------------
		public static void GUI_DelimeterBoxV(int height)
		{
			Color prevColor = GUI.backgroundColor;

			if (EditorGUIUtility.isProSkin)	{ GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f); }
			else							{ GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 1.0f); }
			
			GUILayout.Box("", GUILayout.Width(4), GUILayout.Height(height));
			GUI.backgroundColor = prevColor;
		}

		public static void GUI_DelimeterBoxH(int width)
		{
			Color prevColor = GUI.backgroundColor;

			if (EditorGUIUtility.isProSkin)	{ GUI.backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f); }
			else							{ GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 1.0f); }

			GUILayout.Box("", GUILayout.Width(width), GUILayout.Height(4));
			GUI.backgroundColor = prevColor;
		}


		//----------------------------------------------------------------------------------------------------------
		// 색상 공간
		//----------------------------------------------------------------------------------------------------------
		public static ColorSpace GetColorSpace()
		{
			return QualitySettings.activeColorSpace;
		}

		public static bool IsGammaColorSpace()
		{
			return QualitySettings.activeColorSpace != ColorSpace.Linear;
		}

		//----------------------------------------------------------------------------------------------------------
		// Vector
		//----------------------------------------------------------------------------------------------------------
		private static Vector2 _infVector2 = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
		public static Vector2 InfVector2
		{
			get
			{
				return _infVector2;
			}
		}
		//----------------------------------------------------------------------------------------------------------
		// Set Record 계열 함수
		//----------------------------------------------------------------------------------------------------------

		//private static int _lastUndoID = -1;
		
		private static void SetRecordMeshGroupRecursive(apUndoGroupData.ACTION action, apMeshGroup meshGroup, apMeshGroup rootGroup)
		{
			if(meshGroup == null)
			{
				return;
			}
			if (meshGroup != rootGroup)
			{
				Undo.RegisterCompleteObjectUndo(meshGroup, apUndoGroupData.GetLabel(action));
			}

			for (int i = 0; i < meshGroup._childMeshGroupTransforms.Count; i++)
			{
				apMeshGroup childMeshGroup = meshGroup._childMeshGroupTransforms[i]._meshGroup;
				if(childMeshGroup == meshGroup || childMeshGroup == rootGroup)
				{
					continue;
				}

				SetRecordMeshGroupRecursive(action, childMeshGroup, rootGroup);

			}

			//Prefab Apply
			SetPortraitPrefabApply(meshGroup._parentPortrait);
		}


		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_Portrait(apUndoGroupData.ACTION action,
									apEditor editor,
									apPortrait portrait,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, portrait, null, null, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Portrait);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//MonoObject별로 다르게 Undo를 등록하자
			//Undo.RecordObject(portrait, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(portrait, apUndoGroupData.GetLabel(action));

			
			//Undo.FlushUndoRecordObjects();

			//Prefab Apply
			SetPortraitPrefabApply(portrait);
			
		}



		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_PortraitMeshGroup(	apUndoGroupData.ACTION action,
														apEditor editor,
														apPortrait portrait,
														apMeshGroup meshGroup,
														object keyObject,
														bool isCallContinuous,
														bool isChildRecursive)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, portrait, null, null, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Portrait | apUndoGroupData.SAVE_TARGET.AllMeshGroups);


			
			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//MonoObject별로 다르게 Undo를 등록하자
			//Undo.RecordObject(portrait, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(portrait, apUndoGroupData.GetLabel(action));

			if(meshGroup == null)
			{
				return;
			}

			//Undo.RecordObject(meshGroup, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(meshGroup, apUndoGroupData.GetLabel(action));

			if(isChildRecursive)
			{
				SetRecordMeshGroupRecursive(action, meshGroup, meshGroup);
			}


			//Undo.FlushUndoRecordObjects();
			
			//Prefab Apply
			SetPortraitPrefabApply(portrait);
		}



		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_PortraitAllMeshGroup(apUndoGroupData.ACTION action,
									apEditor editor,
									apPortrait portrait,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, portrait, null, null, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Portrait | apUndoGroupData.SAVE_TARGET.AllMeshGroups);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//MonoObject별로 다르게 Undo를 등록하자
			//Undo.RecordObject(portrait, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(portrait, apUndoGroupData.GetLabel(action));

			//모든 MeshGroup을 Undo에 넣자
			for (int i = 0; i < portrait._meshGroups.Count; i++)
			{
				//Undo.RecordObject(portrait._meshGroups[i], apUndoGroupData.GetLabel(action));
				Undo.RegisterCompleteObjectUndo(portrait._meshGroups[i], apUndoGroupData.GetLabel(action));
			}
			//Undo.FlushUndoRecordObjects();

			//Prefab Apply
			SetPortraitPrefabApply(portrait);
		}


		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION action,
									apEditor editor,
									apPortrait portrait,
									apMeshGroup meshGroup,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, portrait, null, meshGroup, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Portrait | apUndoGroupData.SAVE_TARGET.MeshGroup | apUndoGroupData.SAVE_TARGET.AllModifiers);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//MonoObject별로 다르게 Undo를 등록하자
			//Undo.RecordObject(portrait, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(portrait, apUndoGroupData.GetLabel(action));

			if(meshGroup == null)
			{
				return;
			}
			//Undo.RecordObject(meshGroup, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(meshGroup, apUndoGroupData.GetLabel(action));

			for (int iMod = 0; iMod < meshGroup._modifierStack._modifiers.Count; iMod++)
			{
				//Undo.RecordObject(meshGroup._modifierStack._modifiers[iMod], apUndoGroupData.GetLabel(action));
				Undo.RegisterCompleteObjectUndo(meshGroup._modifierStack._modifiers[iMod], apUndoGroupData.GetLabel(action));
			}
			//Undo.FlushUndoRecordObjects();

			//Prefab Apply
			SetPortraitPrefabApply(portrait);
		}



		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_PortraitMeshGroupModifier(apUndoGroupData.ACTION action,
									apEditor editor,
									apPortrait portrait,
									apMeshGroup meshGroup,
									apModifierBase modifier,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, portrait, null, meshGroup, modifier, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Portrait | apUndoGroupData.SAVE_TARGET.MeshGroup | apUndoGroupData.SAVE_TARGET.AllModifiers);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//MonoObject별로 다르게 Undo를 등록하자
			//Undo.RecordObject(portrait, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(portrait, apUndoGroupData.GetLabel(action));

			if(meshGroup == null)
			{
				return;
			}

			//Undo.RecordObject(meshGroup, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(meshGroup, apUndoGroupData.GetLabel(action));

			if (modifier == null)
			{
				return;
			}

			//Undo.RecordObject(modifier, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(modifier, apUndoGroupData.GetLabel(action));
			
			//Undo.FlushUndoRecordObjects();

			//Prefab Apply
			SetPortraitPrefabApply(portrait);
		}



		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_PortraitModifier(apUndoGroupData.ACTION action,
									apEditor editor,
									apPortrait portrait,
									apModifierBase modifier,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, portrait, null, null, modifier, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Portrait | apUndoGroupData.SAVE_TARGET.MeshGroup | apUndoGroupData.SAVE_TARGET.AllModifiers);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//MonoObject별로 다르게 Undo를 등록하자
			//Undo.RecordObject(portrait, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(portrait, apUndoGroupData.GetLabel(action));

			
			if (modifier == null)
			{
				return;
			}

			//Undo.RecordObject(modifier, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(modifier, apUndoGroupData.GetLabel(action));
			
			//Undo.FlushUndoRecordObjects();
			
			//Prefab Apply
			SetPortraitPrefabApply(portrait);
		}


		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_PortraitAllMeshGroupAndAllModifiers(apUndoGroupData.ACTION action,
									apEditor editor,
									apPortrait portrait,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, portrait, null, null, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Portrait | apUndoGroupData.SAVE_TARGET.AllMeshGroups | apUndoGroupData.SAVE_TARGET.AllModifiers);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//MonoObject별로 다르게 Undo를 등록하자
			//Undo.RecordObject(portrait, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(portrait, apUndoGroupData.GetLabel(action));

			//모든 MeshGroup을 Undo에 넣자
			for (int i = 0; i < portrait._meshGroups.Count; i++)
			{
				//MonoObject별로 다르게 Undo를 등록하자
				//Undo.RecordObject(portrait._meshGroups[i], apUndoGroupData.GetLabel(action));
				Undo.RegisterCompleteObjectUndo(portrait._meshGroups[i], apUndoGroupData.GetLabel(action));

				for (int iMod = 0; iMod < portrait._meshGroups[i]._modifierStack._modifiers.Count; iMod++)
				{
					//Undo.RecordObject(portrait._meshGroups[i]._modifierStack._modifiers[iMod], apUndoGroupData.GetLabel(action));
					Undo.RegisterCompleteObjectUndo(portrait._meshGroups[i]._modifierStack._modifiers[iMod], apUndoGroupData.GetLabel(action));
					
				}
			}

			//Undo.FlushUndoRecordObjects();

			//Prefab Apply
			SetPortraitPrefabApply(portrait);
		}


		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_Mesh(apUndoGroupData.ACTION action,
									apEditor editor,
									apMesh mesh,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, null, mesh, null, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Mesh);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//Undo.RecordObject(mesh, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(mesh, apUndoGroupData.GetLabel(action));

			//Undo.FlushUndoRecordObjects();

			//Prefab Apply
			SetPortraitPrefabApply(editor._portrait);
		}


		


		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_MeshAndMeshGroups(apUndoGroupData.ACTION action,
									apEditor editor,
									apMesh mesh,
									List<apMeshGroup> meshGroups,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, null, mesh, null, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.Mesh | apUndoGroupData.SAVE_TARGET.AllMeshGroups);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//Undo.RecordObject(mesh, apUndoGroupData.GetLabel(action));
			List<UnityEngine.Object> recordObjects = new List<UnityEngine.Object>();
			recordObjects.Add(mesh);
			//Undo.RegisterCompleteObjectUndo(mesh, apUndoGroupData.GetLabel(action));

			if (meshGroups != null && meshGroups.Count > 0)
			{
				for (int i = 0; i < meshGroups.Count; i++)
				{
					//Undo.RegisterCompleteObjectUndo(meshGroups[i], apUndoGroupData.GetLabel(action));
					recordObjects.Add(meshGroups[i]);
				}
			}

			Undo.RegisterCompleteObjectUndo(recordObjects.ToArray(), apUndoGroupData.GetLabel(action));

			//Undo.FlushUndoRecordObjects();

			//Prefab Apply
			SetPortraitPrefabApply(editor._portrait);
		}

		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_MeshGroup(apUndoGroupData.ACTION action,
									apEditor editor,
									apMeshGroup meshGroup,
									object keyObject,
									bool isCallContinuous,
									bool isChildRecursive)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, null, null, meshGroup, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.MeshGroup);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//Undo.RecordObject(meshGroup, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(meshGroup, apUndoGroupData.GetLabel(action));

			if(isChildRecursive)
			{
				SetRecordMeshGroupRecursive(action, meshGroup, meshGroup);
			}
			
			//Undo.FlushUndoRecordObjects();

			//Prefab Apply
			SetPortraitPrefabApply(editor._portrait);
		}


		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_MeshGroupAndModifier(apUndoGroupData.ACTION action,
									apEditor editor,
									apMeshGroup meshGroup,
									apModifierBase modifier,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, null, null, meshGroup, modifier, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.MeshGroup | apUndoGroupData.SAVE_TARGET.Modifier);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//Undo.RecordObject(meshGroup, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(meshGroup, apUndoGroupData.GetLabel(action));

			if(modifier == null)
			{
				return;
			}
			//Undo.RecordObject(modifier, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(modifier, apUndoGroupData.GetLabel(action));
			
			//Undo.FlushUndoRecordObjects();

			//Prefab Apply
			SetPortraitPrefabApply(editor._portrait);
		}



		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_MeshGroupAllModifiers(apUndoGroupData.ACTION action,
									apEditor editor,
									apMeshGroup meshGroup,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, null, null, meshGroup, null, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.MeshGroup | apUndoGroupData.SAVE_TARGET.AllModifiers);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//Undo.RecordObject(meshGroup, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(meshGroup, apUndoGroupData.GetLabel(action));

			for (int i = 0; i < meshGroup._modifierStack._modifiers.Count; i++)
			{
				//Undo.RecordObject(meshGroup._modifierStack._modifiers[i], apUndoGroupData.GetLabel(action));
				Undo.RegisterCompleteObjectUndo(meshGroup._modifierStack._modifiers[i], apUndoGroupData.GetLabel(action));
				
			}

			//Undo.FlushUndoRecordObjects();

			//Prefab Apply
			SetPortraitPrefabApply(editor._portrait);
		}



		/// <summary>
		/// Undo를 위해 Action을 저장한다.
		/// Label과 기록되는 값을 통해서 중복 여부를 체크한다.
		/// </summary>
		public static void SetRecord_Modifier(apUndoGroupData.ACTION action,
									apEditor editor,
									apModifierBase modifier,
									object keyObject,
									bool isCallContinuous)
		{
			if (editor._portrait == null) { return; }

			//연속된 기록이면 Undo/Redo시 한번에 묶어서 실행되어야 한다. (예: 버텍스의 실시간 이동 기록)
			//이전에 요청되었던 기록이면 Undo ID를 유지해야한다.
			bool isNewAction = apUndoGroupData.I.SetAction(action, null, null, null, modifier, keyObject, isCallContinuous, apUndoGroupData.SAVE_TARGET.MeshGroup | apUndoGroupData.SAVE_TARGET.Modifier);


			EditorSceneManager.MarkAllScenesDirty();

			//새로운 변동 사항이라면 UndoID 증가
			if (isNewAction)
			{
				Undo.IncrementCurrentGroup();
				//_lastUndoID = Undo.GetCurrentGroup();
			}

			//Undo.RecordObject(modifier, apUndoGroupData.GetLabel(action));
			Undo.RegisterCompleteObjectUndo(modifier, apUndoGroupData.GetLabel(action));
			

			//Undo.FlushUndoRecordObjects();

			//Prefab Apply
			SetPortraitPrefabApply(editor._portrait);
		}


		public static void SetRecordBeforeCreateOrDestroyObject(apPortrait portrait, string label)
		{
			EditorSceneManager.MarkAllScenesDirty();
			Undo.IncrementCurrentGroup();

			//Portrait, Mesh, MeshGroup, Modifier를 저장하자
			Undo.RegisterCompleteObjectUndo(portrait, label);
			//Mesh와 MeshGroup 상태 저장
			for (int i = 0; i < portrait._meshes.Count; i++)
			{
				Undo.RegisterCompleteObjectUndo(portrait._meshes[i], label);
			}

			for (int i = 0; i < portrait._meshGroups.Count; i++)
			{
				//MonoObject별로 다르게 Undo를 등록하자
				Undo.RegisterCompleteObjectUndo(portrait._meshGroups[i], label);

				for (int iMod = 0; iMod < portrait._meshGroups[i]._modifierStack._modifiers.Count; iMod++)
				{
					Undo.RegisterCompleteObjectUndo(portrait._meshGroups[i]._modifierStack._modifiers[iMod], label);
				}
			}

			//Prefab Apply
			SetPortraitPrefabApply(portrait);
		}


		/// <summary>
		/// Monobehaviour 객체가 생성되니 Undo로 기록할 때 호출하는 함수
		/// </summary>
		/// <param name="createdMonoObject"></param>
		/// <param name="label"></param>
		public static void SetRecordCreateMonoObject(MonoBehaviour createdMonoObject, string label)
		{
			if (createdMonoObject == null)
			{
				return;
			}
			
			Undo.RegisterCreatedObjectUndo(createdMonoObject.gameObject, label);

			
			//Undo.FlushUndoRecordObjects();

			
		}



		public static void SetRecordDestroyMonoObject(MonoBehaviour destroyableMonoObject, string label)
		{
			if(destroyableMonoObject == null)
			{
				return;
			}
			
			Undo.DestroyObjectImmediate(destroyableMonoObject.gameObject);

			//Undo.FlushUndoRecordObjects();

			
		}


		public static void SetRecordDestroyMonoObjects(List<MonoBehaviour> destroyableMonoObjects, string label)
		{
			if(destroyableMonoObjects == null || destroyableMonoObjects.Count == 0)
			{
				return;
			}

			for (int i = 0; i < destroyableMonoObjects.Count; i++)
			{
				Undo.DestroyObjectImmediate(destroyableMonoObjects[i].gameObject);
			}
			

			//Undo.FlushUndoRecordObjects();
		}




		public static void SetEditorDirty()
		{
			EditorSceneManager.MarkAllScenesDirty();
		}

		/// <summary>
		/// Undo는 "같은 메뉴"에서만 가능하다. 메뉴를 전환할 때에는 Undo를 
		/// </summary>
		public static void ResetUndo(apEditor editor)
		{
			//apUndoManager.I.Clear();
			if (editor._portrait != null)
			{
				//Undo.ClearUndo(editor._portrait);//이건 일단 빼보자
				apUndoGroupData.I.Clear();
			}
		}


		public static void OnUndoRedoPerformed()
		{
			apUndoGroupData.I.Clear();
		}
		
		//----------------------------------------------------------------------------------------------------------
		// Prefab Check
		//----------------------------------------------------------------------------------------------------------
		public static void DisconnectPrefab(apPortrait portrait)
		{
			if (portrait == null || portrait.gameObject == null)
			{
				return;
			}

			//<< 유니티 2018.3 관련 API 분기 >>
#if UNITY_2018_3_OR_NEWER
			//Unity 2018.3 전용
			PrefabInstanceStatus prefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(portrait.gameObject);
			if(prefabInstanceStatus == PrefabInstanceStatus.Disconnected)
			{
				//이미 끊어졌다.
				Debug.LogError("Arleady Disconnected");
				return;
			}
#else
			PrefabType prefabType = PrefabUtility.GetPrefabType(portrait.gameObject);
			if(prefabType == PrefabType.DisconnectedPrefabInstance)
			{
				//이미 끊어졌다.
				Debug.LogError("Arleady Disconnected");
				return;
			}
#endif

			//<< 유니티 2018.3 관련 API 분기 >>
#if UNITY_2018_3_OR_NEWER
			GameObject rootGameObj = PrefabUtility.GetOutermostPrefabInstanceRoot(portrait.gameObject);
#else
			GameObject rootGameObj = PrefabUtility.FindRootGameObjectWithSameParentPrefab(portrait.gameObject);
#endif
			if (rootGameObj == null)
			{
				//Debug.LogError("루트 프리팹 GameObject가 없습니다.");
				return;
			}
			//Debug.Log("루트 프리팹 GameObject : " + rootGameObj.name);

#if UNITY_2018_1_OR_NEWER
			UnityEngine.Object prefabObj = PrefabUtility.GetCorrespondingObjectFromSource(rootGameObj);
#else
			UnityEngine.Object prefabObj = PrefabUtility.GetPrefabParent(rootGameObj);
#endif

			if(prefabObj == null)
			{
				//Debug.LogError("연결된 프리팹이 없습니다.");
				return;
			}

			//<< 유니티 2018.3 관련 API 분기 >>
#if UNITY_2018_3_OR_NEWER
			PrefabUtility.UnpackPrefabInstance(rootGameObj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);//차이가 있다. Disconnect가 아니라 정말 끊긴다. 그래서 다시 Apply가 안된다...
#else
			PrefabUtility.DisconnectPrefabInstance(rootGameObj);
#endif
			EditorSceneManager.MarkAllScenesDirty();

		}
		/// <summary>
		/// Set Record를 하면서 Prefab인 경우 Apply를 자동으로 한다.
		/// </summary>
		/// <param name="portrait"></param>
		public static void SetPortraitPrefabApply(apPortrait portrait)
		{
			return;
			//if(portrait == null || portrait.gameObject == null)
			//{
			//	return;
			//}
			//if(!IsPrefab(portrait.gameObject))
			//{
			//	return;
			//}
			////ApplyPrefab(portrait.gameObject);

			//GameObject rootGameObj = PrefabUtility.FindRootGameObjectWithSameParentPrefab(portrait.gameObject);
			//if (rootGameObj == null)
			//{
			//	//Debug.LogError("루트 프리팹 GameObject가 없습니다.");
			//	return;
			//}
			////Debug.Log("루트 프리팹 GameObject : " + rootGameObj.name);

			//UnityEngine.Object prefabObj = PrefabUtility.GetPrefabParent(rootGameObj);

			//if(prefabObj == null)
			//{
			//	//Debug.LogError("연결된 프리팹이 없습니다.");
			//	return;
			//}

			//PrefabUtility.RecordPrefabInstancePropertyModifications(rootGameObj);
			//EditorSceneManager.MarkAllScenesDirty();
		}

		public static bool IsPrefab(GameObject gameObject)
		{
			
			//<< 유니티 2018.3 관련 API 분기 >>
#if UNITY_2018_3_OR_NEWER
			//Unity 2018.3 전용
			PrefabInstanceStatus prefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(gameObject);
			if(prefabInstanceStatus != PrefabInstanceStatus.Connected)
			{
				//프리팹이 아니다.
				return false;
			}

#else

			PrefabType prefabType = PrefabUtility.GetPrefabType(gameObject);
			
			if(prefabType != PrefabType.PrefabInstance)
			{
				//Debug.LogError("프리팹이 아닙니다. : " + prefabType);
				return false;
			}
#endif
			return true;
		}

		public static void ApplyPrefab(GameObject gameObject, bool isReplaceNameBased = false)
		{
			
			//<< 유니티 2018.3 관련 API 분기 >>
#if UNITY_2018_3_OR_NEWER
			GameObject rootGameObj = PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject);
#else
			GameObject rootGameObj = PrefabUtility.FindRootGameObjectWithSameParentPrefab(gameObject);
#endif
			if (rootGameObj == null)
			{
				//Debug.LogError("루트 프리팹 GameObject가 없습니다.");
				return;
			}
			//Debug.Log("루트 프리팹 GameObject : " + rootGameObj.name);

			//UnityEngine.Object prefabObj = PrefabUtility.GetPrefabParent(rootGameObj);
#if UNITY_2018_1_OR_NEWER
			UnityEngine.Object prefabObj = PrefabUtility.GetCorrespondingObjectFromSource(rootGameObj);
#else
			UnityEngine.Object prefabObj = PrefabUtility.GetPrefabParent(rootGameObj);
#endif

			if(prefabObj == null)
			{
				//Debug.LogError("연결된 프리팹이 없습니다.");
				return;
			}
			//Debug.Log("연결된 프리팹 : " + prefabObj.name);

			if (isReplaceNameBased)
			{
				//<< 유니티 2018.3 관련 API 분기 >>
#if UNITY_2018_3_OR_NEWER
				string strPrefabPath = AssetDatabase.GetAssetPath(prefabObj);
				PrefabUtility.SaveAsPrefabAssetAndConnect(rootGameObj, strPrefabPath, InteractionMode.AutomatedAction);
#else
				PrefabUtility.ReplacePrefab(rootGameObj, prefabObj, ReplacePrefabOptions.ConnectToPrefab | ReplacePrefabOptions.ReplaceNameBased);
#endif
			}
			else
			{
#if UNITY_2018_3_OR_NEWER
				string strPrefabPath = AssetDatabase.GetAssetPath(prefabObj);
				PrefabUtility.SaveAsPrefabAssetAndConnect(rootGameObj, strPrefabPath, InteractionMode.AutomatedAction);
#else
				PrefabUtility.ReplacePrefab(rootGameObj, prefabObj, ReplacePrefabOptions.ConnectToPrefab);
#endif
			}

		}


		//----------------------------------------------------------------------------------------------------------
		// GUI : Toggle Button
		//----------------------------------------------------------------------------------------------------------


		public static void ReleaseGUIFocus()
		{
			GUI.FocusControl(null);
		}

		public static Color BoxTextColor
		{
			get
			{
				if (EditorGUIUtility.isProSkin)
				{
					//return Color.white;
					return GUI.skin.label.normal.textColor;
				}
				else
				{
					return GUI.skin.box.normal.textColor;
				}
			}
		}
		
		public static Color ToggleBoxColor_Selected
		{
			get
			{
				if (EditorGUIUtility.isProSkin)
				{
					return new Color(0.0f, 1.0f, 1.0f, 1.0f);
				}
				else
				{
					return new Color(0.0f, 0.2f, 0.3f, 1.0f);
				}
			}
		}

		public static Color ToggleBoxColor_SelectedWithImage
		{
			get
			{
				if (EditorGUIUtility.isProSkin)
				{
					return new Color(0.0f, 1.0f, 1.0f, 1.0f);
				}
				else
				{
					//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					return new Color(0.3f, 1.0f, 1.0f, 1.0f);
				}
			}
		}



		public static Color ToggleBoxColor_NotAvailable
		{
			get
			{

				if (EditorGUIUtility.isProSkin)
				{
					return new Color(0.1f, 0.1f, 0.1f, 1.0f);
				}
				else
				{
					return new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}
			}
		}

		private static apGUIContentWrapper _sGUIContentWrapper = new apGUIContentWrapper(false);


		public static bool ToggledButton(string strText, bool isSelected, int width)
		{
			return ToggledButton(strText, isSelected, width, 20);
		}

		public static bool ToggledButton(string strText, bool isSelected, int width, int height)
		{
			if (isSelected)
			{
				Color prevColor = GUI.backgroundColor;
				GUI.backgroundColor = ToggleBoxColor_Selected;
				
				//이전
				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = Color.white;
				//guiStyle.alignment = TextAnchor.MiddleCenter;
				//guiStyle.margin = GUI.skin.button.margin;

				//if(EditorGUIUtility.isProSkin)
				//{
				//	//밝은 파랑 + 하늘색
				//	guiStyle.normal.textColor = Color.cyan;
				//}
				//else
				//{
				//	//짙은 남색
				//	//GUI.backgroundColor = new Color(0.0f, 0.2f, 0.3f, 1.0f);
				//}

				//GUILayout.Box(strText, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				
				//변경
				GUILayout.Box(	strText, 
								apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Cyan,//<<이건 ProUI일때와 기본의 색이 다르다.
								GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				return GUILayout.Button(strText, GUILayout.Width(width), GUILayout.Height(height));
			}
		}

		public static bool ToggledButton(string strText, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					//if(EditorGUIUtility.isProSkin)
					//{
					//	textColor = Color.black;
					//}
					//else
					//{
					//	textColor = Color.white;
					//}
					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					//if(EditorGUIUtility.isProSkin)
					//{
					//	//밝은 파랑 + 하늘색
					//	textColor = Color.cyan;
					//}
					//else
					//{
					//	//짙은 남색 + 흰색
					//	textColor = Color.white;
					//}

					GUI.backgroundColor = ToggleBoxColor_Selected;
					
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = textColor;
				//guiStyle.alignment = TextAnchor.MiddleCenter;
				//guiStyle.margin = GUI.skin.button.margin;

				//GUILayout.Button(strText, guiStyle, GUILayout.Width(width), GUILayout.Height(height));//더미 버튼

				//변경
				GUILayout.Button(strText, 
					(isAvailable == false ? apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Black : apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Cyan),
					GUILayout.Width(width), GUILayout.Height(height));//더미 버튼

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				//이전
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.alignment = TextAnchor.MiddleCenter;

				//return GUILayout.Button(strText, guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				return GUILayout.Button(strText, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));
			}
		}


		public static bool ToggledButton(string strText, bool isSelected, bool isAvailable, int width, int height, string toolTip)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					//if(EditorGUIUtility.isProSkin)
					//{
					//	textColor = Color.black;
					//}
					//else
					//{
					//	textColor = Color.white;
					//}
					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					//if(EditorGUIUtility.isProSkin)
					//{
					//	//밝은 파랑 + 하늘색
					//	textColor = Color.cyan;
					//}
					//else
					//{
					//	//짙은 남색 + 흰색
					//	textColor = Color.white;
					//}

					GUI.backgroundColor = ToggleBoxColor_Selected;
					
				}


				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = textColor;
				//guiStyle.alignment = TextAnchor.MiddleCenter;
				//guiStyle.margin = GUI.skin.button.margin;

				//이전
				//GUILayout.Button(new GUIContent(strText, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));//더미 버튼

				//변경 19.11.20
				_sGUIContentWrapper.SetTextImageToolTip(strText, null, toolTip);

				GUILayout.Button(_sGUIContentWrapper.Content, 
					(isAvailable == false ? apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Black : apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Cyan),
					GUILayout.Width(width), GUILayout.Height(height));//더미 버튼

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.alignment = TextAnchor.MiddleCenter;

				//이전
				//return GUILayout.Button(new GUIContent(strText, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경 19.11.20
				_sGUIContentWrapper.SetTextImageToolTip(strText, null, toolTip);

				//return GUILayout.Button(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));
			}
		}

		public static bool ToggledButton(Texture2D texture, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					//if(EditorGUIUtility.isProSkin)
					//{
					//	textColor = Color.black;
					//	//GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
					//}
					//else
					//{
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					//}

					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					//if(EditorGUIUtility.isProSkin)
					//{
					//	//밝은 파랑 + 하늘색
					//	textColor = Color.cyan;
					//	//GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					//}
					//else
					//{
					//	//"밝은" 남색 + 흰색
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					//}

					GUI.backgroundColor = ToggleBoxColor_SelectedWithImage;
					
				}
				

				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = textColor;
				//guiStyle.alignment = TextAnchor.MiddleCenter;
				//guiStyle.margin = GUI.skin.button.margin;

				//GUILayout.Box(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				GUILayout.Box(texture, 
					(isAvailable == false ? apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Black : apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Cyan),
					GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.alignment = TextAnchor.MiddleCenter;

				//return GUILayout.Button(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				
				return GUILayout.Button(texture, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));
			}
		}


		public static bool ToggledButton(Texture2D texture, bool isSelected, bool isAvailable, int width, int height, string toolTip)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					//if(EditorGUIUtility.isProSkin)
					//{
					//	textColor = Color.black;
					//	//GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
					//}
					//else
					//{
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					//}

					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					//if(EditorGUIUtility.isProSkin)
					//{
					//	//밝은 파랑 + 하늘색
					//	textColor = Color.cyan;
					//	//GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					//}
					//else
					//{
					//	//"밝은" 남색 + 흰색
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					//}

					GUI.backgroundColor = ToggleBoxColor_SelectedWithImage;
					
				}
				

				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = textColor;
				//guiStyle.alignment = TextAnchor.MiddleCenter;
				//guiStyle.margin = GUI.skin.button.margin;
				
				//이전
				//GUILayout.Box(new GUIContent(texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, texture, toolTip);

				//GUILayout.Box(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				GUILayout.Box(_sGUIContentWrapper.Content, 
					(isAvailable == false ? apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Black : apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Cyan),
					GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.alignment = TextAnchor.MiddleCenter;
				
				//이전
				//return GUILayout.Button(new GUIContent(texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, texture, toolTip);

				//return GUILayout.Button(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));
			}
		}

		public static bool ToggledButton(Texture2D texture, string strText, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					//if(EditorGUIUtility.isProSkin)
					//{
					//	textColor = Color.black;
					//	//GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
					//}
					//else
					//{
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					//}
					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					//if(EditorGUIUtility.isProSkin)
					//{
					//	//밝은 파랑 + 하늘색
					//	textColor = Color.cyan;
					//	//GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					//}
					//else
					//{
					//	//"밝은" 남색 + 흰색
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					//}

					GUI.backgroundColor = ToggleBoxColor_SelectedWithImage;
					
				}


				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = textColor;
				//guiStyle.alignment = TextAnchor.MiddleCenter;
				//guiStyle.margin = GUI.skin.button.margin;

				//이전
				//GUILayout.Box(new GUIContent(strText, texture), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strText, texture, null);

				//GUILayout.Box(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				GUILayout.Box(_sGUIContentWrapper.Content, 
					(isAvailable == false ? apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Black : apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Cyan),
					GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.alignment = TextAnchor.MiddleCenter;

				//이전
				//return GUILayout.Button(new GUIContent(strText, texture), guiStyle, GUILayout.Width(width), GUILayout.Height(height));


				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strText, texture, null);

				//return GUILayout.Button(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));
			}
		}



		public static bool ToggledButton(Texture2D texture, string strText, bool isSelected, bool isAvailable, int width, int height, string toolTip)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					//if(EditorGUIUtility.isProSkin)
					//{
					//	textColor = Color.black;
					//	//GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
					//}
					//else
					//{
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					//}
					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					//if(EditorGUIUtility.isProSkin)
					//{
					//	//밝은 파랑 + 하늘색
					//	textColor = Color.cyan;
					//	//GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					//}
					//else
					//{
					//	//"밝은" 남색 + 흰색
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					//}

					GUI.backgroundColor = ToggleBoxColor_SelectedWithImage;
					
				}


				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = textColor;
				//guiStyle.alignment = TextAnchor.MiddleCenter;
				//guiStyle.margin = GUI.skin.button.margin;

				//이전
				//GUILayout.Box(new GUIContent(strText, texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strText, texture, toolTip);

				//GUILayout.Box(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				GUILayout.Box(	_sGUIContentWrapper.Content, 
								(isAvailable == false ? apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Black : apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Cyan),
								GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.alignment = TextAnchor.MiddleCenter;

				//이전
				//return GUILayout.Button(new GUIContent(strText, texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strText, texture, toolTip);

				//return GUILayout.Button(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));
			}
		}


		public static bool ToggledButton_Ctrl(Texture2D texture, bool isSelected, bool isAvailable, int width, int height, string toolTip, bool isCtrlKey, bool isCommandKey)
		{
			
			bool isCtrl = isCtrlKey;
#if UNITY_EDITOR_OSX
			isCtrl = isCommandKey;
#endif

			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					//if(EditorGUIUtility.isProSkin)
					//{
					//	textColor = Color.black;
					//	//GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
					//}
					//else
					//{
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					//}

					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					//if(EditorGUIUtility.isProSkin)
					//{
					//	//밝은 파랑 + 하늘색
					//	textColor = Color.cyan;
					//	//GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					//}
					//else
					//{
					//	//"밝은" 남색 + 흰색
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					//}

					GUI.backgroundColor = ToggleBoxColor_SelectedWithImage;
					
				}
				

				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = textColor;
				//guiStyle.alignment = TextAnchor.MiddleCenter;
				//guiStyle.margin = GUI.skin.button.margin;

				//이전
				//GUILayout.Box(new GUIContent(texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, texture, toolTip);

				//GUILayout.Box(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				GUILayout.Box(	_sGUIContentWrapper.Content, 
								(isAvailable == false ? apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Black : apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Cyan),
								GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				Color prevColor = GUI.backgroundColor;
				if(isCtrl)
				{	
					//Ctrl 키를 누르면 버튼 색이 바뀐다.
					if (EditorGUIUtility.isProSkin)
					{
						GUI.backgroundColor = new Color(1.0f, 0.0f, 1.0f, 1.0f);
					}
					else
					{
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 1.5f, prevColor.b * 0.5f, 1.0f);
					}
				}
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.alignment = TextAnchor.MiddleCenter;

				//이전
				//bool isBtnResult = GUILayout.Button(new GUIContent(texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, texture, toolTip);

				//bool isBtnResult = GUILayout.Button(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				bool isBtnResult = GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				return isBtnResult;
			}
		}





		public static bool ToggledButton_Ctrl(string strText, bool isSelected, bool isAvailable, int width, int height, string toolTip, bool isCtrlKey, bool isCommandKey)
		{
			
			bool isCtrl = isCtrlKey;
#if UNITY_EDITOR_OSX
			isCtrl = isCommandKey;
#endif

			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					//if(EditorGUIUtility.isProSkin)
					//{
					//	textColor = Color.black;
					//	//GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
					//}
					//else
					//{
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					//}

					GUI.backgroundColor = ToggleBoxColor_NotAvailable;
					
				}
				else if (isSelected)
				{
					//if(EditorGUIUtility.isProSkin)
					//{
					//	//밝은 파랑 + 하늘색
					//	textColor = Color.cyan;
					//	//GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					//}
					//else
					//{
					//	//"밝은" 남색 + 흰색
					//	textColor = Color.white;
					//	//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					//}

					GUI.backgroundColor = ToggleBoxColor_SelectedWithImage;
					
				}
				

				//GUI.skin.box
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.normal.textColor = textColor;
				//guiStyle.alignment = TextAnchor.MiddleCenter;
				//guiStyle.margin = GUI.skin.button.margin;

				//이전
				//GUILayout.Box(new GUIContent(strText, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strText, null, toolTip);

				//GUILayout.Box(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				GUILayout.Box(	_sGUIContentWrapper.Content, 
								(isAvailable == false ? apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Black : apGUIStyleWrapper.I.Box_MiddleCenter_BtnMargin_White2Cyan),
								GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else
			{
				Color prevColor = GUI.backgroundColor;
				if(isCtrl)
				{	
					//Ctrl 키를 누르면 버튼 색이 바뀐다.
					if (EditorGUIUtility.isProSkin)
					{
						GUI.backgroundColor = new Color(1.0f, 0.0f, 1.0f, 1.0f);
					}
					else
					{
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 1.5f, prevColor.b * 0.5f, 1.0f);
					}
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.alignment = TextAnchor.MiddleCenter;

				//이전
				//bool isBtnResult = GUILayout.Button(new GUIContent(strText, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strText, null, toolTip);

				//bool isBtnResult = GUILayout.Button(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				bool isBtnResult = GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				return isBtnResult;
			}
		}



		public static bool ToggledButton_2Side(Texture2D texture, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
					
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				//GUILayout.Box(texture, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
				bool isBtn = GUILayout.Button(texture, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				return GUILayout.Button(texture, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));
			}
		}


		public static bool ToggledButton_2Side(Texture2D texture, bool isSelected, bool isAvailable, int width, int height, string toolTip)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
					
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				//이전
				//bool isBtn = GUILayout.Button(new GUIContent(texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				_sGUIContentWrapper.SetTextImageToolTip(null, texture, toolTip);
				bool isBtn = GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				//이전
				//return GUILayout.Button(new GUIContent(texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, texture, toolTip);
				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));
			}
		}


		public static bool ToggledButton_2Side(Texture2D textureSelected, Texture2D textureNotSelected, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}
				
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				
				bool isBtn = GUILayout.Button(textureSelected, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				return GUILayout.Button(textureNotSelected, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));
			}
		}



		public static bool ToggledButton_2Side(Texture2D textureSelected, Texture2D textureNotSelected, bool isSelected, bool isAvailable, int width, int height, string toolTip)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}
				
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				//이전
				//bool isBtn = GUILayout.Button(new GUIContent(textureSelected, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, textureSelected, toolTip);
				bool isBtn = GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				//이전
				//return GUILayout.Button(new GUIContent(textureNotSelected, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, textureNotSelected, toolTip);
				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));
			}
		}

		public static bool ToggledButton_2Side(Texture2D texture, string strTextSelected, string strTextNotSelected, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.normal.textColor = textColor;

				
				//이전
				//bool isBtn = GUILayout.Button(new GUIContent(strTextSelected, texture), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strTextSelected, texture, null);
				bool isBtn = GUILayout.Button(_sGUIContentWrapper.Content, 
					(isAvailable == false ? apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding_White2Black : apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding_White2Cyan),
					GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				
				//이전
				//return GUILayout.Button(new GUIContent(strTextNotSelected, texture), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strTextNotSelected, texture, null);
				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));
			}
		}

		public static bool ToggledButton_2Side(	Texture2D texture, 
												string strTextSelected, string strTextNotSelected, 
												bool isSelected, 
												bool isAvailable, 
												int width, int height, 
												string toolTip 
												//GUIStyle alignmentStyle = null//기존 > 따로 함수를 나누자
												)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.normal.textColor = textColor;

				//if (alignmentStyle != null)
				//{
				//	guiStyle.alignment = alignmentStyle.alignment;
				//}


				//이전
				//bool isBtn = GUILayout.Button(new GUIContent(strTextSelected, texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strTextSelected, texture, toolTip);

				bool isBtn = GUILayout.Button(	_sGUIContentWrapper.Content, 
												(isAvailable == false ? apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding_White2Black : apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding_White2Cyan),
												GUILayout.Width(width), GUILayout.Height(height));
				
				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				
				//이전
				//return GUILayout.Button(new GUIContent(strTextNotSelected, texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strTextNotSelected, texture, toolTip);

				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));
			}
		}

		//추가 19.11.21 : 왼쪽 배열의 버튼의 경우
		public static bool ToggledButton_2Side_LeftAlign(	Texture2D texture, 
															string strTextSelected, string strTextNotSelected, 
															bool isSelected, 
															bool isAvailable, 
															int width, int height, 
															string toolTip 
														)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}

				_sGUIContentWrapper.SetTextImageToolTip(strTextSelected, texture, toolTip);

				bool isBtn = GUILayout.Button(	_sGUIContentWrapper.Content, 
												(isAvailable == false ? apGUIStyleWrapper.I.Button_MiddleLeft_BoxPadding_White2Black : apGUIStyleWrapper.I.Button_MiddleLeft_BoxPadding_White2Cyan),
												GUILayout.Width(width), GUILayout.Height(height));
				
				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				_sGUIContentWrapper.SetTextImageToolTip(strTextNotSelected, texture, toolTip);

				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleLeft_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));
			}
		}

		public static bool ToggledButton_2Side(string strTextSelected, string strTextNotSelected, bool isSelected, bool isAvailable, int width, int height)
		{
			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.normal.textColor = textColor;
				
				bool isBtn = GUILayout.Button(strTextSelected, 
					(isAvailable == false ? apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding_White2Black : apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding_White2Cyan),
					GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				return GUILayout.Button(strTextNotSelected, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));
			}
		}


		//public static bool ToggledButton_2Side(Texture2D textureSelected, Texture2D textureNotSelected, Texture2D textureNotAvailable,
		//										string strTextSelected, string strTextNotSelected, string strTextNotAvailable,
		//										bool isSelected, bool isAvailable, int width, int height)
		//{
		//	Color prevColor = GUI.backgroundColor;
		//	//Color textColor = Color.white;

		//	if (!isAvailable)
		//	{
		//		if (EditorGUIUtility.isProSkin)
		//		{
		//			textColor = Color.black;
		//			GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
		//		}
		//		else
		//		{
		//			textColor = Color.white;
		//			GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		//		}

		//		GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
		//		guiStyle.padding = GUI.skin.box.padding;
		//		guiStyle.normal.textColor = textColor;
		//		guiStyle.margin = GUI.skin.button.margin;

		//		if (alignmentStyle != null)
		//		{
		//			guiStyle.alignment = alignmentStyle.alignment;
		//		}

		//		//이전
		//		//GUILayout.Box(new GUIContent(strTextNotAvailable, textureNotAvailable), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

		//		//변경
		//		_sGUIContentWrapper.SetTextImageToolTip(strTextNotAvailable, textureNotAvailable, null);
		//		GUILayout.Box(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));

		//		GUI.backgroundColor = prevColor;
		//		return false;
		//	}
		//	else if (isSelected)
		//	{
		//		if (EditorGUIUtility.isProSkin)
		//		{
		//			//밝은 파랑 + 하늘색
		//			textColor = Color.cyan;
		//			GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
		//		}
		//		else
		//		{
		//			//청록색 + 흰색
		//			textColor = Color.white;
		//			//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
		//			GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
		//		}

		//		GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
		//		guiStyle.padding = GUI.skin.box.padding;
		//		guiStyle.normal.textColor = textColor;

		//		if (alignmentStyle != null)
		//		{
		//			guiStyle.alignment = alignmentStyle.alignment;
		//		}
				
		//		//이전
		//		//bool isBtn = GUILayout.Button(new GUIContent(strTextSelected, textureSelected), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

		//		//변경
		//		_sGUIContentWrapper.SetTextImageToolTip(strTextSelected, textureSelected, null);

		//		bool isBtn = GUILayout.Button(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));

		//		GUI.backgroundColor = prevColor;

		//		return isBtn;
		//	}
		//	else
		//	{
		//		GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
		//		guiStyle.padding = GUI.skin.box.padding;

		//		if (alignmentStyle != null)
		//		{
		//			guiStyle.alignment = alignmentStyle.alignment;
		//		}

		//		//이전
		//		//return GUILayout.Button(new GUIContent(strTextNotSelected, textureNotSelected), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

		//		//변경
		//		_sGUIContentWrapper.SetTextImageToolTip(strTextNotSelected, textureNotSelected, null);

		//		return GUILayout.Button(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
		//	}
		//}




		//public static bool ToggledButton_2Side(Texture2D textureSelected, Texture2D textureNotSelected, Texture2D textureNotAvailable,
		//										string strTextSelected, string strTextNotSelected, string strTextNotAvailable,
		//										bool isSelected, bool isAvailable, int width, int height, string tooltip)
		//{
		//	Color prevColor = GUI.backgroundColor;
		//	Color textColor = Color.white;

		//	if (!isAvailable)
		//	{
		//		if (EditorGUIUtility.isProSkin)
		//		{
		//			textColor = Color.black;
		//			GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
		//		}
		//		else
		//		{
		//			textColor = Color.white;
		//			GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
		//		}

		//		GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
		//		guiStyle.padding = GUI.skin.box.padding;
		//		guiStyle.normal.textColor = textColor;
		//		guiStyle.margin = GUI.skin.button.margin;

		//		if (alignmentStyle != null)
		//		{
		//			guiStyle.alignment = alignmentStyle.alignment;
		//		}

		//		//이전
		//		//GUILayout.Box(new GUIContent(strTextNotAvailable, textureNotAvailable, tooltip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

		//		//변경
		//		_sGUIContentWrapper.SetTextImageToolTip(strTextNotAvailable, textureNotAvailable, tooltip);

		//		GUILayout.Box(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));

		//		GUI.backgroundColor = prevColor;
		//		return false;
		//	}
		//	else if (isSelected)
		//	{
		//		if (EditorGUIUtility.isProSkin)
		//		{
		//			//밝은 파랑 + 하늘색
		//			textColor = Color.cyan;
		//			GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
		//		}
		//		else
		//		{
		//			//청록색 + 흰색
		//			textColor = Color.white;
		//			//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
		//			GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
		//		}

		//		GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
		//		guiStyle.padding = GUI.skin.box.padding;
		//		guiStyle.normal.textColor = textColor;

		//		if (alignmentStyle != null)
		//		{
		//			guiStyle.alignment = alignmentStyle.alignment;
		//		}

		//		//이전
		//		//bool isBtn = GUILayout.Button(new GUIContent(strTextSelected, textureSelected, tooltip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

		//		//변경
		//		_sGUIContentWrapper.SetTextImageToolTip(strTextSelected, textureSelected, tooltip);
		//		bool isBtn = GUILayout.Button(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));

		//		GUI.backgroundColor = prevColor;

		//		return isBtn;
		//	}
		//	else
		//	{
		//		GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
		//		guiStyle.padding = GUI.skin.box.padding;

		//		if (alignmentStyle != null)
		//		{
		//			guiStyle.alignment = alignmentStyle.alignment;
		//		}

		//		//이전
		//		//return GUILayout.Button(new GUIContent(strTextNotSelected, textureNotSelected, tooltip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

		//		//변경
		//		_sGUIContentWrapper.SetTextImageToolTip(strTextNotSelected, textureNotSelected, tooltip);
		//		return GUILayout.Button(_sGUIContentWrapper.Content, guiStyle, GUILayout.Width(width), GUILayout.Height(height));
		//	}
		//}


		public static bool ToggledButton_2Side_LeftAlign(	Texture2D textureSelected, Texture2D textureNotSelected, Texture2D textureNotAvailable,
															string strTextSelected, string strTextNotSelected, string strTextNotAvailable,
															bool isSelected, bool isAvailable, int width, int height, string tooltip)
		{
			Color prevColor = GUI.backgroundColor;
			//Color textColor = Color.white;

			if (!isAvailable)
			{
				if (EditorGUIUtility.isProSkin)
				{
					//textColor = Color.black;
					GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
				}
				else
				{
					//textColor = Color.white;
					GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.box);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.normal.textColor = textColor;
				//guiStyle.margin = GUI.skin.button.margin;

				//if (alignmentStyle != null)
				//{
				//	guiStyle.alignment = alignmentStyle.alignment;
				//}

				//이전
				//GUILayout.Box(new GUIContent(strTextNotAvailable, textureNotAvailable, tooltip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strTextNotAvailable, textureNotAvailable, tooltip);

				GUILayout.Box(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Box_MiddleLeft_BtnMargin_White2Black, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;
				return false;
			}
			else if (isSelected)
			{
				if (EditorGUIUtility.isProSkin)
				{
					//밝은 파랑 + 하늘색
					//textColor = Color.cyan;
					GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
				}
				else
				{
					//청록색 + 흰색
					//textColor = Color.white;
					//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
					GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;
				//guiStyle.normal.textColor = textColor;

				//if (alignmentStyle != null)
				//{
				//	guiStyle.alignment = alignmentStyle.alignment;
				//}

				//이전
				//bool isBtn = GUILayout.Button(new GUIContent(strTextSelected, textureSelected, tooltip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strTextSelected, textureSelected, tooltip);
				bool isBtn = GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleLeft_BoxPadding_White2Cyan, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				return isBtn;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				//if (alignmentStyle != null)
				//{
				//	guiStyle.alignment = alignmentStyle.alignment;
				//}

				//이전
				//return GUILayout.Button(new GUIContent(strTextNotSelected, textureNotSelected, tooltip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(strTextNotSelected, textureNotSelected, tooltip);
				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleLeft_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));
			}
		}


		//추가 : Ctrl을 누르면 색상이 바뀐다.
		public static bool ToggledButton_2Side_Ctrl(Texture2D texture, bool isSelected, bool isAvailable, int width, int height, string toolTip, bool isCtrlKey, bool isCommandKey)
		{
			bool isCtrl = isCtrlKey;
#if UNITY_EDITOR_OSX
			isCtrl = isCommandKey;
#endif
			if (isSelected || !isAvailable || isCtrl)
			{
				Color prevColor = GUI.backgroundColor;
				//Color textColor = Color.white;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if(isCtrl)
				{
					//추가 : Ctrl을 누르면 연녹색으로 바뀐다.
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(1.0f, 0.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 1.5f, prevColor.b * 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
					
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				//이전
				//bool isBtn = GUILayout.Button(new GUIContent(texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, texture, toolTip);
				bool isBtn = GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				//이전
				//return GUILayout.Button(new GUIContent(texture, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, texture, toolTip);
				return GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));
			}
		}




		public static bool ToggledButton_2Side_Ctrl(Texture2D textureSelected, Texture2D textureNotSelected, bool isSelected, bool isAvailable, int width, int height, string toolTip, bool isCtrlKey, bool isCommandKey)
		{
			bool isCtrl = isCtrlKey;
#if UNITY_EDITOR_OSX
			isCtrl = isCommandKey;
#endif

			if (isSelected || !isAvailable)
			{
				Color prevColor = GUI.backgroundColor;

				if (!isAvailable)
				{
					//회색 (Pro는 글자도 진해짐)
					if(EditorGUIUtility.isProSkin)
					{
						//textColor = Color.black;
						GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 1.0f);
					}
					else
					{
						//textColor = Color.white;
						GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
					}
				}
				else if(isCtrl)
				{
					//추가 : Ctrl을 누르면 연녹색으로 바뀐다.
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(1.0f, 0.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 1.5f, prevColor.b * 0.5f, 1.0f);
					}
				}
				else if (isSelected)
				{
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						//GUI.backgroundColor = new Color(prevColor.r * 0.6f, prevColor.g * 1.6f, prevColor.b * 1.6f, 1.0f);
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 0.8f, prevColor.b * 1.1f, 1.0f);
					}
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				//이전
				//bool isBtn = GUILayout.Button(new GUIContent(textureSelected, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, textureSelected, toolTip);
				bool isBtn = GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				if (!isAvailable)
				{
					return false;
				}

				return isBtn;
			}
			else
			{
				Color prevColor = GUI.backgroundColor;

				if(isCtrl)
				{
					//추가 : Ctrl을 누르면 연녹색으로 바뀐다.
					if (EditorGUIUtility.isProSkin)
					{
						//밝은 파랑 + 하늘색
						//textColor = Color.cyan;
						GUI.backgroundColor = new Color(1.0f, 0.0f, 1.0f, 1.0f);
					}
					else
					{
						//청록색 + 흰색
						//textColor = Color.white;
						GUI.backgroundColor = new Color(prevColor.r * 0.2f, prevColor.g * 1.5f, prevColor.b * 0.5f, 1.0f);
					}
				}

				//GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
				//guiStyle.padding = GUI.skin.box.padding;

				//이전
				//bool isBtnResult = GUILayout.Button(new GUIContent(textureNotSelected, toolTip), guiStyle, GUILayout.Width(width), GUILayout.Height(height));

				//변경
				_sGUIContentWrapper.SetTextImageToolTip(null, textureNotSelected, toolTip);
				bool isBtnResult = GUILayout.Button(_sGUIContentWrapper.Content, apGUIStyleWrapper.I.Button_MiddleCenter_BoxPadding, GUILayout.Width(width), GUILayout.Height(height));

				GUI.backgroundColor = prevColor;

				return isBtnResult;
			}
		}


		//----------------------------------------------------------------------------------------------------------
		// Delayed Vector2Field
		//----------------------------------------------------------------------------------------------------------
		

		public static Vector2 DelayedVector2Field(Vector2 vectorValue, int width)
		{
			Vector2 result = vectorValue;

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
			if (width > 100)
			{
				int widthLabel = 15;
				int widthData = ((width - ((15 + 2) * 2)) / 2) - 2;
				EditorGUILayout.LabelField(Text_X, GUILayout.Width(widthLabel));
				result.x = EditorGUILayout.DelayedFloatField(vectorValue.x, GUILayout.Width(widthData));

				EditorGUILayout.LabelField(Text_Y, GUILayout.Width(widthLabel));
				result.y = EditorGUILayout.DelayedFloatField(vectorValue.y, GUILayout.Width(widthData));
			}
			else
			{
				int widthData = (width / 2) - 2;
				result.x = EditorGUILayout.DelayedFloatField(vectorValue.x, GUILayout.Width(widthData));
				result.y = EditorGUILayout.DelayedFloatField(vectorValue.y, GUILayout.Width(widthData));
			}



			EditorGUILayout.EndHorizontal();

			return result;
		}

		//----------------------------------------------------------------------------------------------------------
		// 스크롤을 하는 경우, 해당 UI가 출력될지 여부를 리턴한다
		//----------------------------------------------------------------------------------------------------------
		/// <summary>
		/// 스크롤안에 아이템을 출력하는 경우, 출력 영역 안쪽인지 바깥쪽인이 확인할 필요가 있다.
		/// 항목간에 여백이 없어야 한다.
		/// 약간의 여유를 가지고 리턴한다.
		/// </summary>
		/// <param name="itemPosY">항목의 Y값 (따로 계산해둘것)</param>
		/// <param name="itemHeight">항목의 Height</param>
		/// <param name="scroll">스크롤값 (Y값만 사용함)</param>
		/// <param name="scrollLayoutHeight">스크롤 레이아웃의 Height</param>
		/// <returns></returns>
		public static bool IsItemInScroll(int itemPosY, int itemHeight, Vector2 scroll, int scrollLayoutHeight)
		{
			//기본
			if (itemPosY < scroll.y - (itemHeight * 2))
			{
				return false;
			}

			if (itemPosY > (scroll.y + scrollLayoutHeight) + itemHeight)
			{
				return false;
			}

			//테스트
			//if (itemPosY - 50 < scroll.y)
			//{
			//	return false;
			//}

			//if(itemPosY > (scroll.y + scrollLayoutHeight) - 50)
			//{
			//	return false;
			//}

			return true;
		}

		//----------------------------------------------------------------------------------------------------------
		// Slider Float (짧은 경우)
		//----------------------------------------------------------------------------------------------------------
		public static float FloatSlider(string label, float value, float minValue, float maxValue, int totalWidth, int labelWidth)
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Width(totalWidth), GUILayout.Height(25));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(label, GUILayout.Width(labelWidth));

			int width_Slider = (int)((totalWidth - (5 + labelWidth)) * 0.6f);
			int width_Field = totalWidth - (5 + labelWidth + width_Slider + 7);
			value = GUILayout.HorizontalSlider(value, minValue, maxValue, GUILayout.Width(width_Slider), GUILayout.Height(25));
			float nextValue = EditorGUILayout.FloatField(value, GUILayout.Width(width_Field));

			EditorGUILayout.EndHorizontal();

			return Mathf.Clamp(nextValue, minValue, maxValue);
		}

		public static int IntSlider(string label, int value, int minValue, int maxValue, int totalWidth, int labelWidth)
		{
			EditorGUILayout.BeginHorizontal(GUILayout.Width(totalWidth), GUILayout.Height(25));
			GUILayout.Space(5);
			EditorGUILayout.LabelField(label, GUILayout.Width(labelWidth));

			int width_Slider = (int)((totalWidth - (5 + labelWidth)) * 0.6f);
			int width_Field = totalWidth - (5 + labelWidth + width_Slider + 7);
			
			float fValue = GUILayout.HorizontalSlider(value, minValue, maxValue, GUILayout.Width(width_Slider), GUILayout.Height(25));
			value = (int)(fValue + 0.5f);
			int nextValue = EditorGUILayout.IntField(value, GUILayout.Width(width_Field));

			EditorGUILayout.EndHorizontal();

			return Mathf.Clamp(nextValue, minValue, maxValue);
		}

		//----------------------------------------------------------------------------------------------------------
		/// <summary>
		/// Editor의 Left, Right 탭 상단에 "접을 수 있는" 바
		/// 이 함수는 Left-Right로 접을 수 있다.
		/// </summary>
		public static apEditor.UI_FOLD_BTN_RESULT DrawTabFoldTitle_H(apEditor editor, int posX, int posY, int width, int height, apEditor.UI_FOLD_TYPE foldType, bool isLeftUI)
		{
			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);//진한 파란색
			GUI.Box(new Rect(posX, posY, width, height), "", WhiteGUIStyle);
			GUI.backgroundColor = prevColor;

			apEditor.UI_FOLD_BTN_RESULT result = apEditor.UI_FOLD_BTN_RESULT.None;

			
			//Left UI일 때
			//- Unfolded 상태에서 : 우측에 "<<" 아이콘
			//- Folded 상태에서 : 우측에 ">>" 아이콘

			//Right UI일 때
			//- Unfolded 상태에서 : 좌측에 ">>" 아이콘
			//- Folded 상태에서 : 좌측에 "<<" 아이콘

			int leftMargin = 0;
			Texture2D img_FoldH = null;
			if(isLeftUI)
			{
				leftMargin = width - 22;
				if(foldType == apEditor.UI_FOLD_TYPE.Unfolded)	{ img_FoldH = editor.ImageSet.Get(apImageSet.PRESET.GUI_TabFoldLeft_x16); }
				else											{ img_FoldH = editor.ImageSet.Get(apImageSet.PRESET.GUI_TabFoldRight_x16); }
			}
			else
			{
				leftMargin = 2;
				if(foldType == apEditor.UI_FOLD_TYPE.Unfolded)	{ img_FoldH = editor.ImageSet.Get(apImageSet.PRESET.GUI_TabFoldRight_x16); }
				else											{ img_FoldH = editor.ImageSet.Get(apImageSet.PRESET.GUI_TabFoldLeft_x16); }
			}

			//GUIStyle guiStyle_Btn = new GUIStyle(GUI.skin.label);
			//guiStyle_Btn.alignment = TextAnchor.MiddleCenter;
			//guiStyle_Btn.margin = new RectOffset(0, 0, 0, 0);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height+1));//버튼의 크기는 강제
			GUILayout.Space(leftMargin);
			if(GUILayout.Button(img_FoldH, apGUIStyleWrapper.I.Label_MiddleCenter_Margin0, GUILayout.Width(20), GUILayout.Height(height+1)))
			{
				result = apEditor.UI_FOLD_BTN_RESULT.ToggleFold_Horizontal;
			}
			EditorGUILayout.EndHorizontal();


			return result;
		}

		/// <summary>
		/// Editor의 Right1_Upper 탭 상단에 "접을 수 있는" 바
		/// 이 함수는 가로, 세로로 접을 수 있다.
		/// Right1_Upper 한정
		/// </summary>
		public static apEditor.UI_FOLD_BTN_RESULT DrawTabFoldTitle_HV(apEditor editor, int posX, int posY, int width, int height, apEditor.UI_FOLD_TYPE foldTypeH, apEditor.UI_FOLD_TYPE foldTypeV)
		{
			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);//진한 파란색
			GUI.Box(new Rect(posX, posY, width, height), "", WhiteGUIStyle);
			GUI.backgroundColor = prevColor;

			apEditor.UI_FOLD_BTN_RESULT result = apEditor.UI_FOLD_BTN_RESULT.None;

			
			//이 함수는 무조건 RightUI이다.
			//foldTypeH가 우선된다.

			//foldTypeH 상태
			//- Unfolded : 좌측에 ">>" 아이콘, 우측에 "-" 또는 "ㅁ" 아이콘
			//- Folded : 좌측에 "<<" 아이콘

			//foldTypeV 상태 (foldTypeH가 Unfolded일때만)
			//- Unfolded : 우측에 "-" 아이콘
			//- Folded : 우측에 "ㅁ" 아이콘

			int leftMargin = 2;
			int middleMargin = width - (44);
			Texture2D img_FoldH = null;
			Texture2D img_FoldV = null;

			if(foldTypeH == apEditor.UI_FOLD_TYPE.Unfolded)	{ img_FoldH = editor.ImageSet.Get(apImageSet.PRESET.GUI_TabFoldRight_x16); }
			else											{ img_FoldH = editor.ImageSet.Get(apImageSet.PRESET.GUI_TabFoldLeft_x16); }

			if(foldTypeV == apEditor.UI_FOLD_TYPE.Unfolded)	{ img_FoldV = editor.ImageSet.Get(apImageSet.PRESET.GUI_TabFoldVHide_x16); }
			else											{ img_FoldV = editor.ImageSet.Get(apImageSet.PRESET.GUI_TabFoldVShow_x16); }
			

			//GUIStyle guiStyle_Btn = new GUIStyle(GUI.skin.label);
			//guiStyle_Btn.alignment = TextAnchor.MiddleCenter;
			//guiStyle_Btn.margin = new RectOffset(0, 0, 0, 0);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height+1));//버튼의 크기는 강제
			GUILayout.Space(leftMargin);
			if(GUILayout.Button(img_FoldH, apGUIStyleWrapper.I.Label_MiddleCenter_Margin0, GUILayout.Width(20), GUILayout.Height(height+1)))
			{
				result = apEditor.UI_FOLD_BTN_RESULT.ToggleFold_Horizontal;
			}
			if (foldTypeH == apEditor.UI_FOLD_TYPE.Unfolded)
			{
				GUILayout.Space(middleMargin);
				if (GUILayout.Button(img_FoldV, apGUIStyleWrapper.I.Label_MiddleCenter_Margin0, GUILayout.Width(20), GUILayout.Height(height+1)))
				{
					result = apEditor.UI_FOLD_BTN_RESULT.ToggleFold_Vertical;
				}
			}
			EditorGUILayout.EndHorizontal();


			return result;
		}


		/// <summary>
		/// Editor의 Right1_Lower 탭 상단에 "접을 수 있는" 바
		/// 이 함수는 세로로만 접을 수 있다.
		/// Right1_Lower 한정
		/// </summary>
		public static apEditor.UI_FOLD_BTN_RESULT DrawTabFoldTitle_V(apEditor editor, int posX, int posY, int width, int height, apEditor.UI_FOLD_TYPE foldType)
		{
			Color prevColor = GUI.backgroundColor;
			GUI.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);//진한 파란색
			GUI.Box(new Rect(posX, posY, width, height), "", WhiteGUIStyle);
			GUI.backgroundColor = prevColor;

			apEditor.UI_FOLD_BTN_RESULT result = apEditor.UI_FOLD_BTN_RESULT.None;

			
			//- Unfolded 상태에서 : 우측에 "-" 아이콘
			//- Folded 상태에서 : 우측에 "ㅁ" 아이콘

			int leftMargin = width - 22;
			Texture2D img_FoldV = null;
			if(foldType == apEditor.UI_FOLD_TYPE.Unfolded)	{ img_FoldV = editor.ImageSet.Get(apImageSet.PRESET.GUI_TabFoldVHide_x16); }
			else											{ img_FoldV = editor.ImageSet.Get(apImageSet.PRESET.GUI_TabFoldVShow_x16); }

			//GUIStyle guiStyle_Btn = new GUIStyle(GUI.skin.label);
			//guiStyle_Btn.alignment = TextAnchor.MiddleCenter;
			//guiStyle_Btn.margin = new RectOffset(0, 0, 0, 0);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(height+1));//버튼의 크기는 강제
			GUILayout.Space(leftMargin);
			if(GUILayout.Button(img_FoldV, apGUIStyleWrapper.I.Label_MiddleCenter_Margin0, GUILayout.Width(20), GUILayout.Height(height+1)))
			{
				result = apEditor.UI_FOLD_BTN_RESULT.ToggleFold_Vertical;
			}
			EditorGUILayout.EndHorizontal();


			return result;
		}


		//----------------------------------------------------------------------------------------------------------
		// White Color Texture
		//----------------------------------------------------------------------------------------------------------
		//private static Texture2D _whiteSmallTexture = null;
		public static Texture2D WhiteTexture
		{
			get
			{
				return EditorGUIUtility.whiteTexture;
			}
		}

		private static GUIStyle _whiteGUIStyle = null;
		public static GUIStyle WhiteGUIStyle
		{
			get
			{
				if(_whiteGUIStyle == null)
				{
					_whiteGUIStyle = new GUIStyle(GUIStyle.none);
					_whiteGUIStyle.normal.background = WhiteTexture;
				}

				return _whiteGUIStyle;
			}
		}

		private static GUIStyle _whiteGUIStyle_Box = null;
		public static GUIStyle WhiteGUIStyle_Box
		{
			get
			{
				if(_whiteGUIStyle_Box == null)
				{
					_whiteGUIStyle_Box = new GUIStyle(GUI.skin.box);
					_whiteGUIStyle_Box.normal.background = WhiteTexture;
				}

				return _whiteGUIStyle_Box;
			}
		}

		//----------------------------------------------------------------------------------------------------------
		// 미리 정의된 텍스트 (추가적인 생성 없게)
		//----------------------------------------------------------------------------------------------------------
		private static string s_Text_X = "X";
		private static string s_Text_Y = "X";
		private static string s_Text_EMPTY = "";
		private static string s_Text_None = "None";
		private static string s_Text_NoneName = "<None>";

		/// <summary>"X"</summary>
		public static string Text_X { get { return s_Text_X; } }

		/// <summary>"Y"</summary>
		public static string Text_Y { get { return s_Text_Y; } }

		/// <summary>""</summary>
		public static string Text_EMPTY { get { return s_Text_EMPTY; } }

		/// <summary>"None"</summary>
		public static string Text_None { get { return s_Text_None; } }

		/// <summary>"(None)"</summary>
		public static string Text_NoneName { get { return s_Text_NoneName; } }

		//----------------------------------------------------------------------------------------------------------
		// Graphics Functions
		//----------------------------------------------------------------------------------------------------------
		public static float DistanceFromLine(Vector2 posA, Vector2 posB, Vector2 posTarget)
		{
			//float lineLen = Vector2.Distance(posA, posB);
			//if(lineLen < 0.1f)
			//{
			//	return Vector2.Distance(posA, posTarget);
			//}

			//float proj = (posTarget.x - posA.x) * (posB.x - posA.x) + (posTarget.y - posA.y) * (posB.y - posA.y);
			//if(proj < 0)
			//{
			//	return Vector2.Distance(posA, posTarget);
			//}
			//else if(proj > lineLen)
			//{
			//	return Vector2.Distance(posB, posTarget);
			//}

			//return Mathf.Abs((-1) * (posTarget.x - posA.x) * (posB.y - posA.y) + (posTarget.y - posA.y) * (posB.x - posA.x)) / lineLen;

			//float lineLen = Vector2.Distance(posA, posB);
			float dotA = Vector2.Dot(posTarget - posA, (posB - posA).normalized);
			float dotB = Vector2.Dot(posTarget - posB, (posA - posB).normalized);

			if (dotA < 0.0f)
			{
				return Vector2.Distance(posA, posTarget);
			}

			if (dotB < 0.0f)
			{
				return Vector2.Distance(posB, posTarget);
			}

			return Vector2.Distance((posA + (posB - posA).normalized * dotA), posTarget);
		}

		public static bool IsMouseInMesh(Vector2 mousePos, apMesh targetMesh)
		{
			Vector2 mousePosW = apGL.GL2World(mousePos);

			Vector2 mousePosL = mousePosW + targetMesh._offsetPos;//<<이걸 추가해줘야 Local Pos가 된다.

			List<apMeshPolygon> polygons = targetMesh._polygons;
			for (int iPoly = 0; iPoly < polygons.Count; iPoly++)
			{
				List<apMeshTri> tris = polygons[iPoly]._tris;
				for (int iTri = 0; iTri < tris.Count; iTri++)
				{
					apMeshTri tri = tris[iTri];
					if (tri.IsPointInTri(mousePosL))
					{
						return true;
					}
				}
			}
			return false;
		}


		public static bool IsMouseInMesh(Vector2 mousePos, apMesh targetMesh, apMatrix3x3 matrixWorldToMeshLocal)
		{
			Vector2 mousePosW = apGL.GL2World(mousePos);

			Vector2 mousePosL = matrixWorldToMeshLocal.MultiplyPoint(mousePosW);

			//Vector2 mousePosL = mousePosW + targetMesh._offsetPos;//<<이걸 추가해줘야 Local Pos가 된다.

			List<apMeshPolygon> polygons = targetMesh._polygons;
			for (int iPoly = 0; iPoly < polygons.Count; iPoly++)
			{
				List<apMeshTri> tris = polygons[iPoly]._tris;
				for (int iTri = 0; iTri < tris.Count; iTri++)
				{
					apMeshTri tri = tris[iTri];
					if (tri.IsPointInTri(mousePosL))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool IsMouseInRenderUnitMesh(Vector2 mousePos, apRenderUnit meshRenderUnit)
		{
			if (meshRenderUnit._meshTransform == null)
			{
				return false;
			}

			if (meshRenderUnit._meshTransform._mesh == null || meshRenderUnit._renderVerts.Count == 0)
			{
				return false;
			}

			apMesh targetMesh = meshRenderUnit._meshTransform._mesh;
			List<apRenderVertex> rVerts = meshRenderUnit._renderVerts;

			Vector2 mousePosW = apGL.GL2World(mousePos);

			apRenderVertex rVert0, rVert1, rVert2;
			List<apMeshPolygon> polygons = targetMesh._polygons;
			for (int iPoly = 0; iPoly < polygons.Count; iPoly++)
			{
				List<apMeshTri> tris = polygons[iPoly]._tris;
				for (int iTri = 0; iTri < tris.Count; iTri++)
				{
					apMeshTri tri = tris[iTri];
					rVert0 = rVerts[tri._verts[0]._index];
					rVert1 = rVerts[tri._verts[1]._index];
					rVert2 = rVerts[tri._verts[2]._index];

					if (apMeshTri.IsPointInTri(mousePosW,
												rVert0._pos_World,
												rVert1._pos_World,
												rVert2._pos_World))
					{
						return true;
					}
				}
			}
			return false;
		}


		public static bool IsPointInTri(Vector2 point, Vector2 triPoint0, Vector2 triPoint1, Vector2 triPoint2)
		{
			float s = triPoint0.y * triPoint2.x - triPoint0.x * triPoint2.y + (triPoint2.y - triPoint0.y) * point.x + (triPoint0.x - triPoint2.x) * point.y;
			float t = triPoint0.x * triPoint1.y - triPoint0.y * triPoint1.x + (triPoint0.y - triPoint1.y) * point.x + (triPoint1.x - triPoint0.x) * point.y;

			if ((s < 0) != (t < 0))
			{
				return false;
			}

			var A = -triPoint1.y * triPoint2.x + triPoint0.y * (triPoint2.x - triPoint1.x) + triPoint0.x * (triPoint1.y - triPoint2.y) + triPoint1.x * triPoint2.y;
			if (A < 0.0)
			{
				s = -s;
				t = -t;
				A = -A;
			}
			return s > 0 && t > 0 && (s + t) <= A;

		}
		//----------------------------------------------------------------------------------------------------
		public static apImageSet.PRESET GetModifierIconType(apModifierBase.MODIFIER_TYPE modType)
		{
			switch (modType)
			{
				case apModifierBase.MODIFIER_TYPE.Base:
					return apImageSet.PRESET.Modifier_Volume;

				case apModifierBase.MODIFIER_TYPE.Volume:
					return apImageSet.PRESET.Modifier_Volume;

				case apModifierBase.MODIFIER_TYPE.Morph:
					return apImageSet.PRESET.Modifier_Morph;

				case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
					return apImageSet.PRESET.Modifier_AnimatedMorph;

				case apModifierBase.MODIFIER_TYPE.Rigging:
					return apImageSet.PRESET.Modifier_Rigging;

				case apModifierBase.MODIFIER_TYPE.Physic:
					return apImageSet.PRESET.Modifier_Physic;

				case apModifierBase.MODIFIER_TYPE.TF:
					return apImageSet.PRESET.Modifier_TF;

				case apModifierBase.MODIFIER_TYPE.AnimatedTF:
					return apImageSet.PRESET.Modifier_AnimatedTF;

				case apModifierBase.MODIFIER_TYPE.FFD:
					return apImageSet.PRESET.Modifier_FFD;

				case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
					return apImageSet.PRESET.Modifier_AnimatedFFD;

			}
			return apImageSet.PRESET.Modifier_Volume;
		}

		public static apImageSet.PRESET GetPhysicsPresetIconType(apPhysicsPresetUnit.ICON iconType)
		{
			switch (iconType)
			{
				case apPhysicsPresetUnit.ICON.Cloth1:
					return apImageSet.PRESET.Physic_PresetCloth1;
				case apPhysicsPresetUnit.ICON.Cloth2:
					return apImageSet.PRESET.Physic_PresetCloth2;
				case apPhysicsPresetUnit.ICON.Cloth3:
					return apImageSet.PRESET.Physic_PresetCloth3;
				case apPhysicsPresetUnit.ICON.Flag:
					return apImageSet.PRESET.Physic_PresetFlag;
				case apPhysicsPresetUnit.ICON.Hair:
					return apImageSet.PRESET.Physic_PresetHair;
				case apPhysicsPresetUnit.ICON.Ribbon:
					return apImageSet.PRESET.Physic_PresetRibbon;
				case apPhysicsPresetUnit.ICON.RubberHard:
					return apImageSet.PRESET.Physic_PresetRubberHard;
				case apPhysicsPresetUnit.ICON.RubberSoft:
					return apImageSet.PRESET.Physic_PresetRubberSoft;
				case apPhysicsPresetUnit.ICON.Custom1:
					return apImageSet.PRESET.Physic_PresetCustom1;
				case apPhysicsPresetUnit.ICON.Custom2:
					return apImageSet.PRESET.Physic_PresetCustom2;
				case apPhysicsPresetUnit.ICON.Custom3:
					return apImageSet.PRESET.Physic_PresetCustom3;
			}
			return apImageSet.PRESET.Physic_PresetCustom3;
		}


		public static apImageSet.PRESET GetControlParamPresetIconType(apControlParam.ICON_PRESET iconType)
		{
			switch (iconType)
			{
				case apControlParam.ICON_PRESET.None:
					return apImageSet.PRESET.Hierarchy_Param;
				case apControlParam.ICON_PRESET.Head:
					return apImageSet.PRESET.ParamPreset_Head;
				case apControlParam.ICON_PRESET.Body:
					return apImageSet.PRESET.ParamPreset_Body;
				case apControlParam.ICON_PRESET.Hand:
					return apImageSet.PRESET.ParamPreset_Hand;
				case apControlParam.ICON_PRESET.Face:
					return apImageSet.PRESET.ParamPreset_Face;
				case apControlParam.ICON_PRESET.Eye:
					return apImageSet.PRESET.ParamPreset_Eye;
				case apControlParam.ICON_PRESET.Hair:
					return apImageSet.PRESET.ParamPreset_Hair;
				case apControlParam.ICON_PRESET.Equipment:
					return apImageSet.PRESET.ParamPreset_Equip;
				case apControlParam.ICON_PRESET.Cloth:
					return apImageSet.PRESET.ParamPreset_Cloth;
				case apControlParam.ICON_PRESET.Force:
					return apImageSet.PRESET.ParamPreset_Force;
				case apControlParam.ICON_PRESET.Etc:
					return apImageSet.PRESET.ParamPreset_Etc;
			}
			return apImageSet.PRESET.ParamPreset_Etc;
		}

		public static apControlParam.ICON_PRESET GetControlParamPresetIconTypeByCategory(apControlParam.CATEGORY category)
		{
			switch (category)
			{
				case apControlParam.CATEGORY.Head:
					return apControlParam.ICON_PRESET.Head;
				case apControlParam.CATEGORY.Body:
					return apControlParam.ICON_PRESET.Body;
				case apControlParam.CATEGORY.Face:
					return apControlParam.ICON_PRESET.Face;
				case apControlParam.CATEGORY.Hair:
					return apControlParam.ICON_PRESET.Hair;
				case apControlParam.CATEGORY.Equipment:
					return apControlParam.ICON_PRESET.Equipment;
				case apControlParam.CATEGORY.Force:
					return apControlParam.ICON_PRESET.Force;
				case apControlParam.CATEGORY.Etc:
					return apControlParam.ICON_PRESET.Etc;
			}
			return apControlParam.ICON_PRESET.Etc;

		}


		public static apImageSet.PRESET GetSmallModIconType(apModifierBase.MODIFIER_TYPE modType)
		{
			switch (modType)
			{
				case apModifierBase.MODIFIER_TYPE.Base:
					return apImageSet.PRESET.SmallMod_ControlLayer;

				case apModifierBase.MODIFIER_TYPE.Volume:
					return apImageSet.PRESET.SmallMod_ControlLayer;

				case apModifierBase.MODIFIER_TYPE.Morph:
					return apImageSet.PRESET.SmallMod_Morph;

				case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
					return apImageSet.PRESET.SmallMod_AnimMorph;

				case apModifierBase.MODIFIER_TYPE.Rigging:
					return apImageSet.PRESET.SmallMod_Rigging;

				case apModifierBase.MODIFIER_TYPE.Physic:
					return apImageSet.PRESET.SmallMod_Physics;

				case apModifierBase.MODIFIER_TYPE.TF:
					return apImageSet.PRESET.SmallMod_TF;

				case apModifierBase.MODIFIER_TYPE.AnimatedTF:
					return apImageSet.PRESET.SmallMod_AnimTF;

				case apModifierBase.MODIFIER_TYPE.FFD:
					return apImageSet.PRESET.SmallMod_ControlLayer;

				case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
					return apImageSet.PRESET.SmallMod_ControlLayer;
			}
			return apImageSet.PRESET.Modifier_Volume;
		}
		//----------------------------------------------------------------------------------------------------

		public class NameAndIndexPair
		{
			public string _strName = "";
			public int _index = 0;
			public int _indexStrLength = 0;
			public NameAndIndexPair(string strName, string strIndex)
			{
				_strName = strName;
				if (strIndex.Length > 0)
				{
					_index = Int32.Parse(strIndex);
					_indexStrLength = strIndex.Length;
				}
				else
				{
					_index = 0;
					_indexStrLength = 0;
				}
			}
			public string MakeNewName(int index)
			{
				string strIndex = index + "";
				if (strIndex.Length < _indexStrLength)
				{
					int dLength = _indexStrLength - strIndex.Length;
					//0을 붙여주자
					for (int i = 0; i < dLength; i++)
					{
						strIndex = "0" + strIndex;
					}
				}

				return _strName + strIndex;
			}
		}

		public static NameAndIndexPair ParseNumericName(string srcName)
		{
			if (string.IsNullOrEmpty(srcName))
			{
				return new NameAndIndexPair("<None>", "");
			}

			//1. 이름 내에 "숫자로 된 부분"이 있다면, 그중 가장 "뒤의 숫자"를 1 올려서 갱신한다.
			string strName_First = "", strName_Index = "";
			int strMode = 1;//0 : First, 1 : Index
			for (int i = srcName.Length - 1; i >= 0; i--)
			{
				string curStr = srcName.Substring(i, 1);
				switch (strMode)
				{
					case 1:
						{
							if (IsNumericString(curStr))
							{
								strName_Index = curStr + strName_Index;
							}
							else
							{
								strName_First = curStr + strName_First;
								strMode = 0;
							}
						}
						break;

					case 0:
						strName_First = curStr + strName_First;
						break;
				}
			}
			return new NameAndIndexPair(strName_First, strName_Index);
		}


		private static bool IsNumericString(string str)
		{
			if (str == "0" || str == "1" || str == "2" ||
				str == "3" || str == "4" || str == "5" ||
				str == "6" || str == "7" || str == "8" ||
				str == "9")
			{
				return true;
			}
			return false;
		}


		//---------------------------------------------------------------------------------------
		public static T[] AddItemToArray<T>(T addItem, T[] srcArray)
		{
			if (srcArray == null || srcArray.Length == 0)
			{
				return new T[] { addItem };
			}

			int prevArraySize = srcArray.Length;
			int nextArraySize = prevArraySize + 1;

			T[] nextArray = new T[nextArraySize];
			for (int i = 0; i < prevArraySize; i++)
			{
				nextArray[i] = srcArray[i];
			}
			nextArray[nextArraySize - 1] = addItem;
			return nextArray;
		}

		//---------------------------------------------------------------------------------------
		private static string[] s_renderTextureNames = null;
		public static string[] GetRenderTextureSizeNames()
		{
		//	public enum RENDER_TEXTURE_SIZE
		//{
		//	s_64, s_128, s_256, s_512, s_1024
		//}
			if(s_renderTextureNames == null)
			{
				s_renderTextureNames = new string[] { "64", "128", "256", "512", "1024" };
			}
			return s_renderTextureNames;
		}

		//---------------------------------------------------------------------------------------
		//색상 관련 (Hue 방식)
		
		public static Color GetSimilarColor(Color srcColor, float minSaturation, float maxSaturation, float minValue, float maxValue)
		{
			float hue = 0.0f;
			float sat = 0.0f;
			float value = 0.0f;

			Color.RGBToHSV(srcColor, out hue, out sat, out value);

			//HSV 상태에서 랜덤을 주는게 비슷한 색상을 유지하는게 좋다.
			float randHue = UnityEngine.Random.Range(-0.05f, 0.05f);
			float randSaturation = UnityEngine.Random.Range(-0.05f, 0.05f);
			float randValue = UnityEngine.Random.Range(-0.2f, 0.2f);
			
			float newHue = hue + randHue;
			while(newHue < 0.0f)
			{
				newHue += 1.0f;
			}

			while (newHue > 1.0f)
			{
				newHue -= 1.0f;
			}

			float newSaturation = Mathf.Clamp(sat + randSaturation, minSaturation, maxSaturation);
			float newValue = Mathf.Clamp(value + randValue, minValue, maxValue);

			Color resultColor = Color.HSVToRGB(newHue, newSaturation, newValue);
			resultColor.a = 1.0f;
			return resultColor;
		}

		/// <summary>Saturation과 Value는 비슷하지만 Hue가 차이가 많은 색상을 리턴한다.</summary>
		public static Color GetDiffierentColor(Color srcColor, float minSaturation, float maxSaturation, float minValue, float maxValue)
		{
			float hue = 0.0f;
			float sat = 0.0f;
			float value = 0.0f;

			Color.RGBToHSV(srcColor, out hue, out sat, out value);
			
			//Hue의 기준점을 옮겨주자
			//최소 (0.16), 최대 300 (0.83) > Rotate
			float randHue = UnityEngine.Random.Range(0.16f, 0.83f);
			
			//Saturation은 비슷하게
			float randSaturation = UnityEngine.Random.Range(-0.05f, 0.05f);
			float randValue = 0.0f;
			if(value < 0.2f)
			{
				//너무 어두우면 > 밝은 방향으로 강제
				randValue = UnityEngine.Random.Range(0.3f, 0.7f);
			}
			else
			{
				//그 외에는 비슷한 명도로
				randValue = UnityEngine.Random.Range(-0.02f, 0.02f);
			}
			
			
			float newHue = hue + randHue;
			while(newHue > 1.0f)
			{
				newHue -= 1.0f;
			}
			while(newHue < 0.0f)
			{
				newHue += 1.0f;
			}

			float newSaturation = Mathf.Clamp(randSaturation + randSaturation, minSaturation, maxSaturation);
			float newValue = Mathf.Clamp(randValue + randValue, minValue, maxValue);

			Color resultColor = Color.HSVToRGB(newHue, newSaturation, newValue);
			resultColor.a = 1.0f;
			return resultColor;
		}

		/// <summary>
		/// GetDiffierentColor 함수의 변형. 두개의 SrcColor로 부터 다른 색상을 구한다. 기본적으로 SrcColor2는 Hue만 비교한다.
		/// </summary>
		/// <param name="srcColor1"></param>
		/// <param name="srcColor2"></param>
		/// <param name="minSaturation"></param>
		/// <param name="maxSaturation"></param>
		/// <param name="minValue"></param>
		/// <param name="maxValue"></param>
		/// <returns></returns>
		public static Color GetDiffierentColor(Color srcColor1, Color srcColor2, float minSaturation, float maxSaturation, float minValue, float maxValue)
		{
			float hue1 = 0.0f;
			float hue2 = 0.0f;
			float sat = 0.0f;
			float value = 0.0f;

			//사용하진 않음
			float sat2 = 0.0f;
			float value2 = 0.0f;

			Color.RGBToHSV(srcColor1, out hue1, out sat, out value);
			Color.RGBToHSV(srcColor2, out hue2, out sat2, out value2);
			
			
			
			//Saturation은 비슷하게
			float randSaturation = UnityEngine.Random.Range(-0.05f, 0.05f);
			float randValue = 0.0f;
			if(value < 0.2f)
			{
				//너무 어두우면 > 밝은 방향으로 강제
				randValue = UnityEngine.Random.Range(0.3f, 0.7f);
			}
			else
			{
				//그 외에는 비슷한 명도로
				randValue = UnityEngine.Random.Range(-0.02f, 0.02f);
			}
			
			//Hue의 기준점을 옮겨주자
			//최소 (0.16), 최대 300 (0.83) > Rotate
			float newHue = 0.0f;

			//SrcColor2 기준으로 랜덤 가능한 범위를 정하자.
			//영역은 무조건 2개로 나뉠 것
			if(hue2 < hue1)
			{
				hue2 += 1.0f;
			}

			//가능한 영역
			float hueArea_A = hue1 + 0.16f;
			float hueArea_B = hue1 + 0.83f;

			//불가능한 영역 (조금 좁다)
			float hueLimit_A = hue2 - 0.1f;
			float hueLimit_B = hue2 + 0.1f;

			//랜덤 가능한 영역
			float hueRandArea_A = 0.0f;
			float hueRandArea_B = 0.0f;
			float hueRandArea_A2 = 0.0f;
			float hueRandArea_B2 = 0.0f;
			bool isCheck2Area = false;

			if (hueLimit_B < hueArea_A || hueLimit_A > hueArea_B)
			{
				//제한 영역이 완전히 밖으로 나갈 때
				hueRandArea_A = hueArea_A;
				hueRandArea_B = hueArea_B;
			}
			else if (hueLimit_A < hueArea_A && hueArea_A < hueLimit_B)
			{
				//A쪽에 제한 영역이 줄어들었을 때
				hueRandArea_A = hueLimit_B;
				hueRandArea_B = hueArea_B;
			}
			else if (hueLimit_A < hueArea_B && hueArea_B < hueLimit_B)
			{
				//B쪽에 제한 영역이 줄어들었을 때
				hueRandArea_A = hueArea_A;
				hueRandArea_B = hueLimit_A;
			}
			else if (hueArea_A < hueLimit_A && hueLimit_B < hueArea_B)
			{
				//A와 B 안쪽에 들어왔을 때 > 영역이 2개
				hueRandArea_A = hueArea_A;
				hueRandArea_B = hueLimit_A;

				hueRandArea_A2 = hueLimit_B;
				hueRandArea_B2 = hueArea_B;

				isCheck2Area = true;

				if (hueRandArea_B2 < hueRandArea_A2)
				{
					hueRandArea_B2 = hueRandArea_A2 + 0.1f;
				}
			}
			else
			{
				//그 외의 경우
				hueRandArea_A = hueArea_A;
				hueRandArea_B = hueArea_B;
			}
			if(hueRandArea_B < hueRandArea_A)
			{
				hueRandArea_B = hueRandArea_A + 0.1f;
			}
			
			if(isCheck2Area)
			{
				//랜덤 돌릴 영역이 2개일 때 > 50% 확률로 결정
				if(UnityEngine.Random.Range(0, 10) < 5)
				{
					newHue = UnityEngine.Random.Range(hueRandArea_A, hueRandArea_B);
				}
				else
				{
					newHue = UnityEngine.Random.Range(hueRandArea_A2, hueRandArea_B2);
				}
			}
			else
			{
				newHue = UnityEngine.Random.Range(hueRandArea_A, hueRandArea_B);
			}
			while(newHue > 1.0f)
			{
				newHue -= 1.0f;
			}
			while(newHue < 0.0f)
			{
				newHue += 1.0f;
			}

			float newSaturation = Mathf.Clamp(randSaturation + randSaturation, minSaturation, maxSaturation);
			float newValue = Mathf.Clamp(randValue + randValue, minValue, maxValue);

			Color resultColor = Color.HSVToRGB(newHue, newSaturation, newValue);
			resultColor.a = 1.0f;
			return resultColor;
		}

		/// <summary>HSV 방식에서 Saturation(채도), Value(명도)의 범위만 정하고 랜덤한 색상을 구하자.</summary>
		/// <returns></returns>
		public static Color GetRandomColor(float minSaturation, float maxSaturation, float minValue, float maxValue)
		{
			float randSaturation = Mathf.Clamp01(UnityEngine.Random.Range(minSaturation, maxSaturation));
			float randValue = Mathf.Clamp01(UnityEngine.Random.Range(minValue, maxValue));

			float randHue = UnityEngine.Random.Range(0.0f, 1.0f);

			Color resultColor = Color.HSVToRGB(randHue, randSaturation, randValue);
			resultColor.a = 1.0f;
			return resultColor;
		}


		//---------------------------------------------------------------------------------------
		//private static System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();
		//private static string _stopWatchMsg = "";
		//public static void StartCodePerformanceCheck(string stopWatchMsg)
		//{
		//	_stopWatchMsg = stopWatchMsg;
		//	_stopwatch.Reset();
		//	_stopwatch.Start();
		//}

		//public static void StopCodePerformanceCheck()
		//{
		//	_stopwatch.Stop();
		//	long mSec = _stopwatch.ElapsedMilliseconds;
		//	Debug.LogError("Performance [" + _stopWatchMsg + "] : " + (mSec / 1000) + "." + (mSec % 1000) + " secs");
		//	//return _stopwatch.ElapsedTicks + " Ticks";
		//}

		//-------------------------------------------------------------------------------------------
		/// <summary>
		/// "Assets/AnyPortrait/Editor/Materials/"
		/// </summary>
		/// <returns></returns>
		public static string ResourcePath_Material
		{
			get
			{
				return "Assets/AnyPortrait/Editor/Materials/";
			}
		}

		/// <summary>
		/// "Assets/AnyPortrait/Editor/Scripts/Util/"
		/// </summary>
		public static string ResourcePath_Text
		{
			get
			{
				return "Assets/AnyPortrait/Editor/Scripts/Util/";
			}
		}

		/// <summary>
		/// "AnyPortrait/Editor/Scripts/Util/"
		/// </summary>
		public static string ResourcePath_Text_WithoutAssets
		{
			get
			{
				return "AnyPortrait/Editor/Scripts/Util/";
			}
		}

		/// <summary>
		/// "Assets/AnyPortrait/Editor/Images/"
		/// </summary>
		public static string ResourcePath_Icon
		{
			get
			{
				return "Assets/AnyPortrait/Editor/Images/";
			}
		}


		public static bool IsInAssetsFolder(string folderPath)
		{
			System.IO.DirectoryInfo targetDirInfo = new System.IO.DirectoryInfo(folderPath);
			System.IO.DirectoryInfo assetsDirInfo = new System.IO.DirectoryInfo(Application.dataPath);

			if(targetDirInfo == null || !targetDirInfo.Exists)
			{
				return false;
			}
			if(assetsDirInfo == null || !assetsDirInfo.Exists)
			{
				return false;
			}
			//target의 parent->parent...->parent가 assetDirInfo인지 체크
			System.IO.DirectoryInfo curDir = targetDirInfo;
			while(true)
			{
				if(string.Equals(curDir.FullName, assetsDirInfo.FullName))
				{
					return true;
				}

				curDir = curDir.Parent;
				if(curDir.Parent == null || !curDir.Exists)
				{
					return false;
				}
			}
		}

		//--------------------------------------------------------------------------------
		public static int GetAspectRatio_Height(int srcWidth, int targetWidth, int targetHeight)
		{
			float targetAspectRatio = (float)targetWidth / (float)targetHeight;
			//Aspect = W / H
			//W = H * Aspect
			//H = W / Aspect <<

			return (int)(((float)srcWidth / targetAspectRatio) + 0.5f);
		}

		public static int GetAspectRatio_Width(int srcHeight, int targetWidth, int targetHeight)
		{
			float targetAspectRatio = (float)targetWidth / (float)targetHeight;
			//Aspect = W / H
			//W = H * Aspect <<
			//H = W / Aspect

			return (int)(((float)srcHeight * targetAspectRatio) + 0.5f);
		}

		//--------------------------------------------------------------------------------------
		public delegate void FUNC_CHECK_CURRENT_VERSION(bool isSuccess, string currentVersion);
		private static FUNC_CHECK_CURRENT_VERSION _funcCheckCurrentVersion = null;
		private static IEnumerator _coroutine = null;
		private static System.Diagnostics.Stopwatch _coroutineStopWatch = null;
		
		public static void RequestCurrentVersion(FUNC_CHECK_CURRENT_VERSION funcCurrentVersion)
		{
			if (funcCurrentVersion == null)
			{
				return;
			}
			_funcCheckCurrentVersion = funcCurrentVersion;
			_coroutine = Crt_RequestCurrentVersion();
			_coroutineStopWatch = null;

			EditorApplication.update -= ExecuteCoroutine;
			EditorApplication.update += ExecuteCoroutine;
		}


		private static void ExecuteCoroutine()
		{
			if(_coroutine == null)
			{
				//Debug.Log("ExecuteCoroutine => End");
				EditorApplication.update -= ExecuteCoroutine;
				return;
			}

			//Debug.Log("Update Coroutine");
			bool isResult = _coroutine.MoveNext();
			
			if(!isResult)
			{
				_coroutine = null;
				//Debug.Log("ExecuteCoroutine => End");
				EditorApplication.update -= ExecuteCoroutine;
				return;
			}

		}
		//private static void StartCheck()
		//{
		//	if(_coroutineStopWatch == null)
		//	{
		//		_coroutineStopWatch = new System.Diagnostics.Stopwatch();
		//	}
		//	_coroutineStopWatch.Stop();
		//	_coroutineStopWatch.Reset();
		//	_coroutineStopWatch.Start();
		//}
		private static bool CheckWaitTime(float time)
		{
			if(_coroutineStopWatch == null)
			{
				//새로 체크한다 => 타이머 시작
				_coroutineStopWatch = new System.Diagnostics.Stopwatch();
				_coroutineStopWatch.Stop();
				_coroutineStopWatch.Reset();
				_coroutineStopWatch.Start();
				return false;
			}
			//타이머가 가동중이라면 => 시간 체크 => 지정된 시간을 넘은 경우 리턴 True 및 타이머 삭제
			if(_coroutineStopWatch.Elapsed.TotalSeconds > time)
			{
				
				_coroutineStopWatch.Stop();
				_coroutineStopWatch = null;
				return true;
			}
			return false;
		}

		private static IEnumerator Crt_RequestCurrentVersion()
		{
			
			string url = "https://homepi12.wixsite.com/referencedata/anyportrait";

			

			//<< 유니티 2018.3 관련 API 분기 >>
#if UNITY_2018_3_OR_NEWER
			UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(url);
#else
			WWW www = new WWW(url);
#endif
			//Debug.Log("Start Request");

#if UNITY_2018_3_OR_NEWER
			yield return www.SendWebRequest();
#else
			yield return www;
#endif

			float totalTime = 0.0f;//<<전체 시간이 오래 걸리면 포기
			
			while(true)
			{

				//Debug.Log("Progress : " + www.progress + " (" + totalTime + ")");
				//<< 유니티 2018.3 관련 API 분기 >>
#if UNITY_2018_3_OR_NEWER
				if(www.isDone || www.downloadProgress >= 1.0f)
#else
				if(www.isDone || www.progress >= 1.0f)
#endif
				
				{
					//Debug.Log("Progress >> Completed : " + www.progress + " (" + totalTime + ")");
					break;
				}

				//yield return new WaitForSeconds(1.0f);//<<이게 작동을 안한다.
				if (CheckWaitTime(1.0f))
				{
					totalTime += 1.0f;
					//Debug.Log("Progress : " + www.progress + " (" + totalTime + ")");
					if (totalTime > 20.0f)
					{
						yield break;
					}
				}
				else
				{
					yield return new WaitForEndOfFrame();
				}
			}

			if (!CheckWaitTime(2.0f))
			{
				//2초 대기
				yield return new WaitForEndOfFrame();	
			}
			//yield return new WaitForSeconds(2.0f);//실제

			//실제:
			//주석 해제할 것
			totalTime = 0.0f;
			while (true)
			{
				//Is Done 한번 더 체크
				if (www.isDone)
				{
					break;
				}
				if (CheckWaitTime(1.0f))
				{
					totalTime += 1.0f;
					//Debug.Log("Progress : " + www.progress + " (" + totalTime + ")");
					if (totalTime > 20.0f)
					{
						yield break;
					}
				}
				else
				{
					yield return new WaitForEndOfFrame();
				}
			}


			if (!www.isDone)
			{
				//처리가 되지 않았다.
				//Debug.LogError("Request > Not Downloading : " + www.progress + " / " + www.isDone);
				yield break;
			}

			//Debug.Log("Request > Finished : " + www.progress + " / " + www.isDone);

			if(_funcCheckCurrentVersion == null)
			{
				yield break;
			}
			try
			{	

				if (string.IsNullOrEmpty(www.error))
				{
					//성공
					//"[AnyPortrait-CurrentVersion]:[1.0.3]"
					string strKey = "[AnyPortrait-CurrentVersion]";

					//<< 유니티 2018.3 관련 API 분기 >>
#if UNITY_2018_3_OR_NEWER
					string downloadedText = www.downloadHandler.text;
#else
					string downloadedText = www.text;
#endif
					if(downloadedText.Contains(strKey))
					{
						//Debug.LogWarning("Result\n" + www.text);
						int textLength = downloadedText.Length;
						int iStart = downloadedText.IndexOf(strKey);
						int sampleLength = strKey.Length + 20;
						if(iStart + sampleLength > textLength)
						{
							sampleLength = textLength - iStart;
						}
						
						string strSubText = downloadedText.Substring(iStart, sampleLength);
						//Debug.Log("Sub String : " + strSubText + "(" +iStart + ":" + sampleLength +")");

						System.Text.StringBuilder result = new System.Text.StringBuilder();
						string curText = "";
						for (int i = strKey.Length; i < strSubText.Length; i++)
						{
							curText = strSubText.Substring(i, 1);
							if(curText == "]")
							{
								break;
							}
							if(curText == "0" || curText == "1" || curText == "2" || curText == "3"
								|| curText == "4" || curText == "5" || curText == "6" || curText == "7"
								|| curText == "8" || curText == "9" || curText == ".")
							{
								result.Append(curText);
							}
						}

						_funcCheckCurrentVersion(true, result.ToString());
						yield break;
					}
					//실패
					_funcCheckCurrentVersion(false, "Parsing Failed");
					yield break;
				}

				_funcCheckCurrentVersion(false, www.error);
			}
			catch(Exception ex)
			{
				//에러
				_funcCheckCurrentVersion(false, ex.ToString());
				//Debug.LogError("Exception : " + ex);
			}
		}

		//------------------------------------------------------------------------------
		/// <summary>
		/// AssetStore에서 AnyPortrait 페이지를 엽니다.
		/// </summary>
		public static void OpenAssetStorePage()
		{
			UnityEditorInternal.AssetStore.Open("content/111584");
		}



		// 절대 경로 <-> 상대 경로 (Asset에 대하여)
		//-------------------------------------------------------------------------------------------
		public static string GetProjectAssetPath()
		{
			return Application.dataPath;
		}

		

		public enum PATH_INFO_TYPE
		{
			//유효하지 않다
			//절대 경로이며 Asset 폴더 밖에 있다.
			//절대 경로이며 Asset 폴더 안에 있다.
			//상대 경로이며 Asset 폴더 밖에 있다.
			//상대 경로이며 Asset 폴더 안에 있다.
			NotValid,
			Absolute_OutAssetFolder,
			Absolute_InAssetFolder,
			Relative_OutAssetFolder,
			Relative_InAssetFolder,

		}
		public static PATH_INFO_TYPE GetPathInfo(string path)
		{
			if(string.IsNullOrEmpty(path))
			{
				return PATH_INFO_TYPE.NotValid;
			}

			path = path.Replace("\\", "/");
			//Debug.Log("GetPathInfo : " + path);

			//1. path가 Relative인지 확인
			System.IO.DirectoryInfo di_AssetFolder = new System.IO.DirectoryInfo(Application.dataPath);

			System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path);

			//System.Uri uri_target = new Uri(path);
			
			
			//Debug.Log("Asset Path : " + di_AssetFolder.FullName);
			//Debug.Log("Target Path : " + di.FullName);

			if(di.Exists && !path.StartsWith("Assets"))
			{
				//유효한 경로이다 >> 절대 경로이다.
				//Asset 폴더 안쪽에 있는지 확인
				if(di.FullName.StartsWith(di_AssetFolder.FullName))
				{
					//Asset Folder의 경로로 부터 시작한다면
					//>> 절대 경로 + Asset 폴더 안쪽
					//Debug.Log(">>> 절대 경로 + Asset 폴더 안쪽");
					return PATH_INFO_TYPE.Absolute_InAssetFolder;
				}
				else
				{
					//>> 절대 경로 + Asset 폴더 바깥쪽
					//Debug.Log(">>> 절대 경로 + Asset 폴더 바깥쪽");
					return PATH_INFO_TYPE.Absolute_OutAssetFolder;
				}
				
			}
			else
			{
				//유효하지 않다면 상대 경로일 수 있다.
				//string checkPath = Application.dataPath + "/" + path;
				//Debug.Log("Check Path : " + checkPath);
				string projectPath = Application.dataPath;
				projectPath = projectPath.Substring(0, projectPath.Length - 6);//"Assets 글자를 뒤에서 뺀다"

				string fullPath = projectPath + path;
				//Debug.Log("FullPath : " + fullPath);
				//다시 체크
				di = new System.IO.DirectoryInfo(fullPath);
				if(di.Exists)
				{
					//Asset 폴더를 기준으로 하는 상대 경로가 맞다.
					//안쪽에 있는지 확인
					bool isInAssetFoler = IsInFolder(di_AssetFolder.FullName, di.FullName);
					if(isInAssetFoler)
					{
						//>> 상대 경로 + Asset 폴더 안쪽
						//Debug.Log(">>> 상대 경로 + Asset 폴더 안쪽");
						return PATH_INFO_TYPE.Relative_InAssetFolder;
					}
					else
					{
						//>> 상대 경로 + Asset 폴더 바깥쪽
						//Debug.Log(">>> 상대 경로 + Asset 폴더 바깥쪽");
						return PATH_INFO_TYPE.Relative_OutAssetFolder;
					}
				}
				else
				{
					//그냥 잘못된 경로네요..
					
				}

			}
			
			//Debug.Log(">>> 잘못된 경로");
			return PATH_INFO_TYPE.NotValid;
		}

		private static bool IsInFolder(string parentPath, string childPath)
		{
			System.IO.DirectoryInfo di_Parent = new System.IO.DirectoryInfo(parentPath);
			System.IO.DirectoryInfo di_Child = new System.IO.DirectoryInfo(childPath);
			if(!di_Parent.Exists || !di_Child.Exists)
			{
				return false;
			}

			string parentPath_Lower = di_Parent.FullName.ToLower();
			string rootPath_Lower = di_Child.Root.FullName.ToLower();
			System.IO.DirectoryInfo di_Cur = new System.IO.DirectoryInfo(childPath);

			string curPath_Lower = "";
			while (true)
			{
				curPath_Lower = di_Cur.FullName.ToLower();
				//Root에 도달했으면 종료
				if(curPath_Lower.Equals(rootPath_Lower))
				{
					break;
				}

				if (curPath_Lower.Equals(parentPath_Lower))
				{
					//Parent Path가 나왔다.
					return true;
				}



				try
				{
					//위 폴더로 이동해보자
					di_Cur = di_Cur.Parent;
					if(!di_Cur.Exists)
					{
						break;
					}
				}
				catch(Exception)
				{
					//에러 발생
					break;
				}
			}

			return false;
		}


		public static string AbsolutePath2RelativePath(string absPath)
		{
			Uri uri_Asset = new Uri(Application.dataPath);
			Uri uri_AbsPath = new Uri(absPath);

			Uri uri_Relative = uri_Asset.MakeRelativeUri(uri_AbsPath);
			
			string resultPath = uri_Relative.ToString();

			//Debug.LogError("Abs > Rel : " + resultPath);
			resultPath = resultPath.Replace("\\", "/");

			
			//Debug.Log("Prev : [" + resultPath + "]");
			resultPath = DecodeURLEmptyWord(resultPath);
			//Debug.LogError(">>> " + resultPath);
			//Debug.Log("Next : [" + resultPath + "]");
			return resultPath;
		}

		public static string DecodeURLEmptyWord(string urlPath)
		{
			//return urlPath;
			return urlPath.Replace("%20", " ");
		}
		//-------------------------------------------------------------------------------------------
	}
}