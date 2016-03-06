using System;
using System.Collections.Generic;
using System.Linq;
using CVScreeningCore.Models.AtomicCheckValidationState;

namespace CVScreeningCore.Models.AtomicCheckState
{
    public enum AtomicCheckStateType
    {
        NEW = 1, 
        ON_GOING, 
        DONE_OK, 
        DONE_KO, 
        DONE_DISCREPANCY,
        DONE_IMPOSSIBLE,
        ON_PROCESS_FORWARDED,
        PENDING_CONFIRMATION,
        WRONGLY_QUALIFIED,
        NOT_APPLICABLE,
        DEACTIVATED,
        DISABLED
    }


    /// <summary>
    /// Partial class related to atomic check
    /// </summary>
    public abstract class AtomicCheckState
    {
        /// <summary>
        /// Atomic check reference
        /// </summary>
        protected AtomicCheck AtomicCheck;

        /// <summary>
        /// Atomic check name member
        /// </summary>
        private string _name;
        
        /// <summary>
        /// Atomic check name properties
        /// </summary>
        public string Name
        {
            get
            {
                return AtomicCheckStateFactory.GetStateAsString(
                        (AtomicCheckStateType) this.AtomicCheck.AtomicCheckState);
            }
            set { _name = value; }
        }

        /// <summary>
        /// Get the next transition method that needs to be called to go the next state
        /// </summary>
        /// <param name="nextStateType">Next state to go to</param>
        /// <returns></returns>
        public Action GetNextTransitionAsAction(AtomicCheckStateType nextStateType)
        {
            switch (nextStateType)
            {
                case AtomicCheckStateType.NEW:
                    return this.ToNew;
                case AtomicCheckStateType.ON_GOING:
                    return this.ToOnGoing;
                case AtomicCheckStateType.DONE_OK:
                    return this.ToDoneOk;
                case AtomicCheckStateType.DONE_KO:
                    return this.ToDoneKo;
                case AtomicCheckStateType.DONE_DISCREPANCY:
                    return this.ToDoneDiscrepancy;
                case AtomicCheckStateType.DONE_IMPOSSIBLE:
                    return this.ToDoneImpossible;
                case AtomicCheckStateType.ON_PROCESS_FORWARDED:
                    return this.ToOnProcessForwarded;
                case AtomicCheckStateType.PENDING_CONFIRMATION:
                    return this.ToPendingConfirmation;
                case AtomicCheckStateType.WRONGLY_QUALIFIED:
                    return this.ToWronglyQualified;
                case AtomicCheckStateType.NOT_APPLICABLE:
                    return this.ToNotApplicable;
                case AtomicCheckStateType.DEACTIVATED:
                    return this.ToDeactivated;
                case AtomicCheckStateType.DISABLED:
                    return this.ToDisabled;
                default:
                    throw new ArgumentException("Invalid atomic check state type", "type");
            }
        }

        /// <summary>
        /// Get dictionnary with all the atomic check status, key is the atomic check state and value is string 
        /// representing this atomic check state
        /// </summary>
        /// <returns></returns>
        public static IDictionary<int, string> GetAllStatesAsDictionary()
        {
            return Enum.GetValues(
                typeof (AtomicCheckStateType)).Cast<AtomicCheckStateType>().ToDictionary(value => (int) value,
                    AtomicCheckStateFactory.GetStateAsString);
        }


        /// <summary>
        /// Get dictionnary with all the active atomic check status, key is the atomic check state and value is string 
        /// representing this atomic check state.
        /// </summary>
        /// <returns></returns>
        public static IDictionary<int, string> GetActivateStateAsDictionnary()
        {
            return Enum.GetValues(
                typeof(AtomicCheckStateType)).Cast<AtomicCheckStateType>()
                .Where( u => u != AtomicCheckStateType.NEW && u != AtomicCheckStateType.NOT_APPLICABLE && 
                    u != AtomicCheckStateType.DEACTIVATED).ToDictionary(value => (int)value,
                    AtomicCheckStateFactory.GetStateAsString);
        }



        /// <summary>
        /// Get dictionnary with all the next possible atomic check state
        /// representing this atomic check state.
        /// </summary>
        /// <returns></returns>
        public static IDictionary<int, string> GetNextStatesAsDictionnary(
            AtomicCheckValidationStateType state)
        {
            switch (state)
            {
                case AtomicCheckValidationStateType.NOT_PROCESSED:
                case AtomicCheckValidationStateType.REJECTED:

                    return Enum.GetValues(
                        typeof(AtomicCheckStateType)).Cast<AtomicCheckStateType>().Where(
                            u => u != AtomicCheckStateType.NEW && u != AtomicCheckStateType.NOT_APPLICABLE &&
                                 u != AtomicCheckStateType.DEACTIVATED)
                        .ToDictionary(value => (int)value,
                            AtomicCheckStateFactory.GetStateAsString);

                case AtomicCheckValidationStateType.PROCESSED:
                case AtomicCheckValidationStateType.VALIDATED:

                    return Enum.GetValues(
                        typeof(AtomicCheckStateType)).Cast<AtomicCheckStateType>().Where(
                            u => u != AtomicCheckStateType.NEW && u != AtomicCheckStateType.NOT_APPLICABLE &&
                                 u != AtomicCheckStateType.DEACTIVATED && u != AtomicCheckStateType.ON_GOING &&
                                 u != AtomicCheckStateType.WRONGLY_QUALIFIED && u != AtomicCheckStateType.ON_PROCESS_FORWARDED)
                        .ToDictionary(value => (int)value,
                            AtomicCheckStateFactory.GetStateAsString);
                default:
                    throw new ArgumentException("Invalid atomic check validation state type", "type");
            }
        }



        public void Initialize(AtomicCheck atomicCheck)
        {
            AtomicCheck = atomicCheck;
        }

        public virtual AtomicCheckStateType GetCode()
        {
            throw new NotImplementedException();
        }

        public virtual bool IsNew()
        {
            return false;
        }

        public virtual void ToNew()
        {
            throw new System.NotImplementedException();
        }

        public virtual bool IsOnGoing()
        {
            return false;
        }

        public virtual void ToOnGoing()
        {
            throw new System.NotImplementedException();
        }

        public virtual bool IsDoneOk()
        {
            return false;
        }
    
        public virtual void ToDoneOk()
        {
            throw new System.NotImplementedException();
        }

        public virtual bool IsDoneKo()
        {
            return false;
        }

        public virtual void ToDoneKo()
        {
            throw new System.NotImplementedException();
        }
        
        public virtual bool IsDoneImpossible()
        {
            return false;
        }

        public virtual void ToDoneImpossible()
        {
            throw new System.NotImplementedException();
        }

        public virtual bool IsDoneDiscrepancy()
        {
            return false;
        }

        public virtual void ToDoneDiscrepancy()
        {
            throw new System.NotImplementedException();
        }

        public virtual bool IsOnProcessForwarded()
        {
            return false;
        }

        public virtual void ToOnProcessForwarded()
        {
            throw new System.NotImplementedException();
        }

        public virtual bool IsPendingConfirmation()
        {
            return false;
        }

        public virtual void ToPendingConfirmation()
        {
            throw new System.NotImplementedException();
        }

        public virtual bool IsWronglyQualified()
        {
            return false;
        }

        public virtual void ToWronglyQualified()
        {
            throw new System.NotImplementedException();
        }

        public virtual bool IsNotApplicable()
        {
            return false;
        }

        public virtual void ToNotApplicable()
        {
            throw new System.NotImplementedException();
        }

        public virtual bool IsDeactivated()
        {
            return false;
        }

        public virtual void ToDeactivated()
        {
            this.AtomicCheck.State = new AtomicCheckStateDeactivated(this.AtomicCheck);
        }

        public virtual bool IsDisabled()
        {
            return false;
        }

        public virtual void ToDisabled()
        {
            throw new System.NotImplementedException();
        }
    }

    
}
