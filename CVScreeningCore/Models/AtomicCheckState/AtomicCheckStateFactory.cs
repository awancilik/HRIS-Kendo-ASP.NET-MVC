using System;

namespace CVScreeningCore.Models.AtomicCheckState
{
    /// <summary>
    /// Fatory class to retrieve atomic check object
    /// </summary>
    public class AtomicCheckStateFactory
    {
        /// <summary>
        /// Create a new atomic check state
        /// </summary>
        /// <param name="atomicCheck"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static AtomicCheckState Get(AtomicCheck atomicCheck, AtomicCheckStateType type)
        {
            switch (type)
            {
                case AtomicCheckStateType.NEW:
                    return new AtomicCheckStateNew(atomicCheck);
                case AtomicCheckStateType.ON_GOING:
                    return new AtomicCheckStateOnGoing(atomicCheck);
                case AtomicCheckStateType.DONE_OK:
                    return new AtomicCheckStateDoneOk(atomicCheck);
                case AtomicCheckStateType.DONE_KO:
                    return new AtomicCheckStateDoneKo(atomicCheck);
                case AtomicCheckStateType.DONE_DISCREPANCY:
                    return new AtomicCheckStateDoneDiscrepancy(atomicCheck);
                case AtomicCheckStateType.DONE_IMPOSSIBLE:
                    return new AtomicCheckStateDoneImpossible(atomicCheck);
                case AtomicCheckStateType.ON_PROCESS_FORWARDED:
                    return new AtomicCheckStateOnProcessForwarded(atomicCheck);
                case AtomicCheckStateType.PENDING_CONFIRMATION:
                    return new AtomicCheckStatePendingConfirmation(atomicCheck);
                case AtomicCheckStateType.WRONGLY_QUALIFIED:
                    return new AtomicCheckStateWronglyQualified(atomicCheck);
                case AtomicCheckStateType.NOT_APPLICABLE:
                    return new AtomicCheckStateNotApplicable(atomicCheck);
                case AtomicCheckStateType.DEACTIVATED:
                    return new AtomicCheckStateDeactivated(atomicCheck);
                case AtomicCheckStateType.DISABLED:
                    return new AtomicCheckStateDisabled(atomicCheck);
                default:
                    throw new ArgumentException("Invalid atomic check state type", "type");
            }
        }
        
        /// <summary>
        /// Get atomic check state as string
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetStateAsString(AtomicCheckStateType type)
        {
            switch (type)
            {
                case AtomicCheckStateType.NEW:
                    return AtomicCheckStateNew.kAtomicCheckStateNew;
                case AtomicCheckStateType.ON_GOING:
                    return AtomicCheckStateOnGoing.kAtomicCheckStateOnGoing;
                case AtomicCheckStateType.DONE_OK:
                    return AtomicCheckStateDoneOk.kAtomicCheckStateDoneOk;
                case AtomicCheckStateType.DONE_KO:
                    return AtomicCheckStateDoneKo.kAtomicCheckStateDoneKo;
                case AtomicCheckStateType.DONE_DISCREPANCY:
                    return AtomicCheckStateDoneDiscrepancy.kAtomicCheckStateDoneDiscrepancy;
                case AtomicCheckStateType.DONE_IMPOSSIBLE:
                    return AtomicCheckStateDoneImpossible.kAtomicCheckStateDoneImpossible;
                case AtomicCheckStateType.ON_PROCESS_FORWARDED:
                    return AtomicCheckStateOnProcessForwarded.kAtomicCheckStateOnProcessForwarded;
                case AtomicCheckStateType.PENDING_CONFIRMATION:
                    return AtomicCheckStatePendingConfirmation.kAtomicCheckStatePendingConfirmation;
                case AtomicCheckStateType.WRONGLY_QUALIFIED:
                    return AtomicCheckStateWronglyQualified.kAtomicCheckStateWronglyQualified;
                case AtomicCheckStateType.NOT_APPLICABLE:
                    return AtomicCheckStateNotApplicable.kAtomicCheckStateNotApplicable;
                case AtomicCheckStateType.DEACTIVATED:
                    return AtomicCheckStateDeactivated.kAtomicCheckStateDeactivated;
                case AtomicCheckStateType.DISABLED:
                    return AtomicCheckStateDisabled.kAtomicCheckStateDisabled;
                default:
                    throw new ArgumentException("Invalid atomic check state type", "type");
            }
        }

    }
}
