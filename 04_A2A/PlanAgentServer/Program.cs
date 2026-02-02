using A2A;
using A2A.AspNetCore;
using PlanAgentServer;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Create and register your agent
var taskManager = new TaskManager();
var agent = new PlanAgent();
agent.Attach(taskManager);
app.MapA2A(taskManager, "/plans");
app.MapWellKnownAgentCard(taskManager, "/plans");
app.MapHttpA2A(taskManager, "/plans");

app.Run();