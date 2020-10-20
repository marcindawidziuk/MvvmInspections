using System;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.ContextActions;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;
using JetBrains.Util;

namespace ReSharperPlugin.MvvmHelper
{
    [ContextAction(Name = "AutoToObservableProperty", Description = "ConvertsToObservableProperty", Group = "C#", Disabled = false,
        Priority = 99)]
    public class SampleConvertAutoPropertyToObservableProperty : ContextActionBase
    {
        private readonly IPropertyDeclaration _propertyDeclaration;

        public SampleConvertAutoPropertyToObservableProperty(LanguageIndependentContextActionDataProvider dataProvider)
        {
            _propertyDeclaration = dataProvider.GetSelectedElement<IPropertyDeclaration>();
        }


        protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
        {
            var factory = CSharpElementFactory.GetInstance(_propertyDeclaration);
            
            var classBody = _propertyDeclaration?.Parent?.Parent as IClassDeclaration;
            if (classBody == null)
                return null;
            
            //TODO: Check if field exists
            var variableName = GetFieldName(_propertyDeclaration.DeclaredName);
            var fieldDeclaration = factory.CreateFieldDeclaration(_propertyDeclaration.Type, 
                variableName);
            
            classBody.AddClassMemberDeclaration(fieldDeclaration);
            
            var getterExpression = factory.CreateExpression("$0;", variableName);
            
            var setterExpression = factory.CreateExpression("Set(ref $0, value);", variableName);
            _propertyDeclaration.GetAccessor(AccessorKind.SETTER)?.SetBodyExpression(setterExpression);
            _propertyDeclaration.GetAccessor(AccessorKind.GETTER)?.SetBodyExpression(getterExpression);

            return null;
        }

        public override string Text => "Convert to Observable property";

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