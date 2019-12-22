------------------------------------------------------------
		AnyPortrait (Version 1.2.1)
------------------------------------------------------------


Thank you for using AnyPortrait.
AnyPortrait is an extension that helps you create 2D characters in Unity.
When you create a game, I hope that AnyPortrait will be a great help.

Here are some things to know before using AnyPortrait:


1. How to start

To use AnyPortrait, go to "Window > AnyPortrait > 2D Editor".
The work is done in the unit called Portrait.
You can create a new portrait or open an existing one.
For more information, please refer to the User Guide.



2. User Guide

The User's Guide is "AnyPortrait User Guide.pdf" in the Documentation folder.
This file contains two basic tutorials.

AnyPortrait has more features than that, so we recommend that you refer to the homepage.

Homepage with guides : https://www.rainyrizzle.com/



3. Languages

AnyPortrait supports 10 languages.
(English, Korean, French, German, Spanish, Danish, Japanese, Chinese (Traditional / Simplified), Italian, Polish)

It is recommended to select the appropriate language from the Setting menu of AnyPortrait.

The homepage supports English, Korean and Japanese.



4. Support

If you have any problems or problems with using AnyPortrait, please contact us.
You can also report the editor's typographical errors.
If you have the functionality you need, we will try to implement as much as possible.

You can contact us by using the web page or email us.

Report Page : 
https://www.rainyrizzle.com/anyportrait-report-eng (English)
https://www.rainyrizzle.com/anyportrait-report-kor (Korean)

EMail : contactrainyrizzle@gmail.com


Note: I would appreciate it if you acknowledge that it may take some time 
because there are not many developers on our team.



5. License

The license is written in the file "license.txt".
You can also check in "Setting > About" of AnyPortrait.



6. Target device and platform

AnyPortrait has been developed to support PC, mobile, web, and console.
Much has been optimized to be able to run in real games.
We have also made great efforts to ensure compatibility with graphical problems.


However, for practical reasons we can not actually test in all environments, there may be potential problems.
There may also be performance differences depending on your results.

Since we aim to run on any device in any case, 
please contact us for any issues that may be causing the problem.



7. Update Notes

1.0.1 (March 18, 2018)
- Added Italian and Polish.
- Supports Linear Color Space.
- You can change the texture asset setting in the editor.

1.0.2 (March 27, 2018)
- Fixed an issue where the bake could no longer be done with an error message if the mesh was modified after bake.
- Fixed an issue where the backup file could not be opened.
- Fixed a problem where rendering can not be done if Scale has negative value.
- Improved Modifier Lock function.
- Fixed an issue that the modifier is unlocked and multi-processing does not work properly.
- Added Sorting Layer / Order function. You can set it in the Bake dialog, Inspector.
- Sorting Layer / Order values ​​can be changed by script.
- If the target GameObject is Prefab, it is changed to apply automatically when Bake is done. This applies even if it is not Prefab Root.
- Fixed a bug in the number of error messages that users informed. Thank you.
- Fixed an error when importing a PSD file and a process failure.
- Fixed a problem where the shape of a character is distorted if Bake is continued.

1.0.3 (April 14, 2018)
- Significant improvements in Screen Capture
- Transparent color can be specified as background color (Except GIF animation)
- Added ability to save Sprite Sheet
- Screen capture Dialog is deleted and moved to the right screen to improve
- Support screen capture on Mac OSX
- Improved Physics Effects
- Corrected incorrectly calculated inertia when moving from outside
- Modify the gizmo to be inverted if the scale of the object is negative
- When replacing the texture of the mesh, Script Functions that can be replaced with an image registered in AnyPortrait has been added
- Fixed an issue that caused data errors to occur when undoing after creating or deleting objects
- Fixed a problem that when importing animation pose, data is missing while generating timeline automatically
- Fixed an issue where other vertices were selected when using the FFD tool
- Fixed an issue where vertex positions would be strange when undoing when using FFD tool
- Fixed an issue where the modifier did not recognize that the mesh was deleted, resulting in error code output
- Fixed an issue where the clipping mesh would not render properly if the Important option was turned off
- Fixed an issue where sub-mesh groups could not generate clipping meshes
- Fixed a problem where deleted mesh and mesh groups appeared as GameObjects
- Fixed a problem where the script does not change the texture of the mesh

1.0.4 (June 10, 2018)
- Animation can be controlled by Unity Mecanim.
- Bone IK became more natural.
- IK can be controlled by an external Bone.
- Weight can be set when IK is controlled by external Bone, and this weight can be linked to a Control Parameter.
- Mirror copying is possible when creating bones, and you can paste them in reversed poses when copying poses.
- 2 functions for Bones have been added.
- Added Auto-Key function to automatically generate keyframes when making an animation.
- Onion Skin has been improved to change color, rendering method, rendering order, and to render continuous frames during animation making.
- Ctrl + Alt (Command + Alt in OSX) and mouse drag to move or zoom in and out.
- After the mesh is added, it automatically switches to the Setting tab.
- A button has been added at the top of the screen to change "whether mesh output".
- Two-sided rendering can be set in the mesh setting.
- When the new version of AnyPortrait is updated, the first screen informs you.
- Press the Ctrl key (Command key in OSX) to change the color of the buttons that have customized settings.
- The title image of AnyPortrait has been added to the Demo folder.
- The 7th demo scene with new features in version 1.0.4 has been added.
- Fixed an issue where vertex colors were not rendered properly when setting the Physics Modifier of the clipped mesh.
- Fixed an issue where mesh without rigging information could not be processed properly after bake.
- Fixed an issue where some text was not translated in the Bake dialog.
- Fixed an issue where Depth would bake strangely when creating a nested mesh group when importing PSD files.
- Fixed an issue where meshes were generated strangely when creating Atlas of 4096 resolution in PSD files.
- Fixed an issue where the Morph (Animation) modifier was not processed correctly when running animations.

1.0.5 (June 16, 2018)
- Fixed script errors in apEditorUtil.cs in Mac OSX.

1.0.6 (July 14, 2018)
- Re-importing a PSD File is added
- Change the texture asset settings of Atlas created with PSD file to be high quality
- Added ability to collapse tool group in upper UI
- Setting whether to check the latest version in the editor setting dialog
- Fixed intermittent script error when editing animation
- Fixed an issue where when rigging in scene, rigging weights are not normalized and invalid values ​​are passed
- Fixed an error when checking the latest version

1.0.7 (August 6, 2018)
- Fixed an issue where Bake does not work or error occurs when Rigging Weight value is 0 or Bone is not specified
- Fixed iOS missing in default settings in DLL of AnyPortrait

1.1.0 (October 7, 2018)
- Generating Meshes Automatically is added.
- Mirror tool for editing meshes added.
- Added the ability to edit mesh vertices.
- Perspective camera is supported, and a Billboard option for this function is added.
- "Pirate Game 3D" demo scene, which is the 3D version of "Pirate Game", is added.
- When controlling animations as scripts, "SetAnimationSpeed" functions have been added to set the speed of animation.
- When creating meshes, if you press the Ctrl key (Command key on Mac OSX), the cursor snaps to the nearest vertex.
- When creating a mesh, if you press the Shift key and click a edge, a vertex is added on the edge.
- Make Mesh UI changed.
- You can change the Shadow setting (Receive Shadow, Cast Shadow) in the Bake setting.
- Bake dialog UI changed.
- Inspector UI changed.
- You can open the editor directly from the Inspector, and you can also bake it right away.
- Modifiers and Bones can be added to Child Mesh Groups, and the Parent Mesh Group can control them.
- A menu to open "Q & A Web page" is added.
- Fixed an issue where polygons are not generated properly when making a mesh.
- Fixed an issue where animations set to low speed or low FPS would not play smoothly.
- Fixed an issue where Hierarchy was not updated when deleting animations.
- Fixed an issue where Clipping Mask did not work intermittently when playing game.
- Fixed an issue where the IK setting of the first bone was disabled when creating a sequence in succession.
- Fixed an issue where data was intermittently missing when manually saving backups.
- Fixed an error when controlling the control parameters in the Inspector.
- Fixed an issue where thumbnails are output abnormally in the iOS development environment.
- Fixed an issue where animated clips were continuously created unnecessarily when using Optimized Bake while using Mecanim.

1.1.1 (October 11, 2018)
- Fixed a problem where the positions and angles of child bones changed when linking bones.

1.1.2 (November 11, 2018)
- MP4 video export function is added. (available from Unity 2017.4)
- GIF animation quality option is changed to easily set to four levels.
- Maximum Quality of GIF animation is slightly better than it used to be.
- UI is changed to allow stop during animation capture.
- "Lightweight Render Pipeline" is supported. (available from Unity 2018.2)
- The ability to change the Ambient Light to black for AnyPortrait in the Bake dialog is added.
- Script functions to use apPlayData related to animation playback are added.
- If language is set to Japanese, Japanese website will be opened when selecting a menu
- It is changed that Editing mode is started automatically during a process of adding objects to Modifier.
- Fixed an issue where the clipping mask was not correctly calculated depending on the angle of the camera when rendering from a perspective camera.
- Fixed an issue where Blend function calculates weights strangely when doing rigging.
- Changed the editor to terminate automatically when artificially modifying the resource path of AnyPortrait.
- Fixed a problem where Bone was moved to the mouse position as soon as you clicked the Bone's default position.

1.1.3 (November 15, 2018)
- Added the ability to access the Asset Store directly if there is a new update
- Improved screen capture speed and quality
- No limitation on maximum size of screen capture resolution
- Fixed an issue that  transparency was not applied properly when capturing a screen
- Fixed the problem that the update log dialog does not connect to the Japanese homepage

1.1.4 (December 18, 2018)
- Added "Extra Option" to change rendering order and image in real time.
- Draw calls have been further optimized.
- Added "Refresh Meshes" button to refresh mesh in Inspector UI.
- Three scripting functions have been added to control the material of the mesh.
- The unnecessary object information UI is not displayed at the top of the screen.
- Fixed an issue where the parent mesh group appeared in the dialog to add a mesh group.

1.1.5 (December 24, 2018)
- Fixed an issue where the default depth of mesh does not change on Unity 2018.

1.1.6 (April 19, 2019)
- Unity's Timeline is now available, making it possible to create cinematic scenes
- Option to limit the performance of the editor to prevent laptop overheating is added
- Option to decide whether or not the "Selection Lock" be turned on when "Edit Mode" is turned on
- Drawcall is not increased even when the Scale of the Transform is inverted by a negative value
- When "Important option" is turned off, CPU optimization is improved more effectively
- The ability to change the order of items in the Hierarchy UI is added
- Functions to play animation from a specific point are added
- Functions to change "Sorting Layer/Order" targeting an optTransform are added
- The speed of the animation can be adjusted according to the "Speed Multiplier" property of "Animator"
- The design of Inspector UI is better than before
- A button is added to register the Control Parameter to the modifier without pressing the "Record key button"
- The path where Animation clips are saved in Mecanim setting is changed to "Relative path"
- "Edit Mode" turned on when adding a mesh to the Physics modifier
- If the signs of the scale interpolated by modifiers are different from each other, the value is changed discontinuously to not be through 0
- When editing a modifier, the "Edit mode" is not forcibly turned off even if the value of a Control Parameter is changed
- A button to duplicate an Animation event is added
- A warning message appears when a child Mesh Group is associated with an Animation clip
- You can select all vertices with "Ctrl + A"
- You can copy animation keyframes with "Ctrl + C, V"
- Animation curves can be modified in batch when multiple keyframes are selected
- An animation curve can be copied to keyframes of all timeline layers
- When you bake a character, a dialog appears asking you to change the scene's Ambient Color automatically
- "Length of a bone" is added in bone setting UI
- An issue where the logs are continuously output to the console when using some functions is fixed
- An error which is occurred on Hierarchy UI is fixed
- An issue where Unity stopped when docking the AnyPortrait editor to the Unity editor and turning Maximize on and off is fixed
- An issue where an animation with "Once" type could not play normally from Animator is fixed
- An issue where a portrait could no longer be opened when "Extra Option" targets bones is fixed
- A problem where multiple keys could be created with the same value of Control Parameters is fixed
- An error, which is occurred when a scene is switched or Unity editor is restarted with the AnyPortrait editor open, is fixed
- An issue where Extra Option value is missing when copying and pasting the modifier key is fixed
- An issue where FPS of animations are not apply properly when executing "Optimized Bake" is fixed
- An issue where vertices are moved to the mouse position when the mesh was created by pressing the Ctrl key and selecting the nearest vertex is fixed
- An issue where temporarily hidden meshes were forcibly shown when changing the value of a Control Parameter while "Edit Mode" was turned on is fixed
- A problem that can not import a PSD file with invalid channel information is fixed

1.1.7 (July 10, 2019)
- New features, "Material Library" and "Material Set" are added to manage various materials and shaders and apply various rendering techniques.
- Data of modifiers and animations are optimized to reduce the size of the prefab file
- Improved "AsyncInitialize" function to reduce CPU load is added
- Processing speed for some additions and deletions in animation work is improved
- Coping keyframes to other animation clips with "Ctrl+C,V" is available
- It is available to select and add multiple meshes and mesh groups to a mesh group at once
- User can set whether "Controller tab" is switched automatically when animation or modifier is selected
- User can set whether the "Temporary rendering" of the mesh will be reset for reasons such as Undo or key-value change during the task
- The button to reset the mesh's "Temporary rendering" is added
- An issue where "Do Not Show this message" worked reversely in the Ambient Color correction dialog is fixed
- An issue where existing keyframes could not be overwritten when copying animation keyframes with "Ctrl+C,V" key is fixed
- A problem that clipping settings of layers were not applied when importing PSD file is fixed
- A problem that the shadow and normal vector of 2-sided mesh are calculated abnormally is fixed
- An issue where the animation clip was not opened in the editor when the Start Frame and the End Frame are the same is fixed

1.1.8 (September 8, 2019)
- Rigging Modifier is improved
- The Slider UI to modify weights is added
- "Lock function" to restrict weight edits by other bones is added
- "Brush Mode" to edit weights using the mouse is added
- The function to select vertices rigged by the current bone
- The function to copy the rigging weights of multiple vertices based on position
- "Auto-Rig" is greatly improved
- The rigging weight of vertices is renderable as a shape of a pie-chart
- The option to render clipping meshes in LWRP (Lightweight Render Pipeline) is added
- The LWRP 2D material package is added in Material Library
- You can set whether the color of newly added bone is similar to the color of the parent bone in the Setting dialog
- The function to Snap the end of a bone to a child bone is added
- The function to duplicate a bone is added
- "LookAt" method of IK Controller works even for one bone or bone for which IK chain is not set
- You can hide the left and right UI, and the UI design that can fold the right UI up and down has been improved
- Sorting Group is supported
- An option for setting "Sorting Order" is added, and a related script function("SetSortingOrderChangedAutomatically") is added
- When changing the mesh group that is the target of an animation clip, if the parent mesh group is selected, the animation data is migrated without initializing
- A bug that the Modifier's Extra option and Animation Events would not be duplicated when cloning animations is fixed

1.2.0 (October 28, 2019)
- VR is supported
- Target Texture property of Cameras is supported
- It is changed that Billboard option does not apply in Bake processing, only apply while the game is running
- A bug where animation clips for Mecanim would not be saved properly if there is a white space in the save path is fixed.
- A bug that Custom FFD does not work properly when the size is 2 is fixed
- A bug that the bone is not be controlled right after it is detached is fixed
- A bug that functions which control meshes in a batch such as SetMeshImageAll and SetMeshColorAll do not work properly is fixed (Please execute Bake to apply)

1.2.1 (November 25, 2019)
- The performance of the AnyPortrait Editor has been slightly improved and stabilized
- Auto-Scrolling in the animation timeline UI has been improved so that it also works when adding a keyframe or selecting and editing objects
- Improved keyboard shortcuts in the animation timeline UI to better recognize them
- The process method of FPS in the AnyPortrait Editor has been changed to be easier to see
- A bug that Bake is failed when changing the Depth of a Child Mesh Group with the modifier's Extra option is fixed
- A bug that the rendering order changed by the modifier's Extra option is not applied during screen capture is fixed
- A bug that the Auto-Scrolling movement of animation timeline UI does not work properly is fixed
- A bug that the "Temporary Show/Hide" buttons are displayed incorrectly in the Object List UI is fixed



------------------------------------------------------------
			한국어 설명 (버전 1.2.1)
------------------------------------------------------------

AnyPortrait를 사용해주셔서 감사를 드립니다.
AnyPortrait는 2D 캐릭터를 유니티에서 직접 만들 수 있도록 개발된 확장 에디터입니다.
여러분이 게임을 만들 때, AnyPortrait가 많은 도움이 되기를 기대합니다.

아래는 AnyPortrait를 사용하기에 앞서서 알아두시면 좋을 내용입니다.


1. 시작하기

AnyPortrait를 실행하려면 "Window > AnyPortrait > 2D Editor"메뉴를 실행하시면 됩니다.
AnyPortrait는 Portrait라는 단위로 작업을 합니다.
새로운 Portrait를 만들거나 기존의 것을 여시면 됩니다.
더 많은 정보는 "사용 설명서"를 참고하시면 되겠습니다.



2. 사용 설명서

사용 설명서는 Documentation 폴더의 "AnyPortrait User Guide.pdf" 파일입니다.
이 문서에는 2개의 튜토리얼이 작성되어 있습니다.

AnyPortrait의 많은 기능을 사용하시려면 홈페이지를 참고하시길 권장합니다.

홈페이지 : https://www.rainyrizzle.com/



3. 언어

AnyPortrait는 10개의 언어를 지원합니다.
(영어, 한국어, 프랑스어, 독일어, 스페인어, 덴마크어, 일본어, 중국어(번체/간체), 이탈리아어, 폴란드어)

AnyPortrait의 설정 메뉴에서 언어를 선택할 수 있습니다.

홈페이지는 한국어와 영어, 일본어를 지원합니다.



4. 고객 지원

AnyPortrait를 사용하시면서 겪은 문제점이나 개선할 점이 있다면, 저희에게 문의를 주시길 바랍니다.
에디터의 오탈자를 문의 주셔도 좋습니다.
추가적으로 구현되면 좋은 기능을 알려주신다면, 가능한 범위 내에서 구현을 하도록 노력하겠습니다.

문의는 홈페이지나 이메일로 주시면 됩니다.


문의 페이지 : 
https://www.rainyrizzle.com/anyportrait-report-eng (영어)
https://www.rainyrizzle.com/anyportrait-report-kor (한국어)

이메일 : contactrainyrizzle@gmail.com


참고: 저희 팀의 개발자가 많지 않아 처리에 시간이 걸릴 수 있으므로 양해부탁드립니다.



5. 저작권

AnyPortrait에 관련된 저작권은 "license.txt" 파일에 작성이 되어있습니다.
AnyPortrait의 "설정 > About"에서도 확인할 수 있습니다.



6. 대상 기기와 플랫폼

AnyPortrait는 PC, 모바일, 웹, 콘솔에서 구동되도록 개발되었습니다.
실제 게임에서 사용되도록 최적화 하였습니다.
그래픽적인 문제에 대한 높은 호환성을 가지도록 노력하였습니다.

그렇지만, 현실적인 이유로 모든 환경에서 테스트를 할 수 없었기에, 잠재적인 문제점이 있을 수 있습니다.
경우에 따라 사용자의 작업 결과물에 따라서 성능에 차이가 있을 수도 있습니다.

저희는 모든 기기에서 어떠한 경우라도 정상적으로 동작하는 것을 목표로 삼고 있기 때문에,
실행 과정에서 겪는 모든 이슈에 대해 연락을 주신다면 매우 감사하겠습니다.



7. 업데이트 노트

1.0.1 (2018년 3월 18일)
- 이탈리아어, 폴란드어를 추가하였습니다.
- Linear Color Space를 지원합니다.
- 에디터에서 텍스쳐 에셋 설정을 변경할 수 있습니다.

1.0.2 (2018년 3월 27일)
- Bake를 한 이후에 다시 메시를 수정한 경우, 에러 메시지와 함께 더이상 Bake를 할 수 없는 문제가 수정되었습니다.
- 백업 파일을 열 수 없는 문제를 수정하였습니다.
- Scale이 음수 값을 가지는 경우 렌더링이 안되는 문제를 수정하였습니다.
- 모디파이어 잠금(Modifier Lock) 기능을 개선하였습니다.
- 모디파이어 잠금을 해제하고 다중 처리시 제대로 결과가 나오지 않은 점을 수정하였습니다.
- Sorting Layer/Order 기능을 추가하였습니다. Bake 다이얼로그, Inspector에서 설정할 수 있습니다.
- Sorting Layer/Order 값을 스크립트를 이용하여 변경할 수 있습니다.
- 대상이 되는 GameObject가 Prefab인 경우, Bake를 하면 자동으로 Apply를 하도록 변경되었습니다. Prefab Root가 아니어도 적용됩니다.
- 사용자 분들이 알려주신 다수의 에러 메시지들에 대한 버그를 수정하였습니다. 감사합니다.
- PSD 파일을 가져올 때 발생하는 에러와 처리 실패 문제를 수정하였습니다.
- Bake를 계속할 경우 캐릭터의 형태가 왜곡되는 문제를 수정하였습니다.

1.0.3 (2018년 4월 14일)
- 화면 캡쳐 기능이 개선되었습니다.
- 투명색으로 배경으로 화면을 캡쳐하여 이미지로 저장할 수 있습니다. (GIF 제외)
- 스프라이트 시트(Sprite Sheet)로 저장할 수 있습니다.
- 화면 캡쳐 UI가 변경되었습니다.
- Mac OSX에서 화면 캡쳐 기능을 지원합니다.
- 물리 모디파이어가 수정되었습니다.
- 외부에서 위치를 수정할 경우 관성이 잘못 적용되는 문제가 수정되었습니다.
- 객체의 스케일이 음수인 경우 기즈모가 반전되어 나타나도록 수정했습니다.
- 메시의 텍스쳐를 교체할 때, AnyPortrait에 등록된 이미지를 사용할 수 있는 스크립트 함수가 추가되었습니다.
- 객체를 생성하거나 삭제한 이후 "실행 취소"를 할때 발생하는 오류를 수정하였습니다.
- 애니메이션 포즈를 Import하면서 자동으로 타임라인이 생성될 때 데이터가 누락되지 않도록 하였습니다.
- FFD 툴을 사용할 때 다른 버텍스가 선택되지 않도록 수정하였습니다.
- FFD 툴을 사용하고 "실행 취소"를 하면 버텍스의 위치가 이상해지는 문제가 수정되었습니다.
- 모디파이어가 삭제된 메시를 잘못 인식하여 발생시키는 에러를 수정하였습니다.
- Important 옵션이 꺼지면 클리핑 메시가 제대로 렌더링하지 못하는 문제가 수정되었습니다.
- 하위 메시 그룹에서 클리핑 메시를 생성할 수 없는 문제가 수정되었습니다.
- 삭제한 메시나 메시 그룹이 GameObject로 등장하는 문제가 수정되었습니다.
- 스크립트의 함수로 메시의 텍스쳐를 변경할때, 제대로 반영되지 않는 문제가 수정되었습니다.

1.0.4 (2018년 6월 10일)
- 유니티 메카님으로 애니메이션을 제어할 수 있습니다.
- 본 IK의 처리가 자연스러워졌습니다.
- 외부의 본에 의해서 IK가 제어될 수 있습니다.
- 외부의 본에 의해서 IK가 제어될 때 가중치를 설정할 수 있으며, 이 가중치를 컨트롤 파라미터에 연동할 수도 있습니다.
- 본 생성 시 미러 복사가 가능하며, 포즈를 복사할 때 반전된 포즈로 붙여넣을 수 있습니다.
- 본 제어 함수 2종이 추가되었습니다.
- 애니메이션 제작시 자동으로 키프레임을 생성하는 Auto-Key 기능이 추가되었습니다. 
- Onion Skin이 개선되어 색상, 렌더링 방식, 렌더링 순서를 변경할 수 있으며, 애니메이션 작업시 연속된 프레임을 출력할 수 있습니다.
- Ctrl+Alt 키(OSX에서는 Command+Alt키)와 마우스 드래그로 화면을 이동하거나 확대/축소를 할 수 있습니다.
- 메시가 추가된 직후에는 Setting 탭으로 자동으로 전환됩니다.
- 화면 상단에 "메시 출력 여부"를 변경하는 버튼이 추가되었습니다.
- 메시 설정에서 양면 렌더링(2-Sides)을 설정할 수 있습니다.
- 새로운 버전의 AnyPortrait가 업데이트된 경우 첫 화면에서 알려줍니다.
- Ctrl키 (OSX에서는 Command키)를 누르면 사용자 설정이 가능한 버튼들의 색상이 바뀝니다.
- AnyPortrait의 타이틀 이미지가 Demo 폴더에 추가되었습니다.
- 1.0.4 버전의 새로운 기능들이 포함된 7번째 데모 씬이 추가되었습니다.
- 클리핑 마스크가 적용된 메시의 물리 모디파이어 설정시 버텍스 색상이 제대로 출력되지 않는 문제가 수정되었습니다.
- Rigging 정보가 없는 메시가 Bake 후에 제대로 처리가 안되는 문제가 수정되었습니다.
- Bake 다이얼로그에서 일부 텍스트가 번역이 안된 문제가 수정되었습니다.
- PSD 파일을 가져올 때 중첩된 메시 그룹을 생성한 경우 Depth가 이상하게 Bake되는 문제가 수정되었습니다.
- PSD 파일에서 4096 해상도의 아틀라스를 생성할 때 메시들이 이상하게 생성되는 문제가 수정되었습니다.
- 애니메이션을 실행할 때, Morph (Animation) 모디파이어가 제대로 처리되지 않는 문제가 수정되었습니다.

1.0.5 (2018년 6월 16일)
- Mac OSX에서 발생하는 apEditorUtil.cs의 스크립트 에러를 수정하였습니다.

1.0.6 (2018년 7월 14일)
- PSD 다시 가져오기 기능이 추가되었습니다.
- PSD 가져오기 기능시 생성되는 텍스쳐 에셋이 고화질이 되도록 설정됩니다.
- 상단 UI의 도구 그룹을 접을 수 있도록 변경되었습니다.
- 에디터 설정 다이얼로그에서 최신 버전을 확인하는 기능을 켜거나 끌 수 있습니다.
- 애니메이션 편집시 발생하는 스크립트 에러를 수정하였습니다.
- Rigging 값이 Bake 후 잘못 적용되는 문제를 수정하였습니다.
- 최신 버전을 확인할 때 발생하는 에러를 수정하였습니다.

1.0.7 (2018년 8월 6일)
- Rigging 가중치 값이 0이거나 Bone이 할당 안된경우, Bake가 실패하거나 에러가 발생되는 문제가 수정되었습니다.
- AnyPortrait의 DLL의 기본 설정값에서 iOS가 누락된 문제가 수정되었습니다.

1.1.0 (2018년 10월 7일)
- 메시 자동 생성 기능이 추가되었습니다.
- 메시 미러 툴이 추가되었습니다.
- 메시의 버텍스들을 편집할 수 있는 기능이 추가되었습니다.
- Perspective 카메라를 지원하며, 이를 위한 빌보드 옵션이 추가되었습니다.
- "Pirate Game"의 3D 버전인 "Pirate Game 3D" 데모 씬이 추가되었습니다.
- 애니메이션을 스크립트로 제어할 때, 애니메이션의 배속을 설정할 수 있도록 SetAnimationSpeed 함수가 추가되었습니다.
- 메시를 제작할 때, Ctrl 키(Mac OSX에서는 Command 키)를 누르면 가까운 버텍스로 커서가 스냅됩니다.
- 메시를 제작할 때, Shift 키를 누른 상태로 선분을 클릭하면 버텍스가 선분에 추가됩니다.
- 메시 제작 UI가 변경되었습니다.
- Bake 설정에서 그림자 설정(Receive Shadow, Cast Shadow)을 변경할 수 있습니다.
- Bake 다이얼로그의 UI가 변경되었습니다.
- Inspector UI가 변경되었습니다.
- Inspector에서 바로 에디터를 열 수 있으며, 바로 Bake를 할 수도 있습니다.
- 하위의 메시 그룹에 모디파이어와 본을 추가하고, 상위의 메시 그룹이 이를 제어할 수 있도록 개선되었습니다.
- Q&A 웹페이지를 여는 메뉴가 추가되었습니다.
- 메시를 생성할 때 폴리곤이 제대로 생성되지 않는 문제가 수정되었습니다.
- 낮은 배속이나 낮은 FPS로 설정된 애니메이션이 부드럽게 재생되지 않는 문제가 수정되었습니다.
- 애니메이션을 삭제할 때 Hierarchy가 갱신되지 않는 문제가 수정되었습니다.
- 게임 실행 시 Clipping Mask가 간헐적으로 동작하지 않는 문제가 수정되었습니다.
- 본을 연속으로 생성할 때, 첫번째 본의 IK 설정이 Disabled되는 문제가 수정되었습니다.
- 수동 백업 저장 시 간헐적으로 데이터가 누락되는 문제가 수정되었습니다.
- Inspector에서 컨트롤 파라미터를 제어할 때 발생하는 에러가 수정되었습니다.
- iOS 개발 환경에서 썸네일이 비정상적으로 출력되는 문제가 수정되었습니다.
- Mecanim을 사용하는 상태에서 Optimized Bake를 할 때 애니메이션 클립을 중복해서 생성하는 문제가 수정되었습니다.

1.1.1 (2018년 10월 11일)
- 본들을 연결하면 자식 본의 위치와 각도가 바뀌는 문제(v1.1.1)

1.1.2 (2018년 11월 8일)
- MP4 영상 내보내기 기능이 추가되었습니다. (Unity 2017.4부터 가능)
- GIF 애니메이션 품질을 4단계로 쉽게 설정할 수 있도록 변경하였습니다.
- GIF 애니메이션의 최고 품질은 기존보다 조금 더 향상되었습니다.
- 애니메이션 캡쳐 도중 중지할 수 있도록 UI가 변경되었습니다.
- Bake 다이얼로그에서 Lightweight Render Pipeline용 Shader 생성할 수 있습니다.
- Bake 다이얼로그에서 AnyPortrait에 맞게 Ambient Light를 검정색으로 변경하는 기능이 추가되었습니다.
- 애니메이션 재생에 관련된 스크립트 API에서 apPlayData를 활용할 수 있는 함수들이 추가되었습니다.
- 언어가 일본어로 설정된 경우, 메뉴를 선택할 때 일본어 홈페이지로 연결됩니다.
- Modifier에 오브젝트를 등록하는 모든 과정에서, 자동으로 편집 모드가 시작되도록 변경되었습니다.
- Perspective 카메라에서 렌더링하는 경우, 카메라 각도에 따라 Clipping Mask가 제대로 계산되지 않는 문제가 수정되었습니다.
- Rigging을 할 때, Blend 기능을 사용하면 가중치가 이상하게 적용되는 문제가 수정되었습니다.
- AnyPortrait의 리소스 경로를 인위적으로 수정하면 에러가 무한하게 발생하는 문제가 수정되었습니다.
- Bone의 기본 위치를 수정할 때, 클릭하자마자 마우스 위치로 Bone이 이동되는 문제가 수정되었습니다.

1.1.3 (2018년 11월 15일)
- 새로운 업데이트가 있을 경우 에셋 스토어로 바로 접속할 수 있는 기능이 추가되었습니다.
- 화면 캡쳐 속도와 품질이 향상되었습니다.
- 화면 캡쳐 해상도의 제한이 사라졌습니다.
- 화면 캡쳐를 할 때, 투명색이 제대로 적용되지 않는 문제가 수정되었습니다.
- 업데이트 로그 다이얼로그에서 일본어 홈페이지로 접속이 되지 않는 문제가 수정되었습니다.

1.1.4 (2018년 12월 18일)
- 렌더링 순서와 이미지를 실시간으로 변경할 수 있는 Extra 설정이 추가되었습니다.
- 드로우콜이 더욱 최적화되었습니다.
- Inspector UI에서 메시를 갱신하는 "Refresh Meshes" 버튼이 추가되었습니다.
- 매시의 재질을 제어하는 3종의 함수가 추가되었습니다.
- 상단 UI에서 불필요한 객체 정보 UI가 출력되지 않도록 변경되었습니다.
- 메시 그룹을 추가하는 다이얼로그에서 부모 메시 그룹이 나타나는 문제가 수정되었습니다.

1.1.5 (2018년 12월 24일)
- Unity 2018에서 메시의 기본 Depth를 수정할 수 없는 문제가 수정되었습니다.

1.1.6 (2019년 4월 19일)
- 유니티의 Timeline을 지원하여 시네마틱 장면 제작
- 노트북 과열 방지를 위한 에디터 성능 제한 기능 추가
- "편집 모드"가 켜질 때, "선택 잠금"이 같이 켜질지 여부 설정
- Scale이 음수의 값을 가져도 드로우콜이 증가되지 않도록 개선
- Important 옵션을 껐을 때 CPU가 더욱 최적화
- Hierarchy UI에서 항목의 순서를 변경하는 기능 추가됨
- 애니메이션을 특정 시점부터 재생하는 함수가 추가됨
- 일부의 optTransform의 Sorting Layer/Order를 변경하는 함수가 추가됨
- Animator의 Speed Multiplier 속성 적용
- Inspector UI의 디자인 개선
- 컨트롤 파라미터를 선택해서 모디파이어에 추가 가능
- Mecanim 설정의 "애니메이션 클립이 저장되는 경로"가 "상대 경로"로 변경
- 메시를 Physic 모디파이어에 추가할 때 "편집 모드"가 바로 켜지도록 변경
- 모디파이어에서 보간되는 Scale의 부호가 서로 다른 경우, Scale이 불연속적으로 보간되어 0이 되지 않도록 변경
- 모디파이어를 편집할 때, 컨트롤 파라미터의 값을 변경해도 "편집 모드"가 꺼지지 않도록 변경
- 애니메이션 이벤트를 복제할 수 있는 버튼 추가
- 자식인 메시 그룹이 애니메이션 클립에 연결될 때 경고 메시지 출력
- Ctrl + A를 눌러서 모든 버텍스 선택
- Ctrl + C, V를 눌러서 선택한 키프레임들을 복사
- 여러개의 키프레임들의 애니메이션 커브를 일괄 편집할 수 있도록 개선
- 애니메이션 커브를 타임라인 레이어에 관계없이 모든 키프레임에 적용하는 기능 추가
- Bake를 할 때, 씬의 Ambient Color의 설정을 자동으로 변경할 지 물어보는 다이얼로그 출력
- 본 설정 UI에 "Length" 항목 추가
- 일부 기능을 사용할 때 Console에 로그가 계속 출력되는 문제가 수정
- Hierarchy UI에서 발생하는 에러가 수정
- AnyPortrait 에디터를 유니티 에디터에 도킹한 상태에서 최대화를 켰다가 끄면 유니티가 멈추는 문제가 수정
- AnyPortrait 에디터를 연 상태에서, 씬을 전환하거나 유니티 에디터를 재시작할 때 발생하는 에러가 수정
- Once 타입의 애니메이션이 Animator에서 정상적으로 재생되지 않는 문제가 수정
- Extra Option이 본을 대상으로 하는 경우, 해당 Portrait를 더이상 열 수 없는 문제가 수정
- 모디파이어의 키를 복사할 때, Extra Option의 값이 누락되는 문제가 수정
- 컨트롤 파라미터의 동일한 값에 여러개의 키가 생성될 수 있는 문제가 수정
- Optimized Bake를 할 때, 애니메이션의 FPS가 정상적으로 적용되지 않는 문제가 수정
- Ctrl 키를 누르고 가까운 버텍스를 선택할 때, 버텍스가 마우스의 위치로 이동되는 문제가 수정
- "편집 모드"가 켜진 상태에서 컨트롤 파라미터의 값을 변경할 때 숨겨진 메시가 나타나는 문제가 수정
- 유효하지 않은 채널 정보를 가진 PSD 파일을 임포트할 수 없는 문제가 수정

1.1.7 (2019년 7월 10일)
- 재질과 쉐이더을 통합하여 관리하고 다양한 렌더링 기법을 적용할 수 있도록 "재질 라이브러리"와 "재질 세트"가 추가
- 모디파이어와 애니메이션 데이터를 최적화하여 프리팹의 파일 크기 감소
- CPU 부하가 적도록 "개선된 AsyncInitialize" 함수 추가
- 애니메이션 작업시 일부 추가, 삭제 과정에서의 처리 속도 향상
- Ctrl+C,V 키를 이용하여 다른 애니메이션 클립으로 키프레임 복사 가능
- 메시 그룹에 동시에 여러개의 메시, 메시 그룹을 선택하여 추가할 수 있도록 개선
- 애니메이션이나 모디파이어 선택시 자동으로 "Controller 탭"이 열릴지 여부를 사용자가 설정 가능
- 작업 도중에 실행 취소나 키값 변경 등의 이유로 메시의 "임시의 렌더링 여부"가 해제될지 여부를 사용자가 설정 가능
- 메시의 "임시의 렌더링 여부"를 리셋하는 버튼 추가
- Ambient Color 보정 다이얼로그에서 "Do Not Show this message"가 반대로 동작되는 문제 수정
- 애니메이션 키프레임을 Ctrl+C,V 키로 복사할 때, 기존 키프레임을 덮어쓰지 못하는 문제 수정
- PSD 파일을 가져올 때 레이어의 클리핑 설정이 적용안되던 문제 수정
- 양면 메시의 그림자와 노멀 벡터가 비정상적으로 계산되는 문제 수정
- 애니메이션의 Start Frame과 End Frame이 같은 경우 에디터에서 열리지 않는 문제 수정

1.1.8 (2019년 9월 8일)
- 리깅 모디파이어 개선
- 가중치를 수정하는 UI 추가
- 가중치 편집을 제한하는 "잠금 기능" 추가
- "브러시 모드" 추가
- 현재 본에 리깅된 버텍스들을 일괄 선택하는 기능 추가
- "자동 리깅(Auto-Rig)" 기능 대폭 향상
- 원형 그래프 방식으로 버텍스를 출력하는 기능 추가
- Bake 다이얼로그에 SRP에서 클리핑 메시를 렌더링하기 위한 옵션이 추가
- 재질 라이브러리에 LWRP 2D를 지원하는 패키지 추가
- 설정 다이얼로그에 새로 추가되는 본의 색상이 부모의 색상과 유사할지 여부에 관한 옵션 추가
- 본을 자식의 본으로 스냅하는 기능 추가
- 본을 복제하는 기능 추가
- LookAt 방식의 IK 컨트롤러가 단일 본 또는 IK 체인이 설정되지 않은 본에도 적용이 되도록 변경
- 좌우의 UI를 숨기는 버튼이 추가되었으며, 오른쪽 UI를 상하로 접는 버튼의 디자인이 변경
- Sorting Group 지원
- Sorting Order를 설정하는 옵션이 Bake 다이얼로그와 Inspector UI에 추가
- Sorting Order를 설정하는 옵션에 관련된 "SetSortingOrderChangedAutomatically" 함수 추가
- 애니메이션 클립의 대상의 메시 그룹을 변경할 때, 해당 메시 그룹의 부모 메시 그룹을 선택하면 애니메이션 데이터를 유지하는 기능 추가
- 애니메이션 복제시, 모디파이어의 Extra옵션과 애니메이션 이벤트가 복제되지 않는 버그 수정

1.2.0 (2019년 10월 28일)
- VR 지원
- 카메라의 Target Texture 속성 지원
- Bake 실행 시에는 빌보드 옵션이 적용되지 않고, 게임 실행 중에만 적용되도록 변경
- 메카님을 위한 애니메이션 클립 저장 경로에 공백이 있을 경우 제대로 저장되지 않는 버그 수정
- FFD의 크기를 임의로 수정할 때, 크기가 2인 경우 제대로 동작하지 않는 버그 수정
- 본을 Detach한 직후, 해당 본을 제어할 수 없는 버그 수정
- SetMeshImageAll, SetMeshColorAll과 같은 메시들을 일괄 제어하는 함수들이 정상적으로 동작하지 않는 버그 수정 (Bake를 1회 해야 적용됨)

1.2.1 (2019년 11월 25일)
- AnyPortrait 에디터의 성능이 일부 향상
- 애니메이션 타임라인 UI의 자동 스크롤 기능이 키프레임을 추가할 때, 오브젝트를 선택하고 편집할 때에도 동작하도록 개선
- 애니메이션 타임라인 UI의 키보드 단축키 입력처리가 개선
- AnyPortrait 에디터의 FPS 출력 방식이 개선
- 모디파이어의 Extra 옵션으로 자식 메시 그룹의 Depth를 변경하는 경우 Bake가 되지 않는 버그 수정
- 화면 캡쳐시 Extra 옵션에 의해 변경된 렌더링 순서가 적용되지 않는 문제 수정
- 애니메이션 타임라인 UI의 자동 스크롤 기능이 정상적으로 동작하지 않는 버그 수정
- 오브젝트 리스트 UI의 "작업을 위한 일시적 Show/Hide" 버튼이 잘못 출력되는 버그 수정

