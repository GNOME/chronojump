import unittest
import chronojump_importer
import os
import subprocess
import tempfile
import shutil

class TestImporter(unittest.TestCase):
    def test_importerA(self):

        temporary_directory_path = tempfile.mkdtemp(prefix="chronojump_importer_test_")

        print("temporary directory path:", temporary_directory_path)

        source_file = "{}/source-a.sqlite".format(temporary_directory_path)
        destination_file = "{}/destination-a.sqlite".format(temporary_directory_path)

        shutil.copy("/home/carles/git/chronojump/src/chronojump-importer/tests/source-a.sqlite", source_file)

        shutil.copy("/home/carles/git/chronojump/src/chronojump-importer/tests/destination-a.sqlite", destination_file)

        command = "python3 ./chronojump_importer.py --source {} --destination {} --source_session 1".format(source_file, destination_file)

        print("Command: ", command)
        os.system(command)
        os.system("echo .dump | sqlite3 {}/destination-a.sqlite > {}/destination-a.sql".format(temporary_directory_path, temporary_directory_path))
        os.system("echo .dump | sqlite3 /home/carles/git/chronojump/src/chronojump-importer/tests/expected-a.sqlite > {}/expected-a.sql".format(temporary_directory_path))

        diff = subprocess.getoutput("diff -u {}/destination-a.sql {}/expected-a.sql".format(temporary_directory_path, temporary_directory_path))

        self.maxDiff = None
        self.assertEqual(diff, "")

if __name__ == '__main__':
    unittest.main()
