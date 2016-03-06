using CVScreeningCore.Exception;

namespace CVScreeningCore.Models.AtomicCheckState
{
    public class AtomicCheckStateDoneOk : AtomicCheckState
    {
        public const string kAtomicCheckStateDoneOk = "Validated";

        public AtomicCheckStateDoneOk(AtomicCheck atomicCheck)
        {
            AtomicCheck = atomicCheck;
            atomicCheck.AtomicCheckState = (byte)AtomicCheckStateType.DONE_OK;
        }

        public override AtomicCheckStateType GetCode()
        {
            return AtomicCheckStateType.DONE_OK;
        }

        public override bool IsDoneOk()
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

        public override void ToOnProcessForwarded()
        {
            // Atomic check still in the first investigation place
            if (this.AtomicCheck.AtomicCheckCategory == this.AtomicCheck.GetFirstInvestigationPlace())
            {
                // Type of check does not have second mode of investigation
                if (this.AtomicCheck.GetSecondInvestigationPlace() == "")
                    throw new ExceptionAtomicCheckOnProcessForwardImpossible();
            }
            // Atomic check already in second investigation place
            else if (this.AtomicCheck.AtomicCheckCategory == this.AtomicCheck.GetSecondInvestigationPlace())
            {
                throw new ExceptionAtomicCheckOnProcessForwardImpossible();
            }
            this.AtomicCheck.AtomicCheckCategory = this.AtomicCheck.GetSecondInvestigationPlace();
            this.AtomicCheck.setState(AtomicCheckStateType.ON_PROCESS_FORWARDED);
            this.AtomicCheck.Screener = null;

        }


        public override void ToDeactivated()
        {
            this.AtomicCheck.setState(AtomicCheckStateType.DEACTIVATED);
        }

    }
}