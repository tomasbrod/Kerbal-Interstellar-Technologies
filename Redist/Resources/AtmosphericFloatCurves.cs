﻿using System;
using UnityEngine;

namespace KIT.Resources
{
    public class AtmosphericFloatCurves
    {
        public AnimationCurve MassDensityAtmosphereGramPerCubeCm { get; private set; }
        public AnimationCurve ParticlesAtmosphereCubePerMeter { get; private set; }
        public FloatCurve ParticlesHydrogenCubePerMeter { get; private set; }
        public FloatCurve ParticlesHeliumPerCubePerCm { get; private set; }
        public FloatCurve HydrogenIonsPerCubeCm { get; private set; }
        public FloatCurve InterstellarDensityRatio { get; private set; }
        public FloatCurve IonSolarMinimumDayTimeCubeCm { get; private set; }

        private static AtmosphericFloatCurves _instance;

        public static AtmosphericFloatCurves Instance => _instance ?? (_instance = new AtmosphericFloatCurves());

        private AtmosphericFloatCurves()
        {
            InitializeAtmosphereParticles();
        }

        private void InitializeAtmosphereParticles()
        {
            InitializeIonSolarMinimumDayTimeCubeCm();

            InitializeDensityAtmosphereCubeCm();

            InitializeParticlesAtmosphereCurbeM();

            InitializeParticlesHydrogenCubeM();

            InitialiseParticlesHeliumCubePerMeter();

            InitializeHydrogenIonsCubeCm();

            InitializeInterstellarDensityRatio();
        }

        private void InitializeIonSolarMinimumDayTimeCubeCm()
        {
            if (IonSolarMinimumDayTimeCubeCm != null) return;

            IonSolarMinimumDayTimeCubeCm = new FloatCurve();
            IonSolarMinimumDayTimeCubeCm.Add(0, 0);
            IonSolarMinimumDayTimeCubeCm.Add(50, 0);
            IonSolarMinimumDayTimeCubeCm.Add(5.84262011243010e+001f, 1.03692663828734e+001f);
            IonSolarMinimumDayTimeCubeCm.Add(6.00542440281892e+001f, 1.72274291476994e+001f);
            IonSolarMinimumDayTimeCubeCm.Add(6.34476899181606e+001f, 4.75516240683401e+001f);
            IonSolarMinimumDayTimeCubeCm.Add(6.76498143956718e+001f, 1.05589819443358e+002f);
            IonSolarMinimumDayTimeCubeCm.Add(7.01748267991766e+001f, 2.02808715380246e+002f);
            IonSolarMinimumDayTimeCubeCm.Add(7.34640343579081e+001f, 4.18839125445748e+002f);
            IonSolarMinimumDayTimeCubeCm.Add(8.05121871190212e+001f, 1.03692663828734e+003f);
            IonSolarMinimumDayTimeCubeCm.Add(8.35172843676226e+001f, 3.19107497292355e+003f);
            IonSolarMinimumDayTimeCubeCm.Add(8.82365409325018e+001f, 1.09488896512769e+004f);
            IonSolarMinimumDayTimeCubeCm.Add(9.23723302266039e+001f, 2.18063348750633e+004f);
            IonSolarMinimumDayTimeCubeCm.Add(9.75919530763632e+001f, 5.02096739961446e+004f);
            IonSolarMinimumDayTimeCubeCm.Add(1.04055443478603e+002f, 8.34180133250633e+004f);
            IonSolarMinimumDayTimeCubeCm.Add(1.08932690303828e+002f, 9.64388379154446e+004f);
            IonSolarMinimumDayTimeCubeCm.Add(1.15088078589484e+002f, 1.07521685318989e+005f);
            IonSolarMinimumDayTimeCubeCm.Add(1.21591285375184e+002f, 1.19878184595838e+005f);
            IonSolarMinimumDayTimeCubeCm.Add(1.33256775315053e+002f, 1.33654705089111e+005f);
            IonSolarMinimumDayTimeCubeCm.Add(1.47385525613502e+002f, 1.54517039269415e+005f);
            IonSolarMinimumDayTimeCubeCm.Add(1.57146831747898e+002f, 1.78635801924574e+005f);
            IonSolarMinimumDayTimeCubeCm.Add(1.67554626722040e+002f, 2.22053024501554e+005f);
            IonSolarMinimumDayTimeCubeCm.Add(1.80295922340581e+002f, 2.76022752209027e+005f);
            IonSolarMinimumDayTimeCubeCm.Add(1.90483786105181e+002f, 2.96784315039004e+005f);
            IonSolarMinimumDayTimeCubeCm.Add(2.03099479146079e+002f, 3.55779649033949e+005f);
            IonSolarMinimumDayTimeCubeCm.Add(2.18543699066457e+002f, 4.26502184423342e+005f);
            IonSolarMinimumDayTimeCubeCm.Add(2.35162338192397e+002f, 4.93075456902875e+005f);
            IonSolarMinimumDayTimeCubeCm.Add(2.53044702456961e+002f, 5.30163041156278e+005f);
            IonSolarMinimumDayTimeCubeCm.Add(2.62489532691202e+002f, 5.30163041156278e+005f);
            IonSolarMinimumDayTimeCubeCm.Add(2.82449928880915e+002f, 4.75516240683401e+005f);
            IonSolarMinimumDayTimeCubeCm.Add(3.06725324600799e+002f, 3.68917395443824e+005f);
            IonSolarMinimumDayTimeCubeCm.Add(3.55147363347329e+002f, 2.06519293147963e+005f);
            IonSolarMinimumDayTimeCubeCm.Add(4.26562128488569e+002f, 9.64388379154446e+004f);
            IonSolarMinimumDayTimeCubeCm.Add(5.07665101723574e+002f, 5.59798197912328e+004f);
            IonSolarMinimumDayTimeCubeCm.Add(5.56370583524392e+002f, 4.34305446331671e+004f);
            IonSolarMinimumDayTimeCubeCm.Add(6.44203559128576e+002f, 3.02214265517838e+004f);
            IonSolarMinimumDayTimeCubeCm.Add(7.12506211708764e+002f, 2.43123406199979e+004f);
            IonSolarMinimumDayTimeCubeCm.Add(8.17464553275307e+002f, 1.81904113317882e+004f);
            IonSolarMinimumDayTimeCubeCm.Add(1000, 1.17724306767693e+004f);
        }

        private void InitializeInterstellarDensityRatio()
        {
            if (InterstellarDensityRatio != null) return;

            InterstellarDensityRatio = new FloatCurve();
            InterstellarDensityRatio.Add(0, 0);
            InterstellarDensityRatio.Add(69, 0);
            InterstellarDensityRatio.Add(70, 0.05f); // <== Termination Shock
            InterstellarDensityRatio.Add(71, 0.1f);
            InterstellarDensityRatio.Add(72, 0.2f);
            InterstellarDensityRatio.Add(73, 0.3f);
            InterstellarDensityRatio.Add(74, 0.4f);
            InterstellarDensityRatio.Add(75, 0.5f);
            InterstellarDensityRatio.Add(76, 0.6f);
            InterstellarDensityRatio.Add(77, 0.7f);
            InterstellarDensityRatio.Add(78, 0.8f);
            InterstellarDensityRatio.Add(79, 0.9f);
            InterstellarDensityRatio.Add(80, 1);  // <== Helio Pause
            InterstellarDensityRatio.Add(81, 2);
            InterstellarDensityRatio.Add(82, 4);
            InterstellarDensityRatio.Add(83, 8);
            InterstellarDensityRatio.Add(84, 16);
            InterstellarDensityRatio.Add(85, 32);
            InterstellarDensityRatio.Add(86, 64);
            InterstellarDensityRatio.Add(87, 100); //<== Hydrogen Wall
            InterstellarDensityRatio.Add(88, 1 + 33.3f);
            InterstellarDensityRatio.Add(89, 1 + 11.1f);
            InterstellarDensityRatio.Add(90, 1 + 3.7f);
            InterstellarDensityRatio.Add(91, 1 + 1.234f);
            InterstellarDensityRatio.Add(92, 1 + 0.412f);
            InterstellarDensityRatio.Add(93, 1 + 0.137f);
            InterstellarDensityRatio.Add(94, 1 + 0.05f);
            InterstellarDensityRatio.Add(95, 1 + 0.015f);
            InterstellarDensityRatio.Add(96, 1 + 0.005f);
            InterstellarDensityRatio.Add(97, 1 + 0.0017f);
            InterstellarDensityRatio.Add(98, 1 + 5.65e-4f);
            InterstellarDensityRatio.Add(99, 1 + 1.88e-4f); // <== Bow Wave
            InterstellarDensityRatio.Add(100, 1);
            InterstellarDensityRatio.Add(101, 1);
        }

        private void InitializeHydrogenIonsCubeCm()
        {
            if (HydrogenIonsPerCubeCm != null) return;

            HydrogenIonsPerCubeCm = new FloatCurve();
            HydrogenIonsPerCubeCm.Add(0, 0);
            HydrogenIonsPerCubeCm.Add(284, 0);
            HydrogenIonsPerCubeCm.Add(285, 1.00e+8f);
            HydrogenIonsPerCubeCm.Add(299, 2.49e+8f);
            HydrogenIonsPerCubeCm.Add(312, 4.40e+8f);
            HydrogenIonsPerCubeCm.Add(334, 6.34e+8f);
            HydrogenIonsPerCubeCm.Add(359, 8.83e+8f);
            HydrogenIonsPerCubeCm.Add(377, 1.11e+9f);
            HydrogenIonsPerCubeCm.Add(396, 1.26e+9f);
            HydrogenIonsPerCubeCm.Add(411, 1.36e+9f);
            HydrogenIonsPerCubeCm.Add(437, 1.51e+9f);
            HydrogenIonsPerCubeCm.Add(464, 1.65e+9f);
            HydrogenIonsPerCubeCm.Add(504, 1.78e+9f);
            HydrogenIonsPerCubeCm.Add(536, 1.86e+9f);
            HydrogenIonsPerCubeCm.Add(588, 2.00e+9f);
            HydrogenIonsPerCubeCm.Add(641, 2.20e+9f);
            HydrogenIonsPerCubeCm.Add(678, 2.40e+9f);
            HydrogenIonsPerCubeCm.Add(715, 2.64e+9f);
            HydrogenIonsPerCubeCm.Add(754, 2.94e+9f);
            HydrogenIonsPerCubeCm.Add(793, 3.33e+9f);
            HydrogenIonsPerCubeCm.Add(829, 3.76e+9f);
            HydrogenIonsPerCubeCm.Add(864, 4.26e+9f);
            HydrogenIonsPerCubeCm.Add(904, 4.97e+9f);
            HydrogenIonsPerCubeCm.Add(939, 5.72e+9f);
            HydrogenIonsPerCubeCm.Add(968, 6.43e+9f);
            HydrogenIonsPerCubeCm.Add(992, 7.07e+9f);
            HydrogenIonsPerCubeCm.Add(1010, 7.61e+9f);
            HydrogenIonsPerCubeCm.Add(1040, 7.95e+9f);
            HydrogenIonsPerCubeCm.Add(1070, 8.15e+9f);
            HydrogenIonsPerCubeCm.Add(1100, 8.13e+9f);
            HydrogenIonsPerCubeCm.Add(1140, 7.89e+9f);
            HydrogenIonsPerCubeCm.Add(1170, 7.57e+9f);
            HydrogenIonsPerCubeCm.Add(1190, 7.21e+9f);
            HydrogenIonsPerCubeCm.Add(1240, 6.46e+9f);
        }

        private void InitialiseParticlesHeliumCubePerMeter()
        {
            if (ParticlesHeliumPerCubePerCm != null) return;

            ParticlesHeliumPerCubePerCm = new FloatCurve();

            ParticlesHeliumPerCubePerCm.Add(0.0f, 1.462E+14f);
            ParticlesHeliumPerCubePerCm.Add(10, 4.503E+13f);
            ParticlesHeliumPerCubePerCm.Add(20, 9.351E+12f);
            ParticlesHeliumPerCubePerCm.Add(30, 1.757E+12f);
            ParticlesHeliumPerCubePerCm.Add(40, 3.558E+11f);
            ParticlesHeliumPerCubePerCm.Add(50, 9.383E+10f);
            ParticlesHeliumPerCubePerCm.Add(60, 2.611E+10f);
            ParticlesHeliumPerCubePerCm.Add(70, 6.625E+09f);
            ParticlesHeliumPerCubePerCm.Add(80, 1.626E+09f);
            ParticlesHeliumPerCubePerCm.Add(90, 3.918E+08f);
            ParticlesHeliumPerCubePerCm.Add(100, 1.087E+08f);
            ParticlesHeliumPerCubePerCm.Add(110, 4.820E+07f);
            ParticlesHeliumPerCubePerCm.Add(120, 3.060E+07f);
            ParticlesHeliumPerCubePerCm.Add(130, 3.344E+07f);
            ParticlesHeliumPerCubePerCm.Add(140, 3.566E+07f);
            ParticlesHeliumPerCubePerCm.Add(150, 3.257E+07f);
            ParticlesHeliumPerCubePerCm.Add(160, 2.933E+07f);
            ParticlesHeliumPerCubePerCm.Add(170, 2.675E+07f);
            ParticlesHeliumPerCubePerCm.Add(180, 2.467E+07f);
            ParticlesHeliumPerCubePerCm.Add(190, 2.296E+07f);
            ParticlesHeliumPerCubePerCm.Add(200, 2.150E+07f);
            ParticlesHeliumPerCubePerCm.Add(210, 2.021E+07f);
            ParticlesHeliumPerCubePerCm.Add(220, 1.909E+07f);
            ParticlesHeliumPerCubePerCm.Add(230, 1.809E+07f);
            ParticlesHeliumPerCubePerCm.Add(240, 1.717E+07f);
            ParticlesHeliumPerCubePerCm.Add(250, 1.633E+07f);
            ParticlesHeliumPerCubePerCm.Add(260, 1.555E+07f);
            ParticlesHeliumPerCubePerCm.Add(270, 1.483E+07f);
            ParticlesHeliumPerCubePerCm.Add(280, 1.415E+07f);
            ParticlesHeliumPerCubePerCm.Add(290, 1.352E+07f);
            ParticlesHeliumPerCubePerCm.Add(300, 1.292E+07f);
            ParticlesHeliumPerCubePerCm.Add(400, 8.365E+06f);
            ParticlesHeliumPerCubePerCm.Add(500, 5.518E+06f);
            ParticlesHeliumPerCubePerCm.Add(600, 3.687E+06f);
            ParticlesHeliumPerCubePerCm.Add(700, 2.491E+06f);
            ParticlesHeliumPerCubePerCm.Add(800, 1.702E+06f);
            ParticlesHeliumPerCubePerCm.Add(900, 1.175E+06f);
            ParticlesHeliumPerCubePerCm.Add(1000, 8.196E+05f);
        }

        private void InitializeParticlesHydrogenCubeM()
        {
            if (ParticlesHydrogenCubePerMeter != null) return;

            ParticlesHydrogenCubePerMeter = new FloatCurve();

            ParticlesHydrogenCubePerMeter.Add(0, 0);
            ParticlesHydrogenCubePerMeter.Add(72.5f, 1.747e+9f);
            ParticlesHydrogenCubePerMeter.Add(73.0f, 2.872e+9f);
            ParticlesHydrogenCubePerMeter.Add(73.5f, 5.154e+9f);
            ParticlesHydrogenCubePerMeter.Add(74.0f, 1.009e+10f);
            ParticlesHydrogenCubePerMeter.Add(74.5f, 2.138e+10f);
            ParticlesHydrogenCubePerMeter.Add(75.0f, 4.836e+10f);
            ParticlesHydrogenCubePerMeter.Add(75.5f, 1.144e+11f);
            ParticlesHydrogenCubePerMeter.Add(76.0f, 2.760e+11f);
            ParticlesHydrogenCubePerMeter.Add(76.5f, 6.612e+11f);
            ParticlesHydrogenCubePerMeter.Add(77.0f, 1.531e+12f);
            ParticlesHydrogenCubePerMeter.Add(77.5f, 3.351e+12f);
            ParticlesHydrogenCubePerMeter.Add(78.0f, 6.813e+12f);
            ParticlesHydrogenCubePerMeter.Add(78.5f, 1.274e+13f);
            ParticlesHydrogenCubePerMeter.Add(79.0f, 2.180e+13f);
            ParticlesHydrogenCubePerMeter.Add(79.5f, 3.420e+13f);
            ParticlesHydrogenCubePerMeter.Add(80.0f, 4.945e+13f);
            ParticlesHydrogenCubePerMeter.Add(80.5f, 6.637e+13f);
            ParticlesHydrogenCubePerMeter.Add(81.0f, 8.346e+13f);
            ParticlesHydrogenCubePerMeter.Add(81.5f, 9.920e+13f);
            ParticlesHydrogenCubePerMeter.Add(82.0f, 1.124e+14f);
            ParticlesHydrogenCubePerMeter.Add(82.5f, 1.225e+14f);
            ParticlesHydrogenCubePerMeter.Add(83.0f, 1.292e+14f);
            ParticlesHydrogenCubePerMeter.Add(83.5f, 1.328e+14f);
            ParticlesHydrogenCubePerMeter.Add(84.0f, 1.335e+14f);
            ParticlesHydrogenCubePerMeter.Add(84.5f, 1.320e+14f);
            ParticlesHydrogenCubePerMeter.Add(85.0f, 1.287e+14f);
            ParticlesHydrogenCubePerMeter.Add(85.5f, 1.241e+14f);
            ParticlesHydrogenCubePerMeter.Add(86.0f, 1.187e+14f);
            ParticlesHydrogenCubePerMeter.Add(87, 1.066e+13f);
            ParticlesHydrogenCubePerMeter.Add(88, 9.426e+13f);
            ParticlesHydrogenCubePerMeter.Add(89, 8.257e+13f);
            ParticlesHydrogenCubePerMeter.Add(90, 7.203e+13f);
            ParticlesHydrogenCubePerMeter.Add(91, 6.276e+13f);
            ParticlesHydrogenCubePerMeter.Add(92, 5.474e+13f);
            ParticlesHydrogenCubePerMeter.Add(93, 4.786e+13f);
            ParticlesHydrogenCubePerMeter.Add(94, 4.198e+13f);
            ParticlesHydrogenCubePerMeter.Add(95, 3.698e+13f);
            ParticlesHydrogenCubePerMeter.Add(96, 3.272e+13f);
            ParticlesHydrogenCubePerMeter.Add(97, 2.909e+13f);
            ParticlesHydrogenCubePerMeter.Add(98, 2.598e+13f);
            ParticlesHydrogenCubePerMeter.Add(99, 2.332e+13f);
            ParticlesHydrogenCubePerMeter.Add(100, 2.101e+13f);
            ParticlesHydrogenCubePerMeter.Add(101, 1.901e+13f);
            ParticlesHydrogenCubePerMeter.Add(102, 1.726e+13f);
            ParticlesHydrogenCubePerMeter.Add(103, 1.572e+13f);
            ParticlesHydrogenCubePerMeter.Add(104, 1.435e+13f);
            ParticlesHydrogenCubePerMeter.Add(105, 1.313e+13f);
            ParticlesHydrogenCubePerMeter.Add(106, 1.203e+13f);
            ParticlesHydrogenCubePerMeter.Add(107, 1.104e+13f);
            ParticlesHydrogenCubePerMeter.Add(108, 1.013e+13f);
            ParticlesHydrogenCubePerMeter.Add(109, 9.299e+13f);
            ParticlesHydrogenCubePerMeter.Add(110, 8.534e+12f);
            ParticlesHydrogenCubePerMeter.Add(111, 7.827e+12f);
            ParticlesHydrogenCubePerMeter.Add(112, 7.173e+12f);
            ParticlesHydrogenCubePerMeter.Add(113, 6.569e+12f);
            ParticlesHydrogenCubePerMeter.Add(114, 6.012e+12f);
            ParticlesHydrogenCubePerMeter.Add(115, 5.500e+12f);
            ParticlesHydrogenCubePerMeter.Add(120, 3.551e+12f);
            ParticlesHydrogenCubePerMeter.Add(125, 2.477e+12f);
            ParticlesHydrogenCubePerMeter.Add(130, 1.805e+12f);
            ParticlesHydrogenCubePerMeter.Add(140, 1.029e+12f);
            ParticlesHydrogenCubePerMeter.Add(150, 6.468e+11f);
            ParticlesHydrogenCubePerMeter.Add(160, 4.485e+11f);
            ParticlesHydrogenCubePerMeter.Add(170, 3.400e+11f);
            ParticlesHydrogenCubePerMeter.Add(180, 2.774e+11f);
            ParticlesHydrogenCubePerMeter.Add(190, 2.394e+11f);
            ParticlesHydrogenCubePerMeter.Add(200, 2.154e+11f);
            ParticlesHydrogenCubePerMeter.Add(210, 1.995e+11f);
            ParticlesHydrogenCubePerMeter.Add(220, 1.887e+11f);
            ParticlesHydrogenCubePerMeter.Add(230, 1.809e+11f);
            ParticlesHydrogenCubePerMeter.Add(240, 1.752e+11f);
            ParticlesHydrogenCubePerMeter.Add(250, 1.707e+11f);
            ParticlesHydrogenCubePerMeter.Add(300, 1.569e+11f);
            ParticlesHydrogenCubePerMeter.Add(350, 1.477e+11f);
            ParticlesHydrogenCubePerMeter.Add(400, 1.399e+11f);
            ParticlesHydrogenCubePerMeter.Add(450, 1.327e+11f);
            ParticlesHydrogenCubePerMeter.Add(500, 1.260e+11f);
            ParticlesHydrogenCubePerMeter.Add(550, 1.198e+11f);
            ParticlesHydrogenCubePerMeter.Add(600, 1.139e+11f);
            ParticlesHydrogenCubePerMeter.Add(650, 1.085e+11f);
            ParticlesHydrogenCubePerMeter.Add(700, 1.033e+11f);
            ParticlesHydrogenCubePerMeter.Add(750, 9.848e+10f);
            ParticlesHydrogenCubePerMeter.Add(800, 9.393e+10f);
            ParticlesHydrogenCubePerMeter.Add(850, 8.965e+10f);
            ParticlesHydrogenCubePerMeter.Add(900, 8.562e+10f);
            ParticlesHydrogenCubePerMeter.Add(950, 8.182e+10f);
            ParticlesHydrogenCubePerMeter.Add(1000, 7.824e+10f);
        }

        private void InitializeParticlesAtmosphereCurbeM()
        {
            if (ParticlesAtmosphereCubePerMeter != null) return;

            ParticlesAtmosphereCubePerMeter = new AnimationCurve();

            ParticlesAtmosphereCubePerMeter.AddKey(0, 2.55e+25f);
            ParticlesAtmosphereCubePerMeter.AddKey(2, 2.09e+25f);
            ParticlesAtmosphereCubePerMeter.AddKey(4, 1.70e+25f);
            ParticlesAtmosphereCubePerMeter.AddKey(6, 1.37e+25f);
            ParticlesAtmosphereCubePerMeter.AddKey(8, 1.09e+25f);
            ParticlesAtmosphereCubePerMeter.AddKey(10, 8.60e+24f);
            ParticlesAtmosphereCubePerMeter.AddKey(12, 6.49e+24f);
            ParticlesAtmosphereCubePerMeter.AddKey(14, 4.74e+24f);
            ParticlesAtmosphereCubePerMeter.AddKey(16, 3.46e+24f);
            ParticlesAtmosphereCubePerMeter.AddKey(18, 2.53e+24f);
            ParticlesAtmosphereCubePerMeter.AddKey(20, 1.85e+24f);
            ParticlesAtmosphereCubePerMeter.AddKey(22, 1.34e+24f);
            ParticlesAtmosphereCubePerMeter.AddKey(24, 9.76e+23f);
            ParticlesAtmosphereCubePerMeter.AddKey(26, 7.12e+23f);
            ParticlesAtmosphereCubePerMeter.AddKey(28, 5.21e+23f);
            ParticlesAtmosphereCubePerMeter.AddKey(30, 3.83e+23f);
            ParticlesAtmosphereCubePerMeter.AddKey(32, 2.81e+23f);
            ParticlesAtmosphereCubePerMeter.AddKey(34, 2.06e+23f);
            ParticlesAtmosphereCubePerMeter.AddKey(36, 1.51e+23f);
            ParticlesAtmosphereCubePerMeter.AddKey(38, 1.12e+23f);
            ParticlesAtmosphereCubePerMeter.AddKey(40, 8.31e+22f);
            ParticlesAtmosphereCubePerMeter.AddKey(42, 6.23e+22f);
            ParticlesAtmosphereCubePerMeter.AddKey(44, 4.70e+22f);
            ParticlesAtmosphereCubePerMeter.AddKey(46, 3.56e+22f);
            ParticlesAtmosphereCubePerMeter.AddKey(48, 2.74e+22f);
            ParticlesAtmosphereCubePerMeter.AddKey(50, 2.14e+22f);
            ParticlesAtmosphereCubePerMeter.AddKey(52, 1.68e+22f);
            ParticlesAtmosphereCubePerMeter.AddKey(54, 1.33e+22f);
            ParticlesAtmosphereCubePerMeter.AddKey(56, 1.05e+22f);
            ParticlesAtmosphereCubePerMeter.AddKey(58, 8.24e+21f);
            ParticlesAtmosphereCubePerMeter.AddKey(60, 6.44e+21f);
            ParticlesAtmosphereCubePerMeter.AddKey(65, 3.39e+21f);
            ParticlesAtmosphereCubePerMeter.AddKey(70, 1.72e+21f);
            ParticlesAtmosphereCubePerMeter.AddKey(75, 8.30e+20f);
            ParticlesAtmosphereCubePerMeter.AddKey(80, 3.84e+20f);
            ParticlesAtmosphereCubePerMeter.AddKey(85, 1.71e+20f);
            ParticlesAtmosphereCubePerMeter.AddKey(90, 7.12e+19f);
            ParticlesAtmosphereCubePerMeter.AddKey(95, 2.92e+19f);
            ParticlesAtmosphereCubePerMeter.AddKey(100, 1.19e+19f);
            ParticlesAtmosphereCubePerMeter.AddKey(110, 2.45e+18f);
            ParticlesAtmosphereCubePerMeter.AddKey(120, 5.11e+17f);
            ParticlesAtmosphereCubePerMeter.AddKey(140, 9.32e+16f);
            ParticlesAtmosphereCubePerMeter.AddKey(160, 3.16e+16f);
            ParticlesAtmosphereCubePerMeter.AddKey(180, 1.40e+16f);
            ParticlesAtmosphereCubePerMeter.AddKey(200, 7.18e+15f);
            ParticlesAtmosphereCubePerMeter.AddKey(300, 6.51e+14f);
            ParticlesAtmosphereCubePerMeter.AddKey(400, 9.13e+13f);
            ParticlesAtmosphereCubePerMeter.AddKey(500, 2.19e+13f);
            ParticlesAtmosphereCubePerMeter.AddKey(600, 4.89e+12f);
            ParticlesAtmosphereCubePerMeter.AddKey(700, 1.14e+12f);
            ParticlesAtmosphereCubePerMeter.AddKey(800, 5.86e+11f);
            ParticlesAtmosphereCubePerMeter.AddKey(1000, 2.06e+11f);

            // smooth the tangents
            for (int i = 0; i < ParticlesAtmosphereCubePerMeter.keys.Length; i++)
            {
                ParticlesAtmosphereCubePerMeter.SmoothTangents(i, 0);
            }
        }

        private void InitializeDensityAtmosphereCubeCm()
        {
            if (MassDensityAtmosphereGramPerCubeCm != null) return;

            MassDensityAtmosphereGramPerCubeCm = new AnimationCurve();

            MassDensityAtmosphereGramPerCubeCm.AddKey(000, 1.340E-03f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(001, 1.195E-03f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(002, 1.072E-03f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(003, 9.649E-04f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(004, 8.681E-04f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(005, 7.790E-04f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(006, 6.959E-04f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(007, 6.179E-04f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(008, 5.446E-04f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(009, 4.762E-04f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(010, 4.128E-04f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(012, 3.035E-04f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(014, 2.203E-04f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(016, 1.605E-04f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(018, 1.175E-04f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(020, 8.573E-05f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(025, 3.756E-05f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(030, 1.611E-05f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(035, 7.028E-06f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(040, 3.262E-06f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(045, 1.627E-06f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(050, 8.602E-07f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(055, 4.593E-07f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(060, 1.214E-07f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(065, 6.017E-08f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(070, 6.017E-08f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(075, 2.943E-08f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(080, 1.439E-08f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(085, 6.826E-09f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(090, 3.080E-09f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(095, 1.316E-09f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(100, 5.357E-10f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(105, 2.133E-10f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(110, 8.711E-11f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(115, 3.780E-11f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(120, 1.844E-11f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(130, 7.383E-12f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(140, 3.781E-12f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(150, 2.185E-12f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(160, 1.364E-12f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(170, 8.974E-13f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(180, 6.145E-13f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(190, 4.333E-13f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(200, 3.127E-13f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(210, 2.300E-13f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(220, 1.718E-13f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(230, 1.300E-13f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(240, 9.954E-14f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(250, 7.698E-14f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(260, 6.007E-14f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(270, 4.725E-14f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(280, 3.744E-14f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(290, 2.987E-14f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(300, 2.397E-14f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(310, 1.934E-14f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(320, 1.569E-14f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(330, 1.278E-14f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(340, 1.046E-14f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(350, 8.594E-15f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(400, 3.377E-15f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(450, 1.412E-15f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(500, 6.205E-16f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(550, 2.854E-16f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(600, 1.385E-16f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(650, 7.176E-17f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(700, 4.031E-17f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(750, 2.477E-17f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(800, 1.660E-17f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(850, 1.197E-17f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(900, 9.114E-18f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(950, 7.211E-18f);
            MassDensityAtmosphereGramPerCubeCm.AddKey(1000, 5.849E-18f);

            // smooth the tangents
            for (int i = 0; i < MassDensityAtmosphereGramPerCubeCm.keys.Length; i++)
            {
                 MassDensityAtmosphereGramPerCubeCm.SmoothTangents(i, 0);
            }
        }

        public static double CalculateCurrentAtmosphereConcentration(Vessel vessel)
        {
            if (!vessel.mainBody.atmosphere || vessel.mainBody.atmosphereDepth <= 0)
                return 0;

            var comparableEarthAltitudeInKm = vessel.altitude / vessel.mainBody.atmosphereDepth * 85;
            var atmosphereMultiplier = vessel.altitude > vessel.mainBody.atmosphereDepth ? 1
                : Math.Max(0, vessel.altitude / vessel.mainBody.atmosphereDepth) * (vessel.mainBody.atmospherePressureSeaLevel / GameConstants.EarthAtmospherePressureAtSeaLevel);

            var atmosphereParticlesPerCubM = comparableEarthAltitudeInKm > 64000 ? 0
                : comparableEarthAltitudeInKm <= 1000
                    ? Math.Max(0, Instance.ParticlesAtmosphereCubePerMeter.Evaluate((float)comparableEarthAltitudeInKm))
                    : 2.06e+11f * (1 / (Math.Pow(20, (comparableEarthAltitudeInKm - 1000) / 1000)));

			var atmosphereConcentration = atmosphereMultiplier * atmosphereParticlesPerCubM * vessel.obt_speed / PhysicsGlobals.AvogadroConstant;

            return float.IsInfinity((float)atmosphereConcentration) ? 0 : atmosphereConcentration;
        }

        public static double GetAtmosphericGasDensityKgPerCubicMeter(Vessel vessel)
        {
            return GetAtmosphericGasDensityKgPerCubicMeter(vessel.mainBody, vessel.altitude);
        }

        public static double GetAtmosphericGasDensityKgPerCubicMeter(CelestialBody celestialBody, double altitude)
        {
            if (!celestialBody.atmosphere)
                return 0;

            var comparableEarthAltitudeInKm = altitude / celestialBody.atmosphereDepth * 85;
            var atmosphereMultiplier = altitude > celestialBody.atmosphereDepth ? 1
                : Math.Max(0, altitude / celestialBody.atmosphereDepth) * (celestialBody.atmospherePressureSeaLevel / GameConstants.EarthAtmospherePressureAtSeaLevel);

            var atmosphericDensityGramPerSquareCm = comparableEarthAltitudeInKm > 64000 ? 0
                : comparableEarthAltitudeInKm <= 1000
                    ? Math.Max(0, Instance.MassDensityAtmosphereGramPerCubeCm.Evaluate((float)comparableEarthAltitudeInKm))
                    : 5.849E-18f * (1 / (Math.Pow(20, (comparableEarthAltitudeInKm - 1000) / 1000)));

            var atmosphereConcentration = 1e+3 * atmosphereMultiplier * atmosphericDensityGramPerSquareCm;

            return float.IsInfinity((float)atmosphereConcentration) ? 0 : atmosphereConcentration;
        }
    }
}
