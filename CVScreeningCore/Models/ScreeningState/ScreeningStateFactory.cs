using System;
using System.Collections.Generic;
using System.Linq;
using CVScreeningCore.Exception;

namespace CVScreeningCore.Models.ScreeningState
{
    public class ScreeningStateFactory
    {
        public ScreeningState Get(Screening screening, ScreeningStateType type)
        {
            switch (type)
            {
                case ScreeningStateType.NEW:
                    return new ScreeningStateNew(screening);
                case ScreeningStateType.OPEN:
                    return new ScreeningStateOpen(screening);
                case ScreeningStateType.SUBMITTED:
                    return new ScreeningStateSubmitted(screening);
                case ScreeningStateType.VALIDATED:
                    return new ScreeningStateValidated(screening);
                case ScreeningStateType.DEACTIVATED:
                    return new ScreeningStateDeactivated(screening);
                case ScreeningStateType.UPDATING:
                    return new ScreeningStateUpdating(screening);
                default:
                    throw new ExceptionScreeningTypeNotFound("Invalid screening state " + Convert.ToString(type));
            }
        }

        /// <summary>
        /// Get atomic check validation state as string
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetStateAsString(ScreeningStateType type)
        {
            switch (type)
            {
                case ScreeningStateType.NEW:
                    return ScreeningStateNew.kScreeningStateNew;
                case ScreeningStateType.OPEN:
                    return ScreeningStateOpen.kScreeningStateOpen;
                case ScreeningStateType.SUBMITTED:
                    return ScreeningStateSubmitted.kScreeningStateSubmitted;
                case ScreeningStateType.VALIDATED:
                    return ScreeningStateValidated.kScreeningStateValidated;
                case ScreeningStateType.DEACTIVATED:
                    return ScreeningStateDeactivated.kScreeningStateDeactivated;
                case ScreeningStateType.UPDATING:
                    return ScreeningStateUpdating.kScreeningStateUpdating;
                default:
                    throw new ExceptionScreeningTypeNotFound("Invalid screening state " + Convert.ToString(type));
            }
        }

        /// <summary>
        /// Get atomic check validation state as string
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetStateAsStringForClient(ScreeningStateType type)
        {
            switch (type)
            {
                case ScreeningStateType.NEW:
                    return ScreeningStateNew.kScreeningStateNew;
                case ScreeningStateType.OPEN:
                case ScreeningStateType.VALIDATED:
                    return ScreeningStateOpen.kScreeningStateOpen;
                case ScreeningStateType.SUBMITTED:
                    return ScreeningStateSubmitted.kScreeningStateSubmitted;
                case ScreeningStateType.DEACTIVATED:
                    return ScreeningStateDeactivated.kScreeningStateDeactivated;
                case ScreeningStateType.UPDATING:
                    return ScreeningStateUpdating.kScreeningStateUpdating;
                default:
                    throw new ExceptionScreeningTypeNotFound("Invalid screening state " + Convert.ToString(type));
            }
        }

        public static IDictionary<int, string> GetAllStatus()
        {
            IDictionary<int,string> statusDictionary = new Dictionary<int, string>();
            statusDictionary.Add(0,"Select Status...");
            foreach (ScreeningStateType type in Enum.GetValues(typeof(ScreeningStateType)))
            {
                statusDictionary.Add((int)type, type.ToString());
            }
            return statusDictionary;
        }

        public static IDictionary<int, string> GetAllStatusForClient()
        {
            IDictionary<int, string> statusDictionary = new Dictionary<int, string>();
            statusDictionary.Add(0, "Select Status...");
            statusDictionary.Add((int)ScreeningStateType.NEW, ScreeningStateType.NEW.ToString());
            statusDictionary.Add((int)ScreeningStateType.OPEN, ScreeningStateType.OPEN.ToString());
            statusDictionary.Add((int)ScreeningStateType.SUBMITTED, ScreeningStateType.SUBMITTED.ToString());
            statusDictionary.Add((int)ScreeningStateType.UPDATING, ScreeningStateType.UPDATING.ToString());
            return statusDictionary;
        }


        public static bool IsStatusAvailableInEnumList(int value)
        {
            return Enum.IsDefined(typeof (ScreeningStateType), value);
        }

    }
}