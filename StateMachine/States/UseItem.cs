﻿using Decal.Adapter;
using Decal.Adapter.Wrappers;
using System;

namespace ACManager.StateMachine.States
{
    /// <summary>
    /// This class is designed to use items.
    /// </summary>
    class UseItem : StateBase<UseItem>, IState
    {
        private DateTime UseDelay;

        public void Enter(Machine machine)
        {
            UseDelay = DateTime.Now;
        }

        public void Exit(Machine machine)
        {
            machine.CurrentRequest.ItemToUse = null;
        }

        public void Process(Machine machine)
        {
            if (machine.Enabled)
            {
                if (Inventory.GetInventoryCount(machine.CurrentRequest.ItemToUse) > 0)
                {
                    if (DateTime.Now - UseDelay > TimeSpan.FromSeconds(1))
                    {
                        using (WorldObjectCollection inventory = CoreManager.Current.WorldFilter.GetInventory())
                        {
                            inventory.SetFilter(new ByNameFilter(machine.CurrentRequest.ItemToUse));
                            if (inventory.Quantity > 0)
                            {
                                CoreManager.Current.Actions.UseItem(inventory.First.Id, 0);
                                ChatManager.Broadcast($"Portal opened with {machine.CurrentRequest.Destination}. Safe journey, friend.");
                                machine.NextState = Idle.GetInstance;
                            }
                        }
                    }
                }
                else
                {
                    ChatManager.Broadcast($"It appears I've run out of {machine.CurrentRequest.ItemToUse}.");
                    machine.NextState = Idle.GetInstance;
                }
            }
            else
            {
                machine.NextState = Idle.GetInstance;
            }
        }

        public override string ToString()
        {
            return nameof(UseItem);
        }
    }
}
