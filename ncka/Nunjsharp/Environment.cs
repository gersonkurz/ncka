using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nunjsharp
{
    public class Environment
    {
        public readonly Dictionary<string, Nunjsharp.Representation.Template> KnownTemplates = new Dictionary<string, Nunjsharp.Representation.Template>();



        public void ParseTemplate(string name, string content)
        {
            var template = new TemplateParser(name, content).Result;
            KnownTemplates[template.Name] = template;
        }

        public string RenderTemplate(Representation.Template template, object data)
        {
            var context = new RenderContext(this);

            context.TemplateHierarchy.Add(template);
            while(!string.IsNullOrEmpty(template.Extends))
            {
                var parentTemplate = KnownTemplates[template.Extends];
                template.Parent = parentTemplate;
                context.TemplateHierarchy.Add(parentTemplate);
                template = parentTemplate;
            }

            foreach (var item in template.Items)
            {
                item.Render(context, data);
            }
            return context.Result.ToString();
        }

        public string RenderTemplate(string templateName, object data)
        {
            return RenderTemplate(KnownTemplates[templateName], data);
        }

    }
}
