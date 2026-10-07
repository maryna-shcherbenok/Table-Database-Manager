import io
import json
import os

from flask import (
    Flask,
    flash,
    jsonify,
    redirect,
    render_template,
    request,
    send_file,
    url_for
)

from models import Database
from state import get_manager


app = Flask(__name__)

app.secret_key = os.environ.get(
    "SECRET_KEY",
    "lab1-it-stage3-secret-key"
)


DATA_TYPES = [
    "integer",
    "real",
    "char",
    "string",
    "charInvl",
    "string(charInvl)"
]


@app.get("/")
def index():
    manager = get_manager()

    database = manager.current_database
    active_table = None

    if database is not None:
        requested_table = request.args.get("table")

        if requested_table:
            active_table = database.get_table(
                requested_table
            )

        if active_table is None and database.tables:
            active_table = database.tables[0]

    table_columns = {}

    if database is not None:
        table_columns = {
            table.name: [
                column.name
                for column in table.columns
            ]
            for table in database.tables
        }

    return render_template(
        "index.html",
        database=database,
        active_table=active_table,
        data_types=DATA_TYPES,
        table_columns=table_columns
    )


@app.post("/database/create")
def create_database():
    manager = get_manager()

    try:
        name = request.form.get("name", "")

        manager.create_database(name)

        flash(
            "Database created successfully.",
            "success"
        )

    except ValueError as error:
        flash(str(error), "error")

    return redirect(
        url_for("index")
    )


@app.post("/database/open")
def open_database():
    manager = get_manager()

    try:
        uploaded_file = request.files.get(
            "database_file"
        )

        if (
            uploaded_file is None
            or uploaded_file.filename == ""
        ):
            raise ValueError(
                "Choose a JSON file."
            )

        data = json.load(
            uploaded_file.stream
        )

        database = Database.from_dict(data)

        manager.set_database(database)

        flash(
            "Database opened successfully.",
            "success"
        )

    except (
        ValueError,
        json.JSONDecodeError,
        UnicodeDecodeError
    ) as error:

        flash(
            f"Cannot open database: {error}",
            "error"
        )

    return redirect(
        url_for("index")
    )


@app.get("/database/save")
def save_database():
    manager = get_manager()

    try:
        database = manager.current_database

        if database is None:
            raise ValueError(
                "There is no database to save."
            )

        json_text = json.dumps(
            database.to_dict(),
            ensure_ascii=False,
            indent=2
        )

        memory_file = io.BytesIO(
            json_text.encode("utf-8")
        )

        filename = f"{database.name}.json"

        return send_file(
            memory_file,
            mimetype="application/json",
            as_attachment=True,
            download_name=filename
        )

    except ValueError as error:
        flash(str(error), "error")

        return redirect(
            url_for("index")
        )


@app.post("/table/add")
def add_table():
    manager = get_manager()

    try:
        table = manager.create_table(
            request.form.get(
                "table_name",
                ""
            )
        )

        flash(
            f"Table '{table.name}' created.",
            "success"
        )

        return redirect(
            url_for(
                "index",
                table=table.name
            )
        )

    except ValueError as error:
        flash(str(error), "error")

        return redirect(
            url_for("index")
        )


@app.post("/table/delete")
def delete_table():
    manager = get_manager()

    table_name = request.form.get(
        "table_name",
        ""
    )

    try:
        manager.delete_table(table_name)

        flash(
            f"Table '{table_name}' deleted.",
            "success"
        )

    except ValueError as error:
        flash(str(error), "error")

    return redirect(
        url_for("index")
    )


@app.post("/column/add")
def add_column():
    manager = get_manager()

    table_name = request.form.get(
        "table_name",
        ""
    )

    try:
        type_name = request.form.get(
            "type_name",
            ""
        )

        lower_bound = request.form.get(
            "lower_bound"
        )

        upper_bound = request.form.get(
            "upper_bound"
        )

        if type_name not in (
            "charInvl",
            "string(charInvl)"
        ):
            lower_bound = None
            upper_bound = None

        manager.create_column(
            table_name=table_name,
            column_name=request.form.get(
                "column_name",
                ""
            ),
            type_name=type_name,
            lower_bound=lower_bound,
            upper_bound=upper_bound
        )

        flash(
            "Column created successfully.",
            "success"
        )

    except ValueError as error:
        flash(str(error), "error")

    return redirect(
        url_for(
            "index",
            table=table_name
        )
    )


@app.post("/column/delete")
def delete_column():
    manager = get_manager()

    table_name = request.form.get(
        "table_name",
        ""
    )

    try:
        manager.delete_column(
            table_name=table_name,
            column_name=request.form.get(
                "column_name",
                ""
            )
        )

        flash(
            "Column deleted successfully.",
            "success"
        )

    except ValueError as error:
        flash(str(error), "error")

    return redirect(
        url_for(
            "index",
            table=table_name
        )
    )


@app.post("/row/add")
def add_row():
    manager = get_manager()

    table_name = request.form.get(
        "table_name",
        ""
    )

    try:
        manager.add_row(table_name)

        flash(
            "Row added successfully.",
            "success"
        )

    except ValueError as error:
        flash(str(error), "error")

    return redirect(
        url_for(
            "index",
            table=table_name
        )
    )


@app.post("/row/delete")
def delete_row():
    manager = get_manager()

    table_name = request.form.get(
        "table_name",
        ""
    )

    try:
        row_index = int(
            request.form.get(
                "row_index",
                "-1"
            )
        )

        manager.delete_row(
            table_name,
            row_index
        )

        flash(
            "Row deleted successfully.",
            "success"
        )

    except (ValueError, TypeError) as error:
        flash(str(error), "error")

    return redirect(
        url_for(
            "index",
            table=table_name
        )
    )


@app.post("/cell/edit")
def edit_cell():
    manager = get_manager()

    try:
        table_name = request.form.get(
            "table_name",
            ""
        )

        row_index = int(
            request.form.get(
                "row_index",
                "-1"
            )
        )

        column_name = request.form.get(
            "column_name",
            ""
        )

        value = request.form.get(
            "value",
            ""
        )

        manager.edit_cell(
            table_name=table_name,
            row_index=row_index,
            column_name=column_name,
            value=value
        )

        return jsonify({
            "success": True,
            "message": "Cell updated."
        })

    except (ValueError, TypeError) as error:
        return jsonify({
            "success": False,
            "message": str(error)
        }), 400


@app.post("/join")
def join_tables():
    manager = get_manager()

    try:
        first_table = request.form.get(
            "first_table",
            ""
        )

        second_table = request.form.get(
            "second_table",
            ""
        )

        common_field = request.form.get(
            "common_field",
            ""
        )

        if not common_field:
            raise ValueError(
                "Choose a common field."
            )

        result = manager.join_tables(
            first_table,
            second_table,
            common_field
        )

        result = manager.add_join_result(
            result
        )

        flash(
            f"JOIN completed. "
            f"Result table: {result.name}",
            "success"
        )

        return redirect(
            url_for(
                "index",
                table=result.name
            )
        )

    except ValueError as error:
        flash(str(error), "error")

        return redirect(
            url_for("index")
        )


if __name__ == "__main__":
    app.run(debug=True)