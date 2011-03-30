using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.Diagnostics;
using Microsoft.VisualStudio.Text;

namespace FishtailVS
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class FishtailVSFactory : IWpfTextViewCreationListener
    {
        [Import]
        private ITextDocumentFactoryService textDocumentFactory = null;

        [Name("FishtailVS")]
        [TextViewRole(PredefinedTextViewRoles.Document)]
        public AdornmentLayerDefinition editorAdornmentLayer = null;

        /// <summary>
        /// Instantiates a FishtailVS manager when a textView is created.
        /// </summary>
        public void TextViewCreated(IWpfTextView textView)
        {
            Trace.TraceInformation("New TextView created");
            new FishtailEditorMonitor(textView, textDocumentFactory);
        }
    }
}
