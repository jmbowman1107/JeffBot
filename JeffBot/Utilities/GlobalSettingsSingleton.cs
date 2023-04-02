using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JeffBot
{
    public class GlobalSettingsSingleton
    {
        private static GlobalSettings _instance;
        private static bool _isInitialized;

        private GlobalSettingsSingleton() { }

        public static GlobalSettings Instance
        {
            get
            {
                if (!_isInitialized)
                {
                    throw new InvalidOperationException("GlobalSettingsSingleton must be initialized before accessing the instance.");
                }
                return _instance;
            }
        }

        public static void Initialize(GlobalSettings globalSettings)
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException("GlobalSettingsSingleton has already been initialized.");
            }

            _instance = globalSettings;
            _isInitialized = true;
        }
    }
}
