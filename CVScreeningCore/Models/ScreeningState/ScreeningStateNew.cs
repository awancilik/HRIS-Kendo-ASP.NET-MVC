namespace CVScreeningCore.Models.ScreeningState
{
    public class ScreeningStateNew : ScreeningState
    {
        /// <summary>
        /// Screening state label
        /// </summary>
        public const string kScreeningStateNew = "New";

        public ScreeningStateNew(Screening screening)
        {
            Screening = screening;
            screening.ScreeningState = (byte) ScreeningStateType.NEW;
            Name = kScreeningStateNew;
        }

        public override ScreeningStateType GetCode()
        {
            return ScreeningStateType.NEW;
        }

        public override bool IsNew()
        {
            return true;
        }

        public override void ToOpen()
        {
            //transition logic from New to Open
            this.Screening.setState(ScreeningStateType.OPEN);
        }
    }
}