using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis.Models;
using NextGenMapper.Extensions;
using NextGenMapper.PostInitialization;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NextGenMapper.CodeAnalysis
{
    public class SyntaxAnalyzer
    {
        private readonly Dictionary<string, ITypeSymbol> _usedTypes = new();
        public (Type from, Type to)? AnalyzeMapMethodInvocation(MapMethodInvocation mapMethodInvocation)
        {

            if (mapMethodInvocation.SemanticModel.GetSymbol(mapMethodInvocation.Node.Expression) is IMethodSymbol method
                && method.MethodKind == MethodKind.ReducedExtension
                && method.ReducedFrom?.ToDisplayString() == StartMapperSource.FunctionFullName
                && mapMethodInvocation.Node.Expression is MemberAccessExpressionSyntax memberAccess
                && mapMethodInvocation.SemanticModel.GetSymbol(memberAccess.Expression) is ILocalSymbol invocatingVariable)
            {
                var (from, to) = (invocatingVariable.Type, method.ReturnType) switch
                {
                    ({ TypeKind: TypeKind.Enum }, { TypeKind: TypeKind.Enum }) 
                        => (BuildEnum(invocatingVariable.Type, mapMethodInvocation.SemanticModel), BuildEnum(method.ReturnType, mapMethodInvocation.SemanticModel)),
                    ({ }, { }) when method.ReturnType.IsGenericEnumerable() && invocatingVariable.Type.IsGenericEnumerable() 
                        => (BuildCollection(invocatingVariable.Type), BuildCollection(method.ReturnType)),
                    ({ TypeKind: TypeKind.Class }, { TypeKind: TypeKind.Class }) => (BuildType(invocatingVariable.Type), BuildType(method.ReturnType)),
                    _ => default
                };

                return (from, to);
            }

            return default;
        }

        public MapMethod? AnalyzeMapperClassDeclarations(MapperClassDeclaration mapperClassDeclaration)
        {
            if (mapperClassDeclaration.SemanticModel.GetDeclaredSymbol(mapperClassDeclaration.Node) is INamedTypeSymbol mapperClassSymbol
                && mapperClassSymbol.HasAttribute(Annotations.MapperAttributeFullName))
            {
                foreach (var methodDeclarationSyntax in mapperClassDeclaration.Node.GetMethodsDeclarations())
                {
                    if (methodDeclarationSyntax.ParameterList.Parameters.Count == 1
                        && methodDeclarationSyntax.ReturnType != null
                        && mapperClassDeclaration.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax) is IMethodSymbol mapMethodSymbol)
                    {
                        var isPartial = mapMethodSymbol.HasAttribute(Annotations.PartialAttributeFullName);
                        if (!isPartial)
                        {
                            return new CustomMapMethod(methodDeclarationSyntax, BuildParameter(mapMethodSymbol.Parameters.First()), BuildType(mapMethodSymbol.ReturnType));
                        }
                        else
                        {
                            var objectCreationExpressionSyntax = (methodDeclarationSyntax.ExpressionBody, methodDeclarationSyntax.Body) switch
                            {
                                ({ }, _) => methodDeclarationSyntax.ExpressionBody.Expression as ObjectCreationExpressionSyntax,
                                (_, { Statements: var statements }) when statements.OfType<ReturnStatementSyntax>().Count() == 1 
                                    => statements.OfType<ReturnStatementSyntax>().First().Expression as ObjectCreationExpressionSyntax,
                                _ => null
                            };
                            if (objectCreationExpressionSyntax == null)
                            {
                                //TODO: Diagnostics (custom map method must contains single return statement with object creation expression)
                                //body: return new Type();
                                //expressionBody: => new Type();
                                continue;
                            }

                            var objectCreationSymbol = mapperClassDeclaration.SemanticModel.GetSymbol(objectCreationExpressionSyntax) as IMethodSymbol;
                            if (objectCreationSymbol == null)
                            {
                                continue;
                                //TODO: Diagnostics
                            }
                            var constructor = BuildConstructor(objectCreationSymbol, mapMethodSymbol.ReturnType);
                            var initializerExpressions = objectCreationExpressionSyntax.Initializer?.Expressions
                                .OfType<AssignmentExpressionSyntax>().ToImmutableArray() ?? new();
                            var initializerPropertiesNames = initializerExpressions.Select(x => x.Left)
                                .OfType<IdentifierNameSyntax>().Select(x => x.Identifier.ValueText).ToImmutableArray();
                            if (initializerPropertiesNames.Length != initializerExpressions.Length)
                            {
                                //objectCreationExpressionSyntax.Initializer?.Expressions.Where(x => x is not AssignmentExpressionSyntax).Select(x => x.Kind().ToString());
                                //TODO: Diagnostics
                            }
                            var objectCreationExpression = new ObjectCreationExpression(constructor, initializerPropertiesNames);

                            var argumentsSyntax = objectCreationExpressionSyntax.ArgumentList?.Arguments.ToImmutableArray() ?? ImmutableArray.Create<ArgumentSyntax>();
                            var hasDefaultArguments = argumentsSyntax.Any(x => x.IsDefaultLiteralExpression());
                            if (hasDefaultArguments)
                            {
                                return new PartialConstructorMapMethod(objectCreationExpression, argumentsSyntax, initializerExpressions, BuildParameter(mapMethodSymbol.Parameters.First()), BuildType(mapMethodSymbol.ReturnType));
                            }
                            else
                            {
                                var statements = methodDeclarationSyntax.Body != null
                                                                    ? methodDeclarationSyntax.Body.Statements.ToImmutableArray()
                                                                    : new() { SyntaxFactory.ReturnStatement(objectCreationExpressionSyntax).NormalizeWhitespace() };

                                return new PartialMapMethod(objectCreationExpression, statements, BuildParameter(mapMethodSymbol.Parameters.First()), BuildType(mapMethodSymbol.ReturnType));
                            }
                        }
                    }
                }
            }

            return null;
        }

        private Type BuildType(ITypeSymbol typeSymbol)
        {
            var propertiesSymbols = typeSymbol.GetPublicProperties().Where(x => x.CanBeReferencedByName);
            var isUsed = _usedTypes.ContainsKey(typeSymbol.ToDisplayString());
            if (typeSymbol.IsPrimitive() || propertiesSymbols.Count() == 0 || isUsed)
            {
                return new Type(typeSymbol.ToDisplayString(), true, typeSymbol.SpecialType, ImmutableArray.Create<Property>(), ImmutableArray.Create<Constructor>());
            }
            _usedTypes.Add(typeSymbol.ToDisplayString(), typeSymbol);

            var properties = propertiesSymbols.Select(x => Buildproperty(x)).ToImmutableArray();
            if (properties.Length == 0)
            {
                return new Type(typeSymbol.ToDisplayString(), true, typeSymbol.SpecialType, ImmutableArray.Create<Property>(), ImmutableArray.Create<Constructor>());
                //TODO: Diagnostics (type {type} does not have any public properties)
            }
            var constructors = typeSymbol.GetPublicConstructors().Select(x => BuildConstructor(x, typeSymbol)).ToImmutableArray();
            if (constructors.Length == 0)
            {
                return new Type(typeSymbol.ToDisplayString(), true, typeSymbol.SpecialType, ImmutableArray.Create<Property>(), ImmutableArray.Create<Constructor>());
                //TODO: Diagnostics type {type} does not have any public constructors
            }

            var type = new Type(typeSymbol.ToDisplayString(), typeSymbol.IsPrimitive(), typeSymbol.SpecialType, properties, constructors);
            return type;
        }

        private Property Buildproperty(IPropertySymbol propertySymbol) => new Property(propertySymbol.Name, BuildType(propertySymbol.Type), propertySymbol.IsReadOnly);

        private Constructor BuildConstructor(IMethodSymbol constructorSymbol, ITypeSymbol constructorType)
        {
            var parameters = constructorSymbol.Parameters.Select(x => BuildParameter(x)).ToImmutableArray();

            return new Constructor(BuildType(constructorType), parameters);
        }

        private Parameter BuildParameter(IParameterSymbol parameterSymbol) => new Parameter(parameterSymbol.Name, BuildType(parameterSymbol.Type));

        private Enum BuildEnum(ITypeSymbol enumSymbol, SemanticModel semanticModel)
        {
            //TODO: refactor
            var enumDeclaration = enumSymbol.GetFirstDeclaration() as EnumDeclarationSyntax;
            var fields = enumDeclaration!.Members
                .Select(x => semanticModel.GetDeclaredSymbol(x))
                .OfType<IFieldSymbol>()
                .Select(x => new EnumField(x.Name, BuildType(enumSymbol), x.HasConstantValue, x.ConstantValue?.UnboxToLong()))
                .ToImmutableArray();

            return new Enum(fields, enumSymbol.Name);
        }

        private Collection BuildCollection(ITypeSymbol collectionSymbol)
        {
            var elementType = collectionSymbol switch
            {
                IArrayTypeSymbol array => array.ElementType,
                INamedTypeSymbol list when list.IsGenericType && list.Arity == 1 => list.TypeArguments.Single(),
                _ => throw new System.ArgumentOutOfRangeException($"Can`t get type of elements in collection {collectionSymbol}")
            };
            var isArray = collectionSymbol.SpecialType == SpecialType.System_Array;

            return new Collection(BuildType(elementType), isArray, collectionSymbol.Name, collectionSymbol.SpecialType);
        }
    }
}
