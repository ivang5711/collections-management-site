using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Collections.Components.UI;

public partial class WordCloud
{
    [Parameter]
    public List<string> WordsImport { get; set; } = ["nothing here yet"];
    ElementReference InputToFade;

    protected override void OnAfterRender(bool firstRender)
    {

        if (firstRender)
        {
            _ = JsRuntime.InvokeVoidAsync("setWords", WordsImport);
            _ = JsRuntime.InvokeVoidAsync("runMe");
            _ = Fade();
        }
    }

    async Task Fade()
    {
        await JsRuntime.InvokeVoidAsync("unfade", InputToFade);

    }
}