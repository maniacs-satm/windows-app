function updateReadingProgress() {
    var root = document.getElementsByTagName("html")[0];
    var top = document.body.scrollTop;
    var scrollPercentage = 100 * top / (root.scrollHeight - root.clientHeight);
    window.external.notify(scrollPercentage.toString());
}

function changeHtmlAttributes(color, font, fontsize, textalign) {
    document.getElementsByTagName("html")[0].setAttribute("data-color", color);
    document.getElementsByTagName("html")[0].setAttribute("data-font", font);

    document.body.style.fontSize = parseFloat(fontsize) + "px";
    document.body.style.textAlign = textalign;
}

function Initialize() {
    var root = document.getElementsByTagName("html")[0];
    var progress = parseFloat(root.getAttribute("data-progress"));

    var progressCalculated = progress / 100 * (root.scrollHeight - root.clientHeight);
    document.body.scrollTop = progressCalculated;

    $(document).ready(function () {
        $("article").fitVids({ customSelector: "iframe[src^='https://medium.com/media']" });
    });
}

// Copied and modified from: https://stackoverflow.com/a/13433551
function updateTagsElement(newTags) {
    var Obj = document.getElementById("wallabag-tag-list");
    Obj.outerHTML = newTags;
}

function getSelectionText() {
    var text = "";
    if (window.getSelection) {
        text = window.getSelection().toString();
    } else if (document.selection && document.selection.type != "Control") {
        text = document.selection.createRange().text;
    }
    return text;
}