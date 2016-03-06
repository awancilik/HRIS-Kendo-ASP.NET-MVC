using System;

namespace CVScreeningCore.Models.AtomicCheckState
{
    public class AtomicCheckStateNew : AtomicCheckState
    {
        public const string kAtomicCheckStateNew = "New";

        public AtomicCheckStateNew(AtomicCheck atomicCheck)
        {
            AtomicCheck = atomicCheck;
            this.AtomicCheck.AtomicCheckState = (Byte)AtomicCheckStateType.NEW;
            if (!this.AtomicCheck.IsNotProcessed())
                this.AtomicCheck.ValidationState.ToNotProcessed();
        }

        public override AtomicCheckStateType GetCode()
        {
            return AtomicCheckStateType.NEW;
        }

        public override void ToNotApplicable()
        {
            this.AtomicCheck.setState(AtomicCheckStateType.NOT_APPLICABLE);
        }

        public override void ToOnGoing()
        {
            this.AtomicCheck.setState(AtomicCheckStateType.ON_GOING);
        }

        public override void ToDeactivated()
        {
            this.AtomicCheck.setState(AtomicCheckStateType.DEACTIVATED);
        }

        public override bool IsNew()
        {
            return true;
        }
    }
}