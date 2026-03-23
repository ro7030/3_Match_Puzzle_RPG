#if UNITY_EDITOR
using System.Collections.Generic;
using Match3Puzzle.Skill;
using Story;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Match3Puzzle.Skill.Editor
{
    /// <summary>
    /// SkillScene를 통째로 재구성합니다.
    /// 목표: 이미지 교체만으로 화면을 완성할 수 있는 고정 레이아웃 생성.
    /// </summary>
    public static class SkillSceneRebuildTool
    {
        private const string BackgroundPath = "Assets/Resources/Image/UI/Skill_Scene/Skill_Scene.jpg";
        private const string HighlightPath = "Assets/Resources/Image/UI/Skill_Scene/Highlight.png";
        private const string BottomMaskPath = "Assets/Resources/Image/UI/Skill_Scene/Black_Image_Down.png";
        private const string SkillScenePath = "Assets/Scenes/SkillScene.unity";
        private const string CharacterDbPath = "Assets/Resources/ScriptableObjects/Story/CharacterDatabase.asset";
        private const string SkillDbPath = "Assets/Resources/SkillDatabase.asset";

        [MenuItem("MATCH3/Skill Scene/씬 전체 재구성 (이미지 교체형)")]
        public static void RebuildScene()
        {
            var scene = EditorSceneManager.OpenScene(SkillScenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                EditorUtility.DisplayDialog("Skill Scene", "활성 씬이 없습니다.", "확인");
                return;
            }

            bool ok = Application.isBatchMode || EditorUtility.DisplayDialog(
                "Skill Scene 전체 재구성",
                "현재 씬의 루트 오브젝트를 모두 삭제하고 SkillScene 레이아웃을 다시 만듭니다.\n\n계속할까요?",
                "재구성",
                "취소");
            if (!ok) return;

            ClearRootObjects(scene);

            var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
            var highlightSprite = AssetDatabase.LoadAssetAtPath<Sprite>(HighlightPath);
            var bottomMaskSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BottomMaskPath);
            var characterDb = AssetDatabase.LoadAssetAtPath<CharacterDatabase>(CharacterDbPath);
            var skillDb = AssetDatabase.LoadAssetAtPath<SkillDatabase>(SkillDbPath);

            // 필수 루트
            CreateMainCamera();
            CreateEventSystem();

            // Canvas
            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            var canvasRt = canvasGo.GetComponent<RectTransform>();
            StretchFull(canvasRt);

            // 배경
            var background = CreateImage("Background", canvasRt, bgSprite, true);
            StretchFull(background.rectTransform);
            background.raycastTarget = false;

            // 상단바
            var topBar = CreateRect("TopBar", canvasRt);
            SetAnchors(topBar, new Vector2(0f, 0.86f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f));

            var backButton = CreateButton("BackButton", topBar, new Vector2(70f, 70f), new Vector2(70f, -50f));
            backButton.GetComponentInChildren<TextMeshProUGUI>().text = "<";
            var settingsButton = CreateButton("SettingsButton", topBar, new Vector2(70f, 70f), new Vector2(-70f, -50f), anchorMin: new Vector2(1f, 1f), anchorMax: new Vector2(1f, 1f));
            settingsButton.GetComponentInChildren<TextMeshProUGUI>().text = "O";

            // 캐릭터 4인 그리드
            var grid = CreateRect("CharacterSkillGrid", canvasRt);
            SetAnchors(grid, new Vector2(0.03f, 0.28f), new Vector2(0.97f, 0.84f), Vector2.zero, Vector2.zero);
            var hlg = grid.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 28f;
            hlg.padding = new RectOffset(16, 16, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;

            var slotUis = new List<CharacterSkillSlotUI>(4);
            for (int i = 0; i < 4; i++)
            {
                var slotUi = CreateCharacterSlot(i, grid, highlightSprite, bottomMaskSprite);
                slotUis.Add(slotUi);
            }

            // 스킬 선택 패널 (하단 아이콘 행)
            var skillSelectionPanel = CreateRect("SkillSelectionPanel", canvasRt);
            SetAnchors(skillSelectionPanel, new Vector2(0.12f, 0.10f), new Vector2(0.88f, 0.26f), Vector2.zero, Vector2.zero);
            skillSelectionPanel.gameObject.SetActive(false);

            var skillSlotGroups = new List<GameObject>(4);
            var firstGroupSlots = new List<SkillSelectionSlotUI>(3);
            for (int g = 0; g < 4; g++)
            {
                var slotsRoot = CreateRect($"SkillSlots{g}", skillSelectionPanel);
                StretchFull(slotsRoot);
                slotsRoot.gameObject.SetActive(false);
                skillSlotGroups.Add(slotsRoot.gameObject);

                var slotsLayout = slotsRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
                slotsLayout.spacing = 24f;
                slotsLayout.padding = new RectOffset(24, 24, 0, 0);
                slotsLayout.childAlignment = TextAnchor.MiddleCenter;
                slotsLayout.childControlWidth = true;
                slotsLayout.childControlHeight = true;
                slotsLayout.childForceExpandWidth = true;
                slotsLayout.childForceExpandHeight = true;

                for (int i = 0; i < 3; i++)
                {
                    var ss = CreateSkillSelectionSlot(i, slotsRoot);
                    if (g == 0)
                        firstGroupSlots.Add(ss);
                }
            }

            // 하단 스킬 정보
            var skillInfo = CreateRect("SkillInfo", canvasRt);
            SetAnchors(skillInfo, new Vector2(0.30f, 0.02f), new Vector2(0.70f, 0.09f), Vector2.zero, Vector2.zero);
            var skillIcon = CreateImage("SkillIconImage", skillInfo, null, true);
            SetAnchors(skillIcon.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(52f, 0f));
            skillIcon.enabled = false;

            var skillName = CreateTMP("SkillNameText", skillInfo, "스킬 이름", 42);
            SetAnchors(skillName.rectTransform, new Vector2(0f, 0.52f), new Vector2(1f, 1f), new Vector2(60f, 0f), new Vector2(0f, 0f));
            skillName.alignment = TextAlignmentOptions.Left;
            skillName.color = Color.white;

            var skillEffect = CreateTMP("SkillEffectText", skillInfo, "", 30);
            SetAnchors(skillEffect.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0.48f), new Vector2(60f, 0f), new Vector2(0f, 0f));
            skillEffect.alignment = TextAlignmentOptions.Left;
            skillEffect.color = Color.white;

            // 컨트롤러
            var controllerGo = new GameObject("SkillController", typeof(RectTransform), typeof(SkillSceneController));
            controllerGo.transform.SetParent(canvasRt, false);
            var controller = controllerGo.GetComponent<SkillSceneController>();
            var so = new SerializedObject(controller);
            so.FindProperty("characterDatabase").objectReferenceValue = characterDb;
            so.FindProperty("skillDatabase").objectReferenceValue = skillDb;
            so.FindProperty("skillSelectionPanel").objectReferenceValue = skillSelectionPanel.gameObject;
            so.FindProperty("skillInfoIconImage").objectReferenceValue = skillIcon;
            so.FindProperty("skillNameText").objectReferenceValue = skillName;
            so.FindProperty("skillEffectText").objectReferenceValue = skillEffect;
            so.FindProperty("backButton").objectReferenceValue = backButton;

            var idsProp = so.FindProperty("characterIds");
            idsProp.arraySize = 4;
            for (int i = 0; i < 4; i++)
                idsProp.GetArrayElementAtIndex(i).intValue = i;

            var charSlotsProp = so.FindProperty("characterSlots");
            charSlotsProp.arraySize = 4;
            for (int i = 0; i < 4; i++)
                charSlotsProp.GetArrayElementAtIndex(i).objectReferenceValue = slotUis[i];

            var selectionProp = so.FindProperty("skillSelectionSlots");
            selectionProp.arraySize = 3;
            for (int i = 0; i < 3; i++)
                selectionProp.GetArrayElementAtIndex(i).objectReferenceValue = firstGroupSlots[i];

            var groupsProp = so.FindProperty("skillSlotGroups");
            groupsProp.arraySize = 4;
            for (int i = 0; i < 4; i++)
                groupsProp.GetArrayElementAtIndex(i).objectReferenceValue = skillSlotGroups[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(
                    "Skill Scene",
                    "씬 재구성이 완료되었습니다.\n\n" +
                    "이제 각 Image의 Sprite만 바꾸면 됩니다.\n" +
                    "- Slot_0~3/Highlight\n" +
                    "- Slot_0~3/HoverRoot/Portrait\n" +
                    "- Slot_0~3/HoverRoot/BottomMask\n" +
                    "- SkillSlots0~3/Slot_0~2 아이콘",
                    "확인");
            }
        }

        private static CharacterSkillSlotUI CreateCharacterSlot(int index, RectTransform parent, Sprite highlightSprite, Sprite bottomMaskSprite)
        {
            var slot = CreateRect($"Slot_{index}", parent);
            var le = slot.gameObject.AddComponent<LayoutElement>();
            le.minWidth = 220f;
            le.preferredWidth = 260f;
            le.flexibleWidth = 1f;
            le.minHeight = 480f;
            le.preferredHeight = 560f;

            var highlight = CreateImage("Highlight", slot, highlightSprite, true);
            StretchFull(highlight.rectTransform);
            highlight.raycastTarget = false;
            highlight.gameObject.SetActive(false);

            var hoverRoot = CreateRect("HoverRoot", slot);
            StretchFull(hoverRoot);

            var portrait = CreateImage("Portrait", hoverRoot, null, true);
            SetAnchors(portrait.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 60f), new Vector2(0f, 0f));
            portrait.preserveAspect = true;
            portrait.raycastTarget = true;

            var bottomMask = CreateImage("BottomMask", hoverRoot, bottomMaskSprite, true);
            SetAnchors(bottomMask.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 160f));
            bottomMask.preserveAspect = true;
            bottomMask.raycastTarget = false;

            var skillButton = CreateButton("SkillIconButton", slot, new Vector2(130f, 130f), new Vector2(0f, 34f));
            var skillButtonImg = skillButton.GetComponent<Image>();
            skillButtonImg.raycastTarget = false;
            var label = skillButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) Object.DestroyImmediate(label.gameObject);

            var icon = CreateImage("Icon", skillButton.GetComponent<RectTransform>(), null, true);
            StretchFull(icon.rectTransform);
            icon.raycastTarget = false;

            var slotUi = slot.gameObject.AddComponent<CharacterSkillSlotUI>();
            var so = new SerializedObject(slotUi);
            so.FindProperty("portraitImage").objectReferenceValue = portrait;
            so.FindProperty("hoverTarget").objectReferenceValue = hoverRoot;
            so.FindProperty("glowBackground").objectReferenceValue = highlight.gameObject;
            so.FindProperty("skillEquippedRoot").objectReferenceValue = skillButton.gameObject;
            so.FindProperty("skillIconButton").objectReferenceValue = skillButton;
            so.FindProperty("skillIconImage").objectReferenceValue = icon;
            so.ApplyModifiedPropertiesWithoutUndo();

            return slotUi;
        }

        private static SkillSelectionSlotUI CreateSkillSelectionSlot(int index, RectTransform parent)
        {
            var slotButton = CreateButton($"Slot_{index}", parent, new Vector2(170f, 170f), Vector2.zero);
            var le = slotButton.gameObject.AddComponent<LayoutElement>();
            le.minWidth = 170f;
            le.minHeight = 170f;
            le.preferredWidth = 180f;
            le.preferredHeight = 180f;

            var label = slotButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) Object.DestroyImmediate(label.gameObject);

            var icon = CreateImage("Icon", slotButton.GetComponent<RectTransform>(), null, true);
            StretchFull(icon.rectTransform);

            var ui = slotButton.gameObject.AddComponent<SkillSelectionSlotUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("button").objectReferenceValue = slotButton;
            so.FindProperty("iconImage").objectReferenceValue = icon;
            so.ApplyModifiedPropertiesWithoutUndo();
            return ui;
        }

        private static void ClearRootObjects(UnityEngine.SceneManagement.Scene scene)
        {
            var roots = scene.GetRootGameObjects();
            foreach (var go in roots)
                Object.DestroyImmediate(go);
        }

        private static void CreateMainCamera()
        {
            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.orthographic = true;
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }

        private static void CreateEventSystem()
        {
            var esGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            esGo.transform.position = Vector3.zero;
        }

        private static RectTransform CreateRect(string name, RectTransform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            return rt;
        }

        private static Image CreateImage(string name, RectTransform parent, Sprite sprite, bool preserveAspect)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = preserveAspect;
            image.color = Color.white;
            return image;
        }

        private static Button CreateButton(
            string name,
            RectTransform parent,
            Vector2 size,
            Vector2 anchoredPos,
            Vector2? anchorMin = null,
            Vector2? anchorMax = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin ?? new Vector2(0.5f, 0.5f);
            rt.anchorMax = anchorMax ?? new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            var image = go.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.15f);

            var text = CreateTMP("Label", rt, "", 30);
            StretchFull(text.rectTransform);
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            return go.GetComponent<Button>();
        }

        private static TextMeshProUGUI CreateTMP(string name, RectTransform parent, string text, float fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            return tmp;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        private static void SetAnchors(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            rt.localScale = Vector3.one;
        }
    }
}
#endif
