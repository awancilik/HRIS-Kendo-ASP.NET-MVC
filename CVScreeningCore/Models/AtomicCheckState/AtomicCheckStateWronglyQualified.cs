using CVScreeningCore.Exception;

namespace CVScreeningCore.Models.AtomicCheckState
{
    public class AtomicCheckStateWronglyQualified : AtomicCheckState
    {
        public const string kAtomicCheckStateWronglyQualified = "Wrongly qualified";

        public AtomicCheckStateWronglyQualified(AtomicCheck atomicCheck)
        {
            AtomicCheck = atomicCheck;
            atomicCheck.AtomicCheckState = (byte)AtomicCheckStateType.WRONGLY_QUALIFIED;

            // Atomic check cannot be wrongly qualified
            if (!this.AtomicCheck.CanBeWronglyQualified())
            {
                throw new ExceptionAtomicCheckWronglyQualifiedImpossible();
            }

            // Atomic check wrongly qualified is automatically validated if it is not an neighborhood check
            // When a neighborhood check is wrongly qualified, no new atomic check is created after requalification
            // When an atomic check that is not neighborhood check is wongly qualified, new atomic check are created after requalification
            if (!this.AtomicCheck.IsNeighborhoodCheck() && !this.AtomicCheck.IsValidated())
                this.AtomicCheck.ValidationState.ToValidated();

        }

        public override AtomicCheckStateType GetCode()
        {
            return AtomicCheckStateType.WRONGLY_QUALIFIED;
        }

        public override bool IsWronglyQualified()
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

        public override void ToNotApplicable()
        {
            this.AtomicCheck.setState(AtomicCheckStateType.NOT_APPLICABLE);
        }

        public override void ToDisabled()
        {
            this.AtomicCheck.setState(AtomicCheckStateType.DISABLED);
        }
    }
}