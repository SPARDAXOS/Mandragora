using UnityEngine;

namespace Mandragora {
    public class Utility {
        public enum ValidationType {
            DEBUG, WARNING, ERROR
        }

        public static bool Validate(Object target, string message, ValidationType type) {
            if (!target) {
                if (message == null)
                    return false;

                if (type == ValidationType.DEBUG)
                    Debug.Log(message);
                else if (type == ValidationType.WARNING)
                    Debug.LogWarning(message);
                else if (type == ValidationType.ERROR)
                    Debug.LogError(message);

                return false;
            }

            return true;
        }


    }
}
