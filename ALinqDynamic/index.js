requirejs.config({
    shim: {
        markdown: {
            exports: 'Markdown'
        },
        showdown: {
            exports: 'showdown'
        },
        highlightCS: {
            deps: ['highlight']
        }
    },
    paths: {
        css: '../lib/css',
        text: '../lib/text',
        markdown: '../lib/Markdown.Converter',
        showdown: '../lib/showdown',
        highlight: '../lib/highlight.js/highlight.pack',
        highlightCS: '../lib/highlight.js/languages/cs',
        highlightStyle: '../lib/highlight.js/styles/vs'
    }
});
requirejs(['css!../lib/bootstrap/css/bootstrap.css']);
requirejs(['showdown', 'text!index.md'], function (markdown, md) {
    console.assert(markdown != null);
    console.assert(markdown.Converter != null);
    var converter = new markdown.Converter();
    converter.setOption('tables', true);
    var html = converter.makeHtml(md);
    var contentElement = document.getElementById('content');
    console.assert(contentElement != null);
    contentElement.innerHTML = html;
    var ul = contentElement.querySelector('ul');
    // for (let i = 0; i < uls.length; i++) {
    processUL(ul, 2);
    // }
    requirejs(['highlight', 'css!highlightStyle'], function (hljs) {
        hljs.initHighlighting();
    });
});
function processUL(element, level) {
    var lis = element.children;
    for (var i = 0; i < lis.length; i++) {
        var li = lis.item(i);
        var textNode = li.childNodes.item(0);
        var h = document.createElement("h" + level);
        h.innerText = textNode.textContent;
        li.insertBefore(h, textNode);
        li.removeChild(textNode);
        var childElement = li.children.item(1);
        if (childElement != null && childElement.tagName == 'UL') {
            processUL(childElement, level + 1);
        }
        // li.innerHTML = `<h${level}>${li.innerHTML}</h${level}>`
    }
}
