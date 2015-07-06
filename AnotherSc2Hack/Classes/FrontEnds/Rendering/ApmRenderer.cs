﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AnotherSc2Hack.Classes.BackEnds;
using AnotherSc2Hack.Classes.DataStructures.Preference;
using PredefinedTypes;
using Utilities.ExtensionMethods;
using Utilities.Logger;

namespace AnotherSc2Hack.Classes.FrontEnds.Rendering
{
    public class ApmRenderer : BaseRenderer
    {
        public ApmRenderer(GameInfo gInformation, PreferenceManager pSettings, Process sc2Process)
            : base(gInformation, pSettings, sc2Process)
        {
            IsHiddenChanged += ApmRenderer_IsHiddenChanged;
        }

        /// <summary>
        /// Draws the panelspecific data.
        /// </summary>
        /// <param name="g"></param>
        protected override void Draw(BufferedGraphics g)
        {
            try
            {
                if (!GInformation.Gameinfo.IsIngame)
                    return;

                var iValidPlayerCount = GInformation.Gameinfo.ValidPlayerCount;

                if (iValidPlayerCount == 0)
                    return;

                Opacity = PSettings.PreferenceAll.OverlayApm.Opacity;
                var iSingleHeight = Height / iValidPlayerCount;
                var fNewFontSize = (float)((29.0 / 100) * iSingleHeight);
                var fInternalFont = new Font(PSettings.PreferenceAll.OverlayApm.FontName, fNewFontSize, FontStyle.Bold);
                var fInternalFontNormal = new Font(fInternalFont.Name, fNewFontSize, FontStyle.Regular);

                if (!BChangingPosition)
                {
                    Height = PSettings.PreferenceAll.OverlayApm.Height * iValidPlayerCount;
                    Width = PSettings.PreferenceAll.OverlayApm.Width;
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

                    if (GInformation.Player[i].Name.StartsWith("\0") || GInformation.Player[i].NameLength <= 0)
                        continue;

                    if (GInformation.Player[i].Type.Equals(PlayerType.Hostile))
                        continue;

                    if (GInformation.Player[i].Type.Equals(PlayerType.Observer))
                        continue;

                    if (GInformation.Player[i].Type.Equals(PlayerType.Referee))
                        continue;

                    if (CheckIfGameheart(GInformation.Player[i]))
                        continue;




                    if (PSettings.PreferenceAll.OverlayApm.RemoveAi)
                    {
                        if (GInformation.Player[i].Type.Equals(PlayerType.Ai))
                            continue;
                    }

                    if (PSettings.PreferenceAll.OverlayApm.RemoveNeutral)
                    {
                        if (GInformation.Player[i].Type.Equals(PlayerType.Neutral))
                            continue;
                    }

                    if (PSettings.PreferenceAll.OverlayApm.RemoveAllie)
                    {
                        if (Player.LocalPlayer.Index == 16)
                        {
                            //Do nothing
                        }

                        else
                        {
                            if (GInformation.Player[i].Team ==
                                Player.LocalPlayer.Team &&
                                GInformation.Player[i].Index != Player.LocalPlayer.Index)
                                continue;
                        }
                    }

                    if (PSettings.PreferenceAll.OverlayApm.RemoveLocalplayer)
                    {
                        if (Player.LocalPlayer == GInformation.Player[i])
                            continue;
                    }



                    #endregion

                    #region Draw Bounds and Background

                    if (PSettings.PreferenceAll.OverlayApm.DrawBackground)
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

                    var strName = (GInformation.Player[i].ClanTag.StartsWith("\0") || PSettings.PreferenceAll.OverlayApm.RemoveClanTag)
                                         ? GInformation.Player[i].Name
                                         : "[" + GInformation.Player[i].ClanTag + "] " + GInformation.Player[i].Name;

                    g.Graphics.DrawString(
                        strName,
                        fInternalFont,
                        new SolidBrush(clPlayercolor),
                        Brushes.Black, (float)((1.67 / 100) * Width),
                        (float)((24.0 / 100) * iSingleHeight) + iSingleHeight * iCounter,
                        1f, 1f, true);

                    #endregion

                    #region Team

                    g.Graphics.DrawString(
                        "#" + GInformation.Player[i].Team, fInternalFontNormal,
                        Brushes.White,
                        Brushes.Black, (float)((29.67 / 100) * Width),
                        (float)((24.0 / 100) * iSingleHeight) + iSingleHeight * iCounter,
                        1f, 1f, true);

                    #endregion

                    #region Apm

                    g.Graphics.DrawString(
                        "APM: " + GInformation.Player[i].ApmAverage +
                        " [" + GInformation.Player[i].Apm + "]", fInternalFontNormal,
                        Brushes.White,
                        Brushes.Black, (float)((37.0 / 100) * Width),
                        (float)((24.0 / 100) * iSingleHeight) + iSingleHeight * iCounter,
                        1f, 1f, true);


                    #endregion

                    #region Epm

                    g.Graphics.DrawString(
                       "EPM: " + GInformation.Player[i].EpmAverage +
                        " [" + GInformation.Player[i].Epm + "]", fInternalFontNormal,
                        Brushes.White,
                        Brushes.Black, (float)((63.67 / 100) * Width),
                                          (float)((24.0 / 100) * iSingleHeight) + iSingleHeight * iCounter,
                        1f, 1f, true);

                    #endregion

                    #endregion


                    iCounter++;
                }

            }

            catch (Exception ex)
            {
                Logger.Emit(ex);
            }

        }

        /// <summary>
        /// Sends the panel specific data into the Form's controls and settings
        /// </summary>
        protected override void MouseUpTransferData()
        {
            /* Has to be calculated manually because each panels has it's own Neutral handling.. */
            var iValidPlayerCount = GInformation.Gameinfo.ValidPlayerCount;

            PSettings.PreferenceAll.OverlayApm.X  = Location.X;
            PSettings.PreferenceAll.OverlayApm.Y  = Location.Y;
            PSettings.PreferenceAll.OverlayApm.Width = Width;
            PSettings.PreferenceAll.OverlayApm.Height = Height / iValidPlayerCount;
            PSettings.PreferenceAll.OverlayApm.Opacity = Opacity;
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

                PSettings.PreferenceAll.OverlayApm.Width = Cursor.Position.X - Left;

                var iValidPlayerCount = GInformation.Gameinfo.ValidPlayerCount;
                if (PSettings.PreferenceAll.OverlayApm.RemoveNeutral)
                    iValidPlayerCount -= 1;

                if ((Cursor.Position.Y - Top) / iValidPlayerCount >= 5)
                {
                    PSettings.PreferenceAll.OverlayApm.Height = (Cursor.Position.Y - Top) /
                                                        iValidPlayerCount;
                }

                else
                    PSettings.PreferenceAll.OverlayApm.Height = 5;
            }

            var strInput = StrBackupSizeChatbox;

            if (String.IsNullOrEmpty(strInput))
                return;

            if (strInput.Contains('\0'))
                strInput = strInput.Substring(0, strInput.IndexOf('\0'));


            if (strInput.Equals(PSettings.PreferenceAll.OverlayApm.ChangeSize))
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
            Location = new Point(PSettings.PreferenceAll.OverlayApm.X,
                                     PSettings.PreferenceAll.OverlayApm.Y);
            Size = new Size(PSettings.PreferenceAll.OverlayApm.Width, PSettings.PreferenceAll.OverlayApm.Height);
            Opacity = PSettings.PreferenceAll.OverlayApm.Opacity;
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
                PSettings.PreferenceAll.OverlayApm.X = Cursor.Position.X;
                PSettings.PreferenceAll.OverlayApm.Y = Cursor.Position.Y;
            }

            var strInput = StrBackupChatbox;

            if (String.IsNullOrEmpty(strInput))
                return;

            if (strInput.Contains('\0'))
                strInput = strInput.Substring(0, strInput.IndexOf('\0'));

            if (strInput.Equals(PSettings.PreferenceAll.OverlayApm.ChangePosition))
            {
                if (BTogglePosition)
                {
                    BTogglePosition = !BTogglePosition;

                    if (!BSetPosition)
                        BSetPosition = true;
                }
            }

            if (HelpFunctions.HotkeysPressed(Keys.Enter, Keys.Enter, Keys.Enter))
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

        void ApmRenderer_IsHiddenChanged(object sender, EventArgs e)
        {
            PSettings.PreferenceAll.OverlayApm.LaunchStatus = !IsHidden;
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

            PSettings.PreferenceAll.OverlayApm.Height = (Height / iRealPlayerCount);
            PSettings.PreferenceAll.OverlayApm.Width = Width;
            PSettings.PreferenceAll.OverlayApm.X = Location.X;
            PSettings.PreferenceAll.OverlayApm.Y = Location.Y;
        }
    }
}
