﻿using KSP.Localization;
using System;
using KIT.Resources;
using KIT.ResourceScheduler;

namespace KIT.Refinery
{
    class AntimatterFactory : PartModule, IKITModule
    {
        public const double ONE_THIRD = 1.0 / 3.0;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = false), UI_Toggle(disabledText = "#LOC_KSPIE_AntimatterFactory_Off", enabledText = "#LOC_KSPIE_AntimatterFactory_On")]//OffOn
        public bool isActive;
        [KSPField(isPersistant = true, guiActive = true, guiName = "#LOC_KSPIE_AntimatterFactory_powerPecentage"), UI_FloatRange(stepIncrement = 0.5f, maxValue = 100, minValue = 1)]
        public float powerPercentage = 100;

        [KSPField]
        public string activateTitle = "#LOC_KSPIE_AntimatterFactory_producePositron";

        [KSPField(isPersistant = true)]
        public double electricalPowerRatio;
        [KSPField(guiActive = true, guiName = "#LOC_KSPIE_AntimatterFactory_productionRate")]
        public string productionRateTxt;

        [KSPField]
        public double productionRate;
        [KSPField]
        public double efficiencyMultiplier = 10;
        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "#LOC_KSPIE_AntimatterFactory_MaximumPowercapacity", guiUnits = "#LOC_KSPIE_Reactor_megawattUnit")]//Maximum Power capacity
        public double powerCapacity = 1000;
        [KSPField]
        public string resourceName = "Positrons";

        AntimatterGenerator _generator;
        PartResourceDefinition _antimatterDefinition;
        string _disabledText;

        public override void OnStart(StartState state)
        {
            _antimatterDefinition = PartResourceLibrary.Instance.GetDefinition(resourceName);
            _generator = new AntimatterGenerator(part, efficiencyMultiplier, _antimatterDefinition);

            if (state == StartState.Editor)
                return;

            _disabledText = Localizer.Format("#LOC_KSPIE_AntimatterFactory_disabled");

            Fields[nameof(isActive)].guiName = Localizer.Format(activateTitle);
        }

        public override void OnUpdate()
        {
            if (!isActive)
            {
                productionRateTxt = _disabledText;
                return;
            }

            double antimatterRatePerDay = productionRate * PluginSettings.Config.SecondsInDay;

            if (antimatterRatePerDay > 0.1)
            {
                if (antimatterRatePerDay > 1000)
                    productionRateTxt = (antimatterRatePerDay / 1e3).ToString("0.0000") + " g/" + Localizer.Format("#LOC_KSPIE_AntimatterFactory_perday");//day
                else
                    productionRateTxt = (antimatterRatePerDay).ToString("0.0000") + " mg/" + Localizer.Format("#LOC_KSPIE_AntimatterFactory_perday");//day
            }
            else
            {
                if (antimatterRatePerDay > 0.1e-3)
                    productionRateTxt = (antimatterRatePerDay * 1e3).ToString("0.0000") + " ug/" + Localizer.Format("#LOC_KSPIE_AntimatterFactory_perday");//day
                else
                    productionRateTxt = (antimatterRatePerDay * 1e6).ToString("0.0000") + " ng/" + Localizer.Format("#LOC_KSPIE_AntimatterFactory_perday");//day
            }
        }


        public ModuleConfigurationFlags ModuleConfiguration() => ModuleConfigurationFlags.Fourth;

        public void KITFixedUpdate(IResourceManager resMan)
        {
            if (!isActive)
                return;

            // TODO 
            var availablePower = 300; //  getAvailableStableSupply(ResourceSettings.Config.ElectricPowerInMegawatt);
            var resourceBarRatio = resMan.FillFraction(ResourceName.ElectricCharge);
            var effectiveResourceThrottling = resourceBarRatio > ONE_THIRD ? 1 : resourceBarRatio * 3;

            var energyRequestedInMegajoulesPerSecond = Math.Min(powerCapacity, effectiveResourceThrottling * availablePower * (double)(decimal)powerPercentage * 0.01);

            // XXX
            var energyProvidedInMegajoulesPerSecond = resMan.Consume(ResourceName.ElectricCharge, energyRequestedInMegajoulesPerSecond);

            electricalPowerRatio = energyRequestedInMegajoulesPerSecond > 0 ? energyProvidedInMegajoulesPerSecond / energyRequestedInMegajoulesPerSecond : 0;

            _generator.Produce(energyProvidedInMegajoulesPerSecond * (double)(decimal)TimeWarp.fixedDeltaTime);

            productionRate = _generator.ProductionRate;
        }

        public string KITPartName() => part.partInfo.title;
    }
}
