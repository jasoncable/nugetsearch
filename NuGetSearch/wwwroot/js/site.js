var nuGetSearchIndex = (function () {
    var my = {};

    my.setup = function () {
        $('.page-link').off('click').click(function(){
            $('#Page').val($(this).attr('data-page-id'));
            $('form').submit();
        });
        
        $('#SearchInput').change(function(){
            $('#Page').val(0);
        });
        
        
    };

    return my;
}());