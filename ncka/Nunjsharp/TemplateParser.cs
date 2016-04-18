using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Nunjsharp
{
    internal class TemplateParser
    {
        private delegate void ProcessFunc(char c);
        private ProcessFunc CurrentProcessFunc;
        private int LineNumber;
        private int ColumnNumber;
        private readonly StringBuilder Buffer = new StringBuilder();
        private readonly Stack<Representation.Template> Templates = new Stack<Representation.Template>();
        private Representation.VariableLookup CurrentVariableLookup;
        public Representation.Template Result;

        public static Dictionary<string, Representation.FilterMethod> KnownFilterMethods = new Dictionary<string, Representation.FilterMethod>()
        {
            { "upper", new Filters.Uppercase() }
        };

        public TemplateParser(string name, string content)
        {
            Result = new Representation.Template(name, null);
            Templates.Push(Result);
            CurrentProcessFunc = pf_Default;
            Buffer.Clear();
            LineNumber = 1;
            ColumnNumber = 1;

            foreach (char c in content)
            {
                CurrentProcessFunc(c);
                if(c == '\n')
                {
                    ++LineNumber;
                    ColumnNumber = 1;
                }
                else
                {
                    ++ColumnNumber;
                }
            }
            StoreCurrentBuffer();
            Trace.Assert(Templates.Count == 1);
            Trace.Assert(Templates.Peek() == Result);
        }

        private void StoreCurrentBuffer()
        {
            string content = Buffer.ToString();
            if(!string.IsNullOrEmpty(content))
            {
                // default assumption: buffer is a text block
                Templates.Peek().Items.Add(new Representation.Textblock(content));
            }
            Buffer.Clear();
        }

        private void FeedVariableFilterFunc()
        {
            string content = Buffer.ToString().Trim();

            if (!string.IsNullOrEmpty(content))
            {
                //  this is blatantly wrong, but for now it'll have to do
                CurrentVariableLookup.Filters.Add(KnownFilterMethods[content]);
            }
            Buffer.Clear();
        }

        private void FeedVariableLookupParameter()
        {
            string content = Buffer.ToString().Trim();

            if (!string.IsNullOrEmpty(content))
            {
                CurrentVariableLookup.Path.Clear();

                Buffer.Clear();
                int mode = 0;
                foreach(char c in content)
                {
                    if(mode == 1)
                    {
                        if(c == '"')
                        {
                            mode = 2;
                        }
                        else
                        {
                            throw new Exception(SyntaxError("Invalid path specifier: '{0}'", content));
                        }
                    }
                    else if( mode == 2)
                    {
                        if( c == '"')
                        {
                            mode = 3;
                            CurrentVariableLookup.Path.Add(Buffer.ToString());
                            Buffer.Clear();
                        }
                        else
                        {
                            Buffer.Append(c);
                        }
                    }
                    else if(mode == 3)
                    {
                        if(c == ']')
                        {
                            mode = 4;
                        }
                        else
                        {
                            throw new Exception(SyntaxError("Invalid path specifier: '{0}'", content));
                        }
                    }
                    else if (mode == 4)
                    {
                        if (c == '.')
                        {
                            mode = 0;
                        }
                        else if (c == '[')
                        {
                            mode = 1;
                        }
                        else
                        {
                            Buffer.Append(c);
                            mode = 0;
                        }
                    }
                    else if (mode == 0)
                    {
                        if (c == '.')
                        {
                            CurrentVariableLookup.Path.Add(Buffer.ToString());
                            Buffer.Clear();
                        }
                        else if (c == '[')
                        {
                            CurrentVariableLookup.Path.Add(Buffer.ToString());
                            Buffer.Clear();
                            mode = 1;
                        }
                        else
                        {
                            Buffer.Append(c);
                        }
                    }
                }
                if(Buffer.Length > 0)
                {
                    CurrentVariableLookup.Path.Add(Buffer.ToString());
                }
            }
            Buffer.Clear();
        }


        private void FeedGenericExpression()
        {
            string content = Buffer.ToString().Trim();

            if (!string.IsNullOrEmpty(content))
            {
                // HACK HACK HACK
                if(content.StartsWith("block "))
                {
                    string blockName = content.Substring(6);
                    var t = new Representation.Template(blockName, Templates.Peek());
                    Templates.Peek().Items.Add(t);
                    Templates.Push(t);
                    Trace.Assert(Templates.Peek() == t);

                }
                else if(content.Equals("endblock"))
                {
                    Trace.Assert(Templates.Count > 1);
                    Templates.Pop();
                }
                else if (content.StartsWith("extends "))
                {
                    string blockName = content.Substring(8).Trim();
                    Trace.Assert(blockName.StartsWith("\""));
                    Trace.Assert(blockName.EndsWith("\""));
                    blockName = blockName.Substring(1, blockName.Length - 2);
                    Templates.Peek().Extends = blockName;
                }
                else
                {
                    Trace.TraceError("Don't konw bloc: {0}", content);
                    Trace.Assert(false);
                }
            }
            Buffer.Clear();
        }

        private void pf_Default(char c)
        {
            if(c == '{')
            {
                CurrentProcessFunc = pf_StartOfCurlyBracketsExpression;
            }
            else
            {
                Buffer.Append(c);
            }
        }

        private void pf_StartOfCurlyBracketsExpression(char c)
        {
            if (c == '{')
            {
                StoreCurrentBuffer();
                CurrentVariableLookup = new Representation.VariableLookup(Templates.Peek());
                Templates.Peek().Items.Add( CurrentVariableLookup );
                CurrentProcessFunc = pf_RecordVariableLookup;
            }
            else if (c == '%')
            {
                StoreCurrentBuffer();
                CurrentVariableLookup = null;
                CurrentProcessFunc = pf_RecordGenericExpression;
            }
            else if (c == '#')
            {
                StoreCurrentBuffer();
                CurrentVariableLookup = null;
                CurrentProcessFunc = pf_RecordComment;
            }
            else
            {
                Buffer.Append('{');
                Buffer.Append(c);
                CurrentProcessFunc = pf_Default;
            }
        }
        private void pf_RecordComment(char c)
        {
            if (c == '#')
            {
                CurrentProcessFunc = pf_ExpectEndOfComment;
            }
        }

        private void pf_ExpectEndOfComment(char c)
        {
            if (c == '}')
            {
                CurrentProcessFunc = pf_Default;
            }
            else
            {
                // ignore: we are in a comment, after all
                CurrentProcessFunc = pf_RecordComment;
            }
        }

        private void pf_RecordGenericExpression(char c)
        {
            if (c == '%')
            {
                FeedGenericExpression();
                CurrentProcessFunc = pf_ExpectEndOfVariableLookup;
            }
            else
            {
                Buffer.Append(c);
            }
        }

        private void pf_RecordVariableLookup(char c)
        {
            if(c == '}')
            {
                FeedVariableLookupParameter();
                CurrentProcessFunc = pf_ExpectEndOfVariableLookup;
            }
            else if (c == '|')
            {
                FeedVariableLookupParameter();
                CurrentProcessFunc = pf_RecordFilterFunc;
            }
            else
            {
                Buffer.Append(c);
            }
        }

        private void pf_RecordFilterFunc(char c)
        {
            if (c == '}')
            {
                FeedVariableFilterFunc();
                CurrentProcessFunc = pf_ExpectEndOfVariableLookup;
            }
            else if (c == '|')
            {
                FeedVariableFilterFunc();
            }
            else
            {
                Buffer.Append(c);
            }
        }

        private string SyntaxError(string message, params object[] args)
        {
            string temp = string.Format(message, args);
            return string.Format("SyntaxError in line {0}, col {1}: {2}", LineNumber, ColumnNumber, temp);
        }

        private void pf_ExpectEndOfVariableLookup(char c)
        {
            if (c == '}')
            {
                CurrentProcessFunc = pf_Default;
            }
            else
            {
                throw new Exception(SyntaxError("Expected }}, got }%c instead", c));
            }
        }
    }
}
