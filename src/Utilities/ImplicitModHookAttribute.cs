using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PitchBlack
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class ImplicitModHookAttribute :  Attribute
    {
        public ImplicitModHookAttribute()
        {
        }
    }
}
