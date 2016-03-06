
function OnOpenMultiSelect(context, controller, actionRead, secondaryId) {
    var multiSelect = $('#' + context + '_MultiSelectKendoUiViewModel_SelectedItems').data('kendoMultiSelect');
    var urlString = '/' + controller + '/' + actionRead;
    if (secondaryId !== null) {
        urlString += '?id=' + secondaryId;
    }
    var kendoDataSource = new kendo.data.DataSource({
        transport: {
            read: {
                url: urlString,
                dataType: "json" // "jsonp" is required for cross-domain requests; use "json" for same-domain requests
            }
        }
    });
    var editedDataSources;
    kendoDataSource.fetch(function () {
        var data = this.data();
        console.info(data.length);
        editedDataSources = filterScreeningQualification(this, context);
        multiSelect.setDataSource(editedDataSources);
        multiSelect.refresh();
    });
}

function getSecondaryId(context) {
    if (context === 'Faculty') {
        return $('#Faculty_DropDownListKendoUiViewModel_SelectedItem').data('kendoDropDownList').value();
    }
    if (context === 'CertificationPlace') {
        return $('#CertificationPlace_DropDownListKendoUiViewModel_SelectedItem').data('kendoDropDownList').value();
    }
    return null;
}

function getContextName(controller, action) {

    var allowedControllers = ['CertificationPlace', 'Company', 'Court',
    'DriingLicenseOffice', 'HighSchool', 'ImmigrationOffice', 'PoliceController',
    'ProfessionalQualification', 'University'];

    if (!$.inArray(controller, allowedControllers)) {
        return null;
    }

    if (controller === 'Court') {
        return action.replace("Get", "");
    }
    if (controller === 'University')
        return 'Faculty';

    return controller;
}

function filterScreeningQualification(dataSources, controllerName) {
    var alreadyChosenItems = getListOfChosenQualificationPlaces(controllerName);
    if (alreadyChosenItems !== null) {
        return removeAlreadyChosenItemsFromDataSources(dataSources, alreadyChosenItems);
    }
    return null;
}

function removeAlreadyChosenItemsFromDataSources(dataSources, alreadyChosenItems) {
    var i, j, tempItem, stringObject;
    for (j = 0; j < alreadyChosenItems.length; j++) {
        var rawDataSources = dataSources.data();
        for (i = rawDataSources.length - 1; i >= 0; i--) {
            tempItem = rawDataSources[i];
            stringObject = JSON.stringify(rawDataSources[i]);
            var currentChosenItem = alreadyChosenItems[j];
            //check json string of datasource
            if (stringObject.indexOf(currentChosenItem) != -1) {
                dataSources.remove(tempItem);
            }
        }
    }
    return dataSources;
}

function getListOfChosenQualificationPlaces(controllerName) {
    var textArea = $('#' + controllerName + '_AlreadyQualified');
    if (textArea.length !== 0) {
        textArea = textArea[0];
        return textArea.textContent.split(", ");
    }
    return null;
}
