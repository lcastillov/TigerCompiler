using System;
using Antlr.Runtime.Tree;
using Antlr.Runtime;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace TigerCompiler.AST
{
    public class Adaptor : CommonTreeAdaptor
    {
        ICollection<Type> m_types = null;

        public ICollection<Type> Types
        {
            get
            {
                if (m_types == null)
                    m_types = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.Namespace == "TigerCompiler.AST").ToList();
                return m_types;
            }
        }

        public Adaptor() : base() { }

        public override object Create(IToken payload)
        {
            if (payload != null)
            {
                var nodeName = TigerCompiler.Parsing.TigerParser.tokenNames[payload.Type].ToLower();
                nodeName = nodeName.Replace("<", "");
                nodeName = nodeName.Replace(">", "");
                nodeName = nodeName.Replace("_", "");
                nodeName = nodeName + "node";
                var classType = Types.First(x => x.Name.ToLower() == nodeName);
                if (!classType.IsAbstract)
                {
                    return Activator.CreateInstance(classType, payload);
                }
            }
            return base.Create(payload);
        }
    }
}
