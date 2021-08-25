using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.MapDesigners;
using NextGenMapper.CodeAnalysis.Maps;
using NextGenMapper.CodeGeneration;
using NextGenMapper.Extensions;
using NextGenMapper.PostInitialization;
using NextGenMapper.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NextGenMapper
{
    [Generator]
    public class MapperGenerator : ISourceGenerator
    {
        private System.Diagnostics.Stopwatch _mainTimer = new System.Diagnostics.Stopwatch();
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(i =>
            {
                i.AddSource("MapperExtensions", ExtensionsSource.Source);
                i.AddSource("MapperAttribute", Annotations.MapperAttributeText);
                i.AddSource("PartialAttribute", Annotations.PartialAttributeText);
                i.AddSource("StartMapper", StartMapperSource.StartMapper);
            });
            var stopWatch = new OneOffStopwatch();
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
            var elapsed = stopWatch.Stop();
            System.Console.WriteLine($"prepare syntax: {elapsed}");
            _mainTimer.Start();
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var ts = _mainTimer.Elapsed;
            var fromInitializeToExecute = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            System.Console.WriteLine($"after initialize before execute: {fromInitializeToExecute}");

            var executeTimer = new OneOffStopwatch();
            if (context.SyntaxContextReceiver is not SyntaxReceiver receiver)
                return;

            var planner = new MapPlanner();
            var stopWatch1 = new OneOffStopwatch();
            var mapperClassDeclarationsTypes = new List<(Type From, Type To, string Type, SemanticModel SemanticModel, ClassDeclarationSyntax ClassDeclarationSyntax, List<string> Usings)>();
            foreach (var mapperClassDeclaration in receiver.mapperClassDeclarations)
            {
                if (mapperClassDeclaration.SemanticModel.GetDeclaredSymbol(mapperClassDeclaration.Node).HasAttribute(Annotations.MapperAttributeFullName))
                {
                    var usings = mapperClassDeclaration.Node.GetUsingsAndNamespace();
                    var types = HandleCustomMapperClassBuildTypes(mapperClassDeclaration.SemanticModel, mapperClassDeclaration.Node);
                    mapperClassDeclarationsTypes.AddRange(types);
                }
            }
            var buildTypes = stopWatch1.Stop();

            var stopWatch2 = new OneOffStopwatch();
            foreach (var types in mapperClassDeclarationsTypes)
            {
                var maps = HandleCustomMapperClass(types.SemanticModel, types.From, types.To, types.ClassDeclarationSyntax, types.Type);
                foreach (var map in maps)
                {
                    AddMapToPlanner(map, planner, types.Usings);
                }
            }
            var designMaps = stopWatch2.Stop();

            var stopWatch3 = new OneOffStopwatch();
            var mapInvocationsTypes = new List<(Type From, Type To, string Type)>();
            foreach (var mapMethodInvocation in receiver.mapMethodInvocations)
            {
                if (mapMethodInvocation.SemanticModel.GetSymbol(mapMethodInvocation.Node.Expression) is IMethodSymbol method
                    && method.MethodKind == MethodKind.ReducedExtension
                    && method.ReducedFrom?.ToDisplayString() == StartMapperSource.FunctionFullName
                    && mapMethodInvocation.Node.Expression is MemberAccessExpressionSyntax memberAccess
                    && mapMethodInvocation.SemanticModel.GetSymbol(memberAccess.Expression) is ILocalSymbol invocatingVariable
                    && !planner.IsTypesMapAlreadyPlanned(invocatingVariable.Type, method.ReturnType))
                {
                    var types = MapInvocationBuildTypes(mapMethodInvocation.SemanticModel, invocatingVariable.Type, method.ReturnType);
                    mapInvocationsTypes.AddRange(types);
                } 
            }
            var buildCommonTypes = stopWatch3.Stop();


            var stopWatch4 = new OneOffStopwatch();
            foreach (var mapInvocation in mapInvocationsTypes)
            {
                var maps = MapInvocation(mapInvocation.From, mapInvocation.To);
                foreach (var map in maps)
                {
                    AddMapToPlanner(map, planner, new());
                }
            }
            var designCommonMaps = stopWatch4.Stop();

            var stopWatch5 = new OneOffStopwatch();
            var commonMappers = GenerateCommonMapper(planner);

            var customMappers = GenerateCustomMappers(planner);
            var generateMappers = stopWatch4.Stop();

            System.Console.WriteLine($"build common types: {buildCommonTypes}");
            System.Console.WriteLine($"build custom types: {buildTypes}");
            System.Console.WriteLine($"design common maps: {designCommonMaps}");
            System.Console.WriteLine($"design custom maps: {designMaps}");
            System.Console.WriteLine($"generate mappers: {generateMappers}");

            commonMappers.ForEachIndex((index, mapper) => context.AddSource($"{index}_CommonMapper", SourceText.From(mapper, Encoding.UTF8)));
            customMappers.ForEachIndex((index, mapper) => context.AddSource($"{index}_CustomMapper", SourceText.From(mapper, Encoding.UTF8)));

            var execute = executeTimer.Stop();
            System.Console.WriteLine($"execute time: {execute}");
        }

        private List<(Type From, Type To, string Type)> MapInvocationBuildTypes(SemanticModel semanticModel, ITypeSymbol from, ITypeSymbol to)
        {
            var analyzer = new SyntaxAnalyzer();
            var maps = new List<(Type From, Type To, string Type)>();
            if (from.TypeKind == TypeKind.Enum && to.TypeKind == TypeKind.Enum)
            {
                var fromType = analyzer.BuildEnum(from, semanticModel);
                var toType = analyzer.BuildEnum(to, semanticModel);

                maps.Add((fromType, toType, "Enum"));
            }
            else if (from.IsGenericEnumerable() && to.IsGenericEnumerable())
            {
                var fromType = analyzer.BuildCollection(from);
                var toType = analyzer.BuildCollection(to);

                maps.Add((fromType, toType, "Collection"));
            }
            else if (from.TypeKind == TypeKind.Class && to.TypeKind == TypeKind.Class)
            {
                var fromType = analyzer.BuildType(from);
                var toType = analyzer.BuildType(to);

                maps.Add((fromType, toType, "Class"));
            }

            return maps;
        }

        private List<(Type From, Type To, string Type, SemanticModel SemanticModel, ClassDeclarationSyntax ClassDeclarationSyntax, List<string> Usings)> HandleCustomMapperClassBuildTypes(SemanticModel semanticModel, ClassDeclarationSyntax node)
        {
            var analyzer = new SyntaxAnalyzer();
            var maps = new List<(Type From, Type To, string Type, SemanticModel SemanticModel, ClassDeclarationSyntax ClassDeclarationSyntax, List<string> Usings)>();
            foreach (var method in node.GetMethodsDeclarations().Where(x => x.HasSingleParameterWithType()))
            {
                var usings = node.GetUsingsAndNamespace();
                var (to, from) = semanticModel.GetReturnAndParameterType(method);
                if (semanticModel.GetDeclaredSymbol(method).HasAttribute(Annotations.PartialAttributeName) is var isPartial
                    && isPartial == true
                    && method.GetObjectCreateionExpression() is { ArgumentList: { Arguments: var arguments } }
                    && arguments.Any(x => x.IsDefaultLiteralExpression()))
                {
                    var fromType = analyzer.BuildType(from);
                    var toType = analyzer.BuildType(to);

                    maps.Add((fromType, toType, "ClassPartialConstructorMap", semanticModel, node, usings));
                }
                else if (isPartial)
                {
                    var fromType = analyzer.BuildType(from);
                    var toType = analyzer.BuildType(to);

                    maps.Add((fromType, toType, "ClassPartialMap", semanticModel, node, usings));
                }
                else
                {
                    var fromType = analyzer.BuildType(from);
                    var toType = analyzer.BuildType(to);

                    maps.Add((fromType, toType, "CustomTypeMap", semanticModel, node, usings));
                }
            }

            return maps;
        }

        private List<TypeMap> MapInvocation(Type from, Type to)
        {
            var maps = new List<TypeMap>();
            if (from is Enum fromEnum && to is Enum toEnum)
            {
                var designer = new EnumMapDesigner();
                maps.Add(designer.DesignMapsForPlanner(fromEnum, toEnum));
            }
            else if (from is Collection fromCollection && to is Collection toCollection)
            {
                var designer = new CollectionMapDesigner();
                maps.AddRange(designer.DesignMapsForPlanner(fromCollection, toCollection));
            }
            else
            {
                var designer = new ClassMapDesigner();
                maps.AddRange(designer.DesignMapsForPlanner(from, to));
            }

            return maps;
        }

        private List<TypeMap> HandleCustomMapperClass(SemanticModel semanticModel, Type from, Type to, ClassDeclarationSyntax node, string mapType)
        {
            var analyzer = new SyntaxAnalyzer();
            var maps = new List<TypeMap>();
            foreach (var method in node.GetMethodsDeclarations().Where(x => x.HasSingleParameterWithType()))
            {
                if (mapType == "ClassPartialConstructorMap")
                {
                    var designer = new ClassPartialConstructorMapDesigner(semanticModel);
                    maps.AddRange(designer.DesignMapsForPlanner(from, to, method));
                }
                else if (mapType == "ClassPartialMap")
                {
                    var designer = new ClassPartialMapDesigner(semanticModel);
                    maps.AddRange(designer.DesignMapsForPlanner(from, to, method));
                }
                else
                {
                    var designer = new TypeCustomMapDesigner(semanticModel);
                    maps.Add(designer.DesignMapsForPlanner(from, to, method));
                }
            }

            return maps;
        }

        private List<TypeMap> MapInvocation(SemanticModel semanticModel, MapPlanner planner, ITypeSymbol from, ITypeSymbol to)
        {
            var analyzer = new SyntaxAnalyzer();
            var maps = new List<TypeMap>();
            if (from.TypeKind == TypeKind.Enum && to.TypeKind == TypeKind.Enum)
            {
                var fromType = analyzer.BuildEnum(from, semanticModel);
                var toType = analyzer.BuildEnum(to, semanticModel);

                var designer = new EnumMapDesigner();
                maps.Add(designer.DesignMapsForPlanner(fromType, toType));
            }
            else if (from.IsGenericEnumerable() && to.IsGenericEnumerable())
            {
                var fromType = analyzer.BuildCollection(from);
                var toType = analyzer.BuildCollection(to);

                var designer = new CollectionMapDesigner();
                maps.AddRange(designer.DesignMapsForPlanner(fromType, toType));
            }
            else if (from.TypeKind == TypeKind.Class && to.TypeKind == TypeKind.Class)
            {
                var fromType = analyzer.BuildType(from);
                var toType = analyzer.BuildType(to);

                var designer = new ClassMapDesigner();
                maps.AddRange(designer.DesignMapsForPlanner(fromType, toType));
            }

            return maps;
        }

        private List<TypeMap> HandleCustomMapperClass(SemanticModel semanticModel, ClassDeclarationSyntax node)
        {
            var analyzer = new SyntaxAnalyzer();
            var maps = new List<TypeMap>();
            foreach (var method in node.GetMethodsDeclarations().Where(x => x.HasSingleParameterWithType()))
            {
                var (to, from) = semanticModel.GetReturnAndParameterType(method);
                if (semanticModel.GetDeclaredSymbol(method).HasAttribute(Annotations.PartialAttributeName) is var isPartial
                    && isPartial == true
                    && method.GetObjectCreateionExpression() is { ArgumentList: { Arguments: var arguments } }
                    && arguments.Any(x => x.IsDefaultLiteralExpression()))
                {
                    var fromType = analyzer.BuildType(from);
                    var toType = analyzer.BuildType(to);

                    var designer = new ClassPartialConstructorMapDesigner(semanticModel);
                    maps.AddRange(designer.DesignMapsForPlanner(fromType, toType, method));
                }
                else if (isPartial)
                {
                    var fromType = analyzer.BuildType(from);
                    var toType = analyzer.BuildType(to);

                    var designer = new ClassPartialMapDesigner(semanticModel);
                    maps.AddRange(designer.DesignMapsForPlanner(fromType, toType, method));
                }
                else
                {
                    var fromType = analyzer.BuildType(from);
                    var toType = analyzer.BuildType(to);

                    var designer = new TypeCustomMapDesigner(semanticModel);
                    maps.Add(designer.DesignMapsForPlanner(fromType, toType, method));
                }
            }

            return maps;
        }

        private void AddMapToPlanner(TypeMap map, MapPlanner planner, List<string> usings)
        {
            if (map is ClassPartialConstructorMap or ClassPartialMap or TypeCustomMap)
            {
                planner.AddCustomMap(map, usings);
            }
            else
            {
                planner.AddCommonMap(map);
            }
        }

        private List<string> GenerateCommonMapper(MapPlanner planner)
        {
            var commonMapperGenerator = new CommonMapperGenerator();
            var commonMapGroups = planner.MapGroups.Where(x => x.Priority == MapPriority.Common);
            var commonMappers = commonMapGroups.Select(x => commonMapperGenerator.Generate(x));

            return commonMappers.ToList();
        }

        private List<string> GenerateCustomMappers(MapPlanner planner)
        {
            var customMapperGenerator = new CustomMapperGenerator();
            var customMapGroups = planner.MapGroups.Where(x => x.Priority == MapPriority.Custom);
            var customMappers = customMapGroups.Select(x => customMapperGenerator.Generate(x));

            return customMappers.ToList();
        }
    }
}
