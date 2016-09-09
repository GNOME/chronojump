#!/usr/bin/env python3

import unittest
import chronojump_importer
import os
import subprocess
import tempfile
import shutil
import difflib
import ddt
import pprint
import sqlite3

@ddt.ddt
class TestImporter(unittest.TestCase):
    def setUp(self):
        pass

    def tearDown(self):
        pass

    # lists the names. {} will expand to source/destination/expected.
    @ddt.data(
        {'base_filename': 'generic-{}-a.sqlite', 'session': 1},
        {'base_filename': 'generic-{}-b.sqlite', 'session': 1},
        {'base_filename': 'generic-{}-c.sqlite', 'session': 1},
        {'base_filename': 'padu-{}.sqlite', 'session': 19},
        {'base_filename': 'yoyo-{}.sqlite', 'session': 19}
    )
    def test_importerGeneric(self, data):
        base_filename = data['base_filename']
        source_file_name = base_filename.format('source')
        destination_file_name = base_filename.format('destination')
        expected_file_name = base_filename.format('expected')
        original_destination_file_path = base_filename.format('original-destination')

        temporary_directory_path = tempfile.mkdtemp(prefix="chronojump_importer_test_{}".format(base_filename.replace("{}", "")))

        source_file_path = "{}/{}".format(temporary_directory_path, source_file_name)
        destination_file_path = "{}/{}".format(temporary_directory_path, destination_file_name)
        original_destination_file_path = "{}/{}".format(temporary_directory_path, original_destination_file_path)

        shutil.copy("tests/{}".format(source_file_name), source_file_path)
        shutil.copy("tests/{}".format(destination_file_name), destination_file_path)
        shutil.copy("tests/{}".format(destination_file_name), original_destination_file_path)

        chronojump_importer.import_database(source_file_path, destination_file_path, 1)

        os.system("echo .dump | sqlite3 {} > {}/destination.sql".format(destination_file_path, temporary_directory_path))
        os.system("echo .dump | sqlite3 tests/{} > {}/expected.sql".format(expected_file_name, temporary_directory_path))

        actual_file = open(temporary_directory_path + "/destination.sql")
        expected_file = open(temporary_directory_path + "/expected.sql")

        actual_dump = actual_file.readlines()
        expected_dump = expected_file.readlines()

        actual_file.close()
        expected_file.close()

        diff = difflib.unified_diff(actual_dump, expected_dump)
        diff = "".join(diff)

        self.maxDiff = None
        self.assertEqual(diff, "")

        shutil.rmtree(temporary_directory_path)

    def test_increment_suffix(self):
        self.assertEqual(chronojump_importer.Database.increment_suffix("Free Jump"), "Free Jump (1)")
        self.assertEqual(chronojump_importer.Database.increment_suffix("Free Jump (1)"), "Free Jump (2)")
        self.assertEqual(chronojump_importer.Database.increment_suffix("Free Jump (2)"), "Free Jump (3)")

    def test_add_prefix(self):
        l=['hello', 'chronojump']
        actual = chronojump_importer.Database.add_prefix(l, "test_")
        self.assertEqual(actual, ["test_hello", "test_chronojump"])

    def test_update_session_ids(self):
        table = chronojump_importer.Table("test")
        row1 = chronojump_importer.Row()
        row1.set("sessionID", 2)
        row1.set("name", "john")

        row2 = chronojump_importer.Row()
        row2.set("sessionID", 3)
        row2.set("name", "mark")

        table.insert_row(row1)
        table.insert_row(row2)

        table.update_session_ids(4)
        for row in table._table_data:
            self.assertEqual(row.get('sessionID'), 4)

    def test_remove_duplicates_list(self):
        l = [1,1,2,3,2]

        actual = chronojump_importer.remove_duplicates_list(l)

        self.assertEqual(sorted(actual), sorted([1,2,3]))

    def test_update_ids_from_table(self):
        table_to_update = chronojump_importer.Table("table_to_update")
        row1 = chronojump_importer.Row()
        row1.set("name", "john")
        row1.set("personId", 1)

        row2 = chronojump_importer.Row()
        row2.set("name", "mark")
        row2.set("personId", 4)

        row3 = chronojump_importer.Row()
        row3.set("name", "alex")
        row3.set("personId", 5)

        table_to_update.insert_row(row1)
        table_to_update.insert_row(row2)
        table_to_update.insert_row(row3)


        column_to_update = 'personId'

        referenced_table = chronojump_importer.Table("referenced_table")
        row4 = chronojump_importer.Row()
        row4.set("personId", 11)
        row4.set("old_personId", 1)

        row5 = chronojump_importer.Row()
        row5.set("personId", 12)
        row5.set("old_personId", 4)

        referenced_table.insert_row(row4)
        referenced_table.insert_row(row5)

        old_reference_column = 'old_personId'
        new_reference_column = 'personId'

        table_to_update.update_ids(column_to_update, referenced_table, old_reference_column, new_reference_column)

        self.assertEqual(len(table_to_update._table_data), 3)

        def verify_exists(table, name, personId):
            for row in table._table_data:
                if row.get('name') == name and row.get('personId') == personId:
                    return True

            return False

        self.assertTrue(verify_exists(table_to_update, "john", 11))
        self.assertTrue(verify_exists(table_to_update, "mark", 12))
        self.assertTrue(verify_exists(table_to_update, "alex", 5))

    def test_get_column_names(self):
        filename = tempfile.mktemp(prefix="chronojump_importer_test_get_column_", suffix=".sqlite")
        open(filename, 'a').close()

        database = chronojump_importer.Database(filename, read_only=False)
        cursor = database._cursor

        cursor.execute("CREATE TABLE test (uniqueID INTEGER, name TEXT, surname1 TEXT, surname2 TEXT, age INTEGER)")

        columns = database.column_names(table="test", skip_columns=["surname1", "surname2"])

        self.assertEqual(columns, ["uniqueID", "name", "age"])

        database.close()
        os.remove(filename)

    def test_table_name(self):
        table = chronojump_importer.Table("Session")

        self.assertEqual(table.name, "Session")


if __name__ == '__main__':
    unittest.main()
