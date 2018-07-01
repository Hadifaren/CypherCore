/*
 * Copyright (C) 2012-2018 CypherCore <http://github.com/CypherCore>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

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

        // Boss Illysanna Ravencrest
        public const uint RookSpiderling = 98677;
        public const uint RookSpinner = 98681;
        public const uint RisenArcanist = 98280;
        public const uint RisenArcher = 98275;
        public const uint RisenCompanion = 101839;
        public const uint RisenScout = 98691;
        public const uint SoulTornChampion = 98243;
        public const uint CommanderShemdah = 98706;
        public const uint IllysannaRavencrest = 98696;

        // Boss Smashspite the Hateful
        public const uint WrathguardBladelord = 98810;
        public const uint WyrmtongueScavenger = 98792;
        public const uint WyrmtongueTrickster = 98900;
        public const uint BloodscentFelhound = 98813;
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
        public const uint StrikeDown = 225732; // eff dosent work %20 dmg taken

        // Lord Etheldrin Spells
        public const uint SoulEcho = 194966;
        public const uint SpiritBlast = 196883;

        // RookSpiderling Spells
        public const uint HunterRush = 223971;
        public const uint SoulVenum = 225908;
        public const uint SoulVenumDebuff = 225909;
        public const uint InternalRupture = 225917;

        // Risen Arcanist Spells
        public const uint ArcaneBlitz = 200248;

        // Risen Archer Spells
        public const uint ArrowBarrage = 200343; // teleport + channel + dmg
        public const uint Shoot = 193633;

        // Risen Companion Spells
        public const uint BloodthirtyLeap = 225962;
        public const uint BloodthirtyLeap2 = 225963;

        // Risen Scout Spells 
        public const uint KnifeDance = 200291;
        public const uint KnifeDanceEff1 = 200325;

        // Soul-Torn Champion & Commander Shemdah Spells
        public const uint BonebreakingStrike = 200261;

        // Bolder Crush on Stairs
        public const uint BolderCrush = 222397;
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

                switch (creature.GetEntry())
                {
                    case NpcEntries.RookSpiderling:
                        creature.AddAura(Spells.SoulVenum, creature);
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
                        caster.CastSpell(ally, Spells.DarkMendingHeal, true);
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
                        DoCastAOE(Spells.SacrificeSoul, false);
                        _events.Repeat(RandomHelper.URand(20000, 30000));
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
            List<Unit> list = caster.FindNearbyUnits(true, 25.0f);
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
                        target = SelectTarget(SelectAggroTarget.NonTank);
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
                        target = SelectTarget(SelectAggroTarget.NonTank);
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

    [Script]
    class NpcRookSpiderling : ScriptedAI
    {
        public NpcRookSpiderling(Creature creature) : base(creature) { }

        public override void EnterCombat(Unit victim)
        {
            _events.ScheduleEvent(1, 15000);
        }

        public override void JustDied(Unit killer)
        {
            if (IsMythicDungeon())
            {
                DoCastAOE(Spells.InternalRupture, true);
            }
            me.RemoveAurasDueToSpell(Spells.SoulVenum);
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
                        DoCastSelf(Spells.HunterRush);
                        break;

                    default:
                        break;
                }
            });
        }
    }

    [Script]
    class SpellInternalRupture : SpellScript
    {
        public override bool Validate(SpellInfo spellEntry)
        {
            return ValidateSpellInfo(Spells.InternalRupture, Spells.SoulVenumDebuff);
        }

        private void HandleEffect()
        {
            Unit caster = GetCaster();
            Unit target = GetHitUnit();
            if (target != null)
            {
                caster.CastSpell(target, Spells.SoulVenumDebuff, true);
            }
        }

        public override void Register()
        {
            OnHit.Add(new HitHandler(HandleEffect));
        }
    }
    [Script]
    class NpcRisenArcanist : ScriptedAI
    {
        public NpcRisenArcanist(Creature creature) : base(creature) { }

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

            _events.ExecuteEvents(eventIds =>
            {
                Unit target;
                switch (eventIds)
                {
                    case 1:
                        target = SelectTarget(SelectAggroTarget.Random);
                        me.CastSpell(target, Spells.ArcaneBlitz);
                        _events.Repeat(8000);
                        break;

                    default:
                        break;
                }
            });
        }
    }

    [Script]
    class NpcRisenArcher : ScriptedAI
    {
        public NpcRisenArcher(Creature creature) : base(creature) { }

        struct Events
        {
            public const uint Shoot = 0;
            public const uint ArrowBarrage = 1;
        }
        public override void EnterCombat(Unit victim)
        {
            _events.ScheduleEvent(Events.Shoot, 4000);
            _events.ScheduleEvent(Events.ArrowBarrage, RandomHelper.URand(8000, 12000));
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
                    case Events.Shoot:
                        target = SelectTarget(SelectAggroTarget.Random);
                        me.CastSpell(target, Spells.Shoot);
                        break;
                    case Events.ArrowBarrage:
                        target = SelectTarget(SelectAggroTarget.Random);
                        me.CastSpell(target, Spells.ArrowBarrage);
                        break;

                    default:
                        break;
                }
            });
        }
    }

    [Script]
    class NpcRisenCompanion : ScriptedAI
    {
        public NpcRisenCompanion(Creature creature) : base(creature) { }

        public override void EnterCombat(Unit victim)
        {
            _events.ScheduleEvent(1, 2000);
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
                    case 1:
                        target = SelectTarget(SelectAggroTarget.Random);
                        me.CastSpell(target, Spells.BloodthirtyLeap);
                        _events.Repeat(10000);
                        break;

                    default:
                        break;
                }
            });
        }
    }

    [Script]
    class NpcRisenScout : ScriptedAI
    {
        public NpcRisenScout(Creature creature) : base(creature) { }

        public override void EnterCombat(Unit victim)
        {
            _events.ScheduleEvent(1, 5000);
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
                        DoCastAOE(Spells.KnifeDance);
                        _events.Repeat(5000);
                        break;

                    default:
                        break;
                }
            });
        }
    }

    [Script]
    class NpcSoulCommander : ScriptedAI
    {
        public NpcSoulCommander(Creature creature) : base(creature) { }

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

            _events.ExecuteEvents(eventIds =>
            {
                switch (eventIds)
                {
                    case 1:
                        me.CastSpell(me.GetVictim(), Spells.BonebreakingStrike);
                        _events.Repeat(8000);
                        break;

                    default:
                        break;
                }
            });
        }
    }
}
