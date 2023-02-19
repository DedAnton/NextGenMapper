using Benchmark.Utils;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.Extensions;

namespace Benchmark.Benchmarks.Experiments;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net60)]
public class ConstructorAnalysys
{
    private SemanticModel semanticModel;
    private ConstructorDeclarationSyntax constructorDeclaration;
    private IConstructorBodyOperation constructorBodyOperation;

    [GlobalSetup]
    public void Setup()
    {
        var source = @"
public namespace Test
{
    public class MyClass
    {
        public MyClass(
            int property1, 
            int property2, 
            int property3, 
            int property4, 
            int property5, 
            int property6, 
            int property7, 
            int property8, 
            int property9, 
            int property10)
        {
            Property1 = property1;
            Property2 = property2;
            Property3 = property3;
            Property4 = property4;
            Property5 = property5;
            Property6 = property6;
            Property7 = property7;
            Property8 = property8;
            Property9 = property9;
            Property10 = property10;
        }

        public int Property1 { get; }
        public int Property2 { get; }
        public int Property3 { get; }
        public int Property4 { get; }
        public int Property5 { get; }
        public int Property6 { get; }
        public int Property7 { get; }
        public int Property8 { get; }
        public int Property9 { get; }
        public int Property10 { get; }
    }
}";
        var compilation = CompilationHelper.CreateCompilation(new[] { source }, "bench");
        constructorDeclaration = compilation.GetTypeByMetadataName("Test.MyClass").Constructors[0].GetFirstDeclaration() as ConstructorDeclarationSyntax;
        semanticModel = compilation.GetSemanticModel(compilation.SyntaxTrees.First());
        constructorBodyOperation = semanticModel.GetOperation(constructorDeclaration) as IConstructorBodyOperation;
    }

    //[Benchmark]
    public List<Assigment> FromControlFlow() => GetAssigmentsFromControlFlow(constructorDeclaration);

    //[Benchmark]
    public IOperation GetOperation() => semanticModel.GetOperation(constructorDeclaration);

    //[Benchmark]
    public ControlFlowGraph CreateControlFlowGraph() => ControlFlowGraph.Create(constructorBodyOperation);

    [Benchmark]
    public List<Assigment> FromSyntaxOnly() => GetAssigmentsFromSyntaxOnly(constructorDeclaration);

    [Benchmark]
    public Span<Assigment> FromSyntaxOnlyOptimized() => GetAssigmentsFromSyntaxOnlyOptimized(constructorDeclaration);

    private List<Assigment> GetAssigmentsFromControlFlow(ConstructorDeclarationSyntax constructorSyntax)
    {
        List<Assigment> assigmentsFromControlFlow = new();
        var operation = semanticModel.GetOperation(constructorSyntax) as IConstructorBodyOperation;
        var controlFlowGraph = ControlFlowGraph.Create(operation);
        foreach (var block in controlFlowGraph.Blocks)
        {
            if (block is BasicBlock
                {
                    Kind: BasicBlockKind.Block,
                    IsReachable: true,
                    BranchValue: null,
                    ConditionKind: ControlFlowConditionKind.None,
                    Operations.Length: 1
                }
                && block.Operations[0] is IExpressionStatementOperation
                {
                    Operation: ISimpleAssignmentOperation
                    {
                        Target: IPropertyReferenceOperation propertyReference,
                        Value: IParameterReferenceOperation parameterReference
                    }
                })
            {
                assigmentsFromControlFlow.Add(new(propertyReference.Property.Name, parameterReference.Parameter.Name));
            }
            else if (block is BasicBlock
            {
                Kind: BasicBlockKind.Block,
                IsReachable: true,
                BranchValue: IIsNullOperation,
                ConditionKind: ControlFlowConditionKind.WhenTrue,
                Operations.Length: 2
            }
                && block.Operations[0] is IFlowCaptureOperation
                {
                    Value: IPropertyReferenceOperation ifNotNullpropertyReference
                }
                && block.Operations[1] is IFlowCaptureOperation
                {
                    Value: IParameterReferenceOperation ifNotNullparameterReference
                })
            {
                assigmentsFromControlFlow.Add(new(ifNotNullpropertyReference.Property.Name, ifNotNullparameterReference.Parameter.Name));
            }
        }

        return assigmentsFromControlFlow;
    }

    private List<Assigment> GetAssigmentsFromSyntaxOnly(ConstructorDeclarationSyntax constructorSyntax)
    {
        if (constructorSyntax.Body is not null)
        {
            List<Assigment> assigments = new(constructorSyntax.Body.Statements.Count);
            for (int i = 0; i < constructorSyntax.Body.Statements.Count; i++)
            {
                if (constructorSyntax.Body.Statements[i] is ExpressionStatementSyntax
                    { Expression: AssignmentExpressionSyntax assignmentExpression })
                {
                    var assigment = GetAssigment(assignmentExpression);
                    if (assigment is not null)
                    {
                        assigments.Add(assigment);
                    }
                }
            }

            return assigments;
        }
        else
        {
            if (constructorSyntax.ExpressionBody is ArrowExpressionClauseSyntax { Expression: AssignmentExpressionSyntax assignmentExpression })
            {
                var assigment = GetAssigment(assignmentExpression);

                if (assigment is not null)
                {
                    return new List<Assigment> { assigment };
                }
            }
        }

        return new();

        Assigment GetAssigment(AssignmentExpressionSyntax assignmentExpression)
        {
            var property = assignmentExpression.Left switch
            {
                IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText,
                MemberAccessExpressionSyntax
                {
                    Expression: ThisExpressionSyntax,
                    Name: IdentifierNameSyntax identifierName
                } => identifierName.Identifier.ValueText,
                _ => null
            };

            var parameter = assignmentExpression.Right switch
            {
                IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText,
                BinaryExpressionSyntax
                {
                    RawKind: (int)SyntaxKind.CoalesceExpression,
                    Left: IdentifierNameSyntax identifierName
                } => identifierName.Identifier.ValueText,
                _ => null
            };

            if (property is null || parameter is null)
            {
                return null;
            }

            return new Assigment(property, parameter);
        }
    }

    private Span<Assigment> GetAssigmentsFromSyntaxOnlyOptimized(ConstructorDeclarationSyntax constructorSyntax)
    {
        if (constructorSyntax.Body != null)
        {
            Span<Assigment> assigments = new Assigment[constructorSyntax.Body.Statements.Count];
            var assigmentsCount = 0;
            for (int i = 0; i < constructorSyntax.Body.Statements.Count; i++)
            {

                if (constructorSyntax.Body.Statements[i] is ExpressionStatementSyntax
                    { Expression: AssignmentExpressionSyntax assignmentExpression })
                {
                    var assigment = GetAssigment(assignmentExpression);
                    if (assigment is not null)
                    {
                        assigments[assigmentsCount] = assigment;
                        assigmentsCount++;
                    }
                }
            }

            return assigments.Slice(0, assigmentsCount);
        }
        else
        {
            if (constructorSyntax.ExpressionBody is ArrowExpressionClauseSyntax { Expression: AssignmentExpressionSyntax assignmentExpression })
            {
                var assigment = GetAssigment(assignmentExpression);

                if (assigment is not null)
                {
                    return new Assigment[] { assigment };
                }
            }
        }

        return new();

        Assigment GetAssigment(AssignmentExpressionSyntax assignmentExpression)
        {
            var property = assignmentExpression.Left switch
            {
                IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText,
                MemberAccessExpressionSyntax
                {
                    Expression: ThisExpressionSyntax,
                    Name: IdentifierNameSyntax identifierName
                } => identifierName.Identifier.ValueText,
                _ => null
            };

            var parameter = assignmentExpression.Right switch
            {
                IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText,
                BinaryExpressionSyntax
                {
                    RawKind: (int)SyntaxKind.CoalesceExpression,
                    Left: IdentifierNameSyntax identifierName
                } => identifierName.Identifier.ValueText,
                _ => null
            };

            if (property is null || parameter is null)
            {
                return null;
            }

            return new Assigment(property, parameter);
        }
    }
}


