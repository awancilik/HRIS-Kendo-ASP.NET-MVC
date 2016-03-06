namespace CVScreeningCore.Models.AtomicCheckState
{
    public class AtomicCheckStateOnProcessForwarded : AtomicCheckState
    {
        public const string kAtomicCheckStateOnProcessForwarded = "Forwarded";

        public AtomicCheckStateOnProcessForwarded(AtomicCheck atomicCheck)
        {
            AtomicCheck = atomicCheck;
            atomicCheck.AtomicCheckState = (byte)AtomicCheckStateType.ON_PROCESS_FORWARDED;
        }

        public override AtomicCheckStateType GetCode()
        {
            return AtomicCheckStateType.ON_PROCESS_FORWARDED;
        }

        public override bool IsOnProcessForwarded()
        {
            return true;
        }

        public override void ToOnGoing()
        {
            this.AtomicCheck.setState(AtomicCheckStateType.ON_GOING);
        }

        public override void ToDoneKo()
        {
            this.AtomicCheck.setState(AtomicCheckStateType.DONE_KO);
        }

        public override void ToDoneImpossible()
        {
            this.AtomicCheck.setState(AtomicCheckStateType.DONE_IMPOSSIBLE);
        }

        public override void ToDoneDiscrepancy()
        {
            this.AtomicCheck.setState(AtomicCheckStateType.DONE_DISCREPANCY);
        }

        public override void ToPendingConfirmation()
        {
            this.AtomicCheck.setState(AtomicCheckStateType.PENDING_CONFIRMATION);
        }

        public override void ToWronglyQualified()
        {
            this.AtomicCheck.setState(AtomicCheckStateType.WRONGLY_QUALIFIED);
        }

        public override void ToDoneOk()
        {
            this.AtomicCheck.setState(AtomicCheckStateType.DONE_OK);
        }

        public override void ToDeactivated()
        {
            this.AtomicCheck.setState(AtomicCheckStateType.DEACTIVATED);
        }

    }
}