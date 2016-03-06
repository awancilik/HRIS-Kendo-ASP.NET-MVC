using CVScreeningCore.Exception;

namespace CVScreeningCore.Models.AtomicCheckState
{
    public class AtomicCheckStateOnGoing : AtomicCheckState
    {
        public const string kAtomicCheckStateOnGoing = "Ongoing";

        public AtomicCheckStateOnGoing(AtomicCheck atomicCheck)
        {
            AtomicCheck = atomicCheck;
            atomicCheck.AtomicCheckState = (byte)AtomicCheckStateType.ON_GOING;
        }

        public override AtomicCheckStateType GetCode()
        {
            return AtomicCheckStateType.ON_GOING;
        }

        public override bool IsOnGoing()
        {
            return true;
        }

        public override void ToDoneOk()
        {
            this.AtomicCheck.setState(AtomicCheckStateType.DONE_OK);
        }

        public override void ToDoneKo()
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
            this.AtomicCheck.Screener.AtomicCheck.Remove(this.AtomicCheck);
            this.AtomicCheck.Screener = null;

        }

        public override void ToDeactivated()
        {
            this.AtomicCheck.setState(AtomicCheckStateType.DEACTIVATED);
        }
    }
}