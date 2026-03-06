using System.Text.Json.Nodes;

namespace OpenBoxesMobile.Blazor.Services;

public sealed class SortationFlowState
{
    public JsonObject? Product { get; set; }
    public List<JsonObject> TaskList { get; set; } = [];
    public int CurrentTaskIndex { get; set; }
    public bool IsDirectPutaway { get; set; }

    public JsonObject? CurrentTask =>
        CurrentTaskIndex >= 0 && CurrentTaskIndex < TaskList.Count
            ? TaskList[CurrentTaskIndex]
            : null;

    public void Reset()
    {
        Product = null;
        TaskList = [];
        CurrentTaskIndex = 0;
        IsDirectPutaway = false;
    }
}
