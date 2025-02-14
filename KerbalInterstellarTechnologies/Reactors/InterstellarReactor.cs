﻿using KIT.Extensions;
using KIT.External;
using KIT.PowerManagement;
using KIT.Propulsion;
using KIT.Resources;
using KIT.ResourceScheduler;
using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using KIT.Interfaces;
using KIT.PowerManagement.Interfaces;
using TweakScale;
using UnityEngine;

namespace KIT.Reactors
{
    [KSPModule("#LOC_KSPIE_Reactor_moduleName")]
    class InterstellarReactor : PartModule, IKITModule, IKITVariableSupplier, IFNPowerSource, IRescalable<InterstellarReactor>, IPartCostModifier
    {
        public const string Group = "InterstellarReactor";
        public const string GroupTitle = "#LOC_KSPIE_Reactor_groupName";

        public const string UpgradesGroup = "ReactorUpgrades";
        public const string UpgradesGroupDisplayName = "#LOC_KSPIE_Reactor_upgrades";

        //public enum ReactorTypes
        //{
        //    FISSION_MSR = 1,
        //    FISSION_GFR = 2,
        //    FUSION_DT = 4,
        //    FUSION_GEN3 = 8,
        //    AIM_FISSION_FUSION = 16,
        //    ANTIMATTER = 32
        //}

        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "#LOC_KSPIE_Reactor_electricPriority"), UI_FloatRange(stepIncrement = 1, maxValue = 5, minValue = 0)]
        public float electricPowerPriority = 2;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "#LOC_KSPIE_Reactor_powerPercentage"), UI_FloatRange(stepIncrement = 0.5f, maxValue = 100, minValue = 10)]
        public float powerPercentage = 100;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "#LOC_KSPIE_Reactor_ForcedMinimumThrotle"), UI_FloatRange(stepIncrement = 0.5f, maxValue = 100, minValue = 0)]//Forced Minimum Throtle
        public float forcedMinimumThrottle;

        // Persistent True
        [KSPField(isPersistant = true)] public int fuelmode_index = -1;
        [KSPField(isPersistant = true)] public string fuel_mode_name = string.Empty;
        [KSPField(isPersistant = true)] public string fuel_mode_variant = string.Empty;

        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = true, guiName = "#LOC_KSPIE_Reactor_ReactorIsEnabled")]
        public bool IsEnabled;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = true, guiName = "#LOC_KSPIE_Reactor_ReactorIsStated")]
        public bool IsStarted;

        [KSPField(isPersistant = true)] public bool isDeployed;
        [KSPField(isPersistant = true)] public bool isupgraded;
        [KSPField(isPersistant = true)] public bool breedtritium;
        [KSPField(isPersistant = true)] public double ongoing_consumption_rate;
        [KSPField(isPersistant = true)] public double ongoing_wasteheat_rate;
        [KSPField(isPersistant = true)] public bool reactorInit;
        [KSPField(isPersistant = true)] public double neutronEmbrittlementDamage;
        [KSPField(isPersistant = true)] public double maxEmbrittlementFraction = 0.5;
        [KSPField(isPersistant = true)] public float windowPositionX = 20;
        [KSPField(isPersistant = true)] public float windowPositionY = 20;
        [KSPField(isPersistant = true)] public int currentGenerationType;

        [KSPField(isPersistant = true)] public double factorAbsoluteLinear = 1;
        [KSPField(isPersistant = true)] public double storedPowerMultiplier = 1;
        [KSPField(isPersistant = true)] public double stored_fuel_ratio = 1;
        [KSPField(isPersistant = true)] public double fuel_ratio = 1;
        [KSPField(isPersistant = true)] public double requested_thermal_power_ratio = 1;
        [KSPField(isPersistant = true)] public double maximumThermalPower;
        [KSPField(isPersistant = true)] public double maximumChargedPower;

        [KSPField(isPersistant = true)] public double thermal_power_ratio = 1;
        [KSPField(isPersistant = true)] public double charged_power_ratio = 1;
        [KSPField(isPersistant = true)] public double reactor_power_ratio = 1;
        [KSPField(isPersistant = true)] public double power_request_ratio = 1;

        [KSPField] public double maximum_thermal_request_ratio;
        [KSPField] public double maximum_charged_request_ratio;
        [KSPField] public double maximum_reactor_request_ratio;
        [KSPField] public double thermalThrottleRatio;
        [KSPField] public double plasmaThrottleRatio;
        [KSPField] public double chargedThrottleRatio;

        [KSPField(isPersistant = true)] public double storedIsThermalEnergyGeneratorEfficiency;
        [KSPField(isPersistant = true)] public double storedIsPlasmaEnergyGeneratorEfficiency;
        [KSPField(isPersistant = true)] public double storedIsChargedEnergyGeneratorEfficiency;

        [KSPField(isPersistant = true)] public double storedGeneratorThermalEnergyRequestRatio;
        [KSPField(isPersistant = true)] public double storedGeneratorPlasmaEnergyRequestRatio;
        [KSPField(isPersistant = true)] public double storedGeneratorChargedEnergyRequestRatio;

        [KSPField(isPersistant = true)]
        public double ongoing_total_power_generated;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = true, guiName = "#LOC_KSPIE_Reactor_thermalPower", guiFormat = "F6")]
        protected double ongoing_thermal_power_generated;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = true, guiName = "#LOC_KSPIE_Reactor_chargedPower ", guiFormat = "F6")]
        protected double ongoing_charged_power_generated;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiName = "#LOC_KSPIE_Reactor_LithiumModifier", guiFormat = "F6")]
        public double lithium_modifier = 1;

        [KSPField] public double maximumPower;
        [KSPField] public float minimumPowerPercentage = 10;

        [KSPField] public string upgradeTechReqMk2;
        [KSPField] public string upgradeTechReqMk3;
        [KSPField] public string upgradeTechReqMk4;
        [KSPField] public string upgradeTechReqMk5;
        [KSPField] public string upgradeTechReqMk6;
        [KSPField] public string upgradeTechReqMk7;

        [KSPField] public double minimumThrottleMk1;
        [KSPField] public double minimumThrottleMk2;
        [KSPField] public double minimumThrottleMk3;
        [KSPField] public double minimumThrottleMk4;
        [KSPField] public double minimumThrottleMk5;
        [KSPField] public double minimumThrottleMk6;
        [KSPField] public double minimumThrottleMk7;

        [KSPField] public double FuelEfficiencyMk1;
        [KSPField] public double fuelEfficiencyMk2;
        [KSPField] public double FuelEfficiencyMk3;
        [KSPField] public double fuelEfficiencyMk4;
        [KSPField] public double fuelEfficiencyMk5;
        [KSPField] public double fuelEfficiencyMk6;
        [KSPField] public double fuelEfficiencyMk7;

        [KSPField] public double hotBathTemperatureMk1;
        [KSPField] public double hotBathTemperatureMk2;
        [KSPField] public double hotBathTemperatureMk3;
        [KSPField] public double hotBathTemperatureMk4;
        [KSPField] public double hotBathTemperatureMk5;
        [KSPField] public double hotBathTemperatureMk6;
        [KSPField] public double hotBathTemperatureMk7;

        [KSPField] public double coreTemperatureMk1;
        [KSPField] public double coreTemperatureMk2;
        [KSPField] public double coreTemperatureMk3;
        [KSPField] public double coreTemperatureMk4;
        [KSPField] public double coreTemperatureMk5;
        [KSPField] public double coreTemperatureMk6;
        [KSPField] public double coreTemperatureMk7;

        [KSPField] public double basePowerOutputMk1;
        [KSPField] public double basePowerOutputMk2;
        [KSPField] public double basePowerOutputMk3;
        [KSPField] public double basePowerOutputMk4;
        [KSPField] public double basePowerOutputMk5;
        [KSPField] public double basePowerOutputMk6;
        [KSPField] public double basePowerOutputMk7;

        [KSPField] public double fusionEnergyGainFactorMk1 = 10;
        [KSPField] public double fusionEnergyGainFactorMk2;
        [KSPField] public double fusionEnergyGainFactorMk3;
        [KSPField] public double fusionEnergyGainFactorMk4;
        [KSPField] public double fusionEnergyGainFactorMk5;
        [KSPField] public double fusionEnergyGainFactorMk6;
        [KSPField] public double fusionEnergyGainFactorMk7;

        [KSPField] public string fuelModeTechReqLevel2;
        [KSPField] public string fuelModeTechReqLevel3;
        [KSPField] public string fuelModeTechReqLevel4;
        [KSPField] public string fuelModeTechReqLevel5;
        [KSPField] public string fuelModeTechReqLevel6;
        [KSPField] public string fuelModeTechReqLevel7;

        [KSPField(groupName = UpgradesGroup, groupDisplayName = UpgradesGroupDisplayName, guiActive = false, guiActiveEditor = true, guiName = "#LOC_KSPIE_Reactor_powerOutputMk1", guiUnits = "#LOC_KSPIE_Reactor_megawattUnit", guiFormat = "F2")]
        public double powerOutputMk1;
        [KSPField(groupName = UpgradesGroup, groupDisplayName = UpgradesGroupDisplayName, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KSPIE_Reactor_powerOutputMk2", guiUnits = "#LOC_KSPIE_Reactor_megawattUnit", guiFormat = "F2")]
        public double powerOutputMk2;
        [KSPField(groupName = UpgradesGroup, groupDisplayName = UpgradesGroupDisplayName, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KSPIE_Reactor_powerOutputMk3", guiUnits = "#LOC_KSPIE_Reactor_megawattUnit", guiFormat = "F2")]
        public double powerOutputMk3;
        [KSPField(groupName = UpgradesGroup, groupDisplayName = UpgradesGroupDisplayName, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KSPIE_Reactor_powerOutputMk4", guiUnits = "#LOC_KSPIE_Reactor_megawattUnit", guiFormat = "F2")]
        public double powerOutputMk4;
        [KSPField(groupName = UpgradesGroup, groupDisplayName = UpgradesGroupDisplayName, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KSPIE_Reactor_powerOutputMk5", guiUnits = "#LOC_KSPIE_Reactor_megawattUnit", guiFormat = "F2")]
        public double powerOutputMk5;
        [KSPField(groupName = UpgradesGroup, groupDisplayName = UpgradesGroupDisplayName, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KSPIE_Reactor_powerOutputMk6", guiUnits = "#LOC_KSPIE_Reactor_megawattUnit", guiFormat = "F2")]
        public double powerOutputMk6;
        [KSPField(groupName = UpgradesGroup, groupDisplayName = UpgradesGroupDisplayName, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KSPIE_Reactor_powerOutputMk7", guiUnits = "#LOC_KSPIE_Reactor_megawattUnit", guiFormat = "F2")]
        public double powerOutputMk7;

        // Settings
        [KSPField] public double neutronsExhaustRadiationMult = 16;
        [KSPField] public double gammaRayExhaustRadiationMult = 4;
        [KSPField] public double neutronScatteringRadiationMult = 20;

        [KSPField] public bool showEngineConnectionInfo = true;
        [KSPField] public bool showPowerGeneratorConnectionInfo = true;
        [KSPField] public bool mayExhaustInAtmosphereHomeworld = true;
        [KSPField] public bool mayExhaustInLowSpaceHomeworld = true;
        [KSPField] public double minThermalNozzleTempRequired;
        [KSPField] public bool canUseAllPowerForPlasma = true;
        [KSPField] public bool updateModuleCost = true;
        [KSPField] public int minCoolingFactor = 1;
        [KSPField] public double engineHeatProductionMult = 1;
        [KSPField] public double plasmaHeatProductionMult = 1;
        [KSPField] public double engineWasteheatProductionMult = 1;
        [KSPField] public double plasmaWasteheatProductionMult = 1;
        [KSPField] public bool supportMHD;
        [KSPField] public int reactorModeTechBonus;
        [KSPField] public bool canBeCombinedWithLab;
        [KSPField] public bool canBreedTritium;
        [KSPField] public bool canDisableTritiumBreeding = true;
        [KSPField] public bool showShutDownInFlight;
        [KSPField] public bool showForcedMinimumThrottle;
        [KSPField] public bool showPowerPercentage = true;
        [KSPField] public double powerScaleExponent = 3;
        [KSPField] public double costScaleExponent = 1.86325;
        [KSPField] public double breedDivider = 100000;
        [KSPField] public double maxThermalNozzleIsp = 2997.13f;
        [KSPField] public double effectivePowerMultiplier;
        [KSPField] public double bonusBufferFactor = 0.05;
        [KSPField] public double thermalPowerBufferMult = 4;
        [KSPField] public double chargedPowerBufferMult = 4;
        [KSPField] public double massCoreTempExp;
        [KSPField] public double massPowerExp;
        [KSPField] public double heatTransportationEfficiency = 0.9;
        [KSPField] public double ReactorTemp;
        [KSPField] public double powerOutputMultiplier = 1;
        [KSPField] public double upgradedReactorTemp;
        [KSPField] public string animName = "";
        [KSPField] public double animExponent = 1;
        [KSPField] public string loopingAnimationName = "";
        [KSPField] public string startupAnimationName = "";
        [KSPField] public string shutdownAnimationName = "";
        [KSPField] public double reactorSpeedMult = 1;
        [KSPField] public double powerRatio;
        [KSPField] public string upgradedName = "";
        [KSPField] public string originalName = "";

        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActiveEditor = true, guiActive = false, guiFormat = "F2", guiName = "#LOC_KSPIE_Reactor_connectionRadius")]
        public double radius = 2.5;

        [KSPField] public double minimumThrottle;
        [KSPField] public bool canShutdown = true;
        [KSPField] public int reactorType;
        [KSPField] public double fuelEfficiency = 1;
        [KSPField] public bool containsPowerGenerator;
        [KSPField] public double fuelUsePerMJMult = 1;
        [KSPField] public double wasteHeatMultiplier = 1;
        [KSPField] public double wasteHeatBufferMassMult = 2.0e+5;
        [KSPField] public double wasteHeatBufferMult = 1;
        [KSPField] public double hotBathTemperature;
        [KSPField] public bool usePropellantBaseIsp;
        [KSPField] public double emergencyPowerShutdownFraction = 0.99;
        [KSPField] public double thermalPropulsionEfficiency = 1;
        [KSPField] public double plasmaPropulsionEfficiency = 1;
        [KSPField] public double chargedParticlePropulsionEfficiency = 1;

        [KSPField] public double thermalEnergyEfficiency = 1;
        [KSPField] public double chargedParticleEnergyEfficiency = 1;
        [KSPField] public double plasmaEnergyEfficiency = 1;
        [KSPField] public double maxGammaRayPower;

        [KSPField] public double maxChargedParticleUtilisationRatio = 1;
        [KSPField] public double maxChargedParticleUtilisationRatioMk1 = 1;
        [KSPField] public double maxChargedParticleUtilisationRatioMk2 = 1;
        [KSPField] public double maxChargedParticleUtilisationRatioMk3 = 1;
        [KSPField] public double maxChargedParticleUtilisationRatioMk4 = 1;
        [KSPField] public double maxChargedParticleUtilisationRatioMk5 = 1;

        [KSPField] public string maxChargedParticleUtilisationTechMk2;
        [KSPField] public string maxChargedParticleUtilisationTechMk3;
        [KSPField] public string maxChargedParticleUtilisationTechMk4;
        [KSPField] public string maxChargedParticleUtilisationTechMk5;

        [KSPField] public bool hasBuoyancyEffects = true;
        [KSPField] public double geeForceMultiplier = 0.1;
        [KSPField] public double geeForceThreshHold = 9;
        [KSPField] public double geeForceExponent = 2;
        [KSPField] public double minGeeForceModifier = 0.01;

        [KSPField] public bool hasOverheatEffects = true;
        [KSPField] public double overheatMultiplier = 10;
        [KSPField] public double overheatThreshHold = 0.95;
        [KSPField] public double overheatExponent = 2;
        [KSPField] public double minOverheatModifier = 0.01;

        [KSPField] public string soundRunningFilePath = "";
        [KSPField] public double soundRunningPitchMin = 0.4;
        [KSPField] public double soundRunningPitchExp;
        [KSPField] public double soundRunningVolumeExp;
        [KSPField] public double soundRunningVolumeMin;

        [KSPField] public string soundTerminateFilePath = "";
        [KSPField] public string soundInitiateFilePath = "";
        [KSPField] public double neutronEmbrittlementLifepointsMax = 100;
        [KSPField] public double neutronEmbrittlementDivider = 1e+9;
        [KSPField] public double hotBathModifier = 1;
        [KSPField] public double thermalProcessingModifier = 1;
        [KSPField] public int supportedPropellantAtoms = GameConstants.DefaultSupportedPropellantAtoms;
        [KSPField] public int supportedPropellantTypes = GameConstants.DefaultSupportedPropellantTypes;
        [KSPField] public bool fullPowerForNonNeutronAbsorbents = true;
        [KSPField] public bool showPowerPriority = true;
        [KSPField] public bool showSpecialisedUI = true;
        [KSPField] public bool canUseNeutronicFuels = true;
        [KSPField] public bool canUseGammaRayFuels = true;
        [KSPField] public double maxNeutronsRatio = 1.04;
        [KSPField] public double minNeutronsRatio;

        [KSPField] public int FuelModeTechLevel;
        [KSPField] public string BiModelUpgradeTechReq = string.Empty;
        [KSPField] public string PowerUpgradeTechReq = string.Empty;
        [KSPField] public double PowerUpgradeCoreTempMult = 1;

        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = false, guiActive = false, guiActiveEditor = true, guiName = "#LOC_KSPIE_Reactor_rawPowerOutput", guiUnits = "#LOC_KSPIE_Reactor_megawattUnit", guiFormat = "F2")]
        public double CurrentRawPowerOutput;

        [KSPField] public double PowerOutput;
        [KSPField] public double UpgradedPowerOutput;
        [KSPField] public string UpgradeTechReq = string.Empty;
        [KSPField] public bool shouldApplyBalance;
        [KSPField] public double TritiumMolarMassRatio = 3.0160 / 7.0183;
        [KSPField] public double HeliumMolarMassRatio = 4.0023 / 7.0183;

        // GUI strings
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = false, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KSPIE_Reactor_reactorStatus")]
        public string StatusStr = string.Empty;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = false, guiActive = false, guiActiveEditor = true, guiName = "#LOC_KSPIE_Reactor_coreTemperature")]
        public string CoretempStr = string.Empty;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = false, guiActive = true, guiActiveEditor = true, guiName = "#LOC_KSPIE_Reactor_reactorFuelMode")]
        public string FuelModeStr = string.Empty;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = false, guiActive = false, guiActiveEditor = true, guiName = "#LOC_KSPIE_Reactor_connectedRecievers")]
        public string ConnectedReceiversStr = string.Empty;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = false, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KSPIE_Reactor_reactorSurface", guiUnits = " m\xB3")]
        public double ReactorSurface;

        [KSPField] protected double MaxPowerToSupply;
        [KSPField] protected double RequestedThermalToSupplyPerSecond;
        [KSPField] protected double MaxThermalToSupplyPerSecond;
        [KSPField] protected double RequestedChargedToSupplyPerSecond;
        [KSPField] protected double MaxChargedToSupplyPerSecond;
        [KSPField] protected double MinThrottle;
        [KSPField] public double MassCostExponent = 2.5;

        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActiveEditor = false, guiName = "#LOC_KSPIE_Reactor_InitialCost")]//Initial Cost
        public double InitialCost;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActiveEditor = false, guiName = "#LOC_KSPIE_Reactor_CalculatedCost")]//Calculated Cost
        public double CalculatedCost;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActiveEditor = false, guiName = "#LOC_KSPIE_Reactor_MaxResourceCost")]//Max Resource Cost
        public double MaxResourceCost;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActiveEditor = false, guiName = "#LOC_KSPIE_Reactor_ModuleCost")]//Module Cost
        public float ModuleCost;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActiveEditor = false, guiName = "#LOC_KSPIE_Reactor_NeutronEmbrittlementCost")]//Neutron Embrittlement Cost
        public double NeutronEmbrittlementCost;

        // Gui
        [KSPField(guiActive = false, guiActiveEditor = false)]
        public float MassDifference;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KSPIE_Reactor_CalibratedMass", guiUnits = " t")]//calibrated mass
        public float PartMass;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiActiveEditor = true, guiName = "#LOC_KSPIE_Reactor_reactorMass", guiFormat = "F3", guiUnits = " t")]
        public float CurrentMass;
        [KSPField]
        public double MaximumThermalPowerEffective;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KSPIE_Reactor_EmbrittlementFraction", guiFormat = "F4")]//Embrittlement Fraction
        public double EmbrittlementModifier;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KSPIE_Reactor_BuoyancyFraction", guiFormat = "F4")]//Buoyancy Fraction
        public double GeeForceModifier = 1;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KSPIE_Reactor_OverheatFraction", guiFormat = "F4")]//Overheat Fraction
        public double OverheatModifier = 1;

        [KSPField] public double LithiumNeutronAbsorption = 1;
        [KSPField] public bool IsConnectedToThermalGenerator;
        [KSPField] public bool IsConnectedToChargedGenerator;

        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "#LOC_KSPIE_Reactor_reactorControlWindow"), UI_Toggle(disabledText = "#LOC_KSPIE_Reactor_reactorControlWindow_Hidden", enabledText = "#LOC_KSPIE_Reactor_reactorControlWindow_Shown", affectSymCounterparts = UI_Scene.None)]//Hidden-Shown
        public bool RenderWindow;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = true, guiActiveEditor = true, guiName = "#LOC_KSPIE_Reactor_startEnabled"), UI_Toggle(disabledText = "#LOC_KSPIE_Reactor_startEnabled_True", enabledText = "#LOC_KSPIE_Reactor_startEnabled_False")]//True-False
        public bool StartDisabled;

        // This field is NOT used for catch up processing, it is used to determine
        // if the reactor can be refueled / decay has stopped, etc.
        [KSPField] public double LastActiveTime;

        // shared variables
        protected bool DecayOngoing;
        protected bool Initialized;
        protected bool MessagedRanOutOfFuel;

        protected double CurrentGeeForce;
        protected double AnimationStarted;
        protected double PowerPercent;
        protected double TotalAmountLithium;
        protected double TotalMaxAmountLithium;

        protected GUIStyle BoldStyle;
        protected GUIStyle TextStyle;
        protected List<ReactorFuelType> FuelModes;
        protected List<ReactorFuelMode> CurrentFuelVariantsSorted;
        protected ReactorFuelMode CurrentFuelVariant;
        protected AnimationState[] PulseAnimation;
        protected ModuleAnimateGeneric StartupAnimation;
        protected ModuleAnimateGeneric ShutdownAnimation;
        protected ModuleAnimateGeneric LoopingAnimation;

        private FNHabitat _centrifugeHabitat;
        private Rect _windowPosition;
        private ReactorFuelType _currentFuelMode;
        private PartResourceDefinition _lithium6Def;
        private PartResourceDefinition _tritiumDef;
        private PartResourceDefinition _heliumDef;
        private PartResourceDefinition _hydrogenDefinition;
        private FNEmitterController _emitterController;

        private readonly List<ReactorProduction> _reactorProduction = new List<ReactorProduction>();
        private readonly List<IFnEngineNozzle> _connectedEngines = new List<IFnEngineNozzle>();
        private readonly Queue<double> _averageGeeforce = new Queue<double>();
        private readonly Queue<double> _averageOverheat = new Queue<double>();

        private AudioSource _initiateSound;
        private AudioSource _terminateSound;
        private AudioSource _runningSound;

        private double _tritiumDensity;
        private double _helium4Density;
        private double _lithium6Density;

        private double _propulsionRequestRatioSum;
        private double _consumedFuelTotalFixed;
        private double _connectedReceiversSum;
        private double _previousReactorPowerRatio;

        private double _currentThermalEnergyGeneratorMass;
        private double _currentChargedEnergyGeneratorMass;

        private double _tritiumBreedingMassAdjustment;
        private double _heliumBreedingMassAdjustment;
        private double _staticBreedRate;

        private double _currentIsThermalEnergyGeneratorEfficiency;
        private double _currentIsChargedEnergyGeneratorEfficiency;
        private double _currentIsPlasmaEnergyGeneratorEfficiency;

        private double _currentGeneratorThermalEnergyRequestRatio;
        private double _currentGeneratorPlasmaEnergyRequestRatio;
        private double _currentGeneratorChargedEnergyRequestRatio;

        private double _lithiumConsumedPerSecond;
        private double _tritiumProducedPerSecond;
        private double _heliumProducedPerSecond;

        private int _windowId = 90175467;
        private int _deactivateTimer;
        private int _chargedParticleUtilisationLevel = 1;

        bool _hasSpecificFuelModeTechs;
        bool? _hasBiModelUpgradeTechReq;

        // properties
        public double ForcedMinimumThrottleRatio => ((double)(decimal)forcedMinimumThrottle) / 100;

        public int SupportedPropellantAtoms => supportedPropellantAtoms;

        public int SupportedPropellantTypes => supportedPropellantTypes;

        public bool FullPowerForNonNeutronAbsorbents => fullPowerForNonNeutronAbsorbents;

        public double EfficiencyConnectedThermalEnergyGenerator => storedIsThermalEnergyGeneratorEfficiency;

        public double EfficiencyConnectedChargedEnergyGenerator => storedIsChargedEnergyGeneratorEfficiency;

        public double FuelRatio => fuel_ratio;

        public virtual double MagneticNozzlePowerMult => 1;

        public bool MayExhaustInAtmosphereHomeworld => mayExhaustInAtmosphereHomeworld;

        public bool MayExhaustInLowSpaceHomeworld => mayExhaustInLowSpaceHomeworld;

        public double MinThermalNozzleTempRequired => minThermalNozzleTempRequired;

        public virtual double CurrentMeVPerChargedProduct => _currentFuelMode?.MeVPerChargedProduct ?? 0;

        public bool UsePropellantBaseIsp => usePropellantBaseIsp;

        public bool CanUseAllPowerForPlasma => canUseAllPowerForPlasma;

        public double MinCoolingFactor => minCoolingFactor;

        public double EngineHeatProductionMultiplier => engineHeatProductionMult;

        public double PlasmaHeatProductionMultiplier => plasmaHeatProductionMult;

        public double EngineWasteheatProductionMultiplier => engineWasteheatProductionMult;

        public double PlasmaWasteheatProductionMultiplier => plasmaWasteheatProductionMult;

        public double ThermalPropulsionWasteHeatModifier => 1;

        public double ConsumedFuelFixed => _consumedFuelTotalFixed;

        public bool SupportMHD => supportMHD;

        public double Radius => radius;

        public bool IsThermalSource => true;

        public double ThermalProcessingModifier => thermalProcessingModifier;

        public Part Part => part;

        public double ProducedWasteHeat => ongoing_total_power_generated;

        public double ProducedThermalHeat => ongoing_thermal_power_generated;

        public double ProducedChargedPower => ongoing_charged_power_generated;

        public int ProviderPowerPriority => (int)electricPowerPriority;

        public double ThermalTransportationEfficiency => heatTransportationEfficiency;

        public double RawTotalPowerProduced => ongoing_total_power_generated;

        public GenerationType CurrentGenerationType => (GenerationType)currentGenerationType;

        public GenerationType FuelModeTechGeneration => (GenerationType)FuelModeTechLevel;

        public double ChargedParticlePropulsionEfficiency => chargedParticlePropulsionEfficiency * maxChargedParticleUtilisationRatio;

        public double PlasmaPropulsionEfficiency => plasmaPropulsionEfficiency * maxChargedParticleUtilisationRatio;

        public double ThermalPropulsionEfficiency => thermalPropulsionEfficiency;

        public double ThermalEnergyEfficiency => thermalEnergyEfficiency;

        public double PlasmaEnergyEfficiency => plasmaEnergyEfficiency;

        public double ChargedParticleEnergyEfficiency => chargedParticleEnergyEfficiency;

        public bool IsSelfContained => containsPowerGenerator;

        public String UpgradeTechnology => UpgradeTechReq;

        public double PowerBufferBonus => bonusBufferFactor;

        public double RawMaximumPowerForPowerGeneration => RawPowerOutput;

        public double RawMaximumPower => RawPowerOutput;

        public virtual double ReactorEmbrittlementConditionRatio => Math.Min(Math.Max(1 - (neutronEmbrittlementDamage / neutronEmbrittlementLifepointsMax), maxEmbrittlementFraction), 1);

        public virtual double NormalisedMaximumPower => RawPowerOutput * EffectiveEmbrittlementEffectRatio * (CurrentFuelMode?.NormalisedReactionRate ?? 1);

        public virtual double MinimumPower => MaximumPower * MinimumThrottle;

        public virtual double MaximumThermalPower => PowerRatio * NormalisedMaximumPower * ThermalPowerRatio * GeeForceModifier * OverheatModifier;

        public virtual double MaximumChargedPower => PowerRatio * NormalisedMaximumPower * ChargedPowerRatio * GeeForceModifier * OverheatModifier;

        public double ReactorSpeedMult => reactorSpeedMult;

        public virtual bool CanProducePower => stored_fuel_ratio > 0;

        public virtual bool IsNuclear => false;

        public virtual bool IsActive => IsEnabled;

        public virtual bool IsVolatileSource => false;

        public virtual bool IsFuelNeutronRich => false;

        public virtual double MaximumPower => MaximumThermalPower + MaximumChargedPower;

        public virtual double StableMaximumReactorPower => IsEnabled ? NormalisedMaximumPower : 0;

        public int ReactorFuelModeTechLevel => FuelModeTechLevel + reactorModeTechBonus;

        public int ReactorType => reactorType;

        public virtual string TypeName => part.partInfo.title;

        public virtual double ChargedPowerRatio => CurrentFuelMode?.ChargedPowerRatio ?? 0;

        public double ThermalPowerRatio => 1 - ChargedPowerRatio;

        public IElectricPowerGeneratorSource ConnectedThermalElectricGenerator { get; set; }

        public IElectricPowerGeneratorSource ConnectedChargedParticleElectricGenerator { get; set; }

        private void DetermineChargedParticleUtilizationRatio()
        {
            if (PluginHelper.UpgradeAvailable(maxChargedParticleUtilisationTechMk2))
                _chargedParticleUtilisationLevel++;
            if (PluginHelper.UpgradeAvailable(maxChargedParticleUtilisationTechMk3))
                _chargedParticleUtilisationLevel++;
            if (PluginHelper.UpgradeAvailable(maxChargedParticleUtilisationTechMk4))
                _chargedParticleUtilisationLevel++;
            if (PluginHelper.UpgradeAvailable(maxChargedParticleUtilisationTechMk5))
                _chargedParticleUtilisationLevel++;

            if (_chargedParticleUtilisationLevel == 1)
                maxChargedParticleUtilisationRatio = maxChargedParticleUtilisationRatioMk1;
            else if (_chargedParticleUtilisationLevel == 2)
                maxChargedParticleUtilisationRatio = maxChargedParticleUtilisationRatioMk2;
            else if (_chargedParticleUtilisationLevel == 3)
                maxChargedParticleUtilisationRatio = maxChargedParticleUtilisationRatioMk3;
            else if (_chargedParticleUtilisationLevel == 4)
                maxChargedParticleUtilisationRatio = maxChargedParticleUtilisationRatioMk4;
            else
                maxChargedParticleUtilisationRatio = maxChargedParticleUtilisationRatioMk5;
        }

        public ReactorFuelType CurrentFuelMode
        {
            get
            {
                if (_currentFuelMode != null)
                    return _currentFuelMode;

                Debug.Log("[KSPI]: CurrentFuelMode setting default FuelMode");
                SetDefaultFuelMode();

                return _currentFuelMode;
            }
            set
            {
                _currentFuelMode = value;
                MaxPowerToSupply = Math.Max(MaximumPower * TimeWarp.fixedDeltaTime, 0);
                CurrentFuelVariantsSorted = _currentFuelMode.GetVariantsOrderedByFuelRatio(part, FuelEfficiency, MaxPowerToSupply, fuelUsePerMJMult);
                CurrentFuelVariant = CurrentFuelVariantsSorted.First();

                // persist
                fuelmode_index = _currentFuelMode.Index;
                fuel_mode_name = _currentFuelMode.ModeGUIName;
                fuel_mode_variant = CurrentFuelVariant.Name;
            }
        }

        public double PowerRatio
        {
            get
            {
                powerRatio = ((double)(decimal)powerPercentage) / 100;

                return powerRatio;
            }
        }

        public float GetModuleCost(float defaultCost, ModifierStagingSituation sit)
        {
            NeutronEmbrittlementCost = CalculatedCost * Math.Pow((neutronEmbrittlementDamage / neutronEmbrittlementLifepointsMax), 0.5);

            MaxResourceCost = part.Resources.Sum(m => m.maxAmount * m.info.unitCost);

            var dryCost = CalculatedCost - InitialCost;

            ModuleCost = updateModuleCost ? (float)(MaxResourceCost + dryCost - NeutronEmbrittlementCost) : 0;

            return ModuleCost;
        }


        public ModifierChangeWhen GetModuleCostChangeWhen()
        {
            return ModifierChangeWhen.STAGED;
        }

        public void UseProductForPropulsion(IResourceManager resMan, double ratio, double propellantMassPerSecond)
        {
            UseProductForPropulsion(resMan, ratio, propellantMassPerSecond, _hydrogenDefinition);
        }

        public void UseProductForPropulsion(IResourceManager resMan, double ratio, double propellantMassPerSecond, PartResourceDefinition resource)
        {
            if (ratio <= 0) return;

            ResourceName resID;

            foreach (var product in _reactorProduction)
            {
                if (product.Mass <= 0) continue;

                var effectiveMass = ratio * product.Mass;

                // remove product from store
                var fuelAmount = product.FuelMode.DensityInTon > 0 ? (effectiveMass / product.FuelMode.DensityInTon) : 0;
                if (fuelAmount == 0) continue;

                resID = KITResourceSettings.NameToResource(product.FuelMode.ResourceName);
                if (resID == ResourceName.Unknown)
                {
                    part.RequestResource(product.FuelMode.ResourceName, fuelAmount * resMan.FixedDeltaTime());
                }
                else
                {
                    resMan.Consume(resID, fuelAmount);
                }

            }
            resID = KITResourceSettings.NameToResource(resource.name);
            if (resID == ResourceName.Unknown)
            {
                part.RequestResource(resource.name, -propellantMassPerSecond * TimeWarp.fixedDeltaTime / resource.density, ResourceFlowMode.ALL_VESSEL);
            }
            else
            {
                resMan.Produce(resID, propellantMassPerSecond / resource.density);
            }
        }

        public void ConnectWithEngine(IEngineNozzle engine)
        {
            Debug.Log("[KSPI]: ConnectWithEngine ");

            var fnEngine = engine as IFnEngineNozzle;
            if (fnEngine == null)
            {
                Debug.LogError("[KSPI]: engine is not a IFnEngineNozzle");
                return;
            }

            if (!_connectedEngines.Contains(fnEngine))
                _connectedEngines.Add(fnEngine);
        }

        public void DisconnectWithEngine(IEngineNozzle engine)
        {
            Debug.Log("[KSPI]: DisconnectWithEngine ");

            var fnEngine = engine as IFnEngineNozzle;
            if (fnEngine == null)
            {
                Debug.LogError("[KSPI]: engine is not a IFnEngineNozzle");
                return;
            }

            if (_connectedEngines.Contains(fnEngine))
                _connectedEngines.Remove(fnEngine);
        }


        public double FusionEnergyGainFactor
        {
            get
            {
                switch (FuelModeTechGeneration)
                {
                    case GenerationType.Mk7:
                        return fusionEnergyGainFactorMk7;
                    case GenerationType.Mk6:
                        return fusionEnergyGainFactorMk6;
                    case GenerationType.Mk5:
                        return fusionEnergyGainFactorMk5;
                    case GenerationType.Mk4:
                        return fusionEnergyGainFactorMk4;
                    case GenerationType.Mk3:
                        return fusionEnergyGainFactorMk3;
                    case GenerationType.Mk2:
                        return fusionEnergyGainFactorMk2;
                    default:
                        return fusionEnergyGainFactorMk1;
                }
            }
        }

        public virtual double MinimumThrottle
        {
            get
            {
                double leveledThrottle;

                switch (CurrentGenerationType)
                {
                    case GenerationType.Mk7:
                        leveledThrottle = minimumThrottleMk7;
                        break;
                    case GenerationType.Mk6:
                        leveledThrottle = minimumThrottleMk6;
                        break;
                    case GenerationType.Mk5:
                        leveledThrottle = minimumThrottleMk5;
                        break;
                    case GenerationType.Mk4:
                        leveledThrottle = minimumThrottleMk4;
                        break;
                    case GenerationType.Mk3:
                        leveledThrottle = minimumThrottleMk3;
                        break;
                    case GenerationType.Mk2:
                        leveledThrottle = minimumThrottleMk2;
                        break;
                    case GenerationType.Mk1:
                        leveledThrottle = minimumThrottleMk1;
                        break;
                    default:
                        leveledThrottle = minimumThrottleMk7;
                        break;
                }

                return leveledThrottle;
            }
        }


        public void NotifyActiveThermalEnergyGenerator(double efficiency, double powerRatio, bool isMHD, double mass)
        {
            _currentThermalEnergyGeneratorMass = mass;

            if (isMHD)
            {
                _currentIsPlasmaEnergyGeneratorEfficiency = efficiency;
                _currentGeneratorPlasmaEnergyRequestRatio = powerRatio;
            }
            else
            {
                _currentIsThermalEnergyGeneratorEfficiency = efficiency;
                _currentGeneratorThermalEnergyRequestRatio = powerRatio;
            }
            IsConnectedToThermalGenerator = true;
        }

        public void NotifyActiveThermalEnergyGenerator(double efficiency, double powerRatio)
        {
            _currentIsThermalEnergyGeneratorEfficiency = efficiency;
            _currentGeneratorThermalEnergyRequestRatio = powerRatio;
            IsConnectedToThermalGenerator = true;
        }

        public void NotifyActiveChargedEnergyGenerator(double efficiency, double powerRatio)
        {
            _currentIsChargedEnergyGeneratorEfficiency = efficiency;
            _currentGeneratorChargedEnergyRequestRatio = powerRatio;
            IsConnectedToChargedGenerator = true;
        }

        public void NotifyActiveChargedEnergyGenerator(double efficiency, double powerRatio, double mass)
        {
            _currentChargedEnergyGeneratorMass = mass;
            _currentIsChargedEnergyGeneratorEfficiency = efficiency;
            _currentGeneratorChargedEnergyRequestRatio = powerRatio;
            IsConnectedToChargedGenerator = true;
        }

        public bool ShouldApplyBalance(ElectricGeneratorType generatorType)
        {
            shouldApplyBalance = IsConnectedToThermalGenerator && IsConnectedToChargedGenerator;
            return shouldApplyBalance;
        }

        /*
        public void AttachThermalReceiver(Guid key, double radius)
        {
            if (!connectedReceivers.ContainsKey(key))
                connectedReceivers.Add(key, radius);
            UpdateConnectedReceiversStr();
        }

        public void DetachThermalReceiver(Guid key)
        {
            if (connectedReceivers.ContainsKey(key))
                connectedReceivers.Remove(key);
            UpdateConnectedReceiversStr();
        }
        */


        public virtual void OnRescale(ScalingFactor factor)
        {
            try
            {
                // calculate multipliers
                factorAbsoluteLinear = (double)(decimal)factor.absolute.linear;
                Debug.Log("[KSPI]: InterstellarReactor.OnRescale called with " + factorAbsoluteLinear);
                storedPowerMultiplier = Math.Pow(factorAbsoluteLinear, powerScaleExponent);

                InitialCost = part.partInfo.cost * Math.Pow(factorAbsoluteLinear, MassCostExponent);
                CalculatedCost = part.partInfo.cost * Math.Pow(factorAbsoluteLinear, costScaleExponent);

                // update power
                DeterminePowerOutput();

                // refresh generators mass
                ConnectedThermalElectricGenerator?.Refresh();
                ConnectedChargedParticleElectricGenerator?.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError("[KSPI]: InterstellarReactor.OnRescale" + e.Message);
            }
        }

        private void UpdateConnectedReceiversStr()
        {
            /*
            if (connectedReceivers == null) return;

            _connectedReceiversSum = connectedReceivers.Sum(r => r.Value * r.Value);
            connectedReceiversFraction.Clear();
            foreach (var pair in connectedReceivers)
                connectedReceiversFraction[pair.Key] = pair.Value * pair.Value / _connectedReceiversSum;

            reactorSurface = Math.Pow(radius, 2);
            connectedRecieversStr = connectedReceivers.Count() + " (" + _connectedReceiversSum.ToString("0.000") + " m2)";
            */
        }

        public bool HasBimodelUpgradeTechReq
        {
            get
            {
                if (_hasBiModelUpgradeTechReq == null)
                    _hasBiModelUpgradeTechReq = PluginHelper.HasTechRequirementOrEmpty(BiModelUpgradeTechReq);
                return (bool)_hasBiModelUpgradeTechReq;
            }
        }

        public virtual double FuelEfficiency
        {
            get
            {
                double baseEfficiency;
                switch (CurrentGenerationType)
                {
                    case GenerationType.Mk7:
                        baseEfficiency = fuelEfficiencyMk7;
                        break;
                    case GenerationType.Mk6:
                        baseEfficiency = fuelEfficiencyMk6;
                        break;
                    case GenerationType.Mk5:
                        baseEfficiency = fuelEfficiencyMk5;
                        break;
                    case GenerationType.Mk4:
                        baseEfficiency = fuelEfficiencyMk4;
                        break;
                    case GenerationType.Mk3:
                        baseEfficiency = FuelEfficiencyMk3;
                        break;
                    case GenerationType.Mk2:
                        baseEfficiency = fuelEfficiencyMk2;
                        break;
                    default:
                        baseEfficiency = FuelEfficiencyMk1;
                        break;
                }

                return baseEfficiency * CurrentFuelMode.FuelEfficiencyMultiplier;
            }
        }

        public virtual double CoreTemperature
        {
            get
            {
                double baseCoreTemperature;
                switch (CurrentGenerationType)
                {
                    case GenerationType.Mk7:
                        baseCoreTemperature = coreTemperatureMk7;
                        break;
                    case GenerationType.Mk6:
                        baseCoreTemperature = coreTemperatureMk6;
                        break;
                    case GenerationType.Mk5:
                        baseCoreTemperature = coreTemperatureMk5;
                        break;
                    case GenerationType.Mk4:
                        baseCoreTemperature = coreTemperatureMk4;
                        break;
                    case GenerationType.Mk3:
                        baseCoreTemperature = coreTemperatureMk3;
                        break;
                    case GenerationType.Mk2:
                        baseCoreTemperature = coreTemperatureMk2;
                        break;
                    default:
                        baseCoreTemperature = coreTemperatureMk1;
                        break;
                }

                return baseCoreTemperature * Math.Pow(OverheatModifier, 1.5) * EffectiveEmbrittlementEffectRatio * Math.Pow(part.mass / PartMass, massCoreTempExp);
            }
        }

        public virtual double MaxCoreTemperature => CoreTemperature;

        public double HotBathTemperature
        {
            get
            {
                if (hotBathTemperature <= 0)
                {
                    switch (CurrentGenerationType)
                    {
                        case GenerationType.Mk7:
                            hotBathTemperature = hotBathTemperatureMk7;
                            break;
                        case GenerationType.Mk6:
                            hotBathTemperature = hotBathTemperatureMk6;
                            break;
                        case GenerationType.Mk5:
                            hotBathTemperature = hotBathTemperatureMk5;
                            break;
                        case GenerationType.Mk4:
                            hotBathTemperature = hotBathTemperatureMk4;
                            break;
                        case GenerationType.Mk3:
                            hotBathTemperature = hotBathTemperatureMk3;
                            break;
                        case GenerationType.Mk2:
                            hotBathTemperature = hotBathTemperatureMk2;
                            break;
                        default:
                            hotBathTemperature = hotBathTemperatureMk1;
                            break;
                    }
                }

                if (hotBathTemperature <= 0)
                    return CoreTemperature * hotBathModifier;
                else
                    return hotBathTemperature;
            }
        }

        public double EffectiveEmbrittlementEffectRatio
        {
            get
            {
                EmbrittlementModifier = CheatOptions.UnbreakableJoints ? 1 : Math.Sin(ReactorEmbrittlementConditionRatio * Math.PI * 0.5);
                return EmbrittlementModifier;
            }
        }

        public double RawPowerOutput
        {
            get
            {
                double rawPowerOutput;

                switch (CurrentGenerationType)
                {
                    case GenerationType.Mk7:
                        rawPowerOutput = powerOutputMk7;
                        break;
                    case GenerationType.Mk6:
                        rawPowerOutput = powerOutputMk6;
                        break;
                    case GenerationType.Mk5:
                        rawPowerOutput = powerOutputMk5;
                        break;
                    case GenerationType.Mk4:
                        rawPowerOutput = powerOutputMk4;
                        break;
                    case GenerationType.Mk3:
                        rawPowerOutput = powerOutputMk3;
                        break;
                    case GenerationType.Mk2:
                        rawPowerOutput = powerOutputMk2;
                        break;
                    default:
                        rawPowerOutput = powerOutputMk1;
                        break;
                }

                return rawPowerOutput;
            }
        }


        public virtual void StartReactor()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                StartDisabled = false;
            }
            else
            {
                if (IsStarted && IsNuclear) return;

                stored_fuel_ratio = 1;
                IsEnabled = true;
                if (_runningSound != null)
                    _runningSound.Play();
            }
        }

        [KSPEvent(groupName = Group, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KSPIE_Reactor_activateReactor", active = false)]
        public void ActivateReactor()
        {
            Debug.Log("[KSPI]: InterstellarReactor on " + part.name + " was Force Activated");
            part.force_activate();

            Events[nameof(ActivateReactor)].guiActive = false;
            Events[nameof(ActivateReactor)].active = false;

            if (_centrifugeHabitat != null && !_centrifugeHabitat.isDeployed)
            {
                var message = Localizer.Format("#LOC_KSPIE_Reactor_PostMsg1", part.name);
                ScreenMessages.PostScreenMessage(message, 20.0f, ScreenMessageStyle.UPPER_CENTER);
                Debug.LogWarning("[KSPI]: " + message);
                return;
            }

            StartReactor();
            IsStarted = true;
        }

        [KSPEvent(groupName = Group, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KSPIE_Reactor_deactivateReactor", active = true)]
        public void DeactivateReactor()
        {
            if (HighLogic.LoadedSceneIsEditor)
                StartDisabled = true;
            else
            {
                if (IsNuclear) return;

                IsEnabled = false;

                if (_runningSound != null)
                    _runningSound.Stop();
            }
        }

        [KSPEvent(groupName = Group, guiActive = true, guiName = "#LOC_KSPIE_Reactor_enableTritiumBreeding", active = false)]
        public void StartBreedTritiumEvent()
        {
            if (!IsFuelNeutronRich) return;

            breedtritium = true;
        }

        [KSPEvent(groupName = Group, guiActive = true, guiName = "#LOC_KSPIE_Reactor_disableTritiumBreeding", active = true)]
        public void StopBreedTritiumEvent()
        {
            breedtritium = false;
        }

        [KSPAction("#LOC_KSPIE_Reactor_activateReactor")]
        public void ActivateReactorAction(KSPActionParam param)
        {
            if (IsNuclear) return;

            StartReactor();
        }

        [KSPAction("#LOC_KSPIE_Reactor_deactivateReactor")]
        public void DeactivateReactorAction(KSPActionParam param)
        {
            if (IsNuclear) return;

            DeactivateReactor();
        }

        [KSPAction("#LOC_KSPIE_Reactor_toggleReactor")]
        public void ToggleReactorAction(KSPActionParam param)
        {
            if (IsNuclear) return;

            IsEnabled = !IsEnabled;
        }

        public void DeterminePowerOutput()
        {
            MassDifference = part.mass / PartMass;

            effectivePowerMultiplier = storedPowerMultiplier * powerOutputMultiplier * Math.Pow(MassDifference, massPowerExp);

            powerOutputMk1 = basePowerOutputMk1 * effectivePowerMultiplier;
            powerOutputMk2 = basePowerOutputMk2 * effectivePowerMultiplier;
            powerOutputMk3 = basePowerOutputMk3 * effectivePowerMultiplier;
            powerOutputMk4 = basePowerOutputMk4 * effectivePowerMultiplier;
            powerOutputMk5 = basePowerOutputMk5 * effectivePowerMultiplier;
            powerOutputMk6 = basePowerOutputMk6 * effectivePowerMultiplier;
            powerOutputMk7 = basePowerOutputMk7 * effectivePowerMultiplier;

            Fields[nameof(powerOutputMk1)].guiActiveEditor = true;
            Fields[nameof(powerOutputMk2)].guiActiveEditor = !string.IsNullOrEmpty(fuelModeTechReqLevel2);
            Fields[nameof(powerOutputMk3)].guiActiveEditor = !string.IsNullOrEmpty(fuelModeTechReqLevel3);
            Fields[nameof(powerOutputMk4)].guiActiveEditor = !string.IsNullOrEmpty(fuelModeTechReqLevel4);
            Fields[nameof(powerOutputMk5)].guiActiveEditor = !string.IsNullOrEmpty(fuelModeTechReqLevel5);
            Fields[nameof(powerOutputMk6)].guiActiveEditor = !string.IsNullOrEmpty(fuelModeTechReqLevel6);
            Fields[nameof(powerOutputMk7)].guiActiveEditor = !string.IsNullOrEmpty(fuelModeTechReqLevel7);

            // initialise power output when missing
            if (powerOutputMk2 <= 0)
                powerOutputMk2 = powerOutputMk1 * 1.5;
            if (powerOutputMk3 <= 0)
                powerOutputMk3 = powerOutputMk2 * 1.5;
            if (powerOutputMk4 <= 0)
                powerOutputMk4 = powerOutputMk3 * 1.5;
            if (powerOutputMk5 <= 0)
                powerOutputMk5 = powerOutputMk4 * 1.5;
            if (powerOutputMk6 <= 0)
                powerOutputMk6 = powerOutputMk5 * 1.5;
            if (powerOutputMk7 <= 0)
                powerOutputMk7 = powerOutputMk6 * 1.5;

            if (minimumThrottleMk1 <= 0)
                minimumThrottleMk1 = minimumThrottle;
            if (minimumThrottleMk2 <= 0)
                minimumThrottleMk2 = minimumThrottleMk1;
            if (minimumThrottleMk3 <= 0)
                minimumThrottleMk3 = minimumThrottleMk2;
            if (minimumThrottleMk4 <= 0)
                minimumThrottleMk4 = minimumThrottleMk3;
            if (minimumThrottleMk5 <= 0)
                minimumThrottleMk5 = minimumThrottleMk4;
            if (minimumThrottleMk6 <= 0)
                minimumThrottleMk6 = minimumThrottleMk5;
            if (minimumThrottleMk7 <= 0)
                minimumThrottleMk7 = minimumThrottleMk6;
        }

        public override void OnStart(StartState state)
        {
            UpdateReactorCharacteristics();

            InitializeKerbalismEmitter();

            Fields[nameof(electricPowerPriority)].OnValueModified += (a) =>
            {
                GameEvents.onPartPriorityChanged.Fire(part);
            };

            _hydrogenDefinition = PartResourceLibrary.Instance.GetDefinition(KITResourceSettings.HydrogenLqd);

            _windowPosition = new Rect(windowPositionX, windowPositionY, 300, 100);
            _hasBiModelUpgradeTechReq = PluginHelper.HasTechRequirementOrEmpty(BiModelUpgradeTechReq);
            _staticBreedRate = 1 / powerOutputMultiplier / breedDivider / GameConstants.TritiumBreedRate;

            var powerPercentageField = Fields[nameof(powerPercentage)];
            powerPercentageField.guiActive = showPowerPercentage;
            UI_FloatRange[] powerPercentageFloatRange = { powerPercentageField.uiControlFlight as UI_FloatRange, powerPercentageField.uiControlEditor as UI_FloatRange };
            powerPercentageFloatRange[0].minValue = minimumPowerPercentage;
            powerPercentageFloatRange[1].minValue = minimumPowerPercentage;

            if (!part.Resources.Contains(KITResourceSettings.ThermalPower))
            {
                var node = new ConfigNode("RESOURCE");
                node.AddValue("name", KITResourceSettings.ThermalPower);
                node.AddValue("maxAmount", PowerOutput);
                node.AddValue("amount", 0);
                part.AddResource(node);
            }

            // while in edit mode, listen to on attach/detach event
            if (state == StartState.Editor)
            {
                part.OnEditorAttach += OnEditorAttach;
                part.OnEditorDetach += OnEditorDetach;
            }

            _windowId = new System.Random(part.GetInstanceID()).Next(int.MaxValue);
            base.OnStart(state);

            // configure reactor modes
            FuelModes = GetReactorFuelModes();
            SetDefaultFuelMode();
            UpdateFuelMode();

            var myAttachedEngine = part.FindModuleImplementing<ModuleEngines>();
            var myGenerator = part.FindModuleImplementing<FNGenerator>();

            if (state == StartState.Editor)
            {
                MaximumThermalPowerEffective = MaximumThermalPower;
                CoretempStr = CoreTemperature.ToString("0") + " K";

                var displayPartData = myGenerator == null;

                Fields[nameof(radius)].guiActiveEditor = displayPartData;
                Fields[nameof(ConnectedReceiversStr)].guiActiveEditor = displayPartData;
                Fields[nameof(CurrentMass)].guiActiveEditor = displayPartData;

                return;
            }

            InitializeSounds();

            if (!reactorInit)
            {
                if (StartDisabled)
                {
                    Events[nameof(ActivateReactor)].guiActive = true;
                    Events[nameof(ActivateReactor)].active = true;
                    IsEnabled = false;
                    StartDisabled = false;
                    breedtritium = false;
                }
                else
                {
                    IsEnabled = true;
                    breedtritium = true;
                }
                reactorInit = true;
            }

            if (IsEnabled && _runningSound != null)
            {
                _previousReactorPowerRatio = reactor_power_ratio;

                if (vessel.isActiveVessel)
                    _runningSound.Play();
            }

            _tritiumDef = PartResourceLibrary.Instance.GetDefinition(KITResourceSettings.TritiumGas);
            _heliumDef = PartResourceLibrary.Instance.GetDefinition(KITResourceSettings.Helium4Gas);
            _lithium6Def = PartResourceLibrary.Instance.GetDefinition(KITResourceSettings.Lithium6);

            _tritiumDensity = _tritiumDef.density;
            _helium4Density = _heliumDef.density;
            _lithium6Density = _lithium6Def.density;

            _tritiumBreedingMassAdjustment = TritiumMolarMassRatio * _lithium6Density / _tritiumDensity;
            _heliumBreedingMassAdjustment = HeliumMolarMassRatio * _lithium6Density / _helium4Density;


            if (!string.IsNullOrEmpty(animName))
                PulseAnimation = PluginHelper.SetUpAnimation(animName, part);
            if (!string.IsNullOrEmpty(loopingAnimationName))
                LoopingAnimation = part.FindModulesImplementing<ModuleAnimateGeneric>().SingleOrDefault(m => m.animationName == loopingAnimationName);
            if (!string.IsNullOrEmpty(startupAnimationName))
                StartupAnimation = part.FindModulesImplementing<ModuleAnimateGeneric>().SingleOrDefault(m => m.animationName == startupAnimationName);
            if (!string.IsNullOrEmpty(shutdownAnimationName))
                ShutdownAnimation = part.FindModulesImplementing<ModuleAnimateGeneric>().SingleOrDefault(m => m.animationName == shutdownAnimationName);


            _centrifugeHabitat = part.FindModuleImplementing<FNHabitat>();

            // only force activate if Enabled and not with a engine model
            if (IsEnabled && myAttachedEngine == null)
            {
                Debug.Log("[KSPI]: InterstellarReactor on " + part.name + " was Force Activated");
                part.force_activate();

                Fields[nameof(heatTransportationEfficiency)].guiActiveEditor = true;
            }
            else
                Debug.Log("[KSPI]: skipped calling Force on " + part.name);

            Fields[nameof(electricPowerPriority)].guiActive = showPowerPriority;
            Fields[nameof(ReactorSurface)].guiActiveEditor = showSpecialisedUI;
            Fields[nameof(forcedMinimumThrottle)].guiActive = showForcedMinimumThrottle;
            Fields[nameof(forcedMinimumThrottle)].guiActiveEditor = showForcedMinimumThrottle;
        }

        private void InitializeSounds()
        {
            if (!string.IsNullOrWhiteSpace(soundRunningFilePath))
            {
                _runningSound = gameObject.AddComponent<AudioSource>();
                _runningSound.clip = GameDatabase.Instance.GetAudioClip(soundRunningFilePath);
                _runningSound.volume = 0;
                _runningSound.panStereo = 0;
                _runningSound.rolloffMode = AudioRolloffMode.Linear;
                _runningSound.loop = true;
                _runningSound.Stop();
            }

            //soundTerminateFilePath
            if (!string.IsNullOrWhiteSpace(soundTerminateFilePath))
            {
                _terminateSound = gameObject.AddComponent<AudioSource>();
                _terminateSound.clip = GameDatabase.Instance.GetAudioClip(soundTerminateFilePath);
                _terminateSound.volume = 0;
                _terminateSound.panStereo = 0;
                _terminateSound.rolloffMode = AudioRolloffMode.Linear;
                _terminateSound.loop = false;
                _terminateSound.Stop();
            }

            if (!string.IsNullOrWhiteSpace(soundTerminateFilePath))
            {
                _initiateSound = gameObject.AddComponent<AudioSource>();
                _initiateSound.clip = GameDatabase.Instance.GetAudioClip(soundInitiateFilePath);
                _initiateSound.volume = 0;
                _initiateSound.panStereo = 0;
                _initiateSound.rolloffMode = AudioRolloffMode.Linear;
                _initiateSound.loop = false;
                _initiateSound.Stop();
            }
        }

        private void UpdateReactorCharacteristics()
        {
            DeterminePowerGenerationType();

            DetermineFuelModeTechLevel();

            DeterminePowerOutput();

            DetermineChargedParticleUtilizationRatio();

            DetermineFuelEfficiency();

            DetermineCoreTemperature();
        }

        private void DetermineFuelModeTechLevel()
        {
            _hasSpecificFuelModeTechs =
                !string.IsNullOrEmpty(fuelModeTechReqLevel2)
                || !string.IsNullOrEmpty(fuelModeTechReqLevel3)
                || !string.IsNullOrEmpty(fuelModeTechReqLevel4)
                || !string.IsNullOrEmpty(fuelModeTechReqLevel5)
                || !string.IsNullOrEmpty(fuelModeTechReqLevel6)
                || !string.IsNullOrEmpty(fuelModeTechReqLevel7);

            if (string.IsNullOrEmpty(fuelModeTechReqLevel2))
                fuelModeTechReqLevel2 = upgradeTechReqMk2;
            if (string.IsNullOrEmpty(fuelModeTechReqLevel3))
                fuelModeTechReqLevel3 = upgradeTechReqMk3;
            if (string.IsNullOrEmpty(fuelModeTechReqLevel4))
                fuelModeTechReqLevel4 = upgradeTechReqMk4;
            if (string.IsNullOrEmpty(fuelModeTechReqLevel5))
                fuelModeTechReqLevel5 = upgradeTechReqMk5;
            if (string.IsNullOrEmpty(fuelModeTechReqLevel6))
                fuelModeTechReqLevel6 = upgradeTechReqMk6;
            if (string.IsNullOrEmpty(fuelModeTechReqLevel7))
                fuelModeTechReqLevel7 = upgradeTechReqMk7;

            FuelModeTechLevel = 0;
            if (PluginHelper.UpgradeAvailable(fuelModeTechReqLevel2))
                FuelModeTechLevel++;
            if (PluginHelper.UpgradeAvailable(fuelModeTechReqLevel3))
                FuelModeTechLevel++;
            if (PluginHelper.UpgradeAvailable(fuelModeTechReqLevel4))
                FuelModeTechLevel++;
            if (PluginHelper.UpgradeAvailable(fuelModeTechReqLevel5))
                FuelModeTechLevel++;
            if (PluginHelper.UpgradeAvailable(fuelModeTechReqLevel6))
                FuelModeTechLevel++;
            if (PluginHelper.UpgradeAvailable(fuelModeTechReqLevel7))
                FuelModeTechLevel++;
        }

        private void DetermineCoreTemperature()
        {
            // if coreTemperature is missing, first look at legacy value
            if (coreTemperatureMk1 <= 0)
                coreTemperatureMk1 = ReactorTemp;
            if (coreTemperatureMk2 <= 0)
                coreTemperatureMk2 = upgradedReactorTemp;
            if (coreTemperatureMk3 <= 0)
                coreTemperatureMk3 = upgradedReactorTemp * PowerUpgradeCoreTempMult;

            // prevent initial values
            if (coreTemperatureMk1 <= 0)
                coreTemperatureMk1 = 2500;
            if (coreTemperatureMk2 <= 0)
                coreTemperatureMk2 = coreTemperatureMk1;
            if (coreTemperatureMk3 <= 0)
                coreTemperatureMk3 = coreTemperatureMk2;
            if (coreTemperatureMk4 <= 0)
                coreTemperatureMk4 = coreTemperatureMk3;
            if (coreTemperatureMk5 <= 0)
                coreTemperatureMk5 = coreTemperatureMk4;
            if (coreTemperatureMk6 <= 0)
                coreTemperatureMk6 = coreTemperatureMk5;
            if (coreTemperatureMk7 <= 0)
                coreTemperatureMk7 = coreTemperatureMk6;
        }

        private void DetermineFuelEfficiency()
        {
            // if fuel efficiency is missing, try to use legacy value
            if (FuelEfficiencyMk1 <= 0)
                FuelEfficiencyMk1 = fuelEfficiency;

            // prevent any initial values
            if (FuelEfficiencyMk1 <= 0)
                FuelEfficiencyMk1 = 1;
            if (fuelEfficiencyMk2 <= 0)
                fuelEfficiencyMk2 = FuelEfficiencyMk1;
            if (FuelEfficiencyMk3 <= 0)
                FuelEfficiencyMk3 = fuelEfficiencyMk2;
            if (fuelEfficiencyMk4 <= 0)
                fuelEfficiencyMk4 = FuelEfficiencyMk3;
            if (fuelEfficiencyMk5 <= 0)
                fuelEfficiencyMk5 = fuelEfficiencyMk4;
            if (fuelEfficiencyMk6 <= 0)
                fuelEfficiencyMk6 = fuelEfficiencyMk5;
            if (fuelEfficiencyMk7 <= 0)
                fuelEfficiencyMk7 = fuelEfficiencyMk6;
        }

        private void DeterminePowerGenerationType()
        {
            // initialize tech requirement if missing
            if (string.IsNullOrEmpty(upgradeTechReqMk2))
                upgradeTechReqMk2 = UpgradeTechReq;
            if (string.IsNullOrEmpty(upgradeTechReqMk3))
                upgradeTechReqMk3 = PowerUpgradeTechReq;

            // determine number of upgrade techs
            currentGenerationType = 0;
            if (PluginHelper.UpgradeAvailable(upgradeTechReqMk7))
                currentGenerationType++;
            if (PluginHelper.UpgradeAvailable(upgradeTechReqMk6))
                currentGenerationType++;
            if (PluginHelper.UpgradeAvailable(upgradeTechReqMk5))
                currentGenerationType++;
            if (PluginHelper.UpgradeAvailable(upgradeTechReqMk4))
                currentGenerationType++;
            if (PluginHelper.UpgradeAvailable(upgradeTechReqMk3))
                currentGenerationType++;
            if (PluginHelper.UpgradeAvailable(upgradeTechReqMk2))
                currentGenerationType++;
        }

        /// <summary>
        /// Event handler called when part is attached to another part
        /// </summary>
        private void OnEditorAttach()
        {
            try
            {
                Debug.Log("[KSPI]: attach " + part.partInfo.title);
                foreach (var node in part.attachNodes)
                {
                    if (node.attachedPart == null) continue;

                    var generator = node.attachedPart.FindModuleImplementing<FNGenerator>();
                    if (generator != null)
                        generator.FindAndAttachToPowerSource();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[KSPI]: Reactor.OnEditorAttach " + e.Message);
            }
        }

        private void OnEditorDetach()
        {
            try
            {
                Debug.Log("[KSPI]: detach " + part.partInfo.title);

                ConnectedChargedParticleElectricGenerator?.FindAndAttachToPowerSource();
                ConnectedThermalElectricGenerator?.FindAndAttachToPowerSource();
            }
            catch (Exception e)
            {
                Debug.LogError("[KSPI]: Reactor.OnEditorDetach " + e.Message);
            }
        }

        /// <summary>
        /// is called in both VAB and in flight
        /// </summary>
        public virtual void Update()
        {
            DeterminePowerOutput();
            UpdateKerbalismEmitter();

            CurrentMass = part.mass;
            CurrentRawPowerOutput = RawPowerOutput;
            CoretempStr = CoreTemperature.ToString("0") + " K";

            Events[nameof(DeactivateReactor)].guiActive = HighLogic.LoadedSceneIsFlight && IsEnabled && (showShutDownInFlight || HighLogic.CurrentGame.Parameters.CustomParams<KITGamePlayParams>().ExtendedReactorControl);

            if (HighLogic.LoadedSceneIsEditor)
            {
                UpdateConnectedReceiversStr();
                ReactorSurface = radius * radius;
            }
        }

        protected void UpdateFuelMode()
        {
            FuelModeStr = CurrentFuelMode != null ? CurrentFuelMode.ModeGUIName : "null";
        }

        public override void OnUpdate()
        {
            Events[nameof(StartBreedTritiumEvent)].active = canDisableTritiumBreeding && canBreedTritium && !breedtritium && IsFuelNeutronRich && IsEnabled;
            Events[nameof(StopBreedTritiumEvent)].active = canDisableTritiumBreeding && canBreedTritium && breedtritium && IsFuelNeutronRich && IsEnabled;
            UpdateFuelMode();

            if (IsEnabled && CurrentFuelMode != null)
            {
                if (CheatOptions.InfinitePropellant || stored_fuel_ratio > 0.99)
                    StatusStr = Localizer.Format("#LOC_KSPIE_Reactor_status1", PowerPercent.ToString("0.0000"));//"Active (" +  + "%)"
                else if (CurrentFuelVariant != null)
                    StatusStr = "this needs fixing..."; // currentFuelVariant.ReactorFuels.OrderBy(GetFuelAvailability).First().ResourceName + " " + Localizer.Format("#LOC_KSPIE_Reactor_status2");//"Deprived" // do without resource manager.
            }
            else
            {
                StatusStr = PowerPercent > 0
                    ? Localizer.Format("#LOC_KSPIE_Reactor_status3", PowerPercent.ToString("0.0000"))
                    : Localizer.Format("#LOC_KSPIE_Reactor_status4");
            }
        }

        /// <summary>
        /// FixedUpdate is also called when not activated before OnFixedUpdate
        /// </summary>
        public void FixedUpdate()
        {
        }

        public override void OnFixedUpdate() // OnFixedUpdate is only called when (force) activated
        {

        }

        private void UpdatePlayedSound()
        {
            var scaledPitchRatio = Math.Pow(reactor_power_ratio, soundRunningPitchExp);
            var scaledVolumeRatio = Math.Pow(reactor_power_ratio, soundRunningVolumeExp);

            var pitch = soundRunningPitchMin * (1 - scaledPitchRatio) + scaledPitchRatio;
            var volume = reactor_power_ratio <= 0 ? 0 : GameSettings.SHIP_VOLUME * (soundRunningVolumeMin * (1 - scaledVolumeRatio) + scaledVolumeRatio);

            if (_runningSound != null)
            {
                _runningSound.pitch = (float)pitch;
                _runningSound.volume = (float)volume;
            }

            if (_initiateSound != null)
            {
                _initiateSound.pitch = (float)pitch;
                _initiateSound.volume = (float)volume;
            }

            if (_previousReactorPowerRatio > 0 && reactor_power_ratio <= 0)
            {
                if (_initiateSound != null && _initiateSound.isPlaying)
                    _initiateSound.Stop();
                if (_runningSound != null && _runningSound.isPlaying)
                    _runningSound.Stop();

                if (vessel.isActiveVessel && _terminateSound != null && !_terminateSound.isPlaying)
                {
                    _terminateSound.PlayOneShot(_terminateSound.clip);
                    _terminateSound.volume = GameSettings.SHIP_VOLUME;
                }
            }
            else if (_previousReactorPowerRatio <= 0 && reactor_power_ratio > 0)
            {
                if (_runningSound != null && _runningSound.isPlaying)
                    _runningSound.Stop();
                if (_terminateSound != null && _terminateSound.isPlaying)
                    _terminateSound.Stop();

                if (vessel.isActiveVessel)
                {
                    if (_initiateSound != null && !_initiateSound.isPlaying)
                    {
                        _initiateSound.PlayOneShot(_initiateSound.clip);
                        _initiateSound.volume = GameSettings.SHIP_VOLUME;
                    }
                    else if (_runningSound != null)
                        _runningSound.Play();
                }
            }
            else if (_previousReactorPowerRatio > 0 && reactor_power_ratio > 0 && _runningSound != null)
            {
                if (vessel.isActiveVessel && !_runningSound.isPlaying)
                {
                    if ((_initiateSound == null || (_initiateSound != null && !_initiateSound.isPlaying)) &&
                        (_terminateSound == null || (_terminateSound != null && !_terminateSound.isPlaying)))
                        _runningSound.Play();
                }
                else if (!vessel.isActiveVessel && _runningSound.isPlaying)
                {
                    _runningSound.Stop();
                }
            }
        }

        private void UpdateGeeforceModifier()
        {
            if (hasBuoyancyEffects && !CheatOptions.UnbreakableJoints)
            {
                _averageGeeforce.Enqueue(vessel.geeForce);
                if (_averageGeeforce.Count > 10)
                    _averageGeeforce.Dequeue();

                CurrentGeeForce = vessel.geeForce > 0 && _averageGeeforce.Any() ? _averageGeeforce.Average() : 0;

                if (vessel.situation == Vessel.Situations.ORBITING || vessel.situation == Vessel.Situations.SUB_ORBITAL || vessel.situation == Vessel.Situations.ESCAPING)
                {
                    var engines = vessel.FindPartModulesImplementing<ModuleEngines>();
                    if (engines.Any())
                    {
                        var totalThrust = engines.Sum(m => m.realIsp * m.requestedMassFlow * PhysicsGlobals.GravitationalAcceleration * Vector3d.Dot(m.part.transform.up, vessel.transform.up));
                        CurrentGeeForce = Math.Max(CurrentGeeForce, totalThrust / vessel.totalMass / PhysicsGlobals.GravitationalAcceleration);
                    }
                }

                var geeforce = double.IsNaN(CurrentGeeForce) || double.IsInfinity(CurrentGeeForce) ? 0 : CurrentGeeForce;

                var scaledGeeforce = Math.Pow(Math.Max(geeforce - geeForceThreshHold, 0) * geeForceMultiplier, geeForceExponent);

                GeeForceModifier = Math.Min(Math.Max(1 - scaledGeeforce, minGeeForceModifier), 1);
            }
            else
                GeeForceModifier = 1;
        }

        private void UpdateEmbrittlement(double thermalPlasmaRatio, double timeWarpFixedDeltaTime)
        {
            var hasActiveNeutronAbsorption = _connectedEngines.All(m => m.PropellantAbsorbsNeutrons) && thermalPlasmaRatio > 0;
            var lithiumEmbrittlementModifer = 1 - Math.Max(lithium_modifier * 0.9, hasActiveNeutronAbsorption ? 0.9 : 0);

            if (!CheatOptions.UnbreakableJoints && CurrentFuelMode.NeutronsRatio > 0 && CurrentFuelMode.NeutronsRatio > 0)
                neutronEmbrittlementDamage += 5 * lithiumEmbrittlementModifer * ongoing_total_power_generated * timeWarpFixedDeltaTime * CurrentFuelMode.NeutronsRatio / neutronEmbrittlementDivider;
        }

        private void LookForAlternativeFuelTypes()
        {
            var originalFuelMode = CurrentFuelMode;
            var originalFuelRatio = stored_fuel_ratio;

            SwitchToAlternativeFuelWhenAvailable(CurrentFuelMode.AlternativeFuelType1);
            SwitchToAlternativeFuelWhenAvailable(CurrentFuelMode.AlternativeFuelType2);
            SwitchToAlternativeFuelWhenAvailable(CurrentFuelMode.AlternativeFuelType3);
            SwitchToAlternativeFuelWhenAvailable(CurrentFuelMode.AlternativeFuelType4);
            SwitchToAlternativeFuelWhenAvailable(CurrentFuelMode.AlternativeFuelType5);

            if (stored_fuel_ratio < 0.99)
            {
                CurrentFuelMode = originalFuelMode;
                stored_fuel_ratio = originalFuelRatio;
            }
        }

        private void SwitchToAlternativeFuelWhenAvailable(string alternativeFuelTypeName)
        {
            if (stored_fuel_ratio >= 0.99)
                return;

            if (string.IsNullOrEmpty(alternativeFuelTypeName))
                return;

            // look for most advanced version
            var alternativeFuelType = FuelModes.LastOrDefault(m => m.ModeGUIName.Contains(alternativeFuelTypeName));
            if (alternativeFuelType == null)
            {
                Debug.LogWarning("[KSPI]: failed to find fuelType " + alternativeFuelTypeName);
                return;
            }

            Debug.Log("[KSPI]: searching fuelModes for alternative for fuel type " + alternativeFuelTypeName);
            var alternativeFuelVariantsSorted = alternativeFuelType.GetVariantsOrderedByFuelRatio(part, FuelEfficiency, MaxPowerToSupply, fuelUsePerMJMult);

            if (alternativeFuelVariantsSorted == null)
                return;

            var alternativeFuelVariant = alternativeFuelVariantsSorted.FirstOrDefault();
            if (alternativeFuelVariant == null)
            {
                Debug.LogError("[KSPI]: failed to find any variant for fuelType " + alternativeFuelTypeName);
                return;
            }

            if (alternativeFuelVariant.FuelRatio < 0.99)
            {
                Debug.LogWarning("[KSPI]: failed to find sufficient resource for " + alternativeFuelVariant.Name);
                return;
            }

            var message = Localizer.Format("#LOC_KSPIE_Reactor_switchingToAlternativeFuelMode") + " " + alternativeFuelType.ModeGUIName;
            Debug.Log("[KSPI]: " + message);
            ScreenMessages.PostScreenMessage(message, 20.0f, ScreenMessageStyle.UPPER_CENTER);

            CurrentFuelMode = alternativeFuelType;
            stored_fuel_ratio = CurrentFuelVariant.FuelRatio;
        }

        private void StoreGeneratorRequests(IResourceManager resMan)
        {
            storedIsThermalEnergyGeneratorEfficiency = _currentIsThermalEnergyGeneratorEfficiency;
            storedIsPlasmaEnergyGeneratorEfficiency = _currentIsPlasmaEnergyGeneratorEfficiency;
            storedIsChargedEnergyGeneratorEfficiency = _currentIsChargedEnergyGeneratorEfficiency;

            _currentIsThermalEnergyGeneratorEfficiency = 0;
            _currentIsPlasmaEnergyGeneratorEfficiency = 0;
            _currentIsChargedEnergyGeneratorEfficiency = 0;

            var previousStoredRatio = Math.Max(Math.Max(storedGeneratorThermalEnergyRequestRatio, storedGeneratorPlasmaEnergyRequestRatio), storedGeneratorChargedEnergyRequestRatio);

            storedGeneratorThermalEnergyRequestRatio = Math.Max(storedGeneratorThermalEnergyRequestRatio, previousStoredRatio);
            storedGeneratorPlasmaEnergyRequestRatio = Math.Max(storedGeneratorPlasmaEnergyRequestRatio, previousStoredRatio);
            storedGeneratorChargedEnergyRequestRatio = Math.Max(storedGeneratorChargedEnergyRequestRatio, previousStoredRatio);

            var requiredMinimumThrottle = Math.Max(MinimumThrottle, ForcedMinimumThrottleRatio);

            _currentGeneratorThermalEnergyRequestRatio = Math.Max(_currentGeneratorThermalEnergyRequestRatio, requiredMinimumThrottle);
            _currentGeneratorPlasmaEnergyRequestRatio = Math.Max(_currentGeneratorPlasmaEnergyRequestRatio, requiredMinimumThrottle);
            _currentGeneratorChargedEnergyRequestRatio = Math.Max(_currentGeneratorChargedEnergyRequestRatio, requiredMinimumThrottle);

            var thermalDifference = Math.Abs(storedGeneratorThermalEnergyRequestRatio - _currentGeneratorThermalEnergyRequestRatio);
            var plasmaDifference = Math.Abs(storedGeneratorPlasmaEnergyRequestRatio - _currentGeneratorPlasmaEnergyRequestRatio);
            var chargedDifference = Math.Abs(storedGeneratorChargedEnergyRequestRatio - _currentGeneratorChargedEnergyRequestRatio);

            var thermalThrottleIsGrowing = _currentGeneratorThermalEnergyRequestRatio > storedGeneratorThermalEnergyRequestRatio;
            var plasmaThrottleIsGrowing = _currentGeneratorPlasmaEnergyRequestRatio > storedGeneratorPlasmaEnergyRequestRatio;
            var chargedThrottleIsGrowing = _currentGeneratorChargedEnergyRequestRatio > storedGeneratorChargedEnergyRequestRatio;

            var fixedReactorSpeedMultiplier = ReactorSpeedMult * resMan.FixedDeltaTime(); // * timeWarpFixedDeltaTime;
            var minimumAcceleration = resMan.FixedDeltaTime() * resMan.FixedDeltaTime(); //  1; //  timeWarpFixedDeltaTime * timeWarpFixedDeltaTime;

            var thermalAccelerationReductionRatio = thermalThrottleIsGrowing
                ? storedGeneratorThermalEnergyRequestRatio <= 0.5 ? 1 : minimumAcceleration + (1 - storedGeneratorThermalEnergyRequestRatio) / 0.5
                : storedGeneratorThermalEnergyRequestRatio <= 0.5 ? minimumAcceleration + storedGeneratorThermalEnergyRequestRatio / 0.5 : 1;

            var plasmaAccelerationReductionRatio = plasmaThrottleIsGrowing
                ? storedGeneratorPlasmaEnergyRequestRatio <= 0.5 ? 1 : minimumAcceleration + (1 - storedGeneratorPlasmaEnergyRequestRatio) / 0.5
                : storedGeneratorPlasmaEnergyRequestRatio <= 0.5 ? minimumAcceleration + storedGeneratorPlasmaEnergyRequestRatio / 0.5 : 1;

            var chargedAccelerationReductionRatio = chargedThrottleIsGrowing
                ? storedGeneratorChargedEnergyRequestRatio <= 0.5 ? 1 : minimumAcceleration + (1 - storedGeneratorChargedEnergyRequestRatio) / 0.5
                : storedGeneratorChargedEnergyRequestRatio <= 0.5 ? minimumAcceleration + storedGeneratorChargedEnergyRequestRatio / 0.5 : 1;

            var fixedThermalSpeed = fixedReactorSpeedMultiplier > 0 ? Math.Min(thermalDifference, fixedReactorSpeedMultiplier) * thermalAccelerationReductionRatio : thermalDifference;
            var fixedPlasmaSpeed = fixedReactorSpeedMultiplier > 0 ? Math.Min(plasmaDifference, fixedReactorSpeedMultiplier) * plasmaAccelerationReductionRatio : plasmaDifference;
            var fixedChargedSpeed = fixedReactorSpeedMultiplier > 0 ? Math.Min(chargedDifference, fixedReactorSpeedMultiplier) * chargedAccelerationReductionRatio : chargedDifference;

            var thermalChangeFraction = thermalThrottleIsGrowing ? fixedThermalSpeed : -fixedThermalSpeed;
            var plasmaChangeFraction = plasmaThrottleIsGrowing ? fixedPlasmaSpeed : -fixedPlasmaSpeed;
            var chargedChangeFraction = chargedThrottleIsGrowing ? fixedChargedSpeed : -fixedChargedSpeed;

            storedGeneratorThermalEnergyRequestRatio = Math.Max(0, Math.Min(1, storedGeneratorThermalEnergyRequestRatio + thermalChangeFraction));
            storedGeneratorPlasmaEnergyRequestRatio = Math.Max(0, Math.Min(1, storedGeneratorPlasmaEnergyRequestRatio + plasmaChangeFraction));
            storedGeneratorChargedEnergyRequestRatio = Math.Max(0, Math.Min(1, storedGeneratorChargedEnergyRequestRatio + chargedChangeFraction));

            _currentGeneratorThermalEnergyRequestRatio = 0;
            _currentGeneratorPlasmaEnergyRequestRatio = 0;
            _currentGeneratorChargedEnergyRequestRatio = 0;
        }

        protected double GetFuelRatio(ReactorFuel reactorFuel, double efficiency, double megajoules)
        {
            if (CheatOptions.InfinitePropellant)
                return 1;

            var fuelUseForPower = reactorFuel.GetFuelUseForPower(efficiency, megajoules, fuelUsePerMJMult);

            return fuelUseForPower > 0 ? GetFuelAvailability(reactorFuel) / fuelUseForPower : 0;
        }

        private void BreedTritium(IResourceManager resMan, double neutronPowerReceivedEachSecond)
        {

            _lithiumConsumedPerSecond = 0;
            _tritiumProducedPerSecond = 0;
            _heliumProducedPerSecond = 0;
            TotalAmountLithium = 0;
            TotalMaxAmountLithium = 0;

            // verify if there is any lithium6 present
            var partResourceLithium6 = part.Resources[KITResourceSettings.Lithium6];
            if (partResourceLithium6 == null)
                return;

            TotalAmountLithium = partResourceLithium6.amount;
            TotalMaxAmountLithium = partResourceLithium6.maxAmount;

            if (breedtritium == false || neutronPowerReceivedEachSecond <= 0 || resMan.FixedDeltaTime() <= 0)
                return;

            if (TotalAmountLithium.IsInfinityOrNaNorZero() || TotalMaxAmountLithium.IsInfinityOrNaNorZero())
                return;

            LithiumNeutronAbsorption = CheatOptions.UnbreakableJoints ? 1 : Math.Max(0.01, Math.Sqrt(TotalAmountLithium / TotalMaxAmountLithium) - 0.0001);

            if (LithiumNeutronAbsorption <= 0.01)
                return;

            // calculate current maximum lithium consumption
            var breedRate = CurrentFuelMode.TritiumBreedModifier * CurrentFuelMode.NeutronsRatio * _staticBreedRate * neutronPowerReceivedEachSecond * LithiumNeutronAbsorption;
            var lithiumRate = breedRate / _lithium6Density;

            // get spare room tritium
            var spareRoomTritiumAmount = resMan.SpareCapacity(ResourceName.TritiumGas);

            // limit lithium consumption to maximum tritium storage
            var maximumTritiumProduction = lithiumRate * _tritiumBreedingMassAdjustment;
            var maximumLithiumConsumptionRatio = maximumTritiumProduction > 0 ? Math.Min(maximumTritiumProduction, spareRoomTritiumAmount) / maximumTritiumProduction : 0;
            var lithiumRequest = lithiumRate * maximumLithiumConsumptionRatio;

            // consume the lithium
            _lithiumConsumedPerSecond = resMan.CheatOptions().InfinitePropellant ? lithiumRequest : resMan.Consume(ResourceName.Lithium6, lithiumRequest);

            // calculate effective lithium used for tritium breeding
            _lithiumConsumedPerSecond = _lithiumConsumedPerSecond;

            // calculate products
            _tritiumProducedPerSecond = _lithiumConsumedPerSecond * _tritiumBreedingMassAdjustment;
            _heliumProducedPerSecond = _lithiumConsumedPerSecond * _heliumBreedingMassAdjustment;


            // produce tritium and helium
            resMan.Produce(ResourceName.TritiumGas, _tritiumProducedPerSecond);
            resMan.Produce(ResourceName.Helium4Gas, _heliumProducedPerSecond);
        }

        public virtual double GetCoreTempAtRadiatorTemp(double radTemp)
        {
            return CoreTemperature;
        }

        public virtual double GetThermalPowerAtTemp(double temp)
        {
            return MaximumPower;
        }

        public virtual bool ShouldScaleDownJetISP()
        {
            return false;
        }

        public void EnableIfPossible()
        {
            if (!IsNuclear && !IsEnabled)
                IsEnabled = true;
        }

        public override string GetInfo()
        {
            var sb = StringBuilderCache.Acquire();

            const string headerSize = "<size=11>";
            const string headerColor = "<color=#7fdfffff>";

            UpdateReactorCharacteristics();
            if (showEngineConnectionInfo)
            {
                sb.Append(headerSize + headerColor).Append(Localizer.Format("#LOC_KSPIE_Reactor_propulsion")).AppendLine(":</color><size=10>");
                sb.Append(Localizer.Format("#LOC_KSPIE_Reactor_thermalNozzle")).Append(": ");
                UtilizationInfo(sb, thermalPropulsionEfficiency);
                sb.AppendLine().Append(Localizer.Format("#LOC_KSPIE_Reactor_plasmaNozzle")).Append(": ");
                UtilizationInfo(sb, plasmaPropulsionEfficiency);
                sb.AppendLine().Append(Localizer.Format("#LOC_KSPIE_Reactor_magneticNozzle")).Append(": ");
                UtilizationInfo(sb, chargedParticlePropulsionEfficiency);
                sb.AppendLine().AppendLine("</size>");
            }

            if (showPowerGeneratorConnectionInfo)
            {
                sb.Append(headerSize + headerColor).Append(Localizer.Format("#LOC_KSPIE_Reactor_powerGeneration")).AppendLine(":</color><size=10>");
                sb.Append(Localizer.Format("#LOC_KSPIE_Reactor_thermalGenerator")).Append(": ");
                UtilizationInfo(sb, thermalEnergyEfficiency);
                sb.AppendLine().Append(Localizer.Format("#LOC_KSPIE_Reactor_MHDGenerator")).Append(": ");
                UtilizationInfo(sb, plasmaEnergyEfficiency);
                sb.AppendLine().Append(Localizer.Format("#LOC_KSPIE_Reactor_chargedParticleGenerator")).Append(": ");
                UtilizationInfo(sb, chargedParticleEnergyEfficiency);
                sb.AppendLine().AppendLine("</size>");
            }

            if (!string.IsNullOrEmpty(upgradeTechReqMk2))
            {
                sb.Append(headerColor).Append(Localizer.Format("#LOC_KSPIE_Reactor_powerUpgradeTechnologies")).AppendLine(":</color><size=10>");
                if (!string.IsNullOrEmpty(upgradeTechReqMk2))
                    sb.Append("- ").AppendLine(Localizer.Format(PluginHelper.GetTechTitleById(upgradeTechReqMk2)));
                if (!string.IsNullOrEmpty(upgradeTechReqMk3))
                    sb.Append("- ").AppendLine(Localizer.Format(PluginHelper.GetTechTitleById(upgradeTechReqMk3)));
                if (!string.IsNullOrEmpty(upgradeTechReqMk4))
                    sb.Append("- ").AppendLine(Localizer.Format(PluginHelper.GetTechTitleById(upgradeTechReqMk4)));
                if (!string.IsNullOrEmpty(upgradeTechReqMk5))
                    sb.Append("- ").AppendLine(Localizer.Format(PluginHelper.GetTechTitleById(upgradeTechReqMk5)));
                if (!string.IsNullOrEmpty(upgradeTechReqMk6))
                    sb.Append("- ").AppendLine(Localizer.Format(PluginHelper.GetTechTitleById(upgradeTechReqMk6)));
                if (!string.IsNullOrEmpty(upgradeTechReqMk7))
                    sb.Append("- ").AppendLine(Localizer.Format(PluginHelper.GetTechTitleById(upgradeTechReqMk7)));
                sb.AppendLine("</size>");
            }

            if (thermalEnergyEfficiency > 0 || plasmaEnergyEfficiency > 0 || chargedParticleEnergyEfficiency > 0)
            {

                sb.Append(headerColor).Append(Localizer.Format("#LOC_KSPIE_Reactor_ReactorPower")).AppendLine(":</color><size=10>");
                sb.Append("Mk1: ").AppendLine(PluginHelper.GetFormattedPowerString(powerOutputMk1));
                if (!string.IsNullOrEmpty(upgradeTechReqMk2))
                    sb.Append("Mk2: ").AppendLine(PluginHelper.GetFormattedPowerString(powerOutputMk2));
                if (!string.IsNullOrEmpty(upgradeTechReqMk3))
                    sb.Append("Mk3: ").AppendLine(PluginHelper.GetFormattedPowerString(powerOutputMk3));
                if (!string.IsNullOrEmpty(upgradeTechReqMk4))
                    sb.Append("Mk4: ").AppendLine(PluginHelper.GetFormattedPowerString(powerOutputMk4));
                if (!string.IsNullOrEmpty(upgradeTechReqMk5))
                    sb.Append("Mk5: ").AppendLine(PluginHelper.GetFormattedPowerString(powerOutputMk5));
                if (!string.IsNullOrEmpty(upgradeTechReqMk6))
                    sb.Append("Mk6: ").AppendLine(PluginHelper.GetFormattedPowerString(powerOutputMk6));
                if (!string.IsNullOrEmpty(upgradeTechReqMk7))
                    sb.Append("Mk7: ").AppendLine(PluginHelper.GetFormattedPowerString(powerOutputMk7));
                sb.AppendLine("</size>");
            }

            if (_hasSpecificFuelModeTechs)
            {
                sb.AppendLine(headerColor).Append(Localizer.Format("#LOC_KSPIE_Reactor_fuelModeUpgradeTechnologies")).AppendLine(":</color><size=10>");
                if (!string.IsNullOrEmpty(fuelModeTechReqLevel2) && fuelModeTechReqLevel2 != "none")
                    sb.Append("- ").AppendLine(Localizer.Format(PluginHelper.GetTechTitleById(fuelModeTechReqLevel2)));
                if (!string.IsNullOrEmpty(fuelModeTechReqLevel3) && fuelModeTechReqLevel3 != "none")
                    sb.Append("- ").AppendLine(Localizer.Format(PluginHelper.GetTechTitleById(fuelModeTechReqLevel3)));
                if (!string.IsNullOrEmpty(fuelModeTechReqLevel4) && fuelModeTechReqLevel4 != "none")
                    sb.Append("- ").AppendLine(Localizer.Format(PluginHelper.GetTechTitleById(fuelModeTechReqLevel4)));
                if (!string.IsNullOrEmpty(fuelModeTechReqLevel5) && fuelModeTechReqLevel5 != "none")
                    sb.Append("- ").AppendLine(Localizer.Format(PluginHelper.GetTechTitleById(fuelModeTechReqLevel5)));
                if (!string.IsNullOrEmpty(fuelModeTechReqLevel6) && fuelModeTechReqLevel6 != "none")
                    sb.Append("- ").AppendLine(Localizer.Format(PluginHelper.GetTechTitleById(fuelModeTechReqLevel6)));
                if (!string.IsNullOrEmpty(fuelModeTechReqLevel7) && fuelModeTechReqLevel7 != "none")
                    sb.Append("- ").AppendLine(Localizer.Format(PluginHelper.GetTechTitleById(fuelModeTechReqLevel7)));
                sb.AppendLine("</size>");
            }

            var maximumFuelTechLevel = GetMaximumFuelTechLevel();
            var fuelGroups = GetFuelGroups(maximumFuelTechLevel);

            if (fuelGroups.Count > 1)
            {
                sb.Append(headerColor).Append(Localizer.Format("#LOC_KSPIE_Reactor_getInfoFuelModes")).AppendLine(":</color><size=10>");
                foreach (var group in fuelGroups)
                {
                    sb.Append("Mk").Append(Math.Max(0, 1 + group.TechLevel - reactorModeTechBonus)).Append(": ").AppendLine(Localizer.Format(group.ModeGUIName));
                }
                sb.AppendLine("</size>");
            }

            if (plasmaPropulsionEfficiency > 0)
            {
                sb.Append(headerColor).Append(Localizer.Format("#LOC_KSPIE_Reactor_plasmaNozzlePerformance")).AppendLine(":</color><size=10>");
                sb.Append("Mk1: ").AppendLine(PlasmaNozzlePerformance(coreTemperatureMk1, powerOutputMk1));
                if (!string.IsNullOrEmpty(upgradeTechReqMk2))
                    sb.Append("Mk2: ").AppendLine(PlasmaNozzlePerformance(coreTemperatureMk2, powerOutputMk2));
                if (!string.IsNullOrEmpty(upgradeTechReqMk3))
                    sb.Append("Mk3: ").AppendLine(PlasmaNozzlePerformance(coreTemperatureMk3, powerOutputMk3));
                if (!string.IsNullOrEmpty(upgradeTechReqMk4))
                    sb.Append("Mk4: ").AppendLine(PlasmaNozzlePerformance(coreTemperatureMk4, powerOutputMk4));
                if (!string.IsNullOrEmpty(upgradeTechReqMk5))
                    sb.Append("Mk5: ").AppendLine(PlasmaNozzlePerformance(coreTemperatureMk5, powerOutputMk5));
                if (!string.IsNullOrEmpty(upgradeTechReqMk6))
                    sb.Append("Mk6: ").AppendLine(PlasmaNozzlePerformance(coreTemperatureMk6, powerOutputMk6));
                if (!string.IsNullOrEmpty(upgradeTechReqMk7))
                    sb.Append("Mk7: ").AppendLine(PlasmaNozzlePerformance(coreTemperatureMk7, powerOutputMk7));
                sb.AppendLine("</size>");
            }

            if (thermalPropulsionEfficiency > 0)
            {
                sb.Append(headerColor).Append(Localizer.Format("#LOC_KSPIE_Reactor_thermalNozzlePerformance")).AppendLine(":</color><size=10>");
                sb.Append("Mk1: ").AppendLine(ThermalNozzlePerformance(coreTemperatureMk1, powerOutputMk1));
                if (!string.IsNullOrEmpty(upgradeTechReqMk2))
                    sb.Append("Mk2: ").AppendLine(ThermalNozzlePerformance(coreTemperatureMk2, powerOutputMk2));
                if (!string.IsNullOrEmpty(upgradeTechReqMk3))
                    sb.Append("Mk3: ").AppendLine(ThermalNozzlePerformance(coreTemperatureMk3, powerOutputMk3));
                if (!string.IsNullOrEmpty(upgradeTechReqMk4))
                    sb.Append("Mk4: ").AppendLine(ThermalNozzlePerformance(coreTemperatureMk4, powerOutputMk4));
                if (!string.IsNullOrEmpty(upgradeTechReqMk5))
                    sb.Append("Mk5: ").AppendLine(ThermalNozzlePerformance(coreTemperatureMk5, powerOutputMk5));
                if (!string.IsNullOrEmpty(upgradeTechReqMk6))
                    sb.Append("Mk6: ").AppendLine(ThermalNozzlePerformance(coreTemperatureMk6, powerOutputMk6));
                if (!string.IsNullOrEmpty(upgradeTechReqMk7))
                    sb.Append("Mk7: ").AppendLine(ThermalNozzlePerformance(coreTemperatureMk7, powerOutputMk7));
                sb.AppendLine("</size>");
            }

            return sb.ToStringAndRelease();
        }

        private List<ReactorFuelType> GetFuelGroups(int maximumFuelTechLevel)
        {
            var groups = GameDatabase.Instance.GetConfigNodes("REACTOR_FUEL_MODE")
                .Select(node => new ReactorFuelMode(node))
                .Where(fm =>
                       fm.AllFuelResourcesDefinitionsAvailable && fm.AllProductResourcesDefinitionsAvailable
                    && (fm.SupportedReactorTypes & ReactorType) == ReactorType
                    && maximumFuelTechLevel >= fm.TechLevel
                    && FusionEnergyGainFactor >= fm.MinimumFusionGainFactor
                    && (fm.Aneutronic || canUseNeutronicFuels)
                    && maxGammaRayPower >= fm.GammaRayEnergy)
                .GroupBy(mode => mode.ModeGUIName).Select(group => new ReactorFuelType(group)).OrderBy(m => m.TechLevel).ToList();
            return groups;
        }

        private int GetMaximumFuelTechLevel()
        {
            int techLevels = 0;
            if (!string.IsNullOrEmpty(fuelModeTechReqLevel2) && fuelModeTechReqLevel2 != "none") techLevels++;
            if (!string.IsNullOrEmpty(fuelModeTechReqLevel3) && fuelModeTechReqLevel3 != "none") techLevels++;
            if (!string.IsNullOrEmpty(fuelModeTechReqLevel4) && fuelModeTechReqLevel4 != "none") techLevels++;
            if (!string.IsNullOrEmpty(fuelModeTechReqLevel5) && fuelModeTechReqLevel5 != "none") techLevels++;
            if (!string.IsNullOrEmpty(fuelModeTechReqLevel6) && fuelModeTechReqLevel6 != "none") techLevels++;
            if (!string.IsNullOrEmpty(fuelModeTechReqLevel7) && fuelModeTechReqLevel7 != "none") techLevels++;
            var maximumFuelTechLevel = techLevels + reactorModeTechBonus;
            return maximumFuelTechLevel;
        }

        private string ThermalNozzlePerformance(double temperature, double powerInMj)
        {
            var isp = Math.Min(Math.Sqrt(temperature) * 21, maxThermalNozzleIsp);

            var exhaustVelocity = isp * PhysicsGlobals.GravitationalAcceleration;

            var thrust = powerInMj * 2000.0 * thermalPropulsionEfficiency / (exhaustVelocity * powerOutputMultiplier);

            return thrust.ToString("F1") + "kN @ " + isp.ToString("F0") + "s";
        }

        private string PlasmaNozzlePerformance(double temperature, double powerInMj)
        {
            var isp = Math.Sqrt(temperature) * 21;

            var exhaustVelocity = isp * PhysicsGlobals.GravitationalAcceleration;

            var thrust = powerInMj * 2000.0 * plasmaPropulsionEfficiency / (exhaustVelocity * powerOutputMultiplier);

            return thrust.ToString("F1") + "kN @ " + isp.ToString("F0") + "s";
        }

        private void UtilizationInfo(StringBuilder sb, double value)
        {
            sb.Append(RUIutils.GetYesNoUIString(value > 0.0));
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (value > 0.0 && value != 1.0)
            {
                sb.Append(" (<color=orange>").Append((value * 100.0).ToString("F0")).Append("%</color>)");
            }
        }

        protected bool ReactorIsOverheating(IResourceManager resMan)
        {
            if (!CheatOptions.IgnoreMaxTemperature && resMan.FillFraction(ResourceName.WasteHeat) >= emergencyPowerShutdownFraction && canShutdown)
            {
                _deactivateTimer++;
                if (_deactivateTimer > 3)
                    return true;
            }
            else
                _deactivateTimer = 0;

            return false;
        }

        protected List<ReactorFuelType> GetReactorFuelModes()
        {
            var fuelModeConfigs = GameDatabase.Instance.GetConfigNodes("REACTOR_FUEL_MODE");

            var filteredFuelModes = fuelModeConfigs.Select(node => new ReactorFuelMode(node))
                .Where(fm =>
                       fm.AllFuelResourcesDefinitionsAvailable
                    && fm.AllProductResourcesDefinitionsAvailable
                    && (fm.SupportedReactorTypes & ReactorType) == ReactorType
                    && PluginHelper.HasTechRequirementOrEmpty(fm.TechRequirement)
                    && ReactorFuelModeTechLevel >= fm.TechLevel
                    && FusionEnergyGainFactor >= fm.MinimumFusionGainFactor
                    && (fm.Aneutronic || canUseNeutronicFuels)
                    && maxGammaRayPower >= fm.GammaRayEnergy
                    && fm.NeutronsRatio <= maxNeutronsRatio
                    && fm.NeutronsRatio >= minNeutronsRatio
                    ).ToList();

            for (var i = 0; i < filteredFuelModes.Count; i++)
            {
                filteredFuelModes[i].Position = i;
            }

            Debug.Log("[KSPI]: found " + filteredFuelModes.Count + " valid fuel types");

            var groups = filteredFuelModes.GroupBy(mode => mode.ModeGUIName).Select(group => new ReactorFuelType(group)).ToList();

            Debug.Log("[KSPI]: grouped them into " + groups.Count + " valid fuel modes");

            return groups;
        }

        protected bool FuelRequiresLab(bool requiresLab)
        {
            var isConnectedToLab = part.IsConnectedToModule("ScienceModule", 10);

            return !requiresLab || isConnectedToLab && canBeCombinedWithLab;
        }

        public virtual void SetDefaultFuelMode()
        {
            if (FuelModes == null)
            {
                Debug.Log("[KSPI]: SetDefaultFuelMode - load fuel modes");
                FuelModes = GetReactorFuelModes();
            }

            CurrentFuelMode = FuelModes.FirstOrDefault();

            MaxPowerToSupply = Math.Max(MaximumPower * TimeWarp.fixedDeltaTime, 0);

            if (CurrentFuelMode == null)
                print("[KSPI]: Warning : CurrentFuelMode is null");
            else
                print("[KSPI]: CurrentFuelMode = " + CurrentFuelMode.ModeGUIName);
        }

        protected double ConsumeReactorFuel(IResourceManager resMan, ReactorFuel fuel, double powerInMj)
        {
            if (fuel == null)
            {
                Debug.LogError("[KSPI]: ConsumeReactorFuel fuel null");
                return 0;
            }

            if (powerInMj.IsInfinityOrNaNorZero())
                return 0;

            var resID = KITResourceSettings.NameToResource(fuel.Definition.name);
            if (resID == ResourceName.Unknown)
            {
                Debug.Log($"[InterstellarReactor] Reactor fuel {fuel.FuelName} / {fuel.Definition.name} is not known to KIT");
                return 0;
            }

            var consumeAmountInUnitOfStorage = FuelEfficiency > 0 ? powerInMj * fuel.AmountFuelUsePerMJ * fuelUsePerMJMult / FuelEfficiency : 0;

            if (fuel.ConsumeGlobal)
            {
                var result = fuel.Simulate ? 0 : resMan.Consume(resID, consumeAmountInUnitOfStorage);
                return (fuel.Simulate ? consumeAmountInUnitOfStorage : result) * fuel.DensityInTon;
            }

            if (part.Resources.Contains(fuel.ResourceName))
            {
                double reduction = Math.Min(consumeAmountInUnitOfStorage, part.Resources[fuel.ResourceName].amount) * resMan.FixedDeltaTime();
                part.Resources[fuel.ResourceName].amount -= reduction;
                return reduction * fuel.DensityInTon;
            }

            return 0;
        }

        protected virtual double ProduceReactorProduct(IResourceManager resMan, ReactorProduct product, double powerInMj)
        {
            if (product == null)
            {
                Debug.LogError("[KSPI]: ProduceReactorProduct product null");
                return 0;
            }

            if (powerInMj.IsInfinityOrNaNorZero())
                return 0;

            var productSupply = powerInMj * product.AmountProductUsePerMJ * fuelUsePerMJMult / FuelEfficiency;

            if (!product.ProduceGlobal)
            {
                if (part.Resources.Contains(product.ResourceName))
                {
                    var partResource = part.Resources[product.ResourceName];
                    var availableStorage = partResource.maxAmount - partResource.amount;
                    var possibleAmount = Math.Min(productSupply, availableStorage);
                    part.Resources[product.ResourceName].amount += possibleAmount;
                    return productSupply * product.DensityInTon;
                }
                else
                    return 0;
            }

            if (!product.Simulate)
                part.RequestResource(product.Definition.id, -productSupply, ResourceFlowMode.STAGE_PRIORITY_FLOW);
            return productSupply * product.DensityInTon;
        }

        // Called by the UI, no resource manager present
        protected double GetFuelAvailability(ReactorFuel fuel)
        {
            if (fuel?.Definition == null)
            {
                Debug.LogError("[KSPI]: GetFuelAvailability fuel (or fuel.definition) is null");
                return 0;
            }

            if (!fuel.ConsumeGlobal)
                return GetLocalResourceAmount(fuel);

            // This crashes, and I'm not sure why. 
            part.GetConnectedResourceTotals(fuel.Definition.id, out var amount, out _);
            return amount;


            // TODO - do we need the loaded scene part? :| return HighLogic.LoadedSceneIsFlight ? part.GetResourceAvailable(fuel.Definition) : part.FindAmountOfAvailableFuel(fuel.ResourceName, 4);

        }

        protected double GetLocalResourceRatio(ReactorFuel fuel)
        {
            if (part.Resources.Contains(fuel.ResourceName))
                return part.Resources[fuel.ResourceName].amount / part.Resources[fuel.ResourceName].maxAmount;
            else
                return 0;
        }

        protected double GetLocalResourceAmount(ReactorFuel fuel)
        {
            if (part.Resources.Contains(fuel.ResourceName))
                return part.Resources[fuel.ResourceName].amount;
            else
                return 0;
        }

        protected double GetFuelAvailability(PartResourceDefinition definition)
        {
            // called by UI code, no resource manager present

            if (definition == null)
            {
                Debug.LogError("[KSPI]: GetFuelAvailability definition null");
                return 0;
            }

            if (definition.resourceTransferMode == ResourceTransferMode.NONE)
            {
                if (part.Resources.Contains(definition.name))
                    return part.Resources[definition.name].amount;
                else
                    return 0;
            }

            part.GetConnectedResourceTotals(definition.id, out double amount, out _);
            return amount;

            // is the below logic needed? I'd be surprised if so..
            // return HighLogic.LoadedSceneIsFlight ? () => { part.GetConnectedResourceTotals(definition.id, out double amount, out _); return amount; } : part.FindAmountOfAvailableFuel(definition.name, 4);
        }

        protected double GetProductAvailability(ReactorProduct product)
        {
            if (product == null)
            {
                Debug.LogError("[KSPI]: GetFuelAvailability product null");
                return 0;
            }

            if (product.Definition == null)
            {
                Debug.LogError("[KSPI]: GetFuelAvailability product definition null");
                return 0;
            }

            if (!product.ProduceGlobal)
            {
                if (part.Resources.Contains(product.ResourceName))
                    return part.Resources[product.ResourceName].amount;
                else
                    return 0;
            }

            // return HighLogic.LoadedSceneIsFlight ? part.GetResourceAvailable(product.Definition) : part.FindAmountOfAvailableFuel(product.ResourceName, 4);

            part.GetConnectedResourceTotals(product.ResourceName.GetHashCode(), out double amount, out _);
            return amount;
        }

        protected double GetMaxProductAvailability(IResourceManager resMan, ReactorProduct product)
        {
            //throw new IndexOutOfRangeException("GetMaxProductAvailability actually not, but I need to come back and fix this. sorry");

            if (product == null)
            {
                Debug.LogError("[KSPI]: GetMaxProductAvailability product null");
                return 0;
            }

            if (product.Definition == null)
                return 0;

            if (!product.ProduceGlobal)
            {
                if (part.Resources.Contains(product.ResourceName))
                    return part.Resources[product.ResourceName].maxAmount;
                else
                    return 0;
            }

            if (HighLogic.LoadedSceneIsFlight)
            {
                //return part.GetResourceMaxAvailable(product.Definition);
                ResourceName resID = KITResourceSettings.NameToResource(product.Definition.name);
                if (resID == ResourceName.Unknown)
                {
                    Debug.Log($"[InterstellarReactor.GetMaxProductAvailability] Unknown resource {product.Definition.name}");
                    return 0;
                }

                return resMan.MaxCapacity(resID);

            }
            else
                return part.FindMaxAmountOfAvailableFuel(product.ResourceName, 4);
        }


        protected double GetMaxProductAvailability(ReactorProduct product)
        {
            // called by UI code

            if (product == null)
            {
                Debug.LogError("[KSPI]: GetMaxProductAvailability product null");
                return 0;
            }

            if (product.Definition == null)
                return 0;

            if (!product.ProduceGlobal)
            {
                if (part.Resources.Contains(product.ResourceName))
                    return part.Resources[product.ResourceName].maxAmount;
                else
                    return 0;
            }
            /*
            if (HighLogic.LoadedSceneIsFlight)
            {
                //return part.GetResourceMaxAvailable(product.Definition);
                ResourceName resID = KITResourceSettings.NameToResource(product.Definition.name);
                if (resID == ResourceName.Unknown)
                {
                    Debug.Log($"[InterstellarReactor.GetMaxProductAvailability] Unknown resource {product.Definition.name}");
                    return 0;
                }
                return resMan.CurrentCapacity(resID) + resMan.SpareCapacity(resID);

            }
            else
                return part.FindMaxAmountOfAvailableFuel(product.ResourceName, 4);
            */

            part.GetConnectedResourceTotals(product.ResourceName.GetHashCode(), out double amount, out _);
            return amount;
        }


        private void InitializeKerbalismEmitter()
        {
            if (!Kerbalism.IsLoaded)
                return;

            _emitterController = part.FindModuleImplementing<FNEmitterController>();

            if (_emitterController != null)
            {
                _emitterController.diameter = radius;
                _emitterController.exhaustProducesNeutronRadiation = !mayExhaustInLowSpaceHomeworld;
                _emitterController.exhaustProducesGammaRadiation = !mayExhaustInAtmosphereHomeworld;
            }
            else
                Debug.LogWarning("[KSPI]: No Emitter Found om " + part.partInfo.title);
        }

        private void UpdateKerbalismEmitter()
        {
            if (_emitterController == null)
                return;

            _emitterController.reactorActivityFraction = ongoing_consumption_rate;
            _emitterController.fuelNeutronsFraction = CurrentFuelMode.NeutronsRatio;
            _emitterController.lithiumNeutronAbsorbtionFraction = LithiumNeutronAbsorption;
            _emitterController.exhaustActivityFraction = _propulsionRequestRatioSum;
            _emitterController.radioactiveFuelLeakFraction = Math.Max(0, 1 - GeeForceModifier);

            _emitterController.reactorShadowShieldMassProtection = IsConnectedToThermalGenerator || IsConnectedToChargedGenerator
                ? Math.Max(_currentChargedEnergyGeneratorMass, _currentThermalEnergyGeneratorMass) / (radius * radius) / (RawMaximumPower * 0.001)
                : 0;
        }

        public void OnGUI()
        {
            if (vessel == FlightGlobals.ActiveVessel && RenderWindow)
                _windowPosition = GUILayout.Window(_windowId, _windowPosition, Window, Localizer.Format("#LOC_KSPIE_Reactor_reactorControlWindow"));
        }

        protected void PrintToGuiLayout(string label, string value, GUIStyle guiLabelStyle, GUIStyle guiValueStyle, int widthLabel = 150, int widthValue = 150)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, guiLabelStyle, GUILayout.Width(widthLabel));
            GUILayout.Label(value, guiValueStyle, GUILayout.Width(widthValue));
            GUILayout.EndHorizontal();
        }

        protected virtual void WindowReactorStatusSpecificOverride() { }

        protected virtual void WindowReactorControlSpecificOverride() { }

        private void Window(int windowId)
        {
            try
            {
                windowPositionX = _windowPosition.x;
                windowPositionY = _windowPosition.y;

                if (BoldStyle == null)
                    BoldStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, font = PluginHelper.MainFont };

                if (TextStyle == null)
                    TextStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Normal, font = PluginHelper.MainFont };

                if (GUI.Button(new Rect(_windowPosition.width - 20, 2, 18, 18), "x"))
                    RenderWindow = false;

                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                GUILayout.Label(TypeName, BoldStyle, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                if (IsFuelNeutronRich)
                    PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_Reactor_ReactorEmbrittlement"), (100 * (1 - ReactorEmbrittlementConditionRatio)).ToString("0.000000") + "%", BoldStyle, TextStyle);//"Reactor Embrittlement"

                PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_Reactor_Geeforceoverload") + " ", (100 * (1 - GeeForceModifier)).ToString("0.000000") + "%", BoldStyle, TextStyle);//Geeforce overload
                PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_Reactor_Overheating") + " ", (100 * (1 - OverheatModifier)).ToString("0.000000") + "%", BoldStyle, TextStyle);//Overheating

                WindowReactorStatusSpecificOverride();

                PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_Reactor_Radius"), radius + "m", BoldStyle, TextStyle);//"Radius"
                PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_Reactor_CoreTemperature"), CoretempStr, BoldStyle, TextStyle);//"Core Temperature"
                PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_Reactor_StatusLabel"), StatusStr, BoldStyle, TextStyle);//"Status"
                PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_Reactor_FuelMode"), FuelModeStr, BoldStyle, TextStyle);//"Fuel Mode"
                PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_Reactor_FuelEfficiencyLabel"), (FuelEfficiency * 100).ToString(CultureInfo.InvariantCulture), BoldStyle, TextStyle);//"Fuel efficiency"

                WindowReactorControlSpecificOverride();

                PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_Reactor_CurrentMaxPowerOutputLabel"), PluginHelper.GetFormattedPowerString(ongoing_total_power_generated) + " / " + PluginHelper.GetFormattedPowerString(NormalisedMaximumPower), BoldStyle, TextStyle);//"Current/Max Power Output"

                if (ChargedPowerRatio < 1.0)
                    PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_Reactor_CurrentMaxThermalPower"), PluginHelper.GetFormattedPowerString(ongoing_thermal_power_generated) + " / " + PluginHelper.GetFormattedPowerString(MaximumThermalPower), BoldStyle, TextStyle);//"Current/Max Thermal Power"
                if (ChargedPowerRatio > 0)
                    PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_Reactor_CurrentMaxChargedPower"), PluginHelper.GetFormattedPowerString(ongoing_charged_power_generated) + " / " + PluginHelper.GetFormattedPowerString(MaximumChargedPower), BoldStyle, TextStyle);//"Current/Max Charged Power"

                if (CurrentFuelMode != null && CurrentFuelVariant.ReactorFuels != null)
                {
                    PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_Reactor_EnergyProduction"), CurrentFuelVariant.GigawattPerGram.ToString("0.0") + " GW / g", BoldStyle, TextStyle);//"Energy Production"
                    PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_Reactor_FuelUsage"), CurrentFuelVariant.FuelUseInGramPerTeraJoule.ToString("0.000") + " g / TW", BoldStyle, TextStyle);//"Fuel Usage"

                    if (IsFuelNeutronRich && breedtritium && canBreedTritium)
                    {
                        PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_Reactor_FuelNeutronBreedRate"), 100 * CurrentFuelMode.NeutronsRatio + "% ", BoldStyle, TextStyle);//"Fuel Neutron Breed Rate"

                        var tritiumKgDay = _tritiumProducedPerSecond * _tritiumDensity * 1000 * PluginHelper.SecondsInDay;
                        PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_Reactor_TritiumBreedRate"), tritiumKgDay.ToString("0.000000") + " " + Localizer.Format("#LOC_KSPIE_Reactor_kgDay") + " ", BoldStyle, TextStyle);//"Tritium Breed Rate"kg/day

                        var heliumKgDay = _heliumProducedPerSecond * _helium4Density * 1000 * PluginHelper.SecondsInDay;
                        PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_Reactor_HeliumBreedRate"), heliumKgDay.ToString("0.000000") + " " + Localizer.Format("#LOC_KSPIE_Reactor_kgDay") + " ", BoldStyle, TextStyle);//"Helium Breed Rate"kg/day

                        part.GetConnectedResourceTotals(_lithium6Def.id, out var totalLithium6Amount, out var totalLithium6MaxAmount);

                        PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_Reactor_LithiumReserves"), totalLithium6Amount.ToString("0.000") + " L / " + totalLithium6MaxAmount.ToString("0.000") + " L", BoldStyle, TextStyle);//"Lithium Reserves"

                        var lithiumConsumptionDay = _lithiumConsumedPerSecond * PluginHelper.SecondsInDay;
                        PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_Reactor_LithiumConsumption"), lithiumConsumptionDay.ToString("0.00000") + " " + Localizer.Format("#LOC_KSPIE_Reactor_lithiumConsumptionDay"), BoldStyle, TextStyle);//"Lithium Consumption"L/day
                        var lithiumLifetimeTotalDays = lithiumConsumptionDay > 0 ? totalLithium6Amount / lithiumConsumptionDay : 0;

                        var lithiumLifetimeYears = Math.Floor(lithiumLifetimeTotalDays / GameConstants.KerbinYearInDays);
                        var lithiumLifetimeYearsRemainderInDays = lithiumLifetimeTotalDays % GameConstants.KerbinYearInDays;

                        var lithiumLifetimeRemainingDays = Math.Floor(lithiumLifetimeYearsRemainderInDays);
                        var lithiumLifetimeRemainingDaysRemaining = lithiumLifetimeYearsRemainderInDays % 1;

                        var lithiumLifetimeRemainingHours = lithiumLifetimeRemainingDaysRemaining * PluginHelper.SecondsInDay / GameConstants.SecondsInHour;

                        if (lithiumLifetimeYears < 1e9)
                        {
                            if (lithiumLifetimeYears < 1)
                                PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_Reactor_LithiumRemaining"), lithiumLifetimeRemainingDays + " " + Localizer.Format("#LOC_KSPIE_Reactor_days") + " " + lithiumLifetimeRemainingHours.ToString("0.0") + " " + Localizer.Format("#LOC_KSPIE_Reactor_hours"), BoldStyle, TextStyle);//"Lithium Remaining"days""hours
                            else if (lithiumLifetimeYears < 1e3)
                                PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_Reactor_LithiumRemaining"), lithiumLifetimeYears + " " + Localizer.Format("#LOC_KSPIE_Reactor_years") + " " + lithiumLifetimeRemainingDays + " " + Localizer.Format("#LOC_KSPIE_Reactor_days"), BoldStyle, TextStyle);//"Lithium Remaining"years""days
                            else
                                PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_Reactor_LithiumRemaining"), lithiumLifetimeYears + " " + Localizer.Format("#LOC_KSPIE_Reactor_years") + " ", BoldStyle, TextStyle);//"Lithium Remaining"years
                        }

                        part.GetConnectedResourceTotals(_tritiumDef.id, out var totalTritiumAmount, out var totalTritiumMaxAmount);

                        var massTritiumAmount = totalTritiumAmount * _tritiumDensity * 1000;
                        var massTritiumMaxAmount = totalTritiumMaxAmount * _tritiumDensity * 1000;

                        PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_Reactor_TritiumStorage"), massTritiumAmount.ToString("0.000000") + " kg / " + massTritiumMaxAmount.ToString("0.000000") + " kg", BoldStyle, TextStyle);//"Tritium Storage"

                        part.GetConnectedResourceTotals(_heliumDef.id, out var totalHeliumAmount, out var totalHeliumMaxAmount);

                        var massHeliumAmount = totalHeliumAmount * _helium4Density * 1000;
                        var massHeliumMaxAmount = totalHeliumMaxAmount * _helium4Density * 1000;

                        PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_Reactor_HeliumStorage"), massHeliumAmount.ToString("0.000000") + " kg / " + massHeliumMaxAmount.ToString("0.000000") + " kg", BoldStyle, TextStyle);//"Helium Storage"
                    }
                    else
                        PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_Reactor_IsNeutronrich"), IsFuelNeutronRich.ToString(), BoldStyle, TextStyle);//"Is Neutron rich"

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(Localizer.Format("#LOC_KSPIE_Reactor_Fuels") + ":", BoldStyle, GUILayout.Width(150));//Fuels
                    GUILayout.EndHorizontal();

                    foreach (var fuel in CurrentFuelVariant.ReactorFuels)
                    {
                        if (fuel == null)
                            continue;

                        var resourceVariantsDefinitions = CurrentFuelMode.ResourceGroups.First(m => m.Name == fuel.FuelName).ResourceVariantsMetaData;

                        var availableResources = resourceVariantsDefinitions
                            .Select(m => new { resourceDefinition = m.ResourceDefinition, ratio = m.Ratio }).Distinct()
                            .Select(d => new { definition = d.resourceDefinition, amount = GetFuelAvailability(d.resourceDefinition), effectiveDensity = d.resourceDefinition.density * d.ratio })
                            .Where(m => m.amount > 0).ToList();

                        var availabilityInTon = availableResources.Sum(m => m.amount * m.effectiveDensity);

                        var variantText = availableResources.Count > 1 ? " (" + availableResources.Count + " variants)" : "";
                        PrintToGuiLayout(fuel.FuelName + " " + Localizer.Format("#LOC_KSPIE_Reactor_Reserves"), PluginHelper.FormatMassStr(availabilityInTon) + variantText, BoldStyle, TextStyle);//Reserves

                        var tonFuelUsePerHour = ongoing_total_power_generated * fuel.TonsFuelUsePerMJ * fuelUsePerMJMult / FuelEfficiency * PluginHelper.SecondsInHour;
                        var kgFuelUsePerHour = tonFuelUsePerHour * 1000;
                        var kgFuelUsePerDay = kgFuelUsePerHour * PluginHelper.HoursInDay;

                        if (tonFuelUsePerHour > 120)
                            PrintToGuiLayout(fuel.FuelName + " " + Localizer.Format("#LOC_KSPIE_Reactor_Consumption") + " ", PluginHelper.FormatMassStr(tonFuelUsePerHour / 60) + " / " + Localizer.Format("#LOC_KSPIE_Reactor_min"), BoldStyle, TextStyle);//Consumption-min
                        else
                            PrintToGuiLayout(fuel.FuelName + " " + Localizer.Format("#LOC_KSPIE_Reactor_Consumption") + " ", PluginHelper.FormatMassStr(tonFuelUsePerHour) + " / " + Localizer.Format("#LOC_KSPIE_Reactor_hour"), BoldStyle, TextStyle);//Consumption--hour

                        if (kgFuelUsePerDay > 0)
                        {
                            var fuelLifetimeD = availabilityInTon * 1000 / kgFuelUsePerDay;
                            var lifetimeYears = Math.Floor(fuelLifetimeD / GameConstants.KerbinYearInDays);
                            if (lifetimeYears < 1e9)
                            {
                                if (lifetimeYears >= 10)
                                {
                                    var lifetimeYearsDayRemainder = lifetimeYears < 1e+6 ? fuelLifetimeD % GameConstants.KerbinYearInDays : 0;
                                    PrintToGuiLayout(fuel.FuelName + " " + Localizer.Format("#LOC_KSPIE_Reactor_Lifetime"), (double.IsNaN(lifetimeYears) ? "-" : lifetimeYears + " " + Localizer.Format("#LOC_KSPIE_Reactor_years") + " "), BoldStyle, TextStyle);//Lifetime years
                                }
                                else if (lifetimeYears > 0)
                                {
                                    var lifetimeYearsDayRemainder = lifetimeYears < 1e+6 ? fuelLifetimeD % GameConstants.KerbinYearInDays : 0;
                                    PrintToGuiLayout(fuel.FuelName + " " + Localizer.Format("#LOC_KSPIE_Reactor_Lifetime"), (double.IsNaN(lifetimeYears) ? "-" : lifetimeYears + " " + Localizer.Format("#LOC_KSPIE_Reactor_years") + " " + (lifetimeYearsDayRemainder).ToString("0.00")) + " " + Localizer.Format("#LOC_KSPIE_Reactor_days"), BoldStyle, TextStyle);//Lifetime--years--days
                                }
                                else if (fuelLifetimeD < 1)
                                {
                                    var minutesD = fuelLifetimeD * PluginHelper.HoursInDay * 60;
                                    var minutes = (int)Math.Floor(minutesD);
                                    var seconds = (int)Math.Ceiling((minutesD - minutes) * 60);

                                    PrintToGuiLayout(fuel.FuelName + " " + Localizer.Format("#LOC_KSPIE_Reactor_Lifetime"), minutes.ToString("F0") + " " + Localizer.Format("#LOC_KSPIE_Reactor_minutes") + " " + seconds.ToString("F0") + " " + Localizer.Format("#LOC_KSPIE_Reactor_seconds"), BoldStyle, TextStyle);//Lifetime--minutes--seconds
                                }
                                else
                                    PrintToGuiLayout(fuel.FuelName + " " + Localizer.Format("#LOC_KSPIE_Reactor_Lifetime"), (double.IsNaN(fuelLifetimeD) ? "-" : (fuelLifetimeD).ToString("0.00")) + " " + Localizer.Format("#LOC_KSPIE_Reactor_days"), BoldStyle, TextStyle);//Lifetime--days
                            }
                            else
                                PrintToGuiLayout(fuel.FuelName + " " + Localizer.Format("#LOC_KSPIE_Reactor_Lifetime"), "", BoldStyle, TextStyle);//Lifetime
                        }
                        else
                            PrintToGuiLayout(fuel.FuelName + " " + Localizer.Format("#LOC_KSPIE_Reactor_Lifetime"), "", BoldStyle, TextStyle);//Lifetime
                    }

                    if (CurrentFuelVariant.ReactorProducts.Count > 0)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(Localizer.Format("#LOC_KSPIE_Reactor_Products"), BoldStyle, GUILayout.Width(150));//"Products:"
                        GUILayout.EndHorizontal();

                        foreach (var product in CurrentFuelVariant.ReactorProducts)
                        {
                            if (product == null)
                                continue;

                            var availabilityInTon = GetProductAvailability(product) * product.DensityInTon;
                            var maxAvailabilityInTon = GetMaxProductAvailability(product) * product.DensityInTon;

                            GUILayout.BeginHorizontal();
                            GUILayout.Label(product.FuelName + " " + Localizer.Format("#LOC_KSPIE_Reactor_Storage"), BoldStyle, GUILayout.Width(150));//Storage
                            GUILayout.Label(PluginHelper.FormatMassStr(availabilityInTon, "0.00000") + " / " + PluginHelper.FormatMassStr(maxAvailabilityInTon, "0.00000"), TextStyle, GUILayout.Width(150));
                            GUILayout.EndHorizontal();

                            var hourProductionInTon = ongoing_total_power_generated * product.TonsProductUsePerMJ * fuelUsePerMJMult / FuelEfficiency * PluginHelper.SecondsInHour;
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(product.FuelName + " " + Localizer.Format("#LOC_KSPIE_Reactor_Production"), BoldStyle, GUILayout.Width(150));//Production
                            GUILayout.Label(PluginHelper.FormatMassStr(hourProductionInTon) + " / " + Localizer.Format("#LOC_KSPIE_Reactor_hour"), TextStyle, GUILayout.Width(150));//hour
                            GUILayout.EndHorizontal();
                        }
                    }
                }

                if (!IsStarted || !IsNuclear)
                {
                    GUILayout.BeginHorizontal();

                    if (IsEnabled && canShutdown && GUILayout.Button(Localizer.Format("#LOC_KSPIE_Reactor_Deactivate"), GUILayout.ExpandWidth(true)))//"Deactivate"
                        DeactivateReactor();
                    if (!IsEnabled && GUILayout.Button(Localizer.Format("#LOC_KSPIE_Reactor_Activate"), GUILayout.ExpandWidth(true)))//"Activate"
                        ActivateReactor();

                    GUILayout.EndHorizontal();
                }
                else
                {
                    if (IsEnabled)
                    {
                        GUILayout.BeginHorizontal();

                        if (GUILayout.Button(Localizer.Format("#LOC_KSPIE_Reactor_Shutdown"), GUILayout.ExpandWidth(true)))//"Shutdown"
                            IsEnabled = false;

                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndVertical();
                GUI.DragWindow();
            }

            catch (Exception e)
            {
                Debug.LogError("[KSPI]: InterstellarReactor Window(" + windowId + "): " + e.Message);
                throw;
            }

        }

        /// <summary>
        /// electricChargeGeneratedLastUpdate indicates how much ElectricCharge was generated in the since the previous FixedUpdate()
        /// call.
        /// </summary>
        private double _electricChargeGeneratedLastUpdate;

        /// <summary>
        /// electricChargeMissingLastUpdate indicates how much ElectricCharge we could not provide in the last FixedUpdate call.
        /// It is the result of electricChargeBeingRequestedThisUpdate - electricChargeBeingSuppliedThisUpdate
        /// </summary>
        private double _electricChargeMissingLastUpdate;

        /// <summary>
        ///  electricChargeBeingRequestedThisUpdate provides a running total of Electric Charge being requested this cycle.
        /// </summary>
        private double _electricChargeBeingRequestedThisUpdate;

        /// <summary>
        /// electricChargeBeingSuppliedThisUpdate provides a running total of how much Electric Charge we've been able to provide.
        /// </summary>
        private double _electricChargeBeingSuppliedThisUpdate;

        public ModuleConfigurationFlags ModuleConfiguration() => ModuleConfigurationFlags.LocalResources | (ModuleConfigurationFlags)(int)electricPowerPriority;
        
        public void KITFixedUpdate(IResourceManager resMan)
        {
            if (!HighLogic.LoadedSceneIsFlight)
            {
                DeterminePowerOutput();
                MaximumThermalPowerEffective = MaximumThermalPower;
                return;
            }

            ongoing_total_power_generated = 0;

            // KITFixedUpdate always runs before each power draw attempt.
            _electricChargeGeneratedLastUpdate = _electricChargeBeingSuppliedThisUpdate;
            _electricChargeMissingLastUpdate = _electricChargeBeingRequestedThisUpdate - _electricChargeBeingSuppliedThisUpdate;
            _electricChargeBeingRequestedThisUpdate = _electricChargeBeingSuppliedThisUpdate = 0;

            if (!IsEnabled)
            {
                CurrentFuelVariantsSorted = CurrentFuelMode.GetVariantsOrderedByFuelRatio(part, FuelEfficiency, NormalisedMaximumPower, fuelUsePerMJMult);
                CurrentFuelVariant = CurrentFuelVariantsSorted.FirstOrDefault();
                fuel_mode_variant = CurrentFuelVariant?.Name;
                stored_fuel_ratio = CheatOptions.InfinitePropellant ? 1 : CurrentFuelVariant != null ? Math.Min(CurrentFuelVariant.FuelRatio, 1) : 0;

                ongoing_total_power_generated = 0;
                reactor_power_ratio = 0;
                PluginHelper.SetAnimationRatio(0, PulseAnimation);
                PowerPercent = 0;
            }

            // By default, this is IsEnabled == false and IsStarted == false, so enable them for now.
            if (!IsEnabled && !IsStarted)
            {
                IsStarted = true;
                IsEnabled = true;
            }

            StoreGeneratorRequests(resMan);
            DecayOngoing = false;

            if (!IsEnabled && IsNuclear && MaximumPower > 0 && (Planetarium.GetUniversalTime() - LastActiveTime <= 3 * PluginSettings.Config.SecondsInDay))
            {
                reactor_power_ratio = 0;
                PluginHelper.SetAnimationRatio(0, PulseAnimation);
                var powerFraction = 0.1 * Math.Exp(-(Planetarium.GetUniversalTime() - LastActiveTime) / PluginSettings.Config.SecondsInDay / 24.0 * 9.0);
                var powerToSupply = Math.Max(MaximumPower * powerFraction, 0);
                ongoing_thermal_power_generated = powerToSupply;
                resMan.Produce(ResourceName.ThermalPower, powerToSupply);

                ongoing_total_power_generated = ongoing_thermal_power_generated;
                BreedTritium(resMan, ongoing_thermal_power_generated);
                ongoing_consumption_rate = MaximumPower > 0 ? ongoing_thermal_power_generated / MaximumPower : 0;
                PowerPercent = 100 * ongoing_consumption_rate;
                DecayOngoing = true;

                return;
            }

            LastActiveTime = Planetarium.GetUniversalTime();

            maximumPower = MaximumPower;

            UpdateGeeforceModifier();

            // Debug.Log($"[reactor] maximumPower is {maximumPower}, and geeForceModifier is {geeForceModifier}");

            if (IsEnabled && maximumPower > 0) CalculateMaxPowerOutput(resMan);

            UpdatePlayedSound();

            _previousReactorPowerRatio = reactor_power_ratio;

        }

        private void CalculateMaxPowerOutput(IResourceManager resMan)
        {
            if (hasOverheatEffects && !CheatOptions.IgnoreMaxTemperature)
            {
                _averageOverheat.Enqueue(resMan.FillFraction(ResourceName.WasteHeat));
                if (_averageOverheat.Count > 10)
                    _averageOverheat.Dequeue();

                double scaledOverheating = Math.Pow(Math.Max(resMan.FillFraction(ResourceName.WasteHeat) - overheatThreshHold, 0) * overheatMultiplier, overheatExponent);

                OverheatModifier = Math.Min(Math.Max(1 - scaledOverheating, minOverheatModifier), 1);
            }
            else
                OverheatModifier = 1;

            CurrentFuelVariantsSorted = CurrentFuelMode.GetVariantsOrderedByFuelRatio(part, FuelEfficiency, MaxPowerToSupply * GeeForceModifier * OverheatModifier, fuelUsePerMJMult);
            CurrentFuelVariant = CurrentFuelVariantsSorted.FirstOrDefault();

            fuel_mode_variant = CurrentFuelVariant?.Name;

            stored_fuel_ratio = CheatOptions.InfinitePropellant ? 1 : CurrentFuelVariant != null ? Math.Min(CurrentFuelVariant.FuelRatio, 1) : 0;

            // Debug.Log($"[reactor] overheatModifier is {overheatModifier}, fuel_mode_variant is {fuel_mode_variant}, and stored_fuel_ratio is {stored_fuel_ratio}");

            LookForAlternativeFuelTypes();

            var trueVariant = CurrentFuelMode.GetVariantsOrderedByFuelRatio(part, FuelEfficiency, MaxPowerToSupply, fuelUsePerMJMult, false).FirstOrDefault();
            fuel_ratio = CheatOptions.InfinitePropellant ? 1 : trueVariant != null ? Math.Min(trueVariant.FuelRatio, 1) : 0;

            if (fuel_ratio < 0.99999)
            {
                if (!MessagedRanOutOfFuel)
                {
                    MessagedRanOutOfFuel = true;
                    var message = Localizer.Format("#LOC_KSPIE_Reactor_ranOutOfFuelFor") + " " + CurrentFuelMode.ModeGUIName;
                    Debug.Log("[KSPI]: " + message);
                    ScreenMessages.PostScreenMessage(message, 20.0f, ScreenMessageStyle.UPPER_CENTER);
                }
            }
            else
                MessagedRanOutOfFuel = false;

            thermalThrottleRatio = _connectedEngines.Any(m => m.RequiresThermalHeat) ? Math.Min(1, _connectedEngines.Where(m => m.RequiresThermalHeat).Sum(e => e.CurrentThrottle)) : 0;
            plasmaThrottleRatio = _connectedEngines.Any(m => m.RequiresPlasmaHeat) ? Math.Min(1, _connectedEngines.Where(m => m.RequiresPlasmaHeat).Sum(e => e.CurrentThrottle)) : 0;
            chargedThrottleRatio = _connectedEngines.Any(m => m.RequiresChargedPower) ? Math.Min(1, _connectedEngines.Where(m => m.RequiresChargedPower).Max(e => e.CurrentThrottle)) : 0;

            // Debug.Log($"[reactor] stored_fuel_ratio is {stored_fuel_ratio}, thermalThrottleRatio is {thermalThrottleRatio}, plasmaThrottleRatio is {plasmaThrottleRatio}, and chargedThrottleRatio is {chargedThrottleRatio}");

            var thermalPropulsionRatio = ThermalPropulsionEfficiency * thermalThrottleRatio;
            var plasmaPropulsionRatio = PlasmaPropulsionEfficiency * plasmaThrottleRatio;
            var chargedPropulsionRatio = ChargedParticlePropulsionEfficiency * chargedThrottleRatio;

            var thermalGeneratorRatio = thermalEnergyEfficiency * storedGeneratorThermalEnergyRequestRatio;
            var plasmaGeneratorRatio = plasmaEnergyEfficiency * storedGeneratorPlasmaEnergyRequestRatio;
            var chargedGeneratorRatio = chargedParticleEnergyEfficiency * storedGeneratorChargedEnergyRequestRatio;

            _propulsionRequestRatioSum = Math.Min(1, thermalPropulsionRatio + plasmaPropulsionRatio + chargedPropulsionRatio);

            maximum_thermal_request_ratio = Math.Min(thermalPropulsionRatio + plasmaPropulsionRatio + thermalGeneratorRatio + plasmaGeneratorRatio, 1);
            maximum_charged_request_ratio = Math.Min(chargedPropulsionRatio + chargedGeneratorRatio, 1);

            maximum_reactor_request_ratio = Math.Max(maximum_thermal_request_ratio, maximum_charged_request_ratio);

            var powerAccessModifier = Math.Max(
                Math.Max(
                    _connectedEngines.Any(m => !m.RequiresChargedPower) ? 1 : 0,
                    _connectedEngines.Any(m => m.RequiresChargedPower) ? 1 : 0),
               Math.Max(
                    Math.Max(storedIsThermalEnergyGeneratorEfficiency > 0 ? 1 : 0, storedIsPlasmaEnergyGeneratorEfficiency > 0 ? 1 : 0),
                    storedIsChargedEnergyGeneratorEfficiency > 0 ? 1 : 0
               ));

            maximumChargedPower = MaximumChargedPower;
            maximumThermalPower = MaximumThermalPower;

            var maxStoredGeneratorEnergyRequestedRatio = Math.Max(Math.Max(storedGeneratorThermalEnergyRequestRatio, storedGeneratorPlasmaEnergyRequestRatio), storedGeneratorChargedEnergyRequestRatio);
            var maxThrottleRatio = Math.Max(Math.Max(thermalThrottleRatio, plasmaThrottleRatio), chargedThrottleRatio);

            power_request_ratio = Math.Max(maxThrottleRatio, maxStoredGeneratorEnergyRequestedRatio);

            MaxChargedToSupplyPerSecond = maximumChargedPower * stored_fuel_ratio * GeeForceModifier * OverheatModifier * powerAccessModifier;
            RequestedChargedToSupplyPerSecond = MaxChargedToSupplyPerSecond * power_request_ratio * maximum_charged_request_ratio;

            MaxThermalToSupplyPerSecond = maximumThermalPower * stored_fuel_ratio * GeeForceModifier * OverheatModifier * powerAccessModifier;
            RequestedThermalToSupplyPerSecond = MaxThermalToSupplyPerSecond * power_request_ratio * maximum_thermal_request_ratio;

            // Debug.Log($"[reactor] maxThermalToSupplyPerSecond is {maxThermalToSupplyPerSecond}, maxChargedToSupplyPerSecond is {maxChargedToSupplyPerSecond}, power_request_ratio is {power_request_ratio}, maxStoredGeneratorEnergyRequestedRatio is {maxStoredGeneratorEnergyRequestedRatio}, ");

            _maxThermalToSupplyPerSecondAvail = MaxThermalToSupplyPerSecond;
            _maxChargedToSupplyPerSecondAvail = MaxChargedToSupplyPerSecond;

            // XXX todo. fill in the max we want.
            // maxPowerToSupply = Math.Max(maximumPower, );

            // TODO we should pre-emptively generate required power based on previous update information, modified
            // by the vessel excess / demand

            if (MaxChargedToSupplyPerSecond > 0) resMan.Produce(ResourceName.ChargedParticle, 0, MaxChargedToSupplyPerSecond);
            if (MaxThermalToSupplyPerSecond > 0) resMan.Produce(ResourceName.ThermalPower, 0, MaxThermalToSupplyPerSecond);
            //if (maximumPower > 0) resMan.Produce(ResourceName.ElectricCharge, 0, maximumPower);


        }

        private double _maxThermalToSupplyPerSecondAvail;
        private double _maxChargedToSupplyPerSecondAvail;

        public string KITPartName()
        {
            if (part == null || part.partInfo == null) return "Interstellar Reactor";

            return $"{part.partInfo.title}"; // exception {(fuelModes.Count > 1 ? " (" + fuelModeStr + ")" : "")}";
        }
        // TODO
        //if (similarParts != null && similarParts.Count > 1)
        //    displayName += " " + partNrInList;


        // TODO - is this right?
        // Removed the subclass that it got the implementation from. https://github.com/sswelm/KSP-Interstellar-Extended/blob/19b77a81af0f12f6c081d925e919c2aa2f93e5e0/FNPlugin/Powermanagement/ResourceSuppliableModule.cs

        protected readonly Dictionary<Guid, double> ConnectedReceivers = new Dictionary<Guid, double>();
        public readonly Dictionary<Guid, double> connectedReceiversFraction = new Dictionary<Guid, double>();

        public void AttachThermalReceiver(Guid key, double radius)
        {
            if (!ConnectedReceivers.ContainsKey(key))
                ConnectedReceivers.Add(key, radius);
        }

        public void DetachThermalReceiver(Guid key)
        {
            if (ConnectedReceivers.ContainsKey(key))
                ConnectedReceivers.Remove(key);
        }

        public double GetFractionThermalReceiver(Guid key)
        {
            if (connectedReceiversFraction.TryGetValue(key, out var result))
                return result;
            else
                return 0;
        }

        private readonly ResourceName[] thermalResourcesProvided = new[] { ResourceName.ThermalPower };
        public ResourceName[] chargedResourcesProvided = new[] { ResourceName.ThermalPower, ResourceName.ChargedParticle };

        // What resources can this reactor provide, either as intended, or by side effect.
        // an example might be that we can generate ThermalPower when generating ElectricCharge.
        public ResourceName[] ResourcesProvided()
        {
            if (ChargedPowerRatio > 0) return chargedResourcesProvided;
            if (ThermalPowerRatio > 0) return thermalResourcesProvided;
            return null;
        }

        public bool ProvideResource(IResourceManager resMan, ResourceName resource, double requestedAmount)
        {
            if (!IsEnabled) return false;

            if (resMan.CheatOptions().InfinitePropellant)
            {
                resMan.Produce(resource, requestedAmount);
                return true;
            }
            /*
            minThrottle = stored_fuel_ratio > 0 ? MinimumThrottle / stored_fuel_ratio : 1;
            var neededChargedPowerPerSecond = getNeededPowerSupplyPerSecondWithMinimumRatio(maxChargedToSupplyPerSecond, minThrottle, ResourceSettings.Config.ChargedParticleInMegawatt, chargedParticlesManager);
            charged_power_ratio = Math.Min(maximum_charged_request_ratio, maximumChargedPower > 0 ? neededChargedPowerPerSecond / maximumChargedPower : 0);

            maxThermalToSupplyPerSecond = maximumThermalPower * stored_fuel_ratio * geeForceModifier * overheatModifier * powerAccessModifier;
            requestedThermalToSupplyPerSecond = maxThermalToSupplyPerSecond * power_request_ratio * maximum_thermal_request_ratio;

            var neededThermalPowerPerSecond = getNeededPowerSupplyPerSecondWithMinimumRatio(maxThermalToSupplyPerSecond, minThrottle, ResourceSettings.Config.ThermalPowerInMegawatt, thermalHeatManager);
            requested_thermal_power_ratio = maximumThermalPower > 0 ? neededThermalPowerPerSecond / maximumThermalPower : 0;
            thermal_power_ratio = Math.Min(maximum_thermal_request_ratio, requested_thermal_power_ratio);

            reactor_power_ratio = Math.Min(overheatModifier * maximum_reactor_request_ratio, PowerRatio);

            ongoing_charged_power_generated = managedProvidedPowerSupplyPerSecondMinimumRatio(requestedChargedToSupplyPerSecond, maxChargedToSupplyPerSecond, reactor_power_ratio, ResourceSettings.Config.ChargedParticleInMegawatt, chargedParticlesManager);
            ongoing_thermal_power_generated = managedProvidedPowerSupplyPerSecondMinimumRatio(requestedThermalToSupplyPerSecond, maxThermalToSupplyPerSecond, reactor_power_ratio, ResourceSettings.Config.ThermalPowerInMegawatt, thermalHeatManager);
            ongoing_total_power_generated = ongoing_thermal_power_generated + ongoing_charged_power_generated;

            var totalPowerReceivedFixed = ongoing_total_power_generated * timeWarpFixedDeltaTime;

            UpdateEmbrittlement(Math.Max(thermalThrottleRatio, plasmaThrottleRatio), timeWarpFixedDeltaTime);



            ongoing_consumption_rate = maximumPower > 0 ? ongoing_total_power_generated / maximumPower : 0;
            PluginHelper.SetAnimationRatio((float)Math.Pow(ongoing_consumption_rate, animExponent), pulseAnimation);
            powerPcnt = 100 * ongoing_consumption_rate;

            // produce wasteheat
            if (!CheatOptions.IgnoreMaxTemperature)
            {
                // skip first frame of wasteheat production
                var delayedWasteheatRate = ongoing_consumption_rate > ongoing_wasteheat_rate ? Math.Min(ongoing_wasteheat_rate, ongoing_consumption_rate) : ongoing_consumption_rate;

                supplyFNResourcePerSecondWithMax(delayedWasteheatRate * maximumPower, StableMaximumReactorPower, ResourceSettings.Config.WasteHeatInMegawatt);

                ongoing_wasteheat_rate = ongoing_consumption_rate;
            }
            */

            // One Charged Particle is One Thermal Power is one Waste Heat.

            double tmp;
            switch (resource)
            {
                case ResourceName.ThermalPower:
                    tmp = requestedAmount / ThermalPowerRatio;
                    tmp = Math.Min(tmp, _maxThermalToSupplyPerSecondAvail);
                    if (MaximumChargedPower > 0) tmp = Math.Min(tmp, _maxChargedToSupplyPerSecondAvail);
                    break;
                case ResourceName.ChargedParticle:
                    tmp = requestedAmount / ChargedPowerRatio;
                    tmp = Math.Min(tmp, _maxChargedToSupplyPerSecondAvail);
                    if (MaximumThermalPower > 0) tmp = Math.Min(tmp, _maxThermalToSupplyPerSecondAvail);
                    break;
                default:
                    Debug.Log($"[KIT InterstellarReactor] don't know how to handle {resource} request");
                    return true;
            }

            // Is the above right? Is there cases where we want to be able to exceed the limit on one of them? :\

            // Debug.Log($"[Interstellar Reactor] got request for {KITResourceSettings.ResourceToName(resource)} of {requestedAmount}. tmp is {tmp}, ThermalPowerRatio is {ThermalPowerRatio}, ChargedPowerRatio is {ChargedPowerRatio}, and tmp * ThermalPowerRatio is {tmp * ThermalPowerRatio} and also {tmp * ChargedPowerRatio}");

            _maxThermalToSupplyPerSecondAvail -= tmp * ThermalPowerRatio;
            _maxChargedToSupplyPerSecondAvail -= tmp * ChargedPowerRatio;

            ongoing_thermal_power_generated += tmp * ThermalPowerRatio;
            ongoing_charged_power_generated += tmp * ChargedPowerRatio;

            ongoing_total_power_generated = ongoing_thermal_power_generated + ongoing_charged_power_generated;

            double totalPowerReceivedFixed = tmp;
            double powerGenerated = 0;

            _consumedFuelTotalFixed = 0;

            foreach (var reactorFuel in CurrentFuelVariant.ReactorFuels)
            {
                tmp = ConsumeReactorFuel(resMan, reactorFuel, totalPowerReceivedFixed / GeeForceModifier);
                // TODO not correct. :(
                powerGenerated += tmp / reactorFuel.TonsFuelUsePerMJ;

                _consumedFuelTotalFixed += tmp;
            }

            resMan.Produce(ResourceName.ThermalPower, powerGenerated * ThermalPowerRatio);
            resMan.Produce(ResourceName.ChargedParticle, powerGenerated * ChargedPowerRatio);

            /*
            
            TODO - waste heat. Doesn't seem right at the moment.

            ongoing_consumption_rate = maximumPower > 0 ? ongoing_total_power_generated / maximumPower : 0;
            PluginHelper.SetAnimationRatio((float)Math.Pow(ongoing_consumption_rate, animExponent), pulseAnimation);
            powerPcnt = 100 * ongoing_consumption_rate;

            var delayedWasteheatRate = ongoing_consumption_rate > ongoing_wasteheat_rate ? Math.Min(ongoing_wasteheat_rate, ongoing_consumption_rate) : ongoing_consumption_rate;

            resMan.Produce(ResourceName.WasteHeat, delayedWasteheatRate * maximumPower);
            ongoing_wasteheat_rate = ongoing_consumption_rate;
            */

            // refresh production list
            _reactorProduction.Clear();
            // produce reactor products
            foreach (var product in CurrentFuelVariant.ReactorProducts)
            {
                var massProduced = ProduceReactorProduct(resMan, product, totalPowerReceivedFixed / GeeForceModifier);
                if (product.IsPropellant)
                    _reactorProduction.Add(new ReactorProduction() { FuelMode = product, Mass = massProduced });
            }


            BreedTritium(resMan, ongoing_thermal_power_generated);
            return true;
        }
    }
}
