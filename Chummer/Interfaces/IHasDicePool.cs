using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chummer
{
    public interface IHasDicePool
    {
        string DicePoolTooltip { get; }
        int DicePool { get; }
    }
}
