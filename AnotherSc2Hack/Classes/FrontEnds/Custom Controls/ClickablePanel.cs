﻿/// I know this is nasty code and looks utterly ugly
/// But for now, it's good enough.
/// I might reqrite this in the future as any kind of improvement or bugfixing will be a gigantic
/// "fuck you" for the dude that is supposed to change it.
/// I am sorry
/// 
/// bellaPatricia, November 2014


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using AnotherSc2Hack.Classes.BackEnds;

namespace AnotherSc2Hack.Classes.FrontEnds.Custom_Controls
{
    public enum ActiveBorderPosition
    {
        Left,
        Right,
        Top,
        Bottom
    };

    [DefaultEvent("Click")]
    public class ClickablePanel : Panel
    {
        public Panel SettingsPanel { get; set; }

        public ActiveBorderPosition ActiveBorderPosition { get; set; }


        public Color InactiveBackgroundColor
        {
            get { return _inactiveBackgroundColor; }
            set { _inactiveBackgroundColor = value; }
        }

        public Color ActiveBackgroundColor
        {
            get { return _activeBackgroundColor; }
            set { _activeBackgroundColor = value; }
        }

        public Color HoverBackgroundColor
        {
            get { return _hoverBackgroundColor; }
            set {  _hoverBackgroundColor = value; }
        }

        public Color ActiveForegroundColor
        {
            get { return _activeForegroundColor; }
            set { _activeForegroundColor = value; }
        }

        public Color InactiveForegroundColor
        {
            get { return _inactiveForegroundColor; }
            set { _inactiveForegroundColor = value; }
        }



        private Boolean _isClicked = false;

        public Boolean IsClicked
        {
            get { return _isClicked; }
            set
            {
                _isClicked = value;

                if (value)
                {
                    foreach (var panel in Parent.Controls)
                    {
                        if (panel == this)
                        {
                            DisplayColor = _activeForegroundColor;
                            continue;
                        }

                        if (panel.GetType() == typeof (ClickablePanel))
                        {
                            ((ClickablePanel) panel).IsClicked = false;
                            ((ClickablePanel) panel).DisplayColor = _inactiveForegroundColor;
                        }
                    }
                }


                Invalidate();
            }
        }

        private String _displayText = String.Empty;

        public String DisplayText
        {
            get { return _displayText; }
            set
            {
                _displayText = value;

                _lMainText.Text = _displayText;

                RealignLabelLocation();
            }
        }

        private Color _displayColor = Color.Wheat;

        public Color DisplayColor
        {
            get { return _displayColor; }
            set
            {
                _displayColor = value;

                if (_lMainText != null)
                    _lMainText.ForeColor = _displayColor;
            }
        }

        private float _textSize = 9f;

        public float TextSize
        {
            get { return _textSize; }
            set
            {
                _textSize = value;

                if (_lMainText != null)
                {
                    RealignLabelLocation();
                }
            }
        }

        private void RealignLabelLocation()
        {
            _lMainText.Font = new Font(Font.Name, _textSize);

            var fTextSize = TextRenderer.MeasureText(DisplayText, _lMainText.Font);
            var fHeight = (float)Size.Height / 2;
            var fY = fHeight - ((float)fTextSize.Height / 2);

            var posX = Width - fTextSize.Width;
            posX = posX / 2;

            if (_iTextPosX != 0)
                posX = 0;

            _lMainText.Location = new Point(_iTextPosX + posX, (int)fY);
        }

        private Image _imgIcon;
        public Image Icon
        {
            get { return _imgIcon; }
            set
            {
                _imgIcon = value;

                if (value == null)
                {
                    _pcbImage.Visible = false;

                    _iTextPosX = 0;
                    _lMainText.Location = new Point(0,0);
                    _lMainText.TextAlign = ContentAlignment.MiddleCenter;
                }

                else
                {
                    _pcbImage.Visible = true;
                    _iTextPosX = 45;
                    _lMainText.Location = new Point(_iTextPosX, _lMainText.Location.Y);
                }

                if (_pcbImage != null)
                    _pcbImage.Image = _imgIcon;
            }
        }
        private Boolean _isHovering = false;

        public Boolean IsHovering
        {
            get { return _isHovering; }
            set
            {
                _isHovering = value;
                Refresh();
            }
        }

        private Color _inactiveBackgroundColor = Color.FromArgb(255, 66, 79, 90);
        private Color _activeBackgroundColor = Color.FromArgb(255, 52, 63, 72);
        private Color _hoverBackgroundColor = Color.FromArgb(255, 94, 105, 114);
        private Color _inactiveForegroundColor = Color.FromArgb(255, 193, 193, 193);
        private Color _activeForegroundColor = Color.FromArgb(255, 242, 242, 242);
        private Color _newBackgroundColor = Color.Wheat;
        private Label _lMainText = new Label();     //This is only used as placeholder as directly drawing the text on the control is far more efficient (considering the events)
        private Int32 _iTextPosX = 45;
        
        private PictureBox _pcbImage = new PictureBox();

        private Brush _selectionColor = Brushes.Orange;
        private Boolean _bInitCalled = false;

        private static readonly List<ClickablePanel> Instances = new List<ClickablePanel>();

        ~ClickablePanel()
        {
            var index = Instances.FindIndex(x => x.GetHashCode().Equals(GetHashCode()));
            Instances.RemoveAt(index);
        }


        public static void OutputPath()
        {
            foreach (var clickablePanel in Instances)
            {
                Console.WriteLine(HelpFunctions.GetParent(clickablePanel) + Constants.ChrLanguageSplitSign + " " + clickablePanel.DisplayText);
            }
        }

        public ClickablePanel()
        {
            Instances.Add(this);

            SetStyle(ControlStyles.DoubleBuffer |
             ControlStyles.UserPaint |
             ControlStyles.OptimizedDoubleBuffer |
             ControlStyles.AllPaintingInWmPaint, true);
        }

        private void Init()
        {
            if (DisplayText.Length <= 0)
                DisplayText = "[SampleText]";

            TextSize = 13;
            DisplayColor = _inactiveForegroundColor;

            var fTextHeight = TextRenderer.MeasureText("dummy", _lMainText.Font).Height;
            var fHeight = (float)Size.Height / 2;
            var fY = fHeight - ((float)fTextHeight / 2);

            var fYImage = ((float) Size.Height/2) - 12;

            _lMainText.Visible = false;

            _pcbImage.Location = new Point(10, (int)fYImage);
            _pcbImage.MouseEnter += _pcbImage_MouseEnter;
            _pcbImage.Click += _pcbImage_Click;
            _pcbImage.Width = 24;
            _pcbImage.Height = 24;
            _pcbImage.SizeMode = PictureBoxSizeMode.StretchImage;

            Controls.Add(_pcbImage);


            _bInitCalled = true;
        }

        void _pcbImage_Click(object sender, EventArgs e)
        {
            OnClick(e);
        }

        void _pcbImage_MouseEnter(object sender, EventArgs e)
        {
            OnMouseEnter(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            //Remove the Hovering on the other panels (in case there's a hovering issue which happens)
            foreach (var pnl in Parent.Controls)
            {
                if (pnl == this)
                    continue;

                if (pnl.GetType() == typeof (ClickablePanel))
                {
                    ((ClickablePanel)pnl).IsHovering = false;
                }
            }

            IsHovering = true;

            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            IsHovering = false;

            Invalidate();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);


            if (!IsClicked)
                IsClicked = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.DrawString(_lMainText.Text, _lMainText.Font, new SolidBrush(_lMainText.ForeColor), _lMainText.Location);


            if (IsClicked)
            {
                if (ActiveBorderPosition == ActiveBorderPosition.Left)
                    e.Graphics.FillRectangle(_selectionColor, 0, 0, 5, Height);

                else if (ActiveBorderPosition == ActiveBorderPosition.Top)
                    e.Graphics.FillRectangle(_selectionColor, 0, 0, Width, 5);

                else if (ActiveBorderPosition == ActiveBorderPosition.Right)
                    e.Graphics.FillRectangle(_selectionColor, Width - 5, 0, 5, Height);

                else if (ActiveBorderPosition == ActiveBorderPosition.Bottom)
                    e.Graphics.FillRectangle(_selectionColor, 0, Height - 5, Width, 5);

                _newBackgroundColor = _activeBackgroundColor;
            }

            else if (!IsClicked && IsHovering)
            {
                _newBackgroundColor = _hoverBackgroundColor;
            }

            else if (!IsClicked && !IsHovering)
            {
                _newBackgroundColor = _inactiveBackgroundColor;
            }

            BackColor = _newBackgroundColor;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (!_bInitCalled)
                Init();
        }

        /// <summary>
        /// Performs an actual Click on the panel
        /// </summary>
        public void PerformClick()
        {
            OnClick(new EventArgs());
        }

        public static bool ChangeLanguage(string languageFile)
        {
            if (!File.Exists(languageFile))
                return false;

            var strLines = File.ReadAllLines(languageFile, Encoding.Default);

            foreach (var strLine in strLines)
            {
                if (strLine.Length <= 0 ||
                    strLine.StartsWith(";"))
                    continue;

                var strControlAndName = new string[2];
                strControlAndName[0] = strLine.Substring(0, strLine.IndexOf(Constants.ChrLanguageSplitSign));
                strControlAndName[1] = strLine.Substring(strLine.IndexOf(Constants.ChrLanguageSplitSign) + 1);


                var strControlNames = strControlAndName[0].Split(Constants.ChrLanguageControlSplitSign);

                foreach (var clickablePanel in Instances)
                {
                    if (HelpFunctions.CheckParents(clickablePanel, 0, ref strControlNames))
                    {
                        clickablePanel.DisplayText = strControlAndName[1].Trim();
                        clickablePanel.Refresh();
                    }
                }
            }

            return true;
        }
    }
}
