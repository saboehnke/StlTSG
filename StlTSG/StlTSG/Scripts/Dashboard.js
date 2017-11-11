/************************* ↓ User Info Filtering ↓ *************************/

$(document).ready(function ()
{
    $('.filterable .btn-filter').click(function ()
    {
        var $panel = $(this).parents('.filterable'),
        $filters = $panel.find('.filters input'),
        $tbody = $panel.find('.table tbody');
        if ($filters.prop('disabled') === true)
        {
            $filters.prop('disabled', false);
            $filters.first().focus();
        }
        else
        {
            $filters.val('').prop('disabled', true);
            $tbody.find('.no-result').remove();
            $tbody.find('tr').show();
        }
    });

    $('.filterable .filters input').keyup(function (e)
    {
        /* Ignore tab key */
        var code = e.keyCode || e.which;
        if (code === '9') return;
        /* Useful DOM data and selectors */
        var $input = $(this),
        inputContent = $input.val().toLowerCase(),
        $panel = $input.parents('.filterable'),
        column = $panel.find('.filters th').index($input.parents('th')),
        $table = $panel.find('.table'),
        $rows = $table.find('tbody tr');
        /* Dirtiest filter function ever ;) */
        var $filteredRows = $rows.filter(function ()
        {
            var value = $(this).find('td').eq(column).text().toLowerCase();
            return value.indexOf(inputContent) === -1;
        });
        /* Clean previous no-result if exist */
        $table.find('tbody .no-result').remove();
        /* Show all rows, hide filtered ones (never do that outside of a demo ! xD) */
        $rows.show();
        $filteredRows.hide();
        /* Prepend no-result row if all rows are filtered */
        if ($filteredRows.length === $rows.length)
        {
            $table.find('tbody').prepend($('<tr class="no-result text-center"><td colspan="' + $table.find('.filters th').length + '">No result found</td></tr>'));
        }
    });

    $('.btn-update').click(function ()
    {
        $('.input-filters input').each(function() 
        {
            if ($(this).attr("id") === "filter-input")
            {
                if ($(this).attr('disabled'))
                    $(this).removeAttr('disabled');
                else $(this).attr({ 'disabled': 'disabled' });
            }
        })
    });
});

function UpdateAmountOwed(id, amount)
{
    var url = 'SaveOwedAmount';
    var customerData = { ID: id, Amount: amount };
    $.ajax({
        type: 'POST',
        url: url,
        data: customerData,
        dataType: 'json',
        success: function (a)
        {
            alert("Success");
        }
    });
}

/************************* ↑ User Info Filtering ↑ *************************/

/************************* ↓     Date Range     ↓ *************************/

//$(function ()
//{
//    'use strict';
//    $('#dateRange').DatePicker(
//    {
//        type: 'rangedate',
//        startDate: moment().subtract(1, 'week'),
//        endDate: moment(),
//        ranges: [
//        {
//            label: "Yesterday",
//            startDate: moment().subtract(1, 'day'),
//            endDate: moment().subtract(1, 'day')
//        },
//        {
//            label: 'Sunday',
//            startDate: moment().startOf('week'),
//            endDate: moment()
//        },
//        {
//            label: '2 Weeks',
//            startDate: moment().startOf('week').subtract(1, 'week'),
//            endDate: moment()
//        },
//        {
//            label: 'This Month',
//            startDate: moment().startOf('month'),
//            endDate: moment()
//        },
//        {
//            label: 'Last Month',
//            startDate: moment().startOf('month').subtract(1, 'month'),
//            endDate: moment().startOf('month')
//        },
//        {
//            label: 'This Year',
//            startDate: moment().startOf('year'),
//            endDate: moment().startOf('moth')
//        }]
//    });
//});

/************************* ↑     Date Range     ↑ *************************/

/************************* ↓ Dashboard Line Chart ↓ *************************/

var monthlyData = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

var canvas = document.getElementById('lineChart');
var data =
{
    labels: ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"],
    datasets: [
        {
            label: "Appointments",
            fill: false,
            lineTension: 0.1,
            backgroundColor: "rgba(66,139,202,0.4)",
            borderColor: "rgba(66,139,202,1)",
            borderCapStyle: 'butt',
            borderDash: [],
            borderDashOffset: 0.0,
            borderJoinStyle: 'miter',
            pointBorderColor: "rgba(66,139,202,1)",
            pointBackgroundColor: "#fff",
            pointBorderWidth: 1,
            pointHoverRadius: 5,
            pointHoverBackgroundColor: "rgba(66,139,202,1)",
            pointHoverBorderColor: "rgba(66,139,202,1)",
            pointHoverBorderWidth: 2,
            pointRadius: 1,
            pointHitRadius: 10,
            data: monthlyData,
            spanGaps: false,
        }
    ]
};


var lineChart = Chart.Line(canvas,
{
    data: data
});

var ChangeChartYear = function ()
{
    var yearDD = document.getElementById('yearDropdown');
    var chosenYear = yearDD.options[yearDD.selectedIndex].text;

    monthlyData = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
    
    modelData.forEach(function (m)
    {
        var date = moment(m.Date);
        var month = date.format('M');
        var year = date.format('YYYY');

        if (year === chosenYear)
            monthlyData[month] += m.NumberOfUsers;
    });

    lineChart.data.datasets[0].data = monthlyData;
    lineChart.update();
}

/************************* ↑ Dashboard Line Chart ↑ *************************/