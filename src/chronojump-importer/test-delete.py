#!/usr/bin/env python3

import argparse
import logging
import sqlite3
import sys
import json
import os
import shutil
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
 * Copyright (C) 2016-2017 Carles Pina i Estany <carles@pina.cat>
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

    def read(self, table_name, where_condition, join_clause="", group_by_clause="", extra_tables=""):
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

        table_names_str = table_name
        if extra_tables != "":
            table_names_list = [table_names_str] + extra_tables
            table_names_str = ",".join(table_names_list)

        format_data = {"column_names": ",".join(column_names_with_prefixes), "table_names_str": table_names_str,
                       "join_clause": join_clause, "where": where_condition, "group_by": group_by}

        sql = "SELECT {column_names} FROM {table_names_str} {join_clause} {where} {group_by}".format(**format_data)
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
        depends on avoids_duplicate_column.

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


class ImportSession:
    def __init__(self, source_path, destination_path, source_base_directory):
        """ Creates the object to import the session source_session from source_db into destination_db. """

        logging.debug("source path:" + source_path)
        logging.debug("destination path:" + destination_path)

        self.source_path = source_path
        self.destination_path = destination_path
        self.source_base_directory = source_base_directory

        self.source_db = Database(source_path, read_only=True)
        self.destination_db = Database(destination_path, read_only=False)

        self.source_session = None
        self.new_session_id = None

        self.persons77 = None

    def import_into_session(self, source_session, destination_session):
        self.source_session = source_session
        self.new_session_id = destination_session
        self.import_data()

    def import_as_new_session(self, source_session):
        self.source_session = source_session
        self.new_session_id = self._import_session()

        self._import_sport()
        self._import_speciality()
        self.import_data()

    def import_data(self):
        self.persons77 = self._import_persons77()

        self._import_person_session77()

        self._import_jumps()
        self._import_runs()
        self._import_pulse()
        self._import_encoder()

    def _import_session(self):
        """
        Imports the Session information saved in self._source_session (only table Session).
        Returns the new session ID.
        """

        session = self.source_db.read(table_name="Session",
                                      where_condition="Session.uniqueID={}".format(self.source_session))

        number_of_matching_sessions = len(session)

        if number_of_matching_sessions == 0:
            print("Trying to import {session} from {source_file} and it doesn't exist. Cancelling...".format(
                session=self.source_session,
                source_file=self.source_path))
            sys.exit(1)
        elif number_of_matching_sessions > 1:
            print("Found {number_of_sessions} in {source_file} which is not possible. Cancelling...".format(
                number_of_sessions=number_of_matching_sessions,
                source_file=self.source_path))
            sys.exit(1)

        self.destination_db.write(table=session, matches_columns=None,
                                  avoids_duplicate_column="name")

        return session[0].get('new_uniqueID')

    def _import_sport(self):
        sports = self.source_db.read(table_name="sport",
                                     where_condition="Sport.uniqueID=Session.personsSportID AND Session.uniqueID={}".format(self.source_session),
                                     extra_tables=["Session"])

        self.destination_db.write(table=sports,
                                  matches_columns=["name", "userDefined", "hasSpeciallities", "graphLink"])

    def _import_speciality(self):
        # It should change the hasSpeciallities: maybe in the original database didn't have but now after
        # doing this it will have speciallities
        specialities = self.source_db.read(table_name="speciallity",
                                     where_condition="Sport.uniqueID=Session.personsSportID AND Speciallity.sportId=Sport.uniqueID AND Session.uniqueID={}".format(self.source_session),
                                     extra_tables=["Sport", "Session"])

        self.destination_db.write(table=specialities,
                                  matches_columns=["sportID", "name"])


    def _import_persons77(self):
        persons77 = self.source_db.read(table_name="Person77",
                                        where_condition="personSession77.sessionID={}".format(self.source_session),
                                        join_clause="LEFT JOIN personSession77 ON personSession77.personID=Person77.uniqueID",
                                        group_by_clause="Person77.uniqueID")

        self.destination_db.write(table=persons77,
                                  matches_columns=["name"])

        return persons77

    def _import_jumps(self):
        # Imports JumpType table
        jump_types = self.source_db.read(table_name="JumpType",
                                         where_condition="Session.uniqueID={}".format(self.source_session),
                                         join_clause="LEFT JOIN Jump ON JumpType.name=Jump.type LEFT JOIN Session ON Jump.sessionID=Session.uniqueID",
                                         group_by_clause="JumpType.uniqueID")

        self.destination_db.write(table=jump_types,
                                  matches_columns=self.destination_db.column_names("JumpType", ["uniqueID"]),
                                  avoids_duplicate_column="name")

        # Imports JumpRjType table
        jump_rj_types = self.source_db.read(table_name="JumpRjType",
                                            where_condition="Session.uniqueID={}".format(self.source_session),
                                            join_clause="LEFT JOIN JumpRj ON JumpRjType.name=JumpRj.type LEFT JOIN Session on JumpRj.sessionID=Session.uniqueID",
                                            group_by_clause="JumpRjType.uniqueID")

        self.destination_db.write(table=jump_rj_types,
                                  matches_columns=self.destination_db.column_names("JumpRjType", ["uniqueID"]),
                                  avoids_duplicate_column="name")

        # Imports JumpRj table (with the new Person77's uniqueIDs)
        jump_rj = self.source_db.read(table_name="JumpRj",
                                      where_condition="JumpRj.sessionID={}".format(self.source_session))

        jump_rj.update_ids("personID", self.persons77, "uniqueID", "new_uniqueID")
        jump_rj.update_session_ids(self.new_session_id)
        jump_rj.update_ids("type", jump_rj, "old_name", "new_name")

        self.destination_db.write(table=jump_rj, matches_columns=self.destination_db.column_names("JumpRj", skip_columns=["uniqueID", "personID", "sessionID"]))

        # Imports Jump table (with the new Person77's uniqueIDs)
        jump = self.source_db.read(table_name="Jump",
                                   where_condition="Jump.sessionID={}".format(self.source_session))

        jump.update_ids("personID", self.persons77, "uniqueID", "new_uniqueID")
        jump.update_session_ids(self.new_session_id)
        jump.update_ids("type", jump_types, "old_name", "new_name")

        self.destination_db.write(table=jump, matches_columns=self.destination_db.column_names("Jump", skip_columns=["uniqueID", "personID"]))

    def _import_runs(self):
        # Imports RunTypes table
        run_types = self.source_db.read(table_name="RunType",
                                        where_condition="Run.sessionID={}".format(self.source_session),
                                        join_clause="LEFT JOIN Run ON RunType.name=Run.type",
                                        group_by_clause="RunType.uniqueID")

        self.destination_db.write(table=run_types,
                                  matches_columns=self.destination_db.column_names("RunType", ["uniqueID"]),
                                  avoids_duplicate_column="name")

        # Imports RunIntervalTypes table
        run_interval_types = self.source_db.read(table_name="RunIntervalType",
                                                 where_condition="RunInterval.sessionID={}".format(self.source_session),
                                                 join_clause="LEFT JOIN RunInterval ON RunIntervalType.name=RunInterval.type",
                                                 group_by_clause="RunIntervalType.uniqueID")

        self.destination_db.write(table=run_interval_types,
                                  matches_columns=self.destination_db.column_names("RunIntervalType", skip_columns=["uniqueID"]),
                                  avoids_duplicate_column="name")

        # Imports Run table (with the new Person77's uniqueIDs)
        run = self.source_db.read(table_name="Run",
                                  where_condition="Run.sessionID={}".format(self.source_session))
        run.update_ids("personID", self.persons77, "uniqueID", "new_uniqueID")
        run.update_session_ids(self.new_session_id)
        run.update_ids("type", run_types, "old_name", "new_name")
        self.destination_db.write(table=run,
                                  matches_columns=self.destination_db.column_names("Run", skip_columns=["uniqueID", "personID", "sessionID"]))

        # Imports RunInterval table (with the new Person77's uniqueIDs)
        run_interval = self.source_db.read(table_name="RunInterval",
                                           where_condition="RunInterval.sessionID={}".format(self.source_session))
        run_interval.update_ids("personID", self.persons77, "uniqueID", "new_uniqueID")
        run_interval.update_session_ids(self.new_session_id)
        run_interval.update_ids("type", run_interval_types, "old_name", "new_name")
        self.destination_db.write(table=run_interval,
                                  matches_columns=self.destination_db.column_names("RunInterval", skip_columns=["uniqueID", "personID", "sessionID"]))

    def _import_pulse(self):
        # Imports PulseTypes table
        pulse_types = self.source_db.read(table_name="PulseType",
                                          where_condition="Session.uniqueID={}".format(self.source_session),
                                          join_clause="LEFT JOIN Pulse ON PulseType.name=Pulse.type LEFT JOIN Session on Pulse.sessionID=Session.uniqueID",
                                          group_by_clause="PulseType.uniqueID")

        self.destination_db.write(table=pulse_types,
                                  matches_columns=self.destination_db.column_names("PulseType", ["uniqueID"]),
                                  avoids_duplicate_column="name")

        # Imports Pulse table
        pulse = self.source_db.read(table_name="Pulse",
                                    where_condition="Pulse.sessionID={}".format(self.source_session))
        pulse.update_session_ids(self.new_session_id)
        pulse.update_ids("type", pulse_types, "old_name", "new_name")
        self.destination_db.write(pulse, self.destination_db.column_names("Pulse", skip_columns=["uniqueID", "personID", "sessionID"]))

    def _import_person_session77(self):
        # Imports PersonSession77
        person_session_77 = self.source_db.read(table_name="PersonSession77",
                                                where_condition="PersonSession77.sessionID={}".format(self.source_session))
        person_session_77.update_ids("personID", self.persons77, "uniqueID", "new_uniqueID")
        person_session_77.update_session_ids(self.new_session_id)

        # Inserts the person_session_77 table but not for personsIDs that already existed in this session. This is
        # the case if a user imports a session into an existing session and the persons would be already imported.
        self.destination_db.write(table=person_session_77, matches_columns=["sessionID", "personID"])

    def _import_encoder(self):
        # Imports EncoderExercise
        encoder_exercise_from_encoder = self.source_db.read(table_name="EncoderExercise",
                                               where_condition="Encoder.sessionID={}".format(self.source_session),
                                               join_clause="LEFT JOIN Encoder ON Encoder.exerciseID=EncoderExercise.uniqueID",
                                               group_by_clause="EncoderExercise.uniqueID")

        encoder_exercise_from_encoder_1rm = self.source_db.read(table_name="EncoderExercise",
                                                            where_condition="Encoder1RM.sessionID={}".format(
                                                            self.source_session),
                                                            join_clause="LEFT JOIN Encoder1RM ON Encoder1RM.exerciseID=EncoderExercise.uniqueID",
                                                            group_by_clause="EncoderExercise.uniqueID")

        encoder_exercise = Table("encoderExercise")
        encoder_exercise.concatenate_table(encoder_exercise_from_encoder)
        encoder_exercise.concatenate_table(encoder_exercise_from_encoder_1rm)
        encoder_exercise.remove_duplicates()

        self.destination_db.write(table=encoder_exercise,
                                  matches_columns=self.destination_db.column_names("EncoderExercise", ["uniqueID"]))

        # Imports Encoder1RM
        encoder_1rm = self.source_db.read(table_name="Encoder1RM",
                                          where_condition="Encoder1RM.sessionID={}".format(self.source_session))
        encoder_1rm.update_session_ids(self.new_session_id)
        encoder_1rm.update_ids("personID", self.persons77, "uniqueID", "new_uniqueID")
        encoder_1rm.update_ids("exerciseID", encoder_exercise, "uniqueID", "new_uniqueID")
        self.destination_db.write(table=encoder_1rm,
                                  matches_columns=None)

        # Imports Encoder
        encoder = self.source_db.read(table_name="Encoder",
                                      where_condition="Encoder.sessionID={}".format(self.source_session))
        encoder.update_ids("personID", self.persons77, "uniqueID", "new_uniqueID")
        encoder.update_ids("exerciseID", encoder_exercise, "uniqueID", "new_uniqueID")
        encoder.update_session_ids(self.new_session_id)

        self._import_encoder_files(encoder)

        self.destination_db.write(table=encoder,
                                  matches_columns=self.destination_db.column_names("encoder", skip_columns=["uniqueID", "personID", "sessionID", "exerciseID"]))

        # Imports EncoderSignalCurve
        encoder_signal_curve_signals = self.source_db.read(table_name="EncoderSignalCurve",
                                                           where_condition="Encoder.signalOrCurve='signal' AND Encoder.sessionID={}".format(self.source_session),
                                                           join_clause="LEFT JOIN Encoder ON Encoder.uniqueID=EncoderSignalCurve.SignalID")

        encoder_signal_curve_curves = self.source_db.read(table_name="EncoderSignalCurve",
                                                          where_condition="Encoder.signalOrCurve='curve' AND Encoder.sessionID={}".format(self.source_session),
                                                          join_clause="LEFT JOIN Encoder ON Encoder.uniqueID=EncoderSignalCurve.curveID")

        encoder_signal_curve = Table("encoderSignalCurve")
        encoder_signal_curve.concatenate_table(encoder_signal_curve_signals)
        encoder_signal_curve.concatenate_table(encoder_signal_curve_curves)
        encoder_signal_curve.remove_duplicates()

        encoder_signal_curve.update_ids("signalID", encoder, "old_uniqueID", "new_uniqueID")
        encoder_signal_curve.update_ids("curveID", encoder, "old_uniqueID", "new_uniqueID")

        self.destination_db.write(table=encoder_signal_curve,
                                  avoids_duplicate_column=None,
                                  matches_columns=None)

    @staticmethod
    def _encoder_filename(person_id, original_filename):
        """ original_filename is like 1-Carmelo-89-2014-12-03_12-48-54.txt. It only replaces the person_id (1 in this case)"""
        filename=original_filename.split("-", 1)
        filename[0] = str(person_id)
        return "-".join(filename)

    @staticmethod
    def _encoder_url(session_id, signal_or_curve):
        return os.path.join("encoder", str(session_id), "data", signal_or_curve)

    @staticmethod
    def _normalize_path(path):
        """
        The path that it is read from the database might use Windows separators but
        we might be on a Linux system (or OS-X). This function should replace the directory
        separators to the system's ones.

        It assumes that the "/" and "\" characters are only used to separate directories.
        """
        if os.sep == "/":
            # We are on Linux, OS-X or some other system with "/" separators.
            # If the path had "\" then replace them to "/".
            return path.replace("\\", "/")
        elif os.sep == "\\":
            return path.replace("/", "\\")

    def _import_encoder_files(self, encoder_table):
        if self.source_base_directory is None:
            # We are skipping to copy the Encoding files. This is used in unit tests.
            return

        for row in encoder_table:
            # Gets information from row
            person_id = row.get("personID")
            original_filename = row.get("filename")
            original_url = self._normalize_path(row.get("url"))
            session_id = row.get("sessionID")
            signal_or_curve = row.get("signalOrCurve")

            # Prepares the new filename and destination_url
            filename=self._encoder_filename(person_id, original_filename)
            destination_url = self._encoder_url(session_id, signal_or_curve)

            # Sets it to the row
            row.set("filename", filename)
            row.set("url", destination_url)

            # Copies the files to the new place
            destination_directory = os.path.join(self.destination_path, "..", "..", destination_url)
            destination_directory = os.path.abspath(destination_directory)  # os.makedirs() can't handle directories with ".."

            destination_filename = os.path.join(destination_directory, filename)
            source_file = os.path.join(self.source_base_directory, original_url, original_filename)

            if not os.path.isdir(destination_directory):
                os.makedirs(destination_directory)

            shutil.copy(source_file, destination_filename)


def json_information(database_path):
    information = {}
    information['sessions'] = []

    database = Database(database_path, read_only=True)

    sessions = database.read(table_name="Session", where_condition="1=1")

    for session in sessions:
        data = {'uniqueID': session.get('uniqueID'),
                'date': session.get('date'),
                'place': session.get('place'),
                'comments': session.get('comments'),
                'name': session.get('name')
                }
        information['sessions'].append(data)

    preferences = database.read(table_name="Preferences", where_condition="name='databaseVersion'")
    information['databaseVersion'] = preferences[0].get("value")

    return information


def show_json_information(database_path):
    information = json_information(database_path)
    information_str = json.dumps(information, sort_keys=True, indent=4)

    print(information_str)


def process_command_line():
    parser = argparse.ArgumentParser(
        description="Allows to import a session from one Chronojump database file into another one")
    parser.add_argument("--source", type=str, required=True,
                        help="chronojump.sqlite that we are importing from")
    parser.add_argument("--source_base_directory", type=str, required=False,
                        help="Directory where the encoder/ directory (amongst database/, logs/ and multimedia/ can be found\n" +
                             "By default is parent as --source")
    parser.add_argument("--destination", type=str, required=False,
                        help="chronojump.sqlite that we import to")
    parser.add_argument("--source_session", type=int, required=False,
                        help="Session from source that will be imported to the session specified by --destination-session\n"
                             "or to a new session if no --destination-session is specified")
    parser.add_argument("--destination_session", type=int, required=False,
                        help="Imports the [source_session] into the [destination_session]. If not specified imports as\n"
                             "new session.")
    parser.add_argument("--json_information", required=False, action='store_true',
                        help="Shows information of the source database")
    args = parser.parse_args()

    if args.json_information:
        show_json_information(args.source)
    else:
        if args.destination and args.source_session:
            if args.source_base_directory:
                source_base_directory = args.source_base_directory
            else:
                source_base_directory = os.path.join(args.source, "../..")

            importer = ImportSession(args.source, args.destination, source_base_directory)

            if args.destination_session is None:
                importer.import_as_new_session(args.source_session)
            else:
                importer.import_into_session(args.source_session, args.destination_session)
        else:
            print("if --information not used --source, --destination and --source_session parameters are required")


if __name__ == "__main__":
    process_command_line()
