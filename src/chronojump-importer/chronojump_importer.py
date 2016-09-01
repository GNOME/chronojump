#!/usr/bin/env python3

import argparse
import sqlite3
import sys


def results_delete_column(column, results):
    new_results = []

    for row in results:
        new_results.append(list(row[column+1:]))

    return new_results


def get_column_names(db, table):
    cursor = db.cursor()

    cursor.execute("PRAGMA table_info({})".format(table))
    result = cursor.fetchall()

    names = []

    for row in result:
       names.append(row[1])

    return names


def find_jump_types(sessionID, source_db, table):
    """ Returns jumpTypes rows (without the uniqueID) needed by sessionID"""
    source_cursor = source_db.cursor()

    column_names = get_column_names(source_db, table)

    i = 0
    while i < len(column_names):
        column_names[i] = table+"." + column_names[i]
        i+=1

    if table == "JumpType":
        secondary_table = "Jump"
    elif table == "JumpRjType":
        secondary_table = "JumpRj"
    else:
        assert False

    result = source_cursor.execute(("SELECT {} FROM " + table + " LEFT JOIN " + secondary_table + " ON "+table+".name="+secondary_table+".type LEFT JOIN Session ON "+secondary_table+".sessionID=Session.uniqueID WHERE Session.uniqueID={}").format(",".join(column_names), sessionID))

    results = result.fetchall()
    jump_types = results_delete_column(0, results)

    return jump_types


def ids_from_data(db, table, rows):
    """ Returns a list of ids in table. Inserts it if necessary. """
    ids = []

    cursor = db.cursor()

    column_names = get_column_names(db, table)
    column_names = column_names[1:]

    for row in rows:
        where = ""
        for idx, column_name in enumerate(column_names):
            if where != "":
                where += " AND "

            where += column_name + "=\"" + str(row[idx]) + "\""

        sql = "select uniqueID from " + table + " where " + where
        print("Will check:", sql)

        cursor.execute(sql)
        result = cursor.fetchall()

        if len(result) == 0:
            values = "("
            for col in row:
                if values != "(":
                    values += ","
                values += '"' + str(col) + '"'

            values += ")"

            sql = "insert into " + table + " (" + ",".join(column_names) + ") VALUES " + values
            print("SQL insert:", sql)
            cursor.execute(sql)
            newid = cursor.lastrowid

        else:
            print("Not inserting because it already existed")
            newid = result[0][0]

        if newid not in ids:
            ids.append(newid)

    db.commit()
    return ids


def create_select(table_name, column_names, where):
    sql = "SELECT " + ",".join(column_names) + " FROM " + table_name + " WHERE " + where

    return sql


def create_insert(table_name, column_names, row):
    values = "("
    for data in row:
        if values != "(":
            values += ","
        values += '"' + str(data) + '"' # TODO fix escaping here!
    values += ")"

    sql = "INSERT INTO " + table_name + " (" + ",".join(column_names) + ") VALUES " + values

    return sql


def import_table_with_where(source_db, destination_db, table_name, autoincrement_column_name, where):
    column_names = get_column_names(source_db, table_name)

    column_names.remove(autoincrement_column_name)

    sql_select = create_select(table_name, column_names, where)

    source_cursor = source_db.cursor()
    destination_cursor = destination_db.cursor()

    source_cursor.execute(sql_select)

    result = source_cursor.fetchall()

    new_ids = []

    for row in result:
        sql_insert = create_insert(table_name, column_names, row)
        destination_cursor.execute(sql_insert)

        new_id = destination_cursor.lastrowid

        new_ids.append(new_id)

    return new_ids


def import_session(source_db, destination_db, source_session):
    """ Imports souce_session from source_d~/.local/share/Chronojump/database/chronojump.dbb into destination_db. Returns the session_id"""
    ids = import_table_with_where(source_db, destination_db, "Session", "uniqueID", "uniqueID={}".format(source_session))

    assert len(ids) == 1

    return ids[0]


def insert_person77(source_db, destination_db, person_id):
    column_names = get_column_names(source_db, "Person77")
    column_names = column_names[1:]

    source_cursor = source_db.cursor()
    select_sql = create_select("Person77", column_names, "uniqueId = {}".format(person_id))

    source_cursor.execute(select_sql)

    row = source_cursor.fetchall()[0]

    insert_sql = create_insert("Person77", column_names, row)

    destination_cursor = destination_db.cursor()

    destination_cursor.execute(insert_sql)

    person77_id = destination_cursor.lastrowid

    return person77_id


def get_person_id(source_db, destination_db, source_person_id):
    """ Returns the personId if it person_name already exists or creates one and returns the personId"""
    source_cursor = source_db.cursor()
    destination_cursor = destination_db.cursor()

    sql_select = "SELECT name FROM Person77 WHERE uniqueID = {}".format(source_person_id)
    source_cursor.execute(sql_select)

    results = source_cursor.fetchall()

    assert results
    assert len(results) > 0
    assert len(results[0]) > 0

    person_name = results[0][0]

    print("Person name to look for:", person_name)

    sql_select = "SELECT * FROM Person77 WHERE name = '{}'".format(person_name)
    destination_cursor.execute(sql_select)

    result = destination_cursor.fetchall()

    if len(result) == 0:
        return insert_person77(source_db, destination_db, source_person_id)
    else:
        return result[0][0]


def import_jump_rj(source_db, destination_db, source_session, new_session_id):
    source_cursor = source_db.cursor()
    destination_cursor = destination_db.cursor()

    column_names = get_column_names(source_db, "JumpRj")
    column_names = column_names[1:]

    source_cursor.execute("SELECT " + ",".join(column_names) + " FROM JumpRJ WHERE sessionID = {}".format(source_session))

    results = source_cursor.fetchall()

    new_ids = []

    for row in results:
        new_row = list(row)
        personId = row[1]
        new_person_id = get_person_id(source_db, destination_db, personId)
        new_row[0] = new_person_id

        sql_insert = create_insert("JumpRj",column_names, new_row)
        print("Executing:", sql_insert)
        destination_cursor.execute(sql_insert)

        new_id = destination_cursor.lastrowid

        new_ids.append(new_id)

    return new_ids


def import_person_session_77(source_db, destination_db, source_session, destination_session):
    source_cursor = source_db.cursor()

    person_session_77_columns = get_column_names(source_db, "PersonSession77")

    person_session_77_columns = person_session_77_columns[1:]

    source_cursor.execute("SELECT " + ",".join(person_session_77_columns) + " FROM PersonSession77 WHERE sessionID={}".format(source_session))
    results = source_cursor.fetchall()

    for row in results:
        new_row = list(row)
        new_person_id = get_person_id(source_db, destination_db, row[0])

        new_row[0] = new_person_id
        new_row[1] = destination_session

        insert_person_session_77(destination_db, row)


def import_reaction_time(source_db, destination_db, new_session_id):
    """ ... """
    source_cursor = source_db.cursor()
    destination_db = destination_db.cursor()

    columns = get_column_names(source_db, "ReactionTime")
    columns = columns[1:]
    sql = create_select("ReactionTime", columns, "SessionID={}".format(new_session_id))

    source_cursor.execute(sql)

    results = source_cursor.fetchall()

    for row in results:
        new_row = list(row)

        new_person_id = get_person_id(source_db, destination_db, new_row[0])

        new_row[0] = new_person_id

        create_insert("ReactionTime", columns, new_row)


def insert_person_session_77(destination_db, row):
    """ Inserts row into PersonSession77 and returns the uniqueID"""
    destination_cursor = destination_db.cursor()

    column_names = get_column_names(destination_db, "PersonSession77")
    column_names = column_names[1:]

    sql = create_insert("PersonSession77", column_names, row)

    destination_cursor.execute(sql)

    new_id = destination_cursor.lastrowid

    return new_id


def import_database(source_db, destination_db, source_session):
    jump_types = find_jump_types(source_session, source_db, "JumpType")
    ids_from_data(destination_db, "JumpType", jump_types)

    jump_rj_types = find_jump_types(source_session, source_db, "JumpRjType")
    ids_from_data(destination_db, "JumpRjType", jump_rj_types)

    new_session_id = import_session(source_db, destination_db, source_session)
    print("Imported sessionId:", new_session_id)

    import_person_session_77(source_db, destination_db, source_session, new_session_id)

    new_jump_rj_ids = import_jump_rj(source_db, destination_db, source_session, new_session_id)
    print("new_jump_rj_ids:", new_jump_rj_ids)

    import_reaction_time(source_db, destination_db, new_session_id)

    # import_session(source_db, destination_db, source_session)


def open_database(filename, read_only):
    if read_only:
        mode = "ro"
    else:
        mode = "rw"

    uri = "file:{}?mode={}".format(filename,mode)
    conn = sqlite3.connect(uri, uri=True)

    conn.execute("pragma foreign_keys=ON")

    return conn


def main():
    parser = argparse.ArgumentParser(description="Process some integers.")
    parser.add_argument("--source", type=str, required=True,
                        help="chronojump.sqlite that we are importing from")
    parser.add_argument("--destination", type=str, required=True,
                        help="chronojump.sqlite that we import to")
    parser.add_argument("--source_session", type=int, required=True,
                        help="Session from source that will be imported to a new session in destination")
    args = parser.parse_args()

    source_session = args.source_session
    source_db = open_database(args.source, True)
    destination_db = open_database(args.destination, False)

    import_database(source_db, destination_db, source_session)

    destination_db.commit()

    destination_db.close()


if __name__ == "__main__":
    main()
