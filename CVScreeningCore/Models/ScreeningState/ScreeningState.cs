using System;
using System.Linq;

namespace CVScreeningCore.Models.ScreeningState
{
    public enum ScreeningStateType
    {
        NEW = 1,
        OPEN,
        VALIDATED,
        SUBMITTED,
        UPDATING,
        DEACTIVATED
    }


    public abstract class ScreeningState
    {
        protected Screening Screening;

        public string Name { get; set; }

        public virtual ScreeningStateType GetCode()
        {
            throw new NotImplementedException();
        }

        public virtual bool IsUpdating()
        {
            return false;        
        }
        
        public virtual bool IsNew()
        {
            return false;
        }

        public virtual bool IsOpen()
        {
            return false;
        }

        public virtual bool IsValidated()
        {
            return false;
        }

        public virtual bool IsSubmitted()
        {
            return false;
        }

        public virtual bool IsDeactivated()
        {
            return false;
        }

        public virtual void ToNew()
        {
            throw new System.NotImplementedException();
        }

        public virtual void ToOpen()
        {
            throw new System.NotImplementedException();
        }

        public virtual void ToValidated()
        {
            throw new System.NotImplementedException();
        }

        public virtual void ToUpdating()
        {
            throw new System.NotImplementedException();
        }

        public virtual void ToSubmitted()
        {
            throw new System.NotImplementedException();
        }

        public void ToDeactivated()
        {
            foreach (var atomicCheck in Screening.AtomicCheck)
            {
                atomicCheck.State.ToDeactivated();
            }

            foreach (var atomicCheck in Screening.AtomicCheck.Where(atomicCheck => !atomicCheck.IsValidated()))
            {
                atomicCheck.ValidationState.ToValidated();
            }


            Screening.setState(ScreeningStateType.DEACTIVATED);
        }
    }
}