using System;
using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Reflection;
using ModuleKELights;

//namespace ModuleKEFont
//{
//    [KSPAddon(KSPAddon.Startup.Instantly, true)]
//    public class CustomFontLoader : MonoBehaviour
//    {
//        private static readonly string AssemblyPath = Assembly.GetExecutingAssembly().Location;
//        private static readonly string AssemblyDirectory = Path.GetDirectoryName(AssemblyPath);
//        private static readonly string ConfigPath = Path.Combine(AssemblyDirectory, "fontList.dat");

//        void Awake()
//        {
//            FontLoader fontLoader = GameObject.FindObjectOfType<FontLoader>();
//            Debug.Log("Loading KEFonts 1");

//            if (File.Exists(ConfigPath))
//            {
//                string[] fontArray = File.ReadAllLines(ConfigPath);
//                foreach (string fontName in fontArray)
//                {
//                    TMP_FontAsset font = fontLoader.LoadedFonts.Find(t => t.name.Equals(fontName));
//                    fontLoader.AddGameSubFont("en-us", false, font);
//                    font = Resources.Load("Fonts & Materials/" + fontName, typeof(TMP_FontAsset)) as TMP_FontAsset;

//                }
//            }
//        }
//    }
//}

namespace ModuleKELight
{
    class ModuleKELCD : PartModule
    {

        List<string> resourceName = new List<string>();

        bool autoFont = true;

        TextMeshPro LCDTextMesh = null;

        Color lastColor;
        float lastFontSize = 0;

        TMP_FontAsset[] loadedFonts;

        Animation gAnim;
        Animation rAnim;
        Animation yAnim;

        [KSPField]
        public string GAnimName;

        [KSPField]
        public string YAnimName;

        [KSPField]
        public string RAnimName;

        [KSPField]
        public int DefaultFont = -1;

        [KSPField(isPersistant = true)]
        private int resourceScope = 0;

        [KSPField(isPersistant = true)]
        private int loadedResourceIndex = 0;

        [KSPField(isPersistant = true)]
        Color currentColor;

        [KSPField]
        public string ScreenName;

        [KSPField(guiName = "#autoLOC_6001402", isPersistant = true, guiActive = true, guiActiveEditor = true), UI_FloatRange(minValue = 0f, maxValue = 1f, stepIncrement = 0.05f)]
        public float fontColorR = 1f;

        [KSPField(guiName = "#autoLOC_6001403", isPersistant = true, guiActive = true, guiActiveEditor = true), UI_FloatRange(minValue = 0f, maxValue = 1f, stepIncrement = 0.05f)]
        public float fontColorG = 1f;

        [KSPField(guiName = "#autoLOC_6001404", isPersistant = true, guiActive = true, guiActiveEditor = true), UI_FloatRange(minValue = 0f, maxValue = 1f, stepIncrement = 0.05f)]
        public float fontColorB = 1f;

        [KSPField(guiName = "Font Size", isPersistant = true, guiActive = true, guiActiveEditor = true), UI_FloatRange(minValue = 0.1f, maxValue = 10f, stepIncrement = 0.05f)]
        public float fontSize = 1f;

        [KSPField(guiActiveEditor = true, guiActive = true, guiName = "Resource Scope")]
        public string resourceScopeText = string.Empty;

        [KSPField(guiActiveEditor = true, guiActive = true, guiName = "Current Font")]
        public string fontName = string.Empty;

        [KSPField(guiActiveEditor = true, guiActive = true, guiName = "Current Font Index", isPersistant = true)]
        public int loadedFontIndex = 0;

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiActiveUnfocused = false, guiName = "Toggle Resource Scope")]
        public void ToggleResourceScope()
        {
            if (resourceScope == 0)
            {
                resourceScope = 1;
            }
            else
            {
                resourceScope = 0;
            }
            SetResourceScope(resourceScope);
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiActiveUnfocused = false, guiName = "Next Resource")]
        public void nextResource()
        {
            loadedResourceIndex++;
            if (loadedResourceIndex > resourceName.Count - 1)
            {
                loadedResourceIndex = 0;
            }
            SetText(resourceName[loadedResourceIndex]);
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiActiveUnfocused = false, guiName = "Prev Resource")]
        public void prevResource()
        {
            loadedResourceIndex--;
            if (loadedResourceIndex < 0)
            {
                loadedResourceIndex = resourceName.Count - 1;
            }
            SetText(resourceName[loadedResourceIndex]);
        }


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

            LCDTextMesh.enableAutoSizing = autoFont;
            LCDTextMesh.fontSizeMin = 0.1f;
            LCDTextMesh.overflowMode = TextOverflowModes.Truncate;

            LCDTextMesh.alignment = TextAlignmentOptions.Center;
        }

        public void SetResourceScope(int scope)
        {
            if (resourceScope == 0)
            {
                resourceScopeText = "Part";
            }
            else
            {
                resourceScopeText = "Ship";
            }
        }

        private void GetAnims()
        {
            List<Animation> list = new List<Animation>(base.part.FindModelComponents<Animation>());
            int i = 0;
            int count = list.Count;
            while (i < count)
            {
                Animation animation = list[i];
                if (animation[GAnimName] != null)
                {
                    gAnim = animation;
                    gAnim[GAnimName].wrapMode = WrapMode.ClampForever;
                    gAnim[GAnimName].normalizedSpeed = 0f;
                    gAnim[GAnimName].normalizedTime = 0f;
                    gAnim.Play(GAnimName);
                }
                if (animation[YAnimName] != null)
                {
                    yAnim = animation;
                    yAnim[YAnimName].wrapMode = WrapMode.ClampForever;
                    yAnim[YAnimName].normalizedSpeed = 0f;
                    yAnim[YAnimName].normalizedTime = 0f;
                    yAnim.Play(YAnimName);
                }
                if (animation[RAnimName] != null)
                {
                    rAnim = animation;
                    rAnim[RAnimName].wrapMode = WrapMode.ClampForever;
                    rAnim[RAnimName].normalizedSpeed = 0f;
                    rAnim[RAnimName].normalizedTime = 0f;
                    rAnim.Play(RAnimName);
                }
                i++;
            }
         }

        public void SetText(string thisText)
        {
            LCDTextMesh.text = thisText;
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

        private void PlayAnimation(Animation thisAnim, string thisAnimName, float speed)
        {
            thisAnim[thisAnimName].normalizedSpeed = speed;
            thisAnim.Play();
        }

        public override void OnActive()
        {
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

        public void GetResourceNames()
        {
            int resourceCount = PartResourceLibrary.Instance.resourceDefinitions.Count;

            foreach (PartResourceDefinition thisResource in PartResourceLibrary.Instance.resourceDefinitions)
            {
                resourceName.Add(thisResource.name);
            }
            resourceName.Sort();
        }

        public override void OnAwake()
        {
            loadedFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            UnityEngine.Object[] fonts = UnityEngine.Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Font));
            GetResourceNames();
            //foreach (TMP_FontAsset fontlist in loadedFonts)
            //{
            //    Debug.Log("Font: " + fontlist.name);
            //}
        }

        public override void OnStart(StartState state)
        {
            if (autoFont)
            {
                Fields["fontSize"].guiActive = false;
                Fields["fontSize"].guiActiveEditor = false;
            }
            else
            {
                SetFontSize(fontSize);
            }
            if (LCDTextMesh == null)
            {
                ShowText();
            }
            if (LCDTextMesh == null)
            {
                Debug.Log("Error: Unable to create the text mesh!");
                return;
            }
            currentColor = new Color(fontColorR, fontColorG, fontColorB, 1);
            if (DefaultFont > -1 && DefaultFont < loadedFonts.Length - 1)
            {
                loadedFontIndex = DefaultFont;
            }
            GetAnims();
            SetFontColor(currentColor);
            SetFontName(loadedFontIndex);
            SetText(resourceName[loadedResourceIndex]);
            SetResourceScope(resourceScope);
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
            if (rAnim.isPlaying)
            {
                if (rAnim[RAnimName].normalizedTime > 1)
                {
                    rAnim.Stop();
                    rAnim[RAnimName].normalizedTime = 1;
                }
                if (rAnim[RAnimName].normalizedTime < 0)
                {
                    rAnim.Stop();
                    rAnim[RAnimName].normalizedTime = 0;
                }
            }
            if (yAnim.isPlaying)
            {
                if (yAnim[YAnimName].normalizedTime > 1)
                {
                    yAnim.Stop();
                    yAnim[YAnimName].normalizedTime = 1;
                }
                if (yAnim[YAnimName].normalizedTime < 0)
                {
                    yAnim.Stop();
                    yAnim[YAnimName].normalizedTime = 0;
                }
            }
            if (gAnim.isPlaying)
            {
                if (gAnim[GAnimName].normalizedTime > 1)
                {
                    gAnim.Stop();
                    gAnim[GAnimName].normalizedTime = 1;
                }
                if (gAnim[GAnimName].normalizedTime < 0)
                {
                    gAnim.Stop();
                    gAnim[GAnimName].normalizedTime = 0;
                }
            }
            if (this.part.parent)
            {
                double percentage = 0;
                double max = 0;
                switch (resourceScope)
                {                 
                    case 0: //part
                        max = KEFunctions.GetPartResourceMax(this.part.parent, resourceName[loadedResourceIndex]);
                        if (max == 0)
                        {
                            percentage = 0;
                        }
                        else
                        {
                            percentage = KEFunctions.GetPartResourceAmount(this.part.parent, resourceName[loadedResourceIndex]) / max;
                        }
                        break;
                    case 1: //ship
                        if (HighLogic.LoadedSceneIsEditor)
                        {
                            percentage = 0;
                        }
                        else
                        {
                            max = KEFunctions.GetVesselResourceMax(this.part.vessel, resourceName[loadedResourceIndex]);
                            if (max == 0)
                            {
                                percentage = 0;
                            }
                            else
                            {
                                percentage = KEFunctions.GetVesselResourceAmount(this.part.vessel, resourceName[loadedResourceIndex]) / max;
                            }
                        }
                        break;
                }
                if (percentage == 0.0f)
                {
                    if (rAnim[RAnimName].normalizedTime == 1)
                    {
                        PlayAnimation(rAnim, RAnimName, -1.0f);
                    }
                    if (yAnim[YAnimName].normalizedTime == 1)
                    {
                        PlayAnimation(yAnim, YAnimName, -1.0f);
                    }
                    if (gAnim[GAnimName].normalizedTime == 1)
                    {
                        PlayAnimation(gAnim, GAnimName, -1.0f);
                    }
                }
                else if (percentage < 0.25f)
                {
                    if (rAnim[RAnimName].normalizedTime == 0)
                    {
                        PlayAnimation(rAnim, RAnimName, 1.0f);
                    }
                    if (yAnim[YAnimName].normalizedTime == 1)
                    {
                        PlayAnimation(yAnim, YAnimName, -1.0f);
                    }
                    if (gAnim[GAnimName].normalizedTime == 1)
                    {
                        PlayAnimation(gAnim, GAnimName, -1.0f);
                    }
                }
                else if (percentage < 0.5f)
                {
                    if (rAnim[RAnimName].normalizedTime == 1)
                    {
                        PlayAnimation(rAnim, RAnimName, -1.0f);
                    }
                    if (yAnim[YAnimName].normalizedTime == 0)
                    {
                        PlayAnimation(yAnim, YAnimName, 1.0f);
                    }
                    if (gAnim[GAnimName].normalizedTime == 1)
                    {
                        PlayAnimation(gAnim, GAnimName, -1.0f);
                    }
                }
                else
                {
                    if (rAnim[RAnimName].normalizedTime == 1)
                    {
                        PlayAnimation(rAnim, RAnimName, -1.0f);
                    }
                    if (yAnim[YAnimName].normalizedTime == 1)
                    {
                        PlayAnimation(yAnim, YAnimName, -1.0f);
                    }
                    if (gAnim[GAnimName].normalizedTime == 0)
                    {
                        PlayAnimation(gAnim, GAnimName, 1.0f);
                    }
                }
            }
        }
    }
}
