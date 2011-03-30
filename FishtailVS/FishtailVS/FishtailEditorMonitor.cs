using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System.Diagnostics;
using Microsoft.RestrictedUsage.CSharp.Compiler.IDE;
using Microsoft.RestrictedUsage.CSharp.Compiler;
using Microsoft.RestrictedUsage.CSharp.Syntax;
using Microsoft.RestrictedUsage.CSharp.Core;
using Microsoft.RestrictedUsage.CSharp.Semantics;
using System.Linq;
using System.Threading;

namespace FishtailVS
{
    public class FishtailEditorMonitor
    {
        private ITextDocumentFactoryService textDocumentFactory = null;
        private Timer timer = null;

        public FishtailEditorMonitor(IWpfTextView view, ITextDocumentFactoryService textDocumentFactory)
        {
            this.textDocumentFactory = textDocumentFactory;

            view.Caret.PositionChanged += HandleCaretPositionChanged;
            view.TextBuffer.Changed += HandleTextBufferChanged;

            ITextDocument textDocument = null;
            textDocumentFactory.TryGetTextDocument(view.TextBuffer, out textDocument);
            if (textDocument != null)
            {
                IDECompilerHost host = new IDECompilerHost();
                foreach (Compiler compiler in host.Compilers)
                {
                    foreach (SourceFile sourceFile in compiler.GetCompilation().SourceFiles.Values)
                    {
                        if (sourceFile.FileName.Value == textDocument.FilePath)
                        {
                            ParseTree tree = sourceFile.GetParseTree();
                            int a = 5;
                        }
                    }
                }
            }

        }

        protected void HandleCaretPositionChanged(object sender, CaretPositionChangedEventArgs args)
        {
            Trace.TraceInformation("Caret position changed");
        }

        protected void HandleTextBufferChanged(object sender, TextContentChangedEventArgs args)
        {
            Trace.TraceInformation("Text buffer changed");
            timer = new Timer(HandleTimer, sender, 5000, Timeout.Infinite);
        }

        protected void HandleTimer(object textBuffer)
        {
            ITextDocument textDocument = null;
            textDocumentFactory.TryGetTextDocument((ITextBuffer)textBuffer, out textDocument);
            if (textDocument != null)
            {
                IDECompilerHost host = new IDECompilerHost();
                foreach (Compiler compiler in host.Compilers)
                {
                    foreach (SourceFile sourceFile in compiler.GetCompilation().SourceFiles.Values)
                    {
                        if (sourceFile.FileName.Value == textDocument.FilePath)
                        {
                            ParseTree tree = sourceFile.GetParseTree();
                            MethodDeclarationNode methodNode = FindMethodNode(tree.RootNode, "Main");

                            ExpressionTree expTree = compiler.GetCompilation().CompileMethod(methodNode);
                            
                            int a = 5;
                        }
                    }
                }
            }
        }

        protected MethodDeclarationNode FindMethodNode(ParseTreeNode node, string methodName) 
        {
            if (node.IsMethod()) 
            {
                MethodDeclarationNode methodNode = node as MethodDeclarationNode;
                if (methodNode != null)
                {
                    IdentifierNode nameNode = methodNode.MemberName as IdentifierNode;
                    if (nameNode != null && nameNode.Name.Text.Equals(methodName))
                    {
                        return methodNode;
                    }
                }
            }
            foreach (ParseTreeNode childNode in node.Children)
            {
                MethodDeclarationNode result = FindMethodNode(childNode, methodName);
                if (result != null) 
                {
                    return result;
                }
            }
            return null;
        }
    }
}
