namespace CVScreeningCore.Models.ScreeningState
{
    public class ScreeningStateUpdating : ScreeningState
    {
        /// <summary>
        /// Screening state label
        /// </summary>
        public const string kScreeningStateUpdating = "Updating";

        public ScreeningStateUpdating(Screening screening)
        {
            Screening = screening;
            screening.ScreeningState = (byte)ScreeningStateType.UPDATING;
            Name = kScreeningStateUpdating;    
        }

        public override bool IsUpdating()
        {
            return true;
        }

        public override ScreeningStateType GetCode()
        {
            return ScreeningStateType.UPDATING;
        }

        public override void ToValidated()
        {
            Screening.setState(ScreeningStateType.VALIDATED);
        }

        public override void ToSubmitted()
        {
            Screening.setState(ScreeningStateType.SUBMITTED);
        }
    }
}