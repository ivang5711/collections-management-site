// Called when the script first gets loaded on the page.
export function onLoad() {
    console.log('Load');
    runTag();
}

// Called when an enhanced page update occurs, plus once immediately after
// the initial load.
export function onUpdate() {
    console.log('Update');
    runTag();
}

// Called when an enhanced page update removes the script from the page.
export function onDispose() {
    console.log('Dispose');
}

function runTag() {


    var texts = [
        '3D', 'TagCloud', 'JavaScript',
        'CSS3', 'Animation', 'Interactive',
        'Mouse', 'Rolling', 'Sphere',
        '6KB', 'v2.x',
    ];
    //var tc = TagCloud('.content', texts);
    // console.log(tc);
    var color = '#FF5733';
    document.querySelector('.content').style.color = color;

    let rootEl = document.querySelector('.content');
    rootEl.addEventListener('click', function clickEventHandler(e) {
        if (e.target.className === 'tagcloud--item') {
            window.open(`https://www.bing.com/search?q=${e.target.innerText}`, '_blank');
            // your code here
        }
    });
}

    // switch style
    function toDefault() { document.body.classList.remove('light'); }
    function toLight() { document.body.classList.add('light'); }
    // add / remove tag
    function addTag() {
        if (!tc) return;
        texts.push('New');
        tc.update(texts);
    }
    function removeTag() {
        if (!tc) return;
        texts.pop();
        tc.update(texts);
    }
    var otherTcs = [];
    // create and destroy tagcloud
    function toCreate() {
        if (otherTcs.length >= 3) return;
        otherTcs.push(TagCloud('.content', texts));
    }
    function toDestroy() {
        var last = otherTcs[otherTcs.length - 1];
        if (!last) return;
        last.destroy();
        otherTcs.pop();
    }