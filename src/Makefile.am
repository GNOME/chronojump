SUBDIRS = Mini chronojump-importer

ASSEMBLY = Chronojump
TARGET = exe
LINK = $(REF_DEP_CHRONOJUMP)

SOURCES = \
	gui/app1/chronojump.cs\
	gui/app1/chronojumpLogo.cs\
	gui/app1/chronojumpPersons.cs\
	gui/app1/encoder.cs\
	gui/app1/icons.cs\
	gui/app1/jumpsProfile.cs\
	gui/app1/jumpsDjOptimalFall.cs\
	gui/app1/jumpsRunsEvolution.cs\
	gui/app1/jumpsRjFatigue.cs\
	gui/app1/jumpsWeightFVProfile.cs\
	gui/app1/contactsExercise.cs\
	gui/app1/forceSensor.cs\
	gui/app1/forceSensorAnalyze.cs\
	gui/app1/news.cs\
	gui/app1/session/delete.cs\
	gui/app1/session/addEdit.cs\
	gui/app1/session/backup.cs\
	gui/app1/session/export.cs\
	gui/app1/session/loadAndImport.cs\
	gui/app1/session/main.cs\
	gui/app1/session/tagSessionSelect.cs\
	gui/app1/jump.cs\
	gui/app1/menu.cs\
	gui/app1/menu_tiny.cs\
	gui/app1/pulse.cs\
	gui/app1/reactionTime.cs\
	gui/app1/run.cs\
	gui/app1/runEncoder.cs\
	gui/app1/runEncoderAnalyze.cs\
	gui/app1/shortcuts.cs\
	gui/cairo/bars.cs\
	gui/cairo/generic.cs\
	gui/cairo/jumpsDjOptimalFall.cs\
	gui/cairo/jumpsRunsEvolution.cs\
	gui/cairo/jumpsProfile.cs\
	gui/cairo/jumpsRjFatigue.cs\
	gui/cairo/jumpsWeightFVProfile.cs\
	gui/cairo/raceAnalyzer.cs\
	gui/cairo/radial.cs\
	gui/cairo/runDoubleContacts.cs\
	gui/cairo/util.cs\
	gui/cairo/xy.cs\
	gui/chronojumpImporter.cs\
	gui/chronopic.cs\
	gui/chronopicRegister.cs\
	gui/confirm.cs\
	gui/cjCombo.cs\
	gui/encoderGraphObjects.cs\
	gui/encoderTreeviews.cs\
	gui/encoderConfiguration.cs\
	gui/encoderSelectRepetitions.cs\
	gui/error.cs\
	gui/eventExecute.cs\
	gui/eventGraphConfigure.cs\
	gui/event.cs\
	gui/executeAuto.cs\
	gui/exhibition.cs\
	gui/forceSensorElasticBands.cs\
	gui/forceSensorExercise.cs\
	gui/guiTests.cs\
	gui/jump.cs\
	gui/jumpType.cs\
	gui/restTime.cs\
	gui/run.cs\
	gui/runType.cs\
	gui/reactionTime.cs\
	gui/pulse.cs\
	gui/multiChronopic.cs\
	gui/overview.cs\
	gui/person/addModify.cs\
	gui/person/addMultiple.cs\
	gui/person/personSelect.cs\
	gui/person/recuperate.cs\
	gui/person/showAllEvents.cs\
	gui/preferences.cs\
	gui/sendLogAndPoll.cs\
	gui/session/selectStats.cs\
	gui/stats.cs\
	gui/presentation.cs\
	gui/report.cs\
	gui/about.cs\
	gui/dialogMessage.cs\
	gui/dialogCalendar.cs\
	gui/dialogImageTest.cs\
	gui/dialogPersonPopup.cs\
	gui/dialogShortcuts.cs\
	gui/dialogThreshold.cs\
	gui/repetitiveConditions.cs\
	gui/convertWeight.cs\
	gui/genericWindow.cs\
	gui/server.cs\
	gui/splash.cs\
	gui/sprint.cs\
	gui/dialogServerStats.cs\
	gui/evaluator.cs\
	gui/networks.cs\
	gui/usefulObjects.cs\
	gui/trigger.cs\
	gui/webcam.cs\
	statType.cs\
	stats/main.cs\
	stats/chronojumpProfile.cs\
	stats/global.cs\
	stats/sjCmjAbk.cs\
	stats/sjCmjAbkPlus.cs\
	stats/djIndex.cs\
	stats/djPower.cs\
	stats/djQ.cs\
	stats/jumpSimpleSubtraction.cs\
	stats/rjIndex.cs\
	stats/rjPotencyBosco.cs\
	stats/rjEvolution.cs\
	stats/jumpIndexes.cs\
	stats/fv.cs\
	stats/potency.cs\
	stats/rjAVGSD.cs\
	stats/runSimple.cs\
	stats/runIntervallic.cs\
	stats/graphs/graphData.cs\
	stats/graphs/graphROptions.cs\
	stats/graphs/graphSerie.cs\
	stats/graphs/global.cs\
	stats/graphs/sjCmjAbk.cs\
	stats/graphs/sjCmjAbkPlus.cs\
	stats/graphs/djIndex.cs\
	stats/graphs/djPower.cs\
	stats/graphs/djQ.cs\
	stats/graphs/rjIndex.cs\
	stats/graphs/rjPotencyBosco.cs\
	stats/graphs/rjEvolution.cs\
	stats/graphs/jumpIndexes.cs\
	stats/graphs/fv.cs\
	stats/graphs/potency.cs\
	stats/graphs/rjAVGSD.cs\
	stats/graphs/runSimple.cs\
	stats/graphs/runIntervallic.cs\
	stats/graphs/jumpSimpleSubtraction.cs\
	sqlite/chronopicRegister.cs\
	sqlite/main.cs\
	sqlite/preferences.cs\
	sqlite/session.cs\
	sqlite/encoder.cs\
	sqlite/encoderConfiguration.cs\
	sqlite/executeAuto.cs\
	sqlite/forceSensor.cs\
	sqlite/jump.cs\
	sqlite/jumpRj.cs\
	sqlite/jumpType.cs\
	sqlite/news.cs\
	sqlite/run.cs\
	sqlite/runInterval.cs\
	sqlite/runType.cs\
	sqlite/person.cs\
	sqlite/personSession.cs\
	sqlite/personSessionNotUpload.cs\
	sqlite/stat.cs\
	sqlite/pulse.cs\
	sqlite/pulseType.cs\
	sqlite/runEncoder.cs\
	sqlite/reactionTime.cs\
	sqlite/multiChronopic.cs\
	sqlite/event.cs\
	sqlite/json.cs\
	sqlite/sport.cs\
	sqlite/speciallity.cs\
	sqlite/tagSession.cs\
	sqlite/trigger.cs\
	sqlite/usefulObjects.cs\
	sqlite/country.cs\
	sqlite/oldConvert.cs\
	sqlite/server.cs\
	execute/arduinoCapture.cs\
	execute/event.cs\
	execute/jump.cs\
	execute/run.cs\
	execute/runObjects.cs\
	execute/pulse.cs\
	execute/reactionTime.cs\
	execute/multiChronopic.cs\
	exportFiles/exportFiles.cs\
	exportFiles/fs.cs\
	exportFiles/re.cs\
	exportFiles/sprint.cs\
	AssemblyInfo.cs\
	buildInfo.cs\
	chronojump.cs\
	config.cs\
	chronoDebug.cs\
	encoder.cs\
	encoderCapture.cs\
	encoderCaptureInertialBG.cs\
	encoderRhythm.cs\
	encoderRProc.cs\
	executeAuto.cs\
	event.cs\
	eventType.cs\
	FTD2XX_NET.cs\
	forcePlatform.cs\
	forceSensor.cs\
	forceSensorDynamics.cs\
	genericObjects.cs\
	json/json.cs\
	json/utils.cs\
	json/compujump.cs\
	json/compujumpEncoder.cs\
	json/compujumpForceSensor.cs\
	json/compujumpSprint.cs\
	json/compujumpOther.cs\
	json/exhibitions.cs\
	jump.cs\
	jumpsProfile.cs\
	jumpsDjOptimalFall.cs\
	jumpsRunsEvolution.cs\
	jumpsRjFatigue.cs\
	jumpsWeightFVProfile.cs\
	jumpType.cs\
	networks.cs\
	news.cs\
	person.cs\
	personSession.cs\
	run.cs\
	runEncoder.cs\
	runType.cs\
	pulse.cs\
	pulseType.cs\
	reactionTime.cs\
	reactionTimeType.cs\
	multiChronopic.cs\
	multiChronopicType.cs\
	session.cs\
	exportSession.cs\
	preferences.cs\
	restTime.cs\
	sprint.cs\
	tagSession.cs\
	threshold.cs\
	treeview/event.cs\
	treeview/person.cs\
	treeview/jump.cs\
	treeview/run.cs\
	treeview/pulse.cs\
	treeview/reactionTime.cs\
	treeview/multiChronopic.cs\
	trigger.cs\
	util.cs\
	utilAll.cs\
	utilCSV.cs\
	utilDate.cs\
	utilEncoder.cs\
	utilGtk.cs\
	utilLanguage.cs\
	utilMath.cs\
	utilMultimedia.cs\
	constants.cs\
	commandLineEncoder.cs\
	report.cs\
	sport.cs\
	log.cs\
	logB.cs\
	logConsoleCrayon.cs\
	RFID.cs\
	serverPing.cs\
	serverEvaluator.cs\
	staticClassState.cs\
	chronopic.cs\
	chronopic2016.cs\
	chronopicDetect.cs\
	chronopicRegister.cs\
	chronojumpImporter.cs\
	executeProcess.cs\
	version31.cs\
	oldCodeNeedToDBConvert/person.cs\
	oldCodeNeedToDBConvert/personSession.cs\
	oldCodeNeedToDBConvert/sqlite/person.cs\
	oldCodeNeedToDBConvert/sqlite/personSession.cs\
	oldCodeNeedToDBConvert/sqlite/session.cs\
	webcam/webcam.cs\
	webcam/webcamFfmpeg.cs\
	webcam/webcamFfmpegDevices.cs\
	webcam/webcamFfmpegSupportedModes.cs\
	webcam/webcamMplayer.cs

RESOURCES = \
	../glade/app1.glade \
	../glade/confirm_window.glade \
	../glade/convert_weight.glade \
	../glade/dialog_about.glade \
	../glade/dialog_calendar.glade \
	../glade/dialog_image_test.glade \
	../glade/dialog_message.glade \
	../glade/dialog_person_popup.glade \
	../glade/dialog_server_stats.glade \
	../glade/dialog_shortcuts.glade \
	../glade/dialog_threshold.glade \
	../glade/edit_event.glade \
	../glade/encoder_configuration.glade \
	../glade/error_window.glade \
	../glade/evaluator_window.glade \
	../glade/event_graph_configure.glade \
	../glade/execute_auto.glade \
	../glade/generic_window.glade \
	../glade/force_sensor_elastic_bands.glade \
	../glade/force_sensor_exercise.glade \
	../glade/jump_type_add.glade \
	../glade/jumps_runs_more.glade \
	../glade/overview.glade \
	../glade/person_multiple_infinite.glade \
	../glade/person_recuperate.glade \
	../glade/person_select_window.glade \
	../glade/person_show_all_events.glade \
	../glade/person_win.glade \
	../glade/preferences_win.glade \
	../glade/query_server_window.glade \
	../glade/repair_sub_event.glade \
	../glade/repetitive_conditions.glade \
	../glade/report_window.glade \
	../glade/run_type_add.glade \
	../glade/session_upload.glade \
	../glade/splash_window.glade \
	../glade/stats_select_sessions.glade \
	../images/mini/no_image.png,mini/no_image.png \
	../images/sl-jumps.png,selector-jumps.png \
	../images/sl-runs.png,selector-runs.png \
	../images/sl-encoder.png,selector-encoder.png \
	../images/sl-force.png,selector-force.png \
	../images/sl-rt.png,selector-rt.png \
	../images/sl-multichronopic.png,selector-multichronopic.png \
	../images/sl-jumps-small.png,selector-jumps-small.png \
	../images/sl-runs-small.png,selector-runs-small.png \
	../images/sl-encoder-small.png,selector-encoder-small.png \
	../images/sl-force-small.png,selector-force-small.png \
	../images/sl-rt-small.png,selector-rt-small.png \
	../images/sl-multichronopic-small.png,selector-multichronopic-small.png \
	../images/mini/force_sensor_elastic.png,mini/force_sensor_elastic.png \
	../images/mini/force_sensor_not_elastic.png,mini/force_sensor_not_elastic.png \
	../images/mini/run-encoder-manual.png,mini/run-encoder-manual.png \
	../images/mini/run-encoder-resisted.png,mini/run-encoder-resisted.png \
	../images/b-unknown.png,board-unknown.png \
	../images/b-jump-run.png,board-jump-run.png \
	../images/b-encoder.png,board-encoder.png \
	../images/b-arduino-rfid.png,board-arduino-rfid.png \
	../images/b-arduino-force.png,board-arduino-force.png \
	../images/b-arduino-run-encoder.png,board-arduino-run-encoder.png \
	../images/b-accelerometer.png,board-accelerometer.png \
	../images/b-run-wireless.png,board-run-wireless.png \
	../images/chronojump-jumps-small.png,chronojump-jumps-small.png \
	../images/chronojump-runs-small.png,chronojump-runs-small.png \
	../images/chronojump-encoder-small.png,chronojump-encoder-small.png \
	../images/chronojump-inertial.png,chronojump-inertial.png \
	../images/ag_505.png,agility_505.png\
	../images/mini/ag_505.png,mini/agility_505.png \
	../images/ag_20yard.png,agility_20yard.png\
	../images/mini/ag_20yard.png,mini/agility_20yard.png \
	../images/ag_illinois.png,agility_illinois.png\
	../images/mini/ag_illinois.png,mini/agility_illinois.png \
	../images/ag_shuttle.png,agility_shuttle.png\
	../images/mini/ag_shuttle.png,mini/agility_shuttle.png \
	../images/ag_zigzag.png,agility_zigzag.png\
	../images/mini/ag_zigzag.png,mini/agility_zigzag.png \
	../images/ag_t_test.png,agility_t_test.png \
	../images/mini/ag_t_test.png,mini/agility_t_test.png \
	../images/ag_3l3r.png,agility_3l3r.png \
	../images/mini/ag_3l3r.png,mini/agility_3l3r.png \
	../images/jump_free.png,jump_free.png\
	../images/mini/jump_free.png,mini/jump_free.png \
	../images/jump_sj.png,jump_sj.png\
	../images/mini/jump_sj.png,mini/jump_sj.png \
	../images/jump_sj_l.png,jump_sj_l.png\
	../images/mini/jump_sj_l.png,mini/jump_sj_l.png \
	../images/jump_cmj.png,jump_cmj.png\
	../images/mini/jump_cmj.png,mini/jump_cmj.png \
	../images/jump_cmj_l.png,jump_cmj_l.png\
	../images/mini/jump_cmj_l.png,mini/jump_cmj_l.png \
	../images/jump_abk.png,jump_abk.png\
	../images/mini/jump_abk.png,mini/jump_abk.png \
	../images/jump_abk_l.png,jump_abk_l.png\
	../images/mini/jump_abk_l.png,mini/jump_abk_l.png \
	../images/jump_max.png,jump_max.png\
	../images/mini/jump_max.png,mini/jump_max.png \
	../images/jump_dj.png,jump_dj.png\
	../images/mini/jump_dj.png,mini/jump_dj.png \
	../images/jump_dj_a.png,jump_dj_a.png\
	../images/mini/jump_dj_a.png,mini/jump_dj_a.png \
	../images/jump_dj_inside.png,jump_dj_inside.png\
	../images/mini/jump_dj_inside.png,mini/jump_dj_inside.png \
	../images/jump_dj_a_inside.png,jump_dj_a_inside.png\
	../images/mini/jump_dj_a_inside.png,mini/jump_dj_a_inside.png \
	../images/jump_rocket.png,jump_rocket.png \
	../images/mini/jump_rocket.png,mini/jump_rocket.png \
	../images/jump_rj.png,jump_rj.png \
	../images/mini/jump_rj.png,mini/jump_rj.png \
	../images/jump_rj_in.png,jump_rj_in.png \
	../images/mini/jump_rj_in.png,mini/jump_rj_in.png \
	../images/jump_rj_hexagon.png,jump_rj_hexagon.png \
	../images/mini/jump_rj_hexagon.png,mini/jump_rj_hexagon.png \
	../images/run_simple.png,run_simple.png \
	../images/mini/run_simple.png,mini/run_simple.png \
	../images/run_interval.png,run_interval.png \
	../images/mini/run_interval.png,mini/run_interval.png \
	../images/margaria.png,margaria.png\
	../images/mini/margaria.png,mini/margaria.png \
	../images/gesell_dbt.png,gesell_dbt.png \
	../images/mini/gesell_dbt.png,mini/gesell_dbt.png \
	../images/multiChronopic.png,multiChronopic.png \
	../images/mini/multiChronopic.png,mini/multiChronopic.png \
	../images/run_analysis.png,run_analysis.png \
	../images/mini/run_analysis.png,mini/run_analysis.png \
	../images/pulse_free.png,pulse_free.png \
	../images/mini/pulse_free.png,mini/pulse_free.png \
	../images/pulse_custom.png,pulse_custom.png \
	../images/mini/pulse_custom.png,mini/pulse_custom.png \
	../images/reaction_time.png,reaction_time.png \
	../images/mini/reaction_time.png,mini/reaction_time.png \
	../images/reaction_time_discriminative.png,reaction_time_discriminative.png \
	../images/mini/reaction_time_discriminative.png,mini/reaction_time_discriminative.png \
	../images/dj-from-out.png,dj-from-out.png \
	../images/dj-from-in.png,dj-from-in.png \
	../images/jumps-fv.png,jumps-fv.png \
	../images/mtgug.png,mtgug.png \
	../images/mini/mtgug.png,mini/mtgug.png \
	../images/take_off.png,take_off.png \
	../images/mini/take_off.png,mini/take_off.png \
	../images/auto-by-persons.png,auto-by-persons.png \
	../images/auto-by-tests.png,auto-by-tests.png \
	../images/auto-by-series.png,auto-by-series.png \
	../images/auto-person-skip.png,auto-person-skip.png \
	../images/auto-person-remove.png,auto-person-remove.png \
	../images/chronopic.png,chronopic.png \
	../images/chronopic1.jpg,chronopic1.jpg \
	../images/mini/chronopic1.jpg,mini/chronopic1.jpg \
	../images/chronopic2.jpg,chronopic2.jpg \
	../images/mini/chronopic2.jpg,mini/chronopic2.jpg \
	../images/chronopic3.jpg,chronopic3.jpg \
	../images/mini/chronopic3.jpg,mini/chronopic3.jpg \
	../images/modular_platform_with_chronopic.jpg,modular_platform_with_chronopic.jpg \
	../images/mini/modular_platform_with_chronopic.jpg,mini/modular_platform_with_chronopic.jpg \
	../images/plataforma_contactos.jpg,plataforma_contactos.jpg\
	../images/mini/plataforma_contactos.jpg,mini/plataforma_contactos.jpg \
	../images/infrared.jpg,infrared.jpg \
	../images/mini/infrared.jpg,mini/infrared.jpg \
	../images/md/ic_notifications_blue_1x.png,stock_bell.png \
	../images/md/ic_notifications_active_blue_1x.png,stock_bell_active.png \
	../images/md/ic_notifications_none_blue_1x.png,stock_bell_none.png \
	../images/stock_bell_green.png,stock_bell_green.png \
	../images/stock_bell_red.png,stock_bell_red.png \
	../images/line_session_max.png,line_session_max.png \
	../images/line_session_avg.png,line_session_avg.png \
	../images/line_person_max.png,line_person_max.png \
	../images/line_person_max_all_sessions.png,line_person_max_all_sessions.png \
	../images/line_person_avg.png,line_person_avg.png \
	../images/audio-volume-high.png,audio-volume-high.png \
	../images/audio-volume-muted.png,audio-volume-muted.png \
	../images/chronopic_24.png,chronopic_24.png \
	../images/chronopic_128.png,chronopic_128.png \
	../images/gpm-statistics.png,gpm-statistics.png \
	../images/jumps-profile-pie.png,jumps-profile-pie.png \
	../images/spreadsheet.png,spreadsheet.png \
	../images/report_view.png,report_view.png \
	../images/md/attachment_blue.png,image_attachment.png \
	../images/md/grid_on_blue.png,image_grid_on.png \
	../images/md/photo_camera_start_blue.png,image_photo_start_camera.png \
	../images/md/photo_camera_end_blue.png,image_photo_end_camera.png \
	../images/md/photo_camera_do_red.png,image_photo_do.png \
	../images/md/photo_camera_preview_blue.png,image_photo_preview.png \
	../images/md/ic_audio_blue_1x.png,audio.png \
	../images/md/ic_videocam_blue_1x.png,videocamera_on.png \
	../images/md/ic_videocam_off_blue_1x.png,videocamera_off.png \
	../images/md/play_arrow_blue.png,video_play.png \
	../images/md/ic_home_blue_1x.png,image_home.png \
	../images/md/ic_import_1x.png,import.png \
	../images/md/ic_import_yellow_1x.png,import_yellow.png \
	../images/md/ic_export_1x.png,export.png \
	../images/md/ic_settings_blue_1x.png,image_settings.png \
	../images/md/ic_close_blue_1x.png,image_close_blue.png \
	../images/md/ic_close_red_1x.png,image_close.png \
	../images/md/ic_close_blue_2x.png,image_close_big.png \
	../images/md/ic_power_settings_new_red_1x.png,image_quit.png \
	../images/md/weight.png,image_weight.png \
	../images/md/weight_yellow.png,image_weight_yellow.png \
	../images/md/ic_settings_backup_restore_blue_1x.png,image_inertia.png \
	../images/md/ic_settings_backup_restore_yellow_1x.png,image_inertia_yellow.png \
	../images/md/ic_timer_blue_1x.png,image_capture.png \
	../images/md/ic_timer_blue_2x.png,image_capture_big.png \
	../images/md/ic_last_page_blue_1x.png,finish.png \
	../images/gtk-floppy.png,floppy.png \
	../images/md/arrow_back_blue.png,arrow_back.png \
	../images/md/ic_cancel_red_1x.png,image_cancel.png \
	../images/md/done_blue.png,image_done_blue.png \
	../images/md/ic_insert_chart_blue_1x.png,image_analyze.png \
	../images/md/ic_assignment_blue_1x.png,image_analyze_general.png \
	../images/md/ic_fast_forward_blue_1x.png,image_sprint.png \
	../images/md/ic_developer_board_blue_1x.png,image_chronopic_connect.png \
	../images/md/ic_developer_board_blue_2x.png,image_chronopic_connect_big.png \
	../images/md/p_blue_1x.png,image_person.png \
	../images/md/p_yellow_1x.png,image_person_yellow.png \
	../images/md/p_logout_blue_1x.png,image_person_logout.png \
	../images/md/persons_manage.png,persons_manage.png \
	../images/md/ic_photo_camera_blue_2x.png,image_no_photo.png \
	../images/md/ic_photo_camera_yellow_2x.png,image_no_photo_yellow.png \
	../images/md/ic_select_blue_3x.png,image_selected.png \
	../images/md/ic_cached_blue_1x.png,image_recalculate.png \
	../images/md/ic_delete_blue_1x.png,stock_delete.png \
	../images/md/ic_create_new_folder_blue_2x.png,folder_new_big.png \
	../images/md/ic_create_new_folder_blue_1x.png,folder_new.png \
	../images/md/ic_create_new_folder_yellow_1x.png,folder_new_yellow.png \
	../images/md/ic_fullscreen_exit_blue_1x.png,image_minimize.png \
	../images/md/ic_folder_open_blue_2x.png,folder_open_big.png \
	../images/md/ic_folder_open_blue_1x.png,folder_open.png \
	../images/md/ic_folder_open_yellow_1x.png,folder_open_yellow.png \
	../images/md/ic_folder_open_set_blue_1x.png,folder_open_set.png \
	../images/md/ic_visibility_blue_1x.png,image_visibility.png \
	../images/md/p_pin_blue_1x.png,image_person_pin.png \
	../images/md/ic_face_blue_1x.png,image_face.png \
	../images/md/p_add_blue_1x.png,image_person_add.png \
	../images/md/ic_group_add_blue_1x.png,image_group_add.png \
	../images/md/p_outline_blue_1x.png,image_person_outline.png \
	../images/md/image_group_outline.png,image_group_outline.png \
	../images/md/ic_weekend_blue_1x.png,image_rest.png \
	../images/md/ic_weekend_blue_inactive_1x.png,image_rest_inactive.png \
	../images/md/ic_weekend_yellow_1x.png,image_rest_yellow.png \
	../images/md/ic_looks_zero_blue_1x.png,zero.png \
	../images/md/ic_looks_one_blue_1x.png,one.png \
	../images/md/ic_add_circle_blue_1x.png,image_add_test.png \
	../images/md/ic_all_inclusive_blue_1x.png,cont.png \
	../images/md/ic_jump_blue_1x.png,image_jump.png \
	../images/md/ic_jump_simple_blue_1x.png,image_jump_simple.png \
	../images/md/ic_jump_reactive_blue_1x.png,image_jump_reactive.png \
	../images/md/run_blue_2x.png,run_2x.png \
	../images/md/run_blue_1x.png,image_run.png \
	../images/md/run_simple_blue_1x.png,image_run_simple.png \
	../images/md/run_simple_yellow_1x.png,image_run_simple_yellow.png \
	../images/md/run_multiple_blue_1x.png,image_run_multiple.png \
	../images/md/run_multiple_yellow_1x.png,image_run_multiple_yellow.png \
	../images/md/run_photocell_blue_2x.png,run_photocell.png \
	../images/md/run_track_blue_1x.png,run_track_distance.png \
	../images/md/ic_build_blue_1x.png,image_build_24.png \
	../images/md/ic_build_blue_16dp_1x.png,image_build_16.png \
	../images/md/ic_edit_blue_1x.png,image_edit.png \
	../images/md/ic_add_blue_1x.png,image_add.png \
	../images/md/ic_info_outline_blue_1x.png,image_info.png \
	../images/md/ic_fitness_center_blue_1x.png,image_exercise.png \
	../images/md/ic_keyboard_arrow_up_blue_1x.png,image_up.png \
	../images/md/ic_keyboard_arrow_down_blue_1x.png,image_down.png \
	../images/md/arrow_forward_blue.png,arrow_forward.png \
	../images/md/arrow_backward_blue.png,arrow_backward.png \
	../images/md/arrow_forward_red_yellow.png,arrow_forward_emphasis.png \
	../images/md/inertial_rolled.png,inertial_rolled.png \
	../images/md/inertial_half_rolled.png,inertial_half_rolled.png \
	../images/md/inertial_extended.png,inertial_extended.png \
	../images/md/calibrate.png,calibrate.png \
	../images/md/ic_folder_check_blue_1x.png,folder_check.png \
	../images/md/ic_test_inspect_blue_1x.png,image_test_inspect.png \
	../images/md/zoom_in.png,zoom_in.png \
	../images/md/zoom_out.png,zoom_out.png \
	../images/md/portrait_search_blue.png,portrait_zoom.png \
	../images/md/trigger_2x.png,image_encoder_triggers.png \
	../images/md/trigger_no_2x.png,image_encoder_triggers_no.png \
	../images/md/warning_blue.png,image_warning_blue.png \
	../images/md/warning_red.png,image_warning_red.png \
	../images/md/list_alt_blue.png,image_list.png \
	../images/md/mail_outline_blue.png,image_email.png \
	../images/md/send_blue.png,send_blue.png \
	../images/md/check_box_blue.png,image_check.png \
	../images/md/save_blue.png,save.png \
	../images/md/new_releases_blue.png,new.png \
	../images/md/menu_blue.png,image_menu.png \
	../images/md/apps_6_sep_blue.png,image_modes.png \
	../images/md/apps_6_sep_jump_blue.png,image_modes_jump.png \
	../images/md/apps_6_sep_run_blue.png,image_modes_run.png \
	../images/md/apps_6_sep_force_blue.png,image_modes_force.png \
	../images/md/apps_6_sep_encoder_blue.png,image_modes_encoder.png \
	../images/md/apps_6_sep_rt_blue.png,image_modes_rt.png \
	../images/md/apps_6_sep_other_blue.png,image_modes_other.png \
	../images/md/menu_book_blue.png,image_book.png \
	../images/md/keyboard_blue.png,image_keyboard.png \
	../images/md/more_horiz_blue.png,image_more_horiz.png \
	../images/md/about.png,image_about.png \
	../images/md/run_time.png,image_run_time.png \
	../images/md/folders_blue.png,image_folders.png \
	../images/md/folders_manage_blue.png,folders_manage_blue.png \
	../images/md/folders_yellow.png,image_folders_yellow.png \
	../images/md/folders_backup_blue.png,image_db_backup.png \
	../images/md/help_blue.png,image_help.png \
	../images/md/store_blue.png,image_store_blue.png \
	../images/md/store_yellow.png,image_store_yellow.png \
	../images/md/store_blue_news.png,image_store_has_new_products.png \
	../images/chronojump_icon.png,chronojump_icon.png \
	../images/chronojump_icon_transp.png,chronojump_icon_transp.png \
	../images/chronojump_kangaroo_icon_transp.png,chronojump_kangaroo_icon_transp.png \
	../images/chronojump_icon_graph.png,chronojump_icon_graph.png \
	../images/stock_right.png,stock_right.png \
	../images/stock_right_left.png,stock_right_left.png \
	../images/stock_up.png,stock_up.png \
	../images/stock_up_down.png,stock_up_down.png \
	../images/stock_down.png,stock_down.png \
	../images/stock_inertial.png,stock_inertial.png \
	../images/reaction_time_menu.png,reaction_time_menu.png \
	../images/pulse_menu.png,pulse_menu.png \
	../images/multichronopic_menu.png,multichronopic_menu.png \
	../images/bar_relative.png,bar_relative.png \
	../images/bar_absolute.png,bar_absolute.png \
	../images/force_sensor_menu.png,force_sensor_menu.png \
	../images/force_sensor_icon.png,force_sensor_icon.png \
	../images/force_sensor_icon_yellow.png,force_sensor_icon_yellow.png \
	../images/reaction_time_icon.png,reaction_time_icon.png \
	../images/reaction_time_icon_yellow.png,reaction_time_icon_yellow.png \
	../images/other_icon.png,other_icon.png \
	../images/other_icon_yellow.png,other_icon_yellow.png \
	../images/md/race_encoder_blue_1x.png,race_encoder_icon.png \
	../images/md/race_encoder_yellow_1x.png,race_encoder_icon_yellow.png \
	../images/gtk-apply.png,gtk-apply.png \
	../images/gtk-cancel.png,gtk-cancel.png \
	../images/gtk-zoom-in.png,gtk-zoom-in.png \
	../images/gtk-zoom-in-with-text.png,gtk-zoom-in-with-text.png \
	../images/gtk-new-1.png,gtk-new-1.png \
	../images/gtk-new-plus.png,gtk-new-plus.png \
	../images/gtk-open.png,gtk-open.png \
	../images/gtk-open-1.png,gtk-open-1.png \
	../images/gtk-open-plus.png,gtk-open-plus.png \
	../images/import-csv-noheaders.png,import-csv-noheaders.png \
	../images/import-csv-headers.png,import-csv-headers.png \
	../images/import-csv-name-1-column.png,import-csv-name-1-column.png \
	../images/import-csv-name-2-columns.png,import-csv-name-2-columns.png \
	../images/md/first.png,first.png \
	../images/md/last.png,last.png \
	../images/md/left.png,left.png \
	../images/md/left_cut.png,left_cut.png \
	../images/md/right.png,right.png \
	../images/md/right_cut.png,right_cut.png \
	../images/md/link.png,link.png \
	../images/md/link_off.png,link_off.png \
	../images/calendar.png,calendar.png \
	../images/chronojump-logo-transparent.png,chronojump-logo-transparent.png \
	../images/chronojump-logo-transparent-40h.png,chronojump-logo-transparent-40h.png \
	../images/chronojump-logo-2013_320.png,chronojump-logo-2013_320.png \
	../images/chronojump-logo-2013.png,chronojump-logo-2013.png \
	../images/muscle-concentric.png,muscle-concentric.png \
	../images/extra-mass.png,extra-mass.png \
	../images/muscle-excentric.png,muscle-excentric.png \
	../images/muscle-excentric-concentric.png,muscle-excentric-concentric.png \
	../images/laterality-both.png,laterality-both.png \
	../images/laterality-right.png,laterality-right.png \
	../images/laterality-left.png,laterality-left.png \
	../images/ea-individual-current-set.png,encoder-analyze-individual-current-set.png \
	../images/ea-individual-current-session.png,encoder-analyze-individual-current-session.png \
	../images/ea-individual-all-sessions.png,encoder-analyze-individual-all-sessions.png \
	../images/ea-groupal-current-session.png,encoder-analyze-groupal-current-session.png \
	../images/ea-powerbars.png,encoder-analyze-powerbars.png \
	../images/ea-cross.png,encoder-analyze-cross.png \
	../images/ea-1RM.png,encoder-analyze-1RM.png \
	../images/ea-instantaneous.png,encoder-analyze-instantaneous.png \
	../images/ea-single.png,encoder-analyze-single.png \
	../images/ea-side.png,encoder-analyze-side.png \
	../images/ea-superpose.png,encoder-analyze-superpose.png \
	../images/ea-all-set.png,encoder-analyze-all-set.png \
	../images/ea-nmp.png,encoder-analyze-nmp.png \
	../images/ea-eccon-together.png,encoder-analyze-eccon-together.png \
	../images/ea-eccon-separated.png,encoder-analyze-eccon-separated.png \
	../images/ea-position.png,encoder-analyze-position.png \
	../images/ea-speed.png,encoder-analyze-speed.png \
	../images/ea-accel.png,encoder-analyze-accel.png \
	../images/ea-force.png,encoder-analyze-force.png \
	../images/ea-power.png,encoder-analyze-power.png \
	../images/ea-mean.png,encoder-analyze-mean.png \
	../images/ea-max.png,encoder-analyze-max.png \
	../images/ea-range.png,encoder-analyze-range.png \
	../images/ea-time-to-pp.png,encoder-analyze-time-to-pp.png \
	../images/encoder-image-pending.png,encoder-image-pending.png \
	../images/encoder-linear.png,encoder-linear.png \
	../images/equivalentMass.png,equivalentMass.png \
	../images/er-fr.png,encoder-rotary-friction.png \
	../images/er-axis.png,encoder-rotary-axis.png \
	../images/el-blue.png,encoder-linear-blue.png \
	../images/er-fr-blue.png,encoder-rotary-friction-blue.png \
	../images/er-axis-blue.png,encoder-rotary-axis-blue.png \
	../images/el-free-weight.png,encoder-linear-free-weight.png \
	../images/el-free-weight-inv.png,encoder-linear-free-weight-inv.png \
	../images/el-inertial.png,encoder-linear-inertial.png \
	../images/el-on-person-weighted-moving-pulley1.png,encoder-linear-on-person-weighted-moving-pulley1.png \
	../images/el-inv-on-person-weighted-moving-pulley1.png,encoder-linear-inv-on-person-weighted-moving-pulley1.png \
	../images/el-on-person-weighted-moving-pulley2.png,encoder-linear-on-person-weighted-moving-pulley2.png \
	../images/el-inv-on-person-weighted-moving-pulley2.png,encoder-linear-inv-on-person-weighted-moving-pulley2.png \
	../images/el-on-weighted-moving-pulley.png,encoder-linear-on-weighted-moving-pulley.png \
	../images/el-inclined-plane.png,encoder-linear-inclined-plane.png \
	../images/el-inclined-plane-weight-diff-angle.png,encoder-linear-inclined-plane-weight-diff-angle.png \
	../images/el-inclined-plane-weight-diff-angle-mov-pulley.png,encoder-linear-inclined-plane-weight-diff-angle-mov-pulley.png \
	../images/el-pneumatic.png,encoder-linear-pneumatic.png \
	../images/er-fr-pulley.png,encoder-rotary-friction-pulley.png \
	../images/er-fr-pulley-axis.png,encoder-rotary-friction-pulley-axis.png \
	../images/er-fr-side-inertial.png,encoder-rotary-friction-side-inertial.png \
	../images/er-fr-axis-inertial.png,encoder-rotary-friction-axis-inertial.png \
	../images/er-fr-side-inertial-lateral.png,encoder-rotary-friction-side-inertial-lateral.png \
	../images/er-fr-axis-inertial-lateral.png,encoder-rotary-friction-axis-inertial-lateral.png \
	../images/er-fr-side-inertial-mov-pulley.png,encoder-rotary-friction-side-inertial-mov-pulley.png \
	../images/er-fr-axis-inertial-mov-pulley.png,encoder-rotary-friction-axis-inertial-mov-pulley.png \
	../images/er-fr-on-fixed-pulley-with-weighted-moving-pulley.png,encoder-rotary-friction-on-fixed-pulley-with-weighted-moving-pulley.png \
	../images/er-axis-pulley-axis.png,encoder-rotary-axis-pulley-axis.png \
	../images/er-axis-inertial.png,encoder-rotary-axis-inertial.png \
	../images/er-axis-inertial-lateral.png,encoder-rotary-axis-inertial-lateral.png \
	../images/er-axis-inertial-mov-pulley.png,encoder-rotary-axis-inertial-mov-pulley.png \
	../images/er-axis-inertial-mov-pulley-lateral.png,encoder-rotary-axis-inertial-mov-pulley-lateral.png \
	../images/encoder-calcule-im.png,encoder-calcule-im.png \
	../images/er-axis-on-fixed-pulley-with-weighted-moving-pulley.png,encoder-rotary-axis-on-fixed-pulley-with-weighted-moving-pulley.png \
	../images/inertial-start.png,inertial-start.png \
	../images/start.wav,sound_can_start.wav \
	../images/ok.wav,ok.wav \
	../images/bad.wav,sound_bad.wav

IMAGES = $(top_srcdir)/images/chronojump-logo-2013.png \
         $(top_srcdir)/images/chronojump-logo-2013_320.png \
         $(top_srcdir)/images/start.wav \
         $(top_srcdir)/images/ok.wav \
         $(top_srcdir)/images/bad.wav \
         $(top_srcdir)/images/report_web_style.css

LOGO = $(top_srcdir)/images/svg/chronojump_icon.svg

bin_SCRIPTS = chronojump

DESKTOP_FILE = chronojump.desktop.in

include $(top_srcdir)/build/build.mk

EXTRA_DIST += \
	chronojump.in \
	AssemblyInfo.cs.in \
	../images/report_web_style.css \
	$(top_srcdir)/src/angle/*

