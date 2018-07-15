using System;
using System.Collections.Generic;

using Discord;


namespace KrispyBotRW.Ninja {
    internal class NinjaGame {
        private readonly NinjaProfile Challenger, Defender;
        private readonly IMessageChannel Response;
        private readonly NinjaDisplay Display;
        private int Timestep;

        private bool GameOver;

        public enum StepResult { Nothing, Exchange, End, Error }

        private void EndGame(bool challengerWon, bool defenderWon) {
            Challenger.EndGame();
            Defender.EndGame();
            Challenger.GiveExp(challengerWon, Defender.Level.Number, Timestep, Response);
            Defender.GiveExp(defenderWon, Challenger.Level.Number, Timestep, Response);
            GameOver = true;
        }

        public StepResult Step() {
            if (GameOver) return StepResult.Error;
            
            Display.ResetDisplay();
            
            Challenger.CurrentStamina--;
            Defender.CurrentStamina--;

            bool challengerTired = Challenger.CurrentStamina <= 0,
                defenderTired = Defender.CurrentStamina <= 0;
            
            if (Challenger.CurrentStamina == 0) Display.WithCustom(NinjaDisplay.Participant.Challenger,
                "{0} has become tired! He's not moving as fast as before...");
            if (Defender.CurrentStamina == 0) Display.WithCustom(NinjaDisplay.Participant.Defender,
                "{0} has become tired! He's not moving as fast as before...");
            
            int challengerLastMovement = (int)(Timestep / (Challenger.Speed * (challengerTired ? 1.5 : 1))),
                defenderLastMovement = (int)(Timestep / (Defender.Speed * (defenderTired ? 1.5 : 1)));
            Timestep++;
            int challengerMovement = (int)(Timestep / (Challenger.Speed * (challengerTired ? 1.5 : 1))),
                defenderMovement = (int)(Timestep / (Defender.Speed * (defenderTired ? 1.5 : 1)));
            
            int challengerAttackTimes = challengerMovement - challengerLastMovement,
                defenderAttackTimes = defenderMovement - defenderLastMovement;
            int challengerDamage = 0, defenderDamage = 0;
            
            double challengerDodgeOdds = 0.03 + Challenger.Skills.CheckForLevel(0) * 0.05,
                defenderDodgeOdds = 0.03 + Defender.Skills.CheckForLevel(0) * 0.05;
            
            if (KrispyGenerator.Value() <= challengerDodgeOdds) {
                defenderAttackTimes = 0;
                Display.WithCustom(NinjaDisplay.Participant.Challenger,
                    KrispyGenerator.PickLine(KrispyLines.NinjaDodges));
            }
            if (KrispyGenerator.Value() <= defenderDodgeOdds) {
                challengerAttackTimes = 0;
                Display.WithCustom(NinjaDisplay.Participant.Defender,
                    KrispyGenerator.PickLine(KrispyLines.NinjaDodges));
            }

            int challengerCriticalTimes = 0, defenderCriticalTimes = 0;
            double challengerCriticalOdds = 0.03 + Challenger.Skills.CheckForLevel(1) * 0.05,
                defenderCriticalOdds = 0.03 + Defender.Skills.CheckForLevel(1) * 0.05;

            double challengerDmgAmp = 1 + Challenger.Skills.CheckForLevel(5) * 0.3 *
                                      (1 - Challenger.CurrentHP / Challenger.MaxHP),
                defenderDmgAmp = 1 + Defender.Skills.CheckForLevel(5) * 0.3 *
                                 (1 - Defender.CurrentHP / Defender.MaxHP);
            
            for (var a = 0; a < challengerAttackTimes; a++) {
                var dmg = KrispyGenerator.NumberBetween(Challenger.HitMinimum, Challenger.HitMaximum);
                if (KrispyGenerator.Value() <= challengerCriticalOdds) { dmg *= 2; challengerCriticalTimes++; }
                dmg = (int)(dmg * challengerDmgAmp);
                dmg -= Defender.Skills.CheckForLevel(4);
                challengerDamage += dmg;
                Defender.CurrentHP -= dmg;
            }
            
            for (var a = 0; a < defenderAttackTimes; a++) {
                var dmg = KrispyGenerator.NumberBetween(Defender.HitMinimum, Defender.HitMaximum);
                if (KrispyGenerator.Value() <= defenderCriticalOdds) { dmg *= 2; defenderCriticalTimes++; }
                dmg = (int)(dmg * defenderDmgAmp);
                dmg -= Challenger.Skills.CheckForLevel(4);
                defenderDamage += dmg;
                Challenger.CurrentHP -= dmg;
            }
            
            if (challengerAttackTimes > 0 || defenderAttackTimes > 0) {

                if (challengerAttackTimes > 0) {
                    if (Challenger.Skills.SkillExists(6)) {
                        var skill = Challenger.Skills.GetSkill(6);
                        skill.ExtraData--;
                        if (skill.ExtraData <= 0) {
                            skill.ExtraData = skill.BaseSkill.DefaultData;
                            Challenger.CurrentHP =
                                Math.Min(Challenger.CurrentHP +
                                         KrispyGenerator.NumberBetween(skill.Level * 5, skill.Level * 10),
                                    Challenger.MaxHP);
                        }
                    }
                    Display.WithDamage(NinjaDisplay.Participant.Challenger, challengerDamage,
                        challengerAttackTimes, challengerCriticalTimes);
                }
                if (defenderAttackTimes > 0) {
                    if (Defender.Skills.SkillExists(6)) {
                        var skill = Defender.Skills.GetSkill(6);
                        skill.ExtraData--;
                        if (skill.ExtraData <= 0) {
                            skill.ExtraData = skill.BaseSkill.DefaultData;
                            Defender.CurrentHP =
                                Math.Min(Defender.CurrentHP +
                                         KrispyGenerator.NumberBetween(skill.Level * 5, skill.Level * 10),
                                    Defender.MaxHP);
                        }
                    }
                    Display.WithDamage(NinjaDisplay.Participant.Defender, defenderDamage,
                        defenderAttackTimes, defenderCriticalTimes);
                }
                
                bool challengerKO = Challenger.CurrentHP <= 0, defenderKO = Defender.CurrentHP <= 0;
                
                NinjaSkill challengerLastStand = Challenger.Skills.GetSkill(3),
                    defenderLastStand = Defender.Skills.GetSkill(3);
                if (challengerKO && challengerLastStand != null && challengerLastStand.ExtraData > 0) {
                    Challenger.CurrentHP = (int)(Challenger.MaxHP * Challenger.Skills.CheckForLevel(3) * 0.05);
                    Display.WithCustom(NinjaDisplay.Participant.Challenger,
                        "{0} survived a strong hit! They're on their last stand!");
                    challengerLastStand.ExtraData--;
                    challengerKO = false;
                }
                if (defenderKO && defenderLastStand != null && defenderLastStand.ExtraData > 0) {
                    Defender.CurrentHP = (int)(Defender.MaxHP * Defender.Skills.CheckForLevel(3) * 0.05);
                    Display.WithCustom(NinjaDisplay.Participant.Defender,
                        "{0} survived a strong hit! They're on their last stand!");
                    defenderLastStand.ExtraData--;
                    defenderKO = false;
                }

                Display.WithHealthBar(Challenger.CurrentHP, Defender.CurrentHP, Challenger.MaxHP, Defender.MaxHP);
                
                if (challengerKO || defenderKO) {
                    Display.WithKOMessage(challengerKO, defenderKO);
                    EndGame(defenderKO, challengerKO);
                }
            } else Display.WithHealthBar();

            Display.UpdateDisplay().GetAwaiter().GetResult();
            if (GameOver) return StepResult.End;
            if (challengerAttackTimes > 0 || defenderAttackTimes > 0) return StepResult.Exchange;
            return StepResult.Nothing;
        }
        
        public static readonly List<NinjaGame> Games = new List<NinjaGame>();
        
        public NinjaGame(ulong challenger, ulong defender, IMessageChannel responseChannel) {
            Response = responseChannel;
            
            Challenger = NinjaProfile.Profiles[challenger];
            Defender = NinjaProfile.Profiles[defender];

            Challenger.CurrentHP = (int)(Challenger.CurrentHP * 1 - Defender.Skills.CheckForLevel(2) * 0.05);
            Defender.CurrentHP = (int)(Defender.CurrentHP * 1 - Challenger.Skills.CheckForLevel(2) * 0.05);
            
            Display = new NinjaDisplay(Response, Challenger.UserId, Defender.UserId, Defender.ChallengeMessage);

            Challenger.Game = this;
            Defender.Game = this;
        }
    }
}