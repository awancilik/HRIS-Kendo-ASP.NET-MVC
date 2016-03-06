using CVScreeningCore.Models.AtomicCheckState;

namespace CVScreeningCore.Models.AtomicCheckValidationState
{
    public class AtomicCheckValidationStateNotProcessed : AtomicCheckValidationState
    {
        public const string kAtomicCheckValidationStateNotProcessed = "Not submitted";

        public AtomicCheckValidationStateNotProcessed(AtomicCheck atomicCheck)
        {
            AtomicCheck = atomicCheck;
            atomicCheck.AtomicCheckValidationState = (byte)AtomicCheckValidationStateType.NOT_PROCESSED;
        }

        public override AtomicCheckValidationStateType GetCode()
        {
            return AtomicCheckValidationStateType.NOT_PROCESSED;
        }

        public override bool IsNotProcessed()
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