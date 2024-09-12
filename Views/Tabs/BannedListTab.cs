using ACManager.Settings;
using System;
using VirindiViewService.Controls;

namespace ACManager.Views.Tabs
{
    public class BannedListTab
        : IDisposable
    {
        private bool disposedValue;
        private readonly BotManagerView parent;

        HudTextBox UIBannedCharText;
        HudButton UIBannedCharAdd;
        HudList UIBannedCharList;
        HudTextBox UIBannedMonarchText;
        HudButton UIBannedMonarchAdd;
        HudList UIBannedMonarchList;

        public BannedListTab(BotManagerView parent)
        {
            this.parent = parent;

            UIBannedCharText = parent.View != null ? (HudTextBox)parent.View["BannedCharText"] : new HudTextBox();
            UIBannedCharAdd = parent.View != null ? (HudButton)parent.View["BannedCharAdd"] : new HudButton();
            UIBannedCharAdd.Hit += UIBannedCharAdd_Hit;
            UIBannedCharList = parent.View != null ? (HudList)parent.View["BannedCharList"] : new HudList();
            UIBannedCharList.Click += UIBannedCharList_Click;
            UIBannedMonarchText = parent.View != null ? (HudTextBox)parent.View["BannedMonarchText"] : new HudTextBox();
            UIBannedMonarchAdd = parent.View != null ? (HudButton)parent.View["BannedMonarchAdd"] : new HudButton();
            UIBannedMonarchAdd.Hit += UIBannedMonarchAdd_Hit;
            UIBannedMonarchList = parent.View != null ? (HudList)parent.View["BannedMonarchList"] : new HudList();
            UIBannedMonarchList.Click += UIBannedMonarchList_Click;

            LoadBannedCharacters();
        }

        private void LoadBannedCharacters()
        {
            try
            {
                foreach (BannedName item in Utility.BannedCharacterSettings.BannedCharacters)
                {
                    HudList.HudListRowAccessor row = UIBannedCharList.AddRow();
                    ((HudStaticText)row[0]).Text = item.Name;
                    ((HudPictureBox)row[1]).Image = 0x060011F8; // delete
                }

                foreach (BannedName item in Utility.BannedCharacterSettings.BannedMonarchs)
                {
                    HudList.HudListRowAccessor row = UIBannedMonarchList.AddRow();
                    ((HudStaticText)row[0]).Text = item.Name;
                    ((HudPictureBox)row[1]).Image = 0x060011F8; // delete
                }
            }
            catch (Exception ex) { Debug.LogException(ex); }
        }

        private void UIBannedMonarchList_Click(object sender, int row, int col)
        {
            if (col == 1)
            {
                var name = ((HudStaticText)UIBannedMonarchList[row][0]).Text;

                // Remove from table
                UIBannedMonarchList.RemoveRow(row);

                // Remove from settings
                for (var i = Utility.BannedCharacterSettings.BannedMonarchs.Count - 1; i >= 0; i--)
                {
                    if (string.Equals(name, Utility.BannedCharacterSettings.BannedMonarchs[i].Name, StringComparison.OrdinalIgnoreCase))
                        Utility.BannedCharacterSettings.BannedMonarchs.RemoveAt(i);
                }
                Utility.SaveBannedCharacterSettings();
            }
        }

        private void UIBannedMonarchAdd_Hit(object sender, EventArgs e)
        {
            try
            {
                var monarchName = UIBannedMonarchText.Text;

                if (!string.IsNullOrEmpty(monarchName))
                {
                    // Add to table
                    HudList.HudListRowAccessor row = UIBannedMonarchList.AddRow();
                    ((HudStaticText)row[0]).Text = monarchName;
                    ((HudPictureBox)row[1]).Image = 0x060011F8; // delete

                    // Add to settings
                    Utility.BannedCharacterSettings.BannedMonarchs.Add(new BannedName() { Name = monarchName });
                    Utility.SaveBannedCharacterSettings();
                }
            }
            finally
            {

                // Clear form
                UIBannedMonarchText.Text = "";
            }
        }

        private void UIBannedCharList_Click(object sender, int row, int col)
        {
            if (col == 1)
            {
                var name = ((HudStaticText)UIBannedCharList[row][0]).Text;

                // Remove from table
                UIBannedCharList.RemoveRow(row);

                // Remove from settings
                for (var i = Utility.BannedCharacterSettings.BannedCharacters.Count - 1; i >= 0; i--)
                {
                    if (string.Equals(name, Utility.BannedCharacterSettings.BannedCharacters[i].Name, StringComparison.OrdinalIgnoreCase))
                        Utility.BannedCharacterSettings.BannedCharacters.RemoveAt(i);
                }
                Utility.SaveBannedCharacterSettings();
            }
        }

        private void UIBannedCharAdd_Hit(object sender, EventArgs e)
        {
            try
            {
                var charName = UIBannedCharText.Text;

                if (!string.IsNullOrEmpty(charName))
                {
                    // Add to table
                    HudList.HudListRowAccessor row = UIBannedCharList.AddRow();
                    ((HudStaticText)row[0]).Text = charName;
                    ((HudPictureBox)row[1]).Image = 0x060011F8; // delete

                    // Add to settings
                    Utility.BannedCharacterSettings.BannedCharacters.Add(new BannedName() { Name = charName });
                    Utility.SaveBannedCharacterSettings();
                }
            }
            finally
            {
                // Clear form
                UIBannedCharText.Text = "";
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    UIBannedCharAdd.Hit -= UIBannedCharAdd_Hit;
                    UIBannedCharList.Click -= UIBannedCharList_Click;
                    UIBannedMonarchAdd.Hit -= UIBannedMonarchAdd_Hit;
                    UIBannedMonarchList.Click -= UIBannedMonarchList_Click;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
