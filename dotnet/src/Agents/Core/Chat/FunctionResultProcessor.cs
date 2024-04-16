﻿// Copyright (c) Microsoft. All rights reserved.
using System.ComponentModel;
using System.Text.Json;

namespace Microsoft.SemanticKernel.Agents.Chat;

/// <summary>
/// Responsible for processing a <see cref="FunctionResult"/> and returning a strongly
/// typed result for either a <see cref="KernelFunctionSelectionStrategy"/> or
/// <see cref="KernelFunctionTerminationStrategy"/>.
/// </summary>
/// <typeparam name="TResult">The target type of the <see cref="FunctionResult"/>.</typeparam>
public abstract class FunctionResultProcessor<TResult>
{
    /// <summary>
    /// Responsible for translating the provided text result to the requested type.
    /// </summary>
    /// <param name="result">The text content from the function result.</param>
    /// <returns>A translated result.</returns>
    protected abstract TResult? ProcessTextResult(string result);

    /// <summary>
    /// Process a <see cref="FunctionResult"/> and translate to the requested type.
    /// </summary>
    /// <param name="result">The result from a <see cref="KernelFunction"/>.</param>
    /// <returns>A translated result of the requested type.</returns>
    public TResult? InterpretResult(FunctionResult result)
    {
        // Is result already of the requested type?
        if (result.GetType() == typeof(TResult))
        {
            return result.GetValue<TResult>();
        }

        string rawContent = result.GetValue<string>() ?? string.Empty;

        return this.ProcessTextResult(rawContent);
    }

    /// <summary>
    /// Convert the provided text to the processor type.
    /// </summary>
    /// <param name="result">A text result</param>
    /// <returns>A result converted to the requested type.</returns>
    protected TResult? ConvertResult(string result)
    {
        TResult? parsedResult = default;

        if (typeof(string) == typeof(TResult))
        {
            parsedResult = (TResult?)(object?)result;
        }
        else
        {
            TypeConverter? converter = TypeConverterFactory.GetTypeConverter(typeof(TResult));
            if (converter != null)
            {
                parsedResult = (TResult?)converter.ConvertFrom(result); // %%% EXCEPTION ???
            }
            else
            {
                parsedResult = JsonSerializer.Deserialize<TResult>(result);
            }
        }

        return parsedResult;
    }

    internal static FunctionResultProcessor<TResult> CreateDefaultInstance(TResult defaultValue)
        => new DefaultFunctionResultProcessor(defaultValue);

    /// <summary>
    /// Used as default for the specified type.
    /// </summary>
    private sealed class DefaultFunctionResultProcessor(TResult defaultValue) : FunctionResultProcessor<TResult>
    {
        /// <inheritdoc/>
        protected override TResult? ProcessTextResult(string result)
            => this.ConvertResult(result) ?? defaultValue;
    }
}
