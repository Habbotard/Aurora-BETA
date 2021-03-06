﻿using AuroraEmu.Game.Clients;
using AuroraEmu.Game.Items.Models;
using AuroraEmu.Game.Items.Models.Dimmer;
using AuroraEmu.Network.Game.Packets.Composers.Items;
using System.Linq;

namespace AuroraEmu.Network.Game.Packets.Events.Items
{
    public class RoomDimmerSavePresetMessageEvent : IPacketEvent
    {
        public void Run(Client client, MessageEvent msgEvent)
        {
            Item item = client.CurrentRoom.GetWallItems().Where(x => x.Definition.HandleType == AuroraEmu.Game.Items.Handlers.HandleType.DIMMER).ToList()[0];
            DimmerData data = Engine.Locator.ItemController.GetDimmerData(item.Id);

            int preset = msgEvent.ReadVL64();
            int bgMode = msgEvent.ReadVL64();
            string colorCode = msgEvent.ReadString();
            int intensity = msgEvent.ReadVL64();

            data.Enabled = true;
            data.CurrentPreset = preset;
            DimmerPreset dimmerPreset = data.Presets[preset];
            dimmerPreset.BackgroundOnly = bgMode >= 2 ? true : false;
            dimmerPreset.ColorIntensity = intensity;
            dimmerPreset.ColorCode = colorCode;
            item.Data = data.GenerateExtradata();
            client.SendComposer(new ItemUpdateMessageComposer(item));
            Engine.Locator.ItemController.Dao.UpdateDimmerPreset(data);
        }
    }
}
