namespace CVScreeningCore.Models.ScreeningState
{
    public class ScreeningStateSubmitted : ScreeningState
    {
        /// <summary>
        /// Screening state label
        /// </summary>
        public const string kScreeningStateSubmitted = "Submitted";

        public ScreeningStateSubmitted(Screening screening)
        {
            Screening = screening;
            screening.ScreeningState = (byte)ScreeningStateType.SUBMITTED;
            Name = kScreeningStateSubmitted;
        }

        public override ScreeningStateType GetCode()
        {
            return ScreeningStateType.SUBMITTED;
        }

        public override bool IsSubmitted()
        {
            return true;
        }

        public override void ToUpdating()
        {
            Screening.setState(ScreeningStateType.UPDATING);
        }
    }
}