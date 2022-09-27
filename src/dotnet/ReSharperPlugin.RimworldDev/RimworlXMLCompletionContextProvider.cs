using System.Linq;
using JetBrains.Annotations;
using JetBrains.Application.Progress;
using JetBrains.DocumentModel;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Impl;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.CSharp.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Feature.Services.CSharp.CodeCompletion.Settings;
using JetBrains.ReSharper.Feature.Services.Navigation.Requests;
using JetBrains.ReSharper.Features.Intellisense.CodeCompletion.Xml;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Resolve;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Xml.Tree;
using ReSharperPlugin.RimworldDev;

namespace ReSharperPlugin.RimworldDev
{
    public class RimworldXmlCodeCompletionContext : SpecificCodeCompletionContext
    {
        public IXmlFile File;
        
        [CanBeNull]
        public ITreeNode TreeNode;
        
        public TextLookupRanges Ranges { get; private set; }

        public RimworldXmlCodeCompletionContext(
            CodeCompletionContext context,
            IXmlFile file,
            ITreeNode treeNode,
            TextLookupRanges ranges)
            : base(context)
        {
            File = file;
            TreeNode = treeNode;
            Ranges = ranges;
        }

        public override string ContextId => nameof (XmlCodeCompletionContext);
    }

    [IntellisensePart]
    public class RimworlXMLCompletionContextProvider: XmlCodeCompletionContextProvider
    {
        public override bool IsApplicable(CodeCompletionContext context)
        {
            return true;
        }

        public override ISpecificCodeCompletionContext GetCompletionContext(CodeCompletionContext context)
        {
            if (context.File is not IXmlFile xmlFile) return null;
            
            XmlReparsedCodeCompletionContext unterminatedContext = this.CreateUnterminatedContext(xmlFile, context);
            if (unterminatedContext == null)
                return (ISpecificCodeCompletionContext) null;
            unterminatedContext.Init();
            IReference reference = unterminatedContext.Reference;
            ITreeNode treeNode = unterminatedContext.TreeNode;
            if (treeNode == null)
                return (ISpecificCodeCompletionContext) null;
            TreeTextRange treeRange = reference == null ? XmlCodeCompletionContextProvider.GetElementRange(treeNode) : reference.GetTreeTextRange();
            DocumentRange documentRange = unterminatedContext.ToDocumentRange(treeRange);
            if (!documentRange.IsValid())
                return (ISpecificCodeCompletionContext) null;
            ref DocumentRange local1 = ref documentRange;
            DocumentOffset caretDocumentOffset = context.EffectiveCaretDocumentOffset;
            ref DocumentOffset local2 = ref caretDocumentOffset;
            if (!local1.Contains(in local2))
                return (ISpecificCodeCompletionContext) null;

            TextLookupRanges textLookupRanges = CodeCompletionContextProviderBase.GetTextLookupRanges(context, documentRange);
            return new RimworldXmlCodeCompletionContext(context, xmlFile,
                xmlFile.FindNodeAt(unterminatedContext.Range), textLookupRanges);
        }
    }
}