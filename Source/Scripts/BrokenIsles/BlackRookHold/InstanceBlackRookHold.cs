using Framework.Constants;
using Game.AI;
using Game.Entities;
using Game.Maps;
using Game.Scripting;
using Game.Spells;
using System.Collections.Generic;

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
        // Boss Amalgam of souls
        public const uint GhostlyCouncilor = 98370;
        public const uint GhostlyRetainer = 98366;
        public const uint GhostlyProtector = 98368;
        public const uint LadyVelandras = 98538;
        public const uint LordEtheldrin = 98521;
        public const uint AmalgamOfSouls = 98542;
        public const uint SoulEcho = 99090;
        public const uint RestlessSoul = 99664;
    }

    struct Spells
    {
        // Ghostly Counciler Spells
        public const uint SoulBlast = 199663;
        public const uint DarkMending = 225573;
        public const uint DarkMendingHeal = 225578;

        // Ghostly Retainer Spells
        public const uint SoulBlade = 200084;

        // Ghostly Protector Spells
        public const uint SacrificeSoul = 200105;
        public const uint SacrificeSoulAura = 200099;

        // Lady Velandras Spells
        public const uint GlavieToss = 196916;
        public const uint StrikeDown = 225732;

        // Lord Etheldrin Spells
        public const uint SoulEcho = 194966;
        public const uint SpiritBlast = 196883;

    }

    [Script]
    class InstanceBlackRookHold : InstanceMapScript
    {
        public static string InstanceName = "InstanceBlackRookHold";

        public InstanceBlackRookHold() : base(InstanceName, 1501)
        {
        }

        private class InstanceBlackRookHold_InstanceScript : InstanceScript
        {
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
            }
        }

        public override InstanceScript GetInstanceScript(InstanceMap map)
        {
            return new InstanceBlackRookHold_InstanceScript(map);
        }
    }

    [Script]
    class NpcGhostlyCouncilor : ScriptedAI
    {
        public NpcGhostlyCouncilor(Creature creature) : base(creature) { }

        private struct Events
        {
            public const uint SoulBlast = 0;
            public const uint DarkMending = 1;
        }

        public override void EnterCombat(Unit victim)
        {
            _events.ScheduleEvent(Events.SoulBlast, 5000);
            if (IsMythicDungeon() == true)
                _events.ScheduleEvent(Events.DarkMending, 12000);
        }

        public override void UpdateAI(uint diff)
        {
            base.UpdateAI(diff);

            _events.Update(diff);

            if (me.HasUnitState(UnitState.Casting))
                return;

            Unit target;
            _events.ExecuteEvents(eventIds =>
            {
                switch (eventIds)
                {
                    case Events.SoulBlast:
                        target = SelectTarget(SelectAggroTarget.Random);
                        me.CastSpell(target, Spells.SoulBlast);
                        _events.Repeat(5000);
                        break;

                    case Events.DarkMending:
                        DoCast(Spells.DarkMending);
                        _events.Repeat(12000);
                        break;
                }
            });
        }
    }

    [Script]
    class SpellDarkMending : SpellScript
    {
        public override bool Validate(SpellInfo spellEntry)
        {
            return ValidateSpellInfo(Spells.DarkMending, Spells.DarkMendingHeal);
        }

        private void HandleDummy(uint effIndex)
        {
            Unit caster = GetCaster();
            if (caster.HasAura(Spells.DarkMending))
            {
                List<Unit> list = caster.FindNearbyUnits(false, 25.0f);
                if (list.Count != 0)
                {
                    foreach(Unit ally in list)
                    {
                        caster.CastSpell(ally, Spells.DarkMendingHeal);
                    }
                }
            }
        }

        public override void Register()
        {
            OnEffectHitTarget.Add(new EffectHandler(HandleDummy, 0, SpellEffectName.ApplyAura));
        }
    }

    [Script]
    class NpcGhostlyRetainer : ScriptedAI
    {
        public NpcGhostlyRetainer(Creature creature) : base(creature) { }

        public override void EnterCombat(Unit victim)
        {
            _events.ScheduleEvent(1, 8000);
        }

        public override void UpdateAI(uint diff)
        {
            base.UpdateAI(diff);

            _events.Update(diff);

            if (me.HasUnitState(UnitState.Casting))
                return;

            Unit target;
            _events.ExecuteEvents(eventIds =>
            {
                switch (eventIds)
                {
                    case 1:
                        target = SelectTarget(SelectAggroTarget.Random);
                        me.CastSpell(target, Spells.SoulBlade);
                        _events.Repeat(8000);
                        break;

                    default:
                        break;
                }
            });
        }
    }


    [Script]
    class NpcGhostlyProtector : ScriptedAI
    {
        public NpcGhostlyProtector(Creature creature) : base(creature) { }

        public override void EnterCombat(Unit victim)
        {
            _events.ScheduleEvent(1, RandomHelper.URand(20000, 30000));
        }

        public override void UpdateAI(uint diff)
        {
            base.UpdateAI(diff);

            _events.Update(diff);

            if (me.HasUnitState(UnitState.Casting))
                return;

            _events.ExecuteEvents(eventIds =>
            {
                switch (eventIds)
                {
                    case 1:
                        DoCastAOE(Spells.SacrificeSoul);
                        break;
                }
            });
        }
    }


    [Script]
    class SpellSacrificeSoul : SpellScript
    {
        public override bool Validate(SpellInfo spellEntry)
        {
            return ValidateSpellInfo(Spells.DarkMending, Spells.DarkMendingHeal);
        }

        private void TriggerBuff()
        {
            Unit caster = GetCaster();
            List<Unit> list = caster.FindNearbyUnits(false, 25.0f);
            if (list.Count != 0)
            {
                foreach (Unit ally in list)
                {
                    caster.CastSpell(ally, Spells.SacrificeSoulAura);
                }
            }
        }

        public override void Register()
        {
            AfterCast.Add(new CastHandler(TriggerBuff));
        }
    }

    [Script]
    class NpcLadyVelandres : ScriptedAI
    {
        public NpcLadyVelandres(Creature creature) : base(creature) { }

        private struct Events
        {
            public const uint GlaiveToss = 0;
            public const uint StrikeDown = 1;
        }

        public override void EnterCombat(Unit victim)
        {
            _events.ScheduleEvent(Events.GlaiveToss, 8000);
            _events.ScheduleEvent(Events.StrikeDown, 12000);
        }

        public override void UpdateAI(uint diff)
        {
            base.UpdateAI(diff);

            _events.Update(diff);

            if (me.HasUnitState(UnitState.Casting))
                return;

            _events.ExecuteEvents(eventIds =>
            {
                Unit target;
                switch (eventIds)
                {
                    case Events.GlaiveToss:
                        target = SelectTarget(SelectAggroTarget.Random);
                        if (target == me.GetVictim())
                        {
                            target = SelectTarget(SelectAggroTarget.Random);
                        }
                        me.CastSpell(target, Spells.GlavieToss);
                        _events.Repeat(8000);
                        break;
                    case Events.StrikeDown:
                        DoCastVictim(Spells.StrikeDown, false);
                        _events.Repeat(12000);
                        break;

                    default:
                        break;
                }
            });
        }
    }

    [Script]
    class NpcLordEtheldrin : ScriptedAI
    {
        public NpcLordEtheldrin(Creature creature) : base(creature) { }

        private struct Events
        {
            public const uint SoulEcho = 0;
            public const uint SpiritBlast = 1;
        }

        public override void EnterCombat(Unit victim)
        {
            _events.ScheduleEvent(Events.SoulEcho, 10000);
            _events.ScheduleEvent(Events.SpiritBlast, 6000);
        }

        public override void UpdateAI(uint diff)
        {
            base.UpdateAI(diff);

            _events.Update(diff);

            if (me.HasUnitState(UnitState.Casting))
                return;

            _events.ExecuteEvents(eventIds =>
            {
                Unit target;
                switch (eventIds)
                {
                    case Events.SoulEcho:
                        target = SelectTarget(SelectAggroTarget.Random);
                        if (target == me.GetVictim())
                        {
                            target = SelectTarget(SelectAggroTarget.Random);
                        }
                        me.CastSpell(target, Spells.SoulEcho);
                        _events.Repeat(11000);
                        break;
                    case Events.SpiritBlast:
                        target = SelectTarget(SelectAggroTarget.Random);
                        me.CastSpell(target, Spells.SpiritBlast);
                        _events.Repeat(6000);
                        break;

                    default:
                        break;
                }
            });
        }
    }
}
