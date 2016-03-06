
//To filter datasource and hide columns
function filterMatrixDataGrid(dataSourceParam) {
    $('#filterButton').bind('click', function() {
        var columnFilter = $('#ColumnFilter_SelectedItems').data('kendoMultiSelect');
        var colIds = columnFilter.value();
        $.ajax({
            url: '/Dispatching/FilterColumns',
            type: "POST",
            data: "{colIds:" + JSON.stringify(colIds) + "}",
            contentType: "application/json",
            success: function(dataRespond) {
                //set data source from respond
                var dataSource = dataSourceParam;
                dataSource.read();

                //get the grid from DOM
                var grid = $("#gridMatrix").data("kendoGrid");

                //set the new datasource
                grid.setDataSource(dataSource);

                //show all columns
                for (i = 0; i < grid.columns.length; i++) {
                    grid.showColumn(i);
                }

                //hide column to hide
                if (dataRespond.length == 0)
                    return;

                for (var i = 0; i < grid.columns.length; i++) {
                    if ($.inArray(i, dataRespond) > -1 && dataRespond[0] > -1)
                        grid.hideColumn(i);
                }
            }
        });
    });
}