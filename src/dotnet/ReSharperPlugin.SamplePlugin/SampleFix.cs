using System;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CSharp.PostfixTemplates.Templates;
using JetBrains.ReSharper.Feature.Services.Intentions.CreateDeclaration;
using JetBrains.ReSharper.Feature.Services.Intentions.Impl.DeclarationBuilders;
using JetBrains.ReSharper.Feature.Services.QuickFixes;
using JetBrains.ReSharper.Feature.Services.Refactorings.Specific.Rename;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.CSharp.Util;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Refactorings.Rename;
using JetBrains.TextControl;
using JetBrains.Util;

namespace ReSharperPlugin.SamplePlugin
{
    public sealed class SampleFix : QuickFixBase
    {
        private readonly ICSharpDeclaration _declaration;

        public SampleFix(ICSharpDeclaration declaration)
        {
            _declaration = declaration;
        }

        public override string Text => "Write all lower-case";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            return _declaration.IsValid();
            // return _declaration.IsValid() && _declaration is IMethodDeclaration;
        }

        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {

            // AssignmentExpressionTemplate
            // IAssignmentExpression
                
                // backing field
// propertyDeclaration.AccessorDeclarations.First(x => x.Kind == AccessorKind.SETTER).GetCodeBody().ExpressionBody.FirstChild
            var propertyDeclaration = (IPropertyDeclaration) _declaration;
            // var backingField = propertyDeclaration.AccessorDeclarations.First(x => x.Kind == AccessorKind.SETTER).GetCodeBody().ExpressionBody;
            var setterDeclaration = propertyDeclaration.GetAccessor(AccessorKind.SETTER).GetCodeBody().ExpressionBody;
            // var b = propertyDeclaration.GetAccessor(AccessorKind.SETTER)?.Body is IAssignmentExpression;
            // _declaration.SetCodeBody(CSharpCodeBody.Empty);
            // is IAssignmentExpression
            var factory = CSharpElementFactory.GetInstance(_declaration);
            var backingPropertyName = setterDeclaration.FirstChild.GetText();
            var newExpression = factory.CreateExpression("Set(ref $0, value)", backingPropertyName);
            ModificationUtil.ReplaceChild(setterDeclaration, newExpression);
            
            // CSharpElementFactory factory,
            // return (IExpressionStatement) factory.CreateStatement("target = $0;", (object) expression);
            
            // var setter = propertyDeclaration.AccessorDeclarations.First(x => x.Kind == AccessorKind.SETTER);
            // setter.Body
            
            // PropertyDeclarationBuilder.CreateProperty(new CreatePropertyDeclarationContext())
            // PropertyDeclarationBuilder
            // propertyDeclaration.SetBodyExpression(ExpressionFactory)
            
            // methodDeclaration.Tr
            // RenameRefactoringService
            // // methodDeclaration.SetArrowClause(new ArrowCl)
            //
            // // new CSharpCodeBody()
            // //
            // // methodDeclaration.SetBodyExpression()
            // // methodDeclaration.SetBodyExpression()
            // // This is greatly simplified, since we're not updating any references
            // // You will probably see a small indicator in the lower-right
            // // that tells you about an exception being thrown.
            // methodDeclaration.SetName(methodDeclaration.DeclaredName.ToLower());
            //
            // return null;
            // return textControl =>
            // {
            //     textControl
            // }
            return null;
        }
    }
}