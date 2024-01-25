let tags = [];

function setTags(words) {
    tags = words;
}

function runTagCloud() {
    let fill = d3.scale.category20();

    let layout = d3.layout.cloud()
        .size([300, 200])
        .words(tags.map(function (d) {
            return { text: d, size: 10 + Math.random() * 90, test: "test" };
        }))
        .padding(5)
        .rotate(function () { return ~~(Math.random() * 2) * 90; })
        .font("Impact")
        .fontSize(function (d) { return d.size; })
        .on("end", draw);

    layout.start();

    function draw(words) {
        d3.select("#world-cloud").append("svg")
            .attr("width", layout.size()[0])
            .attr("height", layout.size()[1])
            .append("g")
            .attr(
                "transform",
                "translate(" + layout.size()[0] / 2 +
                "," + layout.size()[1] / 2 + ")")
            .selectAll("text")
            .data(words)
            .enter().append("text")
            .style("font-size", function (d) { return d.size + "px"; })
            .style("font-family", "Impact")
            .style("background-color", "red")
            .style("fill", function (d, i) { return fill(i); })
            .attr("text-anchor", "middle")
            .attr("transform", function (d) {
                return "translate(" + [d.x, d.y] + ")rotate(" + d.rotate + ")";
            })
            .text(function (d) { return d.text; })
            .on("click", function (d) {
                setLocalStoreData("searchText", d.text);
                window.open("/search");
            });
    }
}

function setLocalStoreData(key, value) {
    localStorage.setItem(key, value);
}

function getLocalStoreData(key) {
    let lastname = localStorage.getItem(key);
    return lastname;
}

function removeLocalStoreData(key) {
    localStorage.removeItem(key);
}

function setElementTextById(id, text) {
    document.getElementById(id).innerText = text;
}

function focusOnElement(element) {
    element.focus();
}
function toggleCheckbox(element) {
    element.focus();
    if (element.checked) {
        element.checked = false;
    }
    else {
        element.checked = true;
    }
}

function fadeIn(element) {
    let opacity = 0.1;
    element.style.display = 'block';
    let timer = setInterval(function () {
        if (opacity >= 1) {
            clearInterval(timer);
        }
        element.style.opacity = opacity;
        element.style.filter = 'alpha(opacity=' + opacity * 100 + ")";
        opacity += opacity * 0.1;
    }, 10);
}