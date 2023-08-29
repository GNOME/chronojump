/*
 * This file is part of ChronoJump
 *
 * Chronojump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *    (at your option) any later version.
 *
 * Chronojump is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * Copyright (C) 2021-2023   Xavier de Blas <xaviblas@gmail.com>
 */


using System.IO; 			//Directory, ...
using System.Collections; 		//ArrayList
using System.Collections.Generic; 	//List<T>
using System.Threading;
using Mono.Unix;

public class ForceSensorExport : ExportFiles
{
	private Constants.Modes mode;
	private List<ForceSensorRFD> rfdList;
	private ForceSensorImpulse impulse;
	private double duration;
	private int durationPercent;
	private double forceSensorElasticEccMinDispl;
	private int forceSensorNotElasticEccMinForce;
	private double forceSensorElasticConMinDispl;
	private int forceSensorNotElasticConMinForce;
	private bool forceSensorStartEndOptimized;
	private double forceSensorAnalyzeMaxAVGInWindowSeconds;

	private List<ForceSensor> fs_l;
	private ArrayList fsEx_l;
	private static int totalRepsToExport;

	public ForceSensorExport (
			Constants.Modes mode,
			Gtk.Notebook notebook,
			Gtk.Label labelProgress, Gtk.ProgressBar progressbar,
			Gtk.Label labelResult,
			bool includeImages,
			int imageWidth, int imageHeight,
			bool isWindows, int personID, int sessionID,
			List<ForceSensorRFD> rfdList, ForceSensorImpulse impulse,
			double duration, int durationPercent,
			double forceSensorElasticEccMinDispl,
			int forceSensorNotElasticEccMinForce,
			double forceSensorElasticConMinDispl,
			int forceSensorNotElasticConMinForce,
			bool forceSensorStartEndOptimized,
			char exportDecimalSeparator,
			double forceSensorAnalyzeMaxAVGInWindowSeconds)

	{
		Button_done = new Gtk.Button();

		assignParams(notebook, labelProgress, progressbar, new Gtk.Label(), labelResult, includeImages,
				imageWidth, imageHeight, isWindows, personID, sessionID, exportDecimalSeparator);

		this.mode = mode;
		this.rfdList = rfdList;
		this.impulse = impulse;
		this.duration = duration;
		this.durationPercent = durationPercent;
		this.forceSensorElasticEccMinDispl = forceSensorElasticEccMinDispl;
		this.forceSensorNotElasticEccMinForce = forceSensorNotElasticEccMinForce;
		this.forceSensorElasticConMinDispl = forceSensorElasticConMinDispl;
		this.forceSensorNotElasticConMinForce = forceSensorNotElasticConMinForce;
		this.forceSensorStartEndOptimized = forceSensorStartEndOptimized;
		this.forceSensorAnalyzeMaxAVGInWindowSeconds = forceSensorAnalyzeMaxAVGInWindowSeconds;
	}

	private string getTempGraphsDir() {
		return Path.Combine(Path.GetTempPath(), "chronojump_force_sensor_export_graphs_rfd");
	}
	private string getTempGraphsABDir() {
		return Path.Combine(Path.GetTempPath(), "chronojump_force_sensor_export_graphs_ab");
	}
	private string getCSVFileName() {
		return "chronojump_force_sensor_export.csv";
	}
	private string getTempCSVFileName() {
		return Path.Combine(Path.GetTempPath(), getCSVFileName());
	}

	protected override void createOrEmptyDirs()
	{
		//create progressbar and graph files dirs or delete their contents
		createOrEmptyDir(getTempSourceFilesDir());
		createOrEmptyDir(getTempProgressDir());
		createOrEmptyDir(getTempGraphsDir());
		createOrEmptyDir(getTempGraphsABDir());
	}

	protected override bool getData ()
	{
		int elastic = ForceSensor.GetElasticIntFromMode (mode);

		fs_l = SqliteForceSensor.Select(false, -1, personID, sessionID, elastic);
		personSession_l = SqlitePersonSession.SelectCurrentSessionPersons(sessionID, true);
		fsEx_l = SqliteForceSensorExercise.Select (false, -1, elastic, false, "");
		totalRepsToExport = 0;

		return fs_l.Count > 0;
	}

	protected override bool processSets ()
	{
		Person p = new Person();
		PersonSession ps = new PersonSession();

		List<ForceSensorGraphABExport> fsgABe_l = new List<ForceSensorGraphABExport>();

		//to manage sets we need previousPerson and previousExercise
		ForceSensorExportSetManage fsesm = new ForceSensorExportSetManage();

		int count = 1;
		foreach(ForceSensor fs in fs_l)
		{
			messageToProgressbar = string.Format(Catalog.GetString("Preparing sets {0}/{1}"), count++, fs_l.Count);

			if(cancel)
				return false;

			// 1) checks
			//check fs is ok
			if(fs == null || ! Util.FileExists(fs.FullURL))
				continue;

			//check fs has data
			List<string> contents = Util.ReadFileAsStringList(fs.FullURL);
			if(contents.Count < 3)
			{
				//new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileEmptyStr());
				//return;
				continue;
			}

			// 2) get the person
			bool found = false;
			foreach(PersonAndPS paps in personSession_l)
			{
				if(paps.p.UniqueID == fs.PersonID)
				{
					p = paps.p;
					ps = paps.ps;

					found = true;
					break;
				}
			}
			if(! found)
				continue;

			// 3) get the exercise
			found = false;
			ForceSensorExercise fsEx = new ForceSensorExercise();
			foreach(ForceSensorExercise fsExTemp in fsEx_l)
				if(fsExTemp.UniqueID == fs.ExerciseID)
				{
					fsEx = fsExTemp;
					found = true;
					break;
				}
			if(! found)
				continue;

			if(! fsesm.Exists(p.UniqueID, fsEx.UniqueID, fs.Laterality))
				fsesm.AddForceSensorExportSet(p.UniqueID, fsEx.UniqueID, fs.Laterality);

			//make the exercise have EccReps = true in order to have an AB with the concentric and eccentric part
			//and send both to R to be able to have the force window in that AB
			//fsEx.EccReps = true;
			fsEx.RepetitionsShow = ForceSensorExercise.RepetitionsShowTypes.BOTHSEPARATED;

			double eccMinDispl = fsEx.GetEccOrConMinMaybePreferences(true,
					forceSensorElasticEccMinDispl,
					forceSensorNotElasticEccMinForce);
			double conMinDispl = fsEx.GetEccOrConMinMaybePreferences(false,
					forceSensorElasticConMinDispl,
					forceSensorNotElasticConMinForce);

			// 4) create fsAI (includes the repetitions)
			ForceSensorAnalyzeInstant fsAI = new ForceSensorAnalyzeInstant(
					"",
					fs.FullURL,
					-1, -1,
					fsEx, ps.Weight,
					fs.CaptureOption, fs.Stiffness,
					eccMinDispl, conMinDispl
					);

			// 5) call R
			string title = p.Name;
			string exercise = fsEx.Name;
			if (isWindows) {
				title = Util.ConvertToUnicode(title);
				exercise = Util.ConvertToUnicode(exercise);
			}
			if (title == null || title == "")
				title = "unnamed";


			string destination = UtilEncoder.GetmifCSVInputMulti();
			Util.FileDelete(destination);

			//copy file to tmp to be readed by R
			string fsFullURLMoved = Path.Combine(getTempSourceFilesDir(), count.ToString() + ".csv");
			File.Copy(fs.FullURL, fsFullURLMoved, true); //can be overwritten

			//delete result file
			Util.FileDelete(getTempCSVFileName());

			bool addedSet = false;
			int repCount = 1;
			int repConcentricSampleStart = -1;
			bool lastIsCon = false;
			ForceSensorRepetition repLast = null;
			foreach(ForceSensorRepetition rep in fsAI.ForceSensorRepetition_l)
			{
				if(rep.type == ForceSensorRepetition.Types.CON)
				{
					repConcentricSampleStart = rep.sampleStart;
					repLast = rep;
					lastIsCon = true;
				}
				else if(rep.type == ForceSensorRepetition.Types.ECC && repConcentricSampleStart != -1)
				{
					double maxAvgForceInWindow = 0;
					double maxAvgForceInWindowSampleStart = 0;
					double maxAvgForceInWindowSampleEnd = 0;
					bool success = fsAI.CalculateRangeParams(repConcentricSampleStart, rep.sampleEnd,
							forceSensorAnalyzeMaxAVGInWindowSeconds);
					if(success) {
						maxAvgForceInWindow = fsAI.Gmaiw.Max;
						maxAvgForceInWindowSampleStart = fsAI.Gmaiw.MaxSampleStart;
						maxAvgForceInWindowSampleEnd = fsAI.Gmaiw.MaxSampleEnd;
					}

					if(! addedSet) {
						fsesm.AddSet(p.UniqueID, fsEx.UniqueID, fs.Laterality);
						addedSet = true;
					}
					fsgABe_l.Add(new ForceSensorGraphABExport (
								isWindows,
								fsFullURLMoved,
								Util.CSVDecimalColumnIsPoint(fsFullURLMoved, 1),
								fsAI.ForceMAX,			//raw
								maxAvgForceInWindow,		//raw
								forceSensorAnalyzeMaxAVGInWindowSeconds, //raw
								maxAvgForceInWindowSampleStart,	//the start sample of the result
								maxAvgForceInWindowSampleEnd,	//the end sample of the result
								fs.Laterality,
								fsesm.GetCount(p.UniqueID, fsEx.UniqueID, fs.Laterality),
								repCount ++,
								fs.Comments,
								fs.CaptureOption,
								repConcentricSampleStart, 	//start of concentric rep
								rep.sampleEnd,			//end of eccentric rep
								title, exercise, fs.DatePublic, fs.TimePublic, new TriggerList()
								));

					lastIsCon = false;
				}
			}

			/*
			 *1 if the last rep is con, also send to R (no problem if there is no ending ecc phase)
			 *2 if we have not found any rep on this set, just pass from A to B on the set.
				This happens eg if the person starts with the maximum force (or using the forceSensor to weight things)
				or where the test has no force increase at all.
			*/
			if(
					(lastIsCon && repLast != null) 		// *1
					||
					(repCount == 1 && ! lastIsCon) 		// *2
					)
			{
				//if (repCount == 1 && ! lastIsCon) { 		// *2
					int sampleA = 1;
					int sampleB = fsAI.GetLength() -1;
				//}
				if(lastIsCon && repLast != null) { 		// *1
					sampleA = repConcentricSampleStart; //start of concentric rep
					sampleB = repLast.sampleEnd; 	//end of eccentric rep
				}

				double maxAvgForceInWindow = 0;
				double maxAvgForceInWindowSampleStart = 0;
				double maxAvgForceInWindowSampleEnd = 0;
				bool success = fsAI.CalculateRangeParams(sampleA, sampleB,
						forceSensorAnalyzeMaxAVGInWindowSeconds);
				if(success) {
					maxAvgForceInWindow = fsAI.Gmaiw.Max;
					maxAvgForceInWindowSampleStart = fsAI.Gmaiw.MaxSampleStart;
					maxAvgForceInWindowSampleEnd = fsAI.Gmaiw.MaxSampleEnd;
				}

				if(! addedSet) {
					fsesm.AddSet(p.UniqueID, fsEx.UniqueID, fs.Laterality);
					addedSet = true;
				}
				fsgABe_l.Add(new ForceSensorGraphABExport (
							isWindows,
							fsFullURLMoved,
							Util.CSVDecimalColumnIsPoint(fsFullURLMoved, 1),
							fsAI.ForceMAX,			//raw
							maxAvgForceInWindow,		//raw
							forceSensorAnalyzeMaxAVGInWindowSeconds, //raw
							maxAvgForceInWindowSampleStart,	//the start sample of the result
							maxAvgForceInWindowSampleEnd,	//the end sample of the result
							fs.Laterality,
							fsesm.GetCount(p.UniqueID, fsEx.UniqueID, fs.Laterality),
							repCount ++,
							fs.Comments,
							fs.CaptureOption,
							sampleA,
							sampleB,
							title, exercise, fs.DatePublic, fs.TimePublic, new TriggerList()
							));
			}
		}

		if(fsgABe_l.Count > 0)
		{
			totalRepsToExport = fsgABe_l.Count;
			ForceSensorGraph fsg = new ForceSensorGraph(
					rfdList, impulse,
					duration, durationPercent,
					forceSensorStartEndOptimized,
					true, //not used to read data, but used to print data
					exportDecimalSeparator, // at write file
					fsgABe_l,
					forceSensorAnalyzeMaxAVGInWindowSeconds,
					includeImages
					);

			if(! fsg.CallR(imageWidth, imageHeight, false))
			{
				failedRprocess = true;
				return false;
			}
		}

		LogB.Information("Waiting creation of file... ");
		while ( ! ( Util.FileReadable(getTempCSVFileName()) || cancel ) )
			;

		if(cancel)
			return false;

		if(includeImages && ! copyImages(getTempGraphsDir(), exportURL,
					"chronojump_force_sensor_export_graphs_rfd"))
			return false;
		if(includeImages && ! copyImages(getTempGraphsABDir(), exportURL,
					"chronojump_force_sensor_export_graphs_ab"))
			return false;

		//copy the CSV
		//if includeImages, exportURL is a dir, so need a filename to have File.Copy on all systems
		if(includeImages)
			File.Copy(getTempCSVFileName(), Path.Combine(exportURL, getCSVFileName()), true);
		else
			File.Copy(getTempCSVFileName(), exportURL, true);

		return true;
	}

	protected override void setProgressBarTextAndFractionPrepare (int fileCount)
	{
		setProgressBarTextAndFractionDo (fileCount, totalRepsToExport);
	}
	protected override void setProgressBarTextAndFractionDo (int current, int total)
	{
		labelProgress.Text = string.Format(Catalog.GetString("Exporting repetition {0}/{1}"),
				current, total);
		progressbar.Fraction = UtilAll.DivideSafeFraction(current, total);
	}
}

//to count sets according to person and exercise
public class ForceSensorExportSet
{
	public int pID; //personID
	public int exID; //forceSensor exercise ID
	public string lat; //laterality
	public int count; //how many sets with this pID && exID

	public ForceSensorExportSet (int pID, int exID, string lat)
	{
		this.pID = pID;
		this.exID = exID;
		this.lat = lat;
		this.count = 0;
	}

	public bool IsEqual (int pID, int exID, string lat)
	{
		return (this.pID == pID && this.exID == exID && this.lat == lat);
	}
}

public class ForceSensorExportSetManage
{
	List<ForceSensorExportSet> l;

	public ForceSensorExportSetManage()
	{
		l = new List<ForceSensorExportSet>();
	}

	public bool Exists (int pID, int exID, string lat)
	{
		foreach(ForceSensorExportSet fses in l)
			if(fses.IsEqual(pID, exID, lat))
				return true;

		return false;
	}

	public void AddForceSensorExportSet (int pID, int exID, string lat)
	{
		ForceSensorExportSet fses = new ForceSensorExportSet(pID, exID, lat);
		l.Add(fses);
	}

	public void AddSet (int pID, int exID, string lat)
	{
		foreach(ForceSensorExportSet fses in l)
			if(fses.IsEqual(pID, exID, lat))
				fses.count ++;
	}

	public int GetCount (int pID, int exID, string lat)
	{
		foreach(ForceSensorExportSet fses in l)
			if(fses.IsEqual(pID, exID, lat))
				return fses.count;

		return -1;
	}
}
