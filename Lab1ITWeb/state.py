import secrets

from flask import session

from manager import Manager


_managers: dict[str, Manager] = {}


def get_manager() -> Manager:
    session_id = session.get("database_session_id")

    if not session_id:
        session_id = secrets.token_hex(16)
        session["database_session_id"] = session_id

    if session_id not in _managers:
        _managers[session_id] = Manager()

    return _managers[session_id]
