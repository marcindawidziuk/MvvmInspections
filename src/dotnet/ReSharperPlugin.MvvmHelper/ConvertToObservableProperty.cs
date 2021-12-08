using System;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Feature.Services.Refactorings.Specific.Rename;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using JetBrains.Util;

namespace ReSharperPlugin.SamplePlugin
{
    [ContextAction(Name = "ToObservableProperty", Description = "ConvertsToObservableProperty", Group = "C#", Disabled = false,
        Priority = 99)]
    public class SampleConvertToObservableProperty : ContextActionBase
    {
        private readonly IPropertyDeclaration _propertyDeclaration;

        public SampleConvertToObservableProperty(LanguageIndependentContextActionDataProvider dataProvider)
        {
            _propertyDeclaration = dataProvider.GetSelectedElement<IPropertyDeclaration>();
        }


        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            using (WriteLockCookie.Create())
            {
                var setterDeclaration = _propertyDeclaration.GetAccessor(AccessorKind.SETTER).GetCodeBody().ExpressionBody;
                var factory = CSharpElementFactory.GetInstance(_propertyDeclaration);

                if (setterDeclaration?.FirstChild == null)
                {
                    return null;
                }
                
                var backingPropertyName = setterDeclaration.FirstChild.GetText();
                var newExpression = factory.CreateExpression("Set(ref $0, value)", backingPropertyName);
                ModificationUtil.ReplaceChild(setterDeclaration, newExpression);

                return null;
            }
        }

        public override string Text => "Convert to Observable property";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            if (!(_propertyDeclaration?.Parent?.Parent is IClassDeclaration classBody) || !classBody.DeclaredName.EndsWith("ViewModel"))
                return false;
            var element = _propertyDeclaration;
            if (element.GetAccessor(AccessorKind.SETTER) == null)
                return false;

            if (element.GetAccessor(AccessorKind.SETTER).GetCodeBody().ExpressionBody is IAssignmentExpression == false)
                return false;

            return true;

        }
    }
}