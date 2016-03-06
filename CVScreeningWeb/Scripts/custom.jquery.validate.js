/*!
** Custom Jquery validation. Used to override Jquery function
** Copyright (C) Microsoft Corporation. All rights reserved.
*/



/*! 
** Date with format dd/mm/yyyy are not validated by Chrome
** http://www.robertgray.net.au/posts/2012/11/jqueryvalidate-and-chrome-doesnt-validate-australian-dates#.U5Kvj_mSxbU        
*/

function getDateFormat(formatString) {

    var separator = formatString.match(/[.\/\-\s].*?/),
        parts = formatString.split(/\W+/);
    if (!separator || !parts || parts.length === 0) {
        throw new Error("Invalid date format.");
    }
    return { separator: separator, parts: parts };
}

function MyParseDate(date, format) {
    var parts = date.split(format.separator),
        date = new Date(),
        val;
    date.setHours(0);
    date.setMinutes(0);
    date.setSeconds(0);
    date.setMilliseconds(0);
    if (parts.length === format.parts.length) {
        console.log(parts.length);
        for (var i = 0, cnt = format.parts.length; i < cnt; i++) {
            val = parseInt(parts[i], 10) || 1;

            switch (format.parts[i]) {
                case 'dd':
                case 'd':
                    date.setDate(val);
                    break;
                case 'mm':
                case 'm':
                    date.setMonth(val);
                    break;
                case 'yy':
                    date.setFullYear(2000 + val);
                    break;
                case 'yyyy':
                    date.setFullYear(val);
                    break;
            }
        }
    }

    return date;
}

jQuery.validator.addMethod('date',
               function (value, element, params) {
                   if (this.optional(element)) {
                       return true;
                   }
                   var result = false;
                   try {
                       var format = getDateFormat('dd/mm/yyyy');
                       MyParseDate(value, format);
                       result = true;
                   } catch (err) {
                       console.log(err);
                       result = false;
                   }
                   return result;
               });

