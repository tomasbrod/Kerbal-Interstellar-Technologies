﻿using KIT.Extensions;
using KIT.Resources;
using KIT.ResourceScheduler;
using KSP.Localization;
using System;
using UnityEngine;

namespace KIT.Propulsion
{
    public class ModuleEnginesWarp : ModuleEnginesFX, IKITModule
    {
        [KSPField(isPersistant = true)]
        bool IsForceActivated;
        [KSPField(guiActive = false, guiName = "#LOC_KSPIE_ModuleEnginesWarp_MassFlow")]//Mass Flow
        public double requestedFlow;

        [KSPField]
        public double GThreshold = 9;
        [KSPField]
        public string propellant1 = "LqdHydrogen";
        [KSPField]
        public string propellant2;
        [KSPField]
        public string propellant3;
        [KSPField]
        public string propellant4;

        [KSPField]
        public double ratio1 = 1;
        [KSPField]
        public double ratio2;
        [KSPField]
        public double ratio3;
        [KSPField]
        public double ratio4;

        [KSPField]
        public double demandMass;
        [KSPField]
        public double remainingMass;


        [KSPField]
        public double fuelRatio;
        [KSPField]
        private double averageDensityInTonPerLiter;
        [KSPField]
        private double massPropellantRatio;
        [KSPField]
        private double ratioSumWithoutMass;
        [KSPField]
        private double ratioHeadingVersusRequest;
        [KSPField]
        public double totalmassVessel;
        [KSPField]
        public double massDelta;
        [KSPField]
        public double deltaV;

        [KSPField(guiActive = true, guiName = "#autoLOC_6001377", guiUnits = "#autoLOC_7001408", guiFormat = "F6")]
        public double thrust_d;

        Transform _engineThrustTransform;
        Vector3d _engineThrustTransformUp;

        protected double isp_d;
        protected double throttle_d;


        [KSPField]
        public double _realIsp;
        [KSPField]
        public double _thrustPersistent;
        [KSPField]
        public bool _getIgnitionState;
        [KSPField]
        public float _currentThrottle;

        // Persistent values to use during timewarp
        double _throttlePersistent;

        int vesselChangedSIOCountdown;

        private double fuelWithMassPercentage1;
        private double fuelWithMassPercentage2;
        private double fuelWithMassPercentage3;
        private double fuelWithMassPercentage4;

        private double masslessFuelPercentage1;
        private double masslessFuelPercentage2;
        private double masslessFuelPercentage3;
        private double masslessFuelPercentage4;

        public double fuelRequestAmount1;
        public double fuelRequestAmount2;
        public double fuelRequestAmount3;
        public double fuelRequestAmount4;

        double consumedPropellant1;
        double consumedPropellant2;
        double consumedPropellant3;
        double consumedPropellant4;

        PartResourceDefinition propellantResourceDefinition1;
        PartResourceDefinition propellantResourceDefinition2;
        PartResourceDefinition propellantResourceDefinition3;
        PartResourceDefinition propellantResourceDefinition4;

        ResourceName propellantResourceID1;
        ResourceName propellantResourceID2;
        ResourceName propellantResourceID3;
        ResourceName propellantResourceID4;

        // Are we transitioning from timewarp to real time?
        bool _warpToReal;

        public new void FixedUpdate()
        {
        }

        public void VesselChangedSOI()
        {
            vesselChangedSIOCountdown = 10;
        }

        // Update
        public override void OnUpdate()
        {
            // stop engines and drop out of timewarp when X pressed
            if (vessel.packed && _throttlePersistent > 0 && Input.GetKeyDown(KeyCode.X))
            {
                // Return to realtime
                TimeWarp.SetRate(0, true);

                _throttlePersistent = 0;
                vessel.ctrlState.mainThrottle = (float)_throttlePersistent;
            }

            // When transitioning from timewarp to real update throttle
            if (_warpToReal)
            {
                vessel.ctrlState.mainThrottle = (float)_throttlePersistent;
                _warpToReal = false;
            }

            // hide stock thrust
            Fields["finalThrust"].guiActive = false;

            if (IsForceActivated || !isEnabled || !isOperational) return;

            IsForceActivated = true;
            Debug.Log("[KSPI]: ModuleEngineWarp on " + part.name + " was Force Activated");
            part.force_activate();
        }

        private void UpdateFuelFactors()
        {
            if (!String.IsNullOrEmpty(propellant1))
            {
                propellantResourceDefinition1 = PartResourceLibrary.Instance.GetDefinition(propellant1);
                propellantResourceID1 = KITResourceSettings.NameToResource(propellant1);

                if (propellantResourceDefinition1 == null || propellantResourceID1 == ResourceName.Unknown)
                {
                    Debug.Log($"[ModuleEnginesWarp] UpdateFuelFactors propellant1 is not correctly defined -- {(propellantResourceDefinition1 == null ? "definition" : "resource id")}");
                }
            }
            else
            {
                propellantResourceDefinition1 = null;
                propellantResourceID1 = ResourceName.Unknown;
            }

            if (!String.IsNullOrEmpty(propellant2))
            {
                propellantResourceDefinition2 = PartResourceLibrary.Instance.GetDefinition(propellant2);
                propellantResourceID2 = KITResourceSettings.NameToResource(propellant2);

                if (propellantResourceDefinition2 == null || propellantResourceID2 == ResourceName.Unknown)
                {
                    Debug.Log("[ModuleEnginesWarp] UpdateFuelFactors propellant2 is not correctly defined");
                }
            }
            else
            {
                propellantResourceDefinition2 = null;
                propellantResourceID2 = ResourceName.Unknown;
            }

            if (!String.IsNullOrEmpty(propellant3))
            {
                propellantResourceDefinition3 = PartResourceLibrary.Instance.GetDefinition(propellant3);
                propellantResourceID3 = KITResourceSettings.NameToResource(propellant3);

                if (propellantResourceDefinition3 == null || propellantResourceID3 == ResourceName.Unknown)
                {
                    Debug.Log("[ModuleEnginesWarp] UpdateFuelFactors propellant3 is not correctly defined");
                }
            }
            else
            {
                propellantResourceDefinition3 = null;
                propellantResourceID3 = ResourceName.Unknown;
            }

            if (!String.IsNullOrEmpty(propellant4))
            {
                propellantResourceDefinition4 = PartResourceLibrary.Instance.GetDefinition(propellant4);
                propellantResourceID4 = KITResourceSettings.NameToResource(propellant4);

                if (propellantResourceDefinition4 == null || propellantResourceID4 == ResourceName.Unknown)
                {
                    Debug.Log("[ModuleEnginesWarp] UpdateFuelFactors propellant4 is not correctly defined");
                }
            }
            else
            {
                propellantResourceDefinition4 = null;
                propellantResourceID4 = ResourceName.Unknown;
            }

            var ratioSumOverall = 0.0;
            var ratioSumWithMass = 0.0;
            var densitySum = 0.0;

            if (propellantResourceDefinition1 != null)
            {
                ratioSumOverall += ratio1;
                if (propellantResourceDefinition1.density > 0)
                {
                    ratioSumWithMass = ratio1;
                    densitySum += propellantResourceDefinition1.density * ratio1;
                }
            }
            if (propellantResourceDefinition2 != null)
            {
                ratioSumOverall += ratio2;
                if (propellantResourceDefinition2.density > 0)
                {
                    ratioSumWithMass = ratio2;
                    densitySum += propellantResourceDefinition2.density * ratio2;
                }
            }
            if (propellantResourceDefinition3 != null)
            {
                ratioSumOverall += ratio3;
                if (propellantResourceDefinition3.density > 0)
                {
                    ratioSumWithMass = ratio3;
                    densitySum += propellantResourceDefinition3.density * ratio3;
                }
            }
            if (propellantResourceDefinition4 != null)
            {
                ratioSumOverall += ratio4;
                if (propellantResourceDefinition4.density > 0)
                {
                    ratioSumWithMass = ratio4;
                    densitySum += propellantResourceDefinition4.density * ratio4;
                }
            }

            averageDensityInTonPerLiter = densitySum / ratioSumWithMass;
            massPropellantRatio = ratioSumWithMass / ratioSumOverall;
            ratioSumWithoutMass = ratioSumOverall - ratioSumWithMass;

            fuelWithMassPercentage1 = propellantResourceDefinition1 != null && propellantResourceDefinition1.density > 0 ? ratio1 / ratioSumWithMass : 0;
            fuelWithMassPercentage2 = propellantResourceDefinition2 != null && propellantResourceDefinition2.density > 0 ? ratio2 / ratioSumWithMass : 0;
            fuelWithMassPercentage3 = propellantResourceDefinition3 != null && propellantResourceDefinition3.density > 0 ? ratio3 / ratioSumWithMass : 0;
            fuelWithMassPercentage4 = propellantResourceDefinition4 != null && propellantResourceDefinition4.density > 0 ? ratio4 / ratioSumWithMass : 0;

            masslessFuelPercentage1 = propellantResourceDefinition1 != null && propellantResourceDefinition1.density <= 0 ? ratio1 / ratioSumWithoutMass : 0;
            masslessFuelPercentage2 = propellantResourceDefinition2 != null && propellantResourceDefinition2.density <= 0 ? ratio2 / ratioSumWithoutMass : 0;
            masslessFuelPercentage3 = propellantResourceDefinition3 != null && propellantResourceDefinition3.density <= 0 ? ratio3 / ratioSumWithoutMass : 0;
            masslessFuelPercentage4 = propellantResourceDefinition4 != null && propellantResourceDefinition4.density <= 0 ? ratio4 / ratioSumWithoutMass : 0;
        }


        public static double GetResourceAvailable(Part part, PartResourceDefinition definition, ResourceFlowMode flowmode)
        {
            if (definition == null)
            {
                Debug.LogError("[KSPI]: PartResourceDefinition definition is NULL");
                return 0;
            }

            part.GetConnectedResourceTotals(definition.id, flowmode, out var currentAmount, out _);
            return currentAmount;
        }

        private double CollectFuel(IResourceManager resMan, double demandMass, ResourceFlowMode fuelMode = ResourceFlowMode.STACK_PRIORITY_SEARCH)
        {
            fuelRequestAmount1 = 0;
            fuelRequestAmount2 = 0;
            fuelRequestAmount3 = 0;
            fuelRequestAmount4 = 0;

            if (CheatOptions.InfinitePropellant)
                return 1;

            if (demandMass == 0 || double.IsNaN(demandMass) || double.IsInfinity(demandMass))
                return 0;

            var propellantWithMassNeededInLiter = demandMass / averageDensityInTonPerLiter;
            var overallAmountNeeded = propellantWithMassNeededInLiter / massPropellantRatio;
            var masslessResourceNeeded = overallAmountNeeded - propellantWithMassNeededInLiter;

            // first determine lowest available resource ratio
            double availableRatio = 1;
            if (propellantResourceDefinition1 != null && ratio1 > 0)
            {
                fuelRequestAmount1 = fuelWithMassPercentage1 > 0 ? fuelWithMassPercentage1 * propellantWithMassNeededInLiter : masslessFuelPercentage1 * masslessResourceNeeded;
                availableRatio = Math.Min(availableRatio, resMan.CurrentCapacity(propellantResourceID1) / fuelRequestAmount1);

                Debug.Log($"[CollectFuel] first availableRatio is {availableRatio}");
            }
            if (propellantResourceDefinition2 != null && ratio2 > 0)
            {
                fuelRequestAmount2 = fuelWithMassPercentage2 > 0 ? fuelWithMassPercentage2 * propellantWithMassNeededInLiter : masslessFuelPercentage2 * masslessResourceNeeded;
                availableRatio = Math.Min(availableRatio, resMan.CurrentCapacity(propellantResourceID2) / fuelRequestAmount2);

                Debug.Log($"[CollectFuel] second availableRatio is {availableRatio}");
            }
            if (propellantResourceDefinition3 != null && ratio3 > 0)
            {
                fuelRequestAmount3 = fuelWithMassPercentage3 > 0 ? fuelWithMassPercentage3 * propellantWithMassNeededInLiter : masslessFuelPercentage3 * masslessResourceNeeded;
                availableRatio = Math.Min(availableRatio, resMan.CurrentCapacity(propellantResourceID3) / fuelRequestAmount3);
            }
            if (propellantResourceDefinition4 != null && ratio4 > 0)
            {
                fuelRequestAmount4 = fuelWithMassPercentage4 > 0 ? fuelWithMassPercentage4 * propellantWithMassNeededInLiter : masslessFuelPercentage4 * masslessResourceNeeded;
                availableRatio = Math.Min(availableRatio, resMan.CurrentCapacity(propellantResourceID4) / fuelRequestAmount4);
            }

            // ignore insignificant amount
            if (availableRatio < 1e-6)
                return 0;

            consumedPropellant1 = 0;
            consumedPropellant2 = 0;
            consumedPropellant3 = 0;
            consumedPropellant4 = 0;

            double receivedRatio = 1;
            if (fuelRequestAmount1 > 0 && !double.IsNaN(fuelRequestAmount1) && !double.IsInfinity(fuelRequestAmount1))
            {
                consumedPropellant1 = resMan.Consume(propellantResourceID1, fuelRequestAmount1 * availableRatio);
                receivedRatio = Math.Min(receivedRatio, fuelRequestAmount1 > 0 ? consumedPropellant1 / fuelRequestAmount1 : 0);

                Debug.Log($"[CollectFuel] first receivedRatio is {receivedRatio}");
            }
            if (fuelRequestAmount2 > 0 && !double.IsNaN(fuelRequestAmount2) && !double.IsInfinity(fuelRequestAmount2))
            {
                consumedPropellant2 = resMan.Consume(propellantResourceID2, fuelRequestAmount2 * availableRatio);
                receivedRatio = Math.Min(receivedRatio, fuelRequestAmount2 > 0 ? consumedPropellant2 / fuelRequestAmount2 : 0);

                Debug.Log($"[CollectFuel] second receivedRatio is {receivedRatio}");
            }
            if (fuelRequestAmount3 > 0 && !double.IsNaN(fuelRequestAmount3) && !double.IsInfinity(fuelRequestAmount3))
            {
                consumedPropellant3 = resMan.Consume(propellantResourceID3, fuelRequestAmount3 * availableRatio);
                receivedRatio = Math.Min(receivedRatio, fuelRequestAmount3 > 0 ? consumedPropellant3 / fuelRequestAmount3 : 0);
            }
            if (fuelRequestAmount4 > 0 && !double.IsNaN(fuelRequestAmount4) && !double.IsInfinity(fuelRequestAmount4))
            {
                consumedPropellant4 = resMan.Consume(propellantResourceID4, fuelRequestAmount4 * availableRatio);
                receivedRatio = Math.Min(receivedRatio, fuelRequestAmount4 > 0 ? consumedPropellant4 / fuelRequestAmount4 : 0);
            }

            Debug.Log($"[CollectFuel] receivedRatio is {receivedRatio}");
            return Math.Min(receivedRatio, 1);
        }

        // Physics update
        // public override void OnFixedUpdate()
        // {
            // deliberately empty
        // }

        private bool IsPositiveValidNumber(double variable)
        {
            return !double.IsNaN(variable) && !double.IsInfinity(variable) && variable > 0;
        }

        // Format thrust into mN, N, kN
        public static string FormatThrust(double thrust)
        {
            if (thrust < 1e-6)
                return Math.Round(thrust * 1e+9, 3) + " µN";
            if (thrust < 1e-3)
                return Math.Round(thrust * 1e+6, 3) + " mN";
            else if (thrust < 1)
                return Math.Round(thrust * 1e+3, 3) + " N";
            else
                return Math.Round(thrust, 3) + " kN";
        }

        public ModuleConfigurationFlags ModuleConfiguration() => ModuleConfigurationFlags.Fifth;
        
        public void KITFixedUpdate(IResourceManager resMan)
        {
            if (vesselChangedSIOCountdown > 0)
                vesselChangedSIOCountdown--;

            if (FlightGlobals.fetch == null || !isEnabled) return;

            UpdateFuelFactors();

            if (double.IsNaN(requestedMassFlow) || double.IsInfinity(requestedMassFlow))
                Debug.LogWarning("[KSPI]: requestedMassFlow  is " + requestedMassFlow);
            if (double.IsNaN(realIsp) || double.IsInfinity(realIsp))
                Debug.LogWarning("[KSPI]: realIsp  is " + realIsp);
            if (double.IsNaN(finalThrust) || double.IsInfinity(finalThrust))
                Debug.LogWarning("[KSPI]: finalThrust  is " + finalThrust);

            _realIsp = realIsp;
            _currentThrottle = currentThrottle;
            _getIgnitionState = getIgnitionState;

            requestedFlow = requestedMassFlow;
            totalmassVessel = vessel.totalMass;

            // Check if we are in time warp mode
            if (!vessel.packed)
            {
                // allow throttle to be used up to Geeforce threshold
                TimeWarp.GThreshold = GThreshold;

                demandMass = requestedFlow; // * (double)(decimal)resMan.FixedDeltaTime();

                // if not transitioning from warp to real
                // Update values to use during timewarp
                if (!_warpToReal)
                {
                    _throttlePersistent = currentThrottle; //vessel.ctrlState.mainThrottle;

                    if (_throttlePersistent == 0 && finalThrust < 0.0000005)
                        _thrustPersistent = 0;
                    else
                        _thrustPersistent = finalThrust;
                }

                ratioHeadingVersusRequest = 0;
            }
            else
            {
                // Timewarp mode: perturb orbit using thrust
                _warpToReal = true; // Set to true for transition to realtime

                _thrustPersistent = requestedFlow * PhysicsGlobals.GravitationalAcceleration * _realIsp;

                // only persist thrust if active and non zero throttle or significant thrust
                if (getIgnitionState && (currentThrottle > 0 || _thrustPersistent > 0.0000005))
                {
                    ratioHeadingVersusRequest = vessel.PersistHeading(vesselChangedSIOCountdown > 0, ratioHeadingVersusRequest == 1);
                    if (ratioHeadingVersusRequest != 1)
                    {
                        //UnityEngine.Debug.Log("[KSPI]: " + "quit persistent heading: " + ratioHeadingVersusRequest);
                        return;
                    }

                    // determine maximum deltaV during this frame
                    demandMass = requestedFlow * resMan.FixedDeltaTime();
                    remainingMass = totalmassVessel - demandMass;

                    deltaV = _realIsp * PhysicsGlobals.GravitationalAcceleration * Math.Log(totalmassVessel / remainingMass);

                    _engineThrustTransform = part.FindModelTransform(thrustVectorTransformName);
                    if (_engineThrustTransform == null)
                    {
                        _engineThrustTransform = part.transform;
                        _engineThrustTransformUp = _engineThrustTransform.up;
                    }
                    else
                        _engineThrustTransformUp = (Vector3d)_engineThrustTransform.forward * -1;

                    double persistentThrustDot = Vector3d.Dot(_engineThrustTransformUp, vessel.obt_velocity);
                    if (persistentThrustDot < 0 && (vessel.obt_velocity.magnitude <= deltaV * 2))
                    {
                        var message = Localizer.Format("#LOC_KSPIE_ModuleEnginesWarp_PostMsg1");//"Thrust warp stopped - orbital speed too low"
                        ScreenMessages.PostScreenMessage(message, 5, ScreenMessageStyle.UPPER_CENTER);
                        Debug.Log("[KSPI]: " + message);
                        TimeWarp.SetRate(0, true);
                        return;
                    }

                    // using the resource manager interface will reduce the requested resources to fixedDeltaTime for us.
                    fuelRatio = CollectFuel(resMan, requestedFlow, ResourceFlowMode.ALL_VESSEL);

                    // Calculate thrust and deltaV if demand output > 0
                    if (IsPositiveValidNumber(fuelRatio) && IsPositiveValidNumber(demandMass) && IsPositiveValidNumber(totalmassVessel) && IsPositiveValidNumber(_realIsp))
                    {
                        remainingMass = vessel.totalMass - (demandMass * fuelRatio); // Mass at end of burn

                        massDelta = Math.Log(totalmassVessel / remainingMass);
                        deltaV = _realIsp * PhysicsGlobals.GravitationalAcceleration * massDelta; // Delta V from burn
                        vessel.orbit.Perturb(deltaV * _engineThrustTransformUp, Planetarium.GetUniversalTime()); // Update vessel orbit

                        if (fuelRatio < 0.999)
                        {
                            var message = Localizer.Format("#LOC_KSPIE_ModuleEnginesWarp_PostMsg2");//"Thrust warp stopped - running out of propellant"
                            Debug.Log("[KSPI]: " + message);
                            ScreenMessages.PostScreenMessage(message, 5, ScreenMessageStyle.UPPER_CENTER);
                            // Return to realtime
                            TimeWarp.SetRate(0, true);
                        }
                    }
                    else if (demandMass > 0)
                    {
                        var message = Localizer.Format("#LOC_KSPIE_ModuleEnginesWarp_PostMsg3");//"Thrust warp stopped - propellant depleted"
                        Debug.Log("[KSPI]: " + message);
                        ScreenMessages.PostScreenMessage(message, 5, ScreenMessageStyle.UPPER_CENTER);
                        // Return to realtime
                        TimeWarp.SetRate(0, true);
                    }
                }
                else
                {
                    ratioHeadingVersusRequest = vessel.PersistHeading(vesselChangedSIOCountdown > 0);

                    _thrustPersistent = 0;
                    requestedFlow = 0;
                    demandMass = 0;
                    fuelRatio = 0;
                }
            }

            // Update display numbers
            thrust_d = _thrustPersistent;
            isp_d = _realIsp;
            throttle_d = _throttlePersistent;

            base.FixedUpdate();
        }

        public string KITPartName() => part.partInfo.title;
    }

}
