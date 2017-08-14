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
requirejs(['showdown'], function (markdown) {
    console.assert(markdown != null);
    console.assert(markdown.Converter != null);

    var converter = new markdown.Converter();
    converter.setOption('tables', true);

    let filePath = (location.search || '').substring(1);
    if (filePath) {
        requirejs(['highlight', `text!${filePath}.md`, 'css!highlightStyle', 'css!style'], function (hljs, md) {

            var html = converter.makeHtml(md);
            var contentElement = document.getElementById('content');

            console.assert(contentElement != null);
            contentElement.innerHTML = html;

            var tables = contentElement.querySelectorAll('table');
            for (let i = 0; i < tables.length; i++) {
                tables.item(i).className = 'table table-bordered';
            }

            var codes = contentElement.querySelectorAll('code');
            for (let i = 0; i < codes.length; i++) {
                var str = codes.item(i).innerText;//
                //debugger;
                //  var reg = new RegExp(/&lt;/);

                str = str.replace(/&lt;/g, '<');
                str = str.replace(/&amp;lt;/g, '<');
                str = str.replace(/&gt;/g, '>');
                str = str.replace(/&amp;gt;/g, '>');
                //     //codes.item(i).innerHTML = html;
                codes.item(i).innerText = str;
            }

            hljs.initHighlighting();
        });
    }
});


