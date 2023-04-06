using System.Linq;
using System.Reflection;

namespace JeffBot
{
    public class BotSettingsBase
    {
        #region HaveRebootRequiredPropertiesChanged
        public bool HaveRebootRequiredPropertiesChanged(object newObject)
        {
            var objectType = this.GetType();
            var propertiesToCheck = objectType.GetProperties()
                .Where(p => p.GetCustomAttribute<RequiresBotRestartAttribute>() != null)
                .ToList();

            foreach (var property in propertiesToCheck)
            {
                var oldProperty = property.GetValue(this);
                var newProperty = property.GetValue(newObject);

                if (!object.Equals(oldProperty, newProperty))
                {
                    return true;
                }
            }
            return false;
        } 
        #endregion
    }
}