using CVScreeningCore.Exception;

namespace CVScreeningCore.Models.AtomicCheckState
{
    public class AtomicCheckStateDoneKo : AtomicCheckState
    {
        public const string kAtomicCheckStateDoneKo = "Invalid";

        public AtomicCheckStateDoneKo(AtomicCheck atomicCheck)
        {
            AtomicCheck = atomicCheck;
            atomicCheck.AtomicCheckState = (byte)AtomicCheckStateType.DONE_KO;
        }

        public override AtomicCheckStateType GetCode()
        {
            return AtomicCheckStateType.DONE_KO;
        }

        public override bool IsDoneKo()
        {
            return true;
        }

        public override void ToOnGoing()
        {
            this.AtomicCheck.setState(AtomicCheckStateType.ON_GOING);
        }

        public override void ToDoneOk()
        {
            this.AtomicCheck.setState(AtomicCheckStateType.DONE_OK);
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