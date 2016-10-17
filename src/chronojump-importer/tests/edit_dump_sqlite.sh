#!/bin/bash

# Dev tool - to be used only by developers changing unit tests
# for the chronojump-importer.

# Small utility script to help modifying binary .sqlite databases.
# It dumps a dump of the .sqlite file into a temporary SQL statements
# file, opens vim on this file, and then it deletes the original file
# and creates it with the just modified SQL statements.

TEMPORARY_FILE=$(mktemp)

echo .dump | sqlite3 "$1" > "$TEMPORARY_FILE"
vim "$TEMPORARY_FILE"
cp "$1" "$1.bck"
rm "$1"
cat "$TEMPORARY_FILE" | sqlite3 "$1"
echo "Done!"
