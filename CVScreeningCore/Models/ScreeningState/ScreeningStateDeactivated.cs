namespace CVScreeningCore.Models.ScreeningState
{
    public class ScreeningStateDeactivated : ScreeningState
    {
        /// <summary>
        /// Screening state label
        /// </summary>
        public const string kScreeningStateDeactivated = "Deactivated";

        public ScreeningStateDeactivated(Screening screening)
        {
            Screening = screening;
            screening.ScreeningState = (byte) ScreeningStateType.DEACTIVATED;
            Name = kScreeningStateDeactivated;
        }

        public override ScreeningStateType GetCode()
        {
            return ScreeningStateType.DEACTIVATED;
        }

        public override bool IsDeactivated()
        {
            return true;
        }
    }
}