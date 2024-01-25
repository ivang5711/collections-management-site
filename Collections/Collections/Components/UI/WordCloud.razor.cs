using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Collections.Components.UI;

public partial class WordCloud
{
    [Parameter]
    public List<string> WordsImport { get; set; } = ["nothing here yet"];

    private ElementReference InputToFade;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JsRuntime.InvokeVoidAsync("setTags", WordsImport);
            await JsRuntime.InvokeVoidAsync("runTagCloud");
            await Fade();
        }
    }

    private async Task Fade()
    {
        await JsRuntime.InvokeVoidAsync("fadeIn", InputToFade);
    }
}