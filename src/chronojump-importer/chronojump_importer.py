#!/usr/bin/env python3

import copy
import argparse
import sqlite3
import logging
import sys
import re

logging.basicConfig(level=logging.INFO)

"""
/*
 * This file is part of ChronoJump
 *
 * ChronoJump is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * ChronoJump is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * Copyright (C) 2016 Carles Pina
 */
"""


def get_column_names(cursor, table, skip_columns = []):
    """ Returns the column names of table. Doesn't return any columns
    indicated by skip_columns. """
    cursor.execute("PRAGMA table_info({})".format(table))
    result = cursor.fetchall()

    names = []

    for row in result:
        column_name = row[1]
        if column_name not in skip_columns:
            names.append(column_name)

    assert len(names) > 0
    return names


def remove_elements(list_of_elements, elements_to_remove):
    """Returns a new list with list_of_elements without elements_to_remove"""
    result = []

    for element in list_of_elements:
        if element not in elements_to_remove:
            result.append(element)

    return result


def add_prefix(list_of_elements, prefix):
    """  Returns a copy of list_of_elements prefixing each element with prefix. """
    result = []

    for element in list_of_elements:
        result.append("{}{}".format(prefix, element))

    return result


def insert_data_into_table(cursor, table_name, data, matches_columns, avoids_duplicate_column=None):
    """ Data is a list of dictionaries and the keys should match the columns
    of table_name.

    Inserts the data and returns a copy of data with a new key per each
    dictionary: new_unique_id. This is the new uniqueID for this row if it
    didn't exist or the old one. The matching is based on matches_columns.

    For example, if matches_columns = ["Name"] it will insert a new row
    in the table if the name didn't exist and will add new_unique_id
    with this unique id.
    If name already existed it will NOT insert anything in the table
    but will add a new_unique_id with the ID of this person.

    If matches_columns is None it means that will insert the data
    regardless of any column.
    """

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
            execute_query_and_log(cursor, sql, where_values)

            results = cursor.fetchall()

        if matches_columns is None or len(results) == 0:
            # Needs to insert it

            avoids_column_duplicate(cursor=cursor, table_name=table_name, column_name=avoids_duplicate_column, data_row=row)

            new_id = insert_dictionary_into_table(cursor, table_name, row)
            row['importer_action'] = "inserted"

        else:
            # Uses the existing id as new_unique_id
            new_id = results[0][0]
            row['importer_action'] = "reused"

        row['new_uniqueID'] = new_id

    print_summary(table_name, data_result)
    return data_result


def get_data_from_table(cursor, table_name, where_condition, join_clause ="", group_by_clause=""):
    """ Returns a list of dictionaries of the table table_name applying the where_condition, join_clause and group_by_clause. """
    column_names = get_column_names(cursor, table_name)

    column_names_with_prefixes = add_prefix(column_names, "{}.".format(table_name))

    where_condition = " WHERE {} ".format(where_condition)
    assert '"' not in where_condition   # Easy way to avoid problems - where_condition is only used by us (programmers) and
                                        # it doesn't depend on user data.

    if group_by_clause != "":
        group_by = " GROUP BY {}".format(group_by_clause)
    else:
        group_by = ""

    format_data = {"column_names": ",".join(column_names_with_prefixes), "table_name": table_name, "join_clause": join_clause, "where": where_condition, "group_by": group_by}

    sql = "SELECT {column_names} FROM {table_name} {join_clause} {where} {group_by}".format(**format_data)
    execute_query_and_log(cursor, sql, [])

    results = cursor.fetchall()

    data = []
    for row in results:
        r = {}
        for i, col in enumerate(row):
            r[column_names[i]] = col

        data.append(r)

    return data


def insert_dictionary_into_table(cursor, table_name, row, skip_columns=["uniqueID"]):
    """ Inserts the row (it's a dictionary) into table_name and skips skip_column.
    Returns the new Id of the inserted row.
    """
    values = []
    column_names = []
    place_holders = []
    table_column_names = get_column_names(cursor, table_name)

    for column_name in row.keys():
        if column_name in skip_columns or column_name not in table_column_names:
            continue

        values.append(row[column_name])
        column_names.append(column_name)
        place_holders.append("?")

    sql = "INSERT INTO {table_name} ({column_names}) VALUES ({place_holders})".format(table_name=table_name,
                                                                                    column_names=",".join(column_names),
                                                                                    place_holders=",".join(place_holders))
    execute_query_and_log(cursor, sql, values)

    new_id = cursor.lastrowid

    return new_id


def update_session_ids(table, new_session_id):
    """ table argument is a list of dictionaries. It returns a copy of it
     replacing each sessionID by new_session_id.
     """
    result = copy.deepcopy(table)

    changed = False

    for row in table:
        row["sessionID"] = new_session_id
        changed = True
        break

    assert changed

    return result


def print_summary(table_name, table_data):
    inserted_ids = []
    reused_ids = []
    for row in table_data:
        if row['importer_action'] == 'inserted':
            inserted_ids.append(row['uniqueID'])

        elif row['importer_action'] == 'reused':
            reused_ids.append(row['uniqueID'])
        else:
            assert False

    print("{table_name}".format(table_name=table_name))
    print("\tinserted: {inserted_counter} uniqueIDs: {inserted}".format(inserted_counter=len(inserted_ids), inserted=inserted_ids))
    print("\treused: {reused_counter} uniqueIDs: {reused}".format(reused_counter=len(reused_ids), reused=reused_ids))


def remove_duplicates_list(l):
    """ Returns a new list without duplicate elements. """
    result = []

    for index, element in enumerate(l):
        if element not in l[index+1:]:
            result.append(element)

    return result


def increment_suffix(value):
    suffix = re.match("(.*) \(([0-9]+)\)", value)

    if suffix == None:
        return "{} (1)".format(value)
    else:
        base_name = suffix.group(1)
        counter = int(suffix.group(2))
        counter += 1
        return "{} ({})".format(base_name, counter)


def avoids_column_duplicate(cursor, table_name, column_name, data_row):
    """ Makes sure that data_row[column_name] doesn't exist in table_name. If it exists
    it changes data_row[column_name] to the same with (1) or (2)"""
    if column_name is None:
        return

    data_row['old_' + column_name] = data_row[column_name]

    while True:
        sql = "SELECT count(*) FROM {table_name} WHERE {column}=?".format(table_name=table_name, column=column_name)
        binding_values = []
        binding_values.append(data_row[column_name])
        execute_query_and_log(cursor, sql, binding_values)

        results = cursor.fetchall()

        if results[0][0] == 0:
            break
        else:
            data_row[column_name] = increment_suffix(data_row[column_name])
            data_row['new_' + column_name] = data_row[column_name]


def update_ids_from_table(table_to_update, column_to_update, referenced_table, old_referenced_column, new_referenced_column):
    result = copy.deepcopy(table_to_update)

    for row_to_update in result:
        old_id = row_to_update[column_to_update]
        for row_referenced in referenced_table:
            old_column_name = old_referenced_column

            if old_column_name in row_referenced and row_referenced[old_referenced_column] == old_id:
                row_to_update[column_to_update] = row_referenced[new_referenced_column]

    return result


def import_database(source_path, destination_path, source_session):
    """ Imports the session source_session from source_db into destination_db """

    logging.debug("source path:" + source_path)
    logging.debug("destination path:" + destination_path)

    source_db = open_database(source_path, read_only=True)
    destination_db = open_database(destination_path, read_only=False)

    source_cursor = source_db.cursor()
    destination_cursor = destination_db.cursor()

    # Imports the session
    session = get_data_from_table(cursor=source_cursor, table_name="Session",
                                  where_condition="Session.uniqueID={}".format(source_session))

    number_of_matching_sessions = len(session)

    if number_of_matching_sessions == 0:
        print("Trying to import {session} from {source_file} and it doesn't exist. Cancelling...".format(session=source_session,
                                                                                                         source_file=source_path))
        sys.exit(1)
    elif number_of_matching_sessions > 1:
        print("Found {number_of_sessions} in {source_file} which is not possible. Cancelling...".format(number_of_sessions=number_of_matching_sessions,
                                                                                                        source_file=source_path))
        sys.exit(1)

    session = insert_data_into_table(cursor=destination_cursor, table_name="Session", data=session,
                                     matches_columns=get_column_names(destination_cursor, "Session", ["uniqueID"]))

    new_session_id = session[0]['new_uniqueID']

    # Imports JumpType table
    jump_types = get_data_from_table(cursor=source_cursor, table_name="JumpType",
                                     where_condition="Session.uniqueID={}".format(source_session),
                                     join_clause="LEFT JOIN Jump ON JumpType.name=Jump.type LEFT JOIN Session ON Jump.sessionID=Session.uniqueID",
                                     group_by_clause="JumpType.uniqueID")

    jump_types = insert_data_into_table(cursor=destination_cursor, table_name="JumpType", data=jump_types,
                           matches_columns=get_column_names(destination_cursor, "JumpType", ["uniqueID"]),
                           avoids_duplicate_column="name")

    # Imports JumpRjType table
    jump_rj_types = get_data_from_table(cursor=source_cursor, table_name="JumpRjType",
                                        where_condition="Session.uniqueID={}".format(source_session),
                                        join_clause="LEFT JOIN JumpRj ON JumpRjType.name=JumpRj.type LEFT JOIN Session on JumpRj.sessionID=Session.uniqueID",
                                        group_by_clause="JumpRjType.uniqueID")

    jump_rj_types = insert_data_into_table(cursor=destination_cursor, table_name="JumpRjType", data=jump_rj_types,
                           matches_columns=get_column_names(destination_cursor, "JumpRjType", ["uniqueID"]),
                           avoids_duplicate_column="name")

    # Imports Persons77 used by JumpRj table
    persons77_jump_rj = get_data_from_table(cursor=source_cursor, table_name="Person77",
                                            where_condition="JumpRj.sessionID={}".format(source_session),
                                            join_clause="LEFT JOIN JumpRj ON Person77.uniqueID=JumpRj.personID",
                                            group_by_clause="Person77.uniqueID")

    # Imports Person77 used by Jump table
    persons77_jump = get_data_from_table(cursor=source_cursor, table_name="Person77",
                                         where_condition="Jump.sessionID={}".format(source_session),
                                         join_clause="LEFT JOIN Jump ON Person77.uniqueID=Jump.personID",
                                         group_by_clause="Person77.uniqueID")

    persons77 = remove_duplicates_list(persons77_jump + persons77_jump_rj)

    persons77 = insert_data_into_table(cursor=destination_cursor, table_name="Person77", data=persons77,
                                            matches_columns=["name"])

    # Imports JumpRj table (with the new Person77's uniqueIDs)
    jump_rj = get_data_from_table(cursor=source_cursor, table_name="JumpRj",
                                  where_condition="JumpRj.sessionID={}".format(source_session))

    jump_rj = update_ids_from_table(jump_rj, "personID", persons77, "uniqueID", "new_uniqueID")
    jump_rj = update_session_ids(jump_rj, new_session_id)
    jump_rj = update_ids_from_table(jump_rj, "type", persons77, "old_name", "new_name")

    insert_data_into_table(cursor=destination_cursor, table_name="JumpRj", data=jump_rj, matches_columns=None)

    # Imports Jump table (with the new Person77's uniqueIDs)
    jump = get_data_from_table(cursor=source_cursor, table_name="Jump",
                               where_condition="Jump.sessionID={}".format(source_session))

    jump = update_ids_from_table(jump, "personID", persons77, "uniqueID", "new_uniqueID")
    jump = update_session_ids(jump, new_session_id)
    jump = update_ids_from_table(jump, "type", jump_types, "old_name", "new_name")

    insert_data_into_table(cursor=destination_cursor, table_name="Jump", data=jump, matches_columns=None)

    # Imports PersonSession77
    person_session_77 = get_data_from_table(cursor=source_cursor, table_name="PersonSession77",
                                            where_condition="PersonSession77.sessionID={}".format(source_session))
    person_session_77 = update_ids_from_table(person_session_77, "personID", persons77, "uniqueID", "new_uniqueID")
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


def execute_query_and_log(cursor, sql, where_values):
    logging.debug("SQL: {} - values: {}".format(sql, where_values))
    cursor.execute(sql, where_values)


def show_information(database_path):
    database = open_database(database_path, read_only=True)
    cursor = database.cursor()

    sessions = get_data_from_table(cursor=cursor, table_name="Session", where_condition="1=1")

    print("sessionID, date, place, comments")
    for session in sessions:
        print("{uniqueID}, {date}, {place}, {comments}".format(**session))


def process_command_line():
    parser = argparse.ArgumentParser(description="Allows to import a session from one Chronojump database file into another one")
    parser.add_argument("--source", type=str, required=True,
                        help="chronojump.sqlite that we are importing from")
    parser.add_argument("--destination", type=str, required=False,
                        help="chronojump.sqlite that we import to")
    parser.add_argument("--source_session", type=int, required=False,
                        help="Session from source that will be imported to a new session in destination")
    parser.add_argument("--information", required=False, action='store_true',
                        help="Shows information of the source database")
    args = parser.parse_args()

    if args.information:
        show_information(args.source)
    else:
        if args.destination and args.source_session:
            import_database(args.source, args.destination, args.source_session)
        else:
            print("if --information not used --source, --destination and --source_session parameters are required")


if __name__ == "__main__":
    process_command_line()
