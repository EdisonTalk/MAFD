using A2A;
using A2A.AspNetCore;
using HotelAgentServer;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Create and register your agent
var taskManager = new TaskManager();
var agent = new HotelAgent();
agent.Attach(taskManager);
app.MapA2A(taskManager, "/hotels");
app.MapWellKnownAgentCard(taskManager, "/hotels");
app.MapHttpA2A(taskManager, "/hotels");

app.Run();