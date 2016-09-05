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
        self.temporary_directory_path = tempfile.mkdtemp(prefix="chronojump_importer_test_")

    def tearDown(self):
        shutil.rmtree(self.temporary_directory_path)

    # lists the names. They will expand to generic-destination-X.sqlite / generic-source-X.sqlite / generic-expected-X.sqlite
    @ddt.data("a", "b")
    def test_importerGeneric(self, data):
        generic_test = data
        source_file_name = "generic-source-{}.sqlite".format(generic_test)
        destination_file_name = "generic-destination-{}.sqlite".format(generic_test)
        expected_file_name = "generic-expected-{}.sqlite".format(generic_test)
        original_destination_file_path = "generic-original-destination-{}.sqlite".format(generic_test)

        source_file_path = "{}/{}".format(self.temporary_directory_path, source_file_name)
        destination_file_path = "{}/{}".format(self.temporary_directory_path, destination_file_name)
        original_destination_file_path = "{}/{}".format(self.temporary_directory_path, original_destination_file_path)

        shutil.copy("tests/{}".format(source_file_name), source_file_path)
        shutil.copy("tests/{}".format(destination_file_name), destination_file_path)
        shutil.copy("tests/{}".format(destination_file_name), original_destination_file_path)

        chronojump_importer.import_database(source_file_path, destination_file_path, 1)

        os.system("echo .dump | sqlite3 {} > {}/destination.sql".format(destination_file_path, self.temporary_directory_path))
        os.system("echo .dump | sqlite3 tests/{} > {}/expected.sql".format(expected_file_name, self.temporary_directory_path))

        actual_file = open(self.temporary_directory_path + "/destination.sql")
        expected_file = open(self.temporary_directory_path + "/expected.sql")

        actual_dump = actual_file.readlines()
        expected_dump = expected_file.readlines()

        actual_file.close()
        expected_file.close()

        diff = difflib.unified_diff(actual_dump, expected_dump)
        diff = "".join(diff)

        self.maxDiff = None
        self.assertEqual(diff, "")

if __name__ == '__main__':
    unittest.main()
