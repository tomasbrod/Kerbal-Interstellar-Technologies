﻿using System;
using System.Collections.Generic;
using System.Linq;
using KIT.Extensions;
using KIT.Interfaces;
using KIT.PowerManagement;
using KIT.PowerManagement.Interfaces;
using KIT.Propulsion;
using KIT.Resources;
using KIT.ResourceScheduler;
using KIT.Wasteheat;
using KSP.Localization;
using UnityEngine;

namespace KIT.BeamedPower
{
    [KSPModule("#LOC_KSPIE_BeamPowerReceiver_ModuleName1")]//Solar Power Receiver Dish
    class SolarBeamedPowerReceiverDish : SolarBeamedPowerReceiver { } // receives less of a power capacity nerve in NF mode

    [KSPModule("#LOC_KSPIE_BeamPowerReceiver_ModuleName2")]//Solar Power Receiver
    class SolarBeamedPowerReceiver : BeamedPowerReceiver { } // receives less of a power capacity nerve in NF mode

    //---------------------------------------------------------

    [KSPModule("#LOC_KSPIE_BeamPowerReceiver_ModuleName3")]//Microwave Power Receiver Dish
    class MicrowavePowerReceiverDish : MicrowavePowerReceiver { }

    [KSPModule("#LOC_KSPIE_BeamPowerReceiver_ModuleName4")]//Microwave Power Receiver Panel
    class MicrowavePowerReceiverPanel : MicrowavePowerReceiver { }

    [KSPModule("#LOC_KSPIE_BeamPowerReceiver_ModuleName5")]//Microwave Power Receiver
    class MicrowavePowerReceiver : BeamedPowerReceiver { }

    //---------------------------------------------------

    [KSPModule("#LOC_KSPIE_BeamPowerReceiver_ModuleName6")]//Photovoltaic Power Receiver Dish
    class PhotovoltaicPowerReceiverDish : PhotovoltaicPowerReceiver { }

    [KSPModule("#LOC_KSPIE_BeamPowerReceiver_ModuleName7")]//Photovoltaic Power Receiver Dish
    class PhotovoltaicPowerReceiverPanel : PhotovoltaicPowerReceiver { }

    [KSPModule("#LOC_KSPIE_BeamPowerReceiver_ModuleName8")]//Photovoltaic Power Receiver
    class PhotovoltaicPowerReceiver : BeamedPowerReceiver { }

    //---------------------------------------------------

    [KSPModule("#LOC_KSPIE_BeamPowerReceiver_ModuleName9")]//Rectenna Power Receiver Dish
    class RectennaPowerReceiverDish : RectennaPowerReceiver { }

    [KSPModule("#LOC_KSPIE_BeamPowerReceiver_ModuleName10")]//Rectenna Power Receiver Dish
    class RectennaPowerReceiverPanel : RectennaPowerReceiver { }

    [KSPModule("#LOC_KSPIE_BeamPowerReceiver_ModuleName11")]//Rectenna Power Receiver
    class RectennaPowerReceiver : BeamedPowerReceiver { }

    //---------------------------------------------------

    [KSPModule("#LOC_KSPIE_BeamPowerReceiver_ModuleName12")]//Thermal Power Panel Receiver Panel
    class ThermalPowerReceiverPanel : ThermalPowerReceiver { }

    [KSPModule("#LOC_KSPIE_BeamPowerReceiver_ModuleName13")]//Thermal Power Panel Receiver Dish
    class ThermalPowerReceiverDish : ThermalPowerReceiver { }

    [KSPModule("#LOC_KSPIE_BeamPowerReceiver_ModuleName14")]//Thermal Power Receiver
    class ThermalPowerReceiver : BeamedPowerReceiver { }

    //------------------------------------------------------

    [KSPModule("#LOC_KSPIE_BeamPowerReceiver_ModuleName15")]//Beamed Power Receiver Panel
    class BeamedPowerReceiverPanel : BeamedPowerReceiver { }

    [KSPModule("#LOC_KSPIE_BeamPowerReceiver_ModuleName16")]//Beamed Power Receiver Dish
    class BeamedPowerReceiverDish : BeamedPowerReceiver { }

    [KSPModule("#LOC_KSPIE_BeamPowerReceiver_ModuleName17")]//Beamed Power Receiver
    class BeamedPowerReceiver : PartModule, IKITModule, IKITVariableSupplier, IFNPowerSource, IElectricPowerGeneratorSource, IBeamedPowerReceiver // tweakscales with exponent 2.5
    {
        private const int LabelWidth = 200;
        private const int WideLabelWidth = 250;
        private const int ValueWidthWide = 100;
        private const int ValueWidthNormal = 65;
        private const int ValueWidthShort = 30;

        internal const string Group = "BeamedPowerReceiver";
        internal const string GroupTitle = "#LOC_KSPIE_BeamPowerReceiver_groupName";

        [KSPField(isPersistant = true)] public double storedTemp;
        [KSPField(isPersistant = true)] public float windowPositionX = 200;
        [KSPField(isPersistant = true)] public float windowPositionY = 100;
        [KSPField(isPersistant = true)] public bool forceActivateAtStartup;
        [KSPField(isPersistant = true)] protected double total_sat_efficiency_fraction;
        [KSPField(isPersistant = true)] protected double total_beamed_power;
        [KSPField(isPersistant = true)] protected double total_beamed_power_max;
        [KSPField(isPersistant = true)] protected double total_beamed_wasteheat;
        [KSPField(isPersistant = true)] public double thermalSolarInputMegajoules;
        [KSPField(isPersistant = true)] public double thermalSolarInputMegajoulesMax;
        [KSPField(isPersistant = true)] public double storedGeneratorThermalEnergyRequestRatio;
        [KSPField(isPersistant = true)] public double storedIsThermalEnergyGeneratorEfficiency;


        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "#LOC_KSPIE_BeamPowerReceiver_Bandwidth")]//Bandwidth
        [UI_ChooseOption(affectSymCounterparts = UI_Scene.None, scene = UI_Scene.All, suppressEditorShipModified = true)]
        public int selectedBandwidthConfiguration;

        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = false, guiActive = false, guiName = "Max Power Capacity", guiFormat = "F3", guiUnits = " MW")]
        public double maximumReceiverPowerCapacity;


        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = true, guiActive = true, guiName = "Minimum Consumption %"), UI_FloatRange(stepIncrement = 0.5f, maxValue = 100, minValue = 0)]
        public float minimumConsumptionPercentage;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = true, guiActive = true, guiName = "Maximum Consumption %"), UI_FloatRange(stepIncrement = 0.5f, maxValue = 100, minValue = 0)]
        public float maximumConsumptionPercentage = 100;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = true, guiActive = false, guiName = "#LOC_KSPIE_BeamPowerReceiver_EnabledReceiver")]//Enabled
        public bool receiverIsEnabled;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = true, guiActive = true, guiName = "#LOC_KSPIE_BeamPowerReceiver_LinkedforRelay")]//Linked for Relay
        public bool linkedForRelay;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = true, guiActive = false, guiName = "#LOC_KSPIE_BeamPowerReceiver_TargetWavelength", guiFormat = "F5")]//Target Wavelength
        public double targetWavelength;

        //Persistent False
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiName = "#LOC_KSPIE_BeamPowerReceiver_TargetWavelength")]
        public string bandWidthName;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiName = "#LOC_KSPIE_BeamPowerReceiver_WasteheatRatio", guiFormat = "F6")]//Wasteheat Ratio
        public double wasteheatRatio;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiName = "#LOC_KSPIE_BeamPowerReceiver_WasteheatElectricEfficiency", guiFormat = "F6")]//Wasteheat Electric Efficiency
        public double wasteheatElectricConversionEfficiency;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiName = "#LOC_KSPIE_BeamPowerReceiver_BeamedElectricEfficiency", guiFormat = "F6")]//Beamed Electric Efficiency
        public double effectiveBeamedPowerElectricEfficiency;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiName = "#LOC_KSPIE_BeamPowerReceiver_SolarElectricEfficiency", guiFormat = "F6")]//Solar Electric Efficiency
        public double effectiveSolarThermalElectricEfficiency;

        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActiveEditor = true, guiActive = false, guiName = "#LOC_KSPIE_BeamPowerReceiver_ReceiverDiameter", guiFormat = "F3", guiUnits = " m")]//Receiver Diameter
        public double diameter = 1;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActiveEditor = true, guiActive = false, guiName = "#LOC_KSPIE_BeamPowerReceiver_IsThermalReceiverSlave")]//Is Slave
        public bool isThermalReceiverSlave;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActiveEditor = false, guiActive = false, guiName = "#LOC_KSPIE_BeamPowerReceiver_InputPower", guiFormat = "F3", guiUnits = "#LOC_KSPIE_Reactor_megajouleUnit")]//Input Power
        public double powerInputMegajoules;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActiveEditor = false, guiActive = true, guiName = "#LOC_KSPIE_BeamPowerReceiver_MaxInputPower", guiFormat = "F3", guiUnits = "#LOC_KSPIE_Reactor_megajouleUnit")]//Max Input Power
        public double powerInputMegajoulesMax;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActiveEditor = false, guiActive = true, guiName = "#LOC_KSPIE_BeamPowerReceiver_ThermalPower", guiFormat = "F3", guiUnits = "#LOC_KSPIE_Reactor_megajouleUnit")]//Thermal Power
        public double ThermalPower;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActiveEditor = true, guiActive = false, guiName = "#LOC_KSPIE_BeamPowerReceiver_Radius", guiFormat = "F2", guiUnits = " m")]//Radius
        public double radius = 2.5;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = true, guiActiveEditor = true, guiName = "#LOC_KSPIE_BeamPowerReceiver_MinWavelength")]//min Wavelength
        public double minimumWavelength = 0.00000001;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = true, guiActiveEditor = true, guiName = "#LOC_KSPIE_BeamPowerReceiver_MaxWavelength")]//max Wavelength
        public double maximumWavelength = 1;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActiveEditor = true, guiName = "#LOC_KSPIE_BeamPowerReceiver_HotbathTechLevel")]//Hotbath TechLevel
        public int hothBathtechLevel;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiName = "#LOC_KSPIE_BeamPowerReceiver_HotBathTemperature", guiUnits = " K")]//HotBath Temperature
        public double hothBathTemperature = 3200;

        //GUI
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiName = "#LOC_KSPIE_BeamPowerReceiver_CoreTemperature")]//Core Temperature
        public string coreTempererature;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = true, guiName = "#LOC_KSPIE_BeamPowerReceiver_ProducedPower")]//Produced Power
        public string beamedpower;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiName = "#LOC_KSPIE_BeamPowerReceiver_SatellitesConnected")]//Satellites Connected
        public string connectedsats;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiName = "#LOC_KSPIE_BeamPowerReceiver_RelaysConnected")]//Relays Connected
        public string connectedrelays;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiName = "#LOC_KSPIE_BeamPowerReceiver_NetworkDepth")]//Network Depth
        public string networkDepthString;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiName = "#LOC_KSPIE_BeamPowerReceiver_SlavesAmount")]//Connected Slaves
        public int slavesAmount;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiName = "#LOC_KSPIE_BeamPowerReceiver_SlavesPower", guiUnits = "#LOC_KSPIE_Reactor_megawattUnit", guiFormat = "F2")]//Slaves Power
        public double slavesPower;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiName = "#LOC_KSPIE_BeamPowerReceiver_AvailableThermalPower", guiUnits = "#LOC_KSPIE_Reactor_megawattUnit", guiFormat = "F2")]//Available Thermal Power
        public double total_thermal_power_available;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = true, guiName = "#LOC_KSPIE_BeamPowerReceiver_AvailableThermalPower", guiUnits = "#LOC_KSPIE_Reactor_megawattUnit", guiFormat = "F2")]//Thermal Power Supply
        public double total_thermal_power_provided;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = true, guiName = "#LOC_KSPIE_BeamPowerReceiver_MaxThermalPowerSupply", guiUnits = "#LOC_KSPIE_Reactor_megawattUnit", guiFormat = "F2")]//Max Thermal Power Supply
        public double total_thermal_power_provided_max;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KSPIE_BeamPowerReceiver_MaximumInputPower", guiUnits = "#LOC_KSPIE_Reactor_megawattUnit", guiFormat = "F3")]//Maximum Input Power
        public double maximumPower;
        /*
        [KSPField]
        public double maximumElectricPower = 0;
        */
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiActiveEditor = true, guiName = "#LOC_KSPIE_BeamPowerReceiver_MaximumElectricPower", guiUnits = "#LOC_KSPIE_Reactor_megawattUnit", guiFormat = "F3")]//Maximum Electric Power
        public double maximumThermalPower;

        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiName = "#LOC_KSPIE_BeamPowerReceiver_Dissipation", guiUnits = "#LOC_KSPIE_Reactor_megawattUnit", guiFormat = "F3")]//Dissipation
        public double dissipationInMegaJoules;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiName = "#LOC_KSPIE_BeamPowerReceiver_SolarFacingFactor", guiFormat = "F4")]//Sun Facing Factor
        public double solarFacingFactor;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActive = false, guiName = "#LOC_KSPIE_BeamPowerReceiver_SolarFlux", guiFormat = "F4")]//Solar Flux
        public double solarFlux;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = true, guiActive = true, guiName = "#LOC_KSPIE_BeamPowerReceiver_PowerMode"), UI_Toggle(disabledText = "#LOC_KSPIE_BeamPowerReceiver_ElectricMode", enabledText = "#LOC_KSPIE_BeamPowerReceiver_ThermalMode")]//Power Mode--Electric--Thermal
        public bool thermalMode;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = true, guiActive = false, guiName = "#LOC_KSPIE_BeamPowerReceiver_RadiatorMode"), UI_Toggle(disabledText = "#LOC_KSPIE_BeamPowerReceiver_BeamedPowerMode", enabledText = "#LOC_KSPIE_BeamPowerReceiver_RadiatorMode")]//Function--Beamed Power--Radiator
        public bool radiatorMode;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = true, guiActive = true, guiName = "#LOC_KSPIE_BeamPowerReceiver_SolarPowerMode"), UI_Toggle(disabledText = "#LOC_KSPIE_BeamPowerReceiver_BeamedPowerMode", enabledText = "#LOC_KSPIE_BeamPowerReceiver_SolarMode")]//Power Mode--Beamed Power--Solar Only
        public bool solarPowerMode = true;
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = true, guiActive = true, guiName = "#LOC_KSPIE_BeamPowerReceiver_ShowWindow"), UI_Toggle(disabledText = "#LOC_KSPIE_BeamPowerReceiver_WindowHide", enabledText = "#LOC_KSPIE_BeamPowerReceiver_WindowShow")]//Power Reciever Interface--Hidden--Shown
        public bool showWindow;

        // Settings
        [KSPField] public bool isThermalReceiver;
        [KSPField] public bool isEnergyReceiver = true;
        [KSPField] public bool maintainResourceBuffers = true;
        [KSPField] public bool canSwitchBandwidthInEditor;
        [KSPField] public bool canSwitchBandwidthInFlight;
        [KSPField] public bool autoDeploy = true;
        [KSPField] public bool canLinkup = true;
        [KSPField] public bool isMirror;
        [KSPField] public bool showBandWidthName;
        [KSPField] public bool showSelectedBandwidthConfiguration = true;

        [KSPField] public int connectStackdepth = 4;
        [KSPField] public int connectParentdepth = 2;
        [KSPField] public int connectSurfacedepth = 2;
        [KSPField] public int receiverType;
        [KSPField] public int supportedPropellantAtoms = 511;
        [KSPField] public int supportedPropellantTypes = 127;

        [KSPField] public double maximumElectricPower;
        [KSPField] public double electricWasteheatExponent = 1;
        [KSPField] public double electricMaxEfficiency = 1;
        [KSPField] public double facingThreshold;
        [KSPField] public double facingSurfaceExponent = 1;
        [KSPField] public double facingEfficiencyExponent = 0.1;
        [KSPField] public double spotsizeNormalizationExponent = 1;
        [KSPField] public double solarReceptionEfficiency;
        [KSPField] public double solarElectricEfficiency = 0.33;
        [KSPField] public double solarReceptionSurfaceArea;
        [KSPField] public double solarFacingExponent = 1;
        [KSPField] public double alternatorRatio = 1;
        [KSPField] public double minCoolingFactor = 1;
        [KSPField] public double engineHeatProductionMult = 1;
        [KSPField] public double plasmaHeatProductionMult = 1;
        [KSPField] public double engineWasteheatProductionMult = 1;
        [KSPField] public double plasmaWasteheatProductionMult = 1;
        [KSPField] public double heatTransportationEfficiency = 0.7;
        [KSPField] public double thermalPowerBufferMult = 2;
        [KSPField] public double wasteHeatMultiplier = 1;
        [KSPField] public double wasteHeatModifier = 1;
        [KSPField] public double apertureMultiplier = 1;
        [KSPField] public double highSpeedAtmosphereFactor;
        [KSPField] public double atmosphereToleranceModifier = 1;
        [KSPField] public double thermalPropulsionEfficiency = 1;
        [KSPField] public double thermalEnergyEfficiency = 1;
        [KSPField] public double thermalProcessingModifier = 1;

        [KSPField] public double hothBathTemperatureMk1 = 2000;
        [KSPField] public double hothBathTemperatureMk2 = 2500;
        [KSPField] public double hothBathTemperatureMk3 = 3000;
        [KSPField] public double hothBathTemperatureMk4 = 3500;
        [KSPField] public double hothBathTemperatureMk5 = 4000;
        [KSPField] public double hothBathTemperatureMk6 = 4500;

        [KSPField] public string animName = "";
        [KSPField] public string animTName = "";
        [KSPField] public string upgradeTechReqMk2 = "heatManagementSystems";
        [KSPField] public string upgradeTechReqMk3 = "advHeatManagement";
        [KSPField] public string upgradeTechReqMk4 = "specializedRadiators";
        [KSPField] public string upgradeTechReqMk5 = "exoticRadiators";
        [KSPField] public string upgradeTechReqMk6 = "extremeRadiators";

        [KSPField] public double powerCapacityEfficiency;
        [KSPField] public double powerMult = 1;
        [KSPField] public double averageEfficiencyFraction;
        [KSPField] public double currentIsThermalEnergyGeneratorEfficiency;
        [KSPField] public double currentGeneratorThermalEnergyRequestRatio;

        protected BaseField _beamedpowerField;
        protected BaseField _powerInputMegajoulesField;
        protected BaseField _linkedForRelayField;
        protected BaseField _diameterField;
        protected BaseField _slavesAmountField;
        protected BaseField _ThermalPowerField;
        protected BaseField _minimumConsumptionPercentageField;
        protected BaseField _maximumConsumptionPercentageField;
        protected BaseField _selectedBandwidthConfigurationField;
        protected BaseField _maximumWavelengthField;
        protected BaseField _minimumWavelengthField;
        protected BaseField _solarFacingFactorField;
        protected BaseField _solarFluxField;
        protected BaseField _coreTempereratureField;
        protected BaseField _field_kerbalism_output;

        protected BaseField _bandWidthNameField;
        protected BaseField _connectedsatsField;
        protected BaseField _connectedrelaysField;
        protected BaseField _networkDepthStringField;

        protected BaseEvent _linkReceiverBaseEvent;
        protected BaseEvent _unlinkReceiverBaseEvent;
        protected BaseEvent _activateReceiverBaseEvent;
        protected BaseEvent _disableReceiverBaseEvent;

        protected ModuleDeployableSolarPanel DeployableSolarPanel;
        protected ModuleDeployableRadiator DeployableRadiator;
        protected ModuleDeployableAntenna DeployableAntenna;

        protected FNRadiator FNRadiator;

        public Queue<double> BeamedPowerQueue = new Queue<double>(10);
        public Queue<double> BeamedPowerMaxQueue = new Queue<double>(10);
        public Queue<double> SolarFluxQueue = new Queue<double>(50);

        //Internal
        protected bool IsLoaded;
        protected double TotalConversionWasteHeatProduction;
        protected double ConnectedReceiversSum;
        protected int InitializationCountdown;
        protected double PowerDownFraction;

        protected PowerStates PowerState;
        protected List<IFnEngineNozzle> ConnectedEngines = new List<IFnEngineNozzle>();
        protected Dictionary<Vessel, ReceivedPowerData> ReceivedPower = new Dictionary<Vessel, ReceivedPowerData>();
        protected List<BeamedPowerReceiver> ThermalReceiverSlaves = new List<BeamedPowerReceiver>();

        // reference types
        protected Dictionary<Guid, double> ConnectedReceivers = new Dictionary<Guid, double>();
        protected Dictionary<Guid, double> ConnectedReceiversFraction = new Dictionary<Guid, double>();

        protected GUIStyle BoldBlackStyle;
        protected GUIStyle TextBlackStyle;

        // GUI elements declaration
        private Rect _windowPosition;
        private int _windowID;

        private int _restartCounter;

        public void Restart(int counter)
        {
            _restartCounter = counter;
        }

        public void RemoveOtherVesselData()
        {
            var deleteList = new List<Vessel>();

            foreach (var r in ReceivedPower)
            {
                if (r.Key != vessel)
                    deleteList.Add(r.Key);
            }

            foreach (Vessel otherVessel in deleteList)
            {
                ReceivedPower.Remove(otherVessel);
            }
        }

        public void Reset()
        {
            Debug.Log("[KSPI]: BeamedPowerReceiver reset called");
            ReceivedPower.Clear();
        }

        public void UseProductForPropulsion(IResourceManager resMan, double ratio, double propellantMassPerSecond, PartResourceDefinition resource)
        {
            // do nothing
        }

        public double FuelRatio => 1;
        public double MagneticNozzlePowerMult => 1;
        public bool MayExhaustInAtmosphereHomeworld => true;
        public bool MayExhaustInLowSpaceHomeworld => true;
        public double MinThermalNozzleTempRequired => 0;
        public double CurrentMeVPerChargedProduct => 0;
        public bool UsePropellantBaseIsp => false;
        public bool CanUseAllPowerForPlasma => false;
        public bool CanProducePower => ProducedThermalHeat > 0;
        public double MinCoolingFactor => minCoolingFactor;
        public double EngineHeatProductionMultiplier => engineHeatProductionMult;
        public double PlasmaHeatProductionMultiplier => plasmaHeatProductionMult;
        public double EngineWasteheatProductionMultiplier => engineWasteheatProductionMult;
        public double PlasmaWasteheatProductionMultiplier => plasmaWasteheatProductionMult;
        public int ReceiverType => receiverType;
        public double Diameter => diameter;
        public double ApertureMultiplier => apertureMultiplier;
        public double MaximumWavelength => maximumWavelength;
        public double MinimumWavelength => minimumWavelength;
        public double HighSpeedAtmosphereFactor => highSpeedAtmosphereFactor;
        public double FacingThreshold => facingThreshold;
        public double FacingSurfaceExponent => facingSurfaceExponent;
        public double FacingEfficiencyExponent => facingEfficiencyExponent;
        public double SpotSizeNormalizationExponent => spotsizeNormalizationExponent;
        public Part Part => part;
        public Vessel Vessel => vessel;
        public int ProviderPowerPriority => 1;
        public double ConsumedFuelFixed => 0;
        public double ProducedThermalHeat => powerInputMegajoules;
        public double ProducedChargedPower => 0;
        public double PowerRatio => maximumConsumptionPercentage / 100d;
        public double ProducedPower => ProducedThermalHeat;
        public bool SupportMHD => false;
        public double MinimumThrottle => 0;
        public int SupportedPropellantAtoms => supportedPropellantAtoms;
        public int SupportedPropellantTypes => supportedPropellantTypes;
        public bool FullPowerForNonNeutronAbsorbents => true;
        public double ReactorSpeedMult => 1;
        public double ThermalProcessingModifier => thermalProcessingModifier;
        public double ThermalPropulsionWasteHeatModifier => 1;
        public double EfficiencyConnectedThermalEnergyGenerator => storedIsThermalEnergyGeneratorEfficiency;
        public double EfficiencyConnectedChargedEnergyGenerator => 0;

        /*
        public double PowerCapacityEfficiency
        {
            get
            {
                if (!HighLogic.LoadedSceneIsFlight || CheatOptions.IgnoreMaxTemperature || isThermalReceiver)
                    return 1;

                // TODO may this better.. OnAwake, set the ResourceName id?
                // var wasteheatRatio = getResourceBarRatio(ResourceSettings.Config.WasteHeatInMegawatt);
                part.GetConnectedResourceTotals("WasteHeat".GetHashCode(), out var amount, out var max);
                wasteheatRatio = amount / max;

                return 1 - wasteheatRatio * wasteheatRatio;
            }
        }
        */

        public void FindAndAttachToPowerSource()
        {
            // do nothing
        }

        private void DetermineTechLevel()
        {
            hothBathtechLevel = 1;
            if (PluginHelper.UpgradeAvailable(upgradeTechReqMk2)) hothBathtechLevel++;
            if (PluginHelper.UpgradeAvailable(upgradeTechReqMk3)) hothBathtechLevel++;
            if (PluginHelper.UpgradeAvailable(upgradeTechReqMk4)) hothBathtechLevel++;
            if (PluginHelper.UpgradeAvailable(upgradeTechReqMk5)) hothBathtechLevel++;
            if (PluginHelper.UpgradeAvailable(upgradeTechReqMk6)) hothBathtechLevel++;
        }

        private void DetermineCoreTemperature()
        {
            switch (hothBathtechLevel)
            {
                case 1: hothBathTemperature = hothBathTemperatureMk1; break;
                case 2: hothBathTemperature = hothBathTemperatureMk2; break;
                case 3: hothBathTemperature = hothBathTemperatureMk3; break;
                case 4: hothBathTemperature = hothBathTemperatureMk4; break;
                case 5: hothBathTemperature = hothBathTemperatureMk5; break;
                case 6: hothBathTemperature = hothBathTemperatureMk6; break;
                default:
                    break;
            }
        }

        public double WasteheatElectricConversionEfficiency
        {
            get
            {
                if (!HighLogic.LoadedSceneIsFlight || CheatOptions.IgnoreMaxTemperature || electricWasteheatExponent == 0) return 1;

                if (electricWasteheatExponent == 1)
                    return 1 - wasteheatRatio;
                else
                    return 1 - Math.Pow(wasteheatRatio, electricWasteheatExponent);
            }
        }

        public double MaximumRecievePower
        {
            get
            {
                var maxPower = thermalMode && maximumThermalPower > 0
                    ? maximumThermalPower
                    : maximumElectricPower > 0
                        ? maximumElectricPower
                        : maximumPower;

                var scaledPower = maxPower * powerMult;
                return CanBeActiveInAtmosphere ? scaledPower : scaledPower * highSpeedAtmosphereFactor;
            }
        }

        public double AverageEfficiencyFraction
        {
            get
            {
                averageEfficiencyFraction = total_beamed_power > 0 ? total_sat_efficiency_fraction / total_beamed_power : 0;
                return averageEfficiencyFraction;
            }
        }

        public void RegisterAsSlave(BeamedPowerReceiver receiver)
        {
            ThermalReceiverSlaves.Add(receiver);
        }

        public void ConnectWithEngine(IEngineNozzle engine)
        {
            var fnEngine = engine as IFnEngineNozzle;
            if (fnEngine == null)
                return;

            if (!ConnectedEngines.Contains(fnEngine))
                ConnectedEngines.Add(fnEngine);
        }

        public void DisconnectWithEngine(IEngineNozzle engine)
        {
            var fnEngine = engine as IFnEngineNozzle;
            if (fnEngine == null)
                return;

            if (ConnectedEngines.Contains(fnEngine))
                ConnectedEngines.Remove(fnEngine);
        }

        public IElectricPowerGeneratorSource ConnectedThermalElectricGenerator { get; set; }

        public IElectricPowerGeneratorSource ConnectedChargedParticleElectricGenerator { get; set; }

        public void NotifyActiveThermalEnergyGenerator(double efficiency, double powerRatio, bool isMHD, double mass)
        {
            NotifyActiveThermalEnergyGenerator(efficiency, powerRatio);
        }

        public void NotifyActiveThermalEnergyGenerator(double efficiency, double powerRatio)
        {
            currentIsThermalEnergyGeneratorEfficiency = efficiency;
            currentGeneratorThermalEnergyRequestRatio = powerRatio;
        }

        public void NotifyActiveChargedEnergyGenerator(double efficiency, double powerRatio) { }

        public void NotifyActiveChargedEnergyGenerator(double efficiency, double powerRatio, double mass) { }

        public bool IsThermalSource => isThermalReceiver;

        public double RawMaximumPowerForPowerGeneration => powerInputMegajoulesMax;

        public double RawMaximumPower => MaximumRecievePower;

        public bool ShouldApplyBalance(ElectricGeneratorType generatorType) { return false; }

        public void AttachThermalReceiver(Guid key, double radius)
        {
            if (!ConnectedReceivers.ContainsKey(key))
            {
                ConnectedReceivers.Add(key, radius);
                ConnectedReceiversSum = ConnectedReceivers.Sum(r => r.Value);
                ConnectedReceiversFraction = ConnectedReceivers.ToDictionary(a => a.Key, a => a.Value / ConnectedReceiversSum);
            }
        }

        public double ProducedWasteHeat => TotalConversionWasteHeatProduction;

        public void Refresh() { }

        public void DetachThermalReceiver(Guid key)
        {
            if (ConnectedReceivers.ContainsKey(key))
            {
                ConnectedReceivers.Remove(key);
                ConnectedReceiversSum = ConnectedReceivers.Sum(r => r.Value);
                ConnectedReceiversFraction = ConnectedReceivers.ToDictionary(a => a.Key, a => a.Value / ConnectedReceiversSum);
            }
        }

        public double GetFractionThermalReceiver(Guid key)
        {
            return ConnectedReceiversFraction.TryGetValue(key, out double result) ? result : 0.0;
        }

        protected Animation animT;

        protected BeamedPowerTransmitter part_transmitter;
        protected ModuleAnimateGeneric genericAnimation;

        protected CelestialBody localStar;

        protected int connectedsatsi;
        protected int connectedrelaysi;
        protected int networkDepth;
        protected int activeSatsIncr;
        protected long deactivate_timer;
        protected bool has_transmitter;

        public double RawTotalPowerProduced => ThermalPower * TimeWarp.fixedDeltaTime;
        public double ChargedPowerRatio => 0;
        public double PowerBufferBonus => 0;
        public double ThermalTransportationEfficiency => heatTransportationEfficiency;
        public double ThermalPropulsionEfficiency => thermalPropulsionEfficiency;
        public double PlasmaPropulsionEfficiency => 0;
        public double ChargedParticlePropulsionEfficiency => 0;
        public double ThermalEnergyEfficiency => thermalEnergyEfficiency;
        public double PlasmaEnergyEfficiency => 0;
        public double ChargedParticleEnergyEfficiency => 0;
        public bool IsSelfContained => false;
        public double CoreTemperature => hothBathTemperature;
        public double MaxCoreTemperature => hothBathTemperature;
        public double HotBathTemperature => hothBathTemperature;
        public double StableMaximumReactorPower => RawMaximumPower;
        public double MaximumPower => MaximumThermalPower;
        public double MaximumThermalPower => HighLogic.LoadedSceneIsEditor ? maximumThermalPower * powerMult : ThermalPower;
        public double NormalisedMaximumPower => ThermalPower;
        public double MaximumChargedPower => 0;
        public double MinimumPower => 0;
        public bool IsVolatileSource => true;
        public bool IsActive => receiverIsEnabled;
        public bool IsNuclear => false;

        [KSPAction("Toggle Receiver Interface")]
        public void ToggleWindow()
        {
            showWindow = !showWindow;
        }

        private void ActivateReceiverState(bool forced = false)
        {
            receiverIsEnabled = true;
            forceActivateAtStartup = true;

            // force activate to trigger any fairings and generators
            Debug.Log("[KSPI]: BeamedPowerReceiver was force activated on  " + part.name);
            part.force_activate();

            ShowDeployAnimation(forced);
        }

        private void ShowDeployAnimation(bool forced)
        {
            Debug.Log("[KSPI]: MicrowaveReceiver ShowDeployAnimation is called ");

            if (DeployableAntenna != null)
                DeployableAntenna.Extend();

            if (DeployableSolarPanel != null)
                DeployableSolarPanel.Extend();

            if (DeployableRadiator != null)
                DeployableRadiator.Extend();

            if (genericAnimation != null && genericAnimation.GetScalar < 1)
                genericAnimation.Toggle();

            if (FNRadiator != null && FNRadiator.ModuleActiveRadiator != null)
                FNRadiator.ModuleActiveRadiator.Activate();
        }

        private void DeactivateReceiverState(bool forced = false)
        {
            receiverIsEnabled = false;
            forceActivateAtStartup = false;

            ShowUndeployAnimation(forced);
        }

        private void ShowUndeployAnimation(bool forced)
        {
            if (DeployableAntenna != null)
                DeployableAntenna.Retract();

            if (DeployableSolarPanel != null)
                DeployableSolarPanel.Retract();

            if (DeployableRadiator != null)
                DeployableRadiator.Retract();

            if (genericAnimation != null && genericAnimation.GetScalar > 0)
                genericAnimation.Toggle();

            if (FNRadiator != null && FNRadiator.ModuleActiveRadiator != null)
                FNRadiator.ModuleActiveRadiator.Shutdown();
        }

        [KSPEvent(groupName = Group, guiActive = true, guiName = "#LOC_KSPIE_BeamPowerReceiver_LinkReceiver", active = true)]//Link Receiver for Relay
        public void LinkReceiver()
        {
            linkedForRelay = true;

            ShowDeployAnimation(true);
        }

        [KSPEvent(groupName = Group, guiActive = true, guiName = "#LOC_KSPIE_BeamPowerReceiver_UnlinkReceiver", active = true)]//Unlink Receiver for Relay
        public void UnlinkReceiver()
        {
            linkedForRelay = false;

            ShowUndeployAnimation(true);
        }

        [KSPEvent(groupName = Group, guiActive = true, guiActiveEditor = true, guiName = "#LOC_KSPIE_BeamPowerReceiver_ActivateReceiver", active = true)]//Activate Receiver
        public void ActivateReceiver()
        {
            ActivateReceiverState();
        }

        [KSPEvent(groupName = Group, guiActive = true, guiActiveEditor = true, guiName = "#LOC_KSPIE_BeamPowerReceiver_DisableReceiver", active = true)]//Disable Receiver
        public void DisableReceiver()
        {
            DeactivateReceiverState();
        }

        [KSPAction("Activate Receiver")]
        public void ActivateReceiverAction(KSPActionParam param)
        {
            ActivateReceiver();
        }

        [KSPAction("Disable Receiver")]
        public void DisableReceiverAction(KSPActionParam param)
        {
            DisableReceiver();
        }

        [KSPAction("Toggle Receiver")]
        public void ToggleReceiverAction(KSPActionParam param)
        {
            if (receiverIsEnabled)
                DisableReceiver();
            else
                ActivateReceiver();
        }

        public void SetActiveBandwidthConfigurationByWaveLength(double targetWaveLength)
        {
            var foundBandwidthConfiguration = BandwidthConverters?.FirstOrDefault(m => m.minimumWavelength < targetWaveLength && m.maximumWavelength > targetWaveLength);

            if (foundBandwidthConfiguration == null) return;

            IsLoaded = true;
            Debug.Log("[KSPI]: BeamedPowerReceiver - found " + foundBandwidthConfiguration.bandwidthName);
            ActiveBandwidthConfiguration = foundBandwidthConfiguration;
            UpdateProperties();
            selectedBandwidthConfiguration = BandwidthConverters.IndexOf(ActiveBandwidthConfiguration);
            UpdatePartActionWindow();
        }

        private void UpdatePartActionWindow()
        {
            var window = FindObjectsOfType<UIPartActionWindow>().FirstOrDefault(w => w.part == part);
            if (window == null) return;

            foreach (var actionWindow in FindObjectsOfType<UIPartActionWindow>())
            {
                if (window.part != part) continue;
                actionWindow.ClearList();
                actionWindow.displayDirty = true;
            }
        }

        public BandwidthConverter ActiveBandwidthConfiguration;

        private List<BandwidthConverter> _bandwidthConverters;
        public List<BandwidthConverter> BandwidthConverters => _bandwidthConverters;

        // Lambda= Wavelength in nanometers, time in seconds, wattage in normal Watts, returns momentum of whole laser
        public static double PhotonicLaserMomentum(double lambda, uint time, ulong wattage)
        {
            var energySingle = 6.626e-34 * 3e8 / lambda;
            var photonImpulse = wattage * time / energySingle;
            var momentumSingle = 6.626e-34 / lambda;
            var momentumWhole = momentumSingle * photonImpulse;

            return 2 * momentumWhole; //output is in Newtons per second
        }

        //lambda = Wavelength in nanometers, time in seconds, wattage in normal Watts, returns momentum of whole laser
        public static double PhotonicLaserMomentum2(double lambda, int time, long wattage)
        {
            double energyLaser = wattage * time;

            var energySingle = 6.626e-34 * 3e8 / lambda;
            var photonImpulse = energyLaser / energySingle;
            var momentumSingle = (6.626e-34 / (3e8 * lambda)) * 3e8;
            var momentumWhole = momentumSingle * photonImpulse;

            return 2 * momentumWhole;
        }

        public override void OnStart(StartState state)
        {
            //string[] resources_to_supply = new [] {ResourceSettings.Config.ElectricPowerInMegawatt, ResourceSettings.Config.WasteHeatInMegawatt, ResourceSettings.Config.ThermalPowerInMegawatt};

            // this.resources_to_supply = resources_to_supply;

            if (BandwidthConverters == null)
            {
                var rootNode = GameDatabase.Instance.GetConfigNodes("KIT_BandwidthConverters");
                if (rootNode == null || !rootNode.Any())
                {
                    Debug.Log($"[KIT] Beamed Power Receiver OnStart, {(rootNode == null ? "can't find KIT_BandwidthConverters" : "it's empty")}");
                    return;
                }

                var partNode = rootNode[0].GetNode(part.partInfo.name);
                if (partNode == null)
                {
                    Debug.Log($"[KIT] Beamed Power Receiver OnStart, can't find KIT_BandwidthConverters.{part.partInfo.name}");
                    return;
                }

                OnLoad(partNode);
            }

            DetermineTechLevel();
            DetermineCoreTemperature();

            // while in edit mode, listen to on attach/detach event
            if (state == StartState.Editor)
            {
                part.OnEditorAttach += OnEditorAttach;
                part.OnEditorDetach += OnEditorDetach;
            }

            InitializeThermalModeSwitcher();

            InitializeBandwidthSelector();

            Fields[nameof(hothBathtechLevel)].guiActiveEditor = isThermalReceiver;
            Fields[nameof(hothBathTemperature)].guiActiveEditor = isThermalReceiver;

            _linkReceiverBaseEvent = Events[nameof(LinkReceiver)];
            _unlinkReceiverBaseEvent = Events[nameof(UnlinkReceiver)];
            _activateReceiverBaseEvent = Events[nameof(ActivateReceiver)];
            _disableReceiverBaseEvent = Events[nameof(DisableReceiver)];

            coreTempererature = CoreTemperature.ToString("0.0") + " K";
            _coreTempereratureField = Fields[nameof(coreTempererature)];

            if (IsThermalSource && !isThermalReceiverSlave)
            {
                _coreTempereratureField.guiActive = true;
                _coreTempereratureField.guiActiveEditor = true;
            }
            else
            {
                _coreTempereratureField.guiActive = false;
                _coreTempereratureField.guiActiveEditor = false;
            }

            // Determine currently maximum and minimum wavelength
            if (BandwidthConverters.Any())
            {
                if (canSwitchBandwidthInEditor)
                {
                    minimumWavelength = ActiveBandwidthConfiguration.minimumWavelength;
                    maximumWavelength = ActiveBandwidthConfiguration.maximumWavelength;
                }
                else
                {
                    minimumWavelength = BandwidthConverters.Min(m => m.minimumWavelength);
                    maximumWavelength = BandwidthConverters.Max(m => m.maximumWavelength);
                }
            }

            DeployableAntenna = part.FindModuleImplementing<ModuleDeployableAntenna>();
            if (DeployableAntenna != null)
            {
                DeployableAntenna.Events[nameof(ModuleDeployableAntenna.Extend)].guiActive = false;
                DeployableAntenna.Events[nameof(ModuleDeployableAntenna.Retract)].guiActive = false;
            }

            DeployableSolarPanel = part.FindModuleImplementing<ModuleDeployableSolarPanel>();
            HideSlavePartModules();

            var isInSolarModeField = Fields[nameof(solarPowerMode)];
            isInSolarModeField.guiActive = DeployableSolarPanel != null || solarReceptionSurfaceArea > 0;
            isInSolarModeField.guiActiveEditor = DeployableSolarPanel != null || solarReceptionSurfaceArea > 0;

            var dissipationInMegaJoulesField = Fields[nameof(dissipationInMegaJoules)];
            dissipationInMegaJoulesField.guiActive = isMirror;

            if (DeployableSolarPanel == null)
                solarPowerMode = false;

            if (!isMirror)
            {
                FNRadiator = part.FindModuleImplementing<FNRadiator>();
                if (FNRadiator != null)
                {
                    if (FNRadiator.isDeployable)
                    {
                        _activateReceiverBaseEvent.guiName = Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_Deploy");//"Deploy"
                        _disableReceiverBaseEvent.guiName = Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_Retract");//"Retract"
                    }
                    else
                    {
                        _activateReceiverBaseEvent.guiName = Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_Enable");//"Enable"
                        _disableReceiverBaseEvent.guiName = Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_Disable");//"Disable"
                    }

                    FNRadiator.showControls = false;
                    FNRadiator.canRadiateHeat = radiatorMode;
                    FNRadiator.radiatorIsEnabled = radiatorMode;
                }

                var radiatorModeField = Fields[nameof(radiatorMode)];
                radiatorModeField.guiActive = FNRadiator != null;
                radiatorModeField.guiActiveEditor = FNRadiator != null;
            }

            if (state != StartState.Editor)
            {
                _windowPosition = new Rect(windowPositionX, windowPositionY, LabelWidth * 2 + ValueWidthWide * 1 + ValueWidthNormal * 10, 100);

                // create the id for the GUI window
                _windowID = new System.Random(part.GetInstanceID()).Next(int.MinValue, int.MaxValue);

                localStar = GetCurrentStar();

                // compensate for stock solar initialization heating bug
                InitializationCountdown = 10;

                if (forceActivateAtStartup)
                {
                    Debug.Log("[KSPI]: BeamedPowerReceiver on " + part.name + " was Force Activated");
                    part.force_activate();
                }

                if (isThermalReceiverSlave)
                {
                    var result = PowerSourceSearchResult.BreadthFirstSearchForThermalSource(part, (s) => s is BeamedPowerReceiver receiver && receiver != this, connectStackdepth, connectParentdepth, connectSurfacedepth, true);

                    if (result?.Source == null)
                        Debug.LogWarning("[KSPI]: MicrowavePowerReceiver - BreadthFirstSearchForThermalSource-Failed to find Thermal receiver");
                    else
                        ((BeamedPowerReceiver)result.Source).RegisterAsSlave(this);
                }

                // look for any transmitter partModule
                part_transmitter = part.FindModuleImplementing<BeamedPowerTransmitter>();
                if (part_transmitter != null)
                    has_transmitter = true;

                DeployableRadiator = part.FindModuleImplementing<ModuleDeployableRadiator>();
                if (DeployableRadiator != null)
                    DeployableRadiator.Events[nameof(ModuleDeployableRadiator.Extend)].guiActive = false;

                if (!string.IsNullOrEmpty(animTName))
                {
                    animT = part.FindModelAnimators(animTName).FirstOrDefault();
                    if (animT != null)
                    {
                        animT[animTName].enabled = true;
                        animT[animTName].layer = 1;
                        animT[animTName].normalizedTime = 0;
                        animT[animTName].speed = 0.001f;

                        animT.Sample();
                    }
                }

                genericAnimation = part.FindModulesImplementing<ModuleAnimateGeneric>().FirstOrDefault(m => m.animationName == animName);
            }
        }

        private void HideSlavePartModules()
        {
            if (DeployableSolarPanel != null)
            {
                var extendEvent = DeployableSolarPanel.Events[nameof(ModuleDeployableSolarPanel.Extend)];
                extendEvent.guiActive = false;
                extendEvent.guiActive = false;

                var retractEvent = DeployableSolarPanel.Events[nameof(ModuleDeployableSolarPanel.Retract)];
                retractEvent.guiActive = false;
                retractEvent.guiActive = false;
            }
        }

        private void UpdatePowerState()
        {
            PowerDownFraction = 1;
            PowerState = PowerStates.PowerOnline;
        }

        private void PowerDown()
        {
            if (PowerState == PowerStates.PowerOffline) return;

            if (PowerDownFraction > 0)
                PowerDownFraction -= 0.01;

            if (PowerDownFraction <= 0)
                PowerState = PowerStates.PowerOffline;
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
                Debug.LogError("[KSPI]: MicrowavePowerReceiver.OnEditorAttach " + e.Message);
            }
        }

        /// <summary>
        /// Event handler called when part is detached from vessel
        /// </summary>
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

        public override void OnSave(ConfigNode node)
        {
            foreach (var converter in BandwidthConverters)
            {
                converter.Save(node);
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            Debug.Log($"[KSPI Beam Power Receiver] Load()ing");
            base.OnLoad(node);

            part.temperature = storedTemp;
            part.skinTemperature = storedTemp;

            var bandwidthConverterNodes = node.GetNodes("BandwidthConverter");

            if (!bandwidthConverterNodes.Any())
            {
                // Happens when KSP is loading
                return;
            }

            var inlineBandwidthConverters = new List<BandwidthConverter>();

            foreach (var converterNode in bandwidthConverterNodes)
            {
                var converter = new BandwidthConverter(converterNode, part.partInfo.title);
                if (converter.isValid)
                    inlineBandwidthConverters.Add(converter);
                else Debug.Log($"[KSPI] OnLoad unable to parse BandwidthConverter, and it's not valid");
            }

            _bandwidthConverters = inlineBandwidthConverters.OrderByDescending(m => m.minimumWavelength).ToList();
        }

        private void InitializeThermalModeSwitcher()
        {
            // ensure valid values
            if (isThermalReceiver && !isEnergyReceiver)
                thermalMode = true;
            else if (!isThermalReceiver && isEnergyReceiver)
                thermalMode = false;

            var isInThermalModeField = Fields[nameof(thermalMode)];

            isInThermalModeField.guiActive = isThermalReceiver && isEnergyReceiver;
            isInThermalModeField.guiActiveEditor = isThermalReceiver && isEnergyReceiver;
        }

        private void InitializeBandwidthSelector()
        {
            Debug.Log("[KSPI]: Setup Receiver BandWidth Configurations for " + part.partInfo.title);

            _powerInputMegajoulesField = Fields[nameof(powerInputMegajoules)];
            _maximumWavelengthField = Fields[nameof(maximumWavelength)];
            _minimumWavelengthField = Fields[nameof(minimumWavelength)];
            _solarFacingFactorField = Fields[nameof(solarFacingFactor)];
            _linkedForRelayField = Fields[nameof(linkedForRelay)];
            _slavesAmountField = Fields[nameof(slavesAmount)];
            _ThermalPowerField = Fields[nameof(ThermalPower)];
            _minimumConsumptionPercentageField = Fields[nameof(minimumConsumptionPercentage)];
            _maximumConsumptionPercentageField = Fields[nameof(maximumConsumptionPercentage)];
            _beamedpowerField = Fields[nameof(beamedpower)];
            _solarFluxField = Fields[nameof(solarFlux)];
            _diameterField = Fields[nameof(diameter)];

            _connectedsatsField = Fields[nameof(connectedsats)];
            _connectedrelaysField = Fields[nameof(connectedrelays)];
            _networkDepthStringField = Fields[nameof(networkDepthString)];

            _bandWidthNameField = Fields[nameof(bandWidthName)];
            _bandWidthNameField.guiActiveEditor = showBandWidthName || !canSwitchBandwidthInEditor;
            _bandWidthNameField.guiActive = showBandWidthName || !canSwitchBandwidthInFlight;

            _selectedBandwidthConfigurationField = Fields[nameof(selectedBandwidthConfiguration)];
            _selectedBandwidthConfigurationField.guiActiveEditor = showSelectedBandwidthConfiguration && canSwitchBandwidthInEditor;
            _selectedBandwidthConfigurationField.guiActive = showSelectedBandwidthConfiguration && canSwitchBandwidthInFlight;

            var names = BandwidthConverters.Select(m => m.bandwidthName).ToArray();

            if (_selectedBandwidthConfigurationField.uiControlEditor is UI_ChooseOption chooseOptionEditor)
            {
                chooseOptionEditor.options = names;
                chooseOptionEditor.onFieldChanged = UpdateFromGui;
            }

            if (_selectedBandwidthConfigurationField.uiControlFlight is UI_ChooseOption chooseOptionFlight)
            {
                chooseOptionFlight.options = names;
                chooseOptionFlight.onFieldChanged = UpdateFromGui;
            }

            UpdateFromGui(_selectedBandwidthConfigurationField, selectedBandwidthConfiguration);
        }

        private void LoadInitialConfiguration()
        {
            IsLoaded = true;

            var currentWavelength = targetWavelength != 0 ? targetWavelength : 1;

            Debug.Log("[KSPI]: LoadInitialConfiguration initialize initial beam configuration with wavelength target " + currentWavelength);

            // find wavelength closes to target wavelength
            ActiveBandwidthConfiguration = BandwidthConverters.First();
            bandWidthName = ActiveBandwidthConfiguration.bandwidthName;
            selectedBandwidthConfiguration = 0;
            var lowestWavelengthDifference = Math.Abs(currentWavelength - ActiveBandwidthConfiguration.TargetWavelength);

            if (!BandwidthConverters.Any()) return;

            foreach (var currentConfig in BandwidthConverters)
            {
                var configWaveLengthDifference = Math.Abs(currentWavelength - currentConfig.TargetWavelength);

                if (!(configWaveLengthDifference < lowestWavelengthDifference)) continue;

                ActiveBandwidthConfiguration = currentConfig;
                lowestWavelengthDifference = configWaveLengthDifference;
                selectedBandwidthConfiguration = BandwidthConverters.IndexOf(currentConfig);
                bandWidthName = ActiveBandwidthConfiguration.bandwidthName;
            }
        }

        private void UpdateFromGui(BaseField field, object oldFieldValueObj)
        {
            if (BandwidthConverters == null || !BandwidthConverters.Any())
                return;

            if (IsLoaded == false)
                LoadInitialConfiguration();
            else
            {
                if (selectedBandwidthConfiguration < BandwidthConverters.Count)
                    ActiveBandwidthConfiguration = BandwidthConverters[selectedBandwidthConfiguration];
                else
                {
                    selectedBandwidthConfiguration = BandwidthConverters.Count - 1;
                    ActiveBandwidthConfiguration = BandwidthConverters.Last();
                }
            }

            if (ActiveBandwidthConfiguration == null)
                Debug.LogWarning("[KSPI]: BeamedPowerReceiver UpdateFromGUI failed to find BandwidthConfiguration");
            else
                UpdateProperties();
        }

        private void UpdateProperties()
        {
            targetWavelength = ActiveBandwidthConfiguration.TargetWavelength;
            bandWidthName = ActiveBandwidthConfiguration.bandwidthName;

            // update wavelength we can receive
            if (canSwitchBandwidthInEditor)
            {
                minimumWavelength = ActiveBandwidthConfiguration.minimumWavelength;
                maximumWavelength = ActiveBandwidthConfiguration.maximumWavelength;
            }
            else
            {
                minimumWavelength = BandwidthConverters.Min(m => m.minimumWavelength);
                maximumWavelength = BandwidthConverters.Max(m => m.maximumWavelength);
            }
        }

        public bool CanBeActiveInAtmosphere
        {
            get
            {
                if (!HighLogic.LoadedSceneIsFlight)
                    return true;

                if (DeployableAntenna != null && DeployableAntenna.isBreakable)
                    return !DeployableAntenna.ShouldBreakFromPressure();

                if (DeployableRadiator != null && DeployableRadiator.isBreakable)
                    return !DeployableRadiator.ShouldBreakFromPressure();

                if (DeployableSolarPanel != null && DeployableSolarPanel.isBreakable)
                    return !DeployableSolarPanel.ShouldBreakFromPressure();

                if (genericAnimation == null)
                    return true;

                var pressure = FlightGlobals.getStaticPressure(vessel.GetVesselPos()) / 100;
                var dynamicPressure = 0.5 * pressure * 1.2041 * vessel.srf_velocity.sqrMagnitude / 101325;

                if (dynamicPressure <= 0) return true;

                var pressureLoad = dynamicPressure / 1.4854428818159e-3 * 100;

                return !(pressureLoad > 100 * atmosphereToleranceModifier);
            }
        }

        protected CelestialBody GetCurrentStar()
        {
            var depth = 0;
            var star = FlightGlobals.currentMainBody;
            while ((depth < 10) && (star.GetTemperature(0) < 2000))
            {
                star = star.referenceBody;
                depth++;
            }

            if ((star.GetTemperature(0) < 2000) || (star.name == "Galactic Core"))
                star = null;

            return star;
        }

        public void Update()
        {
            var transmitterOn = has_transmitter && (part_transmitter.IsEnabled || part_transmitter.relay);
            var canBeActive = CanBeActiveInAtmosphere;

            _linkReceiverBaseEvent.active = canLinkup && !linkedForRelay && !receiverIsEnabled && !transmitterOn && canBeActive;
            _unlinkReceiverBaseEvent.active = linkedForRelay;

            _activateReceiverBaseEvent.active = !linkedForRelay && !receiverIsEnabled && !transmitterOn && canBeActive;
            _disableReceiverBaseEvent.active = receiverIsEnabled;

            var isNotRelayingOrTransmitting = !linkedForRelay && !transmitterOn;

            _beamedpowerField.guiActive = isNotRelayingOrTransmitting;
            _linkedForRelayField.guiActive = canLinkup && isNotRelayingOrTransmitting;

            _slavesAmountField.guiActive = thermalMode && slavesAmount > 0;
            _ThermalPowerField.guiActive = isThermalReceiverSlave || thermalMode;

            _maximumConsumptionPercentageField.guiActive = receiverIsEnabled;
            _minimumConsumptionPercentageField.guiActive = receiverIsEnabled;
            _minimumWavelengthField.guiActive = receiverIsEnabled;
            _maximumWavelengthField.guiActive = receiverIsEnabled;

            HideSlavePartModules();
        }

        public override void OnUpdate()
        {
            _connectedsatsField.guiActive = connectedsatsi > 0;
            _connectedrelaysField.guiActive = connectedrelaysi > 0;
            _networkDepthStringField.guiActive = networkDepth > 0;

            _solarFacingFactorField.guiActive = solarReceptionSurfaceArea > 0;
            _solarFluxField.guiActive = solarReceptionSurfaceArea > 0;

            _selectedBandwidthConfigurationField.guiActive = (HighLogic.CurrentGame.Parameters.CustomParams<KITGamePlayParams>().ReconfigureAntennas || canSwitchBandwidthInFlight) && receiverIsEnabled;

            if (IsThermalSource)
                coreTempererature = CoreTemperature.ToString("0.0") + " K";

            beamedpower = receiverIsEnabled ? PluginHelper.GetFormattedPowerString(ProducedPower) : Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_BeamPowerOffline");

            connectedsats = $"{connectedsatsi}/{BeamedPowerSources.Instance.GlobalTransmitters.Count}";
            connectedrelays = $"{connectedrelaysi}/{BeamedPowerSources.Instance.GlobalRelays.Count}";
            networkDepthString = networkDepth.ToString();

            CalculateInputPower();

            if (DeployableAntenna != null)
            {
                DeployableAntenna.Events[nameof(DeployableAntenna.Extend)].guiActive = false;
                DeployableAntenna.Events[nameof(DeployableAntenna.Retract)].guiActive = false;
            }
        }

        private double GetSolarFacingFactor(CelestialBody star, Vector3 vesselPosition)
        {
            if (star == null) return 0;

            Vector3d solarDirectionVector = (star.transform.position - vesselPosition).normalized;

            switch (receiverType)
            {
                case 9:
                    return 1;
                case 3:
                    return Math.Max(0, 1 - Vector3d.Dot(part.transform.forward, solarDirectionVector)) / 2;
                default:
                    return Math.Max(0, Vector3d.Dot(part.transform.up, solarDirectionVector));
            }
        }

        public void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsEditor && !part.enabled)
                base.OnFixedUpdate();
        }

        private void OnGUI()
        {
            if (vessel == FlightGlobals.ActiveVessel && showWindow && !HighLogic.LoadedSceneIsEditor)
                _windowPosition = GUILayout.Window(_windowID, _windowPosition, DrawGui, Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_InterfaceWindowTitle"));//"Power Receiver Interface"
        }

        private void DrawGui(int window)
        {
            windowPositionX = _windowPosition.x;
            windowPositionY = _windowPosition.y;

            InitializeStyles();

            if (GUI.Button(new Rect(_windowPosition.width - 20, 2, 18, 18), "x"))
                showWindow = false;

            GUILayout.BeginVertical();

            PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel1"), part.partInfo.title, BoldBlackStyle, TextBlackStyle, 200, 400);//"Receiver Type"
            PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel2"), diameter.ToString("F2"), BoldBlackStyle, TextBlackStyle, 200, 400);//"Receiver Diameter"
            PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel3"), part.vessel.mainBody.name + " @ " + DistanceToText(part.vessel.altitude), BoldBlackStyle, TextBlackStyle, 200, 400);//"Receiver Location"
            PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel4"), powerCapacityEfficiency.ToString("P1"), BoldBlackStyle, TextBlackStyle, 200, 400);//"Power Capacity Efficiency"
            PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel5"), PluginHelper.GetFormattedPowerString(total_beamed_power), BoldBlackStyle, TextBlackStyle, 200, 400);//"Total Current Beamed Power"
            PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel6"), PluginHelper.GetFormattedPowerString(total_beamed_power_max), BoldBlackStyle, TextBlackStyle, 200, 400);//"Total Maximum Beamed Power"
            PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel7"), PluginHelper.GetFormattedPowerString(total_beamed_wasteheat), BoldBlackStyle, TextBlackStyle, 200, 400);//"Total Wasteheat Production"

            GUILayout.BeginHorizontal();
            GUILayout.Label(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel8"), BoldBlackStyle, GUILayout.Width(LabelWidth));//"Transmitter"
            GUILayout.Label(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel9"), BoldBlackStyle, GUILayout.Width(LabelWidth));//"Location"
            GUILayout.Label(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel10"), BoldBlackStyle, GUILayout.Width(ValueWidthNormal));//"Aperture"
            GUILayout.Label(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel11"), BoldBlackStyle, GUILayout.Width(ValueWidthNormal));//"Facing"
            GUILayout.Label(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel12"), BoldBlackStyle, GUILayout.Width(ValueWidthWide));//"Transmit Power"
            GUILayout.Label(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel13"), BoldBlackStyle, GUILayout.Width(ValueWidthNormal));//"Distance"
            GUILayout.Label(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel14"), BoldBlackStyle, GUILayout.Width(ValueWidthNormal));//"SpotSize"
            GUILayout.Label(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel15"), BoldBlackStyle, GUILayout.Width(ValueWidthNormal));//"Wavelength"
            GUILayout.Label(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel16"), BoldBlackStyle, GUILayout.Width(ValueWidthNormal));//"Network Power"
            GUILayout.Label(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel17"), BoldBlackStyle, GUILayout.Width(ValueWidthNormal));//"Available Power"
            GUILayout.Label(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel18"), BoldBlackStyle, GUILayout.Width(ValueWidthNormal));//"Consumed Power"
            GUILayout.Label(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel19"), BoldBlackStyle, GUILayout.Width(ValueWidthNormal));//"Network Efficiency"
            GUILayout.Label(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel20"), BoldBlackStyle, GUILayout.Width(ValueWidthNormal));//"Receiver Efficiency"
            GUILayout.EndHorizontal();

            foreach (ReceivedPowerData receivedPowerData in ReceivedPower.Values)
            {
                if (receivedPowerData.Wavelengths == string.Empty)
                    continue;

                GUILayout.BeginHorizontal();
                GUILayout.Label(receivedPowerData.Transmitter.Vessel.name, TextBlackStyle, GUILayout.Width(LabelWidth));
                GUILayout.Label(receivedPowerData.Transmitter.Vessel.mainBody.name + " @ " + DistanceToText(receivedPowerData.Transmitter.Vessel.altitude), TextBlackStyle, GUILayout.Width(LabelWidth));
                GUILayout.Label((receivedPowerData.Transmitter.Aperture).ToString("##.######") + " m", TextBlackStyle, GUILayout.Width(ValueWidthNormal));
                GUILayout.Label(receivedPowerData.Route.FacingFactor.ToString("P3"), TextBlackStyle, GUILayout.Width(ValueWidthNormal));
                GUILayout.Label(PluginHelper.GetFormattedPowerString(receivedPowerData.TransmitPower), TextBlackStyle, GUILayout.Width(ValueWidthWide));
                GUILayout.Label(DistanceToText(receivedPowerData.Route.Distance), TextBlackStyle, GUILayout.Width(ValueWidthNormal));
                GUILayout.Label(SpotSizeToText(receivedPowerData.Route.SpotSize), TextBlackStyle, GUILayout.Width(ValueWidthNormal));
                GUILayout.Label(receivedPowerData.Wavelengths, TextBlackStyle, GUILayout.Width(ValueWidthNormal));
                GUILayout.Label(PluginHelper.GetFormattedPowerString(receivedPowerData.NetworkPower), TextBlackStyle, GUILayout.Width(ValueWidthNormal));
                GUILayout.Label(PluginHelper.GetFormattedPowerString(receivedPowerData.AvailablePower), TextBlackStyle, GUILayout.Width(ValueWidthNormal));
                GUILayout.Label(PluginHelper.GetFormattedPowerString(receivedPowerData.ConsumedPower), TextBlackStyle, GUILayout.Width(ValueWidthNormal));
                GUILayout.Label(receivedPowerData.Route.Efficiency.ToString("P2"), TextBlackStyle, GUILayout.Width(ValueWidthNormal));
                GUILayout.Label((receivedPowerData.ReceiverEfficiency * 0.01).ToString("P1"), TextBlackStyle, GUILayout.Width(ValueWidthNormal));
                GUILayout.EndHorizontal();
            }

            if (ReceivedPower.Values.Any(m => m.Relays.Count > 0))
            {
                PrintToGuiLayout(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel21"), "", BoldBlackStyle, TextBlackStyle, 200);//"Relays"

                GUILayout.BeginHorizontal();
                GUILayout.Label(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel22"), BoldBlackStyle, GUILayout.Width(WideLabelWidth));//"Transmitter"
                GUILayout.Label(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel23"), BoldBlackStyle, GUILayout.Width(ValueWidthNormal));//"Relay Nr"
                GUILayout.Label(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel24"), BoldBlackStyle, GUILayout.Width(WideLabelWidth));//"Relay Name"
                GUILayout.Label(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel25"), BoldBlackStyle, GUILayout.Width(LabelWidth));//"Relay Location"
                GUILayout.Label(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel26"), BoldBlackStyle, GUILayout.Width(ValueWidthWide));//"Max Capacity"
                GUILayout.Label(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_WinLabel27"), BoldBlackStyle, GUILayout.Width(ValueWidthNormal));//"Aperture"
                GUILayout.Label(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_Diameter"), BoldBlackStyle, GUILayout.Width(ValueWidthNormal));//"Diameter"
                GUILayout.Label(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_MinimumWavelength"), BoldBlackStyle, GUILayout.Width(ValueWidthNormal));//"Min Wavelength"
                GUILayout.Label(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_MaximumWavelength"), BoldBlackStyle, GUILayout.Width(ValueWidthNormal));//"Max Wavelength"
                GUILayout.EndHorizontal();

                foreach (ReceivedPowerData receivedPowerData in ReceivedPower.Values)
                {
                    for (int r = 0; r < receivedPowerData.Relays.Count; r++)
                    {
                        VesselRelayPersistence vesselPersistence = receivedPowerData.Relays[r];

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(r == 0 ? receivedPowerData.Transmitter.Vessel.name : "", TextBlackStyle, GUILayout.Width(WideLabelWidth));
                        GUILayout.Label(r.ToString(), TextBlackStyle, GUILayout.Width(ValueWidthNormal));
                        GUILayout.Label(vesselPersistence.Vessel.name, TextBlackStyle, GUILayout.Width(WideLabelWidth));
                        GUILayout.Label(vesselPersistence.Vessel.mainBody.name + " @ " + DistanceToText(vesselPersistence.Vessel.altitude), TextBlackStyle, GUILayout.Width(LabelWidth));
                        GUILayout.Label(PluginHelper.GetFormattedPowerString(vesselPersistence.PowerCapacity), TextBlackStyle, GUILayout.Width(ValueWidthWide));
                        GUILayout.Label(vesselPersistence.Aperture + " m", TextBlackStyle, GUILayout.Width(ValueWidthNormal));
                        GUILayout.Label(vesselPersistence.Diameter + " m", TextBlackStyle, GUILayout.Width(ValueWidthNormal));
                        GUILayout.Label(WavelengthToText(vesselPersistence.MinimumRelayWavelength), TextBlackStyle, GUILayout.Width(ValueWidthNormal));
                        GUILayout.Label(WavelengthToText(vesselPersistence.MaximumRelayWavelength), TextBlackStyle, GUILayout.Width(ValueWidthNormal));
                        GUILayout.EndHorizontal();
                    }
                }
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void InitializeStyles()
        {
            if (BoldBlackStyle == null)
            {
                BoldBlackStyle = new GUIStyle(GUI.skin.label)
                {
                    fontStyle = FontStyle.Bold,
                    font = PluginHelper.MainFont
                };
            }

            if (TextBlackStyle == null)
            {
                TextBlackStyle = new GUIStyle(GUI.skin.label)
                {
                    fontStyle = FontStyle.Normal,
                    font = PluginHelper.MainFont
                };
            }
        }

        protected void PrintToGuiLayout(string label, string value, GUIStyle boldStyle, GUIStyle textStyle, int widthLabel = 130, int widthValue = 130)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, boldStyle, GUILayout.Width(widthLabel));
            GUILayout.Label(value, textStyle, GUILayout.Width(widthValue));
            GUILayout.EndHorizontal();
        }

        /*
        private void StoreGeneratorRequests()
        {
            storedIsThermalEnergyGeneratorEfficiency = currentIsThermalEnergyGeneratorEfficiency;
            currentIsThermalEnergyGeneratorEfficiency = 0;

            storedGeneratorThermalEnergyRequestRatio = Math.Min(1, currentGeneratorThermalEnergyRequestRatio);
            currentGeneratorThermalEnergyRequestRatio = 0;
        }
        */

        // Is called during OnUpdate to reduce processor load
        private void CalculateInputPower()
        {
            TotalConversionWasteHeatProduction = 0;
            if (wasteheatRatio >= 0.95 && !isThermalReceiver) return;

            // reset all output variables at start of loop
            total_beamed_power = 0;
            total_beamed_power_max = 0;
            total_sat_efficiency_fraction = 0;
            total_beamed_wasteheat = 0;
            connectedsatsi = 0;
            connectedrelaysi = 0;
            networkDepth = 0;
            activeSatsIncr = 0;

            if (!solarPowerMode)
            {
                deactivate_timer = 0;

                var usedRelays = new HashSet<VesselRelayPersistence>();

                foreach (var beamedPowerData in ReceivedPower.Values)
                {
                    beamedPowerData.IsAlive = false;
                }

                //loop all connected beamed power transmitters
                foreach (var connectedTransmitterEntry in InterstellarBeamedPowerHelper.GetConnectedTransmitters(this))
                {
                    var transmitter = connectedTransmitterEntry.Key;

                    if (!ReceivedPower.TryGetValue(transmitter.Vessel, out ReceivedPowerData beamedPowerData))
                    {
                        Debug.Log("[KSPI]: Added ReceivedPowerData for " + transmitter.Vessel.name);
                        beamedPowerData = new ReceivedPowerData
                        {
                            Receiver = this,
                            Transmitter = transmitter
                        };
                        ReceivedPower[beamedPowerData.Transmitter.Vessel] = beamedPowerData;
                    }

                    // first reset owm received power to get correct amount received by others
                    beamedPowerData.IsAlive = true;
                    beamedPowerData.AvailablePower = 0;
                    beamedPowerData.NetworkPower = 0;
                    beamedPowerData.Wavelengths = string.Empty;
                    beamedPowerData.CurrentReceivedPower = 0;
                    beamedPowerData.MaximumReceivedPower = 0;
                    beamedPowerData.ReceiverEfficiency = 0;

                    KeyValuePair<MicrowaveRoute, IList<VesselRelayPersistence>> keyValuePair = connectedTransmitterEntry.Value;
                    beamedPowerData.Route = keyValuePair.Key;
                    beamedPowerData.Relays = keyValuePair.Value;

                    // convert initial beamed power from source into MegaWatt
                    beamedPowerData.TransmitPower = transmitter.GetAvailablePowerInMW();

                    beamedPowerData.NetworkCapacity = beamedPowerData.Relays != null && beamedPowerData.Relays.Count > 0
                        ? Math.Min(beamedPowerData.TransmitPower, beamedPowerData.Relays.Min(m => m.PowerCapacity))
                        : beamedPowerData.TransmitPower;

                    // calculate maximum power available from beamed power network
                    beamedPowerData.PowerUsageOthers = GetEnumeratedPowerFromSatelliteForAllLoadedVessels(beamedPowerData.Transmitter, this);

                    // add to available network power
                    beamedPowerData.NetworkPower = beamedPowerData.NetworkCapacity;

                    // initialize remaining power
                    beamedPowerData.RemainingPower = Math.Max(0, beamedPowerData.NetworkCapacity - beamedPowerData.PowerUsageOthers);

                    foreach (var powerBeam in beamedPowerData.Transmitter.SupportedTransmitWavelengths)
                    {
                        // select active or compatible brandWith Converter
                        var selectedBrandWith = canSwitchBandwidthInEditor
                            ? ActiveBandwidthConfiguration
                            : BandwidthConverters.FirstOrDefault(m => (powerBeam.Wavelength >= m.minimumWavelength && powerBeam.Wavelength <= m.maximumWavelength));

                        // skip if no compatible receiver bandwidth found
                        if (selectedBrandWith == null)
                            continue;

                        var maximumRoutePower = (powerBeam.NuclearPower + powerBeam.SolarPower) * beamedPowerData.Route.Efficiency * 0.001;

                        // subtract any power already received by other receivers
                        var remainingPowerInBeam = Math.Min(beamedPowerData.RemainingPower, maximumRoutePower);

                        // skip if no power remaining
                        if (remainingPowerInBeam <= 0)
                            continue;

                        // construct displayed wavelength
                        if (beamedPowerData.Wavelengths.Length > 0)
                            beamedPowerData.Wavelengths += ",";
                        beamedPowerData.Wavelengths += WavelengthToText(powerBeam.Wavelength);

                        // take into account maximum route capacity
                        var beamNetworkPower = beamedPowerData.Relays != null && beamedPowerData.Relays.Count > 0
                            ? Math.Min(remainingPowerInBeam, beamedPowerData.Relays.Min(m => m.PowerCapacity))
                            : remainingPowerInBeam;

                        // subtract from remaining power
                        beamedPowerData.RemainingPower = Math.Max(0, beamedPowerData.RemainingPower - beamNetworkPower);

                        // determine allowed power
                        var maximumReceivePower = MaximumRecievePower;

                        var currentReceivablePower = Math.Min(maximumReceivePower * PowerRatio, maximumReceivePower * powerCapacityEfficiency);
                        var maximumReceivablePower = maximumReceivePower * powerCapacityEfficiency;

                        // get effective beam to Power efficiency
                        var efficiencyPercentage = thermalMode
                            ? selectedBrandWith.MaxThermalEfficiencyPercentage
                            : selectedBrandWith.MaxElectricEfficiencyPercentage;

                        // convert to fraction
                        var efficiencyFraction = efficiencyPercentage / 100;

                        // limit by amount of beam power the receiver is able to process
                        var satPower = Math.Min(currentReceivablePower, beamNetworkPower * efficiencyFraction);
                        var satPowerMax = Math.Min(maximumReceivablePower, beamNetworkPower * efficiencyFraction);
                        var satWasteheat = Math.Min(currentReceivablePower, beamNetworkPower * (1 - efficiencyFraction));

                        // calculate wasteheat by power conversion
                        var conversionWasteheat = (thermalMode ? 0.05 : 1) * satPower * (1 - efficiencyFraction);

                        // generate conversion wasteheat
                        TotalConversionWasteHeatProduction += conversionWasteheat; // + missedPowerPowerWasteheat;

                        // register amount of raw power recieved
                        beamedPowerData.CurrentReceivedPower = satPower;
                        beamedPowerData.MaximumReceivedPower = satPowerMax;
                        beamedPowerData.ReceiverEfficiency = efficiencyPercentage;
                        beamedPowerData.AvailablePower = satPower > 0 && efficiencyFraction > 0 ? satPower / efficiencyFraction : 0;

                        // convert raw power into effective power
                        total_beamed_power += satPower;
                        total_beamed_power_max += satPowerMax;
                        total_beamed_wasteheat += satWasteheat;
                        total_sat_efficiency_fraction += satPower * efficiencyFraction;

                        if (!(satPower > 0)) continue;

                        activeSatsIncr++;

                        if (beamedPowerData.Relays == null) continue;

                        foreach (var relay in beamedPowerData.Relays)
                        {
                            usedRelays.Add(relay);
                        }
                        networkDepth = Math.Max(networkDepth, beamedPowerData.Relays.Count);
                    }
                }

                connectedsatsi = activeSatsIncr;
                connectedrelaysi = usedRelays.Count;
            }

            //remove dead entries
            var deadEntries = ReceivedPower.Where(m => !m.Value.IsAlive).ToList();
            foreach (var entry in deadEntries)
            {
                Debug.LogWarning("[KSPI]: Removed received power from " + entry.Key.name);
                ReceivedPower.Remove(entry.Key);
            }
        }

        private void UpdatePowerInput()
        {
            BeamedPowerQueue.Enqueue(total_beamed_power);
            if (total_beamed_power > 0)
            {
                BeamedPowerQueue.Enqueue(total_beamed_power);
                BeamedPowerQueue.Dequeue();
            }
            if (BeamedPowerQueue.Count > 20)
                BeamedPowerQueue.Dequeue();

            BeamedPowerMaxQueue.Enqueue(total_beamed_power_max);
            if (total_beamed_power_max > 0)
            {
                BeamedPowerMaxQueue.Enqueue(total_beamed_power_max);
                BeamedPowerMaxQueue.Dequeue();
            }
            if (BeamedPowerMaxQueue.Count > 20)
                BeamedPowerMaxQueue.Dequeue();

            total_beamed_power = BeamedPowerQueue.Average();
            total_beamed_power_max = BeamedPowerMaxQueue.Average();

            powerInputMegajoules = total_beamed_power + thermalSolarInputMegajoules;
            powerInputMegajoulesMax = total_beamed_power_max + thermalSolarInputMegajoulesMax;
        }

        private void CalculateThermalSolarPower()
        {
            if (solarReceptionSurfaceArea <= 0 || solarReceptionEfficiency <= 0)
                return;

            SolarFluxQueue.Enqueue(part.vessel.solarFlux);

            if (SolarFluxQueue.Count > 50)
                SolarFluxQueue.Dequeue();

            solarFlux = SolarFluxQueue.Count > 10
                ? SolarFluxQueue.OrderBy(m => m).Skip(10).Take(30).Average()
                : SolarFluxQueue.Average();

            thermalSolarInputMegajoulesMax = solarReceptionSurfaceArea * (solarFlux / 1e+6) * solarReceptionEfficiency;
            solarFacingFactor = Math.Pow(GetSolarFacingFactor(localStar, part.WCoM), solarFacingExponent);
            thermalSolarInputMegajoules = thermalSolarInputMegajoulesMax * solarFacingFactor;
        }

        private double _alternatorPowerMax;
        private double _alternatorPowerConsumed;

        private double AddAlternatorPower(IResourceManager resMan, double requestedAmount)
        {
            if (alternatorRatio == 0)
                return 0;

            if (!receiverIsEnabled || radiatorMode || (_alternatorPowerMax == _alternatorPowerConsumed))
                return 0;

            var alternatorPower = Math.Min(_alternatorPowerMax - _alternatorPowerConsumed, requestedAmount / AverageEfficiencyFraction);
            if (alternatorPower == 0) return 0;

            // TODO
            //var alternatorWasteheat = Math.Min(alternatorPower * AverageEfficiencyFraction, GetCurrentUnfilledResourceDemand/(ResourceSettings.Config.ElectricPowerInMegawatt) * AverageEfficiencyFraction);

            _alternatorPowerConsumed += alternatorPower;

            var alternatorWasteheat = alternatorPower * AverageEfficiencyFraction;

            resMan.Produce(ResourceName.ElectricCharge, alternatorPower * AverageEfficiencyFraction);
            resMan.Produce(ResourceName.WasteHeat, alternatorWasteheat);

            return alternatorPower * AverageEfficiencyFraction;
        }

        public double MaxStableMegaWattPower => isThermalReceiver ? 0 : powerInputMegajoules;

        public virtual double GetCoreTempAtRadiatorTemp(double radTemp)
        {
            return CoreTemperature;
        }

        public double GetThermalPowerAtTemp(double temp)
        {
            return ThermalPower;
        }

        public double Radius => radius;

        public bool isActive()
        {
            return receiverIsEnabled;
        }

        public bool ShouldScaleDownJetISP()
        {
            return false;
        }

        public void EnableIfPossible()
        {
            if (!receiverIsEnabled && autoDeploy)
                ActivateReceiverState();
        }

        public override string GetInfo()
        {
            return Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_Getinfo", diameter);//"Diameter: " +  + " m"
        }

        public double GetCurrentReceivedPower(VesselMicrowavePersistence vmp)
        {
            if (receiverIsEnabled && ReceivedPower.TryGetValue(vmp.Vessel, out var data))
            {
                return data.CurrentReceivedPower;
            }

            return 0;
        }

        public static double GetEnumeratedPowerFromSatelliteForAllLoadedVessels(VesselMicrowavePersistence vmp, BeamedPowerReceiver currentReceiver)
        {
            double enumeratedPower = 0;
            foreach (Vessel vessel in FlightGlobals.Vessels)
            {
                if (vessel.isEVA) continue;

                var receivers = vessel.FindPartModulesImplementing<BeamedPowerReceiver>().Where(receiver => receiver != currentReceiver);
                foreach (var receiver in receivers)
                {
                    enumeratedPower += receiver.GetCurrentReceivedPower(vmp);
                }
            }
            return enumeratedPower;
        }

        private string WavelengthToText(double wavelength)
        {
            if (wavelength > 1.0e-3)
                return (wavelength * 1.0e+3) + " mm";
            else if (wavelength > 7.5e-7)
                return (wavelength * 1.0e+6) + " µm";
            else if (wavelength > 1.0e-9)
                return (wavelength * 1.0e+9) + " nm";
            else
                return (wavelength * 1.0e+12) + " pm";
        }

        private string DistanceToText(double distance)
        {
            if (distance >= 1.0e+16)
                return (distance / 1.0e+15).ToString("0.00") + " Pm";
            else if (distance >= 1.0e+13)
                return (distance / 1.0e+12).ToString("0.00") + " Tm";
            else if (distance >= 1.0e+10)
                return (distance / 1.0e+9).ToString("0.00") + " Gm";
            else if (distance >= 1.0e+7)
                return (distance / 1.0e+6).ToString("0.00") + " Mm";
            else if (distance >= 1.0e+4)
                return (distance / 1.0e+3).ToString("0.00") + " km";
            else
                return distance.ToString("0") + " m";
        }

        private string SpotSizeToText(double spotSize)
        {
            if (spotSize > 1.0e+3)
                return (spotSize * 1.0e-3).ToString("0.000") + " km";
            else if (spotSize > 1)
                return spotSize.ToString("0.00") + " m";
            else
                return (spotSize * 1.0e+3).ToString("0") + " mm";
        }
        
        public ModuleConfigurationFlags ModuleConfiguration() =>
            ModuleConfigurationFlags.Fifth | ModuleConfigurationFlags.SupplierOnly;

        public void KITFixedUpdate(IResourceManager resMan)
        {
            powerCapacityEfficiency = 1 - resMan.FillFraction(ResourceName.WasteHeat);

            wasteheatRatio = CheatOptions.IgnoreMaxTemperature ? 0 : resMan.FillFraction(ResourceName.WasteHeat);

            CalculateThermalSolarPower();

            if (isMirror && receiverIsEnabled)
            {
                var thermalMassPerKilogram = part.mass * part.thermalMassModifier * PhysicsGlobals.StandardSpecificHeatCapacity * 1e-3;
                dissipationInMegaJoules = PluginHelper.GetBlackBodyDissipation(solarReceptionSurfaceArea, part.temperature) * 1e-6;
                var temperatureChange = resMan.FixedDeltaTime() * -(dissipationInMegaJoules / thermalMassPerKilogram);
                part.temperature = part.temperature + temperatureChange;
            }

            if (InitializationCountdown > 0)
            {
                InitializationCountdown--;

                part.temperature = storedTemp;
                part.skinTemperature = storedTemp;
            }
            else
                storedTemp = part.temperature;

            if (receiverIsEnabled && radiatorMode)
            {
                if (FNRadiator != null)
                {
                    FNRadiator.canRadiateHeat = true;
                    FNRadiator.radiatorIsEnabled = true;
                }
                PowerDown();
                return;
            }

            if (FNRadiator != null)
            {
                FNRadiator.canRadiateHeat = false;
                FNRadiator.radiatorIsEnabled = false;
            }

            try
            {
                if (_restartCounter > 0)
                {
                    _restartCounter--;
                    RemoveOtherVesselData();
                    OnUpdate();
                }

                UpdatePowerInput();

                if (!receiverIsEnabled || radiatorMode)
                {
                    total_thermal_power_provided = 0;
                    total_sat_efficiency_fraction = 0;
                    total_beamed_power = 0;
                    total_beamed_power_max = 0;
                    total_beamed_wasteheat = 0;

                    powerInputMegajoules = 0;
                    powerInputMegajoulesMax = 0;

                    thermalSolarInputMegajoules = 0;
                    thermalSolarInputMegajoulesMax = 0;

                    solarFacingFactor = 0;
                    ThermalPower = 0;

                    PowerDown();

                    if (animT == null) return;

                    animT[animTName].normalizedTime = 0;
                    animT.Sample();

                    return;
                }

                if (wasteheatRatio >= 0.95 && !isThermalReceiver && !solarPowerMode)
                {
                    receiverIsEnabled = false;
                    deactivate_timer++;
                    if (FlightGlobals.ActiveVessel == vessel && deactivate_timer > 2)
                        ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_KSPIE_BeamPowerReceiver_OverheatingMsg"), 5f, ScreenMessageStyle.UPPER_CENTER);//"Warning Dangerous Overheating Detected: Emergency beam power shutdown occuring NOW!"
                    PowerDown();
                    return;
                }

                _alternatorPowerMax = alternatorRatio * powerInputMegajoules * 0.001;
                _alternatorPowerConsumed = 0;

                // add max TP output from there..
                UpdatePowerState();

                // TODO minimum and maximum consumption rates / production rates
                // var minimumRequestedPower = MaximumRecievePower * (minimumConsumptionPercentage / 100d);
                // var calculatedMinimumRatio = Math.Min(1, minimumRequestedPower / totalBeamedElectricPowerProvided);

                if (isThermalReceiverSlave || thermalMode)
                {
                    slavesAmount = ThermalReceiverSlaves.Count;
                    slavesPower = ThermalReceiverSlaves.Sum(m => m.total_thermal_power_provided);

                    total_thermal_power_available = thermalSolarInputMegajoules + total_beamed_power + slavesPower;
                    total_thermal_power_provided = Math.Min(MaximumRecievePower, total_thermal_power_available);
                    total_thermal_power_provided_max = Math.Min(MaximumRecievePower, total_beamed_power_max + thermalSolarInputMegajoulesMax);
                    _totalThermalPowerConsumed = 0;

                    // TODO: revisit if we need to be provide a minimum this way, as we now have the demand based approach for engines.
                    //var thermalEngineThrottleRatio = connectedEngines.Any(m => !m.RequiresChargedPower) ? connectedEngines.Where(m => !m.RequiresChargedPower).Max(e => e.CurrentThrottle) : 0;
                    //var minimumRatio = Math.Max(minimumConsumptionPercentage / 100d, Math.Max(storedGeneratorThermalEnergyRequestRatio, thermalEngineThrottleRatio));

                    resMan.Produce(ResourceName.ThermalPower, 0, total_thermal_power_provided_max);
                    //provideThermalPower(resMan, total_thermal_power_provided_max * minimumRatio);

                    // TODO - max resource settings
                    //var powerGeneratedResult = managedPowerSupplyPerSecondMinimumRatio(total_thermal_power_provided, total_thermal_power_provided_max, minimumRatio, ResourceSettings.Config.ThermalPowerInMegawatt);

                    // resMan.Produce(ResourceName.ThermalPower, total_thermal_power_provided, total_thermal_power_provided_max);

                }
                else
                {
                    wasteheatElectricConversionEfficiency = WasteheatElectricConversionEfficiency;
                    effectiveSolarThermalElectricEfficiency = wasteheatElectricConversionEfficiency * solarElectricEfficiency;
                    effectiveBeamedPowerElectricEfficiency = wasteheatElectricConversionEfficiency * electricMaxEfficiency;

                    var totalBeamedElectricPowerAvailable = thermalSolarInputMegajoules * effectiveSolarThermalElectricEfficiency + total_beamed_power * effectiveBeamedPowerElectricEfficiency;
                    _totalBeamedElectricPowerProvided = Math.Min(MaximumRecievePower, totalBeamedElectricPowerAvailable);
                    _totalBeamedElectricPowerConsumed = 0;
                }

                if (animT != null)
                {
                    var maximumReceivePower = MaximumRecievePower;
                    animT[animTName].normalizedTime = maximumReceivePower > 0 ? (float)Math.Min(total_thermal_power_provided / maximumReceivePower, 1) : 0;
                    animT.Sample();
                }

                ThermalPower = ThermalPower <= 0
                    ? total_thermal_power_provided
                    : total_thermal_power_provided * GameConstants.MicrowaveAlpha + GameConstants.MicrowaveBeta * ThermalPower;

                foreach (var item in ReceivedPower)
                {
                    item.Value.ConsumedPower = 0;
                }

                foreach (var slave in ThermalReceiverSlaves)
                {
                    foreach (var item in slave.ReceivedPower)
                    {
                        item.Value.ConsumedPower = 0;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[KSPI]: Exception in MicrowavePowerReceiver.OnFixedUpdateResourceSuppliable " + e.Message + " at " + e.StackTrace);
            }

        }

        private double _totalBeamedElectricPowerProvided, _totalBeamedElectricPowerConsumed;

        public string KITPartName() => part.partInfo.title;

        private readonly ResourceName[] _electricChargeProvided = { ResourceName.ElectricCharge };
        private readonly ResourceName[] _thermalPowerProvided = { ResourceName.ThermalPower, ResourceName.ElectricCharge /* TODO: alternator works here, yeah? */ };

        public ResourceName[] ResourcesProvided()
        {
            return thermalMode ? _thermalPowerProvided : _electricChargeProvided;
        }

        private double _totalThermalPowerConsumed;

        private void ProvideThermalPower(IResourceManager resMan, double requestedAmount)
        {
            // TODO - max resource settings
            //var powerGeneratedResult = managedPowerSupplyPerSecondMinimumRatio(total_thermal_power_provided, total_thermal_power_provided_max, minimumRatio, ResourceSettings.Config.ThermalPowerInMegawatt);

            var powerGeneratedResult = Math.Min(total_thermal_power_provided - _totalThermalPowerConsumed, requestedAmount);
            _totalThermalPowerConsumed += powerGeneratedResult;
            var supplyRatio = powerGeneratedResult / total_thermal_power_provided;

            resMan.Produce(ResourceName.ThermalPower, powerGeneratedResult, total_thermal_power_provided_max);
            resMan.Produce(ResourceName.WasteHeat, /* powerGeneratedResult + */ supplyRatio * TotalConversionWasteHeatProduction);

            foreach (var item in ReceivedPower)
            {
                item.Value.ConsumedPower += item.Value.AvailablePower * supplyRatio;
            }

            foreach (var slave in ThermalReceiverSlaves)
            {
                foreach (var item in slave.ReceivedPower)
                {
                    item.Value.ConsumedPower += item.Value.AvailablePower * supplyRatio;
                }
            }

        }

        private double ProvideElectricalCharge(IResourceManager resMan, double requestedAmount)
        {
            if (_totalBeamedElectricPowerProvided - _totalBeamedElectricPowerConsumed <= 0) return 0;

            var powerGeneratedResult = Math.Min(_totalBeamedElectricPowerProvided - _totalBeamedElectricPowerConsumed, requestedAmount);
            var supply_ratio = powerGeneratedResult / _totalBeamedElectricPowerConsumed;
            resMan.Produce(ResourceName.ElectricCharge, powerGeneratedResult);

            var solarWasteheat = thermalSolarInputMegajoules * (1 - effectiveSolarThermalElectricEfficiency);
            resMan.Produce(ResourceName.WasteHeat, supply_ratio * TotalConversionWasteHeatProduction + supply_ratio * solarWasteheat);

            foreach (var item in ReceivedPower)
            {
                item.Value.ConsumedPower += item.Value.AvailablePower * supply_ratio;
            }

            return powerGeneratedResult;
        }

        public bool ProvideResource(IResourceManager resMan, ResourceName resource, double requestedAmount)
        {
            if (!receiverIsEnabled || radiatorMode) return false;

            if (resource == ResourceName.ThermalPower)
            {
                if (isThermalReceiverSlave || total_thermal_power_provided == 0) return false;
                ProvideThermalPower(resMan, requestedAmount);
                return true;
            }

            if (resource == ResourceName.ElectricCharge)
            {
                var provided = AddAlternatorPower(resMan, requestedAmount);
                provided += ProvideElectricalCharge(resMan, requestedAmount - provided);

                return true; // do we want to do requestedAmount == provided ?
            }

            return false;
        }
    }
}
