namespace CVScreeningCore.Models.AtomicCheckValidationState
{
    public class AtomicCheckValidationStateProcessed : AtomicCheckValidationState
    {
        public const string kAtomicCheckValidationStateProcessed = "Submitted";

        public AtomicCheckValidationStateProcessed(AtomicCheck atomicCheck)
        {
            AtomicCheck = atomicCheck;
            atomicCheck.AtomicCheckValidationState = (byte)AtomicCheckValidationStateType.PROCESSED;
        }

        public override AtomicCheckValidationStateType GetCode()
        {
            return AtomicCheckValidationStateType.PROCESSED;
        }

        public override bool IsProcessed()
        {
            return true;
        }

        public override void ToValidated()
        {
            this.AtomicCheck.setValidationState(AtomicCheckValidationStateType.VALIDATED);
        }

        public override void ToRejected()
        {
            this.AtomicCheck.setValidationState(AtomicCheckValidationStateType.REJECTED);
        }
    }
}