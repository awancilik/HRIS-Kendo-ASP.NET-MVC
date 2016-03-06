using CVScreeningCore.Exception;

namespace CVScreeningCore.Models.AtomicCheckState
{
    public class AtomicCheckStateDisabled : AtomicCheckState
    {
        public const string kAtomicCheckStateDisabled = "Disabled";

        public AtomicCheckStateDisabled(AtomicCheck atomicCheck)
        {
            AtomicCheck = atomicCheck;
            atomicCheck.AtomicCheckState = (byte)AtomicCheckStateType.DISABLED;
            if (!this.AtomicCheck.IsValidated())
                this.AtomicCheck.ValidationState.ToValidated();
        }

        public override AtomicCheckStateType GetCode()
        {
            return AtomicCheckStateType.DISABLED;
        }

        public override bool IsDisabled()
        {
            return true;
        }
    }
}