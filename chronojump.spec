Name:           chronojump
Version:        0.8.9.5
Release:        1%{?dist}
Summary:        A measurement, management and statistics sport testing tool

Group:          Applications/Engineering
License:        GPLv2+
URL:            http://chronojump.org
Source0:        http://ftp.gnome.org/pub/GNOME/sources/chronojump/0.8/%{name}-%{version}.tar.gz
BuildRoot:      %{_tmppath}/%{name}-%{version}-%{release}-root-%(%{__id_u} -n)

BuildRequires:  mono-core pkgconfig mono-data-sqlite gtk-sharp2 gtk-sharp2-devel desktop-file-utils gettext
Requires:       mono-core mono-data-sqlite gtk-sharp2

%description
ChronoJump is an open hardware, free software, multiplatform complete system
for measurement, management and statistics of sport short-time tests.

Chronojump uses a contact platform and/or photocells, 
and also a chronometer printed circuit designed ad-hoc in
order to obtain precise and trustworthy measurements.

Chronojump is used by trainers, teachers and students.



%prep
%setup -q


%build
%configure
make %{?_smp_mflags}


%install
rm -rf %{buildroot}
make install DESTDIR=%{buildroot}

# this file should be in the standard dir
rm %{buildroot}/%{_datadir}/doc/chronojump/chronojump_manual_es.pdf

# removing non used files:
rm %{buildroot}/%{_libdir}/chronojump/libchronopic.a

desktop-file-install --dir=%{buildroot}%{_datadir}/applications/   %{buildroot}%{_datadir}/applications/chronojump.desktop


%clean
rm -rf %{buildroot}


%files
%defattr(-,root,root,-)
%{_bindir}/chronojump
%{_bindir}/chronojump_mini
%{_bindir}/test-accuracy
%{_bindir}/test-jumps
%{_bindir}/test-stream
%dir %{_libdir}/chronojump
%{_libdir}/chronojump/*
%dir %{_datadir}/chronojump
%{_datadir}/chronojump/*
%{_datadir}/icons/hicolor/48x48/apps/chronojump.png
%{_datadir}/applications/chronojump.desktop

%lang(ar) %{_datadir}/locale/ar/LC_MESSAGES/chronojump.mo
%lang(ca) %{_datadir}/locale/ca/LC_MESSAGES/chronojump.mo
%lang(dz) %{_datadir}/locale/dz/LC_MESSAGES/chronojump.mo
%lang(en_GB) %{_datadir}/locale/en_GB/LC_MESSAGES/chronojump.mo
%lang(es) %{_datadir}/locale/es/LC_MESSAGES/chronojump.mo
%lang(fi) %{_datadir}/locale/fi/LC_MESSAGES/chronojump.mo
%lang(fr) %{_datadir}/locale/fr/LC_MESSAGES/chronojump.mo
%lang(nb) %{_datadir}/locale/nb/LC_MESSAGES/chronojump.mo
%lang(oc) %{_datadir}/locale/oc/LC_MESSAGES/chronojump.mo
%lang(pt) %{_datadir}/locale/pt/LC_MESSAGES/chronojump.mo
%lang(pt_BR) %{_datadir}/locale/pt_BR/LC_MESSAGES/chronojump.mo
%lang(sv) %{_datadir}/locale/sv/LC_MESSAGES/chronojump.mo
%lang(vi) %{_datadir}/locale/vi/LC_MESSAGES/chronojump.mo
%lang(zh_CN) %{_datadir}/locale/zh_CN/LC_MESSAGES/chronojump.mo

%doc README COPYING AUTHORS manual/chronojump_manual_es.pdf


%changelog

* Tue Aug 25 2009 <ismael@olea.org> 0.8.9.5-1
- first release
