namespace MultiSelectionEdge.Models;

internal record ContentSubmission(
    string Id, 
    string Title, 
    string Category, 
    int Length, 
    bool ContainsRisk, 
    string Language);