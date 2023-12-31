﻿let words2 = [
    "man",
    "woman",
    "dog",
    "cat",
    "bird",
    "snake",
    "rain",
    "earth"
];

function setWords(words) {
    words2 = words;
}

function runMe() {
    let fill = d3.scale.category20();

    let layout = d3.layout.cloud()
        .size([300, 200])
        .words(words2.map(function (d) {
            return { text: d, size: 10 + Math.random() * 90, test: "haha" };
        }))
        .padding(5)
        .rotate(function () { return ~~(Math.random() * 2) * 45; })
        .font("Impact")
        .fontSize(function (d) { return d.size; })
        .on("end", draw);

    layout.start();

    let urlMy = "https://www.google.com/search?q=";

    function draw(words) {
        d3.select("#world-cloud").append("svg")
            .attr("width", layout.size()[0])
            .attr("height", layout.size()[1])
            .append("g")
            .attr("transform", "translate(" + layout.size()[0] / 2 + "," + layout.size()[1] / 2 + ")")
            .selectAll("text")
            .data(words)
            .enter().append("text")
            .style("font-size", function (d) { return d.size + "px"; })
            .style("font-family", "Impact")
            .style("background-color", "red")
            .style("fill", function (d, i) { return fill(i); })
            .attr("text-anchor", "middle")
            .attr("transform", function (d) {
                return "translate(" + [d.x, d.y] + ")rotate(" + d.rotate +")";
            })
            .text(function (d) { return d.text; })
            .on("click", function (d, i) {
                window.open(urlMy += d.text);
            })
            ;
    }
}

function createAlert() {
    alert("Hey this is an alert!");
}

function createPrompt(question) {
    return prompt(question);
}

function setElementTextById(id, text) {
    document.getElementById(id).innerText = text;
}

function focusOnElement(element) {
    element.focus();
}
function toggleIt(element) {
    element.focus();
    if (element.checked) {
        element.checked = false;
    }
    else {
        element.checked = true;
    }
}

function unfade(element) {
    let op = 0.1;  // initial opacity
    element.style.display = 'block';
    let timer = setInterval(function () {
        if (op >= 1) {
            clearInterval(timer);
        }
        element.style.opacity = op;
        element.style.filter = 'alpha(opacity=' + op * 100 + ")";
        op += op * 0.1;
    }, 10);
}