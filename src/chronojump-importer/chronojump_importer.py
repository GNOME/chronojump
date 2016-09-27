#!/usr/bin/env python3

import argparse
import logging
import sqlite3
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


class Row:
    """ A row represents a row in a table: it has column-names and their values.
    It can contain column names that are not in the database (this can be used
    to store other information if needed) """

    def __init__(self):
        self._row = {}

    def set(self, column_name, value):
        """ Sets the value to the column_name """
        self._row[column_name] = value

    def get(self, column_name):
        """ Returns the value of column_name. Raise an exception if column_name in this row doesn't exist """
        return self._row[column_name]

    def has_column(self, column_name):
        """ Returns true if the row has the column column_name """
        return column_name in self._row

    def columns(self):
        """ Returns a list of columns in this row """
        return self._row.keys()

    def __eq__(self, other):
        # noinspection PyProtectedMember
        return self._row == other._row


class Table:
    """ This class has Table operations: inserts rows, removes duplicates, updates sessionIDs, etc. """

    def __init__(self, table_name):
        self._table_data = []
        self._table_name = table_name

    def insert_row(self, row):
        self._table_data.append(row)

    def concatenate_table(self, other):
        """ Concatenates other in this table. It doesn't change the table names """
        self._table_data += other

    def remove_duplicates(self):
        """ Remove duplicate rows of the table. The order of the rows in the table could change """
        new_data = []

        for index, element in enumerate(self._table_data):
            if element not in self._table_data[index + 1:]:
                new_data.append(element)

        self._table_data = new_data

    @property
    def name(self):
        """ Property holding the table name """
        return self._table_name

    def update_session_ids(self, new_session_id):
        """ Updates all the sessionID of each row to new_session_id """
        changed = False

        for row in self._table_data:
            row.set("sessionID", new_session_id)
            changed = True

        if len(self._table_data) > 0:
            assert changed

    def update_ids(self, column_to_update, referenced_table, old_referenced_column, new_referenced_column):
        """ For each row: matches column_to_update values with a row in referenced_table old_referenced_column values.
        If they are the same it updates column_to_update with new_referenced_column
        """
        for row_to_update in self._table_data:
            old_id = row_to_update.get(column_to_update)
            for row_referenced in referenced_table:
                old_column_name = old_referenced_column

                if row_referenced.has_column(old_column_name) and row_referenced.get(old_referenced_column) == old_id:
                    row_to_update.set(column_to_update, row_referenced.get(new_referenced_column))

    def __iter__(self):
        return iter(self._table_data)

    def __len__(self):
        return len(self._table_data)

    def __getitem__(self, index):
        return self._table_data[index]


class Database:
    """ A database represents the database and read/writes tables. """

    def __init__(self, source_path, read_only):
        self._is_opened = False
        self._cursor = None
        self._conn = None

        self.open(source_path, read_only)
        self._is_opened = True

    def __del__(self):
        self.close()

    def open(self, filename, read_only):
        """Opens the database specified by filename. On Python3 If read_only is True
        the database is opened in read only mode
        """
        if sys.version_info >= (3, 0):
            if read_only:
                mode = "ro"
            else:
                mode = "rw"

            uri = "file:{}?mode={}".format(filename, mode)
            self._conn = sqlite3.connect(uri, uri=True)
        else:
            # On Python2 there is no uri support. This opens
            # the database always on rw
            self._conn = sqlite3.connect(filename)

        self._conn.execute("pragma foreign_keys=ON")
        self._cursor = self._conn.cursor()

    def close(self):
        if self._is_opened:
            self._conn.commit()
            self._conn.close()
            self._is_opened = False

    def column_names(self, table, skip_columns=None):
        """ Returns a list with the column names of the table. Doesn't return columns mentioned in skip_columns """

        self._cursor.execute("PRAGMA table_info({})".format(table))
        result = self._cursor.fetchall()

        names = []

        for row in result:
            column_name = row[1]
            if skip_columns is None or column_name not in skip_columns:
                names.append(column_name)

        assert len(names) > 0
        return names

    def read(self, table_name, where_condition, join_clause="", group_by_clause=""):
        """ Returns a new table with the contents of this table with where_condition. """
        column_names = self.column_names(table_name)

        column_names_with_prefixes = self._add_prefix(column_names, "{}.".format(table_name))

        where_condition = " WHERE {} ".format(where_condition)
        assert '"' not in where_condition  # Easy way to avoid problems - where_condition is only used by us (programmers) and
        # it doesn't depend on user data.

        if group_by_clause != "":
            group_by = " GROUP BY {}".format(group_by_clause)
        else:
            group_by = ""

        format_data = {"column_names": ",".join(column_names_with_prefixes), "table_name": table_name,
                       "join_clause": join_clause, "where": where_condition, "group_by": group_by}

        sql = "SELECT {column_names} FROM {table_name} {join_clause} {where} {group_by}".format(**format_data)
        self._execute_query_and_log(sql, [])

        results = self._cursor.fetchall()

        table = Table(table_name)

        for row in results:
            table_row = Row()
            for i, col in enumerate(row):
                table_row.set(column_names[i], col)

            table.insert_row(table_row)

        return table

    def write(self, table, matches_columns, avoids_duplicate_column=None):
        """ Writes table into the database.

        Inserts the data and modifies table adding new_unique_id. This is the new uniqueID
        if the row has been inserted or the old one if the row has been reused. This
        depends on avoid_duplicate_columns.

        For example, if matches_columns = ["Name"] it will insert a new row
        in the table if the name didn't exist and will add new_unique_id
        with this unique id.
        If name already existed it will NOT insert anything in the table
        but will add a new_unique_id with the ID of this person.

        If matches_columns is None it means that will insert the data
        regardless of any column.
        """

        for row in table:
            if type(matches_columns) == list:
                where = ""
                where_values = []
                for column in matches_columns:
                    if where != "":
                        where += " AND "
                    where += "{} = ?".format(column)
                    where_values.append(row.get(column))

                format_data = {'table_name': table.name,
                               'where_clause': " WHERE {}".format(where)
                               }

                sql = "SELECT uniqueID FROM {table_name} {where_clause}".format(**format_data)
                self._execute_query_and_log(sql, where_values)

                results = self._cursor.fetchall()

            if matches_columns is None or len(results) == 0:
                # Needs to insert it
                self._avoid_duplicate_value(table_name=table.name, column_name=avoids_duplicate_column, data_row=row)

                new_id = self._write_row(table.name, row)
                row.set('importer_action', 'inserted')

            else:
                # Uses the existing id as new_unique_id
                new_id = results[0][0]
                row.set('importer_action', 'reused')

            row.set('new_uniqueID', new_id)

        self._print_summary(table)

    @staticmethod
    def increment_suffix(value):
        suffix = re.match("(.*) \(([0-9]+)\)", value)

        if suffix is None:
            return "{} (1)".format(value)
        else:
            base_name = suffix.group(1)
            counter = int(suffix.group(2))
            counter += 1
            return "{} ({})".format(base_name, counter)

    @staticmethod
    def _add_prefix(list_of_elements, prefix):
        """  Returns a copy of list_of_elements prefixing each element with prefix. """
        result = []

        for element in list_of_elements:
            result.append("{}{}".format(prefix, element))

        return result

    @staticmethod
    def _print_summary(table):
        """ Prints a summary of which rows has been inserted, which ones reused, during the write operation """
        inserted_ids = []
        reused_ids = []
        for row in table:
            if row.get('importer_action') == 'inserted':
                inserted_ids.append(row.get('uniqueID'))

            elif row.get('importer_action') == 'reused':
                reused_ids.append(row.get('uniqueID'))
            else:
                assert False

        print("{table_name}".format(table_name=table.name))
        print("\tinserted: {inserted_counter} uniqueIDs: {inserted}".format(inserted_counter=len(inserted_ids),
                                                                            inserted=inserted_ids))
        print(
            "\treused: {reused_counter} uniqueIDs: {reused}".format(reused_counter=len(reused_ids),
                                                                    reused=reused_ids))

    def _write_row(self, table_name, row, skip_columns=None):
        """ Inserts the row into the table. Returns the new_id. By default skips uniqueID """

        if skip_columns is None:
            skip_columns = ["uniqueID"]

        values = []
        column_names = []
        place_holders = []
        table_column_names = self.column_names(table_name)

        for column_name in row.columns():
            if column_name in skip_columns or column_name not in table_column_names:
                continue

            values.append(row.get(column_name))
            column_names.append(column_name)
            place_holders.append("?")

        sql = "INSERT INTO {table_name} ({column_names}) VALUES ({place_holders})".format(table_name=table_name,
                                                                                          column_names=",".join(
                                                                                              column_names),
                                                                                          place_holders=",".join(
                                                                                              place_holders))
        self._execute_query_and_log(sql, values)

        new_id = self._cursor.lastrowid

        return new_id

    def _avoid_duplicate_value(self, table_name, column_name, data_row):
        """ Makes sure that data_row[column_name] doesn't exist in table_name (accessing the database).
        If it exists it changes data_row[column_name] to the same with (1) or (2)"""
        if column_name is None:
            return

        original_value = data_row.get(column_name)

        while True:
            sql = "SELECT count(*) FROM {table_name} WHERE {column}=?".format(table_name=table_name, column=column_name)
            binding_values = [data_row.get(column_name)]
            self._execute_query_and_log(sql, binding_values)

            results = self._cursor.fetchall()

            if results[0][0] == 0:
                break
            else:
                data_row.set(column_name, self.increment_suffix(data_row.get(column_name)))
                data_row.set('new_' + column_name, data_row.get(column_name))
                data_row.set('old_' + column_name, original_value)

    def _execute_query_and_log(self, sql, where_values):
        logging.debug("SQL: {} - values: {}".format(sql, where_values))
        self._cursor.execute(sql, where_values)


def import_database(source_path, destination_path, source_session):
    """ Imports the session source_session from source_db into destination_db """

    logging.debug("source path:" + source_path)
    logging.debug("destination path:" + destination_path)

    source_db = Database(source_path, read_only=True)
    destination_db = Database(destination_path, read_only=False)

    # Imports the session
    session = source_db.read(table_name="Session",
                             where_condition="Session.uniqueID={}".format(source_session))

    number_of_matching_sessions = len(session)

    if number_of_matching_sessions == 0:
        print("Trying to import {session} from {source_file} and it doesn't exist. Cancelling...".format(
            session=source_session,
            source_file=source_path))
        sys.exit(1)
    elif number_of_matching_sessions > 1:
        print("Found {number_of_sessions} in {source_file} which is not possible. Cancelling...".format(
            number_of_sessions=number_of_matching_sessions,
            source_file=source_path))
        sys.exit(1)

    destination_db.write(table=session, matches_columns=destination_db.column_names("Session", ["uniqueID"]),
                         avoids_duplicate_column="name")

    new_session_id = session[0].get('new_uniqueID')

    # Imports JumpType table
    jump_types = source_db.read(table_name="JumpType",
                                where_condition="Session.uniqueID={}".format(source_session),
                                join_clause="LEFT JOIN Jump ON JumpType.name=Jump.type LEFT JOIN Session ON Jump.sessionID=Session.uniqueID",
                                group_by_clause="JumpType.uniqueID")

    destination_db.write(table=jump_types,
                         matches_columns=destination_db.column_names("JumpType", ["uniqueID"]),
                         avoids_duplicate_column="name")

    # Imports JumpRjType table
    jump_rj_types = source_db.read(table_name="JumpRjType",
                                   where_condition="Session.uniqueID={}".format(source_session),
                                   join_clause="LEFT JOIN JumpRj ON JumpRjType.name=JumpRj.type LEFT JOIN Session on JumpRj.sessionID=Session.uniqueID",
                                   group_by_clause="JumpRjType.uniqueID")

    destination_db.write(table=jump_rj_types,
                         matches_columns=destination_db.column_names("JumpRjType", ["uniqueID"]),
                         avoids_duplicate_column="name")

    # Imports Persons77 used by JumpRj table
    persons77_jump_rj = source_db.read(table_name="Person77",
                                       where_condition="JumpRj.sessionID={}".format(source_session),
                                       join_clause="LEFT JOIN JumpRj ON Person77.uniqueID=JumpRj.personID",
                                       group_by_clause="Person77.uniqueID")

    # Imports Person77 used by Jump table
    persons77_jump = source_db.read(table_name="Person77",
                                    where_condition="Jump.sessionID={}".format(source_session),
                                    join_clause="LEFT JOIN Jump ON Person77.uniqueID=Jump.personID",
                                    group_by_clause="Person77.uniqueID")

    persons77 = Table("person77")
    persons77.concatenate_table(persons77_jump)
    persons77.concatenate_table(persons77_jump_rj)
    persons77.remove_duplicates()

    destination_db.write(table=persons77,
                         matches_columns=["name"])

    # Imports JumpRj table (with the new Person77's uniqueIDs)
    jump_rj = source_db.read(table_name="JumpRj",
                             where_condition="JumpRj.sessionID={}".format(source_session))

    jump_rj.update_ids("personID", persons77, "uniqueID", "new_uniqueID")
    jump_rj.update_session_ids(new_session_id)
    jump_rj.update_ids("type", persons77, "old_name", "new_name")

    destination_db.write(table=jump_rj, matches_columns=None)

    # Imports Jump table (with the new Person77's uniqueIDs)
    jump = source_db.read(table_name="Jump",
                          where_condition="Jump.sessionID={}".format(source_session))

    jump.update_ids("personID", persons77, "uniqueID", "new_uniqueID")
    jump.update_session_ids(new_session_id)
    jump.update_ids("type", jump_types, "old_name", "new_name")

    destination_db.write(table=jump, matches_columns=None)

    # Imports PersonSession77
    person_session_77 = source_db.read(table_name="PersonSession77",
                                       where_condition="PersonSession77.sessionID={}".format(source_session))
    person_session_77.update_ids("personID", persons77, "uniqueID", "new_uniqueID")
    person_session_77.update_session_ids(new_session_id)
    destination_db.write(table=person_session_77, matches_columns=None)


def show_information(database_path):
    database = Database(database_path, read_only=True)

    sessions = database.read(table_name="Session", where_condition="1=1")

    print("sessionID, date, place, comments")
    for session in sessions:
        data = {'uniqueID': session.get('uniqueID'),
                'date': session.get('date'),
                'place': session.get('place'),
                'comments': session.get('comments')
                }
        print("{uniqueID}, {date}, {place}, {comments}".format(**data))


def process_command_line():
    parser = argparse.ArgumentParser(
        description="Allows to import a session from one Chronojump database file into another one")
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
