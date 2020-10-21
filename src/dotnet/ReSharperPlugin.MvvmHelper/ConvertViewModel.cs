using System;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeStyle;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;
using JetBrains.Util;

namespace ReSharperPlugin.MvvmHelper
{
    [ContextAction(Name = "ConvertViewModel", Description = "ConvertsViewModelPropertiesToObservableProperties", Group = "C#", Disabled = false,
        Priority = -3)]
    public class SampleConvertViewModel : ContextActionBase
    {
        private readonly IPropertyDeclaration _propertyDeclaration;

        public SampleConvertViewModel(LanguageIndependentContextActionDataProvider dataProvider)
        {
            _propertyDeclaration = dataProvider.GetSelectedElement<IPropertyDeclaration>();
        }


        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var factory = CSharpElementFactory.GetInstance(_propertyDeclaration);
            
            var classBody = _propertyDeclaration?.Parent?.Parent as IClassDeclaration;
            if (classBody == null)
            {
                return null;
            }
            
            foreach (var prop in classBody.PropertyDeclarations)
            {
                var variableName = GetFieldName(prop.DeclaredName);
                
                if (prop.GetAccessor(AccessorKind.SETTER).GetCodeBody().IsEmpty == false)
                {
                    continue;
                }
                
                //TODO: Check if field exists
                if (classBody.FieldDeclarationsEnumerable.Any(x => x.DeclaredName == variableName) == false)
                {
                    var fieldDeclaration = factory.CreateFieldDeclaration(prop.Type, 
                        variableName);
                    
                    classBody.AddClassMemberDeclaration(fieldDeclaration);
                }
                
                var getterExpression = factory.CreateExpression("$0;", variableName);
                
                var setterExpression = factory.CreateExpression("Set(ref $0, value);", variableName);
                prop.GetAccessor(AccessorKind.SETTER)?.SetBodyExpression(setterExpression);
                prop.GetAccessor(AccessorKind.GETTER)?.SetBodyExpression(getterExpression);
                prop.FormatNode();
            }

            return null;
            
        }

        public override string Text => "Convert auto properties to Observable properties";

        public override bool IsAvailable(IUserDataHolder cache)
        {
            if (!(_propertyDeclaration?.Parent?.Parent is IClassDeclaration classBody) || !classBody.DeclaredName.EndsWith("ViewModel"))
                return false;
            
            var element = _propertyDeclaration;
            if (element.GetAccessor(AccessorKind.SETTER) == null)
                return false;

            if (element.GetAccessor(AccessorKind.SETTER).GetCodeBody().IsEmpty == false)
                return false;
            
            if (element.GetAccessor(AccessorKind.GETTER).GetCodeBody().IsEmpty == false)
                return false;
            
            return true;
        }
        
        private static string GetFieldName(string propertyName)
        {
            return "_" + char.ToLower(propertyName[0]) + propertyName.Substring(1);
        }

    }
}