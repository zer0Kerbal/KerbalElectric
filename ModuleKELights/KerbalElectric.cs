using KSP.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ModuleKELight
{

    public class ModuleKELight : PartModule, IResourceConsumer
    {
        #region Vars

        private bool dodebug = true;
        private bool blinkOn = false;
        private float blinkOnTime = 0.0f;
        private float blinkOffTime = 0.0f;

        private AnimationState partAnimState;
        public Animation partAnimation = null; //the animation
        public Animation panAnimation = null;
        public Animation tiltAnimation = null;
        public bool animationFound = false;

        Light[] myLights = null;

        private static readonly string AssemblyPath = Assembly.GetExecutingAssembly().Location;
        private static readonly string AssemblyDirectory = Path.GetDirectoryName(AssemblyPath);
        private static readonly string ConfigPath = Path.Combine(AssemblyDirectory, "presets.dat");

        private List<string> lightColorList = new List<string>();

        private Color currentColor;
        private Color lastColor;
        private Color offColor = new Color(0, 0, 0, 1);

        private float resourceFraction;

        Renderer[] rend;
        Material mats;

        private List<PartResourceDefinition> consumedResources;

        #endregion

        #region Fields        
        [KSPField]
        public bool isSlime = false;

        [KSPField]
        public string slimeLensOn;

        [KSPField]
        public string slimeLensOff;

        [KSPField]
        public bool useResources;

        [KSPField]
        public string resourceName = "ElectricCharge";

        [KSPField]
        public string animationName = "LightAnimation";

        [KSPField]
        public string panAnimationName;

        [KSPField]
        public string tiltAnimationName;

        [KSPField(isPersistant = true)]
        public float panTime = 0.0f;

        [KSPField(isPersistant = true)]
        public float tiltTime = 0.0f;

        [KSPField]
        public float resourceAmount = 0.01f;

        [KSPField]
        public bool useAnimationDim = false;

        [KSPField]
        public bool useAutoDim = false;

        [KSPField]
        public float lightBrightenSpeed = 0.3f;

        [KSPField]
        public float lightDimSpeed = 0.8f;

        [KSPField(guiName = "#autoLOC_6001401", guiActive = true)]
        public string displayStatus = Localizer.Format("#autoLOC_219034");

        [KSPField(isPersistant = true)]
        public bool isOn;

        [KSPField(guiName = "#autoLOC_6001402", isPersistant = true, guiActive = false, guiActiveEditor = true), UI_FloatRange(minValue = 0f, maxValue = 1f, stepIncrement = 0.05f)]
        public float lightR = 1f;

        [KSPField(guiName = "#autoLOC_6001403", isPersistant = true, guiActive = false, guiActiveEditor = true), UI_FloatRange(minValue = 0f, maxValue = 1f, stepIncrement = 0.05f)]
        public float lightG = 1f;

        [KSPField(guiName = "#autoLOC_6001404", isPersistant = true, guiActive = false, guiActiveEditor = true), UI_FloatRange(minValue = 0f, maxValue = 1f, stepIncrement = 0.05f)]
        public float lightB = 1f;

        [KSPField(isPersistant = true)]
        public int selectedColor = 0;

        [KSPField]
        public string colorize = "";

        [KSPField]
        public int colorizeType = 1;

        [KSPField]
        public string colorizeOff = "0,0,0,1";

        [KSPField(isPersistant = true, guiName = "Blink Active", guiActive = true, guiActiveEditor = true)]
        public bool blinkActive = false;

        [KSPField(guiActiveEditor = true, guiName = "Current Light Color")]
        public string currentColorName = string.Empty;

        [KSPField(isPersistant = true, guiActive =false, guiActiveEditor = true, guiName = "Blink Delay"), UI_FloatRange(minValue = 0.1f, maxValue = 10, stepIncrement = 0.1f)]
        public float blinkDelay = 1.0f;

        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Blink Duration"), UI_FloatRange(minValue = 0.1f, maxValue = 10, stepIncrement = 0.1f)]
        public float blinkDuration = 1.0f;

        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Pan Speed"), UI_FloatRange(minValue = 0.1f, maxValue = 1, stepIncrement = 0.01f)]
        public float panSpeed = 0.01f;

        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Tilt Speed"), UI_FloatRange(minValue = 0.1f, maxValue = 1, stepIncrement = 0.01f)]
        public float tiltSpeed = 0.01f;

        [KSPField]
        public bool PanningOn = false;

        [KSPField]
        public bool TiltingOn = false;

        [KSPField]
        public bool canBlink = true;
        #endregion

        #region Events
        [KSPEvent(guiActive = false, guiActiveEditor = true, guiActiveUnfocused = false, guiName = "Next Light Color")]
        public void nextLightColor()
        {
            selectedColor++;
            if (selectedColor > lightColorList.Count-1)
            {
                selectedColor = 0;
            }
            switchToColor(selectedColor);
        }

        [KSPEvent(guiActive = false, guiActiveEditor = true, guiActiveUnfocused = false, guiName = "Prev Light Color")]
        public void previousLightColor()
        {
            selectedColor--;
            if (selectedColor < 0)
            {
                selectedColor = lightColorList.Count - 1;
            }
            switchToColor(selectedColor);
        }

        [KSPEvent(name="Toggle Blink",guiActive = true, guiActiveEditor = true, guiActiveUnfocused = false, guiName = "Toggle Blink")]
        public void BlinkActivate()
        {
            if (blinkActive)
            {
                blinkActive = false;
                if (isOn)
                {
                    SetLight(true);
                }
            }
            else
            {
                blinkActive = true;
            }
        }

        [KSPEvent(guiName = "#autoLOC_6001408", guiActive = true, guiActiveEditor = true)]
        public void LightsOff()
        {
            SetLight(false);
        }

        [KSPEvent(guiName = "#autoLOC_6001409", guiActive = true, guiActiveEditor = true)]
        public void LightsOn()
        {
            if (!isOn)
            {
                SetLight(true);
            }
        }

        [KSPEvent(guiName="Pan/Tilt Light", guiActive = true, guiActiveEditor = true)]
        public void PanTiltAction()
        {
            doShowDialog();
        }
        #endregion

        #region Actions

        [KSPAction("Toggle Blink")]
        public void doToggleBlink(KSPActionParam param)
        {
            BlinkActivate();
        }

        [KSPAction("Blink On")]
        public void doBlinkOn(KSPActionParam param)
        {
            blinkActive = false;
            BlinkActivate();
        }

        [KSPAction("Blink Off")]
        public void doBlinkOff(KSPActionParam param)
        {
            blinkActive = true;
            BlinkActivate();
        }

        [KSPAction("#autoLOC_6001405", KSPActionGroup.Light)]
        public void ToggleLightAction(KSPActionParam param)
        {
            if (param.type == KSPActionType.Activate)
            {
                if (!isOn)
                {
                    SetLight(true);
                }
            }
            else
            {
                SetLight(false);
            }
        }

        [KSPAction("#autoLOC_6001406")]
        public void LightOnAction(KSPActionParam param)
        {
            if (!isOn)
            {
                SetLight(true);
            }
        }

        [KSPAction("#autoLOC_6001407")]
        public void LightOffAction(KSPActionParam param)
        {
            SetLight(false);
        }

        #endregion

        #region Functions

        private void doShowDialog()
        {
            PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new MultiOptionDialog("", "Light Position - Click pan once to start, again to stop.",
                    "Kerbal Electric",
                    HighLogic.UISkin,
                    new Rect(0.5f, 0.5f, 150f, 60f),
                    new DialogGUIFlexibleSpace(),
                    new DialogGUIVerticalLayout(
                        new DialogGUIFlexibleSpace(),
                        new DialogGUIButton("Pan Left",
                            delegate
                            {
                                doPanAnim(true);
                            }, 140.0f, 30.0f, false),
                        new DialogGUIButton("Pan Right",
                            delegate
                            {
                                doPanAnim(false);
                                }, 140.0f, 30.0f, false),
                        new DialogGUIButton("Tilt Up",
                            delegate
                            {
                                doTiltAnim(true);
                            }, 140.0f, 30.0f, false),
                        new DialogGUIButton("Tilt Down",
                            delegate
                            {
                                doTiltAnim(false);
                            }
                            , 140.0f, 30.0f, false),
                        new DialogGUIButton("Close", () => { }, 140.0f, 30.0f, true)
                        )),
                false,
                HighLogic.UISkin,false,"Click once to start, again to stop.");

        }

        private void switchToColor(int selectedColor)
        {
            Debug.Log("Switching Colors");
            string[] colorData = lightColorList[selectedColor].Split(',');
            currentColorName = colorData[0];
            lightB = (float)Convert.ToDouble(colorData[3]) / 255;
            lightR = (float)Convert.ToDouble(colorData[1]) / 255;
            lightG = (float)Convert.ToDouble(colorData[2]) / 255;
            foreach (Light L in myLights)
            {
                L.color = new Color(lightR, lightG, lightB, 1);
            }
            if (isSlime)
            {
                SetLens(new Color(lightR, lightG, lightB, 1));
            }
            if (dodebug)
            {
                Debug.Log("Colors Switched");
            }
        }

        public void animDoPlay(float animTime, float animSpeed)
        {
            partAnimState.normalizedTime = animTime;
            partAnimState.normalizedSpeed = animSpeed;
            partAnimation.Play(animationName);
        }

        public void doPanAnim(bool turnLeft)
        {
            PanningOn = !PanningOn;
            if (panAnimation == null)
            {
                GetAnims();
            }
            if (panAnimation != null)
            {
                if (!PanningOn)
                {
                    panAnimation[panAnimationName].normalizedSpeed = 0;
                }
                else
                {
                    if (!turnLeft)
                    {
                        panAnimation[panAnimationName].normalizedSpeed = panSpeed;

                    }
                    else
                    {
                        panAnimation[panAnimationName].normalizedSpeed = panSpeed * -1f;
                    }
                }
            }
        }

        public void doTiltAnim(bool upward)
        {
            TiltingOn = !TiltingOn;
            if (tiltAnimation == null)
            {
                GetAnims();
            }
            if (tiltAnimation != null)
            {
                if (!TiltingOn)
                {
                    tiltAnimation[tiltAnimationName].normalizedSpeed = 0;
                }
                else
                {
                    if (upward)
                    {
                        tiltAnimation[tiltAnimationName].normalizedSpeed = tiltSpeed;
                    }
                    else
                    {
                        tiltAnimation[tiltAnimationName].normalizedSpeed = tiltSpeed * -1f;
                    }
                }
            }
        }

        public void ToggleLight(bool state)
        {
            if (useAnimationDim)
            {
                if (partAnimState == null)
                {
                    GetAnims();
                }
                else
                {
                    if (HighLogic.LoadedSceneIsEditor || blinkActive)
                    {
                        animDoPlay((!state) ? 0f : 1, 0.0f);//toggle the light
                        SetLens((!state) ? offColor : new Color(lightR, lightG, lightB, 1));
                    }
                    else if (HighLogic.LoadedSceneIsFlight)
                    {
                        animDoPlay((state) ? 0f : 1, (state) ? 1.0f : -1.0f);//play the light                   
                        SetLens((!state) ? new Color(lightR, lightG, lightB, 1) : new Color(lightR, lightG, lightB, 1));  //placehoder to make sure lense is on when not in blink mode
                    }
                }
            }
            else //toggling the light on and off
            {
                if (isSlime)
                {
                    foreach (Renderer R in rend)
                    {
                        if (R.name == slimeLensOn)
                        {
                            R.enabled = state;
                        }
                        if (R.name == slimeLensOff)
                        {
                            R.enabled = !state;
                        }
                    }
                    foreach (Light L in myLights)
                    {
                        L.enabled = state;
                    }
                }
                else
                {
                    if (HighLogic.LoadedSceneIsEditor && partAnimState != null)
                    {
                        if (partAnimState != null)
                        {
                            animDoPlay((!state) ? 0f : 1, 0.0f);//toggle the light
                        }
                    }
                    else if (HighLogic.LoadedSceneIsFlight && partAnimState != null)
                    {
                        animDoPlay((!state) ? 0f : 1, 0.0f);//toggle the light
                    }
                    else
                    {
                        foreach (Light L in myLights)
                        {
                            L.enabled = state;
                        }
                    }
                }
            }
        }

        public void SetLight(bool state)
        {
            if (state)
            {
                isOn = true;
                displayStatus = Localizer.Format("#autoLOC_219034");
                base.Events["LightsOn"].active = false;
                base.Events["LightsOff"].active = true;
            }
            else
            {
                isOn = false;
                displayStatus = Localizer.Format("#autoLOC_220477");
                base.Events["LightsOn"].active = true;
                base.Events["LightsOff"].active = false;
            }
            ToggleLight(state);     
        }

        public void SetLens(Color thisColor)
        {
            if (colorize.Length > 0)
            {
                foreach (Renderer R in rend)
                {
                    if (R.name == colorize)
                    {
                        mats = R.material;                       
                        switch (colorizeType)
                        {
                            case 1:
                                mats.SetColor("_Color", thisColor);
                                break;
                            case 2:
                                mats.SetColor("_EmissiveColor", thisColor);
                                break;
                        }
                    }
                }                              
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
                if (animation[animationName] != null)
                {
                    partAnimState = animation[animationName];
                    partAnimation = animation;
                }
                if (animation[panAnimationName] != null)
                {
                    panAnimation = animation;
                    panAnimation[panAnimationName].normalizedSpeed = 0;
                    panAnimation[panAnimationName].normalizedTime = panTime;
                    panAnimation.Play();
                }
                if (animation[tiltAnimationName] != null)
                {
                    tiltAnimation = animation;
                    tiltAnimation[tiltAnimationName].normalizedSpeed = 0;
                    tiltAnimation[tiltAnimationName].normalizedTime = tiltTime;
                    tiltAnimation.Play();
                }
                i++;
            }
            if (partAnimState != null)
            {
                partAnimState.wrapMode = WrapMode.ClampForever;
                partAnimState.normalizedSpeed = 0f;
                partAnimState.normalizedTime = 0f;
                partAnimation.Play(animationName);
            }
        }

        public List<PartResourceDefinition> GetConsumedResources()
        {
            return consumedResources;
        }

        #endregion

        #region On Hooks

        public override void OnAwake()
        {
            if (useResources)
            {
                if (consumedResources == null)
                {
                    consumedResources = new List<PartResourceDefinition>();
                }
                else
                {
                    consumedResources.Clear();
                }
                int i = 0;
                int count = resHandler.inputResources.Count;
                while (i < count)
                {
                    consumedResources.Add(PartResourceLibrary.Instance.GetDefinition(resHandler.inputResources[i].name));
                    i++;
                }
            }
            else if (consumedResources == null)
            {
                consumedResources = new List<PartResourceDefinition>();
            }
            else
            {
                consumedResources.Clear();
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            if (resHandler.inputResources.Count == 0)
            {
                ModuleResource moduleResource = new ModuleResource();
                moduleResource.name = resourceName;
                moduleResource.title = KSPUtil.PrintModuleName(resourceName);
                moduleResource.id = resourceName.GetHashCode();
                moduleResource.rate = (double)resourceAmount;
                resHandler.inputResources.Add(moduleResource);
            }
        }

        public override void OnStart(StartState state)
        {
            if (!canBlink)
            {
                Events["BlinkActivate"].active = false;
                Actions["doToggleBlink"].active = false;
                Actions["doBlinkOn"].active = false;
                Actions["doBlinkOff"].active = false;
                Fields["blinkActive"].guiActive = false;
                Fields["blinkDuration"].guiActive = false;
                Fields["blinkDelay"].guiActive = false;
                Fields["blinkActive"].guiActiveEditor = false;
                Fields["blinkDuration"].guiActiveEditor = false;
                Fields["blinkDelay"].guiActiveEditor = false;
            }
            //Fields["panSpeed"].guiActive = false;
            //Fields["panSpeed"].guiActiveEditor = false;
            //Fields["tiltSpeed"].guiActive = false;
            //Fields["tiltSpeed"].guiActiveEditor = false;
            //Events["PanTiltAction"].active = false;
            rend = part.GetComponentsInChildren<Renderer>();
            GetAnims();
            myLights = part.GetComponentsInChildren<Light>();
            foreach (Light L in myLights)
            {
                L.color = new Color(lightR, lightG, lightB, 1);
            }
            SetLight(isOn);
            SetLens(new Color(lightR, lightG, lightB, 1));
            if (HighLogic.LoadedSceneIsEditor)
            {
                lightColorList.Clear();
                if (File.Exists(ConfigPath))
                {
                    string[] lightArray = File.ReadAllLines(ConfigPath);
                    lightColorList.AddRange(lightArray);
                }
                else
                {
                    lightColorList.Add("Presets not found,1,1,1");
                }
            }
            string[] colorData = colorizeOff.Split(',');
            offColor = new Color((float)Convert.ToDouble(colorData[0]), (float)Convert.ToDouble(colorData[1]), (float)Convert.ToDouble(colorData[2]), (float)Convert.ToDouble(colorData[3]));
        }


        public void FixedUpdate()
        {
            if (tiltAnimation[tiltAnimationName].normalizedTime > 1)
            {
                tiltAnimation[tiltAnimationName].normalizedTime = 1;
                TiltingOn = false;
            }
            if (tiltAnimation[tiltAnimationName].normalizedTime < 0)
            {
                tiltAnimation[tiltAnimationName].normalizedTime = 0;
                TiltingOn = false;
            }
            if (HighLogic.LoadedSceneIsEditor)
            {
                currentColor = new Color(lightR, lightG, lightB, 1);
                if (currentColor != lastColor)
                {
                    SetLens(currentColor);
                    foreach (Light L in myLights)
                    {
                        L.color = currentColor;
                    }
                    lastColor = currentColor;
                }
            }
            else
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (isOn)
                {
                    resourceFraction = (float)resHandler.UpdateModuleResourceInputs(ref displayStatus, 1.0, 0.99, false, false, true);
                    if (resourceFraction < 0.99f)
                    {
                        SetLight(false);
                    }
                }
            }
            if (blinkActive && isOn)
            {
                if (blinkOn)
                {
                    blinkOnTime -= Time.deltaTime;
                    if (blinkOnTime < 0)
                    {
                        ToggleLight(false);
                        blinkOnTime += blinkDuration;
                        blinkOn = false;
                    }
                }
                else
                {
                    blinkOffTime -= Time.deltaTime;
                    if (blinkOffTime < 0)
                    {
                        ToggleLight(true);
                        blinkOffTime += blinkDelay;
                        blinkOn = true;
                    }
                }
            }
        }
        #endregion
    }
}
