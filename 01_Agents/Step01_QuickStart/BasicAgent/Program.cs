using Azure.AI.OpenAI;
using BasicAgent.Models;
using BasicAgent.Plugins;
using CommonShared;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;
using System.Text.Json;

// Load configuration
var config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
#if DEBUG
    .AddJsonFile($"appsettings.Secrets.json", optional: true, reloadOnChange: true)
#endif
    .Build();
var openAIProvider = config.GetSection("OpenAI").Get<OpenAIProvider>();
var azureOpenAIProvider = config.GetSection("AzureOpenAI").Get<OpenAIProvider>();

#region 01-Create an agent with OpenAI
/*
 * Demo01: Create an agent with OpenAI
 */
//var jokerAgent = new OpenAIClient(
//        new ApiKeyCredential(openAIProvider.ApiKey),
//        new OpenAIClientOptions { Endpoint = new Uri(openAIProvider.Endpoint) })
//    .GetChatClient(openAIProvider.ModelId)
//    .CreateAIAgent(instructions: "You are good at telling jokes.", name: "Joker");
//// Method 1: RunAsync
//Console.WriteLine(await jokerAgent.RunAsync("Tell me a joke about a pirate."));
//// Method 2: RunStreamingAsync
//await foreach (var update in jokerAgent.RunStreamingAsync("Tell me a joke about a car."))
//{
//    Console.Write(update);
//}
#endregion

#region 02-Create an agent with AzureOpenAI
/*
 * Demo01: Create an agent with AzureOpenAI
 */
//var jokerAgent = new AzureOpenAIClient(
//    new Uri(azureOpenAIProvider.Endpoint),
//    new ApiKeyCredential(azureOpenAIProvider.ApiKey))
//    .GetChatClient(azureOpenAIProvider.ModelId)
//    .CreateAIAgent(instructions: "You are good at telling jokes.", name: "Joker");
//Console.WriteLine(await jokerAgent.RunAsync("Tell me a joke about a pirate."));
#endregion

#region 03-Running the agent with multi-turn conversation
/*
 * Hold a multi-turn conversation with the agent by creating multiple threads.
 */
//var thread1 = jokerAgent.GetNewThread();
//var thread2 = jokerAgent.GetNewThread();

//Console.WriteLine(await jokerAgent.RunAsync("Tell me a joke about a pirate.", thread1) + Environment.NewLine);
//Console.WriteLine(await jokerAgent.RunAsync("Now add some emojis to the joke and tell it in the voice of a pirate's parrot.", thread1) + Environment.NewLine);

//Console.WriteLine(await jokerAgent.RunAsync("Tell me a joke about a robot.", thread2) + Environment.NewLine);
//Console.WriteLine(await jokerAgent.RunAsync("Now add some emojis to the joke and tell it in the voice of a robot.", thread2) + Environment.NewLine);
#endregion

#region 04-Using function tools directly
//var agent = new OpenAIClient(
//        new ApiKeyCredential(openAIProvider.ApiKey),
//        new OpenAIClientOptions { Endpoint = new Uri(openAIProvider.Endpoint) })
//    .GetChatClient(openAIProvider.ModelId)
//    .CreateAIAgent(instructions: "You are a helpful assistant", tools: [AIFunctionFactory.Create(WeatherServicePlugin.GetCurrentWeatherAsync)]);
//Console.WriteLine(await agent.RunAsync("Tell me the weather like in Chengdu?"));
#endregion

#region 05-Using function tools with Approval
//var weatherTool = AIFunctionFactory.Create(WeatherServicePlugin.GetCurrentWeatherAsync);
//var approvalRequiredWeatherTool = new ApprovalRequiredAIFunction(weatherTool);

//var agent = new OpenAIClient(
//        new ApiKeyCredential(openAIProvider.ApiKey),
//        new OpenAIClientOptions { Endpoint = new Uri(openAIProvider.Endpoint) })
//    .GetChatClient(openAIProvider.ModelId)
//    .CreateAIAgent(instructions: "You are a helpful assistant", tools: [approvalRequiredWeatherTool]);

//var thread = agent.GetNewThread();
//var response = await agent.RunAsync("Tell me the weather like in Chengdu?", thread);
//var userInputRequests = response.UserInputRequests.ToList();

//while(userInputRequests.Count > 0)
//{
//    // Ask the user to approve each function call request.
//    // For simplicity, we are assuming here that only function approval requests are being made.
//    var userInputResponses = userInputRequests
//        .OfType<FunctionApprovalRequestContent>()
//        .Select(functionApprovalRequest =>
//        {
//            Console.WriteLine($"The agent would like to invoke the following function, please reply Y to approve: Name {functionApprovalRequest.FunctionCall.Name}");
//            return new ChatMessage(ChatRole.User, [functionApprovalRequest.CreateResponse(Console.ReadLine()?.Equals("Y", StringComparison.OrdinalIgnoreCase) ?? false)]);
//        })
//        .ToList();

//    // Pass the user input responses back to the agent for further processing.
//    response = await agent.RunAsync(userInputResponses, thread);

//    userInputRequests = response.UserInputRequests.ToList();
//}

//Console.WriteLine(Environment.NewLine + $"Agent: {response}");
#endregion

#region 06-Structured Output
var schema = AIJsonUtilities.CreateJsonSchema(typeof(PersonInfo));
var chatOptions = new ChatOptions()
{
    ResponseFormat = ChatResponseFormat.ForJsonSchema(
        schema: schema,
        schemaName: "PersonInfo",
        schemaDescription: "Information about a person including their name, age, and occupation")
};
var agent = new OpenAIClient(
        new ApiKeyCredential(openAIProvider.ApiKey),
        new OpenAIClientOptions { Endpoint = new Uri(openAIProvider.Endpoint) })
    .GetChatClient(openAIProvider.ModelId)
    .CreateAIAgent(new ChatClientAgentOptions()
    {
        Name = "HelpfulAssistant",
        Instructions = "You are a helpful assistant.",
        ChatOptions = chatOptions
    });

var response = await agent.RunAsync("Please provide information about John Smith, who is a 35-year-old software engineer.");
var pesonInfo = response.Deserialize<PersonInfo>(JsonSerializerOptions.Web);
Console.WriteLine("Assistant Output:");
Console.WriteLine($"Name: {pesonInfo.Name}");
Console.WriteLine($"Age: {pesonInfo.Age}");
Console.WriteLine($"Occupation: {pesonInfo.Occupation}");
#endregion

Console.ReadKey();