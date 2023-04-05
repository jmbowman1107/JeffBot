using System;

namespace JeffBot
{
    public class Singleton<T>
    {
        private static T _instance;
        private static bool _isInitialized;

        #region Instance
        public static T Instance
        {
            get
            {
                if (!_isInitialized)
                {
                    throw new InvalidOperationException($"Singleton<{typeof(T).Name}> must be initialized before accessing the instance.");
                }
                return _instance;
            }
        }
        #endregion

        #region Constructor
        private Singleton() { }
        #endregion

        #region Initialize
        public static void Initialize(T globalSettings)
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException($"Singleton<{typeof(T).Name} has already been initialized.");
            }

            _instance = globalSettings;
            _isInitialized = true;
        }
        #endregion
    }
}