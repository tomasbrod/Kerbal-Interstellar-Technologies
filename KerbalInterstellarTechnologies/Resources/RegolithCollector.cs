﻿using KIT.ResourceScheduler;
using KSP.Localization;
using System;
using UnityEngine;

namespace KIT.Resources
{
    class RegolithCollector : PartModule, IKITModule
    {
        public const string Group = "RegolithCollector";
        public const string GroupTitle = "#LOC_KSPIE_RegolithCollector_groupName";

        // Persistent True
        [KSPField(isPersistant = true)]
        public bool bIsEnabled;
        [KSPField(isPersistant = true)]
        public double lastPowerPercentage;
        [KSPField(isPersistant = true)]
        public double dLastRegolithConcentration;

        // Part properties
        [KSPField(groupName = Group, groupDisplayName = GroupTitle, guiActiveEditor = true, guiName = "#LOC_KSPIE_RegolithCollector_Drillsize", guiUnits = " m\xB3")]//Drill size
        public double drillSize; // Volume of the collector's drill. Raise in part config (for larger drills) to make collecting faster.
        [KSPField(groupName = Group, guiActiveEditor = true, guiName = "#LOC_KSPIE_RegolithCollector_Effectiveness", guiFormat = "P1")]//Drill effectiveness
        public double effectiveness = 1; // Effectiveness of the drill. Lower in part config (to a 0.5, for example) to slow down resource collecting.
        [KSPField(groupName = Group, guiActiveEditor = true, guiName = "#LOC_KSPIE_RegolithCollector_MWRequirements", guiUnits = "#LOC_KSPIE_Reactor_megawattUnit")]//MW Requirements
        public double mwRequirements = 1; // MW requirements of the drill. Affects heat produced.
        [KSPField(groupName = Group, guiActiveEditor = true, guiName = "#LOC_KSPIE_RegolithCollector_WasteHeatModifier", guiFormat = "P1")]//Waste Heat Modifier
        public double wasteHeatModifier = 1; // How much of the power requirements ends up as heat. Change in part cfg, treat as a percentage (1 = 100%). Higher modifier means more energy ends up as waste heat.

        // GUI
        [KSPField(groupName = Group, guiActive = true, guiName = "#LOC_KSPIE_RegolithCollector_RegolithConcentration", guiFormat = "P1")]//Regolith Concentration
        protected string strRegolithConc = "";
        [KSPField(groupName = Group, guiActive = true, guiName = "#LOC_KSPIE_RegolithCollector_Distancefromthesun")]//Distance from the sun
        protected string strStarDist = "";
        [KSPField(groupName = Group, guiActive = true, guiName = "#LOC_KSPIE_RegolithCollector_Drillstatus")]//Drill status
        protected string strCollectingStatus = "";
        [KSPField(groupName = Group, guiActive = true, guiName = "#LOC_KSPIE_RegolithCollector_ReceivedPower")]//Power Usage
        protected string strReceivedPower = "";
        [KSPField(groupName = Group, guiActive = true, guiName = "#LOC_KSPIE_RegolithCollector_Altitude", guiUnits = " m")]//Altitude
        protected string strAltitude = "";
        [KSPField(groupName = Group, isPersistant = true, guiActive = true, guiName = "#LOC_KSPIE_RegolithCollector_ResourceProduction", guiUnits = " Unit/s")]//Resource Production
        public double resourceProduction;


        [KSPEvent(groupName = Group, guiActive = true, guiName = "#LOC_KSPIE_RegolithCollector_ActivateDrill", active = true)]//Activate Drill
        public void ActivateCollector()
        {
            if (IsCollectLegal() != true) return;

            TouchDown = TryRaycastToHitTerrain(); // check if there's ground within reach and if the drill is deployed
            if (TouchDown == false) // if not, no collecting
            {
                ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_KSPIE_RegolithCollector_PostMsg1"), 3, ScreenMessageStyle.LOWER_CENTER);//"Regolith drill not in contact with ground. Make sure drill is deployed and can reach the terrain."
                DisableCollector();
                return;
            }
            bIsEnabled = true;
            OnUpdate();
        }

        [KSPEvent(groupName = Group, guiActive = true, guiName = "#LOC_KSPIE_RegolithCollector_DisableDrill", active = true)]//Disable Drill
        public void DisableCollector()
        {
            bIsEnabled = false;
            OnUpdate();
        }

        [KSPAction("Activate Drill")]
        public void ActivateScoopAction(KSPActionParam param)
        {
            ActivateCollector();
        }

        [KSPAction("Disable Drill")]
        public void DisableScoopAction(KSPActionParam param)
        {
            DisableCollector();
        }

        [KSPAction("Toggle Drill")]
        public void ToggleScoopAction(KSPActionParam param)
        {
            if (bIsEnabled)
                DisableCollector();
            else
                ActivateCollector();
        }

        protected string RegolithResourceName;
        //protected double dDistanceFromStar = 0; // distance of the current vessel from the system's star
        protected double ConcentrationRegolith; // regolith concentration at the current location
        protected double RegolithSpareCapacity; // spare capacity for the regolith on the vessel
        protected double RegolithDensity; // 'density' of regolith at the current spot
        protected double TotalWasteHeatProduction; // total waste heat produced in the cycle
        //protected double dAltitude = 0; // current terrain altitude
        protected bool TouchDown; // helper bool, is the part touching the ground

        private uint _counter; // helper counter for update cycles, so that we can only do some calculations once in a while
        private uint _anotherCounter; // helper counter for fixedupdate cycles, so that we can only do some calculations once in a while (I don't want to add complexity by using the previous counter in two places - also update and fixedupdate cycles can be out of sync, apparently)
        protected double FinalConcentration { get; set; }

        AbundanceRequest _regolithRequest = new AbundanceRequest // create a new request object that we'll reuse to get the current stock-system resource concentration
        {
            ResourceType = HarvestTypes.Planetary,
            ResourceName = KITResourceSettings.Regolith,
            BodyId = 1, // this will need to be updated before 'sending the request'
            Latitude = 0, // this will need to be updated before 'sending the request'
            Longitude = 0, // this will need to be updated before 'sending the request'
            Altitude = 0, // this will need to be updated before 'sending the request'
            CheckForLock = false
        };
        protected CelestialBody LocalStar;

        public override void OnStart(StartState state)
        {
            if (state == StartState.Editor) return; // collecting won't work in editor

            Debug.Log("[KSPI]: RegolithCollector on " + part.name + " was Force Activated");
            part.force_activate();

            LocalStar = KopernicusHelper.GetLocalStar(vessel.mainBody);

            // gets density of the regolith resource
            RegolithResourceName = KITResourceSettings.Regolith;
            RegolithDensity = (double)(decimal)PartResourceLibrary.Instance.GetDefinition(RegolithResourceName).density;

            // this bit goes through parts that contain animations and disables the "Status" field in GUI part window so that it's less crowded
            var MAGlist = part.FindModulesImplementing<ModuleAnimateGeneric>();
            foreach (ModuleAnimateGeneric MAG in MAGlist)
            {
                MAG.Fields[nameof(MAG.status)].guiActive = false;
                MAG.Fields[nameof(MAG.status)].guiActiveEditor = false;
            }
        }


        public override void OnUpdate()
        {
            Events[nameof(ActivateCollector)].active = !bIsEnabled; // will activate the event (i.e. show the gui button) if the process is not enabled
            Events[nameof(DisableCollector)].active = bIsEnabled; // will show the button when the process IS enabled

            Fields[nameof(strReceivedPower)].guiActive = bIsEnabled;

            /*
             * Regolith concentration doesn't really need to be calculated and updated in gui on every update.
             * By hiding it behind a counter that only runs this code once per ten cycles, it should be more performance friendly.
             */
            if (++_counter % 10 != 0) return;

            ConcentrationRegolith = GetFinalConcentration();

            /*
             * If collecting is legal, update the regolith concentration in GUI, otherwise pass a zero string.
             * This way we shouldn't get readings when the vessel is flying or splashed or on a planet with an atmosphere.
             */
            strRegolithConc = IsCollectLegal() ? ConcentrationRegolith.ToString("P0") : "0";
            
            /*
             * F1 string format means fixed point number with one decimal place (i.e. number 1234.567 would be formatted as 1234.5). I might change this eventually to
             * P1 or P0 (num multiplied by hundred and percentage sign with 1 or 0 dec. places).
             * Also update the current altitude in GUI
             */
            strAltitude = (vessel.altitude < 15000) ? (vessel.altitude).ToString("F0") : Localizer.Format("#LOC_KSPIE_RegolithCollector_Altitudetoohigh");//"Too damn high"
            
        }

        public override void OnFixedUpdate()
        {

        }

        // checks if the vessel is not in atmosphere and if it can therefore collect regolith. Also checks if the vessel is landed and if it is not splashed (not sure if non atmospheric bodies can have oceans in KSP or modded galaxies, let's put this in to be sure)
        private bool IsCollectLegal()
        {
            if (vessel.checkLanded() == false || vessel.checkSplashed())
            {
                strStarDist = UpdateDistanceInGui();
                strRegolithConc = "0";
                return false;
            }

            // won't collect in significant atmosphere
            if (FlightGlobals.currentMainBody.atmosphere && FlightGlobals.currentMainBody.atmDensityASL * FlightGlobals.currentMainBody.atmosphereDepth > 7000)
            {
                strStarDist = UpdateDistanceInGui();
                strRegolithConc = "0";
                return false;
            }

            return true; // all checks green, ok to collect
        }

        // this snippet returns true if the part is extended
        private bool IsDrillExtended()
        {
            ModuleAnimationGroup thisPartsAnimGroup = part.FindModuleImplementing<ModuleAnimationGroup>();
            return thisPartsAnimGroup.isDeployed;
        }

        private bool TryRaycastToHitTerrain()
        {
            Vector3d partPosition = part.transform.position; // find the position of the transform in 3d space
            double scaleFactor = part.rescaleFactor; // what is the rescale factor of the drill?
            float drillDistance = (float)(5 * scaleFactor); // adjust the distance for the ray with the rescale factor, needs to be a float for raycast. The 5 is just about the reach of the drill.

            LayerMask terrainMask = 32768; // layermask in unity, number 1 bitshifted to the left 15 times (1 << 15), (terrain = 15, the bitshift is there so that the mask bits are raised; this is a good reading about that: http://answers.unity3d.com/questions/8715/how-do-i-use-layermasks.html)
            Ray drillPartRay = new Ray(partPosition, -part.transform.up); // this ray will start at the part's center and go down in local space coordinates (Vector3d.down is in world space)

            /* This little bit will fire a ray from the part, straight down, in the distance that the part should be able to reach.
             * It returns true if there is solid terrain in the reach AND the drill is extended. Otherwise false.
             * This is actually needed because stock KSP terrain detection is not really dependable. This module was formerly using just part.GroundContact
             * to check for contact, but that seems to be bugged somehow, at least when paired with this drill - it works enough times to pass tests, but when testing
             * this module in a difficult terrain, it just doesn't work properly.
            */
            Physics.Raycast(drillPartRay, out var hit, drillDistance, terrainMask); // use the defined ray, pass info about a hit, go the proper distance and choose the proper layermask
            if (hit.collider == null) return false;

            return IsDrillExtended();
        }

        private double GetFinalConcentration()
        {
            FinalConcentration = CalculateRegolithConcentration(FlightGlobals.currentMainBody.position, LocalStar.transform.position, vessel.altitude);
            FinalConcentration += AdjustConcentrationForLocation();
            return FinalConcentration;
        }

        private double AdjustConcentrationForLocation()
        {
            _regolithRequest.BodyId = FlightGlobals.currentMainBody.flightGlobalsIndex;
            _regolithRequest.Latitude = vessel.latitude;
            _regolithRequest.Longitude = vessel.longitude;
            _regolithRequest.Altitude = vessel.altitude;
            double concentration = ResourceMap.Instance.GetAbundance(_regolithRequest);
            return concentration;
        }

        // calculates regolith concentration - right now just based on the distance of the planet from the sun, so planets will have uniform distribution. We might add latitude as a factor etc.
        private static double CalculateRegolithConcentration(Vector3d planetPosition, Vector3d sunPosition, double altitude)
        {
            // if my reasoning is correct, this is not only the average distance of Kerbin, but also for the Mun.
            // Maybe this is obvious to everyone else or wrong, but I'm tired, so there.
            var avgMunDistance = GameConstants.KerbinSunDistance; 

             /* I decided to incorporate an altitude modifier. According to https://curator.jsc.nasa.gov/lunar/letss/regolith.pdf, most regolith on Moon is deposited in
             * higher altitudes. This is great from a gameplay perspective, because it makes an incentive for players to collect regolith in more difficult circumstances
             * (i.e. landing on highlands instead of flats etc.) and breaks the flatter-is-better base building strategy at least a bit.
             * This check will divide current altitude by 2500. At that arbitrarily-chosen altitude, we should be getting the basic concentration for the planet.
             * Go to a higher terrain and you will find **more** regolith. The + 500 shift is there so that even at altitude of 0 (i.e. Minmus flats etc.) there will
             * still be at least SOME regolith to be mined.
             */
            var altModifier = (altitude + 500) / 2500;
            var atmosphereModifier = Math.Pow(1 - FlightGlobals.currentMainBody.atmDensityASL, FlightGlobals.currentMainBody.atmosphereDepth * 0.001);

            var concentration = atmosphereModifier * altModifier * (avgMunDistance / Vector3d.Distance(planetPosition, sunPosition)); // get a basic concentration. Should range from numbers lower than one for planets further away from the sun, to about 2.5 at Moho
            return concentration;
        }

        // calculates the distance to sun
        private static double CalculateDistanceToSun(Vector3d vesselPosition, Vector3d sunPosition)
        {
            return Vector3d.Distance(vesselPosition, sunPosition);
        }

        // helper function for readying the distance for the GUI
        private string UpdateDistanceInGui()
        {
            return ((CalculateDistanceToSun(part.transform.position, LocalStar.transform.position) - LocalStar.Radius) / 1000).ToString("F0") + " km";
        }

        // the main collecting function
        private void CollectRegolith(IResourceManager resMan)
        {
            ConcentrationRegolith = GetFinalConcentration();

            double dPowerRequirementsMw = PluginSettings.Config.PowerConsumptionMultiplier * mwRequirements; // change the mwRequirements number in part config to change the power consumption

            RegolithSpareCapacity = resMan.SpareCapacity(ResourceName.Regolith);

            if (ConcentrationRegolith > 0 && (RegolithSpareCapacity > 0))
            {
                // Determine available power, using EC if below 2 MW required
                double powerReceivedMw = resMan.Consume(ResourceName.ElectricCharge, dPowerRequirementsMw);

                // show in GUI
                strCollectingStatus = Localizer.Format("#LOC_KSPIE_RegolithCollector_Collectingregolith");//"Collecting regolith"
            }
            else
            {
                lastPowerPercentage = 0;
                dPowerRequirementsMw = 0;
            }

            strReceivedPower = PluginHelper.GetFormattedPowerString(lastPowerPercentage * dPowerRequirementsMw) + " / " +
                PluginHelper.GetFormattedPowerString(dPowerRequirementsMw);

            /*
             * The first important bit.
             * This determines how much solar wind will be collected. Can be tweaked in part configs by changing the collector's effectiveness.
             */

            resourceProduction = ConcentrationRegolith * drillSize * RegolithDensity * effectiveness * lastPowerPercentage;

            // this is the second important bit - do the actual change of the resource amount in the vessel
            resMan.Produce(ResourceName.Regolith, resourceProduction);

            /* This takes care of waste heat production (but takes into account if waste heat mechanics weren't disabled).
             * It's affected by two properties of the drill part - its power requirements and waste heat production percentage.
             * More power hungry drills will produce more heat. More effective drills will produce less heat. More effective power hungry drills should produce
             * less heat than less effective power hungry drills. This should allow us to bring some variety into parts, if we want to.
             */

            if (CheatOptions.IgnoreMaxTemperature) return;

            TotalWasteHeatProduction = dPowerRequirementsMw * wasteHeatModifier; // calculate amount of heat to be produced
            resMan.Produce(ResourceName.WasteHeat, TotalWasteHeatProduction);
        }

        public ModuleConfigurationFlags ModuleConfiguration() => ModuleConfigurationFlags.Third;

        public void KITFixedUpdate(IResourceManager resMan)
        {
            if (FlightGlobals.fetch == null) return;

            if (!bIsEnabled)
            {
                strCollectingStatus = Localizer.Format("#LOC_KSPIE_RegolithCollector_Disabled");//"Disabled"
                strStarDist = UpdateDistanceInGui(); // passes the distance to the GUI
                return;
            }

            // won't collect in atmosphere, while splashed and while flying
            if (!IsCollectLegal())
            {
                DisableCollector();
                return;
            }

            strStarDist = UpdateDistanceInGui();

            // collect solar wind for a single frame
            CollectRegolith(resMan);

            // store current solar wind concentration in case vessel is unloaded
            //dLastRegolithConcentration = CalculateRegolithConcentration(FlightGlobals.currentMainBody.position, localStar.transform.position, vessel.altitude);
            dLastRegolithConcentration = GetFinalConcentration();

            /*
             * This bit will check if the regolith drill has not lost contact with ground. Raycasts are apparently not all that expensive, but still,
             * the counter will delay the check so that it runs only once per hundred cycles. This should be enough and should make it more performance friendly and
             * also less prone to kraken glitches. It also makes sure that this doesn't run before the vessel is fully loaded and shown to the player.
             */
            if (++_anotherCounter % 100 != 0) return;

            TouchDown = TryRaycastToHitTerrain();
            if (TouchDown) return;

            ScreenMessages.PostScreenMessage(Localizer.Format("#LOC_KSPIE_RegolithCollector_PostMsg2"), 3, ScreenMessageStyle.LOWER_CENTER);//"Regolith drill not in contact with ground. Disabling drill."
            DisableCollector();
        }

        public string KITPartName() => part.partInfo.title;
    }
}
