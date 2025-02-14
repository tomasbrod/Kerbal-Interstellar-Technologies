﻿using KSP.Localization;
using System;
using System.Linq;
using KIT.PowerManagement.Interfaces;
using UnityEngine;
using KIT.ResourceScheduler;

namespace KIT.Reactors
{
    abstract class InterstellarFusionReactor : InterstellarReactor, IFNChargedParticleSource
    {
        [KSPField(isPersistant = true)] public int fuel_mode;
        [KSPField(isPersistant = true)] public bool allowJumpStart = true;

        [KSPField] public double magneticNozzlePowerMult = 1;
        [KSPField] public int powerPriority;
        [KSPField] public bool powerIsAffectedByLithium = true;

        [KSPField] public double minimumLithiumModifier = 0.001;
        [KSPField] public double maximumLithiumModifier = 1;
        [KSPField] public double lithiumModifierExponent = 0.5;
        [KSPField] public double maximumChargedIspMult = 100;
        [KSPField] public double minimumChargedIspMult = 1;
        [KSPField] public double maintenancePowerWasteheatRatio = 0.1;

        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiName = "#LOC_KSPIE_FissionPB_Maintance")]//Maintance
        public string electricPowerMaintenance;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiName = "#LOC_KSPIE_FissionPB_PlasmaRatio")]//Plasma Ratio
        public double plasma_ratio = 1;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiName = "#LOC_KSPIE_FissionPB_PlasmaModifier", guiFormat = "F6")]//Plasma Modifier
        public double plasma_modifier = 1;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiName = "#LOC_KSPIE_FissionPB_IsSwappingFuelMode")]//Is Swapping Fuel Mode
        public bool isSwappingFuelMode;

        [KSPField] public double reactorRatioThreshold = 0.000005;
        [KSPField] public double minReactorRatio;

        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiName = "#LOC_KSPIE_FissionPB_RequiredRatio", guiFormat = "F3")]//Required Ratio
        public double required_reactor_ratio;

        // Properties

        public double MaximumChargedIspMult => maximumChargedIspMult;

        public double MinimumChargedIspMult => minimumChargedIspMult;

        public override double MaximumThermalPower => Math.Max(base.MaximumThermalPower * PlasmaModifier * lithium_modifier, 0);

        public override double MaximumChargedPower => base.MaximumChargedPower * PlasmaModifier;

        public override double MagneticNozzlePowerMult => magneticNozzlePowerMult;

        public override bool IsFuelNeutronRich => !CurrentFuelMode.Aneutronic && CurrentFuelMode.NeutronsRatio > 0;

        public double PowerRequirement => RawPowerOutput / FusionEnergyGainFactor;

        public double NormalizedPowerRequirement => PowerRequirement * CurrentFuelMode.NormalisedPowerRequirements;

        public override double StableMaximumReactorPower
        {
            get
            {
                var stablePower = base.StableMaximumReactorPower;

                return stablePower * ChargedPowerRatio + stablePower * ThermalPowerRatio * lithium_modifier;
            }
        }

        public virtual double PlasmaModifier
        {
            get
            {
                plasma_modifier = plasma_ratio >= 1 ? 1 : 0;
                return plasma_modifier;
            }
        }

        public double LithiumModifier
        {
            get
            {
                var modifier = CheatOptions.InfinitePropellant || !powerIsAffectedByLithium || ChargedPowerRatio >= 1 || minimumLithiumModifier == 1 ? 1
                    : TotalAmountLithium > 0
                        ? Math.Min(maximumLithiumModifier, Math.Max(minimumLithiumModifier, Math.Pow(TotalAmountLithium / TotalMaxAmountLithium, lithiumModifierExponent)))
                        : minimumLithiumModifier;

                return modifier;
            }
        }

        private void InitialiseGainFactors()
        {
            if (fusionEnergyGainFactorMk2 == 0)
                fusionEnergyGainFactorMk2 = fusionEnergyGainFactorMk1;
            if (fusionEnergyGainFactorMk3 == 0)
                fusionEnergyGainFactorMk3 = fusionEnergyGainFactorMk2;
            if (fusionEnergyGainFactorMk4 == 0)
                fusionEnergyGainFactorMk4 = fusionEnergyGainFactorMk3;
            if (fusionEnergyGainFactorMk5 == 0)
                fusionEnergyGainFactorMk5 = fusionEnergyGainFactorMk4;
            if (fusionEnergyGainFactorMk6 == 0)
                fusionEnergyGainFactorMk6 = fusionEnergyGainFactorMk5;
            if (fusionEnergyGainFactorMk7 == 0)
                fusionEnergyGainFactorMk7 = fusionEnergyGainFactorMk6;
        }

        public override void OnStart(StartState state)
        {
            InitialiseGainFactors();

            base.OnStart(state);
            Fields[nameof(lithium_modifier)].guiActive = powerIsAffectedByLithium;
        }

        [KSPEvent(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiActiveEditor = true, guiName = "#LOC_KSPIE_FissionPB_NextFusionMode", active = true)]//Next Fusion Mode
        public void NextFusionModeEvent()
        {
            SwitchToNextFuelMode(fuel_mode);
        }

        [KSPEvent(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiActiveEditor = true, guiName = "#LOC_KSPIE_FissionPB_PreviousFusionMode", active = true)]//Previous Fusion Mode
        public void PreviousFusionModeEvent()
        {
            SwitchToPreviousFuelMode(fuel_mode);
        }

        [KSPAction("Next Fusion Mode")]
        public void NextFusionModeAction(KSPActionParam param)
        {
            NextFusionModeEvent();
        }

        [KSPAction("Previous Fusion Mode")]
        public void PreviousFusionModeAction(KSPActionParam param)
        {
            PreviousFusionModeEvent();
        }

        private void SwitchToNextFuelMode(int initialFuelMode)
        {
            if (FuelModes == null || FuelModes.Count == 0)
                return;

            fuel_mode++;
            if (fuel_mode >= FuelModes.Count)
                fuel_mode = 0;

            stored_fuel_ratio = 1;
            CurrentFuelMode = FuelModes[fuel_mode];
            fuel_mode_name = CurrentFuelMode.ModeGUIName;

            UpdateFuelMode();

            if (!FullFuelRequirements() && fuel_mode != initialFuelMode)
                SwitchToNextFuelMode(initialFuelMode);

            isSwappingFuelMode = true;
        }

        private void SwitchToPreviousFuelMode(int initialFuelMode)
        {
            if (FuelModes == null || FuelModes.Count == 0)
                return;

            fuel_mode--;
            if (fuel_mode < 0)
                fuel_mode = FuelModes.Count - 1;

            CurrentFuelMode = FuelModes[fuel_mode];
            fuel_mode_name = CurrentFuelMode.ModeGUIName;

            UpdateFuelMode();

            if (!FullFuelRequirements() && fuel_mode != initialFuelMode)
                SwitchToPreviousFuelMode(initialFuelMode);

            isSwappingFuelMode = true;
        }

        private bool FullFuelRequirements()
        {
            return HasAllFuels() && FuelRequiresLab(CurrentFuelMode.RequiresLab);
        }

        private bool HasAllFuels()
        {
            if (CheatOptions.InfinitePropellant)
                return true;

            var hasAllFuels = true;
            foreach (var fuel in CurrentFuelVariantsSorted.First().ReactorFuels)
            {
                if (!(GetFuelRatio(fuel, FuelEfficiency, NormalisedMaximumPower) < 1)) continue;

                hasAllFuels = false;
                break;
            }
            return hasAllFuels;
        }

        protected override void WindowReactorControlSpecificOverride()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Localizer.Format("#LOC_KSPIE_FissionPB_NextModebutton"), GUILayout.ExpandWidth(true)))//"Next Fusion Mode"
            {
                NextFusionModeEvent();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Localizer.Format("#LOC_KSPIE_FissionPB_PreviousModebutton"), GUILayout.ExpandWidth(true)))//"Previous Fusion Mode"
            {
                PreviousFusionModeEvent();
            }
            GUILayout.EndHorizontal();

            PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_FissionPB_CurrentMaxMaintenance"), electricPowerMaintenance, BoldStyle, TextStyle);//"Current/Max Fusion Maintenance"
        }

        public new void KITFixedUpdate(IResourceManager resMan)
        {
            lithium_modifier = LithiumModifier;
            base.KITFixedUpdate(resMan);

            // determine amount of power needed
            required_reactor_ratio = Math.Max(minReactorRatio, reactor_power_ratio >= reactorRatioThreshold ? reactor_power_ratio : 0);
        }

        public override void SetDefaultFuelMode()
        {
            Debug.Log("[KSPI]: FusionReactor SetDefaultFuelMode");

            if (FuelModes == null)
            {
                Debug.Log("[KSPI]: FusionReactor SetDefaultFuelMode - load fuel modes");
                FuelModes = GetReactorFuelModes();
            }

            if (!string.IsNullOrEmpty(fuel_mode_name) && FuelModes.Any(m => m.ModeGUIName == fuel_mode_name))
            {
                CurrentFuelMode = FuelModes.First(m => m.ModeGUIName == fuel_mode_name);
            }
            else if (!string.IsNullOrEmpty(fuel_mode_variant) && FuelModes.Any(m => m.Variants.Any(l => l.Name == fuel_mode_variant)))
            {
                CurrentFuelMode = FuelModes.First(m => m.Variants.Any(l => l.Name == fuel_mode_variant));
            }
            else if (fuelmode_index >= 0 && FuelModes.Any(m => m.Index == fuelmode_index))
            {
                CurrentFuelMode = FuelModes.First(m => m.Index == fuelmode_index);
            }
            else if (FuelModes.Any(m => m.Index == fuel_mode))
            {
                CurrentFuelMode = FuelModes.First(m => m.Index == fuel_mode);
            }
            else
            {
                CurrentFuelMode = (fuel_mode < FuelModes.Count) ? FuelModes[fuel_mode] : FuelModes.FirstOrDefault();
            }

            fuel_mode = FuelModes.IndexOf(CurrentFuelMode);
        }

        public new ModuleConfigurationFlags ModuleConfiguration()
        {
            return ModuleConfigurationFlags.LocalResources | (ModuleConfigurationFlags) powerPriority;
        }
    }
}
