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

        # shutil.rmtree(self.temporary_directory_path)

    def test_increment_suffix(self):
        self.assertEqual(chronojump_importer.increment_suffix("Free Jump"), "Free Jump (1)")
        self.assertEqual(chronojump_importer.increment_suffix("Free Jump (1)"), "Free Jump (2)")
        self.assertEqual(chronojump_importer.increment_suffix("Free Jump (2)"), "Free Jump (3)")

    def test_remove_elements(self):
        l=[1,2,3,4,5]
        actual = chronojump_importer.remove_elements(l, [2,4])
        self.assertEqual(actual, [1,3,5])

    def test_add_prefix(self):
        l=['hello', 'chronojump']
        actual = chronojump_importer.add_prefix(l, "test_")
        self.assertEqual(actual, ["test_hello", "test_chronojump"])

    def test_update_session_ids(self):
        table=[{'sessionID': 2, 'name': 'hello'}, {'sessionID':3, 'name':'bye'}]

        actual = chronojump_importer.update_session_ids(table, 4)
        for row in actual:
            self.assertEqual(row['sessionID'], 4)

    def test_remove_duplicates_list(self):
        l = [1,1,2,3,2]

        actual = chronojump_importer.remove_duplicates_list(l)

        self.assertEqual(sorted(actual), sorted([1,2,3]))

    def test_update_ids_from_table(self):
        table_to_update = [{'name': 'john', 'personId': 1}, {'name': 'mark', 'personId': 4}, {'name': 'alex', 'personId': 5}]
        column_to_update = 'personId'
        referenced_table = [{'personId': 11, 'old_personId': 1}, {'personId': 12, 'old_personId': 4}]
        old_reference_column = 'old_personId'
        new_reference_column = 'personId'

        actual = chronojump_importer.update_ids_from_table(table_to_update, column_to_update, referenced_table, old_reference_column, new_reference_column)

        self.assertEqual(len(actual), 3)
        self.assertTrue({'name': 'john', 'personId': 11} in actual)
        self.assertTrue({'name': 'mark', 'personId': 12} in actual)
        self.assertTrue({'name': 'alex', 'personId': 5} in actual)

    def test_get_column_names(self):
        filename = tempfile.mktemp(prefix="chronojump_importer_test_get_column_", suffix=".sqlite")
        open(filename, 'a').close()

        database = chronojump_importer.open_database(filename, read_only=False)
        cursor = database.cursor()

        cursor.execute("CREATE TABLE test (uniqueID INTEGER, name TEXT, surname1 TEXT, surname2 TEXT, age INTEGER)")

        columns = chronojump_importer.get_column_names(cursor=cursor, table="test", skip_columns=["surname1", "surname2"])

        self.assertEqual(columns, ["uniqueID", "name", "age"])

        database.close()
        os.remove(filename)


if __name__ == '__main__':
    unittest.main()
