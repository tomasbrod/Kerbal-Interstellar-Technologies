﻿using KIT.Resources;
using KIT.Wasteheat;
using KSP.Localization;
using System;
using System.Linq;
using KIT.Interfaces;
using UnityEngine;

namespace KIT.Reactors
{
    [KSPModule("Nuclear Salt Water Reactor")]
    class InterstellarNSWR : InterstellarFissionMSRGC { }

    [KSPModule("Nuclear Thermal Reactor")]
    class InterstellarFissionNTR : InterstellarFissionMSRGC { }

    [KSPModule("Fission Reactor")]
    class InterstellarFissionReactor : InterstellarFissionMSRGC { }

    [KSPModule("Molten Salt Reactor")]
    class InterstellarMoltenSaltReactor : InterstellarFissionMSRGC { }

    [KSPModule("Fission Reactor")]
    class InterstellarFissionMSRGC : InterstellarReactor, INuclearFuelReprocessable
    {
        [KSPField(isPersistant = true)]
        public int fuel_mode;
        [KSPField]
        public double actinidesModifer = 1;
        [KSPField]
        public double temperatureThrottleExponent = 0.5;
        [KSPField(guiActive = false)]
        public double temp_scale;
        [KSPField(guiActive = false)]
        public double temp_diff;
        [KSPField(guiActive = false)]
        public double minimumTemperature;
        [KSPField(guiActive = false)]
        public bool canDumpActinides;

        BaseEvent _manualRestartEvent;

        PartResourceDefinition _fluorineGasDefinition;
        PartResourceDefinition _depletedFuelDefinition;
        PartResourceDefinition _enrichedUraniumDefinition;
        PartResourceDefinition _oxygenGasDefinition;

        double _fluorineDepletedFuelVolumeMultiplier;
        double _enrichedUraniumVolumeMultiplier;
        double _depletedToEnrichVolumeMultiplier;
        double _oxygenDepletedUraniumVolumeMultiplier;
        double _reactorFuelMaxAmount;

        public override bool IsFuelNeutronRich => !CurrentFuelMode.Aneutronic;

        public override bool IsNuclear => true;

        public double WasteToReprocess => part.Resources.Contains(KITResourceSettings.Actinides) ? part.Resources[KITResourceSettings.Actinides].amount : 0;

        [KSPEvent(groupName = Group, groupDisplayName = GroupTitle, guiName = "#LOC_KSPIE_FissionMSRGC_Dump_Actinides", guiActiveEditor = false, guiActive = true)]
        public void DumpActinides()
        {
            var actinides = part.Resources[KITResourceSettings.Actinides];
            if (actinides == null)
            {
                Debug.LogError("[KSPI]: actinides not found on " + part.partInfo.title);
                return;
            }

            var uranium233 = part.Resources[KITResourceSettings.Uranium233];
            if (uranium233 == null)
            {
                Debug.LogError("[KSPI]: uranium-233 not found on " + part.partInfo.title);
                return;
            }

            actinides.amount = 0;
            uranium233.amount = Math.Max(0, uranium233.amount - actinides.maxAmount);

            var message = Localizer.Format("#LOC_Dumped_Actinides");
            ScreenMessages.PostScreenMessage(message, 20.0f, ScreenMessageStyle.UPPER_CENTER);
            Debug.Log("[KSPI]: " + message);
        }

        [KSPEvent(groupName = Group, groupDisplayName = GroupTitle, guiName = "#LOC_KSPIE_FissionMSRGC_SwapFuel", externalToEVAOnly = true, guiActiveUnfocused = true, guiActive = false, unfocusedRange = 3.5f)]//Swap Fuel
        public void SwapFuelMode()
        {
            if (!part.Resources.Contains(KITResourceSettings.Actinides) || part.Resources[KITResourceSettings.Actinides].amount > 0.01) return;
            DefuelCurrentFuel();
            if (IsCurrentFuelDepleted())
            {
                DisableResources();
                SwitchFuelType();
                EnableResources();
                Refuel();
            }
        }

        [KSPEvent(groupName = Group, groupDisplayName = GroupTitle, guiName = "#LOC_KSPIE_FissionMSRGC_SwapFuel", guiActiveEditor = true, guiActive = false)]//Swap Fuel
        public void EditorSwapFuel()
        {
            if (FuelModes.Count == 1)
                return;

            DisableResources();
            SwitchFuelType();
            EnableResources();

            var modesAvailable = CheckFuelModes();
            // Hide Switch Mode button if theres only one mode for the selected fuel type available
            Events["SwitchMode"].guiActiveEditor = Events["SwitchMode"].guiActive = Events["SwitchMode"].guiActiveUnfocused = modesAvailable > 1;
        }

        [KSPEvent(groupName = Group, groupDisplayName = GroupTitle, guiName = "#LOC_KSPIE_FissionMSRGC_SwitchMode", guiActiveEditor = true, guiActiveUnfocused = true, guiActive = true)]//Switch Mode
        public void SwitchMode()
        {
            var startFirstFuelType = CurrentFuelMode.Variants.First().ReactorFuels.First();
            var currentFirstFuelType = startFirstFuelType;

            // repeat until found same or different fuel mode with same kind of primary fuel
            do
            {
                fuel_mode++;
                if (fuel_mode >= FuelModes.Count)
                    fuel_mode = 0;

                CurrentFuelMode = FuelModes[fuel_mode];
                currentFirstFuelType = CurrentFuelMode.Variants.First().ReactorFuels.First();
            }
            while (currentFirstFuelType.ResourceName != startFirstFuelType.ResourceName);

            FuelModeStr = CurrentFuelMode.ModeGUIName;

            int modesAvailable = CheckFuelModes();
            // Hide Switch Mode button if there's only one mode for the selected fuel type available
            Events["SwitchMode"].guiActiveEditor = Events["SwitchMode"].guiActive = Events["SwitchMode"].guiActiveUnfocused = modesAvailable > 1;
        }

        protected new double GetLocalResourceRatio(ReactorFuel fuel)
        {
            if (part.Resources.Contains(fuel.ResourceName))
                return part.Resources[fuel.ResourceName].amount / part.Resources[fuel.ResourceName].maxAmount;
            else
                return 0;
        }

        [KSPEvent(groupName = Group, groupDisplayName = GroupTitle, guiName = "#LOC_KSPIE_FissionMSRGC_ManualRestart", externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 3.5f)]//Manual Restart
        public void ManualRestart()
        {
            // verify any of the fuel types has at least 50% availability inside the reactor
            if (CurrentFuelMode.Variants.Any(variant => variant.ReactorFuels.All(fuel => GetLocalResourceRatio(fuel) > 0.5)))
                IsEnabled = true;
        }

        [KSPEvent(groupName = Group, groupDisplayName = GroupTitle, guiName = "#LOC_KSPIE_FissionMSRGC_ManualShutdown", externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 3.5f)]//Manual Shutdown
        public void ManualShutdown()
        {
            IsEnabled = false;
        }

        [KSPEvent(groupName = Group, groupDisplayName = GroupTitle, guiName = "#LOC_KSPIE_FissionMSRGC_Refuel", externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 3.5f)]//Refuel
        public void Refuel()
        {
            foreach (ReactorFuel fuel in CurrentFuelMode.Variants.First().ReactorFuels)
            {
                // avoid exceptions, just in case
                if (!part.Resources.Contains(fuel.ResourceName) || !part.Resources.Contains(KITResourceSettings.Actinides)) return;

                var fuelReactor = part.Resources[fuel.ResourceName];
                var actinidesReactor = part.Resources[KITResourceSettings.Actinides];
                var fuelResources = part.vessel.parts.SelectMany(p => p.Resources.Where(r => r.resourceName == fuel.ResourceName && r != fuelReactor)).ToList();

                double spareCapacityForFuel = fuelReactor.maxAmount - actinidesReactor.amount - fuelReactor.amount;
                fuelResources.ForEach(res =>
                {
                    double resourceAvailable = res.amount;
                    double resourceAdded = Math.Min(resourceAvailable, spareCapacityForFuel);
                    fuelReactor.amount += resourceAdded;
                    res.amount -= resourceAdded;
                    spareCapacityForFuel -= resourceAdded;
                });
            }
        }

        public override double MaximumThermalPower
        {
            get
            {
                if (!HighLogic.LoadedSceneIsFlight)
                    return base.MaximumThermalPower;

                if (CheatOptions.UnbreakableJoints)
                {
                    actinidesModifer = 1;
                    return base.MaximumThermalPower;
                }

                var actinidesResource = part.Resources[KITResourceSettings.Actinides];

                if (actinidesResource != null)
                {
                    var fuelActinideMassRatio = 1 - actinidesResource.amount / actinidesResource.maxAmount;

                    actinidesModifer = Math.Pow(fuelActinideMassRatio * fuelActinideMassRatio, CurrentFuelMode.NormalisedReactionRate);

                    return base.MaximumThermalPower * actinidesModifer;
                }

                return base.MaximumThermalPower;
            }
        }

        protected override void WindowReactorStatusSpecificOverride()
        {
            PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_FissionMSRGC_Actinides_Poisoning"), (100 - actinidesModifer * 100).ToString("0.000000") + "%", BoldStyle, TextStyle);
        }

        public override double CoreTemperature
        {
            get
            {
                if (!CheatOptions.IgnoreMaxTemperature && HighLogic.LoadedSceneIsFlight && !isupgraded && PowerPercent >= MinThrottle * 100)
                {
                    var baseCoreTemperature = base.CoreTemperature;

                    if (minimumTemperature > 0)
                        temp_scale = minimumTemperature;
                    else if (vessel != null && FNRadiator.HasRadiatorsForVessel(vessel))
                        temp_scale = FNRadiator.GetAverageMaximumRadiatorTemperatureForVessel(vessel);
                    else
                        temp_scale = baseCoreTemperature / 2;

                    temp_diff = (baseCoreTemperature - temp_scale) * Math.Pow(PowerPercent / 100, temperatureThrottleExponent);
                    return Math.Min(temp_scale + temp_diff, actinidesModifer * baseCoreTemperature);
                }
                else
                    return actinidesModifer * base.CoreTemperature;
            }
        }

        public override double MaxCoreTemperature => actinidesModifer * base.CoreTemperature;

        public override void OnUpdate()
        {
            Events[nameof(ManualShutdown)].active = Events[nameof(ManualShutdown)].guiActiveUnfocused = IsEnabled;
            Events[nameof(Refuel)].active = Events[nameof(Refuel)].guiActiveUnfocused = !IsEnabled && !DecayOngoing;
            Events[nameof(Refuel)].guiName = "Refuel " + (CurrentFuelMode != null ? CurrentFuelMode.ModeGUIName : "");
            Events[nameof(SwapFuelMode)].active = Events[nameof(SwapFuelMode)].guiActiveUnfocused = FuelModes.Count > 1 && !IsEnabled && !DecayOngoing;
            Events[nameof(SwapFuelMode)].guiActive = Events[nameof(SwapFuelMode)].guiActiveUnfocused = FuelModes.Count > 1;

            Events[nameof(SwitchMode)].guiActiveEditor = Events[nameof(SwitchMode)].guiActive = Events[nameof(SwitchMode)].guiActiveUnfocused = CheckFuelModes() > 1;
            Events[nameof(EditorSwapFuel)].guiActiveEditor = FuelModes.Count > 1;
            Events[nameof(DumpActinides)].guiActive = canDumpActinides;

            base.OnUpdate();
        }

        public override void OnStart(StartState state)
        {
            Debug.Log("[KSPI]: OnStart MSRGC " + part.name);

            // start as normal
            base.OnStart(state);

            // auto switch if current fuel mode is depleted
            if (IsCurrentFuelDepleted())
            {
                fuel_mode++;
                if (fuel_mode >= FuelModes.Count)
                    fuel_mode = 0;

                CurrentFuelMode = FuelModes[fuel_mode];
            }

            FuelModeStr = CurrentFuelMode.ModeGUIName;

            _manualRestartEvent = Events[nameof(ManualRestart)];

            _oxygenGasDefinition = PartResourceLibrary.Instance.GetDefinition(KITResourceSettings.OxygenGas);
            _fluorineGasDefinition = PartResourceLibrary.Instance.GetDefinition(KITResourceSettings.FluorineGas);
            _depletedFuelDefinition = PartResourceLibrary.Instance.GetDefinition(KITResourceSettings.DepletedFuel);
            _enrichedUraniumDefinition = PartResourceLibrary.Instance.GetDefinition(KITResourceSettings.EnrichedUranium);

            _depletedToEnrichVolumeMultiplier = _enrichedUraniumDefinition.density / _depletedFuelDefinition.density;
            _fluorineDepletedFuelVolumeMultiplier = ((19 * 4) / 232d) * (_depletedFuelDefinition.density / _fluorineGasDefinition.density);
            _enrichedUraniumVolumeMultiplier = (232d / (16 * 2 + 232d)) * (_depletedFuelDefinition.density / _enrichedUraniumDefinition.density);
            _oxygenDepletedUraniumVolumeMultiplier = ((16 * 2) / (16 * 2 + 232d)) * (_depletedFuelDefinition.density / _oxygenGasDefinition.density);

            var mainReactorFuel = part.Resources.Get(CurrentFuelMode.Variants.First().ReactorFuels.First().ResourceName);
            if (mainReactorFuel != null)
                _reactorFuelMaxAmount = part.Resources.Get(CurrentFuelMode.Variants.First().ReactorFuels.First().ResourceName).maxAmount;

            foreach (ReactorFuelType fuelMode in FuelModes)
            {
                foreach (ReactorFuel fuel in fuelMode.Variants.First().ReactorFuels)
                {
                    var resource = part.Resources.Get(fuel.ResourceName);
                    if (resource == null)
                        // non-tweakable resources
                        part.Resources.Add(fuel.ResourceName, 0, 0, true, false, false, true, 0);
                }
            }

            Events[nameof(DumpActinides)].guiActive = canDumpActinides;
            Events[nameof(SwitchMode)].guiActiveEditor = Events[nameof(SwitchMode)].guiActive = Events[nameof(SwitchMode)].guiActiveUnfocused = CheckFuelModes() > 1;
            Events[nameof(SwapFuelMode)].guiActive = Events[nameof(SwapFuelMode)].guiActiveUnfocused = FuelModes.Count > 1;
            Events[nameof(EditorSwapFuel)].guiActiveEditor = FuelModes.Count > 1;
        }

        public override void OnFixedUpdate()
        {
            // if reactor is overloaded with actinides, stop functioning
            if (IsEnabled && part.Resources.Contains(KITResourceSettings.Actinides))
            {
                if (part.Resources[KITResourceSettings.Actinides].amount >= part.Resources[KITResourceSettings.Actinides].maxAmount)
                {
                    part.Resources[KITResourceSettings.Actinides].amount = part.Resources[KITResourceSettings.Actinides].maxAmount;
                    IsEnabled = false;
                }
            }
            base.OnFixedUpdate();
        }

        public override bool ShouldScaleDownJetISP()
        {
            return true;
        }

        public double ReprocessFuel(double rate)
        {
            if (part.Resources.Contains(KITResourceSettings.Actinides))
            {
                var actinides = part.Resources[KITResourceSettings.Actinides];
                var newActinidesAmount = Math.Max(actinides.amount - rate, 0);
                var actinidesChange = actinides.amount - newActinidesAmount;
                actinides.amount = newActinidesAmount;

                var depletedFuelsRequest = actinidesChange * 0.2;
                var depletedFuelsProduced = -Part.RequestResource(_depletedFuelDefinition.id, -depletedFuelsRequest, ResourceFlowMode.STAGE_PRIORITY_FLOW);

                // first try to replace depletedFuel with enriched uranium
                var enrichedUraniumRequest = depletedFuelsProduced * _enrichedUraniumVolumeMultiplier;
                var enrichedUraniumRetrieved = Part.RequestResource(_enrichedUraniumDefinition.id, enrichedUraniumRequest, ResourceFlowMode.STAGE_PRIORITY_FLOW);
                var receivedEnrichedUraniumFraction = enrichedUraniumRequest > 0 ? enrichedUraniumRetrieved / enrichedUraniumRequest : 0;

                // if missing fluorine is dumped
                var oxygenChange = -Part.RequestResource(_oxygenGasDefinition.id, -depletedFuelsProduced * _oxygenDepletedUraniumVolumeMultiplier * receivedEnrichedUraniumFraction, ResourceFlowMode.STAGE_PRIORITY_FLOW);
                var fluorineChange = -Part.RequestResource(_fluorineGasDefinition.id, -depletedFuelsProduced * _fluorineDepletedFuelVolumeMultiplier * (1 - receivedEnrichedUraniumFraction), ResourceFlowMode.STAGE_PRIORITY_FLOW);

                var reactorFuels = CurrentFuelMode.Variants.First().ReactorFuels;
                var sumUsagePerMw = reactorFuels.Sum(fuel => fuel.AmountFuelUsePerMJ * fuelUsePerMJMult);

                foreach (ReactorFuel fuel in reactorFuels)
                {
                    var fuelResource = part.Resources[fuel.ResourceName];
                    var powerFraction = sumUsagePerMw > 0.0 ? fuel.AmountFuelUsePerMJ * fuelUsePerMJMult / sumUsagePerMw : 1;
                    var newFuelAmount = Math.Min(fuelResource.amount + ((depletedFuelsProduced * 4) + (depletedFuelsProduced * receivedEnrichedUraniumFraction)) * powerFraction * _depletedToEnrichVolumeMultiplier, fuelResource.maxAmount);
                    fuelResource.amount = newFuelAmount;
                }

                return actinidesChange;
            }
            return 0;
        }

        // This Methods loads the correct fuel mode
        public override void SetDefaultFuelMode()
        {
            if (FuelModes == null)
            {
                Debug.Log("[KSPI]: MSRC SetDefaultFuelMode - load fuel modes");
                FuelModes = GetReactorFuelModes();
            }

            CurrentFuelMode = (fuel_mode < FuelModes.Count) ? FuelModes[fuel_mode] : FuelModes.FirstOrDefault();
        }

        private void DisableResources()
        {
            bool editor = HighLogic.LoadedSceneIsEditor;
            foreach (ReactorFuel fuel in CurrentFuelMode.Variants.First().ReactorFuels)
            {
                var resource = part.Resources.Get(fuel.ResourceName);
                if (resource != null)
                {
                    if (editor)
                    {
                        resource.amount = 0;
                        resource.isTweakable = false;
                    }
                    resource.maxAmount = 0;

                }
            }
        }

        private void EnableResources()
        {
            bool editor = HighLogic.LoadedSceneIsEditor;
            foreach (ReactorFuel fuel in CurrentFuelMode.Variants.First().ReactorFuels)
            {
                var resource = part.Resources.Get(fuel.ResourceName);
                if (resource != null)
                {
                    if (editor)
                    {
                        resource.amount = _reactorFuelMaxAmount;
                        resource.isTweakable = true;
                    }
                    resource.maxAmount = _reactorFuelMaxAmount;
                }
            }
        }

        private void SwitchFuelType()
        {
            var startFirstFuelType = CurrentFuelMode.Variants.First().ReactorFuels.First();
            ReactorFuel currentFirstFuelType;

            do
            {
                fuel_mode++;
                if (fuel_mode >= FuelModes.Count)
                    fuel_mode = 0;

                CurrentFuelMode = FuelModes[fuel_mode];
                currentFirstFuelType = CurrentFuelMode.Variants.First().ReactorFuels.First();
            }
            while (currentFirstFuelType.ResourceName == startFirstFuelType.ResourceName);

            FuelModeStr = CurrentFuelMode.ModeGUIName;
        }

        private void DefuelCurrentFuel()
        {
            foreach (ReactorFuel fuel in CurrentFuelMode.Variants.First().ReactorFuels)
            {
                var fuelReactor = part.Resources[fuel.ResourceName];
                var swapResourceList = part.vessel.parts.SelectMany(p => p.Resources.Where(r => r.resourceName == fuel.ResourceName && r != fuelReactor)).ToList();

                swapResourceList.ForEach(res =>
                {
                    double spareCapacityForFuel = res.maxAmount - res.amount;
                    double fuelAdded = Math.Min(fuelReactor.amount, spareCapacityForFuel);
                    fuelReactor.amount -= fuelAdded;
                    res.amount += fuelAdded;
                });
            }
        }

        private bool IsCurrentFuelDepleted()
        {
            return CurrentFuelMode.Variants.First().ReactorFuels.Any(fuel => GetFuelAvailability(fuel) < 0.001);
        }


        // Returns the number of fuel-modes available for the currently selected fuel-type
        public int CheckFuelModes()
        {
            int modesAvailable = 0;
            var fuelType = CurrentFuelMode.Variants.First().ReactorFuels.First().FuelName;
            foreach (var reactorFuelType in FuelModes)
            {
                var currentMode = reactorFuelType.Variants.First().ReactorFuels.First().FuelName;
                if (currentMode == fuelType)
                {
                    modesAvailable++;
                }
            }
            return modesAvailable;
        }

        public override void Update()
        {
            base.Update();

            if (_manualRestartEvent != null)
                _manualRestartEvent.externalToEVAOnly = !HighLogic.CurrentGame.Parameters.CustomParams<KITGamePlayParams>().ExtendedReactorControl;
        }
    }
}
