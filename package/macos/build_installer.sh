#!/bin/sh
set -e

MAC_APP_ROOT_DIR=app
MAC_APP_DIR="${MAC_APP_ROOT_DIR}/Chronojump.app"
MAC_APP_BIN_DIR="${MAC_APP_DIR}/Contents/Home/bin/"
MAC_APP_RESOURCE_DIR="${MAC_APP_DIR}/Contents/Resources/"
MAC_APP_FRAMEWORK_DIR="${MAC_APP_DIR}/Contents/Frameworks/"
ARCH="$2"
MAC_DMG_FILE_NAME="$1-${ARCH}.dmg"
#MAC_PKG_FILE_NAME="$1-${ARCH}.pkg"

PYTHON_VERSION=3.12

APPLE_ID=info@chronojump.org
APPLE_PASSWORD=mylc-ghhj-zfxg-weta
APPLE_TEAM_ID=RXJZ6LH5L4
APPLE_APPLICATION_CERT_NAME="Developer ID Application: Asociacion Chronojump (RXJZ6LH5L4)"
APPLE_INSTALLER_CERT_NAME="Developer ID Installer: Asociacion Chronojump (RXJZ6LH5L4)"

run_codesign()
{
    file=$1
    echo ${file}
    codesign --deep --force --timestamp --options runtime --sign "${APPLE_APPLICATION_CERT_NAME}" --entitlements entitlements.plist ${file}
}

rm -rf ${MAC_APP_BIN_DIR}
rm -rf ${MAC_APP_FRAMEWORK_DIR}
mkdir -p ${MAC_APP_BIN_DIR} ${MAC_APP_FRAMEWORK_DIR}

dotnet publish ../../src/Chronojump-mac.sln -p:BuildTranslations=true --configuration Release -r osx-${ARCH} --self-contained true -o ${MAC_APP_BIN_DIR}
cd ../../src/
dos2unix post-build-mac.sh
chmod +x post-build-mac.sh
sh post-build-mac.sh ../package/macos/app/Chronojump.app/Contents/Home/bin
#cp ../package/macos/app/Chronojump.app/Contents/Home/bin/runtimes/osx-${ARCH}/native/SQLite.Interop.dll ../package/macos/app/Chronojump.app/Contents/Home/bin/SQLite.Interop.dll
cp ../package/macos/deps/runtimes/osx-${ARCH}/native/SQLite.Interop.dll ../package/macos/app/Chronojump.app/Contents/Home/bin/SQLite.Interop.dll
cd ../package/macos
cp ../../binariesMac/7zz app/Chronojump.app/Contents/Home/bin/bin
cp ../../binariesMac/ffmpeg app/Chronojump.app/Contents/Home/bin/bin
cp ../../binariesMac/ffplay app/Chronojump.app/Contents/Home/bin/bin

#TODO: note these cp are for x64, change it to work also on arm64.
#Note also joeries has python 3.11
#No need to add these commands as Python files would be copied from /Library/Frameworks/Python.framework automatically.
#mkdir -p app/Chronojump.app/Contents/Home/bin/bin/x64/Python/Versions/3.12/lib
#mkdir -p app/Chronojump.app/Contents/Home/bin/bin/x64/Python/Versions/3.12/lib/python3.12/config-3.12-darwin
#mkdir -p app/Chronojump.app/Contents/Home/bin/bin/x64/Python/Versions/Current/lib
#mkdir -p app/Chronojump.app/Contents/Home/bin/bin/x64/Python/Versions/Current/lib/python3.12/config-3.12-darwin
#cp deps/bin/x64/Python/Versions/3.12/lib/libpython3.12.dylib app/Chronojump.app/Contents/Home/bin/bin/x64/Python/Versions/3.12/lib
#cp deps/bin/x64/Python/Versions/3.12/lib/python3.12/config-3.12-darwin/libpython3.12.dylib app/Chronojump.app/Contents/Home/bin/bin/x64/Python/Versions/3.12/lib/python3.12/config-3.12-darwin
#cp deps/bin/x64/Python/Versions/Current/lib/libpython3.12.dylib app/Chronojump.app/Contents/Home/bin/bin/x64/Python/Versions/Current/lib
#cp deps/bin/x64/Python/Versions/Current/lib/python3.12/config-3.12-darwin/libpython3.12.dylib app/Chronojump.app/Contents/Home/bin/bin/x64/Python/Versions/Current/lib/python3.12/config-3.12-darwin
#cp deps/bin/x64/Python/Versions/Current/Python app/Chronojump.app/Contents/Home/bin/bin/x64/Python/Versions/Current
#cp deps/bin/x64/Python/Versions/3.12/Python app/Chronojump.app/Contents/Home/bin/bin/x64/Python/Versions/3.12

# Remove stuff we don't need.
rm ${MAC_APP_BIN_DIR}/*.pdb

# Install the GTK dependencies.
echo "Bundling GTK..."

# Get OS Version
#os_version=$(sw_vers -productVersion)    
# Check whether 10.x
#if echo "$os_version" | grep -q '10\.[0-9]\+\..*'; then
#    chmod +x bundle_gtk_osx10.py
#    ./bundle_gtk_osx10.py --resource_dir ${MAC_APP_FRAMEWORK_DIR}/gtk3
#else
#    chmod +x bundle_gtk.py
#    ./bundle_gtk.py --resource_dir ${MAC_APP_FRAMEWORK_DIR}/gtk3
#fi

if [ -e "/usr/local/lib/libglib-2.0.0.dylib" ]; then
    dos2unix bundle_gtk_osx10.py
    chmod +x bundle_gtk_osx10.py
    ./bundle_gtk_osx10.py --resource_dir ${MAC_APP_FRAMEWORK_DIR}/gtk3
else
    dos2unix bundle_gtk.py
    chmod +x bundle_gtk.py
    ./bundle_gtk.py --resource_dir ${MAC_APP_FRAMEWORK_DIR}/gtk3
fi

# Add the GTK lib dir to the library search path (for dlopen()), as an alternative to $DYLD_LIBRARY_PATH.
install_name_tool -add_rpath "@executable_path/../../Frameworks/gtk3/lib" ${MAC_APP_BIN_DIR}/Chronojump

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

for lib in `find ${MAC_APP_BIN_DIR} -name \*.a`
do
    run_codesign ${lib}
done

# Sign the main executable and .NET stuff.
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/Chronojump
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/createdump
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/bin/ffplay
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/bin/ffmpeg
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/bin/7zz
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/libcoreclr.dylib
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/libSystem.Native.dylib
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/libSystem.IO.Ports.Native.dylib
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/libSystem.IO.Compression.Native.dylib
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/libSystem.Globalization.Native.dylib
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/libSystem.Security.Cryptography.Native.Apple.dylib
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/libmscordaccore.dylib
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/libhostfxr.dylib
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/libSystem.Net.Security.Native.dylib
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/libmscordbi.dylib
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/libhostpolicy.dylib
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/libSystem.Security.Cryptography.Native.OpenSsl.dylib
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/libclrjit.dylib
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/libclrgc.dylib
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/bin/x64/Python/Versions/${PYTHON_VERSION}/lib/libpython${PYTHON_VERSION}.dylib
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/bin/x64/Python/Versions/${PYTHON_VERSION}/lib/python${PYTHON_VERSION}/config-${PYTHON_VERSION}-darwin/libpython${PYTHON_VERSION}.dylib
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/bin/x64/Python/Versions/Current/lib/libpython${PYTHON_VERSION}.dylib
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/bin/x64/Python/Versions/Current/lib/python${PYTHON_VERSION}/config-${PYTHON_VERSION}-darwin/libpython${PYTHON_VERSION}.dylib
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/bin/x64/Python/Versions/Current/Python
run_codesign ${MAC_APP_DIR}/Contents/Home/bin/bin/x64/Python/Versions/${PYTHON_VERSION}/Python

# Create and sign the .dmg image, and include a link to drag the app into /Applications
echo "Creating dmg..."
#ln -s /Applications ${MAC_APP_ROOT_DIR}/Applications
#hdiutil create -quiet -srcFolder package -volname "${MAC_DMG_FILE_NAME} Installer" -o ${MAC_DMG_FILE_NAME}
hdiutil create -volname "${MAC_DMG_FILE_NAME} Installer" -srcfolder app -ov -format UDZO ${MAC_DMG_FILE_NAME}
run_codesign ${MAC_DMG_FILE_NAME}

# Notarize
echo "Notarizing dmg..."
xcrun notarytool submit --wait --apple-id=${APPLE_ID} --password ${APPLE_PASSWORD} --team-id ${APPLE_TEAM_ID} ${MAC_DMG_FILE_NAME}

# Staple the result to the dmg
echo "Stapling dmg..."
xcrun stapler staple ${MAC_DMG_FILE_NAME}

#mkdir -p tmp
#echo "Creating pkg..."
#pkgbuild --root app/Chronojump.app --identifier org.chronojump.chronojump tmp/${MAC_PKG_FILE_NAME}
#echo "Signing pkg..."
#productsign --timestamp --sign "${APPLE_INSTALLER_CERT_NAME}" "tmp/${MAC_PKG_FILE_NAME}" "${MAC_PKG_FILE_NAME}"
#rm -rf tmp
#echo "Notarizing pkg..."
#xcrun notarytool submit --wait --apple-id=${APPLE_ID} --password ${APPLE_PASSWORD} --team-id ${APPLE_TEAM_ID} ${MAC_PKG_FILE_NAME}
#echo "Stapling pkg..."
#xcrun stapler staple ${MAC_PKG_FILE_NAME}