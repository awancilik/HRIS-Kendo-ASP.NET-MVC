$(function() {
    $('.closeable').click(function() {
        $(this).parent().parent().parent().hide();
    });

    $('*[confirmPrompt]').click(function(event) {
        var promptText = $(this).attr('confirmPrompt');
        if (!confirm(promptText)) {
            event.preventDefault();
        }
    });

    var url = window.location.pathname,
        urlRegExp = new RegExp(url.replace(/\/$/, ''));
    $('.horizontal-menu li > a').each(function() {
        if (urlRegExp.test($(this).attr('href'))) {
            $(this).parents('li').addClass('active');
        }
    });

});