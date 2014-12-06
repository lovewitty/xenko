// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Paradox.Shaders.Parser.Analysis
{
    public class StatementNodeCouple
    {
        public Statement Statement;
        public Node Node;

        public StatementNodeCouple() : this(null, null) { }

        public StatementNodeCouple(Statement statement, Node node)
        {
            Statement = statement;
            Node = node;
        }
    }
}