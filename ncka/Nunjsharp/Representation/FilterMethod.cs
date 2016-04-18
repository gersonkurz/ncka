using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nunjsharp.Representation
{
    public abstract class FilterMethod
    {
        /**
         * \brief   Applies the given parameters.
         *
         * \param   parameters  Options for controlling the operation.
         *                      Every filter method must support at least one input parameter,
         *                      but it can support any number of additional ones for fine control
         *
         * \return  A string.
         */

        public abstract string Apply(List<string> parameters); 
    }
}
