﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using AnotherSc2Hack.Classes.BackEnds;
using AnotherSc2Hack.Classes.FrontEnds.Rendering;
using PluginInterface;
using Predefined;

namespace AnotherSc2Hack.Classes.FrontEnds.MainHandler
{
    public partial class MainHandler : Form
    {
        #region Variables - Properties

        #region Private 

        
        private Renderer _rMaphack;
        private Renderer _rUnit;
        private Renderer _rResources;
        private Renderer _rIncome;
        private Renderer _rWorker;
        private Renderer _rApm;
        private Renderer _rArmy;
        private Renderer _rProduction;
        private Renderer _rLostUnits;
        private Renderer _rPersonalApm;
        private Renderer _rPersonalClock;
        private Renderer _rPersonalSelection;
        private PredefinedData.Automation _aWorkerProduction;


        private const String StrOnlinePath =
            "https://dl.dropboxusercontent.com/u/62845853/AnotherSc2Hack/UpdateFiles/Sc2Hack_Version";

        private const String StrOnlinePathUpdater =
            "https://dl.dropboxusercontent.com/u/62845853/AnotherSc2Hack/UpdateFiles/Sc2Hack_Updater";

        private const String StrOnlinePublicInformation =
            "https://dl.dropboxusercontent.com/u/62845853/AnotherSc2Hack/UpdateFiles/Sc2Hack_PublicInformation";

        private String _strDownloadString = String.Empty;

        private Boolean _bProcessSet;

        private Boolean _bDevSet;

        private Stopwatch _swMainWatch = new Stopwatch();


        #endregion

        #region Public

        //public Process PSc2Process = null;                                                      //Will be accessable

        //public GameInfo GInformation;                              //Refrehes itself

        #endregion

        #endregion

        private readonly List<IPlugins> _lPlugins = new List<IPlugins>(); 

        public Preferences PSettings { get; set; }
        public GameInfo GInformation { get; set; }
        public Process PSc2Process { get; set; }

        public MainHandler()
        {
            InitializeComponent();

            LoadPlugins();

            GInformation = new GameInfo();

            //Clipboard.SetText((606665725 >> 5).ToString());
            //Clipboard.SetText((606665725 & 0xFFFFFFFC).ToString());
            //PSettings = HelpFunctions.GetPreferences();
            PSettings = new Preferences();
            PSettings.ReadPreferences();
            PSc2Process = GInformation.CStarcraft2;
            GInformation.CSleepTime = PSettings.GlobalDataRefresh;

            SetImageCombolist();
            AssignMethodsToEvents();
            LoadSettingsIntoControls();
            
            //_rTrainer = new Renderer(PredefinedTypes.RenderForm.Trainer, this);
            //_rTrainer.Show();
         
            /* Stuff that gets downloaded.. */
            new Thread(InitSearch).Start();             //Thread for the Updater [Mainapplication]
            new Thread(InitSearchUpdater).Start();      //Thread for the Updater [Updater]
            new Thread(GetPublicInformation).Start();   //Thread for public information [Mainapplication]
            

            HelpFunctions.CheckIfWindowStyleIsFullscreen(GInformation.CWindowStyle);
            HelpFunctions.CheckIfDwmIsEnabled();


            /* Set title for Panels */
            Text = HelpFunctions.SetWindowTitle();

           

            //Automation am = new Automation(this, PredefinedTypes.Automation.WorkerProduction);

            /* Finally, enable the timer */
            tmrGatherInformation.Enabled = true;

            //Automation am = new Automation(this, PredefinedTypes.Automation.Inject);
            //Automation am = new Automation(this, PredefinedTypes.Automation.Production);

       
#if DEBUG
            //var am = new Automation(this, PredefinedTypes.Automation.Testing);
            btnLostUnits.Visible = true;
            

#else 
            tcWorkerAutomation.Enabled = false;
            tcMainTab.TabPages.Remove(tcDebug);
#endif


        }

        public override sealed string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        #region Launch Panels

        #region Buttons

        private void btnMaphack_Click(object sender, EventArgs e)
        {
            HandleButtonClicks(ref _rMaphack, PredefinedData.RenderForm.Maphack);
        }

        private void btnUnit_Click(object sender, EventArgs e)
        {
            HandleButtonClicks(ref _rUnit, PredefinedData.RenderForm.Units);
        }

        private void btnResources_Click(object sender, EventArgs e)
        {
            HandleButtonClicks(ref _rResources, PredefinedData.RenderForm.Resources);
        }

        private void btnIncome_Click(object sender, EventArgs e)
        {
            HandleButtonClicks(ref _rIncome, PredefinedData.RenderForm.Income);
        }

        private void btnArmy_Click(object sender, EventArgs e)
        {
            HandleButtonClicks(ref _rArmy, PredefinedData.RenderForm.Army);
        }

        private void btnApm_Click(object sender, EventArgs e)
        {
            HandleButtonClicks(ref _rApm, PredefinedData.RenderForm.Apm);
        }

        private void btnWorker_Click(object sender, EventArgs e)
        {
            HandleButtonClicks(ref _rWorker, PredefinedData.RenderForm.Worker);
        }

        private void btnProduction_Click(object sender, EventArgs e)
        {
            HandleButtonClicks(ref _rProduction, PredefinedData.RenderForm.Production);
        }

        #endregion

        #region Chat Input / Hotkeys / Button- method

        private void LaunchPanels(ref Renderer ren, PredefinedData.RenderForm typo, String shortcut, Keys a, Keys b,
                                  Keys c)
        {

            #region Hotkeys

            if (HelpFunctions.HotkeysPressed(a, b, c))
            {
                if (ren == null)
                    ren = new Renderer(typo, this);

                if (ren.Created)
                    ren.Close();


                else
                {
                    if (ren.IsDestroyed)
                        ren = new Renderer(typo, this);

                    ren.Text = HelpFunctions.SetWindowTitle();
                    ren.Show();
                }

                Thread.Sleep(200);
            }

            #endregion

            if (PSc2Process == null)
                return;

            #region Chatinput

            var strInput = GInformation.Gameinfo.ChatInput;



            if (String.IsNullOrEmpty(strInput))
                return;

            if (strInput.Contains('\0'))
                strInput = strInput.Substring(0, strInput.IndexOf('\0'));

            if (strInput.Equals(shortcut))
            {
                if (ren == null)
                    ren = new Renderer(typo, this);

                if (ren.Created)
                    ren.Close();


                else
                {
                    if (ren.IsDestroyed)
                        ren = new Renderer(typo, this);

                    ren.Text = HelpFunctions.SetWindowTitle();
                    ren.Show();
                }

                Simulation.Keyboard.Keyboard_SimulateKey(PSc2Process.MainWindowHandle, Keys.Enter, 3);
            }





            #endregion
        }

        private void HandleButtonClicks(ref Renderer ren, PredefinedData.RenderForm typo)
        {

            if (ren == null)
                ren = new Renderer(typo, this);

            if (ren.Created)
                ren.Close();
            

            else
            {
                if (ren.IsDestroyed)
                    ren = new Renderer(typo, this);

                ren.Text = HelpFunctions.SetWindowTitle();
                ren.Show();
            }
        }

        #endregion

        #region Launch Personal APM/ Clock

        #endregion

        #endregion

        /* Make panels in/ visible */
        private void ChangeVisibleState(Boolean state)
        {
            if (_rApm != null)
                _rApm.Visible = state;

            if (_rArmy != null)
                _rArmy.Visible = state;

            if (_rIncome != null)
                _rIncome.Visible = state;

            if (_rMaphack != null)
                _rMaphack.Visible = state;

            if (_rProduction != null)
                _rProduction.Visible = state;

            if (_rResources != null)
                _rResources.Visible = state;

            if (_rUnit != null)
                _rUnit.Visible = state;

            if (_rWorker != null)
                _rWorker.Visible = state;
        }

        /* Change Textbox Content (Width, Height, PosX, PosY) */
        private void ChangeTextboxInformation()
        {
            /* Resource */
            ResourceInformation.txtPosX.Text = PSettings.ResourcePositionX.ToString(CultureInfo.InvariantCulture);
            ResourceInformation.txtPosY.Text = PSettings.ResourcePositionY.ToString(CultureInfo.InvariantCulture);
            ResourceInformation.txtWidth.Text = PSettings.ResourceWidth.ToString(CultureInfo.InvariantCulture);
            ResourceInformation.txtHeight.Text = PSettings.ResourceHeight.ToString(CultureInfo.InvariantCulture);

            /* Income */
            IncomeInformation.txtPosX.Text = PSettings.IncomePositionX.ToString(CultureInfo.InvariantCulture);
            IncomeInformation.txtPosY.Text = PSettings.IncomePositionY.ToString(CultureInfo.InvariantCulture);
            IncomeInformation.txtWidth.Text = PSettings.IncomeWidth.ToString(CultureInfo.InvariantCulture);
            IncomeInformation.txtHeight.Text = PSettings.IncomeHeight.ToString(CultureInfo.InvariantCulture);

            /* Worker */
            WorkerInformation.txtPosX.Text = PSettings.WorkerPositionX.ToString(CultureInfo.InvariantCulture);
            WorkerInformation.txtPosY.Text = PSettings.WorkerPositionY.ToString(CultureInfo.InvariantCulture);
            WorkerInformation.txtWidth.Text = PSettings.WorkerWidth.ToString(CultureInfo.InvariantCulture);
            WorkerInformation.txtHeight.Text = PSettings.WorkerHeight.ToString(CultureInfo.InvariantCulture);

            /* Maphack */
            MaphackInformation.txtPosX.Text = PSettings.MaphackPositionX.ToString(CultureInfo.InvariantCulture);
            MaphackInformation.txtPosY.Text = PSettings.MaphackPositionY.ToString(CultureInfo.InvariantCulture);
            MaphackInformation.txtWidth.Text = PSettings.MaphackWidth.ToString(CultureInfo.InvariantCulture);
            MaphackInformation.txtHeight.Text = PSettings.MaphackHeight.ToString(CultureInfo.InvariantCulture);

            /* Apm */
            ApmInformation.txtPosX.Text = PSettings.ApmPositionX.ToString(CultureInfo.InvariantCulture);
            ApmInformation.txtPosY.Text = PSettings.ApmPositionY.ToString(CultureInfo.InvariantCulture);
            ApmInformation.txtWidth.Text = PSettings.ApmWidth.ToString(CultureInfo.InvariantCulture);
            ApmInformation.txtHeight.Text = PSettings.ApmHeight.ToString(CultureInfo.InvariantCulture);

            /* Army */
            ArmyInformation.txtPosX.Text = PSettings.ArmyPositionX.ToString(CultureInfo.InvariantCulture);
            ArmyInformation.txtPosY.Text = PSettings.ArmyPositionY.ToString(CultureInfo.InvariantCulture);
            ArmyInformation.txtWidth.Text = PSettings.ArmyWidth.ToString(CultureInfo.InvariantCulture);
            ArmyInformation.txtHeight.Text = PSettings.ArmyHeight.ToString(CultureInfo.InvariantCulture);

            /* UnitTab */
            UnittabInformation.txtPosX.Text = PSettings.UnitTabPositionX.ToString(CultureInfo.InvariantCulture);
            UnittabInformation.txtPosY.Text = PSettings.UnitTabPositionY.ToString(CultureInfo.InvariantCulture);
            UnittabInformation.txtWidth.Text = PSettings.UnitTabWidth.ToString(CultureInfo.InvariantCulture);
            UnittabInformation.txtHeight.Text = PSettings.UnitTabHeight.ToString(CultureInfo.InvariantCulture);

            /* Production */
            ProductionTabInformation.txtPosX.Text = PSettings.ProdTabPositionX.ToString(CultureInfo.InvariantCulture);
            ProductionTabInformation.txtPosY.Text = PSettings.ProdTabPositionY.ToString(CultureInfo.InvariantCulture);
            ProductionTabInformation.txtWidth.Text = PSettings.ProdTabWidth.ToString(CultureInfo.InvariantCulture);
            ProductionTabInformation.txtHeight.Text = PSettings.ProdTabHeight.ToString(CultureInfo.InvariantCulture);
        }

        /* SetDrawingrefresh- rate */
        private void SetDrawingRefresh(Renderer renderForm, int iDummy)
        {
            if (renderForm != null &&
                renderForm.Created)
            {
                renderForm.tmrRefreshGraphic.Interval = iDummy;
            }

        }

        private void CheckIfDeveloper()
        {
            if (!_bDevSet)
            {
                if (PSettings.GlobalIsDeveloper.Equals(PredefinedData.IsDeveloper.Dunno))
                {
                    //Do nothing
                }

                else if (PSettings.GlobalIsDeveloper.Equals(PredefinedData.IsDeveloper.True))
                {
                    btnProduction.Enabled = true;
                    btnProduction.Visible = true;

                    _bDevSet = true;
                }

                else
                    _bDevSet = true;


            }
        }

        #region Throw Settings into controls

        /* Load all control- settings into the form */
        private void LoadSettingsIntoControls()
        {
            #region Resources

            ResourceBasics.cmBxRemAi.Text = PSettings.ResourceRemoveAi.ToString();
            ResourceBasics.cmBxRemAllie.Text = PSettings.ResourceRemoveAllie.ToString();
            ResourceBasics.cmBxRemNeutral.Text = PSettings.ResourceRemoveNeutral.ToString();
            ResourceBasics.cmBxRemLocalplayer.Text = PSettings.ResourceRemoveLocalplayer.ToString();
            ResourceBasics.btnFontName.Text = PSettings.ResourceFontName;
            ResourceBasics.btnFontName.Font = new Font(PSettings.ResourceFontName, Font.Size);
            ResourceBasics.tbOpacity.Value = PSettings.ResourceOpacity > 1.0
                ? (Int32) PSettings.ResourceOpacity
                : (Int32) (PSettings.ResourceOpacity*100);
            //ResourceBasics.tbOpacity.Value = (Int32)(PSettings.ResourceOpacity * 100);
            ResourceBasics.lblOpacity.Text = "Opacity: " + ResourceBasics.tbOpacity.Value.ToString(CultureInfo.InvariantCulture) + "%";
            ResourceChatInput.txtToggle.Text = PSettings.ResourceTogglePanel;
            ResourceChatInput.txtPosition.Text = PSettings.ResourceChangePositionPanel;
            ResourceChatInput.txtSize.Text = PSettings.ResourceChangeSizePanel;
            ResourceHotkeys.txtHotkey1.Text = PSettings.ResourceHotkey1.ToString();
            ResourceHotkeys.txtHotkey2.Text = PSettings.ResourceHotkey2.ToString();
            ResourceHotkeys.txtHotkey3.Text = PSettings.ResourceHotkey3.ToString();
            ResourceBasics.chBxDrawBackground.Checked = PSettings.ResourceDrawBackground;
            ResourceBasics.cmBxRemClanTag.Text = PSettings.ResourceRemoveClanTag.ToString();

            #endregion

            #region Income

            IncomeBasics.cmBxRemAi.Text = PSettings.IncomeRemoveAi.ToString();
            IncomeBasics.cmBxRemAllie.Text = PSettings.IncomeRemoveAllie.ToString();
            IncomeBasics.cmBxRemNeutral.Text = PSettings.IncomeRemoveNeutral.ToString();
            IncomeBasics.cmBxRemLocalplayer.Text = PSettings.IncomeRemoveLocalplayer.ToString();
            IncomeBasics.btnFontName.Text = PSettings.IncomeFontName;
            IncomeBasics.btnFontName.Font = new Font(PSettings.IncomeFontName, Font.Size);
            IncomeBasics.tbOpacity.Value = PSettings.IncomeOpacity > 1.0
                ? (Int32)PSettings.IncomeOpacity
                : (Int32)(PSettings.IncomeOpacity * 100);
            IncomeBasics.lblOpacity.Text = "Opacity: " + IncomeBasics.tbOpacity.Value.ToString(CultureInfo.InvariantCulture) + "%";
            IncomeChatInput.txtToggle.Text = PSettings.IncomeTogglePanel;
            IncomeChatInput.txtPosition.Text = PSettings.IncomeChangePositionPanel;
            IncomeChatInput.txtSize.Text = PSettings.IncomeChangeSizePanel;
            IncomeHotkeys.txtHotkey1.Text = PSettings.IncomeHotkey1.ToString();
            IncomeHotkeys.txtHotkey2.Text = PSettings.IncomeHotkey2.ToString();
            IncomeHotkeys.txtHotkey3.Text = PSettings.IncomeHotkey3.ToString();
            IncomeBasics.chBxDrawBackground.Checked = PSettings.IncomeDrawBackground;
            IncomeBasics.cmBxRemClanTag.Text = PSettings.IncomeRemoveClanTag.ToString();

            #endregion

            #region Army

            ArmyBasics.cmBxRemAi.Text = PSettings.ArmyRemoveAi.ToString();
            ArmyBasics.cmBxRemAllie.Text = PSettings.ArmyRemoveAllie.ToString();
            ArmyBasics.cmBxRemNeutral.Text = PSettings.ArmyRemoveNeutral.ToString();
            ArmyBasics.cmBxRemLocalplayer.Text = PSettings.ArmyRemoveLocalplayer.ToString();
            ArmyBasics.btnFontName.Text = PSettings.ArmyFontName;
            ArmyBasics.btnFontName.Font = new Font(PSettings.ArmyFontName, Font.Size);
            ArmyBasics.tbOpacity.Value = PSettings.ArmyOpacity > 1.0
                ? (Int32)PSettings.ArmyOpacity
                : (Int32)(PSettings.ArmyOpacity * 100);
            ArmyBasics.lblOpacity.Text = "Opacity: " + ArmyBasics.tbOpacity.Value.ToString(CultureInfo.InvariantCulture) + "%";
            ArmyChatInput.txtToggle.Text = PSettings.ArmyTogglePanel;
            ArmyChatInput.txtPosition.Text = PSettings.ArmyChangePositionPanel;
            ArmyChatInput.txtSize.Text = PSettings.ArmyChangeSizePanel;
            ArmyHotkeys.txtHotkey1.Text = PSettings.ArmyHotkey1.ToString();
            ArmyHotkeys.txtHotkey2.Text = PSettings.ArmyHotkey2.ToString();
            ArmyHotkeys.txtHotkey3.Text = PSettings.ArmyHotkey3.ToString();
            ArmyBasics.chBxDrawBackground.Checked = PSettings.ArmyDrawBackground;
            ArmyBasics.cmBxRemClanTag.Text = PSettings.ArmyRemoveClanTag.ToString();

            #endregion

            #region Apm

            ApmBasics.cmBxRemAi.Text = PSettings.ApmRemoveAi.ToString();
            ApmBasics.cmBxRemAllie.Text = PSettings.ApmRemoveAllie.ToString();
            ApmBasics.cmBxRemNeutral.Text = PSettings.ApmRemoveNeutral.ToString();
            ApmBasics.cmBxRemLocalplayer.Text = PSettings.ApmRemoveLocalplayer.ToString();
            ApmBasics.btnFontName.Text = PSettings.ApmFontName;
            ApmBasics.btnFontName.Font = new Font(PSettings.ApmFontName, Font.Size);
            ApmBasics.tbOpacity.Value = PSettings.ApmOpacity > 1.0
                ? (Int32)PSettings.ApmOpacity
                : (Int32)(PSettings.ApmOpacity * 100);
            ApmBasics.lblOpacity.Text = "Opacity: " + ApmBasics.tbOpacity.Value.ToString(CultureInfo.InvariantCulture) + "%";
            ApmChatInput.txtToggle.Text = PSettings.ApmTogglePanel;
            ApmChatInput.txtPosition.Text = PSettings.ApmChangePositionPanel;
            ApmChatInput.txtSize.Text = PSettings.ApmChangeSizePanel;
            ApmHotkeys.txtHotkey1.Text = PSettings.ApmHotkey1.ToString();
            ApmHotkeys.txtHotkey2.Text = PSettings.ApmHotkey2.ToString();
            ApmHotkeys.txtHotkey3.Text = PSettings.ApmHotkey3.ToString();
            ApmBasics.chBxDrawBackground.Checked = PSettings.ApmDrawBackground;
            ApmBasics.cmBxRemClanTag.Text = PSettings.ApmRemoveClanTag.ToString();

            #endregion

            #region Worker

            WorkerBasics.btnFontName.Text = PSettings.WorkerFontName;
            WorkerBasics.btnFontName.Font = new Font(PSettings.WorkerFontName, Font.Size);
            WorkerBasics.tbOpacity.Value = PSettings.WorkerOpacity > 1.0
                ? (Int32)PSettings.WorkerOpacity
                : (Int32)(PSettings.WorkerOpacity * 100);
            WorkerBasics.lblOpacity.Text = "Opacity: " + WorkerBasics.tbOpacity.Value.ToString(CultureInfo.InvariantCulture) + "%";
            WorkerChatInput.txtToggle.Text = PSettings.WorkerTogglePanel;
            WorkerChatInput.txtPosition.Text = PSettings.WorkerChangePositionPanel;
            WorkerChatInput.txtSize.Text = PSettings.WorkerChangeSizePanel;
            WorkerHotkeys.txtHotkey1.Text = PSettings.WorkerHotkey1.ToString();
            WorkerHotkeys.txtHotkey2.Text = PSettings.WorkerHotkey2.ToString();
            WorkerHotkeys.txtHotkey3.Text = PSettings.WorkerHotkey3.ToString();
            WorkerBasics.chBxDrawBackground.Checked = PSettings.WorkerDrawBackground;

            #endregion

            #region Maphack

            MaphackBasics.cmBxRemAi.Text = PSettings.MaphackRemoveAi.ToString();
            MaphackBasics.cmBxRemAllie.Text = PSettings.MaphackRemoveAllie.ToString();
            MaphackBasics.cmBxRemNeutral.Text = PSettings.MaphackRemoveNeutral.ToString();
            MaphackBasics.cmBxRemLocalplayer.Text = PSettings.MaphackRemoveLocalplayer.ToString();
            MaphackBasics.tbOpacity.Value = PSettings.MaphackOpacity > 1.0
                ? (Int32)PSettings.MaphackOpacity
                : (Int32)(PSettings.MaphackOpacity * 100);
            MaphackBasics.lblOpacity.Text = "Opacity: " + MaphackBasics.tbOpacity.Value.ToString(CultureInfo.InvariantCulture) + "%";
            MaphackBasics.btnDestinationLine.BackColor = PSettings.MaphackDestinationColor;
            MaphackChatInput.txtToggle.Text = PSettings.MaphackTogglePanel;
            MaphackChatInput.txtPosition.Text = PSettings.MaphackChangePositionPanel;
            MaphackChatInput.txtSize.Text = PSettings.MaphackChangeSizePanel;
            MaphackHotkeys.txtHotkey1.Text = PSettings.MaphackHotkey1.ToString();
            MaphackHotkeys.txtHotkey2.Text = PSettings.MaphackHotkey2.ToString();
            MaphackHotkeys.txtHotkey3.Text = PSettings.MaphackHotkey3.ToString();
            MaphackBasics.chBxMaphackDisableDestinationLine.Checked = PSettings.MaphackDisableDestinationLine;
            MaphackBasics.chBxMaphackColorDefensiveStructuresYellow.Checked = PSettings.MaphackColorDefensivestructuresYellow;
            MaphackBasics.chBxMaphackRemVisionArea.Checked = PSettings.MaphackRemoveVisionArea;
            MaphackBasics.chBxMaphackRemCamera.Checked = PSettings.MaphackRemoveCamera;

            

            /* UnitIds */
            if (PSettings.MaphackUnitIds != null &&
                PSettings.MaphackUnitColors != null)
            {
                if (PSettings.MaphackUnitIds.Count > 0 &&
                    PSettings.MaphackUnitColors.Count > 0)
                {

                    foreach (var t in PSettings.MaphackUnitIds)
                        lstMapUnits.Items.Add(t.ToString());
                }
            }

            #endregion

            #region Unittab

            UnittabBasics.cmBxRemAi.Text = PSettings.UnitTabRemoveAi.ToString();
            UnittabBasics.cmBxRemAllie.Text = PSettings.UnitTabRemoveAllie.ToString();
            UnittabBasics.cmBxRemNeutral.Text = PSettings.UnitTabRemoveNeutral.ToString();
            UnittabBasics.cmBxRemLocalplayer.Text = PSettings.UnitTabRemoveLocalplayer.ToString();
            UnittabBasics.cmBxSplitBuildings.Text = PSettings.UnitTabSplitUnitsAndBuildings.ToString();
            UnittabBasics.tbOpacity.Value = PSettings.UnitTabOpacity > 1.0
                ? (Int32)PSettings.UnitTabOpacity
                : (Int32)(PSettings.UnitTabOpacity * 100);
            UnittabBasics.lblOpacity.Text = "Opacity: " + UnittabBasics.tbOpacity.Value.ToString(CultureInfo.InvariantCulture) + "%";
            UnittabChatInput.txtToggle.Text = PSettings.UnitTogglePanel;
            UnittabChatInput.txtPosition.Text = PSettings.UnitChangePositionPanel;
            UnittabChatInput.txtSize.Text = PSettings.UnitChangeSizePanel;
            UnittabHotkeys.txtHotkey1.Text = PSettings.UnitHotkey1.ToString();
            UnittabHotkeys.txtHotkey2.Text = PSettings.UnitHotkey2.ToString();
            UnittabHotkeys.txtHotkey3.Text = PSettings.UnitHotkey3.ToString();
            txtUnitPictureSize.Text = PSettings.UnitPictureSize.ToString(CultureInfo.InvariantCulture);
            UnittabBasics.btnFontName.Text = PSettings.UnitTabFontName;
            UnittabBasics.btnFontName.Font = new Font(PSettings.UnitTabFontName, Font.Size);
            UnittabBasics.cmBxRemClanTag.Text = PSettings.UnitTabRemoveClanTag.ToString();
            UnittabBasics.cmBxRemProdLine.Text = PSettings.UnitTabRemoveProdLine.ToString();
            chBxUnitTabShowUnits.Checked = PSettings.UnitShowUnits;
            chBxUnitTabShowBuildings.Checked = PSettings.UnitShowBuildings;
            UnittabBasics.cmBxRemChronoboost.Text = PSettings.UnitTabRemoveChronoboost.ToString();
            UnittabBasics.cmBxRemSpellCounter.Text = PSettings.UnitTabRemoveSpellCounter.ToString();

            #endregion

            #region Productiontab

            ProductionTabBasics.cmBxRemAi.Text = PSettings.ProdTabRemoveAi.ToString();
            ProductionTabBasics.cmBxRemAllie.Text = PSettings.ProdTabRemoveAllie.ToString();
            ProductionTabBasics.cmBxRemNeutral.Text = PSettings.ProdTabRemoveNeutral.ToString();
            ProductionTabBasics.cmBxRemLocalplayer.Text = PSettings.ProdTabRemoveLocalplayer.ToString();
            ProductionTabBasics.cmBxSplitBuildings.Text = PSettings.ProdTabSplitUnitsAndBuildings.ToString();
            ProductionTabBasics.tbOpacity.Value = PSettings.ProdTabOpacity > 1.0
                ? (Int32)PSettings.ProdTabOpacity
                : (Int32)(PSettings.ProdTabOpacity * 100);
            ProductionTabBasics.lblOpacity.Text = "Opacity: " + ProductionTabBasics.tbOpacity.Value.ToString(CultureInfo.InvariantCulture) + "%";
            ProductionTabChatInput.txtToggle.Text = PSettings.ProdTogglePanel;
            ProductionTabChatInput.txtPosition.Text = PSettings.ProdChangePositionPanel;
            ProductionTabChatInput.txtSize.Text = PSettings.ProdChangeSizePanel;
            ProductionTabHotkeys.txtHotkey1.Text = PSettings.ProdHotkey1.ToString();
            ProductionTabHotkeys.txtHotkey2.Text = PSettings.ProdHotkey2.ToString();
            ProductionTabHotkeys.txtHotkey3.Text = PSettings.ProdHotkey3.ToString();
            txtProductionTabPictureSize.Text = PSettings.ProdPictureSize.ToString(CultureInfo.InvariantCulture);
            ProductionTabBasics.btnFontName.Text = PSettings.ProdTabFontName;
            ProductionTabBasics.btnFontName.Font = new Font(PSettings.ProdTabFontName, Font.Size);
            ProductionTabBasics.cmBxRemClanTag.Text = PSettings.ProdTabRemoveClanTag.ToString();
            chBxProdTabShowBuildings.Checked = PSettings.ProdTabShowBuildings;
            chBxProdTabShowUnits.Checked = PSettings.ProdTabShowUnits;
            chBxProdTabShowUpgrades.Checked = PSettings.ProdTabShowUpgrades;
            ProductionTabBasics.cmBxRemChronoboost.Text = PSettings.ProdTabRemoveChronoboost.ToString();

            #endregion

            #region Worker Automation

            workerProductionBasics.chBxAutomationEnableWorkerProduction.Checked = PSettings.WorkerAutomation;
            workerProductionBasics.chBxAutoUpgradeToOc.Checked = PSettings.WorkerAutomationAutoupgradeToOc;
            workerProductionBasics.chBxDisableWhenSelecting.Checked = PSettings.WorkerAutomationDisableWhenSelecting;
            workerProductionBasics.chBxDisableWhenWorkerIsSelected.Checked =
                PSettings.WorkerAutomationDisableWhenWorkerIsSelected;
            workerProductionBasics.ntxtDisableWhenApmIsOver.Text = PSettings.WorkerAutomationApmProtection.ToString(CultureInfo.InvariantCulture);
            workerProductionBasics.ntxtMaximumWorkersInGame.Text = PSettings.WorkerAutomationMaximumWorkers.ToString(CultureInfo.InvariantCulture);
            workerProductionBasics.ntxtMaximumWorkersPerBase.Text =
                PSettings.WorkerAutomationMaximumWorkersPerBase.ToString(CultureInfo.InvariantCulture);
            workerProductionBasics.ntxtMaynardWorkerCount.Text = PSettings.WorkerAutomationPufferWorker.ToString(CultureInfo.InvariantCulture);
            workerProductionBasics.rdbDirectWorkerProduction.Checked = PSettings.WorkerAutomationModeDirect;
            workerProductionBasics.rdbRoundWorkerProduction.Checked = PSettings.WorkerAutomationModeRound;
            workerProductionBasics.ktxtBackupGroupKey.Text = PSettings.WorkerAutomationBackupGroup.ToString();
            workerProductionBasics.ktxtMainbuildingGroupKey.Text =
                PSettings.WorkerAutomationMainbuildingGroup.ToString();
            workerProductionBasics.ktxtOrbitalUpgradeKey.Text = PSettings.WorkerAutomationOrbitalKey.ToString();
            workerProductionBasics.ktxtProbeBuildingKey.Text = PSettings.WorkerAutomationProbeKey.ToString();
            workerProductionBasics.ktxtScvBuildingKey.Text = PSettings.WorkerAutomationScvKey.ToString();
            workerProductionHotkeys.txtHotkey1.Text = PSettings.WorkerAutomationHotkey1.ToString();
            workerProductionHotkeys.txtHotkey2.Text = PSettings.WorkerAutomationHotkey2.ToString();
            workerProductionHotkeys.txtHotkey3.Text = PSettings.WorkerAutomationHotkey3.ToString();
            workerProductionBasics.ntxtBuildNextWorkerAt.Text = PSettings.WorkerAutomationStartNextWorkerAt.ToString(CultureInfo.InvariantCulture);

            #endregion

            /* Global */
            CustGlobal.txtDataInterval.Text = PSettings.GlobalDataRefresh.ToString(CultureInfo.InvariantCulture);
            CustGlobal.txtDrawingInterval.Text = PSettings.GlobalDrawingRefresh.ToString(CultureInfo.InvariantCulture);
            CustGlobal.chBxGlobalForegroundDraw.Checked = PSettings.GlobalDrawOnlyInForeground;
            CustGlobal.txtGlobalAdjustKey.Text = PSettings.GlobalChangeSizeAndPosition.ToString();
            CustGlobal.cmBxLanguage.Text = PSettings.GlobalLanguage;

            /* Various */
            Custom_Various.chBxApm.Checked = PSettings.PersonalApm;
            Custom_Various.chBxClock.Checked = PSettings.PersonalClock;
            Custom_Various.chBxApmAlert.Checked = PSettings.PersonalApmAlert;
            Custom_Various.txtApmAlertLimit.Text = PSettings.PersonalApmAlertLimit.ToString(CultureInfo.InvariantCulture);
            
            
            /* - Non settings - */
            CustGlobal.lblMainApplication.Text = "[" + Application.ProductName + "] - Ver.: " +
                                      System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            if (File.Exists("Sc2Hack UpdateManager.exe"))
            {
                var fvInfo = FileVersionInfo.GetVersionInfo("Sc2Hack UpdateManager.exe");

                CustGlobal.lblUpdaterApplication.Text = "[" + fvInfo.ProductName + "] - Ver.: " +
                                             fvInfo.FileVersion;
            }

            /* Load the final positions and textboxes */
            ChangeTextboxInformation();
        }

        #endregion

        #region - Methods for the updater -

        #region Check for an Update for the Main Application

        /* Get new Updates */
        private void btnGetUpdate_Click(object sender, EventArgs e)
        {
            if (File.Exists("Sc2Hack UpdateManager.exe"))
            {
                Process.Start("Sc2Hack UpdateManager.exe");
                Close();
            }

            else
            {
                var wc = new WebClient();
                wc.DownloadFileAsync(
                    new Uri(
                        "https://dl.dropboxusercontent.com/u/62845853/AnotherSc2Hack/Binaries/AnotherSc2Hack_0.2.2.0/Sc2Hack%20UpdateManager.exe"),
                    "Sc2Hack UpdateManager.exe");

                wc.DownloadFileCompleted += wc_DownloadFileCompleted;

            }
        }

        /* In case the file is lost.. */
        void wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (File.Exists("Sc2Hack UpdateManager.exe"))
                Process.Start("Sc2Hack UpdateManager.exe");

            else 
                CustGlobal.btnGetUpdate.PerformClick();
            
        }

        /* Initial search for updates */
        public void InitSearch()
        {
            var iCountTimeOuts = 0;

        TryAnotherRound:

            /* We ping the Server first to exclude exceptions! */
            var myPing = new Ping();

            PingReply myResult;


            try
            {
                myResult = myPing.Send("Dropbox.com", 10);
                Debug.WriteLine("Sent ping! (" + iCountTimeOuts.ToString(CultureInfo.InvariantCulture) + ")");
            }

            catch
            {
                    var iCounter = 0;
                TryAnotherInvoke:

                    MethodInvoker inv =
                    delegate
                    {
                        CustGlobal.btnGetUpdate.ForeColor = Color.Red;
                        CustGlobal.btnGetUpdate.Text = "No Connection!";
                        CustGlobal.btnGetUpdate.Enabled = false;
                    };

                

                    try
                    {
                        Invoke(inv);
                    }
                    catch
                    {
                        if (iCounter > 5)
                        return;

                        iCounter += 1;
                        goto TryAnotherInvoke;
                    }

                    return;
            }

            if (myResult != null && myResult.Status != IPStatus.Success)
            {
                if (iCountTimeOuts >= 10)
                {
                    MessageBox.Show("Can not reach Server!\n\nTry later!", "FAILED");
                    return;
                }

                iCountTimeOuts++;
                goto TryAnotherRound;

            }

            Debug.WriteLine("Initiate Webclient!");

            /* Reset Title */
            MethodInvoker invBtn = delegate
                {
                    CustGlobal.btnGetUpdate.Text = "- Searching - ";
                    CustGlobal.btnGetUpdate.ForeColor = Color.Black;
                    CustGlobal.btnGetUpdate.Enabled = false;
                };

            try
            {
                Invoke(invBtn);
            }

            catch
            {}

            /* Connect to server */
            var privateWebClient = new WebClient
                {
                    Proxy = null
                };

            string strSource;

            try
            {
                Debug.WriteLine("Downloading String");
                strSource = privateWebClient.DownloadString(StrOnlinePath);
                //privateWebClient.DownloadFile(StrOnlinePath, Application.StartupPath + "\\tmp");
                //StreamReader sr = new StreamReader(Application.StartupPath + "\\tmp");
                //strSource = sr.ReadToEnd();
                //sr.Close();

                Debug.WriteLine("String downloaded");
            }

            catch
            {
                MessageBox.Show("Can not reach Server!\n\nTry later!", "FAILED");
                Close();
                return;
            }

            /* Build version from this file */
            var curVer = new Version(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

            /* Build version from online- file (string) */
            var newVer = new Version(GetStringItems(0, strSource));

            /* Version- check */
            if (newVer > curVer)
            {
                PSettings.GlobalIsDeveloper = PredefinedData.IsDeveloper.False;
                //_strDownloadString = GetStringItems(1, strSource);

                MethodInvoker inv =
                    delegate
                    {
                        CustGlobal.btnGetUpdate.ForeColor = Color.Green;
                        CustGlobal.btnGetUpdate.Text = "Get Update!";
                        CustGlobal.btnGetUpdate.Enabled = true;
                    };

                byte bCounter = 0;
            InvokeAgain:
                try
                {
                    Invoke(inv);
                }

                catch
                {
                    bCounter++;
                    if (bCounter >= 5)
                        return;

                    goto InvokeAgain;
                }

                return;
            }

            MethodInvoker inv2;
            if (curVer > newVer)
            {
                PSettings.GlobalIsDeveloper = PredefinedData.IsDeveloper.True;
                inv2 = delegate
                    {
                        CustGlobal.btnGetUpdate.Text = "Developer!";
                        CustGlobal.btnGetUpdate.ForeColor = Color.LightBlue;
                        CustGlobal.btnGetUpdate.Enabled = true;
                    };
            }

            else
            {



                inv2 =
                    delegate
                        {
                            CustGlobal.btnGetUpdate.Text = "Up-To-Date!";
                        };
            }

            byte bCounter2 = 0;
        InvokeAgain2:
            try
            {
                Invoke(inv2);
            }

            catch
            {
                bCounter2++;
                if (bCounter2 >= 5)
                    return;

                goto InvokeAgain2;
            }
        }

        /* Parses out a string of Line x */
        private string GetStringItems(int line, string source)
        {
            /* Is Like
              1  Version=0.0.1.0
              2  Path=https://dl.dropbox.com/u/62845853/AnotherSc2Hack/Binaries/Another%20SC2%20Hack.exe
              3  Changes=https://dl.dropbox.com/u/62845853/AnotherSc2Hack/UpdateFiles/Changes */

            var strmoreSource = source.Split('\n');
            if (strmoreSource[line].Contains("\r"))
                strmoreSource[line] = strmoreSource[line].Substring(0, strmoreSource[line].IndexOf('\r'));

            return strmoreSource[line];
        }

        #endregion

        #region Check for an Update for the Updater

        /* Initial search for updates */
        public void InitSearchUpdater()
        {
            var iCountTimeOuts = 0;

        TryAnotherRound:

            /* We ping the Server first to exclude exceptions! */
            var myPing = new Ping();

            PingReply myResult;


            try
            {
                myResult = myPing.Send("Dropbox.com", 10);
                Debug.WriteLine("Sent ping! (" + iCountTimeOuts.ToString(CultureInfo.InvariantCulture) + ")");
            }

            catch
            {
                iCountTimeOuts++;
                goto TryAnotherRound;
            }

            if (myResult != null && myResult.Status != IPStatus.Success)
            {
                if (iCountTimeOuts >= 10)
                {
                    MessageBox.Show("Can not reach Server!\n\nTry later!", "FAILED");
                    return;
                }

                iCountTimeOuts++;
                goto TryAnotherRound;

            }

            Debug.WriteLine("Initiate Webclient!");


            /* Connect to server */
            var privateWebClient = new WebClient
                {
                    Proxy = null
                };

            string strSource;

            try
            {
                Debug.WriteLine("Downloading String");
                strSource = privateWebClient.DownloadString(StrOnlinePathUpdater);
                //privateWebClient.DownloadFile(StrOnlinePath, Application.StartupPath + "\\tmp");
                //StreamReader sr = new StreamReader(Application.StartupPath + "\\tmp");
                //strSource = sr.ReadToEnd();
                //sr.Close();

                Debug.WriteLine("String downloaded");
            }

            catch
            {
                MessageBox.Show("Can not reach Server!\n\nTry later!", "FAILED");
                return;
            }


            /* Build version from this file */
            Version curVer;
            if (File.Exists(Constants.StrUpdaterPath))
            {
                FileVersionInfo inf = FileVersionInfo.GetVersionInfo(Constants.StrUpdaterPath);
                curVer = new Version(inf.FileVersion);
            }

            else
            {
                curVer = new Version(0,0,0,1);
            }
            

            /* Build version from online- file (string) */
            var newVer = new Version(GetStringItems(0, strSource));

            /* Version- check */
            if (newVer > curVer)
            {
                _strDownloadString = GetStringItems(1, strSource);

                privateWebClient.DownloadFileAsync(new Uri(_strDownloadString), Constants.StrUpdaterPath);
                privateWebClient.DownloadFileCompleted += privateWebClient_DownloadFileCompleted;
            }
        }

        void privateWebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            /* Quick and dirty.. */
            if (File.Exists("Sc2Hack UpdateManager.exe"))
            {
                var fvInfo = FileVersionInfo.GetVersionInfo("Sc2Hack UpdateManager.exe");

                MethodInvoker inf = delegate
                    {
                        CustGlobal.lblUpdaterApplication.Text = "[" + fvInfo.ProductName + "] - Ver.: " +
                                                     fvInfo.FileVersion;
                    };

                Invoke(inf);
            }
        }

        #endregion

        


        #endregion

        private void btnLostUnits_Click(object sender, EventArgs e)
        {
            HandleButtonClicks(ref _rLostUnits, PredefinedData.RenderForm.LostUnits); 
        }

        private void CheckPanelState(PredefinedData.RenderForm rnd)
        {
            #region Unitcommands [Production-/ Unit- panel]

            if (rnd.Equals(PredefinedData.RenderForm.Production) ||
                rnd.Equals(PredefinedData.RenderForm.Units))
            {

                if (_rProduction == null &&
                    _rUnit == null)
                {
                    GInformation.CAccessUnitCommands = false;
                    return;
                }

                if (_rProduction != null &&
                    _rUnit != null)
                {
                    if (!_rProduction.Created &&
                        !_rUnit.Created)
                    {
                        GInformation.CAccessUnitCommands = false;
                    }
                }

                else if (_rProduction != null &&
                         _rUnit == null)
                {
                    if (!_rProduction.Created)
                        GInformation.CAccessUnitCommands = false;
                }

                else if (_rProduction == null &&
                         _rUnit != null)
                {
                    if (!_rUnit.Created)
                        GInformation.CAccessUnitCommands = false;
                }

                else
                {
                    if (rnd.Equals(PredefinedData.RenderForm.Units))
                        if (PSettings.UnitTabRemoveProdLine)
                        {
                            GInformation.CAccessUnitCommands = false;
                            return;
                        }

                    GInformation.CAccessUnitCommands = true;
                }

            }

            #endregion
        }

        private void RefreshPluginData()
        {
            if (_lPlugins == null)
                return;


            foreach (var plugin in _lPlugins)
            {
                /* Refresh some Data */
                plugin.SetMap(GInformation.Map);
                plugin.SetPlayers(GInformation.Player);
                plugin.SetUnits(GInformation.Unit);
                plugin.SetSelection(GInformation.Selection);
                plugin.SetGroups(GInformation.Group);
                plugin.SetGameinfo(GInformation.Gameinfo);

                /* Set Access values for Gameinfo */
                GInformation.CAccessPlayers |= plugin.GetRequiresPlayer();
                GInformation.CAccessSelection |= plugin.GetRequiresSelection();
                GInformation.CAccessUnits |= plugin.GetRequiresUnit();
                GInformation.CAccessUnitCommands |= plugin.GetRequiresUnit();
                GInformation.CAccessGameinfo |= plugin.GetRequiresGameinfo();
                GInformation.CAccessGroups |= plugin.GetRequiresGroups();
                GInformation.CAccessMapInfo |= plugin.GetRequiresMap();
            }
        }

        private void LoadPlugins()
        {
            /* Check if dir. exists */
            if (!Directory.Exists(Constants.StrPluginFolder))
            {
                CustGlobal.lstBxPlugins.Items.Add(Properties.Resources.strNoPluginsFound);
                CustGlobal.lstBxPlugins.Enabled = false;

                Directory.CreateDirectory(Constants.StrPluginFolder);
                return;
            }

            /* List all dll's */
            var strPlugins = Directory.GetFiles(Constants.StrPluginFolder, "*.exe");
            var strTmpPlugins = Directory.GetFiles(Constants.StrPluginFolder, "*.dll");
            var lTmpPlugins = new List<String>();
            foreach (var s in strPlugins)
                lTmpPlugins.Add(s);

            foreach (var s in strTmpPlugins)
                lTmpPlugins.Add(s);


            foreach (var strPlugin in lTmpPlugins)
            {
                try
                {
                    var pluginTypes = Assembly.LoadFile(strPlugin).GetTypes();

                    foreach (var pluginType in pluginTypes)
                    {
                        /* Search for main- plugin method */
                        if (pluginType.ToString().Contains("AnotherSc2HackPlugin"))
                        {
                            var plugin = Activator.CreateInstance(pluginType) as IPlugins;


                            if (plugin == null)
                                break;

                            _lPlugins.Add(plugin);
                            plugin.StartPlugin();
                            CustGlobal.lstBxPlugins.Items.Add(plugin.GetPluginName(), true);

                            break;
                        }
                    }
                }

                catch
                {
                    /* Eat the error! */
                    MessageBox.Show("Couldn't load the plugin \"" + strPlugin + "\".\n" +
                                    "We ignore it!");
                }
            }

            if (CustGlobal.lstBxPlugins.Items.Count <= 0)
            {
                CustGlobal.lstBxPlugins.Items.Add("- No Items found -");
                CustGlobal.lstBxPlugins.Enabled = false;
            }
        }

        private void btnMaphackDefineaMarking_Click(object sender, EventArgs e)
        {
            var iSelectedIndex = lstMapUnits.SelectedIndex;
            /*
            if (iSelectedIndex <= -1)
                return;

            */
            var dm = new DefineMarks();
            dm.Show();
            
        }
    }
}
