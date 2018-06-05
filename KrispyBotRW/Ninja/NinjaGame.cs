using System.Collections.Generic;

using Discord;

namespace KrispyBotRW.Ninja {
    internal class NinjaGame {
        private readonly NinjaProfile Challenger, Defender;
        private readonly IMessageChannel Response;
        private readonly NinjaDisplay Display;
        private int Timestep;

        private bool GameOver;

        public enum StepResult {
            Nothing,
            Exchange,
            End,
            Error
        }

        private void EndGame(bool challengerWon, bool defenderWon) {
            GameOver = true;
            Challenger.Restore();
            Defender.Restore();
            Challenger.GiveExp(challengerWon, Defender.Level.Number, Timestep, Response);
            Defender.GiveExp(defenderWon, Challenger.Level.Number, Timestep, Response);
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
            
            int challengerAttackTimes = challengerMovement - challengerLastMovement,
                defenderAttackTimes = defenderMovement - defenderLastMovement;
            int challengerDamage = 0, defenderDamage = 0;
            
            for (var a = 0; a < challengerAttackTimes; a++) {
                var dmg = KrispyGenerator.NumberBetween(Challenger.HitMinimum, Challenger.HitMaximum);
                if (challengerTired) dmg /= 3 / 2;
                challengerDamage += dmg;
                Defender.CurrentHP -= dmg;
            }
            
            for (var a = 0; a < defenderAttackTimes; a++) {
                var dmg = KrispyGenerator.NumberBetween(Defender.HitMinimum, Defender.HitMaximum);
                if (defenderTired) dmg /= 3 / 2;
                defenderDamage += dmg;
                Challenger.CurrentHP -= dmg;
            }

            if (challengerAttackTimes > 0 || defenderAttackTimes > 0) {
                if (challengerAttackTimes > 0)
                    Display.WithDamage(NinjaDisplay.Participant.Challenger, challengerDamage, challengerAttackTimes);
                if (defenderAttackTimes > 0)
                    Display.WithDamage(NinjaDisplay.Participant.Defendant, defenderDamage, defenderAttackTimes);
                Display.WithHealthBar(Challenger.CurrentHP, Defender.CurrentHP, Challenger.MaxHP, Defender.MaxHP);
                bool challengerKO = Challenger.CurrentHP <= 0, defenderKO = Defender.CurrentHP <= 0;
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
            
            Display = new NinjaDisplay(Response, Challenger.UserId, Defender.UserId, Defender.ChallengeMessage);

            Challenger.InGame = true;
            Defender.InGame = true;
        }
    }
}