using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack.Utilities
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class ImplicitModHookAttribute :  Attribute
    {
        public ImplicitModHookAttribute()
        {
        }
    }
}
