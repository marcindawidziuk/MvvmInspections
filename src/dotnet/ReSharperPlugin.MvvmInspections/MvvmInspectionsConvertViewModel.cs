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
    [ContextAction(Name = "ConvertMvvmViewModelProperties", Description = "Convert Mvvm ViewModel's properties to observable properties", Group = "C#", Disabled = false, Priority = -3)]
    public class MvvmInspectionsConvertViewModel : ContextActionBase
    {
        private readonly IClassDeclaration _classDeclaration;

        public MvvmInspectionsConvertViewModel(LanguageIndependentContextActionDataProvider dataProvider)
        {
            _classDeclaration = dataProvider.GetSelectedElement<IClassDeclaration>();
        }
        
        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            using (WriteLockCookie.Create())
            {
                if (_classDeclaration == null)
                {
                    return null;
                }
                
                var factory = CSharpElementFactory.GetInstance(_classDeclaration);
                
                foreach (var prop in _classDeclaration.PropertyDeclarations)
                {
                    if (prop.GetAccessRights() != AccessRights.PUBLIC)
                    {
                        return null;
                    }
                    
                    var variableName = GetFieldName(prop.DeclaredName);
                    
                    if (prop.GetAccessorDeclaration(AccessorKind.SETTER).GetCodeBody().IsEmpty == false)
                    {
                        continue;
                    }
                    
                    if (_classDeclaration.FieldDeclarationsEnumerable.Any(x => x.DeclaredName == variableName) == false)
                    {
                        var fieldDeclaration = factory.CreateFieldDeclaration(prop.Type, 
                            variableName);
                        
                        _classDeclaration.AddClassMemberDeclaration(fieldDeclaration);
                    }
                    
                    var getterExpression = factory.CreateExpression("$0;", variableName);
                    
                    var setterExpression = factory.CreateExpression("Set(ref $0, value);", variableName);
                    prop.GetAccessorDeclaration(AccessorKind.SETTER)?.SetBodyExpression(setterExpression);
                    prop.GetAccessorDeclaration(AccessorKind.GETTER)?.SetBodyExpression(getterExpression);
                    
                    prop.GetAccessorDeclaration(AccessorKind.GETTER)?.AddLineBreakBefore(CodeFormatProfile.SPACIOUS);
                    prop.GetAccessorDeclaration(AccessorKind.SETTER)?.AddLineBreakBefore(CodeFormatProfile.SPACIOUS);
                    prop.GetAccessorDeclaration(AccessorKind.GETTER)?.FormatNode();
                    prop.GetAccessorDeclaration(AccessorKind.SETTER)?.FormatNode();
                    prop.FormatNode();
                    prop.Parent?.FormatNode();
                }

                return null;
            }
        }

        public override string Text => "Properties to observable properties";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            if (!(_classDeclaration is IClassDeclaration classDeclaration) || !classDeclaration.DeclaredName.EndsWith("ViewModel"))
                return false;
            
            var hasAutoProperties = _classDeclaration.PropertyDeclarations
                .Any(prop => prop.GetAccessorDeclaration(AccessorKind.SETTER).GetCodeBody().IsEmpty == false);
            if (hasAutoProperties == false)
                return false;
            
            return true;
        }
        
        private static string GetFieldName(string propertyName)
        {
            return "_" + char.ToLower(propertyName[0]) + propertyName.Substring(1);
        }

    }
}