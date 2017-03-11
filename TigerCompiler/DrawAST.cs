using Antlr.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TigerCompiler
{
    public static class DrawAST
    {
        private static StreamWriter SW;
        private static int id;

        public static string StringRightPadding(string filename, int padd)
        {
            while (filename.Length < padd)
                filename = filename + " ";
            return filename;
        }

        private static void PrintTree(CommonTree node)
        {
            var currentID = id++;
            var nodeLabel = node.GetType().Name; //node.ToString().Replace("\"", "");
            SW.WriteLine(string.Format("\"{0}_{1}\" [label = \"{2}\", shape=\"rectangle\"];", nodeLabel, currentID, nodeLabel));
            if (node.Children != null)
            {
                foreach (var u in node.Children)
                {
                    var childLabel = u.GetType().Name; //u.ToString().Replace("\"", "");
                    SW.WriteLine(string.Format("    \"{0}_{1}\" -> \"{2}_{3}\";", nodeLabel, currentID, childLabel, id));
                    PrintTree((CommonTree)u);
                }
            }
        }

        public static void PrintTree(CommonTree root, string filename = "ast.txt")
        {
            SW = new StreamWriter(filename);
            SW.WriteLine("digraph G {");
            PrintTree(root);
            SW.WriteLine("}");
            SW.Close();
        }
    }
}
