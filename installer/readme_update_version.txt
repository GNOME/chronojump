How to update version with installJammer:

-create_release.sh

at installjammer:
-Application information:
	+change install version
	+change version string
-Groups and files:
	+add new release dir
	+delete old release dir
	+fix new release dir:
		-uncheck svn dirs (better not for not confusing and losing time)
		-uncheck tar.gz, zip and readme
		-write an alias for linux/findMonoVersion.sh: "findMonoVersionLinux" (remember to press 'Enter')
-if mono for windows changed, do:
  1 change mono at groups and files, mono windows (and with the alias "monoInstallerWindows" (enter)
  2 change Virtual Text Strings MonoWindowsMinimumVersion 


if want to update some files but not version, it needs also to create_release.sh because files copied will be the found on release dir and not in build dir.
Then do the create_release.sh if you don't put the version, it will suggest version found in version.txt, but delete first the release dir of that version because, if not, create_release will not copy anything)
but this is an OBSCURE method that should'nt be used. a new version name is the correct thing
