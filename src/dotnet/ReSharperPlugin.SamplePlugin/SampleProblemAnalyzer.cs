using JetBrains.ReSharper.Feature.Services.Daemon;
using JetBrains.ReSharper.I18n.Services;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;

namespace ReSharperPlugin.SamplePlugin
{
    // Types mentioned in this attribute are used for performance optimizations
    [ElementProblemAnalyzer(
        typeof (IPropertyDeclaration),
        HighlightingTypes = new [] {typeof (SampleHighlighting)})]
    public class SampleProblemAnalyzer : ElementProblemAnalyzer<IPropertyDeclaration>
    {
        protected override void Run(IPropertyDeclaration element, ElementProblemAnalyzerData data, IHighlightingConsumer consumer)
        {
            // Hint: Avoid LINQ methods to increase performance
            // if (element.Name.Any(char.IsUpper))
            //     consumer.AddHighlighting(new IdentifierHasUpperCaseLetterHighlighting(element));
            if (element?.Parent?.Parent is IClassDeclaration classBody && classBody.DeclaredName.EndsWith("ViewModel"))
            {
                if (element.GetAccessor(AccessorKind.SETTER) == null)
                    return;
                
                if (element.GetAccessor(AccessorKind.SETTER).GetCodeBody().ExpressionBody is IAssignmentExpression == false)
                    return;
                // if (element.GetAccessor(AccessorKind.SETTER) is IAssignmentExpression == false)
                //     return;
                // Hint: Also foreach creates additional enumerator
                // ReSharper disable once ForCanBeConvertedToForeach
                // ReSharper disable once LoopCanBeConvertedToQuery
                for (var i = 0; i < element.NameIdentifier?.Name.Length; i++)
                {
                    if (!char.IsUpper(element.NameIdentifier.Name[i])) continue;
                    
                    consumer.AddHighlighting(new SampleHighlighting(element));
                    return;
                }
            }
        }
    }
}