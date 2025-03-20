using System;

namespace IniBuilder
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class IniIgnoreAttribute : Attribute
    {
    }
}