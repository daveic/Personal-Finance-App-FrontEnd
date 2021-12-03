
    google.charts.load('current', {packages: ['corechart', 'line'] });
    google.charts.setOnLoadCallback(drawBasic);

    function drawBasic(model) {

        var data = new google.visualization.DataTable();
        data.addColumn('date', 'X');
        data.addColumn('number', 'Dogs');
        var n = 0;
        var items = model;
        var currentDate = items[1].TrsDateTime;

        var date = currentDate.getDate();
        var month = currentDate.getMonth(); //Be careful! January is 0 not 1
        var year = currentDate.getFullYear();
        /*
        for (i = 0; i < items.length; {
        data.addRows([
            [items[i].TrsDateTime, n + 1]
        ]);
[new Date(2012, 6, 13), 23]
        }

*/

        data.addRows([
            [new Date(year, month, date), 0], [new Date(2012, 6, 13), 23], [new Date(2012, 9, 13), 2]
        ]);

        var options = {
        hAxis: {
        title: 'Time'
            },
            vAxis: {
        title: 'Popularity'
            }
        };

        var chart = new google.visualization.LineChart(document.getElementById('chart_div'));

        chart.draw(data, options);
    }
