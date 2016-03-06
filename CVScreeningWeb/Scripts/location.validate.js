
jQuery.validator.addMethod('locationmandatory', function (value, element, params) {
    return true;
}, '');

jQuery.validator.unobtrusive.adapters.add('locationmandatory', {}, function (options) {
    options.rules['locationmandatory'] = true;
    options.messages['locationmandatory'] = options.message;
});