using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;
using System.CodeDom;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Ast;

namespace FishtailVSTests
{
    [TestClass]
    public class TestCSharpASTs
    {
        private int indentationLevel = 0;

        [TestMethod]
        public void TestMethod1()
        {
            using (StreamReader reader = File.OpenText("TestClass1.cs"))
            {
                using (IParser parser = ParserFactory.CreateParser(SupportedLanguage.CSharp, reader))
                {
                    parser.Parse();
                    DisplayNode(parser.CompilationUnit);
                }
            }
        }

        public void DisplayNode(INode node)
        {
            DisplayWithIndent(node.GetType().Name);
            IncreaseIndent();
            if (node.Children.Count > 0)
            {
                DisplayCollectionNode("Children", node.Children);
            }
            DecreaseIndent();
        }

        private void DisplayCollectionNode(string name, List<INode> children)
        {
            string text = name + " (collection with " + children.Count + " elements)";
            DisplayWithIndent(text);
            IncreaseIndent();
            foreach (INode node in children) 
            {
                DisplayNode(node);
            }
            DecreaseIndent();
        }

        private void IncreaseIndent()
        {
            indentationLevel += 2;
        }

        private void DecreaseIndent()
        {
            indentationLevel -= 2;
        }

        private void DisplayWithIndent(String text)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < indentationLevel; i++)
            {
                builder.Append(' ');
            }
            builder.Append(text);
            Console.WriteLine(builder.ToString());
        }
    }
}
