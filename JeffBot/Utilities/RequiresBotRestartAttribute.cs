using System;

namespace JeffBot
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RequiresBotRestartAttribute : Attribute
    {
    }
}