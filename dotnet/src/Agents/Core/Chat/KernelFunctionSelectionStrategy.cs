﻿// Copyright (c) Microsoft. All rights reserved.
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.SemanticKernel.Agents.Chat;

/// <summary>
/// Round-robin turn-taking strategy.  Agent order is based on the order
/// in which they joined <see cref="AgentGroupChat"/>.
/// </summary>
public class KernelFunctionSelectionStrategy(KernelFunction function) : SelectionStrategy
{
    /// <summary>
    /// A well-known <see cref="KernelArguments"/> key associated with the list of agent names.
    /// </summary>
    public const string ArgumentKeyAgents = "_agents_";

    /// <summary>
    /// A well-known <see cref="KernelArguments"/> key associated with the chat history.
    /// </summary>
    public const string ArgumentKeyHistory = "_history_";

    /// <summary>
    /// Optional arguments used when invoking <see cref="KernelFunctionSelectionStrategy.Function"/>.
    /// </summary>
    public KernelArguments? Arguments { get; init; }

    /// <summary>
    /// The <see cref="Microsoft.SemanticKernel.Kernel"/> used when invoking <see cref="KernelFunctionSelectionStrategy.Function"/>.
    /// </summary>
    public Kernel Kernel { get; set; } = Kernel.CreateBuilder().Build();

    /// <summary>
    /// The <see cref="KernelFunction"/> invoked as selection criteria.
    /// </summary>
    public KernelFunction Function { get; } = function;

    /// <summary>
    /// A <see cref="FunctionResultProcessor{TResult}"/> responsible for translating the <see cref="FunctionResult"/>
    /// to the termination criteria.
    /// </summary>
    public FunctionResultProcessor<string> ResultParser { get; init; } = DefaultInstance;

    /// <summary>
    /// The default selection parser that selects no agent.
    /// </summary>
    private static FunctionResultProcessor<string> DefaultInstance { get; } = FunctionResultProcessor<string>.CreateDefaultInstance(string.Empty);

    /// <inheritdoc/>
    public sealed override async Task<Agent> NextAsync(IReadOnlyList<Agent> agents, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken = default)
    {
        KernelArguments originalArguments = this.Arguments ?? [];
        KernelArguments arguments =
            new(originalArguments, originalArguments.ExecutionSettings?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value))
            {
                { ArgumentKeyAgents, string.Join(",", agents.Select(a => a.Name)) },
                { ArgumentKeyHistory, JsonSerializer.Serialize(history) }, // TODO: GitHub Task #5894
            };

        FunctionResult result = await this.Function.InvokeAsync(this.Kernel, arguments, cancellationToken).ConfigureAwait(false);

        string agentName =
            this.ResultParser.InterpretResult(result) ??
            throw new KernelException("Agent Failure - Strategy unable to determine selection result.");

        return
            agents.Where(a => a.Name == agentName).FirstOrDefault() ??
            throw new KernelException("Agent Failure - Strategy unable to select next agent.");
    }
}
