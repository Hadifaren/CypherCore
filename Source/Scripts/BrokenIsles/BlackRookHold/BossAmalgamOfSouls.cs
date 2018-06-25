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
 * 
 * Based on AshamaneProject <https://github.com/AshamaneProject>
 */

using Framework.Constants;
using Game.AI;
using Game.Entities;
using Game.Scripting;

namespace Scripts.BrokenIsles.BlackRookHold
{
    internal struct Spells
    {
        public const uint ReapSoul = 194956;

        public const uint SoulEcho = 194966;
        public const uint SoulEchoCloneCaster = 194981;
        public const uint SoulEchoDamage = 194960;

        public const uint SwirlingScythe = 195254; // need script
        public const uint SwirlingScytheDamage = 196517;

        // Heroic Diff
        public const uint CallSouls = 196078;
        public const uint CallSoulsVisual = 196925;
        public const uint SoulGorge = 196930; // need script
        public const uint SoulBurst = 196587;
    }

    internal struct Events // time for events need correction i think
    {
        public const uint SoulReap = 0; // its bug ... boss dosent cast spell
        public const uint SoulEcho = 1;
        public const uint SwirlingScythe = 2;
        public const uint CallSouls = 3;
        public const uint SoulBurst = 4;
    }

    internal struct Actions
    {
        public const int SoulConsumed = 0;
        public const int SoulKilled = 1;
    }

    internal struct Stages
    {
        public const byte AllStages = 0;
        public const byte StageOne = 1;
        public const byte StageTwo = 2;
    }
    internal struct Texts
    {
        public const string Aggro = "Consume! Devour!";
        public const string SwirlingScythe = "The harvest has come!";
        public const string SoulEcho = "Leave this meager vessel, and join us...";
        public const string ReapSoul = "I feed on your essence...";
        public const string CallSouls = "Ancient souls of the Black Rook, rise and join our chorus!";
        public const string SoulBurst = "This energy... it is too much!";
    }
    /// Boss ID 98542
    [Script]
    class BossAmalgamOfSouls : BossAI
    {
        private uint SoulsCount;

        Position[] SoulPos =
        {
            new Position(3269.164f, 7554.257f, 14.54204f, 2.02439f),
            new Position(3282.502f, 7576.961f, 14.95347f, 2.978649f),
            new Position(3281.006f, 7597.751f, 14.83461f, 3.673727f),
            new Position(3257.893f, 7611.407f, 14.79525f, 4.639767f),
            new Position(3237.195f, 7609.405f, 14.67427f, 5.134565f),
            new Position(3221.241f, 7587.319f, 14.97383f, 6.128874f),
            new Position(3225.972f, 7565.66f, 14.6152f, 0.6193081f),
            new Position(3246.426f, 7552.234f, 14.60733f, 1.341422f)
        };
        public BossAmalgamOfSouls(Creature creature) : base(creature, EncounterData.AmalgamOfSouls_FirstBoss) { }

        public override void Reset()
        {
            _Reset();
            SoulsCount = 0;
            instance.SetBossState(EncounterData.AmalgamOfSouls_FirstBoss, EncounterState.Fail);
        }

        public void ScheduleEvents()
        {
            _events.SetPhase(Stages.StageOne);
            _events.ScheduleEvent(Events.SoulReap, 20000, 0, Stages.AllStages);
            _events.ScheduleEvent(Events.SoulEcho, 10000, 0, Stages.AllStages);
            _events.ScheduleEvent(Events.SwirlingScythe, 15000, 0, Stages.AllStages);
        }

        public override void EnterCombat(Unit who)
        {
            _EnterCombat();
            ScheduleEvents();
            me.Yell(Texts.Aggro, Language.Universal);
            instance.SetBossState(EncounterData.AmalgamOfSouls_FirstBoss, EncounterState.InProgress);
        }

        public override void JustDied(Unit killer)
        {
            base.JustDied(killer);
            me.RemoveAurasDueToSpell(Spells.CallSoulsVisual);
            me.Say("...ahhh", Language.Universal); // test
            instance.SetBossState(EncounterData.AmalgamOfSouls_FirstBoss, EncounterState.Done);
        }

        public override void KilledUnit(Unit victim)
        {
            if (victim.GetTypeId() == TypeId.Player)
                me.Say("hahaha...", Language.Universal); // test
        }

        public override void DoAction(int param)
        {
            switch (param)
            {
                case Actions.SoulConsumed:
                    me.AddAura(Spells.SoulGorge, me);
                    --SoulsCount;
                    break;
                case Actions.SoulKilled:
                    --SoulsCount;
                    break;
                default:
                    break;
            }
            if (SoulsCount == 0)
                me.RemoveAurasDueToSpell(Spells.CallSoulsVisual);
        }

        public override void UpdateAI(uint diff)
        {
            if (!UpdateVictim())
                return;

            _events.Update(diff);

            if (me.HasUnitState(UnitState.Casting))
                return;
            if (_events.IsInPhase(Stages.StageOne) && HealthBelowPct(51))
            {
                if (IsHeroic() || IsMythicDungeon())
                {
                    _events.SetPhase(Stages.StageTwo);
                    _events.DelayEvents(33000);
                    _events.ScheduleEvent(Events.CallSouls, 3000, 0, Stages.StageTwo);
                    _events.ScheduleEvent(Events.SoulBurst, 33000, 0, Stages.StageTwo);
                }

            }
            Unit target;
            _events.ExecuteEvents(eventIds =>
            {
                switch (eventIds)
                {
                    case Events.SoulReap:
                        me.Yell(Texts.ReapSoul, Language.Universal);
                        SetCombatMovement(false);
                        me.CastSpell(me.GetVictim(), Spells.ReapSoul);
                        _events.Repeat(15000);
                        SetCombatMovement(true);
                        break;
                    case Events.SoulEcho:
                        me.Yell(Texts.SoulEcho, Language.Universal);
                        target = SelectTarget(SelectAggroTarget.Random);
                        DoCast(target, Spells.SoulEcho);
                        _events.Repeat(11000);
                        break;
                    case Events.SwirlingScythe:
                        target = SelectTarget(SelectAggroTarget.Farthest);
                        DoCast(target, Spells.SwirlingScythe);
                        me.Yell(Texts.SwirlingScythe, Language.Universal);
                        _events.Repeat(30000);
                        break;
                    case Events.CallSouls:
                        me.Yell(Texts.CallSouls, Language.Universal);
                        DoCastAOE(Spells.CallSoulsVisual);
                        DoCast(me, Spells.CallSouls);
                        SoulsCount = 6;
                        for (int i = 0; i <= SoulsCount; ++i)
                        {
                            me.SummonCreature(NpcEntries.NpcRestlessSoul, SoulPos[i]);
                        }
                        break;
                    case Events.SoulBurst:
                        me.Yell(Texts.SoulBurst, Language.Universal);
                        DoCastAOE(Spells.SoulBurst);
                        me.RemoveAurasDueToSpell(Spells.SoulGorge);
                        break;
                    default:
                        break;
                }

            });

            DoMeleeAttackIfReady();
        }
    }
    /// Npc ID 99090
    [Script]
    class NpcSoulEcho : ScriptedAI
    {
        public NpcSoulEcho(Creature creature) : base(creature) { }

        public override void IsSummonedBy(Unit summoner)
        {
            _events.ScheduleEvent(1, 4000, 0, 0);
            me.SetFaction(16);
            me.SetFlag(UnitFields.Flags, UnitFlags.NotSelectable | UnitFlags.ImmuneToNpc | UnitFlags.ImmuneToPc);
            summoner.CastSpell(me, Spells.SoulEchoCloneCaster, true);
        }

        public override void UpdateAI(uint diff)
        {
            _events.Update(diff);
            _events.ExecuteEvents(eventIds =>
            {
                switch (eventIds)
                {
                    case 1:
                        me.CastSpell(me.SelectNearestPlayer(5.0f), Spells.SoulEchoDamage, false);
                        me.DespawnOrUnsummon();
                        _events.CancelEvent(1);
                        break;
                    default:
                        break;
                }
            });
        }
    }
    /// Npc ID 99664
    [Script]
    class NpcRestlessSoul : ScriptedAI
    {
        private TempSummon m_TempSummon;
        private Unit m_Summoner;
        private CreatureAI bossAI;
        public NpcRestlessSoul(Creature creature) : base(creature)
        {
            m_TempSummon = me.ToTempSummon();
            m_Summoner = m_TempSummon.GetSummoner();
            bossAI = m_Summoner.ToCreature().GetAI();
        }

        public override void IsSummonedBy(Unit summoner)
        {
            me.SetSpeed(UnitMoveType.Flight, 1.5f);
            me.SetSpeed(UnitMoveType.Run, 1.5f);
            me.SetReactState(ReactStates.Passive);
            me.GetMotionMaster().MovePoint(1, summoner);
        }

        public override void MovementInform(MovementGeneratorType type, uint id)
        {
            if (type != MovementGeneratorType.Point)
                return;
            if (id == 1)
            {
                if (m_Summoner.IsAIEnabled)
                {
                    me.DespawnOrUnsummon();
                    bossAI.DoAction(Actions.SoulConsumed);
                }
            }
        }
        public override void JustDied(Unit killer)
        {
            if (m_Summoner.IsAIEnabled)
            {
                me.DespawnOrUnsummon();
                bossAI.DoAction(Actions.SoulKilled);
            }
        }
    }
    [Script]
    // AreaTrigger 5167  ID 10000
    class AtSwirlingScythe : AreaTriggerEntityScript
    {
        public AtSwirlingScythe() : base("AtSwirlingScythe") { }

        class AtSwirlingScytheAI : AreaTriggerAI
        {
            public AtSwirlingScytheAI(AreaTrigger areatrigger) : base(areatrigger) { }
            public override void OnRemove()
            {
                Unit unit = at.GetCaster();
                unit.Say("Scythe Area Removed Before Trigger...", Language.Universal); // debug
            }
            public override void OnUnitEnter(Unit unit)
            {
                Unit caster = at.GetCaster();
                caster.CastSpell(unit, Spells.SwirlingScytheDamage);
                unit.Say("Scythe Area Triggered...", Language.Universal); // debug
            }
        }

        public override AreaTriggerAI GetAI(AreaTrigger areatrigger)
        {
            return new AtSwirlingScytheAI(areatrigger);
        }
    }
}

