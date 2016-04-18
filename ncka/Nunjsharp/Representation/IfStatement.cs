using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nunjsharp.Representation
{
    public class IfStatement : TemplateItem
    {
        public readonly VariableLookup Expression;
        public readonly List<Textblock> ThenStatements = new List<Textblock>();
        public readonly List<Textblock> ElseStatements = new List<Textblock>();

        public IfStatement(VariableLookup expression)
        {
            Expression = expression;
        }


        internal override void Render(RenderContext context, object data)
        {

        }
    }
}
