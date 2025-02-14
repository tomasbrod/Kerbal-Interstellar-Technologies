﻿using System;
using System.Collections.Generic;
using System.Linq;
using KIT.Extensions;

namespace KIT.BeamedPower
{
    public static class BeamedPowerHelper
    {
        private static double ComputeSpotSize(WaveLengthData waveLengthData, double distanceToSpot, double transmitterAperture, Vessel receivingVessel, Vessel sendingVessel, double apertureMultiplier = 1)
        {
            if (transmitterAperture == 0)
                transmitterAperture = 1;

            if (waveLengthData.Wavelength == 0)
                waveLengthData.Wavelength = 1;

            var effectiveApertureBonus = waveLengthData.Wavelength >= 0.001
                ? 10 * apertureMultiplier
                : apertureMultiplier;

            var spotSize = (1.22 * distanceToSpot * waveLengthData.Wavelength) / (transmitterAperture * effectiveApertureBonus);

            return spotSize;
        }

        private static double ComputeDistanceFacingEfficiency(double spotSizeDiameter, double facingFactor, double diameter, double facingEfficiencyExponent = 1, double spotSizeNormalizationExponent = 1)
        {
            if (spotSizeDiameter <= 0
                || Double.IsPositiveInfinity(spotSizeDiameter)
                || Double.IsNaN(spotSizeDiameter)
                || facingFactor <= 0
                || diameter <= 0)
                return 0;

            var effectiveDistanceFacingEfficiency = Math.Pow(facingFactor, facingEfficiencyExponent) * Math.Pow(Math.Min(1, diameter * facingFactor / spotSizeDiameter), spotSizeNormalizationExponent);

            return effectiveDistanceFacingEfficiency;
        }

        private static double ComputeFacingFactor(Vessel transmitterVessel, IBeamedPowerReceiver receiver)
        {
            // return if no receiving is possible
            if (receiver.HighSpeedAtmosphereFactor == 0 && !receiver.CanBeActiveInAtmosphere)
                return 0;

            return ComputeFacingFactor(transmitterVessel.GetVesselPos(), receiver);
        }

        private static double ComputeFacingFactor(Vector3d transmitPosition, IBeamedPowerReceiver receiver)
        {
            double facingFactor;
            Vector3d directionVector = (transmitPosition - receiver.Vessel.GetVesselPos()).normalized;

            switch (receiver.ReceiverType)
            {
                case 0:
                    //Scale energy reception based on angle of receiver to transmitter from top
                    facingFactor = Math.Max(0, Vector3d.Dot(receiver.Part.transform.up, directionVector));
                    break;
                case 1:
                    // receive from sides
                    facingFactor = Math.Min(1 - Math.Abs(Vector3d.Dot(receiver.Part.transform.up, directionVector)), 1);
                    break;
                case 2:
                    // get the best result of inline and directed receiver
                    facingFactor = Math.Min(1 - Math.Abs(Vector3d.Dot(receiver.Part.transform.up, directionVector)), 1);
                    break;
                case 3:
                    //Scale energy reception based on angle of receiver to transmitter from back
                    facingFactor = Math.Max(0, -Vector3d.Dot(receiver.Part.transform.forward, directionVector));
                    break;
                case 4:
                    // used by single pivoting solar arrays
                    facingFactor = Math.Min(1 - Math.Abs(Vector3d.Dot(receiver.Part.transform.right, directionVector)), 1);
                    break;
                case 5:
                    //Scale energy reception based on angle of receiver to transmitter from bottom
                    facingFactor = Math.Max(0, -Vector3d.Dot(receiver.Part.transform.up, directionVector));
                    break;
                case 6:
                    facingFactor = Math.Min(1, Math.Abs(Vector3d.Dot(receiver.Part.transform.forward, directionVector)));
                    break;
                case 7:
                    //Scale energy reception based on angle of receiver to transmitter from top or bottom
                    facingFactor = Math.Max(0, Math.Abs(Vector3d.Dot(receiver.Part.transform.up, directionVector)));
                    break;
                default:
                    //Scale energy reception based on angle of receiver to transmitter from top
                    facingFactor = Math.Max(0, Vector3d.Dot(receiver.Part.transform.up, directionVector));
                    break;
            }

            if (facingFactor > receiver.FacingThreshold)
                facingFactor = Math.Pow(facingFactor, receiver.FacingSurfaceExponent);
            else
                facingFactor = 0;

            if (receiver.ReceiverType == 2)
                facingFactor = Math.Max(facingFactor, Math.Round(0.4999 + Math.Max(0, Vector3d.Dot(receiver.Part.transform.up, directionVector))));

            return receiver.CanBeActiveInAtmosphere ? facingFactor : receiver.HighSpeedAtmosphereFactor * facingFactor;
        }

        private static double ComputeVisibilityAndDistance(VesselRelayPersistence relay, Vessel targetVessel)
        {
            var result = relay.Vessel.HasLineOfSightWith(targetVessel, 0)
                ? Vector3d.Distance(relay.Vessel.GetVesselPos(), targetVessel.GetVesselPos())
                : -1;

            return result;
        }

        private static double ComputeDistance(Vessel v1, Vessel v2)
        {
            return Vector3d.Distance(v1.GetVesselPos(), v2.GetVesselPos());
        }

        private static double GetAtmosphericEfficiency(Vessel v)
        {
            return Math.Exp(-(FlightGlobals.getStaticPressure(v.GetVesselPos()) / 100) / 5);
        }

        private static double GetAtmosphericEfficiency(double transmitterPressure, double receiverPressure, double waveLengthAbsorption, double distanceInMeter, Vessel receiverVessel, Vessel transmitterVessel)
        {
            // if both in space, efficiency is 100%
            if (transmitterPressure == 0 && receiverPressure == 0)
                return 1;

            var atmosphereDepthInMeter = Math.Max(transmitterVessel.mainBody.atmosphereDepth, receiverVessel.mainBody.atmosphereDepth);

            // calculate the weighted distance a signal
            double atmosphericDistance;
            if (receiverVessel.mainBody == transmitterVessel.mainBody)
            {
                var receiverAltitudeModifier = atmosphereDepthInMeter > 0 && receiverVessel.altitude > atmosphereDepthInMeter
                    ? atmosphereDepthInMeter / receiverVessel.altitude
                    : 1;
                var transmitterAltitudeModifier = atmosphereDepthInMeter > 0 && transmitterVessel.altitude > atmosphereDepthInMeter
                    ? atmosphereDepthInMeter / transmitterVessel.altitude
                    : 1;
                atmosphericDistance = transmitterAltitudeModifier * receiverAltitudeModifier * distanceInMeter;
            }
            else
            {
                var altitudeModifier = transmitterPressure > 0 && receiverPressure > 0 && transmitterVessel.mainBody.atmosphereDepth > 0 && receiverVessel.mainBody.atmosphereDepth > 0
                    ? Math.Max(0, 1 - transmitterVessel.altitude / transmitterVessel.mainBody.atmosphereDepth)
                    + Math.Max(0, 1 - receiverVessel.altitude / receiverVessel.mainBody.atmosphereDepth)
                    : 1;

                atmosphericDistance = altitudeModifier * atmosphereDepthInMeter;
            }

            var absorptionRatio = Math.Pow(atmosphericDistance, Math.Sqrt(Math.Pow(transmitterPressure, 2) + Math.Pow(receiverPressure, 2))) / atmosphereDepthInMeter * waveLengthAbsorption;

            return Math.Exp(-absorptionRatio);
        }

        /// <summary>
        /// Returns transmitters which this vessel can connect
        /// </summary>
        /// <param name="maxHops">Maximum number of relays which can be used for connection to transmitter</param>
        public static IDictionary<VesselMicrowavePersistence, KeyValuePair<MicrowaveRoute, IList<VesselRelayPersistence>>> GetConnectedTransmitters(IBeamedPowerReceiver receiver, int maxHops = 25)
        {
            //these two dictionaries store transmitters and relays and best currently known route to them which is replaced if better one is found. 

            var transmitterRouteDictionary = new Dictionary<VesselMicrowavePersistence, MicrowaveRoute>(); // stores all transmitter we can have a connection with
            var relayRouteDictionary = new Dictionary<VesselRelayPersistence, MicrowaveRoute>();

            var transmittersToCheck = new List<VesselMicrowavePersistence>();//stores all transmitters to which we want to connect

            var receiverAtmosphericPressure = FlightGlobals.getStaticPressure(receiver.Vessel.GetVesselPos()) * 0.01;

            foreach (var vesselMicrowavePersistence in BeamedPowerSources.Instance.GlobalTransmitters.Values)
            {
                var transmitter = (VesselMicrowavePersistence) vesselMicrowavePersistence;

                //ignore if no power or transmitter is on the same vessel
                if (transmitter.Vessel == receiver.Vessel)
                {
                    //Debug.Log("[KSPI]: Transmitter vessel is equal to receiver vessel");
                    continue;
                }

                //first check for direct connection from current vessel to transmitters, will always be optimal
                if (!transmitter.HasPower)
                {
                    //Debug.Log("[KSPI]: Transmitter vessel has no power available");
                    continue;
                }

                if (receiver.Vessel.HasLineOfSightWith(transmitter.Vessel))
                {
                    double facingFactor = ComputeFacingFactor(transmitter.Vessel, receiver);
                    if (facingFactor <= 0)
                        continue;

                    var possibleWavelengths = new List<MicrowaveRoute>();
                    double distanceInMeter = ComputeDistance(receiver.Vessel, transmitter.Vessel);

                    double transmitterAtmosphericPressure = FlightGlobals.getStaticPressure(transmitter.Vessel.GetVesselPos()) * 0.01;

                    foreach (WaveLengthData wavelengthData in transmitter.SupportedTransmitWavelengths)
                    {
                        if (wavelengthData.Wavelength.NotWithin(receiver.MaximumWavelength, receiver.MinimumWavelength))
                            continue;

                        var spotSize = ComputeSpotSize(wavelengthData, distanceInMeter, transmitter.Aperture, receiver.Vessel, transmitter.Vessel, receiver.ApertureMultiplier);

                        double distanceFacingEfficiency = ComputeDistanceFacingEfficiency(spotSize, facingFactor, receiver.Diameter, receiver.FacingEfficiencyExponent, receiver.SpotSizeNormalizationExponent);

                        double atmosphericEfficiency = GetAtmosphericEfficiency(transmitterAtmosphericPressure, receiverAtmosphericPressure, wavelengthData.AtmosphericAbsorption, distanceInMeter, receiver.Vessel, transmitter.Vessel);
                        double transmitterEfficiency = distanceFacingEfficiency * atmosphericEfficiency;

                        possibleWavelengths.Add(new MicrowaveRoute(transmitterEfficiency, distanceInMeter, facingFactor, spotSize, wavelengthData));
                    }

                    var mostEfficientWavelength = possibleWavelengths.Count == 0 ? null : possibleWavelengths.FirstOrDefault(m => m.Efficiency == possibleWavelengths.Max(n => n.Efficiency));

                    if (mostEfficientWavelength != null)
                    {
                        //store in dictionary that optimal route to this transmitter is direct connection, can be replaced if better route is found
                        transmitterRouteDictionary[transmitter] = mostEfficientWavelength;
                    }
                }

                // add all transmitters that are not located on the receiving vessel
                transmittersToCheck.Add(transmitter);
            }

            //this algorithm processes relays in groups in which elements of the first group must be visible from receiver, 
            //elements from the second group must be visible by at least one element from previous group and so on...

            var relaysToCheck = new List<VesselRelayPersistence>();//relays which we have to check - all active relays will be here
            var currentRelayGroup = new List<KeyValuePair<VesselRelayPersistence, int>>();//relays which are in line of sight, and we have not yet checked what they can see. Their index in relaysToCheck is also stored

            int relayIndex = 0;
            foreach (VesselRelayPersistence relay in BeamedPowerSources.Instance.GlobalRelays.Values)
            {
                if (!relay.IsActive) continue;

                if (receiver.Vessel.HasLineOfSightWith(relay.Vessel))
                {
                    var facingFactor = ComputeFacingFactor(relay.Vessel, receiver);
                    if (facingFactor <= 0)
                        continue;

                    double distanceInMeter = ComputeDistance(receiver.Vessel, relay.Vessel);
                    double transmitterAtmosphericPressure = FlightGlobals.getStaticPressure(relay.Vessel.GetVesselPos()) * 0.01;

                    var possibleWavelengths = new List<MicrowaveRoute>();

                    foreach (var wavelengthData in relay.SupportedTransmitWavelengths)
                    {
                        if (wavelengthData.MaxWavelength < receiver.MinimumWavelength || wavelengthData.MinWavelength > receiver.MaximumWavelength)
                            continue;

                        double spotSize = ComputeSpotSize(wavelengthData, distanceInMeter, relay.Aperture, receiver.Vessel, relay.Vessel);
                        double distanceFacingEfficiency = ComputeDistanceFacingEfficiency(spotSize, facingFactor, receiver.Diameter, receiver.FacingEfficiencyExponent, receiver.SpotSizeNormalizationExponent);

                        double atmosphereEfficiency = GetAtmosphericEfficiency(transmitterAtmosphericPressure, receiverAtmosphericPressure, wavelengthData.AtmosphericAbsorption, distanceInMeter, receiver.Vessel, relay.Vessel);
                        double transmitterEfficiency = distanceFacingEfficiency * atmosphereEfficiency;

                        possibleWavelengths.Add(new MicrowaveRoute(transmitterEfficiency, distanceInMeter, facingFactor, spotSize, wavelengthData));
                    }

                    var mostEfficientWavelength = possibleWavelengths.Count == 0 ? null : possibleWavelengths.FirstOrDefault(m => m.Efficiency == possibleWavelengths.Max(n => n.Efficiency));

                    if (mostEfficientWavelength != null)
                    {
                        //store in dictionary that optimal route to this relay is direct connection, can be replaced if better route is found
                        relayRouteDictionary[relay] = mostEfficientWavelength;
                        currentRelayGroup.Add(new KeyValuePair<VesselRelayPersistence, int>(relay, relayIndex));
                    }
                }
                relaysToCheck.Add(relay);
                relayIndex++;
            }

            int hops = 0; //number of hops between relays

            //pre-compute distances and visibility thus limiting number of checks to (Nr^2)/2 + NrNt +Nr + Nt
            if (hops < maxHops && transmittersToCheck.Any())
            {
                double[,] relayToRelayDistances = new double[relaysToCheck.Count, relaysToCheck.Count];
                double[,] relayToTransmitterDistances = new double[relaysToCheck.Count, transmittersToCheck.Count];

                for (int i = 0; i < relaysToCheck.Count; i++)
                {
                    var relayToCheck = relaysToCheck[i];
                    for (int j = i + 1; j < relaysToCheck.Count; j++)
                    {
                        double visibilityAndDistance = ComputeVisibilityAndDistance(relayToCheck, relaysToCheck[j].Vessel);
                        relayToRelayDistances[i, j] = visibilityAndDistance;
                        relayToRelayDistances[j, i] = visibilityAndDistance;
                    }
                    for (int t = 0; t < transmittersToCheck.Count; t++)
                    {
                        relayToTransmitterDistances[i, t] = ComputeVisibilityAndDistance(relayToCheck, transmittersToCheck[t].Vessel);
                    }
                }

                HashSet<int> coveredRelays = new HashSet<int>();

                //runs as long as there is any relay to which we can connect and maximum number of hops have not been breached
                while (hops < maxHops && currentRelayGroup.Any())
                {
                    var nextRelayGroup = new List<KeyValuePair<VesselRelayPersistence, int>>();//will put every relay which is in line of sight of any relay from currentRelayGroup here
                    foreach (var relayEntry in currentRelayGroup) //relays visible from receiver in first iteration, then relays visible from them etc....
                    {
                        VesselRelayPersistence relayPersistence = relayEntry.Key;
                        MicrowaveRoute relayRoute = relayRouteDictionary[relayPersistence];// current best route for this relay
                        double relayRouteFacingFactor = relayRoute.FacingFactor;// it's always facing factor from the beginning of the route
                        double relayAtmosphericPressure = FlightGlobals.getStaticPressure(relayPersistence.Vessel.GetVesselPos()) / 100;

                        for (int t = 0; t < transmittersToCheck.Count; t++)//check if this relay can connect to transmitters
                        {
                            double distanceInMeter = relayToTransmitterDistances[relayEntry.Value, t];

                            //it's >0 if it can see
                            if (distanceInMeter <= 0) continue;

                            VesselMicrowavePersistence transmitterToCheck = transmittersToCheck[t];
                            double newDistance = relayRoute.Distance + distanceInMeter;// total distance from receiver by this relay to transmitter
                            double transmitterAtmosphericPressure = FlightGlobals.getStaticPressure(transmitterToCheck.Vessel.GetVesselPos()) / 100;
                            var possibleWavelengths = new List<MicrowaveRoute>();

                            foreach (var transmitterWavelengthData in transmitterToCheck.SupportedTransmitWavelengths)
                            {
                                if (transmitterWavelengthData.Wavelength.NotWithin(relayPersistence.MaximumRelayWavelength, relayPersistence.MinimumRelayWavelength))
                                    continue;

                                double spotSize = ComputeSpotSize(transmitterWavelengthData, distanceInMeter, transmitterToCheck.Aperture, relayPersistence.Vessel, transmitterToCheck.Vessel);
                                double distanceFacingEfficiency = ComputeDistanceFacingEfficiency(spotSize, 1, relayPersistence.Aperture);

                                double atmosphereEfficiency = GetAtmosphericEfficiency(transmitterAtmosphericPressure, relayAtmosphericPressure, transmitterWavelengthData.AtmosphericAbsorption, distanceInMeter, transmitterToCheck.Vessel, relayPersistence.Vessel);
                                double efficiencyTransmitterToRelay = distanceFacingEfficiency * atmosphereEfficiency;
                                double efficiencyForRoute = efficiencyTransmitterToRelay * relayRoute.Efficiency;

                                possibleWavelengths.Add(new MicrowaveRoute(efficiencyForRoute, newDistance, relayRouteFacingFactor, spotSize, transmitterWavelengthData, relayPersistence));
                            }

                            var mostEfficientWavelength = possibleWavelengths.Count == 0 ? null : possibleWavelengths.FirstOrDefault(m => m.Efficiency == possibleWavelengths.Max(n => n.Efficiency));

                            if (mostEfficientWavelength == null) continue;

                            //this will return true if there is already a route to this transmitter
                            if (transmitterRouteDictionary.TryGetValue(transmitterToCheck, out MicrowaveRoute currentOptimalRoute))
                            {
                                if (currentOptimalRoute.Efficiency < mostEfficientWavelength.Efficiency)
                                {
                                    //if route using this relay is better then replace the old route
                                    transmitterRouteDictionary[transmitterToCheck] = mostEfficientWavelength;
                                }
                            }
                            else
                            {
                                //there is no other route to this transmitter yet known so algorithm puts this one as optimal
                                transmitterRouteDictionary[transmitterToCheck] = mostEfficientWavelength;
                            }
                        }

                        for (var r = 0; r < relaysToCheck.Count; r++)
                        {
                            var nextRelay = relaysToCheck[r];
                            if (nextRelay == relayPersistence) continue;

                            double distanceToNextRelay = relayToRelayDistances[relayEntry.Value, r];
                            if (distanceToNextRelay <= 0) continue;

                            var possibleWavelengths = new List<MicrowaveRoute>();
                            var relayToNextRelayDistance = relayRoute.Distance + distanceToNextRelay;

                            foreach (var transmitterWavelengthData in relayPersistence.SupportedTransmitWavelengths)
                            {
                                if (transmitterWavelengthData.MaxWavelength < relayPersistence.MaximumRelayWavelength || transmitterWavelengthData.MinWavelength > relayPersistence.MinimumRelayWavelength)
                                    continue;

                                double spotSize = ComputeSpotSize(transmitterWavelengthData, distanceToNextRelay, relayPersistence.Aperture, nextRelay.Vessel, relayPersistence.Vessel);
                                double efficiencyByThisRelay = ComputeDistanceFacingEfficiency(spotSize, 1, relayPersistence.Aperture);
                                double efficiencyForRoute = efficiencyByThisRelay * relayRoute.Efficiency;

                                possibleWavelengths.Add(new MicrowaveRoute(efficiencyForRoute, relayToNextRelayDistance, relayRouteFacingFactor, spotSize, transmitterWavelengthData, relayPersistence));
                            }

                            var mostEfficientWavelength = possibleWavelengths.Count == 0 ? null : possibleWavelengths.FirstOrDefault(m => m.Efficiency == possibleWavelengths.Max(n => n.Efficiency));

                            if (mostEfficientWavelength != null)
                            {
                                if (relayRouteDictionary.TryGetValue(nextRelay, out MicrowaveRoute currentOptimalPredecessor))
                                //this will return true if there is already a route to next relay
                                {
                                    //if route using this relay is better
                                    if (currentOptimalPredecessor.Efficiency < mostEfficientWavelength.Efficiency)
                                    {
                                        //we put it in dictionary as optimal
                                        relayRouteDictionary[nextRelay] = mostEfficientWavelength;
                                    }
                                }
                                else //there is no other route to this relay yet known so we put this one as optimal
                                {
                                    relayRouteDictionary[nextRelay] = mostEfficientWavelength;
                                }

                                if (!coveredRelays.Contains(r))
                                {
                                    nextRelayGroup.Add(new KeyValuePair<VesselRelayPersistence, int>(nextRelay, r));
                                    //in next iteration we will check what next relay can see
                                    coveredRelays.Add(r);
                                }
                            }
                        }
                    }
                    currentRelayGroup = nextRelayGroup;
                    //we don't have to check old relays so we just replace whole List
                    hops++;
                }

            }

            //building final result
            var resultDictionary = new Dictionary<VesselMicrowavePersistence, KeyValuePair<MicrowaveRoute, IList<VesselRelayPersistence>>>();

            foreach (var transmitterEntry in transmitterRouteDictionary)
            {
                var vesselPersistence = transmitterEntry.Key;
                var microwaveRoute = transmitterEntry.Value;

                var relays = new Stack<VesselRelayPersistence>();//Last in, first out so relay visible from receiver will always be first
                VesselRelayPersistence relay = microwaveRoute.PreviousRelay;
                while (relay != null)
                {
                    relays.Push(relay);
                    relay = relayRouteDictionary[relay].PreviousRelay;
                }

                resultDictionary.Add(vesselPersistence, new KeyValuePair<MicrowaveRoute, IList<VesselRelayPersistence>>(microwaveRoute, relays.ToList()));
            }

            return resultDictionary;
        }
    }
}
