#!/usr/bin/env python3

import unittest
import chronojump_importer
import os
import subprocess
import tempfile
import shutil


class TestImporter(unittest.TestCase):
    def setUp(self):
        self.temporary_directory_path = tempfile.mkdtemp(prefix="chronojump_importer_test_")

    def tearDown(self):
        pass
        #shutil.rmtree(self.temporary_directory_path)

    def test_importerGeneric(self):

        # lists the names. They will expand to generic-destination-X.sqlite / generic-source-X.sqlite / generic-expected-X.sqlite
        generic_tests = ["a", "b"]

        for generic_test in generic_tests:
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

            # command = "python3 ./chronojump_importer.py --source {} --destination {} --source_session 1".format(source_file_path, destination_file_path)
            # print("Command:", command)
            # os.system(command)

            chronojump_importer.import_database(source_file_path, destination_file_path, 1)

            os.system("echo .dump | sqlite3 {} > {}/destination.sql".format(destination_file_path, self.temporary_directory_path))
            os.system("echo .dump | sqlite3 tests/{} > {}/expected.sql".format(expected_file_name, self.temporary_directory_path))

            command = "diff -u {}/destination.sql {}/expected.sql".format(self.temporary_directory_path, self.temporary_directory_path)
            print("command:",command)
            diff = subprocess.getoutput(command)

            self.maxDiff = None
            self.assertEqual(diff, "")

if __name__ == '__main__':
    unittest.main()
