using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Collections.Components.UI;

public partial class WordCloud
{
    [Parameter]
    public List<string> WordsImport { get; set; } = ["nothing here yet"];

    protected override void OnAfterRender(bool firstRender)
    {

        if (firstRender)
        {
            _ = JsRuntime.InvokeVoidAsync("setWords", WordsImport);
            _ = JsRuntime.InvokeVoidAsync("runMe");
        }
    }
}