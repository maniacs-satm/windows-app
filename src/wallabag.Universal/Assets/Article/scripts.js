function updateReadingProgress() {
    var root = document.getElementsByTagName("html")[0];
    var top = document.body.scrollTop;
    var scrollPercentage = 100 * top / (root.scrollHeight - root.clientHeight);
    window.external.notify(scrollPercentage.toString());
}

function changeHtmlAttributes(color, font, fontsize, lineheight) {
    document.getElementsByTagName("html")[0].setAttribute("data-color", color);
    document.getElementsByTagName("html")[0].setAttribute("data-font", font);

    document.body.style.fontSize = parseFloat(fontsize) + "px";
    document.body.style.lineHeight = parseFloat(lineheight);
}

function Initialize() {
    var root = document.getElementsByTagName("html")[0];
    var fontSize = root.getAttribute("data-font-size");
    var lineHeight = root.getAttribute("data-line-height");
    var progress = root.getAttribute("data-progress");

    document.body.style.fontSize = parseFloat(fontSize) + "px";
    document.body.style.lineHeight = parseFloat(lineHeight);

    var progressCalculated = progress / 100 * (root.scrollHeight - root.clientHeight);
    document.body.scrollTop = progressCalculated;    
}