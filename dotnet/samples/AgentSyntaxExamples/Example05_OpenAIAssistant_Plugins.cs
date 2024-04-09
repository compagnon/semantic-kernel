﻿// Copyright (c) Microsoft. All rights reserved.
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using Plugins;
using Xunit;
using Xunit.Abstractions;

namespace Examples;

/// <summary>
/// Demonstrate creation of <see cref="ChatCompletionAgent"/> and
/// eliciting its response to three explicit user messages.
/// </summary>
public class Example05_OpenAIAssistant_Plugins : BaseTest
{
    private const string HostName = "Host";
    private const string HostInstructions = "Answer questions about the menu.";

    [Fact]
    public async Task RunAsync()
    {
        // Define the agent
        OpenAIAssistantAgent agent =
            await OpenAIAssistantAgent.CreateAsync(
                kernel: this.CreateEmptyKernel(),
                config: new(this.GetApiKey(), this.GetEndpoint()),
                new()
                {
                    Instructions = HostInstructions,
                    Name = HostName,
                    Model = this.GetModel(),
                });

        // Initialize plugin and add to the agent's Kernel (same as direct Kernel usage).
        KernelPlugin plugin = KernelPluginFactory.CreateFromType<MenuPlugin>();
        agent.Kernel.Plugins.Add(plugin);

        // Create a chat for agent interaction.
        var chat = new AgentGroupChat();

        // Respond to user input
        try
        {
            await WriteAgentResponseAsync("Hello");
            await WriteAgentResponseAsync("What is the special soup?");
            await WriteAgentResponseAsync("What is the special drink?");
            await WriteAgentResponseAsync("Thank you");
        }
        finally
        {
            await agent.DeleteAsync();
        }

        // Local function to invoke agent and display the conversation messages.
        async Task WriteAgentResponseAsync(string input)
        {
            chat.AddUserMessage(input);
            this.WriteLine($"# {AuthorRole.User}: '{input}'");

            await foreach (var content in chat.InvokeAsync(agent))
            {
                this.WriteLine($"# {content.Role} - {content.AuthorName ?? "*"}: '{content.Content}'");
            }
        }
    }

    public Example05_OpenAIAssistant_Plugins(ITestOutputHelper output)
        : base(output)
    { }
}
