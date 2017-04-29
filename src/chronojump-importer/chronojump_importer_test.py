#!/usr/bin/env python3

import unittest.mock
import unittest
import chronojump_importer
import os
import tempfile
import shutil
import difflib
import ddt


@ddt.ddt
class TestImporter(unittest.TestCase):
    def setUp(self):
        pass

    def tearDown(self):
        pass

    @staticmethod
    def _prepare_database_to_import(base_filename, source_session=None, destination_session=None):

        if destination_session is None:
            expected_file_name = base_filename.format('expected')
        else:
            expected_file_name = base_filename.format('expected-session-{}-to-{}'.format(source_session, destination_session))

        return TestImporter._prepare_database_to_import_do(base_filename, expected_file_name)

    @staticmethod
    def _prepare_database_to_import_do(base_filename, expected_file_name):
        source_file_name = base_filename.format('source')
        destination_file_name = base_filename.format('destination')
        original_destination_file_path = base_filename.format('original-destination')

        temporary_directory_path = tempfile.mkdtemp(
            prefix="chronojump_importer_test_{}".format(base_filename.replace("{}", "")))
        source_file_path = "{}/{}".format(temporary_directory_path, source_file_name)
        destination_file_path = "{}/{}".format(temporary_directory_path, destination_file_name)
        original_destination_file_path = "{}/{}".format(temporary_directory_path, original_destination_file_path)

        shutil.copy("tests/{}".format(source_file_name), source_file_path)
        shutil.copy("tests/{}".format(destination_file_name), destination_file_path)
        shutil.copy("tests/{}".format(destination_file_name), original_destination_file_path)

        return (source_file_path, destination_file_path, expected_file_name, temporary_directory_path)

    # lists the names. {} will expand to source/destination/expected.
    @ddt.data(
        {'base_filename': 'generic-{}-a.sqlite', 'source_session': 1},
        {'base_filename': 'generic-{}-b.sqlite', 'source_session': 1},
        {'base_filename': 'generic-{}-c.sqlite', 'source_session': 1},
        {'base_filename': 'padu-{}.sqlite', 'source_session': 1},
        {'base_filename': 'yoyo-{}.sqlite', 'source_session': 1},
        {'base_filename': 'user-jump-{}.sqlite', 'source_session': 1},
        {'base_filename': 'yoyo-{}.sqlite', 'source_session': 4, 'destination_session': 5}
    )
    def test_importer_generic(self, data):
        re_creates_test = False  # During development change it to True
                                 # to execute the tests and copy the new
                                 # result as expected test

        (source_file_path, destination_file_path, expected_file_name, temporary_directory_path) = \
            self._prepare_database_to_import(data["base_filename"], data.get("source_session"), data.get("destination_session", None))

        importer = chronojump_importer.ImportSession(source_file_path, destination_file_path, None)

        if 'destination_session' in data:
            importer.import_into_session(data["source_session"], data["destination_session"])
        else:
            importer.import_as_new_session(data["source_session"])

        os.system(
            "echo .dump | sqlite3 {} > {}/destination.sql".format(destination_file_path, temporary_directory_path))
        os.system(
            "echo .dump | sqlite3 tests/{} > {}/expected.sql".format(expected_file_name, temporary_directory_path))

        actual_file = open(temporary_directory_path + "/destination.sql")
        expected_file = open(temporary_directory_path + "/expected.sql")

        actual_dump = actual_file.readlines()
        expected_dump = expected_file.readlines()

        actual_file.close()
        expected_file.close()

        diff = difflib.unified_diff(expected_dump, actual_dump)
        diff = "".join(diff)

        if diff != "":
            # Just to help where the files are when debugging
            print("Temporary directory: ", temporary_directory_path)
            print("Base filename: ", data["base_filename"])

        if re_creates_test:
            shutil.copy(destination_file_path, "tests/" + expected_file_name)

        self.maxDiff = None
        self.assertEqual(diff, "")

        shutil.rmtree(temporary_directory_path)

    @unittest.mock.patch("os.makedirs", side_effect=os.makedirs)
    @unittest.mock.patch("shutil.copy", side_effect=shutil.copy)
    def test_import_encoder_files(self, copy_function, makedirs_function):
        (source_file_path, destination_file_path, expected_file_name, temporary_directory_path) = \
            self._prepare_database_to_import("yoyo-{}.sqlite")

        importer = chronojump_importer.ImportSession(source_file_path, destination_file_path, "fake-base-directory")

        # Mock couldn't be done earlier because these methods are used by sqlite3 when opening the database.
        copy_function.reset_mock()
        makedirs_function.reset_mock()

        copy_function.side_effect = None
        makedirs_function.side_effect = None

        importer.import_as_new_session(7)

        self.assertTrue(copy_function.called)
        self.assertEqual(copy_function.call_count, 11) # 11 encodings for session number 7

    def test_encoder_filename(self):
        new_filename = chronojump_importer.ImportSession._encoder_filename(10, "19-test.txt")
        self.assertEqual("10-test.txt", new_filename)

    def test_normalize_path(self):
        original_os_sep = os.sep

        # I don't think that unittest.mock can mock a non-call function so
        # here it changes os.sep and leave it as it was later on.
        os.sep = "/"
        self.assertEqual("test/directory", chronojump_importer.ImportSession._normalize_path("test\\directory"))
        self.assertEqual("test/directory", chronojump_importer.ImportSession._normalize_path("test/directory"))

        os.sep = "\\"
        self.assertEqual("test\\directory", chronojump_importer.ImportSession._normalize_path("test\\directory"))
        self.assertEqual("test\\directory", chronojump_importer.ImportSession._normalize_path("test/directory"))

        os.sep = original_os_sep

    def test_encoder_url(self):
        new_url = chronojump_importer.ImportSession._encoder_url(11, "signal")
        self.assertEqual(os.path.join("encoder", "11", "data", "signal"), new_url)

    def test_database_version(self):
        database_file = "tests/yoyo-source.sqlite"
        information = chronojump_importer.json_information(database_file)
        print(information)
        self.assertEqual(information['databaseVersion'], '1.28')


class TestRow(unittest.TestCase):
    def test_get(self):
        row = chronojump_importer.Row()
        row.set("name", "sam")

        self.assertEqual(row.get("name"), "sam")

    def test_columns(self):
        row = chronojump_importer.Row()
        row.set("name", "john")
        row.set("year", 1970)

        self.assertEqual(sorted(row.columns()), ["name", "year"])

    def test_has_column(self):
        row = chronojump_importer.Row()
        row.set("name", "john")

        self.assertEqual(row.has_column("name"), True)
        self.assertEqual(row.has_column("year"), False)

    def test_equal(self):
        row1 = chronojump_importer.Row()
        row1.set("name", "john")
        row1.set("year", 1970)

        row2 = chronojump_importer.Row()
        row2.set("name", "john")
        row2.set("year", 1971)

        self.assertNotEqual(row1, row2)
        row2.set("year", 1970)

        self.assertEqual(row1, row2)


class TestTable(unittest.TestCase):
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
        row1 = chronojump_importer.Row()
        row1.set("name", "john")
        row2 = chronojump_importer.Row()
        row2.set("name", "john")
        row3 = chronojump_importer.Row()
        row3.set("name", "sam")

        table = chronojump_importer.Table("Test")
        table.insert_row(row1)
        table.insert_row(row2)
        table.insert_row(row3)

        self.assertEqual(len(table), 3)
        table.remove_duplicates()

        self.assertEqual(len(table), 2)

        expected = [row1, row3]
        for row in table:
            expected.remove(row)

        self.assertEqual(len(expected), 0)

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

        def verify_exists(table, name, person_id):
            for row in table._table_data:
                if row.get('name') == name and row.get('personId') == person_id:
                    return True

            return False

        self.assertTrue(verify_exists(table_to_update, "john", 11))
        self.assertTrue(verify_exists(table_to_update, "mark", 12))
        self.assertTrue(verify_exists(table_to_update, "alex", 5))

    def test_table_name(self):
        table = chronojump_importer.Table("Session")

        self.assertEqual(table.name, "Session")


@ddt.ddt
class TestDatabase(unittest.TestCase):
    def setUp(self):
        self._create_database()
        pass

    def tearDown(self):
        self._destroy_database()
        pass

    def _create_database(self):
        """ Creates an empty file, sets self._database, self._cursor, self._filename """
        self._filename = tempfile.mktemp(prefix="chronojump_importer_test_database", suffix=".sqlite")
        open(self._filename, 'a').close()

        self._database = chronojump_importer.Database(self._filename, read_only=False)
        self._cursor = self._database._cursor

    def _destroy_database(self):
        self._database.close()
        os.remove(self._filename)

    def test_increment_suffix(self):
        self.assertEqual(chronojump_importer.Database.increment_suffix("Free Jump"), "Free Jump (1)")
        self.assertEqual(chronojump_importer.Database.increment_suffix("Free Jump (1)"), "Free Jump (2)")
        self.assertEqual(chronojump_importer.Database.increment_suffix("Free Jump (2)"), "Free Jump (3)")

    def test_add_prefix(self):
        l = ['hello', 'chronojump']

        # Yes, here we test a private and static method. Just handy and it can get re-tested in test_read()
        actual = chronojump_importer.Database._add_prefix(l, "test_")
        self.assertEqual(actual, ["test_hello", "test_chronojump"])

    def test_get_column_names(self):
        self._cursor.execute("CREATE TABLE test (uniqueID INTEGER, name TEXT, surname1 TEXT, surname2 TEXT, age INTEGER)")

        columns = self._database.column_names(table="test", skip_columns=["surname1", "surname2"])

        self.assertEqual(columns, ["uniqueID", "name", "age"])

    @ddt.data(
        {'initial_name': 'John', 'name_to_insert': 'John', 'expected_inserted_name': 'John (1)'},
        {'initial_name': 'Sam', 'name_to_insert': 'John', 'expected_inserted_name': 'John'}
    )
    def test_write(self, data):
        self._cursor.execute("CREATE TABLE test (uniqueID INTEGER PRIMARY KEY, name TEXT)")
        self._cursor.execute("INSERT INTO test (uniqueID, name) VALUES (1, ?)", (data['initial_name'], ))

        table = chronojump_importer.Table("test")
        row = chronojump_importer.Row()
        row.set(column_name="uniqueID", value="2")
        row.set(column_name="name", value=data['name_to_insert'])

        table.insert_row(row)

        self._database.write(table=table, matches_columns=None, avoids_duplicate_column="name")

        self._cursor.execute("SELECT * FROM test WHERE uniqueID=?", (2,))
        result = self._cursor.fetchone()
        self.assertEqual(result[0], 2)
        self.assertEqual(result[1], data['expected_inserted_name'])

    def test_read(self):
        self._cursor.execute("CREATE TABLE test (uniqueID INTEGER PRIMARY KEY, name TEXT)")
        self._cursor.execute("INSERT INTO test (uniqueID, name) VALUES (1, ?)", ("John",))

        table = self._database.read(table_name="test", where_condition="1=1")

        self.assertEqual(len(table), 1)
        row = table[0]
        self.assertEqual(row.get("uniqueID"), 1)
        self.assertEqual(row.get("name"), "John")
        self.assertEqual(self._database.column_names("test"), ["uniqueID", "name"])

if __name__ == '__main__':
    unittest.main(verbosity=2)
