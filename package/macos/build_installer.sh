#!/bin/sh
set -e

MAC_APP_ROOT_DIR=app
MAC_APP_DIR="${MAC_APP_ROOT_DIR}/Chronojump.app"
MAC_APP_BIN_DIR="${MAC_APP_DIR}/Contents/MacOS/"
MAC_APP_RESOURCE_DIR="${MAC_APP_DIR}/Contents/Resources/"
#MAC_APP_FRAMEWORK_DIR="${MAC_APP_DIR}/Contents/Frameworks/"
PACKAGE_VERSION="$1"
PACKAGE_TYPE="$2"
ARCH="$(uname -m)"
if [ "$ARCH" == "arm64" ]; then  
    ARCH="arm64"
else
    ARCH="x64"
fi
MAC_INSTALLER_FILE_NAME="chronojump-${PACKAGE_VERSION}-${ARCH}.${PACKAGE_TYPE}"

PYTHON_VERSION=3.12
R_VERSION=4.3

APPLE_ID=info@chronojump.org
APPLE_PASSWORD=mylc-ghhj-zfxg-weta
APPLE_TEAM_ID=RXJZ6LH5L4
APPLE_APPLICATION_CERT_NAME="Developer ID Application: Asociacion Chronojump (RXJZ6LH5L4)"
APPLE_INSTALLER_CERT_NAME="Developer ID Installer: Asociacion Chronojump (RXJZ6LH5L4)"
MAC_APPLICATION_CERT_NAME="3rd Party Mac Developer Application: Asociacion Chronojump (RXJZ6LH5L4)"
MAC_INSTALLER_CERT_NAME="3rd Party Mac Developer Installer: Asociacion Chronojump (RXJZ6LH5L4)"
if [ "$PACKAGE_TYPE" == "dmg" ]; then  
    APPLICATION_CERT_NAME="${APPLE_APPLICATION_CERT_NAME}"
    INSTALLER_CERT_NAME="${APPLE_INSTALLER_CERT_NAME}"
else
    #APPLICATION_CERT_NAME="${MAC_APPLICATION_CERT_NAME}"
    #INSTALLER_CERT_NAME="${MAC_INSTALLER_CERT_NAME}"
    APPLICATION_CERT_NAME="${APPLE_APPLICATION_CERT_NAME}"
    INSTALLER_CERT_NAME="${APPLE_INSTALLER_CERT_NAME}"
fi

run_codesign()
{
    file=$1
    echo ${file}
    codesign --deep --force --timestamp --options runtime --sign "${APPLICATION_CERT_NAME}" --entitlements chronojump.entitlements ${file}
    #codesign --deep --force --timestamp=none --options runtime --sign "${APPLICATION_CERT_NAME}" --entitlements chronojump.entitlements ${file}
}

#for dir in `find deps/bin/x64/Python -type d -name "*.app"`
#do
#    rm -rf ${dir}
#done
#for dir in `find deps/bin/x64/Python -type d -name "*.dSYM"`
#do
#    rm -rf ${dir}
#done
#for dir in `find deps/R -type d -name "*.app"`
#do
#    rm -rf ${dir}
#done
#for dir in `find deps/R -type d -name "*.dSYM"`
#do
#    rm -rf ${dir}
#done

#for dir in `find app -mindepth 1 -maxdepth 1 -type d ! -name "Chronojump.app"`
#do
#    rm -rf ${dir}
#done

for dir in `find deps -type d -name "*.app"`
do
    rm -rf ${dir}
done
for dir in `find deps -type d -name "*.dSYM"`
do
    rm -rf ${dir}
done
for dir in `find deps -type d -name "_CodeSignature"`
do
    rm -rf ${dir}
done
rm -rf ${MAC_APP_BIN_DIR}
#rm -rf ${MAC_APP_FRAMEWORK_DIR}
mkdir -p ${MAC_APP_BIN_DIR}
#mkdir ${MAC_APP_FRAMEWORK_DIR}
rm -rf ${MAC_APP_RESOURCE_DIR}gtk3

dotnet publish ../../src/Chronojump-mac.sln -p:BuildTranslations=true --configuration Release -r osx-${ARCH} --self-contained true -o ${MAC_APP_BIN_DIR}
cd ../../src/
dos2unix post-build-mac.sh
chmod +x post-build-mac.sh
sh post-build-mac.sh ../package/macos/app/Chronojump.app/Contents/MacOS
#cp ../package/macos/app/Chronojump.app/Contents/MacOS/runtimes/osx-${ARCH}/native/SQLite.Interop.dll ../package/macos/app/Chronojump.app/Contents/MacOS/SQLite.Interop.dll
cp ../package/macos/deps/runtimes/osx-${ARCH}/native/SQLite.Interop.dll ../package/macos/app/Chronojump.app/Contents/MacOS/SQLite.Interop.dll
cd ../package/macos
#cp ../../binariesMac/7zz ${MAC_APP_BIN_DIR}/bin
#cp ../../binariesMac/ffmpeg ${MAC_APP_BIN_DIR}/bin
#cp ../../binariesMac/ffplay ${MAC_APP_BIN_DIR}/bin
rm -rf ${MAC_APP_RESOURCE_DIR}Python-${ARCH}
rm -rf ${MAC_APP_RESOURCE_DIR}R-${ARCH}
mv ${MAC_APP_BIN_DIR}bin/x64/Python ${MAC_APP_RESOURCE_DIR}Python-${ARCH}
mv ${MAC_APP_BIN_DIR}R ${MAC_APP_RESOURCE_DIR}R-${ARCH}

#TODO: note these cp are for x64, change it to work also on arm64.
#Note also joeries has python 3.11
#No need to add these commands as Python files would be copied from /Library/Frameworks/Python.framework automatically.
#mkdir -p app/Chronojump.app/Contents/MacOS/bin/x64/Python/Versions/3.12/lib
#mkdir -p app/Chronojump.app/Contents/MacOS/bin/x64/Python/Versions/3.12/lib/python3.12/config-3.12-darwin
#mkdir -p app/Chronojump.app/Contents/MacOS/bin/x64/Python/Versions/Current/lib
#mkdir -p app/Chronojump.app/Contents/MacOS/bin/x64/Python/Versions/Current/lib/python3.12/config-3.12-darwin
#cp deps/bin/x64/Python/Versions/3.12/lib/libpython3.12.dylib app/Chronojump.app/Contents/MacOS/bin/x64/Python/Versions/3.12/lib
#cp deps/bin/x64/Python/Versions/3.12/lib/python3.12/config-3.12-darwin/libpython3.12.dylib app/Chronojump.app/Contents/MacOS/bin/x64/Python/Versions/3.12/lib/python3.12/config-3.12-darwin
#cp deps/bin/x64/Python/Versions/Current/lib/libpython3.12.dylib app/Chronojump.app/Contents/MacOS/bin/x64/Python/Versions/Current/lib
#cp deps/bin/x64/Python/Versions/Current/lib/python3.12/config-3.12-darwin/libpython3.12.dylib app/Chronojump.app/Contents/MacOS/bin/x64/Python/Versions/Current/lib/python3.12/config-3.12-darwin
#cp deps/bin/x64/Python/Versions/Current/Python app/Chronojump.app/Contents/MacOS/bin/x64/Python/Versions/Current
#cp deps/bin/x64/Python/Versions/3.12/Python app/Chronojump.app/Contents/MacOS/bin/x64/Python/Versions/3.12

# Remove stuff we don't need.
rm ${MAC_APP_BIN_DIR}/*.pdb

# Install the GTK dependencies.
echo "Bundling GTK..."

# Get OS Version
#os_version=$(sw_vers -productVersion)    
# Check whether 10.x
#if echo "$os_version" | grep -q '10\.[0-9]\+\..*'; then
#    chmod +x bundle_gtk_osx10.py
#    ./bundle_gtk_osx10.py --resource_dir ${MAC_APP_RESOURCE_DIR}/gtk3
#else
#    chmod +x bundle_gtk.py
#    ./bundle_gtk.py --resource_dir ${MAC_APP_RESOURCE_DIR}/gtk3
#fi

if [ -e "/usr/local/lib/libglib-2.0.0.dylib" ]; then
    dos2unix bundle_gtk_usr_local.py
    chmod +x bundle_gtk_usr_local.py
    ./bundle_gtk_usr_local.py --resource_dir ${MAC_APP_RESOURCE_DIR}/gtk3
else
    dos2unix bundle_gtk_opt_homebrew.py
    chmod +x bundle_gtk_opt_homebrew.py
    ./bundle_gtk_opt_homebrew.py --resource_dir ${MAC_APP_RESOURCE_DIR}/gtk3
fi

# Add the GTK lib dir to the library search path (for dlopen()), as an alternative to $DYLD_LIBRARY_PATH.
install_name_tool -add_rpath "@executable_path/../Resources/gtk3/lib" ${MAC_APP_BIN_DIR}/Chronojump
chmod +w ${MAC_APP_RESOURCE_DIR}/gtk3/lib/gdk-pixbuf-2.0/2.10.0/loaders/libpixbufloader-svg.a

touch ${MAC_APP_DIR}

# Sign the GTK binaries.
echo "Signing..."
#for lib in `find ${MAC_APP_FRAMEWORK_DIR} -name \*.dylib -or -name \*.so -or -name \*.dll -or -name \*.a`
#do
#    run_codesign ${lib}
#done

for lib in `find ${MAC_APP_RESOURCE_DIR} -name \*.dylib -or -name \*.so -or -name \*.dll -or -name \*.a`
do
    run_codesign ${lib}
done

for lib in `find ${MAC_APP_BIN_DIR} -name \*.dylib -or -name \*.so -or -name \*.dll -or -name \*.a`
do
    run_codesign ${lib}
done

#for lib in `find ${MAC_APP_BIN_DIR} -name \*.a`
#do
#    run_codesign ${lib}
#done

run_codesign ${MAC_APP_BIN_DIR}/createdump
run_codesign ${MAC_APP_BIN_DIR}/bin/ffplay
run_codesign ${MAC_APP_BIN_DIR}/bin/ffmpeg
run_codesign ${MAC_APP_BIN_DIR}/bin/7zz
run_codesign ${MAC_APP_BIN_DIR}/libcoreclr.dylib
run_codesign ${MAC_APP_BIN_DIR}/libSystem.Native.dylib
run_codesign ${MAC_APP_BIN_DIR}/libSystem.IO.Ports.Native.dylib
run_codesign ${MAC_APP_BIN_DIR}/libSystem.IO.Compression.Native.dylib
run_codesign ${MAC_APP_BIN_DIR}/libSystem.Globalization.Native.dylib
run_codesign ${MAC_APP_BIN_DIR}/libSystem.Security.Cryptography.Native.Apple.dylib
run_codesign ${MAC_APP_BIN_DIR}/libmscordaccore.dylib
run_codesign ${MAC_APP_BIN_DIR}/libhostfxr.dylib
run_codesign ${MAC_APP_BIN_DIR}/libSystem.Net.Security.Native.dylib
run_codesign ${MAC_APP_BIN_DIR}/libmscordbi.dylib
run_codesign ${MAC_APP_BIN_DIR}/libhostpolicy.dylib
run_codesign ${MAC_APP_BIN_DIR}/libSystem.Security.Cryptography.Native.OpenSsl.dylib
run_codesign ${MAC_APP_BIN_DIR}/libclrjit.dylib
run_codesign ${MAC_APP_BIN_DIR}/libclrgc.dylib
run_codesign ${MAC_APP_RESOURCE_DIR}Python-${ARCH}/Versions/${PYTHON_VERSION}/lib/libpython${PYTHON_VERSION}.dylib
run_codesign ${MAC_APP_RESOURCE_DIR}Python-${ARCH}/Versions/${PYTHON_VERSION}/lib/python${PYTHON_VERSION}/config-${PYTHON_VERSION}-darwin/libpython${PYTHON_VERSION}.dylib
run_codesign ${MAC_APP_RESOURCE_DIR}Python-${ARCH}/Versions/Current/lib/libpython${PYTHON_VERSION}.dylib
run_codesign ${MAC_APP_RESOURCE_DIR}Python-${ARCH}/Versions/Current/lib/python${PYTHON_VERSION}/config-${PYTHON_VERSION}-darwin/libpython${PYTHON_VERSION}.dylib
run_codesign ${MAC_APP_RESOURCE_DIR}Python-${ARCH}/Versions/Current/Python
run_codesign ${MAC_APP_RESOURCE_DIR}Python-${ARCH}/Versions/${PYTHON_VERSION}/Python
run_codesign ${MAC_APP_RESOURCE_DIR}Python-${ARCH}/Python

run_codesign ${MAC_APP_RESOURCE_DIR}Python-${ARCH}/Versions/${PYTHON_VERSION}/bin/python3
run_codesign ${MAC_APP_RESOURCE_DIR}Python-${ARCH}/Versions/${PYTHON_VERSION}/bin/python3-intel64
run_codesign ${MAC_APP_RESOURCE_DIR}Python-${ARCH}/Versions/${PYTHON_VERSION}/bin/python${PYTHON_VERSION}
run_codesign ${MAC_APP_RESOURCE_DIR}Python-${ARCH}/Versions/${PYTHON_VERSION}/bin/python${PYTHON_VERSION}-intel64
run_codesign ${MAC_APP_RESOURCE_DIR}Python-${ARCH}/Versions/Current/bin/python3
run_codesign ${MAC_APP_RESOURCE_DIR}Python-${ARCH}/Versions/Current/bin/python3-intel64
run_codesign ${MAC_APP_RESOURCE_DIR}Python-${ARCH}/Versions/Current/bin/python${PYTHON_VERSION}
run_codesign ${MAC_APP_RESOURCE_DIR}Python-${ARCH}/Versions/Current/bin/python${PYTHON_VERSION}-intel64
run_codesign ${MAC_APP_RESOURCE_DIR}R-${ARCH}/Resources/Rscript
run_codesign ${MAC_APP_RESOURCE_DIR}R-${ARCH}/Resources/bin/Rscript
run_codesign ${MAC_APP_RESOURCE_DIR}R-${ARCH}/Resources/bin/exec/R
run_codesign ${MAC_APP_RESOURCE_DIR}R-${ARCH}/Resources/bin/fc-cache
run_codesign ${MAC_APP_RESOURCE_DIR}R-${ARCH}/Resources/bin/qpdf
run_codesign ${MAC_APP_RESOURCE_DIR}R-${ARCH}/Versions/${R_VERSION}-${ARCH}/Resources/Rscript
run_codesign ${MAC_APP_RESOURCE_DIR}R-${ARCH}/Versions/${R_VERSION}-${ARCH}/Resources/bin/Rscript
run_codesign ${MAC_APP_RESOURCE_DIR}R-${ARCH}/Versions/${R_VERSION}-${ARCH}/Resources/bin/exec/R
run_codesign ${MAC_APP_RESOURCE_DIR}R-${ARCH}/Versions/${R_VERSION}-${ARCH}/Resources/bin/fc-cache
run_codesign ${MAC_APP_RESOURCE_DIR}R-${ARCH}/Versions/${R_VERSION}-${ARCH}/Resources/bin/qpdf
run_codesign ${MAC_APP_RESOURCE_DIR}R-${ARCH}/Versions/Current/Resources/Rscript
run_codesign ${MAC_APP_RESOURCE_DIR}R-${ARCH}/Versions/Current/Resources/bin/Rscript
run_codesign ${MAC_APP_RESOURCE_DIR}R-${ARCH}/Versions/Current/Resources/bin/exec/R
run_codesign ${MAC_APP_RESOURCE_DIR}R-${ARCH}/Versions/Current/Resources/bin/fc-cache
run_codesign ${MAC_APP_RESOURCE_DIR}R-${ARCH}/Versions/Current/Resources/bin/qpdf

#mv ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/Resources/English.lproj ${MAC_APP_ROOT_DIR}/1_English.lproj
#mv ${MAC_APP_BIN_DIR}bin/x64/Python/Resources/English.lproj ${MAC_APP_ROOT_DIR}/2_English.lproj
#mv ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/lib/tdbcmysql1.1.5 ${MAC_APP_ROOT_DIR}/tdbcmysql1.1.5
#mv ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/lib/thread2.8.8 ${MAC_APP_ROOT_DIR}/thread2.8.8
#mv ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/lib/tdbcodbc1.1.5 ${MAC_APP_ROOT_DIR}/tdbcodbc1.1.5
#mv ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/lib/tk8.6 ${MAC_APP_ROOT_DIR}/tk8.6
#mv ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/lib/itcl4.2.3 ${MAC_APP_ROOT_DIR}/itcl4.2.3
#mv ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/lib/sqlite3.40.0 ${MAC_APP_ROOT_DIR}/sqlite3.40.0
#mv ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/lib/tcl8.6 ${MAC_APP_ROOT_DIR}/tcl8.6
#mv ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/lib/tdbc1.1.5 ${MAC_APP_ROOT_DIR}/tdbc1.1.5
#mv ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/lib/tcl8 ${MAC_APP_ROOT_DIR}/tcl8
#mv ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/lib/python${PYTHON_VERSION} ${MAC_APP_ROOT_DIR}/1_python${PYTHON_VERSION}
#mv ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/share/doc/python${PYTHON_VERSION} ${MAC_APP_ROOT_DIR}/2_python${PYTHON_VERSION}
#mv ${MAC_APP_BIN_DIR}R/Resources/fontconfig/fontconfig/conf.avail ${MAC_APP_ROOT_DIR}/1_conf.avail
#mv ${MAC_APP_BIN_DIR}R/Resources/fontconfig/fonts/conf.d ${MAC_APP_ROOT_DIR}/1_conf.d
#mv ${MAC_APP_BIN_DIR}R/Versions/Current/Resources/fontconfig/fontconfig/conf.avail ${MAC_APP_ROOT_DIR}/2_conf.avail
#mv ${MAC_APP_BIN_DIR}R/Versions/Current/Resources/fontconfig/fonts/conf.d ${MAC_APP_ROOT_DIR}/2_conf.d
#mv ${MAC_APP_BIN_DIR}fonts/conf.d ${MAC_APP_ROOT_DIR}/3_conf.d
#mv ${MAC_APP_FRAMEWORK_DIR}gtk3/lib/gdk-pixbuf-2.0 ${MAC_APP_ROOT_DIR}/gdk-pixbuf-2.0
#mv ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/include/python${PYTHON_VERSION} ${MAC_APP_ROOT_DIR}/3_python${PYTHON_VERSION}
#mv ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/lib/tdbcpostgres1.1.5 ${MAC_APP_ROOT_DIR}/tdbcpostgres1.1.5
#mv ${MAC_APP_FRAMEWORK_DIR}gtk3/share ${MAC_APP_ROOT_DIR}/share
#mv ${MAC_APP_FRAMEWORK_DIR}gtk3/lib/gtk-3.0 ${MAC_APP_ROOT_DIR}/gtk-3.0

# Sign the main executable and .NET stuff.
run_codesign ${MAC_APP_BIN_DIR}/Chronojump
#run_codesign ${MAC_APP_DIR}

#mv ${MAC_APP_ROOT_DIR}/1_English.lproj ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/Resources/English.lproj
#mv ${MAC_APP_ROOT_DIR}/2_English.lproj ${MAC_APP_BIN_DIR}bin/x64/Python/Resources/English.lproj
#mv ${MAC_APP_ROOT_DIR}/tdbcmysql1.1.5 ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/lib/tdbcmysql1.1.5
#mv ${MAC_APP_ROOT_DIR}/thread2.8.8 ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/lib/thread2.8.8
#mv ${MAC_APP_ROOT_DIR}/tdbcodbc1.1.5 ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/lib/tdbcodbc1.1.5
#mv ${MAC_APP_ROOT_DIR}/tk8.6 ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/lib/tk8.6
#mv ${MAC_APP_ROOT_DIR}/itcl4.2.3 ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/lib/itcl4.2.3
#mv ${MAC_APP_ROOT_DIR}/sqlite3.40.0 ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/lib/sqlite3.40.0
#mv ${MAC_APP_ROOT_DIR}/tcl8.6 ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/lib/tcl8.6
#mv ${MAC_APP_ROOT_DIR}/tdbc1.1.5 ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/lib/tdbc1.1.5
#mv ${MAC_APP_ROOT_DIR}/tcl8 ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/lib/tcl8
#mv ${MAC_APP_ROOT_DIR}/1_python${PYTHON_VERSION} ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/lib/python${PYTHON_VERSION}
#mv ${MAC_APP_ROOT_DIR}/2_python${PYTHON_VERSION} ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/share/doc/python${PYTHON_VERSION}
#mv ${MAC_APP_ROOT_DIR}/1_conf.avail ${MAC_APP_BIN_DIR}R/Resources/fontconfig/fontconfig/conf.avail
#mv ${MAC_APP_ROOT_DIR}/1_conf.d ${MAC_APP_BIN_DIR}R/Resources/fontconfig/fonts/conf.d
#mv ${MAC_APP_ROOT_DIR}/2_conf.avail ${MAC_APP_BIN_DIR}R/Versions/Current/Resources/fontconfig/fontconfig/conf.avail
#mv ${MAC_APP_ROOT_DIR}/2_conf.d ${MAC_APP_BIN_DIR}R/Versions/Current/Resources/fontconfig/fonts/conf.d
#mv ${MAC_APP_ROOT_DIR}/3_conf.d ${MAC_APP_BIN_DIR}fonts/conf.d
#mv ${MAC_APP_ROOT_DIR}/gdk-pixbuf-2.0 ${MAC_APP_FRAMEWORK_DIR}gtk3/lib/gdk-pixbuf-2.0
#mv ${MAC_APP_ROOT_DIR}/3_python${PYTHON_VERSION} ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/include/python${PYTHON_VERSION}
#mv ${MAC_APP_ROOT_DIR}/tdbcpostgres1.1.5 ${MAC_APP_BIN_DIR}bin/x64/Python/Versions/Current/lib/tdbcpostgres1.1.5
#mv ${MAC_APP_ROOT_DIR}/share ${MAC_APP_FRAMEWORK_DIR}gtk3/share
#mv ${MAC_APP_ROOT_DIR}/gtk-3.0 ${MAC_APP_FRAMEWORK_DIR}gtk3/lib/gtk-3.0

if [ "$PACKAGE_TYPE" == "dmg" ]; then  
    # Create and sign the .dmg image, and include a link to drag the app into /Applications
    echo "Creating dmg..."
    #ln -s /Applications ${MAC_APP_ROOT_DIR}/Applications
    #hdiutil create -quiet -srcFolder package -volname "${MAC_INSTALLER_FILE_NAME} Installer" -o ${MAC_INSTALLER_FILE_NAME}
    hdiutil create -volname "${MAC_INSTALLER_FILE_NAME} Installer" -srcfolder app -ov -format UDZO ${MAC_INSTALLER_FILE_NAME}
    run_codesign ${MAC_INSTALLER_FILE_NAME}

    # Notarize
    echo "Notarizing dmg..."
    xcrun notarytool submit --wait --apple-id=${APPLE_ID} --password ${APPLE_PASSWORD} --team-id ${APPLE_TEAM_ID} ${MAC_INSTALLER_FILE_NAME}

    # Staple the result to the dmg
    echo "Stapling dmg..."
    xcrun stapler staple ${MAC_INSTALLER_FILE_NAME}
else  
    mkdir -p component-signing
    mkdir -p component
    mkdir -p distribution-signing
    echo "Creating component pkg..."
    pkgbuild --root app/Chronojump.app --install-location /Applications/Chronojump.app --version "${PACKAGE_VERSION}-${ARCH}" --identifier org.chronojump.chronojump "component-signing/${MAC_INSTALLER_FILE_NAME}"
    echo "Signing component pkg..."
    productsign --timestamp --sign "${INSTALLER_CERT_NAME}" "component-signing/${MAC_INSTALLER_FILE_NAME}" "component/${MAC_INSTALLER_FILE_NAME}"
    echo "Creating distribution pkg..."
    productbuild --package "component/${MAC_INSTALLER_FILE_NAME}" "distribution-signing/${MAC_INSTALLER_FILE_NAME}"
    #echo "Creating distribution pkg..."
    #productbuild --component app/Chronojump.app /Applications "distribution-signing/${MAC_INSTALLER_FILE_NAME}"
    echo "Signing distribution pkg..."
    productsign --timestamp --sign "${INSTALLER_CERT_NAME}" "distribution-signing/${MAC_INSTALLER_FILE_NAME}" ${MAC_INSTALLER_FILE_NAME}
    rm -rf distribution-signing
    rm -rf component
    rm -rf component-signing
    echo "Notarizing pkg..."
    xcrun notarytool submit --wait --apple-id=${APPLE_ID} --password ${APPLE_PASSWORD} --team-id ${APPLE_TEAM_ID} ${MAC_INSTALLER_FILE_NAME}
    echo "Stapling pkg..."
    xcrun stapler staple ${MAC_INSTALLER_FILE_NAME}
fi
