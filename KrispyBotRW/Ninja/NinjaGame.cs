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
            
            int challengerLastMovement = (int)(Timestep / Challenger.Speed),
                defenderLastMovement = (int)(Timestep / Defender.Speed);
            Timestep++;
            int challengerMovement = (int)(Timestep / Challenger.Speed),
                defenderMovement = (int)(Timestep / Defender.Speed);

            Challenger.CurrentStamina--;
            Defender.CurrentStamina--;

            bool challengerTired = Challenger.CurrentStamina <= 0,
                defenderTired = Defender.CurrentStamina <= 0;
            
            if (Challenger.CurrentStamina == 0) Display.WithCustom(NinjaDisplay.Participant.Challenger,
                "{0} has become tired! He won't do as much damage from now on.");
            if (Defender.CurrentStamina == 0) Display.WithCustom(NinjaDisplay.Participant.Defender,
                "{0} has become tired! He won't do as much damage from now on.");

            
            int challengerAttackTimes = challengerMovement - challengerLastMovement,
                defenderAttackTimes = defenderMovement - defenderLastMovement;
            int challengerDamage = 0, defenderDamage = 0;
            
            double challengerDodgeOdds = 0.03 + Challenger.Skills.CheckForLevel(0) * 0.05,
                defenderDodgeOdds = 0.03 + Defender.Skills.CheckForLevel(0) * 0.05;
            
            if (KrispyGenerator.Value() <= challengerDodgeOdds) {
                defenderAttackTimes = 0;
                Display.WithCustom(NinjaDisplay.Participant.Challenger, "{0} dodged {1}'s attack!");
            }
            if (KrispyGenerator.Value() <= defenderDodgeOdds) {
                challengerAttackTimes = 0;
                Display.WithCustom(NinjaDisplay.Participant.Defender, "{0} dodged {1}'s attack!");
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
                if (challengerTired) dmg /= 3;
                if (KrispyGenerator.Value() <= challengerCriticalOdds) { dmg *= 2; challengerCriticalTimes++; }
                dmg = (int)(dmg * challengerDmgAmp);
                dmg -= Defender.Skills.CheckForLevel(4);
                challengerDamage += dmg;
                Defender.CurrentHP -= dmg;
            }
            
            for (var a = 0; a < defenderAttackTimes; a++) {
                var dmg = KrispyGenerator.NumberBetween(Defender.HitMinimum, Defender.HitMaximum);
                if (defenderTired) dmg /= 3 / 2;
                if (KrispyGenerator.Value() <= defenderCriticalOdds) { dmg *= 2; defenderCriticalTimes++; }
                dmg = (int)(dmg * defenderDmgAmp);
                dmg -= Challenger.Skills.CheckForLevel(4);
                defenderDamage += dmg;
                Challenger.CurrentHP -= dmg;
            }
            

            if (challengerAttackTimes > 0 || defenderAttackTimes > 0) {
                
                if (challengerAttackTimes > 0)
                    Display.WithDamage(NinjaDisplay.Participant.Challenger, challengerDamage,
                        challengerAttackTimes, challengerCriticalTimes);
                if (defenderAttackTimes > 0)
                    Display.WithDamage(NinjaDisplay.Participant.Defender, defenderDamage,
                        defenderAttackTimes, defenderCriticalTimes);
                
                Display.WithHealthBar(Challenger.CurrentHP, Defender.CurrentHP, Challenger.MaxHP, Defender.MaxHP);
                
                bool challengerKO = Challenger.CurrentHP <= 0, defenderKO = Defender.CurrentHP <= 0;
                
                NinjaSkill challengerLastStand = Challenger.Skills.GetSkill(3),
                    defenderLastStand = Defender.Skills.GetSkill(3);
                if (challengerKO && challengerLastStand != null && challengerLastStand.ExtraData > 0) {
                    Challenger.CurrentHP = (int)(Challenger.MaxHP * Challenger.Skills.CheckForLevel(3) * 0.05);
                    Display.WithCustom(NinjaDisplay.Participant.Challenger,
                        "{0} survived a strong hit! He's on his last stand!");
                    challengerLastStand.ExtraData--;
                    challengerKO = false;
                }
                if (defenderKO && defenderLastStand != null && defenderLastStand.ExtraData > 0) {
                    Defender.CurrentHP = (int)(Defender.MaxHP * Defender.Skills.CheckForLevel(3) * 0.05);
                    Display.WithCustom(NinjaDisplay.Participant.Defender,
                        "{0} survived a strong hit! He's on his last stand!");
                    defenderLastStand.ExtraData--;
                    defenderKO = false;
                }
                
                if (challengerKO || defenderKO) {
                    Display.WithKOMessage(challengerKO, defenderKO);
                    EndGame(defenderKO, challengerKO);
                }
            } else Display.WithHealthBar();

            Display.UpdateDisplay();
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