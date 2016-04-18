using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Nunjsharp.Representation
{
    public class Template : TemplateItem
    {
        public readonly string Name;
        public string Extends;
        public Template Parent;
        public readonly List<TemplateItem> Items = new List<TemplateItem>();

        public Template(string name, Template parent)
        {
            Name = name;
            Parent = parent;
        }

        internal Template FindBlockNamed(string name)
        {
            foreach(TemplateItem ti in Items)
            {
                Template t = ti as Template;
                if( t != null )
                {
                    if(t.Name == name)
                    {
                        return t;
                    }
                }
            }
            return null;
        }

        internal override void Render(RenderContext context, object data)
        {
            foreach(var template in context.TemplateHierarchy)
            {
                var replacement = template.FindBlockNamed(Name);
                if(replacement != null)
                {
                    context.Result.Append(context.Environment.RenderTemplate(replacement, data));
                    return;
                }
            }

            context.Result.Append(context.Environment.RenderTemplate(this, data));
        }
    }
}
