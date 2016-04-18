using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nunjsharp.Representation
{
    public class Textblock : TemplateItem
    {
        public readonly string Text;
        public readonly string Name;

        public Textblock(string text)
        {
            Text = text;
        }

        internal override void Render(RenderContext context, object data)
        {
            context.Result.Append(Text);
        }
    }
}
