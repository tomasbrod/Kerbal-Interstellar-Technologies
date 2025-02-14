﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace KIT.Resources
{
    /// <summary>
    /// If you change this, then you'll also need to change the following areas of code at a minimum:
    ///   - ResourceSettings.ResourceToName
    ///   - ResourceSettings.ValidateResource
    /// </summary>
    public enum ResourceName
    {
        Unknown,

        // We will track vessel wide stats between ElectricCharge and WasteHeat. Do not change this ordering
        // unless you have consulted ResourceManager.cs and resourceProductionStats, as otherwise you might be in for a surprise.

        /// <summary>
        /// ElectricCharge by default is measured in Megajoules
        /// </summary>
        ElectricCharge,
        /// <summary>
        /// ThermalPower by default is measured in Megajoules
        /// </summary>
        ThermalPower,
        /// <summary>
        /// ChargedParticle by default is measured in Megajoules
        /// </summary>
        ChargedParticle,
        /// <summary>
        /// WasteHeat by default is measured in Megajoules
        /// </summary>
        WasteHeat,
        // End vessel wide tracking. 

        // Automatic resource conversion beginning
        // Keep Gas in alphabetical order, then liquids underneath in alphabetical order.

        CarbonDioxideGas,
        CarbonMonoxideGas,
        DeuteriumGas,
        Helium4Gas,
        Helium3Gas,
        HydrogenGas,
        KryptonGas,
        MethaneGas,
        NeonGas,
        NitrogenGas,
        OxygenGas,
        TritiumGas,
        XenonGas,

        CarbonDioxideLqd,
        CarbonMonoxideLqd,
        DeuteriumLqd,
        Helium4Lqd,
        Helium3Lqd,
        HydrogenLqd,
        KryptonLqd,
        MethaneLqd,
        NeonLqd,
        NitrogenLqd,
        OxygenLqd,
        TritiumLqd,
        XenonLqd,

        // end of automatic resource conversion, the following order is not important.

        LiquidFuel,
        Oxidizer,
        MonoPropellant,

        Alumina,
        Aluminium,
        AmmoniaLqd,
        ArgonLqd,
        HydrogenPeroxide,
        Hydrazine,
        FluorineGas,
        Lithium6,
        Lithium7,
        ChlorineGas,
        Nitrogen15Lqd,
        Regolith,
        Sodium,
        SolarWind,
        WaterHeavy,
        WaterPure,
        WaterRaw,

        Actinides,
        DepletedFuel,
        EnrichedUranium,
        DepletedUranium,
        Plutonium238,
        ThoriumTetraflouride,
        UraniumTetraflouride,
        Uranium233,
        UraniumNitride,

        AntiProtium,
        VacuumPlasma,
        ExoticMatter,

        IntakeOxygenAir,
        IntakeLiquid,
        IntakeAtmosphere,

        FusionPellets,
        FissionParticles,
        LithiumDeuteride,

        EndResource,
    }

    public static class KITResourceSettings
    {
        public static string Unknown { get; private set; } = "Unknown";
        public static string EndResource { get; private set; } = "EndResource";

        #region Builtin resources
        public static string ElectricCharge { get; private set; } = "ElectricCharge";
        public static string LiquidFuel { get; private set; } = "LiquidFuel";
        public static string Oxidizer { get; private set; } = "Oxidizer";
        public static string MonoPropellant { get; private set; } = "MonoPropellant";
        #endregion

        #region KIT Basic Resources
        public static string WasteHeat { get; private set; } = "WasteHeat";
        #endregion

        #region Radioactive Resources
        public static string Actinides { get; private set; } = "Actinides";
        public static string DepletedFuel { get; private set; } = "DepletedFuel";
        public static string EnrichedUranium { get; private set; } = "EnrichedUranium";
        public static string DepletedUranium { get; private set; } = "DepletedUranium";
        public static string Plutonium238 { get; private set; } = "Plutonium-238";
        public static string ThoriumTetraflouride { get; private set; } = "ThF4";
        public static string UraniumTetraflouride { get; private set; } = "UF4";
        public static string Uranium233 { get; private set; } = "Uranium-233";
        public static string UraniumNitride { get; private set; } = "UraniumNitride";
        #endregion

        #region Chemical Resources
        public static string Alumina { get; private set; } = "Alumina";
        public static string Aluminium { get; private set; } = "Aluminium";
        public static string AmmoniaLqd { get; private set; } = "LqdAmmonia";
        public static string ArgonLqd { get; private set; } = "LqdArgon";
        public static string CarbonDioxideGas { get; private set; } = "CarbonDioxide";
        public static string CarbonDioxideLqd { get; private set; } = "LqdCO2";
        public static string CarbonMonoxideGas { get; private set; } = "CarbonMonoxide";
        public static string CarbonMonoxideLqd { get; private set; } = "LqdCO";
        public static string DeuteriumLqd { get; private set; } = "LqdDeuterium";
        public static string DeuteriumGas { get; private set; } = "Deuterium";
        public static string Helium4Gas { get; private set; } = "Helium";
        public static string Helium4Lqd { get; private set; } = "LqdHelium";
        public static string Helium3Gas { get; private set; } = "Helium3";
        public static string Helium3Lqd { get; private set; } = "LqdHe3";
        public static string HydrogenGas { get; private set; } = "Hydrogen";
        public static string HydrogenLqd { get; private set; } = "LqdHydrogen";
        public static string HydrogenPeroxide { get; private set; } = "HTP";
        public static string Hydrazine { get; private set; } = "Hydrazine";
        public static string FluorineGas { get; private set; } = "Fluorine";
        public static string KryptonGas { get; private set; } = "KryptonGas";
        public static string KryptonLqd { get; private set; } = "LqdKrypton";
        public static string Lithium6 { get; private set; } = "Lithium6";
        public static string Lithium7 { get; private set; } = "Lithium";
        public static string ChlorineGas { get; private set; } = "Chlorine";
        public static string MethaneGas { get; private set; } = "Methane";
        public static string MethaneLqd { get; private set; } = "LqdMethane";
        public static string NeonGas { get; private set; } = "NeonGas";
        public static string NeonLqd { get; private set; } = "LqdNeon";
        public static string NitrogenGas { get; private set; } = "Nitrogen";
        public static string NitrogenLqd { get; private set; } = "LqdNitrogen";
        public static string Nitrogen15Lqd { get; private set; } = "LqdNitrogen15";
        public static string OxygenGas { get; private set; } = "Oxygen";
        public static string OxygenLqd { get; private set; } = "LqdOxygen";
        public static string Regolith { get; private set; } = "Regolith";
        public static string Sodium { get; private set; } = "Sodium";
        public static string SolarWind { get; private set; } = "SolarWind";
        public static string TritiumGas { get; private set; } = "Tritium";
        public static string TritiumLqd { get; private set; } = "LqdTritium";
        public static string WaterHeavy { get; private set; } = "HeavyWater";
        public static string WaterPure { get; private set; } = "Water";
        public static string WaterRaw { get; private set; } = "LqdWater";
        public static string XenonGas { get; private set; } = "XenonGas";
        public static string XenonLqd { get; private set; } = "LqdXenon";
        public static string FusionPellets { get; private set; } = "FusionPellets";
        public static string FissionParticles { get; private set; } = "FissionParticles";
        public static string LithiumDeuteride { get; private set; } = "LithiumDeuteride";

        #endregion

        #region Pseudo resources
        public static string AntiProtium { get; private set; } = "Antimatter";
        public static string VacuumPlasma { get; private set; } = "VacuumPlasma";
        public static string ExoticMatter { get; private set; } = "ExoticMatter";
        /// <summary>
        /// ChargedParticle is measured in Megajoules
        /// </summary>
        public static string ChargedParticle { get; private set; } = "ChargedParticles";
        /// <summary>
        /// ThermalPower is measured in Megajoules
        /// </summary>
        public static string ThermalPower { get; private set; } = "ThermalPower";

        #endregion

        #region Abstract Resources
        public static string IntakeOxygenAir { get; private set; } = "IntakeAir";
        public static string IntakeLiquid { get; private set; } = "IntakeLqd";
        public static string IntakeAtmosphere { get; private set; } = "IntakeAtm";
        #endregion

        #region Community Resource Pack

        #endregion

        public static string ResourceToName(ResourceName resource)
        {
            switch (resource)
            {
                case ResourceName.Unknown: return ElectricCharge;
                case ResourceName.ElectricCharge: return ElectricCharge;
                case ResourceName.LiquidFuel: return LiquidFuel;
                case ResourceName.Oxidizer: return Oxidizer;
                case ResourceName.MonoPropellant: return MonoPropellant;
                case ResourceName.Actinides: return Actinides;
                case ResourceName.DepletedFuel: return DepletedFuel;
                case ResourceName.EnrichedUranium: return EnrichedUranium;
                case ResourceName.DepletedUranium: return DepletedUranium;
                case ResourceName.Plutonium238: return Plutonium238;
                case ResourceName.ThoriumTetraflouride: return ThoriumTetraflouride;
                case ResourceName.UraniumTetraflouride: return UraniumTetraflouride;
                case ResourceName.Uranium233: return Uranium233;
                case ResourceName.UraniumNitride: return UraniumNitride;

                case ResourceName.Alumina: return "Alumina";
                case ResourceName.Aluminium: return "Aluminium";
                case ResourceName.AmmoniaLqd: return "LqdAmmonia";
                case ResourceName.ArgonLqd: return "LqdArgon";
                case ResourceName.CarbonDioxideGas: return "CarbonDioxide";
                case ResourceName.CarbonDioxideLqd: return "LqdCO2";
                case ResourceName.CarbonMonoxideGas: return "CarbonMonoxide";
                case ResourceName.CarbonMonoxideLqd: return "LqdCO";
                case ResourceName.DeuteriumLqd: return "LqdDeuterium";
                case ResourceName.DeuteriumGas: return "Deuterium";
                case ResourceName.Helium4Gas: return "Helium";
                case ResourceName.Helium4Lqd: return "LqdHelium";
                case ResourceName.Helium3Gas: return "Helium3";
                case ResourceName.Helium3Lqd: return "LqdHe3";
                case ResourceName.HydrogenGas: return "Hydrogen";
                case ResourceName.HydrogenLqd: return "LqdHydrogen";
                case ResourceName.HydrogenPeroxide: return "HTP";
                case ResourceName.Hydrazine: return "Hydrazine";
                case ResourceName.FluorineGas: return "Fluorine";
                case ResourceName.KryptonGas: return "KryptonGas";
                case ResourceName.KryptonLqd: return "LqdKrypton";
                case ResourceName.Lithium6: return "Lithium6";
                case ResourceName.Lithium7: return "Lithium";
                case ResourceName.ChlorineGas: return "Chlorine";
                case ResourceName.MethaneGas: return "Methane";
                case ResourceName.MethaneLqd: return "LqdMethane";
                case ResourceName.NeonGas: return "LqdGas";
                case ResourceName.NeonLqd: return "LqdNeon";
                case ResourceName.NitrogenGas: return "Nitrogen";
                case ResourceName.NitrogenLqd: return "LqdNitrogen";
                case ResourceName.Nitrogen15Lqd: return "LqdNitrogen15";
                case ResourceName.OxygenGas: return "Oxygen";
                case ResourceName.OxygenLqd: return "LqdOxygen";
                case ResourceName.Regolith: return "Regolith";
                case ResourceName.Sodium: return "Sodium";
                case ResourceName.SolarWind: return "SolarWind";
                case ResourceName.TritiumGas: return "Tritium";
                case ResourceName.TritiumLqd: return "LqdTritium";
                case ResourceName.WaterHeavy: return "HeavyWater";
                case ResourceName.WaterPure: return "Water";
                case ResourceName.WaterRaw: return "LqdWater";
                case ResourceName.XenonGas: return "XenonGas";
                case ResourceName.XenonLqd: return "LqdXenon";

                case ResourceName.AntiProtium: return AntiProtium;
                case ResourceName.VacuumPlasma: return VacuumPlasma;
                case ResourceName.ExoticMatter: return ExoticMatter;
                case ResourceName.ChargedParticle: return ChargedParticle;
                case ResourceName.ThermalPower: return ThermalPower;

                case ResourceName.IntakeOxygenAir: return IntakeOxygenAir;
                case ResourceName.IntakeLiquid: return IntakeLiquid;
                case ResourceName.IntakeAtmosphere: return IntakeAtmosphere;

                case ResourceName.WasteHeat: return WasteHeat;

                case ResourceName.FusionPellets: return FusionPellets;
                case ResourceName.FissionParticles: return FissionParticles;
                case ResourceName.LithiumDeuteride: return LithiumDeuteride;
                case ResourceName.EndResource: return EndResource;
                default: throw new InvalidEnumArgumentException(nameof(resource), (int)resource, typeof(ResourceName));
            }
        }

        private static Dictionary<string, ResourceName> _nameToResourceMap;

        /// <summary>
        /// Converts a string to a given ResourceName identifier.
        /// </summary>
        /// <param name="name">Resource name to convert</param>
        /// <returns>Corresponding ResourceName identifier if found, otherwise Unknown.</returns>
        public static ResourceName NameToResource(string name)
        {
            if (_nameToResourceMap == null)
            {
                _nameToResourceMap = new Dictionary<string, ResourceName>(64);

                for (var i = 0; i < (int)ResourceName.EndResource; i++)
                {
                    _nameToResourceMap[ResourceToName((ResourceName)i)] = (ResourceName)i;
                }
            }

            if (!_nameToResourceMap.ContainsKey(name))
            {
                // Debug.Log($"[ResourceSettings.ResourceName] requested to map Unknown resource {name} - this will likely blow up");
                return ResourceName.Unknown;
            }

            return _nameToResourceMap[name];
        }

        /// <summary>
        /// Blows up if the supplied resource value does not map to the enum structure above.
        /// </summary>
        /// <param name="resource"></param>
        public static void ValidateResource(ResourceName resource)
        {
            if (resource <= ResourceName.Unknown || resource >= ResourceName.EndResource)
                throw new InvalidEnumArgumentException(nameof(resource), (int)resource, typeof(ResourceName));
        }


        private static void UpdatePropertyWithConfigNode(ConfigNode pluginSettings, string resourceName, Action<string> property)
        {
            if (!pluginSettings.HasValue(resourceName + "ResourceName")) return;

            var value = pluginSettings.GetValue(resourceName + "ResourceName");
            property(value);
            Debug.Log("[KIT]: " + resourceName + " resource name set to " + property);
        }

        private static bool _warningDisplayed;

        static KITResourceSettings()
        {
            var configRoot = GameDatabase.Instance.GetConfigNodes("KIT_PLUGIN_SETTINGS");
            if (configRoot == null || !configRoot.Any())
            {
                if (_warningDisplayed) return;
                // Localizer.Format() does not work during game initialization, as Unity is not ready for it to call some functions that it does.
                var errorMessage = "KSP Interstellar is unable to detect files required for proper functioning.  Please make sure that this mod has been installed to [Base KSP directory]/GameData/Kerbal-Interstellar-Technologies.";
                PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), "KIT Error", "Kerbal Interstellar Technologies Installation Error", errorMessage, "OK", false, HighLogic.UISkin);

                _warningDisplayed = true;

                return;
            }

            var pluginSettings = configRoot[0];

            // chemical resources
            UpdatePropertyWithConfigNode(pluginSettings, nameof(Actinides), value => Actinides = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(Aluminium), value => Aluminium = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(Alumina), value => Alumina = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(AmmoniaLqd), value => AmmoniaLqd = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(ArgonLqd), value => ArgonLqd = value);

            UpdatePropertyWithConfigNode(pluginSettings, nameof(CarbonDioxideGas), value => CarbonDioxideGas = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(CarbonDioxideLqd), value => CarbonDioxideLqd = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(CarbonMonoxideGas), value => CarbonMonoxideGas = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(CarbonMonoxideLqd), value => CarbonMonoxideLqd = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(ChlorineGas), value => ChlorineGas = value);

            UpdatePropertyWithConfigNode(pluginSettings, nameof(DeuteriumGas), value => DeuteriumGas = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(DeuteriumLqd), value => DeuteriumLqd = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(FluorineGas), value => FluorineGas = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(Helium4Gas), value => Helium4Gas = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(Helium4Lqd), value => Helium4Lqd = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(Helium3Gas), value => Helium3Gas = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(Helium3Lqd), value => Helium3Lqd = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(HydrogenGas), value => HydrogenGas = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(HydrogenLqd), value => HydrogenLqd = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(HydrogenPeroxide), value => HydrogenPeroxide = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(Hydrazine), value => Hydrazine = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(KryptonGas), value => KryptonGas = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(KryptonLqd), value => KryptonLqd = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(Lithium6), value => Lithium6 = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(Lithium7), value => Lithium7 = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(MethaneGas), value => MethaneGas = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(MethaneLqd), value => MethaneLqd = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(NeonGas), value => NeonGas = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(NeonLqd), value => NeonLqd = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(NitrogenGas), value => NitrogenGas = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(NitrogenLqd), value => NitrogenLqd = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(Nitrogen15Lqd), value => Nitrogen15Lqd = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(OxygenGas), value => OxygenGas = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(OxygenLqd), value => OxygenLqd = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(Regolith), value => Regolith = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(Sodium), value => Sodium = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(SolarWind), value => SolarWind = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(WaterPure), value => WaterPure = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(WaterRaw), value => WaterRaw = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(WaterHeavy), value => WaterHeavy = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(TritiumGas), value => TritiumGas = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(TritiumLqd), value => TritiumLqd = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(XenonGas), value => XenonGas = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(XenonLqd), value => XenonLqd = value);

            // abstract resources
            UpdatePropertyWithConfigNode(pluginSettings, nameof(IntakeAtmosphere), value => IntakeAtmosphere = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(IntakeOxygenAir), value => IntakeOxygenAir = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(IntakeLiquid), value => IntakeOxygenAir = value);

            // nuclear resources
            UpdatePropertyWithConfigNode(pluginSettings, nameof(DepletedFuel), value => DepletedFuel = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(EnrichedUranium), value => EnrichedUranium = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(DepletedUranium), value => DepletedUranium = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(Plutonium238), value => Plutonium238 = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(ThoriumTetraflouride), value => ThoriumTetraflouride = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(Uranium233), value => Uranium233 = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(UraniumNitride), value => UraniumNitride = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(UraniumTetraflouride), value => UraniumTetraflouride = value);

            // pseudo resources
            UpdatePropertyWithConfigNode(pluginSettings, nameof(ElectricCharge), value => ElectricCharge = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(ChargedParticle), value => ChargedParticle = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(ThermalPower), value => ThermalPower = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(WasteHeat), value => WasteHeat = value);

            UpdatePropertyWithConfigNode(pluginSettings, nameof(AntiProtium), value => AntiProtium = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(ExoticMatter), value => ExoticMatter = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(VacuumPlasma), value => VacuumPlasma = value);
            UpdatePropertyWithConfigNode(pluginSettings, nameof(FissionParticles), value => FissionParticles = value);
        }

    }
}
