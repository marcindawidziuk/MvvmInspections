    using System;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeStyle;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using JetBrains.Util;

namespace ReSharperPlugin.MvvmInspections
{
    [ContextAction(Name = "ConvertMvvmAutoProperty", Description = "Convert auto property to observable properties", Group = "C#", Disabled = false, Priority = 99)]
    public class MvvmInspectionsConvertAutoPropertyToObservableProperty : ContextActionBase
    {
        private readonly IPropertyDeclaration _propertyDeclaration;

        public MvvmInspectionsConvertAutoPropertyToObservableProperty(LanguageIndependentContextActionDataProvider dataProvider)
        {
            _propertyDeclaration = dataProvider.GetSelectedElement<IPropertyDeclaration>();
        }


        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            using (WriteLockCookie.Create())
            {
                var factory = CSharpElementFactory.GetInstance(_propertyDeclaration);
                
                var classBody = _propertyDeclaration?.Parent?.Parent as IClassDeclaration;
                if (classBody == null)
                    return null;
                
                //TODO: Check if field already exists
                var variableName = GetFieldName(_propertyDeclaration.DeclaredName);
                if (classBody.FieldDeclarationsEnumerable.Any(x => x.DeclaredName == variableName) == false)
                {
                    var fieldDeclaration = factory.CreateFieldDeclaration(_propertyDeclaration.Type, 
                        variableName);
                    
                    classBody.AddClassMemberDeclaration(fieldDeclaration);
                }
                
                var getterExpression = factory.CreateExpression("$0;\n", variableName);
                
                var setterExpression = factory.CreateExpression("Set(ref $0, value);\n", variableName);
                _propertyDeclaration.GetAccessorDeclaration(AccessorKind.SETTER)?.SetBodyExpression(setterExpression);
                _propertyDeclaration.GetAccessorDeclaration(AccessorKind.GETTER)?.SetBodyExpression(getterExpression);
                _propertyDeclaration.GetAccessorDeclaration(AccessorKind.GETTER)?.AddLineBreakBefore(CodeFormatProfile.SPACIOUS);
                _propertyDeclaration.GetAccessorDeclaration(AccessorKind.SETTER)?.AddLineBreakBefore(CodeFormatProfile.SPACIOUS);
                _propertyDeclaration.GetAccessorDeclaration(AccessorKind.GETTER)?.FormatNode();
                _propertyDeclaration.GetAccessorDeclaration(AccessorKind.SETTER)?.FormatNode();
                _propertyDeclaration.FormatNode();
                _propertyDeclaration?.Parent?.FormatNode();

                return null;
            }
        }

        public override string Text => "To observable property";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            if (!(_propertyDeclaration?.Parent?.Parent is IClassDeclaration classBody) || !classBody.DeclaredName.EndsWith("ViewModel"))
                return false;
            var element = _propertyDeclaration;
            if (element.GetAccessorDeclaration(AccessorKind.SETTER) == null)
                return false;
            if (element.GetAccessorDeclaration(AccessorKind.GETTER) == null)
                return false;

            if (element.GetAccessorDeclaration(AccessorKind.SETTER).GetCodeBody().IsEmpty == false)
                return false;
            
            if (element.GetAccessorDeclaration(AccessorKind.GETTER).GetCodeBody().IsEmpty == false)
                return false;
            
            return true;
        }
        
        private static string GetFieldName(string propertyName)
        {
            return "_" + char.ToLower(propertyName[0]) + propertyName.Substring(1);
        }

    }
}