﻿using System;
using System.Collections.Generic;
using System.Linq;
using TweakScale;
using UnityEngine;

namespace KIT.BeamedPower
{
    [KSPModule("Integrated Beam Generator")]//#LOC_KSPIE_BeamGenerator_ModuleName1
    class IntegratedBeamGenerator : BeamGenerator { }

    [KSPModule("Beam Generator")]//#LOC_KSPIE_BeamGenerator_ModuleName2
    class BeamGeneratorModule : BeamGenerator { }

    [KSPModule("Beam Generator")]//#LOC_KSPIE_BeamGenerator_ModuleName2
    class BeamGenerator : PartModule, IPartMassModifier, IRescalable<BeamGenerator>
    {
        public const string Group = "BeamGenerator";
        public const string GroupTitle = "#LOC_KSPIE_BeamGenerator_groupName";

        [KSPField(groupName = Group, groupDisplayName = GroupTitle, isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "#LOC_KSPIE_BeamGenerator_Wavelength")]//Wavelength
        [UI_ChooseOption(affectSymCounterparts = UI_Scene.None, scene = UI_Scene.All, suppressEditorShipModified = true)]
        public int selectedBeamConfiguration;

        [KSPField(isPersistant = true)]
        public bool isInitialized;
        [KSPField(isPersistant = true)]
        public double maximumPower;

        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActiveEditor = true, guiName = "#LOC_KSPIE_BeamGenerator_GeneratorType")]//Generator Type
        public string beamTypeName = "";
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActiveEditor = true, guiActive = true, guiName = "#LOC_KSPIE_BeamGenerator_WavelengthName")]//Wavelength Name
        public string beamWaveName = "";
        [KSPField(groupName = Group, isPersistant = true, guiActiveEditor = false, guiActive = false, guiName = "#LOC_KSPIE_BeamGenerator_Wavelengthinmeter", guiFormat = "F9", guiUnits = " m")]//Wavelength in meter
        public double wavelength;
        [KSPField(groupName = Group, isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "#LOC_KSPIE_BeamGenerator_WaveLengthinSI")]//WaveLength in SI
        public string wavelengthText;
        [KSPField(groupName = Group, isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "#LOC_KSPIE_BeamGenerator_AtmosphericAbsorption", guiFormat = "F3", guiUnits = "%")]//Atmospheric Absorption
        public double atmosphericAbsorptionPercentage = 10;
        [KSPField(groupName = Group, isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "#LOC_KSPIE_BeamGenerator_WaterAbsorption", guiFormat = "F3", guiUnits = "%")]//Water Absorption
        public double waterAbsorptionPercentage = 10;
        [KSPField(groupName = Group, isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "#LOC_KSPIE_BeamGenerator_EfficiencyPercentage", guiFormat = "F0", guiUnits = "%")]//Power to Beam Efficiency
        public double efficiencyPercentage = 90;
        [KSPField(groupName = Group, isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KSPIE_BeamGenerator_StoredMass")]//Stored Mass
        public double storedMassMultiplier;
        [KSPField(groupName = Group, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KSPIE_BeamGenerator_InitialMass", guiUnits = " t")]//Initial Mass
        public double initialMass;
        [KSPField(groupName = Group, guiActive = false, guiActiveEditor = false, guiName = "#LOC_KSPIE_BeamGenerator_TargetMass", guiUnits = " t")]//Target Mass
        public double targetMass = 1;
        [KSPField(groupName = Group, guiActive = true, guiActiveEditor = true, guiName = "#LOC_KSPIE_BeamGenerator_PartMass", guiUnits = " t")]//Part Mass
        public float partMass;

        [KSPField]
        public bool canSwitchWavelengthInFlight = true;
        [KSPField]
        public bool isLoaded;
        [KSPField]
        public int beamType = 1;
        [KSPField]
        public string techLevelMk1 = "start";
        [KSPField]
        public string techLevelMk2 = "";
        [KSPField]
        public string techLevelMk3 = "";
        [KSPField]
        public string techLevelMk4 = "";
        [KSPField]
        public string techLevelMk5 = "";
        [KSPField]
        public string techLevelMk6 = "";
        [KSPField]
        public string techLevelMk7 = "";
        [KSPField]
        public double powerMassFraction = 0.5;
        [KSPField]
        public bool fixedMass;
        [KSPField]
        public bool isInitialzed;

        ConfigNode[] _beamConfigurationNodes;
        BeamConfiguration _activeConfiguration;
        BeamedPowerTransmitter _transmitter;
        BaseField _chooseField;

        int _techLevel;

        private void DetermineTechLevel()
        {
            _techLevel = 0;
            if (PluginHelper.UpgradeAvailable(techLevelMk2))
                _techLevel++;
            if (PluginHelper.UpgradeAvailable(techLevelMk3))
                _techLevel++;
            if (PluginHelper.UpgradeAvailable(techLevelMk4))
                _techLevel++;
            if (PluginHelper.UpgradeAvailable(techLevelMk5))
                _techLevel++;
            if (PluginHelper.UpgradeAvailable(techLevelMk6))
                _techLevel++;
            if (PluginHelper.UpgradeAvailable(techLevelMk7))
                _techLevel++;
        }

        public void Connect(BeamedPowerTransmitter transmitter)
        {
            _transmitter = transmitter;
        }

        private int GetTechLevelFromTechId(string techID)
        {
            if (techID == techLevelMk7)
                return 7;
            else if (techID == techLevelMk6)
                return 6;
            else if (techID == techLevelMk5)
                return 5;
            else if (techID == techLevelMk4)
                return 4;
            else if (techID == techLevelMk3)
                return 3;
            else if (techID == techLevelMk2)
                return 2;
            else if (techID == techLevelMk1)
                return 1;
            else
                return 7;
        }

        private string GetColorCodeFromTechId(string techID)
        {
            if (techID == techLevelMk7)
                return "<color=#ee8800ff>";
            else if (techID == techLevelMk6)
                return "<color=#ee9900ff>";
            else if (techID == techLevelMk5)
                return "<color=#ffaa00ff>";
            else if (techID == techLevelMk4)
                return "<color=#ffbb00ff>";
            else if (techID == techLevelMk3)
                return "<color=#ffcc00ff>";
            else if (techID == techLevelMk2)
                return "<color=#ffdd00ff>";
            else if (techID == techLevelMk1)
                return "<color=#ffff00ff>";
            else
                return "<color=#ffff00ff>";
        }

        private IList<BeamConfiguration> _inlineConfigurations;

        public IList<BeamConfiguration> BeamConfigurations => _inlineConfigurations;
        public IEnumerable<BeamGenerator> FindBeamGenerators(Part origin)
        {
            var attachedParts = part.attachNodes.Where(m => m.attachedPart != null && m.attachedPart != origin);
            var attachedBeanGenerators = attachedParts.Select(m => m.attachedPart.FindModuleImplementing<BeamGenerator>()).Where(m => m != null);

            List<BeamGenerator> indirectBeamGenerators = attachedBeanGenerators.SelectMany(m => m.FindBeamGenerators(m.part)).ToList();
            indirectBeamGenerators.Insert(0, this);
            return indirectBeamGenerators;
        }

        // Note: do note remove, it is called by KSP
        public void Update()
        {
            partMass = part.mass;
        }

        public void UpdateMass(double maximumPower)
        {
            this.maximumPower = maximumPower;
            targetMass = maximumPower * powerMassFraction * 0.001;
        }

        public virtual void OnRescale(ScalingFactor factor)
        {
            try
            {
                Debug.Log("BeamGenerator.OnRescale called with " + factor.absolute.linear);
                storedMassMultiplier = Math.Pow((double)(decimal)factor.absolute.linear, 3);
                initialMass = (double)(decimal)part.prefabMass * storedMassMultiplier;
                if (maximumPower > 0)
                    targetMass = maximumPower * powerMassFraction * 0.001;
                else
                    targetMass = initialMass;
            }
            catch (Exception e)
            {
                Debug.LogError("[KSPI]: BeamGenerator.OnRescale" + e.Message);
            }
        }

        public override void OnStart(StartState state)
        {
            targetMass = part.prefabMass * storedMassMultiplier;
            initialMass = part.prefabMass * storedMassMultiplier;

            if (initialMass == 0)
                initialMass = (double)(decimal)part.prefabMass;
            if (targetMass == 0)
                targetMass = (double)(decimal)part.prefabMass;

            if (BeamConfigurations == null)
            {
                var rootNode = GameDatabase.Instance.GetConfigNodes("KIT_BeamConfiguration");
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

            InitializeWavelengthSelector();
            DetermineTechLevel();
        }

        private void InitializeWavelengthSelector()
        {
            Debug.Log("[KSPI]: Setup Transmit Beams Configurations for " + part.partInfo.title);

            _chooseField = Fields[nameof(selectedBeamConfiguration)];
            _chooseField.guiActive = BeamConfigurations.Count > 1 && (HighLogic.CurrentGame.Parameters.CustomParams<KITGamePlayParams>().ReconfigureAntennas || canSwitchWavelengthInFlight);

            var chooseOptionEditor = _chooseField.uiControlEditor as UI_ChooseOption;
            var chooseOptionFlight = _chooseField.uiControlFlight as UI_ChooseOption;

            var names = BeamConfigurations.Select(m => m.beamWaveName).ToArray();

            if (chooseOptionEditor != null)
                chooseOptionEditor.options = names;

            if (chooseOptionFlight != null)
                chooseOptionFlight.options = names;

            if (!string.IsNullOrEmpty(beamWaveName))
            {
                _activeConfiguration = BeamConfigurations.FirstOrDefault(m => String.Equals(m.beamWaveName, beamWaveName, StringComparison.CurrentCultureIgnoreCase));
                if (_activeConfiguration != null)
                {
                    selectedBeamConfiguration = BeamConfigurations.IndexOf(_activeConfiguration);
                    wavelength = _activeConfiguration.wavelength;
                    return;
                }
            }

            if (wavelength != 0)
            {
                // find first wavelength with equal or shorter wavelength
                _activeConfiguration = BeamConfigurations.FirstOrDefault(m => m.wavelength <= wavelength);

                if (_activeConfiguration == null)
                    _activeConfiguration = selectedBeamConfiguration < BeamConfigurations.Count ? BeamConfigurations[selectedBeamConfiguration] : BeamConfigurations.FirstOrDefault();

                if (_activeConfiguration != null)
                    selectedBeamConfiguration = BeamConfigurations.IndexOf(_activeConfiguration);
            }

            UpdateFromGUI(_chooseField, selectedBeamConfiguration);

            // connect on change event
            if (chooseOptionEditor != null) chooseOptionEditor.onFieldChanged = UpdateFromGUI;
            if (chooseOptionFlight != null) chooseOptionFlight.onFieldChanged = UpdateFromGUI;
        }

        public override void OnUpdate()
        {
            _chooseField.guiActive = HighLogic.CurrentGame.Parameters.CustomParams<KITGamePlayParams>().ReconfigureAntennas || (canSwitchWavelengthInFlight && BeamConfigurations.Count > 1);
        }

        /// <summary>
        /// Called whenever chooseOption is changed by user
        /// </summary>
        /// <param name="field"></param>
        /// <param name="oldFieldValueObj"></param>
        private void UpdateFromGUI(BaseField field, object oldFieldValueObj)
        {
            Debug.Log("[KSPI]: BeamGenerator UpdateFromGUI called");

            if (!BeamConfigurations.Any())
            {
                Debug.LogWarning("[KSPI]: BeamGenerator UpdateFromGUI no BeamConfigurations found");
                return;
            }

            if (isLoaded == false)
                LoadInitialConfiguration();
            else
            {
                if (selectedBeamConfiguration < BeamConfigurations.Count)
                {
                    _activeConfiguration = BeamConfigurations[selectedBeamConfiguration];
                }
                else
                {
                    Debug.LogWarning("[KSPI]: selectedBeamConfiguration < " + BeamConfigurations.Count + ", selecting last");
                    selectedBeamConfiguration = BeamConfigurations.Count - 1;
                    _activeConfiguration = BeamConfigurations.Last();
                }
            }

            if (_activeConfiguration == null)
            {
                Debug.Log("[KSPI]: UpdateFromGUI no activeConfiguration found");
                return;
            }

            beamWaveName = _activeConfiguration.beamWaveName;
            wavelength = _activeConfiguration.wavelength;
            wavelengthText = WavelengthToText(wavelength);
            atmosphericAbsorptionPercentage = _activeConfiguration.atmosphericAbsorptionPercentage;
            waterAbsorptionPercentage = _activeConfiguration.waterAbsorptionPercentage;

            UpdateEfficiencyPercentage();

            // synchronize with receiver;
            if (_transmitter != null && _transmitter.PartReceiver != null)
            {
                Debug.Log("[KSPI]: Called SetActiveBandwidthConfigurationByWaveLength with wavelength " + wavelength);
                _transmitter.PartReceiver.SetActiveBandwidthConfigurationByWaveLength(wavelength);
            }
            //else
            //{
            //    Debug.Log("[KSPI]: No transmitter found ");
            //}
        }

        private static string WavelengthToText(double wavelength)
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

        private void UpdateEfficiencyPercentage()
        {
            _techLevel = -1;

            if (PluginHelper.HasTechRequirementAndNotEmpty(_activeConfiguration.techRequirement3))
                _techLevel++;
            if (PluginHelper.HasTechRequirementAndNotEmpty(_activeConfiguration.techRequirement2))
                _techLevel++;
            if (PluginHelper.HasTechRequirementAndNotEmpty(_activeConfiguration.techRequirement1))
                _techLevel++;
            if (PluginHelper.HasTechRequirementAndNotEmpty(_activeConfiguration.techRequirement0))
                _techLevel++;

            switch (_techLevel)
            {
                case 3: efficiencyPercentage = _activeConfiguration.efficiencyPercentage3; break;
                case 2: efficiencyPercentage = _activeConfiguration.efficiencyPercentage2; break;
                case 1: efficiencyPercentage = _activeConfiguration.efficiencyPercentage1; break;
                case 0: efficiencyPercentage = _activeConfiguration.efficiencyPercentage0; break;
                default:
                    efficiencyPercentage = 0; break;
            }
        }

        private void LoadInitialConfiguration()
        {
            isLoaded = true;

            if (!string.IsNullOrEmpty(beamWaveName))
            {
                _activeConfiguration = BeamConfigurations.FirstOrDefault(m => String.Equals(m.beamWaveName, beamWaveName, StringComparison.CurrentCultureIgnoreCase));
                if (_activeConfiguration != null)
                {
                    selectedBeamConfiguration = BeamConfigurations.IndexOf(_activeConfiguration);
                    wavelength = _activeConfiguration.wavelength;
                    return;
                }
            }

            var currentWavelength = wavelength != 0 ? wavelength : 1;
            _activeConfiguration = BeamConfigurations.FirstOrDefault();

            selectedBeamConfiguration = 0;

            if (BeamConfigurations.Count <= 1 || _activeConfiguration == null)
                return;

            var lowestWavelengthDifference = Math.Abs(currentWavelength - _activeConfiguration.wavelength);

            foreach (var currentConfig in BeamConfigurations)
            {
                var configWaveLengthDifference = Math.Abs(currentWavelength - currentConfig.wavelength);

                if (!(configWaveLengthDifference < lowestWavelengthDifference)) continue;

                _activeConfiguration = currentConfig;
                lowestWavelengthDifference = configWaveLengthDifference;
                selectedBeamConfiguration = BeamConfigurations.IndexOf(currentConfig);
            }
        }

        public float GetModuleMass(float defaultMass, ModifierStagingSituation sit)
        {
            var moduleMassDelta = fixedMass ? 0 : targetMass - initialMass;

            return (float)moduleMassDelta;
        }

        public ModifierChangeWhen GetModuleMassChangeWhen()
        {
            return ModifierChangeWhen.STAGED;
        }

        public override void OnLoad(ConfigNode node)
        // public new void Load(ConfigNode node)
        {
            Debug.Log($"[KSPI Beam Generator] Load()ing");

            _beamConfigurationNodes = node.GetNodes("BeamConfiguration");

            if (!_beamConfigurationNodes.Any())
            {
                Debug.Log("[KSPI]: OnLoad Found no BeamConfigurations - something is broken.");
                return;
            }

            var inlineConfigurations = new List<BeamConfiguration>();
            foreach (var beamConfigurationNode in _beamConfigurationNodes)
            {
                var beamConfiguration = new BeamConfiguration(beamConfigurationNode, part.partInfo.title);
                if (beamConfiguration.IsValid)
                    inlineConfigurations.Add(beamConfiguration);
                else
                    Debug.Log($"[KSPI]: OnLoad discarding BeamConfiguration of {beamConfiguration.beamWaveName} / {beamConfiguration.TechLevel}, it is not valid.");
            }

            _inlineConfigurations = inlineConfigurations.OrderByDescending(m => m.wavelength).ToList();
        }

        public override void OnSave(ConfigNode node)
        {
            foreach (var beamConfiguration in _inlineConfigurations)
            {
                beamConfiguration.Save(node);
            }
        }

        public override string GetInfo()
        {

            return "";
            /*
            var sb = StringBuilderCache.Acquire();

            sb.Append(Localizer.Format("#LOC_KSPIE_BeamGenerator_Type")).Append(": ").AppendLine(beamTypeName);//Type
            sb.Append(Localizer.Format("#LOC_KSPIE_BeamGenerator_CanSwitch")).Append(": ");
            sb.AppendLine(RUIutils.GetYesNoUIString(canSwitchWavelengthInFlight)).AppendLine();//Can Switch In Flight

            if (!string.IsNullOrEmpty(techLevelMk2))
            {
                sb.Append("<color=#7fdfffff>").Append(Localizer.Format("#LOC_KSPIE_BeamGenerator_upgradeTechnologies")).AppendLine(":</color><size=10>");
                if (!string.IsNullOrEmpty(techLevelMk1))
                    sb.Append("<color=#ffff00ff>Mk1:</color> ").AppendLine(Localizer.Format(PluginHelper.GetTechTitleById(techLevelMk1)));
                if (!string.IsNullOrEmpty(techLevelMk2))
                    sb.Append("<color=#ffdd00ff>Mk2:</color> ").AppendLine(Localizer.Format(PluginHelper.GetTechTitleById(techLevelMk2)));
                if (!string.IsNullOrEmpty(techLevelMk3))
                    sb.Append("<color=#ffcc00ff>Mk3:</color> ").AppendLine(Localizer.Format(PluginHelper.GetTechTitleById(techLevelMk3)));
                if (!string.IsNullOrEmpty(techLevelMk4))
                    sb.Append("<color=#ffbb00ff>Mk4:</color> ").AppendLine(Localizer.Format(PluginHelper.GetTechTitleById(techLevelMk4)));
                if (!string.IsNullOrEmpty(techLevelMk5))
                    sb.Append("<color=#ffaa00ff>Mk5:</color> ").AppendLine(Localizer.Format(PluginHelper.GetTechTitleById(techLevelMk5)));
                sb.AppendLine("</size>");
            }

            if (_inlineConfigurations.Count <= 0) return sb.ToString();

            sb.Append("<color=#7fdfffff>").Append(Localizer.Format("#LOC_KSPIE_BeamGenerator_atmosphericAbsorbtion")).AppendLine(":</color><size=10>");
            foreach (var beamConfiguration in _inlineConfigurations)
            {
                sb.Append(ExtendWithSpace(beamConfiguration.atmosphericAbsorptionPercentage + "%", 4));
                sb.Append(" / ").Append(ExtendWithSpace(beamConfiguration.waterAbsorptionPercentage + "%", 4));
                sb.Append("<color=#00ff00ff> ").Append(beamConfiguration.beamWaveName).AppendLine("</color>");
            }
            sb.AppendLine("</size>");

            sb.Append("<color=#7fdfffff>").Append(Localizer.Format("#LOC_KSPIE_BeamGenerator_beamEfficiencies")).AppendLine(":</color><size=10>");            
            foreach (var beamConfiguration in _inlineConfigurations)
            {
                sb.Append("<color=#00ff00ff>").Append(beamConfiguration.beamWaveName).Append("</color>");
                sb.Append("<color=#00e600ff> (").Append(WavelengthToText(beamConfiguration.wavelength)).AppendLine(")</color>  ");
                if (beamConfiguration.efficiencyPercentage0 > 0)
                    sb.Append(GetColorCodeFromTechId(beamConfiguration.techRequirement0)).Append("Mk").
                        Append(GetTechLevelFromTechId(beamConfiguration.techRequirement0)).Append(":</color> ").
                        Append(beamConfiguration.efficiencyPercentage0).Append("% ");
                if (beamConfiguration.efficiencyPercentage1 > 0)
                    sb.Append(GetColorCodeFromTechId(beamConfiguration.techRequirement1)).Append("Mk").
                        Append(GetTechLevelFromTechId(beamConfiguration.techRequirement1)).Append(":</color> ")
                        .Append(beamConfiguration.efficiencyPercentage1).Append("% ");
                if (beamConfiguration.efficiencyPercentage2 > 0)
                    sb.Append(GetColorCodeFromTechId(beamConfiguration.techRequirement2)).Append("Mk").
                        Append(GetTechLevelFromTechId(beamConfiguration.techRequirement2)).Append(":</color> ").
                        Append(beamConfiguration.efficiencyPercentage2).Append("% ");
                if (beamConfiguration.efficiencyPercentage3 > 0)
                    sb.Append(GetColorCodeFromTechId(beamConfiguration.techRequirement3)).Append("Mk").
                        Append(GetTechLevelFromTechId(beamConfiguration.techRequirement3)).Append(":</color> ").
                        Append(beamConfiguration.efficiencyPercentage3).Append("% ");
                sb.AppendLine();
            }
            sb.Append("</size>");

            return sb.ToStringAndRelease();
            */
        }

        private string ExtendWithSpace(string input, int targetLength)
        {
            return input + AddSpaces(targetLength - input.Length);
        }

        private static string AddSpaces(int length)
        {
            var result = "";
            for (var i = 0; i < length; i++)
            {
                result += " ";
            }
            return result;
        }
    }
}
