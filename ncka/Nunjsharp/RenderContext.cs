using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nunjsharp
{
    public class RenderContext
    {
        public readonly Environment Environment;
        public readonly StringBuilder Result = new StringBuilder();
        public readonly List<Representation.Template> TemplateHierarchy = new List<Representation.Template>();

        public RenderContext(Environment environment)
        {
            Environment = environment;
        }
    }
}
