#!/usr/bin/env python3

import xml.etree.ElementTree
import re

"""
 When developing ChronoJump if a new resource image is added it has to be added in the Makefiles
 (this is what it's used for compiling for the release, etc.).
 If a developer wants to use MonoDevelop: the file has to be added in MonoDevelop as well doing:
  -Open the project in MonoDevelop
  -Right click and press Add -> Add Files...
  -Select the file and if asked leave it in the same directory as now
  -Look for the file, right click, Build Action: select Embedded Resource (if multiple files have been added:
   it's possible to select multiple files)
  -Now there are two options:
    -In the properties name: change the "Resource ID" to the filename (it would be images/file_name.png). If only
     one file has been added this is enough
    -If many files have been added: this script can be executed to fix it

How to do it better? The Makefile files could generate the MonoDevelop Solution files. All the information is there.
"""

def fix_resources_local_names():
    original_file = "../chronojump.csproj"
    print("Will read {} (if the project have changed: better close MonoDevelop to make sure that all is saved)".format(original_file))
    project_file = open(original_file, 'r')
    project_string = project_file.read()

    # Removes the xmlns to avoid ElementTree to be prepended everywhere.
    # I didn't find a better solution.
    match = re.search('.* (xmlns="[^"]+").*', project_string)
    assert match

    xmlns = match.group(1)

    project_xml = re.sub(' ' + xmlns, '', project_string, count=1)

    # Reads the string. fromstring returns Element, it converts it to ElementTree.
    element_tree = xml.etree.ElementTree.ElementTree(xml.etree.ElementTree.fromstring(project_xml))

    # Restores the xmlns
    root_attributes = element_tree.getroot().attrib
    root_attributes['xmlns'] = xmlns

    for embedded_resource in element_tree.findall("./ItemGroup/EmbeddedResource"):
        attributes = embedded_resource.attrib
        if 'Include' not in attributes:
            # It's not our type of resource
            continue

        for node in embedded_resource:
            # If it already has a LogicalName: it doesn't change this Element
            if node.tag == "LogicalName":
                continue

        # It uses the Include name (e.g. images/mini/chronopic.png) to set the LogicalName: mini/chronopic.png
        new_path = attributes['Include'].split("\\")[1:] # Removes images/ / first part of the path
        new_path = "/".join(new_path)
        logical_name = xml.etree.ElementTree.Element("LogicalName")
        logical_name.text = new_path

        # Adds the new Element
        embedded_resource.append(logical_name)

    new_file = "new_chronojump.csproj"
    print("Saved the file to {}. Close MonoDevelop and Copy it manually to {}".format(new_file, original_file))
    element_tree.write(new_file)


if __name__ == "__main__":
    fix_resources_local_names()