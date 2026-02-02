using A2A;
using A2A.AspNetCore;
using WeatherAgentServer;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var taskManager = new TaskManager();
var agent = new WeatherAgent();
agent.Attach(taskManager);
// Add JSON-RPC endpoint for A2A
app.MapA2A(taskManager, "/weather");
// Add well-known agent card endpoint for A2A
app.MapWellKnownAgentCard(taskManager, "/weather");
// Add HTTP endpoint for A2A
app.MapHttpA2A(taskManager, "/weather");

app.Run();