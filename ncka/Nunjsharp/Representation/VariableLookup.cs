using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Nunjsharp.Representation
{
    public class VariableLookup : TemplateItem
    {
        public readonly List<string> Path = new List<string>();
        public readonly Template Parent;
        public readonly List<FilterMethod> Filters = new List<FilterMethod>();

        public VariableLookup(Template parent)
        {
            Parent = parent;
        }
        private void RenderSuperTemplate(RenderContext context, object data)
        {
            // this is the "super" template
            string name = Parent.Name;
            var block = Parent.Parent.Parent.FindBlockNamed(name);
            if(block != null)
            {
                // super does not support shit 
                block.Render(context, data);
            }
        }

        internal override void Render(RenderContext context, object data)
        {
            // special handling for "super" lookup
            if((Path.Count == 1) && (Path[0] == "super()"))
            {
                RenderSuperTemplate(context, data);
            }
            else
            {
                var tempPath = new StringBuilder();
                for (int index = 0, maxIndex = Path.Count; index < maxIndex; ++index)
                {
                    string name = Path[index];
                    if (index > 0)
                        tempPath.Append(".");
                    tempPath.Append(name);
                    Type t = data.GetType();
                    FieldInfo fi = t.GetField(name);
                    if (fi == null)
                    {
                        Trace.TraceWarning("- did not find '{0}', data not rendered", tempPath.ToString());
                        break;
                    }
                    if (index == (maxIndex - 1))
                    {
                        ApplyFilters(context, fi.GetValue(data).ToString());
                    }
                    else
                    {
                        data = fi.GetValue(data);
                    }
                }
            }
        }

        private void ApplyFilters(RenderContext context, string data)
        {
            foreach(var filter in Filters)
            {
                List<string> args = new List<string>();
                args.Add(data);
                data = filter.Apply(args);
            }
            context.Result.Append(data);
        }
    }
}
