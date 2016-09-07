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

@ddt.ddt
class TestImporter(unittest.TestCase):
    def setUp(self):
        pass

    def tearDown(self):
        pass

    # lists the names. They will expand to generic-destination-X.sqlite / generic-source-X.sqlite / generic-expected-X.sqlite
    @ddt.data("a", "b", "c")
    def test_importerGeneric(self, data):
        temporary_directory_path = tempfile.mkdtemp(prefix="chronojump_importer_test_")

        generic_test = data
        source_file_name = "generic-source-{}.sqlite".format(generic_test)
        destination_file_name = "generic-destination-{}.sqlite".format(generic_test)
        expected_file_name = "generic-expected-{}.sqlite".format(generic_test)
        original_destination_file_path = "generic-original-destination-{}.sqlite".format(generic_test)

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

if __name__ == '__main__':
    unittest.main()
