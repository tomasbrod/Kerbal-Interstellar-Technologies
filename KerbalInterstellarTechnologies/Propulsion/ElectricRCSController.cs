﻿using KIT.Extensions;
using KIT.Resources;
using KIT.ResourceScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KIT.Propulsion
{
    class ElectricRCSController : PartModule, IKITModule
    {
        public const string Group = "InterstellarRCSModule";
        public const string GroupTitle = "#LOC_KSPIE_RCSModule_groupName";

        [KSPField(isPersistant = true)]
        public int fuel_mode;
        [KSPField(isPersistant = true)]
        public string fuel_mode_name;
        [KSPField(isPersistant = false)]
        public string AnimationName = "";
        [KSPField(isPersistant = false)]
        public double efficiency = 0.8;
        [KSPField(isPersistant = false)]
        public int type = 16;
        [KSPField(isPersistant = false)]
        public float maxThrust = 1;
        [KSPField(isPersistant = false)]
        public float maxIsp = 2000;
        [KSPField(isPersistant = false)]
        public float minIsp = 250;
        [KSPField(isPersistant = false)]
        public string displayName = "";
        [KSPField(isPersistant = false)]
        public bool showConsumption = true;
        [KSPField(isPersistant = false)]
        public double powerMult = 1;
        [KSPField(isPersistant = false)]
        public double bufferMult = 8;
        [KSPField(isPersistant = false)]
        public int rcsIndex;

        [KSPField(isPersistant = true, guiActive = false)]
        public double storedPower;
        [KSPField(isPersistant = true, guiActive = false)]
        public double maxStoredPower;

        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = true, guiActiveEditor = false, guiActive = true, guiName = "#LOC_KSPIE_ElectricRCSController_HasSufficientPower")]//Is Powered
        public bool hasSufficientPower;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = false, guiActiveEditor = true, guiActive = true, guiName = "#LOC_KSPIE_ElectricRCSController_PropellantName")]//Propellant Name
        public string propNameStr = "";
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = false, guiActiveEditor = true, guiActive = false, guiName = "#LOC_KSPIE_ElectricRCSController_PropellantMaximumIsp")]//Propellant Maximum Isp
        public float maxPropellantIsp;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = false, guiActiveEditor = true, guiActive = false, guiName = "#LOC_KSPIE_ElectricRCSController_PropellantThrustMultiplier")]//Propellant Thrust Multiplier
        public double currentThrustMultiplier = 1;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = false, guiActiveEditor = false, guiActive = false, guiName = "#LOC_KSPIE_ElectricRCSController_ThrustIspMultiplier")]//Thrust / ISP Mult
        public string thrustIspMultiplier = "";
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = false, guiActiveEditor = false, guiActive = true, guiName = "#LOC_KSPIE_ElectricRCSController_CurrentTotalThrust", guiFormat = "F2", guiUnits = " kN")]//Current Total Thrust
        public float currentThrust;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = false, guiActiveEditor = true, guiActive = false, guiName = "#LOC_KSPIE_ElectricRCSController_Mass", guiFormat = "F3", guiUnits = " t")]//Mass
        public float partMass;

        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = false, guiActive = true, guiName = "#LOC_KSPIE_ElectricRCSController_Consumption")]//Consumption
        public string powerConsumptionStr = "";
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = false, guiActiveEditor = true, guiActive = true, guiName = "#LOC_KSPIE_ElectricRCSController_Efficiency")]//Efficiency
        public string efficiencyStr = "";
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "#LOC_KSPIE_ElectricRCSController_Power", advancedTweakable = true), UI_Toggle(disabledText = "#LOC_KSPIE_ElectricRCSController_Power_Off", enabledText = "#LOC_KSPIE_ElectricRCSController_Power_On", affectSymCounterparts = UI_Scene.All)]//Power--Off--On
        public bool powerEnabled = true;

        // internal
        private AnimationState[] rcsStates;

        private readonly IList<ElectricEnginePropellant> propellants;
        private ModuleRCSFX attachedRCS;
        private bool oldPowerEnabled;
        private bool delayedVerificationPropellant;
        private BaseField powerConsumptionStrField;

        public ElectricEnginePropellant CurrentPropellant { get; set; }

        public ElectricRCSController()
        {
            hasSufficientPower = true;
            propellants = new List<ElectricEnginePropellant>(32);
        }

        [KSPAction("Next Propellant")]
        public void ToggleNextPropellantAction(KSPActionParam param)
        {
            ToggleNextPropellantEvent();
        }

        [KSPAction("Previous Propellant")]
        public void TogglePreviousPropellantAction(KSPActionParam param)
        {
            TogglePreviousPropellantEvent();
        }

        [KSPEvent(groupName = Group, guiActiveEditor = true, guiActive = true, guiName = "#LOC_KSPIE_ElectricRCSController_NextPropellant", active = true)]//Next Propellant
        public void ToggleNextPropellantEvent()
        {
            SwitchToNextPropellant();
        }

        [KSPEvent(groupName = Group, guiActiveEditor = true, guiActive = true, guiName = "#LOC_KSPIE_ElectricRCSController_PreviousPropellant", active = true)]//Previous Propellant
        public void TogglePreviousPropellantEvent()
        {
            SwitchToPreviousPropellant();
        }

        private void LoadConfig()
        {
            double effectiveIspMultiplier = (type == (int)ElectricEngineType.ARCJET) ?
                CurrentPropellant.DecomposedIspMult : CurrentPropellant.IspMultiplier;
            string propName = CurrentPropellant.Propellant.name;

            var moduleConfig = new ConfigNode("MODULE");
            moduleConfig.AddValue("thrusterPower", attachedRCS.thrusterPower.
                ToString("F3"));
            moduleConfig.AddValue("resourceName", propName);
            moduleConfig.AddValue("resourceFlowMode", "STAGE_PRIORITY_FLOW");

            var propellantConfigNode = moduleConfig.AddNode("PROPELLANT");
            propellantConfigNode.AddValue("name", propName);
            propellantConfigNode.AddValue("ratio", "1");
            propellantConfigNode.AddValue("DrawGauge", "True");
            propellantConfigNode.AddValue("resourceFlowMode", "STAGE_PRIORITY_FLOW");
            attachedRCS.Load(propellantConfigNode);

            currentThrustMultiplier = hasSufficientPower ? CurrentPropellant.
                ThrustMultiplier : CurrentPropellant.ThrustMultiplierCold;

            var effectiveBaseIsp = hasSufficientPower ? maxIsp : minIsp;
            maxPropellantIsp = (float)(effectiveBaseIsp * effectiveIspMultiplier *
                currentThrustMultiplier);

            var atmosphereCurve = new ConfigNode("atmosphereCurve");
            atmosphereCurve.AddValue("key", "0 " + maxPropellantIsp.ToString("F3"));
            if (type == (int)ElectricEngineType.VACUUMTHRUSTER)
            {
                atmosphereCurve.AddValue("key", "1 " + (maxPropellantIsp).ToString("F3"));
            }
            else
            {
                atmosphereCurve.AddValue("key", "1 " + (maxPropellantIsp * 0.5).
                    ToString("F3"));
                atmosphereCurve.AddValue("key", "4 " + (maxPropellantIsp * 0.00001).
                    ToString("F3"));
            }

            moduleConfig.AddNode(atmosphereCurve);
            attachedRCS.Load(moduleConfig);
            thrustIspMultiplier = currentThrustMultiplier.ToString("F2") + " / " +
                maxPropellantIsp.ToString("F0") + " s";
        }

        public override void OnStart(StartState state)
        {
            var rcs = part.FindModulesImplementing<ModuleRCSFX>();
            int fm;
            attachedRCS = (rcsIndex >= rcs.Count) ? null : rcs[rcsIndex];

            if (partMass == 0)
                partMass = part.mass;

            oldPowerEnabled = powerEnabled;
            efficiencyStr = efficiency.ToString("P1");

            if (!string.IsNullOrEmpty(AnimationName))
                rcsStates = PluginHelper.SetUpAnimation(AnimationName, part);

            // Only allow propellants that are compatible with this engine type
            propellants.Clear();
            foreach (var propellant in ElectricEnginePropellant.GetPropellantsEngineForType(
                type))
            {
                if ((propellant.SupportedEngines & type) != 0)
                {
                    propellants.Add(propellant);
                }
            }

            if (propellants.Count < 1)
            {
                Debug.LogError("[KSPI]: No propellants available for RCS type " + type + "!");
            }

            delayedVerificationPropellant = true;
            // find correct fuel mode index
            if (!string.IsNullOrEmpty(fuel_mode_name))
            {
                foreach (var propellant in propellants)
                {
                    if (propellant.PropellantName == fuel_mode_name)
                    {
                        Debug.Log("[KSPI]: ElectricRCSController set fuel mode " +
                            fuel_mode_name);
                        CurrentPropellant = propellant;
                        break;
                    }
                }
            }

            if (CurrentPropellant != null && (fm = propellants.IndexOf(CurrentPropellant)) >= 0)
            {
                fuel_mode = fm;
            }
            SetPropellant(true);

            base.OnStart(state);

            powerConsumptionStrField = Fields[nameof(powerConsumptionStr)];
            powerConsumptionStrField.guiActive = showConsumption;

            maxStoredPower = bufferMult * maxThrust * powerMult * maxIsp * PhysicsGlobals.GravitationalAcceleration / (efficiency * 1000.0);
        }

        private void MovePropellant(bool moveNext)
        {
            if (moveNext)
            {
                fuel_mode++;
                if (fuel_mode >= propellants.Count)
                {
                    fuel_mode = 0;
                }
            }
            else
            {
                fuel_mode--;
                if (fuel_mode < 0)
                {
                    fuel_mode = propellants.Count - 1;
                }
            }
        }

        public void OnEditorAttach()
        {
            delayedVerificationPropellant = true;
        }

        public override void OnUpdate()
        {
            bool rcsIsOn = vessel.ActionGroups[KSPActionGroup.RCS];

            if (delayedVerificationPropellant)
            {
                delayedVerificationPropellant = false;
                SetPropellant(true);
            }

            powerConsumptionStrField.guiActive = attachedRCS != null && rcsIsOn;

            if (rcsStates != null)
            {
                bool rcsPartActive = false;

                var moduleImplementingRcs = part.FindModuleImplementing<ModuleRCS>();
                if (moduleImplementingRcs != null)
                {
                    rcsPartActive = moduleImplementingRcs.isEnabled;
                }

                int n = rcsStates.Length;
                bool extend = attachedRCS != null && attachedRCS.rcsEnabled && rcsIsOn && rcsPartActive;
                for (var i = 0; i < n; i++)
                {
                    var anim = rcsStates[i];
                    if (extend)
                    {
                        if (anim.normalizedTime >= 1.0f)
                        {
                            anim.normalizedTime = 1.0f;
                            anim.speed = 0.0f;
                        }
                        else
                            anim.speed = 1.0f;
                    }
                    else
                    {
                        if (anim.normalizedTime <= 0.0f)
                        {
                            anim.normalizedTime = 0.0f;
                            anim.speed = 0.0f;
                        }
                        else
                            anim.speed = -1.0f;
                    }
                }
            }
        }

        private void SetPropellant(bool moveNext)
        {
            int attempts = propellants.Count;
            do
            {
                CurrentPropellant = propellants[fuel_mode];
                fuel_mode_name = CurrentPropellant.PropellantName;

                var propellant = CurrentPropellant.Propellant;
                string propName = propellant.name;
                if ((!HighLogic.LoadedSceneIsFlight || part.GetConnectedResources(propName).
                    Any()) && PartResourceLibrary.Instance.GetDefinition(propName) != null)
                {
                    propNameStr = CurrentPropellant.PropellantGUIName;
                    LoadConfig();
                    break;
                }
                else
                {
                    Debug.Log("[KSPI]: ElectricRCSController switching mode, cannot use " +
                        propName);
                    MovePropellant(moveNext);
                }
            } while (--attempts >= 0);
        }

        protected void SwitchToNextPropellant()
        {
            MovePropellant(true);
            SetPropellant(true);
        }

        protected void SwitchToPreviousPropellant()
        {
            MovePropellant(false);
            SetPropellant(false);
        }

        [KSPAction("Toggle Power")]
        public void TogglePowerAction(KSPActionParam _)
        {
            powerEnabled = !powerEnabled;

            SetPropellant(true);
        }

        public void Update()
        {
            if (CurrentPropellant == null) return;

            if (oldPowerEnabled != powerEnabled)
            {
                hasSufficientPower = powerEnabled;
                SetPropellant(true);
                oldPowerEnabled = powerEnabled;
            }
        }

        public ModuleConfigurationFlags ModuleConfiguration() => ModuleConfigurationFlags.Second;

        public void KITFixedUpdate(IResourceManager resMan)
        {
            if (attachedRCS == null || !HighLogic.LoadedSceneIsFlight || !vessel.ActionGroups[KSPActionGroup.RCS]) return;

            bool powerConsumed = false;

            var forces = attachedRCS.thrustForces;
            int forcesCount = forces.Length;
            currentThrust = 0.0f;
            for (int i = 0; i < forcesCount; i++)
            {
                currentThrust += forces[i];
            }

            double received = 0.0, requested = 0.0;
            if (powerEnabled)
            {
                if (currentThrust > 0.0f)
                {
                    requested = 0.5 * powerMult * currentThrust * maxIsp * PhysicsGlobals.GravitationalAcceleration / (efficiency * 1000.0 * CurrentPropellant.
                        ThrustMultiplier);
                }
                // XXX - is this the amount correct, might need a * ec in MJ conversion.

                var wanted = requested + Math.Min(requested, Math.Max(0.0, maxStoredPower - storedPower));
                received = resMan.Consume(ResourceName.ElectricCharge, wanted);
                resMan.Produce(ResourceName.WasteHeat, received * (1.0 - efficiency));

                double totalPower = storedPower + received, energyNeed = requested;
                if (totalPower >= energyNeed)
                {
                    powerConsumed = true;
                    totalPower -= energyNeed;
                }
                storedPower = totalPower;
            }
            powerConsumptionStr = PluginHelper.GetFormattedPowerString(received) + " / " +
                PluginHelper.GetFormattedPowerString(requested);

            if (hasSufficientPower != powerConsumed)
            {
                hasSufficientPower = powerConsumed;
                SetPropellant(true);
            }
        }

        public string KITPartName() => $"{part.partInfo.title} ({propNameStr})";
    }
}
