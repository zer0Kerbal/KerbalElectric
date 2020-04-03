using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace ModuleKELights
{
    class ModuleKETestFont : PartModule
    {
        Color lastColor;
        float lastFontSize = 0;

        [KSPField(isPersistant = true)]
        Color currentColor;

        TextMeshPro LCDTextMesh = null;

        [KSPField]
        public string screenText = "None";

        [KSPField]
        public string ScreenName;

        TMP_FontAsset[] loadedFonts;

        bool autoFont = true;

        [KSPField(guiName = "#autoLOC_6001402", isPersistant = true, guiActive = true, guiActiveEditor = true), UI_FloatRange(minValue = 0f, maxValue = 1f, stepIncrement = 0.05f)]
        public float fontColorR = 1f;

        [KSPField(guiName = "#autoLOC_6001403", isPersistant = true, guiActive = true, guiActiveEditor = true), UI_FloatRange(minValue = 0f, maxValue = 1f, stepIncrement = 0.05f)]
        public float fontColorG = 1f;

        [KSPField(guiName = "#autoLOC_6001404", isPersistant = true, guiActive = true, guiActiveEditor = true), UI_FloatRange(minValue = 0f, maxValue = 1f, stepIncrement = 0.05f)]
        public float fontColorB = 1f;

        [KSPField(guiName = "Font Size", isPersistant = true, guiActive = true, guiActiveEditor = true), UI_FloatRange(minValue = 0.1f, maxValue = 10f, stepIncrement = 0.05f)]
        public float fontSize = 1f;

        [KSPField(guiActiveEditor = true, guiActive = true, guiName = "Current Font")]
        public string fontName = string.Empty;

        [KSPField(guiActiveEditor = true, guiActive = true, guiName = "Current Font Index", isPersistant = true)]
        public int loadedFontIndex = 0;

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiActiveUnfocused = false, guiName = "Next Font")]
        public void nextFont()
        {
            loadedFontIndex++;
            if (loadedFontIndex > loadedFonts.Length - 1)
            {
                loadedFontIndex = 0;
            }
            SetFontName(loadedFontIndex);
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiActiveUnfocused = false, guiName = "Prev Font")]
        public void previousFont()
        {
            loadedFontIndex--;
            if (loadedFontIndex < 0)
            {
                loadedFontIndex = loadedFonts.Length - 1;
            }
            SetFontName(loadedFontIndex);
        }

        public override void OnCopy(PartModule fromModule)
        {
            base.OnCopy(fromModule);
            TextMeshPro thisMesh = GetComponentInChildren<TextMeshPro>();
            if (thisMesh != null)
            {
                thisMesh.gameObject.DestroyGameObject();
            }
        }

        public void ShowText()
        {
            GameObject LCDScreen = new GameObject();
            Transform screenTransform = this.part.FindModelTransform(ScreenName);
            LCDScreen.transform.parent = screenTransform;
            LCDScreen.transform.localRotation = screenTransform.localRotation;
            LCDScreen.transform.localRotation = Quaternion.Euler(0, 180f, 0);
            LCDTextMesh = LCDScreen.AddComponent<TextMeshPro>();

            Mesh M = screenTransform.GetComponent<MeshFilter>().mesh;
            RectTransform T = LCDTextMesh.gameObject.GetComponent<RectTransform>();
            T.sizeDelta = new Vector2(M.bounds.size.x, M.bounds.size.y);
            LCDScreen.transform.localPosition = new Vector3(0, 0, (M.bounds.size.z / 2) + 0.01f);

            Debug.Log("TM Created " +T.sizeDelta.x + " " + T.sizeDelta.y);

            LCDTextMesh.enableAutoSizing = autoFont;
            LCDTextMesh.fontSizeMin = 0.1f;
            LCDTextMesh.overflowMode = TextOverflowModes.Truncate;
            LCDTextMesh.alignment = TextAlignmentOptions.Center;

            LCDTextMesh.text = screenText;
        }

        public void SetFontName(int thisFontIndex)
        {
            fontName = loadedFonts[loadedFontIndex].name;
            LCDTextMesh.font = loadedFonts[thisFontIndex];
        }

        public void SetFontColor(Color thisColor)
        {
            LCDTextMesh.color = thisColor;
            lastColor = thisColor;
        }

        public void SetFontSize(float thisFontSize)
        {
            LCDTextMesh.fontSize = thisFontSize;
            lastFontSize = thisFontSize;
        }

        public override void OnAwake()
        {
            loadedFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            UnityEngine.Object[] fonts = UnityEngine.Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Font));
        }

        public override void OnStart(StartState state)
        {
            ShowText();
        }

        public void FixedUpdate()
        {
            if (LCDTextMesh == null)
            {
                return;
            }
            currentColor = new Color(fontColorR, fontColorG, fontColorB, 1);
            if (currentColor != lastColor)
            {
                SetFontColor(currentColor);
            }
            if (!autoFont && fontSize != lastFontSize)
            {
                SetFontSize(fontSize);
            }
        }
    }
}
