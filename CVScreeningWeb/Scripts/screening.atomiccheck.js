/// <reference path="_references.js" />



(function () {

    this.screeningNamespace = this.screeningNamespace || {};
    var ns = this.screeningNamespace;

    // Namespace variables
    ns.statusDropDownList = null; // Proxy object for status drop down list
    ns.validationStatusDropDownList = null; // Proxy object for validation status drop down list
    ns.ListElements = new Array(); // List of elements that could be present in dropdown list
    ns.defaultStatusValue = null;   // Atomic check status, set when the page is loaded
    ns.defaultValidationStatusValue = null;   // Atomic check status, set when the page is loaded
    ns.typeOfCheck = ""; // Type of check of the atomic check

    ///
    /// CLASS DECLARATION
    ///

    //
    // Element present in the dropdown list
    //
    ns.SelectElement = (function () {
        // Constructor
        function SelectElement(elementId, elementValue) {
            this.elementId = elementId;
            this.elementValue = elementValue;
        }

        SelectElement.prototype.generateElementTag = function () {
            return '<option value="' + this.elementId + '">' + this.elementValue + '</option>';
        };

        return SelectElement;
    })();

    ns.CanBeWronglyQualified = function () {
        if (this.typeOfCheck == "Media check simplified" || this.typeOfCheck == "Media check comprehensive" || this.typeOfCheck == "Reference check"
            || this.typeOfCheck == "Reverse directorship" || this.typeOfCheck == "Group sanctions check"
            || this.typeOfCheck == "Contact number verifications check" || this.typeOfCheck == "Medical check"
            || this.typeOfCheck == "Credit check") {
            return false;
        } else {
            return true;
        }
    };

    //
    // Atomic check state Object
    //
    ns.AtomicCheckState = (function () {
        // Constructor
        function AtomicCheckState() {
        }

        AtomicCheckState.prototype.getSelectedValue = function () {
            return ns.statusDropDownList.val;
        };

        AtomicCheckState.prototype.getSelectedText = function () {
            return $('#Status_PostData :selected').text();
        };

        AtomicCheckState.prototype.onGoingClick = function () {
            ns.validationStatusDropDownList.attr("disabled", true);
            ns.buildValidationStatusList();
        };

        AtomicCheckState.prototype.doneOkClick = function () {
            ns.validationStatusDropDownList.attr("disabled", false);
            ns.buildValidationStatusList();
        };

        AtomicCheckState.prototype.doneKoClick = function () {
            ns.validationStatusDropDownList.attr("disabled", false);
            ns.buildValidationStatusList();
        };

        AtomicCheckState.prototype.doneDiscrepancy = function () {
            ns.validationStatusDropDownList.attr("disabled", false);
            ns.buildValidationStatusList();
        };

        AtomicCheckState.prototype.doneImpossible = function () {
            ns.validationStatusDropDownList.attr("disabled", false);
            ns.buildValidationStatusList();
        };

        AtomicCheckState.prototype.onProcessForwarded = function () {
            ns.validationStatusDropDownList.attr("disabled", true);
            ns.buildValidationStatusList();
        };

        AtomicCheckState.prototype.pendingConfirmation = function () {
            ns.validationStatusDropDownList.attr("disabled", true);
            ns.buildValidationStatusList();
        };

        AtomicCheckState.prototype.wronglyQualified = function () {
            ns.validationStatusDropDownList.attr("disabled", true);
            ns.buildValidationStatusList();
        };

        AtomicCheckState.prototype.clear = function () {

        };
        return AtomicCheckState;
    }());

    //
    // Atomic check validation
    //
    ns.AtomicCheckValidationState = (function () {
        function AtomicCheckValidationState() {
        }
        AtomicCheckValidationState.prototype.notProcessedClick = function () {
            ns.buildStatusList();
        };
        AtomicCheckValidationState.prototype.processedClick = function () {
            ns.buildStatusList();
        };
        AtomicCheckValidationState.prototype.rejectedClick = function () {
            ns.buildStatusList();
        };
        AtomicCheckValidationState.prototype.validatedClick = function () {
            ns.buildStatusList();
        };
        AtomicCheckValidationState.prototype.clear = function () {

        };
        return AtomicCheckValidationState;
    }());

    //
    // Generate html tag to build dropdownlist from elements given in parameter
    //
    ns.generateList = function (elements) {
        var list = '';
        for (var i = 0; i < elements.length; i++) {
            var elem = ns.ListElements[elements[i]];
            list += elem.generateElementTag();
        }
        return list;
    };

    //
    // Element present in the dropdown list
    //
    ns.getNextValidationStatusAvailable = function () {

        // If pending confirmation is default value, when use click on status DONE_* 
        // next validation status are NOT PROCESSED or PROCESSED
        if (ns.defaultStatusValue == '8') {
            return ['NOT_PROCESSED', 'PROCESSED'];
        }
        else {
            switch (ns.defaultValidationStatusValue) {
                case '1': // Not processed
                    return ['NOT_PROCESSED', 'PROCESSED'];
                case '2': // Processed
                    return ['PROCESSED', 'REJECTED', 'VALIDATED'];
                case '3': // Rejected
                    return ['PROCESSED', 'REJECTED'];
                case '4': // Validated
                    return ['REJECTED', 'VALIDATED'];
            }
        }
        return "";
    };


    //
    // Element present in the dropdown list
    //
    ns.buildStatusList = function () {

        var value = ns.statusDropDownList.val();

        switch (ns.validationStatusDropDownList.val()) {
            case '1': // Not processed
                var ret = ns.CanBeWronglyQualified();
                if (ns.CanBeWronglyQualified()) {
                    ns.statusDropDownList.html(ns.generateList([
                        'ON_GOING', 'DONE_OK', 'DONE_KO', 'DONE_DISCREPANCY', 'DONE_IMPOSSIBLE', 'ON_PROCESS_FORWARDED', 'PENDING_CONFIRMATION', 'WRONGLFY_QUALIFIED']));
                } else {
                    ns.statusDropDownList.html(ns.generateList([
                        'ON_GOING', 'DONE_OK', 'DONE_KO', 'DONE_DISCREPANCY', 'DONE_IMPOSSIBLE', 'ON_PROCESS_FORWARDED', 'PENDING_CONFIRMATION']));
                }
                break;
            case '2': // Processed
                ns.statusDropDownList.html(ns.generateList([
                    'DONE_OK', 'DONE_KO', 'DONE_DISCREPANCY', 'DONE_IMPOSSIBLE']));
                break; 
            case '3': // Rejected
                ns.statusDropDownList.html(ns.generateList([
                    'DONE_OK', 'DONE_KO', 'DONE_DISCREPANCY', 'DONE_IMPOSSIBLE', 'WRONGLFY_QUALIFIED']));
                break;
            case '4': // Validated
                var statusList = ['DONE_OK', 'DONE_KO', 'DONE_DISCREPANCY', 'DONE_IMPOSSIBLE'];

                // Pending confirmation atomic check are automatically validated
                if (ns.defaultStatusValue == '8') {
                    ns.validationStatusDropDownList.attr("disabled", true);
                    statusList.push('PENDING_CONFIRMATION');
                }

                ns.statusDropDownList.html(ns.generateList(statusList));
                break;
        }

        ns.statusDropDownList.val(value);
    };

    //
    // Element present in the dropdown list
    //
    ns.buildValidationStatusList = function () {

        var value = ns.validationStatusDropDownList.val();

        switch (ns.statusDropDownList.val()) {
            case '2': // On going
                ns.validationStatusDropDownList.html(ns.generateList(['NOT_PROCESSED']));
                break;
            case '3': // Done OK
                ns.validationStatusDropDownList.html(ns.generateList(ns.getNextValidationStatusAvailable()));
                break;
            case '4': // Done KO
                ns.validationStatusDropDownList.html(ns.generateList(ns.getNextValidationStatusAvailable()));
                break;
            case '5': // Done discrepancy
                ns.validationStatusDropDownList.html(ns.generateList(ns.getNextValidationStatusAvailable()));
                break;
            case '6': // Done impossible
                ns.validationStatusDropDownList.html(ns.generateList(ns.getNextValidationStatusAvailable()));
                break;
            case '7': // On process forwarded
                ns.validationStatusDropDownList.html(ns.generateList(['NOT_PROCESSED']));
                break;
            case '8': // Pending confirmation
                ns.validationStatusDropDownList.html(ns.generateList(['VALIDATED']));
                break;
            case '9': // Wrongly qualified
                ns.validationStatusDropDownList.html(ns.generateList(['NOT_PROCESSED']));
                break;
        }

        ns.validationStatusDropDownList.val(value);

    };

    ns.buildStatusAsString = function () {

        // Element for status dropdown list
        ns.ListElements['NEW'] = new ns.SelectElement(2, 'New');
        ns.ListElements['ON_GOING'] = new ns.SelectElement(2, 'Ongoing');
        ns.ListElements['DONE_OK'] = new ns.SelectElement(3, 'Validated');
        ns.ListElements['DONE_KO'] = new ns.SelectElement(4, 'Invalid');
        ns.ListElements['DONE_DISCREPANCY'] = new ns.SelectElement(5, 'Discrepancy');
        ns.ListElements['DONE_IMPOSSIBLE'] = new ns.SelectElement(6, 'Cannot be verified');
        ns.ListElements['ON_PROCESS_FORWARDED'] = new ns.SelectElement(7, 'Forwarded');
        ns.ListElements['PENDING_CONFIRMATION'] = new ns.SelectElement(8, 'Pending confirmation');
        ns.ListElements['WRONGLFY_QUALIFIED'] = new ns.SelectElement(9, 'Wrongly qualified');
        ns.ListElements['NOT_APPLICABLE'] = new ns.SelectElement(10, 'Not applicable');
        ns.ListElements['DEACTIVATED'] = new ns.SelectElement(11, 'Deactivated');
        ns.ListElements['DISABLED'] = new ns.SelectElement(12, 'Disabled');


        // Element for validation status dropdown list
        ns.ListElements['NOT_PROCESSED'] = new ns.SelectElement(1, 'Not submitted');
        ns.ListElements['PROCESSED'] = new ns.SelectElement(2, 'Submitted');
        ns.ListElements['REJECTED'] = new ns.SelectElement(3, 'Rejected QC');
        ns.ListElements['VALIDATED'] = new ns.SelectElement(4, 'Validated QC');

    };

    // Return true whether the value provided in paramater is a status or not
    ns.isAtomicCheckStatus = function (statusValue) {
        for (var key in this.ListElements) {
            if (this.ListElements[key].elementValue == statusValue)
                return true;
        }
        return false;
    };

    // Return true whether the value provided in paramater is a status or not
    ns.isQualifierStatus = function (statusValue) {
        if ("TO QUALIFY" == statusValue || "WRONGLY QUALIFIED" == statusValue) {
            return true;
        } else {
            return false;
        }
    };

    ns.isReportStatus = function (statusValue) {
        if ("Submitted" == statusValue || "Not Submitted" == statusValue) {
            return true;
        } else {
            return false;
        }
    };

    ns.isWronglyQualified = function (statusValue) {
        return "WRONGLY QUALIFIED" == statusValue;
    };

    // Return true whether the value provided in paramater is a status or not
    ns.isScreeningStatus = function (statusValue) {
        var screeningStatus = ['New','Open','Validated','Submitted','Updating','Deactivated'];
        for (var elem in screeningStatus) {
            if (screeningStatus[elem] == statusValue)
                return true;
        }
        return false;
    };




    ns.initialize = function () {

        ns.statusDropDownList = $('#Status_PostData');
        ns.validationStatusDropDownList = $('#ValidationStatus_PostData');
        ns.defaultValidationStatusValue = ns.validationStatusDropDownList.val();
        ns.defaultStatusValue = ns.statusDropDownList.val();
        ns.typeOfCheck = $('#TypeOfCheck').val();

        // Element for status dropdown list
        ns.buildStatusAsString();

        ns.buildStatusList();
        ns.buildValidationStatusList();

        // Event handler when atomic check status is changed
        ns.statusDropDownList.change(function () {
            var atomicCheckState = new ns.AtomicCheckState();
            switch ($(this).val()) {
                case '2': // On going
                    atomicCheckState.onGoingClick();
                    break;
                case '3': // Done OK
                    atomicCheckState.doneOkClick();
                    break;
                case '4': // Done KO
                    atomicCheckState.doneKoClick();
                    break;
                case '5': // Done discrepancy
                    atomicCheckState.doneDiscrepancy();
                    break;
                case '6': // Done impossible
                    atomicCheckState.doneImpossible();
                    break;
                case '7': // On process forwarded
                    atomicCheckState.onProcessForwarded();
                    break;
                case '8': // Pending confirmation
                    atomicCheckState.pendingConfirmation();
                    break;
                case '9': // Wrongly qualified
                    atomicCheckState.wronglyQualified();
                    break;
            }
        });

        // Event handler when atomic check validation status is changed
        ns.validationStatusDropDownList.change(function () {
            var atomicCheckValidationState = new ns.AtomicCheckValidationState();
            switch ($(this).val()) {
                case '1': // Not processed
                    atomicCheckValidationState.notProcessedClick();
                    break;
                case '2': // Processed
                    atomicCheckValidationState.processedClick();
                    break;
                case '3': // Rejected
                    atomicCheckValidationState.rejectedClick();
                    break;
                case '4': // Validated
                    atomicCheckValidationState.validatedClick();
                    break;
            }
        });

    };


})();

