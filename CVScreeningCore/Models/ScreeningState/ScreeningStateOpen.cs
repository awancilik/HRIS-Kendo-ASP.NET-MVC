namespace CVScreeningCore.Models.ScreeningState
{
    public class ScreeningStateOpen : ScreeningState
    {
        /// <summary>
        /// Screening state label
        /// </summary>
        public const string kScreeningStateOpen = "Open";

        public ScreeningStateOpen(Screening screening)
        {
            this.Screening = screening;
            screening.ScreeningState = (byte)ScreeningStateType.OPEN;
            Name = kScreeningStateOpen;
        }

        public override ScreeningStateType GetCode()
        {
            return ScreeningStateType.OPEN;
        }

        public override bool IsOpen()
        {
            return true;
        }

        public override void ToValidated()
        {
            //transition logic from New to Open
            this.Screening.setState(ScreeningStateType.VALIDATED);
        }
    }
}