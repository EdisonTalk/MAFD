using AdvancedAgent.Plugins;
using CommonShared;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using OpenAI;
using System.ClientModel;

// Load configuration
var config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
#if DEBUG
    .AddJsonFile($"appsettings.Secrets.json", optional: true, reloadOnChange: true)
#endif
    .Build();
var openAIProvider = config.GetSection("OpenAI").Get<OpenAIProvider>();

#region 01-Using an Agent as a function tool
//// Step1. Create an AI agent that uses a weather service plugin
//var weatherAgent = new OpenAIClient(
//        new ApiKeyCredential(openAIProvider.ApiKey),
//        new OpenAIClientOptions { Endpoint = new Uri(openAIProvider.Endpoint) })
//    .GetChatClient(openAIProvider.ModelId)
//    .CreateAIAgent(
//        instructions: "You answer questions about the weather.",
//        name: "WeatherAgent",
//        description: "An agent that answers questions about the weather.",
//        tools: [AIFunctionFactory.Create(WeatherServicePlugin.GetWeatherAsync)]);
//// Step2. Create another AI agent that uses the weather agent as a function tool
//var mainAgent = new OpenAIClient(
//        new ApiKeyCredential(openAIProvider.ApiKey),
//        new OpenAIClientOptions { Endpoint = new Uri(openAIProvider.Endpoint) })
//    .GetChatClient(openAIProvider.ModelId)
//    .CreateAIAgent(instructions: "You are a helpful assistant who responds message in Chinese.", tools: [weatherAgent.AsAIFunction()]);
//// Step3. Test the portal agent
//Console.WriteLine(await mainAgent.RunAsync("What is the weather like in Chengdu?"));
#endregion

#region 02-Exposing an Agent as a MCP tool
//var jokerAgent = new OpenAIClient(
//        new ApiKeyCredential(openAIProvider.ApiKey),
//        new OpenAIClientOptions { Endpoint = new Uri(openAIProvider.Endpoint) })
//    .GetChatClient(openAIProvider.ModelId)
//    .CreateAIAgent(instructions: "You are good at telling jokes.", name: "Joker");
//// Expose the agent as a MCP tool
//var jokerMcpTool = McpServerTool.Create(jokerAgent.AsAIFunction());
//// Create a MCP server and register the tool
//// Register the MCP server with StdIO transport and expose the tool via the server.
//var builder = Host.CreateEmptyApplicationBuilder(settings: null);
//builder.Services
//    .AddMcpServer()
//    .WithStdioServerTransport()
//    .WithTools([jokerMcpTool]);
//await builder
//    .Build()
//    .RunAsync();
#endregion

#region 03-Persisted Conversations

#endregion

Console.ReadKey();