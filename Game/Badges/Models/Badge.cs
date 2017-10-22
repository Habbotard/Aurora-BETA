﻿using MySql.Data.MySqlClient;

namespace AuroraEmu.Game.Badges.Models
{
    public class Badge
    {
        public int Id { get; }
        public int Slot { get; set; }
        public string Code { get; }

        public Badge(int id, string code, int slot = 0)
        {
            Id = id;
            Code = code;
            Slot = slot;
        }
        
        public Badge(MySqlDataReader reader)
        {
            Id = reader.GetInt32("id");
            Code = reader.GetString("badge_code");
            Slot = reader.GetInt32("slot_number");
        }
    }
}
