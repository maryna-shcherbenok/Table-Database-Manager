document.addEventListener(
    "DOMContentLoaded",
    () => {
        setupColumnType();
        setupJoinSelectors();
        setupCellEditing();
    }
);


/* ========================================================= */
/* COLUMN TYPE / INTERVAL BOUNDS                             */
/* ========================================================= */

function setupColumnType() {

    const typeSelect =
        document.getElementById("column-type");

    const boundsPanel =
        document.getElementById("bounds-panel");


    if (!typeSelect || !boundsPanel) {
        return;
    }


    function updateBounds() {

        const requiresBounds =
            typeSelect.value === "charInvl"
            ||
            typeSelect.value === "string(charInvl)";


        boundsPanel.classList.toggle(
            "hidden",
            !requiresBounds
        );


        boundsPanel
            .querySelectorAll("input")
            .forEach(input => {
                input.required = requiresBounds;
            });
    }


    typeSelect.addEventListener(
        "change",
        updateBounds
    );


    updateBounds();
}


/* ========================================================= */
/* JOIN SELECTORS                                            */
/* ========================================================= */

function setupJoinSelectors() {

    const first =
        document.getElementById(
            "join-first-table"
        );

    const second =
        document.getElementById(
            "join-second-table"
        );

    const field =
        document.getElementById(
            "join-common-field"
        );


    if (!first || !second || !field) {
        return;
    }


    const columns =
        window.TABLE_COLUMNS || {};


    function updateFields() {

        const firstColumns =
            columns[first.value] || [];

        const secondColumns =
            columns[second.value] || [];


        const secondNames =
            new Set(
                secondColumns.map(
                    name => name.toLowerCase()
                )
            );


        const common =
            firstColumns.filter(
                name =>
                    secondNames.has(
                        name.toLowerCase()
                    )
            );


        field.innerHTML = "";


        if (common.length === 0) {

            const option =
                document.createElement(
                    "option"
                );

            option.value = "";

            option.textContent =
                "No common fields";

            field.appendChild(option);

            return;
        }


        common.forEach(name => {

            const option =
                document.createElement(
                    "option"
                );

            option.value = name;
            option.textContent = name;

            field.appendChild(option);
        });
    }


    if (
        first.options.length > 1
        &&
        first.value === second.value
    ) {
        second.selectedIndex = 1;
    }


    first.addEventListener(
        "change",
        updateFields
    );

    second.addEventListener(
        "change",
        updateFields
    );


    updateFields();
}


/* ========================================================= */
/* CELL EDITING                                              */
/* ========================================================= */

function setupCellEditing() {

    document
        .querySelectorAll(".cell-input")
        .forEach(input => {

            input.dataset.oldValue =
                input.value;


            input.addEventListener(
                "focus",
                () => {

                    input.dataset.oldValue =
                        input.value;
                }
            );


            input.addEventListener(
                "change",
                async () => {

                    await saveCell(input);
                }
            );

        });
}


/* ========================================================= */
/* SAVE CELL                                                 */
/* ========================================================= */

async function saveCell(input) {

    const oldValue =
        input.dataset.oldValue || "";


    const formData =
        new FormData();


    formData.append(
        "table_name",
        input.dataset.table
    );

    formData.append(
        "row_index",
        input.dataset.row
    );

    formData.append(
        "column_name",
        input.dataset.column
    );

    formData.append(
        "value",
        input.value
    );


    try {

        const response =
            await fetch(
                "/cell/edit",
                {
                    method: "POST",
                    body: formData
                }
            );


        const result =
            await response.json();


        if (!response.ok) {

            input.value =
                oldValue;

            input.classList.add(
                "invalid"
            );


            showMessage(
                result.message,
                true
            );


            setTimeout(
                () => {

                    input.classList.remove(
                        "invalid"
                    );

                },
                1800
            );

            return;
        }


        input.dataset.oldValue =
            input.value;


        showMessage(
            "Cell saved.",
            false
        );

    }
    catch {

        input.value =
            oldValue;


        showMessage(
            "Cannot save cell.",
            true
        );
    }
}


/* ========================================================= */
/* SMALL MESSAGE                                             */
/* ========================================================= */

function showMessage(
    text,
    isError
) {

    const message =
        document.getElementById(
            "ajax-message"
        );


    if (!message) {
        return;
    }


    message.textContent =
        text;


    message.classList.toggle(
        "error",
        isError
    );


    message.classList.add(
        "visible"
    );


    clearTimeout(
        window.messageTimer
    );


    window.messageTimer =
        setTimeout(
            () => {

                message.classList.remove(
                    "visible"
                );

            },
            1800
        );
}
