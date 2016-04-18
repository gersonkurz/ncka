using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nunjsharp.Representation
{
    public abstract class TemplateItem
    {
        internal abstract void Render(RenderContext context, object data);
    }
}
