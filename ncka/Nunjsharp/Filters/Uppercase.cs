using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nunjsharp.Filters
{
    public class Uppercase : Nunjsharp.Representation.FilterMethod
    {
        public override string Apply(List<string> parameters)
        {
            return parameters[0].ToUpper();
        }
    }
}
