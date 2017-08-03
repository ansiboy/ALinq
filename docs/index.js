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
        css: 'lib/css',
        text: 'lib/text',
        markdown: 'lib/Markdown.Converter',
        showdown: 'lib/showdown',
        highlight: 'lib/highlight.js/highlight.pack',
        highlightCS: 'lib/highlight.js/languages/cs',
        highlightStyle: 'lib/highlight.js/styles/vs'
    }
});


requirejs(['css!lib/bootstrap/css/bootstrap.css']);
requirejs(['showdown', 'text!ALinqDynamic.md'], function (markdown, md) {
    console.assert(markdown != null);
    console.assert(markdown.Converter != null);

    var converter = new markdown.Converter();
    converter.setOption('tables', true);

    var html = converter.makeHtml(md);
    var contentElement = document.getElementById('content');

    console.assert(contentElement != null);
    contentElement.innerHTML = html;

    requirejs(['highlight', 'css!highlightStyle'], function (hljs) {
        hljs.initHighlighting();
    });

});
