using System;

namespace CVScreeningCore.Models.AtomicCheckState
{
    public class AtomicCheckStateNotApplicable : AtomicCheckState
    {
        public const string kAtomicCheckStateNotApplicable = "Not applicable";

        public AtomicCheckStateNotApplicable(AtomicCheck atomicCheck)
        {
            AtomicCheck = atomicCheck;
            this.AtomicCheck.AtomicCheckState = (Byte)AtomicCheckStateType.NOT_APPLICABLE;
            // Atomic check not applicable is automatically validated
            if (!this.AtomicCheck.IsValidated())
                this.AtomicCheck.ValidationState.ToValidated();
        }

        public override AtomicCheckStateType GetCode()
        {
            return AtomicCheckStateType.NOT_APPLICABLE;
        }

        public override bool IsNotApplicable()
        {
            return true;
        }

        public override void ToNew()
        {
            this.AtomicCheck.setState(AtomicCheckStateType.NEW);
        }

        public override void ToDeactivated()
        {
            this.AtomicCheck.setState(AtomicCheckStateType.DEACTIVATED);
        }

    }
}