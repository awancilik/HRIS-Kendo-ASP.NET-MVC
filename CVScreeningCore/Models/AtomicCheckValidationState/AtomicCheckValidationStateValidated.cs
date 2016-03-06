using CVScreeningCore.Models.AtomicCheckValidationState;

namespace CVScreeningCore.Models.AtomicCheckValidationState
{
    public class AtomicCheckValidationStateValidated : AtomicCheckValidationState
    {
        public const string kAtomicCheckValidationStateValidated = "Validated QC";

        public AtomicCheckValidationStateValidated(AtomicCheck atomicCheck)
        {
            AtomicCheck = atomicCheck;
            atomicCheck.AtomicCheckValidationState = (byte)AtomicCheckValidationStateType.VALIDATED;
        }

        public override AtomicCheckValidationStateType GetCode()
        {
            return AtomicCheckValidationStateType.VALIDATED;
        }
        
        public override bool IsValidated()
        {
            return true;
        }

        public override void ToRejected()
        {
            this.AtomicCheck.setValidationState(AtomicCheckValidationStateType.REJECTED);
        }

        // Special cases used only when pending confirmation status is reopened
        public override void ToNotProcessed()
        {
            this.AtomicCheck.setValidationState(AtomicCheckValidationStateType.NOT_PROCESSED);
        }

        // Special cases used only when pending confirmation status is reopened
        public override void ToProcessed()
        {
            this.AtomicCheck.setValidationState(AtomicCheckValidationStateType.PROCESSED);
        }
    }
}