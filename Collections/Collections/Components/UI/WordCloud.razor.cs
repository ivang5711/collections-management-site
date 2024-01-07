using Microsoft.JSInterop;

namespace Collections.Components.UI;

public partial class WordCloud
{
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            JsRuntime.InvokeVoidAsync("runMe");
        }
    }
}