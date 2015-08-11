﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using AnotherSc2Hack.Classes.BackEnds;
using AnotherSc2Hack.Classes.DataStructures.Preference;
using PredefinedTypes;
using Utilities.ExtensionMethods;

namespace AnotherSc2Hack.Classes.FrontEnds.Rendering
{
    public class ProductionRenderer : BaseRenderer
    {
        /* Size for Unit/ Productionsize */
        private int _iProdPanelWidth;
        private int _iProdPanelWidthWithoutName;
        private int _iProdPosAfterName;

        public ProductionRenderer(GameInfo gInformation, PreferenceManager pSettings, Process sc2Process)
            : base(gInformation, pSettings, sc2Process)
        {
            IsHiddenChanged += ProductionRenderer_IsHiddenChanged;
        }

        /// <summary>
        ///     Draws the panelspecific data.
        /// </summary>
        /// <param name="g"></param>
        protected override void Draw(BufferedGraphics g)
        {
            try
            {
                if (!GInformation.Gameinfo.IsIngame)
                    return;

                Opacity = PSettings.PreferenceAll.OverlayProduction.Opacity;

                /* Add the feature that the window (in case you have all races and more units than your display can hold) 
                 * will split the units to the next line */

                /* Count all included units */
                //CountUnits();
                CountUnits();

                var iHavetoadd = 0; //Adds +1 when a neutral player is on position 0
                var iSize = PSettings.PreferenceAll.OverlayProduction.PictureSize;
                var iPosY = 0;
                var iPosX = 0;
                var iPosYName = 0;

                var iMaximumWidth = 0;
                var fsize = (float) (iSize/3.5);
                var iPosXAfterName = (int) (fsize*14);
                    /* We take the fontsize times the (probably) with a common String- lenght*/
                var iPosYInitial = iPosY;

                var iWidthUnits = 0;
                var iWidthBuildings = 0;
                var iWidthUpgrades = 0;

                if (fsize < 1)
                    fsize = 1;

                var fStringFont = new Font(PSettings.PreferenceAll.OverlayProduction.FontName, fsize, FontStyle.Regular);


                /* Define the startposition of the picture drawing
                 * using the longest name as reference */
                var strPlayerName = string.Empty;
                for (var i = 0; i < GInformation.Player.Count; i++)
                {
                    var strTemp = (GInformation.Player[i].ClanTag.StartsWith("\0") ||
                                   PSettings.PreferenceAll.OverlayProduction.RemoveClanTag)
                        ? GInformation.Player[i].Name
                        : "[" + GInformation.Player[i].ClanTag + "] " + GInformation.Player[i].Name;

                    if (strTemp.Length >= strPlayerName.Length)
                        strPlayerName = strTemp;
                }

                iPosXAfterName = TextRenderer.MeasureText(strPlayerName, fStringFont).Width + 20;


                /* Fix the size of the icons to 25x25 */
                for (var i = 0; i < GInformation.Player.Count; i++)
                {
                    var clPlayercolor = GInformation.Player[i].Color;

                    var tmpPlayer = GInformation.Player[i];

                    #region Teamcolor

                    if (GInformation.Gameinfo.IsTeamcolor)
                    {
                        if (Player.LocalPlayer != null)
                        {
                            if (GInformation.Player[i] == Player.LocalPlayer)
                                clPlayercolor = Color.Green;

                            else if (GInformation.Player[i].Team ==
                                     Player.LocalPlayer.Team &&
                                     GInformation.Player[i] != Player.LocalPlayer)
                                clPlayercolor = Color.Yellow;

                            else
                                clPlayercolor = Color.Red;
                        }
                    }

                    #endregion

                    #region Exceptions - Throw out players

                    /* Remove Ai - Works */
                    if (PSettings.PreferenceAll.OverlayProduction.RemoveAi)
                    {
                        if (GInformation.Player[i].Type == PlayerType.Ai)
                        {
                            continue;
                        }
                    }

                    /* Remove Neutral - Works */
                    if (PSettings.PreferenceAll.OverlayProduction.RemoveNeutral)
                    {
                        if (tmpPlayer.Type == PlayerType.Neutral)
                        {
                            continue;
                        }
                    }

                    /* Remove Localplayer - Works */
                    if (PSettings.PreferenceAll.OverlayProduction.RemoveLocalplayer)
                    {
                        if (tmpPlayer == Player.LocalPlayer)
                            continue;
                    }

                    /* Remove Allie - Works */
                    if (PSettings.PreferenceAll.OverlayProduction.RemoveAllie)
                    {
                        if (tmpPlayer.Team == Player.LocalPlayer.Team &&
                            tmpPlayer != Player.LocalPlayer)
                        {
                            continue;
                        }
                    }

                    if (GInformation.Player[i].Type.Equals(PlayerType.Hostile))
                        continue;

                    if (GInformation.Player[i].Type.Equals(PlayerType.Observer))
                        continue;

                    if (GInformation.Player[i].Type.Equals(PlayerType.Referee))
                        continue;

                    #endregion

                    if (GInformation.Player[i].Name.Length <= 0 ||
                        GInformation.Player[i].Name.StartsWith("\0"))
                        continue;

                    if (CheckIfGameheart(GInformation.Player[i]))
                        continue;


                    iPosX = 0;

                    /* Draw Name in front of Icons */
                    var strName = (GInformation.Player[i].ClanTag.StartsWith("\0") ||
                                   PSettings.PreferenceAll.OverlayProduction.RemoveClanTag)
                        ? GInformation.Player[i].Name
                        : "[" + GInformation.Player[i].ClanTag + "] " + GInformation.Player[i].Name;

                    //Name gets drawn after the icon- drawing is done!


                    iPosX = iPosXAfterName;

                    #region Draw Units

                    if (PSettings.PreferenceAll.OverlayProduction.ShowUnits)
                    {
                        /* Terran */
                        Helper_DrawUnitsProduction(_lTuScv, i, ref iPosX, iPosY, iSize, _imgTuScv, g, clPlayercolor,
                            fStringFont, false);
                        Helper_DrawUnitsProduction(_lTuMarine, i, ref iPosX, iPosY, iSize, _imgTuMarine, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTuMarauder, i, ref iPosX, iPosY, iSize, _imgTuMarauder, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTuReaper, i, ref iPosX, iPosY, iSize, _imgTuReaper, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTuGhost, i, ref iPosX, iPosY, iSize, _imgTuGhost, g, clPlayercolor,
                            fStringFont, false);
                        Helper_DrawUnitsProduction(_lTuMule, i, ref iPosX, iPosY, iSize, _imgTuMule, g, clPlayercolor,
                            fStringFont, false);
                        Helper_DrawUnitsProduction(_lTuHellion, i, ref iPosX, iPosY, iSize, _imgTuHellion, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTuHellbat, i, ref iPosX, iPosY, iSize, _imgTuHellbat, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTuWidowMine, i, ref iPosX, iPosY, iSize, _imgTuWidowMine, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTuSiegetank, i, ref iPosX, iPosY, iSize, _imgTuSiegetank, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTuThor, i, ref iPosX, iPosY, iSize, _imgTuThor, g, clPlayercolor,
                            fStringFont, false);
                        Helper_DrawUnitsProduction(_lTuMedivac, i, ref iPosX, iPosY, iSize, _imgTuMedivac, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTuBanshee, i, ref iPosX, iPosY, iSize, _imgTuBanshee, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTuViking, i, ref iPosX, iPosY, iSize, _imgTuViking, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTuRaven, i, ref iPosX, iPosY, iSize, _imgTuRaven, g, clPlayercolor,
                            fStringFont, false);
                        Helper_DrawUnitsProduction(_lTuBattlecruiser, i, ref iPosX, iPosY, iSize, _imgTuBattlecruiser, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTbAutoTurret, i, ref iPosX, iPosY, iSize, _imgTbAutoTurret, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTuPointDefenseDrone, i, ref iPosX, iPosY, iSize,
                            _imgTuPointDefenseDrone, g, clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTuNuke, i, ref iPosX, iPosY, iSize,
                            _imgTuNuke, g, clPlayercolor, fStringFont, false);


                        /* Protoss */
                        Helper_DrawUnitsProduction(_lPuProbe, i, ref iPosX, iPosY, iSize, _imgPuProbe, g, clPlayercolor,
                            fStringFont, false);
                        Helper_DrawUnitsProduction(_lPuZealot, i, ref iPosX, iPosY, iSize, _imgPuZealot, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPuStalker, i, ref iPosX, iPosY, iSize, _imgPuStalker, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPuSentry, i, ref iPosX, iPosY, iSize, _imgPuSentry, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPuDt, i, ref iPosX, iPosY, iSize, _imgPuDarkTemplar, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPuHt, i, ref iPosX, iPosY, iSize, _imgPuHighTemplar, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPuArchon, i, ref iPosX, iPosY, iSize, _imgPuArchon, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPuImmortal, i, ref iPosX, iPosY, iSize, _imgPuImmortal, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPuColossus, i, ref iPosX, iPosY, iSize, _imgPuColossus, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPuObserver, i, ref iPosX, iPosY, iSize, _imgPuObserver, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPuWarpprism, i, ref iPosX, iPosY, iSize, _imgPuWapprism, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPuPhoenix, i, ref iPosX, iPosY, iSize, _imgPuPhoenix, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPuVoidray, i, ref iPosX, iPosY, iSize, _imgPuVoidray, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPuOracle, i, ref iPosX, iPosY, iSize, _imgPuOracle, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPuCarrier, i, ref iPosX, iPosY, iSize, _imgPuCarrier, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPuTempest, i, ref iPosX, iPosY, iSize, _imgPuTempest, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPuMothershipcore, i, ref iPosX, iPosY, iSize, _imgPuMothershipcore,
                            g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPuMothership, i, ref iPosX, iPosY, iSize, _imgPuMothership, g,
                            clPlayercolor, fStringFont, false);

                        /* Zerg */
                        Helper_DrawUnitsProduction(_lZuLarva, i, ref iPosX, iPosY, iSize, _imgZuLarva, g, clPlayercolor,
                            fStringFont, false);
                        Helper_DrawUnitsProduction(_lZuDrone, i, ref iPosX, iPosY, iSize, _imgZuDrone, g, clPlayercolor,
                            fStringFont, false);
                        Helper_DrawUnitsProduction(_lZuOverlord, i, ref iPosX, iPosY, iSize, _imgZuOverlord, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZuQueen, i, ref iPosX, iPosY, iSize, _imgZuQueen, g, clPlayercolor,
                            fStringFont, false);
                        Helper_DrawUnitsProduction(_lZuZergling, i, ref iPosX, iPosY, iSize, _imgZuZergling, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZuBaneling, i, ref iPosX, iPosY, iSize, _imgZuBaneling, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZuBanelingCocoon, i, ref iPosX, iPosY, iSize, _imgZuBanelingCocoon,
                            g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZuRoach, i, ref iPosX, iPosY, iSize, _imgZuRoach, g, clPlayercolor,
                            fStringFont, false);
                        Helper_DrawUnitsProduction(_lZuHydra, i, ref iPosX, iPosY, iSize, _imgZuHydra, g, clPlayercolor,
                            fStringFont, false);
                        Helper_DrawUnitsProduction(_lZuMutalisk, i, ref iPosX, iPosY, iSize, _imgZuMutalisk, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZuInfestor, i, ref iPosX, iPosY, iSize, _imgZuInfestor, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZuOverseer, i, ref iPosX, iPosY, iSize, _imgZuOverseer, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZuOverseerCocoon, i, ref iPosX, iPosY, iSize, _imgZuOvserseerCocoon,
                            g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZuSwarmhost, i, ref iPosX, iPosY, iSize, _imgZuSwarmhost, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZuUltralisk, i, ref iPosX, iPosY, iSize, _imgZuUltra, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZuViper, i, ref iPosX, iPosY, iSize, _imgZuViper, g, clPlayercolor,
                            fStringFont, false);
                        Helper_DrawUnitsProduction(_lZuCorruptor, i, ref iPosX, iPosY, iSize, _imgZuCorruptor, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZuBroodlord, i, ref iPosX, iPosY, iSize, _imgZuBroodlord, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZuBroodlordCocoon, i, ref iPosX, iPosY, iSize,
                            _imgZuBroodlordCocoon,
                            g, clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZuInfestedTerran, i, ref iPosX, iPosY, iSize,
                           _imgInfestedTerran,
                           g, clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZuInfestedTerranEgg, i, ref iPosX, iPosY, iSize,
                           _imgInfestedTerranEgg,
                           g, clPlayercolor, fStringFont, false);

                        /* Maximum for the units */
                        iWidthUnits = iPosX;
                    }

                    #endregion

                    #region - Split Units and Buildings -

                    if (PSettings.PreferenceAll.OverlayProduction.SplitBuildingsAndUnits)
                    {
                        iHavetoadd = 0;


                        if (GInformation.Player[0].Type == PlayerType.Neutral)
                            iHavetoadd += 1;

                        if (i == iHavetoadd)
                        {
                            if (iPosX > iPosXAfterName)
                            {
                                iPosY += iSize + 2;
                                iPosX = iPosXAfterName;
                            }
                        }

                        else if (i > iHavetoadd)
                        {
                            if (iPosX > iPosXAfterName)
                            {
                                iPosY += iSize + 2;
                                iPosX = iPosXAfterName;
                            }
                        }

                        if (PSettings.PreferenceAll.OverlayProduction.UseTransparentImages)
                            iPosY += 3;
                    }

                    #endregion

                    #region Draw Buildings

                    if (PSettings.PreferenceAll.OverlayProduction.ShowBuildings)
                    {
                        /* Terran */
                        Helper_DrawUnitsProduction(_lTbCommandCenter, i,
                            ref iPosX, iPosY, iSize, _imgTbCc, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lTbOrbitalCommand, i,
                            ref iPosX, iPosY, iSize, _imgTbOc, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTbPlanetaryFortress, i,
                            ref iPosX, iPosY, iSize, _imgTbPf, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTbSupply, i, ref iPosX, iPosY,
                            iSize, _imgTbSupply, g, clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lTbRefinery, i, ref iPosX, iPosY,
                            iSize, _imgTbRefinery, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lTbBunker, i, ref iPosX, iPosY,
                            iSize, _imgTbBunker, g, clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lTbTechlab, i, ref iPosX, iPosY,
                            iSize, _imgTbTechlab, g, clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lTbReactor, i, ref iPosX, iPosY,
                            iSize, _imgTbReactor, g, clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lTbTurrent, i, ref iPosX, iPosY,
                            iSize, _imgTbTurrent, g, clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lTbSensorTower, i, ref iPosX,
                            iPosY, iSize, _imgTbSensorTower, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lTbEbay, i, ref iPosX, iPosY, iSize,
                            _imgTbEbay, g, clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lTbGhostAcademy, i, ref iPosX,
                            iPosY, iSize, _imgTbGhostacademy, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lTbArmory, i, ref iPosX, iPosY,
                            iSize, _imgTbArmory, g, clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lTbFusionCore, i, ref iPosX,
                            iPosY, iSize, _imgTbFusioncore, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lTbBarracks, i, ref iPosX, iPosY,
                            iSize, _imgTbBarracks, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lTbFactory, i, ref iPosX, iPosY,
                            iSize, _imgTbFactory, g, clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lTbStarport, i, ref iPosX, iPosY,
                            iSize, _imgTbStarport, g,
                            clPlayercolor, fStringFont, true);


                        /* Protoss */
                        Helper_DrawUnitsProduction(_lPbNexus, i, ref iPosX, iPosY, iSize, _imgPbNexus, g, clPlayercolor,
                            fStringFont, true);
                        Helper_DrawUnitsProduction(_lPbPylon, i, ref iPosX, iPosY, iSize, _imgPbPylon, g, clPlayercolor,
                            fStringFont, true);
                        Helper_DrawUnitsProduction(_lPbAssimilator, i, ref iPosX, iPosY, iSize, _imgPbAssimilator, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lPbCannon, i, ref iPosX, iPosY, iSize, _imgPbCannon, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lPbDarkshrine, i, ref iPosX, iPosY, iSize, _imgPbDarkShrine, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lPbTemplarArchives, i, ref iPosX, iPosY, iSize,
                            _imgPbTemplarArchives,
                            g, clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lPbTwilight, i, ref iPosX, iPosY, iSize, _imgPbTwillightCouncil, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lPbCybercore, i, ref iPosX, iPosY, iSize, _imgPbCybercore, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lPbForge, i, ref iPosX, iPosY, iSize, _imgPbForge, g, clPlayercolor,
                            fStringFont, true);
                        Helper_DrawUnitsProduction(_lPbFleetbeacon, i, ref iPosX, iPosY, iSize, _imgPbFleetBeacon, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lPbRoboticsSupport, i, ref iPosX, iPosY, iSize,
                            _imgPbRoboticsSupport,
                            g, clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lPbGateway, i, ref iPosX, iPosY, iSize, _imgPbGateway, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lPbWarpgate, i, ref iPosX, iPosY, iSize, _imgPbWarpgate, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lPbStargate, i, ref iPosX, iPosY, iSize, _imgPbStargate, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lPbRobotics, i, ref iPosX, iPosY, iSize, _imgPbRobotics, g,
                            clPlayercolor, fStringFont, true);

                        /* Zerg */
                        Helper_DrawUnitsProduction(_lZbCreepTumor, i, ref iPosX, iPosY, iSize, _imgZbCreepTumor, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lZbHatchery, i, ref iPosX, iPosY, iSize, _imgZbHatchery, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lZbLair, i, ref iPosX, iPosY, iSize, _imgZbLair, g, clPlayercolor,
                            fStringFont, true);
                        Helper_DrawUnitsProduction(_lZbHive, i, ref iPosX, iPosY, iSize, _imgZbHive, g, clPlayercolor,
                            fStringFont, true);
                        Helper_DrawUnitsProduction(_lZbSpawningpool, i, ref iPosX, iPosY, iSize, _imgZbSpawningpool, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lZbEvochamber, i, ref iPosX, iPosY, iSize, _imgZbEvochamber, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lZbExtractor, i, ref iPosX, iPosY, iSize, _imgZbExtractor, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lZbSpine, i, ref iPosX, iPosY, iSize, _imgZbSpinecrawler, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lZbSpore, i, ref iPosX, iPosY, iSize, _imgZbSporecrawler, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lZbHydraden, i, ref iPosX, iPosY, iSize, _imgZbHydraden, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lZbRoachwarren, i, ref iPosX, iPosY, iSize, _imgZbRoachwarren, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lZbSpire, i, ref iPosX, iPosY, iSize, _imgZbSpire, g, clPlayercolor,
                            fStringFont, true);
                        Helper_DrawUnitsProduction(_lZbGreaterspire, i, ref iPosX, iPosY, iSize, _imgZbGreaterspire, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lZbUltracavern, i, ref iPosX, iPosY, iSize, _imgZbUltracavern, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lZbInfestationpit, i, ref iPosX, iPosY, iSize, _imgZbInfestationpit,
                            g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lZbBanelingnest, i, ref iPosX, iPosY, iSize, _imgZbBanelingnest, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lZbNydusbegin, i, ref iPosX, iPosY, iSize, _imgZbNydusNetwork, g,
                            clPlayercolor, fStringFont, true);
                        Helper_DrawUnitsProduction(_lZbNydusend, i, ref iPosX, iPosY, iSize, _imgZbNydusWorm, g,
                            clPlayercolor, fStringFont, true);

                        iWidthBuildings = iPosX;
                    }

                    #endregion

                    #region - Split Units and Buildings -

                    if (PSettings.PreferenceAll.OverlayProduction.SplitBuildingsAndUnits)
                    {
                        iHavetoadd = 0;


                        if (GInformation.Player[0].Type == PlayerType.Neutral)
                            iHavetoadd += 1;

                        if (i == iHavetoadd)
                        {
                            if (iPosX > iPosXAfterName)
                            {
                                iPosY += iSize + 2;
                                iPosX = iPosXAfterName;
                            }
                        }

                        else if (i > iHavetoadd)
                        {
                            if (iPosX > iPosXAfterName)
                            {
                                iPosY += iSize + 2;
                                iPosX = iPosXAfterName;
                            }
                        }

                        if (PSettings.PreferenceAll.OverlayProduction.UseTransparentImages)
                            iPosY += 3;
                    }

                    #endregion

                    #region Upgrades

                    if (PSettings.PreferenceAll.OverlayProduction.ShowUpgrades)
                    {
                        #region Terran

                        Helper_DrawUnitsProduction(_lTupStim, i, ref iPosX, iPosY, iSize, _imgTupStim, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupCombatShields, i, ref iPosX, iPosY, iSize, _imgTupCombatShields,
                            g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupConcussiveShells, i, ref iPosX, iPosY, iSize,
                            _imgTupConcussiveShells, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupBlueFlame, i, ref iPosX, iPosY, iSize, _imgTupBlueFlame, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupDrillingClaws, i, ref iPosX, iPosY, iSize, _imgTupDrillingClaws,
                            g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupTransformationServos, i, ref iPosX, iPosY, iSize,
                            _imgTupTransformatorServos, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupPersonalCloak, i, ref iPosX, iPosY, iSize, _imgTupPersonalCloak,
                            g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupMoebiusReactor, i, ref iPosX, iPosY, iSize,
                            _imgTupMoebiusReactor, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupWeaponRefit, i, ref iPosX, iPosY, iSize, _imgTupWeaponRefit, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupBehemothReactor, i, ref iPosX, iPosY, iSize,
                            _imgTupBehemothReacot, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupCloakingField, i, ref iPosX, iPosY, iSize, _imgTupCloakingField,
                            g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupCorvidReactor, i, ref iPosX, iPosY, iSize, _imgTupCorvidReactor,
                            g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupCaduceusReactor, i, ref iPosX, iPosY, iSize,
                            _imgTupCaduceusReactor, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupDurableMaterials, i, ref iPosX, iPosY, iSize,
                            _imgTupDurableMaterials, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupVehicleShipPlanting1, i, ref iPosX, iPosY, iSize,
                            _imgTupVehicleShipPlanting1, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupVehicleShipPlanting2, i, ref iPosX, iPosY, iSize,
                            _imgTupVehicleShipPlanting2, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupVehicleShipPlanting3, i, ref iPosX, iPosY, iSize,
                            _imgTupVehicleShipPlanting3, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupVehicleWeapon1, i, ref iPosX, iPosY, iSize,
                            _imgTupVehicleWeapon1, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupVehicleWeapon2, i, ref iPosX, iPosY, iSize,
                            _imgTupVehicleWeapon2, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupVehicleWeapon3, i, ref iPosX, iPosY, iSize,
                            _imgTupVehicleWeapon3, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupShipWeapon1, i, ref iPosX, iPosY, iSize, _imgTupShipWeapon1, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupShipWeapon2, i, ref iPosX, iPosY, iSize, _imgTupShipWeapon2, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupShipWeapon3, i, ref iPosX, iPosY, iSize, _imgTupShipWeapon3, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupInfantryArmor1, i, ref iPosX, iPosY, iSize,
                            _imgTupInfantryArmor1, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupInfantryArmor2, i, ref iPosX, iPosY, iSize,
                            _imgTupInfantryArmor2, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupInfantryArmor3, i, ref iPosX, iPosY, iSize,
                            _imgTupInfantryArmor3, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupInfantryWeapon1, i, ref iPosX, iPosY, iSize,
                            _imgTupInfantryWeapon1, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupInfantryWeapon2, i, ref iPosX, iPosY, iSize,
                            _imgTupInfantryWeapon2, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupInfantryWeapon3, i, ref iPosX, iPosY, iSize,
                            _imgTupInfantryWeapon3, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupHighSecAutoTracking, i, ref iPosX, iPosY, iSize,
                            _imgTupHighSecAutoTracking, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupStructureArmor, i, ref iPosX, iPosY, iSize,
                            _imgTupStructureArmor, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lTupNeosteelFrame, i, ref iPosX, iPosY, iSize, _imgTupNeosteelFrame,
                            g,
                            clPlayercolor, fStringFont, false);

                        #endregion

                        #region Protoss

                        Helper_DrawUnitsProduction(_lPupAirArmor1, i, ref iPosX, iPosY, iSize, _imgPupAirArmor1, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPupAirArmor2, i, ref iPosX, iPosY, iSize, _imgPupAirArmor2, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPupAirArmor3, i, ref iPosX, iPosY, iSize, _imgPupAirArmor3, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPupAirWeapon1, i, ref iPosX, iPosY, iSize, _imgPupAirWeapon1, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPupAirWeapon2, i, ref iPosX, iPosY, iSize, _imgPupAirWeapon2, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPupAirWeapon3, i, ref iPosX, iPosY, iSize, _imgPupAirWeapon3, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPupGroundWeapon1, i, ref iPosX, iPosY, iSize, _imgPupGroundWeapon1,
                            g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPupGroundWeapon2, i, ref iPosX, iPosY, iSize, _imgPupGroundWeapon2,
                            g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPupGroundWeapon3, i, ref iPosX, iPosY, iSize, _imgPupGroundWeapon3,
                            g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPupGroundArmor1, i, ref iPosX, iPosY, iSize, _imgPupGroundArmor1, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPupGroundArmor2, i, ref iPosX, iPosY, iSize, _imgPupGroundArmor2, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPupGroundArmor3, i, ref iPosX, iPosY, iSize, _imgPupGroundArmor3, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPupShield1, i, ref iPosX, iPosY, iSize, _imgPupShield1, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPupShield2, i, ref iPosX, iPosY, iSize, _imgPupShield2, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPupShield3, i, ref iPosX, iPosY, iSize, _imgPupShield3, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPupExtendedThermalLance, i, ref iPosX, iPosY, iSize,
                            _imgPupExtendedThermalLance, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPupGraviticBooster, i, ref iPosX, iPosY, iSize,
                            _imgPupGraviticBooster, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPupGraviticDrive, i, ref iPosX, iPosY, iSize, _imgPupGraviticDrive,
                            g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPupGravitonCatapult, i, ref iPosX, iPosY, iSize,
                            _imgPupGravitonCatapult, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPupStorm, i, ref iPosX, iPosY, iSize, _imgPupStorm, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPupBlink, i, ref iPosX, iPosY, iSize, _imgPupBlink, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPupCharge, i, ref iPosX, iPosY, iSize, _imgPupCharge, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPupAnionPulseCrystal, i, ref iPosX, iPosY, iSize,
                            _imgPupAnionPulseCrystals, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lPupWarpGate, i, ref iPosX, iPosY, iSize, _imgPupWarpGate, g,
                            clPlayercolor, fStringFont, false);

                        #endregion

                        #region Zerg

                        Helper_DrawUnitsProduction(_lZupAdrenalGlands, i, ref iPosX, iPosY, iSize, _imgZupAdrenalGlands,
                            g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupAirArmor1, i, ref iPosX, iPosY, iSize, _imgZupAirArmor1, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupAirArmor2, i, ref iPosX, iPosY, iSize, _imgZupAirArmor2, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupAirArmor3, i, ref iPosX, iPosY, iSize, _imgZupAirArmor3, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupAirWeapon1, i, ref iPosX, iPosY, iSize, _imgZupAirWeapon1, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupAirWeapon2, i, ref iPosX, iPosY, iSize, _imgZupAirWeapon2, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupAirWeapon3, i, ref iPosX, iPosY, iSize, _imgZupAirWeapon3, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupBurrow, i, ref iPosX, iPosY, iSize, _imgZupBurrow, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupCentrifugalHooks, i, ref iPosX, iPosY, iSize,
                            _imgZupCentrifugalHooks, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupChitinousPlating, i, ref iPosX, iPosY, iSize,
                            _imgZupChitinousPlating, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupEnduringLocusts, i, ref iPosX, iPosY, iSize,
                            _imgZupEnduringLocusts, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupFlyingLocust, i, ref iPosX, iPosY, iSize,
                            _imgZupFlyingLocust, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupGlialReconstruction, i, ref iPosX, iPosY, iSize,
                            _imgZupGlialReconstruction, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupGroovedSpines, i, ref iPosX, iPosY, iSize, _imgZupGroovedSpines,
                            g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupGroundArmor1, i, ref iPosX, iPosY, iSize, _imgZupGroundArmor1, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupGroundArmor2, i, ref iPosX, iPosY, iSize, _imgZupGroundArmor2, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupGroundArmor3, i, ref iPosX, iPosY, iSize, _imgZupGroundArmor3, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupGroundMelee1, i, ref iPosX, iPosY, iSize, _imgZupGroundMelee1, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupGroundMelee2, i, ref iPosX, iPosY, iSize, _imgZupGroundMelee2, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupGroundMelee3, i, ref iPosX, iPosY, iSize, _imgZupGroundMelee3, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupGroundWeapon1, i, ref iPosX, iPosY, iSize, _imgZupGroundWeapon1,
                            g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupGroundWeapon2, i, ref iPosX, iPosY, iSize, _imgZupGroundWeapon2,
                            g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupGroundWeapon3, i, ref iPosX, iPosY, iSize, _imgZupGroundWeapon3,
                            g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupMetabolicBoost, i, ref iPosX, iPosY, iSize,
                            _imgZupMetabolicBoost, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupMuscularAugments, i, ref iPosX, iPosY, iSize,
                            _imgZupMuscularAugments, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupNeutralParasite, i, ref iPosX, iPosY, iSize,
                            _imgZupNeutralParasite, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupPathoglenGlands, i, ref iPosX, iPosY, iSize,
                            _imgZupPathoglenGlands, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupPneumatizedCarapace, i, ref iPosX, iPosY, iSize,
                            _imgZupPneumatizedCarapace, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupTunnnelingClaws, i, ref iPosX, iPosY, iSize,
                            _imgZupTunnelingClaws, g,
                            clPlayercolor, fStringFont, false);
                        Helper_DrawUnitsProduction(_lZupVentralSacs, i, ref iPosX, iPosY, iSize, _imgZupVentrallSacs, g,
                            clPlayercolor, fStringFont, false);

                        #endregion

                        iWidthUpgrades = iPosX;
                    }

                    #endregion

                    if (iPosX > iPosXAfterName)
                    {
                        iPosY += iSize + 2;

                        if (PSettings.PreferenceAll.OverlayProduction.UseTransparentImages)
                            iPosY += 5;
                    }


                    /* Check which width is bigger */
                    //if (iWidthUnits > iWidthBuildings)
                    //{
                    //    if (iWidthUnits > iWidthUpgrades)
                    //        iPosX = iWidthUnits;

                    //    else if (iWidthUpgrades > iWidthUnits)
                    //        iPosX = iWidthUpgrades;
                    //}

                    //else if (iWidthBuildings > iWidthUnits)
                    //{
                    //    if (iWidthBuildings > iWidthUpgrades)
                    //        iPosX = iWidthBuildings;

                    //    else if (iWidthUpgrades > iWidthBuildings)
                    //        iPosX = iWidthUpgrades;
                    //}

                    //else if (iWidthUpgrades > iWidthUnits)
                    //{
                    //    if (iWidthBuildings > iWidthUpgrades)
                    //        iPosX = iWidthBuildings;

                    //    else if (iWidthUpgrades > iWidthBuildings)
                    //        iPosX = iWidthUpgrades;
                    //}

                    var iWidthMax = HelpFunctions.GetMaximumInteger(iWidthBuildings, iWidthUnits, iWidthUpgrades);
                    iPosX = iWidthMax;

                    //iPosX = iWidthUnits > iWidthBuildings ? iWidthUnits : iWidthBuildings;

                    //iPosX = iWidthUnits > iWidthBuildings ? iWidthUnits : iWidthBuildings;

                    /* Adjust maximum width */
                    if (iPosX >= iMaximumWidth)
                        iMaximumWidth = iPosX;

                    if (iHavetoadd > 0)
                        iMaximumWidth += iSize;


                    if (iWidthMax > iPosXAfterName)
                    {
                        g.Graphics.DrawString(
                            strName,
                            fStringFont,
                            new SolidBrush(clPlayercolor),
                            Brushes.Black, 0 + 10,
                            iPosYName + 10,
                            1f, 1f, true);
                    }

                    iPosYName = iPosY;
                }

                if (FormBorderStyle == FormBorderStyle.None)
                {
                    Width = iMaximumWidth + 1;
                    Height = iPosY;
                }

                else
                {
                    _iProdPanelWidth = iMaximumWidth + 1;
                    _iProdPanelWidthWithoutName = _iProdPanelWidth - iPosXAfterName;
                    _iProdPosAfterName = iPosXAfterName;
                }
            }

            catch (Exception ex)
            {
                Messages.LogFile("Over all", ex);
            }
        }

        /// <summary>
        ///     Sends the panel specific data into the Form's controls and settings
        /// </summary>
        protected override void MouseUpTransferData()
        {
            /* Has to be calculated manually because each panels has it's own Neutral handling.. */
            var iValidPlayerCount = GInformation.Gameinfo.ValidPlayerCount;

            PSettings.PreferenceAll.OverlayProduction.X = Location.X;
            PSettings.PreferenceAll.OverlayProduction.Y = Location.Y;
            PSettings.PreferenceAll.OverlayProduction.Width = Width;
            PSettings.PreferenceAll.OverlayProduction.Height = Height/iValidPlayerCount;
            PSettings.PreferenceAll.OverlayProduction.Opacity = Opacity;
        }

        /// <summary>
        ///     Sends the panel specific data into the Form's controls and settings
        /// </summary>
        protected override void MouseWheelTransferData(MouseEventArgs e)
        {
            if (e.Delta.Equals(120))
            {
                PSettings.PreferenceAll.OverlayProduction.PictureSize += 1;
            }

            else if (e.Delta.Equals(-120))
            {
                PSettings.PreferenceAll.OverlayProduction.PictureSize -= 1;
            }
        }

        /// <summary>
        ///     Sends the panel specific data into the Form's controls and settings
        ///     Also changes the Size directly!
        /// </summary>
        protected override void AdjustPanelSize()
        {
            if (BSetSize)
            {
                tmrRefreshGraphic.Interval = 20;

                PSettings.PreferenceAll.OverlayProduction.Width = Cursor.Position.X - Left;

                if ((Cursor.Position.Y - Top) >= 5)
                    PSettings.PreferenceAll.OverlayProduction.Height = (Cursor.Position.Y - Top);

                else
                    PSettings.PreferenceAll.OverlayProduction.Height = 5;
            }

            var strInput = StrBackupSizeChatbox;

            if (string.IsNullOrEmpty(strInput))
                return;

            if (strInput.Contains('\0'))
                strInput = strInput.Substring(0, strInput.IndexOf('\0'));


            if (strInput.Equals(PSettings.PreferenceAll.OverlayProduction.ChangeSize))
            {
                if (BToggleSize)
                {
                    BToggleSize = !BToggleSize;

                    if (!BSetSize)
                        BSetSize = true;
                }
            }

            if (HelpFunctions.HotkeysPressed(Keys.Enter))
            {
                tmrRefreshGraphic.Interval = PSettings.PreferenceAll.Global.DrawingRefresh;

                BSetSize = false;
                StrBackupSizeChatbox = string.Empty;
            }
        }

        /// <summary>
        ///     Loads the settings of the specific Form into the controls (Location, Size)
        /// </summary>
        protected override void LoadPreferencesIntoControls()
        {
            Location = new Point(PSettings.PreferenceAll.OverlayProduction.X,
                PSettings.PreferenceAll.OverlayProduction.Y);
            Opacity = PSettings.PreferenceAll.OverlayProduction.Opacity;
        }

        /// <summary>
        ///     Sends the panel specific data into the Form's controls and settings
        ///     Also changes the Position directly!
        /// </summary>
        protected override void AdjustPanelPosition()
        {
            if (BSetPosition)
            {
                tmrRefreshGraphic.Interval = 20;

                Location = Cursor.Position;
                PSettings.PreferenceAll.OverlayProduction.X = Cursor.Position.X;
                PSettings.PreferenceAll.OverlayProduction.Y = Cursor.Position.Y;
            }

            var strInput = StrBackupChatbox;

            if (string.IsNullOrEmpty(strInput))
                return;

            if (strInput.Contains('\0'))
                strInput = strInput.Substring(0, strInput.IndexOf('\0'));

            if (strInput.Equals(PSettings.PreferenceAll.OverlayProduction.ChangePosition))
            {
                if (BTogglePosition)
                {
                    BTogglePosition = !BTogglePosition;

                    if (!BSetPosition)
                        BSetPosition = true;
                }
            }

            if (HelpFunctions.HotkeysPressed(Keys.Enter))
            {
                BSetPosition = false;
                StrBackupChatbox = string.Empty;
                tmrRefreshGraphic.Interval = PSettings.PreferenceAll.Global.DrawingRefresh;
            }
        }

        /// <summary>
        ///     Loads some specific data into the Form.
        /// </summary>
        protected override void LoadSpecificData()
        {
        }

        private void ProductionRenderer_IsHiddenChanged(object sender, EventArgs e)
        {
            PSettings.PreferenceAll.OverlayProduction.LaunchStatus = !IsHidden;
        }

        /// <summary>
        ///     Changes settings for a specific Form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void BaseRenderer_ResizeEnd(object sender, EventArgs e)
        {
            /* Required to avoid the size of the border [FormBorderStyle] */
            var tmpOld = FormBorderStyle;
            FormBorderStyle = FormBorderStyle.None;

            /* Calculate amount of unitpictures - width */
            var iAmount = _iProdPanelWidthWithoutName/PSettings.PreferenceAll.OverlayProduction.PictureSize;
            PSettings.PreferenceAll.OverlayProduction.PictureSize = (Width - (_iProdPosAfterName + 1))/
                                                                    iAmount;


            FormBorderStyle = tmpOld;

            /* Temporarily reset interval */
            var oldInterval = tmrRefreshGraphic.Interval;
            tmrRefreshGraphic.Interval = 1;


            tmrRefreshGraphic.Interval = oldInterval;

            PSettings.PreferenceAll.OverlayProduction.X = Location.X;
            PSettings.PreferenceAll.OverlayProduction.Y = Location.Y;
        }

        /* Draw the units */

        private void Helper_DrawUnitsProduction(List<UnitCount> units, int index, ref int posX, int posY, int size,
            Image img,
            BufferedGraphics g, Color clPlayercolor, Font font, bool isStructure)
        {
            if (units == null ||
                units.Count <= 0)
                return;

            var unit = units[index];


            /* Unitamount defines all buildings*/
            if (isStructure)
                unit.UnitAmount -= unit.UnitUnderConstruction;

            /* If there is nothing to draw.. */
            if (unit.UnitUnderConstruction == 0)
                return;

            if (unit.ConstructionState == null)
                return;

            if (unit.ConstructionState.Count == 0)
                return;

            if (float.IsNaN(unit.ConstructionState[0]) ||
                unit.ConstructionState[0].Equals(0.0f))
                return;

            g.Graphics.DrawImage(img, posX, posY, size, size);


            if (unit.UnitUnderConstruction > 0)
            {
                var bDraw = false;

                if (unit.SpeedMultiplier.Count > 0)
                {
                    /* If any of them is above 4069... */
                    foreach (var speed in unit.SpeedMultiplier)
                    {
                        if (speed > 4096)
                        {
                            bDraw = true;
                            break;
                        }
                    }
                }


                if (bDraw && !PSettings.PreferenceAll.OverlayProduction.RemoveChronoboost)
                {
                    HelpFunctions.HelpGraphics.FillRoundRectangle(g.Graphics,
                        new SolidBrush(Color.FromArgb(100, Color.White)),
                        posX + size - 22,
                        posY + 3, 19, 19, 5);
                    g.Graphics.DrawImage(_imgSpeedArrow, new Rectangle(posX + size - 20, posY + 5, 15, 15));
                }


                float fWidth;

                if (unit.UnitUnderConstruction.ToString(CultureInfo.InvariantCulture).Length == 1)
                    fWidth = unit.UnitUnderConstruction.ToString(CultureInfo.InvariantCulture).Length*(font.Size + 4);

                else
                    fWidth = unit.UnitUnderConstruction.ToString(CultureInfo.InvariantCulture).Length*(font.Size);

                HelpFunctions.HelpGraphics.FillRoundRectangle(g.Graphics,
                    new SolidBrush(Color.FromArgb(100, Color.Black)),
                    posX + 1, posY + font.Size + 10, fWidth, font.Size + 9, 5);


                g.Graphics.DrawString(unit.UnitUnderConstruction.ToString(CultureInfo.InvariantCulture), font,
                    Brushes.Orange, posX + 2,
                    posY + font.Size + 9);


                /* Adjust relative size */
                float ftemp = size - 4;
                ftemp *= (unit.ConstructionState[0]/100);

                var iOffset = 5;

                if (!PSettings.PreferenceAll.OverlayProduction.UseTransparentImages)
                    iOffset = 0;


                /* Draw status- line */
                g.Graphics.DrawLine(new Pen(Brushes.Yellow, 2), posX + 2, posY + size - 3 + iOffset,
                    posX + 2 + (int) ftemp,
                    posY + size - 3 + iOffset);
                g.Graphics.DrawRectangle(new Pen(Brushes.Black, 1), posX + 2, posY + size - 5 + iOffset, size - 3, 3);
            }

            if (!PSettings.PreferenceAll.OverlayProduction.UseTransparentImages)
                g.Graphics.DrawRectangle(new Pen(new SolidBrush(clPlayercolor), 2), posX, posY, size, size);

            posX += size;
        }
    }
}