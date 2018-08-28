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
 */

using Framework.Constants;
using Game.AI;
using Game.Entities;
using Game.Scripting;
using System.Collections.Generic;

namespace Scripts.BrokenIsles.BlackRookHold.BossIllysannaRavencrest
{
    struct Spells
    {
        // Stage One: Vengeance
        public const uint BrutalGlaive = 197546;

        public const uint DarkRush = 197484;
        public const uint DarkRushB = 197478;
        public const uint BlazingTrail = 197521;

        public const uint VengefulShear = 197418;

        // Stage Two: Fury 
        public const uint EyeBeams = 197696;
        public const uint EyeBeamsEfc = 197674;
        public const uint EyeBeamsFixate = 197687;
        public const uint FelBlazedGround = 197821;

        public const uint BoneCrushingStrike = 197974;
        // heroic + mythic
        public const uint ArcaneBlitz = 200248;
    }

    struct Events // time for events need correction i think
    {
        public const uint BrutalGlaive = 0;
        public const uint DarkRush = 1;
        public const uint VengefulShear = 2;
        public const uint EyeBeams = 3;
        public const uint CallForGuard = 4;
    }

    struct Stages
    {
        public const byte Vengeance = 1;
        public const byte Fury = 2;
    }

    struct Texts
    {
        public const string Aggro = "We will bury you here, fools.";
        public const string VengefulShear = "You can not escape.";
        public const string DarkRush = "The hunt is eternal...";
        public const string PhaseFuryStarts = "Guards! Hold them off!";
        public const string EyeBeams = "I will burn you alive!";
        public const string KillsPlayer = "Neutralized. Fall before me!";
        public const string Death = "Betrayed...";
    }

    [Script]
    class BossIllysannaRavencrest : BossAI
    {
        private bool first = true;
        private bool IsHeroOrMythic;
        private Position centerPos = new Position(3086.38f, 7295.11f, 103.53f);
        public BossIllysannaRavencrest(Creature creature) : base(creature, EncounterData.IllysannaRavencrest_SecondBoss)
        {
            me.SetPowerType(PowerType.Energy);
            me.SetPower(PowerType.Energy, 100);
        }

        public override void Reset()
        {
            _Reset();
            instance.SetBossState(EncounterData.IllysannaRavencrest_SecondBoss, EncounterState.Fail);
        }

        public void StageVengeance()
        {
            if (first == false)
                _events.CancelEventGroup(2);

            _events.SetPhase(Stages.Vengeance);
            _events.ScheduleEvent(Events.BrutalGlaive, 12000, 1, Stages.Vengeance);
            _events.ScheduleEvent(Events.DarkRush,RandomHelper.URand(10000,20000), 1, Stages.Vengeance);
            _events.ScheduleEvent(Events.VengefulShear, 16000, 1, Stages.Vengeance);
        }

        public void StageFury()
        {
            first = false;
            _events.CancelEventGroup(1);
            _events.SetPhase(Stages.Fury);
            _events.ScheduleEvent(Events.EyeBeams, 7000, 2, Stages.Fury);
            _events.ScheduleEvent(Events.CallForGuard, 1000, 2, Stages.Fury);
        }
        public override void EnterCombat(Unit who)
        {
            _EnterCombat();
            IsHeroOrMythic = IsHeroic() || IsMythicDungeon();
            StageVengeance();

            me.Yell(Texts.Aggro, Language.Universal);
            instance.SetBossState(EncounterData.IllysannaRavencrest_SecondBoss, EncounterState.InProgress);
        }

        public override void JustDied(Unit killer)
        {
            base.JustDied(killer);
            me.Say(Texts.Death, Language.Universal);
            instance.SetBossState(EncounterData.IllysannaRavencrest_SecondBoss, EncounterState.Done);
        }

        public override void KilledUnit(Unit victim)
        {
            if (victim.GetTypeId() == TypeId.Player)
                me.Say(Texts.KillsPlayer, Language.Universal);
        }

        public override void UpdateAI(uint diff)
        {
            if (!UpdateVictim())
                return;

            _events.Update(diff);

            if (me.HasUnitState(UnitState.Casting))
                return;

            if (_events.IsInPhase(Stages.Vengeance) | me.GetPower(PowerType.Energy) == 100)
            {
                StageFury();
            }
            if (_events.IsInPhase(Stages.Fury) | me.GetPower(PowerType.Energy) == 0)
            {
                StageVengeance();
            }

            _events.ExecuteEvents(eventIds =>
            {
                Unit target;
                switch (eventIds)
                {
                    case Events.BrutalGlaive:
                        target = SelectTarget(SelectAggroTarget.Farthest);
                        if (target != null)
                        {
                            me.CastSpell(target, Spells.BrutalGlaive);
                        }
                        _events.Repeat(12000);
                        break;

                    case Events.DarkRush:
                        List<Unit> targetList = new List<Unit>();
                        targetList = SelectTargetList(3, SelectAggroTarget.NonTank, 50.0f, true);
                        foreach (Unit s in targetList)
                        {
                            me.CastSpell(s, Spells.DarkRush, true);
                        }
                        me.Yell(Texts.DarkRush, Language.Universal);
                        _events.Repeat(RandomHelper.URand(10000, 20000));
                        break;

                    case Events.VengefulShear:
                        me.CastSpell(me.GetVictim(), Spells.VengefulShear);
                        me.Yell(Texts.VengefulShear, Language.Universal);
                        _events.Repeat(16000);
                        break;

                    case Events.EyeBeams:
                        target = SelectTarget(SelectAggroTarget.Random);
                        if (target != null)
                        {
                            me.CastSpell(target, Spells.EyeBeams);
                        }
                        me.Yell(Texts.EyeBeams, Language.Universal);
                        _events.Repeat(7000);
                        break;
                    case Events.CallForGuard:
                        me.SummonCreature(NpcEntries.SoulTornVanguardBoss, centerPos, TempSummonType.CorpseTimedDespawn, 1000);
                        if (IsHeroOrMythic)
                        {
                            me.SummonCreature(NpcEntries.RisenArcanistBoss, centerPos, TempSummonType.CorpseTimedDespawn, 1000);
                        }
                        me.Yell(Texts.PhaseFuryStarts, Language.Universal);
                        _events.Repeat(20000);
                        break;

                    default:
                        break;
                }
            });

            DoMeleeAttackIfReady();
        }
    }

    [Script]
    class NpcSoulTornVanguardBoss : ScriptedAI
    {
        public NpcSoulTornVanguardBoss(Creature creature) : base(creature)
        {
        }

        public override void IsSummonedBy(Unit summoner)
        {
            _events.ScheduleEvent(1, 8000);
        }

        public override void UpdateAI(uint diff)
        {
            _events.Update(diff);
            _events.ExecuteEvents(eventIds =>
            {
                switch (eventIds)
                {
                    case 1:
                        me.CastSpell(me.GetVictim(), Spells.BoneCrushingStrike);
                        _events.Repeat(8000);
                        break;
                    default:
                        break;
                }
            });
        }
    }

    [Script]
    class NpcRisenArcanistBoss : ScriptedAI
    {
        public NpcRisenArcanistBoss(Creature creature) : base(creature)
        {
        }

        public override void IsSummonedBy(Unit summoner)
        {
            _events.ScheduleEvent(1, 4000);
        }

        public override void UpdateAI(uint diff)
        {
            _events.Update(diff);
            _events.ExecuteEvents(eventIds =>
            {
                Unit target;
                switch (eventIds)
                {
                    case 1:
                        target = SelectTarget(SelectAggroTarget.Random);
                        me.CastSpell(target, Spells.ArcaneBlitz);
                        _events.Repeat(4000);
                        break;
                    default:
                        break;
                }
            });
        }
    }
}
