let texts = [
    '3D', 'TagCloud', 'JavaScript',
    'CSS3', 'Animation', 'Interactive',
    'Mouse', 'Rolling', 'Sphere',
    '6KB', 'v2.x',
];
let tc = TagCloud('.content', texts);
let color = '#FF5733';
document.querySelector('.content').style.color = color;

let rootEl = document.querySelector('.content');
rootEl.addEventListener('click', function clickEventHandler(e) {
    if (e.target.className === 'tagcloud--item') {
        window.open(
            `https://www.bing.com/search?q=${e.target.innerText}`,
            '_blank');
    }
});

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
let otherTcs = [];
// create and destroy tagcloud
function toCreate() {
    if (otherTcs.length >= 3) return;
    otherTcs.push(TagCloud('.content', texts));
}
function toDestroy() {
    let last = otherTcs[otherTcs.length - 1];
    if (!last) return;
    last.destroy();
    otherTcs.pop();
}