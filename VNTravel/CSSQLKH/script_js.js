document.addEventListener('DOMContentLoaded', function () {
    const search = document.querySelector('.search');
    const table_rows = document.querySelectorAll('tbody tr');
    const table_headings = document.querySelectorAll('thead th');

    // 1. Searching for specific data of HTML table
    search.addEventListener('input', function searchTable() {
        table_rows.forEach((row, i) => {
            let table_data = row.textContent.toLowerCase(),
                search_data = search.value.toLowerCase();

            row.classList.toggle('hide', table_data.indexOf(search_data) < 0);
            row.style.setProperty('--delay', i / 25 + 's');
        })

        document.querySelectorAll('tbody tr:not(.hide)').forEach((visible_row, i) => {
            visible_row.style.backgroundColor = (i % 2 == 0) ? 'transparent' : '#0000000b';
        });
    });

    // 2. Sorting | Ordering data of HTML table
    table_headings.forEach((head, i) => {
        let sort_asc = true;
        head.addEventListener('click', () => {
            table_headings.forEach(head => head.classList.remove('active'));
            head.classList.add('active');

            document.querySelectorAll('td').forEach(td => td.classList.remove('active'));
            table_rows.forEach(row => {
                row.querySelectorAll('td')[i].classList.add('active');
            })

            head.classList.toggle('asc', sort_asc);
            sort_asc = head.classList.contains('asc') ? false : true;

            sortTable(i, sort_asc);
        });
    });

    function sortTable(column, sort_asc) {
        [...table_rows].sort((a, b) => {
            let first_row = a.querySelectorAll('td')[column].textContent.toLowerCase(),
                second_row = b.querySelectorAll('td')[column].textContent.toLowerCase();

            return sort_asc ? (first_row < second_row ? 1 : -1) : (first_row < second_row ? -1 : 1);
        })
            .map(sorted_row => document.querySelector('tbody').appendChild(sorted_row));
    }
});

//export excel
document.addEventListener('DOMContentLoaded', function () {
    const excel_btn = document.querySelector('#toEXCEL');
    const customers_table = document.querySelector('#customers_table');

    const toExcel = function (table) {
        const t_heads = table.querySelectorAll('thead th'),
            tbody_rows = table.querySelectorAll('tbody tr');

        const headings = [...t_heads].map(head => {
            return head.textContent.trim().toLowerCase();
        }).join('\t');

        const table_data = [...tbody_rows].map(row => {
            const cells = row.querySelectorAll('td');
            const row_data = [...cells].map(cell => cell.textContent.trim()).join('\t');
            return row_data;
        }).join('\n');

        return headings + '\n' + table_data;
    }

    excel_btn.onclick = () => {
        const excel = toExcel(customers_table);
        downloadFile(excel, 'excel');
    }

    const downloadFile = function (data, fileType, fileName = '') {
        const a = document.createElement('a');
        a.download = fileName;
        const mime_types = {
            'json': 'application/json',
            'csv': 'text/csv',
            'excel': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        }
        a.href = `
        data:${mime_types[fileType]};charset=utf-8,${encodeURIComponent(data)}
    `;
        document.body.appendChild(a);
        a.click();
        a.remove();
    }
});




