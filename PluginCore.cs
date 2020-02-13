﻿using Decal.Adapter;
using Decal.Adapter.Wrappers;
using MyClasses.MetaViewWrappers;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;

namespace ACManager
{
    [WireUpBaseEvents]
    [MVView("ACManager.mainView.xml")]
    [MVWireUpControlEvents]
    [FriendlyName("AC Manager")]
    public class PluginCore : PluginBase
    {
        const string Module = "General";
        private readonly string PluginName = "Fellowship Manager";
        private FellowshipControl FellowshipControl;
        private ExpTracker ExpTracker;
        private InventoryTracker InventoryTracker;
        private bool AutoRespondEnabled;

        private LogoffEventType LogoffType;

        #region UI Control References
        #region Configuration
        [MVControlReference("SecretPassword")]
        private ITextBox SecretPasswordTextBox = null;
        [MVControlReference("AutoFellow")]
        private ICheckBox AutoFellowCheckBox = null;
        [MVControlReference("AutoRespond")]
        private ICheckBox AutoRespondCheckBox = null;
        [MVControlReference("Components")]
        private ICheckBox ComponentsCheckBox = null;
        [MVControlReference("AnnounceLogoff")]
        private ICheckBox AnnounceLogoff = null;
        #endregion
        #region XP Tracker Tab
        [MVControlReference("XpAtLogon")]
        private IStaticText XpAtLogonText = null;
        [MVControlReference("XpSinceLogon")]
        private IStaticText XpSinceLogonText = null;
        [MVControlReference("XpSinceReset")]
        private IStaticText XpSinceResetText = null;
        [MVControlReference("XpPerHour")]
        private IStaticText XpPerHourText = null;
        [MVControlReference("XpLast5")]
        private IStaticText XpLast5Text = null;
        [MVControlReference("LoginTime")]
        private IStaticText TimeLoggedInText = null;
        [MVControlReference("TimeSinceReset")]
        private IStaticText TimeSinceResetText = null;
        [MVControlReference("TimeToNextLevel")]
        private IStaticText TimeToNextLevelText = null;
        #endregion
        #region Components Tab
        [MVControlReference("LeadScarabCount")]
        private ITextBox LeadScarabText = null;
        [MVControlReference("IronScarabCount")]
        private ITextBox IronScarabText = null;
        [MVControlReference("CopperScarabCount")]
        private ITextBox CopperScarabText = null;
        [MVControlReference("SilverScarabCount")]
        private ITextBox SilverScarabText = null;
        [MVControlReference("GoldScarabCount")]
        private ITextBox GoldScarabText = null;
        [MVControlReference("PyrealScarabCount")]
        private ITextBox PyrealScarabText = null;
        [MVControlReference("PlatinumScarabCount")]
        private ITextBox PlatinumScarabText = null;
        [MVControlReference("ManaScarabCount")]
        private ITextBox ManaScarabText = null;
        [MVControlReference("TaperCount")]
        private ITextBox TaperText = null;
        #endregion
        #endregion

        protected override void Startup()
        {
            try
            {
                MVWireupHelper.WireupStart(this, Host);
            }
            catch (Exception ex) { Utility.LogError(ex); }
        }

        protected override void Shutdown()
        {
            try
            {
                if (LogoffType != LogoffEventType.Authorized) 
                    Notifications.Notify(Utility.CharacterName);
                MVWireupHelper.WireupEnd(this);
            }
            catch (Exception ex) { Utility.LogError(ex); }
        }

        [BaseEvent("LoginComplete", "CharacterFilter")]
        private void LoginComplete(object sender, EventArgs e)
        {
            try
            {
                Utility.Host = Host;
                Utility.Core = Core;
                Utility.PluginName = PluginName;
                Utility.CharacterName = Core.CharacterFilter.Name;
                InventoryTracker = new InventoryTracker(this, Host, Core);
                FellowshipControl = new FellowshipControl(this, Host, Core);
                ExpTracker = new ExpTracker(Host, Core);
                LoadSettings();
                StartXPTracking();

                // Start listening to all chat and parsing if enabled
                Core.ChatBoxMessage += new EventHandler<ChatTextInterceptEventArgs>(ParseChat);
            }
            catch (Exception ex) { Utility.LogError(ex); }
        }

        [BaseEvent("Logoff", "CharacterFilter")]
        private void CharacterFilter_Logoff(object sender, LogoffEventArgs e)
        {
            try
            {
                LogoffType = e.Type;
                Core.ChatBoxMessage -= new EventHandler<ChatTextInterceptEventArgs>(ParseChat);
            }
            catch (Exception ex) { Utility.LogError(ex); }
        }

        private void LoadSettings()
        {
            try
            {
                XmlNode node = Utility.LoadCharacterSettings(Module);
                if (node != null)
                {
                    XmlNodeList settingNodes = node.ChildNodes;
                    if (settingNodes.Count > 0)
                    {
                        foreach (XmlNode aNode in settingNodes)
                        {
                            switch (aNode.Name)
                            {
                                case "AutoRespond":
                                    if (aNode.InnerText.Equals("True"))
                                    {
                                        AutoRespondCheckBox.Checked = true;
                                        AutoRespondEnabled = true;
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
            }
        }

        private void StartXPTracking()
        {
            #region Do Only Once
            XpAtLogonText.Text = String.Format("{0:n0}", ExpTracker.TotalXpAtLogon);
            #endregion

            #region ExpTracker Subscriptions
            ExpTracker.RaiseXpPerHour += Update_XpPerHour;
            ExpTracker.RaiseXpLast5 += Update_XpLast5;
            ExpTracker.RaiseXpEarnedSinceLogon += Update_XpEarnedSinceLogon;
            ExpTracker.RaiseXpEarnedSinceReset += Update_XpEarnedSinceReset;
            ExpTracker.RaiseTimeLoggedIn += Update_TimeLoggedIn;
            ExpTracker.RaiseTimeSinceReset += Update_TimeSinceReset;
            ExpTracker.RaiseTimeToLevel += Update_TimeToLevel;
            #endregion
        }

        private void Update_XpPerHour(object sender, XpEventArgs e)
        {
            XpPerHourText.Text = String.Format("{0:n0}", e.Value);
        }

        private void Update_XpLast5(object sender, XpEventArgs e)
        {
            XpLast5Text.Text = String.Format("{0:n0}", e.Value);
        }

        private void Update_XpEarnedSinceLogon(object sender, XpEventArgs e)
        {
            XpSinceLogonText.Text = String.Format("{0:n0}", e.Value);
        }

        private void Update_XpEarnedSinceReset(object sender, XpEventArgs e)
        {
            XpSinceResetText.Text = String.Format("{0:n0}", e.Value);
        }

        private void Update_TimeLoggedIn(object sender, XpEventArgs e)
        {
            TimeSpan t = TimeSpan.FromSeconds(e.Value);
            TimeLoggedInText.Text = String.Format("{0:D2}d {1:D2}h {2:D2}m {3:d2}s", t.Days, t.Hours, t.Minutes, t.Seconds);
        }

        private void Update_TimeSinceReset(object sender, XpEventArgs e)
        {
            TimeSpan t = TimeSpan.FromSeconds(e.Value);
            TimeSinceResetText.Text = String.Format("{0:D2}d {1:D2}h {2:D2}m {3:D2}s", t.Days, t.Hours, t.Minutes, t.Seconds);
        }

        private void Update_TimeToLevel(object sender, XpEventArgs e)
        {
            try
            {
                TimeSpan t = TimeSpan.FromSeconds(e.Value);
                if (e.Value > 0)
                {
                    TimeToNextLevelText.Text = String.Format("{0:D2}d {1:D2}h {2:D2}m {3:D2}s", t.Days, t.Hours, t.Minutes, t.Seconds);
                }
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
            }
        }

        private void ParseChat(object sender, ChatTextInterceptEventArgs e)
        {
            string sanitizedInput = Regex.Replace(e.Text, @"[^\w:/ ']", string.Empty);

            FellowshipControl.ChatActions(sanitizedInput);

            if (AutoRespondEnabled)
            {
                // refactor to go to InventoryTracker
                Match match;
                string singleCompResponse = "/t {0}, I currently have {1} {2}.";
                string pluralCompResponse = "/t {0}, I currently have {1} {2}s.";
                TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;

                // checking spell components
                string componentsPattern = string.Format(@"(?<guid>\d+):(?<dupleName>.+?)Tell\s(?<msg>tells)\syou\s(?<secret>.*)");

                match = new Regex(componentsPattern).Match(sanitizedInput);
                if (match.Success)
                {
                    string name = match.Groups["dupleName"].Value.Substring(0, match.Groups["dupleName"].Value.Length / 2);
                    if (match.Groups["secret"].Value.Equals("comps"))
                    {
                        string[] comps = {
                            "Lead Scarab",
                            "Iron Scarab",
                            "Copper Scarab",
                            "Silver Scarab",
                            "Gold Scarab",
                            "Pyreal Scarab",
                            "Platinum Scarab",
                            "Mana Scarab",
                            "Prismatic Taper"
                            };
                        foreach (string comp in comps)
                        {
                            WorldObjectCollection collection = Core.WorldFilter.GetInventory();
                            collection.SetFilter(new ByNameFilter(comp));
                            if (collection.Quantity == 0) continue;
                            Host.Actions.InvokeChatParser(
                                collection.Quantity == 1 ?
                                string.Format(singleCompResponse, name, collection.Quantity.ToString(), collection.First.Name) :
                                string.Format(pluralCompResponse, name, collection.Quantity.ToString(), collection.First.Name)
                            );
                        }
                    }
                    else
                    {
                        WorldObjectCollection collection = Core.WorldFilter.GetInventory();
                        collection.SetFilter(new ByNameFilter(textInfo.ToTitleCase(match.Groups["secret"].Value)));
                        if (collection.Count > 0)
                        {
                            Host.Actions.InvokeChatParser(collection.Quantity == 1 ?
                                string.Format(singleCompResponse, name, collection.Quantity.ToString(), collection.First.Name) :
                                string.Format(pluralCompResponse, name, collection.Quantity.ToString(), collection.First.Name)
                            );
                        }
                    }
                }
            }
        }

        [MVControlEvent("AutoRespond", "Change")]
        void AutoRespond_Change(object sender, MVCheckBoxChangeEventArgs e)
        {
            try
            {
                Utility.SaveSetting(Module, AutoRespondCheckBox.Name, e.Checked.ToString());
                AutoRespondEnabled = e.Checked;
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
            }
        }

        [BaseEvent("ChangeFellowship", "CharacterFilter")]
        private void ChangeFellowship(object sender, ChangeFellowshipEventArgs e)
        {
            try
            {
                FellowshipControl.FellowStatus = e.Type;
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
            }
        }

        [MVControlEvent("SecretPassword", "Change")]
        void SecretPassword_Change(object sender, MVTextBoxChangeEventArgs e)
        {
            try
            {
                FellowshipControl.SetPassword(SecretPasswordTextBox.Name, e.Text);
            }
            catch (Exception ex)
            {
                Utility.LogError(ex);
            }
        }

        [MVControlEvent("AutoFellow", "Change")]
        void AutoFellow_Change(object sender, MVCheckBoxChangeEventArgs e)
        {
            try
            {
                FellowshipControl.SetAutoFellow(AutoFellowCheckBox.Name, e.Checked);
            }
            catch (Exception ex) { Utility.LogError(ex); }
        }

        [MVControlEvent("XpReset", "Click")]
        void XpReset_Clicked(object sender, MVControlEventArgs e)
        {
            ExpTracker.Reset();
            XpLast5Text.Text = "0";
            XpPerHourText.Text = "0";
            XpSinceResetText.Text = "0";
            TimeToNextLevelText.Text = String.Format("{0:D2}d {1:D2}h {2:D2}m {3:D2}s", 0, 0, 0, 0);
            TimeSinceResetText.Text = String.Format("{0:D2}d {1:D2}h {2:D2}m {3:D2}s", 0, 0, 0, 0);
        }

        [MVControlEvent("XpFellow", "Click")]
        void XpFellow_Clicked(object sender, MVControlEventArgs e)
        {
            ExpTracker.Report("/f");
        }

        [MVControlEvent("XpAlleg", "Click")]
        void XpAlleg_Clicked(object sender, MVControlEventArgs e)
        {
            ExpTracker.Report("/a");
        }

        public void SetPassword(string value)
        {
            SecretPasswordTextBox.Text = value;
        }

        public void SetAutoFellow(bool value)
        {
            AutoFellowCheckBox.Checked = value;
        }

        public void SetAutoRespond(bool value)
        {
            AutoRespondCheckBox.Checked = value;
        }

        [MVControlEvent("Components", "Change")]
        private void Components_Change(object sender, MVCheckBoxChangeEventArgs e)
        {
            InventoryTracker.LogoffEnabled = e.Checked;
        }

        [MVControlEvent("LeadScarabCount", "Change")]
        private void LeadScarabCountChanged(object sender, MVTextBoxChangeEventArgs e)
        {
            try
            {
                InventoryTracker.SetMinLead(LeadScarabText.Name, e.Text);
            }
            catch (Exception)
            {
            }
        }

        public void SetLeadCount(string value)
        {
            LeadScarabText.Text = value;
        }

        [MVControlEvent("IronScarabCount", "Change")]
        private void IronScarabCountChanged(object sender, MVTextBoxChangeEventArgs e)
        {
            try
            {
                InventoryTracker.SetMinIron(IronScarabText.Name, e.Text);
            }
            catch (Exception)
            {
            }
        }

        public void SetIronCount(string value)
        {
            IronScarabText.Text = value;
        }

        [MVControlEvent("CopperScarabCount", "Change")]
        private void CopperScarabCountChanged(object sender, MVTextBoxChangeEventArgs e)
        {
            try
            {
                InventoryTracker.SetMinCopper(CopperScarabText.Name, e.Text);
            }
            catch (Exception)
            {
            }
        }

        public void SetCopperCount(string value)
        {
            CopperScarabText.Text = value;
        }

        [MVControlEvent("SilverScarabCount", "Change")]
        private void SilverScarabCountChanged(object sender, MVTextBoxChangeEventArgs e)
        {
            try
            {
                InventoryTracker.SetMinSilver(SilverScarabText.Name, e.Text);
            }
            catch (Exception)
            {
            }
        }

        public void SetSilverCount(string value)
        {
            SilverScarabText.Text = value;
        }

        [MVControlEvent("GoldScarabCount", "Change")]
        private void GoldScarabCountChanged(object sender, MVTextBoxChangeEventArgs e)
        {
            try
            {
                InventoryTracker.SetMinGold(GoldScarabText.Name, e.Text);
            }
            catch (Exception)
            {
            }
        }

        public void SetGoldCount(string value)
        {
            GoldScarabText.Text = value;
        }

        [MVControlEvent("PyrealScarabCount", "Change")]
        private void PyrealScarabCountChanged(object sender, MVTextBoxChangeEventArgs e)
        {
            try
            {
                InventoryTracker.SetMinPyreal(PyrealScarabText.Name, e.Text);
            }
            catch (Exception)
            {
            }
        }

        public void SetPyrealCount(string value)
        {
            PyrealScarabText.Text = value;
        }

        [MVControlEvent("PlatinumScarabCount", "Change")]
        private void PlatinumScarabCountChanged(object sender, MVTextBoxChangeEventArgs e)
        {
            try
            {
                InventoryTracker.SetMinPlatinum(PlatinumScarabText.Name, e.Text);
            }
            catch (Exception)
            {
            }
        }

        public void SetPlatinumCount(string value)
        {
            PlatinumScarabText.Text = value;
        }

        [MVControlEvent("ManaScarabCount", "Change")]
        private void ManaScarabCountChanged(object sender, MVTextBoxChangeEventArgs e)
        {
            try
            {
                InventoryTracker.SetMinMana(ManaScarabText.Name, e.Text);
            }
            catch (Exception)
            {
            }
        }

        public void SetManaCount(string value)
        {
            ManaScarabText.Text = value;
        }

        [MVControlEvent("TaperCount", "Change")]
        private void TaperCountChanged(object sender, MVTextBoxChangeEventArgs e)
        {
            try
            {
                InventoryTracker.SetMinTapers(TaperText.Name, e.Text);
            }
            catch (Exception)
            {
            }
        }

        public void SetTaperCount(string value)
        {
            TaperText.Text = value;
        }

        [MVControlEvent("AnnounceLogoff", "Change")]
        private void AnnounceLogoffChange(object sender, MVCheckBoxChangeEventArgs e)
        {
            InventoryTracker.SetAnnounce(AnnounceLogoff.Name, e.Checked);
        }

        public void SetAnnounceCheckBox(bool value)
        {
            AnnounceLogoff.Checked = value;
        }
    }
}
