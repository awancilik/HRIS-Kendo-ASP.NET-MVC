namespace CVScreeningCore.Models.AtomicCheckValidationState
{
    public class AtomicCheckValidationStateRejected : AtomicCheckValidationState
    {
        public const string kAtomicCheckValidationStateRejected = "Rejected QC";

        public AtomicCheckValidationStateRejected(AtomicCheck atomicCheck)
        {
            AtomicCheck = atomicCheck;
            atomicCheck.AtomicCheckValidationState = (byte)AtomicCheckValidationStateType.REJECTED;
        }

        public override AtomicCheckValidationStateType GetCode()
        {
            return AtomicCheckValidationStateType.REJECTED;
        }

        public override bool IsRejected()
        {
            return true;
        }

        public override void ToProcessed()
        {
            this.AtomicCheck.setValidationState(AtomicCheckValidationStateType.PROCESSED);
        }

        public override void ToValidated()
        {
            this.AtomicCheck.setValidationState(AtomicCheckValidationStateType.VALIDATED);
        }
    }
}