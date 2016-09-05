#!/usr/bin/env python3

import argparse
import sqlite3
import sys
import pprint

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


def remove_elements(list_of_elements, elements_to_remove):
    """Returns a new list with list_of_elements without elements_to_remove"""
    result = []

    for element in list_of_elements:
        if element not in elements_to_remove:
            result.append(element)

    return result


def add_prefix(list_of_elements, prefix):
    result = []

    for element in list_of_elements:
        result.append("{}{}".format(prefix, element))

    return result


def insert_data(database, table_name, data, matches_columns):
    """ Inserts data (list of dictionaries) into table_name for database.
    Modifies data and adds new_unique_id. This is the new uniqueid or an
    existing one if a register with matches_columns already existed. """
    cursor = database.cursor()

    for row in data:
        # First check if this already existed

        where = ""
        if len(matches_columns) == 0:
            where = "1=1"
        else:
            for column in matches_columns:
                if where != "":
                    where += " AND "
                where += "{} = '{}'".format(column, row[column])

        format_data = {}
        format_data['table_name'] = table_name
        format_data['where_clause'] = " WHERE {}".format(where)
        sql = "SELECT uniqueId FROM {table_name} {where_clause}".format(**format_data)
        execute_and_log(cursor, sql)

        results = cursor.fetchall()

        if len(results) == 0:
            # Needs to insert
            sql = create_insert_dictionary(table_name, row)
            execute_and_log(cursor, sql)
            new_id = cursor.lastrowid
            row['new_unique_id'] = new_id
        else:
            # Returns uniqueid
            row['new_unique_id'] = results[0][0]

    database.commit()


def return_data_from_table(database, table_name, where_condition, skip_columns, join_clause =""):
    """ Returns a list of lists of the database, table executing where and skips the columns. """
    cursor = database.cursor()

    column_names = get_column_names(database, table_name)

    column_names = remove_elements(column_names, skip_columns)

    column_names_with_prefixes = add_prefix(column_names, "{}.".format(table_name))

    where_condition = " WHERE {} ".format(where_condition)

    format_data = {"column_names": ",".join(column_names_with_prefixes), "table_name": table_name, "join_clause": join_clause, "where": where_condition}

    sql = "SELECT {column_names} FROM {table_name} {join_clause} {where}".format(**format_data)
    execute_and_log(cursor, sql)

    results = cursor.fetchall()

    data = []
    for row in results:
        r = {}
        for i, col in enumerate(row):
            r[column_names[i]] = col

        data.append(r)

    return data

def find_jump_types(sessionID, source_db, table):
    """ Returns jumpTypes rows (without the uniqueID) used by sessionID"""
    source_cursor = source_db.cursor()

    column_names = get_column_names(source_db, table)

    i = 0
    while i < len(column_names):
        column_names[i] = table+"." + column_names[i]
        i += 1

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

        execute_and_log(cursor, sql)
        result = cursor.fetchall()

        if len(result) == 0:
            values = "("
            for col in row:
                if values != "(":
                    values += ","
                values += '"' + str(col) + '"'

            values += ")"

            sql = "insert into " + table + " (" + ",".join(column_names) + ") VALUES " + values
            execute_and_log(cursor, sql)
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


def create_insert_dictionary(table_name, row):
    values = "("
    column_names = []
    for column_name in row.keys():
        if values != "(":
            values += ","
        values += '"' + str(row[column_name]) + '"' # TODO fix escaping here!
        column_names.append(column_name)

    values += ")"

    sql = "INSERT INTO " + table_name + " (" + ",".join(column_names) + ") VALUES " + values

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
    """ TODO: it doesn't work it doesn't know the source session id"""
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


def import_database(source_path, destination_path, source_session):
    """ Imports the session source_session from source_db into destination_db """

    source_db = open_database(source_path, read_only=True)
    destination_db = open_database(destination_path, read_only=False)

    jump_types = return_data_from_table(database=source_db, table_name="JumpType",
                                        where_condition="Session.uniqueID={}".format(source_session),
                                        skip_columns=["uniqueID"],
                                        join_clause="LEFT JOIN Jump ON JumpType.name=Jump.type LEFT JOIN Session ON Jump.sessionID=Session.uniqueID")

    insert_data(database=destination_db, table_name="JumpType", data=jump_types,
                matches_columns=["name", "startIn", "weight", "description"])

    jump_rj_types = return_data_from_table(database=source_db, table_name="JumpRjType",
                                           where_condition="Session.uniqueID={}".format(source_session),
                                           skip_columns=["uniqueID"],
                                           join_clause="LEFT JOIN JumpRj ON JumpRjType.name=JumpRj.type LEFT JOIN Session on JumpRj.sessionID=Session.uniqueID")

    insert_data(database=destination_db, table_name="JumpRjType", data=jump_rj_types,
                matches_columns=["name", "startIn", "weight", "jumpsLimited", "fixedValue", "description"])


    session = return_data_from_table(database=source_db, table_name="Session",
                           where_condition="Session.uniqueID={}".format(source_session),
                           skip_columns=["uniqueID"])

    insert_data(database=destination_db, table_name="Session", data=session,
                              matches_columns=["name", "place", "date", "personsSportID", "personsSpeciallityID", "personsPractice", "comments"])

    new_session_id = session[0]['new_unique_id']

    # new_session_id = import_session(source_db, destination_db, source_session)
    print("Imported sessionId:", new_session_id)

    ### Continue from here

    # import_person_session_77(source_db, destination_db, source_session, new_session_id)

    # new_jump_rj_ids = import_jump_rj(source_db, destination_db, source_session, new_session_id)
    # print("new_jump_rj_ids:", new_jump_rj_ids)

    # import_reaction_time(source_db, destination_db, new_session_id)

    destination_db.commit()

    destination_db.close()


def open_database(filename, read_only):
    """Opens the database specified by filename. If read_only is True
    the database cannot be changed.
    """
    if read_only:
        mode = "ro"
    else:
        mode = "rw"

    uri = "file:{}?mode={}".format(filename,mode)
    conn = sqlite3.connect(uri, uri=True)

    conn.execute("pragma foreign_keys=ON")

    return conn

def execute_and_log(cursor, sql, comment = ""):
    print("Will execute:", sql, comment)
    cursor.execute(sql)

def process_command_line():
    parser = argparse.ArgumentParser(description="Process some integers.")
    parser.add_argument("--source", type=str, required=True,
                        help="chronojump.sqlite that we are importing from")
    parser.add_argument("--destination", type=str, required=True,
                        help="chronojump.sqlite that we import to")
    parser.add_argument("--source_session", type=int, required=True,
                        help="Session from source that will be imported to a new session in destination")
    args = parser.parse_args()

    source_session = args.source_session

    import_database(args.source, args.destination, source_session)


if __name__ == "__main__":
    process_command_line()
