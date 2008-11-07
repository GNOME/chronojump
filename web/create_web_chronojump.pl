#!/usr/bin/perl -w

#
# This file is part of ChronoJump
#
# Chronojump is free software; you can redistribute it and/or modify
#  it under the terms of the GNU General Public License as published by
#   the Free Software Foundation; either version 2 of the License, or   
#    (at your option) any later version.
#    
# Chronojump is distributed in the hope that it will be useful,
#  but WITHOUT ANY WARRANTY; without even the implied warranty of
#   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
#    GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
#  along with this program; if not, write to the Free Software
#   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
#
# Xavier de Blas: 
# http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
#

use strict;

#CONSTANTS
my $currentVersion = "0.7.5";
my $currentVersionDay = "1";
my $currentVersionMonth = 8; #if it's from 1 to 9, put only one digit
my $currentVersionYear = "2008";
my $linuxMailDownloadLink = "http://ftp.gnome.org/pub/GNOME/binaries/linux/chronojump/Chronojump-0.7.5-Linux-x86-Install";
my $linuxMailDownloadName = "Chronojump 0.7.5 Linux";
my $windowsMailDownloadLink = "http://ftp.gnome.org/pub/GNOME/binaries/win32/chronojump/Chronojump-0.7.5-Windows-Install.exe";
my $windowsMailDownloadName = "Chronojump 0.7.5 Windows";
my $maemoMailDownloadLink = "http://mail.gnome.org/archives/chronojump-list/2007-April/msg00002.html";
my $maemoMailDownloadName = "chronojump_mini for Maemo!! Chronojump_mini funcionando en Maemo!!";
my $changesMailDownloadLink = "http://mail.gnome.org/archives/chronojump-list/2008-August/msg00001.html";
my $manualLink = "http://svn.gnome.org/svn/chronojump/trunk/manual/chronojump_manual_es.pdf";
my $glossaryLink = "http://svn.gnome.org/svn/chronojump/trunk/glossary/chronojump_glossary_for_translators.html";

my $awards .= "http://tropheesdulibre.org";

my $construccio_dispositius_mesuraLink = "http://www.vimeo.com/1205809";
my $fonaments_teorics_bescos_velezLink = "http://ftp.gnome.org/pub/GNOME/teams/chronojump/fonaments_teorics-bescos-velez.avi";
my $fonaments_teorics_padullesLink = "http://ftp.gnome.org/pub/GNOME/teams/chronojump/fonaments_teorics-padulles.avi";
my $homenatge_carmelo_boscoLink = "http://ftp.gnome.org/pub/GNOME/teams/chronojump/homenatge_carmelo_bosco.avi";
my $instalacio_construccio_adquisicioLink = "http://ftp.gnome.org/pub/GNOME/teams/chronojump/instalacio_construccio_adquisicio.avi";
my $projecte_chronojumpLink = "http://www.vimeo.com/1198489";
my $us_chronojump_bescos_padullesLink = "http://ftp.gnome.org/pub/GNOME/teams/chronojump/us_chronojump-bescos-padulles.avi";


my $siteURL = "http://projects.gnome.org/chronojump";
#my $siteURL = ".";
my $CVSURL = "http://cvs.gnome.org/viewcvs/chronojump";

my %languages=();
my %photos_software=();
my %photos_hardware=();
my %photos_community=();

my $languagesFile = "languages.txt";
my $photosFile = "photos.txt";
my $authorsFile = "authors.txt";
my $colaborationsFile = "colaborations.txt";
my $contributorsFile = "contributors.txt",


my $authors = &getPeople($authorsFile);
my $colaborations = &getPeople($colaborationsFile);
my $contributors = &getPeople($contributorsFile);


#read languagesFile, contains languageName:suffix
open (LANGUAGESFILE, "data/$languagesFile");
while (<LANGUAGESFILE>) {
	chomp $_;
	my ($langName, $langSuffix)=split(/:/,$_);
	$languages{$langSuffix}=$langName;
}
close LANGUAGESFILE;

#read photosFile, contains s-n;photoLink and h-n;photoLink rows
open (PHOTOSFILE, "data/$photosFile");
while (<PHOTOSFILE>) {
	chomp $_;
	my ($photoTypeNumber, $photoLink)=split(/;/,$_);
	my ($photoType, $photoNumber)=split(/-/,$photoTypeNumber);
	if($photoType eq 's') {
		$photos_software{$photoNumber}=$photoLink;
	} elsif($photoType eq 'h') {
		$photos_hardware{$photoNumber}=$photoLink;
	} else {
		$photos_community{$photoNumber}=$photoLink;
	}
}
close PHOTOSFILE;


#foreach language
for (sort keys %languages) 
{
	my $langSuffix = $_;
	my $langName = $languages{$_};

	#read the title 
	my $headersWithTitle = "";
	my $mainTable = "";
	open IN,"data/langs/$langSuffix/Title";
	foreach(<IN>) {
		chomp $_;
		$headersWithTitle = &getHeadersWithTitle($_);
		$mainTable = &getMainTable();
	}
	close IN;

	#read all the data files for current language
	open DATAFILES,"ls data/langs/$langSuffix/Pages/ | ";
	foreach (<DATAFILES>) {

		#strip out CVS dirs
		if($_ =~ m/CVS/) {
			next;
		}
			
		my $currentPage = $_;
		chomp $currentPage;
		print "--- Processing FILE data/langs/$langSuffix/Pages/$currentPage\n";
		my $returnPage = "";	#page for viewing
		my $returnPrintPage = "";	#page for printing


		#read the links horizontal bar
		my $horizontalBarHTML = "";
		open IN,"data/langs/$langSuffix/Links";
		foreach(<IN>) {
			my @parts = "";
			#see the filename of each horizontal bar link
			if ($_ =~ m{<li><a href="(.*)\.html"}ig) {
				#delete the "_xx" if exists
				@parts = split(/_/,$1);
				#print "$parts[0]\n";
			}
			if ($parts[0] eq $currentPage) {
				#print the "currentPage" style and don't print the link
				$_ =~ m{html">(.*)<\/a};
				$_ = "<li id=\"currentPage\">$1<\/li>";
			} 
			$horizontalBarHTML .= $_;
		}
		close IN;


		#print Title
		$returnPage = $headersWithTitle;
		$returnPage .= $mainTable;
		$returnPrintPage = $headersWithTitle;

		#print links to other languages
		my $languageLinks = &getLanguageLinks($langSuffix, $langName, $currentPage, %languages);
		$returnPage .= $languageLinks;

		#print horizontal link bar
		$returnPage .= $horizontalBarHTML;

		#print some table stuff
		$returnPage .= "<table border = \"0\" width = \"100%\" cellpadding=\"0\" cellspacing=\"0\">\n
			<tr valign=\"top\"><td align=\"left\">\n
			<div id=\"content\">
			<div id=\"content-body\">";

		
		#read the file
		open INFILE, "data/langs/$langSuffix/Pages/$currentPage";
		while (<INFILE>) {
			$returnPage .= $_;
			$returnPrintPage .= $_;
		}
		close INFILE;

		#put printPage link in correct language...
		$returnPage = &convertTitleAndPrintable($returnPage, $langSuffix, $currentPage, "false");
		#... and hide :::startTitle:::, :::endTitle::: in the printable
		$returnPrintPage = &convertTitleAndPrintable($returnPrintPage, $langSuffix, $currentPage, "true");

		#get complete license for this language
		$returnPage .= &getLicense($langSuffix, $authors, $colaborations, $contributors);
		
		$returnPage .= &getFooter($langSuffix);
		$returnPrintPage .= "</body></html>";

		#getPhotoLinks (for screenshots pages)
		$returnPage  = getPhotos($returnPage, $langSuffix);
		$returnPrintPage  = getPhotos($returnPrintPage, $langSuffix);
	
		#convert links to images
		$returnPage  = getSiteLinks($returnPage);
		$returnPrintPage  = getSiteLinks($returnPrintPage);
	
		#write constants
		$returnPage  = getConstants($returnPage, $langSuffix);
		$returnPrintPage  = getConstants($returnPrintPage, $langSuffix);
	
		#filter file (convert á in &aacute; ...)
		#this is for solving a configuration problem in apache of software-libre.org
		$returnPage  = filterHTML($returnPage);
		$returnPrintPage  = filterHTML($returnPrintPage);

		#save files
		my $outputFile = "";
		my $outputPrintFile = "";
		if($langSuffix eq "_en" and $currentPage ne "index") {
			#don't print "_en" in english except for index page 
			#(for distinguish with the new index.html (menu) page)
			$outputFile = "html_created_no_edit/$currentPage" . ".html";
			$outputPrintFile = "html_created_no_edit/print/$currentPage" . ".html";
		} else {
			$outputFile = "html_created_no_edit/$currentPage$langSuffix" . ".html";
			$outputPrintFile = "html_created_no_edit/print/$currentPage$langSuffix" . ".html";
		}
		
		open OUT, ">$outputFile";
		print OUT $returnPage;
		close OUT;
		
		open OUT, ">$outputPrintFile";
		print OUT $returnPrintPage;
		close OUT;
	}
}

sub getHeadersWithTitle {
	my $title=($_);
		
	my $return = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\"\n\"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\n\n<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"es   \">

			  <head>
			  	<meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\" />
		<title>$title</title>
		<style type=\"text/css\">
			\@import url(style.css)\;
		</style>
		</head>

		<body id=\"page-main\" class=\"with-sidebar\">
		";
	return $return;
}

sub getMainTable {
	my $return = "<table width=\"100%\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">
		<tr><td align=\"left\">
		<div align=\"left\"><a href=\"index.html\" border=\"0\"><img src=\"images/chronojump33.png\" alt=\"logo\" width=\"591\" height=\"177\" border=\"0\"></a><br>
		</div>
		</td><td valign=\"bottom\" align=\"right\">
		";
	return $return;
}

sub getLanguageLinks {
	my ($langSuffix, $langName, $currentPage, %languages) = @_;

	my $return = "<div id=\"sidebar\">\n";
	$return .= "<ul>\n";

	my $printLink = "true";
	my $link = "";

	#print other languages if available
	for (sort keys %languages) 
	{
		if($langSuffix eq $_) {
			$printLink = "false";
			$return .= "<li id=\"currentLanguage\">";
		}
		else { 
			$printLink = "true";
			$return .= "<li>"; 
		}
			
		#if found this document in other language show link
		if(-e "data/langs/$_/Pages/$currentPage") {
			#if it's english, don't print the "_en"
			#except for index page (see above)
			if($_ eq "_en" and $currentPage ne "index") {
				$link = "<a href=\"$currentPage.html\">";
			} else {
				$link = "<a href=\"$currentPage$_.html\">";
			}

			if($printLink eq "true") {
				$return .= "$link$languages{$_}</a>";
			} else {
				$return .= "$languages{$_}";
			}
		} else {
			$return .= "$languages{$_} (pending)";
		}

		$return .= "</li>\n";
	}
	$return .= "</ul><br>\n";
	$return .= "</div></td></tr></table>\n";
	return $return;
}

sub getPeople {
	my ($myFile)= @_;
	my $return = "";

	open INFILE, "data/$myFile";
	while (<INFILE>) {
		$return .= $_;
	}
	close INFILE;
	return $return;
}
	

sub getLicense {
	my ($langSuffix, $authors, $colaborations, $contributors) = @_;
	my $return = "";
	
	open INFILE, "data/langs/$langSuffix/License";
	while (<INFILE>) {
		$return .= $_
	}
	close INFILE;

	#change code ":::authors:::" for $authors, and also for colaborations and contributions
	$return =~ s/:::awards:::/$awards/;
	$return =~ s/:::authors:::/$authors/;
	$return =~ s/:::colaborations:::/$colaborations/;
	$return =~ s/:::contributors:::/$contributors/;
	
	return $return;
}

sub getSiteLinks {
	my ($pageContent)= @_;

	$pageContent =~ s/:::imageLink:::/$siteURL\/images/g;
	$pageContent =~ s/:::articleLink:::/$siteURL\/articles/g;
	$pageContent =~ s/:::webLink:::/$siteURL/g; #only used for 'accessible in' when pointing to bibliography web pages
							#used also to access new images on root dir, because image dir is not refresed now on gnome pages
	$pageContent =~ s/:::manualLink:::/$manualLink/g;
	$pageContent =~ s/:::glossaryLink:::/$glossaryLink/g;

	return $pageContent;
}
		
sub getPhotos {
	my ($pageContent)= @_;

	$pageContent =~ s/:::softPhoto-(\d+):::/<tr valign="top"><td width="250">$photos_software{$1}/g;
	$pageContent =~ s/:::hardPhoto-(\d+):::/<td width="250">$photos_hardware{$1}/g;
	$pageContent =~ s/:::softPhotoNull:::/<tr valign="top"><td width="250">&nbsp;<\/td>/g;
	$pageContent =~ s/:::hardPhotoNull:::/<td width="250">&nbsp;<\/td><\/tr>\n/g;
	$pageContent =~ s/:::endSoftwarePhoto:::/<\/td>/g;
	$pageContent =~ s/:::endHardwarePhoto:::/<\/td><\/tr>\n/g;
	$pageContent =~ s/:::communityPhoto-(\d+):::/$photos_community{$1}/g;

	return $pageContent;
}
		
sub getConstants {
	my ($pageContent, $langSuffix)= @_;

	$pageContent =~ s/:::currentVersion:::/$currentVersion/g;
	$pageContent =~ s/:::linuxMailDownloadLink:::/$linuxMailDownloadLink/g;
	$pageContent =~ s/:::linuxMailDownloadName:::/$linuxMailDownloadName/g;
	$pageContent =~ s/:::windowsMailDownloadLink:::/$windowsMailDownloadLink/g;
	$pageContent =~ s/:::windowsMailDownloadName:::/$windowsMailDownloadName/g;
	$pageContent =~ s/:::maemoMailDownloadLink:::/$maemoMailDownloadLink/g;
	$pageContent =~ s/:::maemoMailDownloadName:::/$maemoMailDownloadName/g;
	$pageContent =~ s/:::changesMailDownloadLink:::/$changesMailDownloadLink/g;
	$pageContent =~ s/:::awards:::/$awards/g;

	#Vic conferences
	$pageContent =~ s/:::construccio_dispositius_mesuraLink:::/$construccio_dispositius_mesuraLink/g;
	$pageContent =~ s/:::fonaments_teorics_bescos_velezLink:::/$fonaments_teorics_bescos_velezLink/g;
	$pageContent =~ s/:::fonaments_teorics_padullesLink:::/$fonaments_teorics_padullesLink/g;
	$pageContent =~ s/:::homenatge_carmelo_boscoLink:::/$homenatge_carmelo_boscoLink/g;
	$pageContent =~ s/:::instalacio_construccio_adquisicioLink:::/$instalacio_construccio_adquisicioLink/g;
	$pageContent =~ s/:::projecte_chronojumpLink:::/$projecte_chronojumpLink/g;
	$pageContent =~ s/:::us_chronojump_bescos_padullesLink:::/$us_chronojump_bescos_padullesLink/g;

	my $newDate = getLocalisedDate($langSuffix, $currentVersionDay, $currentVersionMonth, $currentVersionYear);
	$pageContent =~ s/:::currentVersionDate:::/$newDate/g;

	return $pageContent;
}
		
sub convertTitleAndPrintable {
	my ($pageContent, $langSuffix, $currentPage, $isPrintable) = @_;

	if($isPrintable eq "false") {
		my $printLinkName = &getPrintLinkName($langSuffix, $currentPage);
		$pageContent =~ s/:::startTitle:::/<table border=\"0\" width=\"100%\"><tr><td align=\"left\"><h4 id=\"top\">/g;
		$pageContent =~ s/:::endTitle:::/<\/h4><\/td><td align=\"right\">$printLinkName<\/td><\/tr><\/table>/g;
	} else {
		$pageContent =~ s/:::startTitle:::/<h4 id=\"top\">/g;
		$pageContent =~ s/:::endTitle:::/<\/h4>/g;
	}

	return $pageContent;
}

sub getPrintLinkName {
	my ($langSuffix, $currentPage) = @_;
	
	my $printName = "";
	open INFILE, "data/langs/$langSuffix/Print";
	while (<INFILE>) {
		$printName .= $_;
	}
	close INFILE;

	if($langSuffix eq "_en" and $currentPage ne "index") {
		return "<a href=\"print/$currentPage.html\"><font size=\"2\"><tt>$printName</tt></font></a>";
	} else {
		return "<a href=\"print/$currentPage$langSuffix.html\"><font size=\"2\"><tt>$printName</tt></font></a>";
	}
}

sub getFooter {
	my ($langSuffix) = @_;

	my $translator = "";
	open INFILE, "data/langs/$langSuffix/Translator";
	while (<INFILE>) {
		$translator .= $_;
	}
	close INFILE;

	my $designed = "";
	open INFILE, "data/langs/$langSuffix/Designed";
	while (<INFILE>) {
		$designed .= $_;
	}
	close INFILE;


	my $return = "";
	$return .= "<div id=\"footer\">\n
		<hr width=\"98%\" noshade size=\"1\">
		<table align=\"center\" width=\"98%\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\">\n
		<tr><td align=\"left\">$translator</td><td>$designed</td></tr>\n
		</div>\n
		</body>\n
		</html>";

	return $return;
}

sub filterHTML {
	my ($return)= @_;
	
	#TODO: find a perl library that makes this
	
	#spanish
	$return =~ s/&/&/g;
	$return =~ s/á/&aacute;/g;
	$return =~ s/é/&eacute;/g;
	$return =~ s/í/&iacute;/g;
	$return =~ s/ó/&oacute;/g;
	$return =~ s/ú/&uacute;/g;
	$return =~ s/ñ/&ntilde;/g;
	$return =~ s/Á/&Aacute;/g;
	$return =~ s/É/&Eacute;/g;
	$return =~ s/Í/&Iacute;/g;
	$return =~ s/Ó/&Oacute;/g;
	$return =~ s/Ú/&Uacute;/g;
	$return =~ s/Ñ/&Ntilde;/g;
	#$return =~ s/¿/&iquest;/g;
	#$return =~ s/!/&#033;/g;
		
	#catalan
	$return =~ s/à/&agrave;/g;
	$return =~ s/è/&egrave;/g;
	$return =~ s/ò/&ograve;/g;
	$return =~ s/ï/&iuml;/g;
	$return =~ s/ü/&uuml;/g;
	$return =~ s/À/&Agrave;/g;
	$return =~ s/È/&Egrave;/g;
	$return =~ s/Ò/&Ograve;/g;
	$return =~ s/Ï/&Uuml;/g;
	$return =~ s/Ü/&Uuml;/g;
	$return =~ s/ç/&ccedil;/g;
	$return =~ s/Ç/&Ccedil;/g;
	$return =~ s/·/&middot;/g;

	#português
	$return =~ s/ã/&atilde;/g;
	$return =~ s/õ/&otilde;/g;
	$return =~ s/ê/&ecirc;/g;
	$return =~ s/ô/&ocirc;/g;

	#deutsch
	$return =~ s/Ä/&Auml;/g;
	$return =~ s/Ö/&Ouml;/g;
	$return =~ s/ä/&auml;/g;
	$return =~ s/ö/&ouml;/g;
	$return =~ s/ß/&szlig/g;
	
	
	return $return;
}

sub getLocalisedDate {
	my ($langSuffix, $day, $month, $year) = @_;

	my %english = ('1', 'January', '2', 'February', '3', 'March', '4', 'April', '5', 'May', '6', 'June', 
			'7', 'July', '8', 'August', '9', 'September', '10', 'October', '11', 'November', '12', 'December' );

	my %spanish = ('1', 'Enero', '2', 'Febrero', '3', 'Marzo', '4', 'Abril', '5', 'Mayo', '6', 'Junio', 
			'7', 'Julio', '8', 'Agosto', '9', 'Septiembre', '10', 'Octubre', '11', 'Noviembre', '12', 'Diciembre' );

	my %catalan = ('1', 'Gener', '2', 'Febrer', '3', 'Març', '4', 'Abril', '5', 'Maig', '6', 'Juny', 
			'7', 'Juliol', '8', 'Agost', '9', 'Septembre', '10', 'Octubre', '11', 'Novembre', '12', 'Desembre' );

	my %french = ('1', 'Janvier', '2', 'Février', '3', 'Mars', '4', 'Avril', '5', 'Mai', '6', 'Juin', 
			'7', 'Juilliet', '8', 'Aôut', '9', 'Septembre', '10', 'Octobre', '11', 'Novembre', '12', 'Décembre' );

	my %italian = ('1', 'gennaio', '2', 'febbraio', '3', 'marzo', '4', 'aprile', '5', 'maggio', '6', 'giugno', 
			'7', 'luglio', '8', 'agosto', '9', 'settembre', '10', 'ottobre', '11', 'novembre', '12', 'dicembre' );

	my %portuguese = ('1', 'janeiro', '2', 'fevereiro', '3', 'março', '4', 'abril', '5', 'maio', '6', 'junho',
			'7', 'julho', '8', 'agosto', '9', 'setembro', '10', 'outubro', '11', 'novembro', '12', 'dezembro' );

	my %gaelican = ('1', 'Xaneiro', '2', 'Febrero', '3', 'Marzo', '4', 'Abril', '5', 'Mayo', '6', 'Junio', 
			'7', 'Julio', '8', 'Agosto', '9', 'Septiembre', '10', 'Octubre', '11', 'Noviembre', '12', 'Diciembre' );

	my %deutsch = ('1', 'Januar', '2', 'Februar', '3', 'März', '4', 'April', '5', 'Mai', '6', 'Juni', 
			'7', 'Juli', '8', 'August', '9', 'September', '10', 'Oktober', '11', 'November', '12', 'Dezeember' );


	if($langSuffix eq "_en") {
		return "$english{$month}-$day-$year"; #english the only one MM-DD-YYYY (i think)
	} elsif($langSuffix eq "_es") {
		return "$day-$spanish{$month}-$year";
	} elsif($langSuffix eq "_ca") {
		return "$day-$catalan{$month}-$year";
	} elsif($langSuffix eq "_fr") {
		return "$day-$french{$month}-$year";
	} elsif($langSuffix eq "_it") {
		return "$day-$italian{$month}-$year";
	} elsif($langSuffix eq "_pt") {
		return "$day-$portuguese{$month}-$year";
	} elsif($langSuffix eq "_gl") {
		return "$day-$gaelican{$month}-$year";
	} elsif($langSuffix eq "_de") {
		return "$day-$deutsch{$month}-$year";
	} else {
		print "Date on $langSuffix not localized!!!";
		return "$english{$month}-$day-$year";
	}

}
