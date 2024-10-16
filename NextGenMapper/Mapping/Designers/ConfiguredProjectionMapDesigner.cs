﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NextGenMapper.CodeAnalysis;
using NextGenMapper.CodeAnalysis.Targets.MapTargets;
using NextGenMapper.Errors;
using NextGenMapper.Extensions;
using NextGenMapper.Mapping.Maps;
using NextGenMapper.Mapping.Maps.Models;
using NextGenMapper.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace NextGenMapper.Mapping.Designers;

internal static class ConfiguredProjectionMapDesigner
{
    public static ImmutableArray<Map> DesingConfiguredProjectionMap(ConfiguredProjectionTarget target, CancellationToken cancellationToken)
    {
        try
        {
            return DesingConfiguredProjectionMapInternal(target, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var diagnostic = Diagnostics.MapperInternalError(target.Location, ex);
            return ImmutableArray.Create(Map.Error(target.Source, target.Destination, diagnostic));
        }
    }

    private static ImmutableArray<Map> DesingConfiguredProjectionMapInternal(ConfiguredProjectionTarget target, CancellationToken cancellationToken)
    {
        var (source, destination, arguments, isCompleteMethod, location, semanticModel) = target;
        cancellationToken.ThrowIfCancellationRequested();

        var userArgumentsHashSet = new HashSet<string>(StringComparer.InvariantCulture);
        foreach (var argument in target.Arguments)
        {
            var argumentName = argument.NameColon?.Name.Identifier.ValueText;
            if (argumentName is not null)
            {
                userArgumentsHashSet.Add(argumentName);
            }
        }

        var sourceProperties = source.GetPublicReadablePropertiesDictionary();

        var maps = new ValueListBuilder<Map>();
        var (constructor, _, error) = ConstructorFinder.GetPublicDefaultConstructor(destination);
        if (error is MultipleInitializationError multipleInitializationError)
        {
            var diagnostic = Diagnostics.MultipleInitializationError(location, source, destination, multipleInitializationError.ParameterName, multipleInitializationError.InitializedPropertiesString);
            maps.Append(Map.PotentialError(source, destination, diagnostic));

            return maps.ToImmutableArray();
        }
        if (constructor is null)
        {
            var diagnostic = Diagnostics.DefaultConstructorNotFoundError(location, source, destination);
            maps.Append(Map.Error(source, destination, diagnostic));

            return maps.ToImmutableArray();
        }

        cancellationToken.ThrowIfCancellationRequested();
        var destinationProperties = destination.GetPublicWritableProperties();
        if (destinationProperties.Length == 0)
        {
            var diagnostic = Diagnostics.SuitablePropertyNotFoundInDestination(location, source, destination);
            maps.Append(Map.Error(source, destination, diagnostic));

            return maps.ToImmutableArray();
        }

        var initializerProperties = new ValueListBuilder<PropertyMap>(destinationProperties.Length);
        var configuredMapArguments = new ValueListBuilder<NameTypePair>(arguments.Count);
        var isUsedLambdaArguments = false;
        var isUsedRegularArguments = false;
        foreach (var destinationProperty in destinationProperties)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var destinationPropertyType = destinationProperty.Type.ToNotNullableString();
            if (userArgumentsHashSet.Contains(destinationProperty.Name))
            {
                configuredMapArguments.Append(new NameTypePair(destinationProperty.Name, destinationPropertyType));

                if (arguments
                    .First(x => x?.NameColon?.Name.Identifier.ValueText == destinationProperty.Name)
                    .Expression is SimpleLambdaExpressionSyntax lambdaArgument)
                {
                    isUsedLambdaArguments = true;
                    var argumentBody = lambdaArgument.Body;
                    if (lambdaArgument.Parameter.Identifier.Text != "x")
                    {
                        var rewriter = new ParameterRewriter(lambdaArgument.Parameter.Identifier.Text, "test");
                        argumentBody = (CSharpSyntaxNode)rewriter.Visit(argumentBody);
                    }
                    var propertyMap = CreateUserProvidedProeprtyMap(destinationProperty.Name, argumentBody.ToString(), destinationPropertyType);
                    initializerProperties.Append(propertyMap);
                }
                else
                {
                    isUsedRegularArguments = true;
                    var propertyMap = CreateUserProvidedProeprtyMap(destinationProperty.Name, destinationPropertyType);
                    initializerProperties.Append(propertyMap);
                }
            }
            else
            {
                if (sourceProperties.TryGetValue(destinationProperty.Name, out var sourceProperty))
                {
                    if (SourceCodeAnalyzer.IsTypesAreEquals(sourceProperty.Type, destinationProperty.Type))
                    {
                        var propertyMap = new PropertyMap(
                            sourceProperty.Name,
                            destinationProperty.Name,
                            sourceProperty.Type.ToNotNullableString(),
                            destinationPropertyType,
                            sourceProperty.Type.NullableAnnotation == NullableAnnotation.Annotated,
                            destinationProperty.Type.NullableAnnotation == NullableAnnotation.Annotated,
                            isTypesEquals: true,
                            hasImplicitConversion: true);

                        if (propertyMap.IsSourceNullable && !propertyMap.IsDestinationNullable)
                        {
                            var diagnostic = Diagnostics.PossiblePropertyNullReference(location, source, sourceProperty.Name, sourceProperty.Type, destination, destinationProperty.Name, destinationProperty.Type);
                            maps.Append(Map.Error(source, destination, diagnostic));

                            return maps.ToImmutableArray();
                        }

                        initializerProperties.Append(propertyMap);
                    }
                    else
                    {
                        var diagnostic = Diagnostics.PropertiesTypesMustBeEquals(location, sourceProperty, destinationProperty);
                        maps.Append(Map.Error(source, destination, diagnostic));

                        return maps.ToImmutableArray();
                    }
                }
            }
        }

        if (initializerProperties.Length == 0)
        {
            var diagnostic = Diagnostics.NoPropertyMatches(location, source, destination);
            maps.Append(Map.Error(source, destination, diagnostic));

            return maps.ToImmutableArray();
        }

        if (isUsedRegularArguments && isUsedLambdaArguments)
        {
            var diagnostic = Diagnostics.RegularAndLambdaArgumentsInProjection(location, source, destination);
            maps.Append(Map.Error(source, destination, diagnostic));

            return maps.ToImmutableArray();
        }

        var configuredMapArgumentsArray = configuredMapArguments.ToImmutableArray();
        if (configuredMapArguments.Length != arguments.Count)
        {
            isCompleteMethod = false;
            configuredMapArguments = ValueListBuilder<NameTypePair>.Empty;

            var mappedArgumentsNames = configuredMapArgumentsArray.Select(x => x.Name).ToImmutableHashSet();
            var notMappedArgumentsNames = mappedArgumentsNames.SymmetricExcept(userArgumentsHashSet);
            foreach (var argument in notMappedArgumentsNames)
            {
                var diagnostic = Diagnostics.PropertyNotFoundForCoonfiguredProjectionArgument(location, source, destination, argument);
                maps.Append(Map.Error(source, destination, diagnostic));
            }
        }

        var mockMethod = DesignClassMapMockMethod(
            source,
            destination,
            destinationProperties,
            configuredMapArguments.AsSpan(),
            isCompleteMethod);

        var map = Map.ConfiguredProjection(
            source.ToNotNullableString(),
            destination.ToNotNullableString(),
            initializerProperties.ToImmutableArray(),
            configuredMapArguments.ToImmutableArray(),
            isUsedLambdaArguments,
            mockMethod,
            isCompleteMethod,
            location);
        maps.Append(map);

        return maps.ToImmutableArray();
    }

    private static PropertyMap CreateUserProvidedProeprtyMap(string userArgumentName, string userArgumentType)
        => new(
            userArgumentName,
            userArgumentName,
            userArgumentType,
            userArgumentType,
            false,
            false,
            true,
            true,
            userArgumentName);

    private static PropertyMap CreateUserProvidedProeprtyMap(string targetPropertyName, string userArgumentExpression, string userArgumentType)
        => new(
            targetPropertyName,
            targetPropertyName,
            userArgumentType,
            userArgumentType,
            false,
            false,
            true,
            true,
            userArgumentExpression);

    private static ConfiguredMapMockMethod? DesignClassMapMockMethod(
        ITypeSymbol source,
        ITypeSymbol destination,
        ReadOnlySpan<IPropertySymbol> destinationProperties,
        ReadOnlySpan<NameTypePair> arguments,
        bool isCompleteMethod)
    {
        var mockMethodParameters = new NameTypePair[destinationProperties.Length];

        for (var i = 0; i < destinationProperties.Length; i++)
        {
            var parameter = new NameTypePair(destinationProperties[i].Name, destinationProperties[i].Type.ToString());
            mockMethodParameters[i] = parameter;
        }

        var mockMethod = new ConfiguredMapMockMethod(
            source.ToNotNullableString(),
            destination.ToNotNullableString(),
            mockMethodParameters.ToImmutableArray());

        if (isCompleteMethod
            && CompareArgumentsAndParameters(arguments, mockMethod.Parameters.AsSpan()))
        {
            return null;
        }

        return mockMethod;
    }

    private static bool CompareArgumentsAndParameters(ReadOnlySpan<NameTypePair> arguments, ReadOnlySpan<NameTypePair> parameters)
    {
        if (parameters.Length != arguments.Length)
        {
            return false;
        }

        var typesHashSet = new HashSet<string>(StringComparer.InvariantCulture);
        foreach (var argument in arguments)
        {
            typesHashSet.Add(argument.Name);
        }
        foreach (var parameter in parameters)
        {
            typesHashSet.Add(parameter.Name);
        }

        return parameters.Length == typesHashSet.Count;
    }

}
