namespace CVScreeningCore.Models.AtomicCheckState
{
    public class AtomicCheckStateDeactivated : AtomicCheckState
    {
        public const string kAtomicCheckStateDeactivated = "Deactivated";

        public AtomicCheckStateDeactivated(AtomicCheck atomicCheck)
        {
            this.AtomicCheck = atomicCheck;
            this.AtomicCheck.AtomicCheckState = (byte) AtomicCheckStateType.DEACTIVATED;
        }

        public override AtomicCheckStateType GetCode()
        {
            return AtomicCheckStateType.DEACTIVATED;
        }

        public override bool IsDeactivated()
        {
            return true;
        }
    }
}