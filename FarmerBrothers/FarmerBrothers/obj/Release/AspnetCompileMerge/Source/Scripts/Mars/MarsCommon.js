function AlertPopup( message )
{
    var messages = message.split("|");
    
    $('#alertmessage').text('');
    if (messages.length > 1) {
        $('#alertmessage').append('<ul>');
        for (var i = 0; i < messages.length; i++) {
            if (messages[i].length > 0) {
                $('#alertmessage').append('<li>' + messages[i] + '</li>');
            }
        }
        $('#alertmessage').append('</ul>');
    }
    else {
        $('#alertmessage').text(message);
    }
    $("[data-popup='popupalert']").show();
}

