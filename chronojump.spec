Name:           chronojump
Version:        0.8.10
Release:        2%{?dist}
Summary:        A measurement, management and statistics sport testing tool

Group:          Applications/Engineering
License:        GPLv2+
URL:            http://chronojump.org
Source0:        http://ftp.gnome.org/pub/GNOME/sources/chronojump/0.8/%{name}-%{version}.tar.gz
BuildRoot:      %{_tmppath}/%{name}-%{version}-%{release}-root-%(%{__id_u} -n)

BuildRequires:  pkgconfig mono-data-sqlite gtk-sharp2 gtk-sharp2-devel desktop-file-utils gettext mono-devel
Requires:       R-core hicolor-icon-theme

%description
ChronoJump is an open hardware, free software, multiplatform complete system
for measurement, management and statistics of sport short-time tests.

Chronojump uses a contact platform and/or photocells, 
and also a chronometer printed circuit designed ad-hoc in
order to obtain precise and trustworthy measurements.

Chronojump is used by trainers, teachers and students.

%package        doc
Summary:        ChronoJump manuals
Group:          Documentation
Requires:       %{name} = %{version}-%{release}
BuildArch:      noarch

%description doc
ChronoJump is an open hardware, free software, multiplatform complete system
for measurement, management and statistics of sport short-time tests.

These are the manuals for ChronoJump

%prep
%setup -q


%build
%configure
make %{?_smp_mflags}

cat > src/chronojump <<EOF
#!/bin/sh

exec mono "%{_libdir}/chronojump/Chronojump.exe" "\$@"
EOF

%install
rm -rf %{buildroot}
make install DESTDIR=%{buildroot}

# this file should be in the standard dir
rm %{buildroot}/%{_datadir}/doc/chronojump/chronojump_manual_es.pdf

# removing non used files:
rm %{buildroot}/%{_libdir}/chronojump/libchronopic.a
rm %{buildroot}/%{_libdir}/chronojump/libchronopic.la

desktop-file-install --dir=%{buildroot}%{_datadir}/applications/   %{buildroot}%{_datadir}/applications/chronojump.desktop

%find_lang %{name}

%clean
rm -rf %{buildroot}

%post
touch --no-create %{_datadir}/icons/hicolor &>/dev/null || :
update-desktop-database &> /dev/null || :

%postun
if [ $1 -eq 0 ] ; then
    touch --no-create %{_datadir}/icons/hicolor &>/dev/null
    gtk-update-icon-cache %{_datadir}/icons/hicolor &>/dev/null || :
fi
update-desktop-database &> /dev/null || :

%posttrans
gtk-update-icon-cache %{_datadir}/icons/hicolor &>/dev/null || :


%files -f %{name}.lang
%defattr(-,root,root,-)
%{_bindir}/chronojump
%{_bindir}/chronojump_mini
%{_bindir}/chronojump-test-accuracy
%{_bindir}/chronojump-test-jumps
%{_bindir}/chronojump-test-stream
%dir %{_libdir}/chronojump
%{_libdir}/chronojump/*
%dir %{_datadir}/chronojump
%{_datadir}/chronojump/*
%{_datadir}/icons/hicolor/48x48/apps/chronojump.png
%{_datadir}/applications/chronojump.desktop

%doc README COPYING AUTHORS 

%files doc
%defattr(-,root,root,-)
%doc manual/chronojump_manual_es.pdf

%changelog

* Tue Sep 22 2009 <ismael@olea.org> 0.8.10-2
- fixing suggestions from https://bugzilla.redhat.com/show_bug.cgi?id=524707#c3
- added forgotten update-desktop-database calls

* Mon Sep 21 2009 <ismael@olea.org> 0.8.10-1
- update to 0.8.10

* Thu Aug 27 2009 <ismael@olea.org> 0.8.9.5-3
- added doc subpackage

* Wed Aug 26 2009 <ismael@olea.org> 0.8.9.5-2
- minor spec typos
- Use %%find_lang.
- added ldconfig invocation
- removed libchronopic.la

* Tue Aug 25 2009 <ismael@olea.org> 0.8.9.5-1
- first release
