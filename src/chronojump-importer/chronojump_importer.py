#!/usr/bin/env python3

import copy
import argparse
import sqlite3
import logging

import sys
import pprint

logging.basicConfig(level=logging.INFO)


def get_column_names(cursor, table, skip_columns = []):
    cursor.execute("PRAGMA table_info({})".format(table))
    result = cursor.fetchall()

    names = []

    for row in result:
        column_name = row[1]
        if column_name not in skip_columns:
            names.append(column_name)

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


def insert_data_into_table(cursor, table_name, data, matches_columns):
    """ Inserts data (list of dictionaries) into table_name for database.
    Returns a copy of data and adds new_unique_id. This is the new uniqueid or an
    existing one if a register with matches_columns already existed. """

    data_result = copy.deepcopy(data)

    for row in data_result:
        if type(matches_columns) == list:
            where = ""
            if len(matches_columns) == 0:
                where = "1=1"
            else:
                where_values = []
                for column in matches_columns:
                    if where != "":
                        where += " AND "
                    where += "{} = ?".format(column)
                    where_values.append(row[column])

            format_data = {}
            format_data['table_name'] = table_name
            format_data['where_clause'] = " WHERE {}".format(where)
            sql = "SELECT uniqueID FROM {table_name} {where_clause}".format(**format_data)
            cursor.execute(sql, where_values)

            results = cursor.fetchall()

        if matches_columns is None or len(results) == 0:
            # Needs to insert
            insert_dictionary_into_table(cursor, table_name, row)
            new_id = cursor.lastrowid
            row['new_unique_id'] = new_id
        else:
            # Returns uniqueid
            row['new_unique_id'] = results[0][0]

    return data_result


def get_data_from_table(cursor, table_name, where_condition, join_clause ="", group_by_clause=""):
    """ Returns a list of lists of the database, table executing where and skips the columns. """
    column_names = get_column_names(cursor, table_name)

    column_names_with_prefixes = add_prefix(column_names, "{}.".format(table_name))

    where_condition = " WHERE {} ".format(where_condition)

    if group_by_clause != "":
        group_by = " GROUP BY {}".format(group_by_clause)
    else:
        group_by = ""

    format_data = {"column_names": ",".join(column_names_with_prefixes), "table_name": table_name, "join_clause": join_clause, "where": where_condition, "group_by": group_by}

    sql = "SELECT {column_names} FROM {table_name} {join_clause} {where} {group_by}".format(**format_data)
    execute_and_log(cursor, sql)

    results = cursor.fetchall()

    data = []
    for row in results:
        r = {}
        for i, col in enumerate(row):
            r[column_names[i]] = col

        data.append(r)

    return data


def insert_dictionary_into_table(cursor, table_name, row, skip_columns=["uniqueID"]):
    values = []
    column_names = []
    place_holders = []
    for column_name in row.keys():
        if column_name in skip_columns:
            continue

        values.append(row[column_name])
        column_names.append(column_name)
        place_holders.append("?")

    sql = "INSERT INTO {table_name} ({column_names}) VALUES ({place_holders})".format(table_name=table_name,
                                                                                    column_names=",".join(column_names),
                                                                                    place_holders=",".join(place_holders))

    cursor.execute(sql, values)

    return sql


def update_persons77_ids(table, persons77_list):
    result = copy.deepcopy(table)

    for row in table:
        old_person_id = row['personID']
        for persons77 in persons77_list:
            if persons77['uniqueID'] == old_person_id:
                row['personID'] = persons77['new_unique_id']

    return result


def update_session_ids(table, new_session_id):
    result = copy.deepcopy(table)

    changed = False

    for row in table:
        row["sessionID"] = new_session_id
        changed = True
        break

    assert changed

    return result


def import_database(source_path, destination_path, source_session):
    """ Imports the session source_session from source_db into destination_db """

    logging.debug("source path:" + source_path)
    logging.debug("destination path:" + destination_path)

    source_db = open_database(source_path, read_only=True)
    destination_db = open_database(destination_path, read_only=False)

    source_cursor = source_db.cursor()
    destination_cursor = destination_db.cursor()

    # Imports JumpType table
    jump_types = get_data_from_table(cursor=source_cursor, table_name="JumpType",
                                     where_condition="Session.uniqueID={}".format(source_session),
                                     join_clause="LEFT JOIN Jump ON JumpType.name=Jump.type LEFT JOIN Session ON Jump.sessionID=Session.uniqueID",
                                     group_by_clause="JumpType.uniqueID")

    insert_data_into_table(cursor=destination_cursor, table_name="JumpType", data=jump_types,
                           matches_columns=get_column_names(destination_cursor, "JumpType", ["uniqueID"]))

    # Imports JumpRjType table
    jump_rj_types = get_data_from_table(cursor=source_cursor, table_name="JumpRjType",
                                        where_condition="Session.uniqueID={}".format(source_session),
                                        join_clause="LEFT JOIN JumpRj ON JumpRjType.name=JumpRj.type LEFT JOIN Session on JumpRj.sessionID=Session.uniqueID",
                                        group_by_clause="JumpRjType.uniqueID")

    insert_data_into_table(cursor=destination_cursor, table_name="JumpRjType", data=jump_rj_types,
                           matches_columns=get_column_names(destination_cursor, "JumpRjType", ["uniqueID"]))

    # Imports the session
    session = get_data_from_table(cursor=source_cursor, table_name="Session",
                                  where_condition="Session.uniqueID={}".format(source_session))

    session = insert_data_into_table(cursor=destination_cursor, table_name="Session", data=session,
                                     matches_columns=get_column_names(destination_cursor, "Session", ["uniqueID"]))

    new_session_id = session[0]['new_unique_id']

    # Imports Persons77 used by JumpRj table
    persons77_jump_rj = get_data_from_table(cursor=source_cursor, table_name="Person77",
                                            where_condition="JumpRj.sessionID={}".format(source_session),
                                            join_clause="LEFT JOIN JumpRj ON Person77.uniqueID=JumpRj.personID",
                                            group_by_clause="Person77.uniqueID")

    persons77_jump_rj = insert_data_into_table(cursor=destination_cursor, table_name="Person77", data=persons77_jump_rj,
                                               matches_columns=["name"])

    # Imports Person77 used by Jump table
    persons77_jump = get_data_from_table(cursor=source_cursor, table_name="Person77",
                                         where_condition="Jump.sessionID={}".format(source_session),
                                         join_clause="LEFT JOIN Jump ON Person77.uniqueID=Jump.personID",
                                         group_by_clause="Person77.uniqueID")

    persons77_jump = insert_data_into_table(cursor=destination_cursor, table_name="Person77", data=persons77_jump,
                                            matches_columns=["name"])

    persons77 = persons77_jump_rj + persons77_jump

    # Imports JumpRj table (with the new Person77's uniqueIDs)
    jump_rj = get_data_from_table(cursor=source_cursor, table_name="JumpRj",
                                  where_condition="JumpRj.sessionID={}".format(source_session))

    jump_rj = update_persons77_ids(jump_rj, persons77)
    jump_rj = update_session_ids(jump_rj, new_session_id)

    insert_data_into_table(cursor=destination_cursor, table_name="JumpRj", data=jump_rj, matches_columns=None)

    # Imports Jump table (with the new Person77's uniqueIDs)
    jump = get_data_from_table(cursor=source_cursor, table_name="Jump",
                               where_condition="Jump.sessionID={}".format(source_session))

    jump = update_persons77_ids(jump, persons77)
    jump = update_session_ids(jump, new_session_id)

    insert_data_into_table(cursor=destination_cursor, table_name="Jump", data=jump, matches_columns=None)

    # Imports PersonSession77
    person_session_77 = get_data_from_table(cursor=source_cursor, table_name="PersonSession77",
                                            where_condition="PersonSession77.sessionID={}".format(source_session))
    person_session_77 = update_persons77_ids(person_session_77, persons77)
    person_session_77 = update_session_ids(person_session_77, new_session_id)
    insert_data_into_table(cursor=destination_cursor, table_name="PersonSession77", data=person_session_77, matches_columns=None)

    destination_db.commit()
    destination_db.close()

    source_db.close()


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
    logging.debug("SQL: {} -- {}".format(sql,comment))
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
