#!/bin/sh
set -e

MAC_APP_ROOT_DIR=app
MAC_APP_DIR="${MAC_APP_ROOT_DIR}/Chronojump.app"
MAC_APP_BIN_DIR="${MAC_APP_DIR}/Contents/Home/bin/"
MAC_APP_RESOURCE_DIR="${MAC_APP_DIR}/Contents/Resources/"
MAC_APP_FRAMEWORK_DIR="${MAC_APP_DIR}/Contents/Frameworks/"
MAC_DMG_FILE_NAME="$1.dmg"
ARCH="$2"

run_codesign()
{
    file=$1
    echo ${file}
    #codesign --deep --force --timestamp --options runtime --sign "Developer ID Application: Cameron White (D5G6C56TBH)" --entitlements entitlements.plist ${file}
}

rm -rf ${MAC_APP_BIN_DIR}
rm -rf ${MAC_APP_FRAMEWORK_DIR}
mkdir -p ${MAC_APP_BIN_DIR} ${MAC_APP_FRAMEWORK_DIR}

dotnet publish ../../src/Chronojump-mac.sln -p:BuildTranslations=true --configuration Release -r osx-${ARCH} --self-contained true -o ${MAC_APP_BIN_DIR}
cd ../../src/
sh post-build-mac.sh ../package/macos/app/Chronojump.app/Contents/Home/bin
cp ../package/macos/app/Chronojump.app/Contents/Home/bin/runtimes/osx-${ARCH}/native/SQLite.Interop.dll ../package/macos/app/Chronojump.app/Contents/Home/bin/SQLite.Interop.dll
cd ../package/macos

# Remove stuff we don't need.
rm ${MAC_APP_BIN_DIR}/*.pdb

# Install the GTK dependencies.
echo "Bundling GTK..."
chmod +x bundle_gtk.py
./bundle_gtk.py --resource_dir ${MAC_APP_FRAMEWORK_DIR}/gtk3
# Add the GTK lib dir to the library search path (for dlopen()), as an alternative to $DYLD_LIBRARY_PATH.
install_name_tool -add_rpath "@executable_path/../Frameworks/gtk3/lib" ${MAC_APP_BIN_DIR}/Chronojump

touch ${MAC_APP_DIR}

# Sign the GTK binaries.
echo "Signing..."
for lib in `find ${MAC_APP_FRAMEWORK_DIR} -name \*.dylib -or -name \*.so -or -name \*.dll`
do
    run_codesign ${lib}
done

for lib in `find ${MAC_APP_RESOURCE_DIR} -name \*.dylib -or -name \*.so -or -name \*.dll`
do
    run_codesign ${lib}
done

for lib in `find ${MAC_APP_BIN_DIR} -name \*.dylib -or -name \*.so -or -name \*.dll`
do
    run_codesign ${lib}
done

# Sign the main executable and .NET stuff.
run_codesign ${MAC_APP_DIR}

# Create and sign the .dmg image, and include a link to drag the app into /Applications
echo "Creating dmg..."
#ln -s /Applications ${MAC_APP_ROOT_DIR}/Applications
#hdiutil create -quiet -srcFolder package -volname "${MAC_DMG_FILE_NAME} Installer" -o ${MAC_DMG_FILE_NAME}
hdiutil create -volname "${MAC_DMG_FILE_NAME} Installer" -srcfolder app -ov -format UDZO ${MAC_DMG_FILE_NAME}
run_codesign ${MAC_DMG_FILE_NAME}

# Notarize
#echo "Notarizing..."
#xcrun notarytool submit --wait --apple-id=cameronwhite91@gmail.com --password ${MAC_DEV_PASSWORD} --team-id D5G6C56TBH ${MAC_DMG_FILE_NAME}

# Staple the result to the dmg
#echo "Stapling..."
#xcrun stapler staple ${MAC_DMG_FILE_NAME}