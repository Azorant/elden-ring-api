using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace EldenRingAPI.Models
{
    public class Weapon
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        public string name { get; set; }
        public string link { get; set; }
        public AttributeRequirements requirements { get; set; } = new();
        public decimal weight { get; set; }
        public string type { get; set; }
        public string damage { get; set; }
        public string? passive { get; set; }
        public WeaponSkill? skill { get; set; }  
        public List<WeaponAffinity> affinities { get; set; } = new();
        public string updated { get; set; }
    }

    public class WeaponSkill
    {
        public string name { get; set; }
        public string cost { get; set; }  
    }

    public class WeaponAffinity
    {
        public string name { get; set; }
        public List<WeaponLevel> levels { get; set; } = new();
    }

    public class WeaponLevel
    {
        public int level { get; set; }
        public WeaponAttackStats attack { get; set; } = new();
        public WeaponGuardStats guard { get; set; } = new();
        public AttributeScaling scaling { get; set; } = new();
    }

    public class WeaponAttackStats
    {
        public int physical { get; set; }
        public int magic { get; set; }
        public int fire { get; set; }
        public int lightning { get; set; }
        public int holy { get; set; }
        public int critical { get; set; }
        public int stamina { get; set; }
        public int range { get; set; }
        public string? incantationScaling { get; set; }
    }

    public class WeaponGuardStats
    {
        public int physical { get; set; }
        public int magic { get; set; }
        public int fire { get; set; }
        public int lightning { get; set; }
        public int holy { get; set; }
        public int boost { get; set; }
    }

    public class AttributeRequirements
    {
        public int strength { get; set; }
        public int dexterity { get; set; }
        public int intelligence { get; set; }
        public int faith { get; set; }
        public int arcane { get; set; }
    }

    public class AttributeScaling
    {
        public string? strength { get; set; }
        public string? dexterity { get; set; }
        public string? intelligence { get; set; }
        public string? faith { get; set; }
        public string? arcane { get; set; }
    }
}
