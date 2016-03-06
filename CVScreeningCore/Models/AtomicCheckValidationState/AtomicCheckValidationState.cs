using System;
using System.Collections.Generic;
using System.Linq;
using CVScreeningCore.Models.AtomicCheckState;

namespace CVScreeningCore.Models.AtomicCheckValidationState
{
    public enum AtomicCheckValidationStateType
    {
        NOT_PROCESSED = 1,
        PROCESSED,
        REJECTED,
        VALIDATED
    }

    public abstract class AtomicCheckValidationState
    {
        /// <summary>
        /// Atomic check reference
        /// </summary>
        protected AtomicCheck AtomicCheck;

        /// <summary>
        /// Atomic check validation name member
        /// </summary>
        private string _name;

        /// <summary>
        /// Atomic check name properties
        /// </summary>
        public string Name
        {
            get
            {
                return AtomicCheckValidationStateFactory.GetStateAsString(
                        (AtomicCheckValidationStateType)this.AtomicCheck.AtomicCheckValidationState);
            }
            set { _name = value; }
        }

        public virtual AtomicCheckValidationStateType GetCode()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the next transition method that needs to be called to go the next validation state
        /// </summary>
        /// <param name="nextStateType">Next state to go to</param>
        /// <returns></returns>
        public Action GetNextTransitionAsAction(AtomicCheckValidationStateType nextStateType)
        {
            switch (nextStateType)
            {
                case AtomicCheckValidationStateType.NOT_PROCESSED:
                    return this.ToNotProcessed;
                case AtomicCheckValidationStateType.PROCESSED:
                    return this.ToProcessed;
                case AtomicCheckValidationStateType.REJECTED:
                    return this.ToRejected;
                case AtomicCheckValidationStateType.VALIDATED:
                    return this.ToValidated;
                default:
                    throw new ArgumentException("Invalid atomic check validation state type", "type");
            }
        }

        /// <summary>
        /// Get dictionnary with all the next possible atomic check validation state
        /// representing this atomic check state.
        /// </summary>
        /// <returns></returns>
        public static IDictionary<int, string> GetNextValidationStatesAsDictionnary(AtomicCheckValidationStateType state)
        {

            switch (state)
            {
                case AtomicCheckValidationStateType.NOT_PROCESSED:
                    return Enum.GetValues(
                        typeof(AtomicCheckValidationStateType)).Cast<AtomicCheckValidationStateType>().Where(
                            u => u == AtomicCheckValidationStateType.NOT_PROCESSED
                                || u == AtomicCheckValidationStateType.PROCESSED).ToDictionary(
                        value => (int)value, AtomicCheckValidationStateFactory.GetStateAsString);
                case AtomicCheckValidationStateType.REJECTED:
                    return Enum.GetValues(
                        typeof(AtomicCheckValidationStateType)).Cast<AtomicCheckValidationStateType>().Where(
                            u => u == AtomicCheckValidationStateType.REJECTED
                                || u == AtomicCheckValidationStateType.PROCESSED).ToDictionary(
                        value => (int)value, AtomicCheckValidationStateFactory.GetStateAsString);
                    
                case AtomicCheckValidationStateType.PROCESSED:
                    return Enum.GetValues(
                        typeof(AtomicCheckValidationStateType)).Cast<AtomicCheckValidationStateType>().Where(
                            u => u != AtomicCheckValidationStateType.NOT_PROCESSED).ToDictionary(
                        value => (int)value, AtomicCheckValidationStateFactory.GetStateAsString);

                case AtomicCheckValidationStateType.VALIDATED:
                    return Enum.GetValues(
                        typeof(AtomicCheckValidationStateType)).Cast<AtomicCheckValidationStateType>().Where(
                            u => u == AtomicCheckValidationStateType.VALIDATED 
                                || u == AtomicCheckValidationStateType.REJECTED).ToDictionary(
                        value => (int)value, AtomicCheckValidationStateFactory.GetStateAsString);

                default:
                    throw new ArgumentException("Invalid atomic check validation state type", "type");
            }
        }


        /// <summary>
        /// Get dictionnary with all the atomic check status, key is the atomic check state and value is string 
        /// representing this atomic check state
        /// </summary>
        /// <returns></returns>
        public static IDictionary<int, string> GetAllValidationStatesAsDictionary()
        {
            return Enum.GetValues(
                typeof(AtomicCheckValidationStateType)).Cast<AtomicCheckValidationStateType>().ToDictionary(value => (int)value,
                    AtomicCheckValidationStateFactory.GetStateAsString);
        }

        public void Initialize(AtomicCheck atomicCheck)
        {
            AtomicCheck = atomicCheck;
        }

        public virtual bool IsNotProcessed()
        {
            return false;
        }

        public virtual void ToNotProcessed()
        {
            throw new System.NotImplementedException();
        }

        public virtual bool IsProcessed()
        {
            return false;
        }

        public virtual void ToProcessed()
        {
            throw new System.NotImplementedException();
        }

        public virtual bool IsRejected()
        {
            return false;
        }
    
        public virtual void ToRejected()
        {
            throw new System.NotImplementedException();
        }

        public virtual bool IsValidated()
        {
            return false;
        }

        public virtual void ToValidated()
        {
            throw new System.NotImplementedException();
        }
    }
}
