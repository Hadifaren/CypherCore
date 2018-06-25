using Game.Maps;
using Game.Scripting;
using Game.Entities;

namespace Scripts.BrokenIsles.BlackRookHold
{
    struct EncounterData
    {
        public const uint AmalgamOfSouls_FirstBoss = 0;
        public const uint SecondBoss = 1;
        public const uint ThirdBoss = 2;
        public const uint ForthBoss = 3;

        public const uint MaxEncounter = 4;
    }

    struct NpcEntries
    {
        public const uint NpcRestlessSoul = 99664;
    }
    [Script]
    class InstanceBlackRookHold : InstanceMapScript
    {
        public static string InstanceName = "InstanceBlackRookHold";
        public InstanceBlackRookHold() : base(InstanceName, 1501) { }

        class InstanceBlackRookHold_InstanceScript : InstanceScript
        {
            ObjectGuid NpcRestlessSoulGUID;

            public InstanceBlackRookHold_InstanceScript(InstanceMap map) : base(map)
            {
                SetHeaders("BRH");
                SetBossNumber(EncounterData.MaxEncounter);
            }

            public override void OnCreatureCreate(Creature creature)
            {
                if (instance.IsHeroic())
                    creature.SetMaxHealth(creature.GetMaxHealth() * 2);
                if (instance.IsMythicDungeon())
                    creature.SetMaxHealth(creature.GetMaxHealth() * 3);
                switch (creature.GetEntry())
                {
                    case NpcEntries.NpcRestlessSoul:
                        NpcRestlessSoulGUID = creature.GetGUID();
                        break;
                    default:
                        break;
                }
            }
        }

        public override InstanceScript GetInstanceScript(InstanceMap map)
        {
            return new InstanceBlackRookHold_InstanceScript(map);
        }
    }
}
