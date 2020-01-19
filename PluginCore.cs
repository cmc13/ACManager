﻿using System;
using System.Text.RegularExpressions;

using Decal.Adapter;
using Decal.Adapter.Wrappers;
using MyClasses.MetaViewWrappers;

using FellowshipManager.XPTracker;

// Feature requests
// Failure checking on fellowship join
// Time left until next level is reached - DONE!!

namespace FellowshipManager
{
    //Attaches events from core
    [WireUpBaseEvents]

    //View (UI) handling
    [MVView("FellowshipManager.mainView.xml")]
    [MVWireUpControlEvents]
    [FriendlyName("Fellowship Manager")]
    public class PluginCore : PluginBase
    {
        private ExpTracker ExpTracker;

        [MVControlReference("SecretPassword")]
        private ITextBox SecretPasswordTextBox = null;
        [MVControlReference("AutoFellow")]
        private ICheckBox AutoFellowCheckBox = null;
        [MVControlReference("AutoRespond")]
        private ICheckBox AutoRespondCheckBox = null;
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

        protected override void Startup()
        {
            try
            {
                Globals.Init("Fellowship Manager", Host, Core);
                //Initialize the view.
                MVWireupHelper.WireupStart(this, Host);
            }
            catch (Exception ex) { Util.LogError(ex); }
        }

        protected override void Shutdown()
        {
            try
            {
                //Destroy the view.
                MVWireupHelper.WireupEnd(this);
            }
            catch (Exception ex) { Util.LogError(ex); }
        }

        [BaseEvent("LoginComplete", "CharacterFilter")]
        private void CharacterFilter_LoginComplete(object sender, EventArgs e)
        {
            try
            {
                StartXP();
            }
            catch (Exception ex) { Util.LogError(ex); }
        }

        void StartXP()
        {
            ExpTracker = new ExpTracker(Globals.Core);

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
            TimeLoggedInText.Text = String.Format("{0:D2}h {1:D2}m {2:d2}s", t.Hours, t.Minutes, t.Seconds);
        }

        private void Update_TimeSinceReset(object sender, XpEventArgs e)
        {
            TimeSpan t = TimeSpan.FromSeconds(e.Value);
            TimeSinceResetText.Text = String.Format("{0:D2}h {1:D2}m {2:D2}s", t.Hours, t.Minutes, t.Seconds);
        }

        private void Update_TimeToLevel(object sender, XpEventArgs e)
        {
            TimeSpan t = TimeSpan.FromSeconds(e.Value);
            if (e.Value > 0)
            {
                TimeToNextLevelText.Text = String.Format("{0:D2}h {1:D2}m {2:D2}s", t.Hours, t.Minutes, t.Seconds);
            }
        }

        [BaseEvent("Logoff", "CharacterFilter")]
        private void CharacterFilter_Logoff(object sender, LogoffEventArgs e)
        {
            try
            {
                Globals.Core.ChatBoxMessage -= new EventHandler<ChatTextInterceptEventArgs>(AutoFellow_ChatBoxMessage_Watcher);
                Globals.Core.ChatBoxMessage -= new EventHandler<ChatTextInterceptEventArgs>(AutoResponder_ChatBoxMessage_Watcher);
            }
            catch (Exception ex) { Util.LogError(ex); }
        }

        [MVControlEvent("AutoRespond", "Change")]
        void AutoRespond_Change(object sender, MVCheckBoxChangeEventArgs e)
        {
            try
            {
                if (AutoRespondCheckBox.Checked)
                {
                    Globals.Core.ChatBoxMessage += new EventHandler<ChatTextInterceptEventArgs>(AutoResponder_ChatBoxMessage_Watcher);
                    Util.WriteToChat("To get your current component counts from another character, simply /tell 'comps' to this character.");
                }
                else
                {
                    Globals.Core.ChatBoxMessage -= new EventHandler<ChatTextInterceptEventArgs>(AutoResponder_ChatBoxMessage_Watcher);
                }
            }
            catch (Exception ex)
            {
                Util.LogError(ex);
            }
        }

        void AutoResponder_ChatBoxMessage_Watcher(object sender, ChatTextInterceptEventArgs e)
        {
            string sanitizedInput = Regex.Replace(e.Text, @"[^\w:/ ']", string.Empty);
            AutoRespondParser(sanitizedInput);
            //Util.WriteToChat(sanitizedInput);
        }

        void AutoRespondParser(string input)
        {
            Match match;
            string singleCompResponse = "/t {0}, I currently have {1} {2}.";
            string pluralCompResponse = "/t {0}, I currently have {1} {2}s.";

            // checking spell components
            string componentsPattern = string.Format(@"(?<guid>\d+):(?<dupleName>.+?)Tell\s(?<msg>tells).+?(?<secret>comps)");

            match = new Regex(componentsPattern).Match(input);
            if (match.Success && match.Groups["secret"].Value.Equals("comps"))
            {
                string[] s = {
                    "Lead Scarab",
                    "Iron Scarab",
                    "Silver Scarab",
                    "Copper Scarab",
                    "Gold Scarab",
                    "Pyreal Scarab",
                    "Platinum Scarab",
                    "Mana Scarab",
                    "Prismatic Taper"
                };
                string name = match.Groups["dupleName"].Value.Substring(0, match.Groups["dupleName"].Value.Length / 2);
                foreach (string comp in s)
                {
                    WorldObjectCollection collection = Globals.Core.WorldFilter.GetInventory();
                    collection.SetFilter(new ByNameFilter(comp));
                    if (collection.Quantity == 0) continue;
                    Globals.Host.Actions.InvokeChatParser(collection.Quantity == 1 ? string.Format(singleCompResponse, name, collection.Quantity.ToString(), collection.First.Name) : string.Format(pluralCompResponse, name, collection.Quantity.ToString(), collection.First.Name));
                }
            }
        }

        [MVControlEvent("AutoFellow", "Change")]
        void AutoFellow_Change(object sender, MVCheckBoxChangeEventArgs e)
        {
            try
            {
                if (AutoFellowCheckBox.Checked)
                {
                    Globals.Core.ChatBoxMessage += new EventHandler<ChatTextInterceptEventArgs>(AutoFellow_ChatBoxMessage_Watcher);
                    Globals.Host.Actions.FellowshipSetOpen(true);
                }
                else
                {
                    Globals.Core.ChatBoxMessage -= new EventHandler<ChatTextInterceptEventArgs>(AutoFellow_ChatBoxMessage_Watcher);
                }
            }
            catch (Exception ex) { Util.LogError(ex); }
        }

        void AutoFellow_ChatBoxMessage_Watcher(object sender, ChatTextInterceptEventArgs e)
        {
            string sanitizedInput = Regex.Replace(e.Text, @"[^\w:/ ']", string.Empty);
            AutoFellowParser(sanitizedInput);
            //Util.WriteToChat(sanitizedInput);
        }

        void AutoFellowParser(string input)
        {
            Regex regex;
            Match match;

            // Not accepting fellowship requests
            string notAcceptingPattern = @"(?<name>.+?) is not accepting fellowing requests";
            regex = new Regex(notAcceptingPattern);
            match = regex.Match(input);
            if(match.Success)
            {
                Globals.Host.Actions.InvokeChatParser(string.Format("/t {0}, <{1}> You are not accepting fellowship requests!", match.Groups["name"].Value, Globals.PluginName));
                return;
            }

            // Someone joins the fellowship
            string joinedFellowshipPattern = @"(?<name>.+?) joined the fellowship";
            regex = new Regex(joinedFellowshipPattern);
            match = regex.Match(input);
            if(match.Success)
            {
                // do something when someone joins if you want to
            }

            // Someone leaves the fellowship
            string leftFellowshipPattern = @"(?<name>.+?) left the fellowship";
            regex = new Regex(leftFellowshipPattern);
            match = regex.Match(input);
            if(match.Success)
            {
                // do something when someone leaves if you want to
            }

            // Someone sends you a tell, checking for secret password
            string fellowshipPattern = string.Format(@"(?<guid>\d+):(?<dupleName>.+?)Tell\s(?<msg>tells).+?(?<secret>{0})", SecretPasswordTextBox.Text);
            regex = new Regex(fellowshipPattern);
            match = regex.Match(input);
            if (match.Success)
            {
                if (match.Groups["msg"].Value.Equals("tells") && match.Groups["secret"].Value.Equals(SecretPasswordTextBox.Text))
                {
                    string recruitName = match.Groups["dupleName"].Value.Substring(0, match.Groups["dupleName"].Value.Length / 2);
                    Globals.Host.Actions.InvokeChatParser(string.Format("/t {0}, <{1}> Please stand near me, I'm going to try and recruit you into the fellowship.", recruitName, Globals.PluginName));
                    Globals.Host.Actions.FellowshipRecruit(Int32.Parse(match.Groups["guid"].Value));
                    return;
                }
            }
        }


        [MVControlEvent("XpReset", "Click")]
        void XpReset_Clicked(object sender, MVControlEventArgs e)
        {
            ExpTracker.Reset();
            XpLast5Text.Text = "0";
            XpPerHourText.Text = "0";
            XpSinceResetText.Text = "0";
            TimeToNextLevelText.Text = "";
            TimeSinceResetText.Text = String.Format("{0:D2}h {1:D2}m {2:D2}s", 0, 0, 0);
        }

        [MVControlEvent("XpFellow", "Click")]
        void XpFellow_Clicked(object sender, MVControlEventArgs e)
        {
            ReportXp("/f");
        }

        [MVControlEvent("XpAlleg", "Click")]
        void XpAlleg_Clicked(object sender, MVControlEventArgs e)
        {
            ReportXp("/a");
        }

        private void ReportXp(string targetChat)
        {
            //TimeSpan t = DateTime.Now - startTime;
            Globals.Host.Actions.InvokeChatParser(
                String.Format("{0} You have earned {1} XP in {2} for {3} XP/hour ({4} XP in the last 5 minutes). At this rate, you'll hit your next level in {5}.",
                targetChat,
                String.Format("{0:n0}", Globals.Core.CharacterFilter.TotalXP - ExpTracker.XpAtReset),
                String.Format("{0}h {1}m {2}s",
                    String.Format("{0:00}", ExpTracker.TimeSinceReset.Hours),
                    String.Format("{0:00}", ExpTracker.TimeSinceReset.Minutes),
                    String.Format("{0:00}", ExpTracker.TimeSinceReset.Seconds)),
                String.Format("{0:n0}", ExpTracker.XpPerHourLong),
                String.Format("{0:n0}", ExpTracker.XpLast5Long),
                String.Format("{0:D2}h {1:D2}m {2:D2}s",
                    ExpTracker.TimeLeftToLevel.Hours,
                    ExpTracker.TimeLeftToLevel.Minutes,
                    ExpTracker.TimeLeftToLevel.Seconds)));
        }
    }
}
