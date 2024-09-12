using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System;

namespace ACManager.StateMachine
{
    public sealed class BannedMonarchCheck
    {
        public delegate void HandleResponseDelegate(bool isTell, int guid, string name, string message);

        private readonly bool isTell;
        private readonly int guid;
        private readonly string name;
        private readonly string message;
        private readonly HandleResponseDelegate handler;

        public BannedMonarchCheck(bool isTell, int guid, string name, string message, HandleResponseDelegate handler)
        {
            this.isTell = isTell;
            this.guid = guid;
            this.name = name;
            this.message = message;
            this.handler = handler;

            var wo = CoreManager.Current.WorldFilter[guid];
            if (wo.HasIdData)
            {
                CheckMonarch(wo);
            }
            else
            {
                CoreManager.Current.WorldFilter.ChangeObject += WorldFilter_ChangeObject;
                CoreManager.Current.Actions.RequestId(guid);
            }
        }

        private void WorldFilter_ChangeObject(object sender, ChangeObjectEventArgs e)
        {
            if (e.Change == WorldChangeType.IdentReceived && e.Changed.Id == guid)
            {
                CheckMonarch(e.Changed);
            }
        }

        private void CheckMonarch(WorldObject wo)
        {
            bool banned = false;
            foreach (var bannedChar in Utility.BannedCharacterSettings.BannedMonarchs)
            {
                if (wo.Values(StringValueKey.MonarchName).EndsWith(bannedChar.Name, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.ToChat(string.Format("{0} monarchy has been banned and will be ignored.", wo.Values(StringValueKey.MonarchName)));
                    banned = true;
                }
            }

            if (!banned)
                handler(isTell, guid, name, message);
            CoreManager.Current.WorldFilter.ChangeObject -= WorldFilter_ChangeObject;
        }
    }
}
