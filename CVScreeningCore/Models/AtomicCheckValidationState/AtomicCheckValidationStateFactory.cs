using System;
using CVScreeningCore.Models.AtomicCheckState;
using CVScreeningCore.Models.AtomicCheckValidationState;

namespace CVScreeningCore.Models.AtomicCheckValidationState
{
    /// <summary>
    /// Fatory class to retrieve atomic check object
    /// </summary>
    public class AtomicCheckValidationStateFactory
    {
        /// <summary>
        /// Create a new atomic check validation state
        /// </summary>
        /// <param name="atomicCheck"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static  AtomicCheckValidationState Get(AtomicCheck atomicCheck, AtomicCheckValidationStateType type)
        {
            switch (type)
            {
                case AtomicCheckValidationStateType.NOT_PROCESSED:
                    return new AtomicCheckValidationStateNotProcessed(atomicCheck);
                case AtomicCheckValidationStateType.PROCESSED:
                    return new AtomicCheckValidationStateProcessed(atomicCheck);
                case AtomicCheckValidationStateType.REJECTED:
                    return new AtomicCheckValidationStateRejected(atomicCheck);
                case AtomicCheckValidationStateType.VALIDATED:
                    return new AtomicCheckValidationStateValidated(atomicCheck);
                default:
                    throw new ArgumentException("Invalid atomic check validation state type", "type");
            }
        }
        
        /// <summary>
        /// Get atomic check validation state as string
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetStateAsString(AtomicCheckValidationStateType type)
        {
            switch (type)
            {
                case AtomicCheckValidationStateType.NOT_PROCESSED:
                    return AtomicCheckValidationStateNotProcessed.kAtomicCheckValidationStateNotProcessed;
                case AtomicCheckValidationStateType.PROCESSED:
                    return AtomicCheckValidationStateProcessed.kAtomicCheckValidationStateProcessed;
                case AtomicCheckValidationStateType.REJECTED:
                    return AtomicCheckValidationStateRejected.kAtomicCheckValidationStateRejected;
                case AtomicCheckValidationStateType.VALIDATED:
                    return AtomicCheckValidationStateValidated.kAtomicCheckValidationStateValidated;
                default:
                    throw new ArgumentException("Invalid atomic check validation state type", "type");
            }
        }

    }
}
