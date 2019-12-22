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

	public class apEditorController
	{
		// Member
		//--------------------------------------------------
		private apEditor _editor = null;
		public apEditor Editor { get { return _editor; } }

		// Init
		//--------------------------------------------------
		public apEditorController()
		{

		}

		public void SetEditor(apEditor editor)
		{
			_editor = editor;

		}


		// Functions
		//--------------------------------------------------
		// 1. 입력 관련한 함수들
		//--------------------------------------------------
		public void CheckInputEvent()
		{
			//bool isMouseEvent = (Event.current.type == EventType.ScrollWheel || Event.current.isMouse);



			if (Event.current.rawType == EventType.Used)
			{
				return;
			}


			//주의 : _mouseBtn => _mouseSet (Mouse)로 변경되었다.
			//모든 변경 점에 주석을 단다.


			//Event.current.rawType으로 해야 Editor 외부에서의 MouseUp 이벤트를 가져올 수 있다.
			bool isMouseEvent = Event.current.rawType == EventType.ScrollWheel ||
				Event.current.rawType == EventType.MouseDown ||
				Event.current.rawType == EventType.MouseDrag ||
				Event.current.rawType == EventType.MouseMove ||
				Event.current.rawType == EventType.MouseUp;

			Vector2 mousePos = Vector2.zero;
			if (isMouseEvent || Event.current.type == EventType.Repaint)
			{
				mousePos = Event.current.mousePosition - new Vector2(Editor._mainGUIRect.x, Editor._mainGUIRect.y);
				//apMouse.SetMousePos(mousePos, Event.current.mousePosition);//<<이전
				Editor.Mouse.SetMousePos(mousePos, Event.current.mousePosition);//>>변경
			}

			if (isMouseEvent)
			{
				//Vector2 mousePos = Event.current.mousePosition - new Vector2(Editor._mainGUIRect.x, Editor._mainGUIRect.y);
				//apMouse.SetMousePos(mousePos, Event.current.mousePosition);

				//<<이전
				//for (int i = 0; i < 4; i++)
				//{
				//	Editor._mouseBtn[i].ReadyToUpdate();
				//}
				Editor.Mouse.ReadyToUpdate();//>>변경

				if (Event.current.rawType == EventType.ScrollWheel)
				{
					Vector2 deltaValue = Event.current.delta;
					//Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].Update_Wheel((int)(deltaValue.y * 10.0f));//<<이전
					Editor.Mouse.Update_Wheel((int)(deltaValue.y * 10.0f));//>>이후
				}
				else//if (Event.current.isMouse)
				{
					int iMouse = -1;
					switch (Event.current.button)
					{
						case 0://Left
							iMouse = 0;
							break;

						case 1://Right
							iMouse = 1;
							break;

						case 2://Middle
							iMouse = 2;
							break;
					}
					if (iMouse >= 0)
					{


						//>>변경
						bool isMouseInGUI = false;
						if (Event.current.rawType == EventType.MouseDown)
						{
							//Down 이벤트일때만 영역 체크
							isMouseInGUI = IsMouseInGUI(mousePos);
						}
						Editor.Mouse.Update_Button(Event.current.rawType, iMouse, isMouseInGUI);

					}
				}
			}

			if (Event.current.rawType == EventType.KeyDown)
			{
#if UNITY_EDITOR_OSX
				Editor.OnHotKeyDown(Event.current.keyCode, Event.current.command, Event.current.alt, Event.current.shift);
#else
				Editor.OnHotKeyDown(Event.current.keyCode, Event.current.control, Event.current.alt, Event.current.shift);
#endif
			}
			else if (Event.current.rawType == EventType.KeyUp)
			{
				Editor.OnHotKeyUp();
			}
		}

		public bool IsMouseInGUI(Vector2 mousePos)
		{
			if (mousePos.x < 0 || mousePos.x > Editor._mainGUIRect.width)
			{
				return false;
			}

			if (mousePos.y < 0 || mousePos.y > Editor._mainGUIRect.height)
			{
				return false;
			}
			return true;
		}

		public void GUI_Input_CheckClickInCenter()
		{
			//이전
			//apMouse.MouseBtnStatus leftBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Status;
			//apMouse.MouseBtnStatus rightBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Status;
			//apMouse.MouseBtnStatus middleBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].Status;

			apMouse.MouseBtnStatus leftBtnStatus = Editor.Mouse.GetStatusWithoutActionID(apMouseSet.Button.Left);
			apMouse.MouseBtnStatus rightBtnStatus = Editor.Mouse.GetStatusWithoutActionID(apMouseSet.Button.Right);
			apMouse.MouseBtnStatus middleBtnStatus = Editor.Mouse.GetStatusWithoutActionID(apMouseSet.Button.Middle);


			if (leftBtnStatus == apMouse.MouseBtnStatus.Down
				|| rightBtnStatus == apMouse.MouseBtnStatus.Down
				|| middleBtnStatus == apMouse.MouseBtnStatus.Down)
			{
				if (IsMouseInGUI(Editor.Mouse.Pos))
				{
					apEditorUtil.ReleaseGUIFocus();
				}
			}


		}

		public bool GUI_Input_ZoomAndScroll(bool isCtrl, bool isShift, bool isAlt)
		{
			//if (Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].Wheel != 0)//<<이전
			if (Editor.Mouse.Wheel != 0)//>>변경
			{
				//if (IsMouseInGUI(apMouse.PosLast))//<<이전
				if (IsMouseInGUI(Editor.Mouse.PosLast))//>>변경
				{
					//현재 위치에서 마우스의 World 좌표를 구한다.
					//Vector2 mouseW_Relative = apGL.GL2World(apMouse.PosLast);
					float zoomPrev = Editor._zoomListX100[Editor._iZoomX100] * 0.01f;


					//if (Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].Wheel > 0)//<<이전
					if (Editor.Mouse.Wheel > 0)//>>이후
					{
						//줌 아웃 = 인덱스 감소
						Editor._iZoomX100--;
						if (Editor._iZoomX100 < 0) { Editor._iZoomX100 = 0; }
					}
					//else if (Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].Wheel < 0)
					else if (Editor.Mouse.Wheel < 0)//>>이후
					{
						//줌 인 = 인덱스 증가
						Editor._iZoomX100++;
						if (Editor._iZoomX100 >= Editor._zoomListX100.Length)
						{
							Editor._iZoomX100 = Editor._zoomListX100.Length - 1;
						}
					}
					//마우스의 World 좌표는 같아야 한다.
					float zoomNext = Editor._zoomListX100[Editor._iZoomX100] * 0.01f;

					//중심점의 위치를 구하자 (Editor GL 기준)
					Vector2 scroll = new Vector2(Editor._scroll_MainCenter.x * 0.1f * apGL.WindowSize.x,
													Editor._scroll_MainCenter.y * 0.1f * apGL.WindowSize.y);
					Vector2 guiCenterPos = apGL.WindowSizeHalf - scroll;

					//Vector2 deltaMousePos = apMouse.PosLast - guiCenterPos;//<<이전
					Vector2 deltaMousePos = Editor.Mouse.PosLast - guiCenterPos;//>>이후
					Vector2 nextDeltaMousePos = deltaMousePos * (zoomNext / zoomPrev);

					//마우스를 기준으로 확대/축소를 할 수 있도록 줌 상태에 따라서 Scroll을 자동으로 조정하자
					//Delta = Mouse - GUICenter
					//GUICenter = Mouse - Delta
					//WindowSizeHalf - Scroll = Mouse - Delta
					//Scroll - WindowSizeHalf = Delta - Mouse
					//Scroll = (Delta - Mouse) + WindowSizeHalf
					//ScrollCenter * 0.1f * SizeXY = (Delta - Mouse) + WindowSizeHalf
					//ScrollCenter = ((Delta - Mouse) + WindowSizeHalf) / (0.1f * SizeXY)
					//>>이전
					//float nextScrollX = ((nextDeltaMousePos.x - apMouse.PosLast.x) + apGL.WindowSizeHalf.x) / (0.1f * apGL.WindowSize.x);
					//float nextScrollY = ((nextDeltaMousePos.y - apMouse.PosLast.y) + apGL.WindowSizeHalf.y) / (0.1f * apGL.WindowSize.y);

					//>>변셩
					float nextScrollX = ((nextDeltaMousePos.x - Editor.Mouse.PosLast.x) + apGL.WindowSizeHalf.x) / (0.1f * apGL.WindowSize.x);
					float nextScrollY = ((nextDeltaMousePos.y - Editor.Mouse.PosLast.y) + apGL.WindowSizeHalf.y) / (0.1f * apGL.WindowSize.y);

					nextScrollX = Mathf.Clamp(nextScrollX, -500.0f, 500.0f);
					nextScrollY = Mathf.Clamp(nextScrollY, -500.0f, 500.0f);

					Editor._scroll_MainCenter.x = nextScrollX;
					Editor._scroll_MainCenter.y = nextScrollY;

					Editor.SetRepaint();

					//Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].UseWheel();//<이전
					Editor.Mouse.UseWheel();//변경


					return true;
				}
			}

			//이전
			//if (Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].Status == apMouse.MouseBtnStatus.Down ||
			//	Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].Status == apMouse.MouseBtnStatus.Pressed)
			if (Editor.Mouse.IsPressed(apMouseSet.Button.Middle, apMouseSet.ACTION.ScreenMove_MidBtn))//변경
			{
				//if (IsMouseInGUI(apMouse.PosLast))//이전
				if (IsMouseInGUI(Editor.Mouse.PosLast))//변경
				{
					//Vector2 moveDelta = Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].PosDelta;//이전
					Vector2 moveDelta = Editor.Mouse.GetPosDelta(apMouseSet.Button.Middle);//이후
																						   //RealX = scroll * windowWidth * 0.1

					Vector2 sensative = new Vector2(
						1.0f / (Editor._mainGUIRect.width * 0.1f),
						1.0f / (Editor._mainGUIRect.height * 0.1f));

					Editor._scroll_MainCenter.x -= moveDelta.x * sensative.x;
					Editor._scroll_MainCenter.y -= moveDelta.y * sensative.y;

					Editor.SetRepaint();

					//Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].UseMouseDrag();//이전
					Editor.Mouse.UseMouseDrag(apMouseSet.Button.Middle, apMouseSet.ACTION.ScreenMove_MidBtn);//이후

					//추가 : Pan 커서 모양이 나타나도록 하자
					apGL.AddCursorRectDelayed(Editor.Mouse.PosLast, MouseCursor.Pan);
					return true;
				}
			}

			//추가 : Ctrl + Alt를 누르면 
			//- Left Button이 Middle Button의 역할을 한다.
			//- Right Button이 Wheel 역할을 한다.
			if (isCtrl && isAlt)
			{
				if (Editor.Mouse.IsPressed(apMouseSet.Button.Left, apMouseSet.ACTION.ScreenMove_MidBtn))//변경
				{
					if (IsMouseInGUI(Editor.Mouse.PosLast))//변경
					{
						Vector2 moveDelta = Editor.Mouse.GetPosDelta(apMouseSet.Button.Left);//이후

						Vector2 sensative = new Vector2(
							1.0f / (Editor._mainGUIRect.width * 0.1f),
							1.0f / (Editor._mainGUIRect.height * 0.1f));

						Editor._scroll_MainCenter.x -= moveDelta.x * sensative.x;
						Editor._scroll_MainCenter.y -= moveDelta.y * sensative.y;

						Editor.SetRepaint();

						//Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].UseMouseDrag();//이전
						Editor.Mouse.UseMouseDrag(apMouseSet.Button.Left, apMouseSet.ACTION.ScreenMove_MidBtn);//이후

						//추가 : Pan 커서 모양이 나타나도록 하자
						apGL.AddCursorRectDelayed(Editor.Mouse.PosLast, MouseCursor.Pan);

						return true;
					}
				}

				if (Editor.Mouse.IsPressed(apMouseSet.Button.Right, apMouseSet.ACTION.ScreenMove_MidBtn))//변경
				{
					if (IsMouseInGUI(Editor.Mouse.PosLast))//변경
					{
						Vector2 moveDelta = Editor.Mouse.GetPosDelta(apMouseSet.Button.Right);//이후

						float wheelOffset = 0.0f;
						if (Mathf.Abs(moveDelta.x) * 1.5f > Mathf.Abs(moveDelta.y))
						{
							wheelOffset = moveDelta.x;
						}
						else
						{
							wheelOffset = moveDelta.y;
						}

						//현재 위치에서 마우스의 World 좌표를 구한다.
						//Vector2 mouseW_Relative = apGL.GL2World(apMouse.PosLast);
						float zoomPrev = Editor._zoomListX100[Editor._iZoomX100] * 0.01f;

						if (wheelOffset < -1.3f)
						{
							//줌 아웃 = 인덱스 감소
							Editor._iZoomX100--;
							if (Editor._iZoomX100 < 0) { Editor._iZoomX100 = 0; }
						}
						else if (wheelOffset > 1.3f)
						{
							//줌 인 = 인덱스 증가
							Editor._iZoomX100++;
							if (Editor._iZoomX100 >= Editor._zoomListX100.Length)
							{
								Editor._iZoomX100 = Editor._zoomListX100.Length - 1;
							}
						}
						//마우스의 World 좌표는 같아야 한다.
						float zoomNext = Editor._zoomListX100[Editor._iZoomX100] * 0.01f;

						//중심점의 위치를 구하자 (Editor GL 기준)
						Vector2 scroll = new Vector2(Editor._scroll_MainCenter.x * 0.1f * apGL.WindowSize.x,
														Editor._scroll_MainCenter.y * 0.1f * apGL.WindowSize.y);
						Vector2 guiCenterPos = apGL.WindowSizeHalf - scroll;

						Vector2 deltaMousePos = Editor.Mouse.PosLast - guiCenterPos;//>>이후
						Vector2 nextDeltaMousePos = deltaMousePos * (zoomNext / zoomPrev);

						//마우스를 기준으로 확대/축소를 할 수 있도록 줌 상태에 따라서 Scroll을 자동으로 조정하자

						//>>변경
						float nextScrollX = ((nextDeltaMousePos.x - Editor.Mouse.PosLast.x) + apGL.WindowSizeHalf.x) / (0.1f * apGL.WindowSize.x);
						float nextScrollY = ((nextDeltaMousePos.y - Editor.Mouse.PosLast.y) + apGL.WindowSizeHalf.y) / (0.1f * apGL.WindowSize.y);

						nextScrollX = Mathf.Clamp(nextScrollX, -500.0f, 500.0f);
						nextScrollY = Mathf.Clamp(nextScrollY, -500.0f, 500.0f);

						Editor._scroll_MainCenter.x = nextScrollX;
						Editor._scroll_MainCenter.y = nextScrollY;

						Editor.SetRepaint();


						//Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].UseMouseDrag();//이전
						Editor.Mouse.UseMouseDrag(apMouseSet.Button.Right, apMouseSet.ACTION.ScreenMove_MidBtn);//이후

						//추가 : Zoom 커서 모양이 나타나도록 하자
						apGL.AddCursorRectDelayed(Editor.Mouse.PosLast, MouseCursor.Zoom);

						return true;



					}
				}
			}

			return false;
		}


		public void GUI_Input_Modify(float tDelta)
		{
			//이전
			//apMouse.MouseBtnStatus leftBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Status;
			//apMouse.MouseBtnStatus rightBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Status;
			//Vector2 mousePos = apMouse.Pos;

			//이후
			apMouse.MouseBtnStatus leftBtnStatus = Editor.Mouse.GetStatus(apMouseSet.Button.Left, apMouseSet.ACTION.MeshEdit_Modify);
			apMouse.MouseBtnStatus rightBtnStatus = Editor.Mouse.GetStatus(apMouseSet.Button.Right, apMouseSet.ACTION.MeshEdit_Modify);
			Vector2 mousePos = Editor.Mouse.Pos;

#if UNITY_EDITOR_OSX
			bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif


			Editor.Gizmos.Update(tDelta, leftBtnStatus, rightBtnStatus, mousePos, isCtrl, Event.current.shift, Event.current.alt);


		}


		public void GUI_Input_MakeMesh_AutoGen_TRS(float tDelta)
		{
			apMouse.MouseBtnStatus leftBtnStatus = Editor.Mouse.GetStatus(apMouseSet.Button.Left, apMouseSet.ACTION.MeshEdit_AutoGen);
			apMouse.MouseBtnStatus rightBtnStatus = Editor.Mouse.GetStatus(apMouseSet.Button.Right, apMouseSet.ACTION.MeshEdit_AutoGen);
			Vector2 mousePos = Editor.Mouse.Pos;

#if UNITY_EDITOR_OSX
			bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			Editor.Gizmos.Update(tDelta, leftBtnStatus, rightBtnStatus, mousePos, isCtrl, Event.current.shift, Event.current.alt);
		}


		// Make Mesh의 GUI Input 함수
		private bool _isAnyVertexMoved = false;
		private bool _isHiddenEdgeTurnable = false;
		private bool _isMeshVertMovable = false;//< Vertex를 이동할 수 있는 조건 (1. null -> 새로 클릭 / 2. 기존꺼 다시 클릭) ~ 불가 조건 (기존꺼 -> 다른거 클릭)

		public void GUI_Input_MakeMesh(apEditor.MESH_EDIT_MODE_MAKEMESH makeMeshMode)
		{
			if (Event.current.type == EventType.Used)
			{
				return;
			}

			apMouse.MouseBtnStatus leftBtnStatus = Editor.Mouse.GetStatus(apMouseSet.Button.Left, apMouseSet.ACTION.MeshEdit_Make);
			apMouse.MouseBtnStatus rightBtnStatus = Editor.Mouse.GetStatus(apMouseSet.Button.Right, apMouseSet.ACTION.MeshEdit_Make);
			Vector2 mousePos = Editor.Mouse.Pos;

			bool isShift = Event.current.shift;

#if UNITY_EDITOR_OSX
			bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			//추가
			//Ctrl을 누르면 Vertex를 선택하는게 "범위 이내"에서 "가장 가까운"으로 바뀐다. (Snap)
			//Shift를 누르면 1개가 아닌 여러개의 충돌 점을 검색하고, Edge를 만들때 아예 충돌점에 Vertex를 추가하며 강제로 만든다.


			bool isNearestVertexCheckable = false;


			bool isVertEdgeRemovalble = false;

			if (Editor.VertController.Vertex == null)
			{
				isVertEdgeRemovalble = true;//<<이전에 선택한게 없으면 다음 선택시 삭제 가능
			}



			if (Editor.VertController.Mesh == null || Editor.Select.Mesh != Editor.VertController.Mesh)
			{
				if (Editor.Select.Mesh != null)
				{
					Editor.VertController.SetMesh(Editor.Select.Mesh);
				}
				else
				{
					return;
				}
			}


			if (Event.current.isMouse)
			{
				if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
				{
					_isMeshVertMovable = false;//일단 이동 불가


					if (IsMouseInGUI(mousePos))
					{
						if (Editor.Select.Mesh != null)
						{
							//추가: 일단 Mirror 설정 중 Move 값 초기화
							if (Editor._meshEditMirrorMode == apEditor.MESH_EDIT_MIRROR_MODE.Mirror)
							{
								Editor.MirrorSet.ClearMovedVertex();
							}


							if (makeMeshMode != apEditor.MESH_EDIT_MODE_MAKEMESH.Polygon)
							{
								apVertex prevSelectedVert = Editor.VertController.Vertex;

								if (prevSelectedVert == null)
								{
									_isMeshVertMovable = true;//새로 선택하면 -> 다음에 Vert 이동 가능 (1)
								}

								bool isAnySelect = false;
								bool isAnyAddVertOrMesh = false;
								//Ctrl을 누르는 경우 -> 가장 가까운 Vertex를 선택한다. (즉, Vertex 추가는 안된다.)
								if (isCtrl &&
									(makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH.VertexAndEdge || makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH.EdgeOnly))
								{
									apVertex nearestVert = null;
									float minDistToVert = 0.0f;
									for (int i = 0; i < Editor.Select.Mesh._vertexData.Count; i++)
									{
										apVertex vertex = Editor.Select.Mesh._vertexData[i];
										Vector2 vPos = new Vector2(vertex._pos.x, vertex._pos.y) - Editor.Select.Mesh._offsetPos;

										Vector2 posGL = apGL.World2GL(vPos);
										float distToMouse = Vector2.Distance(posGL, mousePos);
										if (nearestVert == null || distToMouse < minDistToVert)
										{
											nearestVert = vertex;
											minDistToVert = distToMouse;
										}
									}
									if (nearestVert != null)
									{
										//가장 가까운 Vert를 찾았다.
										if (prevSelectedVert == nearestVert)
										{
											//같은걸 선택했다.
											//이동 가능
											_isMeshVertMovable = true;

										}
										else
										{
											//추가 4.8 : Ctrl을 누르고 선택한 경우 이동 불가
											_isMeshVertMovable = false;
										}

										Editor.VertController.SelectVertex(nearestVert);
										isAnySelect = true;

										//추가 : 이전 버텍스에서 새로운 버텍스로 자동으로 Edge를 생성해주자
										if (makeMeshMode != apEditor.MESH_EDIT_MODE_MAKEMESH.VertexOnly)
										{
											if (nearestVert != prevSelectedVert
												&& prevSelectedVert != null
												&& nearestVert != null)
											{
												//Debug.Log("Ctrl");
												//Undo - Add Edge
												apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_AddEdge, Editor, Editor.Select.Mesh, null, false);

												Editor.Select.Mesh.MakeNewEdge(prevSelectedVert, nearestVert, isShift);
												isAnyAddVertOrMesh = true;

												//추가 9.12 : 미러 옵션이 있다면 반대쪽도 만들어야 한다.
												if (Editor._meshEditMirrorMode == apEditor.MESH_EDIT_MIRROR_MODE.Mirror)
												{
													Editor.MirrorSet.RefreshMeshWork(Editor.Select.Mesh, Editor.VertController);
													Editor.MirrorSet.AddMirrorVertex(prevSelectedVert, nearestVert, Editor.Select.Mesh, true, isShift, false, false);

												}
											}
										}
									}
								}
								else
								{
									for (int i = 0; i < Editor.Select.Mesh._vertexData.Count; i++)
									{
										apVertex vertex = Editor.Select.Mesh._vertexData[i];
										Vector2 vPos = new Vector2(vertex._pos.x, vertex._pos.y) - Editor.Select.Mesh._offsetPos;

										Vector2 posGL = apGL.World2GL(vPos);

										//어떤 버텍스를 선택했다.
										if (IsVertexClickable(posGL, mousePos))
										{
											if (prevSelectedVert == vertex)
											{
												//같은걸 선택했다.
												//이동 가능
												_isMeshVertMovable = true;
											}

											Editor.VertController.SelectVertex(vertex);
											isAnySelect = true;

											//추가 : 이전 버텍스에서 새로운 버텍스로 자동으로 Edge를 생성해주자
											if (makeMeshMode != apEditor.MESH_EDIT_MODE_MAKEMESH.VertexOnly)
											{
												if (Editor.VertController.Vertex != prevSelectedVert
													&& prevSelectedVert != null
													&& Editor.VertController.Vertex != null)
												{
													//Undo - Add Edge
													apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_AddEdge, Editor, Editor.Select.Mesh, null, false);

													Editor.Select.Mesh.MakeNewEdge(prevSelectedVert, Editor.VertController.Vertex, isShift);
													isAnyAddVertOrMesh = true;


													//추가 9.12 : 미러 옵션이 있다면 반대쪽도 만들어야 한다.
													if (Editor._meshEditMirrorMode == apEditor.MESH_EDIT_MIRROR_MODE.Mirror)
													{
														Editor.MirrorSet.RefreshMeshWork(Editor.Select.Mesh, Editor.VertController);
														Editor.MirrorSet.AddMirrorVertex(prevSelectedVert, Editor.VertController.Vertex, Editor.Select.Mesh, true, isShift, false, false);

													}
												}
											}

											break;
										}
									}
								}


								if (!isAnySelect)
								{
									Editor.VertController.UnselectVertex();

									//아무 버텍스를 선택하지 않았다.
									//새로 추가한다. => Vertex 모드일 때
									if (makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH.VertexOnly ||
										makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH.VertexAndEdge)
									{
										if (Editor.VertController.Vertex == null)
										{
											//Undo - Vertex 추가
											apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_AddVertex, Editor, Editor.Select.Mesh, null, false);

											// <<< 버텍스 추가 >>>
											Vector2 mouseWorld = apGL.GL2World(mousePos) + Editor.Select.Mesh._offsetPos;

											//변경 9.14 : Shift를 누른 상태에서 근처에 Edge가 있다면->그 Edge를 분할한다.
											apVertex addedVert = null;
											bool isAddVertexWithSplitEdge = false;
											//if(isShift && prevSelectedVert == null)
											if (isShift && !isCtrl)
											{
												//Debug.Log("새로운 버텍스 + Shift [" + makeMeshMode + "]");
												//Shift를 눌렀고, 이전에 선택된 Vert가 없다면..
												apMeshEdge nearestEdge = GetMeshNearestEdge(mousePos, Editor.Select.Mesh, 3.0f);//<<기본적인 5가 아닌 3이다. 제한적임
																																//Editor.Select.Mesh._edges[0]
												if (nearestEdge != null)
												{
													Vector2 splitPos = nearestEdge.GetNearestPosOnEdge(apGL.GL2World(mousePos) + Editor.Select.Mesh._offsetPos);
													if (Mathf.Abs(splitPos.x - nearestEdge._vert1._pos.x) < 1 && Mathf.Abs(splitPos.y - nearestEdge._vert1._pos.y) < 1)
													{
														//Vert1과 겹친다.
														addedVert = nearestEdge._vert1;
													}
													else if (Mathf.Abs(splitPos.x - nearestEdge._vert2._pos.x) < 1 && Mathf.Abs(splitPos.y - nearestEdge._vert2._pos.y) < 1)
													{
														//Vert2와 겹친다.
														addedVert = nearestEdge._vert2;
													}
													else
													{
														//겹치는게 없다.
														addedVert = Editor.Select.Mesh.SplitEdge(nearestEdge, splitPos);
														isAddVertexWithSplitEdge = true;
													}
												}
												else
												{
													//그냥 새로 생성
													addedVert = Editor.Select.Mesh.AddVertexAutoUV(mouseWorld);
												}
											}
											else
											{
												//그냥 새로 생성
												addedVert = Editor.Select.Mesh.AddVertexAutoUV(mouseWorld);
											}

											if (addedVert != null)
											{
												Editor.VertController.SelectVertex(addedVert);
											}


											bool isMakeEdge = false;
											if (makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH.VertexAndEdge)
											{
												apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_AddEdge, Editor, Editor.Select.Mesh, null, false);
												//만약 이전에 선택한 버텍스가 있다면
												//Edge를 연결하자
												if (prevSelectedVert != null)
												{
													// <<< 버텍스 -> 선 추가 >>>
													isMakeEdge = true;
													Editor.Select.Mesh.MakeNewEdge(prevSelectedVert, addedVert, isShift);
												}
											}

											//추가 9.12 : 미러 옵션이 있다면 반대쪽도 만들어야 한다.
											if (Editor._meshEditMirrorMode == apEditor.MESH_EDIT_MIRROR_MODE.Mirror)
											{
												Editor.MirrorSet.RefreshMeshWork(Editor.Select.Mesh, Editor.VertController);

												//추가
												Editor.MirrorSet.AddMirrorVertex(prevSelectedVert, addedVert, Editor.Select.Mesh, isMakeEdge, isShift, true, isAddVertexWithSplitEdge);

											}


											//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
											//if(undo != null) { undo.Refresh(); }
											//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
										}
									}
									else
									{
										//Edge 선택 모드에서
										//만약 HiddenEdge를 선택한다면
										//Turn을 하자
										Editor.VertController.UnselectVertex();
										Editor.VertController.UnselectNextVertex();

										if (_isHiddenEdgeTurnable)
										{
											//Undo - Vertex 추가
											apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_EditEdge, Editor, Editor.Select.Mesh, null, false);

											//아무것도 선택하지 않았다면..
											//Hidden Edge의 Trun을 한번 해보자
											apMeshEdge minHiddenEdge = null;
											float minDist = float.MaxValue;
											apMeshPolygon minPoly = null;

											List<apMeshPolygon> polygons = Editor.Select.Mesh._polygons;
											for (int iPoly = 0; iPoly < polygons.Count; iPoly++)
											{
												apMeshPolygon curPoly = polygons[iPoly];

												List<apMeshEdge> hiddenEdges = curPoly._hidddenEdges;

												for (int iHide = 0; iHide < hiddenEdges.Count; iHide++)
												{
													apMeshEdge hiddenEdge = hiddenEdges[iHide];

													Vector2 vPos1 = new Vector2(hiddenEdge._vert1._pos.x, hiddenEdge._vert1._pos.y) - Editor.Select.Mesh._offsetPos;
													Vector2 vPos2 = new Vector2(hiddenEdge._vert2._pos.x, hiddenEdge._vert2._pos.y) - Editor.Select.Mesh._offsetPos;

													float distEdge = apEditorUtil.DistanceFromLine(
																		apGL.World2GL(vPos1),
																		apGL.World2GL(vPos2),
																		mousePos);

													if (distEdge < 5.0f)
													{
														if (minHiddenEdge == null || distEdge < minDist)
														{
															minDist = distEdge;
															minHiddenEdge = hiddenEdge;
															minPoly = curPoly;
														}
													}
												}
											}

											if (minHiddenEdge != null)
											{
												//Debug.Log("Try Hidden Edge Turn");
												if (minPoly.TurnHiddenEdge(minHiddenEdge))
												{
													Editor.Select.Mesh.RefreshPolygonsToIndexBuffer();
												}
											}

											_isHiddenEdgeTurnable = false;
										}
									}
								}
								else
								{
									if (!isAnyAddVertOrMesh)
									{
										//Debug.Log("Start Vertex Edit");
										//Undo - MeshEdit Vertex Pos Changed
										apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_EditVertex, Editor, Editor.Select.Mesh, Editor.VertController.Vertex, false);
									}
								}
							}
							else
							{
								//추가 : Polygon 모드
								//apMeshPolygon prevPolygon = Editor.VertController.Polygon;
								List<apMeshPolygon> polygons = Editor.Select.Mesh._polygons;
								Vector2 meshOffsetPos = Editor.Select.Mesh._offsetPos;
								bool isAnyPolygonSelect = false;
								for (int iPoly = 0; iPoly < polygons.Count; iPoly++)
								{
									apMeshPolygon polygon = polygons[iPoly];
									if (IsPolygonClickable(polygon, meshOffsetPos, mousePos))
									{
										Editor.VertController.SelectPolygon(polygon);
										isAnyPolygonSelect = true;
										break;
									}
								}
								if (!isAnyPolygonSelect)
								{
									Editor.VertController.UnselectVertex();//<<이걸 호출하면 Polygon도 선택 해제됨
								}
							}
							//Editor.Repaint();
							Editor.SetRepaint();

							//통계 재계산 요청
							Editor.Select.SetStatisticsRefresh();
						}
					}
				}
				else if (leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
				{
					if (makeMeshMode != apEditor.MESH_EDIT_MODE_MAKEMESH.EdgeOnly &&
						makeMeshMode != apEditor.MESH_EDIT_MODE_MAKEMESH.Polygon)
					{
						if (Editor.VertController.Vertex != null)
						{
							if (IsMouseInGUI(mousePos))
							{
								if (_isMeshVertMovable)
								{
									if (!_isAnyVertexMoved)
									{
										//?
									}

									//버텍스 이동
									//Undo - MeshEdit Vertex Pos Changed
									apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_EditVertex, Editor, Editor.Select.Mesh, Editor.VertController.Vertex, false);

									Vector2 prevPos = Editor.VertController.Vertex._pos;
									Vector2 nextPos = apGL.GL2World(mousePos) + Editor.Select.Mesh._offsetPos;

									if (Editor._meshEditMirrorMode == apEditor.MESH_EDIT_MIRROR_MODE.Mirror)
									{
										//만약 Mirror가 켜졌다면
										if (Editor._meshTRSOption_MirrorSnapVertOnRuler)
										{
											//Snap 옵션이 켜졌을 때
											if (Editor.MirrorSet.IsOnAxisByMesh(nextPos, Editor.Select.Mesh))
											{
												//Ruler로 위치를 보정할 필요가 있다.
												nextPos = Editor.MirrorSet.GetAxisPosToSnap(nextPos, Editor.Select.Mesh);
											}
										}

									}
									Editor.VertController.Vertex._pos = nextPos;
									Editor.VertController.Mesh.RefreshVertexAutoUV(Editor.VertController.Vertex);

									_isAnyVertexMoved = true;

									//추가 : 미러 옵션이 켜진 경우 : 맞은편 버텍스를 찾아서 움직여보자
									if (Editor._meshEditMirrorMode == apEditor.MESH_EDIT_MIRROR_MODE.Mirror)
									{
										Editor.MirrorSet.MoveMirrorVertex(Editor.VertController.Vertex, prevPos, Editor.VertController.Vertex._pos, Editor.Select.Mesh);
									}

								}
							}
							else
							{
								Editor.VertController.UnselectVertex();
							}

							//Editor.Repaint();
							Editor.SetRepaint();
						}
					}
				}
				else if (leftBtnStatus == apMouse.MouseBtnStatus.Up ||
						leftBtnStatus == apMouse.MouseBtnStatus.Released)
				{
					Editor.VertController.StopEdgeWire();
					//Editor.Repaint();

					_isHiddenEdgeTurnable = true;

					if (_isAnyVertexMoved)
					{
						//apEditorUtil.SetRecord("Vertex Pos Change", Editor._selection.Mesh);
						_isAnyVertexMoved = false;
					}
					//if (Editor.VertController.Vertex != null)
					//{
					//	//마우스를 뗐을때 Unselect?
					//	//일단 냅두자
					//	//Editor.VertController.UnselectVertex();

					//	//Editor.Repaint();
					//	Editor.SetRepaint();
					//}
				}

				//mousePos = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Pos;

				if (rightBtnStatus == apMouse.MouseBtnStatus.Down)
				{
					Editor.VertController.UnselectVertex();

					if (IsMouseInGUI(mousePos))
					{
						if (Editor.Select.Mesh != null)
						{
							bool isAnyRemoved = false;

							if (isVertEdgeRemovalble)
							{
								if (makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH.VertexOnly ||
									makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH.VertexAndEdge)
								{
									//1. 버텍스 제거
									for (int i = 0; i < Editor.Select.Mesh._vertexData.Count; i++)
									{
										apVertex vertex = Editor.Select.Mesh._vertexData[i];
										Vector2 vPos = new Vector2(vertex._pos.x, vertex._pos.y) - Editor.Select.Mesh._offsetPos;

										Vector2 posGL = apGL.World2GL(vPos);

										//어떤 버텍스를 선택했다.
										if (IsVertexClickable(posGL, mousePos))
										{
											// Undo - MeshEdit_VertexRemoved
											apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_RemoveVertex, Editor, Editor.Select.Mesh, vertex, false);

											Editor.Select.Mesh.RemoveVertex(vertex, isShift);

											//추가 9.13 : Mirror Vertex 삭제
											if (Editor._meshEditMirrorMode == apEditor.MESH_EDIT_MIRROR_MODE.Mirror
												&& Editor._meshTRSOption_MirrorRemoved)
											{
												//반대편 버텍스도 삭제한다.
												Editor.MirrorSet.RemoveMirrorVertex(vertex, Editor.Select.Mesh, isShift);
											}

											//Editor.Repaint();
											Editor.SetRepaint();
											isAnyRemoved = true;
											isVertEdgeRemovalble = false;
											break;
										}
									}
								}
							}

							if (isVertEdgeRemovalble)
							{
								if (!isAnyRemoved)
								{
									if (makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH.VertexAndEdge ||
										makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH.EdgeOnly)
									{
										//2. Edge 제거
										apMeshEdge selectEdge = null;


										#region [미사용 코드 : 함수로 옮김]
										//float minDist = float.MaxValue;

										//for (int i = 0; i < Editor.Select.Mesh._edges.Count; i++)
										//{
										//	apMeshEdge edge = Editor.Select.Mesh._edges[i];

										//	if (edge._vert1 == null || edge._vert2 == null)
										//	{
										//		continue;
										//	}

										//	Vector2 vPos1 = new Vector2(edge._vert1._pos.x, edge._vert1._pos.y) - Editor.Select.Mesh._offsetPos;
										//	Vector2 vPos2 = new Vector2(edge._vert2._pos.x, edge._vert2._pos.y) - Editor.Select.Mesh._offsetPos;

										//	float distEdge = apEditorUtil.DistanceFromLine(
										//		apGL.World2GL(vPos1),
										//		apGL.World2GL(vPos2),
										//		mousePos);

										//	if (distEdge < 5.0f)
										//	{
										//		if (selectEdge == null || distEdge < minDist)
										//		{
										//			minDist = distEdge;
										//			selectEdge = edge;
										//		}
										//	}

										//} 
										#endregion
										//변경 : 함수 하나로 합침
										selectEdge = GetMeshNearestEdge(mousePos, Editor.Select.Mesh, 5.0f);

										if (selectEdge != null)
										{
											//삭제합시더
											// Undo - MeshEdit_EdgeRemoved
											apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_RemoveEdge, Editor, Editor.Select.Mesh, selectEdge, false);

											Editor.Select.Mesh.RemoveEdge(selectEdge);
											isVertEdgeRemovalble = false;

											//추가 9.13 : Mirror Edge 삭제
											if (Editor._meshEditMirrorMode == apEditor.MESH_EDIT_MIRROR_MODE.Mirror
												&& Editor._meshTRSOption_MirrorRemoved)
											{
												//반대편 Edge도 삭제한다.
												Editor.MirrorSet.RemoveMirrorEdge(selectEdge, Editor.Select.Mesh);
											}
										}

										Editor.VertController.UnselectVertex();
										Editor.VertController.UnselectNextVertex();
										//Editor.Repaint();
										Editor.SetRepaint();

										//통계 재계산 요청
										Editor.Select.SetStatisticsRefresh();
									}
								}
							}
						}
					}

				}
			}

			isNearestVertexCheckable = false;

			if (Editor.VertController.Vertex != null)
			{
				if (Editor.Select.Mesh != null && IsMouseInGUI(mousePos) && makeMeshMode != apEditor.MESH_EDIT_MODE_MAKEMESH.Polygon)
				{
					if (makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH.VertexAndEdge ||
									makeMeshMode == apEditor.MESH_EDIT_MODE_MAKEMESH.EdgeOnly)
					{
						if (rightBtnStatus == apMouse.MouseBtnStatus.Up ||
							rightBtnStatus == apMouse.MouseBtnStatus.Released)
						{
							if (IsMouseInGUI(mousePos))
							{
								isNearestVertexCheckable = true;
							}
						}
					}
				}

				Editor.VertController.UpdateEdgeWire(mousePos, isShift, isCtrl);
				Editor.VertController.UnselectNextVertex();

				//마우스에서 가까운 Vertex를 찾는다.
				//Ctrl을 누르면 : 가장 가까운거 무조건
				//기본 : Vertex 영역안에 있는거
				if (isNearestVertexCheckable)
				{
					if (isCtrl)
					{
						apVertex nearestVert = null;
						float minDistToVert = 0.0f;
						for (int i = 0; i < Editor.Select.Mesh._vertexData.Count; i++)
						{
							apVertex vertex = Editor.Select.Mesh._vertexData[i];
							Vector2 vPos = new Vector2(vertex._pos.x, vertex._pos.y) - Editor.Select.Mesh._offsetPos;

							Vector2 posGL = apGL.World2GL(vPos);
							float distToMouse = Vector2.Distance(posGL, mousePos);
							if (nearestVert == null || distToMouse < minDistToVert)
							{
								nearestVert = vertex;
								minDistToVert = distToMouse;
							}
						}
						if (nearestVert != null)
						{
							Editor.VertController.SelectNextVertex(nearestVert);

							Vector2 vPos = new Vector2(nearestVert._pos.x, nearestVert._pos.y) - Editor.Select.Mesh._offsetPos;
							Vector2 posGL = apGL.World2GL(vPos);


							Editor.VertController.UpdateEdgeWire(posGL, isShift, isCtrl);
						}
					}
					else
					{
						for (int i = 0; i < Editor.Select.Mesh._vertexData.Count; i++)
						{
							apVertex vertex = Editor.Select.Mesh._vertexData[i];
							Vector2 vPos = new Vector2(vertex._pos.x, vertex._pos.y) - Editor.Select.Mesh._offsetPos;

							Vector2 posGL = apGL.World2GL(vPos);

							//어떤 버텍스를 선택했다.
							if (IsVertexClickable(posGL, mousePos))
							{
								Editor.VertController.SelectNextVertex(vertex);

								Editor.VertController.UpdateEdgeWire(posGL, isShift, isCtrl);
								break;
							}
						}
					}
				}

				Editor.VertController.UpdateSnapEdgeGUIOnly(mousePos, isShift, isCtrl, leftBtnStatus == apMouse.MouseBtnStatus.Pressed);
			}
			else
			{
				Editor.VertController.StopEdgeWire();
				Editor.VertController.UnselectNextVertex();

				if (isCtrl)
				{
					apVertex nearestVert = null;
					float minDistToVert = 0.0f;
					for (int i = 0; i < Editor.Select.Mesh._vertexData.Count; i++)
					{
						apVertex vertex = Editor.Select.Mesh._vertexData[i];
						Vector2 vPos = new Vector2(vertex._pos.x, vertex._pos.y) - Editor.Select.Mesh._offsetPos;

						Vector2 posGL = apGL.World2GL(vPos);
						float distToMouse = Vector2.Distance(posGL, mousePos);
						if (nearestVert == null || distToMouse < minDistToVert)
						{
							nearestVert = vertex;
							minDistToVert = distToMouse;
						}
					}
					if (nearestVert != null)
					{
						//Ctrl을 누르고 있을 때 Next Vertex만은 다시 계산
						Editor.VertController.SelectNextVertex(nearestVert);
					}
				}
				Editor.VertController.UpdateSnapEdgeGUIOnly(mousePos, isShift, isCtrl, leftBtnStatus == apMouse.MouseBtnStatus.Pressed);
			}
		}


		public apMeshEdge GetMeshNearestEdge(Vector2 posGL, apMesh mesh, float offsetGL)
		{
			apMeshEdge curEdge = null;

			//Vector2 posW = apGL.GL2World(posGL) + mesh._offsetPos;

			Vector2 vPos1GL = Vector2.zero;
			Vector2 vPos2GL = Vector2.zero;
			float minX = 0.0f;
			float maxX = 0.0f;
			float minY = 0.0f;
			float maxY = 0.0f;
			float curDist = 0.0f;

			float minDist = 0.0f;
			apMeshEdge minEdge = null;

			for (int i = 0; i < mesh._edges.Count; i++)
			{
				curEdge = mesh._edges[i];

				if (curEdge._vert1 == null || curEdge._vert2 == null)
				{
					continue;
				}

				//기본 사각 범위안에 있는지 확인
				vPos1GL = apGL.World2GL(curEdge._vert1._pos - mesh._offsetPos);
				vPos2GL = apGL.World2GL(curEdge._vert2._pos - mesh._offsetPos);

				minX = Mathf.Min(vPos1GL.x, vPos2GL.x);
				maxX = Mathf.Max(vPos1GL.x, vPos2GL.x);
				minY = Mathf.Min(vPos1GL.y, vPos2GL.y);
				maxY = Mathf.Max(vPos1GL.y, vPos2GL.y);

				if (posGL.x < minX || maxX < posGL.x ||
					posGL.y < minY || maxY < posGL.y)
				{
					continue;
				}

				curDist = apEditorUtil.DistanceFromLine(
					vPos1GL,
					vPos2GL,
					posGL);

				if (curDist < offsetGL)
				{
					if (minEdge == null || curDist < minDist)
					{
						minDist = curDist;
						minEdge = curEdge;
					}
				}

			}
			return minEdge;
		}


		#region [미사용 코드]
		//public void GUI_Input_LinkEdge()
		//{
		//	apMouse.MouseBtnStatus leftBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Status;
		//	apMouse.MouseBtnStatus rightBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Status;
		//	//Vector2 mousePos = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Pos;
		//	Vector2 mousePos = apMouse.Pos;

		//	if (Editor.VertController.Mesh == null || Editor.Select.Mesh != Editor.VertController.Mesh)
		//	{
		//		if (Editor.Select.Mesh != null)
		//		{
		//			Editor.VertController.SetMesh(Editor.Select.Mesh);
		//		}
		//		else
		//		{
		//			return;
		//		}

		//	}

		//	//Down -> Select
		//	//Pressed -> (Select가 있으면) 임시 Wire + 위치 파악해서 두번째 점 (예상 도달 점 체크)
		//	//Up -> (1) (Select가 있으면 + 두번째 점이 있으면) : 연결 -> 두개의 점 모두 해제
		//	//Up -> (2) (Select가 있으면 + 두번째 점이 없으면) => (그래도 좀 가까운 점 찾아서) : 연결 -> 두개의 점 모두 해제 

		//	#region [코드 1 : 드래그 드롭으로 연결하기]
		//	//if(leftBtnStatus == apMouse.MouseBtnStatus.Down)
		//	//{	
		//	//	if(IsMouseInGUI(mousePos))
		//	//	{
		//	//		if(Editor._selection.Mesh != null)
		//	//		{
		//	//			Editor.VertController.UnselectVertex();

		//	//			for (int i = 0; i < Editor._selection.Mesh._vertexData.Count; i++)
		//	//			{
		//	//				apVertex vertex = Editor._selection.Mesh._vertexData[i];
		//	//				Vector2 posGL = apGL.World2GL(vertex._pos);

		//	//				//어떤 버텍스를 선택했다.
		//	//				if(IsVertexClickable(posGL, mousePos))
		//	//				{
		//	//					Editor.VertController.SelectVertex(vertex);
		//	//					break;
		//	//				}
		//	//			}
		//	//			Editor.Repaint();
		//	//		}
		//	//	}
		//	//}
		//	//else if(leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
		//	//{
		//	//	if (Editor.VertController.Vertex != null)
		//	//	{
		//	//		if (IsMouseInGUI(mousePos))
		//	//		{
		//	//			Editor.VertController.UpdateEdgeWire(mousePos);
		//	//		}

		//	//		Editor.VertController.UnselectNextVertex();

		//	//		if (Editor._selection.Mesh != null)
		//	//		{
		//	//			//Pressed -> (Select가 있으면) 임시 Wire + 위치 파악해서 두번째 점 (예상 도달 점 체크)
		//	//			//TODO
		//	//			Vector2 mousePosW = apGL.GL2World(mousePos);
		//	//			apVertex minVert = null;
		//	//			float minDist = float.MaxValue;
		//	//			for (int i = 0; i < Editor._selection.Mesh._vertexData.Count; i++)
		//	//			{
		//	//				apVertex vert = Editor._selection.Mesh._vertexData[i];
		//	//				if (vert == Editor.VertController.Vertex)
		//	//				{
		//	//					continue;
		//	//				}

		//	//				float dist = Vector2.Distance(vert._pos, mousePosW);
		//	//				if (dist < minDist || minVert == null)
		//	//				{
		//	//					minVert = vert;
		//	//					minDist = dist;
		//	//				}
		//	//			}
		//	//			if (minVert != null)
		//	//			{
		//	//				if(minDist / apGL.Zoom < 30.0f)
		//	//				{
		//	//					Editor.VertController.SelectNextVertex(minVert);
		//	//				}
		//	//			}
		//	//		}

		//	//		Editor.Repaint();
		//	//	}
		//	//}
		//	//else
		//	//{
		//	//	if(Editor.VertController.Vertex != null)
		//	//	{
		//	//		//Up -> (1) (Select가 있으면 + 두번째 점이 있으면) : 연결 -> 두개의 점 모두 해제
		//	//		if(Editor.VertController.LinkedNextVertex != null)
		//	//		{
		//	//			if(Editor._selection.Mesh != null)
		//	//			{
		//	//				//새로운 Edge를 추가하자
		//	//				Editor._selection.Mesh.MakeNewEdge(Editor.VertController.Vertex, Editor.VertController.LinkedNextVertex);
		//	//			}
		//	//		}

		//	//		Editor.VertController.StopEdgeWire();
		//	//		Editor.VertController.UnselectVertex();
		//	//		Editor.Repaint();
		//	//	}
		//	//} 
		//	#endregion



		//	//mousePos = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Pos;

		//	#region [코드 1 : 우클릭 = 완성]
		//	//if(rightBtnStatus == apMouse.MouseBtnStatus.Down)
		//	//{
		//	//	Editor.VertController.UnselectVertex();

		//	//	if (IsMouseInGUI(mousePos))
		//	//	{
		//	//		if (Editor._selection.Mesh != null)
		//	//		{
		//	//			//삭제할까.. 아니면 중단할까..
		//	//			//중단을 해보자 (X) 아니다. 삭제를 하자
		//	//			Editor._selection.Mesh.MakeEdgesToIndexBuffer();
		//	//			Editor._meshEditMode = apEditor.MESH_EDIT_MODE.None;
		//	//		}
		//	//	}

		//	//} 
		//	#endregion

		//	//mousePos = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Pos;

		//	if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
		//	{
		//		if (IsMouseInGUI(mousePos))
		//		{
		//			//일단 뭔가의 버텍스를 선택한다.
		//			apVertex clickedVert = null;
		//			for (int i = 0; i < Editor.Select.Mesh._vertexData.Count; i++)
		//			{
		//				apVertex vertex = Editor.Select.Mesh._vertexData[i];
		//				Vector2 vPos = new Vector2(vertex._pos.x, vertex._pos.y) - Editor.Select.Mesh._offsetPos;

		//				Vector2 posGL = apGL.World2GL(vPos);

		//				if (IsVertexClickable(posGL, mousePos))
		//				{
		//					clickedVert = vertex;
		//					break;
		//				}
		//			}

		//			if (clickedVert == null)
		//			{
		//				//아무것도 선택하지 않았다.
		//				Editor.VertController.UnselectVertex();
		//				Editor.VertController.UnselectNextVertex();

		//				if (_isHiddenEdgeTurnable)
		//				{
		//					//아무것도 선택하지 않았다면..
		//					//Hidden Edge의 Trun을 한번 해보자
		//					apMeshEdge minHiddenEdge = null;
		//					float minDist = float.MaxValue;
		//					apMeshPolygon minPoly = null;

		//					List<apMeshPolygon> polygons = Editor.Select.Mesh._polygons;
		//					for (int iPoly = 0; iPoly < polygons.Count; iPoly++)
		//					{
		//						apMeshPolygon curPoly = polygons[iPoly];

		//						List<apMeshEdge> hiddenEdges = curPoly._hidddenEdges;

		//						for (int iHide = 0; iHide < hiddenEdges.Count; iHide++)
		//						{
		//							apMeshEdge hiddenEdge = hiddenEdges[iHide];

		//							Vector2 vPos1 = new Vector2(hiddenEdge._vert1._pos.x, hiddenEdge._vert1._pos.y) - Editor.Select.Mesh._offsetPos;
		//							Vector2 vPos2 = new Vector2(hiddenEdge._vert2._pos.x, hiddenEdge._vert2._pos.y) - Editor.Select.Mesh._offsetPos;

		//							float distEdge = apEditorUtil.DistanceFromLine(
		//												apGL.World2GL(vPos1),
		//												apGL.World2GL(vPos2),
		//												mousePos);

		//							if (distEdge < 5.0f)
		//							{
		//								if (minHiddenEdge == null || distEdge < minDist)
		//								{
		//									minDist = distEdge;
		//									minHiddenEdge = hiddenEdge;
		//									minPoly = curPoly;
		//								}
		//							}
		//						}
		//					}

		//					if (minHiddenEdge != null)
		//					{
		//						//Debug.Log("Try Hidden Edge Turn");
		//						if (minPoly.TurnHiddenEdge(minHiddenEdge))
		//						{
		//							Editor.Select.Mesh.RefreshPolygonsToIndexBuffer();
		//						}
		//					}

		//					_isHiddenEdgeTurnable = false;
		//				}
		//			}
		//			else
		//			{
		//				//뭔가를 선택했다.
		//				apVertex prevSelected = Editor.VertController.Vertex;

		//				//기존에 선택한게 있다
		//				//-> 연결한다.
		//				if (prevSelected != null)
		//				{
		//					// Undo - MeshEdit_AddEdge
		//					//apEditorUtil.SetRecord("MakeEdge", Editor._portrait);
		//					apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshEdit_AddEdge, Editor.Select.Mesh, null, false, Editor);
		//					//bool isSuccess = Editor.Select.Mesh.MakeNewEdge(prevSelected, clickedVert);
		//					Editor.Select.Mesh.MakeNewEdge(prevSelected, clickedVert);

		//				}

		//				//선택
		//				Editor.VertController.SelectVertex(clickedVert);
		//				Editor.VertController.UnselectNextVertex();
		//			}

		//			//Editor.Repaint();
		//			Editor.SetRepaint();
		//		}
		//	}
		//	else if (leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
		//	{
		//		//Pressed 없음
		//		//if (Editor.VertController.Vertex != null)
		//		//{
		//		//	if (IsMouseInGUI(mousePos))
		//		//	{
		//		//		Editor.VertController.UpdateEdgeWire(mousePos);
		//		//	}
		//		//	Editor.Repaint();
		//		//}
		//	}
		//	else if (leftBtnStatus == apMouse.MouseBtnStatus.Up)
		//	{
		//		Editor.VertController.StopEdgeWire();
		//		//Editor.Repaint();

		//		_isHiddenEdgeTurnable = true;
		//	}
		//	else
		//	{
		//		//Released 상태에서도 업데이트
		//		//if (Editor.VertController.Vertex != null)
		//		//{
		//		//	if (IsMouseInGUI(mousePos))
		//		//	{
		//		//		Editor.VertController.UpdateEdgeWire(mousePos);

		//		//		//마우스 근처에서 NextVertex를 선택하자 (롤오버 효과)
		//		//		Vector2 mousePosW = apGL.GL2World(mousePos);
		//		//		apVertex minVert = null;
		//		//		float minDist = float.MaxValue;
		//		//		for (int i = 0; i < Editor._selection.Mesh._vertexData.Count; i++)
		//		//		{
		//		//			apVertex vert = Editor._selection.Mesh._vertexData[i];
		//		//			if (vert == Editor.VertController.Vertex)
		//		//			{
		//		//				continue;
		//		//			}

		//		//			float dist = Vector2.Distance(vert._pos, mousePosW);
		//		//			if (dist < minDist || minVert == null)
		//		//			{
		//		//				minVert = vert;
		//		//				minDist = dist;
		//		//			}
		//		//		}
		//		//		if (minVert != null)
		//		//		{
		//		//			if (minDist / apGL.Zoom < 30.0f)
		//		//			{
		//		//				Editor.VertController.SelectNextVertex(minVert);
		//		//			}
		//		//		}

		//		//		Editor.Repaint();
		//		//	}
		//		//}
		//	}


		//	//mousePos = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Pos;
		//	if (rightBtnStatus == apMouse.MouseBtnStatus.Up)
		//	{
		//		if (IsMouseInGUI(mousePos))
		//		{
		//			//Edge를 지우자
		//			apMeshEdge selectEdge = null;
		//			float minDist = float.MaxValue;

		//			for (int i = 0; i < Editor.Select.Mesh._edges.Count; i++)
		//			{
		//				apMeshEdge edge = Editor.Select.Mesh._edges[i];

		//				if (edge._vert1 == null || edge._vert2 == null)
		//				{
		//					continue;
		//				}

		//				Vector2 vPos1 = new Vector2(edge._vert1._pos.x, edge._vert1._pos.y) - Editor.Select.Mesh._offsetPos;
		//				Vector2 vPos2 = new Vector2(edge._vert2._pos.x, edge._vert2._pos.y) - Editor.Select.Mesh._offsetPos;

		//				float distEdge = apEditorUtil.DistanceFromLine(
		//					apGL.World2GL(vPos1),
		//					apGL.World2GL(vPos2),
		//					mousePos);

		//				if (distEdge < 5.0f)
		//				{
		//					if (selectEdge == null || distEdge < minDist)
		//					{
		//						minDist = distEdge;
		//						selectEdge = edge;
		//					}
		//				}

		//			}

		//			if (selectEdge != null)
		//			{
		//				//삭제합시더
		//				//Editor._selection.Mesh._edges.Remove(selectEdge);
		//				// Undo - Remove Edge
		//				apEditorUtil.SetRecord(apUndoGroupData.ACTION.MeshEdit_RemoveEdge, Editor.Select.Mesh, selectEdge, false, Editor);

		//				Editor.Select.Mesh.RemoveEdge(selectEdge);


		//			}

		//			Editor.VertController.UnselectVertex();
		//			Editor.VertController.UnselectNextVertex();
		//			//Editor.Repaint();
		//			Editor.SetRepaint();
		//		}

		//	}
		//} 
		#endregion



		private bool _isMeshPivotEdit_Moved = false;
		private Vector2 _mouseDownPos_PivotEdit = Vector2.zero;
		public void GUI_Input_PivotEdit(float tDelta)
		{
			//이전
			//apMouse.MouseBtnStatus leftBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Status;
			//apMouse.MouseBtnStatus rightBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Status;

			//Vector2 mousePos = apMouse.Pos;

			//이후
			apMouse.MouseBtnStatus leftBtnStatus = Editor.Mouse.GetStatus(apMouseSet.Button.Left, apMouseSet.ACTION.MeshEdit_Pivot);
			apMouse.MouseBtnStatus rightBtnStatus = Editor.Mouse.GetStatus(apMouseSet.Button.Right, apMouseSet.ACTION.MeshEdit_Pivot);
			Vector2 mousePos = Editor.Mouse.Pos;

			if (Editor.VertController.Mesh == null || Editor.Select.Mesh != Editor.VertController.Mesh)
			{
				if (Editor.Select.Mesh != null)
				{
					Editor.VertController.SetMesh(Editor.Select.Mesh);
				}
				else
				{
					_isMeshPivotEdit_Moved = false;
					return;
				}
			}

			//순서는 Gizmo Transform -> Mouse 위치 체크 -> GizmoUpdate -> 결과 봐서 나머지 처리
			//Editor.Gizmos.Select(Editor.Select.Mesh);
			//Editor.Gizmos.SetTransform(Vector2.zero, Editor.Select.Mesh.Matrix_VertToLocal, apMatrix3x3.identity, apMatrix3x3.identity);

			if (!IsMouseInGUI(mousePos))
			{
				_isMeshPivotEdit_Moved = false;
				return;
			}

#if UNITY_EDITOR_OSX
				bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			//Gizmo 업데이트
			Editor.Gizmos.Update(tDelta, leftBtnStatus, rightBtnStatus, mousePos, isCtrl, Event.current.shift, Event.current.alt);

			if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
			{
				if (apEditorUtil.IsMouseInMesh(mousePos, Editor.Select.Mesh))
				{


					_isMeshPivotEdit_Moved = true;
					_mouseDownPos_PivotEdit = mousePos;
				}
			}
			else if (leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
			{
				if (_isMeshPivotEdit_Moved)
				{
					Vector2 posDownW = apGL.GL2World(_mouseDownPos_PivotEdit);
					//Vector2 posCurW = apGL.GL2World(apMouse.Pos);//이전
					Vector2 posCurW = apGL.GL2World(Editor.Mouse.Pos);//이후

					//아예 함수로 만들것
					//이전 버전 : 그냥 Mesh의 Offset만 바꾼다.

					//apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_SetPivot, Editor, Editor.Select.Mesh, Editor.Select.Mesh._offsetPos, false);
					//Editor.Select.Mesh._offsetPos -= (posCurW - posDownW);//<TODO : 이걸 사용하는 MeshGroup들의 DefaultPos를 역연산해야한다.
					//Editor.Select.Mesh.MakeOffsetPosMatrix();//<<OffsetPos를 수정하면 이걸 바꿔주자

					Vector2 newPivotPos = Editor.Select.Mesh._offsetPos - (posCurW - posDownW);
					SetMeshPivot(Editor.Select.Mesh, newPivotPos);

					_mouseDownPos_PivotEdit = mousePos;
					//Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].UseMouseDrag();
				}
			}
			else
			{
				_isMeshPivotEdit_Moved = false;
			}
		}



		public void GUI_PrintBrushCursor(apGizmos gizmo)
		{
			//이전
			//apMouse.MouseBtnStatus leftBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Status;
			//apMouse.MouseBtnStatus rightBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Status;
			//apMouse.MouseBtnStatus midBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_MIDDLE].Status;
			//Vector2 mousePos = apMouse.Pos;

			float radius = gizmo.BrushRadius;
			apGizmos.BRUSH_COLOR_MODE colorMode = gizmo.BrushColorMode;
			Texture2D image = gizmo.BrushImage;
			float lerp = gizmo.BrushColorLerp;


			//이후
			apMouse.MouseBtnStatus leftBtnStatus = Editor.Mouse.GetStatus(apMouseSet.Button.Left, apMouseSet.ACTION.Brush);
			//apMouse.MouseBtnStatus rightBtnStatus = Editor.Mouse.GetStatus(apMouseSet.Button.Right, apMouseSet.ACTION.Brush);
			apMouse.MouseBtnStatus midBtnStatus = Editor.Mouse.GetStatus(apMouseSet.Button.Middle, apMouseSet.ACTION.Brush);
			Vector2 mousePos = Editor.Mouse.Pos;

			if (!IsMouseInGUI(mousePos))
			{
				//Editor.Repaint();
				Editor.SetRepaint();
				return;
			}

			if (midBtnStatus == apMouse.MouseBtnStatus.Down ||
				midBtnStatus == apMouse.MouseBtnStatus.Pressed)
			{
				//Editor.Repaint();
				Editor.SetRepaint();
				return;
			}

			Color colorRelease = Color.black;
			Color colorClick = Color.black;

			//색상은 총 4개(이동 A, B / 좌클릭 A, B)
			//B는 A보다 조금 더 밝은 색
			//등급이 높아질 수록 색상이 진해짐
			
			//기본 : 노란색 / 주황색
			//증가 : 연두색>녹색 / 하늘색>푸른색
			//감소 : 푸른톤의 보라색>진분홍 / 주황색>붉은색

			switch (colorMode)
			{
				case apGizmos.BRUSH_COLOR_MODE.Default:
					colorRelease = new Color(0.8f, 0.8f, 0.0f, 1.0f);
					colorClick = new Color(1.0f, 0.5f, 0.0f, 1.0f);
					break;

				case apGizmos.BRUSH_COLOR_MODE.Increase_Lv1:
					colorRelease = new Color(0.5f, 1.0f, 0.0f, 1.0f);
					colorClick = new Color(0.0f, 1.0f, 1.0f, 1.0f);
					break;

				case apGizmos.BRUSH_COLOR_MODE.Increase_Lv2:
					colorRelease = new Color(0.3f, 1.0f, 0.0f, 1.0f);
					colorClick = new Color(0.0f, 0.8f, 1.0f, 1.0f);
					break;

				case apGizmos.BRUSH_COLOR_MODE.Increase_Lv3:
					colorRelease = new Color(0.0f, 1.0f, 0.0f, 1.0f);
					colorClick = new Color(0.0f, 0.5f, 1.0f, 1.0f);
					break;

				case apGizmos.BRUSH_COLOR_MODE.Decrease_Lv1:
					colorRelease = new Color(0.8f, 0.0f, 1.0f, 1.0f);
					colorClick = new Color(1.0f, 0.4f, 0.0f, 1.0f);
					break;

				case apGizmos.BRUSH_COLOR_MODE.Decrease_Lv2:
					colorRelease = new Color(1.0f, 0.0f, 1.0f, 1.0f);
					colorClick = new Color(1.0f, 0.2f, 0.0f, 1.0f);
					break;

				case apGizmos.BRUSH_COLOR_MODE.Decrease_Lv3:
					colorRelease = new Color(1.0f, 0.0f, 0.8f, 1.0f);
					colorClick = new Color(1.0f, 0.0f, 0.0f, 1.0f);
					break;

			}

			Color color = Color.yellow;
			float lineWidth = (2.0f * lerp) + (4.0f * (1.0f - lerp));

			if (leftBtnStatus == apMouse.MouseBtnStatus.Down
				|| leftBtnStatus == apMouse.MouseBtnStatus.Pressed)
			{
				color = colorClick * (1.0f - lerp) + ((colorClick * 2.0f + Color.white * 0.2f) * lerp);
			}
			else
			{
				color = colorRelease * (1.0f - lerp) + ((colorRelease * 2.0f + Color.white * 0.2f) * lerp);
			}

			color.a = 1.0f;

			//apGL.DrawCircle(apGL.GL2World(mousePos), radius, color, true);
			apGL.DrawBoldCircleGL(mousePos, radius, lineWidth, color, true);
			if(image != null)
			{
				float sqrt2 = 1.0f / 1.414f;
				apGL.DrawTextureGL(image, mousePos + new Vector2(radius * sqrt2 + 16, radius * sqrt2 + 16), 32, 32, Color.grey, 0.0f);
			}

			//Editor.Repaint();
			Editor.SetRepaint();
		}

		public bool IsVertexClickable(Vector2 vertPos, Vector2 mousePos)
		{
			if (!IsMouseInGUI(vertPos))
			{
				return false;
			}

			Vector2 difPos = mousePos - vertPos;
			if (Mathf.Abs(difPos.x) < 6.0f && Mathf.Abs(difPos.y) < 6.0f)
			{
				return true;
			}
			return false;
		}

		public bool IsPolygonClickable(apMeshPolygon polygon, Vector2 meshOffsetPos, Vector2 mousePos)
		{
			//Vector2 vPos = new Vector2(vertex._pos.x, vertex._pos.y) - Editor.Select.Mesh._offsetPos;
			//Vector2 posGL = apGL.World2GL(vPos);
			//Tri 체크를 해보자
			for (int iTri = 0; iTri < polygon._tris.Count; iTri++)
			{
				apMeshTri tri = polygon._tris[iTri];
				Vector2 vPos0 = apGL.World2GL(new Vector2(tri._verts[0]._pos.x, tri._verts[0]._pos.y) - meshOffsetPos);
				Vector2 vPos1 = apGL.World2GL(new Vector2(tri._verts[1]._pos.x, tri._verts[1]._pos.y) - meshOffsetPos);
				Vector2 vPos2 = apGL.World2GL(new Vector2(tri._verts[2]._pos.x, tri._verts[2]._pos.y) - meshOffsetPos);

				if (apEditorUtil.IsPointInTri(mousePos, vPos0, vPos1, vPos2))
				{
					return true;
				}
			}
			return false;

		}

		public void SetMeshPivot(apMesh mesh, Vector2 nextOffsetPos)
		{
			//이 mesh를 포함하는 모든 MeshGroup을 찾는다.
			if (Editor == null
				|| Editor._portrait == null
				|| mesh == null)
			{
				return;
			}

			List<apMeshGroup> linkedMeshGroups = new List<apMeshGroup>();
			for (int i = 0; i < Editor._portrait._meshGroups.Count; i++)
			{
				apMeshGroup meshGroup = Editor._portrait._meshGroups[i];
				if (meshGroup._childMeshTransforms.Exists(delegate (apTransform_Mesh a)
						 {
							 return a._mesh == mesh;
						 }))
				{
					//이 Mesh를 사용하는 MeshGroup을 추가한다.
					linkedMeshGroups.Add(meshGroup);
				}
			}
			apEditorUtil.SetRecord_MeshAndMeshGroups(apUndoGroupData.ACTION.MeshEdit_SetPivot, Editor, mesh, linkedMeshGroups, mesh, false);

			Vector2 prevOffset = mesh._offsetPos;
			apMatrix3x3 prevOffsetMatrix = apMatrix3x3.TRS(new Vector2(-prevOffset.x, -prevOffset.y), 0, Vector2.one);
			apMatrix3x3 nextOffsetMatrix = apMatrix3x3.TRS(new Vector2(-nextOffsetPos.x, -nextOffsetPos.y), 0, Vector2.one);


			apMatrix3x3 prevDefaultMatrix = apMatrix3x3.identity;
			//apMatrix nextDefaultMatrix = new apMatrix();

			for (int iMG = 0; iMG < linkedMeshGroups.Count; iMG++)
			{
				apMeshGroup meshGroup = linkedMeshGroups[iMG];
				List<apTransform_Mesh> meshTransforms = meshGroup._childMeshTransforms.FindAll(delegate (apTransform_Mesh a)
				{
					return a._mesh == mesh;
				});

				//Mesh를 참조하는 MeshTransform

				for (int iMesh = 0; iMesh < meshTransforms.Count; iMesh++)
				{
					apTransform_Mesh meshTF = meshTransforms[iMesh];
					prevDefaultMatrix.SetMatrix(meshTF._matrix.MtrxToSpace);

					float newPosX = prevDefaultMatrix._m00 * (prevOffsetMatrix._m02 - nextOffsetMatrix._m02)
									+ prevDefaultMatrix._m01 * (prevOffsetMatrix._m12 - nextOffsetMatrix._m12)
									+ prevDefaultMatrix._m02;
					float newPosY = prevDefaultMatrix._m10 * (prevOffsetMatrix._m02 - nextOffsetMatrix._m02)
									+ prevDefaultMatrix._m11 * (prevOffsetMatrix._m12 - nextOffsetMatrix._m12)
									+ prevDefaultMatrix._m12;

					//회전, 크기 값은 동일한다.

					//Debug.Log("[" + meshTF._nickName + "] Pos Changed : " + meshTF._matrix._pos + " >> " + newPosX + ", " + newPosY);

					meshTF._matrix.SetPos(newPosX, newPosY);

				}

				meshGroup.RefreshForce(true);
			}

			mesh._offsetPos = nextOffsetPos;
			mesh.MakeOffsetPosMatrix();


		}






		//------------------------------------------------------------------------------
		public void GUI_Input_MeshGroup_Setting(float tDelta)
		{
			//이전
			//apMouse.MouseBtnStatus leftBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Status;
			//apMouse.MouseBtnStatus rightBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Status;
			//Vector2 mousePos = apMouse.Pos;

			//이후
			apMouse.MouseBtnStatus leftBtnStatus = Editor.Mouse.GetStatus(apMouseSet.Button.Left, apMouseSet.ACTION.MeshGroup_Setting);
			apMouse.MouseBtnStatus rightBtnStatus = Editor.Mouse.GetStatus(apMouseSet.Button.Right, apMouseSet.ACTION.MeshGroup_Setting);
			Vector2 mousePos = Editor.Mouse.Pos;

#if UNITY_EDITOR_OSX
		bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			Editor.Gizmos.Update(tDelta, leftBtnStatus, rightBtnStatus, mousePos, isCtrl, Event.current.shift, Event.current.alt);

		}



		//Bone Edit : 이전에 클릭했던 위치
		//private Vector2 _boneEdit_PrevClickPos = Vector2.zero;
		private bool _boneEdit_isFirstState = true;
		private bool _boneEdit_isMouseClickable = false;
		private bool _boneEdit_isDrawBoneGhost = false;//GUI에서 표시할 Ghost상태의 임시 Bone
		private Vector2 _boneEdit_PrevClickPosW = Vector2.zero;
		private Vector2 _boneEdit_NextGhostBonePosW = Vector2.zero;
		private apBone _boneEdit_PrevSelectedBone = null;



		public bool IsBoneEditGhostBoneDraw { get { return _boneEdit_isDrawBoneGhost; } }
		public Vector2 BoneEditGhostBonePosW_Start { get { return _boneEdit_PrevClickPosW; } }
		public Vector2 BoneEditGhostBonePosW_End { get { return _boneEdit_NextGhostBonePosW; } }
		public apBone BoneEditRollOverBone { get { return _boneEdit_rollOverBone; } }

		private apBone _boneEdit_rollOverBone = null;
		private Vector2 _boneEdit_PrevMousePosWToCheck = Vector2.zero;

		/// <summary>
		/// GUI Input에서 Bone 편집을 하기 전에 모드가 바뀌면 호출해야하는 함수.
		/// 몇가지 변수가 초기화된다.
		/// </summary>
		public void SetBoneEditInit()
		{
			_boneEdit_isFirstState = true;
			//_boneEdit_PrevClickPos = Vector2.zero;
			_boneEdit_isMouseClickable = false;
			_boneEdit_isDrawBoneGhost = false;
			_boneEdit_PrevClickPosW = Vector2.zero;
			_boneEdit_NextGhostBonePosW = Vector2.zero;

			_boneEdit_rollOverBone = null;
			_boneEdit_PrevMousePosWToCheck = Vector2.zero;

			//Debug.Log("_boneEdit_PrevClickPosW -> Zero");

			//_boneEdit_PrevSelectedBone = Editor.Select.Bone;
			_boneEdit_PrevSelectedBone = null;
		}

		public void GUI_Input_MeshGroup_Bone(float tDelta)
		{
			//본 작업을 하자
			//이전
			//apMouse.MouseBtnStatus leftBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Status;
			//apMouse.MouseBtnStatus rightBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Status;
			//Vector2 mousePos = apMouse.Pos;

			//이후
			apMouse.MouseBtnStatus leftBtnStatus = Editor.Mouse.GetStatus(apMouseSet.Button.Left, apMouseSet.ACTION.MeshGroup_Bone);
			apMouse.MouseBtnStatus rightBtnStatus = Editor.Mouse.GetStatus(apMouseSet.Button.Right, apMouseSet.ACTION.MeshGroup_Bone);
			Vector2 mousePos = Editor.Mouse.Pos;

			if (!_boneEdit_isMouseClickable)
			{
				if ((leftBtnStatus == apMouse.MouseBtnStatus.Up ||
					leftBtnStatus == apMouse.MouseBtnStatus.Released)
					&&
					(rightBtnStatus == apMouse.MouseBtnStatus.Up ||
					rightBtnStatus == apMouse.MouseBtnStatus.Released))
				{
					_boneEdit_isMouseClickable = true;
				}
			}

#if UNITY_EDITOR_OSX
		bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			apSelection.BONE_EDIT_MODE boneEditMode = Editor.Select.BoneEditMode;
			switch (boneEditMode)
			{
				case apSelection.BONE_EDIT_MODE.None:
					//아무것도 안합니더
					break;

				case apSelection.BONE_EDIT_MODE.SelectOnly:
				case apSelection.BONE_EDIT_MODE.SelectAndTRS:
					//선택 + TRS
					if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
					{
						if (_boneEdit_isMouseClickable)
						{
							Editor.GizmoController._isBoneSelect_MovePosReset = true;//클릭시에는 리셋을 해주자
							_boneEdit_isMouseClickable = false;
						}

					}
					Editor.Gizmos.Update(tDelta, leftBtnStatus, rightBtnStatus, mousePos, isCtrl, Event.current.shift, Event.current.alt);
					if (rightBtnStatus == apMouse.MouseBtnStatus.Down && IsMouseInGUI(mousePos))
					{
						if (_boneEdit_isMouseClickable)
						{
							Editor.Select.SetBone(null);
							_boneEdit_isMouseClickable = false;
						}
					}
					break;

				case apSelection.BONE_EDIT_MODE.Add:
					//"선택된 Bone"을 Parent로 하여 추가하기.
					{
						apBone curSelectedBone = Editor.Select.Bone;
						apMeshGroup curMeshGroup = Editor.Select.MeshGroup;

						if (curMeshGroup == null)
						{
							break;
						}

						bool isMouseInGUI = IsMouseInGUI(mousePos);


						//1) 처음 Add할때 : 클릭한 곳이 Start 포인트 (Parent 여부는 상관없음)
						//2) 2+ Add할때 : 클릭한 곳이 End 포인트. Start -> End로 Bone을 생성하고, End를 Start로 교체.
						//우클릭하면 (1) 상태로 돌아간다.
						//(1)에서 우클릭을 하면 Add 모드에서 해제되고 Select 모드가 된다.


						//처음 추가할때에는 선택된 본을 Parent으로 한다. (Select 모드에서 선택해야함)
						//추가한 이후에는 추가된 본을 Parent로 하여 계속 수행한다.

						//- Add는 여기서 직접 처리하자
						//- "생성 중"일때는 Ghost 본을 GUI에 출력하자

						if (_boneEdit_isFirstState)
						{
							//_boneEdit_isDrawBoneGhost = false;
							//_boneEdit_isDrawBoneGhostOnMouseMove = false;

							//만약 마우스 입력 없이
							//외부에 의해서 Bone을 바꾸었다면
							//-> Parent를 바꾸려고 한 것.
							//Parent가 바뀌었으면 위치를 자동으로 잡아주자.
							if (curSelectedBone != _boneEdit_PrevSelectedBone)
							{
								_boneEdit_PrevSelectedBone = curSelectedBone;
								_boneEdit_isFirstState = false;
								_boneEdit_isMouseClickable = false;

								curSelectedBone.MakeWorldMatrix(false);//<<이건 IK 적용 전이므로 바로 적용 가능
								curSelectedBone.GUIUpdate();
								Vector3 endPosW = curSelectedBone._shapePoint_End;

								_boneEdit_isDrawBoneGhost = true;//이제 Ghost를 Draw하자

								//_boneEdit_PrevClickPos = apGL.World2GL(endPosW);
								_boneEdit_PrevClickPosW = endPosW;
								_boneEdit_NextGhostBonePosW = _boneEdit_PrevClickPosW;
							}
							else
							{
								if (_boneEdit_isMouseClickable && isMouseInGUI)
								{

									if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
									{
										//좌클릭
										//1) 처음 Add할때 : 클릭한 곳이 Start 포인트 (Parent 여부는 상관없음)
										//_boneEdit_PrevClickPos = mousePos;//마우스 위치를 잡고
										_boneEdit_isFirstState = false;//두번째 스테이트로 바꾼다.
										_boneEdit_isMouseClickable = false;

										_boneEdit_isDrawBoneGhost = true;//이제 Ghost를 Draw하자

										_boneEdit_PrevClickPosW = apGL.GL2World(mousePos);
										_boneEdit_NextGhostBonePosW = _boneEdit_PrevClickPosW;
									}
									else if (rightBtnStatus == apMouse.MouseBtnStatus.Down)
									{
										//우클릭
										//(1)에서 우클릭을 하면 Add 모드에서 해제되고 Select 모드가 된다.
										Editor.Select.SetBoneEditMode(apSelection.BONE_EDIT_MODE.SelectAndTRS, true);
										Editor.Select.SetBone(null);
										_boneEdit_isMouseClickable = false;
										_boneEdit_isDrawBoneGhost = false;


									}
								}
							}
						}
						else
						{
							_boneEdit_NextGhostBonePosW = apGL.GL2World(mousePos);


							if (curSelectedBone != _boneEdit_PrevSelectedBone)
							{
								_boneEdit_PrevSelectedBone = curSelectedBone;
								_boneEdit_isFirstState = false;
								_boneEdit_isMouseClickable = false;

								curSelectedBone.MakeWorldMatrix(false);
								curSelectedBone.GUIUpdate();
								Vector3 endPosW = curSelectedBone._shapePoint_End;

								_boneEdit_isDrawBoneGhost = true;//이제 Ghost를 Draw하자

								//_boneEdit_PrevClickPos = apGL.World2GL(endPosW);
								_boneEdit_PrevClickPosW = endPosW;
								_boneEdit_NextGhostBonePosW = _boneEdit_PrevClickPosW;
							}

							//만약 Ctrl키를 누른다면 각도가 제한된다.
							if (isCtrl)
							{
								if ((_boneEdit_NextGhostBonePosW - _boneEdit_PrevClickPosW).sqrMagnitude > 0.0001f)
								{
									float lineAnlge = Mathf.Atan2(_boneEdit_NextGhostBonePosW.y - _boneEdit_PrevClickPosW.y,
										_boneEdit_NextGhostBonePosW.x - _boneEdit_PrevClickPosW.x) * Mathf.Rad2Deg;

									//Debug.Log("Bone Angle Ctrl : " + lineAnlge);
									float dist = (_boneEdit_NextGhostBonePosW - _boneEdit_PrevClickPosW).magnitude;
									float revSqrt2 = 1.0f / 1.414141f;
									if (lineAnlge < -180 + 22.5f)//Left
									{
										_boneEdit_NextGhostBonePosW.y = _boneEdit_PrevClickPosW.y;
									}
									else if (lineAnlge < -135 + 22.5f)//LB
									{
										_boneEdit_NextGhostBonePosW.x = _boneEdit_PrevClickPosW.x + (-revSqrt2 * dist);
										_boneEdit_NextGhostBonePosW.y = _boneEdit_PrevClickPosW.y + (-revSqrt2 * dist);
									}
									else if (lineAnlge < -90 + 22.5f)//B
									{
										_boneEdit_NextGhostBonePosW.x = _boneEdit_PrevClickPosW.x;
									}
									else if (lineAnlge < -45 + 22.5f)//RB
									{
										_boneEdit_NextGhostBonePosW.x = _boneEdit_PrevClickPosW.x + (revSqrt2 * dist);
										_boneEdit_NextGhostBonePosW.y = _boneEdit_PrevClickPosW.y + (-revSqrt2 * dist);
									}
									else if (lineAnlge < 0 + 22.5f)//R
									{
										_boneEdit_NextGhostBonePosW.y = _boneEdit_PrevClickPosW.y;
									}
									else if (lineAnlge < 45 + 22.5f)//RT
									{
										_boneEdit_NextGhostBonePosW.x = _boneEdit_PrevClickPosW.x + (revSqrt2 * dist);
										_boneEdit_NextGhostBonePosW.y = _boneEdit_PrevClickPosW.y + (revSqrt2 * dist);
									}
									else if (lineAnlge < 90 + 22.5f)//T
									{
										_boneEdit_NextGhostBonePosW.x = _boneEdit_PrevClickPosW.x;
									}
									else if (lineAnlge < 135 + 22.5f)//LT
									{
										_boneEdit_NextGhostBonePosW.x = _boneEdit_PrevClickPosW.x + (-revSqrt2 * dist);
										_boneEdit_NextGhostBonePosW.y = _boneEdit_PrevClickPosW.y + (revSqrt2 * dist);
									}
									else//Left
									{
										_boneEdit_NextGhostBonePosW.y = _boneEdit_PrevClickPosW.y;
									}

									mousePos = apGL.World2GL(_boneEdit_NextGhostBonePosW);

								}
							}



							if (_boneEdit_isMouseClickable && isMouseInGUI)
							{
								if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
								{
									//좌클릭
									//2) 2+ Add할때 : 클릭한 곳이 End 포인트. Start -> End로 Bone을 생성하고, End를 Start로 교체.
									Vector2 startPosW = _boneEdit_PrevClickPosW;
									Vector2 endPosW = _boneEdit_NextGhostBonePosW;


									//Distance가 0이면 안된다.
									if (Vector2.Distance(startPosW, endPosW) > 0.00001f)
									{

										apBone newBone = AddBone(curMeshGroup, curSelectedBone);
										//newBone._defaultMatrix

										apMatrix parentWorldMatrix = null;
										//설정을 복사하자
										if (curSelectedBone != null)
										{
											newBone._shapeWidth = curSelectedBone._shapeWidth;
											newBone._shapeTaper = curSelectedBone._shapeTaper;

											parentWorldMatrix = curSelectedBone._worldMatrix;
										}
										else
										{
											if (curMeshGroup._rootRenderUnit != null)
											{
												parentWorldMatrix = curMeshGroup._rootRenderUnit.WorldMatrixWrap;
												//parentWorldMatrix = new apMatrix();
											}
											else
											{
												parentWorldMatrix = new apMatrix();
											}

											//마지막으로 편집되 Bone Width를 적용
											if (_editor.Select._isLastBoneShapeWidthChanged)
											{
												newBone._shapeWidth = _editor.Select._lastBoneShapeWidth;
											}
										}

										//Parent 기준으로 로컬 좌표계를 구한다.
										Vector2 startPosL = parentWorldMatrix.InvMulPoint2(startPosW);
										Vector2 endPosL = parentWorldMatrix.InvMulPoint2(endPosW);

										float length = (endPosL - startPosL).magnitude;
										float angle = 0.0f;
										//start -> pos를 +Y로 삼도록 각도를 설정한다.
										if (Vector2.Distance(startPosL, endPosL) == 0.0f)
										{
											angle = -0.0f;
										}
										else
										{
											angle = Mathf.Atan2(endPosL.y - startPosL.y, endPosL.x - startPosL.x) * Mathf.Rad2Deg;
											angle += 90.0f;
										}

										angle += 180.0f;
										angle = apUtil.AngleTo180(angle);



										if (curSelectedBone != null)
										{
											curSelectedBone.LinkRecursive(curSelectedBone._level);

											//현재 본에 Child가 추가되었으므로
											//IK를 설정해주자
											//bool isIKConnectable = false;
											if (curSelectedBone._childBones.Count > 0 && curSelectedBone._optionIK == apBone.OPTION_IK.Disabled)
											{
												curSelectedBone._optionIK = apBone.OPTION_IK.IKSingle;
												curSelectedBone._IKTargetBone = curSelectedBone._childBones[0];
												curSelectedBone._IKNextChainedBone = curSelectedBone._childBones[0];

												curSelectedBone._IKTargetBoneID = curSelectedBone._IKTargetBone._uniqueID;
												curSelectedBone._IKNextChainedBoneID = curSelectedBone._IKNextChainedBone._uniqueID;
											}
										}
										else
										{
											newBone.Link(curMeshGroup, null, Editor._portrait);
										}

										newBone.InitTransform();

										newBone._shapeLength = (int)length;

										newBone._defaultMatrix.SetIdentity();
										newBone._defaultMatrix.SetPos(startPosL);
										newBone._defaultMatrix.SetRotate(angle);
										newBone._defaultMatrix.SetScale(1.0f);



										newBone.MakeWorldMatrix(false);
										newBone.GUIUpdate(false);

										//Debug.Log("-Pos : L " + startPosL + ", W " + startPosW + "\n-Angle : " + angle
										//	+ "\n-End Pos : L " + endPosL + ", W " + endPosW + "\n-Delta : L " + (endPosL - startPosL) + ", W " + (endPosW - startPosW)
										//	+ "\n-Parent World Matrix : Pos : W " + parentWorldMatrix._pos + ", Length : " + length);


										//Select 에 선택해주자
										Editor.Select.SetBone(newBone);

										RefreshBoneHierarchy(curMeshGroup);
										RefreshBoneChaining(curMeshGroup);

										Editor.Select.SetBone(newBone);

										_boneEdit_PrevSelectedBone = Editor.Select.Bone;

										Editor.RefreshControllerAndHierarchy(false);
										curMeshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();
										curMeshGroup.RefreshForce();

										//GUI가 바로 출력되면 에러가 있다.
										//다음 Layout까지 출력하지 말도록 제한하자
										Editor.SetGUIVisible(apEditor.DELAYED_UI_TYPE.GUI_MeshGroup_Hierarchy_Delayed, false);//"GUI MeshGroup Hierarchy Delayed"
									}
									//다음을 위해서 마우스 위치 갱신
									//_boneEdit_PrevClickPos = mousePos;//마우스 위치를 잡고
									_boneEdit_PrevClickPosW = apGL.GL2World(mousePos);
									_boneEdit_isMouseClickable = false;

									_boneEdit_isDrawBoneGhost = true;//이제 Ghost를 Draw하자

									Editor.SetRepaint();
								}
								else if (rightBtnStatus == apMouse.MouseBtnStatus.Down)
								{
									//우클릭
									//우클릭하면 (1) 상태로 돌아간다.
									_boneEdit_isFirstState = true;
									_boneEdit_isMouseClickable = false;

									_boneEdit_isDrawBoneGhost = false;//Ghost Draw 종료

									Editor.Select.SetBone(null);
									_boneEdit_PrevSelectedBone = Editor.Select.Bone;
									Editor.RefreshControllerAndHierarchy(false);


								}
							}
						}

						//Editor.SetRepaint();
						//Editor.SetUpdateSkip();//<<이번 업데이트는 Skip을 한다.
					}


					break;

				case apSelection.BONE_EDIT_MODE.Link:
					//선택 + 2번째 선택으로 Parent 연결
					//(Child -> Parent)
					//연결한 후에는 연결 해제
					//우클릭으로 선택 해제
					{
						apBone curSelectedBone = Editor.Select.Bone;
						apMeshGroup curMeshGroup = Editor.Select.MeshGroup;

						if (curMeshGroup == null)
						{
							break;
						}

						bool isMouseInGUI = IsMouseInGUI(mousePos);

						//1) (현재 선택한 Bone 없이) 처음 Bone을 선택할 때 : 클릭한 Bone의 World Matrix + EndPos의 중점을 시작점으로 삼는다.
						//2) 다음 Bone을 선택할 때 : 
						//            이전에 선택한 Bone -> 지금 선택한 Bone으로 Parent 연결을 시도해본다. 
						//            (실패시 Noti)
						//            본 자체 선택을 Null로 지정

						if (curSelectedBone != _boneEdit_PrevSelectedBone)
						{
							_boneEdit_PrevSelectedBone = curSelectedBone;
							_boneEdit_isFirstState = false;
							_boneEdit_isMouseClickable = false;

							if (curSelectedBone != null)
							{
								curSelectedBone.MakeWorldMatrix(false);
								curSelectedBone.GUIUpdate();
								Vector2 midPosW = (curSelectedBone._shapePoint_End + curSelectedBone._worldMatrix._pos) * 0.5f;

								_boneEdit_isDrawBoneGhost = true;//이제 Ghost를 Draw하자

								//_boneEdit_PrevClickPos = apGL.World2GL(midPosW);
								_boneEdit_PrevClickPosW = midPosW;
								_boneEdit_NextGhostBonePosW = midPosW;
							}
							else
							{
								_boneEdit_isDrawBoneGhost = false;
							}
						}

						if (curSelectedBone == null)
						{
							//이전에 선택한 Bone이 없다.
							//새로 선택을 하자

							_boneEdit_isDrawBoneGhost = false;
							_boneEdit_rollOverBone = null;

							if (_boneEdit_isMouseClickable && isMouseInGUI)
							{
								if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
								{
									//좌클릭
									//Bone을 선택할게 있는가
									//<BONE_EDIT> Bone 기본 설정은 자기 자신의 본만 선택할 수 있다. 변경 없음
									List<apBone> boneList = curMeshGroup._boneList_All;
									apBone bone = null;
									for (int i = 0; i < boneList.Count; i++)
									{
										bone = boneList[i];
										if (Editor.GizmoController.IsBoneClick(bone, apGL.GL2World(mousePos), mousePos, Editor._boneGUIRenderMode, Editor.Select.IsBoneIKRenderable))
										{
											//Debug.Log("Selected : " + bone._name);
											Editor.Select.SetBone(bone);
											break;
										}
									}

									if (Editor.Select.Bone != null)
									{
										//새로 선택을 했다.
										curSelectedBone = Editor.Select.Bone;

										Editor.RefreshControllerAndHierarchy(false);

										//_boneEdit_PrevClickPos = mousePos;//마우스 위치를 잡고
										_boneEdit_isMouseClickable = false;

										_boneEdit_isDrawBoneGhost = true;//이제 Ghost를 Draw하자

										Vector2 midPosW = (curSelectedBone._shapePoint_End + curSelectedBone._worldMatrix._pos) * 0.5f;

										_boneEdit_PrevClickPosW = midPosW;
										_boneEdit_NextGhostBonePosW = _boneEdit_PrevClickPosW;
									}
								}
								else if (rightBtnStatus == apMouse.MouseBtnStatus.Down)
								{
									//우클릭
									//(1)에서 우클릭을 하면 Add 모드에서 해제되고 Select 모드가 된다.
									Editor.Select.SetBoneEditMode(apSelection.BONE_EDIT_MODE.SelectAndTRS, true);
									Editor.Select.SetBone(null);
									_boneEdit_isMouseClickable = false;
									_boneEdit_isDrawBoneGhost = false;
								}
							}
						}
						else
						{
							//이전에 선택한 Bone이 있다.
							//다른 Bone을 선택한 후 Parent 연결을 시도하자.
							//연결 후에는 Link를 종료. (Link 여러번 할게 있나?)

							_boneEdit_isDrawBoneGhost = true;
							Vector2 curMousePosW = apGL.GL2World(mousePos);
							float deltaMousePos = Vector2.Distance(curMousePosW, _boneEdit_PrevMousePosWToCheck);
							_boneEdit_NextGhostBonePosW = curMousePosW;

							if (deltaMousePos > 2.0f)
							{
								_boneEdit_PrevMousePosWToCheck = curMousePosW;

								//다시 "가까운 롤오버된 Bone 찾기"
								List<apBone> boneList = curMeshGroup._boneList_All;
								apBone bone = null;
								_boneEdit_rollOverBone = null;
								for (int i = 0; i < boneList.Count; i++)
								{
									bone = boneList[i];
									if (Editor.GizmoController.IsBoneClick(bone, apGL.GL2World(mousePos), mousePos, Editor._boneGUIRenderMode, Editor.Select.IsBoneIKRenderable))
									{
										_boneEdit_rollOverBone = bone;
										break;
									}
								}
							}

							//여기서 클릭을 하면 Parent를 바꾸고 -> CurSelectBone을 교체하자
							//우클릭시 단순히 선택 Bone 해제
							if (_boneEdit_isMouseClickable && isMouseInGUI)
							{
								if (leftBtnStatus == apMouse.MouseBtnStatus.Down)
								{
									//Parent로 이을 Bone을 검색하자
									List<apBone> boneList = curMeshGroup._boneList_All;
									apBone bone = null;
									apBone targetBone = null;
									for (int i = 0; i < boneList.Count; i++)
									{
										bone = boneList[i];
										if (Editor.GizmoController.IsBoneClick(bone, apGL.GL2World(mousePos), mousePos, Editor._boneGUIRenderMode, Editor.Select.IsBoneIKRenderable))
										{
											targetBone = bone;
											break;
										}
									}
									if (targetBone != null)
									{
										//TODO : 가능한지 체크하자
										//Parent를 바꿀때에는
										//targetBone이 재귀적인 Child이면 안된다
										bool isChangeAvailable = true;
										if (curSelectedBone == targetBone)
										{
											isChangeAvailable = false;
										}
										else if (curSelectedBone._parentBone == targetBone)
										{
											isChangeAvailable = false;
										}
										else if (curSelectedBone.GetChildBoneRecursive(targetBone._uniqueID) != null)
										{
											isChangeAvailable = false;
										}

										//가능 여부에 따라서 처리
										if (isChangeAvailable)
										{
											//교체한다.
											SetBoneAsParent(curSelectedBone, targetBone);
											Editor.Notification(curSelectedBone._name + " became a child of " + targetBone._name, true, false);
										}
										else
										{
											//안된다고 에디터 노티로 띄워주자
											Editor.Notification("A Bone that can not be selected as a Parent. Detach first.", true, false);
										}

									}

									if (targetBone != null)
									{
										//처리가 끝났으면 Bone 교체
										Editor.Select.SetBone(targetBone);

										curSelectedBone.MakeWorldMatrix(false);
										curSelectedBone.GUIUpdate();
										Vector3 endPosW = curSelectedBone._shapePoint_End;

										_boneEdit_isDrawBoneGhost = true;//이제 Ghost를 Draw하자

										//_boneEdit_PrevClickPos = apGL.World2GL(endPosW);
										_boneEdit_PrevClickPosW = endPosW;
										_boneEdit_NextGhostBonePosW = _boneEdit_PrevClickPosW;
									}
									else
									{
										//우클릭 한것과 동일하게 작동
										Editor.Select.SetBone(null);
										_boneEdit_isDrawBoneGhost = false;
										_boneEdit_rollOverBone = null;
									}

									_boneEdit_isMouseClickable = false;

								}
								else if (rightBtnStatus == apMouse.MouseBtnStatus.Down)
								{
									_boneEdit_isMouseClickable = false;
									_boneEdit_isDrawBoneGhost = false;

									Editor.Select.SetBone(null);
									_boneEdit_rollOverBone = null;
								}
							}
						}

						//Editor.SetRepaint();
						//Editor.SetUpdateSkip();//<<이번 업데이트는 Skip을 한다.

						//우클릭을 한번 하면 선택 취소.
						//선택 취소된 상태에서 누르면 모드 취소
					}
					break;
			}
		}



		//public bool IsBoneClickable(Vector2 posGL, apBone bone)
		//{

		//	Vector2 posW = apGL.GL2World(posGL);

		//	//5각형 (Taper가 100일땐 4각형)의 클릭 체크
		//	if (apEditorUtil.IsPointInTri(posW, bone._worldMatrix._pos, bone._shapePoint_Mid1, bone._shapePoint_End1))
		//	{
		//		return true;
		//	}

		//	if (apEditorUtil.IsPointInTri(posW, bone._worldMatrix._pos, bone._shapePoint_Mid2, bone._shapePoint_End2))
		//	{
		//		return true;
		//	}

		//	if (bone._shapeTaper < 100)
		//	{
		//		if (apEditorUtil.IsPointInTri(posW, bone._worldMatrix._pos, bone._shapePoint_End1, bone._shapePoint_End2))
		//		{
		//			return true;
		//		}
		//	}

		//	return false;

		//}


		public void GUI_Input_MeshGroup_Modifier(apModifierBase.MODIFIER_TYPE modifierType, float tDelta)
		{
			apMouse.MouseBtnStatus leftBtnStatus = Editor.Mouse.GetStatus(apMouseSet.Button.Left, apMouseSet.ACTION.MeshGroup_Modifier);
			apMouse.MouseBtnStatus rightBtnStatus = Editor.Mouse.GetStatus(apMouseSet.Button.Right, apMouseSet.ACTION.MeshGroup_Modifier);
			Vector2 mousePos = Editor.Mouse.Pos;

			if (modifierType == apModifierBase.MODIFIER_TYPE.Base)
			{
				return;
			}

#if UNITY_EDITOR_OSX
		bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			Editor.Gizmos.Update(tDelta, leftBtnStatus, rightBtnStatus, mousePos, isCtrl, Event.current.shift, Event.current.alt);
		}

		public void GUI_Input_Animation(float tDelta)
		{
			//이전
			//apMouse.MouseBtnStatus leftBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_LEFT].Status;
			//apMouse.MouseBtnStatus rightBtnStatus = Editor._mouseBtn[Editor.MOUSE_BTN_RIGHT].Status;
			//Vector2 mousePos = apMouse.Pos;

			//이후
			apMouse.MouseBtnStatus leftBtnStatus = Editor.Mouse.GetStatus(apMouseSet.Button.Left, apMouseSet.ACTION.MeshGroup_Animation);
			apMouse.MouseBtnStatus rightBtnStatus = Editor.Mouse.GetStatus(apMouseSet.Button.Right, apMouseSet.ACTION.MeshGroup_Animation);
			Vector2 mousePos = Editor.Mouse.Pos;

#if UNITY_EDITOR_OSX
		bool isCtrl = Event.current.command;
#else
			bool isCtrl = Event.current.control;
#endif

			Editor.Gizmos.Update(tDelta, leftBtnStatus, rightBtnStatus, mousePos, isCtrl, Event.current.shift, Event.current.alt);
		}

		//--------------------------------------------------
		// 2. 임시 변수 제어
		//--------------------------------------------------
		public void InitTmpValues()
		{
			Editor._tmpValues.Clear();
		}

		public int GetTmpValue(string keyName, int defaultValue)
		{
			if (Editor._tmpValues.ContainsKey(keyName))
			{
				return Editor._tmpValues[keyName];
			}

			Editor._tmpValues.Add(keyName, defaultValue);
			return defaultValue;
		}

		public void SetTmpValue(string keyName, int setvalue)
		{
			if (Editor._tmpValues.ContainsKey(keyName))
			{
				Editor._tmpValues[keyName] = setvalue;
				return;
			}
			Editor._tmpValues.Add(keyName, setvalue);
		}

		//--------------------------------------------------
		// 3-0. 초기화
		//--------------------------------------------------
		public void PortraitReadyToEdit()
		{
			if (Editor._portrait == null)
			{
				return;
			}

			//추가 19.6.3 : MaterialSet 데이터를 갱신해야한다.
			LinkMaterialSets();

			Editor._portrait.ReadyToEdit();

			//추가
			//썸네일을 찾아서 연결해보자
			string thumnailPath = Editor._portrait._imageFilePath_Thumbnail;
			if (string.IsNullOrEmpty(thumnailPath))
			{
				Editor._portrait._thumbnailImage = null;
			}
			else
			{
				Texture2D thumnailImage = AssetDatabase.LoadAssetAtPath<Texture2D>(thumnailPath);
				Editor._portrait._thumbnailImage = thumnailImage;
			}

			RefreshMeshGroups();

			//Selection.activeGameObject = Editor.Select.Portrait.gameObject;
			Selection.activeGameObject = null;//<<선택을 해제해준다. 프로파일러를 도와줘야져

			//이름을 갱신하자
			CheckAndRefreshGameObjectNames(Editor._portrait);

			//추가 : 삭제되어야 하는 데이터가 있을 수도 있다. 검색하자
			//CheckAndRemoveUnusedModifierData(Editor._portrait, true, true);//<<보류


			

			//SetDirty를 해주자
			apEditorUtil.SetEditorDirty();
		}

		//--------------------------------------------------
		// 3-1. 객체 참조
		//--------------------------------------------------
		public apTextureData GetTextureData(int uniqueID)
		{
			if (Editor._portrait == null)
			{
				return null;
			}

			return Editor._portrait._textureData.Find(delegate (apTextureData a)
			{
				return a._uniqueID == uniqueID;
			});
		}

		public apMesh GetMesh(int uniqueID)
		{
			if (Editor._portrait == null)
			{
				return null;
			}

			return Editor._portrait._meshes.Find(delegate (apMesh a)
			{
				return a._uniqueID == uniqueID;
			});
		}


		public apMeshGroup GetMeshGroup(int uniqueID)
		{
			if (Editor._portrait == null)
			{
				return null;
			}

			return Editor._portrait._meshGroups.Find(delegate (apMeshGroup a)
			{
				return a._uniqueID == uniqueID;
			});
		}


		public apControlParam GetControlParam(string controlKeyName)
		{
			if (Editor._portrait == null)
			{
				return null;
			}
			return Editor._portrait._controller.FindParam(controlKeyName);
		}




		//--------------------------------------------------
		// 3. 객체의 추가 / 삭제
		//--------------------------------------------------
		/// <summary>
		/// Mesh와 MeshGroup은 Monobehaviour로 저장해야한다.
		/// 해당 GameObject가 포함될 Group이 있어야 Monobehaviour를 추가할 수 있다.
		/// 존재하면 추가하지 않는다.
		/// 모든 AddMesh/AddMeshGroup 함수 전에 호출한다.
		/// </summary>
		public void CheckAndMakeObjectGroup()
		{
			if (Editor._portrait == null)
			{
				return;
			}

			apPortrait portrait = Editor._portrait;

			if (portrait._subObjectGroup == null)
			{
				portrait._subObjectGroup = new GameObject("EditorObjects");
				portrait._subObjectGroup.transform.parent = portrait.transform;
				portrait._subObjectGroup.transform.localPosition = Vector3.zero;
				portrait._subObjectGroup.transform.localRotation = Quaternion.identity;
				portrait._subObjectGroup.transform.localScale = Vector3.one;
				portrait._subObjectGroup.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			}

			if (portrait._subObjectGroup_Mesh == null)
			{
				portrait._subObjectGroup_Mesh = new GameObject("Meshes");
				portrait._subObjectGroup_Mesh.transform.parent = portrait._subObjectGroup.transform;
				portrait._subObjectGroup_Mesh.transform.localPosition = Vector3.zero;
				portrait._subObjectGroup_Mesh.transform.localRotation = Quaternion.identity;
				portrait._subObjectGroup_Mesh.transform.localScale = Vector3.one;
				portrait._subObjectGroup_Mesh.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			}

			if (portrait._subObjectGroup_MeshGroup == null)
			{
				portrait._subObjectGroup_MeshGroup = new GameObject("MeshGroups");
				portrait._subObjectGroup_MeshGroup.transform.parent = portrait._subObjectGroup.transform;
				portrait._subObjectGroup_MeshGroup.transform.localPosition = Vector3.zero;
				portrait._subObjectGroup_MeshGroup.transform.localRotation = Quaternion.identity;
				portrait._subObjectGroup_MeshGroup.transform.localScale = Vector3.one;
				portrait._subObjectGroup_MeshGroup.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			}

			if (portrait._subObjectGroup_Modifier == null)
			{
				portrait._subObjectGroup_Modifier = new GameObject("Modifiers");
				portrait._subObjectGroup_Modifier.transform.parent = portrait._subObjectGroup.transform;
				portrait._subObjectGroup_Modifier.transform.localPosition = Vector3.zero;
				portrait._subObjectGroup_Modifier.transform.localRotation = Quaternion.identity;
				portrait._subObjectGroup_Modifier.transform.localScale = Vector3.one;
				portrait._subObjectGroup_Modifier.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			}

			//임시로 HideFlag를 풀자
			//portrait._subObjectGroup.hideFlags = HideFlags.None;
			//portrait._subObjectGroup_Mesh.hideFlags = HideFlags.None;
			//portrait._subObjectGroup_MeshGroup.hideFlags = HideFlags.None;
			//portrait._subObjectGroup_Modifier.hideFlags = HideFlags.None;

			//다시 잠그자
			portrait._subObjectGroup.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
			portrait._subObjectGroup_Mesh.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
			portrait._subObjectGroup_MeshGroup.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
			portrait._subObjectGroup_Modifier.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
		}


		/// <summary>
		/// 오브젝트를 삭제하는 다이얼로그에 사용될 메시지를 만든다.
		/// </summary>
		/// <param name="portrait"></param>
		/// <param name="removingItem"></param>
		/// <param name="nMaximumLines"></param>
		/// <param name="baseMsg"></param>
		/// <param name="warningMsg"></param>
		/// <returns></returns>
		public string GetRemoveItemMessage(apPortrait portrait, object removingItem, int nMaximumLines, string baseMsg, string warningMsg)
		{
			string strChangedItems = Editor.Controller.GetChangedItemsWhenRemoving(portrait, removingItem, nMaximumLines);

			if (!string.IsNullOrEmpty(strChangedItems))
			{
				return baseMsg
					+ "\n\n"
					+ warningMsg
					+ "\n"
					+ strChangedItems;
			}
			else
			{
				return baseMsg;
			}
		}

		/// <summary>
		/// 어떤 항목을 삭제할 때, 이와 연관이 있는 항목들을 모두 열거하는 함수
		/// 지원하는 것은 TextureData, Mesh, MeshGroup, MeshTrasnform, MeshGroupTransform, Modifier
		/// 체크 순서는 TextureData -> Mesh -> MeshGroup -> Transform -> Modifier -> AnimClip 순이다.
		/// 연관된게 없으면 null 리턴
		/// </summary>
		/// <param name="removingItem"></param>
		/// <returns></returns>
		private string GetChangedItemsWhenRemoving(apPortrait portrait, object removingItem, int nMaximumLines)
		{
			if (portrait == null || removingItem == null)
			{
				return null;
			}

			string strResult = "";


			//삭제 확인 -> 
			//Texture -> Mesh -> 연결된 MeshGroup들까지
			//Mesh -> (모든 메시 그룹) + MeshTransform
			//MeshGroup -> 1. (다른 모든 메시 그룹) + MeshGroupTransform / 2. MeshGroup이 연동된 AnimClip
			//MeshTransform / MeshGroupTransform -> 1. (직접 삭제되는 경우) 메시 그룹 / 2. 연결된 Modifier -> 연결된 Timeline을 가진 AnimClip
			//Bone -> 연결된 Modifier -> 연결된 Timeline을 가진 AnimClip
			//Modifier -> 연결된 Timeline을 가진 AnimClip
			//Control Param -> 1. Control Param과 연결된 모든 MeshGroup을 찾는다. / 2. Control Param 타입의 애니메이션 클립

			List<string> resultLogs = new List<string>();

			if (removingItem is apTextureData)
			{
				apTextureData removedTextureData = removingItem as apTextureData;
				if (removedTextureData != null)
				{
					//1. Mesh 찾기 -> 2. 그 Mesh가 연결된 MeshGroup 찾기
					List<apMesh> rel_Meshes = new List<apMesh>();
					int logs = GetChangedMeshesByTextureData(portrait, removedTextureData, rel_Meshes, resultLogs);

					if (logs > 0 && logs < nMaximumLines)
					{
						for (int iMesh = 0; iMesh < rel_Meshes.Count; iMesh++)
						{
							GetChangedMeshGroupsByMesh(portrait, rel_Meshes[iMesh], null, resultLogs);
						}
					}
				}
			}
			else if (removingItem is apMesh)
			{
				apMesh removedMesh = removingItem as apMesh;

				if (removedMesh != null)
				{
					//1. Mesh가 연결된 MeshGroup + TF 찾기
					GetChangedMeshGroupsByMesh(portrait, removedMesh, null, resultLogs);
				}
			}
			else if (removingItem is apMeshGroup)
			{
				apMeshGroup removedMeshGroup = removingItem as apMeshGroup;

				if (removedMeshGroup != null)
				{
					//MeshGroup -> 1. (다른 모든 메시 그룹) + MeshGroupTransform / 2. MeshGroup이 연동된 AnimClip
					GetChangedMeshGroupsByMeshGroup(portrait, removedMeshGroup, null, resultLogs);
					GetChangedAnimClipsByMeshGroup(portrait, removedMeshGroup, null, resultLogs);
				}
			}
			else if (removingItem is apTransform_Mesh)
			{
				apTransform_Mesh removedMeshTransform = removingItem as apTransform_Mesh;

				//MeshTransform / MeshGroupTransform -> 1. (직접 삭제되는 경우) 메시 그룹 / 2. 연결된 Modifier -> 연결된 Timeline을 가진 AnimClip
				if (removedMeshTransform != null)
				{
					apMeshGroup parentMeshGroup = portrait._meshGroups.Find(delegate (apMeshGroup a)
					{
						return a.GetMeshTransform(removedMeshTransform._transformUniqueID) != null;
					});

					if (parentMeshGroup != null)
					{
						resultLogs.Add("[MeshGroup] " + parentMeshGroup.name);

						//연결된 Modifier를 찾자
						GetChangedModifiersBySubObject(portrait, parentMeshGroup,
													removedMeshTransform,
													null,
													null,
													null,
													null, resultLogs);

						if (resultLogs.Count < nMaximumLines)
						{
							//연결된 AnimClip을 찾자
							GetChangedAnimClipsBySubObject(portrait, parentMeshGroup,
														removedMeshTransform,
														null,
														null,
														null,
														null,
														null, resultLogs);
						}
					}
				}
			}
			else if (removingItem is apTransform_MeshGroup)
			{
				apTransform_MeshGroup removedMeshGroupTransform = removingItem as apTransform_MeshGroup;

				//MeshTransform / MeshGroupTransform -> 1. (직접 삭제되는 경우) 메시 그룹 / 2. 연결된 Modifier -> 연결된 Timeline을 가진 AnimClip
				if (removedMeshGroupTransform != null)
				{
					apMeshGroup parentMeshGroup = portrait._meshGroups.Find(delegate (apMeshGroup a)
					{
						return a.GetMeshTransform(removedMeshGroupTransform._transformUniqueID) != null;
					});

					if (parentMeshGroup != null)
					{
						resultLogs.Add("[MeshGroup] " + parentMeshGroup.name);

						//연결된 Modifier를 찾자
						GetChangedModifiersBySubObject(portrait, parentMeshGroup,
													null,
													removedMeshGroupTransform,
													null,
													null,
													null, resultLogs);

						if (resultLogs.Count < nMaximumLines)
						{
							//연결된 AnimClip을 찾자
							GetChangedAnimClipsBySubObject(portrait, parentMeshGroup,
														null,
														removedMeshGroupTransform,
														null,
														null,
														null,
														null, resultLogs);
						}
					}
				}
			}
			else if (removingItem is apBone)
			{
				apBone removedBone = removingItem as apBone;

				//Bone -> 연결된 Modifier -> 연결된 Timeline을 가진 AnimClip
				if (removedBone != null)
				{
					if (removedBone._meshGroup != null)
					{
						apMeshGroup parentMeshGroup = removedBone._meshGroup;

						resultLogs.Add("[MeshGroup] " + parentMeshGroup.name);

						//연결된 Modifier를 찾자
						GetChangedModifiersBySubObject(portrait, parentMeshGroup,
													null,
													null,
													removedBone,
													null,
													null, resultLogs);

						if (resultLogs.Count < nMaximumLines)
						{
							//연결된 AnimClip을 찾자
							GetChangedAnimClipsBySubObject(portrait, parentMeshGroup,
														null,
														null,
														removedBone,
														null,
														null,
														null, resultLogs);
						}
					}
				}
			}
			else if (removingItem is apModifierBase)
			{
				apModifierBase removedModifier = removingItem as apModifierBase;

				//Modifier -> 연결된 Timeline을 가진 AnimClip
				if (removedModifier != null)
				{
					if (removedModifier._meshGroup != null)
					{
						apMeshGroup parentMeshGroup = removedModifier._meshGroup;

						resultLogs.Add("[MeshGroup] " + parentMeshGroup.name);

						//연결된 AnimClip을 찾자
						GetChangedAnimClipsBySubObject(portrait, parentMeshGroup,
														null,
														null,
														null,
														removedModifier,
														null,
														null, resultLogs);
					}
				}
			}
			else if (removingItem is apControlParam)
			{
				apControlParam removedControlParam = removingItem as apControlParam;

				//Control Param -> 1. Control Param과 연결된 모든 MeshGroup을 찾는다. / 2. Control Param 타입의 애니메이션 클립
				if (removedControlParam != null)
				{
					for (int iMeshGroup = 0; iMeshGroup < portrait._meshGroups.Count; iMeshGroup++)
					{
						apMeshGroup meshGroup = portrait._meshGroups[iMeshGroup];

						GetChangedModifiersBySubObject(portrait, meshGroup,
														null,
														null,
														null,
														removedControlParam,
														null, resultLogs);

						if (resultLogs.Count > nMaximumLines)
						{
							break;
						}
					}

					if (resultLogs.Count <= nMaximumLines)
					{
						for (int iMeshGroup = 0; iMeshGroup < portrait._meshGroups.Count; iMeshGroup++)
						{
							apMeshGroup meshGroup = portrait._meshGroups[iMeshGroup];

							//AnimClip을 찾자
							GetChangedAnimClipsBySubObject(portrait, meshGroup,
														null,
														null,
														null,
														null,
														removedControlParam,
														null, resultLogs);

							if (resultLogs.Count > nMaximumLines)
							{
								break;
							}
						}
					}
				}

			}
			else
			{
				return null;
			}

			//로그를 정리하자
			//최대 개수를 넘을 때 : 최대 개수 -1 + ...
			//최대 개수와 같을 때 : 최대 개수
			if (resultLogs.Count > nMaximumLines)
			{
				for (int iLog = 0; iLog < resultLogs.Count; iLog++)
				{
					strResult += resultLogs[iLog] + "\n";
					if (iLog >= nMaximumLines - 2)
					{
						break;
					}
				}
				strResult += "...";
			}
			else
			{
				for (int iLog = 0; iLog < resultLogs.Count; iLog++)
				{
					strResult += resultLogs[iLog];
					if (iLog < resultLogs.Count - 1)
					{
						strResult += "\n";
					}
				}
			}

			return strResult;

		}

		/// <summary>TextureData를 포함하는 Mesh를 찾는다.</summary>
		private int GetChangedMeshesByTextureData(apPortrait portrait, apTextureData textureData, List<apMesh> resultMeshes, List<string> resultLogs)
		{
			if (portrait == null || textureData == null)
			{
				return 0;
			}
			int nResult = 0;
			for (int i = 0; i < portrait._meshes.Count; i++)
			{
				if (portrait._meshes[i].LinkedTextureDataID == textureData._uniqueID)
				{
					resultMeshes.Add(portrait._meshes[i]);
					resultLogs.Add("[Mesh] " + portrait._meshes[i]._name);
					nResult++;
				}
			}
			return nResult;
		}

		/// <summary>Mesh를 포함하는 MeshGroup들을 찾는다. (MeshTransform을 포함)</summary>
		private int GetChangedMeshGroupsByMesh(apPortrait portrait, apMesh mesh, List<apTransform_Mesh> resultMeshTransforms, List<string> resultLogs)
		{
			if (portrait == null || mesh == null)
			{
				return 0;
			}

			int nResult = 0;
			for (int iMG = 0; iMG < portrait._meshGroups.Count; iMG++)
			{
				apMeshGroup meshGroup = portrait._meshGroups[iMG];
				for (int iMeshTransform = 0; iMeshTransform < meshGroup._childMeshTransforms.Count; iMeshTransform++)
				{
					apTransform_Mesh meshTransform = meshGroup._childMeshTransforms[iMeshTransform];
					if (meshTransform._meshUniqueID == mesh._uniqueID)
					{
						if (resultMeshTransforms != null)
						{
							resultMeshTransforms.Add(meshTransform);
						}
						resultLogs.Add("[MeshGroup] " + meshGroup._name + " - " + meshTransform._nickName);
						nResult++;
					}
				}
			}
			return nResult;
		}

		/// <summary>MeshGroup를 포함하는 다른 MeshGroup들을 찾는다. (MeshGroupTransform을 포함)</summary>
		private int GetChangedMeshGroupsByMeshGroup(apPortrait portrait, apMeshGroup meshGroup, List<apTransform_MeshGroup> resultMeshGroupTransforms, List<string> resultLogs)
		{
			if (portrait == null || meshGroup == null)
			{
				return 0;
			}

			int nResult = 0;
			for (int iMG = 0; iMG < portrait._meshGroups.Count; iMG++)
			{
				apMeshGroup otherMeshGroup = portrait._meshGroups[iMG];
				if (otherMeshGroup == meshGroup)
				{
					continue;
				}
				for (int iMeshTransform = 0; iMeshTransform < otherMeshGroup._childMeshGroupTransforms.Count; iMeshTransform++)
				{
					apTransform_MeshGroup meshGroupTransform = otherMeshGroup._childMeshGroupTransforms[iMeshTransform];
					if (meshGroupTransform._meshGroupUniqueID == meshGroup._uniqueID)
					{
						if (resultMeshGroupTransforms != null)
						{
							resultMeshGroupTransforms.Add(meshGroupTransform);
						}
						resultLogs.Add("[MeshGroup] " + otherMeshGroup._name + " - " + meshGroupTransform._nickName);
						nResult++;
					}
				}
			}
			return nResult;
		}




		/// <summary>MeshGroup를 포함하는 AnimClip을 찾는다.</summary>
		private int GetChangedAnimClipsByMeshGroup(apPortrait portrait, apMeshGroup meshGroup, List<apAnimClip> resultAnimClip, List<string> resultLogs)
		{
			if (portrait == null || meshGroup == null)
			{
				return 0;
			}

			int nResult = 0;
			for (int iAnimClip = 0; iAnimClip < portrait._animClips.Count; iAnimClip++)
			{
				apAnimClip animClip = portrait._animClips[iAnimClip];
				if (animClip._targetMeshGroupID == meshGroup._uniqueID)
				{
					if (resultAnimClip != null)
					{
						resultAnimClip.Add(animClip);
					}
					resultLogs.Add("[Animation Clip] " + animClip._name);
					nResult++;
				}
			}
			return nResult;
		}

		/// <summary>MeshGroup를 포함하며 Transform이나 Bone이나 ControlParam 또는 Modifier를 가지고있는 AnimClip을 찾는다.</summary>
		private int GetChangedAnimClipsBySubObject(apPortrait portrait, apMeshGroup meshGroup,
													apTransform_Mesh target_MeshTransform,
													apTransform_MeshGroup target_MeshGroupTransform,
													apBone target_Bone,
													apModifierBase target_Modifier,
													apControlParam target_ControlParam,
													List<apAnimClip> resultAnimClip, List<string> resultLogs)
		{
			if (portrait == null || meshGroup == null)
			{
				return 0;
			}

			int nResult = 0;
			for (int iAnimClip = 0; iAnimClip < portrait._animClips.Count; iAnimClip++)
			{
				apAnimClip animClip = portrait._animClips[iAnimClip];
				if (animClip._targetMeshGroupID != meshGroup._uniqueID)
				{
					continue;
				}

				bool isChangedAnimClip = false;

				//조건이 맞는 Timeline이나 TimelineLayer중 하나라도 있다면 AnimClip은 연관성이 있다.
				for (int iTimeline = 0; iTimeline < animClip._timelines.Count; iTimeline++)
				{
					apAnimTimeline timeline = animClip._timelines[iTimeline];
					if (timeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
					{
						//Modifier 꼬는 Transform/Bone을 찾고자 할 때
						if (target_Modifier != null)
						{
							//Modifier가 연결되는가
							if (target_Modifier == timeline._linkedModifier)
							{
								isChangedAnimClip = true;
								break;
							}
						}
						else
						{
							for (int iLayer = 0; iLayer < timeline._layers.Count; iLayer++)
							{
								apAnimTimelineLayer timelineLayer = timeline._layers[iLayer];
								if (target_MeshTransform != null)
								{
									//MeshTransform을 체크하자
									if (target_MeshTransform._transformUniqueID == timelineLayer._transformID)
									{
										isChangedAnimClip = true;
										break;
									}
								}
								else if (target_MeshGroupTransform != null)
								{
									//MeshGroupTransform을 체크하자
									if (target_MeshGroupTransform._transformUniqueID == timelineLayer._transformID)
									{
										isChangedAnimClip = true;
										break;
									}
								}
								else if (target_Bone != null)
								{
									//Bone을 체크하자
									if (target_Bone._uniqueID == timelineLayer._boneID)
									{
										isChangedAnimClip = true;
										break;
									}
								}
							}

							if (isChangedAnimClip)
							{
								break;
							}
						}
					}
					else if (timeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
					{
						//ControlParam을 찾고자 할 때
						if (target_ControlParam != null)
						{
							for (int iLayer = 0; iLayer < timeline._layers.Count; iLayer++)
							{
								apAnimTimelineLayer timelineLayer = timeline._layers[iLayer];
								if (timelineLayer._controlParamID == target_ControlParam._uniqueID)
								{
									//Control Param에 해당한다.
									isChangedAnimClip = true;
									break;
								}
							}
							if (isChangedAnimClip)
							{
								break;
							}
						}
					}
				}

				if (isChangedAnimClip)
				{
					if (resultAnimClip != null)
					{
						resultAnimClip.Add(animClip);
					}
					resultLogs.Add("[Animation Clip] " + animClip._name);
					nResult++;
				}

			}
			return nResult;
		}

		/// <summary>MeshGroup를 포함하며 Transform이나 Bone이나 ControlParam 또는 Modifier를 가지고있는 AnimClip을 찾는다.</summary>
		private int GetChangedModifiersBySubObject(apPortrait portrait, apMeshGroup meshGroup,
													apTransform_Mesh target_MeshTransform,
													apTransform_MeshGroup target_MeshGroupTransform,
													apBone target_Bone,
													apControlParam target_ControlParam,
													List<apModifierBase> resultModifier, List<string> resultLogs)
		{
			if (portrait == null || meshGroup == null)
			{
				return 0;
			}

			int nResult = 0;
			for (int iMod = 0; iMod < meshGroup._modifierStack._modifiers.Count; iMod++)
			{
				apModifierBase modifier = meshGroup._modifierStack._modifiers[iMod];

				bool isChangedModifier = false;

				if (modifier.SyncTarget == apModifierParamSetGroup.SYNC_TARGET.Controller)
				{
					if (target_ControlParam != null)
					{
						//Control Param에 연결되는지 체크
						for (int iPSG = 0; iPSG < modifier._paramSetGroup_controller.Count; iPSG++)
						{
							apModifierParamSetGroup paramSetGroup = modifier._paramSetGroup_controller[iPSG];
							if (paramSetGroup._keyControlParamID == target_ControlParam._uniqueID)
							{
								//ControlParam이 포함된다.
								isChangedModifier = true;
								break;
							}
						}
					}
				}
				else
				{
					//Transform, Bone에 연결되는지 체크
					for (int iPSG = 0; iPSG < modifier._paramSetGroup_controller.Count; iPSG++)
					{
						apModifierParamSetGroup paramSetGroup = modifier._paramSetGroup_controller[iPSG];
						if (target_MeshTransform != null)
						{
							if (paramSetGroup._syncTransform_Mesh.Contains(target_MeshTransform))
							{
								//MeshTransform이 포함된다.
								isChangedModifier = true;
								break;
							}
						}
						else if (target_MeshGroupTransform != null)
						{
							if (paramSetGroup._syncTransform_MeshGroup.Contains(target_MeshGroupTransform))
							{
								//MeshGroupTransform이 포함된다.
								isChangedModifier = true;
								break;
							}
						}
						else if (target_Bone != null)
						{
							if (paramSetGroup._syncBone.Contains(target_Bone))
							{
								//Bone이 포함된다.
								isChangedModifier = true;
								break;
							}
						}
						else
						{
							continue;
						}

					}
				}

				if (isChangedModifier)
				{
					if (resultModifier != null)
					{
						resultModifier.Add(modifier);
					}
					if (target_ControlParam != null)
					{
						//ControlParam인 경우 MeshGroup + Modifier의 이름으로 알려주자
						resultLogs.Add("[MeshGroup] " + meshGroup.name + " - " + modifier.DisplayName);
					}
					else
					{
						resultLogs.Add("[Modifier] " + modifier.DisplayName);
					}

					nResult++;
					break;
				}

			}

			return nResult;
		}


		//-------------------------------------------------------------------------------

		#region [미사용 코드]
		///// <summary>
		///// 이미지를 삭제한다.
		///// </summary>
		///// <param name="iRemove"></param>
		//public void RemoveImage(int iRemove)
		//{
		//	if (iRemove >= 0 && iRemove < Editor._portrait._textureData.Count)
		//	{
		//		apEditorUtil.SetRecord("Remove Image", Editor._portrait);

		//		apTextureData removedTextureData = Editor._portrait._textureData[iRemove];
		//		if (removedTextureData == Editor.Select.TextureData)
		//		{
		//			Editor.Select.SetNone();//Select된 이미지라면 None으로 바꾸자
		//		}
		//		//int removedID = 
		//		Editor._portrait._textureData.RemoveAt(iRemove);
		//		Editor._portrait.SortTextureData();
		//	}
		//} 
		#endregion

		public void RemoveTexture(apTextureData textureData)
		{
			//Undo - Remove Image
			//apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Main_RemoveImage, Editor, Editor._portrait, textureData, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Image");


			if (textureData == Editor.Select.TextureData)
			{
				Editor.Select.SetNone();//Select된 이미지라면 None으로 바꾸자
			}

			int removedUniqueID = textureData._uniqueID;

			//Editor._portrait.PushUniqueID_Texture(removedUniqueID);
			Editor._portrait.PushUnusedID(apIDManager.TARGET.Texture, removedUniqueID);


			Editor._portrait._textureData.Remove(textureData);
			//Editor._portrait.SortTextureData();


			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			//Debug.Log("Remove Texture");
			Editor._portrait.LinkAndRefreshInEditor(true, null);

			Editor.Hierarchy.SetNeedReset();
			//Editor.Hierarchy.RefreshUnits();
			Editor.RefreshControllerAndHierarchy(false);

		}

		/// <summary>
		/// 이미지를 추가한다.
		/// </summary>
		public apTextureData AddImage()
		{
			//int nextID = Editor._portrait.MakeUniqueID_Texture();
			int nextID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.Texture);
			if (nextID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Texture Add Failed. Please Retry", "Close");
				EditorUtility.DisplayDialog(Editor.GetText(TEXT.AddTextureFailed_Title),
												Editor.GetText(TEXT.AddTextureFailed_Body),
												Editor.GetText(TEXT.Close));

				return null;
			}

			//Undo - Add Image
			apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Main_AddImage, Editor, Editor._portrait, null, false);

			//apTextureData newTexture = new apTextureData(Editor._portrait._textureData.Count);

			apTextureData newTexture = new apTextureData(nextID);

			Editor._portrait._textureData.Add(newTexture);
			Editor.Select.SetImage(newTexture);//<<Selection에도 추가
											   //Editor._portrait.SortTextureData();

			//Editor.Hierarchy.RefreshUnits();
			Editor.RefreshControllerAndHierarchy(false);

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			return newTexture;
		}


		private object _psdDialogLoadKey = null;
		private object _psdReimportDialogLoadKey = null;

		/// <summary>
		/// PSD 툴을 호출하여 자동 생성기를 돌린다.
		/// </summary>
		public void ShowPSDLoadDialog()
		{
			if (apVersion.I.IsDemo)
			{
				//데모 버전에서는 PSD Load를 지원하지 않습니다.
				EditorUtility.DisplayDialog(
					Editor.GetText(TEXT.DemoLimitation_Title),
					Editor.GetText(TEXT.DemoLimitation_Body),
					Editor.GetText(TEXT.Okay)
					);
			}
			else
			{
				_psdDialogLoadKey = apPSDDialog.ShowWindow(_editor, OnPSDImageLoad);
				apPSDReimportDialog.CloseDialog();//<<Reimport는 닫는다.
				_psdReimportDialogLoadKey = null;
			}
		}
		public void OnPSDImageLoad(bool isSuccess, object loadKey,
									string fileName, string filePath,
									List<apPSDLayerData> layerDataList,
									//float atlasScaleRatio, float meshGroupScaleRatio,
									int atlasScaleRatioX100, int meshGroupScaleRatioX100,
									int totalWidth, int totalHeight, int padding,
									int bakedTextureWidth, int bakedTextureHeight,
									int bakeMaximumNumAtlas, bool bakeBlurOption,
									string bakeDstFilePath,
									string bakeDstFileRelativePath)
		{
			if (_psdDialogLoadKey != loadKey)
			{
				_psdDialogLoadKey = null;
				return;
			}

			_psdDialogLoadKey = null;

			if (Editor._portrait == null || !isSuccess)
			{ return; }

			//이제 만들어봅시다.

			float atlasScaleRatio = (float)atlasScaleRatioX100 * 0.01f;
			float meshGroupScaleRatio = (float)meshGroupScaleRatioX100 * 0.01f;

			//Vector2 centerPosOffset = new Vector2((float)totalWidth * 0.5f * scaleRatio, (float)totalHeight * 0.5f * scaleRatio);//이전 코드
			Vector2 centerPosOffset = new Vector2((float)totalWidth * 0.5f * meshGroupScaleRatio, (float)totalHeight * 0.5f * meshGroupScaleRatio);

			//1. Image 로드 + TextureData 생성

			//일단 먼저 -> Image를 로드해야함
			//로드하고 TextureData에도 추가를 한 뒤, LayerData와 연동 맵을 만든다.
			Dictionary<string, Texture2D> savedAtlasPath = new Dictionary<string, Texture2D>();
			Dictionary<apPSDLayerData, apTextureData> layerTextureMapping = new Dictionary<apPSDLayerData, apTextureData>();

			List<apTextureData> addedTextureDataList = new List<apTextureData>();

			//레이어 데이터에 따라서 이미지를 가져와서 리스트에 저장한다.
			//리스트에 저장하면서 새로운 apTextureData를 생성한다.
			for (int i = 0; i < layerDataList.Count; i++)
			{
				if (!layerDataList[i]._isImageLayer || !layerDataList[i]._isBakable)
				{
					continue;
				}

				string assetPath = layerDataList[i]._textureAssetPath;
				Texture2D savedImage = null;

				//Debug.Log("<" + layerDataList[i]._name + "> Image Path [" + assetPath + "]");
				if (savedAtlasPath.ContainsKey(assetPath))
				{
					savedImage = savedAtlasPath[assetPath];
				}
				else
				{
					savedImage = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
					if (savedImage == null)
					{
						Debug.LogError("Image Is Null Path [" + assetPath + "]");
					}
					savedAtlasPath.Add(assetPath, savedImage);
				}

				if (savedImage != null)
				{
					apTextureData textureData = Editor._portrait._textureData.Find(delegate (apTextureData a)
					{
						return a._image == savedImage;
					});

					//Debug.Log("Texture Asset : " + savedImage.name + " / Size : " + savedImage.width + "x" + savedImage.height);

					if (textureData == null)
					{
						textureData = AddImage();
						//이전 코드 : 4096 해상도에서 문제가 발생한다.
						//textureData.SetImage(savedImage, savedImage.width, savedImage.height);

						//변경된 코드 : 4096 해상도에서도 동작함
						textureData.SetImage(savedImage, bakedTextureWidth, bakedTextureHeight);

						//textureData._isPSDFile = true;
						textureData._assetFullPath = AssetDatabase.GetAssetPath(savedImage);

						//Debug.Log("Add Imaged : " + savedImage.name);

						addedTextureDataList.Add(textureData);
					}

					layerTextureMapping.Add(layerDataList[i], textureData);
				}
			}


			//2. Transform을 생성하자

			//1. Root가 될 MeshGroup을 만든다.
			apMeshGroup rootMeshGroup = AddMeshGroup();
			rootMeshGroup._name = fileName;


			//2. Parent가 없는 LayerData를 찾으면서 Mesh 또는 MeshGroup을 만들어주자
			//<추가> Depth는 LayerIndex와 같다.
			apPSDSet psdSet = AddNewPSDSet(false);


			//>> Import 정보를 PSD Set에 저장한다.
			psdSet.SetPSDBakeData(filePath, fileName,
									rootMeshGroup,
									addedTextureDataList,
									bakedTextureWidth, bakedTextureHeight,
									totalWidth, totalHeight,
									0, 0,
									bakeDstFilePath, bakeDstFileRelativePath,
									bakeMaximumNumAtlas, padding, bakeBlurOption,
									atlasScaleRatioX100, meshGroupScaleRatioX100
								);




			//3. Child가 있으면 재귀적으로 생성해준다.
			//RecursiveParsePSDLayers(layerDataList, 0, rootMeshGroup, layerTextureMapping, scaleRatio, centerPosOffset, padding);
			RecursiveParsePSDLayers(layerDataList, 0, rootMeshGroup, layerTextureMapping, atlasScaleRatio, meshGroupScaleRatio, centerPosOffset, padding, psdSet);


			rootMeshGroup.SortRenderUnits(true);
			//rootMeshGroup.SetAllRenderUnitForceUpdate();
			rootMeshGroup.RefreshForce();

			RefreshMeshGroups();


			//추가
			//Clipping 처리

			Editor.OnAnyObjectAddedOrRemoved();//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor._portrait.LinkAndRefreshInEditor(true, null);//추가 : 전체 리셋을 한다.
			Editor.Hierarchy.SetNeedReset();
			Editor.RefreshControllerAndHierarchy(false);

		}

		private void RecursiveParsePSDLayers(List<apPSDLayerData> layerDataList,
												int curLevel,
												apMeshGroup parentMeshGroup,
												Dictionary<apPSDLayerData, apTextureData> layerTextureMapping,
												float atlasScaleRatio, float meshGroupScaleRatio,
												Vector2 centerPosOffset, int padding,
												apPSDSet psdSet)
		{
			for (int i = 0; i < layerDataList.Count; i++)
			{
				apPSDLayerData curLayer = layerDataList[i];
				if (curLayer._hierarchyLevel != curLevel)
				{
					continue;
				}

				if (!curLayer._isBakable)
				{
					SetPSDLayerToPSDSet_NotBaked(curLayer, psdSet);
					continue;
				}

				if (curLayer._isImageLayer)
				{
					//이미지 레이어인 경우)
					//Mesh로 만들고 MeshTransform으로서 추가한다.
					//미리 Vertex를 Atlas 정보에 맞게 만들어주자
					apMesh newMesh = AddMesh();
					if (newMesh == null)
					{
						Debug.LogError("PSD Load Error : No Mesh Created");
						continue;
					}

					if (layerTextureMapping.ContainsKey(curLayer))
					{
						apTextureData textureData = layerTextureMapping[curLayer];

						//이전 코드
						//newMesh._textureData = textureData;

						//변경 코드 4.1
						newMesh.SetTextureData(textureData);

						float resizeRatioW = (float)textureData._width / (float)curLayer._bakedData._width;
						float resizeRatioH = (float)textureData._height / (float)curLayer._bakedData._height;

						//실제 텍스쳐 에셋의 크기와 저장할때의 원본 이미지 크기는 다를 수 있다.
						//텍스쳐 에셋 크기를 존중하는게 기본이다.
						Vector2 offsetPos = new Vector2(
							(float)curLayer._bakedImagePos_Left + ((float)curLayer._bakedWidth * 0.5f),
							(float)curLayer._bakedImagePos_Top + ((float)curLayer._bakedHeight * 0.5f));



						offsetPos.x *= resizeRatioW;
						offsetPos.y *= resizeRatioH;

						float atlasPos_Left = curLayer._bakedImagePos_Left * resizeRatioW;
						float atlasPos_Right = (curLayer._bakedImagePos_Left + curLayer._bakedWidth) * resizeRatioW;
						float atlasPos_Top = curLayer._bakedImagePos_Top * resizeRatioH;
						float atlasPos_Bottom = (curLayer._bakedImagePos_Top + curLayer._bakedHeight) * resizeRatioH;

						float halfSize_W = (float)textureData._width * 0.5f;
						float halfSize_H = (float)textureData._height * 0.5f;

						atlasPos_Left -= halfSize_W;
						atlasPos_Right -= halfSize_W;
						atlasPos_Top -= halfSize_H;
						atlasPos_Bottom -= halfSize_H;

						//Padding도 적용하자
						//atlasPos_Left -= padding;
						atlasPos_Right += padding * 2;
						//atlasPos_Top -= padding * 2;
						atlasPos_Bottom += padding * 2;

						offsetPos.x -= halfSize_W;
						offsetPos.y -= halfSize_H;

						offsetPos.x += padding;
						offsetPos.y += padding;

						//PSD용 이므로 Atlas정보를 넣어주자
						newMesh._isPSDParsed = true;
						newMesh._atlasFromPSD_LT = new Vector2(atlasPos_Left, atlasPos_Top);
						newMesh._atlasFromPSD_RB = new Vector2(atlasPos_Right, atlasPos_Bottom);

						newMesh.ResetVerticesByRect(offsetPos, atlasPos_Left, atlasPos_Top, atlasPos_Right, atlasPos_Bottom);
						Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영
					}

					newMesh._name = curLayer._name + "_Mesh";

					//Parent에 MeshTransform을 등록하자
					apTransform_Mesh meshTransform = AddMeshToMeshGroup(newMesh, parentMeshGroup);

					if (meshTransform == null)
					{
						//EditorUtility.DisplayDialog("Error", "Creating Mesh is failed", "Close");
						EditorUtility.DisplayDialog(Editor.GetText(TEXT.MeshCreationFailed_Title),
														Editor.GetText(TEXT.MeshCreationFailed_Body),
														Editor.GetText(TEXT.Close));
						return;
					}

					meshTransform._nickName = curLayer._name;

					//기준 위치를 잡아주자
					meshTransform._matrix = new apMatrix();
					if (curLevel == 0)
					{
						//meshTransform._matrix.SetPos(curLayer._posOffsetLocal * scaleRatio - centerPosOffset);
						meshTransform._matrix.SetPos(curLayer._posOffsetLocal * meshGroupScaleRatio - centerPosOffset);
					}
					else
					{
						//meshTransform._matrix.SetPos(curLayer._posOffsetLocal * scaleRatio);
						meshTransform._matrix.SetPos(curLayer._posOffsetLocal * meshGroupScaleRatio);
					}

					//추가 7.9 : Mesh > MeshTransform의 크기가 더이상 같지 않다.
					float meshScale = meshGroupScaleRatio / atlasScaleRatio;//MeshGroup의 확대 비율 / Atlas의 확대 비율
					meshTransform._matrix.SetScale(meshScale);

					meshTransform._matrix.MakeMatrix();

					if (curLayer._isClipping)
					{
						if (curLayer._isClippingValid && parentMeshGroup != null)
						{
							//Debug.Log("PSD Layer 클리핑 적용 [" + curLayer._name + "]");
							AddClippingMeshTransform(parentMeshGroup, meshTransform, false, false);
						}
					}

					//PSD Set에 저장
					SetPSDLayerToPSDSet(curLayer, psdSet, meshTransform._transformUniqueID);
				}
				else
				{
					//폴더 레이어인 경우)
					//MeshGroup으로 만들고 MeshGroupTransform으로서 추가한다.
					//재귀적으로 하위 호출을 한다.
					apMeshGroup newMeshGroup = AddMeshGroup();
					if (newMeshGroup == null)
					{
						Debug.LogError("PSD Load Error : No MeshGroup Created");
						continue;
					}

					newMeshGroup._name = curLayer._name + "_MeshGroup";

					apTransform_MeshGroup meshGroupTransform = AddMeshGroupToMeshGroup(newMeshGroup, parentMeshGroup, null);

					meshGroupTransform._nickName = curLayer._name;

					//기준 위치를 잡아주자
					meshGroupTransform._matrix = new apMatrix();
					if (curLevel == 0)
					{
						meshGroupTransform._matrix.SetPos(curLayer._posOffsetLocal * meshGroupScaleRatio - centerPosOffset);
					}
					else
					{
						meshGroupTransform._matrix.SetPos(curLayer._posOffsetLocal * meshGroupScaleRatio);
					}
					//MeshGroup의 스케일은 그대로
					meshGroupTransform._matrix.MakeMatrix();


					//PSD Set에 저장
					SetPSDLayerToPSDSet(curLayer, psdSet, meshGroupTransform._transformUniqueID);

					//자식 노드를 검색해서 처리하자
					if (curLayer._childLayers != null)
					{
						//RecursiveParsePSDLayers(curLayer._childLayers, curLayer._hierarchyLevel + 1, newMeshGroup, layerTextureMapping, scaleRatio, centerPosOffset, padding);
						RecursiveParsePSDLayers(curLayer._childLayers, curLayer._hierarchyLevel + 1, newMeshGroup, layerTextureMapping, atlasScaleRatio, meshGroupScaleRatio, centerPosOffset, padding, psdSet);
					}


					newMeshGroup.SortRenderUnits(true);
					//newMeshGroup.SetAllRenderUnitForceUpdate();
					newMeshGroup.RefreshForce();
				}
			}
		}




		// 추가 : PSD Reimport
		public void ShowPSDReimportDialog()
		{
			_psdDialogLoadKey = null;
			_psdReimportDialogLoadKey = apPSDReimportDialog.ShowWindow(_editor, OnPSDImageReimport);
		}


		//PSD 파일을 다시 임포트한다.
		//OnPSDImageLoad 함수와 거의 동일한 방식이지만, 기존 Transform에 적용하고, 텍스쳐도 덮어쓰기 때문에 많은 부분이 더 추가된다.
		//"새로운 데이터"인지 "기존 데이터"인지 구분하는것이 중요
		public void OnPSDImageReimport(bool isSuccess, object loadKey,
										string fileName, string filePath,
										List<apPSDLayerData> layerDataList,
										int atlasScaleRatioX100, int meshGroupScaleRatioX100, int prevAtlasScaleRatioX100,
										int totalWidth, int totalHeight, int padding,
										int bakedTextureWidth, int bakedTextureHeight,
										int bakeMaximumNumAtlas, bool bakeBlurOption,
										float centerOffsetDeltaX, float centerOffsetDeltaY,
										string bakeDstFilePath, string bakeDstFileRelativePath,
										apPSDSet psdSet
										//float deltaScaleRatio
										)
		{
			if (_psdReimportDialogLoadKey != loadKey)
			{
				_psdReimportDialogLoadKey = null;
				return;
			}

			_psdReimportDialogLoadKey = null;

			if (Editor._portrait == null || !isSuccess || psdSet == null)
			{
				return;
			}
			if (psdSet._linkedTargetMeshGroup == null)
			{
				//Reimport할 MeshGroup이 없어도 실패
				return;
			}

			//이제 만들어봅시다.

			float atlasScaleRatio = (float)atlasScaleRatioX100 * 0.01f;
			float meshGroupScaleRatio = (float)meshGroupScaleRatioX100 * 0.01f;
			float prevAtlasScaleRatio = (float)prevAtlasScaleRatioX100 * 0.01f;

			//Debug.Log("Reimport Result");
			//Debug.Log("  Prev Atlas Scale X100 : " + prevAtlasScaleRatioX100);
			//Debug.Log("  Next Atlas Scale X100 : " + atlasScaleRatioX100);
			//Debug.Log("  Mesh Group Scale X100 : " + meshGroupScaleRatioX100);

			Vector2 centerPosOffset = new Vector2((float)totalWidth * 0.5f * meshGroupScaleRatio, (float)totalHeight * 0.5f * meshGroupScaleRatio);
			centerPosOffset.x -= psdSet._nextBakeCenterOffsetDelta_X * meshGroupScaleRatio;//Reimport : CenterPosOffset이 수정된다.
			centerPosOffset.y -= psdSet._nextBakeCenterOffsetDelta_Y * meshGroupScaleRatio;

			//1. Image 로드 + TextureData 생성
			//사용된 TextureData는 삭제. 이미지 에셋은 삭제하지 않는다.

			//일단 먼저 -> Image를 로드해야함
			//로드하고 TextureData에도 추가를 한 뒤, LayerData와 연동 맵을 만든다.
			Dictionary<string, Texture2D> savedAtlasPath = new Dictionary<string, Texture2D>();
			Dictionary<apPSDLayerData, apTextureData> layerTextureMapping = new Dictionary<apPSDLayerData, apTextureData>();

			//Reimport : 이전의 apTextureData로서 덮어씌워진다. (실제로는 새로운게 생성되고 이건 삭제된다)
			List<apTextureData> removableTextureData = new List<apTextureData>();
			for (int i = 0; i < psdSet._targetTextureDataList.Count; i++)
			{
				removableTextureData.Add(psdSet._targetTextureDataList[i]._linkedTextureData);
			}

			List<apTextureData> addedTextureDataList = new List<apTextureData>();

			//레이어 데이터에 따라서 이미지를 가져와서 리스트에 저장한다.
			//리스트에 저장하면서 새로운 apTextureData를 생성한다.
			for (int i = 0; i < layerDataList.Count; i++)
			{
				if (!layerDataList[i]._isImageLayer || !layerDataList[i]._isBakable)
				{
					continue;
				}

				string assetPath = layerDataList[i]._textureAssetPath;
				Texture2D savedImage = null;

				//Debug.Log("<" + layerDataList[i]._name + "> Image Path [" + assetPath + "]");
				if (savedAtlasPath.ContainsKey(assetPath))
				{
					savedImage = savedAtlasPath[assetPath];
				}
				else
				{
					savedImage = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
					if (savedImage == null)
					{
						Debug.LogError("Image Is Null Path [" + assetPath + "]");
					}
					savedAtlasPath.Add(assetPath, savedImage);
				}


				if (savedImage != null)
				{
					apTextureData textureData = Editor._portrait._textureData.Find(delegate (apTextureData a)
					{
						return a._image == savedImage;
					});

					//Debug.Log("Texture Asset : " + savedImage.name + " / Size : " + savedImage.width + "x" + savedImage.height);

					if (textureData == null)
					{
						textureData = AddImage();
						//이전 코드 : 4096 해상도에서 문제가 발생한다.
						//textureData.SetImage(savedImage, savedImage.width, savedImage.height);

						//변경된 코드 : 4096 해상도에서도 동작함
						textureData.SetImage(savedImage, bakedTextureWidth, bakedTextureHeight);

						//textureData._isPSDFile = true;
						textureData._assetFullPath = AssetDatabase.GetAssetPath(savedImage);

						//Debug.Log("Add Imaged : " + savedImage.name);

						addedTextureDataList.Add(textureData);

					}
					else
					{
						//Reimport 과정에서 이미지가 그대로 사용되었다고 하더라도,
						//PSD 이미지 정보는 바뀌었을 것이다.
						textureData.SetImage(savedImage, bakedTextureWidth, bakedTextureHeight);
						textureData._assetFullPath = AssetDatabase.GetAssetPath(savedImage);
					}

					layerTextureMapping.Add(layerDataList[i], textureData);

					//만약 이 apTextureData가 그대로 사용되면 삭제되어선 안된다.
					if (removableTextureData.Contains(textureData))
					{
						removableTextureData.Remove(textureData);
					}
				}
			}

			//2. Transform을 변경/생성하자
			// - Reimport 대상인 Transform >> 위치값과 Vertex/Texture를 변경한다.
			// - Reimport 대상이 아닌 Transform >> 무시
			// - Reimport를 해야하지만 Transform이 없는 경우 >> 새로 생성
			//Target MeshGroup은 있어야 한다.

			//>> Import 정보를 PSD Set에 저장한다.
			psdSet.SetPSDBakeData(filePath, fileName,
									psdSet._linkedTargetMeshGroup,
									addedTextureDataList,
									bakedTextureWidth, bakedTextureHeight,
									totalWidth, totalHeight,
									centerOffsetDeltaX, centerOffsetDeltaY,
									bakeDstFilePath, bakeDstFileRelativePath,
									bakeMaximumNumAtlas, padding, bakeBlurOption,
									atlasScaleRatioX100, meshGroupScaleRatioX100
								);


			apMeshGroup rootMeshGroup = psdSet._linkedTargetMeshGroup;

			RecursiveParsePSDLayers_Reimport(layerDataList, 0,
												rootMeshGroup,
												layerTextureMapping,
												//scaleRatio, 
												atlasScaleRatio, meshGroupScaleRatio, prevAtlasScaleRatio,
												centerPosOffset, padding,
												psdSet);


			rootMeshGroup.SortRenderUnits(true);
			rootMeshGroup.RefreshForce();

			RefreshMeshGroups();







			//3. 마무리
			Editor.OnAnyObjectAddedOrRemoved();//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor._portrait.LinkAndRefreshInEditor(true, null);//추가 : 전체 리셋을 한다.
			Editor.Hierarchy.SetNeedReset();
			Editor.RefreshControllerAndHierarchy(false);

		}



		private void RecursiveParsePSDLayers_Reimport(List<apPSDLayerData> layerDataList,
														int curLevel,
														apMeshGroup parentMeshGroup,
														Dictionary<apPSDLayerData, apTextureData> layerTextureMapping,
														float atlasScaleRatio, float meshGroupScaleRatio, float prevAtlasScaleRatio,
														Vector2 centerPosOffset, int padding,
														apPSDSet psdSet)
		{
			for (int i = 0; i < layerDataList.Count; i++)
			{
				apPSDLayerData curLayer = layerDataList[i];

				if (curLayer._hierarchyLevel != curLevel)
				{
					continue;
				}

				if (!curLayer._isBakable)
				{
					SetPSDLayerToPSDSet_NotBaked(curLayer, psdSet);
					continue;
				}



				// - Reimport 대상인 Transform >> 위치값과 Vertex/Texture를 변경한다.
				// - Reimport 대상이 아닌 Transform >> 무시 (이건 위에서 isBakable)로 체크한다.
				// - Reimport를 해야하지만 Transform이 없는 경우 >> 새로 생성

				bool isRemapSelected = curLayer._isRemapSelected;
				apTransform_Mesh remapMeshTF = null;
				apTransform_MeshGroup remapMeshGroupTF = null;
				if (isRemapSelected)
				{
					if (curLayer._isImageLayer)
					{
						remapMeshTF = curLayer._remap_MeshTransform;
						if (remapMeshTF == null)
						{
							isRemapSelected = false;
						}
					}
					else
					{
						remapMeshGroupTF = curLayer._remap_MeshGroupTransform;
						if (remapMeshGroupTF == null)
						{
							isRemapSelected = false;
						}
					}
				}


				if (curLayer._isImageLayer)
				{
					//이미지 레이어인 경우)

					if (remapMeshTF != null)
					{
						//1) Remap할 MeshTransform이 있다. > MeshTransform의 위치와 Vertex/Texture를 수정해야한다.
						apMesh remapMesh = remapMeshTF._mesh;

						if (remapMesh != null && layerTextureMapping.ContainsKey(curLayer))
						{
							apTextureData textureData = layerTextureMapping[curLayer];

							remapMesh.SetTextureData(textureData);

							float resizeRatioW = (float)textureData._width / (float)curLayer._bakedData._width;
							float resizeRatioH = (float)textureData._height / (float)curLayer._bakedData._height;

							//실제 텍스쳐 에셋의 크기와 저장할때의 원본 이미지 크기는 다를 수 있다.
							//텍스쳐 에셋 크기를 존중하는게 기본이다.
							Vector2 offsetPos = new Vector2(
								(float)curLayer._bakedImagePos_Left + ((float)curLayer._bakedWidth * 0.5f),
								(float)curLayer._bakedImagePos_Top + ((float)curLayer._bakedHeight * 0.5f));

							offsetPos.x *= resizeRatioW;
							offsetPos.y *= resizeRatioH;

							float atlasPos_Left = curLayer._bakedImagePos_Left * resizeRatioW;
							float atlasPos_Right = (curLayer._bakedImagePos_Left + curLayer._bakedWidth) * resizeRatioW;
							float atlasPos_Top = curLayer._bakedImagePos_Top * resizeRatioH;
							float atlasPos_Bottom = (curLayer._bakedImagePos_Top + curLayer._bakedHeight) * resizeRatioH;

							float halfSize_W = (float)textureData._width * 0.5f;
							float halfSize_H = (float)textureData._height * 0.5f;

							atlasPos_Left -= halfSize_W;
							atlasPos_Right -= halfSize_W;
							atlasPos_Top -= halfSize_H;
							atlasPos_Bottom -= halfSize_H;

							//Padding도 적용하자
							//atlasPos_Left -= padding;
							atlasPos_Right += padding * 2;
							//atlasPos_Top -= padding * 2;
							atlasPos_Bottom += padding * 2;

							offsetPos.x -= halfSize_W;
							offsetPos.y -= halfSize_H;

							offsetPos.x += padding;
							offsetPos.y += padding;

							//변경된 Atlas 정보를 넣어주자
							remapMesh.MoveVertexToRemappedAtlas(offsetPos,
																	curLayer._remapPosOffsetDelta_X, curLayer._remapPosOffsetDelta_Y,
																	new Vector2(atlasPos_Left, atlasPos_Top),//Atlas LT
																	new Vector2(atlasPos_Right, atlasPos_Bottom),//Atlas RB
																												 //deltaScaleRatio
																	prevAtlasScaleRatio,
																	atlasScaleRatio
																	);
							Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영
						}

						if (remapMesh != null)
						{
							//이름은 바꿔준다.
							remapMesh._name = curLayer._name + "_Mesh";
						}
						remapMeshTF._nickName = curLayer._name;

						//기준 위치를 잡아주자
						remapMeshTF._matrix = new apMatrix();
						if (curLevel == 0)
						{
							remapMeshTF._matrix.SetPos(curLayer._posOffsetLocal * meshGroupScaleRatio - centerPosOffset);
						}
						else
						{
							remapMeshTF._matrix.SetPos(curLayer._posOffsetLocal * meshGroupScaleRatio);
						}

						//추가 7.9 : Mesh > MeshTransform의 크기가 더이상 같지 않다.
						float meshScale = meshGroupScaleRatio / atlasScaleRatio;//MeshGroup의 확대 비율 / Atlas의 확대 비율
						remapMeshTF._matrix.SetScale(meshScale);

						remapMeshTF._matrix.MakeMatrix();


						//PSD Set에 저장
						SetPSDLayerToPSDSet(curLayer, psdSet, remapMeshTF._transformUniqueID);

						//클리핑 처리는 하지 않는다.
						//if (curLayer._isClipping && curLayer._isClippingValid && parentMeshGroup != null)
						//{
						//	AddClippingMeshTransform(parentMeshGroup, meshTransform, false);
						//}
					}
					else
					{
						//2) Remap할 MeshTransform이 없다. > 새로 생성해야한다.
						//Mesh로 만들고 MeshTransform으로서 추가한다.
						//미리 Vertex를 Atlas 정보에 맞게 만들어주자
						apMesh newMesh = AddMesh();
						if (newMesh == null)
						{
							Debug.LogError("PSD Load Error : No Mesh Created");
							continue;
						}

						if (layerTextureMapping.ContainsKey(curLayer))
						{
							apTextureData textureData = layerTextureMapping[curLayer];

							//이전 코드
							//newMesh._textureData = textureData;

							//변경 코드 4.1
							newMesh.SetTextureData(textureData);

							float resizeRatioW = (float)textureData._width / (float)curLayer._bakedData._width;
							float resizeRatioH = (float)textureData._height / (float)curLayer._bakedData._height;

							//실제 텍스쳐 에셋의 크기와 저장할때의 원본 이미지 크기는 다를 수 있다.
							//텍스쳐 에셋 크기를 존중하는게 기본이다.
							Vector2 offsetPos = new Vector2(
								(float)curLayer._bakedImagePos_Left + ((float)curLayer._bakedWidth * 0.5f),
								(float)curLayer._bakedImagePos_Top + ((float)curLayer._bakedHeight * 0.5f));



							offsetPos.x *= resizeRatioW;
							offsetPos.y *= resizeRatioH;

							float atlasPos_Left = curLayer._bakedImagePos_Left * resizeRatioW;
							float atlasPos_Right = (curLayer._bakedImagePos_Left + curLayer._bakedWidth) * resizeRatioW;
							float atlasPos_Top = curLayer._bakedImagePos_Top * resizeRatioH;
							float atlasPos_Bottom = (curLayer._bakedImagePos_Top + curLayer._bakedHeight) * resizeRatioH;

							float halfSize_W = (float)textureData._width * 0.5f;
							float halfSize_H = (float)textureData._height * 0.5f;

							atlasPos_Left -= halfSize_W;
							atlasPos_Right -= halfSize_W;
							atlasPos_Top -= halfSize_H;
							atlasPos_Bottom -= halfSize_H;

							//Padding도 적용하자
							//atlasPos_Left -= padding;
							atlasPos_Right += padding * 2;
							//atlasPos_Top -= padding * 2;
							atlasPos_Bottom += padding * 2;

							offsetPos.x -= halfSize_W;
							offsetPos.y -= halfSize_H;

							offsetPos.x += padding;
							offsetPos.y += padding;

							//PSD용 이므로 Atlas정보를 넣어주자
							newMesh._isPSDParsed = true;
							newMesh._atlasFromPSD_LT = new Vector2(atlasPos_Left, atlasPos_Top);
							newMesh._atlasFromPSD_RB = new Vector2(atlasPos_Right, atlasPos_Bottom);

							newMesh.ResetVerticesByRect(offsetPos, atlasPos_Left, atlasPos_Top, atlasPos_Right, atlasPos_Bottom);
							Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영
						}

						newMesh._name = curLayer._name + "_Mesh";

						//Parent에 MeshTransform을 등록하자
						apTransform_Mesh meshTransform = AddMeshToMeshGroup(newMesh, parentMeshGroup);

						if (meshTransform == null)
						{
							//EditorUtility.DisplayDialog("Error", "Creating Mesh is failed", "Close");
							EditorUtility.DisplayDialog(Editor.GetText(TEXT.MeshCreationFailed_Title),
															Editor.GetText(TEXT.MeshCreationFailed_Body),
															Editor.GetText(TEXT.Close));
							return;
						}

						meshTransform._nickName = curLayer._name;

						//기준 위치를 잡아주자
						meshTransform._matrix = new apMatrix();
						if (curLevel == 0)
						{
							meshTransform._matrix.SetPos(curLayer._posOffsetLocal * meshGroupScaleRatio - centerPosOffset);
						}
						else
						{
							meshTransform._matrix.SetPos(curLayer._posOffsetLocal * meshGroupScaleRatio);
						}

						//추가 7.9 : Mesh > MeshTransform의 크기가 더이상 같지 않다.
						float meshScale = meshGroupScaleRatio / atlasScaleRatio;//MeshGroup의 확대 비율 / Atlas의 확대 비율
						meshTransform._matrix.SetScale(meshScale);

						meshTransform._matrix.MakeMatrix();

						if (curLayer._isClipping && curLayer._isClippingValid && parentMeshGroup != null)
						{
							AddClippingMeshTransform(parentMeshGroup, meshTransform, false, false);
						}

						//PSD Set에 저장
						SetPSDLayerToPSDSet(curLayer, psdSet, meshTransform._transformUniqueID);
					}
				}
				else
				{
					//폴더 레이어인 경우)
					if (remapMeshGroupTF != null)
					{
						//이름을 바꾸자
						remapMeshGroupTF._nickName = curLayer._name + "_MeshGroup";

						//기준 위치 수정
						remapMeshGroupTF._matrix = new apMatrix();
						if (curLevel == 0)
						{
							remapMeshGroupTF._matrix.SetPos(curLayer._posOffsetLocal * meshGroupScaleRatio - centerPosOffset);
						}
						else
						{
							remapMeshGroupTF._matrix.SetPos(curLayer._posOffsetLocal * meshGroupScaleRatio);
						}
						remapMeshGroupTF._matrix.MakeMatrix();

						//자식 노드를 검색해서 처리하자
						if (curLayer._childLayers != null)
						{
							RecursiveParsePSDLayers_Reimport(curLayer._childLayers,
																curLayer._hierarchyLevel + 1,
																remapMeshGroupTF._meshGroup,
																layerTextureMapping,
																atlasScaleRatio, meshGroupScaleRatio, prevAtlasScaleRatio,
																centerPosOffset,
																padding, psdSet
																);
						}

						remapMeshGroupTF._meshGroup.SortRenderUnits(true);
						remapMeshGroupTF._meshGroup.RefreshForce();

						//PSD Set에 저장
						SetPSDLayerToPSDSet(curLayer, psdSet, remapMeshGroupTF._transformUniqueID);
					}
					else
					{
						//MeshGroup으로 만들고 MeshGroupTransform으로서 추가한다.
						//재귀적으로 하위 호출을 한다.
						apMeshGroup newMeshGroup = AddMeshGroup();
						if (newMeshGroup == null)
						{
							Debug.LogError("PSD Load Error : No MeshGroup Created");
							continue;
						}

						newMeshGroup._name = curLayer._name + "_MeshGroup";

						apTransform_MeshGroup meshGroupTransform = AddMeshGroupToMeshGroup(newMeshGroup, parentMeshGroup, null);

						meshGroupTransform._nickName = curLayer._name;

						//기준 위치를 잡아주자
						meshGroupTransform._matrix = new apMatrix();
						if (curLevel == 0)
						{
							meshGroupTransform._matrix.SetPos(curLayer._posOffsetLocal * meshGroupScaleRatio - centerPosOffset);
						}
						else
						{
							meshGroupTransform._matrix.SetPos(curLayer._posOffsetLocal * meshGroupScaleRatio);
						}
						meshGroupTransform._matrix.MakeMatrix();

						//PSD Set에 저장
						SetPSDLayerToPSDSet(curLayer, psdSet, meshGroupTransform._transformUniqueID);

						//자식 노드를 검색해서 처리하자
						if (curLayer._childLayers != null)
						{
							RecursiveParsePSDLayers_Reimport(curLayer._childLayers, curLayer._hierarchyLevel + 1,
																newMeshGroup,
																layerTextureMapping,
																atlasScaleRatio, meshGroupScaleRatio, prevAtlasScaleRatio,
																centerPosOffset,
																padding, psdSet);
						}


						newMeshGroup.SortRenderUnits(true);
						//newMeshGroup.SetAllRenderUnitForceUpdate();
						newMeshGroup.RefreshForce();
					}

				}
			}
		}


		private void SetPSDLayerToPSDSet(apPSDLayerData psdLayer, apPSDSet psdSet, int transformID)
		{
			if (psdSet == null || psdLayer == null)
			{
				return;
			}
			psdSet.SetPSDLayerData(psdLayer._layerIndex,
									psdLayer._name,
									psdLayer._width,
									psdLayer._height,
									//psdLayer._posOffset_Left,
									//psdLayer._posOffset_Top,
									//psdLayer._posOffset_Right,
									//psdLayer._posOffset_Bottom,
									psdLayer._isImageLayer,
									//psdLayer._bakedWidth,
									//psdLayer._bakedHeight,
									//psdLayer._bakedImagePos_Left,
									//psdLayer._bakedImagePos_Top,
									transformID,
									(psdLayer._isRemapSelected ? psdLayer._remapPosOffsetDelta_X : 0.0f),
									(psdLayer._isRemapSelected ? psdLayer._remapPosOffsetDelta_Y : 0.0f)
								);
		}

		private void SetPSDLayerToPSDSet_NotBaked(apPSDLayerData psdLayer, apPSDSet psdSet)
		{
			if (psdSet == null || psdLayer == null)
			{
				return;
			}
			psdSet.SetNotBakedLayerData(
									psdLayer._layerIndex,
									psdLayer._name,
									psdLayer._isImageLayer
								);
		}

		public apPSDSet AddNewPSDSet(bool isUndoRecord)
		{
			if (Editor._portrait == null)
			{
				return null;
			}



			//새로운 ID를 만든다. (랜덤 + 검색) (랜덤 실패 후에는 특정 구역의 숫자 할당)
			int nextUniqueID = -1;
			int count = 0;
			bool isValidID = false;
			while (true)
			{
				nextUniqueID = UnityEngine.Random.Range(1000, 99999);
				bool isExistID = Editor._portrait._bakedPsdSets.Exists(delegate (apPSDSet a)
				{
					return a._uniqueID == nextUniqueID;
				});
				if (!isExistID)
				{
					//겹치는게 없다 => 유효한 ID
					isValidID = true;
					break;
				}
				count++;
				if (count > 50)
				{
					//50번이나 시도해서 실패했다.
					break;
				}
			}
			if (!isValidID)
			{
				//50번의 시도 안에 유효한 ID가 없으면 100~1000번대를 순서대로 체크한다.
				for (int i = 100; i < 1000; i++)
				{
					nextUniqueID = i;
					bool isExistID = Editor._portrait._bakedPsdSets.Exists(delegate (apPSDSet a)
					{
						return a._uniqueID == nextUniqueID;
					});
					if (!isExistID)
					{
						//겹치는게 없다 => 유효한 ID
						isValidID = true;
						break;
					}
				}
			}
			if (!isValidID)
			{
				Debug.LogError("AnyPortrait : A new PSD Import Set could not be created. please try again.");
				return null;
			}

			if (isUndoRecord)
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.PSDSet_AddNewPSDSet, Editor, Editor._portrait, Editor._portrait, false);
			}


			apPSDSet newPsdSet = new apPSDSet(nextUniqueID);
			Editor._portrait._bakedPsdSets.Add(newPsdSet);

			return newPsdSet;
		}


		//--------------------------------------------------------------------------------------------------------------------------------

		#region [미사용 코드]
		///// <summary>
		///// 메시를 삭제한다.
		///// </summary>
		///// <param name="iRemove"></param>
		//public void RemoveMesh(int iRemove)
		//{
		//	if (iRemove >= 0 && iRemove < Editor._portrait._meshes.Count)
		//	{
		//		apEditorUtil.SetRecord("Remove Mesh", Editor._portrait);

		//		apMesh removedMesh = Editor._portrait._meshes[iRemove];

		//		if (removedMesh == Editor.Select.Mesh)
		//		{
		//			Editor.Select.SetNone();
		//		}

		//		Editor._portrait._meshes.RemoveAt(iRemove);

		//		if (removedMesh != null)
		//		{	
		//			MonoBehaviour.DestroyImmediate(removedMesh.gameObject);
		//		}
		//	}
		//} 
		#endregion





		public apMesh AddMesh()
		{
			//ObjectGroup을 체크하여 만들어주자
			CheckAndMakeObjectGroup();

			//int nextID = Editor._portrait.MakeUniqueID_Mesh();
			int nextID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.Mesh);
			if (nextID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Mesh Add Failed. Please Retry", "Close");
				EditorUtility.DisplayDialog(Editor.GetText(TEXT.MeshAddFailed_Title),
												Editor.GetText(TEXT.MeshAddFailed_Body),
												Editor.GetText(TEXT.Close));
				return null;
			}

			//Undo - Add Mesh
			//apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Main_AddMesh, Editor, Editor._portrait, null, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Create Mesh");

			int nMeshes = Editor._portrait._meshes.Count;

			//GameObject로 만드는 경우
			string newName = "New Mesh (" + nMeshes + ")";
			GameObject newGameObj = new GameObject(newName);
			newGameObj.transform.parent = Editor._portrait._subObjectGroup_Mesh.transform;
			newGameObj.transform.localPosition = Vector3.zero;
			newGameObj.transform.localRotation = Quaternion.identity;
			newGameObj.transform.localScale = Vector3.one;
			newGameObj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

			apMesh newMesh = newGameObj.AddComponent<apMesh>();
			//apMesh newMesh = new apMesh();


			newMesh._uniqueID = nextID;
			newMesh._name = newName;

			newMesh.ReadyToEdit(Editor._portrait);

			Editor._portrait._meshes.Add(newMesh);
			Editor.Select.SetMesh(newMesh);

			//Editor.Hierarchy.RefreshUnits();
			Editor.RefreshControllerAndHierarchy(false);

			//Mesh Hierarchy Filter를 활성화한다.
			Editor.SetHierarchyFilter(apEditor.HIERARCHY_FILTER.Mesh, true);


			//Undo - Create 추가
			apEditorUtil.SetRecordCreateMonoObject(newMesh, "Create Mesh");

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			//프리팹이었다면 Apply
			apEditorUtil.SetPortraitPrefabApply(Editor._portrait);

			return newMesh;
		}


		public void RemoveMesh(apMesh mesh)
		{
			if (mesh == Editor.Select.Mesh)
			{
				Editor.Select.SetNone();
			}

			//Undo
			//apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Main_RemoveMesh, Editor, Editor._portrait, mesh, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Mesh");

			Editor._portrait.PushUnusedID(apIDManager.TARGET.Mesh, mesh._uniqueID);

			Editor._portrait._meshes.Remove(mesh);

			//if (mesh != null)
			//{	
			//	MonoBehaviour.DestroyImmediate(mesh.gameObject);
			//}

			//추가
			if (mesh != null)
			{
				//Undo.DestroyObjectImmediate(mesh.gameObject);
				apEditorUtil.SetRecordDestroyMonoObject(mesh, "Remove Mesh");
			}

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor._portrait.LinkAndRefreshInEditor(true, null);


			//Editor.Hierarchy.RefreshUnits();
			Editor.Hierarchy.SetNeedReset();
			Editor.RefreshControllerAndHierarchy(false);

			//프리팹이었다면 Apply
			apEditorUtil.SetPortraitPrefabApply(Editor._portrait);
		}

		/// <summary>
		/// 메시의 미러 복사를 한다.
		/// </summary>
		public void DuplicateMirrorVertices()
		{
			if (Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
				|| Editor.Select.Mesh == null
				|| Editor._meshEditMirrorMode != apEditor.MESH_EDIT_MIRROR_MODE.Mirror
				|| Editor.Select.Mesh.LinkedTextureData == null
				|| Editor.Select.Mesh.LinkedTextureData._image == null)
			{
				return;
			}
			int nVert = Editor.VertController.Vertices != null ? Editor.VertController.Vertices.Count : 0;
			if (nVert == 0)
			{
				return;
			}

			//Mirror Set을 리셋한다.
			Editor.MirrorSet.Refresh(Editor.Select.Mesh, true);

			if (Editor.MirrorSet._cloneVerts.Count == 0)
			{
				return;
			}
			apMesh mesh = Editor.Select.Mesh;
			apTextureData textureData = Editor.Select.Mesh.LinkedTextureData;

			apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_VertexCopied, Editor, mesh, mesh, false);

			//Vector2 meshOffset = mesh._offsetPos;
			//Vector2 imageHalfOffset = new Vector2(textureData._width * 0.5f, textureData._height * 0.5f);

			List<apMirrorVertexSet.CloneVertex> cloneVerts = Editor.MirrorSet._cloneVerts;
			List<apMirrorVertexSet.CloneVertex> crossVerts = Editor.MirrorSet._crossVerts;
			List<apMirrorVertexSet.CloneEdge> cloneEdges = Editor.MirrorSet._cloneEdges;


			Dictionary<apMirrorVertexSet.CloneVertex, apVertex> clone2Vert = new Dictionary<apMirrorVertexSet.CloneVertex, apVertex>();
			Dictionary<apVertex, apMirrorVertexSet.CloneVertex> vert2Clone = new Dictionary<apVertex, apMirrorVertexSet.CloneVertex>();

			apMirrorVertexSet.CloneVertex curCloneVert = null;
			apVertex mirrorVert = null;



			//1. Clone Vert를 모두 생성한다. (CrossVert 포함)
			// (isOnAxis인 경우, 새로 생성하지는 않고 위치만 변경한다. Dictionary엔 추가)
			for (int iClone = 0; iClone < cloneVerts.Count; iClone++)
			{
				curCloneVert = cloneVerts[iClone];
				if (curCloneVert._isOnAxis)
				{
					//위치를 보정한다.
					if (mesh._isMirrorX)
					{
						//X 보정
						curCloneVert._srcVert._pos.x = mesh._mirrorAxis.x;
					}
					else
					{
						//Y 보정
						curCloneVert._srcVert._pos.y = mesh._mirrorAxis.y;
					}
					mirrorVert = curCloneVert._srcVert;
				}
				else
				{
					//새로 생성한다.
					mirrorVert = mesh.AddVertexAutoUV(curCloneVert._pos);
				}

				clone2Vert.Add(curCloneVert, mirrorVert);
				vert2Clone.Add(mirrorVert, curCloneVert);
			}


			for (int iCross = 0; iCross < crossVerts.Count; iCross++)
			{
				curCloneVert = crossVerts[iCross];

				//Cross는 비교 없이 바로 만들면 된다.
				mirrorVert = mesh.AddVertexAutoUV(curCloneVert._pos);

				clone2Vert.Add(curCloneVert, mirrorVert);
				vert2Clone.Add(mirrorVert, curCloneVert);
			}


			//2. Cross Vert로 인해서 Edge가 쪼개져야 할 경우가 있다.
			apMirrorVertexSet.CloneEdge curCloneEdge = null;
			apMeshEdge splitEdge = null;
			apVertex curVert = null;


			for (int iCross = 0; iCross < crossVerts.Count; iCross++)
			{
				curCloneVert = crossVerts[iCross];
				curVert = clone2Vert[curCloneVert];

				splitEdge = curCloneVert._srcSplitEdge;
				if (splitEdge != null)
				{
					//기존의 Edge를 2개로 분리한다.
					//Edge 추가
					mesh.MakeNewEdge(curVert, splitEdge._vert1, false);
					mesh.MakeNewEdge(curVert, splitEdge._vert2, false);

					//기존의 Edge 삭제
					mesh.RemoveEdge(splitEdge);
				}

			}


			//3. 그 외의 CloneEdge를 실제 Edge로 만들자
			apVertex edgeVert1 = null;
			apVertex edgeVert2 = null;
			for (int iEdge = 0; iEdge < cloneEdges.Count; iEdge++)
			{
				curCloneEdge = cloneEdges[iEdge];

				edgeVert1 = clone2Vert[curCloneEdge._cloneVert1];
				edgeVert2 = clone2Vert[curCloneEdge._cloneVert2];
				mesh.MakeNewEdge(edgeVert1, edgeVert2, false);
			}

			//Mesh를 만들자
			mesh.MakeEdgesToPolygonAndIndexBuffer();
			Editor.VertController.UnselectVertex();//<<버텍스 선택은 모두 해제
			Editor.SetRepaint();

			Editor.MirrorSet.Refresh(mesh, true);
		}


		//버텍스를 정렬한다.
		public enum VERTEX_ALIGN_REQUEST
		{
			MinX,
			CenterX,
			MaxX,
			MinY,
			CenterY,
			MaxY,
			DistributeX,
			DistributeY

		}
		public void AlignVertices(VERTEX_ALIGN_REQUEST alignType)
		{
			if (Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh
				|| Editor.Select.Mesh == null
				|| Editor.Select.Mesh.LinkedTextureData == null
				|| Editor.Select.Mesh.LinkedTextureData._image == null
				|| Editor._meshEditMode != apEditor.MESH_EDIT_MODE.MakeMesh
				|| Editor._meshEditeMode_MakeMesh != apEditor.MESH_EDIT_MODE_MAKEMESH.TRS)
			{
				return;
			}

			if (Editor.Gizmos.IsFFDMode)
			{
				//FFD 모드에서는 처리가 안된다.
				return;
			}

			apMesh mesh = Editor.Select.Mesh;
			List<apVertex> vertices = mesh._vertexData;
			List<apVertex> selectedVertices = Editor.VertController.Vertices;
			if (vertices.Count <= 0 || selectedVertices.Count <= 1)
			{
				return;
			}

			apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_VertexMoved, Editor, mesh, mesh, true);

			apVertex vert = null;
			float minX = 0.0f;
			float maxX = 0.0f;
			float minY = 0.0f;
			float maxY = 0.0f;

			//먼저 범위를 정한다.
			//Pos는 MeshOffset을 적용하지 않는다.
			vert = selectedVertices[0];
			minX = vert._pos.x;
			maxX = vert._pos.x;
			minY = vert._pos.y;
			maxY = vert._pos.y;
			for (int i = 0; i < selectedVertices.Count; i++)
			{
				vert = selectedVertices[i];

				minX = Mathf.Min(minX, vert._pos.x);
				maxX = Mathf.Max(maxX, vert._pos.x);
				minY = Mathf.Min(minY, vert._pos.y);
				maxY = Mathf.Max(maxY, vert._pos.y);
			}
			float centerX = minX * 0.5f + maxX * 0.5f;
			float centerY = minY * 0.5f + maxY * 0.5f;


			switch (alignType)
			{
				case VERTEX_ALIGN_REQUEST.MinX:
					for (int i = 0; i < selectedVertices.Count; i++)
					{
						vert = selectedVertices[i];
						vert._pos.x = minX;
					}
					break;

				case VERTEX_ALIGN_REQUEST.CenterX:
					for (int i = 0; i < selectedVertices.Count; i++)
					{
						vert = selectedVertices[i];
						vert._pos.x = centerX;
					}
					break;

				case VERTEX_ALIGN_REQUEST.MaxX:
					for (int i = 0; i < selectedVertices.Count; i++)
					{
						vert = selectedVertices[i];
						vert._pos.x = maxX;
					}
					break;

				case VERTEX_ALIGN_REQUEST.MinY:
					for (int i = 0; i < selectedVertices.Count; i++)
					{
						vert = selectedVertices[i];
						vert._pos.y = minY;
					}
					break;

				case VERTEX_ALIGN_REQUEST.CenterY:
					for (int i = 0; i < selectedVertices.Count; i++)
					{
						vert = selectedVertices[i];
						vert._pos.y = centerY;
					}
					break;

				case VERTEX_ALIGN_REQUEST.MaxY:
					for (int i = 0; i < selectedVertices.Count; i++)
					{
						vert = selectedVertices[i];
						vert._pos.y = maxY;
					}
					break;

				case VERTEX_ALIGN_REQUEST.DistributeX:
				case VERTEX_ALIGN_REQUEST.DistributeY:
					//일단 새로운 리스트를 만들어서 X, 또는 Y 순으로 Sort한다.
					{
						List<apVertex> sortedVerts = new List<apVertex>();
						for (int i = 0; i < selectedVertices.Count; i++)
						{
							sortedVerts.Add(selectedVertices[i]);
						}

						if (alignType == VERTEX_ALIGN_REQUEST.DistributeX)
						{
							//X 좌표로 정렬 (올림차순)
							sortedVerts.Sort(delegate (apVertex a, apVertex b)
							{
								return (int)((a._pos.x - b._pos.x) * 1000.0f);
							});
						}
						else
						{
							//Y 좌표로 정렬 (올림차순)
							sortedVerts.Sort(delegate (apVertex a, apVertex b)
							{
								return (int)((a._pos.y - b._pos.y) * 1000.0f);
							});
						}

						for (int i = 0; i < sortedVerts.Count; i++)
						{
							vert = sortedVerts[i];
							if (i == 0)
							{
								if (alignType == VERTEX_ALIGN_REQUEST.DistributeX)
								{
									vert._pos.x = minX;
								}
								else
								{
									vert._pos.y = minY;
								}
							}
							else if (i == sortedVerts.Count - 1)
							{
								if (alignType == VERTEX_ALIGN_REQUEST.DistributeX)
								{
									vert._pos.x = maxX;
								}
								else
								{
									vert._pos.y = maxY;
								}
							}
							else
							{
								float lerp = (float)i / (float)(sortedVerts.Count - 1);
								if (alignType == VERTEX_ALIGN_REQUEST.DistributeX)
								{
									vert._pos.x = minX * (1.0f - lerp) + maxX * lerp;
								}
								else
								{
									vert._pos.y = minY * (1.0f - lerp) + maxY * lerp;
								}
							}
						}

					}
					break;

			}

			for (int i = 0; i < selectedVertices.Count; i++)
			{
				mesh.RefreshVertexAutoUV(selectedVertices[i]);
			}
		}




		//---------------------------------------------------------------------------------
		// Control Param 추가 / 제거 / 편집
		//---------------------------------------------------------------------------------
		public void AddParam()
		{
			//Undo - Add Param
			apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Main_AddParam, Editor, Editor._portrait, null, false);


			//int iNextIndex = Editor._portrait.MakeUniqueID_ControlParam();
			int iNextIndex = 0;
			while (true)
			{
				apControlParam existParam = Editor.ParamControl.FindParam("New Param (" + iNextIndex + ")");
				if (existParam != null)
				{
					iNextIndex++;
				}
				else
				{
					break;
				}
			}
			string strNewName = "New Param (" + iNextIndex + ")";

			Editor.ParamControl.Ready(Editor._portrait);
			apControlParam newParam = Editor.ParamControl.AddParam_Float(strNewName, false, apControlParam.CATEGORY.Etc, 0.0f);
			if (newParam == null)
			{
				EditorUtility.DisplayDialog(
									Editor.GetText(TEXT.DemoLimitation_Title),
									Editor.GetText(TEXT.DemoLimitation_Body_AddParam),
									Editor.GetText(TEXT.Okay)
									);
				return;
			}
			newParam.Ready(Editor._portrait);
			if (newParam != null)
			{
				Editor.Select.SetParam(newParam);
			}

			//Editor.Hierarchy.RefreshUnits();
			Editor.RefreshControllerAndHierarchy(false);

			//Param Hierarchy Filter를 활성화한다.
			Editor.SetHierarchyFilter(apEditor.HIERARCHY_FILTER.Param, true);

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();
		}

		public void ChangeParamName(apControlParam cParam, string strNextName)
		{
			if (Editor._portrait == null)
			{
				return;
			}


			//string prevName = cParam._keyName;

			apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.ControlParam_SettingChanged, Editor, Editor._portrait, null, false);

			#region [미사용 코드] string 값으로 Param을 저장했을때의 코드(지금은 int형 ID)
			//for (int iMeshGroup = 0; iMeshGroup < Editor._portrait._meshGroups.Count; iMeshGroup++)
			//{
			//	apMeshGroup meshGroup = Editor._portrait._meshGroups[iMeshGroup];
			//	for (int iMod = 0; iMod < meshGroup._modifierStack._modifiers.Count; iMod++)
			//	{
			//		apModifierBase mod = meshGroup._modifierStack._modifiers[iMod];

			//		for (int iParamSetGroup = 0; iParamSetGroup < mod._paramSetGroup_controller.Count; iParamSetGroup++)
			//		{
			//			apModifierParamSetGroup paramSetGroup = mod._paramSetGroup_controller[iParamSetGroup];

			//			//같다면 이름을 바꾸자
			//			if(paramSetGroup._keyControlParam != null && 
			//				paramSetGroup._keyControlParam == cParam)
			//			{
			//				paramSetGroup._keyControlParamName = strNextName;
			//			}
			//		}
			//	}
			//} 
			#endregion

			cParam._keyName = strNextName;
			Editor.RefreshControllerAndHierarchy(false);
		}

		public void RemoveParam(apControlParam cParam)
		{
			if (Editor.Select.Param == cParam)
			{
				Editor.Select.SetNone();
			}

			//apEditorUtil.SetRecord_PortraitAllMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Main_RemoveParam, Editor, Editor._portrait, cParam, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Control Parameter");

			int removedParamID = cParam._uniqueID;

			//Editor._portrait.PushUniqueID_ControlParam(removedParamID);
			Editor._portrait.PushUnusedID(apIDManager.TARGET.ControlParam, removedParamID);

			Editor.ParamControl._controlParams.Remove(cParam);

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor._portrait.LinkAndRefreshInEditor(true, null);

			//Editor.Hierarchy.RefreshUnits();
			Editor.ResetHierarchyAll();
			Editor.RefreshControllerAndHierarchy(false);


		}


		public void SetControlParamPreset(apControlParam cParam, apControlParamPresetUnit preset)
		{
			if (cParam == null || preset == null)
			{
				return;
			}
			apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.ControlParam_SettingChanged, Editor, Editor._portrait, cParam, false);

			//이름이 겹치는 건 피해야한다.
			string nextName = preset._keyName;
			bool isNameOverwrite = false;
			//동일한 이름이 있다면 메시지로 알려주자
			if (Editor.ParamControl.FindParam(nextName) != null)
			{
				//이름이 겹친다.
				isNameOverwrite = true;
			}

			if (!isNameOverwrite)
			{
				//이름 적용
				cParam._keyName = preset._keyName;
			}
			cParam._category = preset._category;
			cParam._iconPreset = preset._iconPreset;
			cParam._valueType = preset._valueType;

			cParam._int_Def = preset._int_Def;
			cParam._float_Def = preset._float_Def;
			cParam._vec2_Def = preset._vec2_Def;

			cParam._int_Min = preset._int_Min;
			cParam._int_Max = preset._int_Max;
			cParam._float_Min = preset._float_Min;
			cParam._float_Max = preset._float_Max;
			cParam._vec2_Min = preset._vec2_Min;
			cParam._vec2_Max = preset._vec2_Max;

			cParam._label_Min = preset._label_Min;
			cParam._label_Max = preset._label_Max;
			cParam._snapSize = preset._snapSize;

			Editor.RefreshControllerAndHierarchy(false);

			if (isNameOverwrite)
			{
				//이름이 중복되었음을 알려준다.
				EditorUtility.DisplayDialog(Editor.GetText(TEXT.ControlParamPreset_NameOverwrite_Title),
											Editor.GetText(TEXT.ControlParamPreset_NameOverwrite_Body),
											Editor.GetText(TEXT.Okay));
			}
		}

		//-------------------------------------------------------------------------------

		public apAnimClip AddAnimClip(bool isSetRecord = true)
		{
			if (isSetRecord)
			{
				//Undo - Add Animation
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Main_AddAnimation, Editor, Editor._portrait, null, false);
			}

			//int iNextIndex = Editor._portrait.MakeUniqueID_AnimClip();
			int iNextIndex = 0;
			//int iNextUniqueID = Editor._portrait.MakeUniqueID_AnimClip();
			int iNextUniqueID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimClip);

			if (iNextUniqueID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Anim Clip Creating Failed", "Close");
				EditorUtility.DisplayDialog(Editor.GetText(TEXT.AnimCreateFailed_Title),
												Editor.GetText(TEXT.AnimCreateFailed_Body),
												Editor.GetText(TEXT.Close));
				return null;
			}

			while (true)
			{
				bool isExist = Editor._portrait._animClips.Exists(delegate (apAnimClip a)
				{
					return a._name.Equals("New AnimClip (" + iNextIndex + ")");
				});

				if (isExist)
				{
					iNextIndex++;
				}
				else
				{
					break;
				}
			}

			string strNewName = "New AnimClip (" + iNextIndex + ")";

			apAnimClip newAnimClip = new apAnimClip();
			newAnimClip.Init(Editor._portrait, strNewName, iNextUniqueID);
			newAnimClip.LinkEditor(Editor._portrait);

			Editor._portrait._animClips.Add(newAnimClip);

			//이전
			//Editor.RefreshTimelineLayers(true);//<<추가 : 타임라인 정보를 리셋
			//Editor.RefreshControllerAndHierarchy();

			//변경 19.5.21
			Editor.RefreshControllerAndHierarchy(true);

			//Animation Hierarchy Filter를 활성화한다.
			Editor.SetHierarchyFilter(apEditor.HIERARCHY_FILTER.Animation, true);

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			return newAnimClip;
		}


		public void RemoveAnimClip(apAnimClip animClip)
		{
			//Remove - Animation
			//apEditorUtil.SetRecord_PortraitAllMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Main_RemoveAnimation, Editor, Editor._portrait, animClip, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Animation Clip");

			if (Editor.Select.AnimClip == animClip)
			{
				Editor.Select.SetNone();
			}

			int removedAnimClipID = animClip._uniqueID;

			//Editor._portrait.PushUniqueID_AnimClip(removedAnimClipID);
			Editor._portrait.PushUnusedID(apIDManager.TARGET.AnimClip, removedAnimClipID);

			Editor._portrait._animClips.Remove(animClip);


			//MeshGroup의 각 Modifier의 "Animated" 계열의 링크를 모두 끊어야 한다.
			//타임라인 정보를 리셋
			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null);

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor._portrait.LinkAndRefreshInEditor(true, null);
			Editor.Hierarchy.SetNeedReset();
			Editor.RefreshControllerAndHierarchy(false);

			//프리팹이었다면 Apply
			apEditorUtil.SetPortraitPrefabApply(Editor._portrait);
		}


		/// <summary>
		/// AnimClip을 복제하자
		/// </summary>
		/// <param name="srcAnimClip"></param>
		public void DuplicateAnimClip(apAnimClip srcAnimClip)
		{
			if (apVersion.I.IsDemo)
			{
				if (Editor._portrait._animClips.Count >= 2)
				{
					//이미 2개를 넘었다.
					//복사할 수 없다.
					EditorUtility.DisplayDialog(
						Editor.GetText(TEXT.DemoLimitation_Title),
						Editor.GetText(TEXT.DemoLimitation_Body_AddAnimation),
						Editor.GetText(TEXT.Okay)
						);

					return;
				}
			}


			//Undo - Anim Clip 복사
			apEditorUtil.SetRecord_PortraitAllMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Anim_DupAnimClip, Editor, Editor._portrait, null, false);

			//일단 복사
			apAnimClip dupAnimClip = AddAnimClip(true);

			if (dupAnimClip == null)
			{
				return;
			}

			//1. AnimClip의 기본 정보를 복사한다.
			string dupAnimClipName = srcAnimClip._name + " Copy";

			//중복되지 않은 이름을 찾는다.
			if (Editor._portrait._animClips.Exists(delegate (apAnimClip a)
			{ return string.Equals(dupAnimClipName, a._name); }))
			{
				//오잉 똑같은 이름이 있네염...
				int copyIndex = -1;
				for (int iCopyIndex = 1; iCopyIndex < 1000; iCopyIndex++)
				{
					if (!Editor._portrait._animClips.Exists(delegate (apAnimClip a)
					{ return string.Equals(dupAnimClipName + " (" + iCopyIndex + ")", a._name); }))
					{
						copyIndex = iCopyIndex;
						break;
					}
				}
				if (copyIndex < 0)
				{ dupAnimClipName += " (1000+)"; }
				else
				{ dupAnimClipName += " (" + copyIndex + ")"; }
			}


			dupAnimClip._name = dupAnimClipName;
			dupAnimClip._portrait = srcAnimClip._portrait;
			dupAnimClip._targetMeshGroupID = srcAnimClip._targetMeshGroupID;
			dupAnimClip._targetMeshGroup = srcAnimClip._targetMeshGroup;
			dupAnimClip._targetOptTranform = srcAnimClip._targetOptTranform;

			dupAnimClip.SetOption_FPS(srcAnimClip.FPS);
			dupAnimClip.SetOption_StartFrame(srcAnimClip.StartFrame);
			dupAnimClip.SetOption_EndFrame(srcAnimClip.EndFrame);
			dupAnimClip.SetOption_IsLoop(srcAnimClip.IsLoop);

			//어떤 Src로 복사할지를 연결해둔다.
			Dictionary<apAnimTimeline, apAnimTimeline> dupTimelinePairs = new Dictionary<apAnimTimeline, apAnimTimeline>();
			Dictionary<apAnimTimelineLayer, apAnimTimelineLayer> dupTimelineLayerPairs = new Dictionary<apAnimTimelineLayer, apAnimTimelineLayer>();
			Dictionary<apAnimKeyframe, apAnimKeyframe> dupKeyframePairs = new Dictionary<apAnimKeyframe, apAnimKeyframe>();


			//2. Timeline을 하나씩 복사한다.
			for (int iT = 0; iT < srcAnimClip._timelines.Count; iT++)
			{
				apAnimTimeline srcTimeline = srcAnimClip._timelines[iT];
				apAnimTimeline dupTimeline = AddAnimTimeline(srcTimeline._linkType, srcTimeline._modifierUniqueID, dupAnimClip, false, false);
				if (dupTimeline == null)
				{
					//EditorUtility.DisplayDialog("Error", "Timeline Adding Failed.", "Close");
					EditorUtility.DisplayDialog(Editor.GetText(TEXT.AnimDuplicatedFailed_Title),
													Editor.GetText(TEXT.AnimDuplicatedFailed_Body),
													Editor.GetText(TEXT.Close));
					return;
				}

				dupTimeline._guiColor = srcTimeline._guiColor;
				dupTimeline._linkedModifier = srcTimeline._linkedModifier;
				dupTimeline._linkedOptModifier = srcTimeline._linkedOptModifier;

				//Dup - Src 순으로 복사된 Timeline을 저장하자.
				dupTimelinePairs.Add(dupTimeline, srcTimeline);

			}

			//Sync를 한번 해두자
			AddAndSyncAnimClipToModifier(dupAnimClip);

			foreach (KeyValuePair<apAnimTimeline, apAnimTimeline> timelinePair in dupTimelinePairs)
			{
				apAnimTimeline dupTimeline = timelinePair.Key;
				apAnimTimeline srcTimeline = timelinePair.Value;

				//3. TimelineLayer를 하나씩 복사한다.
				for (int iTL = 0; iTL < srcTimeline._layers.Count; iTL++)
				{
					apAnimTimelineLayer srcLayer = srcTimeline._layers[iTL];

					int nextLayerID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimTimelineLayer);
					if (nextLayerID < 0)
					{
						//EditorUtility.DisplayDialog("Error", "Timeline Layer Add Failed", "Close");
						EditorUtility.DisplayDialog(Editor.GetText(TEXT.AnimDuplicatedFailed_Title),
														Editor.GetText(TEXT.AnimDuplicatedFailed_Body),
														Editor.GetText(TEXT.Close));

						return;
					}

					apAnimTimelineLayer dupLayer = new apAnimTimelineLayer();
					dupLayer.Link(dupTimeline._parentAnimClip, dupTimeline);

					//if (dupTimeline._linkType == apAnimClip.LINK_TYPE.Bone)
					//{
					//	Debug.LogError("TODO : Bone 타입의 Timeline 복제가 구현되지 않았다.");
					//}
					dupLayer._uniqueID = nextLayerID;
					dupLayer._parentAnimClip = srcLayer._parentAnimClip;
					dupLayer._parentTimeline = srcLayer._parentTimeline;
					//dupLayer._isMeshTransform = srcLayer._isMeshTransform;

					dupLayer._linkModType = srcLayer._linkModType;

					dupLayer._transformID = srcLayer._transformID;
					dupLayer._linkedMeshTransform = srcLayer._linkedMeshTransform;
					dupLayer._linkedMeshGroupTransform = srcLayer._linkedMeshGroupTransform;
					dupLayer._linkedOptTransform = srcLayer._linkedOptTransform;
					dupLayer._guiColor = srcLayer._guiColor;
					dupLayer._targetParamSetGroup = srcLayer._targetParamSetGroup;
					dupLayer._boneID = srcLayer._boneID;
					dupLayer._controlParamID = srcLayer._controlParamID;
					dupLayer._linkedControlParam = srcLayer._linkedControlParam;
					dupLayer._linkType = srcLayer._linkType;
					dupLayer._linkedBone = srcLayer._linkedBone;//<<Bone 추가

					//Debug.LogError("TODO : Timeline Layer복사시 Linked Opt Bone도 복사할 것");


					dupTimeline._layers.Add(dupLayer);


					dupTimelineLayerPairs.Add(dupLayer, srcLayer);

					//이게 왜 필요했지????? @ㅅ@??
					////여기서 Link 한번
					//Editor.RefreshTimelineLayers(true);//<<추가 : 타임라인 정보를 리셋
					//Editor._portrait.LinkAndRefreshInEditor();

					//apModifierParamSetGroup modParamSetGroup = null;
					//if (dupTimeline._linkedModifier != null)
					//{
					//	modParamSetGroup = dupTimeline._linkedModifier._paramSetGroup_controller.Find(delegate (apModifierParamSetGroup a)
					//	{
					//		return a._keyAnimTimelineLayer == dupLayer;
					//	});
					//}
				}
			}

			AddAndSyncAnimClipToModifier(dupAnimClip);

			//여기서 리프레시 한번 더
			dupAnimClip.RefreshTimelines(null);


			foreach (KeyValuePair<apAnimTimelineLayer, apAnimTimelineLayer> layerPair in dupTimelineLayerPairs)
			{
				apAnimTimelineLayer dupLayer = layerPair.Key;
				apAnimTimelineLayer srcLayer = layerPair.Value;

				//4. 키프레임도 복사하자.
				for (int iK = 0; iK < srcLayer._keyframes.Count; iK++)
				{
					apAnimKeyframe srcKeyframe = srcLayer._keyframes[iK];

					apAnimKeyframe dupKeyframe = AddAnimKeyframe(srcKeyframe._frameIndex, dupLayer, false, false, false, false);
					if (dupKeyframe == null)
					{
						//EditorUtility.DisplayDialog("Error", "Keyframe Adding Failed", "Closed");
						EditorUtility.DisplayDialog(Editor.GetText(TEXT.AnimDuplicatedFailed_Title),
														Editor.GetText(TEXT.AnimDuplicatedFailed_Body),
														Editor.GetText(TEXT.Close));
						return;
					}

					//Curkey 복사
					dupKeyframe._curveKey = new apAnimCurve(srcKeyframe._curveKey, srcKeyframe._frameIndex);
					dupKeyframe._isKeyValueSet = srcKeyframe._isKeyValueSet;
					dupKeyframe._isActive = srcKeyframe._isActive;



					//Control Type 복사
					//dupKeyframe._conSyncValue_Bool = srcKeyframe._conSyncValue_Bool;
					dupKeyframe._conSyncValue_Int = srcKeyframe._conSyncValue_Int;
					dupKeyframe._conSyncValue_Float = srcKeyframe._conSyncValue_Float;
					dupKeyframe._conSyncValue_Vector2 = srcKeyframe._conSyncValue_Vector2;
					//dupKeyframe._conSyncValue_Vector3 = srcKeyframe._conSyncValue_Vector3;
					//dupKeyframe._conSyncValue_Color = srcKeyframe._conSyncValue_Color;

					dupKeyframe.Link(dupLayer);



					dupKeyframePairs.Add(dupKeyframe, srcKeyframe);
				}
			}

			//각 Keyframe<->Modifier Param 을 일괄 연결한 뒤에..
			AddAndSyncAnimClipToModifier(dupAnimClip);

			//여기서부터 ModMesh/ModBone 를 복사하도록 하자
			foreach (KeyValuePair<apAnimKeyframe, apAnimKeyframe> keyframePair in dupKeyframePairs)
			{
				apAnimKeyframe dupKeyframe = keyframePair.Key;
				apAnimKeyframe srcKeyframe = keyframePair.Value;

				//apAnimTimelineLayer dupLayer = dupKeyframe._parentTimelineLayer;
				//apAnimTimeline dupTimeline = dupKeyframe._parentTimelineLayer._parentTimeline;

				if (dupKeyframe._linkedModMesh_Editor != null && srcKeyframe._linkedModMesh_Editor != null)
				{
					//ModMesh 복사
					apModifiedMesh srcModMesh = srcKeyframe._linkedModMesh_Editor;
					apModifiedMesh dupModMesh = dupKeyframe._linkedModMesh_Editor;


					if (srcModMesh._vertices != null && srcModMesh._vertices.Count > 0)
					{
						if (dupModMesh._vertices == null)
						{ dupModMesh._vertices = new List<apModifiedVertex>(); }
						dupModMesh._vertices.Clear();

						apModifiedVertex srcModVert = null;
						apModifiedVertex dupModVert = null;
						for (int i = 0; i < srcModMesh._vertices.Count; i++)
						{
							srcModVert = srcModMesh._vertices[i];
							dupModVert = new apModifiedVertex();

							dupModVert._modifiedMesh = dupModMesh;
							dupModVert._vertexUniqueID = srcModVert._vertexUniqueID;
							dupModVert._vertIndex = srcModVert._vertIndex;

							dupModVert._mesh = srcModVert._mesh;
							dupModVert._vertex = srcModVert._vertex;
							dupModVert._deltaPos = srcModVert._deltaPos;

							dupModVert._overlapWeight = srcModVert._overlapWeight;

							dupModMesh._vertices.Add(dupModVert);
						}
					}

					if (dupModMesh._transformMatrix == null)
					{ dupModMesh._transformMatrix = new apMatrix(); }
					dupModMesh._transformMatrix.SetMatrix(srcModMesh._transformMatrix);

					dupModMesh._meshColor = srcModMesh._meshColor;
					dupModMesh._isVisible = srcModMesh._isVisible;

					//버그 수정 19.8.23 : Extra Option이 복제 안되는 문제 수정
					dupModMesh._isExtraValueEnabled = srcModMesh._isExtraValueEnabled;
					if(dupModMesh._extraValue == null)
					{
						dupModMesh._extraValue = new apModifiedMesh.ExtraValue();
						dupModMesh._extraValue.Init();
					}

					if (srcModMesh._isExtraValueEnabled && srcModMesh._extraValue != null)
					{
						dupModMesh._extraValue._isDepthChanged =	srcModMesh._extraValue._isDepthChanged;
						dupModMesh._extraValue._deltaDepth =		srcModMesh._extraValue._deltaDepth;
						dupModMesh._extraValue._isTextureChanged =	srcModMesh._extraValue._isTextureChanged;
						dupModMesh._extraValue._linkedTextureData = srcModMesh._extraValue._linkedTextureData;
						dupModMesh._extraValue._textureDataID =		srcModMesh._extraValue._textureDataID;
						dupModMesh._extraValue._weightCutout =		srcModMesh._extraValue._weightCutout;
						dupModMesh._extraValue._weightCutout_AnimPrev = srcModMesh._extraValue._weightCutout_AnimPrev;
						dupModMesh._extraValue._weightCutout_AnimNext = srcModMesh._extraValue._weightCutout_AnimNext;
					}

				}
				else if (dupKeyframe._linkedModBone_Editor != null && srcKeyframe._linkedModBone_Editor != null)
				{
					//Mod Bone 복사
					apModifiedBone srcModBone = srcKeyframe._linkedModBone_Editor;
					apModifiedBone dupModBone = dupKeyframe._linkedModBone_Editor;

					if (dupModBone._transformMatrix == null)
					{
						dupModBone._transformMatrix = new apMatrix();
					}

					dupModBone._transformMatrix.SetMatrix(srcModBone._transformMatrix);
				}
			}

			foreach (KeyValuePair<apAnimTimelineLayer, apAnimTimelineLayer> layerPair in dupTimelineLayerPairs)
			{
				//DupLayer를 Sort하자.
				layerPair.Key.SortAndRefreshKeyframes();//이전


				//Debug.Log("Dup Layer Keyframe Refreshed [" + layerPair.Key.DisplayName + "] : " + nRefreshed);
			}

			#region [미사용 코드] ModMesh 복사 코드였으나 제대로 작동하지 않음
			//foreach (KeyValuePair<apAnimKeyframe, apAnimKeyframe> keyframePair in dupKeyframePairs)
			//{
			//	apAnimKeyframe dupKeyframe = keyframePair.Key;
			//	apAnimKeyframe srcKeyframe = keyframePair.Value;

			//	apAnimTimelineLayer dupLayer = dupKeyframe._parentTimelineLayer;
			//	apAnimTimeline dupTimeline = dupKeyframe._parentTimelineLayer._parentTimeline;

			//	//Anim Modifier 타입이면
			//	//1) Sync를 먼저하고
			//	//2) 등록된 ParamSet을 복사한다.
			//	if (dupTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
			//	{
			//		//Modifier 자동 연결

			//		if (dupLayer._targetParamSetGroup != null)
			//		{

			//			apModifierParamSet paramSet = dupLayer._targetParamSetGroup._paramSetList.Find(delegate (apModifierParamSet a)
			//				{
			//					return a.SyncKeyframe == dupKeyframe;
			//				});

			//			if (paramSet != null && paramSet._meshData.Count > 0)
			//			{
			//				dupKeyframe.LinkModMesh_Editor(paramSet, paramSet._meshData[0]);
			//			}
			//			else
			//			{
			//				if (paramSet == null)
			//				{
			//					Debug.LogError("Anim Mod Param Set 링크 실패 : Param Set이 Null이다. (" + dupLayer._targetParamSetGroup._paramSetList.Count + ")");
			//				}
			//				else
			//				{
			//					Debug.LogError("Anim Mod Param Set 링크 실패 : Mesh Data가 없다.");
			//				}
			//			}
			//		}



			//		if (dupKeyframe._linkedModMesh_Editor != null &&
			//			srcKeyframe._linkedModMesh_Editor != null)
			//		{
			//			apModifiedMesh srcModMesh = srcKeyframe._linkedModMesh_Editor;
			//			apModifiedMesh dupModMesh = dupKeyframe._linkedModMesh_Editor;

			//			dupModMesh.RefreshModifiedValues();

			//			if (srcModMesh._transformUniqueID != dupModMesh._transformUniqueID ||
			//				srcModMesh._isMeshTransform != dupModMesh._isMeshTransform)
			//			{
			//				Debug.LogError("Anim Modifier 타입의 Keyframe Mod Mesh 복사 실패 > Link된 Transform이 다르다.");
			//			}
			//			else
			//			{
			//				int nVertSrc = srcModMesh._vertices.Count;
			//				int nVertDup = dupModMesh._vertices.Count;
			//				if (nVertSrc == nVertDup)
			//				{
			//					for (int iVert = 0; iVert < nVertSrc; iVert++)
			//					{
			//						dupModMesh._vertices[iVert]._deltaPos = srcModMesh._vertices[iVert]._deltaPos;
			//						dupModMesh._vertices[iVert]._overlapWeight = srcModMesh._vertices[iVert]._overlapWeight;
			//					}
			//				}
			//				else
			//				{
			//					Debug.LogError("Anim Modifier 타입의 Keyframe Mod Mesh 복사시 Vertex 개수가 다르다");
			//				}

			//				dupModMesh._transformMatrix.SetMatrix(srcModMesh._transformMatrix);
			//				dupModMesh._meshColor = srcModMesh._meshColor;
			//				dupModMesh._isVisible = srcModMesh._isVisible;
			//			}

			//		}
			//		else
			//		{
			//			Debug.LogError("Anim Modifier 타입의 Keyframe Mod Mesh 복사 실패 > Null 발생 [" +
			//				"Src ModMesh is Null : " + (srcKeyframe._linkedModMesh_Editor == null)
			//				+ " / Dup ModMesh is Null : " + (dupKeyframe._linkedModMesh_Editor == null)
			//				+ " / Link Mod is Null : " + (dupTimeline._linkedModifier == null)
			//				+ " / Link Param Set Group is Null : " + (dupLayer._targetParamSetGroup == null) + "]");
			//		}

			//	}

			//} 
			#endregion


			//버그 수정 19.8.23 : 애니메이션 이벤트도 추가해야한다.
			if(srcAnimClip._animEvents != null && srcAnimClip._animEvents.Count > 0)
			{
				apAnimEvent dupEvent = null;
				apAnimEvent srcEvent = null;
				for (int iEvent = 0; iEvent < srcAnimClip._animEvents.Count; iEvent++)
				{
					srcEvent = srcAnimClip._animEvents[iEvent];
					dupEvent = new apAnimEvent();
					dupEvent.CopyFromAnimEvent(srcEvent);

					dupAnimClip._animEvents.Add(dupEvent);
				}
				
			}




			dupAnimClip.RefreshTimelines(null);

			//AnimClip이 추가되었다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor._portrait.LinkAndRefreshInEditor(true, dupAnimClip._targetMeshGroup);

			//Editor.RefreshTimelineLayers(true);//<<추가 : 타임라인 정보를 리셋
			Editor.RefreshControllerAndHierarchy(true);

			//Refresh 추가
			Editor.Select.RefreshAnimEditing(true);

			Editor.Notification(srcAnimClip._name + " > " + dupAnimClip._name + " Duplicated", true, false);
		}



		public void ImportAnimClip(apRetarget retargetData, apMeshGroup targetMeshGroup, apAnimClip targetAnimClip, bool isMerge)
		{
			if (Editor.Select.AnimClip == null
				|| Editor.Select.AnimClip != targetAnimClip
				|| !retargetData.IsAnimFileLoaded)
			{
				return;
			}

			//Undo
			apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Anim_ImportAnimClip, Editor, Editor._portrait, targetMeshGroup, null, false);


			//유효한 리스트만 정리한다. (타임라인레이어는 따로 정리해야한다.)
			List<apRetargetSubUnit> ret_Transforms = retargetData._animFile._transforms_Total.FindAll(delegate (apRetargetSubUnit a)
			{
				return a._isImported && (a._targetMeshTransform != null || a._targetMeshGroupTransform != null);
			});

			List<apRetargetSubUnit> ret_Bones = retargetData._animFile._bones_Total.FindAll(delegate (apRetargetSubUnit a)
			{
				return a._isImported && a._targetBone != null;
			});

			List<apRetargetControlParam> ret_ControlParams = retargetData._animFile._controlParams.FindAll(delegate (apRetargetControlParam a)
			{
				return a._isImported && a._targetControlParam != null;
			});

			List<apRetargetTimelineUnit> ret_Timelines = retargetData._animFile._timelineUnits.FindAll(delegate (apRetargetTimelineUnit a)
			{
				return a._isImported && a._targetTimeline != null;
			});

			List<apRetargetAnimEvent> ret_Events = retargetData._animFile._animEvents.FindAll(delegate (apRetargetAnimEvent a)
			{
				return a._isImported;
			});

			//0. AnimClip 기본 설정을 적용
			//1. 작업해야할 타임라인을 찾자
			//2. 타임라인의 레이어를 생성 또는 연결한다.
			// - 존재하는 타임라인 레이어라면 키프레임을 모두 삭제한다. (Merge가 아니라면)
			// - 타임라인, 타임라인 레이어의 기본 객체는 ret 리스트를 참조한다. 실패했다면 참조하지 않는다.
			//3. 타임라인의 키프레임들을 생성한다.
			//4. 생성된 키프레임의 modMesh, modBone을 생성+Link하고 값을 넣는다.
			//5. Event를 넣자

			targetAnimClip.SetOption_FPS(retargetData.AnimFile._FPS);
			targetAnimClip.SetOption_StartFrame(retargetData.AnimFile._startFrame);
			targetAnimClip.SetOption_EndFrame(retargetData.AnimFile._endFrame);
			targetAnimClip.SetOption_IsLoop(retargetData.AnimFile._isLoop);



			for (int iT = 0; iT < ret_Timelines.Count; iT++)
			{
				apRetargetTimelineUnit ret_TimelineUnit = ret_Timelines[iT];
				apAnimTimeline targetTimeline = ret_TimelineUnit._targetTimeline;

				List<apRetargetTimelineLayerUnit> ret_Layers = ret_TimelineUnit._layerUnits;
				for (int iLayer = 0; iLayer < ret_Layers.Count; iLayer++)
				{
					//레이어가 존재하고 있다면 기존의 키프레임을 지우고 참조
					//없다면 새로 만든다.

					apRetargetTimelineLayerUnit ret_LayerUnit = ret_Layers[iLayer];


					if (ret_TimelineUnit._linkType == apAnimClip.LINK_TYPE.ControlParam)
					{
						//ControlParam 타입인 경우
						int srcControlParamID = ret_LayerUnit._controlParamID;
						//여기에 해당하는 실제 ControlParamID를 찾자
						apRetargetControlParam targetUnit = ret_ControlParams.Find(delegate (apRetargetControlParam a)
						{
							return a._controlParamUniqueID == srcControlParamID;
						});

						if (targetUnit == null)
						{
							//엥.. 매칭된게 없네요.
							//Import가 안되었나.
							//Debug.LogError("No Imported Control Param : " + ret_LayerUnit._displayName);
							continue;
						}

						//레이어에 연결된 ControlParam
						apControlParam targetControlParam = targetUnit._targetControlParam;

						//기존의 레이어가 있는가
						apAnimTimelineLayer targetTimelineLayer = targetTimeline.GetTimelineLayer(targetControlParam);
						if (targetTimelineLayer != null)
						{
							//<1> 기존 레이어가 있다.
							if (!isMerge)
							{
								//만약 Merge가 아닌 Replace라면 기존의 키프레임을 모두 지워야함
								List<apAnimKeyframe> prevKeyframes = new List<apAnimKeyframe>();
								for (int iKey = 0; iKey < targetTimelineLayer._keyframes.Count; iKey++)
								{
									prevKeyframes.Add(targetTimelineLayer._keyframes[iKey]);
								}
								RemoveKeyframes(prevKeyframes, false);
							}
						}
						else
						{
							//<2> 레이어를 새로 만들어야 한다.
							targetTimelineLayer = AddAnimTimelineLayer(targetControlParam, targetTimeline, false);
							if (targetTimelineLayer == null)
							{
								Debug.LogError("TImelineLayer Add Error");
								continue;
							}
						}

						//타겟 타임라인 레이어에 Keyframe을 채워넣어야 한다.
						List<apRetargetKeyframeUnit> ret_Keyframes = ret_LayerUnit._keyframeUnits;
						for (int iKey = 0; iKey < ret_Keyframes.Count; iKey++)
						{
							//키프레임을 추가한다.
							apRetargetKeyframeUnit keyUnit = ret_Keyframes[iKey];

							int frameIndex = keyUnit._frameIndex;

							//만약 겹치는 Keyframe이 있다면 삭제
							apAnimKeyframe overlapKeyframe = targetTimelineLayer.GetKeyframeByFrameIndex(frameIndex);
							if (overlapKeyframe != null)
							{
								RemoveKeyframe(overlapKeyframe, false);
							}

							apAnimKeyframe addedKeyframe = AddAnimKeyframe(frameIndex, targetTimelineLayer, false, false, false, false);
							if (addedKeyframe == null)
							{
								Debug.LogError("Keyframe 생성 실패");
								continue;
							}

							//설정값을 복사하자
							addedKeyframe._isKeyValueSet = keyUnit._isKeyValueSet;
							addedKeyframe._isActive = keyUnit._isActive;

							addedKeyframe._isLoopAsStart = keyUnit._isLoopAsStart;
							addedKeyframe._isLoopAsEnd = keyUnit._isLoopAsEnd;
							addedKeyframe._loopFrameIndex = keyUnit._loopFrameIndex;

							addedKeyframe._activeFrameIndexMin = keyUnit._activeFrameIndexMin;
							addedKeyframe._activeFrameIndexMax = keyUnit._activeFrameIndexMax;

							addedKeyframe._activeFrameIndexMin_Dummy = keyUnit._activeFrameIndexMin_Dummy;
							addedKeyframe._activeFrameIndexMax_Dummy = keyUnit._activeFrameIndexMax_Dummy;

							if (addedKeyframe._curveKey != null)
							{
								addedKeyframe._curveKey = new apAnimCurve();
							}
							addedKeyframe._curveKey._prevTangentType = keyUnit._curve_PrevTangentType;
							addedKeyframe._curveKey._prevSmoothX = keyUnit._curve_PrevSmoothX;
							addedKeyframe._curveKey._prevSmoothY = keyUnit._curve_PrevSmoothY;
							addedKeyframe._curveKey._nextTangentType = keyUnit._curve_NextTangentType;
							addedKeyframe._curveKey._nextSmoothX = keyUnit._curve_NextSmoothX;
							addedKeyframe._curveKey._nextSmoothY = keyUnit._curve_NextSmoothY;

							addedKeyframe._conSyncValue_Int = keyUnit._conSyncValue_Int;
							addedKeyframe._conSyncValue_Float = keyUnit._conSyncValue_Float;
							addedKeyframe._conSyncValue_Vector2 = keyUnit._conSyncValue_Vector2;
						}


					}
					else if (ret_TimelineUnit._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
					{
						//Animated Modifier인 경우
						apAnimTimelineLayer.LINK_MOD_TYPE linkModType = apAnimTimelineLayer.LINK_MOD_TYPE.None;
						int srcTransformID = -1;
						int srcBoneID = -1;
						apRetargetSubUnit targetUnit = null;

						if (ret_LayerUnit._linkModType == apAnimTimelineLayer.LINK_MOD_TYPE.MeshTransform)
						{
							linkModType = apAnimTimelineLayer.LINK_MOD_TYPE.MeshTransform;
							srcTransformID = ret_LayerUnit._transformID;

							targetUnit = ret_Transforms.Find(delegate (apRetargetSubUnit a)
							{
								return a._type == apRetargetSubUnit.TYPE.MeshTransform &&
										a._uniqueID == srcTransformID;
							});
						}
						else if (ret_LayerUnit._linkModType == apAnimTimelineLayer.LINK_MOD_TYPE.MeshGroupTransform)
						{
							linkModType = apAnimTimelineLayer.LINK_MOD_TYPE.MeshGroupTransform;
							srcTransformID = ret_LayerUnit._transformID;

							targetUnit = ret_Transforms.Find(delegate (apRetargetSubUnit a)
							{
								return a._type == apRetargetSubUnit.TYPE.MeshGroupTransform &&
										a._uniqueID == srcTransformID;
							});
						}
						else if (ret_LayerUnit._linkModType == apAnimTimelineLayer.LINK_MOD_TYPE.Bone)
						{
							linkModType = apAnimTimelineLayer.LINK_MOD_TYPE.Bone;
							srcBoneID = ret_LayerUnit._boneID;

							targetUnit = ret_Bones.Find(delegate (apRetargetSubUnit a)
							{
								return a._type == apRetargetSubUnit.TYPE.Bone &&
										a._uniqueID == srcBoneID;
							});
						}
						else
						{
							Debug.LogError("Wrong Link Mod Type");
							continue;
						}


						//여기에 해당하는 실제 SubUnit을 찾자


						if (targetUnit == null)
						{
							//엥.. 매칭된게 없네요.
							//Import가 안되었나.
							//Debug.LogError("No Imported Transform/Bone : " + ret_LayerUnit._displayName);
							continue;
						}

						//레이어에 연결된 Transform/Bone
						apTransform_Mesh targetMeshTransform = null;
						apTransform_MeshGroup targetMeshGroupTransform = null;
						apBone targetBone = null;

						apAnimTimelineLayer targetTimelineLayer = null;

						switch (linkModType)
						{
							case apAnimTimelineLayer.LINK_MOD_TYPE.MeshTransform:
								targetMeshTransform = targetUnit._targetMeshTransform;
								targetTimelineLayer = targetTimeline.GetTimelineLayer(targetMeshTransform);
								break;

							case apAnimTimelineLayer.LINK_MOD_TYPE.MeshGroupTransform:
								targetMeshGroupTransform = targetUnit._targetMeshGroupTransform;
								targetTimelineLayer = targetTimeline.GetTimelineLayer(targetMeshGroupTransform);
								break;

							case apAnimTimelineLayer.LINK_MOD_TYPE.Bone:
								targetBone = targetUnit._targetBone;
								targetTimelineLayer = targetTimeline.GetTimelineLayer(targetBone);
								break;
						}


						//기존의 레이어가 있는가
						if (targetTimelineLayer != null)
						{
							//<1> 기존 레이어가 있다.
							if (!isMerge)
							{
								//만약 Merge가 아닌 Replace라면 기존의 키프레임을 모두 지워야함
								List<apAnimKeyframe> prevKeyframes = new List<apAnimKeyframe>();
								for (int iKey = 0; iKey < targetTimelineLayer._keyframes.Count; iKey++)
								{
									prevKeyframes.Add(targetTimelineLayer._keyframes[iKey]);
								}
								RemoveKeyframes(prevKeyframes, false);
							}
						}
						else
						{
							//<2> 레이어를 새로 만들어야 한다.
							switch (linkModType)
							{
								case apAnimTimelineLayer.LINK_MOD_TYPE.MeshTransform:
									targetTimelineLayer = AddAnimTimelineLayer(targetMeshTransform, targetTimeline, false);
									break;

								case apAnimTimelineLayer.LINK_MOD_TYPE.MeshGroupTransform:
									targetTimelineLayer = AddAnimTimelineLayer(targetMeshGroupTransform, targetTimeline, false);
									break;

								case apAnimTimelineLayer.LINK_MOD_TYPE.Bone:
									targetTimelineLayer = AddAnimTimelineLayer(targetBone, targetTimeline, false);
									break;
							}



							if (targetTimelineLayer == null)
							{
								Debug.LogError("TImelineLayer Add Error");
								continue;
							}
						}

						//타임라인 정보를 리셋
						Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null);

						//AnimClip이 추가되었다.
						Editor.OnAnyObjectAddedOrRemoved();
						Editor._portrait.LinkAndRefreshInEditor(true, null);

						//타겟 타임라인 레이어에 Keyframe을 채워넣어야 한다.
						List<apRetargetKeyframeUnit> ret_Keyframes = ret_LayerUnit._keyframeUnits;
						for (int iKey = 0; iKey < ret_Keyframes.Count; iKey++)
						{
							//키프레임을 추가한다.
							apRetargetKeyframeUnit keyUnit = ret_Keyframes[iKey];

							int frameIndex = keyUnit._frameIndex;

							//만약 겹치는 Keyframe이 있다면 삭제
							apAnimKeyframe overlapKeyframe = targetTimelineLayer.GetKeyframeByFrameIndex(frameIndex);
							if (overlapKeyframe != null)
							{
								RemoveKeyframe(overlapKeyframe, false);
							}

							apAnimKeyframe addedKeyframe = AddAnimKeyframe(frameIndex, targetTimelineLayer, false, false, false, false);
							if (addedKeyframe == null)
							{
								Debug.LogError("Keyframe 생성 실패");
								continue;
							}

							//설정값을 복사하자
							addedKeyframe._isKeyValueSet = keyUnit._isKeyValueSet;
							addedKeyframe._isActive = keyUnit._isActive;

							addedKeyframe._isLoopAsStart = keyUnit._isLoopAsStart;
							addedKeyframe._isLoopAsEnd = keyUnit._isLoopAsEnd;
							addedKeyframe._loopFrameIndex = keyUnit._loopFrameIndex;

							addedKeyframe._activeFrameIndexMin = keyUnit._activeFrameIndexMin;
							addedKeyframe._activeFrameIndexMax = keyUnit._activeFrameIndexMax;

							addedKeyframe._activeFrameIndexMin_Dummy = keyUnit._activeFrameIndexMin_Dummy;
							addedKeyframe._activeFrameIndexMax_Dummy = keyUnit._activeFrameIndexMax_Dummy;

							if (addedKeyframe._curveKey != null)
							{
								addedKeyframe._curveKey = new apAnimCurve();
							}
							addedKeyframe._curveKey._prevTangentType = keyUnit._curve_PrevTangentType;
							addedKeyframe._curveKey._prevSmoothX = keyUnit._curve_PrevSmoothX;
							addedKeyframe._curveKey._prevSmoothY = keyUnit._curve_PrevSmoothY;
							addedKeyframe._curveKey._nextTangentType = keyUnit._curve_NextTangentType;
							addedKeyframe._curveKey._nextSmoothX = keyUnit._curve_NextSmoothX;
							addedKeyframe._curveKey._nextSmoothY = keyUnit._curve_NextSmoothY;

							addedKeyframe._conSyncValue_Int = keyUnit._conSyncValue_Int;
							addedKeyframe._conSyncValue_Float = keyUnit._conSyncValue_Float;
							addedKeyframe._conSyncValue_Vector2 = keyUnit._conSyncValue_Vector2;


							//ModMesh/ModBone 값을 넣어주자
							switch (linkModType)
							{
								case apAnimTimelineLayer.LINK_MOD_TYPE.MeshTransform:
								case apAnimTimelineLayer.LINK_MOD_TYPE.MeshGroupTransform:
									if (addedKeyframe._linkedModMesh_Editor != null)
									{
										addedKeyframe._linkedModMesh_Editor._transformMatrix.SetMatrix(keyUnit._modTransformMatrix);
										addedKeyframe._linkedModMesh_Editor._meshColor = keyUnit._modMeshColor;
										addedKeyframe._linkedModMesh_Editor._isVisible = keyUnit._modVisible;
									}
									else
									{
										Debug.LogError("No LinkedModMesh");
									}
									break;

								case apAnimTimelineLayer.LINK_MOD_TYPE.Bone:
									if (addedKeyframe._linkedModBone_Editor != null)
									{
										addedKeyframe._linkedModBone_Editor._transformMatrix.SetMatrix(keyUnit._modTransformMatrix);
									}
									else
									{
										Debug.LogError("No LinkedModBone");
									}
									break;
							}
						}
					}
				}

			}

			//이벤트를 넣자
			//만약 Replace라면 기존 이벤트 삭제
			if (!isMerge)
			{
				targetAnimClip._animEvents.Clear();
			}
			else
			{
				//추가하기 전에
				//동일한 이름 + 동일한 프레임이 있다면 삭제하자 (Merge라고 하더라도)

				for (int i = 0; i < ret_Events.Count; i++)
				{
					apRetargetAnimEvent ret_Event = ret_Events[i];

					targetAnimClip._animEvents.RemoveAll(delegate (apAnimEvent a)
					{
						return string.Equals(a._eventName, ret_Event._eventName)
								&& a._frameIndex == ret_Event._frameIndex
								&& a._callType == ret_Event._callType;
					});
				}

			}

			for (int i = 0; i < ret_Events.Count; i++)
			{
				apRetargetAnimEvent ret_Event = ret_Events[i];

				apAnimEvent newEvent = new apAnimEvent();

				newEvent._frameIndex = ret_Event._frameIndex;
				newEvent._frameIndex_End = ret_Event._frameIndex_End;
				newEvent._eventName = ret_Event._eventName;

				newEvent._callType = ret_Event._callType;

				if (newEvent._subParams == null)
				{
					newEvent._subParams = new List<apAnimEvent.SubParameter>();
				}

				if (ret_Event._subParams != null && ret_Event._subParams.Count > 0)
				{
					for (int iSubParam = 0; iSubParam < ret_Event._subParams.Count; iSubParam++)
					{
						apAnimEvent.SubParameter newSubParam = new apAnimEvent.SubParameter();
						apRetargetAnimEvent.SubParameter ret_SubParam = ret_Event._subParams[iSubParam];

						newSubParam._paramType = ret_SubParam._paramType;

						newSubParam._boolValue = ret_SubParam._boolValue;
						newSubParam._intValue = ret_SubParam._intValue;
						newSubParam._floatValue = ret_SubParam._floatValue;
						newSubParam._vec2Value = ret_SubParam._vec2Value;
						newSubParam._strValue = ret_SubParam._strValue;

						newSubParam._intValue_End = ret_SubParam._intValue_End;
						newSubParam._floatValue_End = ret_SubParam._floatValue_End;
						newSubParam._vec2Value_End = ret_SubParam._vec2Value_End;

						newEvent._subParams.Add(newSubParam);
					}
				}


				targetAnimClip._animEvents.Add(newEvent);
			}

			//이벤트 정렬도 하자
			targetAnimClip._animEvents.Sort(delegate (apAnimEvent a, apAnimEvent b)
			{
				if (a._frameIndex == b._frameIndex)
				{
					return string.Compare(a._eventName, b._eventName);
				}
				return a._frameIndex - b._frameIndex;
			});


			//타임라인 정보를 리셋
			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null);

			Editor._portrait.LinkAndRefreshInEditor(true, targetAnimClip._targetMeshGroup);//다시 리셋
			Editor.RefreshControllerAndHierarchy(false);


			//Refresh 추가
			Editor.Select.RefreshAnimEditing(true);

			Editor.Select.AutoSelectAnimWorkKeyframe();


			//완료되었쩌여
			EditorUtility.DisplayDialog(Editor.GetText(TEXT.Retarget_ImportAnimComplete_Title),
															Editor.GetText(TEXT.Retarget_ImportAnimComplete_Body),
															Editor.GetText(TEXT.Close));

		}




		public apAnimTimeline AddAnimTimeline(apAnimClip.LINK_TYPE linkType, int modifierUniqueID, apAnimClip targetAnimClip, bool errorMsg = true, bool isSetRecordAndRefresh = true)
		{
			if (targetAnimClip == null)
			{
				return null;
			}

			//Timeline을 추가해야한다.
			if (isSetRecordAndRefresh)
			{
				apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Anim_AddTimeline, Editor, Editor._portrait, targetAnimClip._targetMeshGroup, null, false);
			}

			int nextTimelineID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimTimeline);
			if (nextTimelineID < 0)
			{
				if (errorMsg)
				{
					//EditorUtility.DisplayDialog("Error", "Timeline Adding Failed", "Close");
					EditorUtility.DisplayDialog(Editor.GetText(TEXT.AnimTimelineAddFailed_Title),
													Editor.GetText(TEXT.AnimTimelineAddFailed_Body),
													Editor.GetText(TEXT.Close));
				}
				return null;
			}



			apAnimTimeline newTimeline = new apAnimTimeline();
			newTimeline.Init(linkType, nextTimelineID, modifierUniqueID, targetAnimClip);

			targetAnimClip._timelines.Add(newTimeline);

			newTimeline.Link(targetAnimClip);

			if (isSetRecordAndRefresh)
			{
				//바로 Timeline을 선택한다.
				Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null);//<<추가 : 타임라인 정보를 리셋

				Editor.Select.SetAnimTimeline(newTimeline, true);
				Editor.RefreshControllerAndHierarchy(false);

				//Refresh 추가
				Editor.Select.RefreshAnimEditing(true);
			}

			//추가 : MeshGroup Hierarchy를 갱신합시다.
			Editor.Hierarchy_MeshGroup.RefreshUnits();
			Editor.Hierarchy_AnimClip.RefreshUnits();

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			return newTimeline;
		}

		public void RemoveAnimTimeline(apAnimTimeline animTimeline)
		{
			if (animTimeline == null)
			{
				return;
			}
			//Undo - Remove AnimTimeline
			//apEditorUtil.SetRecord_PortraitMeshGroupModifier(apUndoGroupData.ACTION.Anim_RemoveTimeline, 
			//													Editor, 
			//													Editor._portrait, 
			//													animTimeline._parentAnimClip._targetMeshGroup, 
			//													animTimeline._linkedModifier, null, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Timeline");

			Editor._portrait.PushUnusedID(apIDManager.TARGET.AnimTimeline, animTimeline._uniqueID);


			//선택중이면 제외
			Editor.Select.CancelAnimEditing();
			Editor.Select.SetAnimTimeline(null, true);
			if (Editor.Select.AnimTimeline != null)
			{
				//Debug.LogError("Error!!! : AnimTimeline 해제가 안되었다!!");
			}

			apAnimClip parentAnimClip = animTimeline._parentAnimClip;
			apMeshGroup targetMeshGroup = null;
			if (parentAnimClip == null)
			{
				//?? 없네요.. 에러가..
				//Debug.LogError("Error : AnimClip이 연결되지 않은 Timeline 제거");
			}
			else
			{
				animTimeline._linkedModifier = null;
				animTimeline._modifierUniqueID = -1;
				//뭔가 더 있어야하지 않으려나..
				targetMeshGroup = parentAnimClip._targetMeshGroup;

				parentAnimClip._timelines.Remove(animTimeline);

				//자동 삭제도 수행한다.
				parentAnimClip.RemoveUnlinkedTimeline();
			}

			//객체가 추가/삭제시 호출
			Editor.OnAnyObjectAddedOrRemoved();

			//전체 Refresh를 해야한다.
			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null);//<<추가 : 타임라인 정보를 리셋

			Editor._portrait.LinkAndRefreshInEditor(false, targetMeshGroup);
			Editor.RefreshControllerAndHierarchy(false);

			//Refresh 추가
			Editor.Select.RefreshAnimEditing(true);

		}



		public apAnimTimelineLayer AddAnimTimelineLayer(object targetObject, apAnimTimeline parentTimeline, bool isRecordAndRefresh = true)
		{
			if (targetObject == null || parentTimeline == null)
			{
				return null;
			}

			if (isRecordAndRefresh)
			{
				//Undo - Add TimelineLayer
				apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Anim_AddTimelineLayer, Editor, Editor._portrait, parentTimeline._parentAnimClip._targetMeshGroup, null, false);
			}

			//이미 추가되었으면 리턴
			if (parentTimeline.IsObjectAddedInLayers(targetObject))
			{
				return null;
			}

			int nextLayerID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimTimelineLayer);
			if (nextLayerID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Timeline Layer Add Failed", "Close");
				EditorUtility.DisplayDialog(Editor.GetText(TEXT.AnimTimelineLayerAddFailed_Title),
												Editor.GetText(TEXT.AnimTimelineLayerAddFailed_Body),
												Editor.GetText(TEXT.Close));
				return null;
			}





			apAnimTimelineLayer newLayer = new apAnimTimelineLayer();
			newLayer.Link(parentTimeline._parentAnimClip, parentTimeline);

			switch (parentTimeline._linkType)
			{
				case apAnimClip.LINK_TYPE.AnimatedModifier:
					{
						int transformID = -1;
						bool isMeshTransform = false;
						if (targetObject is apTransform_Mesh)
						{
							apTransform_Mesh meshTransform = targetObject as apTransform_Mesh;
							transformID = meshTransform._transformUniqueID;
							isMeshTransform = true;

							newLayer.Init_TransformOfModifier(parentTimeline, nextLayerID, transformID, isMeshTransform);


						}
						else if (targetObject is apTransform_MeshGroup)
						{
							apTransform_MeshGroup meshGroupTransform = targetObject as apTransform_MeshGroup;
							transformID = meshGroupTransform._transformUniqueID;
							isMeshTransform = false;

							newLayer.Init_TransformOfModifier(parentTimeline, nextLayerID, transformID, isMeshTransform);
						}
						else if (targetObject is apBone)
						{
							apBone bone = targetObject as apBone;
							newLayer.Init_Bone(parentTimeline, nextLayerID, bone._uniqueID);
						}
						else
						{
							//?
							Debug.LogError(">> [Unknown Type]");
						}
					}
					break;


				case apAnimClip.LINK_TYPE.ControlParam:
					{
						int controlParamID = -1;
						if (targetObject is apControlParam)
						{
							apControlParam controlParam = targetObject as apControlParam;
							controlParamID = controlParam._uniqueID;
						}
						newLayer.Init_ControlParam(parentTimeline, nextLayerID, controlParamID);
					}
					break;

				default:
					Debug.LogError("TODO : 정의되지 않은 타입의 Layer 추가 코드 필요[" + parentTimeline._linkType + "]");
					break;
			}

			parentTimeline._layers.Add(newLayer);

			//전체 Refresh를 해야한다.
			if (parentTimeline._parentAnimClip._targetMeshGroup != null)
			{
				parentTimeline._parentAnimClip._targetMeshGroup.LinkModMeshRenderUnits();
				parentTimeline._parentAnimClip._targetMeshGroup.RefreshModifierLink();
				parentTimeline._parentAnimClip._targetMeshGroup._modifierStack.InitModifierCalculatedValues();


				//추가 : ExMode에 추가한다.
				Editor.Select.RefreshMeshGroupExEditingFlags(
								parentTimeline._parentAnimClip._targetMeshGroup,
								parentTimeline._linkedModifier,
								null,
								parentTimeline._parentAnimClip,
								true);
			}

			if (isRecordAndRefresh)
			{
				//이전
				//Editor.RefreshTimelineLayers(true);//<<추가 : 타임라인 정보를 리셋

				//이후 19.5.21 : 새로운 레이어를 위주로 갱신
				Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, newLayer);



				//4.1 추가된 데이터가 있으면 일단 호출한다.
				Editor.OnAnyObjectAddedOrRemoved();


				Editor._portrait.LinkAndRefreshInEditor(false, parentTimeline._parentAnimClip._targetMeshGroup);
				Editor.RefreshControllerAndHierarchy(false);

				if (parentTimeline._linkedModifier != null)
				{
					parentTimeline._linkedModifier.RefreshParamSet();
				}

				Editor.Select.SetAnimTimelineLayer(newLayer, true);
				Editor.Select.AutoSelectAnimTimelineLayer(true, false);//<<타임라인 자동 스크롤 선택

				//Refresh 추가
				Editor.Select.RefreshAnimEditing(true);

				Editor.Select.AutoSelectAnimWorkKeyframe();
			}

			return newLayer;
		}


		public apAnimTimelineLayer AddAnimTimelineLayerForAllTransformObject(apMeshGroup parentMeshGroup, bool isTargetTransform, bool isAddChildTransformAddable, apAnimTimeline parentTimeline)
		{
			if (parentMeshGroup == null || parentTimeline == null)
			{
				return null;
			}

			//Undo - Add TimelineLayer
			apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Anim_AddTimelineLayer, Editor, Editor._portrait, parentMeshGroup, null, false);

			List<object> targetObjects = new List<object>();

			//목표를 리스트로 찾자
			FindChildTransformsOrBones(parentMeshGroup, parentMeshGroup._rootMeshGroupTransform, isTargetTransform, targetObjects, isAddChildTransformAddable);

			apAnimTimelineLayer firstLayer = null;
			int startFrame = parentTimeline._parentAnimClip.StartFrame;

			List<apAnimTimelineLayer> addedLayers = new List<apAnimTimelineLayer>();
			for (int iTargetObjects = 0; iTargetObjects < targetObjects.Count; iTargetObjects++)
			{
				object targetObject = targetObjects[iTargetObjects];

				//이미 추가되었으면 리턴
				if (parentTimeline.IsObjectAddedInLayers(targetObject))
				{
					continue;
				}

				int nextLayerID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimTimelineLayer);
				if (nextLayerID < 0)
				{
					//EditorUtility.DisplayDialog("Error", "Timeline Layer Add Failed", "Close");
					EditorUtility.DisplayDialog(Editor.GetText(TEXT.AnimTimelineLayerAddFailed_Title),
													Editor.GetText(TEXT.AnimTimelineLayerAddFailed_Body),
													Editor.GetText(TEXT.Close));

					return null;
				}





				apAnimTimelineLayer newLayer = new apAnimTimelineLayer();
				newLayer.Link(parentTimeline._parentAnimClip, parentTimeline);

				if (firstLayer == null)
				{
					firstLayer = newLayer;
				}

				switch (parentTimeline._linkType)
				{
					case apAnimClip.LINK_TYPE.AnimatedModifier:
						{
							int transformID = -1;
							bool isMeshTransform = false;
							if (targetObject is apTransform_Mesh)
							{
								apTransform_Mesh meshTransform = targetObject as apTransform_Mesh;
								transformID = meshTransform._transformUniqueID;
								isMeshTransform = true;

								newLayer.Init_TransformOfModifier(parentTimeline, nextLayerID, transformID, isMeshTransform);


							}
							else if (targetObject is apTransform_MeshGroup)
							{
								apTransform_MeshGroup meshGroupTransform = targetObject as apTransform_MeshGroup;
								transformID = meshGroupTransform._transformUniqueID;
								isMeshTransform = false;

								newLayer.Init_TransformOfModifier(parentTimeline, nextLayerID, transformID, isMeshTransform);
							}
							else if (targetObject is apBone)
							{
								apBone bone = targetObject as apBone;
								newLayer.Init_Bone(parentTimeline, nextLayerID, bone._uniqueID);
							}
							else
							{
								//?
								Debug.LogError(">> [Unknown Type]");
							}
						}
						break;


					case apAnimClip.LINK_TYPE.ControlParam:
						{
							int controlParamID = -1;
							if (targetObject is apControlParam)
							{
								apControlParam controlParam = targetObject as apControlParam;
								controlParamID = controlParam._uniqueID;
							}
							newLayer.Init_ControlParam(parentTimeline, nextLayerID, controlParamID);
						}
						break;

					default:
						Debug.LogError("TODO : 정의되지 않은 타입의 Layer 추가 코드 필요[" + parentTimeline._linkType + "]");
						break;
				}

				parentTimeline._layers.Add(newLayer);


				//시작 프레임에 Keyframe을 추가하자
				//AddAnimKeyframe(startFrame, newLayer, false, false, false, false);
				//>> 이걸 Refresh 후로 미루자
				addedLayers.Add(newLayer);
			}


			//전체 Refresh를 해야한다.
			if (parentTimeline._parentAnimClip._targetMeshGroup != null)
			{
				parentTimeline._parentAnimClip._targetMeshGroup.LinkModMeshRenderUnits();
				parentTimeline._parentAnimClip._targetMeshGroup.RefreshModifierLink();
				parentTimeline._parentAnimClip._targetMeshGroup._modifierStack.InitModifierCalculatedValues();

				//추가 : ExMode에 추가한다.
				Editor.Select.RefreshMeshGroupExEditingFlags(
								parentTimeline._parentAnimClip._targetMeshGroup,
								parentTimeline._linkedModifier,
								null,
								parentTimeline._parentAnimClip,
								true);
			}

			//전체 타임라인 갱신
			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null);

			Editor._portrait.LinkAndRefreshInEditor(false, null);
			Editor.RefreshControllerAndHierarchy(false);

			//if(parentTimeline._linkedModifier != null)
			//{
			//	Debug.Log("AnimLayer Add -> RefreshParamSet");
			//	parentTimeline._linkedModifier.RefreshParamSet();
			//}

			for (int i = 0; i < addedLayers.Count; i++)
			{
				AddAnimKeyframe(startFrame, addedLayers[i], false, false, false, false);
			}

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			//다시 Refresh
			Editor._portrait.LinkAndRefreshInEditor(false, parentTimeline._parentAnimClip._targetMeshGroup);
			Editor.RefreshControllerAndHierarchy(true);//<<true : Timeline도 모두 갱신한다.

			Editor.Select.SetAnimTimelineLayer(firstLayer, true);

			//Refresh 추가
			Editor.Select.RefreshAnimEditing(true);

			Editor.Select.AutoSelectAnimWorkKeyframe();

			return firstLayer;
		}

		/// <summary>
		/// AnimTimelineLayer에 오브젝트를 추가하기 위해 "모든 Mesh/MeshGroup Transform"을 찾거나 "Bone"을 찾는 함수
		/// </summary>
		public void FindChildTransformsOrBones(apMeshGroup meshGroup, apTransform_MeshGroup meshGroupTransform, bool isTargetTransform, List<object> resultList, bool isChildTransformSupport)
		{
			if (isTargetTransform)
			{
				if (meshGroup != meshGroupTransform._meshGroup)
				{
					resultList.Add(meshGroupTransform);
				}


				for (int i = 0; i < meshGroupTransform._meshGroup._childMeshTransforms.Count; i++)
				{
					resultList.Add(meshGroupTransform._meshGroup._childMeshTransforms[i]);
				}
			}
			else
			{
				//<BONE_EDIT> Recursive로 모든 오브젝트를 찾기 때문에 Bone은 자기 자신만 체크하면 된다.
				List<apBone> bones = new List<apBone>();
				for (int i = 0; i < meshGroupTransform._meshGroup._boneList_Root.Count; i++)
				{
					MakeRecursiveList(meshGroupTransform._meshGroup._boneList_Root[i], bones);
				}

				for (int i = 0; i < bones.Count; i++)
				{
					resultList.Add(bones[i]);
				}

			}

			if (isChildTransformSupport)
			{
				for (int i = 0; i < meshGroupTransform._meshGroup._childMeshGroupTransforms.Count; i++)
				{
					apTransform_MeshGroup childMeshGroup = meshGroupTransform._meshGroup._childMeshGroupTransforms[i];
					FindChildTransformsOrBones(meshGroup, childMeshGroup, isTargetTransform, resultList, isChildTransformSupport);
				}
			}
		}

		private void MakeRecursiveList(apBone targetBone, List<apBone> resultList)
		{
			resultList.Add(targetBone);
			if (targetBone._childBones != null)
			{
				for (int i = 0; i < targetBone._childBones.Count; i++)
				{
					MakeRecursiveList(targetBone._childBones[i], resultList);
				}
			}
		}


		public void RemoveAnimTimelineLayer(apAnimTimelineLayer animTimelineLayer)
		{
			if (animTimelineLayer == null)
			{
				return;
			}
			//Undo - Remove Anim Timeline Layer
			//apEditorUtil.SetRecord_PortraitMeshGroupModifier(apUndoGroupData.ACTION.Anim_RemoveTimelineLayer, 
			//													Editor, 
			//													Editor._portrait, 
			//													animTimelineLayer._parentAnimClip._targetMeshGroup, 
			//													animTimelineLayer._parentTimeline._linkedModifier, null, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Timeline Layer");

			//ID 반납
			Editor._portrait.PushUnusedID(apIDManager.TARGET.AnimTimelineLayer, animTimelineLayer._uniqueID);



			//선택중이면 제외
			if (Editor.Select.AnimTimelineLayer == animTimelineLayer)
			{
				Editor.Select.SetAnimTimelineLayer(null, true);
			}

			apMeshGroup targetMeshGroup = null;
			apAnimTimeline parentTimeline = animTimelineLayer._parentTimeline;
			if (parentTimeline != null)
			{
				parentTimeline._layers.Remove(animTimelineLayer);

				animTimelineLayer._transformID = -1;
				animTimelineLayer._boneID = -1;
				animTimelineLayer._controlParamID = -1;

				//자동 삭제도 해준다.
				parentTimeline.RemoveUnlinkedLayer();

				if (parentTimeline._parentAnimClip != null)
				{
					targetMeshGroup = parentTimeline._parentAnimClip._targetMeshGroup;
				}
			}
			else
			{
				Debug.LogError("Error : Parent Timeline이 없는 Layer 제거 시도");
			}

			//객체가 추가/삭제시 호출
			Editor.OnAnyObjectAddedOrRemoved();

			//전체 Refresh를 해야한다.
			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null);

			Editor._portrait.LinkAndRefreshInEditor(false, targetMeshGroup);
			Editor.RefreshControllerAndHierarchy(false);

			//Refresh 추가
			Editor.Select.RefreshAnimEditing(true);

			//추가 : 19.11.23
			apEditorUtil.ReleaseGUIFocus();
		}

		//TODO : 이거 사용해야한다. + 추가하면 Modifier와 연동바로 할 것
		public apAnimKeyframe AddAnimKeyframe(int targetFrame, apAnimTimelineLayer parentLayer, bool isMakeCurrentBlendData, bool isErrorMsg = true, bool isSetRecord = true, bool isRefresh = true)
		{
			if (parentLayer == null)
			{
				return null;
			}

			if (isSetRecord)
			{
				apEditorUtil.SetRecord_PortraitMeshGroupModifier(apUndoGroupData.ACTION.Anim_AddKeyframe, Editor,
																	Editor._portrait,
																	parentLayer._parentAnimClip._targetMeshGroup,
																	parentLayer._parentTimeline._linkedModifier, null, false);
			}

			apAnimKeyframe existFrame = parentLayer.GetKeyframeByFrameIndex(targetFrame);
			if (existFrame != null)
			{
				//이미 해당 프레임에 값이 있다.
				//EditorUtility.DisplayDialog("Error", "Keyframe is already Added", "Closed");
				EditorUtility.DisplayDialog(Editor.GetText(TEXT.AnimKeyframeAddFailed_Title),
												Editor.GetText(TEXT.AnimKeyframeAddFailed_Body_Already),
												Editor.GetText(TEXT.Close));

				return null;
			}

			int nextKeyframeID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimKeyFrame);
			if (nextKeyframeID < 0)
			{
				if (isErrorMsg)
				{
					//EditorUtility.DisplayDialog("Error", "Keyframe Adding Failed", "Closed");
					EditorUtility.DisplayDialog(Editor.GetText(TEXT.AnimKeyframeAddFailed_Title),
													Editor.GetText(TEXT.AnimKeyframeAddFailed_Body_Error),
													Editor.GetText(TEXT.Close));

				}
				return null;
			}


			bool isIntControlParamLayer = (parentLayer._linkType == apAnimClip.LINK_TYPE.ControlParam)
												&& (parentLayer._linkedControlParam != null && parentLayer._linkedControlParam._valueType == apControlParam.TYPE.Int);


			apAnimKeyframe newKeyframe = new apAnimKeyframe();
			newKeyframe.Init(nextKeyframeID, targetFrame, isIntControlParamLayer);
			newKeyframe.Link(parentLayer);




			parentLayer._keyframes.Add(newKeyframe);

			if (parentLayer._parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
			{
				//Modifier에 연동되는 타입이라면
				//전체적으로 돌면서 자동으로 Modifier와 연동을 해보자
				AddAndSyncAnimClipToModifier(parentLayer._parentTimeline._parentAnimClip);
			}

			if (isMakeCurrentBlendData)
			{
				if (apEditor.IS_DEBUG_DETAILED)
				{
					Debug.LogError("TODO : Set Key -> isMakeCurrentBlendData 현재 중간 값을 이용해서 ModMesh 값을 세팅한다.");

				}
				MakeBlendModifiedDataAnimated(newKeyframe);
			}


			//전체 Refresh를 해야한다.
			if (isRefresh)
			{
				Editor._portrait.LinkAndRefreshInEditor(false, parentLayer._parentAnimClip._targetMeshGroup);
				Editor.RefreshControllerAndHierarchy(false);

				Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.Timelines | apEditor.REFRESH_TIMELINE_REQUEST.LinkKeyframeAndModifier, parentLayer);

				//중요 : LinkAndRefreshInEditor 이후에는 다음 함수들을 꼭 호출해야한다.
				//Debug.LogError("Add Keyframe + Refresh Link");
				//parentLayer._parentAnimClip._targetMeshGroup.LinkModMeshRenderUnits();
				//parentLayer._parentAnimClip._targetMeshGroup.RefreshModifierLink();
				//parentLayer._parentAnimClip._targetMeshGroup._modifierStack.RefreshAndSort(true);

				//Refresh 추가
				Editor.Select.RefreshAnimEditing(true);
			}

			//추가 : 19.11.23
			apEditorUtil.ReleaseGUIFocus();

			return newKeyframe;
		}




		public apAnimKeyframe AddCopiedAnimKeyframe(int targetFrameIndex, apAnimTimelineLayer parentLayer, bool isMakeCurrentBlendData, apAnimKeyframe srcKeyframe, bool isRefresh, bool isRecord)
		{
			if (parentLayer == null)
			{
				return null;
			}

			//Undo - 키프레임 복사
			if (isRecord)
			{
				apEditorUtil.SetRecord_PortraitMeshGroupModifier(apUndoGroupData.ACTION.Anim_DupKeyframe, Editor,
																	Editor._portrait,
																	parentLayer._parentAnimClip._targetMeshGroup,
																	parentLayer._parentTimeline._linkedModifier,
																	null, false);
			}

			int nextKeyframeID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimKeyFrame);
			if (nextKeyframeID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Keyframe Adding Failed", "Closed");
				EditorUtility.DisplayDialog(Editor.GetText(TEXT.AnimKeyframeAddFailed_Title),
												Editor.GetText(TEXT.AnimKeyframeAddFailed_Body_Error),
												Editor.GetText(TEXT.Close));
				return null;
			}

			bool isIntControlParamLayer = (parentLayer._linkType == apAnimClip.LINK_TYPE.ControlParam)
												&& (parentLayer._linkedControlParam != null && parentLayer._linkedControlParam._valueType == apControlParam.TYPE.Int);


			apAnimKeyframe newKeyframe = new apAnimKeyframe();
			newKeyframe.Init(nextKeyframeID, targetFrameIndex, isIntControlParamLayer);
			newKeyframe.Link(parentLayer);

			if (isMakeCurrentBlendData)
			{
				//Debug.LogError("TODO : Set Key -> isMakeCurrentBlendData");
			}


			parentLayer._keyframes.Add(newKeyframe);

			if (parentLayer._parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
			{
				//Modifier에 연동되는 타입이라면
				//전체적으로 돌면서 자동으로 Modifier와 연동을 해보자
				//Debug.Log("Start Sync To Animated Modifier");
				AddAndSyncAnimClipToModifier(parentLayer._parentTimeline._parentAnimClip);
			}

			//이전
			//Editor.RefreshTimelineLayers(false);

			//변경 19.5.21 : 복제된 키프레임의 타임라인 레이어만 갱신한다.
			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.Timelines | apEditor.REFRESH_TIMELINE_REQUEST.LinkKeyframeAndModifier, parentLayer);


			//값을 넣어서 복사하자
			if (srcKeyframe != null)
			{
				newKeyframe._curveKey = new apAnimCurve(srcKeyframe._curveKey, newKeyframe._frameIndex);
				newKeyframe._isKeyValueSet = srcKeyframe._isKeyValueSet;

				//newKeyframe._conSyncValue_Bool = srcKeyframe._conSyncValue_Bool;
				newKeyframe._conSyncValue_Int = srcKeyframe._conSyncValue_Int;
				newKeyframe._conSyncValue_Float = srcKeyframe._conSyncValue_Float;
				newKeyframe._conSyncValue_Vector2 = srcKeyframe._conSyncValue_Vector2;
				//newKeyframe._conSyncValue_Vector3 = srcKeyframe._conSyncValue_Vector3;
				//newKeyframe._conSyncValue_Color = srcKeyframe._conSyncValue_Color;


				if (newKeyframe._linkedModMesh_Editor != null && srcKeyframe._linkedModMesh_Editor != null)
				{
					//Mod Mesh 값을 복사하자
					List<apModifiedVertex> srcVertList = srcKeyframe._linkedModMesh_Editor._vertices;
					apMatrix srcTransformMatrix = srcKeyframe._linkedModMesh_Editor._transformMatrix;
					Color srcMeshColor = srcKeyframe._linkedModMesh_Editor._meshColor;
					bool isVisible = srcKeyframe._linkedModMesh_Editor._isVisible;

					newKeyframe._linkedModMesh_Editor._transformMatrix.SetMatrix(srcTransformMatrix);
					newKeyframe._linkedModMesh_Editor._meshColor = srcMeshColor;
					newKeyframe._linkedModMesh_Editor._isVisible = isVisible;

					apModifiedVertex srcModVert = null;
					apModifiedVertex dstModVert = null;
					for (int i = 0; i < srcVertList.Count; i++)
					{
						srcModVert = srcVertList[i];
						dstModVert = newKeyframe._linkedModMesh_Editor._vertices[i];

						if (dstModVert._vertexUniqueID != srcModVert._vertexUniqueID)
						{
							dstModVert = newKeyframe._linkedModMesh_Editor._vertices.Find(delegate (apModifiedVertex a)
							{
								return a._vertexUniqueID == srcModVert._vertexUniqueID;
							});
						}

						if (dstModVert != null)
						{
							dstModVert._deltaPos = srcModVert._deltaPos;
						}
					}
				}

				if (newKeyframe._linkedModBone_Editor != null && srcKeyframe._linkedModBone_Editor != null)
				{
					//ModBone도 복사하자
					if (newKeyframe._linkedModBone_Editor._transformMatrix == null)
					{
						newKeyframe._linkedModBone_Editor._transformMatrix = new apMatrix();
					}
					newKeyframe._linkedModBone_Editor._transformMatrix.SetMatrix(srcKeyframe._linkedModBone_Editor._transformMatrix);
				}

				//else
				//{
				//	//만약 Src만 있다면 체크해볼 필요가 있다. 연동이 안된 상태에서 복사를 시도했기 때문
				//	if (srcKeyframe._linkedModMesh_Editor != null)
				//	{
				//		Debug.LogError("Copy Keyframe Error : No Linked ModMesh");
				//	}
				//}
			}

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			//전체 Refresh를 해야한다.
			Editor._portrait.LinkAndRefreshInEditor(false, parentLayer._parentAnimClip._targetMeshGroup);
			if (isRefresh)
			{
				Editor.RefreshControllerAndHierarchy(false);
				Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.Timelines | apEditor.REFRESH_TIMELINE_REQUEST.LinkKeyframeAndModifier, parentLayer);

				//Refresh 추가
				Editor.Select.RefreshAnimEditing(true);
			}

			//추가 : 19.11.23
			apEditorUtil.ReleaseGUIFocus();

			return newKeyframe;
		}


		/// <summary>
		/// AnimClip의 모든 레이어에 대해 Keyframe을 일괄적으로 생성한다.
		/// </summary>
		public List<apAnimKeyframe> AddAnimKeyframeToAllLayer(int targetFrame, apAnimClip animClip, bool isMakeCurrentBlendData)
		{
			if (animClip == null)
			{
				return null;
			}
			if (animClip._targetMeshGroup == null)
			{
				return null;
			}

			apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Anim_AddKeyframe,
																	Editor,
																	Editor._portrait,
																	animClip._targetMeshGroup, null, false);


			List<apAnimKeyframe> resultKeyframes = new List<apAnimKeyframe>();

			apAnimTimeline timeline = null;
			apAnimTimelineLayer timelineLayer = null;
			for (int iTimeline = 0; iTimeline < animClip._timelines.Count; iTimeline++)
			{
				timeline = animClip._timelines[iTimeline];
				for (int iLayer = 0; iLayer < timeline._layers.Count; iLayer++)
				{
					//각 레이어마다 Keyframe을 추가해주자
					timelineLayer = timeline._layers[iLayer];

					apAnimKeyframe existFrame = timelineLayer.GetKeyframeByFrameIndex(targetFrame);
					if (existFrame != null)
					{
						//이미 해당 프레임에 값이 있다.
						//패스
						continue;
					}

					int nextKeyframeID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimKeyFrame);
					if (nextKeyframeID < 0)
					{
						//... ID가 안나온다. 패스
						continue;
					}

					bool isIntControlParamLayer = (timelineLayer._linkType == apAnimClip.LINK_TYPE.ControlParam)
												&& (timelineLayer._linkedControlParam != null && timelineLayer._linkedControlParam._valueType == apControlParam.TYPE.Int);

					apAnimKeyframe newKeyframe = new apAnimKeyframe();
					newKeyframe.Init(nextKeyframeID, targetFrame, isIntControlParamLayer);
					newKeyframe.Link(timelineLayer);




					timelineLayer._keyframes.Add(newKeyframe);

					if (timelineLayer._parentTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
					{
						//Modifier에 연동되는 타입이라면
						//전체적으로 돌면서 자동으로 Modifier와 연동을 해보자
						AddAndSyncAnimClipToModifier(timelineLayer._parentTimeline._parentAnimClip);
					}

					if (isMakeCurrentBlendData)
					{
						if (apEditor.IS_DEBUG_DETAILED)
						{
							Debug.LogError("TODO : Set Key -> isMakeCurrentBlendData 현재 중간 값을 이용해서 ModMesh 값을 세팅한다.");

						}
						MakeBlendModifiedDataAnimated(newKeyframe);
					}

					//Result 에 추가
					resultKeyframes.Add(newKeyframe);
				}
			}




			Editor._portrait.LinkAndRefreshInEditor(false, animClip._targetMeshGroup);
			Editor.RefreshControllerAndHierarchy(true);

			//Refresh 추가
			Editor.Select.RefreshAnimEditing(true);

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			//추가 : 19.11.23
			apEditorUtil.ReleaseGUIFocus();

			return resultKeyframes;
		}


		public void RemoveKeyframe(apAnimKeyframe animKeyframe, bool isSetRecordAndRefresh = true)
		{
			if (animKeyframe == null)
			{
				return;
			}

			apMeshGroup targetMeshGroup = null;
			targetMeshGroup = animKeyframe._parentTimelineLayer._parentAnimClip._targetMeshGroup;
			if (isSetRecordAndRefresh)
			{
				//Undo - Remove Keyframe
				apEditorUtil.SetRecord_PortraitMeshGroupModifier(apUndoGroupData.ACTION.Anim_RemoveKeyframe,
					Editor,
					Editor._portrait,
					targetMeshGroup,
					animKeyframe._parentTimelineLayer._parentTimeline._linkedModifier, animKeyframe, false);
			}

			//ID 반탑
			Editor._portrait.PushUnusedID(apIDManager.TARGET.AnimKeyFrame, animKeyframe._uniqueID);



			//선택중이면 제외
			if (Editor.Select.AnimKeyframe == animKeyframe || Editor.Select.AnimKeyframes.Contains(animKeyframe))
			{
				Editor.Select.SetAnimKeyframe(null, false, apGizmos.SELECT_TYPE.New);
			}

			//Timeline Layer에서 삭제
			apAnimTimelineLayer parentLayer = animKeyframe._parentTimelineLayer;
			if (parentLayer != null)
			{
				parentLayer._keyframes.Remove(animKeyframe);
			}


			if (isSetRecordAndRefresh)
			{
				//전체 Refresh를 해야한다.
				Editor._portrait.LinkAndRefreshInEditor(false, targetMeshGroup);
				Editor.RefreshControllerAndHierarchy(false);
				//Editor.RefreshTimelineLayers(false);//이전

				Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.Timelines | apEditor.REFRESH_TIMELINE_REQUEST.LinkKeyframeAndModifier, parentLayer);

				//Refresh 추가
				Editor.Select.RefreshAnimEditing(true);
			}

			//추가 : 19.11.23
			apEditorUtil.ReleaseGUIFocus();
		}

		public void RemoveKeyframes(List<apAnimKeyframe> animKeyframes, bool isSetRecordAndRefresh = true)
		{
			if (animKeyframes == null || animKeyframes.Count == 0)
			{
				return;
			}

			apMeshGroup targetMeshGroup = null;
			targetMeshGroup = animKeyframes[0]._parentTimelineLayer._parentAnimClip._targetMeshGroup;
			if (isSetRecordAndRefresh)
			{
				//Undo - Remove Keyframes : 여러개를 동시에 삭제하지만 Multiple은 아니고 리스트값을 넣어주자
				apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Anim_RemoveKeyframe,
																	Editor,
																	Editor._portrait,
																	targetMeshGroup,
																	null, false);
			}

			List<apAnimKeyframe> targetKeyframes = new List<apAnimKeyframe>();
			for (int i = 0; i < animKeyframes.Count; i++)
			{
				targetKeyframes.Add(animKeyframes[i]);
			}



			for (int i = 0; i < targetKeyframes.Count; i++)
			{
				apAnimKeyframe animKeyframe = targetKeyframes[i];

				//ID 반납
				Editor._portrait.PushUnusedID(apIDManager.TARGET.AnimKeyFrame, animKeyframe._uniqueID);

				//선택중이면 제외
				if (Editor.Select.AnimKeyframe == animKeyframe || Editor.Select.AnimKeyframes.Contains(animKeyframe))
				{
					Editor.Select.SetAnimKeyframe(null, false, apGizmos.SELECT_TYPE.New);
				}

				//Timeline Layer에서 삭제
				apAnimTimelineLayer parentLayer = animKeyframe._parentTimelineLayer;
				if (parentLayer != null)
				{
					parentLayer._keyframes.Remove(animKeyframe);
				}
			}

			if (isSetRecordAndRefresh)
			{
				//전체 Refresh를 해야한다.
				Editor._portrait.LinkAndRefreshInEditor(false, targetMeshGroup);
				Editor.RefreshControllerAndHierarchy(true);
				//Editor.RefreshTimelineLayers(false);

				//Refresh 추가
				Editor.Select.RefreshAnimEditing(true);
			}

			//추가 : 19.11.23
			apEditorUtil.ReleaseGUIFocus();
		}


		//AnimClip / Timeline / TimelineLayer / Keyframe과 Modifier 연동
		/// <summary>
		/// 이미 생성된 Timeline/Timelinelayer에 대해서 Modifier 내부의 ParamSetGroup/ParamSet 까지 만들어서
		/// ModMesh를 생성하게 한다. > 수정) 링크된 값에 따라 ModBone을 만든다.
		/// (중복을 체크하여 자동으로 만드므로 Refresh에 가깝다)
		/// </summary>
		/// <param name="animClip"></param>
		public void AddAndSyncAnimClipToModifier(apAnimClip animClip, bool isPrintLog = false)
		{
			apMeshGroup targetMeshGroup = animClip._targetMeshGroup;
			if (targetMeshGroup == null)
			{
				//if (apEditor.IS_DEBUG_DETAILED)
				//{
				//	Debug.LogError("AddAndSyncAnimClipToModifier Error : Target Mesh Group이 없다. [" + animClip._name + "]");
				//}
				return;
			}
			apModifierStack modStack = targetMeshGroup._modifierStack;
			List<apAnimTimeline> timelines = animClip._timelines;

			apAnimTimeline curTimeline = null;
			for (int iTimeline = 0; iTimeline < timelines.Count; iTimeline++)
			{
				curTimeline = timelines[iTimeline];

				//if(isPrintLog)
				//{
				//	Debug.Log("Check Timeline Link [" + curTimeline.DisplayName + "]");
				//}

				if (curTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
				{
					//Modifier 연동 타입일 때
					apModifierBase modifier = curTimeline._linkedModifier;//링크가 되어있어야 한다.
					if (!modStack._modifiers.Contains(modifier))
					{
						//Modifier가 없는데요?
						if (apEditor.IS_DEBUG_DETAILED)
						{
							Debug.LogError("Anim-Modifier Sync 문제 : Modifier를 찾을 수 없다. [Is Null : " + (modifier == null) + "]");
						}
						continue;
					}

					if (isPrintLog) { Debug.Log("Anim Modifier Check"); }

					apAnimTimelineLayer curLayer = null;
					for (int iLayer = 0; iLayer < curTimeline._layers.Count; iLayer++)
					{
						curLayer = curTimeline._layers[iLayer];

						//if (isPrintLog)
						//{
						//	Debug.Log("Layer > ParamSetGroup [Clip ID : " + animClip._uniqueID 
						//		+ " / Timeline ID : " + curTimeline._uniqueID 
						//		+ " / Layer ID : " + curLayer._uniqueID);
						//}

						List<apModifierParamSetGroup> paramSetGroupList = modifier._paramSetGroup_controller;
						apModifierParamSetGroup paramSetGroup = paramSetGroupList.Find(delegate (apModifierParamSetGroup a)
						{
							if (a._syncTarget == apModifierParamSetGroup.SYNC_TARGET.KeyFrame)
							{
								if (a._keyAnimClipID == animClip._uniqueID &&
									a._keyAnimTimelineID == curTimeline._uniqueID &&
									a._keyAnimTimelineLayerID == curLayer._uniqueID)
								{
									//ID가 모두 동일하다 (링크가 될 paramSetGroup이 이미 만들어졌다.
									return true;
								}
							}

							return false;
						});

						bool isAddParamSetGroup = false;
						if (paramSetGroup == null)
						{
							//if (isPrintLog)
							//{
							//	Debug.LogError("Not Param Set Group => Make ParamSetGroup");
							//}
							isAddParamSetGroup = true;
							//Debug.LogError("Add ParamSetGroup <- " + curLayer.DisplayName);
							//TODO : 여기서부터 작업을 하자
							paramSetGroup = new apModifierParamSetGroup(Editor._portrait, modifier, modifier.GetNextParamSetLayerIndex());
							paramSetGroup.SetTimeline(animClip, curTimeline, curLayer);
							paramSetGroup.LinkPortrait(Editor._portrait, modifier);

							modifier._paramSetGroup_controller.Add(paramSetGroup);
							//여기서는 ParamSet을 등록하진 않는다.
						}
						else
						{
							//if (isPrintLog)
							//{
							//	Debug.Log("Exist Param Set Group => Link ParamSetGroup");
							//}

							//Debug.Log("Link ParamSetGroup <- " + curLayer.DisplayName);
							//연동을 한번 더 해주자
							paramSetGroup._keyAnimClip = animClip;
							paramSetGroup._keyAnimTimeline = curTimeline;
							paramSetGroup._keyAnimTimelineLayer = curLayer;
							curLayer.LinkParamSetGroup(paramSetGroup);
						}

						//연동될 Tranform이 있는지 확인하자
						int linkedTransformID = -1;
						apTransform_Mesh linkedMeshTransform = null;
						apTransform_MeshGroup linkedMeshGroupTransform = null;
						apBone linkedBone = null;
						int linkedBoneID = -1;

						if (curLayer._linkedMeshTransform != null)
						{
							linkedMeshTransform = curLayer._linkedMeshTransform;
							linkedTransformID = linkedMeshTransform._transformUniqueID;
						}
						else if (curLayer._linkedMeshGroupTransform != null)
						{
							linkedMeshGroupTransform = curLayer._linkedMeshGroupTransform;
							linkedTransformID = linkedMeshGroupTransform._transformUniqueID;
						}
						else if (curLayer._linkedBone != null)
						{
							linkedBone = curLayer._linkedBone;
							linkedBoneID = linkedBone._uniqueID;
						}
						//TODO : Bone에 대해서도 처리해야한다.
						else
						{
							if (isPrintLog)
							{
								Debug.LogError("Link Transform Failed : Null in Layer");
							}
						}




						//Key를 추가해준다.
						List<apModifierParamSet> paramSetList = paramSetGroup._paramSetList;
						apAnimKeyframe curKeyframe = null;
						for (int iKeyframe = 0; iKeyframe < curLayer._keyframes.Count; iKeyframe++)
						{
							curKeyframe = curLayer._keyframes[iKeyframe];
							apModifierParamSet targetParamSet = paramSetList.Find(delegate (apModifierParamSet a)
							{
								return a.SyncKeyframe == curKeyframe;
							});

							if (targetParamSet == null)
							{
								//Debug.LogError("Add ParamSet <- Keyframe " + curKeyframe._frameIndex);
								//없다면 생성
								targetParamSet = new apModifierParamSet();
								targetParamSet.LinkParamSetGroup(paramSetGroup);
								targetParamSet.LinkSyncKeyframe(curKeyframe);

								paramSetList.Add(targetParamSet);
							}
							else
							{
								//Debug.Log("Link ParamSet <- Keyframe " + curKeyframe._frameIndex);
								//이미 있다면 서로 연결
								targetParamSet.LinkParamSetGroup(paramSetGroup);
								targetParamSet.LinkSyncKeyframe(curKeyframe);
							}


							if (linkedTransformID >= 0)
							{
								apModifiedMesh addedModMesh = null;
								//Modifier에 연동을 해주자
								if (linkedMeshTransform != null)
								{
									if (isAddParamSetGroup)
									{
										//Debug.Log("Add ModMesh > MeshTransform");
									}
									//ModeMesh는 추가하되, Refresh는 나중에 하자 (마지막 인자를 false로 둔다)
									addedModMesh = modifier.AddMeshTransform(targetMeshGroup, linkedMeshTransform, targetParamSet, true, true, false);
								}
								else if (linkedMeshGroupTransform != null)
								{
									if (isAddParamSetGroup)
									{
										//Debug.Log("Add ModMesh > MeshGroupTransform");
									}
									//ModeMesh는 추가하되, Refresh는 나중에 하자 (마지막 인자를 false로 둔다)
									addedModMesh = modifier.AddMeshGroupTransform(targetMeshGroup, linkedMeshGroupTransform, targetParamSet, true, true, false);
								}
								if (addedModMesh == null)
								{
									//Debug.LogError("Add Mod Mesh Failed");
									curKeyframe.LinkModMesh_Editor(targetParamSet, null);
								}
								else
								{
									curKeyframe.LinkModMesh_Editor(targetParamSet, addedModMesh);
								}
							}
							else if (linkedBoneID > 0)
							{
								//Bone 추가
								apModifiedBone addModBone = modifier.AddBone(linkedBone, targetParamSet, true, false);
								if (addModBone == null)
								{
									curKeyframe.LinkModBone_Editor(targetParamSet, null);
								}
								else
								{
									curKeyframe.LinkModBone_Editor(targetParamSet, addModBone);
								}
							}

						}


						paramSetGroup.RefreshSync();
					}

				}
			}
			//ModMesh 링크를 여기서 일괄적으로 처리
			targetMeshGroup.RefreshModifierLink();

			//추가 : ExMode에 추가한다.
			Editor.Select.RefreshMeshGroupExEditingFlags(
							targetMeshGroup,
							null,
							null,
							animClip,
							true);
		}



		// ModMesh/ModBone을 만들때, 기본 값이 아니라 두개의 키프레임 도는 몇개의 ParamSet으로부터 보간하여 값을 만들때의 함수.
		//여기서는 일단 애니메이션 키프레임의 중간값을 가져오는 것만 생각한다.
		public void MakeBlendModifiedDataAnimated(apAnimKeyframe targetKeyframe)
		{
			apAnimTimelineLayer timelineLayer = targetKeyframe._parentTimelineLayer;
			apAnimTimeline timeline = timelineLayer._parentTimeline;
			apAnimClip animClip = timeline._parentAnimClip;
			//apMeshGroup targetMeshGroup = animClip._targetMeshGroup;

			int targetFrameIndex = targetKeyframe._frameIndex;

			//앞쪽 Keyframe과 뒤쪽 Keyframe을 가져온다.
			//1) 없는 경우 -> 리턴
			//2) 하나만 있는 경우 -> 그 키프레임의 값을 복사한다.
			//3) 두개가 있는 경우 -> Curve 보간을 이용하여 중간 값을 가져온다.

			//두개의 값을 가져올 때 -> Loop 타입 여부에 따라 키를 가져와야 한다.
			apAnimKeyframe prevKeyframe = null;
			apAnimKeyframe nextKeyframe = null;

			apAnimKeyframe minKeyframe = null;
			apAnimKeyframe maxKeyframe = null;

			bool isLoop = animClip.IsLoop;

			List<apAnimKeyframe> keyframes = timelineLayer._keyframes;

			//1. 일단 Loop 없이 키값을 가져온다.

			apAnimKeyframe curKeyframe = null;
			for (int i = 0; i < keyframes.Count; i++)
			{
				curKeyframe = keyframes[i];
				if (curKeyframe == targetKeyframe)
				{
					continue;
				}

				if (minKeyframe == null || curKeyframe._frameIndex < minKeyframe._frameIndex)
				{
					//가장 앞쪽의 프레임을 찾자
					minKeyframe = curKeyframe;
				}

				if (maxKeyframe == null || curKeyframe._frameIndex > maxKeyframe._frameIndex)
				{
					//가장 뒤쪽의 프레임을 찾자
					maxKeyframe = curKeyframe;
				}

				if (curKeyframe._frameIndex < targetFrameIndex)
				{
					//이전 프레임일때
					if (prevKeyframe == null || curKeyframe._frameIndex > prevKeyframe._frameIndex)
					{
						//이전 프레임 중 [최대 프레임]을 찾는다.
						prevKeyframe = curKeyframe;
					}
				}
				else if (curKeyframe._frameIndex > targetFrameIndex)
				{
					//다음 프레임일때
					if (nextKeyframe == null || curKeyframe._frameIndex < nextKeyframe._frameIndex)
					{
						//다음 프레임 중 [최소 프레임]을 찾는다.
						nextKeyframe = curKeyframe;
					}
				}
			}

			if (prevKeyframe == null && nextKeyframe == null)
			{
				//1) 둘다 없을 때 -> 리턴
				//Debug.LogError("Blend : No Keyframes");
				return;
			}

			//하나만 있다면 + Loop일때 반대편 Keyframe을 찾자
			if (prevKeyframe == null || nextKeyframe == null)
			{
				if (isLoop)
				{
					if (prevKeyframe == null)
					{
						//Prev가 Null이라면
						// (Loop Max) .... [Target] - [Next]
						//Max Keyframe을 검토하여 Prev로 넣어주자
						if (maxKeyframe != null && nextKeyframe != maxKeyframe)
						{
							prevKeyframe = maxKeyframe;
						}
					}
					else if (nextKeyframe == null)
					{
						//Next가 Null이라면
						// [Prev] - [Target] ...... (Loop Min)
						//Min Keyframe을 검토하여 Next로 넣어주자
						if (minKeyframe != null && prevKeyframe != minKeyframe)
						{
							nextKeyframe = minKeyframe;
						}
					}
				}
			}

			//3) 둘다 있을때를 먼저 검토한다.
			if (prevKeyframe != null && nextKeyframe != null)
			{
				//Debug.Log("Blend : 2 Keyframes");
				int prevFrameIndex = prevKeyframe._frameIndex;
				int nextFrameIndex = nextKeyframe._frameIndex;

				if (prevFrameIndex > targetFrameIndex && isLoop)
				{
					prevFrameIndex = prevKeyframe._loopFrameIndex;
				}

				if (nextFrameIndex < targetFrameIndex && isLoop)
				{
					nextFrameIndex = nextKeyframe._loopFrameIndex;
				}

				float itp = apAnimCurveResult.CalculateInterpolation_Float(
										(float)targetFrameIndex, targetFrameIndex,
										prevFrameIndex, nextFrameIndex,
										prevKeyframe._curveKey,
										nextKeyframe._curveKey);

				//itp = 0 : A값을 사용
				//itp = 1 : B값을 사용

				//1. 키프레임의 값을 보간하여 적용. CurveKey 포함
				targetKeyframe._curveKey = new apAnimCurve(prevKeyframe._curveKey, nextKeyframe._curveKey, targetFrameIndex);
				targetKeyframe._isKeyValueSet = prevKeyframe._isKeyValueSet;

				//Controller Param 값을 입력
				targetKeyframe._conSyncValue_Int = (int)((((float)(prevKeyframe._conSyncValue_Int) * (1 - itp)) + ((float)(nextKeyframe._conSyncValue_Int) * (itp))) + 0.5f);
				targetKeyframe._conSyncValue_Float = (prevKeyframe._conSyncValue_Float * (1 - itp)) + (nextKeyframe._conSyncValue_Float * itp);
				targetKeyframe._conSyncValue_Vector2 = (prevKeyframe._conSyncValue_Vector2 * (1 - itp)) + (nextKeyframe._conSyncValue_Vector2 * itp);

				//2. Linked ModMesh를 수정
				if (targetKeyframe._linkedModMesh_Editor != null &&
					prevKeyframe._linkedModMesh_Editor != null &&
					nextKeyframe._linkedModMesh_Editor != null)
				{
					List<apModifiedVertex> prevVertList = prevKeyframe._linkedModMesh_Editor._vertices;
					apMatrix prevTransformMatrix = prevKeyframe._linkedModMesh_Editor._transformMatrix;
					Color prevMeshColor = prevKeyframe._linkedModMesh_Editor._meshColor;
					bool prevIsVisible = prevKeyframe._linkedModMesh_Editor._isVisible;
					if (!prevIsVisible)
					{
						prevMeshColor.a = 0.0f;
					}

					List<apModifiedVertex> nextVertList = nextKeyframe._linkedModMesh_Editor._vertices;
					apMatrix nextTransformMatrix = nextKeyframe._linkedModMesh_Editor._transformMatrix;
					Color nextMeshColor = nextKeyframe._linkedModMesh_Editor._meshColor;
					bool nextIsVisible = nextKeyframe._linkedModMesh_Editor._isVisible;
					if (!nextIsVisible)
					{
						nextMeshColor.a = 0.0f;
					}

					targetKeyframe._linkedModMesh_Editor._transformMatrix.SetZero();
					targetKeyframe._linkedModMesh_Editor._transformMatrix._pos = (prevTransformMatrix._pos * (1 - itp)) + (nextTransformMatrix._pos * itp);
					targetKeyframe._linkedModMesh_Editor._transformMatrix._angleDeg = (prevTransformMatrix._angleDeg * (1 - itp)) + (nextTransformMatrix._angleDeg * itp);
					targetKeyframe._linkedModMesh_Editor._transformMatrix._scale = (prevTransformMatrix._scale * (1 - itp)) + (nextTransformMatrix._scale * itp);
					targetKeyframe._linkedModMesh_Editor._transformMatrix.MakeMatrix();


					targetKeyframe._linkedModMesh_Editor._meshColor = (prevMeshColor * (1 - itp)) + (nextMeshColor * itp);
					targetKeyframe._linkedModMesh_Editor._isVisible = prevIsVisible | nextIsVisible;//<<하나라도 true이면 오케이

					apModifiedVertex prevModVert = null;
					apModifiedVertex nextModVert = null;
					apModifiedVertex dstModVert = null;


					for (int i = 0; i < prevVertList.Count; i++)
					{
						prevModVert = prevVertList[i];
						//같은 인덱스를 먼저 찾고, 다르면 일일이 Find
						nextModVert = nextVertList[i];

						if (nextModVert._vertexUniqueID != prevModVert._vertexUniqueID)
						{
							nextModVert = nextVertList.Find(delegate (apModifiedVertex a)
							{
								return a._vertexUniqueID == prevModVert._vertexUniqueID;
							});
						}

						dstModVert = targetKeyframe._linkedModMesh_Editor._vertices[i];

						if (dstModVert._vertexUniqueID != prevModVert._vertexUniqueID)
						{
							dstModVert = targetKeyframe._linkedModMesh_Editor._vertices.Find(delegate (apModifiedVertex a)
							{
								return a._vertexUniqueID == prevModVert._vertexUniqueID;
							});
						}

						//Vertex 보간
						if (dstModVert != null && nextModVert != null)
						{
							dstModVert._deltaPos = (prevModVert._deltaPos * (1 - itp)) + (nextModVert._deltaPos * itp);
						}
					}

				}

				//3. Linked ModBone을 수정
				if (targetKeyframe._linkedModBone_Editor != null &&
					prevKeyframe._linkedModBone_Editor != null &&
					nextKeyframe._linkedModBone_Editor != null)
				{
					apMatrix prevTransformMatrix = prevKeyframe._linkedModBone_Editor._transformMatrix;
					apMatrix nextTransformMatrix = nextKeyframe._linkedModBone_Editor._transformMatrix;

					targetKeyframe._linkedModBone_Editor._transformMatrix.SetZero();
					targetKeyframe._linkedModBone_Editor._transformMatrix._pos = (prevTransformMatrix._pos * (1 - itp)) + (nextTransformMatrix._pos * itp);
					targetKeyframe._linkedModBone_Editor._transformMatrix._angleDeg = (prevTransformMatrix._angleDeg * (1 - itp)) + (nextTransformMatrix._angleDeg * itp);
					targetKeyframe._linkedModBone_Editor._transformMatrix._scale = (prevTransformMatrix._scale * (1 - itp)) + (nextTransformMatrix._scale * itp);
					targetKeyframe._linkedModBone_Editor._transformMatrix.MakeMatrix();
				}


			}
			else if (prevKeyframe != null || nextKeyframe != null)
			{
				//2) 한개만 유효한 경우 그냥 복사
				apAnimKeyframe srcKeyframe = null;
				if (prevKeyframe != null)
				{
					srcKeyframe = prevKeyframe;
				}
				else
				{
					srcKeyframe = nextKeyframe;
				}

				//Debug.Log("Blend : 1 Keyframe");


				targetKeyframe._curveKey = new apAnimCurve(srcKeyframe._curveKey, targetFrameIndex);
				targetKeyframe._isKeyValueSet = srcKeyframe._isKeyValueSet;

				targetKeyframe._conSyncValue_Int = srcKeyframe._conSyncValue_Int;
				targetKeyframe._conSyncValue_Float = srcKeyframe._conSyncValue_Float;
				targetKeyframe._conSyncValue_Vector2 = srcKeyframe._conSyncValue_Vector2;


				if (targetKeyframe._linkedModMesh_Editor != null && srcKeyframe._linkedModMesh_Editor != null)
				{
					//Mod Mesh 값을 복사하자
					List<apModifiedVertex> srcVertList = srcKeyframe._linkedModMesh_Editor._vertices;
					apMatrix srcTransformMatrix = srcKeyframe._linkedModMesh_Editor._transformMatrix;
					Color srcMeshColor = srcKeyframe._linkedModMesh_Editor._meshColor;
					bool isVisible = srcKeyframe._linkedModMesh_Editor._isVisible;

					targetKeyframe._linkedModMesh_Editor._transformMatrix.SetMatrix(srcTransformMatrix);
					targetKeyframe._linkedModMesh_Editor._meshColor = srcMeshColor;
					targetKeyframe._linkedModMesh_Editor._isVisible = isVisible;

					apModifiedVertex srcModVert = null;
					apModifiedVertex dstModVert = null;
					for (int i = 0; i < srcVertList.Count; i++)
					{
						srcModVert = srcVertList[i];
						dstModVert = targetKeyframe._linkedModMesh_Editor._vertices[i];

						if (dstModVert._vertexUniqueID != srcModVert._vertexUniqueID)
						{
							dstModVert = targetKeyframe._linkedModMesh_Editor._vertices.Find(delegate (apModifiedVertex a)
							{
								return a._vertexUniqueID == srcModVert._vertexUniqueID;
							});
						}

						if (dstModVert != null)
						{
							dstModVert._deltaPos = srcModVert._deltaPos;
						}
					}
				}

				if (targetKeyframe._linkedModBone_Editor != null && srcKeyframe._linkedModBone_Editor != null)
				{
					//ModBone도 복사하자
					if (targetKeyframe._linkedModBone_Editor._transformMatrix == null)
					{
						targetKeyframe._linkedModBone_Editor._transformMatrix = new apMatrix();
					}
					targetKeyframe._linkedModBone_Editor._transformMatrix.SetMatrix(srcKeyframe._linkedModBone_Editor._transformMatrix);
				}
			}
			else
			{
				//엥 둘다 없네요 이 무슨..
				//Debug.LogError("Blend : No Keyframes");
				return;
			}

		}







		public void CopyAnimCurveToAllKeyframes(apAnimCurveResult srcCurveResult, apAnimTimelineLayer animLayer, apAnimClip animClip)
		{
			if (srcCurveResult == null || animLayer == null || animClip == null)
			{
				Debug.LogError("CopyAnimCurveToAllKeyframes Failed : Case 1");
				return;
			}
			if (srcCurveResult._curveKeyA == null || srcCurveResult._curveKeyB == null)
			{
				Debug.LogError("CopyAnimCurveToAllKeyframes Failed : Case 2");
				return;
			}

			//변경 3.30 : 적용 범위 : 현재 타임라인 레이어만 / 모든 타임라인 레이어 / 취소
			int iBtn = EditorUtility.DisplayDialogComplex(Editor.GetText(TEXT.DLG_CopyAnimCurveToAllKey_Title),
													Editor.GetText(TEXT.DLG_CopyAnimCurveToAllKey_Body),
													Editor.GetText(TEXT.DLG_CopyAnimCurveToAllKey_SelectedLayer),
													Editor.GetText(TEXT.DLG_CopyAnimCurveToAllKey_AllLayer),
													Editor.GetText(TEXT.Cancel));

			if (iBtn == 2)
			{
				//취소했다.
				return;
			}
			bool isCopyToAllLayers = (iBtn == 1);

			//이건 Modified를 수정하지 않으므로 Portrait만 수정하자
			apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor, Editor._portrait, null, false);

			if (isCopyToAllLayers)
			{
				//모든 레이어의 키프레임들에 복사 << 추가 3.31
				apAnimTimeline curTimeline = null;
				apAnimTimelineLayer curLayer = null;
				apAnimKeyframe keyframe = null;
				for (int iTimeline = 0; iTimeline < animClip._timelines.Count; iTimeline++)
				{
					curTimeline = animClip._timelines[iTimeline];
					for (int iLayer = 0; iLayer < curTimeline._layers.Count; iLayer++)
					{
						curLayer = curTimeline._layers[iLayer];

						for (int iKey = 0; iKey < curLayer._keyframes.Count; iKey++)
						{
							keyframe = curLayer._keyframes[iKey];

							if (keyframe._curveKey._prevCurveResult != srcCurveResult)
							{
								keyframe._curveKey._prevCurveResult.CopyCurve(srcCurveResult);
							}


							if (keyframe._curveKey._nextCurveResult != srcCurveResult)
							{
								keyframe._curveKey._nextCurveResult.CopyCurve(srcCurveResult);
							}
						}
					}
				}

			}
			else
			{
				//현재 레이어의 키프레임들에만 복사
				for (int i = 0; i < animLayer._keyframes.Count; i++)
				{
					apAnimKeyframe keyframe = animLayer._keyframes[i];
					if (keyframe._curveKey._prevCurveResult != srcCurveResult)
					{
						keyframe._curveKey._prevCurveResult.CopyCurve(srcCurveResult);
					}


					if (keyframe._curveKey._nextCurveResult != srcCurveResult)
					{
						keyframe._curveKey._nextCurveResult.CopyCurve(srcCurveResult);
					}
				}
			}



		}


		//추가 3.30 : 키프레임을 SnapShot에서 꺼내서 복사한다.
		public void CopyAnimKeyframeFromSnapShot(apAnimClip animClip, int frameIndex)
		{
			if (!apSnapShotManager.I.IsKeyframesPastableOnTimelineUI(animClip))
			{
				//추가 19.6.27 : 다른 AnimClip에 복사를 할 수 있다.
				//만약 AnimClip만 다른 것이라면?
				if(apSnapShotManager.I.IsKeyframesPastableOnTimelineUI_ToOtherAnimClip(animClip))
				{
					//다른 애니메이션 클립으로 키프레임을 복사하자.
					CopyAnimKeyframeFromSnapShotToOtherAnimClip(animClip, frameIndex);
				}
				return;
			}

			List<apSnapShotStackUnit> snapshotData = apSnapShotManager.I.GetKeyframesOnTimelineUI();
			int snapshotFrame = apSnapShotManager.I.StartFrameOfKeyframesOnTimelineUI;

			//얼마나 이동되어 복붙해야하는가
			int deltaFrame = frameIndex - snapshotFrame;

			//Record
			apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Anim_CopyKeyframe, Editor,
																	Editor._portrait,
																	animClip._targetMeshGroup, animClip, false);

			apSnapShot_Keyframe curSnapshot = null;
			for (int iData = 0; iData < snapshotData.Count; iData++)
			{
				curSnapshot = snapshotData[iData]._snapShot as apSnapShot_Keyframe;
				if (curSnapshot == null || curSnapshot.KeyTimelineLayer == null)
				{
					continue;
				}

				//1. 목표 프레임을 결정한다.
				int dstFrameIndex = curSnapshot.SavedFrameIndex + deltaFrame;


				//2. 키프레임을 생성한다. 
				//버그 수정 19.6.27 : 기존의 키프레임이 있다면 그걸 이용, 그렇지 않다면 삭제하는 것으로..
				apAnimKeyframe targetKeyframe = curSnapshot.KeyTimelineLayer.GetKeyframeByFrameIndex(dstFrameIndex);
				if(targetKeyframe == null)
				{
					targetKeyframe = AddAnimKeyframe(dstFrameIndex, curSnapshot.KeyTimelineLayer, false, false, false, false);
				}
				//else
				//{
				//	Debug.Log("기존의 키프레임 [" + dstFrameIndex + "]에 복사했다");
				//}
				

				if (targetKeyframe != null)
				{
					//데이터를 복사한다.
					curSnapshot.Load(targetKeyframe);
				}


			}

			//Refresh
			Editor._portrait.LinkAndRefreshInEditor(false, animClip._targetMeshGroup);
			Editor.RefreshControllerAndHierarchy(true);
			Editor.Select.RefreshAnimEditing(true);

		}


		public void CopyAnimKeyframeFromSnapShotToOtherAnimClip(apAnimClip targetAnimClip, int frameIndex)
		{
			List<apSnapShotStackUnit> snapshotData = apSnapShotManager.I.GetKeyframesOnTimelineUI();
			int snapshotFrame = apSnapShotManager.I.StartFrameOfKeyframesOnTimelineUI;

			//두가지를 물어봐야한다.
			//1. (프레임이 다른 경우)
			//2. (타임라인이나 타임라인 레이어가 없는 경우)

			bool isCheck_NoTimelineLayer = false;//질문을 했는지 확인

			bool isCopyCreatingNewTimelineLayer = false;


			//얼마나 이동되어 복붙해야하는가
			int deltaFrame = frameIndex - snapshotFrame;

			if(deltaFrame != 0)
			{
				//1. (프레임이 다른 경우) "저장된 프레임과 복사하고자 하는 프레임이 같지 않습니다" > (1) 현재 프레임에 복사하기 (2) 저장된 프레임에 복사하기 (3) 취소
				//"Select a frame to copy"
				//"The current frame and the saved frame are not the same.\nWhere do you want to copy?\nCurrent : " + frameIndex + " / Saved : " + snapshotFrame
				//"Current Frame"
				//"Saved Frame"
				int resultBtn = EditorUtility.DisplayDialogComplex(	Editor.GetText(TEXT.DLG_CopyKeyframesToOtherClipAndPos_Title),
																	Editor.GetTextFormat(TEXT.DLG_CopyKeyframesToOtherClipAndPos_Body, frameIndex, snapshotFrame), 
																	Editor.GetText(TEXT.DLG_CopyKeyframesToOtherClipAndPos_Current), 
																	Editor.GetText(TEXT.DLG_CopyKeyframesToOtherClipAndPos_Saved), 
																	Editor.GetText(TEXT.Cancel));

				if(resultBtn == 0)
				{
					//현재 프레임에 저장한다. -> Delta Frame 유지하기
				}
				else if(resultBtn == 1)
				{
					//저장된 프레임에 저장한다. (deltaFrame을 0으로 고정)
					deltaFrame = 0;
				}
				else
				{
					//복사 취소
					return;
				}
			}


			bool isAnyAdded = false;

			//Record
			apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Anim_CopyKeyframe, Editor,
																	Editor._portrait,
																	targetAnimClip._targetMeshGroup, targetAnimClip, false);

			apSnapShot_Keyframe curSnapshot = null;
			for (int iData = 0; iData < snapshotData.Count; iData++)
			{
				curSnapshot = snapshotData[iData]._snapShot as apSnapShot_Keyframe;
				if (curSnapshot == null || curSnapshot.KeyTimelineLayer == null
					)
				{
					continue;
				}

				//1. 목표 프레임을 결정한다.
				int dstFrameIndex = curSnapshot.SavedFrameIndex + deltaFrame;

				//2. <중요> 타겟이 될 TimelineLayer를 찾거나 연결하자.
				//서로 다른 AnimClip이므로, 최대한 유사한 TimelineLayer를 연결해야한다.
				apAnimTimelineLayer srcTimelineLayer = curSnapshot.KeyTimelineLayer;
				apAnimTimeline srcTimeline = srcTimelineLayer._parentTimeline;

				apAnimTimeline targetTimeline = null;
				apAnimTimelineLayer targetTimelineLayer = null;

				//복사 가능한 Src인지 확인한다.
				if(srcTimeline == null)
				{
					continue;
				}
				if(srcTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
				{
					if(srcTimelineLayer._linkedControlParam == null)
					{
						//유효한 ControlParam이 없다.
						continue;
					}
				}
				else
				{
					if(srcTimeline._linkedModifier == null)
					{
						//연결된 유효한 Modifier가 없다.
						continue;
					}

					if(srcTimelineLayer._linkedMeshTransform == null
						&& srcTimelineLayer._linkedMeshGroupTransform == null
						&& srcTimelineLayer._linkedBone == null)
					{
						//연결된 대상이 아무것도 없다.
						continue;
					}
				}

				//적절한 TimelineLayer를 찾자
				switch (srcTimeline._linkType)
				{
					//1) Timeline이 Control Param 타입일 때
					case apAnimClip.LINK_TYPE.ControlParam:
						{
							apControlParam linkedControlParam = srcTimelineLayer._linkedControlParam;

							targetTimeline = targetAnimClip._timelines.Find(delegate (apAnimTimeline a)
							{
								return a._linkType == apAnimClip.LINK_TYPE.ControlParam;
							});

							if(targetTimeline != null)
							{
								targetTimelineLayer = targetTimeline._layers.Find(delegate(apAnimTimelineLayer a)
								{
									return a._linkedControlParam == linkedControlParam;
								});
							}
						}
						break;

						//2) Timeline이 Modifier에 연결되는 타입일 때
					case apAnimClip.LINK_TYPE.AnimatedModifier:
						{
							apModifierBase lnkedModifier = srcTimeline._linkedModifier;

							targetTimeline = targetAnimClip._timelines.Find(delegate (apAnimTimeline a)
							{
								return a._linkType == apAnimClip.LINK_TYPE.AnimatedModifier
										&& a._linkedModifier == lnkedModifier;
							});

							if(targetTimeline != null)
							{
								if (srcTimelineLayer._linkedMeshTransform != null)
								{
									//MeshTransform과 연결된 경우
									targetTimelineLayer = targetTimeline._layers.Find(delegate (apAnimTimelineLayer a)
									{
										return a._linkedMeshTransform == srcTimelineLayer._linkedMeshTransform;
									});
								}

								if (srcTimelineLayer._linkedMeshGroupTransform != null)
								{
									//MeshGroupTransform과 연결된 경우
									targetTimelineLayer = targetTimeline._layers.Find(delegate (apAnimTimelineLayer a)
									{
										return a._linkedMeshGroupTransform == srcTimelineLayer._linkedMeshGroupTransform;
									});
								}

								if (srcTimelineLayer._linkedBone != null)
								{
									//Bone과 연결된 경우
									targetTimelineLayer = targetTimeline._layers.Find(delegate (apAnimTimelineLayer a)
									{
										return a._linkedBone == srcTimelineLayer._linkedBone;
									});
								}
							}
						}
						break;
				}

				//적절한TimelineLayer를 찾지 못한 경우
				if(targetTimelineLayer == null)
				{
					if(!isCheck_NoTimelineLayer)
					{
						//질문을 해봅시더
						//2. (타임라인이나 타임라인 레이어가 없는 경우) "키프레임이 복사될 적절한 타임라인 레이어가 없습니다. 타임라인 레이어를 새로 생성할까요?" > (1) 생성하고 복사하기 (2) 무시하기
						//"No Timeline Layer"
						//"There is no appropriate Timeline Layer to copy the keyframes. Create a new Timeline Layer?"
						//"Ignore"
						bool result = EditorUtility.DisplayDialog(	Editor.GetText(TEXT.DLG_NoTimelineLayerCopingKeyframes_Title), 
																	Editor.GetText(TEXT.DLG_NoTimelineLayerCopingKeyframes_Body),
																	Editor.GetText(TEXT.Okay),
																	Editor.GetText(TEXT.Ignore));

						//일단 질문은 끝
						isCheck_NoTimelineLayer = true;

						if (result)
						{
							//Timeline과 Timeline Layer를 생성하면서 복사한다.
							isCopyCreatingNewTimelineLayer = true;
						}
						else
						{
							//존재하지 않는 Timeline Layer는 무시한다.
							isCopyCreatingNewTimelineLayer = false;
						}
					}

					//"무시하기"를 선택한 경우
					if(!isCopyCreatingNewTimelineLayer)
					{
						//없으면 Skip
						continue;
					}

					//타임라인 레이어를 새로 만들자

					if(targetTimeline == null)
					{
						//먼저, Timeline이 없다면 SrcTimeline의 정보를 이용해서 생성하자.
						targetTimeline = AddAnimTimeline(srcTimeline._linkType, srcTimeline._modifierUniqueID, targetAnimClip, false, false);
					}

					if(targetTimeline == null)
					{
						Debug.LogError("AnyPortrait : Creating a new Timeline while copying keyframes failed.");
						continue;
					}

					object targetObject = null;
					if (targetTimeline._linkType == apAnimClip.LINK_TYPE.AnimatedModifier)
					{
						if (srcTimelineLayer._linkedMeshTransform != null)				{ targetObject = srcTimelineLayer._linkedMeshTransform; }
						else if (srcTimelineLayer._linkedMeshGroupTransform != null)	{ targetObject = srcTimelineLayer._linkedMeshGroupTransform; }
						else if (srcTimelineLayer._linkedBone != null)					{ targetObject = srcTimelineLayer._linkedBone; }
					}
					else
					{
						if (srcTimelineLayer._linkedControlParam != null)				{ targetObject = srcTimelineLayer._linkedControlParam; }
					}
						
					targetTimelineLayer = AddAnimTimelineLayer(targetObject, targetTimeline, false);

					if(targetTimelineLayer == null)
					{
						Debug.LogError("AnyPortrait : Creating a new Timeline Layer while copying keyframes failed.");
						continue;
					}

					//타임라인 레이어가 추가될때마다 리프레시 
					isAnyAdded = true;
					Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, targetTimelineLayer);

					Editor._portrait.LinkAndRefreshInEditor(false, targetAnimClip._targetMeshGroup);

					if (targetTimeline._linkedModifier != null)
					{
						targetTimeline._linkedModifier.RefreshParamSet();
					}
				}

				//여기까지 하면 Timeline과 Timeline Layer 생성 or 검색이 모두 완료되었다.


				//2. 키프레임을 생성 또는 참조한다. 
				apAnimKeyframe targetKeyframe = targetTimelineLayer.GetKeyframeByFrameIndex(dstFrameIndex);
				if(targetKeyframe == null)
				{
					targetKeyframe = AddAnimKeyframe(dstFrameIndex, targetTimelineLayer, false, false, false, false);
					isAnyAdded = true;
				}


				if (targetKeyframe != null)
				{
					//데이터를 복사한다.
					curSnapshot.Load(targetKeyframe);
				}
				

			}

			//Refresh
			Editor._portrait.LinkAndRefreshInEditor(true, targetAnimClip._targetMeshGroup);//Link Reset도 해야한다.
			Editor.RefreshControllerAndHierarchy(true);
			Editor.Select.RefreshAnimEditing(true);

			if(isAnyAdded)
			{
				Editor.OnAnyObjectAddedOrRemoved();
			}
		}


		//추가 19.8.20 : 애니메이션의 메시 그룹을 변경할 때, 데이터를 이전할 수 있다.
		private int _requestedModType = 0;
		private int _requestedModValidationKey = -1;
		public void MigrateAnimClipToMeshGroup(apAnimClip animClip, apMeshGroup nextMeshGroup)
		{
			if(Editor == null || Editor._portrait == null || animClip == null)
			{
				return;
			}

			//Undo
			apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Anim_SetMeshGroup, Editor, Editor._portrait, nextMeshGroup, null, false);

			//타임라인 데이터를 유지한 상태에서 소유권을 바꾼다.
			apMeshGroup prevMeshGroup = animClip._targetMeshGroup;
			if(prevMeshGroup == nextMeshGroup || prevMeshGroup == null || nextMeshGroup == null)
			{
				return;
			}

			//매핑할 데이터
			Dictionary<apModifierBase, apModifierBase> mapping_Modifiers = new Dictionary<apModifierBase, apModifierBase>();
			
			//일단 모디파이어가 있는지 확인 > 없다면 추가하자.
			//어떤 모디파이어가 필요한가 체크
			List<apAnimTimeline> timelines = animClip._timelines;
			apAnimTimeline curTimeline = null;
			apAnimTimelineLayer curTimelineLayer = null;
			apAnimKeyframe curKeyframe = null;

			Dictionary<apAnimKeyframe, apModifiedMesh> keyframe2PrevModMesh = new Dictionary<apAnimKeyframe, apModifiedMesh>();
			Dictionary<apAnimKeyframe, apModifiedBone> keyframe2PrevModBone = new Dictionary<apAnimKeyframe, apModifiedBone>();
			
			for (int i = 0; i < timelines.Count; i++)
			{
				curTimeline = null;
				curTimelineLayer = null;
				curKeyframe = null;

				curTimeline = timelines[i];
				
				if(curTimeline._linkType != apAnimClip.LINK_TYPE.AnimatedModifier)
				{
					continue;
				}
				
				apModifierBase prevModifier = curTimeline._linkedModifier;
				if(prevModifier == null)
				{
					continue;
				}

				//같은 타입의 모디파이어가 있는지 확인
				apModifierBase nextModifier = nextMeshGroup._modifierStack._modifiers.Find(delegate(apModifierBase a)
				{
					return a.ModifierType == prevModifier.ModifierType;
				});

				if (nextModifier == null)
				{
					//없다면 만들자
					_requestedModType = (int)prevModifier.ModifierType;
					_requestedModValidationKey = -1;
					apVersion.I.RequestAddableModifierTypes(OnModifierValidationKeyCheck);
					//모디파이어를 추가하기 위한 Key가 적용되었을 것

					nextModifier = AddModifierToMeshGroup(nextMeshGroup, prevModifier.ModifierType, _requestedModValidationKey, false);
					_requestedModType = -1;
					_requestedModValidationKey = -1;
				}
				
				//Mod > Mod 매핑
				if (nextModifier != null)
				{
					if (!mapping_Modifiers.ContainsKey(prevModifier))
					{
						mapping_Modifiers.Add(prevModifier, nextModifier);
					}
				}
				else
				{
					continue;
				}

				//기본 설정 몇가지를 복사하자. (단, False > True 방향으로만)
				if(prevModifier._isColorPropertyEnabled)
				{
					nextModifier._isColorPropertyEnabled = true;
				}
				
				if(prevModifier._isExtraPropertyEnabled)
				{
					nextModifier._isExtraPropertyEnabled = true;
				}
				

				//ParamSetGroup 변경부터 중요 >> AddTimeline 함수를 참고해보자.
				//- 현재 타임라인의 정보를 모두 바꾸자
				//- 타임라인 레이어의 정보를 모두 바꾸자
				//- 키프레임의 정보를 모두 바꾸자
				//- 링크 다시하면 어떻게 되지 않을까??? -ㅅ-?
				//- ModMesh/ModBone을 복사해야한다.

				curTimeline._linkedModifier = nextModifier;
				curTimeline._modifierUniqueID = nextModifier._uniqueID;

				for (int iLayer = 0; iLayer < curTimeline._layers.Count; iLayer++)
				{
					curTimelineLayer = curTimeline._layers[iLayer];

					#region [미사용 코드]
					////타임라인 레이어는 자동으로 되나?? 검사하자.
					//if(curTimelineLayer._linkedMeshTransform != null)
					//{
					//	apTransform_Mesh prevLinkedMeshTF = curTimelineLayer._linkedMeshTransform;

					//	//동일한게 있는지 찾자
					//	apTransform_Mesh nextLinkedMeshTF = nextMeshGroup.GetMeshTransformRecursive(prevLinkedMeshTF._transformUniqueID);
					//	//if(nextLinkedMeshTF != null)
					//	//{
					//	//	UnityEngine.Debug.Log(">> Find Next Mesh Transform!");
					//	//}
					//	//else
					//	//{
					//	//	UnityEngine.Debug.LogError(">> No Mesh Transform....");
					//	//}

					//}
					//else if(curTimelineLayer._linkedMeshGroupTransform != null)
					//{
					//	apTransform_MeshGroup prevLinkedMeshGroupTF = curTimelineLayer._linkedMeshGroupTransform;
					//	//UnityEngine.Debug.Log(">> MeshGroup Transform Type : " + prevLinkedMeshGroupTF._nickName + " (" + prevLinkedMeshGroupTF._transformUniqueID + ")");

					//	//동일한게 있는지 찾자
					//	apTransform_MeshGroup nextLinkedMeshGroupTF = nextMeshGroup.GetMeshGroupTransformRecursive(prevLinkedMeshGroupTF._transformUniqueID);
					//	//if(nextLinkedMeshGroupTF != null)
					//	//{
					//	//	UnityEngine.Debug.Log(">> Find Next MeshGroup Transform!");
					//	//}
					//	//else
					//	//{
					//	//	UnityEngine.Debug.LogError(">> No MeshGroup Transform....");
					//	//}
					//}
					//else if(curTimelineLayer._linkedBone != null)
					//{
					//	apBone prevLinkedBone = curTimelineLayer._linkedBone;
					//	//UnityEngine.Debug.Log(">> Bone Type : " + prevLinkedBone._name + " (" + prevLinkedBone._uniqueID + ")");

					//	//동일한게 있는지 찾자
					//	apBone nextLinkedBone = nextMeshGroup.GetBoneRecursive(prevLinkedBone._uniqueID);

					//	//if(nextLinkedBone != null)
					//	//{
					//	//	UnityEngine.Debug.Log(">> Find Next Bone!");
					//	//}
					//	//else
					//	//{
					//	//	UnityEngine.Debug.LogError(">> No Bone....");
					//	//}
					//}
					////else
					////{
					////	UnityEngine.Debug.LogError(">> No Linked Object..");
					////} 
					#endregion


					//키프레임을 검사하자
					//ModMesh/ModBone은 다시 링크하면 사라질 것 같으니 따로 저장하자.

					for (int iKeyframe = 0; iKeyframe < curTimelineLayer._keyframes.Count; iKeyframe++)
					{
						curKeyframe = curTimelineLayer._keyframes[iKeyframe];
						if(curKeyframe._linkedModMesh_Editor != null)
						{
							keyframe2PrevModMesh.Add(curKeyframe, curKeyframe._linkedModMesh_Editor);
						}
						if(curKeyframe._linkedModBone_Editor != null)
						{
							keyframe2PrevModBone.Add(curKeyframe, curKeyframe._linkedModBone_Editor);
						}
					}
				}

				
			}
			
			//타겟 변경
			animClip._targetMeshGroup = nextMeshGroup;
			animClip._targetMeshGroupID = nextMeshGroup._uniqueID;

			//일단 Link를 먼저 하자.
			animClip.LinkEditor(Editor._portrait);

			//이제 ParamSetGroup/ParamSet을 자동으로 생성
			AddAndSyncAnimClipToModifier(animClip, false);

			//다시 링크를 하자.
			Editor._portrait.LinkAndRefreshInEditor(true, null);

			curTimeline = null;
			curTimelineLayer = null;
			curKeyframe = null;
			
			//ModMesh와 ModBone의 데이터를 복사해주자.
			for (int iTimeline = 0; iTimeline < timelines.Count; iTimeline++)
			{
				curTimeline = timelines[iTimeline];
				if (curTimeline._linkType != apAnimClip.LINK_TYPE.AnimatedModifier)
				{
					continue;
				}

				curTimelineLayer = null;
				curKeyframe = null;
				for (int iLayer = 0; iLayer < curTimeline._layers.Count; iLayer++)
				{
					curTimelineLayer = curTimeline._layers[iLayer];
					for (int iKeyframe = 0; iKeyframe < curTimelineLayer._keyframes.Count; iKeyframe++)
					{
						curKeyframe = curTimelineLayer._keyframes[iKeyframe];
						apModifiedMesh prevModMesh = null;
						apModifiedBone prevModBone = null;
						apModifiedMesh nextModMesh = curKeyframe._linkedModMesh_Editor;
						apModifiedBone nextModBone = curKeyframe._linkedModBone_Editor;

						if(keyframe2PrevModMesh.ContainsKey(curKeyframe))
						{
							prevModMesh = keyframe2PrevModMesh[curKeyframe];
						}
						if(keyframe2PrevModBone.ContainsKey(curKeyframe))
						{
							prevModBone = keyframe2PrevModBone[curKeyframe];
						}

						//ModifiedMesh / ModifiedBone 중에서 애니메이션되는 값만 복사를 하자.
						
						if(nextModMesh != null && prevModMesh != null)
						{	
							//1) ModVert 복사
							if (nextModMesh._vertices != null && nextModMesh._vertices.Count > 0)
							{
								apModifiedVertex nextModVert = null;
								apModifiedVertex prevModVert = null;
								for (int iVert = 0; iVert < nextModMesh._vertices.Count; iVert++)
								{
									nextModVert = nextModMesh._vertices[iVert];
									
									if(nextModVert._vertexUniqueID < 0)
									{
										continue;
									}

									//값을 복사할 데이터를 찾자
									prevModVert = null;

									//데이터가 그대로 있는지확인
									if(prevModMesh._vertices != null && iVert < prevModMesh._vertices.Count)
									{
										prevModVert = prevModMesh._vertices[iVert];
										if(prevModVert._vertexUniqueID != nextModVert._vertexUniqueID)
										{
											//잉.. 다른 버텍스네..
											prevModVert = null;
										}
									}
									if(prevModVert == null)
									{
										prevModVert = prevModMesh._vertices.Find(delegate(apModifiedVertex a)
										{
											return a._vertexUniqueID == nextModVert._vertexUniqueID;
										});
									}
									if(prevModVert != null)
									{
										//값을 복사하자.
										nextModVert._deltaPos = prevModVert._deltaPos;
									}
								}
							}

							//2) Transform / Color 복사
							nextModMesh._transformMatrix.SetMatrix(prevModMesh._transformMatrix);
							nextModMesh._meshColor = prevModMesh._meshColor;
							nextModMesh._isVisible = prevModMesh._isVisible;

							//3) Extra Property 복사
							nextModMesh._isExtraValueEnabled = prevModMesh._isExtraValueEnabled;
							if(nextModMesh._extraValue == null)
							{
								nextModMesh._extraValue = new apModifiedMesh.ExtraValue();
								nextModMesh._extraValue.Init();
							}

							if (prevModMesh._isExtraValueEnabled)
							{
								if (prevModMesh._extraValue != null)
								{
									nextModMesh._extraValue._isDepthChanged = prevModMesh._extraValue._isDepthChanged;
									nextModMesh._extraValue._deltaDepth = prevModMesh._extraValue._deltaDepth;
									nextModMesh._extraValue._isTextureChanged = prevModMesh._extraValue._isTextureChanged;
									nextModMesh._extraValue._linkedTextureData = prevModMesh._extraValue._linkedTextureData;
									nextModMesh._extraValue._textureDataID = prevModMesh._extraValue._textureDataID;
									nextModMesh._extraValue._weightCutout = prevModMesh._extraValue._weightCutout;
									nextModMesh._extraValue._weightCutout_AnimPrev = prevModMesh._extraValue._weightCutout_AnimPrev;
									nextModMesh._extraValue._weightCutout_AnimNext = prevModMesh._extraValue._weightCutout_AnimNext;

									//UnityEngine.Debug.Log(">>> ExtraValue 복사 (Depth : " + nextModMesh._extraValue._isDepthChanged + " / Texture : " + nextModMesh._extraValue._isTextureChanged + ")");
								}
							}
						}

						if(nextModBone != null && prevModBone != null)
						{
							//ModBone은 Matrix 하나다.
							nextModBone._transformMatrix.SetMatrix(prevModBone._transformMatrix);
						}
					}
				}
			}

			//기존의 MeshGroup의 Modifier->ParamSetGroup에서 이 AnimClip은 제외하자.
			if(prevMeshGroup != null)
			{
				for (int i = 0; i < prevMeshGroup._modifierStack._modifiers.Count; i++)
				{
					apModifierBase prevModifier = prevMeshGroup._modifierStack._modifiers[i];
					if(!prevModifier.IsAnimated)
					{
						continue;
					}
					
					prevModifier._paramSetGroup_controller.RemoveAll(delegate(apModifierParamSetGroup a)
					{
						return a._keyAnimClip == animClip;
					});

					prevModifier._paramSetGroupAnimPacks.RemoveAll(delegate(apModifierParamSetGroupAnimPack a)
					{
						return a.LinkedAnimClip == animClip;
					});
				}
			}

			//Link를 다시 하자.
			animClip.LinkEditor(Editor._portrait);

			//객체가 추가/삭제시 호출
			Editor.OnAnyObjectAddedOrRemoved();

			//전체 Refresh를 해야한다.
			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null);//<<추가 : 타임라인 정보를 리셋

			AddAndSyncAnimClipToModifier(animClip, false);

			//두개의 MeshGroup에 대해서 모두 갱신
			//Editor._portrait.LinkAndRefreshInEditor(false, animClip._targetMeshGroup);
			Editor._portrait.LinkAndRefreshInEditor(false, null);

			Editor.RefreshControllerAndHierarchy(false);

			Editor.Select.SetAnimClip(animClip);

			//Refresh 추가
			//Editor.Select.SetNone();
			//Editor.Select.SetAnimClip(animClip);
			Editor.Select.RefreshAnimEditing(true);
			
		}

		private void OnModifierValidationKeyCheck(int[] modifierTypes, int[] validationKey, string[] names)
		{
			for (int i = 0; i < modifierTypes.Length; i++)
			{
				if(_requestedModType == modifierTypes[i])
				{
					_requestedModValidationKey = validationKey[i];
					return;
				}
			}
		}
		//-----------------------------------------------------------------------------------
		//-----------------------------------------------------------------------------------


		public apMeshGroup AddMeshGroup()
		{
			if (Editor._portrait == null)
			{
				return null;
			}

			//연결할 GameObjectGroup을 체크하자
			CheckAndMakeObjectGroup();


			//Undo - Add Mesh Group
			//apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Main_AddMeshGroup, Editor, Editor._portrait, null, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Create MeshGroup");

			//int nextID = Editor._portrait.MakeUniqueID_MeshGroup();
			int nextID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.MeshGroup);
			int nextRootTransformID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.Transform);

			if (nextID < 0 || nextRootTransformID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Mesh Add Group Failed. Please Retry", "Close");
				EditorUtility.DisplayDialog(Editor.GetText(TEXT.MeshGroupAddFailed_Title),
												Editor.GetText(TEXT.MeshGroupAddFailed_Body),
												Editor.GetText(TEXT.Close));
				return null;
			}


			int nMeshGroups = Editor._portrait._meshGroups.Count;

			//GameObject로 만드는 경우
			string newName = "New Mesh Group (" + nMeshGroups + ")";
			GameObject newGameObj = new GameObject(newName);
			newGameObj.transform.parent = Editor._portrait._subObjectGroup_MeshGroup.transform;
			newGameObj.transform.localPosition = Vector3.zero;
			newGameObj.transform.localRotation = Quaternion.identity;
			newGameObj.transform.localScale = Vector3.one;
			newGameObj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

			apMeshGroup newGroup = newGameObj.AddComponent<apMeshGroup>();

			//apMeshGroup newGroup = new apMeshGroup();

			newGroup._uniqueID = nextID;
			newGroup._presetType = apMeshGroup.PRESET_TYPE.Default;
			newGroup._name = newName;

			newGroup.MakeRootTransform(nextRootTransformID);//<<추가 : Root Transform 생성

			newGroup.Init(Editor._portrait);

			Editor._portrait._meshGroups.Add(newGroup);
			//Debug.Log("MeshGroup Added");

			newGroup.RefreshModifierLink();
			Editor._portrait.LinkAndRefreshInEditor(true, newGroup);//전체 Link 갱신

			//추가 : ExMode에 추가한다.
			Editor.Select.RefreshMeshGroupExEditingFlags(
							newGroup,
							null,
							null,
							null,
							true);

			Editor.Hierarchy.SetNeedReset();
			Editor.RefreshControllerAndHierarchy(false);
			//Editor.Hierarchy.RefreshUnits();

			//MeshGroup Hierarchy Filter를 활성화한다.
			Editor.SetHierarchyFilter(apEditor.HIERARCHY_FILTER.MeshGroup, true);

			//Undo - Create 추가
			apEditorUtil.SetRecordCreateMonoObject(newGroup, "Create MeshGroup");

			//프리팹이었다면 Apply
			apEditorUtil.SetPortraitPrefabApply(Editor._portrait);

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			return newGroup;
		}




		public void RemoveMeshGroup(apMeshGroup meshGroup)
		{
			if (Editor._portrait == null)
			{
				return;
			}

			//Undo - Remove MeshGroup
			//apEditorUtil.SetRecord_PortraitMeshGroupAndAllModifiers(apUndoGroupData.ACTION.Main_RemoveMeshGroup, 
			//														Editor,
			//														Editor._portrait, 
			//														meshGroup, null, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Mesh Group");

			//int meshGroupID = meshGroup._uniqueID;
			////Editor._portrait.PushUniqueID_MeshGroup(meshGroupID);

			List<MonoBehaviour> removedObjects = new List<MonoBehaviour>();
			List<apRootUnit> removedRootUnits = new List<apRootUnit>();
			List<apAnimClip> removedAnimClips = new List<apAnimClip>();


			RemoveChildMeshGroupsRecursive(meshGroup, removedObjects, removedRootUnits, removedAnimClips);

			//removedObjects.Add(meshGroup);

			////meshGroup의 Modifier도 같이 삭제해야 한다.
			//for (int iMod = 0; iMod < meshGroup._modifierStack._modifiers.Count; iMod++)
			//{
			//	apModifierBase modifier = meshGroup._modifierStack._modifiers[iMod];

			//	Editor._portrait.PushUnusedID(apIDManager.TARGET.Modifier, modifier._uniqueID);


			//	//Undo.DestroyObjectImmediate(modifier.gameObject);//<< 나중에 한꺼번에
			//	removedObjects.Add(modifier);
			//}

			//Editor._portrait.PushUnusedID(apIDManager.TARGET.MeshGroup, meshGroupID);

			//Editor._portrait._meshGroups.Remove(meshGroup);

			//if (meshGroup != null)
			//{
			//	//추가 : MeshGroup이 포함된 AnimClip과 RootUnit을 삭제한다.
			//	Editor._portrait._rootUnits.RemoveAll(delegate (apRootUnit a)
			//	{
			//		return a._childMeshGroup != null && a._childMeshGroup == meshGroup;
			//	});

			//	Editor._portrait._animClips.RemoveAll(delegate(apAnimClip a)
			//	{
			//		return a._targetMeshGroup != null && a._targetMeshGroup == meshGroup;
			//	});
			//}

			for (int iRoot = 0; iRoot < removedRootUnits.Count; iRoot++)
			{
				Editor._portrait._rootUnits.Remove(removedRootUnits[iRoot]);
			}

			for (int iAnim = 0; iAnim < removedAnimClips.Count; iAnim++)
			{
				Editor._portrait.PushUnusedID(apIDManager.TARGET.AnimClip, removedAnimClips[iAnim]._uniqueID);//ID 반환하고..
				Editor._portrait._animClips.Remove(removedAnimClips[iAnim]);
			}

			//MeshGroup + Modifier
			if (meshGroup != null)
			{
				//Undo.DestroyObjectImmediate(meshGroup.gameObject);
				apEditorUtil.SetRecordDestroyMonoObjects(removedObjects, "Remove MeshGroup");
			}

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor._portrait.LinkAndRefreshInEditor(true, null);//<<전체 링크 갱신

			Editor.Hierarchy.SetNeedReset();
			Editor.RefreshControllerAndHierarchy(false);
			//Editor.Hierarchy.RefreshUnits();

			//프리팹이었다면 Apply
			apEditorUtil.SetPortraitPrefabApply(Editor._portrait);
		}

		/// <summary>
		/// Target과 기준으로 하위의 MeshGroup들을 삭제한다.
		/// 실제 삭제는 리스트를 받아서 하자
		/// </summary>
		/// <param name="targetMeshGroup"></param>
		private void RemoveChildMeshGroupsRecursive(apMeshGroup targetMeshGroup,
														List<MonoBehaviour> removedObjects,
														List<apRootUnit> removedRootUnits,
														List<apAnimClip> removedAnimClips)
		{
			if (targetMeshGroup == null)
			{
				return;
			}


			//[Recursive]
			//Child MeshGroup부터 갔다온다.
			for (int iChild = 0; iChild < targetMeshGroup._childMeshGroupTransforms.Count; iChild++)
			{
				apTransform_MeshGroup childMeshGroupTransform = targetMeshGroup._childMeshGroupTransforms[iChild];
				RemoveChildMeshGroupsRecursive(childMeshGroupTransform._meshGroup, removedObjects, removedRootUnits, removedAnimClips);

			}


			int meshGroupID = targetMeshGroup._uniqueID;
			//Editor._portrait.PushUniqueID_MeshGroup(meshGroupID);

			removedObjects.Add(targetMeshGroup);

			//meshGroup의 Modifier도 같이 삭제해야 한다.
			for (int iMod = 0; iMod < targetMeshGroup._modifierStack._modifiers.Count; iMod++)
			{
				apModifierBase modifier = targetMeshGroup._modifierStack._modifiers[iMod];

				Editor._portrait.PushUnusedID(apIDManager.TARGET.Modifier, modifier._uniqueID);
				removedObjects.Add(modifier);
			}

			Editor._portrait.PushUnusedID(apIDManager.TARGET.MeshGroup, meshGroupID);//<<ID는 반납한다.
			Editor._portrait._meshGroups.Remove(targetMeshGroup);


			if (targetMeshGroup != null)
			{
				//같이 삭제되어야 하는 RootUnit과 AnimClip들
				for (int iRoot = 0; iRoot < Editor._portrait._rootUnits.Count; iRoot++)
				{
					apRootUnit rootUnit = Editor._portrait._rootUnits[iRoot];
					if (rootUnit._childMeshGroup != null && rootUnit._childMeshGroup == targetMeshGroup)
					{
						if (!removedRootUnits.Contains(rootUnit))
						{
							removedRootUnits.Add(rootUnit);
						}
					}
				}

				for (int iAnim = 0; iAnim < Editor._portrait._animClips.Count; iAnim++)
				{
					apAnimClip animClip = Editor._portrait._animClips[iAnim];
					if (animClip._targetMeshGroup != null && animClip._targetMeshGroup == targetMeshGroup)
					{
						if (!removedAnimClips.Contains(animClip))
						{
							removedAnimClips.Add(animClip);
						}
					}
				}

			}

		}





		public void DetachMeshInMeshGroup(apTransform_Mesh targetMeshTransform, apMeshGroup parentMeshGroup)
		{
			if (Editor._portrait == null)
			{
				return;
			}

			//추가 : 이 Transform이 Child에 속하는 것인지, 아니면 Recursive에 속하는 것인지 확인해야한다.
			//Recursive인 경우 해당 MeshGroup을 찾아야 한다.
			apMeshGroup parentMeshGroupOfTeransform = parentMeshGroup;
			if (parentMeshGroup.GetMeshTransform(targetMeshTransform._transformUniqueID) == null)
			{
				Debug.LogError("<Detach : 해당 MeshTransform이 MeshGroup에 존재하지 않는다.");
				//Recursive에 존재하는지 확인
				parentMeshGroupOfTeransform = parentMeshGroup.GetSubParentMeshGroupOfTransformRecursive(targetMeshTransform, null);

				//못찾은 경우
				if (parentMeshGroupOfTeransform == null)
				{
					Debug.LogError("<Parent MeshGroup>를 찾을 수 없다.");
					return;
				}
				if (parentMeshGroupOfTeransform != parentMeshGroup)
				{
					Debug.LogError("Recursive한 MeshTransform이다. Parent MeshGroup이 다르다.");
				}
			}

			//Undo - Detach
			//apEditorUtil.SetRecord_MeshGroupAllModifiers(apUndoGroupData.ACTION.MeshGroup_DetachMesh, Editor, parentMeshGroup, targetMeshTransform, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Detach");

			int removedUniqueID = targetMeshTransform._transformUniqueID;
			//Editor._portrait.PushUniqueID_Transform(removedUniqueID);
			Editor._portrait.PushUnusedID(apIDManager.TARGET.Transform, removedUniqueID);



			//parentMeshGroup._childMeshTransforms.Remove(targetMeshTransform);
			parentMeshGroupOfTeransform._childMeshTransforms.Remove(targetMeshTransform);//<<삭제는 이쪽에서 

			//parentMeshGroup.ResetRenderUnits();
			//parentMeshGroup.RefreshModifierLink();
			//parentMeshGroup.SortRenderUnits(true);

			//추가 : ExMode에 추가한다.
			Editor.Select.RefreshMeshGroupExEditingFlags(
								parentMeshGroup,
								null,
								null,
								null,
								true);

			Editor.Hierarchy.SetNeedReset();

			//추가 / 삭제시 요청한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor._portrait.LinkAndRefreshInEditor(true, parentMeshGroup);

			parentMeshGroup.ResetRenderUnits();
			parentMeshGroup.RefreshModifierLink();
			parentMeshGroup.SortRenderUnits(true);

			parentMeshGroup.RefreshForce();

			Editor.ResetHierarchyAll();
			Editor.RefreshControllerAndHierarchy(false);


		}

		public void DetachMeshGroupInMeshGroup(apTransform_MeshGroup targetMeshGroupTransform, apMeshGroup parentMeshGroup)
		{
			if (Editor._portrait == null)
			{
				return;
			}

			apMeshGroup parentMeshGroupOfTeransform = parentMeshGroup;
			if (parentMeshGroup.GetMeshGroupTransform(targetMeshGroupTransform._transformUniqueID) == null)
			{
				Debug.LogError("<Detach : 해당 MeshGroupTransform이 MeshGroup에 존재하지 않는다.");
				//Recursive에 존재하는지 확인
				parentMeshGroupOfTeransform = parentMeshGroup.GetSubParentMeshGroupOfTransformRecursive(null, targetMeshGroupTransform);

				//못찾은 경우
				if (parentMeshGroupOfTeransform == null)
				{
					Debug.LogError("<Parent MeshGroup>를 찾을 수 없다.");
					return;
				}
				if (parentMeshGroupOfTeransform != parentMeshGroup)
				{
					Debug.LogError("Recursive한 MeshTransform이다. Parent MeshGroup이 다르다.");
				}
			}
			//Undo - Detach
			//apEditorUtil.SetRecord_MeshGroupAllModifiers(apUndoGroupData.ACTION.MeshGroup_DetachMeshGroup, 
			//												Editor, parentMeshGroup, targetMeshGroupTransform, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Detach");

			if (targetMeshGroupTransform._meshGroup != null)
			{
				targetMeshGroupTransform._meshGroup._parentMeshGroup = null;
				targetMeshGroupTransform._meshGroup._parentMeshGroupID = -1;

				//targetMeshGroupTransform._meshGroup.SortRenderUnits(true);
			}

			int removedUniqueID = targetMeshGroupTransform._transformUniqueID;
			//Editor._portrait.PushUniqueID_Transform(removedUniqueID);
			Editor._portrait.PushUnusedID(apIDManager.TARGET.Transform, removedUniqueID);

			//parentMeshGroup._childMeshGroupTransforms.Remove(targetMeshGroupTransform);
			parentMeshGroupOfTeransform._childMeshGroupTransforms.Remove(targetMeshGroupTransform);//<<변경

			//parentMeshGroup.ResetRenderUnits();
			//parentMeshGroup.RefreshModifierLink();
			//parentMeshGroup.SortRenderUnits(true);

			//추가 : ExMode에 추가한다.
			Editor.Select.RefreshMeshGroupExEditingFlags(
								parentMeshGroup,
								null,
								null,
								null,
								true);

			Editor.Hierarchy.SetNeedReset();

			//추가 / 삭제시 요청한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor._portrait.LinkAndRefreshInEditor(true, parentMeshGroup);

			parentMeshGroup.ResetRenderUnits();
			parentMeshGroup.RefreshModifierLink();
			parentMeshGroup.SortRenderUnits(true);
			if (parentMeshGroup._parentMeshGroup != null)
			{
				parentMeshGroup._parentMeshGroup.RefreshModifierLink();
			}

			parentMeshGroup.RefreshForce();

			Editor.ResetHierarchyAll();
			Editor.RefreshControllerAndHierarchy(false);
			Editor.SetRepaint();



		}

		public void RefreshMeshGroups()
		{
			List<apMeshGroup> meshGroups = Editor._portrait._meshGroups;
			for (int i = 0; i < meshGroups.Count; i++)
			{
				apMeshGroup meshGroup = meshGroups[i];

				List<apRenderUnit> removableRenderUnits = new List<apRenderUnit>();

				if (meshGroup._rootRenderUnit == null)
				{
					continue;
				}
				CheckRemovableRenderUnit(meshGroup, meshGroup._rootRenderUnit, removableRenderUnits);

				if (removableRenderUnits.Count > 0)
				{
					meshGroup._renderUnits_All.RemoveAll(delegate (apRenderUnit a)
					{
						return removableRenderUnits.Contains(a);
					});

					meshGroup.SetDirtyToReset();
					meshGroup.RefreshForce();
					meshGroup.SortRenderUnits(true);
				}

				//Bone Refresh도 여기서 하자
				RefreshBoneHierarchy(meshGroup);
				RefreshBoneChaining(meshGroup);
			}
		}




		private void CheckRemovableRenderUnit(apMeshGroup parentMeshGroup, apRenderUnit curRenderUnit, List<apRenderUnit> removableRenderUnits)
		{
			bool isRemovable = false;
			if (curRenderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
			{
				//메시가 존재하는가
				if (curRenderUnit._meshTransform == null)
				{
					isRemovable = true;
					//Debug.LogError("R 1");
				}
				else if (curRenderUnit._meshTransform._mesh == null)
				{
					isRemovable = true;
					//Debug.LogError("R 2");
				}
				else if (GetMesh(curRenderUnit._meshTransform._mesh._uniqueID) == null)
				{
					isRemovable = true;
					//Debug.LogError("R 3"); 
				}
			}
			else
			{
				//메시 그룹이 존재하는가?
				//if(curRenderUnit._meshGroupTransform == null)											{ isRemovable = true; Debug.LogError("R 4"); }
				if (curRenderUnit._meshGroup == null)
				{
					isRemovable = true;
					//Debug.LogError("R 5");
				}
				else if (GetMeshGroup(curRenderUnit._meshGroup._uniqueID) == null)
				{
					isRemovable = true;
					//Debug.LogError("R 6 - " + curRenderUnit._meshGroup._name);
				}
			}

			if (isRemovable)
			{
				//이후 모든 Child는 다 Remove한다.
				AddChildRenderUnitsToRemove(parentMeshGroup, curRenderUnit, removableRenderUnits);
			}
			else
			{
				for (int i = 0; i < curRenderUnit._childRenderUnits.Count; i++)
				{
					CheckRemovableRenderUnit(parentMeshGroup, curRenderUnit._childRenderUnits[i], removableRenderUnits);
				}
			}
		}

		private void AddChildRenderUnitsToRemove(apMeshGroup parentMeshGroup, apRenderUnit curRenderUnit, List<apRenderUnit> removableRenderUnits)
		{
			if (curRenderUnit._unitType == apRenderUnit.UNIT_TYPE.GroupNode)
			{
				if (curRenderUnit._meshGroup != null)
				{
					if (curRenderUnit._meshGroup._parentMeshGroup == parentMeshGroup)
					{
						//Debug.LogError("Removable Unit : " + curRenderUnit._meshGroup._name + " In " + parentMeshGroup._name);
						curRenderUnit._meshGroup._parentMeshGroup = null;
						curRenderUnit._meshGroup._parentMeshGroupID = -1;
					}
				}
			}
			removableRenderUnits.Add(curRenderUnit);

			for (int i = 0; i < curRenderUnit._childRenderUnits.Count; i++)
			{
				AddChildRenderUnitsToRemove(parentMeshGroup, curRenderUnit._childRenderUnits[i], removableRenderUnits);
			}
		}

		/// <summary>
		/// Mesh의 Vertex가 바뀌면 이 함수를 호출한다.
		/// 모든 Render Unit들의 Vertex Buffer를 다시 리셋하게 만든다.
		/// </summary>
		public void ResetAllRenderUnitsVertexIndex()
		{
			if (Editor._portrait == null)
			{
				return;
			}

			for (int iMG = 0; iMG < Editor._portrait._meshGroups.Count; iMG++)
			{
				apMeshGroup meshGroup = Editor._portrait._meshGroups[iMG];
				for (int iRU = 0; iRU < meshGroup._renderUnits_All.Count; iRU++)
				{
					apRenderUnit renderUnit = meshGroup._renderUnits_All[iRU];
					renderUnit.ResetVertexIndex();
				}
			}

			//통계 재계산 요청
			Editor.Select.SetStatisticsRefresh();
		}

		//------------------------------------------------------------------------------------------
		// 본
		//------------------------------------------------------------------------------------------

		/// <summary>
		/// 본을 생성하여 TargetMeshGroup에 추가한다.
		/// 만약 루트 본이 아닌 경우 : ParentBone에 값을 넣어주면 자동으로 Child에 포함된다.
		/// null을 넣으면 루트 본으로 설정되어 MeshGroup에서 따로 관리하도록 한다.
		/// 그외 설정은 리턴값을 받아서 처리하자
		/// </summary>
		/// <param name="targetMeshGroup"></param>
		/// <param name="parentBone"></param>
		/// <returns></returns>
		public apBone AddBone(apMeshGroup targetMeshGroup, apBone parentBone, bool isRecordUndo = true)
		{
			if (Editor._portrait == null || targetMeshGroup == null)
			{
				return null;
			}
			if (isRecordUndo)
			{
				//Undo
				apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_AddBone, Editor, targetMeshGroup, null, false, false);
			}

			int nextID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.Bone);
			int meshGroupID = targetMeshGroup._uniqueID;

			if (nextID < 0 || meshGroupID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Adding Bone is Failed. Please Retry", "Close");

				EditorUtility.DisplayDialog(Editor.GetText(TEXT.BoneAddFailed_Title),
												Editor.GetText(TEXT.BoneAddFailed_Body),
												Editor.GetText(TEXT.Close));
				return null;
			}



			//이름을 지어주자
			string parentName = "Bone ";
			if (parentBone != null)
			{
				parentName = parentBone._name;
			}
			apEditorUtil.NameAndIndexPair nameIndexPair = apEditorUtil.ParseNumericName(parentName);

			int curIndex = nameIndexPair._index;
			//이제 index를 올려가면서 겹치는게 있는지 확인한다.

			curIndex++;
			string resultName = "";
			int nCnt = 0;
			while (true)
			{
				resultName = nameIndexPair.MakeNewName(curIndex);

				bool isAnySameName = false;
				if (parentBone == null)
				{
					for (int i = 0; i < targetMeshGroup._boneList_Root.Count; i++)
					{
						if (string.Equals(targetMeshGroup._boneList_Root[i]._name, resultName))
						{
							//같은 이름이 있다.
							isAnySameName = true;
							break;
						}
					}
				}
				else
				{
					for (int i = 0; i < parentBone._childBones.Count; i++)
					{
						if (string.Equals(parentBone._childBones[i]._name, resultName))
						{
							//같은 이름이 있다.
							isAnySameName = true;
							break;
						}
					}
				}
				if (isAnySameName)
				{
					//똑같은 이름이 있네염..
					curIndex++;

					nCnt++;
					if (nCnt > 100)
					{
						Debug.Log("Error");
						break;
					}
				}
				else
				{
					//다른 이름 찾음
					break;
				}
			}

			#region [미사용 코드]
			//string baseName = "Bone ";
			//int nameNumber = 0;

			//string resultName = baseName + nameNumber;
			//if(parentBone == null)
			//{
			//	//동일한 이름이 Root에 있는지 체크하자
			//	resultName = baseName + nameNumber;

			//	if (targetMeshGroup._boneList_Root.Count > 0)
			//	{
			//		while (true)
			//		{
			//			bool isAnySameName = false;
			//			for (int i = 0; i < targetMeshGroup._boneList_Root.Count; i++)
			//			{
			//				if(string.Equals(targetMeshGroup._boneList_Root[i]._name, resultName))
			//				{
			//					//같은 이름이 있다.
			//					isAnySameName = true;
			//					break;
			//				}
			//			}
			//			if(isAnySameName)
			//			{
			//				//똑같은 이름이 있네염..
			//				nameNumber++;
			//				resultName = baseName + nameNumber;
			//			}
			//			else
			//			{
			//				//다른 이름 찾음
			//				break;
			//			}
			//		}
			//	}

			//}
			//else
			//{
			//	baseName = parentBone._name + "";
			//	resultName = baseName + nameNumber;

			//	//동일한 이름이 Child에 있는지 체크하자
			//	if(parentBone._childBones.Count > 0)
			//	{
			//		while (true)
			//		{
			//			bool isAnySameName = false;
			//			for (int i = 0; i < parentBone._childBones.Count; i++)
			//			{
			//				if(string.Equals(parentBone._childBones[i]._name, resultName))
			//				{
			//					//같은 이름이 있다.
			//					isAnySameName = true;
			//					break;
			//				}
			//			}

			//			if(isAnySameName)
			//			{
			//				//똑같은 이름이 있네염..
			//				nameNumber++;
			//				resultName = baseName + nameNumber;
			//			}
			//			else
			//			{
			//				//다른 이름 찾음
			//				break;
			//			}
			//		}

			//	}
			//} 
			#endregion

			apBone newBone = new apBone(nextID, meshGroupID, resultName);

			if (parentBone != null)
			{
				//변경 19.8.13 : 옵션에 따라서
				//- 색이 일정하게 되거나
				//- 색이 아예 비슷하지 않게 되도록 만든다.
				
				Color boneGUIColor = Color.black;

				if(Editor._rigOption_NewChildBoneColorIsLikeParent)
				{
					//비슷한 색상
					boneGUIColor = apEditorUtil.GetSimilarColor(parentBone._color, 0.8f, 1.0f, 0.9f, 1.0f);
				}
				else
				{
					//완전히 다른 색상
					if(parentBone._parentBone != null)
					{
						//부모의 부모가 있는 경우
						boneGUIColor = apEditorUtil.GetDiffierentColor(parentBone._color, parentBone._parentBone._color, 0.8f, 1.0f, 0.9f, 1.0f);
					}
					else
					{
						boneGUIColor = apEditorUtil.GetDiffierentColor(parentBone._color, 0.8f, 1.0f, 0.9f, 1.0f);
					}

					
				}

				boneGUIColor.a = 1.0f;
				
				newBone._color = boneGUIColor;
			}
			else
			{
				//랜덤한 색상으로 정하자. (19.8.13)
				//기존과 달리 채도가 높고 밝은 색상으로 고른다.
				Color boneGUIColor = apEditorUtil.GetRandomColor(0.8f, 1.0f, 0.9f, 1.0f);
				boneGUIColor.a = 1.0f;
				newBone._color = boneGUIColor;
			}

			//버그 수정 10.2 : IK 설정 초기화가 안되어있었다.
			newBone._optionIK = apBone.OPTION_IK.Disabled;
			newBone._IKTargetBone = null;
			newBone._IKNextChainedBone = null;
			newBone._IKTargetBoneID = -1;
			newBone._IKNextChainedBoneID = -1;

			//ParentBone을 포함해서 Link를 한다.
			//ParentBone이 있다면 이 Bone이 ChildList로 자동으로 추가된다.
			newBone.Link(targetMeshGroup, parentBone, Editor._portrait);
			newBone.InitTransform();

			targetMeshGroup._boneList_All.Add(newBone);//<<새로운 Bone을 추가하자

			if (newBone._parentBone == null)
			{
				//Root Bone이라면
				targetMeshGroup._boneList_Root.Add(newBone);//Root List에도 추가한다.
			}

			//newBone.Link(targetMeshGroup, parentBone);//<<이걸 두번할 필요가 없는데..

			targetMeshGroup.RefreshBoneGUIVisible();

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			return newBone;
		}


		public void RemoveAllBones(apMeshGroup targetMeshGroup)
		{
			if (targetMeshGroup == null)
			{
				return;
			}
			//Undo
			//apEditorUtil.SetRecord_MeshGroupAllModifiers(apUndoGroupData.ACTION.MeshGroup_RemoveAllBones, Editor, targetMeshGroup, null, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove All Bones");

			//일단 ID 반납
			int nBones = targetMeshGroup._boneList_All.Count;
			for (int i = 0; i < nBones; i++)
			{
				Editor._portrait.PushUnusedID(apIDManager.TARGET.Bone, targetMeshGroup._boneList_All[i]._uniqueID);
			}

			targetMeshGroup._boneList_All.Clear();
			targetMeshGroup._boneList_Root.Clear();

			Editor.Select.SetBone(null);

			Editor._portrait.LinkAndRefreshInEditor(true, targetMeshGroup);

			targetMeshGroup.SortRenderUnits(true);
			targetMeshGroup.RefreshForce();

			Editor.RefreshControllerAndHierarchy(false);

			Editor.Notification("All Bones of [" + targetMeshGroup._name + "] are removed", true, false);

			targetMeshGroup.RefreshBoneGUIVisible();

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();
		}


		public void RemoveBone(apBone bone, bool isRemoveChildren)
		{
			if (bone == null)
			{
				return;
			}

			//Debug.Log("Remove Bone : " + bone._name + " / Remove Children : " + isRemoveChildren);

			apMeshGroup meshGroup = bone._meshGroup;
			apBone parentBone = bone._parentBone;

			List<string> removedNames = new List<string>();

			//apEditorUtil.SetRecord_MeshGroupAllModifiers(apUndoGroupData.ACTION.MeshGroup_RemoveBone, Editor, bone._meshGroup, bone, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Bone");

			if (!isRemoveChildren)
			{
				//< Children을 삭제하지 않을때 >
				//1. Parent에서 Bone을 삭제한다.
				//2. Bone의 Child를 Parent (또는 Null)에 연결한다.

				//Parent - [삭제할 Bone] - Child에서
				//Child를 삭제하지 않는다면
				//Parent <- Child로 연결한다.

				//3.연결할 때, 각각의 Child의 Matrix를 갱신한다.

				//4. MeshGroup에서 Bone을 삭제하고 Selection에서 해제한다.
				//5. Refresh

				removedNames.Add(bone._name);

				//1)
				if (parentBone != null)
				{
					parentBone._childBones.Remove(bone);
					parentBone._childBoneIDs.Remove(bone._uniqueID);
				}

				//2, 3)
				for (int i = 0; i < bone._childBones.Count; i++)
				{
					apBone childBone = bone._childBones[i];

					apMatrix nextParent_Matrix = null;
					if (parentBone != null)
					{
						childBone._parentBone = parentBone;
						childBone._parentBoneID = parentBone._uniqueID;

						if (!parentBone._childBones.Contains(childBone))
						{
							parentBone._childBones.Add(childBone);
						}
						if (!parentBone._childBoneIDs.Contains(childBone._uniqueID))
						{
							parentBone._childBoneIDs.Add(childBone._uniqueID);
						}

						nextParent_Matrix = parentBone._worldMatrix;
					}
					else
					{
						childBone._parentBone = null;
						childBone._parentBoneID = -1;

						if (bone._renderUnit != null)
						{
							nextParent_Matrix = bone._renderUnit.WorldMatrixWrap;
						}
						else
						{
							nextParent_Matrix = new apMatrix();
						}
					}

					//기본 default * local 변환값이 들어간 Local Bone Matrix를 구하자
					//apMatrix localBoneMatrix = apMatrix.RMultiply(childBone._defaultMatrix, childBone._localMatrix);

					//현재의 worldMatrix
					apMatrix worldBoneMatrix = childBone._worldMatrix;

					//default (Prev) * localMatrix (고정) * parentMatrix (Prev) => World Matrix (동일)
					//default (Next) * localMatrix (고정) * parentMatrix (Next) => World Matrix (동일)

					// [Default (Next) * local Matrix] = World Matrix inv parentMatrix (Next)
					// Default
					apMatrix newDefaultMatrix = apMatrix.RInverse(apMatrix.RInverse(worldBoneMatrix, nextParent_Matrix), childBone._localMatrix);
					newDefaultMatrix._angleDeg = apUtil.AngleTo180(newDefaultMatrix._angleDeg);
					childBone._defaultMatrix.SetMatrix(newDefaultMatrix);
				}


				//IK Option은 바꾸지 않는다.

				//4. MeshGroup에서 Bone을 삭제하고 Selection에서 해제한다.
				//혹시 모를 연동을 위해 에러를 발생하도록 하자
				Editor._portrait.PushUnusedID(apIDManager.TARGET.Bone, bone._uniqueID);

				bone._parentBone = null;
				bone._parentBoneID = -1;
				bone._meshGroup = null;
				bone._meshGroupID = -1;
				bone._childBones.Clear();
				bone._childBoneIDs.Clear();

				meshGroup._boneList_All.Remove(bone);
				meshGroup._boneList_Root.Remove(bone);

			}
			else
			{
				//< 모든 Children을 삭제한다. >
				//Parent에서 bone 연결 끊고 삭제하면 되므로 간단.
				if (parentBone != null)
				{
					parentBone._childBones.Remove(bone);
					parentBone._childBoneIDs.Remove(bone._uniqueID);
				}

				//재귀적으로 삭제를 해주자
				RemoveBoneWithChildrenRecursive(bone, meshGroup, removedNames);
			}

			Editor.Select.SetBone(null);


			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor._portrait.LinkAndRefreshInEditor(true, meshGroup);//전체 리셋

			RefreshBoneHierarchy(bone._meshGroup);
			RefreshBoneChaining(bone._meshGroup);

			if (meshGroup != null)
			{
				meshGroup.SortRenderUnits(true);
				meshGroup.RefreshForce();
			}

			Editor.Hierarchy_MeshGroup.ResetSubUnits();//<아예 리셋해야함
			Editor.Hierarchy_AnimClip.ResetSubUnits();
			Editor.RefreshControllerAndHierarchy(false);



			if (removedNames.Count == 1)
			{
				Editor.Notification("[" + removedNames[0] + "] is removed", true, false);
			}
			else
			{
				string strRemoved = "";
				int nNames = removedNames.Count;
				if (nNames > 3)
				{
					nNames = 3;
				}
				for (int i = 0; i < nNames; i++)
				{
					if (i != 0)
					{
						strRemoved += ", ";
					}
					strRemoved += removedNames[i];

				}
				if (removedNames.Count > 3)
				{
					strRemoved += "...";
				}

				Editor.Notification("[" + strRemoved + "] are removed", true, false);
			}


		}

		private void RemoveBoneWithChildrenRecursive(apBone bone, apMeshGroup meshGroup, List<string> removedNames)
		{
			for (int i = 0; i < bone._childBones.Count; i++)
			{
				RemoveBoneWithChildrenRecursive(bone._childBones[i], meshGroup, removedNames);
			}

			Editor._portrait.PushUnusedID(apIDManager.TARGET.Bone, bone._uniqueID);
			meshGroup._boneList_All.Remove(bone);
			meshGroup._boneList_Root.Remove(bone);

			bone._parentBone = null;
			bone._parentBoneID = -1;
			bone._meshGroup = null;
			bone._meshGroupID = -1;
			bone._childBones.Clear();
			bone._childBoneIDs.Clear();

			removedNames.Add(bone._name);
		}

		public void AttachBoneToChild(apBone bone, apBone attachedBone)
		{
			if (bone == null || attachedBone == null)
			{
				return;
			}


			//Undo
			apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_AttachBoneToChild, Editor, bone._meshGroup, bone, false, false);

			if (bone.GetParentRecursive(attachedBone._uniqueID) != null)
			{
				//Parent가 오면 Loop가 발생한다.
				return;
			}
			apMeshGroup targetMeshGroup = bone._meshGroup;


			//Child로 추가한다.
			//Child의 Parent를 연결한다.
			//기존의 Child의 Parent에서는 Child를 제외한다.
			//IK가 Disabled -> Single로 가능하면 Single로 만들어준다.
			//Child의 Default Matrix를 보정해준다.

			targetMeshGroup.RefreshForce();
			apMatrix worldBoneMatrix_Prev = new apMatrix(attachedBone._worldMatrix_NonModified);



			if (!bone._childBones.Contains(attachedBone))
			{
				bone._childBones.Add(attachedBone);
			}
			if (!bone._childBoneIDs.Contains(attachedBone._uniqueID))
			{
				bone._childBoneIDs.Add(attachedBone._uniqueID);
			}

			attachedBone._parentBone = bone;
			attachedBone._parentBoneID = bone._uniqueID;

			apBone prevParentBoneOffAttachedBone = attachedBone._parentBone;
			if (prevParentBoneOffAttachedBone != null)
			{
				prevParentBoneOffAttachedBone._childBones.Remove(attachedBone);
				prevParentBoneOffAttachedBone._childBoneIDs.Remove(attachedBone._uniqueID);
			}

			if (bone._childBones.Count == 1 && bone._optionIK == apBone.OPTION_IK.Disabled)
			{
				bone._optionIK = apBone.OPTION_IK.IKSingle;
				bone._IKTargetBone = attachedBone;
				bone._IKTargetBoneID = attachedBone._uniqueID;

				bone._IKNextChainedBone = attachedBone;
				bone._IKNextChainedBoneID = attachedBone._uniqueID;
			}

			targetMeshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();
			targetMeshGroup.RefreshForce(true);

			//Attached Bone의 Default Matrix를 갱신하자

			//기본 default * local 변환값이 들어간 Local Bone Matrix를 구하자
			//apMatrix localBoneMatrix = apMatrix.RMultiply(attachedBone._defaultMatrix, attachedBone._localMatrix);

			//현재의 worldMatrix
			//apMatrix worldBoneMatrix = attachedBone._worldMatrix;


			//default (Prev) * localMatrix (고정) * parentMatrix (Prev) => World Matrix (동일)
			//default (Next) * localMatrix (고정) * parentMatrix (Next) => World Matrix (동일)

			apMatrix nextParent_Matrix = null;
			if (bone._renderUnit != null)
			{
				nextParent_Matrix = bone._renderUnit.WorldMatrixWrap;
			}
			else
			{
				nextParent_Matrix = new apMatrix();
			}
			nextParent_Matrix = bone._worldMatrix;


			// [Default (Next) * local Matrix] = World Matrix inv parentMatrix (Next)
			// Default
			apMatrix newDefaultMatrix = apMatrix.RInverse(apMatrix.RInverse(worldBoneMatrix_Prev, nextParent_Matrix), attachedBone._localMatrix);
			//apMatrix newDefaultMatrix = apMatrix.RInverse(worldBoneMatrix, nextParent_Matrix);
			newDefaultMatrix._angleDeg = apUtil.AngleTo180(newDefaultMatrix._angleDeg);
			attachedBone._defaultMatrix.SetMatrix(newDefaultMatrix);//<<여기서 적용. 일단 빼보자

			Editor._portrait.LinkAndRefreshInEditor(false, targetMeshGroup);
			targetMeshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();
			targetMeshGroup.RefreshForce(true);
			targetMeshGroup.UpdateBonesWorldMatrix();



			apMeshGroup.BoneListSet boneSet = null;
			for (int iSet = 0; iSet < targetMeshGroup._boneListSets.Count; iSet++)
			{
				boneSet = targetMeshGroup._boneListSets[iSet];

				for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)
				{
					boneSet._bones_Root[iRoot].MakeWorldMatrix(true);
					boneSet._bones_Root[iRoot].GUIUpdate(true);
				}
			}


			RefreshBoneHierarchy(bone._meshGroup);
			RefreshBoneChaining(bone._meshGroup);


			Editor.Hierarchy_MeshGroup.ResetSubUnits();//<아예 리셋해야함
			Editor.RefreshControllerAndHierarchy(false);

			Editor.OnAnyObjectAddedOrRemoved();
			Editor.SetRepaint();
		}

		public void DetachBoneFromChild(apBone bone, apBone detachedBone)
		{
			if (bone == null || detachedBone == null)
			{
				return;
			}
			//Undo
			apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_DetachBoneFromChild, Editor, bone._meshGroup, bone, false, false);

			apMeshGroup targetMeshGroup = bone._meshGroup;

			//Child를 제거한다.
			//Child는 Parent가 없어졌으므로 Root가 된다.
			//Default Matrix 보존해줄 것
			targetMeshGroup.RefreshForce();

			apMatrix worldBoneMatrix_Prev = new apMatrix(detachedBone._worldMatrix);//<<변경 : 값을 복사해야한다.

			bone._childBones.Remove(detachedBone);
			bone._childBoneIDs.Remove(detachedBone._uniqueID);

			detachedBone._parentBone = null;
			detachedBone._parentBoneID = -1;

			//DetachedBone의 Parent가 없으므로 IK는 해제되고 Root에 들어가야 한다.
			if(!targetMeshGroup._boneList_Root.Contains(detachedBone))
			{
				targetMeshGroup._boneList_Root.Add(detachedBone);
			}

			


			targetMeshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();
			targetMeshGroup.RefreshForce(true);

			//Detached Bone의 Default Matrix를 갱신하자

			//기본 default * local 변환값이 들어간 Local Bone Matrix를 구하자
			//apMatrix localBoneMatrix = apMatrix.RMultiply(detachedBone._defaultMatrix, detachedBone._localMatrix);

			//현재의 worldMatrix


			//default (Prev) * localMatrix (고정) * parentMatrix (Prev) => World Matrix (동일)
			//default (Next) * localMatrix (고정) * parentMatrix (Next) => World Matrix (동일)

			apMatrix nextParent_Matrix = null;
			if (bone._renderUnit != null)
			{
				nextParent_Matrix = bone._renderUnit.WorldMatrixWrap;
			}
			else
			{
				nextParent_Matrix = new apMatrix();
			}
			//nextParent_Matrix = bone._worldMatrix;


			// [Default (Next) * local Matrix] = World Matrix inv parentMatrix (Next)
			// Default
			apMatrix newDefaultMatrix = apMatrix.RInverse(apMatrix.RInverse(worldBoneMatrix_Prev, nextParent_Matrix), detachedBone._localMatrix);
			newDefaultMatrix._angleDeg = apUtil.AngleTo180(newDefaultMatrix._angleDeg);
			detachedBone._defaultMatrix.SetMatrix(newDefaultMatrix);

			targetMeshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();

			targetMeshGroup.RefreshForce();
			Editor._portrait.LinkAndRefreshInEditor(false, targetMeshGroup);

			targetMeshGroup.UpdateBonesWorldMatrix();

			apMeshGroup.BoneListSet boneSet = null;
			for (int iSet = 0; iSet < targetMeshGroup._boneListSets.Count; iSet++)
			{
				boneSet = targetMeshGroup._boneListSets[iSet];

				for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)
				{
					boneSet._bones_Root[iRoot].MakeWorldMatrix(true);
					boneSet._bones_Root[iRoot].GUIUpdate(true);
				}
			}

			RefreshBoneHierarchy(targetMeshGroup);
			RefreshBoneChaining(targetMeshGroup);


			Editor.Hierarchy_MeshGroup.ResetSubUnits();//<아예 리셋해야함
			Editor.RefreshControllerAndHierarchy(false);
		}


		public void SetBoneAsParent(apBone bone, apBone parentBone)
		{
			if (bone == null)
			{
				return;
			}

			if (parentBone == bone._parentBone)
			{
				return;
			}





			//Undo
			apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_SetBoneAsParent, Editor, bone._meshGroup, bone, false, false);

			//Parent Bone은 Null이 될 수 있다. (Root가 되는 경우)
			apMeshGroup targetMeshGroup = bone._meshGroup;

			targetMeshGroup.RefreshForce();
			apMatrix worldBoneMatrix_Prev = new apMatrix(bone._worldMatrix_NonModified);

			//1. 기존의 Parent에서 지금 Bone을 Child에서 뺀다.
			//2. 새로운 Parent에서 지금 Bone을 추가한다.
			//3. 새로운 Parent를 지금 Bone의 Parent로 등록한다.
			//4. Refresh
			//중요 > WorldMatrix를 보존해야한다.
			apMatrix prevParent_Matrix = null;
			apMatrix nextParent_Matrix = null;


			apBone prevParent = bone._parentBone;
			if (prevParent != null)
			{
				prevParent._childBones.Remove(bone);
				prevParent._childBoneIDs.Remove(bone._uniqueID);

				prevParent_Matrix = prevParent._worldMatrix;
			}
			else
			{
				if (bone._renderUnit != null)
				{
					prevParent_Matrix = bone._renderUnit.WorldMatrixWrap;
				}
				else
				{
					prevParent_Matrix = new apMatrix();
				}
			}

			if (parentBone != null)
			{
				parentBone._childBones.Add(bone);
				parentBone._childBoneIDs.Add(bone._uniqueID);

				bone._parentBone = parentBone;
				bone._parentBoneID = parentBone._uniqueID;

				if (parentBone._optionIK == apBone.OPTION_IK.Disabled && parentBone._childBones.Count == 1)
				{
					//처음 들어간거라면 자동으로 IK를 설정해주자.
					parentBone._optionIK = apBone.OPTION_IK.IKSingle;
					parentBone._IKTargetBone = parentBone._childBones[0];
					parentBone._IKTargetBoneID = parentBone._childBones[0]._uniqueID;

					parentBone._IKNextChainedBone = parentBone._childBones[0];
					parentBone._IKNextChainedBoneID = parentBone._childBones[0]._uniqueID;
				}

				nextParent_Matrix = parentBone._worldMatrix;
			}
			else
			{
				bone._parentBone = null;
				bone._parentBoneID = -1;

				if (bone._renderUnit != null)
				{
					nextParent_Matrix = bone._renderUnit.WorldMatrixWrap;
				}
				else
				{
					nextParent_Matrix = new apMatrix();
				}
			}

			targetMeshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();
			targetMeshGroup.RefreshForce(true);

			//Default Matrix를 갱신하자

			//기본 default * local 변환값이 들어간 Local Bone Matrix를 구하자
			//apMatrix localBoneMatrix = apMatrix.RMultiply(bone._defaultMatrix, bone._localMatrix);

			//현재의 worldMatrix
			//apMatrix worldBoneMatrix = bone._worldMatrix;

			//default (Prev) * localMatrix (고정) * parentMatrix (Prev) => World Matrix (동일)
			//default (Next) * localMatrix (고정) * parentMatrix (Next) => World Matrix (동일)

			// [Default (Next) * local Matrix] = World Matrix inv parentMatrix (Next)
			// Default
			apMatrix newDefaultMatrix = apMatrix.RInverse(apMatrix.RInverse(worldBoneMatrix_Prev, nextParent_Matrix), bone._localMatrix);
			newDefaultMatrix._angleDeg = apUtil.AngleTo180(newDefaultMatrix._angleDeg);
			bone._defaultMatrix.SetMatrix(newDefaultMatrix);

			//bone._meshGroup.RefreshForce();
			//Editor._portrait.LinkAndRefreshInEditor(false, targetMeshGroup);
			Editor._portrait.LinkAndRefreshInEditor(false, targetMeshGroup);
			targetMeshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();
			targetMeshGroup.RefreshForce(true);
			targetMeshGroup.UpdateBonesWorldMatrix();

			apMeshGroup.BoneListSet boneSet = null;
			for (int iSet = 0; iSet < targetMeshGroup._boneListSets.Count; iSet++)
			{
				boneSet = targetMeshGroup._boneListSets[iSet];

				for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)
				{
					boneSet._bones_Root[iRoot].MakeWorldMatrix(true);
					boneSet._bones_Root[iRoot].GUIUpdate(true);
				}
			}


			RefreshBoneHierarchy(bone._meshGroup);
			RefreshBoneChaining(bone._meshGroup);


			Editor.Hierarchy_MeshGroup.ResetSubUnits();//<아예 리셋해야함
			Editor.RefreshControllerAndHierarchy(false);

			Editor.OnAnyObjectAddedOrRemoved();
			Editor.SetRepaint();
		}


		public void SetBoneAsIKTarget(apBone bone, apBone IKTargetBone)
		{
			if (bone == null || IKTargetBone == null)
			{
				return;
			}

			if (bone.GetChildBoneRecursive(IKTargetBone._uniqueID) == null)
			{
				return;
			}

			apBone nextChainedBone = bone.FindNextChainedBone(IKTargetBone._uniqueID);
			if (nextChainedBone == null)
			{
				return;
			}

			apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_SetBoneAsIKTarget, Editor, bone._meshGroup, bone, false, false);

			//string prevIKTargetBoneName = "<None>";
			//if (bone._IKTargetBone != null)
			//{
			//	prevIKTargetBoneName = bone._IKTargetBone._name;
			//}

			//Debug.Log("Set Bone As IK Target : [" + prevIKTargetBoneName + " >> " + IKTargetBone._name + "] (" + bone._name + ")");



			bone._IKTargetBone = IKTargetBone;
			bone._IKTargetBoneID = IKTargetBone._uniqueID;

			bone._IKNextChainedBone = nextChainedBone;
			bone._IKNextChainedBoneID = nextChainedBone._uniqueID;


			RefreshBoneHierarchy(bone._meshGroup);
			RefreshBoneChaining(bone._meshGroup);

			Editor.RefreshControllerAndHierarchy(false);

			//string curIKTargetBoneName = "<None>";
			//if (bone._IKTargetBone != null)
			//{
			//	curIKTargetBoneName = bone._IKTargetBone._name;
			//}

			//Debug.Log("Set Bone As IK Target : Refresh Finined [ Cur IK Target : " + curIKTargetBoneName + " / Request IK Target : " + IKTargetBone._name + "]");
		}

		public void SetBoneAsIKPositionControllerEffector(apBone bone, apBone effectorBone)
		{
			if (bone == null)
			{
				return;
			}

			apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_BoneIKControllerChanged, Editor, bone._meshGroup, bone, false, false);

			if (bone._IKController == null)
			{
				bone._IKController = new apBoneIKController();
				bone._IKController.Link(bone, bone._meshGroup, Editor._portrait);
			}

			bone._IKController._controllerType = apBoneIKController.CONTROLLER_TYPE.Position;
			bone._IKController._effectorBone = effectorBone;
			if (effectorBone == null)
			{
				bone._IKController._effectorBoneID = -1;
			}
			else
			{
				bone._IKController._effectorBoneID = effectorBone._uniqueID;
			}

			RefreshBoneHierarchy(bone._meshGroup);
			RefreshBoneChaining(bone._meshGroup);
		}

		public void SetBoneAsIKLookAtControllerEffectorOrStartBone(apBone bone, apBone effectorOrStartBone, bool isEffectBone)
		{
			if (bone == null)
			{
				return;
			}

			apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_BoneIKControllerChanged, Editor, bone._meshGroup, bone, false, false);

			if (bone._IKController == null)
			{
				bone._IKController = new apBoneIKController();
				bone._IKController.Link(bone, bone._meshGroup, Editor._portrait);
			}

			bone._IKController._controllerType = apBoneIKController.CONTROLLER_TYPE.LookAt;
			if (isEffectBone)
			{
				bone._IKController._effectorBone = effectorOrStartBone;
				if (effectorOrStartBone == null)
				{
					bone._IKController._effectorBoneID = -1;
				}
				else
				{
					bone._IKController._effectorBoneID = effectorOrStartBone._uniqueID;
				}
			}
			else
			{
				//bone._IKController._startBone = effectorOrStartBone;
				//if (effectorOrStartBone == null)
				//{
				//	bone._IKController._startBoneID = -1;
				//}
				//else
				//{
				//	bone._IKController._startBoneID = effectorOrStartBone._uniqueID;
				//}
			}

			////SubChain을 Refresh한다.
			//bone._IKController.RefreshSubChainedBones();

			RefreshBoneHierarchy(bone._meshGroup);
			RefreshBoneChaining(bone._meshGroup);
		}


		public void SetBoneAsMirror(apBone bone, apBone mirror)
		{
			if (bone == null)
			{
				return;
			}
			apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_BoneMirrorChanged, Editor, bone._meshGroup, bone, false, false);

			apBone prevMirrorBone = bone._mirrorBone;//기존의 Mirror Bone
			if (prevMirrorBone != null
				&& prevMirrorBone != mirror)
			{
				if (prevMirrorBone._mirrorBone == bone)
				{
					//Mirror가 바뀌었다면
					//기존 Mirror는 연결을 끊는다.
					prevMirrorBone._mirrorBone = null;
					prevMirrorBone._mirrorBoneID = -1;
				}
			}

			bone._mirrorBone = mirror;
			if (bone._mirrorBone != null)
			{
				bone._mirrorBoneID = bone._mirrorBone._uniqueID;
			}
			else
			{
				bone._mirrorBoneID = -1;
			}

			//Mirror도 서로 연결해준다.
			if (mirror != null)
			{
				mirror._mirrorBone = bone;
				mirror._mirrorBoneID = bone._uniqueID;
			}
		}


		/// <summary>
		/// 새로운 Mirror Bone을 만든다.
		/// </summary>
		/// <param name="meshGroup"></param>
		/// <param name="bone"></param>
		public void MakeNewMirrorBone(apMeshGroup meshGroup, apBone bone)
		{
			if (meshGroup == null || bone == null)
			{
				return;
			}

			//경고 문구
			//1. 이미 Mirror가 있는 경우
			//2. Child를 모두 Mirror할 것인지
			if (bone._mirrorBone != null)
			{
				bool isResult = EditorUtility.DisplayDialog(
					Editor.GetText(TEXT.DLG_MirrorBoneWarning_Title),//"Creating a Mirror Bone", 
					Editor.GetText(TEXT.DLG_MirrorBoneWarning_Body_Aleady),//"Mirror Bone already exists. Create a new Mirror Bone?",
					Editor.GetText(TEXT.Okay), Editor.GetText(TEXT.Cancel));
				if (!isResult)
				{
					return;
				}
			}

			int createWithChild = EditorUtility.DisplayDialogComplex(
				Editor.GetText(TEXT.DLG_MirrorBoneWarning_Title),//"Creating a mirror bone",
				Editor.GetText(TEXT.DLG_MirrorBoneWarning_Body_Children),//"Do you also want to create the mirror bones for the child Bones? If any of the child Bones have a Mirror Bone, they will be disconnected.",
				Editor.GetText(TEXT.DLG_MirrorBoneWarning_Btn1_WithAllChildBones),//"with all Child Bones", 
				Editor.GetText(TEXT.DLG_MirrorBoneWarning_Btn2_OnlySelectedBone),//"Only selected bone", 
				Editor.GetText(TEXT.Cancel));

			//Debug.Log("Create Mirror [" + createWithChild + "]");
			if (createWithChild == 2)
			{
				//Cancel
				return;
			}

			//Undo
			apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_AddBone, Editor, meshGroup, null, false, false);

			bool isCreateWithChild = (createWithChild == 0);

			//첫 Bone을 제외하면 Recursive하게 해야한다.
			//첫 Bone은 Offset을 적용, 그 외에는 링크에 맞게 동일
			//처리는 Recursive하게 해야한다.
			Dictionary<apBone, apBone> src2Mirror = new Dictionary<apBone, apBone>();
			Dictionary<apBone, apBone> mirror2Src = new Dictionary<apBone, apBone>();

			MakeNewMirrorBoneHierarchyRecursive(meshGroup,
														bone, bone._parentBone,
														bone,
														isCreateWithChild,
														src2Mirror,
														mirror2Src);


			Editor._portrait.LinkAndRefreshInEditor(false, meshGroup);
			meshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();
			meshGroup.RefreshForce(true);
			meshGroup.UpdateBonesWorldMatrix();

			RefreshBoneHierarchy(meshGroup);

			//IK 연결을 갱신한다.
			if (bone._mirrorBone != null)
			{
				SetMirrorBonesLinks(bone._mirrorBone, src2Mirror, mirror2Src);
			}

			//RefreshBoneChainingUnit(meshGroup, bone);
			RefreshBoneChainingUnit(bone);//<<MeshGroup 빠짐
			if (bone._mirrorBone != null)
			{
				//RefreshBoneChainingUnit(meshGroup, bone._mirrorBone);
				RefreshBoneChainingUnit(bone._mirrorBone);//<<MeshGroup 빠짐
			}

			Editor.RefreshControllerAndHierarchy(false);
		}

		/// <summary>
		/// Mirror Bone을 만든다.
		/// 이 단계에서는 Mirror Bone 생성, 계층 연결, 기본 속성 복사만 한다.
		/// IK
		/// </summary>
		/// <param name="meshGroup"></param>
		/// <param name="srcBone"></param>
		/// <param name="mirrorParentBone"></param>
		/// <param name="rootSrcBone"></param>
		/// <param name="isRecursive"></param>
		private void MakeNewMirrorBoneHierarchyRecursive(apMeshGroup meshGroup,
														apBone srcBone, apBone mirrorParentBone,
														apBone rootSrcBone,
														bool isRecursive,
														Dictionary<apBone, apBone> src2Mirror,
														Dictionary<apBone, apBone> mirror2Src)
		{
			//새로운 Bone을 만들고
			apBone newBone = AddBone(meshGroup, mirrorParentBone, false);

			src2Mirror.Add(srcBone, newBone);//Pair에 추가
			mirror2Src.Add(newBone, srcBone);

			//1. 이름을 맞춘다.
			//" L " <-> " R "
			string boneName = srcBone._name;
			if (boneName.Contains(" L ")) { boneName = boneName.Replace(" L ", " R "); }
			else if (boneName.Contains(" R ")) { boneName = boneName.Replace(" R ", " L "); }
			else if (boneName.EndsWith(" L")) { boneName = boneName.Replace(" L", " R"); }
			else if (boneName.EndsWith(" R")) { boneName = boneName.Replace(" R", " L"); }
			else if (boneName.StartsWith("L ")) { boneName = boneName.Replace("L ", "R "); }
			else if (boneName.StartsWith("R ")) { boneName = boneName.Replace("R ", "L "); }
			else if (boneName.Contains(" Left")) { boneName = boneName.Replace(" Left", " Right"); }
			else if (boneName.Contains(" Right")) { boneName = boneName.Replace(" Right", " Left"); }
			else if (boneName.Contains("Left ")) { boneName = boneName.Replace("Left ", "Right "); }
			else if (boneName.Contains("Right ")) { boneName = boneName.Replace("Right ", "Left "); }
			else
			{
				if (rootSrcBone._mirrorOption == apBone.MIRROR_OPTION.X) { boneName = boneName + " (Mirror X)"; }
				else { boneName = boneName + " (Mirror Y)"; }
			}

			newBone._name = boneName;

			//2. 기본 속성 복사
			newBone._renderUnit = srcBone._renderUnit;

			//3. Parent와 연결
			if (mirrorParentBone != null)
			{
				newBone._parentBoneID = mirrorParentBone._uniqueID;
				newBone._parentBone = mirrorParentBone;
			}
			else
			{
				newBone._parentBoneID = -1;
				newBone._parentBone = null;
			}

			newBone._childBoneIDs.Clear();
			newBone._childBones.Clear();

			Vector2 mirrorPos = Vector2.zero;
			float mirrorAngle = 0.0f;
			if (srcBone._parentBone == null)
			{
				//Root Bone이라면
				if (srcBone._mirrorOption == apBone.MIRROR_OPTION.X)
				{
					//X 반전 + Offset
					mirrorPos.x = -1 * (srcBone._defaultMatrix._pos.x - srcBone._mirrorCenterOffset) + srcBone._mirrorCenterOffset;
					mirrorPos.y = srcBone._defaultMatrix._pos.y;
					mirrorAngle = -srcBone._defaultMatrix._angleDeg;
				}
				else
				{
					//Y 반전 + Offset
					mirrorPos.y = -1 * (srcBone._defaultMatrix._pos.y - srcBone._mirrorCenterOffset) + srcBone._mirrorCenterOffset;
					mirrorPos.x = srcBone._defaultMatrix._pos.x;
					float srcAngle = apUtil.AngleTo180(srcBone._defaultMatrix._angleDeg);
					if (srcAngle > 0.0f)
					{
						mirrorAngle = apUtil.AngleTo180(180.0f - srcAngle);
					}
					else
					{
						mirrorAngle = apUtil.AngleTo180(-180.0f - srcAngle);
					}

				}
			}
			else
			{
				mirrorPos = new Vector2(-srcBone._defaultMatrix._pos.x, srcBone._defaultMatrix._pos.y);
				mirrorAngle = -srcBone._defaultMatrix._angleDeg;
			}

			newBone._defaultMatrix.SetTRS(mirrorPos,
											mirrorAngle,
											srcBone._defaultMatrix._scale);

			newBone._color = srcBone._color +
				new Color(UnityEngine.Random.Range(-0.2f, 0.2f),
							UnityEngine.Random.Range(-0.2f, 0.2f),
							UnityEngine.Random.Range(-0.2f, 0.2f),
							0.0f);
			newBone._shapeWidth = srcBone._shapeWidth;
			newBone._shapeLength = srcBone._shapeLength;
			newBone._shapeTaper = srcBone._shapeTaper;
			newBone._shapeHelper = srcBone._shapeHelper;

			newBone._optionLocalMove = srcBone._optionLocalMove;
			newBone._optionIK = srcBone._optionIK;
			newBone._isIKTail = srcBone._isIKTail;

			newBone._isIKAngleRange = srcBone._isIKAngleRange;

			//대칭인 경우 이 값이 바뀔 것
			float mirrorRange_A = -srcBone._IKAngleRange_Lower;
			float mirrorRange_B = -srcBone._IKAngleRange_Upper;
			newBone._IKAngleRange_Lower = Mathf.Min(mirrorRange_A, mirrorRange_B);
			newBone._IKAngleRange_Upper = Mathf.Max(mirrorRange_A, mirrorRange_B);
			newBone._IKAnglePreferred = -srcBone._IKAnglePreferred;

			//TODO IK 어쩔..
			//.......
			if (newBone._IKController == null)
			{
				newBone._IKController = new apBoneIKController();
			}
			if (srcBone._IKController == null)
			{
				srcBone._IKController = new apBoneIKController();
			}
			newBone._IKController._controllerType = srcBone._IKController._controllerType;
			newBone._IKController._defaultMixWeight = srcBone._IKController._defaultMixWeight;
			newBone._IKController._isWeightByControlParam = srcBone._IKController._isWeightByControlParam;
			newBone._IKController._weightControlParamID = srcBone._IKController._weightControlParamID;
			newBone._IKController._weightControlParam = srcBone._IKController._weightControlParam;

			newBone._isSocketEnabled = srcBone._isSocketEnabled;

			//MirrorBone 서로 연결
			newBone._mirrorBoneID = srcBone._uniqueID;
			newBone._mirrorBone = srcBone;
			srcBone._mirrorBoneID = newBone._uniqueID;
			srcBone._mirrorBone = newBone;

			//Mirror 옵션은 Root의 것을 이용
			srcBone._mirrorOption = rootSrcBone._mirrorOption;
			newBone._mirrorOption = rootSrcBone._mirrorOption;

			//TODO : _mirrorCenterOffset 계산하기
			//- Root Bone이라면 값 반전
			//- Root Bone이 아니라면 다시 Default 확인해서 다시 계산한다.
			newBone._mirrorCenterOffset = -(srcBone._mirrorCenterOffset);

			if (!isRecursive)
			{
				return;
			}

			//자식 본에 대해서도 작성을 한다.
			if (srcBone._childBones != null && srcBone._childBones.Count > 0)
			{
				for (int i = 0; i < srcBone._childBones.Count; i++)
				{
					apBone childBone = srcBone._childBones[i];

					MakeNewMirrorBoneHierarchyRecursive(meshGroup,
														childBone, newBone,
														rootSrcBone,
														true,
														src2Mirror,
														mirror2Src);
				}
			}

		}

		/// <summary>
		/// Mirror된 Bone 연결 (특히 IK)을 갱신한다.
		/// Mirror가 되면 연결이 바뀐다.
		/// </summary>
		/// <param name="meshGroup"></param>
		/// <param name="mirrorBone"></param>
		/// <param name="mirrorPair"></param>
		/// <param name="srcBones"></param>
		/// <param name="mirrorBones"></param>
		private void SetMirrorBonesLinks(apBone mirrorBone,
											Dictionary<apBone, apBone> src2Mirror,
											Dictionary<apBone, apBone> mirror2Src)
		{
			apBone srcBone = mirror2Src[mirrorBone];

			//IK Target Bone
			mirrorBone._IKTargetBone = GetMirrorBone(srcBone._IKTargetBone, src2Mirror);
			mirrorBone._IKTargetBoneID = (mirrorBone._IKTargetBone != null ? mirrorBone._IKTargetBone._uniqueID : -1);

			//IK Chained Bone
			mirrorBone._IKNextChainedBone = GetMirrorBone(srcBone._IKNextChainedBone, src2Mirror);
			mirrorBone._IKNextChainedBoneID = (mirrorBone._IKNextChainedBone != null ? mirrorBone._IKNextChainedBone._uniqueID : -1);

			//IK Header Bone
			mirrorBone._IKHeaderBone = GetMirrorBone(srcBone._IKHeaderBone, src2Mirror);
			mirrorBone._IKHeaderBoneID = (mirrorBone._IKHeaderBone != null ? mirrorBone._IKHeaderBone._uniqueID : -1);

			//IK Controller
			if (srcBone._IKController == null)
			{
				srcBone._IKController = new apBoneIKController();
			}
			if (mirrorBone._IKController == null)
			{
				mirrorBone._IKController = new apBoneIKController();
			}
			mirrorBone._IKController._effectorBone = GetMirrorBone(srcBone._IKController._effectorBone, src2Mirror);
			mirrorBone._IKController._effectorBoneID = (mirrorBone._IKController._effectorBone != null ? mirrorBone._IKController._effectorBone._uniqueID : -1);

			if (mirrorBone._childBones != null && mirrorBone._childBones.Count > 0)
			{
				for (int i = 0; i < mirrorBone._childBones.Count; i++)
				{
					SetMirrorBonesLinks(mirrorBone._childBones[i],
											src2Mirror,
											mirror2Src);
				}
			}
		}

		private apBone GetMirrorBone(apBone srcBone, Dictionary<apBone, apBone> src2Mirror)
		{
			if (srcBone == null) { return null; }
			if (src2Mirror.ContainsKey(srcBone)) { return src2Mirror[srcBone]; }
			return null;
		}



		public void DuplicateBone(apMeshGroup meshGroup, apBone srcBone, float offsetX, float offsetY, bool isDuplicateChildren)
		{
			if(meshGroup == null || srcBone == null)
			{
				return;
			}

			//Undo
			apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_AddBone, Editor, meshGroup, null, false, false);

			//1. 일단 본들을 생성하고, Src>Copied를 연결한다.
			//2. IK 설정도 연결한다.

			//World Matrix에서 위치를 보정할 것이므로, 필요한 행렬 변수를 가져오자
			apBone parentBone = srcBone._parentBone;
			apMatrix default2WorldMatrix = null;
			if(parentBone != null)
			{
				parentBone.MakeWorldMatrix(true);
				default2WorldMatrix = new apMatrix(parentBone._worldMatrix_NonModified);
			}
			else
			{
				srcBone.MakeWorldMatrix(true);
				if(srcBone._renderUnit != null)
				{
					default2WorldMatrix = new apMatrix(srcBone._renderUnit.WorldMatrixWrapWithoutModified);
				}
				else
				{
					default2WorldMatrix = new apMatrix();
					default2WorldMatrix.SetIdentity();
				}
			}



			Dictionary<apBone, apBone> Src2Dst = new Dictionary<apBone, apBone>();
			Dictionary<apBone, apBone> Dst2Src = new Dictionary<apBone, apBone>();


			//기본 한개의 본을 만들고, 옵션에 따라 Recursive하게 만든다.
			//일단 만들고 속성 복사
			//- IK 설정은 이후에 일괄적으로 연결한다.
			//- Mirror 속성은 복사되지 않는다.
			apBone copiedBone = AddBone(meshGroup, parentBone, false);

			copiedBone._name = srcBone._name + " (Copied)";

			//위치는 World 좌표계로 Offset을 줘야한다.
			apMatrix targetMatrix = new apMatrix(srcBone._worldMatrix_NonModified);
			targetMatrix._pos.x += offsetX;
			targetMatrix._pos.y += offsetY;
			targetMatrix.RInverse(default2WorldMatrix);
			

			copiedBone._defaultMatrix = new apMatrix(targetMatrix);
			//copiedBone._defaultMatrix._pos += new Vector2(offsetX, offsetY);

			copiedBone._color = srcBone._color;
			copiedBone._shapeWidth = srcBone._shapeWidth;
			copiedBone._shapeLength = srcBone._shapeLength;
			copiedBone._shapeTaper = srcBone._shapeTaper;

			copiedBone._shapeHelper = srcBone._shapeHelper;

			copiedBone._isSocketEnabled = srcBone._isSocketEnabled;

			//매핑
			Src2Dst.Add(srcBone, copiedBone);
			Dst2Src.Add(copiedBone, srcBone);

			
			if(isDuplicateChildren)
			{
				//자식들도 복사해주자
				DuplicateBonesRecursive(copiedBone, srcBone, meshGroup, /*offsetX, offsetY, */Src2Dst, Dst2Src);
			}

			//모두 복사가 끝났다면, IK설정을 복사한다.
			foreach (KeyValuePair<apBone, apBone> Src2DstPair in Src2Dst)
			{
				apBone src = Src2DstPair.Key;
				apBone dst = Src2DstPair.Value;

				//IK 설정을 복사하자.
				dst._optionLocalMove = src._optionLocalMove;
				dst._optionIK = src._optionIK;

				dst._isIKTail = src._isIKTail;

				dst._isIKAngleRange = src._isIKAngleRange;
				dst._IKAngleRange_Lower = src._IKAngleRange_Lower;
				dst._IKAngleRange_Upper = src._IKAngleRange_Upper;
				dst._IKAnglePreferred = src._IKAnglePreferred;

				apBone dstIKTargetBone = null;
				if(src._IKTargetBoneID >= 0 && src._IKTargetBone != null)
				{
					//IK 설정이 복사가 될지 확인하자.
					dstIKTargetBone = null;
					if(Src2Dst.ContainsKey(src._IKTargetBone))
					{
						//IK Target과 대칭되는 복제된 본
						dstIKTargetBone = Src2Dst[src._IKTargetBone];
					}
				}
				
				if(dstIKTargetBone != null)
				{
					//같이 복제된 IK 타겟이 있다.
					dst._IKTargetBoneID = dstIKTargetBone._uniqueID;
					dst._IKTargetBone = dstIKTargetBone;

					//그 외의 본도 복사하자. (없으면 일단 생략)
					apBone dstIKNextChainedBone = null;
					apBone dstIKHeaderBone = null;

					if(src._IKNextChainedBone != null)
					{
						if(Src2Dst.ContainsKey(src._IKNextChainedBone))
						{
							dstIKNextChainedBone = Src2Dst[src._IKNextChainedBone];
						}
					}

					if(src._IKHeaderBone != null)
					{
						if(Src2Dst.ContainsKey(src._IKHeaderBone))
						{
							dstIKHeaderBone = Src2Dst[src._IKHeaderBone];
						}
					}

					if(dstIKNextChainedBone != null)
					{
						dst._IKNextChainedBoneID = dstIKNextChainedBone._uniqueID;
						dst._IKNextChainedBone = dstIKNextChainedBone;
					}
					else
					{
						dst._IKNextChainedBoneID = -1;
						dst._IKNextChainedBone = null;
					}

					if(dstIKHeaderBone != null)
					{
						dst._IKHeaderBoneID = dstIKHeaderBone._uniqueID;
						dst._IKHeaderBone = dstIKHeaderBone;
					}
					else
					{
						dst._IKHeaderBoneID = -1;
						dst._IKHeaderBone = null;
					}
				}
				else
				{
					//연결된 IK 타겟이 없다.
					dst._IKTargetBoneID = -1;
					dst._IKTargetBone = null;

					dst._IKNextChainedBoneID = -1;
					dst._IKNextChainedBone = null;

					dst._IKHeaderBoneID = -1;
					dst._IKHeaderBone = null;
				}

				//IK Controller에 대해서도 설정하자
				//이건 복제된게 없다면 그대로 가도 상관없다.
				if(dst._IKController == null)
				{
					dst._IKController = new apBoneIKController();
				}
				if(src._IKController != null)
				{
					apBoneIKController srcIKCont = src._IKController;
					apBoneIKController dstIKCont = dst._IKController;

					dstIKCont._controllerType = srcIKCont._controllerType;
					dstIKCont._parentBone = dst;

					dstIKCont._effectorBoneID = srcIKCont._effectorBoneID;
					dstIKCont._effectorBone = srcIKCont._effectorBone;

					dstIKCont._defaultMixWeight = srcIKCont._defaultMixWeight;

					dstIKCont._isWeightByControlParam = srcIKCont._isWeightByControlParam;
					dstIKCont._weightControlParamID = srcIKCont._weightControlParamID;
					dstIKCont._weightControlParam = srcIKCont._weightControlParam;
				}
			}
			
			Editor._portrait.LinkAndRefreshInEditor(false, meshGroup);
			meshGroup.LinkBoneListToChildMeshGroupsAndRenderUnits();
			meshGroup.RefreshForce(true);
			meshGroup.UpdateBonesWorldMatrix();
			meshGroup.RefreshBoneGUIVisible();

			RefreshBoneChaining(meshGroup);
			RefreshBoneHierarchy(meshGroup);

			Editor.RefreshControllerAndHierarchy(false);

		}
		private void DuplicateBonesRecursive(apBone parentCopiedBone, apBone parentSrcBone, 
											apMeshGroup meshGroup,
											//float offsetX, float offsetY, 
											Dictionary<apBone, apBone> Src2Dst, Dictionary<apBone, apBone> Dst2Src)
		{
			//연결시킬 자식 본을 하나 꺼내자
			if(parentSrcBone._childBones == null && parentSrcBone._childBones.Count == 0)
			{
				return;
			}

			for (int iChild = 0; iChild < parentSrcBone._childBones.Count; iChild++)
			{
				apBone childSrcBone = parentSrcBone._childBones[iChild];

				//자식에 대한 복제 본을 만들자
				apBone childCopiedBone = AddBone(meshGroup, parentCopiedBone, false);

				childCopiedBone._name = childSrcBone._name + " (Copied)";
				childCopiedBone._defaultMatrix = new apMatrix(childSrcBone._defaultMatrix);
				//childCopiedBone._defaultMatrix._pos += new Vector2(offsetX, offsetY);

				childCopiedBone._color = childSrcBone._color;
				childCopiedBone._shapeWidth = childSrcBone._shapeWidth;
				childCopiedBone._shapeLength = childSrcBone._shapeLength;
				childCopiedBone._shapeTaper = childSrcBone._shapeTaper;

				childCopiedBone._shapeHelper = childSrcBone._shapeHelper;

				childCopiedBone._isSocketEnabled = childSrcBone._isSocketEnabled;

				//매핑
				Src2Dst.Add(childSrcBone, childCopiedBone);
				Dst2Src.Add(childCopiedBone, childSrcBone);

				//새로운 본에 대한 자식 본을 계속 만들자
				DuplicateBonesRecursive(childCopiedBone, childSrcBone, meshGroup, /*offsetX, offsetY, */Src2Dst, Dst2Src);
			}
		}

		public void SnapBoneEndToChildBone(apBone srcBone, apBone childBone, apMeshGroup meshGroup)
		{
			if(meshGroup == null || srcBone == null || childBone == null || srcBone == childBone)
			{
				return;
			}

			if(srcBone._childBones == null)
			{
				return;
			}
			if(!srcBone._childBones.Contains(childBone))
			{
				return;
			}

			//Undo
			apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_BoneSettingChanged, Editor, meshGroup, null, false, false);

			//World Matrix에서 위치를 보정할 것이므로, 필요한 행렬 변수를 가져오자
			apBone parentBone = srcBone._parentBone;
			apMatrix default2WorldMatrix = null;
			if(parentBone != null)
			{
				parentBone.MakeWorldMatrix(true);
				default2WorldMatrix = new apMatrix(parentBone._worldMatrix_NonModified);
			}
			else
			{
				srcBone.MakeWorldMatrix(true);
				if(srcBone._renderUnit != null)
				{
					default2WorldMatrix = new apMatrix(srcBone._renderUnit.WorldMatrixWrapWithoutModified);
				}
				else
				{
					default2WorldMatrix = new apMatrix();
					default2WorldMatrix.SetIdentity();
				}
			}

			//회전과 ShapeLegth를 조절하자.
			apMatrix srcWorldMatrix_Prev = new apMatrix(srcBone._worldMatrix_NonModified);
			apMatrix targetChildWorldMatrix = childBone._worldMatrix_NonModified;
			float dist2Target = Vector2.Distance(targetChildWorldMatrix._pos, srcWorldMatrix_Prev._pos);

			//일단 길이부터 바꾸자.
			srcBone._shapeLength = (int)(dist2Target + 0.5f);
			
			//길이가 0 이상이라면
			if(srcBone._shapeLength > 0)
			{
				//각도도 바꾸고, 다른 Child의 Default Matrix도 모두 바꿔야 한다.
				float angle2Child = Mathf.Atan2(targetChildWorldMatrix._pos.y - srcWorldMatrix_Prev._pos.y, targetChildWorldMatrix._pos.x - srcWorldMatrix_Prev._pos.x);
				angle2Child *= Mathf.Rad2Deg;
				angle2Child -= 90.0f;
				
				while(angle2Child < -180.0f)
				{
					angle2Child += 360.0f;
				}

				while(angle2Child > 180.0f)
				{
					angle2Child -= 360.0f;
				}

				apMatrix srcWorldMatrix_Next = new apMatrix(srcWorldMatrix_Prev);
				srcWorldMatrix_Next._angleDeg = angle2Child;
				srcWorldMatrix_Next.MakeMatrix(true);

				//World(next) > Default로 변환하자.
				apMatrix srcDefaultMatrix_Next = new apMatrix(srcWorldMatrix_Next);
				srcDefaultMatrix_Next.RInverse(default2WorldMatrix);

				

				//자식 본들의 Default Matrix도 바꾸자
				apMatrix childMatrix_Next = new apMatrix();

				for (int iChild = 0; iChild < srcBone._childBones.Count; iChild++)
				{
					apBone curChildBone = srcBone._childBones[iChild];

					childMatrix_Next.SetMatrix(curChildBone._worldMatrix_NonModified);//World는 유지
					//Child의 Default->World Matrix는 Parent인 srcBone의 World Matrix이다.

					childMatrix_Next.RInverse(srcWorldMatrix_Next);//Default2World가 바뀐 것 (Next로)

					//바뀐 Default 적용
					curChildBone._defaultMatrix.SetMatrix(childMatrix_Next);
				}

				//변경된 Default Matrix를 적용
				srcBone._defaultMatrix.SetMatrix(srcDefaultMatrix_Next);

				//다시 연산
				srcBone.MakeWorldMatrix(true);
				
			}
			
			srcBone.GUIUpdate(true, false);

			meshGroup.RefreshForce(true);
			meshGroup.UpdateBonesWorldMatrix();
			
			RefreshBoneChaining(meshGroup);
		}
		

		//---------------------------------------------------------------------------------------


		/// <summary>
		/// 해당 MeshGroup의 본들의 계층 연결 관계를 다시 갱신한다.
		/// IK Chaining은 호출되지 않으므로 별도로 (RefreshBoneChaining)를 호출하자
		/// 이 함수는 Link 이후에 호출해주자
		/// </summary>
		/// <param name="meshGroup"></param>
		public void RefreshBoneHierarchy(apMeshGroup meshGroup)
		{
			if (meshGroup == null)
			{
				return;
			}

			//<BONE_EDIT>
			//for (int i = 0; i < meshGroup._boneList_Root.Count; i++)
			//{
			//	//Root 부터 재귀적으로 호출한다.
			//	RefreshBoneHierarchyUnit(meshGroup, meshGroup._boneList_Root[i], null);
			//}

			//if (meshGroup._childMeshGroupTransformsWithBones.Count > 0)
			//{
			//	for (int i = 0; i < meshGroup._childMeshGroupTransformsWithBones.Count; i++)
			//	{
			//		apTransform_MeshGroup meshGroupTransform = meshGroup._childMeshGroupTransformsWithBones[i];
			//		if (meshGroupTransform._meshGroup != null)
			//		{
			//			RefreshBoneHierarchy(meshGroupTransform._meshGroup);
			//		}
			//	}
			//}

			//>> Bone Set으로 변경
			apMeshGroup.BoneListSet boneSet = null;
			for (int iSet = 0; iSet < meshGroup._boneListSets.Count; iSet++)
			{
				boneSet = meshGroup._boneListSets[iSet];
				for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)
				{
					RefreshBoneHierarchyUnit(boneSet._bones_Root[iRoot], null);
				}
			}

			meshGroup.RefreshBoneGUIVisible();
		}


		//private void RefreshBoneHierarchyUnit(apMeshGroup meshGroup, apBone bone, apBone parentBone)
		private void RefreshBoneHierarchyUnit(apBone bone, apBone parentBone)//<<MeshGroup 필요 없음
		{
			if (bone == null)
			{
				return;
			}
			if (parentBone == null)
			{
				bone._parentBone = null;
				bone._parentBoneID = -1;
			}
			else
			{
				bone._parentBone = parentBone;
				bone._parentBoneID = parentBone._uniqueID;
			}

			bone._childBoneIDs.Clear();//ID 리스트는 일단 Clear
			bone._childBones.RemoveAll(delegate (apBone a)
			{
				return a == null;
			});

			for (int i = 0; i < bone._childBones.Count; i++)
			{
				apBone childBone = bone._childBones[i];
				//ID 연결해주고..
				bone._childBoneIDs.Add(childBone._uniqueID);

				//계층적으로 호출하며 이어가자
				//RefreshBoneHierarchyUnit(meshGroup, childBone, bone);
				RefreshBoneHierarchyUnit(childBone, bone);

			}
		}


		/// <summary>
		/// 해당 본의 IK를 포함한 Chain 갱신을 한다.
		/// 기본 Link이후에 수행해야한다.
		/// IK 설정을 변경하면 한번씩 호출하자
		/// 초기화 이후에도 호출
		/// </summary>
		/// <param name="meshGroup"></param>
		public void RefreshBoneChaining(apMeshGroup meshGroup)
		{
			if (meshGroup == null)
			{
				return;
			}

			//<BONE_EDIT>
			//for (int i = 0; i < meshGroup._boneList_Root.Count; i++)
			//{
			//	//Root 부터 재귀적으로 호출한다.
			//	RefreshBoneChainingUnit(meshGroup, meshGroup._boneList_Root[i]);
			//}

			////추가
			////내부적으로도 BoneChaining을 다시 연결해주자
			//for (int i = 0; i < meshGroup._boneList_Root.Count; i++)
			//{
			//	meshGroup._boneList_Root[i].LinkBoneChaining();
			//}

			////추가 : Validation도 체크한다.
			//for (int i = 0; i < meshGroup._boneList_Root.Count; i++)
			//{
			//	//Root 부터 재귀적으로 호출한다. 
			//	RefreshBoneIKControllerValidationRecursive(meshGroup, meshGroup._boneList_Root[i]);
			//}

			//if (meshGroup._childMeshGroupTransformsWithBones.Count > 0)
			//{
			//	for (int i = 0; i < meshGroup._childMeshGroupTransformsWithBones.Count; i++)
			//	{
			//		apTransform_MeshGroup meshGroupTransform = meshGroup._childMeshGroupTransformsWithBones[i];
			//		if (meshGroupTransform._meshGroup != null)
			//		{
			//			RefreshBoneChaining(meshGroupTransform._meshGroup);
			//		}
			//	}
			//}

			//>> Bone Set으로 통합
			apMeshGroup.BoneListSet boneSet = null;
			for (int iSet = 0; iSet < meshGroup._boneListSets.Count; iSet++)
			{
				boneSet = meshGroup._boneListSets[iSet];
				for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)
				{
					//Root 부터 재귀적으로 호출한다.
					RefreshBoneChainingUnit(boneSet._bones_Root[iRoot]);
				}

				//내부적으로도 BoneChaining을 다시 연결해주자
				for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)
				{
					boneSet._bones_Root[iRoot].LinkBoneChaining();
				}

				//추가 : Validation도 체크한다.
				for (int iRoot = 0; iRoot < boneSet._bones_Root.Count; iRoot++)
				{
					RefreshBoneIKControllerValidationRecursive(boneSet._meshGroup, boneSet._bones_Root[iRoot]);
				}

			}


		}

		//private void RefreshBoneChainingUnit(apMeshGroup meshGroup, apBone bone)
		private void RefreshBoneChainingUnit(apBone bone)//<<MeshGroup이 필요없다.
		{
			if (bone == null)
			{
				return;
			}

			//1. Parent의 값에 따라서 IK Tail / IK Chained 처리를 갱신한다.
			bool isLocalMovable = false;
			if (bone._parentBone != null)
			{
				//Parent의 IK 옵션에 따라서 Tail 처리를 한다.
				switch (bone._parentBone._optionIK)
				{
					case apBone.OPTION_IK.Disabled:
						//Parent의 IK가 꺼져있다.
						//Chained라면 해제해준다.
						if (bone._optionIK == apBone.OPTION_IK.IKChained)
						{
							bone._optionIK = apBone.OPTION_IK.IKSingle;
							bone._IKTargetBone = null;
							bone._IKTargetBoneID = -1;

							bone._IKNextChainedBone = null;
							bone._IKNextChainedBoneID = -1;
						}

						bone._isIKTail = false;
						bone._IKHeaderBone = null;
						bone._IKHeaderBoneID = -1;
						isLocalMovable = true;
						break;

					case apBone.OPTION_IK.IKHead:
					case apBone.OPTION_IK.IKSingle:
					case apBone.OPTION_IK.IKChained:
						{
							//1) Parent가 자신을 타겟으로 삼고 있다면 Tail 처리
							//2) Parent가 자신의 자식을 타겟으로 삼고 있다면 Chained + Tail 처리
							//3) Parent가 자신 또는 자신의 자식을 타겟으로 삼고있지 않다면 IK 타겟이 아니다.
							int IKTargetBoneID = bone._parentBone._IKTargetBoneID;
							apBone IKTargetBone = bone._parentBone._IKTargetBone;
							int IKNextChainedBoneID = bone._parentBone._IKNextChainedBoneID;

							if (bone._uniqueID == IKTargetBoneID)
							{
								//1) 자신을 타겟으로 삼고 있는 경우
								//자신이 Chained였다면 이를 풀어줘야 한다.
								if (bone._optionIK == apBone.OPTION_IK.IKChained)
								{
									bone._optionIK = apBone.OPTION_IK.IKSingle;
									bone._IKTargetBone = null;
									bone._IKTargetBoneID = -1;

									bone._IKNextChainedBone = null;
									bone._IKNextChainedBoneID = -1;
								}

								bone._isIKTail = true;

								//bone._IKHeaderBone = bone._parentBone;
								//bone._IKHeaderBoneID = bone._parentBone._uniqueID;
								if (bone._parentBone._optionIK == apBone.OPTION_IK.IKHead
									|| bone._parentBone._optionIK == apBone.OPTION_IK.IKSingle)
								{
									bone._IKHeaderBone = bone._parentBone;
									bone._IKHeaderBoneID = bone._parentBone._uniqueID;
								}
								else
								{
									bone._IKHeaderBone = bone._parentBone._IKHeaderBone;
									bone._IKHeaderBoneID = bone._parentBone._IKHeaderBoneID;
								}
							}
							else if (bone._uniqueID == IKNextChainedBoneID)
							{
								//2) Parent가 자신의 자식을 타겟으로 삼고 있다면 Chained + Tail 처리
								bone._optionIK = apBone.OPTION_IK.IKChained;
								bone._isIKTail = true;

								//Parent가 Header로 삼고있는 Bone을 Header로 연결하여 공유한다.
								if (bone._parentBone._optionIK == apBone.OPTION_IK.IKHead
									|| bone._parentBone._optionIK == apBone.OPTION_IK.IKSingle)
								{
									bone._IKHeaderBone = bone._parentBone;
									bone._IKHeaderBoneID = bone._parentBone._uniqueID;
								}
								else
								{
									bone._IKHeaderBone = bone._parentBone._IKHeaderBone;
									bone._IKHeaderBoneID = bone._parentBone._IKHeaderBoneID;
								}


								if (bone._IKHeaderBone == null)
								{
									Debug.LogError("Bone Chaining Error : Header를 찾을 수 없다. [" + bone._parentBone._name + " -> " + bone._name + "]");
								}
								else
								{
									//Debug.Log("Chained : " + bone._IKHeaderBone._name + " >> " + bone._parentBone._name + " >> " + bone._name);
								}


								bone._IKNextChainedBone = bone.FindNextChainedBone(IKTargetBoneID);
								if (bone._IKNextChainedBone == null)
								{
									bone._IKNextChainedBoneID = -1;
									Debug.LogError("Bone Chaining Error : IK Chained가 이어지지 않았다. [" + bone._parentBone._name + " -> " + bone._name + " -> (끊김) -> " + IKTargetBone._name);
								}
								else
								{
									bone._IKNextChainedBoneID = bone._IKNextChainedBone._uniqueID;
								}

								//타겟을 공유한다.
								bone._IKTargetBone = IKTargetBone;
								bone._IKTargetBoneID = IKTargetBoneID;


							}
							else
							{
								//3) Parent가 자신 또는 자신의 자식을 타겟으로 삼고있지 않다면 IK 타겟이 아니다.
								//IK Chain이 끊겼다.

								if (bone._optionIK == apBone.OPTION_IK.IKChained)
								{
									bone._optionIK = apBone.OPTION_IK.IKSingle;
									bone._IKTargetBone = null;
									bone._IKTargetBoneID = -1;

									bone._IKNextChainedBone = null;
									bone._IKNextChainedBoneID = -1;
								}

								bone._isIKTail = false;
								bone._IKHeaderBone = null;
								bone._IKHeaderBoneID = -1;
								isLocalMovable = true;
							}
						}
						break;
				}
			}
			else
			{
				bone._isIKTail = false;
				bone._IKHeaderBone = null;
				bone._IKHeaderBoneID = -1;

				isLocalMovable = true;
			}


			//2. Child로의 IK 설정에 따라서 이어지는 Chain 처리를 한다.

			switch (bone._optionIK)
			{
				case apBone.OPTION_IK.Disabled:
					{
						//IK가 꺼져있으니 값을 날리자
						bone._IKTargetBoneID = -1;
						bone._IKTargetBone = null;

						bone._IKNextChainedBoneID = -1;
						bone._IKNextChainedBone = null;
					}
					break;

				case apBone.OPTION_IK.IKChained:
					//Chain 처리는 위의 Parent 처리에서 연동해서 이미 수행했다.
					break;

				case apBone.OPTION_IK.IKHead:
					{
						int targetIKBoneID = bone._IKTargetBoneID;
						int nextChainedBoneID = bone._IKNextChainedBoneID;

						//갱신 작업이 필요한지 체크
						bool isRefreshNeed = false;
						if (bone._IKTargetBone == null || bone._IKNextChainedBone == null)
						{
							//ID는 있는데 연결이 안되었네요
							//다시 연결 필요
							isRefreshNeed = true;
						}
						else
						{
							//검색속도가 빠른 -> Parent로의 함수를 이용하여 유효한 링크인지 판단한다.
							if (bone._IKTargetBone.GetParentRecursive(bone._uniqueID) == null
								|| bone._IKNextChainedBone.GetParentRecursive(bone._uniqueID) == null)
							{
								isRefreshNeed = true;
							}
						}

						if (isRefreshNeed)
						{
							//Target을 기준으로 ID와 레퍼런스 연동을 하자
							apBone targetBone = bone.GetChildBoneRecursive(targetIKBoneID);
							apBone nextChainedBone = bone.FindNextChainedBone(nextChainedBoneID);

							if (targetBone == null || nextChainedBone == null)
							{
								//못찾았네요...
								//Debug.LogError("Bone Chaining Error : IK Header가 적절한 타겟을 찾지 못했다. [" + bone._name + "] > IK 해제됨");

								//에러로 인해 초기화 할때는
								//Child Bone이 1개라면 Single로 초기화
								//Child Bone이 여러개라면 Disabled
								if (bone._childBones.Count == 1 && bone._childBones[0] != null)
								{
									//IKSingle로 초기화하자
									apBone childBone = bone._childBones[0];

									bone._IKTargetBoneID = childBone._uniqueID;
									bone._IKTargetBone = childBone;

									bone._IKNextChainedBoneID = childBone._uniqueID;
									bone._IKNextChainedBone = childBone;

									bone._optionIK = apBone.OPTION_IK.IKSingle;
								}
								else
								{
									//Disabled로 초기화하자
									bone._IKTargetBoneID = -1;
									bone._IKTargetBone = null;

									bone._IKNextChainedBoneID = -1;
									bone._IKNextChainedBone = null;

									bone._optionIK = apBone.OPTION_IK.Disabled;
								}
							}
							else
							{
								//타겟이 있다. 마저 연결하자
								bone._IKTargetBoneID = targetIKBoneID;
								bone._IKTargetBone = targetBone;

								bone._IKNextChainedBoneID = nextChainedBoneID;
								bone._IKNextChainedBone = nextChainedBone;
							}
						}
					}
					break;

				case apBone.OPTION_IK.IKSingle:
					{
						//연결이 유효하면 -> 지속
						//연결이 유효하지 않으면 무조건 Disabled로 바꾼다.
						//자동 연결은 하지 말자

						int targetIKBoneID = bone._IKTargetBoneID;
						int nextChainedBoneID = bone._IKNextChainedBoneID;

						//갱신 작업이 필요한지 체크
						bool isRefreshNeed = false;
						if (bone._IKTargetBone == null || bone._IKNextChainedBone == null)
						{
							//ID는 있는데 연결이 안되었네요
							//다시 연결 필요
							isRefreshNeed = true;
						}
						else
						{
							//Parent/Child 연결 관계가 유효한가
							if (bone._IKTargetBone._parentBone != bone
								|| bone._IKNextChainedBone._parentBone != bone)
							{
								//직접 연결이 안되어있다.
								isRefreshNeed = true;
							}
						}

						if (isRefreshNeed)
						{
							apBone targetBone = bone.GetChildBone(targetIKBoneID);
							apBone nextChainedBone = bone.GetChildBone(targetIKBoneID);

							bool isInvalid = false;
							if (targetBone == null || nextChainedBone == null || targetBone != nextChainedBone)
							{
								isInvalid = true;
							}

							if (!isInvalid)
							{
								//유효한 연결
								bone._IKTargetBoneID = targetIKBoneID;
								bone._IKTargetBone = targetBone;

								bone._IKNextChainedBoneID = nextChainedBoneID;
								bone._IKNextChainedBone = nextChainedBone;
							}
							else
							{
								//유효하지 않은 연결
								//Disabled로 바꾸자
								bone._IKTargetBoneID = -1;
								bone._IKTargetBone = null;

								bone._IKNextChainedBoneID = -1;
								bone._IKNextChainedBone = null;

								bone._optionIK = apBone.OPTION_IK.Disabled;
							}

						}
					}
					break;
			}

			//3. IK 값에 따라서 Local Move 처리를 확인한다.
			if (!isLocalMovable)
			{
				if (bone._optionLocalMove == apBone.OPTION_LOCAL_MOVE.Enabled)
				{
					//IK Tail로 세팅된 상태라면 LocalMove는 불가능하다
					bone._optionLocalMove = apBone.OPTION_LOCAL_MOVE.Disabled;
				}
			}

			//4. Child도 Bone 체크를 하자
			for (int i = 0; i < bone._childBones.Count; i++)
			{
				//RefreshBoneChainingUnit(meshGroup, bone._childBones[i]);
				RefreshBoneChainingUnit(bone._childBones[i]);
			}

		}

		/// <summary>
		/// Bone Chaining 체크할때 같이 실행되는 함수
		/// Position/LookAt Controller가 설정되어 있을 때, 각각의 Bone의 설정들이 유효한지 확인한다.
		/// </summary>
		/// <param name="meshGroup"></param>
		/// <param name="bone"></param>
		private void RefreshBoneIKControllerValidationRecursive(apMeshGroup meshGroup, apBone bone)
		{
			if (bone == null)
			{
				return;
			}


			if (bone._IKController == null)
			{
				bone._IKController = new apBoneIKController();
				bone._IKController.Link(bone, meshGroup, Editor._portrait);
			}

			bone._IKController.CheckValidation();

			for (int i = 0; i < bone._childBones.Count; i++)
			{
				RefreshBoneIKControllerValidationRecursive(meshGroup, bone._childBones[i]);
			}
		}

		//-----------------------------------------------------------------------------
		// 본 리타겟
		//-----------------------------------------------------------------------------
		public void ImportBonesFromRetargetBaseFile(apMeshGroup targetMeshGroup, apRetarget retarget)
		{
			if (Editor.Select.MeshGroup == null
				|| Editor.Select.MeshGroup != targetMeshGroup
				|| !retarget.IsBaseFileLoaded)
			{
				return;
			}

			List<apRetargetBoneUnit> importBoneUnits = new List<apRetargetBoneUnit>();
			//Import되는 것만 가져오자
			for (int i = 0; i < retarget.BaseBoneUnits.Count; i++)
			{
				apRetargetBoneUnit boneUnit = retarget.BaseBoneUnits[i];
				if (boneUnit._isImportEnabled)
				{
					importBoneUnits.Add(boneUnit);
				}
			}

			if (importBoneUnits.Count == 0)
			{
				//Import 할게 없네용?
				return;
			}

			float importScale = retarget._importScale;

			//Undo
			apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_AddBone, Editor, targetMeshGroup, null, false, false);
			//Unit ID -> Bone ID(새로 생성) 으로 연결하는 Map을 만들자
			Dictionary<int, int> unitID2BoneID = new Dictionary<int, int>();

			for (int i = 0; i < importBoneUnits.Count; i++)
			{
				apRetargetBoneUnit boneUnit = importBoneUnits[i];
				int newUniqueBoneID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.Bone);
				if (newUniqueBoneID < 0)
				{
					//ID 발급에 실패했다.
					EditorUtility.DisplayDialog(Editor.GetText(TEXT.BoneAddFailed_Title),
												Editor.GetText(TEXT.BoneAddFailed_Body),
												Editor.GetText(TEXT.Close));
					return;
				}
				unitID2BoneID.Add(boneUnit._unitID, newUniqueBoneID);
				boneUnit._boneUniqueID = newUniqueBoneID;//<<여기에 새로 발급한 ID를 직접 넣어주자
			}

			//Bone을 일일이 만들어주자
			//Link는 몰아서 나중에 하자
			List<apBone> addedBoneList = new List<apBone>();

			for (int i = 0; i < importBoneUnits.Count; i++)
			{
				apRetargetBoneUnit boneUnit = importBoneUnits[i];
				apBone newBone = new apBone(boneUnit._boneUniqueID, targetMeshGroup._uniqueID, boneUnit._name);

				newBone.InitTransform();

				newBone._parentBoneID = (unitID2BoneID.ContainsKey(boneUnit._parentUnitID) ? unitID2BoneID[boneUnit._parentUnitID] : -1);
				newBone._level = boneUnit._level;

				newBone._childBoneIDs.Clear();

				if (boneUnit._childUnitID != null && boneUnit._childUnitID.Count > 0)
				{
					for (int iChild = 0; iChild < boneUnit._childUnitID.Count; iChild++)
					{
						int childUnitID = boneUnit._childUnitID[iChild];
						int childBoneID = unitID2BoneID.ContainsKey(childUnitID) ? unitID2BoneID[childUnitID] : -1;
						if (childBoneID >= 0)
						{
							newBone._childBoneIDs.Add(childBoneID);
						}
					}
				}

				newBone._defaultMatrix.SetMatrix(boneUnit._defaultMatrix);
				newBone._defaultMatrix._pos *= importScale;
				newBone._defaultMatrix._angleDeg = apUtil.AngleTo180(newBone._defaultMatrix._angleDeg);
				newBone._defaultMatrix.MakeMatrix();

				if (boneUnit._isShapeEnabled)
				{
					//Shape를 적용한다면..
					newBone._color = boneUnit._color;
					newBone._shapeWidth = (int)(boneUnit._shapeWidth * importScale + 0.5f);
					newBone._shapeLength = (int)(boneUnit._shapeLength * importScale + 0.5f);
					newBone._shapeTaper = boneUnit._shapeTaper;
				}

				if (boneUnit._isIKEnabled)
				{
					//IK를 적용한다면
					newBone._optionIK = boneUnit._optionIK;
					newBone._isIKTail = boneUnit._isIKTail;
					newBone._IKTargetBoneID = unitID2BoneID.ContainsKey(boneUnit._IKTargetBoneUnitID) ? unitID2BoneID[boneUnit._IKTargetBoneUnitID] : -1;
					newBone._IKNextChainedBoneID = unitID2BoneID.ContainsKey(boneUnit._IKNextChainedBoneUnitID) ? unitID2BoneID[boneUnit._IKNextChainedBoneUnitID] : -1;
					newBone._IKHeaderBoneID = unitID2BoneID.ContainsKey(boneUnit._IKHeaderBoneUnitID) ? unitID2BoneID[boneUnit._IKHeaderBoneUnitID] : -1;

					newBone._isIKAngleRange = boneUnit._isIKAngleRange;
					newBone._IKAngleRange_Lower = boneUnit._IKAngleRange_Lower;
					newBone._IKAngleRange_Upper = boneUnit._IKAngleRange_Upper;
					newBone._IKAnglePreferred = boneUnit._IKAnglePreferred;
				}

				newBone._isSocketEnabled = boneUnit._isSocketEnabled;

				//일단 전체 리스트에 넣자
				targetMeshGroup._boneList_All.Add(newBone);

				addedBoneList.Add(newBone);
			}

			//추가되었던 Bone을 Link한다.
			for (int i = 0; i < addedBoneList.Count; i++)
			{
				apBone bone = addedBoneList[i];

				apBone parentBone = targetMeshGroup.GetBone(bone._parentBoneID);
				if (parentBone == null)
				{
					//Parent가 없으면 Root에 추가
					targetMeshGroup._boneList_Root.Add(bone);
				}

				bone.Link(targetMeshGroup, parentBone, Editor._portrait);
				bone.MakeWorldMatrix(false);
				bone.GUIUpdate();

			}

			//Bone IK를 갱신한다.
			RefreshBoneChaining(targetMeshGroup);

			//GUI를 업데이트한다.
			targetMeshGroup.UpdateBonesWorldMatrix();//<<변경
			for (int i = 0; i < targetMeshGroup._boneList_Root.Count; i++)
			{
				//targetMeshGroup._boneList_Root[i].MakeWorldMatrix(true);//이전
				targetMeshGroup._boneList_Root[i].GUIUpdate(true);
			}

			Editor._portrait.LinkAndRefreshInEditor(false, targetMeshGroup);
			Editor.RefreshControllerAndHierarchy(false);

			Editor.Notification("Bones are loaded from file", true, false);

		}



		/// <summary>
		/// Retarget 파일로부터 "단일 포즈"를 Import한다.
		/// 대상은 Modifier
		/// </summary>
		public void ImportBonePoseFromRetargetSinglePoseFileToModifier(apMeshGroup targetMeshGroup, apRetarget retarget, apModifierBase targetModifier, apModifierParamSet paramSet,
																		apDialog_RetargetSinglePoseImport.IMPORT_METHOD importMethod)
		{
			if (Editor._portrait == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.MeshGroup != targetMeshGroup
				|| Editor.Select.Modifier != targetModifier
				|| Editor.Select.ParamSetOfMod != paramSet)
			{
				return;
			}


			//TODO.. 
			Dictionary<apBone, apRetargetBonePoseUnit> validBonePoseUnits = new Dictionary<apBone, apRetargetBonePoseUnit>();
			for (int i = 0; i < retarget.SinglePoseFile._bones.Count; i++)
			{
				apRetargetBonePoseUnit boneUnit = retarget.SinglePoseFile._bones[i];

				//동일한 Bone이 존재하는가
				//<BONE_EDIT>
				//apBone bone = targetMeshGroup.GetBone(boneUnit._uniqueID);

				//>>Bone Set 이용
				apBone bone = targetMeshGroup.GetBoneRecursive(boneUnit._uniqueID);


				if (bone != null)
				{
					validBonePoseUnits.Add(bone, boneUnit);
				}
			}
			if (validBonePoseUnits.Count == 0)
			{
				return;
			}

			bool isXAnyMirror = true;
			if (importMethod == apDialog_RetargetSinglePoseImport.IMPORT_METHOD.Mirror)
			{
				//Mirror인 경우
				//X 대칭인지, Y 대칭인지 확인해야한다.
				//Bone 리스트에서 Root 본(한개만) 찾고, 그 Root 본이 Y 대칭이면 Y 대칭으로 계산
				//그렇지 않다면 기본적으로 X 대칭이다.
				foreach (KeyValuePair<apBone, apRetargetBonePoseUnit> posePair in validBonePoseUnits)
				{
					apBone srcBone = posePair.Key;
					if (srcBone._parentBone == null && srcBone._mirrorBone != null)
					{
						if (srcBone._mirrorOption == apBone.MIRROR_OPTION.Y)
						{
							//Y대칭을 해야한다.
							isXAnyMirror = false;
							break;
						}
					}
				}
			}

			apEditorUtil.SetRecord_MeshGroupAndModifier(apUndoGroupData.ACTION.Retarget_ImportSinglePoseToMod, Editor, targetMeshGroup, targetModifier, null, false);

			//Dict
			foreach (KeyValuePair<apBone, apRetargetBonePoseUnit> posePair in validBonePoseUnits)
			{
				apBone srcBone = posePair.Key;
				apRetargetBonePoseUnit poseData = posePair.Value;

				apBone mirrorBone = srcBone._mirrorBone;

				apModifiedBone modBone = null;

				if (importMethod == apDialog_RetargetSinglePoseImport.IMPORT_METHOD.Normal
					|| mirrorBone == null)
				{
					//1. 기본 방식으로 Import
					modBone = paramSet._boneData.Find(delegate (apModifiedBone a)
					{
						return a._bone == srcBone;
					});

					//잉.. ModBone이 없군요. 등록해드리겠습니다.
					if (modBone == null)
					{
						modBone = targetModifier.AddBone(srcBone, paramSet);

					}
				}
				else
				{
					modBone = paramSet._boneData.Find(delegate (apModifiedBone a)
					{
						return a._bone == mirrorBone;
					});

					//잉.. ModBone이 없군요. 등록해드리겠습니다.
					if (modBone == null)
					{
						modBone = targetModifier.AddBone(mirrorBone, paramSet);

					}
				}


				if (modBone == null)
				{
					//처리 후에도 안된다면 스킵
					continue;
				}

				//Matrix를 대입해주자
				if (importMethod == apDialog_RetargetSinglePoseImport.IMPORT_METHOD.Normal)
				{
					//1. Normal
					modBone._transformMatrix.SetMatrix(poseData._localMatrix);
				}
				else
				{
					//2. Mirror
					bool isXInverse = true;
					if (mirrorBone == null)
					{
						isXInverse = isXAnyMirror;
					}
					else
					{
						isXInverse = (srcBone._parentBone != null || srcBone._mirrorOption == apBone.MIRROR_OPTION.X);
					}
					if (isXInverse)
					{
						modBone._transformMatrix.SetTRS(
								new Vector2(-poseData._localMatrix._pos.x, poseData._localMatrix._pos.y),//X 반전
												-poseData._localMatrix._angleDeg,
												poseData._localMatrix._scale
											);
					}
					else
					{
						modBone._transformMatrix.SetTRS(
								new Vector2(poseData._localMatrix._pos.x, -poseData._localMatrix._pos.y),//Y 반전
												-poseData._localMatrix._angleDeg,
												poseData._localMatrix._scale
											);
					}
				}
			}

			bool isChanged = Editor.Select.SubEditedParamSetGroup.RefreshSync();
			if (isChanged)
			{
				Editor.Select.MeshGroup.LinkModMeshRenderUnits();//<<Link 전에 이걸 먼저 선언한다.
				Editor.Select.MeshGroup.RefreshModifierLink();
			}
			Editor.Select.AutoSelectModMeshOrModBone();

			//추가 : ExMode에 추가한다.
			Editor.Select.RefreshMeshGroupExEditingFlags(
								Editor.Select.MeshGroup,
								targetModifier,
								Editor.Select.SubEditedParamSetGroup,
								null,
								true);

			Editor.RefreshControllerAndHierarchy(false);

			Editor.Select.MeshGroup.RefreshForce(true);

			Editor.SetRepaint();

		}

		/// <summary>
		/// Retarget 파일로부터 "단일 포즈"를 Import한다.
		/// 대상은 AnimClip
		/// </summary>
		/// <param name="targetMeshGroup"></param>
		/// <param name="retarget"></param>
		/// <param name="targetAnimClip"></param>
		/// <param name="targetFrame"></param>
		public void ImportBonePoseFromRetargetSinglePoseFileToAnimClip(apMeshGroup targetMeshGroup, apRetarget retarget, apAnimClip targetAnimClip, apAnimTimeline targetTimeline, int targetFrame,
																		apDialog_RetargetSinglePoseImport.IMPORT_METHOD importMethod)
		{
			if (Editor._portrait == null
				|| Editor.Select.AnimClip == null
				|| Editor.Select.AnimClip._targetMeshGroup == null
				|| Editor.Select.AnimClip != targetAnimClip
				|| Editor.Select.AnimClip._targetMeshGroup != targetMeshGroup
				|| targetTimeline == null
				|| targetTimeline._linkedModifier == null
				|| !targetTimeline._linkedModifier.IsTarget_Bone)
			{
				return;
			}


			//TODO...>>>

			Dictionary<apBone, apRetargetBonePoseUnit> validBonePoseUnits = new Dictionary<apBone, apRetargetBonePoseUnit>();
			for (int i = 0; i < retarget.SinglePoseFile._bones.Count; i++)
			{
				apRetargetBonePoseUnit boneUnit = retarget.SinglePoseFile._bones[i];
				//동일한 Bone이 존재하는가
				//<BONE_EDIT>
				//apBone bone = targetMeshGroup.GetBone(boneUnit._uniqueID);

				//>> Recursive로 변경
				apBone bone = targetMeshGroup.GetBoneRecursive(boneUnit._uniqueID);
				if (bone != null)
				{
					validBonePoseUnits.Add(bone, boneUnit);
				}
			}

			if (validBonePoseUnits.Count == 0)
			{
				return;
			}

			bool isXAnyMirror = true;
			if (importMethod == apDialog_RetargetSinglePoseImport.IMPORT_METHOD.Mirror)
			{
				//Mirror인 경우
				//X 대칭인지, Y 대칭인지 확인해야한다.
				//Bone 리스트에서 Root 본(한개만) 찾고, 그 Root 본이 Y 대칭이면 Y 대칭으로 계산
				//그렇지 않다면 기본적으로 X 대칭이다.
				foreach (KeyValuePair<apBone, apRetargetBonePoseUnit> posePair in validBonePoseUnits)
				{
					apBone srcBone = posePair.Key;
					if (srcBone._parentBone == null && srcBone._mirrorBone != null)
					{
						if (srcBone._mirrorOption == apBone.MIRROR_OPTION.Y)
						{
							//Y대칭을 해야한다.
							isXAnyMirror = false;
							break;
						}
					}
				}
			}



			apEditorUtil.SetRecord_PortraitMeshGroupModifier(apUndoGroupData.ACTION.Retarget_ImportSinglePoseToAnim, Editor, Editor._portrait, targetMeshGroup, targetTimeline._linkedModifier, null, false);

			bool isAnyTimelinelayerCreated = false;

			//Dictionary 돌면서 Timelinelayer 여부 체크하고 Keyframe을 체크한 뒤, ModBone을 넣어주자
			//두번 도는데, 일단 TimelineLayer를 추가하고 처리하자
			foreach (KeyValuePair<apBone, apRetargetBonePoseUnit> posePair in validBonePoseUnits)
			{
				apBone srcBone = posePair.Key;
				apRetargetBonePoseUnit poseData = posePair.Value;

				apBone mirrorBone = srcBone._mirrorBone;

				//1. Timelinelayer가 존재하는가
				apAnimTimelineLayer timelineLayer = null;
				if (importMethod == apDialog_RetargetSinglePoseImport.IMPORT_METHOD.Normal
					|| mirrorBone == null)
				{
					timelineLayer = targetTimeline.GetTimelineLayer(srcBone);
				}
				else
				{
					timelineLayer = targetTimeline.GetTimelineLayer(mirrorBone);
				}

				if (timelineLayer == null)
				{
					//새로 만들자
					int nextLayerID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.AnimTimelineLayer);
					if (nextLayerID < 0)
					{
						//EditorUtility.DisplayDialog("Error", "Timeline Layer Add Failed", "Close");
						EditorUtility.DisplayDialog(Editor.GetText(TEXT.AnimTimelineLayerAddFailed_Title),
														Editor.GetText(TEXT.AnimTimelineLayerAddFailed_Body),
														Editor.GetText(TEXT.Close));
						return;
					}

					timelineLayer = new apAnimTimelineLayer();
					timelineLayer.Link(targetAnimClip, targetTimeline);

					if (importMethod == apDialog_RetargetSinglePoseImport.IMPORT_METHOD.Normal || mirrorBone == null)
					{
						timelineLayer.Init_Bone(targetTimeline, nextLayerID, srcBone._uniqueID);
					}
					else
					{
						timelineLayer.Init_Bone(targetTimeline, nextLayerID, mirrorBone._uniqueID);
					}

					targetTimeline._layers.Add(timelineLayer);

					isAnyTimelinelayerCreated = true;//<타임라인 레이어가 추가되었다.
				}



				//현재 프레임에 Keyframe이 있는가
				apAnimKeyframe keyframe = timelineLayer.GetKeyframeByFrameIndex(targetFrame);
				if (keyframe == null)
				{
					//키프레임이 없다. 추가해주자
					AddAnimKeyframe(targetFrame, timelineLayer, false, false, false, false);
				}
			}

			//Refresh 하자
			Editor._portrait.LinkAndRefreshInEditor(false, targetMeshGroup);

			if (isAnyTimelinelayerCreated)
			{
				//Sync를 한번 해두자
				AddAndSyncAnimClipToModifier(Editor.Select.AnimClip);
			}


			targetMeshGroup.LinkModMeshRenderUnits();
			targetMeshGroup.RefreshModifierLink();
			targetMeshGroup._modifierStack.InitModifierCalculatedValues();

			Editor.RefreshTimelineLayers(apEditor.REFRESH_TIMELINE_REQUEST.All, null);

			//다시 돌자
			foreach (KeyValuePair<apBone, apRetargetBonePoseUnit> posePair in validBonePoseUnits)
			{
				apBone srcBone = posePair.Key;
				apRetargetBonePoseUnit poseData = posePair.Value;

				apBone mirrorBone = srcBone._mirrorBone;

				//로드할 본에 대해 타임라인 레이어를 찾는다 => 위에서 생성을 했다.
				//타임라인 레이어에 대해서 현재 프레임에 해당하는 키프레임이 있는가 => 없을 수 없다. 없으면 만들었을 테니까
				//붙여넣기할 키프레임에 대해 LinkedModBone_Editor가 있는가 => 에엥? (이건 에러 맞다.. 잘 못 판단함)
				apAnimTimelineLayer timelineLayer = null;

				if (importMethod == apDialog_RetargetSinglePoseImport.IMPORT_METHOD.Normal || mirrorBone == null)
				{
					timelineLayer = targetTimeline.GetTimelineLayer(srcBone);
				}
				else
				{
					timelineLayer = targetTimeline.GetTimelineLayer(mirrorBone);
				}

				if (timelineLayer == null)
				{
					Debug.Log("AnyPortrait : Importing Pose | No TimelineLayer to load data.");//생성한 타임라인 레이어가 
					continue;
				}

				apAnimKeyframe keyframe = timelineLayer.GetKeyframeByFrameIndex(targetFrame);
				if (keyframe == null)
				{
					//키프레임이 없으면 안된다. 위 코드에서 생성했기 때문
					Debug.Log("AnyPortrait : Importing Pose | No Keyframe to load data.");
					continue;
				}

				if (keyframe._linkedModBone_Editor == null)
				{
					//
					Debug.Log("AnyPortrait : Importing Pose | No Bone Data to load data.");
					continue;
				}

				//Matrix를 대입해주자
				if (importMethod == apDialog_RetargetSinglePoseImport.IMPORT_METHOD.Normal)
				{
					//1. Normal
					keyframe._linkedModBone_Editor._transformMatrix.SetMatrix(poseData._localMatrix);
				}
				else
				{
					//2. Mirror
					bool isXInverse = true;
					if (mirrorBone == null)
					{
						isXInverse = isXAnyMirror;
					}
					else
					{
						isXInverse = (srcBone._parentBone != null || srcBone._mirrorOption == apBone.MIRROR_OPTION.X);
					}

					if (isXInverse)
					{
						keyframe._linkedModBone_Editor._transformMatrix.SetTRS(
								new Vector2(-poseData._localMatrix._pos.x, poseData._localMatrix._pos.y),//X 반전
												-poseData._localMatrix._angleDeg,
												poseData._localMatrix._scale
											);
					}
					else
					{
						keyframe._linkedModBone_Editor._transformMatrix.SetTRS(
								new Vector2(poseData._localMatrix._pos.x, -poseData._localMatrix._pos.y),//Y 반전
												-poseData._localMatrix._angleDeg,
												poseData._localMatrix._scale
											);
					}
				}
			}

			//다시 Refresh
			targetMeshGroup.RefreshModifierLink();
			targetMeshGroup._modifierStack.InitModifierCalculatedValues();

			//추가 : ExMode에 추가한다.
			Editor.Select.RefreshMeshGroupExEditingFlags(
								targetMeshGroup,
								targetTimeline._linkedModifier,
								null,
								targetAnimClip,
								true);


			Editor.RefreshControllerAndHierarchy(true);//TimelineLayer도 갱신
			Editor.Select.AutoSelectAnimWorkKeyframe();
			//Refresh 추가
			Editor.Select.RefreshAnimEditing(true);

			//targetMeshGroup.RefreshForce(true);
			//targetAnimClip.Update_Editor(0.0f, true, true)
			Editor.SetRepaint();
		}

		//-----------------------------------------------------------------------------
		// 본 리깅
		//-----------------------------------------------------------------------------
		/// <summary>
		/// 현재 선택한 ModMesh의 ModVertRig의 Weight 리스트에 "현재 선택한 Bone"을 선택한 Weight와 함께 추가한다.
		/// 만약 선택한 ModVertRig(1개 이상)가 없고 Bone을 선택하지 않았다면 패스한다.
		/// Bone이 등록되지 않았다면 자동으로 등록하며, AutoNormalize가 되어있다면 같이 수행한다.
		/// </summary>
		/// <param name="calculateType">연산 타입. 0 : 대입, 1 : 더하기, 2 : 곱하기</param>
		/// <param name="weight"></param>
		/// <param name="isSetOtherRigValue0or1">이 값이 true이면 0이나 1일 때에 Normalize를 하면 다른 RigData에 값을 할당한다. (이전에는 안되며 false가 기본값)</param>
		public void SetBoneWeight(float weight, int calculateType, bool isRecord = true, bool isSetOtherRigValue0or1 = false)
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModRenderVertListOfMod == null
				|| Editor.Select.ModRenderVertListOfMod.Count == 0
				|| Editor.Select.Bone == null
				|| !Editor.Select.IsRigEditBinding)
			{
				return;
			}

			apBone bone = Editor.Select.Bone;
			List<apModifiedVertexRig> vertRigs = new List<apModifiedVertexRig>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertRig != null)
				{
					vertRigs.Add(modRenderVert._modVertRig);
				}
			}
			if (vertRigs.Count == 0)
			{
				return;
			}

			if (isRecord)
			{
				//Undo - 연속 입력 가능
				apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SetBoneWeight, Editor, Editor.Select.Modifier, null, true);
			}

			bool isAutoNormalize = Editor.Select._rigEdit_isAutoNormalize;

			apModifiedVertexRig vertRig = null;
			apModifiedVertexRig.WeightPair targetWeightPair = null;
			for (int iVertRig = 0; iVertRig < vertRigs.Count; iVertRig++)
			{
				vertRig = vertRigs[iVertRig];



				//Bone이 있는가?
				targetWeightPair = null;
				for (int iPair = 0; iPair < vertRig._weightPairs.Count; iPair++)
				{
					if (vertRig._weightPairs[iPair]._bone == bone)
					{
						targetWeightPair = vertRig._weightPairs[iPair];
						break;
					}
				}
				//없으면 추가
				if (targetWeightPair == null)
				{
					targetWeightPair = new apModifiedVertexRig.WeightPair(bone);
					vertRig._weightPairs.Add(targetWeightPair);
				}
				switch (calculateType)
				{
					case 0://대입
						targetWeightPair._weight = weight;
						break;

					case 1://더하기
						targetWeightPair._weight += weight;
						break;

					case 2://곱하기
						targetWeightPair._weight *= weight;
						break;
				}
				//if (isMultiply)
				//{
				//	targetWeightPair._weight *= weight;
				//}
				//else
				//{
				//	targetWeightPair._weight = weight;
				//}

				vertRig.CalculateTotalWeight();

				if (isAutoNormalize)
				{
					//Normalize를 하자
					vertRig.NormalizeExceptPair(targetWeightPair, isSetOtherRigValue0or1);
				}
			}

			Editor.Select.MeshGroup.RefreshForce();
			Editor.RefreshControllerAndHierarchy(false);
		}


		/// <summary>
		/// Bone Weight를 Normalize한다.
		/// </summary>
		public void SetBoneWeightNormalize()
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModRenderVertListOfMod == null
				|| Editor.Select.ModRenderVertListOfMod.Count == 0
				//|| Editor.Select.Bone == null
				|| !Editor.Select.IsRigEditBinding)
			{
				return;
			}

			//apBone bone = Editor.Select.Bone;
			List<apModifiedVertexRig> vertRigs = new List<apModifiedVertexRig>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertRig != null)
				{
					vertRigs.Add(modRenderVert._modVertRig);
				}
			}
			if (vertRigs.Count == 0)
			{
				return;
			}
			//Undo
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SetBoneWeight, Editor, Editor.Select.Modifier, null, false);

			apModifiedVertexRig vertRig = null;
			for (int iVertRig = 0; iVertRig < vertRigs.Count; iVertRig++)
			{
				vertRig = vertRigs[iVertRig];

				vertRig.Normalize();
			}

			Editor.Select.MeshGroup.RefreshForce();
			Editor.RefreshControllerAndHierarchy(false);
		}

		/// <summary>
		/// Bone Weight를 Prune한다.
		/// </summary>
		public void SetBoneWeightPrune()
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModRenderVertListOfMod == null
				|| Editor.Select.ModRenderVertListOfMod.Count == 0
				|| Editor.Select.Bone == null
				|| !Editor.Select.IsRigEditBinding)
			{
				return;
			}

			apBone bone = Editor.Select.Bone;
			List<apModifiedVertexRig> vertRigs = new List<apModifiedVertexRig>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertRig != null)
				{
					vertRigs.Add(modRenderVert._modVertRig);
				}
			}
			if (vertRigs.Count == 0)
			{
				return;
			}

			//Undo
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SetBoneWeight, Editor, Editor.Select.Modifier, false, false);

			apModifiedVertexRig vertRig = null;
			for (int iVertRig = 0; iVertRig < vertRigs.Count; iVertRig++)
			{
				vertRig = vertRigs[iVertRig];

				vertRig.Prune();
			}

			Editor.Select.MeshGroup.RefreshForce();
			Editor.RefreshControllerAndHierarchy(false);
		}



		/// <summary>
		/// Bone Weight를 Blend한다.
		/// Blend시 주의점
		/// Blend는 내부의 Weight가 아니라 "주변의 Weight를 비교"하여 Blend한다.
		/// Mesh를 참조하여 "연결된 Vertex"를 가지는 "VertRigs"를 모두 검색한 뒤,
		/// "주변 Weight의 평균값" 10% + "자신의 Weight" + 90%를 적용한다.
		/// 연산 결과가 요청된 다른 Vertex에 영향을 주지 않도록 결과를 따로 저장했다가 일괄 적용한다.
		/// </summary>
		public void SetBoneWeightBlend()
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModMeshOfMod == null
				|| Editor.Select.ModRenderVertListOfMod == null
				|| Editor.Select.ModRenderVertListOfMod.Count == 0
				//|| Editor.Select.Bone == null<<여기선 Bone이 없어도 됩니다.
				|| !Editor.Select.IsRigEditBinding)
			{
				return;
			}

			//apBone bone = Editor.Select.Bone;
			apModifiedMesh modMesh = Editor.Select.ModMeshOfMod;
			List<apModifiedVertexRig> vertRigs = new List<apModifiedVertexRig>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertRig != null)
				{
					vertRigs.Add(modRenderVert._modVertRig);
				}
			}
			if (vertRigs.Count == 0)
			{
				return;
			}

			//Undo
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SetBoneWeight, Editor, Editor.Select.Modifier, null, false);

			apBone curSelectedBone = Editor.Select.Bone;

			//요청된 VertRig와 연결된 외부의 Weight 평균값
			Dictionary<apModifiedVertexRig, List<apModifiedVertexRig.WeightPair>> linkedWeightAvgs = new Dictionary<apModifiedVertexRig, List<apModifiedVertexRig.WeightPair>>();

			apModifiedVertexRig vertRig = null;
			for (int iVertRig = 0; iVertRig < vertRigs.Count; iVertRig++)
			{
				//각 Vertex별로 검사 시작
				vertRig = vertRigs[iVertRig];

				List<apModifiedVertexRig.WeightPair> curWeightPairs = vertRig._weightPairs;

				//연결된 Vertex
				List<apVertex> linkedVerts = vertRig._mesh.GetLinkedVertex(vertRig._vertex, true);

				if (linkedVerts == null)
				{
					continue;
				}

				//연결된 VerRigs를 가져오자
				List<apModifiedVertexRig> linkedVertRigs = modMesh._vertRigs.FindAll(delegate (apModifiedVertexRig a)
				{
					return a != vertRig && linkedVerts.Contains(a._vertex);
				});

				if (linkedVertRigs.Count == 0)
				{
					continue;
				}


				//평균값을 내자
				//Bone+Weight 조합으로 다 더한 뒤
				//연결된 VertRig 개수만큼 나누자
				List<apModifiedVertexRig.WeightPair> weightPairList = new List<apModifiedVertexRig.WeightPair>();
				int nLinkedVertRigs = linkedVertRigs.Count;
				for (int iLink = 0; iLink < nLinkedVertRigs; iLink++)
				{
					apModifiedVertexRig linkedVertRig = linkedVertRigs[iLink];
					for (int iWP = 0; iWP < linkedVertRig._weightPairs.Count; iWP++)
					{
						apModifiedVertexRig.WeightPair linkedWeightPair = linkedVertRig._weightPairs[iWP];
						
						bool isExistBoneInTable = curWeightPairs.Exists(delegate (apModifiedVertexRig.WeightPair a)
						{
							return a._bone == linkedWeightPair._bone;
						});

						bool isSelectedBone = curSelectedBone != null && curSelectedBone == linkedWeightPair._bone;

						if (!isExistBoneInTable && !isSelectedBone)
						{
							//테이블에 Bone이 없다면 Blend 타겟에 넣지 않는다.
							//+ 선택된 Bone도 아니라면
							continue;
						}

						////추가 19.8.25 : 선택되지 본의 Lock 설정이 있다면, 값은 "현재의 값"으로 대체해야한다.
						//bool isLock = linkedWeightPair._bone._isRigLock && linkedWeightPair._bone != curSelectedBone;

						apModifiedVertexRig.WeightPair existPair = weightPairList.Find(delegate (apModifiedVertexRig.WeightPair a)
						{
							return a._bone == linkedWeightPair._bone;
						});

						if (existPair != null)
						{
							//이미 존재하는 Bone이다.
							//Weight만 추가하자.
							existPair._weight += linkedWeightPair._weight;
						}
						else
						{
							//등록되지 않은 Bone이다.
							//새로 추가하자
							apModifiedVertexRig.WeightPair newPair = new apModifiedVertexRig.WeightPair(linkedWeightPair._bone);
							newPair._weight = linkedWeightPair._weight;
							weightPairList.Add(newPair);
						}
					}
				}
				//평균값을 내자
				for (int iWP = 0; iWP < weightPairList.Count; iWP++)
				{
					weightPairList[iWP]._weight /= nLinkedVertRigs;
				}

				//연산 결과에 등록[요청 VertRig + 주변의 Rig 데이터]
				linkedWeightAvgs.Add(vertRig, weightPairList);
			}

			//값을 넣어주자
			//비율은 20% + 80%
			float ratio_Src = 0.8f;
			float ratio_Link = 0.2f;

			List<apModifiedVertexRig> modVertRigList = new List<apModifiedVertexRig>();
			Dictionary<apModifiedVertexRig, float> modVertRig_2_PrevTotalWeight = new Dictionary<apModifiedVertexRig, float>();
			Dictionary<apModifiedVertexRig, float> modVertRig_2_LockedWeight = new Dictionary<apModifiedVertexRig, float>();
			Dictionary<apModifiedVertexRig, bool> modVertRig_2_IsAnyLocked = new Dictionary<apModifiedVertexRig, bool>();
			

			foreach (KeyValuePair<apModifiedVertexRig, List<apModifiedVertexRig.WeightPair>> rigPair in linkedWeightAvgs)
			{
				apModifiedVertexRig targetVertRig = rigPair.Key;
				List<apModifiedVertexRig.WeightPair> linkedWeightPairs = rigPair.Value;

				//Total Weight를 계산하자 + 나중을 위해 Pair로 기록.
				targetVertRig.CalculateTotalWeight();

				float prevTotalWeight = 0.0f;
				float prevLockedWeight = 0.0f;
				bool isAnyLocked = false;
				for (int iWP = 0; iWP < targetVertRig._weightPairs.Count; iWP++)
				{
					apModifiedVertexRig.WeightPair weightPair = targetVertRig._weightPairs[iWP];
					prevTotalWeight += weightPair._weight;
					if(weightPair._bone._isRigLock && weightPair._bone != curSelectedBone)
					{
						//RigLock이 걸린 상태인가?
						isAnyLocked = true;
						prevLockedWeight += weightPair._weight;
					}
				}

				//변경 19.8.27 : 이건 아래의 보정 코드에서 체크하는 걸로
				//float prevTotalWeight = targetVertRig._totalWeight;
				//if (Editor.Select._rigEdit_isAutoNormalize || Mathf.Approximately(prevTotalWeight, 1.0f))
				//{
				//	//Prev Weight가 되어야 하는 값 (Auto Normalize 옵션이거나 1 근처일 때)
				//	prevTotalWeight = 1.0f;
				//}

				//List/Dictionary에 필요한 값을 추가하자.
				if(!modVertRigList.Contains(targetVertRig))
				{
					modVertRigList.Add(targetVertRig);
				}

				if (!modVertRig_2_PrevTotalWeight.ContainsKey(targetVertRig))
				{
					modVertRig_2_PrevTotalWeight.Add(targetVertRig, prevTotalWeight);
				}

				if(!modVertRig_2_LockedWeight.ContainsKey(targetVertRig))
				{
					modVertRig_2_LockedWeight.Add(targetVertRig, prevLockedWeight);
				}

				if(!modVertRig_2_IsAnyLocked.ContainsKey(targetVertRig))
				{
					modVertRig_2_IsAnyLocked.Add(targetVertRig, isAnyLocked);
				}




				//1) Bone이 없다면 추가해준다.
				//2) targetVertRig 기준으로 : 80% 20% 비율로 계산

				//3) 추가 : Lock걸린 경우 제외하고 변경해야한다.

				for (int i = 0; i < linkedWeightPairs.Count; i++)
				{
					apModifiedVertexRig.WeightPair linkedPair = linkedWeightPairs[i];

					if (!targetVertRig._weightPairs.Exists(delegate (apModifiedVertexRig.WeightPair a)
					 {
						 return a._bone == linkedPair._bone;
					 }))
					{
						//새로 추가해준다.
						apModifiedVertexRig.WeightPair newPair = new apModifiedVertexRig.WeightPair(linkedPair._bone);
						newPair._weight = 0.0f;
						targetVertRig._weightPairs.Add(newPair);
					}
				}

				for (int i = 0; i < targetVertRig._weightPairs.Count; i++)
				{
					apModifiedVertexRig.WeightPair targetWeight = targetVertRig._weightPairs[i];
					apModifiedVertexRig.WeightPair linkedWeight = linkedWeightPairs.Find(delegate (apModifiedVertexRig.WeightPair a)
					{
						return a._bone == targetWeight._bone;
					});

					//추가 19.8.25 : 만약 Lock 상태이며 현재 선택된 본이 아니라면 기존의 값을 유지한다.
					bool isRigLock = (targetWeight._bone._isRigLock && targetWeight._bone != curSelectedBone);

					if (!isRigLock)
					{
						if (linkedWeight != null)
						{
							targetWeight._weight = targetWeight._weight * ratio_Src + linkedWeight._weight * ratio_Link;
						}
						else
						{
							targetWeight._weight = targetWeight._weight * ratio_Src;
						}
					}
				}
			}

			//Blend 이후에 Weight가 바뀌는 버그가 있다.
			//Weight값을 보정해주자
			//참고 : 보정일 때는 RigLock이 적용이 안된다.
			//foreach (KeyValuePair<apModifiedVertexRig, float> prevWeightPair in modVertRig_2_PrevTotalWeight)
			for (int iModVert = 0; iModVert < modVertRigList.Count; iModVert++)
			{
				apModifiedVertexRig modVertRig = modVertRigList[iModVert];
				float prevTotalWeight = modVertRig_2_PrevTotalWeight[modVertRig];
				float prevLockedWeight = modVertRig_2_LockedWeight[modVertRig];
				bool isAnyLocked = modVertRig_2_IsAnyLocked[modVertRig];
				//apModifiedVertexRig modRig = prevWeightPair.Key;
				//float prevTotalWeight = prevWeightPair.Value;

				if (Editor.Select._rigEdit_isAutoNormalize || Mathf.Approximately(prevTotalWeight, 1.0f))
				{
					//TotalWeight를 1로 만들어야 한다.
					float correctRatio = 0.0f;
					if(prevTotalWeight > 0.0f)
					{
						correctRatio = 1.0f / prevTotalWeight;
					}
					else
					{
						correctRatio = 1.0f;
					}
					prevTotalWeight = 1.0f;
					prevLockedWeight *= correctRatio;
				}

				//Total Weight를 다시 계산하자
				modVertRig.CalculateTotalWeight();
				
				float nextTotalWeight = modVertRig._totalWeight;
				float nextLockedWeight = 0.0f;
				


				//변경 19.8.27
				//1) RigLock이 없다면 > 그냥 바로 보정
				//2) RigLock이 하나라도 있다면 > Unlocked Weight만 계산해서 PrevUnlockedWeight를 만들어야 한다.
				//단, Unlocked가 0이거나 음수라면, 1)의 경우로 계산한다.

				bool isCheckOnlyUnlocked = false;//이게 False면 1), True면 2)로 계산한다.
				float prevUnlockedWeight = prevTotalWeight - prevLockedWeight;
				float nextUnlockedWeight = 0.0f;

				if (isAnyLocked)
				{
					for (int iWP = 0; iWP < modVertRig._weightPairs.Count; iWP++)
					{
						apModifiedVertexRig.WeightPair weightPair = modVertRig._weightPairs[iWP];
						if (weightPair._bone._isRigLock && weightPair._bone != curSelectedBone)
						{
							//RigLock이 걸린 상태인가?
							nextLockedWeight += weightPair._weight;
						}
					}
					nextUnlockedWeight = nextTotalWeight - nextLockedWeight;
					if(nextUnlockedWeight > 0.0f && prevUnlockedWeight > 0.0f)
					{
						//Unlocked만 체크해봅시다.
						isCheckOnlyUnlocked = true;
					}
				}
				if (!isCheckOnlyUnlocked)
				{
					//1) 모든 Weight에 대하여 보정 (totalWeight가 0 이상일때)
					if (!Mathf.Approximately(prevTotalWeight, nextTotalWeight) && nextTotalWeight > 0.0f)
					{
						//Debug.LogError("Bone Weight Blend 후 Weight 변경 : [" + prevTotalWeight + "] > [" + nextTotalWeight + "]");

						//다시 Prev에 맞추어야 한다.
						float correctToPrevRatio = prevTotalWeight / nextTotalWeight;

						apModifiedVertexRig.WeightPair curPair = null;
						for (int i = 0; i < modVertRig._weightPairs.Count; i++)
						{
							curPair = modVertRig._weightPairs[i];
							curPair._weight *= correctToPrevRatio;
						}

						//마지막 계산
						modVertRig.CalculateTotalWeight();

						//Debug.Log("보정 후 가중치 : " + modRig._totalWeight);
					}
				}
				else
				{
					//2) Unlocked에 대하여만 보정
					float correctToPrevRatio = prevUnlockedWeight / nextUnlockedWeight;

					apModifiedVertexRig.WeightPair curPair = null;
					for (int i = 0; i < modVertRig._weightPairs.Count; i++)
					{
						curPair = modVertRig._weightPairs[i];
						if (!curPair._bone._isRigLock || curPair._bone == curSelectedBone)
						{
							//RigLock이 걸리지 않은 경우 (조건문 반대임)
							curPair._weight *= correctToPrevRatio;
						}
					}

					modVertRig.CalculateTotalWeight();
				}
				


			}



			Editor.Select.MeshGroup.RefreshForce();
			Editor.RefreshControllerAndHierarchy(false);
		}

		/// <summary>
		/// 선택한 Vertex Rig에 대해서 Grow 또는 Select 선택을 한다.
		/// </summary>
		/// <param name="isGrow"></param>
		public void SelectVertexRigGrowOrShrink(bool isGrow)
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModMeshOfMod == null
				|| Editor.Select.ModRenderVertListOfMod == null
				|| Editor.Select.ModRenderVertListOfMod.Count == 0
				//|| Editor.Select.Bone == null<<여기선 Bone이 없어도 됩니다.
				|| !Editor.Select.IsRigEditBinding)
			{
				return;
			}

			//apBone bone = Editor.Select.Bone;
			apModifiedMesh modMesh = Editor.Select.ModMeshOfMod;
			List<apModifiedVertexRig> vertRigs = new List<apModifiedVertexRig>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertRig != null)
				{
					vertRigs.Add(modRenderVert._modVertRig);
				}
			}
			if (vertRigs.Count == 0)
			{
				return;
			}

			//apEditorUtil.SetRecord("Select Vertex Rig Grow Or Shrink", Editor._portrait);


			apModifiedVertexRig vertRig = null;
			if (isGrow)
			{
				//Grow인 경우
				//- 각 Vertex에 대해서 Linked Vertex를 가져오고, 기존의 리스트에 없는 Vertex를 선택해준다.
				List<apModifiedVertexRig> addVertRigs = new List<apModifiedVertexRig>();
				for (int iVR = 0; iVR < vertRigs.Count; iVR++)
				{
					vertRig = vertRigs[iVR];

					List<apVertex> linkedVerts = vertRig._mesh.GetLinkedVertex(vertRig._vertex, false);
					for (int iVert = 0; iVert < linkedVerts.Count; iVert++)
					{
						bool isExist = vertRigs.Exists(delegate (apModifiedVertexRig a)
						{
							return a._vertex == linkedVerts[iVert];
						});
						if (isExist)
						{
							//하나라도 이미 선택된 거네요.
							//패스
							continue;
						}

						apVertex addVertex = linkedVerts[iVert];
						//하나도 선택되지 않은 Vertex라면
						//새로 추가해주자
						apModifiedVertexRig addVertRig = modMesh._vertRigs.Find(delegate (apModifiedVertexRig a)
						{
							return a._vertex == addVertex;
						});
						if (addVertRig != null)
						{
							if (!addVertRigs.Contains(addVertRig))
							{
								addVertRigs.Add(addVertRig);
							}
						}
					}
				}
				//일괄적으로 추가해주자
				for (int i = 0; i < addVertRigs.Count; i++)
				{
					Editor.Select.AddModVertexOfModifier(null, addVertRigs[i], null, addVertRigs[i]._renderVertex);
				}
			}
			else
			{
				//Shirink인 경우
				//- 각 Vertex에 대해서 Linked Vertex를 가져온다.
				//1) Linked Vert가 없으면 : 삭제 리스트에 넣는다.
				//2) Linked Vert 중에서 "지금 선택 중인 Vert"에 해당하지 않는 Vert가 하나라도 있으면 : 삭제 리스트에 넣는다.
				//3) 모든 Linked Vert가 "지금 선택중인 Vert"에 해당된다면 유지
				List<apModifiedVertexRig> removeVertRigs = new List<apModifiedVertexRig>();
				for (int iVR = 0; iVR < vertRigs.Count; iVR++)
				{
					vertRig = vertRigs[iVR];

					List<apVertex> linkedVerts = vertRig._mesh.GetLinkedVertex(vertRig._vertex, false);
					if (linkedVerts == null || linkedVerts.Count == 0)
					{
						//1) 연결된게 없으면 삭제
						removeVertRigs.Add(vertRig);
						continue;
					}

					//모든 Vertex가 현재 선택된 상태인지 확인하자
					bool isAllSelected = true;
					for (int iVert = 0; iVert < linkedVerts.Count; iVert++)
					{
						bool isExist = vertRigs.Exists(delegate (apModifiedVertexRig a)
						{
							return a._vertex == linkedVerts[iVert];
						});
						if (!isExist)
						{
							//선택되지 않은 Vertex를 발견!
							isAllSelected = false;
							break;
						}
					}
					if (!isAllSelected)
					{
						//2) 하나라도 선택되지 않은 Link Vertex가 발견되었다면 이건 삭제 대상이다.
						if (!removeVertRigs.Contains(vertRig))
						{
							removeVertRigs.Add(vertRig);
						}
					}
					else
					{
						//추가)
						//만약 외곽선에 위치한 Vertex라면 우선순위에 포함된다.
						bool isOutlineVertex = vertRig._mesh.IsOutlineVertex(vertRig._vertex);
						if (isOutlineVertex)
						{
							if (!removeVertRigs.Contains(vertRig))
							{
								removeVertRigs.Add(vertRig);
							}
						}
					}
				}

				if (removeVertRigs.Count > 0)
				{
					//하나씩 삭제하자

					for (int i = 0; i < removeVertRigs.Count; i++)
					{
						vertRig = removeVertRigs[i];
						Editor.Select.RemoveModVertexOfModifier(null, vertRig, null, vertRig._renderVertex);
					}
				}

			}

		}

		public void SelectVerticesOfTheBone()
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModMeshOfMod == null
				|| Editor.Select.Bone == null)
			{
				return;
			}

			apBone targetBone = Editor.Select.Bone;
			apModifiedMesh modMesh = Editor.Select.ModMeshOfMod;
			if(modMesh._vertRigs == null || modMesh._vertRigs.Count == 0)
			{
				return;
			}

			bool isCtrl = false;
#if UNITY_EDITOR_OSX
			isCtrl = Event.current.command;
#else
			isCtrl = Event.current.control;
#endif
			if(!isCtrl)
			{
				//선택하기 전에 기존 버텍스 선택 취소
				Editor.Select.SetModVertexOfModifier(null, null, null, null);
			}

			List<apModifiedVertexRig> addVertRigs = new List<apModifiedVertexRig>();

			apModifiedVertexRig vertRig = null;
			for (int iVertRig = 0; iVertRig < modMesh._vertRigs.Count; iVertRig++)
			{
				vertRig = modMesh._vertRigs[iVertRig];
				if(vertRig == null)
				{
					continue;
				}
				if(vertRig._weightPairs.Exists(delegate(apModifiedVertexRig.WeightPair a)
				{
					return a._bone == targetBone;
				}))
				{
					//선택된 본과 연결되어 있다면
					addVertRigs.Add(vertRig);
				}
			}

			//일괄적으로 추가해주자
			if (addVertRigs.Count > 0)
			{
				for (int i = 0; i < addVertRigs.Count; i++)
				{
					Editor.Select.AddModVertexOfModifier(null, addVertRigs[i], null, addVertRigs[i]._renderVertex);
				}
			}
		}

		public void SetBoneAutoRig()
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModMeshOfMod == null
				|| Editor.Select.ModRenderVertListOfMod == null
				|| Editor.Select.ModRenderVertListOfMod.Count == 0
				|| !Editor.Select.IsRigEditBinding)
			{
				return;
			}

			apMeshGroup meshGroup = Editor.Select.MeshGroup;

			//1. 대상이 되는 VertRig를 리스트로 만든다.
			apModifiedMesh modMesh = Editor.Select.ModMeshOfMod;
			modMesh._renderUnit.CalculateWorldPositionWithoutModifier();

			List<apModifiedVertexRig> vertRigs = new List<apModifiedVertexRig>();
			List<apModifiedVertexRig> vertRigs_All = new List<apModifiedVertexRig>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;
			

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertRig != null)
				{
					if(!vertRigs.Contains(modRenderVert._modVertRig))
					{
						vertRigs.Add(modRenderVert._modVertRig);
						vertRigs_All.Add(modRenderVert._modVertRig);
					}
				}
			}
			if (vertRigs.Count == 0)
			{
				return;
			}

			//전체 VertRig도 가져오자
			apModifiedVertexRig curVertRig = null;
			for (int i = 0; i < modMesh._vertRigs.Count; i++)
			{
				curVertRig = modMesh._vertRigs[i];
				if (!vertRigs_All.Contains(curVertRig))
				{
					vertRigs_All.Add(curVertRig);
				}
			}


			List<apBone> bones = meshGroup._boneList_All;
			

			//Undo
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SetBoneWeight, Editor, Editor.Select.Modifier, null, false);

			apModifiedVertexRig vertRig = null;

			//2. 버텍스마다 "가장 가까운 Bone"을 찾자.
			//- 데이터는 Dictionary<Bone + Vert List> 형식으로 저장
			//- 만약 이미 Rig 데이터가 있다면, 가장 큰 값의 Bone을 선택한다. (여러개일 수 있음)

			Dictionary<apBone, apAutoRigUtil.EnvelopeInfo> bone2Envelope = new Dictionary<apBone, apAutoRigUtil.EnvelopeInfo>();
			Dictionary<apModifiedVertexRig,apAutoRigUtil.EnvelopeInfo> vertRig2NearestEnvelope = new Dictionary<apModifiedVertexRig, apAutoRigUtil.EnvelopeInfo>();
			Vector2 vertPos = Vector2.zero;
			apBone curBone = null;
			apBone nearestBone = null;
			apModifiedVertexRig.WeightPair curWeightPair = null;

			
			//for (int iVR = 0; iVR < vertRigs.Count; iVR++)
			for (int iVR = 0; iVR < vertRigs_All.Count; iVR++)
			{	
				vertRig = vertRigs_All[iVR];
				vertPos = vertRig._renderVertex._pos_World_NoMod;

				bool isSelectedVertRig = vertRigs.Contains(vertRig);

				curBone = null;
				nearestBone = null;
				
				if(vertRig._weightPairs != null && vertRig._weightPairs.Count > 0)
				{
					//등록된 Bone이 있다면, 일단 여기서 먼저 체크한다.
					float maxWeight = 0.0f;
					for (int iPair = 0; iPair < vertRig._weightPairs.Count; iPair++)
					{
						 curWeightPair = vertRig._weightPairs[iPair];
						if(curWeightPair == null || curWeightPair._bone == null)
						{
							continue;
						}

						if(curWeightPair._weight > 0.0f)
						{
							if(curWeightPair._weight > maxWeight || nearestBone == null)
							{
								//가장 Weight가 큰 것을 선택한다.
								nearestBone = curWeightPair._bone;
								maxWeight = curWeightPair._weight;
							}
						}
					}
				}

				if(nearestBone == null)
				{
					//등록된 Bone이 없거나 유효한 Weight의 Bone이 없었다면
					//> 전체 검색
					float minDist = 0.0f;
					float dst = 0.0f;
					for (int iBone = 0; iBone < bones.Count; iBone++)
					{
						curBone = bones[iBone];
						dst = apAutoRigUtil.GetDistanceToBone(vertPos, curBone);
						if(dst < minDist || nearestBone == null)
						{
							//가장 가까운 Bone
							minDist = dst;
							nearestBone = curBone;
						}
					}
				}

				if(nearestBone == null)
				{
					//가장 가까운 Bone이 없다 > 에러
					continue;
				}

				//Bone > Vert 매핑 데이터에 넣자
				apAutoRigUtil.EnvelopeInfo nearestEnvelope = null;

				if(!bone2Envelope.ContainsKey(nearestBone))
				{
					nearestEnvelope = new apAutoRigUtil.EnvelopeInfo(nearestBone);
					bone2Envelope.Add(nearestBone, nearestEnvelope);
				}
				else
				{
					nearestEnvelope = bone2Envelope[nearestBone];
				}
				nearestEnvelope.AddNearVertRig(vertRig, !isSelectedVertRig);
				vertRig2NearestEnvelope.Add(vertRig, nearestEnvelope);
			}

			//3. Bone마다 가장 멀리 있는 Vertex를 기준으로 Envelope Size를 구한다.
			foreach (KeyValuePair<apBone, apAutoRigUtil.EnvelopeInfo> envelopePair in bone2Envelope)
			{
				envelopePair.Value.CalculateSize();
			}

			//3-2. Envelope가 생성되지 못한 Bone에 대해서도 Envelope 크기를 만든다.
			//(1) 인접한 본 (Parent 또는 Child) 중에 Envelope가 만들어진 본이 있다면, 그 크기의 70%를 크기로 이용한다.
			//(2) 그렇지 않다면, 본 길이 의 20%를 크기로 이용한다.
			//(3) 헬퍼인 경우 아주 작은 0.1의 값을 가진다.
			//(4) 실제 크기는 (1), (2), (3) 중에서 가장 큰 값을 이용한다.
			List<apAutoRigUtil.EnvelopeInfo> otherEnvelopes = new List<apAutoRigUtil.EnvelopeInfo>();
			for (int iBone = 0; iBone < bones.Count; iBone++)
			{
				curBone = bones[iBone];
				if(bone2Envelope.ContainsKey(curBone))
				{
					//이미 등록된 본이면 생략
					continue;
				}

				//함수를 이용하여 EnvelopeInfo를 만든다.
				apAutoRigUtil.EnvelopeInfo newEnvInfo = apAutoRigUtil.MakeEnvelopeWithoutVertRigs(curBone, bone2Envelope);
				otherEnvelopes.Add(newEnvInfo);
			}

			//이제 이걸 모두 리스트에 합친다.
			for (int i = 0; i < otherEnvelopes.Count; i++)
			{
				apAutoRigUtil.EnvelopeInfo envInfo = otherEnvelopes[i];
				bone2Envelope.Add(envInfo._bone, envInfo);
			}

			//Debug.Log("----------------------------------------");
			//연결된 본과 EnvelopeInfo 단위로 연결해야한다.
			foreach (KeyValuePair<apBone, apAutoRigUtil.EnvelopeInfo> envelopePair in bone2Envelope)
			{
				envelopePair.Value.LinkEnvelopeInfo(bone2Envelope);
			}
			//Debug.Log("----------------------------------------");
			//4. Envelope > VertRig를 모두 검사하여 Weight를 계산한다.
			//- VertRig의 Weight를 모두 초기화
			//- VertRig를 기준으로 Envelope를 모두 검사하여 Dist 기준으로 Weight를 넣는다.
			//- Normalize를 한다.

			for (int iVR = 0; iVR < vertRigs.Count; iVR++)
			{
				vertRig = vertRigs[iVR];
				//Debug.Log("Vert Rig [" + iVR + "]--------");

				vertPos = vertRig._renderVertex._pos_World_NoMod;

				if(vertRig._weightPairs == null)
				{
					vertRig._weightPairs = new List<apModifiedVertexRig.WeightPair>();
				}
				vertRig._weightPairs.Clear();//<<아예 초기화

				//원래 가장 가까웠던 EnvInfo를 찾자
				apAutoRigUtil.EnvelopeInfo nearestEnvInfo = null;
				if(vertRig2NearestEnvelope.ContainsKey(vertRig))
				{
					nearestEnvInfo = vertRig2NearestEnvelope[vertRig];
				}

				//Envelope를 모두 검사하여 Bone+Weight를 추가한다.
				apAutoRigUtil.EnvelopeInfo envInfo = null;
				foreach (KeyValuePair<apBone, apAutoRigUtil.EnvelopeInfo> envelopePair in bone2Envelope)
				{
					envInfo = envelopePair.Value;

					
					
					float weight = apAutoRigUtil.GetVertRigWeight(vertRig, envInfo, nearestEnvInfo);
					if(weight < 0.001f)
					{
						continue;
					}

					//WeightPair 값을 만들어서 넣자
					apModifiedVertexRig.WeightPair newWeightPair = new apModifiedVertexRig.WeightPair(envInfo._bone);
					newWeightPair._weight = weight;

					vertRig._weightPairs.Add(newWeightPair);
				}
				
				//Normalize
				vertRig.Normalize();

				//Debug.Log(">>");
			}
		}

		public void SetBoneAutoRig_Old()
		{
			//선택한 Vertex에 대해서 자동으로 Rigging을 해주자
			//VertRig에 등록한 Vertex에 대해서만 수행하자.
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModMeshOfMod == null
				|| Editor.Select.ModRenderVertListOfMod == null
				|| Editor.Select.ModRenderVertListOfMod.Count == 0
				//|| Editor.Select.Bone == null<<여기선 Bone이 없어도 됩니다.
				|| !Editor.Select.IsRigEditBinding)
			{
				return;
			}

			//apBone bone = Editor.Select.Bone;
			apModifiedMesh modMesh = Editor.Select.ModMeshOfMod;
			List<apModifiedVertexRig> vertRigs = new List<apModifiedVertexRig>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertRig != null)
				{
					vertRigs.Add(modRenderVert._modVertRig);
				}
			}
			if (vertRigs.Count == 0)
			{
				return;
			}

			//Undo
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SetBoneWeight, Editor, Editor.Select.Modifier, null, false);


			apModifiedVertexRig vertRig = null;

			float bias_zero = 0.0001f;
			for (int iVR = 0; iVR < vertRigs.Count; iVR++)
			{
				vertRig = vertRigs[iVR];

				if (vertRig._renderVertex == null)
				{
					continue;
				}

				Vector2 posW = vertRig._renderVertex._pos_World;
				//이제 영역 비교 및 거리 역으로 Weight를 계산
				//값을 넣고 나중에 Normalize하자
				for (int iW = 0; iW < vertRig._weightPairs.Count; iW++)
				{
					apBone bone = vertRig._weightPairs[iW]._bone;

					Vector2 bonePos_Start = bone._worldMatrix._pos;
					Vector2 bonePos_End = bone._shapePoint_End;

					Vector2 bonePos_Mid1 = bone._shapePoint_Mid1;
					Vector2 bonePos_Mid2 = bone._shapePoint_Mid2;

					float distLimit_Min = Vector2.Distance(bonePos_Mid1, bonePos_Mid2) * 0.25f;
					float distLimit_Max = distLimit_Min * 8.0f;

					if (distLimit_Max == 0.0f)
					{
						vertRig._weightPairs[iW]._weight = bias_zero;
					}
					else
					{
						//Bone 선분과 Vertex사이의 거리가 Min 이내로 들어오면 1의 값을 가진다.
						//Min-Max 사이에 들어오면 
						//Min일때 1, Max일때 0의 Weight 값(선형)을 가진다.
						//Max 밖일때는 0
						//0대신 최소값을 넣자.
						//Normalize를 수행한 후, 너무 작은 값을 0으로 만든 뒤에 다시 Normalize 반복

						float distToVert = apEditorUtil.DistanceFromLine(bonePos_Start, bonePos_End, posW);
						float weight = 0.0f;
						if (distToVert < distLimit_Min)
						{
							weight = 1.0f;
						}
						else if (distToVert < distLimit_Max)
						{
							float itp = distToVert - distLimit_Min;
							float length = distLimit_Max - distLimit_Min;


							weight = (1.0f * (length - itp) + bias_zero * itp) / length;
						}
						else
						{
							weight = bias_zero;
						}
						vertRig._weightPairs[iW]._weight = weight;
					}
				}

				vertRig.Normalize();

				//너무 작은 Weight를 0으로 만들자
				for (int iW = 0; iW < vertRig._weightPairs.Count; iW++)
				{
					if (vertRig._weightPairs[iW]._weight < 0.01f)
					{
						vertRig._weightPairs[iW]._weight = 0.0f;
					}
				}

				//다시 Normalize
				vertRig.Normalize();
			}

			Editor.Select.MeshGroup.RefreshForce();
		}

		/// <summary>
		/// 추가 19.7.27 : Rigging 편집을 위한 Lock을 모두 해제한다.
		/// 메시 그룹을 선택했거나, Rigging Binding 편집을 켜거나 끌 때 호출하자.
		/// </summary>
		public void ResetBoneRigLock(apMeshGroup meshGroup)
		{
			if(meshGroup == null)
			{
				return;
			}

			if(meshGroup._boneList_All == null || meshGroup._boneList_All.Count == 0)
			{
				return;
			}

			apBone bone = null;
			for (int iBone = 0; iBone < meshGroup._boneList_All.Count; iBone++)
			{
				bone = meshGroup._boneList_All[iBone];
				bone._isRigLock = false;
			}

			
			//자식 메시그룹에 대해서도
			if(meshGroup._childMeshGroupTransforms != null && meshGroup._childMeshGroupTransforms.Count > 0)
			{
				apMeshGroup childMeshGroup = null;
				for (int iChild = 0; iChild < meshGroup._childMeshGroupTransforms.Count; iChild++)
				{
					childMeshGroup = meshGroup._childMeshGroupTransforms[iChild]._meshGroup;
					if(childMeshGroup != meshGroup)
					{
						ResetBoneRigLock(childMeshGroup);
					}
				}
			}
		}


		public void RemoveVertRigData(List<apSelection.ModRenderVert> selectedVerts, apBone targetBone)
		{
			//Undo
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_RemoveBoneWeight, Editor, Editor.Select.Modifier, null, false);

			bool isAutoNormalize = Editor.Select._rigEdit_isAutoNormalize;

			for (int iVert = 0; iVert < selectedVerts.Count; iVert++)
			{
				apSelection.ModRenderVert modRenderVert = selectedVerts[iVert];
				if (modRenderVert._modVertRig != null)
				{
					modRenderVert._modVertRig._weightPairs.RemoveAll(delegate (apModifiedVertexRig.WeightPair a)
					{
						return a._bone == targetBone;
					});

					if (isAutoNormalize)
					{
						modRenderVert._modVertRig.Normalize();
					}

					modRenderVert._modVertRig.CalculateTotalWeight();
				}
			}

			Editor.Select.MeshGroup.RefreshForce();
			Editor.RefreshControllerAndHierarchy(false);
		}

		//Rigging과 유사하게 Physic/Volume Weight도 처리하자
		/// <summary>
		/// Physic / Volume Modifier의 Vertex Weight를 지정한다.
		/// </summary>
		/// <param name="weight"></param>
		/// <param name="calculateType">연산 타입. 0 : 대입, 1 : 더하기, 2 : 곱하기</param>
		public void SetPhyVolWeight(float weight, int calculateType)
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModMeshOfMod == null
				|| Editor.Select.ModRenderVertListOfMod == null
				|| Editor.Select.ModRenderVertListOfMod.Count == 0
				|| Editor.Select.ExEditingMode == apSelection.EX_EDIT.None)
			{
				//Debug.LogError("Failed..");
				return;
			}

			List<apModifiedVertexWeight> vertWeights = new List<apModifiedVertexWeight>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertWeight != null)
				{
					vertWeights.Add(modRenderVert._modVertWeight);
				}
			}
			if (vertWeights.Count == 0)
			{
				return;
			}

			//Undo - 연속 입력 가능
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SetPhysicsWeight, Editor, Editor.Select.Modifier, null, true);


			for (int i = 0; i < vertWeights.Count; i++)
			{
				float curWeight = vertWeights[i]._weight;
				switch (calculateType)
				{
					case 0://대입
						curWeight = weight;
						break;

					case 1://더하기
						curWeight += weight;
						break;

					case 2://곱하기
						curWeight *= weight;
						break;
				}
				curWeight = Mathf.Clamp01(curWeight);
				vertWeights[i]._weight = curWeight;
			}

			//Weight Refresh
			Editor.Select.ModMeshOfMod.RefreshVertexWeights(Editor._portrait, Editor.Select.Modifier.IsPhysics, Editor.Select.Modifier.IsVolume);

			Editor.Select.MeshGroup.RefreshForce();
			Editor.RefreshControllerAndHierarchy(false);
		}

		public void SetPhyVolWeightBlend()
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModRenderVertListOfMod == null
				|| Editor.Select.ModRenderVertListOfMod.Count == 0
				|| Editor.Select.ExEditingMode == apSelection.EX_EDIT.None
				|| Editor.Select.SubMeshInGroup == null
				|| Editor.Select.ModMeshOfMod == null)
			{
				//Debug.LogError("Failed..");
				return;
			}

			List<apModifiedVertexWeight> vertWeights = new List<apModifiedVertexWeight>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;
			apModifiedMesh modMesh = Editor.Select.ModMeshOfMod;

			apMesh targetMesh = Editor.Select.SubMeshInGroup._mesh;
			if (targetMesh == null)
			{
				Debug.LogError("Mesh is Null");
				return;
			}



			//평균값 로직은 문제가 많다.
			//모든 VertWeight를 대상으로
			//해당 Mesh에서 연결된 1Level Vert를 일일이 검색한뒤,
			//검색된 Vert의 ModVertWeight를 구하고,
			//그 ModVertWeight의 Weight의 평균을 구해서 Dictionary로 상태로 저장한다.

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertWeight != null)
				{
					vertWeights.Add(modRenderVert._modVertWeight);
				}
			}
			if (vertWeights.Count == 0)
			{
				return;
			}

			//Undo - 연속 입력 가능
			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SetPhysicsWeight, Editor, Editor.Select.Modifier, null, true);


			Dictionary<apModifiedVertexWeight, float> avgWeights = new Dictionary<apModifiedVertexWeight, float>();

			for (int iModVert = 0; iModVert < vertWeights.Count; iModVert++)
			{
				apModifiedVertexWeight modVertWeight = vertWeights[iModVert];



				float totalWeight = 0.0f;
				int nWeight = 0;

				//자기 자신도 추가
				totalWeight += modVertWeight._weight;
				nWeight++;

				List<apVertex> linkedVert = targetMesh.GetLinkedVertex(modVertWeight._vertex, true);
				for (int iLV = 0; iLV < linkedVert.Count; iLV++)
				{
					apModifiedVertexWeight linkedModVertWeight = modMesh._vertWeights.Find(delegate (apModifiedVertexWeight a)
					{
						return a._vertex == linkedVert[iLV];
					});
					if (linkedModVertWeight != null && linkedModVertWeight != modVertWeight)
					{
						totalWeight += linkedModVertWeight._weight;
						nWeight++;
					}
					else
					{
						Debug.LogError("Link Vert에 해당하는 ModVert를 찾을 수 없다.");
					}
				}
				if (nWeight > 0)
				{
					totalWeight /= nWeight;
					avgWeights.Add(modVertWeight, totalWeight);
				}
			}
			//계산된 평균값을 넣어주자
			float ratio_Src = 0.8f;
			float ratio_Avg = 0.2f;
			foreach (KeyValuePair<apModifiedVertexWeight, float> vertWeightPair in avgWeights)
			{
				vertWeightPair.Key._weight = vertWeightPair.Key._weight * ratio_Src + vertWeightPair.Value * ratio_Avg;
			}


			//평균값을 두고, 기존 80%, 평균 20%로 넣어주자
			//float avgWeight = 0.0f;
			//for (int i = 0; i < vertWeights.Count; i++)
			//{
			//	avgWeight += vertWeights[i]._weight;
			//}
			//avgWeight /= vertWeights.Count;


			//for (int i = 0; i < vertWeights.Count; i++)
			//{
			//	vertWeights[i]._weight = (vertWeights[i]._weight * ratio_Src) + (avgWeight * ratio_Avg);
			//}

			//Weight Refresh
			Editor.Select.ModMeshOfMod.RefreshVertexWeights(Editor._portrait, Editor.Select.Modifier.IsPhysics, Editor.Select.Modifier.IsVolume);

			Editor.Select.MeshGroup.RefreshForce();
			Editor.RefreshControllerAndHierarchy(false);
		}


		public void SelectVertexWeightGrowOrShrink(bool isGrow)
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModRenderVertListOfMod == null
				|| Editor.Select.ModRenderVertListOfMod.Count == 0
				|| Editor.Select.ExEditingMode == apSelection.EX_EDIT.None
				|| Editor.Select.ModMeshOfMod == null)
			{
				//Debug.LogError("Failed..");
				return;
			}

			apModifiedMesh modMesh = Editor.Select.ModMeshOfMod;
			List<apModifiedVertexWeight> vertWeights = new List<apModifiedVertexWeight>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertWeight != null)
				{
					vertWeights.Add(modRenderVert._modVertWeight);
				}
			}
			if (vertWeights.Count == 0)
			{
				return;
			}

			apModifiedVertexWeight vertWeight = null;
			if (isGrow)
			{
				//Grow인 경우
				//- 각 Vertex에 대해서 Linked Vertex를 가져오고, 기존의 리스트에 없는 Vertex를 선택해준다.
				List<apModifiedVertexWeight> addVertWeights = new List<apModifiedVertexWeight>();
				for (int iVW = 0; iVW < vertWeights.Count; iVW++)
				{
					vertWeight = vertWeights[iVW];

					List<apVertex> linkedVerts = vertWeight._mesh.GetLinkedVertex(vertWeight._vertex, false);
					for (int iVert = 0; iVert < linkedVerts.Count; iVert++)
					{
						bool isExist = vertWeights.Exists(delegate (apModifiedVertexWeight a)
						{
							return a._vertex == linkedVerts[iVert];
						});
						if (isExist)
						{
							//하나라도 이미 선택된 거네요.
							//패스
							continue;
						}

						apVertex addVertex = linkedVerts[iVert];
						//하나도 선택되지 않은 Vertex라면
						//새로 추가해주자
						apModifiedVertexWeight addVertWeight = modMesh._vertWeights.Find(delegate (apModifiedVertexWeight a)
						{
							return a._vertex == addVertex;
						});
						if (addVertWeight != null)
						{
							if (!addVertWeights.Contains(addVertWeight))
							{
								addVertWeights.Add(addVertWeight);
							}
						}
					}
				}
				//일괄적으로 추가해주자
				for (int i = 0; i < addVertWeights.Count; i++)
				{
					Editor.Select.AddModVertexOfModifier(null, null, addVertWeights[i], addVertWeights[i]._renderVertex);
				}
			}
			else
			{
				//Shirink인 경우
				//- 각 Vertex에 대해서 Linked Vertex를 가져온다.
				//1) Linked Vert가 없으면 : 삭제 리스트에 넣는다.
				//2) Linked Vert 중에서 "지금 선택 중인 Vert"에 해당하지 않는 Vert가 하나라도 있으면 : 삭제 리스트에 넣는다.
				//3) 모든 Linked Vert가 "지금 선택중인 Vert"에 해당된다면 유지
				List<apModifiedVertexWeight> removeVertWeights = new List<apModifiedVertexWeight>();
				for (int iVW = 0; iVW < vertWeights.Count; iVW++)
				{
					vertWeight = vertWeights[iVW];

					List<apVertex> linkedVerts = vertWeight._mesh.GetLinkedVertex(vertWeight._vertex, false);
					if (linkedVerts == null || linkedVerts.Count == 0)
					{
						//1) 연결된게 없으면 삭제
						removeVertWeights.Add(vertWeight);
						continue;
					}

					//모든 Vertex가 현재 선택된 상태인지 확인하자
					bool isAllSelected = true;
					for (int iVert = 0; iVert < linkedVerts.Count; iVert++)
					{
						bool isExist = vertWeights.Exists(delegate (apModifiedVertexWeight a)
						{
							return a._vertex == linkedVerts[iVert];
						});
						if (!isExist)
						{
							//선택되지 않은 Vertex를 발견!
							isAllSelected = false;
							break;
						}
					}
					if (!isAllSelected)
					{
						//2) 하나라도 선택되지 않은 Link Vertex가 발견되었다면 이건 삭제 대상이다.
						if (!removeVertWeights.Contains(vertWeight))
						{
							removeVertWeights.Add(vertWeight);
						}
					}
					else
					{
						//추가)
						//만약 외곽선에 위치한 Vertex라면 우선순위에 포함된다.
						bool isOutlineVertex = vertWeight._mesh.IsOutlineVertex(vertWeight._vertex);
						if (isOutlineVertex)
						{
							if (!removeVertWeights.Contains(vertWeight))
							{
								removeVertWeights.Add(vertWeight);
							}
						}
					}
				}

				if (removeVertWeights.Count > 0)
				{
					//하나씩 삭제하자

					for (int i = 0; i < removeVertWeights.Count; i++)
					{
						vertWeight = removeVertWeights[i];
						Editor.Select.RemoveModVertexOfModifier(null, null, vertWeight, vertWeight._renderVertex);
					}
				}

			}
		}

		public void SetPhysicsViscostyGroupID(int iViscosityID, bool isViscosityAdd)
		{
			if (Editor.Select.Modifier == null
				|| Editor.Select.MeshGroup == null
				|| Editor.Select.ModRenderVertListOfMod == null
				|| Editor.Select.ModRenderVertListOfMod.Count == 0
				|| Editor.Select.ExEditingMode == apSelection.EX_EDIT.None
				|| Editor.Select.ModMeshOfMod == null)
			{
				//Debug.LogError("Failed..");
				return;
			}

			apModifiedMesh modMesh = Editor.Select.ModMeshOfMod;
			List<apModifiedVertexWeight> vertWeights = new List<apModifiedVertexWeight>();
			List<apSelection.ModRenderVert> modRenderVerts = Editor.Select.ModRenderVertListOfMod;

			apSelection.ModRenderVert modRenderVert = null;
			for (int i = 0; i < modRenderVerts.Count; i++)
			{
				modRenderVert = modRenderVerts[i];
				if (modRenderVert._modVertWeight != null)
				{
					vertWeights.Add(modRenderVert._modVertWeight);
				}
			}
			if (vertWeights.Count == 0)
			{
				return;
			}

			apEditorUtil.SetRecord_Modifier(apUndoGroupData.ACTION.Modifier_SetPhysicsProperty, Editor, Editor.Select.Modifier, null, true);

			apModifiedVertexWeight vertWeight = null;
			for (int i = 0; i < vertWeights.Count; i++)
			{
				vertWeight = vertWeights[i];
				if (iViscosityID == 0)
				{
					//0으로 초기화
					vertWeight._physicParam._viscosityGroupID = 0;
				}
				else
				{
					if (isViscosityAdd)
					{
						//추가한다.
						vertWeight._physicParam._viscosityGroupID |= iViscosityID;
					}
					else
					{
						//삭제한다.
						vertWeight._physicParam._viscosityGroupID &= ~iViscosityID;

					}
				}

			}
		}

		public void ResetPhysicsValues()
		{
			if (Editor._portrait == null)
			{
				return;
			}

			for (int iMG = 0; iMG < Editor._portrait._meshGroups.Count; iMG++)
			{
				apMeshGroup meshGroup = Editor._portrait._meshGroups[iMG];

				List<apModifierBase> modifiers = meshGroup._modifierStack._modifiers;
				for (int iMod = 0; iMod < modifiers.Count; iMod++)
				{
					apModifierBase mod = modifiers[iMod];

					if (!mod.IsPhysics)
					{
						continue;
					}

					for (int iPSG = 0; iPSG < mod._paramSetGroup_controller.Count; iPSG++)
					{
						apModifierParamSetGroup paramSetGroup = mod._paramSetGroup_controller[iPSG];

						for (int iPS = 0; iPS < paramSetGroup._paramSetList.Count; iPS++)
						{
							apModifierParamSet paramSet = paramSetGroup._paramSetList[iPS];

							for (int iModMesh = 0; iModMesh < paramSet._meshData.Count; iModMesh++)
							{
								apModifiedMesh modMesh = paramSet._meshData[iModMesh];

								List<apModifiedVertexWeight> vertWeights = modMesh._vertWeights;
								for (int iVW = 0; iVW < vertWeights.Count; iVW++)
								{
									apModifiedVertexWeight vertWeight = vertWeights[iVW];
									if (vertWeight == null)
									{
										continue;
									}

									vertWeight._calculatedDeltaPos = Vector2.zero;
									vertWeight.DampPhysicVertex();
								}
							}
						}
					}

				}
			}
		}


		//----------------a----------------------------------
		// 4. 메시 작업
		//--------------------------------------------------
		public void StartMeshEdgeWork()
		{
			if (Editor.Select.Mesh == null)
			{
				return;
			}

			Editor.Select.Mesh.StartNewEdgeWork();

		}

		public void CheckMeshEdgeWorkRemained()
		{
			Editor.VertController.StopEdgeWire();
			if (Editor.Select.Mesh == null)
			{
				return;
			}

			if (Editor.Select.Mesh.IsEdgeWorking() && Editor.Select.Mesh.IsAnyWorkedEdge())
			{
				//Undo
				apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_MakeEdges, Editor, Editor.Select.Mesh, null, false);

				//bool isResult = EditorUtility.DisplayDialog("Confirm Edges", "Edge working is not completed.\nDo you want to complete it?", "Make Edges", "Remove");
				bool isResult = EditorUtility.DisplayDialog(_editor.GetText(TEXT.MeshEditChanged_Title),
																_editor.GetText(TEXT.MeshEditChanged_Body),
																_editor.GetText(TEXT.MeshEditChanged_Okay),
																_editor.GetText(TEXT.Cancel)
																);
				if (isResult)
				{
					//Editor._selection.Mesh.MakeEdgesToIndexBuffer();
					Editor.Select.Mesh.MakeEdgesToPolygonAndIndexBuffer();
					Editor.Controller.ResetAllRenderUnitsVertexIndex();//<<추가. RenderUnit에 Mesh 변경사항 반영
				}
				else
				{
					//Editor._selection.Mesh.CancelEdgeWork();
				}
			}
		}

		/// <summary>
		/// [Deprecated]
		/// </summary>
		/// <param name="volumeValue"></param>
		public void PaintVolumeValue(float volumeValue)
		{
			//이 코드는 사용하지 않습니다.
			//if(Editor.Select.Mesh == null)
			//{
			//	return;
			//}
			//apEditorUtil.SetRecord("Paint Volume Value", Editor._portrait);

			//List<apVertex> vertices = Editor.Select.Mesh._vertexData;
			//for (int i = 0; i < vertices.Count; i++)
			//{
			//	vertices[i]._volumeWeight = volumeValue / 100.0f;
			//}

			////Editor.Repaint();
			//Editor.SetRepaint();
		}

		public float GetBrushValue(float dist, float brushRadius, float value, float hardness)
		{
			if (dist > brushRadius)
			{
				return 0.0f;
			}
			if (dist < 0.0f)
			{
				dist = 0.0f;
			}

			value *= 0.01f;
			hardness *= 0.01f;

			hardness = Mathf.Clamp01(hardness);

			float softValue = 1.0f * (brushRadius - dist) / brushRadius;

			float resultValue = 1.0f * (hardness) + softValue * (1.0f - hardness);
			return resultValue * value;

		}

		//public int GetNearestBrushSizeIndex(float brushSize)
		//{
		//	int iNearest = -1;
		//	float minDiff = float.MaxValue;

		//	for (int i = 0; i < Editor._brushPreset_Size.Length; i++)
		//	{
		//		float diff = Mathf.Abs(Editor._brushPreset_Size[i] - brushSize);
		//		if (iNearest < 0 || diff < minDiff)
		//		{
		//			minDiff = diff;
		//			iNearest = i;
		//		}
		//	}
		//	return iNearest;
		//}

		//public float GetNextBrushSize(float brushSize, bool isIncrease)
		//{
		//	int iNearest = GetNearestBrushSizeIndex(brushSize);
		//	if (isIncrease)
		//	{
		//		iNearest++;
		//	}
		//	else
		//	{
		//		iNearest--;
		//	}

		//	if (iNearest >= Editor._brushPreset_Size.Length)
		//	{
		//		iNearest = Editor._brushPreset_Size.Length - 1;
		//	}
		//	else if (iNearest < 0)
		//	{
		//		iNearest = 0;
		//	}
		//	return Editor._brushPreset_Size[iNearest];
		//}

		public void RemoveSelectedMeshPolygon(object paramObject)
		{
			if (Editor._portrait == null)
			{
				return;
			}
			if (Editor.Select.SelectionType != apSelection.SELECTION_TYPE.Mesh ||
				Editor._meshEditeMode_MakeMesh != apEditor.MESH_EDIT_MODE_MAKEMESH.Polygon)
			{
				return;
			}
			if (Editor.Select.Mesh == null || Editor.VertController.Polygon == null)
			{
				return;
			}

			apEditorUtil.SetRecord_Mesh(apUndoGroupData.ACTION.MeshEdit_EditPolygons, Editor, Editor.Select.Mesh, null, false);

			Editor.Select.Mesh._polygons.Remove(Editor.VertController.Polygon);
			Editor.Select.Mesh.RefreshPolygonsToIndexBuffer();
			Editor.VertController.UnselectVertex();

			Editor.Controller.ResetAllRenderUnitsVertexIndex();
		}

		//--------------------------------------------------
		// 5. 메시 그룹 작업
		//--------------------------------------------------
		public apTransform_Mesh AddMeshToMeshGroup(apMesh addedMesh, bool isRecordAndRefresh = true)
		{
			if (Editor == null)
			{
				//EditorUtility.DisplayDialog("Error", "Adding Mesh is failed", "Close");
				EditorUtility.DisplayDialog(Editor.GetText(TEXT.MeshAttachFailed_Title),
												Editor.GetText(TEXT.MeshAttachFailed_Body),
												Editor.GetText(TEXT.Close));
				return null;
			}
			return AddMeshToMeshGroup(addedMesh, Editor.Select.MeshGroup, isRecordAndRefresh);
		}
		public apTransform_Mesh AddMeshToMeshGroup(apMesh addedMesh, apMeshGroup targetMeshGroup, bool isRecordAndRefresh = true)
		{
			if (Editor == null || targetMeshGroup == null || addedMesh == null)
			{
				return null;
			}

			if (isRecordAndRefresh)
			{
				//Undo
				apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_AttachMesh, Editor, targetMeshGroup, addedMesh, false, true);
			}

			int nSameMesh = targetMeshGroup._childMeshTransforms.FindAll(delegate (apTransform_Mesh a)
			{
				return a._meshUniqueID == addedMesh._uniqueID;
			}).Count;

			string newNickName = addedMesh._name;
			if (nSameMesh > 0)
			{
				newNickName += " (" + (nSameMesh + 1) + ")";
			}

			//int nextID = Editor._portrait.MakeUniqueID_Transform();
			int nextID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.Transform);

			if (nextID < 0)
			{
				return null;
			}
			apTransform_Mesh newMeshTransform = new apTransform_Mesh(nextID);

			newMeshTransform._meshUniqueID = addedMesh._uniqueID;
			newMeshTransform._nickName = newNickName;
			newMeshTransform._mesh = addedMesh;
			newMeshTransform._matrix = new apMatrix();
			newMeshTransform._isVisible_Default = true;

			//Depth는 가장 큰 값으로 들어간다.
			int maxDepth = targetMeshGroup.GetLastDepth();
			newMeshTransform._depth = maxDepth + 1;

			//추가 19.6.12 : Material Set 초기화
			newMeshTransform._linkedMaterialSet = Editor._portrait.GetDefaultMaterialSet();
			if(newMeshTransform._linkedMaterialSet != null)
			{
				newMeshTransform._materialSetID = newMeshTransform._linkedMaterialSet._uniqueID;
			}
			else
			{
				newMeshTransform._materialSetID = -1;
			}

			newMeshTransform._isUseDefaultMaterialSet = true;

			newMeshTransform._isCustomShader = false;
			if(newMeshTransform._customMaterialProperties == null)
			{
				newMeshTransform._customMaterialProperties = new List<apTransform_Mesh.CustomMaterialProperty>();
			}
			else
			{
				newMeshTransform._customMaterialProperties.Clear();
			}
			



			targetMeshGroup._childMeshTransforms.Add(newMeshTransform);

			if (isRecordAndRefresh)
			{
				targetMeshGroup.SetDirtyToReset();
				targetMeshGroup.RefreshForce();

				//추가 / 삭제시 요청한다.
				Editor.OnAnyObjectAddedOrRemoved();

				Editor.RefreshControllerAndHierarchy(false);
				//Editor.Repaint();
				Editor.SetRepaint();
			}
			

			return newMeshTransform;
		}

		public class AttachMeshGroupError
		{
			public bool _isError = false;
			public int _nError = 0;
			public List<apMeshGroup> _meshGroups = new List<apMeshGroup>();
			public AttachMeshGroupError()
			{
				_isError = false;
				_nError = 0;
				if(_meshGroups == null)
				{
					_meshGroups = new List<apMeshGroup>();
				}
				_meshGroups.Clear();
			}
		}

		public apTransform_MeshGroup AddMeshGroupToMeshGroup(apMeshGroup addedMeshGroup, AttachMeshGroupError error, bool isRecordAndRefresh = true)
		{
			if (Editor == null)
			{
				return null;
			}
			return AddMeshGroupToMeshGroup(addedMeshGroup, Editor.Select.MeshGroup, error, isRecordAndRefresh);
		}

		public apTransform_MeshGroup AddMeshGroupToMeshGroup(apMeshGroup addedMeshGroup, apMeshGroup targetMeshGroup, AttachMeshGroupError error, bool isRecordAndRefresh = true)
		{
			if (Editor == null || targetMeshGroup == null || addedMeshGroup == null)
			{
				return null;
			}


			bool isExist = targetMeshGroup._childMeshGroupTransforms.Exists(delegate (apTransform_MeshGroup a)
			{
				return a._meshGroupUniqueID == addedMeshGroup._uniqueID;
			});

			if (isExist)
			{
				return null;
			}

			if (isRecordAndRefresh)
			{
				//Undo
				apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_AttachMeshGroup, Editor, targetMeshGroup, addedMeshGroup, false, true);
			}


			string newNickName = addedMeshGroup._name;

			//int nextID = Editor._portrait.MakeUniqueID_Transform();
			int nextID = Editor._portrait.MakeUniqueID(apIDManager.TARGET.Transform);
			if (nextID < 0)
			{
				return null;
			}

			apTransform_MeshGroup newMeshGroupTransform = new apTransform_MeshGroup(nextID);

			newMeshGroupTransform._meshGroupUniqueID = addedMeshGroup._uniqueID;
			newMeshGroupTransform._nickName = newNickName;
			newMeshGroupTransform._meshGroup = addedMeshGroup;
			newMeshGroupTransform._matrix = new apMatrix();
			newMeshGroupTransform._isVisible_Default = true;

			//Depth는 가장 큰 값으로 들어간다.
			int maxDepth = targetMeshGroup.GetLastDepth();
			newMeshGroupTransform._depth = maxDepth + 1;

			targetMeshGroup._childMeshGroupTransforms.Add(newMeshGroupTransform);

			newMeshGroupTransform._meshGroup._parentMeshGroup = targetMeshGroup;
			newMeshGroupTransform._meshGroup._parentMeshGroupID = targetMeshGroup._uniqueID;


			//추가 19.8.20 : 추가되는 MeshGroup에 (Animation) 모디파이어가 있다면 경고 문구를 띄워야 한다.
			if (error != null)
			{
				if (addedMeshGroup._modifierStack != null && addedMeshGroup._modifierStack._modifiers != null)
				{
					bool isAnyAnimModifier = false;
					for (int iMod = 0; iMod < addedMeshGroup._modifierStack._modifiers.Count; iMod++)
					{
						apModifierBase subModifier = addedMeshGroup._modifierStack._modifiers[iMod];
						if (subModifier == null)
						{
							continue;
						}

						if(subModifier.IsAnimated)
						{
							isAnyAnimModifier = true;
							break;
						}
					}
					if(isAnyAnimModifier)
					{
						error._isError = true;
						error._nError++;
						error._meshGroups.Add(addedMeshGroup);
					}
				}
			}

			//버그 수정 19.8.20 : 추가된 MeshGroup이 루트 유닛인 경우 > RootUnit에서 삭제
			apRootUnit linkedRootUnit = Editor._portrait._rootUnits.Find(delegate(apRootUnit a)
			{
				return a._childMeshGroup == addedMeshGroup;
			});
			if(linkedRootUnit != null)
			{
				Editor._portrait._mainMeshGroupIDList.Remove(addedMeshGroup._uniqueID);
				Editor._portrait._mainMeshGroupList.Remove(addedMeshGroup);
				Editor._portrait._rootUnits.Remove(linkedRootUnit);
			}


			

			if (isRecordAndRefresh)
			{
				targetMeshGroup.SetDirtyToReset();
				//targetMeshGroup.SetAllRenderUnitForceUpdate();
				targetMeshGroup.RefreshForce();

				//추가 / 삭제시 요청한다.
				Editor.OnAnyObjectAddedOrRemoved();

				Editor.Hierarchy.SetNeedReset();
				Editor.RefreshControllerAndHierarchy(false);
				//Editor.Repaint();
				Editor.SetRepaint();
			}

			return newMeshGroupTransform;
		}




		public bool AddClippingMeshTransform(apMeshGroup meshGroup, apTransform_Mesh meshTransform, bool isShowErrorDialog, bool isRecordAndRefresh = true)
		{
			if (meshGroup == null || meshTransform == null)
			{
				return false;
			}

			//Parent도 Clip을 지정한 뒤 -> Refresh만 잘 하면 된다.
			if (meshGroup.GetMeshTransform(meshTransform._transformUniqueID) == null)
			{
				//해당 메시 그룹에 존재하지 않는 트랜스폼이다.

				if (meshGroup.GetMeshTransformRecursive(meshTransform._transformUniqueID) == null)
				{
					//Debug.LogError("해당 메시 그룹에 존재하지 않는 트랜스폼이다. -> Child 에도 없다. 끝");
					return false;
				}
			}

			if (isRecordAndRefresh)
			{
				apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_ClippingChanged, Editor, meshGroup, null, false, true);
			}


			//이미 Clip 상태이면 패스
			//if(meshTransform._isClipping_Child || meshTransform._isClipping_Parent)
			//{
			//	return false;
			//}

			//속성 바꾸고 자동으로 Sort 및 Clipping 연결
			meshTransform._isClipping_Child = true;
			meshGroup.SetDirtyToSort();
			meshGroup.SortRenderUnits(true);

			meshGroup.RefreshForce();
			Editor.Hierarchy_MeshGroup.RefreshUnits();
			return true;
		}



		public void ReleaseClippingMeshTransform(apMeshGroup meshGroup, apTransform_Mesh meshTransform)
		{
			if (meshGroup == null || meshTransform == null)
			{
				return;
			}

			if (meshGroup.GetMeshTransform(meshTransform._transformUniqueID) == null)
			{
				//해당 메시 그룹에 존재하지 않는 트랜스폼이다.
				if (meshGroup.GetMeshTransformRecursive(meshTransform._transformUniqueID) == null)
				{
					return;
				}
			}

			apEditorUtil.SetRecord_MeshGroup(apUndoGroupData.ACTION.MeshGroup_ClippingChanged, Editor, meshGroup, null, false, true);

			meshTransform._isClipping_Child = false;
			meshTransform._clipIndexFromParent = -1;
			meshTransform._clipParentMeshTransform = null;

			meshGroup.SetDirtyToSort();
			meshGroup.SortRenderUnits(true);
			meshGroup.RefreshForce();
			Editor.Hierarchy_MeshGroup.RefreshUnits();
		}


		/// <summary>
		/// 작업시 TmpWorkVisible이 각 Transform에 저장되어있다. 이값을 초기화(true)한다.
		/// 에디터 옵션에 따라서 이 함수는 무시될 수 있다. (인자에 false입력시..)
		/// </summary>
		/// <param name="meshGroup"></param>
		public void SetMeshGroupTmpWorkVisibleReset(apMeshGroup meshGroup, bool isForceWOEditorOption, bool isTargetMeshes, bool isTargetBones)
		{
			if (meshGroup == null)
			{
				return;
			}

			if(!isForceWOEditorOption && !Editor._isRestoreTempMeshVisibilityWhenTackEnded)
			{
				//Visibility 복구 옵션이 false인 경우
				meshGroup.RefreshForce();//<<RefeshForce만 실행하자

				//일단 검사를 하자
				CheckTmpWorkVisible(meshGroup);

				return;
			}

			if (isTargetMeshes)
			{
				for (int i = 0; i < meshGroup._renderUnits_All.Count; i++)
				{
					meshGroup._renderUnits_All[i].ResetTmpWorkVisible();
				}
			}

			if (isTargetBones)
			{
				if (meshGroup._boneListSets != null && meshGroup._boneListSets.Count > 0)
				{
					List<apBone> boneRootList = null;
					for (int iBontSet = 0; iBontSet < meshGroup._boneListSets.Count; iBontSet++)
					{
						boneRootList = meshGroup._boneListSets[iBontSet]._bones_Root;
						if (boneRootList != null && boneRootList.Count > 0)
						{
							for (int iBone = 0; iBone < boneRootList.Count; iBone++)
							{
								boneRootList[iBone].ResetGUIVisibleRecursive();
							}
						}
					}
				}
			}
			
			//TmpWorkVisible 이 초기화되었다.
			if(isTargetMeshes && isTargetBones)
			{
				Editor.Select.SetTmpWorkVisibleChanged(false, false);
			}
			else
			{
				//일부만 되었다면 검사를 하자
				CheckTmpWorkVisible(meshGroup);
			}
			

			//SetMeshGoupTransformTmpWorkVisibleReset(meshGroup._rootMeshGroupTransform);
			meshGroup.RefreshForce();
		}

		/// <summary>
		/// 작업시 TmpWorkVisible을 일괄 적용한다.
		/// 옵션으로 "제외 대상"을 하나 받을 수 있다.
		/// </summary>
		/// <param name="meshGroup"></param>
		public void SetMeshGroupTmpWorkVisibleAll(apMeshGroup meshGroup, bool isVisibleTmpWork, apRenderUnit exceptTarget)
		{
			if (meshGroup == null)
			{
				return;
			}

			apRenderUnit renderUnit = null;
			for (int i = 0; i < meshGroup._renderUnits_All.Count; i++)
			{
				renderUnit = meshGroup._renderUnits_All[i];
				if (renderUnit == exceptTarget)
				{
					continue;
				}
				if (renderUnit == meshGroup._rootRenderUnit)
				{
					continue;
				}
				if (renderUnit._unitType == apRenderUnit.UNIT_TYPE.GroupNode)
				{
					//GroupNode는 토글되지 않는다.
					continue;
				}
				if (isVisibleTmpWork == renderUnit._isVisible_WithoutParent)
				{
					//현재 Visible이 같으면 토글하지 않는다.
					continue;
				}

				//의도한 Visible 값이 아니다.
				//토글하자
				if (renderUnit._isVisible_WithoutParent == renderUnit._isVisibleCalculated)
				{
					if (isVisibleTmpWork)
					{
						//Hide -> Show
						renderUnit._isVisibleWorkToggle_Hide2Show = true;
						renderUnit._isVisibleWorkToggle_Show2Hide = false;
					}
					else
					{
						//Show -> Hide
						renderUnit._isVisibleWorkToggle_Hide2Show = false;
						renderUnit._isVisibleWorkToggle_Show2Hide = true;
					}
				}
				else
				{
					renderUnit._isVisibleWorkToggle_Hide2Show = false;
					renderUnit._isVisibleWorkToggle_Show2Hide = false;
				}
			}

			meshGroup.RefreshForce();


			//TmpWorkVisible 여부 체크 > 이 함수가 호출될 때 외부에서 같이 호출되더라
			//CheckTmpWorkVisible(meshGroup);
		}

		/// <summary>
		/// MeshGroup작업시 TmpWorkVisible이 변경된 것(Mesh, Bone)이 있는지 확인한다.
		/// TmpWorkVisible을 변경할 경우 이 함수를 꼭 호출해주자.
		/// </summary>
		/// <param name="meshGroup"></param>
		public void CheckTmpWorkVisible(apMeshGroup meshGroup)
		{
			if (meshGroup == null)
			{
				return;
			}

			bool isAnyMeshChanged = false;
			bool isAnyBoneChanged = false;

			apRenderUnit renderUnit = null;
			apBone bone = null;

			for (int i = 0; i < meshGroup._renderUnits_All.Count; i++)
			{
				renderUnit = meshGroup._renderUnits_All[i];
				if(renderUnit._isVisibleWorkToggle_Hide2Show ||
					renderUnit._isVisibleWorkToggle_Show2Hide)
				{
					//변경된 메시가 있다.
					isAnyMeshChanged = true;
					break;
				}
			}

			if(meshGroup._boneListSets != null && meshGroup._boneListSets.Count > 0)
			{
				List<apBone> boneList = null;
				for (int iBontSet = 0; iBontSet < meshGroup._boneListSets.Count; iBontSet++)
				{
					boneList = meshGroup._boneListSets[iBontSet]._bones_All;
					if(boneList != null && boneList.Count > 0)
					{
						for (int iBone = 0; iBone < boneList.Count; iBone++)
						{
							bone = boneList[iBone];
							if(!bone.IsGUIVisible)
							{
								//보여지지 않는 본이 있다.
								isAnyBoneChanged = true;
								break;
							}
						}
					}

					if(isAnyBoneChanged)
					{
						break;
					}

				}
			}

			Editor.Select.SetTmpWorkVisibleChanged(isAnyMeshChanged, isAnyBoneChanged);
		}



		//--------------------------------------------------
		// 메시 그룹의 Modifier 작업
		//--------------------------------------------------
		public void AddModifier(apModifierBase.MODIFIER_TYPE _type, int validationKey)
		{
			if (Editor._portrait == null || Editor.Select.MeshGroup == null)
			{
				return;
			}

			//ObjectGroup을 체크하여 만들어주자
			CheckAndMakeObjectGroup();


			//Undo
			//apEditorUtil.SetRecord_MeshGroupAllModifiers(apUndoGroupData.ACTION.MeshGroup_AddModifier, Editor, Editor.Select.MeshGroup, null, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Create Modifier");

			apModifierStack modStack = Editor.Select.MeshGroup._modifierStack;
			int newID = modStack.GetNewModifierID((int)_type, validationKey);
			if (newID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Modifier Adding Failed. Please Retry", "Close");

				EditorUtility.DisplayDialog(Editor.GetText(TEXT.ModifierAddFailed_Title),
												Editor.GetText(TEXT.ModifierAddFailed_Body),
												Editor.GetText(TEXT.Close));
				return;
			}

			//GameObject로 만드는 경우
			GameObject newGameObj = null;



			int newLayer = modStack.GetLastLayer() + 1;
			apModifierBase newModifier = null;
			switch (_type)
			{
				case apModifierBase.MODIFIER_TYPE.Base:
					newGameObj = new GameObject("Modifier - Base");
					//newModifier = new apModifierBase();//<<이건 처리하지 않습니다... 사실은;
					newModifier = newGameObj.AddComponent<apModifierBase>();
					break;

				case apModifierBase.MODIFIER_TYPE.Volume:
					newGameObj = new GameObject("Modifier - Volume");
					//newModifier = new apModifier_Volume();
					newModifier = newGameObj.AddComponent<apModifier_Volume>();
					break;

				case apModifierBase.MODIFIER_TYPE.Morph:
					newGameObj = new GameObject("Modifier - Morph");
					//newModifier = new apModifier_Morph();
					newModifier = newGameObj.AddComponent<apModifier_Morph>();
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
					newGameObj = new GameObject("Modifier - AnimatedMorph");
					//newModifier = new apModifier_AnimatedMorph();
					newModifier = newGameObj.AddComponent<apModifier_AnimatedMorph>();
					break;

				case apModifierBase.MODIFIER_TYPE.Rigging:
					newGameObj = new GameObject("Modifier - Rigging");
					//newModifier = new apModifier_Rigging();
					newModifier = newGameObj.AddComponent<apModifier_Rigging>();
					break;
				case apModifierBase.MODIFIER_TYPE.Physic:
					newGameObj = new GameObject("Modifier - Physic");
					//newModifier = new apModifier_Physic();
					newModifier = newGameObj.AddComponent<apModifier_Physic>();
					break;

				case apModifierBase.MODIFIER_TYPE.TF:
					newGameObj = new GameObject("Modifier - TF");
					//newModifier = new apModifier_TF();
					newModifier = newGameObj.AddComponent<apModifier_TF>();
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedTF:
					newGameObj = new GameObject("Modifier - AnimatedTF");
					//newModifier = new apModifier_AnimatedTF();
					newModifier = newGameObj.AddComponent<apModifier_AnimatedTF>();
					break;

				case apModifierBase.MODIFIER_TYPE.FFD:
					newGameObj = new GameObject("Modifier - FFD");
					//newModifier = new apModifier_FFD();
					newModifier = newGameObj.AddComponent<apModifier_FFD>();
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
					newGameObj = new GameObject("Modifier - AnimatedFFD");
					//newModifier = new apModifier_AnimatedFFD();
					newModifier = newGameObj.AddComponent<apModifier_AnimatedFFD>();
					break;

				default:
					Debug.LogError("TODO : 정의되지 않은 타입 [" + _type + "]");
					break;
			}


			newGameObj.transform.parent = Editor._portrait._subObjectGroup_Modifier.transform;
			newGameObj.transform.localPosition = Vector3.zero;
			newGameObj.transform.localRotation = Quaternion.identity;
			newGameObj.transform.localScale = Vector3.one;
			newGameObj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;




			newModifier.LinkPortrait(Editor._portrait);
			newModifier.SetInitSetting(newID, newLayer, Editor.Select.MeshGroup._uniqueID, Editor.Select.MeshGroup);

			//추가 : 보간 방식 기본값을 Interpolation에서 Additive로 변경
			newModifier._blendMethod = apModifierBase.BLEND_METHOD.Additive;

			modStack.AddModifier(newModifier, _type);

			modStack.RefreshAndSort(true);//<Sort!


			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor.Select.SetModifier(newModifier);
			Editor.RefreshControllerAndHierarchy(false);
			Editor.SetRepaint();

			//Undo - Create 추가
			apEditorUtil.SetRecordCreateMonoObject(newModifier, "Create Modifier");

			//프리팹이었다면 Apply
			apEditorUtil.SetPortraitPrefabApply(Editor._portrait);
		}



		public apModifierBase AddModifierToMeshGroup(apMeshGroup targetMeshGroup, apModifierBase.MODIFIER_TYPE _type, int validationKey, bool isRecord)
		{
			if (Editor._portrait == null || targetMeshGroup == null)
			{
				return null;
			}

			//ObjectGroup을 체크하여 만들어주자
			CheckAndMakeObjectGroup();


			//Undo
			if (isRecord)
			{
				apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Create Modifier");
			}

			apModifierStack modStack = targetMeshGroup._modifierStack;
			int newID = modStack.GetNewModifierID((int)_type, validationKey);
			if (newID < 0)
			{
				//EditorUtility.DisplayDialog("Error", "Modifier Adding Failed. Please Retry", "Close");

				EditorUtility.DisplayDialog(Editor.GetText(TEXT.ModifierAddFailed_Title),
												Editor.GetText(TEXT.ModifierAddFailed_Body),
												Editor.GetText(TEXT.Close));
				return null;
			}

			//GameObject로 만드는 경우
			GameObject newGameObj = null;



			int newLayer = modStack.GetLastLayer() + 1;
			apModifierBase newModifier = null;
			switch (_type)
			{
				case apModifierBase.MODIFIER_TYPE.Base:
					newGameObj = new GameObject("Modifier - Base");
					//newModifier = new apModifierBase();//<<이건 처리하지 않습니다... 사실은;
					newModifier = newGameObj.AddComponent<apModifierBase>();
					break;

				case apModifierBase.MODIFIER_TYPE.Volume:
					newGameObj = new GameObject("Modifier - Volume");
					//newModifier = new apModifier_Volume();
					newModifier = newGameObj.AddComponent<apModifier_Volume>();
					break;

				case apModifierBase.MODIFIER_TYPE.Morph:
					newGameObj = new GameObject("Modifier - Morph");
					//newModifier = new apModifier_Morph();
					newModifier = newGameObj.AddComponent<apModifier_Morph>();
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedMorph:
					newGameObj = new GameObject("Modifier - AnimatedMorph");
					//newModifier = new apModifier_AnimatedMorph();
					newModifier = newGameObj.AddComponent<apModifier_AnimatedMorph>();
					break;

				case apModifierBase.MODIFIER_TYPE.Rigging:
					newGameObj = new GameObject("Modifier - Rigging");
					//newModifier = new apModifier_Rigging();
					newModifier = newGameObj.AddComponent<apModifier_Rigging>();
					break;
				case apModifierBase.MODIFIER_TYPE.Physic:
					newGameObj = new GameObject("Modifier - Physic");
					//newModifier = new apModifier_Physic();
					newModifier = newGameObj.AddComponent<apModifier_Physic>();
					break;

				case apModifierBase.MODIFIER_TYPE.TF:
					newGameObj = new GameObject("Modifier - TF");
					//newModifier = new apModifier_TF();
					newModifier = newGameObj.AddComponent<apModifier_TF>();
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedTF:
					newGameObj = new GameObject("Modifier - AnimatedTF");
					//newModifier = new apModifier_AnimatedTF();
					newModifier = newGameObj.AddComponent<apModifier_AnimatedTF>();
					break;

				case apModifierBase.MODIFIER_TYPE.FFD:
					newGameObj = new GameObject("Modifier - FFD");
					//newModifier = new apModifier_FFD();
					newModifier = newGameObj.AddComponent<apModifier_FFD>();
					break;

				case apModifierBase.MODIFIER_TYPE.AnimatedFFD:
					newGameObj = new GameObject("Modifier - AnimatedFFD");
					//newModifier = new apModifier_AnimatedFFD();
					newModifier = newGameObj.AddComponent<apModifier_AnimatedFFD>();
					break;

				default:
					Debug.LogError("TODO : 정의되지 않은 타입 [" + _type + "]");
					break;
			}


			newGameObj.transform.parent = Editor._portrait._subObjectGroup_Modifier.transform;
			newGameObj.transform.localPosition = Vector3.zero;
			newGameObj.transform.localRotation = Quaternion.identity;
			newGameObj.transform.localScale = Vector3.one;
			newGameObj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			

			newModifier.LinkPortrait(Editor._portrait);
			newModifier.SetInitSetting(newID, newLayer, targetMeshGroup._uniqueID, targetMeshGroup);

			//추가 : 보간 방식 기본값을 Interpolation에서 Additive로 변경
			newModifier._blendMethod = apModifierBase.BLEND_METHOD.Additive;

			modStack.AddModifier(newModifier, _type);

			modStack.RefreshAndSort(true);//<Sort!


			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();
			
			//Undo - Create 추가
			apEditorUtil.SetRecordCreateMonoObject(newModifier, "Create Modifier");

			//프리팹이었다면 Apply
			apEditorUtil.SetPortraitPrefabApply(Editor._portrait);

			return newModifier;
		}


		/// <summary>
		/// 현재의 Modifier에 선택한 Transform을 ModMesh로 등록한다.
		/// </summary>
		public void AddModMesh_WithSubMeshOrSubMeshGroup()
		{
			if (Editor.Select.SubEditedParamSetGroup == null || Editor.Select.Modifier == null || Editor.Select.MeshGroup == null)
			{
				return;
			}

			//apModifiedMesh addedModMesh = null;
			List<apModifierParamSet> modParamSetList = Editor.Select.SubEditedParamSetGroup._paramSetList;

			if (Editor.Select.SubMeshInGroup != null)
			{
				for (int iParam = 0; iParam < modParamSetList.Count; iParam++)
				{
					//addedModMesh = Editor.Select.Modifier.AddMeshTransform(Editor.Select.MeshGroup, Editor.Select.SubMeshInGroup, modParamSetList[iParam]);
					Editor.Select.Modifier.AddMeshTransform(Editor.Select.MeshGroup,
						Editor.Select.SubMeshInGroup,
						modParamSetList[iParam],
						false,
						Editor.Select.Modifier.IsTarget_ChildMeshTransform,
						true);
				}
			}
			else if (Editor.Select.SubMeshGroupInGroup != null)
			{
				for (int iParam = 0; iParam < modParamSetList.Count; iParam++)
				{
					//addedModMesh = Editor.Select.Modifier.AddMeshGroupTransform(Editor.Select.MeshGroup, Editor.Select.SubMeshGroupInGroup, modParamSetList[iParam]);
					Editor.Select.Modifier.AddMeshGroupTransform(Editor.Select.MeshGroup, Editor.Select.SubMeshGroupInGroup, modParamSetList[iParam], false, true);
				}
			}

			bool isChanged = Editor.Select.SubEditedParamSetGroup.RefreshSync();
			if (isChanged)
			{
				Editor.Select.MeshGroup.LinkModMeshRenderUnits();//<<Link 전에 이걸 먼저 선언한다.
				Editor.Select.MeshGroup.RefreshModifierLink();
			}

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor.Select.AutoSelectModMeshOrModBone();
			//추가 : ExMode에 추가한다.
			Editor.Select.RefreshMeshGroupExEditingFlags(
								Editor.Select.MeshGroup,
								Editor.Select.Modifier,
								Editor.Select.SubEditedParamSetGroup,
								null,
								true);
		}

		/// <summary>
		/// 현재의 Modifier와 선택한 Bone을 연동하여 ModBone을 생성하여 추가한다.
		/// </summary>
		public void AddModBone_WithSelectedBone()
		{
			if (Editor.Select.SubEditedParamSetGroup == null || Editor.Select.Modifier == null || Editor.Select.MeshGroup == null)
			{
				return;
			}
			if (Editor.Select.Bone == null)
			{
				return;
			}
			apBone bone = Editor.Select.Bone;

			List<apModifierParamSet> modParamSetList = Editor.Select.SubEditedParamSetGroup._paramSetList;

			for (int iParam = 0; iParam < modParamSetList.Count; iParam++)
			{
				Editor.Select.Modifier.AddBone(bone, modParamSetList[iParam], false, true);
			}

			bool isChanged = Editor.Select.SubEditedParamSetGroup.RefreshSync();
			if (isChanged)
			{
				Editor.Select.MeshGroup.LinkModMeshRenderUnits();//<<Link 전에 이걸 먼저 선언한다.
				Editor.Select.MeshGroup.RefreshModifierLink();
			}

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			//TODO : 원래는 AutoSelectModMesh가 들어와야 하지만 -> 여기서는 ModBone이 선택되어야 한다.
			//ModBone이 apSelection에 존재하도록 설정할 것
			//Editor.Select.AutoSelectModMesh();
			Editor.Select.AutoSelectModMeshOrModBone();

			//추가 : ExMode에 추가한다.
			Editor.Select.RefreshMeshGroupExEditingFlags(
								Editor.Select.MeshGroup,
								Editor.Select.Modifier,
								Editor.Select.SubEditedParamSetGroup,
								null,
								true);
		}

		public void AddControlParamToModifier(apControlParam controlParam, bool isUseCurrentKey = true, bool isAddCurrentMeshToParamSet = true)
		{
			//이 ControlParam에 해당하는 ParamSetGroup이 있는지 체크한다.
			if (Editor.Select.Modifier == null)
			{
				Debug.LogError("AddControlParamToModifier -> No Modifier");
				return;
			}

			//Undo
			apEditorUtil.SetRecord_MeshGroupAndModifier(apUndoGroupData.ACTION.Modifier_LinkControlParam, Editor, Editor.Select.Modifier._meshGroup, Editor.Select.Modifier, controlParam, false);


			apModifierParamSetGroup paramSetGroup = Editor.Select.Modifier._paramSetGroup_controller.Find(delegate (apModifierParamSetGroup a)
			{
				return a._syncTarget == apModifierParamSetGroup.SYNC_TARGET.Controller &&
						a._keyControlParam == controlParam;
			});


			apModifierParamSet targetParamSet = null;

			if (paramSetGroup == null)
			{
				//없다면 만들어주자
				paramSetGroup = new apModifierParamSetGroup(Editor._portrait, Editor.Select.Modifier, Editor.Select.Modifier.GetNextParamSetLayerIndex());
				paramSetGroup.SetController(controlParam);

				Editor.Select.Modifier._paramSetGroup_controller.Add(paramSetGroup);
				Editor.Select.SetParamSetGroupOfModifier(paramSetGroup);
			}
			else
			{
				//추가 3.22
				//있다면 >> 만약 Key를 추가할 때 목표 Key와 동일한 Key가 이미 있다면 불가
				if (paramSetGroup._paramSetList != null && paramSetGroup._paramSetList.Count > 0)
				{
					//"현재"와 동일한 키가 있는지 확인하자
					//또는 기본값과 동일한지 체크
					apModifierParamSet existedParamSet = null;
					for (int i = 0; i < paramSetGroup._paramSetList.Count; i++)
					{
						existedParamSet = paramSetGroup._paramSetList[i];
						//ParamSet으로 새로 추가하고자 하는 컨트롤러의 목표 값(현재 또는 기본값)이 이미 등록된 ParamSet의 값과 같다면
						//이 값으로 ParamSet으로 새로 추가하면 안된다.
						bool isSameKey = false;
						float bias = 0.001f;

						switch (controlParam._valueType)
						{
							case apControlParam.TYPE.Int:
								if (isUseCurrentKey)
								{
									isSameKey = existedParamSet._conSyncValue_Int == controlParam._int_Cur;
								}
								else
								{
									isSameKey = existedParamSet._conSyncValue_Int == controlParam._int_Def;
								}
								break;
							case apControlParam.TYPE.Float:
								if (controlParam._snapSize > 0)
								{
									//Bias를 결정
									bias = Mathf.Min(bias, Mathf.Abs((controlParam._float_Max - controlParam._float_Min) / controlParam._snapSize) * 0.2f);
								}

								if (isUseCurrentKey)
								{
									isSameKey = Mathf.Abs(existedParamSet._conSyncValue_Float - controlParam._float_Cur) < bias;
								}
								else
								{
									isSameKey = Mathf.Abs(existedParamSet._conSyncValue_Float - controlParam._float_Def) < bias;
								}
								break;
							case apControlParam.TYPE.Vector2:
								if (controlParam._snapSize > 0)
								{
									//Bias를 결정
									bias = Mathf.Min(bias,
										Mathf.Abs((controlParam._vec2_Max.x - controlParam._vec2_Min.x) / controlParam._snapSize) * 0.2f,
										Mathf.Abs((controlParam._vec2_Max.y - controlParam._vec2_Min.y) / controlParam._snapSize) * 0.2f
										);
								}

								if (isUseCurrentKey)
								{
									//X, Y 좌표가 모두 같아야함
									isSameKey = Mathf.Abs(existedParamSet._conSyncValue_Vector2.x - controlParam._vec2_Cur.x) < bias
												&& Mathf.Abs(existedParamSet._conSyncValue_Vector2.y - controlParam._vec2_Cur.y) < bias;
								}
								else
								{
									//X, Y 좌표가 모두 같아야함
									isSameKey = Mathf.Abs(existedParamSet._conSyncValue_Vector2.x - controlParam._vec2_Def.x) < bias
												&& Mathf.Abs(existedParamSet._conSyncValue_Vector2.y - controlParam._vec2_Def.y) < bias;
								}
								break;
						}

						if (isSameKey)
						{
							//키가 같다 => 새로 ParamSet을 추가할 필요가 없다.
							targetParamSet = existedParamSet;
							//Debug.LogError("동일한 값의 Key가 이미 추가되어 있다! 3.22");
							break;
						}
					}

				}
			}

			//추가 3.22 : 무조건 Key (ParamSet)를 생성하지 말고, 기존과 겹친다면 패스
			if (targetParamSet == null)
			{
				//Key를 추가하자

				apModifierParamSet newParamSet = new apModifierParamSet();

				newParamSet.LinkParamSetGroup(paramSetGroup);//Link도 해준다.
															 //newParamSet.SetController(controlParam);
				switch (controlParam._valueType)
				{
					//case apControlParam.TYPE.Bool:
					//	newParamSet._conSyncValue_Bool = controlParam._bool_Cur;
					//	break;

					case apControlParam.TYPE.Int:
						if (isUseCurrentKey)
						{
							newParamSet._conSyncValue_Int = controlParam._int_Cur;
						}
						else
						{
							//추가 3.22 : 기본값으로 생성하는 기능
							newParamSet._conSyncValue_Int = controlParam._int_Def;
						}

						break;

					case apControlParam.TYPE.Float:
						if (isUseCurrentKey)
						{
							newParamSet._conSyncValue_Float = controlParam._float_Cur;
						}
						else
						{
							//추가 3.22 : 기본값으로 생성하는 기능
							newParamSet._conSyncValue_Float = controlParam._float_Def;
						}

						break;

					case apControlParam.TYPE.Vector2:
						if (isUseCurrentKey)
						{
							newParamSet._conSyncValue_Vector2 = controlParam._vec2_Cur;
						}
						else
						{
							//추가 3.22 : 기본값으로 생성하는 기능
							newParamSet._conSyncValue_Vector2 = controlParam._vec2_Def;
						}

						break;
				}


				paramSetGroup._paramSetList.Add(newParamSet);
				paramSetGroup.RefreshSync();

				targetParamSet = newParamSet;
			}


			//만약, 현재 선택중인 Mesh나 MeshGroup이 ModMesh로서 ParamSetList에 없다면 추가해주는 것도 좋을 것 같다.
			//추가 3.22 : 현재는 무조건 추가지만, isAddCurrentMeshToParamSet의 값이 true인 경우에 한해서만..
			if (isAddCurrentMeshToParamSet)
			{
				bool isAddedModMesh = false;
				bool isAnyTransformSelected = false;

				bool isAddedModBone = false;
				bool isAnyBoneSelected = false;
				if (Editor.Select.SubMeshInGroup != null)
				{
					isAnyTransformSelected = true;
					isAddedModMesh = paramSetGroup.IsSubMeshInGroup(Editor.Select.SubMeshInGroup);
				}
				else if (Editor.Select.SubMeshGroupInGroup != null)
				{
					isAnyTransformSelected = true;
					isAddedModMesh = paramSetGroup.IsSubMeshGroupInGroup(Editor.Select.SubMeshGroupInGroup);
				}
				else if (Editor.Select.Bone != null && Editor.Select.Modifier.IsTarget_Bone)
				{
					isAnyBoneSelected = true;
					isAddedModBone = paramSetGroup.IsBoneContain(Editor.Select.Bone);
				}

				if (!isAddedModMesh && isAnyTransformSelected)
				{
					//Debug.Log("Auto Make ModMesh");
					//자동으로 Start Edit 버튼 누른 것 같은 효과를 주자
					AddModMesh_WithSubMeshOrSubMeshGroup();//<<이 함수는 Start Edit에도 있다.
					paramSetGroup.RefreshSync();
				}
				else if (!isAddedModBone && isAnyBoneSelected)
				{
					AddModBone_WithSelectedBone();
					paramSetGroup.RefreshSync();
				}
			}

			Editor.Select.SetParamSetGroupOfModifier(paramSetGroup);
			Editor.Select.SetParamSetOfModifier(targetParamSet);

			Editor.Select.Modifier.RefreshParamSet();

			Editor.Select.MeshGroup.LinkModMeshRenderUnits();//<<Link 전에 이걸 먼저 선언한다.
			Editor.Select.MeshGroup.RefreshModifierLink();

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			//추가 : ExMode에 추가한다.
			Editor.Select.RefreshMeshGroupExEditingFlags(
								Editor.Select.MeshGroup,
								Editor.Select.Modifier,
								paramSetGroup,
								null,
								true);

			Editor.SetRepaint();
		}


		/// <summary>
		/// Static 타입 (아무런 입력 연동값이 없음)의 ParamSetGroup을 Modifier에 등록한다.
		/// Static 타입은 ParamSetGroup(연동 오브젝트) 1개와 ParamSet(키값) 1개만 가진다. (그 아래 ModifiedMesh를 여러개 가진다)
		/// </summary>
		public void AddStaticParamSetGroupToModifier()
		{
			//이 ControlParam에 해당하는 ParamSetGroup이 있는지 체크한다.
			if (Editor.Select.Modifier == null)
			{
				Debug.LogError("AddStaticParamSetGroupToModifier -> No Modifier");
				return;
			}

			//Undo
			apEditorUtil.SetRecord_MeshGroupAndModifier(apUndoGroupData.ACTION.Modifier_AddStaticParamSetGroup, Editor, Editor.Select.Modifier._meshGroup, Editor.Select.Modifier, null, false);


			if (Editor.Select.Modifier._paramSetGroup_controller.Count > 0)
			{
				//Static 타입은 한개의 ParamSetGroup만 적용한다.
				return;
			}

			apModifierParamSetGroup paramSetGroup = new apModifierParamSetGroup(Editor._portrait, Editor.Select.Modifier, Editor.Select.Modifier.GetNextParamSetLayerIndex());
			paramSetGroup.SetStatic();//<Static 타입

			Editor.Select.Modifier._paramSetGroup_controller.Add(paramSetGroup);
			Editor.Select.SetParamSetGroupOfModifier(paramSetGroup);



			//Static 타입은 한개의 ParamSet을 가진다.

			apModifierParamSet newParamSet = new apModifierParamSet();

			newParamSet.LinkParamSetGroup(paramSetGroup);//Link도 해준다.
			paramSetGroup._paramSetList.Add(newParamSet);
			paramSetGroup.RefreshSync();


			Editor.Select.SetParamSetGroupOfModifier(paramSetGroup);
			Editor.Select.SetParamSetOfModifier(newParamSet);

			Editor.Select.Modifier.RefreshParamSet();

			Editor.Select.MeshGroup.LinkModMeshRenderUnits();//<<Link 전에 이걸 먼저 선언한다.
			Editor.Select.MeshGroup.RefreshModifierLink();

			//추가 : ExMode에 추가한다.
			Editor.Select.RefreshMeshGroupExEditingFlags(
								Editor.Select.MeshGroup,
								Editor.Select.Modifier,
								paramSetGroup,
								null,
								true);

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor.SetRepaint();
		}



		public void LayerChange(apModifierBase modifier, bool isLayerUp)
		{
			if (Editor._portrait == null || Editor.Select.MeshGroup == null)
			{ return; }

			apModifierStack modStack = Editor.Select.MeshGroup._modifierStack;

			if (!modStack._modifiers.Contains(modifier))
			{ return; }


			//Undo를 기록하자
			apEditorUtil.SetRecord_MeshGroupAllModifiers(apUndoGroupData.ACTION.Modifier_LayerChanged, Editor, Editor.Select.MeshGroup, null, false);


			modStack.RefreshAndSort(false);

			int prevIndex = modStack._modifiers.IndexOf(modifier);
			int nextIndex = prevIndex;
			if (isLayerUp)
			{
				nextIndex++;

				if (nextIndex >= modStack._modifiers.Count)
				{ return; }
			}
			else
			{
				nextIndex--;

				if (nextIndex < 0)
				{ return; }
			}



			//순서를 바꿀 모디파이어다.
			apModifierBase swapMod = modStack._modifiers[nextIndex];

			//인덱스를 서로 바꾸자
			swapMod._layer = prevIndex;
			modifier._layer = nextIndex;

			modStack.RefreshAndSort(false);

			Editor.RefreshControllerAndHierarchy(false);
			Editor.SetRepaint();
		}



		public void RemoveModifier(apModifierBase modifier)
		{
			if (Editor._portrait == null || Editor.Select.MeshGroup == null)
			{
				return;
			}

			apModifierStack modStack = Editor.Select.MeshGroup._modifierStack;

			if (!modStack._modifiers.Contains(modifier))
			{
				return;
			}

			//Undo
			//apEditorUtil.SetRecord_MeshGroupAllModifiers(apUndoGroupData.ACTION.MeshGroup_RemoveModifier, Editor, Editor.Select.MeshGroup, modifier, false);
			apEditorUtil.SetRecordBeforeCreateOrDestroyObject(Editor._portrait, "Remove Modifier");

			apMeshGroup targetMeshGroup = Editor.Select.MeshGroup;

			int modifierID = modifier._uniqueID;
			//Editor._portrait.PushUniqueID_Modifier(modifierID);
			Editor._portrait.PushUnusedID(apIDManager.TARGET.Modifier, modifierID);

			modStack.RemoveModifier(modifier);
			modStack.RefreshAndSort(true);

			//추가
			if (modifier != null)
			{
				//Undo.DestroyObjectImmediate(modifier.gameObject);
				apEditorUtil.SetRecordDestroyMonoObject(modifier, "Remove Modifier");
			}

			//다시 연결
			targetMeshGroup.RefreshModifierLink();

			Editor.Select.SetModifier(null);

			//추가 : ExMode에 추가한다.
			Editor.Select.RefreshMeshGroupExEditingFlags(
								targetMeshGroup,
								null,
								null,
								null,
								true);

			//4.1 추가된 데이터가 있으면 일단 호출한다.
			Editor.OnAnyObjectAddedOrRemoved();

			Editor._portrait.LinkAndRefreshInEditor(true, targetMeshGroup);//<<전체 갱신

			targetMeshGroup.SortRenderUnits(true);
			targetMeshGroup.RefreshForce(true);

			Editor.RefreshControllerAndHierarchy(false);

			//프리팹이었다면 Apply
			apEditorUtil.SetPortraitPrefabApply(Editor._portrait);
		}




		//추가 5.21
		//만약 더이상 링크가 되지 않는 Modifier / Modifier ParamSetGroup / Modifier ParamSet / Modified Mesh/Bone이 있다면
		//자동으로 삭제를 해야한다.
		//삭제 시점은 
		//- Mesh/MeshGroup/AnimClip/ControlParam이 삭제 되었을 때
		//- 타임라인/타임라인 레이어가 삭제되었을 때
		//- AnimClip의 대상이 바뀌었을 때 (이건 물어봐야함)
		//- 에디터 첫 링크할때
		public void CheckAndRemoveUnusedModifierData(apPortrait portrait, bool isShowDialog, bool isSaveToUndo)
		{
			//1. 일단 체크하자
			if (portrait == null)
			{
				return;
			}

			//삭제되어야 하는 객체들
			//List<apModifierBase> _rmvModifiers = new List<apModifierBase>();
			//List<apModifierParamSetGroup> _rmvParamSetGroups = new List<apModifierParamSetGroup>();
			//List<apModifierParamSet> _rmvParamSets = new List<apModifierParamSet>();
			//List<apModifiedMesh> _rmvModMeshes = new List<apModifiedMesh>();
			//List<apModifiedBone> _rmvModBones = new List<apModifiedBone>();

			//1) MeshGroup -> 체크
			//2) Modifier -> 체크
			//3) Timeline / TimelineLayer -> 체크
			apMeshGroup meshGroup = null;
			apModifierBase modifier = null;
			apModifierParamSetGroup paramSetGroup = null;
			apModifierParamSet paramSet = null;
			for (int iMeshGroup = 0; iMeshGroup < portrait._meshGroups.Count; iMeshGroup++)
			{
				meshGroup = portrait._meshGroups[iMeshGroup];
				for (int iModifier = 0; iModifier < meshGroup._modifierStack._modifiers.Count; iModifier++)
				{
					modifier = meshGroup._modifierStack._modifiers[iModifier];

					for (int iPSG = 0; iPSG < modifier._paramSetGroup_controller.Count; iPSG++)
					{
						paramSetGroup = modifier._paramSetGroup_controller[iPSG];

						apAnimClip animClip = null;
						apAnimTimeline timeline = null;
						apAnimTimelineLayer timelineLayer = null;

						//체크한다.
						bool isRemovablePSG = false;
						switch (paramSetGroup._syncTarget)
						{
							case apModifierParamSetGroup.SYNC_TARGET.ControllerWithoutKey:
							case apModifierParamSetGroup.SYNC_TARGET.Bones:
								//사용하지 않는 값
								break;

							case apModifierParamSetGroup.SYNC_TARGET.Controller:
								{
									//Control Param과 연결되어야 한다.
									//없으면 검색
									if (paramSetGroup._keyControlParam == null)
									{
										if (portrait._controller.FindParam(paramSetGroup._keyControlParamID) == null)
										{
											isRemovablePSG = true;
										}
									}
								}
								break;

							case apModifierParamSetGroup.SYNC_TARGET.KeyFrame:
								{
									//KeyAnim이 있어야 한다.
									//없으면 검색
									animClip = paramSetGroup._keyAnimClip;
									if (animClip == null)
									{
										animClip = portrait.GetAnimClip(paramSetGroup._keyAnimClipID);
										if (animClip == null)
										{
											isRemovablePSG = true;
										}

									}

									if (animClip != null)
									{
										//반대로, AnimClip에 이 ParamSetGroup이 속한게 아니라면 문제
										if (animClip._targetMeshGroup != meshGroup)
										{
											//MeshGroup이 다르다.
											isRemovablePSG = true;
										}
									}


									if (!isRemovablePSG && animClip != null)
									{
										//타임라인이 있는지 검사하자
										timeline = paramSetGroup._keyAnimTimeline;
										if (timeline == null)
										{
											timeline = animClip.GetTimeline(paramSetGroup._keyAnimTimelineID);
											if (timeline == null)
											{
												//타임라인이 없네용
												isRemovablePSG = true;
											}
										}
										if (timeline != null)
										{
											//반대로, 해당 Timeline에 paramSetGroup에 포함이 안된다면 그것도 문제

										}
									}

									if (!isRemovablePSG && animClip != null && timeline != null)
									{
										//타임라인 레이어가 있는지도 검사하자
										timelineLayer = paramSetGroup._keyAnimTimelineLayer;
										if (timelineLayer == null)
										{
											timelineLayer = timeline.GetTimelineLayer(paramSetGroup._keyAnimTimelineLayerID);
											if (timelineLayer == null)
											{
												//타임라인 레이어가 없네용
												isRemovablePSG = true;
											}
										}
									}
								}
								break;

							case apModifierParamSetGroup.SYNC_TARGET.Static:
								//Static은 기본적으로 삭제되는 요건이 없다.
								break;


						}

						if (isRemovablePSG)
						{
							//>> 삭제되어야 하는 ParamSetGroup
							//Debug.LogError("삭제되어야 하는 ParamSetGroup");
							//_rmvParamSetGroups.Add(paramSetGroup);
							continue;
						}

						//삭제가 안되도 된다면
						//ParamSet을 분석하자
						for (int iPS = 0; iPS < paramSetGroup._paramSetList.Count; iPS++)
						{
							paramSet = paramSetGroup._paramSetList[iPS];

							//- 연결된 Control Param이 없다면 ParamSetGroup이 이미 삭제 되었을 것이다.
							//- Anim타입일 때 Keyframe이 없다면 삭제되어야 한다.
							//- Modified Mesh가 유효한지 테스트한다.
							//- Modified Bone이 유효한지 테스트한다.
							bool isRemovablePS = false;
							if (paramSetGroup._syncTarget == apModifierParamSetGroup.SYNC_TARGET.KeyFrame)
							{
								if (timelineLayer == null)
								{
									//Debug.LogError("??? 위에서 ParamSetGroup이 삭제 안되었는데 정작 timelineLayer가 null이다.");
								}
								else
								{
									apAnimKeyframe animKeyframe = timelineLayer.GetKeyframeByID(paramSet._keyframeUniqueID);
									if (animKeyframe == null)
									{
										//키프레임이 없어졌다. >> 삭제
										isRemovablePS = true;
										//Debug.LogError("삭제되어야 하는 ParamSet");
									}
								}
							}
							if (!isRemovablePS)
							{
								//Modified Mesh / Bone을 테스트하자
								if (paramSet._meshData != null && paramSet._meshData.Count > 0)
								{
									apModifiedMesh modMesh = null;
									for (int iModMesh = 0; iModMesh < paramSet._meshData.Count; iModMesh++)
									{
										modMesh = paramSet._meshData[iModMesh];

										if (modMesh._meshGroupOfModifier == null &&
													modMesh._meshGroupOfTransform == null)
										{
											//둘다 Null이면 뭐지.. Link 전인가
											Debug.LogError("Link MeshGroup Error");
										}


										//if(modMesh._isMeshTransform)
										//{
										//	if(modMesh._transform_Mesh == null)
										//	{
										//		Debug.LogError("MeshTransform이 없는 ModMesh");
										//	}
										//	else if(modMesh._renderUnit == null)
										//	{
										//		Debug.LogError("RenderUnit이 없다;");
										//	}
										//}
										//else
										//{
										//	if(modMesh._transform_MeshGroup == null)
										//	{
										//		Debug.LogError("MeshGroupTransform이 없는 ModMesh");
										//	}
										//	else if(modMesh._renderUnit == null)
										//	{
										//		Debug.LogError("RenderUnit이 없다;");
										//	}
										//}
									}
								}

								if (paramSet._boneData != null && paramSet._boneData.Count > 0)
								{
									apModifiedBone modBone = null;
									for (int iModBone = 0; iModBone < paramSet._boneData.Count; iModBone++)
									{
										modBone = paramSet._boneData[iModBone];
										//if(modBone._bone == null)
										//{
										//	Debug.LogError("Bone없는 ModBone");
										//}
									}
								}
							}


						}
					}
				}

			}



		}

		//----------------------------------------------------------------------------------
		// Bake
		//----------------------------------------------------------------------------------
		/// <summary>
		/// 현재 Portrait를 실행가능한 버전으로 Bake하자
		/// </summary>
		public apBakeResult Bake()
		{
			if (Editor._portrait == null)
			{
				return null;
			}

			apEditorUtil.SetEditorDirty();

			apBakeResult bakeResult = new apBakeResult();


			//추가 19.5.26 : v1.1.7의 용량 최적화가 적용되었는가 (=modMeshSet을 이용하도록 설정되었는가)
			bool isSizeOptimizedV117 = true;

			//bool isSizeOptimizedV117 = false;//<<테스트
			
			//추가 19.8.5
			bool isUseSRP = Editor._isUseSRP;


			//추가 10.26 : Bake에서는 빌보드가 꺼져야 한다.
			//임시로 껐다가 마지막에 다시 복구
			apPortrait.BILLBOARD_TYPE billboardType = Editor._portrait._billboardType;
			Editor._portrait._billboardType = apPortrait.BILLBOARD_TYPE.None;//임시로 끄자


			//Bake 방식 변경
			//일단 숨겨진 GameObject를 제외한 모든 객체를 리스트로 저장한다.
			//LinkParam 형태로 저장을 한다.
			//LinkParam으로 저장하면서 <apOpt 객체>와 <그렇지 않은 객체>를 구분한다.
			//"apOpt 객체"는 나중에 (1)재활용 할지 (2) 삭제 할지 결정한다.
			//"그렇지 않은 GameObject"는 Hierarchy 정보를 가진채 (1) 링크를 유지할 지(재활용되는 경우) (2) Unlink Group에 넣을지 결정한다.
			//만약 재활용되지 않는 (apOpt GameObject)에서 알수 없는 Component가 발견된 경우 -> 이건 삭제 예외 대상에 넣는다.

			//분류를 위한 그룹
			//1. ReadyToRecycle
			// : 기존에 RootUnit과 그 하위에 있었던 GameObject들이다. 분류 전에 일단 여기로 들어간다.
			// : 분류 후에는 원칙적으로 하위에 어떤 객체도 남아선 안된다.

			//2. RemoveTargets
			// : apOpt를 가진 GameObject 그룹 중에서 사용되지 않았던 그룹이다. 
			// : 처리 후에는 이 GameObject를 통째로 삭제한다.

			//3. UnlinkedObjects
			// : apOpt를 가지지 않은 GameObject중에서 재활용되지 않은 객체들


			GameObject groupObj_1_ReadyToRecycle = new GameObject("__Baking_1_ReadyToRecycle");
			GameObject groupObj_2_RemoveTargets = new GameObject("__Baking_2_RemoveTargets");


			GameObject groupObj_3_UnlinkedObjects = null;
			if (Editor._portrait._bakeUnlinkedGroup == null)
			{
				groupObj_3_UnlinkedObjects = new GameObject("__UnlinkedObjects");
				Editor._portrait._bakeUnlinkedGroup = groupObj_3_UnlinkedObjects;
			}
			else
			{
				groupObj_3_UnlinkedObjects = Editor._portrait._bakeUnlinkedGroup;
				groupObj_3_UnlinkedObjects.name = "__UnlinkedObjects";
			}




			groupObj_1_ReadyToRecycle.transform.parent = Editor._portrait.transform;
			groupObj_2_RemoveTargets.transform.parent = Editor._portrait.transform;
			groupObj_3_UnlinkedObjects.transform.parent = Editor._portrait.transform;

			groupObj_1_ReadyToRecycle.transform.localPosition = Vector3.zero;
			groupObj_2_RemoveTargets.transform.localPosition = Vector3.zero;
			groupObj_3_UnlinkedObjects.transform.localPosition = Vector3.zero;

			groupObj_1_ReadyToRecycle.transform.localRotation = Quaternion.identity;
			groupObj_2_RemoveTargets.transform.localRotation = Quaternion.identity;
			groupObj_3_UnlinkedObjects.transform.localRotation = Quaternion.identity;

			groupObj_1_ReadyToRecycle.transform.localScale = Vector3.one;
			groupObj_2_RemoveTargets.transform.localScale = Vector3.one;
			groupObj_3_UnlinkedObjects.transform.localScale = Vector3.one;


			//2. 기존 RootUnit을 Recycle로 옮긴다.
			//옮기면서 "Prev List"를 만들어야 한다. Recycle을 하기 위함
			List<apOptRootUnit> prevOptRootUnits = new List<apOptRootUnit>();
			if (Editor._portrait._optRootUnitList != null)
			{
				for (int i = 0; i < Editor._portrait._optRootUnitList.Count; i++)
				{
					apOptRootUnit optRootUnit = Editor._portrait._optRootUnitList[i];
					if (optRootUnit != null)
					{
						optRootUnit.transform.parent = groupObj_1_ReadyToRecycle.transform;

						prevOptRootUnits.Add(optRootUnit);
					}
				}
			}



			//삭제하는 코드
			//일단 이 코드는 사용하지 않습니다.
			//if (Editor._portrait._optRootUnitList != null)
			//{
			//	for (int i = 0; i < Editor._portrait._optRootUnitList.Count; i++)
			//	{
			//		apOptRootUnit optRootUnit = Editor._portrait._optRootUnitList[i];
			//		if (optRootUnit != null && optRootUnit.gameObject != null)
			//		{
			//			GameObject.DestroyImmediate(optRootUnit.gameObject);
			//		}
			//	}
			//}
			//else
			//{
			//	Editor._portrait._optRootUnitList = new List<apOptRootUnit>();
			//}

			//RootUnit 리스트를 초기화한다.
			if (Editor._portrait._optRootUnitList == null)
			{
				Editor._portrait._optRootUnitList = new List<apOptRootUnit>();
			}

			Editor._portrait._optRootUnitList.Clear();
			Editor._portrait._curPlayingOptRootUnit = null;
			//if(Editor._portrait._optRootUnit != null)
			//{
			//	GameObject.DestroyImmediate(Editor._portrait._optRootUnit.gameObject);
			//}

			if (Editor._portrait._optTransforms == null) { Editor._portrait._optTransforms = new List<apOptTransform>(); }
			if (Editor._portrait._optMeshes == null) { Editor._portrait._optMeshes = new List<apOptMesh>(); }
			//if (Editor._portrait._optMaskedMeshes == null)		{ Editor._portrait._optMaskedMeshes = new List<apOptMesh>(); }
			//if (Editor._portrait._optClippedMeshes == null)		{ Editor._portrait._optClippedMeshes = new List<apOptMesh>(); }
			if (Editor._portrait._optTextureData == null) { Editor._portrait._optTextureData = new List<apOptTextureData>(); }//<<텍스쳐 데이터 추가



			Editor._portrait._optTransforms.Clear();
			Editor._portrait._optMeshes.Clear();
			//Editor._portrait._optMaskedMeshes.Clear();
			//Editor._portrait._optClippedMeshes.Clear();
			Editor._portrait._optTextureData.Clear();
			//Editor._portrait._isAnyMaskedMeshes = false;

			//추가
			//Batched Matrial 관리 객체가 생겼다.
			if (Editor._portrait._optBatchedMaterial == null)
			{
				Editor._portrait._optBatchedMaterial = new apOptBatchedMaterial();
			}
			else
			{
				Editor._portrait._optBatchedMaterial.Clear(true);//<<이미 생성되어 있다면 초기화
			}

			////추가 11.6 : LWRP Shader를 사용하는지 체크하고, 필요한 경우 생성해야한다.
			//CheckAndCreateLWRPShader();


			//3. 텍스쳐 데이터를 먼저 만들자.
			for (int i = 0; i < Editor._portrait._textureData.Count; i++)
			{
				apTextureData textureData = Editor._portrait._textureData[i];
				apOptTextureData newOptTexData = new apOptTextureData();

				newOptTexData.Bake(i, textureData);
				Editor._portrait._optTextureData.Add(newOptTexData);
			}

			//4. 추가 : Reset
			//TODO : 이 함수를 호출한 이후에, 현재 Mesh Group에 대해서 추가 처리 필요
			//이 함수를 호출하면 계층적인 MeshGroup 내부늬 Modifier 연결이 풀린다.
			//이 코드 두개가 포함되어야 한다.
			//meshGroup.LinkModMeshRenderUnits();
			//meshGroup.RefreshModifierLink();
			Editor._portrait.LinkAndRefreshInEditor(false, null);


			//추가 : 사용되지 않는 Monobehaviour는 삭제해야한다.
			CheckAndRemoveUnusedMonobehaviours(Editor._portrait);

			//이름을 갱신한다.
			CheckAndRefreshGameObjectNames(Editor._portrait);


			



			//4. OptTransform을 만들자 (RootUnit부터)

			for (int i = 0; i < Editor._portrait._rootUnits.Count; i++)
			{
				apRootUnit rootUnit = Editor._portrait._rootUnits[i];

				//업데이트를 한번 해주자

				//추가 : 계층구조의 MeshGroup인 경우 이 코드가 추가되어야 한다.
				if (rootUnit._childMeshGroup != null)
				{
					rootUnit._childMeshGroup.SortRenderUnits(true);//렌더 유닛의 Depth를 다시 계산해야한다. <<
					rootUnit._childMeshGroup.LinkModMeshRenderUnits();
					rootUnit._childMeshGroup.RefreshModifierLink();
				}

				rootUnit.Update(0.0f, false, false);


				apOptRootUnit optRootUnit = null;

				//1. Root Unit
				//재활용 가능한지 판단한다.


				bool isRecycledRootUnit = false;
				apOptRootUnit recycledOptRootUnit = GetRecycledRootUnit(rootUnit, prevOptRootUnits);



				if (recycledOptRootUnit != null)
				{

					//재활용이 된다.
					optRootUnit = recycledOptRootUnit;

					//일부 값은 다시 리셋
					optRootUnit.name = "Root Unit " + i;
					optRootUnit._portrait = Editor._portrait;
					optRootUnit._transform = optRootUnit.transform;

					optRootUnit.transform.parent = Editor._portrait.transform;
					optRootUnit.transform.localPosition = Vector3.zero;
					optRootUnit.transform.localRotation = Quaternion.identity;
					optRootUnit.transform.localScale = Vector3.one;

					//재활용에 성공했으니 OptUnit은 제외한다.
					prevOptRootUnits.Remove(recycledOptRootUnit);
					isRecycledRootUnit = true;

					//Count+1 : Recycled Opt
					bakeResult.AddCount_RecycledOptGameObject();
				}
				else
				{
					//새로운 RootUnit이다.
					optRootUnit = AddGameObject<apOptRootUnit>("Root Unit " + i, Editor._portrait.transform);

					optRootUnit._portrait = Editor._portrait;
					optRootUnit._rootOptTransform = null;
					optRootUnit._transform = optRootUnit.transform;

					//Count+1 : New Opt
					bakeResult.AddCount_NewOptGameObject();
				}

				optRootUnit.ClearChildLinks();//Child Link를 초기화한다.

				//추가 12.6 : SortedRenderBuffer에 관련한 Bake 코드 <<
				optRootUnit.BakeSortedRenderBuffer(Editor._portrait, rootUnit);


				Editor._portrait._optRootUnitList.Add(optRootUnit);



				//재활용에 성공했다면
				//기존의 GameObject + Bake 여부를 재귀적 리스트로 작성한다.
				apBakeLinkManager bakeLinkManager = null;
				if (isRecycledRootUnit)
				{
					bakeLinkManager = new apBakeLinkManager();

					//파싱하자.
					bakeLinkManager.Parse(optRootUnit._rootOptTransform.gameObject, recycledOptRootUnit.gameObject);
				}

				apMeshGroup childMainMeshGroup = rootUnit._childMeshGroup;

				//0. 추가
				//일부 Modified Mesh를 갱신해야한다.
				if (childMainMeshGroup != null && rootUnit._childMeshGroupTransform != null)
				{
					//Refresh를 한번 해주자
					childMainMeshGroup.RefreshForce();

					List<apModifierBase> modifiers = childMainMeshGroup._modifierStack._modifiers;
					for (int iMod = 0; iMod < modifiers.Count; iMod++)
					{
						apModifierBase mod = modifiers[iMod];
						if (mod._paramSetGroup_controller != null)
						{
							for (int iPSG = 0; iPSG < mod._paramSetGroup_controller.Count; iPSG++)
							{
								apModifierParamSetGroup psg = mod._paramSetGroup_controller[iPSG];
								for (int iPS = 0; iPS < psg._paramSetList.Count; iPS++)
								{
									apModifierParamSet ps = psg._paramSetList[iPS];
									ps.UpdateBeforeBake(Editor._portrait, childMainMeshGroup, rootUnit._childMeshGroupTransform);
								}
							}
						}
					}
				}

				//1. 1차 Bake : GameObject 만들기
				//List<apMeshGroup> meshGroups = Editor._portrait._meshGroups;
				if (childMainMeshGroup != null && rootUnit._childMeshGroupTransform != null)
				{
					//정렬 한번 해주고
					childMainMeshGroup.SortRenderUnits(true);

					apRenderUnit rootRenderUnit = childMainMeshGroup._rootRenderUnit;
					//apRenderUnit rootRenderUnit = Editor._portrait._rootUnit._renderUnit;
					if (rootRenderUnit != null)
					{
						//apTransform_MeshGroup meshGroupTransform = Editor._portrait._rootUnit._childMeshGroupTransform;
						apTransform_MeshGroup meshGroupTransform = rootRenderUnit._meshGroupTransform;

						if (meshGroupTransform == null)
						{
							Debug.LogError("Bake Error : MeshGroupTransform Not Found [" + childMainMeshGroup._name + "]");
						}
						else
						{
							MakeMeshGroupToOptTransform(rootRenderUnit,
															meshGroupTransform, optRootUnit.transform,
															null,
															optRootUnit,
															bakeLinkManager, bakeResult,
															Editor._portrait._bakeZSize,
															Editor._isBakeColorSpaceToGamma,//<<감마 색상 공간으로 Bake할 것인가
															Editor._isUseSRP,//LWRP Shader를 사용할 것인가
															Editor._portrait,
															childMainMeshGroup,
															isSizeOptimizedV117,
															isUseSRP);
							//MakeMeshGroupToOptTransform(null, meshGroupTransform, Editor._portrait._optRootUnit.transform, null);
						}
					}
					else
					{
						Debug.LogError("Bake Error : RootMeshGroup Not Found [" + childMainMeshGroup._name + "]");
					}
				}



				//optRootUnit.transform.localScale = Vector3.one * 0.01f;
				optRootUnit.transform.localScale = Vector3.one * Editor._portrait._bakeScale;


				// 이전에 Bake 했던 정보에서 가져왔다면
				//만약 "재활용되지 않은 GameObject"를 찾아서 별도의 처리를 해야한다.
				if (isRecycledRootUnit && bakeLinkManager != null)
				{
					bakeLinkManager.SetHierarchyNotRecycledObjects(groupObj_1_ReadyToRecycle, groupObj_2_RemoveTargets, groupObj_3_UnlinkedObjects, bakeResult);

				}

				//추가 12.6 : Bake 함수 추가 <<
				optRootUnit.BakeComplete();

			}


			if (prevOptRootUnits.Count > 0)
			{
				//TODO : 여기서부터 작성하자
				//이 유닛들은 Remove Target으로 이동해야 한다.

				for (int i = 0; i < prevOptRootUnits.Count; i++)
				{
					prevOptRootUnits[i].transform.parent = groupObj_2_RemoveTargets.transform;

					//만약 여기서 알수없는 GameObject나 Compnent에 대해서는 Remove가 아니라 Unlink로 옮겨야 한다.
					apBakeLinkManager prevBakeManager = new apBakeLinkManager();
					prevBakeManager.Parse(prevOptRootUnits[i]._rootOptTransform.gameObject, null);

					prevBakeManager.SetHierarchyToUnlink(groupObj_3_UnlinkedObjects, bakeResult);

				}
			}


			//TODO: 이제 그룹을 삭제하던가 경고 다이얼로그를 띄워주던가 하자
			UnityEngine.Object.DestroyImmediate(groupObj_1_ReadyToRecycle);
			UnityEngine.Object.DestroyImmediate(groupObj_2_RemoveTargets);

			if (groupObj_3_UnlinkedObjects.transform.childCount == 0)
			{
				UnityEngine.Object.DestroyImmediate(groupObj_3_UnlinkedObjects);

				Editor._portrait._bakeUnlinkedGroup = null;
			}


			//1-2. Masked Mesh 연결해주기
			//if (Editor._portrait._optMaskedMeshes.Count > 0 || Editor._portrait._optClippedMeshes.Count > 0)
			//{
			//	Editor._portrait._isAnyMaskedMeshes = true;
			//}

			for (int i = 0; i < Editor._portrait._optMeshes.Count; i++)
			{
				apOptMesh optMesh = Editor._portrait._optMeshes[i];
				if (optMesh._isMaskParent)
				{
					//Parent라면..
					//apOptMesh[] childMeshes = new apOptMesh[3];
					//for (int iChild = 0; iChild < 3; iChild++)
					//{
					//	childMeshes[iChild] = null;
					//	if(optMesh._clipChildIDs[iChild] >= 0)
					//	{
					//		apOptTransform optTransform = Editor._portrait.GetOptTransform(optMesh._clipChildIDs[iChild]);
					//		if(optTransform != null && optTransform._childMesh != null)
					//		{
					//			childMeshes[iChild] = optTransform._childMesh;
					//		}

					//	}
					//}
					//optMesh.LinkAsMaskParent(childMeshes);//<<이거 사용 안합니더
				}
				else if (optMesh._isMaskChild)
				{
					apOptTransform optTransform = Editor._portrait.GetOptTransform(optMesh._clipParentID);
					apOptMesh parentMesh = null;
					if (optTransform != null && optTransform._childMesh != null)
					{
						parentMesh = optTransform._childMesh;
					}
					optMesh.LinkAsMaskChild(parentMesh);
				}
			}

			//2. 2차 Bake : Modifier 만들기
			List<apOptTransform> optTransforms = Editor._portrait._optTransforms;
			for (int i = 0; i < optTransforms.Count; i++)
			{
				apOptTransform optTransform = optTransforms[i];

				apMeshGroup srcMeshGroup = Editor._portrait.GetMeshGroup(optTransform._meshGroupUniqueID);
				optTransform.BakeModifier(Editor._portrait, srcMeshGroup, isSizeOptimizedV117);
			}


			//3. 3차 Bake : ControlParam/KeyFrame ~~> Modifier <- [Calculated Param] -> OptTrasform + Mesh
			Editor._portrait.Initialize();

			//4. 첫번째 OptRoot만 보여주도록 하자
			if (Editor._portrait._optRootUnitList.Count > 0)
			{
				Editor._portrait.ShowRootUnitWhenBake(Editor._portrait._optRootUnitList[0]);
			}

			//5. AnimClip의 데이터를 받아서 AnimPlay 데이터로 만들자
			if (Editor._portrait._animPlayManager == null)
			{
				Editor._portrait._animPlayManager = new apAnimPlayManager();
			}

			Editor._portrait._animPlayManager.InitAndLink();
			Editor._portrait._animPlayManager._animPlayDataList.Clear();

			for (int i = 0; i < Editor._portrait._animClips.Count; i++)
			{
				apAnimClip animClip = Editor._portrait._animClips[i];
				int animClipID = animClip._uniqueID;
				string animClipName = animClip._name;
				int targetMeshGroupID = animClip._targetMeshGroupID;

				apAnimPlayData animPlayData = new apAnimPlayData(animClipID, targetMeshGroupID, animClipName);
				Editor._portrait._animPlayManager._animPlayDataList.Add(animPlayData);

			}

			//6. 한번 업데이트를 하자 (소켓들이 갱신된다)
			if (Editor._portrait._optRootUnitList.Count > 0)
			{
				apOptRootUnit optRootUnit = null;
				for (int i = 0; i < Editor._portrait._optRootUnitList.Count; i++)
				{
					//이전 : 함수가 너무 반복되어 래핑되었다. 함수를 제거한닷
					//Editor._portrait._optRootUnitList[i].RemoveAllCalculateResultParams();

					//변경
					optRootUnit = Editor._portrait._optRootUnitList[i];
					if (optRootUnit._rootOptTransform != null)
					{
						optRootUnit._rootOptTransform.ClearResultParams(true);
						optRootUnit._rootOptTransform.ResetCalculateStackForBake(true);
					}
					else
					{
						Debug.LogError("AnyPortrait : No Root Opt Transform on RootUnit");
					}

				}

				//이 코드는 위에 추가되었다. "optRootUnit._rootOptTransform.ResetCalculateStackForBake(true);"
				//추가 3.22 : Bake후 메시가 변경되었을 경우에 다시 리셋할 필요가 있다.
				//for (int i = 0; i < Editor._portrait._optRootUnitList.Count; i++)
				//{
				//	Editor._portrait._optRootUnitList[i].ResetCalculateStackForBake();
				//}

				for (int i = 0; i < Editor._portrait._optRootUnitList.Count; i++)
				{
					Editor._portrait._optRootUnitList[i].UpdateTransforms(0.0f);
				}

				////디버그를 해보자
				//for (int i = 0; i < Editor._portrait._optRootUnitList.Count; i++)
				//{	
				//	Editor._portrait._optRootUnitList[i].DebugBoneMatrix();
				//}
				//Debug.LogError("------------------------------------------");

			}



			//6. Mask 메시 한번 더 갱신
			//if(Editor._portrait._optMaskedMeshes.Count > 0)
			//{
			//	for (int i = 0; i < Editor._portrait._optMaskedMeshes.Count; i++)
			//	{
			//		Editor._portrait._optMaskedMeshes[i].RefreshMaskedMesh();
			//	}
			//}
			//> 변경 : Child 위주로 변경
			//if (Editor._portrait._optClippedMeshes.Count > 0)
			//{
			//	for (int i = 0; i < Editor._portrait._optClippedMeshes.Count; i++)
			//	{
			//		Editor._portrait._optClippedMeshes[i].RefreshClippedMesh();
			//	}
			//}


			//추가 3.22 
			//6-2. LayerOrder 갱신하자
			string sortingLayerName = "";
			bool isValidSortingLayer = false;
			if (SortingLayer.IsValid(Editor._portrait._sortingLayerID))
			{
				sortingLayerName = SortingLayer.IDToName(Editor._portrait._sortingLayerID);
				isValidSortingLayer = true;
			}
			else
			{
				if (SortingLayer.layers.Length > 0)
				{
					sortingLayerName = SortingLayer.layers[0].name;
					isValidSortingLayer = true;
				}
				else
				{
					isValidSortingLayer = false;
				}
			}
			if (isValidSortingLayer)
			{
				Editor._portrait.SetSortingLayer(sortingLayerName);
			}
			//변경 19.8.19 : 옵션이 적용되는 경우에 한해서
			if (Editor._portrait._sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.SetOrder)
			{
				Editor._portrait.SetSortingOrder(Editor._portrait._sortingOrder);
			}


			//추가 19.5.26
			//6-3. 최적화 옵션으로 Bake 되었는지 체크
			Editor._portrait._isSizeOptimizedV117 = isSizeOptimizedV117;



			//7. 기본 GameObject 타입 (Mesh, MeshGroup, Modifier) 중에서 사용되지 않는 객체는 삭제해주자
			List<apMesh> usingMeshes = new List<apMesh>();
			List<apMeshGroup> usingMeshGroups = new List<apMeshGroup>();
			List<apModifierBase> usingModifiers = new List<apModifierBase>();

			for (int i = 0; i < Editor._portrait._meshes.Count; i++)
			{
				Editor._portrait._meshes[i].gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

				usingMeshes.Add(Editor._portrait._meshes[i]);
			}

			for (int i = 0; i < Editor._portrait._meshGroups.Count; i++)
			{
				apMeshGroup meshGroup = Editor._portrait._meshGroups[i];
				meshGroup.gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

				usingMeshGroups.Add(meshGroup);

				for (int iMod = 0; iMod < meshGroup._modifierStack._modifiers.Count; iMod++)
				{
					meshGroup._modifierStack._modifiers[iMod].gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

					usingModifiers.Add(meshGroup._modifierStack._modifiers[iMod]);
				}
			}

			CheckAndMakeObjectGroup();

			//각 서브 오브젝트 하위의 그룹들을 체크하여 유효하지 않는게 있는지 체크한다.

			List<GameObject> unusedMeshGameObjects = new List<GameObject>();
			List<GameObject> unusedMeshGroupGameObjects = new List<GameObject>();
			List<GameObject> unusedModifierGameObjects = new List<GameObject>();

			for (int iMesh = 0; iMesh < Editor._portrait._subObjectGroup_Mesh.transform.childCount; iMesh++)
			{
				Transform meshTF = Editor._portrait._subObjectGroup_Mesh.transform.GetChild(iMesh);
				apMesh targetMesh = meshTF.GetComponent<apMesh>();

				if (targetMesh == null)
				{
					//Mesh가 없는 GameObject 발견
					Debug.Log("No Mesh GameObject : " + meshTF.gameObject.name);

					unusedMeshGameObjects.Add(meshTF.gameObject);
				}
				else if (!usingMeshes.Contains(targetMesh))
				{
					//사용되지 않는 Mesh 발견
					Debug.Log("Unused Mesh Found : " + targetMesh._name);

					unusedMeshGameObjects.Add(meshTF.gameObject);
				}
			}

			for (int iMeshGroup = 0; iMeshGroup < Editor._portrait._subObjectGroup_MeshGroup.transform.childCount; iMeshGroup++)
			{
				Transform meshGroupTF = Editor._portrait._subObjectGroup_MeshGroup.transform.GetChild(iMeshGroup);
				apMeshGroup targetMeshGroup = meshGroupTF.GetComponent<apMeshGroup>();

				if (targetMeshGroup == null)
				{
					//MeshGroup이 없는 GameObject 발견
					//Debug.Log("No MeshGroup GameObject : " + meshGroupTF.gameObject.name);

					unusedMeshGroupGameObjects.Add(meshGroupTF.gameObject);
				}
				else if (!usingMeshGroups.Contains(targetMeshGroup))
				{
					//사용되지 않는 MeshGroup 발견
					//Debug.Log("Unused MeshGroup Found : " + targetMeshGroup._name);

					unusedMeshGroupGameObjects.Add(meshGroupTF.gameObject);
				}
			}

			for (int iMod = 0; iMod < Editor._portrait._subObjectGroup_Modifier.transform.childCount; iMod++)
			{
				Transform modTF = Editor._portrait._subObjectGroup_Modifier.transform.GetChild(iMod);
				apModifierBase targetMod = modTF.GetComponent<apModifierBase>();

				if (targetMod == null)
				{
					//Modifier가 없는 GameObject 발견
					//Debug.Log("No Modifier GameObject : " + modTF.gameObject.name);

					unusedModifierGameObjects.Add(modTF.gameObject);
				}
				else if (!usingModifiers.Contains(targetMod))
				{
					//사용되지 않는 Modifier 발견
					//Debug.Log("Unused Modifier Found : " + targetMod.DisplayName);

					unusedModifierGameObjects.Add(modTF.gameObject);
				}
			}

			//참조되지 않은건 삭제하자
			for (int i = 0; i < unusedMeshGameObjects.Count; i++)
			{
				UnityEngine.Object.DestroyImmediate(unusedMeshGameObjects[i]);
			}
			for (int i = 0; i < unusedMeshGroupGameObjects.Count; i++)
			{
				UnityEngine.Object.DestroyImmediate(unusedMeshGroupGameObjects[i]);
			}
			for (int i = 0; i < unusedModifierGameObjects.Count; i++)
			{
				UnityEngine.Object.DestroyImmediate(unusedModifierGameObjects[i]);
			}

			//여기서 Opt 업뎃을 하나 할까..
			//Editor._portrait.Hide();
			//Editor._portrait.Show();
			//Editor._portrait.UpdateForce();

			//추가3.22
			//Portrait가 Prefab이라면
			//Bake와 동시에 Apply를 해야한다.
			//if(apEditorUtil.IsPrefab(Editor._portrait.gameObject))
			//{
			//	apEditorUtil.ApplyPrefab(Editor._portrait.gameObject, true);
			//	//그리고 다시 Apply를 해제
			//	apEditorUtil.DisconnectPrefab(Editor._portrait);
			//}

			//메카님 옵션이 켜져 있다면
			//1. Animation Clip들을 리소스로 생성한다.
			//2. Animator 컴포넌트를 추가한다.


			if (Editor._portrait._isUsingMecanim)
			{
				//추가 3.22 : animClip 경로가 절대 경로인 경우, 여러 작업자가 공유해서 쓸 수 없다.
				//상대 경로로 바꾸는 작업을 해야한다.
				CheckAnimationsBasePathForV116(Editor._portrait);

				CreateAnimationsWithMecanim(Editor._portrait, Editor._portrait._mecanimAnimClipResourcePath);
			}

			apEditorUtil.SetEditorDirty();

			//추가. Bake 후 처리
			ProcessAfterBake();

			//추가 19.10.26 : 빌보드 설정을 다시 복구
			Editor._portrait._billboardType = billboardType;

			return bakeResult;
		}

		//객체를 생성하기 전에 이전에 Bake된 것을 재활용하기 위한 함수

		private apOptRootUnit GetRecycledRootUnit(apRootUnit srcRootUnit, List<apOptRootUnit> prevObjects)
		{
			//Debug.Log("RootUnit 재활용 찾기");
			if (srcRootUnit._childMeshGroup != null && srcRootUnit._childMeshGroup._rootRenderUnit != null && srcRootUnit._childMeshGroup._rootRenderUnit._meshGroupTransform != null)
			{
				apTransform_MeshGroup rootMGTransform = srcRootUnit._childMeshGroup._rootRenderUnit._meshGroupTransform;

				apOptRootUnit prevOptRootUnit = null;
				for (int i = 0; i < prevObjects.Count; i++)
				{
					prevOptRootUnit = prevObjects[i];


					if (prevOptRootUnit._rootOptTransform != null)
					{

						//동일한 OptTransform을 가진다면 복사 가능함
						if (IsOptTransformRecyclable(prevOptRootUnit._rootOptTransform, null, rootMGTransform))
						{
							return prevOptRootUnit;
						}
					}
				}
			}

			return null;
		}

		private bool IsOptTransformRecyclable(apOptTransform prevOptTransform, apTransform_Mesh meshTransform, apTransform_MeshGroup meshGroupTransform)
		{
			if (meshTransform != null)
			{
				if (prevOptTransform._unitType == apOptTransform.UNIT_TYPE.Mesh)
				{
					return prevOptTransform._transformID == meshTransform._transformUniqueID;
				}
			}
			else if (meshGroupTransform != null)
			{
				if (prevOptTransform._unitType == apOptTransform.UNIT_TYPE.Group)
				{
					return prevOptTransform._transformID == meshGroupTransform._transformUniqueID;
				}
			}

			return false;
		}





		private T AddGameObject<T>(string name, Transform parent) where T : MonoBehaviour
		{
			GameObject newGameObject = new GameObject(name);
			newGameObject.transform.parent = parent;
			newGameObject.transform.localPosition = Vector3.zero;
			newGameObject.transform.localRotation = Quaternion.identity;
			newGameObject.transform.localScale = Vector3.one;

			return newGameObject.AddComponent<T>();
		}



		private void MakeMeshGroupToOptTransform(apRenderUnit renderUnit,
													apTransform_MeshGroup meshGroupTransform,
													Transform parent, apOptTransform parentTransform,
													apOptRootUnit targetOptRootUnit,
													apBakeLinkManager bakeLinkManager,
													apBakeResult bakeResult,
													float bakeZScale,
													bool isGammaColorSpace,
													bool isLWRPShader,
													apPortrait targetOptPortrait,
													apMeshGroup rootMeshGroup,
													bool isSizeOptimizedV117,
													bool isUseSRP)
		{
			string objectName = meshGroupTransform._nickName;
			int meshGroupUniqueID = -1;
			if (meshGroupTransform._meshGroup != null)
			{
				objectName = meshGroupTransform._meshGroup._name;
				meshGroupUniqueID = meshGroupTransform._meshGroup._uniqueID;
			}

			apMeshGroup meshGroup = meshGroupTransform._meshGroup;

			//if(meshGroupTransform._nickName.Length == 0)
			//{
			//	Debug.LogWarning("Empy Name : " + meshGroupTransform._meshGroup._name);
			//}

			apOptTransform optTransform = null;
			if (bakeLinkManager != null)
			{
				optTransform = bakeLinkManager.FindOptTransform(null, meshGroupTransform);
				if (optTransform != null)
				{
					//재활용에 성공했다.
					optTransform.gameObject.name = objectName;
					optTransform.transform.parent = parent;

					optTransform.transform.localPosition = Vector3.zero;
					optTransform.transform.localRotation = Quaternion.identity;
					optTransform.transform.localScale = Vector3.one;

					//Count+1 : Recycled Opt
					bakeResult.AddCount_RecycledOptGameObject();
				}
			}

			if (optTransform == null)
			{
				//재활용에 실패했다면 생성
				optTransform = AddGameObject<apOptTransform>(objectName, parent);

				//Count+1 : New Opt
				bakeResult.AddCount_NewOptGameObject();
			}

			//OptTransform을 설정하자
			#region [미사용 코드] SetBasicSetting 함수로 대체
			//optTransform._transformID = meshGroupTransform._transformUniqueID;
			//optTransform._transform = optTransform.transform;

			//optTransform._depth = meshGroupTransform._depth;
			//optTransform._defaultMatrix = new apMatrix(meshGroupTransform._matrix);

			////optTransform._transform.localPosition = optTransform._defaultMatrix.Pos3 - new Vector3(0.0f, 0.0f, (float)optTransform._depth * 0.1f);
			//optTransform._transform.localPosition = optTransform._defaultMatrix.Pos3 - new Vector3(0.0f, 0.0f, (float)optTransform._depth);
			//optTransform._transform.localRotation = Quaternion.Euler(0.0f, 0.0f, optTransform._defaultMatrix._angleDeg);
			//optTransform._transform.localScale = optTransform._defaultMatrix._scale; 
			#endregion

			int renderUnitLevel = -1;
			if (renderUnit != null)
			{
				renderUnitLevel = renderUnit._level;
			}
			optTransform.Bake(targetOptPortrait,//meshGroup, 
								parentTransform,
								targetOptRootUnit,
								meshGroupTransform._nickName,
								meshGroupTransform._transformUniqueID,
								meshGroupUniqueID,
								meshGroupTransform._matrix,
								false,
								renderUnitLevel, meshGroupTransform._depth,
								meshGroupTransform._isVisible_Default,
								meshGroupTransform._meshColor2X_Default,
								bakeZScale,
								isSizeOptimizedV117);

			//첫 초기화 Matrix(No-Mod)를 만들어주자 - Mesh Bake에서 사용된다.
			if (optTransform._matrix_TF_ToParent == null) { optTransform._matrix_TF_ToParent = new apMatrix(); }
			if (optTransform._matrix_TF_ParentWorld_NonModified == null) { optTransform._matrix_TF_ParentWorld_NonModified = new apMatrix(); }
			if (optTransform._matrix_TFResult_WorldWithoutMod == null) { optTransform._matrix_TFResult_WorldWithoutMod = new apMatrix(); }

			optTransform._matrix_TF_ToParent.SetMatrix(optTransform._defaultMatrix);
			optTransform._matrix_TF_ParentWorld_NonModified.SetIdentity();
			if (parentTransform != null)
			{
				optTransform._matrix_TF_ParentWorld_NonModified.SetMatrix(parentTransform._matrix_TFResult_WorldWithoutMod);
			}
			optTransform._matrix_TFResult_WorldWithoutMod.SetIdentity();
			optTransform._matrix_TFResult_WorldWithoutMod.RMultiply(optTransform._matrix_TF_ToParent);
			optTransform._matrix_TFResult_WorldWithoutMod.RMultiply(optTransform._matrix_TF_ParentWorld_NonModified);


			//RootUnit에 등록하자
			targetOptRootUnit.AddChildTransform(optTransform, rootMeshGroup.SortedBuffer.GetBufferData(renderUnit));


			//apBone을 추가해주자
			if (meshGroup._boneList_All.Count > 0)
			{
				MakeOptBone(meshGroup, optTransform, targetOptRootUnit, bakeLinkManager, bakeResult);
			}
			else
			{
				optTransform._boneList_All = null;
				optTransform._boneList_Root = null;
				optTransform._isBoneUpdatable = false;
			}




			//추가
			//소켓을 붙이자
			if (meshGroupTransform._isSocket)
			{
				apOptNode socketNode = null;
				if (bakeLinkManager != null)
				{
					socketNode = bakeLinkManager.FindOptTransformSocket(optTransform);
					if (socketNode != null)
					{
						socketNode.gameObject.name = meshGroupTransform._nickName + " Socket";
						socketNode.transform.parent = optTransform.transform;
						socketNode.transform.localPosition = Vector3.zero;
						socketNode.transform.localRotation = Quaternion.identity;
						socketNode.transform.localScale = Vector3.one;

						//Count+1 : Recycled Opt
						bakeResult.AddCount_RecycledOptGameObject();
					}

				}

				if (socketNode == null)
				{
					socketNode = AddGameObject<apOptNode>(meshGroupTransform._nickName + " Socket", optTransform.transform);

					//Count+1 : New Opt
					bakeResult.AddCount_NewOptGameObject();
				}
				optTransform._socketTransform = socketNode.transform;
			}
			else
			{
				optTransform._socketTransform = null;
			}


			if (parentTransform != null)
			{
				parentTransform.AddChildTransforms(optTransform);
			}

			//만약 Root라면 ->
			if (parentTransform == null)
			{
				targetOptRootUnit._rootOptTransform = optTransform;
			}
			targetOptPortrait._optTransforms.Add(optTransform);


			if (renderUnit != null)
			{
				for (int i = 0; i < renderUnit._childRenderUnits.Count; i++)
				{
					apRenderUnit childRenderUnit = renderUnit._childRenderUnits[i];

					apTransform_MeshGroup childTransform_MeshGroup = childRenderUnit._meshGroupTransform;
					apTransform_Mesh childTransform_Mesh = childRenderUnit._meshTransform;

					if (childTransform_MeshGroup != null)
					{
						MakeMeshGroupToOptTransform(childRenderUnit, childTransform_MeshGroup, optTransform.transform, optTransform, targetOptRootUnit, bakeLinkManager, bakeResult, bakeZScale, isGammaColorSpace, isLWRPShader, targetOptPortrait, rootMeshGroup, isSizeOptimizedV117, isUseSRP);
					}
					else if (childTransform_Mesh != null)
					{
						MakeMeshToOptTransform(childRenderUnit, childTransform_Mesh, meshGroup, optTransform.transform, optTransform, targetOptRootUnit, bakeLinkManager, bakeResult, bakeZScale, isGammaColorSpace, isLWRPShader, targetOptPortrait, rootMeshGroup, isSizeOptimizedV117, isUseSRP);
					}
					else
					{
						Debug.LogError("Empty Render Unit");
					}
				}
			}
			else
			{
				Debug.LogError("No RenderUnit");
			}

			#region [미사용 코드] Child 등록 코드 (RenderUnit 없음)
			//apMeshGroup meshGroup = meshGroupTransform._meshGroup;
			////Child를 연결하자
			//if (meshGroup != null)
			//{

			//	// Child Mesh를 등록한다.
			//	if (meshGroup._childMeshTransforms.Count > 0)
			//	{
			//		for (int i = 0; i < meshGroup._childMeshTransforms.Count; i++)
			//		{
			//			apTransform_Mesh childMeshTransform = meshGroup._childMeshTransforms[i];
			//			MakeMeshToOptTransform(childMeshTransform, meshGroup, optTransform.transform);
			//		}
			//	}

			//	//Child MeshGroup을 등록한다.
			//	if(meshGroup._childMeshGroupTransforms.Count > 0)
			//	{
			//		for (int i = 0; i < meshGroup._childMeshGroupTransforms.Count; i++)
			//		{
			//			apTransform_MeshGroup childMeshGroupTransform = meshGroup._childMeshGroupTransforms[i];
			//			MakeMeshGroupToOptTransform(childMeshGroupTransform, optTransform.transform);
			//		}
			//	}
			//} 
			#endregion
		}

		private void MakeMeshToOptTransform(apRenderUnit renderUnit,
												apTransform_Mesh meshTransform,
												apMeshGroup parentMeshGroup,
												Transform parent,
												apOptTransform parentTransform,
												apOptRootUnit targetOptRootUnit,
												apBakeLinkManager bakeLinkManager,
												apBakeResult bakeResult,
												float bakeZScale,
												bool isGammaColorSpace,
												bool isLWRPShader,
												apPortrait targetOptPortrait,
												apMeshGroup rootMeshGroup,
												bool isSizeOptimizedV117,
												bool isUseSRP)
		{
			apOptTransform optTransform = null;
			if (bakeLinkManager != null)
			{
				optTransform = bakeLinkManager.FindOptTransform(meshTransform, null);
				if (optTransform != null)
				{
					//재활용에 성공했다.
					optTransform.gameObject.name = meshTransform._nickName;
					optTransform.transform.parent = parent;

					optTransform.transform.localPosition = Vector3.zero;
					optTransform.transform.localRotation = Quaternion.identity;
					optTransform.transform.localScale = Vector3.one;

					//Count+1 : Recycled Opt
					bakeResult.AddCount_RecycledOptGameObject();
				}

			}

			if (optTransform == null)
			{
				//재활용에 실패했다면 생성
				optTransform = AddGameObject<apOptTransform>(meshTransform._nickName, parent);

				//Count+1 : New Opt
				bakeResult.AddCount_NewOptGameObject();
			}





			//OptTransform을 설정하자
			#region [미사용 코드] SetBasicSetting 함수로 대체
			//optTransform._transformID = meshTransform._transformUniqueID;
			//optTransform._transform = optTransform.transform;

			//optTransform._depth = meshTransform._depth;
			//optTransform._defaultMatrix = new apMatrix(meshTransform._matrix);

			////optTransform._transform.localPosition = optTransform._defaultMatrix.Pos3 - new Vector3(0.0f, 0.0f, (float)optTransform._depth * 0.1f);
			//optTransform._transform.localPosition = optTransform._defaultMatrix.Pos3 - new Vector3(0.0f, 0.0f, (float)optTransform._depth);
			//optTransform._transform.localRotation = Quaternion.Euler(0.0f, 0.0f, optTransform._defaultMatrix._angleDeg);
			//optTransform._transform.localScale = optTransform._defaultMatrix._scale; 
			#endregion

			optTransform.Bake(targetOptPortrait, //null, 
								parentTransform,
								targetOptRootUnit,
								meshTransform._nickName,
								meshTransform._transformUniqueID,
								-1,
								meshTransform._matrix,
								true,
								renderUnit._level, meshTransform._depth,
								meshTransform._isVisible_Default,
								meshTransform._meshColor2X_Default,
								bakeZScale,
								isSizeOptimizedV117);

			//Debug.Log("Mesh OptTransform Bake [" + optTransform.name + "] Pivot : " + meshTransform._matrix._pos);
			//첫 초기화 Matrix(No-Mod)를 만들어주자 - Mesh Bake에서 사용된다.
			if (optTransform._matrix_TF_ToParent == null) { optTransform._matrix_TF_ToParent = new apMatrix(); }
			if (optTransform._matrix_TF_ParentWorld_NonModified == null) { optTransform._matrix_TF_ParentWorld_NonModified = new apMatrix(); }
			if (optTransform._matrix_TFResult_WorldWithoutMod == null) { optTransform._matrix_TFResult_WorldWithoutMod = new apMatrix(); }

			optTransform._matrix_TF_ToParent.SetMatrix(optTransform._defaultMatrix);
			optTransform._matrix_TF_ParentWorld_NonModified.SetIdentity();
			if (parentTransform != null)
			{
				optTransform._matrix_TF_ParentWorld_NonModified.SetMatrix(parentTransform._matrix_TFResult_WorldWithoutMod);
			}
			optTransform._matrix_TFResult_WorldWithoutMod.SetIdentity();
			optTransform._matrix_TFResult_WorldWithoutMod.RMultiply(optTransform._matrix_TF_ToParent);
			optTransform._matrix_TFResult_WorldWithoutMod.RMultiply(optTransform._matrix_TF_ParentWorld_NonModified);


			//추가
			//소켓을 붙이자
			if (meshTransform._isSocket)
			{
				apOptNode socketNode = null;
				if (bakeLinkManager != null)
				{

					socketNode = bakeLinkManager.FindOptTransformSocket(optTransform);
					if (socketNode != null)
					{
						socketNode.gameObject.name = meshTransform._nickName + " Socket";
						socketNode.transform.parent = optTransform.transform;
						socketNode.transform.localPosition = Vector3.zero;
						socketNode.transform.localRotation = Quaternion.identity;
						socketNode.transform.localScale = Vector3.one;

						//Count+1 : Recycled Opt
						bakeResult.AddCount_RecycledOptGameObject();
					}

				}

				if (socketNode == null)
				{
					socketNode = AddGameObject<apOptNode>(meshTransform._nickName + " Socket", optTransform.transform);

					//Count+1 : New Opt
					bakeResult.AddCount_NewOptGameObject();
				}
				optTransform._socketTransform = socketNode.transform;
			}
			else
			{
				optTransform._socketTransform = null;
			}

			if (parentTransform != null)
			{
				parentTransform.AddChildTransforms(optTransform);
			}

			targetOptPortrait._optTransforms.Add(optTransform);

			//RootUnit에 등록하자
			targetOptRootUnit.AddChildTransform(optTransform, rootMeshGroup.SortedBuffer.GetBufferData(renderUnit));


			//하위에 OptMesh를 만들자
			apMesh mesh = meshTransform._mesh;
			if (mesh != null)
			{
				apOptMesh optMesh = null;

				if (bakeLinkManager != null)
				{
					optMesh = bakeLinkManager.FindOptMesh(optTransform);
					if (optMesh != null)
					{
						optMesh.gameObject.name = meshTransform._nickName + "_Mesh";
						optMesh.transform.parent = optTransform.transform;
						optMesh.transform.localPosition = Vector3.zero;
						optMesh.transform.localRotation = Quaternion.identity;
						optMesh.transform.localScale = Vector3.one;

						//필수 컴포넌트가 비었는지도 확인
						if (optMesh.GetComponent<MeshFilter>() == null)
						{
							optMesh.gameObject.AddComponent<MeshFilter>();
						}
						if (optMesh.GetComponent<MeshRenderer>() == null)
						{
							optMesh.gameObject.AddComponent<MeshRenderer>();
						}

						//Count+1 : Recycled Opt
						bakeResult.AddCount_RecycledOptGameObject();

					}
				}
				if (optMesh == null)
				{
					//재활용이 안되었으니 직접 만들자
					optMesh = AddGameObject<apOptMesh>(meshTransform._nickName + "_Mesh", optTransform.transform);
					optMesh.gameObject.AddComponent<MeshFilter>();
					optMesh.gameObject.AddComponent<MeshRenderer>();

					//Count+1 : New Opt
					bakeResult.AddCount_NewOptGameObject();
				}


				List<apVertex> verts = mesh._vertexData;

				List<Vector3> posList = new List<Vector3>();
				List<Vector2> UVList = new List<Vector2>();
				List<int> IDList = new List<int>();
				List<int> triList = new List<int>();
				List<float> zDepthList = new List<float>();

				apVertex vert = null;
				for (int i = 0; i < verts.Count; i++)
				{
					vert = verts[i];
					posList.Add(vert._pos);
					UVList.Add(vert._uv);
					IDList.Add(vert._uniqueID);
					zDepthList.Add(vert._zDepth);
				}

				for (int i = 0; i < mesh._indexBuffer.Count; i++)
				{
					triList.Add(mesh._indexBuffer[i]);
				}

				Texture2D texture = null;
				apOptTextureData optTextureData = null;//<<연결될 OptTextureData

				//이전 코드
				//if (mesh._textureData != null)
				//{
				//	texture = mesh._textureData._image;
				//	optTextureData = targetOptPortrait._optTextureData.Find(delegate (apOptTextureData a)
				//	{
				//		return a._srcUniqueID == mesh._textureData._uniqueID;
				//	});
				//}

				//변경 코드 4.1
				if (mesh.LinkedTextureData != null)
				{
					texture = mesh.LinkedTextureData._image;
					optTextureData = targetOptPortrait._optTextureData.Find(delegate (apOptTextureData a)
					{
						return a._srcUniqueID == mesh.LinkedTextureData._uniqueID;
					});
				}

				//Mesh Bake를 하자
				optMesh._portrait = targetOptPortrait;
				optMesh._uniqueID = meshTransform._transformUniqueID;

				//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

				//이전 : MaterialSet / Material Library를 사용하지 않는 경우
				////Shader 설정
				//Shader shaderNormal = GetOptMeshShader(meshTransform._shaderType, false, isGammaColorSpace, isLWRPShader);
				//Shader shaderMask = GetOptMeshShader(meshTransform._shaderType, true, isGammaColorSpace, isLWRPShader);
				//Shader shaderAlphaMask = GetOptAlphaMaskShader(isLWRPShader);
				//if (meshTransform._isCustomShader && meshTransform._customShader != null)
				//{
				//	shaderNormal = meshTransform._customShader;
				//	shaderMask = meshTransform._customShader;
				//}


				////통합 재질을 찾자
				//int batchedMatID = -1;
				//if (texture != null && optTextureData != null && !meshTransform._isClipping_Child)
				//{
				//	apOptBatchedMaterial.MaterialUnit batchedMatUnit = targetOptPortrait._optBatchedMaterial.MakeBatchedMaterial_Prev(texture, optTextureData._textureID, shaderNormal);
				//	if (batchedMatUnit != null)
				//	{
				//		batchedMatID = batchedMatUnit._uniqueID;
				//	}
				//}

				//변경 19.6.15 : Material Set / Material Library를 사용하는 경우
				//Mat Info 만들기 전에 다시 Mat Set 다시 설정
				if(meshTransform._isUseDefaultMaterialSet)
				{
					meshTransform._linkedMaterialSet = targetOptPortrait.GetDefaultMaterialSet();
					if(meshTransform._linkedMaterialSet != null)
					{
						meshTransform._materialSetID = meshTransform._linkedMaterialSet._uniqueID;
					}
				}
				else
				{
					if(meshTransform._materialSetID >= 0)
					{
						meshTransform._linkedMaterialSet = targetOptPortrait.GetMaterialSet(meshTransform._materialSetID);
						if (meshTransform._linkedMaterialSet == null)
						{
							//연결될 MatSet이 없다면.. > 기본값
							meshTransform._linkedMaterialSet = targetOptPortrait.GetDefaultMaterialSet();
							if (meshTransform._linkedMaterialSet != null)
							{
								meshTransform._materialSetID = meshTransform._linkedMaterialSet._uniqueID;
							}
							else
							{
								meshTransform._materialSetID = -1;
							}
						}
					}
					else
					{
						meshTransform._linkedMaterialSet = null;
					}
				}


				apOptMaterialInfo matInfo = new apOptMaterialInfo();
				int textureDataID = -1;
				if(meshTransform._mesh != null)
				{
					//기존 방식 (SrcUniqueID : 에디터용을 사용했다.)
					textureDataID = meshTransform._mesh.LinkedTextureDataID;

					apOptTextureData optTexData = targetOptPortrait._optTextureData.Find(delegate(apOptTextureData a)
					{
						return a._srcUniqueID == meshTransform._mesh.LinkedTextureDataID;
					});
					if(optTexData != null)
					{
						//Debug.Log("optTexData를 MatInfo로 저장 : " + optTexData._name + "(" + optTexData._srcUniqueID + ") : " + optTexData._textureID);
						textureDataID = optTexData._textureID;
					}
					else
					{
						//Debug.LogError("실패 : optTexData를 찾지 못했다. : " + meshTransform._mesh.LinkedTextureDataID);
						textureDataID = meshTransform._mesh.LinkedTextureDataID;
					}
					
				}
				matInfo.Bake(meshTransform, targetOptPortrait, !isGammaColorSpace, textureDataID, Editor.MaterialLibrary);

				Shader shader_AlphaMask = null;
				if(meshTransform._linkedMaterialSet != null)
				{
					shader_AlphaMask = meshTransform._linkedMaterialSet._shader_AlphaMask;
				}
				else
				{
					shader_AlphaMask = targetOptPortrait.GetDefaultMaterialSet()._shader_AlphaMask;
				}

				//Debug.LogError("Bake Mesh : " + meshTransform._nickName);

				//Material Info를 이용하여 BatchedMatID를 찾자
				int batchedMatID = -1;
				if (texture != null && optTextureData != null && !meshTransform._isClipping_Child)
				{
					apOptBatchedMaterial.MaterialUnit batchedMatUnit = targetOptPortrait._optBatchedMaterial.MakeBatchedMaterial_MatInfo(matInfo);
					if (batchedMatUnit != null)
					{
						batchedMatID = batchedMatUnit._uniqueID;
					}
				}
				//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

				//Render Texture 크기
				int maskRenderTextureSize = 0;
				switch (meshTransform._renderTexSize)
				{
					case apTransform_Mesh.RENDER_TEXTURE_SIZE.s_64:		maskRenderTextureSize = 64;		break;
					case apTransform_Mesh.RENDER_TEXTURE_SIZE.s_128:	maskRenderTextureSize = 128;	break;
					case apTransform_Mesh.RENDER_TEXTURE_SIZE.s_256:	maskRenderTextureSize = 256;	break;
					case apTransform_Mesh.RENDER_TEXTURE_SIZE.s_512:	maskRenderTextureSize = 512;	break;
					case apTransform_Mesh.RENDER_TEXTURE_SIZE.s_1024:	maskRenderTextureSize = 1024;	break;
					default:
						maskRenderTextureSize = 64;
						Debug.LogError("Unknown RenderTexture Size [" + meshTransform._renderTexSize + "]");
						break;
				}

				bool isVisibleDefault = true;

				if (!meshTransform._isVisible_Default)
				{
					isVisibleDefault = false;
				}
				else
				{
					//Parent로 올라가면서 VisibleDefault가 하나라도 false이면 false
					apRenderUnit curRenderUnit = renderUnit;
					while (true)
					{
						if (curRenderUnit == null) { break; }

						if (curRenderUnit._unitType == apRenderUnit.UNIT_TYPE.Mesh)
						{
							if (curRenderUnit._meshTransform != null)
							{
								if (!curRenderUnit._meshTransform._isVisible_Default)
								{
									isVisibleDefault = false;
									break;
								}
							}
							else
							{
								break;
							}
						}
						else if (curRenderUnit._unitType == apRenderUnit.UNIT_TYPE.GroupNode)
						{
							if (curRenderUnit._meshGroupTransform != null)
							{
								if (!curRenderUnit._meshGroupTransform._isVisible_Default)
								{
									isVisibleDefault = false;
									break;
								}
							}
							else
							{
								break;
							}
						}
						//위로 이동
						curRenderUnit = curRenderUnit._parentRenderUnit;
					}
				}

				//추가 : 그림자 설정
				apPortrait.SHADOW_CASTING_MODE shadowCastMode = targetOptPortrait._meshShadowCastingMode;
				bool receiveShadow = targetOptPortrait._meshReceiveShadow;
				if (!meshTransform._isUsePortraitShadowOption)
				{
					shadowCastMode = meshTransform._shadowCastingMode;
					receiveShadow = meshTransform._receiveShadow;
				}



				//이전 버전의 BakeMesh
				optMesh.BakeMesh(posList.ToArray(),
									UVList.ToArray(),
									IDList.ToArray(),
									triList.ToArray(),
									zDepthList.ToArray(),
									mesh._offsetPos,
									optTransform,
									texture,
									//<텍스쳐 ID가 들어가야 한다.
									(optTextureData != null ? optTextureData._textureID : -1),
									meshTransform._shaderType,

									//이전
									//shaderNormal,
									//shaderMask,
									//shaderAlphaMask,

									//변경 : 19.6.15 : Material Info 이용
									matInfo,
									shader_AlphaMask,

									maskRenderTextureSize,
									isVisibleDefault,
									meshTransform._isClipping_Parent,
									meshTransform._isClipping_Child,
									batchedMatID,
									//batchedMaterial,
									meshTransform._isAlways2Side,
									shadowCastMode,
									receiveShadow,

									isUseSRP
									);

				//역으로 OptTextureData에도 OptMesh를 등록
				if (optTextureData != null)
				{
					optTextureData.AddLinkOptMesh(optMesh);
				}

				//Clipping의 기본 정보를 넣고, 나중에 연결하자
				if (meshTransform._isClipping_Parent)
				{
					List<int> clipIDs = new List<int>();
					for (int iClip = 0; iClip < meshTransform._clipChildMeshes.Count; iClip++)
					{
						clipIDs.Add(meshTransform._clipChildMeshes[iClip]._transformID);
					}

					optMesh.SetMaskBasicSetting_Parent(clipIDs);
					//optMesh.SetMaskBasicSetting_Parent(meshTransform._clipChildMeshTransformIDs);

					//따로 관리할 마스크 메시에 넣는다.
					//마스크 메시에 추가하는 건 생략한다.
					//Editor._portrait._optMaskedMeshes.Add(optMesh);
				}
				else if (meshTransform._isClipping_Child)
				{
					optMesh.SetMaskBasicSetting_Child(meshTransform._clipParentMeshTransform._transformUniqueID);

					//마스크 메시에 추가하는 건 생략한다.
					//Editor._portrait._optClippedMeshes.Add(optMesh);
				}


				//Parent Transform에 등록하자
				optTransform.SetChildMesh(optMesh);

				targetOptPortrait._optMeshes.Add(optMesh);
			}
		}


		private Shader GetOptMeshShader(apPortrait.SHADER_TYPE shaderType, bool isClipping, bool isGammaColorSpace, bool isLightweightRenderPipeline)
		{
			string folderPath = null;
			string fileName = null;
			if (isGammaColorSpace)
			{
				if (!isLightweightRenderPipeline)
				{
					//Gamma + Default
					folderPath = apShaderGenerator.ShaderPath;

					switch (shaderType)
					{
						case apPortrait.SHADER_TYPE.AlphaBlend: fileName = (isClipping ? "apShader_ClippedWithMask" : "apShader_Transparent"); break;
						case apPortrait.SHADER_TYPE.Additive: fileName = (isClipping ? "apShader_ClippedWithMask_Additive" : "apShader_Transparent_Additive"); break;
						case apPortrait.SHADER_TYPE.Multiplicative: fileName = (isClipping ? "apShader_ClippedWithMask_Multiplicative" : "apShader_Transparent_Multiplicative"); break;
						case apPortrait.SHADER_TYPE.SoftAdditive: fileName = (isClipping ? "apShader_ClippedWithMask_SoftAdditive" : "apShader_Transparent_SoftAdditive"); break;
					}
				}
				else
				{
					//Gamma + LWRP
					folderPath = apShaderGenerator.ShaderPath_LWRP;

					switch (shaderType)
					{
						case apPortrait.SHADER_TYPE.AlphaBlend: fileName = (isClipping ? "apShader_LWRP_ClippedWithMask" : "apShader_LWRP_Transparent"); break;
						case apPortrait.SHADER_TYPE.Additive: fileName = (isClipping ? "apShader_LWRP_ClippedWithMask_Additive" : "apShader_LWRP_Transparent_Additive"); break;
						case apPortrait.SHADER_TYPE.Multiplicative: fileName = (isClipping ? "apShader_LWRP_ClippedWithMask_Multiplicative" : "apShader_LWRP_Transparent_Multiplicative"); break;
						case apPortrait.SHADER_TYPE.SoftAdditive: fileName = (isClipping ? "apShader_LWRP_ClippedWithMask_SoftAdditive" : "apShader_LWRP_Transparent_SoftAdditive"); break;
					}
				}
			}
			else
			{
				if (!isLightweightRenderPipeline)
				{
					//Linear + Default
					folderPath = apShaderGenerator.ShaderPath_Linear;

					switch (shaderType)
					{
						case apPortrait.SHADER_TYPE.AlphaBlend: fileName = (isClipping ? "apShader_L_ClippedWithMask" : "apShader_L_Transparent"); break;
						case apPortrait.SHADER_TYPE.Additive: fileName = (isClipping ? "apShader_L_ClippedWithMask_Additive" : "apShader_L_Transparent_Additive"); break;
						case apPortrait.SHADER_TYPE.Multiplicative: fileName = (isClipping ? "apShader_L_ClippedWithMask_Multiplicative" : "apShader_L_Transparent_Multiplicative"); break;
						case apPortrait.SHADER_TYPE.SoftAdditive: fileName = (isClipping ? "apShader_L_ClippedWithMask_SoftAdditive" : "apShader_L_Transparent_SoftAdditive"); break;
					}
				}
				else
				{
					//Linear + LWRP
					folderPath = apShaderGenerator.ShaderPath_Linear_LWRP;

					switch (shaderType)
					{
						case apPortrait.SHADER_TYPE.AlphaBlend: fileName = (isClipping ? "apShader_LWRP_L_ClippedWithMask" : "apShader_LWRP_L_Transparent"); break;
						case apPortrait.SHADER_TYPE.Additive: fileName = (isClipping ? "apShader_LWRP_L_ClippedWithMask_Additive" : "apShader_LWRP_L_Transparent_Additive"); break;
						case apPortrait.SHADER_TYPE.Multiplicative: fileName = (isClipping ? "apShader_LWRP_L_ClippedWithMask_Multiplicative" : "apShader_LWRP_L_Transparent_Multiplicative"); break;
						case apPortrait.SHADER_TYPE.SoftAdditive: fileName = (isClipping ? "apShader_LWRP_L_ClippedWithMask_SoftAdditive" : "apShader_LWRP_L_Transparent_SoftAdditive"); break;
					}
				}
			}

			return AssetDatabase.LoadAssetAtPath<Shader>(folderPath + "/" + fileName + ".shader");


			#region [미사용 코드] :Material에서 Shader를 추출하는 구식 방법
			//Material targetMat = null;
			//string materialAssetName = "";
			//switch (shaderType)
			//{
			//	case apPortrait.SHADER_TYPE.AlphaBlend:
			//		if (isGammaColorSpace)
			//		{
			//			if (!isClipping)	{ materialAssetName = "apMat_Opt_Normal"; }
			//			else				{ materialAssetName = "apMat_Opt_Clipped"; }
			//		}
			//		else
			//		{
			//			if (!isClipping)	{ materialAssetName = "apMat_L_Opt_Normal"; }
			//			else				{ materialAssetName = "apMat_L_Opt_Clipped"; }
			//		}
			//		break;

			//	case apPortrait.SHADER_TYPE.Additive:
			//		if (isGammaColorSpace)
			//		{
			//			if (!isClipping)	{ materialAssetName = "apMat_Opt_Normal Additive"; }
			//			else				{ materialAssetName = "apMat_Opt_Clipped Additive"; }
			//		}
			//		else
			//		{
			//			if (!isClipping)	{ materialAssetName = "apMat_L_Opt_Normal Additive"; }
			//			else				{ materialAssetName = "apMat_L_Opt_Clipped Additive"; }
			//		}

			//		break;

			//	case apPortrait.SHADER_TYPE.SoftAdditive:
			//		if(isGammaColorSpace)
			//		{
			//			if (!isClipping)	{ materialAssetName = "apMat_Opt_Normal SoftAdditive"; }
			//			else				{ materialAssetName = "apMat_Opt_Clipped SoftAdditive"; }
			//		}
			//		else
			//		{
			//			if (!isClipping)	{ materialAssetName = "apMat_L_Opt_Normal SoftAdditive"; }
			//			else				{ materialAssetName = "apMat_L_Opt_Clipped SoftAdditive"; }
			//		}

			//		break;

			//	case apPortrait.SHADER_TYPE.Multiplicative:
			//		if(isGammaColorSpace)
			//		{
			//			if (!isClipping)	{ materialAssetName = "apMat_Opt_Normal Multiplicative"; }
			//			else				{ materialAssetName = "apMat_Opt_Clipped Multiplicative"; }
			//		}
			//		else
			//		{
			//			if (!isClipping)	{ materialAssetName = "apMat_L_Opt_Normal Multiplicative"; }
			//			else				{ materialAssetName = "apMat_L_Opt_Clipped Multiplicative"; }
			//		}

			//		break;
			//}
			//if (string.IsNullOrEmpty(materialAssetName))
			//{
			//	return null;
			//}
			////경로 변경 : "Assets/Editor/AnyPortraitTool/" => apEditorUtil.ResourcePath_Material
			//if (isGammaColorSpace)
			//{
			//	targetMat = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + materialAssetName + ".mat");
			//}
			//else
			//{
			//	//Linear Color Space인 경우 저장된 위치가 다르다
			//	targetMat = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + "Linear/" + materialAssetName + ".mat");
			//}
			//if (targetMat == null)
			//{
			//	Debug.LogError("Error : Invalid Shader [" + materialAssetName + "]");
			//	return null;
			//}

			//return targetMat.shader; 
			#endregion
		}

		private Shader GetOptAlphaMaskShader(bool isLightweightRenderPipeline)
		{
			string assetPath = null;
			if (!isLightweightRenderPipeline)
			{
				//Default
				assetPath = apShaderGenerator.ShaderPath + "/" + "apShader_AlphaMask.shader";
			}
			else
			{
				//LWRP
				assetPath = apShaderGenerator.ShaderPath_LWRP + "/" + "apShader_LWRP_AlphaMask.shader";
			}

			return AssetDatabase.LoadAssetAtPath<Shader>(assetPath);

			#region [미사용 코드]
			//Material targetMat = null;
			//string materialAssetName = "apMat_Opt_AlphaMask";
			////경로 변경 : "Assets/Editor/AnyPortraitTool/" => apEditorUtil.ResourcePath_Material
			//targetMat = AssetDatabase.LoadAssetAtPath<Material>(apEditorUtil.ResourcePath_Material + materialAssetName + ".mat");
			//if (targetMat == null)
			//{
			//	Debug.LogError("Error : Invalid Shader [" + materialAssetName + "]");
			//	return null;
			//}

			//return targetMat.shader; 
			#endregion
		}

		private void MakeOptBone(apMeshGroup srcMeshGroup,
									apOptTransform targetOptTransform,
									apOptRootUnit targetOptRootUnit,
									apBakeLinkManager bakeLinkManager,
									apBakeResult bakeResult)
		{
			//1. Bone Group을 만들고
			//2. Bone을 계층적으로 추가하자 (재귀 함수 필요)

			apOptNode boneGroupNode = null;
			if (bakeLinkManager != null)
			{
				boneGroupNode = bakeLinkManager.FindOptBoneGroupNode();
				if (boneGroupNode != null)
				{
					boneGroupNode.gameObject.name = "__Bone Group";
					boneGroupNode.transform.parent = targetOptTransform.transform;
					boneGroupNode.transform.localPosition = Vector3.zero;
					boneGroupNode.transform.localRotation = Quaternion.identity;
					boneGroupNode.transform.localScale = Vector3.one;

					boneGroupNode._param = 100;

					//Count+1 : Recycled Opt
					bakeResult.AddCount_RecycledOptGameObject();

				}
			}
			if (boneGroupNode == null)
			{
				boneGroupNode = AddGameObject<apOptNode>("__Bone Group", targetOptTransform.transform);
				boneGroupNode._param = 100;//<<Bone Group의 Param은 100이다.

				//Count+1 : New Opt
				bakeResult.AddCount_NewOptGameObject();
			}


			targetOptTransform._boneGroup = boneGroupNode.transform;
			targetOptTransform._boneList_All = null;
			targetOptTransform._boneList_Root = null;
			targetOptTransform._isBoneUpdatable = true;

			List<apBone> rootBones = srcMeshGroup._boneList_Root;
			List<apOptBone> totalOptBones = new List<apOptBone>();
			for (int i = 0; i < rootBones.Count; i++)
			{
				apOptBone newRootBone = MakeOptBoneRecursive(srcMeshGroup, rootBones[i], null, targetOptTransform, targetOptRootUnit, totalOptBones, bakeLinkManager, bakeResult);
				targetOptTransform._boneList_Root = apEditorUtil.AddItemToArray<apOptBone>(newRootBone, targetOptTransform._boneList_Root);
			}

			targetOptTransform._boneList_All = totalOptBones.ToArray();



			int nBones = totalOptBones.Count;
			//이제 전체 Bone을 돌면서 링크를 해주자
			//TODO
			for (int i = 0; i < totalOptBones.Count; i++)
			{
				totalOptBones[i].Link(targetOptTransform);

			}
			//Root에서부터 LinkChaining을 실행하자
			for (int i = 0; i < targetOptTransform._boneList_Root.Length; i++)
			{
				targetOptTransform._boneList_Root[i].LinkBoneChaining();
			}
		}

		private apOptBone MakeOptBoneRecursive(apMeshGroup srcMeshGroup,
												apBone srcBone,
												apOptBone parentOptBone,
												apOptTransform targetOptTransform,
												apOptRootUnit targetOptRootUnit,
												List<apOptBone> resultOptBones,
												apBakeLinkManager bakeLinkManager,
												apBakeResult bakeResult)
		{
			Transform parentTransform = targetOptTransform._boneGroup;
			if (parentOptBone != null)
			{
				parentTransform = parentOptBone.transform;
			}
			apOptBone newBone = null;

			if (bakeLinkManager != null)
			{
				newBone = bakeLinkManager.FindOptBone(srcBone);
				if (newBone != null)
				{
					newBone.gameObject.name = srcBone._name;
					newBone.transform.parent = parentTransform;
					newBone.transform.localPosition = Vector3.zero;
					newBone.transform.localRotation = Quaternion.identity;
					newBone.transform.localScale = Vector3.one;

					//Count+1 : Recycled Opt
					bakeResult.AddCount_RecycledOptGameObject();
				}

			}
			if (newBone == null)
			{
				newBone = AddGameObject<apOptBone>(srcBone._name, parentTransform);

				//Count+1 : New Opt
				bakeResult.AddCount_NewOptGameObject();
			}

			srcBone.GUIUpdate(false);

			//TODO : Bake 해야한다.
			//Link를 제외한 Bake를 먼저 하자.
			//Link는 ID를 이용하여 일괄적으로 처리
			newBone.Bake(srcBone);

			//RootUnit에 등록하자
			targetOptRootUnit.AddChildBone(newBone);


			if (srcBone._isSocketEnabled)
			{
				//소켓을 붙여주자
				apOptNode socketNode = null;
				if (bakeLinkManager != null)
				{
					socketNode = bakeLinkManager.FindOptBoneSocket(newBone);
					if (socketNode != null)
					{
						socketNode.gameObject.name = srcBone._name + " Socket";
						socketNode.transform.parent = newBone.transform;
						socketNode.transform.localPosition = Vector3.zero;
						socketNode.transform.localRotation = Quaternion.identity;
						socketNode.transform.localScale = Vector3.one;

						//Count+1 : Recycled Opt
						bakeResult.AddCount_RecycledOptGameObject();
					}

				}

				if (socketNode == null)
				{
					socketNode = AddGameObject<apOptNode>(srcBone._name + " Socket", newBone.transform);

					//Count+1 : New Opt
					bakeResult.AddCount_NewOptGameObject();
				}
				newBone._socketTransform = socketNode.transform;
			}

			if (parentOptBone != null)
			{
				newBone._parentBone = parentOptBone;
				parentOptBone._childBones = apEditorUtil.AddItemToArray<apOptBone>(newBone, parentOptBone._childBones);
			}


			resultOptBones.Add(newBone);
			//하위 Child Bone에 대해서도 반복

			for (int i = 0; i < srcBone._childBones.Count; i++)
			{
				MakeOptBoneRecursive(srcMeshGroup,
										srcBone._childBones[i],
										newBone,
										targetOptTransform,
										targetOptRootUnit,
										resultOptBones,
										bakeLinkManager,
										bakeResult);
			}


			return newBone;
		}



		/// <summary>
		/// 만약 사용하지 않는 Monobehaviour 객체가 있는 경우 삭제를 해야한다.
		/// </summary>
		/// <param name="portrait"></param>
		public void CheckAndRemoveUnusedMonobehaviours(apPortrait portrait)
		{
			if (portrait == null)
			{
				return;
			}
			//Monobehaiour는 Mesh, MeshGroup, Modifier이다.
			if (portrait._subObjectGroup_Mesh == null ||
				portrait._subObjectGroup_MeshGroup == null ||
				portrait._subObjectGroup_Modifier == null)
			{
				return;
			}
			//실제로 존재하는 데이터를 정리한다.
			List<GameObject> meshObjects = new List<GameObject>();
			List<GameObject> meshGroupObjects = new List<GameObject>();
			List<GameObject> modifierObjects = new List<GameObject>();

			apMesh mesh = null;
			apMeshGroup meshGroup = null;
			apModifierBase modifier = null;

			for (int i = 0; i < portrait._meshes.Count; i++)
			{
				mesh = portrait._meshes[i];
				if (mesh == null) { continue; }

				meshObjects.Add(mesh.gameObject);
			}

			for (int i = 0; i < portrait._meshGroups.Count; i++)
			{
				meshGroup = portrait._meshGroups[i];
				if (meshGroup == null) { continue; }

				meshGroupObjects.Add(meshGroup.gameObject);

				for (int iMod = 0; iMod < meshGroup._modifierStack._modifiers.Count; iMod++)
				{
					modifier = meshGroup._modifierStack._modifiers[iMod];
					if (modifier == null) { continue; }

					modifierObjects.Add(modifier.gameObject);
				}
			}

			//이제 Child GameObject를 확인하자
			int nChild_Mesh = portrait._subObjectGroup_Mesh.transform.childCount;
			int nChild_MeshGroup = portrait._subObjectGroup_MeshGroup.transform.childCount;
			int nChild_Modifier = portrait._subObjectGroup_Modifier.transform.childCount;
			List<GameObject> unusedGameObjects = new List<GameObject>();

			GameObject curGameObject = null;

			//1. Mesh
			for (int i = 0; i < nChild_Mesh; i++)
			{
				curGameObject = portrait._subObjectGroup_Mesh.transform.GetChild(i).gameObject;
				if (!meshObjects.Contains(curGameObject))
				{
					//안쓰는게 나왔다.
					unusedGameObjects.Add(curGameObject);
				}
			}

			//2. MeshGroup
			for (int i = 0; i < nChild_MeshGroup; i++)
			{
				curGameObject = portrait._subObjectGroup_MeshGroup.transform.GetChild(i).gameObject;
				if (!meshGroupObjects.Contains(curGameObject))
				{
					//안쓰는게 나왔다.
					unusedGameObjects.Add(curGameObject);
				}
			}

			//3. Modifier
			for (int i = 0; i < nChild_Modifier; i++)
			{
				curGameObject = portrait._subObjectGroup_Modifier.transform.GetChild(i).gameObject;
				if (!modifierObjects.Contains(curGameObject))
				{
					//안쓰는게 나왔다.
					unusedGameObjects.Add(curGameObject);
				}
			}

			if (unusedGameObjects.Count > 0)
			{
				//Debug.LogError("삭제되어야 하는 게임 오브젝트가 나왔다.");
				for (int i = 0; i < unusedGameObjects.Count; i++)
				{
					//Debug.LogError("[" + i + "] " + unusedGameObjects[i].name);
					Undo.DestroyObjectImmediate(unusedGameObjects[i]);
				}
			}
		}


		/// <summary>
		/// GameObject들의 이름을 갱신하자
		/// Mesh, MeshGroup이 그 대상
		/// </summary>
		/// <param name="portrait"></param>
		public void CheckAndRefreshGameObjectNames(apPortrait portrait)
		{
			//숨어있는 GameObject들의 이름을 갱신한다.
			if (portrait == null)
			{
				return;
			}
			if (portrait._subObjectGroup_Mesh == null ||
				portrait._subObjectGroup_MeshGroup == null ||
				portrait._subObjectGroup_Modifier == null)
			{
				return;
			}
			apMesh mesh = null;
			apMeshGroup meshGroup = null;

			for (int i = 0; i < portrait._meshes.Count; i++)
			{
				mesh = portrait._meshes[i];
				if (mesh == null) { continue; }

				mesh.gameObject.name = mesh._name;
			}

			for (int i = 0; i < portrait._meshGroups.Count; i++)
			{
				meshGroup = portrait._meshGroups[i];
				if (meshGroup == null) { continue; }

				meshGroup.gameObject.name = meshGroup._name;
			}
		}

		//추가

		//버전 1.1.6에서 애니메이션 경로가 "절대 경로"에서 "상대 경로"로 바뀌었다.
		//절대 경로인지 확인하여 상대 경로로 전환한다.
		private void CheckAnimationsBasePathForV116(apPortrait targetPortrait)
		{
			string basePath = targetPortrait._mecanimAnimClipResourcePath;

			if (!string.IsNullOrEmpty(basePath))
			{
				//경로를 체크하자
				apEditorUtil.PATH_INFO_TYPE pathInfo = apEditorUtil.GetPathInfo(basePath);
				switch (pathInfo)
				{
					case apEditorUtil.PATH_INFO_TYPE.Absolute_InAssetFolder:
						{
							//Asset 안의 절대 경로 >> 메시지 없이 바로 상대 경로로 바꾼다.
							apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_BakeOptionChanged, _editor, targetPortrait, targetPortrait, false);
							targetPortrait._mecanimAnimClipResourcePath = apEditorUtil.AbsolutePath2RelativePath(basePath);
						}
						break;

					case apEditorUtil.PATH_INFO_TYPE.Absolute_OutAssetFolder:
					case apEditorUtil.PATH_INFO_TYPE.NotValid:
					case apEditorUtil.PATH_INFO_TYPE.Relative_OutAssetFolder:
						{
							//잘못된 경로이므로 다시 지정하라고 안내
							//1. 일단 안내 메시지를 띄운다 > 
							//2. Okay인 경우 > Save Panel 을 띄운다.
							//3. Save Panel에서 유효한 Path를 리턴 받은 경우 검사
							//4. 유효한 경로라면 저장, 아니라면 다시 경고 메시지 (이때는 저장 불가)
							bool isReset = EditorUtility.DisplayDialog(_editor.GetText(TEXT.DLG_AnimClipSavePathValidationError_Title),
								_editor.GetText(TEXT.DLG_AnimClipSavePathValidationError_Body),
								_editor.GetText(TEXT.Okay),
								_editor.GetText(TEXT.Cancel));

							if (isReset)
							{
								string nextPath = EditorUtility.SaveFolderPanel("Select to export animation clips", "", "");

								if (!string.IsNullOrEmpty(nextPath))
								{
									if (apEditorUtil.IsInAssetsFolder(nextPath))
									{
										//유효한 폴더인 경우
										//중요 : 경로가 절대 경로로 찍힌다.
										//상대 경로로 바꾸자
										apEditorUtil.PATH_INFO_TYPE pathInfoType = apEditorUtil.GetPathInfo(nextPath);
										if (pathInfoType == apEditorUtil.PATH_INFO_TYPE.Absolute_InAssetFolder)
										{
											//절대 경로 + Asset 폴더 안쪽이라면
											nextPath = apEditorUtil.AbsolutePath2RelativePath(nextPath);

										}

										apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_BakeOptionChanged, _editor, targetPortrait, targetPortrait, false);

										targetPortrait._mecanimAnimClipResourcePath = nextPath;
									}
									else
									{
										//유효한 폴더가 아닌 경우
										EditorUtility.DisplayDialog(
													_editor.GetText(TEXT.DLG_AnimClipSavePathValidationError_Title),
													_editor.GetText(TEXT.DLG_AnimClipSavePathResetError_Body),
													_editor.GetText(TEXT.Close));
									}
								}
							}
						}
						break;

					case apEditorUtil.PATH_INFO_TYPE.Relative_InAssetFolder:
						//Asset 안의 상대 경로 >> 그대로 둔다. >> 근데 %20이 포함되어 있다면?
						if(basePath.Contains("%20"))
						{
							string nextPath = apEditorUtil.DecodeURLEmptyWord(basePath);

							apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Portrait_BakeOptionChanged, _editor, targetPortrait, targetPortrait, false);
							targetPortrait._mecanimAnimClipResourcePath = nextPath;
						}

						break;
				}
			}
		}


		private bool CreateAnimationsWithMecanim(apPortrait targetPortrait, string basePath)
		{
			if (targetPortrait == null)
			{
				return false;
			}
			if (!targetPortrait._isUsingMecanim)
			{
				return false;
			}

			//Debug.Log("Base Path : " + basePath);

			//1. 경로 체크
			if (!basePath.EndsWith("/"))
			{
				basePath += "/";
			}

			basePath = basePath.Replace("\\", "/");


			System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(basePath);

			//변경 3.24 : basePath가 절대 경로에서 상대 경로(Assets로 시작되는..)로 바뀌었다.
			//보통은 그래도 경로가 인식이 되는데, 경로 인식이 안된다면 Asset 폴더의 절대 경로를 한번 더 붙여주자
			if (!di.Exists)
			{
				if (basePath.StartsWith("Assets"))
				{
					//상대 경로로서 충분하다면
					string projectRootPath = Application.dataPath;
					//뒤의 Assets을 빼자 (6글자 빼자)
					projectRootPath = projectRootPath.Substring(0, projectRootPath.Length - 6);

					//루트 + / 로 되어 있을 것
					string absPath = projectRootPath + basePath;

					di = new System.IO.DirectoryInfo(absPath);
				}
			}

			if (!di.Exists)
			{
				Debug.LogError("AnyPortrait : Wrong Animation Clip Destination Folder [" + basePath + "]");
				return false;
			}

			string fullPath = di.FullName;

			//AssetDataBase는 Assets 부터 시작해야한다.
			string projectPath = Application.dataPath + "/";

			//Debug.Log("DataPath : " + projectPath);
			//Debug.Log("BasePath : " + basePath);

			System.Uri uri_dataPath = new Uri(projectPath);
			//System.Uri uri_basePath = new Uri(basePath);
			System.Uri uri_basePath = new Uri(fullPath);

			if (!apEditorUtil.IsInAssetsFolder(fullPath))
			{
				Debug.LogError("AnyPortrait : Wrong Animation Clip Destination Folder [" + fullPath + "]");
				return false;
			}

			//string relativePath = "Assets/" + uri_dataPath.MakeRelativeUri(uri_basePath).ToString();
			string relativePath = uri_dataPath.MakeRelativeUri(uri_basePath).ToString();

			if (!relativePath.StartsWith("Assets/"))
			{
				relativePath = "Assets/" + relativePath;
			}
			if (!relativePath.EndsWith("/"))
			{
				relativePath += "/";
			}
			//Debug.Log("AnimClip Result Path : " + relativePath);

			//2. Animator 체크
			if (targetPortrait._animator == null)
			{
				targetPortrait._animator = targetPortrait.gameObject.AddComponent<Animator>();
			}

			//3. AnimatorController 있는지 체크 > 없다면 만든다. 다만 있을 경우엔 더이상 수정하지 않는다.
			UnityEditor.Animations.AnimatorController newAnimController = null;
			UnityEditor.Animations.AnimatorController runtimeAnimController = null;
			if (targetPortrait._animator.runtimeAnimatorController == null)
			{
				//AnimatorController는 파일 덮어쓰기는 아예 안되고, 새로 만드는 것만 가능
				//새로 만들자

				string animControllerPath = relativePath + targetPortrait.name + "-AnimController";
				string animControllerExp = "controller";
				string animControllerFullPath = GetNewUniqueAssetName(animControllerPath, animControllerExp, typeof(UnityEditor.Animations.AnimatorController));

				newAnimController = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(animControllerFullPath);
				targetPortrait._animator.runtimeAnimatorController = newAnimController;
			}
			else
			{
				runtimeAnimController = targetPortrait._animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
			}

			//4. 애니메이션 클립 체크
			//이미 AnimationClip이 있다면 덮어씌운다.
			//없다면 새로 생성한다. 이때 에셋 이름은 "충돌되지 않게" 만든다.

			for (int iAnim = 0; iAnim < targetPortrait._animClips.Count; iAnim++)
			{
				AnimationClip createdAnimClipAsset = CreateAnimationClipAsset(targetPortrait, targetPortrait._animClips[iAnim], relativePath);
				if (createdAnimClipAsset != null)
				{
					//데이터를 저장하자
					//targetPortrait._animClipAssetPairs.Add(new apAnimMecanimData_AssetPair(targetPortrait._animClips[iAnim]._uniqueID, createdAnimClipAsset));
					targetPortrait._animClips[iAnim]._animationClipForMecanim = createdAnimClipAsset;

					if (newAnimController != null)
					{
						//자동으로 생성된 AnimController가 있는 경우
						if (newAnimController.layers.Length > 0)
						{
							//animController.layers[0].stateMachine.AddStateMachineBehaviour()
							UnityEditor.Animations.AnimatorState newMotionState = newAnimController.AddMotion(createdAnimClipAsset, 0);
							newMotionState.motion = createdAnimClipAsset;
							newMotionState.name = targetPortrait._animClips[iAnim]._name;

						}
					}
				}
			}

			//추가 : "비어있는 애니메이션 클립"을 만든다.
			AnimationClip emptyAnimClipAsset = CreateEmptyAnimationClipAsset(targetPortrait, relativePath);
			targetPortrait._emptyAnimClipForMecanim = emptyAnimClipAsset;

			if (newAnimController != null)
			{
				//자동으로 생성된 AnimController가 있는 경우
				if (newAnimController.layers.Length > 0)
				{
					//animController.layers[0].stateMachine.AddStateMachineBehaviour()
					UnityEditor.Animations.AnimatorState newMotionState = newAnimController.AddMotion(emptyAnimClipAsset, 0);
					newMotionState.motion = emptyAnimClipAsset;
					newMotionState.name = "Empty";
				}
			}



			//4. 1차적으로 레이어 Refresh
			//이름으로 비교하여 없으면 추가, 있으면 넣기 방식으로 갱신한다.
			List<apAnimMecanimData_Layer> mecanimLayers = new List<apAnimMecanimData_Layer>();

			if (newAnimController != null || runtimeAnimController != null)
			{
				UnityEditor.Animations.AnimatorController curAnimController = null;
				if (newAnimController != null)
				{
					curAnimController = newAnimController;
				}
				else
				{
					curAnimController = runtimeAnimController;
				}

				if (curAnimController.layers != null && curAnimController.layers.Length > 0)
				{
					for (int iLayer = 0; iLayer < curAnimController.layers.Length; iLayer++)
					{
						apAnimMecanimData_Layer newLayerData = new apAnimMecanimData_Layer();
						newLayerData._layerIndex = iLayer;
						newLayerData._layerName = curAnimController.layers[iLayer].name;
						newLayerData._blendType = apAnimMecanimData_Layer.MecanimLayerBlendType.Unknown;
						switch (curAnimController.layers[iLayer].blendingMode)
						{
							case UnityEditor.Animations.AnimatorLayerBlendingMode.Override:
								newLayerData._blendType = apAnimMecanimData_Layer.MecanimLayerBlendType.Override;
								break;

							case UnityEditor.Animations.AnimatorLayerBlendingMode.Additive:
								newLayerData._blendType = apAnimMecanimData_Layer.MecanimLayerBlendType.Additive;
								break;
						}
						mecanimLayers.Add(newLayerData);
					}
				}

				targetPortrait._animatorLayerBakedData.Clear();
				for (int i = 0; i < mecanimLayers.Count; i++)
				{
					targetPortrait._animatorLayerBakedData.Add(new apAnimMecanimData_Layer(mecanimLayers[i]));
				}
			}



			apEditorUtil.SetEditorDirty();
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			return true;
		}

		//Animation Clip 만들기
		private AnimationClip CreateAnimationClipAsset(apPortrait targetPortrait, apAnimClip targetAnimClip, string basePath)
		{
			//이미 AnimationClip이 있다면 덮어씌운다.
			//없다면 새로 생성한다. 이때 에셋 이름은 "충돌되지 않게" 만든다.

			float timeLength = targetAnimClip.TimeLength;

			AnimationClip resultAnimClip = null;



			string animClipAssetPath = "";
			bool isCreate = false;
			if (targetAnimClip._animationClipForMecanim != null)
			{
				//1. 이미 저장된 AnimationClip이 있는 경우
				//> 저장된 에셋 Path와 이름을 공유한다. 
				//> 해당 에셋을 덮어씌운다.
				//수정 : 덮어씌우지 말고 이걸 그냥 수정할 순 없을까
				resultAnimClip = targetAnimClip._animationClipForMecanim;
				animClipAssetPath = AssetDatabase.GetAssetPath(targetAnimClip._animationClipForMecanim);
				isCreate = false;
			}
			else
			{
				isCreate = true;
			}

			if (string.IsNullOrEmpty(animClipAssetPath))
			{
				//2. 새로 만들어야 하는 경우 or Asset 경로를 찾지 못했을 경우
				//> "겹치지 않는 이름"으로 생성한다.
				resultAnimClip = new AnimationClip();
				resultAnimClip.name = targetPortrait.name + "-" + targetAnimClip._name;
				animClipAssetPath = GetNewUniqueAssetName(basePath + resultAnimClip.name, "anim", typeof(AnimationClip));
				isCreate = true;
			}

			resultAnimClip.legacy = false;
			if (targetAnimClip.IsLoop)
			{
				resultAnimClip.wrapMode = WrapMode.Loop;
			}
			else
			{
				resultAnimClip.wrapMode = WrapMode.Once;
			}
			AnimationUtility.SetEditorCurve(resultAnimClip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.x"), AnimationCurve.Linear(0.0f, 0.0f, timeLength, 0.0f));
			AnimationUtility.SetEditorCurve(resultAnimClip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.y"), AnimationCurve.Linear(0.0f, 0.0f, timeLength, 0.0f));
			AnimationUtility.SetEditorCurve(resultAnimClip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.z"), AnimationCurve.Linear(0.0f, 0.0f, timeLength, 0.0f));

			if (isCreate)
			{
				AssetDatabase.CreateAsset(resultAnimClip, animClipAssetPath);
			}


			return AssetDatabase.LoadAssetAtPath<AnimationClip>(animClipAssetPath);

		}



		private string GetNewUniqueAssetName(string assetPathWOExtension, string extension, Type type)
		{
			if (AssetDatabase.LoadAssetAtPath(assetPathWOExtension + "." + extension, type) != null)
			{
				//에셋이 이미 존재한다.
				//이름을 바꾼다.
				int newNameIndex = 1;
				string newName = "";
				while (true)
				{
					newName = assetPathWOExtension + " (" + newNameIndex + ")." + extension;

					if (AssetDatabase.LoadAssetAtPath(newName, type) == null)
					{
						//겹치는게 없다.
						return newName;//새로운 이름을 찾았다.
					}

					newNameIndex++;
				}
			}
			else
			{
				//에셋이 없다. 그대로 사용하자
				return assetPathWOExtension + "." + extension;
			}
		}



		private AnimationClip CreateEmptyAnimationClipAsset(apPortrait targetPortrait, string basePath)
		{
			//이미 AnimationClip이 있다면 덮어씌운다.
			//없다면 새로 생성한다. 이때 에셋 이름은 "충돌되지 않게" 만든다.

			float timeLength = 1.0f;

			if (targetPortrait._emptyAnimClipForMecanim != null)
			{
				return targetPortrait._emptyAnimClipForMecanim;
			}
			AnimationClip resultAnimClip = new AnimationClip();
			resultAnimClip.name = targetPortrait.name + "-Empty";
			string animClipAssetPath = GetNewUniqueAssetName(basePath + resultAnimClip.name, "anim", typeof(AnimationClip));

			resultAnimClip.legacy = false;
			resultAnimClip.wrapMode = WrapMode.Loop;

			AnimationUtility.SetEditorCurve(resultAnimClip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.x"), AnimationCurve.Linear(0.0f, 0.0f, timeLength, 0.0f));
			AnimationUtility.SetEditorCurve(resultAnimClip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.y"), AnimationCurve.Linear(0.0f, 0.0f, timeLength, 0.0f));
			AnimationUtility.SetEditorCurve(resultAnimClip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.z"), AnimationCurve.Linear(0.0f, 0.0f, timeLength, 0.0f));

			AssetDatabase.CreateAsset(resultAnimClip, animClipAssetPath);
			return AssetDatabase.LoadAssetAtPath<AnimationClip>(animClipAssetPath);

		}


		//19.8.5 : LWRP 옵션은 있어도 Material Library
		//		//추가 11.6 : 만약 LWRP Shader를 사용한다면, 
		//		private void CheckAndCreateLWRPShader()
		//		{
		//			if (_editor == null)
		//			{
		//				return;
		//			}
		//#if UNITY_2018_1_OR_NEWER
		//#else
		//			//하위버전에서는 설정이 False로 강제된다.
		//			_editor._isUseLWRPShader = false;
		//#endif
		//			if (!_editor._isUseLWRPShader)
		//			{
		//				return;
		//			}

		//			//LWRP Shader를 사용한다.
		//			apShaderGenerator shaderGen = new apShaderGenerator();
		//			if (!shaderGen.IsAnyMissingLWRPShader)
		//			{
		//				//모두 로드되었다고 합니다.
		//				return;
		//			}

		//			//LWRP Shader를 만들자.
		//			shaderGen.GenerateLWRPShaders();
		//		} 
		


		//------------------------------------------------------------------------------------
		// Optimized Bake
		//------------------------------------------------------------------------------------
		/// <summary>
		/// 현재 Portrait를 실행가능한 버전으로 Bake하자
		/// </summary>
		public apBakeResult OptimizedBake(apPortrait srcPortrait, apPortrait targetOptPortrait)
		{
			if (srcPortrait == null)
			{
				return null;
			}


			//추가 19.5.26 : v1.1.7에 추가된 "용량 최적화 옵션"이 적용되어 Bake를 하는가?
			bool isSizeOptimizedV117 = true;
			//bool isSizeOptimizedV117 = false;//테스트

			//추가 19.8.5
			bool isUseSRP = Editor._isUseSRP;

			apEditorUtil.SetEditorDirty();

			apBakeResult bakeResult = new apBakeResult();

			//Optimized에서 타겟이 되는 Portrait가 없다면 새로 만들어준다.
			if (targetOptPortrait == null)
			{
				GameObject dstPortraitGameObj = new GameObject(srcPortrait.gameObject.name + " (Optimized)");
				dstPortraitGameObj.transform.parent = srcPortrait.transform.parent;
				dstPortraitGameObj.transform.localPosition = srcPortrait.transform.localPosition;
				dstPortraitGameObj.transform.localRotation = srcPortrait.transform.localRotation;
				dstPortraitGameObj.transform.localScale = srcPortrait.transform.localScale;

				dstPortraitGameObj.layer = srcPortrait.gameObject.layer;

				targetOptPortrait = dstPortraitGameObj.AddComponent<apPortrait>();
			}

			//< Optimized Bake와 일반 Bake의 차이 >
			//- 순서는 일반 Bake와 동일하게 처리된다. (참조 에러를 막기 위해 Instantiate 등의 방법을 제외한다)
			//- 생성/제거되는 GameObject는 모두 taretOptPortrait에 속한다.
			//- 데이터는 srcPortrait에서 가져온다.
			//- 이 코드내에 Editor._portrait는 한번도 등장해선 안된다.

			//< 일단 Bake 했으니 초기 정보를 연결해준다. >
			//0. Bake 했다는 기본 정보 복사
			targetOptPortrait._isOptimizedPortrait = true;
			targetOptPortrait._bakeSrcEditablePortrait = srcPortrait;
			srcPortrait._bakeTargetOptPortrait = targetOptPortrait;

			//Editable GameObject로 저장되는 정보를 제외하고 모두 복사한다.
			//1. Controller 복사
			targetOptPortrait._controller._portrait = targetOptPortrait;
			targetOptPortrait._controller._controlParams.Clear();

			for (int iCP = 0; iCP < srcPortrait._controller._controlParams.Count; iCP++)
			{
				apControlParam srcParam = srcPortrait._controller._controlParams[iCP];

				apControlParam newParam = new apControlParam();
				newParam._portrait = targetOptPortrait;
				newParam.CopyFromControlParam(srcParam);//<<복사하자

				//리스트에 추가
				targetOptPortrait._controller._controlParams.Add(newParam);
			}

			//2. AnimClip 복사 (링크정보에 관한건 제외하고)
			// (AnimPlayManager는 나중에 Link하면 자동으로 연결됨)

			//추가 10.5 : 기존에 생성되었던 Animation Clip Asset은 없어지면 안된다.
			Dictionary<int, AnimationClip> animID2AnimAssets = new Dictionary<int, AnimationClip>();
			if (targetOptPortrait._animClips != null && targetOptPortrait._animClips.Count > 0)
			{
				for (int i = 0; i < targetOptPortrait._animClips.Count; i++)
				{
					apAnimClip beforeAnimClip = targetOptPortrait._animClips[i];
					if (beforeAnimClip != null && beforeAnimClip._animationClipForMecanim != null)
					{
						if (!animID2AnimAssets.ContainsKey(beforeAnimClip._uniqueID))
						{
							animID2AnimAssets.Add(beforeAnimClip._uniqueID, beforeAnimClip._animationClipForMecanim);
						}
					}
				}
			}
			targetOptPortrait._animClips.Clear();

			for (int iAnim = 0; iAnim < srcPortrait._animClips.Count; iAnim++)
			{
				apAnimClip srcAnimClip = srcPortrait._animClips[iAnim];

				//AnimClip을 Src로 부터 복사해서 넣자
				apAnimClip newAnimClip = new apAnimClip();
				newAnimClip.CopyFromAnimClip(srcAnimClip);

				if (animID2AnimAssets.ContainsKey(newAnimClip._uniqueID))
				{
					//추가 : Mecanim에 사용된 AnimAsset을 재활용해야한다.
					newAnimClip._animationClipForMecanim = animID2AnimAssets[newAnimClip._uniqueID];
				}

				targetOptPortrait._animClips.Add(newAnimClip);
			}

			//3. MainMeshGroup ID 복사
			targetOptPortrait._mainMeshGroupIDList.Clear();
			for (int iMainMG = 0; iMainMG < srcPortrait._mainMeshGroupIDList.Count; iMainMG++)
			{
				//ID(int) 복사
				targetOptPortrait._mainMeshGroupIDList.Add(srcPortrait._mainMeshGroupIDList[iMainMG]);
			}

			//4. 다른 정보들 복사
			targetOptPortrait._FPS = srcPortrait._FPS;

			targetOptPortrait._bakeScale = srcPortrait._bakeScale;
			targetOptPortrait._bakeZSize = srcPortrait._bakeZSize;

			targetOptPortrait._imageFilePath_Thumbnail = srcPortrait._imageFilePath_Thumbnail;

			targetOptPortrait._isImportant = srcPortrait._isImportant;
			targetOptPortrait._autoPlayAnimClipID = srcPortrait._autoPlayAnimClipID;

			targetOptPortrait._sortingLayerID = srcPortrait._sortingLayerID;
			targetOptPortrait._sortingOrder = srcPortrait._sortingOrder;

			targetOptPortrait._isUsingMecanim = srcPortrait._isUsingMecanim;
			targetOptPortrait._mecanimAnimClipResourcePath = srcPortrait._mecanimAnimClipResourcePath;

			targetOptPortrait._billboardType = srcPortrait._billboardType;
			targetOptPortrait._meshShadowCastingMode = srcPortrait._meshShadowCastingMode;
			targetOptPortrait._meshReceiveShadow = srcPortrait._meshReceiveShadow;

			//4-2. Material Set 복사
			targetOptPortrait._materialSets.Clear();
			for (int i = 0; i < srcPortrait._materialSets.Count; i++)
			{
				apMaterialSet srcMatSet = srcPortrait._materialSets[i];
				apMaterialSet copiedMatSet = new apMaterialSet();
				copiedMatSet.CopyFromSrc(srcMatSet, srcMatSet._uniqueID, false, false, srcMatSet._isDefault);
				targetOptPortrait._materialSets.Add(copiedMatSet);
			}


			//추가 10.26 : Bake에서는 빌보드가 꺼져야 한다.
			//임시로 껐다가 마지막에 다시 복구
			apPortrait.BILLBOARD_TYPE billboardType = targetOptPortrait._billboardType;
			targetOptPortrait._billboardType = apPortrait.BILLBOARD_TYPE.None;//임시로 끄자
			


			// 지금부터는 일반 Bake처럼 진행이 된다.
			// 1. Editor._portrait대신 targetOptPortrait를 사용한다.
			// 2. 데이터는 Mesh, MeshGroup, Modifier 정보는 srcPortrait 정보를 사용한다.


			//Bake 방식 변경
			//일단 숨겨진 GameObject를 제외한 모든 객체를 리스트로 저장한다.
			//LinkParam 형태로 저장을 한다.
			//LinkParam으로 저장하면서 <apOpt 객체>와 <그렇지 않은 객체>를 구분한다.
			//"apOpt 객체"는 나중에 (1)재활용 할지 (2) 삭제 할지 결정한다.
			//"그렇지 않은 GameObject"는 Hierarchy 정보를 가진채 (1) 링크를 유지할 지(재활용되는 경우) (2) Unlink Group에 넣을지 결정한다.
			//만약 재활용되지 않는 (apOpt GameObject)에서 알수 없는 Component가 발견된 경우 -> 이건 삭제 예외 대상에 넣는다.

			//분류를 위한 그룹
			//1. ReadyToRecycle
			// : 기존에 RootUnit과 그 하위에 있었던 GameObject들이다. 분류 전에 일단 여기로 들어간다.
			// : 분류 후에는 원칙적으로 하위에 어떤 객체도 남아선 안된다.

			//2. RemoveTargets
			// : apOpt를 가진 GameObject 그룹 중에서 사용되지 않았던 그룹이다. 
			// : 처리 후에는 이 GameObject를 통째로 삭제한다.

			//3. UnlinkedObjects
			// : apOpt를 가지지 않은 GameObject중에서 재활용되지 않은 객체들


			GameObject groupObj_1_ReadyToRecycle = new GameObject("__Baking_1_ReadyToRecycle");
			GameObject groupObj_2_RemoveTargets = new GameObject("__Baking_2_RemoveTargets");


			GameObject groupObj_3_UnlinkedObjects = null;
			if (targetOptPortrait._bakeUnlinkedGroup == null)
			{
				groupObj_3_UnlinkedObjects = new GameObject("__UnlinkedObjects");
				targetOptPortrait._bakeUnlinkedGroup = groupObj_3_UnlinkedObjects;
			}
			else
			{
				groupObj_3_UnlinkedObjects = targetOptPortrait._bakeUnlinkedGroup;
				groupObj_3_UnlinkedObjects.name = "__UnlinkedObjects";
			}




			groupObj_1_ReadyToRecycle.transform.parent = targetOptPortrait.transform;
			groupObj_2_RemoveTargets.transform.parent = targetOptPortrait.transform;
			groupObj_3_UnlinkedObjects.transform.parent = targetOptPortrait.transform;

			groupObj_1_ReadyToRecycle.transform.localPosition = Vector3.zero;
			groupObj_2_RemoveTargets.transform.localPosition = Vector3.zero;
			groupObj_3_UnlinkedObjects.transform.localPosition = Vector3.zero;

			groupObj_1_ReadyToRecycle.transform.localRotation = Quaternion.identity;
			groupObj_2_RemoveTargets.transform.localRotation = Quaternion.identity;
			groupObj_3_UnlinkedObjects.transform.localRotation = Quaternion.identity;

			groupObj_1_ReadyToRecycle.transform.localScale = Vector3.one;
			groupObj_2_RemoveTargets.transform.localScale = Vector3.one;
			groupObj_3_UnlinkedObjects.transform.localScale = Vector3.one;


			//2. 기존 RootUnit을 Recycle로 옮긴다.
			//옮기면서 "Prev List"를 만들어야 한다. Recycle을 하기 위함
			List<apOptRootUnit> prevOptRootUnits = new List<apOptRootUnit>();
			if (targetOptPortrait._optRootUnitList != null)
			{
				for (int i = 0; i < targetOptPortrait._optRootUnitList.Count; i++)
				{
					apOptRootUnit optRootUnit = targetOptPortrait._optRootUnitList[i];
					if (optRootUnit != null)
					{
						optRootUnit.transform.parent = groupObj_1_ReadyToRecycle.transform;

						prevOptRootUnits.Add(optRootUnit);
					}
				}
			}


			//RootUnit 리스트를 초기화한다.
			if (targetOptPortrait._optRootUnitList == null)
			{
				targetOptPortrait._optRootUnitList = new List<apOptRootUnit>();
			}

			targetOptPortrait._optRootUnitList.Clear();
			targetOptPortrait._curPlayingOptRootUnit = null;

			if (targetOptPortrait._optTransforms == null) { targetOptPortrait._optTransforms = new List<apOptTransform>(); }
			if (targetOptPortrait._optMeshes == null) { targetOptPortrait._optMeshes = new List<apOptMesh>(); }
			if (targetOptPortrait._optTextureData == null) { targetOptPortrait._optTextureData = new List<apOptTextureData>(); }//<<텍스쳐 데이터 추가

			targetOptPortrait._optTransforms.Clear();
			targetOptPortrait._optMeshes.Clear();
			targetOptPortrait._optTextureData.Clear();

			//추가
			//Batched Matrial 관리 객체가 생겼다.
			if (targetOptPortrait._optBatchedMaterial == null)
			{
				targetOptPortrait._optBatchedMaterial = new apOptBatchedMaterial();
			}
			else
			{
				targetOptPortrait._optBatchedMaterial.Clear(true);//<<이미 생성되어 있다면 초기화
			}

			////추가 11.6 : LWRP Shader를 사용하는지 체크하고, 필요한 경우 생성해야한다.
			//CheckAndCreateLWRPShader();


			// srcPortrait로 부터 가져온 데이터는 앞에 src를 붙인다.

			//3. 텍스쳐 데이터를 먼저 만들자.
			// Src -> Target
			for (int i = 0; i < srcPortrait._textureData.Count; i++)
			{
				apTextureData srcTextureData = srcPortrait._textureData[i];
				apOptTextureData newOptTexData = new apOptTextureData();

				newOptTexData.Bake(i, srcTextureData);
				targetOptPortrait._optTextureData.Add(newOptTexData);
			}

			//4. 추가 : Reset
			srcPortrait.LinkAndRefreshInEditor(false, null); // Source를 먼저 준비

			//4. OptTransform을 만들자 (RootUnit부터)
			// Src -> Taret
			for (int i = 0; i < srcPortrait._rootUnits.Count; i++)
			{
				apRootUnit srcRootUnit = srcPortrait._rootUnits[i];

				//추가 : 계층구조의 MeshGroup인 경우 이 코드가 추가되어야 한다.
				if (srcRootUnit._childMeshGroup != null)
				{
					srcRootUnit._childMeshGroup.SortRenderUnits(true);//렌더 유닛의 Depth를 다시 계산해야한다. <<
					srcRootUnit._childMeshGroup.LinkModMeshRenderUnits();
					srcRootUnit._childMeshGroup.RefreshModifierLink();
				}

				//업데이트를 한번 해주자
				srcRootUnit.Update(0.0f, false, false);

				apOptRootUnit optRootUnit = null;

				//1. Root Unit
				//재활용 가능한지 판단한다.
				bool isRecycledRootUnit = false;
				apOptRootUnit recycledOptRootUnit = GetRecycledRootUnit(srcRootUnit, prevOptRootUnits);

				if (recycledOptRootUnit != null)
				{

					//재활용이 된다.
					optRootUnit = recycledOptRootUnit;

					//일부 값은 다시 리셋
					optRootUnit.name = "Root Portrait " + i;
					optRootUnit._portrait = targetOptPortrait;
					optRootUnit._transform = optRootUnit.transform;

					optRootUnit.transform.parent = targetOptPortrait.transform;
					optRootUnit.transform.localPosition = Vector3.zero;
					optRootUnit.transform.localRotation = Quaternion.identity;
					optRootUnit.transform.localScale = Vector3.one;

					//재활용에 성공했으니 OptUnit은 제외한다.
					prevOptRootUnits.Remove(recycledOptRootUnit);
					isRecycledRootUnit = true;

					//Count+1 : Recycled Opt
					bakeResult.AddCount_RecycledOptGameObject();
				}
				else
				{
					//새로운 RootUnit이다.
					optRootUnit = AddGameObject<apOptRootUnit>("Root Portrait " + i, targetOptPortrait.transform);

					optRootUnit._portrait = targetOptPortrait;
					optRootUnit._rootOptTransform = null;
					optRootUnit._transform = optRootUnit.transform;

					//Count+1 : New Opt
					bakeResult.AddCount_NewOptGameObject();
				}

				optRootUnit.ClearChildLinks();//Child Link를 초기화한다.

				//추가 12.6 : SortedRenderBuffer에 관련한 Bake 코드 <<
				optRootUnit.BakeSortedRenderBuffer(targetOptPortrait, srcRootUnit);

				targetOptPortrait._optRootUnitList.Add(optRootUnit);



				//재활용에 성공했다면
				//기존의 GameObject + Bake 여부를 재귀적 리스트로 작성한다.
				apBakeLinkManager bakeLinkManager = null;
				if (isRecycledRootUnit)
				{
					bakeLinkManager = new apBakeLinkManager();

					//파싱하자.
					bakeLinkManager.Parse(optRootUnit._rootOptTransform.gameObject, recycledOptRootUnit.gameObject);
				}

				apMeshGroup srcChildMainMeshGroup = srcRootUnit._childMeshGroup;

				//0. 추가
				//일부 Modified Mesh를 갱신해야한다.
				if (srcChildMainMeshGroup != null && srcRootUnit._childMeshGroupTransform != null)
				{
					//Refresh를 한번 해주자
					srcChildMainMeshGroup.RefreshForce();

					List<apModifierBase> srcModifiers = srcChildMainMeshGroup._modifierStack._modifiers;
					for (int iMod = 0; iMod < srcModifiers.Count; iMod++)
					{
						apModifierBase mod = srcModifiers[iMod];
						if (mod._paramSetGroup_controller != null)
						{
							for (int iPSG = 0; iPSG < mod._paramSetGroup_controller.Count; iPSG++)
							{
								apModifierParamSetGroup psg = mod._paramSetGroup_controller[iPSG];
								for (int iPS = 0; iPS < psg._paramSetList.Count; iPS++)
								{
									apModifierParamSet ps = psg._paramSetList[iPS];
									ps.UpdateBeforeBake(srcPortrait, srcChildMainMeshGroup, srcRootUnit._childMeshGroupTransform);
								}
							}
						}
					}
				}

				//1. 1차 Bake : GameObject 만들기
				//List<apMeshGroup> meshGroups = Editor._portrait._meshGroups;
				if (srcChildMainMeshGroup != null && srcRootUnit._childMeshGroupTransform != null)
				{
					//정렬 한번 해주고
					srcChildMainMeshGroup.SortRenderUnits(true);

					apRenderUnit srcRootRenderUnit = srcChildMainMeshGroup._rootRenderUnit;
					//apRenderUnit rootRenderUnit = Editor._portrait._rootUnit._renderUnit;
					if (srcRootRenderUnit != null)
					{
						//apTransform_MeshGroup meshGroupTransform = Editor._portrait._rootUnit._childMeshGroupTransform;
						apTransform_MeshGroup srcMeshGroupTransform = srcRootRenderUnit._meshGroupTransform;

						if (srcMeshGroupTransform == null)
						{
							Debug.LogError("Bake Error : MeshGroupTransform Not Found [" + srcChildMainMeshGroup._name + "]");
						}
						else
						{
							MakeMeshGroupToOptTransform(srcRootRenderUnit,
															srcMeshGroupTransform,
															optRootUnit.transform,
															null,
															optRootUnit,
															bakeLinkManager,
															bakeResult,
															targetOptPortrait._bakeZSize,
															Editor._isBakeColorSpaceToGamma,//<<감마 색상 공간으로 Bake할 것인가
															Editor._isUseSRP,//LWRP Shader를 사용할 것인가
															targetOptPortrait,
															srcChildMainMeshGroup,
															isSizeOptimizedV117,
															isUseSRP);
							//MakeMeshGroupToOptTransform(null, meshGroupTransform, Editor._portrait._optRootUnit.transform, null);
						}
					}
					else
					{
						Debug.LogError("Bake Error : RootMeshGroup Not Found [" + srcChildMainMeshGroup._name + "]");
					}
				}



				//optRootUnit.transform.localScale = Vector3.one * 0.01f;
				optRootUnit.transform.localScale = Vector3.one * targetOptPortrait._bakeScale;


				// 이전에 Bake 했던 정보에서 가져왔다면
				//만약 "재활용되지 않은 GameObject"를 찾아서 별도의 처리를 해야한다.
				if (isRecycledRootUnit && bakeLinkManager != null)
				{
					bakeLinkManager.SetHierarchyNotRecycledObjects(groupObj_1_ReadyToRecycle, groupObj_2_RemoveTargets, groupObj_3_UnlinkedObjects, bakeResult);

				}


				//추가 12.6 : Bake 함수 추가 <<
				optRootUnit.BakeComplete();


			}


			if (prevOptRootUnits.Count > 0)
			{
				//TODO : 여기서부터 작성하자
				//이 유닛들은 Remove Target으로 이동해야 한다.

				for (int i = 0; i < prevOptRootUnits.Count; i++)
				{
					prevOptRootUnits[i].transform.parent = groupObj_2_RemoveTargets.transform;

					//만약 여기서 알수없는 GameObject나 Compnent에 대해서는 Remove가 아니라 Unlink로 옮겨야 한다.
					apBakeLinkManager prevBakeManager = new apBakeLinkManager();
					prevBakeManager.Parse(prevOptRootUnits[i]._rootOptTransform.gameObject, null);

					prevBakeManager.SetHierarchyToUnlink(groupObj_3_UnlinkedObjects, bakeResult);

				}
			}


			//TODO: 이제 그룹을 삭제하던가 경고 다이얼로그를 띄워주던가 하자
			UnityEngine.Object.DestroyImmediate(groupObj_1_ReadyToRecycle);
			UnityEngine.Object.DestroyImmediate(groupObj_2_RemoveTargets);

			if (groupObj_3_UnlinkedObjects.transform.childCount == 0)
			{
				UnityEngine.Object.DestroyImmediate(groupObj_3_UnlinkedObjects);

				targetOptPortrait._bakeUnlinkedGroup = null;
			}


			for (int i = 0; i < targetOptPortrait._optMeshes.Count; i++)
			{
				apOptMesh optMesh = targetOptPortrait._optMeshes[i];
				if (optMesh._isMaskChild)
				{
					apOptTransform optTransform = targetOptPortrait.GetOptTransform(optMesh._clipParentID);
					apOptMesh parentMesh = null;
					if (optTransform != null && optTransform._childMesh != null)
					{
						parentMesh = optTransform._childMesh;
					}
					optMesh.LinkAsMaskChild(parentMesh);
				}
			}

			//2. 2차 Bake : Modifier 만들기
			List<apOptTransform> optTransforms = targetOptPortrait._optTransforms;
			for (int i = 0; i < optTransforms.Count; i++)
			{
				apOptTransform optTransform = optTransforms[i];

				apMeshGroup srcMeshGroup = srcPortrait.GetMeshGroup(optTransform._meshGroupUniqueID);
				optTransform.BakeModifier(targetOptPortrait, srcMeshGroup, isSizeOptimizedV117);
			}


			//3. 3차 Bake : ControlParam/KeyFrame ~~> Modifier <- [Calculated Param] -> OptTrasform + Mesh
			targetOptPortrait.SetFirstInitializeAfterBake();
			targetOptPortrait.Initialize();



			//4. 첫번째 OptRoot만 보여주도록 하자
			if (targetOptPortrait._optRootUnitList.Count > 0)
			{
				targetOptPortrait.ShowRootUnitWhenBake(targetOptPortrait._optRootUnitList[0]);
			}


			//5. AnimClip의 데이터를 받아서 AnimPlay 데이터로 만들자
			if (targetOptPortrait._animPlayManager == null)
			{
				targetOptPortrait._animPlayManager = new apAnimPlayManager();
			}

			targetOptPortrait._animPlayManager.InitAndLink();
			targetOptPortrait._animPlayManager._animPlayDataList.Clear();

			for (int i = 0; i < targetOptPortrait._animClips.Count; i++)
			{
				apAnimClip animClip = targetOptPortrait._animClips[i];
				int animClipID = animClip._uniqueID;
				string animClipName = animClip._name;
				int targetMeshGroupID = animClip._targetMeshGroupID;

				apAnimPlayData animPlayData = new apAnimPlayData(animClipID, targetMeshGroupID, animClipName);
				targetOptPortrait._animPlayManager._animPlayDataList.Add(animPlayData);

			}


			//6. 한번 업데이트를 하자 (소켓들이 갱신된다)
			if (targetOptPortrait._optRootUnitList.Count > 0)
			{
				apOptRootUnit optRootUnit = null;
				for (int i = 0; i < targetOptPortrait._optRootUnitList.Count; i++)
				{
					//이전
					//taretOptPortrait._optRootUnitList[i].RemoveAllCalculateResultParams();

					//변경
					optRootUnit = targetOptPortrait._optRootUnitList[i];
					if (optRootUnit._rootOptTransform != null)
					{
						optRootUnit._rootOptTransform.ClearResultParams(true);
						optRootUnit._rootOptTransform.ResetCalculateStackForBake(true);
					}
					else
					{
						Debug.LogError("AnyPortrait : No Root Opt Transform on RootUnit (OptBake)");
					}
				}

				//추가 3.22 : Bake후 메시가 변경되었을 경우에 다시 리셋할 필요가 있다.
				//for (int i = 0; i < taretOptPortrait._optRootUnitList.Count; i++)
				//{
				//	taretOptPortrait._optRootUnitList[i].ResetCalculateStackForBake();
				//}

				for (int i = 0; i < targetOptPortrait._optRootUnitList.Count; i++)
				{
					targetOptPortrait._optRootUnitList[i].UpdateTransforms(0.0f);
				}
			}
			//taretOptPortrait.ResetMeshesCommandBuffers(false);

			//taretOptPortrait.UpdateForce();

			// 원래는 "사용하지 않는 Mesh, MeshGroup 등을 삭제하는 코드"가 있는데,
			// Opt에서는 필요가 없다.
			//추가 3.22 
			//6-2. LayerOrder 갱신하자
			string sortingLayerName = "";
			bool isValidSortingLayer = false;
			if (SortingLayer.IsValid(Editor._portrait._sortingLayerID))
			{
				sortingLayerName = SortingLayer.IDToName(Editor._portrait._sortingLayerID);
				isValidSortingLayer = true;
			}
			else
			{
				if (SortingLayer.layers.Length > 0)
				{
					sortingLayerName = SortingLayer.layers[0].name;
					isValidSortingLayer = true;
				}
				else
				{
					isValidSortingLayer = false;
				}
			}
			if (isValidSortingLayer)
			{
				targetOptPortrait.SetSortingLayer(sortingLayerName);
			}
			//변경 19.8.19 : 옵션이 적용되는 경우에 한해서
			if (Editor._portrait._sortingOrderOption == apPortrait.SORTING_ORDER_OPTION.SetOrder)
			{
				targetOptPortrait.SetSortingOrder(Editor._portrait._sortingOrder);
			}


			//추가 19.5.26
			//6-3. 최적화 옵션으로 Bake 되었는지 체크
			targetOptPortrait._isSizeOptimizedV117 = isSizeOptimizedV117;



			//추가3.22
			//Portrait가 Prefab이라면
			//Bake와 동시에 Apply를 해야한다.
			//if(apEditorUtil.IsPrefab(taretOptPortrait.gameObject))
			//{
			//	apEditorUtil.ApplyPrefab(taretOptPortrait.gameObject);
			//}

			//추가 4.26
			//메카님 옵션이 켜져 있다면
			//1. Animation Clip들을 리소스로 생성한다.
			//2. Animator 컴포넌트를 추가한다.
			//TODO : > Optimized에서도
			if (targetOptPortrait._isUsingMecanim)
			{
				//추가 3.22 : animClip 경로가 절대 경로인 경우, 여러 작업자가 공유해서 쓸 수 없다.
				//상대 경로로 바꾸는 작업을 해야한다.
				CheckAnimationsBasePathForV116(targetOptPortrait);

				CreateAnimationsWithMecanim(targetOptPortrait, targetOptPortrait._mecanimAnimClipResourcePath);
				targetOptPortrait.Initialize();
			}

			apEditorUtil.SetEditorDirty();


			//추가. Bake 후 처리
			ProcessAfterBake();

			//추가 19.10.26 : 빌보드 설정을 다시 복구
			targetOptPortrait._billboardType = billboardType;

			return bakeResult;
		}


		/// <summary>
		/// Bake / OptimizedBake 이후에 호출해야하는 함수.
		/// 현재 편집되는 것에 따라서 Link를 다시 해야한다.
		/// </summary>
		private void ProcessAfterBake()
		{
			apPortrait portrait = Editor.Select.Portrait;
			if (portrait == null)
			{
				return;
			}
			apMeshGroup meshGroup = null;
			switch (Editor.Select.SelectionType)
			{
				case apSelection.SELECTION_TYPE.Overall:
					if (Editor.Select.RootUnit != null)
					{
						meshGroup = Editor.Select.RootUnit._childMeshGroup;
					}
					break;


				case apSelection.SELECTION_TYPE.MeshGroup:
					meshGroup = Editor.Select.MeshGroup;
					break;

				case apSelection.SELECTION_TYPE.Animation:
					if (Editor.Select.AnimClip != null)
					{
						meshGroup = Editor.Select.AnimClip._targetMeshGroup;
					}

					break;
			}
			if (meshGroup != null)
			{
				//현재 작업 중인 MeshGroup을 찾아서 Link를 다시 한다.
				portrait.LinkAndRefreshInEditor(false, meshGroup);
			}

		}
		//----------------------------------------------------------------------------------
		// GUI - Input
		//----------------------------------------------------------------------------------
		//------------------------------------------------------------------------------
		public int GUI_Controller_Upper(int width)
		{
			if (Editor == null)
			{
				return 0;
			}
			if (Editor._portrait == null)
			{
				return 0;
			}
			EditorGUILayout.LabelField(Editor.GetUIWord(UIWORD.Category));//"Category"

#if UNITY_2017_3_OR_NEWER
			Editor._curParamCategory = (apControlParam.CATEGORY)EditorGUILayout.EnumFlagsField(apGUIContentWrapper.Empty.Content, Editor._curParamCategory, GUILayout.Width(width));
#else
			Editor._curParamCategory = (apControlParam.CATEGORY)EditorGUILayout.EnumMaskPopup(apGUIContentWrapper.Empty.Content, Editor._curParamCategory, GUILayout.Width(width));
#endif


			GUILayout.Space(5);
			//56 - 15 = 41

			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(25));
			GUILayout.Space(4);
			//리셋 기능은 뺀다.

			if (GUILayout.Button(Editor.GetUIWord(UIWORD.SetDefaultAll)))//"Set Default All"
			{
				//bool isResult = EditorUtility.DisplayDialog("Reset", "Really Set Default Value?", "Set Default All", "Cancel");
				bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.ControlParamDefaultAll_Title),
																Editor.GetText(TEXT.ControlParamDefaultAll_Body),
																Editor.GetText(TEXT.ControlParamDefaultAll_Okay),
																Editor.GetText(TEXT.Cancel));

				if (isResult)
				{
					List<apControlParam> cParams = Editor.ParamControl._controlParams;

					for (int i = 0; i < cParams.Count; i++)
					{
						cParams[i].SetDefault();
					}
				}
				//Editor.Hierarchy.RefreshUnits();
				Editor.RefreshControllerAndHierarchy(false);
			}
			GUILayout.Space(4);
			EditorGUILayout.EndHorizontal();
			//56 + 60 => 41 + 25 = 
			//return 56 + 60;
			return 41 + 25;
		}

		public void SetDefaultAllControlParams()
		{
			if (Editor == null || Editor._portrait == null)
			{
				return;
			}

			List<apControlParam> cParams = Editor.ParamControl._controlParams;

			for (int i = 0; i < cParams.Count; i++)
			{
				cParams[i].SetDefault();
			}
		}



		public void GUI_Controller(int width, int height, int scrollY)
		{
			if (Editor == null)
			{
				return;
			}
			if (Editor._portrait == null)
			{
				return;
			}

			bool isRecording = false;
			List<apModifierParamSet> modParamSetList = null;
			apModifierParamSetGroup modParamSetGroup = null;

			//ControlParam 타입의 Timeline을 작업중인가.
			bool isAnimEditing = false;
			apAnimTimeline animTimeline = null;
			apAnimTimelineLayer animTimelineLayer = null;
			apAnimKeyframe animKeyframe = null;

			//TODO : 현재 레코딩 중인지 체크
			if (Editor.Select.SelectionType == apSelection.SELECTION_TYPE.MeshGroup)
			{
				if (Editor.Select.Modifier != null)
				{
					//[중요] 모디파이어중에서 ControlParam의 영향을 받을 경우 여기서 추가해서 키를 추가할 수 있게 세팅해야 한다.
					switch (Editor.Select.Modifier.SyncTarget)
					{
						case apModifierParamSetGroup.SYNC_TARGET.Controller:
							isRecording = true;
							modParamSetGroup = Editor.Select.SubEditedParamSetGroup;
							//modParamSetList = Editor.Select.Modifier._paramSetList;
							if (modParamSetGroup != null)
							{
								modParamSetList = modParamSetGroup._paramSetList;
							}

							break;
					}
				}
			}
			else if (Editor.Select.SelectionType == apSelection.SELECTION_TYPE.Animation)
			{
				//추가 : Animation 상황에서도 레코딩이 가능하다. 단, isRecording(키 생성 방식)은 아니고
				//그 자체가 Keyframe의 값으로 치환되어야 한다.
				if (Editor.Select.AnimClip != null && Editor.Select.ExAnimEditingMode != apSelection.EX_EDIT.None)
				{
					if (Editor.Select.AnimTimeline != null && Editor.Select.AnimTimeline._linkType == apAnimClip.LINK_TYPE.ControlParam)
					{
						isAnimEditing = true;
						animTimeline = Editor.Select.AnimTimeline;
						animTimelineLayer = Editor.Select.AnimTimelineLayer;
						if (animTimelineLayer != null)
						{
							//기존 : 선택한 키프레임
							//if (Editor.Select.AnimKeyframes.Count == 1 && Editor.Select.AnimKeyframe != null)
							//{
							//	animKeyframe = Editor.Select.AnimKeyframe;
							//}

							//변경 : 현재의 키프레임
							animKeyframe = Editor.Select.AnimWorkKeyframe;
						}
					}
				}
			}



			List<apControlParam> cParams = Editor.ParamControl._controlParams;
			for (int i = 0; i < cParams.Count; i++)
			{
				if ((byte)(cParams[i]._category & Editor._curParamCategory) != 0)
				{
					GUI_ControlParam(cParams[i], width,
										isRecording, modParamSetList, modParamSetGroup,
										isAnimEditing, animTimelineLayer, animKeyframe,
										height, scrollY);
					GUILayout.Space(10);
				}
			}
		}


		private void GUI_ControlParam(apControlParam controlParam, int width,
										bool isRecording, List<apModifierParamSet> modParamSetList, apModifierParamSetGroup curParamSetGroup,
										bool isAnimEditing, apAnimTimelineLayer animTimelineLayer, apAnimKeyframe animKeyframe,
										int windowHeight, int windowScrollY)
		{
			width -= 10;


			int labelWidth = width - (50 + 4);
			if (isRecording)
			{
				labelWidth = width - (75 + 7);
			}

			int recordBtnSize = 25;
			int presetIconSize = 32;
			EditorGUILayout.BeginHorizontal(GUILayout.Width(width), GUILayout.Height(presetIconSize));
			GUILayout.Box(Editor.ImageSet.Get(apEditorUtil.GetControlParamPresetIconType(controlParam._iconPreset)), GUI.skin.label, GUILayout.Width(presetIconSize), GUILayout.Height(presetIconSize));

			EditorGUILayout.LabelField(controlParam._keyName, GUILayout.Width(labelWidth - (presetIconSize + 4)), GUILayout.Height(presetIconSize));

			apModifierParamSet recordedKey = null;
			apModifierParamSet prevRecordedKey = Editor.Select.ParamSetOfMod;

			bool isCurSelected = false;
			if (curParamSetGroup != null)
			{
				if (curParamSetGroup._keyControlParam == controlParam)
				{
					isCurSelected = true;
				}
			}
			else if (isAnimEditing && animTimelineLayer != null)
			{
				if (animTimelineLayer._linkedControlParam == controlParam)
				{
					isCurSelected = true;
				}
			}



			List<apModifierParamSet> recordKeyParamSet = null;
			if (isRecording &&
				//controlParam._isRange && 
				(controlParam._valueType == apControlParam.TYPE.Int ||
				controlParam._valueType == apControlParam.TYPE.Float ||
				controlParam._valueType == apControlParam.TYPE.Vector2
				//controlParam._valueType == apControlParam.TYPE.Vector3
				))
			{
				recordKeyParamSet = new List<apModifierParamSet>();
				bool isRecordKey = false;

				if (curParamSetGroup != null && modParamSetList != null)
				{
					//TODO : 현재 레코딩 키 위에 컨트롤러가 있는지 체크
					for (int i = 0; i < modParamSetList.Count; i++)
					{
						apModifierParamSet modParamSet = modParamSetList[i];
						if (curParamSetGroup._syncTarget != apModifierParamSetGroup.SYNC_TARGET.Controller)
						{
							continue;
						}
						if (curParamSetGroup._keyControlParam != controlParam)
						{
							continue;
						}



						recordKeyParamSet.Add(modParamSet);

						//현재 레코드 키 위에 있는지 체크하자
						float biasX = 0.0f;
						float biasY = 0.0f;

						if (recordedKey == null)
						{
							switch (controlParam._valueType)
							{
								case apControlParam.TYPE.Int:
									{
										if (controlParam._int_Cur == modParamSet._conSyncValue_Int)
										{
											isRecordKey = true;
										}
									}
									break;

								case apControlParam.TYPE.Float:
									{
										biasX = (Mathf.Abs(controlParam._float_Max - controlParam._float_Min) / (float)controlParam._snapSize) * 0.2f;

										if (controlParam._float_Cur > modParamSet._conSyncValue_Float - biasX &&
											controlParam._float_Cur < modParamSet._conSyncValue_Float + biasX
											)
										{
											isRecordKey = true;
										}
									}
									break;

								case apControlParam.TYPE.Vector2:
									{
										//biasX = Mathf.Abs(controlParam._vec2_Max.x - controlParam._vec2_Min.x) * 0.05f;
										//biasY = Mathf.Abs(controlParam._vec2_Max.y - controlParam._vec2_Min.y) * 0.05f;

										//if (biasX > 0.05f)
										//{ biasX = 0.05f; }
										//if (biasY > 0.05f)
										//{ biasY = 0.05f; }

										//bias는 기본 Snap 크기의 절반이다.
										biasX = (Mathf.Abs(controlParam._vec2_Max.x - controlParam._vec2_Min.x) / (float)controlParam._snapSize) * 0.2f;
										biasY = (Mathf.Abs(controlParam._vec2_Max.y - controlParam._vec2_Min.y) / (float)controlParam._snapSize) * 0.2f;

										if (controlParam._vec2_Cur.x > modParamSet._conSyncValue_Vector2.x - biasX &&
											controlParam._vec2_Cur.x < modParamSet._conSyncValue_Vector2.x + biasX &&
											controlParam._vec2_Cur.y > modParamSet._conSyncValue_Vector2.y - biasY &&
											controlParam._vec2_Cur.y < modParamSet._conSyncValue_Vector2.y + biasY
											)
										{
											isRecordKey = true;
										}
									}
									break;



							}
						}

						if (isRecordKey && recordedKey == null)
						{
							recordedKey = modParamSet;
						}
					}
				}


				if (Editor.Select.SubEditedParamSetGroup != null)
				{
					if (prevRecordedKey != recordedKey)
					{
						if (recordedKey != null)
						{
							//자동으로 선택해주자
							if (Editor.Select.SubEditedParamSetGroup._keyControlParam == controlParam &&
									Editor.Select.SubEditedParamSetGroup._paramSetList.Contains(recordedKey))
							{
								//만약
								//현재 Modifier에서 Record 키 작업중이라면
								//현재 ParamSet을 Select에 지정하는 것도 좋겠다.
								Editor.Select.SetParamSetOfModifier(recordedKey);
							}
						}
						else
						{
							if (Editor.Select.SubEditedParamSetGroup._keyControlParam == controlParam)
							{
								//apSelection.EX_EDIT prevExMode = Editor.Select.ExEditingMode;
								Editor.Select.SetParamSetOfModifier(null, true);//<<변경 3.31 : ExEdit 모드가 해제되지 않도록 만든다.
								Editor.Hierarchy_MeshGroup.RefreshUnits();

								//apSelection.EX_EDIT nextExMode = Editor.Select.ExEditingMode;

								//if(nextExMode != prevExMode)
								//{
								//	Debug.LogError("테스트 : Control Param시 Ex모드가 변경 [" + prevExMode + " >> " + nextExMode + "]");
								//}
							}
						}
					}
				}


				if (isRecordKey)
				{
					//선택된 RecordKey가 있다.
					if (GUILayout.Button(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Controller_RemoveRecordKey), "Remove Key"), GUILayout.Width(recordBtnSize), GUILayout.Height(recordBtnSize)))
					{
						//bool isResult = EditorUtility.DisplayDialog("Remove Record Key", "Remove Record Key?", "Remove", "Cancel");
						bool isResult = EditorUtility.DisplayDialog(Editor.GetText(TEXT.RemoveRecordKey_Title),
																		Editor.GetText(TEXT.RemoveRecordKey_Body),
																		Editor.GetText(TEXT.Remove),
																		Editor.GetText(TEXT.Cancel));
						if (isResult && recordedKey != null)
						{
							RemoveRecordKey(recordedKey, recordKeyParamSet);
						}
					}
				}
				else
				{
					//선택된 RecordKey가 없다.
					//단, 이걸 출력하려면 -> 
					if (GUILayout.Button(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Controller_MakeRecordKey), "Make Key"), GUILayout.Width(recordBtnSize), GUILayout.Height(recordBtnSize)))
					{
						AddControlParamToModifier(controlParam);

						//추가 : ExEdit 모드가 아니라면, Modifier에 추가할 때 자동으로 ExEdit 상태로 전환
						if (Editor.Select.ExEditingMode == apSelection.EX_EDIT.None && Editor.Select.IsExEditable)
						{
							Editor.Select.SetModifierExclusiveEditing(apSelection.EX_EDIT.ExOnly_Edit);

							//변경 : 원래 SelectionLock을 설정하는 것에서, 따로 지정하지 않는 걸로 변경
							//Editor.Select.SetModifierExclusiveEditKeyLock(true);//처음 Editing 작업시 Lock을 거는 것으로 변경
						}
					}
				}
			}

			bool isRepaint = false;

			//Set Default 버튼
			if (GUILayout.Button(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Controller_Default), "Set Default"), GUILayout.Width(recordBtnSize), GUILayout.Height(recordBtnSize)))
			{
				controlParam.SetDefault();
				//Editor.Repaint();
				isRepaint = true;
				//Editor.SetRepaint();
			}

			//Edit Parameter 버튼
			if (GUILayout.Button(new GUIContent(Editor.ImageSet.Get(apImageSet.PRESET.Controller_Edit), "Edit Parameter"), GUILayout.Width(recordBtnSize), GUILayout.Height(recordBtnSize)))
			{
				Editor.Select.SetParam(controlParam);
				//Editor.Repaint();
				Editor.SetLeftTab(apEditor.TAB_LEFT.Hierarchy);
				Editor.RefreshControllerAndHierarchy(false);

				isRepaint = true;
				//Editor.SetRepaint();

			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);
			GUIStyle guiStyle_RightLabel = new GUIStyle(GUI.skin.label);
			guiStyle_RightLabel.alignment = TextAnchor.UpperRight;

			int unitHeight = 24;
			int guiHeight = 0;
			switch (controlParam._valueType)
			{
				case apControlParam.TYPE.Int:
				case apControlParam.TYPE.Float:
					{
						guiHeight += unitHeight * 3;
					}
					break;

				case apControlParam.TYPE.Vector2:
					{
						guiHeight += unitHeight * 6;
					}
					break;
			}


			Rect lastRect = GUILayoutUtility.GetLastRect();
			Color prevColor = GUI.backgroundColor;

			if ((lastRect.y - windowScrollY) + guiHeight > -10 && lastRect.y - windowScrollY < windowHeight)
			{
				//영역 안에 있을때만 배경을 칠하자
				if (EditorGUIUtility.isProSkin)
				{
					if (isCurSelected)
					{
						GUI.backgroundColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
					}
					else
					{
						GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
					}
				}
				else
				{
					if (isCurSelected)
					{
						GUI.backgroundColor = new Color(0.9f, 0.7f, 0.7f, 1.0f);
					}
					else
					{
						GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
					}
				}


				GUI.Box(new Rect(lastRect.x, lastRect.y, width + 10, guiHeight), "");
				GUI.backgroundColor = prevColor;
			}



			Vector2 guiPos = new Vector2(lastRect.x, lastRect.y + 25);

			switch (controlParam._valueType)
			{
				//case apControlParam.TYPE.Bool:
				//	{
				//		bool boolNext = EditorGUILayout.Toggle(controlParam._bool_Cur);
				//		if(controlParam._bool_Cur != boolNext)
				//		{
				//			controlParam._bool_Cur = boolNext;
				//			isRepaint = true;
				//		}
				//	}
				//	break;

				case apControlParam.TYPE.Int:
					{
						int intNext = controlParam._int_Cur;
						//if(controlParam._isRange)
						//{	

						//1차로 체크
						intNext = EditorGUILayout.DelayedIntField(controlParam._int_Cur);
						if (intNext != controlParam._int_Cur)
						{
							controlParam._int_Cur = intNext;
							isRepaint = true;
						}

						EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
						EditorGUILayout.LabelField(controlParam._label_Min, GUILayout.Width(width / 2 - 5));
						EditorGUILayout.LabelField(controlParam._label_Max, guiStyle_RightLabel, GUILayout.Width(width / 2 - 5));
						EditorGUILayout.EndHorizontal();

						//intNext = EditorGUILayout.IntSlider(controlParam._int_Cur, controlParam._int_Min, controlParam._int_Max);
						intNext = apControllerGL.DrawIntSlider(guiPos + new Vector2(0, unitHeight - 3), width, controlParam, isRecording, recordKeyParamSet, recordedKey);
						intNext = Mathf.Clamp(intNext, controlParam._int_Min, controlParam._int_Max);

						GUILayout.Space(unitHeight);

						//}
						//else
						//{
						//	intNext = EditorGUILayout.IntField(controlParam._int_Cur);
						//}

						if (intNext != controlParam._int_Cur)
						{
							controlParam._int_Cur = intNext;
							isRepaint = true;
						}
					}
					break;

				case apControlParam.TYPE.Float:
					{
						float floatNext = controlParam._float_Cur;
						//if(controlParam._isRange)
						//{
						//1차로 체크
						floatNext = EditorGUILayout.DelayedFloatField(controlParam._float_Cur);
						floatNext = Mathf.Clamp(floatNext, controlParam._float_Min, controlParam._float_Max);
						if (floatNext != controlParam._float_Cur)
						{
							controlParam._float_Cur = floatNext;
							isRepaint = true;
						}


						EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
						EditorGUILayout.LabelField(controlParam._label_Min, GUILayout.Width(width / 2 - 5));
						EditorGUILayout.LabelField(controlParam._label_Max, guiStyle_RightLabel, GUILayout.Width(width / 2 - 5));
						EditorGUILayout.EndHorizontal();

						//floatNext = EditorGUILayout.Slider(controlParam._float_Cur, controlParam._float_Min, controlParam._float_Max);

						floatNext = apControllerGL.DrawFloatSlider(guiPos + new Vector2(0, unitHeight - 3), width, controlParam, isRecording, recordKeyParamSet, recordedKey);

						GUILayout.Space(unitHeight);

						floatNext = Mathf.Clamp(floatNext, controlParam._float_Min, controlParam._float_Max);
						//}
						//else
						//{
						//	floatNext = EditorGUILayout.FloatField(controlParam._float_Cur);

						//}

						if (floatNext != controlParam._float_Cur)
						{
							controlParam._float_Cur = floatNext;
							isRepaint = true;
						}
					}
					break;

				case apControlParam.TYPE.Vector2:
					{
						Vector2 vec2Next = controlParam._vec2_Cur;
						//if(controlParam._isRange)
						//{

						EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
						vec2Next.x = EditorGUILayout.DelayedFloatField(vec2Next.x, GUILayout.Width(width / 2 - 5));
						vec2Next.y = EditorGUILayout.DelayedFloatField(vec2Next.y, GUILayout.Width(width / 2 - 5));

						vec2Next.x = Mathf.Clamp(vec2Next.x, controlParam._vec2_Min.x, controlParam._vec2_Max.x);
						vec2Next.y = Mathf.Clamp(vec2Next.y, controlParam._vec2_Min.y, controlParam._vec2_Max.y);



						//여기서 1차로 한번 검사
						if (vec2Next.x != controlParam._vec2_Cur.x || vec2Next.y != controlParam._vec2_Cur.y)
						{
							controlParam._vec2_Cur = vec2Next;
							isRepaint = true;
						}
						EditorGUILayout.EndHorizontal();


						EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
						EditorGUILayout.LabelField(controlParam._label_Max, GUILayout.Width(width));
						EditorGUILayout.EndHorizontal();

						//2차로 쉽게 제어
						vec2Next = apControllerGL.DrawVector2Slider(guiPos + new Vector2(0, unitHeight - 3), width, unitHeight * 3, controlParam, isRecording, recordKeyParamSet, recordedKey);

						vec2Next.x = Mathf.Clamp(vec2Next.x, controlParam._vec2_Min.x, controlParam._vec2_Max.x);
						vec2Next.y = Mathf.Clamp(vec2Next.y, controlParam._vec2_Min.y, controlParam._vec2_Max.y);

						GUILayout.Space(unitHeight * 3);
						EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
						EditorGUILayout.LabelField(controlParam._label_Min, guiStyle_RightLabel, GUILayout.Width(width));
						EditorGUILayout.EndHorizontal();


						if (vec2Next.x != controlParam._vec2_Cur.x || vec2Next.y != controlParam._vec2_Cur.y)
						{
							controlParam._vec2_Cur = vec2Next;
							isRepaint = true;
						}
					}
					break;


			}

			GUILayout.Space(20);

			//애니메이션 작업 중이라면 => ControlParam의 값을 바로 keyframe
			if (isRepaint && isAnimEditing && animTimelineLayer != null && isCurSelected)
			{
				if (animKeyframe == null && Editor._isAnimAutoKey)
				{
					//AnimKeyframe이 없으며 AutoKey가 추가되어 있다면
					//자동으로 키를 만들자.
					animKeyframe = Editor.Controller.AddAnimKeyframe(Editor.Select.AnimClip.CurFrame, animTimelineLayer, true, false, false, true);
				}

				if (animKeyframe != null)
				{
					apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.Anim_KeyframeValueChanged, Editor, Editor._portrait, animKeyframe, true);

					switch (controlParam._valueType)
					{
						case apControlParam.TYPE.Int:
							animKeyframe._conSyncValue_Int = controlParam._int_Cur;
							break;

						case apControlParam.TYPE.Float:
							animKeyframe._conSyncValue_Float = controlParam._float_Cur;
							break;

						case apControlParam.TYPE.Vector2:
							animKeyframe._conSyncValue_Vector2 = controlParam._vec2_Cur;
							break;
					}
				}

			}

			if (isRepaint)
			{
				//Editor.Repaint();
				Editor.SetRepaint();
			}
		}


		public void ResetControlParams()
		{
			Editor.ParamControl._controlParams.Clear();
			Editor.ParamControl.MakeReservedParams();
		}


		public void RemoveRecordKey(apModifierParamSet recordedKey, List<apModifierParamSet> recordKeyParamSet)
		{
			bool isResetSelect_ParamSet = false;
			bool isResetSelect_ParamSetGroup = false;
			if (recordedKey == null)
			{
				return;
			}

			if (recordedKey == Editor.Select.ParamSetOfMod)
			{
				isResetSelect_ParamSet = true;
			}

			apEditorUtil.SetRecord_MeshGroupAndModifier(apUndoGroupData.ACTION.MeshGroup_RemoveParamSet,
									Editor,
									Editor.Select.MeshGroup,
									Editor.Select.Modifier,
									recordedKey,
									false);

			if (Editor.Select.Modifier != null && Editor.Select.SubEditedParamSetGroup != null)
			{
				apModifierParamSetGroup paramSetGroup = Editor.Select.SubEditedParamSetGroup;
				paramSetGroup._paramSetList.Remove(recordedKey);

				//Editor.Select.Modifier._paramSetList.Remove(recordedKey);
				Editor.Select.Modifier.RefreshParamSet();

				Editor.Select.MeshGroup.LinkModMeshRenderUnits();//<<Link 전에 이걸 먼저 선언한다.
				Editor.Select.MeshGroup.RefreshModifierLink();

				if (!Editor.Select.Modifier._paramSetGroup_controller.Contains(paramSetGroup))
				{
					//그사이에 ParamSetGroup이 사라졌다면
					isResetSelect_ParamSetGroup = true;
				}


			}

			if (recordKeyParamSet != null)
			{
				recordKeyParamSet.Remove(recordedKey);
			}

			//Select에서 선택중인게 삭제 되었다면..
			if (isResetSelect_ParamSet)
			{
				Editor.Select.SetParamSetOfModifier(null);
			}
			if (isResetSelect_ParamSetGroup)
			{
				Editor.Select.SetParamSetGroupOfModifier(null);

			}

			//추가 : ExMode에 추가한다.
			Editor.Select.RefreshMeshGroupExEditingFlags(
								Editor.Select.MeshGroup,
								Editor.Select.Modifier,
								Editor.Select.SubEditedParamSetGroup,
								null,
								true);
		}

		public void OnHotKeyEvent_GizmoSelect(object paramObject)
		{
			Editor.Gizmos.SetControlType(apGizmos.CONTROL_TYPE.Select);
		}
		public void OnHotKeyEvent_GizmoMove(object paramObject)
		{
			Editor.Gizmos.SetControlType(apGizmos.CONTROL_TYPE.Move);
		}
		public void OnHotKeyEvent_GizmoRotate(object paramObject)
		{
			Editor.Gizmos.SetControlType(apGizmos.CONTROL_TYPE.Rotate);
		}
		public void OnHotKeyEvent_GizmoScale(object paramObject)
		{
			Editor.Gizmos.SetControlType(apGizmos.CONTROL_TYPE.Scale);
		}

		public void OnHotKeyEvent_OnionVisibleToggle(object paramObject)
		{
			Editor.Onion.SetVisible(!Editor.Onion.IsVisible);
		}


		// 추가 19.6.3 : MaterialSet에 관련된 함수들
		//-------------------------------------------------------------------------------
		public void LinkMaterialSets()
		{
			if(_editor == null || _editor._portrait == null)
			{
				return;
			}

			apPortrait portrait = _editor._portrait;
			if(portrait._materialSets == null)
			{
				portrait._materialSets = new List<apMaterialSet>();
			}
			List<apMaterialSet> matSets = portrait._materialSets;

			apMaterialLibrary matLibrary = _editor.MaterialLibrary;

			//만약 Preset이 없다면 Load.
			if(matLibrary.Presets.Count == 0)
			{
				matLibrary.Load();
			}

			//1. 만약 MatSet이 아무것도 없다면, 기본값인 Unlit을 연결해줘야 한다.
			if(matSets.Count == 0)
			{
				AddMaterialSet(matLibrary.Presets[0], true, true);
			}


			//2. Shader, Texture Asset들을 연결하자 (Portrait의 데이터와 Library 포함)
			for (int iMat = 0; iMat < portrait._materialSets.Count; iMat++)
			{
				LinkMaterialSetAssets(portrait._materialSets[iMat], false, portrait);
			}

			for (int iMat = 0; iMat < matLibrary.Presets.Count; iMat++)
			{
				LinkMaterialSetAssets(matLibrary.Presets[iMat], true, portrait);
			}

			//3. Default가 1개여야 한다. 되도록 설정하자.
			int nDefault = 0;
			for (int i = 0; i < matSets.Count; i++)
			{
				apMaterialSet matSet = matSets[i];
				if(matSet._isDefault)
				{
					nDefault++;
				}
			}

			if(nDefault == 0)
			{
				//Default가 없다면?
				//첫번째 항목을 Default로 설정
				matSets[0]._isDefault = true;
			}
			else if(nDefault > 1)
			{
				//Default가 1개를 넘었다면?
				//맨 앞의 Default 외의 Default는 해제
				bool isDefaultFound = false;
				for (int i = 0; i < matSets.Count; i++)
				{
					apMaterialSet matSet = matSets[i];
					if (matSet._isDefault)
					{
						if (!isDefaultFound)
						{
							//유지
							isDefaultFound = true;
						}
						else
						{
							//Default 해제
							matSet._isDefault = false;
						}
					}
					
				}
			}

		}
		


		public void LinkMaterialSetAssets(apMaterialSet matSet, bool isPreset, apPortrait portrait)
		{
			//1. Shader 연결
			MaterialSetLoadResult result_Shader_Normal_AlphaBlend =		LoadAssetOfMaterialSet<Shader>(	matSet._shader_Normal_AlphaBlend,		matSet._shaderPath_Normal_AlphaBlend);
			MaterialSetLoadResult result_Shader_Normal_Additive =		LoadAssetOfMaterialSet<Shader>(	matSet._shader_Normal_Additive,			matSet._shaderPath_Normal_Additive);
			MaterialSetLoadResult result_Shader_Normal_SoftAdditive =	LoadAssetOfMaterialSet<Shader>(	matSet._shader_Normal_SoftAdditive,		matSet._shaderPath_Normal_SoftAdditive);
			MaterialSetLoadResult result_Shader_Normal_Multiplicative = LoadAssetOfMaterialSet<Shader>(	matSet._shader_Normal_Multiplicative,	matSet._shaderPath_Normal_Multiplicative);

			MaterialSetLoadResult result_Shader_Clipped_AlphaBlend =		LoadAssetOfMaterialSet<Shader>(	matSet._shader_Clipped_AlphaBlend,		matSet._shaderPath_Clipped_AlphaBlend);
			MaterialSetLoadResult result_Shader_Clipped_Additive =			LoadAssetOfMaterialSet<Shader>(	matSet._shader_Clipped_Additive,		matSet._shaderPath_Clipped_Additive);
			MaterialSetLoadResult result_Shader_Clipped_SoftAdditive =		LoadAssetOfMaterialSet<Shader>(	matSet._shader_Clipped_SoftAdditive,	matSet._shaderPath_Clipped_SoftAdditive);
			MaterialSetLoadResult result_Shader_Clipped_Multiplicative =	LoadAssetOfMaterialSet<Shader>(	matSet._shader_Clipped_Multiplicative,	matSet._shaderPath_Clipped_Multiplicative);

			MaterialSetLoadResult result_Shader_L_Normal_AlphaBlend =		LoadAssetOfMaterialSet<Shader>(	matSet._shader_L_Normal_AlphaBlend,		matSet._shaderPath_L_Normal_AlphaBlend);
			MaterialSetLoadResult result_Shader_L_Normal_Additive =			LoadAssetOfMaterialSet<Shader>(	matSet._shader_L_Normal_Additive,		matSet._shaderPath_L_Normal_Additive);
			MaterialSetLoadResult result_Shader_L_Normal_SoftAdditive =		LoadAssetOfMaterialSet<Shader>(	matSet._shader_L_Normal_SoftAdditive,	matSet._shaderPath_L_Normal_SoftAdditive);
			MaterialSetLoadResult result_Shader_L_Normal_Multiplicative =	LoadAssetOfMaterialSet<Shader>(	matSet._shader_L_Normal_Multiplicative,	matSet._shaderPath_L_Normal_Multiplicative);

			MaterialSetLoadResult result_Shader_L_Clipped_AlphaBlend =		LoadAssetOfMaterialSet<Shader>(	matSet._shader_L_Clipped_AlphaBlend,		matSet._shaderPath_L_Clipped_AlphaBlend);
			MaterialSetLoadResult result_Shader_L_Clipped_Additive =		LoadAssetOfMaterialSet<Shader>(	matSet._shader_L_Clipped_Additive,			matSet._shaderPath_L_Clipped_Additive);
			MaterialSetLoadResult result_Shader_L_Clipped_SoftAdditive =	LoadAssetOfMaterialSet<Shader>(	matSet._shader_L_Clipped_SoftAdditive,		matSet._shaderPath_L_Clipped_SoftAdditive);
			MaterialSetLoadResult result_Shader_L_Clipped_Multiplicative =	LoadAssetOfMaterialSet<Shader>(	matSet._shader_L_Clipped_Multiplicative,	matSet._shaderPath_L_Clipped_Multiplicative);

			MaterialSetLoadResult result_Shader_AlphaMask =	LoadAssetOfMaterialSet<Shader>(	matSet._shader_AlphaMask,	matSet._shaderPath_AlphaMask);

			//Normal
			matSet._shader_Normal_AlphaBlend =		result_Shader_Normal_AlphaBlend.asset as Shader;
			matSet._shaderPath_Normal_AlphaBlend =	result_Shader_Normal_AlphaBlend.path;

			matSet._shader_Normal_Additive =		result_Shader_Normal_Additive.asset as Shader;
			matSet._shaderPath_Normal_Additive =	result_Shader_Normal_Additive.path;

			matSet._shader_Normal_SoftAdditive =		result_Shader_Normal_SoftAdditive.asset as Shader;
			matSet._shaderPath_Normal_SoftAdditive =	result_Shader_Normal_SoftAdditive.path;

			matSet._shader_Normal_Multiplicative =		result_Shader_Normal_Multiplicative.asset as Shader;
			matSet._shaderPath_Normal_Multiplicative =	result_Shader_Normal_Multiplicative.path;

			//Clipped
			matSet._shader_Clipped_AlphaBlend =		result_Shader_Clipped_AlphaBlend.asset as Shader;
			matSet._shaderPath_Clipped_AlphaBlend =	result_Shader_Clipped_AlphaBlend.path;

			matSet._shader_Clipped_Additive =		result_Shader_Clipped_Additive.asset as Shader;
			matSet._shaderPath_Clipped_Additive =	result_Shader_Clipped_Additive.path;

			matSet._shader_Clipped_SoftAdditive =		result_Shader_Clipped_SoftAdditive.asset as Shader;
			matSet._shaderPath_Clipped_SoftAdditive =	result_Shader_Clipped_SoftAdditive.path;

			matSet._shader_Clipped_Multiplicative =		result_Shader_Clipped_Multiplicative.asset as Shader;
			matSet._shaderPath_Clipped_Multiplicative =	result_Shader_Clipped_Multiplicative.path;

			//Normal (Linear)
			matSet._shader_L_Normal_AlphaBlend =		result_Shader_L_Normal_AlphaBlend.asset as Shader;
			matSet._shaderPath_L_Normal_AlphaBlend =	result_Shader_L_Normal_AlphaBlend.path;

			matSet._shader_L_Normal_Additive =		result_Shader_L_Normal_Additive.asset as Shader;
			matSet._shaderPath_L_Normal_Additive =	result_Shader_L_Normal_Additive.path;

			matSet._shader_L_Normal_SoftAdditive =		result_Shader_L_Normal_SoftAdditive.asset as Shader;
			matSet._shaderPath_L_Normal_SoftAdditive =	result_Shader_L_Normal_SoftAdditive.path;

			matSet._shader_L_Normal_Multiplicative =		result_Shader_L_Normal_Multiplicative.asset as Shader;
			matSet._shaderPath_L_Normal_Multiplicative =	result_Shader_L_Normal_Multiplicative.path;

			//Clipped (Linear)
			matSet._shader_L_Clipped_AlphaBlend =		result_Shader_L_Clipped_AlphaBlend.asset as Shader;
			matSet._shaderPath_L_Clipped_AlphaBlend =	result_Shader_L_Clipped_AlphaBlend.path;

			matSet._shader_L_Clipped_Additive =		result_Shader_L_Clipped_Additive.asset as Shader;
			matSet._shaderPath_L_Clipped_Additive =	result_Shader_L_Clipped_Additive.path;

			matSet._shader_L_Clipped_SoftAdditive =		result_Shader_L_Clipped_SoftAdditive.asset as Shader;
			matSet._shaderPath_L_Clipped_SoftAdditive =	result_Shader_L_Clipped_SoftAdditive.path;

			matSet._shader_L_Clipped_Multiplicative =		result_Shader_L_Clipped_Multiplicative.asset as Shader;
			matSet._shaderPath_L_Clipped_Multiplicative =	result_Shader_L_Clipped_Multiplicative.path;

			//Alpha Mask
			matSet._shader_AlphaMask =		result_Shader_AlphaMask.asset as Shader;
			matSet._shaderPath_AlphaMask =	result_Shader_AlphaMask.path;



			if(!isPreset)
			{
				matSet._linkedPresetMaterial = _editor.MaterialLibrary.GetPresetUnit(matSet._linkedPresetID);
				if(matSet._linkedPresetMaterial == null)
				{
					matSet._linkedPresetID = -1;
				}
			}
			else
			{
				matSet._linkedPresetMaterial = null;
				matSet._linkedPresetID = -1;
			}
			

			//2. 텍스쳐 연결
			//연결될 텍스쳐 데이터들
			List<apTextureData> srcImages = portrait._textureData;

			apMaterialSet.PropertySet propSet = null;
			for (int iProp = 0; iProp < matSet._propertySets.Count; iProp++)
			{
				propSet = matSet._propertySets[iProp];

				if (propSet._propType != apMaterialSet.SHADER_PROP_TYPE.Texture)
				{
					//텍스쳐 타입이 아니라면, 데이터를 삭제하자.
					propSet._value_CommonTexture = null;
					propSet._commonTexturePath = "";
					propSet._imageTexturePairs.Clear();
				}
				else
				{
					//Common Texture를 사용하는 경우
					MaterialSetLoadResult commonTexResult = LoadAssetOfMaterialSet<Texture>(propSet._value_CommonTexture, propSet._commonTexturePath);

					propSet._value_CommonTexture = commonTexResult.asset as Texture;
					propSet._commonTexturePath = commonTexResult.path;

					
					//텍스쳐 <-> 이미지 (TextureData) 연결
					//- 연결은 자동. 
					//1) 일단 기존 데이터에서 연결을 하자
					//적당한 Image가 없다면 > 연결 해제
					apMaterialSet.PropertySet.ImageTexturePair imgTexPair = null;
					for (int iPair = 0; iPair < propSet._imageTexturePairs.Count; iPair++)
					{
						imgTexPair = propSet._imageTexturePairs[iPair];

						//일단 연결
						imgTexPair._targetTextureData = portrait.GetTexture(imgTexPair._textureDataID);
					}

					propSet._imageTexturePairs.RemoveAll(delegate (apMaterialSet.PropertySet.ImageTexturePair a)
					{
						return a._targetTextureData == null;//<<Link에 실패한 경우
						});

					//2) TextureData에서 존재하지 않는 데이터가 있다면 자동으로 추가
					for (int iSrc = 0; iSrc < srcImages.Count; iSrc++)
					{
						apTextureData srcImage = srcImages[iSrc];

						if (!propSet._imageTexturePairs.Exists(delegate (apMaterialSet.PropertySet.ImageTexturePair a)
						 {
							 return a._targetTextureData == srcImage;
						 }))
						{
							//아직 등록되지 않은 TextureData이다.
							apMaterialSet.PropertySet.ImageTexturePair newPair = new apMaterialSet.PropertySet.ImageTexturePair();
							newPair._textureDataID = srcImage._uniqueID;
							newPair._targetTextureData = srcImage;
							newPair._textureAsset = null;
							newPair._textureAssetPath = "";

							//Pair 리스트에 추가하자.
							propSet._imageTexturePairs.Add(newPair);
						}
					}



					for (int iPair = 0; iPair < propSet._imageTexturePairs.Count; iPair++)
					{
						imgTexPair = propSet._imageTexturePairs[iPair];

						//이제 텍스쳐 에셋과 연결
						MaterialSetLoadResult textureLoadResult = LoadAssetOfMaterialSet<Texture>(imgTexPair._textureAsset, imgTexPair._textureAssetPath);

						imgTexPair._textureAsset = textureLoadResult.asset as Texture;
						imgTexPair._textureAssetPath = textureLoadResult.path;
					}

					//끝으로, 이름순으로 정렬
					propSet._imageTexturePairs.Sort(
					delegate (apMaterialSet.PropertySet.ImageTexturePair a, apMaterialSet.PropertySet.ImageTexturePair b)
					{
						return string.Compare(a._targetTextureData._name, b._targetTextureData._name);
					});
				}
				
			}
		}


		private MaterialSetLoadResult LoadAssetOfMaterialSet<T>(T asset, string assetPath) where T : UnityEngine.Object
		{
			 MaterialSetLoadResult result = new MaterialSetLoadResult();

			if(asset != null)
			{
				//Shader가 이미 있다면 > Path를 변경하자
				string pathOfAsset = AssetDatabase.GetAssetPath(asset);
				result.asset = asset;
				result.path = pathOfAsset;
			}
			else
			{
				//Shader가 없다면 > Path에서 에셋을 열자
				T assetFromPath = AssetDatabase.LoadAssetAtPath<T>(assetPath);
				if(assetFromPath != null)
				{
					result.asset = assetFromPath;
					result.path = assetPath;
				}
				else
				{
					//로드 실패
					result.asset = null;
					result.path = "";
				}
			}

			return result;
			
		}

		private struct MaterialSetLoadResult
		{
			public UnityEngine.Object asset;
			public string path;
		}
		

		



		/// <summary>
		/// Src가 되는 MaterialSet으로 부터 새로운 MaterialSet을 생성한다.
		/// Preset이라면 연동이 되어야 하며, 그렇지 않다면 값만 복사된다.
		/// Src는 꼭 넣어주자.
		/// </summary>
		/// <param name="srcPreset"></param>
		/// <returns></returns>
		public apMaterialSet AddMaterialSet(apMaterialSet srcMaterialSet, bool isFromPreset, bool isDefault)
		{
			if(_editor == null || _editor._portrait == null)
			{
				return null;
			}
			apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.MaterialSetAdded, _editor, _editor._portrait, _editor._portrait, false);

			apPortrait portrait = _editor._portrait;
			if(portrait._materialSets == null)
			{
				portrait._materialSets = new List<apMaterialSet>();
			}
			//List<apMaterialSet> matSets = portrait._materialSets;
			apMaterialLibrary matLibrary = _editor.MaterialLibrary;

			//새로운 ID를 만들자
			int newUniqueID = -1;
			int cnt = 0;
			while(true)
			{
				int nextID = UnityEngine.Random.Range(2000, 99999);
				apMaterialSet existMatSet = portrait.GetMaterialSet(nextID);
				if(existMatSet == null)
				{
					//이 값은 사용할 수 있겠다.
					newUniqueID = nextID;
					break;
				}

				cnt++;
				if(cnt > 100)
				{
					//100번이 넘도록 실패했다면?
					break;
				}
			}
			if(newUniqueID < 0)
			{
				//ID 할당에 
				//1000~1999 사이의 값을 할당하자.
				for (int iNextID = 1000; iNextID < 2000; iNextID++)
				{
					if(portrait.GetMaterialSet(iNextID) == null)
					{
						newUniqueID = iNextID;
						break;
					}
				}
			}

			if(newUniqueID < 0)
			{
				Debug.LogError("AnyPortrait : Failed to create new Material Set. please try again.");
				return null;
			}

			apMaterialSet newMatSet = new apMaterialSet();
			newMatSet.Init();
			if(srcMaterialSet != null)
			{
				newMatSet.CopyFromSrc(srcMaterialSet, newUniqueID, isFromPreset, false, false);
			}
			else
			{
				newMatSet._uniqueID = newUniqueID;
				newMatSet._name = "<No Name>";

				//새로운 MatSet에 기본 프로퍼티는 넣어야지
				newMatSet.AddProperty("_Color", true, apMaterialSet.SHADER_PROP_TYPE.Color);
				newMatSet.AddProperty("_MainTex", true, apMaterialSet.SHADER_PROP_TYPE.Texture);
				newMatSet.AddProperty("_MaskTex", true, apMaterialSet.SHADER_PROP_TYPE.Texture);
				newMatSet.AddProperty("_MaskScreenSpaceOffset", true, apMaterialSet.SHADER_PROP_TYPE.Vector);
			}
			
			newMatSet._isDefault = isDefault;

			portrait._materialSets.Add(newMatSet);


			//만약, isDefault = true였다면, 다른 MaterialSet들 중에 Default가 있는걸 없애야함
			if(isDefault)
			{
				apMaterialSet curMatSet = null;
				for (int i = 0; i < portrait._materialSets.Count; i++)
				{
					curMatSet = portrait._materialSets[i];
					if(curMatSet._isDefault && curMatSet != newMatSet)
					{
						curMatSet._isDefault = false;//<<Default 속성을 해제한다.
					}
				}
			}

			//연결
			LinkMaterialSets();

			return newMatSet;
		}


		/// <summary>
		/// Portrait의 MaterialSet을 연결된 Preset의 값으로 복구한다.
		/// Texture Per Image 속성의 Texture 프로퍼티는 값을 유지한다.
		/// </summary>
		/// <param name="matSet"></param>
		public bool RestoreMaterialSetToPreset(apMaterialSet matSet)
		{
			if(matSet == null || matSet._linkedPresetMaterial == null || !_editor._portrait._materialSets.Contains(matSet))
			{
				return false;
			}

			apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.MaterialSetChanged, _editor, _editor._portrait, _editor._portrait, false);

			apMaterialSet preset = matSet._linkedPresetMaterial;

			//값을 복사하자.
			matSet._icon = preset._icon;

			matSet._shaderPath_Normal_AlphaBlend =			preset._shaderPath_Normal_AlphaBlend;
			matSet._shaderPath_Normal_Additive =			preset._shaderPath_Normal_Additive;
			matSet._shaderPath_Normal_SoftAdditive =		preset._shaderPath_Normal_SoftAdditive;
			matSet._shaderPath_Normal_Multiplicative =		preset._shaderPath_Normal_Multiplicative;
			matSet._shaderPath_Clipped_AlphaBlend =			preset._shaderPath_Clipped_AlphaBlend;
			matSet._shaderPath_Clipped_Additive =			preset._shaderPath_Clipped_Additive;
			matSet._shaderPath_Clipped_SoftAdditive =		preset._shaderPath_Clipped_SoftAdditive;
			matSet._shaderPath_Clipped_Multiplicative =		preset._shaderPath_Clipped_Multiplicative;
			matSet._shaderPath_L_Normal_AlphaBlend =		preset._shaderPath_L_Normal_AlphaBlend;
			matSet._shaderPath_L_Normal_Additive =			preset._shaderPath_L_Normal_Additive;
			matSet._shaderPath_L_Normal_SoftAdditive =		preset._shaderPath_L_Normal_SoftAdditive;
			matSet._shaderPath_L_Normal_Multiplicative =	preset._shaderPath_L_Normal_Multiplicative;
			matSet._shaderPath_L_Clipped_AlphaBlend =		preset._shaderPath_L_Clipped_AlphaBlend;
			matSet._shaderPath_L_Clipped_Additive =			preset._shaderPath_L_Clipped_Additive;
			matSet._shaderPath_L_Clipped_SoftAdditive =		preset._shaderPath_L_Clipped_SoftAdditive;
			matSet._shaderPath_L_Clipped_Multiplicative =	preset._shaderPath_L_Clipped_Multiplicative;
			matSet._shaderPath_AlphaMask =					preset._shaderPath_AlphaMask;

			matSet._shader_Normal_AlphaBlend =			preset._shader_Normal_AlphaBlend;
			matSet._shader_Normal_Additive =			preset._shader_Normal_Additive;
			matSet._shader_Normal_SoftAdditive =		preset._shader_Normal_SoftAdditive;
			matSet._shader_Normal_Multiplicative =		preset._shader_Normal_Multiplicative;
			matSet._shader_Clipped_AlphaBlend =			preset._shader_Clipped_AlphaBlend;
			matSet._shader_Clipped_Additive =			preset._shader_Clipped_Additive;
			matSet._shader_Clipped_SoftAdditive =		preset._shader_Clipped_SoftAdditive;
			matSet._shader_Clipped_Multiplicative =		preset._shader_Clipped_Multiplicative;
			matSet._shader_L_Normal_AlphaBlend =		preset._shader_L_Normal_AlphaBlend;
			matSet._shader_L_Normal_Additive =			preset._shader_L_Normal_Additive;
			matSet._shader_L_Normal_SoftAdditive =		preset._shader_L_Normal_SoftAdditive;
			matSet._shader_L_Normal_Multiplicative =	preset._shader_L_Normal_Multiplicative;
			matSet._shader_L_Clipped_AlphaBlend =		preset._shader_L_Clipped_AlphaBlend;
			matSet._shader_L_Clipped_Additive =			preset._shader_L_Clipped_Additive;
			matSet._shader_L_Clipped_SoftAdditive =		preset._shader_L_Clipped_SoftAdditive;
			matSet._shader_L_Clipped_Multiplicative =	preset._shader_L_Clipped_Multiplicative;
			matSet._shader_AlphaMask =					preset._shader_AlphaMask;


			matSet._isNeedToSetBlackColoredAmbient = preset._isNeedToSetBlackColoredAmbient;

			//프로퍼티 복사
			//- 일단 기존 프로퍼티를 복사
			//- 프로퍼티를 복사하면서 Texture인 경우에만 보존
			List<apMaterialSet.PropertySet> prevPropertySets = matSet._propertySets;

			matSet._propertySets = new List<apMaterialSet.PropertySet>();//<<새롭게 만들자


			apMaterialSet.PropertySet presetPropSet = null;
			apMaterialSet.PropertySet dstPropSet = null;
			for (int i = 0; i < preset._propertySets.Count; i++)
			{
				presetPropSet = preset._propertySets[i];
				dstPropSet = null;

				if(presetPropSet._propType == apMaterialSet.SHADER_PROP_TYPE.Texture
					&& !presetPropSet._isReserved)
				{
					//Reserved가 아닌 Texture타입의 PropSet이라면
					//기존값이 있는지 확인하자.
					apMaterialSet.PropertySet existPropSet = prevPropertySets.Find(delegate(apMaterialSet.PropertySet a)
					{
						return string.Equals(a._name, presetPropSet._name) 
						&& !a._isReserved 
						&& a._propType == apMaterialSet.SHADER_PROP_TYPE.Texture
						&& !a._isCommonTexture;//<<Common Texture가 아닌 경우에 한해서
					});

					if(existPropSet != null)
					{
						//대체 가능한 PropSet이 있다면
						dstPropSet = existPropSet;
					}
				}

				if(dstPropSet == null)
				{
					//새로 복사해서 만들어야 하는 경우
					dstPropSet = new apMaterialSet.PropertySet();
					dstPropSet.CopyFromSrc(presetPropSet);
				}

				matSet._propertySets.Add(dstPropSet);
				
			}
			
			LinkMaterialSetAssets(matSet, false, _editor._portrait);


			return true;

		}


		public apMaterialSet DuplicateMaterialSet(apMaterialSet srcMatSet)
		{
			if(_editor == null || _editor._portrait == null || srcMatSet == null)
			{
				//처리 불가
				return null;
			}

			bool isPreset = false;

			if(_editor._portrait._materialSets.Contains(srcMatSet))
			{
				//Portrait의 데이터이다.
				isPreset = false;
			}
			else if(_editor.MaterialLibrary.Presets.Contains(srcMatSet))
			{
				//프리셋이다.
				isPreset = true;
			}
			else
			{
				//???? 알수 없는 데이터
				return null;
			}

			apMaterialSet newMatSet = null;
			if(isPreset)
			{
				//프리셋이라면
				newMatSet = _editor.MaterialLibrary.AddNewPreset(srcMatSet, true, srcMatSet._name + " Copy");
			}
			else
			{
				

				//프리셋이 아니라면
				newMatSet = AddMaterialSet(srcMatSet, false, false);
				if(newMatSet == null)
				{
					return null;
				}
				newMatSet._name = newMatSet._name + " Copy";
			}

			return newMatSet;
		}


		public void RemoveMaterialSet(apMaterialSet matSet)
		{
			if(_editor == null || _editor._portrait == null || matSet == null)
			{
				//처리 불가
				return;
			}

			bool isPreset = false;

			if(_editor._portrait._materialSets.Contains(matSet))
			{
				//Portrait의 데이터이다.
				isPreset = false;
			}
			else if(_editor.MaterialLibrary.Presets.Contains(matSet))
			{
				//프리셋이다.
				isPreset = true;
			}
			else
			{
				//???? 알수 없는 데이터
				return;
			}

			if(isPreset)
			{
				//예약된 Material Set이면 삭제 불가
				if(matSet._isReserved)
				{
					return;
				}

				_editor.MaterialLibrary.Presets.Remove(matSet);
				_editor.MaterialLibrary.Save();
			}
			else
			{
				apEditorUtil.SetRecord_Portrait(apUndoGroupData.ACTION.MaterialSetRemoved, _editor, _editor._portrait, _editor._portrait, false);

				_editor._portrait._materialSets.Remove(matSet);
			}
		}
		//-------------------------------------------------------------------------------------------
		

		//-------------------------------------------------------------------------------------------
		
	}
}