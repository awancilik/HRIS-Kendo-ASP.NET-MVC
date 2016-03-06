namespace CVScreeningCore.Models.ScreeningState
{
    public class ScreeningStateValidated : ScreeningState
    {
        /// <summary>
        /// Screening state label
        /// </summary>
        public const string kScreeningStateValidated = "Validated";

        public ScreeningStateValidated(Screening screening)
        {
            Screening = screening;
            screening.ScreeningState = (byte) ScreeningStateType.VALIDATED;
            Name = kScreeningStateValidated;
        }

        public override ScreeningStateType GetCode()
        {
            return ScreeningStateType.VALIDATED;
        }

        public override bool IsValidated()
        {
            return true;
        }

        public override void ToOpen()
        {
            this.Screening.setState(ScreeningStateType.OPEN);
        }

        public override void ToUpdating()
        {
            this.Screening.setState(ScreeningStateType.UPDATING);
        }

        public override void ToSubmitted()
        {
            this.Screening.setState(ScreeningStateType.SUBMITTED);
        }
    }
}