namespace MapReduce.Models;

internal record ChunkEnvelope(string ChunkStateKey, string Text, int Order);