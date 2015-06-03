﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using AnotherSc2Hack.Classes.BackEnds;
using AnotherSc2Hack.Classes.DataStructures.Preference;
using Predefined;
using Utilities.ExtensionMethods;

namespace AnotherSc2Hack.Classes.FrontEnds.Rendering
{
    public class ResourcesRenderer : BaseRenderer
    {

        private Image _imgMinerals = Properties.Resources.Mineral_Protoss,
                      _imgGas = Properties.Resources.Gas_Protoss,
                      _imgSupply = Properties.Resources.Supply_Protoss,
                      _imgWorker = Properties.Resources.P_Probe;


        public ResourcesRenderer(GameInfo gInformation, PreferenceManager pSettings, Process sc2Process)
            : base(gInformation, pSettings, sc2Process)
        {
            IsHiddenChanged += ResourcesRenderer_IsHiddenChanged;
        }

        /// <summary>
        /// Draws the panelspecific data.
        /// </summary>
        /// <param name="g"></param>
        protected override void Draw(BufferedGraphics g)
        {
            GInformation.CAccessPlayers = true;

            try
            {
                if (!GInformation.Gameinfo.IsIngame)
                    return;

                var iValidPlayerCount = GInformation.Gameinfo.ValidPlayerCount;

                if (iValidPlayerCount == 0)
                    return;

                Opacity = PSettings.PreferenceAll.OverlayResources.Opacity;
                var iSingleHeight = Height / iValidPlayerCount;
                var fNewFontSize = (float)((29.0 / 100) * iSingleHeight);
                var fInternalFont = new Font(PSettings.PreferenceAll.OverlayResources.FontName, fNewFontSize, FontStyle.Bold);
                var fInternalFontNormal = new Font(fInternalFont.Name, fNewFontSize, FontStyle.Regular);

                if (!BChangingPosition)
                {
                    Height = PSettings.PreferenceAll.OverlayResources.Height * iValidPlayerCount;
                    Width = PSettings.PreferenceAll.OverlayResources.Width;


                }

                var iCounter = 0;


                for (var i = 0; i < GInformation.Player.Count; i++)
                {
                    var clPlayercolor = GInformation.Player[i].Color;

                    #region Teamcolor

                    RendererHelper.TeamColor(GInformation.Player, i,
                                              GInformation.Gameinfo.IsTeamcolor, ref clPlayercolor);

                    #endregion

                    #region Escape sequences

                    if (PSettings.PreferenceAll.OverlayResources.RemoveAi)
                    {
                        if (GInformation.Player[i].Type.Equals(PredefinedData.PlayerType.Ai))
                            continue;
                    }

                    if (PSettings.PreferenceAll.OverlayResources.RemoveNeutral)
                    {
                        if (GInformation.Player[i].Type.Equals(PredefinedData.PlayerType.Neutral))
                            continue;
                    }

                    if (PSettings.PreferenceAll.OverlayResources.RemoveAllie)
                    {
                        if (GInformation.Player[0].Localplayer == 16)
                        {
                            //Do nothing
                        }

                        else
                        {
                            if (GInformation.Player[i].Team ==
                                GInformation.Player[GInformation.Player[i].Localplayer].Team &&
                                !GInformation.Player[i].IsLocalplayer)
                                continue;
                        }
                    }

                    if (PSettings.PreferenceAll.OverlayResources.RemoveLocalplayer)
                    {
                        if (GInformation.Player[i].IsLocalplayer)
                            continue;
                    }



                    if (GInformation.Player[i].Name.StartsWith("\0") || GInformation.Player[i].NameLength <= 0)
                        continue;

                    if (GInformation.Player[i].Type.Equals(PredefinedData.PlayerType.Hostile))
                        continue;

                    if (GInformation.Player[i].Type.Equals(PredefinedData.PlayerType.Observer))
                        continue;

                    if (GInformation.Player[i].Type.Equals(PredefinedData.PlayerType.Referee))
                        continue;

                    if (CheckIfGameheart(GInformation.Player[i]))
                        continue;

                    #endregion

                    #region SetValidImages (Race)

                    if (GInformation.Player[i].PlayerRace.Equals(PredefinedData.PlayerRace.Terran))
                    {
                        _imgMinerals = Properties.Resources.Mineral_Terran;
                        _imgGas = Properties.Resources.Gas_Terran;
                        _imgSupply = Properties.Resources.Supply_Terran;
                        _imgWorker = Properties.Resources.T_SCV;
                    }

                    else if (GInformation.Player[i].PlayerRace.Equals(PredefinedData.PlayerRace.Protoss))
                    {
                        _imgMinerals = Properties.Resources.Mineral_Protoss;
                        _imgGas = Properties.Resources.Gas_Protoss;
                        _imgSupply = Properties.Resources.Supply_Protoss;
                        _imgWorker = Properties.Resources.P_Probe;
                    }

                    else
                    {
                        _imgMinerals = Properties.Resources.Mineral_Zerg;
                        _imgGas = Properties.Resources.Gas_Zerg;
                        _imgSupply = Properties.Resources.Supply_Zerg;
                        _imgWorker = Properties.Resources.Z_Drone;
                    }

                    #endregion

                    #region Draw Bounds and Background

                    if (PSettings.PreferenceAll.OverlayResources.DrawBackground)
                    {
                        /* Background */
                        g.Graphics.FillRectangle(Brushes.Gray, 1, 1 + (iSingleHeight * iCounter), Width - 2,
                                                 iSingleHeight - 2);

                        /* Border */
                        g.Graphics.DrawRectangle(new Pen(new SolidBrush(clPlayercolor), 2), 1,
                                                 1 + (iSingleHeight * iCounter),
                                                 Width - 2, iSingleHeight - 2);
                    }

                    #endregion

                    #region Content Drawing

                    #region Name

                    var strName = (GInformation.Player[i].ClanTag.StartsWith("\0") || PSettings.PreferenceAll.OverlayResources.RemoveClanTag)
                                         ? GInformation.Player[i].Name
                                         : "[" + GInformation.Player[i].ClanTag + "] " + GInformation.Player[i].Name;

                    g.Graphics.DrawString(strName, fInternalFont,
                        new SolidBrush(clPlayercolor),
                        Brushes.Black, (float)((1.67 / 100) * Width),
                        (float)((24.0 / 100) * iSingleHeight) + iSingleHeight * iCounter,
                        1f, 1f, true);

                    #endregion

                    #region Team

                    g.Graphics.DrawString("#" + GInformation.Player[i].Team, fInternalFontNormal,
                        Brushes.White,
                        Brushes.Black, (float)((29.67 / 100) * Width),
                        (float)((24.0 / 100) * iSingleHeight) + iSingleHeight * iCounter,
                        1f, 1f, true);

                    #endregion

                    #region Minerals

                    /* Icon */
                    g.Graphics.DrawImage(_imgMinerals, (float)((37.0 / 100) * Width),
                        (float)((14.0 / 100) * iSingleHeight) + (Height / iValidPlayerCount) * iCounter,
                        (float)((70.0 / 100) * iSingleHeight), (float)((70.0 / 100) * iSingleHeight), Brushes.Black, 1f, 1f,
                        false);

                    g.Graphics.DrawString(
                        GInformation.Player[i].Minerals.ToString(CultureInfo.InvariantCulture),
                        fInternalFontNormal,
                        Brushes.White,
                        Brushes.Black, (float)((43.67 / 100) * Width),
                        (float)((24.0 / 100) * iSingleHeight) + iSingleHeight * iCounter,
                        1f, 1f, true);

                    #endregion

                    #region Gas

                    /* Icon */
                    g.Graphics.DrawImage(_imgGas, (float)((57.0 / 100) * Width),
                        (float)((14.0 / 100) * iSingleHeight) + (Height / iValidPlayerCount) * iCounter,
                        (float)((70.0 / 100) * iSingleHeight), (float)((70.0 / 100) * iSingleHeight), Brushes.Black, 1f, 1f,
                        false);

                    g.Graphics.DrawString(
                        GInformation.Player[i].Gas.ToString(CultureInfo.InvariantCulture),
                        fInternalFontNormal,
                        Brushes.White,
                        Brushes.Black, (float)((63.67 / 100) * Width),
                        (float)((24.0 / 100) * iSingleHeight) + iSingleHeight * iCounter,
                        1f, 1f, true);

                    #endregion

                    #region Supply

                    /* Icon */
                    g.Graphics.DrawImage(_imgSupply, (float)((75.0 / 100) * Width),
                        (float)((14.0 / 100) * iSingleHeight) + (Height / iValidPlayerCount) * iCounter,
                        (float)((70.0 / 100) * iSingleHeight), (float)((70.0 / 100) * iSingleHeight), Brushes.Black, 1f, 1f,
                        false);

                    g.Graphics.DrawString(
                        GInformation.Player[i].SupplyMin.ToString(CultureInfo.InvariantCulture) + "/" +
                        GInformation.Player[i].SupplyMax,
                        fInternalFontNormal,
                        Brushes.White,
                        Brushes.Black, (float)((81.67 / 100) * Width),
                        (float)((24.0 / 100) * iSingleHeight) + iSingleHeight * iCounter,
                        1f, 1f, true);

                    #endregion

                    #endregion


                    iCounter++;
                }
            }

            catch (Exception ex)
            {
                Messages.LogFile("Over all", ex);
            }
        }

        /// <summary>
        /// Sends the panel specific data into the Form's controls and settings
        /// </summary>
        protected override void MouseUpTransferData()
        {
            /* Has to be calculated manually because each panels has it's own Neutral handling.. */
            var iValidPlayerCount = GInformation.Gameinfo.ValidPlayerCount;

            PSettings.PreferenceAll.OverlayResources.X = Location.X;
            PSettings.PreferenceAll.OverlayResources.Y = Location.Y;
            PSettings.PreferenceAll.OverlayResources.Width = Width;
            PSettings.PreferenceAll.OverlayResources.Height = Height / iValidPlayerCount;
            PSettings.PreferenceAll.OverlayResources.Opacity = Opacity;
        }

        /// <summary>
        /// Sends the panel specific data into the Form's controls and settings
        /// </summary>
        protected override void MouseWheelTransferData(MouseEventArgs e)
        {
            if (e.Delta.Equals(120))
            {
                Width += 4;
                Height += 1;
            }

            else if (e.Delta.Equals(-120))
            {
                Width -= 4;
                Height -= 1;
            }
        }

        /// <summary>
        /// Sends the panel specific data into the Form's controls and settings
        /// Also changes the Size directly!
        /// </summary>
        protected override void AdjustPanelSize()
        {
            if (BSetSize)
            {
                tmrRefreshGraphic.Interval = 20;

                PSettings.PreferenceAll.OverlayResources.Width = Cursor.Position.X - Left;

                var iValidPlayerCount = GInformation.Gameinfo.ValidPlayerCount;
                if (PSettings.PreferenceAll.OverlayResources.RemoveNeutral)
                    iValidPlayerCount -= 1;

                if ((Cursor.Position.Y - Top) / iValidPlayerCount >= 5)
                {
                    PSettings.PreferenceAll.OverlayResources.Height = (Cursor.Position.Y - Top) /
                                                        iValidPlayerCount;
                }

                else
                    PSettings.PreferenceAll.OverlayResources.Height = 5;
            }

            var strInput = StrBackupSizeChatbox;

            if (String.IsNullOrEmpty(strInput))
                return;

            if (strInput.Contains('\0'))
                strInput = strInput.Substring(0, strInput.IndexOf('\0'));


            if (strInput.Equals(PSettings.PreferenceAll.OverlayResources.ChangeSize))
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
        /// Loads the settings of the specific Form into the controls (Location, Size)
        /// </summary>
        protected override void LoadPreferencesIntoControls()
        {
            Location = new Point(PSettings.PreferenceAll.OverlayResources.X,
                                     PSettings.PreferenceAll.OverlayResources.Y);
            Size = new Size(PSettings.PreferenceAll.OverlayResources.Width, PSettings.PreferenceAll.OverlayResources.Height);
            Opacity = PSettings.PreferenceAll.OverlayResources.Opacity;
        }

        /// <summary>
        /// Sends the panel specific data into the Form's controls and settings
        /// Also changes the Position directly!
        /// </summary>
        protected override void AdjustPanelPosition()
        {
            if (BSetPosition)
            {
                tmrRefreshGraphic.Interval = 20;

                Location = Cursor.Position;
                PSettings.PreferenceAll.OverlayResources.X = Cursor.Position.X;
                PSettings.PreferenceAll.OverlayResources.Y = Cursor.Position.Y;
            }

            var strInput = StrBackupChatbox;

            if (String.IsNullOrEmpty(strInput))
                return;

            if (strInput.Contains('\0'))
                strInput = strInput.Substring(0, strInput.IndexOf('\0'));

            if (strInput.Equals(PSettings.PreferenceAll.OverlayResources.ChangePosition))
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
        /// Loads some specific data into the Form.
        /// </summary>
        protected override void LoadSpecificData()
        {
            
        }

        void ResourcesRenderer_IsHiddenChanged(object sender, EventArgs e)
        {
            PSettings.PreferenceAll.OverlayResources.LaunchStatus = !IsHidden;
        }

        /// <summary>
        /// Changes settings for a specific Form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void BaseRenderer_ResizeEnd(object sender, EventArgs e)
        {
            /* If the Valid Player count is zero, change it.. */
            var iValidPlayerCount = GInformation.Gameinfo.ValidPlayerCount;

            var iRealPlayerCount = iValidPlayerCount == 0 ? 1 : iValidPlayerCount;

            PSettings.PreferenceAll.OverlayResources.Height = (Height / iRealPlayerCount);
            PSettings.PreferenceAll.OverlayResources.Width = Width;
            PSettings.PreferenceAll.OverlayResources.X = Location.X;
            PSettings.PreferenceAll.OverlayResources.Y = Location.Y;
        }
    }
}
