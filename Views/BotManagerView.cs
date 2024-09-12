﻿using ACManager.StateMachine;
using ACManager.Views.Tabs;
using System;
using VirindiViewService;

namespace ACManager.Views
{
    public class BotManagerView : IDisposable
    {
        public Machine Machine { get; set; }
        public HudView View { get; set; }
        public ConfigTab ConfigTab { get; set; }
        public GemsTab GemsTab { get; set; }
        public PortalsTab PortalsTab { get; set; }
        public AdvertisementsTab AdvertisementsTab { get; set; }
        public InventoryTab InventoryTab { get; set; }
        public EquipmentTab EquipmentTab { get; set; }
        public BannedListTab BannedListTab { get; set; }

        public BotManagerView(Machine machine)
        {
            try
            {
                Machine = machine;
                VirindiViewService.XMLParsers.Decal3XMLParser parser = new VirindiViewService.XMLParsers.Decal3XMLParser();
                parser.ParseFromResource("ACManager.Views.botManagerView.xml", out ViewProperties Properties, out ControlGroup Controls);

                View = new HudView(Properties, Controls);

                ConfigTab = new ConfigTab(this);
                PortalsTab = new PortalsTab(this);
                GemsTab = new GemsTab(this);
                AdvertisementsTab = new AdvertisementsTab(this);
                InventoryTab = new InventoryTab(this);
                EquipmentTab = new EquipmentTab(this);
                BannedListTab = new BannedListTab(this);
            }
            catch (Exception ex) { Debug.LogException(ex); }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ConfigTab?.Dispose();
                    GemsTab?.Dispose();
                    PortalsTab?.Dispose();
                    AdvertisementsTab?.Dispose();
                    InventoryTab?.Dispose();
                    EquipmentTab?.Dispose();
                    View?.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
